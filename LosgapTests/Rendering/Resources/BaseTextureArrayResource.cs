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
	public class BaseTextureArrayResourceTest {

		#region Tests
		[TestMethod]
		public void TestMipDimensions() {
			// Define variables and constants
			const uint ORIGINAL_WIDTH_TX = 1 << 10;
			Texture1DArray<TexelFormat.RGBA32UInt> textureArray = TextureFactory.NewTexture1D<TexelFormat.RGBA32UInt>()
				.WithWidth(ORIGINAL_WIDTH_TX)
				.WithMipAllocation(true)
				.WithUsage(ResourceUsage.Write)
				.CreateArray(10);

			// Set up context


			// Execute
			for (int i = 0; ORIGINAL_WIDTH_TX >> i > 0; ++i) {
				Assert.AreEqual(ORIGINAL_WIDTH_TX >> i, textureArray.MipWidth((uint) i));
			}

#if !DEVELOPMENT && !RELEASE
			try {
				textureArray.MipWidth(10000U);
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
#endif

			// Assert outcome
			textureArray.Dispose();
		}

		[TestMethod]
		public void TestClone() {
			// Define variables and constants
			const uint MIP0_WIDTH = 512U;
			const uint ARRAY_LENGTH = 10U;
			Texture1DArray<TexelFormat.RGBA32UInt> srcArray = TextureFactory.NewTexture1D<TexelFormat.RGBA32UInt>()
				.WithDynamicDetail(true)
				.WithInitialData(
					Enumerable.Repeat(
						Enumerable.Range(0, (int) TextureUtils.GetSizeTexels(true, MIP0_WIDTH))
							.Select(i => (uint) i)
							.Select(i => new TexelFormat.RGBA32UInt { R = i, G = i * 2, B = i * 3, A = i * 4 }),
						(int) ARRAY_LENGTH
					)
					.Flatten()
					.ToArray()
				)
				.WithMipAllocation(true)
				.WithMipGenerationTarget(false)
				.WithPermittedBindings(GPUBindings.ReadableShaderResource)
				.WithUsage(ResourceUsage.Immutable)
				.WithMipAllocation(true)
				.WithUsage(ResourceUsage.Write)
				.WithWidth(MIP0_WIDTH)
				.CreateArray(ARRAY_LENGTH);

			// Set up context


			// Execute
			Texture1DArray<TexelFormat.RGBA32UInt> dstArray = srcArray.Clone().CreateArray(srcArray.ArrayLength);

			// Assert outcome
			Assert.AreEqual(srcArray.IsGlobalDetailTarget, dstArray.IsGlobalDetailTarget);
			Assert.AreEqual(srcArray.IsMipGenTarget, dstArray.IsMipGenTarget);
			Assert.AreEqual(srcArray.IsMipmapped, dstArray.IsMipmapped);
			Assert.AreEqual(srcArray.ArrayLength, dstArray.ArrayLength);
			Assert.AreEqual(srcArray.NumMips, dstArray.NumMips);
			Assert.AreEqual(srcArray.PermittedBindings, dstArray.PermittedBindings);
			Assert.AreEqual(srcArray.Size, dstArray.Size);
			Assert.AreEqual(srcArray.TexelFormat, dstArray.TexelFormat);
			Assert.AreEqual(srcArray.TexelSizeBytes, dstArray.TexelSizeBytes);
			Assert.AreEqual(srcArray.Usage, dstArray.Usage);
			Assert.AreEqual(srcArray.Width, dstArray.Width);

			srcArray.Dispose();
			dstArray.Dispose();
		}

		[TestMethod]
		public void TestCloneWithInitData() {
			// Define variables and constants
			const uint MIP0_WIDTH = 512U;
			const uint ARRAY_LENGTH = 10U;
			Texture1DArray<TexelFormat.RGBA32UInt> srcArray = TextureFactory.NewTexture1D<TexelFormat.RGBA32UInt>()
				.WithDynamicDetail(false)
				.WithInitialData(
					Enumerable.Range(0, (int) TextureUtils.GetSizeTexels(true, MIP0_WIDTH) * (int) ARRAY_LENGTH)
						.Select(i => (uint) i)
						.Select(i => new TexelFormat.RGBA32UInt { R = i, G = i * 2, B = i * 3, A = i * 4 })
					.ToArray()
				)
				.WithMipAllocation(true)
				.WithMipGenerationTarget(false)
				.WithPermittedBindings(GPUBindings.None)
				.WithUsage(ResourceUsage.StagingRead)
				.WithWidth(MIP0_WIDTH)
				.CreateArray(ARRAY_LENGTH);

			// Set up context


			// Execute
			var cloneArray = srcArray.Clone(true).CreateArray(ARRAY_LENGTH);

			// Assert outcome
			uint curVal = 0U;
			foreach (Texture1D<TexelFormat.RGBA32UInt> texture1D in cloneArray) {
				TexelFormat.RGBA32UInt[] texData = texture1D.ReadAll();
				for (int i = 0; i < texData.Length; i++) {
					Assert.AreEqual(curVal, texData[i].R);
					Assert.AreEqual(curVal * 2, texData[i].G);
					Assert.AreEqual(curVal * 3, texData[i].B);
					Assert.AreEqual(curVal * 4, texData[i].A);

					++curVal;
				}
			}

			cloneArray.Dispose();
			srcArray.Dispose();
		}

		[TestMethod]
		public void TestCopyTo() {
			LosgapSystem.InvokeOnMaster(() => {
				// Define variables and constants
				const uint MIP0_WIDTH = 512U;
				const uint ARRAY_LENGTH = 10U;
				Texture1DArray<TexelFormat.RGBA32UInt> srcArray = TextureFactory.NewTexture1D<TexelFormat.RGBA32UInt>()
					.WithDynamicDetail(false)
					.WithInitialData(
						Enumerable.Repeat(
							Enumerable.Range(0, (int) TextureUtils.GetSizeTexels(true, MIP0_WIDTH))
								.Select(i => (uint) i)
								.Select(i => new TexelFormat.RGBA32UInt { R = i, G = i * 2, B = i * 3, A = i * 4 }),
							(int) ARRAY_LENGTH
							)
							.Flatten()
							.ToArray()
					)
					.WithMipAllocation(true)
					.WithMipGenerationTarget(false)
					.WithPermittedBindings(GPUBindings.ReadableShaderResource)
					.WithUsage(ResourceUsage.Immutable)
					.WithMipAllocation(true)
					.WithUsage(ResourceUsage.Write)
					.WithWidth(MIP0_WIDTH)
					.CreateArray(ARRAY_LENGTH);

				// Set up context
				Texture1DArray<TexelFormat.RGBA32UInt> dstArray = srcArray.Clone()
					.WithUsage(ResourceUsage.StagingRead)
					.WithPermittedBindings(GPUBindings.None)
					.CreateArray(srcArray.ArrayLength);

				srcArray.CopyTo(dstArray);

				var data = dstArray[4].ReadAll();

				// Execute
				for (int i = 0, curMipIndex = 0; i < data.Length; ++curMipIndex) {
					for (int j = 0; j < dstArray.MipWidth((uint) curMipIndex); ++j, ++i) {
						Assert.AreEqual((uint) i, data[i].R);
						Assert.AreEqual((uint) i * 2, data[i].G);
						Assert.AreEqual((uint) i * 3, data[i].B);
						Assert.AreEqual((uint) i * 4, data[i].A);
					}
				}

				// Assert outcome
				srcArray.Dispose();
				dstArray.Dispose();
			});
		}

		[TestMethod]
		public void TestArrayElementsAreIdentical() {
			// Define variables and constants
			const uint MIP0_WIDTH = 512U;
			const uint ARRAY_LENGTH = 10U;
			Texture1DArray<TexelFormat.RGBA32UInt> srcArray = TextureFactory.NewTexture1D<TexelFormat.RGBA32UInt>()
				.WithDynamicDetail(true)
				.WithInitialData(
					Enumerable.Repeat(
						Enumerable.Range(0, (int) TextureUtils.GetSizeTexels(true, MIP0_WIDTH))
							.Select(i => (uint) i)
							.Select(i => new TexelFormat.RGBA32UInt { R = i, G = i * 2, B = i * 3, A = i * 4 }),
						(int) ARRAY_LENGTH
					)
					.Flatten()
					.ToArray()
				)
				.WithMipAllocation(true)
				.WithMipGenerationTarget(false)
				.WithPermittedBindings(GPUBindings.ReadableShaderResource)
				.WithUsage(ResourceUsage.Immutable)
				.WithMipAllocation(true)
				.WithUsage(ResourceUsage.Write)
				.WithWidth(MIP0_WIDTH)
				.CreateArray(ARRAY_LENGTH);

			// Set up context


			// Execute


			// Assert outcome
			foreach (Texture1D<TexelFormat.RGBA32UInt> arrayElement in srcArray) {
				Assert.AreEqual(srcArray.IsGlobalDetailTarget, arrayElement.IsGlobalDetailTarget);
				Assert.AreEqual(srcArray.IsMipGenTarget, arrayElement.IsMipGenTarget);
				Assert.AreEqual(srcArray.IsMipmapped, arrayElement.IsMipmapped);
				Assert.AreEqual(srcArray.NumMips, arrayElement.NumMips);
				Assert.AreEqual(srcArray.PermittedBindings, arrayElement.PermittedBindings);
				Assert.AreEqual(srcArray.Size / srcArray.ArrayLength, arrayElement.Size);
				Assert.AreEqual(srcArray.TexelFormat, arrayElement.TexelFormat);
				Assert.AreEqual(srcArray.TexelSizeBytes, arrayElement.TexelSizeBytes);
				Assert.AreEqual(srcArray.Usage, arrayElement.Usage);
				Assert.AreEqual(srcArray.Width, arrayElement.Width);
			}
			
			srcArray.Dispose();
		}

		[TestMethod]
		public void TestGenerateMips() {
			LosgapSystem.InvokeOnMaster(() => {
				// Define variables and constants
				const uint WIDTH_TX = 256U;
				const uint HEIGHT_TX = 128U;
				const uint ARR_LEN = 4U;
				Texture2DArray<TexelFormat.RGBA8UNorm> sourceTex = TextureFactory.NewTexture2D<TexelFormat.RGBA8UNorm>()
					.WithWidth(WIDTH_TX)
					.WithHeight(HEIGHT_TX)
					.WithMipAllocation(true)
					.WithMipGenerationTarget(true)
					.WithPermittedBindings(GPUBindings.ReadableShaderResource | GPUBindings.RenderTarget)
					.WithUsage(ResourceUsage.Write)
					.CreateArray(ARR_LEN);

				Texture2DArray<TexelFormat.RGBA8UNorm> copyDestTex = sourceTex.Clone()
					.WithUsage(ResourceUsage.StagingRead)
					.WithPermittedBindings(GPUBindings.None)
					.WithMipGenerationTarget(false)
					.CreateArray(ARR_LEN);

				// Set up context
				for (uint u = 0U; u < ARR_LEN; u++) {
					sourceTex[u].Write(
						// ReSharper disable PossibleLossOfFraction No one cares
						Enumerable.Range(0, (int) TextureUtils.GetSizeTexels(false, WIDTH_TX, HEIGHT_TX))
							.Select(i => new TexelFormat.RGBA8UNorm { R = 0f, G = 0.33f, B = 0.67f, A = 1f }).ToArray(),
						// ReSharper restore PossibleLossOfFraction
						new SubresourceBox(0U, WIDTH_TX, 0U, HEIGHT_TX),
						0U
					);
				}

				// Execute
				sourceTex.GenerateMips();
				sourceTex.CopyTo(copyDestTex);

				// Assert outcome
				for (uint u = 0U; u < ARR_LEN; u++) {
					for (uint i = 1U; i < TextureUtils.GetNumMips(WIDTH_TX, HEIGHT_TX); ++i) {
						IEnumerable<TexelFormat.RGBA8UNorm> outData = copyDestTex[u].Read(i);
						// ReSharper disable CompareOfFloatsByEqualityOperator Exact equality is fine here
						Assert.IsTrue(outData.Any(texel => texel.R != 0f || texel.G != 0f || texel.B != 0f || texel.A != 0f));
						// ReSharper restore CompareOfFloatsByEqualityOperator
					}
				}

				sourceTex.Dispose();
				copyDestTex.Dispose();
			});
		}
		#endregion
	}
}