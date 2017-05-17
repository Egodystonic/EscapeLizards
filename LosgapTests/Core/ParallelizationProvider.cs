// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 07 10 2014 at 13:27 by Ben Bowen

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap {
	[TestClass]
	public class ParallelizationProviderTest {
		#region Tests
		[TestMethod]
		[Timeout(5000)]
		public void SingleModuleSingleIterationTest() {
			LosgapSystem.InvokeOnMaster(() => { // Must invoke on master because the PP has assurances that the master thread is invoking operations
				// Define variables and constants
				const int MODULE_EXEC_LENGTH = 500021;
				const int MODULE_BLOCK_SIZE = 200;
				ILosgapModule[] inputExecutionUnits = {
					new MockModule(MODULE_BLOCK_SIZE, Int32.MaxValue), 	
				};
				ParallelizationProvider pp = new ParallelizationProvider();

				// Set up context
				((MockModule) inputExecutionUnits[0]).SetExecutionRange(MODULE_EXEC_LENGTH);
				((MockModule) inputExecutionUnits[0]).PostExec += () => {
					int[] workspace = ((MockModule) inputExecutionUnits[0]).Workspace;
					for (int i = 0; i < MODULE_EXEC_LENGTH; ++i) {
						Assert.AreEqual(i, workspace[i]);
					}
					((IDisposable) pp).Dispose();
				};

				// Execute
				inputExecutionUnits[0].PipelineIterate(pp, 0L);

				// Assert outcome
			});
		}

		[TestMethod]
		public void MultipleModuleMultipleIterationTest() {
			LosgapSystem.InvokeOnMaster(() => { // Must invoke on master because the PP has assurances that the master thread is invoking operations
				// Define variables and constants
				const int NUM_ITERATIONS = 500;
				int[] moduleExecLengths = {
					136,
					88433,
					287,
					1
				};
				int[] moduleBlockSizes = {
					313,
					5000,
					2,
					1000
				};
				ILosgapModule[] inputExecutionUnits = moduleBlockSizes.Select(mbs => new MockModule(mbs, Int32.MaxValue)).ToArray();
				for (int i = 0; i < moduleExecLengths.Length; i++) {
					((MockModule) inputExecutionUnits[i]).SetExecutionRange(moduleExecLengths[i]);
				}
				ParallelizationProvider pp = new ParallelizationProvider();

				// Set up context

				// Execute
				for (int i = 0; i < NUM_ITERATIONS; i++) {
					foreach (ILosgapModule module in inputExecutionUnits) {
						module.PipelineIterate(pp, 0L);
					}
				}

				// Assert outcome
				for (int i = 0; i < inputExecutionUnits.Length; i++) {
					MockModule executionUnit = (MockModule) inputExecutionUnits[i];
					Assert.IsTrue(executionUnit.PostExecuteCalls == NUM_ITERATIONS);
					int[] workspace = executionUnit.Workspace;
					for (int w = 0; w < workspace.Length; ++w) {
						Assert.AreEqual(executionUnit.PostExecuteCalls * w, workspace[w]);
					}
				}
			});
		}

		[TestMethod]
		public void TestInvokeOnAll() {
			LosgapSystem.InvokeOnMaster(() => { // Must invoke on master because the PP has assurances that the master thread is invoking operations
				// Define variables and constants
				const int NUM_ITERATIONS = 10000;
				HashSet<Thread> invokedThreads = new HashSet<Thread>();
				ParallelizationProvider pp = new ParallelizationProvider();

				// Set up context


				// Execute
				for (int i = 0; i < NUM_ITERATIONS; i++) {
					invokedThreads.Clear();
					pp.InvokeOnAll(() => {
						lock (invokedThreads) {
							invokedThreads.Add(Thread.CurrentThread);
						}
					}, true);
					Assert.AreEqual(pp.NumThreads, (uint) invokedThreads.Count);
					invokedThreads.Clear();
					pp.InvokeOnAll(() => {
						lock (invokedThreads) {
							invokedThreads.Add(Thread.CurrentThread);
						}
					}, false);
					Assert.AreEqual(pp.NumThreads - 1U, (uint) invokedThreads.Count);
					Assert.IsFalse(invokedThreads.Contains(Thread.CurrentThread));
				}

				// Assert outcome

			});
		}

		[TestMethod]
		public void TestForceSingleThreaded() {
			LosgapSystem.InvokeOnMaster(() => { // Must invoke on master because the PP has assurances that the master thread is invoking operations
				const int NUM_ITERATIONS = 10000;
				HashSet<Thread> invokedThreads = new HashSet<Thread>();
				ParallelizationProvider pp = new ParallelizationProvider();

				for (int i = 0; i < NUM_ITERATIONS; i++) {
					pp.ForceSingleThreadedMode = true;
					invokedThreads.Clear();
					pp.InvokeOnAll(() => {
						lock (invokedThreads) {
							invokedThreads.Add(Thread.CurrentThread);
						}
					}, true);
					Assert.AreEqual(LosgapSystem.MasterThread, invokedThreads.Single());
					invokedThreads.Clear();
					pp.Execute(100, 1, atomic => {
						lock (invokedThreads) {
							invokedThreads.Add(Thread.CurrentThread);
						}
					});
					Assert.AreEqual(LosgapSystem.MasterThread, invokedThreads.Single());
					pp.ForceSingleThreadedMode = false;
				}

			});
		}

		[TestMethod]
		public void TestWaitForSlavesToExit() {
			LosgapSystem.InvokeOnMaster(() => { // Must invoke on master because the PP has assurances that the master thread is invoking operations
				const int NUM_ITERATIONS = 300;

				for (int i = 0; i < NUM_ITERATIONS; ++i) {
					ParallelizationProvider pp = new ParallelizationProvider();
					pp.Execute(10, 1, x => {
						if (x > 10) Console.WriteLine("This will never be called.");
					});
					(pp as IDisposable).Dispose();
					pp.WaitForSlavesToExit();
				}
			});
		}
		#endregion
	}
}