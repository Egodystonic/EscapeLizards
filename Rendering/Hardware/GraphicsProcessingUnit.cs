// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 16 10 2014 at 17:14 by Ben Bowen

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// Represents a GPU (graphics adapter, video card) installed on this system.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = (int) InteropUtils.StructPacking.Safe)]
	public unsafe struct GraphicsProcessingUnit : IEquatable<GraphicsProcessingUnit> {
		internal readonly int Index;
		/// <summary>
		/// The vendor-ascribed name of the GPU.
		/// </summary>
		public readonly InteropStringPtr Name;

		/// <summary>
		/// The amount of dedicated VRAM installed in the card that is available for this application to use.
		/// </summary>
		public readonly ByteSize DedicatedVideoMemory;
		/// <summary>
		/// The amount of dedicated system RAM that the operating system has appropriated for this application (and only this application)
		/// to use as surrogate VRAM, if the dedicated video memory is used up.
		/// </summary>
		public readonly ByteSize DedicatedSystemMemory;
		/// <summary>
		/// The amount of system RAM that the operating system will permit this application to use as surrogate VRAM, 
		/// if the dedicated video memory is used up, and enough system RAM is free.
		/// </summary>
		public readonly ByteSize SharedSystemMemory;

		private readonly int numOutputs;
		private readonly OutputDisplay* outputArrPtr;

		/// <summary>
		/// An enumeration of <see cref="OutputDisplay"/>s that are connected to this GPU.
		/// </summary>
		public IEnumerable<OutputDisplay> Outputs {
			get {
				OutputDisplay[] result = new OutputDisplay[numOutputs];
				for (int i = 0; i < numOutputs; ++i) {
					result[i] = *(outputArrPtr + i);
				}
				return result;
			}
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(GraphicsProcessingUnit other) {
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
			return obj is GraphicsProcessingUnit && Equals((GraphicsProcessingUnit)obj);
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
		/// Checks whether or not the two GPUs refer to the same input slot on this system. This operator only checks the index
		/// value for equality.
		/// </summary>
		/// <param name="left">The first operand.</param>
		/// <param name="right">The second operand.</param>
		/// <returns>True if both operands refer to the same hardware input slot.</returns>
		public static bool operator ==(GraphicsProcessingUnit left, GraphicsProcessingUnit right) {
			return left.Equals(right);
		}

		/// <summary>
		/// Checks whether or not the two GPUs refer to the different input slots on this system. This operator only checks the index
		/// value for equality.
		/// </summary>
		/// <param name="left">The first operand.</param>
		/// <param name="right">The second operand.</param>
		/// <returns>True if both operands refer to different hardware input slots.</returns>
		public static bool operator !=(GraphicsProcessingUnit left, GraphicsProcessingUnit right) {
			return !left.Equals(right);
		}

		/// <summary>
		/// Returns a string containing the input slot index and vendor name of this GPU.
		/// </summary>
		/// <returns>A string in the format "[n]:Name".</returns>
		public override string ToString() {
			return "[" + Index + "]: " + Name + "; DVM = " + DedicatedVideoMemory + ", DSM = " + DedicatedSystemMemory + ", SSM = " + SharedSystemMemory + "; " + Outputs.Count() + " outputs";
		}
	}
}