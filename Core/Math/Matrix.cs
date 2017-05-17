// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 20 11 2014 at 13:07 by Ben Bowen

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap {
	/// <summary>
	/// Sixteen floating-point components grouped together that are used to represent geometric transformations.
	/// </summary>
	/// <remarks>
	/// Matrices in LOSGAP are always 4x4, and are row-major.
	/// <para>
	/// Matrix equality comparisons automatically have a built-in floating-point tolerance (<see cref="MathUtils.FlopsErrorMargin"/>).
	/// </para>
	/// </remarks>
	[StructLayout(LayoutKind.Sequential, Pack = XMMATRIX_ALIGNMENT, Size = 16 * sizeof(float))]
	public partial struct Matrix : IToleranceEquatable<Matrix> {
		internal const int XMMATRIX_ALIGNMENT = 16;

		/// <summary>
		/// A Matrix where every element is equal to 0f.
		/// </summary>
		public static readonly Matrix ZERO = new Matrix(Vector4.ZERO, Vector4.ZERO, Vector4.ZERO, Vector4.ZERO);

		/// <summary>
		/// The identity matrix (equivalent to 1 in linear arithmetic).
		/// </summary>
		public static readonly Matrix IDENTITY = new Matrix(
			1f, 0f, 0f, 0f,
			0f, 1f, 0f, 0f,
			0f, 0f, 1f, 0f,
			0f, 0f, 0f, 1f
		);

		/// <summary>
		/// The first row of the matrix, represented as a <see cref="Vector4"/>.
		/// </summary>
		public readonly Vector4 RowA;

		/// <summary>
		/// The second row of the matrix, represented as a <see cref="Vector4"/>.
		/// </summary>
		public readonly Vector4 RowB;

		/// <summary>
		/// The third row of the matrix, represented as a <see cref="Vector4"/>.
		/// </summary>
		public readonly Vector4 RowC;

		/// <summary>
		/// The fourth row of the matrix, represented as a <see cref="Vector4"/>.
		/// </summary>
		public readonly Vector4 RowD;

		/// <summary>
		/// The first column of the matrix, represented as a <see cref="Vector4"/>.
		/// </summary>
		public Vector4 ColumnA {
			get {
				return new Vector4(RowA[0], RowB[0], RowC[0], RowD[0]);
			}
		}

		/// <summary>
		/// The second column of the matrix, represented as a <see cref="Vector4"/>.
		/// </summary>
		public Vector4 ColumnB {
			get {
				return new Vector4(RowA[1], RowB[1], RowC[1], RowD[1]);
			}
		}

		/// <summary>
		/// The third column of the matrix, represented as a <see cref="Vector4"/>.
		/// </summary>
		public Vector4 ColumnC {
			get {
				return new Vector4(RowA[2], RowB[2], RowC[2], RowD[2]);
			}
		}

		/// <summary>
		/// The fourth column of the matrix, represented as a <see cref="Vector4"/>.
		/// </summary>
		public Vector4 ColumnD {
			get {
				return new Vector4(RowA[3], RowB[3], RowC[3], RowD[3]);
			}
		}

		/// <summary>
		/// Builds a new Matrix from four constituent row <see cref="Vector4"/>s.
		/// </summary>
		/// <param name="rowA">The first row of the new matrix, in <see cref="Vector4"/> form.</param>
		/// <param name="rowB">The second row of the new matrix, in <see cref="Vector4"/> form.</param>
		/// <param name="rowC">The third row of the new matrix, in <see cref="Vector4"/> form.</param>
		/// <param name="rowD">The fourth row of the new matrix, in <see cref="Vector4"/> form.</param>
		public Matrix(Vector4 rowA, Vector4 rowB, Vector4 rowC, Vector4 rowD)
			: this() {
			RowA = rowA;
			RowB = rowB;
			RowC = rowC;
			RowD = rowD;
		}

		/// <summary>
		/// Builds a new Matrix by specifying each individual component. Each component is optional, and if left unspecified,
		/// will default to 0f.
		/// </summary>
		/// <param name="r0C0">Value at first row, first column.</param>
		/// <param name="r0C1">Value at first row, second column.</param>
		/// <param name="r0C2">Value at first row, third column.</param>
		/// <param name="r0C3">Value at first row, fourth column.</param>
		/// <param name="r1C0">Value at second row, first column.</param>
		/// <param name="r1C1">Value at second row, second column.</param>
		/// <param name="r1C2">Value at second row, third column.</param>
		/// <param name="r1C3">Value at second row, fourth column.</param>
		/// <param name="r2C0">Value at third row, first column.</param>
		/// <param name="r2C1">Value at third row, second column.</param>
		/// <param name="r2C2">Value at third row, third column.</param>
		/// <param name="r2C3">Value at third row, fourth column.</param>
		/// <param name="r3C0">Value at fourth row, first column.</param>
		/// <param name="r3C1">Value at fourth row, second column.</param>
		/// <param name="r3C2">Value at fourth row, third column.</param>
		/// <param name="r3C3">Value at fourth row, fourth column.</param>
		public Matrix(float r0C0 = 0f, float r0C1 = 0f, float r0C2 = 0f, float r0C3 = 0f,
					  float r1C0 = 0f, float r1C1 = 0f, float r1C2 = 0f, float r1C3 = 0f,
					  float r2C0 = 0f, float r2C1 = 0f, float r2C2 = 0f, float r2C3 = 0f,
					  float r3C0 = 0f, float r3C1 = 0f, float r3C2 = 0f, float r3C3 = 0f) {
			RowA = new Vector4(r0C0, r0C1, r0C2, r0C3);
			RowB = new Vector4(r1C0, r1C1, r1C2, r1C3);
			RowC = new Vector4(r2C0, r2C1, r2C2, r2C3);
			RowD = new Vector4(r3C0, r3C1, r3C2, r3C3);
		}

		/// <summary>
		/// Builds a matrix from the values in the given array. The array slice <see cref="ArraySlice{T}.Length"/> must be
		/// exactly <c>16</c>.
		/// </summary>
		/// <param name="values">The values to build this Matrix from. The values will be interpreted in row-by-row order,
		/// i.e. the first four values will constitute <see cref="RowA"/>, the second four <see cref="RowB"/>, etc.</param>
		public Matrix(ArraySlice<float> values) {
			Assure.Equal(values.Length, 16);
			RowA = new Vector4(values[0], values[1], values[2], values[3]);
			RowB = new Vector4(values[4], values[5], values[6], values[7]);
			RowC = new Vector4(values[8], values[9], values[10], values[11]);
			RowD = new Vector4(values[12], values[13], values[14], values[15]);
		}

		/// <summary>
		/// Creates a new Matrix that represents the chosen scaling factors along all three axes.
		/// </summary>
		/// <param name="scaleX">The amount to scale in the x direction by (1f = 100%).</param>
		/// <param name="scaleY">The amount to scale in the y direction by (1f = 100%).</param>
		/// <param name="scaleZ">The amount to scale in the z direction by (1f = 100%).</param>
		/// <returns>A new Matrix that contains the given scales, no translations, and no rotations.</returns>
		public static Matrix FromScale(float scaleX = 1f, float scaleY = 1f, float scaleZ = 1f) {
			return new Matrix(
				scaleX, 0f, 0f, 0f,
				0f, scaleY, 0f, 0f,
				0f, 0f, scaleZ, 0f,
				0f, 0f, 0f, 1f
				);
		}

		/// <summary>
		/// Creates a new Matrix that represents the chosen scaling factors along all three axes.
		/// </summary>
		/// <param name="scale">A <see cref="Vector4"/> whose <see cref="Vector4.X"/>,
		/// <see cref="Vector4.Y"/>, and <see cref="Vector4.Z"/> components specify the scales along 
		/// the X, Y, and Z axes. The <see cref="Vector4.W"/> component is ignored.</param>
		/// <returns>A new Matrix that contains the given scales, no translations, and no rotations.</returns>
		public static Matrix FromScale(Vector4 scale) {
			return new Matrix(
				scale.X, 0f, 0f, 0f,
				0f, scale.Y, 0f, 0f,
				0f, 0f, scale.Z, 0f,
				0f, 0f, 0f, 1f
				);
		}

		/// <summary>
		/// Creates a new Matrix that represents the chosen rotations around each axis. Rotations are applied in the order: 
		/// Yaw x Pitch x Roll, which is equivalent to <paramref name="rotY"/> * <paramref name="rotX"/> * <paramref name="rotZ"/>.
		/// </summary>
		/// <param name="rotX">The clockwise rotation, in radians, around the forward/backward axis.</param>
		/// <param name="rotY">The clockwise rotation, in radians, around the left/right axis.</param>
		/// <param name="rotZ">The clockwise rotation, in radians, around the up/down axis.</param>
		/// <returns>A single Matrix that is the product of three matrices that represent the three separate given rotations,
		/// with a uniform scale of 1x, and no translations along any axis.</returns>
		public static Matrix FromRotation(float rotX = 0f, float rotY = 0f, float rotZ = 0f) {
			rotX = MathUtils.NormalizeRadians(-rotX, MathUtils.RadianNormalizationRange.PositiveNegativeFullRange);
			rotY = MathUtils.NormalizeRadians(-rotY, MathUtils.RadianNormalizationRange.PositiveNegativeFullRange);
			rotZ = MathUtils.NormalizeRadians(-rotZ, MathUtils.RadianNormalizationRange.PositiveNegativeFullRange);

			float sinX = (float) Math.Sin(rotX);
			float cosX = (float) Math.Cos(rotX);
			float sinY = (float) Math.Sin(rotY);
			float cosY = (float) Math.Cos(rotY);
			float sinZ = (float) Math.Sin(rotZ);
			float cosZ = (float) Math.Cos(rotZ);

			Matrix yaw = new Matrix(r0C0: cosY, r0C2: -sinY, r1C1: 1f, r2C0: sinY, r2C2: cosY, r3C3: 1f);
			Matrix pitch = new Matrix(r0C0: 1f, r1C1: cosX, r1C2: sinX, r2C1: -sinX, r2C2: cosX, r3C3: 1f);
			Matrix roll = new Matrix(r0C0: cosZ, r0C1: sinZ, r1C0: -sinZ, r1C1: cosZ, r2C2: 1f, r3C3: 1f);

			return yaw * pitch * roll;
		}

		/// <summary>
		/// Creates a new Matrix that represents the given rotation <see cref="Quaternion"/> in Cartesian space (as a rotation
		/// around the X, Y, and Z world axes).
		/// </summary>
		/// <param name="rotation">The rotation to represent in Matrix form. Does not need to be unit-length.</param>
		/// <returns>A new Matrix that encapsulates the given <paramref name="rotation"/>, with a uniform scale of 1x, and no
		/// translations.</returns>
		public static Matrix FromRotation(Quaternion rotation) {
			rotation = rotation.ToUnit();
			float x = rotation.X;
			float y = rotation.Y;
			float z = rotation.Z;
			float w = rotation.W;
			float xSq = rotation.X * rotation.X;
			float ySq = rotation.Y * rotation.Y;
			float zSq = rotation.Z * rotation.Z;
			return new Matrix(
				1f - 2f * ySq - 2f * zSq, 2f * x * y + 2f * z * w, 2f * x * z - 2f * y * w, 0f,
				2f * x * y - 2f * z * w, 1f - 2f * xSq - 2f * zSq, 2f * y * z + 2f * x * w, 0f,
				2f * x * z + 2f * y * w, 2f * y * z - 2f * x * w, 1f - 2f * xSq - 2f * ySq, 0f,
				0f, 0f, 0f, 1f
			);
		}

		/// <summary>
		/// Creates a new Matrix that represents the chosen rotations around each axis. Rotations are applied in the order: 
		/// Yaw x Pitch x Roll, which is equivalent to <c>Y * X * Z</c>.
		/// </summary>
		/// <param name="rotation">A <see cref="Vector3"/> whose <see cref="Vector3.X"/>,
		/// <see cref="Vector3.Y"/>, and <see cref="Vector3.Z"/> components specify the clockwise rotations around 
		/// the X, Y, and Z axes.</param>
		/// <returns>A single Matrix that is the product of three matrices that represent the three separate given rotations,
		/// with a uniform scale of 1x, and no translations along any axis.</returns>
		public static Matrix FromRotation(Vector3 rotation) {
			return FromRotation(rotation.X, rotation.Y, rotation.Z);
		}

		/// <summary>
		/// Creates a new Matrix that represents the chosen translation along all three geometric axes.
		/// </summary>
		/// <param name="transX">The amount to translate by in the x direction (1f = 1 metre).</param>
		/// <param name="transY">The amount to translate by in the y direction (1f = 1 metre).</param>
		/// <param name="transZ">The amount to translate by in the z direction (1f = 1 metre).</param>
		/// <returns>A new Matrix that contains the given translation, a uniform scale of 1x, and no rotations.</returns>
		public static Matrix FromTranslation(float transX = 0f, float transY = 0f, float transZ = 0f) {
			return new Matrix(
				1f, 0f, 0f, 0f,
				0f, 1f, 0f, 0f,
				0f, 0f, 1f, 0f,
				transX, transY, transZ, 1f
			);
		}

		/// <summary>
		/// Creates a new Matrix that represents the chosen translation along all three geometric axes.
		/// </summary>
		/// <param name="translation">A <see cref="Vector4"/> whose <see cref="Vector4.X"/>,
		/// <see cref="Vector4.Y"/>, and <see cref="Vector4.Z"/> components specify the translations along 
		/// the X, Y, and Z axes. The <see cref="Vector4.W"/> component is ignored.</param>
		/// <returns>A new Matrix that contains the given translation, a uniform scale of 1x, and no rotations.</returns>
		public static Matrix FromTranslation(Vector4 translation) {
			return new Matrix(
				1f, 0f, 0f, 0f,
				0f, 1f, 0f, 0f,
				0f, 0f, 1f, 0f,
				translation.X, translation.Y, translation.Z, 1f
			);
		}

		/// <summary>
		/// Creates a matrix that is the product of all supplied transformations. The order of multiplication is:
		/// ScaleMatrix * RotationMatrix * TranslationMatrix (commonly known as an SRT transformation).
		/// </summary>
		/// <param name="scale">A <see cref="Vector3"/> whose <see cref="Vector3.X"/>,
		/// <see cref="Vector3.Y"/>, and <see cref="Vector3.Z"/> components specify the scales along 
		/// the X, Y, and Z axes.</param>
		/// <param name="rotation">A <see cref="Quaternion"/> that represents the desired composite rotation around
		/// all three axes. Does not need to be unit-length.</param>
		/// <param name="translation">A <see cref="Vector3"/> whose <see cref="Vector3.X"/>,
		/// <see cref="Vector3.Y"/>, and <see cref="Vector3.Z"/> components specify the translations along 
		/// the X, Y, and Z axes.</param>
		/// <returns>A single matrix that is the product of the three transformations described by the given
		/// scaling, rotation, and translation parameters.</returns>
		public static Matrix FromSRT(
		Vector3 scale,
		Quaternion rotation,
		Vector3 translation
		) {
			rotation = rotation.ToUnit();
			float rotX = rotation.X;
			float rotY = rotation.Y;
			float rotZ = rotation.Z;
			float rotW = rotation.W;
			float rotXSq = rotX * rotX;
			float rotYSq = rotY * rotY;
			float rotZSq = rotZ * rotZ;

			Vector4 rowA = new Vector4(
				1f - 2f * rotYSq - 2f * rotZSq,
				2f * rotX * rotY + 2f * rotZ * rotW,
				2f * rotX * rotZ - 2f * rotY * rotW,
				0f
			) * scale.X;
			Vector4 rowB = new Vector4(
				2f * rotX * rotY - 2f * rotZ * rotW,
				1f - 2f * rotXSq - 2f * rotZSq,
				2f * rotY * rotZ + 2f * rotX * rotW,
				0f
			) * scale.Y;
			Vector4 rowC = new Vector4(
				2f * rotX * rotZ + 2f * rotY * rotW,
				2f * rotY * rotZ - 2f * rotX * rotW,
				1f - 2f * rotXSq - 2f * rotYSq,
				0f
			) * scale.Z;
			Vector4 rowD = new Vector4(translation.X, translation.Y, translation.Z, 1f);

			return new Matrix(rowA, rowB, rowC, rowD);
		}

		/// <summary>
		/// Creates a matrix that is the product of all supplied transformations, and then transpose it. The order of multiplication is:
		/// ScaleMatrix * RotationMatrix * TranslationMatrix (commonly known as an SRT transformation).
		/// </summary>
		/// <param name="scale">A <see cref="Vector3"/> whose <see cref="Vector3.X"/>,
		/// <see cref="Vector3.Y"/>, and <see cref="Vector3.Z"/> components specify the scales along 
		/// the X, Y, and Z axes.</param>
		/// <param name="rotation">A <see cref="Quaternion"/> that represents the desired composite rotation around
		/// all three axes. Does not need to be unit-length.</param>
		/// <param name="translation">A <see cref="Vector3"/> whose <see cref="Vector3.X"/>,
		/// <see cref="Vector3.Y"/>, and <see cref="Vector3.Z"/> components specify the translations along 
		/// the X, Y, and Z axes.</param>
		/// <returns>A single matrix that is the transpose of the product of the three transformations described by the given
		/// scaling, rotation, and translation parameters. This method is faster than using the <see cref="Transpose"/> parameter on a
		/// Matrix returned from <see cref="FromSRT(Ophidian.Losgap.Vector3,Ophidian.Losgap.Quaternion,Ophidian.Losgap.Vector3)"/>.</returns>
		public static Matrix FromSRTTransposed(
		Vector3 scale,
		Quaternion rotation,
		Vector3 translation
		) {
			rotation = rotation.ToUnit();
			float rotX = rotation.X;
			float rotY = rotation.Y;
			float rotZ = rotation.Z;
			float rotW = rotation.W;
			float rotXSq = rotX * rotX;
			float rotYSq = rotY * rotY;
			float rotZSq = rotZ * rotZ;

			Vector4 rowA = new Vector4(
				(1f - 2f * rotYSq - 2f * rotZSq) * scale.X,
				(2f * rotX * rotY - 2f * rotZ * rotW) * scale.Y,
				(2f * rotX * rotZ + 2f * rotY * rotW) * scale.Z,
				translation.X
			);
			Vector4 rowB = new Vector4(
				(2f * rotX * rotY + 2f * rotZ * rotW) * scale.X,
				(1f - 2f * rotXSq - 2f * rotZSq) * scale.Y,
				(2f * rotY * rotZ - 2f * rotX * rotW) * scale.Z,
				translation.Y
			);
			Vector4 rowC = new Vector4(
				(2f * rotX * rotZ - 2f * rotY * rotW) * scale.X,
				(2f * rotY * rotZ + 2f * rotX * rotW) * scale.Y,
				(1f - 2f * rotXSq - 2f * rotYSq) * scale.Z,
				translation.Z
			);
			Vector4 rowD = new Vector4(0f, 0f, 0f, 1f);

			return new Matrix(rowA, rowB, rowC, rowD);
		}

		/// <summary>
		/// Returns a string representation of this Matrix.
		/// </summary>
		/// <returns>
		/// A string representation of this Matrix.
		/// </returns>
		public override string ToString() {
			const int ELEMENT_SPACING = 2;
			const int NUM_DEC_PLACE = 3;
			IEnumerable<float> allValues = new Vector4[] {
				RowA, RowB, RowC, RowD	
			}.SelectMany(row => new float[] { row.X, row.Y, row.Z, row.W });

			int ceilLog10 = (int) Math.Max(Math.Ceiling(Math.Log10(allValues.Max())), 1);
			int padRight = ceilLog10 + ELEMENT_SPACING + NUM_DEC_PLACE + 1;
			if (allValues.Any(value => value < 0f)) ++padRight;

			return "Matrix {" + Environment.NewLine
				+ "\t" + this[0, 0].ToString(NUM_DEC_PLACE).PadRight(padRight) 
				+ this[0, 1].ToString(NUM_DEC_PLACE).PadRight(padRight) 
				+ this[0, 2].ToString(NUM_DEC_PLACE).PadRight(padRight) 
				+ this[0, 3].ToString(NUM_DEC_PLACE).PadRight(padRight) + Environment.NewLine
				+ "\t" + this[1, 0].ToString(NUM_DEC_PLACE).PadRight(padRight)
				+ this[1, 1].ToString(NUM_DEC_PLACE).PadRight(padRight)
				+ this[1, 2].ToString(NUM_DEC_PLACE).PadRight(padRight)
				+ this[1, 3].ToString(NUM_DEC_PLACE).PadRight(padRight) + Environment.NewLine
				+ "\t" + this[2, 0].ToString(NUM_DEC_PLACE).PadRight(padRight)
				+ this[2, 1].ToString(NUM_DEC_PLACE).PadRight(padRight)
				+ this[2, 2].ToString(NUM_DEC_PLACE).PadRight(padRight)
				+ this[2, 3].ToString(NUM_DEC_PLACE).PadRight(padRight) + Environment.NewLine
				+ "\t" + this[3, 0].ToString(NUM_DEC_PLACE).PadRight(padRight)
				+ this[3, 1].ToString(NUM_DEC_PLACE).PadRight(padRight)
				+ this[3, 2].ToString(NUM_DEC_PLACE).PadRight(padRight)
				+ this[3, 3].ToString(NUM_DEC_PLACE).PadRight(padRight) + Environment.NewLine
			+ "}";
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(Matrix other) {
			return EqualsWithTolerance(other, MathUtils.FlopsErrorMargin);
		}

		/// <summary>
		/// Test whether this object is EXACTLY equal to <paramref name="other"/>. No floating-point tolerance will be accounted
		/// for: The two structures must be bitwise identical.
		/// </summary>
		/// <param name="other">The other Matrix to test against.</param>
		/// <returns>True if this object is EXACTLY equal to <paramref name="other"/>.</returns>
		public bool EqualsExactly(Matrix other) {
			return RowA.EqualsExactly(other.RowA)
				&& RowB.EqualsExactly(other.RowB)
				&& RowC.EqualsExactly(other.RowC)
				&& RowD.EqualsExactly(other.RowD);
		}

		/// <summary>
		/// Test whether this object is equal to <paramref name="other"/> within a given <paramref name="tolerance"/>. Even if the
		/// two structures are not identical, this method will return <c>true</c> if they are 'close enough' (e.g. within the tolerance).
		/// </summary>
		/// <param name="other">The other Matrix to test against.</param>
		/// <param name="tolerance">The amount of tolerance to give. For any floating-point component in both objects, if the difference
		/// between them is less than this value, they will be considered equal.</param>
		/// <returns>True if this object is equal to <paramref name="other"/> with the given <paramref name="tolerance"/>.</returns>
		public bool EqualsWithTolerance(Matrix other, double tolerance) {
			Assure.GreaterThanOrEqualTo(tolerance, 0f, "Tolerance must not be negative.");
			return RowA.EqualsWithTolerance(other.RowA, tolerance)
				&& RowB.EqualsWithTolerance(other.RowB, tolerance)
				&& RowC.EqualsWithTolerance(other.RowC, tolerance)
				&& RowD.EqualsWithTolerance(other.RowD, tolerance);
		}

		/// <summary>
		/// Indicates whether this instance and a specified object are equal.
		/// </summary>
		/// <returns>
		/// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false. 
		/// </returns>
		/// <param name="obj">The object to compare with the current instance. </param><filterpriority>2</filterpriority>
		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) {
				return false;
			}
			return obj is Matrix && Equals((Matrix)obj);
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>
		/// A 32-bit signed integer that is the hash code for this instance.
		/// </returns>
		/// <filterpriority>2</filterpriority>
		public override int GetHashCode() {
			unchecked {
				int hashCode = RowA.GetHashCode();
				hashCode = (hashCode * 397) ^ RowB.GetHashCode();
				hashCode = (hashCode * 397) ^ RowC.GetHashCode();
				hashCode = (hashCode * 397) ^ RowD.GetHashCode();
				return hashCode;
			}
		}

		/// <summary>
		/// Gets the <see cref="Vector4"/> representing a row of values in this matrix.
		/// </summary>
		/// <param name="rowIndex">The row to select, where
		/// 0 = <see cref="RowA"/>, 1 = <see cref="RowB"/>, 2 = <see cref="RowC"/> and 3 = <see cref="RowD"/>.</param>
		/// <returns>A <see cref="Vector4"/> representation of the selected row.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="rowIndex"/> is &lt; 0 or &gt; 3.</exception>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		public Vector4 this[int rowIndex] {
			get {
				switch (rowIndex) {
					case 0: return RowA;
					case 1: return RowB;
					case 2: return RowC;
					case 3: return RowD;
					default: throw new ArgumentException("Matrix only defines 4 rows (0:A, 1:B, 2:C, 3:D): " +
																"Can not request row at index " + rowIndex + "!", "rowIndex");
				}
			}
		}

		/// <summary>
		/// Gets the value at the indexed row and column of this matrix.
		/// </summary>
		/// <param name="rowIndex">The row to select, where
		/// 0 = <see cref="RowA"/>, 1 = <see cref="RowB"/>, 2 = <see cref="RowC"/> and 3 = <see cref="RowD"/>.</param>
		/// <param name="columnIndex">The element of the selected row to select, where
		/// 0 = <see cref="Vector4.X"/>, 1 = <see cref="Vector4.Y"/>, 2 = <see cref="Vector4.Z"/> and 3 = <see cref="Vector4.W"/></param>
		/// <returns>The <see cref="float"/> value of the indexed element.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="rowIndex"/> 
		/// or <paramref name="columnIndex"/> is &lt; 0 or &gt; 3.</exception>
		public float this[int rowIndex, int columnIndex] {
			get {
				return this[rowIndex][columnIndex];
			}
		}

		/// <summary>
		/// Indicates whether or not the two operands are equal (within a tolerance defined by <see cref="MathUtils.FlopsErrorMargin"/>).
		/// </summary>
		/// <param name="left">The first operand.</param>
		/// <param name="right">The second operand.</param>
		/// <returns>True if all elements in both Matrices are equal.</returns>
		public static bool operator ==(Matrix left, Matrix right) {
			return left.Equals(right);
		}

		/// <summary>
		/// Indicates whether or not the two operands are equal (within a tolerance defined by <see cref="MathUtils.FlopsErrorMargin"/>).
		/// </summary>
		/// <param name="left">The first operand.</param>
		/// <param name="right">The second operand.</param>
		/// <returns>True if any elements in both Matrices are not equal.</returns>
		public static bool operator !=(Matrix left, Matrix right) {
			return !left.Equals(right);
		}
	}
}