// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 19 11 2014 at 15:55 by Ben Bowen

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap.Rendering {
	[TestClass]
	public class TextureUtilsTest {

		[TestInitialize]
		public void SetUp() { }


		#region Tests
		[TestMethod]
		public void TestGetNumMips() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(6U, TextureUtils.GetNumMips(32U));
			Assert.AreEqual(1U, TextureUtils.GetNumMips(1U));
			Assert.AreEqual(6U, TextureUtils.GetNumMips(1U, 32U));
			Assert.AreEqual(8U, TextureUtils.GetNumMips(64U, 32U, 128U));
		}

		[TestMethod]
		public void TestGetSubresourceIndex() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(0U, TextureUtils.GetSubresourceIndex(1U, 0U, 0U));
			Assert.AreEqual(2U, TextureUtils.GetSubresourceIndex(3U, 2U, 0U));
			Assert.AreEqual(14U, TextureUtils.GetSubresourceIndex(3U, 2U, 4U));
			Assert.AreEqual(39U, TextureUtils.GetSubresourceIndex(9U, 3U, 4U));
		}

		[TestMethod]
		public void TestGetDimensionsForMipLevel() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(128U, TextureUtils.GetDimensionForMipLevel(128U, 0U));
			Assert.AreEqual(32U, TextureUtils.GetDimensionForMipLevel(128U, 2U));
			Assert.AreEqual(1U, TextureUtils.GetDimensionForMipLevel(128U, 100000U));
			Assert.AreEqual(500U, TextureUtils.GetDimensionForMipLevel(500U, 0U));
		}

		[TestMethod]
		public void TestGetSize() {
			// Define variables and constants


			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(1000U, TextureUtils.GetSize(1U, false, 10U, 10U, 10U));
			Assert.AreEqual(4000U, TextureUtils.GetSize(4U, false, 10U, 10U, 10U));
			Assert.AreEqual(16384U, TextureUtils.GetSize(4U, false, 16U, 16U, 16U));
			Assert.AreEqual(9364U, TextureUtils.GetSize(4U, true, 16U, 16U, 8U));
		}

		[TestMethod]
		public void TestGetSizeTexels() {
			// Define variables and constants


			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(1000U, TextureUtils.GetSizeTexels(false, 10U, 10U, 10U));
			Assert.AreEqual(1000U, TextureUtils.GetSizeTexels(false, 10U, 10U, 10U));
			Assert.AreEqual(4096U, TextureUtils.GetSizeTexels(false, 16U, 16U, 16U));
			Assert.AreEqual(2341U, TextureUtils.GetSizeTexels(true, 16U, 16U, 8U));
		}
		#endregion
	}
}