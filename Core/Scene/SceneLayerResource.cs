// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 06 01 2015 at 10:29 by Ben Bowen

using System;
using System.Collections.Generic;

namespace Ophidian.Losgap {
	/// <summary>
	/// Represents a resource attached to a scene layer.
	/// </summary>
	public abstract class SceneLayerResource {
		/// <summary>
		/// The <see cref="SceneLayer"/> that this resource is attached to.
		/// </summary>
		public readonly SceneLayer OwningLayer;

		/// <summary>
		/// Creates this scene layer resource, setting the given <see cref="OwningLayer"/>.
		/// </summary>
		/// <param name="owningLayer">The <see cref="SceneLayer"/> that this resource is being attached to. Never null.</param>
		protected SceneLayerResource(SceneLayer owningLayer) {
			if (owningLayer == null) throw new ArgumentNullException("owningLayer");
			if (owningLayer.IsDisposed) throw new ObjectDisposedException("owningLayer");
			OwningLayer = owningLayer;
			Logger.Debug("Created new " + this + ".");
		}

		/// <summary>
		/// Called when the <see cref="OwningLayer"/> is enabled.
		/// </summary>
		protected internal abstract void Enable();
		/// <summary>
		/// Called when the <see cref="OwningLayer"/> is disabled.
		/// </summary>
		protected internal abstract void Disable();
		/// <summary>
		/// Called when the <see cref="OwningLayer"/> is disposed.
		/// </summary>
		protected internal abstract void Dispose();

		/// <summary>
		/// Returns a string representing the state/values of this instance.
		/// </summary>
		/// <returns>
		/// A new string with details of this instance.
		/// </returns>
		public override string ToString() {
			return OwningLayer + " resource '" + GetType().Name + "'";
		}
	}
}