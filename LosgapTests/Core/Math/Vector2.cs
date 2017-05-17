// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 20 11 2014 at 14:52 by Ben Bowen

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap {
	[TestClass]
	public class Vector2Test {

		[TestInitialize]
		public void SetUp() { }


		#region Tests
		[TestMethod]
		public void ShouldCorrectlySupportArithmeticOperators() {
			// Define variables and constants
			Vector2 vectorA = new Vector2(2f, 3f);
			Vector2 vectorB = new Vector2(1.5f, -3f);
			float scalarK = 2f;

			// Assert outcome
			Assert.AreEqual(vectorA + vectorB, new Vector2(3.5f, 0f));
			Assert.AreEqual(vectorA - vectorB, new Vector2(0.5f, 6f));
			Assert.AreEqual(vectorA * scalarK, new Vector2(4f, 6f));
			Assert.AreEqual(vectorB / scalarK, new Vector2(0.75f, -1.5f));
			Assert.AreEqual(scalarK * vectorB, new Vector2(3f, -6f));

			Assert.AreEqual(-10.5f, vectorA * vectorB);
			Assert.AreEqual(-vectorA, new Vector2(-2f, -3f));

			Assert.AreEqual(vectorA > vectorB, vectorA.Length > vectorB.Length);
			Assert.AreEqual(vectorB < vectorA, vectorB.Length < vectorA.Length);
			Assert.AreEqual(vectorA >= vectorB, vectorA.Length >= vectorB.Length);
			Assert.AreEqual(vectorB <= vectorA, vectorB.Length <= vectorA.Length);
		}

		[TestMethod]
		public void ShouldCorrectlySupportVectorCalculations() {
			// Define variables and constants
			Vector2 vector = new Vector2(10f, 100f);
			Vector2 forwardVector = new Vector2(10f, 0f);
			Vector2 rightVector = new Vector2(0f, 10f);

			// Assert outcome
			Assert.AreEqual(100.4988f, vector.Length, 0.01f);
			Assert.IsTrue(Math.Abs(Math.Pow(100.4988f, 2d) - vector.LengthSquared) < 0.01d);
			Assert.AreEqual(new Vector2(0.100f, 0.995f), vector.ToUnit());
			Assert.IsTrue(vector.ToUnit().IsUnit);

			Assert.AreEqual(MathUtils.PI_OVER_TWO, Vector2.AngleBetween(forwardVector, rightVector));
			Assert.AreEqual(Vector2.AngleBetween(forwardVector, rightVector), Vector2.AngleBetween(rightVector, forwardVector));

			Assert.AreEqual(100f, forwardVector * rightVector);
		}

		[TestMethod]
		public void ShouldCorrectlySwizzleVectors() {
			// Define variables and constants
			Vector2 vector = new Vector2(0f, 1f);

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(new Vector2(1f, 0f), vector[1, 0]);
			Assert.AreEqual(new Vector3(1f, 0f, 0f), vector[1, 0, 0]);
			Assert.AreEqual(new Vector4(1f, 0f, 0f, 1f), vector[1, 0, 0, 1]);
		}

		[TestMethod]
		public void ShouldEquateVectorsWithFPTolerance() {
			// Define variables and constants
			Vector2 v1 = new Vector2(16f, 16f);
			Vector2 v2 = new Vector2(16.000001f, 15.99996523f);

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(v1, v2);
			Assert.IsFalse(v1.EqualsExactly(v2));
		}

		[TestMethod]
		public void TestIndexersAndSwizzles() {
			// Define variables and constants
			Vector2 oneTwoThreeFour = new Vector2(1f, 2f);

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(1f, oneTwoThreeFour[0]);
			Assert.AreEqual(2f, oneTwoThreeFour[1]);

			Assert.AreEqual(new Vector2(1f, 2f), oneTwoThreeFour[0, 1]);
		}

		[TestMethod]
		public void TestIsUnit() {
			// Define variables and constants
			Vector2[] units = {
				Vector2.ONE.ToUnit(),
				Vector2.UP,
				Vector2.DOWN,
				Vector2.LEFT,
				Vector2.RIGHT,
				new Vector2(1351f, 1351f).ToUnit(), 
			};
			Vector2[] notUnits = {
				Vector2.ONE,
				Vector2.UP * -3f,
				Vector2.DOWN * 0.5f,
				Vector2.LEFT * -0.5f,
				Vector2.RIGHT * 0.9f,
				new Vector2(1351f, 1351f), 
			};

			// Set up context


			// Execute


			// Assert outcome
			foreach (Vector2 vector in units) {
				Assert.IsTrue(vector.IsUnit);
			}
			foreach (Vector2 vector in notUnits) {
				Assert.IsFalse(vector.IsUnit);
			}
		}

		[TestMethod]
		public void TestLength() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(2f, new Vector2(2f, 0f).Length);
			Assert.AreEqual(2f, new Vector2(-2f, 0f).Length);
			Assert.AreEqual(Math.Sqrt(10f * 10f + -15f * -15f), new Vector2(10f, -15f).Length, 0.00001d);
		}

		[TestMethod]
		public void TestLengthSquared() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(4f, new Vector2(2f, 0f).LengthSquared);
			Assert.AreEqual(4f, new Vector2(-2f, 0f).LengthSquared);
			Assert.AreEqual(10f * 10f + -15f * -15f, new Vector2(10f, -15f).LengthSquared);
		}

		[TestMethod]
		public void TestAngleBetween() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(MathUtils.PI, Vector2.AngleBetween(Vector2.UP, Vector2.DOWN));
			Assert.AreEqual(MathUtils.PI_OVER_TWO, Vector2.AngleBetween(Vector2.UP, Vector2.RIGHT));
			Assert.AreEqual(MathUtils.PI_OVER_TWO, Vector2.AngleBetween(Vector2.DOWN, Vector2.LEFT));
			Assert.AreEqual(0f, Vector2.AngleBetween(Vector2.UP, Vector2.UP));
			Assert.AreEqual(MathUtils.DegToRad(78.11134f), Vector2.AngleBetween(new Vector2(2f, -3f), new Vector2(5f, 2f)), 0.001d);
		}

		[TestMethod]
		public void TestCrossProduct() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(-2f, new Vector2(1f, 2f) * new Vector2(3f, 4f));
			Assert.AreEqual(-1f, new Vector2(-1f, 25f) * new Vector2(0f, 1f));
		}

		[TestMethod]
		public void TestToUnit() {
			// Define variables and constants
			Vector2[] notUnits = {
				Vector2.ONE * 3f,
				Vector2.UP * -3f,
				Vector2.DOWN * 0.5f,
				Vector2.LEFT * -0.5f,
				Vector2.RIGHT * 0.9999f,
				new Vector2(1351f, 1351f), 
			};
			Vector2[] units = {
				Vector2.ONE.ToUnit(),
				Vector2.DOWN,
				Vector2.DOWN,
				Vector2.RIGHT,
				Vector2.RIGHT,
				new Vector2(0.707f, 0.707f), 
			};
			

			// Set up context


			// Execute


			// Assert outcome
			for (int i = 0; i < notUnits.Length; i++) {
				Assert.AreEqual(units[i], notUnits[i].ToUnit());
			}
		}

		[TestMethod]
		public void TestWithLength() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(new Vector2(0f, 3f), Vector2.UP.WithLength(3f));
			Assert.AreEqual(new Vector2(0f, -3f), Vector2.UP.WithLength(-3f));
			Assert.AreEqual(new Vector2(1.414f, 1.414f), new Vector2(1351f, 1351f).WithLength(2f));
			Assert.AreEqual(Vector2.ZERO, new Vector2(31f, 3561f).WithLength(0f));
		}

		[TestMethod]
		public void TestDistance() {
			// Define variables and constants
			Vector2 a = new Vector2(5f, 5f);
			Vector2 b = new Vector2(7f, 7f);
			Vector2 c = new Vector2(5f, 5f);

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual((float) Math.Sqrt(8), Vector2.Distance(a, b));
			Assert.AreEqual(0f, Vector2.Distance(a, c));
			Assert.AreEqual((float) Math.Sqrt(8), Vector2.Distance(b, c));
		}

		[TestMethod]
		public void TestLerp() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(new Vector2(7.5f, 7.5f), Vector2.Lerp(new Vector2(5f, 5f), new Vector2(10f, 10f), 0.5f));
			Assert.AreEqual(new Vector2(9f, 9f), Vector2.Lerp(new Vector2(5f, 5f), new Vector2(10f, 10f), 0.8f));
			Assert.AreEqual(new Vector2(5f, 5f), Vector2.Lerp(new Vector2(5f, 5f), new Vector2(10f, 10f), 0f));
		}

		[TestMethod]
		public void TestProjectedOnTo() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(new Vector2(0f, 5f), new Vector2(5f, 5f).ProjectedOnto(Vector2.UP));
			Assert.AreEqual(new Vector2(0f, -5f), new Vector2(5f, -5f).ProjectedOnto(Vector2.UP));
			Assert.AreEqual(new Vector2(10f, 10f), new Vector2(10f, 10f).ProjectedOnto(Vector2.ONE[0, 1]));
		}
		#endregion
	}
}