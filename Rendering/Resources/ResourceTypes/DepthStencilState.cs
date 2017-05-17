// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 28 01 2015 at 16:44 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// Represents a state object that contains depth/stencil setup information for the rendering pipeline.
	/// Depth/stencil state describes how the GPU will calculate which fragments to show in the eventual image, when there are multiple polygons
	/// overlapping the same pixels.
	/// </summary>
	public sealed class DepthStencilState : BaseResource {
		private const long DEPTH_STENCIL_STATE_OBJECT_SIZE = 50L;
		/// <summary>
		/// Whether or not depth testing is enabled.
		/// </summary>
		public readonly bool DepthTestingEnabled;
		/// <summary>
		/// The depth-testing function used to compare two potential fragments.
		/// </summary>
		public readonly DepthComparisonFunction DepthComparisonFunction;

		/// <summary>
		/// Gets the set of permitted bindings to the GPU for this resource that determine where in the rendering pipeline this resource
		/// may be used.
		/// </summary>
		/// <remarks>
		/// Returns <see cref="GPUBindings.None"/> for all DepthStencilState objects.
		/// </remarks>
		public override unsafe GPUBindings PermittedBindings {
			get { return GPUBindings.None; }
		}

		/// <summary>
		/// Creates a new Depth/Stencil state with standard default parameters.
		/// </summary>
		public DepthStencilState()
			: this(true) { }

		/// <summary>
		/// Creates a new Depth/Stencil state with standard default parameters and the option to enable/disable depth testing.
		/// </summary>
		/// <param name="enableDepthTesting">Whether or not depth testing is enabled.</param>
		public DepthStencilState(bool enableDepthTesting)
			: this (enableDepthTesting, DepthComparisonFunction.PassNearestElements) { }

		/// <summary>
		/// Creates a new Depth/Stencil state with standard default parameters and the option to override some depth testing parameters.
		/// </summary>
		/// <param name="enableDepthTesting">Whether or not depth testing is enabled.</param>
		/// <param name="depthComparisonFunction">The depth-testing function used to compare two potential fragments.</param>
		public DepthStencilState(bool enableDepthTesting, DepthComparisonFunction depthComparisonFunction) 
			: base(CreateDepthStencilState(enableDepthTesting, depthComparisonFunction), ResourceUsage.Immutable, DEPTH_STENCIL_STATE_OBJECT_SIZE) {
			DepthTestingEnabled = enableDepthTesting;
			DepthComparisonFunction = depthComparisonFunction;
		}

		/// <summary>
		/// Used by the <see cref="BaseResource.ToString"/> implementation in <see cref="BaseResource"/> to create a child-class-specific
		/// string that provides additional data about this resource.
		/// </summary>
		/// <returns>A string in the format DATUM1 + <see cref="BaseResource.DESC_VALUE_SEPARATOR"/> + DATUM2 +
		/// <see cref="BaseResource.DESC_VALUE_SEPARATOR"/> + DATUM3 etc</returns>
		protected override unsafe string CreateResourceDescString() {
			return
				"DSState" + DESC_TYPE_SEPARATOR
				+ (DepthTestingEnabled ? "Depth enabled" : "Depth disabled") + DESC_VALUE_SEPARATOR
				+ DepthComparisonFunction + DESC_VALUE_SEPARATOR;
		}

		private static unsafe DepthStencilStateResourceHandle CreateDepthStencilState(bool enableDepthTesting, 
			DepthComparisonFunction depthComparisonFunction) {
			DepthStencilStateResourceHandle outResourceHandle;

			InteropUtils.CallNative(NativeMethods.ResourceFactory_CreateDSState,
				RenderingModule.Device,
				(InteropBool) enableDepthTesting,
				depthComparisonFunction,
				(IntPtr) (&outResourceHandle)
			).ThrowOnFailure();

			return outResourceHandle;
		}
	}
}