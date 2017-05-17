// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 29 10 2014 at 15:14 by Ben Bowen

using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap.Rendering {
	[TestClass]
	public class ConstantBufferTest {

		[TestInitialize]
		public void SetUp() { }


		#region Tests
		[TestMethod]
		public void TestClone() {
			// Define variables and constants
			const ResourceUsage USAGE = ResourceUsage.DiscardWrite;
			ConstantBuffer<decimal> firstBuffer = BufferFactory.NewConstantBuffer<decimal>().WithUsage(USAGE);

			// Set up context


			// Execute
			ConstantBuffer<decimal> secondBuffer = firstBuffer.Clone();
			ConstantBuffer<decimal> thirdBuffer = secondBuffer.Clone().WithInitialData(10516m);

			// Assert outcome
			Assert.AreEqual(USAGE, secondBuffer.Usage);
			Assert.AreEqual(firstBuffer.Usage, secondBuffer.Usage);
			Assert.AreEqual(firstBuffer.Size, secondBuffer.Size);

			Assert.AreEqual(USAGE, thirdBuffer.Usage);

			firstBuffer.Dispose();
			secondBuffer.Dispose();
			thirdBuffer.Dispose();
		}

		[TestMethod]
		public void TestCopyTo() {
			// Define variables and constants
			ConstantBuffer<decimal> srcBuffer = BufferFactory.NewConstantBuffer<decimal>()
				.WithUsage(ResourceUsage.Immutable)
				.WithInitialData(13515m);
			ConstantBuffer<decimal> dstBuffer = srcBuffer.Clone()
				.WithUsage(ResourceUsage.DiscardWrite);

			// Set up context


			// Execute
			srcBuffer.CopyTo(dstBuffer);
			srcBuffer.CopyTo(dstBuffer);

			try {
				dstBuffer.CopyTo(srcBuffer);
				Assert.Fail();
			}
			catch (ResourceOperationUnavailableException) { }

			// Assert outcome
			Assert.IsFalse(srcBuffer.CanBeCopyDestination);
			Assert.IsTrue(dstBuffer.CanBeCopyDestination);

			srcBuffer.Dispose();
			dstBuffer.Dispose();
		}

		[TestMethod]
		public void TestDiscardWrite() {
			// Define variables and constants
			ConstantBuffer<decimal> testBuffer = BufferFactory.NewConstantBuffer<decimal>()
				.WithUsage(ResourceUsage.DiscardWrite);

			// Set up context


			// Execute
			testBuffer.DiscardWrite(15m);

			// Assert outcome
			Assert.IsTrue(testBuffer.CanDiscardWrite);

			testBuffer.Dispose();
		}
		#endregion
	}
}