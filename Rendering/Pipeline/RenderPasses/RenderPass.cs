// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 06 01 2015 at 13:21 by Ben Bowen

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// Render passes combine various input and output parameters in order to produce a visual output of the scene, usually
	/// executed many times per second, in order to create a 3D simulation.
	/// </summary>
	public abstract class RenderPass : IDisposable {
		[ThreadStatic]
		private static RenderCommandQueue threadLocalRCQ;
		private static readonly ConcurrentDictionary<Thread, RenderCommandQueue> rcqMap = new ConcurrentDictionary<Thread, RenderCommandQueue>(); 
		/// <summary>
		/// The name for this render pass.
		/// </summary>
		public readonly string Name;
		/// <summary>
		/// The synchronization object that should be locked over when making changes to the internal state of this render pass.
		/// </summary>
		protected readonly object InstanceMutationLock = new object();
		private bool isEnabled = true;
		private bool isDisposed = false;
		private event Action<RenderPass> prePass;
		private event Action<RenderPass> postPass;

		private static RenderCommandQueue ThreadLocalRCQ {
			get {
				if (threadLocalRCQ != null) return threadLocalRCQ;
				else {
					if (Thread.CurrentThread == LosgapSystem.MasterThread) threadLocalRCQ = new ImmediateRCQ();
					else threadLocalRCQ = new DeferredRCQ();
					rcqMap[Thread.CurrentThread] = threadLocalRCQ;
					return threadLocalRCQ;
				}
			}
		}

		/// <summary>
		/// Whether or not this pass is enabled. When not enabled, the pass will be skipped on each frame.
		/// </summary>
		public bool IsEnabled {
			get {
				lock (InstanceMutationLock) {
					return isEnabled;
				}
			}
			set {
				using (RenderingModule.RenderStateBarrier.AcquirePermit(withLock: InstanceMutationLock)) {
					isEnabled = value;
				}
			}
		}

		/// <summary>
		/// An event fired on each frame, just before the pass is executed.
		/// </summary>
		/// <remarks>The only parameter to the <see cref="Action{T}"/> delegate is this pass.</remarks>
		public event Action<RenderPass> PrePass {
			add {
				lock (InstanceMutationLock) {
					prePass += value;
				}
			}
			remove {
				lock (InstanceMutationLock) {
					prePass -= value;
				}
			}
		}

		/// <summary>
		/// An event fired on each frame, just after the pass has completed.
		/// </summary>
		/// <remarks>The only parameter to the <see cref="Action{T}"/> delegate is this pass.</remarks>
		public event Action<RenderPass> PostPass {
			add {
				lock (InstanceMutationLock) {
					postPass += value;
				}
			}
			remove {
				lock (InstanceMutationLock) {
					postPass -= value;
				}
			}
		}

		/// <summary>
		/// Whether or not the configuration of input/output parameters on this render pass is valid.
		/// If not valid, this pass will not be executed, and will be skipped on each frame until it is once again in a valid state.
		/// </summary>
		public abstract bool IsValid { get; }

		/// <summary>
		/// Whether or not this pass has been disposed.
		/// </summary>
		public bool IsDisposed {
			get {
				lock (InstanceMutationLock) {
					return isDisposed;
				}
			}
		}

		/// <summary>
		/// Constructs the base type with the given pass name.
		/// </summary>
		/// <param name="name">The name of this render pass. Must not be null.</param>
		protected RenderPass(string name) {
			Assure.NotNull(name);
			Name = name;
		}

#if DEBUG
		~RenderPass() {
			if (!IsDisposed) Logger.Warn(this + " was not disposed before finalization!");
		}
#endif

		/// <summary>
		/// Queues a render command to be executed by the graphics API.
		/// </summary>
		/// <remarks>
		/// All queued commands are queued for the local thread only. Therefore, some commands may need to be duplicated for each thread.
		/// </remarks>
		/// <param name="command">The command to queue.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected static void QueueRenderCommand(RenderCommand command) {
			ThreadLocalRCQ.QueueCommand(command);
		}

		/// <summary>
		/// Reserve a slot in the command queue to insert a command in to later. The reservation is only valid until the command queue
		/// is flushed, and the reserved slot must be used by that time.
		/// </summary>
		/// <remarks>
		/// All queued commands are queued for the local thread only. Therefore, some commands may need to be duplicated for each thread.
		/// </remarks>
		/// <returns>An index in to the command queue that can be used to insert a command later.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected static uint ReserveCommandSlot() {
			return ThreadLocalRCQ.ReserveCommandSlot();
		}

		/// <summary>
		/// Inserts a command in to a previously-reserved queue slot.
		/// </summary>
		/// <remarks>
		/// All queued commands are queued for the local thread only. Therefore, some commands may need to be duplicated for each thread.
		/// </remarks>
		/// <param name="reservedCommandSlot">The index of the slot previously reserved with <see cref="ReserveCommandSlot"/>.</param>
		/// <param name="command">The command to insert.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected static void QueueRenderCommand(uint reservedCommandSlot, RenderCommand command) {
			ThreadLocalRCQ.QueueCommand(reservedCommandSlot, command);
		}

		/// <summary>
		/// Queues the given <paramref name="action"/> in to the command queue.  The action will be executed when the command queue is flushed.
		/// </summary>
		/// <remarks>
		/// All queued commands are queued for the local thread only. Therefore, some commands may need to be duplicated for each thread.
		/// </remarks>
		/// <param name="action">The action to queue. Must not be null.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected static void QueueAction(Action action) {
			Assure.NotNull(action);
			ThreadLocalRCQ.QueueAction(action);
		}

		protected static uint GetNumCommandsQueued() {
			return ThreadLocalRCQ.NumCommandsQueued;
		}

		protected static string GetCommandTypeBreakdown() {
			return ThreadLocalRCQ.CommandTypeBreakdown;
		}

		/// <summary>
		/// Flush all previously queued commands on the local thread. This call invokes all queued commands on the graphics API.
		/// </summary>
		/// <remarks>
		/// All queued commands are queued for the local thread only. Therefore, each thread may need to be flushed.
		/// </remarks>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected static void FlushRenderCommands() {
			ThreadLocalRCQ.Flush();
		}

		/// <summary>
		/// Queues a switch to the given shader. Should usually be immediately followed by a call to
		/// <see cref="QueueShaderResourceUpdate(Ophidian.Losgap.Rendering.Shader)"/> or
		/// <see cref="QueueShaderResourceUpdate(Ophidian.Losgap.Rendering.Shader,Ophidian.Losgap.Rendering.ShaderResourcePackage)"/>.
		/// </summary>
		/// <remarks>
		/// All queued commands are queued for the local thread only. Therefore, some commands may need to be duplicated for each thread.
		/// </remarks>
		/// <param name="shader">The shader to switch to. Must not be null or disposed.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0",
			Justification = "The parameter is validated by the assurances."),
		MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected static void QueueShaderSwitch(Shader shader) {
			Assure.NotNull(shader);
			Assure.False(shader.IsDisposed, "Shader was disposed.");
			shader.RCQSwitchToShader(ThreadLocalRCQ);
		}

		/// <summary>
		/// Queues a switch of pipeline resources to those required by the given shader (and additionally queues the writing
		/// of constant buffer values, where relevant). The resources and constant buffer values used will be those set directly
		/// on the <paramref name="shader"/>'s <see cref="Shader.ResourceBindings"/>.
		/// </summary>
		/// <remarks>
		/// All queued commands are queued for the local thread only. Therefore, some commands may need to be duplicated for each thread.
		/// </remarks>
		/// <param name="shader">The shader whose resources should be enabled. Must not be null or disposed.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0",
			Justification = "The parameter is validated by the assurances."),
		MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected static void QueueShaderResourceUpdate(Shader shader) {
			Assure.NotNull(shader);
			Assure.False(shader.IsDisposed, "Shader was disposed.");
			shader.RCQUpdateResources(ThreadLocalRCQ);
		}

		/// <summary>
		/// Queues a switch of pipeline resources to those required by the given shader (and additionally queues the writing
		/// of constant buffer values, where relevant). The resources and constant buffer values used will
		/// be those supplied by the given <paramref name="resourcePackage"/>.
		/// </summary>
		/// <remarks>
		/// All queued commands are queued for the local thread only. Therefore, some commands may need to be duplicated for each thread.
		/// </remarks>
		/// <param name="shader">The shader whose resources should be enabled. Must not be null or disposed.</param>
		/// <param name="resourcePackage">The resources to set for the given <paramref name="shader"/>. Must not be null.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0",
			Justification = "The parameter is validated by the assurances."),
		MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected static void QueueShaderResourceUpdate(Shader shader, ShaderResourcePackage resourcePackage) {
			Assure.NotNull(shader);
			Assure.False(shader.IsDisposed, "Shader was disposed.");
			Assure.NotNull(resourcePackage);
			shader.RCQUpdateResources(ThreadLocalRCQ, resourcePackage);
		}

		/// <summary>
		/// Completes the pass by presenting the back buffer for the given <paramref name="window"/>. This call is not queued (it is
		/// enacted immediately). Once the scene has been successfully rendered, this call must be made in order to show the result on the
		/// screen.
		/// </summary>
		/// <param name="window">The back buffer to present. Must not be null.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0",
			Justification = "The parameter is validated by the assurances."),
		MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected static void PresentBackBuffer(Window window) {
			Assure.NotNull(window);

			SwapChainHandle outSCH;
			if (!window.GetWindowSwapChain(out outSCH)) return; // Return if window is closed.

			unsafe {
				char* failReason = stackalloc char[InteropUtils.MAX_INTEROP_FAIL_REASON_STRING_LENGTH + 1];
				bool success = NativeMethods.RenderPassManager_PresentBackBuffer((IntPtr) failReason, outSCH);
				if (!success) throw new NativeOperationFailedException(Marshal.PtrToStringUni((IntPtr) failReason));
			}
		}

		/// <summary>
		/// Returns a list of all currently queued commands/actions (encapsulated as <see cref="RCQItem"/>s) on this thread only.
		/// </summary>
		/// <returns>The list of all items (<see cref="RenderCommand"/>s and <see cref="Action"/>s)
		/// that have been queued since the last call to <see cref="FlushRenderCommands"/>.</returns>
		/// <seealso cref="GetCurrentlyQueuedItemsOnAllThreads"/>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
			Justification = "I don't think a property makes sense here."), 
		MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected static IList<RCQItem> GetCurrentlyQueuedItems() {
			return ThreadLocalRCQ.GetCurrentQueue();
		}

		/// <summary>
		/// Returns a list of all currently queued commands/actions (encapsulated as <see cref="RCQItem"/>s) on each active thread
		/// (including the master and any slave threads).
		/// </summary>
		/// <returns>The list of all items (<see cref="RenderCommand"/>s and <see cref="Action"/>s)
		/// that have been queued since the last call to <see cref="FlushRenderCommands"/> on each thread.</returns>
		/// <seealso cref="GetCurrentlyQueuedItems"/>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", 
			Justification = "I don't think a property makes sense here."),
		MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected static IDictionary<Thread, IList<RCQItem>> GetCurrentlyQueuedItemsOnAllThreads() {
			return rcqMap.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.GetCurrentQueue() as IList<RCQItem>);
		}

		/// <summary>
		/// Disposes managed resources and invalides the render pass permanently.
		/// </summary>
		public virtual void Dispose() {
			lock (InstanceMutationLock) {
				if (isDisposed) return;
				isDisposed = true;
			}
		}

		/// <summary>
		/// Returns a string with the name and type of the render pass.
		/// </summary>
		/// <returns>A string with the name and type of the render pass.</returns>
		public override string ToString() {
			return "Render Pass '" + Name + "' + (" + GetType().Name + ")";
		}

		internal void OnPrePass() {
			lock (InstanceMutationLock) {
				if (prePass != null) prePass(this);
			}
		}

		internal void OnPostPass() {
			lock (InstanceMutationLock) {
				if (postPass != null) postPass(this);
			}
		}

		/// <summary>
		/// Called from the <see cref="LosgapSystem.MasterThread"/> when this render pass should execute.
		/// Before this method is called, mutations on the <see cref="RenderingModule.RenderStateBarrier"/> are frozen; and will remain frozen
		/// until this method returns.
		/// </summary>
		/// <param name="pp">The parallelization provider currently in use by the system, providing a way to utilise all cores of the system.</param>
		protected internal abstract void Execute(ParallelizationProvider pp);
	}
}