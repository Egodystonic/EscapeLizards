// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 27 10 2014 at 11:32 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// An <see cref="IResourceBuilder"/> that can create <see cref="IndexBuffer"/>s.
	/// </summary>
	public sealed class IndexBufferBuilder : BaseResourceBuilder<IndexBufferBuilder, IndexBuffer, ArraySlice<uint>?> {
		private readonly uint length;

		internal IndexBufferBuilder()
			: this(ResourceUsage.Immutable, 0U, null) { }

		internal IndexBufferBuilder(ResourceUsage usage, uint length, ArraySlice<uint>? initialData)
			: base(usage, initialData) {
				this.length = length;
		}

		/// <summary>
		/// Sets the <see cref="ResourceUsage"/> to use when creating the <see cref="IndexBuffer"/>.
		/// See the <see cref="SupportedUsagesAttribute"/> on <see cref="IndexBuffer"/> for a list of permitted usages.
		/// </summary>
		/// <param name="usage">The usage to use/set.</param>
		/// <returns>An identical IndexBufferBuilder with the specified <paramref name="usage"/> parameter.</returns>
		public override IndexBufferBuilder WithUsage(ResourceUsage usage) {
			return new IndexBufferBuilder(usage, length, InitialData);
		}

		/// <summary>
		/// Sets the initial data to use when creating the <see cref="IndexBuffer"/>. Resources with a usage of 
		/// <see cref="ResourceUsage.Immutable"/> must have initial data provided.
		/// </summary>
		/// <param name="initialData">The data to set. 
		/// If not null, the resultant index buffer will have a length equal to the <see cref="ArraySlice{T}.Length"/>
		/// property. This means if you previously set a length with <see cref="WithLength"/>, the value will be overwritten.
		/// If null, no initial data will be set.</param>
		/// <returns>An identical IndexBufferBuilder with the specified <paramref name="initialData"/> parameter.</returns>
		public override IndexBufferBuilder WithInitialData(ArraySlice<uint>? initialData) {
			return new IndexBufferBuilder(Usage, initialData != null ? initialData.Value.Length : length, initialData);
		}

		/// <summary>
		/// Sets the length to use when creating the <see cref="IndexBuffer"/>. If you have previously set initial data with
		/// <see cref="WithInitialData"/>, the data will be dropped unless the given <paramref name="length"/> is equal to the 
		/// initial data length.
		/// </summary>
		/// <param name="length">The buffer length.</param>
		/// <returns>An identical IndexBufferBuilder with the specified buffer <paramref name="length"/>.</returns>
		public IndexBufferBuilder WithLength(uint length) {
			ArraySlice<uint>? newInitData = (InitialData != null && length == InitialData.Value.Length ? InitialData : null);
			return new IndexBufferBuilder(Usage, length, newInitData);
		}

		/// <summary>
		/// Creates a new <see cref="IndexBuffer"/> with the supplied builder parameters.
		/// </summary>
		/// <remarks>
		/// In debug mode, this method will check a large number of <see cref="Assure">assurances</see> 
		/// on the builder parameters before creating the resource.
		/// </remarks>
		/// <returns>A new <see cref="IndexBuffer"/>.</returns>
		public unsafe override IndexBuffer Create() {
			Assure.True(Usage != ResourceUsage.Immutable || InitialData != null, "You must supply initial data to an immutable resource.");
			Assure.GreaterThan(length, 0U, "Can not create an index buffer with 0 indices.");

			GCHandle? pinnedArrayHandle = null;
			IntPtr initialDataPtr = IntPtr.Zero;

			if (InitialData != null) {
				pinnedArrayHandle = GCHandle.Alloc(InitialData.Value.ContainingArray, GCHandleType.Pinned);
				initialDataPtr = pinnedArrayHandle.Value.AddrOfPinnedObject() + (IndexBuffer.INDEX_SIZE_BYTES * (int) InitialData.Value.Offset);
			}

			try {
				BufferResourceHandle outResourceHandle;
				InteropUtils.CallNative(NativeMethods.ResourceFactory_CreateIndexBuffer,
					RenderingModule.Device,
					length,
					Usage.GetUsage(),
					Usage.GetCPUUsage(),
					initialDataPtr,
					(IntPtr) (&outResourceHandle)
				).ThrowOnFailure();

				return new IndexBuffer(outResourceHandle, Usage, length);
			}
			finally {
				if (pinnedArrayHandle != null) pinnedArrayHandle.Value.Free();
			}
		}
	}
}