// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 17 10 2014 at 16:02 by Ben Bowen

using System;
using System.Linq;
using System.Runtime.InteropServices;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Rendering {
	[StructLayout(LayoutKind.Sequential, Pack = (int) InteropUtils.StructPacking.Safe)]
	internal struct SwapChainHandle : IEquatable<SwapChainHandle> {
		public static readonly SwapChainHandle NULL = new SwapChainHandle();
		private readonly IntPtr swapChainPtr;

		public bool Equals(SwapChainHandle other) {
			return swapChainPtr == other.swapChainPtr;
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) {
				return false;
			}
			return obj is SwapChainHandle && Equals((SwapChainHandle)obj);
		}

		public override int GetHashCode() {
			return swapChainPtr.GetHashCode();
		}

		public static bool operator ==(SwapChainHandle left, SwapChainHandle right) {
			return left.Equals(right);
		}

		public static bool operator !=(SwapChainHandle left, SwapChainHandle right) {
			return !left.Equals(right);
		}

		public static explicit operator IntPtr(SwapChainHandle operand) {
			return operand.swapChainPtr;
		}
	}
}