// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 20 11 2014 at 13:34 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;

namespace Ophidian.Losgap {
	public partial struct Vector4 {
		/// <summary>
		/// Indicates whether or not this vector is unit length (i.e. its <see cref="Length"/> == 1.0f).
		/// This property has a built in tolerance of <see cref="MathUtils.FlopsErrorMargin"/>.
		/// </summary>
		public bool IsUnit {
			get {
				return Math.Abs(LengthSquared - 1f) < MathUtils.FlopsErrorMargin;
			}
		}

		/// <summary>
		/// The length of this vector.
		/// </summary>
		/// <remarks>
		/// When simply using the length of a vector in a comparison operation, consider using <see cref="LengthSquared"/>
		/// instead; as it removes the costly square-root calculation.
		/// </remarks>
		public float Length {
			get {
				if (IsUnit) return 1f;
				return (float) Math.Sqrt(LengthSquared);
			}
		}


		/// <summary>
		/// The length of this vector, squared.
		/// </summary>
		/// <remarks>
		/// Consider using this instead of <see cref="Length"/> when only comparing the length of a vector, as this method
		/// does not invoke a costly square-root operation.
		/// </remarks>
		public float LengthSquared {
			get {
				return X * X + Y * Y + Z * Z + W * W;
			}
		}

		/// <summary>
		/// Gets the angle formed between two vectors, in radians.
		/// </summary>
		/// <param name="v1">The first vector.</param>
		/// <param name="v2">The second vector.</param>
		/// <returns>Inverse cosine of: The sum of: Each component in <paramref name="v1"/> multiplied with the corresponding
		/// component in <paramref name="v2"/>.</returns>
		public static float AngleBetween(Vector4 v1, Vector4 v2) {
			Assure.NotEqual(v1, ZERO, "Can not find angle for zero vector.");
			Assure.NotEqual(v2, ZERO, "Can not find angle for zero vector.");
			v1 = v1.ToUnit();
			v2 = v2.ToUnit();

			return (float) Math.Acos(v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z + v1.W * v2.W);
		}

		/// <summary>
		/// Calculates the scalar product (dot product) of the two Vectors.
		/// </summary>
		/// <param name="v1">The first operand.</param>
		/// <param name="v2">The second operand.</param>
		/// <returns><c>v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z + v1.W * v2.W</c></returns>
		public static float Dot(Vector4 v1, Vector4 v2) {
			return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z + v1.W * v2.W;
		}

		/// <summary>
		/// Returns the 3D cross product of <paramref name="v1"/> and <paramref name="v2"/>.
		/// </summary>
		/// <remarks>
		/// Returns a new Vector that is perpendicular to both <paramref name="v1"/> and <paramref name="v2"/>.
		/// The fourth component of each input vector (<see cref="W"/>) is ignored.
		/// </remarks>
		/// <param name="v1">The first vector.</param>
		/// <param name="v2">The second vector.</param>
		/// <returns>A mutually orthogonal vector. The returned vector will not be of unit length unless
		/// both input vectors (<paramref name="v1"/> &amp; <paramref name="v2"/>) are also unit vectors.</returns>
		public static Vector4 Cross(Vector4 v1, Vector4 v2) {
			return new Vector4(
				v1.Y * v2.Z - v1.Z * v2.Y,
				v1.Z * v2.X - v1.X * v2.Z,
				v1.X * v2.Y - v1.Y * v2.X,
				0f
			);
		}

		/// <summary>
		/// Returns the distance between the two vectors (when they are representing points in 4D space).
		/// </summary>
		/// <param name="v1">The first vector.</param>
		/// <param name="v2">The second vector.</param>
		/// <returns>The distance between the two vectors.</returns>
		public static float Distance(Vector4 v1, Vector4 v2) {
			return (v2 - v1).Length;
		}

		/// <summary>
		/// Returns the distance between the two vectors, squared (when they are representing points in 1D, 2D, 3D, or 4D space).
		/// </summary>
		/// <remarks>
		/// This method is faster than <see cref="Distance(Vector4,Vector4)"/> and therefore should be
		/// used when only using the distance for comparisons.
		/// </remarks>
		/// <param name="v1">The first vector.</param>
		/// <param name="v2">The second vector.</param>
		/// <returns>The distance between the two vectors, squared.</returns>
		public static float DistanceSquared(Vector4 v1, Vector4 v2) {
			return (v2 - v1).LengthSquared;
		}

		/// <summary>
		/// Returns a vector that is somewhere between the <paramref name="start"/> and <paramref name="end"/> points, according
		/// to <paramref name="amount"/>.
		/// </summary>
		/// <param name="start">The start point.</param>
		/// <param name="end">The end point.</param>
		/// <param name="amount">The distance from <paramref name="start"/> (and towards <paramref name="end"/>) to get the vector for,
		/// between <c>0f</c> and <c>1f</c>. A value of <c>0f</c> will return <paramref name="start"/>, and a value of <c>1f</c>
		/// will return <paramref name="end"/>.</param>
		/// <returns><c>start + (end - start) * amount</c>.</returns>
		public static Vector4 Lerp(Vector4 start, Vector4 end, float amount) {
			return start + (end - start) * amount;
		}

		/// <summary>
		/// Returns a vector with the same orientation as this one but with a <see cref="Length"/> of <c>1f</c>.
		/// </summary>
		/// <remarks>
		/// This operation is also referred to as Normalization.
		/// </remarks>
		/// <returns>[this.X / Length, this.Y / Length, this.Z / Length, this.W / Length]</returns>
		public Vector4 ToUnit() {
			Assure.False(EqualsExactly(ZERO), "Zero vector has no corresponding unit.");
			if (IsUnit) return this;
			float magnitude = Length;
			return new Vector4(X / magnitude, Y / magnitude, Z / magnitude, W / magnitude);
		}

		/// <summary>
		/// Returns a new vector that has the same orientation as this vector, but with the supplied
		/// <paramref name="length"/>.
		/// </summary>
		/// <param name="length">The new length. Supplying a negative value will reverse
		/// the direction of the resultant vector.</param>
		/// <returns><c>ToUnit() * length</c></returns>
		public Vector4 WithLength(float length) {
			return ToUnit() * length;
		}

		/// <summary>
		/// Returns the dot product of this vector and <paramref name="other"/>.
		/// </summary>
		/// <param name="other">The second vector.</param>
		/// <returns><c>Vector.Dot(this, other)</c>.</returns>
		public float Dot(Vector4 other) {
			return Dot(this, other);
		}

		/// <summary>
		/// Returns the 3D cross product of this and <paramref name="other"/>.
		/// </summary>
		/// <remarks>
		/// Returns a new Vector that is perpendicular to both this and <paramref name="other"/>.
		/// The fourth component of each input vector (<see cref="W"/>) is ignored.
		/// </remarks>
		/// <param name="other">The second vector.</param>
		/// <returns>A mutually orthogonal vector. The returned vector will not be of unit length unless
		/// both input vectors (this &amp; <paramref name="other"/>) are also unit vectors.</returns>
		public Vector4 Cross(Vector4 other) {
			return Cross(this, other);
		}

		public Vector4 Scale(Vector4 other) {
			return new Vector4(X * other.X, Y * other.Y, Z * other.Z, W * other.W);
		}

		/// <summary>
		/// Returns the projection of this vector on the given <paramref name="axis"/> vector (needs not be a primary world axis).
		/// </summary>
		/// <param name="axis">The axis to project on to.</param>
		/// <returns>A new Vector that is the the equivalent part of this vector that 'points' along the <paramref name="axis"/>
		/// vector.</returns>
		public Vector4 ProjectedOnto(Vector4 axis) {
			Assure.NotEqual(axis, ZERO, "Can not project on to zero vector.");
			axis = axis.ToUnit();
			return Dot(this, axis) * axis;
		}

		/// <summary>
		/// Adds each component in each Vector to create a new Vector that is the sum of both operands.
		/// </summary>
		/// <param name="lhs">The first operand.</param>
		/// <param name="rhs">The second operand.</param>
		/// <returns><c>[lhs.X + rhs.X, lhs.Y + rhs.Y, lhs.Z + rhs.Z, lhs.W + rhs.W]</c></returns>
		public static Vector4 operator +(Vector4 lhs, Vector4 rhs) {
			return new Vector4(lhs.X + rhs.X, lhs.Y + rhs.Y, lhs.Z + rhs.Z, lhs.W + rhs.W);
		}

		/// <summary>
		/// Subtracts each component in <paramref name="rhs"/> from the corresponding component in <paramref name="lhs"/>
		/// to create a new Vector that is the difference of both operands.
		/// </summary>
		/// <param name="lhs">The first operand.</param>
		/// <param name="rhs">The second operand.</param>
		/// <returns><c>[lhs.X - rhs.X, lhs.Y - rhs.Y, lhs.Z - rhs.Z, lhs.W - rhs.W]</c></returns>
		public static Vector4 operator -(Vector4 lhs, Vector4 rhs) {
			return new Vector4(lhs.X - rhs.X, lhs.Y - rhs.Y, lhs.Z - rhs.Z, lhs.W - rhs.W);
		}

		/// <summary>
		/// Multiplies the magnitude of the Vector <paramref name="lhs"/> by the scalar <paramref name="rhs"/>.
		/// </summary>
		/// <param name="lhs">The Vector operand.</param>
		/// <param name="rhs">The scalar operand.</param>
		/// <returns><c>[lhs.X * rhs, lhs.Y * rhs, lhs.Z * rhs, lhs.W * rhs]</c></returns>
		public static Vector4 operator *(Vector4 lhs, float rhs) {
			return new Vector4(lhs.X * rhs, lhs.Y * rhs, lhs.Z * rhs, lhs.W * rhs);
		}

		/// <summary>
		/// Multiplies the magnitude of the Vector <paramref name="rhs"/> by the scalar <paramref name="lhs"/>.
		/// </summary>
		/// <param name="lhs">The scalar operand.</param>
		/// <param name="rhs">The Vector operand.</param>
		/// <returns><c>[lhs * rhs.X, lhs * rhs.Y, lhs * rhs.Z, lhs * rhs.W]</c></returns>
		public static Vector4 operator *(float lhs, Vector4 rhs) {
			return new Vector4(lhs * rhs.X, lhs * rhs.Y, lhs * rhs.Z, lhs * rhs.W);
		}

		/// <summary>
		/// Divides the magnitude of the Vector <paramref name="lhs"/> by the scalar <paramref name="rhs"/>.
		/// </summary>
		/// <param name="lhs">The Vector operand.</param>
		/// <param name="rhs">The scalar operand.</param>
		/// <returns><c>[lhs.X / rhs, lhs.Y / rhs, lhs.Z / rhs, lhs.W / rhs]</c></returns>
		public static Vector4 operator /(Vector4 lhs, float rhs) {
			return new Vector4(lhs.X / rhs, lhs.Y / rhs, lhs.Z / rhs, lhs.W / rhs);
		}

		public static Vector4 operator /(float lhs, Vector4 rhs) {
			return new Vector4(lhs / rhs.X, lhs / rhs.Y, lhs / rhs.Z, lhs / rhs.W);
		}

		
		/// <summary>
		/// Returns the 3D cross product of <paramref name="lhs"/> and <paramref name="rhs"/>.
		/// </summary>
		/// <remarks>
		/// Returns a new Vector that is perpendicular to both <paramref name="lhs"/> and <paramref name="rhs"/>.
		/// The fourth component of each input vector (<see cref="W"/>) is ignored.
		/// </remarks>
		/// <param name="lhs">The first vector.</param>
		/// <param name="rhs">The second vector.</param>
		/// <returns>A mutually orthogonal vector. The returned vector will not be of unit length unless
		/// both input vectors (<paramref name="lhs"/> &amp; <paramref name="rhs"/>) are also unit vectors.</returns>
		public static Vector4 operator *(Vector4 lhs, Vector4 rhs) {
			return Cross(lhs, rhs);
		}

		/// <summary>
		/// Flips the direction of the <paramref name="operand"/>. Does not alter its magnitude (length).
		/// </summary>
		/// <param name="operand">The Vector to be flipped.</param>
		/// <returns><c>[operand.X * -1, operand.Y * -1, operand.Z * -1, operand.W * -1]</c></returns>
		public static Vector4 operator -(Vector4 operand) {
			return new Vector4(operand.X * -1, operand.Y * -1, operand.Z * -1, operand.W * -1);
		}

		public static Vector4 operator ~(Vector4 operand) {
			return new Vector4(1f / operand.X, 1f / operand.Y, 1f / operand.Z, 1f / operand.W);
		}

		/// <summary>
		/// Indicates whether or not the <see cref="Length"/> of <paramref name="lhs"/> is greater than the <see cref="Length"/>
		/// of <paramref name="rhs"/>.
		/// </summary>
		/// <param name="lhs">The first operand.</param>
		/// <param name="rhs">The second operand.</param>
		/// <returns><c>lhs.LengthSquared &gt; rhs.LengthSquared</c></returns>
		public static bool operator >(Vector4 lhs, Vector4 rhs) {
			return lhs.LengthSquared > rhs.LengthSquared;
		}

		/// <summary>
		/// Indicates whether or not the <see cref="Length"/> of <paramref name="lhs"/> is less than the <see cref="Length"/>
		/// of <paramref name="rhs"/>.
		/// </summary>
		/// <param name="lhs">The first operand.</param>
		/// <param name="rhs">The second operand.</param>
		/// <returns><c>lhs.LengthSquared &lt; rhs.LengthSquared</c></returns>
		public static bool operator <(Vector4 lhs, Vector4 rhs) {
			return lhs.LengthSquared < rhs.LengthSquared;
		}

		/// <summary>
		/// Indicates whether or not the <see cref="Length"/> of <paramref name="lhs"/> is greater than
		/// or equal to the <see cref="Length"/> of <paramref name="rhs"/>.
		/// </summary>
		/// <remarks>
		/// The equality comparison has a tolerance of <see cref="MathUtils.FlopsErrorMargin"/>. If you wish to test for precise equality,
		/// use a combination of the <see cref="op_GreaterThan">greater-than operator</see> and <see cref="EqualsExactly"/>.
		/// </remarks>
		/// <param name="lhs">The first operand.</param>
		/// <param name="rhs">The second operand.</param>
		/// <returns><c>lhs.LengthSquared &gt; rhs.LengthSquared || lhs == rhs</c></returns>
		public static bool operator >=(Vector4 lhs, Vector4 rhs) {
			return lhs.LengthSquared > rhs.LengthSquared || lhs == rhs;
		}

		/// <summary>
		/// Indicates whether or not the <see cref="Length"/> of <paramref name="lhs"/> is less than
		/// or equal to the <see cref="Length"/> of <paramref name="rhs"/>.
		/// </summary>
		/// <remarks>
		/// The equality comparison has a tolerance of <see cref="MathUtils.FlopsErrorMargin"/>. If you wish to test for precise equality,
		/// use a combination of the <see cref="op_LessThan">less-than operator</see> and <see cref="EqualsExactly"/>.
		/// </remarks>
		/// <param name="lhs">The first operand.</param>
		/// <param name="rhs">The second operand.</param>
		/// <returns><c>lhs.LengthSquared &lt; rhs.LengthSquared || lhs == rhs</c></returns>
		public static bool operator <=(Vector4 lhs, Vector4 rhs) {
			return lhs.LengthSquared < rhs.LengthSquared || lhs == rhs;
		}
	}
}