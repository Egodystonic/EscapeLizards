// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 23 01 2015 at 17:14 by Ben Bowen

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap {
	/// <summary>
	/// Represents the transformation applied to a single object, consisting of a <see cref="Scale"/>, followed by a <see cref="Rotation"/>, then
	/// finally a <see cref="Translation"/>.
	/// </summary>
	/// <remarks>
	/// Transform objects are immutable, but you can create mutated copies easily using the <see cref="With"/> method.
	/// </remarks>
	[StructLayout(LayoutKind.Explicit, Pack = (int) InteropUtils.StructPacking.Optimal, Size = Vector4.XMVECTOR_ALIGNMENT * 3)]
	public struct Transform : IToleranceEquatable<Transform> {
		/// <summary>
		/// The default transform, with no scaling, rotation, or translation applied (<see cref="Scale"/> is <see cref="Vector3.ONE"/>,
		/// <see cref="Rotation"/> is <see cref="Quaternion.IDENTITY"/>, <see cref="Translation"/> is <see cref="Vector3.ZERO"/>).
		/// </summary>
		public static readonly Transform DEFAULT_TRANSFORM = new Transform(Vector3.ONE, Quaternion.IDENTITY, Vector3.ZERO);
		/// <summary>
		/// The scale (sizing) applied in this transform.
		/// </summary>
		[FieldOffset(Vector4.XMVECTOR_ALIGNMENT * 0)]
		public readonly Vector3 Scale;
		/// <summary>
		/// The rotation (orientation offset) applied in this transform.
		/// </summary>
		[FieldOffset(Vector4.XMVECTOR_ALIGNMENT * 1)]
		public readonly Quaternion Rotation;
		/// <summary>
		/// The translation (displacement from the origin) applied in this transform.
		/// </summary>
		[FieldOffset(Vector4.XMVECTOR_ALIGNMENT * 2)]
		public readonly Vector3 Translation;

		/// <summary>
		/// Creates a Scale/Rotation/Translation matrix from this Transform.
		/// </summary>
		public Matrix AsMatrix {
			get {
				return Matrix.FromSRT(Scale, Rotation, Translation);
			}
		}

		/// <summary>
		/// Creates a transposed (column-major) Scale/Rotation/Translation matrix from this Transform.
		/// </summary>
		public Matrix AsMatrixTransposed {
			get {
				return Matrix.FromSRTTransposed(Scale, Rotation, Translation);
			}
		}

		/// <summary>
		/// Constructs a new Transform struct.
		/// </summary>
		/// <param name="scale">The scale (sizing) to apply.</param>
		/// <param name="rotation">The rotation (orientation offset) to apply.</param>
		/// <param name="translation">The translation (displacement from the origin) to apply.</param>
		public Transform(Vector3 scale, Quaternion rotation, Vector3 translation) {
			Assure.NotEqual(rotation, Quaternion.ZERO);
			Scale = scale;
			Rotation = rotation;
			Translation = translation;
		}

		public static Transform Parse(string transformString) {
			Assure.NotNull(transformString);
			try {
				string[] components = transformString.Trim('(', ')').Split('|');
				Vector3 scale = Vector3.Parse(components[0].Split(':')[1].Trim());
				Quaternion rotation = Quaternion.Parse(components[1].Split(':')[1].Trim());
				Vector3 translation = Vector3.Parse(components[2].Split(':')[1].Trim());

				return new Transform(scale, rotation, translation);
			}
			catch (Exception e) {
				throw new FormatException("Could not parse transform string.", e);
			}
		}

		public static Transform Lerp(Transform a, Transform b, float amount) {
			return new Transform(
				Vector3.Lerp(a.Scale, b.Scale, amount),
				Quaternion.LerpAndNormalize(a.Rotation, b.Rotation, amount),
				Vector3.Lerp(a.Translation, b.Translation, amount)
			);
		}

		public static Transform Slerp(Transform a, Transform b, float amount) {
			return new Transform(
				Vector3.Lerp(a.Scale, b.Scale, amount),
				Quaternion.Slerp(a.Rotation, b.Rotation, amount),
				Vector3.Lerp(a.Translation, b.Translation, amount)
			);
		}

		/// <summary>
		/// Creates a new Transform that is a copy of this one, but with the specified parameters overridden with new values.
		/// </summary>
		/// <remarks>
		/// Example usage:
		/// <code>
		/// Translation t = new Translation(Vector3.ONE * 2f, Quaternion.IDENTITY, new Vector3(10f, -3f, 1f));
		/// Translation fiveMetresForward = t.With(translation: t.Translation + Vector3.FORWARD * 5f);
		/// Translation originalScale = t.With(scale: Vector3.ONE);
		/// Translation upsideDown = t.With(rotation: Quaternion.FromAxialRotation(Vector3.FORWARD, MathUtils.PI));
		/// </code>
		/// </remarks>
		/// <param name="scale">The new scale. Leave as <c>null</c> to use the current <see cref="Scale"/>.</param>
		/// <param name="rotation">The new rotation. Leave as <c>null</c> to use the current <see cref="Rotation"/>.</param>
		/// <param name="translation">The new translation. Leave as <c>null</c> to use the current <see cref="Translation"/>.</param>
		/// <returns>A new Transform with the same properties as this one, except where parameters have been specified and overridden.</returns>
		public Transform With(Vector3? scale = null, Quaternion? rotation = null, Vector3? translation = null) {
			return new Transform(scale ?? Scale, rotation ?? Rotation, translation ?? Translation);
		}

		public Transform ScaleBy(float scaleSize) {
			return new Transform(Scale * scaleSize, Rotation, Translation);
		}

		public Transform ScaleBy(Vector3 perAxisScaleSize) {
			return new Transform(Scale.Scale(perAxisScaleSize), Rotation, Translation);
		}

		public Transform RotateBy(float radians, Vector3 axis) {
			return new Transform(Scale, Rotation * Quaternion.FromAxialRotation(axis, radians), Translation);
		}

		public Transform RotateBy(Quaternion rotation) {
			return new Transform(Scale, Rotation * rotation, Translation);
		}

		public Transform RotateAround(Vector3 pivotPoint, Quaternion rotation) {
			return new Transform(Scale, Rotation * rotation, pivotPoint + (Translation - pivotPoint) * rotation);
		}

		public Transform TranslateBy(Vector3 translation) {
			return new Transform(Scale, Rotation, Translation + translation);
		}


		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(Transform other) {
			return Scale.Equals(other.Scale) && Rotation.Equals(other.Rotation) && Translation.Equals(other.Translation);
		}

		/// <summary>
		/// Test whether this object is EXACTLY equal to <paramref name="other"/>. No floating-point tolerance will be accounted
		/// for: The two structures must be bitwise identical.
		/// </summary>
		/// <param name="other">The other Transform to test against.</param>
		/// <returns>True if this object is EXACTLY equal to <paramref name="other"/>.</returns>
		public bool EqualsExactly(Transform other) {
			return Scale.EqualsExactly(other.Scale) && Rotation.EqualsExactly(other.Rotation) && Translation.EqualsExactly(other.Translation);
		}

		/// <summary>
		/// Test whether this object is equal to <paramref name="other"/> within a given <paramref name="tolerance"/>. Even if the
		/// two structures are not identical, this method will return <c>true</c> if they are 'close enough' (e.g. within the tolerance).
		/// </summary>
		/// <param name="other">The other Transform to test against.</param>
		/// <param name="tolerance">The amount of tolerance to give. For any floating-point component in both objects, if the difference
		/// between them is less than this value, they will be considered equal.</param>
		/// <returns>True if this object is equal to <paramref name="other"/> with the given <paramref name="tolerance"/>.</returns>
		public bool EqualsWithTolerance(Transform other, double tolerance) {
			return Scale.EqualsWithTolerance(other.Scale, tolerance)
				&& Rotation.EqualsWithTolerance(other.Rotation, tolerance)
				&& Translation.EqualsWithTolerance(other.Translation, tolerance);
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
			return obj is Transform && Equals((Transform)obj);
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
				int hashCode = Scale.GetHashCode();
				hashCode = (hashCode * 397) ^ Rotation.GetHashCode();
				hashCode = (hashCode * 397) ^ Translation.GetHashCode();
				return hashCode;
			}
		}

		/// <summary>
		/// Ascertains whether or not the two Transforms are equal (within a tolerance of <see cref="MathUtils.FlopsErrorMargin"/>).
		/// </summary>
		/// <param name="left">The first Transform.</param>
		/// <param name="right">The second Transform.</param>
		/// <returns>True if the Transforms are equal within the <see cref="MathUtils.FlopsErrorMargin">error margin</see>), false if not.</returns>
		public static bool operator ==(Transform left, Transform right) {
			return left.Equals(right);
		}

		/// <summary>
		/// Ascertains whether or not the two Transforms are different (outside a tolerance of <see cref="MathUtils.FlopsErrorMargin"/>).
		/// </summary>
		/// <param name="left">The first Transform.</param>
		/// <param name="right">The second Transform.</param>
		/// <returns>True if the Transforms are different (more than the <see cref="MathUtils.FlopsErrorMargin">error margin</see>), false if not.</returns>
		public static bool operator !=(Transform left, Transform right) {
			return !left.Equals(right);
		}

		public static Transform operator +(Transform left, Transform right) {
			return new Transform(
				left.Scale.Scale(right.Scale),
				left.Rotation * right.Rotation,
				left.Translation + right.Translation
			);
		}

		public static Transform operator -(Transform left, Transform right) {
			return new Transform(
				left.Scale.Scale(new Vector3(1f / right.Scale.X, 1f / right.Scale.Y, 1f / right.Scale.Z)),
				left.Rotation * -right.Rotation,
				left.Translation - right.Translation
			);
		}

		public static Vector3 operator *(Vector3 vect, Transform transform) {
			return vect.Scale(transform.Scale).RotateBy(transform.Rotation) + transform.Translation;
		}
		public static Vector3 operator *(Transform transform, Vector3 vect) {
			return vect.Scale(transform.Scale).RotateBy(transform.Rotation) + transform.Translation;
		}

		/// <summary>
		/// Creates a string that represents this Transform.
		/// </summary>
		/// <returns>A string that represents this Transform.</returns>
		public override string ToString() {
			return ToString(3);
		}

		public string ToString(int numDecimalPlaces) {
			return "Transform (Scale: " + Scale.ToString(numDecimalPlaces) + " | Rotation: " + Rotation.ToString(numDecimalPlaces) + " | Translation: " + Translation.ToString(numDecimalPlaces) + ")";
		}
	}
}