// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 22 02 2015 at 18:26 by Ben Bowen

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap.Rendering {
	[TestClass]
	public class ResourceViewTest {

		[TestInitialize]
		public void SetUp() { }


		#region Tests
		[TestMethod]
		public void TestCreateDefaultViewForBuffers() {
			const uint TEST_BUFFER_LEN = 300U;
			IGeneralBuffer testResource;
			ShaderResourceView testSRV;

			testResource = BufferFactory.NewBuffer<int>().WithLength(TEST_BUFFER_LEN)
				.WithUsage(ResourceUsage.DiscardWrite).WithPermittedBindings(GPUBindings.ReadableShaderResource).Create();
			testSRV = testResource.CreateView();
			Assert.IsFalse(testSRV.ResourceOrViewDisposed);
			Assert.AreEqual(testResource, testSRV.Resource);
			Assert.AreEqual(0U, ((ShaderBufferResourceView) testSRV).FirstElementIndex);
			Assert.AreEqual(TEST_BUFFER_LEN, ((ShaderBufferResourceView) testSRV).NumElements);
			testResource.Dispose();
			Assert.IsTrue(testSRV.ResourceOrViewDisposed);
			testSRV.Dispose();
			Assert.IsTrue(testSRV.IsDisposed);

			testResource = BufferFactory.NewBuffer<Matrix>().WithLength(TEST_BUFFER_LEN)
				.WithUsage(ResourceUsage.DiscardWrite).WithPermittedBindings(GPUBindings.ReadableShaderResource).Create();
			testSRV = testResource.CreateView();
			Assert.IsFalse(testSRV.ResourceOrViewDisposed);
			Assert.AreEqual(testResource, testSRV.Resource);
			Assert.AreEqual(0U, ((ShaderBufferResourceView) testSRV).FirstElementIndex);
			Assert.AreEqual(TEST_BUFFER_LEN, ((ShaderBufferResourceView) testSRV).NumElements);
			testResource.Dispose();
			Assert.IsTrue(testSRV.ResourceOrViewDisposed);
			testSRV.Dispose();
			Assert.IsTrue(testSRV.IsDisposed);
		}

		[TestMethod]
		public void TestCreateDefaultViewForTextures() {
			const uint TEST_TEXTURE_WIDTH = 128U;
			const uint TEST_TEXTURE_HEIGHT = 64U;
			const uint TEST_TEXTURE_DEPTH = 32U;
			const uint TEST_ARRAY_LEN = 10U;
			IResource testResource;
			ShaderResourceView testSRV;

			testResource = TextureFactory.NewTexture1D<TexelFormat.RGB32Float>()
				.WithDynamicDetail(false)
				.WithMipAllocation(true)
				.WithMipGenerationTarget(false)
				.WithPermittedBindings(GPUBindings.ReadableShaderResource)
				.WithUsage(ResourceUsage.Write)
				.WithWidth(TEST_TEXTURE_WIDTH)
				.Create();
			testSRV = ((ITexture) testResource).CreateView();
			Assert.IsFalse(testSRV.ResourceOrViewDisposed);
			Assert.AreEqual(testResource, testSRV.Resource);
			Assert.AreEqual(0U, ((ShaderTextureResourceView) testSRV).FirstMipIndex);
			Assert.AreEqual(TextureUtils.GetNumMips(TEST_TEXTURE_WIDTH), ((ShaderTextureResourceView) testSRV).NumMips);
			testResource.Dispose();
			Assert.IsTrue(testSRV.ResourceOrViewDisposed);
			testSRV.Dispose();
			Assert.IsTrue(testSRV.IsDisposed);

			testResource = TextureFactory.NewTexture2D<TexelFormat.RGB32Float>()
				.WithDynamicDetail(false)
				.WithMipAllocation(true)
				.WithMipGenerationTarget(false)
				.WithPermittedBindings(GPUBindings.ReadableShaderResource)
				.WithUsage(ResourceUsage.Write)
				.WithWidth(TEST_TEXTURE_WIDTH)
				.WithHeight(TEST_TEXTURE_HEIGHT)
				.Create();
			testSRV = ((ITexture) testResource).CreateView();
			Assert.IsFalse(testSRV.ResourceOrViewDisposed);
			Assert.AreEqual(testResource, testSRV.Resource);
			Assert.AreEqual(0U, ((ShaderTextureResourceView) testSRV).FirstMipIndex);
			Assert.AreEqual(TextureUtils.GetNumMips(TEST_TEXTURE_WIDTH, TEST_TEXTURE_HEIGHT), ((ShaderTextureResourceView) testSRV).NumMips);
			testResource.Dispose();
			Assert.IsTrue(testSRV.ResourceOrViewDisposed);
			testSRV.Dispose();
			Assert.IsTrue(testSRV.IsDisposed);

			testResource = TextureFactory.NewTexture3D<TexelFormat.RGB32Float>()
				.WithDynamicDetail(false)
				.WithMipAllocation(true)
				.WithMipGenerationTarget(false)
				.WithPermittedBindings(GPUBindings.ReadableShaderResource)
				.WithUsage(ResourceUsage.Write)
				.WithWidth(TEST_TEXTURE_WIDTH)
				.WithHeight(TEST_TEXTURE_HEIGHT)
				.WithDepth(TEST_TEXTURE_DEPTH)
				.Create();
			testSRV = ((ITexture) testResource).CreateView();
			Assert.IsFalse(testSRV.ResourceOrViewDisposed);
			Assert.AreEqual(testResource, testSRV.Resource);
			Assert.AreEqual(0U, ((ShaderTextureResourceView) testSRV).FirstMipIndex);
			Assert.AreEqual(TextureUtils.GetNumMips(TEST_TEXTURE_WIDTH, TEST_TEXTURE_HEIGHT, TEST_TEXTURE_DEPTH), ((ShaderTextureResourceView) testSRV).NumMips);
			testResource.Dispose();
			Assert.IsTrue(testSRV.ResourceOrViewDisposed);
			testSRV.Dispose();
			Assert.IsTrue(testSRV.IsDisposed);

			testResource = TextureFactory.NewTexture1D<TexelFormat.RGB32Float>()
				.WithDynamicDetail(false)
				.WithMipAllocation(true)
				.WithMipGenerationTarget(false)
				.WithPermittedBindings(GPUBindings.ReadableShaderResource)
				.WithUsage(ResourceUsage.Write)
				.WithWidth(TEST_TEXTURE_WIDTH)
				.CreateArray(TEST_ARRAY_LEN);
			testSRV = ((ITextureArray) testResource).CreateView();
			Assert.IsFalse(testSRV.ResourceOrViewDisposed);
			Assert.AreEqual(testResource, testSRV.Resource);
			Assert.AreEqual(0U, ((ShaderTextureArrayResourceView) testSRV).FirstMipIndex);
			Assert.AreEqual(TextureUtils.GetNumMips(TEST_TEXTURE_WIDTH), ((ShaderTextureArrayResourceView) testSRV).NumMips);
			Assert.AreEqual(0U, ((ShaderTextureArrayResourceView) testSRV).FirstArrayElementIndex);
			Assert.AreEqual(TEST_ARRAY_LEN, ((ShaderTextureArrayResourceView) testSRV).NumArrayElements);
			testResource.Dispose();
			Assert.IsTrue(testSRV.ResourceOrViewDisposed);
			testSRV.Dispose();
			Assert.IsTrue(testSRV.IsDisposed);

			testResource = TextureFactory.NewTexture2D<TexelFormat.RGB32Float>()
				.WithDynamicDetail(false)
				.WithMipAllocation(true)
				.WithMipGenerationTarget(false)
				.WithPermittedBindings(GPUBindings.ReadableShaderResource)
				.WithUsage(ResourceUsage.Write)
				.WithWidth(TEST_TEXTURE_WIDTH)
				.WithHeight(TEST_TEXTURE_HEIGHT)
				.CreateArray(TEST_ARRAY_LEN);
			testSRV = ((ITextureArray) testResource).CreateView();
			Assert.IsFalse(testSRV.ResourceOrViewDisposed);
			Assert.AreEqual(testResource, testSRV.Resource);
			Assert.AreEqual(0U, ((ShaderTextureArrayResourceView) testSRV).FirstMipIndex);
			Assert.AreEqual(TextureUtils.GetNumMips(TEST_TEXTURE_WIDTH, TEST_TEXTURE_HEIGHT), ((ShaderTextureArrayResourceView) testSRV).NumMips);
			Assert.AreEqual(0U, ((ShaderTextureArrayResourceView) testSRV).FirstArrayElementIndex);
			Assert.AreEqual(TEST_ARRAY_LEN, ((ShaderTextureArrayResourceView) testSRV).NumArrayElements);
			testResource.Dispose();
			Assert.IsTrue(testSRV.ResourceOrViewDisposed);
			testSRV.Dispose();
			Assert.IsTrue(testSRV.IsDisposed);
		}

		[TestMethod]
		public void TestCustomSRVForBuffers() {
			const uint TEST_BUFFER_LEN = 300U;
			const uint TEST_VIEW_START = 100U;
			const uint TEST_VIEW_LEN = 50U;
			IGeneralBuffer testResource;
			ShaderResourceView testSRV;

			testResource = BufferFactory.NewBuffer<int>().WithLength(TEST_BUFFER_LEN).WithUsage(ResourceUsage.DiscardWrite).Create();
			testSRV = testResource.CreateView(TEST_VIEW_START, TEST_VIEW_LEN);
			Assert.IsFalse(testSRV.ResourceOrViewDisposed);
			Assert.AreEqual(testResource, testSRV.Resource);
			Assert.AreEqual(TEST_VIEW_START, ((ShaderBufferResourceView) testSRV).FirstElementIndex);
			Assert.AreEqual(TEST_VIEW_LEN, ((ShaderBufferResourceView) testSRV).NumElements);
			testResource.Dispose();
			Assert.IsTrue(testSRV.ResourceOrViewDisposed);
			testSRV.Dispose();
			Assert.IsTrue(testSRV.IsDisposed);

			testResource = BufferFactory.NewBuffer<Matrix>().WithLength(TEST_BUFFER_LEN).WithUsage(ResourceUsage.DiscardWrite).Create();
			testSRV = testResource.CreateView(TEST_VIEW_START, TEST_VIEW_LEN);
			Assert.IsFalse(testSRV.ResourceOrViewDisposed);
			Assert.AreEqual(testResource, testSRV.Resource);
			Assert.AreEqual(TEST_VIEW_START, ((ShaderBufferResourceView) testSRV).FirstElementIndex);
			Assert.AreEqual(TEST_VIEW_LEN, ((ShaderBufferResourceView) testSRV).NumElements);
			testResource.Dispose();
			Assert.IsTrue(testSRV.ResourceOrViewDisposed);
			testSRV.Dispose();
			Assert.IsTrue(testSRV.IsDisposed);
		}

		[TestMethod]
		public void TestCustomUAVForBuffers() {
			const uint TEST_BUFFER_LEN = 300U;
			const uint TEST_VIEW_START = 100U;
			const uint TEST_VIEW_LEN = 50U;
			const bool TEST_APPEND_CONSUME = true;
			const bool TEST_INCLUDE_COUNTER = false;
			IGeneralBuffer testResource;
			UnorderedAccessView testUAV;

			testResource = BufferFactory.NewBuffer<int>().WithLength(TEST_BUFFER_LEN)
				.WithUsage(ResourceUsage.Write).WithPermittedBindings(GPUBindings.WritableShaderResource).Create();
			testUAV = testResource.CreateUnorderedAccessView(TEST_VIEW_START, TEST_VIEW_LEN, false, false);
			Assert.IsFalse(testUAV.ResourceOrViewDisposed);
			Assert.AreEqual(testResource, testUAV.Resource);
			Assert.AreEqual(TEST_VIEW_START, ((UnorderedBufferAccessView) testUAV).FirstElementIndex);
			Assert.AreEqual(true, ((UnorderedBufferAccessView) testUAV).IsRawAccessView);
			Assert.AreEqual(TEST_VIEW_LEN, ((UnorderedBufferAccessView) testUAV).NumElements);
			Assert.AreEqual(false, ((UnorderedBufferAccessView) testUAV).AppendConsumeSupport);
			Assert.AreEqual(false, ((UnorderedBufferAccessView) testUAV).IncludesCounter);
			testResource.Dispose();
			Assert.IsTrue(testUAV.ResourceOrViewDisposed);
			testUAV.Dispose();
			Assert.IsTrue(testUAV.IsDisposed);

			testResource = BufferFactory.NewBuffer<Matrix>().WithLength(TEST_BUFFER_LEN)
				.WithUsage(ResourceUsage.Write).WithPermittedBindings(GPUBindings.WritableShaderResource).Create();
			testUAV = testResource.CreateUnorderedAccessView(TEST_VIEW_START, TEST_VIEW_LEN, TEST_APPEND_CONSUME, TEST_INCLUDE_COUNTER);
			Assert.IsFalse(testUAV.ResourceOrViewDisposed);
			Assert.AreEqual(testResource, testUAV.Resource);
			Assert.AreEqual(TEST_VIEW_START, ((UnorderedBufferAccessView) testUAV).FirstElementIndex);
			Assert.AreEqual(false, ((UnorderedBufferAccessView) testUAV).IsRawAccessView);
			Assert.AreEqual(TEST_VIEW_LEN, ((UnorderedBufferAccessView) testUAV).NumElements);
			Assert.AreEqual(TEST_APPEND_CONSUME, ((UnorderedBufferAccessView) testUAV).AppendConsumeSupport);
			Assert.AreEqual(TEST_INCLUDE_COUNTER, ((UnorderedBufferAccessView) testUAV).IncludesCounter);
			testResource.Dispose();
			Assert.IsTrue(testUAV.ResourceOrViewDisposed);
			testUAV.Dispose();
			Assert.IsTrue(testUAV.IsDisposed);
		}

		[TestMethod]
		public void TestCreateCustomSRVForTextures() {
			const uint TEST_TEXTURE_WIDTH = 128U;
			const uint TEST_TEXTURE_HEIGHT = 64U;
			const uint TEST_TEXTURE_DEPTH = 32U;
			const uint TEST_ARRAY_LEN = 10U;
			const uint TEST_SRV_MIP_START = 2U;
			const uint TEST_SRV_NUM_MIPS = 3U;
			const uint TEST_SRV_ARR_START = 5U;
			const uint TEST_SRV_ARR_LEN = 4U;
			IResource testResource;
			ShaderResourceView testSRV;

			testResource = TextureFactory.NewTexture1D<TexelFormat.RGB32Float>()
				.WithDynamicDetail(false)
				.WithMipAllocation(true)
				.WithMipGenerationTarget(false)
				.WithPermittedBindings(GPUBindings.ReadableShaderResource)
				.WithUsage(ResourceUsage.Write)
				.WithWidth(TEST_TEXTURE_WIDTH)
				.Create();
			testSRV = ((ITexture) testResource).CreateView(TEST_SRV_MIP_START, TEST_SRV_NUM_MIPS);
			Assert.IsFalse(testSRV.ResourceOrViewDisposed);
			Assert.AreEqual(testResource, testSRV.Resource);
			Assert.AreEqual(TEST_SRV_MIP_START, ((ShaderTextureResourceView) testSRV).FirstMipIndex);
			Assert.AreEqual(TEST_SRV_NUM_MIPS, ((ShaderTextureResourceView) testSRV).NumMips);
			testResource.Dispose();
			Assert.IsTrue(testSRV.ResourceOrViewDisposed);
			testSRV.Dispose();
			Assert.IsTrue(testSRV.IsDisposed);

			testResource = TextureFactory.NewTexture2D<TexelFormat.RGB32Float>()
				.WithDynamicDetail(false)
				.WithMipAllocation(true)
				.WithMipGenerationTarget(false)
				.WithPermittedBindings(GPUBindings.ReadableShaderResource)
				.WithUsage(ResourceUsage.Write)
				.WithWidth(TEST_TEXTURE_WIDTH)
				.WithHeight(TEST_TEXTURE_HEIGHT)
				.Create();
			testSRV = ((ITexture) testResource).CreateView(TEST_SRV_MIP_START, TEST_SRV_NUM_MIPS);
			Assert.IsFalse(testSRV.ResourceOrViewDisposed);
			Assert.AreEqual(testResource, testSRV.Resource);
			Assert.AreEqual(TEST_SRV_MIP_START, ((ShaderTextureResourceView) testSRV).FirstMipIndex);
			Assert.AreEqual(TEST_SRV_NUM_MIPS, ((ShaderTextureResourceView) testSRV).NumMips);
			testResource.Dispose();
			Assert.IsTrue(testSRV.ResourceOrViewDisposed);
			testSRV.Dispose();
			Assert.IsTrue(testSRV.IsDisposed);

			testResource = TextureFactory.NewTexture3D<TexelFormat.RGB32Float>()
				.WithDynamicDetail(false)
				.WithMipAllocation(true)
				.WithMipGenerationTarget(false)
				.WithPermittedBindings(GPUBindings.ReadableShaderResource)
				.WithUsage(ResourceUsage.Write)
				.WithWidth(TEST_TEXTURE_WIDTH)
				.WithHeight(TEST_TEXTURE_HEIGHT)
				.WithDepth(TEST_TEXTURE_DEPTH)
				.Create();
			testSRV = ((ITexture) testResource).CreateView(TEST_SRV_MIP_START, TEST_SRV_NUM_MIPS);
			Assert.IsFalse(testSRV.ResourceOrViewDisposed);
			Assert.AreEqual(testResource, testSRV.Resource);
			Assert.AreEqual(TEST_SRV_MIP_START, ((ShaderTextureResourceView) testSRV).FirstMipIndex);
			Assert.AreEqual(TEST_SRV_NUM_MIPS, ((ShaderTextureResourceView) testSRV).NumMips);
			testResource.Dispose();
			Assert.IsTrue(testSRV.ResourceOrViewDisposed);
			testSRV.Dispose();
			Assert.IsTrue(testSRV.IsDisposed);

			testResource = TextureFactory.NewTexture1D<TexelFormat.RGB32Float>()
				.WithDynamicDetail(false)
				.WithMipAllocation(true)
				.WithMipGenerationTarget(false)
				.WithPermittedBindings(GPUBindings.ReadableShaderResource)
				.WithUsage(ResourceUsage.Write)
				.WithWidth(TEST_TEXTURE_WIDTH)
				.CreateArray(TEST_ARRAY_LEN);
			testSRV = ((ITextureArray) testResource).CreateView(TEST_SRV_MIP_START, TEST_SRV_NUM_MIPS, TEST_SRV_ARR_START, TEST_SRV_ARR_LEN);
			Assert.IsFalse(testSRV.ResourceOrViewDisposed);
			Assert.AreEqual(testResource, testSRV.Resource);
			Assert.AreEqual(TEST_SRV_MIP_START, ((ShaderTextureArrayResourceView) testSRV).FirstMipIndex);
			Assert.AreEqual(TEST_SRV_NUM_MIPS, ((ShaderTextureArrayResourceView) testSRV).NumMips);
			Assert.AreEqual(TEST_SRV_ARR_START, ((ShaderTextureArrayResourceView) testSRV).FirstArrayElementIndex);
			Assert.AreEqual(TEST_SRV_ARR_LEN, ((ShaderTextureArrayResourceView) testSRV).NumArrayElements);
			testResource.Dispose();
			Assert.IsTrue(testSRV.ResourceOrViewDisposed);
			testSRV.Dispose();
			Assert.IsTrue(testSRV.IsDisposed);

			testResource = TextureFactory.NewTexture2D<TexelFormat.RGB32Float>()
				.WithDynamicDetail(false)
				.WithMipAllocation(true)
				.WithMipGenerationTarget(false)
				.WithPermittedBindings(GPUBindings.ReadableShaderResource)
				.WithUsage(ResourceUsage.Write)
				.WithWidth(TEST_TEXTURE_WIDTH)
				.WithHeight(TEST_TEXTURE_HEIGHT)
				.CreateArray(TEST_ARRAY_LEN);
			testSRV = ((ITextureArray) testResource).CreateView(TEST_SRV_MIP_START, TEST_SRV_NUM_MIPS, TEST_SRV_ARR_START, TEST_SRV_ARR_LEN);
			Assert.IsFalse(testSRV.ResourceOrViewDisposed);
			Assert.AreEqual(testResource, testSRV.Resource);
			Assert.AreEqual(TEST_SRV_MIP_START, ((ShaderTextureArrayResourceView) testSRV).FirstMipIndex);
			Assert.AreEqual(TEST_SRV_NUM_MIPS, ((ShaderTextureArrayResourceView) testSRV).NumMips);
			Assert.AreEqual(TEST_SRV_ARR_START, ((ShaderTextureArrayResourceView) testSRV).FirstArrayElementIndex);
			Assert.AreEqual(TEST_SRV_ARR_LEN, ((ShaderTextureArrayResourceView) testSRV).NumArrayElements);
			testResource.Dispose();
			Assert.IsTrue(testSRV.ResourceOrViewDisposed);
			testSRV.Dispose();
			Assert.IsTrue(testSRV.IsDisposed);
		}

		[TestMethod]
		public void TestDSVCreation() {
			Texture2D<TexelFormat.DepthStencil> testResource = TextureFactory.NewTexture2D<TexelFormat.DepthStencil>()
				.WithDynamicDetail(false)
				.WithMipAllocation(true)
				.WithMipGenerationTarget(false)
				.WithPermittedBindings(GPUBindings.DepthStencilTarget)
				.WithUsage(ResourceUsage.Write)
				.WithWidth(512U)
				.WithHeight(128U)
				.WithMultisampling(false);
			DepthStencilView testDSV = testResource.CreateDepthStencilView(1U);
			Assert.IsFalse(testDSV.ResourceOrViewDisposed);
			Assert.AreEqual(testResource, testDSV.Resource);
			Assert.AreEqual(1U, testDSV.MipIndex);
			testResource.Dispose();
			Assert.IsTrue(testDSV.ResourceOrViewDisposed);
			testDSV.Dispose();
			Assert.IsTrue(testDSV.IsDisposed);

			Texture2DArray<TexelFormat.DepthStencil> testResourceArr = TextureFactory.NewTexture2D<TexelFormat.DepthStencil>()
				.WithDynamicDetail(false)
				.WithMipAllocation(true)
				.WithMipGenerationTarget(false)
				.WithPermittedBindings(GPUBindings.DepthStencilTarget)
				.WithUsage(ResourceUsage.Write)
				.WithWidth(512U)
				.WithHeight(128U)
				.WithMultisampling(false)
				.CreateArray(10U);
			testDSV = testResourceArr[3].CreateDepthStencilView(1U);
			Assert.IsFalse(testDSV.ResourceOrViewDisposed);
			Assert.AreEqual(testResourceArr[3], testDSV.Resource);
			Assert.AreEqual(1U, testDSV.MipIndex);
			testResourceArr.Dispose();
			Assert.IsTrue(testDSV.ResourceOrViewDisposed);
			testDSV.Dispose();
			Assert.IsTrue(testDSV.IsDisposed);
		}

		[TestMethod]
		public void TestRTVCreation() {
			Texture2D<TexelFormat.RenderTarget> testResource = TextureFactory.NewTexture2D<TexelFormat.RenderTarget>()
				.WithDynamicDetail(false)
				.WithMipAllocation(false)
				.WithMipGenerationTarget(false)
				.WithPermittedBindings(GPUBindings.RenderTarget)
				.WithUsage(ResourceUsage.Write)
				.WithWidth(800U)
				.WithHeight(600U)
				.WithMultisampling(true);
			RenderTargetView testRTV = testResource.CreateRenderTargetView(0U);
			Assert.IsFalse(testRTV.ResourceOrViewDisposed);
			Assert.AreEqual(testResource, testRTV.Resource);
			Assert.AreEqual(0U, testRTV.MipIndex);
			testResource.Dispose();
			Assert.IsTrue(testRTV.ResourceOrViewDisposed);
			testRTV.Dispose();
			Assert.IsTrue(testRTV.IsDisposed);

			Texture2DArray<TexelFormat.RenderTarget> testResourceArr = TextureFactory.NewTexture2D<TexelFormat.RenderTarget>()
				.WithDynamicDetail(false)
				.WithMipAllocation(false)
				.WithMipGenerationTarget(false)
				.WithPermittedBindings(GPUBindings.RenderTarget)
				.WithUsage(ResourceUsage.Write)
				.WithWidth(800U)
				.WithHeight(600U)
				.WithMultisampling(true)
				.CreateArray(10U);
			testRTV = testResourceArr[3].CreateRenderTargetView(0U);
			Assert.IsFalse(testRTV.ResourceOrViewDisposed);
			Assert.AreEqual(testResourceArr[3], testRTV.Resource);
			Assert.AreEqual(0U, testRTV.MipIndex);
			testResourceArr.Dispose();
			Assert.IsTrue(testRTV.ResourceOrViewDisposed);
			testRTV.Dispose();
			Assert.IsTrue(testRTV.IsDisposed);
		}
		#endregion
	}
}