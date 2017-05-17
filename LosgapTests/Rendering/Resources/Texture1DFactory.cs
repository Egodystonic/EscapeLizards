// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 29 10 2014 at 09:20 by Ben Bowen

using System;
using System.Linq;
using System.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap.Rendering {
	[TestClass]
	public class Texture1DFactoryTest {
		#region Tests
		[TestMethod]
		public void TestCreationParameters() {
			// Define variables and constants
			var defaultBuilder = TextureFactory.NewTexture1D<TexelFormat.RGBA32UInt>().WithUsage(ResourceUsage.DiscardWrite).WithWidth(100);
			var withStagingUsage = defaultBuilder.WithUsage(ResourceUsage.StagingReadWrite).WithPermittedBindings(GPUBindings.None);
			var withReadWriteBindings = defaultBuilder.WithUsage(ResourceUsage.Write).WithPermittedBindings(GPUBindings.ReadableShaderResource | GPUBindings.WritableShaderResource);
			var withDifferentFormat = defaultBuilder.WithTexelFormat<TexelFormat.Int8>();
			var withWidth300 = defaultBuilder.WithWidth(300);
			var withMipAllocation = defaultBuilder.WithUsage(ResourceUsage.Write).WithWidth(1 << 9).WithMipAllocation(true);
			var withDynDetail = withMipAllocation.WithDynamicDetail(true);
			var withMipGen = withMipAllocation
				.WithUsage(ResourceUsage.Write)
				.WithPermittedBindings(GPUBindings.RenderTarget | GPUBindings.ReadableShaderResource)
				.WithMipGenerationTarget(true)
				.WithTexelFormat<TexelFormat.RGBA8UNorm>();

			// Set up context

			// Execute

			// Assert outcome
			ITexture1D tex = withStagingUsage.Create();
			Assert.AreEqual(ResourceUsage.StagingReadWrite, tex.Usage);
			tex.Dispose();
			tex = withReadWriteBindings.Create();
			Assert.AreEqual(GPUBindings.ReadableShaderResource | GPUBindings.WritableShaderResource, tex.PermittedBindings);
			tex.Dispose();
			tex = withDifferentFormat.Create();
			Assert.AreEqual(typeof(TexelFormat.Int8), tex.TexelFormat);
			tex.Dispose();
			tex = withWidth300.Create();
			Assert.AreEqual(300U, tex.Width);
			tex.Dispose();
			tex = withMipAllocation.Create();
			Assert.AreEqual(TextureUtils.GetNumMips(1 << 9), tex.NumMips);
			tex.Dispose();
			tex = withDynDetail.Create();
			Assert.AreEqual(true, tex.IsGlobalDetailTarget);
			tex.Dispose();
			tex = withMipGen.Create();
			Assert.AreEqual(true, tex.IsMipGenTarget);
			tex.Dispose();

			ITexture1DArray ta = withStagingUsage.CreateArray(10);
			Assert.AreEqual(ResourceUsage.StagingReadWrite, ta.Usage);
			ta.Dispose();
			ta = withReadWriteBindings.CreateArray(10);
			Assert.AreEqual(GPUBindings.ReadableShaderResource | GPUBindings.WritableShaderResource, ta.PermittedBindings);
			ta.Dispose();
			ta = withDifferentFormat.WithUsage(ResourceUsage.Write).CreateArray(10);
			Assert.AreEqual(typeof(TexelFormat.Int8), ta.TexelFormat);
			ta.Dispose();
			ta = withWidth300.WithUsage(ResourceUsage.Write).CreateArray(10);
			Assert.AreEqual(300U, ta.Width);
			ta.Dispose();
			ta = withMipAllocation.CreateArray(10);
			Assert.AreEqual(TextureUtils.GetNumMips(1 << 9), ta.NumMips);
			ta.Dispose();
			ta = withDynDetail.CreateArray(10);
			Assert.AreEqual(true, ta.IsGlobalDetailTarget);
			ta.Dispose();
			ta = withMipGen.CreateArray(10);
			Assert.AreEqual(true, ta.IsMipGenTarget);
			ta.Dispose();
			ta = defaultBuilder.WithUsage(ResourceUsage.Immutable).WithInitialData(new TexelFormat.RGBA32UInt[1000]).CreateArray(10);
			Assert.AreEqual(10U, ta.ArrayLength);
			ta.Dispose();

#if !DEVELOPMENT && !RELEASE
			try {
				TextureFactory.NewTexture1D<TexelFormat.Float32>()
					.WithUsage(ResourceUsage.Immutable)
					.WithInitialData(null)
					.Create();
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
			try {
				TextureFactory.NewTexture1D<TexelFormat.Float32>()
					.WithUsage(ResourceUsage.DiscardWrite)
					.WithPermittedBindings(GPUBindings.None)
					.Create();
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
			try {
				TextureFactory.NewTexture1D<TexelFormat.Float32>()
					.WithUsage(ResourceUsage.StagingRead)
					.WithPermittedBindings(GPUBindings.ReadableShaderResource)
					.Create();
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
			try {
				TextureFactory.NewTexture1D<TexelFormat.Float32>()
					.WithWidth(0U)
					.Create();
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
			try {
				TextureFactory.NewTexture1D<TexelFormat.Float32>()
					.WithMipAllocation(true)
					.WithWidth(140)
					.Create();
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
			try {
				TextureFactory.NewTexture1D<TexelFormat.Float32>()
					.WithMipAllocation(true)
					.WithMipGenerationTarget(true)
					.WithWidth(1 << 4)
					.WithUsage(ResourceUsage.Write)
					.WithPermittedBindings(GPUBindings.None)
					.Create();
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
			try {
				TextureFactory.NewTexture1D<TexelFormat.Float32>()
					.WithMipAllocation(false)
					.WithMipGenerationTarget(true)
					.WithWidth(1 << 4)
					.WithUsage(ResourceUsage.Write)
					.WithPermittedBindings(GPUBindings.RenderTarget | GPUBindings.ReadableShaderResource)
					.Create();
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
			try {
				TextureFactory.NewTexture1D<TexelFormat.Float32>()
					.WithMipAllocation(true)
					.WithMipGenerationTarget(true)
					.WithWidth(1 << 4)
					.WithUsage(ResourceUsage.Write)
					.WithPermittedBindings(GPUBindings.RenderTarget | GPUBindings.ReadableShaderResource)
					.WithInitialData(new TexelFormat.Float32[(1 << 5) - 1])
					.Create();
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
			try {
				TextureFactory.NewTexture1D<TexelFormat.Float32>()
					.WithMipAllocation(true)
					.WithWidth(1 << 4)
					.WithUsage(ResourceUsage.Immutable)
					.WithPermittedBindings(GPUBindings.ReadableShaderResource)
					.WithInitialData(new TexelFormat.Float32[1 << 4])
					.Create();
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
			try {
				TextureFactory.NewTexture1D<TexelFormat.Float32>()
					.WithMipAllocation(false)
					.WithWidth(1 << 4)
					.WithUsage(ResourceUsage.Immutable)
					.WithPermittedBindings(GPUBindings.ReadableShaderResource)
					.WithInitialData(new TexelFormat.Float32[(1 << 5) - 1])
					.Create();
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
#endif
		}

		[TestMethod]
		public void TestCreationWithInitialData() {
			// Define variables and constants
			const uint TEXTURE_WIDTH = 1 << 6;
			Texture1DBuilder<TexelFormat.RGBA8UInt> texBuilder = TextureFactory.NewTexture1D<TexelFormat.RGBA8UInt>()
				.WithUsage(ResourceUsage.StagingRead)
				.WithPermittedBindings(GPUBindings.None)
				.WithWidth(TEXTURE_WIDTH);

			TexelFormat.RGBA8UInt[] initialDataA = new TexelFormat.RGBA8UInt[TEXTURE_WIDTH];
			TexelFormat.RGBA8UInt[] initialDataB = new TexelFormat.RGBA8UInt[(TEXTURE_WIDTH << 1) - 1];
			Texture1D<TexelFormat.RGBA8UInt> testTextureA, testTextureB;

			TexelArray1D<TexelFormat.RGBA8UInt> texAData;
			TexelArray1D<TexelFormat.RGBA8UInt>[] texBData = new TexelArray1D<TexelFormat.RGBA8UInt>[TextureUtils.GetNumMips(TEXTURE_WIDTH)];

			// Set up context
			for (uint i = 0; i < initialDataA.Length; ++i) {
				initialDataA[i].R = (byte) i;
				initialDataA[i].G = (byte) (i * 2);
				initialDataA[i].B = (byte) (i * 3);
				initialDataA[i].A = (byte) (i * 4);
			}
			testTextureA = texBuilder.WithInitialData(initialDataA).Create();

			uint mipWidth = TEXTURE_WIDTH;
			uint texelIndex = 0U;
			while (mipWidth > 0) {
				for (uint i = 0; i < mipWidth; ++i, ++texelIndex) {
					initialDataB[texelIndex].R = (byte) (i + mipWidth);
					initialDataB[texelIndex].G = (byte) (i * 2 + mipWidth);
					initialDataB[texelIndex].B = (byte) (i * 3 + mipWidth);
					initialDataB[texelIndex].A = (byte) (i * 4 + mipWidth);
				}
				mipWidth >>= 1;
			}
			testTextureB = texBuilder.WithMipAllocation(true).WithInitialData(initialDataB).Create();



			// Execute
			texAData = testTextureA.Read(0U);
			for (uint i = 0; i < texBData.Length; ++i) {
				texBData[i] = testTextureB.Read(i);
			}

			// Assert outcome
			for (uint i = 0; i < texAData.Width; ++i) {
				Assert.AreEqual((byte) i, initialDataA[i].R);
				Assert.AreEqual((byte) (i * 2), initialDataA[i].G);
				Assert.AreEqual((byte) (i * 3), initialDataA[i].B);
				Assert.AreEqual((byte) (i * 4), initialDataA[i].A);
			}

			mipWidth = TEXTURE_WIDTH;
			texelIndex = 0U;
			while (mipWidth > 0) {
				for (uint i = 0; i < mipWidth; ++i, ++texelIndex) {
					Assert.AreEqual((byte) (i + mipWidth), initialDataB[texelIndex].R);
					Assert.AreEqual((byte) (i * 2 + mipWidth), initialDataB[texelIndex].G);
					Assert.AreEqual((byte) (i * 3 + mipWidth), initialDataB[texelIndex].B);
					Assert.AreEqual((byte) (i * 4 + mipWidth), initialDataB[texelIndex].A);
				}
				mipWidth >>= 1;
			}

			testTextureA.Dispose();
			testTextureB.Dispose();
		}
		#endregion
	}
}