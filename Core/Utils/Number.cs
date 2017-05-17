// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 13 10 2014 at 11:37 by Ben Bowen

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Ophidian.Losgap {
	/// <summary>
	/// A struct capable of representing almost any .NET BCL number (sbyte, byte, short, ushort, int, uint, long, ulong, float, double) in
	/// the same way, with minimal boxing/unboxing.
	/// </summary>
	/// <remarks>
	/// Unless stated otherwise, you can assume any operation on a Number will not require boxing/unboxing. THIS IS A LIE, DYNAMIC BOXES VALUE TYPES
	/// <para>
	/// Using a Number is considerably slower than using the raw numeric types, so it should be used in places only where it is convenient and
	/// performance/speed is not a critical concern. Furthermore, all Numbers take 16 bytes of memory, which is anywhere from 2 to 16 times
	/// more memory than the built-in numeric types.
	/// </para>
	/// <para>
	/// The standard way to use the Number type is to implicitly cast any numeric value to a Number, then explicitly cast your desired type
	/// 'back out' if required. For example, the <see cref="MathUtils.Clamp"/> method is written to be used as follows:
	/// <code>
	/// playerScore = (int) MathUtils.Clamp(playerScore, -100, 100);
	/// </code>
	/// The signature of the method and the flexibility of the Number class allows you to use any combination of numeric types
	/// and get a meaningful answer.
	/// </para>
	/// </remarks>
	[StructLayout(LayoutKind.Explicit, Size = 16)]
	public unsafe partial struct Number : IConvertible {
		private const int VALUE_OFFSET_BYTES = 8;
		[FieldOffset(0)]
		private readonly NumericType actualType;

		[FieldOffset(VALUE_OFFSET_BYTES)]
		private readonly SByte asSignedByte;
		[FieldOffset(VALUE_OFFSET_BYTES)]
		private readonly Byte asUnsignedByte;
		[FieldOffset(VALUE_OFFSET_BYTES)]
		private readonly Int16 asSignedShort;
		[FieldOffset(VALUE_OFFSET_BYTES)]
		private readonly UInt16 asUnsignedShort;
		[FieldOffset(VALUE_OFFSET_BYTES)]
		private readonly Int32 asSignedInt;
		[FieldOffset(VALUE_OFFSET_BYTES)]
		private readonly UInt32 asUnsignedInt;
		[FieldOffset(VALUE_OFFSET_BYTES)]
		private readonly Int64 asSignedLong;
		[FieldOffset(VALUE_OFFSET_BYTES)]
		private readonly UInt64 asUnsignedLong;
		[FieldOffset(VALUE_OFFSET_BYTES)]
		private readonly Single asFloat;
		[FieldOffset(VALUE_OFFSET_BYTES)]
		private readonly Double asDouble;

		/// <summary>
		/// The actual value that this Number represents.
		/// </summary>
		/// <remarks>
		/// Using this property will force boxing to occur, which incurrs a performance penalty. Prefer casting directly from Number objects
		/// instead; e.g. <c>int myValue = (int) number;</c>.
		/// </remarks>
		public object ActualValue {
			get {
				switch (actualType) {
					case NumericType.SByte: return asSignedByte;
					case NumericType.UByte: return asUnsignedByte;
					case NumericType.SShort: return asSignedShort;
					case NumericType.UShort: return asUnsignedShort;
					case NumericType.SInt: return asSignedInt;
					case NumericType.UInt: return asUnsignedInt;
					case NumericType.SLong: return asSignedLong;
					case NumericType.ULong: return asUnsignedLong;
					case NumericType.Float: return asFloat;
					case NumericType.Double: return asDouble;
					default: return asDouble;
				}
			}
		}

		/// <summary>
		/// Creates a new Number whose <see cref="ActualValue"/> is <paramref name="value"/>.
		/// </summary>
		/// <param name="value">The internal value of this Number.</param>
		public Number(sbyte value)
			: this() {
			this.asSignedByte = value;
			actualType = NumericType.SByte;
		}

		/// <summary>
		/// Creates a new Number whose <see cref="ActualValue"/> is <paramref name="value"/>.
		/// </summary>
		/// <param name="value">The internal value of this Number.</param>
		public Number(byte value)
			: this() {
			this.asUnsignedByte = value;
			actualType = NumericType.UByte;
		}

		/// <summary>
		/// Creates a new Number whose <see cref="ActualValue"/> is <paramref name="value"/>.
		/// </summary>
		/// <param name="value">The internal value of this Number.</param>
		public Number(short value)
			: this() {
				this.asSignedShort = value;
			actualType = NumericType.SShort;
		}

		/// <summary>
		/// Creates a new Number whose <see cref="ActualValue"/> is <paramref name="value"/>.
		/// </summary>
		/// <param name="value">The internal value of this Number.</param>
		public Number(ushort value)
			: this() {
				this.asUnsignedShort = value;
			actualType = NumericType.UShort;
		}

		/// <summary>
		/// Creates a new Number whose <see cref="ActualValue"/> is <paramref name="value"/>.
		/// </summary>
		/// <param name="value">The internal value of this Number.</param>
		public Number(int value)
			: this() {
				this.asSignedInt = value;
			actualType = NumericType.SInt;
		}

		/// <summary>
		/// Creates a new Number whose <see cref="ActualValue"/> is <paramref name="value"/>.
		/// </summary>
		/// <param name="value">The internal value of this Number.</param>
		public Number(uint value)
			: this() {
				this.asUnsignedInt = value;
			actualType = NumericType.UInt;
		}

		/// <summary>
		/// Creates a new Number whose <see cref="ActualValue"/> is <paramref name="value"/>.
		/// </summary>
		/// <param name="value">The internal value of this Number.</param>
		public Number(long value)
			: this() {
				this.asSignedLong = value;
			actualType = NumericType.SLong;
		}

		/// <summary>
		/// Creates a new Number whose <see cref="ActualValue"/> is <paramref name="value"/>.
		/// </summary>
		/// <param name="value">The internal value of this Number.</param>
		public Number(ulong value)
			: this() {
			this.asUnsignedLong = value;
			actualType = NumericType.ULong;
		}

		/// <summary>
		/// Creates a new Number whose <see cref="ActualValue"/> is <paramref name="value"/>.
		/// </summary>
		/// <param name="value">The internal value of this Number.</param>
		public Number(float value)
			: this() {
				this.asFloat = value;
			actualType = NumericType.Float;
		}

		/// <summary>
		/// Creates a new Number whose <see cref="ActualValue"/> is <paramref name="value"/>.
		/// </summary>
		/// <param name="value">The internal value of this Number.</param>
		public Number(double value)
			: this() {
				this.asDouble = value;
			actualType = NumericType.Double;
		}

		/// <summary>
		/// Creates a new Number whose <see cref="ActualValue"/> is <paramref name="value"/>, provided that <paramref name="value"/> is of a
		/// permitted numeric type.
		/// </summary>
		/// <remarks>
		/// The constructor will invoke an unboxing operation, which incurrs a performance penalty.
		/// </remarks>
		/// <param name="value">The internal value of this Number. Must not be null.</param>
		/// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not of a valid numeric type
		/// (one of: sbyte, byte, short, ushort, int, uint, long, ulong, float, double).</exception>
		public Number(object value)
			: this() {
			if (value == null) throw new ArgumentNullException("value");
			if (!IsNumeric(value)) {
				throw new ArgumentException("Provided object's type (" + value.GetType().Name + ") is not a numeric type.", "value");
			}
			if (value is Number) value = ((Number) value).ActualValue;

			TypeCode unknownTypeCode = Type.GetTypeCode(value.GetType());
			actualType = TypeCodeToNumericType(unknownTypeCode);
			switch (actualType) {
				case NumericType.SByte: asSignedByte = (SByte) value; break;
				case NumericType.UByte: asUnsignedByte = (Byte) value; break;
				case NumericType.SShort: asSignedShort = (Int16) value; break;
				case NumericType.UShort: asUnsignedShort = (UInt16) value; break;
				case NumericType.SInt: asSignedInt = (Int32) value; break;
				case NumericType.UInt: asUnsignedInt = (UInt32) value; break;
				case NumericType.SLong: asSignedLong = (Int64) value; break;
				case NumericType.ULong: asUnsignedLong = (UInt64) value; break;
				case NumericType.Float: asFloat = (Single) value; break;
				case NumericType.Double: asDouble = (Double) value; break;
			}
		}
	}
}