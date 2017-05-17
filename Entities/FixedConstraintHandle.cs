// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 26 06 2015 at 16:21 by Ben Bowen

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Ophidian.Losgap.Interop;
using Ophidian.Losgap.Rendering;

namespace Ophidian.Losgap.Entities {
	[StructLayout(LayoutKind.Sequential, Pack = (int) InteropUtils.StructPacking.Safe)]
	public struct FixedConstraintHandle : IEquatable<FixedConstraintHandle>, IDisposable {
		public static readonly FixedConstraintHandle NULL = new FixedConstraintHandle();
		private readonly IntPtr resourcePtr;

		public bool Equals(FixedConstraintHandle other) {
			return resourcePtr == other.resourcePtr;
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) {
				return false;
			}
			return obj is FixedConstraintHandle && Equals((FixedConstraintHandle) obj);
		}

		public override int GetHashCode() {
			return resourcePtr.GetHashCode();
		}

		public void Dispose() {
			PhysicsManager.DestroyConstraint(this);
		}

		public static bool operator ==(FixedConstraintHandle left, FixedConstraintHandle right) {
			return left.Equals(right);
		}

		public static bool operator !=(FixedConstraintHandle left, FixedConstraintHandle right) {
			return !left.Equals(right);
		}
	}
}