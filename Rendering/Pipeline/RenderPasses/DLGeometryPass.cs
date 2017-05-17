// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 22 04 2015 at 14:17 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;

namespace Ophidian.Losgap.Rendering {
	public sealed partial class DLGeometryPass : RenderPass {
		private ModelHandle __EGGHACK_MH; // We only render the egg rotation, rather than use it in 
		private Quaternion __EGGHACK_ROT = Quaternion.IDENTITY; // the physics simulation, because bullet sucks basically
		private ModelHandle __VEGG_MH;
		private readonly Dictionary<ModelInstanceHandle, Quaternion> __VEGG_MIH_ARR = new Dictionary<ModelInstanceHandle, Quaternion>();
		internal const int NUM_GBUFFER_TEXTURES = 5;
		private readonly List<SceneLayer> addedSceneLayers = new List<SceneLayer>();
		private readonly Texture2D<TexelFormat.RGBA32Float>[] gBuffer = new Texture2D<TexelFormat.RGBA32Float>[NUM_GBUFFER_TEXTURES];
		private readonly RenderTargetView[] gBufferViews = new RenderTargetView[NUM_GBUFFER_TEXTURES];
		private Camera input;
		private SceneViewport output;
		private Dictionary<GeometryCache, VertexShader> deferredGeometryVertexShaders = new Dictionary<GeometryCache, VertexShader>();
		private RasterizerState rsState;
		private DepthStencilState dsState;
		private BlendState blendState;
		private ShadowPass shadowPass;
		private bool clearOutputBeforePass;

		public BlendState BlendState {
			get {
				lock (InstanceMutationLock) {
					return blendState;
				}
			}
			set {
				lock (InstanceMutationLock) {
					blendState = value;
				}
			}
		}

		public ModelHandle _EGGHACK_MH {
			set {
				lock (InstanceMutationLock) {
					__EGGHACK_MH = value;
				}
			}
		}

		public ModelHandle _VEGG_MH {
			set {
				lock (InstanceMutationLock) {
					__VEGG_MH = value;
				}
			}
		}

		public Quaternion _EGGHACK_ROT {
			get {
				lock (InstanceMutationLock) {
					return __EGGHACK_ROT;
				}
			}
			set {
				lock (InstanceMutationLock) {
					__EGGHACK_ROT = value;
				}
			}
		}

		public void _SET_VEGG_INST(IEnumerable<ModelInstanceHandle> instances) {
			lock (InstanceMutationLock) {
				__VEGG_MIH_ARR.Clear();
				foreach (ModelInstanceHandle veggInst in instances) {
					__VEGG_MIH_ARR.Add(veggInst, Quaternion.IDENTITY);
				}
			}
		}

		public void _ADD_VEGG_ROT(ModelInstanceHandle mih, Quaternion rot) {
			lock (InstanceMutationLock) {
				__VEGG_MIH_ARR[mih] *= rot;
			}
		}

		public bool ClearOutputBeforePass {
			get {
				lock (InstanceMutationLock) {
					return clearOutputBeforePass;
				}
			}
			set {
				lock (InstanceMutationLock) {
					clearOutputBeforePass = value;
				}
			}
		}

		public Camera Input {
			get {
				lock (InstanceMutationLock) {
					return input;
				}
			}
			set {
				using (RenderingModule.RenderStateBarrier.AcquirePermit(withLock: InstanceMutationLock)) {
					input = value;
				}
			}
		}

		public SceneViewport Output {
			get {
				lock (InstanceMutationLock) {
					return output;
				}
			}
			set {
				using (RenderingModule.RenderStateBarrier.AcquirePermit(withLock: InstanceMutationLock)) {
					output = value;
				}
			}
		}

		public ShadowPass ShadowPass {
			get {
				lock (InstanceMutationLock) {
					return shadowPass;
				}
			}
			set {
				lock (InstanceMutationLock) {
					shadowPass = value;
				}
			}
		}

		/// <summary>
		/// The rasterizer state that will be used to generate pixels from rendered geometry.
		/// </summary>
		public RasterizerState RasterizerState {
			get {
				lock (InstanceMutationLock) {
					return rsState;
				}
			}
			set {
				using (RenderingModule.RenderStateBarrier.AcquirePermit(withLock: InstanceMutationLock)) {
					rsState = value;
				}
			}
		}

		/// <summary>
		/// The depth/stencil state that will be used to select fragments for overlapping geometry (amongst other things).
		/// </summary>
		public DepthStencilState DepthStencilState {
			get {
				lock (InstanceMutationLock) {
					return dsState;
				}
			}
			set {
				using (RenderingModule.RenderStateBarrier.AcquirePermit(withLock: InstanceMutationLock)) {
					dsState = value;
				}
			}
		}

		public override bool IsValid {
			get {
				Vector2 outputSizePixels = output.SizePixels;
				bool noDisposedLayers = true;
				for (int i = 0; i < addedSceneLayers.Count; ++i) {
					if (addedSceneLayers[i].IsDisposed) noDisposedLayers = false;
				}
				return !IsDisposed
					&& addedSceneLayers.Count > 0
					&& noDisposedLayers
					&& input != null
					&& !input.IsDisposed
					&& output != null
					&& !output.IsDisposed
					&& outputSizePixels.X > 0f
					&& outputSizePixels.Y > 0f
					&& rsState != null
					&& !rsState.IsDisposed
					&& dsState != null
					&& !dsState.IsDisposed;
			}
		}

		public DLGeometryPass(string name) : base(name) {
			setUpCacheForLocalThreadAct = SetUpCacheForLocalThread;
			renderCacheIterateMatAct = RenderCache_IterateMaterial;
			setInstanceBufferAndFlushCommandsAct = () => {
				// Unbind shadow buffer
				QueueShaderSwitch(geomFSWithShadowSupport);
				QueueShaderResourceUpdate(geomFSWithShadowSupport, geomFSShadowUnbindPackage);
				SetInstanceBufferAndFlushCommands();
			};
		}

		public void SetVSForCache(GeometryCache cache, VertexShader vs) {
			Assure.NotNull(cache);
			using (RenderingModule.RenderStateBarrier.AcquirePermit(withLock: InstanceMutationLock)) {
				if (vs == null) deferredGeometryVertexShaders.Remove(cache);
				else deferredGeometryVertexShaders[cache] = vs;
			}
		}

		public void AddLayer(SceneLayer layer) {
			Assure.NotNull(layer);
			using (RenderingModule.RenderStateBarrier.AcquirePermit(withLock: InstanceMutationLock)) {
				if (addedSceneLayers.Contains(layer)) throw new InvalidOperationException("Given layer has already been added.");
				addedSceneLayers.Add(layer);
			}
		}

		public void RemoveLayer(SceneLayer layer) {
			Assure.NotNull(layer);
			using (RenderingModule.RenderStateBarrier.AcquirePermit(withLock: InstanceMutationLock)) {
				if (!addedSceneLayers.Remove(layer)) throw new InvalidOperationException("Given layer is not already added.");
			}
		}

		public void ClearLayers() {
			using (RenderingModule.RenderStateBarrier.AcquirePermit(withLock: InstanceMutationLock)) {
				addedSceneLayers.Clear();
			}
		}

		internal void GetLightPassParameters(out Texture2D<TexelFormat.RGBA32Float>[] gBuffer, out Camera input, out SceneViewport output) {
			gBuffer = this.gBuffer;
			input = this.input;
			output = this.output;
		}
	}
}