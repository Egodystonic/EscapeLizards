// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 25 11 2014 at 17:43 by Ben Bowen

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap {
	[TestClass]
	public class RectangleTest {

		[TestInitialize]
		public void SetUp() { }

		static readonly Rectangle testangleA = new Rectangle(0f, 0f, 0f, 0f);
		static readonly Rectangle testangleB = new Rectangle(10f, 10f, 30f, 50f);
		static readonly Rectangle testangleC = new Rectangle(-10f, -10f, -30f, 40f);
		static readonly Rectangle testangleD = new Rectangle(12f, 12f, 30f, 30f);
		static readonly Rectangle testangleE = new Rectangle(50f, 50f, -10f, -100f);
		static readonly Rectangle testangleF = new Rectangle(10f, 10f, -1f, -1f);
		static readonly Rectangle testangleG = new Rectangle(0f, 0f, 9.5f, 9.5f);
		static readonly Rectangle testangleH = new Rectangle(0f, 0f, 500f, 9.5f);

		static readonly Rectangle[] testangles = {
			testangleA,
			testangleB,
			testangleC,
			testangleD,
			testangleE,
			testangleF,
			testangleG,
			testangleH
		};


		#region Tests
		[TestMethod]
		public void TestArea() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(0f,		testangleA.Area);
			Assert.AreEqual(1500f,	testangleB.Area);
			Assert.AreEqual(1200f,	testangleC.Area);
			Assert.AreEqual(900f,	testangleD.Area);
			Assert.AreEqual(1000f,	testangleE.Area);
			Assert.AreEqual(1f,		testangleF.Area);
			Assert.AreEqual(90.25f, testangleG.Area);
			Assert.AreEqual(4750f,	testangleH.Area);
		}

		[TestMethod]
		public void TestCtor() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(new Rectangle(0f, 0f, 0f, 0f),		testangleA);
			Assert.AreEqual(new Rectangle(10f, 10f, 30f, 50f),	testangleB);
			Assert.AreEqual(new Rectangle(-40f, -10f, 30f, 40f),	testangleC);
			Assert.AreEqual(new Rectangle(12f, 12f, 30f, 30f),	testangleD);
			Assert.AreEqual(new Rectangle(40f, -50f, 10f, 100f),	testangleE);
			Assert.AreEqual(new Rectangle(9f, 9f, 1f, 1f),	testangleF);
			Assert.AreEqual(new Rectangle(0f, 0f, 9.5f, 9.5f), testangleG);
			Assert.AreEqual(new Rectangle(0f, 0f, 500f, 9.5f),	testangleH);
		}

		[TestMethod]
		public void TestGetCorners() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			foreach (Rectangle testangle in testangles) {
				Assert.AreEqual(testangle.BottomLeft.X, testangle.GetCornerX(Rectangle.RectangleCorner.TopLeft));
				Assert.AreEqual(testangle.BottomLeft.X, testangle.GetCornerX(Rectangle.RectangleCorner.BottomLeft));
				Assert.AreEqual(testangle.BottomLeft.X + testangle.Width, testangle.GetCornerX(Rectangle.RectangleCorner.TopRight));
				Assert.AreEqual(testangle.BottomLeft.X + testangle.Width, testangle.GetCornerX(Rectangle.RectangleCorner.BottomRight));

				Assert.AreEqual(testangle.BottomLeft.Y + testangle.Height, testangle.GetCornerY(Rectangle.RectangleCorner.TopLeft));
				Assert.AreEqual(testangle.BottomLeft.Y + testangle.Height, testangle.GetCornerY(Rectangle.RectangleCorner.TopRight));
				Assert.AreEqual(testangle.BottomLeft.Y, testangle.GetCornerY(Rectangle.RectangleCorner.BottomLeft));
				Assert.AreEqual(testangle.BottomLeft.Y, testangle.GetCornerY(Rectangle.RectangleCorner.BottomRight));
			}
		}

		[TestMethod]
		public void TestIntersex() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsFalse(testangleA.Intersects(testangleB));
			Assert.IsFalse(testangleB.Intersects(testangleC));
			Assert.IsFalse(testangleC.Intersects(testangleD));
			Assert.IsTrue(testangleD.Intersects(testangleE));
			Assert.IsFalse(testangleE.Intersects(testangleF));
			Assert.IsTrue(testangleF.Intersects(testangleG));
			Assert.IsTrue(testangleG.Intersects(testangleH));

			Assert.IsFalse(testangleA.Intersects(testangleC));
			Assert.IsTrue(testangleB.Intersects(testangleD));
			Assert.IsFalse(testangleC.Intersects(testangleE));
			Assert.IsFalse(testangleD.Intersects(testangleF));
			Assert.IsFalse(testangleE.Intersects(testangleG));
			Assert.IsTrue(testangleF.Intersects(testangleH));
			Assert.IsTrue(testangleG.Intersects(testangleA));

			foreach (Rectangle testangle1 in testangles) {
				foreach (Rectangle testangle2 in testangles) {
					Assert.AreEqual(testangle1.Intersects(testangle2), testangle2.Intersects(testangle1));
				}
			}
		}

		[TestMethod]
		public void TestContainsPoint() {
			// Define variables and constants
			Vector2 pointA = new Vector2(0f, 0f);
			Vector2 pointB = new Vector2(-10f, 10f);
			Vector2 pointC = new Vector2(5f, 5f);


			// Set up context


			// Execute


			// Assert outcome
			Assert.IsTrue(testangleA.Contains(pointA));
			Assert.IsFalse(testangleB.Contains(pointA));
			Assert.IsFalse(testangleC.Contains(pointA));
			Assert.IsFalse(testangleD.Contains(pointA));
			Assert.IsFalse(testangleE.Contains(pointA));
			Assert.IsFalse(testangleF.Contains(pointA));
			Assert.IsTrue(testangleG.Contains(pointA));
			Assert.IsTrue(testangleH.Contains(pointA));

			Assert.IsFalse(testangleA.Contains(pointB));
			Assert.IsFalse(testangleB.Contains(pointB));
			Assert.IsTrue(testangleC.Contains(pointB));
			Assert.IsFalse(testangleD.Contains(pointB));
			Assert.IsFalse(testangleE.Contains(pointB));
			Assert.IsFalse(testangleF.Contains(pointB));
			Assert.IsFalse(testangleG.Contains(pointB));
			Assert.IsFalse(testangleH.Contains(pointB));

			Assert.IsFalse(testangleA.Contains(pointC));
			Assert.IsFalse(testangleB.Contains(pointC));
			Assert.IsFalse(testangleC.Contains(pointC));
			Assert.IsFalse(testangleD.Contains(pointC));
			Assert.IsFalse(testangleE.Contains(pointC));
			Assert.IsFalse(testangleF.Contains(pointC));
			Assert.IsTrue(testangleG.Contains(pointC));
			Assert.IsTrue(testangleH.Contains(pointC));
		}

		[TestMethod]
		public void TestCircleIntersect() {
			// Define variables and constants
			Circle testCircleA = new Circle(12f, 10f, 35f);
			Circle testCircleB = new Circle(-3, -3f, 9f);

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsTrue(testangleA.Intersects(testCircleA));
			Assert.IsTrue(testangleB.Intersects(testCircleA));
			Assert.IsTrue(testangleC.Intersects(testCircleA));
			Assert.IsTrue(testangleD.Intersects(testCircleA));
			Assert.IsFalse(testangleE.Intersects(testCircleA));
			Assert.IsTrue(testangleF.Intersects(testCircleA));
			Assert.IsTrue(testangleG.Intersects(testCircleA));
			Assert.IsTrue(testangleH.Intersects(testCircleA));

			Assert.IsTrue(testangleA.Intersects(testCircleB));
			Assert.IsFalse(testangleB.Intersects(testCircleB));
			Assert.IsFalse(testangleC.Intersects(testCircleB));
			Assert.IsFalse(testangleD.Intersects(testCircleB));
			Assert.IsFalse(testangleE.Intersects(testCircleB));
			Assert.IsFalse(testangleF.Intersects(testCircleB));
			Assert.IsTrue(testangleG.Intersects(testCircleB));
			Assert.IsTrue(testangleH.Intersects(testCircleB));
		}

		[TestMethod]
		public void TestEquals() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(testangleA, testangleA);
			Assert.IsTrue(testangleA.EqualsWithTolerance(new Rectangle(0.5f, 0.5f, 0.5f, 0.5f), 1f));
			Assert.IsFalse(testangleA.EqualsWithTolerance(testangleB, 1f));
			Assert.AreNotEqual(testangleC, testangleD);
		}

		[TestMethod]
		public void TestContainsCircle() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsFalse(testangleA.Contains(new Circle(0f, 0f, 1f)));
			Assert.IsFalse(testangleB.Contains(new Circle(25f, 30f, 16f)));
			Assert.IsTrue(testangleC.Contains(new Circle(-25f, 10f, 15f)));
			Assert.IsTrue(testangleD.Contains(new Circle(27f, 27f, 15f)));
			Assert.IsTrue(testangleE.Contains(new Circle(43f, 0f, 2f)));
		}

		[TestMethod]
		public void TestContainsRectangle() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsFalse(testangleA.Contains(new Rectangle(0f, 0f, 1f, 1f)));
			Assert.IsFalse(testangleB.Contains(new Rectangle(20f, 20f, 20f, 41f)));
			Assert.IsTrue(testangleC.Contains(new Rectangle(-40f, -10f, 30f, 40f)));
			Assert.IsTrue(testangleD.Contains(new Rectangle(22f, 22f, 10f, 10f)));
			Assert.IsTrue(testangleE.Contains(new Rectangle(45f, 0f, 5f, 50f)));
		}

		[TestMethod]
		public void TestIntersectionWith() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(new Rectangle(0f, 0f, 0f, 0f), testangleA.IntersectionWith(testangleB));
			Assert.AreEqual(new Rectangle(40f, 12f, 2f, 30f), testangleD.IntersectionWith(testangleE));
			Assert.AreEqual(new Rectangle(9f, 9f, 0.5f, 0.5f), testangleF.IntersectionWith(testangleG));
			Assert.AreEqual(new Rectangle(0f, 0f, 9.5f, 9.5f), testangleG.IntersectionWith(testangleH));

			foreach (Rectangle testangle1 in testangles) {
				foreach (Rectangle testangle2 in testangles) {
					Assert.AreEqual(testangle1.IntersectionWith(testangle2), testangle2.IntersectionWith(testangle1));
				}
			}
		}

		[TestMethod]
		public void TestDistanceFromPoint() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(0f, testangleD.DistanceFrom(new Vector2(20f, 20f)));
			Assert.AreEqual(0f, testangleE.DistanceFrom(new Vector2(40f, -50f)));
			Assert.AreEqual(1f, testangleF.DistanceFrom(new Vector2(8f, 9f)));
			Assert.AreEqual(22.5f, testangleG.DistanceFrom(new Vector2(0f, 32f)));
			Assert.AreEqual((float) Math.Sqrt(32 * 32 + 32 * 32), testangleH.DistanceFrom(new Vector2(532f, 41.5f)));
		}

		[TestMethod]
		public void TestDistanceFromRectangle() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual((float) Math.Sqrt(100f + 100f), testangleA.DistanceFrom(testangleB));
			Assert.AreEqual(20f, testangleB.DistanceFrom(testangleC));
			Assert.AreEqual(22f, testangleC.DistanceFrom(testangleD));
			Assert.AreEqual(30f, testangleE.DistanceFrom(testangleF));

			foreach (Rectangle testangle1 in testangles) {
				foreach (Rectangle testangle2 in testangles) {
					Assert.AreEqual(testangle1.DistanceFrom(testangle2), testangle2.DistanceFrom(testangle1));
				}
			}
		}

		[TestMethod]
		public void TestDistanceFromCircle() {
			// Define variables and constants
			Circle testCircleA = new Circle(5f, 5f, 10f);
			Circle testCircleB = new Circle(-5f, -5f, -10f);
			Circle testCircleC = new Circle(0f, 0f, -10f);
			Circle testCircleD = new Circle(20f, 20f, 0f);
			Circle testCircleE = new Circle(19f, 19f, 1f);

			Circle[] testCircles = {
				testCircleA, testCircleB, testCircleC, testCircleD, testCircleE
			};

			// Set up context


			// Execute


			// Assert outcome
			foreach (Rectangle testangle in testangles) {
				foreach (Circle testCircle in testCircles) {
					Assert.AreEqual(testangle.DistanceFrom(testCircle), testCircle.DistanceFrom(testangle));
				}
			}
		}
		#endregion
	}
}