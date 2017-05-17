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
		/// Assures that <paramref name="predicate"/> returns <c>true</c> for at least one item in <paramref name="subject"/>.
		/// </summary>
		/// <typeparam name="T">The element type in the <paramref name="subject"/> collection.</typeparam>
		/// <param name="subject">The collection to test. Must not be null.</param>
		/// <param name="predicate">The predicate function used to determine if a given element matches the criteria for this assurance.
		/// Must not be null.</param>
		/// <param name="failMessage">The message to log or include in the exception if the assurance fails. Passing null indicates that
		/// a default message should be used.</param>
		/// <param name="callerMemberName">This parameter's value is compiler-generated, and should be left unprovided.</param>
		/// <param name="callerFilePath">This parameter's value is compiler-generated, and should be left unprovided.</param>
		/// <param name="callerLineNumber">This parameter's value is compiler-generated, and should be left unprovided.</param>
		[Conditional("DEBUG")]
		public static void Any<T>(
			IEnumerable<T> subject,
			Func<T, bool> predicate,
			string failMessage = null,
			[CallerMemberName] string callerMemberName = null,
			[CallerFilePath] string callerFilePath = null,
			[CallerLineNumber] int callerLineNumber = -1) 
		{
			if (subject == null) throw new ArgumentNullException("subject");
			if (predicate == null) throw new ArgumentNullException("predicate");
			
			if (!subject.Any(predicate)) {
				if (failMessage == null) failMessage = "Supplied " + typeof(T).Name + " collection did not have any suitable elements!";
				failMessage += " Collection contents: " + subject.ToStringOfContents();
				HandleFailure(failMessage, callerMemberName, callerFilePath, callerLineNumber);
			}
		}

		/// <summary>
		/// Assures that <paramref name="predicate"/> returns <c>true</c> for all items in <paramref name="subject"/>.
		/// </summary>
		/// <typeparam name="T">The element type in the <paramref name="subject"/> collection.</typeparam>
		/// <param name="subject">The collection to test. Must not be null.</param>
		/// <param name="predicate">The predicate function used to determine if a given element matches the criteria for this assurance.
		/// Must not be null.</param>
		/// <param name="failMessage">The message to log or include in the exception if the assurance fails. Passing null indicates that
		/// a default message should be used.</param>
		/// <param name="callerMemberName">This parameter's value is compiler-generated, and should be left unprovided.</param>
		/// <param name="callerFilePath">This parameter's value is compiler-generated, and should be left unprovided.</param>
		/// <param name="callerLineNumber">This parameter's value is compiler-generated, and should be left unprovided.</param>
		[Conditional("DEBUG")]
		public static void All<T>(
			IEnumerable<T> subject,
			Func<T, bool> predicate,
			string failMessage = null,
			[CallerMemberName] string callerMemberName = null,
			[CallerFilePath] string callerFilePath = null,
			[CallerLineNumber] int callerLineNumber = -1) 
		{
			if (subject == null) throw new ArgumentNullException("subject");
			if (predicate == null) throw new ArgumentNullException("predicate");

			if (!subject.All(predicate)) {
				if (failMessage == null) failMessage = "Supplied " + typeof(T).Name + " collection had at least one unsuitable element!";
				failMessage += " Collection contents: " + subject.ToStringOfContents(element => (predicate(element) ? String.Empty : "[! Fail] ") + element.ToString());
				HandleFailure(failMessage, callerMemberName, callerFilePath, callerLineNumber);
			}
		}

		/// <summary>
		/// Assures that <paramref name="predicate"/> returns <c>true</c> for no items in <paramref name="subject"/>.
		/// </summary>
		/// <typeparam name="T">The element type in the <paramref name="subject"/> collection.</typeparam>
		/// <param name="subject">The collection to test. Must not be null.</param>
		/// <param name="predicate">The predicate function used to determine if a given element matches the criteria for failing this assurance.
		/// Must not be null.</param>
		/// <param name="failMessage">The message to log or include in the exception if the assurance fails. Passing null indicates that
		/// a default message should be used.</param>
		/// <param name="callerMemberName">This parameter's value is compiler-generated, and should be left unprovided.</param>
		/// <param name="callerFilePath">This parameter's value is compiler-generated, and should be left unprovided.</param>
		/// <param name="callerLineNumber">This parameter's value is compiler-generated, and should be left unprovided.</param>
		[Conditional("DEBUG")]
		public static void None<T>(
			IEnumerable<T> subject,
			Func<T, bool> predicate,
			string failMessage = null,
			[CallerMemberName] string callerMemberName = null,
			[CallerFilePath] string callerFilePath = null,
			[CallerLineNumber] int callerLineNumber = -1) 
		{
			if (subject == null) throw new ArgumentNullException("subject");
			if (predicate == null) throw new ArgumentNullException("predicate");

			if (subject.Any(predicate)) {
				if (failMessage == null) failMessage = "Supplied " + typeof(T).Name + " collection had at least one unsuitable element!";
				failMessage += " Collection contents: " + subject.ToStringOfContents(element => (predicate(element) ? "[! Fail] " : String.Empty) + element.ToString());
				HandleFailure(failMessage, callerMemberName, callerFilePath, callerLineNumber);
			}
		}
	}
}