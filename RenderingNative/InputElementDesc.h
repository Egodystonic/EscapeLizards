// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created by Ben Bowen

#pragma once
#include "../CoreNative/LosgapCore.h"
#include <d3d11.h>

namespace losgap {
	/*
	A simplified input element descriptor, used for interop
	*/
#pragma pack(push, STRUCT_PACKING_SAFE)
	struct InputElementDesc {
		INTEROP_STRING SemanticName;
		uint32_t SemanticIndex;
		DXGI_FORMAT ElementFormat;
		uint32_t InputSlot;
		INTEROP_BOOL IsPerVertexData;
	};
#pragma pack(pop)
}