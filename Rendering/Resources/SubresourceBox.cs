// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 28 10 2014 at 13:34 by Ben Bowen

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// A cuboid struct that represents a subregion of a <see cref="IResource">resource</see>.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = (int) InteropUtils.StructPacking.Safe, Size = 6 * sizeof(uint))]
	public struct SubresourceBox : IEquatable<SubresourceBox> {
		/// <summary>
		/// The left side of the region in the u-dimension.
		/// </summary>
		public readonly uint Left;
		/// <summary>
		/// The right side of the region in the u-dimension.
		/// </summary>
		public readonly uint Right;
		/// <summary>
		/// The top side of the region in the v-dimension.
		/// </summary>
		public readonly uint Top;
		/// <summary>
		/// The bottom side of the region in the v-dimension.
		/// </summary>
		public readonly uint Bottom;
		/// <summary>
		/// The front side of the region in the w-dimension.
		/// </summary>
		public readonly uint Front;
		/// <summary>
		/// The back side of the region in the w-dimension.
		/// </summary>
		public readonly uint Back;

		/// <summary>
		/// The width of this box; e.g. <c>Right - Left</c>.
		/// </summary>
		public uint Width {
			get {
				return Right - Left;
			}
		}
		/// <summary>
		/// The height of this box; e.g. <c>Bottom - Top</c>.
		/// </summary>
		public uint Height {
			get {
				return Bottom - Top;
			}
		}
		/// <summary>
		/// The depth of this box; e.g. <c>Back - Front</c>.
		/// </summary>
		public uint Depth {
			get {
				return Back - Front;
			}
		}

		/// <summary>
		/// The volume represented by this box, e.g. <c>Width * Height * Depth</c>.
		/// </summary>
		public uint Volume {
			get {
				return Width * Height * Depth;
			}
		}

		/// <summary>
		/// Constructs a new subresource box with a height and depth of 1, and the supplied left/right co-ordinates.
		/// </summary>
		/// <param name="left">The left side of the region in the u-dimension.</param>
		/// <param name="right">The right side of the region in the u-dimension.</param>
		public SubresourceBox(uint left, uint right)
			: this(left, right, 0U, 1U, 0U, 1U) { }

		/// <summary>
		/// Constructs a new subresource box with a depth of 1, and the supplied left/right and top/bottom co-ordinates.
		/// </summary>
		/// <param name="left">The left side of the region in the u-dimension.</param>
		/// <param name="right">The right side of the region in the u-dimension.</param>
		/// <param name="top">The top side of the region in the v-dimension.</param>
		/// <param name="bottom">The bottom side of the region in the v-dimension.</param>
		public SubresourceBox(uint left, uint right, uint top, uint bottom)
		: this(left, right, top, bottom, 0U, 1U) { }

		/// <summary>
		/// Constructs a new subresource box with the supplied left/right, top/bottom, and front/back co-ordinates.
		/// </summary>
		/// <param name="left">The left side of the region in the u-dimension.</param>
		/// <param name="right">The right side of the region in the u-dimension.</param>
		/// <param name="top">The top side of the region in the v-dimension.</param>
		/// <param name="bottom">The bottom side of the region in the v-dimension.</param>
		/// <param name="front">The front side of the region in the w-dimension.</param>
		/// <param name="back">The back side of the region in the w-dimension.</param>
		public SubresourceBox(uint left, uint right, uint top, uint bottom, uint front, uint back) {
			Left = left;
			Right = right;
			Top = top;
			Bottom = bottom;
			Front = front;
			Back = back;
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(SubresourceBox other) {
			return Left == other.Left && Right == other.Right && Top == other.Top && Bottom == other.Bottom && Front == other.Front && Back == other.Back;
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
			return obj is SubresourceBox && Equals((SubresourceBox)obj);
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
				int hashCode = (int)Left;
				hashCode = (hashCode * 397) ^ (int)Right;
				hashCode = (hashCode * 397) ^ (int)Top;
				hashCode = (hashCode * 397) ^ (int)Bottom;
				hashCode = (hashCode * 397) ^ (int)Front;
				hashCode = (hashCode * 397) ^ (int)Back;
				return hashCode;
			}
		}

		/// <summary>
		/// Indicates whether or not the two boxes represent identical regions of a subresource.
		/// </summary>
		/// <param name="left">The first operand.</param>
		/// <param name="right">The second operand.</param>
		/// <returns>True if all co-ordinates on both operands are identical.</returns>
		public static bool operator ==(SubresourceBox left, SubresourceBox right) {
			return left.Equals(right);
		}

		/// <summary>
		/// Indicates whether or not the two boxes represent identical regions of a subresource.
		/// </summary>
		/// <param name="left">The first operand.</param>
		/// <param name="right">The second operand.</param>
		/// <returns>False if all co-ordinates on both operands are identical.</returns>
		public static bool operator !=(SubresourceBox left, SubresourceBox right) {
			return !left.Equals(right);
		}
	}
}