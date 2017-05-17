// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 15 01 2015 at 14:02 by Ben Bowen

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// A <see cref="SceneLayerResource"/> added automatically to every <see cref="SceneLayer"/> that enables and tracks creation of
	/// <see cref="ModelInstanceData"/>s.
	/// </summary>
	public unsafe sealed class LayerModelInstanceManager : SceneLayerResource {
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
			Justification = "Code is called reflectively.")]
		private LayerModelInstanceManager(SceneLayer owningLayer) : base(owningLayer) { }

		/// <summary>
		/// Creates a new instance of a model specified by the given <paramref name="modelHandle"/>.
		/// </summary>
		/// <param name="modelHandle">The handle to the model that you want to create an instance of.</param>
		/// <param name="material">The initial <see cref="Material"/> to apply to the newly created instance. Must not be null or disposed.</param>
		/// <param name="initialTransform">The initial <see cref="Transform"/> to be appled to the newly created instance.</param>
		/// <returns>A reserved <see cref="ModelInstanceHandle"/>.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1",
			Justification = "Member is validated by assurances.")]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ModelInstanceHandle CreateInstance(ModelHandle modelHandle, Material material, Transform initialTransform) {
			uint modelIndex = modelHandle.ModelIndex;
			Assure.NotNull(material);
			Assure.False(material.IsDisposed, "Given material was disposed!");
			return GeometryCache.GetCacheByID(modelHandle.GeoCacheID).AllocInstance(modelIndex, OwningLayer.Index, material.Index, initialTransform);
		}

		/// <summary>
		/// Called when the <see cref="SceneLayerResource.OwningLayer"/> is enabled.
		/// </summary>
		protected override void Enable() {
			// Do nothing
		}

		/// <summary>
		/// Called when the <see cref="SceneLayerResource.OwningLayer"/> is disabled.
		/// </summary>
		protected override void Disable() {
			// Do nothing
		}

		/// <summary>
		/// Called when the <see cref="SceneLayerResource.OwningLayer"/> is disposed.
		/// </summary>
		protected override void Dispose() {
			// Do nothing
		}
	}
}