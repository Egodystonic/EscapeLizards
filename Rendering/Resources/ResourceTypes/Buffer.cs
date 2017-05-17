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
	/// Represents an array of any generic element type (<typeparamref name="TElement"/>) that can be used by the rendering pipeline.
	/// </summary>
	/// <remarks>
	/// You can create buffers by using the <see cref="BufferFactory"/> class, and setting parameters on the returned
	/// <see cref="BufferBuilder{TVertex}"/> before calling <see cref="BufferBuilder{TElement}.Create"/>.
	/// </remarks>
	/// <typeparam name="TElement">A <see cref="Extensions.IsBlittable">blittable</see> struct type that represents a single element.
	/// </typeparam>
	[SupportedUsages(ResourceUsage.Immutable, ResourceUsage.DiscardWrite, ResourceUsage.Write, 
		ResourceUsage.StagingRead, ResourceUsage.StagingWrite, ResourceUsage.StagingReadWrite)]
	[SupportedGPUBindings(GPUBindings.ReadableShaderResource | GPUBindings.WritableShaderResource)]
	public sealed class Buffer<TElement> : BaseBufferResource, IGeneralBuffer where TElement : struct {
		private readonly bool isStructured;
		private readonly GPUBindings permittedBindings;

		/// <summary>
		/// Whether or not the buffer is composed of struct elements in HLSL (<c>true</c>), or primitives (<c>false</c>).
		/// </summary>
		public bool IsStructured {
			get { return isStructured; }
		}

		/// <summary>
		/// Gets the set of permitted bindings to the GPU for this resource that determine where in the rendering pipeline this resource
		/// may be used.
		/// </summary>
		/// <remarks>
		/// Depending on this value, resources may be bound to one or
		/// more parts of the rendering pipeline; and subsequently may have different optional use-cases. However, resources should be
		/// created only with the <see cref="GPUBindings"/> required for their intended application, for maximum performance (see also:
		/// <see cref="GPUBindings"/>).
		/// </remarks>
		public override unsafe GPUBindings PermittedBindings {
			get { return permittedBindings; }
		}

		internal Buffer(
			ResourceHandle resourceHandle, ResourceUsage usage, uint ElementSizeBytes, 
			uint length, GPUBindings permittedBindings, bool isStructured) 
			: base(resourceHandle, usage, ElementSizeBytes * length, typeof(TElement), ElementSizeBytes, length) {
			this.permittedBindings = permittedBindings;
			this.isStructured = isStructured;
		}

		/// <summary>
		/// Returns a <see cref="BufferBuilder{TVertex}"/> that has all its properties set
		/// to the same values as are used by this buffer. A clone of this buffer can then be created by calling
		/// <see cref="BufferBuilder{TElement}.Create"/> on the returned builder.
		/// </summary>
		/// <param name="includeData">If true, the <see cref="BufferBuilder{TVertex}.WithInitialData">initial data</see> of the 
		/// returned resource builder will be set to a copy of the data contained within this resource. Requires that this
		/// resource's <see cref="BaseResource.CanRead"/> property returns <c>true</c>.</param>
		/// <returns>A new <see cref="BufferBuilder{TVertex}"/>.</returns>
		/// <exception cref="ResourceOperationUnavailableException">Thrown if <paramref name="includeData"/> is <c>true</c>,
		/// but <see cref="BaseResource.CanRead"/> is <c>false</c>.</exception>
		public BufferBuilder<TElement> Clone(bool includeData = false) {
			if (includeData) return new BufferBuilder<TElement>(Usage, Length, permittedBindings, Read());
			else return new BufferBuilder<TElement>(Usage, Length, permittedBindings, null);
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
		public void CopyTo(Buffer<TElement> dest) {
			base.CopyTo(dest);
		}

		/// <summary>
		/// Copies a region of this buffer to the destination buffer (at the specified <paramref name="destStartElement"/>).
		/// The region is specified as <paramref name="numElements"/> elements, starting from <paramref name="firstElement"/>.
		/// </summary>
		/// <param name="dest">The destination resource. Must not be null.</param>
		/// <param name="firstElement">The index of the first element to copy.</param>
		/// <param name="numElements">The number of elements to copy, starting with <paramref name="firstElement"/>.</param>
		/// <param name="destStartElement">The offset in to <paramref name="dest"/> to copy to.</param>
		/// <exception cref="ResourceOperationUnavailableException">Thrown if dest returns <c>false</c> for 
		/// <see cref="BaseResource.CanBeCopyDestination"/>.</exception>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters",
			Justification = "The child type is required."),
		System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0",
			Justification = "The parameter is checked by the assurance.")]
		public void CopyTo(Buffer<TElement> dest, uint firstElement, uint numElements, uint destStartElement) {
			Assure.LessThanOrEqualTo(
				firstElement + numElements,
				Length,
				"Buffer overflow: Please ensure you are not attempting to copy from past the end of the source buffer."
			);
			Assure.LessThanOrEqualTo(
				destStartElement + numElements,
				dest.Length,
				"Buffer overflow: Please ensure you are not attempting to copy to past the end of the destination buffer."
			);

			base.CopyTo(
				dest,
				new SubresourceBox(firstElement * ElementSizeBytes, (firstElement + numElements) * ElementSizeBytes),
				0U,
				0U,
				destStartElement * dest.ElementSizeBytes,
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
		/// <param name="writeOffsetElements">The offset in to the resource to begin the write operation.</param>
		/// <exception cref="ResourceOperationUnavailableException">Thrown if <see cref="BaseResource.CanDiscardWrite"/> is
		/// <c>false</c>.</exception>
		public void DiscardWrite(ArraySlice<TElement> data, uint writeOffsetElements = 0U) {
			GCHandle pinnedDataHandle = GCHandle.Alloc(data.ContainingArray, GCHandleType.Pinned);

			try {
				base.MapDiscardWrite(
					pinnedDataHandle.AddrOfPinnedObject() + (int) (data.Offset * ElementSizeBytes),
					data.Length * ElementSizeBytes,
					writeOffsetElements * ElementSizeBytes
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
		/// <param name="writeOffsetElements">The offset in to the resource to begin the write operation.</param>
		/// <exception cref="ResourceOperationUnavailableException">Thrown if <see cref="BaseResource.CanWrite"/> is
		/// <c>false</c>.</exception>
		public void Write(ArraySlice<TElement> data, uint writeOffsetElements = 0U) {
			GCHandle pinnedDataHandle = GCHandle.Alloc(data.ContainingArray, GCHandleType.Pinned);
			try {
				if (Usage.ShouldUpdateSubresourceRegion()) {
					Assure.LessThanOrEqualTo(
						data.Length + writeOffsetElements,
						Length,
						"Buffer overrun: Please ensure you are not writing past the end of the buffer."
					);
					base.UpdateSubregion(
						pinnedDataHandle.AddrOfPinnedObject() + (int) (data.Offset * ElementSizeBytes),
						writeOffsetElements * ElementSizeBytes,
						(writeOffsetElements + data.Length) * ElementSizeBytes
					);
				}
				else {
					base.MapWrite(
						pinnedDataHandle.AddrOfPinnedObject() + (int) (data.Offset * ElementSizeBytes),
						data.Length * ElementSizeBytes,
						writeOffsetElements * ElementSizeBytes
					);
				}
			}
			finally {
				pinnedDataHandle.Free();
			}
		}

		/// <summary>
		/// Performs a <see cref="ResourceUsage.StagingRead"/> on this buffer, and copies the data in to a new array.
		/// </summary>
		/// <returns>An array of <typeparamref name="TElement"/>s that were read from the buffer.</returns>
		/// <exception cref="ResourceOperationUnavailableException">Thrown if <see cref="BaseResource.CanRead"/> is
		/// <c>false</c>.</exception>
		public TElement[] Read() {
			TElement[] result = new TElement[Length];
			base.MapRead(dataPtr => UnsafeUtils.CopyGenericArray<TElement>(dataPtr, result, ElementSizeBytes));
			return result;
		}

		/// <summary>
		/// Performs a <see cref="ResourceUsage.StagingReadWrite"/> on this buffer, allowing an in-place modification of the data
		/// through a read/write operation. This can be faster than an individual read and write operation in certain use cases.
		/// </summary>
		/// <param name="dataMutationAction">An action that takes the supplied <see cref="RawResourceDataView1D{T}"/> and uses it to
		/// manipulate the data in-place. The supplied resource view is only valid for the duration of the invocation of this
		/// <see cref="Action"/>.</param>
		/// <exception cref="ResourceOperationUnavailableException">Thrown if <see cref="BaseResource.CanReadWrite"/> is
		/// <c>false</c>.</exception>
		public void ReadWrite(Action<RawResourceDataView1D<TElement>> dataMutationAction) {
			base.MapReadWrite(dataPtr => dataMutationAction(new RawResourceDataView1D<TElement>(dataPtr, ElementSizeBytes, Length)));
		}

		/// <summary>
		/// Creates a new view to this buffer.
		/// </summary>
		/// <returns>A new shader resource view that exposes the entirety of the buffer.</returns>
		public ShaderBufferResourceView CreateView() {
			return CreateView(0U, Length);
		}

		/// <summary>
		/// Creates a new view to this buffer.
		/// </summary>
		/// <param name="firstElementIndex">The index of the first element in this buffer that will be accessible through the returned view.</param>
		/// <param name="numElements">The number of elements in this buffer that will be accessible through the returned view.</param>
		/// <returns>A new resource view that permits reading data from this buffer.</returns>
		public unsafe ShaderBufferResourceView CreateView(uint firstElementIndex, uint numElements) {
			if (firstElementIndex + numElements > Length || numElements == 0U) throw new ArgumentOutOfRangeException("numElements");
			if ((PermittedBindings & GPUBindings.ReadableShaderResource) != GPUBindings.ReadableShaderResource) {
				throw new InvalidOperationException("Can not create an shader resource view to a resource that was created without the "
					+ GPUBindings.ReadableShaderResource + " binding.");
			}

			ShaderResourceViewHandle outViewHandle;

			InteropUtils.CallNative(
				NativeMethods.ResourceFactory_CreateSRVToBuffer,
				RenderingModule.Device,
				(BufferResourceHandle) ResourceHandle,
				GetFormatForType(ElementType),
				firstElementIndex,
				numElements,
				(IntPtr) (&outViewHandle)
			).ThrowOnFailure();

			return new ShaderBufferResourceView(outViewHandle, this, firstElementIndex, numElements);
		}

		/// <summary>
		/// Creates a new unordered access view to this buffer.
		/// </summary>
		/// <remarks>
		/// The returned UAV will be <see cref="UnorderedBufferAccessView.IsRawAccessView">raw</see> if <see cref="IGeneralBuffer.IsStructured"/> is <c>false</c>.
		/// </remarks>
		/// <param name="firstElementIndex">The index of the first element in this buffer that will be accessible through the returned view.</param>
		/// <param name="numElements">The number of elements in this buffer that will be accessible through the returned view.</param>
		/// <param name="appendConsumeSupport">Whether or not this view will support HLSL append/consume access.</param>
		/// <param name="includeCounter">Whether or not this view will include a 'hidden' counter to allow the HLSL commands
		/// <c>IncrementCounter</c> and <c>DecrementCounter</c> to be used on it.</param>
		/// <returns>A new resource view that permits reading and writing data from/to this buffer.</returns>
		public unsafe UnorderedBufferAccessView CreateUnorderedAccessView(uint firstElementIndex, uint numElements, bool appendConsumeSupport, bool includeCounter) {
			if (firstElementIndex + numElements > Length || numElements == 0U) throw new ArgumentOutOfRangeException("numElements");
			if ((PermittedBindings & GPUBindings.WritableShaderResource) != GPUBindings.WritableShaderResource) {
				throw new InvalidOperationException("Can not create an unordered access view to a resource that was created without the "
					+ GPUBindings.WritableShaderResource + " binding.");
			}
			if (appendConsumeSupport && includeCounter) {
				throw new ArgumentException("Can not include append/consume support and a counter.", "includeCounter");
			}

			UnorderedAccessViewHandle outViewHandle;

			if (isStructured) {
				InteropUtils.CallNative(
					NativeMethods.ResourceFactory_CreateUAVToBuffer,
					RenderingModule.Device,
					(BufferResourceHandle) ResourceHandle,
					GetFormatForType(ElementType),
					firstElementIndex,
					numElements,
					(InteropBool) appendConsumeSupport,
					(InteropBool) includeCounter,
					(IntPtr) (&outViewHandle)
				).ThrowOnFailure();
			}
			else { // raw
				if (appendConsumeSupport) {
					throw new ArgumentException("Can not create an append/consume UAV to an unstructured buffer.", "appendConsumeSupport");
				}
				if (includeCounter){
					throw new ArgumentException("Can not create a UAV with a counter to an unstructured buffer.", "includeCounter");
				}
				InteropUtils.CallNative(
					NativeMethods.ResourceFactory_CreateUAVToRawBuffer,
					RenderingModule.Device,
					(BufferResourceHandle) ResourceHandle,
					firstElementIndex,
					numElements,
					(InteropBool) false,
					(InteropBool) false,
					(IntPtr) (&outViewHandle)
				).ThrowOnFailure();
			}

			return new UnorderedBufferAccessView(outViewHandle, this, firstElementIndex, numElements, appendConsumeSupport, includeCounter, !isStructured);
		}

		/// <summary>
		/// Used by the <see cref="BaseResource.ToString"/> implementation in <see cref="BaseResource"/> to create a child-class-specific
		/// string that provides additional data about this resource.
		/// </summary>
		/// <returns>A string in the format DATUM1 + <see cref="BaseResource.DESC_VALUE_SEPARATOR"/> + DATUM2 + 
		/// <see cref="BaseResource.DESC_VALUE_SEPARATOR"/> + DATUM3 etc</returns>
		protected override unsafe string CreateResourceDescString() {
			return
				"Buf<" + ElementType.Name + ">" + DESC_TYPE_SEPARATOR
				+ Length + " len" + DESC_VALUE_SEPARATOR
				;
		}
	}
}