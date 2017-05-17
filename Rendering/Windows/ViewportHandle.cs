// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 17 10 2014 at 12:04 by Ben Bowen

using System;
using System.Linq;
using System.Runtime.InteropServices;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Rendering {
	[StructLayout(LayoutKind.Sequential, Pack = (int) InteropUtils.StructPacking.Safe)]
	internal struct ViewportHandle : IEquatable<ViewportHandle> {
		public static readonly ViewportHandle NULL = new ViewportHandle();
		private readonly IntPtr viewportPtr;

		public bool Equals(ViewportHandle other) {
			return viewportPtr == other.viewportPtr;
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) {
				return false;
			}
			return obj is ViewportHandle && Equals((ViewportHandle) obj);
		}

		public override int GetHashCode() {
			return viewportPtr.GetHashCode();
		}

		public static bool operator ==(ViewportHandle left, ViewportHandle right) {
			return left.Equals(right);
		}

		public static bool operator !=(ViewportHandle left, ViewportHandle right) {
			return !left.Equals(right);
		}

		public static explicit operator IntPtr(ViewportHandle operand) {
			return operand.viewportPtr;
		}
	}
}