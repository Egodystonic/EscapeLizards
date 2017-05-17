// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 26 11 2014 at 10:50 by Ben Bowen

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap {
	[TestClass]
	public class CircleTest {

		[TestInitialize]
		public void SetUp() { }

		static readonly Circle testCircleA = new Circle(5f, 5f, 10f);
		static readonly Circle testCircleB = new Circle(-5f, -5f, -10f);
		static readonly Circle testCircleC = new Circle(0f, 0f, -10f);
		static readonly Circle testCircleD = new Circle(20f, 20f, 0f);
		static readonly Circle testCircleE = new Circle(19f, 19f, 1f);

		static readonly Circle[] testCircles = {
			testCircleA, testCircleB, testCircleC, testCircleD, testCircleE
		};

		#region Tests
		[TestMethod]
		public void TestArea() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(314.1593f,	testCircleA.Area, 0.001f);
			Assert.AreEqual(314.1593f,	testCircleB.Area, 0.001f);
			Assert.AreEqual(314.1593f,	testCircleC.Area, 0.001f);
			Assert.AreEqual(0f,			testCircleD.Area, 0.001f);
			Assert.AreEqual(3.1415f,	testCircleE.Area, 0.001f);
		}

		[TestMethod]
		public void TestCircumference() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(62.83186f,	testCircleA.Circumference, 0.001f);
			Assert.AreEqual(62.83186f,	testCircleB.Circumference, 0.001f);
			Assert.AreEqual(62.83186f,	testCircleC.Circumference, 0.001f);
			Assert.AreEqual(0f,			testCircleD.Circumference, 0.001f);
			Assert.AreEqual(6.2830f,	testCircleE.Circumference, 0.001f);
		}

		[TestMethod]
		public void TestIntersects() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsTrue(testCircleA.Intersects(testCircleA));
			Assert.IsTrue(testCircleA.Intersects(testCircleB));
			Assert.IsTrue(testCircleA.Intersects(testCircleC));
			Assert.IsFalse(testCircleA.Intersects(testCircleD));
			Assert.IsFalse(testCircleA.Intersects(testCircleE));

			Assert.IsTrue(testCircleB.Intersects(testCircleB));
			Assert.IsTrue(testCircleB.Intersects(testCircleC));
			Assert.IsFalse(testCircleB.Intersects(testCircleD));
			Assert.IsFalse(testCircleB.Intersects(testCircleE));

			Assert.IsTrue(testCircleC.Intersects(testCircleC));
			Assert.IsFalse(testCircleC.Intersects(testCircleD));
			Assert.IsFalse(testCircleC.Intersects(testCircleE));

			Assert.IsTrue(testCircleD.Intersects(testCircleD));
			Assert.IsFalse(testCircleD.Intersects(testCircleE));

			Assert.IsTrue(testCircleE.Intersects(testCircleE));

			foreach (Circle testCircle1 in testCircles) {
				foreach (Circle testCircle2 in testCircles) {
					Assert.AreEqual(testCircle1.Intersects(testCircle2), testCircle2.Intersects(testCircle1));
				}	
			}
		}

		[TestMethod]
		public void TestContainsPoint() {
			// Define variables and constants
			Vector2 pointA = new Vector2(10f, 10f);
			Vector2 pointB = new Vector2(-2f, -2f);

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsTrue(testCircleA.Contains(pointA));
			Assert.IsFalse(testCircleB.Contains(pointA));
			Assert.IsFalse(testCircleC.Contains(pointA));
			Assert.IsFalse(testCircleD.Contains(pointA));
			Assert.IsFalse(testCircleE.Contains(pointA));

			Assert.IsTrue(testCircleA.Contains(pointB));
			Assert.IsTrue(testCircleB.Contains(pointB));
			Assert.IsTrue(testCircleC.Contains(pointB));
			Assert.IsFalse(testCircleD.Contains(pointB));
			Assert.IsFalse(testCircleE.Contains(pointB));
		}

		[TestMethod]
		public void TestEquals() {
			// Define variables and constants


			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(testCircleA, testCircleA);
			Assert.IsTrue(testCircleD.EqualsWithTolerance(testCircleE, 2f));
			Assert.IsFalse(testCircleB.EqualsWithTolerance(testCircleC, 2f));
			Assert.AreNotEqual(testCircleE, testCircleA);
		}

		[TestMethod]
		public void TestContainsCircle() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsTrue(testCircleA.Contains(new Circle(5f, 5f, 5f)));
			Assert.IsTrue(testCircleB.Contains(new Circle(0f, -5f, 5f)));
			Assert.IsTrue(testCircleC.Contains(new Circle(-3f, 6f, 1.5f)));
			Assert.IsFalse(testCircleD.Contains(new Circle(20f, 20f, 1f)));
			Assert.IsFalse(testCircleE.Contains(new Circle(19f, 20f, 1f)));
		}

		[TestMethod]
		public void TestContainsRectangle() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsTrue(testCircleA.Contains(new Rectangle(-2f, -2f, 12f, 12f)));
			Assert.IsTrue(testCircleB.Contains(new Rectangle(-5f, 0f, 4.8f, -4.8f)));
			Assert.IsTrue(testCircleC.Contains(new Rectangle(-5f, -5f, 10f, 10f)));
			Assert.IsFalse(testCircleD.Contains(new Rectangle(20f, 20f, 1f, 1f)));
			Assert.IsFalse(testCircleE.Contains(new Rectangle(20.5f, 20.5f, 0f, 0f)));
		}

		[TestMethod]
		public void TestDistanceFromCircle() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(0f, testCircleA.DistanceFrom(new Circle(5f, 25f, 10f)));
			Assert.AreEqual(10f, testCircleB.DistanceFrom(new Circle(-5f, 25f, 10f)));
			Assert.AreEqual(0f, testCircleC.DistanceFrom(new Circle(5f, 5f, 5f)));
			Assert.AreEqual((float) Math.Sqrt(900f + 400f) - 10f, testCircleD.DistanceFrom(new Circle(-10f, 0f, 10f)));
			Assert.AreEqual(98f, testCircleE.DistanceFrom(new Circle(119f, 19f, 1f)));

			foreach (Circle testCircle1 in testCircles) {
				foreach (Circle testCircle2 in testCircles) {
					Assert.AreEqual(testCircle1.DistanceFrom(testCircle2), testCircle2.DistanceFrom(testCircle1));
				}
			}
		}

		[TestMethod]
		public void TestDistanceFromRectangle() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(0f, testCircleA.DistanceFrom(new Rectangle(10f, 10f, 100f, 100f)));
			Assert.AreEqual(1f, testCircleB.DistanceFrom(new Rectangle(6f, -5f, 10f, 10f)));
			Assert.AreEqual(0f, testCircleC.DistanceFrom(new Rectangle(10f, 0f, 5f, 5f)));
			Assert.AreEqual((float) Math.Sqrt(100f + 100f), testCircleD.DistanceFrom(new Rectangle(30f, 30f, 1f, 1f)));
			Assert.AreEqual((float) Math.Sqrt(121f + 121f), testCircleE.DistanceFrom(new Rectangle(31f, 31f, 1f, 1f)));
		}

		[TestMethod]
		public void TestDistanceFromPoint() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(0f, testCircleA.DistanceFrom(new Vector2(15f, 5f)));
			Assert.AreEqual(10f, testCircleB.DistanceFrom(new Vector2(15f, -5f)));
			Assert.AreEqual(0f, testCircleC.DistanceFrom(new Vector2(5f, 5f)));
			Assert.AreEqual((float) Math.Sqrt(200f), testCircleD.DistanceFrom(new Vector2(10f, 30f)));
			Assert.AreEqual((float) Math.Sqrt(200f) - 1f, testCircleE.DistanceFrom(new Vector2(9f, 29f)));
		}
		#endregion
	}
}