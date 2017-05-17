// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 14 10 2014 at 11:37 by Ben Bowen

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap {
	/// <summary>
	/// Represents a number of bytes (e.g. the size of a file, or amount of memory allocated, etc.).
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = (int) InteropUtils.StructPacking.Safe, Size = sizeof(long))]
	public struct ByteSize : IEquatable<ByteSize>, IComparable<ByteSize> {
		/// <summary>
		/// The number of bytes in a Kibibyte (commonly referred to erroneously as a Kilobyte).
		/// </summary>
		public const long BYTES_IN_A_KIBIBYTE = 1024L;
		/// <summary>
		/// The number of bytes in a Mebibyte (commonly referred to erroneously as a Megabyte).
		/// </summary>
		public const long BYTES_IN_A_MEBIBYTE = BYTES_IN_A_KIBIBYTE * 1024L;
		/// <summary>
		/// The number of bytes in a Gibibyte (commonly referred to erroneously as a Gigabyte).
		/// </summary>
		public const long BYTES_IN_A_GIBIBYTE = BYTES_IN_A_MEBIBYTE * 1024L;
		/// <summary>
		/// The number of bytes in a Tebibyte (commonly referred to erroneously as a Terabyte).
		/// </summary>
		public const long BYTES_IN_A_TEBIBYTE = BYTES_IN_A_GIBIBYTE * 1024L;
		private const string TO_STRING_FORMAT = "{0:n3}";

		/// <summary>
		/// Whether or not to use the IEC unit names (Kibi-, Mebi-, Gibi-, and Tebibyte) when converting a ByteSize object to a string.
		/// If false, the more user-friendly (but technically incorrect) terms will be used (e.g. Kilo-, Mega-, Giga-, and Terabyte).
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "InTo",
			Justification = "InTo are two separate words.")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible",
			Justification = "There are no thread safety issues: The field is volatile and atomic, and does not synchronize with any " +
			"other state.")]
		public static volatile bool UseIECUnitsInToString = true;

		/// <summary>
		/// The value of this ByteSize in bytes.
		/// </summary>
		public readonly long InBytes;

		/// <summary>
		/// The number of KiB that this ByteSize represents.
		/// </summary>
		/// <seealso cref="BYTES_IN_A_KIBIBYTE"/>
		public double InKibibytes {
			get {
				return (double) InBytes / BYTES_IN_A_KIBIBYTE;
			}
		}
		/// <summary>
		/// The number of MiB that this ByteSize represents.
		/// </summary>
		/// <seealso cref="BYTES_IN_A_MEBIBYTE"/>
		public double InMebibytes {
			get {
				return (double) InBytes / BYTES_IN_A_MEBIBYTE;
			}
		}
		/// <summary>
		/// The number of GiB that this ByteSize represents.
		/// </summary>
		/// <seealso cref="BYTES_IN_A_GIBIBYTE"/>
		public double InGibibytes {
			get {
				return (double) InBytes / BYTES_IN_A_GIBIBYTE;
			}
		}
		/// <summary>
		/// The number of TiB that this ByteSize represents.
		/// </summary>
		/// <seealso cref="BYTES_IN_A_TEBIBYTE"/>
		public double InTebibytes {
			get {
				return (double) InBytes / BYTES_IN_A_TEBIBYTE;
			}
		}

		/// <summary>
		/// Creates a new ByteSize with the specified number of bytes.
		/// </summary>
		/// <param name="numBytes">The number of bytes that this ByteSize represents. May be negative.</param>
		public ByteSize(long numBytes) {
			InBytes = numBytes;
		}

		/// <summary>
		/// Creates a new ByteSize with the chosen inputs. The resultant value for <see cref="InBytes"/> will be the sum of the provided
		/// parameters.
		/// </summary>
		/// <param name="numBytes">A number of bytes to add.</param>
		/// <param name="numKibibytes">A number of KiB to add.</param>
		/// <param name="numMebibytes">A number of MiB to add.</param>
		/// <param name="numGibibytes">A number of GiB to add.</param>
		/// <param name="numTebibytes">A number of TiB to add.</param>
		public ByteSize(
			long numBytes = 0L,
			double numKibibytes = 0.0d, 
			double numMebibytes = 0.0d, 
			double numGibibytes = 0.0d, 
			double numTebibytes = 0.0d
		) {
			InBytes = (long) (
				(numTebibytes * BYTES_IN_A_TEBIBYTE)
				+ (numGibibytes * BYTES_IN_A_GIBIBYTE)
				+ (numMebibytes * BYTES_IN_A_MEBIBYTE)
				+ (numKibibytes * BYTES_IN_A_KIBIBYTE))
				+ numBytes;
		}

		/// <summary>
		/// Returns a cleanly-formatted string representation of the value this ByteSize represents.
		/// </summary>
		/// <returns>
		/// "X Y" where X is a value in units 'Y', and Y is the largest sized byte-unit that this ByteSize represents at least
		/// one of.
		/// </returns>
		public override string ToString() {
			if (InBytes >= BYTES_IN_A_TEBIBYTE) {
				return String.Format(TO_STRING_FORMAT, InTebibytes) + (UseIECUnitsInToString ? " TiB" : " TB");
			}
			else if (InBytes >= BYTES_IN_A_GIBIBYTE) {
				return String.Format(TO_STRING_FORMAT, InGibibytes) + (UseIECUnitsInToString ? " GiB" : " GB");
			}
			else if (InBytes >= BYTES_IN_A_MEBIBYTE) {
				return String.Format(TO_STRING_FORMAT, InMebibytes) + (UseIECUnitsInToString ? " MiB" : " MB");
			}
			else if (InBytes >= BYTES_IN_A_KIBIBYTE) {
				return String.Format(TO_STRING_FORMAT, InKibibytes) + (UseIECUnitsInToString ? " KiB" : " KB");
			}
			else {
				return InBytes + " B";
			}
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(ByteSize other) {
			return InBytes == other.InBytes;
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
			return obj is ByteSize && Equals((ByteSize)obj);
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>
		/// A 32-bit signed integer that is the hash code for this instance.
		/// </returns>
		/// <filterpriority>2</filterpriority>
		public override int GetHashCode() {
			return InBytes.GetHashCode();
		}

		/// <summary>
		/// Compares the current object with another object of the same type.
		/// </summary>
		/// <returns>
		/// A value that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other"/> parameter.Zero This object is equal to <paramref name="other"/>. Greater than zero This object is greater than <paramref name="other"/>. 
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public int CompareTo(ByteSize other) {
			return InBytes.CompareTo(other.InBytes);
		}

		/// <summary>
		/// Determines whether the two ByteSizes are the same.
		/// </summary>
		/// <param name="left">The first operand.</param>
		/// <param name="right">The second operand.</param>
		/// <returns>True if both operands have the same value for <see cref="InBytes"/>.</returns>
		public static bool operator ==(ByteSize left, ByteSize right) {
			return left.Equals(right);
		}

		/// <summary>
		/// Determines whether the two ByteSizes are not the same.
		/// </summary>
		/// <param name="left">The first operand.</param>
		/// <param name="right">The second operand.</param>
		/// <returns>True if both operands have different values for <see cref="InBytes"/>.</returns>
		public static bool operator !=(ByteSize left, ByteSize right) {
			return !left.Equals(right);
		}

		/// <summary>
		/// Returns a new ByteSize that is the two operands' sizes summed together.
		/// </summary>
		/// <param name="lhs">The first operand.</param>
		/// <param name="rhs">The second operand.</param>
		/// <returns>The equivalent to <c>new ByteSize(lhs.InBytes + rhs.InBytes)</c>.</returns>
		public static ByteSize operator +(ByteSize lhs, ByteSize rhs) {
			return new ByteSize(lhs.InBytes + rhs.InBytes);
		}
		/// <summary>
		/// Returns a new ByteSize that is the difference of the two operands' sizes.
		/// </summary>
		/// <param name="lhs">The first operand.</param>
		/// <param name="rhs">The second operand.</param>
		/// <returns>The equivalent to <c>new ByteSize(lhs.InBytes - rhs.InBytes)</c>.</returns>
		public static ByteSize operator -(ByteSize lhs, ByteSize rhs) {
			return new ByteSize(lhs.InBytes - rhs.InBytes);
		}
		/// <summary>
		/// Ascertains whether the first operand represents a larger size than the second.
		/// </summary>
		/// <param name="lhs">The first operand.</param>
		/// <param name="rhs">The second operand.</param>
		/// <returns>The equivalent to <c>lhs.InBytes &gt; rhs.InBytes</c>.</returns>
		public static bool operator >(ByteSize lhs, ByteSize rhs) {
			return lhs.InBytes > rhs.InBytes;
		}
		/// <summary>
		/// Ascertains whether the first operand represents a smaller size than the second.
		/// </summary>
		/// <param name="lhs">The first operand.</param>
		/// <param name="rhs">The second operand.</param>
		/// <returns>The equivalent to <c>lhs.InBytes &lt; rhs.InBytes</c>.</returns>
		public static bool operator <(ByteSize lhs, ByteSize rhs) {
			return lhs.InBytes < rhs.InBytes;
		}
		/// <summary>
		/// Ascertains whether the first operand represents a larger or equal size compared to the second.
		/// </summary>
		/// <param name="lhs">The first operand.</param>
		/// <param name="rhs">The second operand.</param>
		/// <returns>The equivalent to <c>lhs.InBytes &gt;= rhs.InBytes</c>.</returns>
		public static bool operator >=(ByteSize lhs, ByteSize rhs) {
			return lhs.InBytes >= rhs.InBytes;
		}
		/// <summary>
		/// Ascertains whether the first operand represents a smaller or equal size compared to the second.
		/// </summary>
		/// <param name="lhs">The first operand.</param>
		/// <param name="rhs">The second operand.</param>
		/// <returns>The equivalent to <c>lhs.InBytes &lt;= rhs.InBytes</c>.</returns>
		public static bool operator <=(ByteSize lhs, ByteSize rhs) {
			return lhs.InBytes <= rhs.InBytes;
		}
		/// <summary>
		/// Returns a new ByteSize that is the size of the first operand multiplied by the second.
		/// </summary>
		/// <param name="lhs">The original size.</param>
		/// <param name="rhs">The multiplicative factor.</param>
		/// <returns>The equivalent to <c>new ByteSize(lhs.InBytes * rhs)</c>.</returns>
		public static ByteSize operator *(ByteSize lhs, long rhs) {
			return new ByteSize(lhs.InBytes * rhs);
		}
		/// <summary>
		/// Returns a new ByteSize that is the size of the second operand multiplied by the first.
		/// </summary>
		/// <param name="lhs">The multiplicative factor.</param>
		/// <param name="rhs">The original size.</param>
		/// <returns>The equivalent to <c>new ByteSize(lhs * rhs.InBytes)</c>.</returns>
		public static ByteSize operator *(long lhs, ByteSize rhs) {
			return new ByteSize(lhs * rhs.InBytes);
		}
		/// <summary>
		/// Returns a new ByteSize that is the size of the first operand divided by the second.
		/// </summary>
		/// <param name="lhs">The original size.</param>
		/// <param name="rhs">The divisive factor.</param>
		/// <returns>The equivalent to <c>new ByteSize(lhs.InBytes / rhs)</c>.</returns>
		public static ByteSize operator /(ByteSize lhs, long rhs) {
			return new ByteSize(lhs.InBytes / rhs);
		}
		
		/// <summary>
		/// Returns the byte size of the supplied operand as a long.
		/// </summary>
		/// <param name="operand">The ByteSize.</param>
		/// <returns>The equivalent to <c>operand.InBytes</c>.</returns>
		public static implicit operator long(ByteSize operand) {
			return operand.InBytes;
		}

		/// <summary>
		/// Returns a ByteSize that is the equivalent to the number of bytes supplied as the operand.
		/// </summary>
		/// <param name="operand">The number of bytes.</param>
		/// <returns>The equivalent to <c>new ByteSize(operand)</c>.</returns>
		public static implicit operator ByteSize(long operand) {
			return new ByteSize(operand);
		}
	}
}