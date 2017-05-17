// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 26 11 2014 at 11:42 by Ben Bowen

using System;
using System.Collections.Generic;

namespace Ophidian.Losgap {
	/// <summary>
	/// Represents a three-dimensional (axis-aligned) cuboid.
	/// </summary>
	public struct Cuboid : IToleranceEquatable<Cuboid> {
		internal const int NUM_CORNERS = 8;

		/// <summary>
		/// The position of the front-bottom-left corner of this cuboid, in 3D space.
		/// </summary>
		public readonly Vector3 FrontBottomLeft; // Lol front bottom

		/// <summary>
		/// The size of this cuboid along the x-axis.
		/// </summary>
		public readonly float Width;
		/// <summary>
		/// The size of this cuboid along the y-axis.
		/// </summary>
		public readonly float Height;
		/// <summary>
		/// The size of this cuboid along the z-axis.
		/// </summary>
		public readonly float Depth;

		/// <summary>
		/// The entire space taken up by this cuboid.
		/// </summary>
		public float Volume {
			get {
				return Width * Height * Depth;
			}
		}

		/// <summary>
		/// The area covered by the surface of this cuboid.
		/// </summary>
		public float SurfaceArea {
			get {
				return 2f * (Width * Height + Height * Depth + Depth * Width);
			}
		}

		public Vector3 CenterPoint {
			get {
				return FrontBottomLeft + new Vector3(Width / 2f, Height / 2f, Depth / 2f);
			}
		}

		/// <summary>
		/// Creates a new Cuboid with the given <paramref name="frontBottomLeft"/> corner position, 
		/// <paramref name="width"/>, <paramref name="height"/>, and <paramref name="depth"/>.
		/// </summary>
		/// <param name="frontBottomLeft">The position of the front-bottom-left corner of this cuboid.</param>
		/// <param name="width">The size of this cuboid along the x-axis.</param>
		/// <param name="height">The size of this cuboid along the y-axis.</param>
		/// <param name="depth">The size of this cuboid along the z-axis.</param>
		public Cuboid(Vector3 frontBottomLeft, float width, float height, float depth) {
			if (width < 0f) {
				frontBottomLeft = new Vector3(frontBottomLeft, x: frontBottomLeft.X + width);
				width = -width;
			}
			if (height < 0f) {
				frontBottomLeft = new Vector3(frontBottomLeft, y: frontBottomLeft.Y + height);
				height = -height;
			}
			if (depth < 0f) {
				frontBottomLeft = new Vector3(frontBottomLeft, z: frontBottomLeft.Z + depth);
				depth = -depth;
			}
			FrontBottomLeft = frontBottomLeft;
			Width = width;
			Height = height;
			Depth = depth;
		}

		/// <summary>
		/// Creates a new Cuboid with the X/<see cref="Width"/> and Y/<see cref="Height"/> dimensions
		/// specified by a <see cref="Rectangle"/> (<paramref name="frontFace"/>). The remaining dimension is specified by the
		/// <paramref name="z"/> and <paramref name="depth"/> parameters.
		/// </summary>
		/// <param name="frontFace">A rectangle that specifies the front face of the resultant cuboid in the XY plane.</param>
		/// <param name="z">The position on the z-axis of the front-bottom-left corner of this cuboid.</param>
		/// <param name="depth">The size of this cuboid along the z-axis.</param>
		public Cuboid(Rectangle frontFace, float z, float depth)
			: this(new Vector3(frontFace.BottomLeft, z: z), frontFace.Width, frontFace.Height, depth) { }

		/// <summary>
		/// Creates a new cuboid by specifying the dimensions along all three axes.
		/// </summary>
		/// <param name="x">The position on the x-axis of the front-bottom-left corner of this cuboid.</param>
		/// <param name="y">The position on the y-axis of the front-bottom-left corner of this cuboid.</param>
		/// <param name="z">The position on the z-axis of the front-bottom-left corner of this cuboid.</param>
		/// <param name="width">The size of this cuboid along the x-axis.</param>
		/// <param name="height">The size of this cuboid along the y-axis.</param>
		/// <param name="depth">The size of this cuboid along the z-axis.</param>
		public Cuboid(float x, float y, float z, float width, float height, float depth) : this(new Vector3(x, y, z), width, height, depth) { }

		/// <summary>
		/// An enumeration of corners in a 3D cuboid.
		/// </summary>
		public enum CuboidCorner {
			/// <summary>
			/// The front-top-left corner.
			/// </summary>
			FrontTopLeft = NUM_CORNERS - 8,
			/// <summary>
			/// The front-top-right corner.
			/// </summary>
			FrontTopRight = NUM_CORNERS - 7,
			/// <summary>
			/// The front-bottom-left corner.
			/// </summary>
			FrontBottomLeft = NUM_CORNERS - 6,
			/// <summary>
			/// The front-bottom-right corner.
			/// </summary>
			FrontBottomRight = NUM_CORNERS - 5,
			/// <summary>
			/// The back-top-left corner.
			/// </summary>
			BackTopLeft = NUM_CORNERS - 4,
			/// <summary>
			/// The back-top-right corner.
			/// </summary>
			BackTopRight = NUM_CORNERS - 3,
			/// <summary>
			/// The back-bottom-left corner.
			/// </summary>
			BackBottomLeft = NUM_CORNERS - 2,
			/// <summary>
			/// The back-bottom-right corner.
			/// </summary>
			BackBottomRight = NUM_CORNERS - 1
		}

		/// <summary>
		/// An enumeration of sides of a 3D cuboid.
		/// </summary>
		public enum CuboidSide {
			/// <summary>
			/// The positive-Z-facing side of the cuboid.
			/// </summary>
			Front,
			/// <summary>
			/// The positive-Y-facing side of the cuboid.
			/// </summary>
			Top,
			/// <summary>
			/// The negative-Z-facing side of the cuboid.
			/// </summary>
			Back,
			/// <summary>
			/// The negative-Y-facing side of the cuboid.
			/// </summary>
			Bottom,
			/// <summary>
			/// The negative-X-facing side of the cuboid.
			/// </summary>
			Left,
			/// <summary>
			/// The positive-X-facing side of the cuboid.
			/// </summary>
			Right
		}

		/// <summary>
		/// Returns the X co-ordinate of the requested <paramref name="corner"/> of this cuboid.
		/// </summary>
		/// <param name="corner">The corner whose position it is you wish to obtain.</param>
		/// <returns>The position on the X-axis of the requested <paramref name="corner"/> of this cuboid.</returns>
		public float GetCornerX(CuboidCorner corner) {
			Assure.True(Enum.IsDefined(typeof(CuboidCorner), corner), "Invalid corner constant: " + corner + " is not a valid CuboidCorner value.");
			switch (corner) {
				case CuboidCorner.FrontTopRight:
				case CuboidCorner.FrontBottomRight:
				case CuboidCorner.BackTopRight:
				case CuboidCorner.BackBottomRight:
					return FrontBottomLeft.X + Width;
				default:
					return FrontBottomLeft.X;
			}
		}

		/// <summary>
		/// Returns the Y co-ordinate of the requested <paramref name="corner"/> of this cuboid.
		/// </summary>
		/// <param name="corner">The corner whose position it is you wish to obtain.</param>
		/// <returns>The position on the Y-axis of the requested <paramref name="corner"/> of this cuboid.</returns>
		public float GetCornerY(CuboidCorner corner) {
			Assure.True(Enum.IsDefined(typeof(CuboidCorner), corner), "Invalid corner constant: " + corner + " is not a valid CuboidCorner value.");
			switch (corner) {
				case CuboidCorner.FrontTopRight:
				case CuboidCorner.FrontTopLeft:
				case CuboidCorner.BackTopRight:
				case CuboidCorner.BackTopLeft:
					return FrontBottomLeft.Y + Height;
				default:
					return FrontBottomLeft.Y;
			}
		}

		/// <summary>
		/// Returns the Z co-ordinate of the requested <paramref name="corner"/> of this cuboid.
		/// </summary>
		/// <param name="corner">The corner whose position it is you wish to obtain.</param>
		/// <returns>The position on the Z-axis of the requested <paramref name="corner"/> of this cuboid.</returns>
		public float GetCornerZ(CuboidCorner corner) {
			Assure.True(Enum.IsDefined(typeof(CuboidCorner), corner), "Invalid corner constant: " + corner + " is not a valid CuboidCorner value.");
			switch (corner) {
				case CuboidCorner.BackTopLeft:
				case CuboidCorner.BackTopRight:
				case CuboidCorner.BackBottomLeft:
				case CuboidCorner.BackBottomRight:
					return FrontBottomLeft.Z + Depth;
				default:
					return FrontBottomLeft.Z;
			}
		}

		/// <summary>
		/// Returns the X, Y and Z co-ordinate of the requested <paramref name="corner"/> of this rectangle.
		/// </summary>
		/// <param name="corner">The corner whose position it is you wish to obtain.</param>
		/// <returns>A <see cref="Vector3"/> whose <see cref="Vector3.X"/>, <see cref="Vector3.Y"/> and <see cref="Vector3.Z"/> components
		/// are set to the X, Y and Z position of the requested <paramref name="corner"/> on this cuboid.</returns>
		public Vector3 GetCorner(CuboidCorner corner) {
			return new Vector3(GetCornerX(corner), GetCornerY(corner), GetCornerZ(corner));
		}

		/// <summary>
		/// Returns a rectangle describing the requested <paramref name="side"/> of this cuboid (transposed in to the 2D XY plane).
		/// </summary>
		/// <param name="side">The side that you wish to get a description of.</param>
		/// <returns>A <see cref="Rectangle"/> whose dimensions represent the requested <paramref name="side"/>
		/// of this cuboid.</returns>
		public Rectangle GetSide(CuboidSide side) {
			Assure.True(Enum.IsDefined(typeof(CuboidSide), side), "Invalid side constant: " + side + " is not a valid CuboidSide value.");
			switch (side) {
				case CuboidSide.Top:
				case CuboidSide.Bottom:
					return new Rectangle((Vector2) FrontBottomLeft[0, 2], Width, Depth);
				case CuboidSide.Left:
				case CuboidSide.Right:
					return new Rectangle((Vector2) FrontBottomLeft[2, 1], Depth, Height);
				default:
					return new Rectangle((Vector2) FrontBottomLeft[0, 1], Width, Height);
			}
		}

		#region Shape interactions
		/// <summary>
		/// Indicates whether or not the given <paramref name="point"/> lies inside this cuboid.
		/// </summary>
		/// <param name="point">The point to check.</param>
		/// <returns>True if the given <paramref name="point"/> is inside this cuboid, false if it is outside.</returns>
		public bool Contains(Vector3 point) {
			float errorMargin = MathUtils.FlopsErrorMargin;
			return point.X <= FrontBottomLeft.X + Width + errorMargin
				&& point.X + errorMargin >= FrontBottomLeft.X
				&& point.Y <= FrontBottomLeft.Y + Height + errorMargin
				&& point.Y + errorMargin >= FrontBottomLeft.Y
				&& point.Z <= FrontBottomLeft.Z + Depth + errorMargin
				&& point.Z + errorMargin >= FrontBottomLeft.Z;
		}

		/// <summary>
		/// Indicates whether or not the given <paramref name="ray"/> is contained entirely within this cuboid.
		/// </summary>
		/// <param name="ray">The ray to check.</param>
		/// <returns>True if the <paramref name="ray"/> is not <see cref="Ray.IsInfiniteLength">infinite length</see> and its
		/// <see cref="Ray.StartPoint"/> and <see cref="Ray.EndPoint"/> are inside this shape.</returns>
		public bool Contains(Ray ray) {
			return !ray.IsInfiniteLength && Contains(ray.StartPoint) && Contains(ray.EndPoint.Value);
		}

		/// <summary>
		/// Indicates whether or not the given shape is contained entirely within this cuboid (no part of the <paramref name="other"/>
		/// shape may be outside this one).
		/// </summary>
		/// <param name="other">The shape to check.</param>
		/// <returns>True if the other shape is completely within this cuboid, false if any part of it
		/// is outside the cuboid.</returns>
		public bool Contains(Cuboid other) {
			float errorMargin = MathUtils.FlopsErrorMargin;
			return other.FrontBottomLeft.X + errorMargin >= FrontBottomLeft.X
				&& other.FrontBottomLeft.X + other.Width <= FrontBottomLeft.X + Width + errorMargin
				&& other.FrontBottomLeft.Y + errorMargin >= FrontBottomLeft.Y
				&& other.FrontBottomLeft.Y + other.Height <= FrontBottomLeft.Y + Height + errorMargin
				&& other.FrontBottomLeft.Z + errorMargin >= FrontBottomLeft.Z
				&& other.FrontBottomLeft.Z + other.Depth <= FrontBottomLeft.Z + Depth + errorMargin;
		}

		/// <summary>
		/// Indicates whether or not the given shape is contained entirely within this cuboid (no part of the <paramref name="other"/>
		/// shape may be outside this one).
		/// </summary>
		/// <param name="other">The shape to check.</param>
		/// <returns>True if the other shape is completely within this cuboid, false if any part of it
		/// is outside the cuboid.</returns>
		public bool Contains(Cone other) {
			float errorMargin = MathUtils.FlopsErrorMargin;
			if (other.IsConicalFrustum) {
				bool topSectionFits =
					other.TopCenter.X - other.TopRadius + errorMargin >= FrontBottomLeft.X
					&& other.TopCenter.X + other.TopRadius <= FrontBottomLeft.X + Width + errorMargin
					&& other.TopCenter.Z - other.TopRadius + errorMargin >= FrontBottomLeft.Z
					&& other.TopCenter.Z + other.TopRadius + errorMargin <= FrontBottomLeft.Z + Depth + errorMargin;
				if (!topSectionFits) return false;
			}

			return other.TopCenter.Y + errorMargin >= FrontBottomLeft.Y
				&& other.TopCenter.Y + other.Height <= FrontBottomLeft.Y + Height + errorMargin
				&& other.TopCenter.X - other.BottomRadius + errorMargin >= FrontBottomLeft.X
				&& other.TopCenter.X + other.BottomRadius <= FrontBottomLeft.X + Width + errorMargin
				&& other.TopCenter.Z - other.BottomRadius + errorMargin >= FrontBottomLeft.Z
				&& other.TopCenter.Z + other.BottomRadius <= FrontBottomLeft.Z + Depth + errorMargin;
		}

		/// <summary>
		/// Indicates whether or not the given shape is contained entirely within this cuboid (no part of the <paramref name="other"/>
		/// shape may be outside this one).
		/// </summary>
		/// <param name="other">The shape to check.</param>
		/// <returns>True if the other shape is completely within this cuboid, false if any part of it
		/// is outside the cuboid.</returns>
		public bool Contains(Cylinder other) {
			float errorMargin = MathUtils.FlopsErrorMargin;
			return other.Center.X + other.Radius <= FrontBottomLeft.X + Width + errorMargin
				&& other.Center.X - other.Radius + errorMargin >= FrontBottomLeft.X
				&& other.Center.Z + other.Radius <= FrontBottomLeft.Z + Depth + errorMargin
				&& other.Center.Z - other.Radius + errorMargin >= FrontBottomLeft.Z
				&& other.Center.Y + other.Height / 2f <= FrontBottomLeft.Y + Height + errorMargin
				&& other.Center.Y - other.Height / 2f + errorMargin >= FrontBottomLeft.Y;
		}

		/// <summary>
		/// Indicates whether or not the given shape is contained entirely within this cuboid (no part of the <paramref name="other"/>
		/// shape may be outside this one).
		/// </summary>
		/// <param name="other">The shape to check.</param>
		/// <returns>True if the other shape is completely within this cuboid, false if any part of it
		/// is outside the cuboid.</returns>
		public bool Contains(Sphere other) {
			float errorMargin = MathUtils.FlopsErrorMargin;
			return other.Center.X + other.Radius <= FrontBottomLeft.X + Width + errorMargin
				&& other.Center.Y + other.Radius <= FrontBottomLeft.Y + Height + errorMargin
				&& other.Center.Z + other.Radius <= FrontBottomLeft.Z + Depth + errorMargin
				&& other.Center.X - other.Radius + errorMargin >= FrontBottomLeft.X
				&& other.Center.Y - other.Radius + errorMargin >= FrontBottomLeft.Y
				&& other.Center.Z - other.Radius + errorMargin >= FrontBottomLeft.Z;
		}

		/// <summary>
		/// Indicates the shortest distance from the given <paramref name="point"/> to the edge of this cuboid
		/// (not the centre!).
		/// </summary>
		/// <param name="point">The point to check.</param>
		/// <returns>The shortest distance from the given <paramref name="point"/> to the edge of this cuboid.
		/// If the <paramref name="point"/> is inside this rectangle, <c>0f</c> will be returned.</returns>
		public float DistanceFrom(Vector3 point) {
			return (float) Math.Sqrt(DistanceFromSquared(point));
		}

		/// <summary>
		/// Indicates the shortest distance from the edge of the given shape to the edge of this cuboid.
		/// </summary>
		/// <param name="other">The shape to check.</param>
		/// <returns>The shortest distance connecting the outer-edges of both shapes.
		/// If the shapes intersect, <c>0f</c> will be returned.</returns>
		public float DistanceFrom(Cuboid other) {
			if (Intersects(other)) return 0f;

			float min = Single.MaxValue;
			for (int i = 0; i < NUM_CORNERS; ++i ) {
				min = Math.Min(min, DistanceFromSquared(other.GetCorner((CuboidCorner) i)));
				min = Math.Min(min, other.DistanceFromSquared(GetCorner((CuboidCorner) i)));
			}

			return (float) Math.Sqrt(min);
		}

		/// <summary>
		/// Indicates the shortest distance from the edge of the given shape to the edge of this cuboid.
		/// </summary>
		/// <param name="other">The shape to check.</param>
		/// <returns>The shortest distance connecting the outer-edges of both shapes.
		/// If the shapes intersect, <c>0f</c> will be returned.</returns>
		public float DistanceFrom(Sphere other) {
			return other.DistanceFrom(this);
		}

		/// <summary>
		/// Indicates the shortest distance from the edge of this cuboid to the <paramref name="plane"/>.
		/// </summary>
		/// <param name="plane">The plane to get the distance to.</param>
		/// <returns>The shortest distance connecting the outer-edges of this cuboid to the <paramref name="plane"/>.
		/// If the plane intersects this cuboid, <c>0f</c> will be returned.</returns>
		public float DistanceFrom(Plane plane) {
			return plane.DistanceFrom(this);
		}

		/// <summary>
		/// Indicates whether any portion of this cuboid and the <paramref name="other"/> shape overlap.
		/// </summary>
		/// <param name="other">The shape to check against this one.</param>
		/// <returns>True if any part of both shapes overlap, false if they are entirely separate.</returns>
		public bool Intersects(Cuboid other) {
			float errorMargin = MathUtils.FlopsErrorMargin;
			return (
					other.FrontBottomLeft.X <= FrontBottomLeft.X + Width + errorMargin 
					&& other.FrontBottomLeft.Y <= FrontBottomLeft.Y + Height + errorMargin 
					&& other.FrontBottomLeft.Z <= FrontBottomLeft.Z + Depth + errorMargin
					&& other.FrontBottomLeft.X + other.Width + errorMargin >= FrontBottomLeft.X 
					&& other.FrontBottomLeft.Y + other.Height + errorMargin >= FrontBottomLeft.Y 
					&& other.FrontBottomLeft.Z + other.Depth + errorMargin >= FrontBottomLeft.Z
				) || (
					FrontBottomLeft.X <= other.FrontBottomLeft.X + other.Width + errorMargin 
					&& FrontBottomLeft.Y <= other.FrontBottomLeft.Y + other.Height + errorMargin 
					&& FrontBottomLeft.Z <= other.FrontBottomLeft.Z + other.Depth + errorMargin
					&& FrontBottomLeft.X + Width + errorMargin >= other.FrontBottomLeft.X 
					&& FrontBottomLeft.Y + Height + errorMargin >= other.FrontBottomLeft.Y 
					&& FrontBottomLeft.Z + Depth + errorMargin >= other.FrontBottomLeft.Z
				);
		}

		/// <summary>
		/// Indicates whether any portion of this cuboid and the <paramref name="other"/> shape overlap.
		/// </summary>
		/// <param name="other">The shape to check against this one.</param>
		/// <returns>True if any part of both shapes overlap, false if they are entirely separate.</returns>
		public bool Intersects(Cylinder other) {
			float errorMargin = MathUtils.FlopsErrorMargin;
			return other.Center.X + other.Radius + errorMargin >= FrontBottomLeft.X 
				&& other.Center.X - other.Radius <= FrontBottomLeft.X + Width + errorMargin
				&& other.Center.Y + other.Height / 2f + errorMargin >= FrontBottomLeft.Y 
				&& other.Center.Y - other.Height / 2f <= FrontBottomLeft.Y + Height + errorMargin
				&& other.Center.Z + other.Radius >= FrontBottomLeft.Z + errorMargin 
				&& other.Center.Z - other.Radius <= FrontBottomLeft.Z + Depth + errorMargin;
		}

		/// <summary>
		/// Indicates whether any portion of this cuboid and the <paramref name="other"/> shape overlap.
		/// </summary>
		/// <param name="other">The shape to check against this one.</param>
		/// <returns>True if any part of both shapes overlap, false if they are entirely separate.</returns>
		public bool Intersects(Sphere other) {
			return DistanceFrom(other.Center) < other.Radius + MathUtils.FlopsErrorMargin;
		}

		/// <summary>
		/// Indicates whether this cuboid crosses the given <paramref name="plane"/>.
		/// </summary>
		/// <param name="plane">The plane to check for intersection with this shape.</param>
		/// <returns>True if any part of this cuboid crosses the <paramref name="plane"/> (or sits on it).</returns>
		public bool Intersects(Plane plane) {
			return plane.Intersects(this);
		}

		/// <summary>
		/// Determines the area of overlap between this Cuboid and <paramref name="other"/>.
		/// </summary>
		/// <param name="other">The other cuboid.</param>
		/// <returns>A new Cuboid that represents the volume that is encapsulated by both <c>this</c> and
		/// <paramref name="other"/>. If <c>this</c> and <paramref name="other"/> do not intersect at all,
		/// an 'empty' cuboid is returned.</returns>
		public Cuboid IntersectionWith(Cuboid other) {
			if (!Intersects(other)) return new Cuboid(0f, 0f, 0f, 0f, 0f, 0f);
			float x = Math.Max(FrontBottomLeft.X, other.FrontBottomLeft.X);
			float y = Math.Max(FrontBottomLeft.Y, other.FrontBottomLeft.Y);
			float z = Math.Max(FrontBottomLeft.Z, other.FrontBottomLeft.Z);
			return new Cuboid(
				x,
				y,
				z,
				Math.Min(FrontBottomLeft.X + Width, other.FrontBottomLeft.X + other.Width) - x,
				Math.Min(FrontBottomLeft.Y + Height, other.FrontBottomLeft.Y + other.Height) - y,
				Math.Min(FrontBottomLeft.Z + Depth, other.FrontBottomLeft.Z + other.Depth) - z
			);
		}

		/// <summary>
		/// Determines the first point on this cuboid that is touched by the given <paramref name="ray"/>.
		/// </summary>
		/// <param name="ray">The ray to test for intersection with this cuboid.</param>
		/// <returns>A <see cref="Vector3"/> indicating the first point on the cuboid edge touched by the ray,
		/// or <c>null</c> if no intersection occurs.</returns>
		public Vector3? IntersectionWith(Ray ray) {
			return ray.IntersectionWith(this);
		}
		#endregion

		/// <summary>
		/// Returns a string representation of this Cuboid.
		/// </summary>
		/// <returns>A string that lists the geometric properties of this shape.</returns>
		public override string ToString() {
			return "(Cuboid FrontBottomLeft:" + FrontBottomLeft + 
				" Width:" + Width.ToString(3) + " Height:" + Height.ToString(3) + " Depth:" + Depth.ToString(3) + ")";
		}


		#region Equality
		/// <summary>
		/// Test whether this object is EXACTLY equal to <paramref name="other"/>. No floating-point tolerance will be accounted
		/// for: The two structures must be bitwise identical.
		/// </summary>
		/// <param name="other">The other Cuboid to test against.</param>
		/// <returns>True if this object is EXACTLY equal to <paramref name="other"/>.</returns>
		public bool EqualsExactly(Cuboid other) {
			// ReSharper disable CompareOfFloatsByEqualityOperator
			return FrontBottomLeft.EqualsExactly(other.FrontBottomLeft) && Width == other.Width && Height == other.Height && Depth == other.Depth;
			// ReSharper restore CompareOfFloatsByEqualityOperator
		}

		/// <summary>
		/// Test whether this object is equal to <paramref name="other"/> within a given <paramref name="tolerance"/>. Even if the
		/// two structures are not identical, this method will return <c>true</c> if they are 'close enough' (e.g. within the tolerance).
		/// </summary>
		/// <param name="other">The other Cuboid to test against.</param>
		/// <param name="tolerance">The amount of tolerance to give. For any floating-point component in both objects, if the difference
		/// between them is less than this value, they will be considered equal.</param>
		/// <returns>True if this object is equal to <paramref name="other"/> with the given <paramref name="tolerance"/>.</returns>
		public bool EqualsWithTolerance(Cuboid other, double tolerance) {
			Assure.GreaterThanOrEqualTo(tolerance, 0f, "Tolerance must not be negative.");
			return FrontBottomLeft.EqualsWithTolerance(other.FrontBottomLeft, tolerance)
				&& Math.Abs(Width - other.Width) < tolerance
				&& Math.Abs(Height - other.Height) < tolerance
				&& Math.Abs(Depth - other.Depth) < tolerance;
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(Cuboid other) {
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
			return obj is Cuboid && Equals((Cuboid)obj);
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
				int hashCode = FrontBottomLeft.GetHashCode();
				hashCode = (hashCode * 397) ^ Width.GetHashCode();
				hashCode = (hashCode * 397) ^ Height.GetHashCode();
				hashCode = (hashCode * 397) ^ Depth.GetHashCode();
				return hashCode;
			}
		}

		/// <summary>
		/// Indicates whether or not the two shapes are equal.
		/// </summary>
		/// <param name="left">The first shape.</param>
		/// <param name="right">The second shape.</param>
		/// <returns>True if the shapes represent equal dimensions and positions, false if not.</returns>
		public static bool operator ==(Cuboid left, Cuboid right) {
			return left.Equals(right);
		}

		/// <summary>
		/// Indicates whether or not the two shapes are unequal.
		/// </summary>
		/// <param name="left">The first shape.</param>
		/// <param name="right">The second shape.</param>
		/// <returns>False if the shapes represent equal dimensions and positions, true if not.</returns>
		public static bool operator !=(Cuboid left, Cuboid right) {
			return !left.Equals(right);
		}
		#endregion

		private float DistanceFromSquared(Vector3 point) {
			if (Contains(point)) return 0f;

			float distanceX = point.X - (float) MathUtils.Clamp(point.X, FrontBottomLeft.X, FrontBottomLeft.X + Width);
			float distanceY = point.Y - (float) MathUtils.Clamp(point.Y, FrontBottomLeft.Y, FrontBottomLeft.Y + Height);
			float distanceZ = point.Z - (float) MathUtils.Clamp(point.Z, FrontBottomLeft.Z, FrontBottomLeft.Z + Depth);

			return distanceX * distanceX + distanceY * distanceY + distanceZ * distanceZ;
		}
	}
}