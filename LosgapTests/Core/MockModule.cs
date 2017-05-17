// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 07 10 2014 at 13:27 by Ben Bowen

using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

namespace Ophidian.Losgap {
	public sealed class MockModule : ILosgapModule {
		private readonly object syncObject = new object();
		private int preExecuteCalls = 0;
		private int postExecuteCalls = 0;
		private int[] workspace = new int[0];
		private readonly int execBlockSize;
		private readonly int tickRateMs;

		public event Action PostExec;
		public event Action ExecBlock;

		public int PreExecuteCalls {
			get {
				lock (syncObject) {
					return preExecuteCalls;
				}
			}
		}

		public int PostExecuteCalls {
			get {
				lock (syncObject) {
					return postExecuteCalls;
				}
			}
		}

		public int[] Workspace {
			get {
				lock (syncObject) {
					return workspace;
				}
			}
		}

		public MockModule(int execBlockSize, int tickRateMs) {
			this.execBlockSize = execBlockSize;
			this.tickRateMs = tickRateMs;
		}

		public void SetExecutionRange(int range) {
			lock (syncObject) {
				workspace = new int[range];
			}
		}

		public long TickIntervalMs {
			get { return MathUtils.MILLIS_IN_ONE_SECOND / tickRateMs; }
		}

		public void ModuleAdded() {
			
		}

		public void PipelineIterate(ParallelizationProvider parallelizationProvider, long deltaMs) {
			lock (syncObject) {
				++preExecuteCalls;
			}

			parallelizationProvider.Execute(workspace.Length, execBlockSize, ExecuteBlock);

			lock (syncObject) {
				++postExecuteCalls;
				Action postExecHandler = PostExec;
				if (postExecHandler != null) {
					postExecHandler();
				}
			}
		}

		private void ExecuteBlock(int execIndex) {
			try {
				int workingValue = execIndex + 1;
				workingValue /= 5;
				workingValue += execIndex * execIndex % ((workingValue / (execIndex + 1)) + 19);
				workingValue = (int) Math.Pow(workingValue, execIndex % 17);

				if (execIndex < 0) workspace[execIndex] = workingValue; // Will never happen, but I'm tricking the compiler / JIT to make sure it doesn't optimize it all away
				else workspace[execIndex] = execIndex + workspace[execIndex];

				if (ExecBlock != null) ExecBlock();
			}
			catch (Exception) {
				Logger.Fatal("Exec " + execIndex + " on " + Thread.CurrentThread.Name + " with " + workspace.Length);
				throw;
			}
		}
	}
}