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
		/// Assures that <paramref name="subject"/> is not null.
		/// </summary>
		/// <typeparam name="T">The type of <paramref name="subject"/>.</typeparam>
		/// <param name="subject">The object to inspect for nullity.</param>
		/// <param name="failMessage">The message to log or include in the exception if the assurance fails. Passing null indicates that
		/// a default message should be used.</param>
		/// <param name="callerMemberName">This parameter's value is compiler-generated, and should be left unprovided.</param>
		/// <param name="callerFilePath">This parameter's value is compiler-generated, and should be left unprovided.</param>
		/// <param name="callerLineNumber">This parameter's value is compiler-generated, and should be left unprovided.</param>
		[Conditional("DEBUG")]
		public static void NotNull<T>(
			T subject,
			string failMessage = null,
			[CallerMemberName] string callerMemberName = null,
			[CallerFilePath] string callerFilePath = null,
			[CallerLineNumber] int callerLineNumber = -1)
			where T : class {
			if (subject == null) {
				if (failMessage == null) failMessage = "Supplied " + typeof(T).Name + " must not be null!";
				HandleFailure(failMessage, callerMemberName, callerFilePath, callerLineNumber);
			}
		}

		/// <summary>
		/// Assures that <paramref name="subject"/> is null.
		/// </summary>
		/// <typeparam name="T">The type of <paramref name="subject"/>.</typeparam>
		/// <param name="subject">The object to inspect for nullity.</param>
		/// <param name="failMessage">The message to log or include in the exception if the assurance fails. Passing null indicates that
		/// a default message should be used.</param>
		/// <param name="callerMemberName">This parameter's value is compiler-generated, and should be left unprovided.</param>
		/// <param name="callerFilePath">This parameter's value is compiler-generated, and should be left unprovided.</param>
		/// <param name="callerLineNumber">This parameter's value is compiler-generated, and should be left unprovided.</param>
		[Conditional("DEBUG")]
		public static void Null<T>(
			T subject,
			string failMessage = null,
			[CallerMemberName] string callerMemberName = null,
			[CallerFilePath] string callerFilePath = null,
			[CallerLineNumber] int callerLineNumber = -1)
			where T : class {
			if (subject != null) {
				if (failMessage == null) failMessage = "Supplied " + typeof(T).Name + " must be null!";
				HandleFailure(failMessage, callerMemberName, callerFilePath, callerLineNumber);
			}
		}

		/// <summary>
		/// Assures that <paramref name="subject"/> is not null.
		/// </summary>
		/// <typeparam name="T">The non-nullable equivalent type of <paramref name="subject"/>.</typeparam>
		/// <param name="subject">The object to inspect for nullity.</param>
		/// <param name="failMessage">The message to log or include in the exception if the assurance fails. Passing null indicates that
		/// a default message should be used.</param>
		/// <param name="callerMemberName">This parameter's value is compiler-generated, and should be left unprovided.</param>
		/// <param name="callerFilePath">This parameter's value is compiler-generated, and should be left unprovided.</param>
		/// <param name="callerLineNumber">This parameter's value is compiler-generated, and should be left unprovided.</param>
		[Conditional("DEBUG")]
		public static void NotNull<T>(
			T? subject,
			string failMessage = null,
			[CallerMemberName] string callerMemberName = null,
			[CallerFilePath] string callerFilePath = null,
			[CallerLineNumber] int callerLineNumber = -1)
			where T : struct {
			if (!subject.HasValue) {
				if (failMessage == null) failMessage = "Supplied " + typeof(T).Name + " must not be null!";
				HandleFailure(failMessage, callerMemberName, callerFilePath, callerLineNumber);
			}
		}

		/// <summary>
		/// Assures that <paramref name="subject"/> is null.
		/// </summary>
		/// <typeparam name="T">The non-nullable equivalent type of <paramref name="subject"/>.</typeparam>
		/// <param name="subject">The object to inspect for nullity.</param>
		/// <param name="failMessage">The message to log or include in the exception if the assurance fails. Passing null indicates that
		/// a default message should be used.</param>
		/// <param name="callerMemberName">This parameter's value is compiler-generated, and should be left unprovided.</param>
		/// <param name="callerFilePath">This parameter's value is compiler-generated, and should be left unprovided.</param>
		/// <param name="callerLineNumber">This parameter's value is compiler-generated, and should be left unprovided.</param>
		[Conditional("DEBUG")]
		public static void Null<T>(
			T? subject,
			string failMessage = null,
			[CallerMemberName] string callerMemberName = null,
			[CallerFilePath] string callerFilePath = null,
			[CallerLineNumber] int callerLineNumber = -1)
			where T : struct {
			if (subject.HasValue) {
				if (failMessage == null) failMessage = "Supplied " + typeof(T).Name + " must be null!";
				HandleFailure(failMessage, callerMemberName, callerFilePath, callerLineNumber);
			}
		}
	}
}