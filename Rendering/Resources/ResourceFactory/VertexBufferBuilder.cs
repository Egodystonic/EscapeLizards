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
	/// An <see cref="IResourceBuilder"/> that can create <see cref="VertexBuffer{TVertex}">VertexBuffer</see>s.
	/// </summary>
	/// <typeparam name="TVertex">A <see cref="Extensions.IsBlittable">blittable</see> struct type that represents a single vertex or
	/// vertex attribute.
	/// </typeparam>
	public sealed class VertexBufferBuilder<TVertex> : BaseResourceBuilder<VertexBufferBuilder<TVertex>, VertexBuffer<TVertex>, ArraySlice<TVertex>?> 
	where TVertex : struct {
		private readonly uint length;

		internal VertexBufferBuilder()
			: this(ResourceUsage.Immutable, 0U, null) { }

		internal VertexBufferBuilder(ResourceUsage usage, uint length, ArraySlice<TVertex>? initialData)
			: base(usage, initialData) {
			Assure.True(typeof(TVertex).IsBlittable());
			Assure.GreaterThan(UnsafeUtils.SizeOf<TVertex>(), 0);
			this.length = length;
		}

		/// <summary>
		/// Sets the <see cref="ResourceUsage"/> to use when creating the <see cref="VertexBuffer{TVertex}"/>.
		/// See the <see cref="SupportedUsagesAttribute"/> on <see cref="VertexBuffer{TVertex}"/> for a list of permitted usages.
		/// </summary>
		/// <param name="usage">The usage to use/set.</param>
		/// <returns>An identical VertexBufferBuilder with the specified <paramref name="usage"/> parameter.</returns>
		public override VertexBufferBuilder<TVertex> WithUsage(ResourceUsage usage) {
			return new VertexBufferBuilder<TVertex>(usage, length, InitialData);
		}

		/// <summary>
		/// Sets the initial data to use when creating the <see cref="VertexBuffer{TVertex}"/>. Resources with a usage of 
		/// <see cref="ResourceUsage.Immutable"/> must have initial data provided.
		/// </summary>
		/// <param name="initialData">The data to set. 
		/// If not null, the resultant vertex buffer will have a length equal to the <see cref="ArraySlice{T}.Length"/>
		/// property. This means if you previously set a length with <see cref="WithLength"/>, the value will be overwritten.
		/// If null, no initial data will be set.</param>
		/// <returns>An identical VertexBufferBuilder with the specified <paramref name="initialData"/> parameter.</returns>
		public override VertexBufferBuilder<TVertex> WithInitialData(ArraySlice<TVertex>? initialData) {
			return new VertexBufferBuilder<TVertex>(Usage, initialData != null ? initialData.Value.Length : length, initialData);
		}

		/// <summary>
		/// Changes the vertex type to use when creating the <see cref="VertexBuffer{TVertex}"/>.
		/// </summary>
		/// <typeparam name="TVertexNew">A <see cref="Extensions.IsBlittable">blittable</see> struct type that represents a single vertex
		/// in the new buffer.</typeparam>
		/// <returns>An identical VertexBufferBuilder with the specified new vertex type.</returns>
		public VertexBufferBuilder<TVertexNew> WithVertexType<TVertexNew>() where TVertexNew : struct {
			return new VertexBufferBuilder<TVertexNew>(Usage, length, null);
		}
		/// <summary>
		/// Sets the length to use when creating the <see cref="VertexBuffer{TVertex}"/>. If you have previously set initial data with
		/// <see cref="WithInitialData"/>, the data will be dropped unless the given <paramref name="length"/> is equal to the 
		/// initial data length.
		/// </summary>
		/// <param name="length">The buffer length.</param>
		/// <returns>An identical VertexBufferBuilder with the specified buffer <paramref name="length"/>.</returns>
		public VertexBufferBuilder<TVertex> WithLength(uint length) {
			ArraySlice<TVertex>? newInitData = (InitialData != null && length == InitialData.Value.Length ? InitialData : null);
			return new VertexBufferBuilder<TVertex>(Usage, length, newInitData);
		}

		/// <summary>
		/// Creates a new <see cref="VertexBuffer{TVertex}"/> with the supplied builder parameters.
		/// </summary>
		/// <remarks>
		/// In debug mode, this method will check a large number of <see cref="Assure">assurances</see> 
		/// on the builder parameters before creating the resource.
		/// </remarks>
		/// <returns>A new <see cref="VertexBuffer{TVertex}"/>.</returns>
		public unsafe override VertexBuffer<TVertex> Create() {
			Assure.True(Usage != ResourceUsage.Immutable || InitialData != null, "You must supply initial data to an immutable resource.");
			Assure.GreaterThan(length, 0U, "Can not create a vertex buffer with 0 vertices.");

			GCHandle? pinnedArrayHandle = null;
			IntPtr initialDataPtr = IntPtr.Zero;
			try {
				int vertexSize = UnsafeUtils.SizeOf<TVertex>();

				if (InitialData != null) {
					pinnedArrayHandle = GCHandle.Alloc(InitialData.Value.ContainingArray, GCHandleType.Pinned);
					initialDataPtr = pinnedArrayHandle.Value.AddrOfPinnedObject() + (vertexSize * (int) InitialData.Value.Offset);
				}

				BufferResourceHandle outResourceHandle;
				InteropUtils.CallNative(NativeMethods.ResourceFactory_CreateVertexBuffer,
					RenderingModule.Device,
					(uint) vertexSize,
					length,
					Usage.GetUsage(),
					Usage.GetCPUUsage(),
					initialDataPtr,
					(IntPtr) (&outResourceHandle)
				).ThrowOnFailure();

				return new VertexBuffer<TVertex>(outResourceHandle, Usage, (uint) vertexSize, length);
			}
			finally {
				if (pinnedArrayHandle != null) pinnedArrayHandle.Value.Free();
			}
		}
	}
}