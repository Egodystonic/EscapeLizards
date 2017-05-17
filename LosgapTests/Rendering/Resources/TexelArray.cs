// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 19 11 2014 at 15:40 by Ben Bowen

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap.Rendering {
	[TestClass]
	public class TexelArrayTest {

		[TestInitialize]
		public void SetUp() { }


		#region Tests
		[TestMethod]
		public unsafe void TestIndexAccessors() {
			// Define variables and constants
			const int WIDTH = 4;
			const int HEIGHT = 8;
			const int DEPTH = 2;
			TexelFormat.Int8[] data = new TexelFormat.Int8[WIDTH * HEIGHT * DEPTH];

			TexelArray1D<TexelFormat.Int8> array1D = new TexelArray1D<TexelFormat.Int8>(data);
			TexelArray2D<TexelFormat.Int8> array2D = new TexelArray2D<TexelFormat.Int8>(data, WIDTH);
			TexelArray3D<TexelFormat.Int8> array3D = new TexelArray3D<TexelFormat.Int8>(data, WIDTH, HEIGHT);

			// Set up context
			for (int i = 0; i < WIDTH * HEIGHT * DEPTH; ++i) {
				data[i].Value = (sbyte) i;
			}

			// Execute


			// Assert outcome
			Assert.AreEqual((sbyte) 3, array1D[3].Value);
			Assert.AreEqual((sbyte) 22, array2D[2, 5].Value);
			Assert.AreEqual((sbyte) 54, array3D[2, 5, 1].Value);
		}
		#endregion
	}
}