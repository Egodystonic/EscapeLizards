// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 09 10 2015 at 14:13 by Ben Bowen

using System;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Linq;
using Ophidian.Losgap;

namespace Egodystonic.EscapeLizards {
	public sealed class LevelSkyLight {
		private const int NUM_DECIMAL_PLACES = 8;
		private const string LIGHT_ELEMENT_NAME = "skyLight";
		private const string LIGHT_POSITION_ATTR_NAME = "position";
		private const string LIGHT_COLOR_ATTR_NAME = "color";
		private const string LIGHT_RADIUS_ATTR_NAME = "radius";
		private const string LIGHT_LUMINANCE_MULTIPLIER_ATTR_NAME = "lumMult";
		public readonly Vector3 Position;
		public readonly Vector3 Color;
		public readonly float Radius;
		public readonly float LuminanceMultiplier;

		public LevelSkyLight(Vector3 position, Vector3 color, float radius, float luminanceMultiplier) {
			this.Position = position;
			this.Color = color;
			this.Radius = radius;
			this.LuminanceMultiplier = luminanceMultiplier;
		}

		public XElement Serialize() {
			XElement lightElement = new XElement(LIGHT_ELEMENT_NAME);
			lightElement.SetAttributeValue(LIGHT_POSITION_ATTR_NAME, Position.ToString(NUM_DECIMAL_PLACES));
			lightElement.SetAttributeValue(LIGHT_COLOR_ATTR_NAME, Color.ToString(NUM_DECIMAL_PLACES));
			lightElement.SetAttributeValue(LIGHT_RADIUS_ATTR_NAME, Radius.ToString(NUM_DECIMAL_PLACES));
			lightElement.SetAttributeValue(LIGHT_LUMINANCE_MULTIPLIER_ATTR_NAME, LuminanceMultiplier.ToString(NUM_DECIMAL_PLACES));
			return lightElement;
		}

		public static LevelSkyLight Deserialize(XElement lightElement) {
			return new LevelSkyLight(
				Vector3.Parse(lightElement.Attribute(LIGHT_POSITION_ATTR_NAME).Value), 
				Vector3.Parse(lightElement.Attribute(LIGHT_COLOR_ATTR_NAME).Value),
				float.Parse(lightElement.Attribute(LIGHT_RADIUS_ATTR_NAME).Value, CultureInfo.InvariantCulture),
				float.Parse(lightElement.Attribute(LIGHT_LUMINANCE_MULTIPLIER_ATTR_NAME).Value, CultureInfo.InvariantCulture)
			);
		}

		public override string ToString() {
			return Color.ToString(3) + " (" + Radius.ToString(0) + ")";
		}
	}
}