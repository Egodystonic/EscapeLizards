// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 05 12 2014 at 15:06 by Ben Bowen

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap {
	[TestClass]
	public class ConeTest {

		[TestInitialize]
		public void SetUp() { }

		private static readonly Cone testConeA = new Cone(new Vector3(0f, 0f, 0f), 3f, 3f, 0f);
		private static readonly Cone testConeB = new Cone(new Vector3(0f, 0f, 0f), -3f, -3f, -1f);
		private static readonly Cone testConeC = new Cone(new Vector3(5f, 5f, 5f), 0f, 1f, 3f);
		private static readonly Cone testConeD = new Cone(new Vector3(-5f, -5f, -5f), 3f, 0f, 0f);
		private static readonly Cone testConeE = new Cone(new Vector3(1f, 2f, 3f), 4f, 5f, 6f);

		private static readonly Cone[] testCones = {
			testConeA, testConeB, testConeC, testConeD, testConeE
		};

		#region Tests
		[TestMethod]
		public void TestBottomCenter() {
			Assert.AreEqual(new Vector3(0f, -3f, 0f), testConeA.BottomCenter);
			Assert.AreEqual(new Vector3(0f, 0f, 0f), testConeB.BottomCenter);
			Assert.AreEqual(new Vector3(5f, 4f, 5f), testConeC.BottomCenter);
			Assert.AreEqual(new Vector3(-5f, -5f, -5f), testConeD.BottomCenter);
			Assert.AreEqual(new Vector3(1f, -3f, 3f), testConeE.BottomCenter);
		}

		[TestMethod]
		public void TestIsConicalFrustum() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsFalse(testConeA.IsConicalFrustum);
			Assert.IsTrue(testConeB.IsConicalFrustum);
			Assert.IsFalse(testConeC.IsConicalFrustum);
			Assert.IsFalse(testConeD.IsConicalFrustum);
			Assert.IsTrue(testConeE.IsConicalFrustum);
		}

		[TestMethod]
		public void TestSurfaceArea() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(68.26f, testConeA.SurfaceArea, 0.01f);
			Assert.AreEqual(76.72f, testConeB.SurfaceArea, 0.01f);
			Assert.AreEqual(58.08f, testConeC.SurfaceArea, 0.01f);
			Assert.AreEqual(0f, testConeD.SurfaceArea, 0.01f);
			Assert.AreEqual(332.54f, testConeE.SurfaceArea, 0.01f);
		}

		[TestMethod]
		public void TestVolume() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(28.27f, testConeA.Volume, 0.01f);
			Assert.AreEqual(40.84f, testConeB.Volume, 0.01f);
			Assert.AreEqual(9.42f, testConeC.Volume, 0.01f);
			Assert.AreEqual(0f, testConeD.Volume, 0.01f);
			Assert.AreEqual(397.93f, testConeE.Volume, 0.01f);
		}

		[TestMethod]
		public void TestCtor() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(new Cone(1f, 2f, 3f, 4f, 5f, 6f), testConeE);
			Assert.AreEqual(new Cone(0f, 3f, 0f, 1f, 3f, 3f), testConeB);
		}

		[TestMethod]
		public void TestGetRadius() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(1f, testConeA.GetRadius(1f), 0.01f);
			Assert.AreEqual(2.33f, testConeB.GetRadius(1f), 0.01f);
			Assert.AreEqual(0f, testConeC.GetRadius(1f), 0.01f);
			Assert.AreEqual(0f, testConeD.GetRadius(0f), 0.01f);
			Assert.AreEqual(5f, testConeE.GetRadius(2.5f), 0.01f);
		}

		[TestMethod]
		public void TestInverse() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(new Cone(0f, 0f, 0f, 0f, 3f, 3f), testConeA.Inverse);
			Assert.AreEqual(new Cone(0f, 3f, 0f, 3f, 3f, 1f), testConeB.Inverse);
			Assert.AreEqual(new Cone(5f, 5f, 5f, 3f, 1f, 0f), testConeC.Inverse);
			Assert.AreEqual(new Cone(-5f, -5f, -5f, 0f, 0f, 3f), testConeD.Inverse);
			Assert.AreEqual(new Cone(1f, 2f, 3f, 6f, 5f, 4f), testConeE.Inverse);
		}

		[TestMethod]
		public void TestContainsPoint() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsTrue(testConeA.Contains(new Vector3(0f, -1f, 0f)));
			Assert.IsTrue(testConeB.Contains(Vector3.ONE.WithLength(2f)));
			Assert.IsTrue(testConeC.Contains(new Vector3(5f, 4f, 5f)));
			Assert.IsFalse(testConeD.Contains(new Vector3(-5f, -5.1f, -5f)));
			Assert.IsFalse(testConeE.Contains(new Vector3(5.1f, 7f, 3f)));
			Assert.IsFalse(testConeE.Contains(new Vector3(1f, 2f, 9.1f)));
		}

		[TestMethod]
		public void TestContainsRay() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsTrue(testConeA.Contains(new Ray(new Vector3(0f, -1f, 0f), Vector3.UP, 1f)));
			Assert.IsTrue(testConeB.Contains(Ray.FromStartAndEndPoint(Vector3.ONE.WithLength(2f), Vector3.ONE.WithLength(1f))));
			Assert.IsTrue(testConeC.Contains(new Ray(new Vector3(5, 4, 5), Vector3.UP, 1f)));
			Assert.IsFalse(testConeD.Contains(new Ray(new Vector3(-5f, -5f, -5f), Vector3.UP, 1f)));
			Assert.IsFalse(testConeE.Contains(new Ray(new Vector3(1f, 2f, 3f), Vector3.DOWN)));
		}

		[TestMethod]
		public void TestContainsCone() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsTrue(testConeA.Contains(new Cone(0f, -1f, 0f, 2f, 1f, 0f)));
			Assert.IsTrue(testConeB.Contains(new Cone(0f, 1f, 0f, 0.33f, 1f, 1f)));
			Assert.IsTrue(testConeC.Contains(new Cone(5f, 5f, 5f, 0.25f, 0.25f, 3f)));
			Assert.IsFalse(testConeD.Contains(new Cone(-5f, -5f, -5f, 3f, 0.1f, 0f)));
			Assert.IsFalse(testConeE.Contains(new Cone(1f, 2f, 3f, 1f, 6f, 1f)));

			foreach (Cone testCone1 in testCones) {
				foreach (Cone testCone2 in testCones) {
					Assert.AreEqual(testCone1.Contains(testCone2), testCone2.Contains(testCone1));
				}
			}
		}

		[TestMethod]
		public void TestContainsCuboid() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsTrue(testConeA.Contains(new Cuboid(-0.5f, -3f, -0.5f, 1f, 1f, 1f)));
			Assert.IsTrue(testConeB.Contains(new Cuboid(-0.707f, 0f, -0.707f, 1.414f, 3f, 1.414f)));
			Assert.IsTrue(testConeC.Contains(new Cuboid(5f, 4.5f, 5f, 0.707f, 0.5f, 0.707f)));
			Assert.IsFalse(testConeD.Contains(new Cuboid(-5f, -5f, -5f, 0f, 0f, 0.1f)));
			Assert.IsFalse(testConeE.Contains(new Cuboid(1f, 0f, 3f, 4f, 1f, 6f)));
		}

		[TestMethod]
		public void TestIntersectsCone() {
			// Define variables and constants
			Assert.IsTrue(testConeA.Intersects(new Cone(0f, 1f, 0f, 4f, 5f, 0f)));
			Assert.IsTrue(testConeB.Intersects(new Cone(0f, 0f, 0f, -3f, -3f, -1f)));
			Assert.IsTrue(testConeC.Intersects(new Cone(5f, 4f, 5f, 1f, 1f)));
			Assert.IsFalse(testConeD.Intersects(new Cone(100f, 100f, 100f, 0f, 0f)));
			Assert.IsTrue(testConeE.Intersects(new Cone(1f, 3f, 3f, 10f, 1f - MathUtils.FlopsErrorMargin, 1f)));

			// Set up context


			// Execute


			// Assert outcome
			foreach (Cone testCone1 in testCones) {
				foreach (Cone testCone2 in testCones) {
					Assert.AreEqual(testCone1.Intersects(testCone2), testCone2.Intersects(testCone1));
				}
			}
		}
		#endregion
	}
}