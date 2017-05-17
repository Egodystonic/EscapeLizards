// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 07 10 2014 at 11:32 by Ben Bowen

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap {
	[TestClass]
	public class DefaultLoggingProviderTest {

		[TestInitialize]
		public void SetUp() { }


		#region Tests
		[TestMethod]
		[ExpectedException(typeof(ObjectDisposedException))]
		public void ShouldThrowWhenSettingFileIfDisposed() {
			// Define variables and constants
			DefaultLoggingProvider provider = new DefaultLoggingProvider(true, true, true);
			provider.Dispose();

			// Set up context


			// Execute
			provider.SetLogFileLocation("test.log");

			// Assert outcome

		}

		[TestMethod]
		public void ShouldSilentlyFailWhenWritingMessageIfDisposed() {
			// Define variables and constants
			DefaultLoggingProvider provider = new DefaultLoggingProvider(true, true, true);
			provider.Dispose();

			// Set up context


			// Execute
			provider.WriteMessage(LogMessageSeverity.Standard, "Unit test", null, "UnitTest", "UnitTest.cs", 0);

			// Assert outcome

		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void ShouldThrowWhenWritingAMessageBeforeFileIsSet() {
			// Define variables and constants
			DefaultLoggingProvider provider = new DefaultLoggingProvider(true, true, true);
			
			// Set up context


			// Execute
			provider.WriteMessage(LogMessageSeverity.Standard, "Unit test", null, "UnitTest", "UnitTest.cs", 0);

			// Assert outcome

		}
		#endregion
	}
}