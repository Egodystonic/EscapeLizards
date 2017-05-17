// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 26 01 2015 at 10:04 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

namespace Ophidian.Losgap {
	/// <summary>
	/// A barrier that allows mutation to sensitive program state as long as the state is not in use; and blocks modification to that state
	/// when it is in use.
	/// </summary>
	/// <remarks>
	/// Instances of this class can be used to control modification to variables that are used by critical parts of the application.
	/// <para>
	/// When a thread wishes to modify sensitive state, it should acquire a <see cref="MutationPermit"/> for the duration of the modifications,
	/// using the <see cref="AcquirePermit()"/> method as follows:
	/// <code>
	/// using (stateMutationBarrier.AcquirePermit()) {
	///		sensitiveVar = false;
	///		sensitiveVar2 = "Example";
	/// }
	/// </code>
	/// If the modifying thread also requires changes to be made inside a lock, the <see cref="AcquirePermit(object)"/> overload
	/// can be used:
	/// <code>
	/// using (stateMutationBarrier.AcquirePermit(withLock: mutationLock)) {
	///		// mutationLock is now held until the using{} block is exited;
	///		// this has the same effect as writing lock (mutationLock) { } around the code inside the using statement 
	/// }
	/// </code>
	/// </para>
	/// <para>
	/// When the 'owning' routine wishes to freeze mutations to sensitive variables, it can do so by calling <see cref="FreezeMutations"/>.
	/// Consequently, all calls to <see cref="AcquirePermit()"/> and its overloads will result in the callers being blocked until the 'owning'
	/// routine calls <see cref="UnfreezeMutations"/>.
	/// </para>
	/// <para>
	/// If a previously-acquired <see cref="MutationPermit"/> is still in use when <see cref="FreezeMutations"/> is called, the call to
	/// <see cref="FreezeMutations"/> will block until all acquired permits have been disposed.
	/// </para>
	/// </remarks>
	public sealed class StateMutationBarrier {
		private readonly object freezeLock = new object();
		private int activeMutatorsCounter = 0;
		private readonly Func<bool> freezeMasterBlockPredicate;

		public StateMutationBarrier() {
			freezeMasterBlockPredicate = FreezeMasterBlockPredicate;
		}

		/// <summary>
		/// A struct representing permission to mutate sensitive program state controlled by the issuing <see cref="StateMutationBarrier"/>.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes",
			Justification = "Equality comparison of mutation permits is not recommended.")]
		public struct MutationPermit : IDisposable {
			private readonly StateMutationBarrier barrier;
			private readonly object additionalLockObject;
#if DEBUG
			private bool isDisposed;
#endif

			internal MutationPermit(StateMutationBarrier barrier, object additionalLockObject) {
				Assure.NotNull(barrier);
				if (Monitor.IsEntered(barrier.freezeLock)) throw new ApplicationException("Can not acquire permit: Mutations are locked from current thread.");
#if DEBUG
				isDisposed = false;
#endif
				this.barrier = barrier;
				this.additionalLockObject = additionalLockObject;
				lock (barrier.freezeLock) {
					++barrier.activeMutatorsCounter;
				}
				if (additionalLockObject != null) Monitor.Enter(additionalLockObject);
			}

			/// <summary>
			/// Relinquishes permission for mutation, allowing state-freezing to happen.
			/// </summary>
			public void Dispose() {
				if (additionalLockObject != null) Monitor.Exit(additionalLockObject);
				lock (barrier.freezeLock) {
#if DEBUG
					Assure.False(isDisposed, "Can not dispose same MutationPermit more than once!");
					isDisposed = true;
#endif
					--barrier.activeMutatorsCounter;
					if (barrier.activeMutatorsCounter == 0) {
						Monitor.PulseAll(barrier.freezeLock);
					}
				}
			}
		}


		/// <summary>
		/// Whether or not the state guarded by this barrier is currently 'frozen' (e.g. <c>true</c> if mutations are currently disallowed, or 
		/// <c>false</c> if <see cref="MutationPermit"/>s can be <see cref="AcquirePermit()">acquired</see>).
		/// </summary>
		public bool IsFrozen {
			get {
				if (Monitor.IsEntered(freezeLock)) return true;
				else {
					bool couldEnter = Monitor.TryEnter(freezeLock);
					if (couldEnter) Monitor.Exit(freezeLock);
					return !couldEnter;
				}
			}
		}

		/// <summary>
		/// Returns a <see cref="MutationPermit"/> that denotes that state changes may occur
		/// for as long as it is not <see cref="MutationPermit.Dispose">disposed</see>.
		/// If mutations are currently <see cref="IsFrozen">frozen</see>, this method will block until the state 'owner' calls
		/// <see cref="UnfreezeMutations"/>.
		/// </summary>
		/// <returns>A new <see cref="MutationPermit"/>. Must be disposed once you are finished mutating sensitive state.</returns>
		public MutationPermit AcquirePermit() {
			return new MutationPermit(this, null);
		}

		/// <summary>
		/// Returns a <see cref="MutationPermit"/> that denotes that state changes may occur
		/// for as long as it is not <see cref="MutationPermit.Dispose">disposed</see>.
		/// If mutations are currently <see cref="IsFrozen">frozen</see>, this method will block until the state 'owner' calls
		/// <see cref="UnfreezeMutations"/>.
		/// <para>
		/// Additionally, this method will enter a lock over the given lock object (<paramref name="withLock"/>) until the returned
		/// <see cref="MutationPermit"/> is <see cref="MutationPermit.Dispose">disposed</see>.
		/// </para>
		/// </summary>
		/// <param name="withLock">An object to <c>lock</c> over (Enter/Exit) for the duration of the mutations.
		/// Must not be null.</param>
		/// <returns>A new <see cref="MutationPermit"/>. Must be disposed once you are finished mutating sensitive state.</returns>
		public MutationPermit AcquirePermit(object withLock) {
			return new MutationPermit(this, withLock);
		}

		/// <summary>
		/// Freeze mutations to sensitive state, blocking any callers to <see cref="AcquirePermit()"/>.
		/// This method will block if mutations are currently ongoing on another thread; and will return once all active <see cref="MutationPermit"/>s
		/// have been disposed.
		/// </summary>
		public void FreezeMutations() {
			Assure.False(Monitor.IsEntered(freezeLock), "MutationPermits have not been disposed on current thread or mutations are already frozen.");
			Monitor.Enter(freezeLock);
			var msb = LosgapSystem.ActiveMSB;
			if (msb != null) msb.MasterWaitOnExternalLock(freezeLock, freezeMasterBlockPredicate);
			Assure.Equal(activeMutatorsCounter, 0);
		}

		private bool FreezeMasterBlockPredicate() {
			Assure.True(Monitor.IsEntered(freezeLock));
			Assure.Equal(Thread.CurrentThread, LosgapSystem.MasterThread);
			return activeMutatorsCounter == 0;
		}

		/// <summary>
		/// Relinquish control of the protected state, allowing mutations to proceed once again.
		/// </summary>
		public void UnfreezeMutations() {
			Assure.True(Monitor.IsEntered(freezeLock), "Mutations are not currently frozen!");
			Monitor.Exit(freezeLock);
		}
	}
}