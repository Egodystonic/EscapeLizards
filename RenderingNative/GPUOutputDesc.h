// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created by Ben Bowen

#pragma once
#include "../CoreNative/LosgapCore.h"

#include "NativeOutputResolution.h"

namespace losgap {
#pragma pack(push, STRUCT_PACKING_SAFE)
	struct GPUOutputDesc {
		int32_t Index;
		INTEROP_STRING Name;

		INTEROP_BOOL IsPrimaryOutput;

		int32_t NumNativeResolutions;
		NativeOutputResolution* NativeResolutions;
	};
#pragma pack(pop)
}