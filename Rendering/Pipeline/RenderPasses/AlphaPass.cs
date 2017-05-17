// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 06 01 2015 at 16:25 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

namespace Ophidian.Losgap.Rendering {
	public sealed partial class AlphaPass : RenderPass {
		private readonly List<SceneLayer> addedSceneLayers = new List<SceneLayer>();
		private Camera input;
		private SceneViewport output;
		private VertexShader vertexShader;
		private RasterizerState rsState;
		private DepthStencilState dsState;
		private BlendState blendState;
		private DLGeometryPass mainGeomPass;
		private bool presentAfterPass = false;

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

		public DLGeometryPass MainGeomPass {
			get {
				lock (InstanceMutationLock) {
					return mainGeomPass;
				}
			}
			set {
				lock (InstanceMutationLock) {
					mainGeomPass = value;
				}
			}
		}

		/// <summary>
		/// Whether or not the configuration of input/output parameters on this render pass is valid.
		/// If not valid, this pass will not be executed, and will be skipped on each frame until it is once again in a valid state.
		/// </summary>
		public override bool IsValid {
			get {
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
					&& vertexShader != null
					&& !vertexShader.IsDisposed
					&& rsState != null
					&& !rsState.IsDisposed
					&& dsState != null
					&& !dsState.IsDisposed
					&& blendState != null
					&& !blendState.IsDisposed;
			}
		}

		/// <summary>
		/// The place in the scene that the pass will be rendered from.
		/// </summary>
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

		/// <summary>
		/// The viewport that the scene will be rendered to.
		/// </summary>
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

		/// <summary>
		/// The vertex shader that will be used to draw the geometry for rendered model instances.
		/// </summary>
		public VertexShader VertexShader {
			get {
				lock (InstanceMutationLock) {
					return vertexShader;
				}
			}
			set {
				using (RenderingModule.RenderStateBarrier.AcquirePermit(withLock: InstanceMutationLock)) {
					vertexShader = value;
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

		/// <summary>
		/// Creates a new model instance pass.
		/// </summary>
		/// <param name="name">The name to give this pass. Must not be null.</param>
		public AlphaPass(string name) : base(name) {
			setUpCacheForLocalThreadAct = SetUpCacheForLocalThread;
			renderCacheIterateMatAct = RenderCache_IterateMaterial;
			setInstanceBufferAndFlushCommandsAct = SetInstanceBufferAndFlushCommands;
		}

		/// <summary>
		/// Add a scene layer to the rendering for this pass. All model instances created on the given <paramref name="layer"/>
		/// (e.g. with <see cref="SceneLayerRenderingExtensions.CreateModelInstance"/>) will be rendered (unless filtered out, e.g. by frustum
		/// culling).
		/// </summary>
		/// <param name="layer">The layer to add. Must not be null.</param>
		/// <exception cref="InvalidOperationException">Thrown if the given <paramref name="layer"/> is already currently added to this pass.
		/// </exception>
		public void AddLayer(SceneLayer layer) {
			Assure.NotNull(layer);
			using (RenderingModule.RenderStateBarrier.AcquirePermit(withLock: InstanceMutationLock)) {
				if (addedSceneLayers.Contains(layer)) throw new InvalidOperationException("Given layer has already been added.");
				addedSceneLayers.Add(layer);
			}
		}

		/// <summary>
		/// Remove a scene layer from the rendering for this pass. Model instances created on the given <paramref name="layer"/>
		/// will no longer be rendered.
		/// </summary>
		/// <param name="layer">The layer to remove. Must not be null.</param>
		/// <exception cref="InvalidOperationException">Thrown if the given <paramref name="layer"/> is not currently added to this pass.
		/// </exception>
		public void RemoveLayer(SceneLayer layer) {
			Assure.NotNull(layer);
			using (RenderingModule.RenderStateBarrier.AcquirePermit(withLock: InstanceMutationLock)) {
				if (!addedSceneLayers.Remove(layer)) throw new InvalidOperationException("Given layer is not already added.");
			}
		}

		/// <summary>
		/// Remove all scene layers from the rendering for this pass. Layers can then be re-added with <see cref="AddLayer"/>.
		/// </summary>
		public void ClearLayers() {
			using (RenderingModule.RenderStateBarrier.AcquirePermit(withLock: InstanceMutationLock)) {
				addedSceneLayers.Clear();
			}
		}
	}
}