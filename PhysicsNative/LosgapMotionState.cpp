// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created by Ben Bowen

#include "LosgapMotionState.h"

namespace losgap {
	LosgapMotionState::LosgapMotionState(btVector3* translationPtr, btQuaternion* rotationPtr, btVector3* translationOffsetPtr) :
		translationPtr(translationPtr),
		rotationPtr(rotationPtr),
		translationOffsetPtr(translationOffsetPtr) { }

	void LosgapMotionState::getWorldTransform(btTransform& refTrans) const { 
		refTrans.setOrigin(*translationPtr + *translationOffsetPtr);
		refTrans.setRotation(*rotationPtr);
	}
	void LosgapMotionState::setWorldTransform(const btTransform& trans) {
		*translationPtr = trans.getOrigin() - *translationOffsetPtr;
		*rotationPtr = trans.getRotation();
	}
}