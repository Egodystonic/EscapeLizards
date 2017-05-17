// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created by Ben Bowen

#pragma once
#include "../CoreNative/LosgapCore.h"

#include "GPUOutputDesc.h"

namespace losgap {
#pragma pack(push, STRUCT_PACKING_SAFE)
	struct GPUDesc {
		int32_t Index;
		INTEROP_STRING Name;

		int64_t DedicatedVideoMemory;
		int64_t DedicatedSystemMemory;
		int64_t SharedSystemMemory;

		int32_t NumOutputs;
		GPUOutputDesc* Outputs;
	};
#pragma pack(pop)
}