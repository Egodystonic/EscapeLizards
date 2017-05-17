// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 21 07 2015 at 15:03 by Ben Bowen

using System;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Linq;
using Ophidian.Losgap;
using Ophidian.Losgap.Entities;
using Ophidian.Losgap.Rendering;

namespace Egodystonic.EscapeLizards {
	public sealed class Shadowcaster : LevelGameObject {
		public static readonly Vector3 DEFAULT_ORTHOGRAPHIC_DIMENSIONS = new Vector3(2000f, 2000f, PhysicsManager.ONE_METRE_SCALED * 3f);
		private const string RES_MULT_ATTR_NAME = "resMult";
		private const string DEPTH_OFFSET_ATTR_NAME = "depthOffset";

		private float resolutionMultiplier = 1f;
		private float depthOffset = 30f;

		public override ModelHandle Model {
			get { return AssetLocator.ShadowcasterModel; }
		}

		public override Material Material {
			get { return AssetLocator.ShadowcasterMaterial; }
		}

		public override PhysicsShapeHandle PhysicsShape {
			get { return PhysicsShapeHandle.NULL; }
		}

		public override bool EditorOnly {
			get { return true; }
		}

		public override bool IgnorePhysicsInEditor {
			get { return true; }
		}

		public float ResolutionMultiplier {
			get {
				lock (instanceMutationLock) {
					return resolutionMultiplier;
				}
			}
			set {
				lock (instanceMutationLock) {
					resolutionMultiplier = value;
				}
			}
		}

		public float DepthOffset {
			get {
				lock (instanceMutationLock) {
					return depthOffset;
				}
			}
			set {
				lock (instanceMutationLock) {
					depthOffset = value;
				}
			}
		}

		public Shadowcaster(LevelDescription parentLevel, int id) : base(parentLevel, id) { }

		private Shadowcaster(LevelDescription parentLevel, LevelGeometryEntity groundingGeometryEntity, Transform transform, bool useSimpleGrounding, int id) : base(parentLevel, groundingGeometryEntity, transform, useSimpleGrounding, id) { }

		public override string ToString() {
			return "Shadowcaster (" + resolutionMultiplier.ToString(1) + "x, +" + depthOffset.ToString(1) + ")";
		}

		public override LevelGameObject Clone(int id) {
			return new Shadowcaster(ParentLevel, GroundingGeometryEntity, Transform, UseSimpleGrounding, id);
		}

		public override string LongDescription {
			get {
				return "Shadowcaster" + CreateCommonDescString();
			}
		}

		protected override void AddSerializationData(XElement element) {
			element.SetAttributeValue(RES_MULT_ATTR_NAME, resolutionMultiplier.ToString(NUM_DECIMAL_PLACES));
			element.SetAttributeValue(DEPTH_OFFSET_ATTR_NAME, depthOffset.ToString(NUM_DECIMAL_PLACES));
		}

		protected override void PerformCustomDeserialization(XElement element) {
			resolutionMultiplier = float.Parse(element.Attribute(RES_MULT_ATTR_NAME).Value, CultureInfo.InvariantCulture);
			depthOffset = float.Parse(element.Attribute(DEPTH_OFFSET_ATTR_NAME).Value, CultureInfo.InvariantCulture);
		}

		protected override GroundableEntity CreateEntityObject(out float mass) {
			mass = 0f;
			return new ShadowcasterEntity(Transform, new Vector3(DEFAULT_ORTHOGRAPHIC_DIMENSIONS.X / resolutionMultiplier, DEFAULT_ORTHOGRAPHIC_DIMENSIONS.Y / resolutionMultiplier, DEFAULT_ORTHOGRAPHIC_DIMENSIONS.Z * depthOffset));
		}
	}
}