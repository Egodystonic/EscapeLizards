// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created by Ben Bowen

#pragma once
#include "../CoreNative/LosgapCore.h"
#include "WindowHandle.h"
#include "DeviceFactory.h"

#include <Windows.h>
#include <mutex>

namespace losgap {
	/* 
	Static class for creating windows.
	*/
	class WindowFactory {
	public:
		static const LosgapString WINDOW_CLASS_NAME;
		static const LosgapString LOSGAP_ICON;
		static const UINT DEFAULT_WINDOW_WIDTH_PX = DeviceFactory::INITIAL_BACK_BUFFER_WIDTH;
		static const UINT DEFAULT_WINDOW_HEIGHT_PX = DeviceFactory::INITIAL_BACK_BUFFER_HEIGHT;

#undef CreateWindow
		static WindowHandle* CreateWindow(LosgapString iconFile, ID3D11Device* devicePtr, uint32_t bufferCount);

		static uint32_t GetClientWidth(WindowHandle* windowHandle);
		static uint32_t GetClientHeight(WindowHandle* windowHandle);
		static void GetClientPosition(WindowHandle* windowHandle, int32_t* outX, int32_t* outY);
		static void SetClientPosition(WindowHandle* windowHandle, int32_t x, int32_t y);
		static void SetResolution(WindowHandle* window, uint32_t widthPx, uint32_t heightPx);

		static WindowFullscreenState GetFullscreenState(WindowHandle* windowHandle);
		static void SetFullscreenState(WindowHandle* windowHandle, WindowFullscreenState fullscreenState);

		static bool GetCursorVisibility(WindowHandle* windowHandle);
		static void SetCursorVisibility(WindowHandle* windowHandle, bool visibilityState);

		static void SetWindowTitle(WindowHandle* windowHandle, LosgapString windowTitle);

		static WindowHandle* GetFocusedWindowHandle();

		static D3D11_VIEWPORT* CreateViewport();
		static void AlterViewport(D3D11_VIEWPORT* viewportHandle, uint32_t topLeftX, uint32_t topLeftY, uint32_t widthPx, uint32_t heightPx);
		static void DestroyViewport(D3D11_VIEWPORT* viewportHandle);

		static void GetWindowBackBufferRTVAndDSV(WindowHandle* windowHandle, ID3D11RenderTargetView** outRTVPtr, ID3D11DepthStencilView** outDSVPtr);
		static IDXGISwapChain* GetWindowSwapChain(WindowHandle* windowHandle);

		static bool HydrateMessagePump(WindowHandle* windowHandle); // returns true if a resize has occured since last hydration
		static void ClearWindow(ID3D11DeviceContext* deviceContextPtr, WindowHandle* windowHandle);

		static void CloseWindow(WindowHandle* windowHandle);
		static void CleanUpWindowResources(WindowHandle* windowHandle);

		// Not exported
		static void SetDSVAndRTVOnWindowHandle(WindowHandle* windowHandle, uint32_t width, uint32_t height);
		static void LockWindowModifications();
		static void UnlockWindowModifications();
	private:
		static LRESULT CALLBACK HandleWindowMessage(HWND window, UINT umsg, WPARAM wParam, LPARAM lParam);
		static HWND CreateWindow(LosgapString windowTitle, LosgapString iconFile);
	};
}