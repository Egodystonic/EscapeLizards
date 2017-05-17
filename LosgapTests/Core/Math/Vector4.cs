// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 20 11 2014 at 14:52 by Ben Bowen

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap {
	[TestClass]
	public class Vector4Test {

		[TestInitialize]
		public void SetUp() { }


		#region Tests
		[TestMethod]
		public void ShouldCorrectlySupportArithmeticOperators() {
			// Define variables and constants
			Vector4 vectorA = new Vector4(2f, 3f, 4f, 5f);
			Vector4 vectorB = new Vector4(1.5f, -3f, 4.5f, -10f);
			float scalarK = 2f;

			// Assert outcome
			Assert.AreEqual(vectorA + vectorB, new Vector4(3.5f, 0f, 8.5f, -5f));
			Assert.AreEqual(vectorA - vectorB, new Vector4(0.5f, 6f, -0.5f, 15f));
			Assert.AreEqual(vectorA * scalarK, new Vector4(4f, 6f, 8f, 10f));
			Assert.AreEqual(vectorB / scalarK, new Vector4(0.75f, -1.5f, 2.25f, -5f));
			Assert.AreEqual(scalarK * vectorB, new Vector4(3f, -6f, 9f, -20f));

			Assert.AreEqual(new Vector4(25.500f, -3.000f, -10.500f, 0.000f), vectorA * vectorB);
			Assert.AreEqual(-vectorA, new Vector4(-2f, -3f, -4f, -5f));

			Assert.AreEqual(vectorA > vectorB, vectorA.Length > vectorB.Length);
			Assert.AreEqual(vectorB < vectorA, vectorB.Length < vectorA.Length);
			Assert.AreEqual(vectorA >= vectorB, vectorA.Length >= vectorB.Length);
			Assert.AreEqual(vectorB <= vectorA, vectorB.Length <= vectorA.Length);
		}

		[TestMethod]
		public void ShouldCorrectlySupportVectorCalculations() {
			// Define variables and constants
			Vector4 vector = new Vector4(10f, 100f, 500f, -20f);
			Vector4 forwardVector = new Vector4(10f, 0f, 0f, 0f);
			Vector4 rightVector = new Vector4(0f, 10f, 0f, 0f);

			// Assert outcome
			Assert.AreEqual(510.392f, vector.Length);
			Assert.IsTrue(Math.Abs(Math.Pow(510.392d, 2d) - vector.LengthSquared) < 0.01d);
			Assert.AreEqual(new Vector4(0.0195928f, 0.195928f, 0.979639f, -0.0391856f), vector.ToUnit());
			Assert.IsTrue(vector.ToUnit().IsUnit);

			Assert.AreEqual(MathUtils.PI_OVER_TWO, Vector4.AngleBetween(forwardVector, rightVector));
			Assert.AreEqual(Vector4.AngleBetween(forwardVector, rightVector), Vector4.AngleBetween(rightVector, forwardVector));

			Assert.AreEqual(new Vector4(0f, 0f, 1f, 0f), (forwardVector * rightVector).ToUnit());

			Assert.AreEqual(new Vector4(0f, 0f, 100f, 0f), forwardVector * rightVector);
		}

		[TestMethod]
		public void ShouldCorrectlySwizzleVectors() {
			// Define variables and constants
			Vector4 vector = new Vector4(0f, 1f, 2f, 3f);

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(new Vector4(2f, 1f, 0f, 3f), vector[2, 1, 0, 3]);
		}

		[TestMethod]
		public void ShouldEquateVectorsWithFPTolerance() {
			// Define variables and constants
			Vector4 v1 = new Vector4(16f, 16f, 16f, 16f);
			Vector4 v2 = new Vector4(16.000001f, 15.99996523f, 15.99990f, 16.00099999f);

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(v1, v2);
			Assert.IsFalse(v1.EqualsExactly(v2));
		}

		[TestMethod]
		public void TestIndexersAndSwizzles() {
			// Define variables and constants
			Vector4 oneTwoThreeFour = new Vector4(1f, 2f, 3f, 4f);

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(1f, oneTwoThreeFour[0]);
			Assert.AreEqual(2f, oneTwoThreeFour[1]);
			Assert.AreEqual(3f, oneTwoThreeFour[2]);
			Assert.AreEqual(4f, oneTwoThreeFour[3]);

			Assert.AreEqual(new Vector4(1f, 2f, 0f, 0f), oneTwoThreeFour[0, 1]);
			Assert.AreEqual(new Vector4(1f, 2f, 3f, 0f), oneTwoThreeFour[0, 1, 2]);
			Assert.AreEqual(new Vector4(1f, 2f, 3f, 4f), oneTwoThreeFour[0, 1, 2, 3]);
		}

		[TestMethod]
		public void TestIsUnit() {
			// Define variables and constants
			Vector4[] units = {
				Vector4.ONE.ToUnit(),
				Vector4.UP,
				Vector4.DOWN,
				Vector4.LEFT,
				Vector4.RIGHT,
				new Vector4(1351f, 1351f, 666f, -135f).ToUnit(), 
			};
			Vector4[] notUnits = {
				Vector4.ONE,
				Vector4.UP * -3f,
				Vector4.DOWN * 0.5f,
				Vector4.LEFT * -0.5f,
				Vector4.RIGHT * 0.9f,
				new Vector4(1351f, 1351f, 666f, -135f), 
			};

			// Set up context


			// Execute


			// Assert outcome
			foreach (Vector4 vector in units) {
				Assert.IsTrue(vector.IsUnit);
			}
			foreach (Vector4 vector in notUnits) {
				Assert.IsFalse(vector.IsUnit);
			}
		}

		[TestMethod]
		public void TestLength() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(2f, new Vector4(2f, 0f, 0f, 0f).Length);
			Assert.AreEqual(2f, new Vector4(-2f, 0f, 0f, 0f).Length);
			Assert.AreEqual(Math.Sqrt(10f * 10f + -15f * -15f + -25f * -25f + 50f * 50f), new Vector4(10f, -15f, -25f, 50f).Length, 0.00001d);
		}

		[TestMethod]
		public void TestLengthSquared() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(4f, new Vector4(2f, 0f, 0f, 0f).LengthSquared);
			Assert.AreEqual(4f, new Vector4(-2f, 0f, 0f, 0f).LengthSquared);
			Assert.AreEqual(10f * 10f + -15f * -15f + -25f * -25f + 50f * 50f, new Vector4(10f, -15f, -25f, 50f).LengthSquared);
		}

		[TestMethod]
		public void TestAngleBetween() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(MathUtils.PI, Vector4.AngleBetween(Vector4.FORWARD, Vector4.BACKWARD));
			Assert.AreEqual(MathUtils.PI_OVER_TWO, Vector4.AngleBetween(Vector4.FORWARD, Vector4.RIGHT));
			Assert.AreEqual(MathUtils.PI_OVER_TWO, Vector4.AngleBetween(Vector4.FORWARD, Vector4.LEFT));
			Assert.AreEqual(0f, Vector4.AngleBetween(Vector4.UP, Vector4.UP));
			Assert.AreEqual(MathUtils.DegToRad(74.26f), Vector4.AngleBetween(new Vector4(2f, -3f, 4f, 0f), new Vector4(5f, 2f, 1f, 0f)), 0.001d);
		}

		[TestMethod]
		public void TestCrossProduct() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(new Vector4(-2f, 4f, -2f, 0f), new Vector4(1f, 2f, 3f, 0f) * new Vector4(3f, 4f, 5f, 0f));
			Assert.AreEqual(new Vector4(3f, 0f, -1f, 0f), new Vector4(-1f, 25f, -3f, 0f) * new Vector4(0f, 1f, 0f, 0f));
			Assert.AreEqual(new Vector4(3f, 0f, -1f, 0f), new Vector4(-1f, 25f, -3f, 0f) * new Vector4(0f, 1f, 0f, 0f));
		}

		[TestMethod]
		public void TestToUnit() {
			// Define variables and constants
			Vector4[] notUnits = {
				Vector4.ONE * 3f,
				Vector4.UP * -3f,
				Vector4.DOWN * 0.5f,
				Vector4.LEFT * -0.5f,
				Vector4.RIGHT * 0.9999f,
				new Vector4(1351f, 1351f, 666f, -135f), 
			};
			Vector4[] units = {
				Vector4.ONE.ToUnit(),
				Vector4.DOWN,
				Vector4.DOWN,
				Vector4.RIGHT,
				Vector4.RIGHT,
				new Vector4(0.666f, 0.666f, 0.328f, -0.067f), 
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
			Assert.AreEqual(new Vector4(0f, 0f, 3f, 0f), Vector4.FORWARD.WithLength(3f));
			Assert.AreEqual(new Vector4(0f, 0f, -3f, 0f), Vector4.FORWARD.WithLength(-3f));
			Assert.AreEqual(new Vector4(1.332f, 1.332f, 0.656f, -0.134f), new Vector4(1351f, 1351f, 666f, -135f).WithLength(2f));
			Assert.AreEqual(Vector4.ZERO, new Vector4(31f, 3561f, -13f, 15.135f).WithLength(0f));
		}

		[TestMethod]
		public void TestDistance() {
			// Define variables and constants
			Vector4 a = new Vector4(5f, 5f, 0f, 0f);
			Vector4 b = new Vector4(7f, 7f, 0f, 0f);
			Vector4 c = new Vector4(5f, 5f, 5f, 0f);

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual((float) Math.Sqrt(8), Vector4.Distance(a, b));
			Assert.AreEqual(5f, Vector4.Distance(a, c));
			Assert.AreEqual((float) Math.Sqrt(33), Vector4.Distance(b, c));
		}

		[TestMethod]
		public void TestLerp() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(new Vector4(7.5f, 7.5f, 0f, 0f), Vector4.Lerp(new Vector4(5f, 5f, 0f, 0f), new Vector4(10f, 10f, 0f, 0f), 0.5f));
			Assert.AreEqual(new Vector4(9f, 9f, 0f, 0f), Vector4.Lerp(new Vector4(5f, 5f, 0f, 0f), new Vector4(10f, 10f, 0f, 0f), 0.8f));
			Assert.AreEqual(new Vector4(5f, 5f, 0f, 0f), Vector4.Lerp(new Vector4(5f, 5f, 0f, 0f), new Vector4(10f, 10f, 0f, 0f), 0f));
		}

		[TestMethod]
		public void TestProjectedOnTo() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(new Vector4(0f, 5f, 0f, 0f), new Vector4(5f, 5f, 5f, 0f).ProjectedOnto(Vector4.UP));
			Assert.AreEqual(new Vector4(0f, -5f, 0f, 0f), new Vector4(5f, -5f, 5f, 0f).ProjectedOnto(Vector4.UP));
			Assert.AreEqual(new Vector4(10f, 10f, 10f, 0f), new Vector4(10f, 10f, 10f, 0f).ProjectedOnto(Vector4.ONE[0, 1, 2]));
		}

		[TestMethod]
		public void TestVectorCasts() {
			// Define variables and constants
			Vector2 twoVec = new Vector2(20f, 21f);
			Vector3 threeVec = new Vector3(30f, 31f, 32f);
			Vector4 fourVec = new Vector4(40f, 41f, 42f, 43f);

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(new Vector3(40f, 41f, 42f), (Vector3) fourVec);
			Assert.AreEqual(new Vector2(40f, 41f), (Vector2) fourVec);
			Assert.AreEqual(new Vector2(30f, 31f), (Vector2) threeVec);

			Assert.AreEqual(new Vector3(20f, 21f, 0f), (Vector3) twoVec);
			Assert.AreEqual(new Vector4(20f, 21f, 0f, 0f), (Vector4) twoVec);
			Assert.AreEqual(new Vector4(30f, 31f, 32f, 0f), (Vector4) threeVec);
		}
		#endregion
	}
}