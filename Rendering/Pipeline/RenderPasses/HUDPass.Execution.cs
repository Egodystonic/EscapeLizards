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
	public unsafe partial class HUDPass {
		private class ZIndexComparer : IComparer<KeyValuePair<Material, ModelInstanceManager.MIDArray>> {
			public int Compare(KeyValuePair<Material, ModelInstanceManager.MIDArray> x, KeyValuePair<Material, ModelInstanceManager.MIDArray> y) {
				return x.Key.ZIndex.CompareTo(y.Key.ZIndex);
			}
		}

		private const int INITIAL_TRANSFORM_BUF_LEN = 1;

		private static readonly IComparer<KeyValuePair<Material, ModelInstanceManager.MIDArray>> zIndexComparer = new ZIndexComparer();
		private static readonly VertexBufferBuilder<Matrix> gpuInstanceBufferBuilder = BufferFactory.NewVertexBuffer<Matrix>().WithUsage(ResourceUsage.DiscardWrite);
		private static readonly Texture2DBuilder<TexelFormat.RGBA8UNorm> textureBuilder = TextureFactory.NewTexture2D<TexelFormat.RGBA8UNorm>()
			.WithMipAllocation(false)
			.WithMipGenerationTarget(false);
		private static readonly VertexBufferBuilder<LightPlaneVertex> glowPlaneBuilder = BufferFactory.NewVertexBuffer<LightPlaneVertex>()
			.WithLength(6U)
			.WithUsage(ResourceUsage.Immutable);
		private static readonly Texture2DBuilder<TexelFormat.DepthStencil> dsBufferBuilder = TextureFactory.NewTexture2D<TexelFormat.DepthStencil>()
			.WithMipAllocation(false)
			.WithMipGenerationTarget(false)
			.WithPermittedBindings(GPUBindings.DepthStencilTarget)
			.WithUsage(ResourceUsage.Write);

		private GeometryCache currentCache;
		private ArraySlice<KeyValuePair<Material, ModelInstanceManager.MIDArray>> currentInstanceData;
		private KeyValuePair<Material, ModelInstanceManager.MIDArray>[] instanceDataSortSpace = new KeyValuePair<Material, ModelInstanceManager.MIDArray>[0];
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
		private DepthStencilViewHandle windowDSVH;

		private readonly TextureSampler copySSO = new TextureSampler(TextureFilterType.Bilinear, TextureWrapMode.Border, AnisotropicFilteringLevel.None);
		private readonly BlendState glowBlendState = new BlendState(BlendOperation.AlphaAwareSrcOverride);
		private readonly BlendState dstMergeBlend = new BlendState(BlendOperation.Additive);
		private VertexBuffer<LightPlaneVertex> glowPlaneVertices;
		private Texture2D<TexelFormat.RGBA8UNorm> preGlowTargetBuffer;
		private RenderTargetView preGlowTargetBufferRTV;
		private ShaderResourceView preGlowTargetBufferSRV;
		private Texture2D<TexelFormat.RGBA8UNorm> glowSrcBuffer, glowDstBuffer;
		private Texture2D<TexelFormat.DepthStencil> glowDSBuffer;
		private DepthStencilView glowDSBufferDSV;
		private RenderTargetView glowSrcBufferRTV;
		private ShaderResourceView glowSrcBufferSRV;
		private RenderTargetView glowDstBufferRTV;
		private ShaderResourceView glowDstBufferSRV;
		private InputLayoutHandle glowPlaneInputLayout = InputLayoutHandle.NULL;

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
				if (glowPlaneVertices != null) glowPlaneVertices.Dispose();
				if (glowPlaneInputLayout != InputLayoutHandle.NULL) {
					InteropUtils.CallNative(NativeMethods.ResourceFactory_ReleaseResource, (ResourceHandle) glowPlaneInputLayout).ThrowOnFailure();
				}
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
			// Set up buffers
			if (!output.TargetWindow.GetWindowRTVAndDSV(out windowRTVH, out windowDSVH)) return;
			Vector2 viewportSizePixels = output.TargetWindow.AddedViewports.First().SizePixels;
			if (viewportSizePixels.X <= 0f || viewportSizePixels.Y <= 0f) return;
			SetUpBuffers(viewportSizePixels);

			// Clear pre-glow buffer
			QueueRenderCommand(RenderCommand.ClearRenderTarget(preGlowTargetBufferRTV));
			QueueRenderCommand(RenderCommand.ClearRenderTarget(glowSrcBufferRTV));
			QueueRenderCommand(RenderCommand.ClearRenderTarget(glowDstBufferRTV));

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
				SetUpCacheForLocalThread();

				// Iterate all model instances (ordered by material && ZIndex)
				//if (instanceDataSortSpace.Length < currentInstanceData.Length) instanceDataSortSpace = new KeyValuePair<Material, ModelInstanceManager.MIDArray>[currentInstanceData.Length * 2];
				//for (int i = 0; i < currentInstanceData.Length; i += 2) {
				//	var a = currentInstanceData[i];
				//	if (currentInstanceData.Length == i + 1) {
				//		if (currentInstanceData.Length >= 3) {
				//			if (a.Key.ZIndex < instanceDataSortSpace[i - 2].Key.ZIndex) {
				//				instanceDataSortSpace[i] = instanceDataSortSpace[i - 1];
				//				instanceDataSortSpace[i - 1] = instanceDataSortSpace[i - 2];
				//				instanceDataSortSpace[i - 2] = a;
				//			}
				//			else if (a.Key.ZIndex < instanceDataSortSpace[i - 1].Key.ZIndex) {
				//				instanceDataSortSpace[i] = instanceDataSortSpace[i - 1];
				//				instanceDataSortSpace[i - 1] = a;
				//			}
				//			else instanceDataSortSpace[i] = a;
				//		}
				//		else instanceDataSortSpace[i] = a;
				//	}
				//	else {
				//		var b = currentInstanceData[i + 1];
				//		if (a.Key.ZIndex <= b.Key.ZIndex) {
				//			instanceDataSortSpace[i] = a;
				//			instanceDataSortSpace[i + 1] = b;
				//		}
				//		else {
				//			instanceDataSortSpace[i] = b;
				//			instanceDataSortSpace[i + 1] = a;
				//		}
				//	}
				//}
				Array.Sort(currentInstanceData.ContainingArray, 0, (int) currentInstanceData.Length, zIndexComparer);
				foreach (KeyValuePair<Material, ModelInstanceManager.MIDArray> material in currentInstanceData) {
					for (int i = 0; i < currentInstanceData.Length; ++i) {
						if (currentInstanceData[i].Value == material.Value && currentInstanceData[i].Key == material.Key) {
							RenderCache_IterateMaterial(i);
							break;
						}
					}
				}

				// Set instance buffer and write to it
				if (gpuInstanceBuffer == null || gpuInstanceBuffer.Length < cpuInstanceBuffer.Length) {
					if (gpuInstanceBuffer != null) gpuInstanceBuffer.Dispose();
					gpuInstanceBuffer = gpuInstanceBufferBuilder.WithLength((uint) cpuInstanceBuffer.Length).Create();
				}
				gpuInstanceBuffer.DiscardWrite(cpuInstanceBuffer); // Happens immediately (required)

				// Set instance buffer and flush all commands, first on immediate context, then on each deferred
				SetInstanceBufferAndFlushCommands();
			}

			///* =============================================
			// * PREPARE FOR GLOW
			// * ============================================= */

			//// Clear glow buffers
			//QueueRenderCommand(RenderCommand.ClearRenderTarget(glowSrcBufferRTV));

			//// Set blend state
			//QueueRenderCommand(RenderCommand.SetBlendState(glowBlendState));

			//// Set topology
			//QueueRenderCommand(RenderCommand.SetPrimitiveTopology(RenderCommand.DEFAULT_PRIMITIVE_TOPOLOGY));

			//// Set input layout
			//QueueRenderCommand(RenderCommand.SetInputLayout(glowPlaneInputLayout));

			//// Enqueue VS commands
			//QueueShaderSwitch(glowVS);
			//QueueShaderResourceUpdate(glowVS);

			///* =============================================
			// * DOWNSCALE TO GLOW SRC BUFFER
			// * ============================================= */

			//// Set up output merger
			//QueueRenderCommand(RenderCommand.SetRenderTargets(glowDSBufferDSV, glowSrcBufferRTV));

			//// Switch to copy shader
			//QueueShaderSwitch(scaleDownShader);
			//QueueShaderResourceUpdate(scaleDownShader, scaleDownShaderResPkg);

			//// Draw fullscreen triangles
			//QueueRenderCommand(RenderCommand.Draw(0, 3U));
			//QueueRenderCommand(RenderCommand.Draw(3, 3U));

			//// Unbind resources
			//QueueShaderResourceUpdate(scaleDownShader, scaleDownShaderResUnbindPkg);

			///* =============================================
			// * RENDER GLOW
			// * ============================================= */

			//// Set up output merger
			//QueueRenderCommand(RenderCommand.SetRenderTargets(glowDSBufferDSV, glowDstBufferRTV));

			//// Switch to glow shader
			//QueueShaderSwitch(glowShader);
			//QueueShaderResourceUpdate(glowShader, glowShaderVResPkg);

			//// Set blend state
			//QueueRenderCommand(RenderCommand.SetBlendState(dstMergeBlend));

			//// Draw fullscreen triangles
			//QueueRenderCommand(RenderCommand.Draw(0, 3U));
			//QueueRenderCommand(RenderCommand.Draw(3, 3U));

			//// Unbind resources
			//QueueShaderResourceUpdate(glowShader, glowShaderVResUnbindPkg);

			///* =============================================
			// * UPSCALE TO BACK BUFFER
			// * ============================================= */

			//// Set up output merger
			//QueueRenderCommand(RenderCommand.SetRenderTargets(output.TargetWindow));

			//// Switch to copy shader
			//QueueShaderSwitch(scaleUpShader);
			//QueueShaderResourceUpdate(scaleUpShader, scaleUpShaderResPkg);

			//// Draw fullscreen triangles
			//QueueRenderCommand(RenderCommand.Draw(0, 3U));
			//QueueRenderCommand(RenderCommand.Draw(3, 3U));

			//// Unbind resources
			//QueueShaderResourceUpdate(scaleUpShader, scaleUpShaderResUnbindPkg);

			// Present
			FlushRenderCommands();
			if (presentAfterPass) PresentBackBuffer(Output.TargetWindow);
		}

		private void SetUpBuffers(Vector2 outputVPSizePixels) {
			bool bufferChange = false;

			if (preGlowTargetBuffer == null || preGlowTargetBuffer.Width != (uint)outputVPSizePixels.X || preGlowTargetBuffer.Height != (uint)outputVPSizePixels.Y) {
				if (preGlowTargetBufferRTV != null) preGlowTargetBufferRTV.Dispose();
				if (preGlowTargetBufferSRV != null) preGlowTargetBufferSRV.Dispose();
				if (preGlowTargetBuffer != null) preGlowTargetBuffer.Dispose();

				preGlowTargetBuffer = textureBuilder
					.WithWidth((uint) outputVPSizePixels.X)
					.WithHeight((uint) outputVPSizePixels.Y)
					.WithPermittedBindings(GPUBindings.ReadableShaderResource | GPUBindings.RenderTarget)
					.WithUsage(ResourceUsage.Write);
				preGlowTargetBufferRTV = preGlowTargetBuffer.CreateRenderTargetView(0U);
				preGlowTargetBufferSRV = preGlowTargetBuffer.CreateView();

				bufferChange = true;
			}

			Vector2 outputSizeOverTwo = outputVPSizePixels * 0.5f;

			if (glowSrcBuffer == null || glowSrcBuffer.Width != (uint) outputSizeOverTwo.X || glowSrcBuffer.Height != (uint) outputSizeOverTwo.Y) {
				if (glowSrcBufferRTV != null) glowSrcBufferRTV.Dispose();
				if (glowSrcBufferSRV != null) glowSrcBufferSRV.Dispose();
				if (glowSrcBuffer != null) glowSrcBuffer.Dispose();

				glowSrcBuffer = textureBuilder
					.WithWidth((uint) outputSizeOverTwo.X)
					.WithHeight((uint) outputSizeOverTwo.Y)
					.WithPermittedBindings(GPUBindings.ReadableShaderResource | GPUBindings.RenderTarget)
					.WithUsage(ResourceUsage.Write);
				glowSrcBufferRTV = glowSrcBuffer.CreateRenderTargetView(0U);
				glowSrcBufferSRV = glowSrcBuffer.CreateView();

				bufferChange = true;
			}

			if (glowDstBuffer == null || glowDstBuffer.Width != (uint) outputSizeOverTwo.X || glowDstBuffer.Height != (uint) outputSizeOverTwo.Y) {
				if (glowDstBufferRTV != null) glowDstBufferRTV.Dispose();
				if (glowDstBufferSRV != null) glowDstBufferSRV.Dispose();
				if (glowDstBuffer != null) glowDstBuffer.Dispose();

				glowDstBuffer = textureBuilder
					.WithWidth((uint) outputSizeOverTwo.X)
					.WithHeight((uint) outputSizeOverTwo.Y)
					.WithPermittedBindings(GPUBindings.ReadableShaderResource | GPUBindings.RenderTarget)
					.WithUsage(ResourceUsage.Write);
				glowDstBufferRTV = glowDstBuffer.CreateRenderTargetView(0U);
				glowDstBufferSRV = glowDstBuffer.CreateView();

				bufferChange = true;
			}

			if (glowDSBuffer == null || glowDSBuffer.Width != (uint) outputSizeOverTwo.X || glowDSBuffer.Height != (uint) outputSizeOverTwo.Y) {
				if (glowDSBufferDSV != null) glowDSBufferDSV.Dispose();
				if (glowDSBuffer != null) glowDSBuffer.Dispose();

				glowDSBuffer = dsBufferBuilder
					.WithWidth((uint) outputSizeOverTwo.X)
					.WithHeight((uint) outputSizeOverTwo.Y);
				glowDSBufferDSV = glowDSBuffer.CreateDepthStencilView(0U);

				bufferChange = true;
			}

			if (bufferChange) SetUpShaderResPackages();
		}

		private void SetUpShaderResPackages() {
			if (scaleDownShader != null) {
				scaleDownShaderResPkg = new ShaderResourcePackage();
				scaleDownShaderResPkg.SetValue((ResourceViewBinding) scaleDownShader.GetBindingByIdentifier("SourceTex"), preGlowTargetBufferSRV);
				scaleDownShaderResPkg.SetValue((TextureSamplerBinding) scaleDownShader.GetBindingByIdentifier("SourceSampler"), copySSO);
				scaleDownShaderResUnbindPkg = new ShaderResourcePackage();
				scaleDownShaderResUnbindPkg.SetValue((ResourceViewBinding) scaleDownShader.GetBindingByIdentifier("SourceTex"), null);
			}
			if (scaleUpShader != null) {
				scaleUpShaderResPkg = new ShaderResourcePackage();
				scaleUpShaderResPkg.SetValue((ResourceViewBinding) scaleUpShader.GetBindingByIdentifier("SourceTex"), glowDstBufferSRV);
				scaleUpShaderResPkg.SetValue((TextureSamplerBinding) scaleUpShader.GetBindingByIdentifier("SourceSampler"), copySSO);
				scaleUpShaderResUnbindPkg = new ShaderResourcePackage();
				scaleUpShaderResUnbindPkg.SetValue((ResourceViewBinding) scaleUpShader.GetBindingByIdentifier("SourceTex"), null);
			}
			if (glowShader != null) {
				glowShaderVResPkg = new ShaderResourcePackage();
				glowShaderVResPkg.SetValue((ResourceViewBinding) glowShader.GetBindingByIdentifier("GlowSrc"), glowSrcBufferSRV);
				glowShaderVResUnbindPkg = new ShaderResourcePackage();
				glowShaderVResUnbindPkg.SetValue((ResourceViewBinding) glowShader.GetBindingByIdentifier("GlowSrc"), null);
			}
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
			QueueRenderCommand(RenderCommand.SetRenderTargets(output.TargetWindow));

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

		private void SetUpGlowPlane() {
			if (glowPlaneVertices != null) glowPlaneVertices.Dispose();
			if (glowPlaneInputLayout != InputLayoutHandle.NULL) {
				InteropUtils.CallNative(NativeMethods.ResourceFactory_ReleaseResource, (ResourceHandle) glowPlaneInputLayout).ThrowOnFailure();
			}
			if (glowVS != null) {
				glowPlaneVertices = glowPlaneBuilder.WithInitialData(new[] {
					new LightPlaneVertex(new Vector3(-1f, -1f, 0f), new Vector2(0f, 1f)),
					new LightPlaneVertex(new Vector3(-1f, 1f, 0f), new Vector2(0f, 0f)),
					new LightPlaneVertex(new Vector3(1f, 1f, 0f), new Vector2(1f, 0f)),

					new LightPlaneVertex(new Vector3(1f, 1f, 0f), new Vector2(1f, 0f)),
					new LightPlaneVertex(new Vector3(1f, -1f, 0f), new Vector2(1f, 1f)),
					new LightPlaneVertex(new Vector3(-1f, -1f, 0f), new Vector2(0f, 1f)),
				});

				InputElementDesc[] inputElements = {
					new InputElementDesc(
						"POSITION",
						0U,
						BaseResource.GetFormatForType(typeof(Vector3)),
						0U,
						true
					),
					new InputElementDesc(
						"TEXCOORD",
						0U,
						BaseResource.GetFormatForType(typeof(Vector2)),
						0U,
						true
					)
				};

				InputLayoutHandle outInputLayoutPtr;
				InteropUtils.CallNative(NativeMethods.ShaderManager_CreateInputLayout,
					RenderingModule.Device,
					glowVS.Handle,
					inputElements,
					(uint) inputElements.Length,
					(IntPtr) (&outInputLayoutPtr)
				).ThrowOnFailure();

				glowPlaneInputLayout = outInputLayoutPtr;

				glowVS.GetBindingByIdentifier("POSITION").Bind(glowPlaneVertices);
			}
		}
	}
}