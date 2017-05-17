// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 06 01 2015 at 17:44 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// A class containing <see cref="RenderingModule"/>-specific extensions to <see cref="SceneLayer"/>s.
	/// </summary>
	public static unsafe class SceneLayerRenderingExtensions {
		private const string RENDERING_ENABLED_PROP = "Rendering.RenderingEnabled";

		/// <summary>
		/// Enable or disable rendering all objects added to this scene layer.
		/// </summary>
		/// <param name="this">The extended SceneLayer.</param>
		/// <param name="enabled">True to enable rendering, false to disable.</param>
		public static void SetRenderingEnabled(this SceneLayer @this, bool enabled) {
			if (@this == null) throw new ArgumentNullException("this");
			
			@this[RENDERING_ENABLED_PROP] = enabled;
		}

		/// <summary>
		/// Returns whether or not rendering is currently enabled for this scene layer.
		/// </summary>
		/// <param name="this">The extended SceneLayer.</param>
		/// <returns>True if rendering is currently enabled, false if not.</returns>
		public static bool GetRenderingEnabled(this SceneLayer @this) {
			if (@this == null) throw new ArgumentNullException("this");

			return (bool) @this[RENDERING_ENABLED_PROP];
		}

		/// <summary>
		/// Creates a new instance of the model specified by the given <paramref name="modelHandle"/>.
		/// </summary>
		/// <param name="this">The extended <see cref="SceneLayer"/>.</param>
		/// <param name="modelHandle">The handle to the model that you want to create an instance of.</param>
		/// <param name="material">The initial <see cref="Material"/> to apply to the newly created instance. Must not be null or disposed.</param>
		/// <param name="initialTransform">The initial <see cref="Transform"/> to be appled to the newly created instance.</param>
		/// <returns>A reserved <see cref="ModelInstanceHandle"/>.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0",
			Justification = "Param is checked by assurance."), 
		MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ModelInstanceHandle CreateModelInstance(this SceneLayer @this, ModelHandle modelHandle, Material material, 
			Transform initialTransform) {
			Assure.NotNull(@this);
			return @this.GetResource<LayerModelInstanceManager>().CreateInstance(modelHandle, material, initialTransform);
		}
	}
}