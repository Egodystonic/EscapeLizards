// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 13 03 2015 at 14:12 by Ben Bowen

using System;
using System.Collections.Generic;

namespace Ophidian.Losgap.CSG {
	public struct CSGVertex : IEquatable<CSGVertex> {
		public readonly Vector3 Position;
		public readonly Vector3 Normal;
		public readonly Vector2 TexUV;

		public CSGVertex(Vector3 position, Vector3 normal, Vector2 texUv) {
			Position = position;
			Normal = normal;
			TexUV = texUv;
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(CSGVertex other) {
			return Position.Equals(other.Position) && Normal.Equals(other.Normal) && TexUV.Equals(other.TexUV);
		}

		/// <summary>
		/// Indicates whether this instance and a specified object are equal.
		/// </summary>
		/// <returns>
		/// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false. 
		/// </returns>
		/// <param name="obj">The object to compare with the current instance. </param>
		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) {
				return false;
			}
			return obj is CSGVertex && Equals((CSGVertex)obj);
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>
		/// A 32-bit signed integer that is the hash code for this instance.
		/// </returns>
		public override int GetHashCode() {
			unchecked {
				int hashCode = Position.GetHashCode();
				hashCode = (hashCode * 397) ^ Normal.GetHashCode();
				hashCode = (hashCode * 397) ^ TexUV.GetHashCode();
				return hashCode;
			}
		}

		public static bool operator ==(CSGVertex left, CSGVertex right) {
			return left.Equals(right);
		}

		public static bool operator !=(CSGVertex left, CSGVertex right) {
			return !left.Equals(right);
		}
	}
}