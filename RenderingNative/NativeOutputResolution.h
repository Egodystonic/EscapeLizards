// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created by Ben Bowen

#pragma once
#include "../CoreNative/LosgapCore.h"

#include <cstdint>

namespace losgap {
#pragma pack(push, STRUCT_PACKING_SAFE)
	struct NativeOutputResolution {
		int32_t Index;
		
		uint32_t ResX, ResY;
		uint32_t RefreshRateNumerator;
		uint32_t RefreshRateDenominator;
	};
#pragma pack(pop)
}