// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 08 01 2015 at 15:04 by Ben Bowen

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// Represents a command that can be queued up and sent through the rendering pipeline.
	/// Each render command maps to exactly one action invoked on the GPU/Graphics API.
	/// </summary>
	/// <remarks>
	/// Use the static methods of this type to create RenderCommands.
	/// </remarks>
	[StructLayout(LayoutKind.Sequential, Pack = (int) InteropUtils.StructPacking.Safe)]
	public partial struct RenderCommand : IEquatable<RenderCommand> {
		internal readonly RenderCommandInstruction Instruction;
		internal readonly RenderCommandArgument Arg1;
		internal readonly RenderCommandArgument Arg2;
		internal readonly RenderCommandArgument Arg3;

		internal RenderCommand(RenderCommandInstruction instruction) : this() {
			this.Instruction = instruction;
		}

		internal RenderCommand(RenderCommandInstruction instruction, RenderCommandArgument arg1)
			: this() {
			this.Instruction = instruction;
			this.Arg1 = arg1;
		}

		internal RenderCommand(RenderCommandInstruction instruction, RenderCommandArgument arg1, RenderCommandArgument arg2) 
			: this(instruction, arg1, arg2, RenderCommandArgument.BLANK) { }

		internal RenderCommand(RenderCommandInstruction instruction, RenderCommandArgument arg1, RenderCommandArgument arg2, RenderCommandArgument arg3) {
			this.Instruction = instruction;
			this.Arg1 = arg1;
			this.Arg2 = arg2;
			this.Arg3 = arg3;
		}

		private static IntPtr AllocAndZeroTemp(uint numBytes) {
			IntPtr result = RenderCommandTempMemPool.GetLocalPool().Reserve(numBytes);
			UnsafeUtils.ZeroMem(result, numBytes);
			return result;
		}

		/// <summary>
		/// Returns a string detailing the command and its parameters.
		/// </summary>
		/// <returns>A string detailing the command and its parameters.</returns>
		public override string ToString() {
			return Instruction + "(" + Arg1 + ", " + Arg2 + ", " + Arg3 + ")";
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(RenderCommand other) {
			return Instruction == other.Instruction && Arg1.Equals(other.Arg1) && Arg2.Equals(other.Arg2) && Arg3.Equals(other.Arg3);
		}

		/// <summary>
		/// Indicates whether this instance and a specified object are equal.
		/// </summary>
		/// <returns>
		/// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false. 
		/// </returns>
		/// <param name="obj">The object to compare with the current instance. </param>
		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) {
				return false;
			}
			return obj is RenderCommand && Equals((RenderCommand)obj);
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>
		/// A 32-bit signed integer that is the hash code for this instance.
		/// </returns>
		public override int GetHashCode() {
			unchecked {
				int hashCode = Instruction.GetHashCode();
				hashCode = (hashCode * 397) ^ Arg1.GetHashCode();
				hashCode = (hashCode * 397) ^ Arg2.GetHashCode();
				hashCode = (hashCode * 397) ^ Arg3.GetHashCode();
				return hashCode;
			}
		}

		/// <summary>
		/// Ascertains whether or not the two commands invoke identical instructions (including arguments).
		/// </summary>
		/// <param name="left">The first command.</param>
		/// <param name="right">The second command.</param>
		/// <returns>True if the two commands are identical. False if not.</returns>
		public static bool operator ==(RenderCommand left, RenderCommand right) {
			return left.Equals(right);
		}

		/// <summary>
		/// Ascertains whether or not the two commands invoke different instructions, or the same instruction with different arguments.
		/// </summary>
		/// <param name="left">The first command.</param>
		/// <param name="right">The second command.</param>
		/// <returns>True if the two commands are different in any way, false if they are identical.</returns>
		public static bool operator !=(RenderCommand left, RenderCommand right) {
			return !left.Equals(right);
		}
	}
}