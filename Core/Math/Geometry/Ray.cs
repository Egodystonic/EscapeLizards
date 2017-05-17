// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 08 12 2014 at 16:21 by Ben Bowen

using System;
using System.Collections.Generic;

namespace Ophidian.Losgap {
	/// <summary>
	/// Represents a directed line; expanding a given <see cref="Length"/> (may be <see cref="float.PositiveInfinity">infinity</see>) from
	/// a <see cref="StartPoint"/>.
	/// </summary>
	public struct Ray : IToleranceEquatable<Ray> {
		/// <summary>
		/// The origin of the ray.
		/// </summary>
		public readonly Vector3 StartPoint;
		/// <summary>
		/// The direction that this ray is pointing (from <see cref="StartPoint"/>).
		/// </summary>
		/// <remarks>
		/// This vector is guaranteed to be unit length.
		/// </remarks>
		public readonly Vector3 Orientation;
		/// <summary>
		/// The length of the ray. May be <see cref="float.PositiveInfinity"/>. Will never be negative.
		/// </summary>
		public readonly float Length;

		/// <summary>
		/// True if <see cref="Length"/> is equal to <see cref="float.PositiveInfinity"/>.
		/// </summary>
		public bool IsInfiniteLength {
			get {
				return Single.IsPositiveInfinity(Length);
			}
		}

		/// <summary>
		/// Returns the end point (where the ray line stops). If this ray <see cref="IsInfiniteLength"/>, returns <c>null</c>.
		/// </summary>
		public Vector3? EndPoint {
			get {
				if (IsInfiniteLength) return null;
				return StartPoint + Orientation * Length;
			}
		}

		/// <summary>
		/// Creates a new ray with the given origin (<paramref name="startPoint"/>), direction (<paramref name="orientation"/>), and
		/// optionally a <paramref name="length"/>.
		/// </summary>
		/// <param name="startPoint">The origin of the ray.</param>
		/// <param name="orientation">The direction that this ray is pointing (from <see cref="StartPoint"/>).
		/// Does not need to be unit length (it will be normalized).</param>
		/// <param name="length">The length of the ray. Leave as <see cref="float.PositiveInfinity"/> for an unbounded ray.
		/// Providing a negative value will flip the orientation of the ray.</param>
		public Ray(Vector3 startPoint, Vector3 orientation, float length = Single.PositiveInfinity) {
			if (length < 0f) {
				orientation *= -1f;
				length = Math.Abs(length);
			}
			StartPoint = startPoint;
			Orientation = orientation.ToUnit();
			Length = length;
		}

		/// <summary>
		/// Constructs a ray from a given <paramref name="startPoint"/> to a given <paramref name="endPoint"/>.
		/// </summary>
		/// <param name="startPoint">The origin of the ray.</param>
		/// <param name="endPoint">The end point of the ray.</param>
		/// <returns>A new Ray with the given <paramref name="startPoint"/>, and an <see cref="Orientation"/>
		/// and <see cref="Length"/> that results in it ending at the given <paramref name="endPoint"/>.</returns>
		public static Ray FromStartAndEndPoint(Vector3 startPoint, Vector3 endPoint) {
			if (endPoint.EqualsExactly(startPoint)) return new Ray(startPoint, Vector3.FORWARD, 0f);
			Vector3 endFromStart = endPoint - startPoint;
			return new Ray(startPoint, endFromStart, endFromStart.Length);
		}

		/// <summary>
		/// Returns the same ray, but with a new length.
		/// </summary>
		/// <param name="newLength">The new length.</param>
		/// <returns><c>new Ray(StartPoint, Orientation, newLength)</c>.</returns>
		public Ray WithLength(float newLength) {
			return new Ray(StartPoint, Orientation, newLength);
		}

		#region Shape interactions
		/// <summary>
		/// Indicates whether or not the given <paramref name="point"/> lies on the line bounded by this ray.
		/// </summary>
		/// <param name="point">The point to check.</param>
		/// <returns>True if the given <paramref name="point"/> is on this line, false if it is not.</returns>
		public bool Contains(Vector3 point) {
			return DistanceFrom(point) < MathUtils.FlopsErrorMargin;
		}

		/// <summary>
		/// Indicates whether or not the given ray is contained entirely within this one (no part of the <paramref name="other"/>
		/// ray may be outside this one).
		/// </summary>
		/// <param name="other">The ray to check. If this parameter <see cref="IsInfiniteLength"/>, this method will always
		/// return <c>false</c>.</param>
		/// <returns>True if the other ray is completely within this one, false if any part of it
		/// is outside it.</returns>
		public bool Contains(Ray other) {
			if (other.IsInfiniteLength) return false;
			return Contains(other.StartPoint) && Contains(other.EndPoint.Value);
		}

		/// <summary>
		/// Indicates the shortest distance from the given <paramref name="point"/> to this ray.
		/// </summary>
		/// <param name="point">The point to check.</param>
		/// <returns>The shortest distance from the given <paramref name="point"/> to any point on this ray.
		/// If the <paramref name="point"/> lies on this ray, <c>0f</c> will be returned.</returns>
		public float DistanceFrom(Vector3 point) {
			float distance = Vector3.Distance(point, PointProjection(point));
			if (Math.Abs(distance) < MathUtils.FlopsErrorMargin) return 0f;
			else return distance;
		}

		/// <summary>
		/// Indicates the shortest distance from the given <paramref name="other"/> ray to this one.
		/// </summary>
		/// <param name="other">The ray to compare.</param>
		/// <returns>The distance between the closest points on the two rays. If the rays intersect, <c>0f</c> will be returned.</returns>
		public float DistanceFrom(Ray other) {
			return Math.Min(other.DistanceFrom(ClosestPointTo(other)), DistanceFrom(other.ClosestPointTo(this)));
		}

		/// <summary>
		/// Returns the closest point on THIS ray to the <paramref name="other"/> ray.
		/// </summary>
		/// <param name="other">The secondary ray.</param>
		/// <returns>The point on this ray that is closest to the <paramref name="other"/> ray.</returns>
		public Vector3 ClosestPointTo(Ray other) {
			float b = Vector3.Dot(Orientation, other.Orientation);

			if (Math.Abs(Math.Abs(b) - 1f) < MathUtils.FlopsErrorMargin) { // Lines are parallel (or near enough)
				float potential;
				float minDist = other.DistanceFrom(StartPoint);
				Vector3 result = StartPoint;
				if (minDist < MathUtils.FlopsErrorMargin) return result;

				if (!IsInfiniteLength) {
					potential = other.DistanceFrom(EndPoint.Value);
					if (potential < MathUtils.FlopsErrorMargin) return EndPoint.Value;
					if (potential < minDist) {
						minDist = potential;
						result = EndPoint.Value;
					}
				}

				potential = DistanceFrom(other.StartPoint);
				if (potential < MathUtils.FlopsErrorMargin) return other.StartPoint;
				if (potential < minDist) {
					minDist = potential;
					result = PointProjection(other.StartPoint);
				}

				if (!other.IsInfiniteLength) {
					potential = DistanceFrom(other.EndPoint.Value);
					if (potential < minDist) return PointProjection(other.EndPoint.Value);
				}

				return result;
			}

			Vector3 startDiff = StartPoint - other.StartPoint;
			float d = Vector3.Dot(Orientation, startDiff);
			float e = Vector3.Dot(other.Orientation, startDiff);

			float oneMinusNegOrientDotSquared = 1f - (b * b);

			float distFromStart = (b * e - d) / oneMinusNegOrientDotSquared;

			if (distFromStart > Length) return EndPoint.Value;
			else if (distFromStart < 0f) return StartPoint;
			else return StartPoint + Orientation * distFromStart;
		}

		/// <summary>
		/// Gets the closest point on this ray to the given <paramref name="point"/>.
		/// </summary>
		/// <remarks>
		/// If the projection of the point on to the line defined by <see cref="Orientation"/> extends beyond the ray <see cref="Length"/>, or
		/// 'behind' its <see cref="StartPoint"/>, the returned <see cref="Vector3"/> will be <see cref="EndPoint"/> or <see cref="StartPoint"/>,
		/// respectively. If this is undesired behaviour, use <see cref="StrictPointProjection"/> instead.
		/// </remarks>
		/// <param name="point">The point to find the projection of.</param>
		/// <returns>The orthographic projection of <paramref name="point"/> on to this ray.</returns>
		public Vector3 PointProjection(Vector3 point) {
			// ReSharper disable once CompareOfFloatsByEqualityOperator This quick-path only works if the ray length is exactly 0f
			if (Length == 0f) return StartPoint;

			if (IsInfiniteLength) {
				float distance = Vector3.Dot(point - StartPoint, Orientation);
				if (distance < 0f) return StartPoint;
				else return StartPoint + distance * Orientation;
			}
			else {
				Vector3 rayDist = EndPoint.Value - StartPoint;
				float elongationFactor = Vector3.Dot(point - StartPoint, rayDist) / (Length * Length);
				if (elongationFactor < 0f) return StartPoint;
				else if (elongationFactor > 1f) return EndPoint.Value;
				return StartPoint + elongationFactor * rayDist;
			}
			
		}

		/// <summary>
		/// Gets the closest point on this ray to the given <paramref name="point"/>. Unlike <see cref="PointProjection"/>,
		/// if the projection of the point on to the line defined by <see cref="Orientation"/> extends beyond the ray <see cref="Length"/>, or
		/// 'behind' its <see cref="StartPoint"/>, this method will return <c>null</c> instead of 'clamping' the point on to the ray.
		/// </summary>
		/// <param name="point">The point to find the projection of.</param>
		/// <returns>The orthographic projection of <paramref name="point"/> on to this ray, or <c>null</c> if the projection extends
		/// beyond the <see cref="StartPoint"/> or <see cref="EndPoint"/>.</returns>
		public Vector3? StrictPointProjection(Vector3 point) {
			// ReSharper disable once CompareOfFloatsByEqualityOperator This quick-path only works if the ray length is exactly 0f
			if (Length == 0f) return null;

			if (IsInfiniteLength) {
				Vector3 projection = point.ProjectedOnto(Orientation);
				if (StartPoint == projection || (projection - StartPoint).ToUnit() != Orientation) return null;
				else return projection;
			}
			else {
				Vector3 endPoint = EndPoint.Value;
				float elongationFactor = Vector3.Dot(point - StartPoint, endPoint - StartPoint) / (Length * Length);
				if (elongationFactor < 0f || elongationFactor > 1f) return null;
				return StartPoint + elongationFactor * (endPoint - StartPoint);
			}
		}

		/// <summary>
		/// Determines the point where the two rays intersect.
		/// </summary>
		/// <param name="other">The second ray.</param>
		/// <returns>A <see cref="Vector3"/> indicating the point where the rays meet if they overlap, or <c>null</c>
		/// if no intersection occurs.</returns>
		public Vector3? IntersectionWith(Ray other) {
			Vector3 closestPoint = ClosestPointTo(other);
			if (other.Contains(closestPoint)) return closestPoint;
			else return null;
		}
		
		/// <summary>
		/// Determines the first point on the cuboid that is touched by this ray.
		/// </summary>
		/// <param name="other">The cuboid to test for intersection with this ray.</param>
		/// <returns>A <see cref="Vector3"/> indicating the first point on the cuboid edge touched by this ray,
		/// or <c>null</c> if no intersection occurs.</returns>
		// Based on "Fast Ray-Box Intersection" algorithm by Andrew Woo, "Graphics Gems", Academic Press, 1990
		public unsafe Vector3? IntersectionWith(Cuboid other) {
			const int NUM_DIMENSIONS = 3;
			Assure.Equal(NUM_DIMENSIONS, 3); // If that value is ever changed, this algorithm will need some maintenance

			const byte QUADRANT_MIN = 0;
			const byte QUADRANT_MAX = 1;
			const byte QUADRANT_BETWEEN = 2;

			// Step 1: Work out which direction from the start point to test for intersection for all 3 dimensions, and the distance
			byte* quadrants = stackalloc byte[NUM_DIMENSIONS];
			float* candidatePlanes = stackalloc float[NUM_DIMENSIONS];
			float* cuboidMinPoints = stackalloc float[NUM_DIMENSIONS];
			float* cuboidMaxPoints = stackalloc float[NUM_DIMENSIONS];
			bool startPointIsInsideCuboid = true;

			cuboidMinPoints[0] = other.FrontBottomLeft.X;
			cuboidMinPoints[1] = other.FrontBottomLeft.Y;
			cuboidMinPoints[2] = other.FrontBottomLeft.Z;
			cuboidMaxPoints[0] = other.FrontBottomLeft.X + other.Width;
			cuboidMaxPoints[1] = other.FrontBottomLeft.Y + other.Height;
			cuboidMaxPoints[2] = other.FrontBottomLeft.Z + other.Depth;

			for (byte i = 0; i < NUM_DIMENSIONS; ++i) {
				if (StartPoint[i] < cuboidMinPoints[i]) {
					quadrants[i] = QUADRANT_MIN;
					candidatePlanes[i] = cuboidMinPoints[i];
					startPointIsInsideCuboid = false;
				}
				else if (StartPoint[i] > cuboidMaxPoints[i]) {
					quadrants[i] = QUADRANT_MAX;
					candidatePlanes[i] = cuboidMaxPoints[i];
					startPointIsInsideCuboid = false;
				}
				else {
					quadrants[i] = QUADRANT_BETWEEN;
				}
			}

			if (startPointIsInsideCuboid) return StartPoint;

			// Step 2: Find farthest dimension from cuboid
			float maxDistance = Single.NegativeInfinity;
			byte maxDistanceDimension = 0;

			for (byte i = 0; i < NUM_DIMENSIONS; ++i) {
				// ReSharper disable once CompareOfFloatsByEqualityOperator Exact check is desired here: Anything other than 0f is usable
				if (quadrants[i] != QUADRANT_BETWEEN && Orientation[i] != 0f) {
					float thisDimensionDist = (candidatePlanes[i] - StartPoint[i]) / Orientation[i];
					if (thisDimensionDist > maxDistance) {
						maxDistance = thisDimensionDist;
						maxDistanceDimension = i;
					}
				}
			}

			float errorMargin = MathUtils.FlopsErrorMargin;
			if (maxDistance < 0f || maxDistance - Length > errorMargin) return null;

			// Step 3: Find potential intersection point
			float* intersectionPoint = stackalloc float[NUM_DIMENSIONS];
			for (byte i = 0; i < NUM_DIMENSIONS; ++i) {
				if (maxDistanceDimension == i) {
					intersectionPoint[i] = StartPoint[i] + maxDistance * Orientation[i];
					if (cuboidMinPoints[i] - intersectionPoint[i] > errorMargin || intersectionPoint[i] - cuboidMaxPoints[i] > errorMargin) return null;
				}
				else intersectionPoint[i] = candidatePlanes[i];

			}

			Vector3 result = new Vector3(intersectionPoint[0], intersectionPoint[1], intersectionPoint[2]);
			if (!IsInfiniteLength && Vector3.DistanceSquared(StartPoint, result) > Length * Length + errorMargin * errorMargin) return null;
			else if (!Contains(result)) return null;
			else return result;
		}

		/// <summary>
		/// Determines the first point on the sphere that is touched by this ray.
		/// </summary>
		/// <param name="other">The sphere to test for intersection with this ray.</param>
		/// <returns>A <see cref="Vector3"/> indicating the first point on the sphere edge touched by this ray,
		/// or <c>null</c> if no intersection occurs.</returns>
		public Vector3? IntersectionWith(Sphere other) {
			if (other.Contains(StartPoint)) return StartPoint;
			else if (!IsInfiniteLength && other.Contains(EndPoint.Value)) return new Ray(StartPoint, Orientation).IntersectionWith(other);

			float errorMargin = MathUtils.FlopsErrorMargin;

			Vector3 startPointToSphereCentre = other.Center - StartPoint;
			if (Vector3.Dot(Orientation, startPointToSphereCentre) <= -errorMargin) return null;

			Vector3 sphereCentreProjectedOnToRay = PointProjection(other.Center);
			float projectionDistanceSquared = Vector3.DistanceSquared(sphereCentreProjectedOnToRay, other.Center);
			float radiusSquared = other.Radius * other.Radius;

			if (projectionDistanceSquared > radiusSquared + errorMargin * errorMargin) return null;
			else if (projectionDistanceSquared < radiusSquared) {
				float distFromRayStartToIntersection =
					(sphereCentreProjectedOnToRay - StartPoint).Length - (float)Math.Sqrt(radiusSquared - projectionDistanceSquared);
				return StartPoint + Orientation * distFromRayStartToIntersection;
			}
			else return sphereCentreProjectedOnToRay;
		}

		/// <summary>
		/// Determines the point on the plane that is crossed by this ray.
		/// </summary>
		/// <param name="other">The plane to test for intersection with this ray.</param>
		/// <returns>A <see cref="Vector3"/> indicating the point on the plane touched by this ray,
		/// or <c>null</c> if no intersection occurs.</returns>
		public Vector3? IntersectionWith(Plane other) {
			if (other.LocationOf(StartPoint) == Plane.PointPlaneRelationship.PointOnPlane) return StartPoint;
			
			// ReSharper disable once CompareOfFloatsByEqualityOperator If it's not exactly 0f, there's an intersection
			if (Vector3.Dot(Orientation, other.Normal) == 0f) return null;

			Vector3 planeCentreFromStartPoint = other.CentrePoint - StartPoint;
			float distFromStartPoint = Vector3.Dot(other.Normal, planeCentreFromStartPoint) / Vector3.Dot(other.Normal, Orientation);
			if (distFromStartPoint > Length) return null;
			else if (distFromStartPoint < -MathUtils.FlopsErrorMargin) return null;
			else return StartPoint + Orientation * distFromStartPoint;
		}

		/// <summary>
		/// Indicates the angle (in radians) that would be formed between the plane normal and this ray, should an intersection
		/// occur. A value is returned even if the ray does not intersect the plane (the ray is extended in both directions to the plane).
		/// </summary>
		/// <param name="plane">The reflection plane.</param>
		/// <returns>The incident angle, in radians, of the intersection.</returns>
		public float IncidentAngleWith(Plane plane) {
			return (float) Math.Acos(1f - Math.Abs(Vector3.Dot(plane.Normal, Orientation)));
		}

		/// <summary>
		/// Calculates the reflection of this ray hitting the given <paramref name="plane"/>.
		/// </summary>
		/// <param name="plane">The reflection plane.</param>
		/// <returns>An infinite-length ray representing the reflection of this ray off the <paramref name="plane"/>. If this ray and the <paramref name="plane"/>
		/// do not intersect, <c>null</c> is returned.</returns>
		public Ray? Reflect(Plane plane) {
			Vector3? intersection = IntersectionWith(plane);
			if (intersection == null) return null;

			Vector3 reflectedOrientation = -2f * Vector3.Dot(plane.Normal, Orientation) * plane.Normal + Orientation;

			return new Ray(intersection.Value, reflectedOrientation);
		}
		#endregion

		/// <summary>
		/// Returns a string representation of this Sphere.
		/// </summary>
		/// <returns>A string that lists the geometric properties of this shape.</returns>
		public override string ToString() {
			return "(Ray Start:" + StartPoint + " Orientation:" + Orientation
				+ " Length:" + (IsInfiniteLength ? "Inf" : Length.ToString(3)) + ")";
		}

		#region Equality
		/// <summary>
		/// Test whether this object is EXACTLY equal to <paramref name="other"/>. No floating-point tolerance will be accounted
		/// for: The two structures must be bitwise identical.
		/// </summary>
		/// <param name="other">The other Ray to test against.</param>
		/// <returns>True if this object is EXACTLY equal to <paramref name="other"/>.</returns>
		public bool EqualsExactly(Ray other) {
			// ReSharper disable CompareOfFloatsByEqualityOperator
			return StartPoint.EqualsExactly(other.StartPoint)
				&& Orientation.EqualsExactly(other.Orientation)
				&& Length == other.Length;
			// ReSharper restore CompareOfFloatsByEqualityOperator
		}

		/// <summary>
		/// Test whether this object is equal to <paramref name="other"/> within a given <paramref name="tolerance"/>. Even if the
		/// two structures are not identical, this method will return <c>true</c> if they are 'close enough' (e.g. within the tolerance).
		/// </summary>
		/// <param name="other">The other Ray to test against.</param>
		/// <param name="tolerance">The amount of tolerance to give. For any floating-point component in both objects, if the difference
		/// between them is less than this value, they will be considered equal.</param>
		/// <returns>True if this object is equal to <paramref name="other"/> with the given <paramref name="tolerance"/>.</returns>
		public bool EqualsWithTolerance(Ray other, double tolerance) {
			Assure.GreaterThanOrEqualTo(tolerance, 0f, "Tolerance must not be negative.");
			return IsInfiniteLength == other.IsInfiniteLength
				&& StartPoint.EqualsWithTolerance(other.StartPoint, tolerance)
				&& Orientation.EqualsWithTolerance(other.Orientation, tolerance)
				&& (IsInfiniteLength || (Math.Abs(Length - other.Length) < tolerance));
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(Ray other) {
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
			return obj is Ray && Equals((Ray) obj);
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
				int hashCode = StartPoint.GetHashCode();
				hashCode = (hashCode * 397) ^ Orientation.GetHashCode();
				hashCode = (hashCode * 397) ^ Length.GetHashCode();
				return hashCode;
			}
		}

		/// <summary>
		/// Indicates whether or not the two shapes are equal.
		/// </summary>
		/// <param name="left">The first shape.</param>
		/// <param name="right">The second shape.</param>
		/// <returns>True if the shapes represent equal dimensions and positions, false if not.</returns>
		public static bool operator ==(Ray left, Ray right) {
			return left.Equals(right);
		}

		/// <summary>
		/// Indicates whether or not the two shapes are unequal.
		/// </summary>
		/// <param name="left">The first shape.</param>
		/// <param name="right">The second shape.</param>
		/// <returns>False if the shapes represent equal dimensions and positions, true if not.</returns>
		public static bool operator !=(Ray left, Ray right) {
			return !left.Equals(right);
		}
		#endregion
	}
}