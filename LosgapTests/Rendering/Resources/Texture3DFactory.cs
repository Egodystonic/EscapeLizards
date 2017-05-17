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
	public class Texture3DFactoryTest {
		#region Tests
		[TestMethod]
		public void TestCreationParameters() {
			// Define variables and constants
			var defaultBuilder = TextureFactory.NewTexture3D<TexelFormat.RGBA32UInt>().WithUsage(ResourceUsage.DiscardWrite)
				.WithWidth(100).WithHeight(256).WithDepth(8);
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
			ITexture3D tex = withStagingUsage.Create();
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
			Assert.AreEqual(TextureUtils.GetNumMips(1 << 9, 256), tex.NumMips);
			tex.Dispose();
			tex = withDynDetail.Create();
			Assert.AreEqual(true, tex.IsGlobalDetailTarget);
			tex.Dispose();
			tex = withMipGen.Create();
			Assert.AreEqual(true, tex.IsMipGenTarget);
			tex.Dispose();

#if !DEVELOPMENT && !RELEASE
			try {
				TextureFactory.NewTexture3D<TexelFormat.Float32>()
					.WithUsage(ResourceUsage.Immutable)
					.WithInitialData(null)
					.Create();
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
			try {
				TextureFactory.NewTexture3D<TexelFormat.Float32>()
					.WithUsage(ResourceUsage.DiscardWrite)
					.WithPermittedBindings(GPUBindings.None)
					.Create();
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
			try {
				TextureFactory.NewTexture3D<TexelFormat.Float32>()
					.WithUsage(ResourceUsage.StagingRead)
					.WithPermittedBindings(GPUBindings.ReadableShaderResource)
					.Create();
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
			try {
				TextureFactory.NewTexture3D<TexelFormat.Float32>()
					.WithWidth(0U)
					.Create();
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
			try {
				TextureFactory.NewTexture3D<TexelFormat.Float32>()
					.WithMipAllocation(true)
					.WithWidth(140)
					.WithHeight(1U)
					.Create();
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
			try {
				TextureFactory.NewTexture3D<TexelFormat.Float32>()
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
				TextureFactory.NewTexture3D<TexelFormat.Float32>()
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
				TextureFactory.NewTexture3D<TexelFormat.Float32>()
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
				TextureFactory.NewTexture3D<TexelFormat.Float32>()
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
				TextureFactory.NewTexture3D<TexelFormat.Float32>()
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
#endif
		}

		[TestMethod]
		public void TestCreationWithInitialData() {
			// Define variables and constants
			const uint TEXTURE_WIDTH = 1 << 6;
			const uint TEXTURE_HEIGHT = 1 << 4;
			const uint TEXTURE_DEPTH = 1 << 3;
			Texture3DBuilder<TexelFormat.RGBA8UInt> texBuilder = TextureFactory.NewTexture3D<TexelFormat.RGBA8UInt>()
				.WithUsage(ResourceUsage.StagingRead)
				.WithPermittedBindings(GPUBindings.None)
				.WithWidth(TEXTURE_WIDTH)
				.WithHeight(TEXTURE_HEIGHT)
				.WithDepth(TEXTURE_DEPTH);

			TexelFormat.RGBA8UInt[] initialDataA = new TexelFormat.RGBA8UInt[TEXTURE_WIDTH * TEXTURE_HEIGHT * TEXTURE_DEPTH];
			TexelFormat.RGBA8UInt[] initialDataB = new TexelFormat.RGBA8UInt[TextureUtils.GetSizeTexels(true, TEXTURE_WIDTH, TEXTURE_HEIGHT, TEXTURE_DEPTH)];
			Texture3D<TexelFormat.RGBA8UInt> testTextureA, testTextureB;

			TexelArray3D<TexelFormat.RGBA8UInt> texAData;
			TexelArray3D<TexelFormat.RGBA8UInt>[] texBData = new TexelArray3D<TexelFormat.RGBA8UInt>[TextureUtils.GetNumMips(TEXTURE_WIDTH, TEXTURE_HEIGHT, TEXTURE_DEPTH)];

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
			uint mipDepth = TEXTURE_DEPTH;
			uint texelIndex = 0U;
			while (mipWidth > 1U || mipHeight > 1U || mipDepth > 1U) {
				for (uint w = 0; w < mipDepth; ++w) {
					for (uint v = 0; v < mipHeight; ++v) {
						for (uint u = 0; u < mipWidth; ++u, ++texelIndex) {
							initialDataB[texelIndex].R = (byte) (u + mipWidth + v + mipHeight + w + mipDepth);
							initialDataB[texelIndex].G = (byte) (u + mipWidth + v + mipHeight + w + mipDepth * 2);
							initialDataB[texelIndex].B = (byte) (u + mipWidth + v + mipHeight + w + mipDepth * 3);
							initialDataB[texelIndex].A = (byte) (u + mipWidth + v + mipHeight + w + mipDepth * 4);
						}
					}
				}
				mipWidth = Math.Max(1U, mipWidth >> 1);
				mipHeight = Math.Max(1U, mipHeight >> 1);
				mipDepth = Math.Max(1U, mipDepth >> 1);
			}
			initialDataB[initialDataB.Length - 1] = new TexelFormat.RGBA8UInt { R = 3, G = 4, B = 5, A = 6 };
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
				for (uint w = 0; w < testTextureB.MipDepth(mipIndex); ++w) {
					for (uint v = 0; v < testTextureB.MipHeight(mipIndex); ++v) {
						for (uint u = 0; u < testTextureB.MipWidth(mipIndex); ++u) {
							Assert.AreEqual((byte) (u + testTextureB.MipWidth(mipIndex) + v + testTextureB.MipHeight(mipIndex) + w + testTextureB.MipDepth(mipIndex)), texBData[mipIndex][u, v, w].R);
							Assert.AreEqual((byte) (u + testTextureB.MipWidth(mipIndex) + v + testTextureB.MipHeight(mipIndex) + w + testTextureB.MipDepth(mipIndex) * 2), texBData[mipIndex][u, v, w].G);
							Assert.AreEqual((byte) (u + testTextureB.MipWidth(mipIndex) + v + testTextureB.MipHeight(mipIndex) + w + testTextureB.MipDepth(mipIndex) * 3), texBData[mipIndex][u, v, w].B);
							Assert.AreEqual((byte) (u + testTextureB.MipWidth(mipIndex) + v + testTextureB.MipHeight(mipIndex) + w + testTextureB.MipDepth(mipIndex) * 4), texBData[mipIndex][u, v, w].A);
						}
					}
				}
			}

			testTextureA.Dispose();
			testTextureB.Dispose();
		}
		#endregion
	}
}