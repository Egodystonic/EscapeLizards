// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 22 12 2014 at 13:32 by Ben Bowen

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Rendering {
	[StructLayout(LayoutKind.Sequential, Pack = (int) InteropUtils.StructPacking.Safe)]
	internal struct ShaderHandle : IEquatable<ShaderHandle> {
		public static readonly ShaderHandle NULL = new ShaderHandle();
		private readonly IntPtr shaderPtr;

		public bool Equals(ShaderHandle other) {
			return shaderPtr == other.shaderPtr;
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) {
				return false;
			}
			return obj is ShaderHandle && Equals((ShaderHandle) obj);
		}

		public override int GetHashCode() {
			return shaderPtr.GetHashCode();
		}

		public static bool operator ==(ShaderHandle left, ShaderHandle right) {
			return left.Equals(right);
		}

		public static bool operator !=(ShaderHandle left, ShaderHandle right) {
			return !left.Equals(right);
		}

		public static explicit operator IntPtr(ShaderHandle operand) {
			return operand.shaderPtr;
		}
	}
}