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
	public sealed class LizardCoin : LevelGameObject {
		public override ModelHandle Model {
			get { return AssetLocator.LizardCoinModel; }
		}

		public override Material Material {
			get { return AssetLocator.LizardCoinMaterial; }
		}

		public override PhysicsShapeHandle PhysicsShape {
			get { return AssetLocator.LizardCoinShape; }
		}

		public override bool EditorOnly {
			get { return false; }
		}

		public override bool IgnorePhysicsInEditor {
			get { return true; }
		}

		public override bool DynamicsCollisionOnly {
			get { return true; }
		}

		public LizardCoin(LevelDescription parentLevel, int id) : base(parentLevel, id) { }

		private LizardCoin(LevelDescription parentLevel, LevelGeometryEntity groundingGeometryEntity, Transform transform, bool useSimpleGrounding, int id) : base(parentLevel, groundingGeometryEntity, transform, useSimpleGrounding, id) { }

		public override string ToString() {
			return "Lizard Coin";
		}

		public override LevelGameObject Clone(int id) {
			return new LizardCoin(ParentLevel, GroundingGeometryEntity, Transform, UseSimpleGrounding, id);
		}

		public override string LongDescription {
			get {
				return "Lizard Coin" + CreateCommonDescString();
			}
		}

		protected override void AddSerializationData(XElement element) {
			// Nothing extra
		}

		protected override void PerformCustomDeserialization(XElement element) {
			// Nothing extra
		}

		protected override GroundableEntity CreateEntityObject(out float mass) {
			mass = GameplayConstants.EGG_MASS * 0.00001f;
			return new LizardCoinEntity(Transform);
		}
	}
}