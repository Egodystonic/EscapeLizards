// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 27 10 2014 at 11:32 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Policy;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// An <see cref="IResourceBuilder"/> that can create generic <see cref="Buffer{TElement}">buffer</see>s.
	/// </summary>
	/// <typeparam name="TElement">A <see cref="Extensions.IsBlittable">blittable</see> struct type that represents a single element
	/// in the buffer.
	/// </typeparam>
	public sealed class BufferBuilder<TElement> : BaseResourceBuilder<BufferBuilder<TElement>, Buffer<TElement>, ArraySlice<TElement>?> 
	where TElement : struct {
		private readonly uint length;
		private readonly GPUBindings permittedBindings;

		internal BufferBuilder()
			: this(ResourceUsage.Immutable, 0U, GPUBindings.ReadableShaderResource, null) { }

		internal BufferBuilder(ResourceUsage usage, uint length, GPUBindings permittedBindings, 
			ArraySlice<TElement>? initialData)
			: base(usage, initialData) {
			Assure.True(typeof(TElement).IsBlittable());
			Assure.GreaterThan(UnsafeUtils.SizeOf<TElement>(), 0);
			this.length = length;
			this.permittedBindings = permittedBindings;
		}

		/// <summary>
		/// Sets the <see cref="ResourceUsage"/> to use when creating the <see cref="Buffer{TElement}"/>.
		/// See the <see cref="SupportedUsagesAttribute"/> on <see cref="Buffer{TElement}"/> for a list of permitted usages.
		/// </summary>
		/// <param name="usage">The usage to use/set.</param>
		/// <returns>An identical BufferBuilder with the specified <paramref name="usage"/> parameter.</returns>
		public override BufferBuilder<TElement> WithUsage(ResourceUsage usage) {
			return new BufferBuilder<TElement>(usage, length, permittedBindings, InitialData);
		}

		/// <summary>
		/// Sets the permitted <see cref="GPUBindings"/> to use when creating the <see cref="Buffer{TElement}"/>.
		/// See the <see cref="SupportedGPUBindingsAttribute"/> on <see cref="Buffer{TElement}"/> for a list of supported options.
		/// </summary>
		/// <param name="permittedBindings">The bindings to use/set.</param>
		/// <returns>An identical BufferBuilder with the specified <paramref name="permittedBindings"/>.</returns>
		public BufferBuilder<TElement> WithPermittedBindings(GPUBindings permittedBindings) {
			ThrowIfUnsupportedGPUBinding(permittedBindings);
			return new BufferBuilder<TElement>(Usage, length, permittedBindings, InitialData);
		}

		/// <summary>
		/// Sets the initial data to use when creating the <see cref="Buffer{TElement}"/>. Resources with a usage of 
		/// <see cref="ResourceUsage.Immutable"/> must have initial data provided.
		/// </summary>
		/// <param name="initialData">The data to set. 
		/// If not null, the resultant buffer will have a length equal to the <see cref="ArraySlice{T}.Length"/>
		/// property. This means if you previously set a length with <see cref="WithLength"/>, the value will be overwritten.
		/// If null, no initial data will be set.</param>
		/// <returns>An identical BufferBuilder with the specified <paramref name="initialData"/> parameter.</returns>
		public override BufferBuilder<TElement> WithInitialData(ArraySlice<TElement>? initialData) {
			return new BufferBuilder<TElement>(
				Usage, 
				initialData != null ? initialData.Value.Length : length, 
				permittedBindings, 
				initialData
			);
		}

		/// <summary>
		/// Changes the element type to use when creating the <see cref="Buffer{TVertex}"/>.
		/// </summary>
		/// <typeparam name="TElementNew">
		/// A <see cref="Extensions.IsBlittable">blittable</see> struct type that represents a single element in the buffer.
		/// </typeparam>
		/// <returns>An identical BufferBuilder with the specified new vertex type.</returns>
		public BufferBuilder<TElementNew> WithElementType<TElementNew>() where TElementNew : struct {
			return new BufferBuilder<TElementNew>(Usage, length, permittedBindings, null);
		}

		/// <summary>
		/// Sets the length to use when creating the <see cref="Buffer{TVertex}"/>. If you have previously set initial data with
		/// <see cref="WithInitialData"/>, the data will be dropped unless the given <paramref name="length"/> is equal to the 
		/// initial data length.
		/// </summary>
		/// <param name="length">The buffer length.</param>
		/// <returns>An identical BufferBuilder with the specified buffer <paramref name="length"/>.</returns>
		public BufferBuilder<TElement> WithLength(uint length) {
			ArraySlice<TElement>? newInitData = (InitialData != null && length == InitialData.Value.Length ? InitialData : null);
			return new BufferBuilder<TElement>(Usage, length, permittedBindings, newInitData);
		}

		/// <summary>
		/// Creates a new <see cref="Buffer{TElement}"/> with the supplied builder parameters.
		/// </summary>
		/// <remarks>
		/// In debug mode, this method will check a large number of <see cref="Assure">assurances</see> 
		/// on the builder parameters before creating the resource.
		/// </remarks>
		/// <returns>A new <see cref="Buffer{TElement}"/>.</returns>
		public unsafe override Buffer<TElement> Create() {
			Assure.True(Usage != ResourceUsage.Immutable || InitialData != null, "You must supply initial data to an immutable resource.");
			Assure.False(
				(Usage == ResourceUsage.Immutable || Usage == ResourceUsage.DiscardWrite) && permittedBindings == GPUBindings.None,
				"An immutable or discard-write resource with no permitted bindings is useless."
			);
			Assure.False(
				Usage.GetUsage() == 0x3 && permittedBindings != GPUBindings.None,
				"Staging resources can not be bound to the pipeline."
			);
			Assure.GreaterThan(length, 0U, "Can not create a buffer with 0 elements.");

			InteropBool isStructured = (BaseResource.GetFormatForType(typeof(TElement)) == ResourceFormat.Unknown);
			InteropBool allowRawAccess =
				!isStructured
				&& (int) (permittedBindings & (GPUBindings.WritableShaderResource | GPUBindings.ReadableShaderResource)) != 0;

			GCHandle? pinnedArrayHandle = null;
			IntPtr initialDataPtr = IntPtr.Zero;
			try {
				int elementSizeBytes = UnsafeUtils.SizeOf<TElement>();

				if (InitialData != null) {
					pinnedArrayHandle = GCHandle.Alloc(InitialData.Value.ContainingArray, GCHandleType.Pinned);
					initialDataPtr = pinnedArrayHandle.Value.AddrOfPinnedObject() + (elementSizeBytes * (int) InitialData.Value.Offset);
				}

				BufferResourceHandle outResourceHandle;
				InteropUtils.CallNative(NativeMethods.ResourceFactory_CreateBuffer,
					RenderingModule.Device,
					(uint) elementSizeBytes,
					length,
					Usage.GetUsage(),
					Usage.GetCPUUsage(),
					(PipelineBindings) permittedBindings,
					isStructured,
					allowRawAccess,
					initialDataPtr,
					(IntPtr) (&outResourceHandle)
				).ThrowOnFailure();

				return new Buffer<TElement>(outResourceHandle, Usage, (uint) elementSizeBytes, length, permittedBindings, isStructured);
			}
			finally {
				if (pinnedArrayHandle != null) pinnedArrayHandle.Value.Free();
			}
		}
	}
}