// All code copyright (c) 2016 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 15 01 2016 at 16:13 by Ben Bowen

using System;
using System.Collections.Generic;

namespace Egodystonic.EscapeLizards {
	public struct LevelID : IEquatable<LevelID> {
		public readonly byte WorldIndex;
		public readonly byte LevelIndex;

		public LevelID? NextLevelInWorld {
			get {
				LevelID theoreticalNextID = new LevelID(WorldIndex, (byte) (LevelIndex + 1));
				return LevelDatabase.LevelExists(theoreticalNextID) ? theoreticalNextID : (LevelID?) null;
			}
		}

		public LevelID(byte worldIndex, byte levelIndex) {
			WorldIndex = worldIndex;
			LevelIndex = levelIndex;
		}

		public override string ToString() {
			return (WorldIndex + 1) + ":" + (LevelIndex + 1);
		}

		public static LevelID Parse(string s) {
			var split = s.Split(':');
			return new LevelID((byte) (Byte.Parse(split[0]) - 1), (byte) (Byte.Parse(split[1]) - 1));
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(LevelID other) {
			return WorldIndex == other.WorldIndex && LevelIndex == other.LevelIndex;
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
			return obj is LevelID && Equals((LevelID)obj);
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>
		/// A 32-bit signed integer that is the hash code for this instance.
		/// </returns>
		public override int GetHashCode() {
			unchecked {
				return (WorldIndex.GetHashCode() * 397) ^ LevelIndex.GetHashCode();
			}
		}

		public static bool operator ==(LevelID left, LevelID right) {
			return left.Equals(right);
		}

		public static bool operator !=(LevelID left, LevelID right) {
			return !left.Equals(right);
		}
	}
}