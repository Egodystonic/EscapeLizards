// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created by Ben Bowen

#pragma once
#include "../CoreNative/LosgapCore.h"

namespace losgap {
	/*
	A struct used as a more packing-controlled version of D3D11_BOX
	*/
#pragma pack (push, STRUCT_PACKING_SAFE)
	struct SubresourceBox {
		uint32_t left, right, top, bottom, front, back;
	};
#pragma pack (pop)
}