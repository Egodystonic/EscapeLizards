// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 21 11 2014 at 15:03 by Ben Bowen

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Ophidian.Losgap {
	/// <summary>
	/// A complex construct containing an imaginary 3-dimension vector (<see cref="X"/>, <see cref="Y"/>, <see cref="Z"/>) and a real
	/// scalar value (<see cref="W"/>); that represent the quotient of two directed lines in three-dimensional space.
	/// </summary>
	/// <remarks>
	/// Quaternions are commonly used to represent rotations.
	/// </remarks>
	[StructLayout(LayoutKind.Sequential, Pack = XMVECTOR_ALIGNMENT, Size = 4 * sizeof(float))]
	public partial struct Quaternion : IToleranceEquatable<Quaternion> {
		private const int XMVECTOR_ALIGNMENT = 16;
		private const float VECTOR_DOT_IDENTITY_BOUND = 0.999999f;
		private const float VECTOR_DOT_180_BOUND = -0.999999f;

		/// <summary>
		/// The 'zero quaternion': A quaternion that has a 0 for each component.
		/// </summary>
		public static readonly Quaternion ZERO = new Quaternion(0f, 0f, 0f, 0f);

		/// <summary>
		/// The identity quaternion, equivalent to '1' in scalar maths (<c>&lt;[0f, 0f, 0f] : 1f&gt;</c>).
		/// </summary>
		public static readonly Quaternion IDENTITY = new Quaternion(0f, 0f, 0f, 1f);

		/// <summary>
		/// The X-component of the imaginary vector part of this Quaternion.
		/// </summary>
		public float X;
		/// <summary>
		/// The Y-component of the imaginary vector part of this Quaternion.
		/// </summary>
		public float Y;
		/// <summary>
		/// The Z-component of the imaginary vector part of this Quaternion.
		/// </summary>
		public float Z;
		/// <summary>
		/// The real part of this Quaternion.
		/// </summary>
		public float W;

		/// <summary>
		/// Constructs this Quaternion with the given real and imaginary components.
		/// </summary>
		/// <param name="x">The X-component of the imaginary vector part of this Quaternion.</param>
		/// <param name="y">The Y-component of the imaginary vector part of this Quaternion.</param>
		/// <param name="z">The Z-component of the imaginary vector part of this Quaternion.</param>
		/// <param name="w">The real part of this Quaternion.</param>
		public Quaternion(float x, float y, float z, float w) {
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

		/// <summary>
		/// Constructs this Quaternion by copying the values of the supplied <see cref="Vector4"/> in to their
		/// corresponding fields.
		/// </summary>
		/// <param name="v">The <see cref="Vector4"/> to copy.</param>
		public Quaternion(Vector4 v) : this(v.X, v.Y, v.Z, v.W) { }

		/// <summary>
		/// Constructs a copy of the given Quaternion <paramref name="q"/>, but with the option to override
		/// individual parts.
		/// </summary>
		/// <param name="q">The quaternion to copy.</param>
		/// <param name="x">If not null, this value will be used for <see cref="X"/>. If null, <c>q.X</c> will be used.</param>
		/// <param name="y">If not null, this value will be used for <see cref="Y"/>. If null, <c>q.Y</c> will be used.</param>
		/// <param name="z">If not null, this value will be used for <see cref="Z"/>. If null, <c>q.Z</c> will be used.</param>
		/// <param name="w">If not null, this value will be used for <see cref="W"/>. If null, <c>q.W</c> will be used.</param>
		public Quaternion(Quaternion q, float? x = null, float? y = null, float? z = null, float? w = null) 
			: this(x ?? q.X, y ?? q.Y, z ?? q.Z, w ?? q.W) { }

		/// <summary>
		/// Constructs a Quaternion from a <see cref="Vector4"/> that describes the axial rotation and rotation amount.
		/// </summary>
		/// <param name="axisAndRotation">The vector that contains the axial rotation (<see cref="Vector4.X"/>, 
		/// <see cref="Vector4.Y"/>, and <see cref="Vector4.Z"/>), and clockwise rotation amount in radians (<see cref="Vector4.W"/>).
		/// This parameter does not need to be <see cref="Vector4.IsUnit">normalized</see>.</param>
		/// <returns>A quaternion that is an encoding of the given axial rotation.</returns>
		public static Quaternion FromAxialRotation(Vector4 axisAndRotation) {
			return FromAxialRotation((Vector3) axisAndRotation, axisAndRotation.W);
		}

		/// <summary>
		/// Constructs a Quaternion from parameters that describe a rotation axis, and a rotation amount.
		/// </summary>
		/// <param name="axisX">The rotation axis's X-component.</param>
		/// <param name="axisY">The rotation axis's Y-component.</param>
		/// <param name="axisZ">The rotation axis's Z-component.</param>
		/// <param name="rotation">The clockwise rotation around the given rotation axis, in radians.</param>
		/// <returns>A quaternion that is an encoding of the given axial rotation.</returns>
		public static Quaternion FromAxialRotation(float axisX, float axisY, float axisZ, float rotation) {
			return FromAxialRotation(new Vector3(axisX, axisY, axisZ), rotation);
		}

		/// <summary>
		/// Constructs a Quaternion from a <see cref="Vector3"/> that describes a rotation axis and a <see cref="float"/>
		/// that describes the rotation amount.
		/// </summary>
		/// <param name="axis">The vector that contains the rotation axis.
		/// This parameter does not need to be <see cref="Vector3.IsUnit">normalized</see>.</param>
		/// <param name="rotation">The clockwise rotation around the given rotation axis, in radians.</param>
		/// <returns>A quaternion that is an encoding of the given axial rotation.</returns>
		public static Quaternion FromAxialRotation(Vector3 axis, float rotation) {
			return new Quaternion(new Vector4(axis.ToUnit() * (float) Math.Sin(-rotation / 2f), w: (float) Math.Cos(rotation / 2f)));
		}

		/// <summary>
		/// Constructs a Quaternion that applies the given radial rotations in <paramref name="yaw"/>, <paramref name="pitch"/>,
		/// <paramref name="roll"/> order.
		/// </summary>
		/// <param name="pitch">The rotation, in radians, around the X-axis.</param>
		/// <param name="yaw">The rotation, in radians, around the Y-axis.</param>
		/// <param name="roll">The rotation, in radians, around the Z-axis.</param>
		/// <returns>A Quaternion that encompasses all three given rotations in yaw-pitch-roll order.</returns>
		public static Quaternion FromEulerRotations(float pitch = 0f, float yaw = 0f, float roll = 0f) {
			return FromAxialRotation(Vector3.UP, yaw) * FromAxialRotation(Vector3.RIGHT, pitch) * FromAxialRotation(Vector3.FORWARD, roll);
		}

		/// <summary>
		/// Constructs a Quaternion that represents the rotation required to move an orientation from <paramref name="start"/>
		/// to <paramref name="end"/>.
		/// </summary>
		/// <param name="start">The starting orientation.</param>
		/// <param name="end">The ending orientation.</param>
		/// <returns>A Quaternion that is an encoding of the necessary rotation to move from <paramref name="start"/>
		/// to <paramref name="end"/>.</returns>
		public static Quaternion FromVectorTransition(Vector3 start, Vector3 end) {
			start = start.ToUnit();
			end = end.ToUnit();

			float dot = Vector3.Dot(start, end);

			if (dot > VECTOR_DOT_IDENTITY_BOUND) {
				return IDENTITY;
			}
			else if (dot < VECTOR_DOT_180_BOUND) {
				Vector3 rotAxis = start * Vector3.RIGHT;
				if (rotAxis == Vector3.ZERO) rotAxis = start * Vector3.UP;
				return FromAxialRotation(rotAxis, MathUtils.PI);
			}
			else {
				Vector3 startCrossEnd = start * end;
				return new Quaternion(
					startCrossEnd.X,
					startCrossEnd.Y,
					startCrossEnd.Z,
					(float) Math.Sqrt(start.LengthSquared * end.LengthSquared) + dot
				).ToUnit();
			}
		}

		public static Quaternion Parse(string quaternionString) {
			Assure.NotNull(quaternionString);
			try {
				string[] components = quaternionString.Trim('<', '>').Split('~');
				Vector3 xyz = Vector3.Parse(components[0].Trim());
				float w = float.Parse(components[1].Trim(), CultureInfo.InvariantCulture);

				return new Quaternion(new Vector4(xyz, w: w));
			}
			catch (Exception e) {
				throw new FormatException("Could not parse transform string.", e);
			}
		}

		/// <summary>
		/// Returns a string representation of this Quaternion.
		/// </summary>
		/// <returns>A string that shows the imaginary part, and then the real part.</returns>
		public override string ToString() {
			return ToString(3);
		}

		public string ToString(int numDecimalPlaces) {
			return "<[" + X.ToString(numDecimalPlaces) + ", " + Y.ToString(numDecimalPlaces) + ", " + Z.ToString(numDecimalPlaces) + "] ~ " + W.ToString(numDecimalPlaces) + ">";
		}

		public string ToStringAsEuler(int numDecimalPlaces = 3) {
			return "<{" +
				RotAroundX.ToString(numDecimalPlaces) + "rad, " +
				RotAroundY.ToString(numDecimalPlaces) + "rad, " +
				RotAroundZ.ToString(numDecimalPlaces) + "rad" + "}>";
		}

		/// <summary>
		/// Test whether this object is EXACTLY equal to <paramref name="other"/>. No floating-point tolerance will be accounted
		/// for: The two structures must be bitwise identical.
		/// </summary>
		/// <param name="other">The other Quaternion to test against.</param>
		/// <returns>True if this object is EXACTLY equal to <paramref name="other"/>.</returns>
		public bool EqualsExactly(Quaternion other) {
			// ReSharper disable CompareOfFloatsByEqualityOperator
			return X == other.X && Y == other.Y && Z == other.Z && W == other.W;
			// ReSharper restore CompareOfFloatsByEqualityOperator
		}

		/// <summary>
		/// Test whether this object is equal to <paramref name="other"/> within a given <paramref name="tolerance"/>. Even if the
		/// two structures are not identical, this method will return <c>true</c> if they are 'close enough' (e.g. within the tolerance).
		/// </summary>
		/// <param name="other">The other Quaternion to test against.</param>
		/// <param name="tolerance">The amount of tolerance to give. For any floating-point component in both objects, if the difference
		/// between them is less than this value, they will be considered equal.</param>
		/// <returns>True if this object is equal to <paramref name="other"/> with the given <paramref name="tolerance"/>.</returns>
		public bool EqualsWithTolerance(Quaternion other, double tolerance) {
			Assure.GreaterThanOrEqualTo(tolerance, 0f, "Tolerance must not be negative.");
			return Math.Abs(X - other.X) < tolerance
				&& Math.Abs(Y - other.Y) < tolerance
				&& Math.Abs(Z - other.Z) < tolerance
				&& Math.Abs(W - other.W) < tolerance;
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(Quaternion other) {
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
			return obj is Quaternion && Equals((Quaternion)obj);
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
				int hashCode = X.GetHashCode();
				hashCode = (hashCode * 397) ^ Y.GetHashCode();
				hashCode = (hashCode * 397) ^ Z.GetHashCode();
				hashCode = (hashCode * 397) ^ W.GetHashCode();
				return hashCode;
			}
		}

		/// <summary>
		/// Indicates whether or not the two operands are equal (within a tolerance defined by <see cref="MathUtils.FlopsErrorMargin"/>).
		/// </summary>
		/// <param name="left">The first operand.</param>
		/// <param name="right">The second operand.</param>
		/// <returns>True if the imaginary part (<see cref="X"/>, <see cref="Y"/>, and <see cref="Z"/>) and the real part
		/// (<see cref="W"/>) in both Quaternions are equal.</returns>
		public static bool operator ==(Quaternion left, Quaternion right) {
			return left.Equals(right);
		}

		/// <summary>
		/// Indicates whether or not the two operands are equal (within a tolerance defined by <see cref="MathUtils.FlopsErrorMargin"/>).
		/// </summary>
		/// <param name="left">The first operand.</param>
		/// <param name="right">The second operand.</param>
		/// <returns>True if the imaginary part (<see cref="X"/>, <see cref="Y"/>, and <see cref="Z"/>) or the real part
		/// (<see cref="W"/>) in both Quaternions are not equal.</returns>
		public static bool operator !=(Quaternion left, Quaternion right) {
			return !left.Equals(right);
		}

		/// <summary>
		/// Converts the given <see cref="Vector4"/> to a Quaternion by copying the value of each component in <paramref name="operand"/>
		/// to the corresponding value of the result quaternion.
		/// </summary>
		/// <param name="operand">The Vector to copy.</param>
		/// <returns><c>new Quaternion(operand.X, operand.Y, operand.Z, operand.W)</c></returns>
		public static explicit operator Quaternion(Vector4 operand) {
			return new Quaternion(operand.X, operand.Y, operand.Z, operand.W);
		}
	}
}