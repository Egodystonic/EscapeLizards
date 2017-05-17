// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 20 11 2014 at 15:17 by Ben Bowen

using System;
using System.Collections.Generic;

namespace Ophidian.Losgap {
	public partial struct Matrix {
		/// <summary>
		/// The transpose of this matrix, e.g. a flipped matrix so that each column becomes each row; e.g. RowA = ColumnA, etc.
		/// </summary>
		/// <returns>
		/// new <see cref="Matrix"/>(<see cref="ColumnA"/>, <see cref="ColumnB"/>, <see cref="ColumnC"/>, <see cref="ColumnD"/>)
		/// </returns>
		public Matrix Transpose {
			get {
				return new Matrix(ColumnA, ColumnB, ColumnC, ColumnD);
			}
		}

		/// <summary>
		/// The cofactor matrix for this matrix. Accessing this property will induce an expensive calculation;
		/// so make sure to copy the result if using it multiple times in a method.
		/// </summary>
		/// <returns>The cofactor of this matrix.</returns>
		public Matrix Cofactor {
			get {
				return new Matrix(
					GetMinor(0, 0).GetDeterminant(3), GetMinor(0, 1).GetDeterminant(3) * -1, 
					GetMinor(0, 2).GetDeterminant(3), GetMinor(0, 3).GetDeterminant(3) * -1,
					GetMinor(1, 0).GetDeterminant(3) * -1, GetMinor(1, 1).GetDeterminant(3), 
					GetMinor(1, 2).GetDeterminant(3) * -1, GetMinor(1, 3).GetDeterminant(3),
					GetMinor(2, 0).GetDeterminant(3), GetMinor(2, 1).GetDeterminant(3) * -1, 
					GetMinor(2, 2).GetDeterminant(3), GetMinor(2, 3).GetDeterminant(3) * -1,
					GetMinor(3, 0).GetDeterminant(3) * -1, GetMinor(3, 1).GetDeterminant(3), 
					GetMinor(3, 2).GetDeterminant(3) * -1, GetMinor(3, 3).GetDeterminant(3)
				);
			}
		}

		/// <summary>
		/// The <see cref="Transpose"/> of the <see cref="Cofactor"/> of this matrix.
		/// Accessing this property will induce an expensive calculation;
		/// so make sure to copy the result if using it multiple times in a method.
		/// </summary>
		/// <returns>The adjoint of this matrix.</returns>
		public Matrix Adjoint {
			get {
				return Cofactor.Transpose;
			}
		}

		/// <summary>
		/// True if the <see cref="GetDeterminant">determinant</see> of this matrix is not 0f.
		/// </summary>
		/// <returns>True if this matrix can be <see cref="Inverse">inverted</see>.</returns>
		public bool HasInverse {
			get {
				// ReSharper disable once CompareOfFloatsByEqualityOperator
				return GetDeterminant() != 0f;
			}
		}

		/// <summary>
		/// The inverse of this Matrix. Not all matrices have an inverse, see: <see cref="HasInverse"/>.
		/// </summary>
		/// <returns>Returns the <see cref="Adjoint"/> divided by the <see cref="GetDeterminant">determinant</see>.</returns>
		public Matrix Inverse {
			get {
				Assure.True(HasInverse, "Matrix inverse function called on matrix with no inverse!");
				if (IsOrthogonal) return Transpose;
				else return Adjoint / GetDeterminant();
			}
		}

		/// <summary>
		/// Multiplies the two matrices. Remember that Matrix multiplication is not commutative.
		/// </summary>
		/// <param name="matA">The first matrix.</param>
		/// <param name="matB">The second matrix.</param>
		/// <param name="resultantDimension">The number of rows and columns in <paramref name="matA"/> and 
		/// <paramref name="matB"/> that should be used in the multiplication calculation. The resultant matrix will
		/// also have this many rows and columns filled in.</param>
		/// <returns><paramref name="matA"/> x <paramref name="matB"/></returns>
		public static Matrix Multiply(Matrix matA, Matrix matB, int resultantDimension = 4) {
			Assure.BetweenOrEqualTo(resultantDimension, 1, 4, "Resultant dimension must be between 1 and 4.");

			switch (resultantDimension) {
				case 1:
					return new Matrix(r0C0: Vector4.Dot(matA.RowA, matB.ColumnA));
				case 2:
					return new Matrix(
						r0C0: Vector4.Dot(matA.RowA, matB.ColumnA),
						r0C1: Vector4.Dot(matA.RowA, matB.ColumnB),
						r1C0: Vector4.Dot(matA.RowB, matB.ColumnA),
						r1C1: Vector4.Dot(matA.RowB, matB.ColumnB)
					);
				case 3:
					Vector4 matBColA = matB.ColumnA;
					Vector4 matBColB = matB.ColumnB;
					Vector4 matBColC = matB.ColumnC;
					return new Matrix(
						r0C0: Vector4.Dot(matA.RowA, matBColA),
						r0C1: Vector4.Dot(matA.RowA, matBColB),
						r0C2: Vector4.Dot(matA.RowA, matBColC),
						r1C0: Vector4.Dot(matA.RowB, matBColA),
						r1C1: Vector4.Dot(matA.RowB, matBColB),
						r1C2: Vector4.Dot(matA.RowB, matBColC),
						r2C0: Vector4.Dot(matA.RowC, matBColA),
						r2C1: Vector4.Dot(matA.RowC, matBColB),
						r2C2: Vector4.Dot(matA.RowC, matBColC)
					);
				case 4:
					return matA * matB;
				default:
					Logger.Warn("Matrix.Multiply called with resultantDimension of: " + resultantDimension + "!");
					return ZERO;
			}
		}

		/// <summary>
		/// Whether or not this Matrix is orthogonal; i.e. each row is mutually orthonormal. If true, the <see cref="Inverse"/>
		/// of the matrix is equal to its <see cref="Transpose"/>, which provides a substantially faster calculation.
		/// </summary>
		public bool IsOrthogonal {
			get {
				return RowA.IsUnit && RowB.IsUnit && RowC.IsUnit && RowD.IsUnit
					&& Math.Abs(Vector4.Dot(RowA, RowB)) < MathUtils.FlopsErrorMargin
					&& Math.Abs(Vector4.Dot(RowB, RowC)) < MathUtils.FlopsErrorMargin
					&& Math.Abs(Vector4.Dot(RowC, RowD)) < MathUtils.FlopsErrorMargin
					&& Math.Abs(Vector4.Dot(RowD, RowA)) < MathUtils.FlopsErrorMargin;
			}
		}

		/// <summary>
		/// Returns a new <see cref="Matrix"/> that is the same as this Matrix, except with the row indexed by
		/// <paramref name="row"/> and the column indexed by <paramref name="column"/> removed.
		/// </summary>
		/// <remarks>
		/// The three remaining rows and columns will be 'squeezed up' in to a 3x3 matrix, and the fourth row and fourth
		/// column will be filled with zeroes. An example follows:
		/// <code>
		/// Matrix matrix = new Matrix(
		///		00f, 01f, 02f, 03f,
		///		10f, 11f, 12f, 13f,
		///		20f, 21f, 22f, 23f,
		///		30f, 31f, 32f, 33f
		/// );
		/// 
		/// Matrix minor22 = matrix.GetMinor(2, 2);
		/// 
		/// // minor22:
		/// // [00f, 01f, 03f, 00f]
		/// // [10f, 11f, 13f, 00f]
		/// // [30f, 31f, 33f, 00f]
		/// // [00f, 00f, 00f, 00f]
		/// </code>
		/// </remarks>
		/// <param name="row">The row index to remove, from 0 to 3.</param>
		/// <param name="column">The column index to remove, from 0 to 3.</param>
		/// <exception cref="ArgumentException">Thrown if <paramref name="row"/> or <paramref name="column"/>
		/// are &lt; 0 or &gt; 3.</exception>
		/// <returns>The requested matrix minor.</returns>
		public Matrix GetMinor(int row, int column) {
			Assure.BetweenOrEqualTo(row, 0, 3, "Row index must be between 0 and 3.");
			Assure.BetweenOrEqualTo(column, 0, 3, "Column index must be between 0 and 3.");

			switch (row) {
				case 0:
					switch (column) {
						case 0: // Row 0 Column 0
							return new Matrix(RowB[1, 2, 3], RowC[1, 2, 3], RowD[1, 2, 3], Vector4.ZERO);
						case 1: // Row 0 Column 1
							return new Matrix(RowB[0, 2, 3], RowC[0, 2, 3], RowD[0, 2, 3], Vector4.ZERO);
						case 2: // Row 0 Column 2
							return new Matrix(RowB[0, 1, 3], RowC[0, 1, 3], RowD[0, 1, 3], Vector4.ZERO);
						case 3: // Row 0 Column 3
							return new Matrix(RowB[0, 1, 2], RowC[0, 1, 2], RowD[0, 1, 2], Vector4.ZERO);
					}
					break;
				case 1:
					switch (column) {
						case 0: // Row 1 Column 0
							return new Matrix(RowA[1, 2, 3], RowC[1, 2, 3], RowD[1, 2, 3], Vector4.ZERO);
						case 1: // Row 1 Column 1
							return new Matrix(RowA[0, 2, 3], RowC[0, 2, 3], RowD[0, 2, 3], Vector4.ZERO);
						case 2: // Row 1 Column 2
							return new Matrix(RowA[0, 1, 3], RowC[0, 1, 3], RowD[0, 1, 3], Vector4.ZERO);
						case 3: // Row 1 Column 3
							return new Matrix(RowA[0, 1, 2], RowC[0, 1, 2], RowD[0, 1, 2], Vector4.ZERO);
					}
					break;
				case 2:
					switch (column) {
						case 0: // Row 2 Column 0
							return new Matrix(RowA[1, 2, 3], RowB[1, 2, 3], RowD[1, 2, 3], Vector4.ZERO);
						case 1: // Row 2 Column 1
							return new Matrix(RowA[0, 2, 3], RowB[0, 2, 3], RowD[0, 2, 3], Vector4.ZERO);
						case 2: // Row 2 Column 2
							return new Matrix(RowA[0, 1, 3], RowB[0, 1, 3], RowD[0, 1, 3], Vector4.ZERO);
						case 3: // Row 2 Column 3
							return new Matrix(RowA[0, 1, 2], RowB[0, 1, 2], RowD[0, 1, 2], Vector4.ZERO);
					}
					break;
				case 3:
					switch (column) {
						case 0: // Row 3 Column 0
							return new Matrix(RowA[1, 2, 3], RowB[1, 2, 3], RowC[1, 2, 3], Vector4.ZERO);
						case 1: // Row 3 Column 1
							return new Matrix(RowA[0, 2, 3], RowB[0, 2, 3], RowC[0, 2, 3], Vector4.ZERO);
						case 2: // Row 3 Column 2
							return new Matrix(RowA[0, 1, 3], RowB[0, 1, 3], RowC[0, 1, 3], Vector4.ZERO);
						case 3: // Row 3 Column 3
							return new Matrix(RowA[0, 1, 2], RowB[0, 1, 2], RowC[0, 1, 2], Vector4.ZERO);
					}
					break;
			}

			throw new ArgumentException("Matrix minor requested with row: " + row + ", column: " + column + "!");
		}

		

		/// <summary>
		/// Returns the determinant of this Matrix, used to calculate the <see cref="Cofactor"/> and <see cref="Inverse"/>.
		/// </summary>
		/// <param name="dimension">The number of rows and columns in this matrix to use in the determinant calculation.</param>
		/// <returns>The determinant of this matrix.</returns>
		public float GetDeterminant(int dimension = 4) {
			Assure.BetweenOrEqualTo(dimension, 1, 4, "Input dimension must be between 1 and 4.");

			switch (dimension) {
				case 1: return RowA.X;
				case 2:
					return RowA.X * RowB.Y - RowA.Y * RowB.X;
				case 3:
					return RowA.X * (RowB.Y * RowC.Z - RowB.Z * RowC.Y)
						- RowA.Y * (RowB.X * RowC.Z - RowB.Z * RowC.X)
						+ RowA.Z * (RowB.X * RowC.Y - RowB.Y * RowC.X);
				default:
					return
						RowA.X * GetMinor(0, 0).GetDeterminant(3)
						- RowA.Y * GetMinor(0, 1).GetDeterminant(3)
						+ RowA.Z * GetMinor(0, 2).GetDeterminant(3)
						- RowA.W * GetMinor(0, 3).GetDeterminant(3);
			}
		}

		public Vector2 Transform(Vector2 input) {
			return (Vector2) Transform(new Vector4(input.X, input.Y, 0f, 1f));
		}

		public Vector3 Transform(Vector3 input) {
			return (Vector3) Transform(new Vector4(input.X, input.Y, input.Z, 1f));
		}

		public unsafe Vector4 Transform(Vector4 input) {
			return new Vector4(
				RowA.X * input.X + RowB.X * input.Y + RowC.X * input.Z + RowD.X * input.W,
				RowA.Y * input.X + RowB.Y * input.Y + RowC.Y * input.Z + RowD.Y * input.W,
				RowA.Z * input.X + RowB.Z * input.Y + RowC.Z * input.Z + RowD.Z * input.W,
				RowA.W * input.X + RowB.W * input.Y + RowC.W * input.Z + RowD.W * input.W
			);
		}

		/// <summary>
		/// Adds each component in both operands, returning a new Matrix whose elements are a sum of their respective
		/// elements in <paramref name="lhs"/> and <paramref name="rhs"/>.
		/// </summary>
		/// <param name="lhs">The first operand.</param>
		/// <param name="rhs">The second operand.</param>
		/// <returns><c>new Matrix(lhs.RowA + rhs.RowA, lhs.RowB + rhs.RowB, lhs.RowC + rhs.RowC, lhs.RowD + rhs.RowD)</c></returns>
		public static Matrix operator +(Matrix lhs, Matrix rhs) {
			return new Matrix(lhs.RowA + rhs.RowA, lhs.RowB + rhs.RowB, lhs.RowC + rhs.RowC, lhs.RowD + rhs.RowD);
		}

		/// <summary>
		/// Subtracts each component in <paramref name="rhs"/> from the corresponding component in <paramref name="lhs"/>, 
		/// returning a new Matrix whose elements are each the difference of their respective elements in 
		/// <paramref name="lhs"/> and <paramref name="rhs"/>.
		/// </summary>
		/// <param name="lhs">The first operand.</param>
		/// <param name="rhs">The second operand.</param>
		/// <returns><c>new Matrix(lhs.RowA - rhs.RowA, lhs.RowB - rhs.RowB, lhs.RowC - rhs.RowC, lhs.RowD - rhs.RowD)</c></returns>
		public static Matrix operator -(Matrix lhs, Matrix rhs) {
			return new Matrix(lhs.RowA - rhs.RowA, lhs.RowB - rhs.RowB, lhs.RowC - rhs.RowC, lhs.RowD - rhs.RowD);
		}

		/// <summary>
		/// Multiplies each component in the Matrix operand by the scalar operand, and returns a new Matrix as the result.
		/// </summary>
		/// <param name="lhs">The Matrix operand.</param>
		/// <param name="rhs">The scalar operand.</param>
		/// <returns><c>new Matrix(lhs.RowA * rhs, lhs.RowB * rhs, lhs.RowC * rhs, lhs.RowD * rhs)</c></returns>
		public static Matrix operator *(Matrix lhs, float rhs) {
			return new Matrix(lhs.RowA * rhs, lhs.RowB * rhs, lhs.RowC * rhs, lhs.RowD * rhs);
		}

		/// <summary>
		/// Multiplies each component in the Matrix operand by the scalar operand, and returns a new Matrix as the result.
		/// </summary>
		/// <param name="lhs">The scalar operand.</param>
		/// <param name="rhs">The Matrix operand.</param>
		/// <returns><c>new Matrix(lhs * rhs.RowA, lhs * rhs.RowB, lhs * rhs.RowC, lhs * rhs.RowD)</c></returns>
		public static Matrix operator *(float lhs, Matrix rhs) {
			return new Matrix(lhs * rhs.RowA, lhs * rhs.RowB, lhs * rhs.RowC, lhs * rhs.RowD);
		}

		public static Vector4 operator *(Vector4 lhs, Matrix rhs) {
			return rhs.Transform(lhs);
		}

		public static Vector4 operator *(Matrix lhs, Vector4 rhs) {
			return lhs.Transform(rhs);
		}

		public static Vector3 operator *(Vector3 lhs, Matrix rhs) {
			return rhs.Transform(lhs);
		}

		public static Vector3 operator *(Matrix lhs, Vector3 rhs) {
			return lhs.Transform(rhs);
		}

		public static Vector2 operator *(Vector2 lhs, Matrix rhs) {
			return rhs.Transform(lhs);
		}

		public static Vector2 operator *(Matrix lhs, Vector2 rhs) {
			return lhs.Transform(rhs);
		}

		/// <summary>
		/// Divides each component in the Matrix operand by the scalar operand, and returns a new Matrix as the result.
		/// </summary>
		/// <param name="lhs">The Matrix operand.</param>
		/// <param name="rhs">The scalar operand.</param>
		/// <returns><c>new Matrix(lhs.RowA / rhs, lhs.RowB / rhs, lhs.RowC / rhs, lhs.RowD / rhs)</c></returns>
		public static Matrix operator /(Matrix lhs, float rhs) {
			return new Matrix(lhs.RowA / rhs, lhs.RowB / rhs, lhs.RowC / rhs, lhs.RowD / rhs);
		}

		/// <summary>
		/// Returns the multiplication of <paramref name="lhs"/> by <paramref name="rhs"/>. Matrix multiplication is not
		/// commutative, which means that this will not (most of the time) produce the same result as <c>rhs * lhs</c>!
		/// </summary>
		/// <param name="lhs">The left-hand operand.</param>
		/// <param name="rhs">The right-hand operand.</param>
		/// <returns><paramref name="lhs"/> multiplied by <paramref name="rhs"/>.</returns>
		public static Matrix operator *(Matrix lhs, Matrix rhs) {
			Vector4 rhsColA = rhs.ColumnA;
			Vector4 rhsColB = rhs.ColumnB;
			Vector4 rhsColC = rhs.ColumnC;
			Vector4 rhsColD = rhs.ColumnD;
			return new Matrix(
				Vector4.Dot(lhs.RowA, rhsColA), Vector4.Dot(lhs.RowA, rhsColB), Vector4.Dot(lhs.RowA, rhsColC), Vector4.Dot(lhs.RowA, rhsColD),
				Vector4.Dot(lhs.RowB, rhsColA), Vector4.Dot(lhs.RowB, rhsColB), Vector4.Dot(lhs.RowB, rhsColC), Vector4.Dot(lhs.RowB, rhsColD),
				Vector4.Dot(lhs.RowC, rhsColA), Vector4.Dot(lhs.RowC, rhsColB), Vector4.Dot(lhs.RowC, rhsColC), Vector4.Dot(lhs.RowC, rhsColD),
				Vector4.Dot(lhs.RowD, rhsColA), Vector4.Dot(lhs.RowD, rhsColB), Vector4.Dot(lhs.RowD, rhsColC), Vector4.Dot(lhs.RowD, rhsColD)
			);
		}

		/// <summary>
		/// Returns the multiplication of <paramref name="lhs"/> by the inverse of <paramref name="rhs"/>. Matrix multiplication is not
		/// commutative, which means that this will not (most of the time) produce the same result as <c>rhs / lhs</c>!
		/// </summary>
		/// <param name="lhs">The left-hand operand.</param>
		/// <param name="rhs">The right-hand operand.</param>
		/// <returns><paramref name="lhs"/> multiplied by <paramref name="rhs"/>.<see cref="Inverse"/>.</returns>
		public static Matrix operator /(Matrix lhs, Matrix rhs) {
			return lhs * rhs.Inverse;
		}

		/// <summary>
		/// Returns the <see cref="Inverse"/> of <paramref name="operand"/>.
		/// </summary>
		/// <param name="operand">The operand.</param>
		/// <returns><c>operand.Inverse</c></returns>
		public static Matrix operator ~(Matrix operand) {
			return operand.Inverse;
		}
	}
}