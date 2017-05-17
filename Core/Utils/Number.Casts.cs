// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 13 10 2014 at 11:37 by Ben Bowen

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Ophidian.Losgap {
	public unsafe partial struct Number {
		private const string RATIONAL_TO_STRING_FORMAT = "{0:n4}";

		/// <summary>
		/// Constrains (e.g. clamps) the given number to the range of a <see cref="SByte"/>.
		/// </summary>
		/// <param name="input">The input value.</param>
		/// <returns>The clamped input value.</returns>
		public static SByte ConstrainToSByte(Number input) {
			if (input.IsIntegral) return (SByte) MathUtils.Clamp((Int64) input, SByte.MinValue, SByte.MaxValue);
			else return (SByte) MathUtils.Clamp((Double) input, SByte.MinValue, SByte.MaxValue);
		}

		/// <summary>
		/// Constrains (e.g. clamps) the given number to the range of a <see cref="Byte"/>.
		/// </summary>
		/// <param name="input">The input value.</param>
		/// <returns>The clamped input value.</returns>
		public static Byte ConstrainToByte(Number input) {
			if (input.IsIntegral) {
				if (input.IsSigned) return (Byte) MathUtils.Clamp((Int64) input, Byte.MinValue, Byte.MaxValue);
				else return (Byte) MathUtils.Clamp((UInt64) input, Byte.MinValue, Byte.MaxValue);
			}
			else return (Byte) MathUtils.Clamp((Double) input, Byte.MinValue, Byte.MaxValue);
		}

		/// <summary>
		/// Constrains (e.g. clamps) the given number to the range of a <see cref="Int16"/>.
		/// </summary>
		/// <param name="input">The input value.</param>
		/// <returns>The clamped input value.</returns>
		public static Int16 ConstrainToInt16(Number input) {
			if (input.IsIntegral) return (Int16) MathUtils.Clamp((Int64) input, Int16.MinValue, Int16.MaxValue);
			else return (Int16) MathUtils.Clamp((Double) input, Int16.MinValue, Int16.MaxValue);
		}

		/// <summary>
		/// Constrains (e.g. clamps) the given number to the range of a <see cref="UInt16"/>.
		/// </summary>
		/// <param name="input">The input value.</param>
		/// <returns>The clamped input value.</returns>
		public static UInt16 ConstrainToUInt16(Number input) {
			if (input.IsIntegral) {
				if (input.IsSigned) return (UInt16) MathUtils.Clamp((Int64) input, UInt16.MinValue, UInt16.MaxValue);
				else return (UInt16) MathUtils.Clamp((UInt64) input, UInt16.MinValue, UInt16.MaxValue);
			}
			else return (UInt16) MathUtils.Clamp((Double) input, UInt16.MinValue, UInt16.MaxValue);
		}

		/// <summary>
		/// Constrains (e.g. clamps) the given number to the range of a <see cref="Int32"/>.
		/// </summary>
		/// <param name="input">The input value.</param>
		/// <returns>The clamped input value.</returns>
		public static Int32 ConstrainToInt32(Number input) {
			if (input.IsIntegral) return (Int32) MathUtils.Clamp((Int64) input, Int32.MinValue, Int32.MaxValue);
			else return (Int32) MathUtils.Clamp((Double) input, Int32.MinValue, Int32.MaxValue);
		}

		/// <summary>
		/// Constrains (e.g. clamps) the given number to the range of a <see cref="UInt32"/>.
		/// </summary>
		/// <param name="input">The input value.</param>
		/// <returns>The clamped input value.</returns>
		public static UInt32 ConstrainToUInt32(Number input) {
			if (input.IsIntegral) {
				if (input.IsSigned) return (UInt32) MathUtils.Clamp((Int64) input, UInt32.MinValue, UInt32.MaxValue);
				else return (UInt32) MathUtils.Clamp((UInt64) input, UInt32.MinValue, UInt32.MaxValue);
			}
			else return (UInt32) MathUtils.Clamp((Double) input, UInt32.MinValue, UInt32.MaxValue);
		}

		/// <summary>
		/// Constrains (e.g. clamps) the given number to the range of a <see cref="Int64"/>.
		/// </summary>
		/// <param name="input">The input value.</param>
		/// <returns>The clamped input value.</returns>
		public static Int64 ConstrainToInt64(Number input) {
			if (input.IsIntegral) {
				if (input.IsSigned) return (Int64) input;
				else return (Int64) MathUtils.Clamp((UInt64) input, 0UL, Int64.MaxValue);
			}
			else return (Int64) MathUtils.Clamp((Double) input, Int64.MinValue, Int64.MaxValue);
		}

		/// <summary>
		/// Constrains (e.g. clamps) the given number to the range of a <see cref="UInt64"/>.
		/// </summary>
		/// <param name="input">The input value.</param>
		/// <returns>The clamped input value.</returns>
		public static UInt64 ConstrainToUInt64(Number input) {
			if (input.IsIntegral) {
				if (input.IsSigned) return (UInt64) MathUtils.Clamp((Int64) input, 0L, Int64.MaxValue);
				else return (UInt64) input;
			}
			else return (UInt64) MathUtils.Clamp((Double) input, Int64.MinValue, Int64.MaxValue);
		}

		/// <summary>
		/// Constrains (e.g. clamps) the given number to the range of a <see cref="Single"/>.
		/// </summary>
		/// <param name="input">The input value.</param>
		/// <returns>The clamped input value.</returns>
		public static Single ConstrainToSingle(Number input) {
			return (Single) MathUtils.Clamp((Double) input, Single.MinValue, Single.MaxValue);
		}

		/// <summary>
		/// Constrains (e.g. clamps) the given number to the range of a <see cref="Double"/>.
		/// </summary>
		/// <param name="input">The input value.</param>
		/// <returns>The clamped input value.</returns>
		public static Double ConstrainToDouble(Number input) {
			return (Double) input;
		}

		/// <summary>
		/// Serialized the given number in to a platform-and-environment-specific byte array.
		/// </summary>
		/// <param name="value">The input value.</param>
		/// <returns>The equivalent of using the relevant <see cref="BitConverter"/>.GetBytes() method on <paramref name="value"/>.</returns>
		public static byte[] Serialize(Number value) {
			byte[] result = new byte[GetNumericTypeSizeBytes(value.actualType)];
			byte* curByte = ((byte*) &value) + VALUE_OFFSET_BYTES;
			for (int i = 0; i < result.Length; ++i) {
				result[i] = *(curByte + i);
			}
			return result;
		}

		/// <summary>
		/// Parses the given string in to a Number.
		/// </summary>
		/// <param name="value">The string to parse. The string may be in any valid numeric format.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
		/// <exception cref="FormatException">Thrown if the string could not be parsed in to a Number.</exception>
		/// <returns>A Number that numerically represents the given string.</returns>
		public static Number Parse(string value) {
			if (value == null) throw new ArgumentNullException("value");

			#region Specified types
			value = value.ToUpper().Snip('L');
			if (value.Contains("M")) {
				try {
					return Double.Parse(value.Snip('M'), CultureInfo.InvariantCulture);
				}
				catch (FormatException) { }
				catch (OverflowException) { }
			}
			else if (value.Contains("D")) {
				try {
					return Double.Parse(value.Snip('D'), CultureInfo.InvariantCulture);
				}
				catch (FormatException) { }
				catch (OverflowException) { }
			}
			else if (value.Contains("F")) {
				try {
					return Single.Parse(value.Snip('F'), CultureInfo.InvariantCulture);
				}
				catch (FormatException) { }
				catch (OverflowException) { }
			}
			else if (value.Contains("U")) {
				try {
					return UInt64.Parse(value.Snip('U'), CultureInfo.InvariantCulture);
				}
				catch (FormatException) { }
				catch (OverflowException) { }
			}
			#endregion

			try {
				try {
					return Int64.Parse(value, CultureInfo.InvariantCulture);
				}
				catch (FormatException) {
					return Double.Parse(value, CultureInfo.InvariantCulture);
				}
				catch (OverflowException) {
					return UInt64.Parse(value, CultureInfo.InvariantCulture);
				}
			}
			catch (FormatException e) {
				throw new FormatException("Could not parse string '" + value + "': Not a valid number.", e);
			}
			catch (OverflowException e) {
				throw new FormatException("Could not parse string '" + value + "': Value is too large/small.", e);
			}
		}

		private static TypeCode NumericTypeToTypeCode(NumericType numericType) {
			switch (numericType) {
				case NumericType.SByte: return TypeCode.SByte;
				case NumericType.UByte: return TypeCode.Byte;
				case NumericType.SShort: return TypeCode.Int16;
				case NumericType.UShort: return TypeCode.UInt16;
				case NumericType.SInt: return TypeCode.Int32;
				case NumericType.UInt: return TypeCode.UInt32;
				case NumericType.SLong: return TypeCode.Int64;
				case NumericType.ULong: return TypeCode.UInt64;
				case NumericType.Float: return TypeCode.Single;
				case NumericType.Double: return TypeCode.Double;
				default: return TypeCode.Object;
			}
		}
		private static NumericType TypeCodeToNumericType(TypeCode typeCode) {
			switch (typeCode) {
				case TypeCode.SByte: return NumericType.SByte;
				case TypeCode.Byte: return NumericType.UByte;
				case TypeCode.Int16: return NumericType.SShort;
				case TypeCode.UInt16: return NumericType.UShort;
				case TypeCode.Int32: return NumericType.SInt;
				case TypeCode.UInt32: return NumericType.UInt;
				case TypeCode.Int64: return NumericType.SLong;
				case TypeCode.UInt64: return NumericType.ULong;
				case TypeCode.Single: return NumericType.Float;
				case TypeCode.Double: return NumericType.Double;
				default: return NumericType.Double;
			}
		}

		/// <summary>
		/// Returns the <see cref="T:System.TypeCode"/> for this instance.
		/// </summary>
		/// <returns>
		/// The enumerated constant that is the <see cref="T:System.TypeCode"/> of the class or value type that implements this interface.
		/// </returns>
		/// <filterpriority>2</filterpriority>
		public TypeCode GetTypeCode() {
			return NumericTypeToTypeCode(actualType);
		}

		/// <summary>
		/// Returns a string representation of the <see cref="ActualValue"/> that this Number represents.
		/// </summary>
		/// <returns>
		/// A string formatted according to the type of <see cref="ActualValue"/>.
		/// </returns>
		public override string ToString() {
			if (IsIntegral) {
				if (IsSigned) return ((Int64) this).ToString();
				else return ((UInt64) this).ToString();
			}
			else return String.Format(RATIONAL_TO_STRING_FORMAT, ((Double) this));
		}

		/// <summary>
		/// Returns a string representation of the <see cref="ActualValue"/> that this Number represents.
		/// </summary>
		/// <param name="numDecimalPlaces">The number of decimal places to include in the output string.</param>
		/// <returns>A string formatted with the specified number of decimal places.</returns>
		public string ToString(int numDecimalPlaces) {
			if (numDecimalPlaces < 0) throw new ArgumentException("Number of decimal places must be positive.", "numDecimalPlaces");

			string formatString = "{0:n" + numDecimalPlaces + "}";

			if (IsIntegral) {
				if (IsSigned) return String.Format(formatString, ((Int64) this));
				else return String.Format(formatString, ((UInt64) this));
			}
			else return String.Format(formatString, ((Double) this));
		}

		bool IConvertible.ToBoolean(IFormatProvider provider) {
			return asUnsignedLong != 0UL;
		}

		char IConvertible.ToChar(IFormatProvider provider) {
			return (char) (this as IConvertible).ToUInt16(provider);
		}

		sbyte IConvertible.ToSByte(IFormatProvider provider) {
			return (SByte) this;
		}

		byte IConvertible.ToByte(IFormatProvider provider) {
			return (Byte) this;
		}

		short IConvertible.ToInt16(IFormatProvider provider) {
			return (Int16) this;
		}

		ushort IConvertible.ToUInt16(IFormatProvider provider) {
			return (UInt16) this;
		}

		int IConvertible.ToInt32(IFormatProvider provider) {
			return (Int32) this;
		}

		uint IConvertible.ToUInt32(IFormatProvider provider) {
			return (UInt32) this;
		}

		long IConvertible.ToInt64(IFormatProvider provider) {
			return (Int64) this;
		}

		ulong IConvertible.ToUInt64(IFormatProvider provider) {
			return (UInt64) this;
		}

		float IConvertible.ToSingle(IFormatProvider provider) {
			return (Single) this;
		}

		double IConvertible.ToDouble(IFormatProvider provider) {
			return (Double) this;
		}

		decimal IConvertible.ToDecimal(IFormatProvider provider) {
			return (Decimal) this;
		}

		DateTime IConvertible.ToDateTime(IFormatProvider provider) {
			throw new InvalidCastException("Can not cast Number to DateTime.");
		}

		string IConvertible.ToString(IFormatProvider provider) {
			return ToString();
		}

		object IConvertible.ToType(Type conversionType, IFormatProvider provider) {
			if (conversionType == typeof(Double)) return (this as IConvertible).ToDouble(provider);
			if (conversionType == typeof(Single)) return (this as IConvertible).ToSingle(provider);
			if (conversionType == typeof(Int64)) return (this as IConvertible).ToInt64(provider);
			if (conversionType == typeof(Int16)) return (this as IConvertible).ToInt16(provider);
			if (conversionType == typeof(Byte)) return (this as IConvertible).ToByte(provider);
			if (conversionType == typeof(SByte)) return (this as IConvertible).ToSByte(provider);
			if (conversionType == typeof(UInt32)) return (this as IConvertible).ToUInt32(provider);
			if (conversionType == typeof(UInt64)) return (this as IConvertible).ToUInt64(provider);
			if (conversionType == typeof(UInt16)) return (this as IConvertible).ToUInt16(provider);
			if (conversionType == typeof(Decimal)) return (this as IConvertible).ToDecimal(provider);
			if (conversionType == typeof(Int32)) return (this as IConvertible).ToInt32(provider);
			if (conversionType == typeof(Number) || conversionType == typeof(Object)) return this;
			throw new InvalidCastException("Can not cast Number to " + (conversionType == null ? "null" : conversionType.Name) + ".");
		}

		/// <summary>
		/// Stores the <paramref name="operand"/> in a new Number.
		/// </summary>
		/// <param name="operand">The value to store.</param>
		/// <returns>The equivalent of <c>new Number(operand)</c>.</returns>
		public static implicit operator Number(SByte operand) {
			return new Number(operand);
		}
		/// <summary>
		/// Stores the <paramref name="operand"/> in a new Number.
		/// </summary>
		/// <param name="operand">The value to store.</param>
		/// <returns>The equivalent of <c>new Number(operand)</c>.</returns>
		public static implicit operator Number(Byte operand) {
			return new Number(operand);
		}
		/// <summary>
		/// Stores the <paramref name="operand"/> in a new Number.
		/// </summary>
		/// <param name="operand">The value to store.</param>
		/// <returns>The equivalent of <c>new Number(operand)</c>.</returns>
		public static implicit operator Number(Int16 operand) {
			return new Number(operand);
		}
		/// <summary>
		/// Stores the <paramref name="operand"/> in a new Number.
		/// </summary>
		/// <param name="operand">The value to store.</param>
		/// <returns>The equivalent of <c>new Number(operand)</c>.</returns>
		public static implicit operator Number(UInt16 operand) {
			return new Number(operand);
		}
		/// <summary>
		/// Stores the <paramref name="operand"/> in a new Number.
		/// </summary>
		/// <param name="operand">The value to store.</param>
		/// <returns>The equivalent of <c>new Number(operand)</c>.</returns>
		public static implicit operator Number(Int32 operand) {
			return new Number(operand);
		}
		/// <summary>
		/// Stores the <paramref name="operand"/> in a new Number.
		/// </summary>
		/// <param name="operand">The value to store.</param>
		/// <returns>The equivalent of <c>new Number(operand)</c>.</returns>
		public static implicit operator Number(UInt32 operand) {
			return new Number(operand);
		}
		/// <summary>
		/// Stores the <paramref name="operand"/> in a new Number.
		/// </summary>
		/// <param name="operand">The value to store.</param>
		/// <returns>The equivalent of <c>new Number(operand)</c>.</returns>
		public static implicit operator Number(Int64 operand) {
			return new Number(operand);
		}
		/// <summary>
		/// Stores the <paramref name="operand"/> in a new Number.
		/// </summary>
		/// <param name="operand">The value to store.</param>
		/// <returns>The equivalent of <c>new Number(operand)</c>.</returns>
		public static implicit operator Number(UInt64 operand) {
			return new Number(operand);
		}
		/// <summary>
		/// Stores the <paramref name="operand"/> in a new Number.
		/// </summary>
		/// <param name="operand">The value to store.</param>
		/// <returns>The equivalent of <c>new Number(operand)</c>.</returns>
		public static implicit operator Number(Single operand) {
			return new Number(operand);
		}
		/// <summary>
		/// Stores the <paramref name="operand"/> in a new Number.
		/// </summary>
		/// <param name="operand">The value to store.</param>
		/// <returns>The equivalent of <c>new Number(operand)</c>.</returns>
		public static implicit operator Number(Double operand) {
			return new Number(operand);
		}

		/// <summary>
		/// Casts the internal value (<see cref="ActualValue"/>) of the <paramref name="operand"/> to a <see cref="SByte"/> without boxing.
		/// </summary>
		/// <param name="operand">The number to cast.</param>
		/// <returns>The equivalent to <c>(SByte) operand.ActualValue</c>, but without boxing.</returns>
		public static explicit operator SByte(Number operand) {
			switch (operand.actualType) {
				case NumericType.SByte: return operand.asSignedByte;
				case NumericType.UByte: return (SByte) operand.asUnsignedByte;
				case NumericType.SShort: return (SByte) operand.asSignedShort;
				case NumericType.UShort: return (SByte) operand.asUnsignedShort;
				case NumericType.SInt: return (SByte) operand.asSignedInt;
				case NumericType.UInt: return (SByte) operand.asUnsignedInt;
				case NumericType.SLong: return (SByte) operand.asSignedLong;
				case NumericType.ULong: return (SByte) operand.asUnsignedLong;
				case NumericType.Float: return (SByte) operand.asFloat;
				case NumericType.Double: return (SByte) operand.asDouble;
				default: return operand.asSignedByte;
			}
		}

		/// <summary>
		/// Casts the internal value (<see cref="ActualValue"/>) of the <paramref name="operand"/> to a <see cref="Byte"/> without boxing.
		/// </summary>
		/// <param name="operand">The number to cast.</param>
		/// <returns>The equivalent to <c>(Byte) operand.ActualValue</c>, but without boxing.</returns>
		public static explicit operator Byte(Number operand) {
			switch (operand.actualType) {
				case NumericType.SByte: return (Byte) operand.asSignedByte;
				case NumericType.UByte: return operand.asUnsignedByte;
				case NumericType.SShort: return (Byte) operand.asSignedShort;
				case NumericType.UShort: return (Byte) operand.asUnsignedShort;
				case NumericType.SInt: return (Byte) operand.asSignedInt;
				case NumericType.UInt: return (Byte) operand.asUnsignedInt;
				case NumericType.SLong: return (Byte) operand.asSignedLong;
				case NumericType.ULong: return (Byte) operand.asUnsignedLong;
				case NumericType.Float: return (Byte) operand.asFloat;
				case NumericType.Double: return (Byte) operand.asDouble;
				default: return operand.asUnsignedByte;
			}
		}

		/// <summary>
		/// Casts the internal value (<see cref="ActualValue"/>) of the <paramref name="operand"/> to an <see cref="Int16"/> without boxing.
		/// </summary>
		/// <param name="operand">The number to cast.</param>
		/// <returns>The equivalent to <c>(Int16) operand.ActualValue</c>, but without boxing.</returns>
		public static explicit operator Int16(Number operand) {
			switch (operand.actualType) {
				case NumericType.SByte: return operand.asSignedByte;
				case NumericType.UByte: return operand.asUnsignedByte;
				case NumericType.SShort: return operand.asSignedShort;
				case NumericType.UShort: return (Int16) operand.asUnsignedShort;
				case NumericType.SInt: return (Int16) operand.asSignedInt;
				case NumericType.UInt: return (Int16) operand.asUnsignedInt;
				case NumericType.SLong: return (Int16) operand.asSignedLong;
				case NumericType.ULong: return (Int16) operand.asUnsignedLong;
				case NumericType.Float: return (Int16) operand.asFloat;
				case NumericType.Double: return (Int16) operand.asDouble;
				default: return operand.asSignedShort;
			}
		}

		/// <summary>
		/// Casts the internal value (<see cref="ActualValue"/>) of the <paramref name="operand"/> to an <see cref="UInt16"/> without boxing.
		/// </summary>
		/// <param name="operand">The number to cast.</param>
		/// <returns>The equivalent to <c>(UInt16) operand.ActualValue</c>, but without boxing.</returns>
		public static explicit operator UInt16(Number operand) {
			switch (operand.actualType) {
				case NumericType.SByte: return (UInt16) operand.asSignedByte;
				case NumericType.UByte: return operand.asUnsignedByte;
				case NumericType.SShort: return (UInt16) operand.asSignedShort;
				case NumericType.UShort: return operand.asUnsignedShort;
				case NumericType.SInt: return (UInt16) operand.asSignedInt;
				case NumericType.UInt: return (UInt16) operand.asUnsignedInt;
				case NumericType.SLong: return (UInt16) operand.asSignedLong;
				case NumericType.ULong: return (UInt16) operand.asUnsignedLong;
				case NumericType.Float: return (UInt16) operand.asFloat;
				case NumericType.Double: return (UInt16) operand.asDouble;
				default: return operand.asUnsignedShort;
			}
		}

		/// <summary>
		/// Casts the internal value (<see cref="ActualValue"/>) of the <paramref name="operand"/> to an <see cref="Int32"/> without boxing.
		/// </summary>
		/// <param name="operand">The number to cast.</param>
		/// <returns>The equivalent to <c>(Int32) operand.ActualValue</c>, but without boxing.</returns>
		public static explicit operator Int32(Number operand) {
			switch (operand.actualType) {
				case NumericType.SByte: return operand.asSignedByte;
				case NumericType.UByte: return operand.asUnsignedByte;
				case NumericType.SShort: return operand.asSignedShort;
				case NumericType.UShort: return operand.asUnsignedShort;
				case NumericType.SInt: return operand.asSignedInt;
				case NumericType.UInt: return (Int32) operand.asUnsignedInt;
				case NumericType.SLong: return (Int32) operand.asSignedLong;
				case NumericType.ULong: return (Int32) operand.asUnsignedLong;
				case NumericType.Float: return (Int32) operand.asFloat;
				case NumericType.Double: return (Int32) operand.asDouble;
				default: return operand.asSignedInt;
			}
		}

		/// <summary>
		/// Casts the internal value (<see cref="ActualValue"/>) of the <paramref name="operand"/> to an <see cref="UInt32"/> without boxing.
		/// </summary>
		/// <param name="operand">The number to cast.</param>
		/// <returns>The equivalent to <c>(UInt32) operand.ActualValue</c>, but without boxing.</returns>
		public static explicit operator UInt32(Number operand) {
			switch (operand.actualType) {
				case NumericType.SByte: return (UInt32) operand.asSignedByte;
				case NumericType.UByte: return operand.asUnsignedByte;
				case NumericType.SShort: return (UInt32) operand.asSignedShort;
				case NumericType.UShort: return operand.asUnsignedShort;
				case NumericType.SInt: return (UInt32) operand.asSignedInt;
				case NumericType.UInt: return operand.asUnsignedInt;
				case NumericType.SLong: return (UInt32) operand.asSignedLong;
				case NumericType.ULong: return (UInt32) operand.asUnsignedLong;
				case NumericType.Float: return (UInt32) operand.asFloat;
				case NumericType.Double: return (UInt32) operand.asDouble;
				default: return operand.asUnsignedInt;
			}
		}

		/// <summary>
		/// Casts the internal value (<see cref="ActualValue"/>) of the <paramref name="operand"/> to an <see cref="Int64"/> without boxing.
		/// </summary>
		/// <param name="operand">The number to cast.</param>
		/// <returns>The equivalent to <c>(Int64) operand.ActualValue</c>, but without boxing.</returns>
		public static explicit operator Int64(Number operand) {
			switch (operand.actualType) {
				case NumericType.SByte: return operand.asSignedByte;
				case NumericType.UByte: return operand.asUnsignedByte;
				case NumericType.SShort: return operand.asSignedShort;
				case NumericType.UShort: return operand.asUnsignedShort;
				case NumericType.SInt: return operand.asSignedInt;
				case NumericType.UInt: return operand.asUnsignedInt;
				case NumericType.SLong: return operand.asSignedLong;
				case NumericType.ULong: return (Int64) operand.asUnsignedLong;
				case NumericType.Float: return (Int64) operand.asFloat;
				case NumericType.Double: return (Int64) operand.asDouble;
				default: return operand.asSignedLong;
			}
		}

		/// <summary>
		/// Casts the internal value (<see cref="ActualValue"/>) of the <paramref name="operand"/> to an <see cref="UInt64"/> without boxing.
		/// </summary>
		/// <param name="operand">The number to cast.</param>
		/// <returns>The equivalent to <c>(UInt64) operand.ActualValue</c>, but without boxing.</returns>
		public static explicit operator UInt64(Number operand) {
			switch (operand.actualType) {
				case NumericType.SByte: return (UInt64) operand.asSignedByte;
				case NumericType.UByte: return operand.asUnsignedByte;
				case NumericType.SShort: return (UInt64) operand.asSignedShort;
				case NumericType.UShort: return operand.asUnsignedShort;
				case NumericType.SInt: return (UInt64) operand.asSignedInt;
				case NumericType.UInt: return operand.asUnsignedInt;
				case NumericType.SLong: return (UInt64) operand.asSignedLong;
				case NumericType.ULong: return operand.asUnsignedLong;
				case NumericType.Float: return (UInt64) operand.asFloat;
				case NumericType.Double: return (UInt64) operand.asDouble;
				default: return operand.asUnsignedLong;
			}
		}

		/// <summary>
		/// Casts the internal value (<see cref="ActualValue"/>) of the <paramref name="operand"/> to a <see cref="Single"/> without boxing.
		/// </summary>
		/// <param name="operand">The number to cast.</param>
		/// <returns>The equivalent to <c>(Single) operand.ActualValue</c>, but without boxing.</returns>
		public static explicit operator Single(Number operand) {
			switch (operand.actualType) {
				case NumericType.SByte: return operand.asSignedByte;
				case NumericType.UByte: return operand.asUnsignedByte;
				case NumericType.SShort: return operand.asSignedShort;
				case NumericType.UShort: return operand.asUnsignedShort;
				case NumericType.SInt: return operand.asSignedInt;
				case NumericType.UInt: return operand.asUnsignedInt;
				case NumericType.SLong: return operand.asSignedLong;
				case NumericType.ULong: return operand.asUnsignedLong;
				case NumericType.Float: return operand.asFloat;
				case NumericType.Double: return (Single) operand.asDouble;
				default: return operand.asFloat;
			}
		}

		/// <summary>
		/// Casts the internal value (<see cref="ActualValue"/>) of the <paramref name="operand"/> to a <see cref="Double"/> without boxing.
		/// </summary>
		/// <param name="operand">The number to cast.</param>
		/// <returns>The equivalent to <c>(Double) operand.ActualValue</c>, but without boxing.</returns>
		public static explicit operator Double(Number operand) {
			switch (operand.actualType) {
				case NumericType.SByte: return operand.asSignedByte;
				case NumericType.UByte: return operand.asUnsignedByte;
				case NumericType.SShort: return operand.asSignedShort;
				case NumericType.UShort: return operand.asUnsignedShort;
				case NumericType.SInt: return operand.asSignedInt;
				case NumericType.UInt: return operand.asUnsignedInt;
				case NumericType.SLong: return operand.asSignedLong;
				case NumericType.ULong: return operand.asUnsignedLong;
				case NumericType.Float: return operand.asFloat;
				case NumericType.Double: return operand.asDouble;
				default: return operand.asDouble;
			}
		}

		/// <summary>
		/// Casts the internal value (<see cref="ActualValue"/>) of the <paramref name="operand"/> to a <see cref="Decimal"/> without boxing.
		/// </summary>
		/// <param name="operand">The number to cast.</param>
		/// <returns>The equivalent to <c>(Decimal) operand.ActualValue</c>, but without boxing.</returns>
		public static explicit operator Decimal(Number operand) {
			switch (operand.actualType) {
				case NumericType.SByte: return operand.asSignedByte;
				case NumericType.UByte: return operand.asUnsignedByte;
				case NumericType.SShort: return operand.asSignedShort;
				case NumericType.UShort: return operand.asUnsignedShort;
				case NumericType.SInt: return operand.asSignedInt;
				case NumericType.UInt: return operand.asUnsignedInt;
				case NumericType.SLong: return operand.asSignedLong;
				case NumericType.ULong: return operand.asUnsignedLong;
				case NumericType.Float: return (Decimal) operand.asFloat;
				case NumericType.Double: return (Decimal) operand.asDouble;
				default: return (Decimal) operand.asDouble;
			}
		}
	}
}