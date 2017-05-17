// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 06 10 2014 at 13:23 by Ben Bowen

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Ophidian.Losgap {
	/// <summary>
	/// A static class used to provide runtime assertions on code; on debug builds only. The assertions can be used to confirm 
	/// assumptions in any application code or in the framework itself.
	/// </summary>
	/// <remarks>
	/// This class can be used to help identify runtime bugs and general coding errors whilst your application is executing.
	/// By adding assurances to your code at various steps, you can find problems in game logic from their source;
	/// rather than letting errors cascade until results are seen many steps down the stack.
	/// <para>
	/// To add an assurance, simply call the appropriate method on this class at the relevant place in your code. ONLY debug builds
	/// will compile the call; meaning that you can only check assurances from within debug builds. However, this also means that
	/// development and release builds will be unaffected by assurances. In other words, using assurances and the Assure class
	/// will not slow down your development and release product builds whatsoever.
	/// </para>
	/// <para>
	/// The action taken when an assurance fails can be configured via the <see cref="FailureStrategy"/> property.
	/// </para>
	/// </remarks>
	public static partial class Assure {
		private static readonly object staticMutationLock = new object();
		private static AssuranceFailureStrategy failureStrategy = AssuranceFailureStrategy.ThrowException;

		/// <summary>
		/// The action to take when any assertion fails.
		/// </summary>
		public static AssuranceFailureStrategy FailureStrategy {
			get {
				lock (staticMutationLock) {
					return failureStrategy;
				}
			}
			set {
				lock (staticMutationLock) {
					failureStrategy = value;
				}
			}
		}

		/// <summary>
		/// Invokes an automatic assurance failure with the given reason (<paramref name="failMessage"/>).
		/// </summary>
		/// <param name="failMessage">The message to log or include in the exception that explains the reason for this failure.</param>
		/// <param name="callerMemberName">This parameter's value is compiler-generated, and should be left unprovided.</param>
		/// <param name="callerFilePath">This parameter's value is compiler-generated, and should be left unprovided.</param>
		/// <param name="callerLineNumber">This parameter's value is compiler-generated, and should be left unprovided.</param>
		[Conditional("DEBUG")]
		public static void Fail(
			string failMessage,
			[CallerMemberName] string callerMemberName = null,
			[CallerFilePath] string callerFilePath = null,
			[CallerLineNumber] int callerLineNumber = -1)
		{
			if (failMessage == null) failMessage = "Unknown reason for failure.";
			HandleFailure(failMessage, callerMemberName, callerFilePath, callerLineNumber);
		}

		private static void HandleFailure(
				string failureMessage,
				string callerMemberName,
				string callerFilePath,
				int callerLineNumber,
				[CallerMemberName] string callingAssuranceMemberName = null
			) {
				string message = "Assurance '" + callingAssuranceMemberName + "' failed in "
							+ callerMemberName + "() at " + Path.GetFileName(callerFilePath) + ":" + callerLineNumber
							+ ": " + failureMessage;

			switch (FailureStrategy) {
				case AssuranceFailureStrategy.LogWarning:
					Logger.Warn(message);
					break;
				case AssuranceFailureStrategy.ThrowException:
					throw new AssuranceFailedException(message);
			}
		}
	}
}