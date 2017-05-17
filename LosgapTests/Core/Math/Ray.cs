// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 09 12 2014 at 17:13 by Ben Bowen

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap {
	[TestClass]
	public class RayTest {

		[TestInitialize]
		public void SetUp() { }

		private static Ray testRayA = new Ray(Vector3.ZERO, Vector3.RIGHT);
		private static Ray testRayB = new Ray(new Vector3(10f, 0f, 0f), Vector3.LEFT, 20f);
		private static Ray testRayC = new Ray(new Vector3(30f, 30f, 30f), new Vector3(-1f, -1f, -1f));
		private static Ray testRayD = Ray.FromStartAndEndPoint(Vector3.ONE[0, 1, 2] * -30f, Vector3.ONE[0, 1, 2] * 30f);

		private static Ray[] testRays = { testRayA, testRayB, testRayC, testRayD };

		#region Tests
		[TestMethod]
		public void TestIsInfiniteLength() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsTrue(testRayA.IsInfiniteLength);
			Assert.IsFalse(testRayB.IsInfiniteLength);
			Assert.IsTrue(testRayC.IsInfiniteLength);
			Assert.IsFalse(testRayD.IsInfiniteLength);
		}

		[TestMethod]
		public void TestEndPoint() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(null, testRayA.EndPoint);
			Assert.AreEqual(new Vector3(-10f, 0f, 0f), testRayB.EndPoint);
			Assert.AreEqual(null, testRayC.EndPoint);
			Assert.AreEqual(new Vector3(30f, 30f, 30f), testRayD.EndPoint);
		}

		[TestMethod]
		public void TestCtor() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(Ray.FromStartAndEndPoint(new Vector3(10f, 0f, 0f), new Vector3(-10f, 0f, 0f)), testRayB);
			Assert.AreEqual(new Ray(new Vector3(Vector3.ONE[0, 1, 2] * -30f), Vector3.ONE[0, 1, 2], Vector3.Distance(Vector3.ONE[0, 1, 2] * -30f, Vector3.ONE[0, 1, 2] * 30f)), testRayD);
		}

		[TestMethod]
		public void TestWithLength() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(Ray.FromStartAndEndPoint(Vector3.ZERO, Vector3.RIGHT * 10f), testRayA.WithLength(10f));
			Assert.AreEqual(new Ray(new Vector3(10f, 0f, 0f), Vector3.LEFT, 200f), testRayB.WithLength(200f));
			Assert.AreEqual(Ray.FromStartAndEndPoint(new Vector3(30f, 30f, 30f), Vector3.ZERO), testRayC.WithLength((float) Math.Sqrt(900f * 3f)));
			Assert.AreEqual(new Ray(Vector3.ONE[0, 1, 2] * -30f, Vector3.ONE[0, 1, 2]), testRayD.WithLength(Single.PositiveInfinity));
		}

		[TestMethod]
		public void TestContainsPoint() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsTrue(testRayA.Contains(new Vector3(1f, 0f, 0f)));
			Assert.IsFalse(testRayA.Contains(new Vector3(-1f, 0f, 0f)));
			Assert.IsTrue(testRayB.Contains(Vector3.ZERO));
			Assert.IsFalse(testRayB.Contains(new Vector3(-11f, 0f, 0f)));
			Assert.IsTrue(testRayC.Contains(Vector3.ZERO));
			Assert.IsFalse(testRayC.Contains(new Vector3(0f, 0f, 0.1f)));
			Assert.IsTrue(testRayD.Contains(new Vector3(-30f, -30f, -30f)));
			Assert.IsFalse(testRayD.Contains(new Vector3(100f, 12f, -4f)));
		}

		[TestMethod]
		public void TestContainsRay() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsTrue(testRayA.Contains(testRayB.WithLength(5f)));
			Assert.IsFalse(testRayA.Contains(testRayC));
			Assert.IsTrue(testRayB.Contains(new Ray(Vector3.ZERO, Vector3.UP, 0f)));
			Assert.IsFalse(testRayB.Contains(new Ray(Vector3.ZERO, Vector3.UP, 0.1f)));
			Assert.IsTrue(testRayC.Contains(Ray.FromStartAndEndPoint(new Vector3(20f, 20f, 20f), Vector3.ZERO)));
			Assert.IsFalse(testRayC.Contains(new Ray(new Vector3(20f, 20f, 20f), new Vector3(-1f, -1f, -1f))));
			Assert.IsTrue(testRayD.Contains(testRayD));
			Assert.IsFalse(testRayD.Contains(testRayC));
		}

		[TestMethod]
		public void TestDistanceFromPoint() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(0f, testRayA.DistanceFrom(new Vector3(new Vector3(30f, 0f, 0f))));
			Assert.AreEqual(100f, testRayB.DistanceFrom(new Vector3(new Vector3(110f, 0f, 0f))));
			Assert.AreEqual((float) Math.Sqrt(300f), testRayC.DistanceFrom(new Vector3(new Vector3(40f, 40f, 40f))));
			Assert.AreEqual(30f, testRayD.DistanceFrom(new Vector3(new Vector3(-60f, -30f, -30f))));
			Assert.AreEqual(24.495f, testRayD.DistanceFrom(new Vector3(new Vector3(0f, -30f, -30f))), 0.001f);
		}

		[TestMethod]
		public void TestDistanceFromRay() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(0f, testRayA.DistanceFrom(testRayB));
			Assert.AreEqual(10f, testRayA.DistanceFrom(new Ray(new Vector3(0f, 10f, 0f), Vector3.RIGHT)));
			Assert.AreEqual(10f, testRayA.DistanceFrom(new Ray(new Vector3(0f, 10f, 0f), Vector3.LEFT)));
			Assert.AreEqual((float) Math.Sqrt(200f), testRayA.DistanceFrom(new Ray(new Vector3(-10f, 10f, 0f), Vector3.LEFT)));
			Assert.AreEqual(0f, testRayB.DistanceFrom(testRayB));
			Assert.AreEqual(0f, testRayC.DistanceFrom(testRayD));
			Assert.AreEqual(0f, testRayC.DistanceFrom(new Ray(new Vector3(-30f, 30f, 30f), new Vector3(1f, -1f, -1f))));
			Assert.AreEqual((float) Math.Sqrt(30000f), testRayD.DistanceFrom(Ray.FromStartAndEndPoint(Vector3.ONE[0, 1, 2] * 130f, Vector3.ONE[0, 1, 2] * 230f)));
			Assert.AreEqual((float) Math.Sqrt(30000f), Ray.FromStartAndEndPoint(Vector3.ONE[0, 1, 2] * 230f, Vector3.ONE[0, 1, 2] * 130f).DistanceFrom(testRayD));

			foreach (Ray testRay1 in testRays) {
				foreach (Ray testRay2 in testRays) {
					Assert.AreEqual(testRay1.DistanceFrom(testRay2), testRay2.DistanceFrom(testRay1));
				}	
			}
		}

		[TestMethod]
		public void TestClosestPointTo() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(Vector3.ZERO, testRayA.ClosestPointTo(testRayB));
			Assert.AreEqual(Vector3.ZERO, testRayA.ClosestPointTo(new Ray(new Vector3(0f, 10f, 0f), Vector3.RIGHT)));
			Assert.AreEqual(Vector3.ZERO, testRayA.ClosestPointTo(new Ray(new Vector3(0f, 10f, 0f), Vector3.LEFT)));
			Assert.AreEqual(Vector3.ZERO, testRayA.ClosestPointTo(new Ray(new Vector3(-10f, 10f, 0f), Vector3.LEFT)));
			Assert.AreEqual(new Vector3(10f, 0f, 0f), testRayA.ClosestPointTo(new Ray(new Vector3(10f, 10f, 0f), Vector3.RIGHT)));
			Assert.AreEqual(new Vector3(-10f, 0f, 0f), testRayB.ClosestPointTo(new Ray(new Vector3(-30f, 0f, 0f), Vector3.RIGHT, 10f)));
			Assert.AreEqual(new Vector3(-10f, 0f, 0f), testRayB.ClosestPointTo(new Ray(new Vector3(-30f, 10f, 0f), Vector3.RIGHT, 10f)));
			Assert.AreEqual(Vector3.ZERO, testRayC.ClosestPointTo(new Ray(new Vector3(-30f, 30f, 30f), new Vector3(1f, -1f, -1f))));
			Assert.AreEqual(Vector3.ONE[0, 1, 2] * 30f, testRayD.ClosestPointTo(Ray.FromStartAndEndPoint(Vector3.ONE[0, 1, 2] * 130f, Vector3.ONE[0, 1, 2] * 230f)));
		}

		[TestMethod]
		public void TestPointProjection() {
			// Define variables and constants


			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(new Vector3(30f, 0f, 0f), testRayA.PointProjection(new Vector3(30f, 0f, 0f)));
			Assert.AreEqual(testRayB.StartPoint, testRayB.PointProjection(new Vector3(110f, 0f, 0f)));
			Assert.AreEqual(testRayC.StartPoint, testRayC.PointProjection(new Vector3(40f, 45f, 50f)));
			Assert.AreEqual(new Vector3(10f, 10f, 10f), testRayD.PointProjection(new Vector3(30f, -30f, 30f)));
		}

		[TestMethod]
		public void TestStrictPointProjection() {
			// Define variables and constants


			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(new Vector3(30f, 0f, 0f), testRayA.StrictPointProjection(new Vector3(30f, 0f, 0f)));
			Assert.AreEqual(null, testRayB.StrictPointProjection(new Vector3(110f, 0f, 0f)));
			Assert.AreEqual(null, testRayC.StrictPointProjection(new Vector3(40f, 45f, 50f)));
			Assert.AreEqual(new Vector3(10f, 10f, 10f), testRayD.StrictPointProjection(new Vector3(30f, -30f, 30f)));
		}

		[TestMethod]
		public void TestIntersectionWithRay() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(Vector3.ZERO, testRayA.IntersectionWith(testRayB));
			Assert.AreEqual(Vector3.ZERO, testRayB.IntersectionWith(testRayC));
			Assert.AreEqual(null, testRayC.IntersectionWith(new Ray(new Vector3(-10f, 0f, 0f), Vector3.DOWN, 10f)));
			Assert.AreEqual(new Vector3(-20f, -20f, -20f), testRayD.IntersectionWith(new Ray(new Vector3(0, -20f, -20f), Vector3.LEFT)));
		}

		[TestMethod]
		public void TestIntersectionWithCuboid() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(new Vector3(10f, 0f, 0f), testRayA.IntersectionWith(new Cuboid(new Vector3(10f, 0f, 0f), 100f, 100f, 100f)));
			Assert.AreEqual(null, testRayA.IntersectionWith(new Cuboid(new Vector3(-10f, 0f, 0f), -100f, 100f, 100f)));
			Assert.AreEqual(new Vector3(-10f, 0f, 0f), testRayB.IntersectionWith(new Cuboid(new Vector3(-10f, 0f, 0f), -100f, 100f, 100f)));
			Assert.AreEqual(null, testRayB.IntersectionWith(new Cuboid(new Vector3(-20f, 0f, 0f), -100f, 100f, 100f)));
			Assert.AreEqual(new Vector3(5f, 5f, 5f), testRayC.IntersectionWith(new Cuboid(Vector3.ZERO, 5f, 5f, 5f)));
			Assert.AreEqual(null, testRayC.IntersectionWith(new Cuboid(new Vector3(0f, 5f, 0f), 0f, 5f, 0f)));
			Assert.AreEqual(new Vector3(0f, 0f, 0f), testRayD.IntersectionWith(new Cuboid(Vector3.ZERO, 5f, 5f, 5f)));
			Assert.AreEqual(null, testRayD.IntersectionWith(new Cuboid(new Vector3(0f, -5f, 0f), 0f, -5f, 0f)));
		}

		[TestMethod]
		public void TestIntersectionWithSphere() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(new Vector3(10f, 0f, 0f), testRayA.IntersectionWith(new Sphere(20f, 0f, 0f, 10f)));
			Assert.AreEqual(null, testRayA.IntersectionWith(new Sphere(-20f, 0f, 0f, 10f)));
			Assert.AreEqual(new Vector3(10f, 0f, 0f), testRayB.IntersectionWith(new Sphere(20f, 0f, 0f, 10f)));
			Assert.AreEqual(null, testRayB.IntersectionWith(new Sphere(-21f, 0f, 0f, 10f)));
			Assert.AreEqual(new Vector3(5.519f, 5.519f, 5.519f), testRayC.IntersectionWith(new Sphere(0f, 10f, 0f, 9f)));
			Assert.AreEqual(null, testRayC.IntersectionWith(new Sphere(0f, 10f, 0f, 6f)));
			Assert.AreEqual(Vector3.ONE[0, 1, 2].WithLength(-6.73f), testRayD.IntersectionWith(new Sphere(0f, 0f, 0f, 6.73f)));
			Assert.AreEqual(null, testRayD.IntersectionWith(new Sphere(Vector3.ONE[0, 1, 2] * 33f, 2f)));
		}

		[TestMethod]
		public void TestIntersectionWithPlane() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(new Vector3(10f, 0f, 0f), testRayA.IntersectionWith(new Plane(Vector3.RIGHT, -10f)));
			Assert.AreEqual(new Vector3(10f, 0f, 0f), testRayA.IntersectionWith(new Plane(Vector3.LEFT, 10f)));
			Assert.AreEqual(new Vector3(-10f, 0f, 0f), testRayB.IntersectionWith(new Plane(Vector3.LEFT, -10f)));
			Assert.AreEqual(null, testRayB.IntersectionWith(new Plane(Vector3.RIGHT, -11f)));
			Assert.AreEqual(new Vector3(11f, 11f, 11f), testRayC.IntersectionWith(new Plane(Vector3.RIGHT, -11f)));
			Assert.AreEqual(new Vector3(-11f, -11f, -11f), testRayC.IntersectionWith(new Plane(Vector3.RIGHT, 11f)));
			Assert.AreEqual(null, testRayD.IntersectionWith(new Plane(Vector3.RIGHT, -31f)));
			Assert.AreEqual(null, testRayD.IntersectionWith(new Plane(Vector3.RIGHT, 31f)));
		}

		[TestMethod]
		public void TestIncidentAngleWith() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			// TODO these are wrong... (the hell was I thinking?)
			Assert.AreEqual(0f, testRayA.IncidentAngleWith(new Plane(Vector3.RIGHT, -10f)));
			Assert.AreEqual(0f, testRayA.IncidentAngleWith(new Plane(Vector3.LEFT, 10f)));
			Assert.AreEqual(0f, testRayA.IncidentAngleWith(new Plane(Vector3.LEFT, -10f)));
			Assert.AreEqual(MathUtils.PI_OVER_TWO, testRayB.IncidentAngleWith(new Plane(Vector3.UP, 0f)));
			Assert.AreEqual(0.9553166f, testRayC.IncidentAngleWith(new Plane(Vector3.LEFT, 0f)));
			Assert.AreEqual(0.9553166f, testRayC.IncidentAngleWith(new Plane(Vector3.LEFT, -500f)));
			Assert.AreEqual(0.9553166f, testRayC.IncidentAngleWith(new Plane(Vector3.LEFT, 500f)));
		}

		[TestMethod]
		public void TestReflect() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(new Ray(new Vector3(10f, 0f, 0f), Vector3.LEFT), testRayA.Reflect(new Plane(Vector3.RIGHT, -10f)));
			Assert.AreEqual(new Ray(new Vector3(10f, 0f, 0f), Vector3.LEFT), testRayA.Reflect(new Plane(Vector3.LEFT, 10f)));
			Assert.AreEqual(null, testRayA.Reflect(new Plane(Vector3.RIGHT, 10f)));
			Assert.AreEqual(null, testRayB.Reflect(new Plane(Vector3.RIGHT, -11f)));
			Assert.AreEqual(null, testRayB.Reflect(new Plane(Vector3.RIGHT, 11f)));
			Assert.AreEqual(new Ray(Vector3.ZERO, new Vector3(1f, -1f, -1f)), testRayC.Reflect(new Plane(Vector3.RIGHT, 0f)));
			Assert.AreEqual(new Ray(Vector3.ZERO, new Vector3(-1f, 1f, 1f)), testRayD.Reflect(new Plane(Vector3.RIGHT, 0f)));
		}
		#endregion
	}
}