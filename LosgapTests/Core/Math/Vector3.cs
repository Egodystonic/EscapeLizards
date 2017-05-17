// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 20 11 2014 at 14:52 by Ben Bowen

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap {
	[TestClass]
	public class Vector3Test {

		[TestInitialize]
		public void SetUp() { }


		#region Tests
		[TestMethod]
		public void ShouldCorrectlySupportArithmeticOperators() {
			// Define variables and constants
			Vector3 vectorA = new Vector3(2f, 3f, 4f);
			Vector3 vectorB = new Vector3(1.5f, -3f, 4.5f);
			float scalarK = 2f;

			// Assert outcome
			Assert.AreEqual(vectorA + vectorB, new Vector3(3.5f, 0f, 8.5f));
			Assert.AreEqual(vectorA - vectorB, new Vector3(0.5f, 6f, -0.5f));
			Assert.AreEqual(vectorA * scalarK, new Vector3(4f, 6f, 8f));
			Assert.AreEqual(vectorB / scalarK, new Vector3(0.75f, -1.5f, 2.25f));
			Assert.AreEqual(scalarK * vectorB, new Vector3(3f, -6f, 9f));

			Assert.AreEqual(new Vector3(25.500f, -3.000f, -10.500f), vectorA * vectorB);
			Assert.AreEqual(-vectorA, new Vector3(-2f, -3f, -4f));

			Assert.AreEqual(vectorA > vectorB, vectorA.Length > vectorB.Length);
			Assert.AreEqual(vectorB < vectorA, vectorB.Length < vectorA.Length);
			Assert.AreEqual(vectorA >= vectorB, vectorA.Length >= vectorB.Length);
			Assert.AreEqual(vectorB <= vectorA, vectorB.Length <= vectorA.Length);
		}

		[TestMethod]
		public void ShouldCorrectlySupportVectorCalculations() {
			// Define variables and constants
			Vector3 vector = new Vector3(10f, 100f, 500f);
			Vector3 forwardVector = new Vector3(10f, 0f, 0f);
			Vector3 rightVector = new Vector3(0f, 10f, 0f);

			// Assert outcome
			Assert.AreEqual(510f, vector.Length);
			Assert.IsTrue(Math.Abs(Math.Pow(510, 2d) - vector.LengthSquared) < 0.01d);
			Assert.AreEqual(new Vector3(0.0195928f, 0.195928f, 0.979639f), vector.ToUnit());
			Assert.IsTrue(vector.ToUnit().IsUnit);

			Assert.AreEqual(MathUtils.PI_OVER_TWO, Vector3.AngleBetween(forwardVector, rightVector));
			Assert.AreEqual(Vector3.AngleBetween(forwardVector, rightVector), Vector3.AngleBetween(rightVector, forwardVector));

			Assert.AreEqual(new Vector3(0f, 0f, 1f), (forwardVector * rightVector).ToUnit());

			Assert.AreEqual(new Vector3(0f, 0f, 100f), forwardVector * rightVector);
		}

		[TestMethod]
		public void ShouldCorrectlySwizzleVectors() {
			// Define variables and constants
			Vector3 vector = new Vector3(0f, 1f, 2f);

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(new Vector3(2f, 1f, 0f), vector[2, 1, 0]);
			Assert.AreEqual(new Vector4(2f, 1f, 0f, 1f), vector[2, 1, 0, 1]);
		}

		[TestMethod]
		public void ShouldEquateVectorsWithFPTolerance() {
			// Define variables and constants
			Vector3 v1 = new Vector3(16f, 16f, 16f);
			Vector3 v2 = new Vector3(16.000001f, 15.99996523f, 15.99990f);

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(v1, v2);
			Assert.IsFalse(v1.EqualsExactly(v2));
		}

		[TestMethod]
		public void TestIndexersAndSwizzles() {
			// Define variables and constants
			Vector3 oneTwoThreeFour = new Vector3(1f, 2f, 3f);

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(1f, oneTwoThreeFour[0]);
			Assert.AreEqual(2f, oneTwoThreeFour[1]);
			Assert.AreEqual(3f, oneTwoThreeFour[2]);

			Assert.AreEqual(new Vector3(1f, 2f, 0f), oneTwoThreeFour[0, 1]);
			Assert.AreEqual(new Vector3(1f, 2f, 3f), oneTwoThreeFour[0, 1, 2]);
		}

		[TestMethod]
		public void TestIsUnit() {
			// Define variables and constants
			Vector3[] units = {
				Vector3.ONE.ToUnit(),
				Vector3.UP,
				Vector3.DOWN,
				Vector3.LEFT,
				Vector3.RIGHT,
				new Vector3(1351f, 1351f, 666f).ToUnit(), 
			};
			Vector3[] notUnits = {
				Vector3.ONE,
				Vector3.UP * -3f,
				Vector3.DOWN * 0.5f,
				Vector3.LEFT * -0.5f,
				Vector3.RIGHT * 0.9f,
				new Vector3(1351f, 1351f, 666f), 
			};

			// Set up context


			// Execute


			// Assert outcome
			foreach (Vector3 vector in units) {
				Assert.IsTrue(vector.IsUnit);
			}
			foreach (Vector3 vector in notUnits) {
				Assert.IsFalse(vector.IsUnit);
			}
		}

		[TestMethod]
		public void TestLength() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(2f, new Vector3(2f, 0f, 0f).Length);
			Assert.AreEqual(2f, new Vector3(-2f, 0f, 0f).Length);
			Assert.AreEqual(Math.Sqrt(10f * 10f + -15f * -15f + -25f * -25f), new Vector3(10f, -15f, -25f).Length, 0.00001d);
		}

		[TestMethod]
		public void TestLengthSquared() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(4f, new Vector3(2f, 0f, 0f).LengthSquared);
			Assert.AreEqual(4f, new Vector3(-2f, 0f, 0f).LengthSquared);
			Assert.AreEqual(10f * 10f + -15f * -15f + -25f * -25f, new Vector3(10f, -15f, -25f).LengthSquared);
		}

		[TestMethod]
		public void TestAngleBetween() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(MathUtils.PI, Vector3.AngleBetween(Vector3.FORWARD, Vector3.BACKWARD));
			Assert.AreEqual(MathUtils.PI_OVER_TWO, Vector3.AngleBetween(Vector3.FORWARD, Vector3.RIGHT));
			Assert.AreEqual(MathUtils.PI_OVER_TWO, Vector3.AngleBetween(Vector3.FORWARD, Vector3.LEFT));
			Assert.AreEqual(0f, Vector3.AngleBetween(Vector3.UP, Vector3.UP));
			Assert.AreEqual(MathUtils.DegToRad(74.26f), Vector3.AngleBetween(new Vector3(2f, -3f, 4f), new Vector3(5f, 2f, 1f)), 0.001d);
		}

		[TestMethod]
		public void TestCrossProduct() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(new Vector3(-2f, 4f, -2f), new Vector3(1f, 2f, 3f) * new Vector3(3f, 4f, 5f));
			Assert.AreEqual(new Vector3(3f, 0f, -1f), new Vector3(-1f, 25f, -3f) * new Vector3(0f, 1f, 0f));
			Assert.AreEqual(new Vector3(3f, 0f, -1f), new Vector3(-1f, 25f, -3f) * new Vector3(0f, 1f, 0f));
		}

		[TestMethod]
		public void TestToUnit() {
			// Define variables and constants
			Vector3[] notUnits = {
				Vector3.ONE * 3f,
				Vector3.UP * -3f,
				Vector3.DOWN * 0.5f,
				Vector3.LEFT * -0.5f,
				Vector3.RIGHT * 0.9999f,
				new Vector3(1351f, 1351f, 666f), 
			};
			Vector3[] units = {
				Vector3.ONE.ToUnit(),
				Vector3.DOWN,
				Vector3.DOWN,
				Vector3.RIGHT,
				Vector3.RIGHT,
				new Vector3(0.666f, 0.666f, 0.328f), 
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
			Assert.AreEqual(new Vector3(0f, 0f, 3f), Vector3.FORWARD.WithLength(3f));
			Assert.AreEqual(new Vector3(0f, 0f, -3f), Vector3.FORWARD.WithLength(-3f));
			Assert.AreEqual(new Vector3(1.332f, 1.332f, 0.656f), new Vector3(1351f, 1351f, 666f).WithLength(2f));
			Assert.AreEqual(Vector3.ZERO, new Vector3(31f, 3561f, -13f).WithLength(0f));
		}

		[TestMethod]
		public void TestDistance() {
			// Define variables and constants
			Vector3 a = new Vector3(5f, 5f, 0f);
			Vector3 b = new Vector3(7f, 7f, 0f);
			Vector3 c = new Vector3(5f, 5f, 5f);

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual((float) Math.Sqrt(8), Vector3.Distance(a, b));
			Assert.AreEqual(5f, Vector3.Distance(a, c));
			Assert.AreEqual((float) Math.Sqrt(33), Vector3.Distance(b, c));
		}

		[TestMethod]
		public void TestLerp() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(new Vector3(7.5f, 7.5f, 0f), Vector3.Lerp(new Vector3(5f, 5f, 0f), new Vector3(10f, 10f, 0f), 0.5f));
			Assert.AreEqual(new Vector3(9f, 9f, 0f), Vector3.Lerp(new Vector3(5f, 5f, 0f), new Vector3(10f, 10f, 0f), 0.8f));
			Assert.AreEqual(new Vector3(5f, 5f, 0f), Vector3.Lerp(new Vector3(5f, 5f, 0f), new Vector3(10f, 10f, 0f), 0f));
		}

		[TestMethod]
		public void TestProjectedOnTo() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(new Vector3(0f, 5f, 0f), new Vector3(5f, 5f, 5f).ProjectedOnto(Vector3.UP));
			Assert.AreEqual(new Vector3(0f, -5f, 0f), new Vector3(5f, -5f, 5f).ProjectedOnto(Vector3.UP));
			Assert.AreEqual(new Vector3(10f, 10f, 10f), new Vector3(10f, 10f, 10f).ProjectedOnto(Vector3.ONE[0, 1, 2]));
		}

		[TestMethod]
		public void TestOrthonormalize() {
			// Define variables and constants
			Vector3 a1 = new Vector3(2f, 0f, 0f);
			Vector3 b1 = new Vector3(0f, 1f, 0f);
			Vector3 c1 = new Vector3(0f, 0f, -3f);

			Vector3 a2 = new Vector3(1,1,1);
			Vector3 b2 = new Vector3(2,-5,3);
			Vector3 c2 = new Vector3(8,4,0);

			// Set up context


			// Execute
			Vector3.Orthonormalize(ref a1, ref b1, ref c1);
			Vector3.Orthonormalize(ref a2, ref b2, ref c2);

			// Assert outcome
			Assert.AreEqual(Vector3.RIGHT, a1);
			Assert.AreEqual(Vector3.UP, b1);
			Assert.AreEqual(Vector3.BACKWARD, c1);

			Assert.AreEqual(new Vector3(1, 1, 1) / (float) Math.Sqrt(3f), a2);
			Assert.AreEqual(new Vector3((float) Math.Sqrt(2f / 19f), -5f / (float) Math.Sqrt(38f), 3f / (float) Math.Sqrt(38f)), b2);
			Assert.AreEqual(new Vector3((float) Math.Sqrt(2f / 57f) * 4f, -1f / (float) Math.Sqrt(114f), -7f / (float) Math.Sqrt(114f)), c2);
		}

		[TestMethod]
		public void TestOrthonormalizedAgainst() {
			// Define variables and constants
			Vector3 a1 = new Vector3(2f, 0f, 0f);
			Vector3 b1 = new Vector3(0f, 1f, 0f);

			Vector3 a2 = new Vector3(1, 1, 1);
			Vector3 b2 = new Vector3(2, -5, 3);

			// Set up context


			// Execute

			// Assert outcome
			Assert.AreEqual(Vector3.RIGHT, a1.OrthonormalizedAgainst(b1));
			Assert.AreEqual(Vector3.UP, b1.OrthonormalizedAgainst(a1));

			Assert.AreEqual(
				a2.ToUnit(), 
				a2.OrthonormalizedAgainst(b2)
			);
			Assert.AreEqual(
				new Vector3((float) Math.Sqrt(2f / 19f), -5f / (float) Math.Sqrt(38f), 3f / (float) Math.Sqrt(38f)), 
				b2.OrthonormalizedAgainst(a2)
			);

			((TestLoggingProvider) Logger.LoggingProvider).SuppressMessages();
			Assert.AreEqual(MathUtils.PI_OVER_TWO, Vector3.AngleBetween(Vector3.UP, Vector3.UP.OrthonormalizedAgainst(Vector3.UP)));
			((TestLoggingProvider) Logger.LoggingProvider).AllowMessages();
		}

		[TestMethod]
		public void TestGetOrthogonals() {
			Vector3 outA, outB;

			Vector3.UP.GetOrthogonals(out outA, out outB);
			Assert.AreEqual(Vector3.LEFT, outA);
			Assert.AreEqual(Vector3.FORWARD, outB);

			Vector3.DOWN.GetOrthogonals(out outA, out outB);
			Assert.AreEqual(Vector3.LEFT, outA);
			Assert.AreEqual(Vector3.BACKWARD, outB);

			Vector3.LEFT.GetOrthogonals(out outA, out outB);
			Assert.AreEqual(Vector3.DOWN, outA);
			Assert.AreEqual(Vector3.FORWARD, outB);

			Vector3.ONE.GetOrthogonals(out outA, out outB);
			Assert.AreEqual(new Vector3(0f, -1.225f, 1.225f), outA);
			Assert.AreEqual(new Vector3(1.414f, -0.707f, -0.707f), outB);

			Vector3.ONE.WithLength(30f).GetOrthogonals(out outA, out outB);
			Assert.AreEqual(new Vector3(0f, -1.225f, 1.225f).WithLength(30f), outA);
			Assert.AreEqual(new Vector3(1.414f, -0.707f, -0.707f).WithLength(30f), outB);

			new Vector3(3151f, -146146f, 9555f).ToUnit().GetOrthogonals(out outA, out outB);
			Assert.IsTrue(outA.IsUnit);
			Assert.IsTrue(outB.IsUnit);

#if !DEVELOPMENT && !RELEASE
			try {
				Vector3.ZERO.GetOrthogonals(out outA, out outB);
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
#endif
		}
		#endregion
	}
}