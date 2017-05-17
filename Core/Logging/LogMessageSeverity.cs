// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 06 10 2014 at 13:34 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;

namespace Ophidian.Losgap {
	/// <summary>
	/// Represents the severity of a log message.
	/// </summary>
	public enum LogMessageSeverity {
		/// <summary>
		/// Indicates that the message is used by programmers to debug potential problems.
		/// </summary>
		Debug = 0,
		/// <summary>
		/// Indicates a standard-level, purely informational, message.
		/// </summary>
		Standard = 1,
		/// <summary>
		/// Indicates that a potential error or configuration fault has occurred.
		/// </summary>
		Warning = 2,
		/// <summary>
		/// Indicates that an unrecoverable issue has occurred, and that the integrity of the application is likely compromised.
		/// </summary>
		Fatal = 3
	}
}