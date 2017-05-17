// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created by Ben Bowen

#pragma once
#include "../CoreNative/LosgapCore.h"
#include "btBulletDynamicsCommon.h"

namespace losgap {
	/*
	An interop struct detailing a collision on a raycast
	*/
#pragma pack(push, STRUCT_PACKING_SAFE)
	struct RayTestCollisionDesc {
		char hitPosition[sizeof(btVector3)];
		btRigidBody* hitBody;
		
		RayTestCollisionDesc(btRigidBody* body, btVector3& position)
			: hitBody(body) {
			memcpy(hitPosition, ((char*) &position), sizeof(btVector3));
		}
	};
#pragma pack(pop)
}