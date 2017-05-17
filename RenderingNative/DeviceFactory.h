// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created by Ben Bowen

#pragma once
#include "../CoreNative/LosgapCore.h"

#include <dxgi.h>
#include <d3d11.h>
#include <atomic>

#include "GPUDesc.h"

namespace losgap {
	/* 
	Class used to iterate over the installed devices and create the ID3D11Device*.
	It is assumed that the methods of this class will always only be called from one thread (the master).
	It is also assumed that Init() will be called once exactly, from that thread, before any other calls are made.
	*/
	class DeviceFactory {
	private:
		static std::atomic<uint32_t> msaaLevel;
		static std::atomic<bool> vsyncEnabled;

	public:
		static const DXGI_FORMAT SUPPORTED_RTV_FORMAT = DXGI_FORMAT_R8G8B8A8_UNORM;
		static const DXGI_FORMAT SUPPORTED_DSV_FORMAT = DXGI_FORMAT_D24_UNORM_S8_UINT;
		static const D3D_FEATURE_LEVEL SUPPORTED_FEATURE_LEVEL = D3D_FEATURE_LEVEL_11_0;
		static const UINT INITIAL_BACK_BUFFER_WIDTH = 800U;
		static const UINT INITIAL_BACK_BUFFER_HEIGHT = 600U;
		static const UINT INITIAL_BACK_BUFFER_REFRESH_RATE_NUMERATOR = 60U;
		static const UINT INITIAL_BACK_BUFFER_REFRESH_RATE_DENOMINATOR = 1U;
		static const DXGI_MODE_SCALING SWAP_CHAIN_SCALING_MODE = DXGI_MODE_SCALING_UNSPECIFIED;
		static const DXGI_MODE_SCANLINE_ORDER SWAP_CHAIN_SCANLINE_ORDER = DXGI_MODE_SCANLINE_ORDER_UNSPECIFIED;

		static void Init();

		static ID3D11Device* CreateDevice(int32_t adapterIndex, int32_t outputAdapterIndex, int32_t outputIndex, bool& outSupportsMTRendering);

		static uint32_t GetMSAALevel();
		static void SetMSAALevel(uint32_t msaaLevel);
		static bool GetVSyncEnabled();
		static void SetVSyncEnabled(bool vsyncEnabled);

		static void ReleaseDevice(ID3D11Device* devicePtr);

		// Not exported
		static IDXGIAdapter* GetCurrentAdapter();
		static IDXGIOutput* GetCurrentOutput();

		static IDXGISwapChain* CreateSwapChain(ID3D11Device* devicePtr, HWND windowHandle, uint32_t bufferCount);
		static DXGI_SAMPLE_DESC GetMSAASampleDesc(ID3D11Device* devicePtr, DXGI_FORMAT format);
		static DXGI_RATIONAL GetNativeRefreshRate(IDXGIOutput* outputPtr, uint32_t widthPx, uint32_t heightPx);
	};
}