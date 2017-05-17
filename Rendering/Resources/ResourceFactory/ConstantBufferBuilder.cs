// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 30 10 2014 at 11:35 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// An <see cref="IResourceBuilder"/> that can create <see cref="ConstantBuffer{TConstants}">constant buffer</see>s.
	/// </summary>
	/// <typeparam name="TConstants">A <see cref="Extensions.IsBlittable">blittable</see> struct type that represents the data
	/// that can be set in the created <see cref="ConstantBuffer{TConstants}"/>.</typeparam>
	public sealed class ConstantBufferBuilder<TConstants> 
		: BaseResourceBuilder<ConstantBufferBuilder<TConstants>, ConstantBuffer<TConstants>, TConstants>
		where TConstants : struct {
		
		internal ConstantBufferBuilder() : this(ResourceUsage.Immutable, default(TConstants)) { }

		internal ConstantBufferBuilder(ResourceUsage usage, TConstants initialData) : base(usage, initialData) {
			Assure.True(typeof(TConstants).IsBlittable());
			Assure.GreaterThan(UnsafeUtils.SizeOf<TConstants>(), 0);
			Assure.Equal(UnsafeUtils.SizeOf<TConstants>() % 16, 0, "Constant buffer size must be a multiple of 16 bytes.");
		}

		/// <summary>
		/// Sets the <see cref="ResourceUsage"/> to use when creating the <see cref="ConstantBuffer{TConstants}"/>.
		/// See the <see cref="SupportedUsagesAttribute"/> on <see cref="ConstantBuffer{TConstants}"/> for a list of permitted usages.
		/// </summary>
		/// <param name="usage">The usage to use/set.</param>
		/// <returns>An identical ConstantBufferBuilder with the specified <paramref name="usage"/> parameter.</returns>
		public override ConstantBufferBuilder<TConstants> WithUsage(ResourceUsage usage) {
			return new ConstantBufferBuilder<TConstants>(usage, InitialData);
		}

		/// <summary>
		/// Sets the initial data to use when creating the <see cref="ConstantBuffer{TConstants}"/>. Resources with a usage of 
		/// <see cref="ResourceUsage.Immutable"/> must have initial data provided.
		/// </summary>
		/// <param name="initialData">The initial value to set.</param>
		/// <returns>An identical ConstantBufferBuilder with the specified <paramref name="initialData"/> parameter.</returns>
		public override ConstantBufferBuilder<TConstants> WithInitialData(TConstants initialData) {
			return new ConstantBufferBuilder<TConstants>(Usage, initialData);
		}

		/// <summary>
		/// Creates a new <see cref="ConstantBuffer{TConstants}"/> with the supplied builder parameters.
		/// </summary>
		/// <remarks>
		/// In debug mode, this method will check a large number of <see cref="Assure">assurances</see> 
		/// on the builder parameters before creating the resource.
		/// </remarks>
		/// <returns>A new <see cref="ConstantBuffer{TConstants}"/>.</returns>
		public unsafe override ConstantBuffer<TConstants> Create() {
			uint structSizeBytes = (uint) UnsafeUtils.SizeOf<TConstants>();
			byte* initValueStackCopy = stackalloc byte[(int) structSizeBytes];
			UnsafeUtils.WriteGenericToPtr((IntPtr) initValueStackCopy, InitialData, structSizeBytes);

			BufferResourceHandle outResourceHandle;
			InteropUtils.CallNative(NativeMethods.ResourceFactory_CreateConstantBuffer,
				RenderingModule.Device,
				structSizeBytes,
				Usage.GetUsage(),
				Usage.GetCPUUsage(),
				(IntPtr) initValueStackCopy,
				(IntPtr) (&outResourceHandle)
			).ThrowOnFailure();

			return new ConstantBuffer<TConstants>(outResourceHandle, Usage, structSizeBytes);
		}
	}
}