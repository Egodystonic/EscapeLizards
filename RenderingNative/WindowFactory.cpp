// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created by Ben Bowen

#include "WindowFactory.h"
#include "DeviceFactory.h"
#include "ResourceFactory.h"
#include <map>

namespace losgap {
	const LosgapString WindowFactory::WINDOW_CLASS_NAME = { "LosgapWindowClass" };
	const LosgapString WindowFactory::LOSGAP_ICON = { "losgap.ico" };
	const LONG WINDOW_BORDER_STYLE = WS_CAPTION | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX | WS_SYSMENU;
	const LONG WINDOW_BORDER_STYLE_EX = WS_EX_DLGMODALFRAME | WS_EX_CLIENTEDGE | WS_EX_STATICEDGE;

	int32_t windowClassIncrementer = 0;

	// Unfortunately this lock is necessary because mutations over windows 
	// can be triggered asynchronously from the user's inputs or the OS events.
	std::mutex globalWindowLock;
	std::map<HWND, WindowHandle*> windowHandleMapping;

#pragma region ::CreateWindow()
	WindowHandle* WindowFactory::CreateWindow(LosgapString iconFile, ID3D11Device* devicePtr, uint32_t bufferCount) {
		HINSTANCE applicationInstance = GetModuleHandle(nullptr);
		if (applicationInstance == nullptr) throw LosgapException { "Could not get current application module handle." };

		HICON windowIcon = (HICON) LoadImage(
			nullptr,
			LosgapString::AsNewCWString(iconFile).get(),
			IMAGE_ICON,
			0,
			0,
			LR_LOADFROMFILE | LR_DEFAULTSIZE | LR_SHARED | LR_LOADTRANSPARENT
			);

		WNDCLASSEX windowDesc;
		ZeroMemory(&windowDesc, sizeof(windowDesc));
		windowDesc.cbSize = sizeof(windowDesc);
		windowDesc.style = CS_HREDRAW | CS_VREDRAW | CS_OWNDC;
		windowDesc.lpfnWndProc = HandleWindowMessage;
		windowDesc.cbClsExtra = 0;
		windowDesc.cbWndExtra = 0;
		windowDesc.hInstance = applicationInstance;
		windowDesc.hIcon = windowIcon;
		windowDesc.hCursor = nullptr;
		windowDesc.hbrBackground = (HBRUSH) GetStockObject(BLACK_BRUSH);
		windowDesc.lpszMenuName = nullptr;
		std::unique_ptr<const wchar_t> className = LosgapString::AsNewCWString(WINDOW_CLASS_NAME + std::to_string(windowClassIncrementer++));
		windowDesc.lpszClassName = className.get();
		windowDesc.hIconSm = nullptr;

		ATOM registerClassResult = RegisterClassEx(&windowDesc);
		if (registerClassResult == 0) {
			throw LosgapException { LosgapString::Concat("Could not register window class '", windowDesc.lpszClassName, "'.") };
		}

		HWND hWnd = CreateWindowEx(
			WS_EX_APPWINDOW,
			windowDesc.lpszClassName,
			L"Untitled Losgap Window",
			WS_SYSMENU | WS_MAXIMIZEBOX | WS_MINIMIZEBOX | WS_CLIPSIBLINGS | WS_CLIPCHILDREN | WS_EX_ACCEPTFILES | WS_THICKFRAME,
			0,
			0,
			DEFAULT_WINDOW_WIDTH_PX,
			DEFAULT_WINDOW_HEIGHT_PX,
			nullptr,
			nullptr,
			applicationInstance,
			nullptr
			);

		if (hWnd == nullptr) throw LosgapString { "Could not create window." };

		IDXGISwapChain* swapChainPtr = DeviceFactory::CreateSwapChain(devicePtr, hWnd, bufferCount);

		ShowWindow(hWnd, SW_SHOW);

		WindowHandle* result = new WindowHandle { };
		result->showCursor = true;
		result->devicePtr = devicePtr;
		result->swapChainPtr = swapChainPtr;
		result->windowHandle = hWnd;
		result->windowClosureFlag = INTEROP_BOOL_FALSE;
		result->resizeSinceLastCheck = false;

		SetDSVAndRTVOnWindowHandle(result, DEFAULT_WINDOW_WIDTH_PX, DEFAULT_WINDOW_HEIGHT_PX);

		std::lock_guard<std::mutex> lockGuard { globalWindowLock };
		windowHandleMapping[hWnd] = result;

		return result;
	}
	EXPORT(WindowFactory_CreateWindow, INTEROP_STRING windowIconFilePath, ID3D11Device* devicePtr, uint32_t bufferCount, 
		WindowHandle** outWindowHandlePtr, INTEROP_BOOL** outWindowClosureFlagPtr) {
		if (windowIconFilePath == nullptr) EXPORT_FAIL("Window icon file path can not be null.");
		*outWindowHandlePtr = WindowFactory::CreateWindow(windowIconFilePath, devicePtr, bufferCount);
		*outWindowClosureFlagPtr = &(*outWindowHandlePtr)->windowClosureFlag;
		EXPORT_END;
	}
#pragma endregion

#pragma region ::GetClient[Width/Height]()
	uint32_t WindowFactory::GetClientWidth(WindowHandle* windowHandle) {
		std::lock_guard<std::mutex> lockGuard { globalWindowLock };
		if (windowHandle == nullptr) throw LosgapException { "Window handle must not be null." };

		if (INTEROP_BOOL_TO_CBOOL(windowHandle->windowClosureFlag)) return 0U;

		if (FALSE == IsWindow(windowHandle->windowHandle)) throw LosgapException { "Invalid window handle." };
		RECT outClientRect;
		ZeroMemory(&outClientRect, sizeof(outClientRect));

		if (FALSE == GetClientRect(windowHandle->windowHandle, &outClientRect)) {
			throw LosgapException { "Could not get window client area." };
		}
		return outClientRect.right - outClientRect.left;
	}
	EXPORT(WindowFactory_GetClientWidth, WindowHandle* windowHandle, uint32_t& outWidth) {
		outWidth = WindowFactory::GetClientWidth(windowHandle);
		EXPORT_END;
	}

	uint32_t WindowFactory::GetClientHeight(WindowHandle* windowHandle) {
		std::lock_guard<std::mutex> lockGuard { globalWindowLock };
		if (windowHandle == nullptr) throw LosgapException { "Window handle must not be null." };

		if (INTEROP_BOOL_TO_CBOOL(windowHandle->windowClosureFlag)) return 0U;

		if (FALSE == IsWindow(windowHandle->windowHandle)) throw LosgapException { "Invalid window handle." };
		RECT outClientRect;
		ZeroMemory(&outClientRect, sizeof(outClientRect));

		if (FALSE == GetClientRect(windowHandle->windowHandle, &outClientRect)) {
			throw LosgapException { "Could not get window client area." };
		}
		return outClientRect.bottom - outClientRect.top;
	}
	EXPORT(WindowFactory_GetClientHeight, WindowHandle* windowHandle, uint32_t& outHeight) {
		outHeight = WindowFactory::GetClientHeight(windowHandle);
		EXPORT_END;
	}
#pragma endregion

#pragma region ::[Get/Set]FullscreenState()
	WindowFullscreenState WindowFactory::GetFullscreenState(WindowHandle* windowHandle) {
		std::lock_guard<std::mutex> lockGuard { globalWindowLock };
		if (windowHandle == nullptr) throw LosgapException { "Window handle must not be null." };

		if (INTEROP_BOOL_TO_CBOOL(windowHandle->windowClosureFlag)) return WindowFullscreenState::NotFullscreen;

		BOOL outIsFullscreen;
		IDXGIOutput* outFSOutput;

		CHECK_CALL(windowHandle->swapChainPtr->GetFullscreenState(&outIsFullscreen, &outFSOutput));

		if (TRUE == outIsFullscreen) return WindowFullscreenState::StandardFullscreen;

		LONG windowStyle = GetWindowLong(windowHandle->windowHandle, GWL_STYLE);
		if ((windowStyle & WS_THICKFRAME) == 0) return WindowFullscreenState::BorderlessFullscreen;
		else return WindowFullscreenState::NotFullscreen;
	}
	EXPORT(WindowFactory_GetFullscreenState, WindowHandle* windowHandle, WindowFullscreenState& outFullscreenState) {
		outFullscreenState = WindowFactory::GetFullscreenState(windowHandle);
		EXPORT_END;
	}

	void WindowFactory::SetFullscreenState(WindowHandle* windowHandle, WindowFullscreenState fullscreenState) {
		std::lock_guard<std::mutex> lockGuard { globalWindowLock };
		if (windowHandle == nullptr) throw LosgapException { "Window handle must not be null." };

		if (INTEROP_BOOL_TO_CBOOL(windowHandle->windowClosureFlag)) return;

		// Step 1: Set device/swap chain state to non-fullscreen if that's what we've been asked to do
		HRESULT setStateResult = 0;
		if (fullscreenState != WindowFullscreenState::StandardFullscreen) {
			HRESULT setStateResult = windowHandle->swapChainPtr->SetFullscreenState(FALSE, nullptr);

			if (setStateResult == DXGI_ERROR_NOT_CURRENTLY_AVAILABLE) {
				OutputDebugString(L"Couldn't set window non-fullscreen, it is likely occluded or unfocussed.");
			}
			else if (setStateResult == DXGI_STATUS_MODE_CHANGE_IN_PROGRESS) {
				OutputDebugString(L"Fullscreen transition already in progress.");
			}
			else CHECK_CALL(setStateResult);
		}

		// Step 2: Set window border etc
		RECT outClientRect;
		ZeroMemory(&outClientRect, sizeof(outClientRect));

		if (FALSE == GetClientRect(windowHandle->windowHandle, &outClientRect)) {
			throw LosgapException { "Could not get window client area." };
		}

		LONG x = outClientRect.left;
		LONG y = outClientRect.top;
		LONG width = outClientRect.right - outClientRect.left;
		LONG height = outClientRect.bottom - outClientRect.top;

		LONG windowStyle = GetWindowLong(windowHandle->windowHandle, GWL_STYLE);
		LONG windowStyleEx = GetWindowLong(windowHandle->windowHandle, GWL_EXSTYLE);

		DXGI_OUTPUT_DESC targetOutputDesc;
		BOOL getMonitorInfoResult = FALSE;
		CHECK_CALL(DeviceFactory::GetCurrentOutput()->GetDesc(&targetOutputDesc));
		HMONITOR targetMonitor = targetOutputDesc.Monitor;
		MONITORINFO outMonitorInfo;
		ZeroMemory(&outMonitorInfo, sizeof(outMonitorInfo));
		outMonitorInfo.cbSize = sizeof(outMonitorInfo);
		getMonitorInfoResult = GetMonitorInfo(targetMonitor, &outMonitorInfo);

		if (TRUE == getMonitorInfoResult) {
			x = outMonitorInfo.rcMonitor.left;
			y = outMonitorInfo.rcMonitor.top;
		}
		else {
			OutputDebugString(LosgapString::AsNewCWString("Failed to get monitor info for setting borderless mode.").get());
		}

		if (fullscreenState != WindowFullscreenState::NotFullscreen) {
			windowStyle &= ~WINDOW_BORDER_STYLE;
			windowStyleEx &= ~WINDOW_BORDER_STYLE_EX;

			width = outMonitorInfo.rcMonitor.right - outMonitorInfo.rcMonitor.left;
			height = outMonitorInfo.rcMonitor.bottom - outMonitorInfo.rcMonitor.top;
		}
		else {
			windowStyle |= WINDOW_BORDER_STYLE;
			windowStyleEx |= WINDOW_BORDER_STYLE_EX;

			// Set client size if not fullscreen (otherwise the DXGI device takes care of it for us)
			if (fullscreenState == WindowFullscreenState::NotFullscreen) {
				width = windowHandle->desiredResWidth;
				height = windowHandle->desiredResHeight;
			}
		}

		SetWindowLong(windowHandle->windowHandle, GWL_STYLE, windowStyle);
		SetWindowLong(windowHandle->windowHandle, GWL_EXSTYLE, windowStyleEx);

		// Step 1b: "Resize" window to force border changes
		BOOL setWindowResult = SetWindowPos(
			windowHandle->windowHandle,
			NULL,
			x,
			y,
			width,
			height,
			SWP_FRAMECHANGED | SWP_NOZORDER | SWP_SHOWWINDOW
			);

		if (FALSE == setWindowResult) {
			OutputDebugString(LosgapString::AsNewCWString("Set window failed when adjusting fullscreen state.").get());
		}

		// Step 3: Set device/swap chain state to fullscreen if that's what we've been asked to do
		if (fullscreenState == WindowFullscreenState::StandardFullscreen) {
			HRESULT setStateResult = windowHandle->swapChainPtr->SetFullscreenState(TRUE, DeviceFactory::GetCurrentOutput());

			if (setStateResult == DXGI_ERROR_NOT_CURRENTLY_AVAILABLE) {
				OutputDebugString(L"Couldn't set window fullscreen, it is likely occluded or unfocussed.");
			}
			else if (setStateResult == DXGI_STATUS_MODE_CHANGE_IN_PROGRESS) {
				OutputDebugString(L"Fullscreen transition already in progress.");
			}
			else CHECK_CALL(setStateResult);
		}

		windowHandle->desiredFullscreenState = fullscreenState;
	}
	EXPORT(WindowFactory_SetFullscreenState, WindowHandle* windowHandle, WindowFullscreenState fullscreenState) {
		WindowFactory::SetFullscreenState(windowHandle, fullscreenState);
		EXPORT_END;
	}
#pragma endregion

#pragma region ::SetResolution()
	void WindowFactory::SetResolution(WindowHandle* windowHandle, uint32_t widthPx, uint32_t heightPx) {
		std::lock_guard<std::mutex> lockGuard { globalWindowLock };
		if (windowHandle == nullptr) throw LosgapException { "Window handle must not be null." };

		if (INTEROP_BOOL_TO_CBOOL(windowHandle->windowClosureFlag)) return;

		DXGI_MODE_DESC newTargetDesc;
		ZeroMemory(&newTargetDesc, sizeof(newTargetDesc));
		newTargetDesc.Format = DeviceFactory::SUPPORTED_RTV_FORMAT;
		newTargetDesc.Height = heightPx;
		newTargetDesc.Width = widthPx;
		if (DeviceFactory::GetVSyncEnabled()) {
			newTargetDesc.RefreshRate = DeviceFactory::GetNativeRefreshRate(nullptr, widthPx, heightPx);
		}
		else {
			newTargetDesc.RefreshRate.Numerator = 0U;
			newTargetDesc.RefreshRate.Denominator = 1U;
		}
		newTargetDesc.Scaling = DeviceFactory::SWAP_CHAIN_SCALING_MODE;
		newTargetDesc.ScanlineOrdering = DeviceFactory::SWAP_CHAIN_SCANLINE_ORDER;
		CHECK_CALL(windowHandle->swapChainPtr->ResizeTarget(&newTargetDesc));

		windowHandle->desiredResWidth = widthPx;
		windowHandle->desiredResHeight = heightPx;

		if (windowHandle->desiredFullscreenState == WindowFullscreenState::BorderlessFullscreen) {
			DXGI_OUTPUT_DESC targetOutputDesc;
			CHECK_CALL(DeviceFactory::GetCurrentOutput()->GetDesc(&targetOutputDesc));
			HMONITOR targetMonitor = targetOutputDesc.Monitor;
			MONITORINFO outMonitorInfo;
			ZeroMemory(&outMonitorInfo, sizeof(outMonitorInfo));
			outMonitorInfo.cbSize = sizeof(outMonitorInfo);
			BOOL getMonitorInfoResult = GetMonitorInfo(targetMonitor, &outMonitorInfo);

			if (FALSE == getMonitorInfoResult) {
				OutputDebugString(LosgapString::AsNewCWString("Failed to get monitor info for setting borderless mode.").get());
				return;
			}

			BOOL setWindowResult = SetWindowPos(
				windowHandle->windowHandle,
				NULL,
				outMonitorInfo.rcMonitor.left,
				outMonitorInfo.rcMonitor.top,
				outMonitorInfo.rcMonitor.right - outMonitorInfo.rcMonitor.left,
				outMonitorInfo.rcMonitor.bottom - outMonitorInfo.rcMonitor.top,
				SWP_NOZORDER | SWP_SHOWWINDOW
			);

			if (FALSE == setWindowResult) {
				OutputDebugString(LosgapString::AsNewCWString("Set window failed when adjusting fullscreen state.").get());
				return;
			}
		}
	}
	EXPORT(WindowFactory_SetResolution, WindowHandle* windowHandle, uint32_t widthPx, uint32_t heightPx) {
		WindowFactory::SetResolution(windowHandle, widthPx, heightPx);
		EXPORT_END;
	}
#pragma endregion

#pragma region ::[Get/Set]ClientPosition
	void WindowFactory::GetClientPosition(WindowHandle* windowHandle, int32_t* outX, int32_t* outY) {
		std::lock_guard<std::mutex> lockGuard { globalWindowLock };
		if (windowHandle == nullptr) throw LosgapException { "Window handle must not be null." };

		if (INTEROP_BOOL_TO_CBOOL(windowHandle->windowClosureFlag)) {
			*outX = 0U;
			*outY = 0U;
			return;
		}

		if (FALSE == IsWindow(windowHandle->windowHandle)) throw LosgapException { "Invalid window handle." };
		
		POINT outClientCorner;
		ZeroMemory(&outClientCorner, sizeof(outClientCorner));

		if (FALSE == ClientToScreen(windowHandle->windowHandle, &outClientCorner)) {
			throw LosgapException { "Could not acquire screen co-ordinates for window." };
		}

		*outX = static_cast<int32_t>(outClientCorner.x);
		*outY = static_cast<int32_t>(outClientCorner.y);
	}
	EXPORT(WindowFactory_GetClientPosition, WindowHandle* windowHandle, int32_t* outX, int32_t* outY) {
		WindowFactory::GetClientPosition(windowHandle, outX, outY);
		EXPORT_END;
	}
	void WindowFactory::SetClientPosition(WindowHandle* windowHandle, int32_t x, int32_t y) {
		
	}
#pragma endregion

#pragma region ::[G/S]etCursorVisibility()
	bool WindowFactory::GetCursorVisibility(WindowHandle* windowHandle) {
		std::lock_guard<std::mutex> lockGuard { globalWindowLock };
		if (windowHandle == nullptr) throw LosgapException { "Window handle must not be null." };

		if (INTEROP_BOOL_TO_CBOOL(windowHandle->windowClosureFlag)) return true;

		return windowHandle->showCursor;
	}
	EXPORT(WindowFactory_GetCursorVisibility, WindowHandle* windowHandle, INTEROP_BOOL& outVisibilityState) {
		outVisibilityState = CBOOL_TO_INTEROP_BOOL(WindowFactory::GetCursorVisibility(windowHandle));
		EXPORT_END;
	}

	void WindowFactory::SetCursorVisibility(WindowHandle* windowHandle, bool visibilityState) {
		std::lock_guard<std::mutex> lockGuard { globalWindowLock };
		if (windowHandle == nullptr) throw LosgapException { "Window handle must not be null." };

		if (INTEROP_BOOL_TO_CBOOL(windowHandle->windowClosureFlag)) return;

		windowHandle->showCursor = visibilityState;
	}
	EXPORT(WindowFactory_SetCursorVisibility, WindowHandle* windowHandle, INTEROP_BOOL visibilityState) {
		WindowFactory::SetCursorVisibility(windowHandle, INTEROP_BOOL_TO_CBOOL(visibilityState));
		EXPORT_END;
	}
#pragma endregion

#pragma region ::SetWindowTitle()
	void WindowFactory::SetWindowTitle(WindowHandle* windowHandle, LosgapString windowTitle) {
		std::lock_guard<std::mutex> lockGuard { globalWindowLock };
		if (windowHandle == nullptr) throw LosgapException { "Window handle must not be null." };

		if (INTEROP_BOOL_TO_CBOOL(windowHandle->windowClosureFlag)) return;

		if (FALSE == SetWindowTextW(windowHandle->windowHandle, LosgapString::AsNewCWString(windowTitle).get())) {
			OutputDebugStringW(LosgapString::AsNewCWString(LosgapString::Concat("Could not set window title to ", windowTitle, "!")).get());
		}
	}
	EXPORT(WindowFactory_SetWindowTitle, WindowHandle* windowHandle, INTEROP_STRING windowTitle) {
		if (windowTitle == nullptr) EXPORT_FAIL("Window title must not be null!");
		WindowFactory::SetWindowTitle(windowHandle, windowTitle);
		EXPORT_END;
	}
#pragma endregion

#pragma region ::GetFocusedWindowHandle()
	WindowHandle* WindowFactory::GetFocusedWindowHandle() {
		HWND hwnd = GetForegroundWindow();
		if (hwnd == 0) return nullptr;

		std::lock_guard<std::mutex> lockGuard { globalWindowLock };

		if (windowHandleMapping.find(hwnd) == windowHandleMapping.end()) return nullptr;
		return windowHandleMapping.at(hwnd);
	}
	EXPORT(WindowFactory_GetFocusedWindowHandle, WindowHandle** outWindowHandlePtr) {
		*outWindowHandlePtr = WindowFactory::GetFocusedWindowHandle();
		EXPORT_END;
	}
#pragma endregion

#pragma region ::[Create/Alter/Destroy]Viewport()
	D3D11_VIEWPORT* WindowFactory::CreateViewport() {
		std::lock_guard<std::mutex> lockGuard { globalWindowLock };

		D3D11_VIEWPORT* result = new D3D11_VIEWPORT { };
		ZeroMemory(result, sizeof(*result));
		result->MinDepth = 0.0f;
		result->MaxDepth = 1.0f;
		return result;
	}
	EXPORT(WindowFactory_CreateViewport, D3D11_VIEWPORT** outViewportPtr) {
		*outViewportPtr = WindowFactory::CreateViewport();
		EXPORT_END;
	}
	void WindowFactory::AlterViewport(D3D11_VIEWPORT* viewportHandle, uint32_t topLeftX, uint32_t topLeftY,
		uint32_t widthPx, uint32_t heightPx) {
		std::lock_guard<std::mutex> lockGuard { globalWindowLock };

		viewportHandle->Height = static_cast<FLOAT>(heightPx);
		viewportHandle->Width = static_cast<FLOAT>(widthPx);
		viewportHandle->TopLeftX = static_cast<FLOAT>(topLeftX);
		viewportHandle->TopLeftY = static_cast<FLOAT>(topLeftY);
	}
	EXPORT(WindowFactory_AlterViewport, D3D11_VIEWPORT* viewportPtr, uint32_t topLeftX, uint32_t topLeftY,
		uint32_t widthPx, uint32_t heightPx) {
		WindowFactory::AlterViewport(viewportPtr, topLeftX, topLeftY, widthPx, heightPx);
		EXPORT_END;
	}
	void WindowFactory::DestroyViewport(D3D11_VIEWPORT* viewportHandle) {
		std::lock_guard<std::mutex> lockGuard { globalWindowLock };

		delete viewportHandle;
	}
	EXPORT(WindowFactory_DestroyViewport, D3D11_VIEWPORT* viewportPtr) {
		WindowFactory::DestroyViewport(viewportPtr);
		EXPORT_END;
	}
#pragma endregion

#pragma region ::GetWindow[BackBufferRTVAndDSV/SwapChain]
	void WindowFactory::GetWindowBackBufferRTVAndDSV(WindowHandle* windowHandle, ID3D11RenderTargetView** outRTVPtr, ID3D11DepthStencilView** outDSVPtr) {
		std::lock_guard<std::mutex> lockGuard { globalWindowLock };
		if (windowHandle == nullptr) throw LosgapException { "Window handle must not be null." };
		if (INTEROP_BOOL_TO_CBOOL(windowHandle->windowClosureFlag)) {
			*outRTVPtr = nullptr;
			*outDSVPtr = nullptr;
		}
		else {
			*outRTVPtr = windowHandle->rtvPtr;
			*outDSVPtr = windowHandle->dsvPtr;
		}
	}
	EXPORT(WindowFactory_GetWindowBackBufferRTVAndDSV, WindowHandle* windowHandle, ID3D11RenderTargetView** outRTVPtr, ID3D11DepthStencilView** outDSVPtr) {
		WindowFactory::GetWindowBackBufferRTVAndDSV(windowHandle, outRTVPtr, outDSVPtr);
		EXPORT_END;
	}

	IDXGISwapChain* WindowFactory::GetWindowSwapChain(WindowHandle* windowHandle) {
		std::lock_guard<std::mutex> lockGuard { globalWindowLock };
		if (windowHandle == nullptr) throw LosgapException { "Window handle must not be null." };
		if (INTEROP_BOOL_TO_CBOOL(windowHandle->windowClosureFlag)) return nullptr;

		return windowHandle->swapChainPtr;
	}
	EXPORT(WindowFactory_GetWindowSwapChain, WindowHandle* windowHandle, IDXGISwapChain** outSwapChainPtr) {
		*outSwapChainPtr = WindowFactory::GetWindowSwapChain(windowHandle);
		EXPORT_END;
	}
#pragma endregion

#pragma region ::HydrateMessagePump()/ClearWindow()
	bool WindowFactory::HydrateMessagePump(WindowHandle* windowHandle) {
		std::lock_guard<std::mutex> lockGuard { globalWindowLock };
		if (windowHandle == nullptr) throw LosgapException { "Window handle can not be null." };

		if (INTEROP_BOOL_TO_CBOOL(windowHandle->windowClosureFlag)) return false;

		if (FALSE == IsWindow(windowHandle->windowHandle)) throw LosgapException { "Invalid window handle." };

		MSG outMsg;
		ZeroMemory(&outMsg, sizeof(outMsg));
		
		while (PeekMessage(&outMsg, windowHandle->windowHandle, 0U, 0U, PM_REMOVE)) {
			TranslateMessage(&outMsg);
			DispatchMessage(&outMsg);

			ZeroMemory(&outMsg, sizeof(outMsg));
		}

		if (windowHandle->resizeSinceLastCheck) {
			windowHandle->resizeSinceLastCheck = false;
			return true;
		}
		else return false;
	}
	EXPORT(WindowFactory_HydrateMessagePump, WindowHandle* windowHandle, INTEROP_BOOL* outWindowResized) {
		*outWindowResized = WindowFactory::HydrateMessagePump(windowHandle);
		EXPORT_END;
	}

	static const FLOAT BACK_BUFFER_CLEAR_COLOR[4] = { 0.0f, 0.0f, 0.0f, 0.0f };
	void WindowFactory::ClearWindow(ID3D11DeviceContext* deviceContextPtr, WindowHandle* windowHandle) {
		if (deviceContextPtr == nullptr) throw LosgapException { "Device context pointer must not be null." };

		std::lock_guard<std::mutex> lockGuard { globalWindowLock };
		if (windowHandle == nullptr) throw LosgapException { "Window handle can not be null." };

		if (INTEROP_BOOL_TO_CBOOL(windowHandle->windowClosureFlag)) return;

		deviceContextPtr->ClearRenderTargetView(windowHandle->rtvPtr, BACK_BUFFER_CLEAR_COLOR);
		deviceContextPtr->ClearDepthStencilView(windowHandle->dsvPtr, D3D11_CLEAR_DEPTH | D3D11_CLEAR_STENCIL, 1.0f, 0xFF);
	}
	EXPORT(WindowFactory_ClearWindow, ID3D11DeviceContext* deviceContextPtr, WindowHandle* windowHandle) {
		WindowFactory::ClearWindow(deviceContextPtr, windowHandle);
		EXPORT_END;
	}
#pragma endregion

#pragma region ::CloseWindow()
	void WindowFactory::CloseWindow(WindowHandle* windowHandle) {
		std::lock_guard<std::mutex> lockGuard { globalWindowLock };
		if (windowHandle == nullptr) throw LosgapException { "Window handle can not be null." };

		if (INTEROP_BOOL_TO_CBOOL(windowHandle->windowClosureFlag)) return;

		HWND hWnd = windowHandle->windowHandle;

		if (FALSE == IsWindow(hWnd)) throw LosgapException { "Invalid window handle." };
		if (FALSE == ::CloseWindow(hWnd)) throw LosgapException { "Could not close window." };
	}
	EXPORT(WindowFactory_CloseWindow, WindowHandle* windowHandle) {
		WindowFactory::CloseWindow(windowHandle);
		EXPORT_END;
	}
#pragma endregion

#pragma region ::CleanUpWindowResources() 
	void WindowFactory::CleanUpWindowResources(WindowHandle* windowHandle) {
		std::lock_guard<std::mutex> lockGuard { globalWindowLock };
		if (windowHandle->swapChainPtr == nullptr) throw LosgapException { "Can not release null swap chain." };
		
		windowHandle->swapChainPtr->SetFullscreenState(FALSE, nullptr);

		RELEASE_COM(windowHandle->dsvPtr);
		RELEASE_COM(windowHandle->rtvPtr);
		RELEASE_COM(windowHandle->swapChainPtr);

		windowHandleMapping.erase(windowHandle->windowHandle);
		delete windowHandle;
	}
	EXPORT(WindowFactory_CleanUpWindowResources, WindowHandle* windowHandle) {
		WindowFactory::CleanUpWindowResources(windowHandle);
		EXPORT_END;
	}
#pragma endregion

#pragma region [Unlock/Lock]WindowModifications
	void WindowFactory::LockWindowModifications() {
		globalWindowLock.lock();
	}

	void WindowFactory::UnlockWindowModifications() {
		globalWindowLock.unlock();
	}
#pragma endregion

#pragma region ::HandleWindowMessage()
	void HandleWindowMessage_Size(HWND window, UINT umsg, WPARAM wParam, LPARAM lParam) {
		if (windowHandleMapping.find(window) == windowHandleMapping.end()) return;
		WindowHandle* windowHandle = windowHandleMapping[window];

		if (INTEROP_BOOL_TO_CBOOL(windowHandle->windowClosureFlag) || LOWORD(lParam) <= 0 || HIWORD(lParam) <= 0) return;

		RELEASE_COM(windowHandle->dsvPtr);
		RELEASE_COM(windowHandle->rtvPtr);

		HRESULT resizeResult = windowHandle->swapChainPtr->ResizeBuffers(0U, LOWORD(lParam), HIWORD(lParam), DXGI_FORMAT_UNKNOWN, DXGI_SWAP_CHAIN_FLAG_ALLOW_MODE_SWITCH);
		if (FAILED(resizeResult)) {
			OutputDebugString(LosgapString::AsNewCWString(
				"NATIVE ERROR: Attempted to resize swap chain, but failed: " + DESCRIPTION(resizeResult) + "\r\n"
				).get());
		}

		try {
			WindowFactory::SetDSVAndRTVOnWindowHandle(windowHandle, LOWORD(lParam), HIWORD(lParam));
		}
		catch (const LosgapException& e) {
			OutputDebugString(LosgapString::AsNewCWString(
				"NATIVE ERROR: Handling window resize, but setting of DSV/RTV threw exception: " + e.Message + "\r\n"
				).get());
		}

		windowHandle->resizeSinceLastCheck = true;
	}

	void HandleWindowMessage_SetCursor(HWND window, UINT umsg, WPARAM wParam, LPARAM lParam) {
		if (windowHandleMapping.find(window) == windowHandleMapping.end()) {
			SetCursor(LoadCursor(nullptr, IDC_ARROW));
			return;
		}
		WindowHandle* windowHandle = windowHandleMapping[window];

		if (INTEROP_BOOL_TO_CBOOL(windowHandle->windowClosureFlag)) return;

		POINT outCursorPosition;
		RECT outClientRect;
		RECT outWindowRect;
		if (FALSE == GetCursorPos(&outCursorPosition)) {
			OutputDebugString(LosgapString::AsNewCWString(
				"Could not set cursor style: GetCursorPos returned FALSE."
			).get());
			return;
		}
		if (FALSE == GetClientRect(window, &outClientRect)) {
			OutputDebugString(LosgapString::AsNewCWString(
				"Could not set cursor style: GetClientRect returned FALSE."
				).get());
			return;
		}
		ClientToScreen(window, reinterpret_cast<POINT*>(&outClientRect.left));
		ClientToScreen(window, reinterpret_cast<POINT*>(&outClientRect.right));
		if (FALSE == GetWindowRect(window, &outWindowRect)) {
			OutputDebugString(LosgapString::AsNewCWString(
				"Could not set cursor style: GetWindowRect returned FALSE."
				).get());
			return;
		}

		if (outCursorPosition.x < outWindowRect.left
			|| outCursorPosition.x > outWindowRect.right
			|| outCursorPosition.y < outWindowRect.top
			|| outCursorPosition.y > outWindowRect.bottom) {
			SetCursor(LoadCursor(nullptr, IDC_ARROW));
		}
		else {
			bool isInVerticalBorder = outCursorPosition.x < outClientRect.left
				|| outCursorPosition.x > outClientRect.right;
			bool isInHorizontalBorder = outCursorPosition.y < outClientRect.top
				|| outCursorPosition.y > outClientRect.bottom;
			
			if (isInHorizontalBorder) {
				bool isInTitleBar = outCursorPosition.y < outClientRect.bottom
					&& outCursorPosition.y > outWindowRect.top + GetSystemMetrics(SM_CYSIZEFRAME);

				if (isInVerticalBorder) {
					if (isInTitleBar) SetCursor(LoadCursor(nullptr, IDC_SIZEWE));
					else if ((outCursorPosition.x < outClientRect.left && outCursorPosition.y < outClientRect.top)
						|| (outCursorPosition.x > outClientRect.right && outCursorPosition.y > outClientRect.bottom)) {
						SetCursor(LoadCursor(nullptr, IDC_SIZENWSE));
					}
					else SetCursor(LoadCursor(nullptr, IDC_SIZENESW));
				}
				else if (isInTitleBar) SetCursor(LoadCursor(nullptr, IDC_ARROW));
				else SetCursor(LoadCursor(nullptr, IDC_SIZENS));
			}
			else if (isInVerticalBorder) SetCursor(LoadCursor(nullptr, IDC_SIZEWE));
			else if (windowHandle->showCursor) SetCursor(LoadCursor(nullptr, IDC_ARROW)); 
			else SetCursor(nullptr);
		}
	}

	void HandleWindowMessage_Close(HWND window, UINT umsg, WPARAM wParam, LPARAM lParam) { 
		if (windowHandleMapping.find(window) == windowHandleMapping.end()) {
			if (FALSE == DestroyWindow(window)) {
				OutputDebugString(L"Failed to destroy window after WM_CLOSE message.");
			}
			return;
		}
		WindowHandle* windowHandle = windowHandleMapping[window];

		if (FALSE == DestroyWindow(window)) {
			OutputDebugString(L"Failed to destroy window after WM_CLOSE message.");
		}

		windowHandle->windowClosureFlag = INTEROP_BOOL_TRUE;
	}

	LRESULT CALLBACK WindowFactory::HandleWindowMessage(HWND window, UINT umsg, WPARAM wParam, LPARAM lParam) {
		switch (umsg) {
			case WM_SIZE: 
				HandleWindowMessage_Size(window, umsg, wParam, lParam);
				return 0L;
			case WM_SETCURSOR:
				HandleWindowMessage_SetCursor(window, umsg, wParam, lParam);
				return TRUE;
			case WM_CLOSE:
				HandleWindowMessage_Close(window, umsg, wParam, lParam);
				return 0L;
			default: return DefWindowProc(window, umsg, wParam, lParam);
		}
	}
#pragma endregion

#pragma region ::SetDSVAndRTVOnWindowHandle
	void WindowFactory::SetDSVAndRTVOnWindowHandle(WindowHandle* windowHandle, uint32_t width, uint32_t height) {
		bool isMS = DeviceFactory::GetMSAALevel() > 1U;

		RAIICOMWrapper<ID3D11Texture2D*> backBufferPtr;
		CHECK_CALL(windowHandle->swapChainPtr->GetBuffer(0U, __uuidof(ID3D11Texture2D), reinterpret_cast<void**>(backBufferPtr.GetPtr())));

		windowHandle->rtvPtr = ResourceFactory::CreateRTV(
			windowHandle->devicePtr, 
			backBufferPtr.Get(), 
			0U, 
			DeviceFactory::SUPPORTED_RTV_FORMAT,
			isMS
		);

		RAIICOMWrapper<ID3D11Texture2D*> dsTexPtr { 
			ResourceFactory::CreateTexture2DArray(
				windowHandle->devicePtr, width, height,
				1U, false, true, DXGI_FORMAT_D24_UNORM_S8_UINT, D3D11_USAGE_DEFAULT, static_cast<D3D11_CPU_ACCESS_FLAG>(0), 
				D3D11_BIND_DEPTH_STENCIL, false, false, nullptr
			) 
		};

		windowHandle->dsvPtr = ResourceFactory::CreateDSV(
			windowHandle->devicePtr, 
			dsTexPtr.Get(), 
			0U, 
			DeviceFactory::SUPPORTED_DSV_FORMAT,
			isMS
		);
	}
#pragma endregion
}