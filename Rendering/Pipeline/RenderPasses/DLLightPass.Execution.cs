// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 22 01 2015 at 15:43 by Ben Bowen

using System;
using System.Collections;
using System.Configuration;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Rendering {
	public unsafe partial class DLLightPass {
		private const int LIGHTING_TILE_GRANULARITY = 5;
		private static readonly List<LightProperties[]> perTileLightPropsWorkspace = new List<LightProperties[]>();
		private static readonly int[] perTileLightPropCounts = new int[LIGHTING_TILE_GRANULARITY * LIGHTING_TILE_GRANULARITY];
		private static readonly float[] tileOffsetsX = new float[LIGHTING_TILE_GRANULARITY + 1];
		private static readonly float[] tileOffsetsY = new float[LIGHTING_TILE_GRANULARITY + 1];
		private static readonly VertexBufferBuilder<LightPlaneVertex> lightPlaneBuilder = BufferFactory.NewVertexBuffer<LightPlaneVertex>()
			.WithLength(6U)
			.WithUsage(ResourceUsage.Immutable);
		private static readonly Texture2DBuilder<TexelFormat.RGBA8UNorm> preBloomBufferBuilder = TextureFactory.NewTexture2D<TexelFormat.RGBA8UNorm>()
			.WithMipAllocation(false)
			.WithMipGenerationTarget(false)
			.WithPermittedBindings(GPUBindings.RenderTarget | GPUBindings.ReadableShaderResource)
			.WithUsage(ResourceUsage.Write);
		private static readonly Texture2DBuilder<TexelFormat.DepthStencil> dsBufferBuilder = TextureFactory.NewTexture2D<TexelFormat.DepthStencil>()
			.WithMipAllocation(false)
			.WithMipGenerationTarget(false)
			.WithPermittedBindings(GPUBindings.DepthStencilTarget)
			.WithUsage(ResourceUsage.Write);
		private readonly Texture2D<TexelFormat.RGBA32Float>[] previousGBuffer = new Texture2D<TexelFormat.RGBA32Float>[DLGeometryPass.NUM_GBUFFER_TEXTURES];
		private readonly LightProperties[] lightPropsWorkspace = new LightProperties[MAX_DYNAMIC_LIGHTS];
		private Texture2D<TexelFormat.RGBA8UNorm> preBloomBuffer, reducedBloomBuffer, bloomTargetBuffer;
		private Texture2D<TexelFormat.DepthStencil> bloomResizeCopyDSBuffer, dsThrowawayBuffer;
		private Texture2D<TexelFormat.RGBA8UNorm> nonDepthOfFieldBackBuffer, reducedNonDepthOfFieldBackBuffer, depthOfFieldBackBuffer;
		private ShaderResourceView preBloomBufferSRV;
		private ShaderResourceView reducedBloomBufferSRV, bloomTargetBufferSRV;
		private RenderTargetView preBloomBufferRTV, bloomTargetBufferRTV;
		private RenderTargetView reducedBloomBufferRTV;
		private DepthStencilView bloomResizeCopyDSDSV;
		private DepthStencilView dsThrowawayBufferDSV;
		private RenderTargetView nonDepthOfFieldBackBufferRTV, reducedNonDepthOfFieldBackBufferRTV, depthOfFieldBackBufferRTV;
		private ShaderResourceView nonDepthOfFieldBackBufferSRV, reducedNonDepthOfFieldBackBufferSRV, depthOfFieldBackBufferSRV;
		private readonly ShaderResourceView[] gBufferSRVArray = new ShaderResourceView[DLGeometryPass.NUM_GBUFFER_TEXTURES];
		private readonly RasterizerState rsState = new RasterizerState(false, TriangleCullMode.NoCulling, false, 0, 0f, 0f, false);
		private readonly DepthStencilState dsState = new DepthStencilState(false);
		private readonly BlendState blendState = new BlendState(BlendOperation.Additive);
		private readonly BlendState outliningBlendState = new BlendState(BlendOperation.Minimal);
		//private readonly TextureSampler outliningSamplerA = new TextureSampler(TextureFilterType.MinMagMipLinear, TextureWrapMode.Clamp, AnisotropicFilteringLevel.None);
		//private readonly TextureSampler outliningSamplerB = new TextureSampler(TextureFilterType.MinMagMipPoint, TextureWrapMode.Clamp, AnisotropicFilteringLevel.None);
		private readonly TextureSampler copySSO = new TextureSampler(TextureFilterType.Trilinear, TextureWrapMode.Mirror, AnisotropicFilteringLevel.EightTimes);
		private VertexBuffer<LightPlaneVertex> lightPlaneVertices;
		private InputLayoutHandle lightPlaneInputLayout = InputLayoutHandle.NULL;
		private readonly DynamicLightSorter dynamicLightComparer = new DynamicLightSorter();
		private int dynamicLightCap = MAX_DYNAMIC_LIGHTS;
		private ConstantBufferBinding lightVSScalarsBufferBinding = null;

		public int? DynamicLightCap {
			get {
				lock (InstanceMutationLock) {
					return dynamicLightCap < MAX_DYNAMIC_LIGHTS ? (int?) dynamicLightCap : null;
				}
			}
			set {
				lock (InstanceMutationLock) {
					dynamicLightCap = value ?? MAX_DYNAMIC_LIGHTS;
				}
			}
		}

		public override void Dispose() {
			lock (InstanceMutationLock) {
				if (IsDisposed) return;
				rsState.Dispose();
				dsState.Dispose();
				lightPlaneVertices.Dispose();
				gBufferSRVArray.ForEach(srv => srv.Dispose());
				InteropUtils.CallNative(NativeMethods.ResourceFactory_ReleaseResource, (ResourceHandle) lightPlaneInputLayout).ThrowOnFailure();
				blendState.Dispose();
				base.Dispose();
			}
		}

		protected internal override void Execute(ParallelizationProvider pp) {
			// See if we need to resize the light plane and get the new GBuffer
			Camera input;
			SceneViewport output;
			Texture2D<TexelFormat.RGBA32Float>[] currentGBuffer;
			geometryPass.GetLightPassParameters(out currentGBuffer, out input, out output);
			Vector2 outputSizePixels = output.SizePixels;
			if (outputSizePixels.X <= 0f || outputSizePixels.Y <= 0f) return;
			RenderTargetViewHandle windowRTV; DepthStencilViewHandle windowDSV;
			bool windowStillOpen = output.TargetWindow.GetWindowRTVAndDSV(out windowRTV, out windowDSV);
			if (!windowStillOpen) return;
			CheckGeometryPassParameters(currentGBuffer, outputSizePixels);

			// Clear the bloom textures
			QueueRenderCommand(RenderCommand.ClearRenderTarget(preBloomBufferRTV));
			QueueRenderCommand(RenderCommand.ClearRenderTarget(reducedBloomBufferRTV));
			QueueRenderCommand(RenderCommand.ClearRenderTarget(bloomTargetBufferRTV));
			QueueRenderCommand(RenderCommand.ClearRenderTarget(nonDepthOfFieldBackBufferRTV));
			QueueRenderCommand(RenderCommand.ClearRenderTarget(reducedNonDepthOfFieldBackBufferRTV));
			QueueRenderCommand(RenderCommand.ClearRenderTarget(depthOfFieldBackBufferRTV));
			QueueRenderCommand(RenderCommand.ClearDepthStencil(bloomResizeCopyDSDSV));
			QueueRenderCommand(RenderCommand.ClearDepthStencil(dsThrowawayBufferDSV));

			// Set topology
			QueueRenderCommand(RenderCommand.SetPrimitiveTopology(RenderCommand.DEFAULT_PRIMITIVE_TOPOLOGY));

			// Set input layout
			QueueRenderCommand(RenderCommand.SetInputLayout(lightPlaneInputLayout));

			// Enqueue VS commands
			QueueShaderSwitch(dlLightVS);
			QueueShaderResourceUpdate(dlLightVS, vsResPackage);

			/* =======================================
			 * STAGE: DYNAMIC LIGHTING APPLICATION
			 * ======================================= */

			// Enqueue FS commands
			Vector4 cameraPos = input.Position;
			((ConstantBufferBinding) dlLightFS.GetBindingByIdentifier("CameraProperties")).SetValue((byte*) (&cameraPos));
			QueueShaderSwitch(dlLightFS);

			// Set rasterizer state
			QueueRenderCommand(RenderCommand.SetRasterizerState(rsState));
			QueueRenderCommand(RenderCommand.SetViewport(output));

			// Set depth stencil state
			QueueRenderCommand(RenderCommand.SetDepthStencilState(dsState));

			// Set blend state
			QueueRenderCommand(RenderCommand.SetBlendState(blendState));

			// Set up output merger
			QueueRenderCommand(RenderCommand.SetRenderTargets(dsThrowawayBufferDSV.ResourceViewHandle, nonDepthOfFieldBackBufferRTV.ResourceViewHandle, preBloomBufferRTV.ResourceViewHandle));

			// Draw lights
			//input = new Camera();
			//input.Position = Vector3.ZERO;
			//input.Orient(Vector3.FORWARD, Vector3.UP);

			var frustum = input.GetFrustum(output);
			int numLights = addedLights.Count;
			int numLightsInFrustum = 0;
			for (int i = 0; i < numLights; ++i) {
				if (!frustum.IsWithinFrustum(new Sphere(addedLights[i].Position, addedLights[i].Radius))) continue;
				lightPropsWorkspace[numLightsInFrustum++] = addedLights[i].Properties;
			}
			if (numLightsInFrustum > dynamicLightCap) {
				dynamicLightComparer.CameraPosition = input.Position;
				Array.Sort(lightPropsWorkspace, 0, numLightsInFrustum, dynamicLightComparer);
				numLightsInFrustum = dynamicLightCap;
			}

			Buffer<LightProperties> lightBuffer = (Buffer<LightProperties>) (((ResourceViewBinding) dlLightFS.GetBindingByIdentifier("LightBuffer")).GetBoundResource().Resource);
			var lightMetaCBuffer = (ConstantBufferBinding) dlLightFS.GetBindingByIdentifier("LightMeta");
			QueueShaderResourceUpdate(dlLightFS, fsResPackage);

			Array.Clear(perTileLightPropCounts, 0, perTileLightPropCounts.Length);

			

			Vector3 upDir = input.UpDirection;
			Vector3 downDir = -input.UpDirection;
			Vector3 rightDir = upDir.Cross(input.Orientation);
			Vector3 leftDir = -rightDir;
			var worldToProjMat = (*((Matrix*) input.GetRecalculatedViewMatrix()) * *((Matrix*) output.GetRecalculatedProjectionMatrix(input)));

			for (int i = 0; i < numLightsInFrustum; ++i) {
				var lightProps = lightPropsWorkspace[i];
				//lightProps = new LightProperties(
				//	Vector3.LEFT + Vector3.BACKWARD, 3f, Vector3.ONE
				//);
				var lightCentre = lightProps.Position;
				var lightTop = new Vector4(lightCentre + upDir * lightProps.Radius, w: 1f);
				var lightBottom = new Vector4(lightCentre + downDir * lightProps.Radius, w: 1f);
				var lightLeftmost = new Vector4(lightCentre + leftDir * lightProps.Radius, w: 1f);
				var lightRightmost = new Vector4(lightCentre + rightDir * lightProps.Radius, w: 1f);

				var lightTopProjspace = lightTop * worldToProjMat;
				var lightBottomProjspace = lightBottom * worldToProjMat;
				var lightLeftmostProjspace = lightLeftmost * worldToProjMat;
				var lightRightmostProjspace = lightRightmost * worldToProjMat;

				var lightTopScreenSpace = lightTopProjspace / Math.Abs(lightTopProjspace.W);
				var lightBottomScreenSpace = lightBottomProjspace / Math.Abs(lightBottomProjspace.W);
				var lightLeftmostScreenSpace = lightLeftmostProjspace / Math.Abs(lightLeftmostProjspace.W);
				var lightRightmostScreenSpace = lightRightmostProjspace / Math.Abs(lightRightmostProjspace.W);

				var xMin = ((float) MathUtils.Clamp(lightLeftmostScreenSpace.X, -1f, 1f) + 1f) * 0.5f;
				var xMax = ((float) MathUtils.Clamp(lightRightmostScreenSpace.X, -1f, 1f) + 1f) * 0.5f;
				var yMin = ((float) MathUtils.Clamp(lightBottomScreenSpace.Y, -1f, 1f) + 1f) * 0.5f;
				var yMax = ((float) MathUtils.Clamp(lightTopScreenSpace.Y, -1f, 1f) + 1f) * 0.5f;

				for (int x = 0; x < LIGHTING_TILE_GRANULARITY; ++x) {
					for (int y = 0; y < LIGHTING_TILE_GRANULARITY; ++y) {
						var tileXMin = tileOffsetsX[x];
						var tileXMax = tileOffsetsX[x + 1];
						var tileYMin = tileOffsetsY[y];
						var tileYMax = tileOffsetsY[y + 1];

						if (xMax < tileXMin
							|| xMin > tileXMax
							|| yMax < tileYMin
							|| yMin > tileYMax) {
							continue;
						}

						var bucketIndex = x * LIGHTING_TILE_GRANULARITY + y;
						perTileLightPropsWorkspace[bucketIndex][perTileLightPropCounts[bucketIndex]++] = lightProps;
					}
				}
			}

			for (int x = 0; x < LIGHTING_TILE_GRANULARITY; ++x) {
				for (int y = 0; y < LIGHTING_TILE_GRANULARITY; ++y) {
					var bucketIndex = x * LIGHTING_TILE_GRANULARITY + y;
					var numLightsOnThisTile = perTileLightPropCounts[bucketIndex];

					if (numLightsOnThisTile == 0) continue;

					var scalars = new Vector4(tileOffsetsX[x], tileOffsetsX[x + 1], tileOffsetsY[y], tileOffsetsY[y + 1]); // 0f to 1f, from bottom left corner
					QueueRenderCommand(RenderCommand.DiscardWriteShaderConstantBuffer(
						lightBuffer, 
						new ArraySlice<LightProperties>(perTileLightPropsWorkspace[bucketIndex], 0U, (uint) numLightsOnThisTile), 
						(uint) sizeof(LightProperties)
					));
					int* numLightsWithPadding = stackalloc int[4];
					numLightsWithPadding[0] = numLightsOnThisTile;

					QueueRenderCommand(RenderCommand.DiscardWriteShaderConstantBuffer(lightMetaCBuffer, (IntPtr) (numLightsWithPadding)));

					QueueRenderCommand(RenderCommand.DiscardWriteShaderConstantBuffer(lightVSScalarsBufferBinding, (IntPtr) (&scalars)));
					QueueRenderCommand(RenderCommand.Draw(0, 3U));
					QueueRenderCommand(RenderCommand.Draw(3, 3U));
				}
			}
			
			
			

			// Unbind gbuffer
			QueueShaderResourceUpdate(dlLightFS, fsUnbindResPackage);

			/* =======================================
			 * STAGE: ADD AMBIENT LIGHT (DL FINAL)
			 * ======================================= */

			// Switch to finalization shader
			var scalarsFinal = new Vector4(0f, 1f, 0f, 1f);
			QueueRenderCommand(RenderCommand.DiscardWriteShaderConstantBuffer(lightVSScalarsBufferBinding, (IntPtr) (&scalarsFinal)));
			QueueShaderSwitch(dlFinalFS);
			QueueShaderResourceUpdate(dlFinalFS, finalizationShaderResPackage);

			// Draw finalization triangles
			QueueRenderCommand(RenderCommand.Draw(0, 3U));
			QueueRenderCommand(RenderCommand.Draw(3, 3U));

			// Unbind resources
			QueueShaderResourceUpdate(dlFinalFS, finalizationShaderUnbindResPackage);

			/* =======================================
			 * STAGE: OUTLINING
			 * ======================================= */

			// Switch to outlining shader
			QueueShaderSwitch(outliningShader);
			QueueShaderResourceUpdate(outliningShader, outliningShaderResPackage);

			// Set blend state
			QueueRenderCommand(RenderCommand.SetBlendState(outliningBlendState));

			// Draw outlining triangles
			QueueRenderCommand(RenderCommand.Draw(0, 3U));
			QueueRenderCommand(RenderCommand.Draw(3, 3U));

			// Unbind resources
			QueueShaderResourceUpdate(outliningShader, outliningShaderUnbindResPackage);

			/* =======================================
			 * STAGE: DOWNSCALE PRE-BLOOM BUFFER TEX
			 * ======================================= */

			// Set up output merger
			QueueRenderCommand(RenderCommand.SetRenderTargets(bloomResizeCopyDSDSV, reducedBloomBufferRTV));

			// Switch to copy shader
			QueueShaderSwitch(copyShader);
			QueueShaderResourceUpdate(copyShader, copyShaderResPackage);

			// Set blend state
			QueueRenderCommand(RenderCommand.SetBlendState(blendState));

			// Draw fullscreen triangles
			QueueRenderCommand(RenderCommand.Draw(0, 3U));
			QueueRenderCommand(RenderCommand.Draw(3, 3U));

			// Unbind resources
			QueueShaderResourceUpdate(copyShader, copyShaderUnbindResPackage);

			/* =======================================
			 * STAGE: BLOOM RENDER TO BLOOM TARGET
			 * ======================================= */

			// Clear DSV
			QueueRenderCommand(RenderCommand.ClearDepthStencil(bloomResizeCopyDSDSV));

			// Set up output merger
			QueueRenderCommand(RenderCommand.SetRenderTargets(bloomResizeCopyDSDSV, bloomTargetBufferRTV));

			// Switch to bloom H shader
			QueueShaderSwitch(bloomHShader);
			QueueShaderResourceUpdate(bloomHShader, bloomHShaderResPackage);

			// Draw fullscreen triangles
			QueueRenderCommand(RenderCommand.Draw(0, 3U));
			QueueRenderCommand(RenderCommand.Draw(3, 3U));

			// Unbind resources
			QueueShaderResourceUpdate(bloomHShader, bloomHShaderUnbindResPackage);

			// Switch to bloom V shader
			QueueShaderSwitch(bloomVShader);
			QueueShaderResourceUpdate(bloomVShader, bloomVShaderResPackage);

			// Draw fullscreen triangles
			QueueRenderCommand(RenderCommand.Draw(0, 3U));
			QueueRenderCommand(RenderCommand.Draw(3, 3U));

			// Unbind resources
			QueueShaderResourceUpdate(bloomVShader, bloomVShaderUnbindResPackage);

			/* =======================================
			 * STAGE: COPY BLOOM RESULT ON TO NON-DoF BUFFER
			 * ======================================= */

			// Set up output merger
			QueueRenderCommand(RenderCommand.SetRenderTargets(dsThrowawayBufferDSV.ResourceViewHandle, nonDepthOfFieldBackBufferRTV));

			// Switch to reverse copy shader
			QueueShaderSwitch(copyReverseShader);
			QueueShaderResourceUpdate(copyReverseShader, copyReverseShaderResPackage);

			// Draw fullscreen triangles
			QueueRenderCommand(RenderCommand.Draw(0, 3U));
			QueueRenderCommand(RenderCommand.Draw(3, 3U));

			// Unbind resources
			QueueShaderResourceUpdate(copyReverseShader, copyReverseShaderUnbindResPackage);

			/* =======================================
			 * STAGE: DOWNSCALE NON-DoF SCENE
			 * ======================================= */

			// Set up output merger
			QueueRenderCommand(RenderCommand.SetRenderTargets(bloomResizeCopyDSDSV, reducedNonDepthOfFieldBackBufferRTV));

			// Switch to copy shader
			QueueShaderSwitch(copyShader);
			QueueShaderResourceUpdate(copyShader, copyDoFShaderResPackage);

			// Set blend state
			QueueRenderCommand(RenderCommand.SetBlendState(blendState));

			// Draw fullscreen triangles
			QueueRenderCommand(RenderCommand.Draw(0, 3U));
			QueueRenderCommand(RenderCommand.Draw(3, 3U));

			// Unbind resources
			QueueShaderResourceUpdate(copyShader, copyDoFShaderUnbindResPackage);

			/* =======================================
			 * STAGE: BLUR NON-DoF SCENE
			 * ======================================= */

			// Set up output merger
			QueueRenderCommand(RenderCommand.SetRenderTargets(bloomResizeCopyDSDSV, depthOfFieldBackBufferRTV));

			// Switch to copy shader
			QueueShaderSwitch(blurShader);
			QueueShaderResourceUpdate(blurShader, blurShaderResPackage);

			// Set blend state
			QueueRenderCommand(RenderCommand.SetBlendState(blendState));

			// Draw fullscreen triangles
			QueueRenderCommand(RenderCommand.Draw(0, 3U));
			QueueRenderCommand(RenderCommand.Draw(3, 3U));

			// Unbind resources
			QueueShaderResourceUpdate(blurShader, blurShaderUnbindResPackage);

			/* =======================================
			 * STAGE: RENDER TO BACK BUFFER WITH DoF SELECTION
			 * ======================================= */

			// Set up output merger
			QueueRenderCommand(RenderCommand.SetRenderTargets(windowDSV, windowRTV));

			// Switch to copy shader
			QueueShaderSwitch(dofShader);
			QueueShaderResourceUpdate(dofShader, dofShaderResPackage);

			// Set blend state
			QueueRenderCommand(RenderCommand.SetBlendState(blendState));

			// Draw fullscreen triangles
			QueueRenderCommand(RenderCommand.Draw(0, 3U));
			QueueRenderCommand(RenderCommand.Draw(3, 3U));

			// Unbind resources
			QueueShaderResourceUpdate(dofShader, dofShaderUnbindResPackage);





			// Flush + present
			FlushRenderCommands();
			if (presentAfterPass) {
				PresentBackBuffer(output.TargetWindow);
			}
		}

		private void CheckGeometryPassParameters(Texture2D<TexelFormat.RGBA32Float>[] currentGBuffer, Vector2 outputSizePixels) {
			if (preBloomBuffer == null || preBloomBufferRTV.ResourceOrViewDisposed || preBloomBufferSRV.ResourceOrViewDisposed 
				|| preBloomBuffer.Width != (uint) outputSizePixels.X || preBloomBuffer.Height != (uint) outputSizePixels.Y) {
				if (preBloomBufferRTV != null && !preBloomBufferRTV.IsDisposed) preBloomBufferRTV.Dispose();
				if (preBloomBufferSRV != null && !preBloomBufferSRV.IsDisposed) preBloomBufferSRV.Dispose();
				if (preBloomBuffer != null && !preBloomBuffer.IsDisposed) preBloomBuffer.Dispose();
				if (dsThrowawayBufferDSV != null && !dsThrowawayBufferDSV.IsDisposed) dsThrowawayBufferDSV.Dispose();
				if (dsThrowawayBuffer != null && !dsThrowawayBuffer.IsDisposed) dsThrowawayBuffer.Dispose();
				preBloomBuffer = preBloomBufferBuilder.WithWidth((uint) outputSizePixels.X).WithHeight((uint) outputSizePixels.Y);
				dsThrowawayBuffer = dsBufferBuilder.WithWidth((uint) outputSizePixels.X).WithHeight((uint) outputSizePixels.Y);
				preBloomBufferSRV = preBloomBuffer.CreateView();
				preBloomBufferRTV = preBloomBuffer.CreateRenderTargetView(0U);
				dsThrowawayBufferDSV = dsThrowawayBuffer.CreateDepthStencilView(0U);
				RecalculateLightTileOffsets();
			}
			if (nonDepthOfFieldBackBuffer == null || nonDepthOfFieldBackBufferRTV.ResourceOrViewDisposed || nonDepthOfFieldBackBufferSRV.ResourceOrViewDisposed
			|| nonDepthOfFieldBackBuffer.Width != (uint)outputSizePixels.X || nonDepthOfFieldBackBuffer.Height != (uint)outputSizePixels.Y) {
				if (nonDepthOfFieldBackBuffer != null && !nonDepthOfFieldBackBuffer.IsDisposed) nonDepthOfFieldBackBuffer.Dispose();
				if (nonDepthOfFieldBackBufferRTV != null && !nonDepthOfFieldBackBufferRTV.IsDisposed) nonDepthOfFieldBackBufferRTV.Dispose();
				if (nonDepthOfFieldBackBufferSRV != null && !nonDepthOfFieldBackBufferSRV.IsDisposed) nonDepthOfFieldBackBufferSRV.Dispose();
				nonDepthOfFieldBackBuffer = preBloomBufferBuilder.WithWidth((uint) outputSizePixels.X).WithHeight((uint) outputSizePixels.Y);
				nonDepthOfFieldBackBufferRTV = nonDepthOfFieldBackBuffer.CreateRenderTargetView(0U);
				nonDepthOfFieldBackBufferSRV = nonDepthOfFieldBackBuffer.CreateView();
			}
			outputSizePixels /= 2f;
			if (reducedBloomBuffer == null || reducedBloomBufferRTV.ResourceOrViewDisposed || reducedBloomBufferSRV.ResourceOrViewDisposed 
				|| reducedBloomBuffer.Width != (uint) outputSizePixels.X || reducedBloomBuffer.Height != (uint) outputSizePixels.Y) {
				if (reducedBloomBufferRTV != null && !reducedBloomBufferRTV.IsDisposed) reducedBloomBufferRTV.Dispose();
				if (reducedBloomBufferSRV != null && !reducedBloomBufferSRV.IsDisposed) reducedBloomBufferSRV.Dispose();
				if (reducedBloomBuffer != null && !reducedBloomBuffer.IsDisposed) reducedBloomBuffer.Dispose();
				reducedBloomBuffer = preBloomBufferBuilder.WithWidth((uint) outputSizePixels.X).WithHeight((uint) outputSizePixels.Y);
				reducedBloomBufferSRV = reducedBloomBuffer.CreateView();
				reducedBloomBufferRTV = reducedBloomBuffer.CreateRenderTargetView(0U);

				if (bloomTargetBufferSRV != null) bloomTargetBufferSRV.Dispose();
				if (bloomTargetBuffer != null) bloomTargetBuffer.Dispose();
				bloomTargetBuffer = reducedBloomBuffer.Clone();
				bloomTargetBufferSRV = bloomTargetBuffer.CreateView();
				bloomTargetBufferRTV = bloomTargetBuffer.CreateRenderTargetView(0U);

				if (bloomResizeCopyDSDSV != null) bloomResizeCopyDSDSV.Dispose();
				if (bloomResizeCopyDSBuffer != null) bloomResizeCopyDSBuffer.Dispose();
				bloomResizeCopyDSBuffer = dsBufferBuilder.WithWidth((uint) outputSizePixels.X).WithHeight((uint) outputSizePixels.Y);
				bloomResizeCopyDSDSV = bloomResizeCopyDSBuffer.CreateDepthStencilView(0U);
			}

			if (depthOfFieldBackBuffer == null || depthOfFieldBackBufferRTV.ResourceOrViewDisposed || depthOfFieldBackBufferSRV.ResourceOrViewDisposed
			|| depthOfFieldBackBuffer.Width != (uint) outputSizePixels.X || depthOfFieldBackBuffer.Height != (uint) outputSizePixels.Y) {
				if (depthOfFieldBackBuffer != null && !depthOfFieldBackBuffer.IsDisposed) depthOfFieldBackBuffer.Dispose();
				if (depthOfFieldBackBufferRTV != null && !depthOfFieldBackBufferRTV.IsDisposed) depthOfFieldBackBufferRTV.Dispose();
				if (depthOfFieldBackBufferSRV != null && !depthOfFieldBackBufferSRV.IsDisposed) depthOfFieldBackBufferSRV.Dispose();
				depthOfFieldBackBuffer = preBloomBufferBuilder.WithWidth((uint) outputSizePixels.X).WithHeight((uint) outputSizePixels.Y);
				depthOfFieldBackBufferRTV = depthOfFieldBackBuffer.CreateRenderTargetView(0U);
				depthOfFieldBackBufferSRV = depthOfFieldBackBuffer.CreateView();
			}
			if (reducedNonDepthOfFieldBackBuffer == null || reducedNonDepthOfFieldBackBufferRTV.ResourceOrViewDisposed || reducedNonDepthOfFieldBackBufferSRV.ResourceOrViewDisposed
			|| reducedNonDepthOfFieldBackBuffer.Width != (uint) outputSizePixels.X || reducedNonDepthOfFieldBackBuffer.Height != (uint) outputSizePixels.Y) {
				if (reducedNonDepthOfFieldBackBuffer != null && !reducedNonDepthOfFieldBackBuffer.IsDisposed) reducedNonDepthOfFieldBackBuffer.Dispose();
				if (reducedNonDepthOfFieldBackBufferRTV != null && !reducedNonDepthOfFieldBackBufferRTV.IsDisposed) reducedNonDepthOfFieldBackBufferRTV.Dispose();
				if (reducedNonDepthOfFieldBackBufferSRV != null && !reducedNonDepthOfFieldBackBufferSRV.IsDisposed) reducedNonDepthOfFieldBackBufferSRV.Dispose();
				reducedNonDepthOfFieldBackBuffer = preBloomBufferBuilder.WithWidth((uint) outputSizePixels.X).WithHeight((uint) outputSizePixels.Y);
				reducedNonDepthOfFieldBackBufferRTV = reducedNonDepthOfFieldBackBuffer.CreateRenderTargetView(0U);
				reducedNonDepthOfFieldBackBufferSRV = reducedNonDepthOfFieldBackBuffer.CreateView();
			}

			if (outliningShader != null && outliningShaderResPackage.GetValue((ResourceViewBinding) outliningShader.GetBindingByIdentifier("GeomDepthBuffer")) != GeometryPass.PrimaryDSBufferSRV) {
				outliningShaderResPackage.SetValue((ResourceViewBinding) outliningShader.GetBindingByIdentifier("GeomDepthBuffer"), GeometryPass.PrimaryDSBufferSRV);
				outliningShaderUnbindResPackage.SetValue((ResourceViewBinding) outliningShader.GetBindingByIdentifier("GeomDepthBuffer"), null);
			}

			if (dofShader != null && dofShaderResPackage.GetValue((ResourceViewBinding)dofShader.GetBindingByIdentifier("SceneDepth")) != GeometryPass.PrimaryDSBufferSRV) {
				dofShaderResPackage.SetValue((ResourceViewBinding) dofShader.GetBindingByIdentifier("SceneDepth"), GeometryPass.PrimaryDSBufferSRV);
				dofShaderUnbindResPackage.SetValue((ResourceViewBinding) dofShader.GetBindingByIdentifier("SceneDepth"), null);
			}

			bool bufferChange = false;
			for (int i = 0; i < DLGeometryPass.NUM_GBUFFER_TEXTURES; ++i) {
				if (previousGBuffer[i] != currentGBuffer[i]) {
					bufferChange = true;
					break;
				}
			}
			if (!bufferChange) return;
			Array.Copy(currentGBuffer, previousGBuffer, DLGeometryPass.NUM_GBUFFER_TEXTURES);
			for (int i = 0; i < DLGeometryPass.NUM_GBUFFER_TEXTURES; ++i) {
				if (gBufferSRVArray[i] != null) gBufferSRVArray[i].Dispose();
				gBufferSRVArray[i] = currentGBuffer[i].CreateView();
			}
			if (lightPlaneVertices != null) lightPlaneVertices.Dispose();
			if (lightPlaneInputLayout != InputLayoutHandle.NULL) {
				InteropUtils.CallNative(NativeMethods.ResourceFactory_ReleaseResource, (ResourceHandle) lightPlaneInputLayout).ThrowOnFailure();
			}

			lightPlaneVertices = lightPlaneBuilder.WithInitialData(new[] {
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
				dlLightVS.Handle,
				inputElements,
				(uint) inputElements.Length,
				(IntPtr) (&outInputLayoutPtr)
			).ThrowOnFailure();

			lightPlaneInputLayout = outInputLayoutPtr;

			SetVSResources();
			SetFSResources();
		}

		private void SetFSResources() {
			if (dlLightFS != null) {
				for (int i = 0; i < DLGeometryPass.NUM_GBUFFER_TEXTURES; ++i) {
					string bindingIdentifier;
					switch (i) {
						case 0: bindingIdentifier = "NormalGB"; break;
						case 1: bindingIdentifier = "DiffuseGB"; break;
						case 2: bindingIdentifier = "SpecularGB"; break;
						case 3: bindingIdentifier = "PositionGB"; break;
						case 4: bindingIdentifier = "EmissiveGB"; break;
						default: throw new InvalidOperationException("Only four gbuffer textures are known.");
					}

					if (gBufferSRVArray[i] != null) {
						fsResPackage.SetValue((ResourceViewBinding) dlLightFS.GetBindingByIdentifier(bindingIdentifier), gBufferSRVArray[i]);
					}

					fsUnbindResPackage.SetValue((ResourceViewBinding) dlLightFS.GetBindingByIdentifier(bindingIdentifier), null);
				}
			}
			if (dlFinalFS != null) {
				finalizationShaderResPackage.SetValue((ResourceViewBinding) dlFinalFS.GetBindingByIdentifier("DiffuseGB"), gBufferSRVArray[1]);
				finalizationShaderUnbindResPackage.SetValue((ResourceViewBinding) dlFinalFS.GetBindingByIdentifier("DiffuseGB"), null);
			}
			if (outliningShader != null) {
				outliningShaderResPackage.SetValue((ResourceViewBinding) outliningShader.GetBindingByIdentifier("NormalGB"), gBufferSRVArray[0]);
				outliningShaderUnbindResPackage.SetValue((ResourceViewBinding) outliningShader.GetBindingByIdentifier("NormalGB"), null);
			}
			if (bloomHShader != null) {
				bloomHShaderResPackage.SetValue((ResourceViewBinding) bloomHShader.GetBindingByIdentifier("PreBloomImage"), reducedBloomBufferSRV);
				bloomHShaderUnbindResPackage.SetValue((ResourceViewBinding) bloomHShader.GetBindingByIdentifier("PreBloomImage"), null);
			}
			if (bloomVShader != null) {
				bloomVShaderResPackage.SetValue((ResourceViewBinding) bloomVShader.GetBindingByIdentifier("PreBloomImage"), reducedBloomBufferSRV);
				bloomVShaderUnbindResPackage.SetValue((ResourceViewBinding) bloomVShader.GetBindingByIdentifier("PreBloomImage"), null);
			}
			if (copyShader != null) {
				copyShaderResPackage.SetValue((ResourceViewBinding) copyShader.GetBindingByIdentifier("SourceTex"), preBloomBufferSRV);
				copyShaderUnbindResPackage.SetValue((ResourceViewBinding) copyShader.GetBindingByIdentifier("SourceTex"), null);
				copyDoFShaderResPackage.SetValue((ResourceViewBinding) copyShader.GetBindingByIdentifier("SourceTex"), nonDepthOfFieldBackBufferSRV);
				copyDoFShaderUnbindResPackage.SetValue((ResourceViewBinding) copyShader.GetBindingByIdentifier("SourceTex"), null);
				copyShader.GetBindingByIdentifier("SourceSampler").Bind(copySSO);
			}
			if (copyReverseShader != null) {
				copyReverseShaderResPackage.SetValue((ResourceViewBinding) copyReverseShader.GetBindingByIdentifier("SourceTex"), bloomTargetBufferSRV);
				copyReverseShaderUnbindResPackage.SetValue((ResourceViewBinding) copyReverseShader.GetBindingByIdentifier("SourceTex"), null);
				copyReverseShader.GetBindingByIdentifier("SourceSampler").Bind(copySSO);
			}
			if (blurShader != null) {
				blurShaderResPackage.SetValue((ResourceViewBinding) blurShader.GetBindingByIdentifier("UnblurredScene"), reducedNonDepthOfFieldBackBufferSRV);
				blurShaderUnbindResPackage.SetValue((ResourceViewBinding) blurShader.GetBindingByIdentifier("UnblurredScene"), null);
			}
			if (dofShader != null) {
				dofShaderResPackage.SetValue((ResourceViewBinding) dofShader.GetBindingByIdentifier("UnblurredScene"), nonDepthOfFieldBackBufferSRV);
				dofShaderResPackage.SetValue((ResourceViewBinding) dofShader.GetBindingByIdentifier("BlurredScene"), depthOfFieldBackBufferSRV);
				dofShaderUnbindResPackage.SetValue((ResourceViewBinding) dofShader.GetBindingByIdentifier("UnblurredScene"), null);
				dofShaderUnbindResPackage.SetValue((ResourceViewBinding) dofShader.GetBindingByIdentifier("BlurredScene"), null);
				dofShader.GetBindingByIdentifier("SourceSampler").Bind(copySSO);
			}
		}

		private void SetVSResources() {
			if (dlLightVS != null && lightPlaneVertices != null) {
				vsResPackage.SetValue((VertexInputBinding) dlLightVS.GetBindingByIdentifier("POSITION"), lightPlaneVertices);
				lightVSScalarsBufferBinding = ((ConstantBufferBinding) dlLightVS.GetBindingByIdentifier("Scalars"));
			}
		}

		private void RecalculateLightTileOffsets() {
			if (perTileLightPropsWorkspace.Count < LIGHTING_TILE_GRANULARITY * LIGHTING_TILE_GRANULARITY) {
				perTileLightPropsWorkspace.Clear();
				for (int i = 0; i < LIGHTING_TILE_GRANULARITY * LIGHTING_TILE_GRANULARITY; ++i) {
					perTileLightPropsWorkspace.Add(new LightProperties[MAX_DYNAMIC_LIGHTS]);
				}
			}

			tileOffsetsY[0] = tileOffsetsX[0] = 0f;
			tileOffsetsY[tileOffsetsY.Length - 1] = tileOffsetsX[tileOffsetsX.Length - 1] = 1f;
			for (int i = 1; i < tileOffsetsX.Length - 1; ++i) {
				tileOffsetsY[i] = tileOffsetsX[i] = (1f / LIGHTING_TILE_GRANULARITY) * i;
			}
		}
	}
}