// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 08 10 2014 at 14:38 by Ben Bowen

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap {
	[TestClass]
	public class VersionTest {

		[TestInitialize]
		public void SetUp() { }


		#region Tests
		[TestMethod]
		public void TestStringCtor() {
			// Define variables and constants
			const string V2_0_STRING = "2";
			const string V2_1_STRING = "2.1";
			Version v2_0a = new Version(V2_0_STRING);
			Version v2_1a = new Version(V2_1_STRING);
			Version v2_0b = new Version(v2_0a.ToString());
			Version v2_1b = new Version(v2_1a.ToString());

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(2, v2_0a.MajorVersion);
			Assert.AreEqual(0, v2_0b.MinorVersion);
			Assert.AreEqual(2, v2_1a.MajorVersion);
			Assert.AreEqual(1, v2_1b.MinorVersion);
		}

		[TestMethod]
		public void TestEquality() {
			// Define variables and constants
			Version v2_0a = new Version(2, 0);
			Version v2_0b = new Version(2, 0);
			Version v2_1 = new Version(2, 1);
			Version v1_0 = new Version(1, 0);

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(v2_0a, v2_0a);
			Assert.AreEqual(v2_0a, v2_0b);
			Assert.AreNotEqual(v2_0a, v2_1);
			Assert.AreNotEqual(v2_0b, v1_0);
		}

		[TestMethod]
		public void TestComparisons() {
			// ReSharper disable EqualExpressionComparison
			// Define variables and constants
			Version v2_1 = new Version(2, 1);
			Version v2_0 = new Version(2, 0);
			Version v3_0 = new Version(3, 0);
			Version v1_9 = new Version(1, 9);

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsTrue(v2_1 > v2_0);
			Assert.IsFalse(v2_1 > v3_0);
			Assert.IsTrue(v2_1 > v1_9);

			Assert.IsTrue(v2_0 < v3_0);
			Assert.IsFalse(v2_0 < v1_9);

			Assert.IsFalse(v3_0 <= v1_9);
#pragma warning disable 1718
			Assert.IsTrue(v3_0 >= v3_0);
#pragma warning restore 1718
			// ReSharper enable EqualExpressionComparison
		}
		#endregion
	}
}