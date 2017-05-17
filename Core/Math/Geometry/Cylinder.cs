// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 28 11 2014 at 14:40 by Ben Bowen

using System;
using System.Collections.Generic;

namespace Ophidian.Losgap {
	/// <summary>
	/// Represents a right-cylinder, oriented with the base/top pointing along the Y-axis, in 3D space.
	/// </summary>
	public struct Cylinder : IToleranceEquatable<Cylinder> {
		/// <summary>
		/// The position of the centre of this cylinder in 3D space.
		/// </summary>
		public readonly Vector3 Center;

		/// <summary>
		/// The size of this cylinder along the Y axis.
		/// </summary>
		public readonly float Height;

		/// <summary>
		/// The size of this cylinder along the XZ plane.
		/// </summary>
		public readonly float Radius;

		/// <summary>
		/// The total area covered by the surface of this cylinder.
		/// </summary>
		public float SurfaceArea {
			get { return MathUtils.TWO_PI * Radius * Height + MathUtils.TWO_PI * Radius * Radius; }
		}

		/// <summary>
		/// The entire volume taken up by this cylinder.
		/// </summary>
		public float Volume {
			get { return MathUtils.PI * Radius * Radius * Height; }
		}

		/// <summary>
		/// Creates a new cylinder with the given <paramref name="center"/> position, <paramref name="radius"/>, 
		/// and <paramref name="height"/>.
		/// </summary>
		/// <param name="center">The position of the centre of this cylinder.</param>
		/// <param name="radius">The size of this cylinder along the XZ plane.</param>
		/// <param name="height">The size of this cylinder along the Y axis.</param>
		public Cylinder(Vector3 center, float radius, float height) {
			Center = center;
			Height = Math.Abs(height);
			Radius = Math.Abs(radius);
		}

		/// <summary>
		/// Creates a new cylinder with the given centre position (<paramref name="x"/>, <paramref name="y"/>, and <paramref name="z"/>), 
		/// <paramref name="radius"/>, and <paramref name="height"/>.
		/// </summary>
		/// <param name="x">The position on the x-axis of the centre of this cylinder.</param>
		/// <param name="y">The position on the y-axis of the centre of this cylinder.</param>
		/// <param name="z">The position on the z-axis of the centre of this cylinder.</param>
		/// <param name="radius">The size of this cylinder along the XZ plane.</param>
		/// <param name="height">The size of this cylinder along the Y axis.</param>
		public Cylinder(float x, float y, float z, float radius, float height) : this(new Vector3(x, y, z), radius, height) { }

		/// <summary>
		/// Returns a new <see cref="Cone"/> that has the exact same proportions and position as this cylinder.
		/// </summary>
		/// <returns>A cone whose <see cref="Cone.TopCenter"/> and <see cref="Cone.Height"/> properties are set according 
		/// to this cylinder's <see cref="Center"/> position and <see cref="Height"/>;
		/// and whose <see cref="Cone.TopRadius"/> and <see cref="Cone.BottomRadius"/> 
		/// are equal to this cylinder's <see cref="Radius"/>.</returns>
		public Cone ToSymmetricalConicalFrustum() {
			return new Cone(Center.X, Center.Y + Height / 2f, Center.Z, Radius, Height, Radius);
		}


		#region Shape interactions
		/// <summary>
		/// Indicates whether or not the given <paramref name="point"/> lies inside this cylinder.
		/// </summary>
		/// <param name="point">The point to check.</param>
		/// <returns>True if the given <paramref name="point"/> is inside this cylinder, false if it is outside.</returns>
		public bool Contains(Vector3 point) {
			float errorMargin = MathUtils.FlopsErrorMargin;
			return Vector3.DistanceSquared(Center[0, 2], point[0, 2]) <= Radius * Radius + errorMargin * errorMargin
				&& Math.Abs(point.Y - Center.Y) <= Height / 2f + errorMargin;
		}

		/// <summary>
		/// Indicates whether or not the given <paramref name="ray"/> is contained entirely within this cylinder.
		/// </summary>
		/// <param name="ray">The ray to check.</param>
		/// <returns>True if the <paramref name="ray"/> is not <see cref="Ray.IsInfiniteLength">infinite length</see> and its
		/// <see cref="Ray.StartPoint"/> and <see cref="Ray.EndPoint"/> are inside this shape.</returns>
		public bool Contains(Ray ray) {
			return !ray.IsInfiniteLength && Contains(ray.StartPoint) && Contains(ray.EndPoint.Value);
		}

		/// <summary>
		/// Indicates whether or not the given shape is contained entirely within this cylinder (no part of the <paramref name="other"/>
		/// shape may be outside this one).
		/// </summary>
		/// <param name="other">The shape to check.</param>
		/// <returns>True if the other shape is completely within this cylinder, false if any part of it
		/// is outside the cylinder.</returns>
		public bool Contains(Cylinder other) {
			float errorMargin = MathUtils.FlopsErrorMargin;
			return Vector3.Distance(Center[0, 2], other.Center[0, 2]) + other.Radius <= Radius + errorMargin
				&& Math.Abs(Center.Y - other.Center.Y) + other.Height / 2f <= Height / 2f + errorMargin;
		}

		/// <summary>
		/// Indicates whether or not the given shape is contained entirely within this cylinder (no part of the <paramref name="other"/>
		/// shape may be outside this one).
		/// </summary>
		/// <param name="other">The shape to check.</param>
		/// <returns>True if the other shape is completely within this cylinder, false if any part of it
		/// is outside the cylinder.</returns>
		public bool Contains(Cone other) {
			float errorMargin = MathUtils.FlopsErrorMargin;
			float halfHeightPlusErrorMargin = Height / 2f + errorMargin;
			return Vector3.Distance(Center[0, 2], other.TopCenter[0, 2])
				+ Math.Max(other.TopRadius, other.BottomRadius) <= Radius + errorMargin
				&& Math.Abs(Center.Y - other.TopCenter.Y) <= halfHeightPlusErrorMargin
				&& Math.Abs(Center.Y - (other.TopCenter.Y - other.Height)) <= halfHeightPlusErrorMargin;
		}

		/// <summary>
		/// Indicates whether or not the given shape is contained entirely within this cylinder (no part of the <paramref name="other"/>
		/// shape may be outside this one).
		/// </summary>
		/// <param name="other">The shape to check.</param>
		/// <returns>True if the other shape is completely within this cylinder, false if any part of it
		/// is outside the cylinder.</returns>
		public bool Contains(Cuboid other) {
			float halfHeight = Height / 2f;
			float errorMargin = MathUtils.FlopsErrorMargin;
			return Contains(other.GetCorner(Cuboid.CuboidCorner.FrontTopLeft))
				&& Contains(other.GetCorner(Cuboid.CuboidCorner.FrontTopRight))
				&& Contains(other.GetCorner(Cuboid.CuboidCorner.BackTopLeft))
				&& Contains(other.GetCorner(Cuboid.CuboidCorner.BackTopRight))
				&& other.FrontBottomLeft.Y + other.Height <= Center.Y + halfHeight + errorMargin
				&& other.FrontBottomLeft.Y + errorMargin >= Center.Y - halfHeight;
		}

		/// <summary>
		/// Indicates whether or not the given shape is contained entirely within this cylinder (no part of the <paramref name="other"/>
		/// shape may be outside this one).
		/// </summary>
		/// <param name="other">The shape to check.</param>
		/// <returns>True if the other shape is completely within this cylinder, false if any part of it
		/// is outside the cylinder.</returns>
		public bool Contains(Sphere other) {
			float halfHeight = Height / 2f;
			float halfRadius = other.Radius / 2f;
			float errorMargin = MathUtils.FlopsErrorMargin;
			return Vector3.Distance(Center[0, 2], other.Center[0, 2]) + other.Radius <= Radius + errorMargin
				&& other.Center.Y + halfRadius <= Center.Y + halfHeight + errorMargin
				&& other.Center.Y - halfRadius + errorMargin >= Center.Y - halfHeight;
		}

		/// <summary>
		/// Indicates whether any portion of this cylinder and the <paramref name="other"/> shape overlap.
		/// </summary>
		/// <param name="other">The shape to check against this one.</param>
		/// <returns>True if any part of both shapes overlap, false if they are entirely separate.</returns>
		public bool Intersects(Cylinder other) {
			float errorMargin = MathUtils.FlopsErrorMargin;
			return Vector3.Distance(Center[0, 2], other.Center[0, 2]) <= Radius + other.Radius + errorMargin
				&& Math.Abs(Center.Y - other.Center.Y) <= Height / 2f + other.Height / 2f + errorMargin;
		}

		/// <summary>
		/// Indicates whether any portion of this cylinder and the <paramref name="other"/> shape overlap.
		/// </summary>
		/// <param name="other">The shape to check against this one.</param>
		/// <returns>True if any part of both shapes overlap, false if they are entirely separate.</returns>
		public bool Intersects(Cuboid other) {
			return other.Intersects(this);
		}

		/// <summary>
		/// Indicates whether any portion of this cylinder and the <paramref name="other"/> shape overlap.
		/// </summary>
		/// <param name="other">The shape to check against this one.</param>
		/// <returns>True if any part of both shapes overlap, false if they are entirely separate.</returns>
		public bool Intersects(Sphere other) {
			return other.Intersects(this);
		}

		/// <summary>
		/// Indicates whether any portion of this cylinder and the <paramref name="other"/> shape overlap.
		/// </summary>
		/// <param name="other">The shape to check against this one.</param>
		/// <returns>True if any part of both shapes overlap, false if they are entirely separate.</returns>
		public bool Intersects(Cone other) {
			return other.Intersects(this);
		}
		#endregion

		/// <summary>
		/// Returns a string representation of this Cylinder.
		/// </summary>
		/// <returns>A string that lists the geometric properties of this shape.</returns>
		public override string ToString() {
			return "(Cylinder Centre:" + Center + " Height:" + Height.ToString(3) + " Radius:" + Radius + ")";
		}

		#region Equality
		/// <summary>
		/// Test whether this object is EXACTLY equal to <paramref name="other"/>. No floating-point tolerance will be accounted
		/// for: The two structures must be bitwise identical.
		/// </summary>
		/// <param name="other">The other Cylinder to test against.</param>
		/// <returns>True if this object is EXACTLY equal to <paramref name="other"/>.</returns>
		public bool EqualsExactly(Cylinder other) {
			// ReSharper disable CompareOfFloatsByEqualityOperator
			return Center.EqualsExactly(other.Center)
				&& Height == other.Height
				&& Radius == other.Radius;
			// ReSharper restore CompareOfFloatsByEqualityOperator
		}

		/// <summary>
		/// Test whether this object is equal to <paramref name="other"/> within a given <paramref name="tolerance"/>. Even if the
		/// two structures are not identical, this method will return <c>true</c> if they are 'close enough' (e.g. within the tolerance).
		/// </summary>
		/// <param name="other">The other Cylinder to test against.</param>
		/// <param name="tolerance">The amount of tolerance to give. For any floating-point component in both objects, if the difference
		/// between them is less than this value, they will be considered equal.</param>
		/// <returns>True if this object is equal to <paramref name="other"/> with the given <paramref name="tolerance"/>.</returns>
		public bool EqualsWithTolerance(Cylinder other, double tolerance) {
			Assure.GreaterThanOrEqualTo(tolerance, 0f, "Tolerance must not be negative.");
			return Center.EqualsWithTolerance(other.Center, tolerance)
				&& Math.Abs(Height - other.Height) < tolerance
				&& Math.Abs(Radius - other.Radius) < tolerance;
		}
		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(Cylinder other) {
			return EqualsExactly(other);
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
			return obj is Cylinder && Equals((Cylinder) obj);
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
				int hashCode = Center.GetHashCode();
				hashCode = (hashCode * 397) ^ Height.GetHashCode();
				hashCode = (hashCode * 397) ^ Radius.GetHashCode();
				return hashCode;
			}
		}

		/// <summary>
		/// Indicates whether or not the two shapes are equal.
		/// </summary>
		/// <param name="left">The first shape.</param>
		/// <param name="right">The second shape.</param>
		/// <returns>True if the shapes represent equal dimensions and positions, false if not.</returns>
		public static bool operator ==(Cylinder left, Cylinder right) {
			return left.Equals(right);
		}

		/// <summary>
		/// Indicates whether or not the two shapes are unequal.
		/// </summary>
		/// <param name="left">The first shape.</param>
		/// <param name="right">The second shape.</param>
		/// <returns>False if the shapes represent equal dimensions and positions, true if not.</returns>
		public static bool operator !=(Cylinder left, Cylinder right) {
			return !left.Equals(right);
		}
		#endregion

		/// <summary>
		/// Converts the given <paramref name="operand"/> to a symmetrical conical frustum.
		/// The given cone has exactly the same properties as the cylinder.
		/// </summary>
		/// <param name="operand">The cylinder to convert.</param>
		/// <returns><c>operand.ToSymmetricalConicalFrustum()</c>.</returns>
		public static implicit operator Cone(Cylinder operand) {
			return operand.ToSymmetricalConicalFrustum();
		}
	}
}