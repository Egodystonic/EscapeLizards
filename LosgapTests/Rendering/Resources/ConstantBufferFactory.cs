// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 29 10 2014 at 09:20 by Ben Bowen

using System;
using System.Linq;
using System.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap.Rendering {
	[TestClass]
	public class ConstantBufferFactoryTest {

		private struct SixteenByteStruct {
			private float a, b, c, d;

			public SixteenByteStruct(float a, float b, float c, float d) {
				this.a = a;
				this.b = b;
				this.c = c;
				this.d = d;
			}
		}

		[TestInitialize]
		public void SetUp() { }


		#region Tests
		[TestMethod]
		public void TestCreation() {
			// Define variables and constants
			ConstantBuffer<SixteenByteStruct> defaultBuffer = BufferFactory.NewConstantBuffer<SixteenByteStruct>();
			ConstantBuffer<SixteenByteStruct> freqWriteBuffer = defaultBuffer.Clone().WithUsage(ResourceUsage.DiscardWrite);
			ConstantBuffer<SixteenByteStruct> initDataBuffer = defaultBuffer.Clone().WithInitialData(new SixteenByteStruct(1f, 2f, 3f, 4f));

			// Set up context

			// Execute

			// Assert outcome
			Assert.AreEqual(ResourceUsage.Immutable, defaultBuffer.Usage);
			Assert.AreEqual(ResourceUsage.DiscardWrite, freqWriteBuffer.Usage);
			Assert.AreEqual(defaultBuffer.Usage, initDataBuffer.Usage);
			Assert.AreEqual(16L, initDataBuffer.Size.InBytes);

			try {
				freqWriteBuffer.Clone().WithUsage(ResourceUsage.StagingRead);
				Assert.Fail();
			}
			catch (ArgumentException) { }

			defaultBuffer.Dispose();
			freqWriteBuffer.Dispose();
			initDataBuffer.Dispose();
		}
		#endregion
	}
}