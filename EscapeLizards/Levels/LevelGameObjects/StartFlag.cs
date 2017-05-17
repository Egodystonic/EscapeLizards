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
	public sealed class StartFlag : LevelGameObject {
		public override ModelHandle Model {
			get { return AssetLocator.StartFlagModel; }
		}

		public override Material Material {
			get { return AssetLocator.StartFlagMaterial; }
		}

		public override PhysicsShapeHandle PhysicsShape {
			get { return AssetLocator.UnitSphereShape; }
		}

		public override bool EditorOnly {
			get { return true; }
		}

		public override bool IgnorePhysicsInEditor {
			get { return true; }
		}

		public StartFlag(LevelDescription parentLevel, int id) : base(parentLevel, id) { }

		private StartFlag(LevelDescription parentLevel, LevelGeometryEntity groundingGeometryEntity, Transform transform, bool useSimpleGrounding, int id) : base(parentLevel, groundingGeometryEntity, transform, useSimpleGrounding, id) { }

		public override string ToString() {
			return "Start flag";
		}

		public override LevelGameObject Clone(int id) {
			return new StartFlag(ParentLevel, GroundingGeometryEntity, Transform, UseSimpleGrounding, id);
		}

		public override string LongDescription {
			get {
				return "Start flag" + CreateCommonDescString();
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
			return new StartFlagEntity(Transform);
		}
	}
}