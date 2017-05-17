// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 30 10 2014 at 11:17 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Markup;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// Represents a single set of related values that are used in a shader program. These 'constants' may actually be updated
	/// at any time (the constancy refers to their immutability during a single shader invocation).
	/// </summary>
	/// <remarks>
	/// Constant buffers are usually used to provide configuration or per-material/per-object data to shaders.
	/// <para>
	/// You can create constant buffers by using the <see cref="BufferFactory"/> class, and setting parameters on the returned
	/// <see cref="ConstantBufferBuilder{TConstants}"/> before calling <see cref="ConstantBufferBuilder{TConstants}.Create"/>.
	/// </para>
	/// </remarks>
	/// <typeparam name="TConstants">A <see cref="Extensions.IsBlittable">blittable</see> struct type that represents the set of 
	/// related values that are encapsulated in this cbuffer. The values are always updated together, and therefore
	/// a single constant buffer should contain elements that are all updated at a similar time, or at similar intervals.
	/// </typeparam>
	[SupportedUsages(ResourceUsage.Immutable, ResourceUsage.DiscardWrite)]
	public sealed class ConstantBuffer<TConstants> : BaseBufferResource, IConstantBuffer where TConstants : struct {
		/// <summary>
		/// Gets the set of permitted bindings to the GPU for this resource that determine where in the rendering pipeline this resource
		/// may be used.
		/// </summary>
		/// <remarks>
		/// Constant buffer objects always return <see cref="GPUBindings.None"/>, as they can not be bound in a traditional manner.
		/// </remarks>
		public override unsafe GPUBindings PermittedBindings {
			get { return GPUBindings.None; }
		}

		internal ConstantBuffer(ResourceHandle resourceHandle, ResourceUsage usage, uint structSizeBytes) 
			: base(resourceHandle, usage, structSizeBytes, typeof(TConstants), structSizeBytes, 1U) {
		}

		/// <summary>
		/// Returns a <see cref="ConstantBufferBuilder{TConstants}"/> that has all its properties set
		/// to the same values as are used by this buffer. A clone of this buffer can then be created by calling
		/// <see cref="ConstantBufferBuilder{TConstants}.Create"/> on the returned builder.
		/// </summary>
		/// <returns>A new <see cref="ConstantBufferBuilder{TConstants}"/>.</returns>
		public ConstantBufferBuilder<TConstants> Clone() {
			return new ConstantBufferBuilder<TConstants>(Usage, default(TConstants));
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
		public void CopyTo(ConstantBuffer<TConstants> dest) {
			base.CopyTo(dest);
		}

		/// <summary>
		/// Updates the buffer.
		/// </summary>
		/// <param name="value">The new value.</param>
		/// <exception cref="ResourceOperationUnavailableException">Thrown if <see cref="BaseResource.CanDiscardWrite"/> is
		/// <c>false</c>.</exception>
		public unsafe void DiscardWrite(TConstants value) {
			TypedReference valueref = __makeref(value);
			IntPtr valuePtr = *((IntPtr*) &valueref);

			base.MapDiscardWrite(valuePtr, (uint) Size, 0U);
		}


		/// <summary>
		/// Used by the <see cref="BaseResource.ToString"/> implementation in <see cref="BaseResource"/> to create a child-class-specific
		/// string that provides additional data about this resource.
		/// </summary>
		/// <returns>A string in the format DATUM1 + <see cref="BaseResource.DESC_VALUE_SEPARATOR"/> + DATUM2 + 
		/// <see cref="BaseResource.DESC_VALUE_SEPARATOR"/> + DATUM3 etc</returns>
		protected override unsafe string CreateResourceDescString() {
			return
				"ConstantBuf<" + ElementType.Name + ">" + DESC_TYPE_SEPARATOR
				;
		}
	}
}