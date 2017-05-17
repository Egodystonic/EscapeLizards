// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 13 10 2014 at 11:37 by Ben Bowen

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Ophidian.Losgap {
	public unsafe partial struct Number {
		/// <summary>
		/// Whether or not the <see cref="ActualValue"/> that this number represents is integral (e.g. not rational).
		/// </summary>
		public bool IsIntegral {
			get {
				return IsNumericTypeIntegral(actualType);
			}
		}
		/// <summary>
		/// Whether or not the <see cref="ActualValue"/> includes a sign bit, and can represent negative values.
		/// </summary>
		public bool IsSigned {
			get {
				return IsNumericTypeSigned(actualType);
			}
		}

		/* 0xABC:
		 * A: 1 = rational, 0 = integral
		 * B: byte size
		 * C: 1 = signed, 0 = unsigned
		 */
		private enum NumericType {
			UByte = 0x010,
			SByte = 0x011,
			UShort = 0x020,
			SShort = 0x021,
			UInt = 0x040,
			SInt = 0x041,
			ULong = 0x080,
			SLong = 0x081,
			Float = 0x141,
			Double = 0x181
		}

		/// <summary>
		/// Ascertains whether or not the supplied <paramref name="value"/> is of a numeric type.
		/// </summary>
		/// <param name="value">The value to test. Null is permitted (but <c>false</c> will be returned).</param>
		/// <returns>The equivalent of calling <see cref="IsNumericType(System.Type)"/> with <c>value.GetType()</c>.</returns>
		public static bool IsNumeric(object value) {
			return value != null && IsNumericType(value.GetType());
		}

		/// <summary>
		/// Whether or not the supplied type represents a number.
		/// </summary>
		/// <param name="type">The type to check. Null is permitted (but <c>false</c> will be returned).</param>
		/// <returns>True if the given <paramref name="type"/> is a numeric type (including <see cref="decimal"/>).</returns>
		public static bool IsNumericType(Type type) {
			return
				type == typeof(Number) ||
				type == typeof(Double) ||
				type == typeof(Single) ||
				type == typeof(Int64) ||
				type == typeof(Int16) ||
				type == typeof(Byte) ||
				type == typeof(SByte) ||
				type == typeof(UInt32) ||
				type == typeof(UInt64) ||
				type == typeof(UInt16) ||
				type == typeof(Decimal) ||
				type == typeof(Int32);
		}

		/// <summary>
		/// Returns the maximum value of the numeric type represented by the given <paramref name="typeCode"/>.
		/// </summary>
		/// <param name="typeCode">The type to get the maximum value for.</param>
		/// <returns>MaxValue of the equivalent type.</returns>
		public static Number MaxValue(TypeCode typeCode) {
			switch (typeCode) {
				case TypeCode.SByte: return SByte.MaxValue;
				case TypeCode.Byte: return Byte.MaxValue;
				case TypeCode.Int16: return Int16.MaxValue;
				case TypeCode.UInt16: return UInt16.MaxValue;
				case TypeCode.Int32: return Int32.MaxValue;
				case TypeCode.UInt32: return UInt32.MaxValue;
				case TypeCode.Int64: return Int64.MaxValue;
				case TypeCode.UInt64: return UInt64.MaxValue;
				case TypeCode.Single: return Single.MaxValue;
				case TypeCode.Double: return Double.MaxValue;
				default: throw new ArgumentException("Supplied typecode '" + typeCode + "' can not have a " +
					"maximum value represented by an " + typeof(Number).FullName + "!", "typeCode");
			}
		}

		/// <summary>
		/// Returns the minimum value of the numeric type represented by the given <paramref name="typeCode"/>.
		/// </summary>
		/// <param name="typeCode">The type to get the minimum value for.</param>
		/// <returns>MinValue of the equivalent type.</returns>
		public static Number MinValue(TypeCode typeCode) {
			switch (typeCode) {
				case TypeCode.SByte: return SByte.MinValue;
				case TypeCode.Byte: return Byte.MinValue;
				case TypeCode.Int16: return Int16.MinValue;
				case TypeCode.UInt16: return UInt16.MinValue;
				case TypeCode.Int32: return Int32.MinValue;
				case TypeCode.UInt32: return UInt32.MinValue;
				case TypeCode.Int64: return Int64.MinValue;
				case TypeCode.UInt64: return UInt64.MinValue;
				case TypeCode.Single: return Single.MinValue;
				case TypeCode.Double: return Double.MinValue;
				default: throw new ArgumentException("Supplied typecode '" + typeCode + "' can not have a " +
					"maximum value represented by an " + typeof(Number).FullName + "!", "typeCode");
			}
		}

		/// <summary>
		/// Whether or not the supplied TypeCode represents a number.
		/// </summary>
		/// <param name="typeCode">The TypeCode to check.</param>
		/// <returns>True if the given <paramref name="typeCode"/> is a numeric type (including <see cref="decimal"/>).</returns>
		public static bool IsNumericType(TypeCode typeCode) {
			return IsNumericType(typeCode.ToType());
		}

		/// <summary>
		/// Returns the maximum value of the numeric type represented by the given <paramref name="numericType"/>.
		/// </summary>
		/// <param name="numericType">The type to get the maximum value for.
		/// Must be a valid numeric type (see <see cref="IsNumericType(System.Type)"/>).</param>
		/// <returns>MaxValue of the equivalent type.</returns>
		public static Number MaxValue(Type numericType) {
			if (!IsNumericType(numericType)) {
				throw new ArgumentException("Supplied type '" + (numericType == null ? "null" : numericType.Name) + "' is not numeric!");
			}
			return MaxValue(Type.GetTypeCode(numericType));
		}

		/// <summary>
		/// Returns the minimum value of the numeric type represented by the given <paramref name="numericType"/>.
		/// </summary>
		/// <param name="numericType">The type to get the minimum value for.
		/// Must be a valid numeric type (see <see cref="IsNumericType(System.Type)"/>).</param>
		/// <returns>MinValue of the equivalent type.</returns>
		public static Number MinValue(Type numericType) {
			if (!IsNumericType(numericType)) {
				throw new ArgumentException("Supplied type '" + (numericType == null ? "null" : numericType.Name) + "' is not numeric!");
			}
			return MinValue(Type.GetTypeCode(numericType));
		}

		private static bool IsNumericTypeIntegral(NumericType type) {
			return (((int) type) & 0x100) == 0;
		}

		private static bool IsNumericTypeSigned(NumericType type) {
			return (((int) type) & 0x1) == 1;
		}

		private static int GetNumericTypeSizeBytes(NumericType type) {
			return (((int) type) & 0xF0) >> 4;
		}

		private static NumericType BiggerType(NumericType a, NumericType b) {
			return a > b ? a : b;
		}
	}
}