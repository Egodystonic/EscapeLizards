// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 07 10 2014 at 11:38 by Ben Bowen

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap {
	[TestClass]
	public class LoggerTest {

		[TestInitialize]
		public void SetUp() { }


		#region Tests
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ShouldRejectNullLoggingProviders() {
			// ReSharper disable ExpressionIsAlwaysNull
			// Define variables and constants
			ILoggingProvider nullLoggingProvider = null;

			// Set up context


			// Execute
			Logger.LoggingProvider = nullLoggingProvider;

			// Assert outcome
			// ReSharper restore ExpressionIsAlwaysNull
		}

		[TestMethod]
		[ExpectedException(typeof(ObjectDisposedException))]
		public void ShouldRejectDisposedLoggingProviders() {
			// Define variables and constants
			ILoggingProvider disposedLoggingProvider = new MockLoggingProvider();
			disposedLoggingProvider.Dispose();

			// Set up context


			// Execute
			Logger.LoggingProvider = disposedLoggingProvider;

			// Assert outcome

		}

#if TEST_LOGGING // Because these methods work on a singleton that might mess with the TestLoggingProvider that we set at assembly init
		[TestMethod]
		public void ShouldDisposeOldProviderWhenSettingANewOne() {
			// Define variables and constants
			ILoggingProvider oldProvider = new MockLoggingProvider();
			ILoggingProvider newProvider = new MockLoggingProvider();

			// Set up context
			Logger.LoggingProvider = oldProvider;

			// Execute
			Logger.LoggingProvider = newProvider;

			// Assert outcome
			Assert.IsTrue(oldProvider.IsDisposed);
			Assert.IsFalse(newProvider.IsDisposed);
		}

		[TestMethod]
		public void ShouldTellProviderToOpenFileAfterItIsSet() {
			// Define variables and constants
			MockLoggingProvider provider = new MockLoggingProvider();

			// Set up context

	
			// Execute
			Logger.LoggingProvider = provider;

			// Assert outcome
			Assert.AreEqual(Logger.FileLocation, provider.LastLogFileLocation);
		}

		[TestMethod]
		public void ShouldUpdateFileLocationInProviderWhenSet() {
			// Define variables and constants
			const string TEST_FILE_LOCATION = "test.log";
			MockLoggingProvider provider = new MockLoggingProvider();

			// Set up context
			Logger.LoggingProvider = provider;

			// Execute
			Logger.FileLocation = TEST_FILE_LOCATION;

			// Assert outcome
			Assert.AreEqual(TEST_FILE_LOCATION, provider.LastLogFileLocation);
		}
		
		[TestMethod]
		public void TestLogMethods() {
			// Define variables and constants
			string[] messages = new string[] {
				"1", "2", "3", "4"
			};
			Exception[] exceptions = new Exception[] {
				new ApplicationException(),
				new ApplicationException(),
				new ApplicationException(),
				new ApplicationException()
			};
			MockLoggingProvider provider = new MockLoggingProvider();

			// Set up context
			Logger.LoggingProvider = provider;

			// Execute
			for (int i = 0; i < 4; ++i) {
				switch (i) {
					case 0:
						Logger.Debug(messages[i], exceptions[i]);
						Assert.AreEqual(LogMessageSeverity.Debug, provider.LasMessageSeverity);
						break;
					case 1:
						Logger.Log(messages[i], exceptions[i]);
						Assert.AreEqual(LogMessageSeverity.Standard, provider.LasMessageSeverity);
						break;
					case 2:
						Logger.Warn(messages[i], exceptions[i]);
						Assert.AreEqual(LogMessageSeverity.Warning, provider.LasMessageSeverity);
						break;
					case 3:
						Logger.Fatal(messages[i], exceptions[i]);
						Assert.AreEqual(LogMessageSeverity.Fatal, provider.LasMessageSeverity);
						break;
				}

				Assert.AreEqual(messages[i], provider.LastMessage);
				Assert.AreEqual(exceptions[i], provider.LastMessageException);
			}	

			// Assert outcome

		}
#endif
		#endregion
	}
}