// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 09 10 2014 at 13:30 by Ben Bowen

using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace Ophidian.Losgap.Interop {
	/// <summary>
	/// Used by some interop methods to access strings allocated in native code.
	/// Usage of this pointer type indicates a reference to a string created by the native side of the framework, for which managed
	/// code has no control over the lifetime.
	/// The pointed-to string is expected to be encoded in UTF-16 (2-byte).
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = (int) InteropUtils.StructPacking.Safe)]
	public struct InteropStringPtr : IEquatable<InteropStringPtr> {
		private readonly IntPtr ptrToUnicodeString;

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(InteropStringPtr other) {
			return ptrToUnicodeString == other.ptrToUnicodeString;
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
			return obj is InteropStringPtr && Equals((InteropStringPtr)obj);
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>
		/// A 32-bit signed integer that is the hash code for this instance.
		/// </returns>
		/// <filterpriority>2</filterpriority>
		public override int GetHashCode() {
			return ptrToUnicodeString.GetHashCode();
		}

		/// <summary>
		/// Copies the string pointed to by this InteropStringPtr on to the managed heap as a CLR string, and returns
		/// a reference to it. 
		/// </summary>
		/// <returns>
		/// A managed string copy of the pointed to native UTF-16 string.
		/// </returns>
		public override string ToString() {
			return this;
		}

		/// <summary>
		/// Indicates whether <paramref name="left"/> and <paramref name="right"/> refer to the same string
		/// (e.g. their pointer values are equal).
		/// </summary>
		/// <param name="left">The first operand.</param>
		/// <param name="right">The second operand.</param>
		/// <returns>True if both operands refer to the same location in memory.</returns>
		public static bool operator ==(InteropStringPtr left, InteropStringPtr right) {
			return left.Equals(right);
		}

		/// <summary>
		/// Indicates whether <paramref name="left"/> and <paramref name="right"/> refer to different strings
		/// (e.g. their pointer values are not equal).
		/// </summary>
		/// <param name="left">The first operand.</param>
		/// <param name="right">The second operand.</param>
		/// <returns>True if both operands refer to the different locations in memory.</returns>
		public static bool operator !=(InteropStringPtr left, InteropStringPtr right) {
			return !left.Equals(right);
		}

		/// <summary>
		/// Copies the string pointed to by this InteropStringPtr on to the managed heap as a CLR string, and returns
		/// a reference to it.
		/// </summary>
		/// <param name="operand">The InteropStringPtr.</param>
		/// <returns>A managed string copy of the pointed to native UTF-16 string.</returns>
		public static implicit operator string(InteropStringPtr operand) {
			return Marshal.PtrToStringUni(operand.ptrToUnicodeString);
		}
	}
}