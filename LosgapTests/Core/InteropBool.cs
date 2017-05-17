// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 10 10 2014 at 09:12 by Ben Bowen

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ophidian.Losgap.Interop;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap {
	[TestClass]
	public class InteropBoolTest {

		[TestInitialize]
		public void SetUp() { }


		#region Tests
		[TestMethod]
		public void ShouldRepresentBoolConsistently() {
			// Define variables and constants
			InteropBool @true = true;
			InteropBool @false = false;

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsTrue(@true);
			Assert.IsFalse(@false);
			Assert.IsTrue(@true == true);
			Assert.IsTrue(@false == false);
			Assert.IsFalse(@true != true);
			Assert.IsFalse(@false != false);
			Assert.IsFalse(@true == @false);
			Assert.IsFalse(@false == @true);
			Assert.IsTrue(@true != @false);
			Assert.IsTrue(@false != @true);
		}

		[TestMethod]
		public unsafe void ShouldUseZeroForFalseOnly() {
			// Define variables and constants
			byte zero = 0;
			byte one = 1;
			byte twoFiveFive = 255;
			byte two = 2;

			// Set up context
			InteropBool interopZero = *((InteropBool*) &zero);
			InteropBool interopOne = *((InteropBool*) &one);
			InteropBool interopTwoFiveFive = *((InteropBool*) &twoFiveFive);
			InteropBool interopTwo = *((InteropBool*) &two);

			// Execute


			// Assert outcome
			Assert.IsFalse(interopZero);
			Assert.IsTrue(interopOne);
			Assert.IsTrue(interopTwoFiveFive);
			Assert.IsTrue(interopTwo);
		}
		#endregion
	}
}