// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 06 10 2014 at 13:23 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;

namespace Ophidian.Losgap {
	/// <summary>
	/// An enumeration of possible actions taken by the <see cref="Assure"/> class when an assurance fails.
	/// </summary>
	public enum AssuranceFailureStrategy {
		/// <summary>
		/// Indicates that any assurance that fails should be simply ignored.
		/// </summary>
		DoNothing,
		/// <summary>
		/// Indicates that any assurance that fails should throw an <see cref="AssuranceFailedException"/>.
		/// </summary>
		ThrowException,
		/// <summary>
		/// Indicates that any assurance that fails should add a warning in to the log file by calling <see cref="Logger.Warn"/>.
		/// </summary>
		LogWarning
		
	}
}