// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 16 02 2015 at 10:48 by Ben Bowen

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap {
	[TestClass]
	public class TransformTest {

		[TestInitialize]
		public void SetUp() { }


		#region Tests
		[TestMethod]
		public void TestAsMatrix() {
			// Define variables and constants
			Transform a = Transform.DEFAULT_TRANSFORM;
			Transform b = new Transform(Vector3.ONE * 3f, Quaternion.FromAxialRotation(Vector3.UP, MathUtils.PI), Vector3.BACKWARD * 100f);
			Transform c = new Transform().With(rotation: Quaternion.IDENTITY);
			Transform d = new Transform(Vector3.ZERO, Quaternion.FromAxialRotation(Vector3.LEFT, 0f), Vector3.ZERO);

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(Matrix.FromSRT(Vector3.ONE, Quaternion.IDENTITY, Vector3.ZERO), a.AsMatrix);
			Assert.AreEqual(Matrix.FromSRTTransposed(Vector3.ONE, Quaternion.IDENTITY, Vector3.ZERO), a.AsMatrixTransposed);

			Assert.AreEqual(Matrix.FromSRT(Vector3.ONE * 3f, Quaternion.FromAxialRotation(Vector3.UP, MathUtils.PI), Vector3.BACKWARD * 100f), b.AsMatrix);
			Assert.AreEqual(Matrix.FromSRTTransposed(Vector3.ONE * 3f, Quaternion.FromAxialRotation(Vector3.UP, MathUtils.PI), Vector3.BACKWARD * 100f), b.AsMatrixTransposed);

			Assert.AreEqual(Matrix.FromSRT(Vector3.ZERO, Quaternion.IDENTITY, Vector3.ZERO), c.AsMatrix);
			Assert.AreEqual(Matrix.FromSRTTransposed(Vector3.ZERO, Quaternion.IDENTITY, Vector3.ZERO), c.AsMatrixTransposed);

			Assert.AreEqual(Matrix.FromSRT(d.Scale, d.Rotation, d.Translation), d.AsMatrix);
			Assert.AreEqual(Matrix.FromSRTTransposed(d.Scale, d.Rotation, d.Translation), d.AsMatrixTransposed);
		}

		[TestMethod]
		public void TestWith() {
			// Define variables and constants
			Transform a = Transform.DEFAULT_TRANSFORM;
			Transform b = new Transform(Vector3.ONE * 3f, Quaternion.FromAxialRotation(Vector3.UP, MathUtils.PI), Vector3.BACKWARD * 100f);
			Transform c = new Transform();
			Transform d = new Transform(Vector3.ZERO, Quaternion.FromAxialRotation(Vector3.LEFT, 0f), Vector3.ZERO);

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(a, a.With());
			Assert.AreEqual(a, b.With(scale: Vector3.ONE, rotation: Quaternion.IDENTITY, translation: Vector3.ZERO));
			Assert.AreEqual(c, a.With(scale: Vector3.ZERO, rotation: Quaternion.ZERO));
			Assert.AreEqual(d, d.With(scale: Vector3.ZERO));
			Assert.AreEqual(Transform.DEFAULT_TRANSFORM, c.With(scale: Vector3.ONE, rotation: Quaternion.IDENTITY));
		}

		[TestMethod]
		public void TestEquals() {
			// Define variables and constants
			Transform a = Transform.DEFAULT_TRANSFORM;
			Transform b = new Transform(Vector3.ONE * 3f, Quaternion.FromAxialRotation(Vector3.UP, MathUtils.PI), Vector3.BACKWARD * 100f);
			Transform c = new Transform();
			Transform d = new Transform(Vector3.ZERO, Quaternion.FromAxialRotation(Vector3.LEFT, 0f), Vector3.ZERO);

			// Set up context
			float halfErrorMargin = MathUtils.FlopsErrorMargin / 2f;

			// Execute


			// Assert outcome
			Assert.AreEqual(a, Transform.DEFAULT_TRANSFORM);
			Assert.AreEqual(a, Transform.DEFAULT_TRANSFORM.With(scale: new Vector3(Transform.DEFAULT_TRANSFORM.Scale, x: 1f - halfErrorMargin)));
			Assert.AreEqual(c, new Transform(Vector3.ONE * halfErrorMargin, new Quaternion(halfErrorMargin, halfErrorMargin, halfErrorMargin, halfErrorMargin), Vector3.ZERO));
			Assert.AreNotEqual(b, d);
			Assert.AreNotEqual(b, b.With(scale: Vector3.ONE * (3f + halfErrorMargin * 3f)));
		}
		#endregion
	}
}