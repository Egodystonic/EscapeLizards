// All code copyright (c) 2016 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 28 10 2016 at 16:40 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using Ophidian.Losgap;
using Ophidian.Losgap.Entities;

namespace Egodystonic.EscapeLizards {
	public static class CollisionBitPool {
		private static readonly object staticMutationLock = new object();
		private static readonly Stack<CollisionBitEntity> freeBits = new Stack<CollisionBitEntity>();
		private static readonly Dictionary<CollisionBitEntity, float> usedBits = new Dictionary<CollisionBitEntity, float>();
		private static readonly List<CollisionBitEntity> usedBitsRemovalWorkspace = new List<CollisionBitEntity>();
		private const float LIFETIME = 5f;
		private const float MIN_SPEED = PhysicsManager.ONE_METRE_SCALED * 1f;
		private const int MAX_BITS_PER_PHYS_LEVEL = 7;

		private static int maxBits {
			get {
				return (int) Config.PhysicsLevel * MAX_BITS_PER_PHYS_LEVEL;
			}
		}

		private static CollisionBitEntity Pop() {
			if (freeBits.Count == 0) {
				return new CollisionBitEntity();
			}
			else {
				var result = freeBits.Pop();
				result.Reveal();
				return result;
			}
		}

		private static void Push(CollisionBitEntity unused) {
			freeBits.Push(unused);
			unused.Hide();
		}

		public static void DisseminateBits(Vector3 position, Vector3 velocity, int count) {
			if (velocity == Vector3.ZERO) return;
			velocity = velocity.WithLength(Math.Max(MIN_SPEED, velocity.Length));
			Quaternion veloToPerpRot = Quaternion.FromVectorTransition(velocity, velocity.AnyPerpendicular());
			Quaternion veloRot = Quaternion.FromAxialRotation(velocity, MathUtils.TWO_PI);
			lock (staticMutationLock) {
				for (int i = 0; i < count; ++i) {
					var bit = Pop();
					bit.SetTranslation(position);
					bit.Velocity = velocity * veloToPerpRot.Subrotation(RandomProvider.Next(0.1f, 0.5f)) * veloRot.Subrotation(RandomProvider.Next(0f, 1f));
					usedBits.Add(bit, EntityModule.ElapsedTime);
				}

				while (usedBits.Count > maxBits) {
					KeyValuePair<CollisionBitEntity, float> leastRecentKVP = new KeyValuePair<CollisionBitEntity, float>(null, Single.MaxValue);
					foreach (var usedBit in usedBits) {
						if (usedBit.Value < leastRecentKVP.Value) leastRecentKVP = usedBit;
					}

					if (leastRecentKVP.Key != null) {
						usedBits.Remove(leastRecentKVP.Key);
						Push(leastRecentKVP.Key);
					}
				}
			}
		}

		public static void Tick(float deltaTime) {
			lock (staticMutationLock) {
				usedBitsRemovalWorkspace.Clear();

				foreach (var kvp in usedBits) {
					if (EntityModule.ElapsedTime - kvp.Value > LIFETIME) usedBitsRemovalWorkspace.Add(kvp.Key);
				}

				for (int i = 0; i < usedBitsRemovalWorkspace.Count; ++i) {
					var bit = usedBitsRemovalWorkspace[i];
					usedBits.Remove(bit);
					Push(bit);
				}
			}
		}

		public static void ClearAll() {
			lock (staticMutationLock) {
				foreach (var kvp in usedBits) {
					var bit = kvp.Key;
					Push(bit);
					bit.SetScale(Vector3.ZERO);
					bit.SetGravity(Vector3.ZERO);
					bit.RemoveAllForceAndTorque();
				}

				usedBits.Clear();
			}
		}
	}
}