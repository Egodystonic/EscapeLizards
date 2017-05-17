// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 07 10 2014 at 11:40 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;

namespace Ophidian.Losgap {
	public sealed class MockLoggingProvider : ILoggingProvider {
		public string LastLogFileLocation = null;
		public LogMessageSeverity LasMessageSeverity;
		public string LastMessage;
		public Exception LastMessageException;
		private bool isDisposed;

		public void Dispose() {
			isDisposed = true;
		}

		public bool IsDisposed {
			get { return isDisposed; }
		}

		public void SetLogFileLocation(string filePath) {
			LastLogFileLocation = filePath;
		}

		public void WriteMessage(LogMessageSeverity messageSeverity, string message, Exception associatedException, string callerMemberName, string callerFilePath, int callerLineNumber) {
			LasMessageSeverity = messageSeverity;
			LastMessage = message;
			LastMessageException = associatedException;
		}
	}
}