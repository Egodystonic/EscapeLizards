// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created by Ben Bowen

#include "ShaderManager.h"
#include <map>
#include <mutex>
#include <fstream>

namespace losgap {
	std::mutex VS_BINARY_MAP_MUTEX { };
	std::map<ID3D11VertexShader*, const void*> VERTEX_SHADER_BINARIES { };
	std::map<ID3D11VertexShader*, UINT> VERTEX_SHADER_BINARY_SIZES { };

	ID3D11DeviceChild* ShaderManager::LoadShader(ID3D11Device* devicePtr, LosgapString shaderFilePath, ShaderType shaderType) {
		if (devicePtr == nullptr) throw LosgapException { "Device pointer must not be null." };
		std::ifstream shaderFileInputStream { LosgapString::AsNewCString(shaderFilePath).get(), std::ios_base::in | std::ios_base::binary };
		if (!shaderFileInputStream.is_open() || shaderFileInputStream.bad()) {
			throw LosgapException { "Failed to open shader '" + shaderFilePath + "': Could not open stream." };
		}

		shaderFileInputStream.seekg(0, std::ios_base::end);
		SIZE_T fileLen = static_cast<SIZE_T>(shaderFileInputStream.tellg());
		shaderFileInputStream.seekg(0, std::ios_base::beg);
		const void* shaderBinary = new char[fileLen];
		shaderFileInputStream.read(const_cast<char*>(static_cast<const char*>(shaderBinary)), static_cast<std::streamsize>(fileLen));
		shaderFileInputStream.close();

		ID3D11DeviceChild* result = nullptr;

		switch (shaderType) {
			case ShaderType::Vertex: {
				ID3D11VertexShader* resultAsVS = static_cast<ID3D11VertexShader*>(result);
				CHECK_CALL(devicePtr->CreateVertexShader(shaderBinary, fileLen, nullptr, &resultAsVS));
				VS_BINARY_MAP_MUTEX.lock();
				VERTEX_SHADER_BINARIES.emplace(resultAsVS, shaderBinary);
				VERTEX_SHADER_BINARY_SIZES.emplace(resultAsVS, static_cast<UINT>(fileLen));
				VS_BINARY_MAP_MUTEX.unlock();
				result = static_cast<ID3D11DeviceChild*>(resultAsVS);
				break;
			}
			case ShaderType::Fragment: {
				ID3D11PixelShader* resultAsPS = static_cast<ID3D11PixelShader*>(result);
				CHECK_CALL(devicePtr->CreatePixelShader(shaderBinary, fileLen, nullptr, &resultAsPS));
				delete[] shaderBinary;
				result = static_cast<ID3D11DeviceChild*>(resultAsPS);
				break;
			}
			default: {
				delete[] shaderBinary;
				throw LosgapException { "Unknown shader type: " + std::to_string(shaderType) + "." };
				break;
			}
		}

		return result;
	}
	EXPORT(ShaderManager_LoadShader, ID3D11Device* devicePtr, INTEROP_STRING shaderFilePath, 
		ShaderType shaderType, ID3D11DeviceChild** outShaderPtr) {
		*outShaderPtr = ShaderManager::LoadShader(devicePtr, shaderFilePath, shaderType);
		EXPORT_END;
	}

	void ShaderManager::UnloadShader(ID3D11DeviceChild* shader, ShaderType shaderType) {
		if (shader == nullptr) throw LosgapException { "Can not unload null shader." };
		if (shaderType == ShaderType::Vertex) {
			VS_BINARY_MAP_MUTEX.lock();
			VERTEX_SHADER_BINARIES.erase(static_cast<ID3D11VertexShader*>(shader));
			VERTEX_SHADER_BINARY_SIZES.erase(static_cast<ID3D11VertexShader*>(shader));
			VS_BINARY_MAP_MUTEX.unlock();
		}
		shader->Release();
	}
	EXPORT(ShaderManager_UnloadShader, ID3D11DeviceChild* shaderPtr, ShaderType shaderType) {
		ShaderManager::UnloadShader(shaderPtr, shaderType);
		EXPORT_END;
	}

	ID3D11InputLayout* ShaderManager::CreateInputLayout(ID3D11Device* devicePtr, ID3D11VertexShader* shaderPtr,
		InputElementDesc* inputElementDescArr, uint32_t inputElementDescArrLen) {
		if (devicePtr == nullptr) throw LosgapException { "Device pointer must not be null." };
		if (shaderPtr == nullptr) throw LosgapException { "Shader pointer must not be null." };
		D3D11_INPUT_ELEMENT_DESC* inputElements = new D3D11_INPUT_ELEMENT_DESC[static_cast<size_t>(inputElementDescArrLen)];
		std::unique_ptr<const char>* convertedNameArr = new std::unique_ptr<const char>[static_cast<size_t>(inputElementDescArrLen)];
		for (uint32_t i = 0; i < inputElementDescArrLen; ++i) {
			inputElements[i].Format = inputElementDescArr[i].ElementFormat;
			inputElements[i].InputSlot = inputElementDescArr[i].InputSlot;
			if (inputElementDescArr[i].IsPerVertexData) {
				inputElements[i].InputSlotClass = D3D11_INPUT_CLASSIFICATION::D3D11_INPUT_PER_VERTEX_DATA;
				inputElements[i].InstanceDataStepRate = 0U;
			}
			else {
				inputElements[i].InputSlotClass = D3D11_INPUT_CLASSIFICATION::D3D11_INPUT_PER_INSTANCE_DATA;
				inputElements[i].InstanceDataStepRate = 1U;
			}
			inputElements[i].AlignedByteOffset = D3D11_APPEND_ALIGNED_ELEMENT;
			inputElements[i].SemanticIndex = inputElementDescArr[i].SemanticIndex;
			convertedNameArr[i] = LosgapString::AsNewCString(inputElementDescArr[i].SemanticName);
			inputElements[i].SemanticName = convertedNameArr[i].get();
		}

		VS_BINARY_MAP_MUTEX.lock();
		if (VERTEX_SHADER_BINARIES.find(shaderPtr) == VERTEX_SHADER_BINARIES.end()
			|| VERTEX_SHADER_BINARY_SIZES.find(shaderPtr) == VERTEX_SHADER_BINARY_SIZES.end()) {
			VS_BINARY_MAP_MUTEX.unlock();
			throw LosgapException { "Could not find shader binary or binary size for given vertex shader pointer." };
		}
		const void* shaderBinary = VERTEX_SHADER_BINARIES.at(shaderPtr);
		UINT binarySize = VERTEX_SHADER_BINARY_SIZES.at(shaderPtr);
		VS_BINARY_MAP_MUTEX.unlock();

		ID3D11InputLayout* outResult;

		HRESULT layoutCreationResult = devicePtr->CreateInputLayout(
			inputElements,
			inputElementDescArrLen,
			shaderBinary,
			binarySize,
			&outResult
		);

		delete[] inputElements;
		delete[] convertedNameArr;

		CHECK_CALL(layoutCreationResult);

		return outResult;
	}
	EXPORT(ShaderManager_CreateInputLayout, ID3D11Device* devicePtr, ID3D11VertexShader* shaderPtr,
		InputElementDesc* inputElementDescArr, uint32_t inputElementDescArrLen, ID3D11InputLayout** outInputLayoutHandle) {
		*outInputLayoutHandle = ShaderManager::CreateInputLayout(devicePtr, shaderPtr, inputElementDescArr, inputElementDescArrLen);
		EXPORT_END;
	}
}