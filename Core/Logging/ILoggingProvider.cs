// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 06 10 2014 at 13:33 by Ben Bowen

using System;
using System.Collections.Generic;
using System.IO;

namespace Ophidian.Losgap {
	/// <summary>
	/// Represents an object that takes log message details passed on to it via the <see cref="Logger"/>, and performs an action with them
	/// (usually writing them to a file in a specific format).
	/// </summary>
	public interface ILoggingProvider : IDisposable {
		/// <summary>
		/// Whether or not the provider has been disposed.
		/// </summary>
		bool IsDisposed { get; }

		/// <summary>
		/// Sets the new location for the log file. If a file was already open, it should be flushed and closed before the new one is opened.
		/// </summary>
		/// <param name="filePath">The location of the new file to open. This will be an absolute (fully qualified) path.</param>
		/// <exception cref="ObjectDisposedException">Thrown if <see cref="IsDisposed"/> is true.</exception>
		/// <exception cref="UnauthorizedAccessException">Thrown if the file could not be opened because the application does
		/// not have the relevant permissions.</exception>
		/// <exception cref="IOException">Thrown if the file could not be opened for writing.</exception>
		/// <exception cref="ArgumentException">Thrown if <paramref name="filePath"/> does not represent a valid file path.</exception>
		void SetLogFileLocation(string filePath);

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
		/// <exception cref="ObjectDisposedException">Thrown if <see cref="IsDisposed"/> is true.</exception>
		/// <exception cref="InvalidOperationException">Thrown if this method is called before <see cref="SetLogFileLocation"/> has
		/// been successfully called at least once.</exception>
		/// <exception cref="IOException">Thrown if the file could not be written to.</exception>
		void WriteMessage(
			LogMessageSeverity messageSeverity, 
			string message, 
			Exception associatedException, 
			string callerMemberName,
			string callerFilePath,
			int callerLineNumber
		);
	}
}