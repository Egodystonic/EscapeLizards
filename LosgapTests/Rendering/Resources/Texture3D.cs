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
	public class Texture3DTest {
		#region Tests
		[TestMethod]
		public void TestClone() {
			// Define variables and constants
			const uint WIDTH_TX = 128U;
			const uint HEIGHT_TX = 256U;
			const uint DEPTH_TX = 16U;
			Texture3D<TexelFormat.Int8> srcTex = TextureFactory.NewTexture3D<TexelFormat.Int8>()
				.WithDynamicDetail(false)
				.WithInitialData(
					Enumerable.Range(0, (int) TextureUtils.GetSizeTexels(true, WIDTH_TX, HEIGHT_TX, DEPTH_TX))
					.Select(i => new TexelFormat.Int8 { Value = (sbyte) i })
					.ToArray()
				)
				.WithMipAllocation(true)
				.WithMipGenerationTarget(false)
				.WithPermittedBindings(GPUBindings.None)
				.WithUsage(ResourceUsage.StagingRead)
				.WithWidth(WIDTH_TX)
				.WithHeight(HEIGHT_TX)
				.WithDepth(DEPTH_TX);

			// Set up context
			

			// Execute
			Texture3D<TexelFormat.Int8> destNoCopy = srcTex.Clone(false);
			Texture3D<TexelFormat.Int8> destCopy = srcTex.Clone(true);

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

			TexelFormat.Int8[] copiedData = destCopy.ReadAll();
			for (int i = 0; i < copiedData.Length; ++i) {
				Assert.AreEqual((sbyte) i, copiedData[i].Value);
			}

			srcTex.Dispose();
			destNoCopy.Dispose();
			destCopy.Dispose();
		}

		[TestMethod]
		public void TestCopyTo() {
			LosgapSystem.InvokeOnMaster(() => {
				// Define variables and constants
				const uint WIDTH_TX = 256U;
				const uint HEIGHT_TX = 32U;
				const uint DEPTH_TX = 32U;

				const uint NUM_TEXELS_TO_COPY_PER_ROW = 25U;
				const uint FIRST_TEXEL_TO_COPY_IN_ROW = 25U;
				const uint NUM_ROWS_TO_COPY_PER_SLICE = 5U;
				const uint FIRST_ROW_TO_COPY_IN_SLICE = 1U;
				const uint NUM_SLICES_TO_COPY = 10U;
				const uint FIRST_SLICE_TO_COPY = 4U;
				const uint SRC_MIP_INDEX = 1U;
				const uint DST_MIP_INDEX = 1U;
				const uint DST_WRITE_OFFSET_X = 15U;
				const uint DST_WRITE_OFFSET_Y = 2U;
				const uint DST_WRITE_OFFSET_Z = 0U;

				const float DATA_VALUE_ADDITION_W = (float) (HEIGHT_TX >> 1) * (float) (WIDTH_TX >> 1);
				const float DATA_VALUE_ADDITION_V = (float) (WIDTH_TX >> 1);
				const float DATA_VALUE_START_R = 
					WIDTH_TX * HEIGHT_TX * DEPTH_TX
						+ FIRST_TEXEL_TO_COPY_IN_ROW 
						+ DATA_VALUE_ADDITION_V * FIRST_ROW_TO_COPY_IN_SLICE 
						+ DATA_VALUE_ADDITION_W * FIRST_SLICE_TO_COPY;

				TexelFormat.RGBA32Float[] initialData = Enumerable.Range(0, (int) TextureUtils.GetSizeTexels(true, WIDTH_TX, HEIGHT_TX, DEPTH_TX))
					.Select(i => new TexelFormat.RGBA32Float { R = (float) i, G = (float) i * 2f, B = (float) i * 4f, A = (float) i * 8f })
					.ToArray();

				Texture3D<TexelFormat.RGBA32Float> srcTex = TextureFactory.NewTexture3D<TexelFormat.RGBA32Float>()
					.WithDynamicDetail(false)
					.WithInitialData(initialData)
					.WithMipAllocation(true)
					.WithMipGenerationTarget(false)
					.WithPermittedBindings(GPUBindings.ReadableShaderResource)
					.WithUsage(ResourceUsage.Immutable)
					.WithWidth(WIDTH_TX)
					.WithHeight(HEIGHT_TX)
					.WithDepth(DEPTH_TX);

				// Set up context

				// Execute
				Texture3D<TexelFormat.RGBA32Float> dstTex = srcTex.Clone()
					.WithUsage(ResourceUsage.StagingRead)
					.WithPermittedBindings(GPUBindings.None);
				SubresourceBox targetBox = new SubresourceBox(
					FIRST_TEXEL_TO_COPY_IN_ROW, FIRST_TEXEL_TO_COPY_IN_ROW + NUM_TEXELS_TO_COPY_PER_ROW,
					FIRST_ROW_TO_COPY_IN_SLICE, FIRST_ROW_TO_COPY_IN_SLICE + NUM_ROWS_TO_COPY_PER_SLICE,
					FIRST_SLICE_TO_COPY, FIRST_SLICE_TO_COPY + NUM_SLICES_TO_COPY
					);
				srcTex.CopyTo(
					dstTex,
					targetBox,
					SRC_MIP_INDEX,
					DST_MIP_INDEX,
					DST_WRITE_OFFSET_X,
					DST_WRITE_OFFSET_Y,
					DST_WRITE_OFFSET_Z
					);

				// Assert outcome
				TexelArray3D<TexelFormat.RGBA32Float> copiedData = dstTex.Read(DST_MIP_INDEX);
				for (int w = 0; w < NUM_SLICES_TO_COPY; ++w) {
					for (int v = 0; v < NUM_ROWS_TO_COPY_PER_SLICE; ++v) {
						for (int u = 0; u < NUM_TEXELS_TO_COPY_PER_ROW; ++u) {
							var thisTexel = copiedData[u + (int) DST_WRITE_OFFSET_X, v + (int) DST_WRITE_OFFSET_Y, w + (int) DST_WRITE_OFFSET_Z];
							Assert.AreEqual((float) (DATA_VALUE_START_R + u + v * DATA_VALUE_ADDITION_V + w * DATA_VALUE_ADDITION_W), thisTexel.R);
							Assert.AreEqual((float) (DATA_VALUE_START_R + u + v * DATA_VALUE_ADDITION_V + w * DATA_VALUE_ADDITION_W) * 2f, thisTexel.G);
							Assert.AreEqual((float) (DATA_VALUE_START_R + u + v * DATA_VALUE_ADDITION_V + w * DATA_VALUE_ADDITION_W) * 4f, thisTexel.B);
							Assert.AreEqual((float) (DATA_VALUE_START_R + u + v * DATA_VALUE_ADDITION_V + w * DATA_VALUE_ADDITION_W) * 8f, thisTexel.A);
						}
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
				const uint DEPTH_TX = 1U;

				const int NUM_TX_TO_WRITE = (int) (WIDTH_TX * HEIGHT_TX * DEPTH_TX);

				Texture3D<TexelFormat.RGB32UInt> srcTex = TextureFactory.NewTexture3D<TexelFormat.RGB32UInt>()
					.WithPermittedBindings(GPUBindings.ReadableShaderResource)
					.WithUsage(ResourceUsage.DiscardWrite)
					.WithWidth(WIDTH_TX)
					.WithHeight(HEIGHT_TX)
					.WithDepth(DEPTH_TX);

				// Set up context


				// Execute
				srcTex.DiscardWrite(
					Enumerable.Range(0, NUM_TX_TO_WRITE)
						.Select(i => new TexelFormat.RGB32UInt { R = (uint) i, G = (uint) i * 2, B = (uint) i * 3 })
						.ToArray(),
					0U);

				Texture3D<TexelFormat.RGB32UInt> dstTex = srcTex.Clone()
					.WithUsage(ResourceUsage.StagingRead)
					.WithPermittedBindings(GPUBindings.None);

				srcTex.CopyTo(dstTex);

				// Assert outcome
				TexelFormat.RGB32UInt[] copiedData = dstTex.Read(0U).Data;
				for (int i = 0; i < NUM_TX_TO_WRITE; ++i) {
					Assert.AreEqual((uint) i, copiedData[i].R);
					Assert.AreEqual((uint) i * 2U, copiedData[i].G);
					Assert.AreEqual((uint) i * 3U, copiedData[i].B);
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
				const uint DEPTH_TX = 100U;

				const uint WRITE_OFFSET_U = 30U;
				const uint WRITE_OFFSET_V = 30U;
				const uint WRITE_OFFSET_W = 30U;

				SubresourceBox writeTarget = new SubresourceBox(
					WRITE_OFFSET_U, WIDTH_TX,
					WRITE_OFFSET_V, HEIGHT_TX,
					WRITE_OFFSET_W, DEPTH_TX
					);

				Texture3D<TexelFormat.RGBA8Int> srcTex = TextureFactory.NewTexture3D<TexelFormat.RGBA8Int>()
					.WithUsage(ResourceUsage.Write)
					.WithWidth(WIDTH_TX)
					.WithHeight(HEIGHT_TX)
					.WithDepth(DEPTH_TX);

				// Set up context


				// Execute
				var initData = Enumerable.Range(0, (int) writeTarget.Volume)
					.Select(i => new TexelFormat.RGBA8Int { R = (sbyte) i, G = (sbyte) (i * 2), B = (sbyte) (i * 3), A = (sbyte) (i * 4) })
					.ToArray();
				srcTex.Write(
					initData,
					writeTarget
					);

				Texture3D<TexelFormat.RGBA8Int> dstTex = srcTex.Clone()
					.WithUsage(ResourceUsage.StagingRead)
					.WithPermittedBindings(GPUBindings.None);

				srcTex.CopyTo(dstTex);

				// Assert outcome
				TexelArray3D<TexelFormat.RGBA8Int> copiedData = dstTex.Read(0U);
				for (uint w = WRITE_OFFSET_W, value = 0U; w < DEPTH_TX; ++w) {
					for (uint v = WRITE_OFFSET_V; v < HEIGHT_TX; ++v) {
						for (uint u = WRITE_OFFSET_U; u < WIDTH_TX; ++u, ++value) {
							Assert.AreEqual((sbyte) value, copiedData[(int) u, (int) v, (int) w].R);
							Assert.AreEqual((sbyte) (value * 2U), copiedData[(int) u, (int) v, (int) w].G);
							Assert.AreEqual((sbyte) (value * 3U), copiedData[(int) u, (int) v, (int) w].B);
							Assert.AreEqual((sbyte) (value * 4U), copiedData[(int) u, (int) v, (int) w].A);
						}
					}
				}

				srcTex.Dispose();
				dstTex.Dispose();
			});
		}

		[TestMethod]
		public void TestReadAndReadAll() {
			// Define variables and constants
			const uint WIDTH_TX = 32U;
			const uint HEIGHT_TX = 16U;
			const uint DEPTH_TX = 16U;

			Texture3D<TexelFormat.RGBA32Int> srcTex = TextureFactory.NewTexture3D<TexelFormat.RGBA32Int>()
				.WithInitialData(
					Enumerable.Range(0, (int) TextureUtils.GetSizeTexels(true, WIDTH_TX, HEIGHT_TX, DEPTH_TX))
					.Select(i => new TexelFormat.RGBA32Int { R = i, G = i * 2, B = i * 3, A = i * 4 })
					.ToArray()
				)
				.WithMipAllocation(true)
				.WithPermittedBindings(GPUBindings.None)
				.WithUsage(ResourceUsage.StagingRead)
				.WithWidth(WIDTH_TX)
				.WithHeight(HEIGHT_TX)
				.WithDepth(DEPTH_TX);

			TexelFormat.RGBA32Int[] readAllData = srcTex.ReadAll();
			for (int i = 0, curMipIndex = 0; i < readAllData.Length; ++curMipIndex) {
				var readData = srcTex.Read((uint) curMipIndex);
				for (int w = 0; w < readData.Depth; ++w) {
					for (int v = 0; v < readData.Height; ++v) {
						for (int u = 0; u < readData.Width; ++u, ++i) {
							Assert.AreEqual(readData[u, v, w].R, readAllData[i].R);
							Assert.AreEqual(readData[u, v, w].G, readAllData[i].G);
							Assert.AreEqual(readData[u, v, w].B, readAllData[i].B);
							Assert.AreEqual(readData[u, v, w].A, readAllData[i].A);
						}
					}
				}
			}

			srcTex.Dispose();
		}

		[TestMethod]
		public void TestReadWrite() {
			// Define variables and constants
			const uint WIDTH_TX = 32U;
			const uint HEIGHT_TX = 32U;
			const uint DEPTH_TX = 32U;
			const uint TARGET_MIP_INDEX = 0U;
			const uint DATA_WRITE_OFFSET_U = 10U;
			const uint DATA_WRITE_OFFSET_V = 10U;
			const uint DATA_WRITE_OFFSET_W = 10U;
			const uint DATA_WRITE_CUBE_SIZE = 10U;

			Texture3D<TexelFormat.RGBA32UInt> srcTex = TextureFactory.NewTexture3D<TexelFormat.RGBA32UInt>()
				.WithInitialData(
					Enumerable.Range(0, (int) TextureUtils.GetSizeTexels(true, WIDTH_TX, HEIGHT_TX, DEPTH_TX))
					.Select(i => (uint) i)
					.Select(i => new TexelFormat.RGBA32UInt { R = i, G = i * 2, B = i * 3, A = i * 4 })
					.ToArray()
				)
				.WithMipAllocation(true)
				.WithPermittedBindings(GPUBindings.None)
				.WithUsage(ResourceUsage.StagingReadWrite)
				.WithWidth(WIDTH_TX)
				.WithHeight(HEIGHT_TX)
				.WithDepth(DEPTH_TX);

			srcTex.ReadWrite(data => {
				for (uint u = DATA_WRITE_OFFSET_U; u < DATA_WRITE_OFFSET_U + DATA_WRITE_CUBE_SIZE; ++u) {
					for (uint v = DATA_WRITE_OFFSET_V; v < DATA_WRITE_OFFSET_V + DATA_WRITE_CUBE_SIZE; ++v) {
						for (uint w = DATA_WRITE_OFFSET_W; w < DATA_WRITE_OFFSET_W + DATA_WRITE_CUBE_SIZE; ++w) {
							data[(int) u, (int) v, (int) w] = new TexelFormat.RGBA32UInt { R = w + u, G = w - v, B = w + u + v, A = w - u - v };
						}
					}
				}
			}, 
			TARGET_MIP_INDEX);

			var readData = srcTex.Read(TARGET_MIP_INDEX);
			for (uint u = DATA_WRITE_OFFSET_U; u < DATA_WRITE_OFFSET_U + DATA_WRITE_CUBE_SIZE; ++u) {
				for (uint v = DATA_WRITE_OFFSET_V; v < DATA_WRITE_OFFSET_V + DATA_WRITE_CUBE_SIZE; ++v) {
					for (uint w = DATA_WRITE_OFFSET_W; w < DATA_WRITE_OFFSET_W + DATA_WRITE_CUBE_SIZE; ++w) {
						Assert.AreEqual(w + u, readData[(int) u, (int) v, (int) w].R);
						Assert.AreEqual(w - v, readData[(int) u, (int) v, (int) w].G);
						Assert.AreEqual(w + u + v, readData[(int) u, (int) v, (int) w].B);
						Assert.AreEqual(w - u - v, readData[(int) u, (int) v, (int) w].A);
					}
				}
			}

			srcTex.Dispose();
		}
		#endregion
	}
}