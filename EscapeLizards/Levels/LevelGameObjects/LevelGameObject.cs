// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 13 07 2015 at 17:23 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using Ophidian.Losgap;
using Ophidian.Losgap.Entities;
using Ophidian.Losgap.Rendering;

namespace Egodystonic.EscapeLizards {
	public abstract class LevelGameObject {
		protected const int NUM_DECIMAL_PLACES = 8;
		private const string GAME_OBJECT_ELEMENT_NAME = "gameObject";
		private const string TYPE_ATTRIBUTE_NAME = "type";
		private const string ID_ATTRIBUTE_NAME = "id";
		private const string GROUNDING_ENTITY_ID_ATTRIBUTE_NAME = "groundingEntityID";
		private const string TRANSFORM_ATTRIBUTE_NAME = "transform";
		private const string USE_SIMPLE_GROUNDING_ATTRIBUTE_NAME = "useSimpleGrounding";
		private static readonly Dictionary<Type, string> typeNames = new Dictionary<Type, string> {
			{ typeof(StartFlag), "startFlag" },
			{ typeof(DynamicLight), "dynamicLight" },
			{ typeof(IntroCameraAttracter), "introCameraAttracter" },
			{ typeof(FinishingBell), "finishingBell" },
			{ typeof(VultureEgg), "vultureEgg" },
			{ typeof(Shadowcaster), "shadowCaster" },
			{ typeof(LizardCoin), "lizardCoin" }
		};
		public readonly int ID;
		protected readonly object instanceMutationLock = new object();
		protected readonly LevelDescription ParentLevel;
		private int? groundingEntityID;
		private Transform transform;
		private bool useSimpleGrounding;

		public bool UseSimpleGrounding {
			get {
				lock (instanceMutationLock) {
					return useSimpleGrounding;
				}
			}
			set {
				lock (instanceMutationLock) {
					useSimpleGrounding = value;
				}
			}
		}

		public LevelGeometryEntity GroundingGeometryEntity {
			get {
				lock (instanceMutationLock) {
					if (groundingEntityID == null) return null;
					return ParentLevel.GetEntityByID(groundingEntityID.Value);
				}
			}
			set {
				lock (instanceMutationLock) {
					groundingEntityID = (value == null ? (int?) null : value.ID);
				}
			}
		}

		public Transform Transform {
			get {
				lock (instanceMutationLock) {
					return transform;
				}
			}
			set {
				lock (instanceMutationLock) {
					transform = value;
				}
			}
		}

		public XElement Serialize() {
			lock (instanceMutationLock) {
				XElement result = new XElement(GAME_OBJECT_ELEMENT_NAME);
				result.SetAttributeValue(TYPE_ATTRIBUTE_NAME, typeNames[GetType()]);
				result.SetAttributeValue(ID_ATTRIBUTE_NAME, ID);
				result.SetAttributeValue(GROUNDING_ENTITY_ID_ATTRIBUTE_NAME, groundingEntityID.HasValue ? groundingEntityID.ToString() : String.Empty);
				result.SetAttributeValue(TRANSFORM_ATTRIBUTE_NAME, transform.ToString(NUM_DECIMAL_PLACES));
				result.SetAttributeValue(USE_SIMPLE_GROUNDING_ATTRIBUTE_NAME, useSimpleGrounding.ToString());
				AddSerializationData(result);
				return result;
			}
		}

		public static LevelGameObject Deserialize(LevelDescription parentLevel, XElement element) {
			string typeName = element.Attribute(TYPE_ATTRIBUTE_NAME).Value;
			int id = Int32.Parse(element.Attribute(ID_ATTRIBUTE_NAME).Value);
			Type goType = typeNames.Single(kvp => kvp.Value == typeName).Key;
			LevelGameObject result = (LevelGameObject) Activator.CreateInstance(goType, parentLevel, id);
			lock (result.instanceMutationLock) {
				string groundingEntityAttrValue = element.Attribute(GROUNDING_ENTITY_ID_ATTRIBUTE_NAME).Value;
				result.groundingEntityID = groundingEntityAttrValue == String.Empty ? (int?) null : Int32.Parse(groundingEntityAttrValue);
				result.transform = Transform.Parse(element.Attribute(TRANSFORM_ATTRIBUTE_NAME).Value);
				result.useSimpleGrounding = Boolean.Parse(element.Attribute(USE_SIMPLE_GROUNDING_ATTRIBUTE_NAME).Value);
				result.PerformCustomDeserialization(element);
			}
			return result;
		}

		protected abstract void AddSerializationData(XElement element);
		protected abstract void PerformCustomDeserialization(XElement element);

		protected LevelGameObject(LevelDescription parentLevel, int id) {
			this.ParentLevel = parentLevel;
			ID = id;
		}

		protected LevelGameObject(LevelDescription parentLevel, LevelGeometryEntity groundingGeometryEntity, Transform transform, bool useSimpleGrounding, int id) {
			this.ParentLevel = parentLevel;
			groundingEntityID = groundingGeometryEntity == null ? null : (int?) groundingGeometryEntity.ID;
			this.transform = transform;
			this.useSimpleGrounding = useSimpleGrounding;
			ID = id;
		}

		public abstract LevelGameObject Clone(int id);

		public abstract ModelHandle Model { get; }
		public abstract Material Material { get; }
		public abstract PhysicsShapeHandle PhysicsShape { get; }
		public abstract bool EditorOnly { get; }
		public abstract bool IgnorePhysicsInEditor { get; }
		public virtual bool DynamicsCollisionOnly { get { return false; } }

		public abstract string LongDescription { get; }

		protected string CreateCommonDescString() {
			return (GroundingGeometryEntity == null ? ", ungrounded" : ", grounded to '" + GroundingGeometryEntity.Tag + "'")
				+ ". Transform: " + Transform;
		}

		protected abstract GroundableEntity CreateEntityObject(out float mass);

		public virtual GroundableEntity CreateEntity() {
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
				if (!EntryPoint.InEditor || !IgnorePhysicsInEditor) {
					result.SetPhysicsShape(PhysicsShape, Vector3.ZERO, mass, disablePerformanceDeactivation: true, collideWithOnlyDynamics: DynamicsCollisionOnly);
				}
				result.SetModelInstance(AssetLocator.GameLayer, Model, Material);
			}

			if (groundingEntity != null) {
				result.Ground(groundingEntity);
				result.UseSimpleGrounding = UseSimpleGrounding;
			}
			return result;
		}
	}
}