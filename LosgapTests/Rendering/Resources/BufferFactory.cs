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
	public class BufferFactoryTest {

		[TestInitialize]
		public void SetUp() { }


		#region Tests
		[TestMethod]
		public void TestCreation() {
			// Define variables and constants
			BufferBuilder<int> defaultBufferBuilder = BufferFactory.NewBuffer<int>().WithLength(1);
			Buffer<int> bufferWithFrequentWrite = defaultBufferBuilder.WithUsage(ResourceUsage.DiscardWrite);
			Buffer<int> bufferWith300Ele = bufferWithFrequentWrite.Clone().WithLength(300);
			Buffer<int> bufferWithInitData = defaultBufferBuilder.WithInitialData(Enumerable.Range(0, 10).ToArray());
			Buffer<long> bufferWith200LongEle = bufferWithFrequentWrite.Clone().WithElementType<long>().WithLength(200);
			Buffer<int> bufferWithNoGPUBindings = bufferWithFrequentWrite.Clone()
				.WithPermittedBindings(GPUBindings.None)
				.WithUsage(ResourceUsage.StagingRead);

			// Set up context

			// Execute

			// Assert outcome
			Assert.AreEqual(ResourceUsage.DiscardWrite, bufferWithFrequentWrite.Usage);
			Assert.AreEqual(300L, bufferWith300Ele.Length);
			Assert.AreEqual(10L, bufferWithInitData.Length);
			Assert.AreEqual(200L, bufferWith200LongEle.Length);
			Assert.IsFalse(bufferWithInitData.IsStructured);
			Assert.IsTrue(bufferWith200LongEle.IsStructured);
			Assert.AreEqual(GPUBindings.None, bufferWithNoGPUBindings.PermittedBindings);

			Assert.AreNotEqual(bufferWithNoGPUBindings.PermittedBindings, bufferWithFrequentWrite.PermittedBindings);

#if !DEVELOPMENT && !RELEASE
			try {
				defaultBufferBuilder.WithUsage(ResourceUsage.StagingRead).WithPermittedBindings(GPUBindings.ReadableShaderResource).Create();
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
#endif

			try {
				defaultBufferBuilder.WithPermittedBindings(GPUBindings.RenderTarget);
				Assert.Fail();
			}
			catch (ArgumentException) { }

			try {
				defaultBufferBuilder.WithPermittedBindings(GPUBindings.DepthStencilTarget);
				Assert.Fail();
			}
			catch (ArgumentException) { }

			bufferWithFrequentWrite.Dispose();
			bufferWith300Ele.Dispose();
			bufferWithInitData.Dispose();
			bufferWith200LongEle.Dispose();
			bufferWithNoGPUBindings.Dispose();
		}
		#endregion
	}
}