// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 07 10 2014 at 14:25 by Ben Bowen

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ophidian.Losgap {
	/// <summary>
	/// Represents a logging provider that the unit tests can set that will do nothing except print to console
	/// </summary>
	public sealed class TestLoggingProvider : DefaultLoggingProvider {
		private uint suppressMessageCounter = 0U;
		private uint discardCounter = 0U;

		public bool SuppressionActive {
			get {
				lock (InstanceMutationLock) {
					return suppressMessageCounter != 0U;
				}
			}
		}

		public TestLoggingProvider() : base(true, true, true) { }

		public void SuppressMessages() {
			lock (InstanceMutationLock) {
				++suppressMessageCounter;
			}
		}

		public void AllowMessages() {
			lock (InstanceMutationLock) {
				if (suppressMessageCounter == 0U) throw new InvalidOperationException("Suppression is not already active.");
				--suppressMessageCounter;
				if (suppressMessageCounter == 0U) {
#if !DEVELOPMENT && !RELEASE
					Logger.Debug(discardCounter + " messages suppressed.");
#endif
					discardCounter = 0U;
				}
			}
		}

		public override void WriteMessage(LogMessageSeverity messageSeverity, string message, Exception associatedException, string callerMemberName, string callerFilePath, int callerLineNumber) {
			lock (InstanceMutationLock) {
				if (suppressMessageCounter > 0U && messageSeverity != LogMessageSeverity.Fatal) {
					++discardCounter;
					return;
				}
			}
			base.WriteMessage(messageSeverity, message, associatedException, callerMemberName, callerFilePath, callerLineNumber);
		}
	}
}