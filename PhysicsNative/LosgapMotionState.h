// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created by Ben Bowen

#pragma once
#include "../CoreNative/LosgapCore.h"
#include "btBulletDynamicsCommon.h"


namespace losgap {
	/*
		A custom motion state that gets/sets world transform from the C# side
	*/
	class LosgapMotionState : public btMotionState {
	private:
		btVector3* const translationPtr;
		btQuaternion* const rotationPtr;
		btVector3* const translationOffsetPtr;

	public:
		LosgapMotionState(btVector3* translationPtr, btQuaternion* rotationPtr, btVector3* translationOffsetPtr);

		void getWorldTransform(btTransform& refTrans) const override;
		void setWorldTransform(const btTransform& trans) override;
	};
}