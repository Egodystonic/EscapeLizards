// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created by Ben Bowen

#pragma once
#include "../CoreNative/LosgapCore.h"

namespace losgap {
	/*
	An enumeration of possible rendering instructions, passed from managed code to native
	*/
	enum RenderCommandInstruction : uint64_t {
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
	};
}