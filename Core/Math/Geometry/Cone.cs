// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 28 11 2014 at 15:25 by Ben Bowen

using System;
using System.Collections.Generic;

namespace Ophidian.Losgap {
	/// <summary>
	/// Represents a right-cone (or potentially a conical frustum) in three-dimensional space.
	/// </summary>
	public struct Cone : IToleranceEquatable<Cone> {
		/// <summary>
		/// The position of the top-centre of this cone; i.e. the 'point' at its top (assuming the <see cref="TopRadius"/> is <c>0f</c>).
		/// </summary>
		public readonly Vector3 TopCenter;
		/// <summary>
		/// The radius of the top face of the cone. For standard cones, this value will be 0f.
		/// </summary>
		public readonly float TopRadius;
		/// <summary>
		/// The radius of the bottom face of the cone.
		/// </summary>
		public readonly float BottomRadius;
		/// <summary>
		/// The extent of the cone on the Y-axis.
		/// </summary>
		public readonly float Height;

		/// <summary>
		/// If true, both <see cref="BottomRadius"/> and <see cref="TopRadius"/> are greater than <c>0f</c>, meaning that this cone is
		/// actually a conical frustum.
		/// </summary>
		public bool IsConicalFrustum {
			get {
				// ReSharper disable CompareOfFloatsByEqualityOperator Anything other than 0 is a conical frustum
				return BottomRadius != 0f && TopRadius != 0f;
				// ReSharper restore CompareOfFloatsByEqualityOperator
			}
		}

		/// <summary>
		/// The area covered by the surface of this cone.
		/// </summary>
		public float SurfaceArea {
			get {
				// ReSharper disable once CompareOfFloatsByEqualityOperator 
				if (Height == 0f) return 0f;
				float radiiDiff = (BottomRadius - TopRadius);
				return MathUtils.PI * (BottomRadius + TopRadius) * (float) Math.Sqrt(radiiDiff * radiiDiff + Height * Height)
					+ MathUtils.PI * TopRadius * TopRadius + MathUtils.PI * BottomRadius * BottomRadius;
			}
		}

		/// <summary>
		/// The entire space taken up by this cone.
		/// </summary>
		public float Volume {
			get {
				return MathUtils.PI * (Height / 3f) * (BottomRadius * BottomRadius + BottomRadius * TopRadius + TopRadius * TopRadius);
			}
		}

		/// <summary>
		/// A new cone that represents this one turned 'upside-down', i.e. by swapping its <see cref="TopRadius"/> and <see cref="BottomRadius"/>.
		/// </summary>
		public Cone Inverse {
			get {
				return new Cone(TopCenter, TopRadius, Height, BottomRadius);
			}
		}

		/// <summary>
		/// The position of the bottom-centre of this cone; i.e. the 'point' at the centre of its bottom.
		/// </summary>
		public Vector3 BottomCenter {
			get {
				return new Vector3(TopCenter, y: TopCenter.Y - Height);
			}
		}

		/// <summary>
		/// Gets the radius of the cone at the specified vertical distance from <see cref="TopCenter"/>.
		/// </summary>
		/// <param name="distanceFromTop">The distance from the top centre of the cone.</param>
		/// <returns>The radius of the cone at the requested distance.</returns>
		/// <exception cref="AssuranceFailedException">Thrown if <paramref name="distanceFromTop"/>
		/// is not between <c>0f</c> and <see cref="Height"/>.</exception>
		public float GetRadius(float distanceFromTop) {
			distanceFromTop = (float) MathUtils.Clamp(distanceFromTop, 0f, Height);
			// ReSharper disable once CompareOfFloatsByEqualityOperator Only need this workaround if Height == 0f
			if (Height == 0f) return 0f;
			return (Height - distanceFromTop) / Height * TopRadius + distanceFromTop / Height * BottomRadius;
		}

		/// <summary>
		/// Creates a new Cone with the given <paramref name="topCenter"/> point, <paramref name="bottomRadius"/>, and <paramref name="height"/>.
		/// Optionally, a <paramref name="topRadius"/> may also be specified.
		/// </summary>
		/// <param name="topCenter">The position of the top-centre point on this cone.</param>
		/// <param name="bottomRadius">The radius of the bottom face of this cone.</param>
		/// <param name="height">The height of this cone.</param>
		/// <param name="topRadius">The radius of the top face of this cone. For a standard closed nappe, this should be <c>0f</c>.</param>
		public Cone(Vector3 topCenter, float bottomRadius, float height, float topRadius = 0f) {
			if (height < 0f) {
				TopRadius = Math.Abs(bottomRadius);
				BottomRadius = Math.Abs(topRadius);
				topCenter = new Vector3(topCenter, y: topCenter.Y - height);
			}
			else {
				TopRadius = Math.Abs(topRadius);
				BottomRadius = Math.Abs(bottomRadius);
			}
			Height = Math.Abs(height);

			TopCenter = topCenter;
		}

		/// <summary>
		/// Creates a new Cone with the given top-centre point (<paramref name="topCenterX"/>, <paramref name="topCenterY"/>,
		/// and <paramref name="topCenterZ"/>), <paramref name="bottomRadius"/>, and <paramref name="height"/>.
		/// Optionally, a <paramref name="topRadius"/> may also be specified.
		/// </summary>
		/// <param name="topCenterX">The position on the x-axis of the centre of this cone.</param>
		/// <param name="topCenterY">The position on the y-axis of the top of this cone.</param>
		/// <param name="topCenterZ">The position on the z-axis of the centre of this cone.</param>
		/// <param name="bottomRadius">The radius of the bottom face of this cone.</param>
		/// <param name="height">The height of this cone.</param>
		/// <param name="topRadius">The radius of the top face of this cone.</param>
		public Cone(float topCenterX, float topCenterY, float topCenterZ, float bottomRadius, float height, float topRadius = 0f)
			: this(new Vector3(topCenterX, topCenterY, topCenterZ), bottomRadius, height, topRadius) { }


		#region Shape interactions
		/// <summary>
		/// Indicates whether or not the given <paramref name="point"/> lies inside this cone.
		/// </summary>
		/// <param name="point">The point to check.</param>
		/// <returns>True if the given <paramref name="point"/> is inside this cone, false if it is outside.</returns>
		public bool Contains(Vector3 point) {
			float errorMargin = MathUtils.FlopsErrorMargin;
			if (point.Y > TopCenter.Y + errorMargin || point.Y + errorMargin < TopCenter.Y - Height) return false;
			float radiusAtPointHeight = GetRadius(TopCenter.Y - point.Y);
			return Vector3.DistanceSquared(TopCenter[0, 2], point[0, 2]) 
				<= radiusAtPointHeight * radiusAtPointHeight + errorMargin * errorMargin;
		}

		/// <summary>
		/// Indicates whether or not the given <paramref name="ray"/> is contained entirely within this cone.
		/// </summary>
		/// <param name="ray">The ray to check.</param>
		/// <returns>True if the <paramref name="ray"/> is not <see cref="Ray.IsInfiniteLength">infinite length</see> and its
		/// <see cref="Ray.StartPoint"/> and <see cref="Ray.EndPoint"/> are inside this shape.</returns>
		public bool Contains(Ray ray) {
			return !ray.IsInfiniteLength && Contains(ray.StartPoint) && Contains(ray.EndPoint.Value);
		}

		/// <summary>
		/// Indicates whether or not the given shape is contained entirely within this cone (no part of the <paramref name="other"/>
		/// shape may be outside this one).
		/// </summary>
		/// <param name="other">The shape to check.</param>
		/// <returns>True if the other shape is completely within this cone, false if any part of it
		/// is outside the cone.</returns>
		public bool Contains(Cone other) {
			float errorMargin = MathUtils.FlopsErrorMargin;
			if (other.TopCenter.Y > TopCenter.Y + errorMargin || other.TopCenter.Y - other.Height + errorMargin < TopCenter.Y - Height) return false;
			float radiusAtConeTop = GetRadius(TopCenter.Y - other.TopCenter.Y);
			float radiusAtConeBottom = GetRadius(TopCenter.Y - (other.TopCenter.Y - other.Height));
			float xzDist = Vector3.Distance(TopCenter[0, 2], other.TopCenter[0, 2]);
			return xzDist + other.TopRadius <= radiusAtConeTop + errorMargin && xzDist + other.BottomRadius <= radiusAtConeBottom + errorMargin;
		}

		/// <summary>
		/// Indicates whether or not the given shape is contained entirely within this cone (no part of the <paramref name="other"/>
		/// shape may be outside this one).
		/// </summary>
		/// <param name="other">The shape to check.</param>
		/// <returns>True if the other shape is completely within this cone, false if any part of it
		/// is outside the cone.</returns>
		public bool Contains(Cylinder other) {
			return Contains(other.ToSymmetricalConicalFrustum());
		}

		/// <summary>
		/// Indicates whether or not the given shape is contained entirely within this cone (no part of the <paramref name="other"/>
		/// shape may be outside this one).
		/// </summary>
		/// <param name="other">The shape to check.</param>
		/// <returns>True if the other shape is completely within this cone, false if any part of it
		/// is outside the cone.</returns>
		public bool Contains(Cuboid other) {
			float errorMargin = MathUtils.FlopsErrorMargin;
			if (other.FrontBottomLeft.Y + errorMargin < TopCenter.Y - Height
				|| other.FrontBottomLeft.Y + other.Height > TopCenter.Y + errorMargin) {
				return false;
			}

			if (BottomRadius > TopRadius) {
				return Contains(other.GetCorner(Cuboid.CuboidCorner.FrontTopLeft)) 
					&& Contains(other.GetCorner(Cuboid.CuboidCorner.BackTopRight));
			}
			else {
				return Contains(other.GetCorner(Cuboid.CuboidCorner.FrontBottomLeft)) 
					&& Contains(other.GetCorner(Cuboid.CuboidCorner.BackBottomRight));
			}
		}

		/// <summary>
		/// Indicates whether any portion of this cone and the <paramref name="other"/> shape overlap.
		/// </summary>
		/// <param name="other">The shape to check against this one.</param>
		/// <returns>True if any part of both shapes overlap, false if they are entirely separate.</returns>
		public bool Intersects(Cone other) {
			float errorMargin = MathUtils.FlopsErrorMargin;
			if (TopCenter.Y + errorMargin < other.TopCenter.Y - other.Height || other.TopCenter.Y + errorMargin < TopCenter.Y - Height) return false;
			float centreDiff = Vector3.Distance(TopCenter[0, 2], other.TopCenter[0, 2]) - errorMargin;
			if (TopCenter.Y <= other.TopCenter.Y) {
				if (centreDiff <= TopRadius + other.GetRadius(other.TopCenter.Y - TopCenter.Y)) return true;
			}
			else if (other.TopCenter.Y < TopCenter.Y) {
				if (centreDiff <= other.TopRadius + other.GetRadius(TopCenter.Y - other.TopCenter.Y)) return true;
			}

			if (TopCenter.Y - Height >= other.TopCenter.Y - other.Height) {
				if (centreDiff <= BottomRadius + other.GetRadius(other.TopCenter.Y - (TopCenter.Y - Height))) return true;
			}
			else if (other.TopCenter.Y - other.Height > TopCenter.Y - Height) {
				if (centreDiff <= other.BottomRadius + other.GetRadius(TopCenter.Y - (other.TopCenter.Y - other.Height))) return true;
			}

			return false;
		}

		/// <summary>
		/// Indicates whether any portion of this cone and the <paramref name="other"/> shape overlap.
		/// </summary>
		/// <param name="other">The shape to check against this one.</param>
		/// <returns>True if any part of both shapes overlap, false if they are entirely separate.</returns>
		public bool Intersects(Cylinder other) {
			return other.ToSymmetricalConicalFrustum().Intersects(this);
		}
		#endregion

		/// <summary>
		/// Returns a string representation of this Cone.
		/// </summary>
		/// <returns>A string that lists the geometric properties of this shape.</returns>
		public override string ToString() {
			return "(Cone TopCenter:" + TopCenter + 
				" TopRadius:" + TopRadius.ToString(3) + " BottomRadius:" + BottomRadius.ToString(3) + " Height:" + Height.ToString(3) + ")";
		}


		#region Equality
		/// <summary>
		/// Test whether this object is EXACTLY equal to <paramref name="other"/>. No floating-point tolerance will be accounted
		/// for: The two structures must be bitwise identical.
		/// </summary>
		/// <param name="other">The other Cone to test against.</param>
		/// <returns>True if this object is EXACTLY equal to <paramref name="other"/>.</returns>
		public bool EqualsExactly(Cone other) {
			// ReSharper disable CompareOfFloatsByEqualityOperator
			return TopCenter.EqualsExactly(other.TopCenter)
				&& BottomRadius == other.BottomRadius
				&& Height == other.Height
				&& TopRadius == other.TopRadius;
			// ReSharper restore CompareOfFloatsByEqualityOperator
		}

		/// <summary>
		/// Test whether this object is equal to <paramref name="other"/> within a given <paramref name="tolerance"/>. Even if the
		/// two structures are not identical, this method will return <c>true</c> if they are 'close enough' (e.g. within the tolerance).
		/// </summary>
		/// <param name="other">The other Cone to test against.</param>
		/// <param name="tolerance">The amount of tolerance to give. For any floating-point component in both objects, if the difference
		/// between them is less than this value, they will be considered equal.</param>
		/// <returns>True if this object is equal to <paramref name="other"/> with the given <paramref name="tolerance"/>.</returns>
		public bool EqualsWithTolerance(Cone other, double tolerance) {
			Assure.GreaterThanOrEqualTo(tolerance, 0f, "Tolerance must not be negative.");
			return TopCenter.EqualsWithTolerance(other.TopCenter, tolerance)
				&& Math.Abs(TopRadius - other.TopRadius) < tolerance
				&& Math.Abs(BottomRadius - other.BottomRadius) < tolerance
				&& Math.Abs(Height - other.Height) < tolerance;
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(Cone other) {
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
			return obj is Cone && Equals((Cone) obj);
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>
		/// A 32-bit signed integer that is the hash code for this instance.
		/// </returns>
		public override int GetHashCode() {
			unchecked {
				int hashCode = TopCenter.GetHashCode();
				hashCode = (hashCode * 397) ^ TopRadius.GetHashCode();
				hashCode = (hashCode * 397) ^ BottomRadius.GetHashCode();
				hashCode = (hashCode * 397) ^ Height.GetHashCode();
				return hashCode;
			}
		}

		/// <summary>
		/// Indicates whether or not the two shapes are equal.
		/// </summary>
		/// <param name="left">The first shape.</param>
		/// <param name="right">The second shape.</param>
		/// <returns>True if the shapes represent equal dimensions and positions, false if not.</returns>
		public static bool operator ==(Cone left, Cone right) {
			return left.Equals(right);
		}

		/// <summary>
		/// Indicates whether or not the two shapes are unequal.
		/// </summary>
		/// <param name="left">The first shape.</param>
		/// <param name="right">The second shape.</param>
		/// <returns>False if the shapes represent equal dimensions and positions, true if not.</returns>
		public static bool operator !=(Cone left, Cone right) {
			return !left.Equals(right);
		}
		#endregion
	}
}