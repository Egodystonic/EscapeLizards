// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 20 11 2014 at 13:10 by Ben Bowen

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Ophidian.Losgap {
	/// <summary>
	/// Four floating-point components that are grouped together and used to represent magnitudes, velocities, and points/co-ordinates.
	/// </summary>
	/// <remarks>
	/// Vectors are used to represent points in geometric space or length + direction combinations from an arbitrary
	/// source.
	/// <para>
	/// Vector equality comparisons automatically have a built-in floating-point tolerance (<see cref="MathUtils.FlopsErrorMargin"/>).
	/// </para>
	/// </remarks>
	/// <seealso cref="Vector3"/>
	/// <seealso cref="Vector2"/>
	[StructLayout(LayoutKind.Explicit, Pack = XMVECTOR_ALIGNMENT, Size = 4 * sizeof(float))]
	public partial struct Vector4 : IToleranceEquatable<Vector4> {
		internal const int XMVECTOR_ALIGNMENT = 16;

		/// <summary>
		/// A vector where all elements are equal to 0f: [0f, 0f, 0f, 0f]
		/// </summary>
		public static readonly Vector4 ZERO = new Vector4(0f, 0f, 0f, 0f);

		/// <summary>
		/// A vector where all elements are equal to 1f: [1f, 1f, 1f, 1f]
		/// </summary>
		public static readonly Vector4 ONE = new Vector4(1f, 1f, 1f, 1f);

		/// <summary>
		/// A vector that represents one unit in the left direction: [-1f, 0f, 0f, 0f]
		/// </summary>
		public static readonly Vector4 LEFT = new Vector4(-1f, 0f, 0f, 0f);

		/// <summary>
		/// A vector that represents one unit in the right direction: [1f, 0f, 0f, 0f]
		/// </summary>
		public static readonly Vector4 RIGHT = new Vector4(1f, 0f, 0f, 0f);

		/// <summary>
		/// A vector that represents one unit in the forward direction: [0f, 0f, 1f, 0f]
		/// </summary>
		public static readonly Vector4 FORWARD = new Vector4(0f, 0f, 1f, 0f);

		/// <summary>
		/// A vector that represents one unit in the backward direction: [0f, 0f, -1f, 0f]
		/// </summary>
		public static readonly Vector4 BACKWARD = new Vector4(0f, 0f, -1f, 0f);

		/// <summary>
		/// A vector that represents one unit in the up direction: [0f, 1f, 0f, 0f]
		/// </summary>
		public static readonly Vector4 UP = new Vector4(0f, 1f, 0f, 0f);

		/// <summary>
		/// A vector that represents one unit in the down direction: [0f, -1f, 0f, 0f]
		/// </summary>
		public static readonly Vector4 DOWN = new Vector4(0f, -1f, 0f, 0f);

		/// <summary>
		/// The first component of this Vector.
		/// </summary>
		[FieldOffset(0)]
		public readonly float X;

		/// <summary>
		/// The second component of this Vector.
		/// </summary>
		[FieldOffset(4)]
		public readonly float Y;

		/// <summary>
		/// The third component of this Vector.
		/// </summary>
		[FieldOffset(8)]
		public readonly float Z;

		/// <summary>
		/// The fourth component of this Vector.
		/// </summary>
		[FieldOffset(12)]
		public readonly float W;

		/// <summary>
		/// Creates a new Vector with the given components.
		/// </summary>
		/// <remarks>
		/// Conventionally, the <see cref="W"/> component should be 1f for points, and 0f for magnitudes.
		/// </remarks>
		/// <param name="x">The first element of this vector.</param>
		/// <param name="y">The second element of this vector.</param>
		/// <param name="z">The third element of this vector.</param>
		/// <param name="w">The fourth element of this vector.</param>
		public Vector4(float x, float y, float z, float w) {
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

		/// <summary>
		/// Constructs a copy of the given Vector <paramref name="v"/>, but with the option to override
		/// individual components.
		/// </summary>
		/// <param name="v">The vector to copy.</param>
		/// <param name="x">If not null, this value will be used for <see cref="X"/>. If null, <c>v.X</c> will be used.</param>
		/// <param name="y">If not null, this value will be used for <see cref="Y"/>. If null, <c>v.Y</c> will be used.</param>
		/// <param name="z">If not null, this value will be used for <see cref="Z"/>. If null, <c>v.Z</c> will be used.</param>
		/// <param name="w">If not null, this value will be used for <see cref="W"/>. If null, <c>v.W</c> will be used.</param>
		public Vector4(Vector4 v, float? x = null, float? y = null, float? z = null, float? w = null) 
			: this(x ?? v.X, y ?? v.Y, z ?? v.Z, w ?? v.W) { }

		public static Vector4 Parse(string vectorString) {
			Assure.NotNull(vectorString);
			try {
				string[] numericComponents = vectorString.Trim('[', ']').Split(',');
				float x = float.Parse(numericComponents[0].Trim(), CultureInfo.InvariantCulture);
				float y = float.Parse(numericComponents[1].Trim(), CultureInfo.InvariantCulture);
				float z = float.Parse(numericComponents[2].Trim(), CultureInfo.InvariantCulture);
				float w = float.Parse(numericComponents[3].Trim(), CultureInfo.InvariantCulture);

				return new Vector4(x, y, z, w);
			}
			catch (Exception e) {
				throw new FormatException("Could not parse vector string.", e);
			}
		}


		/// <summary>
		/// Gets a string representation of this vector.
		/// </summary>
		/// <returns>
		/// Returns a string representation of this vector in the form [X, Y, Z, W].
		/// </returns>
		public override string ToString() {
			return ToString(3);
		}

		public string ToString(int numDecimalPlaces) {
			return "[" + X.ToString(numDecimalPlaces) + ", " + Y.ToString(numDecimalPlaces) + ", " + Z.ToString(numDecimalPlaces) + ", " + W.ToString(numDecimalPlaces) + "]";
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(Vector4 other) {
			return EqualsWithTolerance(other, MathUtils.FlopsErrorMargin);
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
			return obj is Vector4 && Equals((Vector4) obj);
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
				// ReSharper disable ImpureMethodCallOnReadonlyValueField
				int hashCode = X.GetHashCode();
				hashCode = (hashCode * 397) ^ Y.GetHashCode();
				hashCode = (hashCode * 397) ^ Z.GetHashCode();
				hashCode = (hashCode * 397) ^ W.GetHashCode();
				// ReSharper restore ImpureMethodCallOnReadonlyValueField
				return hashCode;
			}
		}

		/// <summary>
		/// Test whether this object is EXACTLY equal to <paramref name="other"/>. No floating-point tolerance will be accounted
		/// for: The two structures must be bitwise identical.
		/// </summary>
		/// <param name="other">The other Vector to test against.</param>
		/// <returns>True if this object is EXACTLY equal to <paramref name="other"/>.</returns>
		public bool EqualsExactly(Vector4 other) {
			// ReSharper disable CompareOfFloatsByEqualityOperator
			return X == other.X && Y == other.Y && Z == other.Z && W == other.W;
			// ReSharper restore CompareOfFloatsByEqualityOperator
		}

		/// <summary>
		/// Test whether this object is equal to <paramref name="other"/> within a given <paramref name="tolerance"/>. Even if the
		/// two structures are not identical, this method will return <c>true</c> if they are 'close enough' (e.g. within the tolerance).
		/// </summary>
		/// <param name="other">The other Vector to test against.</param>
		/// <param name="tolerance">The amount of tolerance to give. For any floating-point component in both objects, if the difference
		/// between them is less than this value, they will be considered equal.</param>
		/// <returns>True if this object is equal to <paramref name="other"/> with the given <paramref name="tolerance"/>.</returns>
		public bool EqualsWithTolerance(Vector4 other, double tolerance) {
			Assure.GreaterThanOrEqualTo(tolerance, 0f, "Tolerance must not be negative.");
			return Math.Abs(X - other.X) < tolerance
				&& Math.Abs(Y - other.Y) < tolerance
				&& Math.Abs(Z - other.Z) < tolerance
				&& Math.Abs(W - other.W) < tolerance;
		}

		/// <summary>
		/// Returns the component of this Vector according to the supplied <paramref name="component"/> index.
		/// </summary>
		/// <param name="component">0 = <see cref="X"/>, 1 = <see cref="Y"/>, 2 = <see cref="Z"/>, 3 = <see cref="W"/></param>
		/// <returns>The requested component.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="component"/> is &lt; 0 or &gt; 3.</exception>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations",
			Justification = "What else am I supposed to do?")]
		public float this[int component] {
			get {
				switch (component) {
					case 0: return X;
					case 1: return Y;
					case 2: return Z;
					case 3: return W;
					default: throw new ArgumentOutOfRangeException("component", "Vector only defines 4 components (0:x, 1:y, 2:z, 3:w): " +
																"Can not request component with index " + component + "!");
				}
			}
		}

		/// <summary>
		/// Returns a new vector in the form: [X, Y]; where the values for <see cref="X"/> and
		/// <see cref="Y"/> are chosen according to the components indexed by <paramref name="c0"/> and <paramref name="c1"/>.
		/// </summary>
		/// <param name="c0">The component of this vector that will become the X value of the new vector; where
		/// 0 = <see cref="X"/>, 1 = <see cref="Y"/>, 2 = <see cref="Z"/>, 3 = <see cref="W"/></param>
		/// <param name="c1">The component of this vector that will become the Y value of the new vector; where
		/// 0 = <see cref="X"/>, 1 = <see cref="Y"/>, 2 = <see cref="Z"/>, 3 = <see cref="W"/></param>
		/// <returns>A new <see cref="Vector2"/> that is the selected swizzle of this vector.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="c0"/> or <paramref name="c1"/>
		/// are &lt; 0 or &gt; 3.</exception>
		public Vector2 this[int c0, int c1] {
			get {
				return new Vector2(this[c0], this[c1]);
			}
		}

		/// <summary>
		/// Returns a new vector in the form: [X, Y, Z]; where the values for <see cref="X"/>, <see cref="Y"/> and
		/// <see cref="Z"/> are chosen according to the components indexed by <paramref name="c0"/> to <paramref name="c2"/>.
		/// </summary>
		/// <param name="c0">The component of this vector that will become the X value of the new vector; where
		/// 0 = <see cref="X"/>, 1 = <see cref="Y"/>, 2 = <see cref="Z"/>, 3 = <see cref="W"/></param>
		/// <param name="c1">The component of this vector that will become the Y value of the new vector; where
		/// 0 = <see cref="X"/>, 1 = <see cref="Y"/>, 2 = <see cref="Z"/>, 3 = <see cref="W"/></param>
		/// <param name="c2">The component of this vector that will become the Z value of the new vector; where
		/// 0 = <see cref="X"/>, 1 = <see cref="Y"/>, 2 = <see cref="Z"/>, 3 = <see cref="W"/></param>
		/// <returns>A new <see cref="Vector3"/> that is the selected swizzle of this vector.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Thrown if <paramref name="c0"/>, <paramref name="c1"/> or <paramref name="c2"/>
		/// are &lt; 0 or &gt; 3.</exception>
		public Vector3 this[int c0, int c1, int c2] {
			get {
				return new Vector3(this[c0], this[c1], this[c2]);
			}
		}

		/// <summary>
		/// Returns a new vector in the form: [X, Y, Z, W]; where the values for <see cref="X"/>, <see cref="Y"/>, <see cref="Z"/>,
		/// and <see cref="Z"/> are chosen according to the components indexed by <paramref name="c0"/> to <paramref name="c3"/>.
		/// </summary>
		/// <param name="c0">The component of this vector that will become the X value of the new vector; where
		/// 0 = <see cref="X"/>, 1 = <see cref="Y"/>, 2 = <see cref="Z"/>, 3 = <see cref="W"/></param>
		/// <param name="c1">The component of this vector that will become the Y value of the new vector; where
		/// 0 = <see cref="X"/>, 1 = <see cref="Y"/>, 2 = <see cref="Z"/>, 3 = <see cref="W"/></param>
		/// <param name="c2">The component of this vector that will become the Z value of the new vector; where
		/// 0 = <see cref="X"/>, 1 = <see cref="Y"/>, 2 = <see cref="Z"/>, 3 = <see cref="W"/></param>
		/// <param name="c3">The component of this vector that will become the W value of the new vector; where
		/// 0 = <see cref="X"/>, 1 = <see cref="Y"/>, 2 = <see cref="Z"/>, 3 = <see cref="W"/></param>
		/// <returns>A new <see cref="Vector4"/> that is the selected swizzle of this vector.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Thrown if <paramref name="c0"/>, <paramref name="c1"/>, <paramref name="c2"/> or <paramref name="c3"/>
		/// are &lt; 0 or &gt; 3.</exception>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1025:ReplaceRepetitiveArgumentsWithParamsArray",
			Justification = "I don't want to create an array; so I don't have to create garbage.")]
		public Vector4 this[int c0, int c1, int c2, int c3] {
			get {
				return new Vector4(this[c0], this[c1], this[c2], this[c3]);
			}
		}

		/// <summary>
		/// Indicates whether or not the two operands are equal (within a tolerance defined by <see cref="MathUtils.FlopsErrorMargin"/>).
		/// </summary>
		/// <param name="left">The first operand.</param>
		/// <param name="right">The second operand.</param>
		/// <returns>True if all components in both Vectors are equal.</returns>
		public static bool operator ==(Vector4 left, Vector4 right) {
			return left.Equals(right);
		}

		/// <summary>
		/// Indicates whether or not the two operands are equal (within a tolerance defined by <see cref="MathUtils.FlopsErrorMargin"/>).
		/// </summary>
		/// <param name="left">The first operand.</param>
		/// <param name="right">The second operand.</param>
		/// <returns>True if any components in both Vectors are not equal.</returns>
		public static bool operator !=(Vector4 left, Vector4 right) {
			return !left.Equals(right);
		}

		/// <summary>
		/// Converts the given <see cref="Quaternion"/> to a Vector by copying the value of each component in <paramref name="operand"/>
		/// to the corresponding value of the result vector.
		/// </summary>
		/// <param name="operand">The Quaternion to copy.</param>
		/// <returns><c>new Vector(operand.X, operand.Y, operand.Z, operand.W)</c></returns>
		public static explicit operator Vector4(Quaternion operand) {
			return new Vector4(operand.X, operand.Y, operand.Z, operand.W);
		}
	}
}