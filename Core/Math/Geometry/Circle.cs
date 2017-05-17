// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 26 11 2014 at 10:11 by Ben Bowen

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;

namespace Ophidian.Losgap {
	/// <summary>
	/// Represents a two-dimensional circle on the XY plane.
	/// </summary>
	public struct Circle : IToleranceEquatable<Circle> {
		/// <summary>
		/// The position of the centre of this circle in 2D space.
		/// </summary>
		public readonly Vector2 Center;
		/// <summary>
		/// The radius of this circle. May be zero, but will never by negative.
		/// </summary>
		public readonly float Radius;

		/// <summary>
		/// The surface area covered by this circle.
		/// </summary>
		public float Area {
			get {
				return MathUtils.PI * Radius * Radius;
			}
		}

		/// <summary>
		/// The length of a curved line running around the edge of this circle.
		/// </summary>
		public float Circumference {
			get {
				return MathUtils.TWO_PI * Radius;
			}
		}

		/// <summary>
		/// Creates a new Circle with the given <paramref name="center"/> position and <paramref name="radius"/>.
		/// </summary>
		/// <param name="center">The position of the centre of this circle.</param>
		/// <param name="radius">The radius of this circle.</param>
		public Circle(Vector2 center, float radius) {
			Center = center;
			Radius = Math.Abs(radius);
		}

		/// <summary>
		/// Creates a new Circle with the given centre position (<paramref name="x"/> &amp; <paramref name="y"/>) and
		/// <paramref name="radius"/>.
		/// </summary>
		/// <param name="x">The position along the X-axis of the centre of this circle.</param>
		/// <param name="y">The position along the Y-axis of the centre of this circle.</param>
		/// <param name="radius">The radius of this circle.</param>
		public Circle(float x, float y, float radius) : this(new Vector2(x, y), radius) { }

		#region Shape interactions
		/// <summary>
		/// Indicates whether or not the given <paramref name="point"/> lies on this circle.
		/// </summary>
		/// <param name="point">The point to check.</param>
		/// <returns>True if <paramref name="point"/> lies on/inside this circle, false if it lies outside.</returns>
		public bool Contains(Vector2 point) {
			float errorMargin = MathUtils.FlopsErrorMargin;
			return Vector2.DistanceSquared(Center, point) <= Radius * Radius + errorMargin * errorMargin;
		}

		/// <summary>
		/// Indicates whether or not the given shape is contained entirely within this circle (no overlap).
		/// </summary>
		/// <param name="other">The shape to check.</param>
		/// <returns>True if the other shape is completely within this circle, false if any part of it
		/// is outside the circle.</returns>
		public bool Contains(Circle other) {
			return Vector2.Distance(Center, other.Center) + other.Radius <= Radius + MathUtils.FlopsErrorMargin;
		}

		/// <summary>
		/// Indicates whether or not the given shape is contained entirely within this circle (no overlap).
		/// </summary>
		/// <param name="other">The shape to check.</param>
		/// <returns>True if the other shape is completely within this circle, false if any part of it
		/// is outside the circle.</returns>
		public bool Contains(Rectangle other) {
			float errorMargin = MathUtils.FlopsErrorMargin;
			float radSqPlusErrMargin = Radius * Radius + errorMargin * errorMargin;
			return Vector2.DistanceSquared(other.BottomLeft, Center) <= radSqPlusErrMargin
				&& Vector2.DistanceSquared(new Vector2(other.BottomLeft.X, other.BottomLeft.Y + other.Height), Center) <= radSqPlusErrMargin
				&& Vector2.DistanceSquared(new Vector2(other.BottomLeft.X + other.Width, other.BottomLeft.Y), Center) <= radSqPlusErrMargin
				&& Vector2.DistanceSquared(new Vector2(other.BottomLeft.X + other.Width, other.BottomLeft.Y + other.Height), Center) <= radSqPlusErrMargin;
		}

		/// <summary>
		/// Indicates the shortest distance from the given <paramref name="point"/> to the edge of this circle
		/// (not the centre!).
		/// </summary>
		/// <param name="point">The point to check.</param>
		/// <returns>The shortest distance from the given <paramref name="point"/> to the edge of this circle.
		/// If the <paramref name="point"/> lies on this circle, <c>0f</c> will be returned.</returns>
		public float DistanceFrom(Vector2 point) {
			return Math.Max(Vector2.Distance(Center, point) - Radius, 0f);
		}

		/// <summary>
		/// Indicates the shortest distance from the edge of the given shape to the edge of this circle.
		/// </summary>
		/// <param name="other">The shape to check.</param>
		/// <returns>The shortest distance connecting the outer-edges of both shapes.
		/// If the shapes intersect, <c>0f</c> will be returned.</returns>
		public float DistanceFrom(Circle other) {
			return Math.Max(Vector2.Distance(Center, other.Center) - (Radius + other.Radius), 0f);
		}

		/// <summary>
		/// Indicates the shortest distance from the edge of the given shape to the edge of this circle.
		/// </summary>
		/// <param name="other">The shape to check.</param>
		/// <returns>The shortest distance connecting the outer-edges of both shapes.
		/// If the shapes intersect, <c>0f</c> will be returned.</returns>
		public float DistanceFrom(Rectangle other) {
			if (Intersects(other)) return 0f;

			float distanceX = Math.Max(Math.Abs(
				Center.X - (float) MathUtils.Clamp(Center.X, other.BottomLeft.X, other.BottomLeft.X + other.Width)) - Radius, 
				0f);
			float distanceY = Math.Max(Math.Abs(
				Center.Y - (float) MathUtils.Clamp(Center.Y, other.BottomLeft.Y, other.BottomLeft.Y + other.Height)) - Radius, 
				0f);

			return (float) Math.Sqrt(distanceX * distanceX + distanceY * distanceY);
		}

		/// <summary>
		/// Indicates whether any portion of this circle and the <paramref name="other"/> shape overlap.
		/// </summary>
		/// <param name="other">The shape to check against this one.</param>
		/// <returns>True if any part of both shapes overlap, false if they are entirely separate.</returns>
		public bool Intersects(Circle other) {
			return Vector2.Distance(Center, other.Center) <= Radius + other.Radius + MathUtils.FlopsErrorMargin;
		}

		/// <summary>
		/// Indicates whether any portion of this circle and the <paramref name="other"/> shape overlap.
		/// </summary>
		/// <param name="other">The shape to check against this one.</param>
		/// <returns>True if any part of both shapes overlap, false if they are entirely separate.</returns>
		public bool Intersects(Rectangle other) {
			return other.Intersects(this);
		}
		#endregion

		/// <summary>
		/// Returns a string representation of this Circle.
		/// </summary>
		/// <returns>A string that lists the geometric properties of this shape.</returns>
		public override string ToString() {
			return "(Circle Centre:" + Center + " Radius:" + Radius.ToString(3) + ")";
		}

		#region Equality
		/// <summary>
		/// Test whether this object is EXACTLY equal to <paramref name="other"/>. No floating-point tolerance will be accounted
		/// for: The two structures must be bitwise identical.
		/// </summary>
		/// <param name="other">The other Circle to test against.</param>
		/// <returns>True if this object is EXACTLY equal to <paramref name="other"/>.</returns>
		public bool EqualsExactly(Circle other) {
			// ReSharper disable CompareOfFloatsByEqualityOperator
			return Center.EqualsExactly(other.Center) && Radius == other.Radius;
			// ReSharper restore CompareOfFloatsByEqualityOperator
		}

		/// <summary>
		/// Test whether this object is equal to <paramref name="other"/> within a given <paramref name="tolerance"/>. Even if the
		/// two structures are not identical, this method will return <c>true</c> if they are 'close enough' (e.g. within the tolerance).
		/// </summary>
		/// <param name="other">The other Circle to test against.</param>
		/// <param name="tolerance">The amount of tolerance to give. For any floating-point component in both objects, if the difference
		/// between them is less than this value, they will be considered equal.</param>
		/// <returns>True if this object is equal to <paramref name="other"/> with the given <paramref name="tolerance"/>.</returns>
		public bool EqualsWithTolerance(Circle other, double tolerance) {
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
		public bool Equals(Circle other) {
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
			return obj is Circle && Equals((Circle) obj);
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
		public static bool operator ==(Circle left, Circle right) {
			return left.Equals(right);
		}

		/// <summary>
		/// Indicates whether or not the two shapes are unequal.
		/// </summary>
		/// <param name="left">The first shape.</param>
		/// <param name="right">The second shape.</param>
		/// <returns>False if the shapes represent equal dimensions and positions, true if not.</returns>
		public static bool operator !=(Circle left, Circle right) {
			return !left.Equals(right);
		}
		#endregion
	}
}