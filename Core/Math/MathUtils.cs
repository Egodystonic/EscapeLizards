// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 14 10 2014 at 09:22 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

namespace Ophidian.Losgap {
	/// <summary>
	/// A set of utility methods and constants for mathematical operations not provided by the FCL's <see cref="Math"/> type.
	/// </summary>
	public static class MathUtils {
		/// <summary>
		/// The number of milliseconds in a single second.
		/// </summary>
		public const long MILLIS_IN_ONE_SECOND = 1000L;

		/// <summary>
		/// The golden ratio; two quantities are in the golden ratio if their ratio is the same 
		/// as the ratio of their sum to the larger of the two quantities.
		/// </summary>
		public const float GOLDEN_RATIO = 1.6180339887f;

		private static float flopsErrorMargin = 0.001f;

		/// <summary>
		/// The maximum error margin permitted when doing floating-point comparisons. Mostly used by types that implement
		/// <see cref="IToleranceEquatable{T}"/>.
		/// </summary>
		public static float FlopsErrorMargin {
			get {
				float result = flopsErrorMargin;
				Thread.MemoryBarrier();
				return result;
			}
			set {
				Thread.MemoryBarrier();
				flopsErrorMargin = value;
			}
		}

		#region Radians
		/// <summary>
		/// Half of a clockwise turn, in radians.
		/// </summary>
		public const float PI = (float) Math.PI;
		/// <summary>
		/// A full clockwise turn, in radians.
		/// </summary>
		public const float TWO_PI = 6.28318530718f;
		/// <summary>
		/// Quarter of a clockwise turn, in radians.
		/// </summary>
		public const float PI_OVER_TWO = 1.57079632679f;
		/// <summary>
		/// Three-quarters of a clockwise turn, in radians.
		/// </summary>
		public const float THREE_PI_OVER_TWO = 4.71238898038f;

		/// <summary>
		/// 3.1415f / 180f; used when converting values to and from degrees and radians.
		/// </summary>
		public const float PI_OVER_180 = PI / 180f;

		/// <summary>
		/// Range to normalize an input of radians to. Used by the <see cref="MathUtils.NormalizeRadians"/> method.
		/// </summary>
		public enum RadianNormalizationRange {
			/// <summary>
			/// Normalize the input from 0 to <see cref="MathUtils.TWO_PI"/>.
			/// </summary>
			PositiveFullRange,
			/// <summary>
			/// Normalize the input from -<see cref="PI"/> to <see cref="PI"/>.
			/// </summary>
			PositiveNegativeHalfRange,
			/// <summary>
			/// Normalize the input from -<see cref="MathUtils.TWO_PI"/> to <see cref="MathUtils.TWO_PI"/>.
			/// </summary>
			PositiveNegativeFullRange
		}

		/// <summary>
		/// Converts the given value from degrees to radians.
		/// </summary>
		/// <param name="degrees">The value in degrees.</param>
		/// <returns><paramref name="degrees"/> converted to radians.</returns>
		/// <seealso cref="RadToDeg"/>
		public static float DegToRad(float degrees) {
			return degrees * PI_OVER_180;
		}

		/// <summary>
		/// Converts the given value from radians to degrees.
		/// </summary>
		/// <param name="radians">The value in radians.</param>
		/// <returns><paramref name="radians"/> converted to degrees.</returns>
		/// <seealso cref="DegToRad"/>
		public static float RadToDeg(float radians) {
			return radians / PI_OVER_180;
		}

		/// <summary>
		/// Normalizes the provided <paramref name="radians"/> value to the specified range. 
		/// This method will return a value that is mathematically similar in a trigonometric context.
		/// </summary>
		/// <param name="radians">The input value, in radians.</param>
		/// <param name="normalizationRange">The range to normalize <paramref name="radians"/> to.</param>
		/// <returns>The original value <paramref name="radians"/>, normalized to the given range.</returns>
		public static float NormalizeRadians(float radians, RadianNormalizationRange normalizationRange = RadianNormalizationRange.PositiveNegativeHalfRange) {
			switch (normalizationRange) {
				case RadianNormalizationRange.PositiveFullRange:
					while (radians < 0f) {
						radians += TWO_PI;
					}
					while (radians > TWO_PI) {
						radians -= TWO_PI;
					}
					return radians;
				case RadianNormalizationRange.PositiveNegativeHalfRange:
					while (radians < -PI) {
						radians += TWO_PI;
					}
					while (radians > PI) {
						radians -= TWO_PI;
					}
					return radians;
				case RadianNormalizationRange.PositiveNegativeFullRange:
					while (radians < -TWO_PI) {
						radians += TWO_PI;
					}
					while (radians > TWO_PI) {
						radians -= TWO_PI;
					}
					return radians;
			}

			return radians;
		}
		#endregion

		#region Clamp
		/// <summary>
		/// Clamps the <paramref name="input"/> value between <paramref name="min"/> and <paramref name="max"/> (inclusive).
		/// </summary>
		/// <param name="input">The input value.</param>
		/// <param name="min">The minimum permitted value.</param>
		/// <param name="max">The maximum permitted value.</param>
		/// <returns>The result of <c>input &lt; min ? min : input&gt; max ? max : input</c>.</returns>
		public static Number Clamp(Number input, Number min, Number max) {
			return input < min ? min : input > max ? max : input;
		}
		#endregion

		#region Bits etc
		/// <summary>
		/// Ascertains whether the given number is exactly a power-of-two (e.g. 1, 2, 4, 8, 16, etc.).
		/// </summary>
		/// <param name="number">The number to check. If <see cref="Number.IsIntegral"/> is <c>false</c>, this 
		/// function will check only the integral part of the number.</param>
		/// <returns>True if <paramref name="number"/> is a power of two, false if not.</returns>
		public static bool IsPowerOfTwo(Number number) {
			return (number & (number - 1)) == 0;
		}

		/// <summary>
		/// Gets the position of the most significant bit in the given <paramref name="number"/>. For example, if 
		/// <paramref name="number"/> is <c>57</c>, this method returns <c>6</c>.
		/// </summary>
		/// <param name="number">The number to check. If <see cref="Number.IsIntegral"/> is <c>false</c>, this 
		/// function will only use the integral part of the number.</param>
		/// <returns>The position of the highest-set-bit in the <paramref name="number"/>.</returns>
		public static Number MostSignificantBit(Number number) {
			uint result = 0;
			while (number > 0) {
				number >>= 1;
				++result;
			}

			return result;
		}
		#endregion
	}
}