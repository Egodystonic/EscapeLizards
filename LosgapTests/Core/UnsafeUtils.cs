// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 15 10 2014 at 14:01 by Ben Bowen

using System;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap {
	[TestClass]
	public unsafe class UnsafeUtilsTest {
#pragma warning disable 169
		private struct ZeroByteStruct {
			
		}
		private struct OneByteStruct {
			private byte a;
		}
		private struct FourByteStruct {
			private int a;
		}
		private struct EightByteStruct {
			private long a;
		}
		private struct ThirtyTwoByteStruct {
			private long a, b, c, d;
		}
		private struct SixtyFourByteStruct {
			private decimal a, b, c, d;
		}
		private struct OneTwentyEightByteStruct {
			private decimal a, b, c, d, e, f, g, h;
		}
#pragma warning restore 169

		[TestInitialize]
		public void SetUp() { }


		#region Tests
		[TestMethod]
		public void TestSizeOf() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			unsafe {
				Assert.AreEqual(sizeof(ZeroByteStruct), UnsafeUtils.SizeOf<ZeroByteStruct>());
				Assert.AreEqual(sizeof(OneByteStruct), UnsafeUtils.SizeOf<OneByteStruct>());
				Assert.AreEqual(sizeof(FourByteStruct), UnsafeUtils.SizeOf<FourByteStruct>());
				Assert.AreEqual(sizeof(EightByteStruct), UnsafeUtils.SizeOf<EightByteStruct>());
				Assert.AreEqual(sizeof(ThirtyTwoByteStruct), UnsafeUtils.SizeOf<ThirtyTwoByteStruct>());
				Assert.AreEqual(sizeof(SixtyFourByteStruct), UnsafeUtils.SizeOf<SixtyFourByteStruct>());
				Assert.AreEqual(sizeof(OneTwentyEightByteStruct), UnsafeUtils.SizeOf<OneTwentyEightByteStruct>());
			}
		}

		[TestMethod]
		public void TestReinterpret() {
			// Define variables and constants
			const long TICKS = 635489789912683304L;
			DateTime standardDT = new DateTime(TICKS);

			// Set up context


			// Execute


			// Assert outcome
			long dtAsLong = UnsafeUtils.Reinterpret<DateTime, long>(standardDT, sizeof(long));
			DateTime longAsDT = UnsafeUtils.Reinterpret<long, DateTime>(TICKS, sizeof(DateTime));

			Assert.AreEqual(TICKS, dtAsLong);
			Assert.AreEqual(standardDT, longAsDT);
		}

		[TestMethod]
		public void TestWriteAndReadGeneric() {
			// Define variables and constants
			const long TEST_VALUE = 4000L;
			long testLong = 0L;

			// Set up context


			// Execute
			UnsafeUtils.WriteGenericToPtr((IntPtr) (&testLong), TEST_VALUE, sizeof(long));

			// Assert outcome
			Assert.AreEqual(TEST_VALUE, testLong);
			Assert.AreEqual(testLong, UnsafeUtils.ReadGenericFromPtr<long>((IntPtr) (&testLong), sizeof(long)));
		}

		[TestMethod]
		public void TestArrayCopy() {
			// Define variables and constants
			int[] source = Enumerable.Range(0, 10).ToArray();
			byte* destinationA = stackalloc byte[sizeof(int) * source.Length];
			int[] destinationB = new int[source.Length];

			// Set up context


			// Execute
			UnsafeUtils.CopyGenericArray<int>(source, (IntPtr) destinationA, sizeof(int));
			UnsafeUtils.CopyGenericArray((IntPtr) destinationA, new ArraySlice<int>(destinationB, 4, 3), sizeof(int));

			// Assert outcome
			for (int i = 4; i < 4 + 3; ++i) {
				Assert.AreEqual(i - 4, destinationB[i]);
			}

			UnsafeUtils.CopyGenericArray<int>((IntPtr) destinationA, destinationB, sizeof(int));
			for (int i = 0; i < source.Length; ++i) {
				Assert.AreEqual(source[i], destinationB[i]);
				Assert.AreEqual(source[i], *((int*) (destinationA + i * sizeof(int))));
			}
		}

		[TestMethod]
		public void TestMemCopy() {
			// Define variables and constants
			const int NUM_INTS_TO_ALLOC = 300;
			int* src = (int*) Marshal.AllocHGlobal(sizeof(int) * NUM_INTS_TO_ALLOC);
			int* dest = (int*) Marshal.AllocHGlobal(sizeof(int) * NUM_INTS_TO_ALLOC);

			// Set up context
			for (int i = 0; i < NUM_INTS_TO_ALLOC; i++) {
				src[i] = i * 3;
			}

			// Execute
			UnsafeUtils.MemCopy((IntPtr) src, (IntPtr) dest, (NUM_INTS_TO_ALLOC / 2) * sizeof(int));
			for (int i = 0; i < NUM_INTS_TO_ALLOC / 2; i++) {
				Assert.AreEqual(i * 3, dest[i]);
			}

			// Assert outcome
			UnsafeUtils.MemCopy((IntPtr) dest, (IntPtr) (dest + NUM_INTS_TO_ALLOC / 2), (NUM_INTS_TO_ALLOC / 2) * sizeof(int));
			for (int i = NUM_INTS_TO_ALLOC / 2; i < (NUM_INTS_TO_ALLOC / 2) * 2; i++) {
				Assert.AreEqual((i - (NUM_INTS_TO_ALLOC / 2)) * 3, dest[i]);
			}

			Marshal.FreeHGlobal((IntPtr) src);
			Marshal.FreeHGlobal((IntPtr) dest);
		}

		[TestMethod]
		public void TestZeroMem() {
			// Define variables and constants
			const int NUM_INTS_TO_ALLOC = 300;
			int* dest = (int*) Marshal.AllocHGlobal(sizeof(int) * NUM_INTS_TO_ALLOC);

			// Set up context


			// Execute
			UnsafeUtils.ZeroMem((IntPtr) dest, NUM_INTS_TO_ALLOC * sizeof(int));
			for (int i = 0; i < NUM_INTS_TO_ALLOC; i++) {
				Assert.AreEqual(0, dest[i]);
				dest[i] = i;
			}

			// Assert outcome
			UnsafeUtils.ZeroMem((IntPtr) dest, NUM_INTS_TO_ALLOC * sizeof(int));
			for (int i = 0; i < NUM_INTS_TO_ALLOC; i++) {
				Assert.AreEqual(0, dest[i]);
			}
		}
		#endregion
	}
}