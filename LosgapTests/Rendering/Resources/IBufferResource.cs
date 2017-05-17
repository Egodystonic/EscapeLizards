// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 04 11 2014 at 16:16 by Ben Bowen

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap.Rendering {
	[TestClass]
	public class IBufferResourceTest {
		#region Tests
		[TestMethod]
		public void TestDiscardWrite() {
			LosgapSystem.InvokeOnMaster(() => {
				// Define variables and constants
				IBuffer res = BufferFactory.NewBuffer<int>()
					.WithUsage(ResourceUsage.DiscardWrite)
					.WithLength(100)
					.Create();

				IBuffer copyDest = (res as Buffer<int>).Clone()
					.WithUsage(ResourceUsage.StagingRead)
					.WithPermittedBindings(GPUBindings.None)
					.Create();

				// Set up context


				// Execute
				Assert.IsTrue(res.CanDiscardWrite);
				res.DiscardWrite(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 10U);
				res.CopyTo(copyDest);

				// Assert outcome
				byte[] readData = copyDest.Read();
				for (int i = 10; i < 20; ++i) {
					Assert.AreEqual(i - 9, readData[i]);
				}

				res.Dispose();
				copyDest.Dispose();
			});
		}

		[TestMethod]
		public void TestWrite() {
			LosgapSystem.InvokeOnMaster(() => {
				// Define variables and constants
				IBuffer res = BufferFactory.NewBuffer<int>()
					.WithUsage(ResourceUsage.Write)
					.WithLength(100)
					.Create();

				IBuffer copyDest = (res as Buffer<int>).Clone()
					.WithUsage(ResourceUsage.StagingRead)
					.WithPermittedBindings(GPUBindings.None)
					.Create();

				// Set up context


				// Execute
				Assert.IsTrue(res.CanWrite);
				res.Write(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 10U);
				res.CopyTo(copyDest);

				// Assert outcome
				byte[] readData = copyDest.Read();
				for (int i = 10; i < 20; ++i) {
					Assert.AreEqual(i - 9, readData[i]);
				}

				res.Dispose();
				copyDest.Dispose();
			});
		}

		[TestMethod]
		public void TestReadWrite() {
			LosgapSystem.InvokeOnMaster(() => {
				// Define variables and constants
				IBuffer res = BufferFactory.NewBuffer<int>()
					.WithUsage(ResourceUsage.Write)
					.WithLength(100)
					.Create();

				IBuffer copyDest = (res as Buffer<int>).Clone()
					.WithUsage(ResourceUsage.StagingReadWrite)
					.WithPermittedBindings(GPUBindings.None)
					.Create();

				// Set up context
				Assert.IsTrue(res.CanWrite);
				res.Write(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 10U);
				res.CopyTo(copyDest);

				// Execute
				copyDest.ReadWrite(data => {
					for (int i = 0; i < data.Length; ++i) {
						data[i] *= 2;
					}
				});

				// Assert outcome
				byte[] readData = copyDest.Read();
				for (int i = 10; i < 20; ++i) {
					Assert.AreEqual((i - 9) * 2, readData[i]);
				}

				res.Dispose();
				copyDest.Dispose();
			});
		}
		#endregion
	}
}