// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 06 10 2014 at 14:29 by Ben Bowen

// So we can write to the debug console in development mode
#if DEVELOPMENT
#define DEBUG
#endif

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;

namespace Ophidian.Losgap {
	/// <summary>
	/// Implementation of <see cref="ILoggingProvider"/> that is used by the <see cref="Logger"/> by default, until any other implementation
	/// is set (see <see cref="Logger.LoggingProvider"/>).
	/// </summary>
	/// <remarks>
	/// This class provides optional timestamping and caller info logging, and flushes each log event to the log file after writing.
	/// <para>
	/// Additionally, it broadcasts messages to stdout and the ingame developer console.
	/// </para>
	/// </remarks>
	public class DefaultLoggingProvider : ILoggingProvider {
		/// <summary>
		/// The timestamp format used to append timestamps to messages when <see cref="AddTimestamps"/> is true.
		/// </summary>
		public const string TIMESTAMP_FORMAT = "hh.mm:ss+fff";
		private const string FATAL_LOG_PREFIX = "!!!\t";
		private const string WARNING_LOG_PREFIX = " ! \t";
		private const string STANDARD_LOG_PREFIX = "   \t";
		private const string DEBUG_LOG_PREFIX = "   \t  ~";
		private const string ASSOCIATED_EXCEPTION_PREFIX = "\t\t\t\t\t\t\tAssociated exception: ";
		private const string STACK_TRACE_LINE_PREFIX = "\t\t\t\t\t\t\t\t";

		/// <summary>
		/// If true, the provider will add a timestamp before each log message.
		/// </summary>
		public readonly bool AddTimestamps;
		/// <summary>
		/// If true, the provider will append the callsite at the end of every log message.
		/// </summary>
		public readonly bool AppendCaller;
		/// <summary>
		/// If true, the provider will append detailed caller information at the end of every log message.
		/// </summary>
		public readonly bool IncludeDetailedCallerInfo;
		/// <summary>
		/// Synchronization object for locking over when mutating the state of this logger.
		/// </summary>
		protected readonly object InstanceMutationLock = new object();
		private readonly StringBuilder logLineBuilder = new StringBuilder();
		private TextWriter currentFileStreamWriter = null;
		private bool isDisposed = false;

		/// <summary>
		/// Whether or not the provider has been disposed.
		/// </summary>
		public bool IsDisposed {
			get {
				lock (InstanceMutationLock) {
					return isDisposed;
				}
			}
		}

		/// <summary>
		/// Constructs a new default logging provider.
		/// </summary>
		/// <param name="addTimestamps">If true, the provider will append a timestamp at the beginning of every log message.</param>
		/// <param name="appendCaller">If true, the provider will append the callsite at the end of every log message.</param>
		/// <param name="includeDetailedCallerInfo">If true, the provider will append detailed caller information at the end of every log message.
		/// Has no effect if <paramref name="appendCaller"/> is <c>false</c>.</param>
		public DefaultLoggingProvider(bool addTimestamps, bool appendCaller, bool includeDetailedCallerInfo) {
			if (includeDetailedCallerInfo && !appendCaller) includeDetailedCallerInfo = false;
			AddTimestamps = addTimestamps;
			AppendCaller = appendCaller;
			IncludeDetailedCallerInfo = includeDetailedCallerInfo;
		}

		/// <summary>
		/// Sets the new location for the log file. If a file was already open, it should be flushed and closed before the new one is opened.
		/// </summary>
		/// <param name="filePath">The location of the new file to open.</param>
		/// <exception cref="ObjectDisposedException">Thrown if <see cref="ILoggingProvider.IsDisposed"/> is true.</exception>
		/// <exception cref="UnauthorizedAccessException">Thrown if the file could not be opened because the application does
		/// not have the relevant permissions.</exception>
		/// <exception cref="IOException">Thrown if the file could not be opened for writing.</exception>
		/// <exception cref="ArgumentException">Thrown if <paramref name="filePath"/> does not represent a valid file path.</exception>
		public virtual void SetLogFileLocation(string filePath) {
			lock (InstanceMutationLock) {
				if (IsDisposed) throw new ObjectDisposedException("This logging provider has been disposed: Can not set new log file location!");
				if (!IOUtils.IsValidFilePath(filePath)) throw new ArgumentException("'" + filePath + "' is not a valid file path!", "filePath");

				DisposeCurrentStream();

#if !RELEASE && !DEVELOPMENT
				try {
					currentFileStreamWriter = File.CreateText(filePath);
				}
				catch (Exception e) {
					Assure.Fail("Could not create log file at '" + filePath + "'. Exception: " + e.GetAllMessages());
					throw;
				}
#else
				currentFileStreamWriter = File.CreateText(filePath);
#endif

#if !RELEASE && !DEVELOPMENT
				string openingMessage = "Platform version " + LosgapSystem.Version + ", debug build";
#elif DEVELOPMENT
				string openingMessage = "Platform version " + LosgapSystem.Version + ", development build";
#else
				var version = Assembly.GetExecutingAssembly().GetName().Version;
				string openingMessage = "Build " + version.Build + "." + version.Revision;
#endif

				currentFileStreamWriter.Write(LosgapSystem.ApplicationName + ", made by Egodystonic Studios " +
					"( http://www.escapelizards.com | http://www.egodystonic.com ) :: ");
				currentFileStreamWriter.WriteLine(openingMessage);
				currentFileStreamWriter.WriteLine("Installation directory set to " + LosgapSystem.InstallationDirectory.FullName + ".");
				currentFileStreamWriter.WriteLine("Mutable data directory set to " + LosgapSystem.MutableDataDirectory.FullName + ".");
				currentFileStreamWriter.WriteLine("Working directory is " + Environment.CurrentDirectory + ".");
				currentFileStreamWriter.WriteLine("Logical core count is " + Environment.ProcessorCount + ".");
				currentFileStreamWriter.WriteLine();
				currentFileStreamWriter.Flush();
			}
		}

		/// <summary>
		/// Writes a message to the log file.
		/// </summary>
		/// <param name="messageSeverity">The severity level of the message.</param>
		/// <param name="message">The message (should never be null).</param>
		/// <param name="associatedException">The exception, if any, associated with this message. If there is no associated
		/// exception, this parameter will be null.</param>
		/// <param name="callerMemberName">The name of the member that invoked this log message.</param>
		/// <param name="callerFilePath">The name of the file containing the member that invoked this log message.</param>
		/// <param name="callerLineNumber">The line in the <paramref name="callerFilePath"/> where this log message was invoked.</param>
		/// <exception cref="ObjectDisposedException">Thrown if <see cref="ILoggingProvider.IsDisposed"/> is true.</exception>
		/// <exception cref="InvalidOperationException">Thrown if this method is called before <see cref="ILoggingProvider.SetLogFileLocation"/> has
		/// been successfully called at least once.</exception>
		/// <exception cref="IOException">Thrown if the file could not be written to.</exception>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1",
			Justification = "Parameter is validated by the assurances.")]
		public virtual void WriteMessage(LogMessageSeverity messageSeverity, string message, Exception associatedException, string callerMemberName, string callerFilePath, int callerLineNumber) {
			Assure.NotNull(message);
			Assure.NotNull(callerMemberName);
			Assure.NotNull(callerFilePath);
			message = message.Replace(Environment.NewLine, Environment.NewLine + STACK_TRACE_LINE_PREFIX);

			lock (InstanceMutationLock) {
				if (IsDisposed) return; // Safest option: No point throwing exceptions if some random component elicits a warning on shutdown
				if (currentFileStreamWriter == null) {
					throw new InvalidOperationException("Can not write log line: No log file location has successfully been set on this logging provider!");
				}

				logLineBuilder.Clear();

				if (AddTimestamps) {
					logLineBuilder.Append(DateTime.Now.ToString(TIMESTAMP_FORMAT));
					logLineBuilder.Append("\t");
				}

				switch (messageSeverity) {
					case LogMessageSeverity.Standard:
						logLineBuilder.Append(STANDARD_LOG_PREFIX);
						break;
					case LogMessageSeverity.Debug:
						logLineBuilder.Append(DEBUG_LOG_PREFIX);
						break;
					case LogMessageSeverity.Warning:
						logLineBuilder.Append(WARNING_LOG_PREFIX);
						break;
					case LogMessageSeverity.Fatal:
						logLineBuilder.Append(FATAL_LOG_PREFIX);
						break;
				}

				logLineBuilder.Append(message);

				if (AppendCaller) {
					logLineBuilder.Append(" <= ");
					logLineBuilder.Append(callerMemberName);
					if (IncludeDetailedCallerInfo) {
						logLineBuilder.Append("() in ");
						logLineBuilder.Append(Path.GetFileName(callerFilePath));
						logLineBuilder.Append(":");
						logLineBuilder.Append(callerLineNumber);
					}
					else logLineBuilder.Append("()");
				}

				if (associatedException != null) {
					logLineBuilder.AppendLine();

					associatedException.ForEachException(e => {
						logLineBuilder.Append(ASSOCIATED_EXCEPTION_PREFIX);
						logLineBuilder.AppendLine(associatedException.GetAllMessages(true).Replace(Environment.NewLine, " "));

						if (e is ExternalException) {
							logLineBuilder.Append(STACK_TRACE_LINE_PREFIX);
							logLineBuilder.AppendLine("** External exception code: 0x" + (e as ExternalException).ErrorCode.ToString("x").ToUpper());
						}

						if (e.StackTrace != null) {
							foreach (string line in e.StackTrace.Split(Environment.NewLine)
								.Select(line => line.Trim())
								.Where(line => !line.IsNullOrWhiteSpace())) {
								logLineBuilder.Append(STACK_TRACE_LINE_PREFIX);
								logLineBuilder.AppendLine(line);
							}
						}
						else {
							logLineBuilder.Append(STACK_TRACE_LINE_PREFIX);
							logLineBuilder.AppendLine("<No stack trace>");
						}
						
					});
				}
				
				logLineBuilder.AppendLine();

#if !RELEASE && !DEVELOPMENT
				try {
					currentFileStreamWriter.Write(logLineBuilder);
					currentFileStreamWriter.Flush();
				}
				catch (Exception e) {
					Assure.Fail("Could not write message to log file. Exception: " + e.GetAllMessages());
					throw;
				}
#else
				currentFileStreamWriter.Write(logLineBuilder);
				currentFileStreamWriter.Flush();
#endif

				BroadcastMessage(messageSeverity, message, associatedException);
			}
		}

		/// <summary>
		/// Disposes the current file stream (if any).
		/// </summary>
		public virtual void Dispose() {
			lock (InstanceMutationLock) {
				DisposeCurrentStream();
				isDisposed = true;
			}
		}

		private void DisposeCurrentStream() {
			lock (InstanceMutationLock) {
				if (currentFileStreamWriter != null) {
					currentFileStreamWriter.Flush();
					currentFileStreamWriter.Dispose();
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "associatedException",
			Justification = "Parameter is used in non-release build."), 
		System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "message",
			Justification = "Parameter is used in non-release build."),
		System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "messageSeverity",
			Justification = "Parameter is used in non-release build.")]
		private static void BroadcastMessage(LogMessageSeverity messageSeverity, string message, Exception associatedException) {
#if !RELEASE
			string stdoutMessage = "Logger: "
				+ (messageSeverity == LogMessageSeverity.Standard ? String.Empty : "[" + messageSeverity + "] ")
				+ message
				+ (associatedException == null ? String.Empty : " Ex: " + associatedException.GetAllMessages(true));
			Debug.WriteLine(stdoutMessage);
#endif
		}
	}
}