// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 26 11 2014 at 12:40 by Ben Bowen

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap {
	[TestClass]
	public class CuboidTest {

		[TestInitialize]
		public void SetUp() { }

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
		public void TestCenterPoint() {
			Assert.AreEqual(new Vector3(2.5f, 2.5f, 2.5f), testCuboidA.CenterPoint);
			Assert.AreEqual(new Vector3(-20f, -20f, -20f), testCuboidB.CenterPoint);
			Assert.AreEqual(new Vector3(7.5f, 9f, 10.5f), testCuboidC.CenterPoint);
			Assert.AreEqual(new Vector3(-10f, 5f, 25f), testCuboidD.CenterPoint);
			Assert.AreEqual(new Vector3(7.5f, -3f + 1.25f, 2f), testCuboidE.CenterPoint);
		}
		
		[TestMethod]
		public void TestVolume() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(125f,	testCuboidA.Volume);
			Assert.AreEqual(8000f,	testCuboidB.Volume);
			Assert.AreEqual(504f,	testCuboidC.Volume);
			Assert.AreEqual(20000f,	testCuboidD.Volume);
			Assert.AreEqual(45f,	testCuboidE.Volume);
		}

		[TestMethod]
		public void TestSurfaceArea() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(150f, testCuboidA.SurfaceArea);
			Assert.AreEqual(2400f, testCuboidB.SurfaceArea);
			Assert.AreEqual(382f, testCuboidC.SurfaceArea);
			Assert.AreEqual(5800f, testCuboidD.SurfaceArea);
			Assert.AreEqual(91f, testCuboidE.SurfaceArea);
		}

		[TestMethod]
		public void TestCtor() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(new Cuboid(new Rectangle(0f, 0f, 5f, 5f), 0f, 5f), testCuboidA);
			Assert.AreEqual(new Cuboid(new Rectangle(-30f, -30f, 20f, 20f), -30f, 20f), testCuboidB);
			Assert.AreEqual(new Cuboid(new Rectangle(4f, 5f, 7f, 8f), 6f, 9f), testCuboidC);
			Assert.AreEqual(new Cuboid(new Rectangle(-30f, -20f, 40f, 50f), 20f, 10f), testCuboidD);
			Assert.AreEqual(new Cuboid(new Rectangle(3f, -3f, 9f, 2.5f), 1f, 2f), testCuboidE);
		}

		[TestMethod]
		public void TestGetCorner() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			foreach (Cuboid testCuboid in testCuboids) {
				Assert.AreEqual(testCuboid.FrontBottomLeft.X, testCuboid.GetCornerX(Cuboid.CuboidCorner.FrontTopLeft));
				Assert.AreEqual(testCuboid.FrontBottomLeft.X, testCuboid.GetCornerX(Cuboid.CuboidCorner.FrontBottomLeft));
				Assert.AreEqual(testCuboid.FrontBottomLeft.X, testCuboid.GetCornerX(Cuboid.CuboidCorner.BackTopLeft));
				Assert.AreEqual(testCuboid.FrontBottomLeft.X, testCuboid.GetCornerX(Cuboid.CuboidCorner.BackBottomLeft));
				Assert.AreEqual(testCuboid.FrontBottomLeft.X + testCuboid.Width, testCuboid.GetCornerX(Cuboid.CuboidCorner.FrontTopRight));
				Assert.AreEqual(testCuboid.FrontBottomLeft.X + testCuboid.Width, testCuboid.GetCornerX(Cuboid.CuboidCorner.FrontBottomRight));
				Assert.AreEqual(testCuboid.FrontBottomLeft.X + testCuboid.Width, testCuboid.GetCornerX(Cuboid.CuboidCorner.BackTopRight));
				Assert.AreEqual(testCuboid.FrontBottomLeft.X + testCuboid.Width, testCuboid.GetCornerX(Cuboid.CuboidCorner.BackBottomRight));

				Assert.AreEqual(testCuboid.FrontBottomLeft.Y + testCuboid.Height, testCuboid.GetCornerY(Cuboid.CuboidCorner.FrontTopLeft));
				Assert.AreEqual(testCuboid.FrontBottomLeft.Y + testCuboid.Height, testCuboid.GetCornerY(Cuboid.CuboidCorner.FrontTopRight));
				Assert.AreEqual(testCuboid.FrontBottomLeft.Y + testCuboid.Height, testCuboid.GetCornerY(Cuboid.CuboidCorner.BackTopLeft));
				Assert.AreEqual(testCuboid.FrontBottomLeft.Y + testCuboid.Height, testCuboid.GetCornerY(Cuboid.CuboidCorner.BackTopRight));
				Assert.AreEqual(testCuboid.FrontBottomLeft.Y, testCuboid.GetCornerY(Cuboid.CuboidCorner.FrontBottomLeft));
				Assert.AreEqual(testCuboid.FrontBottomLeft.Y, testCuboid.GetCornerY(Cuboid.CuboidCorner.FrontBottomRight));
				Assert.AreEqual(testCuboid.FrontBottomLeft.Y, testCuboid.GetCornerY(Cuboid.CuboidCorner.BackBottomLeft));
				Assert.AreEqual(testCuboid.FrontBottomLeft.Y, testCuboid.GetCornerY(Cuboid.CuboidCorner.BackBottomRight));

				Assert.AreEqual(testCuboid.FrontBottomLeft.Z, testCuboid.GetCornerZ(Cuboid.CuboidCorner.FrontTopLeft));
				Assert.AreEqual(testCuboid.FrontBottomLeft.Z, testCuboid.GetCornerZ(Cuboid.CuboidCorner.FrontTopRight));
				Assert.AreEqual(testCuboid.FrontBottomLeft.Z, testCuboid.GetCornerZ(Cuboid.CuboidCorner.FrontBottomLeft));
				Assert.AreEqual(testCuboid.FrontBottomLeft.Z, testCuboid.GetCornerZ(Cuboid.CuboidCorner.FrontBottomRight));
				Assert.AreEqual(testCuboid.FrontBottomLeft.Z + testCuboid.Depth, testCuboid.GetCornerZ(Cuboid.CuboidCorner.BackTopLeft));
				Assert.AreEqual(testCuboid.FrontBottomLeft.Z + testCuboid.Depth, testCuboid.GetCornerZ(Cuboid.CuboidCorner.BackTopRight));
				Assert.AreEqual(testCuboid.FrontBottomLeft.Z + testCuboid.Depth, testCuboid.GetCornerZ(Cuboid.CuboidCorner.BackBottomLeft));
				Assert.AreEqual(testCuboid.FrontBottomLeft.Z + testCuboid.Depth, testCuboid.GetCornerZ(Cuboid.CuboidCorner.BackBottomRight));
			}
		}

		[TestMethod]
		public void TestGetSide() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			foreach (Cuboid testCuboid in testCuboids) {
				Assert.AreEqual(
					new Rectangle(testCuboid.FrontBottomLeft.X, testCuboid.FrontBottomLeft.Y, testCuboid.Width, testCuboid.Height),
					testCuboid.GetSide(Cuboid.CuboidSide.Front)
				);
				Assert.AreEqual(
					new Rectangle(testCuboid.FrontBottomLeft.X, testCuboid.FrontBottomLeft.Y, testCuboid.Width, testCuboid.Height),
					testCuboid.GetSide(Cuboid.CuboidSide.Back)
				);
				Assert.AreEqual(
					new Rectangle(testCuboid.FrontBottomLeft.Z, testCuboid.FrontBottomLeft.Y, testCuboid.Depth, testCuboid.Height),
					testCuboid.GetSide(Cuboid.CuboidSide.Left)
				);
				Assert.AreEqual(
					new Rectangle(testCuboid.FrontBottomLeft.Z, testCuboid.FrontBottomLeft.Y, testCuboid.Depth, testCuboid.Height),
					testCuboid.GetSide(Cuboid.CuboidSide.Right)
				);
				Assert.AreEqual(
					new Rectangle(testCuboid.FrontBottomLeft.X, testCuboid.FrontBottomLeft.Z, testCuboid.Width, testCuboid.Depth),
					testCuboid.GetSide(Cuboid.CuboidSide.Top)
				);
				Assert.AreEqual(
					new Rectangle(testCuboid.FrontBottomLeft.X, testCuboid.FrontBottomLeft.Z, testCuboid.Width, testCuboid.Depth),
					testCuboid.GetSide(Cuboid.CuboidSide.Bottom)
				);
			}
		}

		[TestMethod]
		public void TestIntersects() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsTrue(testCuboidA.Intersects(testCuboidA));
			Assert.IsFalse(testCuboidA.Intersects(testCuboidB));
			Assert.IsFalse(testCuboidA.Intersects(testCuboidC));
			Assert.IsFalse(testCuboidA.Intersects(testCuboidD));
			Assert.IsFalse(testCuboidA.Intersects(testCuboidE));

			Assert.IsTrue(testCuboidB.Intersects(testCuboidB));
			Assert.IsFalse(testCuboidB.Intersects(testCuboidC));
			Assert.IsFalse(testCuboidB.Intersects(testCuboidD));
			Assert.IsFalse(testCuboidB.Intersects(testCuboidE));

			Assert.IsTrue(testCuboidC.Intersects(testCuboidC));
			Assert.IsFalse(testCuboidC.Intersects(testCuboidD));
			Assert.IsFalse(testCuboidC.Intersects(testCuboidE));

			Assert.IsTrue(testCuboidD.Intersects(testCuboidD));
			Assert.IsFalse(testCuboidD.Intersects(testCuboidE));

			Assert.IsTrue(testCuboidE.Intersects(testCuboidE));

			// The test set was a bit crap (no intersections), so here's some intersections too
			Assert.IsTrue(
				testCuboidA.Intersects(new Cuboid(2f, 2f, 2f, 5f, 5f, 5f))
			);
			Assert.IsTrue(
				testCuboidB.Intersects(new Cuboid(-20f, -10f, 30f, 1f, 1f, -100f))
			);
			Assert.IsTrue(
				testCuboidC.Intersects(new Cuboid(5f, 6f, 7f, 8f, 9f, 10f))
			);
			Assert.IsTrue(
				testCuboidD.Intersects(new Cuboid(0f, 0f, 25f, 0f, 0f, 0f))
			);
			Assert.IsTrue(
				testCuboidE.Intersects(new Cuboid(12f, -0.5f, 3f, -1f, -1f, -1f))
			);

			foreach (Cuboid testCuboid1 in testCuboids) {
				foreach (Cuboid testCuboid2 in testCuboids) {
					Assert.AreEqual(testCuboid1.Intersects(testCuboid2), testCuboid2.Intersects(testCuboid1));
				}
			}
		}

		[TestMethod]
		public void TestContainsPoint() {
			// Define variables and constants
			Vector3 pointA = new Vector3(0f, 0f, 0f);
			Vector3 pointB = new Vector3(-10f, 10f, 25f);
			Vector3 pointC = new Vector3(5f, 5f, 5f);


			// Set up context


			// Execute


			// Assert outcome
			Assert.IsTrue(testCuboidA.Contains(pointA));
			Assert.IsFalse(testCuboidB.Contains(pointA));
			Assert.IsFalse(testCuboidC.Contains(pointA));
			Assert.IsFalse(testCuboidD.Contains(pointA));
			Assert.IsTrue(testCuboidE.Contains(new Vector3(5f, -1f, 2f)));

			Assert.IsFalse(testCuboidA.Contains(pointB));
			Assert.IsFalse(testCuboidB.Contains(pointB));
			Assert.IsFalse(testCuboidC.Contains(pointB));
			Assert.IsTrue(testCuboidD.Contains(pointB));
			Assert.IsFalse(testCuboidE.Contains(pointB));

			Assert.IsTrue(testCuboidA.Contains(pointC));
			Assert.IsTrue(testCuboidB.Contains(pointC * -3f));
			Assert.IsTrue(testCuboidC.Contains(pointC + new Vector3(0f, 0f, 3f)));
			Assert.IsFalse(testCuboidD.Contains(pointC));
			Assert.IsFalse(testCuboidE.Contains(pointC));
		}

		[TestMethod]
		public void TestContainsRay() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsTrue(testCuboidA.Contains(new Ray(Vector3.ZERO, Vector3.ONE[0, 1, 2], 5f)));
			Assert.IsTrue(testCuboidB.Contains(Ray.FromStartAndEndPoint(new Vector3(-10f, -10f, -10f), new Vector3(-30f, -30f, -30f))));
			Assert.IsTrue(testCuboidC.Contains(Ray.FromStartAndEndPoint(new Vector3(4f, 5f, 6f), new Vector3(7f, 8f, 9f))));
			Assert.IsFalse(testCuboidD.Contains(new Ray(new Vector3(10f, -20f, 30f), Vector3.UP, 51f)));
			Assert.IsFalse(testCuboidE.Contains(new Ray(new Vector3(3f, -3f, 1f), Vector3.UP)));
		}

		[TestMethod]
		public void TestEquals() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(testCuboidA, testCuboidA);
			Assert.IsTrue(testCuboidC.EqualsWithTolerance(new Cuboid(3.5f, 5.5f, 5.5f, 7.5f, 7.5f, 9.5f), 1f));
			Assert.IsFalse(testCuboidC.EqualsExactly(new Cuboid(4f, 5f, 6f, 7f, 8f, 9.001f)));
			Assert.AreNotEqual(testCuboidD, testCuboidE);
		}

		[TestMethod]
		public void TestSphereIntersect() {
			// Define variables and constants
			Sphere testSphereA = new Sphere(12f, 10f, 11f, 35f);
			Sphere testSphereB = new Sphere(-3f, -3f, -3f, 9f);
			Sphere testSphereC = new Sphere(10f, 10f, 10f, 2f);

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsTrue(testCuboidA.Intersects(testSphereA));
			Assert.IsFalse(testCuboidB.Intersects(testSphereA));
			Assert.IsTrue(testCuboidC.Intersects(testSphereA));
			Assert.IsTrue(testCuboidD.Intersects(testSphereA));
			Assert.IsTrue(testCuboidE.Intersects(testSphereA));

			Assert.IsTrue(testCuboidA.Intersects(testSphereB));
			Assert.IsTrue(new Cuboid(-7f, -7f, -7f, -20f, -20f, -20f).Intersects(testSphereB));
			Assert.IsFalse(testCuboidC.Intersects(testSphereB));
			Assert.IsFalse(testCuboidD.Intersects(testSphereB));
			Assert.IsTrue(testCuboidE.Intersects(testSphereB));

			Assert.IsFalse(testCuboidA.Intersects(testSphereC));
			Assert.IsFalse(testCuboidB.Intersects(testSphereC));
			Assert.IsTrue(testCuboidC.Intersects(testSphereC));
			Assert.IsFalse(testCuboidD.Intersects(testSphereC));
			Assert.IsFalse(testCuboidE.Intersects(testSphereC));
		}

		[TestMethod]
		public void TestContainsCuboid() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsTrue(testCuboidA.Contains(new Cuboid(0f, 0f, 0f, 5f, 5f, 5f)));
			Assert.IsTrue(testCuboidB.Contains(new Cuboid(-20f, -10f, -30f, 0f, -10f, 10f)));
			Assert.IsTrue(testCuboidC.Contains(new Cuboid(5f, 6f, 7f, 5f, 6f, 7f)));
			Assert.IsFalse(testCuboidD.Contains(new Cuboid(100f, 100f, 100f, 100f, 100f, 100f)));
			Assert.IsFalse(testCuboidE.Contains(new Cuboid(4f, -2f, 2f, 1f, 1f, 2f)));
		}

		[TestMethod]
		public void TestContainsCone() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsTrue(testCuboidA.Contains(new Cone(2.5f, 2.5f, 2.5f, 2f, 2f)));
			Assert.IsTrue(testCuboidB.Contains(new Cone(-20f, -15f, -25f, 2f, 5f)));
			Assert.IsTrue(testCuboidC.Contains(new Cone(7.5f, 5f, 10.5f, 3.5f, 8f, 2.5f)));
			Assert.IsFalse(testCuboidD.Contains(new Cone(-20f, -20f, 25f, 3f, 30f, 10f)));
			Assert.IsFalse(testCuboidE.Contains(new Cone(6f, -3f, 1.5f, 0.3f, 10f)));
		}

		[TestMethod]
		public void TestContainsCylinder() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsTrue(testCuboidA.Contains(new Cylinder(2.5f, 2.5f, 2.5f, 2.5f, 5f)));
			Assert.IsTrue(testCuboidB.Contains(new Cylinder(-20f, -25f, -15f, 4f, 8f)));
			Assert.IsTrue(testCuboidC.Contains(new Cylinder(7.5f, 9f, 10.5f, 3.5f, 4f)));
			Assert.IsFalse(testCuboidD.Contains(new Cylinder(-20f, 0f, 25f, 25f, 1f)));
			Assert.IsFalse(testCuboidE.Contains(new Cylinder(3f, 0f, 1f, 1f, 1f)));
		}

		[TestMethod]
		public void TestContainsSphere() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsTrue(testCuboidA.Contains(new Sphere(2.5f, 2.5f, 2.5f, 2.5f)));
			Assert.IsTrue(testCuboidB.Contains(new Sphere(-20f, -15f, -25f, 5f)));
			Assert.IsTrue(testCuboidC.Contains(new Sphere(4f, 5f, 6f, 0f)));
			Assert.IsFalse(testCuboidD.Contains(new Sphere(-10f, 5f, 25f, 5.2f)));
			Assert.IsFalse(testCuboidE.Contains(new Sphere(100f, 100f, 100f, 0f)));
		}

		[TestMethod]
		public void TestDistanceFromPoint() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(5f, testCuboidA.DistanceFrom(new Vector3(4f, 3f, 10f)));
			Assert.AreEqual(0f, testCuboidB.DistanceFrom(new Vector3(-20f, -25f, -15f)));
			Assert.AreEqual((float) Math.Sqrt(3f), testCuboidC.DistanceFrom(new Vector3(3f, 4f, 5f)));
			Assert.AreEqual((float) Math.Sqrt(300f), testCuboidD.DistanceFrom(new Vector3(-40f, 40f, 10f)));
			Assert.AreEqual((float) Math.Sqrt(9f + 16f), testCuboidE.DistanceFrom(new Vector3(15f, -7f, 1.5f)));
		}

		[TestMethod]
		public void TestDistanceFromCuboid() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(50f, testCuboidA.DistanceFrom(new Cuboid(2.5f, 4f, 55f, 1f, 1f, 1f)));
			Assert.AreEqual((float) Math.Sqrt(25f + 100f + (15f * 15f)), testCuboidB.DistanceFrom(new Cuboid(-5f, 0f, 5f, 100f, 30f, 0f)));
			Assert.AreEqual((float) Math.Sqrt(3f), testCuboidC.DistanceFrom(new Cuboid(3f, 4f, 5f, -1f, -10f, -100f)));
			Assert.AreEqual(0f, testCuboidD.DistanceFrom(new Cuboid(-40f, -30f, 10f, 10f, 10f, 10f)));
			Assert.AreEqual(0f, testCuboidE.DistanceFrom(new Cuboid(2f, -4f, 0f, 1.4f, 1.3f, 1.2f)));

			foreach (Cuboid testCuboid1 in testCuboids) {
				foreach (Cuboid testCuboid2 in testCuboids) {
					Assert.AreEqual(testCuboid1.DistanceFrom(testCuboid2), testCuboid2.DistanceFrom(testCuboid1));
				}
			}
		}

		[TestMethod]
		public void TestDistanceFromSphere() {
			// Define variables and constants
			Sphere testSphereA = new Sphere(5f, 5f, 2.5f, 10f);
			Sphere testSphereB = new Sphere(-5f, -5f, -2.5f, -10f);
			Sphere testSphereC = new Sphere(0f, 0f, 0f, -10f);
			Sphere testSphereD = new Sphere(20f, 20f, 10f, 0f);
			Sphere testSphereE = new Sphere(19f, 19f, 9.5f, 1f);

			Sphere[] testSpheres = {
				testSphereA, testSphereB, testSphereC, testSphereD, testSphereE
			};

			// Set up context


			// Execute


			// Assert outcome
			foreach (Cuboid testCuboid in testCuboids) {
				foreach (Sphere testSphere in testSpheres) {
					Assert.AreEqual(testCuboid.DistanceFrom(testSphere), testSphere.DistanceFrom(testCuboid));
				}
			}
		}

		[TestMethod]
		public void TestIntersectsCylinder() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsTrue(testCuboidA.Intersects(new Cylinder(2f, 2f, 2f, 2f, 4f)));
			Assert.IsTrue(testCuboidB.Intersects(new Cylinder(-20f, -25f, -15f, 5f, 5f)));
			Assert.IsTrue(testCuboidC.Intersects(new Cylinder(3f, 4f, 5f, 3f, 3f)));
			Assert.IsFalse(testCuboidD.Intersects(new Cylinder(-100f, -100f, -100f, 10f, 10f)));
			Assert.IsFalse(testCuboidE.Intersects(new Cylinder(6f, -1.5f, 4.5f, 1f, 1f)));
		}

		[TestMethod]
		public void TestIntersectionWith() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(testCuboidA, testCuboidA.IntersectionWith(new Cuboid(-5f, -5f, -5f, 15f, 15f, 15f)));
			Assert.AreEqual(new Cuboid(-20f, -15f, -25f, 5f, 5f, 5f), testCuboidB.IntersectionWith(new Cuboid(-20f, -15f, -25f, 5f, 5f, 5f)));
			Assert.AreEqual(new Cuboid(6f, 5f, 6f, 5f, 5f, 2f), testCuboidC.IntersectionWith(new Cuboid(6f, 5f, 4f, 6f, 5f, 4f)));
			Assert.AreEqual(new Cuboid(0f, 0f, 0f, 0f, 0f, 0f), testCuboidD.IntersectionWith(new Cuboid(100f, 100f, 100f, 1f, 1f, 1f)));
			Assert.AreEqual(new Cuboid(0f, 0f, 0f, 0f, 0f, 0f), testCuboidE.IntersectionWith(new Cuboid(3f, -3f, 4f, 1f, 1f, 1f)));
		}
		#endregion
	}
}