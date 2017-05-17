// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 28 01 2015 at 16:44 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// Represents a setup for the rasterizer stage of the graphics pipeline. RasterizerState objects control the way polygons are
	/// converted to pixels.
	/// </summary>
	public sealed class RasterizerState : BaseResource {
		private const int FILL_MODE_WIREFRAME = 2;
		private const int FILL_MODE_SOLID = 3;
		private const uint RSSTATE_SIZE_BYTES = 25U;
		/// <summary>
		/// True if polygons should not be shaded in (instead only the connecting lines will be drawn).
		/// </summary>
		public readonly bool WireframeMode;
		/// <summary>
		/// The cull mode applied to drawn polygons.
		/// </summary>
		public readonly TriangleCullMode TriangleCulling;
		/// <summary>
		/// Whether or not to 'flip' faces, so that back-facing polygons are treated as forward-facing and vice-versa.
		/// </summary>
		public readonly bool FlipFaces;
		/// <summary>
		/// Depth value added to a given pixel. See https://msdn.microsoft.com/en-us/library/windows/desktop/cc308048%28v=vs.85%29.aspx.
		/// </summary>
		public readonly int DepthBias;
		/// <summary>
		/// Maximum depth bias of a pixel. See https://msdn.microsoft.com/en-us/library/windows/desktop/cc308048%28v=vs.85%29.aspx.
		/// </summary>
		public readonly float DepthBiasClamp;
		/// <summary>
		/// Scalar on a given pixel's slope. See https://msdn.microsoft.com/en-us/library/windows/desktop/cc308048%28v=vs.85%29.aspx.
		/// </summary>
		public readonly float SlopeScaledDepthBias;
		/// <summary>
		/// Whether or not to enable depth-clipping.
		/// </summary>
		public readonly bool EnableZClipping;

		/// <summary>
		/// Gets the set of permitted bindings to the GPU for this resource that determine where in the rendering pipeline this resource
		/// may be used.
		/// </summary>
		/// <remarks>
		/// Returns <see cref="GPUBindings.None"/> for all RasterizerState objects.
		/// </remarks>
		public override unsafe GPUBindings PermittedBindings {
			get { return GPUBindings.None; }
		}

		/// <summary>
		/// Constructs a new RasterizerState with default parameters for depth-testing.
		/// </summary>
		/// <param name="wireframeMode">True if polygons should not be shaded in (instead only the connecting lines will be drawn).</param>
		/// <param name="triangleCulling">The cull mode applied to drawn polygons.</param>
		/// <param name="flipFaces">Whether or not to 'flip' faces, so that back-facing polygons are treated as forward-facing and vice-versa.</param>
		public RasterizerState(bool wireframeMode, TriangleCullMode triangleCulling, bool flipFaces) 
			: this(wireframeMode, triangleCulling, flipFaces, 0, 0f, 0f, true) { }

		/// <summary>
		/// Constructs a new RasterizerState.
		/// </summary>
		/// <param name="wireframeMode">True if polygons should not be shaded in (instead only the connecting lines will be drawn).</param>
		/// <param name="triangleCulling">The cull mode applied to drawn polygons.</param>
		/// <param name="flipFaces">Whether or not to 'flip' faces, so that back-facing polygons are treated as forward-facing and vice-versa.</param>
		/// <param name="depthBias">Depth value added to a given pixel.
		/// See https://msdn.microsoft.com/en-us/library/windows/desktop/cc308048%28v=vs.85%29.aspx.</param>
		/// <param name="depthBiasClamp">Maximum depth bias of a pixel.
		/// See https://msdn.microsoft.com/en-us/library/windows/desktop/cc308048%28v=vs.85%29.aspx.</param>
		/// <param name="slopeScaledDepthBias">Scalar on a given pixel's slope.
		/// See https://msdn.microsoft.com/en-us/library/windows/desktop/cc308048%28v=vs.85%29.aspx.</param>
		/// <param name="enableZClipping">Whether or not to enable depth-clipping.</param>
		public RasterizerState(bool wireframeMode, TriangleCullMode triangleCulling, bool flipFaces, 
			int depthBias, float depthBiasClamp, float slopeScaledDepthBias, bool enableZClipping) 
		: base(
			CreateRasterizerState(wireframeMode, triangleCulling, flipFaces, depthBias, depthBiasClamp, slopeScaledDepthBias, enableZClipping), 
			ResourceUsage.Immutable, 
			RSSTATE_SIZE_BYTES
		) {
			WireframeMode = wireframeMode;
			TriangleCulling = triangleCulling;
			FlipFaces = flipFaces;
			DepthBias = depthBias;
			DepthBiasClamp = depthBiasClamp;
			SlopeScaledDepthBias = slopeScaledDepthBias;
			EnableZClipping = enableZClipping;
		}

		/// <summary>
		/// Used by the <see cref="BaseResource.ToString"/> implementation in <see cref="BaseResource"/> to create a child-class-specific
		/// string that provides additional data about this resource.
		/// </summary>
		/// <returns>A string in the format DATUM1 + <see cref="BaseResource.DESC_VALUE_SEPARATOR"/> + DATUM2 +
		/// <see cref="BaseResource.DESC_VALUE_SEPARATOR"/> + DATUM3 etc</returns>
		protected override unsafe string CreateResourceDescString() {
			return
				"RastState" + DESC_TYPE_SEPARATOR
				+ (WireframeMode ? "Wireframe fill" : "Solid fill") + DESC_VALUE_SEPARATOR
				+ TriangleCulling + DESC_VALUE_SEPARATOR;
		}

		private static unsafe RasterizerStateResourceHandle CreateRasterizerState(bool wireframeMode, TriangleCullMode triangleCulling, bool flipFaces,
			int depthBias, float depthBiasClamp, float slopeScaledDepthBias, bool enableZClipping) {
			RasterizerStateResourceHandle outResourceHandle;

			InteropUtils.CallNative(NativeMethods.ResourceFactory_CreateRSState,
				RenderingModule.Device,
				wireframeMode ? FILL_MODE_WIREFRAME : FILL_MODE_SOLID,
				(int) triangleCulling,
				(InteropBool) flipFaces,
				depthBias,
				depthBiasClamp,
				slopeScaledDepthBias,
				(InteropBool) enableZClipping,
				(IntPtr) (&outResourceHandle)
			).ThrowOnFailure();

			return outResourceHandle;
		}
	}
}