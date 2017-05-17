// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 24 02 2015 at 13:17 by Ben Bowen

using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap {
	[TestClass]
	public class MasterSlaveBarrierTest {

		[TestInitialize]
		public void SetUp() { }


		#region Tests
		[TestMethod]
		public void TestOpenAndClose() {
			MasterSlaveBarrier testBarrier = new MasterSlaveBarrier(2U);
			Thread slaveA = new Thread(testBarrier.SlaveWaitForFirstOpen);
			slaveA.Start();
			Thread slaveB = new Thread(testBarrier.SlaveWaitForFirstOpen);
			slaveB.Start();
			Assert.IsFalse(slaveA.Join(TimeSpan.FromSeconds(0.5d)));
			Assert.IsFalse(slaveB.Join(TimeSpan.FromSeconds(0.5d)));
			LosgapSystem.InvokeOnMaster(testBarrier.MasterOpenBarrier);
			slaveA.Join();
			slaveB.Join();

			slaveA = new Thread(testBarrier.SlaveWaitForReset);
			slaveA.Start();
			slaveB = new Thread(testBarrier.SlaveWaitForReset);

			bool barrierClosed = false;
			object barrierClosePulse = new object();
			LosgapSystem.InvokeOnMasterAsync(() => {
				testBarrier.MasterWaitForClose();
				testBarrier.MasterOpenBarrier();
				lock (barrierClosePulse) {
					barrierClosed = true;
					Monitor.Pulse(barrierClosePulse);
				}
			});

			Thread.Sleep(TimeSpan.FromSeconds(0.5d));
			Assert.IsFalse(barrierClosed);

			Monitor.Enter(barrierClosePulse);
			slaveB.Start();
			slaveA.Join();
			slaveB.Join();
			Monitor.Wait(barrierClosePulse);
			Monitor.Exit(barrierClosePulse);
			Assert.IsTrue(barrierClosed);

			testBarrier.Dispose();
		}

		[TestMethod]
		public void TestDispose() {
			MasterSlaveBarrier testBarrier = new MasterSlaveBarrier(2U);
			Thread slaveA = new Thread(testBarrier.SlaveWaitForFirstOpen);
			Thread slaveB = new Thread(testBarrier.SlaveWaitForFirstOpen);
			slaveA.Start();
			LosgapSystem.InvokeOnMaster(() => testBarrier.Dispose());
			slaveB.Start();

			slaveA.Join();
			slaveB.Join();

			testBarrier = new MasterSlaveBarrier(2U);
			slaveA = new Thread(() => {
				testBarrier.SlaveWaitForFirstOpen();
				testBarrier.SlaveWaitForReset();
			});
			slaveB = new Thread(() => {
				testBarrier.SlaveWaitForFirstOpen();
				testBarrier.SlaveWaitForReset();
			});

			slaveA.Start();
			slaveB.Start();

			LosgapSystem.InvokeOnMaster(() => {
				testBarrier.MasterOpenBarrier();
				testBarrier.MasterWaitForClose();
				testBarrier.Dispose();
			});

			slaveA.Join();
			slaveB.Join();
		}
		#endregion
	}
}