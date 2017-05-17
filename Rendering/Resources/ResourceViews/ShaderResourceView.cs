// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 12 11 2014 at 10:04 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// A view of a resource that permits read-only access from one or more shader programs, and other stages of the graphics pipeline.
	/// </summary>
	public abstract class ShaderResourceView : BaseResourceView {
		internal new ShaderResourceViewHandle ResourceViewHandle {
			get {
				return (ShaderResourceViewHandle) base.ResourceViewHandle;
			}
		}

		internal ShaderResourceView(ShaderResourceViewHandle resourceViewHandle, IResource resource) : base(resourceViewHandle, resource) { }
	}

	/// <summary>
	/// A <see cref="ShaderResourceView"/> to a <see cref="IBuffer"/>.
	/// </summary>
	public sealed class ShaderBufferResourceView : ShaderResourceView {
		/// <summary>
		/// The element in the underlying <see cref="Resource"/> that will be returned when accessing this view at index 0.
		/// </summary>
		public readonly uint FirstElementIndex;
		/// <summary>
		/// The number of elements exposed from the underlying <see cref="Resource"/> that will be visible through this view.
		/// </summary>
		public readonly uint NumElements;

		/// <summary>
		/// The resource that this view is connected to. 
		/// </summary>
		public new IBuffer Resource {
			get {
				return (IBuffer) base.Resource;
			}
		}

		internal ShaderBufferResourceView(ShaderResourceViewHandle resourceViewHandle, IBuffer resource, 
			uint firstElementIndex, uint numElements)
			: base(resourceViewHandle, resource) {
			FirstElementIndex = firstElementIndex;
			NumElements = numElements;
		}
	}

	/// <summary>
	/// A <see cref="ShaderResourceView"/> to a <see cref="ITexture"/>.
	/// </summary>
	public sealed class ShaderTextureResourceView : ShaderResourceView {
		/// <summary>
		/// The index of the first mip level to be exposed from the <see cref="Resource"/> at index 0 of this view.
		/// </summary>
		public readonly uint FirstMipIndex;
		/// <summary>
		/// The number of mips in the <see cref="Resource"/> exposed via this view.
		/// </summary>
		public readonly uint NumMips;

		/// <summary>
		/// The resource that this view is connected to. 
		/// </summary>
		public new ITexture Resource {
			get {
				return (ITexture) base.Resource;
			}
		}

		internal ShaderTextureResourceView(ShaderResourceViewHandle resourceViewHandle, ITexture resource,
			uint firstMipIndex, uint numMips)
			: base(resourceViewHandle, resource) {
			FirstMipIndex = firstMipIndex;
			NumMips = numMips;
		}
	}

	/// <summary>
	/// A <see cref="ShaderResourceView"/> to a <see cref="ITextureArray"/>.
	/// </summary>
	public sealed class ShaderTextureArrayResourceView : ShaderResourceView {
		/// <summary>
		/// The index of the first mip level to be exposed from each texture in the <see cref="Resource"/> at mip-index 0 of this view.
		/// </summary>
		public readonly uint FirstMipIndex;
		/// <summary>
		/// The number of mips exposed in the <see cref="Resource"/> to this view.
		/// </summary>
		public readonly uint NumMips;
		/// <summary>
		/// The index of the first texture in the <see cref="Resource"/> to be exposed at index 0 of this view.
		/// </summary>
		public readonly uint FirstArrayElementIndex;
		/// <summary>
		/// The number of textures exposed via this view in the <see cref="Resource"/>.
		/// </summary>
		public readonly uint NumArrayElements;

		/// <summary>
		/// The resource that this view is connected to. 
		/// </summary>
		public new ITextureArray Resource {
			get {
				return (ITextureArray) base.Resource;
			}
		}

		internal ShaderTextureArrayResourceView(ShaderResourceViewHandle resourceViewHandle, ITextureArray resource,
			uint firstMipIndex, uint numMips, uint firstArrayElementIndex, uint numArrayElements)
			: base(resourceViewHandle, resource) {
			FirstMipIndex = firstMipIndex;
			NumMips = numMips;
			FirstArrayElementIndex = firstArrayElementIndex;
			NumArrayElements = numArrayElements;
		}
	}
}