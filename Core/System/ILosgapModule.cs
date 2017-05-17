// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 16 10 2014 at 15:18 by Ben Bowen

using System;
using System.Collections.Generic;
using System.Threading;

namespace Ophidian.Losgap {
	/// <summary>
	/// Represents a module that can be added to the <see cref="LosgapSystem"/>. A module represents an isolated set of functionality
	/// in the LOSGAP framework that can be enabled or disabled separately from other modules.
	/// </summary>
	public interface ILosgapModule {
		/// <summary>
		/// The desired minimum number of milliseconds between invocations of <see cref="PipelineIterate"/> on this module.
		/// </summary>
		long TickIntervalMs { get; }

		/// <summary>
		/// Called by the <see cref="LosgapSystem"/> when the module has been added (via <see cref="LosgapSystem.AddModule"/>). This
		/// method is guaranteed to only ever be called once during the lifetime of the application; and is a good place for the module
		/// to perform any initialization logic.
		/// </summary>
		void ModuleAdded();

		/// <summary>
		/// Called by the <see cref="LosgapSystem"/> when it is time for this module to 'tick'. At this point, the module should execute
		/// its logic for the current frame.
		/// </summary>
		/// <param name="parallelizationProvider">An object that facilitates multithreaded execution of state. Never null.</param>
		/// <param name="deltaMs">The time, in milliseconds, that has elapsed since the last invocation of this method.</param>
		void PipelineIterate(ParallelizationProvider parallelizationProvider, long deltaMs);
	}
}