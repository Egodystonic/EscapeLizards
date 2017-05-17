// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created by Ben Bowen

#include "ResourceFactory.h"
#include "RenderPassManager.h"
#include <DirectXMath.h>

#define CAST(arg, type) *reinterpret_cast<type*>(&arg)

namespace losgap {
#pragma region Instructions: Set Pipeline State
	void Instr_SetPrimitiveTopology(ID3D11DeviceContext* deviceContextPtr, D3D11_PRIMITIVE_TOPOLOGY topology) {
		deviceContextPtr->IASetPrimitiveTopology(topology);
	}

	void Instr_SetInputLayout(ID3D11DeviceContext* deviceContextPtr, ID3D11InputLayout* layout) {
		deviceContextPtr->IASetInputLayout(layout);
	}

	void Instr_SetShader(RenderCommandInstruction instruction, ID3D11DeviceContext* deviceContextPtr, uint64_t shaderPtr) {
		switch (instruction) {
			case FSSetShader:
				deviceContextPtr->PSSetShader(CAST(shaderPtr, ID3D11PixelShader*), nullptr, 0U);
				break;
			case VSSetShader:
				deviceContextPtr->VSSetShader(CAST(shaderPtr, ID3D11VertexShader*), nullptr, 0U);
				break;
		}
	}

	void Instr_SetRSState(ID3D11DeviceContext* deviceContextPtr, ID3D11RasterizerState* rs) {
		deviceContextPtr->RSSetState(rs);
	}

	void Instr_SetDSState(ID3D11DeviceContext* deviceContextPtr, ID3D11DepthStencilState* ds) {
		deviceContextPtr->OMSetDepthStencilState(ds, 0xFF);
	}

	void Instr_SetBlendState(ID3D11DeviceContext* deviceContextPtr, ID3D11BlendState* bs) {
		deviceContextPtr->OMSetBlendState(bs, nullptr, 0xFFFFFFFF);
	}

	void Instr_SetViewport(ID3D11DeviceContext* deviceContextPtr, D3D11_VIEWPORT* vp) {
		deviceContextPtr->RSSetViewports(1U, vp);

		D3D11_RECT scissorRect;
		scissorRect.left = 0U;
		scissorRect.right = static_cast<LONG>(vp->Width);
		scissorRect.top = 0U;
		scissorRect.bottom = static_cast<LONG>(vp->Height);

		deviceContextPtr->RSSetScissorRects(1U, &scissorRect);
	}

	void Instr_SetRenderTargets(ID3D11DeviceContext* deviceContextPtr, ID3D11RenderTargetView** rtvPtrArr, ID3D11DepthStencilView* dsvPtr, uint32_t numRTVs) {
		deviceContextPtr->OMSetRenderTargets(numRTVs, rtvPtrArr, dsvPtr);
	}
#pragma endregion
#pragma region Instructions: Set Resources
	// Objects with static storage duration (3.7.1) shall be zero-initialized (8.5) before any other initialization takes place.
	static const uint32_t SET_VBUF_OFFSET_ARR[D3D11_VS_INPUT_REGISTER_COUNT];

	void Instr_SetVertexBuffers(ID3D11DeviceContext* deviceContextPtr, ID3D11Buffer** vBufferArr, uint32_t* strideArr, uint32_t numBuffers) {
		deviceContextPtr->IASetVertexBuffers(
			0U,
			numBuffers,
			vBufferArr,
			strideArr,
			SET_VBUF_OFFSET_ARR
			);
	}

	void Instr_SetIndexBuffer(ID3D11DeviceContext* deviceContextPtr, ID3D11Buffer* indexBufferPtr) {
		deviceContextPtr->IASetIndexBuffer(indexBufferPtr, ResourceFactory::INDEX_BUFFER_FORMAT, 0U);
	}

	void Instr_SetInstanceBuffer(ID3D11DeviceContext* deviceContextPtr, ID3D11Buffer* instanceBufferPtr,
		uint32_t slotIndex) {
		uint32_t stride = 64U; // Vector4 size (4x4 float == 4x4x4 == 64)
		uint32_t offset = 0U;
		deviceContextPtr->IASetVertexBuffers(
			slotIndex,
			1U,
			&instanceBufferPtr,
			&stride,
			&offset
			);
	}

	void Instr_SetCBuffers(RenderCommandInstruction instruction, ID3D11DeviceContext* deviceContextPtr,
		ID3D11Buffer** cBufferArr, uint32_t numBuffers, uint32_t startSlot) {
		switch (instruction) {
			case FSSetCBuffers:
				deviceContextPtr->PSSetConstantBuffers(startSlot, numBuffers, cBufferArr);
				break;
			case VSSetCBuffers:
				deviceContextPtr->VSSetConstantBuffers(startSlot, numBuffers, cBufferArr);
				break;
		}
	}

	void Instr_SetSamplers(RenderCommandInstruction instruction, ID3D11DeviceContext* deviceContextPtr,
		ID3D11SamplerState** samplerArr, uint32_t numSamplers, uint32_t startSlot) {
		switch (instruction) {
			case FSSetSamplers:
				deviceContextPtr->PSSetSamplers(startSlot, numSamplers, samplerArr);
				break;
			case VSSetSamplers:
				deviceContextPtr->VSSetSamplers(startSlot, numSamplers, samplerArr);
				break;
		}
	}

	void Instr_SetResources(RenderCommandInstruction instruction, ID3D11DeviceContext* deviceContextPtr,
		ID3D11ShaderResourceView** srvArr, uint32_t numViews, uint32_t startSlot) {
		switch (instruction) {
			case FSSetResources:
				deviceContextPtr->PSSetShaderResources(startSlot, numViews, srvArr);
				break;
			case VSSetSamplers:
				deviceContextPtr->VSSetShaderResources(startSlot, numViews, srvArr);
				break;
		}
	}

	void Instr_CBDiscardWrite(ID3D11DeviceContext* deviceContextPtr,
		ID3D11Buffer* cBufferPtr, void* dataPtr, uint32_t numBytes) {
		D3D11_MAPPED_SUBRESOURCE outMappedSubresource;
		CHECK_CALL(deviceContextPtr->Map(cBufferPtr, 0U, D3D11_MAP::D3D11_MAP_WRITE_DISCARD, 0U, &outMappedSubresource));
		memcpy(outMappedSubresource.pData, dataPtr, numBytes);
		deviceContextPtr->Unmap(cBufferPtr, 0U);
	}

	void Instr_BufferWrite(ID3D11DeviceContext* deviceContextPtr,
		ID3D11Buffer* bufferPtr, void* dataPtr, uint32_t numBytes) {
		D3D11_MAPPED_SUBRESOURCE outMappedSubresource;
		CHECK_CALL(deviceContextPtr->Map(bufferPtr, 0U, D3D11_MAP::D3D11_MAP_WRITE_DISCARD, 0U, &outMappedSubresource));
		memcpy(outMappedSubresource.pData, dataPtr, numBytes);
		deviceContextPtr->Unmap(bufferPtr, 0U);
	}
#pragma endregion
#pragma region Instructions: Dispatch
	static const FLOAT BACK_BUFFER_CLEAR_COLOR[4] = { 0.0f, 0.0f, 0.0f, 0.0f };

	void Instr_DrawIndexedInstanced(ID3D11DeviceContext* deviceContextPtr,
		int32_t firstVertexIndex, uint32_t firstIndexIndex, uint32_t numIndices, uint32_t firstInstanceIndex, uint32_t numInstances) {
		deviceContextPtr->DrawIndexedInstanced(numIndices, numInstances, firstIndexIndex, firstVertexIndex, firstInstanceIndex);
	}

	void Instr_Draw(ID3D11DeviceContext* deviceContextPtr, int32_t firstVertexIndex, uint32_t numVertices) {
		deviceContextPtr->Draw(numVertices, firstVertexIndex);
	}

	void Instr_ClearRenderTarget(ID3D11DeviceContext* deviceContextPtr, ID3D11RenderTargetView* rtv) {
		deviceContextPtr->ClearRenderTargetView(rtv, BACK_BUFFER_CLEAR_COLOR);
	}

	void Instr_ClearDepthStencil(ID3D11DeviceContext* deviceContextPtr, ID3D11DepthStencilView* dsv) {
		deviceContextPtr->ClearDepthStencilView(dsv, D3D11_CLEAR_DEPTH | D3D11_CLEAR_STENCIL, 1.0f, 0xFF);
	}

	void Instr_SwapChainPresent(ID3D11DeviceContext* deviceContextPtr, IDXGISwapChain* swapChainPtr) {
		RenderPassManager::PresentBackBuffer(swapChainPtr);
	}

	void Instr_FinishCommandList(ID3D11DeviceContext* deferredContextPtr, ID3D11CommandList** outCommandListPtr) {
		deferredContextPtr->FinishCommandList(FALSE, outCommandListPtr);
	}
#pragma endregion

#pragma region Instruction Switch
	void SwitchOverCommand(ID3D11DeviceContext* deviceContextPtr, RenderCommand command) {
		switch (command.Instruction) { // Ordered roughly by frequency
			case RenderCommandInstruction::DrawIndexedInstanced: {
				uint32_t* arg23AsUInt = reinterpret_cast<uint32_t*>(&command.Arg2);
				uint32_t* arg45AsUInt = reinterpret_cast<uint32_t*>(&command.Arg3);

				Instr_DrawIndexedInstanced(
					deviceContextPtr,
					CAST(command.Arg1, int32_t),
					arg23AsUInt[0],
					arg23AsUInt[1],
					arg45AsUInt[0],
					arg45AsUInt[1]
					);
				break;
			}
			case RenderCommandInstruction::Draw: {
				Instr_Draw(
					deviceContextPtr,
					CAST(command.Arg1, int32_t),
					CAST(command.Arg2, int32_t)
				);
				break;
			}
			case RenderCommandInstruction::VSSetCBuffers:
			case RenderCommandInstruction::FSSetCBuffers: {
				Instr_SetCBuffers(
					command.Instruction,
					deviceContextPtr,
					CAST(command.Arg1, ID3D11Buffer**),
					CAST(command.Arg2, uint32_t),
					CAST(command.Arg3, uint32_t)
					);
				break;
			}
			case RenderCommandInstruction::VSSetResources:
			case RenderCommandInstruction::FSSetResources: {
				Instr_SetResources(
					command.Instruction,
					deviceContextPtr,
					CAST(command.Arg1, ID3D11ShaderResourceView**),
					CAST(command.Arg2, uint32_t),
					CAST(command.Arg3, uint32_t)
					);
				break;
			}
			case RenderCommandInstruction::CBDiscardWrite: {
				Instr_CBDiscardWrite(
					deviceContextPtr,
					CAST(command.Arg1, ID3D11Buffer*),
					CAST(command.Arg2, void*),
					CAST(command.Arg3, uint32_t)
					);
				break;
			}
			case RenderCommandInstruction::BufferWrite: {
				Instr_BufferWrite(
					deviceContextPtr,
					CAST(command.Arg1, ID3D11Buffer*),
					CAST(command.Arg2, void*),
					CAST(command.Arg3, uint32_t)
					);
				break;
			}
			case RenderCommandInstruction::VSSetSamplers:
			case RenderCommandInstruction::FSSetSamplers: {
				Instr_SetSamplers(
					command.Instruction,
					deviceContextPtr,
					CAST(command.Arg1, ID3D11SamplerState**),
					CAST(command.Arg2, uint32_t),
					CAST(command.Arg3, uint32_t)
					);
				break;
			}
			case RenderCommandInstruction::VSSetShader:
			case RenderCommandInstruction::FSSetShader: {
				Instr_SetShader(
					command.Instruction,
					deviceContextPtr,
					command.Arg1
					);
				break;
			}
			case RenderCommandInstruction::SetVertexBuffers: {
				Instr_SetVertexBuffers(
					deviceContextPtr,
					CAST(command.Arg1, ID3D11Buffer**),
					CAST(command.Arg2, uint32_t*),
					CAST(command.Arg3, uint32_t)
					);
				break;
			}
			case RenderCommandInstruction::SetIndexBuffer: {
				Instr_SetIndexBuffer(deviceContextPtr, CAST(command.Arg1, ID3D11Buffer*));
				break;
			}
			case RenderCommandInstruction::SetInstanceBuffer: {
				Instr_SetInstanceBuffer(deviceContextPtr, CAST(command.Arg1, ID3D11Buffer*), CAST(command.Arg2, uint32_t));
				break;
			}
			case RenderCommandInstruction::SetInputLayout: {
				Instr_SetInputLayout(deviceContextPtr, CAST(command.Arg1, ID3D11InputLayout*));
				break;
			}
			case RenderCommandInstruction::SetRSState: {
				Instr_SetRSState(deviceContextPtr, CAST(command.Arg1, ID3D11RasterizerState*));
				break;
			}
			case RenderCommandInstruction::SetDSState: {
				Instr_SetDSState(deviceContextPtr, CAST(command.Arg1, ID3D11DepthStencilState*));
				break;
			}
			case RenderCommandInstruction::SetBlendState: {
				Instr_SetBlendState(deviceContextPtr, CAST(command.Arg1, ID3D11BlendState*));
				break;
			}
			case RenderCommandInstruction::SetViewport: {
				Instr_SetViewport(deviceContextPtr, CAST(command.Arg1, D3D11_VIEWPORT*));
				break;
			}
			case RenderCommandInstruction::SetRenderTargets: {
				Instr_SetRenderTargets(
					deviceContextPtr,
					CAST(command.Arg1, ID3D11RenderTargetView**),
					CAST(command.Arg2, ID3D11DepthStencilView*),
					CAST(command.Arg3, uint32_t)
					);
				break;
			}
			case RenderCommandInstruction::ClearRenderTarget: {
				Instr_ClearRenderTarget(deviceContextPtr, CAST(command.Arg1, ID3D11RenderTargetView*));
				break;
			}
			case RenderCommandInstruction::ClearDepthStencil: {
				Instr_ClearDepthStencil(deviceContextPtr, CAST(command.Arg1, ID3D11DepthStencilView*));
				break;
			}
			case RenderCommandInstruction::SwapChainPresent: {
				Instr_SwapChainPresent(deviceContextPtr, CAST(command.Arg1, IDXGISwapChain*));
				break;
			}
			case RenderCommandInstruction::FinishCommandList: {
				Instr_FinishCommandList(deviceContextPtr, CAST(command.Arg1, ID3D11CommandList**));
				break;
			}
			case RenderCommandInstruction::SetPrimitiveTopology: {
				Instr_SetPrimitiveTopology(deviceContextPtr, CAST(command.Arg1, D3D11_PRIMITIVE_TOPOLOGY));
				break;
			}
			case RenderCommandInstruction::NoOperation: {
				break;
			}
			default: {
				throw LosgapException { "Unknown render instruction: " + std::to_string(command.Instruction) };
			}
		}
	}
#pragma endregion

	void RenderPassManager::FlushInstructions(ID3D11DeviceContext* deviceContextPtr, RenderCommand* commandArr, uint32_t commandArrLen) {
		if (deviceContextPtr == nullptr) throw LosgapException { "Device context pointer must not be null!" };
		if (commandArr == nullptr) throw LosgapException { "Render command array pointer must not be null!" };
		for (uint32_t i = 0U; i < commandArrLen; ++i) {
			SwitchOverCommand(deviceContextPtr, commandArr[i]);
		}
	}
	EXPORT(RenderPassManager_FlushInstructions, ID3D11DeviceContext* deviceContextPtr, RenderCommand* commandArr, uint32_t commandArrLen) {
		RenderPassManager::FlushInstructions(deviceContextPtr, commandArr, commandArrLen);
		EXPORT_END;
	}

	void RenderPassManager::ExecuteCommandList(ID3D11DeviceContext* immedContextPtr, ID3D11CommandList* commandListPtr) {
		if (immedContextPtr == nullptr) throw LosgapException { "Immediate device context pointer must not be null!" };
		if (commandListPtr == nullptr) throw LosgapException { "Commnad list pointer must not be null!" };

		immedContextPtr->ExecuteCommandList(commandListPtr, FALSE);
		commandListPtr->Release();
	}
	EXPORT(RenderPassManager_ExecuteCommandList, ID3D11DeviceContext* immedContextPtr, ID3D11CommandList* commandListPtr) {
		RenderPassManager::ExecuteCommandList(immedContextPtr, commandListPtr);
		EXPORT_END;
	}

	void RenderPassManager::PresentBackBuffer(IDXGISwapChain* swapChainPtr) {
		if (swapChainPtr == nullptr) throw LosgapException { "Swap chain pointer must not be null." };
		swapChainPtr->Present(0U, 0U);
	}
	EXPORT(RenderPassManager_PresentBackBuffer, IDXGISwapChain* swapChainPtr) {
		RenderPassManager::PresentBackBuffer(swapChainPtr);
		EXPORT_END;
	}
}