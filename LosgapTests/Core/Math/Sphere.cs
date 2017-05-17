// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 26 11 2014 at 15:23 by Ben Bowen

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap {
	[TestClass]
	public class SphereTest {

		[TestInitialize]
		public void SetUp() { }

		static readonly Sphere testSphereA = new Sphere(5f, 5f, 2.5f, 10f);
		static readonly Sphere testSphereB = new Sphere(-5f, -5f, -2.5f, -10f);
		static readonly Sphere testSphereC = new Sphere(0f, 0f, 0f, -10f);
		static readonly Sphere testSphereD = new Sphere(20f, 20f, 10f, 0f);
		static readonly Sphere testSphereE = new Sphere(19f, 19f, 9.5f, 1f);

		static readonly Sphere[] testSpheres = {
			testSphereA, testSphereB, testSphereC, testSphereD, testSphereE
		};

		#region Tests
		[TestMethod]
		public void TestPointProjection() {
			Assert.AreEqual(Vector3.UP * 10f + new Vector3(5f, 5f, 2.5f), testSphereA.PointProjection(new Vector3(5f, 105f, 2.5f)));
			Assert.AreEqual(Vector3.DOWN * 10f + new Vector3(-5f, -5f, -2.5f), testSphereB.PointProjection(new Vector3(-5f, -105f, -2.5f)));
			Assert.AreEqual(new Vector3(4f, 5f, 6f).WithLength(10f), testSphereC.PointProjection(new Vector3(4f, 5f, 6f)));
			Assert.AreEqual(new Vector3(20f, 20f, 10f), testSphereD.PointProjection(new Vector3(99f, -13f, 0.3f)));
			Assert.AreEqual(new Vector3(19f, 19f, 9.5f) - Vector3.ONE.ToUnit(), testSphereE.PointProjection(testSphereE.Center - Vector3.ONE));
		}

		[TestMethod]
		public void TestVolume() {
			// Define variables and constants


			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(4188.79f, testSphereA.Volume, 0.01f);
			Assert.AreEqual(4188.79f, testSphereB.Volume, 0.01f);
			Assert.AreEqual(4188.79f, testSphereC.Volume, 0.01f);
			Assert.AreEqual(0f, testSphereD.Volume, 0.01f);
			Assert.AreEqual(4.19f, testSphereE.Volume, 0.01f);
		}

		[TestMethod]
		public void TestSurfaceArea() {
			// Define variables and constants


			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(1256.64f, testSphereA.SurfaceArea, 0.01f);
			Assert.AreEqual(1256.64f, testSphereB.SurfaceArea, 0.01f);
			Assert.AreEqual(1256.64f, testSphereC.SurfaceArea, 0.01f);
			Assert.AreEqual(0f, testSphereD.SurfaceArea, 0.01f);
			Assert.AreEqual(12.57f, testSphereE.SurfaceArea, 0.01f);
		}

		[TestMethod]
		public void TestCtor() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(new Sphere(new Circle(5f, 5f, 10f), 2.5f), testSphereA);
			Assert.AreEqual(new Sphere(new Circle(-5f, -5f, 10f), -2.5f), testSphereB);
			Assert.AreEqual(new Sphere(new Circle(0f, 0f, 10f), 0f), testSphereC);
			Assert.AreEqual(new Sphere(new Circle(20f, 20f, 0f), 10f), testSphereD);
			Assert.AreEqual(new Sphere(new Circle(19f, 19f, 1f), 9.5f), testSphereE);
		}

		[TestMethod]
		public void TestGetSlice() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(8.66025448f, testSphereA.GetRadius(5f));
			Assert.AreEqual(9.797959f, testSphereB.GetRadius(-2f));
			Assert.AreEqual(0f, testSphereC.GetRadius(10f));
			Assert.AreEqual(0f, testSphereD.GetRadius(0f));
			Assert.AreEqual(0.1410673f, testSphereE.GetRadius(0.99f));
		}

		[TestMethod]
		public void TestIntersectsSphere() {
			// Define variables and constants


			// Set up context


			// Execute


			// Assert outcome
			Assert.IsTrue(testSphereA.Intersects(testSphereA));
			Assert.IsTrue(testSphereA.Intersects(testSphereB));
			Assert.IsTrue(testSphereA.Intersects(testSphereC));
			Assert.IsFalse(testSphereA.Intersects(testSphereD));
			Assert.IsFalse(testSphereA.Intersects(testSphereE));

			Assert.IsTrue(testSphereB.Intersects(testSphereB));
			Assert.IsTrue(testSphereB.Intersects(testSphereC));
			Assert.IsFalse(testSphereB.Intersects(testSphereD));
			Assert.IsFalse(testSphereB.Intersects(testSphereE));

			Assert.IsTrue(testSphereC.Intersects(testSphereC));
			Assert.IsFalse(testSphereC.Intersects(testSphereD));
			Assert.IsFalse(testSphereC.Intersects(testSphereE));

			Assert.IsTrue(testSphereD.Intersects(testSphereD));
			Assert.IsFalse(testSphereD.Intersects(testSphereE));

			Assert.IsTrue(testSphereE.Intersects(testSphereE));

			foreach (Sphere testSphere1 in testSpheres) {
				foreach (Sphere testSphere2 in testSpheres) {
					Assert.AreEqual(testSphere1.Intersects(testSphere2), testSphere2.Intersects(testSphere1));
				}
			}
		}

		[TestMethod]
		public void TestContainsPoint() {
			// Define variables and constants
			Vector3 pointA = new Vector3(0f, 0f, 0f);
			Vector3 pointB = new Vector3(5f, 5f, 5f);
			Vector3 pointC = new Vector3(20f, 20f, 10f);

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsTrue(testSphereA.Contains(pointA));
			Assert.IsTrue(testSphereB.Contains(pointA));
			Assert.IsTrue(testSphereC.Contains(pointA));
			Assert.IsFalse(testSphereD.Contains(pointA));
			Assert.IsFalse(testSphereE.Contains(pointA));

			Assert.IsTrue(testSphereA.Contains(pointB));
			Assert.IsFalse(testSphereB.Contains(pointB));
			Assert.IsTrue(testSphereC.Contains(pointB));
			Assert.IsFalse(testSphereD.Contains(pointB));
			Assert.IsFalse(testSphereE.Contains(pointB));

			Assert.IsFalse(testSphereA.Contains(pointC));
			Assert.IsFalse(testSphereB.Contains(pointC));
			Assert.IsFalse(testSphereC.Contains(pointC));
			Assert.IsTrue(testSphereD.Contains(pointC));
			Assert.IsFalse(testSphereE.Contains(pointC));
		}

		[TestMethod]
		public void TestContainsRay() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsTrue(testSphereA.Contains(new Ray(new Vector3(5f, 5f, 2.5f), Vector3.RIGHT, 10f)));
			Assert.IsTrue(testSphereB.Contains(new Ray(new Vector3(-5f, -5f, -2.5f), Vector3.RIGHT, 1f)));
			Assert.IsTrue(testSphereC.Contains(new Ray(new Vector3(0f, 0f, 0f), Vector3.RIGHT, 0f)));
			Assert.IsFalse(testSphereD.Contains(new Ray(new Vector3(20f, 20f, 10f), Vector3.RIGHT, 0.1f)));
			Assert.IsFalse(testSphereE.Contains(new Ray(new Vector3(19f, 19f, 9.5f), Vector3.RIGHT)));
		}

		[TestMethod]
		public void TestEquality() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(testSphereC, testSphereC);
			Assert.IsTrue(testSphereD.EqualsWithTolerance(testSphereE, 2f));
			Assert.IsFalse(testSphereD.EqualsExactly(testSphereE));
			Assert.AreNotEqual(testSphereA, testSphereB);
		}

		[TestMethod]
		public void TestDistanceFromCuboid() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(10f, testSphereA.DistanceFrom(new Cuboid(25f, -15f, 22.5f, 10f, 10f, -10f)));
			Assert.AreEqual((float) Math.Sqrt(25f + 100f + (15f * 15f)), testSphereB.DistanceFrom(new Cuboid(10f, 15f, 22.5f, 0f, 0f, 0f)));
			Assert.AreEqual(1f, testSphereC.DistanceFrom(new Cuboid(11f, 10f, 10f, 1f, 2f, 3f)));
			Assert.AreEqual(0f, testSphereD.DistanceFrom(new Cuboid(10f, 10f, 5f, 20f, 20f, 10f)));
			Assert.AreEqual(0f, testSphereE.DistanceFrom(new Cuboid(20f, 20f, 10.5f, 1f, 1f, 1f)));
		}

		[TestMethod]
		public void TestContainsSphere() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsTrue(testSphereA.Contains(new Sphere(5f, 5f, 2.5f, 9f)));
			Assert.IsTrue(testSphereB.Contains(new Sphere(-5f, -5f, -2.5f, 10f)));
			Assert.IsTrue(testSphereC.Contains(new Sphere(new Vector3(1f, 1f, 1f).WithLength(9f), 1f)));
			Assert.IsFalse(testSphereD.Contains(new Sphere(20f, 20f, 10f, 1f)));
			Assert.IsFalse(testSphereE.Contains(new Sphere(18f, 19f, 9.5f, 1f)));
		}

		[TestMethod]
		public void TestContainsCone() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsTrue(testSphereA.Contains(new Cone(5f, 15f, 2.5f, 10f, 10f)));
			Assert.IsTrue(testSphereB.Contains(new Cone(-5f, 0f, -2.5f, 10f, 5f, 8.66025448f)));
			Assert.IsFalse(testSphereB.Contains(new Cone(0f, 0f, 2.5f, 10f, 5f, 9f)));
			Assert.IsTrue(testSphereC.Contains(new Cone(2f, 4f, -2f, 6f, 8f)));
			Assert.IsFalse(testSphereD.Contains(new Cone(20f, 20f, 10f, 1f, 1f, 1f)));
			Assert.IsFalse(testSphereE.Contains(new Cone(19f, 18f, 9.5f, 1f, 1f)));
		}

		[TestMethod]
		public void TestContainsCuboid() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsTrue(testSphereA.Contains(new Cuboid(4f, 4f, 1.5f, 2f, 2f, 2f)));
			Assert.IsTrue(testSphereB.Contains(new Cuboid(-5f, -5f, -2.5f, 3f, 3f, 3f)));
			Assert.IsTrue(testSphereC.Contains(new Cuboid(-4f, 4f, -4f, 0f, 0f, 0f)));
			Assert.IsFalse(testSphereD.Contains(new Cuboid(10f, 10f, 5f, 30f, 30f, 15f)));
			Assert.IsFalse(testSphereE.Contains(new Cuboid(19f, 19f, 9.5f, 1f, 1f, 1f)));
		}

		[TestMethod]
		public void TestContainsCylinder() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsTrue(testSphereA.Contains(new Cylinder(5f, 5f, 2.5f, 8.66025448f, 5f)));
			Assert.IsTrue(testSphereB.Contains(new Cylinder(0f, 0f, 0f, 1f, 1f)));
			Assert.IsTrue(testSphereC.Contains(new Cylinder(0f, 0f, 0f, 4.358899f, 9f)));
			Assert.IsFalse(testSphereD.Contains(new Cylinder(20f, 19.5f, 10f, 1f, 1f)));
			Assert.IsFalse(testSphereE.Contains(new Cylinder(19f, 18.5f, 9.5f, 1f, 1f)));
		}

		[TestMethod]
		public void TestDistanceFromPoint() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(10f, testSphereA.DistanceFrom(new Vector3(25f, 5f, 2.5f)));
			Assert.AreEqual(10f, testSphereB.DistanceFrom(new Vector3(-5f, -5f, 17.5f)));
			Assert.AreEqual((float) Math.Sqrt(1200f) - 10f, testSphereC.DistanceFrom(new Vector3(20f, 20f, 20f)));
			Assert.AreEqual(0f, testSphereD.DistanceFrom(new Vector3(20f, 20f, 10f)));
			Assert.AreEqual(0f, testSphereE.DistanceFrom(new Vector3(19.5f, 19.5f, 10f)));
		}

		[TestMethod]
		public void TestDistanceFromSphere() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual((float) Math.Sqrt(30000f) - 20f, testSphereC.DistanceFrom(new Sphere(100f, 100f, 100f, 10f)));
			Assert.AreEqual(0f, testSphereA.DistanceFrom(new Sphere(24f, 5f, 2.5f, 10f)));

			foreach (Sphere testSphere1 in testSpheres) {
				foreach (Sphere testSphere2 in testSpheres) {
					Assert.AreEqual(testSphere1.DistanceFrom(testSphere2), testSphere2.DistanceFrom(testSphere1));
				}
			}
		}

		[TestMethod]
		public void TestIntersectsCylinder() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsTrue(testSphereA.Intersects(new Cylinder(5f, 5f, 2.5f, 1f, 1f)));
			Assert.IsTrue(testSphereB.Intersects(new Cylinder(-5f, 7f, -2.5f, 1f, 6f)));
			Assert.IsTrue(testSphereC.Intersects(new Cylinder(10f, 9.5f, 0f, 5.65f, 1f)));
			Assert.IsFalse(testSphereD.Intersects(new Cylinder(100f, 20f, 100f, 60f, 100f)));
			Assert.IsFalse(testSphereC.Intersects(new Cylinder(10f, 10f, 0f, 5.5f, 1f)));
		}
		#endregion
	}
}