// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created by Ben Bowen

#pragma once
#include "../CoreNative/LosgapCore.h"
#include "RenderCommandInstruction.h"
#include <d3d11.h>

namespace losgap {
	/*
	An interop struct detailing a single render command
	*/
#pragma pack(push, STRUCT_PACKING_SAFE)
	struct RenderCommand {
		RenderCommandInstruction Instruction;
		uint64_t Arg1;
		uint64_t Arg2;
		uint64_t Arg3;
	};
#pragma pack(pop)
}