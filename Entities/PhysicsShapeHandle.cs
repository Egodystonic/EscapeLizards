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
	public struct PhysicsShapeHandle : IEquatable<PhysicsShapeHandle>, IDisposable {
		public static readonly PhysicsShapeHandle NULL = new PhysicsShapeHandle();
		private readonly IntPtr resourcePtr;

		public bool Equals(PhysicsShapeHandle other) {
			return resourcePtr == other.resourcePtr;
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) {
				return false;
			}
			return obj is PhysicsShapeHandle && Equals((PhysicsShapeHandle) obj);
		}

		public override int GetHashCode() {
			return resourcePtr.GetHashCode();
		}

		public void Dispose() {
			PhysicsManager.DestroyShape(this);
		}

		public static bool operator ==(PhysicsShapeHandle left, PhysicsShapeHandle right) {
			return left.Equals(right);
		}

		public static bool operator !=(PhysicsShapeHandle left, PhysicsShapeHandle right) {
			return !left.Equals(right);
		}
	}
}