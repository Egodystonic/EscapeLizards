// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 03 11 2014 at 13:33 by Ben Bowen

using System;
using System.Collections.Generic;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// Represents a buffer <see cref="IResource">resource</see>. Buffer resources are essentially fixed-length arrays; where the
	/// array element type depends on the buffer type.
	/// </summary>
	public interface IBuffer : IResource {
		/// <summary>
		/// The element type of each element in this buffer.
		/// </summary>
		Type ElementType { get; }
		/// <summary>
		/// The size, in bytes, of a single element in this buffer.
		/// </summary>
		uint ElementSizeBytes { get; }
		/// <summary>
		/// The number of elements in this buffer.
		/// </summary>
		uint Length { get; }

		/// <summary>
		/// Performs a <see cref="ResourceUsage.DiscardWrite">discard-write</see>, writing the given
		/// <paramref name="data"/> at the specified <paramref name="offset"/> after discarding all the current data.
		/// </summary>
		/// <param name="data">The data to write. The data <see cref="ArraySlice{T}.Length"/> must be a multiple of
		/// <see cref="ElementSizeBytes"/>.</param>
		/// <param name="offset">The byte offset to being writing data to. Must be a multiple of
		/// <see cref="ElementSizeBytes"/>.</param>
		/// <exception cref="ResourceOperationUnavailableException">Thrown if <see cref="IResource.CanDiscardWrite"/>
		/// is <c>false</c>.</exception>
		void DiscardWrite(ArraySlice<byte> data, uint offset);

		/// <summary>
		/// Performs a <see cref="ResourceUsage.Write">write</see>, writing the given
		/// <paramref name="data"/> at the specified <paramref name="offset"/>.
		/// </summary>
		/// <param name="data">The data to write. The data <see cref="ArraySlice{T}.Length"/> must be a multiple of
		/// <see cref="ElementSizeBytes"/>.</param>
		/// <param name="offset">The byte offset to being writing data to. Must be a multiple of
		/// <see cref="ElementSizeBytes"/>.</param>
		/// <exception cref="ResourceOperationUnavailableException">Thrown if <see cref="IResource.CanWrite"/>
		/// is <c>false</c>.</exception>
		void Write(ArraySlice<byte> data, uint offset);

		/// <summary>
		/// Performs a <see cref="ResourceUsage.StagingRead">read</see>, copying all of the data in the resource
		/// to a byte array.
		/// </summary>
		/// <returns>A new byte array that contains a copy of that data in the resource.</returns>
		/// <exception cref="ResourceOperationUnavailableException">Thrown if <see cref="IResource.CanRead"/>
		/// is <c>false</c>.</exception>
		byte[] Read();

		/// <summary>
		/// Performs a <see cref="ResourceUsage.StagingReadWrite">read/write</see>, which first reads the data from the
		/// resource, and then allows the caller to manipulate the data, before copying it back to the GPU.
		/// This method provides a performance improvement over manually calling <see cref="Read"/> followed by <see cref="Write"/>.
		/// </summary>
		/// <param name="readWriteAction">An <see cref="Action"/> that manipulates the read data in-place.</param>
		/// <exception cref="ResourceOperationUnavailableException">Thrown if <see cref="IResource.CanReadWrite"/>
		/// is <c>false</c>.</exception>
		void ReadWrite(Action<byte[]> readWriteAction);
	}



	/// <summary>
	/// Represents a buffer of elements used for general purposes.
	/// </summary>
	public interface IGeneralBuffer : IBuffer {
		/// <summary>
		/// Whether or not the buffer is composed of struct elements in HLSL (<c>true</c>), or primitives (<c>false</c>).
		/// </summary>
		bool IsStructured { get; }

		/// <summary>
		/// Creates a new view to this buffer.
		/// </summary>
		/// <returns>A new shader resource view that exposes the entirety of the buffer.</returns>
		ShaderBufferResourceView CreateView();

		/// <summary>
		/// Creates a new view to this buffer.
		/// </summary>
		/// <param name="firstElementIndex">The index of the first element in this buffer that will be accessible through the returned view.</param>
		/// <param name="numElements">The number of elements in this buffer that will be accessible through the returned view.</param>
		/// <returns>A new resource view that permits reading data from this buffer.</returns>
		ShaderBufferResourceView CreateView(uint firstElementIndex, uint numElements);

		/// <summary>
		/// Creates a new unordered access view to this buffer. The buffer must have been created with the
		/// <see cref="GPUBindings.WritableShaderResource"/> GPU binding.
		/// </summary>
		/// <remarks>
		/// The returned UAV will be <see cref="UnorderedBufferAccessView.IsRawAccessView">raw</see> if <see cref="IsStructured"/> is <c>false</c>.
		/// </remarks>
		/// <param name="firstElementIndex">The index of the first element in this buffer that will be accessible through the returned view.</param>
		/// <param name="numElements">The number of elements in this buffer that will be accessible through the returned view.</param>
		/// <param name="appendConsumeSupport">Whether or not this view will support HLSL append/consume access.</param>
		/// <param name="includeCounter">Whether or not this view will include a 'hidden' counter to allow the HLSL commands
		/// <c>IncrementCounter</c> and <c>DecrementCounter</c> to be used on it.</param>
		/// <returns>A new resource view that permits reading and writing data from/to this buffer.</returns>
		UnorderedBufferAccessView CreateUnorderedAccessView(uint firstElementIndex, uint numElements, bool appendConsumeSupport, bool includeCounter);
	}

	/// <summary>
	/// Represents an array of vertices or vertex attributes that can be used by the rendering pipeline.
	/// </summary>
	/// <seealso cref="VertexBuffer{TVertex}"/>
	public interface IVertexBuffer : IBuffer { }

	/// <summary>
	/// Represents an array of indices (references to vertices in a <see cref="IVertexBuffer"/>)
	/// that can be used by the rendering pipeline.
	/// </summary>
	/// <seealso cref="IndexBuffer"/>
	public interface IIndexBuffer : IBuffer { }

	/// <summary>
	/// Represents a single set of related values that are used in a shader program. These 'constants' may actually be updated
	/// at any time (the constancy refers to their immutability during a single shader invocation).
	/// </summary>
	/// <seealso cref="ConstantBuffer{TConstants}"/>
	public interface IConstantBuffer : IBuffer { }
}