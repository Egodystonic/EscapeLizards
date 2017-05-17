// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 13 02 2015 at 15:43 by Ben Bowen

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

namespace Ophidian.Losgap {
	internal sealed class MasterSlaveBarrier : IDisposable {
		private readonly object barrierOpenLock = new object();
		private readonly object barrierClosedLock = new object();
		private readonly object externalLockObjLock = new object();
		private readonly ConcurrentQueue<PipelineMasterInvocation> masterInvocationQueue = new ConcurrentQueue<PipelineMasterInvocation>();
		private readonly uint numSlaves;
		private bool barrierOpen = false;
		private uint slavesRemaining = 0U;
		private bool isDisposed = false;
		private object externalLockObj = null;

		public MasterSlaveBarrier(uint numSlaves) {
			this.numSlaves = numSlaves;
		}

		public void MasterOpenBarrier() {
			Assure.Equal(Thread.CurrentThread, LosgapSystem.MasterThread);
			Monitor.Enter(barrierClosedLock);
			if (numSlaves > 0U) {
				barrierOpen = true;
				slavesRemaining = numSlaves;
			}
			Monitor.PulseAll(barrierClosedLock);
			Monitor.Exit(barrierClosedLock);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
			Justification = "We catch the exception here so we can re-raise it at the master-invocation callsite.")]
		public void MasterWaitForClose() {
			Assure.Equal(Thread.CurrentThread, LosgapSystem.MasterThread);
			Monitor.Enter(barrierOpenLock);

			hydrateInvocationQueue:
			while (masterInvocationQueue.Count > 0) {
				PipelineMasterInvocation pmi;
				if (!masterInvocationQueue.TryDequeue(out pmi)) continue;
				try {
					if (pmi.IsSynchronousCall) {
						Monitor.Enter(pmi.InvocationCompleteMonitor);
						pmi.Action();
					}
					else pmi.Action();
				}
				catch (Exception e) {
					if (pmi.IsSynchronousCall) pmi.RaisedException = e;
					else LosgapSystem.ExitWithError("Exception raised in asynchronous pipeline master invocation.", e);
				}
				finally {
					if (pmi.IsSynchronousCall) {
						Monitor.Pulse(pmi.InvocationCompleteMonitor);
						Monitor.Exit(pmi.InvocationCompleteMonitor);
					}
				}
			}
			if (barrierOpen) {
				Monitor.Wait(barrierOpenLock);
				goto hydrateInvocationQueue;
			}
			Monitor.Exit(barrierOpenLock);
		}

		public void MasterWaitOnExternalLock(object lockObj, Func<bool> completionPredicate) {
			Assure.NotNull(lockObj);
			Assure.NotNull(completionPredicate);
			Assure.True(Monitor.IsEntered(lockObj));
			Assure.Equal(Thread.CurrentThread, LosgapSystem.MasterThread);

			Monitor.Enter(externalLockObjLock);
			externalLockObj = lockObj;
			try {
				while (!completionPredicate()) {
					while (masterInvocationQueue.Count > 0) {
						PipelineMasterInvocation pmi;
						if (!masterInvocationQueue.TryDequeue(out pmi)) continue;
						try {
							if (pmi.IsSynchronousCall) {
								Monitor.Enter(pmi.InvocationCompleteMonitor);
								pmi.Action();
							}
							else pmi.Action();
						}
						catch (Exception e) {
							if (pmi.IsSynchronousCall) pmi.RaisedException = e;
							else LosgapSystem.ExitWithError("Exception raised in asynchronous pipeline master invocation.", e);
						}
						finally {
							if (pmi.IsSynchronousCall) {
								Monitor.Pulse(pmi.InvocationCompleteMonitor);
								Monitor.Exit(pmi.InvocationCompleteMonitor);
							}
						}
					}
					Monitor.Exit(externalLockObjLock);
					Monitor.Wait(lockObj);
					Monitor.Enter(externalLockObjLock);
				}
			}
			finally {
				externalLockObj = null;
				Monitor.Exit(externalLockObjLock);
			}
		}

		public void SlaveWaitForFirstOpen() {
			Assure.NotEqual(Thread.CurrentThread, LosgapSystem.MasterThread);

			Monitor.Enter(barrierClosedLock);
			if (!barrierOpen && !isDisposed) Monitor.Wait(barrierClosedLock);
			Monitor.Exit(barrierClosedLock);
		}

		public void SlaveWaitForReset() {
			Assure.NotEqual(Thread.CurrentThread, LosgapSystem.MasterThread);

			Monitor.Enter(barrierClosedLock);
			if (isDisposed) {
				Monitor.Exit(barrierClosedLock);
				return;
			}
			Monitor.Enter(barrierOpenLock);
			if (--slavesRemaining == 0U) {
				barrierOpen = false;
				Monitor.Pulse(barrierOpenLock);
			}
			Monitor.Exit(barrierOpenLock);
			Monitor.Wait(barrierClosedLock);
			Monitor.Exit(barrierClosedLock);
		}

		public void SlaveQueueOnMaster(PipelineMasterInvocation pmi) {
			Assure.NotNull(pmi);
			Assure.NotEqual(Thread.CurrentThread, LosgapSystem.MasterThread);
			Monitor.Enter(barrierOpenLock);
			masterInvocationQueue.Enqueue(pmi);
			Monitor.Pulse(barrierOpenLock);
			object externalLockObjLocal;
			lock (externalLockObjLock) {
				externalLockObjLocal = externalLockObj;
			}
			Monitor.Exit(barrierOpenLock);
			if (externalLockObjLocal != null) {
				Monitor.Enter(externalLockObjLocal);
				Monitor.PulseAll(externalLockObjLocal);
				Monitor.Exit(externalLockObjLocal);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
			Justification = "We catch the exception here so we can re-raise it at the master-invocation callsite.")]
		public void MasterHydratePMIQueue() {
			Assure.Equal(Thread.CurrentThread, LosgapSystem.MasterThread);
			Monitor.Enter(barrierClosedLock);

			while (masterInvocationQueue.Count > 0) {
				PipelineMasterInvocation pmi;
				if (!masterInvocationQueue.TryDequeue(out pmi)) continue;
				try {
					if (pmi.IsSynchronousCall) {
						Monitor.Enter(pmi.InvocationCompleteMonitor);
						pmi.Action();
					}
					else pmi.Action();
				}
				catch (Exception e) {
					if (pmi.IsSynchronousCall) pmi.RaisedException = e;
					else LosgapSystem.ExitWithError("Exception raised in asynchronous pipeline master invocation.", e);
				}
				finally {
					if (pmi.IsSynchronousCall) {
						Monitor.Pulse(pmi.InvocationCompleteMonitor);
						Monitor.Exit(pmi.InvocationCompleteMonitor);
					}
				}
			}
			Monitor.Exit(barrierClosedLock);
		}

		public void Dispose() {
			Monitor.Enter(barrierClosedLock);
			isDisposed = true;
			Monitor.PulseAll(barrierClosedLock);
			Monitor.Exit(barrierClosedLock);
		}
	}
}