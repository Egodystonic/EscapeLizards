// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 17 10 2014 at 12:04 by Ben Bowen

using System;
using System.Linq;
using System.Runtime.InteropServices;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Rendering {
	[StructLayout(LayoutKind.Sequential, Pack = (int) InteropUtils.StructPacking.Safe)]
	internal struct DeviceHandle : IEquatable<DeviceHandle> {
		public static readonly DeviceHandle NULL = new DeviceHandle();
		private readonly IntPtr devicePtr;

		public bool Equals(DeviceHandle other) {
			return devicePtr == other.devicePtr;
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) {
				return false;
			}
			return obj is DeviceHandle && Equals((DeviceHandle)obj);
		}

		public override int GetHashCode() {
			return devicePtr.GetHashCode();
		}

		public static bool operator ==(DeviceHandle left, DeviceHandle right) {
			return left.Equals(right);
		}

		public static bool operator !=(DeviceHandle left, DeviceHandle right) {
			return !left.Equals(right);
		}
	}
}