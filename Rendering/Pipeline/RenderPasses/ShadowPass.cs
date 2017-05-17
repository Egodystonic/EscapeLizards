// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 22 04 2015 at 14:17 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Rendering {
	public sealed class ShadowPass : RenderPass {
		private Camera lightCam;
		private SceneViewport output;
		private VertexShader shadowVS;
		private FragmentShader shadowFS;
		private ShaderResourcePackage vsResPackage;
		private ShaderResourcePackage fsResPackage;
		private ShaderResourcePackage vsUnbindResPackage;
		private ShaderResourcePackage fsUnbindResPackage;
		private readonly RasterizerState shadowRasterizer = new RasterizerState(false, TriangleCullMode.BackfaceCulling, false);
		private readonly DepthStencilState shadowDSState = new DepthStencilState(true);
		private readonly BlendState shadowBlendState = new BlendState(BlendOperation.None);
		private readonly List<SceneLayer> addedSceneLayers = new List<SceneLayer>();

		private const int INITIAL_TRANSFORM_BUF_LEN = 1;
		private static readonly VertexBufferBuilder<Matrix> gpuInstanceBufferBuilder = BufferFactory.NewVertexBuffer<Matrix>().WithUsage(ResourceUsage.DiscardWrite);
		private GeometryCache currentCache;
		private ArraySlice<KeyValuePair<Material, ModelInstanceManager.MIDArray>> currentInstanceData;
		private SceneLayer[] currentSceneLayers = new SceneLayer[0];
		private static byte frameNum;
		[ThreadStatic]
		private static FastClearList<Transform>[] materialFilteringWorkspace;
		[ThreadStatic]
		private static Matrix[] instanceConcatWorkspace;
		[ThreadStatic]
		private static FragmentShader lastSetFragmentShader;
		[ThreadStatic]
		private static byte lastFrameNum;
		[ThreadStatic]
		private static uint reservedSetIBCommandSlot;
		private Matrix[] cpuInstanceBuffer = new Matrix[INITIAL_TRANSFORM_BUF_LEN];
		private VertexBuffer<Matrix> gpuInstanceBuffer;
		private uint cpuInstanceBufferCurIndex;

		private readonly Action setUpCacheForLocalThreadAct;
		private readonly Action<int> renderCacheIterateMatAct;
		private readonly Action setInstanceBufferAndFlushCommandsAct;

		public ShaderResourceView ShadowBufferSRV {
			get {
				lock (InstanceMutationLock) {
					return shadowBufferSRV;
				}
			}
		}

		public Camera LightCam {
			get {
				lock (InstanceMutationLock) {
					return lightCam;
				}
			}
			set {
				lock (InstanceMutationLock) {
					lightCam = value;
				}
			}
		}

		public SceneViewport Output {
			get {
				lock (InstanceMutationLock) {
					return output;
				}
			}
			set {
				lock (InstanceMutationLock) {
					output = value;
				}
			}
		}

		public VertexShader ShadowVS {
			get {
				lock (InstanceMutationLock) {
					return shadowVS;
				}
			}
			set {
				using (RenderingModule.RenderStateBarrier.AcquirePermit(withLock: InstanceMutationLock)) {
					shadowVS = value;
					vsResPackage = new ShaderResourcePackage();
					vsUnbindResPackage = new ShaderResourcePackage();
				}
			}
		}

		public FragmentShader ShadowFS {
			get {
				lock (InstanceMutationLock) {
					return shadowFS;
				}
			}
			set {
				using (RenderingModule.RenderStateBarrier.AcquirePermit(withLock: InstanceMutationLock)) {
					shadowFS = value;
					fsResPackage = new ShaderResourcePackage();
					fsUnbindResPackage = new ShaderResourcePackage();
				}
			}
		}

		public override bool IsValid {
			get {
				return !IsDisposed && lightCam != null && !lightCam.IsDisposed;
			}
		}

		public ShadowPass(string name) : base(name) {
			setUpCacheForLocalThreadAct = SetUpCacheForLocalThread;
			renderCacheIterateMatAct = RenderCache_IterateMaterial;
			setInstanceBufferAndFlushCommandsAct = SetInstanceBufferAndFlushCommands;
		}

		private readonly Texture2DBuilder<TexelFormat.R24G8Typeless> shadowBufferBuilder = TextureFactory.NewTexture2D<TexelFormat.R24G8Typeless>()
			.WithMipAllocation(false)
			.WithMipGenerationTarget(false)
			.WithPermittedBindings(GPUBindings.DepthStencilTarget | GPUBindings.ReadableShaderResource)
			.WithUsage(ResourceUsage.Write);
		private Texture2D<TexelFormat.R24G8Typeless> shadowBuffer;
		private DepthStencilView shadowBufferDSV;
		private ShaderResourceView shadowBufferSRV;

		public override void Dispose() {
			lock (InstanceMutationLock) {
				if (IsDisposed) return;
				if (shadowBuffer != null && !shadowBuffer.IsDisposed) shadowBuffer.Dispose();
				base.Dispose();
			}
		}



		protected internal unsafe override void Execute(ParallelizationProvider pp) {
			// See if we need to resize the depth buffer
			Vector2 viewportDimensions = output.SizePixels;
			if (viewportDimensions.X < 1f || viewportDimensions.Y < 1f) return;
			uint viewportX = (uint) viewportDimensions.X;
			uint viewportY = (uint) viewportDimensions.Y;
			if (shadowBuffer == null || shadowBufferDSV == null || shadowBufferDSV.ResourceOrViewDisposed 
				|| shadowBufferSRV == null || shadowBufferSRV.ResourceOrViewDisposed
				|| shadowBuffer.Width != viewportX || shadowBuffer.Height != viewportY) {
				if (shadowBufferDSV != null && !shadowBufferDSV.IsDisposed) shadowBufferDSV.Dispose();
				if (shadowBuffer != null && !shadowBuffer.IsDisposed) shadowBuffer.Dispose();
				shadowBuffer = shadowBufferBuilder.WithWidth(viewportX).WithHeight(viewportY);
				shadowBufferDSV = shadowBuffer.CreateDepthStencilView<TexelFormat.DepthStencil>(0U);
				shadowBufferSRV = shadowBuffer.CreateView<TexelFormat.R24UnormX8Typeless>(0U, 1U);
			}

			// Clear the depth buffer
			QueueRenderCommand(RenderCommand.ClearDepthStencil(shadowBufferDSV));

			List<GeometryCache> activeCaches = GeometryCache.ActiveCaches;
			foreach (GeometryCache c in activeCaches) {
				// Set view/proj matrix
				Matrix vpMat = (*((Matrix*) lightCam.GetRecalculatedViewMatrix()) * *((Matrix*) Output.GetRecalculatedProjectionMatrix(lightCam))).Transpose;
				byte* vpMapPtr = (byte*) &vpMat;
				shadowVS.ViewProjMatBinding.SetValue(vpMapPtr);

				// Set state for current cache
				cpuInstanceBufferCurIndex = 0;
				List<SceneLayer> allEnabledLayers = Scene.EnabledLayers;
				uint maxLayer = 0U;
				for (int i = 0; i < allEnabledLayers.Count; ++i) {
					if (allEnabledLayers[i].Index > maxLayer) {
						maxLayer = allEnabledLayers[i].Index;
					}
				}
				if (allEnabledLayers.Count > 0 && currentSceneLayers.Length <= maxLayer) {
					currentSceneLayers = new SceneLayer[maxLayer + 1U];
				}
				Array.Clear(currentSceneLayers, 0, currentSceneLayers.Length);
				foreach (SceneLayer layer in allEnabledLayers) currentSceneLayers[layer.Index] = layer;
				currentCache = c;
				++frameNum;
				Thread.MemoryBarrier();
				currentInstanceData = currentCache.GetModelInstanceData();

				// Set up each thread
				pp.InvokeOnAll(setUpCacheForLocalThreadAct, true); // membar here

				// Iterate all model instances (ordered by material)
				pp.Execute((int) currentInstanceData.Length, (int) (currentInstanceData.Length / (pp.NumThreads << 3)) + 1, renderCacheIterateMatAct);

				// Set instance buffer and write to it
				if (gpuInstanceBuffer == null || gpuInstanceBuffer.Length < cpuInstanceBuffer.Length) {
					if (gpuInstanceBuffer != null) gpuInstanceBuffer.Dispose();
					gpuInstanceBuffer = gpuInstanceBufferBuilder.WithLength((uint) cpuInstanceBuffer.Length).Create();
				}
				gpuInstanceBuffer.DiscardWrite(cpuInstanceBuffer); // Happens immediately (required)

				// Set instance buffer and flush all commands, first on immediate context, then on each deferred
				SetInstanceBufferAndFlushCommands();
				pp.InvokeOnAll(setInstanceBufferAndFlushCommandsAct, false);
			}
		}

		private void SetUpCacheForLocalThread() {
			// Set topology
			QueueRenderCommand(RenderCommand.SetPrimitiveTopology(RenderCommand.DEFAULT_PRIMITIVE_TOPOLOGY));

			// Set input layout
			GeometryInputLayout shaderInputLayout = currentCache.GetInputLayout(shadowVS);
			QueueRenderCommand(RenderCommand.SetInputLayout(shaderInputLayout));

			// Enqueue VS commands
			QueueRenderCommand(RenderCommand.SetIndexBuffer(currentCache.IndexBuffer));
			QueueShaderSwitch(shadowVS);
			QueueShaderResourceUpdate(shadowVS, shaderInputLayout.ResourcePackage);

			// Enqueue FS commands
			QueueShaderSwitch(shadowFS);

			// Set rasterizer state
			QueueRenderCommand(RenderCommand.SetRasterizerState(shadowRasterizer));
			QueueRenderCommand(RenderCommand.SetViewport(Output));

			// Set depth stencil state
			QueueRenderCommand(RenderCommand.SetDepthStencilState(shadowDSState));

			// Set blend state
			QueueRenderCommand(RenderCommand.SetBlendState(shadowBlendState));

			// Set up output merger
			QueueRenderCommand(RenderCommand.SetRenderTargets(shadowBufferDSV));

			// Reserve a space for setting the instance buffer
			reservedSetIBCommandSlot = ReserveCommandSlot();
		}

		private unsafe void RenderCache_IterateMaterial(int materialIndex) {
			// Set up context variables
			KeyValuePair<Material, ModelInstanceManager.MIDArray> currentKVP = currentInstanceData[materialIndex];
			ModelInstanceManager.MIDArray currentMID = currentKVP.Value;

			// Skip this material if it or its shader are disposed
			if (currentKVP.Key.IsDisposed) return;

			// Filter & sort
			if (materialFilteringWorkspace == null || materialFilteringWorkspace.Length < currentCache.NumModels) {
				materialFilteringWorkspace = new FastClearList<Transform>[currentCache.NumModels];
				for (int i = 0; i < materialFilteringWorkspace.Length; ++i) materialFilteringWorkspace[i] = new FastClearList<Transform>();
			}
			for (int i = 0; i < materialFilteringWorkspace.Length; ++i) materialFilteringWorkspace[i].Clear();

			ModelInstanceData* midData = currentMID.Data;
			uint numInstances = 0U;
			for (uint i = 0U; i < currentMID.Length; ++i) {
				ModelInstanceData curMID = midData[i];
				if (!curMID.InUse) continue;
				SceneLayer layer = currentSceneLayers[curMID.SceneLayerIndex];
				if (layer == null || !layer.GetRenderingEnabled() || !addedSceneLayers.Contains(layer)) continue;

				materialFilteringWorkspace[curMID.ModelIndex].Add(curMID.Transform);
				++numInstances;
			}

			// Concatenate & queue render commands
			if (instanceConcatWorkspace == null || instanceConcatWorkspace.Length < numInstances) {
				instanceConcatWorkspace = new Matrix[numInstances << 1]; // x2 so we don't create loads of garbage if the count keeps increasing by 1
			}

			uint instanceStartOffset = RenderCache_IterateMaterial_ConcatReserve(numInstances);
			uint nextWorkspaceIndex = 0;
			uint outVBStartIndex, outIBStartIndex, outVBCount, outIBCount;

			for (uint mI = 0U; mI < materialFilteringWorkspace.Length; ++mI) {
				FastClearList<Transform> filteredTransformList = materialFilteringWorkspace[mI];
				int numFilteredTransforms = filteredTransformList.Count;
				if (numFilteredTransforms == 0) continue;

				currentCache.GetModelBufferValues(mI, out outVBStartIndex, out outIBStartIndex, out outVBCount, out outIBCount);

				QueueRenderCommand(RenderCommand.DrawIndexedInstanced(
					(int) outVBStartIndex,
					outIBStartIndex,
					outIBCount,
					nextWorkspaceIndex + instanceStartOffset,
					(uint) numFilteredTransforms
				));

				for (int iI = 0; iI < numFilteredTransforms; ++iI) {
					instanceConcatWorkspace[nextWorkspaceIndex++] = filteredTransformList[iI].AsMatrixTransposed;
				}
			}

			RenderCache_IterateMaterial_Concat(instanceConcatWorkspace, instanceStartOffset, numInstances);
		}

		private uint RenderCache_IterateMaterial_ConcatReserve(uint numInstances) {
			lock (InstanceMutationLock) {
				cpuInstanceBufferCurIndex += numInstances;
				return cpuInstanceBufferCurIndex - numInstances;
			}
		}

		private void RenderCache_IterateMaterial_Concat(Matrix[] localInstanceData, uint offset, uint count) {
			lock (InstanceMutationLock) {
				if (offset + count > cpuInstanceBuffer.Length) {
					// x2 (e.g. '<< 1') below to avoid excessive garbage when the limit keeps increasing by small amounts
					Matrix[] newCPUInstanceBuffer = new Matrix[(offset + count) << 1];
					Array.Copy(cpuInstanceBuffer, 0, newCPUInstanceBuffer, 0, cpuInstanceBuffer.Length);
					cpuInstanceBuffer = newCPUInstanceBuffer;
				}

				Array.Copy(localInstanceData, 0, cpuInstanceBuffer, offset, count);
			}
		}

		private void SetInstanceBufferAndFlushCommands() {
			QueueRenderCommand(
				reservedSetIBCommandSlot,
				RenderCommand.SetInstanceBuffer(gpuInstanceBuffer, shadowVS.InstanceDataBinding.SlotIndex)
			);
			FlushRenderCommands();
		}

		public void AddLayer(SceneLayer layer) {
			Assure.NotNull(layer);
			using (RenderingModule.RenderStateBarrier.AcquirePermit(withLock: InstanceMutationLock)) {
				if (addedSceneLayers.Contains(layer)) throw new InvalidOperationException("Given layer has already been added.");
				addedSceneLayers.Add(layer);
			}
		}

		public void RemoveLayer(SceneLayer layer) {
			Assure.NotNull(layer);
			using (RenderingModule.RenderStateBarrier.AcquirePermit(withLock: InstanceMutationLock)) {
				if (!addedSceneLayers.Remove(layer)) throw new InvalidOperationException("Given layer is not already added.");
			}
		}

		public void ClearLayers() {
			using (RenderingModule.RenderStateBarrier.AcquirePermit(withLock: InstanceMutationLock)) {
				addedSceneLayers.Clear();
			}
		}
	}
}