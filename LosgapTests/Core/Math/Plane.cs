// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 04 12 2014 at 11:24 by Ben Bowen

using System;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap {
	[TestClass]
	public class PlaneTest {

		[TestInitialize]
		public void SetUp() { }

		static readonly Plane testPlaneA = new Plane(Vector3.UP, -10f);
		static readonly Plane testPlaneB = new Plane(Vector3.UP, 10f);
		static readonly Plane testPlaneC = new Plane(Vector3.DOWN, 10f);
		static readonly Plane testPlaneD = new Plane(new Vector3(-13515f, 135f, 0.135f), 45f);

		static readonly Plane[] testPlanes = {
			testPlaneA, testPlaneB, testPlaneC, testPlaneD
		};

		static readonly Cuboid testCuboidA = new Cuboid(0f, 0f, 0f, 5f, 5f, 5f);
		static readonly Cuboid testCuboidB = new Cuboid(-10f, -10f, -10f, -20f, -20f, -20f);
		static readonly Cuboid testCuboidC = new Cuboid(4f, 5f, 6f, 7f, 8f, 9f);
		static readonly Cuboid testCuboidD = new Cuboid(10f, -20f, 30f, -40f, 50f, -10f);
		static readonly Cuboid testCuboidE = new Cuboid(3f, -3f, 1f, 9f, 2.5f, 2f);

		static readonly Cuboid[] testCuboids = {
			testCuboidA, testCuboidB, testCuboidC, testCuboidD, testCuboidE
		};

		#region Tests
		[TestMethod]
		public void TestFlipped() {
			Assert.AreEqual(testPlaneA, -testPlaneC);
			Assert.AreEqual(new Plane(Vector3.DOWN, -10f), testPlaneB.Flipped);
			Assert.AreEqual(new Plane(-testPlaneD.Normal, -45f), -testPlaneD);
		}

		[TestMethod]
		public void TestFromPoints() {
			Assert.AreEqual(testPlaneA, Plane.FromPoints(new Vector3(0f, 10f, 0f), new Vector3(0f, 10f, 20f), new Vector3(20f, 10f, -20f)));
			Assert.AreEqual(testPlaneB, Plane.FromPoints(new Vector3(0f, -10f, 0f), new Vector3(0f, -10f, 20f), new Vector3(20f, -10f, -20f)));
			Assert.AreEqual(-testPlaneC, Plane.FromPoints(new Vector3(0f, 10f, 0f), new Vector3(0f, 10f, 20f), new Vector3(20f, 10f, -20f)));

			try {
				Plane.FromPoints(Vector3.ONE, Vector3.ONE * 2f, Vector3.ONE * 4f);
				Assert.Fail();
			}
			catch (ArgumentException) { }
		}

		[TestMethod]
		public void TestCtor() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(testPlaneD.Normal.X, new Vector3(-13515f, 135f, 0.135f).ToUnit().X);
			Assert.AreEqual(testPlaneD.Normal.Y, new Vector3(-13515f, 135f, 0.135f).ToUnit().Y);
			Assert.AreEqual(testPlaneD.Normal.Z, new Vector3(-13515f, 135f, 0.135f).ToUnit().Z);

			Assert.AreEqual(testPlaneA, new Plane(Vector3.UP, new Vector3(5f, 10f, 100f)));
			Assert.AreEqual(testPlaneB, new Plane(Vector3.UP, new Vector3(-5f, -10f, 0f)));
		}

		[TestMethod]
		public void TestCentrePoint() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(new Vector3(0f, 10f, 0f), testPlaneA.CentrePoint);
			Assert.AreEqual(new Vector3(0f, -10f, 0f), testPlaneB.CentrePoint);
			Assert.AreEqual(new Vector3(0f, 10f, 0f), testPlaneC.CentrePoint);
			Assert.AreEqual(new Vector3(-13515f, 135f, 0.135f).WithLength(-45f), testPlaneD.CentrePoint);
		}

		[TestMethod]
		public void TestDistanceFromCuboid() {
			// Define variables and constants


			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(10f, testCuboidA.DistanceFrom(new Plane(Vector3.RIGHT, 10f)));
			Assert.AreEqual((float) Math.Sqrt(200f), testCuboidB.DistanceFrom(new Plane(Vector3.LEFT + Vector3.DOWN, 0f)));
			Assert.AreEqual(105f, testCuboidC.DistanceFrom(new Plane(Vector3.DOWN, -100f)));
			Assert.AreEqual(0f, testCuboidD.DistanceFrom(new Plane(Vector3.FORWARD, -20f)));
			Assert.AreEqual(0f, testCuboidE.DistanceFrom(new Plane(Vector3.LEFT, 12f)));
		}

		[TestMethod]
		public void TestLocationOf() {
			// Define variables and constants


			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(Plane.PointPlaneRelationship.PointInFrontOfPlane, testPlaneA.LocationOf(new Vector3(0f, 11f, 0f)));
			Assert.AreEqual(Plane.PointPlaneRelationship.PointInFrontOfPlane, testPlaneB.LocationOf(new Vector3(0f, 11f, 0f)));
			Assert.AreEqual(Plane.PointPlaneRelationship.PointBehindPlane, testPlaneC.LocationOf(new Vector3(0f, 11f, 0f)));
			Assert.AreEqual(Plane.PointPlaneRelationship.PointOnPlane, testPlaneD.LocationOf(new Vector3(-13515f, 135f, 0.135f).WithLength(-45f)));
		}

		[TestMethod]
		public void TestContainsRay() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsTrue(testPlaneA.Contains(new Ray(new Vector3(0, 10, 0), Vector3.RIGHT)));
			Assert.IsTrue(testPlaneB.Contains(new Ray(new Vector3(0, -10, 0), Vector3.BACKWARD, 10f)));
			Assert.IsFalse(testPlaneC.Contains(new Ray(new Vector3(0, 11, 0), Vector3.FORWARD)));
			Assert.IsFalse(testPlaneD.Contains(new Ray(new Vector3(-13515f, 135f, 0.135f).WithLength(-45f), Vector3.UP)));
		}

		[TestMethod]
		public void TestDistanceFromPoint() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(1f, testPlaneA.DistanceFrom(new Vector3(0f, 11f, 0f)));
			Assert.AreEqual(21f, testPlaneB.DistanceFrom(new Vector3(0f, 11f, 0f)));
			Assert.AreEqual(0f, testPlaneC.DistanceFrom(new Vector3(-100000f, 10f, 413135f)));
			Assert.AreEqual(33f, testPlaneD.DistanceFrom(new Vector3(-13515f, 135f, 0.135f).WithLength(-45f) + new Vector3(-13515f, 135f, 0.135f).WithLength(-33f)), 0.01f);
		}

		[TestMethod]
		public void TestDistanceFromPlane() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(20f, testPlaneA.DistanceFrom(testPlaneB));
			Assert.AreEqual(0f, testPlaneA.DistanceFrom(testPlaneC));
			Assert.AreEqual(0f, testPlaneA.DistanceFrom(testPlaneD));

			foreach (Plane testPlane1 in testPlanes) {
				foreach (Plane testPlane2 in testPlanes) {
					Assert.AreEqual(testPlane1.DistanceFrom(testPlane2), testPlane2.DistanceFrom(testPlane1));
				}
			}
		}

		[TestMethod]
		public void TestDistanceFromSphere() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(15f, testPlaneA.DistanceFrom(new Sphere(30f, 30f, 30f, 5f)));
			Assert.AreEqual(35f, testPlaneB.DistanceFrom(new Sphere(12501051f, 30f, -0.135135f, 5f)));
			Assert.AreEqual(0f, testPlaneC.DistanceFrom(new Sphere(0f, 30f, 0f, 20f)));
			Assert.AreEqual(0f, testPlaneD.DistanceFrom(new Sphere(new Vector3(-13515f, 135f, 0.135f).WithLength(-42f), 4f)));
		}

		[TestMethod]
		public void TestIntersectsSphere() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsFalse(testPlaneA.Intersects(new Sphere(30f, 30f, 30f, 5f)));
			Assert.IsFalse(testPlaneB.Intersects(new Sphere(12501051f, 30f, -0.135135f, 5f)));
			Assert.IsTrue(testPlaneC.Intersects(new Sphere(0f, 30f, 0f, 20f)));
			Assert.IsTrue(testPlaneD.Intersects(new Sphere(new Vector3(-13515f, 135f, 0.135f).WithLength(-42f), 4f)));
		}

		[TestMethod]
		public void TestIntersectsCuboid() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsFalse(testCuboidA.Intersects(new Plane(Vector3.RIGHT, 10f)));
			Assert.IsFalse(testCuboidB.Intersects(new Plane(Vector3.LEFT + Vector3.DOWN, 0f)));
			Assert.IsFalse(testCuboidC.Intersects(new Plane(Vector3.DOWN, -100f)));
			Assert.IsTrue(testCuboidD.Intersects(new Plane(Vector3.FORWARD, -20f)));
			Assert.IsTrue(testCuboidE.Intersects(new Plane(Vector3.LEFT, 12f)));
		}

		[TestMethod]
		public void TestIntersectsPlane() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsTrue(testPlaneA.Intersects(new Plane(Vector3.RIGHT, 1f)));
			Assert.IsTrue(testPlaneB.Intersects(new Plane(Vector3.RIGHT, 1f)));
			Assert.IsFalse(testPlaneC.Intersects(new Plane(Vector3.UP, 1f)));
			Assert.IsFalse(testPlaneD.Intersects(new Plane(new Vector3(-13515f, 135f, 0.135f).WithLength(135135135f), -15161f)));
		}

		[TestMethod]
		public void TestPointProjection() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(new Vector3(5f, 10f, -45f), testPlaneA.PointProjection(new Vector3(5f, -300f, -45f)));
			Assert.AreEqual(new Vector3(5f, -10f, -45f), testPlaneB.PointProjection(new Vector3(5f, -300f, -45f)));
			Assert.AreEqual(new Vector3(5f, 10f, -45f), testPlaneC.PointProjection(new Vector3(5f, -300f, -45f)));
			Assert.IsTrue(new Vector3(45.016f, -0.450f, 0.000f).EqualsWithTolerance(testPlaneD.PointProjection(new Vector3(-13515f, 135f, 0.135f).WithLength(-99999f)), 0.1d));
		}

		[TestMethod]
		public void TestRayProjection() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(
				Ray.FromStartAndEndPoint(new Vector3(5f, 10f, -45f), new Vector3(100f, 10f, -90f)),
				testPlaneA.RayProjection(Ray.FromStartAndEndPoint(new Vector3(5f, 450f, -45f), new Vector3(100f, -200f, -90f)))
			);
			Assert.AreEqual(
				Ray.FromStartAndEndPoint(new Vector3(5f, -10f, -45f), new Vector3(100f, -10f, -90f)),
				testPlaneB.RayProjection(Ray.FromStartAndEndPoint(new Vector3(5f, 450f, -45f), new Vector3(100f, -200f, -90f)))
			);
			Assert.AreEqual(
				Ray.FromStartAndEndPoint(new Vector3(5f, 10f, -45f), new Vector3(100f, 10f, -90f)),
				testPlaneC.RayProjection(Ray.FromStartAndEndPoint(new Vector3(5f, 450f, -45f), new Vector3(100f, -200f, -90f)))
			);
			Assert.AreEqual(
				0f,
				new Vector3(testPlaneD.Normal.X, testPlaneD.Normal.Y, testPlaneD.Normal.Z).Dot(testPlaneD.RayProjection(new Ray(new Vector3(-13515f, 135f, 0.135f).WithLength(-45f), Vector3.RIGHT)).Orientation),
				0.001f
			);
		}

		[TestMethod]
		public void TestIntersectionWithPlane() {
			// Define variables and constants
			Vector3 outLineOrientation;
			Vector3 outPointOnLine;

			// Set up context


			// Execute


			// Assert outcome
			testPlaneA.IntersectionWith(new Plane(Vector3.RIGHT, -10f), out outLineOrientation, out outPointOnLine);
			Assert.AreEqual(new Vector3(0f, 0f, 1f), new Vector3(outLineOrientation, z: Math.Abs(outLineOrientation.Z)));
			Assert.AreEqual(new Vector3(10f, 10f, 0f), outPointOnLine);

			testPlaneB.IntersectionWith(new Plane(Vector3.FORWARD, 10f), out outLineOrientation, out outPointOnLine);
			Assert.AreEqual(new Vector3(1f, 0f, 0f), new Vector3(outLineOrientation, x: Math.Abs(outLineOrientation.X)));
			Assert.AreEqual(new Vector3(0f, -10f, -10f), outPointOnLine);

			new Plane(Vector3.RIGHT, 0f).IntersectionWith(new Plane(Vector3.FORWARD, 10f), out outLineOrientation, out outPointOnLine);
			Assert.AreEqual(new Vector3(0f, 1f, 0f), new Vector3(outLineOrientation, y: Math.Abs(outLineOrientation.Y)));
			Assert.AreEqual(new Vector3(0f, 0f, -10f), outPointOnLine);

			Assert.IsFalse(testPlaneA.IntersectionWith(testPlaneB, out outLineOrientation, out outPointOnLine));
		}
		#endregion
	}
}