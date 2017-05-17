// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 24 11 2014 at 11:02 by Ben Bowen

using System;
using System.Collections.Generic;

namespace Ophidian.Losgap {
	public partial struct Quaternion {
		private const float SLERP_MAX_COS_PHI_BEFORE_LERP = 1f - 0.001f;

		/// <summary>
		/// The conjugate of this Quaternion.
		/// </summary>
		public Quaternion Conjugate {
			get {
				return new Quaternion(-X, -Y, -Z, W);
			}
		}

		/// <summary>
		/// The norm (magnitude, length) of this quaternion.
		/// </summary>
		/// <seealso cref="NormSquared"/>
		public float Norm {
			get {
				return (float) Math.Sqrt(X * X + Y * Y + Z * Z + W * W);
			}
		}

		/// <summary>
		/// The norm (magnitude, length) of this quaternion, squared. Using this property instead of <see cref="Norm"/>
		/// is desirable for comparison-only operations, as it avoids a square-root operation.
		/// </summary>
		public float NormSquared {
			get {
				return X * X + Y * Y + Z * Z + W * W;
			}
		}

		/// <summary>
		/// True if this Quaternion is unit-length.
		/// </summary>
		public bool IsUnit {
			get {
				return Math.Abs(Norm - 1f) < MathUtils.FlopsErrorMargin;
			}
		}

		/// <summary>
		/// True if this Quaternion has an inverse.
		/// </summary>
		public bool HasInverse {
			get {
				return !EqualsExactly(ZERO);
			}
		}

		/// <summary>
		/// The inverse of this quaternion. When using a quaternion to encode a rotation, this property represents an equal rotation in the opposite
		/// direction.
		/// </summary>
		public Quaternion Inverse {
			get {
				Assure.False(EqualsExactly(ZERO), "Zero quaternion has no inverse.");
				return Conjugate / NormSquared;
			}
		}

		public Quaternion Exponential {
			get {
				Vector3 xyz = new Vector3(X, Y, Z);
				float xyzLength = xyz.Length;
				float expW = (float) Math.Exp(W);
				xyz = xyz.ToUnit() * expW * (float) Math.Sin(xyzLength);
				return new Quaternion(xyz.X, xyz.Y, xyz.Z, expW * (float) Math.Cos(xyzLength));
			}
		}

		/// <summary>
		/// A <see cref="Matrix"/> that encapsulates the rotation denoted by this quaternion.
		/// </summary>
		public Matrix RotMatrix {
			get {
				return Matrix.FromRotation(this);
			}
		}

		public float RotAroundX {
			get {
				return Vector3.AngleBetween(Vector3.UP, Vector3.UP * this);
			}
		}

		public float RotAroundY {
			get {
				return Vector3.AngleBetween(Vector3.FORWARD, Vector3.FORWARD * this);
			}
		}

		public float RotAroundZ {
			get {
				return Vector3.AngleBetween(Vector3.RIGHT, Vector3.RIGHT * this);
			}
		}

		/// <summary>
		/// Returns a Quaternion that represents a <paramref name="fraction"/> of this rotation, where 1.0f is the whole rotation, 0.0f is equal to
		/// <see cref="IDENTITY"/>, 2.0f is double the rotation, and -1.0f is the rotation in the reverse direction, etc.
		/// </summary>
		/// <param name="fraction"></param>
		/// <returns></returns>
		public Quaternion Subrotation(float fraction) {
			if (Math.Abs(fraction) < MathUtils.FlopsErrorMargin || this.EqualsWithTolerance(IDENTITY, MathUtils.FlopsErrorMargin)) return IDENTITY;
			float length = Norm;
			Vector4 xyzUnit = new Vector4(X, Y, Z, 0f).ToUnit();
			return (((Quaternion) xyzUnit) * (fraction * (float) Math.Acos(W / length))).Exponential * (float) Math.Pow(length, fraction);
		}

		/// <summary>
		/// Combines the two Quaternions, assuming they represent rotations, in to a single quaternion that is the equivalent to
		/// rotating an entity by <paramref name="rot1"/> and then immediately by <paramref name="rot2"/>.
		/// </summary>
		/// <param name="rot1">The first rotation Quaternion.</param>
		/// <param name="rot2">The second rotation Quaternion.</param>
		/// <returns><c>rot1 * rot2</c>.</returns>
		public static Quaternion CombineRotations(Quaternion rot1, Quaternion rot2) {
			return rot1 * rot2;
		}

		/// <summary>
		/// Combines the three Quaternions, assuming they represent rotations, in to a single quaternion that is the equivalent to
		/// rotating an entity by <paramref name="rot1"/> and then immediately by <paramref name="rot2"/>, and then immediately by
		/// <paramref name="rot3"/>.
		/// </summary>
		/// <param name="rot1">The first rotation Quaternion.</param>
		/// <param name="rot2">The second rotation Quaternion.</param>
		/// <param name="rot3">The third rotation Quaternion.</param>
		/// <returns><c>rot1 * rot2 * rot3</c>.</returns>
		public static Quaternion CombineRotations(Quaternion rot1, Quaternion rot2, Quaternion rot3) {
			return rot1 * rot2 * rot3;
		}

		/// <summary>
		/// Combines the given Quaternions, assuming they represent rotations, in to a single quaternion that is the equivalent to 
		/// rotating an entity by each single rotation in order.
		/// </summary>
		/// <param name="rotations">The array of Quaternion rotations to combine.</param>
		/// <returns>A new Quaternion that is the combined product of each individual rotation.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0",
			Justification = "Argument is validated by the Assurance.")]
		public static Quaternion CombineRotations(params Quaternion[] rotations) {
			Assure.NotNull(rotations);
			Assure.GreaterThan(rotations.Length, 0, "Can not supply an empty array to combine rotations.");
			Quaternion result = rotations[0];
			for (int i = 1; i < rotations.Length; i++) {
				result *= rotations[i];
			}
			return result;
		}

		/// <summary>
		/// Calculates the scalar product (dot product) of the two Quaternions.
		/// </summary>
		/// <param name="v1">The first operand.</param>
		/// <param name="v2">The second operand.</param>
		/// <returns><c>v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z + v1.W * v2.W</c></returns>
		public static float Dot(Quaternion v1, Quaternion v2) {
			return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z + v1.W * v2.W;
		}

		/// <summary>
		/// Performs a linear interpolation between two rotation Quaternions, returning a new Quaternion that represents a rotation
		/// somewhere between the <paramref name="start"/> rotation and the <paramref name="end"/> rotation (according to 
		/// <paramref name="distanceMultiplier"/>).
		/// </summary>
		/// <remarks>
		/// In comparison to <see cref="Slerp"/>, this method is faster, and works better for very small distances. However,
		/// over large changes in rotation, the transformation will exhibit non-linear speed between linear interstices of
		/// <paramref name="distanceMultiplier"/>. In these cases, it may be preferable to use <see cref="Slerp"/>.
		/// </remarks>
		/// <param name="start">The initial, starting rotation. Use <see cref="IDENTITY"/> for no rotation.</param>
		/// <param name="end">The destination, ending rotation.</param>
		/// <param name="distanceMultiplier">The distance from <paramref name="start"/> (towards <paramref name="end"/>)
		/// to interpolate to; where <c>0f</c> is all the way at the <paramref name="start"/> rotation, and
		/// <c>1f</c> is all the way at the <paramref name="end"/> rotation.</param>
		/// <param name="forceShortestPath">If true, the interpolation will take the shortest path between <paramref name="start"/>
		/// and <paramref name="end"/>. If false, the interpolation will take the route that always 'reverses' the rotation from <paramref name="start"/>
		/// 'back' to <paramref name="end"/>.</param>
		/// <returns>A rotation Quaternion that has been linearly interpolated according to the given
		/// parameters and then reprojected back on to the unit hypersphere.</returns>
		public static Quaternion LerpAndNormalize(Quaternion start, Quaternion end, float distanceMultiplier, bool forceShortestPath = true) {
			start = start.ToUnit() * 100f;
			end = end.ToUnit() * 100f;	
			if (forceShortestPath && Dot(start, end) < 0f) return (start + distanceMultiplier * ((end * -1f) - start)).ToUnit();
			else return start + distanceMultiplier * (end - start);
		}

		/// <summary>
		/// Performs a spherical linear interpolation between two rotation Quaternions, 
		/// returning a new Quaternion that represents a rotation somewhere between the 
		/// <paramref name="start"/> rotation and the <paramref name="end"/> rotation (according to 
		/// <paramref name="distanceMultiplier"/>).
		/// </summary>
		/// <remarks>
		/// In comparison to <see cref="LerpAndNormalize"/>, this method is slower. For small angles, the implementation
		/// of this method will defer to <see cref="LerpAndNormalize"/> anyway.
		/// However, transformations in rotation will always exhibit linear speed for linear interstices of
		/// <paramref name="distanceMultiplier"/>. When this is necessary, it is preferable to use this method instead of
		/// <see cref="LerpAndNormalize"/>.
		/// </remarks>
		/// <param name="start">The initial, starting rotation. Use <see cref="IDENTITY"/> for no rotation.</param>
		/// <param name="end">The destination, ending rotation.</param>
		/// <param name="distanceMultiplier">The distance from <paramref name="start"/> (towards <paramref name="end"/>)
		/// to interpolate to; where <c>0f</c> is all the way at the <paramref name="start"/> rotation, and
		/// <c>1f</c> is all the way at the <paramref name="end"/> rotation.</param>
		/// <param name="forceShortestPath">If true, the interpolation will take the shortest path between <paramref name="start"/>
		/// and <paramref name="end"/>. If false, the interpolation will take the route that always 'reverses' the rotation from <paramref name="start"/>
		/// 'back' to <paramref name="end"/>.</param>
		/// <returns>A rotation Quaternion that has been linearly interpolated across the surface of a 4-sphere 
		/// according to the given parameters.</returns>
		public static Quaternion Slerp(Quaternion start, Quaternion end, float distanceMultiplier, bool forceShortestPath = true) {
			start = start.ToUnit() * 100f;
			end = end.ToUnit() * 100f;

			if ((start - end).NormSquared > (end - start).NormSquared) end = end * -1f;

			float cosAngleBetween = Dot(start, end);

			if (Math.Abs(cosAngleBetween) > SLERP_MAX_COS_PHI_BEFORE_LERP) return LerpAndNormalize(start, end, distanceMultiplier);

			float angleBetween = (float) Math.Acos(cosAngleBetween);
			float sinAngleBetween = (float) Math.Sin(angleBetween);

			float forceShortestMultiplier = (forceShortestPath && cosAngleBetween < 0f ? -1f : 1f);

			return ((float) (Math.Sin(angleBetween * (1f - distanceMultiplier)) / sinAngleBetween) * start
				+ (float) (Math.Sin(angleBetween * distanceMultiplier) / sinAngleBetween) * forceShortestMultiplier * end).ToUnit();
		}

		/// <summary>
		/// Rotates the given <paramref name="vector"/> around the origin by the given <paramref name="rotation"/>.
		/// </summary>
		/// <param name="vector">The vector to rotate around the origin.</param>
		/// <param name="rotation">The rotation to apply.</param>
		/// <returns>A new vector that is the input <paramref name="vector"/> rotated around the origin by <paramref name="rotation"/>.</returns>
		public static Vector3 RotateVector(Vector3 vector, Quaternion rotation) {
			Vector3 rotationXYZ = new Vector3(rotation.X, rotation.Y, rotation.Z);
			Vector3 t = 2f * Vector3.Cross(rotationXYZ, vector);
			return vector + rotation.W * t + Vector3.Cross(rotationXYZ, t);
		}

		/// <summary>
		/// Finds the Quaternion that represents a rotation from <paramref name="start"/> to <paramref name="end"/>.
		/// </summary>
		/// <param name="start">The starting rotation.</param>
		/// <param name="end">The ending rotation.</param>
		/// <returns>A new Quaternion that will rotate a given entity or vector from
		/// <paramref name="start"/> to <paramref name="end"/> when applied.</returns>
		public static Quaternion RotationBetween(Quaternion start, Quaternion end) {
			return -start * end;
		}

		/// <summary>
		/// Finds the distance between the two given rotations, in radians.
		/// </summary>
		/// <param name="start">The starting rotation.</param>
		/// <param name="end">The ending rotation.</param>
		/// <returns>The smallest distance between the two rotations, in radians.</returns>
		public static float DistanceBetween(Quaternion start, Quaternion end) {
			Quaternion rotationBetween = RotationBetween(start, end);
			return Math.Abs(2f * (float) Math.Atan2(rotationBetween.Norm, rotationBetween.W));
		}

		/// <summary>
		/// Finds the distance between the two given rotations, in radians; and additionally finds the Quaternion that
		/// represents a rotation from <paramref name="start"/> to <paramref name="end"/>.
		/// </summary>
		/// <remarks>
		/// This method essentially combines <see cref="RotationBetween"/> and
		/// <see cref="DistanceBetween(Ophidian.Losgap.Quaternion,Ophidian.Losgap.Quaternion)"/> in to one method; and is slightly more efficient
		/// than calling both the aforementioned methods separately.
		/// </remarks>
		/// <param name="start">The starting rotation.</param>
		/// <param name="end">The ending rotation.</param>
		/// <param name="rotationBetween">A new Quaternion that will rotate a given entity or vector from
		/// <paramref name="start"/> to <paramref name="end"/> when applied.</param>
		/// <returns>The smallest distance between the two rotations, in radians.</returns>
		public static float DistanceBetween(Quaternion start, Quaternion end, out Quaternion rotationBetween) {
			rotationBetween = RotationBetween(start, end);
			return Math.Abs(2f * (float) Math.Atan2(rotationBetween.Norm, rotationBetween.W));
		}

		/// <summary>
		/// Returns a unit-length quaternion with the same inter-component proportions.
		/// </summary>
		/// <returns>A unit length Quaternion.</returns>
		public Quaternion ToUnit() {
			Assure.False(EqualsExactly(ZERO), "Zero quaternion has no corresponding unit.");
			if (IsUnit) return this;
			float magnitude = Norm;
			return new Quaternion(X / magnitude, Y / magnitude, Z / magnitude, W / magnitude);
		}

		/// <summary>
		/// Adds the given <paramref name="rotation"/> Quaternion to this one, returning a new Quaternion that
		/// is the combination of both.
		/// </summary>
		/// <param name="rotation">The rotation to add to this one.</param>
		/// <returns><c>CombineRotations(this, rotation)</c>.</returns>
		public Quaternion AddRotation(Quaternion rotation) {
			return CombineRotations(this, rotation);
		}

		/// <summary>
		/// Subtracts the given <paramref name="rotation"/> Quaternion from this one, returning a new Quaternion that
		/// is the combination of this rotation and the inverse of the given parameter.
		/// </summary>
		/// <param name="rotation">The rotation to remove from this one.</param>
		/// <returns><c>CombineRotations(this, -rotation)</c>.</returns>
		public Quaternion RemoveRotation(Quaternion rotation) {
			return CombineRotations(this, -rotation);
		}

		/// <summary>
		/// Adds each component in each Quaternion to create a new Quaternion that is the sum of both operands.
		/// </summary>
		/// <param name="lhs">The first operand.</param>
		/// <param name="rhs">The second operand.</param>
		/// <returns><c>&lt;[lhs.X + rhs.X, lhs.Y + rhs.Y, lhs.Z + rhs.Z] : lhs.W + rhs.W&gt;</c></returns>
		public static Quaternion operator +(Quaternion lhs, Quaternion rhs) {
			return new Quaternion(lhs.X + rhs.X, lhs.Y + rhs.Y, lhs.Z + rhs.Z, lhs.W + rhs.W);
		}

		/// <summary>
		/// Subtracts each component in <paramref name="rhs"/> from the corresponding component in
		/// <paramref name="lhs"/> to create a new Quaternion that is the difference of both operands.
		/// </summary>
		/// <param name="lhs">The first operand.</param>
		/// <param name="rhs">The second operand.</param>
		/// <returns><c>&lt;[lhs.X - rhs.X, lhs.Y - rhs.Y, lhs.Z - rhs.Z] : lhs.W - rhs.W&gt;</c></returns>
		public static Quaternion operator -(Quaternion lhs, Quaternion rhs) {
			return new Quaternion(lhs.X - rhs.X, lhs.Y - rhs.Y, lhs.Z - rhs.Z, lhs.W - rhs.W);
		}

		public static Quaternion operator ^(Quaternion lhs, float rhs) {
			return lhs.Subrotation(rhs);
		}

		/// <summary>
		/// Multiplies the two Quaternions together. Quaternion multiplication is not commutative (e.g.
		/// <c>lhs * rhs</c> is not necessarily equal to <c>rhs * lhs</c>).
		/// </summary>
		/// <param name="lhs">The first quaternion.</param>
		/// <param name="rhs">The second quaternion.</param>
		/// <returns><c>&lt;[av + bu + u X v] : ab - u . v&gt;</c> where u and v are the imaginary parts of
		/// <paramref name="lhs"/> and <paramref name="rhs"/> respectively, and a and b are the real parts of 
		/// <paramref name="lhs"/> and <paramref name="rhs"/> respectively.</returns>
		public static Quaternion operator *(Quaternion lhs, Quaternion rhs) {
			return new Quaternion(
				(rhs.Y * lhs.Z - rhs.Z * lhs.Y) + rhs.W * lhs.X + lhs.W * rhs.X,
				(rhs.Z * lhs.X - rhs.X * lhs.Z) + rhs.W * lhs.Y + lhs.W * rhs.Y,
				(rhs.X * lhs.Y - rhs.Y * lhs.X) + rhs.W * lhs.Z + lhs.W * rhs.Z,
				rhs.W * lhs.W - (rhs.X * lhs.X + rhs.Y * lhs.Y + rhs.Z * lhs.Z)
			);
		}

		/// <summary>
		/// Multiplies every component in <paramref name="lhs"/> by the scalar <paramref name="rhs"/>.
		/// </summary>
		/// <param name="lhs">The quaternion.</param>
		/// <param name="rhs">The scalar.</param>
		/// <returns><c>new Quaternion(lhs.X * rhs, lhs.Y * rhs, lhs.Z * rhs, lhs.W * rhs)</c></returns>
		public static Quaternion operator *(Quaternion lhs, float rhs) {
			return new Quaternion(lhs.X * rhs, lhs.Y * rhs, lhs.Z * rhs, lhs.W * rhs);
		}

		/// <summary>
		/// Multiplies every component in <paramref name="rhs"/> by the scalar <paramref name="lhs"/>.
		/// </summary>
		/// <param name="lhs">The scalar.</param>
		/// <param name="rhs">The quaternion.</param>
		/// <returns><c>new Quaternion(rhs.X * lhs, rhs.Y * lhs, rhs.Z * lhs, rhs.W * lhs)</c></returns>
		public static Quaternion operator *(float lhs, Quaternion rhs) {
			return new Quaternion(rhs.X * lhs, rhs.Y * lhs, rhs.Z * lhs, rhs.W * lhs);
		}

		/// <summary>
		/// Returns the result of rotating <paramref name="lhs"/> around the origin by <paramref name="rhs"/>.
		/// </summary>
		/// <param name="lhs">The vector to rotate.</param>
		/// <param name="rhs">The rotation to apply.</param>
		/// <returns><c>Quaternion.RotateVector(lhs, rhs)</c>.</returns>
		public static Vector3 operator *(Vector3 lhs, Quaternion rhs) {
			return RotateVector(lhs, rhs);
		}

		/// <summary>
		/// Returns the result of rotating <paramref name="rhs"/> around the origin by <paramref name="lhs"/>.
		/// </summary>
		/// <param name="lhs">The rotation to apply.</param>
		/// <param name="rhs">The vector to rotate.</param>
		/// <returns><c>Quaternion.RotateVector(rhs, lhs)</c>.</returns>
		public static Vector3 operator *(Quaternion lhs, Vector3 rhs) {
			return RotateVector(rhs, lhs);
		}

		/// <summary>
		/// Divides every component in <paramref name="lhs"/> by the scalar <paramref name="rhs"/>.
		/// </summary>
		/// <param name="lhs">The quaternion.</param>
		/// <param name="rhs">The scalar.</param>
		/// <returns><c>new Quaternion(lhs.X / rhs, lhs.Y / rhs, lhs.Z / rhs, lhs.W / rhs)</c></returns>
		public static Quaternion operator /(Quaternion lhs, float rhs) {
			return new Quaternion(lhs.X / rhs, lhs.Y / rhs, lhs.Z / rhs, lhs.W / rhs);
		}

		/// <summary>
		/// Returns the conjugate of <paramref name="operand"/>.
		/// </summary>
		/// <param name="operand">The quaternion whose conjugate it is you wish to obtain.</param>
		/// <returns><c>operand.Conjugate</c></returns>
		public static Quaternion operator ~(Quaternion operand) {
			return operand.Conjugate;
		}

		/// <summary>
		/// Returns the inverse of <paramref name="operand"/>.
		/// </summary>
		/// <param name="operand">The quaternion whose inverse it is you wish to obtain.</param>
		/// <returns><c>operand.Inverse</c></returns>
		public static Quaternion operator -(Quaternion operand) {
			return operand.Inverse;
		}
	}
}