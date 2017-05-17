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
	public class IndexBufferFactoryTest {

		[TestInitialize]
		public void SetUp() { }


		#region Tests
		[TestMethod]
		public void TestCreation() {
			// Define variables and constants
			IndexBufferBuilder defaultBuilder = BufferFactory.NewIndexBuffer().WithLength(1);
			IndexBufferBuilder builderWithFrequentWrite = defaultBuilder.WithUsage(ResourceUsage.DiscardWrite);
			IndexBufferBuilder builderWith300Indices = builderWithFrequentWrite.WithLength(300);
			IndexBufferBuilder builderWithInitData = defaultBuilder
				.WithInitialData(Enumerable.Range(0, 10).Select(@int => (uint) @int).ToArray());

			// Set up context


			// Execute
			IndexBuffer withFreqWriteBuffer = builderWithFrequentWrite;
			IndexBuffer with300IndexBuffer = builderWith300Indices;
			IndexBuffer withInitDataBuffer = builderWithInitData;

			// Assert outcome
			Assert.AreEqual(ResourceUsage.DiscardWrite, withFreqWriteBuffer.Usage);
			Assert.AreEqual(300U, with300IndexBuffer.Length);
			Assert.AreEqual(10U, withInitDataBuffer.Length);

			try {
				builderWithInitData.WithUsage(ResourceUsage.StagingRead);
				Assert.Fail();
			}
			catch (ArgumentException) { }

			withFreqWriteBuffer.Dispose();
			with300IndexBuffer.Dispose();
			withInitDataBuffer.Dispose();
		}
		#endregion
	}
}