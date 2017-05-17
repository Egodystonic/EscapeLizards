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
	/// Represents an array of vertices or vertex attributes (of type <typeparamref name="TVertex"/>) that can be used by the rendering pipeline.
	/// </summary>
	/// <remarks>
	/// You can create vertex buffers by using the <see cref="BufferFactory"/> class, and setting parameters on the returned
	/// <see cref="VertexBufferBuilder{TVertex}"/> before calling <see cref="VertexBufferBuilder{TVertex}.Create"/>.
	/// </remarks>
	/// <typeparam name="TVertex">A <see cref="Extensions.IsBlittable">blittable</see> struct type that represents a single vertex.
	/// </typeparam>
	[SupportedUsages(ResourceUsage.Immutable, ResourceUsage.DiscardWrite, ResourceUsage.Write)]
	public sealed class VertexBuffer<TVertex> : BaseBufferResource, IVertexBuffer where TVertex : struct {
		/// <summary>
		/// Gets the set of permitted bindings to the GPU for this resource that determine where in the rendering pipeline this resource
		/// may be used.
		/// </summary>
		/// <remarks>
		/// Vertex buffer objects always return <see cref="GPUBindings.None"/>, as they can not be bound in a traditional manner.
		/// </remarks>
		public override unsafe GPUBindings PermittedBindings {
			get { return GPUBindings.None; }
		}

		internal VertexBuffer(ResourceHandle resourceHandle, ResourceUsage usage, uint vertexSizeBytes, uint length) 
			: base(resourceHandle, usage, vertexSizeBytes * length, typeof(TVertex), vertexSizeBytes, length) { }

		/// <summary>
		/// Returns a <see cref="VertexBufferBuilder{TVertex}"/> that has all its properties set
		/// to the same values as are used by this buffer. A clone of this buffer can then be created by calling
		/// <see cref="VertexBufferBuilder{TVertex}.Create"/> on the returned builder.
		/// </summary>
		/// <returns>A new <see cref="VertexBufferBuilder{TVertex}"/>.</returns>
		public VertexBufferBuilder<TVertex> Clone() {
			return new VertexBufferBuilder<TVertex>(Usage, Length, null);
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
		public void CopyTo(VertexBuffer<TVertex> dest) {
			base.CopyTo(dest);
		}

		/// <summary>
		/// Copies a region of this buffer to the destination buffer (at the specified <paramref name="destStartVertex"/>).
		/// The region is specified as <paramref name="numVertices"/> vertices, starting from <paramref name="firstVertex"/>.
		/// </summary>
		/// <param name="dest">The destination resource. Must not be null.</param>
		/// <param name="firstVertex">The index of the first vertex to copy.</param>
		/// <param name="numVertices">The number of vertices to copy, starting with <paramref name="firstVertex"/>.</param>
		/// <param name="destStartVertex">The offset in to <paramref name="dest"/> to copy to.</param>
		/// <exception cref="ResourceOperationUnavailableException">Thrown if dest returns <c>false</c> for 
		/// <see cref="BaseResource.CanBeCopyDestination"/>.</exception>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters",
			Justification = "The child type is required."),
		System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0",
			Justification = "The parameter is checked by the assurance.")]
		public void CopyTo(VertexBuffer<TVertex> dest, uint firstVertex, uint numVertices, uint destStartVertex) {
			Assure.LessThanOrEqualTo(
				firstVertex + numVertices,
				Length,
				"Buffer overflow: Please ensure you are not attempting to copy from past the end of the source buffer."
			);
			Assure.LessThanOrEqualTo(
				destStartVertex + numVertices,
				dest.Length,
				"Buffer overflow: Please ensure you are not attempting to copy to past the end of the destination buffer."
			);

			base.CopyTo(
				dest, 
				new SubresourceBox(firstVertex * ElementSizeBytes, (firstVertex + numVertices) * ElementSizeBytes), 
				0U, 
				0U, 
				destStartVertex * dest.ElementSizeBytes, 
				0U, 
				0U
			);
		}

		/// <summary>
		/// Performs a <see cref="ResourceUsage.DiscardWrite"/> on this buffer. A discard-write is a faster write that first discards
		/// the old data, then writes the new data.
		/// </summary>
		/// <param name="data">The data to write.
		/// <see cref="ArraySlice{T}.Length">Length</see> vertices will be copied from the given
		/// array slice. The copy will start from the specified <see cref="ArraySlice{T}.Offset">Offset</see> in the
		/// contained array.
		/// </param>
		/// <param name="writeOffsetVertices">The offset in to the resource to begin the write operation.</param>
		/// <exception cref="ResourceOperationUnavailableException">Thrown if <see cref="BaseResource.CanDiscardWrite"/> is
		/// <c>false</c>.</exception>
		public void DiscardWrite(ArraySlice<TVertex> data, uint writeOffsetVertices = 0U) {
			GCHandle pinnedDataHandle = GCHandle.Alloc(data.ContainingArray, GCHandleType.Pinned);

			try {
				base.MapDiscardWrite(
					pinnedDataHandle.AddrOfPinnedObject() + (int) (data.Offset * ElementSizeBytes),
					data.Length * ElementSizeBytes,
					writeOffsetVertices * ElementSizeBytes
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
		/// <see cref="ArraySlice{T}.Length">Length</see> vertices will be copied from the given
		/// array slice. The copy will start from the specified <see cref="ArraySlice{T}.Offset">Offset</see> in the
		/// contained array.
		/// </param>
		/// <param name="writeOffsetVertices">The offset in to the resource to begin the write operation.</param>
		/// <exception cref="ResourceOperationUnavailableException">Thrown if <see cref="BaseResource.CanWrite"/> is
		/// <c>false</c>.</exception>
		public void Write(ArraySlice<TVertex> data, uint writeOffsetVertices) {
			GCHandle pinnedDataHandle = GCHandle.Alloc(data.ContainingArray, GCHandleType.Pinned);
			try {
				if (Usage.ShouldUpdateSubresourceRegion()) {
					Assure.LessThanOrEqualTo(
						data.Length + writeOffsetVertices,
						Length,
						"Buffer overrun: Please ensure you are not writing past the end of the buffer."
					);
					base.UpdateSubregion(
						pinnedDataHandle.AddrOfPinnedObject() + (int) (data.Offset * ElementSizeBytes),
						writeOffsetVertices * ElementSizeBytes, 
						(writeOffsetVertices + data.Length) * ElementSizeBytes
					);
				}
				else {
					base.MapWrite(
						pinnedDataHandle.AddrOfPinnedObject() + (int) (data.Offset * ElementSizeBytes),
						data.Length * ElementSizeBytes,
						writeOffsetVertices * ElementSizeBytes
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
				"VertexBuf<" + ElementType.Name + ">" + DESC_TYPE_SEPARATOR
				+ Length + " len" + DESC_VALUE_SEPARATOR
				;
		}
	}
}