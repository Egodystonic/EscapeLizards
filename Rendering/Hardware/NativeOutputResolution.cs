// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 16 10 2014 at 17:16 by Ben Bowen

using System;
using System.Linq;
using System.Runtime.InteropServices;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// Represents a display resolution supported natively by an <see cref="OutputDisplay"/>.
	/// Using a natively supported resolution when setting <see cref="Window"/> dimensions can sometimes offer a performance
	/// improvement.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = (int) InteropUtils.StructPacking.Safe)]
	public struct NativeOutputResolution : IEquatable<NativeOutputResolution> {
		internal readonly int Index;

		/// <summary>
		/// The width (in pixels) of this resolution.
		/// </summary>
		public readonly uint ResolutionWidth;
		/// <summary>
		/// The height (in pixels) of this resolution.
		/// </summary>
		public readonly uint ResolutionHeight;

		private readonly uint refreshRateNumerator;
		private readonly uint refreshRateDenominator;

		/// <summary>
		/// The refresh rate that the <see cref="OutputDisplay"/> will run at when this output resolution is selected.
		/// </summary>
		public uint RefreshRateHz {
			get {
				return refreshRateNumerator / refreshRateDenominator;
			}
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(NativeOutputResolution other) {
			return ResolutionWidth == other.ResolutionWidth 
				&& refreshRateNumerator == other.refreshRateNumerator 
				&& ResolutionHeight == other.ResolutionHeight 
				&& refreshRateDenominator == other.refreshRateDenominator;
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
			return obj is NativeOutputResolution && Equals((NativeOutputResolution)obj);
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>
		/// A 32-bit signed integer that is the hash code for this instance.
		/// </returns>
		/// <filterpriority>2</filterpriority>
		public override int GetHashCode() {
			unchecked {
				int hashCode = (int)ResolutionWidth;
				hashCode = (hashCode * 397) ^ (int)refreshRateNumerator;
				hashCode = (hashCode * 397) ^ (int)ResolutionHeight;
				hashCode = (hashCode * 397) ^ (int)refreshRateDenominator;
				return hashCode;
			}
		}

		/// <summary>
		/// Returns a string containing the resolution <see cref="ResolutionWidth"/>, <see cref="ResolutionHeight"/>, and
		/// <see cref="RefreshRateHz"/> of this object.
		/// </summary>
		/// <returns>A string in the format "XxY @ RHz"</returns>
		public override string ToString() {
			return ResolutionWidth + "x" + ResolutionHeight + " @ " + RefreshRateHz + "Hz";
		}

		/// <summary>
		/// Indicates whether the two <see cref="NativeOutputResolution"/>s are identical.
		/// </summary>
		/// <param name="left">The first operand.</param>
		/// <param name="right">The second operand.</param>
		/// <returns>True if both resolutions represent the same width, height, and refresh rate.</returns>
		public static bool operator ==(NativeOutputResolution left, NativeOutputResolution right) {
			return left.Equals(right);
		}

		/// <summary>
		/// Indicates whether the two <see cref="NativeOutputResolution"/>s are dissimilar.
		/// </summary>
		/// <param name="left">The first operand.</param>
		/// <param name="right">The second operand.</param>
		/// <returns>True if both resolutions represent different widths, heights, or refresh rates.</returns>
		public static bool operator !=(NativeOutputResolution left, NativeOutputResolution right) {
			return !left.Equals(right);
		}
	}
}