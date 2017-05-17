// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created by Ben Bowen

#pragma once
#include "../CoreNative/LosgapCore.h"
#include "RenderCommand.h"
#include <d3d11.h>

namespace losgap {
	/*
	A static class that sets up the rendering pipeline for a render pass, and orchestrates the execution of that pass
	*/
	class RenderPassManager {
	public:
		static void FlushInstructions(ID3D11DeviceContext* deviceContextPtr, RenderCommand* commandArr, uint32_t commandArrLen);
		static void ExecuteCommandList(ID3D11DeviceContext* immedContextPtr, ID3D11CommandList* commandListPtr);
		static void PresentBackBuffer(IDXGISwapChain* swapChainPtr);
	};
}