// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 13 10 2014 at 11:37 by Ben Bowen

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Ophidian.Losgap {
	public unsafe partial struct Number : IEquatable<Number> {
		/// <summary>
		/// Indicates whether this Number represents an equal numeric value to <paramref name="other"/>,
		/// regardless of their <see cref="ActualValue"/>s.
		/// </summary>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(Number other) {
			// ReSharper disable CompareOfFloatsByEqualityOperator
			NumericType biggerType = BiggerType(actualType, other.actualType);
			if (IsIntegral && other.IsIntegral) {
				if (IsSigned && other.IsSigned) {
					return (long) this == (long) other;
				}
				else if (!IsSigned && !other.IsSigned) {
					return (ulong) this == (ulong) other;
				}
				else {
					if (!this.IsSigned && (ulong) this > Int64.MaxValue) return false;
					else if (!other.IsSigned && (ulong) other > Int64.MaxValue) return false;
					else return (long) this == (long) other;
				}
			}
			else if (biggerType == NumericType.Float) {
				return (float) this == (float) other;
			}
			else {
				return (double) this == (double) other;
			}
			// ReSharper restore CompareOfFloatsByEqualityOperator
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
			return obj is Number && Equals((Number)obj);
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>
		/// A 32-bit signed integer that is the hash code for this instance.
		/// </returns>
		/// <filterpriority>2</filterpriority>
		public override int GetHashCode() {
			return (int) (asSignedLong ^ (asSignedLong >> 32));
		}

		/// <summary>
		/// Ascertains whether the two operands represent equal numeric values, regardless of their <see cref="ActualValue"/>s.
		/// </summary>
		/// <param name="left">The first operand.</param>
		/// <param name="right">The second operand.</param>
		/// <returns>True if left and right are numerically equivalent, ignoring the types of their <see cref="ActualValue"/>s.</returns>
		public static bool operator ==(Number left, Number right) {
			return left.Equals(right);
		}

		/// <summary>
		/// Ascertains whether the two operands do not represent equal numeric values, regardless of their <see cref="ActualValue"/>s.
		/// </summary>
		/// <param name="left">The first operand.</param>
		/// <param name="right">The second operand.</param>
		/// <returns>True if left and right are not numerically equivalent, ignoring the types of their <see cref="ActualValue"/>s.</returns>
		public static bool operator !=(Number left, Number right) {
			return !left.Equals(right);
		}

		/// <summary>
		/// Returns negated value of <paramref name="lhs"/>.
		/// </summary>
		/// <param name="lhs">The value to negate.</param>
		/// <returns>The negated value of the operand.</returns>
		public static Number operator -(Number lhs) {
			if (lhs.IsIntegral) return -((long) lhs);
			else return -((double) lhs);
		}

		/// <summary>
		/// Expands the type of <paramref name="lhs"/>'s <see cref="ActualValue"/> to be at least 4 bytes wide if 
		/// <paramref name="lhs"/> <see cref="IsIntegral">is an integral value</see>.
		/// </summary>
		/// <remarks>
		/// This operator has no effect is <paramref name="lhs"/>'s <see cref="IsIntegral"/> property returns <c>false</c>.
		/// </remarks>
		/// <param name="lhs">The value to expand.</param>
		/// <returns>A new number whose <see cref="ActualValue"/> is at least 4 bytes wide.</returns>
		public static Number operator +(Number lhs) {
			if (lhs.IsIntegral) {
				if (lhs.IsSigned) return ((long) lhs);
				else return ((ulong) lhs);
			}
			else return lhs;
		}

		/// <summary>
		/// Returns a new Number that is equal to <paramref name="lhs"/>, incremented once.
		/// </summary>
		/// <param name="lhs">The value to increment.</param>
		/// <returns>++<paramref name="lhs"/>.</returns>
		public static Number operator ++(Number lhs) {
			if (lhs.IsIntegral) {
				if (lhs.IsSigned) {
					switch (GetNumericTypeSizeBytes(lhs.actualType)) {
						case 1: return (sbyte) (((sbyte) lhs) + 1);
						case 2: return (short) (((short) lhs) + 1);
						case 4: return ((int) lhs) + 1;
						default: return ((long) lhs) + 1L;
					}
				}
				else {
					switch (GetNumericTypeSizeBytes(lhs.actualType)) {
						case 1: return (byte) (((byte) lhs) + 1U);
						case 2: return (ushort) (((ushort) lhs) + 1U);
						case 4: return ((uint) lhs) + 1U;
						default: return ((ulong) lhs) + 1UL;
					}
				}
			}
			else {
				switch (GetNumericTypeSizeBytes(lhs.actualType)) {
					case 4: return ((float) lhs) + 1f;
					default: return ((double) lhs) + 1d;
				}
			}
		}

		/// <summary>
		/// Returns a new Number that is equal to <paramref name="lhs"/>, decremented once.
		/// </summary>
		/// <param name="lhs">The value to decrement.</param>
		/// <returns>--<paramref name="lhs"/>.</returns>
		public static Number operator --(Number lhs) {
			if (lhs.IsIntegral) {
				if (lhs.IsSigned) {
					switch (GetNumericTypeSizeBytes(lhs.actualType)) {
						case 1: return (sbyte) (((sbyte) lhs) - 1);
						case 2: return (short) (((short) lhs) - 1);
						case 4: return ((int) lhs) - 1;
						default: return ((long) lhs) - 1L;
					}
				}
				else {
					switch (GetNumericTypeSizeBytes(lhs.actualType)) {
						case 1: return (byte) (((byte) lhs) - 1U);
						case 2: return (ushort) (((ushort) lhs) - 1U);
						case 4: return ((uint) lhs) - 1U;
						default: return ((ulong) lhs) - 1UL;
					}
				}
			}
			else {
				switch (GetNumericTypeSizeBytes(lhs.actualType)) {
					case 4: return ((float) lhs) - 1f;
					default: return ((double) lhs) - 1d;
				}
			}
		}

		/// <summary>
		/// Returns a new Number that represents the sum of <paramref name="lhs"/> and <paramref name="rhs"/>.
		/// </summary>
		/// <param name="lhs">The first operand.</param>
		/// <param name="rhs">The second operand.</param>
		/// <returns><c>new Number(lhs.ActualValue + rhs.ActualValue)</c>.</returns>
		public static Number operator +(Number lhs, Number rhs) {
			NumericType biggerType = BiggerType(lhs.actualType, rhs.actualType);
			if (IsNumericTypeIntegral(biggerType)) {
				if (lhs.IsSigned || rhs.IsSigned) {
					switch (GetNumericTypeSizeBytes(biggerType) * (lhs.IsSigned != rhs.IsSigned ? 2 : 1)) {
						case 1: return (sbyte) (((sbyte) lhs) + ((sbyte) rhs));
						case 2: return (short) (((short) lhs) + ((short) rhs));
						case 4: return ((int) lhs) + ((int) rhs);
						default: return ((long) lhs) + ((long) rhs);
					}
				}
				else {
					switch (GetNumericTypeSizeBytes(biggerType)) {
						case 1: return (byte) (((byte) lhs) + ((byte) rhs));
						case 2: return (ushort) (((ushort) lhs) + ((ushort) rhs));
						case 4: return ((uint) lhs) + ((uint) rhs);
						default: return ((ulong) lhs) + ((ulong) rhs);
					}
				}
			}
			else {
				switch (GetNumericTypeSizeBytes(biggerType)) {
					case 4: return ((float) lhs) + ((float) rhs);
					default: return ((double) lhs) + ((double) rhs);
				}
			}
		}

		/// <summary>
		/// Returns a new Number that represents the value of <paramref name="lhs"/> less <paramref name="rhs"/>.
		/// </summary>
		/// <param name="lhs">The first operand.</param>
		/// <param name="rhs">The second operand.</param>
		/// <returns><c>new Number(lhs.ActualValue - rhs.ActualValue)</c>.</returns>
		public static Number operator -(Number lhs, Number rhs) {
			NumericType biggerType = BiggerType(lhs.actualType, rhs.actualType);
			if (IsNumericTypeIntegral(biggerType)) {
				if (lhs.IsSigned || rhs.IsSigned) {
					switch (GetNumericTypeSizeBytes(biggerType) * (lhs.IsSigned != rhs.IsSigned ? 2 : 1)) {
						case 1: return (sbyte) (((sbyte) lhs) - ((sbyte) rhs));
						case 2: return (short) (((short) lhs) - ((short) rhs));
						case 4: return ((int) lhs) - ((int) rhs);
						default: return ((long) lhs) - ((long) rhs);
					}
				}
				else {
					switch (GetNumericTypeSizeBytes(biggerType)) {
						case 1: return (byte) (((byte) lhs) - ((byte) rhs));
						case 2: return (ushort) (((ushort) lhs) - ((ushort) rhs));
						case 4: return ((uint) lhs) - ((uint) rhs);
						default: return ((ulong) lhs) - ((ulong) rhs);
					}
				}
			}
			else {
				switch (GetNumericTypeSizeBytes(biggerType)) {
					case 4: return ((float) lhs) - ((float) rhs);
					default: return ((double) lhs) - ((double) rhs);
				}
			}
		}

		/// <summary>
		/// Returns a new Number that represents the value of <paramref name="lhs"/> divided by <paramref name="rhs"/>.
		/// </summary>
		/// <param name="lhs">The first operand.</param>
		/// <param name="rhs">The second operand.</param>
		/// <returns><c>new Number(lhs.ActualValue / rhs.ActualValue)</c>.</returns>
		public static Number operator /(Number lhs, Number rhs) {
			NumericType biggerType = BiggerType(lhs.actualType, rhs.actualType);
			if (IsNumericTypeIntegral(biggerType)) {
				if (lhs.IsSigned || rhs.IsSigned) {
					switch (GetNumericTypeSizeBytes(biggerType) * (lhs.IsSigned != rhs.IsSigned ? 2 : 1)) {
						case 1: return (sbyte) (((sbyte) lhs) / ((sbyte) rhs));
						case 2: return (short) (((short) lhs) / ((short) rhs));
						case 4: return ((int) lhs) / ((int) rhs);
						default: return ((long) lhs) / ((long) rhs);
					}
				}
				else {
					switch (GetNumericTypeSizeBytes(biggerType)) {
						case 1: return (byte) (((byte) lhs) / ((byte) rhs));
						case 2: return (ushort) (((ushort) lhs) / ((ushort) rhs));
						case 4: return ((uint) lhs) / ((uint) rhs);
						default: return ((ulong) lhs) / ((ulong) rhs);
					}
				}
			}
			else {
				switch (GetNumericTypeSizeBytes(biggerType)) {
					case 4: return ((float) lhs) / ((float) rhs);
					default: return ((double) lhs) / ((double) rhs);
				}
			}
		}

		/// <summary>
		/// Returns a new Number that represents the sum of <paramref name="lhs"/> multiplied by <paramref name="rhs"/>.
		/// </summary>
		/// <param name="lhs">The first operand.</param>
		/// <param name="rhs">The second operand.</param>
		/// <returns><c>new Number(lhs.ActualValue * rhs.ActualValue)</c>.</returns>
		public static Number operator *(Number lhs, Number rhs) {
			NumericType biggerType = BiggerType(lhs.actualType, rhs.actualType);
			if (IsNumericTypeIntegral(biggerType)) {
				if (lhs.IsSigned || rhs.IsSigned) {
					switch (GetNumericTypeSizeBytes(biggerType) * (lhs.IsSigned != rhs.IsSigned ? 2 : 1)) {
						case 1: return (sbyte) (((sbyte) lhs) * ((sbyte) rhs));
						case 2: return (short) (((short) lhs) * ((short) rhs));
						case 4: return ((int) lhs) * ((int) rhs);
						default: return ((long) lhs) * ((long) rhs);
					}
				}
				else {
					switch (GetNumericTypeSizeBytes(biggerType)) {
						case 1: return (byte) (((byte) lhs) * ((byte) rhs));
						case 2: return (ushort) (((ushort) lhs) * ((ushort) rhs));
						case 4: return ((uint) lhs) * ((uint) rhs);
						default: return ((ulong) lhs) * ((ulong) rhs);
					}
				}
			}
			else {
				switch (GetNumericTypeSizeBytes(biggerType)) {
					case 4: return ((float) lhs) * ((float) rhs);
					default: return ((double) lhs) * ((double) rhs);
				}
			}
		}

		/// <summary>
		/// Returns a new Number that represents the remainder of a division where <paramref name="lhs"/> is the numerator and 
		/// <paramref name="rhs"/> is the denominator.
		/// </summary>
		/// <param name="lhs">The first operand.</param>
		/// <param name="rhs">The second operand.</param>
		/// <returns><c>new Number(lhs.ActualValue % rhs.ActualValue)</c>.</returns>
		public static Number operator %(Number lhs, Number rhs) {
			NumericType biggerType = BiggerType(lhs.actualType, rhs.actualType);
			if (IsNumericTypeIntegral(biggerType)) {
				if (lhs.IsSigned || rhs.IsSigned) {
					switch (GetNumericTypeSizeBytes(biggerType) * (lhs.IsSigned != rhs.IsSigned ? 2 : 1)) {
						case 1: return (sbyte) (((sbyte) lhs) % ((sbyte) rhs));
						case 2: return (short) (((short) lhs) % ((short) rhs));
						case 4: return ((int) lhs) % ((int) rhs);
						default: return ((long) lhs) % ((long) rhs);
					}
				}
				else {
					switch (GetNumericTypeSizeBytes(biggerType)) {
						case 1: return (byte) (((byte) lhs) % ((byte) rhs));
						case 2: return (ushort) (((ushort) lhs) % ((ushort) rhs));
						case 4: return ((uint) lhs) % ((uint) rhs);
						default: return ((ulong) lhs) % ((ulong) rhs);
					}
				}
			}
			else {
				switch (GetNumericTypeSizeBytes(biggerType)) {
					case 4: return ((float) lhs) % ((float) rhs);
					default: return ((double) lhs) % ((double) rhs);
				}
			}
		}

		/// <summary>
		/// Returns a new Number that represents the result of a bitwise-AND between <paramref name="lhs"/> and <paramref name="rhs"/>.
		/// </summary>
		/// <remarks>
		/// If <paramref name="lhs"/> or <paramref name="rhs"/> currently represent non-integral values, they will be cast to the nearest
		/// integral value before this operation proceeds.
		/// </remarks>
		/// <param name="lhs">The first operand.</param>
		/// <param name="rhs">The second operand.</param>
		/// <returns><c>new Number(lhs.ActualValue &amp; rhs.ActualValue)</c>.</returns>
		public static Number operator &(Number lhs, Number rhs) {
			NumericType biggerType;
			if (lhs.IsIntegral && !rhs.IsIntegral) biggerType = lhs.actualType;
			else if (!lhs.IsIntegral && rhs.IsIntegral) biggerType = rhs.actualType;
			else biggerType = BiggerType(lhs.actualType, rhs.actualType);

			if (lhs.IsSigned || rhs.IsSigned) {
				switch (GetNumericTypeSizeBytes(biggerType) * (lhs.IsSigned != rhs.IsSigned ? 2 : 1)) {
					case 1: return (sbyte) (((sbyte) lhs) & ((sbyte) rhs));
					case 2: return (short) (((short) lhs) & ((short) rhs));
					case 4: return ((int) lhs) & ((int) rhs);
					default: return ((long) lhs) & ((long) rhs);
				}
			}
			else {
				switch (GetNumericTypeSizeBytes(biggerType)) {
					case 1: return (byte) (((byte) lhs) & ((byte) rhs));
					case 2: return (ushort) (((ushort) lhs) & ((ushort) rhs));
					case 4: return ((uint) lhs) & ((uint) rhs);
					default: return ((ulong) lhs) & ((ulong) rhs);
				}
			}
		}

		/// <summary>
		/// Returns a new Number that represents the result of a bitwise-OR between <paramref name="lhs"/> and <paramref name="rhs"/>.
		/// </summary>
		/// <remarks>
		/// If <paramref name="lhs"/> or <paramref name="rhs"/> currently represent non-integral values, they will be cast to the nearest
		/// integral value before this operation proceeds.
		/// </remarks>
		/// <param name="lhs">The first operand.</param>
		/// <param name="rhs">The second operand.</param>
		/// <returns><c>new Number(lhs.ActualValue | rhs.ActualValue)</c>.</returns>
		public static Number operator |(Number lhs, Number rhs) {
			NumericType biggerType;
			if (lhs.IsIntegral && !rhs.IsIntegral) biggerType = lhs.actualType;
			else if (!lhs.IsIntegral && rhs.IsIntegral) biggerType = rhs.actualType;
			else biggerType = BiggerType(lhs.actualType, rhs.actualType);

			if (lhs.IsSigned || rhs.IsSigned) {
				switch (GetNumericTypeSizeBytes(biggerType) * (lhs.IsSigned != rhs.IsSigned ? 2 : 1)) {
					case 1: return (sbyte) (((sbyte) lhs) | ((sbyte) rhs));
					case 2: return (short) (((short) lhs) | ((short) rhs));
					case 4: return ((int) lhs) | ((int) rhs);
					default: return ((long) lhs) | ((long) rhs);
				}
			}
			else {
				switch (GetNumericTypeSizeBytes(biggerType)) {
					case 1: return (byte) (((byte) lhs) | ((byte) rhs));
					case 2: return (ushort) (((ushort) lhs) | ((ushort) rhs));
					case 4: return ((uint) lhs) | ((uint) rhs);
					default: return ((ulong) lhs) | ((ulong) rhs);
				}
			}
		}

		/// <summary>
		/// Returns a new Number that represents the result of a bitwise-XOR between <paramref name="lhs"/> and <paramref name="rhs"/>.
		/// </summary>
		/// <remarks>
		/// If <paramref name="lhs"/> or <paramref name="rhs"/> currently represent non-integral values, they will be cast to the nearest
		/// integral value before this operation proceeds.
		/// </remarks>
		/// <param name="lhs">The first operand.</param>
		/// <param name="rhs">The second operand.</param>
		/// <returns><c>new Number(lhs.ActualValue ^ rhs.ActualValue)</c>.</returns>
		public static Number operator ^(Number lhs, Number rhs) {
			NumericType biggerType;
			if (lhs.IsIntegral && !rhs.IsIntegral) biggerType = lhs.actualType;
			else if (!lhs.IsIntegral && rhs.IsIntegral) biggerType = rhs.actualType;
			else biggerType = BiggerType(lhs.actualType, rhs.actualType);

			if (lhs.IsSigned || rhs.IsSigned) {
				switch (GetNumericTypeSizeBytes(biggerType) * (lhs.IsSigned != rhs.IsSigned ? 2 : 1)) {
					case 1: return (sbyte) (((sbyte) lhs) ^ ((sbyte) rhs));
					case 2: return (short) (((short) lhs) ^ ((short) rhs));
					case 4: return ((int) lhs) ^ ((int) rhs);
					default: return ((long) lhs) ^ ((long) rhs);
				}
			}
			else {
				switch (GetNumericTypeSizeBytes(biggerType)) {
					case 1: return (byte) (((byte) lhs) ^ ((byte) rhs));
					case 2: return (ushort) (((ushort) lhs) ^ ((ushort) rhs));
					case 4: return ((uint) lhs) ^ ((uint) rhs);
					default: return ((ulong) lhs) ^ ((ulong) rhs);
				}
			}
		}

		/// <summary>
		/// Returns a new Number that represents the result of <paramref name="lhs"/> bit-shifted left <paramref name="rhs"/> times.
		/// </summary>
		/// <remarks>
		/// If <paramref name="lhs"/> currently represent a non-integral value, it will be cast to the nearest
		/// integral value before this operation proceeds.
		/// </remarks>
		/// <param name="lhs">The first operand.</param>
		/// <param name="rhs">The second operand.</param>
		/// <returns><c>new Number(lhs.ActualValue &lt;&lt; rhs)</c>.</returns>
		public static Number operator <<(Number lhs, int rhs) {
			if (lhs.IsSigned) {
				switch (GetNumericTypeSizeBytes(lhs.actualType)) {
					case 1: return (sbyte) (((sbyte) lhs) << rhs);
					case 2: return (short) (((short) lhs) << rhs);
					case 4: return ((int) lhs << rhs);
					default: return ((long) lhs << rhs);
				}
			}
			else {
				switch (GetNumericTypeSizeBytes(lhs.actualType)) {
					case 1: return (byte) (((byte) lhs) << rhs);
					case 2: return (ushort) (((ushort) lhs) << rhs);
					case 4: return ((uint) lhs << rhs);
					default: return ((ulong) lhs << rhs);
				}
			}
		}

		/// <summary>
		/// Returns a new Number that represents the result of <paramref name="lhs"/> bit-shifted right <paramref name="rhs"/> times.
		/// </summary>
		/// <remarks>
		/// If <paramref name="lhs"/> currently represent a non-integral value, it will be cast to the nearest
		/// integral value before this operation proceeds.
		/// </remarks>
		/// <param name="lhs">The first operand.</param>
		/// <param name="rhs">The second operand.</param>
		/// <returns><c>new Number(lhs.ActualValue &gt;&gt; rhs)</c>.</returns>
		public static Number operator >>(Number lhs, int rhs) {
			if (lhs.IsSigned) {
				switch (GetNumericTypeSizeBytes(lhs.actualType)) {
					case 1: return (sbyte) (((sbyte) lhs) >> rhs);
					case 2: return (short) (((short) lhs) >> rhs);
					case 4: return ((int) lhs >> rhs);
					default: return ((long) lhs >> rhs);
				}
			}
			else {
				switch (GetNumericTypeSizeBytes(lhs.actualType)) {
					case 1: return (byte) (((byte) lhs) >> rhs);
					case 2: return (ushort) (((ushort) lhs) >> rhs);
					case 4: return ((uint) lhs >> rhs);
					default: return ((ulong) lhs >> rhs);
				}
			}
		}

		/// <summary>
		/// Indicates whether <paramref name="lhs"/>'s numerical value is greater than <paramref name="rhs"/>'s numerical value.
		/// </summary>
		/// <param name="lhs">The first operand.</param>
		/// <param name="rhs">The second operand.</param>
		/// <returns><c>lhs.ActualValue &gt; rhs.ActualValue</c>.</returns>
		public static bool operator >(Number lhs, Number rhs) {
			if (lhs.IsIntegral && rhs.IsIntegral) {
				if (lhs.IsSigned && rhs.IsSigned) {
					return (long) lhs > (long) rhs;
				}
				else if (!lhs.IsSigned && !rhs.IsSigned) {
					return (ulong)lhs > (ulong)rhs;
				}
				else {
					if (!lhs.IsSigned && (ulong) lhs > Int64.MaxValue) return true;
					else if (!rhs.IsSigned && (ulong) rhs > Int64.MaxValue) return false;
					else return (long) lhs > (long) rhs;
				}
			}
			else {
				return ((double) lhs) > ((double) rhs);
			}
		}

		/// <summary>
		/// Indicates whether <paramref name="lhs"/>'s numerical value is less than <paramref name="rhs"/>'s numerical value.
		/// </summary>
		/// <param name="lhs">The first operand.</param>
		/// <param name="rhs">The second operand.</param>
		/// <returns><c>lhs.ActualValue &lt; rhs.ActualValue</c>.</returns>
		public static bool operator <(Number lhs, Number rhs) {
			if (lhs.IsIntegral && rhs.IsIntegral) {
				if (lhs.IsSigned && rhs.IsSigned) {
					return (long) lhs < (long) rhs;
				}
				else if (!lhs.IsSigned && !rhs.IsSigned) {
					return (ulong) lhs < (ulong) rhs;
				}
				else {
					if (!lhs.IsSigned && (ulong) lhs > Int64.MaxValue) return false;
					else if (!rhs.IsSigned && (ulong) rhs > Int64.MaxValue) return true;
					else return (long) lhs < (long) rhs;
				}
			}
			else {
				return ((double) lhs) < ((double) rhs);
			}
		}

		/// <summary>
		/// Indicates whether <paramref name="lhs"/>'s numerical value is greater than or equal to <paramref name="rhs"/>'s numerical value.
		/// </summary>
		/// <param name="lhs">The first operand.</param>
		/// <param name="rhs">The second operand.</param>
		/// <returns><c>lhs.ActualValue &gt;= rhs.ActualValue</c>.</returns>
		public static bool operator >=(Number lhs, Number rhs) {
			if (lhs.IsIntegral && rhs.IsIntegral) {
				if (lhs.IsSigned && rhs.IsSigned) {
					return (long) lhs >= (long) rhs;
				}
				else if (!lhs.IsSigned && !rhs.IsSigned) {
					return (ulong) lhs >= (ulong) rhs;
				}
				else {
					if (!lhs.IsSigned && (ulong) lhs > Int64.MaxValue) return true;
					else if (!rhs.IsSigned && (ulong) rhs > Int64.MaxValue) return false;
					else return (long) lhs >= (long) rhs;
				}
			}
			else {
				return ((double) lhs) >= ((double) rhs);
			}
		}

		/// <summary>
		/// Indicates whether <paramref name="lhs"/>'s numerical value is less than or equal to <paramref name="rhs"/>'s numerical value.
		/// </summary>
		/// <param name="lhs">The first operand.</param>
		/// <param name="rhs">The second operand.</param>
		/// <returns><c>lhs.ActualValue &lt;= rhs.ActualValue</c>.</returns>
		public static bool operator <=(Number lhs, Number rhs) {
			if (lhs.IsIntegral && rhs.IsIntegral) {
				if (lhs.IsSigned && rhs.IsSigned) {
					return (long) lhs <= (long) rhs;
				}
				else if (!lhs.IsSigned && !rhs.IsSigned) {
					return (ulong) lhs <= (ulong) rhs;
				}
				else {
					if (!lhs.IsSigned && (ulong) lhs > Int64.MaxValue) return false;
					else if (!rhs.IsSigned && (ulong) rhs > Int64.MaxValue) return true;
					else return (long) lhs <= (long) rhs;
				}
			}
			else {
				return ((double) lhs) <= ((double) rhs);
			}
		}
	}
}