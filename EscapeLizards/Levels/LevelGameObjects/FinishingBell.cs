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
	public sealed class FinishingBell : LevelGameObject {
		public override ModelHandle Model {
			get { return AssetLocator.FinishingBellModel; }
		}

		public override Material Material {
			get { return AssetLocator.FinishingBellMaterial; }
		}

		public override PhysicsShapeHandle PhysicsShape {
			get { return AssetLocator.FinishingBellPhysicsShape; }
		}

		public override bool EditorOnly {
			get { return false; }
		}

		public override bool IgnorePhysicsInEditor {
			get { return true; }
		}

		public FinishingBell(LevelDescription parentLevel, int id) : base(parentLevel, id) { }

		private FinishingBell(LevelDescription parentLevel, LevelGeometryEntity groundingGeometryEntity, Transform transform, bool useSimpleGrounding, int id) : base(parentLevel, groundingGeometryEntity, transform, useSimpleGrounding, id) { }

		public override string ToString() {
			return "Finishing Bell";
		}

		public override LevelGameObject Clone(int id) {
			return new FinishingBell(ParentLevel, GroundingGeometryEntity, Transform, UseSimpleGrounding, id);
		}

		public override string LongDescription {
			get {
				return "Finishing Bell" + CreateCommonDescString();
			}
		}

		protected override void AddSerializationData(XElement element) {
			// Nothing extra
		}

		protected override void PerformCustomDeserialization(XElement element) {
			// Nothing extra
		}

		protected override GroundableEntity CreateEntityObject(out float mass) {
			mass = 0f;
			return new FinishingBellEntity(Transform);
		}
	}
}