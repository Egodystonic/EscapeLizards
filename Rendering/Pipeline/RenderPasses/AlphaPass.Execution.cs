// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 22 01 2015 at 15:43 by Ben Bowen

using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Rendering {
	public unsafe partial class AlphaPass {
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
		private RenderTargetViewHandle windowRTVH;

		private readonly Action setUpCacheForLocalThreadAct;
		private readonly Action<int> renderCacheIterateMatAct;
		private readonly Action setInstanceBufferAndFlushCommandsAct;

		/// <summary>
		/// Disposes managed resources and invalides the render pass permanently.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2215:Dispose methods should call base class dispose",
			Justification = "No need to call base class, all it does is set isDisposed.")]
		public override void Dispose() {
			lock (InstanceMutationLock) {
				if (IsDisposed) return;
				if (gpuInstanceBuffer != null) gpuInstanceBuffer.Dispose();
				base.Dispose();
			}
		}

		/// <summary>
		/// Called from the <see cref="LosgapSystem.MasterThread"/> when this render pass should execute.
		/// Before this method is called, mutations on the <see cref="RenderingModule.RenderStateBarrier"/> are frozen; and will remain frozen
		/// until this method returns.
		/// </summary>
		/// <param name="pp">The parallelization provider currently in use by the system, providing a way to utilise all cores of the system.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling",
			Justification = "Will monitor complexity of this method carefully, but it sits at 31 dependencies right now, 1 more than the recommended max."),
		System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0",
			Justification = "ParallelizationProvider will never be null- if it was, there's a fatal flaw in the engine somewhere.")]
		protected internal override void Execute(ParallelizationProvider pp) {
			DepthStencilViewHandle _;
			if (!output.TargetWindow.GetWindowRTVAndDSV(out windowRTVH, out _)) return;

			List<GeometryCache> activeCaches = GeometryCache.ActiveCaches;
			foreach (GeometryCache c in activeCaches) {
				// Set view/proj matrix
				Matrix vpMat = (*((Matrix*) Input.GetRecalculatedViewMatrix()) * *((Matrix*) Output.GetRecalculatedProjectionMatrix(Input))).Transpose;
				byte* vpMapPtr = (byte*) &vpMat;
				VertexShader.ViewProjMatBinding.SetValue(vpMapPtr);

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
				pp.InvokeOnAll(setUpCacheForLocalThreadAct, true); // also emits a membar

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

			// Present
			FlushRenderCommands();
			if (presentAfterPass) PresentBackBuffer(Output.TargetWindow);
		}

		private void SetUpCacheForLocalThread() {
			// Set topology
			QueueRenderCommand(RenderCommand.SetPrimitiveTopology(RenderCommand.DEFAULT_PRIMITIVE_TOPOLOGY));

			// Set input layout
			GeometryInputLayout shaderInputLayout = currentCache.GetInputLayout(vertexShader);
			QueueRenderCommand(RenderCommand.SetInputLayout(shaderInputLayout));

			// Enqueue VS commands
			QueueRenderCommand(RenderCommand.SetIndexBuffer(currentCache.IndexBuffer));
			QueueShaderSwitch(VertexShader);
			QueueShaderResourceUpdate(VertexShader, currentCache.GetInputLayout(VertexShader).ResourcePackage);

			// Set rasterizer state
			QueueRenderCommand(RenderCommand.SetRasterizerState(rsState));
			QueueRenderCommand(RenderCommand.SetViewport(Output));

			// Set depth stencil state
			QueueRenderCommand(RenderCommand.SetDepthStencilState(dsState));

			// Set blend state
			QueueRenderCommand(RenderCommand.SetBlendState(blendState));
			
			// Set up output merger
			QueueRenderCommand(RenderCommand.SetRenderTargets(mainGeomPass.PrimaryDSBufferDSV.ResourceViewHandle, windowRTVH));

			// Reserve a space for setting the instance buffer
			reservedSetIBCommandSlot = ReserveCommandSlot();
		}

		private void RenderCache_IterateMaterial(int materialIndex) {
			// Set up context variables
			KeyValuePair<Material, ModelInstanceManager.MIDArray> currentKVP = currentInstanceData[materialIndex];
			Material currentMaterial = currentKVP.Key;
			ModelInstanceManager.MIDArray currentMID = currentKVP.Value;

			// Skip this material if it or its shader are disposed
			if (currentMaterial.IsDisposed || currentMaterial.Shader.IsDisposed) return;

			// Prepare shader according to material params, and switch to it or update it
			if (lastSetFragmentShader != currentMaterial.Shader || lastFrameNum != frameNum) {
				lastSetFragmentShader = currentMaterial.Shader;
				lastFrameNum = frameNum;
				QueueShaderSwitch(lastSetFragmentShader);
			}
			QueueShaderResourceUpdate(lastSetFragmentShader, currentMaterial.FragmentShaderResourcePackage);


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
				RenderCommand.SetInstanceBuffer(gpuInstanceBuffer, vertexShader.InstanceDataBinding.SlotIndex)
			);
			FlushRenderCommands();
		}
	}
}