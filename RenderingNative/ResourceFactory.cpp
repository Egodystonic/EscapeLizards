// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created by Ben Bowen

#include "ResourceFactory.h"
#include "DeviceFactory.h"
#include "SubresourceBox.h"
#include "InitialResourceDataDesc.h"
#include "WICTextureLoader.h"

namespace losgap {
#pragma region Resource View Creation
#pragma region ::CreateRTV[ToTexArr]()
	ID3D11RenderTargetView* ResourceFactory::CreateRTV(ID3D11Device* devicePtr, ID3D11Texture2D* resPtr, uint32_t mipIndex, DXGI_FORMAT format, bool isMS) {
		if (devicePtr == nullptr) throw LosgapException { "Device pointer was null!" };
		if (resPtr == nullptr) throw LosgapException { "Resource pointer was null!" };

		ID3D11RenderTargetView* outRTV;
		D3D11_RENDER_TARGET_VIEW_DESC rtvDesc;
		ZeroMemory(&rtvDesc, sizeof(rtvDesc));
		if (isMS) {
			rtvDesc.ViewDimension = D3D11_RTV_DIMENSION_TEXTURE2DMS;
		}
		else {
			rtvDesc.ViewDimension = D3D11_RTV_DIMENSION_TEXTURE2D;
			rtvDesc.Texture2D.MipSlice = mipIndex;
		}
		rtvDesc.Format = format;

		CHECK_CALL(devicePtr->CreateRenderTargetView(resPtr, &rtvDesc, &outRTV));

		return outRTV;
	}
	EXPORT(ResourceFactory_CreateRTV, ID3D11Device* devicePtr, ID3D11Texture2D* renderTargetPtr, uint32_t mipIndex, DXGI_FORMAT format, INTEROP_BOOL isMultisampled,
		ID3D11RenderTargetView** outRTVPtr) {
		*outRTVPtr = ResourceFactory::CreateRTV(devicePtr, renderTargetPtr, mipIndex, format, INTEROP_BOOL_TO_CBOOL(isMultisampled));
		EXPORT_END;
	}

	ID3D11RenderTargetView* ResourceFactory::CreateRTVToTexArr(ID3D11Device* devicePtr, ID3D11Texture2D* resPtr, uint32_t mipIndex, uint32_t arrIndex, DXGI_FORMAT format, bool isMS) {
		if (devicePtr == nullptr) throw LosgapException { "Device pointer was null!" };
		if (resPtr == nullptr) throw LosgapException { "Resource pointer was null!" };

		ID3D11RenderTargetView* outRTV;
		D3D11_RENDER_TARGET_VIEW_DESC rtvDesc;
		ZeroMemory(&rtvDesc, sizeof(rtvDesc));
		if (isMS) {
			rtvDesc.ViewDimension = D3D11_RTV_DIMENSION_TEXTURE2DMSARRAY;
			rtvDesc.Texture2DMSArray.FirstArraySlice = mipIndex;
			rtvDesc.Texture2DMSArray.ArraySize = 1U;
		}
		else {
			rtvDesc.ViewDimension = D3D11_RTV_DIMENSION_TEXTURE2DARRAY;
			rtvDesc.Texture2DArray.MipSlice = mipIndex;
			rtvDesc.Texture2DArray.FirstArraySlice = mipIndex;
			rtvDesc.Texture2DArray.ArraySize = 1U;
		}
		rtvDesc.Format = format;
		
		CHECK_CALL(devicePtr->CreateRenderTargetView(resPtr, &rtvDesc, &outRTV));

		return outRTV;
	}
	EXPORT(ResourceFactory_CreateRTVToTexArr, ID3D11Device* devicePtr, ID3D11Texture2D* renderTargetPtr, uint32_t mipIndex, uint32_t arrIndex, DXGI_FORMAT format, 
		INTEROP_BOOL isMultisampled, ID3D11RenderTargetView** outRTVPtr) {
		*outRTVPtr = ResourceFactory::CreateRTVToTexArr(devicePtr, renderTargetPtr, mipIndex, arrIndex, format, INTEROP_BOOL_TO_CBOOL(isMultisampled));
		EXPORT_END;
	}
#pragma endregion

#pragma region ::CreateDSV[ToTexArr]()
	ID3D11DepthStencilView* ResourceFactory::CreateDSV(ID3D11Device* devicePtr, ID3D11Texture2D* resPtr, uint32_t mipIndex, DXGI_FORMAT format, bool isMS) {
		if (devicePtr == nullptr) throw LosgapException { "Device pointer was null!" };
		if (resPtr == nullptr) throw LosgapException { "Resource pointer was null!" };

		ID3D11DepthStencilView* outDSV;
		D3D11_DEPTH_STENCIL_VIEW_DESC dsvDesc;
		ZeroMemory(&dsvDesc, sizeof(dsvDesc));
		if (isMS) {
			dsvDesc.ViewDimension = D3D11_DSV_DIMENSION_TEXTURE2DMS;
		}
		else {
			dsvDesc.ViewDimension = D3D11_DSV_DIMENSION_TEXTURE2D;
			dsvDesc.Texture2D.MipSlice = mipIndex;
		}
		dsvDesc.Format = format;

		CHECK_CALL(devicePtr->CreateDepthStencilView(resPtr, &dsvDesc, &outDSV));

		return outDSV;
	}
	EXPORT(ResourceFactory_CreateDSV, ID3D11Device* devicePtr, ID3D11Texture2D* renderTargetPtr, uint32_t mipIndex, DXGI_FORMAT format, INTEROP_BOOL isMultisampled,
		ID3D11DepthStencilView** outDSVPtr) {
		*outDSVPtr = ResourceFactory::CreateDSV(devicePtr, renderTargetPtr, mipIndex, format, INTEROP_BOOL_TO_CBOOL(isMultisampled));
		EXPORT_END;
	}

	ID3D11DepthStencilView* ResourceFactory::CreateDSVToTexArr(ID3D11Device* devicePtr, ID3D11Texture2D* resPtr, uint32_t mipIndex, uint32_t arrIndex, DXGI_FORMAT format, bool isMS) {
		if (devicePtr == nullptr) throw LosgapException { "Device pointer was null!" };
		if (resPtr == nullptr) throw LosgapException { "Resource pointer was null!" };

		ID3D11DepthStencilView* outDSV;
		D3D11_DEPTH_STENCIL_VIEW_DESC dsvDesc;
		ZeroMemory(&dsvDesc, sizeof(dsvDesc));
		if (isMS) {
			dsvDesc.ViewDimension = D3D11_DSV_DIMENSION_TEXTURE2DMSARRAY;
			dsvDesc.Texture2DMSArray.FirstArraySlice = arrIndex;
			dsvDesc.Texture2DMSArray.ArraySize = 1U;
		}
		else {
			dsvDesc.ViewDimension = D3D11_DSV_DIMENSION_TEXTURE2DARRAY;
			dsvDesc.Texture2DArray.MipSlice = mipIndex;
			dsvDesc.Texture2DArray.FirstArraySlice = arrIndex;
			dsvDesc.Texture2DArray.ArraySize = 1U;
		}
		dsvDesc.Format = format;

		CHECK_CALL(devicePtr->CreateDepthStencilView(resPtr, &dsvDesc, &outDSV));

		return outDSV;
	}
	EXPORT(ResourceFactory_CreateDSVToTexArr, ID3D11Device* devicePtr, ID3D11Texture2D* renderTargetPtr, uint32_t mipIndex, uint32_t arrIndex, DXGI_FORMAT format, 
		INTEROP_BOOL isMultisampled, ID3D11DepthStencilView** outDSVPtr) {
		*outDSVPtr = ResourceFactory::CreateDSVToTexArr(devicePtr, renderTargetPtr, mipIndex, arrIndex, format, INTEROP_BOOL_TO_CBOOL(isMultisampled));
		EXPORT_END;
	}
#pragma endregion

#pragma region ::CreateSRVTo[...]Buffer()
	ID3D11ShaderResourceView* ResourceFactory::CreateSRVToBuffer(ID3D11Device* devicePtr, ID3D11Buffer* resPtr,
		DXGI_FORMAT format,	UINT firstElement, UINT numElements) {
		if (devicePtr == nullptr) throw LosgapException { "Device pointer was null!" };
		if (resPtr == nullptr) throw LosgapException { "Resource pointer was null!" };

		D3D11_SHADER_RESOURCE_VIEW_DESC srvDesc;
		ZeroMemory(&srvDesc, sizeof(srvDesc));

		srvDesc.Format = format;
		srvDesc.ViewDimension = D3D11_SRV_DIMENSION::D3D11_SRV_DIMENSION_BUFFER;
		srvDesc.Buffer.FirstElement = firstElement;
		srvDesc.Buffer.NumElements = numElements;

		ID3D11ShaderResourceView* outSRV;

		CHECK_CALL(devicePtr->CreateShaderResourceView(resPtr, &srvDesc, &outSRV));

		return outSRV;
	}
	EXPORT(ResourceFactory_CreateSRVToBuffer, ID3D11Device* devicePtr, ID3D11Buffer* resPtr, 
		int32_t format, uint32_t firstElement, uint32_t numElements, ID3D11ShaderResourceView** outSRVPtr) {
		*outSRVPtr = ResourceFactory::CreateSRVToBuffer(devicePtr, resPtr, static_cast<DXGI_FORMAT>(format), firstElement, numElements);
		EXPORT_END;
	}
	ID3D11ShaderResourceView* ResourceFactory::CreateSRVToRawBuffer(ID3D11Device* devicePtr, ID3D11Buffer* resPtr,
		UINT firstElement, UINT numElements) {
		if (devicePtr == nullptr) throw LosgapException { "Device pointer was null!" };
		if (resPtr == nullptr) throw LosgapException { "Resource pointer was null!" };

		D3D11_SHADER_RESOURCE_VIEW_DESC srvDesc;
		ZeroMemory(&srvDesc, sizeof(srvDesc));

		srvDesc.Format = DXGI_FORMAT_UNKNOWN;
		srvDesc.ViewDimension = D3D11_SRV_DIMENSION::D3D11_SRV_DIMENSION_BUFFEREX;
		srvDesc.BufferEx.FirstElement = firstElement;
		srvDesc.BufferEx.NumElements = numElements;
		srvDesc.BufferEx.Flags = D3D11_BUFFEREX_SRV_FLAG::D3D11_BUFFEREX_SRV_FLAG_RAW;

		ID3D11ShaderResourceView* outSRV;

		CHECK_CALL(devicePtr->CreateShaderResourceView(resPtr, &srvDesc, &outSRV));

		return outSRV;
	}
	EXPORT(ResourceFactory_CreateSRVToRawBuffer, ID3D11Device* devicePtr, ID3D11Buffer* resPtr,
		uint32_t firstElement, uint32_t numElements, ID3D11ShaderResourceView** outSRVPtr) {
		*outSRVPtr = ResourceFactory::CreateSRVToRawBuffer(devicePtr, resPtr, firstElement, numElements);
		EXPORT_END;
	}
#pragma endregion
#pragma region ::CreateSRVToTexture1D[...]()
	ID3D11ShaderResourceView* ResourceFactory::CreateSRVToTexture1D(ID3D11Device* devicePtr, ID3D11Texture1D* resPtr,
		DXGI_FORMAT format, UINT firstMipIndex, UINT numMips) {
		if (devicePtr == nullptr) throw LosgapException { "Device pointer was null!" };
		if (resPtr == nullptr) throw LosgapException { "Resource pointer was null!" };

		D3D11_SHADER_RESOURCE_VIEW_DESC srvDesc;
		ZeroMemory(&srvDesc, sizeof(srvDesc));

		srvDesc.Format = format;
		srvDesc.ViewDimension = D3D11_SRV_DIMENSION::D3D11_SRV_DIMENSION_TEXTURE1D;
		srvDesc.Texture1D.MipLevels = numMips;
		srvDesc.Texture1D.MostDetailedMip = firstMipIndex;

		ID3D11ShaderResourceView* outSRV;

		CHECK_CALL(devicePtr->CreateShaderResourceView(resPtr, &srvDesc, &outSRV));

		return outSRV;
	}
	EXPORT(ResourceFactory_CreateSRVToTexture1D, ID3D11Device* devicePtr, ID3D11Texture1D* resPtr,
		int32_t format, uint32_t firstMipIndex, uint32_t numMips, ID3D11ShaderResourceView** outSRVPtr) {
		*outSRVPtr = ResourceFactory::CreateSRVToTexture1D(devicePtr, resPtr, static_cast<DXGI_FORMAT>(format), firstMipIndex, numMips);
		EXPORT_END;
	}
	ID3D11ShaderResourceView* ResourceFactory::CreateSRVToTexture1DArray(ID3D11Device* devicePtr, ID3D11Texture1D* resPtr,
		DXGI_FORMAT format, UINT firstMipIndex, UINT numMips, UINT firstElementIndex, UINT numElements) {
		if (devicePtr == nullptr) throw LosgapException { "Device pointer was null!" };
		if (resPtr == nullptr) throw LosgapException { "Resource pointer was null!" };

		D3D11_SHADER_RESOURCE_VIEW_DESC srvDesc;
		ZeroMemory(&srvDesc, sizeof(srvDesc));

		srvDesc.Format = format;
		srvDesc.ViewDimension = D3D11_SRV_DIMENSION::D3D11_SRV_DIMENSION_TEXTURE1DARRAY;
		srvDesc.Texture1DArray.ArraySize = numElements;
		srvDesc.Texture1DArray.FirstArraySlice = firstElementIndex;
		srvDesc.Texture1DArray.MipLevels = numMips;
		srvDesc.Texture1DArray.MostDetailedMip = firstMipIndex;

		ID3D11ShaderResourceView* outSRV;

		CHECK_CALL(devicePtr->CreateShaderResourceView(resPtr, &srvDesc, &outSRV));

		return outSRV;
	}
	EXPORT(ResourceFactory_CreateSRVToTexture1DArray, ID3D11Device* devicePtr, ID3D11Texture1D* resPtr,
		int32_t format, uint32_t firstMipIndex, uint32_t numMips, uint32_t firstElementIndex, uint32_t numElements, 
		ID3D11ShaderResourceView** outSRVPtr) {
		*outSRVPtr = ResourceFactory::CreateSRVToTexture1DArray(
			devicePtr, resPtr, static_cast<DXGI_FORMAT>(format), firstMipIndex, numMips, firstElementIndex, numElements
		);
		EXPORT_END;
	}
#pragma endregion
#pragma region ::CreateSRVToTexture2D[...]()
	ID3D11ShaderResourceView* ResourceFactory::CreateSRVToTexture2D(ID3D11Device* devicePtr, ID3D11Texture2D* resPtr,
		DXGI_FORMAT format, UINT firstMipIndex, UINT numMips) {
		if (devicePtr == nullptr) throw LosgapException { "Device pointer was null!" };
		if (resPtr == nullptr) throw LosgapException { "Resource pointer was null!" };

		D3D11_SHADER_RESOURCE_VIEW_DESC srvDesc;
		ZeroMemory(&srvDesc, sizeof(srvDesc));

		srvDesc.Format = format;
		srvDesc.ViewDimension = D3D11_SRV_DIMENSION::D3D11_SRV_DIMENSION_TEXTURE2D;
		srvDesc.Texture2D.MipLevels = numMips;
		srvDesc.Texture2D.MostDetailedMip = firstMipIndex;

		ID3D11ShaderResourceView* outSRV;

		CHECK_CALL(devicePtr->CreateShaderResourceView(resPtr, &srvDesc, &outSRV));

		return outSRV;
	}
	EXPORT(ResourceFactory_CreateSRVToTexture2D, ID3D11Device* devicePtr, ID3D11Texture2D* resPtr,
		int32_t format, uint32_t firstMipIndex, uint32_t numMips, ID3D11ShaderResourceView** outSRVPtr) {
		*outSRVPtr = ResourceFactory::CreateSRVToTexture2D(devicePtr, resPtr, static_cast<DXGI_FORMAT>(format), firstMipIndex, numMips);
		EXPORT_END;
	}
	ID3D11ShaderResourceView* ResourceFactory::CreateSRVToTexture2DArray(ID3D11Device* devicePtr, ID3D11Texture2D* resPtr,
		DXGI_FORMAT format, UINT firstMipIndex, UINT numMips, UINT firstElementIndex, UINT numElements) {
		if (devicePtr == nullptr) throw LosgapException { "Device pointer was null!" };
		if (resPtr == nullptr) throw LosgapException { "Resource pointer was null!" };

		D3D11_SHADER_RESOURCE_VIEW_DESC srvDesc;
		ZeroMemory(&srvDesc, sizeof(srvDesc));

		srvDesc.Format = format;
		srvDesc.ViewDimension = D3D11_SRV_DIMENSION::D3D11_SRV_DIMENSION_TEXTURE2DARRAY;
		srvDesc.Texture2DArray.ArraySize = numElements;
		srvDesc.Texture2DArray.FirstArraySlice = firstElementIndex;
		srvDesc.Texture2DArray.MipLevels = numMips;
		srvDesc.Texture2DArray.MostDetailedMip = firstMipIndex;

		ID3D11ShaderResourceView* outSRV;

		CHECK_CALL(devicePtr->CreateShaderResourceView(resPtr, &srvDesc, &outSRV));

		return outSRV;
	}
	EXPORT(ResourceFactory_CreateSRVToTexture2DArray, ID3D11Device* devicePtr, ID3D11Texture2D* resPtr,
		int32_t format, uint32_t firstMipIndex, uint32_t numMips, uint32_t firstElementIndex, uint32_t numElements,
		ID3D11ShaderResourceView** outSRVPtr) {
		*outSRVPtr = ResourceFactory::CreateSRVToTexture2DArray(
			devicePtr, resPtr, static_cast<DXGI_FORMAT>(format), firstMipIndex, numMips, firstElementIndex, numElements
			);
		EXPORT_END;
	}

	ID3D11ShaderResourceView* ResourceFactory::CreateSRVToTexture2DMS(ID3D11Device* devicePtr, ID3D11Texture2D* resPtr,
		DXGI_FORMAT format) {
		if (devicePtr == nullptr) throw LosgapException { "Device pointer was null!" };
		if (resPtr == nullptr) throw LosgapException { "Resource pointer was null!" };

		D3D11_SHADER_RESOURCE_VIEW_DESC srvDesc;
		ZeroMemory(&srvDesc, sizeof(srvDesc));

		srvDesc.Format = format;
		srvDesc.ViewDimension = D3D11_SRV_DIMENSION::D3D11_SRV_DIMENSION_TEXTURE2DMS;

		ID3D11ShaderResourceView* outSRV;

		CHECK_CALL(devicePtr->CreateShaderResourceView(resPtr, &srvDesc, &outSRV));

		return outSRV;
	}
	EXPORT(ResourceFactory_CreateSRVToTexture2DMS, ID3D11Device* devicePtr, ID3D11Texture2D* resPtr,
		int32_t format, ID3D11ShaderResourceView** outSRVPtr) {
		*outSRVPtr = ResourceFactory::CreateSRVToTexture2DMS(devicePtr, resPtr, static_cast<DXGI_FORMAT>(format));
		EXPORT_END;
	}
	ID3D11ShaderResourceView* ResourceFactory::CreateSRVToTexture2DMSArray(ID3D11Device* devicePtr, ID3D11Texture2D* resPtr,
		DXGI_FORMAT format, UINT firstElementIndex, UINT numElements) {
		if (devicePtr == nullptr) throw LosgapException { "Device pointer was null!" };
		if (resPtr == nullptr) throw LosgapException { "Resource pointer was null!" };

		D3D11_SHADER_RESOURCE_VIEW_DESC srvDesc;
		ZeroMemory(&srvDesc, sizeof(srvDesc));

		srvDesc.Format = format;
		srvDesc.ViewDimension = D3D11_SRV_DIMENSION::D3D11_SRV_DIMENSION_TEXTURE2DMSARRAY;
		srvDesc.Texture2DMSArray.ArraySize = numElements;
		srvDesc.Texture2DMSArray.FirstArraySlice = firstElementIndex;

		ID3D11ShaderResourceView* outSRV;

		CHECK_CALL(devicePtr->CreateShaderResourceView(resPtr, &srvDesc, &outSRV));

		return outSRV;
	}
	EXPORT(ResourceFactory_CreateSRVToTexture2DMSArray, ID3D11Device* devicePtr, ID3D11Texture2D* resPtr,
		int32_t format, uint32_t firstElementIndex, uint32_t numElements,
		ID3D11ShaderResourceView** outSRVPtr) {
		*outSRVPtr = ResourceFactory::CreateSRVToTexture2DMSArray(
			devicePtr, resPtr, static_cast<DXGI_FORMAT>(format), firstElementIndex, numElements
			);
		EXPORT_END;
	}

	ID3D11ShaderResourceView* ResourceFactory::CreateSRVToTexture2DCube(ID3D11Device* devicePtr, ID3D11Texture2D* resPtr,
		DXGI_FORMAT format, UINT firstMipIndex, UINT numMips) {
		if (devicePtr == nullptr) throw LosgapException { "Device pointer was null!" };
		if (resPtr == nullptr) throw LosgapException { "Resource pointer was null!" };

		D3D11_SHADER_RESOURCE_VIEW_DESC srvDesc;
		ZeroMemory(&srvDesc, sizeof(srvDesc));

		srvDesc.Format = format;
		srvDesc.ViewDimension = D3D11_SRV_DIMENSION::D3D11_SRV_DIMENSION_TEXTURECUBE;
		srvDesc.TextureCube.MipLevels = numMips;
		srvDesc.TextureCube.MostDetailedMip = firstMipIndex;

		ID3D11ShaderResourceView* outSRV;

		CHECK_CALL(devicePtr->CreateShaderResourceView(resPtr, &srvDesc, &outSRV));

		return outSRV;
	}
	EXPORT(ResourceFactory_CreateSRVToTexture2DCube, ID3D11Device* devicePtr, ID3D11Texture2D* resPtr,
		int32_t format, uint32_t firstMipIndex, uint32_t numMips, ID3D11ShaderResourceView** outSRVPtr) {
		*outSRVPtr = ResourceFactory::CreateSRVToTexture2DCube(devicePtr, resPtr, static_cast<DXGI_FORMAT>(format), firstMipIndex, numMips);
		EXPORT_END;
	}
	ID3D11ShaderResourceView* ResourceFactory::CreateSRVToTexture2DCubeArray(ID3D11Device* devicePtr, ID3D11Texture2D* resPtr,
		DXGI_FORMAT format, UINT firstMipIndex, UINT numMips, UINT firstFaceIndex, UINT numCubes) {
		if (devicePtr == nullptr) throw LosgapException { "Device pointer was null!" };
		if (resPtr == nullptr) throw LosgapException { "Resource pointer was null!" };

		D3D11_SHADER_RESOURCE_VIEW_DESC srvDesc;
		ZeroMemory(&srvDesc, sizeof(srvDesc));

		srvDesc.Format = format;
		srvDesc.ViewDimension = D3D11_SRV_DIMENSION::D3D11_SRV_DIMENSION_TEXTURECUBEARRAY;
		srvDesc.TextureCubeArray.First2DArrayFace = firstFaceIndex;
		srvDesc.TextureCubeArray.NumCubes = numCubes;
		srvDesc.TextureCubeArray.MipLevels = numMips;
		srvDesc.TextureCubeArray.MostDetailedMip = firstMipIndex;

		ID3D11ShaderResourceView* outSRV;

		CHECK_CALL(devicePtr->CreateShaderResourceView(resPtr, &srvDesc, &outSRV));

		return outSRV;
	}
	EXPORT(ResourceFactory_CreateSRVToTexture2DCubeArray, ID3D11Device* devicePtr, ID3D11Texture2D* resPtr,
		int32_t format, uint32_t firstMipIndex, uint32_t numMips, uint32_t firstElementIndex, uint32_t numElements,
		ID3D11ShaderResourceView** outSRVPtr) {
		*outSRVPtr = ResourceFactory::CreateSRVToTexture2DCubeArray(
			devicePtr, resPtr, static_cast<DXGI_FORMAT>(format), firstMipIndex, numMips, firstElementIndex, numElements
			);
		EXPORT_END;
	}
#pragma endregion
#pragma region ::CreateSRVToTexture3D()
	ID3D11ShaderResourceView* ResourceFactory::CreateSRVToTexture3D(ID3D11Device* devicePtr, ID3D11Texture3D* resPtr,
		DXGI_FORMAT format, UINT firstMipIndex, UINT numMips) {
		if (devicePtr == nullptr) throw LosgapException { "Device pointer was null!" };
		if (resPtr == nullptr) throw LosgapException { "Resource pointer was null!" };

		D3D11_SHADER_RESOURCE_VIEW_DESC srvDesc;
		ZeroMemory(&srvDesc, sizeof(srvDesc));

		srvDesc.Format = format;
		srvDesc.ViewDimension = D3D11_SRV_DIMENSION::D3D11_SRV_DIMENSION_TEXTURE3D;
		srvDesc.Texture3D.MipLevels = numMips;
		srvDesc.Texture3D.MostDetailedMip = firstMipIndex;

		ID3D11ShaderResourceView* outSRV;

		CHECK_CALL(devicePtr->CreateShaderResourceView(resPtr, &srvDesc, &outSRV));

		return outSRV;
	}
	EXPORT(ResourceFactory_CreateSRVToTexture3D, ID3D11Device* devicePtr, ID3D11Texture3D* resPtr,
		int32_t format, uint32_t firstMipIndex, uint32_t numMips, ID3D11ShaderResourceView** outSRVPtr) {
		*outSRVPtr = ResourceFactory::CreateSRVToTexture3D(devicePtr, resPtr, static_cast<DXGI_FORMAT>(format), firstMipIndex, numMips);
		EXPORT_END;
	}
#pragma endregion

#pragma region ::CreateUAVTo[...]Buffer()
	ID3D11UnorderedAccessView* ResourceFactory::CreateUAVToBuffer(ID3D11Device* devicePtr, ID3D11Buffer* resPtr,
		DXGI_FORMAT format,	UINT firstElement, UINT numElements, bool allowAppendConsume, bool includeCounter) {
		if (devicePtr == nullptr) throw LosgapException { "Device pointer was null!" };
		if (resPtr == nullptr) throw LosgapException { "Resource pointer was null!" };

		D3D11_UNORDERED_ACCESS_VIEW_DESC uavDesc;
		ZeroMemory(&uavDesc, sizeof(uavDesc));

		uavDesc.Format = format;
		uavDesc.ViewDimension = D3D11_UAV_DIMENSION::D3D11_UAV_DIMENSION_BUFFER;
		uavDesc.Buffer.FirstElement = firstElement;
		uavDesc.Buffer.NumElements = numElements;
		uavDesc.Buffer.Flags =
			(allowAppendConsume ? D3D11_BUFFER_UAV_FLAG::D3D11_BUFFER_UAV_FLAG_APPEND : 0U)
			| (includeCounter ? D3D11_BUFFER_UAV_FLAG::D3D11_BUFFER_UAV_FLAG_COUNTER : 0U);

		ID3D11UnorderedAccessView* outUAV;

		CHECK_CALL(devicePtr->CreateUnorderedAccessView(resPtr, &uavDesc, &outUAV));

		return outUAV;
	}
	EXPORT(ResourceFactory_CreateUAVToBuffer, ID3D11Device* devicePtr, ID3D11Buffer* resPtr,
		int32_t format, uint32_t firstElement, uint32_t numElements, INTEROP_BOOL allowAppendConsume, INTEROP_BOOL includeCounter,
		ID3D11UnorderedAccessView** outUAVPtr) {
		*outUAVPtr = ResourceFactory::CreateUAVToBuffer(
			devicePtr, resPtr, static_cast<DXGI_FORMAT>(format), 
			firstElement, numElements, INTEROP_BOOL_TO_CBOOL(allowAppendConsume), INTEROP_BOOL_TO_CBOOL(includeCounter)
		);
		EXPORT_END;
	}
	ID3D11UnorderedAccessView* ResourceFactory::CreateUAVToRawBuffer(ID3D11Device* devicePtr, ID3D11Buffer* resPtr,
		UINT firstElement, UINT numElements, bool allowAppendConsume, bool includeCounter) {
		if (devicePtr == nullptr) throw LosgapException { "Device pointer was null!" };
		if (resPtr == nullptr) throw LosgapException { "Resource pointer was null!" };

		D3D11_UNORDERED_ACCESS_VIEW_DESC uavDesc;
		ZeroMemory(&uavDesc, sizeof(uavDesc));

		uavDesc.Format = DXGI_FORMAT_R32_TYPELESS;
		uavDesc.ViewDimension = D3D11_UAV_DIMENSION::D3D11_UAV_DIMENSION_BUFFER;
		uavDesc.Buffer.FirstElement = firstElement;
		uavDesc.Buffer.NumElements = numElements;
		uavDesc.Buffer.Flags =
			D3D11_BUFFER_UAV_FLAG::D3D11_BUFFER_UAV_FLAG_RAW
			| (allowAppendConsume ? D3D11_BUFFER_UAV_FLAG::D3D11_BUFFER_UAV_FLAG_APPEND : 0U)
			| (includeCounter ? D3D11_BUFFER_UAV_FLAG::D3D11_BUFFER_UAV_FLAG_COUNTER : 0U);

		ID3D11UnorderedAccessView* outUAV;

		CHECK_CALL(devicePtr->CreateUnorderedAccessView(resPtr, &uavDesc, &outUAV));

		return outUAV;
	}
	EXPORT(ResourceFactory_CreateUAVToRawBuffer, ID3D11Device* devicePtr, ID3D11Buffer* resPtr,
		uint32_t firstElement, uint32_t numElements, INTEROP_BOOL allowAppendConsume, INTEROP_BOOL includeCounter,
		ID3D11UnorderedAccessView** outUAVPtr) {
		*outUAVPtr = ResourceFactory::CreateUAVToRawBuffer(
			devicePtr, resPtr,
			firstElement, numElements, INTEROP_BOOL_TO_CBOOL(allowAppendConsume), INTEROP_BOOL_TO_CBOOL(includeCounter)
			);
		EXPORT_END;
	}
#pragma endregion
#pragma region ::CreateUAVToTexture1D[...]()
	ID3D11UnorderedAccessView* ResourceFactory::CreateUAVToTexture1D(ID3D11Device* devicePtr, ID3D11Texture1D* resPtr,
		DXGI_FORMAT format, UINT mipIndex) {
		if (devicePtr == nullptr) throw LosgapException { "Device pointer was null!" };
		if (resPtr == nullptr) throw LosgapException { "Resource pointer was null!" };

		D3D11_UNORDERED_ACCESS_VIEW_DESC uavDesc;
		ZeroMemory(&uavDesc, sizeof(uavDesc));

		uavDesc.Format = format;
		uavDesc.ViewDimension = D3D11_UAV_DIMENSION::D3D11_UAV_DIMENSION_TEXTURE1D;
		uavDesc.Texture1D.MipSlice = mipIndex;

		ID3D11UnorderedAccessView* outUAV;

		CHECK_CALL(devicePtr->CreateUnorderedAccessView(resPtr, &uavDesc, &outUAV));

		return outUAV;
	}
	EXPORT(ResourceFactory_CreateUAVToTexture1D, ID3D11Device* devicePtr, ID3D11Texture1D* resPtr,
		int32_t format, uint32_t firstMipIndex, ID3D11UnorderedAccessView** outUAVPtr) {
		*outUAVPtr = ResourceFactory::CreateUAVToTexture1D(devicePtr, resPtr, static_cast<DXGI_FORMAT>(format), firstMipIndex);
		EXPORT_END;
	}
	ID3D11UnorderedAccessView* ResourceFactory::CreateUAVToTexture1DArray(ID3D11Device* devicePtr, ID3D11Texture1D* resPtr,
		DXGI_FORMAT format, UINT mipIndex, UINT firstElementIndex, UINT numElements) {
		if (devicePtr == nullptr) throw LosgapException { "Device pointer was null!" };
		if (resPtr == nullptr) throw LosgapException { "Resource pointer was null!" };

		D3D11_UNORDERED_ACCESS_VIEW_DESC uavDesc;
		ZeroMemory(&uavDesc, sizeof(uavDesc));

		uavDesc.Format = format;
		uavDesc.ViewDimension = D3D11_UAV_DIMENSION::D3D11_UAV_DIMENSION_TEXTURE1DARRAY;
		uavDesc.Texture1DArray.MipSlice = mipIndex;
		uavDesc.Texture1DArray.FirstArraySlice = firstElementIndex;
		uavDesc.Texture1DArray.ArraySize = numElements;

		ID3D11UnorderedAccessView* outUAV;

		CHECK_CALL(devicePtr->CreateUnorderedAccessView(resPtr, &uavDesc, &outUAV));

		return outUAV;
	}
	EXPORT(ResourceFactory_CreateUAVToTexture1DArray, ID3D11Device* devicePtr, ID3D11Texture1D* resPtr,
		int32_t format, uint32_t firstMipIndex, uint32_t firstElementIndex, uint32_t numElements, ID3D11UnorderedAccessView** outUAVPtr) {
		*outUAVPtr = ResourceFactory::CreateUAVToTexture1DArray(
			devicePtr, resPtr, static_cast<DXGI_FORMAT>(format), firstMipIndex, firstElementIndex, numElements
		);
		EXPORT_END;
	}
#pragma endregion
#pragma region ::CreateUAVToTexture2D[...]()
	ID3D11UnorderedAccessView* ResourceFactory::CreateUAVToTexture2D(ID3D11Device* devicePtr, ID3D11Texture2D* resPtr,
		DXGI_FORMAT format, UINT mipIndex) {
		if (devicePtr == nullptr) throw LosgapException { "Device pointer was null!" };
		if (resPtr == nullptr) throw LosgapException { "Resource pointer was null!" };

		D3D11_UNORDERED_ACCESS_VIEW_DESC uavDesc;
		ZeroMemory(&uavDesc, sizeof(uavDesc));

		uavDesc.Format = format;
		uavDesc.ViewDimension = D3D11_UAV_DIMENSION::D3D11_UAV_DIMENSION_TEXTURE2D;
		uavDesc.Texture2D.MipSlice = mipIndex;

		ID3D11UnorderedAccessView* outUAV;

		CHECK_CALL(devicePtr->CreateUnorderedAccessView(resPtr, &uavDesc, &outUAV));

		return outUAV;
	}
	EXPORT(ResourceFactory_CreateUAVToTexture2D, ID3D11Device* devicePtr, ID3D11Texture2D* resPtr,
		int32_t format, uint32_t firstMipIndex, ID3D11UnorderedAccessView** outUAVPtr) {
		*outUAVPtr = ResourceFactory::CreateUAVToTexture2D(devicePtr, resPtr, static_cast<DXGI_FORMAT>(format), firstMipIndex);
		EXPORT_END;
	}
	ID3D11UnorderedAccessView* ResourceFactory::CreateUAVToTexture2DArray(ID3D11Device* devicePtr, ID3D11Texture2D* resPtr,
		DXGI_FORMAT format, UINT mipIndex, UINT firstElementIndex, UINT numElements) {
		if (devicePtr == nullptr) throw LosgapException { "Device pointer was null!" };
		if (resPtr == nullptr) throw LosgapException { "Resource pointer was null!" };

		D3D11_UNORDERED_ACCESS_VIEW_DESC uavDesc;
		ZeroMemory(&uavDesc, sizeof(uavDesc));

		uavDesc.Format = format;
		uavDesc.ViewDimension = D3D11_UAV_DIMENSION::D3D11_UAV_DIMENSION_TEXTURE2DARRAY;
		uavDesc.Texture2DArray.MipSlice = mipIndex;
		uavDesc.Texture2DArray.FirstArraySlice = firstElementIndex;
		uavDesc.Texture2DArray.ArraySize = numElements;

		ID3D11UnorderedAccessView* outUAV;

		CHECK_CALL(devicePtr->CreateUnorderedAccessView(resPtr, &uavDesc, &outUAV));

		return outUAV;
	}
	EXPORT(ResourceFactory_CreateUAVToTexture2DArray, ID3D11Device* devicePtr, ID3D11Texture2D* resPtr,
		int32_t format, uint32_t firstMipIndex, uint32_t firstElementIndex, uint32_t numElements, ID3D11UnorderedAccessView** outUAVPtr) {
		*outUAVPtr = ResourceFactory::CreateUAVToTexture2DArray(
			devicePtr, resPtr, static_cast<DXGI_FORMAT>(format), firstMipIndex, firstElementIndex, numElements
			);
		EXPORT_END;
	}
#pragma endregion
#pragma region ::CreateUAVToTexture3D()
	ID3D11UnorderedAccessView* ResourceFactory::CreateUAVToTexture3D(ID3D11Device* devicePtr, ID3D11Texture3D* resPtr,
		DXGI_FORMAT format, UINT mipIndex, UINT firstWCoordOffset, UINT wCoordRange) {
		if (devicePtr == nullptr) throw LosgapException { "Device pointer was null!" };
		if (resPtr == nullptr) throw LosgapException { "Resource pointer was null!" };
		if (wCoordRange == 0U) throw LosgapException { "W-Coord range must be positive!" };

		D3D11_UNORDERED_ACCESS_VIEW_DESC uavDesc;
		ZeroMemory(&uavDesc, sizeof(uavDesc));

		uavDesc.Format = format;
		uavDesc.ViewDimension = D3D11_UAV_DIMENSION::D3D11_UAV_DIMENSION_TEXTURE3D;
		uavDesc.Texture3D.MipSlice = mipIndex;
		uavDesc.Texture3D.FirstWSlice = firstWCoordOffset;
		uavDesc.Texture3D.WSize = wCoordRange;

		ID3D11UnorderedAccessView* outUAV;

		CHECK_CALL(devicePtr->CreateUnorderedAccessView(resPtr, &uavDesc, &outUAV));

		return outUAV;
	}
	EXPORT(ResourceFactory_CreateUAVToTexture3D, ID3D11Device* devicePtr, ID3D11Texture3D* resPtr,
		int32_t format, uint32_t firstMipIndex, uint32_t firstWCoordOffset, uint32_t wCoordRange, ID3D11UnorderedAccessView** outUAVPtr) {
		*outUAVPtr = ResourceFactory::CreateUAVToTexture3D(devicePtr, resPtr, static_cast<DXGI_FORMAT>(format), 
			firstMipIndex, firstWCoordOffset, wCoordRange);
		EXPORT_END;
	}
#pragma endregion
#pragma endregion

#pragma region Resource Creation
#pragma region ::Create[...]Buffer()
	ID3D11Buffer* ResourceFactory::CreateVertexBuffer(ID3D11Device* devicePtr, UINT vertexStructSizeBytes, UINT numVertices,
		D3D11_USAGE usage, D3D11_CPU_ACCESS_FLAG cpuUsage, D3D11_SUBRESOURCE_DATA* initialDataPtr) {
		if (devicePtr == nullptr) throw LosgapException { "Device pointer was null!" };
		if (initialDataPtr == nullptr && usage == D3D11_USAGE::D3D11_USAGE_IMMUTABLE) {
			throw LosgapException { "Initial data must not be null when creating an immutable resource." };
		}
		if (vertexStructSizeBytes == 0U) throw LosgapException { "Vertex struct size must be greater than zero." };
		if (numVertices == 0U) throw LosgapException { "Vertex buffer must have at least one vertex." };

		D3D11_BUFFER_DESC bufferDesc;
		ZeroMemory(&bufferDesc, sizeof(bufferDesc));

		bufferDesc.BindFlags = D3D11_BIND_FLAG::D3D11_BIND_VERTEX_BUFFER;
		bufferDesc.ByteWidth = vertexStructSizeBytes * numVertices;
		bufferDesc.CPUAccessFlags = cpuUsage;
		bufferDesc.Usage = usage;
		bufferDesc.StructureByteStride = vertexStructSizeBytes;

		ID3D11Buffer* outBuffer;
		CHECK_CALL(devicePtr->CreateBuffer(&bufferDesc, initialDataPtr, &outBuffer));

		return outBuffer;
	}
	EXPORT(ResourceFactory_CreateVertexBuffer, ID3D11Device* devicePtr,
		uint32_t vertexStructSizeBytes, uint32_t numVertices, int32_t usage, int32_t cpuUsage, void* initialDataPtr,
		ID3D11Buffer** outResPtr) {
		D3D11_SUBRESOURCE_DATA* d3d11InitDataPtr = nullptr;
		D3D11_SUBRESOURCE_DATA subresourceData;
		if (initialDataPtr != nullptr) {
			ZeroMemory(&subresourceData, sizeof(subresourceData));
			subresourceData.pSysMem = initialDataPtr;

			d3d11InitDataPtr = &subresourceData;
		}

		*outResPtr = ResourceFactory::CreateVertexBuffer(
			devicePtr, vertexStructSizeBytes, numVertices, 
			static_cast<D3D11_USAGE>(usage), static_cast<D3D11_CPU_ACCESS_FLAG>(cpuUsage), d3d11InitDataPtr
		);
		EXPORT_END;
	}
	ID3D11Buffer* ResourceFactory::CreateIndexBuffer(ID3D11Device* devicePtr, UINT numIndices,
		D3D11_USAGE usage, D3D11_CPU_ACCESS_FLAG cpuUsage, D3D11_SUBRESOURCE_DATA* initialDataPtr) {
		if (devicePtr == nullptr) throw LosgapException { "Device pointer was null!" };
		if (initialDataPtr == nullptr && usage == D3D11_USAGE::D3D11_USAGE_IMMUTABLE) {
			throw LosgapException { "Initial data must not be null when creating an immutable resource." };
		}

		D3D11_BUFFER_DESC bufferDesc;
		ZeroMemory(&bufferDesc, sizeof(bufferDesc));

		bufferDesc.BindFlags = D3D11_BIND_FLAG::D3D11_BIND_INDEX_BUFFER;
		bufferDesc.ByteWidth = GetFormatByteSize(INDEX_BUFFER_FORMAT) * numIndices;
		bufferDesc.CPUAccessFlags = cpuUsage;
		bufferDesc.Usage = usage;
		bufferDesc.StructureByteStride = GetFormatByteSize(INDEX_BUFFER_FORMAT);

		ID3D11Buffer* outBuffer;
		CHECK_CALL(devicePtr->CreateBuffer(&bufferDesc, initialDataPtr, &outBuffer));

		return outBuffer;
	}
	EXPORT(ResourceFactory_CreateIndexBuffer, ID3D11Device* devicePtr,
		uint32_t numIndices, int32_t usage, int32_t cpuUsage, void* initialDataPtr,
		ID3D11Buffer** outResPtr) {
		D3D11_SUBRESOURCE_DATA* d3d11InitDataPtr = nullptr;
		if (initialDataPtr != nullptr) {
			D3D11_SUBRESOURCE_DATA subresourceData;
			ZeroMemory(&subresourceData, sizeof(subresourceData));
			subresourceData.pSysMem = initialDataPtr;
			subresourceData.SysMemSlicePitch = subresourceData.SysMemPitch = 
				numIndices * ResourceFactory::GetFormatByteSize(ResourceFactory::INDEX_BUFFER_FORMAT);

			d3d11InitDataPtr = &subresourceData;
		}

		*outResPtr = ResourceFactory::CreateIndexBuffer(
			devicePtr, numIndices,
			static_cast<D3D11_USAGE>(usage), static_cast<D3D11_CPU_ACCESS_FLAG>(cpuUsage), d3d11InitDataPtr
		);
		EXPORT_END;
	}
	ID3D11Buffer* ResourceFactory::CreateConstantBuffer(ID3D11Device* devicePtr, UINT numBytes,
		D3D11_USAGE usage, D3D11_CPU_ACCESS_FLAG cpuUsage, D3D11_SUBRESOURCE_DATA* initialDataPtr) {
		if (devicePtr == nullptr) throw LosgapException { "Device pointer was null!" };
		if (initialDataPtr == nullptr && usage == D3D11_USAGE::D3D11_USAGE_IMMUTABLE) {
			throw LosgapException { "Initial data must not be null when creating an immutable resource." };
		}

		D3D11_BUFFER_DESC bufferDesc;
		ZeroMemory(&bufferDesc, sizeof(bufferDesc));

		bufferDesc.BindFlags = D3D11_BIND_FLAG::D3D11_BIND_CONSTANT_BUFFER;
		bufferDesc.ByteWidth = numBytes;
		bufferDesc.CPUAccessFlags = cpuUsage;
		bufferDesc.Usage = usage;

		ID3D11Buffer* outBuffer;
		CHECK_CALL(devicePtr->CreateBuffer(&bufferDesc, initialDataPtr, &outBuffer));

		return outBuffer;
	}
	EXPORT(ResourceFactory_CreateConstantBuffer, ID3D11Device* devicePtr,
		uint32_t numBytes, int32_t usage, int32_t cpuUsage, void* initialDataPtr,
		ID3D11Buffer** outResPtr) {
		D3D11_SUBRESOURCE_DATA* d3d11InitDataPtr = nullptr;
		if (initialDataPtr != nullptr) {
			D3D11_SUBRESOURCE_DATA subresourceData;
			ZeroMemory(&subresourceData, sizeof(subresourceData));
			subresourceData.pSysMem = initialDataPtr;

			d3d11InitDataPtr = &subresourceData;
		}

		*outResPtr = ResourceFactory::CreateConstantBuffer(
			devicePtr, numBytes,
			static_cast<D3D11_USAGE>(usage), static_cast<D3D11_CPU_ACCESS_FLAG>(cpuUsage), d3d11InitDataPtr
		);
		EXPORT_END;
	}
	ID3D11Buffer* ResourceFactory::CreateBuffer(ID3D11Device* devicePtr, UINT elementSizeBytes, UINT numElements,
		D3D11_USAGE usage, D3D11_CPU_ACCESS_FLAG cpuUsage, D3D11_BIND_FLAG pipelineBindings, bool isStructured,
		bool allowRawAccess, D3D11_SUBRESOURCE_DATA* initialDataPtr) {
		if (devicePtr == nullptr) throw LosgapException { "Device pointer was null!" };
		if (initialDataPtr == nullptr && usage == D3D11_USAGE::D3D11_USAGE_IMMUTABLE) {
			throw LosgapException { "Initial data must not be null when creating an immutable resource." };
		}
		if (elementSizeBytes == 0U) throw LosgapException { "Element size must be greater than zero." };

		D3D11_BUFFER_DESC bufferDesc;
		ZeroMemory(&bufferDesc, sizeof(bufferDesc));

		bufferDesc.BindFlags = pipelineBindings;
		bufferDesc.ByteWidth = elementSizeBytes * numElements;
		bufferDesc.CPUAccessFlags = cpuUsage;
		bufferDesc.Usage = usage;
		bufferDesc.StructureByteStride = elementSizeBytes;
		bufferDesc.MiscFlags = 
			(isStructured ? D3D11_RESOURCE_MISC_FLAG::D3D11_RESOURCE_MISC_BUFFER_STRUCTURED : 0)
			| (allowRawAccess ? D3D11_RESOURCE_MISC_FLAG::D3D11_RESOURCE_MISC_BUFFER_ALLOW_RAW_VIEWS : 0);

		ID3D11Buffer* outBuffer;
		CHECK_CALL(devicePtr->CreateBuffer(&bufferDesc, initialDataPtr, &outBuffer));

		return outBuffer;
	}
	EXPORT(ResourceFactory_CreateBuffer, ID3D11Device* devicePtr,
		uint32_t elementSizeBytes, uint32_t numElements, int32_t usage, int32_t cpuUsage, int32_t pipelineBindings,
		INTEROP_BOOL isStructured, INTEROP_BOOL allowRawAccess, void* initialDataPtr, ID3D11Buffer** outResPtr) {
		D3D11_SUBRESOURCE_DATA* d3d11InitDataPtr = nullptr;
		if (initialDataPtr != nullptr) {
			D3D11_SUBRESOURCE_DATA subresourceData;
			ZeroMemory(&subresourceData, sizeof(subresourceData));
			subresourceData.pSysMem = initialDataPtr;

			d3d11InitDataPtr = &subresourceData;
		}

		*outResPtr = ResourceFactory::CreateBuffer(
			devicePtr, elementSizeBytes, numElements,
			static_cast<D3D11_USAGE>(usage), static_cast<D3D11_CPU_ACCESS_FLAG>(cpuUsage),
			static_cast<D3D11_BIND_FLAG>(pipelineBindings), INTEROP_BOOL_TO_CBOOL(isStructured),
			INTEROP_BOOL_TO_CBOOL(allowRawAccess), d3d11InitDataPtr
		);
		EXPORT_END;
	}
#pragma endregion
#pragma region ::CreateTexture1D[...]()
	ID3D11Texture1D* ResourceFactory::CreateTexture1DArray(ID3D11Device* devicePtr, UINT widthTx, UINT numTextures,
		bool allocateMipmaps, DXGI_FORMAT format, D3D11_USAGE usage, D3D11_CPU_ACCESS_FLAG cpuUsage,
		D3D11_BIND_FLAG pipelineBindings, bool allowMipGeneration, bool allowLODClamping, D3D11_SUBRESOURCE_DATA* initialDataPtr) {
		if (devicePtr == nullptr) throw LosgapException { "Device pointer was null!" };
		if (initialDataPtr == nullptr && usage == D3D11_USAGE::D3D11_USAGE_IMMUTABLE) {
			throw LosgapException { "Initial data must not be null when creating an immutable resource." };
		}
		if (allowLODClamping && !allocateMipmaps) {
			throw LosgapException { "Invalid configuration: Allowing LOD clamping has no effect when not allocating for mip mapping." };
		}

		D3D11_TEXTURE1D_DESC texDesc;
		ZeroMemory(&texDesc, sizeof(texDesc));
		texDesc.Width = widthTx;
		texDesc.MipLevels = allocateMipmaps ? 0U : 1U;
		texDesc.ArraySize = numTextures;
		texDesc.Format = format;
		texDesc.Usage = usage;
		texDesc.BindFlags = pipelineBindings;
		texDesc.CPUAccessFlags = cpuUsage;
		texDesc.MiscFlags =
			(allowMipGeneration ? D3D11_RESOURCE_MISC_FLAG::D3D11_RESOURCE_MISC_GENERATE_MIPS : 0)
			| (allowLODClamping ? D3D11_RESOURCE_MISC_FLAG::D3D11_RESOURCE_MISC_RESOURCE_CLAMP : 0);


		ID3D11Texture1D* outTexture;
		CHECK_CALL(devicePtr->CreateTexture1D(&texDesc, initialDataPtr, &outTexture));

		return outTexture;
	}
	EXPORT(ResourceFactory_CreateTexture1DArray, ID3D11Device* devicePtr,
		uint32_t widthTx, uint32_t numTextures, INTEROP_BOOL allocateMipmaps, 
		int32_t format, int32_t usage, int32_t cpuUsage, int32_t pipelineBindings,
		INTEROP_BOOL allocateMipGeneration, INTEROP_BOOL allowLODClamping, InitialResourceDataDesc* initialDataArrPtr, 
		uint32_t initialDataArrLen, ID3D11Texture1D** outResPtr) {
		D3D11_SUBRESOURCE_DATA* d3d11InitDataPtr = nullptr;
		if (initialDataArrPtr != nullptr) {
			d3d11InitDataPtr = new D3D11_SUBRESOURCE_DATA[initialDataArrLen];

			for (uint32_t i = 0; i < initialDataArrLen; ++i) {
				d3d11InitDataPtr[i].pSysMem = initialDataArrPtr[i].Data;
				d3d11InitDataPtr[i].SysMemPitch = initialDataArrPtr[i].DataRowDistanceBytes;
				d3d11InitDataPtr[i].SysMemSlicePitch = initialDataArrPtr[i].DataSliceDistanceBytes;
			}
		}

		*outResPtr = ResourceFactory::CreateTexture1DArray(
			devicePtr, widthTx, numTextures, INTEROP_BOOL_TO_CBOOL(allocateMipmaps), static_cast<DXGI_FORMAT>(format),
			static_cast<D3D11_USAGE>(usage), static_cast<D3D11_CPU_ACCESS_FLAG>(cpuUsage),
			static_cast<D3D11_BIND_FLAG>(pipelineBindings), INTEROP_BOOL_TO_CBOOL(allocateMipGeneration),
			INTEROP_BOOL_TO_CBOOL(allowLODClamping), d3d11InitDataPtr
		);
		EXPORT_END;
	}
#pragma endregion
#pragma region ::CreateTexture2D[...]()
	ID3D11Texture2D* ResourceFactory::CreateTexture2DArray(ID3D11Device* devicePtr, UINT widthTx, UINT heightTx, UINT numTextures,
		bool allocateMipmaps, bool multisampled, DXGI_FORMAT format, D3D11_USAGE usage, D3D11_CPU_ACCESS_FLAG cpuUsage,
		D3D11_BIND_FLAG pipelineBindings, bool allowMipGeneration, bool allowLODClamping, D3D11_SUBRESOURCE_DATA* initialDataPtr) {
		if (devicePtr == nullptr) throw LosgapException { "Device pointer was null!" };
		if (initialDataPtr == nullptr && usage == D3D11_USAGE::D3D11_USAGE_IMMUTABLE) {
			throw LosgapException { "Initial data must not be null when creating an immutable resource." };
		}
		if (initialDataPtr != nullptr && multisampled) {
			throw LosgapException { "Initial subresource data must be null when creating a multisampled texture." };
		}
		if (allowLODClamping && !allocateMipmaps) {
			throw LosgapException { "Invalid configuration: Allowing LOD clamping has no effect when not allocating for mip mapping." };
		}
		if (multisampled && allocateMipmaps) {
			throw LosgapException { "Can not create a multisampled 2D texture with mipmap allocation." };
		}

		D3D11_TEXTURE2D_DESC texDesc;
		ZeroMemory(&texDesc, sizeof(texDesc));
		texDesc.Width = widthTx;
		texDesc.Height = heightTx;
		texDesc.MipLevels = allocateMipmaps ? 0U : 1U;
		texDesc.ArraySize = numTextures;
		texDesc.Format = format;
		if (multisampled) {
			texDesc.SampleDesc = DeviceFactory::GetMSAASampleDesc(devicePtr, format);
		}
		else {
			texDesc.SampleDesc.Count = 1;
			texDesc.SampleDesc.Quality = 0;
		}

		texDesc.Usage = usage;
		texDesc.BindFlags = pipelineBindings;
		texDesc.CPUAccessFlags = cpuUsage;
		texDesc.MiscFlags = 
			(allowMipGeneration ? D3D11_RESOURCE_MISC_FLAG::D3D11_RESOURCE_MISC_GENERATE_MIPS : 0)
			| (allowLODClamping ? D3D11_RESOURCE_MISC_FLAG::D3D11_RESOURCE_MISC_RESOURCE_CLAMP : 0);

		
		ID3D11Texture2D* outTexture;
		CHECK_CALL(devicePtr->CreateTexture2D(&texDesc, initialDataPtr, &outTexture));

		return outTexture;
	}
	EXPORT(ResourceFactory_CreateTexture2DArray, ID3D11Device* devicePtr,
		uint32_t widthTx, uint32_t heightTx, uint32_t numTextures, INTEROP_BOOL allocateMipmaps, INTEROP_BOOL multisampled,
		int32_t format, int32_t usage, int32_t cpuUsage, int32_t pipelineBindings,
		INTEROP_BOOL allocateMipGeneration, INTEROP_BOOL allowLODClamping, InitialResourceDataDesc* initialDataArrPtr,
		uint32_t initialDataArrLen, ID3D11Texture2D** outResPtr) {
		D3D11_SUBRESOURCE_DATA* d3d11InitDataPtr = nullptr;
		if (initialDataArrPtr != nullptr) {
			d3d11InitDataPtr = new D3D11_SUBRESOURCE_DATA[initialDataArrLen];

			for (uint32_t i = 0; i < initialDataArrLen; ++i) {
				d3d11InitDataPtr[i].pSysMem = initialDataArrPtr[i].Data;
				d3d11InitDataPtr[i].SysMemPitch = initialDataArrPtr[i].DataRowDistanceBytes;
				d3d11InitDataPtr[i].SysMemSlicePitch = initialDataArrPtr[i].DataSliceDistanceBytes;
			}
		}

		*outResPtr = ResourceFactory::CreateTexture2DArray(
			devicePtr, widthTx, heightTx, numTextures, INTEROP_BOOL_TO_CBOOL(allocateMipmaps), INTEROP_BOOL_TO_CBOOL(multisampled),
			static_cast<DXGI_FORMAT>(format), static_cast<D3D11_USAGE>(usage), static_cast<D3D11_CPU_ACCESS_FLAG>(cpuUsage),
			static_cast<D3D11_BIND_FLAG>(pipelineBindings), INTEROP_BOOL_TO_CBOOL(allocateMipGeneration),
			INTEROP_BOOL_TO_CBOOL(allowLODClamping), d3d11InitDataPtr
			);
		EXPORT_END;
	}

	ID3D11Texture2D* ResourceFactory::CreateTexture2DCubeArray(ID3D11Device* devicePtr, UINT widthTx, UINT heightTx, UINT numCubes,
		bool allocateMipmaps, bool multisampled, DXGI_FORMAT format, D3D11_USAGE usage, D3D11_CPU_ACCESS_FLAG cpuUsage,
		D3D11_BIND_FLAG pipelineBindings, bool allowMipGeneration, bool allowLODClamping, D3D11_SUBRESOURCE_DATA* initialDataPtr) {
		if (devicePtr == nullptr) throw LosgapException { "Device pointer was null!" };
		if (initialDataPtr == nullptr && usage == D3D11_USAGE::D3D11_USAGE_IMMUTABLE) {
			throw LosgapException { "Initial data must not be null when creating an immutable resource." };
		}
		if (initialDataPtr != nullptr && multisampled) {
			throw LosgapException { "Initial subresource data must be null when creating a multisampled texture." };
		}
		if (allowLODClamping && !allocateMipmaps) {
			throw LosgapException { "Invalid configuration: Allowing LOD clamping has no effect when not allocating for mip mapping." };
		}
		if (multisampled && allocateMipmaps) {
			throw LosgapException { "Can not create a multisampled 2D texture with mipmap allocation." };
		}

		D3D11_TEXTURE2D_DESC texDesc;
		ZeroMemory(&texDesc, sizeof(texDesc));
		texDesc.Width = widthTx;
		texDesc.Height = heightTx;
		texDesc.MipLevels = allocateMipmaps ? 0U : 1U;
		texDesc.ArraySize = numCubes * 6U;
		texDesc.Format = format;
		if (multisampled) {
			texDesc.SampleDesc = DeviceFactory::GetMSAASampleDesc(devicePtr, format);
		}
		else {
			texDesc.SampleDesc.Count = 1;
			texDesc.SampleDesc.Quality = 0;
		}

		texDesc.Usage = usage;
		texDesc.BindFlags = pipelineBindings;
		texDesc.CPUAccessFlags = cpuUsage;
		texDesc.MiscFlags =
			D3D11_RESOURCE_MISC_FLAG::D3D11_RESOURCE_MISC_TEXTURECUBE
			| (allowMipGeneration ? D3D11_RESOURCE_MISC_FLAG::D3D11_RESOURCE_MISC_GENERATE_MIPS : 0)
			| (allowLODClamping ? D3D11_RESOURCE_MISC_FLAG::D3D11_RESOURCE_MISC_RESOURCE_CLAMP : 0);


		ID3D11Texture2D* outTexture;
		CHECK_CALL(devicePtr->CreateTexture2D(&texDesc, initialDataPtr, &outTexture));

		return outTexture;
	}
	EXPORT(ResourceFactory_CreateTexture2DCubeArray, ID3D11Device* devicePtr,
		uint32_t widthTx, uint32_t heightTx, uint32_t numCubes, INTEROP_BOOL allocateMipmaps, INTEROP_BOOL multisampled,
		int32_t format, int32_t usage, int32_t cpuUsage, int32_t pipelineBindings,
		INTEROP_BOOL allocateMipGeneration, INTEROP_BOOL allowLODClamping, InitialResourceDataDesc* initialDataArrPtr,
		uint32_t initialDataArrLen, ID3D11Texture2D** outResPtr) {
		D3D11_SUBRESOURCE_DATA* d3d11InitDataPtr = nullptr;
		if (initialDataArrPtr != nullptr) {
			d3d11InitDataPtr = new D3D11_SUBRESOURCE_DATA[initialDataArrLen];

			for (uint32_t i = 0; i < initialDataArrLen; ++i) {
				d3d11InitDataPtr[i].pSysMem = initialDataArrPtr[i].Data;
				d3d11InitDataPtr[i].SysMemPitch = initialDataArrPtr[i].DataRowDistanceBytes;
				d3d11InitDataPtr[i].SysMemSlicePitch = initialDataArrPtr[i].DataSliceDistanceBytes;
			}
		}

		*outResPtr = ResourceFactory::CreateTexture2DCubeArray(
			devicePtr, widthTx, heightTx, numCubes, INTEROP_BOOL_TO_CBOOL(allocateMipmaps), INTEROP_BOOL_TO_CBOOL(multisampled),
			static_cast<DXGI_FORMAT>(format), static_cast<D3D11_USAGE>(usage), static_cast<D3D11_CPU_ACCESS_FLAG>(cpuUsage),
			static_cast<D3D11_BIND_FLAG>(pipelineBindings), INTEROP_BOOL_TO_CBOOL(allocateMipGeneration),
			INTEROP_BOOL_TO_CBOOL(allowLODClamping), d3d11InitDataPtr
			);
		EXPORT_END;
	}

	ID3D11Texture2D* ResourceFactory::LoadTexture2D(ID3D11Device* devicePtr, LosgapString filePath, bool allocateMipmaps,
		D3D11_USAGE usage, D3D11_CPU_ACCESS_FLAG cpuUsage, D3D11_BIND_FLAG pipelineBindings, bool allowMipGeneration, bool allowLODClamping) {
		if (devicePtr == nullptr) throw LosgapException { "Device pointer was null!" };
		if (allowLODClamping && !allocateMipmaps) {
			throw LosgapException { "Invalid configuration: Allowing LOD clamping has no effect when not allocating for mip mapping." };
		}
		
		ID3D11Resource* outResult;
		std::unique_ptr<const wchar_t> filePathAsCWStr = LosgapString::AsNewCWString(filePath);

		CHECK_CALL(DirectX::CreateWICTextureFromFileEx(
			devicePtr,
			filePathAsCWStr.get(),
			0,
			usage,
			pipelineBindings,
			cpuUsage,
			(allowMipGeneration ? D3D11_RESOURCE_MISC_FLAG::D3D11_RESOURCE_MISC_GENERATE_MIPS : 0) 
				| (allowLODClamping ? D3D11_RESOURCE_MISC_FLAG::D3D11_RESOURCE_MISC_RESOURCE_CLAMP : 0),
			false,
			&outResult,
			nullptr
		));

		return static_cast<ID3D11Texture2D*>(outResult);
	}
	EXPORT(ResourceFactory_LoadTexture2D, ID3D11Device* devicePtr, INTEROP_STRING filePath, INTEROP_BOOL allocateMipmaps, D3D11_USAGE usage,
		D3D11_CPU_ACCESS_FLAG cpuUsage, D3D11_BIND_FLAG pipelineBindings, INTEROP_BOOL allowMipGeneration, INTEROP_BOOL allowLODClamping,
		ID3D11Texture2D** outResPtr, uint32_t* outWidth, uint32_t* outHeight, DXGI_FORMAT* outFormat) {

		*outResPtr = ResourceFactory::LoadTexture2D(
			devicePtr,
			filePath,
			INTEROP_BOOL_TO_CBOOL(allocateMipmaps),
			usage,
			cpuUsage,
			pipelineBindings,
			INTEROP_BOOL_TO_CBOOL(allowMipGeneration),
			INTEROP_BOOL_TO_CBOOL(allowLODClamping)
		);

		D3D11_TEXTURE2D_DESC outTexDesc;
		(*outResPtr)->GetDesc(&outTexDesc);

		*outWidth = outTexDesc.Width;
		*outHeight = outTexDesc.Height;
		*outFormat = outTexDesc.Format;
		EXPORT_END;
	}
#pragma endregion
#pragma region ::CreateTexture3D[...]()
	ID3D11Texture3D* ResourceFactory::CreateTexture3D(ID3D11Device* devicePtr, UINT widthTx, UINT heightTx, UINT depthTx,
		bool allocateMipmaps, DXGI_FORMAT format, D3D11_USAGE usage, D3D11_CPU_ACCESS_FLAG cpuUsage,
		D3D11_BIND_FLAG pipelineBindings, bool allowMipGeneration, bool allowLODClamping, D3D11_SUBRESOURCE_DATA* initialDataPtr) {
		if (devicePtr == nullptr) throw LosgapException { "Device pointer was null!" };
		if (initialDataPtr == nullptr && usage == D3D11_USAGE::D3D11_USAGE_IMMUTABLE) {
			throw LosgapException { "Initial data must not be null when creating an immutable resource." };
		}
		if (allowLODClamping && !allocateMipmaps) {
			throw LosgapException { "Invalid configuration: Allowing LOD clamping has no effect when not allocating for mip mapping." };
		}

		D3D11_TEXTURE3D_DESC texDesc;
		ZeroMemory(&texDesc, sizeof(texDesc));
		texDesc.Width = widthTx;
		texDesc.Height = heightTx;
		texDesc.Depth = depthTx;
		texDesc.MipLevels = allocateMipmaps ? 0U : 1U;
		texDesc.Format = format;
		texDesc.Usage = usage;
		texDesc.BindFlags = pipelineBindings;
		texDesc.CPUAccessFlags = cpuUsage;
		texDesc.MiscFlags =
			(allowMipGeneration ? D3D11_RESOURCE_MISC_FLAG::D3D11_RESOURCE_MISC_GENERATE_MIPS : 0)
			| (allowLODClamping ? D3D11_RESOURCE_MISC_FLAG::D3D11_RESOURCE_MISC_RESOURCE_CLAMP : 0);


		ID3D11Texture3D* outTexture;
		CHECK_CALL(devicePtr->CreateTexture3D(&texDesc, initialDataPtr, &outTexture));

		return outTexture;
	}
	EXPORT(ResourceFactory_CreateTexture3D, ID3D11Device* devicePtr,
		uint32_t widthTx, uint32_t heightTx, uint32_t depthTx, INTEROP_BOOL allocateMipmaps,
		int32_t format, int32_t usage, int32_t cpuUsage, int32_t pipelineBindings,
		INTEROP_BOOL allocateMipGeneration, INTEROP_BOOL allowLODClamping, InitialResourceDataDesc* initialDataArrPtr,
		uint32_t initialDataArrLen, ID3D11Texture3D** outResPtr) {
		D3D11_SUBRESOURCE_DATA* d3d11InitDataPtr = nullptr;
		if (initialDataArrPtr != nullptr) {
			d3d11InitDataPtr = new D3D11_SUBRESOURCE_DATA[initialDataArrLen];

			for (uint32_t i = 0; i < initialDataArrLen; ++i) {
				d3d11InitDataPtr[i].pSysMem = initialDataArrPtr[i].Data;
				d3d11InitDataPtr[i].SysMemPitch = initialDataArrPtr[i].DataRowDistanceBytes;
				d3d11InitDataPtr[i].SysMemSlicePitch = initialDataArrPtr[i].DataSliceDistanceBytes;
			}
		}

		*outResPtr = ResourceFactory::CreateTexture3D(
			devicePtr, widthTx, heightTx, depthTx, INTEROP_BOOL_TO_CBOOL(allocateMipmaps),
			static_cast<DXGI_FORMAT>(format), static_cast<D3D11_USAGE>(usage), static_cast<D3D11_CPU_ACCESS_FLAG>(cpuUsage),
			static_cast<D3D11_BIND_FLAG>(pipelineBindings), INTEROP_BOOL_TO_CBOOL(allocateMipGeneration),
			INTEROP_BOOL_TO_CBOOL(allowLODClamping), d3d11InitDataPtr
			);
		EXPORT_END;
	}
#pragma endregion
#pragma region ::CreateSSO()
	ID3D11SamplerState* ResourceFactory::CreateSSO(ID3D11Device* devicePtr, D3D11_FILTER textureFilterType,
		D3D11_TEXTURE_ADDRESS_MODE textureAddressingMode, UINT maxAnisotropy,
		FLOAT borderColorR, FLOAT borderColorG, FLOAT borderColorB, FLOAT borderColorA) {
		if (devicePtr == nullptr) throw LosgapException { "Device pointer was null!" };

		D3D11_SAMPLER_DESC samplerDesc;
		samplerDesc.Filter = textureFilterType;
		samplerDesc.AddressU = samplerDesc.AddressV = samplerDesc.AddressW = textureAddressingMode;
		samplerDesc.MaxAnisotropy = maxAnisotropy;
		samplerDesc.BorderColor[0] = borderColorR;
		samplerDesc.BorderColor[1] = borderColorG;
		samplerDesc.BorderColor[2] = borderColorB;
		samplerDesc.BorderColor[3] = borderColorA;
		samplerDesc.ComparisonFunc = D3D11_COMPARISON_FUNC::D3D11_COMPARISON_NEVER;
		samplerDesc.MipLODBias = 0.0f;
		samplerDesc.MinLOD = 0.0f;
		samplerDesc.MaxLOD = D3D11_FLOAT32_MAX;

		ID3D11SamplerState* outSSO;

		CHECK_CALL(devicePtr->CreateSamplerState(&samplerDesc, &outSSO));

		return outSSO;
	}
	EXPORT(ResourceFactory_CreateSSO, ID3D11Device* devicePtr, D3D11_FILTER textureFilterType,
		D3D11_TEXTURE_ADDRESS_MODE textureAddressingMode, uint32_t maxAnisotropy,
		float_t borderColorR, float_t borderColorG, float_t borderColorB, float_t borderColorA,
		ID3D11SamplerState** outResPtr) {
		*outResPtr = ResourceFactory::CreateSSO(devicePtr, textureFilterType, textureAddressingMode, maxAnisotropy, borderColorR,
			borderColorG, borderColorB, borderColorA);

		EXPORT_END;
	}
#pragma endregion
#pragma region ::CreateRSState()
	ID3D11RasterizerState* ResourceFactory::CreateRSState(ID3D11Device* devicePtr, D3D11_FILL_MODE fillMode, D3D11_CULL_MODE cullMode,
		bool flipFaces, int depthBias, float depthBiasClamp, float slopeScaledDepthBias, bool enableZClipping) {
		if (devicePtr == nullptr) throw LosgapException { "Device pointer must not be null." };
		ID3D11RasterizerState* result;

		D3D11_RASTERIZER_DESC stateDesc;
		stateDesc.AntialiasedLineEnable = true;
		stateDesc.CullMode = cullMode;
		stateDesc.DepthBias = depthBias;
		stateDesc.DepthBiasClamp = depthBiasClamp;
		stateDesc.DepthClipEnable = enableZClipping;
		stateDesc.FillMode = fillMode;
		stateDesc.FrontCounterClockwise = flipFaces;
		stateDesc.MultisampleEnable = true;
		stateDesc.ScissorEnable = false;
		stateDesc.SlopeScaledDepthBias = slopeScaledDepthBias;

		CHECK_CALL(devicePtr->CreateRasterizerState(&stateDesc, &result));

		return result;
	}
	EXPORT(ResourceFactory_CreateRSState, ID3D11Device* devicePtr, D3D11_FILL_MODE fillMode, D3D11_CULL_MODE cullMode, INTEROP_BOOL flipFaces, 
		int32_t depthBias, float_t depthBiasClamp, float_t slopeScaledDepthBias, INTEROP_BOOL enableZClipping, 
		ID3D11RasterizerState** outRasterizerStatePtr) {
		*outRasterizerStatePtr = ResourceFactory::CreateRSState(
			devicePtr,
			fillMode,
			cullMode,
			INTEROP_BOOL_TO_CBOOL(flipFaces),
			depthBias,
			depthBiasClamp,
			slopeScaledDepthBias,
			INTEROP_BOOL_TO_CBOOL(enableZClipping)
		);
		EXPORT_END;
	}
#pragma endregion
#pragma region ::CreateDSState()
	ID3D11DepthStencilState* ResourceFactory::CreateDSState(ID3D11Device* devicePtr, bool depthEnable, D3D11_COMPARISON_FUNC comparisonFunc) {
		if (devicePtr == nullptr) throw LosgapException { "Device pointer should not be null." };
		D3D11_DEPTH_STENCIL_DESC dsDesc;
		ZeroMemory(&dsDesc, sizeof(dsDesc));

		dsDesc.DepthEnable = static_cast<BOOL>(depthEnable);
		dsDesc.DepthWriteMask = D3D11_DEPTH_WRITE_MASK::D3D11_DEPTH_WRITE_MASK_ALL;
		dsDesc.DepthFunc = comparisonFunc;
		dsDesc.StencilEnable = FALSE;
		dsDesc.StencilReadMask = 0xFF;
		dsDesc.StencilWriteMask = 0x00;
		dsDesc.FrontFace.StencilFunc = D3D11_COMPARISON_ALWAYS;
		dsDesc.FrontFace.StencilDepthFailOp = D3D11_STENCIL_OP_KEEP;
		dsDesc.FrontFace.StencilPassOp = D3D11_STENCIL_OP_KEEP;
		dsDesc.FrontFace.StencilFailOp = D3D11_STENCIL_OP_KEEP;
		dsDesc.BackFace.StencilFunc = D3D11_COMPARISON_ALWAYS;
		dsDesc.BackFace.StencilDepthFailOp = D3D11_STENCIL_OP_KEEP;
		dsDesc.BackFace.StencilPassOp = D3D11_STENCIL_OP_KEEP;
		dsDesc.BackFace.StencilFailOp = D3D11_STENCIL_OP_KEEP;

		ID3D11DepthStencilState* outDSStatePtr;
		CHECK_CALL(devicePtr->CreateDepthStencilState(&dsDesc, &outDSStatePtr));
		
		return outDSStatePtr;
	}
	EXPORT(ResourceFactory_CreateDSState, ID3D11Device* devicePtr, INTEROP_BOOL depthEnable, D3D11_COMPARISON_FUNC comparisonFunc, 
		ID3D11DepthStencilState** outDSStatePtr) {
		*outDSStatePtr = ResourceFactory::CreateDSState(devicePtr, INTEROP_BOOL_TO_CBOOL(depthEnable), comparisonFunc);
		EXPORT_END;
	}
#pragma endregion
#pragma region ::CreateBlendState()
	ID3D11BlendState* ResourceFactory::CreateBlendState(ID3D11Device* devicePtr, bool enableBlending, D3D11_BLEND_OP blendOp) {
		if (devicePtr == nullptr) throw LosgapException { "Device pointer must not be null." };
		D3D11_BLEND_DESC blendDesc;
		ZeroMemory(&blendDesc, sizeof(blendDesc));
		blendDesc.AlphaToCoverageEnable = FALSE;
		blendDesc.IndependentBlendEnable = FALSE;
		blendDesc.RenderTarget[0].BlendEnable = static_cast<BOOL>(enableBlending);
		blendDesc.RenderTarget[0].SrcBlend =
			blendDesc.RenderTarget[0].DestBlend =
			blendDesc.RenderTarget[0].SrcBlendAlpha =
			blendDesc.RenderTarget[0].DestBlendAlpha = D3D11_BLEND::D3D11_BLEND_ONE;
		if ((blendOp & 0x10000) != 0) {
			blendOp = static_cast<D3D11_BLEND_OP>(blendOp & ~0x10000);
			blendDesc.RenderTarget[0].SrcBlend = D3D11_BLEND::D3D11_BLEND_SRC_ALPHA;
			blendDesc.RenderTarget[0].DestBlend = D3D11_BLEND::D3D11_BLEND_INV_SRC_ALPHA;
		}
		blendDesc.RenderTarget[0].BlendOp = blendDesc.RenderTarget[0].BlendOpAlpha = blendOp;
		blendDesc.RenderTarget[0].RenderTargetWriteMask = D3D11_COLOR_WRITE_ENABLE_ALL;

		ID3D11BlendState* outBlendState;
		CHECK_CALL(devicePtr->CreateBlendState(&blendDesc, &outBlendState));

		return outBlendState;
	}
	EXPORT(ResourceFactory_CreateBlendState, ID3D11Device* devicePtr, INTEROP_BOOL enableBlending, D3D11_BLEND_OP blendOp, ID3D11BlendState** outBlendState) {
		*outBlendState = ResourceFactory::CreateBlendState(devicePtr, INTEROP_BOOL_TO_CBOOL(enableBlending), blendOp);
		EXPORT_END;
	}
#pragma endregion
#pragma endregion

#pragma region Resource Manipulations
#pragma region ::CopyResource()
	void ResourceFactory::CopyResource(ID3D11DeviceContext* contextPtr, ID3D11Resource* srcResPtr, ID3D11Resource* dstResPtr) {
		if (contextPtr == nullptr) throw LosgapException { "Context pointer must not be null." };
		if (srcResPtr == nullptr) throw LosgapException { "Source resource pointer must not be null." };
		if (dstResPtr == nullptr) throw LosgapException { "Destination resource pointer must not be null." };

		contextPtr->CopyResource(dstResPtr, srcResPtr);
	}
	EXPORT(ResourceFactory_CopyResource, ID3D11DeviceContext* contextPtr, ID3D11Resource* srcResPtr, ID3D11Resource* dstResPtr) {
		ResourceFactory::CopyResource(contextPtr, srcResPtr, dstResPtr);
		EXPORT_END;
	}
#pragma endregion
#pragma region ::[Unm/M]apResource()
	D3D11_MAPPED_SUBRESOURCE ResourceFactory::MapSubresource(ID3D11DeviceContext* contextPtr, ID3D11Resource* resourcePtr,
		UINT subresourceIndex, D3D11_MAP mapType) {
		if (contextPtr == nullptr) throw LosgapException { "Context pointer must not be null." };
		if (resourcePtr == nullptr) throw LosgapException { "Resource pointer must not be null." };

		D3D11_MAPPED_SUBRESOURCE result;
		ZeroMemory(&result, sizeof(result));

		CHECK_CALL(contextPtr->Map(
			resourcePtr,
			subresourceIndex,
			mapType,
			0U,
			&result
		));

		return result;
	}
	EXPORT(ResourceFactory_MapSubresource, ID3D11DeviceContext* contextPtr, ID3D11Resource* resourcePtr,
		uint32_t subresourceIndex, D3D11_MAP mapType, 
		void** outResourceDataPtr, uint32_t* outResourceDataRowLenBytes, uint32_t* outResourceDataSliceLenBytes) {
		D3D11_MAPPED_SUBRESOURCE mappedResource = ResourceFactory::MapSubresource(contextPtr, resourcePtr, subresourceIndex, mapType);
		*outResourceDataPtr = mappedResource.pData;
		*outResourceDataRowLenBytes = mappedResource.RowPitch;
		*outResourceDataSliceLenBytes = mappedResource.DepthPitch;
		EXPORT_END;
	}

	void ResourceFactory::UnmapSubresource(ID3D11DeviceContext* contextPtr, ID3D11Resource* resourcePtr, UINT subresourceIndex) {
		if (contextPtr == nullptr) throw LosgapException { "Context pointer must not be null." };
		if (resourcePtr == nullptr) throw LosgapException { "Resource pointer must not be null." };

		contextPtr->Unmap(resourcePtr, subresourceIndex);
	}
	EXPORT(ResourceFactory_UnmapSubresource, ID3D11DeviceContext* contextPtr, ID3D11Resource* resourcePtr, uint32_t subresourceIndex) {
		ResourceFactory::UnmapSubresource(contextPtr, resourcePtr, subresourceIndex);
		EXPORT_END;
	}
#pragma endregion
#pragma region ::UpdateSubresourceRegion()
	void ResourceFactory::UpdateSubresourceRegion(ID3D11DeviceContext* contextPtr, ID3D11Resource* resourcePtr, UINT subresourceIndex,
		const D3D11_BOX* subBox, const void* data, UINT dataRowDistanceBytes, UINT dataSliceDistanceBytes) {
		if (contextPtr == nullptr) throw LosgapException { "Context pointer must not be null." };
		if (resourcePtr == nullptr) throw LosgapException { "Resource pointer must not be null." };

		contextPtr->UpdateSubresource( // Warning: This will fail dramatically on a deferred context if the driver does not support MT rendering.
			resourcePtr, 
			subresourceIndex,
			subBox, 
			data, 
			dataRowDistanceBytes, 
			dataSliceDistanceBytes
		);
	}
	EXPORT(ResourceFactory_UpdateSubresourceRegion, ID3D11DeviceContext* contextPtr, ID3D11Resource* resourcePtr, uint32_t subresourceIndex,
		SubresourceBox* subBox, const void* data, uint32_t dataRowDistanceBytes, uint32_t dataSliceDistanceBytes) {
		D3D11_BOX d3dSubBox;
		if (subBox != nullptr) {
			d3dSubBox.left = subBox->left;
			d3dSubBox.right = subBox->right;
			d3dSubBox.top = subBox->top;
			d3dSubBox.bottom = subBox->bottom;
			d3dSubBox.front = subBox->front;
			d3dSubBox.back = subBox->back;
		}

		ResourceFactory::UpdateSubresourceRegion(
			contextPtr,
			resourcePtr,
			subresourceIndex,
			subBox == nullptr ? nullptr : &d3dSubBox,
			data,
			dataRowDistanceBytes,
			dataSliceDistanceBytes
		);
		EXPORT_END;
	}
#pragma endregion
#pragma region ::CopySubresourceRegion()
	void ResourceFactory::CopySubresourceRegion(ID3D11DeviceContext* contextPtr,
		ID3D11Resource* srcResPtr, UINT srcSubresourceIndex, const D3D11_BOX* srcSubBox,
		ID3D11Resource* dstResPtr, UINT dstSubresourceIndex, UINT dstX, UINT dstY, UINT dstZ) {
		if (contextPtr == nullptr) throw LosgapException { "Device context pointer must not be null." };
		if (srcResPtr == nullptr) throw LosgapException { "Source resource pointer must not be null." };
		if (dstResPtr == nullptr) throw LosgapException { "Destination resource pointer must not be null." };

		contextPtr->CopySubresourceRegion(dstResPtr, dstSubresourceIndex, dstX, dstY, dstZ, srcResPtr, srcSubresourceIndex, srcSubBox);
	}
	EXPORT(ResourceFactory_CopySubresourceRegion, ID3D11DeviceContext* contextPtr,
		ID3D11Resource* srcResPtr, uint32_t srcSubresourceIndex, SubresourceBox* srcSubBox,
		ID3D11Resource* dstResPtr, uint32_t dstSubresourceIndex, uint32_t dstX, uint32_t dstY, uint32_t dstZ) {
		D3D11_BOX d3dSubBox;
		d3dSubBox.left = srcSubBox->left;
		d3dSubBox.right = srcSubBox->right;
		d3dSubBox.top = srcSubBox->top;
		d3dSubBox.bottom = srcSubBox->bottom;
		d3dSubBox.front = srcSubBox->front;
		d3dSubBox.back = srcSubBox->back;

		ResourceFactory::CopySubresourceRegion(
			contextPtr,
			srcResPtr,
			srcSubresourceIndex,
			&d3dSubBox,
			dstResPtr,
			dstSubresourceIndex,
			dstX,
			dstY,
			dstZ
		);

		EXPORT_END;
	}
#pragma endregion
#pragma region ::GenerateMips()
	void ResourceFactory::GenerateMips(ID3D11DeviceContext* contextPtr, ID3D11ShaderResourceView* srvToTexturePtr) {
		if (contextPtr == nullptr) throw LosgapException { "Context pointer must not be null." };
		if (srvToTexturePtr == nullptr) throw LosgapException { "SRV pointer must not be null." };

		contextPtr->GenerateMips(srvToTexturePtr);
	}
	EXPORT(ResourceFactory_GenerateMips, ID3D11DeviceContext* contextPtr, ID3D11ShaderResourceView* srvToTexturePtr) {
		ResourceFactory::GenerateMips(contextPtr, srvToTexturePtr);
		EXPORT_END;
	}
#pragma endregion
#pragma endregion

#pragma region ::ReleaseResourceOrView()
	void ResourceFactory::ReleaseResourceOrView(IUnknown* resourceOrViewPtr) {
		if (resourceOrViewPtr == nullptr) throw LosgapException { "Resource view pointer must not be null." };
		RELEASE_COM(resourceOrViewPtr);
	}
	EXPORT(ResourceFactory_ReleaseResourceOrView, IUnknown* resourceOrViewPtr) {
		ResourceFactory::ReleaseResourceOrView(resourceOrViewPtr);
		EXPORT_END;
	}
#pragma endregion
#pragma region ::SetTextureQualityOffset()
	void ResourceFactory::SetTextureQualityOffset(ID3D11DeviceContext* contextPtr, ID3D11Resource* texturePtr, UINT qualityReductionLevel) {
		if (contextPtr == nullptr) throw LosgapException { "Context pointer must not be null." };
		if (texturePtr == nullptr) throw LosgapException { "Texture pointer must not be null." };

		contextPtr->SetResourceMinLOD(texturePtr, (FLOAT) qualityReductionLevel);
	}
	EXPORT(ResourceFactory_SetTextureQualityOffset, 
		ID3D11DeviceContext* contextPtr, ID3D11Resource* texturePtr, uint32_t qualityReductionLevel) {
		ResourceFactory::SetTextureQualityOffset(contextPtr, texturePtr, qualityReductionLevel);
		EXPORT_END;
	}
#pragma endregion
#pragma region ::GetNumMips()
	UINT ResourceFactory::GetNumMips(ID3D11Texture1D* texturePtr) {
		if (texturePtr == nullptr) throw LosgapException { "Texture pointer must not be null." };

		D3D11_TEXTURE1D_DESC outTextureDesc; 
		ZeroMemory(&outTextureDesc, sizeof(outTextureDesc));

		texturePtr->GetDesc(&outTextureDesc);

		return outTextureDesc.MipLevels;
	}
	EXPORT(ResourceFactory_GetNumMips1D, ID3D11Texture1D* texturePtr, uint32_t* outNumMips) {
		*outNumMips = ResourceFactory::GetNumMips(texturePtr);
		EXPORT_END;
	}

	UINT ResourceFactory::GetNumMips(ID3D11Texture2D* texturePtr) {
		if (texturePtr == nullptr) throw LosgapException { "Texture pointer must not be null." };

		D3D11_TEXTURE2D_DESC outTextureDesc;
		ZeroMemory(&outTextureDesc, sizeof(outTextureDesc));

		texturePtr->GetDesc(&outTextureDesc);

		return outTextureDesc.MipLevels;
	}
	EXPORT(ResourceFactory_GetNumMips2D, ID3D11Texture2D* texturePtr, uint32_t* outNumMips) {
		*outNumMips = ResourceFactory::GetNumMips(texturePtr);
		EXPORT_END;
	}

	UINT ResourceFactory::GetNumMips(ID3D11Texture3D* texturePtr) {
		if (texturePtr == nullptr) throw LosgapException { "Texture pointer must not be null." };

		D3D11_TEXTURE3D_DESC outTextureDesc;
		ZeroMemory(&outTextureDesc, sizeof(outTextureDesc));

		texturePtr->GetDesc(&outTextureDesc);

		return outTextureDesc.MipLevels;
	}
	EXPORT(ResourceFactory_GetNumMips3D, ID3D11Texture3D* texturePtr, uint32_t* outNumMips) {
		*outNumMips = ResourceFactory::GetNumMips(texturePtr);
		EXPORT_END;
	}
#pragma endregion
#pragma region ::GetFormatByteSize()
	UINT ResourceFactory::GetFormatByteSize(DXGI_FORMAT format) {
		switch (format) {
			case DXGI_FORMAT_R32G32B32A32_TYPELESS:
			case DXGI_FORMAT_R32G32B32A32_FLOAT:
			case DXGI_FORMAT_R32G32B32A32_UINT:
			case DXGI_FORMAT_R32G32B32A32_SINT:
				return 16;

			case DXGI_FORMAT_R32G32B32_TYPELESS:
			case DXGI_FORMAT_R32G32B32_FLOAT:
			case DXGI_FORMAT_R32G32B32_UINT:
			case DXGI_FORMAT_R32G32B32_SINT:
				return 12;

			case DXGI_FORMAT_R16G16B16A16_TYPELESS:
			case DXGI_FORMAT_R16G16B16A16_FLOAT:
			case DXGI_FORMAT_R16G16B16A16_UNORM:
			case DXGI_FORMAT_R16G16B16A16_UINT:
			case DXGI_FORMAT_R16G16B16A16_SNORM:
			case DXGI_FORMAT_R16G16B16A16_SINT:
			case DXGI_FORMAT_R32G32_TYPELESS:
			case DXGI_FORMAT_R32G32_FLOAT:
			case DXGI_FORMAT_R32G32_UINT:
			case DXGI_FORMAT_R32G32_SINT:
			case DXGI_FORMAT_R32G8X24_TYPELESS:
			case DXGI_FORMAT_D32_FLOAT_S8X24_UINT:
			case DXGI_FORMAT_R32_FLOAT_X8X24_TYPELESS:
			case DXGI_FORMAT_X32_TYPELESS_G8X24_UINT:
				return 8;

			case DXGI_FORMAT_R10G10B10A2_TYPELESS:
			case DXGI_FORMAT_R10G10B10A2_UNORM:
			case DXGI_FORMAT_R10G10B10A2_UINT:
			case DXGI_FORMAT_R11G11B10_FLOAT:
			case DXGI_FORMAT_R8G8B8A8_TYPELESS:
			case DXGI_FORMAT_R8G8B8A8_UNORM:
			case DXGI_FORMAT_R8G8B8A8_UNORM_SRGB:
			case DXGI_FORMAT_R8G8B8A8_UINT:
			case DXGI_FORMAT_R8G8B8A8_SNORM:
			case DXGI_FORMAT_R8G8B8A8_SINT:
			case DXGI_FORMAT_R16G16_TYPELESS:
			case DXGI_FORMAT_R16G16_FLOAT:
			case DXGI_FORMAT_R16G16_UNORM:
			case DXGI_FORMAT_R16G16_UINT:
			case DXGI_FORMAT_R16G16_SNORM:
			case DXGI_FORMAT_R16G16_SINT:
			case DXGI_FORMAT_R32_TYPELESS:
			case DXGI_FORMAT_D32_FLOAT:
			case DXGI_FORMAT_R32_FLOAT:
			case DXGI_FORMAT_R32_UINT:
			case DXGI_FORMAT_R32_SINT:
			case DXGI_FORMAT_R24G8_TYPELESS:
			case DXGI_FORMAT_D24_UNORM_S8_UINT:
			case DXGI_FORMAT_R24_UNORM_X8_TYPELESS:
			case DXGI_FORMAT_X24_TYPELESS_G8_UINT:
			case DXGI_FORMAT_B8G8R8A8_UNORM:
			case DXGI_FORMAT_B8G8R8X8_UNORM:
				return 4;

			case DXGI_FORMAT_R8G8_TYPELESS:
			case DXGI_FORMAT_R8G8_UNORM:
			case DXGI_FORMAT_R8G8_UINT:
			case DXGI_FORMAT_R8G8_SNORM:
			case DXGI_FORMAT_R8G8_SINT:
			case DXGI_FORMAT_R16_TYPELESS:
			case DXGI_FORMAT_R16_FLOAT:
			case DXGI_FORMAT_D16_UNORM:
			case DXGI_FORMAT_R16_UNORM:
			case DXGI_FORMAT_R16_UINT:
			case DXGI_FORMAT_R16_SNORM:
			case DXGI_FORMAT_R16_SINT:
			case DXGI_FORMAT_B5G6R5_UNORM:
			case DXGI_FORMAT_B5G5R5A1_UNORM:
				return 2;

			case DXGI_FORMAT_R8_TYPELESS:
			case DXGI_FORMAT_R8_UNORM:
			case DXGI_FORMAT_R8_UINT:
			case DXGI_FORMAT_R8_SNORM:
			case DXGI_FORMAT_R8_SINT:
			case DXGI_FORMAT_A8_UNORM:
				return 1;

			case DXGI_FORMAT_BC2_TYPELESS:
			case DXGI_FORMAT_BC2_UNORM:
			case DXGI_FORMAT_BC2_UNORM_SRGB:
			case DXGI_FORMAT_BC3_TYPELESS:
			case DXGI_FORMAT_BC3_UNORM:
			case DXGI_FORMAT_BC3_UNORM_SRGB:
			case DXGI_FORMAT_BC5_TYPELESS:
			case DXGI_FORMAT_BC5_UNORM:
			case DXGI_FORMAT_BC5_SNORM:
				return 16;

			case DXGI_FORMAT_R1_UNORM:
			case DXGI_FORMAT_BC1_TYPELESS:
			case DXGI_FORMAT_BC1_UNORM:
			case DXGI_FORMAT_BC1_UNORM_SRGB:
			case DXGI_FORMAT_BC4_TYPELESS:
			case DXGI_FORMAT_BC4_UNORM:
			case DXGI_FORMAT_BC4_SNORM:
				return 8;

			case DXGI_FORMAT_R9G9B9E5_SHAREDEXP:
				return 4;

			case DXGI_FORMAT_R8G8_B8G8_UNORM:
			case DXGI_FORMAT_G8R8_G8B8_UNORM:
				return 4;

			default: // Including DXGI_FORMAT_UNKNOWN
				throw LosgapException { "Can not get byte size of unknown format." };
		}
	}
#pragma endregion
}