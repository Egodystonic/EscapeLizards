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
	public class Texture2DFactoryTest {
		#region Tests
		[TestMethod]
		public void TestCreationParameters() {
			// Define variables and constants
			var defaultBuilder = TextureFactory.NewTexture2D<TexelFormat.RGBA32UInt>().WithUsage(ResourceUsage.DiscardWrite).WithWidth(100).WithHeight(256);
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
			var withMS = defaultBuilder.WithMultisampling(true);

			// Set up context

			// Execute

			// Assert outcome
			ITexture2D tex = withStagingUsage.Create();
			Assert.AreEqual(ResourceUsage.StagingReadWrite, tex.Usage);
			tex.Dispose();
			tex = withReadWriteBindings.Create();
			tex.Dispose();
			Assert.AreEqual(GPUBindings.ReadableShaderResource | GPUBindings.WritableShaderResource, tex.PermittedBindings);
			tex = withDifferentFormat.Create();
			tex.Dispose();
			Assert.AreEqual(typeof(TexelFormat.Int8), tex.TexelFormat);
			tex = withWidth300.Create();
			tex.Dispose();
			Assert.AreEqual(300U, tex.Width);
			tex = withMipAllocation.Create();
			tex.Dispose();
			Assert.AreEqual(TextureUtils.GetNumMips(1 << 9, 256), tex.NumMips);
			tex = withDynDetail.Create();
			tex.Dispose();
			Assert.AreEqual(true, tex.IsGlobalDetailTarget);
			tex = withMipGen.Create();
			tex.Dispose();
			Assert.AreEqual(true, tex.IsMipGenTarget);
			tex = withMS.Create();
			tex.Dispose();
			Assert.AreEqual(true, tex.IsMultisampled);
			tex.Dispose();

			ITexture2DArray ta = withStagingUsage.CreateArray(10);
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
			Assert.AreEqual(TextureUtils.GetNumMips(1 << 9, 256), ta.NumMips);
			ta.Dispose();
			ta = withDynDetail.CreateArray(10);
			Assert.AreEqual(true, ta.IsGlobalDetailTarget);
			ta.Dispose();
			ta = withMipGen.CreateArray(10);
			Assert.AreEqual(true, ta.IsMipGenTarget);
			ta.Dispose();
			ta = withMS.WithUsage(ResourceUsage.Write).CreateArray(10);
			Assert.AreEqual(true, ta.IsMultisampled);
			ta.Dispose();

			ta = defaultBuilder.WithUsage(ResourceUsage.Immutable).WithInitialData(new TexelFormat.RGBA32UInt[10U * 100U * 256U]).CreateArray(10);
			Assert.AreEqual(10U, ta.ArrayLength);
			ta.Dispose();

#if !DEVELOPMENT && !RELEASE
			try {
				TextureFactory.NewTexture2D<TexelFormat.Float32>()
					.WithUsage(ResourceUsage.Immutable)
					.WithInitialData(null)
					.Create();
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
			try {
				TextureFactory.NewTexture2D<TexelFormat.Float32>()
					.WithUsage(ResourceUsage.DiscardWrite)
					.WithPermittedBindings(GPUBindings.None)
					.Create();
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
			try {
				TextureFactory.NewTexture2D<TexelFormat.Float32>()
					.WithUsage(ResourceUsage.StagingRead)
					.WithPermittedBindings(GPUBindings.ReadableShaderResource)
					.Create();
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
			try {
				TextureFactory.NewTexture2D<TexelFormat.Float32>()
					.WithWidth(0U)
					.Create();
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
			try {
				TextureFactory.NewTexture2D<TexelFormat.Float32>()
					.WithMipAllocation(true)
					.WithWidth(140)
					.WithHeight(1U)
					.Create();
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
			try {
				TextureFactory.NewTexture2D<TexelFormat.Float32>()
					.WithMipAllocation(true)
					.WithMipGenerationTarget(true)
					.WithWidth(1 << 4)
					.WithHeight(1 << 4)
					.WithUsage(ResourceUsage.Write)
					.WithPermittedBindings(GPUBindings.None)
					.Create();
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
			try {
				TextureFactory.NewTexture2D<TexelFormat.Float32>()
					.WithMipAllocation(false)
					.WithMipGenerationTarget(true)
					.WithWidth(1 << 4)
					.WithHeight(1 << 4)
					.WithUsage(ResourceUsage.Write)
					.WithPermittedBindings(GPUBindings.RenderTarget | GPUBindings.ReadableShaderResource)
					.Create();
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
			try {
				TextureFactory.NewTexture2D<TexelFormat.Float32>()
					.WithMipAllocation(true)
					.WithMipGenerationTarget(true)
					.WithWidth(1 << 4)
					.WithHeight(1 << 4)
					.WithUsage(ResourceUsage.Write)
					.WithPermittedBindings(GPUBindings.RenderTarget | GPUBindings.ReadableShaderResource)
					.WithInitialData(new TexelFormat.Float32[(1 << 5) - 1])
					.Create();
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
			try {
				TextureFactory.NewTexture2D<TexelFormat.Float32>()
					.WithMipAllocation(true)
					.WithWidth(1 << 4)
					.WithHeight(1 << 4)
					.WithUsage(ResourceUsage.Immutable)
					.WithPermittedBindings(GPUBindings.ReadableShaderResource)
					.WithInitialData(new TexelFormat.Float32[1 << 4])
					.Create();
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
			try {
				TextureFactory.NewTexture2D<TexelFormat.Float32>()
					.WithMipAllocation(false)
					.WithWidth(1 << 4)
					.WithHeight(1 << 4)
					.WithUsage(ResourceUsage.Immutable)
					.WithPermittedBindings(GPUBindings.ReadableShaderResource)
					.WithInitialData(new TexelFormat.Float32[(1 << 5) - 1])
					.Create();
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
			try {
				TextureFactory.NewTexture2D<TexelFormat.Float32>()
					.WithMipAllocation(true)
					.WithWidth(32U)
					.WithHeight(32U)
					.WithMultisampling(true)
					.Create();
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
			try {
				TextureFactory.NewTexture2D<TexelFormat.Float32>()
					.WithMipAllocation(false)
					.WithWidth(2U)
					.WithHeight(2U)
					.WithMultisampling(true)
					.WithInitialData(new TexelFormat.Float32[4])
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
			const uint TEXTURE_HEIGHT = 1 << 4;
			Texture2DBuilder<TexelFormat.RGBA8UInt> texBuilder = TextureFactory.NewTexture2D<TexelFormat.RGBA8UInt>()
				.WithUsage(ResourceUsage.StagingRead)
				.WithPermittedBindings(GPUBindings.None)
				.WithWidth(TEXTURE_WIDTH)
				.WithHeight(TEXTURE_HEIGHT);

			TexelFormat.RGBA8UInt[] initialDataA = new TexelFormat.RGBA8UInt[TEXTURE_WIDTH * TEXTURE_HEIGHT];
			TexelFormat.RGBA8UInt[] initialDataB = new TexelFormat.RGBA8UInt[TextureUtils.GetSizeTexels(true, TEXTURE_WIDTH, TEXTURE_HEIGHT)];
			Texture2D<TexelFormat.RGBA8UInt> testTextureA, testTextureB;

			TexelArray2D<TexelFormat.RGBA8UInt> texAData;
			TexelArray2D<TexelFormat.RGBA8UInt>[] texBData = new TexelArray2D<TexelFormat.RGBA8UInt>[TextureUtils.GetNumMips(TEXTURE_WIDTH, TEXTURE_HEIGHT)];

			// Set up context
			for (uint i = 0; i < initialDataA.Length; ++i) {
				initialDataA[i].R = (byte) i;
				initialDataA[i].G = (byte) (i * 2);
				initialDataA[i].B = (byte) (i * 3);
				initialDataA[i].A = (byte) (i * 4);
			}
			testTextureA = texBuilder.WithInitialData(initialDataA).Create();

			uint mipWidth = TEXTURE_WIDTH;
			uint mipHeight = TEXTURE_HEIGHT;
			uint texelIndex = 0U;
			while (mipWidth > 1U || mipHeight > 1U) {
				for (uint v = 0; v < mipHeight; ++v) {
					for (uint u = 0; u < mipWidth; ++u, ++texelIndex) {
						initialDataB[texelIndex].R = (byte) (u + mipWidth + v + mipHeight);
						initialDataB[texelIndex].G = (byte) (u + mipWidth + v + mipHeight * 2);
						initialDataB[texelIndex].B = (byte) (u + mipWidth + v + mipHeight * 3);
						initialDataB[texelIndex].A = (byte) (u + mipWidth + v + mipHeight * 4);
					}
				}
				mipWidth = Math.Max(1U, mipWidth >> 1);
				mipHeight = Math.Max(1U, mipHeight >> 1);
			}
			initialDataB[initialDataB.Length - 1] = new TexelFormat.RGBA8UInt { R = 2, G = 3, B = 4, A = 5 };
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

			for (uint mipIndex = 0U; mipIndex < testTextureB.NumMips; ++mipIndex) {
				for (uint v = 0; v < testTextureB.MipHeight(mipIndex); ++v) {
					for (uint u = 0; u < testTextureB.MipWidth(mipIndex); ++u) {
						Assert.AreEqual((byte) (u + testTextureB.MipWidth(mipIndex) + v + testTextureB.MipHeight(mipIndex)), texBData[mipIndex][u, v].R);
						Assert.AreEqual((byte) (u + testTextureB.MipWidth(mipIndex) + v + testTextureB.MipHeight(mipIndex) * 2), texBData[mipIndex][u, v].G);
						Assert.AreEqual((byte) (u + testTextureB.MipWidth(mipIndex) + v + testTextureB.MipHeight(mipIndex) * 3), texBData[mipIndex][u, v].B);
						Assert.AreEqual((byte) (u + testTextureB.MipWidth(mipIndex) + v + testTextureB.MipHeight(mipIndex) * 4), texBData[mipIndex][u, v].A);
					}
				}
				
			}

			testTextureA.Dispose();
			testTextureB.Dispose();
		}
		#endregion
	}
}