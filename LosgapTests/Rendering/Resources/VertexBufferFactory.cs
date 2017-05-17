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
	public class VertexBufferFactoryTest {

		[TestInitialize]
		public void SetUp() { }


		#region Tests
		[TestMethod]
		public void TestCreation() {
			// Define variables and constants
			VertexBufferBuilder<int> defaultBuilder = BufferFactory.NewVertexBuffer<int>().WithLength(1);
			VertexBufferBuilder<int> builderWithFrequentWrite = defaultBuilder.WithUsage(ResourceUsage.DiscardWrite);
			VertexBufferBuilder<int> builderWith300Verts = builderWithFrequentWrite.WithLength(300);
			VertexBufferBuilder<int> builderWithInitData = defaultBuilder.WithInitialData(Enumerable.Range(0, 10).ToArray());
			VertexBufferBuilder<long> builderWith200LongVert = builderWithFrequentWrite.WithVertexType<long>().WithLength(200);

			// Set up context


			// Execute
			VertexBuffer<int> withFreqWriteBuffer = builderWithFrequentWrite;
			VertexBuffer<int> with300VertBuffer = builderWith300Verts;
			VertexBuffer<int> withInitDataBuffer = builderWithInitData;
			VertexBuffer<long> with200LongBuffer = builderWith200LongVert;

			// Assert outcome
			Assert.AreEqual(ResourceUsage.DiscardWrite, withFreqWriteBuffer.Usage);
			Assert.AreEqual(300U, with300VertBuffer.Length);
			Assert.AreEqual(10U, withInitDataBuffer.Length);
			Assert.AreEqual(200U * sizeof(long), with200LongBuffer.Size.InBytes);

			try {
				builderWithInitData.WithUsage(ResourceUsage.StagingRead);
				Assert.Fail();
			}
			catch (ArgumentException) { }

			withFreqWriteBuffer.Dispose();
			with300VertBuffer.Dispose();
			withInitDataBuffer.Dispose();
			with200LongBuffer.Dispose();
		}
		#endregion
	}
}