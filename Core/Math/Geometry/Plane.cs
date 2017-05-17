// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 27 11 2014 at 10:16 by Ben Bowen

using System;
using System.Collections.Generic;

namespace Ophidian.Losgap {
	/// <summary>
	/// Represents an infinite plane along two arbitrary (perpendicular) axes.
	/// </summary>
	public struct Plane : IToleranceEquatable<Plane> {
		/// <summary>
		/// The normal vector to the plane (e.g. a vector that is perpendicular to the plane).
		/// Guaranteed to be unit length.
		/// </summary>
		public readonly Vector3 Normal;

		/// <summary>
		/// The distance that you would have to travel (times the <see cref="Normal"/>)
		/// to get to the origin [0, 0, 0] from the closest point on the plane to the origin (the <see cref="CentrePoint"/>).
		/// </summary>
		/// <remarks>
		/// For example, if the <see cref="Normal"/> was [0, 1, 0] and the closest point
		/// to the origin was at [0, 3, 0], this value would be <c>-3</c>, because <c>-3 * [0, 1, 0]</c> is equal to the vector that
		/// takes you from the closest point ([0, 3, 0]) to the origin ([0, 0, 0]).
		/// </remarks>
		public readonly float DistanceToOrigin;

		/// <summary>
		/// Returns the point on this plane that is closest to the world origin ([0, 0, 0]).
		/// </summary>
		/// <remarks>
		/// This point can also be used as an arbitrary point-on-the-plane for geometric calculations.
		/// </remarks>
		public Vector3 CentrePoint {
			get {
				return Normal.WithLength(-DistanceToOrigin);
			}
		}

		/// <summary>
		/// Returns a new plane that has the same position and geometric values as this plane, but with a flipped
		/// (reversed) <see cref="Normal"/>, so that it 'faces' the opposite direction.
		/// </summary>
		public Plane Flipped {
			get {
				return new Plane(-Normal, CentrePoint);
			}
		}

		/// <summary>
		/// Constructs a new plane with the given <paramref name="normal"/> vector,
		/// and a point on the plane (<paramref name="position"/>).
		/// </summary>
		/// <param name="normal">The normal vector to the plane (e.g. a vector that is perpendicular to the plane).
		/// Does not need to be unit length.</param>
		/// <param name="position">The position of a point that lies on the plane. Any point on the plane is acceptable 
		/// (does not need to be the <see cref="CentrePoint"/>).</param>
		public Plane(Vector3 normal, Vector3 position) {
			Assure.NotEqual(normal, Vector3.ZERO, "Normal vector can not be zero vector.");
			Normal = normal.ToUnit();
			DistanceToOrigin = Vector3.Dot(Normal, -position);
		}

		/// <summary>
		/// Constructs a new plane with the given <paramref name="normal"/> vector,
		/// and shortest distance to the origin (<paramref name="distanceToOrigin"/>).
		/// </summary>
		/// <param name="normal">The normal vector to the plane (e.g. a vector that is perpendicular to the plane).
		/// Does not need to be unit length.</param>
		/// <param name="distanceToOrigin">The distance from the <see cref="CentrePoint"/> to the world origin.
		/// See: <see cref="DistanceToOrigin"/>.</param>
		public Plane(Vector3 normal, float distanceToOrigin) {
			Assure.NotEqual(normal, Vector3.ZERO, "Normal vector can not be zero vector.");
			Normal = normal.ToUnit();
			DistanceToOrigin = distanceToOrigin;
		}

		/// <summary>
		/// Constructs a new plane with the given normal (<paramref name="normalX"/>, <paramref name="normalY"/>, <paramref name="normalZ"/>),
		/// and shortest distance to the origin (<paramref name="distanceToOrigin"/>).
		/// </summary>
		/// <remarks>
		/// The given normal vector need not be unit length.
		/// </remarks>
		/// <param name="normalX">The X-value of the normal vector to the plane (e.g. a vector that is perpendicular to the plane).</param>
		/// <param name="normalY">The Y-value of the normal vector to the plane (e.g. a vector that is perpendicular to the plane).</param>
		/// <param name="normalZ">The Z-value of the normal vector to the plane (e.g. a vector that is perpendicular to the plane).</param>
		/// <param name="distanceToOrigin">The distance from the <see cref="CentrePoint"/> to the world origin.
		/// See: <see cref="DistanceToOrigin"/>.</param>
		public Plane(float normalX, float normalY, float normalZ, float distanceToOrigin)
			: this(new Vector3(normalX, normalY, normalZ), distanceToOrigin) { }

		public static Plane FromPoints(Vector3 pointA, Vector3 pointB, Vector3 pointC) {
			Vector3 normal = (pointB - pointA).Cross(pointC - pointA);
			if (normal == Vector3.ZERO) throw new ArgumentException("Three colinear points are not enough to specify a plane.");
			return new Plane(normal, pointA);
		}

		#region Shape interactions
		/// <summary>
		/// An enumeration of possible positions for a given point to be, relative to a plane.
		/// </summary>
		public enum PointPlaneRelationship {
			/// <summary>
			/// Indicates that the given point is on the opposite side of the 'normal' side of the plane (e.g. the point is 'behind' the normal vector).
			/// </summary>
			PointBehindPlane,
			/// <summary>
			/// Indicates that the given point is on the 'normal' side of the plane (e.g. 'in front of' the normal vector).
			/// </summary>
			PointInFrontOfPlane,
			/// <summary>
			/// Indicates that the given point lies directly on the plane.
			/// </summary>
			PointOnPlane
		}

		/// <summary>
		/// Find the location of the given point relative to this plane.
		/// </summary>
		/// <param name="point">The point to locate.</param>
		/// <returns>A <see cref="PointPlaneRelationship"/> value that describes the location of the point, relative to this plane.</returns>
		public PointPlaneRelationship LocationOf(Vector3 point) {
			float signedDistance = SignedDistanceFrom(point);
			if (Math.Abs(signedDistance) < MathUtils.FlopsErrorMargin) return PointPlaneRelationship.PointOnPlane;
			else if (signedDistance > 0f) return PointPlaneRelationship.PointInFrontOfPlane;
			else return PointPlaneRelationship.PointBehindPlane;
			
		}

		/// <summary>
		/// Indicates whether or not the given <paramref name="ray"/> lies directly along this plane (e.g. they are colinear).
		/// </summary>
		/// <param name="ray">The ray to check.</param>
		/// <returns>True if the <paramref name="ray"/>'s <see cref="Ray.StartPoint"/> is on the plane, and its <see cref="Ray.Orientation"/>
		/// points along the plane.</returns>
		public bool Contains(Ray ray) {
			return LocationOf(ray.StartPoint) == PointPlaneRelationship.PointOnPlane 
				// Multiply orientation by 2 to overcome FP inaccuracy
				&& LocationOf(ray.StartPoint + ray.Orientation * 2f) == PointPlaneRelationship.PointOnPlane;
		}

		/// <summary>
		/// Gets the shortest distance from the given <paramref name="point"/> to this plane.
		/// </summary>
		/// <param name="point">The point to get the distance to.</param>
		/// <returns>The distance of the most direct route from the point to this plane.</returns>
		public float DistanceFrom(Vector3 point) {
			return Math.Abs(SignedDistanceFrom(point));
		}

		/// <summary>
		/// Gets the shortest distance from the given <paramref name="point"/> to this plane; signed so that a negative value
		/// indicates that the point lies behind the plane, and a positive value indicates that the point lies in front of the plane.
		/// </summary>
		/// <param name="point">The point to get the distance to.</param>
		/// <returns>The distance of the most direct route from the point to this plane.</returns>
		/// <seealso cref="DistanceFrom(Ophidian.Losgap.Vector3)"/>
		/// <seealso cref="LocationOf"/>
		public float SignedDistanceFrom(Vector3 point) {
			return Vector3.Dot(Normal, point) + DistanceToOrigin;
		}

		/// <summary>
		/// Gets the distance between two planes. If the planes are not parallel, this method will always return <c>0f</c>.
		/// </summary>
		/// <param name="plane">The other plane.</param>
		/// <returns>The distance between the planes (or <c>0f</c> if they intersect).</returns>
		public float DistanceFrom(Plane plane) {
			if (Normal * plane.Normal != Vector3.ZERO) return 0f;
			else return DistanceFrom(plane.CentrePoint);
		}

		/// <summary>
		/// Gets the shortest distance from this plane to the <paramref name="cuboid"/>'s outer edge.
		/// </summary>
		/// <param name="cuboid">The cuboid to get the distance to.</param>
		/// <returns>The distance of the shortest path from the cuboid's edge (not its centre) to this plane, or
		/// <c>0f</c> if there is an intersection.</returns>
		public float DistanceFrom(Cuboid cuboid) {
			if (Intersects(cuboid)) return 0f;

			float min = Single.MaxValue;
			for (int i = 0; i < Cuboid.NUM_CORNERS; ++i) {
				min = Math.Min(min, DistanceFrom(cuboid.GetCorner((Cuboid.CuboidCorner) i)));
			}

			return min;
		}

		/// <summary>
		/// Gets the shortest distance from this plane to the <paramref name="sphere"/>'s outer edge.
		/// </summary>
		/// <param name="sphere">The sphere to get the distance to.</param>
		/// <returns>The distance of the shortest path from the sphere's edge (not its centre) to this plane, or
		/// <c>0f</c> if there is an intersection.</returns>
		public float DistanceFrom(Sphere sphere) {
			return Math.Max(DistanceFrom(sphere.Center) - sphere.Radius, 0f);
		}

		/// <summary>
		/// Projects the given <paramref name="point"/> on to the plane.
		/// </summary>
		/// <remarks>
		/// This differs from <see cref="OrientationProjection"/> as it treats the <paramref name="point"/> as a point, and not an orientation
		/// or direction vector. This means that the returned value is the given <paramref name="point"/> translated on to this plane.
		/// </remarks>
		/// <param name="point">The point to project.</param>
		/// <returns>The location of the point on the plane that is closest to <paramref name="point"/>.</returns>
		public Vector3 PointProjection(Vector3 point) {
			Vector3 planeCentreToPoint = point - CentrePoint;
			float projectedDistance = Vector3.Dot(planeCentreToPoint, Normal);
			return point - projectedDistance * Normal;
		}

		/// <summary>
		/// Projects the given <paramref name="vector"/> on to the plane, as an orientation.
		/// </summary>
		/// <remarks>
		/// This differs from <see cref="PointProjection"/> as it flattens the given vector 'against' an imaginary plane with the same
		/// <see cref="Normal"/> as this one, but with a <see cref="DistanceToOrigin"/> of <c>0f</c> (i.e. intersecting the origin).
		/// This returns a new vector that still 'originates' at <c>[0, 0, 0]</c> (and is therefore not translated).
		/// </remarks>
		/// <param name="vector">The vector (orientation) to project.</param>
		/// <returns>The orientation vector provided, 'flattened' against the plane.</returns>
		public Vector3 OrientationProjection(Vector3 vector) {
			return vector - (vector.Dot(Normal) * Normal);
		}

		/// <summary>
		/// Projects the given <paramref name="ray"/> on to the plane.
		/// </summary>
		/// <param name="ray">The ray to project.</param>
		/// <returns>A new ray with the same orientation along the 2D axes of this plane (but with the third dimension removed),
		/// moved on to the plane.</returns>
		public Ray RayProjection(Ray ray) {
			if (ray.IsInfiniteLength) {
				Vector3 newStart = PointProjection(ray.StartPoint);
				return new Ray(newStart, PointProjection(ray.StartPoint + ray.Orientation) - newStart);
			}
			else return Ray.FromStartAndEndPoint(PointProjection(ray.StartPoint), PointProjection(ray.EndPoint.Value));
		}

		/// <summary>
		/// Indicates whether the <paramref name="sphere"/> intersects (crosses) this plane at any point.
		/// </summary>
		/// <param name="sphere">The sphere to test.</param>
		/// <returns>True if the sphere intersects this plane (or touches it), false if not.</returns>
		public bool Intersects(Sphere sphere) {
			return DistanceFrom(sphere) <= MathUtils.FlopsErrorMargin;
		}

		/// <summary>
		/// Indicates whether the <paramref name="cuboid"/> intersects (crosses) this plane at any point.
		/// </summary>
		/// <param name="cuboid">The cuboid to test.</param>
		/// <returns>True if the cuboid intersects this plane (or touches it), false if not.</returns>
		public bool Intersects(Cuboid cuboid) {
			PointPlaneRelationship firstPointLocation = LocationOf(cuboid.GetCorner((Cuboid.CuboidCorner) 0));
			for (int i = 1; i < Cuboid.NUM_CORNERS; ++i) {
				if (LocationOf(cuboid.GetCorner((Cuboid.CuboidCorner) i)) != firstPointLocation) return true;
			}
			return false;
		}

		/// <summary>
		/// Indicates whether the other <paramref name="plane"/> intersects (crosses) this plane at any point.
		/// </summary>
		/// <param name="plane">The second plane.</param>
		/// <returns>True if the planes cross, false if they are parallel.</returns>
		public bool Intersects(Plane plane) {
			// ReSharper disable once CompareOfFloatsByEqualityOperator If it's not exactly 0f, they're perpendicular
			return DistanceFrom(plane) < MathUtils.FlopsErrorMargin;
		}

		/// <summary>
		/// Determines the infinite line created where this plane and <paramref name="other"/> intersect.
		/// </summary>
		/// <param name="other">The other plane.</param>
		/// <param name="outLineOrientation">The orientation (direction) of the intersection line.</param>
		/// <param name="outPointOnLine">A single point on the intersection line.</param>
		/// <returns>Returns <c>true</c> if an intersection occurs, or <c>false</c> if the two planes are parallel (meaning
		/// they will never intersect). If this method returns <c>false</c>, <paramref name="outLineOrientation"/>
		/// and <paramref name="outPointOnLine"/> will be set to <see cref="Vector3.ZERO"/>, and should not be used.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "outLine",
			Justification = "See below"), 
		System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "OnLine",
			Justification = "See below"),
		System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "outPoint",
			Justification = "There's nothing wrong here, false alarm")]
		public bool IntersectionWith(Plane other, out Vector3 outLineOrientation, out Vector3 outPointOnLine) {		
			if (Math.Abs(Vector3.Dot(Normal, other.Normal) - 1f) < MathUtils.FlopsErrorMargin) {
				outLineOrientation = outPointOnLine = Vector3.ZERO;
				return false;
			}

			outLineOrientation = this.Normal * other.Normal;
			float maxAbsOrient = Math.Max(Math.Abs(outLineOrientation.X), Math.Max(Math.Abs(outLineOrientation.Y), Math.Abs(outLineOrientation.Z)));
			// ReSharper disable CompareOfFloatsByEqualityOperator They will be identical
			if (maxAbsOrient == Math.Abs(outLineOrientation.X)) {
				float denominator = this.Normal.Y * other.Normal.Z - other.Normal.Y * this.Normal.Z;
				outPointOnLine = new Vector3(
					0f,
					(this.Normal.Z * other.DistanceToOrigin - other.Normal.Z * DistanceToOrigin) / denominator,
					(other.Normal.Y * DistanceToOrigin - this.Normal.Y * other.DistanceToOrigin) / denominator
				);
			}
			else if (maxAbsOrient == Math.Abs(outLineOrientation.Y)) {
				float denominator = this.Normal.Z * other.Normal.X - other.Normal.Z * this.Normal.X;
				outPointOnLine = new Vector3(
					(other.Normal.Z * DistanceToOrigin - this.Normal.Z * other.DistanceToOrigin) / denominator,
					0f,
					(this.Normal.X * other.DistanceToOrigin - other.Normal.X * DistanceToOrigin) / denominator
				);
			}
			else {
				float denominator = this.Normal.X * other.Normal.Y - other.Normal.X * this.Normal.Y;
				outPointOnLine = new Vector3(
					(this.Normal.Y * other.DistanceToOrigin - other.Normal.Y * DistanceToOrigin) / denominator,
					(other.Normal.X * DistanceToOrigin - this.Normal.X * other.DistanceToOrigin) / denominator,
					0f
				);
			}
			// ReSharper restore CompareOfFloatsByEqualityOperator
			return true;
		}

		/// <summary>
		/// Determines the point on this plane that is crossed by the <paramref name="ray"/>.
		/// </summary>
		/// <param name="ray">The ray to test for intersection with this plane.</param>
		/// <returns>A <see cref="Vector3"/> indicating the point on this plane touched by the ray,
		/// or <c>null</c> if no intersection occurs.</returns>
		public Vector3? IntersectionWith(Ray ray) {
			return ray.IntersectionWith(this);
		}

		/// <summary>
		/// Indicates the angle (in radians) that would be formed between this plane's normal and the given <paramref name="ray"/>, 
		/// should an intersection occur. 
		/// A value is returned even if the ray does not intersect this plane (the ray is extended in both directions to the plane).
		/// </summary>
		/// <param name="ray">The incident ray.</param>
		/// <returns>The incident angle, in radians, of the intersection.</returns>
		public float IncidentAngleWith(Ray ray) {
			return ray.IncidentAngleWith(this);
		}

		/// <summary>
		/// Calculates the reflection of the given <paramref name="ray"/> hitting this plane.
		/// </summary>
		/// <param name="ray">The incident ray.</param>
		/// <returns>An infinite-length ray representing the reflection of the given <paramref name="ray"/> off
		/// this plane. If the ray and this plane do not intersect, <c>null</c> is returned.</returns>
		public Ray? Reflect(Ray ray) {
			return ray.Reflect(this);
		}
		#endregion

		/// <summary>
		/// Returns a string representation of this Plane.
		/// </summary>
		/// <returns>A string that lists the geometric properties of this shape.</returns>
		public override string ToString() {
			return "(Plane Normal:" + Normal + " DistanceToOrigin:" + DistanceToOrigin.ToString(3) + ")";
		}


		#region Equality
		/// <summary>
		/// Test whether this object is EXACTLY equal to <paramref name="other"/>. No floating-point tolerance will be accounted
		/// for: The two structures must be bitwise identical.
		/// </summary>
		/// <param name="other">The other Plane to test against.</param>
		/// <returns>True if this object is EXACTLY equal to <paramref name="other"/>.</returns>
		public bool EqualsExactly(Plane other) {
			// ReSharper disable CompareOfFloatsByEqualityOperator
			return Normal.EqualsExactly(other.Normal)
				&& DistanceToOrigin == other.DistanceToOrigin;
			// ReSharper restore CompareOfFloatsByEqualityOperator
		}

		/// <summary>
		/// Test whether this object is equal to <paramref name="other"/> within a given <paramref name="tolerance"/>. Even if the
		/// two structures are not identical, this method will return <c>true</c> if they are 'close enough' (e.g. within the tolerance).
		/// </summary>
		/// <param name="other">The other Plane to test against.</param>
		/// <param name="tolerance">The amount of tolerance to give. For any floating-point component in both objects, if the difference
		/// between them is less than this value, they will be considered equal.</param>
		/// <returns>True if this object is equal to <paramref name="other"/> with the given <paramref name="tolerance"/>.</returns>
		public bool EqualsWithTolerance(Plane other, double tolerance) {
			Assure.GreaterThanOrEqualTo(tolerance, 0f, "Tolerance must not be negative.");
			return Normal.EqualsWithTolerance(other.Normal, tolerance)
				&& Math.Abs(DistanceToOrigin - other.DistanceToOrigin) < tolerance;
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(Plane other) {
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
			return obj is Plane && Equals((Plane) obj);
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
				int hashCode = Normal.GetHashCode();
				hashCode = (hashCode * 397) ^ DistanceToOrigin.GetHashCode();
				return hashCode;
			}
		}

		/// <summary>
		/// Indicates whether or not the two shapes are equal.
		/// </summary>
		/// <param name="left">The first shape.</param>
		/// <param name="right">The second shape.</param>
		/// <returns>True if the shapes represent equal dimensions and positions, false if not.</returns>
		public static bool operator ==(Plane left, Plane right) {
			return left.Equals(right);
		}

		/// <summary>
		/// Indicates whether or not the two shapes are unequal.
		/// </summary>
		/// <param name="left">The first shape.</param>
		/// <param name="right">The second shape.</param>
		/// <returns>False if the shapes represent equal dimensions and positions, true if not.</returns>
		public static bool operator !=(Plane left, Plane right) {
			return !left.Equals(right);
		}
		#endregion

		/// <summary>
		/// Returns a new plane that has the same position and geometric values as <paramref name="operand"/>, but with a flipped
		/// (reversed) <see cref="Normal"/>, so that it 'faces' the opposite direction.
		/// </summary>
		/// <param name="operand">The plane to 'flip'.</param>
		/// <returns><c>operand.Flipped</c></returns>
		public static Plane operator -(Plane operand) {
			return operand.Flipped;
		}
	}
}