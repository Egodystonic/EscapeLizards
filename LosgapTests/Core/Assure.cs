// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 07 10 2014 at 11:18 by Ben Bowen

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap {
	[TestClass]
	public class AssureTest {

		[TestInitialize]
		public void SetUp() {
			Assure.FailureStrategy = AssuranceFailureStrategy.ThrowException;
		}

		#region Tests
		[TestMethod]
		public void TestNotNull() {
			// ReSharper disable ExpressionIsAlwaysNull
			// Define variables and constants
			object nonNullObj = new object();
			object nullObj = null;
			int? nonNullInt = 3;
			int? nullInt = null;

			// Set up context


			// Execute
			Assure.NotNull(nonNullObj);
			try {
				Assure.NotNull(nullObj);
				Assert.Fail(); // Should throw the exception
			}
			catch (AssuranceFailedException) { }

			Assure.NotNull(nonNullInt);
			try {
				Assure.NotNull(nullInt);
				Assert.Fail(); // Should throw the exception
			}
			catch (AssuranceFailedException) { }

			// Assert outcome

			// ReSharper restore ExpressionIsAlwaysNull
		}

		[TestMethod]
		public void TestNull() {
			// ReSharper disable ExpressionIsAlwaysNull
			// Define variables and constants
			object nonNullObj = new object();
			object nullObj = null;
			int? nonNullInt = 3;
			int? nullInt = null;

			// Set up context


			// Execute
			Assure.Null(nullObj);
			try {
				Assure.Null(nonNullObj);
				Assert.Fail(); // Should throw the exception
			}
			catch (AssuranceFailedException) { }

			Assure.Null(nullInt);
			try {
				Assure.Null(nonNullInt);
				Assert.Fail(); // Should throw the exception
			}
			catch (AssuranceFailedException) { }

			// Assert outcome

			// ReSharper restore ExpressionIsAlwaysNull
		}

		[TestMethod]
		[ExpectedException(typeof(AssuranceFailedException))]
		public void TestFail() {
			// Define variables and constants
			

			// Set up context


			// Execute
			Assure.Fail("Unit test");

			// Assert outcome

		}

		[TestMethod]
		public void TestAny() {
			// Define variables and constants
			IEnumerable<int> passingCollection = new[] {
				1, 2, 7, -1, 99, 12
			};
			IEnumerable<int> failingCollection = new[] {
				5, 136, 136, 76, 8, 32, 0
			};

			// Set up context


			// Execute
			Assure.Any(passingCollection, i => i < 0);
			try {
				Assure.Any(failingCollection, i => i < 0);
				Assert.Fail();
			}
			catch (AssuranceFailedException) {

			}

			// Assert outcome

		}

		[TestMethod]
		public void TestAll() {
			// Define variables and constants
			IEnumerable<int> passingCollection = new[] {
				1, 2, 7, 31, 99, 12
			};
			IEnumerable<int> failingCollection = new[] {
				5, 136, 136, 76, 8, 32, 0
			};

			// Set up context


			// Execute
			Assure.All(passingCollection, i => i > 0);
			try {
				Assure.All(failingCollection, i => i > 0);
				Assert.Fail();
			}
			catch (AssuranceFailedException) {

			}

			// Assert outcome

		}

		[TestMethod]
		public void TestNone() {
			// Define variables and constants
			IEnumerable<int> passingCollection = new[] {
				1, 2, 7, 31, 99, 12
			};
			IEnumerable<int> failingCollection = new[] {
				5, 136, 136, 76, 8, 32, 0
			};

			// Set up context


			// Execute
			Assure.None(passingCollection, i => i <= 0);
			try {
				Assure.None(failingCollection, i => i <= 0);
				Assert.Fail();
			}
			catch (AssuranceFailedException) {

			}

			// Assert outcome

		}

		[TestMethod]
		public void TestEqual() {
			// Define variables and constants
			const string DOG_STRING_A = "Dog";
			const string DOG_STRING_B = "Dog";
			const string CAT_STRING = "Cat";

			// Set up context


			// Execute
			Assure.Equal(DOG_STRING_A, DOG_STRING_B);
			try {
				Assure.Equal(DOG_STRING_A, CAT_STRING);
				Assert.Fail();
			}
			catch (AssuranceFailedException) {

			}

			Assure.Equal(1, 1);
			try {
				Assure.Equal(1, 2);
				Assert.Fail();
			}
			catch (AssuranceFailedException) {

			}

			// Assert outcome

		}

		[TestMethod]
		public void TestNotEqual() {
			// Define variables and constants
			const string DOG_STRING_A = "Dog";
			const string DOG_STRING_B = "Dog";
			const string CAT_STRING = "Cat";

			// Set up context


			// Execute
			Assure.NotEqual(DOG_STRING_A, CAT_STRING);
			try {
				Assure.NotEqual(DOG_STRING_A, DOG_STRING_B);
				Assert.Fail();
			}
			catch (AssuranceFailedException) {

			}

			// Assert outcome
			Assure.NotEqual(new DateTime(400L), new DateTime(500L));
			try {
				Assure.NotEqual(new DateTime(400L), new DateTime(400L));
				Assert.Fail();
			}
			catch (AssuranceFailedException) {

			}
		}

		[TestMethod]
		public void True() {
			// Define variables and constants

			// Set up context


			// Execute
			Assure.True(true);
			try {
				Assure.True(false);
				Assert.Fail();
			}
			catch (AssuranceFailedException) {

			}

			// Assert outcome

		}

		[TestMethod]
		public void False() {
			// Define variables and constants

			// Set up context


			// Execute
			Assure.False(false);
			try {
				Assure.False(true);
				Assert.Fail();
			}
			catch (AssuranceFailedException) {

			}

			// Assert outcome

		}

		[TestMethod]
		public void TestGreaterThan() {
			// Define variables and constants
			const int THREE = 3;
			const int FOUR = 4;

			// Set up context


			// Execute
			Assure.GreaterThan(FOUR, THREE);
			try {
				Assure.GreaterThan(THREE, FOUR);
				Assert.Fail();
			}
			catch (AssuranceFailedException) {

			}

			// Assert outcome

		}

		[TestMethod]
		public void TestGreaterThanOrEqualTo() {
			// Define variables and constants
			const int THREE = 3;
			const int FOUR = 4;

			// Set up context


			// Execute
			Assure.GreaterThanOrEqualTo(FOUR, THREE);
			Assure.GreaterThanOrEqualTo(FOUR, FOUR);
			try {
				Assure.GreaterThanOrEqualTo(THREE, FOUR);
				Assert.Fail();
			}
			catch (AssuranceFailedException) {

			}

			// Assert outcome

		}

		[TestMethod]
		public void TestLessThan() {
			// Define variables and constants
			const int FIVE = 5;
			const int FOUR = 4;

			// Set up context


			// Execute
			Assure.LessThan(FOUR, FIVE);
			try {
				Assure.LessThan(FIVE, FOUR);
				Assert.Fail();
			}
			catch (AssuranceFailedException) {

			}

			// Assert outcome

		}

		[TestMethod]
		public void TestLessThanOrEqualTo() {
			// Define variables and constants
			const int FIVE = 5;
			const int FOUR = 4;

			// Set up context


			// Execute
			Assure.LessThanOrEqualTo(FOUR, FIVE);
			Assure.LessThanOrEqualTo(FOUR, FOUR);
			try {
				Assure.LessThanOrEqualTo(FIVE, FOUR);
				Assert.Fail();
			}
			catch (AssuranceFailedException) {

			}

			// Assert outcome

		}

		[TestMethod]
		public void TestBetween() {
			// Define variables and constants
			const int FIVE = 5;
			const int FOUR = 4;
			const int THREE = 3;

			// Set up context


			// Execute
			Assure.Between(FOUR, THREE, FIVE);
			try {
				Assure.Between(FOUR, FOUR, FIVE);
				Assert.Fail();
			}
			catch (AssuranceFailedException) {

			}

			// Assert outcome

		}

		[TestMethod]
		public void TestBetweenOrEqualTo() {
			// Define variables and constants
			const int FIVE = 5;
			const int FOUR = 4;
			const int THREE = 3;

			// Set up context


			// Execute
			Assure.BetweenOrEqualTo(FOUR, THREE, FIVE);
			Assure.BetweenOrEqualTo(FOUR, FOUR, FIVE);
			try {
				Assure.BetweenOrEqualTo(THREE, FOUR, FIVE);
				Assert.Fail();
			}
			catch (AssuranceFailedException) {

			}

			// Assert outcome

		}
		#endregion
	}
}