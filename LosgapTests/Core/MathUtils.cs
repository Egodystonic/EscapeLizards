// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 14 10 2014 at 13:49 by Ben Bowen

using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap {
	[TestClass]
	public class MathUtilsTest {

		[TestInitialize]
		public void SetUp() { }


		#region Tests
		[TestMethod]
		public void TestRadDegConversions() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(MathUtils.PI_OVER_TWO, MathUtils.DegToRad(90f));
			Assert.AreEqual(MathUtils.THREE_PI_OVER_TWO, MathUtils.DegToRad(270f));
			Assert.AreEqual(180f, MathUtils.RadToDeg(MathUtils.PI));
			Assert.AreEqual(360f, MathUtils.RadToDeg(MathUtils.TWO_PI));
			Assert.AreEqual(MathUtils.PI, MathUtils.DegToRad(MathUtils.RadToDeg(MathUtils.PI)));
		}

		[TestMethod]
		public void TestNormalize() {
			// Define variables and constants
			const float THREE_QUARTERS_POSITIVE = MathUtils.THREE_PI_OVER_TWO;
			const float ONE_QUARTER_POSITIVE = MathUtils.PI_OVER_TWO;
			const float ONE_HALF_NEGATIVE = -MathUtils.PI;
			const float ONE_AND_ONE_HALF_NEGATIVE = -MathUtils.PI * 3f;
			const float TEST_ERROR_MARGIN = 0.0001f;

			// Set up context


			// Execute
			

			// Assert outcome
			Assert.AreEqual(THREE_QUARTERS_POSITIVE,
				MathUtils.NormalizeRadians(THREE_QUARTERS_POSITIVE, MathUtils.RadianNormalizationRange.PositiveFullRange),
				TEST_ERROR_MARGIN);
			Assert.AreEqual(ONE_QUARTER_POSITIVE,
				MathUtils.NormalizeRadians(ONE_QUARTER_POSITIVE, MathUtils.RadianNormalizationRange.PositiveFullRange),
				TEST_ERROR_MARGIN);
			Assert.AreEqual(MathUtils.PI,
				MathUtils.NormalizeRadians(ONE_HALF_NEGATIVE, MathUtils.RadianNormalizationRange.PositiveFullRange),
				TEST_ERROR_MARGIN);
			Assert.AreEqual(MathUtils.PI,
				MathUtils.NormalizeRadians(ONE_AND_ONE_HALF_NEGATIVE, MathUtils.RadianNormalizationRange.PositiveFullRange),
				TEST_ERROR_MARGIN);

			Assert.AreEqual(-MathUtils.PI_OVER_TWO,
				MathUtils.NormalizeRadians(THREE_QUARTERS_POSITIVE, MathUtils.RadianNormalizationRange.PositiveNegativeHalfRange),
				TEST_ERROR_MARGIN);
			Assert.AreEqual(ONE_QUARTER_POSITIVE,
				MathUtils.NormalizeRadians(ONE_QUARTER_POSITIVE, MathUtils.RadianNormalizationRange.PositiveNegativeHalfRange),
				TEST_ERROR_MARGIN);
			Assert.AreEqual(ONE_HALF_NEGATIVE,
				MathUtils.NormalizeRadians(ONE_HALF_NEGATIVE, MathUtils.RadianNormalizationRange.PositiveNegativeHalfRange),
				TEST_ERROR_MARGIN);
			Assert.AreEqual(ONE_HALF_NEGATIVE,
				MathUtils.NormalizeRadians(ONE_AND_ONE_HALF_NEGATIVE, MathUtils.RadianNormalizationRange.PositiveNegativeHalfRange),
				TEST_ERROR_MARGIN);

			Assert.AreEqual(THREE_QUARTERS_POSITIVE,
				MathUtils.NormalizeRadians(THREE_QUARTERS_POSITIVE, MathUtils.RadianNormalizationRange.PositiveNegativeFullRange),
				TEST_ERROR_MARGIN);
			Assert.AreEqual(ONE_QUARTER_POSITIVE,
				MathUtils.NormalizeRadians(ONE_QUARTER_POSITIVE, MathUtils.RadianNormalizationRange.PositiveNegativeFullRange),
				TEST_ERROR_MARGIN);
			Assert.AreEqual(ONE_HALF_NEGATIVE,
				MathUtils.NormalizeRadians(ONE_HALF_NEGATIVE, MathUtils.RadianNormalizationRange.PositiveNegativeFullRange),
				TEST_ERROR_MARGIN);
			Assert.AreEqual(ONE_HALF_NEGATIVE,
				MathUtils.NormalizeRadians(ONE_AND_ONE_HALF_NEGATIVE, MathUtils.RadianNormalizationRange.PositiveNegativeFullRange),
				TEST_ERROR_MARGIN);
		}

		[TestMethod]
		public void TestPowerOfTwo() {
			// Define variables and constants
			Number[] powersOfTwo = new Number[] {
				(sbyte) 4,
				(byte) 16,
				(short) 8,
				(ushort) 1,
				0,
				(uint) (1 << 13),
				32768L,
				(ulong) 2 << 19,
				32.1515f,
				128d
			};

			Number[] nonPowersOfTwo = new Number[] {
				(sbyte) 41,
				(byte) 161,
				(short) 81,
				(ushort) 12,
				3,
				(uint) (3 << 13),
				32748L,
				(ulong) 5 << 19,
				31f,
				127.555d
			};

			// Set up context


			// Execute
			Assert.IsTrue(powersOfTwo.All(MathUtils.IsPowerOfTwo));
			Assert.IsFalse(nonPowersOfTwo.Any(MathUtils.IsPowerOfTwo));

			// Assert outcome

		}

		[TestMethod]
		public void TestMSB() {
			// Define variables and constants

			// Set up context


			// Execute
			Assert.AreEqual(3, MathUtils.MostSignificantBit((sbyte) 4));
			Assert.AreEqual(5, MathUtils.MostSignificantBit((byte) 16));
			Assert.AreEqual(4, MathUtils.MostSignificantBit((short) 8));
			Assert.AreEqual(1, MathUtils.MostSignificantBit((ushort) 1));
			Assert.AreEqual(0, MathUtils.MostSignificantBit(0));
			Assert.AreEqual(15, MathUtils.MostSignificantBit((uint) (3 << 13)));
			Assert.AreEqual(15, MathUtils.MostSignificantBit(32478L));
			Assert.AreEqual(22, MathUtils.MostSignificantBit((ulong) (5 << 19)));
			Assert.AreEqual(5, MathUtils.MostSignificantBit(31f));
			Assert.AreEqual(7, MathUtils.MostSignificantBit(127.555d));

			// Assert outcome

		}
		#endregion
	}
}