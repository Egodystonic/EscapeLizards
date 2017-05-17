// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 16 02 2015 at 11:41 by Ben Bowen

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap {
	[TestClass]
	public class StateMutationBarrierTest {

		[TestInitialize]
		public void SetUp() { }


		#region Tests
		[TestMethod]
		public void TestBarrierMethods() {
			// Define variables and constants
			StateMutationBarrier testBarrier = new StateMutationBarrier();
			object lockObj = new object();

			// Set up context
			

			// Execute
			testBarrier.AcquirePermit().Dispose();
			var permit = testBarrier.AcquirePermit(withLock: lockObj);
			Assert.IsTrue(Monitor.IsEntered(lockObj));
			Assert.IsFalse(testBarrier.IsFrozen);
			permit.Dispose();
#if !DEVELOPMENT && !RELEASE
			try {
				permit.Dispose();
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
#endif

			testBarrier.FreezeMutations();
			Assert.IsTrue(testBarrier.IsFrozen);
			testBarrier.UnfreezeMutations();
			Assert.IsFalse(testBarrier.IsFrozen);
			Thread t = new Thread(testBarrier.FreezeMutations);
			t.Start();
			t.Join();
			Assert.IsTrue(testBarrier.IsFrozen);
			t = new Thread(() => {
				try {
					testBarrier.AcquirePermit();
					Assert.Fail();
				}
				catch (ThreadAbortException) {
					Thread.ResetAbort();
				}
			});
			t.Start();
			Thread.Sleep(500);
			t.Abort();

			// Assert outcome

		}
		#endregion
	}
}