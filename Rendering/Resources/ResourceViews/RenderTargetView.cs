// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 12 11 2014 at 10:04 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// Represents a view on to a 2-dimensional texture that can be used for rendering on to.
	/// </summary>
	public sealed class RenderTargetView : BaseResourceView {
		/// <summary>
		/// The mip index of the <see cref="Resource"/> that this view refers to.
		/// </summary>
		public readonly uint MipIndex;

		internal new RenderTargetViewHandle ResourceViewHandle {
			get {
				return (RenderTargetViewHandle) base.ResourceViewHandle;
			}
		}

		/// <summary>
		/// The resource that this view is connected to. 
		/// </summary>
		public new ITexture2D Resource {
			get {
				return (ITexture2D) base.Resource;
			}
		}

		internal RenderTargetView(RenderTargetViewHandle resourceViewHandle, ITexture2D resource, uint mipIndex) : base(resourceViewHandle, resource) {
			MipIndex = mipIndex;
		}
	}
}