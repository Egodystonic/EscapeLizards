// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 22 01 2015 at 15:43 by Ben Bowen

using System;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Rendering {
	public unsafe partial class DLGeometryPass {
		[ThreadStatic]
		private static ModelInstanceData[] sortedModelData;
		[ThreadStatic]
		private static MIDComparer localMidComparer;

		private const int INITIAL_TRANSFORM_BUF_LEN = 1;
		private static readonly Texture2DBuilder<TexelFormat.RGBA32Float> gBufferBuilder =
			TextureFactory.NewTexture2D<TexelFormat.RGBA32Float>()
			.WithPermittedBindings(GPUBindings.ReadableShaderResource | GPUBindings.RenderTarget)
			.WithUsage(ResourceUsage.Write);

		private static readonly VertexBufferBuilder<Matrix> gpuInstanceBufferBuilder = BufferFactory.NewVertexBuffer<Matrix>().WithUsage(ResourceUsage.DiscardWrite);

		private GeometryCache currentCache;
		private VertexShader currentVS;
		private ArraySlice<KeyValuePair<Material, ModelInstanceManager.MIDArray>> currentInstanceData;
		private SceneLayer[] currentSceneLayers = new SceneLayer[0];
		private ShaderResourceView previousShadowBufferSRV;

		private static readonly Texture2DBuilder<TexelFormat.R24G8Typeless> dsBufferBuilder = TextureFactory.NewTexture2D<TexelFormat.R24G8Typeless>()
			.WithMipAllocation(false)
			.WithMipGenerationTarget(false)
			.WithPermittedBindings(GPUBindings.ReadableShaderResource | GPUBindings.DepthStencilTarget)
			.WithUsage(ResourceUsage.Write);
		private Texture2D<TexelFormat.R24G8Typeless> primaryDSBuffer;
		private DepthStencilView primaryDSBufferDSV;
		private ShaderResourceView primaryDSBufferSRV;

		public DepthStencilView PrimaryDSBufferDSV {
			get {
				lock (InstanceMutationLock) {
					return primaryDSBufferDSV;
				}
			}
		}

		public ShaderResourceView PrimaryDSBufferSRV {
			get {
				lock (InstanceMutationLock) {
					return primaryDSBufferSRV;
				}
			}
		}

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
		private FragmentShader geomFSWithShadowSupport;
		private ShaderResourcePackage geomFSShadowUnbindPackage;

		private readonly Action setUpCacheForLocalThreadAct;
		private readonly Action<int> renderCacheIterateMatAct;
		private readonly Action setInstanceBufferAndFlushCommandsAct;

		public FragmentShader GeomFSWithShadowSupport {
			set {
				lock (InstanceMutationLock) {
					geomFSWithShadowSupport = value;
					geomFSShadowUnbindPackage = new ShaderResourcePackage();
					foreach (var resourceViewBinding in geomFSWithShadowSupport.ResourceViewBindings) {
						geomFSShadowUnbindPackage.SetValue(resourceViewBinding, null);
					}
				}
			}
		}

		/// <summary>
		/// Disposes managed resources and invalides the render pass permanently.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2215:Dispose methods should call base class dispose",
			Justification = "No need to call base class, all it does is set isDisposed.")]
		public override void Dispose() {
			lock (InstanceMutationLock) {
				if (IsDisposed) return;
				if (gpuInstanceBuffer != null) gpuInstanceBuffer.Dispose();
				gBufferViews.ForEach(gbv => gbv.Dispose());
				gBuffer.ForEach(gb => gb.Dispose());
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
			for (int i = 0; i < NUM_GBUFFER_TEXTURES; ++i) {
				if (gBufferViews[i] != null) QueueRenderCommand(RenderCommand.ClearRenderTarget(gBufferViews[i]));
			}
			if (clearOutputBeforePass) {
				QueueRenderCommand(RenderCommand.ClearRenderTarget(output.TargetWindow));
				QueueRenderCommand(RenderCommand.ClearDepthStencil(output.TargetWindow));	
			}

			Vector2 outputSizePixels = output.SizePixels;
			if (gBuffer[0] == null || gBuffer[0].Width != (uint) outputSizePixels.X || gBuffer[0].Height != (uint) outputSizePixels.Y) {
				for (int i = 0; i < NUM_GBUFFER_TEXTURES; ++i) {
					if (gBufferViews[i] != null) gBufferViews[i].Dispose();
					if (gBuffer[i] != null) gBuffer[i].Dispose();
					gBuffer[i] = gBufferBuilder.WithWidth((uint) outputSizePixels.X).WithHeight((uint) outputSizePixels.Y);
					gBufferViews[i] = gBuffer[i].CreateRenderTargetView(0U);
				}
			}
			if (primaryDSBuffer == null || primaryDSBuffer.Width != outputSizePixels.X || primaryDSBuffer.Height != outputSizePixels.Y) {
				if (primaryDSBuffer != null) primaryDSBuffer.Dispose();
				if (primaryDSBufferDSV != null) primaryDSBufferDSV.Dispose();
				if (primaryDSBufferSRV != null) primaryDSBufferSRV.Dispose();

				primaryDSBuffer = dsBufferBuilder.WithWidth((uint) outputSizePixels.X).WithHeight((uint) outputSizePixels.Y);
				primaryDSBufferDSV = primaryDSBuffer.CreateDepthStencilView<TexelFormat.DepthStencil>(0U);
				primaryDSBufferSRV = primaryDSBuffer.CreateView<TexelFormat.R24UnormX8Typeless>(0U, 1U);
			}
			previousShadowBufferSRV = shadowPass.ShadowBufferSRV;

			// Clear main DSV
			QueueRenderCommand(RenderCommand.ClearDepthStencil(primaryDSBufferDSV));

			List<GeometryCache> activeCaches = GeometryCache.ActiveCaches;
			foreach (GeometryCache c in activeCaches) {
				if (!deferredGeometryVertexShaders.ContainsKey(c)) continue;
				currentVS = deferredGeometryVertexShaders[c];

				// Set view/proj matrices
				var vpMatrices = new GeomPassProjViewMatrices {
					MainCameraVPMat = (*((Matrix*)Input.GetRecalculatedViewMatrix()) * *((Matrix*)Output.GetRecalculatedProjectionMatrix(Input))).Transpose,
					ShadowCameraVPMat = (*((Matrix*)shadowPass.LightCam.GetRecalculatedViewMatrix()) * *((Matrix*)Output.GetRecalculatedProjectionMatrix(shadowPass.LightCam))).Transpose
				};
				byte* vpMatPtr = (byte*) &vpMatrices;
				currentVS.ViewProjMatBinding.SetValue(vpMatPtr);
				
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

				// Unbind shadow buffer
				QueueShaderSwitch(geomFSWithShadowSupport);
				QueueShaderResourceUpdate(geomFSWithShadowSupport, geomFSShadowUnbindPackage);

				// Set instance buffer and flush all commands, first on immediate context, then on each deferred
				SetInstanceBufferAndFlushCommands();
				pp.InvokeOnAll(setInstanceBufferAndFlushCommandsAct, false);
			}
		}

		private void SetUpCacheForLocalThread() {
			// Set topology
			QueueRenderCommand(RenderCommand.SetPrimitiveTopology(RenderCommand.DEFAULT_PRIMITIVE_TOPOLOGY));

			// Set input layout
			GeometryInputLayout shaderInputLayout = currentCache.GetInputLayout(currentVS);
			QueueRenderCommand(RenderCommand.SetInputLayout(shaderInputLayout));

			// Enqueue VS commands
			QueueRenderCommand(RenderCommand.SetIndexBuffer(currentCache.IndexBuffer));
			QueueShaderSwitch(currentVS);
			QueueShaderResourceUpdate(currentVS, shaderInputLayout.ResourcePackage);

			// Set rasterizer state
			QueueRenderCommand(RenderCommand.SetRasterizerState(rsState));
			QueueRenderCommand(RenderCommand.SetViewport(Output));

			// Set depth stencil state
			QueueRenderCommand(RenderCommand.SetDepthStencilState(dsState));

			// Set blend state
			QueueRenderCommand(RenderCommand.SetBlendState(blendState));

			// Set up output merger
			QueueRenderCommand(RenderCommand.SetRenderTargets(output.TargetWindow, primaryDSBufferDSV, gBufferViews));

			// Reserve a space for setting the instance buffer
			reservedSetIBCommandSlot = ReserveCommandSlot();
		}

		[ThreadStatic]
		private static ShaderResourcePackage modifiedSRP;

		private void RenderCache_IterateMaterial(int materialIndex) {
			// Set up context variables
			KeyValuePair<Material, ModelInstanceManager.MIDArray> currentKVP = currentInstanceData[materialIndex];
			Material currentMaterial = currentKVP.Key;
			ModelInstanceManager.MIDArray currentMID = currentKVP.Value;

			// Skip this material if it or its shader are disposed
			if (currentMaterial.IsDisposed || currentMaterial.Shader.IsDisposed) return;

			// Skip this material if we're not using it
			bool inUse = false;
			for (int i = 0; i < currentMID.Length; ++i) {
				if (currentMID.Data[i].InUse) {
					inUse = true;
					break;
				}
			}
			if (!inUse) return;

			// Prepare shader according to material params, and switch to it or update it
			if (lastSetFragmentShader != currentMaterial.Shader || lastFrameNum != frameNum) {
				lastSetFragmentShader = currentMaterial.Shader;
				lastFrameNum = frameNum;
				QueueShaderSwitch(lastSetFragmentShader);
			}
			var queuedSRP = currentMaterial.FragmentShaderResourcePackage;
			if (lastSetFragmentShader == geomFSWithShadowSupport) {
				if (modifiedSRP == null) modifiedSRP = new ShaderResourcePackage();
				modifiedSRP.CopyFrom(queuedSRP);
				modifiedSRP.SetValue((ResourceViewBinding) lastSetFragmentShader.GetBindingByIdentifier("ShadowMap"), previousShadowBufferSRV);
				queuedSRP = modifiedSRP;
			}
			QueueShaderResourceUpdate(lastSetFragmentShader, queuedSRP);

			// Filter & sort
			if (materialFilteringWorkspace == null || materialFilteringWorkspace.Length < currentCache.NumModels) {
				materialFilteringWorkspace = new FastClearList<Transform>[currentCache.NumModels];
				for (int i = 0; i < materialFilteringWorkspace.Length; ++i) materialFilteringWorkspace[i] = new FastClearList<Transform>();
			}
			for (int i = 0; i < materialFilteringWorkspace.Length; ++i) materialFilteringWorkspace[i].Clear();

			SortByProximityToCamera(currentMID);
			uint numInstances = 0U;
			for (uint i = 0U; i < currentMID.Length; ++i) {
				ModelInstanceData curMID = sortedModelData[i];
				if (!curMID.InUse) continue;
				SceneLayer layer = currentSceneLayers[curMID.SceneLayerIndex];
				if (layer == null || !layer.GetRenderingEnabled() || !addedSceneLayers.Contains(layer)) continue;

				if (curMID.ModelIndex == __VEGG_MH.ModelIndex && currentCache.ID == __VEGG_MH.GeoCacheID) {
					int instanceIndex = 0;
					for (int j = 0; j < currentMID.Length; ++j) {
						if (currentMID.Data[j].Transform == curMID.Transform) {
							instanceIndex = j;
							break;
						}
					}
					Quaternion rot = Quaternion.IDENTITY;
					foreach (var kvp in __VEGG_MIH_ARR) {
						if (kvp.Key.InstanceIndex == instanceIndex) {
							rot = kvp.Value;
							break;
						}
					}
					materialFilteringWorkspace[curMID.ModelIndex].Add(curMID.Transform.RotateBy(rot));
				}
				else materialFilteringWorkspace[curMID.ModelIndex].Add(curMID.Transform);
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
					if (mI == __EGGHACK_MH.ModelIndex && currentCache.ID == __EGGHACK_MH.GeoCacheID) {
						instanceConcatWorkspace[nextWorkspaceIndex++] = filteredTransformList[iI].RotateBy(__EGGHACK_ROT).AsMatrixTransposed;
					}
					else instanceConcatWorkspace[nextWorkspaceIndex++] = filteredTransformList[iI].AsMatrixTransposed;
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
				RenderCommand.SetInstanceBuffer(gpuInstanceBuffer, currentVS.InstanceDataBinding.SlotIndex)
			);
			FlushRenderCommands();
		}

		private void SortByProximityToCamera(ModelInstanceManager.MIDArray midArray) {
			if (localMidComparer == null) localMidComparer = new MIDComparer();
			if (sortedModelData == null || sortedModelData.Length < midArray.Length) sortedModelData = new ModelInstanceData[midArray.Length << 1];

			for (int i = 0; i < midArray.Length; ++i) sortedModelData[i] = midArray.Data[i];

			localMidComparer.CamPos = Input.Position;
			Array.Sort(sortedModelData, 0, (int) midArray.Length, localMidComparer);
		}

		private class MIDComparer : IComparer<ModelInstanceData> {
			public Vector3 CamPos;

			public int Compare(ModelInstanceData x, ModelInstanceData y) {
				return Vector3.DistanceSquared(x.Transform.Translation, CamPos).CompareTo(Vector3.DistanceSquared(y.Transform.Translation, CamPos));
			}
		}
	}
}