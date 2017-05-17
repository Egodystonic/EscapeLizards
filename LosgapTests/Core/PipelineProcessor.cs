// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 12 01 2015 at 15:05 by Ben Bowen

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap {
	[TestClass]
	public class PipelineProcessorTest {

		[TestInitialize]
		public void SetUp() { }


		#region Tests
		[TestMethod]
		public void ShouldRunModulesAtCorrectTickRate() {
			LosgapSystem.InvokeOnMaster(() => {
				const int MODULE_EXEC_LENGTH = 1;
				const int MODULE_BLOCK_SIZE = 1;
				const int MODULE_TICK_RATE_MS = 10;
				const int TEST_ERROR_MARGIN_NUM_TICKS = 5;
				ILosgapModule[] inputExecutionUnits = {
					new MockModule(MODULE_BLOCK_SIZE, MODULE_TICK_RATE_MS), 	
				};
				PipelineProcessor processor = new PipelineProcessor(inputExecutionUnits);

				// Set up context
				((MockModule) inputExecutionUnits[0]).SetExecutionRange(MODULE_EXEC_LENGTH);
				Task.Run(() => {
					Thread.Sleep(1000);
					processor.Dispose();
				});

				// Execute
				processor.Start();

				// Assert outcome
				Assert.IsTrue(((MockModule) inputExecutionUnits[0]).PostExecuteCalls <= MODULE_TICK_RATE_MS + TEST_ERROR_MARGIN_NUM_TICKS);
			});
		}

		[TestMethod]
		[Timeout(10000)]
		public void PMIShouldNotStallPipeline() {
			LosgapSystem.InvokeOnMaster(() => {
				// Define variables and constants
				const int MODULE_EXEC_LENGTH = 15;
				const int MODULE_BLOCK_SIZE = 1;
				const int MODULE_TICK_RATE_MS = 10;
				ILosgapModule[] inputExecutionUnits = {
					new MockModule(MODULE_BLOCK_SIZE, MODULE_TICK_RATE_MS), 	
				};
				PipelineProcessor processor = new PipelineProcessor(inputExecutionUnits);

				// Set up context
				((MockModule) inputExecutionUnits[0]).SetExecutionRange(MODULE_EXEC_LENGTH);
				((MockModule) inputExecutionUnits[0]).ExecBlock += () => {
					Thread.Sleep(15);
					processor.InvokeOnMaster(new PipelineMasterInvocation(() => Thread.Sleep(15), true));
				};
				((MockModule) inputExecutionUnits[0]).PostExec += processor.Dispose;

				// Execute
				processor.Start();

				// Assert outcome

			});
		} 
		#endregion
	}
}