// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 06 10 2014 at 13:19 by Ben Bowen

using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace Ophidian.Losgap {
	public static partial class LosgapSystem {
		private static IList<ILosgapModule> addedModuleList = new List<ILosgapModule>();

		private static PipelineProcessor processor = null;
		private static Thread masterThread;
		private static Action systemStarting;
		private static Action systemExited;

		/// <summary>
		/// An event invoked when the system is about to start.
		/// </summary>
		public static event Action SystemStarting {
			add {
				lock (staticMutationLock) {
					systemStarting += value;
				}
			}
			remove {
				lock (staticMutationLock) {
					systemStarting -= value;
				}
			}
		}
		/// <summary>
		/// An event invoked when the system has completed the last frame and shut down completely.
		/// </summary>
		public static event Action SystemExited {
			add {
				lock (staticMutationLock) {
					systemExited += value;
				}
			}
			remove {
				lock (staticMutationLock) {
					systemExited -= value;
				}
			}
		}

		/// <summary>
		/// Whether or not the pipeline is currently running. The pipeline begins execution after you call <see cref="Start"/>, and stops
		/// once you call <see cref="Exit"/>.
		/// </summary>
		public static bool IsRunning {
			get {
				lock (staticMutationLock) {
					return processor != null;
				}
			}
		}

		/// <summary>
		/// The thread that orchestrates the pipeline. The master thread is the thread that is always used to call certain methods on every
		/// <see cref="ILosgapModule"/>, and can have actions invoked on it with <see cref="InvokeOnMaster(Action)"/>.
		/// </summary>
		/// <remarks>
		/// The master thread is set to the first thread to access various properties or methods of this class.
		/// Until those methods/properties has been accessed, this property will return <c>null</c>.
		/// <para>
		/// You can explicitly set this value, as long as it has not already been set to a different thread.
		/// </para>
		/// </remarks>
		public static Thread MasterThread {
			get {
				lock (staticMutationLock) {
					return masterThread;
				}
			}
			set {
				if (value == null) throw new ArgumentNullException("value");
				lock (staticMutationLock) {
					if (masterThread != null && masterThread != value) {
						throw new InvalidOperationException("Master thread has already been set to thread with name " +
						"'" + masterThread.Name + "'.");
					}

					masterThread = value;
				}
			}
		}

		internal static MasterSlaveBarrier ActiveMSB {
			get {
				lock (staticMutationLock) {
					if (processor == null) return null;
					return processor.ParallelizationProvider.WorkBarrier;
				}
			}
		}

		/// <summary>
		/// Add a module to the system. Adding a module means it (and its associated functionality) will be usable by your application.
		/// You must add a module before you can access any of its features.
		/// </summary>
		/// <param name="moduleType">The module type to add. Must not have already been added. Must not be null.</param>
		/// <exception cref="InvalidOperationException">Thrown if you try to add a module while the system
		/// <see cref="IsRunning">is running</see>. You must add all desired modules before the pipeline starts.</exception>
		/// <exception cref="InvalidOperationException">Thrown if you call this method from any thread other than the 
		/// <see cref="MasterThread"/>.</exception>
		/// <exception cref="ArgumentException">Thrown if a module of the same type has already been added.</exception>
		public static void AddModule(Type moduleType) {
			if (moduleType == null) throw new ArgumentNullException("moduleType");
			

			lock (staticMutationLock) {
				if (addedModuleList.Any(addedMod => addedMod.GetType() == moduleType)) {
					throw new ArgumentException("Module of type '" + moduleType.Name + "' already added.", "moduleType");
				}
				if (IsRunning) throw new InvalidOperationException("Can not add more modules while the system is running.");

				ThrowIfCurrentThreadIsNotMaster();

				ILosgapModule module = (ILosgapModule) Activator.CreateInstance(moduleType, true);

				addedModuleList.Add(module);
				module.ModuleAdded();
				Logger.Log("Module added: " + moduleType.Name + ".");
			}
		}

		/// <summary>
		/// Ascertains whether or not a module of the given type has already been added to the pipeline.
		/// </summary>
		/// <param name="moduleType">The type of module to check for. Must not be null.</param>
		/// <returns>True if a module of <paramref name="moduleType"/> is currently in the pipeline, false if not.</returns>
		public static bool IsModuleAdded(Type moduleType) {
			if (moduleType == null) throw new ArgumentNullException("moduleType");

			lock (staticMutationLock) {
				return addedModuleList.Any(mod => mod.GetType() == moduleType);
			}
		}

		/// <summary>
		/// Start the system. Calling this method begins the pipeline for each added module, and blocks the calling thread until the
		/// application calls <see cref="Exit"/>.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown if the pipeline is already running.</exception>
		/// <exception cref="InvalidOperationException">Thrown if you call this method from any thread other than the 
		/// <see cref="MasterThread"/>.</exception>
		public static void Start() {
			PipelineProcessor newProcessor;

			lock (staticMutationLock) {
				ThrowIfCurrentThreadIsNotMaster();
				if (IsRunning) throw new InvalidOperationException("Pipeline is already running.");
				
				GCSettings.LatencyMode = GCLatencyMode.Interactive;
				OnSystemStarting();
				CreatePipelineProcessor();
				newProcessor = processor;
			}

			newProcessor.Start();

			lock (staticMutationLock) {
				MasterThreadExited();
			}
		}

		/// <summary>
		/// Tells the pipeline to stop after the current frame has finished. This method only signals the pipeline to stop, it does
		/// not block the caller until the operation has completed. You can be notified when this has occurred by subscribing to the
		/// <see cref="SystemExited"/> event.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown if the system is not currently running.</exception>
		public static void Exit() {
			lock (staticMutationLock) {
				if (!IsRunning) return;
				processor.Dispose();
				processor = null;
			}
#if DEBUG
			// Perform a full GC so we can invoke warnings for non-disposed IDisposable garbage
			GC.Collect(2, GCCollectionMode.Forced, true);
#endif
		}

		private static void ThrowIfCurrentThreadIsNotMaster() {
			if (MasterThread == null) {
				MasterThread = Thread.CurrentThread;
				if (MasterThread.Name == null) MasterThread.Name = "LosgapMaster";
			}
			else {
				if (MasterThread != Thread.CurrentThread) {
					throw new InvalidOperationException("Invalid cross-thread invocation: " +
						"That action can only be performed from the master thread!");
				}
			}
		}

		private static void CreatePipelineProcessor() {
			if (addedModuleList.Count <= 0) {
				throw new InvalidOperationException("At least one module must be added to the pipeline before it can be started.");
			}

			processor = new PipelineProcessor(addedModuleList.ToArray());
		}

		private static void MasterThreadExited() {
			OnSystemExited();
		}

		private static void OnSystemStarting() {
			lock (staticMutationLock) {
				Logger.Log("System starting.");
				if (systemStarting != null) systemStarting();
			}
		}

		private static void OnSystemExited() {
			lock (staticMutationLock) {
				if (systemExited != null) systemExited();
				Logger.Log("System exited.");
			}
		}
	}
}