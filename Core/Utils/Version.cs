// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 06 10 2014 at 13:02 by Ben Bowen

using System;
using System.Linq;

namespace Ophidian.Losgap {
	/// <summary>
	/// Represents a 2-value version number in the format <c>Major.Minor</c>.
	/// </summary>
	public struct Version : IEquatable<Version> {
		/// <summary>
		/// The major version number.
		/// </summary>
		public readonly int MajorVersion;
		/// <summary>
		/// The minor version number.
		/// </summary>
		public readonly int MinorVersion;

		/// <summary>
		/// Constructs a new version number with the supplied major and minor components.
		/// </summary>
		/// <param name="majorVersion">The major version number.</param>
		/// <param name="minorVersion">The minor version number.</param>
		public Version(int majorVersion, int minorVersion) {
			MajorVersion = majorVersion;
			MinorVersion = minorVersion;
		}

		/// <summary>
		/// Constructs a new version by parsing the given string.
		/// </summary>
		/// <param name="version">The version string. Must be in the format <c>"X.Y"</c> where X and Y are valid 32-bit integers, and
		/// represent the major and minor version numbers respectively.</param>
		/// <exception cref="FormatException">Thrown if X or Y in <paramref name="version"/> do not represent a valid 32-bit integer.</exception>
		/// <exception cref="OverflowException">Thrown if X or Y in <paramref name="version"/> represent a value outside of the range of
		/// a 32-bit integer.</exception>
		public Version(string version) {
			if (version == null) throw new ArgumentNullException("version");
			
			string[] splitOnDot = version.Split('.');
			MajorVersion = Int32.Parse(splitOnDot[0]);
			MinorVersion = splitOnDot.Length < 2 ? 0 : Int32.Parse(splitOnDot[1]);
		}

		/// <summary>
		/// Gets the string representation of this Version.
		/// </summary>
		/// <returns>A string in the format <c>X.Y</c>, where X and Y represent <see cref="MajorVersion"/> and <see cref="MinorVersion"/>
		/// respectively.</returns>
		public override string ToString() {
			return MajorVersion + "." + MinorVersion;
		}


		#region Equality
		/// <summary>
		/// Checks to see if this Version is equal to <paramref name="other"/>.
		/// </summary>
		/// <param name="other">The other Version.</param>
		/// <returns>True if both Versions' <see cref="MajorVersion"/> and <see cref="MinorVersion"/> are equal.</returns>
		public bool Equals(Version other) {
			return MajorVersion == other.MajorVersion && MinorVersion == other.MinorVersion;
		}

		/// <summary>
		/// Checks to see if <paramref name="obj"/> is another <see cref="Version"/> and that it is equal to this one.
		/// </summary>
		/// <param name="obj">The comparison object.</param>
		/// <returns>True if <paramref name="obj"/> is a Version and 
		/// both Versions' <see cref="MajorVersion"/> and <see cref="MinorVersion"/> are equal</returns>
		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) {
				return false;
			}
			return obj is Version && Equals((Version)obj);
		}

		/// <summary>
		/// Gets the unique hash code for this version.
		/// </summary>
		/// <returns>A hash code calculated on the <see cref="MajorVersion"/> and <see cref="MinorVersion"/>.</returns>
		public override int GetHashCode() {
			unchecked {
				return (MajorVersion * 397) ^ MinorVersion;
			}
		}

		/// <summary>
		/// Checks if <paramref name="lhs"/> is equal to <paramref name="rhs"/>.
		/// </summary>
		/// <param name="lhs">The first operand.</param>
		/// <param name="rhs">The second operand.</param>
		/// <returns>True if both operands' <see cref="MajorVersion"/> and <see cref="MinorVersion"/> match.</returns>
		public static bool operator ==(Version lhs, Version rhs) {
			return lhs.Equals(rhs);
		}

		/// <summary>
		/// Checks if <paramref name="lhs"/> is not equal to <paramref name="rhs"/>.
		/// </summary>
		/// <param name="lhs">The first operand.</param>
		/// <param name="rhs">The second operand.</param>
		/// <returns>True if either operands' <see cref="MajorVersion"/> or <see cref="MinorVersion"/>s are different.</returns>
		public static bool operator !=(Version lhs, Version rhs) {
			return !lhs.Equals(rhs);
		}
		#endregion

		#region Comparisons
		/// <summary>
		/// Checks whether <paramref name="lhs"/> represents a more recent version than <paramref name="rhs"/>.
		/// </summary>
		/// <param name="lhs">The first operand.</param>
		/// <param name="rhs">The second operand.</param>
		/// <returns>True if <paramref name="lhs"/>'s <see cref="MajorVersion"/> is greater than <paramref name="rhs"/>'s; or if
		/// <paramref name="lhs"/>'s <see cref="MinorVersion"/> is greater than <paramref name="rhs"/>'s and their <see cref="MajorVersion"/>s
		/// are the same.</returns>
		public static bool operator >(Version lhs, Version rhs) {
			return lhs.MajorVersion != rhs.MajorVersion ?
				lhs.MajorVersion > rhs.MajorVersion :
				lhs.MinorVersion > rhs.MinorVersion;
		}
		/// <summary>
		/// Checks whether <paramref name="lhs"/> represents a less recent version than <paramref name="rhs"/>.
		/// </summary>
		/// <param name="lhs">The first operand.</param>
		/// <param name="rhs">The second operand.</param>
		/// <returns>True if <paramref name="lhs"/>'s <see cref="MajorVersion"/> is less than <paramref name="rhs"/>'s; or if
		/// <paramref name="lhs"/>'s <see cref="MinorVersion"/> is less than <paramref name="rhs"/>'s and their <see cref="MajorVersion"/>s
		/// are the same.</returns>
		public static bool operator <(Version lhs, Version rhs) {
			return lhs.MajorVersion != rhs.MajorVersion ?
				lhs.MajorVersion < rhs.MajorVersion :
				lhs.MinorVersion < rhs.MinorVersion;
		}
		/// <summary>
		/// Checks whether <paramref name="lhs"/> represents version that is at least as recent as <paramref name="rhs"/>.
		/// </summary>
		/// <param name="lhs">The first operand.</param>
		/// <param name="rhs">The second operand.</param>
		/// <returns>True if <paramref name="lhs"/>'s <see cref="MajorVersion"/> is greater than or equal to <paramref name="rhs"/>'s; or if
		/// <paramref name="lhs"/>'s <see cref="MinorVersion"/> is greater than or equal to <paramref name="rhs"/>'s and
		/// their <see cref="MajorVersion"/>s are the same.</returns>
		public static bool operator >=(Version lhs, Version rhs) {
			return lhs.MajorVersion != rhs.MajorVersion ?
				lhs.MajorVersion >= rhs.MajorVersion :
				lhs.MinorVersion >= rhs.MinorVersion;
		}
		/// <summary>
		/// Checks whether <paramref name="lhs"/> represents version that is no more recent than <paramref name="rhs"/>.
		/// </summary>
		/// <param name="lhs">The first operand.</param>
		/// <param name="rhs">The second operand.</param>
		/// <returns>True if <paramref name="lhs"/>'s <see cref="MajorVersion"/> is less than or equal to <paramref name="rhs"/>'s; or if
		/// <paramref name="lhs"/>'s <see cref="MinorVersion"/> is less than or equal to <paramref name="rhs"/>'s and
		/// their <see cref="MajorVersion"/>s are the same.</returns>
		public static bool operator <=(Version lhs, Version rhs) {
			return lhs.MajorVersion != rhs.MajorVersion ?
				lhs.MajorVersion <= rhs.MajorVersion :
				lhs.MinorVersion <= rhs.MinorVersion;
		}
		#endregion

	}
}