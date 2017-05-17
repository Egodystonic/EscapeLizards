// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 06 10 2014 at 13:31 by Ben Bowen

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Ophidian.Losgap {
	/// <summary>
	/// Static class that exposes methods for logging text messages (to a log file).
	/// </summary>
	public static class Logger {
		private const string NULL_MESSAGE = "<No message>";
		private static readonly object staticMutationLock = new object();
		private static ILoggingProvider loggingProvider = null;
		private static string fileLocation = null;

		/// <summary>
		/// Gets or sets the <see cref="ILoggingProvider">logging provider</see> used to handle log messages.
		/// </summary>
		public static ILoggingProvider LoggingProvider {
			get {
				lock (staticMutationLock) {
					if (loggingProvider == null) {
						loggingProvider = new DefaultLoggingProvider(true, true, false);
						UpdateLoggingProviderFileLocation();
					}
					return loggingProvider;
				}
			}
			set {
				lock (staticMutationLock) {
					if (loggingProvider == value) return;
					if (value == null) throw new ArgumentNullException("value");
					if (value.IsDisposed) throw new ObjectDisposedException("Can not set LoggingProvider to disposed value!");

					if (loggingProvider != null && !loggingProvider.IsDisposed) loggingProvider.Dispose();
					loggingProvider = value;

					UpdateLoggingProviderFileLocation();
				}
			}
		}

		/// <summary>
		/// Gets or sets the name/location of the log file, relative to the <see cref="LosgapSystem.MutableDataDirectory"/>.
		/// </summary>
		public static string FileLocation {
			get {
				lock (staticMutationLock) {
					if (fileLocation == null) {
						fileLocation = LosgapSystem.ApplicationName + ".log";
					}
					return fileLocation;
				}
			}
			set {
				lock (staticMutationLock) {
					if (!IOUtils.IsValidFilePath(value)) {
						throw new ArgumentException("Supplied file path '" + value + "' is not a valid filename.", "value");
					}
					fileLocation = value;

					UpdateLoggingProviderFileLocation();
				}
			}
		}

		/// <summary>
		/// Logs the given <paramref name="message"/> (and <paramref name="associatedException"/>, if one is provided) with a severity level of
		/// <see cref="LogMessageSeverity.Debug"/>.
		/// Additionally, calls to this method will only be compiled / invoked on Debug builds.
		/// </summary>
		/// <param name="message">The message to log. If an object of any type but string is provided, <c>message.ToString()</c> will be
		/// used as the message text. Null values are tolerated.</param>
		/// <param name="associatedException">The exception to log alongside the <paramref name="message"/>. May be null if there is no
		/// associated exception.</param>
		/// <param name="callerMemberName">This parameter's value is compiler-generated, and should be left unprovided.</param>
		/// <param name="callerFilePath">This parameter's value is compiler-generated, and should be left unprovided.</param>
		/// <param name="callerLineNumber">This parameter's value is compiler-generated, and should be left unprovided.</param>
		[Conditional("DEBUG")]
		public static void Debug(
			object message,
			Exception associatedException = null,
			[CallerMemberName] string callerMemberName = null,
			[CallerFilePath] string callerFilePath = null,
			[CallerLineNumber] int callerLineNumber = -1
		) {
			try {
				lock (staticMutationLock) {
					LoggingProvider.WriteMessage(
						LogMessageSeverity.Debug,
						message.ToStringNullSafe(NULL_MESSAGE),
						associatedException,
						callerMemberName,
						callerFilePath,
						callerLineNumber
					);
				}
			}
			catch (IOException e) {
				System.Diagnostics.Debug.WriteLine("Warning: Failed to set write log message to '" + FileLocation + "'. " +
					"Exception details: " + e.GetAllMessages());
			}
		}

		/// <summary>
		/// Logs the given <paramref name="message"/> (and <paramref name="associatedException"/>, if one is provided) with a severity level of
		/// <see cref="LogMessageSeverity.Standard"/>.
		/// </summary>
		/// <param name="message">The message to log. If an object of any type but string is provided, <c>message.ToString()</c> will be
		/// used as the message text. Null values are tolerated.</param>
		/// <param name="associatedException">The exception to log alongside the <paramref name="message"/>. May be null if there is no
		/// associated exception.</param>
		/// <param name="callerMemberName">This parameter's value is compiler-generated, and should be left unprovided.</param>
		/// <param name="callerFilePath">This parameter's value is compiler-generated, and should be left unprovided.</param>
		/// <param name="callerLineNumber">This parameter's value is compiler-generated, and should be left unprovided.</param>
		public static void Log(
			object message,
			Exception associatedException = null,
			[CallerMemberName] string callerMemberName = null,
			[CallerFilePath] string callerFilePath = null,
			[CallerLineNumber] int callerLineNumber = -1
		) {
			try {
				lock (staticMutationLock) {
					LoggingProvider.WriteMessage(
						LogMessageSeverity.Standard,
						message.ToStringNullSafe(NULL_MESSAGE),
						associatedException,
						callerMemberName,
						callerFilePath,
						callerLineNumber
					);
				}
			}
			catch (IOException e) {
				System.Diagnostics.Debug.WriteLine("Warning: Failed to set write log message to '" + FileLocation + "'. " +
					"Exception details: " + e.GetAllMessages());
			}
		}

		/// <summary>
		/// Logs the given <paramref name="message"/> (and <paramref name="associatedException"/>, if one is provided) with a severity level of
		/// <see cref="LogMessageSeverity.Warning"/>.
		/// </summary>
		/// <param name="message">The message to log. If an object of any type but string is provided, <c>message.ToString()</c> will be
		/// used as the message text. Null values are tolerated.</param>
		/// <param name="associatedException">The exception to log alongside the <paramref name="message"/>. May be null if there is no
		/// associated exception.</param>
		/// <param name="callerMemberName">This parameter's value is compiler-generated, and should be left unprovided.</param>
		/// <param name="callerFilePath">This parameter's value is compiler-generated, and should be left unprovided.</param>
		/// <param name="callerLineNumber">This parameter's value is compiler-generated, and should be left unprovided.</param>
		public static void Warn(
			object message,
			Exception associatedException = null,
			[CallerMemberName] string callerMemberName = null,
			[CallerFilePath] string callerFilePath = null,
			[CallerLineNumber] int callerLineNumber = -1
		) {
			try {
				lock (staticMutationLock) {
					LoggingProvider.WriteMessage(
						LogMessageSeverity.Warning,
						message.ToStringNullSafe(NULL_MESSAGE),
						associatedException,
						callerMemberName,
						callerFilePath,
						callerLineNumber
					);
				}
			}
			catch (IOException e) {
				System.Diagnostics.Debug.WriteLine("Warning: Failed to set write log message to '" + FileLocation + "'. " +
					"Exception details: " + e.GetAllMessages());
			}
		}

		/// <summary>
		/// Logs the given <paramref name="message"/> (and <paramref name="associatedException"/>, if one is provided) with a severity level of
		/// <see cref="LogMessageSeverity.Fatal"/>.
		/// </summary>
		/// <param name="message">The message to log. If an object of any type but string is provided, <c>message.ToString()</c> will be
		/// used as the message text. Null values are tolerated.</param>
		/// <param name="associatedException">The exception to log alongside the <paramref name="message"/>. May be null if there is no
		/// associated exception.</param>
		/// <param name="callerMemberName">This parameter's value is compiler-generated, and should be left unprovided.</param>
		/// <param name="callerFilePath">This parameter's value is compiler-generated, and should be left unprovided.</param>
		/// <param name="callerLineNumber">This parameter's value is compiler-generated, and should be left unprovided.</param>
		public static void Fatal(
			object message,
			Exception associatedException = null,
			[CallerMemberName] string callerMemberName = null,
			[CallerFilePath] string callerFilePath = null,
			[CallerLineNumber] int callerLineNumber = -1
		) {
			try {
				lock (staticMutationLock) {
					LoggingProvider.WriteMessage(
						LogMessageSeverity.Fatal,
						message.ToStringNullSafe(NULL_MESSAGE),
						associatedException,
						callerMemberName,
						callerFilePath,
						callerLineNumber
					);
				}
			}
			catch (IOException e) {
				System.Diagnostics.Debug.WriteLine("Warning: Failed to set write log message to '" + FileLocation + "'. " +
					"Exception details: " + e.GetAllMessages());
			}
		}

		private static void UpdateLoggingProviderFileLocation() {
			try {
				LoggingProvider.SetLogFileLocation(Path.Combine(LosgapSystem.MutableDataDirectory.FullName, FileLocation));
			}
			catch (IOException e) {
				System.Diagnostics.Debug.WriteLine("Warning: Failed to set log file location to '" + FileLocation + "'. " +
					"Exception details: " + e.GetAllMessages());
			}
			catch (UnauthorizedAccessException e) {
				System.Diagnostics.Debug.WriteLine("Warning: Failed to set log file location to '" + FileLocation + "'. " +
					"Exception details: " + e.GetAllMessages());
			}
		} 
	}
}