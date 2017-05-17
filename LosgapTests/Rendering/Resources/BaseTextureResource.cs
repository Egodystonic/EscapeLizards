// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 10 11 2014 at 14:33 by Ben Bowen

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap.Rendering {
	[TestClass]
	public class BaseTextureResourceTest {
		#region Tests
		[TestMethod]
		public void TestMipDimensions() {
			// Define variables and constants
			const uint ORIGINAL_WIDTH_TX = 1 << 10;
			Texture1D<TexelFormat.RGBA32UInt> texture = TextureFactory.NewTexture1D<TexelFormat.RGBA32UInt>()
				.WithWidth(ORIGINAL_WIDTH_TX)
				.WithMipAllocation(true)
				.WithUsage(ResourceUsage.Write);

			// Set up context


			// Execute
			for (int i = 0; ORIGINAL_WIDTH_TX >> i > 0; ++i) {
				Assert.AreEqual(ORIGINAL_WIDTH_TX >> i, texture.MipWidth((uint) i));
			}

#if !DEVELOPMENT && !RELEASE
			try {
				texture.MipWidth(10000U);
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
#endif

			// Assert outcome
			texture.Dispose();
		}

		[TestMethod]
		public void TestSubresIndex() {
			// Define variables and constants
			Texture1D<TexelFormat.RGBA32UInt> texture = TextureFactory.NewTexture1D<TexelFormat.RGBA32UInt>()
				.WithWidth(1 << 5)
				.WithMipAllocation(true)
				.WithUsage(ResourceUsage.Write);

			// Set up context


			// Execute
			Assert.AreEqual(texture.GetSubresourceIndex(0U), 0U);
			Assert.AreEqual(texture.GetSubresourceIndex(1U), 1U);
			Assert.AreEqual(texture.GetSubresourceIndex(2U), 2U);

#if !DEVELOPMENT && !RELEASE
			try {
				texture.GetSubresourceIndex(10000U);
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
#endif

			// Assert outcome
			texture.Dispose();
		}

		[TestMethod]
		public void TestCopyTo() {
			LosgapSystem.InvokeOnMaster(() => {
				// Define variables and constants
				const uint WIDTH_TX = 512U;
				Texture1D<TexelFormat.RGBA32Int> srcTex = TextureFactory.NewTexture1D<TexelFormat.RGBA32Int>()
					.WithDynamicDetail(false)
					.WithInitialData(
						Enumerable.Range(0, (int) TextureUtils.GetSizeTexels(true, WIDTH_TX))
							.Select(i => new TexelFormat.RGBA32Int { R = i, G = i * 2, B = i * 3, A = i * 4 })
							.ToArray()
					)
					.WithMipAllocation(true)
					.WithMipGenerationTarget(false)
					.WithPermittedBindings(GPUBindings.ReadableShaderResource)
					.WithUsage(ResourceUsage.Immutable)
					.WithWidth(WIDTH_TX);

				// Set up context


				// Execute
				Texture1D<TexelFormat.RGBA32Int> dstTex = srcTex.Clone()
					.WithUsage(ResourceUsage.StagingRead)
					.WithPermittedBindings(GPUBindings.None);
				srcTex.CopyTo(dstTex);

				// Assert outcome
				TexelFormat.RGBA32Int[] copiedData = dstTex.ReadAll();
				for (int i = 0; i < copiedData.Length; ++i) {
					Assert.AreEqual(i, copiedData[i].R);
					Assert.AreEqual(i * 2, copiedData[i].G);
					Assert.AreEqual(i * 3, copiedData[i].B);
					Assert.AreEqual(i * 4, copiedData[i].A);
				}

				srcTex.Dispose();
				dstTex.Dispose();
			});
		}

		[TestMethod]
		public void TestGenerateMips() {
			LosgapSystem.InvokeOnMaster(() => {
				// Define variables and constants
				const uint WIDTH_TX = 256U;
				const uint HEIGHT_TX = 128U;
				Texture2D<TexelFormat.RGBA8UNorm> sourceTex = TextureFactory.NewTexture2D<TexelFormat.RGBA8UNorm>()
					.WithWidth(WIDTH_TX)
					.WithHeight(HEIGHT_TX)
					.WithMipAllocation(true)
					.WithMipGenerationTarget(true)
					.WithPermittedBindings(GPUBindings.ReadableShaderResource | GPUBindings.RenderTarget)
					.WithUsage(ResourceUsage.Write);

				Texture2D<TexelFormat.RGBA8UNorm> copyDestTex = sourceTex.Clone()
					.WithUsage(ResourceUsage.StagingRead)
					.WithPermittedBindings(GPUBindings.None)
					.WithMipGenerationTarget(false);

				// Set up context
				sourceTex.Write(
					// ReSharper disable PossibleLossOfFraction No one cares
					Enumerable.Range(0, (int) TextureUtils.GetSizeTexels(false, WIDTH_TX, HEIGHT_TX))
						.Select(i => new TexelFormat.RGBA8UNorm { R = 0f, G = 0.33f, B = 0.67f, A = 1f }).ToArray(),
					// ReSharper restore PossibleLossOfFraction
					new SubresourceBox(0U, WIDTH_TX, 0U, HEIGHT_TX),
					0U
				);

				// Execute
				sourceTex.GenerateMips();
				sourceTex.CopyTo(copyDestTex);

				// Assert outcome
				for (uint i = 1U; i < TextureUtils.GetNumMips(WIDTH_TX, HEIGHT_TX); ++i) {
					IEnumerable<TexelFormat.RGBA8UNorm> outData = copyDestTex.Read(i);
					// ReSharper disable CompareOfFloatsByEqualityOperator Exact equality is fine here
					Assert.IsTrue(outData.Any(texel => texel.R != 0f || texel.G != 0f || texel.B != 0f || texel.A != 0f));
					// ReSharper restore CompareOfFloatsByEqualityOperator
				}

				sourceTex.Dispose();
				copyDestTex.Dispose();
			});
		}
		#endregion
	}
}