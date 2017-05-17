// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 19 11 2014 at 13:05 by Ben Bowen

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap.Rendering {
	[TestClass]
	public class InitialResourceDataDescTest {

		[TestInitialize]
		public void SetUp() { }


		#region Tests
		[TestMethod]
		public unsafe void TestCreation() {
			// Define variables and constants
			const uint NUM_TEXTURES = 10U;
			const uint TEXEL_SIZE_BYTES = 16;
			const uint TEX_WIDTH = 512U;
			const uint TEX_HEIGHT = 128U;
			const uint TEX_DEPTH = 32U;
			uint numMipsPerTex = TextureUtils.GetNumMips(TEX_WIDTH, TEX_HEIGHT, TEX_DEPTH);
			IntPtr mockDataStart = new IntPtr(100);

			// Set up context


			// Execute
			InitialResourceDataDesc[] initialDataArray = InitialResourceDataDesc.CreateDataArr(
				mockDataStart,
				NUM_TEXTURES,
				numMipsPerTex,
				TEX_WIDTH,
				TEX_HEIGHT,
				TEX_DEPTH,
				TEXEL_SIZE_BYTES
			);

			// Assert outcome
			Assert.AreEqual((int) (NUM_TEXTURES * numMipsPerTex), initialDataArray.Length);
			for (uint tex = 0U; tex < NUM_TEXTURES; ++tex) {
				for (uint mip = 0U; mip < numMipsPerTex; ++mip) {
					InitialResourceDataDesc thisDesc = initialDataArray[numMipsPerTex * tex + mip];
					IntPtr expectedDataStart = mockDataStart;
					expectedDataStart += (int) (TextureUtils.GetSize(TEXEL_SIZE_BYTES, true, TEX_WIDTH, TEX_HEIGHT, TEX_DEPTH) * tex);
					for (uint i = 0U; i < mip; ++i) {
						uint mipWidth = TextureUtils.GetDimensionForMipLevel(TEX_WIDTH, i);
						uint mipHeight = TextureUtils.GetDimensionForMipLevel(TEX_HEIGHT, i);
						uint mipDepth = TextureUtils.GetDimensionForMipLevel(TEX_DEPTH, i);
						expectedDataStart += (int) TextureUtils.GetSize(TEXEL_SIZE_BYTES, false, mipWidth, mipHeight, mipDepth);
					}
					
					Assert.AreEqual(expectedDataStart, thisDesc.Data);
					Assert.AreEqual(TextureUtils.GetDimensionForMipLevel(TEX_WIDTH, mip) * TEXEL_SIZE_BYTES, thisDesc.DataRowStrideBytes);
					Assert.AreEqual(
						TextureUtils.GetDimensionForMipLevel(TEX_WIDTH, mip) 
						* TextureUtils.GetDimensionForMipLevel(TEX_HEIGHT, mip) 
						* TEXEL_SIZE_BYTES, 
						thisDesc.DataSliceStrideBytes
					);
				}
			}
		}
		#endregion
	}
}