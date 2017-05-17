// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 22 04 2015 at 14:17 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;

namespace Ophidian.Losgap.Rendering {
	public sealed partial class DLLightPass : RenderPass {
		private readonly List<Light> addedLights = new List<Light>();
		private DLGeometryPass geometryPass;
		private VertexShader dlLightVS;
		private FragmentShader dlLightFS;
		private FragmentShader dlFinalFS, outliningShader;
		private FragmentShader bloomHShader, bloomVShader;
		private FragmentShader copyShader, copyReverseShader;
		private FragmentShader blurShader;
		private FragmentShader dofShader;
		private ShaderResourcePackage vsResPackage;
		private ShaderResourcePackage fsResPackage;
		private ShaderResourcePackage fsUnbindResPackage;
		private ShaderResourcePackage finalizationShaderResPackage;
		private ShaderResourcePackage finalizationShaderUnbindResPackage;
		private ShaderResourcePackage outliningShaderResPackage;
		private ShaderResourcePackage outliningShaderUnbindResPackage;
		private ShaderResourcePackage bloomHShaderResPackage, bloomVShaderResPackage;
		private ShaderResourcePackage bloomHShaderUnbindResPackage, bloomVShaderUnbindResPackage;
		private ShaderResourcePackage copyShaderResPackage;
		private ShaderResourcePackage copyShaderUnbindResPackage;
		private ShaderResourcePackage copyDoFShaderResPackage;
		private ShaderResourcePackage copyDoFShaderUnbindResPackage;
		private ShaderResourcePackage copyReverseShaderResPackage;
		private ShaderResourcePackage copyReverseShaderUnbindResPackage;
		private ShaderResourcePackage blurShaderResPackage;
		private ShaderResourcePackage blurShaderUnbindResPackage;
		private ShaderResourcePackage dofShaderResPackage;
		private ShaderResourcePackage dofShaderUnbindResPackage;
		private bool presentAfterPass = true;

		public bool PresentAfterPass {
			get {
				lock (InstanceMutationLock) {
					return presentAfterPass;
				}
			}
			set {
				lock (InstanceMutationLock) {
					presentAfterPass = value;
				}
			}
		}

		public DLGeometryPass GeometryPass {
			get {
				lock (InstanceMutationLock) {
					return geometryPass;
				}
			}
			set {
				using (RenderingModule.RenderStateBarrier.AcquirePermit(withLock: InstanceMutationLock)) {
					geometryPass = value;
				}
			}
		}

		public VertexShader DLLightVertexShader {
			get {
				lock (InstanceMutationLock) {
					return dlLightVS;
				}
			}
			set {
				using (RenderingModule.RenderStateBarrier.AcquirePermit(withLock: InstanceMutationLock)) {
					dlLightVS = value;
					vsResPackage = new ShaderResourcePackage();
					SetVSResources();
				}
			}
		}

		public FragmentShader DLLightFragmentShader {
			get {
				lock (InstanceMutationLock) {
					return dlLightFS;
				}
			}
			set {
				using (RenderingModule.RenderStateBarrier.AcquirePermit(withLock: InstanceMutationLock)) {
					dlLightFS = value;
					fsResPackage = new ShaderResourcePackage();
					fsUnbindResPackage = new ShaderResourcePackage();
					SetFSResources();
				}
			}
		}

		public FragmentShader DLLightFinalizationShader {
			get {
				lock (InstanceMutationLock) {
					return dlFinalFS;
				}
			}
			set {
				using (RenderingModule.RenderStateBarrier.AcquirePermit(withLock: InstanceMutationLock)) {
					dlFinalFS = value;
					finalizationShaderResPackage = new ShaderResourcePackage();
					finalizationShaderUnbindResPackage = new ShaderResourcePackage();
					SetFSResources();
				}
			}
		}

		public FragmentShader OutliningShader {
			get {
				lock (InstanceMutationLock) {
					return outliningShader;
				}
			}
			set {
				using (RenderingModule.RenderStateBarrier.AcquirePermit(withLock: InstanceMutationLock)) {
					outliningShader = value;
					outliningShaderResPackage = new ShaderResourcePackage();
					outliningShaderUnbindResPackage = new ShaderResourcePackage();
					SetFSResources();
				}
			}
		}

		public FragmentShader BloomHShader {
			get {
				lock (InstanceMutationLock) {
					return bloomHShader;
				}
			}
			set {
				using (RenderingModule.RenderStateBarrier.AcquirePermit(withLock: InstanceMutationLock)) {
					bloomHShader = value;
					bloomHShaderResPackage = new ShaderResourcePackage();
					bloomHShaderUnbindResPackage = new ShaderResourcePackage();
					SetFSResources();
				}
			}
		}

		public FragmentShader BloomVShader {
			get {
				lock (InstanceMutationLock) {
					return bloomVShader;
				}
			}
			set {
				using (RenderingModule.RenderStateBarrier.AcquirePermit(withLock: InstanceMutationLock)) {
					bloomVShader = value;
					bloomVShaderResPackage = new ShaderResourcePackage();
					bloomVShaderUnbindResPackage = new ShaderResourcePackage();
					SetFSResources();
				}
			}
		}

		public FragmentShader CopyShader {
			get {
				lock (InstanceMutationLock) {
					return copyShader;
				}
			}
			set {
				using (RenderingModule.RenderStateBarrier.AcquirePermit(withLock: InstanceMutationLock)) {
					copyShader = value;
					copyShaderResPackage = new ShaderResourcePackage();
					copyShaderUnbindResPackage = new ShaderResourcePackage();
					copyDoFShaderResPackage = new ShaderResourcePackage();
					copyDoFShaderUnbindResPackage = new ShaderResourcePackage();
					SetFSResources();
				}
			}
		}

		public FragmentShader CopyReverseShader {
			get {
				lock (InstanceMutationLock) {
					return copyReverseShader;
				}
			}
			set {
				using (RenderingModule.RenderStateBarrier.AcquirePermit(withLock: InstanceMutationLock)) {
					copyReverseShader = value;
					copyReverseShaderResPackage = new ShaderResourcePackage();
					copyReverseShaderUnbindResPackage = new ShaderResourcePackage();
					SetFSResources();
				}
			}
		}

		public FragmentShader BlurShader {
			get {
				lock (InstanceMutationLock) {
					return blurShader;
				}
			}
			set {
				using (RenderingModule.RenderStateBarrier.AcquirePermit(withLock: InstanceMutationLock)) {
					blurShader = value;
					blurShaderResPackage = new ShaderResourcePackage();
					blurShaderUnbindResPackage = new ShaderResourcePackage();
					SetFSResources();
				}
			}
		}

		public FragmentShader DoFShader {
			get {
				lock (InstanceMutationLock) {
					return dofShader;
				}
			}
			set {
				using (RenderingModule.RenderStateBarrier.AcquirePermit(withLock: InstanceMutationLock)) {
					dofShader = value;
					dofShaderResPackage = new ShaderResourcePackage();
					dofShaderUnbindResPackage = new ShaderResourcePackage();
					SetFSResources();
				}
			}
		}

		public override bool IsValid {
			get {
				return !IsDisposed
					&& geometryPass != null
					&& !geometryPass.IsDisposed
					&& dlLightVS != null
					&& !dlLightVS.IsDisposed
					&& dlLightFS != null
					&& !dlLightFS.IsDisposed
					&& dlFinalFS != null
					&& !dlFinalFS.IsDisposed
					&& copyShader != null
					&& !copyShader.IsDisposed
					&& copyReverseShader != null
					&& !copyReverseShader.IsDisposed;
			}
		}

		public DLLightPass(string name) : base(name) { }

		public void SetLensProperties(float focalDistance, float maxBlurDistance) {
			lock (InstanceMutationLock) {
				Vector4 lensProps = new Vector4(
					this.geometryPass.Output.NearPlaneDist,
					this.geometryPass.Output.FarPlaneDist,
					focalDistance,
					maxBlurDistance
				);
				unsafe {
					((ConstantBufferBinding) dofShader.GetBindingByIdentifier("LensProperties")).SetValue((byte*) (&lensProps));
				}
			}
		}

		public void AddLight(Light light) {
			Assure.NotNull(light);
			lock (InstanceMutationLock) {
				Assure.False(addedLights.Contains(light), "Light is already added!");
				if (addedLights.Count == MAX_DYNAMIC_LIGHTS) {
					//Logger.Warn("Tried to add a light when max lights already exceeded.");
					return;
				}
				addedLights.Add(light);
			}
		}

		public void RemoveLight(Light light) {
			Assure.NotNull(light);
			lock (InstanceMutationLock) {
				addedLights.Remove(light);
			}
		}

		public void ClearLights() {
			lock (InstanceMutationLock) {
				addedLights.Clear();
			}
		}

		public const int MAX_DYNAMIC_LIGHTS = 128;
	}
}