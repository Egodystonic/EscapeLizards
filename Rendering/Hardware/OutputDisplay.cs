// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 16 10 2014 at 17:18 by Ben Bowen

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// Represents a display (screen, monitor, output) connected to a <see cref="GraphicsProcessingUnit"/>.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = (int) InteropUtils.StructPacking.Safe)]
	public unsafe struct OutputDisplay : IEquatable<OutputDisplay> {
		internal readonly int Index;

		/// <summary>
		/// The name of the output, according to the operating system.
		/// </summary>
		public readonly InteropStringPtr Name;
		/// <summary>
		/// Whether or not this output is the primary desktop display.
		/// </summary>
		public readonly InteropBool IsPrimaryOutput;

		private readonly int numNativeResolutions;
		private readonly NativeOutputResolution* nativeResolutionArrPtr;

		/// <summary>
		/// An numeration of <see cref="NativeOutputResolution"/> structures that is the list of all natively supported resolutions
		/// that this output provides.
		/// </summary>
		public IEnumerable<NativeOutputResolution> NativeResolutions {
			get {
				NativeOutputResolution[] result = new NativeOutputResolution[numNativeResolutions];
				for (int i = 0; i < numNativeResolutions; ++i) {
					result[i] = *(nativeResolutionArrPtr + i);
				}
				return result;
			}
		}

		public NativeOutputResolution HighestResolution {
			get {
				return NativeResolutions.OrderByDescending(res => res.ResolutionWidth * res.ResolutionHeight).First();
			}
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(OutputDisplay other) {
			return Index == other.Index;
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
			return obj is OutputDisplay && Equals((OutputDisplay)obj);
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>
		/// A 32-bit signed integer that is the hash code for this instance.
		/// </returns>
		/// <filterpriority>2</filterpriority>
		public override int GetHashCode() {
			return Index;
		}

		/// <summary>
		/// Indicates whether both operands both refer to the same slot index on their parent <see cref="GraphicsProcessingUnit"/>s.
		/// </summary>
		/// <param name="left">The first operand.</param>
		/// <param name="right">The second operand.</param>
		/// <returns>True if both operands have the same input slot index. Does not regard any other fields.</returns>
		public static bool operator ==(OutputDisplay left, OutputDisplay right) {
			return left.Equals(right);
		}

		/// <summary>
		/// Indicates whether both operands both refer to the different slot indices on their parent <see cref="GraphicsProcessingUnit"/>s.
		/// </summary>
		/// <param name="left">The first operand.</param>
		/// <param name="right">The second operand.</param>
		/// <returns>True if both operands have different input slot indices. Does not regard any other fields.</returns>
		public static bool operator !=(OutputDisplay left, OutputDisplay right) {
			return !left.Equals(right);
		}

		/// <summary>
		/// Returns a string containing the GPU output slot index and OS name of this output.
		/// </summary>
		/// <returns>A string in the format "[n]:Name".</returns>
		public override string ToString() {
			return "[" + Index + "]: " + Name;
		}
	}
}