// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 19 11 2014 at 15:40 by Ben Bowen

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap.Rendering {
	[TestClass]
	public class RawResourceDataViewTest {

		[TestInitialize]
		public void SetUp() { }


		#region Tests
		[TestMethod]
		public unsafe void TestIndexAccessors() {
			// Define variables and constants
			const int WIDTH = 4;
			const int HEIGHT = 8;
			const int DEPTH = 2;
			byte* ptr = stackalloc byte[WIDTH * HEIGHT * DEPTH];

			RawResourceDataView1D<byte> view1D = new RawResourceDataView1D<byte>((IntPtr) ptr, 1U, WIDTH);
			RawResourceDataView2D<byte> view2D = new RawResourceDataView2D<byte>((IntPtr) ptr, 1U, WIDTH, HEIGHT, WIDTH);
			RawResourceDataView3D<byte> view3D = new RawResourceDataView3D<byte>((IntPtr) ptr, 1U, WIDTH, HEIGHT, DEPTH, WIDTH, WIDTH * HEIGHT);

			// Set up context
			for (int i = 0; i < WIDTH * HEIGHT * DEPTH; ++i) {
				ptr[i] = (byte) i;
			}

			// Execute


			// Assert outcome
			Assert.AreEqual((byte) 3, view1D[3]);
			Assert.AreEqual((byte) 22, view2D[2, 5]);
			Assert.AreEqual((byte) 54, view3D[2, 5, 1]);
		}
		#endregion
	}
}