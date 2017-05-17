// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created by Ben Bowen

#pragma once
#include "../CoreNative/LosgapCore.h"

#include <d3d11.h>
#include <mutex>

#include "WindowFullscreenState.h"

namespace losgap {
	/*
		Encompasses the handle to a window capable of displaying visual content, along with all the state objects tied to it.
	*/
	struct WindowHandle {
		DECLARE_DEFAULT_CONSTRUCTOR(WindowHandle);

		HWND windowHandle;
		IDXGISwapChain* swapChainPtr;
		ID3D11Device* devicePtr;
		ID3D11DepthStencilView* dsvPtr;
		ID3D11RenderTargetView* rtvPtr;
		bool showCursor;
		WindowFullscreenState desiredFullscreenState;
		uint32_t desiredResWidth;
		uint32_t desiredResHeight;
		INTEROP_BOOL windowClosureFlag;
		bool resizeSinceLastCheck;
	};
}