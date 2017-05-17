// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 08 05 2015 at 17:14 by Ben Bowen

using System;
using System.Collections.Generic;
using System.Linq;

namespace Ophidian.Losgap.Rendering {
	public class FontString : IDisposable, IHUDObject {
		private readonly object instanceMutationLock = new object();
		private readonly Font font;
		private readonly SceneLayer sceneLayer;
		private readonly SceneViewport viewport;
		private readonly Dictionary<FontCharacter, Material> materialsCache = new Dictionary<FontCharacter, Material>();
		private readonly Func<FontCharacter, Material> createMaterialFunc;
		private ViewportAnchoring anchoring;
		private Vector2 scale;
		private Vector2 anchorOffset;
		private string text = String.Empty;
		private Vector4 color = Vector4.ONE;
		private readonly FastClearList<FontCharacter> characterArray = new FastClearList<FontCharacter>();
		private readonly List<Material> activeMaterials = new List<Material>();
		private readonly FastClearList<ModelInstanceHandle> activeInstances = new FastClearList<ModelInstanceHandle>();
		private Vector2 actualSizePixels;
		private int zIndex = 0;

		public Vector4 Color {
			get {
				lock (instanceMutationLock) {
					return color;
				}
			}
			set {
				lock (instanceMutationLock) {
					color = value;
					RefreshColorOnly();
				}
			}
		}

		public string Text {
			get {
				lock (instanceMutationLock) {
					return text;
				}
			}
			set {
				lock (instanceMutationLock) {
					text = value;
					RefreshText();
				}
			}
		}

		public Vector2 AnchorOffset {
			get {
				lock (instanceMutationLock) {
					return anchorOffset;
				}
			}
			set {
				lock (instanceMutationLock) {
					anchorOffset = value;
					RefreshText();
				}
			}
		}

		public Vector2 Scale {
			get {
				lock (instanceMutationLock) {
					return scale;
				}
			}
			set {
				lock (instanceMutationLock) {
					scale = value;
					RefreshText();
				}
			}
		}

		public ViewportAnchoring Anchoring {
			get {
				lock (instanceMutationLock) {
					return anchoring;
				}
			}
			set {
				lock (instanceMutationLock) {
					anchoring = value;
					RefreshText();
				}
			}
		}

		public float Rotation {
			get { return 0f; }
			set { /* do nothing */ }
		}

		object IHUDObject.GetValueAsObject() {
			lock (instanceMutationLock) {
				return text;
			}
		}

		public IHUDObject Clone() {
			lock (instanceMutationLock) {
				var result = font.AddString(sceneLayer, viewport, anchoring, anchorOffset, scale);
				result.Text = text;
				result.Color = color;
				return result;
			}
		}

		public void CopyTo(IHUDObject targetObj) {
			lock (instanceMutationLock) {
				FontString objAsFontString = (FontString) targetObj;

				objAsFontString.anchoring = anchoring;
				objAsFontString.anchorOffset = anchorOffset;
				objAsFontString.scale = scale;
				objAsFontString.text = text;
				objAsFontString.color = color;
				objAsFontString.RefreshText();
			}
		}

		public Vector2 Dimensions {
			get {
				lock (instanceMutationLock) {
					return new Vector2(actualSizePixels.X / viewport.SizePixels.X, actualSizePixels.Y / viewport.SizePixels.Y);
				}
			}
		}

		public int ZIndex {
			get {
				lock (instanceMutationLock) {
					return zIndex;
				}
			}
			set {
				lock (instanceMutationLock) {
					zIndex = value;
					foreach (var material in materialsCache.Values) {
						material.ZIndex = value;
					}
				}
			}
		}

		internal FontString(Font font, SceneLayer sceneLayer, SceneViewport viewport, ViewportAnchoring anchoring, Vector2 anchorOffset, Vector2 scale) {
			this.sceneLayer = sceneLayer;
			this.viewport = viewport;
			this.anchoring = anchoring;
			this.anchorOffset = anchorOffset;
			this.scale = scale;
			this.font = font;
			this.createMaterialFunc = CreateNewMaterial;

			viewport.TargetWindow.WindowResized += ResizeText;
		}

		public void Dispose() {
			lock (instanceMutationLock) {
				for (int i = 0; i < activeInstances.Count; ++i) {
					if (activeMaterials[i] == null) break;
					activeInstances[i].Dispose();
				}
				materialsCache.Values.Where(mat => !mat.IsDisposed).ForEach(mat => mat.Dispose());
				viewport.TargetWindow.WindowResized -= ResizeText;
			}
		}

		public void AdjustAlpha(float alpha) {
			lock (instanceMutationLock) {
				color = new Vector4(color, w: (float) MathUtils.Clamp(color.W + alpha, 0f, 1f));
				RefreshColorOnly();
			}
		}

		public void SetAlpha(float alpha) {
			lock (instanceMutationLock) {
				color = new Vector4(color, w: (float) MathUtils.Clamp(alpha, 0f, 1f));
				RefreshColorOnly();
			}
		}

		private void ResizeText(Window _, uint __, uint ___) {
			lock (instanceMutationLock) {
				RefreshText();
			}
		}

		private void RefreshColorOnly() {
			for (int i = 0; i < activeMaterials.Count; ++i) {
				activeMaterials[i].SetMaterialConstantValue(font.TextFSColorCBB, color);
			}
		}

		private void RefreshText() {
			using (RenderingModule.RenderStateBarrier.AcquirePermit()) {
				for (int i = 0; i < activeInstances.Count; ++i) {
					if (activeMaterials[i] == null) break;
					activeInstances[i].Dispose();
				}

				characterArray.Clear();
				activeMaterials.Clear();
				activeInstances.Clear();

				for (int i = 0; i < text.Length; ++i) characterArray.Add(font.GetFontChar(text[i]));

				Vector2 viewportSizePixels = viewport.SizePixels;
				Vector2 actualScale = scale.Scale(new Vector2(viewportSizePixels.X / Font.FULL_SCALE_RESOLUTION.X, viewportSizePixels.Y / Font.FULL_SCALE_RESOLUTION.Y));
				Vector2 anchorOffsetPixels = viewportSizePixels.Scale(anchorOffset);
				float cursorPos;
				float yOffset;

				// Anchoring enum values are bitmasks to make this bit work. See comments in ViewportAnchoring
				// Falling to default values in both switch statements will result in a standard top-left corner selection
				float actualLineHeight = font.LineHeightPixels * actualScale.Y;
				float scaledCharHeight = font.MaxCharHeight * actualScale.Y;
				switch ((int)anchoring & 0xF0) { // Vertical bits
					case 0x10: // 1 == "Bottom"
						yOffset = anchorOffsetPixels.Y + scaledCharHeight;
						break;
					case 0x20: // 2 == "Centered"
						yOffset = (viewportSizePixels.Y + scaledCharHeight) * 0.5f;
						break;
					default: // 0 == "Top"
						yOffset = viewportSizePixels.Y - anchorOffsetPixels.Y;
						break;
				}

				float stringSizeX = ((text.Length - 1) * font.KerningPixels);
				for (int i = 0; i < text.Length; ++i) {
					stringSizeX += characterArray[i].Boundary.Width;
				}
				stringSizeX *= actualScale.X;
				switch ((int)anchoring & 0xF) { // Horizontal bits
					case 0x1: // 1 == "Right"
						cursorPos = -stringSizeX;
						for (int c = 0; c < text.Length; ++c) {
							FontCharacter fontChar = characterArray[c];
							activeMaterials.Add(materialsCache.GetOrCreate(fontChar, createMaterialFunc));
							activeInstances.Add(sceneLayer.CreateModelInstance(
								fontChar.ModelHandle,
								activeMaterials[c],
								new Transform(
									actualScale,
									Quaternion.IDENTITY,
									new Vector2(cursorPos + (viewportSizePixels.X - anchorOffsetPixels.X), yOffset - (fontChar.YOffset * actualScale.Y + actualLineHeight))
									)
								));
							cursorPos += (font.KerningPixels + fontChar.Boundary.Width) * actualScale.X;
						}
						break;
					case 0x2: // 2 == "Centered"
						cursorPos = -stringSizeX / 2f;
						for (int c = 0; c < text.Length; ++c) {
							FontCharacter fontChar = characterArray[c];
							activeMaterials.Add(materialsCache.GetOrCreate(fontChar, createMaterialFunc));
							activeInstances.Add(sceneLayer.CreateModelInstance(
								fontChar.ModelHandle,
								activeMaterials[c],
								new Transform(
									actualScale,
									Quaternion.IDENTITY,
									new Vector2(cursorPos + viewportSizePixels.X / 2f, yOffset - (fontChar.YOffset * actualScale.Y + actualLineHeight))
									)
								));
							cursorPos += (font.KerningPixels + fontChar.Boundary.Width) * actualScale.X;
						}
						break;
					default: // 0 == "Left"
						cursorPos = 0f;
						for (int c = 0; c < text.Length; ++c) {
							FontCharacter fontChar = characterArray[c];
							activeMaterials.Add(materialsCache.GetOrCreate(fontChar, createMaterialFunc));
							activeInstances.Add(sceneLayer.CreateModelInstance(
								fontChar.ModelHandle,
								activeMaterials[c],
								new Transform(
									actualScale,
									Quaternion.IDENTITY,
									new Vector2(cursorPos + anchorOffsetPixels.X, yOffset - (fontChar.YOffset * actualScale.Y + actualLineHeight))
									)
								));
							cursorPos += (font.KerningPixels + fontChar.Boundary.Width) * actualScale.X;
						}
						break;
				}

				actualSizePixels = new Vector2(stringSizeX, actualLineHeight);

				RefreshColorOnly();
			}
		}

		private Material CreateNewMaterial(FontCharacter fontChar) {
			Material result = fontChar.CreateNewColorableMaterial();
			result.SetMaterialConstantValue(font.TextFSColorCBB, color);
			result.ZIndex = zIndex;
			return result;
		}
	}
}