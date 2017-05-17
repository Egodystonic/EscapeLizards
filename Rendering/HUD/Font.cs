// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 07 05 2015 at 17:31 by Ben Bowen

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Ophidian.Losgap.Rendering {
	public sealed class Font : IDisposable {
		public const string CHAR_MAP_SHADER_RES_NAME = "DiffuseMap";
		public const string TEXT_COLOR_SHADER_CB_NAME = "TextProperties";
		public static readonly Vector2 FULL_SCALE_RESOLUTION = new Vector2(1920f, 1080f);
		public readonly string Name;
		public readonly uint LineHeightPixels;
		public readonly int KerningPixels;
		public readonly GeometryCache CharacterGeometry;
		public readonly uint MaxCharHeight;
		internal readonly ConstantBufferBinding TextFSColorCBB;
		private readonly object instanceMutationLock = new object();
		private readonly ITexture2D[] characterPages;
		private readonly ShaderResourceView[] characterPageViews;
		private readonly Dictionary<char, FontCharacter> characters;
		private bool isDisposed = false;

		private Font(string name, uint lineHeightPixels, int kerningPixels, GeometryCache characterGeometry,
			ITexture2D[] characterPages, ShaderResourceView[] characterPageViews, Dictionary<char, FontCharacter> characters, 
			ConstantBufferBinding textFsColorCbb, uint maxCharHeight) {
			Name = name;
			LineHeightPixels = lineHeightPixels;
			KerningPixels = kerningPixels;
			this.characterPages = characterPages;
			this.characters = characters;
			TextFSColorCBB = textFsColorCbb;
			MaxCharHeight = maxCharHeight;
			CharacterGeometry = characterGeometry;
			this.characterPageViews = characterPageViews;
		}

		public static Font Load(string fontFile, FragmentShader textFS, uint? lineHeightPixels, int? kerningPixels) {
			Assure.NotNull(fontFile);
			Assure.NotNull(textFS);
			Assure.False(textFS.IsDisposed);
			if (!IOUtils.IsValidFilePath(fontFile) || !File.Exists(fontFile)) {
				throw new FileNotFoundException("File '" + fontFile + "' not found: Could not load font.");
			}

			XDocument fontDocument = XDocument.Load(fontFile, LoadOptions.None);
			XElement root = fontDocument.Root;
			XElement commonElement = root.Element("common");
			if (commonElement == null) throw new InvalidOperationException("Could not find common element in given font file.");

			string name = Path.GetFileNameWithoutExtension(fontFile).CapitalizeFirst();
			uint texWidth;
			uint texHeight;
			try {
				texWidth = uint.Parse(commonElement.Attribute("scaleW").Value);
				texHeight = uint.Parse(commonElement.Attribute("scaleH").Value);
				if (lineHeightPixels == null) lineHeightPixels = uint.Parse(commonElement.Attribute("lineHeight").Value) / 2U;
			}
			catch (Exception e) {
				throw new InvalidOperationException("Could not read scaleW, scaleH, or lineHeight value!", e);
			}

			XElement pagesElement = root.Element("pages");
			IEnumerable<XElement> pageElements = pagesElement.Elements("page");

			ITexture2D[] pageArray = new ITexture2D[pageElements.Count()];
			ShaderResourceView[] characterPageViews = new ShaderResourceView[pageArray.Length];

			foreach (XElement pageElement in pageElements) {
				int id;
				string filename;
				try {
					id = int.Parse(pageElement.Attribute("id").Value);
					filename = pageElement.Attribute("file").Value;
				}
				catch (Exception e) {
					throw new InvalidOperationException("Could not read page ID or filename for page " + pageElement + ".", e);
				}

				string fullFilename = Path.Combine(Path.GetDirectoryName(fontFile), filename);
				if (!IOUtils.IsValidFilePath(fullFilename) || !File.Exists(fullFilename)) {
					throw new InvalidOperationException("Page file '" + fullFilename + "' does not exist!");
				}
				if (id < 0 || id >= pageArray.Length || pageArray[id] != null) {
					throw new InvalidOperationException("Invalid or duplicate page ID '" + id + "'.");
				}

				pageArray[id] = TextureFactory.LoadTexture2D()
					.WithFilePath(fullFilename)
					.WithPermittedBindings(GPUBindings.ReadableShaderResource)
					.WithUsage(ResourceUsage.Immutable)
					.Create();

				characterPageViews[id] = pageArray[id].CreateView();
			}

			GeometryCacheBuilder<DefaultVertex> characterCacheBuilder = new GeometryCacheBuilder<DefaultVertex>();
			Dictionary<char, FontCharacter> charMap = new Dictionary<char, FontCharacter>();
			XElement charsElement = root.Element("chars");
			foreach (XElement charElement in charsElement.Elements("char")) {
				char unicodeValue;
				uint x, y, width, height;
				int pageID, yOffset;

				try {
					unicodeValue = (char) short.Parse(charElement.Attribute("id").Value);
					x = uint.Parse(charElement.Attribute("x").Value);
					y = uint.Parse(charElement.Attribute("y").Value);
					width = uint.Parse(charElement.Attribute("width").Value);
					height = uint.Parse(charElement.Attribute("height").Value);
					pageID = int.Parse(charElement.Attribute("page").Value);
					yOffset = int.Parse(charElement.Attribute("yoffset").Value);

				}
				catch (Exception e) {
					throw new InvalidOperationException("Could not acquire character ID, page ID, or dimensions for char " + charElement + ".", e);
				}

				Rectangle charMapBoundary = new Rectangle(x, y, width, height);

				ModelHandle modelHandle = characterCacheBuilder.AddModel(
					"Font_" + name + "_Character_" + unicodeValue,
					new[] {
						new DefaultVertex(
							new Vector3(0f, 0f, 1f),
							Vector3.BACKWARD, new Vector2(
								charMapBoundary.GetCornerX(Rectangle.RectangleCorner.BottomLeft) / texWidth, 
								charMapBoundary.GetCornerY(Rectangle.RectangleCorner.BottomLeft) / texHeight
								)),
						new DefaultVertex(
							new Vector3(charMapBoundary.Width, 0f, 1f),
							Vector3.BACKWARD, new Vector2(
								charMapBoundary.GetCornerX(Rectangle.RectangleCorner.BottomRight) / texWidth, 
								charMapBoundary.GetCornerY(Rectangle.RectangleCorner.BottomRight) / texHeight
								)),
						new DefaultVertex(
							new Vector3(charMapBoundary.Width, -charMapBoundary.Height, 1f),
							Vector3.BACKWARD, new Vector2(
								charMapBoundary.GetCornerX(Rectangle.RectangleCorner.TopRight) / texWidth, 
								charMapBoundary.GetCornerY(Rectangle.RectangleCorner.TopRight) / texHeight
								)),
						new DefaultVertex(
							new Vector3(0f, -charMapBoundary.Height, 1f),
							Vector3.BACKWARD, new Vector2(
								charMapBoundary.GetCornerX(Rectangle.RectangleCorner.TopLeft) / texWidth, 
								charMapBoundary.GetCornerY(Rectangle.RectangleCorner.TopLeft) / texHeight
								)),
					},
					new[] { 0U, 1U, 3U, 1U, 2U, 3U }
				);

				//yOffset = 0;
				//if (unicodeValue == '.') yOffset = (int) (lineHeightPixels.Value * 0.9f);

				charMap.Add(
					unicodeValue,
					new FontCharacter(
						unicodeValue,
						charMapBoundary,
						modelHandle,
						textFS,
						characterPageViews[pageID],
						yOffset
					)
				);
			}

			if (kerningPixels == null) kerningPixels = (int) (charMap.Values.Max(value => value.Boundary.Width) * 0.15f);

			uint maxCharHeight = (uint) charMap.Values.Max(fc => fc.Boundary.Height);

			return new Font(
				name,
				lineHeightPixels.Value,
				kerningPixels.Value, 
				characterCacheBuilder.Build(), 
				pageArray, 
				characterPageViews,
				charMap,
				(ConstantBufferBinding) textFS.GetBindingByIdentifier(TEXT_COLOR_SHADER_CB_NAME),
				maxCharHeight
			);
		}

		public FontString AddString(SceneLayer sceneLayer, SceneViewport viewport, 
			ViewportAnchoring anchoring, Vector2 anchorOffset, Vector2 scale) {
			lock (instanceMutationLock) {

				Assure.NotNull(sceneLayer);
				Assure.False(sceneLayer.IsDisposed);
				Assure.NotNull(viewport);
				Assure.False(viewport.IsDisposed);
				if (isDisposed) throw new ObjectDisposedException(Name);

				return new FontString(this, sceneLayer, viewport, anchoring, anchorOffset, scale);
			}
		}

		public void Dispose() {
			lock (instanceMutationLock) {
				if (isDisposed) return;
				CharacterGeometry.Dispose();
				characterPages.ForEach(page => page.Dispose());
				characterPageViews.ForEach(view => view.Dispose());
				isDisposed = true;
			}
		}

		internal FontCharacter GetFontChar(char @char) {
			FontCharacter matchingCharacter;
			if (characters.ContainsKey(@char)) matchingCharacter = characters[@char];
			else if (Char.IsLower(@char) && characters.ContainsKey(Char.ToUpper(@char))) matchingCharacter = characters[Char.ToUpper(@char)];
			else if (Char.IsUpper(@char) && characters.ContainsKey(Char.ToLower(@char))) matchingCharacter = characters[Char.ToLower(@char)];
			else if (characters.ContainsKey((char) 0)) matchingCharacter = characters[(char) 0];
			else if (characters.ContainsKey('?')) matchingCharacter = characters['?'];
			else matchingCharacter = characters.Values.First();

			return matchingCharacter;
		}
	}
}