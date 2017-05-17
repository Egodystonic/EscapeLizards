// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 17 10 2014 at 12:04 by Ben Bowen

using System;
using System.Linq;
using System.Runtime.InteropServices;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Rendering {
	[StructLayout(LayoutKind.Sequential, Pack = (int) InteropUtils.StructPacking.Safe)]
	internal struct DeviceContextHandle : IEquatable<DeviceContextHandle> {
		public static readonly DeviceContextHandle NULL = new DeviceContextHandle();
		private readonly IntPtr deviceContextPtr;

		public bool Equals(DeviceContextHandle other) {
			return deviceContextPtr == other.deviceContextPtr;
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) {
				return false;
			}
			return obj is DeviceContextHandle && Equals((DeviceContextHandle)obj);
		}

		public override int GetHashCode() {
			return deviceContextPtr.GetHashCode();
		}

		public static bool operator ==(DeviceContextHandle left, DeviceContextHandle right) {
			return left.Equals(right);
		}

		public static bool operator !=(DeviceContextHandle left, DeviceContextHandle right) {
			return !left.Equals(right);
		}
	}
}