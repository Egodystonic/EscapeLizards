// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 08 10 2014 at 13:37 by Ben Bowen

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap {
	public partial class ExtensionsTest {
		private class TestClass {
			public TestClass(int a) {
				
			}
		}
		private struct TestStruct {
			private string myString;
			public TestStruct(string myString) {
				this.myString = myString;
			}
		}

		#region Tests
		[TestMethod]
		public void TestIsInstantiable() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsTrue(typeof(object).IsInstantiable());
			Assert.IsFalse(typeof(Math).IsInstantiable());
			Assert.IsFalse(typeof(TestClass).IsInstantiable(true));
		}

		[TestMethod]
		public void TestGetDefaultValue() {
			// Define variables and constants

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(default(int), typeof(int).GetDefaultValue());
			Assert.AreEqual(default(bool), typeof(bool).GetDefaultValue());
			Assert.AreEqual(default(string), typeof(string).GetDefaultValue());
		}

		[TestMethod]
		public void TestIsStatic() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsTrue(typeof(Math).IsStatic());
			Assert.IsFalse(typeof(object).IsStatic());
		}

		[TestMethod]
		public void TestIsNumeric() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsTrue(typeof(int).IsNumeric());
			Assert.IsTrue(typeof(decimal).IsNumeric());
			Assert.IsTrue(typeof(byte).IsNumeric());
			Assert.IsTrue(typeof(ulong).IsNumeric());
			Assert.IsTrue(typeof(Number).IsNumeric());
			Assert.IsFalse(typeof(string).IsNumeric());
		}

		[TestMethod]
		public void TestIsBlittable() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsTrue(typeof(int).IsBlittable());
			Assert.IsTrue(typeof(IntPtr).IsBlittable());
			Assert.IsFalse(typeof(string).IsBlittable());
			Assert.IsFalse(typeof(TestStruct).IsBlittable());
		}

		[TestMethod]
		public void TestToType() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(typeof(double), Type.GetTypeCode(typeof(double)).ToType());
			Assert.AreEqual(typeof(char), Type.GetTypeCode(typeof(char)).ToType());
			Assert.AreEqual(typeof(string), Type.GetTypeCode(typeof(string)).ToType());
		}
		#endregion
	}
}