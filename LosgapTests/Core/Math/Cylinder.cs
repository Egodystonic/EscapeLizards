// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 05 12 2014 at 10:40 by Ben Bowen

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap {
	[TestClass]
	public class CylinderTest {

		[TestInitialize]
		public void SetUp() { }

		private static readonly Cylinder testCylinderA = new Cylinder(0f, 0f, 0f, 6f, 10f);
		private static readonly Cylinder testCylinderB = new Cylinder(0f, 0f, 0f, 11f, 2f);
		private static readonly Cylinder testCylinderC = new Cylinder(10f, -10f, 5f, -10f, -10f);
		private static readonly Cylinder testCylinderD = new Cylinder(4f, 5f, 6f, 0f, 0f);
		private static readonly Cylinder testCylinderE = new Cylinder(1f, 2f, 3f, 5f, 4f);

		private static readonly Cylinder[] testCylinders = {
			testCylinderA,
			testCylinderB,
			testCylinderC,
			testCylinderD,
			testCylinderE
		};

		#region Tests
		[TestMethod]
		public void TestSurfaceArea() {
			// Define variables and constants


			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(603.19f, testCylinderA.SurfaceArea, 0.01f);
			Assert.AreEqual(898.5f, testCylinderB.SurfaceArea, 0.01f);
			Assert.AreEqual(1256.64f, testCylinderC.SurfaceArea, 0.01f);
			Assert.AreEqual(0f, testCylinderD.SurfaceArea, 0.01f);
			Assert.AreEqual(282.74f, testCylinderE.SurfaceArea, 0.01f);
		}

		[TestMethod]
		public void TestVolume() {
			// Define variables and constants


			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(1130.97f, testCylinderA.Volume, 0.01f);
			Assert.AreEqual(760.27f, testCylinderB.Volume, 0.01f);
			Assert.AreEqual(3141.59f, testCylinderC.Volume, 0.01f);
			Assert.AreEqual(0f, testCylinderD.Volume, 0.01f);
			Assert.AreEqual(314.16f, testCylinderE.Volume, 0.01f);
		}

		[TestMethod]
		public void TestConstructor() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(new Cylinder(Vector3.ZERO, 6f, 10f), testCylinderA);
			Assert.AreEqual(new Cylinder(new Vector3(1f, 2f, 3f), 5f, 4f), testCylinderE);

			Assert.AreEqual(10f, testCylinderC.Height);
			Assert.AreEqual(10f, testCylinderC.Radius);
		}

		[TestMethod]
		public void TestToConicalFrustum() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(new Cone(0f, 5f, 0f, 6f, 10f, 6f), testCylinderA.ToSymmetricalConicalFrustum());
			Assert.AreEqual(new Cone(0f, 1f, 0f, 11f, 2f, 11f), testCylinderB.ToSymmetricalConicalFrustum());
			Assert.AreEqual(new Cone(10f, -5f, 5f, 10f, 10f, 10f), testCylinderC.ToSymmetricalConicalFrustum());
			Assert.AreEqual(new Cone(4f, 5f, 6f, 0f, 0f, 0f), testCylinderD.ToSymmetricalConicalFrustum());
			Assert.AreEqual(new Cone(1f, 4f, 3f, 5f, 4f, 5f), testCylinderE.ToSymmetricalConicalFrustum());
		}

		[TestMethod]
		public void TestContainsPoint() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsTrue(testCylinderA.Contains(Vector3.ZERO));
			Assert.IsTrue(testCylinderB.Contains(new Vector3(1f, -1f, 0f)));
			Assert.IsTrue(testCylinderC.Contains(new Vector3(10f, -10f, 5f) + new Vector3(1f, 0f, 1f).WithLength(10f)));
			Assert.IsFalse(testCylinderD.Contains(new Vector3(4f, 5f, 6.1f)));
			Assert.IsFalse(testCylinderE.Contains(new Vector3(4f, 5f, 6f)));
		}

		[TestMethod]
		public void TestContainsRay() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsTrue(testCylinderA.Contains(new Ray(Vector3.ZERO, Vector3.UP, 5f)));
			Assert.IsTrue(testCylinderB.Contains(new Ray(Vector3.LEFT.WithLength(5.5f), Vector3.RIGHT, 11f)));
			Assert.IsTrue(testCylinderC.Contains(new Ray(new Vector3(10f, -10f, 5f), Vector3.UP, 0f)));
			Assert.IsFalse(testCylinderD.Contains(new Ray(new Vector3(4, 5, 6), Vector3.RIGHT, 1f)));
			Assert.IsFalse(testCylinderE.Contains(new Ray(new Vector3(1, 2, 3), Vector3.UP)));
		}

		[TestMethod]
		public void TestContainsCylinder() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsTrue(testCylinderA.Contains(new Cylinder(0f, 0f, 0f, 6f, 10f)));
			Assert.IsTrue(testCylinderB.Contains(new Cylinder(0.707f, 0.5f, -0.707f, 10f, 1f)));
			Assert.IsFalse(testCylinderC.Contains(new Cylinder(15f, -10f, 10f, 5f, 5f)));
			Assert.IsFalse(testCylinderD.Contains(new Cylinder(4f, 5f, 6f, 0f, 0.1f)));
			Assert.IsFalse(testCylinderE.Contains(new Cylinder(1f, 2f, 3f, 5.1f, 4f)));

			foreach (Cylinder testCylinder1 in testCylinders) {
				foreach (Cylinder testCylinder2 in testCylinders) {
					Assert.AreEqual(testCylinder1.Contains(testCylinder2), testCylinder2.Contains(testCylinder1));
				}
			}
		}

		[TestMethod]
		public void TestContainsCone() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsTrue(testCylinderA.Contains(new Cone(0f, 5f, 0f, 6f, 10f)));
			Assert.IsTrue(testCylinderB.Contains(new Cone(0f, 1f, 0f, 1f, 1f, 2f)));
			Assert.IsFalse(testCylinderB.Contains(new Cone(0f, 1f, 0f, 1f, 3f, 2f)));
			Assert.IsTrue(testCylinderC.Contains(new Cone(10f + 1.414f, -5f, 5f + 1.414f, 8f, 10f)));
			Assert.IsFalse(testCylinderD.Contains(new Cone(4f, 5f, 6f, 0f, 0.1f)));
			Assert.IsFalse(testCylinderE.Contains(new Cone(1f, 2f, 3f, 4f, 4f)));
			Assert.IsFalse(testCylinderE.Contains(new Cone(new Vector3(1f, 2f, 3f) + new Vector3(1f, 0f, 1f).WithLength(4f), 1.1f, 1f)));
		}

		[TestMethod]
		public void TestContainsCuboid() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsTrue(testCylinderA.Contains(new Cuboid(0f, 0f, 0f, 4.24f, 5f, 4.24f)));
			Assert.IsTrue(testCylinderB.Contains(new Cuboid(0f, -1f, 0f, 4.24f, 2f, 4.24f)));
			Assert.IsTrue(testCylinderC.Contains(new Cuboid(10f, -5f, 5f, 0f, 0f, 0f)));
			Assert.IsFalse(testCylinderD.Contains(new Cuboid(4f, 5f, 6f, 0.1f, 0f, 0f)));
			Assert.IsFalse(testCylinderE.Contains(new Cuboid(1f, 2f, 3f, 1f, 3f, 1f)));
			Assert.IsFalse(testCylinderE.Contains(new Cuboid(1f, 2f, 3f, 3f, 1f, -5.1f)));
		}

		[TestMethod]
		public void TestContainsSphere() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsTrue(testCylinderA.Contains(new Sphere(0f, 0f, 0f, 6f)));
			Assert.IsTrue(testCylinderB.Contains(new Sphere(0f, 0f, 9f, 2f)));
			Assert.IsTrue(testCylinderC.Contains(new Sphere(10f, -10f, 5f, 10f)));
			Assert.IsFalse(testCylinderD.Contains(new Sphere(4f, 5f, 6f, 0.1f)));
			Assert.IsFalse(testCylinderE.Contains(new Sphere(0.9f, 2f, 3f, 5f)));
		}

		[TestMethod]
		public void TestIntersectsCylinder() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsTrue(testCylinderA.Intersects(new Cylinder(-3f, 3f, -3f, 6f, 6f)));
			Assert.IsTrue(testCylinderB.Intersects(new Cylinder(-3f, 3f, -3f, 6f, 6f)));
			Assert.IsTrue(testCylinderC.Intersects(new Cylinder(20f, 0f, 15f, 20f, 30f)));
			Assert.IsFalse(testCylinderD.Intersects(new Cylinder(3f, 4f, 5f, 0f, 0f)));
			Assert.IsFalse(testCylinderE.Intersects(new Cylinder(5.950f, 2.000f, 7.950f, 1.99f, 4f)));
		}
		#endregion
	}
}