// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 26 01 2015 at 18:16 by Ben Bowen

using System;
using System.Collections.Generic;
using System.Net;

namespace Ophidian.Losgap.Rendering {
	public partial struct RenderCommand {
		/// <summary>
		/// The default primitive topology used by the rendering / geometry cache system. See <see cref="SetPrimitiveTopology"/>.
		/// </summary>
		public const PrimitiveTopology DEFAULT_PRIMITIVE_TOPOLOGY = PrimitiveTopology.TriangleList;
		/// <summary>
		/// The maximum number of render targets that can be set with
		/// <see cref="SetRenderTargets(Ophidian.Losgap.Rendering.DepthStencilView,Ophidian.Losgap.Rendering.RenderTargetView[])"/>.
		/// </summary>
		public const uint MAX_RENDER_TARGETS = 8U;

		/// <summary>
		/// Creates a render command that instructs the graphics pipeline to read all proceeding data with the given <paramref name="topology"/>.
		/// </summary>
		/// <param name="topology">The topology to set.
		/// Leave as the default value (<see cref="DEFAULT_PRIMITIVE_TOPOLOGY"/>) for usage with standard GeometryCache data.</param>
		/// <returns>A new RenderCommand that represents the requested instruction and given parameters.</returns>
		public static RenderCommand SetPrimitiveTopology(PrimitiveTopology topology = DEFAULT_PRIMITIVE_TOPOLOGY) {
			return new RenderCommand(RenderCommandInstruction.SetPrimitiveTopology, (int) topology);
		}

		/// <summary>
		/// Creates a render command that instructs the graphics pipeline to use the specified input layout to assemble vertex
		/// information for subsequent draw calls.
		/// </summary>
		/// <param name="inputLayout">The input layout to use. Must not be null or disposed.</param>
		/// <returns>A new RenderCommand that represents the requested instruction and given parameters.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0",
			Justification = "The parameter is validated by the assurances."), 
		System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters",
			Justification = "Using GeoInputLayout instead of BaseResource provides the type safety for the non-type-safe ResourceHandle.")]
		public static RenderCommand SetInputLayout(GeometryInputLayout inputLayout) {
			Assure.NotNull(inputLayout);
			Assure.False(inputLayout.IsDisposed, "Input layout was disposed.");
			return new RenderCommand(RenderCommandInstruction.SetInputLayout, (IntPtr) inputLayout.ResourceHandle);
		}

		internal static RenderCommand SetInputLayout(InputLayoutHandle inputLayout) {
			Assure.NotEqual(inputLayout, InputLayoutHandle.NULL);
			return new RenderCommand(RenderCommandInstruction.SetInputLayout, (IntPtr) (ResourceHandle) inputLayout);
		}

		/// <summary>
		/// Creates a render command that instructs the graphics pipeline to use the specified rasterizer state for subsequent draw calls.
		/// </summary>
		/// <param name="rs">The rasterizer state to use. Must not be null or disposed.</param>
		/// <returns>A new RenderCommand that represents the requested instruction and given parameters.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0",
			Justification = "The parameter is validated by the assurances."), 
		System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters",
			Justification = "Using RasterizerState instead of BaseResource provides the type safety for the non-type-safe ResourceHandle.")]
		public static RenderCommand SetRasterizerState(RasterizerState rs) {
			Assure.NotNull(rs);
			Assure.False(rs.IsDisposed, "Rasterizer state was disposed.");
			return new RenderCommand(RenderCommandInstruction.SetRSState, (IntPtr) rs.ResourceHandle);
		}

		/// <summary>
		/// Creates a render command that instructs the graphics pipeline to use the specified depth/stencil for subsequent draw calls.
		/// </summary>
		/// <param name="ds">The depth/stencil state to use. Must not be null or disposed.</param>
		/// <returns>A new RenderCommand that represents the requested instruction and given parameters.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters",
			Justification = "Using DepthStencilState instead of BaseResource provides the type safety for the non-type-safe ResourceHandle."), 
		System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0",
			Justification = "The parameter is validated by the assurances.")]
		public static RenderCommand SetDepthStencilState(DepthStencilState ds) {
			Assure.NotNull(ds);
			Assure.False(ds.IsDisposed, "Depth stencil state was disposed.");
			return new RenderCommand(RenderCommandInstruction.SetDSState, (IntPtr) ds.ResourceHandle);
		}

		public static RenderCommand SetBlendState(BlendState bs) {
			Assure.NotNull(bs);
			Assure.False(bs.IsDisposed, "Blend state was disposed.");
			return new RenderCommand(RenderCommandInstruction.SetBlendState, (IntPtr) bs.ResourceHandle);
		}

		/// <summary>
		/// Creates a render command that instructs the graphics pipeline to set the output viewport (where the current scene is rendered to).
		/// </summary>
		/// <param name="vp">The viewport to set. Must not be null or disposed.</param>
		/// <returns>A new RenderCommand that represents the requested instruction and given parameters.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0",
			Justification = "The parameter is validated by the assurances.")]
		public static RenderCommand SetViewport(SceneViewport vp) {
			Assure.NotNull(vp);
			Assure.False(vp.IsDisposed, "Viewport was disposed.");
			return new RenderCommand(RenderCommandInstruction.SetViewport, (IntPtr) vp.ViewportHandle);
		}

		/// <summary>
		/// Creates a render command that instructs the graphics pipeline to set the selected shader to be used for all subsequent
		/// draw calls. Only one shader of each shader type may be set at any one time.
		/// </summary>
		/// <param name="shader">The shader to set. Must not be null or disposed.</param>
		/// <returns>A new RenderCommand that represents the requested instruction and given parameters.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0",
			Justification = "The parameter is validated by the assurances.")]
		public static RenderCommand SetShader(Shader shader) {
			Assure.NotNull(shader);
			Assure.False(shader.IsDisposed, "Shader was disposed.");
			return new RenderCommand(shader.SetShaderInstruction, (IntPtr) shader.Handle);
		}

		/// <summary>
		/// Creates a render command that instructs the graphics pipeline to set the given depth/stencil resource and render targets.
		/// All subsequent draws will be written to these resources.
		/// </summary>
		/// <param name="dsv">The view to the depth/stencil target to set. Must not be null or disposed.</param>
		/// <param name="rtvArr">An array of views to render targets to set. No element may be null or disposed.</param>
		/// <returns>A new RenderCommand that represents the requested instruction and given parameters.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0",
			Justification = "The parameter is validated by the assurances."), 
		System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1",
			Justification = "The parameter is validated by the assurances.")]
		public static unsafe RenderCommand SetRenderTargets(DepthStencilView dsv, params RenderTargetView[] rtvArr) {
			Assure.NotNull(dsv);
			Assure.False(dsv.ResourceOrViewDisposed, "Depth Stencil View or its resource was disposed.");
			Assure.NotNull(rtvArr);
			Assure.None(rtvArr, rtv => rtv == null, "One or more elements in the render target view array were null.");
			Assure.None(
				rtvArr,
				rtv => rtv.ResourceOrViewDisposed,
				"One or more elements in the render target view array (or their resources) were disposed."
			);
			Assure.LessThanOrEqualTo(rtvArr.Length, MAX_RENDER_TARGETS, "Maximum of " + MAX_RENDER_TARGETS + " render targets permitted.");
			RenderTargetViewHandle* rtvArrPtr = (RenderTargetViewHandle*) AllocAndZeroTemp((uint) (rtvArr.Length * sizeof(RenderTargetViewHandle)));

			for (int i = 0; i < rtvArr.Length; i++) {
				rtvArrPtr[i] = rtvArr[i].ResourceViewHandle;
			}

			return new RenderCommand(
				RenderCommandInstruction.SetRenderTargets,
				(IntPtr) rtvArrPtr,
				(IntPtr) (ResourceViewHandle) dsv.ResourceViewHandle,
				(uint) rtvArr.Length
			);
		}

		public static unsafe RenderCommand SetRenderTargets(DepthStencilView dsv, RenderTargetView rtv) {
			return SetRenderTargets(dsv.ResourceViewHandle, rtv.ResourceViewHandle);
		}

		public static unsafe RenderCommand SetRenderTargets(DepthStencilView dsv) {
			return new RenderCommand(
				RenderCommandInstruction.SetRenderTargets,
				IntPtr.Zero,
				(IntPtr) (ResourceViewHandle) dsv.ResourceViewHandle,
				0U
			);
		}

		internal static unsafe RenderCommand SetRenderTargets(DepthStencilViewHandle dsv, params RenderTargetViewHandle[] rtvArr) {
			Assure.NotNull(rtvArr);
			Assure.None(rtvArr, rtv => rtv == null, "One or more elements in the render target view array were null.");
			Assure.LessThanOrEqualTo(rtvArr.Length, MAX_RENDER_TARGETS, "Maximum of " + MAX_RENDER_TARGETS + " render targets permitted.");
			RenderTargetViewHandle* rtvArrPtr = (RenderTargetViewHandle*) AllocAndZeroTemp((uint) (rtvArr.Length * sizeof(RenderTargetViewHandle)));

			for (int i = 0; i < rtvArr.Length; i++) {
				rtvArrPtr[i] = rtvArr[i];
			}

			return new RenderCommand(
				RenderCommandInstruction.SetRenderTargets,
				(IntPtr) rtvArrPtr,
				(IntPtr) (ResourceViewHandle) dsv,
				(uint) rtvArr.Length
			);
		}

		internal static unsafe RenderCommand SetRenderTargets(DepthStencilViewHandle dsv, RenderTargetViewHandle rtv1, RenderTargetViewHandle rtv2) {
			RenderTargetViewHandle* rtvArrPtr = (RenderTargetViewHandle*) AllocAndZeroTemp(2U * (uint) sizeof(RenderTargetViewHandle));

			rtvArrPtr[0] = rtv1;
			rtvArrPtr[1] = rtv2;

			return new RenderCommand(
				RenderCommandInstruction.SetRenderTargets,
				(IntPtr) rtvArrPtr,
				(IntPtr) (ResourceViewHandle) dsv,
				2U
			);
		}

		internal static unsafe RenderCommand SetRenderTargets(DepthStencilViewHandle dsv, RenderTargetViewHandle rtv) {
			RenderTargetViewHandle* rtvPtr = (RenderTargetViewHandle*) AllocAndZeroTemp((uint) sizeof(RenderTargetViewHandle));

			*rtvPtr = rtv;

			return new RenderCommand(
				RenderCommandInstruction.SetRenderTargets,
				(IntPtr) rtvPtr,
				(IntPtr) (ResourceViewHandle) dsv,
				1U
			);
		}

		internal static unsafe RenderCommand SetRenderTargets(DepthStencilViewHandle dsv, RenderTargetView rtv) {
			Assure.NotNull(rtv);
			Assure.False(
				rtv.ResourceOrViewDisposed,
				"One or more elements in the render target view array (or their resources) were disposed."
			);
			RenderTargetViewHandle* rtvArrPtr = (RenderTargetViewHandle*) AllocAndZeroTemp((uint) (sizeof(RenderTargetViewHandle)));
			*rtvArrPtr = rtv.ResourceViewHandle;


			return new RenderCommand(
				RenderCommandInstruction.SetRenderTargets,
				(IntPtr) rtvArrPtr,
				(IntPtr) (ResourceViewHandle) dsv,
				1U
			);
		}

		internal static unsafe RenderCommand SetRenderTargets(Window blackOut, DepthStencilView dsv, params RenderTargetView[] rtvArr) {
			Assure.NotNull(rtvArr);
			Assure.None(rtvArr, rtv => rtv == null, "One or more elements in the render target view array were null.");
			Assure.None(
				rtvArr,
				rtv => rtv.ResourceOrViewDisposed,
				"One or more elements in the render target view array (or their resources) were disposed."
			);
			Assure.LessThanOrEqualTo(rtvArr.Length, MAX_RENDER_TARGETS, "Maximum of " + MAX_RENDER_TARGETS + " render targets permitted.");
			Assure.NotNull(dsv);
			Assure.False(dsv.IsDisposed);
			RenderTargetViewHandle* rtvArrPtr = (RenderTargetViewHandle*) AllocAndZeroTemp((uint) ((rtvArr.Length + 1) * sizeof(RenderTargetViewHandle)));

			RenderTargetViewHandle outRTV;
			DepthStencilViewHandle outDSV;

			bool windowStillOpen = blackOut.GetWindowRTVAndDSV(out outRTV, out outDSV);

			if (!windowStillOpen) return new RenderCommand(RenderCommandInstruction.NoOperation);

			rtvArrPtr[0] = outRTV;
			for (int i = 1; i < rtvArr.Length + 1; i++) {
				rtvArrPtr[i] = rtvArr[i - 1].ResourceViewHandle;
			}

			return new RenderCommand(
				RenderCommandInstruction.SetRenderTargets,
				(IntPtr) rtvArrPtr,
				(IntPtr) (ResourceViewHandle) dsv.ResourceViewHandle,
				(uint) rtvArr.Length + 1U
			);
		}

		/// <summary>
		/// Creates a render command that instructs the graphics pipeline to set the depth/stencil resource and render target to those
		/// owned by the given <see cref="Window"/>.
		/// </summary>
		/// <param name="renderTarget">The target window. Must not be null.</param>
		/// <returns>A new RenderCommand that represents the requested instruction and given parameters.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0",
			Justification = "The parameter is validated by the assurances.")]
		public static unsafe RenderCommand SetRenderTargets(Window renderTarget) {
			Assure.NotNull(renderTarget);
			RenderTargetViewHandle* rtvHandlePtr = (RenderTargetViewHandle*) AllocAndZeroTemp((uint) sizeof(RenderTargetViewHandle));

			RenderTargetViewHandle outRTV;
			DepthStencilViewHandle outDSV;

			bool windowStillOpen = renderTarget.GetWindowRTVAndDSV(out outRTV, out outDSV);

			if (!windowStillOpen) return new RenderCommand(RenderCommandInstruction.NoOperation);

			*rtvHandlePtr = outRTV;
			return new RenderCommand(
				RenderCommandInstruction.SetRenderTargets,
				(IntPtr) rtvHandlePtr,
				(IntPtr) (ResourceViewHandle) outDSV,
				1U
			);
		}
	}
}