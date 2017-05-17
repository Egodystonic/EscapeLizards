// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 25 02 2015 at 19:55 by Ben Bowen

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap.Rendering {
	[TestClass]
	public class RenderCommandTest {
		#region Tests
		[TestMethod]
		public void TestCtor() {
			RenderCommand testCommand = new RenderCommand(RenderCommandInstruction.FinishCommandList, 1, 2, 3);
			Assert.AreEqual(RenderCommandInstruction.FinishCommandList, testCommand.Instruction);
			Assert.AreEqual((RenderCommandArgument) 1, testCommand.Arg1);
			Assert.AreEqual((RenderCommandArgument) 2, testCommand.Arg2);
			Assert.AreEqual((RenderCommandArgument) 3, testCommand.Arg3);
		}

		[TestMethod]
		public void TestSetPrimitiveTopology() {
			RenderCommand testCommand = RenderCommand.SetPrimitiveTopology(RenderCommand.DEFAULT_PRIMITIVE_TOPOLOGY);
			Assert.AreEqual(RenderCommandInstruction.SetPrimitiveTopology, testCommand.Instruction);
			Assert.AreEqual((RenderCommandArgument) (int) RenderCommand.DEFAULT_PRIMITIVE_TOPOLOGY, testCommand.Arg1);
		}

		[TestMethod]
		public void TestSetInputLayout() {
			GeometryCacheBuilder<DefaultVertex> gcb = new GeometryCacheBuilder<DefaultVertex>();
			gcb.AddModel("TSIL_a", new DefaultVertex[] { new DefaultVertex(Vector3.ONE) }, new uint[] { 0U });
			GeometryCache cache = gcb.Build();
			ConstantBuffer<Matrix> vpMat = BufferFactory.NewConstantBuffer<Matrix>().WithUsage(ResourceUsage.DiscardWrite);
			VertexShader shader = new VertexShader(
				@"Tests\SimpleVS.cso",
				new VertexInputBinding(0U, "INSTANCE_TRANSFORM"),
				new ConstantBufferBinding(0U, "CameraTransform", vpMat),
				new VertexInputBinding(1U, "POSITION")
			);

			GeometryInputLayout gil = cache.GetInputLayout(shader);

			RenderCommand testCommand = RenderCommand.SetInputLayout(gil);
			Assert.AreEqual(RenderCommandInstruction.SetInputLayout, testCommand.Instruction);
			Assert.AreEqual((RenderCommandArgument) (IntPtr) gil.ResourceHandle, testCommand.Arg1);

#if !DEVELOPMENT && !RELEASE
			try {
				RenderCommand.SetInputLayout(null);
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
#endif

			shader.Dispose();
			vpMat.Dispose();
			cache.Dispose();
			gil.Dispose();

#if !DEVELOPMENT && !RELEASE
			try {
				RenderCommand.SetInputLayout(gil);
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
#endif
		}

		[TestMethod]
		public void TestSetRasterizerState() {
			RasterizerState rs = new RasterizerState(true, TriangleCullMode.FrontfaceCulling, true);

			RenderCommand testCommand = RenderCommand.SetRasterizerState(rs);
			Assert.AreEqual(RenderCommandInstruction.SetRSState, testCommand.Instruction);
			Assert.AreEqual((RenderCommandArgument) (IntPtr) rs.ResourceHandle, testCommand.Arg1);

#if !DEVELOPMENT && !RELEASE
			try {
				RenderCommand.SetRasterizerState(null);
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
#endif

			rs.Dispose();

#if !DEVELOPMENT && !RELEASE
			try {
				RenderCommand.SetRasterizerState(rs);
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
#endif
		}

		[TestMethod]
		public void TestSetDepthStencilState() {
			DepthStencilState ds = new DepthStencilState(true);

			RenderCommand testCommand = RenderCommand.SetDepthStencilState(ds);
			Assert.AreEqual(RenderCommandInstruction.SetDSState, testCommand.Instruction);
			Assert.AreEqual((RenderCommandArgument) (IntPtr) ds.ResourceHandle, testCommand.Arg1);

#if !DEVELOPMENT && !RELEASE
			try {
				RenderCommand.SetDepthStencilState(null);
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
#endif

			ds.Dispose();

#if !DEVELOPMENT && !RELEASE
			try {
				RenderCommand.SetDepthStencilState(ds);
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
#endif
		}

		[TestMethod]
		public void TestSetViewport() {
			Window window = new Window("Test Window");

			RenderCommand testCommand = RenderCommand.SetViewport(window);
			Assert.AreEqual(RenderCommandInstruction.SetViewport, testCommand.Instruction);
			Assert.AreEqual((RenderCommandArgument) (IntPtr) window.AddedViewports.ElementAt(0).ViewportHandle, testCommand.Arg1);


#if !DEVELOPMENT && !RELEASE
			try {
				RenderCommand.SetViewport(null);
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
#endif

			window.Close();
		}

		[TestMethod]
		public void TestSetShader() {
			ConstantBuffer<Matrix> vpMat = BufferFactory.NewConstantBuffer<Matrix>().WithUsage(ResourceUsage.DiscardWrite);
			VertexShader vs = VertexShader.NewDefaultShader(vpMat);

			ConstantBuffer<Vector4> colorVec = BufferFactory.NewConstantBuffer<Vector4>().WithUsage(ResourceUsage.DiscardWrite);
			FragmentShader fs = new FragmentShader(@"Tests\SimpleFS.cso", new ConstantBufferBinding(0U, "MaterialProperties", colorVec));

			RenderCommand testCommand = RenderCommand.SetShader(vs);
			Assert.AreEqual(RenderCommandInstruction.VSSetShader, testCommand.Instruction);
			Assert.AreEqual((RenderCommandArgument) (IntPtr) vs.Handle, testCommand.Arg1);

			testCommand = RenderCommand.SetShader(fs);
			Assert.AreEqual(RenderCommandInstruction.FSSetShader, testCommand.Instruction);
			Assert.AreEqual((RenderCommandArgument) (IntPtr) fs.Handle, testCommand.Arg1);

#if !DEVELOPMENT && !RELEASE
			try {
				RenderCommand.SetShader(null);
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
#endif

			vs.Dispose();
			fs.Dispose();
			vpMat.Dispose();
			colorVec.Dispose();

#if !DEVELOPMENT && !RELEASE
			try {
				RenderCommand.SetShader(fs);
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
#endif
		}

		[TestMethod]
		public unsafe void TestSetRenderTargets() {
			Texture2DArray<TexelFormat.RenderTarget> backBufferArray = TextureFactory.NewTexture2D<TexelFormat.RenderTarget>()
				.WithWidth(800U)
				.WithHeight(600U)
				.WithDynamicDetail(false)
				.WithMipAllocation(false)
				.WithMipGenerationTarget(false)
				.WithMultisampling(false)
				.WithPermittedBindings(GPUBindings.RenderTarget)
				.WithUsage(ResourceUsage.Write)
				.CreateArray(RenderCommand.MAX_RENDER_TARGETS + 1U);

			Texture2D<TexelFormat.DepthStencil> depthStencil = backBufferArray.Clone()
				.WithTexelFormat<TexelFormat.DepthStencil>()
				.WithPermittedBindings(GPUBindings.DepthStencilTarget);

			RenderTargetView[] rtvArr = backBufferArray.Select(tex => tex.CreateRenderTargetView(0U)).ToArray();
			DepthStencilView dsv = depthStencil.CreateDepthStencilView(0U);

			RenderCommand testCommand = RenderCommand.SetRenderTargets(dsv, rtvArr.Take((int) RenderCommand.MAX_RENDER_TARGETS).ToArray());
			Assert.AreEqual(RenderCommandInstruction.SetRenderTargets, testCommand.Instruction);
			RenderTargetViewHandle* rtvArrPtr
				= (RenderTargetViewHandle*) new IntPtr(UnsafeUtils.Reinterpret<RenderCommandArgument, long>(testCommand.Arg1, sizeof(long)));
			for (int i = 0; i < RenderCommand.MAX_RENDER_TARGETS; ++i) {
				Assert.AreEqual(rtvArr[i].ResourceViewHandle, rtvArrPtr[i]);
			}
			Assert.AreEqual(
				dsv.ResourceViewHandle, 
				UnsafeUtils.Reinterpret<IntPtr, DepthStencilViewHandle>(new IntPtr(UnsafeUtils.Reinterpret<RenderCommandArgument, long>(testCommand.Arg2, sizeof(long))), sizeof(DepthStencilViewHandle))
			);
			Assert.AreEqual((RenderCommandArgument) RenderCommand.MAX_RENDER_TARGETS, testCommand.Arg3);

#if !DEVELOPMENT && !RELEASE
			try {
				RenderCommand.SetRenderTargets(null as DepthStencilView, rtvArr.Take((int) RenderCommand.MAX_RENDER_TARGETS).ToArray());
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }

			try {
				RenderCommand.SetRenderTargets(dsv, null);
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }

			try {
				RenderCommand.SetRenderTargets(dsv, rtvArr[0], rtvArr[1], null, rtvArr[2]);
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }

			try {
				RenderCommand.SetRenderTargets(dsv, rtvArr);
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
#endif

			rtvArr.ForEach(rtv => rtv.Dispose());
			dsv.Dispose();
			backBufferArray.Dispose();
			depthStencil.Dispose();

#if !DEVELOPMENT && !RELEASE
			try {
				RenderCommand.SetRenderTargets(dsv, rtvArr.Take((int) RenderCommand.MAX_RENDER_TARGETS).ToArray());
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
#endif
		}

		[TestMethod]
		public unsafe void TestSetRenderTargetsWithWindow() {
			Window renderTargetWindow = new Window("Test window");

			RenderTargetViewHandle outRTV;
			DepthStencilViewHandle outDSV;

			renderTargetWindow.GetWindowRTVAndDSV(out outRTV, out outDSV);

			RenderCommand testCommand = RenderCommand.SetRenderTargets(renderTargetWindow);
			Assert.AreEqual(RenderCommandInstruction.SetRenderTargets, testCommand.Instruction);
			Assert.AreEqual(
				outRTV, 
				((RenderTargetViewHandle*) new IntPtr(UnsafeUtils.Reinterpret<RenderCommandArgument, long>(testCommand.Arg1, sizeof(long))))[0]
			);
			Assert.AreEqual((RenderCommandArgument) (IntPtr) (ResourceViewHandle) outDSV, testCommand.Arg2);
			Assert.AreEqual((RenderCommandArgument) 1U, testCommand.Arg3);

			renderTargetWindow.Close();

			LosgapSystem.InvokeOnMaster(() => { }); // Wait for the window to be closed

			testCommand = RenderCommand.SetRenderTargets(renderTargetWindow);
			Assert.AreEqual(RenderCommandInstruction.NoOperation, testCommand.Instruction);

#if !DEVELOPMENT && !RELEASE
			try {
				RenderCommand.SetRenderTargets(null as Window);
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
#endif
		}

		[TestMethod]
		public unsafe void TestDrawIndexedInstanced() {
			RenderCommand testCommand = RenderCommand.DrawIndexedInstanced(1, 2U, 3U, 4U, 5U);
			Assert.AreEqual(RenderCommandInstruction.DrawIndexedInstanced, testCommand.Instruction);
			Assert.AreEqual((RenderCommandArgument) 1, testCommand.Arg1);
			
			ulong arg2AsUlong = UnsafeUtils.Reinterpret<RenderCommandArgument, ulong>(testCommand.Arg2, sizeof(ulong));
			uint* arg23Ptr = (uint*) &arg2AsUlong;
			Assert.AreEqual(2U, arg23Ptr[0]);
			Assert.AreEqual(3U, arg23Ptr[1]);

			ulong arg3AsUlong = UnsafeUtils.Reinterpret<RenderCommandArgument, ulong>(testCommand.Arg3, sizeof(ulong));
			uint* arg45Ptr = (uint*) &arg3AsUlong;
			Assert.AreEqual(4U, arg45Ptr[0]);
			Assert.AreEqual(5U, arg45Ptr[1]);
		}

		[TestMethod]
		public void TestClearRenderTarget() {
			Texture2D<TexelFormat.RenderTarget> renderTarget = TextureFactory.NewTexture2D<TexelFormat.RenderTarget>()
				.WithWidth(800U)
				.WithHeight(600U)
				.WithDynamicDetail(false)
				.WithMipAllocation(false)
				.WithMipGenerationTarget(false)
				.WithMultisampling(false)
				.WithPermittedBindings(GPUBindings.RenderTarget)
				.WithUsage(ResourceUsage.Write);

			RenderTargetView rtv = renderTarget.CreateRenderTargetView(0U);

			RenderCommand testCommand = RenderCommand.ClearRenderTarget(rtv);
			Assert.AreEqual(RenderCommandInstruction.ClearRenderTarget, testCommand.Instruction);
			Assert.AreEqual((RenderCommandArgument) (IntPtr) (ResourceViewHandle) rtv.ResourceViewHandle, testCommand.Arg1);

#if !DEVELOPMENT && !RELEASE
			try {
				RenderCommand.ClearRenderTarget(null as RenderTargetView);
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
#endif

			rtv.Dispose();
			renderTarget.Dispose();

#if !DEVELOPMENT && !RELEASE
			try {
				RenderCommand.ClearRenderTarget(rtv);
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
#endif
		}

		[TestMethod]
		public unsafe void TestClearRenderTargetWithWindow() {
			Window renderTargetWindow = new Window("Test window");

			RenderTargetViewHandle outRTV;
			DepthStencilViewHandle outDSV;

			renderTargetWindow.GetWindowRTVAndDSV(out outRTV, out outDSV);

			RenderCommand testCommand = RenderCommand.ClearRenderTarget(renderTargetWindow);
			Assert.AreEqual(RenderCommandInstruction.ClearRenderTarget, testCommand.Instruction);
			Assert.AreEqual(
				outRTV,
				UnsafeUtils.Reinterpret<IntPtr, RenderTargetViewHandle>(new IntPtr(UnsafeUtils.Reinterpret<RenderCommandArgument, long>(testCommand.Arg1, sizeof(long))), sizeof(RenderTargetViewHandle))
			);

			renderTargetWindow.Close();

			LosgapSystem.InvokeOnMaster(() => { }); // Wait for the window to be closed

			testCommand = RenderCommand.ClearRenderTarget(renderTargetWindow);
			Assert.AreEqual(RenderCommandInstruction.NoOperation, testCommand.Instruction);

#if !DEVELOPMENT && !RELEASE
			try {
				RenderCommand.ClearRenderTarget(null as Window);
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
#endif
		}


		[TestMethod]
		public void TestClearDepthStencil() {
			Texture2D<TexelFormat.DepthStencil> depthStencilBuffer = TextureFactory.NewTexture2D<TexelFormat.DepthStencil>()
				.WithWidth(800U)
				.WithHeight(600U)
				.WithDynamicDetail(false)
				.WithMipAllocation(false)
				.WithMipGenerationTarget(false)
				.WithMultisampling(false)
				.WithPermittedBindings(GPUBindings.DepthStencilTarget)
				.WithUsage(ResourceUsage.Write);

			DepthStencilView dsv = depthStencilBuffer.CreateDepthStencilView(0U);

			RenderCommand testCommand = RenderCommand.ClearDepthStencil(dsv);
			Assert.AreEqual(RenderCommandInstruction.ClearDepthStencil, testCommand.Instruction);
			Assert.AreEqual((RenderCommandArgument) (IntPtr) (ResourceViewHandle) dsv.ResourceViewHandle, testCommand.Arg1);

#if !DEVELOPMENT && !RELEASE
			try {
				RenderCommand.ClearDepthStencil(null as DepthStencilView);
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
#endif

			dsv.Dispose();
			depthStencilBuffer.Dispose();

#if !DEVELOPMENT && !RELEASE
			try {
				RenderCommand.ClearDepthStencil(dsv);
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
#endif
		}

		[TestMethod]
		public unsafe void TestDepthStencilTargetWithWindow() {
			Window depthStencilWindow = new Window("Test window");

			RenderTargetViewHandle outRTV;
			DepthStencilViewHandle outDSV;

			depthStencilWindow.GetWindowRTVAndDSV(out outRTV, out outDSV);

			RenderCommand testCommand = RenderCommand.ClearDepthStencil(depthStencilWindow);
			Assert.AreEqual(RenderCommandInstruction.ClearDepthStencil, testCommand.Instruction);
			Assert.AreEqual(
				outDSV,
				UnsafeUtils.Reinterpret<IntPtr, DepthStencilViewHandle>(new IntPtr(UnsafeUtils.Reinterpret<RenderCommandArgument, long>(testCommand.Arg1, sizeof(long))), sizeof(DepthStencilViewHandle))
			);

			depthStencilWindow.Close();

			LosgapSystem.InvokeOnMaster(() => { }); // Wait for the window to be closed

			testCommand = RenderCommand.ClearDepthStencil(depthStencilWindow);
			Assert.AreEqual(RenderCommandInstruction.NoOperation, testCommand.Instruction);

#if !DEVELOPMENT && !RELEASE
			try {
				RenderCommand.ClearDepthStencil(null as Window);
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
#endif
		}

		[TestMethod]
		public void TestSetInstanceBuffer() {
			const uint INPUT_SLOT = 3U;
			VertexBuffer<Matrix> instanceBuffer = BufferFactory.NewVertexBuffer<Matrix>()
				.WithLength(10U)
				.WithUsage(ResourceUsage.DiscardWrite);

			RenderCommand testCommand = RenderCommand.SetInstanceBuffer(instanceBuffer, INPUT_SLOT);
			Assert.AreEqual(RenderCommandInstruction.SetInstanceBuffer, testCommand.Instruction);
			Assert.AreEqual((RenderCommandArgument) (IntPtr) instanceBuffer.ResourceHandle, testCommand.Arg1);
			Assert.AreEqual((RenderCommandArgument) INPUT_SLOT, testCommand.Arg2);

			instanceBuffer.Dispose();

#if !DEVELOPMENT && !RELEASE
			try {
				RenderCommand.SetInstanceBuffer(null, INPUT_SLOT);
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }

			try {
				RenderCommand.SetInstanceBuffer(instanceBuffer, INPUT_SLOT);
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
#endif
		}

		[TestMethod]
		public void TestSetIndexBuffer() {
			IndexBuffer ib = BufferFactory.NewIndexBuffer().WithLength(300U).WithUsage(ResourceUsage.DiscardWrite);

			RenderCommand testCommand = RenderCommand.SetIndexBuffer(ib);
			Assert.AreEqual(RenderCommandInstruction.SetIndexBuffer, testCommand.Instruction);
			Assert.AreEqual((RenderCommandArgument) (IntPtr) ib.ResourceHandle, testCommand.Arg1);

			ib.Dispose();

#if !DEVELOPMENT && !RELEASE
			try {
				RenderCommand.SetIndexBuffer(null);
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }

			try {
				RenderCommand.SetIndexBuffer(ib);
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
#endif
		}

		[TestMethod]
		public unsafe void SetShaderConstantBuffers() {
			ConstantBuffer<Vector4> cb0 = BufferFactory.NewConstantBuffer<Vector4>().WithUsage(ResourceUsage.DiscardWrite);
			ConstantBuffer<Matrix> cb1 = BufferFactory.NewConstantBuffer<Matrix>().WithUsage(ResourceUsage.DiscardWrite);

			Shader shader = new FragmentShader(
				@"Tests\SimpleFS.cso",
				new ConstantBufferBinding(0U, "CB0", cb0),
				new ConstantBufferBinding(1U, "CB1", cb1)
			);

			RenderCommand testCommand = RenderCommand.SetShaderConstantBuffers(shader);
			Assert.AreEqual(RenderCommandInstruction.FSSetCBuffers, testCommand.Instruction);
			ResourceHandle* resHandleArray = (ResourceHandle*) new IntPtr(UnsafeUtils.Reinterpret<RenderCommandArgument, long>(testCommand.Arg1, sizeof(long)));
			Assert.AreEqual(cb0.ResourceHandle, resHandleArray[0]);
			Assert.AreEqual(cb1.ResourceHandle, resHandleArray[1]);
			Assert.AreEqual((RenderCommandArgument) shader.NumConstantBufferSlots, testCommand.Arg2);

			shader.Dispose();
			cb1.Dispose();
			cb0.Dispose();

#if !DEVELOPMENT && !RELEASE
			try {
				RenderCommand.SetShaderConstantBuffers(null);
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }

			try {
				RenderCommand.SetShaderConstantBuffers(shader);
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
#endif
		}

		[TestMethod]
		public unsafe void TestSetShaderTextureSamplers() {
			TextureSampler ts0 = new TextureSampler(TextureFilterType.Anisotropic, TextureWrapMode.Border, AnisotropicFilteringLevel.EightTimes);
			TextureSampler ts2 = new TextureSampler(TextureFilterType.Anisotropic, TextureWrapMode.Border, AnisotropicFilteringLevel.EightTimes);
			
			Shader shader = new FragmentShader(
				@"Tests\SimpleFS.cso",
				new TextureSamplerBinding(0U, "TS0"),
				new TextureSamplerBinding(1U, "TS1"),
				new TextureSamplerBinding(2U, "TS2")
			);

			Dictionary<TextureSamplerBinding, TextureSampler> tsDict = new Dictionary<TextureSamplerBinding, TextureSampler>();
			tsDict[shader.TextureSamplerBindings[0]] = ts0;
			tsDict[shader.TextureSamplerBindings[2]] = ts2;

			RenderCommand testCommand = RenderCommand.SetShaderTextureSamplers(shader, tsDict);
			Assert.AreEqual(RenderCommandInstruction.FSSetSamplers, testCommand.Instruction);
			ResourceHandle* resHandleArray = (ResourceHandle*) new IntPtr(UnsafeUtils.Reinterpret<RenderCommandArgument, long>(testCommand.Arg1, sizeof(long)));
			Assert.AreEqual(ts0.ResourceHandle, resHandleArray[0]);
			Assert.AreEqual(ResourceHandle.NULL, resHandleArray[1]);
			Assert.AreEqual(ts2.ResourceHandle, resHandleArray[2]);
			Assert.AreEqual((RenderCommandArgument) 3U, testCommand.Arg2);

#if !DEVELOPMENT && !RELEASE
			try {
				RenderCommand.SetShaderTextureSamplers(null, tsDict);
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }

			try {
				RenderCommand.SetShaderTextureSamplers(shader, null);
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
#endif

			ts0.Dispose();
			ts2.Dispose();

#if !DEVELOPMENT && !RELEASE
			try {
				RenderCommand.SetShaderTextureSamplers(shader, tsDict);
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
#endif

			shader.Dispose();

#if !DEVELOPMENT && !RELEASE
			try {
				RenderCommand.SetShaderTextureSamplers(shader, new Dictionary<TextureSamplerBinding, TextureSampler>());
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
#endif
		}

		[TestMethod]
		public unsafe void TestSetShaderResourceViews() {
			Texture2DBuilder<TexelFormat.RGBA32UInt> texBuilder = TextureFactory.NewTexture2D<TexelFormat.RGBA32UInt>()
				.WithWidth(100U)
				.WithHeight(100U)
				.WithUsage(ResourceUsage.DiscardWrite);
			Texture2D<TexelFormat.RGBA32UInt> tex0 = texBuilder.Create();
			Texture2D<TexelFormat.RGBA32UInt> tex2 = texBuilder.Create();

			BaseResourceView rv0 = tex0.CreateView();
			BaseResourceView rv2 = tex2.CreateView();

			Shader shader = new FragmentShader(
				@"Tests\SimpleFS.cso",
				new ResourceViewBinding(0U, "RV0"),
				new ResourceViewBinding(1U, "RV1"),
				new ResourceViewBinding(2U, "RV2")
			);

			Dictionary<ResourceViewBinding, BaseResourceView> rvDict = new Dictionary<ResourceViewBinding, BaseResourceView>();
			rvDict[shader.ResourceViewBindings[0]] = rv0;
			rvDict[shader.ResourceViewBindings[2]] = rv2;

			RenderCommand testCommand = RenderCommand.SetShaderResourceViews(shader, rvDict);
			Assert.AreEqual(RenderCommandInstruction.FSSetResources, testCommand.Instruction);
			ResourceViewHandle* resHandleArray = (ResourceViewHandle*) new IntPtr(UnsafeUtils.Reinterpret<RenderCommandArgument, long>(testCommand.Arg1, sizeof(long)));
			Assert.AreEqual(rv0.ResourceViewHandle, resHandleArray[0]);
			Assert.AreEqual(ResourceViewHandle.NULL, resHandleArray[1]);
			Assert.AreEqual(rv2.ResourceViewHandle, resHandleArray[2]);
			Assert.AreEqual((RenderCommandArgument) 3U, testCommand.Arg2);

#if !DEVELOPMENT && !RELEASE
			try {
				RenderCommand.SetShaderResourceViews(null, rvDict);
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }

			try {
				RenderCommand.SetShaderResourceViews(shader, null);
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
#endif

			tex0.Dispose();
			tex2.Dispose();
			rv0.Dispose();
			rv2.Dispose();

#if !DEVELOPMENT && !RELEASE
			try {
				RenderCommand.SetShaderResourceViews(shader, rvDict);
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
#endif

			shader.Dispose();

#if !DEVELOPMENT && !RELEASE
			try {
				RenderCommand.SetShaderResourceViews(shader, new Dictionary<ResourceViewBinding, BaseResourceView>());
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
#endif
		}

		[TestMethod]
		public unsafe void TestSetShaderVertexBuffers() {
			VertexBufferBuilder<Vector3> vbBuilder = BufferFactory.NewVertexBuffer<Vector3>()
				.WithLength(100U)
				.WithUsage(ResourceUsage.DiscardWrite);
			VertexBuffer<Vector3> vb0 = vbBuilder.Create();
			VertexBuffer<Vector2> vb2 = vbBuilder.WithVertexType<Vector2>().Create();

			ConstantBuffer<Matrix> vpTransBuffer = BufferFactory.NewConstantBuffer<Matrix>().WithUsage(ResourceUsage.DiscardWrite);

			VertexShader shader = new VertexShader(
				@"Tests\SimpleVS.cso",
				new VertexInputBinding(8U, "Instance"), 
				new ConstantBufferBinding(0U, "VPTB", vpTransBuffer),
				new VertexInputBinding(0U, "VB0"),
				new VertexInputBinding(1U, "VB1"),
				new VertexInputBinding(2U, "VB2")
			);

			Dictionary<VertexInputBinding, IVertexBuffer> vbDict = new Dictionary<VertexInputBinding, IVertexBuffer>();
			vbDict[shader.InputBindings[0]] = vb0;
			vbDict[shader.InputBindings[2]] = vb2;

			RenderCommand testCommand = RenderCommand.SetShaderVertexBuffers(shader, vbDict);
			Assert.AreEqual(RenderCommandInstruction.SetVertexBuffers, testCommand.Instruction);
			ResourceHandle* resHandleArray = (ResourceHandle*) new IntPtr(UnsafeUtils.Reinterpret<RenderCommandArgument, long>(testCommand.Arg1, sizeof(long)));
			Assert.AreEqual(vb0.ResourceHandle, resHandleArray[0]);
			Assert.AreEqual(ResourceHandle.NULL, resHandleArray[1]);
			Assert.AreEqual(vb2.ResourceHandle, resHandleArray[2]);
			uint* bufferStrideArray = (uint*) new IntPtr(UnsafeUtils.Reinterpret<RenderCommandArgument, long>(testCommand.Arg2, sizeof(long)));
			Assert.AreEqual((uint) sizeof(Vector3), bufferStrideArray[0]);
			Assert.AreEqual(0U, bufferStrideArray[1]);
			Assert.AreEqual((uint) sizeof(Vector2), bufferStrideArray[2]);
			Assert.AreEqual((RenderCommandArgument) 9U, testCommand.Arg3);

#if !DEVELOPMENT && !RELEASE
			try {
				RenderCommand.SetShaderVertexBuffers(null, vbDict);
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }

			try {
				RenderCommand.SetShaderVertexBuffers(shader, null);
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
#endif

			vb0.Dispose();
			vb2.Dispose();
			vpTransBuffer.Dispose();
			vpTransBuffer.Dispose();

#if !DEVELOPMENT && !RELEASE
			try {
				RenderCommand.SetShaderVertexBuffers(shader, vbDict);
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
#endif

			shader.Dispose();

#if !DEVELOPMENT && !RELEASE
			try {
				RenderCommand.SetShaderVertexBuffers(shader, new Dictionary<VertexInputBinding, IVertexBuffer>());
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
#endif
		}

		[TestMethod]
		public unsafe void TestDiscardWriteShaderConstantBuffer() {
			ConstantBuffer<Vector4> cb = BufferFactory.NewConstantBuffer<Vector4>().WithUsage(ResourceUsage.DiscardWrite);
			ConstantBufferBinding cbb = new ConstantBufferBinding(0U, "CB0", cb);
			Vector4 initialValue = Vector4.FORWARD;
			cbb.SetValue((byte*) (&initialValue));

			RenderCommand testCommand = RenderCommand.DiscardWriteShaderConstantBuffer(cbb, cbb.CurValuePtr);
			Assert.AreEqual(RenderCommandInstruction.CBDiscardWrite, testCommand.Instruction);
			Assert.AreEqual((RenderCommandArgument) (IntPtr) cbb.GetBoundResource().ResourceHandle, testCommand.Arg1);
			Assert.AreEqual(*((Vector4*) cbb.CurValuePtr), *((Vector4*) new IntPtr(UnsafeUtils.Reinterpret<RenderCommandArgument, long>(testCommand.Arg2, sizeof(long)))));
			Assert.AreEqual((RenderCommandArgument) cbb.BufferSizeBytes, testCommand.Arg3);

#if !DEVELOPMENT && !RELEASE
			try {
				RenderCommand.DiscardWriteShaderConstantBuffer(null, cbb.CurValuePtr);
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }

			try {
				RenderCommand.DiscardWriteShaderConstantBuffer(cbb, IntPtr.Zero);
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
#endif

			(cbb as IDisposable).Dispose();

#if !DEVELOPMENT && !RELEASE
			try {
				RenderCommand.DiscardWriteShaderConstantBuffer(cbb, cbb.CurValuePtr);
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
#endif

			cb.Dispose();
		}
		#endregion
	}
}