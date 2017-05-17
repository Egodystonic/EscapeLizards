// All code copyright (c) 2017 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 17 01 2017 at 19:56 by Ben Bowen

using System;
using System.Collections.Generic;
using Ophidian.Losgap;
using Ophidian.Losgap.Entities;

namespace Egodystonic.EscapeLizards {
	public struct EusocialMovementTrack : IEquatable<EusocialMovementTrack> {
		public readonly GeometryEntity Entity;
		public readonly float StartTime;
		public readonly Vector3 StartPoint;
		public readonly Vector3 EndPoint;

		public EusocialMovementTrack(GeometryEntity entity, float startTime, Vector3 startPoint, Vector3 endPoint) {
			Entity = entity;
			StartTime = startTime;
			StartPoint = startPoint;
			EndPoint = endPoint;
		}

		public bool Equals(EusocialMovementTrack other) {
			return Equals(Entity, other.Entity);
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) {
				return false;
			}
			return obj is EusocialMovementTrack && Equals((EusocialMovementTrack)obj);
		}

		public override int GetHashCode() {
			return (Entity != null ? Entity.GetHashCode() : 0);
		}

		public static bool operator ==(EusocialMovementTrack left, EusocialMovementTrack right) {
			return left.Equals(right);
		}

		public static bool operator !=(EusocialMovementTrack left, EusocialMovementTrack right) {
			return !left.Equals(right);
		}
	}
}