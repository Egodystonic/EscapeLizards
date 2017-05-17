// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 08 10 2014 at 14:19 by Ben Bowen

using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap {
	[TestClass]
	public class FileUtilsTest {

		[TestInitialize]
		public void SetUp() { }


		#region Tests
		[TestMethod]
		public void TestIsValidFilePath() {
			// Define variables and constants
			string[] validPaths = {
				@"C:\hello.txt",
				@"something.exe",
				@"$file.$$"
			};
			string[] invalidPaths = {
				null,
				String.Empty,
				@"C:\",
				@"\..\.",
				@"test*.log",
				@"\\shared\",
			};

			// Set up context


			// Execute


			// Assert outcome
			foreach (string validPath in validPaths) {
				Assert.IsTrue(IOUtils.IsValidFilePath(validPath));
			}
			foreach (string invalidPath in invalidPaths) {
				Assert.IsFalse(IOUtils.IsValidFilePath(invalidPath));
			}
		}
		#endregion
	}
}