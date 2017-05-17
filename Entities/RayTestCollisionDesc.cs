// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 10 09 2015 at 12:38 by Ben Bowen

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Entities {
	[StructLayout(LayoutKind.Sequential, Pack = (int) InteropUtils.StructPacking.Safe)]
	internal struct RayTestCollisionDesc : IEquatable<RayTestCollisionDesc> {
		public readonly Vector4 Position;
		public readonly PhysicsBodyHandle BodyHandle;

		public bool Equals(RayTestCollisionDesc other) {
			return BodyHandle.Equals(other.BodyHandle) && Position.Equals(other.Position);
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) {
				return false;
			}
			return obj is RayTestCollisionDesc && Equals((RayTestCollisionDesc)obj);
		}

		public override int GetHashCode() {
			unchecked {
				return (BodyHandle.GetHashCode() * 397) ^ Position.GetHashCode();
			}
		}

		public static bool operator ==(RayTestCollisionDesc left, RayTestCollisionDesc right) {
			return left.Equals(right);
		}

		public static bool operator !=(RayTestCollisionDesc left, RayTestCollisionDesc right) {
			return !left.Equals(right);
		}
	}
}