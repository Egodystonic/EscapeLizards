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
	public class Texture2DTest {
		#region Tests
		[TestMethod]
		public void TestClone() {
			// Define variables and constants
			const uint WIDTH_TX = 512U;
			const uint HEIGHT_TX = 1024U;
			Texture2D<TexelFormat.Int8> srcTex = TextureFactory.NewTexture2D<TexelFormat.Int8>()
				.WithDynamicDetail(false)
				.WithInitialData(
					Enumerable.Range(0, (int) TextureUtils.GetSizeTexels(true, WIDTH_TX, HEIGHT_TX))
					.Select(i => new TexelFormat.Int8 { Value = (sbyte) i })
					.ToArray()
				)
				.WithMipAllocation(true)
				.WithMipGenerationTarget(false)
				.WithPermittedBindings(GPUBindings.None)
				.WithUsage(ResourceUsage.StagingRead)
				.WithMultisampling(false)
				.WithWidth(WIDTH_TX)
				.WithHeight(HEIGHT_TX);

			// Set up context
			

			// Execute
			Texture2D<TexelFormat.Int8> destNoCopy = srcTex.Clone(false);
			Texture2D<TexelFormat.Int8> destCopy = srcTex.Clone(true);

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
			Assert.AreEqual(srcTex.IsMultisampled, destNoCopy.IsMultisampled);

			TexelFormat.Int8[] copiedData = destCopy.ReadAll();
			for (int i = 0; i < copiedData.Length; ++i) {
				Assert.AreEqual((sbyte) i, copiedData[i].Value);
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
				const uint HEIGHT_TX = 32U;

				const uint NUM_TEXELS_TO_COPY_PER_ROW = 25U;
				const uint FIRST_TEXEL_TO_COPY_IN_ROW = 25U;
				const uint NUM_ROWS_TO_COPY = 5U;
				const uint FIRST_ROW_TO_COPY = 1U;
				const uint SRC_MIP_INDEX = 0U;
				const uint DST_MIP_INDEX = 2U;
				const uint DST_WRITE_OFFSET_X = 15U;
				const uint DST_WRITE_OFFSET_Y = 2U;

				const float DATA_VALUE_START_R_ROW_ADDITION = (float) WIDTH_TX;
				const float DATA_VALUE_START_R = FIRST_TEXEL_TO_COPY_IN_ROW + DATA_VALUE_START_R_ROW_ADDITION * FIRST_ROW_TO_COPY;

				TexelFormat.RGBA32Float[] initialData = Enumerable.Range(0, (int) TextureUtils.GetSizeTexels(true, WIDTH_TX, HEIGHT_TX))
					.Select(i => new TexelFormat.RGBA32Float { R = (float) i, G = (float) i * 2f, B = (float) i * 4f, A = (float) i * 8f })
					.ToArray();

				Texture2D<TexelFormat.RGBA32Float> srcTex = TextureFactory.NewTexture2D<TexelFormat.RGBA32Float>()
					.WithDynamicDetail(false)
					.WithMultisampling(false)
					.WithInitialData(initialData)
					.WithMipAllocation(true)
					.WithMipGenerationTarget(false)
					.WithPermittedBindings(GPUBindings.ReadableShaderResource)
					.WithUsage(ResourceUsage.Immutable)
					.WithWidth(WIDTH_TX)
					.WithHeight(HEIGHT_TX);

				// Set up context

				// Execute
				Texture2D<TexelFormat.RGBA32Float> dstTex = srcTex.Clone()
					.WithUsage(ResourceUsage.StagingRead)
					.WithPermittedBindings(GPUBindings.None);
				SubresourceBox targetBox = new SubresourceBox(
					FIRST_TEXEL_TO_COPY_IN_ROW, FIRST_TEXEL_TO_COPY_IN_ROW + NUM_TEXELS_TO_COPY_PER_ROW,
					FIRST_ROW_TO_COPY, FIRST_ROW_TO_COPY + NUM_ROWS_TO_COPY
					);
				srcTex.CopyTo(
					dstTex,
					targetBox,
					SRC_MIP_INDEX,
					DST_MIP_INDEX,
					DST_WRITE_OFFSET_X,
					DST_WRITE_OFFSET_Y
					);

				// Assert outcome
				TexelArray2D<TexelFormat.RGBA32Float> copiedData = dstTex.Read(DST_MIP_INDEX);
				for (int v = 0; v < NUM_ROWS_TO_COPY; ++v) {
					for (int u = 0; u < NUM_TEXELS_TO_COPY_PER_ROW; ++u) {
						var thisTexel = copiedData[u + (int) DST_WRITE_OFFSET_X, v + (int) DST_WRITE_OFFSET_Y];
						Assert.AreEqual((float) (DATA_VALUE_START_R + DATA_VALUE_START_R_ROW_ADDITION * v + u), thisTexel.R);
						Assert.AreEqual((float) (DATA_VALUE_START_R + DATA_VALUE_START_R_ROW_ADDITION * v + u) * 2f, thisTexel.G);
						Assert.AreEqual((float) (DATA_VALUE_START_R + DATA_VALUE_START_R_ROW_ADDITION * v + u) * 4f, thisTexel.B);
						Assert.AreEqual((float) (DATA_VALUE_START_R + DATA_VALUE_START_R_ROW_ADDITION * v + u) * 8f, thisTexel.A);
					}
				}

				srcTex.Dispose();
				dstTex.Dispose();
			});
		}

		[TestMethod]
		public void TestDiscardWrite() {
			LosgapSystem.InvokeOnMaster(() => {
				// Define variables and constants
				const uint WIDTH_TX = 400U;
				const uint HEIGHT_TX = 200U;

				const uint WRITE_OFFSET_U = 190U;
				const uint WRITE_OFFSET_V = 10U;
				const int NUM_TX_TO_WRITE = 13555;

				Texture2D<TexelFormat.RGB32UInt> srcTex = TextureFactory.NewTexture2D<TexelFormat.RGB32UInt>()
					.WithPermittedBindings(GPUBindings.ReadableShaderResource)
					.WithUsage(ResourceUsage.DiscardWrite)
					.WithWidth(WIDTH_TX)
					.WithHeight(HEIGHT_TX);

				// Set up context


				// Execute
				srcTex.DiscardWrite(
					Enumerable.Range(0, NUM_TX_TO_WRITE)
						.Select(i => new TexelFormat.RGB32UInt { R = (uint) i, G = (uint) i * 2, B = (uint) i * 3 })
						.ToArray(),
					0U,
					WRITE_OFFSET_U,
					WRITE_OFFSET_V
					);

				Texture2D<TexelFormat.RGB32UInt> dstTex = srcTex.Clone()
					.WithUsage(ResourceUsage.StagingRead)
					.WithPermittedBindings(GPUBindings.None);


				srcTex.CopyTo(dstTex);

				// Assert outcome
				TexelFormat.RGB32UInt[] copiedData = dstTex.Read(0U).Data;
				int offset = (int) (WRITE_OFFSET_V * WIDTH_TX + WRITE_OFFSET_U);
				for (int i = 0; i < NUM_TX_TO_WRITE; ++i) {
					Assert.AreEqual((uint) i, copiedData[i + offset].R);
					Assert.AreEqual((uint) i * 2U, copiedData[i + offset].G);
					Assert.AreEqual((uint) i * 3U, copiedData[i + offset].B);
				}

				srcTex.Dispose();
				dstTex.Dispose();
			});
		}

		[TestMethod]
		public void TestWrite() {
			LosgapSystem.InvokeOnMaster(() => {
				// Define variables and constants
				const uint WIDTH_TX = 100U;
				const uint HEIGHT_TX = 100U;

				const uint WRITE_OFFSET_U = 30U;
				const uint WRITE_OFFSET_V = 30U;

				SubresourceBox writeTarget = new SubresourceBox(
					WRITE_OFFSET_U, WIDTH_TX,
					WRITE_OFFSET_V, HEIGHT_TX
					);

				Texture2D<TexelFormat.RGBA8Int> srcTex = TextureFactory.NewTexture2D<TexelFormat.RGBA8Int>()
					.WithUsage(ResourceUsage.Write)
					.WithMultisampling(true)
					.WithWidth(WIDTH_TX)
					.WithHeight(HEIGHT_TX);

				// Set up context


				// Execute
				srcTex.Write(
					Enumerable.Range(0, (int) writeTarget.Volume)
						.Select(i => new TexelFormat.RGBA8Int { R = (sbyte) i, G = (sbyte) (i * 2), B = (sbyte) (i * 3), A = (sbyte) (i * 4) })
						.ToArray(),
					writeTarget
					);

				Texture2D<TexelFormat.RGBA8Int> dstTex = srcTex.Clone()
					.WithUsage(ResourceUsage.StagingRead)
					.WithPermittedBindings(GPUBindings.None);

				srcTex.CopyTo(dstTex);

				// Assert outcome
				TexelArray2D<TexelFormat.RGBA8Int> copiedData = dstTex.Read(0U);
				for (uint v = WRITE_OFFSET_V, value = 0U; v < HEIGHT_TX; ++v) {
					for (uint u = WRITE_OFFSET_U; u < WIDTH_TX; ++u, ++value) {
						Assert.AreEqual((sbyte) value, copiedData[(int) u, (int) v].R);
						Assert.AreEqual((sbyte) (value * 2U), copiedData[(int) u, (int) v].G);
						Assert.AreEqual((sbyte) (value * 3U), copiedData[(int) u, (int) v].B);
						Assert.AreEqual((sbyte) (value * 4U), copiedData[(int) u, (int) v].A);
					}
				}

				srcTex.Dispose();
				dstTex.Dispose();
			});
		}

		[TestMethod]
		public void TestReadAndReadAll() {
			// Define variables and constants
			const uint WIDTH_TX = 512U;
			const uint HEIGHT_TX = 64U;

			Texture2D<TexelFormat.RGBA32Int> srcTex = TextureFactory.NewTexture2D<TexelFormat.RGBA32Int>()
				.WithInitialData(
					Enumerable.Range(0, (int) TextureUtils.GetSizeTexels(true, WIDTH_TX, HEIGHT_TX))
					.Select(i => new TexelFormat.RGBA32Int { R = i, G = i * 2, B = i * 3, A = i * 4 })
					.ToArray()
				)
				.WithMipAllocation(true)
				.WithPermittedBindings(GPUBindings.None)
				.WithUsage(ResourceUsage.StagingRead)
				.WithWidth(WIDTH_TX)
				.WithHeight(HEIGHT_TX);

			TexelFormat.RGBA32Int[] readAllData = srcTex.ReadAll();
			for (int i = 0, curMipIndex = 0; i < readAllData.Length; ++curMipIndex) {
				var readData = srcTex.Read((uint) curMipIndex);
				for (int v = 0; v < readData.Height; ++v) {
					for (int u = 0; u < readData.Width; ++u, ++i) {
						Assert.AreEqual(readData[u, v].R, readAllData[i].R);
						Assert.AreEqual(readData[u, v].G, readAllData[i].G);
						Assert.AreEqual(readData[u, v].B, readAllData[i].B);
						Assert.AreEqual(readData[u, v].A, readAllData[i].A);
					}
				}
			}

			srcTex.Dispose();
		}

		[TestMethod]
		public void TestReadWrite() {
			// Define variables and constants
			const uint WIDTH_TX = 512U;
			const uint HEIGHT_TX = 512U;
			const uint TARGET_MIP_INDEX = 2U;
			const uint DATA_WRITE_OFFSET_U = 10U;
			const uint DATA_WRITE_OFFSET_V = 10U;
			const uint DATA_WRITE_SQUARE_SIZE = 80U;

			Texture2D<TexelFormat.RGBA32UInt> srcTex = TextureFactory.NewTexture2D<TexelFormat.RGBA32UInt>()
				.WithInitialData(
					Enumerable.Range(0, (int) TextureUtils.GetSizeTexels(true, WIDTH_TX, HEIGHT_TX))
					.Select(i => (uint) i)
					.Select(i => new TexelFormat.RGBA32UInt { R = i, G = i * 2, B = i * 3, A = i * 4 })
					.ToArray()
				)
				.WithMipAllocation(true)
				.WithPermittedBindings(GPUBindings.None)
				.WithUsage(ResourceUsage.StagingReadWrite)
				.WithWidth(WIDTH_TX)
				.WithHeight(HEIGHT_TX);

			srcTex.ReadWrite(data => {
				for (uint u = DATA_WRITE_OFFSET_U; u < DATA_WRITE_OFFSET_U + DATA_WRITE_SQUARE_SIZE; ++u) {
					for (uint v = DATA_WRITE_OFFSET_V; v < DATA_WRITE_OFFSET_V + DATA_WRITE_SQUARE_SIZE; ++v) {
						data[(int) u, (int) v] = new TexelFormat.RGBA32UInt { R = u, G = v, B = u + v, A = u - v };
					}
				}
			}, 
			TARGET_MIP_INDEX);

			var readData = srcTex.Read(TARGET_MIP_INDEX);
			for (uint u = DATA_WRITE_OFFSET_U; u < DATA_WRITE_OFFSET_U + DATA_WRITE_SQUARE_SIZE; ++u) {
				for (uint v = DATA_WRITE_OFFSET_V; v < DATA_WRITE_OFFSET_V + DATA_WRITE_SQUARE_SIZE; ++v) {
					Assert.AreEqual(u, readData[(int) u, (int) v].R);
					Assert.AreEqual(v, readData[(int) u, (int) v].G);
					Assert.AreEqual(u + v, readData[(int) u, (int) v].B);
					Assert.AreEqual(u - v, readData[(int) u, (int) v].A);
				}
			}

			srcTex.Dispose();
		}
		#endregion
	}
}