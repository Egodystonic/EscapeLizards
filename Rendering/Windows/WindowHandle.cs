// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 17 10 2014 at 12:04 by Ben Bowen

using System;
using System.Linq;
using System.Runtime.InteropServices;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Rendering {
	[StructLayout(LayoutKind.Sequential, Pack = (int) InteropUtils.StructPacking.Safe)]
	internal struct WindowHandle : IEquatable<WindowHandle> {
		public static readonly WindowHandle NULL = new WindowHandle();
		private readonly IntPtr windowHandle;

		public bool Equals(WindowHandle other) {
			return windowHandle == other.windowHandle;
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) {
				return false;
			}
			return obj is WindowHandle && Equals((WindowHandle) obj);
		}

		public override int GetHashCode() {
			return windowHandle.GetHashCode();
		}

		public static bool operator ==(WindowHandle left, WindowHandle right) {
			return left.Equals(right);
		}

		public static bool operator !=(WindowHandle left, WindowHandle right) {
			return !left.Equals(right);
		}
	}
}