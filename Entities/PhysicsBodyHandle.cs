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
	internal struct PhysicsBodyHandle : IEquatable<PhysicsBodyHandle>, IDisposable {
		public static readonly PhysicsBodyHandle NULL = new PhysicsBodyHandle();
		private readonly IntPtr resourcePtr;

		public bool Equals(PhysicsBodyHandle other) {
			return resourcePtr == other.resourcePtr;
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) {
				return false;
			}
			return obj is PhysicsBodyHandle && Equals((PhysicsBodyHandle) obj);
		}

		public override int GetHashCode() {
			return resourcePtr.GetHashCode();
		}

		public void Dispose() {
			Assure.NotEqual(this, NULL);
			PhysicsManager.DestroyRigidBody(this);
		}

		public static bool operator ==(PhysicsBodyHandle left, PhysicsBodyHandle right) {
			return left.Equals(right);
		}

		public static bool operator !=(PhysicsBodyHandle left, PhysicsBodyHandle right) {
			return !left.Equals(right);
		}
	}
}