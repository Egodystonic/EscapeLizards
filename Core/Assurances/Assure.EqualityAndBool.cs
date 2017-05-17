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
	public static partial class Assure {
		/// <summary>
		/// Assures that <paramref name="subjectA"/> is equal to <paramref name="subjectB"/>.
		/// </summary>
		/// <param name="subjectA">The first object to check. May be null.</param>
		/// <param name="subjectB">The second object to check. May be null.</param>
		/// <param name="failMessage">The message to log or include in the exception if the assurance fails. Passing null indicates that
		/// a default message should be used.</param>
		/// <param name="callerMemberName">This parameter's value is compiler-generated, and should be left unprovided.</param>
		/// <param name="callerFilePath">This parameter's value is compiler-generated, and should be left unprovided.</param>
		/// <param name="callerLineNumber">This parameter's value is compiler-generated, and should be left unprovided.</param>
		[Conditional("DEBUG")]
		public static void Equal<T, U>(
			T subjectA,
			U subjectB,
			string failMessage = null,
			[CallerMemberName] string callerMemberName = null,
			[CallerFilePath] string callerFilePath = null,
			[CallerLineNumber] int callerLineNumber = -1) {
				if (!Equals(subjectA, subjectB)) {
				if (failMessage == null) failMessage = "Supplied subjects (" + subjectA + ", " + subjectB + ") were not equal!";
				HandleFailure(failMessage, callerMemberName, callerFilePath, callerLineNumber);
			}
		}

		/// <summary>
		/// Assures that <paramref name="subjectA"/> is not equal to <paramref name="subjectB"/>.
		/// </summary>
		/// <param name="subjectA">The first object to check. May be null.</param>
		/// <param name="subjectB">The second object to check. May be null.</param>
		/// <param name="failMessage">The message to log or include in the exception if the assurance fails. Passing null indicates that
		/// a default message should be used.</param>
		/// <param name="callerMemberName">This parameter's value is compiler-generated, and should be left unprovided.</param>
		/// <param name="callerFilePath">This parameter's value is compiler-generated, and should be left unprovided.</param>
		/// <param name="callerLineNumber">This parameter's value is compiler-generated, and should be left unprovided.</param>
		[Conditional("DEBUG")]
		public static void NotEqual<T, U>(
			T subjectA,
			U subjectB,
			string failMessage = null,
			[CallerMemberName] string callerMemberName = null,
			[CallerFilePath] string callerFilePath = null,
			[CallerLineNumber] int callerLineNumber = -1) {
				if (Equals(subjectA, subjectB)) {
				if (failMessage == null) failMessage = "Supplied subjects (" + subjectA + ", " + subjectB + ") were equal!";
				HandleFailure(failMessage, callerMemberName, callerFilePath, callerLineNumber);
			}
		}

		/// <summary>
		/// Assures that <paramref name="subject"/> is <c>true</c>.
		/// </summary>
		/// <param name="subject">The boolean expression that should evaluate to <c>true</c>.</param>
		/// <param name="failMessage">The message to log or include in the exception if the assurance fails. Passing null indicates that
		/// a default message should be used.</param>
		/// <param name="callerMemberName">This parameter's value is compiler-generated, and should be left unprovided.</param>
		/// <param name="callerFilePath">This parameter's value is compiler-generated, and should be left unprovided.</param>
		/// <param name="callerLineNumber">This parameter's value is compiler-generated, and should be left unprovided.</param>
		[Conditional("DEBUG")]
		public static void True(
			bool subject,
			string failMessage = null,
			[CallerMemberName] string callerMemberName = null,
			[CallerFilePath] string callerFilePath = null,
			[CallerLineNumber] int callerLineNumber = -1) {
			if (!subject) {
				if (failMessage == null) failMessage = "Supplied statement was not true!";
				HandleFailure(failMessage, callerMemberName, callerFilePath, callerLineNumber);
			}
		}

		/// <summary>
		/// Assures that <paramref name="subject"/> is <c>false</c>.
		/// </summary>
		/// <param name="subject">The boolean expression that should evaluate to <c>false</c>.</param>
		/// <param name="failMessage">The message to log or include in the exception if the assurance fails. Passing null indicates that
		/// a default message should be used.</param>
		/// <param name="callerMemberName">This parameter's value is compiler-generated, and should be left unprovided.</param>
		/// <param name="callerFilePath">This parameter's value is compiler-generated, and should be left unprovided.</param>
		/// <param name="callerLineNumber">This parameter's value is compiler-generated, and should be left unprovided.</param>
		[Conditional("DEBUG")]
		public static void False(
			bool subject,
			string failMessage = null,
			[CallerMemberName] string callerMemberName = null,
			[CallerFilePath] string callerFilePath = null,
			[CallerLineNumber] int callerLineNumber = -1) {
			if (subject) {
				if (failMessage == null) failMessage = "Supplied statement was not false!";
				HandleFailure(failMessage, callerMemberName, callerFilePath, callerLineNumber);
			}
		}
	}
}