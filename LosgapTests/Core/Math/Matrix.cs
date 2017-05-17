// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 20 11 2014 at 16:11 by Ben Bowen

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap {
	[TestClass]
	public class MatrixTest {

		[TestInitialize]
		public void SetUp() { }


		#region Tests
		[TestMethod]
		public void ShouldCorrectlyImplementOperators() {
			// Define variables and constants
			Matrix zeroToFifteen = new Matrix(
				0f, 1f, 2f, 3f,
				4f, 5f, 6f, 7f,
				8f, 9f, 10f, 11f,
				12f, 13f, 14f, 15f
			);
			Matrix zeroToFifteenPlusIdentity = new Matrix(
				1f, 1f, 2f, 3f,
				4f, 6f, 6f, 7f,
				8f, 9f, 11f, 11f,
				12f, 13f, 14f, 16f
			);
			Matrix zeroToFifteenMinusIdentity = new Matrix(
				-1f, 1f, 2f, 3f,
				4f, 4f, 6f, 7f,
				8f, 9f, 9f, 11f,
				12f, 13f, 14f, 14f
			);
			Matrix zeroToFifteenTimesTwo = new Matrix(
				0f, 2f, 4f, 6f,
				8f, 10f, 12f, 14f,
				16f, 18f, 20f, 22f,
				24f, 26f, 28f, 30f
			);
			Matrix zeroToFifteenOverTwo = new Matrix(
				0f, 0.5f, 1f, 1.5f,
				2f, 2.5f, 3f, 3.5f,
				4f, 4.5f, 5f, 5.5f,
				6f, 6.5f, 7f, 7.5f
			);
			Matrix ztfTimesZtfPlusIdent = new Matrix(
				56f, 63f, 70f, 77f,
				156f, 179f, 202f, 225f,
				256f, 295f, 334f, 373f,
				356f, 411f, 466f, 521f
			);
			Matrix ztfOverZtfPlusIdent = new Matrix(
				8f / 7f, 31f / 49f, 6f / 49f, -19f / 49f,
				4f / 7f, 19f / 49f, 10f / 49f, 1f / 49f,
				0f, 1f / 7f, 2f / 7f, 3f / 7f,
				-4f / 7f, -5f / 49f, 18f / 49f, 41f / 49f
			);

			// Assert outcome
			Assert.AreEqual(zeroToFifteenPlusIdentity, zeroToFifteen + Matrix.IDENTITY);
			Assert.AreEqual(zeroToFifteenMinusIdentity, zeroToFifteen - Matrix.IDENTITY);
			Assert.AreEqual(zeroToFifteenTimesTwo, zeroToFifteen * 2f);
			Assert.AreEqual(zeroToFifteenOverTwo, zeroToFifteen / 2f);

			Assert.AreEqual(ztfTimesZtfPlusIdent, zeroToFifteen * zeroToFifteenPlusIdentity);
			Assert.AreEqual(ztfOverZtfPlusIdent, zeroToFifteen / zeroToFifteenPlusIdentity);

			Assert.AreEqual(zeroToFifteenPlusIdentity.Inverse, ~zeroToFifteenPlusIdentity);
		}

		[TestMethod]
		public void ShouldCorrectlyPerformMatrixMathsOperations() {
			// Define variables and constants
			Matrix zeroToFifteen = new Matrix(
				0f, 1f, 2f, 3f,
				4f, 5f, 6f, 7f,
				8f, 9f, 10f, 11f,
				12f, 13f, 14f, 15f
			);
			Matrix ztfMinor11 = new Matrix(
				0f, 2f, 3f, 0f,
				8f, 10f, 11f, 0f,
				12f, 14f, 15f, 0f,
				0f, 0f, 0f, 0f
			);
			Matrix ztfTransposed = new Matrix(
				0f, 4f, 8f, 12f,
				1f, 5f, 9f, 13f,
				2f, 6f, 10f, 14f,
				3f, 7f, 11f, 15f
			);
			float ztfDet = 0f;
			Matrix ztfCofactor = Matrix.ZERO;

			// Assert outcome
			Assert.AreEqual(ztfMinor11, zeroToFifteen.GetMinor(1, 1));
			Assert.AreEqual(ztfTransposed, zeroToFifteen.Transpose);
			Assert.AreEqual(ztfDet, zeroToFifteen.GetDeterminant());
			Assert.AreEqual(ztfCofactor, zeroToFifteen.Cofactor);
			Assert.AreEqual(ztfCofactor, zeroToFifteen.Cofactor.Adjoint);
			// ReSharper disable once CompareOfFloatsByEqualityOperator
			Assert.AreEqual(Math.Abs(zeroToFifteen.GetDeterminant(4)) != 0f, zeroToFifteen.HasInverse);
		}

		[TestMethod]
		public void TestColumnProps() {
			// Define variables and constants
			Matrix zeroToFifteen = new Matrix(
				0f, 1f, 2f, 3f,
				4f, 5f, 6f, 7f,
				8f, 9f, 10f, 11f,
				12f, 13f, 14f, 15f
			);

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(new Vector4(0f, 4f, 8f, 12f), zeroToFifteen.ColumnA);
			Assert.AreEqual(new Vector4(1f, 5f, 9f, 13f), zeroToFifteen.ColumnB);
			Assert.AreEqual(new Vector4(2f, 6f, 10f, 14f), zeroToFifteen.ColumnC);
			Assert.AreEqual(new Vector4(3f, 7f, 11f, 15f), zeroToFifteen.ColumnD);
		}

		[TestMethod]
		public void TestArraySliceConstructor() {
			// Define variables and constants
			Matrix zeroToFifteen = new Matrix(
				0f, 1f, 2f, 3f,
				4f, 5f, 6f, 7f,
				8f, 9f, 10f, 11f,
				12f, 13f, 14f, 15f
			);

			float[] testArrA = {
				0f, 1f, 2f, 3f,
				4f, 5f, 6f, 7f,
				8f, 9f, 10f, 11f,
				12f, 13f, 14f, 15f
			};

			float[] testArrB = {
				17f, 17f, 17f, 17f,
				0f, 1f, 2f, 3f,
				4f, 5f, 6f, 7f,
				8f, 9f, 10f, 11f,
				12f, 13f, 14f, 15f,
				17f, 17f, 17f, 17f,
			};

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(zeroToFifteen, new Matrix(testArrA));
			Assert.AreEqual(zeroToFifteen, new Matrix(new ArraySlice<float>(testArrB, 4, 16)));
		}

		[TestMethod]
		public void TestTranspose() {
			// Define variables and constants
			Matrix zeroToFifteen = new Matrix(
				0f, 1f, 2f, 3f,
				4f, 5f, 6f, 7f,
				8f, 9f, 10f, 11f,
				12f, 13f, 14f, 15f
			);
			Matrix zeroToFifteenT = new Matrix(
				0f, 4f, 8f, 12f,
				1f, 5f, 9f, 13f,
				2f, 6f, 10f, 14f,
				3f, 7f, 11f, 15f
			);

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(zeroToFifteenT, zeroToFifteen.Transpose);
			Assert.AreEqual(zeroToFifteen, zeroToFifteenT.Transpose);
		}

		[TestMethod]
		public void TestIndexers() {
			// Define variables and constants
			Matrix zeroToFifteen = new Matrix(
				0f, 1f, 2f, 3f,
				4f, 5f, 6f, 7f,
				8f, 9f, 10f, 11f,
				12f, 13f, 14f, 15f
			);

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(zeroToFifteen.RowA, zeroToFifteen[0]);
			Assert.AreEqual(zeroToFifteen.RowB, zeroToFifteen[1]);
			Assert.AreEqual(zeroToFifteen.RowC, zeroToFifteen[2]);
			Assert.AreEqual(zeroToFifteen.RowD, zeroToFifteen[3]);
			for (int r = 0; r < 4; ++r) {
				for (int c = 0; c < 4; ++c) {
					Assert.AreEqual(r * 4f + (float) c, zeroToFifteen[r, c]);
				}
			}
		}

		[TestMethod]
		public void TestEquality() {
			// Define variables and constants
			Matrix zeroToFifteen = new Matrix(
				0f, 1f, 2f, 3f,
				4f, 5f, 6f, 7f,
				8f, 9f, 10f, 11f,
				12f, 13f, 14f, 15f
			);
			Matrix zeroToFifteenSlightlyOff = new Matrix(
				0.0001f, 1f, 2f, 3f,
				4f, 5f, 6.0001f, 7f,
				8f, 9f, 10f, 11f,
				12f, 13f, 14.0001f, 15f
			);

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(zeroToFifteen, zeroToFifteenSlightlyOff);
			Assert.IsFalse(zeroToFifteen.EqualsExactly(zeroToFifteenSlightlyOff));
			Assert.IsTrue(zeroToFifteen == zeroToFifteenSlightlyOff);
		}

		[TestMethod]
		public void TestCofactor() {
			// Define variables and constants
			Matrix matA = new Matrix(
				1, 2, 3, 0,
				4, 5, 6, 0,
				5, 4, 3, 9,
				1, 2, 3, 4
			);
			Matrix matB = Matrix.FromRotation(1f, 2f, -1.5f);

			Matrix matACofactor = new Matrix(
				-63, 126, -63, 0,
				24, -48, 24, 0,
				-12, 24, -12, 0,
				27, -54, 27, 0
			);

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(matACofactor, matA.Cofactor);
			Assert.AreEqual(matB, matB.Cofactor);
		}

		[TestMethod]
		public void TestAdjoint() {
			// Define variables and constants
			Matrix matA = new Matrix(
				1, 2, 3, 0,
				4, 5, 6, 0,
				5, 4, 3, 9,
				1, 2, 3, 4
			);
			Matrix matB = Matrix.FromRotation(1f, 2f, -1.5f);

			Matrix matAAdj = new Matrix(
				-63, 24, -12, 27,
				126, -48, 24, -54,
				-63, 24, -12, 27,
				0, 0, 0, 0
			);

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(matAAdj, matA.Adjoint);
			Assert.AreEqual(matB.Transpose, matB.Adjoint);
		}

		[TestMethod]
		public void TestHasInverse() {
			// Define variables and constants
			Matrix zeroToFifteen = new Matrix(
				0f, 1f, 2f, 3f,
				4f, 5f, 6f, 7f,
				8f, 9f, 10f, 11f,
				12f, 13f, 14f, 15f
			);
			Matrix rotMat = Matrix.FromRotation((float) Math.PI, (float) Math.PI * 2f, (float) Math.PI / 2f);

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsFalse(zeroToFifteen.HasInverse);
			Assert.IsTrue(rotMat.HasInverse);
		}

		[TestMethod]
		public void TestInverse() {
			// Define variables and constants
			Matrix testA = new Matrix(
				4, 8, 9, 7,
				5, 6, 9, 67,
				5, 78, 6, 8,
				5, 89, 6, 5
			);
			Matrix testB = Matrix.FromRotation((float) Math.PI, (float) Math.PI * 2f, (float) Math.PI / 2f);

			Matrix invA = new Matrix(
				-1632, -2292, 45186, -39300,
				-45, 63, -1257, 1230,
				2165, 783, -18347, 15832,
				-165, 231, -795, 696
			) / 11442;

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(invA, testA.Inverse);
			Assert.AreEqual(testB.Transpose, testB.Inverse);
		}

		[TestMethod]
		public void TestIsOrthogonal() {
			// Define variables and constants
			Matrix testA = new Matrix(
				4, 8, 9, 7,
				5, 6, 9, 67,
				5, 78, 6, 8,
				5, 89, 6, 5
			);
			Matrix testB = Matrix.FromRotation((float) Math.PI, (float) Math.PI * 2f, (float) Math.PI / 2f);

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsFalse(testA.IsOrthogonal);
			Assert.IsTrue(testB.IsOrthogonal);
		}

		[TestMethod]
		public void TestGetMinor() {
			// Define variables and constants
			Matrix zeroToFifteen = new Matrix(
				0f, 1f, 2f, 3f,
				4f, 5f, 6f, 7f,
				8f, 9f, 10f, 11f,
				12f, 13f, 14f, 15f
			);

			// Set up context


			// Execute


			// Assert outcome
			for (int row = 0; row < 4; ++row) {
				for (int column = 0; column < 4; ++column) {
					int x, y, z;
					switch (column) {
						case 0: x = 1; y = 2; z = 3; break;
						case 1: x = 0; y = 2; z = 3; break;
						case 2: x = 0; y = 1; z = 3; break;
						default: x = 0; y = 1; z = 2; break;
					}
					Vector4 a, b, c;
					switch (row) {
						case 0: a = zeroToFifteen.RowB; b = zeroToFifteen.RowC; c = zeroToFifteen.RowD; break;
						case 1: a = zeroToFifteen.RowA; b = zeroToFifteen.RowC; c = zeroToFifteen.RowD; break;
						case 2: a = zeroToFifteen.RowA; b = zeroToFifteen.RowB; c = zeroToFifteen.RowD; break;
						default: a = zeroToFifteen.RowA; b = zeroToFifteen.RowB; c = zeroToFifteen.RowC; break;
					}

					Matrix expected = new Matrix(a[x, y, z], b[x, y, z], c[x, y, z], Vector3.ZERO);

					Assert.AreEqual(expected, zeroToFifteen.GetMinor(row, column));
				}
			}
		}

		[TestMethod]
		public void TestDeterminant() {
			// Define variables and constants
			Matrix testA = new Matrix(
				4, 8, 9, 7,
				5, 6, 9, 67,
				5, 78, 6, 8,
				5, 89, 6, 5
			);

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(11442, testA.GetDeterminant());
			Assert.AreEqual(696, testA.GetDeterminant(3));
			Assert.AreEqual(-16, testA.GetDeterminant(2));
			Assert.AreEqual(4, testA.GetDeterminant(1));
		}

		[TestMethod]
		public void TestTranslationMatrix() {
			// Define variables and constants
			const float X_TRANSLATION = 10f;
			const float Y_TRANSLATION = -40f;
			const float Z_TRANSLATION = -99.345f;
			Matrix expected = new Matrix(
				1f, 0f, 0f, 0f,
				0f, 1f, 0f, 0f,
				0f, 0f, 1f, 0f,
				X_TRANSLATION, Y_TRANSLATION, Z_TRANSLATION, 1f
			);

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(expected, Matrix.FromTranslation(X_TRANSLATION, Y_TRANSLATION, Z_TRANSLATION));
			Assert.AreEqual(
				Matrix.FromTranslation(X_TRANSLATION, Y_TRANSLATION, Z_TRANSLATION),
				Matrix.FromTranslation(new Vector3(X_TRANSLATION, Y_TRANSLATION, Z_TRANSLATION))
			);
		}

		[TestMethod]
		public void TestScaleMatrix() {
			// Define variables and constants
			const float X_SCALE = 10f;
			const float Y_SCALE = -40f;
			const float Z_SCALE = -99.345f;
			Matrix expected = new Matrix(
				X_SCALE, 0f, 0f, 0f,
				0f, Y_SCALE, 0f, 0f,
				0f, 0f, Z_SCALE, 0f,
				0f, 0f, 0f, 1f
			);

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(expected, Matrix.FromScale(X_SCALE, Y_SCALE, Z_SCALE));
			Assert.AreEqual(
				Matrix.FromScale(X_SCALE, Y_SCALE, Z_SCALE),
				Matrix.FromScale(new Vector3(X_SCALE, Y_SCALE, Z_SCALE))
			);
		}

		[TestMethod]
		public void TestRotationMatrix() {
			// Define variables and constants
			const float ROT_X = MathUtils.THREE_PI_OVER_TWO;
			const float ROT_Y = MathUtils.PI_OVER_TWO;
			const float ROT_Z = -MathUtils.TWO_PI;
			Matrix expected = new Matrix(
				0f, -1f, 0f, 0f,
				0f, 0f, 1f, 0f,
				-1f, 0f, 0f, 0f,
				0f, 0f, 0f, 1f
			);

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(expected, Matrix.FromRotation(ROT_X, ROT_Y, ROT_Z));
			Assert.AreEqual(Matrix.FromRotation(ROT_X, ROT_Y, ROT_Z), Matrix.FromRotation(new Vector3(ROT_X, ROT_Y, ROT_Z)));
			Assert.AreEqual(Vector3.UP, Matrix.FromRotation(ROT_X, ROT_Y, ROT_Z).Transform(Vector3.LEFT));
		}

		[TestMethod]
		public void TestRotationMatrixWithQuaternions() {
			// Define variables and constants
			Quaternion quatA = new Quaternion(2f, -1f, -3f, 0f);
			Quaternion quatB = new Quaternion(22f, -31f, -43f, 10f);
			Quaternion quatC = new Quaternion(0f, -1f, -0.5f, 1f);
			Quaternion quatD = new Quaternion(-100f, 575f, 990f, 100f);

			Matrix expectedA = new Matrix(
				-0.428f, -0.285f, -0.857f, 0f,
				-0.285f, -0.857f, 0.428f, 0f,
				-0.857f, 0.428f, 0.285f, 0f,
				0f, 0f, 0f, 1f
			);
			Matrix expectedB = new Matrix(
				-0.655f, -0.655f, -0.374f, 0f,
				-0.148f, -0.374f, 0.915f, 0f,
				-0.740f, 0.655f, 0.148f, 0f,
				0f, 0f, 0f, 1f
			);
			Matrix expectedC = new Matrix(
				-0.111f, -0.444f, 0.888f, 0f,
				0.444f, 0.777f, 0.444f, 0f,
				-0.888f, 0.444f, 0.111f, 0f,
				0f, 0f, 0f, 1f
			);
			Matrix expectedD = new Matrix(
				-0.969f, 0.062f, -0.235f, 0f,
				-0.235f, -0.488f, 0.840f, 0f,
				-0.062f, 0.870f, 0.488f, 0f,
				0f, 0f, 0f, 1f
			);

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(expectedA, Matrix.FromRotation(quatA));
			Assert.AreEqual(expectedB, Matrix.FromRotation(quatB));
			Assert.AreEqual(expectedC, Matrix.FromRotation(quatC));
			Assert.AreEqual(expectedD, Matrix.FromRotation(quatD));
		}

		[TestMethod]
		public void TestFromSRT() {
			// Define variables and constants
			const float X_SCALE = 10f;
			const float Y_SCALE = -40f;
			const float Z_SCALE = -99.345f;
			Matrix scaleMat = new Matrix(
				X_SCALE, 0f, 0f, 0f,
				0f, Y_SCALE, 0f, 0f,
				0f, 0f, Z_SCALE, 0f,
				0f, 0f, 0f, 1f
			);

			const float ROT_X = MathUtils.THREE_PI_OVER_TWO;
			const float ROT_Y = MathUtils.PI_OVER_TWO;
			const float ROT_Z = -MathUtils.TWO_PI;
			Matrix rotMat = Quaternion.FromEulerRotations(ROT_X, ROT_Y, ROT_Z).RotMatrix;

			const float X_TRANSLATION = 10f;
			const float Y_TRANSLATION = -40f;
			const float Z_TRANSLATION = -99.345f;
			Matrix transMat = new Matrix(
				1f, 0f, 0f, 0f,
				0f, 1f, 0f, 0f,
				0f, 0f, 1f, 0f,
				X_TRANSLATION, Y_TRANSLATION, Z_TRANSLATION, 1f
			);

			Matrix expected = scaleMat * rotMat * transMat;

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(
				expected,
				Matrix.FromSRT(
					new Vector3(X_SCALE, Y_SCALE, Z_SCALE),
					Quaternion.FromEulerRotations(ROT_X, ROT_Y, ROT_Z),
					new Vector3(X_TRANSLATION, Y_TRANSLATION, Z_TRANSLATION)
				)
			);
		}

		[TestMethod]
		public void TestFromSRTTransposed() {
			// Define variables and constants
			const float X_SCALE = 10f;
			const float Y_SCALE = -40f;
			const float Z_SCALE = -99.345f;
			Matrix scaleMat = new Matrix(
				X_SCALE, 0f, 0f, 0f,
				0f, Y_SCALE, 0f, 0f,
				0f, 0f, Z_SCALE, 0f,
				0f, 0f, 0f, 1f
			);

			const float ROT_X = MathUtils.THREE_PI_OVER_TWO;
			const float ROT_Y = MathUtils.PI_OVER_TWO;
			const float ROT_Z = -MathUtils.TWO_PI;
			Matrix rotMat = Quaternion.FromEulerRotations(ROT_X, ROT_Y, ROT_Z).RotMatrix;

			const float X_TRANSLATION = 10f;
			const float Y_TRANSLATION = -40f;
			const float Z_TRANSLATION = -99.345f;
			Matrix transMat = new Matrix(
				1f, 0f, 0f, 0f,
				0f, 1f, 0f, 0f,
				0f, 0f, 1f, 0f,
				X_TRANSLATION, Y_TRANSLATION, Z_TRANSLATION, 1f
			);

			Matrix expected = (scaleMat * rotMat * transMat).Transpose;

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(
				expected, 
				Matrix.FromSRTTransposed(
					new Vector3(X_SCALE, Y_SCALE, Z_SCALE), 
					Quaternion.FromEulerRotations(ROT_X, ROT_Y, ROT_Z), 
					new Vector3(X_TRANSLATION, Y_TRANSLATION, Z_TRANSLATION)
				)
			);
		}

		[TestMethod]
		public void TestTransform() {
			Matrix scaleX = Matrix.FromScale(scaleX: 2f);
			Matrix scaleYAndZ = Matrix.FromScale(scaleY: 3f, scaleZ: -4f);
			Matrix rotAroundX = Matrix.FromRotation(rotX: MathUtils.PI_OVER_TWO);
			Matrix rotAroundYAndZ = Matrix.FromRotation(rotY: MathUtils.PI, rotZ: MathUtils.THREE_PI_OVER_TWO);
			Matrix translateAlongX = Matrix.FromTranslation(transX: 10f);
			Matrix translateAlongYAndZ = Matrix.FromTranslation(transY: 30f, transZ: -2f);
			Matrix srt = Matrix.FromSRT(
				new Vector3(2f, 4f, 6f),
				Quaternion.FromEulerRotations(MathUtils.PI_OVER_TWO, MathUtils.PI, MathUtils.THREE_PI_OVER_TWO),
				new Vector3(-10f, -20f, -30f)
			);

			Vector3 testVector = new Vector3(10f, -30f, 50f);

			Assert.AreEqual(new Vector3(20f, -30f, 50f), scaleX * testVector);
			Assert.AreEqual(new Vector3(10f, -90f, -200f), testVector * scaleYAndZ);
			Assert.AreEqual(testVector * Quaternion.FromAxialRotation(Vector3.RIGHT, MathUtils.PI_OVER_TWO), testVector * rotAroundX);
			Assert.AreEqual(
				testVector 
					* Quaternion.FromAxialRotation(Vector3.UP, MathUtils.PI)
					* Quaternion.FromAxialRotation(Vector3.FORWARD, MathUtils.THREE_PI_OVER_TWO),
				rotAroundYAndZ * testVector
			);
			Assert.AreEqual(new Vector3(20f, -30f, 50f), testVector * translateAlongX);
			Assert.AreEqual(new Vector3(10f, 0f, 48f), testVector * translateAlongYAndZ);
			Assert.AreEqual(
				testVector.Scale(new Vector3(2f, 4f, 6f))
					* Quaternion.FromEulerRotations(MathUtils.PI_OVER_TWO, MathUtils.PI, MathUtils.THREE_PI_OVER_TWO)
					+ new Vector3(-10f, -20f, -30f), 
				srt * testVector
			);
		}
		#endregion
	}
}