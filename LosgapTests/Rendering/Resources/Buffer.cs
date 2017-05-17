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
	public class BufferTest {

		#region Tests
		[TestMethod]
		public void TestClone() {
			// Define variables and constants
			const ResourceUsage USAGE = ResourceUsage.StagingReadWrite;
			const uint LENGTH = 21515;
			const GPUBindings GPU_BINDINGS = GPUBindings.None;
			int[] initialData = new int[LENGTH];

			// Set up context
			for (int i = 0; i < initialData.Length; i++) {
				initialData[i] = i * 3;
			}

			Buffer<int> firstBuffer = BufferFactory.NewBuffer<int>()
				.WithUsage(USAGE)
				.WithLength(LENGTH)
				.WithPermittedBindings(GPU_BINDINGS)
				.WithInitialData(initialData);

			// Execute
			Buffer<int> secondBuffer = firstBuffer.Clone();
			Buffer<long> thirdBuffer = secondBuffer.Clone().WithElementType<long>();
			Buffer<int> fourthBuffer = firstBuffer.Clone(true);

			// Assert outcome
			Assert.AreEqual(USAGE, secondBuffer.Usage);
			Assert.AreEqual(firstBuffer.Usage, secondBuffer.Usage);
			Assert.AreEqual(LENGTH, secondBuffer.Length);
			Assert.AreEqual(firstBuffer.Length, secondBuffer.Length);
			Assert.AreEqual(GPU_BINDINGS, secondBuffer.PermittedBindings);
			Assert.AreEqual(firstBuffer.PermittedBindings, secondBuffer.PermittedBindings);
			Assert.AreEqual(false, secondBuffer.IsStructured);
			Assert.AreEqual(firstBuffer.IsStructured, secondBuffer.IsStructured);

			Assert.AreEqual(USAGE, thirdBuffer.Usage);
			Assert.AreEqual(secondBuffer.Length, thirdBuffer.Length);
			Assert.AreEqual(secondBuffer.Size * 2U, thirdBuffer.Size);

			int[] firstBufferData = firstBuffer.Read();
			int[] fourthBufferData = fourthBuffer.Read();
			for (int i = 0; i < initialData.Length; i++) {
				Assert.AreEqual(initialData[i], firstBufferData[i]);
				Assert.AreEqual(firstBufferData[i], fourthBufferData[i]);
			}

			firstBuffer.Dispose();
			secondBuffer.Dispose();
			thirdBuffer.Dispose();
			fourthBuffer.Dispose();
		}

		[TestMethod]
		public void TestCopyTo() {
			LosgapSystem.InvokeOnMaster(() => {
				// Define variables and constants
				const uint B_COPY_FIRST_ELEMENT = 100U;
				const uint B_COPY_NUM_ELEMENTS = 2000U;
				const uint B_COPY_DEST_OFFSET = 500U;

				Buffer<float> srcBuffer = BufferFactory.NewBuffer<float>()
					.WithUsage(ResourceUsage.Immutable)
					.WithInitialData(Enumerable.Range(0, 3000).Select(@int => (float) (@int * 3)).ToArray());
				Buffer<float> dstBufferA = srcBuffer.Clone()
					.WithUsage(ResourceUsage.StagingRead)
					.WithPermittedBindings(GPUBindings.None);
				Buffer<float> dstBufferB = dstBufferA.Clone();

				// Set up context

				// Execute
				srcBuffer.CopyTo(dstBufferA);
				srcBuffer.CopyTo(dstBufferB, B_COPY_FIRST_ELEMENT, B_COPY_NUM_ELEMENTS, B_COPY_DEST_OFFSET);

				try {
					dstBufferA.CopyTo(srcBuffer);
					Assert.Fail();
				}
				catch (ResourceOperationUnavailableException) { }

#if !DEVELOPMENT && !RELEASE
			try {
				srcBuffer.CopyTo(dstBufferA, 1600, 2000, 500);
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }

			try {
				srcBuffer.CopyTo(dstBufferB, 0, 2000, 2000);
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
#endif

				// Assert outcome
				float[] dstBufferAData = dstBufferA.Read();
				for (int i = 0; i < dstBufferA.Length; i++) {
					Assert.AreEqual(i * 3, dstBufferAData[i]);
				}

				float[] dstBufferBData = dstBufferB.Read();
				for (uint i = B_COPY_DEST_OFFSET; i < B_COPY_DEST_OFFSET + B_COPY_NUM_ELEMENTS; ++i) {
					Assert.AreEqual((i + B_COPY_FIRST_ELEMENT - B_COPY_DEST_OFFSET) * 3, dstBufferBData[i]);
				}

				Assert.IsFalse(srcBuffer.CanBeCopyDestination);
				Assert.IsTrue(dstBufferA.CanBeCopyDestination);
				Assert.IsTrue(dstBufferB.CanBeCopyDestination);

				srcBuffer.Dispose();
				dstBufferA.Dispose();
				dstBufferB.Dispose();
			});
		}

		[TestMethod]
		public void TestWrite() {
			LosgapSystem.InvokeOnMaster(() => {
				// Define variables and constants
				Buffer<int> testBuffer = BufferFactory.NewBuffer<int>()
					.WithUsage(ResourceUsage.Write)
					.WithLength(300);
				Buffer<int> testCopyDest = testBuffer.Clone()
					.WithUsage(ResourceUsage.StagingRead)
					.WithPermittedBindings(GPUBindings.None);

				Buffer<decimal> testBuffer2 = testBuffer.Clone()
					.WithUsage(ResourceUsage.StagingReadWrite)
					.WithPermittedBindings(GPUBindings.None)
					.WithElementType<decimal>();

				// Set up context


				// Execute
				testBuffer.Write(Enumerable.Range(0, 300).ToArray(), 0);
				testBuffer.CopyTo(testCopyDest);
				int[] copiedData = testCopyDest.Read();
				for (int i = 0; i < copiedData.Length; ++i) {
					Assert.AreEqual(i, copiedData[i]);
				}

				testBuffer.Write(Enumerable.Range(0, 200).ToArray(), 100);
				testBuffer.CopyTo(testCopyDest);
				copiedData = testCopyDest.Read();
				for (int i = 0; i < 100; ++i) {
					Assert.AreEqual(i, copiedData[i]);
				}
				for (int i = 100; i < 300; ++i) {
					Assert.AreEqual(i - 100, copiedData[i]);
				}

				testBuffer.Write(new ArraySlice<int>(Enumerable.Range(0, 200).ToArray(), 50, 50), 24);
				testBuffer.CopyTo(testCopyDest);
				copiedData = testCopyDest.Read();
				for (int i = 24; i < 24 + 50; ++i) {
					Assert.AreEqual(i + 26, copiedData[i]);
				}

#if !DEVELOPMENT && !RELEASE
			try {
				testBuffer.Write(new ArraySlice<int>(Enumerable.Range(0, 200).ToArray(), 50), 250);
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
#endif

				testBuffer2.Write(new decimal[] { 3m, 6m, 9m }, 101);
				decimal[] readData = testBuffer2.Read();
				for (int i = 101; i < 1 - 4; ++i) {
					Assert.AreEqual((i - 100) * 3m, readData[i]);
				}

				// Assert outcome
				Assert.IsTrue(testBuffer.CanWrite);
				Assert.IsTrue(testBuffer2.CanWrite);

				testBuffer.Dispose();
				testCopyDest.Dispose();
				testBuffer2.Dispose();
			});
		}

		[TestMethod]
		public void TestDiscardWrite() {
			LosgapSystem.InvokeOnMaster(() => {
				// Define variables and constants
				Buffer<decimal> testBuffer = BufferFactory.NewBuffer<decimal>()
					.WithUsage(ResourceUsage.DiscardWrite)
					.WithLength(300);
				Buffer<decimal> copyDestBuffer = testBuffer.Clone()
					.WithUsage(ResourceUsage.StagingRead)
					.WithPermittedBindings(GPUBindings.None);

				// Set up context


				// Execute
				testBuffer.DiscardWrite(Enumerable.Range(0, 300).Select(i => (decimal) i).ToArray(), 0);
				testBuffer.DiscardWrite(Enumerable.Range(0, 200).Select(i => (decimal) i).ToArray(), 100);
				testBuffer.DiscardWrite(new ArraySlice<decimal>(Enumerable.Range(0, 200).Select(i => (decimal) i).ToArray(), 50, 50), 24);

#if !DEVELOPMENT && !RELEASE
			try {
				testBuffer.DiscardWrite(new ArraySlice<decimal>(Enumerable.Range(0, 200).Select(i => (decimal) i).ToArray(), 50), 250);
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
#endif

				testBuffer.CopyTo(copyDestBuffer);
				decimal[] testData = copyDestBuffer.Read();
				for (int i = 24; i < 24 + 50; ++i) {
					Assert.AreEqual((decimal) (50 + (i - 24)), testData[i]);
				}

				// Assert outcome
				Assert.IsTrue(testBuffer.CanDiscardWrite);
				Assert.IsFalse(copyDestBuffer.CanDiscardWrite);

				testBuffer.Dispose();
				copyDestBuffer.Dispose();
			});
		}

		[TestMethod]
		public void TestReadWrite() {
			// Define variables and constants
			Buffer<long> testBuffer = BufferFactory.NewBuffer<long>()
				.WithLength(300)
				.WithUsage(ResourceUsage.StagingReadWrite)
				.WithPermittedBindings(GPUBindings.None);

			// Set up context
			testBuffer.Write(Enumerable.Range(0, 300).Select(i => (long) i).ToArray(), 0);

			// Execute
			testBuffer.ReadWrite(dataView => {
				for (int i = 0; i < dataView.Width; ++i) {
					dataView[i] *= 2L;
				}	
			});

			// Assert outcome
			long[] data = testBuffer.Read();
			for (int i = 0; i < data.Length; ++i) {
				Assert.AreEqual(i * 2, data[i]);
			}

			testBuffer.Dispose();
		}
		#endregion
	}
}