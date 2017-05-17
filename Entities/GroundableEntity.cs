// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 15 07 2015 at 18:26 by Ben Bowen

using System;
using System.Linq;
using System.Security;

namespace Ophidian.Losgap.Entities {
	public abstract class GroundableEntity : Entity {
		private Transform initialTransform;
		private GeometryEntity groundingEntity;
		private Transform groundingInitialTransform;
		protected Vector3 groundingToThisConstraint;
		private bool useSimpleGrounding = false;

		public Transform InitialTransform {
			get {
				lock (InstanceMutationLock) {
					return initialTransform;
				}
			}
		}

		public Transform GroundingInitialTransform {
			get {
				lock (InstanceMutationLock) {
					return groundingInitialTransform;
				}
			}
		}

		public GeometryEntity GroundingEntity {
			get {
				lock (InstanceMutationLock) {
					return groundingEntity;
				}
			}
		}

		public bool IsGrounded {
			get {
				lock (InstanceMutationLock) {
					return groundingEntity != null && !groundingEntity.IsDisposed;
				}
			}
		}

		public bool UseSimpleGrounding {
			get {
				lock (InstanceMutationLock) {
					return useSimpleGrounding;
				}
			}
			set {
				lock (InstanceMutationLock) {
					useSimpleGrounding = value;
				}
			}
		}

		protected GroundableEntity(Transform initialTransform) {
			Transform = this.initialTransform = initialTransform;
		}

		public virtual void Ground(GeometryEntity e) {
			Assure.NotNull(e);
			Assure.False(e.IsDisposed);
			lock (InstanceMutationLock) {
				Assure.False(IsDisposed);
				initialTransform = transform;
				groundingEntity = e;
				groundingInitialTransform = (e is PresetMovementEntity ? ((PresetMovementEntity) e).MovementSteps[0].Transform : e.Transform);
				groundingToThisConstraint = initialTransform.Translation - groundingInitialTransform.Translation;
			}
			SetGravity(Vector3.ZERO);
		}

		public virtual void Unground(float mass, Vector3 gravity) {
			lock (InstanceMutationLock) {
				groundingEntity = null;
				SetMass(mass);
			}
			SetGravity(gravity);
		}

		public virtual void MoveGroundedMassless(Vector3 translation, Quaternion rot) {
			lock (InstanceMutationLock) {
				if (!IsGrounded || !HasPhysics) return;
				initialTransform = initialTransform.RotateBy(rot).With(translation: translation);
				groundingInitialTransform = (groundingEntity is PresetMovementEntity ? ((PresetMovementEntity) groundingEntity).MovementSteps[0].Transform : groundingEntity.Transform);
				groundingToThisConstraint = initialTransform.Translation - groundingInitialTransform.Translation;
			}
		}

		protected override void Tick(float deltaTimeSeconds) {
			base.Tick(deltaTimeSeconds);
			RealignGrounding();
		}

		public virtual void RealignGrounding() {
			lock (InstanceMutationLock) {
				if (groundingEntity == null || groundingEntity.IsDisposed) return;
				Transform groundingCurTransform = groundingEntity.Transform;
				//if (useSimpleGrounding) {
				//	SetTranslation(groundingCurTransform.Translation + groundingToThisConstraint);
				//}
				//else {
				Quaternion rotOffset = (groundingCurTransform.Rotation * -groundingInitialTransform.Rotation).ToUnit();
				if (useSimpleGrounding) rotOffset *= initialTransform.Rotation;
				// commented out two lines that were making the binding stuff weird...
				//Vector3 scaleDiff = groundingCurTransform.Scale.Scale(1f / groundingInitialTransform.Scale);
				Transform = new Transform(
					Transform.Scale,
					initialTransform.Rotation * rotOffset,
					//groundingCurTransform.Translation + (groundingToThisConstraint * rotOffset).WithLength(groundingToThisConstraint.Length * scaleDiff.ProjectedOnto(groundingToThisConstraint).Length)
					groundingCurTransform.Translation + (groundingToThisConstraint * rotOffset)
				);
				//}
			}
			if (HasPhysics) {
				Velocity = Vector3.ZERO;
				AngularVelocity = Vector3.ZERO;
			}
		}
	}
}