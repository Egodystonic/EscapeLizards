// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 10 09 2015 at 12:44 by Ben Bowen

using System;
using System.Collections.Generic;

namespace Ophidian.Losgap.Entities {
	public struct RayTestCollision : IEquatable<RayTestCollision> {
		public readonly Entity Entity;
		public readonly Vector3 HitPoint;

		public RayTestCollision(Entity entity, Vector3 hitPoint) {
			Entity = entity;
			HitPoint = hitPoint;
		}

		public bool Equals(RayTestCollision other) {
			return Equals(Entity, other.Entity) && HitPoint.Equals(other.HitPoint);
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) {
				return false;
			}
			return obj is RayTestCollision && Equals((RayTestCollision)obj);
		}

		public override int GetHashCode() {
			unchecked {
				return ((Entity != null ? Entity.GetHashCode() : 0) * 397) ^ HitPoint.GetHashCode();
			}
		}

		public static bool operator ==(RayTestCollision left, RayTestCollision right) {
			return left.Equals(right);
		}

		public static bool operator !=(RayTestCollision left, RayTestCollision right) {
			return !left.Equals(right);
		}
	}
}