// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 08 10 2014 at 13:37 by Ben Bowen

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap {
	public partial class ExtensionsTest {
		#region Tests
		[TestMethod]
		public void TestIsNullOrEmpty() {
			// Define variables and constants
			const string NULL_STRING = null;
			const string EMPTY_STRING = "";
			const string WHITESPACE_STRING = " ";
			const string TEXT_STRING = "text";

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsTrue(NULL_STRING.IsNullOrEmpty());
			Assert.IsTrue(EMPTY_STRING.IsNullOrEmpty());
			Assert.IsFalse(WHITESPACE_STRING.IsNullOrEmpty());
			Assert.IsFalse(TEXT_STRING.IsNullOrEmpty());
		}

		[TestMethod]
		public void TestIsNullOrWhitespace() {
			// Define variables and constants
			const string NULL_STRING = null;
			const string EMPTY_STRING = "";
			const string WHITESPACE_STRING = " ";
			const string TEXT_STRING = "text";

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsTrue(NULL_STRING.IsNullOrWhiteSpace());
			Assert.IsTrue(EMPTY_STRING.IsNullOrWhiteSpace());
			Assert.IsTrue(WHITESPACE_STRING.IsNullOrWhiteSpace());
			Assert.IsFalse(TEXT_STRING.IsNullOrWhiteSpace());
		}

		[TestMethod]
		public void TestSplit() {
			// Define variables and constants
			const string TARGET_STRING = "This is a test string. There is a double  space in this sentence.";
			const string SEPARATOR = " ";
			string[] bclSplit = TARGET_STRING.Split(new[] { SEPARATOR }, StringSplitOptions.None);
			string[] bclSplitRemoveEmpty = TARGET_STRING.Split(new[] { SEPARATOR }, StringSplitOptions.RemoveEmptyEntries);

			// Set up context
			

			// Execute
			string[] extSplit = TARGET_STRING.Split(SEPARATOR, StringSplitOptions.None);
			string[] extSplitRemoveEmpty = TARGET_STRING.Split(SEPARATOR, StringSplitOptions.RemoveEmptyEntries);

			// Assert outcome
			Assert.AreEqual(bclSplit.Length, extSplit.Length);
			Assert.AreEqual(bclSplitRemoveEmpty.Length, extSplitRemoveEmpty.Length);
			for (int i = 0; i < bclSplit.Length; i++) {
				Assert.AreEqual(bclSplit[i], extSplit[i]);
			}
			for (int i = 0; i < bclSplitRemoveEmpty.Length; i++) {
				Assert.AreEqual(bclSplitRemoveEmpty[i], extSplitRemoveEmpty[i]);
			}
		}

		[TestMethod]
		public void TestRepeat() {
			// Define variables and constants
			const string DOG_STRING = "dog";
			const string SEPARATOR = "happy";

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual("dogdogdog", DOG_STRING.Repeat(3));
			Assert.AreEqual("doghappydoghappydog", DOG_STRING.Repeat(3, SEPARATOR));
		}
		#endregion
	}
}