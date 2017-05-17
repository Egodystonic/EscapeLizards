// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created by Ben Bowen

#pragma once
#include "../CoreNative/LosgapCore.h"
#include "ShaderType.h"
#include "InputElementDesc.h"
#include <d3d11.h>

namespace losgap {
	/*
	A static class that loads, unloads, and sets up various shaders
	*/
	class ShaderManager {
	public:
		static ID3D11DeviceChild* LoadShader(ID3D11Device* devicePtr, LosgapString shaderFilePath, ShaderType shaderType);
		static void UnloadShader(ID3D11DeviceChild* shader, ShaderType shaderType);

		static ID3D11InputLayout* CreateInputLayout(ID3D11Device* devicePtr, ID3D11VertexShader* shaderPtr,
			InputElementDesc* inputElementDescArr, uint32_t inputElementDescArrLen);
	};
}