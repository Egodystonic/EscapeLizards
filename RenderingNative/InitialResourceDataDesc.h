// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created by Ben Bowen

#pragma once
#include "../CoreNative/LosgapCore.h"

namespace losgap {
#pragma pack (push, STRUCT_PACKING_SAFE)
	/*
	A struct that maps in a controlled way on to a D3D11_SUBRESOURCE_DESC.
	*/
	struct InitialResourceDataDesc {
		void* Data;
		uint32_t DataRowDistanceBytes;
		uint32_t DataSliceDistanceBytes;
	};
#pragma pack (pop)
}