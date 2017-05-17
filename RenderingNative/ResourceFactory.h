// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created by Ben Bowen

#pragma once
#include "../CoreNative/LosgapCore.h"

#include <d3d11.h>

namespace losgap {
	/*
	A static class that exports D3D11 resource and resource view creation methods.
	*/
	class ResourceFactory {
	public:
		static const DXGI_FORMAT INDEX_BUFFER_FORMAT = DXGI_FORMAT::DXGI_FORMAT_R32_UINT;

#pragma region Resource View Creation
		static ID3D11RenderTargetView* CreateRTV(ID3D11Device* devicePtr, ID3D11Texture2D* resPtr, uint32_t mipIndex, DXGI_FORMAT format, bool isMS);
		static ID3D11RenderTargetView* CreateRTVToTexArr(ID3D11Device* devicePtr, ID3D11Texture2D* resPtr, uint32_t mipIndex, uint32_t arrIndex, DXGI_FORMAT format, bool isMS);

		static ID3D11DepthStencilView* CreateDSV(ID3D11Device* devicePtr, ID3D11Texture2D* resPtr, uint32_t mipIndex, DXGI_FORMAT format, bool isMS);
		static ID3D11DepthStencilView* CreateDSVToTexArr(ID3D11Device* devicePtr, ID3D11Texture2D* resPtr, uint32_t mipIndex, uint32_t arrIndex, DXGI_FORMAT format, bool isMS);

		static ID3D11ShaderResourceView* CreateSRVToBuffer(ID3D11Device* devicePtr, ID3D11Buffer* resPtr, 
			DXGI_FORMAT format, UINT firstElement, UINT numElements);
		static ID3D11ShaderResourceView* CreateSRVToRawBuffer(ID3D11Device* devicePtr, ID3D11Buffer* resPtr,
			UINT firstElement, UINT numElements);

		static ID3D11ShaderResourceView* CreateSRVToTexture1D(ID3D11Device* devicePtr, ID3D11Texture1D* resPtr, 
			DXGI_FORMAT format, UINT firstMipIndex, UINT numMips);
		static ID3D11ShaderResourceView* CreateSRVToTexture1DArray(ID3D11Device* devicePtr, ID3D11Texture1D* resPtr, 
			DXGI_FORMAT format, UINT firstMipIndex, UINT numMips, UINT firstElementIndex, UINT numElements);

		static ID3D11ShaderResourceView* CreateSRVToTexture2D(ID3D11Device* devicePtr, ID3D11Texture2D* resPtr,
			DXGI_FORMAT format, UINT firstMipIndex, UINT numMips);
		static ID3D11ShaderResourceView* CreateSRVToTexture2DArray(ID3D11Device* devicePtr, ID3D11Texture2D* resPtr,
			DXGI_FORMAT format, UINT firstMipIndex, UINT numMips, UINT firstElementIndex, UINT numElements);
		static ID3D11ShaderResourceView* CreateSRVToTexture2DMS(ID3D11Device* devicePtr, ID3D11Texture2D* resPtr,
			DXGI_FORMAT format);
		static ID3D11ShaderResourceView* CreateSRVToTexture2DMSArray(ID3D11Device* devicePtr, ID3D11Texture2D* resPtr,
			DXGI_FORMAT format, UINT firstElementIndex, UINT numElements);
		static ID3D11ShaderResourceView* CreateSRVToTexture2DCube(ID3D11Device* devicePtr, ID3D11Texture2D* resPtr,
			DXGI_FORMAT format, UINT firstMipIndex, UINT numMips);
		static ID3D11ShaderResourceView* CreateSRVToTexture2DCubeArray(ID3D11Device* devicePtr, ID3D11Texture2D* resPtr,
			DXGI_FORMAT format, UINT firstMipIndex, UINT numMips, UINT firstFaceIndex, UINT numCubes);

		static ID3D11ShaderResourceView* CreateSRVToTexture3D(ID3D11Device* devicePtr, ID3D11Texture3D* resPtr,
			DXGI_FORMAT format, UINT firstMipIndex, UINT numMips);
		
		static ID3D11UnorderedAccessView* CreateUAVToBuffer(ID3D11Device* devicePtr, ID3D11Buffer* resPtr,
			DXGI_FORMAT format, UINT firstElement, UINT numElements, bool allowAppendConsume, bool includeCounter);
		static ID3D11UnorderedAccessView* CreateUAVToRawBuffer(ID3D11Device* devicePtr, ID3D11Buffer* resPtr,
			UINT firstElement, UINT numElements, bool allowAppendConsume, bool includeCounter);

		static ID3D11UnorderedAccessView* CreateUAVToTexture1D(ID3D11Device* devicePtr, ID3D11Texture1D* resPtr,
			DXGI_FORMAT format, UINT mipIndex);
		static ID3D11UnorderedAccessView* CreateUAVToTexture1DArray(ID3D11Device* devicePtr, ID3D11Texture1D* resPtr,
			DXGI_FORMAT format, UINT mipIndex, UINT firstElementIndex, UINT numElements);
		static ID3D11UnorderedAccessView* CreateUAVToTexture2D(ID3D11Device* devicePtr, ID3D11Texture2D* resPtr,
			DXGI_FORMAT format, UINT mipIndex);
		static ID3D11UnorderedAccessView* CreateUAVToTexture2DArray(ID3D11Device* devicePtr, ID3D11Texture2D* resPtr,
			DXGI_FORMAT format, UINT mipIndex, UINT firstElementIndex, UINT numElements);
		static ID3D11UnorderedAccessView* CreateUAVToTexture3D(ID3D11Device* devicePtr, ID3D11Texture3D* resPtr,
			DXGI_FORMAT format, UINT mipIndex, UINT firstWCoordOffset, UINT wCoordRange);
#pragma endregion

#pragma region Resource Creation
		static ID3D11Buffer* CreateVertexBuffer(ID3D11Device* devicePtr, UINT vertexStructSizeBytes, UINT numVertices,
			D3D11_USAGE usage, D3D11_CPU_ACCESS_FLAG cpuUsage, D3D11_SUBRESOURCE_DATA* initialDataPtr);
		static ID3D11Buffer* CreateIndexBuffer(ID3D11Device* devicePtr, UINT numIndices,
			D3D11_USAGE usage, D3D11_CPU_ACCESS_FLAG cpuUsage, D3D11_SUBRESOURCE_DATA* initialDataPtr);
		static ID3D11Buffer* CreateConstantBuffer(ID3D11Device* devicePtr, UINT numBytes,
			D3D11_USAGE usage, D3D11_CPU_ACCESS_FLAG cpuUsage, D3D11_SUBRESOURCE_DATA* initialDataPtr);
		static ID3D11Buffer* CreateBuffer(ID3D11Device* devicePtr, UINT elementSizeBytes, UINT numElements,
			D3D11_USAGE usage, D3D11_CPU_ACCESS_FLAG cpuUsage, D3D11_BIND_FLAG pipelineBindings, bool isStructured, 
			bool allowRawAccess, D3D11_SUBRESOURCE_DATA* initialDataPtr);

		static ID3D11Texture1D* CreateTexture1DArray(ID3D11Device* devicePtr, UINT widthTx, UINT numTextures,
			bool allocateMipmaps, DXGI_FORMAT format, D3D11_USAGE usage, D3D11_CPU_ACCESS_FLAG cpuUsage,
			D3D11_BIND_FLAG pipelineBindings, bool allowMipGeneration, bool allowLODClamping, D3D11_SUBRESOURCE_DATA* initialDataPtr);

		static ID3D11Texture2D* CreateTexture2DArray(ID3D11Device* devicePtr, UINT widthTx, UINT heightTx, UINT numTextures,
			bool allocateMipmaps, bool multisampled, DXGI_FORMAT format, D3D11_USAGE usage, D3D11_CPU_ACCESS_FLAG cpuUsage,
			D3D11_BIND_FLAG pipelineBindings, bool allowMipGeneration, bool allowLODClamping, D3D11_SUBRESOURCE_DATA* initialDataPtr);
		static ID3D11Texture2D* CreateTexture2DCubeArray(ID3D11Device* devicePtr, UINT widthTx, UINT heightTx, UINT numCubes,
			bool allocateMipmaps, bool multisampled, DXGI_FORMAT format, D3D11_USAGE usage, D3D11_CPU_ACCESS_FLAG cpuUsage,
			D3D11_BIND_FLAG pipelineBindings, bool allowMipGeneration, bool allowLODClamping, D3D11_SUBRESOURCE_DATA* initialDataPtr);
		static ID3D11Texture2D* LoadTexture2D(ID3D11Device* devicePtr, LosgapString filePath, bool allocateMipmaps,
			D3D11_USAGE usage, D3D11_CPU_ACCESS_FLAG cpuUsage, D3D11_BIND_FLAG pipelineBindings, bool allowMipGeneration, bool allowLODClamping);

		static ID3D11Texture3D* CreateTexture3D(ID3D11Device* devicePtr, UINT widthTx, UINT heightTx, UINT depthTx,
			bool allocateMipmaps, DXGI_FORMAT format, D3D11_USAGE usage, D3D11_CPU_ACCESS_FLAG cpuUsage,
			D3D11_BIND_FLAG pipelineBindings, bool allowMipGeneration, bool allowLODClamping, D3D11_SUBRESOURCE_DATA* initialDataPtr);

		static ID3D11SamplerState* CreateSSO(ID3D11Device* devicePtr, D3D11_FILTER textureFilterType,
			D3D11_TEXTURE_ADDRESS_MODE textureAddressingMode, UINT maxAnisotropy,
			FLOAT borderColorR, FLOAT borderColorG, FLOAT borderColorB, FLOAT borderColorA);

		static ID3D11RasterizerState* CreateRSState(ID3D11Device* devicePtr, D3D11_FILL_MODE fillMode, D3D11_CULL_MODE cullMode,
			bool flipFaces, int depthBias, float depthBiasClamp, float slopeScaledDepthBias, bool enableZClipping);

		static ID3D11DepthStencilState* CreateDSState(ID3D11Device* devicePtr, bool depthEnable, D3D11_COMPARISON_FUNC comparisonFunc);
		
		static ID3D11BlendState* CreateBlendState(ID3D11Device* devicePtr, bool enableBlending, D3D11_BLEND_OP blendOp);
#pragma endregion

#pragma region Resource Manipulations
		static void CopyResource(ID3D11DeviceContext* contextPtr, ID3D11Resource* srcResPtr, ID3D11Resource* dstResPtr);
		
		static D3D11_MAPPED_SUBRESOURCE MapSubresource(ID3D11DeviceContext* contextPtr, ID3D11Resource* resourcePtr,
			UINT subresourceIndex, D3D11_MAP mapType);
		static void UnmapSubresource(ID3D11DeviceContext* contextPtr, ID3D11Resource* resourcePtr, UINT subresourceIndex);

		static void UpdateSubresourceRegion(ID3D11DeviceContext* contextPtr, ID3D11Resource* resourcePtr, UINT subresourceIndex, 
			const D3D11_BOX* subBox, const void* data, UINT dataRowDistanceBytes, UINT dataSliceDistanceBytes);

		static void CopySubresourceRegion(ID3D11DeviceContext* contextPtr,
			ID3D11Resource* srcResPtr, UINT srcSubresourceIndex, const D3D11_BOX* srcSubBox,
			ID3D11Resource* dstResPtr, UINT dstSubresourceIndex, UINT dstX, UINT dstY, UINT dstZ);

		static void GenerateMips(ID3D11DeviceContext* contextPtr, ID3D11ShaderResourceView* srvToTexturePtr);
#pragma endregion

		static void ReleaseResourceOrView(IUnknown* resourceOrViewPtr);
		static void SetTextureQualityOffset(ID3D11DeviceContext* contextPtr, ID3D11Resource* texturePtr, UINT qualityReductionLevel);

		static UINT GetNumMips(ID3D11Texture1D* texturePtr);
		static UINT GetNumMips(ID3D11Texture2D* texturePtr);
		static UINT GetNumMips(ID3D11Texture3D* texturePtr);

		// Not exported
		static UINT GetFormatByteSize(DXGI_FORMAT format);
	};
}