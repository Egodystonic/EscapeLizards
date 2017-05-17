// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 22 01 2015 at 17:56 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;

namespace Ophidian.Losgap.Rendering {
	internal enum RenderCommandInstruction : ulong {
		NoOperation = 0U,
		SetPrimitiveTopology,
		SetInputLayout,
		SetVertexBuffers,
		SetInstanceBuffer,
		SetIndexBuffer,
		SetRenderTargets,
		CBDiscardWrite,
		VSSetCBuffers,
		VSSetSamplers,
		VSSetResources,
		VSSetShader,
		SetRSState,
		SetDSState,
		SetBlendState,
		SetViewport,
		FSSetCBuffers,
		FSSetSamplers,
		FSSetResources,
		FSSetShader,
		DrawIndexedInstanced,
		Draw,
		ClearRenderTarget,
		ClearDepthStencil,
		SwapChainPresent,
		FinishCommandList,
		BufferWrite,
	}
}