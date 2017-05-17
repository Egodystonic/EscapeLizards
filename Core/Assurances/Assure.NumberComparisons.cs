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
		/// Assures that <paramref name="subject"/> is greater than <paramref name="min"/>.
		/// </summary>
		/// <param name="subject">The number to test.</param>
		/// <param name="min">The first lowest value that <paramref name="subject"/> must be greater than. If <paramref name="subject"/>
		/// is equal to or lower than this value, the assurance will fail.</param>
		/// <param name="failMessage">The message to log or include in the exception if the assurance fails. Passing null indicates that
		/// a default message should be used.</param>
		/// <param name="callerMemberName">This parameter's value is compiler-generated, and should be left unprovided.</param>
		/// <param name="callerFilePath">This parameter's value is compiler-generated, and should be left unprovided.</param>
		/// <param name="callerLineNumber">This parameter's value is compiler-generated, and should be left unprovided.</param>
		[Conditional("DEBUG")]
		public static void GreaterThan(
			Number subject,
			Number min,
			string failMessage = null,
			[CallerMemberName] string callerMemberName = null,
			[CallerFilePath] string callerFilePath = null,
			[CallerLineNumber] int callerLineNumber = -1) {
			if (subject <= min) {
				if (failMessage == null) {
					failMessage = "Given value was supposed to be greater than " + min + ", " +
						"but was " + subject + ".";
				}
				HandleFailure(failMessage, callerMemberName, callerFilePath, callerLineNumber);
			}
		}

		/// <summary>
		/// Assures that <paramref name="subject"/> is greater than or equal to <paramref name="min"/>.
		/// </summary>
		/// <param name="subject">The number to test.</param>
		/// <param name="min">The lowest value that <paramref name="subject"/> is permitted to be. If <paramref name="subject"/>
		/// is lower than this value, the assurance will fail.</param>
		/// <param name="failMessage">The message to log or include in the exception if the assurance fails. Passing null indicates that
		/// a default message should be used.</param>
		/// <param name="callerMemberName">This parameter's value is compiler-generated, and should be left unprovided.</param>
		/// <param name="callerFilePath">This parameter's value is compiler-generated, and should be left unprovided.</param>
		/// <param name="callerLineNumber">This parameter's value is compiler-generated, and should be left unprovided.</param>
		[Conditional("DEBUG")]
		public static void GreaterThanOrEqualTo(
			Number subject,
			Number min,
			string failMessage = null,
			[CallerMemberName] string callerMemberName = null,
			[CallerFilePath] string callerFilePath = null,
			[CallerLineNumber] int callerLineNumber = -1) {
			if (subject < min) {
				if (failMessage == null) {
					failMessage = "Given value was supposed to be greater than or equal to " + min + ", " +
						"but was " + subject + ".";
				}
				HandleFailure(failMessage, callerMemberName, callerFilePath, callerLineNumber);
			}
		}

		/// <summary>
		/// Assures that <paramref name="subject"/> is less than <paramref name="max"/>.
		/// </summary>
		/// <param name="subject">The number to test.</param>
		/// <param name="max">The first highest value that <paramref name="subject"/> must be less than. If <paramref name="subject"/>
		/// is equal to or higher than this value, the assurance will fail.</param>
		/// <param name="failMessage">The message to log or include in the exception if the assurance fails. Passing null indicates that
		/// a default message should be used.</param>
		/// <param name="callerMemberName">This parameter's value is compiler-generated, and should be left unprovided.</param>
		/// <param name="callerFilePath">This parameter's value is compiler-generated, and should be left unprovided.</param>
		/// <param name="callerLineNumber">This parameter's value is compiler-generated, and should be left unprovided.</param>
		[Conditional("DEBUG")]
		public static void LessThan(
			Number subject,
			Number max,
			string failMessage = null,
			[CallerMemberName] string callerMemberName = null,
			[CallerFilePath] string callerFilePath = null,
			[CallerLineNumber] int callerLineNumber = -1) {
			if (subject >= max) {
				if (failMessage == null) {
					failMessage = "Given value was supposed to be less than " + max + ", " +
						"but was " + subject + ".";
				}
				HandleFailure(failMessage, callerMemberName, callerFilePath, callerLineNumber);
			}
		}

		/// <summary>
		/// Assures that <paramref name="subject"/> is less than or equal to <paramref name="max"/>.
		/// </summary>
		/// <param name="subject">The number to test.</param>
		/// <param name="max">The greatest value that <paramref name="subject"/> is permitted to be. If <paramref name="subject"/>
		/// is greater than this value, the assurance will fail.</param>
		/// <param name="failMessage">The message to log or include in the exception if the assurance fails. Passing null indicates that
		/// a default message should be used.</param>
		/// <param name="callerMemberName">This parameter's value is compiler-generated, and should be left unprovided.</param>
		/// <param name="callerFilePath">This parameter's value is compiler-generated, and should be left unprovided.</param>
		/// <param name="callerLineNumber">This parameter's value is compiler-generated, and should be left unprovided.</param>
		[Conditional("DEBUG")]
		public static void LessThanOrEqualTo(
			Number subject,
			Number max,
			string failMessage = null,
			[CallerMemberName] string callerMemberName = null,
			[CallerFilePath] string callerFilePath = null,
			[CallerLineNumber] int callerLineNumber = -1) {
			if (subject > max) {
				if (failMessage == null) {
					failMessage = "Given value was supposed to be less than or equal to " + max + ", " +
						"but was " + subject + ".";
				}
				HandleFailure(failMessage, callerMemberName, callerFilePath, callerLineNumber);
			}
		}

		/// <summary>
		/// Assures that <paramref name="subject"/> is greater than <paramref name="min"/> and less than <paramref name="max"/>.
		/// </summary>
		/// <param name="subject">The number to test.</param>
		/// <param name="min">The first lowest value that <paramref name="subject"/> must be greater than. If <paramref name="subject"/>
		/// is equal to or lower than this value, the assurance will fail.</param>
		/// <param name="max">The first highest value that <paramref name="subject"/> must be less than. If <paramref name="subject"/>
		/// is equal to or higher than this value, the assurance will fail.</param>
		/// <param name="failMessage">The message to log or include in the exception if the assurance fails. Passing null indicates that
		/// a default message should be used.</param>
		/// <param name="callerMemberName">This parameter's value is compiler-generated, and should be left unprovided.</param>
		/// <param name="callerFilePath">This parameter's value is compiler-generated, and should be left unprovided.</param>
		/// <param name="callerLineNumber">This parameter's value is compiler-generated, and should be left unprovided.</param>
		[Conditional("DEBUG")]
		public static void Between(
			Number subject,
			Number min,
			Number max,
			string failMessage = null,
			[CallerMemberName] string callerMemberName = null,
			[CallerFilePath] string callerFilePath = null,
			[CallerLineNumber] int callerLineNumber = -1) {
			if (subject <= min || subject >= max) {
				if (failMessage == null) {
					failMessage = "Given value was supposed to be between " + min + " and " + max + ", " +
						"but was " + subject + ".";
				}
				HandleFailure(failMessage, callerMemberName, callerFilePath, callerLineNumber);
			}
		}

		/// <summary>
		/// Assures that <paramref name="subject"/> is greater than or equal to <paramref name="min"/> and less than
		/// or equal to <paramref name="max"/>.
		/// </summary>
		/// <param name="subject">The number to test.</param>
		/// <param name="min">The lowest value that <paramref name="subject"/> is permitted to be. If <paramref name="subject"/>
		/// is lower than this value, the assurance will fail.</param>
		/// <param name="max">The greatest value that <paramref name="subject"/> is permitted to be. If <paramref name="subject"/>
		/// is greater than this value, the assurance will fail.</param>
		/// <param name="failMessage">The message to log or include in the exception if the assurance fails. Passing null indicates that
		/// a default message should be used.</param>
		/// <param name="callerMemberName">This parameter's value is compiler-generated, and should be left unprovided.</param>
		/// <param name="callerFilePath">This parameter's value is compiler-generated, and should be left unprovided.</param>
		/// <param name="callerLineNumber">This parameter's value is compiler-generated, and should be left unprovided.</param>
		[Conditional("DEBUG")]
		public static void BetweenOrEqualTo(
			Number subject,
			Number min,
			Number max,
			string failMessage = null,
			[CallerMemberName] string callerMemberName = null,
			[CallerFilePath] string callerFilePath = null,
			[CallerLineNumber] int callerLineNumber = -1) {
			if (subject < min || subject > max) {
				if (failMessage == null) {
					failMessage = "Given value was supposed to be between or equal to " + min + " and " + max + ", " +
						"but was " + subject + ".";
				}
				HandleFailure(failMessage, callerMemberName, callerFilePath, callerLineNumber);
			}
		}

		/// <summary>
		/// Assures that <paramref name="subjectA"/> is equal to <paramref name="subjectB"/>.
		/// </summary>
		/// <param name="subjectA">The first number to check. May be null.</param>
		/// <param name="subjectB">The second number to check. May be null.</param>
		/// <param name="failMessage">The message to log or include in the exception if the assurance fails. Passing null indicates that
		/// a default message should be used.</param>
		/// <param name="callerMemberName">This parameter's value is compiler-generated, and should be left unprovided.</param>
		/// <param name="callerFilePath">This parameter's value is compiler-generated, and should be left unprovided.</param>
		/// <param name="callerLineNumber">This parameter's value is compiler-generated, and should be left unprovided.</param>
		[Conditional("DEBUG")]
		public static void Equal(
			Number subjectA,
			Number subjectB,
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
		/// <param name="subjectA">The first number to check. May be null.</param>
		/// <param name="subjectB">The second number to check. May be null.</param>
		/// <param name="failMessage">The message to log or include in the exception if the assurance fails. Passing null indicates that
		/// a default message should be used.</param>
		/// <param name="callerMemberName">This parameter's value is compiler-generated, and should be left unprovided.</param>
		/// <param name="callerFilePath">This parameter's value is compiler-generated, and should be left unprovided.</param>
		/// <param name="callerLineNumber">This parameter's value is compiler-generated, and should be left unprovided.</param>
		[Conditional("DEBUG")]
		public static void NotEqual(
			Number subjectA,
			Number subjectB,
			string failMessage = null,
			[CallerMemberName] string callerMemberName = null,
			[CallerFilePath] string callerFilePath = null,
			[CallerLineNumber] int callerLineNumber = -1) {
			if (Equals(subjectA, subjectB)) {
				if (failMessage == null) failMessage = "Supplied subjects (" + subjectA + ", " + subjectB + ") were equal!";
				HandleFailure(failMessage, callerMemberName, callerFilePath, callerLineNumber);
			}
		}
	}
}