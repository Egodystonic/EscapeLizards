// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 27 10 2014 at 11:52 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// Represents an array of indices (references to vertices in a <see cref="VertexBuffer{TVertex}"/>)
	/// that can be used by the rendering pipeline.
	/// </summary>
	/// <remarks>
	/// You can create index buffers by using the <see cref="BufferFactory"/> class, and setting parameters on the returned
	/// <see cref="IndexBufferBuilder"/> before calling <see cref="IndexBufferBuilder.Create"/>.
	/// <para>
	/// Indices are always of type <see cref="uint"/>.
	/// </para>
	/// </remarks>
	[SupportedUsages(ResourceUsage.Immutable, ResourceUsage.DiscardWrite, ResourceUsage.Write)]
	public sealed class IndexBuffer : BaseBufferResource, IIndexBuffer {
		/// <summary>
		/// The size, in bytes, of a single element in an index buffer.
		/// </summary>
		public const byte INDEX_SIZE_BYTES = sizeof(uint);
		/// <summary>
		/// The numeric type of any element in an index buffer.
		/// </summary>
		public static readonly Type IndexElementType = typeof(uint);

		/// <summary>
		/// Gets the set of permitted bindings to the GPU for this resource that determine where in the rendering pipeline this resource
		/// may be used.
		/// </summary>
		/// <remarks>
		/// Index buffer objects always return <see cref="GPUBindings.None"/>, as they can not be bound in a traditional manner.
		/// </remarks>
		public override unsafe GPUBindings PermittedBindings {
			get { return GPUBindings.None; }
		}

		internal IndexBuffer(ResourceHandle resourceHandle, ResourceUsage usage, uint length)
			: base(resourceHandle, usage, length * INDEX_SIZE_BYTES, typeof(uint), INDEX_SIZE_BYTES, length) { }

		/// <summary>
		/// Returns an <see cref="IndexBufferBuilder"/> that has all its properties set
		/// to the same values as are used by this buffer. A clone of this buffer can then be created by calling
		/// <see cref="IndexBufferBuilder.Create"/> on the returned builder.
		/// </summary>
		/// <returns>A new <see cref="IndexBufferBuilder"/>.</returns>
		public IndexBufferBuilder Clone() {
			return new IndexBufferBuilder(Usage, Length, null);
		}

		/// <summary>
		/// Copies this buffer to the destination buffer.
		/// </summary>
		/// <param name="dest">The destination resource. Must not be null.</param>
		/// <exception cref="AssuranceFailedException">(Debug only) Thrown if both resources' <see cref="BaseResource.Size"/>s or 
		/// <see cref="object.GetType">type</see>s are not equal; or if <c>this == dest</c>.</exception>
		/// <exception cref="ResourceOperationUnavailableException">Thrown if dest returns <c>false</c> for 
		/// <see cref="BaseResource.CanBeCopyDestination"/>.</exception>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters",
			Justification = "The child type is required.")]
		public void CopyTo(IndexBuffer dest) {
			base.CopyTo(dest);
		}

		/// <summary>
		/// Copies a region of this buffer to the destination buffer (at the specified <paramref name="destStartIndex"/>).
		/// The region is specified as <paramref name="numIndexes"/> elements, starting from <paramref name="firstIndex"/>.
		/// </summary>
		/// <param name="dest">The destination resource. Must not be null.</param>
		/// <param name="firstIndex">The index of the first element to copy.</param>
		/// <param name="numIndexes">The number of elements to copy, starting with <paramref name="firstIndex"/>.</param>
		/// <param name="destStartIndex">The offset in to <paramref name="dest"/> to copy to.</param>
		/// <exception cref="ResourceOperationUnavailableException">Thrown if dest returns <c>false</c> for 
		/// <see cref="BaseResource.CanBeCopyDestination"/>.</exception>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters",
			Justification = "The child type is required.")]
		public void CopyTo(IndexBuffer dest, uint firstIndex, uint numIndexes, uint destStartIndex) {
			Assure.LessThanOrEqualTo(
				firstIndex + numIndexes,
				Length,
				"Buffer overflow: Please ensure you are not attempting to copy from past the end of the source buffer."
			);
			Assure.LessThanOrEqualTo(
				destStartIndex + numIndexes,
				dest.Length,
				"Buffer overflow: Please ensure you are not attempting to copy to past the end of the destination buffer."
			);

			base.CopyTo(
				dest,
				new SubresourceBox(checked(firstIndex * INDEX_SIZE_BYTES), checked((firstIndex + numIndexes) * INDEX_SIZE_BYTES)),
				0U,
				0U,
				checked(destStartIndex * INDEX_SIZE_BYTES),
				0U,
				0U
			);
		}

		/// <summary>
		/// Performs a <see cref="ResourceUsage.DiscardWrite"/> on this buffer. A discard-write is a faster write that first discards
		/// the old data, then writes the new data.
		/// </summary>
		/// <param name="data">The data to write.
		/// <see cref="ArraySlice{T}.Length">Length</see> elements will be copied from the given
		/// array slice. The copy will start from the specified <see cref="ArraySlice{T}.Offset">Offset</see> in the
		/// contained array.
		/// </param>
		/// <param name="writeOffsetIndexes">The offset in to the resource to begin the write operation.</param>
		/// <exception cref="ResourceOperationUnavailableException">Thrown if <see cref="BaseResource.CanDiscardWrite"/> is
		/// <c>false</c>.</exception>
		public void DiscardWrite(ArraySlice<uint> data, uint writeOffsetIndexes = 0U) {
			GCHandle pinnedDataHandle = GCHandle.Alloc(data.ContainingArray, GCHandleType.Pinned);

			try {
				base.MapDiscardWrite(
					pinnedDataHandle.AddrOfPinnedObject() + (int) checked(data.Offset * INDEX_SIZE_BYTES),
					checked(data.Length * INDEX_SIZE_BYTES),
					checked(writeOffsetIndexes * INDEX_SIZE_BYTES)
				);
			}
			finally {
				pinnedDataHandle.Free();
			}
		}

		/// <summary>
		/// Performs a <see cref="ResourceUsage.Write"/> on this buffer.
		/// </summary>
		/// <param name="data">The data to write.
		/// <see cref="ArraySlice{T}.Length">Length</see> elements will be copied from the given
		/// array slice. The copy will start from the specified <see cref="ArraySlice{T}.Offset">Offset</see> in the
		/// contained array.
		/// </param>
		/// <param name="writeOffsetIndexes">The offset in to the resource to begin the write operation.</param>
		/// <exception cref="ResourceOperationUnavailableException">Thrown if <see cref="BaseResource.CanWrite"/> is
		/// <c>false</c>.</exception>
		public void Write(ArraySlice<uint> data, uint writeOffsetIndexes) {
			GCHandle pinnedDataHandle = GCHandle.Alloc(data.ContainingArray, GCHandleType.Pinned);
			try {
				if (Usage.ShouldUpdateSubresourceRegion()) {
					Assure.LessThanOrEqualTo(
						data.Length + writeOffsetIndexes,
						Length,
						"Buffer overrun: Please ensure you are not writing past the end of the buffer."
					);
					base.UpdateSubregion(
						pinnedDataHandle.AddrOfPinnedObject() + (int) checked(data.Offset * INDEX_SIZE_BYTES),
						checked(writeOffsetIndexes * INDEX_SIZE_BYTES), 
						checked((writeOffsetIndexes + data.Length) * INDEX_SIZE_BYTES)
					);
				}
				else {
					base.MapWrite(
						pinnedDataHandle.AddrOfPinnedObject() + (int) checked(data.Offset * INDEX_SIZE_BYTES),
						checked(data.Length * INDEX_SIZE_BYTES),
						checked(writeOffsetIndexes * INDEX_SIZE_BYTES)
					);
				}
			}
			finally {
				pinnedDataHandle.Free();
			}
		}

		/// <summary>
		/// Used by the <see cref="BaseResource.ToString"/> implementation in <see cref="BaseResource"/> to create a child-class-specific
		/// string that provides additional data about this resource.
		/// </summary>
		/// <returns>A string in the format DATUM1 + <see cref="BaseResource.DESC_VALUE_SEPARATOR"/> + DATUM2 + 
		/// <see cref="BaseResource.DESC_VALUE_SEPARATOR"/> + DATUM3 etc</returns>
		protected override unsafe string CreateResourceDescString() {
			return
				"IndexBuf" + DESC_TYPE_SEPARATOR
				+ Length + " len" + DESC_VALUE_SEPARATOR
				;
		}
	}
}