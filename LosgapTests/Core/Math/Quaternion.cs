// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 24 11 2014 at 11:39 by Ben Bowen

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap {
	[TestClass]
	public class QuaternionTest {

		[TestInitialize]
		public void SetUp() { }


		#region Tests
		[TestMethod]
		public void ShouldCorrectlySupportArithmeticOperators() {
			// Define variables and constants
			Quaternion quatA = new Quaternion(2f, 3f, 4f, 5f);
			Quaternion quatB = new Quaternion(1.5f, -3f, 4.5f, -10f);
			float scalarK = 2f;

			// Assert outcome
			Assert.AreEqual(quatA + quatB, new Quaternion(3.5f, 0f, 8.5f, -5f));
			Assert.AreEqual(quatA - quatB, new Quaternion(0.5f, 6f, -0.5f, 15f));
			Assert.AreEqual(quatA * scalarK, new Quaternion(4f, 6f, 8f, 10f));
			Assert.AreEqual(quatB / scalarK, new Quaternion(0.75f, -1.5f, 2.25f, -5f));
			Assert.AreEqual(scalarK * quatB, new Quaternion(3f, -6f, 9f, -20f));

			Assert.AreEqual(new Quaternion(-38f, -42f, -7f, -62f), quatA * quatB);
		}

		[TestMethod]
		public void ShouldCorrectlySupportQuaternionCalculations() {
			// Define variables and constants
			Quaternion quatA = new Quaternion(10f, 100f, 500f, -20f);

			// Assert outcome
			Assert.AreEqual(510.392f, quatA.Norm);
			Assert.IsTrue(Math.Abs(Math.Pow(510.392d, 2d) - quatA.NormSquared) < 0.01d);
			Assert.AreEqual(new Quaternion(0.0195928f, 0.195928f, 0.979639f, -0.0391856f), quatA.ToUnit());
			Assert.IsTrue(quatA.ToUnit().IsUnit);
		}

		[TestMethod]
		public void ShouldEquateQuaternionsWithFPTolerance() {
			// Define variables and constants
			Quaternion v1 = new Quaternion(16f, 16f, 16f, 16f);
			Quaternion v2 = new Quaternion(16.000001f, 15.99996523f, 15.99990f, 16.00099999f);

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(v1, v2);
			Assert.IsFalse(v1.EqualsExactly(v2));
		}

		[TestMethod]
		public void TestConjugate() {
			// Define variables and constants
			Quaternion quatA = new Quaternion(2f, -1f, -3f, 0f);
			Quaternion quatB = new Quaternion(22f, -31f, -43f, 10f);

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(new Quaternion(-2f, 1f, 3f, 0f), quatA.Conjugate);
			Assert.AreEqual(new Quaternion(-22f, 31f, 43f, 10f), quatB.Conjugate);
		}

		[TestMethod]
		public void TestNorm() {
			// Define variables and constants
			Quaternion quatA = new Quaternion(2f, -1f, -3f, 0f);
			Quaternion quatB = new Quaternion(22f, -31f, -43f, 10f);

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual((float) Math.Sqrt(14), quatA.Norm);
			Assert.AreEqual(14f, quatA.NormSquared);
			Assert.AreEqual((float) Math.Sqrt(3394), quatB.Norm);
			Assert.AreEqual(3394f, quatB.NormSquared);
		}

		[TestMethod]
		public void TestInverse() {
			// Define variables and constants
			Quaternion quatA = new Quaternion(2f, -1f, -3f, 0f);
			Quaternion quatB = new Quaternion(22f, -31f, -43f, 10f);

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsFalse(Quaternion.ZERO.HasInverse);
			Assert.AreEqual(new Quaternion(-1f / 7f, 1f / 14f, 3f / 14f, 0f), quatA.Inverse);
			Assert.AreEqual(new Quaternion(-11f / 1697f, 31f / 3394f, 43f / 3394f, 5f / 1697f), -quatB);
		}

		[TestMethod]
		public void TestToUnit() {
			// Define variables and constants
			Quaternion[] notUnits = {
				new Quaternion(Vector3.ONE * 3f),
				new Quaternion(Vector3.UP * -3f),
				new Quaternion(Vector3.DOWN * 0.5f),
				new Quaternion(Vector3.LEFT * -0.5f),
				new Quaternion(Vector3.RIGHT * 0.95f),
				new Quaternion(1351f, 1351f, 666f, -135f)
			};
			Quaternion[] units = {
				new Quaternion(Vector3.ONE.ToUnit()),
				new Quaternion(Vector3.DOWN),
				new Quaternion(Vector3.DOWN),
				new Quaternion(Vector3.RIGHT),
				new Quaternion(Vector3.RIGHT),
				new Quaternion(0.666f, 0.666f, 0.328f, -0.067f), 
			};


			// Set up context


			// Execute


			// Assert outcome
			for (int i = 0; i < notUnits.Length; i++) {
				Assert.IsFalse(notUnits[i].IsUnit);
				Assert.IsTrue(units[i].IsUnit);
				Assert.AreEqual(units[i], notUnits[i].ToUnit());
			}
		}

		[TestMethod]
		public void TestRotationCombiners() {
			// Define variables and constants
			Vector3 startDirection = Vector3.FORWARD;

			Quaternion quatA = Quaternion.FromAxialRotation(Vector3.DOWN, MathUtils.PI_OVER_TWO);
			Quaternion quatB = Quaternion.FromAxialRotation(Vector3.FORWARD, MathUtils.PI_OVER_TWO);
			Quaternion quatC = Quaternion.FromAxialRotation(Vector3.RIGHT, -MathUtils.PI_OVER_TWO);
			Quaternion quatD = quatA * quatB;

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(Vector3.RIGHT, startDirection * quatA);
			Assert.AreEqual(Vector3.DOWN, startDirection * quatA * quatB);
			Assert.AreEqual(Vector3.DOWN, startDirection * quatD);
			Assert.AreEqual(Vector3.BACKWARD, startDirection * quatA * quatB * quatC);
			Assert.AreEqual(Vector3.BACKWARD, startDirection * quatD * quatC);

			Assert.AreEqual(quatA * quatB, Quaternion.CombineRotations(quatA, quatB));
			Assert.AreEqual(quatA * quatB * quatC, Quaternion.CombineRotations(quatA, quatB, quatC));
			Assert.AreEqual(quatA * quatB * quatC, Quaternion.CombineRotations(quatA, quatB, quatC));
			Assert.AreEqual(quatA * quatB * quatC * quatD, Quaternion.CombineRotations(quatA, quatB, quatC, quatD));
		}

		[TestMethod]
		public void TestFromVectorTransitionOrAxialRotation() {
			// Define variables and constants


			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(
				Quaternion.FromAxialRotation(
					Vector3.UP,
					Vector3.AngleBetween(Vector3.FORWARD, Vector3.BACKWARD)
				),
				Quaternion.FromVectorTransition(Vector3.FORWARD, Vector3.BACKWARD)
			);
			Assert.AreEqual(
				Quaternion.FromAxialRotation(
					Vector3.DOWN,
					MathUtils.PI_OVER_TWO
				),
				Quaternion.FromVectorTransition(Vector3.FORWARD, Vector3.RIGHT)
			);
			Assert.AreEqual(
				Quaternion.FromAxialRotation(
					Vector3.UP,
					MathUtils.PI_OVER_TWO
				),
				Quaternion.FromVectorTransition(Vector3.FORWARD, Vector3.LEFT)
			);
			Assert.AreEqual(
				Quaternion.FromAxialRotation(
					Vector3.LEFT,
					Vector3.AngleBetween(Vector3.UP, Vector3.UP)
				),
				Quaternion.FromVectorTransition(Vector3.UP, Vector3.UP)
			);
			Assert.AreEqual(
				Quaternion.FromAxialRotation(
					-(new Vector3(2f, -3f, 4f) * new Vector3(5f, 2f, 1f)),
					Vector3.AngleBetween(new Vector3(2f, -3f, 4f), new Vector3(5f, 2f, 1f))
				),
				Quaternion.FromVectorTransition(new Vector3(2f, -3f, 4f), new Vector3(5f, 2f, 1f))
			);
			Assert.AreEqual(
				Quaternion.FromAxialRotation(
					new Vector3(2f, -3f, 4f) * new Vector3(5f, 2f, 1f),
					Vector3.AngleBetween(new Vector3(2f, -3f, 4f), new Vector3(5f, 2f, 1f))
				),
				-Quaternion.FromVectorTransition(new Vector3(2f, -3f, 4f), new Vector3(5f, 2f, 1f))
			);
		}

		[TestMethod]
		public void TestLerpAndNormalize() {
			// Define variables and constants
			Quaternion targetA = Quaternion.FromAxialRotation(Vector3.DOWN, MathUtils.PI);
			Quaternion targetB = Quaternion.FromAxialRotation(Vector3.FORWARD, -MathUtils.THREE_PI_OVER_TWO);

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(
				(Quaternion.IDENTITY * 0.5f + targetA * 0.5f).ToUnit(),
				Quaternion.LerpAndNormalize(Quaternion.IDENTITY, targetA, 0.5f)
			);
			Assert.AreEqual(
				(targetA * 0.8f + targetB * 0.2f).ToUnit(),
				Quaternion.LerpAndNormalize(targetA, targetB, 0.2f)
			);
		}

		[TestMethod]
		public void TestSlerp() {
			// Define variables and constants
			Quaternion quatA = Quaternion.IDENTITY;
			Quaternion quatB = Quaternion.FromAxialRotation(Vector3.UP, -3.1415f);
			Quaternion quatC = Quaternion.FromAxialRotation(Vector3.FORWARD, -((3.1415f * 3f) / 2f));

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(new Quaternion(0f, 0.58777f, 0f, 0.809028f), Quaternion.Slerp(quatA, quatB, 0.4f));
			Quaternion answerB = Quaternion.Slerp(quatB, quatC, 0.85f);
			Assert.IsTrue(answerB.Equals(new Quaternion(0.000f, 0.233f, 0.688f, -0.688f)) || answerB.Equals(new Quaternion(0.000f, 0.233f, -0.688f, 0.688f)));
			Quaternion answerC = Quaternion.Slerp(quatC, quatA, 1f);
			Assert.IsTrue(answerC.Equals(Quaternion.IDENTITY) || answerC.Equals(Quaternion.IDENTITY * -1f));
		}

		[TestMethod]
		public void TestRotateVector() {
			// Define variables and constants
			Vector3 a = Vector3.RIGHT;
			Vector3 b = Vector3.UP;
			Vector3 c = Vector3.DOWN;
			Vector3 d = Vector3.FORWARD * 10f;
			Vector3 e = new Vector3(5, 10, 5);
			Vector3 f = Vector3.ONE;

			Quaternion q1 = Quaternion.FromAxialRotation(Vector3.UP, MathUtils.PI);
			Quaternion q2 = Quaternion.FromAxialRotation(Vector3.DOWN, -MathUtils.PI_OVER_TWO);

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(Vector3.LEFT, q1 * a);
			Assert.AreEqual(Vector3.UP, q1 * b);
			Assert.AreEqual(Vector3.DOWN, q1 * c);
			Assert.AreEqual(Vector3.BACKWARD * 10f, q1 * d);
			Assert.AreEqual(new Vector3(-5, 10, -5), q1 * e);
			Assert.AreEqual(new Vector3(-1, 1, -1), q1 * f);

			Assert.AreEqual(Vector3.FORWARD, Quaternion.RotateVector(a, q2));
			Assert.AreEqual(Vector3.UP, Quaternion.RotateVector(b, q2));
			Assert.AreEqual(Vector3.DOWN, Quaternion.RotateVector(c, q2));
			Assert.AreEqual(Vector3.LEFT * 10f, d.RotateBy(q2));
			Assert.AreEqual(new Vector3(-5, 10, 5), e.RotateBy(q2));
			Assert.AreEqual(new Vector3(-1, 1, 1), f.RotateBy(q2));
		}

		[TestMethod]
		public void TestFromEuler() {
			// Define variables and constants
			Vector3 v1 = Vector3.FORWARD;
			Vector3 v2 = Vector3.UP;
			Vector3 v3 = Vector3.LEFT;

			// Set up context


			// Execute
			Quaternion fromEuler = Quaternion.FromEulerRotations(MathUtils.PI, MathUtils.PI_OVER_TWO, MathUtils.THREE_PI_OVER_TWO);

			// Assert outcome
			Assert.AreEqual(Vector3.DOWN, v1.RotateBy(fromEuler));
			Assert.AreEqual(Vector3.RIGHT, v2.RotateBy(fromEuler));
			Assert.AreEqual(Vector3.FORWARD, v3.RotateBy(fromEuler));
		}
		#endregion
	}
}