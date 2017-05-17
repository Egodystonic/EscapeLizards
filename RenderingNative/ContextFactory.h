// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created by Ben Bowen

#pragma once
#include "../CoreNative/LosgapCore.h"

#include <dxgi.h>
#include <d3d11.h>

namespace losgap {
	/*
	Provides immediate and deferred device contexts to the system.
	*/
	class ContextFactory {
	public:
		static ID3D11DeviceContext* GetImmediateContext(ID3D11Device* devicePtr);
		static ID3D11DeviceContext* CreateDeferredContext(ID3D11Device* devicePtr);

		static void ExecuteDeferredCommandLists(ID3D11DeviceContext* immediateContextPtr, 
			ID3D11DeviceContext** deferredContextPtrArr, UINT numDeferredContexts);

		static void ReleaseContext(ID3D11DeviceContext* contextPtr);
	};
}