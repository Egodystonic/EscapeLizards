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
	public sealed class DynamicLight : LevelGameObject {
		private const string LIGHT_RADIUS_ATTR_NAME = "lightRadius";
		private const string LIGHT_COLOR_ATTR_NAME = "lightColor";
		private const string LIGHT_LUM_MULT_ATTR_NAME = "lumMult";
		private float lightRadius = PhysicsManager.ONE_METRE_SCALED;
		private Vector3 lightColor = Vector3.ONE * 0.35f;
		private float luminanceMultiplier = 1f;

		public float LightRadius {
			get {
				lock (instanceMutationLock) {
					return lightRadius;
				}
			}
			set {
				lock (instanceMutationLock) {
					Assure.GreaterThanOrEqualTo(value, 0f);
					lightRadius = value;
				}
			}
		}

		public Vector3 LightColor {
			get {
				lock (instanceMutationLock) {
					return lightColor;
				}
			}
			set {
				lock (instanceMutationLock) {
					lightColor = value;
				}
			}
		}

		public float LuminanceMultiplier {
			get {
				lock (instanceMutationLock) {
					return luminanceMultiplier;
				}
			}
			set {
				lock (instanceMutationLock) {
					luminanceMultiplier = value;
				}
			}
		}

		public override ModelHandle Model {
			get { return AssetLocator.DynamicLightModel; }
		}

		public override Material Material {
			get { return AssetLocator.DynamicLightMaterial; }
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

		public override string LongDescription {
			get { return "Dynamic Light" + CreateCommonDescString(); }
		}

		public DynamicLight(LevelDescription parentLevel, int id) 
			: base(parentLevel, id) { }
		private DynamicLight(LevelDescription parentLevel, LevelGeometryEntity groundingGeometryEntity, Transform transform, bool useSimpleGrounding, int id) : base(parentLevel, groundingGeometryEntity, transform, useSimpleGrounding, id) { }

		protected override void AddSerializationData(XElement element) {
			element.SetAttributeValue(LIGHT_RADIUS_ATTR_NAME, lightRadius.ToString(NUM_DECIMAL_PLACES));
			element.SetAttributeValue(LIGHT_COLOR_ATTR_NAME, lightColor.ToString(NUM_DECIMAL_PLACES));
			element.SetAttributeValue(LIGHT_LUM_MULT_ATTR_NAME, luminanceMultiplier.ToString(NUM_DECIMAL_PLACES));
		}

		protected override void PerformCustomDeserialization(XElement element) {
			lightRadius = float.Parse(element.Attribute(LIGHT_RADIUS_ATTR_NAME).Value, CultureInfo.InvariantCulture);
			lightColor = Vector3.Parse(element.Attribute(LIGHT_COLOR_ATTR_NAME).Value);
			luminanceMultiplier = float.Parse(element.Attribute(LIGHT_LUM_MULT_ATTR_NAME).Value, CultureInfo.InvariantCulture);
		}

		public override LevelGameObject Clone(int id) {
			return new DynamicLight(ParentLevel, GroundingGeometryEntity, Transform, UseSimpleGrounding, id) {
				lightColor = lightColor,
				lightRadius = lightRadius,
				LuminanceMultiplier = luminanceMultiplier
			};
		}

		protected override GroundableEntity CreateEntityObject(out float mass) {
			mass = 0f;
			return new DynamicLightEntity(Transform, lightRadius, lightColor, luminanceMultiplier);
		}

		public override string ToString() {
			return "Dynamic Light (" + LightColor.ToString(1) + ", " + LightRadius.ToString(0) + ")";
		}
	}
}