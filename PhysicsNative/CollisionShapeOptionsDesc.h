// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created by Ben Bowen

#pragma once
#include "../CoreNative/LosgapCore.h"
#include "btBulletDynamicsCommon.h"

namespace losgap {
	/*
	An interop struct detailing collision shape options
	*/
#pragma pack(push, STRUCT_PACKING_SAFE)
	struct CollisionShapeOptionsDesc {
		btVector3 Scaling;

		CollisionShapeOptionsDesc() : Scaling(btVector3 { 1.0f, 1.0f, 1.0f }) { }
	};
#pragma pack(pop)
}