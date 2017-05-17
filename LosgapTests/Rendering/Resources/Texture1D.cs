// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 29 10 2014 at 15:14 by Ben Bowen

using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap.Rendering {
	[TestClass]
	public class Texture1DTest {
		#region Tests
		[TestMethod]
		public void TestClone() {
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
				.WithPermittedBindings(GPUBindings.None)
				.WithUsage(ResourceUsage.StagingRead)
				.WithWidth(WIDTH_TX);

			// Set up context


			// Execute
			Texture1D<TexelFormat.RGBA32Int> destNoCopy = srcTex.Clone(false);
			Texture1D<TexelFormat.RGBA32Int> destCopy = srcTex.Clone(true);

			// Assert outcome
			Assert.AreEqual(srcTex.IsGlobalDetailTarget, destNoCopy.IsGlobalDetailTarget);
			Assert.AreEqual(srcTex.IsMipGenTarget, destNoCopy.IsMipGenTarget);
			Assert.AreEqual(srcTex.IsMipmapped, destNoCopy.IsMipmapped);
			Assert.AreEqual(srcTex.NumMips, destNoCopy.NumMips);
			Assert.AreEqual(srcTex.PermittedBindings, destNoCopy.PermittedBindings);
			Assert.AreEqual(srcTex.Size, destNoCopy.Size);
			Assert.AreEqual(srcTex.TexelFormat, destNoCopy.TexelFormat);
			Assert.AreEqual(srcTex.TexelSizeBytes, destNoCopy.TexelSizeBytes);
			Assert.AreEqual(srcTex.Usage, destNoCopy.Usage);
			Assert.AreEqual(srcTex.Width, destNoCopy.Width);

			TexelFormat.RGBA32Int[] copiedData = destCopy.ReadAll();
			for (int i = 0; i < copiedData.Length; ++i) {
				Assert.AreEqual(i, copiedData[i].R);
				Assert.AreEqual(i * 2, copiedData[i].G);
				Assert.AreEqual(i * 3, copiedData[i].B);
				Assert.AreEqual(i * 4, copiedData[i].A);
			}

			srcTex.Dispose();
			destCopy.Dispose();
			destNoCopy.Dispose();
		}

		[TestMethod]
		public void TestCopyTo() {
			LosgapSystem.InvokeOnMaster(() => {
				// Define variables and constants
				const uint WIDTH_TX = 512U;

				const uint NUM_TEXELS_TO_COPY = 25U;
				const uint FIRST_TEXEL_TO_COPY = 25U;
				const uint SRC_MIP_INDEX = 1U;
				const uint DST_MIP_INDEX = 3U;
				const uint DST_WRITE_OFFSET = 15U;
				const uint DATA_VALUE_START_R = 512U + FIRST_TEXEL_TO_COPY;

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
				srcTex.CopyTo(
					dstTex,
					new SubresourceBox(FIRST_TEXEL_TO_COPY, FIRST_TEXEL_TO_COPY + NUM_TEXELS_TO_COPY),
					SRC_MIP_INDEX,
					DST_MIP_INDEX,
					DST_WRITE_OFFSET
					);

				// Assert outcome
				TexelArray1D<TexelFormat.RGBA32Int> copiedData = dstTex.Read(DST_MIP_INDEX);
				for (int i = 0; i < NUM_TEXELS_TO_COPY; ++i) {
					Assert.AreEqual(DATA_VALUE_START_R + i, copiedData[i + (int) DST_WRITE_OFFSET].R);
					Assert.AreEqual((DATA_VALUE_START_R + i) * 2, copiedData[i + (int) DST_WRITE_OFFSET].G);
					Assert.AreEqual((DATA_VALUE_START_R + i) * 3, copiedData[i + (int) DST_WRITE_OFFSET].B);
					Assert.AreEqual((DATA_VALUE_START_R + i) * 4, copiedData[i + (int) DST_WRITE_OFFSET].A);
				}

				srcTex.Dispose();
				dstTex.Dispose();
			});
		}

		[TestMethod]
		public void TestDiscardWrite() {
			LosgapSystem.InvokeOnMaster(() => {
				// Define variables and constants
				const uint WIDTH_TX = 512U;

				const uint WRITE_OFFSET = 30U;
				const int NUM_TX_TO_WRITE = (int) (WIDTH_TX - (WRITE_OFFSET * 2U));

				Texture1D<TexelFormat.RGBA32Int> srcTex = TextureFactory.NewTexture1D<TexelFormat.RGBA32Int>()
					.WithPermittedBindings(GPUBindings.ReadableShaderResource)
					.WithUsage(ResourceUsage.DiscardWrite)
					.WithWidth(WIDTH_TX);

				// Set up context


				// Execute
				srcTex.DiscardWrite(
					Enumerable.Range(0, NUM_TX_TO_WRITE)
						.Select(i => new TexelFormat.RGBA32Int { R = i, G = i * 2, B = i * 3, A = i * 4 })
						.ToArray(),
					0U,
					WRITE_OFFSET
					);

				Texture1D<TexelFormat.RGBA32Int> dstTex = srcTex.Clone()
					.WithUsage(ResourceUsage.StagingRead)
					.WithPermittedBindings(GPUBindings.None);

				srcTex.CopyTo(dstTex);

				// Assert outcome
				TexelArray1D<TexelFormat.RGBA32Int> copiedData = dstTex.Read(0U);
				for (int i = 0; i < NUM_TX_TO_WRITE; ++i) {
					Assert.AreEqual(i, copiedData[i + (int) WRITE_OFFSET].R);
					Assert.AreEqual(i * 2, copiedData[i + (int) WRITE_OFFSET].G);
					Assert.AreEqual(i * 3, copiedData[i + (int) WRITE_OFFSET].B);
					Assert.AreEqual(i * 4, copiedData[i + (int) WRITE_OFFSET].A);
				}

				srcTex.Dispose();
				dstTex.Dispose();
			});
		}

		[TestMethod]
		public void TestWrite() {
			LosgapSystem.InvokeOnMaster(() => {
				// Define variables and constants
				const uint WIDTH_TX = 512U;

				const uint TARGET_MIP_INDEX = 1U;
				const uint WRITE_OFFSET = 30U;
				const int NUM_TX_TO_WRITE = 198;

				Texture1D<TexelFormat.RGBA32Int> srcTex = TextureFactory.NewTexture1D<TexelFormat.RGBA32Int>()
					.WithPermittedBindings(GPUBindings.ReadableShaderResource)
					.WithMipAllocation(true)
					.WithDynamicDetail(true)
					.WithUsage(ResourceUsage.Write)
					.WithWidth(WIDTH_TX);

				// Set up context


				// Execute
				srcTex.Write(
					Enumerable.Range(0, NUM_TX_TO_WRITE)
						.Select(i => new TexelFormat.RGBA32Int { R = i, G = i * 2, B = i * 3, A = i * 4 })
						.ToArray(),
					TARGET_MIP_INDEX,
					WRITE_OFFSET
					);

				Texture1D<TexelFormat.RGBA32Int> dstTex = srcTex.Clone()
					.WithDynamicDetail(false)
					.WithUsage(ResourceUsage.StagingRead)
					.WithPermittedBindings(GPUBindings.None);

				srcTex.CopyTo(dstTex);

				// Assert outcome
				TexelArray1D<TexelFormat.RGBA32Int> copiedData = dstTex.Read(TARGET_MIP_INDEX);
				for (int i = 0; i < NUM_TX_TO_WRITE; ++i) {
					Assert.AreEqual(i, copiedData[i + (int) WRITE_OFFSET].R);
					Assert.AreEqual(i * 2, copiedData[i + (int) WRITE_OFFSET].G);
					Assert.AreEqual(i * 3, copiedData[i + (int) WRITE_OFFSET].B);
					Assert.AreEqual(i * 4, copiedData[i + (int) WRITE_OFFSET].A);
				}

				srcTex.Dispose();
				dstTex.Dispose();
			});
		}

		[TestMethod]
		public void TestReadAndReadAll() {
			// Define variables and constants
			const uint WIDTH_TX = 512U;

			Texture1D<TexelFormat.RGBA32Int> srcTex = TextureFactory.NewTexture1D<TexelFormat.RGBA32Int>()
				.WithInitialData(
					Enumerable.Range(0, (int) TextureUtils.GetSizeTexels(true, WIDTH_TX))
					.Select(i => new TexelFormat.RGBA32Int { R = i, G = i * 2, B = i * 3, A = i * 4 })
					.ToArray()
				)
				.WithMipAllocation(true)
				.WithPermittedBindings(GPUBindings.None)
				.WithUsage(ResourceUsage.StagingRead)
				.WithWidth(WIDTH_TX);

			TexelFormat.RGBA32Int[] readAllData = srcTex.ReadAll();
			for (int i = 0, curMipIndex = 0; i < readAllData.Length; ++curMipIndex) {
				var readData = srcTex.Read((uint) curMipIndex);
				for (int j = 0; j < readData.Width; ++j, ++i) {
					Assert.AreEqual(readData[j], readAllData[i]);
				}
			}

			srcTex.Dispose();
		}

		[TestMethod]
		public void TestReadWrite() {
			// Define variables and constants
			const uint WIDTH_TX = 512U;
			const uint TARGET_MIP_INDEX = 4U;
			const uint DATA_WRITE_OFFSET = 10U;
			const uint DATA_WRITE_COUNT = 10U;

			Texture1D<TexelFormat.RGBA32UInt> srcTex = TextureFactory.NewTexture1D<TexelFormat.RGBA32UInt>()
				.WithInitialData(
					Enumerable.Range(0, (int) TextureUtils.GetSizeTexels(true, WIDTH_TX))
					.Select(i => (uint) i)
					.Select(i => new TexelFormat.RGBA32UInt { R = i, G = i * 2, B = i * 3, A = i * 4 })
					.ToArray()
				)
				.WithMipAllocation(true)
				.WithPermittedBindings(GPUBindings.None)
				.WithUsage(ResourceUsage.StagingReadWrite)
				.WithWidth(WIDTH_TX);

			srcTex.ReadWrite(data => {
				for (uint i = DATA_WRITE_OFFSET; i < DATA_WRITE_OFFSET + DATA_WRITE_COUNT; ++i) {
					data[(int) i] = new TexelFormat.RGBA32UInt { R = i, G = i * 3, B = i * 6, A = i * 9 };
				}
			},
			TARGET_MIP_INDEX);

			var readData = srcTex.Read(TARGET_MIP_INDEX);
			for (uint i = DATA_WRITE_OFFSET; i < DATA_WRITE_OFFSET + DATA_WRITE_COUNT; ++i) {
				Assert.AreEqual(i, readData[(int) i].R);
				Assert.AreEqual(i * 3, readData[(int) i].G);
				Assert.AreEqual(i * 6, readData[(int) i].B);
				Assert.AreEqual(i * 9, readData[(int) i].A);
			}

			srcTex.Dispose();
		}
		#endregion
	}
}