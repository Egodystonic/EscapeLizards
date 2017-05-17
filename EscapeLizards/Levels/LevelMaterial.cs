// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 26 05 2015 at 13:54 by Ben Bowen

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Linq;
using Ophidian.Losgap;

namespace Egodystonic.EscapeLizards {
	public sealed class LevelMaterial {
		private const string MATERIAL_ELEMENT_NAME = "material";
		private const string NAME_ATTRIBUTE_NAME = "name";
		private const string TEX_FILE_ATTRIBUTE_NAME = "texFile";
		private const string NORM_FILE_ATTRIBUTE_NAME = "normFile";
		private const string SPEC_FILE_ATTRIBUTE_NAME = "specFile";
		private const string EMIS_FILE_ATTRIBUTE_NAME = "emisFile";
		private const string SPEC_POWER_ATTRIBUTE_NAME = "specPower";
		private const string SPEC_PROPS_ATTRIBUTE_NAME = "specProps";
		private const string GEN_MIPS_ATTRIBUTE_NAME = "mipGen";
		private const string ID_ATTRIBUTE_NAME = "id";
		private const int NUM_SPEC_PROPS_DEC_PLACES = 8;
		public readonly string Name;
		public readonly string TextureFileName;
		public readonly string NormalFileName;
		public readonly string SpecularFileName;
		public readonly string EmissiveFileName;
		public readonly float SpecularPower;
		public readonly bool GenerateMips;
		public readonly int ID;

		public LevelMaterial(string name, string textureFileName, string normalFileName, string specularFileName, string emissiveFileName, float specularPower, bool generateMips, int id) {
			Name = name;
			TextureFileName = textureFileName;
			NormalFileName = normalFileName;
			SpecularFileName = specularFileName;
			EmissiveFileName = emissiveFileName;
			SpecularPower = specularPower;
			GenerateMips = generateMips;
			ID = id;
		}

		public LevelMaterial(XElement xmlElement) {
			Name = xmlElement.Attribute(NAME_ATTRIBUTE_NAME).Value;
			TextureFileName = xmlElement.Attribute(TEX_FILE_ATTRIBUTE_NAME).Value;
			string textureRoot = Path.Combine(
				Path.GetDirectoryName(TextureFileName),
				Path.GetFileNameWithoutExtension(TextureFileName)
			);
			string possibleN = textureRoot + "_n" + Path.GetExtension(TextureFileName);
			string possibleS = textureRoot + "_s" + Path.GetExtension(TextureFileName);
			string possibleE = textureRoot + "_e" + Path.GetExtension(TextureFileName);
			if (xmlElement.Attribute(NORM_FILE_ATTRIBUTE_NAME) == null) {
				NormalFileName = File.Exists(Path.Combine(AssetLocator.MaterialsDir, possibleN)) ? possibleN : null;
			}
			else NormalFileName = xmlElement.Attribute(NORM_FILE_ATTRIBUTE_NAME).Value;
			if (xmlElement.Attribute(SPEC_FILE_ATTRIBUTE_NAME) == null) {
				SpecularFileName = File.Exists(Path.Combine(AssetLocator.MaterialsDir, possibleS)) ? possibleS : null;
			}
			else SpecularFileName = xmlElement.Attribute(SPEC_FILE_ATTRIBUTE_NAME).Value;
			if (xmlElement.Attribute(EMIS_FILE_ATTRIBUTE_NAME) == null) {
				EmissiveFileName = File.Exists(Path.Combine(AssetLocator.MaterialsDir, possibleE)) ? possibleE : null;
			}
			else EmissiveFileName = xmlElement.Attribute(EMIS_FILE_ATTRIBUTE_NAME).Value;
			if (xmlElement.Attribute(SPEC_POWER_ATTRIBUTE_NAME) == null) SpecularPower = Vector4.Parse(xmlElement.Attribute(SPEC_PROPS_ATTRIBUTE_NAME).Value).W;
			else SpecularPower = float.Parse(xmlElement.Attribute(SPEC_POWER_ATTRIBUTE_NAME).Value, CultureInfo.InvariantCulture);
			GenerateMips = bool.Parse(xmlElement.Attribute(GEN_MIPS_ATTRIBUTE_NAME).Value);
			ID = int.Parse(xmlElement.Attribute(ID_ATTRIBUTE_NAME).Value);
		}

		public XElement Serialize() {
			XElement result = new XElement(MATERIAL_ELEMENT_NAME);
			result.SetAttributeValue(NAME_ATTRIBUTE_NAME, Name);
			result.SetAttributeValue(TEX_FILE_ATTRIBUTE_NAME, TextureFileName);
			result.SetAttributeValue(NORM_FILE_ATTRIBUTE_NAME, NormalFileName);
			result.SetAttributeValue(SPEC_FILE_ATTRIBUTE_NAME, SpecularFileName);
			result.SetAttributeValue(EMIS_FILE_ATTRIBUTE_NAME, EmissiveFileName);
			result.SetAttributeValue(SPEC_POWER_ATTRIBUTE_NAME, SpecularPower.ToString(NUM_SPEC_PROPS_DEC_PLACES));
			result.SetAttributeValue(GEN_MIPS_ATTRIBUTE_NAME, GenerateMips);
			result.SetAttributeValue(ID_ATTRIBUTE_NAME, ID);
			return result;
		}

		public override string ToString() {
			return Name + " (" + TextureFileName + ")";
		}
	}
}