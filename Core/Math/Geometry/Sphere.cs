// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 26 11 2014 at 14:22 by Ben Bowen

using System;
using System.Collections.Generic;

namespace Ophidian.Losgap {
	/// <summary>
	/// Represents a three-dimensional sphere.
	/// </summary>
	public struct Sphere : IToleranceEquatable<Sphere> {
		/// <summary>
		/// The centre of the sphere in 3D space.
		/// </summary>
		public readonly Vector3 Center;
		/// <summary>
		/// The size of this sphere along any axis.
		/// </summary>
		public readonly float Radius;

		/// <summary>
		/// The entire space taken up by this sphere.
		/// </summary>
		public float Volume {
			get {
				return (4f / 3f) * MathUtils.PI * Radius * Radius * Radius;
			}
		}

		/// <summary>
		/// The area covered by the surface of this sphere.
		/// </summary>
		public float SurfaceArea {
			get {
				return 4f * MathUtils.PI * Radius * Radius;
			}
		}

		public float Circumference {
			get {
				return 2f * MathUtils.PI * Radius;
			}
		}

		public float Diameter {
			get {
				return 2f * Radius;
			}
		}

		/// <summary>
		/// Creates a new sphere with the given <paramref name="center"/> position and <paramref name="radius"/>.
		/// </summary>
		/// <param name="center">The position of the centre of this sphere.</param>
		/// <param name="radius">The size of this sphere along any axis.</param>
		public Sphere(Vector3 center, float radius) {
			Center = center;
			Radius = Math.Abs(radius);
		}

		/// <summary>
		/// Creates a new sphere with the given centre position (<paramref name="x"/>, <paramref name="y"/>, and <paramref name="z"/>)
		/// and <paramref name="radius"/>.
		/// </summary>
		/// <param name="x">The position on the x-axis of the centre of this sphere.</param>
		/// <param name="y">The position on the y-axis of the centre of this sphere.</param>
		/// <param name="z">The position on the z-axis of the centre of this sphere.</param>
		/// <param name="radius">The size of this sphere along any axis.</param>
		public Sphere(float x, float y, float z, float radius) : this(new Vector3(x, y, z), radius) { }

		/// <summary>
		/// Creates a new sphere by adding a third positional dimension (<paramref name="z"/>)
		/// to a pre-existing <see cref="Circle"/> (<paramref name="greatCircle"/>).
		/// </summary>
		/// <param name="greatCircle">The circle to extrapolate. The circle's <see cref="Circle.Center"/> components
		/// will form the <see cref="Vector3.X"/> and <see cref="Vector3.Y"/> co-ordinates of the <see cref="Center"/> of the new sphere,
		/// and the <see cref="Circle.Radius"/> property will form its <see cref="Radius"/>.</param>
		/// <param name="z">The position on the z-axis of the centre of this sphere.</param>
		public Sphere(Circle greatCircle, float z) : this(new Vector3(greatCircle.Center, z: z), greatCircle.Radius) { }	

		/// <summary>
		/// Gets the radius of the circle taken by slicing through this sphere at the given distance from its centre.
		/// </summary>
		/// <param name="distanceFromCentre">The distance from the centre to get the slice's radius for.</param>
		/// <returns>The radius of the circle at the given distance from the sphere's centre.</returns>
		/// <exception cref="AssuranceFailedException">Thrown if <paramref name="distanceFromCentre"/> is greater than
		/// <see cref="Radius"/>.</exception>
		public float GetRadius(float distanceFromCentre) {
			Assure.LessThanOrEqualTo(distanceFromCentre, Radius, "Distance from centre must be smaller than or equal to the sphere radius!");
			return (float) Math.Sqrt((Radius * Radius) - (distanceFromCentre * distanceFromCentre));
		}

		#region Shape interactions
		/// <summary>
		/// Indicates whether or not the given <paramref name="point"/> lies inside this sphere.
		/// </summary>
		/// <param name="point">The point to check.</param>
		/// <returns>True if the given <paramref name="point"/> is inside this sphere, false if it is outside.</returns>
		public bool Contains(Vector3 point) {
			float errorMargin = MathUtils.FlopsErrorMargin;
			return Vector3.DistanceSquared(Center, point) <= Radius * Radius + errorMargin * errorMargin;
		}

		/// <summary>
		/// Indicates whether or not the given <paramref name="ray"/> is contained entirely within this sphere.
		/// </summary>
		/// <param name="ray">The ray to check.</param>
		/// <returns>True if the <paramref name="ray"/> is not <see cref="Ray.IsInfiniteLength">infinite length</see> and its
		/// <see cref="Ray.StartPoint"/> and <see cref="Ray.EndPoint"/> are inside this shape.</returns>
		public bool Contains(Ray ray) {
			return !ray.IsInfiniteLength && Contains(ray.StartPoint) && Contains(ray.EndPoint.Value);
		}

		/// <summary>
		/// Indicates whether or not the given shape is contained entirely within this sphere (no part of the <paramref name="other"/>
		/// shape may be outside this one).
		/// </summary>
		/// <param name="other">The shape to check.</param>
		/// <returns>True if the other shape is completely within this sphere, false if any part of it
		/// is outside the sphere.</returns>
		public bool Contains(Sphere other) {
			return Vector3.Distance(Center, other.Center) + other.Radius <= Radius + MathUtils.FlopsErrorMargin;
		}

		/// <summary>
		/// Indicates whether or not the given shape is contained entirely within this sphere (no part of the <paramref name="other"/>
		/// shape may be outside this one).
		/// </summary>
		/// <param name="other">The shape to check.</param>
		/// <returns>True if the other shape is completely within this sphere, false if any part of it
		/// is outside the sphere.</returns>
		public bool Contains(Cone other) {
			float distanceFromCentreToConeTop = Math.Abs(other.TopCenter.Y - Center.Y);
			float distanceFromCentreToConeBottom = Math.Abs(Center.Y - (other.TopCenter.Y - other.Height));
			if (distanceFromCentreToConeTop > Radius || distanceFromCentreToConeBottom > Radius) return false;

			float sphereRadiusAtConeTop = GetRadius(distanceFromCentreToConeTop);
			float sphereRadiusAtConeBottom = GetRadius(distanceFromCentreToConeBottom);

			float xzDistance = Vector3.Distance(other.TopCenter[0, 2], Center[0, 2]);
			float errorMargin = MathUtils.FlopsErrorMargin;
			return xzDistance + other.TopRadius <= sphereRadiusAtConeTop + errorMargin
				&& xzDistance + other.BottomRadius <= sphereRadiusAtConeBottom + errorMargin;
		}

		/// <summary>
		/// Indicates whether or not the given shape is contained entirely within this sphere (no part of the <paramref name="other"/>
		/// shape may be outside this one).
		/// </summary>
		/// <param name="other">The shape to check.</param>
		/// <returns>True if the other shape is completely within this sphere, false if any part of it
		/// is outside the sphere.</returns>
		public bool Contains(Cuboid other) {
			float errorMarginSq = MathUtils.FlopsErrorMargin;
			errorMarginSq *= errorMarginSq;
			float radiusSq = Radius * Radius;
			for (int i = 0; i < Cuboid.NUM_CORNERS; ++i) {
				if (Vector3.DistanceSquared(Center, other.GetCorner((Cuboid.CuboidCorner)i)) > radiusSq + errorMarginSq) {
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Indicates whether or not the given shape is contained entirely within this sphere (no part of the <paramref name="other"/>
		/// shape may be outside this one).
		/// </summary>
		/// <param name="other">The shape to check.</param>
		/// <returns>True if the other shape is completely within this sphere, false if any part of it
		/// is outside the sphere.</returns>
		public bool Contains(Cylinder other) {
			return Contains(other.ToSymmetricalConicalFrustum());
		}

		/// <summary>
		/// Indicates the shortest distance from the given <paramref name="point"/> to the edge of this sphere
		/// (not the centre!).
		/// </summary>
		/// <param name="point">The point to check.</param>
		/// <returns>The shortest distance from the given <paramref name="point"/> to the edge of this sphere.
		/// If the <paramref name="point"/> is inside this rectangle, <c>0f</c> will be returned.</returns>
		public float DistanceFrom(Vector3 point) {
			return Math.Max(Vector3.Distance(point, Center) - Radius, 0f);
		}

		/// <summary>
		/// Indicates the shortest distance from the edge of the given shape to the edge of this sphere.
		/// </summary>
		/// <param name="other">The shape to check.</param>
		/// <returns>The shortest distance connecting the outer-edges of both shapes.
		/// If the shapes intersect, <c>0f</c> will be returned.</returns>
		public float DistanceFrom(Sphere other) {
			return Math.Max(Vector3.Distance(Center, other.Center) - (Radius + other.Radius), 0f);
		}

		/// <summary>
		/// Indicates the shortest distance from the edge of the given shape to the edge of this sphere.
		/// </summary>
		/// <param name="other">The shape to check.</param>
		/// <returns>The shortest distance connecting the outer-edges of both shapes.
		/// If the shapes intersect, <c>0f</c> will be returned.</returns>
		public float DistanceFrom(Cuboid other) {
			if (Intersects(other)) return 0f;

			float distanceX = Math.Max(Math.Abs(
				Center.X - (float) MathUtils.Clamp(Center.X, other.FrontBottomLeft.X, other.FrontBottomLeft.X + other.Width)) - Radius, 0f);
			float distanceY = Math.Max(Math.Abs(
				Center.Y - (float) MathUtils.Clamp(Center.Y, other.FrontBottomLeft.Y, other.FrontBottomLeft.Y + other.Height)) - Radius, 0f);
			float distanceZ = Math.Max(Math.Abs(
				Center.Z - (float) MathUtils.Clamp(Center.Z, other.FrontBottomLeft.Z, other.FrontBottomLeft.Z + other.Depth)) - Radius, 0f);

			return (float) Math.Sqrt(distanceX * distanceX + distanceY * distanceY + distanceZ * distanceZ);
		}

		/// <summary>
		/// Indicates the shortest distance from the edge of this sphere to the <paramref name="plane"/>.
		/// </summary>
		/// <param name="plane">The plane to get the distance to.</param>
		/// <returns>The shortest distance connecting the outer-edges of this sphere to the <paramref name="plane"/>.
		/// If the plane intersects this sphere, <c>0f</c> will be returned.</returns>
		public float DistanceFrom(Plane plane) {
			return plane.DistanceFrom(this);
		}

		/// <summary>
		/// Indicates whether any portion of this sphere and the <paramref name="other"/> shape overlap.
		/// </summary>
		/// <param name="other">The shape to check against this one.</param>
		/// <returns>True if any part of both shapes overlap, false if they are entirely separate.</returns>
		public bool Intersects(Sphere other) {
			return Vector3.Distance(Center, other.Center) <= Radius + other.Radius + MathUtils.FlopsErrorMargin;
		}

		/// <summary>
		/// Indicates whether any portion of this sphere and the <paramref name="other"/> shape overlap.
		/// </summary>
		/// <param name="other">The shape to check against this one.</param>
		/// <returns>True if any part of both shapes overlap, false if they are entirely separate.</returns>
		public bool Intersects(Cylinder other) {
			float errorMargin = MathUtils.FlopsErrorMargin;
			float cylinderHalfHeight = other.Height / 2f;
			if (other.Center.Y + cylinderHalfHeight < Center.Y - Radius + errorMargin
				|| other.Center.Y - cylinderHalfHeight > Center.Y + Radius + errorMargin) {
				return false;
			}

			float distanceToCylinderTop = Math.Abs((other.Center.Y + cylinderHalfHeight) - Center.Y);
			float distanceToCylinderBottom = Math.Abs((other.Center.Y - cylinderHalfHeight) - Center.Y);
			float sphereRadiusAtCylinderTop = distanceToCylinderTop > Radius ? 0f : GetRadius(distanceToCylinderTop);
			float sphereRadiusAtCylinderBottom = distanceToCylinderBottom > Radius ? 0f : GetRadius(distanceToCylinderBottom);

			return Vector3.Distance(Center[0, 2], other.Center[0, 2])
				- Math.Max(sphereRadiusAtCylinderTop, sphereRadiusAtCylinderBottom)
				<= other.Radius + errorMargin;
		}

		/// <summary>
		/// Indicates whether any portion of this sphere and the <paramref name="other"/> shape overlap.
		/// </summary>
		/// <param name="other">The shape to check against this one.</param>
		/// <returns>True if any part of both shapes overlap, false if they are entirely separate.</returns>
		public bool Intersects(Cuboid other) {
			return other.Intersects(this);
		}

		/// <summary>
		/// Indicates whether this sphere crosses the given <paramref name="plane"/>.
		/// </summary>
		/// <param name="plane">The plane to check for intersection with this shape.</param>
		/// <returns>True if any part of this sphere crosses the <paramref name="plane"/> (or sits on it).</returns>
		public bool Intersects(Plane plane) {
			return plane.Intersects(this);
		}

		/// <summary>
		/// Determines the first point on this sphere that is touched by the given <paramref name="ray"/>.
		/// </summary>
		/// <param name="ray">The ray to test for intersection with this sphere.</param>
		/// <returns>A <see cref="Vector3"/> indicating the first point on the sphere edge touched by the ray,
		/// or <c>null</c> if no intersection occurs.</returns>
		public Vector3? IntersectionWith(Ray ray) {
			return ray.IntersectionWith(this);
		}

		/// <summary>
		/// Projects the given <paramref name="point"/> on to this sphere.
		/// </summary>
		/// <param name="point">The point to project on to this sphere.</param>
		/// <returns>A point that lies on the surface of this sphere that is closest to the given input <paramref name="point"/>.</returns>
		public Vector3 PointProjection(Vector3 point) {
			return Center + (point - Center).WithLength(Radius);
		}
		#endregion

		/// <summary>
		/// Returns a string representation of this Sphere.
		/// </summary>
		/// <returns>A string that lists the geometric properties of this shape.</returns>
		public override string ToString() {
			return "(Sphere Center:" + Center + " Radius:" + Radius.ToString(3) + ")";
		}

		#region Equality
		/// <summary>
		/// Test whether this object is EXACTLY equal to <paramref name="other"/>. No floating-point tolerance will be accounted
		/// for: The two structures must be bitwise identical.
		/// </summary>
		/// <param name="other">The other Sphere to test against.</param>
		/// <returns>True if this object is EXACTLY equal to <paramref name="other"/>.</returns>
		public bool EqualsExactly(Sphere other) {
			// ReSharper disable CompareOfFloatsByEqualityOperator
			return Center.EqualsExactly(other.Center) && Radius == other.Radius;
			// ReSharper restore CompareOfFloatsByEqualityOperator
		}

		/// <summary>
		/// Test whether this object is equal to <paramref name="other"/> within a given <paramref name="tolerance"/>. Even if the
		/// two structures are not identical, this method will return <c>true</c> if they are 'close enough' (e.g. within the tolerance).
		/// </summary>
		/// <param name="other">The other Sphere to test against.</param>
		/// <param name="tolerance">The amount of tolerance to give. For any floating-point component in both objects, if the difference
		/// between them is less than this value, they will be considered equal.</param>
		/// <returns>True if this object is equal to <paramref name="other"/> with the given <paramref name="tolerance"/>.</returns>
		public bool EqualsWithTolerance(Sphere other, double tolerance) {
			Assure.GreaterThanOrEqualTo(tolerance, 0f, "Tolerance must not be negative.");
			return Center.EqualsWithTolerance(other.Center, tolerance)
				&& Math.Abs(Radius - other.Radius) < tolerance;
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(Sphere other) {
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
			return obj is Sphere && Equals((Sphere) obj);
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
		public static bool operator ==(Sphere left, Sphere right) {
			return left.Equals(right);
		}

		/// <summary>
		/// Indicates whether or not the two shapes are unequal.
		/// </summary>
		/// <param name="left">The first shape.</param>
		/// <param name="right">The second shape.</param>
		/// <returns>False if the shapes represent equal dimensions and positions, true if not.</returns>
		public static bool operator !=(Sphere left, Sphere right) {
			return !left.Equals(right);
		}
		#endregion
	}
}