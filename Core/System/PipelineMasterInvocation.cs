// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 09 10 2014 at 10:19 by Ben Bowen

using System;
using System.Collections.Generic;

namespace Ophidian.Losgap {
	internal class PipelineMasterInvocation {
		[ThreadStatic]
		private static Stack<PipelineMasterInvocation> objectPool;

		[ThreadStatic]
		static int count;

		[ThreadStatic]
		private static List<PipelineMasterInvocation> leasedAsync;

		public readonly object InvocationCompleteMonitor = new object();
		private Action action;
		private bool isSynchronousCall;
		private Exception raisedException;
		public volatile bool AsyncRoutineComplete;

		public Action Action {
			get {
				return action;
			}
		}

		public bool IsSynchronousCall {
			get {
				return isSynchronousCall;
			}
		}

		public Exception RaisedException {
			get {
				return raisedException;
			}
			set {
				raisedException = value;
			}
		}

		public static PipelineMasterInvocation Create(Action action, bool isSynchronousCall) {
			if (objectPool == null) objectPool = new Stack<PipelineMasterInvocation>();
			PipelineMasterInvocation result;
			if (objectPool.Count == 0) {
				if (leasedAsync != null) {
					for (int i = leasedAsync.Count - 1; i >= 0; --i) {
						if (leasedAsync[i].AsyncRoutineComplete) {
							objectPool.Push(leasedAsync[i]);
							leasedAsync.RemoveAt(i);
						} 
					}
					if (objectPool.Count > 0) result = objectPool.Pop();
					else result = new PipelineMasterInvocation();
				}
				else result = new PipelineMasterInvocation();
			}
			else result = objectPool.Pop();

			result.action = action;
			result.isSynchronousCall = isSynchronousCall;
			result.raisedException = null;

			if (!isSynchronousCall) {
				result.AsyncRoutineComplete = false;
				if (leasedAsync == null) leasedAsync = new List<PipelineMasterInvocation>();
				leasedAsync.Add(result);
			}

			return result;
		}

		public static void Free(PipelineMasterInvocation pmi) {
			objectPool.Push(pmi);
		}

		private PipelineMasterInvocation() {
			++count;
			if ((count & 1023) == 1023) Console.WriteLine(count);
		}

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		public override string ToString() {
			return (IsSynchronousCall ? "Synchronous" : "Asynchronous") + " master invocation with "
				+ (RaisedException == null ? "no raised exceptions." : "a raised " + RaisedException.GetType().Name + " exception.");
		}
	}
}