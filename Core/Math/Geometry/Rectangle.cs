// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 25 11 2014 at 17:22 by Ben Bowen

using System;
using System.Collections.Generic;

namespace Ophidian.Losgap {
	/// <summary>
	/// Represents a two-dimensional (axis-aligned) rectangle on the XY plane.
	/// </summary>
	public struct Rectangle : IToleranceEquatable<Rectangle> {
		internal const int NUM_CORNERS = 4;

		/// <summary>
		/// The position of the bottom-left corner of this rectangle, in 2D space.
		/// </summary>
		public readonly Vector2 BottomLeft;
		/// <summary>
		/// The size of this rectangle along the x-axis.
		/// </summary>
		public readonly float Width;
		/// <summary>
		/// The size of this rectangle along the y-axis.
		/// </summary>
		public readonly float Height;

		/// <summary>
		/// The surface area covered by this rectangle.
		/// </summary>
		public float Area {
			get {
				return Width * Height;
			}
		}

		/// <summary>
		/// Creates a new Rectangle with the given <paramref name="bottomLeft"/> corner,
		/// <paramref name="width"/>, and <paramref name="height"/>.
		/// </summary>
		/// <param name="bottomLeft">The position of the bottom-left corner of this rectangle.</param>
		/// <param name="width">The size of this rectangle along the x-axis.</param>
		/// <param name="height">The size of this rectangle along the y-axis.</param>
		public Rectangle(Vector2 bottomLeft, float width, float height) {
			if (width < 0f) {
				bottomLeft = new Vector2(bottomLeft.X + width, bottomLeft.Y);
				width = -width;
			}
			if (height < 0f) {
				bottomLeft = new Vector2(bottomLeft.X, bottomLeft.Y + height);
				height = -height;
			}
			BottomLeft = bottomLeft;
			Width = width;
			Height = height;
		}

		/// <summary>
		/// Creates a new Rectangle with the given bottom-left corner position (<paramref name="x"/> &amp; <paramref name="y"/>),
		/// <paramref name="width"/>, and <paramref name="height"/>.
		/// </summary>
		/// <param name="x">The position on the x-axis of the bottom-left corner of this rectangle.</param>
		/// <param name="y">The position on the y-axis of the bottom-left corner of this rectangle.</param>
		/// <param name="width">The size of this rectangle along the x-axis.</param>
		/// <param name="height">The size of this rectangle along the y-axis.</param>
		public Rectangle(float x, float y, float width, float height) : this(new Vector2(x, y), width, height) { }


		/// <summary>
		/// An enumeration of corners in a 2D rectangle.
		/// </summary>
		public enum RectangleCorner {
			/// <summary>
			/// The top-left corner.
			/// </summary>
			TopLeft = NUM_CORNERS - 4,
			/// <summary>
			/// The top-right corner.
			/// </summary>
			TopRight = NUM_CORNERS - 3,
			/// <summary>
			/// The bottom-left corner.
			/// </summary>
			BottomLeft = NUM_CORNERS - 2,
			/// <summary>
			/// The bottom-right corner.
			/// </summary>
			BottomRight = NUM_CORNERS - 1
		}

		/// <summary>
		/// Returns the X co-ordinate of the requested <paramref name="corner"/> of this rectangle.
		/// </summary>
		/// <param name="corner">The corner whose position it is you wish to obtain.</param>
		/// <returns>The position on the X-axis of the requested <paramref name="corner"/> of this rectangle.</returns>
		public float GetCornerX(RectangleCorner corner) {
			Assure.True(Enum.IsDefined(typeof(RectangleCorner), corner), "Invalid corner constant: " + corner + " is not a valid RectangleCorner value.");
			switch (corner) {
				case RectangleCorner.TopRight:
				case RectangleCorner.BottomRight:
					return BottomLeft.X + Width;
				default: return BottomLeft.X;
			}
		}

		/// <summary>
		/// Returns the Y co-ordinate of the requested <paramref name="corner"/> of this rectangle.
		/// </summary>
		/// <param name="corner">The corner whose position it is you wish to obtain.</param>
		/// <returns>The position on the Y-axis of the requested <paramref name="corner"/> of this rectangle.</returns>
		public float GetCornerY(RectangleCorner corner) {
			Assure.True(Enum.IsDefined(typeof(RectangleCorner), corner), "Invalid corner constant: " + corner + " is not a valid RectangleCorner value.");
			switch (corner) {
				case RectangleCorner.TopLeft:
				case RectangleCorner.TopRight:
					return BottomLeft.Y + Height;
				default: return BottomLeft.Y;
			}
		}

		/// <summary>
		/// Returns the X and Y co-ordinate of the requested <paramref name="corner"/> of this rectangle.
		/// </summary>
		/// <param name="corner">The corner whose position it is you wish to obtain.</param>
		/// <returns>A <see cref="Vector2"/> whose <see cref="Vector2.X"/> and <see cref="Vector2.Y"/> components
		/// are set to the X and Y position of the requested <paramref name="corner"/> on this rectangle.</returns>
		public Vector2 GetCorner(RectangleCorner corner) {
			return new Vector2(GetCornerX(corner), GetCornerY(corner));
		}

		#region Shape interactions
		/// <summary>
		/// Indicates whether or not the given <paramref name="point"/> lies on this rectangle.
		/// </summary>
		/// <param name="point">The point to check.</param>
		/// <returns>True if <paramref name="point"/> lies on/inside this rectangle, false if it lies outside.</returns>
		public bool Contains(Vector2 point) {
			float errorMargin = MathUtils.FlopsErrorMargin;
			return point.X <= BottomLeft.X + Width + errorMargin
				&& point.X + errorMargin >= BottomLeft.X
				&& point.Y <= BottomLeft.Y + Height + errorMargin
				&& point.Y + errorMargin >= BottomLeft.Y;
		}

		/// <summary>
		/// Indicates whether or not the given shape is contained entirely within this rectangle (no overlap).
		/// </summary>
		/// <param name="other">The shape to check.</param>
		/// <returns>True if the other shape is completely within this rectangle, false if any part of it
		/// is outside the rectangle.</returns>
		public bool Contains(Rectangle other) {
			float errorMargin = MathUtils.FlopsErrorMargin;
			return other.BottomLeft.X + errorMargin >= BottomLeft.X
				&& other.BottomLeft.X + other.Width <= BottomLeft.X + Width + errorMargin
				&& other.BottomLeft.Y + errorMargin >= BottomLeft.Y
				&& other.BottomLeft.Y + other.Height <= BottomLeft.Y + Height + errorMargin;
		}

		/// <summary>
		/// Indicates whether or not the given shape is contained entirely within this rectangle (no overlap).
		/// </summary>
		/// <param name="other">The shape to check.</param>
		/// <returns>True if the other shape is completely within this rectangle, false if any part of it
		/// is outside the rectangle.</returns>
		public bool Contains(Circle other) {
			float errorMargin = MathUtils.FlopsErrorMargin;
			return other.Center.X + other.Radius <= BottomLeft.X + Width + errorMargin
				&& other.Center.Y + other.Radius <= BottomLeft.Y + Height + errorMargin
				&& other.Center.X - other.Radius + errorMargin >= BottomLeft.X
				&& other.Center.Y - other.Radius + errorMargin >= BottomLeft.Y;
		}

		/// <summary>
		/// Indicates the shortest distance from the given <paramref name="point"/> to the edge of this rectangle
		/// (not the centre!).
		/// </summary>
		/// <param name="point">The point to check.</param>
		/// <returns>The shortest distance from the given <paramref name="point"/> to the edge of this rectangle.
		/// If the <paramref name="point"/> lies on this rectangle, <c>0f</c> will be returned.</returns>
		public float DistanceFrom(Vector2 point) {
			return (float) Math.Sqrt(DistanceFromSquared(point));
		}

		/// <summary>
		/// Indicates the shortest distance from the edge of the given shape to the edge of this rectangle.
		/// </summary>
		/// <param name="other">The shape to check.</param>
		/// <returns>The shortest distance connecting the outer-edges of both shapes.
		/// If the shapes intersect, <c>0f</c> will be returned.</returns>
		public float DistanceFrom(Rectangle other) {
			if (Intersects(other)) return 0f;

			float min = Single.MaxValue;
			for (int i = 0; i < NUM_CORNERS; ++i) {
				min = Math.Min(min, DistanceFromSquared(other.GetCorner((RectangleCorner) i)));
				min = Math.Min(min, other.DistanceFromSquared(GetCorner((RectangleCorner) i)));
			}

			return (float) Math.Sqrt(min);
		}

		/// <summary>
		/// Indicates the shortest distance from the edge of the given shape to the edge of this rectangle.
		/// </summary>
		/// <param name="other">The shape to check.</param>
		/// <returns>The shortest distance connecting the outer-edges of both shapes.
		/// If the shapes intersect, <c>0f</c> will be returned.</returns>
		public float DistanceFrom(Circle other) {
			return other.DistanceFrom(this);
		}

		/// <summary>
		/// Indicates whether any portion of this rectangle and the <paramref name="other"/> shape overlap.
		/// </summary>
		/// <param name="other">The shape to check against this one.</param>
		/// <returns>True if any part of both shapes overlap, false if they are entirely separate.</returns>
		public bool Intersects(Rectangle other) {
			float errorMargin = MathUtils.FlopsErrorMargin;
			return (
					other.BottomLeft.X <= BottomLeft.X + Width + errorMargin
					&& other.BottomLeft.Y <= BottomLeft.Y + Height + errorMargin
					&& other.BottomLeft.X + other.Width + errorMargin >= BottomLeft.X
					&& other.BottomLeft.Y + other.Height + errorMargin >= BottomLeft.Y
				) || (
					BottomLeft.X <= other.BottomLeft.X + other.Width + errorMargin
					&& BottomLeft.Y <= other.BottomLeft.Y + other.Height + errorMargin
					&& BottomLeft.X + Width + errorMargin >= other.BottomLeft.X
					&& BottomLeft.Y + Height + errorMargin >= other.BottomLeft.Y
				);
		}

		/// <summary>
		/// Indicates whether any portion of this rectangle and the <paramref name="other"/> shape overlap.
		/// </summary>
		/// <param name="other">The shape to check against this one.</param>
		/// <returns>True if any part of both shapes overlap, false if they are entirely separate.</returns>
		public bool Intersects(Circle other) {
			if (Contains(other.Center)) return true;
			for (int i = 0; i < NUM_CORNERS; ++i) {
				if (other.Contains(GetCorner((RectangleCorner) i))) return true;
			}
			return false;
		}

		/// <summary>
		/// Determines the area of overlap between this Rectangle and <paramref name="other"/>.
		/// </summary>
		/// <param name="other">The other rectangle.</param>
		/// <returns>A new Rectangle that represents the area that is covered by both <c>this</c> and
		/// <paramref name="other"/>. If <c>this</c> and <paramref name="other"/> do not intersect at all,
		/// an 'empty' rectangle is returned.</returns>
		public Rectangle IntersectionWith(Rectangle other) {
			if (!Intersects(other)) return new Rectangle(0f, 0f, 0f, 0f);
			float x = Math.Max(BottomLeft.X, other.BottomLeft.X);
			float y = Math.Max(BottomLeft.Y, other.BottomLeft.Y);
			return new Rectangle(
				x, 
				y, 
				Math.Min(BottomLeft.X + Width, other.BottomLeft.X + other.Width) - x, 
				Math.Min(BottomLeft.Y + Height, other.BottomLeft.Y + other.Height) - y
			);
		}
		#endregion

		/// <summary>
		/// Returns a string representation of this Rectangle.
		/// </summary>
		/// <returns>A string that lists the geometric properties of this shape.</returns>
		public override string ToString() {
			return "(Rectangle BottomLeft:" + BottomLeft + " Width:" + Width.ToString(3) + " Height:" + Height.ToString(3) + ")";
		}


		#region Equality
		/// <summary>
		/// Test whether this object is EXACTLY equal to <paramref name="other"/>. No floating-point tolerance will be accounted
		/// for: The two structures must be bitwise identical.
		/// </summary>
		/// <param name="other">The other Rectangle to test against.</param>
		/// <returns>True if this object is EXACTLY equal to <paramref name="other"/>.</returns>
		public bool EqualsExactly(Rectangle other) {
			// ReSharper disable CompareOfFloatsByEqualityOperator
			return BottomLeft.EqualsExactly(other.BottomLeft) && Width == other.Width && Height == other.Height;
			// ReSharper restore CompareOfFloatsByEqualityOperator
		}

		/// <summary>
		/// Test whether this object is equal to <paramref name="other"/> within a given <paramref name="tolerance"/>. Even if the
		/// two structures are not identical, this method will return <c>true</c> if they are 'close enough' (e.g. within the tolerance).
		/// </summary>
		/// <param name="other">The other Rectangle to test against.</param>
		/// <param name="tolerance">The amount of tolerance to give. For any floating-point component in both objects, if the difference
		/// between them is less than this value, they will be considered equal.</param>
		/// <returns>True if this object is equal to <paramref name="other"/> with the given <paramref name="tolerance"/>.</returns>
		public bool EqualsWithTolerance(Rectangle other, double tolerance) {
			Assure.GreaterThanOrEqualTo(tolerance, 0f, "Tolerance must not be negative.");
			return BottomLeft.EqualsWithTolerance(other.BottomLeft, tolerance)
				&& Math.Abs(Width - other.Width) < tolerance
				&& Math.Abs(Height - other.Height) < tolerance;
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(Rectangle other) {
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
			return obj is Rectangle && Equals((Rectangle)obj);
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
				int hashCode = BottomLeft.GetHashCode();
				hashCode = (hashCode * 397) ^ Width.GetHashCode();
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
		public static bool operator ==(Rectangle left, Rectangle right) {
			return left.Equals(right);
		}

		/// <summary>
		/// Indicates whether or not the two shapes are unequal.
		/// </summary>
		/// <param name="left">The first shape.</param>
		/// <param name="right">The second shape.</param>
		/// <returns>False if the shapes represent equal dimensions and positions, true if not.</returns>
		public static bool operator !=(Rectangle left, Rectangle right) {
			return !left.Equals(right);
		}
		#endregion


		private float DistanceFromSquared(Vector2 point) {
			if (Contains(point)) return 0f;

			float distanceX = point.X - (float) MathUtils.Clamp(point.X, BottomLeft.X, BottomLeft.X + Width);
			float distanceY = point.Y - (float) MathUtils.Clamp(point.Y, BottomLeft.Y, BottomLeft.Y + Height);

			return distanceX * distanceX + distanceY * distanceY;
		}
	}
}