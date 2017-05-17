// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 22 01 2015 at 18:05 by Ben Bowen

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Rendering {
	[StructLayout(LayoutKind.Sequential, Pack = (int) InteropUtils.StructPacking.Safe)]
	internal unsafe struct RenderCommandArgument : IEquatable<RenderCommandArgument> {
		public static readonly RenderCommandArgument BLANK = new RenderCommandArgument();
		// ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
		private readonly ulong dataAsULong;

		private RenderCommandArgument(ulong dataAsULong) {
			this.dataAsULong = dataAsULong;
		}

		public override string ToString() {
			return dataAsULong.ToString();
		}

		public static implicit operator RenderCommandArgument(IntPtr operand) {
			RenderCommandArgument result = new RenderCommandArgument();
			*((IntPtr*) &result) = operand;
			return result;
		}

		public static implicit operator RenderCommandArgument(SByte operand) {
			RenderCommandArgument result = new RenderCommandArgument();
			*((SByte*) &result) = operand;
			return result;
		}
		
		public static implicit operator RenderCommandArgument(Byte operand) {
			return new RenderCommandArgument(operand);
		}
		
		public static implicit operator RenderCommandArgument(Int16 operand) {
			RenderCommandArgument result = new RenderCommandArgument();
			*((Int16*) &result) = operand;
			return result;
		}
		
		public static implicit operator RenderCommandArgument(UInt16 operand) {
			return new RenderCommandArgument(operand);
		}
		
		public static implicit operator RenderCommandArgument(Int32 operand) {
			RenderCommandArgument result = new RenderCommandArgument();
			*((Int32*) &result) = operand;
			return result;
		}
		
		public static implicit operator RenderCommandArgument(UInt32 operand) {
			return new RenderCommandArgument(operand);
		}
		
		public static implicit operator RenderCommandArgument(Int64 operand) {
			RenderCommandArgument result = new RenderCommandArgument();
			*((Int64*) &result) = operand;
			return result;
		}
		
		public static implicit operator RenderCommandArgument(UInt64 operand) {
			return new RenderCommandArgument(operand);
		}
	
		public static implicit operator RenderCommandArgument(Single operand) {
			RenderCommandArgument result = new RenderCommandArgument();
			*((Single*) &result) = operand;
			return result;
		}
		
		public static implicit operator RenderCommandArgument(Double operand) {
			return new RenderCommandArgument(*((ulong*) &operand));
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(RenderCommandArgument other) {
			return dataAsULong == other.dataAsULong;
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
			return obj is RenderCommandArgument && Equals((RenderCommandArgument)obj);
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>
		/// A 32-bit signed integer that is the hash code for this instance.
		/// </returns>
		public override int GetHashCode() {
			return dataAsULong.GetHashCode();
		}

		public static bool operator ==(RenderCommandArgument left, RenderCommandArgument right) {
			return left.Equals(right);
		}

		public static bool operator !=(RenderCommandArgument left, RenderCommandArgument right) {
			return !left.Equals(right);
		}
	}
}