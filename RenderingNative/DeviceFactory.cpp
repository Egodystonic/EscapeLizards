// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created by Ben Bowen

#include "DeviceFactory.h"
#include "ResourceFactory.h"
#include <vector>

extern "C" {
	__declspec(dllexport) DWORD NvOptimusEnablement = 0x00000001;
	__declspec(dllexport) int AmdPowerXpressRequestHighPerformance = 1;
}

namespace losgap {
	std::atomic<uint32_t> DeviceFactory::msaaLevel = 1U;
	std::atomic<bool> DeviceFactory::vsyncEnabled = true;

	IDXGIFactory* dxgiFactory;
	std::vector<IDXGIAdapter*> adapterPtrVect;
	std::vector<std::vector<IDXGIOutput*>> outputPtrVectVect;
	GPUDesc* gpuDescArr = nullptr;
	IDXGIAdapter* selectedAdapterPtr = nullptr;
	IDXGIOutput* selectedOutputPtr = nullptr;

#pragma region ::Init()
	void CreateGPUDescArray() {
		if (dxgiFactory == nullptr) {
			throw LosgapException { "DXGI Factory not instantiated. A call to init may have failed, or the call may not have been made. " };
		}

		gpuDescArr = new GPUDesc[adapterPtrVect.size()];
		for (size_t i = 0; i < adapterPtrVect.size(); ++i) {
#pragma warning(suppress: 6386) // Justification: I can't see how it thinks that will ever write past gpuDescArr... No matter how hard I try.
			gpuDescArr[i].Index = static_cast<int32_t>(i);
			DXGI_ADAPTER_DESC outAdapterDesc;
			ZeroMemory(&outAdapterDesc, sizeof(outAdapterDesc));
			CHECK_CALL(adapterPtrVect[i]->GetDesc(&outAdapterDesc));
			gpuDescArr[i].Name = LosgapString::AsNewChar16String(outAdapterDesc.Description).release();
			gpuDescArr[i].DedicatedVideoMemory = outAdapterDesc.DedicatedVideoMemory;
			gpuDescArr[i].DedicatedSystemMemory = outAdapterDesc.DedicatedSystemMemory;
			gpuDescArr[i].SharedSystemMemory = outAdapterDesc.SharedSystemMemory;
			gpuDescArr[i].NumOutputs = static_cast<int32_t>(outputPtrVectVect[i].size());
			gpuDescArr[i].Outputs = new GPUOutputDesc[outputPtrVectVect[i].size()];

			for (size_t j = 0; j < outputPtrVectVect[i].size(); ++j) {
				gpuDescArr[i].Outputs[j].Index = static_cast<int32_t>(j);
				DXGI_OUTPUT_DESC outOutputDesc;
				ZeroMemory(&outOutputDesc, sizeof(outOutputDesc));
				CHECK_CALL(outputPtrVectVect[i][j]->GetDesc(&outOutputDesc));
				gpuDescArr[i].Outputs[j].Name = LosgapString::AsNewChar16String(outOutputDesc.DeviceName).release();
				MONITORINFO outMonitorInfo;
				ZeroMemory(&outMonitorInfo, sizeof(outMonitorInfo));
				outMonitorInfo.cbSize = sizeof(outMonitorInfo);
				if (FALSE == GetMonitorInfo(outOutputDesc.Monitor, &outMonitorInfo)) {
					throw LosgapException { 
						LosgapString::Concat("Could not get monitor info for output '", gpuDescArr[i].Outputs[j].Name, "'.")
					};
				}
				gpuDescArr[i].Outputs[j].IsPrimaryOutput = outMonitorInfo.dwFlags & MONITORINFOF_PRIMARY;

				UINT numDisplayModes;
				CHECK_CALL(outputPtrVectVect[i][j]->GetDisplayModeList(
					DeviceFactory::SUPPORTED_RTV_FORMAT,
					DXGI_ENUM_MODES_INTERLACED,
					&numDisplayModes,
					nullptr
					));

				gpuDescArr[i].Outputs[j].NumNativeResolutions = static_cast<int32_t>(numDisplayModes);
				gpuDescArr[i].Outputs[j].NativeResolutions = new NativeOutputResolution[numDisplayModes];
				std::vector<DXGI_MODE_DESC> modeDescVect { numDisplayModes };
				CHECK_CALL(outputPtrVectVect[i][j]->GetDisplayModeList(
					DeviceFactory::SUPPORTED_RTV_FORMAT,
					DXGI_ENUM_MODES_INTERLACED,
					&numDisplayModes,
					&modeDescVect[0]
					));

				for (size_t k = 0; k < numDisplayModes; ++k) {
					gpuDescArr[i].Outputs[j].NativeResolutions[k].Index = static_cast<int32_t>(k);
					gpuDescArr[i].Outputs[j].NativeResolutions[k].ResX = modeDescVect[k].Width;
					gpuDescArr[i].Outputs[j].NativeResolutions[k].ResY = modeDescVect[k].Height;
					gpuDescArr[i].Outputs[j].NativeResolutions[k].RefreshRateNumerator = modeDescVect[k].RefreshRate.Numerator;
					gpuDescArr[i].Outputs[j].NativeResolutions[k].RefreshRateDenominator = modeDescVect[k].RefreshRate.Denominator;
				}
			}
		}
	}
	void DeviceFactory::Init() {
		if (dxgiFactory != nullptr) throw LosgapException { "Device factory has already been initialised!" };
		CHECK_CALL(CreateDXGIFactory(__uuidof(IDXGIFactory), (void**) &dxgiFactory));

		IDXGIAdapter* outAdapterPtr = nullptr;
		for (size_t i = 0; /* break below */; ++i) {
			HRESULT enumAdapterRes = dxgiFactory->EnumAdapters(static_cast<UINT>(i), &outAdapterPtr);
			if (enumAdapterRes == DXGI_ERROR_NOT_FOUND) break;
			else if (FAILED(enumAdapterRes)) {
				throw LosgapException { "Could not enumerate adapters: " + DESCRIPTION(enumAdapterRes) };
			}
			else {
				adapterPtrVect.push_back(outAdapterPtr);
			}

			outputPtrVectVect.push_back(std::vector<IDXGIOutput*> { });

			IDXGIOutput* outOutputPtr = nullptr;
			for (size_t j = 0; /* break below */; ++j) {
				HRESULT enumOutputRes = outAdapterPtr->EnumOutputs(static_cast<UINT>(j), &outOutputPtr);
				if (enumOutputRes == DXGI_ERROR_NOT_FOUND) break;
				else if (FAILED(enumOutputRes)) {
					throw LosgapException { "Could not enumerate outputs: " + DESCRIPTION(enumOutputRes) };
				}
				else {
					outputPtrVectVect[i].push_back(outOutputPtr);
				}
			}
		}

		CreateGPUDescArray();
	}
	EXPORT(DeviceFactory_Init, GPUDesc** outDescArrPtr, int32_t* outDescArrLen) {
		DeviceFactory::Init();
		*outDescArrPtr = gpuDescArr;
		*outDescArrLen = static_cast<int32_t>(adapterPtrVect.size());
		EXPORT_END
	}
#pragma endregion
	
#pragma region ::CreateDevice()
	ID3D11Device* DeviceFactory::CreateDevice(int32_t adapterIndex, int32_t outputAdapterIndex, int32_t outputIndex, bool& outSupportsMTRendering) {
		if (dxgiFactory == nullptr) throw LosgapException { "Init device factory first." };
		if (adapterIndex < 0 || outputIndex < 0 || outputAdapterIndex < 0) throw LosgapException { "Adapter or output index was negative." };
		if (static_cast<size_t>(adapterIndex) >= adapterPtrVect.size()
			|| static_cast<size_t>(outputAdapterIndex) >= adapterPtrVect.size()
			|| static_cast<size_t>(outputIndex) >= outputPtrVectVect[outputAdapterIndex].size()) {
			throw LosgapException { "Adapter or output index were out of bounds." };
		}

		selectedAdapterPtr = adapterPtrVect[adapterIndex];
		selectedOutputPtr = outputPtrVectVect[outputAdapterIndex][outputIndex];

#ifdef DEBUG 
#define DEBUG_CREATION_FLAG D3D11_CREATE_DEVICE_DEBUG
#else 
#define DEBUG_CREATION_FLAG 0
#endif

		ID3D11Device* outDevicePtr;
		ID3D11DeviceContext* outContextPtr;
		D3D_FEATURE_LEVEL outFeatureLevel;

		CHECK_CALL(D3D11CreateDevice(
			selectedAdapterPtr,
			D3D_DRIVER_TYPE_UNKNOWN,
			nullptr,
			D3D11_CREATE_DEVICE_PREVENT_ALTERING_LAYER_SETTINGS_FROM_REGISTRY | DEBUG_CREATION_FLAG,
			&SUPPORTED_FEATURE_LEVEL,
			1,
			D3D11_SDK_VERSION,
			&outDevicePtr,
			&outFeatureLevel,
			&outContextPtr
		));

		RELEASE_COM(outContextPtr); // We don't use this here

		if (outFeatureLevel != SUPPORTED_FEATURE_LEVEL) {
			throw LosgapException { "Selected GPU does not support DX11." };
		}
		if (outDevicePtr == nullptr) {
			throw LosgapException { "Failed to get device." };
		}

		
		// Check that this driver supports multithreaded rendering
		D3D11_FEATURE_DATA_THREADING threadingCaps = { FALSE, FALSE };
		CHECK_CALL(outDevicePtr->CheckFeatureSupport(D3D11_FEATURE_THREADING, &threadingCaps, sizeof(threadingCaps)));
		outSupportsMTRendering = threadingCaps.DriverCommandLists == TRUE;

		return outDevicePtr;
	}
	EXPORT(DeviceFactory_CreateDevice, int32_t selectedAdapterIndex, int32_t selectedOutputAdapterIndex, int32_t selectedOutputIndex, ID3D11Device** outDevicePtrPtr,
		INTEROP_BOOL* outSupportsMTRendering) {
		bool outMTSupport;
		*outDevicePtrPtr = DeviceFactory::CreateDevice(selectedAdapterIndex, selectedOutputAdapterIndex, selectedOutputIndex, outMTSupport);
		if (*outDevicePtrPtr == nullptr) EXPORT_FAIL("Failed to get device pointer.");
		*outSupportsMTRendering = CBOOL_TO_INTEROP_BOOL(outMTSupport);
		EXPORT_END;
	}
#pragma endregion

#pragma region ::[G/S]etMSAALevel()
	uint32_t DeviceFactory::GetMSAALevel() {
		return msaaLevel;
	}
	EXPORT(DeviceFactory_GetMSAALevel, uint32_t& outMSAALevel) {
		outMSAALevel = DeviceFactory::GetMSAALevel();
		EXPORT_END;
	}
	void DeviceFactory::SetMSAALevel(uint32_t msaaLevel) {
		if (msaaLevel == 0) msaaLevel = 1;
		if ((msaaLevel & (msaaLevel - 1)) != 0) throw LosgapException { "MSAA level must be a power of two!" };
		DeviceFactory::msaaLevel = msaaLevel;
	}
	EXPORT(DeviceFactory_SetMSAALevel, uint32_t msaaLevel) {
		DeviceFactory::SetMSAALevel(msaaLevel);
		EXPORT_END;
	}
#pragma endregion

#pragma region ::[G/S]etVSyncEnabled()
	bool DeviceFactory::GetVSyncEnabled() {
		return vsyncEnabled;
	}
	EXPORT(DeviceFactory_GetVSyncEnabled, INTEROP_BOOL& outVsyncEnabled) {
		outVsyncEnabled = CBOOL_TO_INTEROP_BOOL(DeviceFactory::GetVSyncEnabled());
		EXPORT_END;
	}
	void DeviceFactory::SetVSyncEnabled(bool vsyncEnabled) {
		DeviceFactory::vsyncEnabled = vsyncEnabled;
	}
	EXPORT(DeviceFactory_SetVSyncEnabled, INTEROP_BOOL vsyncEnabled) {
		DeviceFactory::SetVSyncEnabled(INTEROP_BOOL_TO_CBOOL(vsyncEnabled));
		EXPORT_END;
	}
#pragma endregion

#pragma region ::ReleaseDevice()
	void DeviceFactory::ReleaseDevice(ID3D11Device* devicePtr) {
		if (dxgiFactory == nullptr) throw LosgapException { "Init device factory first." };
		if (devicePtr == nullptr) throw LosgapException { "Can not release null device pointer." };
		RELEASE_COM(devicePtr);
	}
	EXPORT(DeviceFactory_ReleaseDevice, ID3D11Device* devicePtr) {
		DeviceFactory::ReleaseDevice(devicePtr);
		EXPORT_END;
	}
#pragma endregion

#pragma region ::GetCurrent[Adapter/Output]
	IDXGIAdapter* DeviceFactory::GetCurrentAdapter() {
		return selectedAdapterPtr;
	}
	IDXGIOutput* DeviceFactory::GetCurrentOutput() {
		return selectedOutputPtr;
	}
#pragma endregion

#pragma region ::CreateSwapChain()
	IDXGISwapChain* DeviceFactory::CreateSwapChain(ID3D11Device* devicePtr, HWND windowHandle, uint32_t bufferCount) {
		if (bufferCount == 0) throw LosgapException { "Buffer count must be at least 1." };
		if (dxgiFactory == nullptr) throw LosgapException { "Init device factory first." };
		if (devicePtr == nullptr) throw LosgapException { "Device pointer must not be null." };
		if (windowHandle == nullptr) throw LosgapException { "Window handle must not be null." };

		DXGI_SWAP_CHAIN_DESC swapChainDesc;
		ZeroMemory(&swapChainDesc, sizeof(swapChainDesc));

		swapChainDesc.BufferCount = bufferCount;
		swapChainDesc.BufferDesc.Format = SUPPORTED_RTV_FORMAT;
		swapChainDesc.BufferDesc.Height = INITIAL_BACK_BUFFER_HEIGHT;
		swapChainDesc.BufferDesc.RefreshRate.Numerator = INITIAL_BACK_BUFFER_REFRESH_RATE_NUMERATOR;
		swapChainDesc.BufferDesc.RefreshRate.Denominator = INITIAL_BACK_BUFFER_REFRESH_RATE_DENOMINATOR;
		swapChainDesc.BufferDesc.Scaling = SWAP_CHAIN_SCALING_MODE;
		swapChainDesc.BufferDesc.ScanlineOrdering = SWAP_CHAIN_SCANLINE_ORDER;
		swapChainDesc.BufferDesc.Width = INITIAL_BACK_BUFFER_WIDTH;
		swapChainDesc.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;
		swapChainDesc.Flags = DXGI_SWAP_CHAIN_FLAG_ALLOW_MODE_SWITCH;
		swapChainDesc.OutputWindow = windowHandle;
		swapChainDesc.SampleDesc = GetMSAASampleDesc(devicePtr, SUPPORTED_RTV_FORMAT);
		swapChainDesc.SwapEffect = DXGI_SWAP_EFFECT_DISCARD;
		swapChainDesc.Windowed = true;

		IDXGISwapChain* outSwapChainPtr;
		CHECK_CALL(dxgiFactory->CreateSwapChain(
			devicePtr,
			&swapChainDesc,
			&outSwapChainPtr
			));

		dxgiFactory->MakeWindowAssociation(windowHandle, 0U);
		return outSwapChainPtr;
	}
#pragma endregion

#pragma region ::GetMSAASampleDesc()
	DXGI_SAMPLE_DESC DeviceFactory::GetMSAASampleDesc(ID3D11Device* devicePtr, DXGI_FORMAT format) {
		DXGI_SAMPLE_DESC result;

		UINT msaaLevelLocal = msaaLevel;
		UINT outQualLevel;		
		while (true) {
			if (msaaLevelLocal == 1) {
				result.Count = 1;
				result.Quality = 0;
				return result;
			}

			CHECK_CALL(devicePtr->CheckMultisampleQualityLevels(SUPPORTED_RTV_FORMAT, msaaLevelLocal, &outQualLevel));
			if (outQualLevel > 0) {
				result.Count = msaaLevelLocal;
				result.Quality = outQualLevel - 1;
				return result;
			}
			else msaaLevelLocal >>= 1;
		}
	}
#pragma endregion

#pragma region ::GetNativeRefreshRate()
	DXGI_RATIONAL DeviceFactory::GetNativeRefreshRate(IDXGIOutput* outputPtr, uint32_t widthPx, uint32_t heightPx) {
		DXGI_RATIONAL result { 0U, 1U };

		if (outputPtr == nullptr) {
			if (selectedOutputPtr == nullptr) {
				result.Numerator = INITIAL_BACK_BUFFER_REFRESH_RATE_NUMERATOR;
				result.Denominator = INITIAL_BACK_BUFFER_REFRESH_RATE_DENOMINATOR;
				OutputDebugString(LosgapString::AsNewCWString(
					"Obtaining native refresh rate for resolution: " + std::to_string(widthPx) + "x" + std::to_string(heightPx)
					+ ": Failed to get output.\r\n"
				).get());
				return result;
			}
			outputPtr = selectedOutputPtr;
		}

		NativeOutputResolution* nativeResolutionArr;
		uint32_t numNativeResolutions;
		for (size_t i = 0; i < adapterPtrVect.size(); ++i) {
			for (size_t j = 0; j < outputPtrVectVect[i].size(); ++j) {
				if (outputPtrVectVect[i][j] == outputPtr) {
					nativeResolutionArr = gpuDescArr[i].Outputs[j].NativeResolutions;
					numNativeResolutions = gpuDescArr[i].Outputs[j].NumNativeResolutions;
					goto outerLoopEnd;
				}
			}
		}

		result.Numerator = INITIAL_BACK_BUFFER_REFRESH_RATE_NUMERATOR;
		result.Denominator = INITIAL_BACK_BUFFER_REFRESH_RATE_DENOMINATOR;
		OutputDebugString(LosgapString::AsNewCWString(
			"Obtaining native refresh rate for resolution: " + std::to_string(widthPx) + "x" + std::to_string(heightPx)
			+ ": Failed to find gpu desc.\r\n"
		).get());
		return result;

		outerLoopEnd: 
		for (uint32_t i = 0U; i < numNativeResolutions; ++i) {
			if (nativeResolutionArr[i].ResX == widthPx && nativeResolutionArr[i].ResY == heightPx
				&& ((float_t) nativeResolutionArr[i].RefreshRateNumerator / (float_t) nativeResolutionArr[i].RefreshRateDenominator) >
					((float_t) result.Numerator / (float_t) result.Denominator)) {
				result.Numerator = nativeResolutionArr[i].RefreshRateNumerator;
				result.Denominator = nativeResolutionArr[i].RefreshRateDenominator;
			}
		}

		if (result.Numerator == 0U) {
			result.Numerator = INITIAL_BACK_BUFFER_REFRESH_RATE_NUMERATOR;
			result.Denominator = INITIAL_BACK_BUFFER_REFRESH_RATE_DENOMINATOR;

			OutputDebugString(LosgapString::AsNewCWString(
				"Obtaining native refresh rate for resolution: " + std::to_string(widthPx) + "x" + std::to_string(heightPx)
				+ ": Using default resolution.\r\n"
			).get());
		}
		else {
			Debug(LosgapString::AsNewCWString(
				"Obtained native refresh rate for resolution: " + std::to_string(widthPx) + "x" + std::to_string(heightPx)
				+ ": " + std::to_string(result.Numerator) + " / " + std::to_string(result.Denominator) + "\r\n"
			).get());
		}

		return result;
	}
#pragma endregion
}