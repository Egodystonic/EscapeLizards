// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 09 01 2015 at 14:29 by Ben Bowen

using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

namespace Ophidian.Losgap {
	/// <summary>
	/// Provides a way for <see cref="ILosgapModule"/>s to execute code across all cores of the host machine; using an architecture
	/// designed to facilitate using near-100% of CPU resources.
	/// </summary>
	public sealed class ParallelizationProvider : IDisposable {
		private const int MAX_SLAVE_THREAD_DEATH_WAIT_MS = 4000;

		/// <summary>
		/// The number of threads (including the master/invoking thread)
		/// </summary>
		public readonly uint NumThreads;
		internal readonly MasterSlaveBarrier WorkBarrier;
		private readonly object slaveThreadExitMonitor = new object();
#if DEBUG
		private readonly Thread[] slaveThreads;
#endif
		private uint numSlavesStillRunning;
		/// <summary>
		/// While <c>true</c>, this provider will only invoke actions and/or execute on the <see cref="LosgapSystem.MasterThread"/>.
		/// Defaults to <c>false</c>. This can be used to limit non-parallelizable modules to execution on a single thread, but it is
		/// not a replacement for writing thread-safe code.
		/// </summary>
		public volatile bool ForceSingleThreadedMode = false;
		private volatile bool isDisposed = false;
		private bool currentWorkIsInvokeAllAction;
		private Action<int> currentAction;
		private Action currentInvokeAllAction;
		private int currentBlockSize = 0;
		private int numReservableBlocks = 0;


		internal ParallelizationProvider() {
			NumThreads = (uint) (Environment.ProcessorCount + LosgapSystem.ThreadOversubscriptionFactor);
			if (NumThreads > LosgapSystem.MaxThreadCount) NumThreads = LosgapSystem.MaxThreadCount;

			numSlavesStillRunning = NumThreads - 1U;
			WorkBarrier = new MasterSlaveBarrier(numSlavesStillRunning);

#if DEBUG
			slaveThreads = new Thread[NumThreads - 1];
			for (int i = 0; i < NumThreads - 1; ++i) {
				slaveThreads[i] = new Thread(StartSlave) {
					Priority = LosgapSystem.SlaveThreadPriority,
					Name = "LosgapSlave_" + i,
					IsBackground = true,
					CurrentCulture = new CultureInfo("en-US")
				};
				slaveThreads[i].Start();
			}
#else
			for (int i = 0; i < NumThreads - 1; ++i) {
				new Thread(StartSlave) {
					Priority = LosgapSystem.SlaveThreadPriority,
					Name = "LosgapSlave_" + i,
					IsBackground = true,
					CurrentCulture = new CultureInfo("en-US")
				}.Start();
			}
#endif

		}

		/// <summary>
		/// Invokes the given <paramref name="atomicAction"/> multiple times from multiple threads.
		/// </summary>
		/// <remarks>This method can only be invoked from the <see cref="LosgapSystem.MasterThread"/>.</remarks>
		/// <param name="numAtomics">The number of times <paramref name="atomicAction"/> should be invoked.</param>
		/// <param name="blockSize">The number of invocations of <paramref name="atomicAction"/> that should be executed
		/// serially by a single thread before that thread looks for more work. Too-low values may cause false sharing and
		/// high pipeline-state contention. Too-high values may cause unfair load-balancing, resulting in an underused CPU.</param>
		/// <param name="atomicAction">The action to invoke <paramref name="numAtomics"/> times. The only argument is the
		/// invocation counter, which is unique for each invocation, and ranges from <c>0</c> to <c>numAtomics - 1</c>.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2", 
			Justification = "Checked by assurance.")]
		public void Execute(int numAtomics, int blockSize, Action<int> atomicAction) {
			Assure.Equal(Thread.CurrentThread, LosgapSystem.MasterThread, "Execution of parallel processor should be from master thread.");
			Assure.NotNull(atomicAction);

			currentAction = atomicAction;
			currentBlockSize = blockSize;
			int numFullBlocks = numAtomics / blockSize;
			numReservableBlocks = numFullBlocks;
			currentWorkIsInvokeAllAction = false;

			bool singleThreadedMode = ForceSingleThreadedMode;
			if (!singleThreadedMode) WorkBarrier.MasterOpenBarrier();

			// Do the 'odd-sized' ending block
			for (int i = blockSize * numFullBlocks; i < numAtomics; ++i) atomicAction(i);

			for (int blockIndex = Interlocked.Decrement(ref numReservableBlocks);
						blockIndex >= 0;
						blockIndex = Interlocked.Decrement(ref numReservableBlocks)) {
				int blockStartInc = currentBlockSize * blockIndex;
				int blockEndEx = currentBlockSize * (blockIndex + 1);

				for (int i = blockStartInc; i < blockEndEx; ++i) currentAction(i);
			}

			if (!singleThreadedMode) WorkBarrier.MasterWaitForClose();
		}

		/// <summary>
		/// Invokes the given <paramref name="action"/> on all slave threads, and also the master thread (invoking thread) if
		/// <paramref name="includeMaster"/> is <c>true</c>.
		/// </summary>
		/// <remarks>This method can only be invoked from the <see cref="LosgapSystem.MasterThread"/>.</remarks>
		/// <param name="action">The action to invoke on all threads. Must not be null.</param>
		/// <param name="includeMaster">True if this action should be executed by the calling (master) thread as well.</param>
		public void InvokeOnAll(Action action, bool includeMaster) {
			Assure.Equal(Thread.CurrentThread, LosgapSystem.MasterThread, "Invocation of parallel processor should be from master thread.");
			Assure.NotNull(action);

			currentInvokeAllAction = action;
			currentWorkIsInvokeAllAction = true;

			bool singleThreadedMode = ForceSingleThreadedMode;
			if (!singleThreadedMode) WorkBarrier.MasterOpenBarrier();
			if (includeMaster) currentInvokeAllAction();
			if (!singleThreadedMode) WorkBarrier.MasterWaitForClose();
		}

		/// <summary>
		/// Stops all threads, allowing them to finish and be garbage collected.
		/// </summary>
		void IDisposable.Dispose() {
			isDisposed = true;
			WorkBarrier.Dispose();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
			Justification = "Rethrowing the exception would be useless here, as ExitWithError is called instead.")]
		private void StartSlave() {
			try {
				WorkBarrier.SlaveWaitForFirstOpen();
				while (!isDisposed) {
					if (currentWorkIsInvokeAllAction) {
						currentInvokeAllAction();
					}
					else {
						for (int blockIndex = Interlocked.Decrement(ref numReservableBlocks);
							blockIndex >= 0;
							blockIndex = Interlocked.Decrement(ref numReservableBlocks)) {
							int blockStartInc = currentBlockSize * blockIndex;
							int blockEndEx = currentBlockSize * (blockIndex + 1);

							for (int i = blockStartInc; i < blockEndEx; ++i) currentAction(i);
						}
					}

					WorkBarrier.SlaveWaitForReset();
				}

				Monitor.Enter(slaveThreadExitMonitor);
				if (--numSlavesStillRunning == 0) Monitor.Pulse(slaveThreadExitMonitor);
				Monitor.Exit(slaveThreadExitMonitor);
				Logger.Debug("Slave thread " + Thread.CurrentThread.Name + " has exited normally.");
			}
			catch (ThreadAbortException) {
				Thread.ResetAbort();
			}
			catch (Exception e) {
				LosgapSystem.ExitWithError("Slave thread encounted an unhandled exception.", e);
			}
		}

		internal void SlaveQueueOnMaster(PipelineMasterInvocation pmi) {
			WorkBarrier.SlaveQueueOnMaster(pmi);
		}

		internal void MasterHydratePMIQueue() {
			WorkBarrier.MasterHydratePMIQueue();
		}

		internal void WaitForSlavesToExit() {
			Monitor.Enter(slaveThreadExitMonitor);
			if (numSlavesStillRunning == 0) {
				Monitor.Exit(slaveThreadExitMonitor);
				return;
			}
			
			bool waitSuccess = Monitor.Wait(slaveThreadExitMonitor, MAX_SLAVE_THREAD_DEATH_WAIT_MS);
			Monitor.Exit(slaveThreadExitMonitor);
			if (!waitSuccess) {
				Logger.Warn("One or more slave threads did not exit cleanly within " + MAX_SLAVE_THREAD_DEATH_WAIT_MS + "ms.");
			}
		}

		#region Pipeline Stall Monitoring
#if DEBUG
		internal void PrintThreadTraces() {
			Logger.Debug(
				"TRACE BEGIN: <Thread " + LosgapSystem.MasterThread.Name + "> " + Environment.NewLine
				+ GetStackTrace(LosgapSystem.MasterThread).ToStringNullSafe("(!) Could not acquire stack trace.").Replace("   ", String.Empty)
			);

			for (int i = 0; i < slaveThreads.Length; ++i) {
				Logger.Debug(
					"TRACE BEGIN: <Thread " + slaveThreads[i].Name + "> " + Environment.NewLine
					+ GetStackTrace(slaveThreads[i]).ToStringNullSafe("(!) Could not acquire stack trace.").Replace("   ", String.Empty)
				);
			}
		}

#pragma warning disable 618 // Suspend / resume are obsolete, but this is the one case where they make sense
		private static StackTrace GetStackTrace(Thread targetThread) {
			ManualResetEventSlim fallbackThreadReady = new ManualResetEventSlim();
			ManualResetEventSlim exitedSafely = new ManualResetEventSlim();
			try {
				new Thread(delegate() {
					fallbackThreadReady.Set();
					while (!exitedSafely.Wait(200)) {
						try {
							targetThread.Resume();
						}
						catch (Exception) {/*Whatever happens, do never stop to resume the main-thread regularly until the main-thread has exited safely.*/}
					}
				}).Start();
				fallbackThreadReady.Wait();
				//From here, you have about 200ms to get the stack-trace.
				StackTrace trace = null;
				try {
					targetThread.Suspend();
					trace = new StackTrace(targetThread, true);
				}
				catch (ThreadStateException) {
					//failed to get stack trace, since the fallback-thread resumed the thread
					//possible reasons:
					//1.) This thread was just too slow
					//2.) A deadlock occurred
					//Automatic retry seems too risky here, so just return null.
				}
				try {
					targetThread.Resume();
				}
				catch (ThreadStateException) {/*Thread is running again already*/}
				return trace;
			}
			finally {
				//Just signal the backup-thread to stop.
				exitedSafely.Set();
			}
		}
#pragma warning restore 618
#endif
		#endregion
	}
}