// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 21 07 2015 at 15:03 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Linq;
using Ophidian.Losgap;
using Ophidian.Losgap.Entities;
using Ophidian.Losgap.Rendering;

namespace Egodystonic.EscapeLizards {
	public sealed class VultureEgg : LevelGameObject {
		public override ModelHandle Model {
			get { return AssetLocator.VultureEggModel; }
		}

		public override Material Material {
			get { return AssetLocator.VultureEggMaterial; }
		}

		public override PhysicsShapeHandle PhysicsShape {
			get { return AssetLocator.VultureEggPhysicsShape; }
		}

		public override bool EditorOnly {
			get { return false; }
		}

		public override bool IgnorePhysicsInEditor {
			get { return true; }
		}

		public VultureEgg(LevelDescription parentLevel, int id) : base(parentLevel, id) { }

		private VultureEgg(LevelDescription parentLevel, LevelGeometryEntity groundingGeometryEntity, Transform transform, bool useSimpleGrounding, int id) : base(parentLevel, groundingGeometryEntity, transform, useSimpleGrounding, id) { }

		public override string ToString() {
			return "Vulture Egg";
		}

		public override LevelGameObject Clone(int id) {
			return new VultureEgg(ParentLevel, GroundingGeometryEntity, Transform, UseSimpleGrounding, id);
		}

		public override string LongDescription {
			get {
				return "Vulture Egg" + CreateCommonDescString();
			}
		}

		protected override void AddSerializationData(XElement element) {
			// Nothing extra
		}

		protected override void PerformCustomDeserialization(XElement element) {
			// Nothing extra
		}

		protected override GroundableEntity CreateEntityObject(out float mass) {
			mass = GameplayConstants.VULTURE_EGG_MASS;
			return new VultureEggEntity(Transform);
		}

		public override GroundableEntity CreateEntity() {
			GroundableEntity result;
			GeometryEntity groundingEntity;
			float mass;
			lock (instanceMutationLock) {
				result = CreateEntityObject(out mass);
				if (EntryPoint.InEditor) mass = 0f;
				groundingEntity = GroundingGeometryEntity != null ?
					ParentLevel.GetEntityRepresentation(GroundingGeometryEntity) :
					null;
			}

			if (!EditorOnly || EntryPoint.InEditor) {
				if (PhysicsShape != PhysicsShapeHandle.NULL) {
					result.SetPhysicsShape(
						PhysicsShape,
						Vector3.ZERO,
						mass,
						disablePerformanceDeactivation: true,
						restitution: GameplayConstants.EGG_RESTITUTION,
						linearDamping: GameplayConstants.EGG_DAMPING_LINEAR,
						angularDamping: GameplayConstants.EGG_DAMPING_ANGULAR,
						friction: GameplayConstants.EGG_FRICTION,
						rollingFriction: GameplayConstants.EGG_FRICTION_ROLLING
					);
					result.EnableContinuousCollisionDetection(GameplayConstants.MIN_SPEED_FOR_EGG_CCD, GameplayConstants.EGG_COLLISION_RADIUS * 0.01f);	
				}
				result.SetModelInstance(AssetLocator.GameLayer, Model, Material);
			}

			if (groundingEntity != null) result.Ground(groundingEntity);
			return result;
		}
	}
}