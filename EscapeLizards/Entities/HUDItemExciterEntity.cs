// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 16 07 2015 at 01:07 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using Ophidian.Losgap;
using Ophidian.Losgap.Entities;
using Ophidian.Losgap.Rendering;

namespace Egodystonic.EscapeLizards {
	public sealed class HUDItemExciterEntity : Entity {
		struct ObjDetails : IEquatable<ObjDetails> {
			public readonly float SpawnTime;
			public readonly Vector2 Velocity;
			public readonly float InitialAlpha;
			public readonly IHUDObject HUDObject;

			public ObjDetails(float spawnTime, Vector2 velocity, float initialAlpha, IHUDObject obj) {
				SpawnTime = spawnTime;
				Velocity = velocity;
				InitialAlpha = initialAlpha;
				HUDObject = obj;
			}

			public bool Equals(ObjDetails other) {
				return Equals(HUDObject, other.HUDObject);
			}

			public override bool Equals(object obj) {
				if (ReferenceEquals(null, obj)) {
					return false;
				}
				return obj is ObjDetails && Equals((ObjDetails)obj);
			}

			public override int GetHashCode() {
				return (HUDObject != null ? HUDObject.GetHashCode() : 0);
			}

			public static bool operator ==(ObjDetails left, ObjDetails right) {
				return left.Equals(right);
			}

			public static bool operator !=(ObjDetails left, ObjDetails right) {
				return !left.Equals(right);
			}
		}

		private static readonly List<HUDItemExciterEntity> allExciters = new List<HUDItemExciterEntity>();
		private static readonly object staticMutationLock = new object();

		private IHUDObject targetObj;
		private Vector3? colorOverride;
		private float opacityMultiplier = 0.4f; 
		private float speed = 0.02f;
		private float lifetime = 2f;
		private float spawnInterval = 1f / 15f;
		private float timeSinceLastSpawn = 0f;
		private readonly Stack<IHUDObject> unusedObjects = new Stack<IHUDObject>();
		private readonly List<ObjDetails> activeElements = new List<ObjDetails>();
		private readonly List<ObjDetails> disposalWorkspace = new List<ObjDetails>();

		public Vector3? ColorOverride {
			get {
				lock (InstanceMutationLock) {
					return colorOverride;
				}
			}
			set {
				lock (InstanceMutationLock) {
					colorOverride = value;
				}
			}
		}

		public float OpacityMultiplier {
			get {
				lock (InstanceMutationLock) {
					return opacityMultiplier;
				}
			}
			set {
				lock (InstanceMutationLock) {
					opacityMultiplier = value;
				}
			}
		}

		public float Speed {
			get {
				lock (InstanceMutationLock) {
					return speed;
				}
			}
			set {
				lock (InstanceMutationLock) {
					speed = value;
				}
			}
		}

		public float Lifetime {
			get {
				lock (InstanceMutationLock) {
					return lifetime;
				}
			}
			set {
				lock (InstanceMutationLock) {
					lifetime = value;
				}
			}
		}

		public float CountPerSec {
			get {
				lock (InstanceMutationLock) {
					return 1f / spawnInterval;
				}
			}
			set {
				lock (InstanceMutationLock) {
					spawnInterval = 1f / value;
				}
			}
		}

		public IHUDObject TargetObj {
			get {
				lock (InstanceMutationLock) {
					return targetObj;
				}
			}
			set {
				lock (InstanceMutationLock) {
					targetObj = value;
				}
			}
		}

		public HUDItemExciterEntity(IHUDObject targetObj) {
			this.targetObj = targetObj;
			lock (staticMutationLock) {
				allExciters.Add(this);
			}
		}

		protected override void Tick(float deltaTimeSeconds) {
			base.Tick(deltaTimeSeconds);

			if (deltaTimeSeconds > 1f) return;

			float curTime = EntityModule.ElapsedTime;
			disposalWorkspace.Clear();
			foreach (var ele in activeElements) {
				float eleLife = curTime - ele.SpawnTime;
				if (eleLife >= Lifetime) {
					disposalWorkspace.Add(ele);
					continue;
				}

				ele.HUDObject.AnchorOffset += ele.Velocity * deltaTimeSeconds;
				ele.HUDObject.SetAlpha(ele.InitialAlpha * (1f - eleLife / lifetime));
			}
			foreach (var toBeDisposed in disposalWorkspace) {
				activeElements.Remove(toBeDisposed);
				unusedObjects.Push(toBeDisposed.HUDObject);
			}

			if (targetObj == null) return;

			timeSinceLastSpawn += deltaTimeSeconds;
			while (timeSinceLastSpawn >= spawnInterval) {
				float alpha = targetObj.Color.W * opacityMultiplier;

				IHUDObject newObj;
				if (unusedObjects.Count == 0) {
					newObj = targetObj.Clone();
				}
				else {
					newObj = unusedObjects.Pop();
					targetObj.CopyTo(newObj);
				}

				if (colorOverride != null) newObj.Color = new Vector4(colorOverride.Value, w: alpha);
				else newObj.SetAlpha(alpha);
				newObj.ZIndex = targetObj.ZIndex + 1;

				Vector2 velo = new Vector2(
					RandomProvider.Next(-1f, 1f),
					RandomProvider.Next(-1f, 1f)
				);
				if (velo == Vector2.ZERO) velo = Vector2.UP;
				velo = velo.WithLength(speed);

				activeElements.Add(new ObjDetails(curTime, velo, alpha, newObj));

				timeSinceLastSpawn -= spawnInterval;
			}
		}

		public void ClearAll() {
			lock (InstanceMutationLock) {
				foreach (var activeElement in activeElements) {
					activeElement.HUDObject.Dispose();
				}
				activeElements.Clear();

				foreach (var unusedObject in unusedObjects) {
					unusedObject.Dispose();
				}
				unusedObjects.Clear();
			}
		}

		public override void Dispose() {
			base.Dispose();
			ClearAll();
			lock (staticMutationLock) {
				allExciters.Remove(this);
			}
		}

		public static void ClearExciterMaterials() {
			lock (staticMutationLock) {
				foreach (var exciter in allExciters) {
					exciter.ClearAll();
				}
			}
		}
	}
}