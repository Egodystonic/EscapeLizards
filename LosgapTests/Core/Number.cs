// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 14 10 2014 at 14:18 by Ben Bowen

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap {
	[TestClass]
	public partial class NumberTest {
		private const SByte CONST_SBYTE = -19;
		private const Byte CONST_BYTE = 68;
		private const Int16 CONST_INT16 = -9136;
		private const UInt16 CONST_UINT16 = 54321;
		private const Int32 CONST_INT32 = -912751;
		private const UInt32 CONST_UINT32 = 3978465330;
		private const Int64 CONST_INT64 = -97846533043;
		private const UInt64 CONST_UINT64 = UInt64.MaxValue;
		private const Single CONST_SINGLE = 12.453f;
		private const Double CONST_DOUBLE = -561.1616f;

		private readonly Number[] allNumbers = {
			CONST_SBYTE,
			CONST_BYTE,
			CONST_INT16,
			CONST_UINT16,
			CONST_INT32,
			CONST_UINT32,
			CONST_INT64,
			CONST_UINT64,
			CONST_SINGLE,
			CONST_DOUBLE,
		};

		[TestInitialize]
		public void SetUp() { }


		#region Tests
		[TestMethod]
		public void ShouldCastBackToOriginalTypeWithoutLoss() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(CONST_SBYTE, (SByte) new Number(CONST_SBYTE));
			Assert.AreEqual(CONST_BYTE, (Byte) new Number(CONST_BYTE));
			Assert.AreEqual(CONST_INT16, (Int16) new Number(CONST_INT16));
			Assert.AreEqual(CONST_UINT16, (UInt16) new Number(CONST_UINT16));
			Assert.AreEqual(CONST_INT32, (Int32) new Number(CONST_INT32));
			Assert.AreEqual(CONST_UINT32, (UInt32) new Number(CONST_UINT32));
			Assert.AreEqual(CONST_INT64, (Int64) new Number(CONST_INT64));
			Assert.AreEqual(CONST_UINT64, (UInt64) new Number(CONST_UINT64));
			Assert.AreEqual(CONST_SINGLE, (Single) new Number(CONST_SINGLE));
			Assert.AreEqual(CONST_DOUBLE, (Double) new Number(CONST_DOUBLE));
		}

		[TestMethod]
		public void TestParse() {
			// Define variables and constants
			const string INT = "3";
			const string UINT = "3U";
			const string LONG_A = "3000653412L";
			const string LONG_B = "3000653412";
			const string ULONG_A = "3000653412UL";
			const string ULONG_B = "3000653412U";
			const string ULONG_C = "9223372036854775808";
			const string FLOAT = "166.611";
			const string DOUBLE = "-135.41d";
			const string DECIMAL = "4000.10m";

			const string INVALID_A = "-9U";
			const string INVALID_B = "xyz";
			const string INVALID_C = "";
			const string INVALID_D = "315671306713690713690713690713690706971369071369071369013760";
			const string INVALID_E = null;

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(3L, Number.Parse(INT).ActualValue);
			Assert.AreEqual(3UL, Number.Parse(UINT).ActualValue);
			Assert.AreEqual(3000653412L, Number.Parse(LONG_A).ActualValue);
			Assert.AreEqual(3000653412L, Number.Parse(LONG_B).ActualValue);
			Assert.AreEqual(3000653412UL, Number.Parse(ULONG_A).ActualValue);
			Assert.AreEqual(3000653412UL, Number.Parse(ULONG_B).ActualValue);
			Assert.AreEqual(9223372036854775808UL, Number.Parse(ULONG_C).ActualValue);
			Assert.AreEqual(166.611d, Number.Parse(FLOAT).ActualValue);
			Assert.AreEqual(-135.41d, Number.Parse(DOUBLE).ActualValue);
			Assert.AreEqual(4000.10d, Number.Parse(DECIMAL).ActualValue);

			try {
				Number.Parse(INVALID_A);
				Assert.Fail();
			}
			catch (FormatException) { }

			try {
				Number.Parse(INVALID_B);
				Assert.Fail();
			}
			catch (FormatException) { }
			try {
				Number.Parse(INVALID_C);
				Assert.Fail();
			}
			catch (FormatException) { }
			try {
				Number.Parse(INVALID_D);
				Assert.Fail();
			}
			catch (FormatException) { }
			try {
				Number.Parse(INVALID_E);
				Assert.Fail();
			}
			catch (ArgumentNullException) { }
		}

		[TestMethod]
		public void ShouldCorrectlyBoxAndUnboxWithoutLosingTypeInformation() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			foreach (Number number in allNumbers) {
				Assert.AreEqual(number.ActualValue, new Number(number.ActualValue).ActualValue);
				Assert.AreEqual(number.GetTypeCode(), new Number(number.ActualValue).GetTypeCode());
				Assert.AreEqual(number.ActualValue.GetType(), new Number(number.ActualValue).ActualValue.GetType());
			}
		}

		[TestMethod]
		public void TestConstrain() {
			// Define variables and constants

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual((sbyte) 3, Number.ConstrainToSByte((byte) 3));
			Assert.AreEqual(SByte.MaxValue, Number.ConstrainToSByte((byte) 190));
			Assert.AreEqual((sbyte) 70, Number.ConstrainToSByte(70f));
			Assert.AreEqual(SByte.MinValue, Number.ConstrainToSByte(-1561361d));

			Assert.AreEqual((byte) 3, Number.ConstrainToByte((sbyte) 3));
			Assert.AreEqual(Byte.MinValue, Number.ConstrainToByte((sbyte) -10));
			Assert.AreEqual((byte) 70, Number.ConstrainToByte(70f));
			Assert.AreEqual(Byte.MaxValue, Number.ConstrainToByte(1561361d));

			Assert.AreEqual((short) 3, Number.ConstrainToInt16(3));
			Assert.AreEqual(Int16.MinValue, Number.ConstrainToInt16(-151361));
			Assert.AreEqual(Int16.MaxValue, Number.ConstrainToInt16(60000f));
			Assert.AreEqual((short) -900, Number.ConstrainToInt16(-900d));

			Assert.AreEqual((ushort) 3, Number.ConstrainToUInt16((byte) 3));
			Assert.AreEqual(UInt16.MinValue, Number.ConstrainToUInt16(-15));
			Assert.AreEqual((ushort) 60000, Number.ConstrainToUInt16(60000f));
			Assert.AreEqual(UInt16.MaxValue, Number.ConstrainToUInt16(70000.136161d));

			Assert.AreEqual(3, Number.ConstrainToInt32((sbyte) 3));
			Assert.AreEqual(Int32.MinValue, Number.ConstrainToInt32(-151361246246L));
			Assert.AreEqual(Int32.MaxValue, Number.ConstrainToInt32(6000961060f));
			Assert.AreEqual(-900, Number.ConstrainToInt32(-900d));

			Assert.AreEqual(3U, Number.ConstrainToUInt32((byte) 3));
			Assert.AreEqual(UInt32.MinValue, Number.ConstrainToUInt32(-15));
			Assert.AreEqual(600000U, Number.ConstrainToUInt32(600000f));
			Assert.AreEqual(UInt32.MaxValue, Number.ConstrainToUInt32(700614624600.136161d));

			Assert.AreEqual(-50L, Number.ConstrainToInt64((sbyte) -50L));
			Assert.AreEqual(Int64.MinValue, Number.ConstrainToInt64(Int64.MinValue));
			Assert.AreEqual(Int64.MaxValue, Number.ConstrainToInt64(UInt64.MaxValue));
			Assert.AreEqual(-900L, Number.ConstrainToInt64(-900d));

			Assert.AreEqual(3UL, Number.ConstrainToUInt64((byte) 3));
			Assert.AreEqual(UInt64.MinValue, Number.ConstrainToUInt64(-15L));
			Assert.AreEqual(600000UL, Number.ConstrainToUInt64(600000f));
			Assert.AreEqual(UInt64.MaxValue, Number.ConstrainToUInt64(UInt64.MaxValue));

			Assert.AreEqual(3f, Number.ConstrainToSingle((byte) 3));
			Assert.AreEqual(Single.MinValue, Number.ConstrainToSingle(Double.MinValue));
			Assert.AreEqual(600000f, Number.ConstrainToSingle(600000U));
			Assert.AreEqual(Single.MaxValue, Number.ConstrainToSingle(Double.MaxValue));

			Assert.AreEqual(3d, Number.ConstrainToDouble(3UL));
			Assert.AreEqual(Double.MinValue, Number.ConstrainToDouble(Double.MinValue));
			Assert.AreEqual(600000d, Number.ConstrainToDouble(600000U));
			Assert.AreEqual(Double.MaxValue, Number.ConstrainToDouble(Double.MaxValue));
		}

		[TestMethod]
		public void TestToString() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			for (int i = 0; i < allNumbers.Length; i++) {
				if (allNumbers[i].IsIntegral) Assert.AreEqual(allNumbers[i].ActualValue.ToString(), allNumbers[i].ToString());
				else Assert.AreEqual(String.Format("{0:n4}", allNumbers[i].ActualValue), allNumbers[i].ToString());
			}
		}

		[TestMethod]
		public void TestToBytes() {
			// Define variables and constants
			MemoryStream inputStream = new MemoryStream(8);
			BinaryReader binaryDeserializer = new BinaryReader(inputStream);

			// Set up context


			// Execute


			// Assert outcome
			foreach (Number number in allNumbers) {
				inputStream.Position = 0L;
				byte[] numberAsBytes = Number.Serialize(number);
				inputStream.Write(numberAsBytes, 0, numberAsBytes.Length);
				inputStream.Position = 0L;
				switch (number.GetTypeCode()) {
					case TypeCode.SByte:
						Assert.AreEqual(number.ActualValue, binaryDeserializer.ReadSByte());
						return;
					case TypeCode.Byte:
						Assert.AreEqual(number.ActualValue, binaryDeserializer.ReadByte());
						return;
					case TypeCode.Int16:
						Assert.AreEqual(number.ActualValue, binaryDeserializer.ReadInt16());
						return;
					case TypeCode.UInt16:
						Assert.AreEqual(number.ActualValue, binaryDeserializer.ReadUInt16());
						return;
					case TypeCode.Int32:
						Assert.AreEqual(number.ActualValue, binaryDeserializer.ReadInt32());
						return;
					case TypeCode.UInt32:
						Assert.AreEqual(number.ActualValue, binaryDeserializer.ReadUInt32());
						return;
					case TypeCode.Int64:
						Assert.AreEqual(number.ActualValue, binaryDeserializer.ReadInt64());
						return;
					case TypeCode.UInt64:
						Assert.AreEqual(number.ActualValue, binaryDeserializer.ReadUInt64());
						return;
					case TypeCode.Single:
						Assert.AreEqual(number.ActualValue, binaryDeserializer.ReadSingle());
						return;
					case TypeCode.Double:
						Assert.AreEqual(number.ActualValue, binaryDeserializer.ReadDouble());
						return;
					default:
						Assert.Fail("Unexpected type code.");
						return;
				}
			}
		}

		[TestMethod]
		public void TestIsProperties() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsTrue(new Number(CONST_SBYTE).IsIntegral);
			Assert.IsTrue(new Number(CONST_BYTE).IsIntegral);
			Assert.IsTrue(new Number(CONST_INT16).IsIntegral);
			Assert.IsTrue(new Number(CONST_UINT16).IsIntegral);
			Assert.IsTrue(new Number(CONST_INT32).IsIntegral);
			Assert.IsTrue(new Number(CONST_UINT32).IsIntegral);
			Assert.IsTrue(new Number(CONST_INT64).IsIntegral);
			Assert.IsTrue(new Number(CONST_UINT64).IsIntegral);
			Assert.IsFalse(new Number(CONST_SINGLE).IsIntegral);
			Assert.IsFalse(new Number(CONST_DOUBLE).IsIntegral);

			Assert.IsTrue(new Number(CONST_SBYTE).IsSigned);
			Assert.IsTrue(new Number(CONST_INT16).IsSigned);
			Assert.IsTrue(new Number(CONST_INT32).IsSigned);
			Assert.IsTrue(new Number(CONST_INT64).IsSigned);
			Assert.IsTrue(new Number(CONST_SINGLE).IsSigned);
			Assert.IsTrue(new Number(CONST_DOUBLE).IsSigned);
			Assert.IsFalse(new Number(CONST_BYTE).IsSigned);
			Assert.IsFalse(new Number(CONST_UINT16).IsSigned);
			Assert.IsFalse(new Number(CONST_UINT32).IsSigned);
			Assert.IsFalse(new Number(CONST_UINT64).IsSigned);
		}

		[TestMethod]
		public void TestIsNumeric() {
			// Define variables and constants
			Number number = 3;
			const string STRING = "Hi";
			const bool BOOL = true;
			object @object = new object();
			const decimal DECIMAL = 12m;

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsTrue(Number.IsNumeric(number));
			Assert.IsFalse(Number.IsNumeric(STRING));
			Assert.IsFalse(Number.IsNumeric(BOOL));
			Assert.IsFalse(Number.IsNumeric(@object));
			Assert.IsTrue(Number.IsNumeric(DECIMAL));

			Assert.IsTrue(Number.IsNumericType(number.GetType()));
			Assert.IsFalse(Number.IsNumericType(STRING.GetType()));
			Assert.IsFalse(Number.IsNumericType(BOOL.GetType()));
			Assert.IsFalse(Number.IsNumericType(@object.GetType()));
			Assert.IsTrue(Number.IsNumericType(DECIMAL.GetType()));

			foreach (Number num in allNumbers) {
				Assert.IsTrue(Number.IsNumeric(num.ActualValue));
				Assert.IsTrue(Number.IsNumericType(num.ActualValue.GetType()));
				Assert.IsTrue(Number.IsNumericType(num.GetTypeCode()));
				Assert.IsTrue(Number.IsNumericType(num.GetTypeCode().ToType()));
			}
		}

		[TestMethod]
		public void TestMinAndMaxValue() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			foreach (Number number in allNumbers) {
				Type numberType = number.ActualValue.GetType();
				// ReSharper disable PossibleNullReferenceException
				object actualMax = numberType.GetField("MaxValue", BindingFlags.Static | BindingFlags.Public).GetValue(null);
				object actualMin = numberType.GetField("MinValue", BindingFlags.Static | BindingFlags.Public).GetValue(null);
				// ReSharper restore PossibleNullReferenceException

				Assert.AreEqual(actualMax, Number.MaxValue(numberType).ActualValue);
				Assert.AreEqual(actualMax, Number.MaxValue(number.GetTypeCode()).ActualValue);
				Assert.AreEqual(actualMin, Number.MinValue(numberType).ActualValue);
				Assert.AreEqual(actualMin, Number.MinValue(number.GetTypeCode()).ActualValue);
			}

			Assert.AreNotEqual(new Number(-400), new Number(4294966896U));
		}

		[TestMethod]
		public void TestEquals() {
			// Define variables and constants
			Number[] fifties = {
				(sbyte) 50,
				(byte) 50,
				(short) 50,
				(ushort) 50,
				50,
				50U,
				50L,
				50UL,
				50f,
				50d
			};

			// Set up context


			// Execute


			// Assert outcome
			for (int i = 0; i < fifties.Length; i++) {
				for (int j = 0; j < fifties.Length; j++) {
					Assert.AreEqual(fifties[i], fifties[j]);
					Assert.IsTrue(fifties[i] == fifties[j]);
					Assert.IsTrue(fifties[i] == new Number(fifties[j]));
					Assert.IsTrue(new Number(fifties[i]) == fifties[j]);
				}
			}

			IEnumerable<Number> allNumbersAndFifties = allNumbers.Concat(fifties);

			foreach (Number numA in allNumbersAndFifties) {
				foreach (Number numB in allNumbersAndFifties) {
					Assert.AreEqual(numA == numB, numA.ActualValue == numB.ActualValue || numA.Equals(numB)); // The BCL number equality members are messy :(
					Assert.AreEqual(numA.Equals(numB), numB.Equals(numA));
				}
			}
		}

		[TestMethod]
		public void TestOperators() {
			// Define variables and constants
			const double RATIONAL_ERROR_MARGIN = 0.001d;

			// Set up context


			// Execute


			// Assert outcome
			foreach (Number number in allNumbers) {
				dynamic realValueDyn = number.ActualValue;
				try {
					Assert.AreEqual((Number) (-realValueDyn), -number);
					Assert.AreEqual((Number) (+realValueDyn), +number);
				}
				catch (RuntimeBinderException) { } // Ignorable 

				try {
					if (number.IsIntegral) {
						if (number.GetTypeCode() == TypeCode.SByte) {
							Assert.AreEqual((Number)(sbyte)(realValueDyn >> 4), number >> 4);
							Assert.AreEqual((Number)(sbyte)(realValueDyn << 9), number << 9);
						}
						else if (number.GetTypeCode() == TypeCode.Byte) {
							Assert.AreEqual((Number)(byte)(realValueDyn >> 4), number >> 4);
							Assert.AreEqual((Number)(byte)(realValueDyn << 9), number << 9);
						}
						else if (number.GetTypeCode() == TypeCode.Int16) {
							Assert.AreEqual((Number)(short)(realValueDyn >> 4), number >> 4);
							Assert.AreEqual((Number)(short)(realValueDyn << 9), number << 9);
						}
						else if (number.GetTypeCode() == TypeCode.UInt16) {
							Assert.AreEqual((Number)(ushort)(realValueDyn >> 4), number >> 4);
							Assert.AreEqual((Number)(ushort)(realValueDyn << 9), number << 9);
						}
						else {
							Assert.AreEqual((Number)(realValueDyn >> 4), number >> 4);
							Assert.AreEqual((Number)(realValueDyn << 9), number << 9);
						}
					}
					else if (number.GetTypeCode() == TypeCode.Single) {
						Assert.AreEqual((Number)((int)realValueDyn >> 4), number >> 4);
						Assert.AreEqual((Number)((int)realValueDyn << 9), number << 9);
					}
					else {
						Assert.AreEqual((Number)((long)realValueDyn >> 4), number >> 4);
						Assert.AreEqual((Number)((long)realValueDyn << 9), number << 9);
					}
				}
				catch (RuntimeBinderException) { } // Ignorable

				foreach (Number number2 in allNumbers) {
					dynamic realValueDyn2 = number2.ActualValue;
					if (number.IsIntegral && number2.IsIntegral) {
						try {
							// The next if check is because Number returns a different value. The CLR decides that byte + byte returns
							// an int; but it casts the operands before the operation, so (byte) 200 + (byte) 200 is 400. But Number
							// will return 144. In my opinion, this is the more correct (consistent) way. None of the answers here convinced
							// me any different: http://stackoverflow.com/questions/941584/byte-byte-int-why
							if (!number.IsSigned && !number2.IsSigned
								&& ((realValueDyn + realValueDyn2) is int || (realValueDyn - realValueDyn2) is int)) {
								Assert.AreEqual((ulong)(realValueDyn + realValueDyn2), (ulong)number + number2);
								Assert.AreEqual((ulong)(realValueDyn - realValueDyn2), (ulong)number - number2);
							}
							else {
								Assert.AreEqual((Number)(realValueDyn + realValueDyn2), number + number2);
								Assert.AreEqual((Number)(realValueDyn - realValueDyn2), number - number2);
							}
						}
						catch (RuntimeBinderException) { } // Ignorable

						try {
							// Same as before, C# promotes everything to int for fuck's sake
							if (Number.Serialize(number).Length < sizeof(short) || Number.Serialize(number2).Length < sizeof(short)) {
								Assert.AreEqual((sbyte)(realValueDyn * realValueDyn2), (sbyte)(number * number2));
								Assert.AreEqual((sbyte)(realValueDyn / realValueDyn2), (sbyte)(number / number2));
								Assert.AreEqual((sbyte)(realValueDyn % realValueDyn2), (sbyte)(number % number2));
								Assert.AreEqual((sbyte)(realValueDyn & realValueDyn2), (sbyte)(number & number2));
								Assert.AreEqual((sbyte)(realValueDyn | realValueDyn2), (sbyte)(number | number2));
								Assert.AreEqual((sbyte)(realValueDyn ^ realValueDyn2), (sbyte)(number ^ number2));
							}
							else if (Number.Serialize(number).Length < sizeof(int) || Number.Serialize(number2).Length < sizeof(int)) {
								Assert.AreEqual((short)(realValueDyn * realValueDyn2), (short)(number * number2));
								Assert.AreEqual((short)(realValueDyn / realValueDyn2), (short)(number / number2));
								Assert.AreEqual((short)(realValueDyn % realValueDyn2), (short)(number % number2));
								Assert.AreEqual((short)(realValueDyn & realValueDyn2), (short)(number & number2));
								Assert.AreEqual((short)(realValueDyn | realValueDyn2), (short)(number | number2));
								Assert.AreEqual((short)(realValueDyn ^ realValueDyn2), (short)(number ^ number2));
							}
							else {
								Assert.AreEqual((Number)(realValueDyn * realValueDyn2), number * number2);
								Assert.AreEqual((Number)(realValueDyn / realValueDyn2), number / number2);
								Assert.AreEqual((Number)(realValueDyn % realValueDyn2), number % number2);
								if (number.IsIntegral && number2.IsIntegral) {
									Assert.AreEqual((Number)(realValueDyn & realValueDyn2), number & number2);
									Assert.AreEqual((Number)(realValueDyn | realValueDyn2), number | number2);
									Assert.AreEqual((Number)(realValueDyn ^ realValueDyn2), number ^ number2);
								}
								else if (number.GetTypeCode() == TypeCode.Single && number2.GetTypeCode() == TypeCode.Single) {
									Assert.AreEqual((Number)((int)realValueDyn & (int)realValueDyn2), number & number2);
									Assert.AreEqual((Number)((int)realValueDyn | (int)realValueDyn2), number | number2);
									Assert.AreEqual((Number)((int)realValueDyn ^ (int)realValueDyn2), number ^ number2);
								}
								else {
									Assert.AreEqual((Number)((long)realValueDyn & (long)realValueDyn2), number & number2);
									Assert.AreEqual((Number)((long)realValueDyn | (long)realValueDyn2), number | number2);
									Assert.AreEqual((Number)((long)realValueDyn ^ (long)realValueDyn2), number ^ number2);
								}
							}
						}
						catch (RuntimeBinderException) { } // Ignorable

						try {
							Assert.AreEqual(realValueDyn > realValueDyn2, number > number2);
							Assert.AreEqual(realValueDyn >= realValueDyn2, number >= number2);
							Assert.AreEqual(realValueDyn < realValueDyn2, number < number2);
							Assert.AreEqual(realValueDyn <= realValueDyn2, number <= number2);
						}
						catch (RuntimeBinderException) { } // Ignorable
					}
					else {
						Assert.AreEqual((double) (Number) (realValueDyn + realValueDyn2), (double) (number + number2), RATIONAL_ERROR_MARGIN);
						Assert.AreEqual((double) (Number) (realValueDyn - realValueDyn2), (double) (number - number2), RATIONAL_ERROR_MARGIN);
						Assert.AreEqual((double) (Number) (realValueDyn * realValueDyn2), (double) (number * number2), RATIONAL_ERROR_MARGIN);
						Assert.AreEqual((double) (Number) (realValueDyn / realValueDyn2), (double) (number / number2), RATIONAL_ERROR_MARGIN);
					}
				}


				Number numberCopy = number;
				dynamic copyRealValueDyn = numberCopy.ActualValue;
				try {
					Assert.AreEqual((Number)(++copyRealValueDyn), ++numberCopy);
					Assert.AreEqual((Number)(--copyRealValueDyn), --numberCopy);
				}
				catch (RuntimeBinderException) { } // Ignorable
			}
		}
		#endregion
	}
}