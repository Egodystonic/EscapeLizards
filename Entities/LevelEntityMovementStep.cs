// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 26 05 2015 at 17:30 by Ben Bowen

using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace Ophidian.Losgap.Entities {
	public sealed class LevelEntityMovementStep {
		private const int NUM_DECIMAL_PLACES = 8;
		public const string MOVEMENT_STEP_ELEMENT_NAME = "movementStep";
		private const string TRANSFORM_ATTR_NAME = "transform";
		private const string TRAVEL_TIME_ATTR_NAME = "travelTime";
		private const string SMOOTHING_ATTR_NAME = "smoothing";
		public readonly Transform Transform;
		public readonly float TravelTime;
		public readonly bool SmoothTransition;

		public LevelEntityMovementStep(Transform transform, float travelTime, bool smoothTransition) {
			Transform = transform;
			TravelTime = travelTime;
			SmoothTransition = smoothTransition;
		}

		public LevelEntityMovementStep(XElement stepElement) {
			Transform = Transform.Parse(stepElement.Attribute(TRANSFORM_ATTR_NAME).Value);
			TravelTime = float.Parse(stepElement.Attribute(TRAVEL_TIME_ATTR_NAME).Value, CultureInfo.InvariantCulture);
			SmoothTransition = bool.Parse(stepElement.Attribute(SMOOTHING_ATTR_NAME).Value);
		}

		public void Serialize(XElement parentElement) {
			XElement movementStepElement = new XElement(MOVEMENT_STEP_ELEMENT_NAME);
			movementStepElement.SetAttributeValue(TRANSFORM_ATTR_NAME, Transform.ToString(NUM_DECIMAL_PLACES));
			movementStepElement.SetAttributeValue(TRAVEL_TIME_ATTR_NAME, TravelTime.ToString(NUM_DECIMAL_PLACES));
			movementStepElement.SetAttributeValue(SMOOTHING_ATTR_NAME, SmoothTransition.ToString());
			parentElement.Add(movementStepElement);
		}

		public override string ToString() {
			return Transform.Translation + " in " + TravelTime + "s";
		}

		public LevelEntityMovementStep Clone() {
			return new LevelEntityMovementStep(Transform, TravelTime, SmoothTransition);
		}
	}
}