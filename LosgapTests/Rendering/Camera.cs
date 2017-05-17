// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 19 12 2014 at 11:18 by Ben Bowen

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap.Rendering {
	[TestClass]
	public class CameraTest {

		[TestInitialize]
		public void SetUp() { }


		#region Tests
		[TestMethod]
		public void TestMove() {
			// Define variables and constants
			Camera testCamera = new Camera();

			// Set up context
			testCamera.Position = Vector3.ONE * 10f;

			// Execute
			testCamera.Move(Vector3.UP * 9f + Vector3.BACKWARD * -0.4f);

			// Assert outcome
			Assert.AreEqual(new Vector3(10f, 19f, 10.4f), testCamera.Position);
		}

		[TestMethod]
		public void TestLookAt() {
			// Define variables and constants
			Camera testCameraA = new Camera();
			Camera testCameraB = new Camera();

			// Set up context
			testCameraA.Position = Vector3.UP * 10f;
			testCameraB.Position = Vector3.DOWN * 10f + Vector3.RIGHT * 10f;


			// Execute
			testCameraA.LookAt(Vector3.ZERO, Vector3.FORWARD);
			testCameraB.LookAt(Vector3.ZERO, Vector3.UP);

			// Assert outcome
			Assert.AreEqual(Vector3.DOWN, testCameraA.Orientation);
			Assert.AreEqual(Vector3.FORWARD, testCameraA.UpDirection);

			Assert.AreEqual(new Vector3(-0.707f, 0.707f, 0f), testCameraB.Orientation);
			Assert.AreEqual(new Vector3(0.707f, 0.707f, 0f), testCameraB.UpDirection);
		}

		[TestMethod]
		public void TestOrient() {
			// Define variables and constants
			Camera testCameraA = new Camera();
			Camera testCameraB = new Camera();

			// Set up context
			testCameraA.Position = Vector3.UP * 10f;
			testCameraB.Position = Vector3.DOWN * 10f + Vector3.RIGHT * 10f;


			// Execute
			testCameraA.Orient(Vector3.DOWN, Vector3.FORWARD);
			testCameraB.Orient(new Vector3(-1f, 1f, 0f), Vector3.UP);

			// Assert outcome
			Assert.AreEqual(Vector3.DOWN, testCameraA.Orientation);
			Assert.AreEqual(Vector3.FORWARD, testCameraA.UpDirection);

			Assert.AreEqual(new Vector3(-0.707f, 0.707f, 0f), testCameraB.Orientation);
			Assert.AreEqual(new Vector3(0.707f, 0.707f, 0f), testCameraB.UpDirection);
		}

		[TestMethod]
		public void TestRotate() {
			// Define variables and constants
			Camera testCameraA = new Camera();
			Camera testCameraB = new Camera();

			// Set up context
			testCameraA.Orient(Vector3.FORWARD, Vector3.UP);
			testCameraB.Orient(Vector3.DOWN, Vector3.FORWARD);

			// Execute
			testCameraA.Rotate(Quaternion.FromAxialRotation(Vector3.UP, MathUtils.THREE_PI_OVER_TWO));

			// Assert outcome
			Assert.AreEqual(Vector3.RIGHT, testCameraA.Orientation);
			Assert.AreEqual(Vector3.UP, testCameraA.UpDirection);
		}

		[TestMethod]
		public void TestDeriveFOV() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(MathUtils.DegToRad(68f), Camera.DeriveVerticalFOV(MathUtils.DegToRad(90f), 1.5f), 0.02f);
			Assert.AreEqual(MathUtils.DegToRad(89f), Camera.DeriveVerticalFOV(MathUtils.DegToRad(105f), 800f / 600f), 0.02f);

			Assert.AreEqual(MathUtils.DegToRad(68f), Camera.DeriveVerticalFOV(Camera.DeriveHorizontalFOV(MathUtils.DegToRad(68f), 1.5f), 1.5f), 0.02f);
			Assert.AreEqual(MathUtils.DegToRad(89f), Camera.DeriveVerticalFOV(Camera.DeriveHorizontalFOV(MathUtils.DegToRad(89f), 800f / 600f), 800f / 600f), 0.02f);
		}

		[TestMethod]
		public unsafe void TestGetRecalculatedViewMatrix() {
			// Define variables and constants
			Camera testCameraA = new Camera();
			Camera testCameraB = new Camera();

			// Set up context
			testCameraA.Position = new Vector3(3f, 4f, 5f);
			testCameraA.Orient(Vector3.FORWARD, Vector3.UP);

			testCameraB.Position = Vector3.ZERO;
			testCameraB.Orient(new Vector3(0f, 0.707f, 0.707f), new Vector3(0.816f, 0.408f, -0.408f));

			// Execute
			Matrix viewMatA = *((Matrix*) testCameraA.GetRecalculatedViewMatrix());
			Matrix viewMatB = *((Matrix*) testCameraB.GetRecalculatedViewMatrix());

			// Assert outcome
			Assert.AreEqual(
				new Matrix(
					1f, 0f, 0f, 0f,
					0f, 1f, 0f, 0f,
					0f, 0f, 1f, 0f,
					-3f, -4f, -5f, 1f
				), viewMatA
			);

			Assert.AreEqual(
				new Matrix(
					0.577f, 0.816f, 0f, 0f,
					-0.577f, 0.408f, 0.707f, 0f,
					0.577f, -0.408f, 0.707f, 0f,
					0f, 0f, 0f, 1f
				), viewMatB
			);
		}

		[TestMethod]
		public void TestGetVerticalFOV() {
			// Define variables and constants
			Camera testCameraA = new Camera();
			Camera testCameraB = new Camera();

			// Set up context


			// Execute
			testCameraA.SetHorizontalFOV(MathUtils.DegToRad(90f));
			testCameraB.SetVerticalFOV(MathUtils.DegToRad(90f));

			// Assert outcome
			Assert.AreEqual(MathUtils.DegToRad(68f), testCameraA.GetVerticalFOV(1.5f), 0.02f);
			Assert.AreEqual(MathUtils.DegToRad(90f), testCameraB.GetVerticalFOV(0f), 0.02f);
		}
		#endregion
	}
}