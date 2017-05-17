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
	public sealed class IntroCameraAttracter : LevelGameObject {
		public override ModelHandle Model {
			get { return AssetLocator.IntroCameraModel; }
		}

		public override Material Material {
			get { return AssetLocator.IntroCameraMaterial; }
		}

		public override PhysicsShapeHandle PhysicsShape {
			get { return AssetLocator.UnitSphereShape; }
		}

		public override bool EditorOnly {
			get { return true; }
		}

		public override string LongDescription {
			get { return "Intro Camera Attracter" + CreateCommonDescString(); }
		}

		public override bool IgnorePhysicsInEditor {
			get { return true; }
		}

		public IntroCameraAttracter(LevelDescription parentLevel, int id) 
			: base(parentLevel, id) { }
		private IntroCameraAttracter(LevelDescription parentLevel, LevelGeometryEntity groundingGeometryEntity, Transform transform, bool useSimpleGrounding, int id) : base(parentLevel, groundingGeometryEntity, transform, useSimpleGrounding, id) { }

		protected override void AddSerializationData(XElement element) {
			// No extra data
		}

		protected override void PerformCustomDeserialization(XElement element) {
			// No extra data
		}

		public override LevelGameObject Clone(int id) {
			return new IntroCameraAttracter(ParentLevel, GroundingGeometryEntity, Transform, UseSimpleGrounding, id);
		}

		protected override GroundableEntity CreateEntityObject(out float mass) {
			mass = 0f;
			return new IntroCameraAttracterEntity(Transform);
		}

		public override string ToString() {
			return "Intro Camera Attracter";
		}
	}
}