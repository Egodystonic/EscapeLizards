// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 06 10 2014 at 13:05 by Ben Bowen

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Ophidian.Losgap {
	internal sealed class PipelineProcessor : IDisposable {
		internal readonly ParallelizationProvider ParallelizationProvider;
		private readonly ILosgapModule[] modules;
		private readonly long[] lastTickTimes;
		private readonly Stopwatch pipelineTimer = Stopwatch.StartNew();
#if DEBUG
		private const double FRAME_TIMEOUT_MS = 4000d;
		private readonly System.Timers.Timer frameTimer = new System.Timers.Timer(FRAME_TIMEOUT_MS);
		private volatile ILosgapModule frameTimeoutCulprit;
#endif

		private volatile bool isDisposed = false;

		public PipelineProcessor(ILosgapModule[] modules) {
			if (modules == null) throw new ArgumentNullException("modules");
			if (modules.Length == 0) throw new ArgumentException("At least one module required to run pipeline.", "modules");

			this.modules = modules;
			lastTickTimes = new long[modules.Length];

			ParallelizationProvider = new ParallelizationProvider();

#if DEBUG
			frameTimer.AutoReset = false;
			frameTimer.Elapsed += (sender, args) => FrameTimeout();
#endif
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
			Justification = "We catch the exception here so we can re-raise it at the master-invocation callsite.")]
		public void InvokeOnMaster(PipelineMasterInvocation pmi) {
			if (isDisposed) throw new ObjectDisposedException("Can not invoke actions on master when pipeline is disposed!");
			if (Thread.CurrentThread == LosgapSystem.MasterThread) {
				try {
					pmi.Action();
				}
				catch (Exception e) {
					if (pmi.IsSynchronousCall) pmi.RaisedException = e;
					else LosgapSystem.ExitWithError("Exception raised in asynchronous pipeline master invocation.", e);
				}
			}
			else {
				if (pmi.IsSynchronousCall) {
					Monitor.Enter(pmi.InvocationCompleteMonitor);
					ParallelizationProvider.SlaveQueueOnMaster(pmi);
					Monitor.Wait(pmi.InvocationCompleteMonitor);
					Monitor.Exit(pmi.InvocationCompleteMonitor);
				}
				else ParallelizationProvider.SlaveQueueOnMaster(pmi);
			}
		}

		public void Dispose() {
			((IDisposable) ParallelizationProvider).Dispose();
			isDisposed = true;
		}

		//private static readonly Dictionary<ILosgapModule, int> frameCounts = new Dictionary<ILosgapModule, int>();
		//private static readonly Dictionary<ILosgapModule, double> cumulativeTickTimes = new Dictionary<ILosgapModule, double>();
		//private static Stopwatch timer = Stopwatch.StartNew();

		public void Start() {
			while (!isDisposed) {
#if DEBUG
				frameTimer.Start();
#endif
				for (int i = 0; i < modules.Length; ++i) {
#if DEBUG
					frameTimeoutCulprit = modules[i];
#endif
					long elapsedMs = pipelineTimer.ElapsedMilliseconds;
					long tickDeltaMs = elapsedMs - lastTickTimes[i];
					if (tickDeltaMs > modules[i].TickIntervalMs) {
						//if (!frameCounts.ContainsKey(modules[i])) {
						//	frameCounts.Add(modules[i], 0);
						//	cumulativeTickTimes.Add(modules[i], 0f);
						//}
						//var timeBefore = timer.Elapsed;
						modules[i].PipelineIterate(ParallelizationProvider, tickDeltaMs);
						lastTickTimes[i] = elapsedMs;
						//var iterateTime = (timer.Elapsed - timeBefore).TotalSeconds;
						//cumulativeTickTimes[modules[i]] += iterateTime;
						//if (++frameCounts[modules[i]] == 1000) {
						//	Logger.Log("TIMER: " + modules[i].GetType().Name + ": " + (cumulativeTickTimes[modules[i]] / 1000d) + "s avg");

						//	frameCounts[modules[i]] = 0;
						//	cumulativeTickTimes[modules[i]] = 0;
						//}
					}
				}

#if DEBUG
				frameTimer.Stop();
#endif

				ParallelizationProvider.MasterHydratePMIQueue();
			}

			ParallelizationProvider.WaitForSlavesToExit();
		}

#if DEBUG
		private void FrameTimeout() {
			Logger.Debug("Frame timeout (" + FRAME_TIMEOUT_MS + "ms) detected at " + frameTimeoutCulprit + "! Beginning pipeline thread traces...");
			ParallelizationProvider.PrintThreadTraces();
		}
#endif
	}
}