// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 29 10 2014 at 15:14 by Ben Bowen

using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap.Rendering {
	[TestClass]
	public class IndexBufferTest {

		[TestInitialize]
		public void SetUp() { }


		#region Tests
		[TestMethod]
		public void TestClone() {
			// Define variables and constants
			const ResourceUsage USAGE = ResourceUsage.Write;
			const uint NUM_INDICES = 21515;
			IndexBuffer firstBuffer = BufferFactory.NewIndexBuffer().WithUsage(USAGE).WithLength(NUM_INDICES);

			// Set up context


			// Execute
			IndexBuffer secondBuffer = firstBuffer.Clone();
			IndexBuffer thirdBuffer = secondBuffer.Clone().WithLength(NUM_INDICES * 2U);

			// Assert outcome
			Assert.AreEqual(USAGE, secondBuffer.Usage);
			Assert.AreEqual(firstBuffer.Usage, secondBuffer.Usage);
			Assert.AreEqual(NUM_INDICES, secondBuffer.Length);
			Assert.AreEqual(firstBuffer.Length, secondBuffer.Length);

			Assert.AreEqual(USAGE, thirdBuffer.Usage);
			Assert.AreEqual(secondBuffer.Length * 2U, thirdBuffer.Length);

			firstBuffer.Dispose();
			secondBuffer.Dispose();
			thirdBuffer.Dispose();
		}

		[TestMethod]
		public void TestCopyTo() {
			// Define variables and constants
			IndexBuffer srcBuffer = BufferFactory.NewIndexBuffer()
				.WithUsage(ResourceUsage.Immutable)
				.WithInitialData(Enumerable.Range(0, 3000).Select(@int => (uint) @int).ToArray());
			IndexBuffer dstBuffer = srcBuffer.Clone()
				.WithUsage(ResourceUsage.DiscardWrite);

			// Set up context


			// Execute
			srcBuffer.CopyTo(dstBuffer);
			srcBuffer.CopyTo(dstBuffer, 100, 2000, 500);

			try {
				dstBuffer.CopyTo(srcBuffer);
				Assert.Fail();
			}
			catch (ResourceOperationUnavailableException) { }

#if !DEVELOPMENT && !RELEASE
			try {
				srcBuffer.CopyTo(dstBuffer, 1600, 2000, 500);
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }

			try {
				srcBuffer.CopyTo(dstBuffer, 0, 2000, 2000);
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
#endif

			// Assert outcome
			Assert.IsFalse(srcBuffer.CanBeCopyDestination);
			Assert.IsTrue(dstBuffer.CanBeCopyDestination);

			srcBuffer.Dispose();
			dstBuffer.Dispose();
		}

		[TestMethod]
		public void TestWrite() {
			// Define variables and constants
			IndexBuffer testBuffer = BufferFactory.NewIndexBuffer()
				.WithUsage(ResourceUsage.Write)
				.WithLength(300);

			// Set up context


			// Execute
			testBuffer.Write(Enumerable.Range(0, 300).Select(@int => (uint) @int).ToArray(), 0);
			testBuffer.Write(Enumerable.Range(0, 200).Select(@int => (uint) @int).ToArray(), 100);
			testBuffer.Write(new ArraySlice<uint>(Enumerable.Range(0, 200).Select(@int => (uint) @int).ToArray(), 50, 50), 24);

#if !DEVELOPMENT && !RELEASE
			try {
				testBuffer.Write(new ArraySlice<uint>(Enumerable.Range(0, 200).Select(@int => (uint) @int).ToArray(), 50), 250);
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
#endif

			// Assert outcome
			Assert.IsTrue(testBuffer.CanWrite);

			testBuffer.Dispose();
		}

		[TestMethod]
		public void TestDiscardWrite() {
			// Define variables and constants
			IndexBuffer testBuffer = BufferFactory.NewIndexBuffer()
				.WithUsage(ResourceUsage.DiscardWrite)
				.WithLength(300);

			// Set up context


			// Execute
			testBuffer.DiscardWrite(Enumerable.Range(0, 300).Select(@int => (uint) @int).ToArray(), 0);
			testBuffer.DiscardWrite(Enumerable.Range(0, 200).Select(@int => (uint) @int).ToArray(), 100);
			testBuffer.DiscardWrite(new ArraySlice<uint>(Enumerable.Range(0, 200).Select(@int => (uint) @int).ToArray(), 50, 50), 24);

#if !DEVELOPMENT && !RELEASE
			try {
				testBuffer.DiscardWrite(new ArraySlice<uint>(Enumerable.Range(0, 200).Select(@int => (uint) @int).ToArray(), 50), 250);
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
#endif

			// Assert outcome
			Assert.IsTrue(testBuffer.CanDiscardWrite);

			testBuffer.Dispose();
		}
		#endregion
	}
}