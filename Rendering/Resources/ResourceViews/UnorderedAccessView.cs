// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 12 11 2014 at 10:04 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// A view of a resource that permits unordered (random) read/write access to that resource through one or more shader programs.
	/// </summary>
	public abstract class UnorderedAccessView : BaseResourceView {
		internal new UnorderedAccessViewHandle ResourceViewHandle {
			get {
				return (UnorderedAccessViewHandle) base.ResourceViewHandle;
			}
		}

		internal UnorderedAccessView(UnorderedAccessViewHandle resourceViewHandle, IResource resource) : base(resourceViewHandle, resource) { }
	}

	/// <summary>
	/// An <see cref="UnorderedAccessView"/> to a <see cref="IBuffer"/>.
	/// </summary>
	public sealed class UnorderedBufferAccessView : UnorderedAccessView {
		/// <summary>
		/// The element in the underlying <see cref="Resource"/> that will be returned when accessing this view at index 0.
		/// </summary>
		public readonly uint FirstElementIndex;
		/// <summary>
		/// The number of elements exposed from the underlying <see cref="Resource"/> that will be visible through this view.
		/// </summary>
		public readonly uint NumElements;
		/// <summary>
		/// Whether or not this view allows append/consume buffer wrappers in shader programs.
		/// </summary>
		public readonly bool AppendConsumeSupport;
		/// <summary>
		/// Whether or not this view includes a 'hidden' counter that enables usage of the <c>IncrementCounter</c> and <c>DecrementCounter</c>
		/// HLSL methods.
		/// </summary>
		public readonly bool IncludesCounter;
		/// <summary>
		/// Whether or not this buffer can be accessed as a raw byte-buffer.
		/// </summary>
		public readonly bool IsRawAccessView;

		/// <summary>
		/// The resource that this view is connected to. 
		/// </summary>
		public new IBuffer Resource {
			get {
				return (IBuffer) base.Resource;
			}
		}

		internal UnorderedBufferAccessView(UnorderedAccessViewHandle resourceViewHandle, IBuffer resource,
			uint firstElementIndex, uint numElements, bool appendConsumeSupport, bool includesCounter, bool isRawAccessView)
			: base(resourceViewHandle, resource) {
			FirstElementIndex = firstElementIndex;
			NumElements = numElements;
			AppendConsumeSupport = appendConsumeSupport;
			IncludesCounter = includesCounter;
			IsRawAccessView = isRawAccessView;
		}
	}

	/// <summary>
	/// An <see cref="UnorderedAccessView"/> to a <see cref="ITexture"/>.
	/// </summary>
	public sealed class UnorderedTextureAccessView : UnorderedAccessView {
		/// <summary>
		/// The mip index of the <see cref="Resource"/> that this view can 'see'.
		/// </summary>
		public readonly uint MipIndex;

		/// <summary>
		/// The resource that this view is connected to. 
		/// </summary>
		public new ITexture Resource {
			get {
				return (ITexture) base.Resource;
			}
		}

		internal UnorderedTextureAccessView(UnorderedAccessViewHandle resourceViewHandle, ITexture resource, uint mipIndex)
			: base(resourceViewHandle, resource) {
				MipIndex = mipIndex;
		}
	}

	/// <summary>
	/// An <see cref="UnorderedAccessView"/> to a <see cref="ITextureArray"/>.
	/// </summary>
	public sealed class UnorderedTextureArrayAccessView : UnorderedAccessView {
		/// <summary>
		/// The mip index of each texture in the <see cref="Resource"/> that this view can 'see'.
		/// </summary>
		public readonly uint MipIndex;
		/// <summary>
		/// The index of the first texture in the <see cref="Resource"/> that will be returned when a shader program accesses index 0 of this view.
		/// </summary>
		public readonly uint FirstArrayElementIndex;
		/// <summary>
		/// The number of textures from the <see cref="Resource"/> visible in this view.
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

		internal UnorderedTextureArrayAccessView(UnorderedAccessViewHandle resourceViewHandle, ITextureArray resource,
			uint mipIndex, uint firstArrayElementIndex, uint numArrayElements)
			: base(resourceViewHandle, resource) {
			MipIndex = mipIndex;
			FirstArrayElementIndex = firstArrayElementIndex;
			NumArrayElements = numArrayElements;
		}
	}
}