// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 19 03 2015 at 18:10 by Ben Bowen

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Ophidian.Losgap.Interop;
using Ophidian.Losgap.Rendering;

namespace Ophidian.Losgap.Entities {
	public abstract unsafe class Entity : IDisposable {
		private const long TRANSFORM_ALIGNMENT = 16L;
		protected readonly object InstanceMutationLock = new object();
		private bool isDisposed = false;
		private ModelInstanceHandle? modelInstance = null;
		protected AlignedAllocation<Transform> transform = new AlignedAllocation<Transform>(TRANSFORM_ALIGNMENT, (uint) sizeof(Transform));
		private PhysicsBodyHandle physicsBody = PhysicsBodyHandle.NULL;
		private AlignedAllocation<Vector4>? physicsShapeOffset = null;
		private List<Entity> currentCollisionCallbackTouchesA;
		private List<Entity> currentCollisionCallbackTouchesB;
		private bool fillingCollisionCallbackListA = true;
		private event Action<Entity, Entity> collisionDetected;

		public event Action<Entity, Entity> CollisionDetected {
			add {
				lock (InstanceMutationLock) {
					Assure.False(IsDisposed);
					Assure.NotEqual(PhysicsBody, PhysicsBodyHandle.NULL);
					if (collisionDetected == null) {
						EntityModule.AddCollisionCallbackReportingForEntity(this);
						currentCollisionCallbackTouchesA = new List<Entity>();
						currentCollisionCallbackTouchesB = new List<Entity>();
					}
					collisionDetected += value;
				}
			}
			remove {
				lock (InstanceMutationLock) {
					Assure.False(IsDisposed);
					Assure.NotEqual(PhysicsBody, PhysicsBodyHandle.NULL);
					collisionDetected -= value;
					if (collisionDetected == null) EntityModule.RemoveCollisionCallbackReportingForEntity(this);
				}
			}
		}

		public bool IsDisposed {
			get {
				lock (InstanceMutationLock) {
					return isDisposed;
				}
			}
		}

		public Transform Transform {
			get {
				lock (InstanceMutationLock) {
					Assure.False(isDisposed);
					return transform;
				}
			}
			set {
				lock (InstanceMutationLock) {
					Assure.False(isDisposed);
					transform.Write(value);
					TransformChanged();
				}
			}
		}

		public Vector3 Velocity {
			get {
				PhysicsBodyHandle pbhLocal;
				lock (InstanceMutationLock) pbhLocal = physicsBody;
				if (pbhLocal == PhysicsBodyHandle.NULL) {
					throw new InvalidOperationException("Must set physics shape before using physics-based entity members.");
				}
				return PhysicsManager.GetBodyLinearVelocity(pbhLocal);
			}
			set {
				PhysicsBodyHandle pbhLocal;
				lock (InstanceMutationLock) pbhLocal = physicsBody;
				if (pbhLocal == PhysicsBodyHandle.NULL) {
					throw new InvalidOperationException("Must set physics shape before using physics-based entity members.");
				}
				PhysicsManager.SetBodyLinearVelocity(pbhLocal, value);
			}
		}

		public Vector3 AngularVelocity {
			get {
				PhysicsBodyHandle pbhLocal;
				lock (InstanceMutationLock) pbhLocal = physicsBody;
				if (pbhLocal == PhysicsBodyHandle.NULL) {
					throw new InvalidOperationException("Must set physics shape before using physics-based entity members.");
				}
				return PhysicsManager.GetBodyAngularVelocity(pbhLocal);
			}
			set {
				PhysicsBodyHandle pbhLocal;
				lock (InstanceMutationLock) pbhLocal = physicsBody;
				if (pbhLocal == PhysicsBodyHandle.NULL) {
					throw new InvalidOperationException("Must set physics shape before using physics-based entity members.");
				}
				PhysicsManager.SetBodyAngularVelocity(pbhLocal, value);
			}
		}

		public bool HasPhysics {
			get {
				lock (InstanceMutationLock) {
					return physicsBody != PhysicsBodyHandle.NULL;
				}
			}
		}

		internal PhysicsBodyHandle PhysicsBody {
			get {
				lock (InstanceMutationLock) {
					return physicsBody;
				}
			}
		}

		protected ModelInstanceHandle? ModelInstance {
			get {
				lock (InstanceMutationLock) {
					return modelInstance;
				}
			}
		}

		protected Entity() {
			EntityModule.AddActiveEntity(this);
			transform.Write(Transform.DEFAULT_TRANSFORM);
		}

		internal void TouchDetected(Entity other) {
			if (fillingCollisionCallbackListA) currentCollisionCallbackTouchesA.Add(other);
			else currentCollisionCallbackTouchesB.Add(other);
		}

		internal void EnterLock() {
			Monitor.Enter(InstanceMutationLock);
		}
		internal void ExitLock() {
			Monitor.Exit(InstanceMutationLock);
		}

		public void ScaleBy(float scaleSize) {
			lock (InstanceMutationLock) {
				transform.Write(transform.Read().ScaleBy(scaleSize));
				TransformChanged();
			}
		}

		public void ScaleBy(Vector3 perAxisScaleSize) {
			lock (InstanceMutationLock) {
				transform.Write(transform.Read().ScaleBy(perAxisScaleSize));
				TransformChanged();
			}
		}

		public void RotateBy(float radians, Vector3 axis) {
			lock (InstanceMutationLock) {
				transform.Write(transform.Read().RotateBy(radians, axis));
				TransformChanged();
			}
		}

		public void RotateBy(Quaternion rotation) {
			lock (InstanceMutationLock) {
				transform.Write(transform.Read().RotateBy(rotation));
				TransformChanged();
			}
		}

		public void RotateAround(Vector3 pivotPoint, Quaternion rotation) {
			lock (InstanceMutationLock) {
				transform.Write(transform.Read().RotateAround(pivotPoint, rotation));
				TransformChanged();
			}
		}

		public void TranslateBy(Vector3 translation) {
			lock (InstanceMutationLock) {
				transform.Write(transform.Read().TranslateBy(translation));
				TransformChanged();
			}
		}

		public void SetScale(Vector3 scale) {
			lock (InstanceMutationLock) {
				transform.Write(transform.Read().With(scale: scale));
				TransformChanged();
			}
		}

		public void SetRotation(Quaternion rotation) {
			lock (InstanceMutationLock) {
				transform.Write(transform.Read().With(rotation: rotation));
				TransformChanged();
			}
		}

		public void SetTranslation(Vector3 translation) {
			lock (InstanceMutationLock) {
				transform.Write(transform.Read().With(translation: translation));
				TransformChanged();
			}
		}

		public void SetModelInstance(SceneLayer layer, ModelHandle modelHandle, Material material) {
			lock (InstanceMutationLock) {
				Assure.False(isDisposed);
				if (modelInstance != null) modelInstance.Value.Dispose();
				modelInstance = layer.CreateModelInstance(modelHandle, material, transform);
			}
		}

		public void DisposeModelInstance() {
			lock (InstanceMutationLock) {
				if (modelInstance != null) modelInstance.Value.Dispose();
			}
		}

		public void SetPhysicsShape(
			PhysicsShapeHandle shapeHandle, 
			Vector3 physicsShapeOffset,
			float mass,
			bool forceIntransigence = false,
			bool disablePerformanceDeactivation = false,
			bool collideOnlyWithWorld = false,
			bool collideWithOnlyDynamics = false,
			float restitution = PhysicsManager.DEFAULT_RESTITUTION,
			float linearDamping = PhysicsManager.DEFAULT_LINEAR_DAMPING,
			float angularDamping = PhysicsManager.DEFAULT_ANGULAR_DAMPING,
			float friction = PhysicsManager.DEFAULT_FRICTION,
			float rollingFriction = PhysicsManager.DEFAULT_ROLLING_FRICTION) {
				LosgapSystem.InvokeOnMaster(() => { // Anti-deadlock measures x.x
				lock (InstanceMutationLock) {
					if (physicsBody != PhysicsBodyHandle.NULL) physicsBody.Dispose();
					if (this.physicsShapeOffset == null) this.physicsShapeOffset = new AlignedAllocation<Vector4>(TRANSFORM_ALIGNMENT, (uint) sizeof(Vector4));
					this.physicsShapeOffset.Value.Write(physicsShapeOffset);
					physicsBody = PhysicsManager.CreateRigidBody(
						shapeHandle,
						mass,
						disablePerformanceDeactivation, 
						forceIntransigence,
						collideOnlyWithWorld,
						collideWithOnlyDynamics,
						transform.AlignedPointer + 32,
						transform.AlignedPointer + 16,
						this.physicsShapeOffset.Value.AlignedPointer
						);
				}
			});
			SetPhysicsProperties(restitution, linearDamping, angularDamping, friction, rollingFriction);
		}

		public void SetPhysicsProperties(
			float restitution = PhysicsManager.DEFAULT_RESTITUTION,
			float linearDamping = PhysicsManager.DEFAULT_LINEAR_DAMPING,
			float angularDamping = PhysicsManager.DEFAULT_ANGULAR_DAMPING,
			float friction = PhysicsManager.DEFAULT_FRICTION,
			float rollingFriction = PhysicsManager.DEFAULT_ROLLING_FRICTION) {
			PhysicsBodyHandle physicsBodyLocal;
			lock (InstanceMutationLock) {
				if (physicsBody == PhysicsBodyHandle.NULL) {
					throw new InvalidOperationException("Must set physics shape before using physics-based entity members.");
				}
				physicsBodyLocal = physicsBody;
			}
			PhysicsManager.SetBodyProperties(physicsBodyLocal, restitution, linearDamping, angularDamping, friction, rollingFriction);
		}

		public void SetMass(float newMass) {
			PhysicsBodyHandle physicsBodyLocal;
			lock (InstanceMutationLock) {
				if (physicsBody == PhysicsBodyHandle.NULL) return;
				physicsBodyLocal = physicsBody;
			}
			PhysicsManager.SetBodyMass(physicsBodyLocal, newMass);
		}

		public void SetGravity(Vector3 gravity) {
			PhysicsBodyHandle physicsBodyLocal;
			lock (InstanceMutationLock) {
				if (physicsBody == PhysicsBodyHandle.NULL) return;
				physicsBodyLocal = physicsBody;
			}
			PhysicsManager.SetBodyGravity(physicsBodyLocal, gravity);
		}

		public void EnableContinuousCollisionDetection(float minSpeedForCCD, float ccdProjectionRadius) {
			PhysicsBodyHandle physicsBodyLocal;
			lock (InstanceMutationLock) {
				if (physicsBody == PhysicsBodyHandle.NULL) return;
				physicsBodyLocal = physicsBody;
			}
			PhysicsManager.SetBodyCCD(physicsBodyLocal, minSpeedForCCD, ccdProjectionRadius);
		}

		public void AddForce(Vector3 force) {
			PhysicsBodyHandle physicsBodyLocal;
			lock (InstanceMutationLock) {
				if (physicsBody == PhysicsBodyHandle.NULL) return;
				physicsBodyLocal = physicsBody;
			}
			PhysicsManager.AddForceToBody(physicsBodyLocal, force);
		}

		public void SubtractForce(Vector3 force) {
			AddForce(-force);
		}

		public void AddImpulse(Vector3 impulse) {
			PhysicsBodyHandle physicsBodyLocal;
			lock (InstanceMutationLock) {
				if (physicsBody == PhysicsBodyHandle.NULL) return;
				physicsBodyLocal = physicsBody;
			}
			PhysicsManager.AddForceImpulseToBody(physicsBodyLocal, impulse);
		}

		public void AddTorque(Vector3 torque) {
			PhysicsBodyHandle physicsBodyLocal;
			lock (InstanceMutationLock) {
				if (physicsBody == PhysicsBodyHandle.NULL) return;
				physicsBodyLocal = physicsBody;
			}
			PhysicsManager.AddTorqueToBody(physicsBodyLocal, torque);
		}

		public void SubtractTorque(Vector3 torque) {
			AddTorque(-torque);
		}

		public void AddTorqueImpulse(Vector3 impulse) {
			PhysicsBodyHandle physicsBodyLocal;
			lock (InstanceMutationLock) {
				if (physicsBody == PhysicsBodyHandle.NULL) return;
				physicsBodyLocal = physicsBody;
			}
			PhysicsManager.AddTorqueImpulseToBody(physicsBodyLocal, impulse);
		}

		public void RemoveAllForceAndTorque() {
			PhysicsBodyHandle physicsBodyLocal;
			lock (InstanceMutationLock) {
				if (physicsBody == PhysicsBodyHandle.NULL) return;
				physicsBodyLocal = physicsBody;
			}
			PhysicsManager.RemoveAllForceAndTorqueFromBody(physicsBodyLocal);
		}

		public void ReactivateBody() {
			PhysicsBodyHandle physicsBodyLocal;
			lock (InstanceMutationLock) {
				if (physicsBody == PhysicsBodyHandle.NULL) return;
				physicsBodyLocal = physicsBody;
			}
			PhysicsManager.ReactivateBody(physicsBodyLocal);
		}

		internal void SynchronizedTick(float deltaTimeSeconds) {
			lock (InstanceMutationLock) {
				if (isDisposed) return;
				if (modelInstance != null) {
					ModelInstanceHandle mih = modelInstance.Value;
					mih.Transform = transform;
				}
				if (collisionDetected != null) {
					List<Entity> current, previous;
					if (fillingCollisionCallbackListA) {
						current = currentCollisionCallbackTouchesA;
						previous = currentCollisionCallbackTouchesB;
					}
					else {
						current = currentCollisionCallbackTouchesB;
						previous = currentCollisionCallbackTouchesA;
					}

					foreach (Entity other in current) {
						if (!previous.Contains(other)) collisionDetected(this, other);
					}
					previous.Clear();
					fillingCollisionCallbackListA = !fillingCollisionCallbackListA;
				}
				if (isDisposed) return;
				Tick(deltaTimeSeconds);
			}
		}
		protected virtual void Tick(float deltaTimeSeconds) { }

		protected virtual void TransformChanged() {
			if (physicsBody != PhysicsBodyHandle.NULL) PhysicsManager.UpdateBodyTransform(physicsBody);
		}

		public void DisposePhysicsBody() {
			if (physicsBody != PhysicsBodyHandle.NULL) {
				if (collisionDetected != null) EntityModule.RemoveCollisionCallbackReportingForEntity(this);
				LosgapSystem.InvokeOnMasterAsync(() => {
					physicsBody.Dispose();
					physicsBody = PhysicsBodyHandle.NULL;
				});
			}
		}

		public virtual void Dispose() {
			lock (InstanceMutationLock) {
				if (isDisposed) return;
				isDisposed = true;
				DisposePhysicsBody();
				EntityModule.RemoveActiveEntity(this);
				if (modelInstance != null) modelInstance.Value.Dispose();
				modelInstance = null;
				transform.Dispose();
				if (physicsShapeOffset != null) physicsShapeOffset.Value.Dispose();
			}
		}
	}
}