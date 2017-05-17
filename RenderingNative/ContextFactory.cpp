// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created by Ben Bowen

#include "ContextFactory.h"

namespace losgap {
#pragma region ::GetImmediateContext()
	ID3D11DeviceContext* ContextFactory::GetImmediateContext(ID3D11Device* devicePtr) {
		if (devicePtr == nullptr) throw LosgapException { "Device pointer must not be null." };

		ID3D11DeviceContext* outDeviceContextPtr;
		devicePtr->GetImmediateContext(&outDeviceContextPtr);

		return outDeviceContextPtr;
	}
	EXPORT(ContextFactory_GetImmediateContext, ID3D11Device* devicePtr, ID3D11DeviceContext** outContextPtr) {
		*outContextPtr = ContextFactory::GetImmediateContext(devicePtr);
		EXPORT_END;
	}
#pragma endregion

#pragma region ::CreateDeferredContext()
	ID3D11DeviceContext* ContextFactory::CreateDeferredContext(ID3D11Device* devicePtr) {
		if (devicePtr == nullptr) throw LosgapException { "Device pointer must not be null." };

		ID3D11DeviceContext* outDeviceContextPtr;
		CHECK_CALL(devicePtr->CreateDeferredContext(0U, &outDeviceContextPtr));

		return outDeviceContextPtr;
	}
	EXPORT(ContextFactory_CreateDeferredContext, ID3D11Device* devicePtr, ID3D11DeviceContext** outContextPtr) {
		*outContextPtr = ContextFactory::CreateDeferredContext(devicePtr);
		EXPORT_END;
	}
#pragma endregion

#pragma region ::ExecuteDeferredCommandLists()
	void ContextFactory::ExecuteDeferredCommandLists(ID3D11DeviceContext* immediateContextPtr,
		ID3D11DeviceContext** deferredContextPtrArr, UINT numDeferredContexts) {
		if (immediateContextPtr == nullptr) throw LosgapException { "Immediate context pointer must not be null." };
		if (deferredContextPtrArr == nullptr) throw LosgapException { "Deferred context pointer array must not be null." };

		ID3D11CommandList* outCommandList;

		for (UINT i = 0U; i < numDeferredContexts; ++i) {
			if (deferredContextPtrArr[i] == nullptr) {
				throw LosgapException { "Deferred context pointer at index " + std::to_string(i) + " was null." };
			}

			CHECK_CALL(deferredContextPtrArr[i]->FinishCommandList(FALSE, &outCommandList));
			immediateContextPtr->ExecuteCommandList(outCommandList, FALSE);
		}
	}
	EXPORT(ContextFactory_ExecuteDeferredCommandLists, ID3D11DeviceContext* immediateContextPtr,
		ID3D11DeviceContext** deferredContextPtrArr, uint32_t numDeferredContexts) {
		ContextFactory::ExecuteDeferredCommandLists(immediateContextPtr, deferredContextPtrArr, numDeferredContexts);
		EXPORT_END;
	}
#pragma endregion

#pragma region ::ReleaseContext()
	void ContextFactory::ReleaseContext(ID3D11DeviceContext* contextPtr) {
		if (contextPtr == nullptr) throw LosgapException { "Context pointer must not be null." };

		RELEASE_COM(contextPtr);
	}
	EXPORT(ContextFactory_ReleaseContext, ID3D11DeviceContext* contextPtr) {
		ContextFactory::ReleaseContext(contextPtr);
		EXPORT_END;
	}
#pragma endregion
}