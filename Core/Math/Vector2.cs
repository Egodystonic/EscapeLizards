// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 15 12 2014 at 10:34 by Ben Bowen

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ophidian.Losgap {
	/// <summary>
	/// Two floating-point components that are grouped together and used to represent magnitudes, velocities, and points/co-ordinates.
	/// </summary>
	/// <remarks>
	/// Vectors are used to represent points in geometric space or length + direction combinations from an arbitrary
	/// source.
	/// <para>
	/// Vector equality comparisons automatically have a built-in floating-point tolerance (<see cref="MathUtils.FlopsErrorMargin"/>).
	/// </para>
	/// </remarks>
	/// <seealso cref="Vector4"/>
	/// <seealso cref="Vector3"/>
	[StructLayout(LayoutKind.Explicit, Size = 2 * sizeof(float))]
	public partial struct Vector2 : IToleranceEquatable<Vector2> {
		/// <summary>
		/// A vector where all elements are equal to 0f: [0f, 0f]
		/// </summary>
		public static readonly Vector2 ZERO = new Vector2(0f, 0f);

		/// <summary>
		/// A vector where all elements are equal to 1f: [1f, 1f, 1f]
		/// </summary>
		public static readonly Vector2 ONE = new Vector2(1f, 1f);

		/// <summary>
		/// A vector that represents one unit in the left direction: [-1f, 0f]
		/// </summary>
		public static readonly Vector2 LEFT = new Vector2(-1f, 0f);

		/// <summary>
		/// A vector that represents one unit in the right direction: [1f, 0f]
		/// </summary>
		public static readonly Vector2 RIGHT = new Vector2(1f, 0f);

		/// <summary>
		/// A vector that represents one unit in the up direction: [0f, 1f]
		/// </summary>
		public static readonly Vector2 UP = new Vector2(0f, 1f);

		/// <summary>
		/// A vector that represents one unit in the down direction: [0f, -1f]
		/// </summary>
		public static readonly Vector2 DOWN = new Vector2(0f, -1f);

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
		/// Creates a new Vector with the given components.
		/// </summary>
		/// <param name="x">The first element of this vector.</param>
		/// <param name="y">The second element of this vector.</param>
		public Vector2(float x, float y) : this() {
			X = x;
			Y = y;
		}

		/// <summary>
		/// Constructs a copy of the given Vector <paramref name="v"/>, but with the option to override
		/// individual components.
		/// </summary>
		/// <param name="v">The vector to copy.</param>
		/// <param name="x">If not null, this value will be used for <see cref="X"/>. If null, <c>v.X</c> will be used.</param>
		/// <param name="y">If not null, this value will be used for <see cref="Y"/>. If null, <c>v.Y</c> will be used.</param>
		public Vector2(Vector2 v, float? x = null, float? y = null)
			: this(x ?? v.X, y ?? v.Y) { }

		public static Vector2 Parse(string vectorString) {
			Assure.NotNull(vectorString);
			try {
				string[] numericComponents = vectorString.Trim('[',']').Split(',');
				float x = float.Parse(numericComponents[0].Trim(), CultureInfo.InvariantCulture);
				float y = float.Parse(numericComponents[1].Trim(), CultureInfo.InvariantCulture);

				return new Vector2(x, y);
			}
			catch (Exception e) {
				throw new FormatException("Could not parse vector string.", e);
			}
		}

		/// <summary>
		/// Gets a string representation of this vector.
		/// </summary>
		/// <returns>
		/// Returns a string representation of this vector in the form [X, Y].
		/// </returns>
		public override string ToString() {
			return ToString(3);
		}

		public string ToString(int numDecimalPlaces) {
			return "[" + X.ToString(numDecimalPlaces) + ", " + Y.ToString(numDecimalPlaces) + "]";
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(Vector2 other) {
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
			return obj is Vector2 && Equals((Vector2) obj);
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>
		/// A 32-bit signed integer that is the hash code for this instance.
		/// </returns>
		public override int GetHashCode() {
			unchecked {
				// ReSharper disable ImpureMethodCallOnReadonlyValueField
				int hashCode = X.GetHashCode();
				hashCode = (hashCode * 397) ^ Y.GetHashCode();
				// ReSharper restore ImpureMethodCallOnReadonlyValueField
				return hashCode;
			}
		}

		/// <summary>
		/// Test whether this object is EXACTLY equal to <paramref name="other"/>. No floating-point tolerance will be accounted
		/// for: The two structures must be bitwise identical.
		/// </summary>
		/// <param name="other">The other Vector3 to test against.</param>
		/// <returns>True if this object is EXACTLY equal to <paramref name="other"/>.</returns>
		public bool EqualsExactly(Vector2 other) {
			// ReSharper disable CompareOfFloatsByEqualityOperator
			return X == other.X && Y == other.Y;
			// ReSharper restore CompareOfFloatsByEqualityOperator
		}

		/// <summary>
		/// Test whether this object is equal to <paramref name="other"/> within a given <paramref name="tolerance"/>. Even if the
		/// two structures are not identical, this method will return <c>true</c> if they are 'close enough' (e.g. within the tolerance).
		/// </summary>
		/// <param name="other">The other Vector3 to test against.</param>
		/// <param name="tolerance">The amount of tolerance to give. For any floating-point component in both objects, if the difference
		/// between them is less than this value, they will be considered equal.</param>
		/// <returns>True if this object is equal to <paramref name="other"/> with the given <paramref name="tolerance"/>.</returns>
		public bool EqualsWithTolerance(Vector2 other, double tolerance) {
			Assure.GreaterThanOrEqualTo(tolerance, 0f, "Tolerance must not be negative.");
			return Math.Abs(X - other.X) < tolerance
				&& Math.Abs(Y - other.Y) < tolerance;
		}

		/// <summary>
		/// Returns the component of this Vector according to the supplied <paramref name="component"/> index.
		/// </summary>
		/// <param name="component">0 = <see cref="X"/>, 1 = <see cref="Y"/></param>
		/// <returns>The requested component.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="component"/> is &lt; 0 or &gt; 1.</exception>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations",
			Justification = "What else am I supposed to do?")]
		public float this[int component] {
			get {
				switch (component) {
					case 0: return X;
					case 1: return Y;
					default: throw new ArgumentOutOfRangeException("component", "Vector3 only defines 3 components (0:x, 1:y, 2:z): " +
																"Can not request component with index " + component + "!");
				}
			}
		}

		/// <summary>
		/// Returns a new vector in the form: [X, Y]; where the values for <see cref="X"/> and
		/// <see cref="Y"/> are chosen according to the components indexed by <paramref name="c0"/> and <paramref name="c1"/>.
		/// </summary>
		/// <param name="c0">The component of this vector that will become the X value of the new vector; where
		/// 0 = <see cref="X"/>, 1 = <see cref="Y"/></param>
		/// <param name="c1">The component of this vector that will become the Y value of the new vector; where
		/// 0 = <see cref="X"/>, 1 = <see cref="Y"/></param>
		/// <returns>A new <see cref="Vector3"/> that is the selected swizzle of this vector.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="c0"/> or <paramref name="c1"/>
		/// are &lt; 0 or &gt; 1.</exception>
		public Vector2 this[int c0, int c1] {
			get { return new Vector2(this[c0], this[c1]); }
		}

		/// <summary>
		/// Returns a new vector in the form: [X, Y, Z]; where the values for <see cref="Vector3.X"/>, <see cref="Vector3.Y"/>,
		/// and <see cref="Vector3.Z"/> are chosen according to the components 
		/// indexed by <paramref name="c0"/>, <paramref name="c1"/>, and <paramref name="c2"/>.
		/// </summary>
		/// <param name="c0">The component of this vector that will become the X value of the new vector; where
		/// 0 = <see cref="X"/>, 1 = <see cref="Y"/></param>
		/// <param name="c1">The component of this vector that will become the Y value of the new vector; where
		/// 0 = <see cref="X"/>, 1 = <see cref="Y"/></param>
		/// <param name="c2">The component of this vector that will become the Z value of the new vector; where
		/// 0 = <see cref="X"/>, 1 = <see cref="Y"/></param>
		/// <returns>A new <see cref="Vector3"/> that is the selected swizzle of this vector.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="c0"/> or <paramref name="c1"/>
		/// are &lt; 0 or &gt; 2.</exception>
		public Vector3 this[int c0, int c1, int c2] {
			get { return new Vector3(this[c0], this[c1], this[c2]); }
		}

		/// <summary>
		/// Returns a new vector in the form: [X, Y, Z, W]; where the values for <see cref="Vector4.X"/>, <see cref="Vector4.Y"/>,
		/// <see cref="Vector4.Z"/>, and <see cref="Vector4.W"/> are chosen according to the components 
		/// indexed by <paramref name="c0"/>, <paramref name="c1"/>, <paramref name="c2"/>, and <paramref name="c3"/>.
		/// </summary>
		/// <param name="c0">The component of this vector that will become the X value of the new vector; where
		/// 0 = <see cref="X"/>, 1 = <see cref="Y"/></param>
		/// <param name="c1">The component of this vector that will become the Y value of the new vector; where
		/// 0 = <see cref="X"/>, 1 = <see cref="Y"/></param>
		/// <param name="c2">The component of this vector that will become the Z value of the new vector; where
		/// 0 = <see cref="X"/>, 1 = <see cref="Y"/></param>
		/// <param name="c3">The component of this vector that will become the W value of the new vector; where
		/// 0 = <see cref="X"/>, 1 = <see cref="Y"/></param>
		/// <returns>A new <see cref="Vector4"/> that is the selected swizzle of this vector.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="c0"/> or <paramref name="c1"/>
		/// are &lt; 0 or &gt; 2.</exception>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1025:ReplaceRepetitiveArgumentsWithParamsArray",
			Justification = "I don't want to create an array (+ garbage).")]
		public Vector4 this[int c0, int c1, int c2, int c3] {
			get { return new Vector4(this[c0], this[c1], this[c2], this[c3]); }
		}

		/// <summary>
		/// Indicates whether or not the two operands are equal (within a tolerance defined by <see cref="MathUtils.FlopsErrorMargin"/>).
		/// </summary>
		/// <param name="left">The first operand.</param>
		/// <param name="right">The second operand.</param>
		/// <returns>True if all components in both Vectors are equal.</returns>
		public static bool operator ==(Vector2 left, Vector2 right) {
			return left.Equals(right);
		}

		/// <summary>
		/// Indicates whether or not the two operands are equal (within a tolerance defined by <see cref="MathUtils.FlopsErrorMargin"/>).
		/// </summary>
		/// <param name="left">The first operand.</param>
		/// <param name="right">The second operand.</param>
		/// <returns>True if any components in both Vectors are not equal.</returns>
		public static bool operator !=(Vector2 left, Vector2 right) {
			return !left.Equals(right);
		}

		/// <summary>
		/// Converts a <see cref="Vector2"/> to a <see cref="Vector4"/>.
		/// </summary>
		/// <param name="operand">The Vector2 to convert.</param>
		/// <returns>A new Vector4 whose <see cref="Vector4.X"/> and <see cref="Vector4.Y"/>
		/// components are set according to the <paramref name="operand"/>'s <see cref="X"/> and <see cref="Y"/>
		/// components; and with a <see cref="Vector4.Z"/> and <see cref="Vector4.W"/> value of <c>0f</c>.</returns>
		public static implicit operator Vector4(Vector2 operand) {
			return new Vector4(operand.X, operand.Y, 0f, 0f);
		}

		/// <summary>
		/// Converts a <see cref="Vector4"/> to a <see cref="Vector2"/>.
		/// </summary>
		/// <param name="operand">The Vector4 to convert.</param>
		/// <returns>A new Vector2 whose <see cref="X"/> and <see cref="Y"/>
		/// components are set according to the <paramref name="operand"/>'s <see cref="Vector4.X"/>
		/// and <see cref="Vector4.Y"/> components. The <paramref name="operand"/>'s <see cref="Vector4.Z"/> and <see cref="Vector4.W"/>
		/// components will be ignored.</returns>
		public static explicit operator Vector2(Vector4 operand) {
			return new Vector2(operand.X, operand.Y);
		}

		/// <summary>
		/// Converts a <see cref="Vector2"/> to a <see cref="Vector3"/>.
		/// </summary>
		/// <param name="operand">The Vector2 to convert.</param>
		/// <returns>A new Vector3 whose <see cref="Vector3.X"/> and <see cref="Vector3.Y"/>
		/// components are set according to the <paramref name="operand"/>'s <see cref="X"/> and <see cref="Y"/>
		/// components; and with a <see cref="Vector4.Z"/> value of <c>0f</c>.</returns>
		public static implicit operator Vector3(Vector2 operand) {
			return new Vector3(operand.X, operand.Y, 0f);
		}

		/// <summary>
		/// Converts a <see cref="Vector3"/> to a <see cref="Vector2"/>.
		/// </summary>
		/// <param name="operand">The Vector3 to convert.</param>
		/// <returns>A new Vector2 whose <see cref="X"/> and <see cref="Y"/>
		/// components are set according to the <paramref name="operand"/>'s <see cref="Vector3.X"/>
		/// and <see cref="Vector3.Y"/> components. The <paramref name="operand"/>'s <see cref="Vector4.Z"/>
		/// component will be ignored.</returns>
		public static explicit operator Vector2(Vector3 operand) {
			return new Vector2(operand.X, operand.Y);
		}
	}
}