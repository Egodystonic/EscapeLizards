// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 17 11 2014 at 10:28 by Ben Bowen

using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap {
	[TestClass]
	public class ArraySliceTest {

		[TestInitialize]
		public void SetUp() { }


		#region Tests
		[TestMethod]
		public void TestConstructors() {
			// Define variables and constants
			int[] testArray = Enumerable.Range(0, 10).ToArray();

			// Set up context


			// Execute
			var fullSlice = new ArraySlice<int>(testArray);
			var offset3Slice = new ArraySlice<int>(testArray, 3);
			var offset3Length4Slice = new ArraySlice<int>(testArray, 3, 4);

#if !DEVELOPMENT && !RELEASE
			try {
				new ArraySlice<int>(null);
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
			try {
				new ArraySlice<int>(testArray, 40);
				Assert.Fail();
			}
			catch (OverflowException) { }
			try {
				new ArraySlice<int>(testArray, 2, 9);
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
#endif

			// Assert outcome
			Assert.IsTrue(fullSlice.IsFullArraySlice);
			Assert.IsFalse(offset3Slice.IsFullArraySlice);
			Assert.IsFalse(offset3Length4Slice.IsFullArraySlice);

			Assert.AreEqual((uint) testArray.Length, fullSlice.Length);
			Assert.AreEqual((uint) testArray.Length - 3, offset3Slice.Length);
			Assert.AreEqual((uint) 4, offset3Length4Slice.Length);

			Assert.AreEqual(0U, fullSlice.Offset);
			Assert.AreEqual(3U, offset3Slice.Offset);
			Assert.AreEqual(3U, offset3Length4Slice.Offset);
		}

		[TestMethod]
		public void TestEnumerator() {
			// Define variables and constants
			int[] testArray = Enumerable.Range(0, 10).ToArray();

			// Set up context
			var fullSlice = new ArraySlice<int>(testArray);
			var offset3Slice = new ArraySlice<int>(testArray, 3);
			var offset3Length4Slice = new ArraySlice<int>(testArray, 3, 4);

			// Execute
			int testResFullSlice = 0, testResOffset3Slice = 0, testResOffset3Length4Slice = 0;
			foreach (int i in fullSlice) {
				testResFullSlice += i;
			}
			foreach (int i in offset3Slice) {
				testResOffset3Slice += i;
			}
			foreach (int i in offset3Length4Slice) {
				testResOffset3Length4Slice += i;
			}

			// Assert outcome
			Assert.AreEqual(45, testResFullSlice);
			Assert.AreEqual(42, testResOffset3Slice);
			Assert.AreEqual(18, testResOffset3Length4Slice);
		}

		[TestMethod]
		public void TestCopyAndIndexer() {
			// Define variables and constants
			int[] testArray = Enumerable.Range(0, 10).ToArray();

			// Set up context
			var fullSlice = new ArraySlice<int>(testArray);
			var offset3Slice = new ArraySlice<int>(testArray, 3);
			var offset3Length4Slice = new ArraySlice<int>(testArray, 3, 4);

			// Execute
			int[] testResFullSlice, testResOffset3Slice, testResOffset3Length4Slice;
			testResFullSlice = fullSlice.CopySliceToNewArray();
			testResOffset3Slice = offset3Slice.CopySliceToNewArray();
			testResOffset3Length4Slice = offset3Length4Slice.CopySliceToNewArray();

			// Assert outcome
			Assert.AreEqual((int) fullSlice.Length, testResFullSlice.Length);
			Assert.AreEqual((int) offset3Slice.Length, testResOffset3Slice.Length);
			Assert.AreEqual((int) offset3Length4Slice.Length, testResOffset3Length4Slice.Length);

			for (int i = 0; i < fullSlice.Length; ++i) {
				Assert.AreEqual(fullSlice[i], testResFullSlice[i]);
			}
			for (int i = 0; i < offset3Slice.Length; ++i) {
				Assert.AreEqual(offset3Slice[i], testResOffset3Slice[i]);
			}
			for (int i = 0; i < offset3Length4Slice.Length; ++i) {
				Assert.AreEqual(offset3Length4Slice[i], testResOffset3Length4Slice[i]);
			}
		}
		#endregion
	}
}