// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 21 10 2015 at 11:42 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.Mime;

namespace Ophidian.Losgap.Rendering {
	public sealed class HUDTexture : IDisposable, IHUDObject {
		private const string HUDFS_TEX_PROPS_CBB_NAME = "TextureProperties";
		private const string HUDFS_TEX_BINDING_NAME = "DiffuseMap";
		private static readonly object staticMutationLock = new object();
		private static ModelHandle? hudTextureModel = null;
		private readonly object instanceMutationLock = new object();
		private readonly FragmentShader fragmentShader;
		private readonly SceneLayer sceneLayer;
		private readonly ConstantBufferBinding fsTexColorMultiplyBinding;
		private readonly ResourceViewBinding fsTexBinding; 
		private readonly SceneViewport targetViewport;
		private readonly Material material;
		private ModelInstanceHandle curModelInstance;
		private ITexture2D texture = null;
		private ShaderResourceView texView = null;
		private ViewportAnchoring anchoring = ViewportAnchoring.Centered;
		private Vector2 scale = Vector2.ONE * 0.1f;
		private float rotation = 0f;
		private Vector2 anchorOffset = Vector2.ZERO;
		private Vector4 color = Vector4.ONE;
		private AspectRatioCorrectionStrategy aspectCorrectionStrategy = AspectRatioCorrectionStrategy.None;

		public enum AspectRatioCorrectionStrategy {
			None,
			PreserveHorizontalScaling,
			PreserveVerticalScaling,
			UseBestUniformScaling
		}

		public ITexture2D Texture {
			get {
				lock (instanceMutationLock) {
					return texture;
				}
			}
			set {
				lock (instanceMutationLock) {
					texture = value;
					if (texture != null) {
						using (RenderingModule.RenderStateBarrier.AcquirePermit()) {
							if (texView != null) texView.Dispose();
							texView = texture.CreateView();
							material.SetMaterialResource(fsTexBinding, texView);
						}
						SetModelTransform();	
					}
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
					SetModelTransform();
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
					SetModelTransform();
				}
			}
		}

		public float Rotation {
			get {
				lock (instanceMutationLock) {
					return rotation;
				}
			}
			set {
				lock (instanceMutationLock) {
					rotation = value;
					SetModelTransform();
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
					SetModelTransform();
				}
			}
		}

		public Vector4 Color {
			get {
				lock (instanceMutationLock) {
					return color;
				}
			}
			set {
				using (RenderingModule.RenderStateBarrier.AcquirePermit(withLock: instanceMutationLock)) {
					color = value;
					material.SetMaterialConstantValue(fsTexColorMultiplyBinding, color);
				}
			}
		}

		public AspectRatioCorrectionStrategy AspectCorrectionStrategy {
			get {
				lock (instanceMutationLock) {
					return aspectCorrectionStrategy;
				}
			}
			set {
				lock (instanceMutationLock) {
					aspectCorrectionStrategy = value;
					SetModelTransform();
				}
			}
		}

		public int ZIndex {
			get {
				lock (instanceMutationLock) {
					return material.ZIndex;
				}
			}
			set {
				lock (instanceMutationLock) {
					material.ZIndex = value;
				}
			}
		}

		object IHUDObject.GetValueAsObject() {
			lock (instanceMutationLock) {
				return texture;
			}
		}

		public IHUDObject Clone() {
			lock (instanceMutationLock) {
				return new HUDTexture(
					fragmentShader,
					sceneLayer,
					targetViewport
				) {
					Anchoring = anchoring,
					AnchorOffset = anchorOffset,
					AspectCorrectionStrategy = aspectCorrectionStrategy,
					Color = color,
					Rotation = rotation,
					Scale = scale,
					Texture = texture,
					ZIndex = material.ZIndex
				};
			}
		}

		public void CopyTo(IHUDObject targetObj) {
			lock (instanceMutationLock) {
				HUDTexture targetAsTex = (HUDTexture) targetObj;
				targetAsTex.Anchoring = anchoring;
				targetAsTex.AnchorOffset = anchorOffset;
				targetAsTex.AspectCorrectionStrategy = aspectCorrectionStrategy;
				targetAsTex.Color = color;
				targetAsTex.Rotation = rotation;
				targetAsTex.Scale = scale;
				targetAsTex.Texture = texture;
				targetAsTex.ZIndex = material.ZIndex;
			}
		}

		public HUDTexture(FragmentShader fragmentShader, SceneLayer targetLayer, SceneViewport targetViewport) {
			lock (staticMutationLock) {
				if (hudTextureModel == null) throw new InvalidOperationException("HUD Texture Model must be set before creating any HUDTexture objects.");
			}
			this.targetViewport = targetViewport;
			this.material = new Material("HUDTexture Mat", fragmentShader);
			this.curModelInstance = targetLayer.CreateModelInstance(hudTextureModel.Value, material, Transform.DEFAULT_TRANSFORM);
			this.fsTexColorMultiplyBinding = (ConstantBufferBinding) fragmentShader.GetBindingByIdentifier(HUDFS_TEX_PROPS_CBB_NAME);
			this.fsTexBinding = (ResourceViewBinding) fragmentShader.GetBindingByIdentifier(HUDFS_TEX_BINDING_NAME);
			material.SetMaterialConstantValue(fsTexColorMultiplyBinding, color);
			targetViewport.TargetWindow.WindowResized += WindowResize;

			this.fragmentShader = fragmentShader;
			this.sceneLayer = targetLayer;
		}

		public static void SetHUDTextureModel(ModelHandle modelHandle) {
			lock (staticMutationLock) {
				hudTextureModel = modelHandle;
			}
		}

		public void Dispose() {
			lock (instanceMutationLock) {
				curModelInstance.Dispose();
				if (texView != null) texView.Dispose();
				material.Dispose();
				targetViewport.TargetWindow.WindowResized -= WindowResize;
			}
		}

		private void WindowResize(Window window, uint width, uint height) {
			lock (instanceMutationLock) SetModelTransform();
		}

		public void AdjustAlpha(float alpha) {
			using (RenderingModule.RenderStateBarrier.AcquirePermit(withLock: instanceMutationLock)) {
				color = new Vector4(color, w: (float) MathUtils.Clamp(color.W + alpha, 0f, 1f));
				material.SetMaterialConstantValue(fsTexColorMultiplyBinding, color);
			}
		}

		public void SetAlpha(float alpha) {
			using (RenderingModule.RenderStateBarrier.AcquirePermit(withLock: instanceMutationLock)) {
				color = new Vector4(color, w: (float) MathUtils.Clamp(alpha, 0f, 1f));
				material.SetMaterialConstantValue(fsTexColorMultiplyBinding, color);
			}
		}

		public void SetTextureFromCachedView(ShaderResourceView texView) {
			try {
				lock (instanceMutationLock) {
					using (RenderingModule.RenderStateBarrier.AcquirePermit()) {
						if (this.texView != null) this.texView.Dispose();
						this.texView = null;
						texture = (ITexture2D) texView.Resource;
						material.SetMaterialResource(fsTexBinding, texView);
					}
					SetModelTransform();
				}
			}
			catch (Exception e) {
				Console.WriteLine(e);
			}
		}

		private void SetModelTransform() {
			if (texture == null) return;

			Vector2 vpSizePixels = targetViewport.SizePixels;
			Vector2 originalTexSizePixels = new Vector2(texture.Width, texture.Height);
			Vector2 desiredTexSizePixels = vpSizePixels.Scale(scale);
			Vector2 texScale = desiredTexSizePixels.Scale(1f / originalTexSizePixels);
			switch (aspectCorrectionStrategy) {
				case AspectRatioCorrectionStrategy.PreserveHorizontalScaling:
					desiredTexSizePixels = originalTexSizePixels * texScale.X;
				break;
				case AspectRatioCorrectionStrategy.PreserveVerticalScaling:
					desiredTexSizePixels = originalTexSizePixels * texScale.Y;
				break;
				case AspectRatioCorrectionStrategy.UseBestUniformScaling:
					desiredTexSizePixels = originalTexSizePixels * ((texScale.X + texScale.Y) * 0.5f);
				break;
			}

			float posX, posY;

			switch ((int) anchoring & 0xF0) { // Vertical bits
				case 0x10: // 1 == "Bottom"
					posY = anchorOffset.Y * vpSizePixels.Y;
					break;
				case 0x20: // 2 == "Centered"
					posY = (vpSizePixels.Y * 0.5f) - (desiredTexSizePixels.Y * 0.5f) - (anchorOffset.Y * vpSizePixels.Y);
					break;
				default: // 0 == "Top"
					posY = (vpSizePixels.Y - desiredTexSizePixels.Y) - vpSizePixels.Y * anchorOffset.Y;
					break;
			}

			switch ((int) anchoring & 0xF) { // Horizontal bits
				case 0x1: // 1 == "Right"
					posX = (vpSizePixels.X - desiredTexSizePixels.X) - vpSizePixels.X * anchorOffset.X;
					break;
				case 0x2: // 2 == "Centered"
					posX = (vpSizePixels.X * 0.5f) - (desiredTexSizePixels.X * 0.5f) + (anchorOffset.X * vpSizePixels.X);
					break;
				default: // 0 == "Left"
					posX = anchorOffset.X * vpSizePixels.X;
					break;
			}

			Vector3 pos = new Vector3(posX, posY, 0f);
			using (RenderingModule.RenderStateBarrier.AcquirePermit()) {
				curModelInstance.Transform = new Transform(
					new Vector3(desiredTexSizePixels.X, desiredTexSizePixels.Y, 1f),
					Quaternion.IDENTITY,
					pos
				).RotateAround(pos + desiredTexSizePixels * 0.5f, Quaternion.FromAxialRotation(Vector3.FORWARD, rotation));
			}
		}
	}
}