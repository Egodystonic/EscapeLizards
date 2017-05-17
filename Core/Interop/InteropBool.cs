// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 09 10 2014 at 13:30 by Ben Bowen

using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace Ophidian.Losgap.Interop {
	/// <summary>
	/// Represents a boolean value that must be used in place of the CLR <see cref="bool"/> when communicating with native code.
	/// </summary>
	/// <remarks>
	/// This type contains implicit casts to / from <see cref="bool"/> values.
	/// </remarks>
	[StructLayout(LayoutKind.Sequential, Pack = (int) InteropUtils.StructPacking.Safe, Size = sizeof(byte))]
	public struct InteropBool : IEquatable<InteropBool> {
		/// <summary>
		/// Represents a <c>true</c> value for an <see cref="InteropBool"/>.
		/// </summary>
		public static readonly InteropBool TRUE = new InteropBool(Byte.MaxValue);
		/// <summary>
		/// Represents a <c>false</c> value for an <see cref="InteropBool"/>.
		/// </summary>
		public static readonly InteropBool FALSE = new InteropBool(Byte.MinValue);

		private readonly byte value;

		private InteropBool(byte value) {
			this.value = value;
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(InteropBool other) {
			return value == other.value;
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
			return obj is InteropBool && Equals((InteropBool)obj);
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>
		/// A 32-bit signed integer that is the hash code for this instance.
		/// </returns>
		/// <filterpriority>2</filterpriority>
		public override int GetHashCode() {
			return this ? TRUE.value : FALSE.value;
		}

		/// <summary>
		/// Returns the string representation of the boolean value this InteropBool represents.
		/// </summary>
		/// <returns>
		/// "true" or "false".
		/// </returns>
		public override string ToString() {
			return this ? "true" : "false";
		}

		/// <summary>
		/// Compares the two given operands, and returns true if they represent the same boolean value.
		/// </summary>
		/// <param name="lhs">The first operand.</param>
		/// <param name="rhs">The second operand.</param>
		/// <returns>True if <paramref name="lhs"/> and <paramref name="rhs"/> both represent true or false.</returns>
		public static bool operator ==(InteropBool lhs, InteropBool rhs) {
			return lhs.Equals(rhs);
		}

		/// <summary>
		/// Compares the two given operands, and returns true if they represent different boolean values.
		/// </summary>
		/// <param name="lhs">The first operand.</param>
		/// <param name="rhs">The second operand.</param>
		/// <returns>True if <paramref name="lhs"/> represents true and <paramref name="rhs"/> represents false, or vice-versa.</returns>
		public static bool operator !=(InteropBool lhs, InteropBool rhs) {
			return !lhs.Equals(rhs);
		}

		/// <summary>
		/// Returns a new <see cref="InteropBool"/> that represents the <paramref name="operand"/>'s truth value.
		/// </summary>
		/// <param name="operand">The value to be converted.</param>
		/// <returns>A new <see cref="InteropBool"/> that represents the <paramref name="operand"/>'s truth value.</returns>
		public static implicit operator InteropBool(bool operand) {
			return operand ? TRUE : FALSE;
		}

		/// <summary>
		/// Returns the truth value that <paramref name="operand"/> represents.
		/// </summary>
		/// <param name="operand">The value to be converted.</param>
		/// <returns>Returns the truth value that <paramref name="operand"/> represents.</returns>
		public static implicit operator bool(InteropBool operand) {
			return operand != FALSE;
		}
	}
}