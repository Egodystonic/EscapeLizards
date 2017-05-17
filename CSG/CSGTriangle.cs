using System;
using System.Linq;

namespace Ophidian.Losgap.CSG {
	internal struct CSGTriangle : IEquatable<CSGTriangle> {
		public readonly uint A, B, C;

		public CSGTriangle(uint a, uint b, uint c) {
			A = a;
			B = b;
			C = c;
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(CSGTriangle other) {
			return (A == other.A || A == other.B || A == other.C)
				&& (B == other.A || B == other.B || B == other.C)
				&& (C == other.A || C == other.B || C == other.C);
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
			return obj is CSGTriangle && Equals((CSGTriangle)obj);
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>
		/// A 32-bit signed integer that is the hash code for this instance.
		/// </returns>
		public override int GetHashCode() {
			unchecked {
				int hashCode = (int)A;
				hashCode = (hashCode * 397) ^ (int)B;
				hashCode = (hashCode * 397) ^ (int)C;
				return hashCode;
			}
		}

		public static bool operator ==(CSGTriangle left, CSGTriangle right) {
			return left.Equals(right);
		}

		public static bool operator !=(CSGTriangle left, CSGTriangle right) {
			return !left.Equals(right);
		}
	}
}