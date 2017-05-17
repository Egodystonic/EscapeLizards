// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 28 01 2015 at 16:44 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Rendering {
	public sealed class BlendState : BaseResource {
		private const uint BLEND_STATE_SIZE_BYTES = 8U * 32U + 8U;
		
		public readonly BlendOperation BlendOperation;

		/// <summary>
		/// Gets the set of permitted bindings to the GPU for this resource that determine where in the rendering pipeline this resource
		/// may be used.
		/// </summary>
		/// <remarks>
		/// Returns <see cref="GPUBindings.None"/> for all BlendState objects.
		/// </remarks>
		public override unsafe GPUBindings PermittedBindings {
			get { return GPUBindings.None; }
		}

		public BlendState(BlendOperation blendOperation)
		: base(
			CreateBlendState(blendOperation), 
			ResourceUsage.Immutable, 
			BLEND_STATE_SIZE_BYTES
		) {
			BlendOperation = blendOperation;
		}

		/// <summary>
		/// Used by the <see cref="BaseResource.ToString"/> implementation in <see cref="BaseResource"/> to create a child-class-specific
		/// string that provides additional data about this resource.
		/// </summary>
		/// <returns>A string in the format DATUM1 + <see cref="BaseResource.DESC_VALUE_SEPARATOR"/> + DATUM2 +
		/// <see cref="BaseResource.DESC_VALUE_SEPARATOR"/> + DATUM3 etc</returns>
		protected override unsafe string CreateResourceDescString() {
			return
				"BlendState" + DESC_TYPE_SEPARATOR
				+ (BlendOperation != BlendOperation.None ? "Blending type: " + BlendOperation : "No blending") + DESC_VALUE_SEPARATOR;
		}

		private static unsafe BlendStateResourceHandle CreateBlendState(BlendOperation blendOperation) {
			BlendStateResourceHandle outResourceHandle;

			InteropUtils.CallNative(NativeMethods.ResourceFactory_CreateBlendState,
				RenderingModule.Device,
				(InteropBool) (blendOperation != BlendOperation.None),
				blendOperation,
				(IntPtr) (&outResourceHandle)
			).ThrowOnFailure();

			return outResourceHandle;
		}
	}
}