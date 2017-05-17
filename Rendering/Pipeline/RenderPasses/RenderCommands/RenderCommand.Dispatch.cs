// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 26 01 2015 at 18:16 by Ben Bowen

using System;
using System.Collections.Generic;

namespace Ophidian.Losgap.Rendering {
	public partial struct RenderCommand {
		/// <summary>
		/// Creates a render command that instructs the graphics pipeline to draw an indexed, instanced model.
		/// </summary>
		/// <param name="firstVertexIndex">The vertex buffer starting index for this model. See <see cref="GeometryCache.GetModelBufferValues"/>.</param>
		/// <param name="firstIndexIndex">The index buffer starting index for this model. See <see cref="GeometryCache.GetModelBufferValues"/>.</param>
		/// <param name="numIndices">The number of indices used to describe this model. See <see cref="GeometryCache.GetModelBufferValues"/>.</param>
		/// <param name="firstInstanceIndex">The index of the first transformation matrix in the instance buffer to use.</param>
		/// <param name="numInstances">The number of instances to draw.</param>
		/// <returns>A new RenderCommand that represents the requested instruction and given parameters.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Indices",
			Justification = "Indices is the direct3d term.")]
		public unsafe static RenderCommand DrawIndexedInstanced(int firstVertexIndex, uint firstIndexIndex, uint numIndices, uint firstInstanceIndex, uint numInstances) {
			ulong arg23 = 0UL;
			uint* arg23AsUint = (uint*) &arg23;
			arg23AsUint[0] = firstIndexIndex;
			arg23AsUint[1] = numIndices;

			ulong arg45 = 0UL;
			uint* arg45AsUint = (uint*) &arg45;
			arg45AsUint[0] = firstInstanceIndex;
			arg45AsUint[1] = numInstances;

			return new RenderCommand(RenderCommandInstruction.DrawIndexedInstanced, firstVertexIndex, arg23, arg45);
		}

		public unsafe static RenderCommand Draw(int firstVertexIndex, uint numVertices) {
			return new RenderCommand(RenderCommandInstruction.Draw, firstVertexIndex, numVertices);
		}

		/// <summary>
		/// Creates a render command that instructs the graphics pipeline to clear the given render target to all-black (0, 0, 0, 0).
		/// </summary>
		/// <param name="renderTarget">The render target to clear. Must not be null or disposed.</param>
		/// <returns>A new RenderCommand that represents the requested instruction and given parameters.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0",
			Justification = "The parameter is validated by the assurances.")]
		public static RenderCommand ClearRenderTarget(RenderTargetView renderTarget) {
			Assure.NotNull(renderTarget);
			Assure.False(renderTarget.ResourceOrViewDisposed, "Render target view or its resource was disposed.");
			return new RenderCommand(RenderCommandInstruction.ClearRenderTarget, (IntPtr) (ResourceViewHandle) renderTarget.ResourceViewHandle);
		}

		/// <summary>
		/// Creates a render command that instructs the graphics pipeline to clear the given window's back buffer to all-black (0, 0, 0, 0).
		/// </summary>
		/// <param name="renderTarget">The window to clear. Must not be null.</param>
		/// <returns>A new RenderCommand that represents the requested instruction and given parameters.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0",
			Justification = "The parameter is validated by the assurances.")]
		public static unsafe RenderCommand ClearRenderTarget(Window renderTarget) {
			Assure.NotNull(renderTarget);
			RenderTargetViewHandle outRTV;
			DepthStencilViewHandle outDSV;

			bool windowStillOpen = renderTarget.GetWindowRTVAndDSV(out outRTV, out outDSV);

			if (!windowStillOpen) return new RenderCommand(RenderCommandInstruction.NoOperation);

			return new RenderCommand(RenderCommandInstruction.ClearRenderTarget, (IntPtr) (ResourceViewHandle) outRTV);
		}

		/// <summary>
		/// Creates a render command that instructs the graphics pipeline to clear the given depth/stencil buffer to a default state.
		/// </summary>
		/// <param name="depthStencilView">The depth/stencil view to clear. Must not be null or disposed.</param>
		/// <returns>A new RenderCommand that represents the requested instruction and given parameters.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0",
			Justification = "The parameter is validated by the assurances.")]
		public static RenderCommand ClearDepthStencil(DepthStencilView depthStencilView) {
			Assure.NotNull(depthStencilView);
			Assure.False(depthStencilView.ResourceOrViewDisposed, "Depth stencil view or its resource was disposed.");
			return new RenderCommand(RenderCommandInstruction.ClearDepthStencil, (IntPtr) (ResourceViewHandle) depthStencilView.ResourceViewHandle);
		}

		/// <summary>
		/// Creates a render command that instructs the graphics pipeline to clear the given window's depth/stencil buffer to a default state.
		/// </summary>
		/// <param name="depthStencil">The window to clear. Must not be null.</param>
		/// <returns>A new RenderCommand that represents the requested instruction and given parameters.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0",
			Justification = "The parameter is validated by the assurances.")]
		public static unsafe RenderCommand ClearDepthStencil(Window depthStencil) {
			Assure.NotNull(depthStencil);
			RenderTargetViewHandle outRTV;
			DepthStencilViewHandle outDSV;

			bool windowStillOpen = depthStencil.GetWindowRTVAndDSV(out outRTV, out outDSV);

			if (!windowStillOpen) return new RenderCommand(RenderCommandInstruction.NoOperation);

			return new RenderCommand(RenderCommandInstruction.ClearDepthStencil, (IntPtr) (ResourceViewHandle) outDSV);
		}

		/// <summary>
		/// Creates a render command that instructs the graphics pipeline to present the current scene to the given window.
		/// </summary>
		/// <param name="backBuffer">The window to present to. Must not be null.</param>
		/// <returns>A new RenderCommand that represents the requested instruction and given parameters.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0",
			Justification = "The parameter is validated by the assurances.")]
		public static RenderCommand PresentBackBuffer(Window backBuffer) {
			Assure.NotNull(backBuffer);
			SwapChainHandle outSCH;

			bool windowStillOpen = backBuffer.GetWindowSwapChain(out outSCH);
			if (!windowStillOpen) return new RenderCommand(RenderCommandInstruction.NoOperation);
			return new RenderCommand(RenderCommandInstruction.SwapChainPresent, (IntPtr) outSCH);
		}
	}
}