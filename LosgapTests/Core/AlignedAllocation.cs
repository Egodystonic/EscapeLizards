// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 19 12 2014 at 11:00 by Ben Bowen

using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ophidian.Losgap.Interop;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap {
	[TestClass]
	public class AlignedAllocationTest {

		[TestInitialize]
		public void SetUp() { }


		#region Tests
		[TestMethod]
		public void TestAlignment() {
			// Define variables and constants
			const int NUM_ALLOCATIONS_PER_TEST_ALIGNMENT = 350;
			int[] testAlignments = { 1, 2, 3, 4, 6, 8, 12, 16, 32 };

			// Set up context


			// Execute


			// Assert outcome
			for (int i = 0; i < testAlignments.Length; i++) {
				for (int j = 0; j < NUM_ALLOCATIONS_PER_TEST_ALIGNMENT; j++) {
					AlignedAllocation<Matrix> alignedAlloc = new AlignedAllocation<Matrix>(testAlignments[i]);
					Assert.IsTrue(((long) alignedAlloc.AlignedPointer) % testAlignments[i] == 0L);
					alignedAlloc.Dispose();
				}
			}
		}

		[TestMethod]
		public void TestReadAndWrite() {
			// Define variables and constants
			const int NUM_ALLOCS = 200;
			AlignedAllocation<Matrix>[] allocs = new AlignedAllocation<Matrix>[NUM_ALLOCS];

			// Set up context
			for (int i = 0; i < allocs.Length; i++) {
				allocs[i] = new AlignedAllocation<Matrix>(16L);
			}

			// Execute
			for (int i = 0; i < allocs.Length; i++) {
				allocs[i].Write(new Matrix(
					i, i * 2, i * 3, i * 4, 
					i * 11, i * 12, i * 13, i * 14, 
					i * 21, i * 22, i * 23, i * 24,
					i * 31, i * 32, i * 33, i * 34
				));
			}

			// Assert outcome
			for (int i = 0; i < allocs.Length; i++) {
				Assert.AreEqual(new Matrix(
					i, i * 2, i * 3, i * 4,
					i * 11, i * 12, i * 13, i * 14,
					i * 21, i * 22, i * 23, i * 24,
					i * 31, i * 32, i * 33, i * 34
				), allocs[i]);
			}

			allocs.ForEach(alloc => alloc.Dispose());
		}

		[TestMethod]
		public unsafe void TestAllocArray() {
			// Define variables and constants
			const int NUM_ALLOCS = 2000;
			const long ALLOC_ALIGNMENT = 2L;

			// Set up context
			AlignedAllocation<Vector4> alignedArray = AlignedAllocation<Vector4>.AllocArray(ALLOC_ALIGNMENT, NUM_ALLOCS);
			
			// Execute
			*((Vector4*) (alignedArray.AlignedPointer + NUM_ALLOCS - 1)) = new Vector4(1, 2, 3, 4);

			// Assert outcome
			Assert.AreEqual(new Vector4(1, 2, 3, 4), *((Vector4*) (alignedArray.AlignedPointer + NUM_ALLOCS - 1)));
			Assert.AreEqual(0L, (long) alignedArray.AlignedPointer % ALLOC_ALIGNMENT);

			alignedArray.Dispose();
		}
		#endregion
	}
}