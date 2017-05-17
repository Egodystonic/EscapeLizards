// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 31 08 2015 at 12:42 by Ben Bowen

using System;
using System.Collections.Generic;
using Ophidian.Losgap;

namespace Egodystonic.EscapeLizards {
	public struct GameboardTiltData : IEquatable<GameboardTiltData> {
		public readonly GameboardTiltDirection TiltDir;
		public readonly Quaternion TargetRot;
		public readonly float TiltAmount;

		public GameboardTiltData(GameboardTiltDirection tiltDir, Quaternion targetRot, float tiltAmount) {
			TiltDir = tiltDir;
			TargetRot = targetRot;
			TiltAmount = tiltAmount;
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(GameboardTiltData other) {
			return TiltDir == other.TiltDir && TargetRot.Equals(other.TargetRot) && TiltAmount == other.TiltAmount;
		}

		/// <summary>
		/// Indicates whether this instance and a specified object are equal.
		/// </summary>
		/// <returns>
		/// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false. 
		/// </returns>
		/// <param name="obj">The object to compare with the current instance. </param>
		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) {
				return false;
			}
			return obj is GameboardTiltData && Equals((GameboardTiltData)obj);
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>
		/// A 32-bit signed integer that is the hash code for this instance.
		/// </returns>
		public override int GetHashCode() {
			unchecked {
				return ((int)TiltDir * 397) ^ TargetRot.GetHashCode() ^ TiltAmount.GetHashCode();
			}
		}

		public static bool operator ==(GameboardTiltData left, GameboardTiltData right) {
			return left.Equals(right);
		}

		public static bool operator !=(GameboardTiltData left, GameboardTiltData right) {
			return !left.Equals(right);
		}
	}
}