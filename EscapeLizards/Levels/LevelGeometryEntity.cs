// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 26 05 2015 at 17:26 by Ben Bowen

using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Linq;
using Ophidian.Losgap;
using Ophidian.Losgap.Entities;

namespace Egodystonic.EscapeLizards {
	public sealed class LevelGeometryEntity {
		private const int NUM_DECIMAL_PLACES = 8;
		private const string ENTITY_ELEMENT_NAME = "geomEntity";
		private const string TAG_ATTRIBUTE_NAME = "tag";
		private const string ID_ATTRIBUTE_NAME = "id";
		private const string GEOM_ID_ATTRIBUTE_NAME = "geomID";
		private const string MAT_ID_ATTRIBUTE_NAME = "matID";
		private const string ALT_MOVE_DIR_ATTRIBUTE_NAME = "altMoveDir";
		private const string INIT_DELAY_ATTRIBUTE_NAME = "initDelay";
		private const string MOVEMENT_STEPS_ELEMENT_NAME = "movementFrames";
		private const string TEX_PAN_ELEMENT_NAME = "texPan";
		public readonly int ID;
		public readonly LevelDescription ParentLevel;
		private readonly object instanceMutationLock = new object();
		private readonly List<LevelEntityMovementStep> movementSteps = new List<LevelEntityMovementStep>();
		private bool alternatingMovementDirection;
		private string tag;
		private int geomID;
		private int matID;
		private float initialDelay;
		private Vector2 texPan;

		public string Tag {
			get {
				lock (instanceMutationLock) {
					return tag;
				}
			}
			set {
				lock (instanceMutationLock) {
					tag = value;
				}
			}
		}

		public bool IsStatic {
			get {
				lock (instanceMutationLock) {
					return movementSteps.Count == 1;
				}
			}
		}

		public LevelEntityMovementStep InitialMovementStep {
			get {
				lock (instanceMutationLock) {
					return movementSteps[0];
				}
			}
		}

		public IReadOnlyList<LevelEntityMovementStep> MovementSteps {
			get {
				return new ReadOnlyCollection<LevelEntityMovementStep>(movementSteps.ToList()); // ToList() to prevent concurrency problems
			}
		}

		public LevelGeometry Geometry {
			get {
				int geomIDLocal;
				lock (instanceMutationLock) {
					geomIDLocal = geomID;
				}
				return ParentLevel.GetGeometryByID(geomIDLocal);
			}
			set {
				lock (instanceMutationLock) {
					geomID = value.ID;
				}
			}
		}

		public LevelMaterial Material {
			get {
				int matIDLocal;
				lock (instanceMutationLock) {
					matIDLocal = matID;
				}
				return ParentLevel.GetMaterialByID(matIDLocal);
			}
			set {
				lock (instanceMutationLock) {
					matID = value.ID;
				}
			}
		}

		public bool AlternatingMovementDirection {
			get {
				lock (instanceMutationLock) {
					return alternatingMovementDirection;
				}
			}
			set {
				lock (instanceMutationLock) {
					alternatingMovementDirection = value;
				}
			}
		}

		public float InitialDelay {
			get {
				lock (instanceMutationLock) {
					return initialDelay;
				}
			}
			set {
				lock (instanceMutationLock) {
					initialDelay = value;
				}
			}
		}

		public Vector2 TexPan {
			get {
				lock (instanceMutationLock) {
					return texPan;
				}
			}
			set {
				lock (instanceMutationLock) {
					texPan = value;
				}
			}
		}

		public LevelGeometryEntity(LevelDescription parentLevel, string tag, LevelGeometry geom, LevelMaterial mat, int id, LevelEntityMovementStep movementStep = null) {
			Tag = tag;
			geomID = geom.ID;
			matID = mat.ID;
			ID = id;
			this.ParentLevel = parentLevel;
			AddMovementStep(movementStep ?? new LevelEntityMovementStep(Transform.DEFAULT_TRANSFORM, 0f, false));
			alternatingMovementDirection = false;
			initialDelay = 0f;
			texPan = Vector2.ZERO;
		}

		public LevelGeometryEntity(LevelDescription parentLevel, XElement xmlElement) {
			this.ParentLevel = parentLevel;
			Tag = xmlElement.Attribute(TAG_ATTRIBUTE_NAME).Value;
			geomID = int.Parse(xmlElement.Attribute(GEOM_ID_ATTRIBUTE_NAME).Value);
			matID = int.Parse(xmlElement.Attribute(MAT_ID_ATTRIBUTE_NAME).Value);
			ID = int.Parse(xmlElement.Attribute(ID_ATTRIBUTE_NAME).Value);
			alternatingMovementDirection = bool.Parse(xmlElement.Attribute(ALT_MOVE_DIR_ATTRIBUTE_NAME).Value);
			var initDelayAttr = xmlElement.Attribute(INIT_DELAY_ATTRIBUTE_NAME);
			initialDelay = initDelayAttr == null ? 0f : float.Parse(initDelayAttr.Value, CultureInfo.InvariantCulture);
			var texPanAttr = xmlElement.Attribute(TEX_PAN_ELEMENT_NAME);
			if (texPanAttr == null) texPan = Vector2.ZERO;
			else texPan = Vector2.Parse(texPanAttr.Value);
			foreach (XElement movementStep in xmlElement.Element(MOVEMENT_STEPS_ELEMENT_NAME).Elements(LevelEntityMovementStep.MOVEMENT_STEP_ELEMENT_NAME)) {
				movementSteps.Add(new LevelEntityMovementStep(movementStep));
			}
		}

		public XElement Serialize() {
			lock (instanceMutationLock) {
				XElement result = new XElement(ENTITY_ELEMENT_NAME);
				result.SetAttributeValue(TAG_ATTRIBUTE_NAME, Tag);
				result.SetAttributeValue(ID_ATTRIBUTE_NAME, ID);
				result.SetAttributeValue(GEOM_ID_ATTRIBUTE_NAME, geomID);
				result.SetAttributeValue(MAT_ID_ATTRIBUTE_NAME, matID);
				result.SetAttributeValue(ALT_MOVE_DIR_ATTRIBUTE_NAME, alternatingMovementDirection);
				result.SetAttributeValue(TEX_PAN_ELEMENT_NAME, texPan.ToString(NUM_DECIMAL_PLACES));
				result.SetAttributeValue(INIT_DELAY_ATTRIBUTE_NAME, initialDelay.ToString(NUM_DECIMAL_PLACES));
				XElement movementStepsElement = new XElement(MOVEMENT_STEPS_ELEMENT_NAME);
				foreach (var levelEntityMovementStep in movementSteps) {
					levelEntityMovementStep.Serialize(movementStepsElement);
				}
				result.Add(movementStepsElement);
				return result;
			}
		}

		public LevelGeometryEntity Clone(int id) {
			lock (instanceMutationLock) {
				LevelGeometryEntity clone = new LevelGeometryEntity(ParentLevel, Tag + " Clone", Geometry, Material, id, InitialMovementStep.Clone());
				for (int i = 1; i < movementSteps.Count; ++i) clone.AddMovementStep(movementSteps[i].Clone());
				clone.AlternatingMovementDirection = alternatingMovementDirection;
				clone.InitialDelay = initialDelay;
				clone.TexPan = texPan;
				return clone;
			}
		}

		public override string ToString() {
			return Tag + " (" + Material.Name + " " + Geometry.GetShortDesc() + ")";
		}

		public void AddMovementStep(LevelEntityMovementStep step, LevelEntityMovementStep insertAfterTarget = null) {
			lock (instanceMutationLock) {
				if (insertAfterTarget != null) {
					int index = movementSteps.IndexOf(insertAfterTarget);
					movementSteps.Insert(index + 1, step);
				}
				else movementSteps.Add(step);
			}
		}

		public void RemoveMovementStep(LevelEntityMovementStep step) {
			lock (instanceMutationLock) {
				movementSteps.Remove(step);
			}
		}

		public void ReplaceMovementStep(LevelEntityMovementStep existing, LevelEntityMovementStep replacement) {
			lock (instanceMutationLock) {
				movementSteps.Replace(existing, replacement);
			}
		}
	}
}