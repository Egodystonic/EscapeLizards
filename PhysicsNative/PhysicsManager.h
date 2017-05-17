// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created by Ben Bowen

#pragma once
#include "..\CoreNative\LosgapCore.h"
#include "btBulletDynamicsCommon.h"
#include "CollisionShapeOptionsDesc.h"
#include "RayTestCollisionDesc.h"

namespace losgap {
	/*
	Physics manager
	*/
	class PhysicsManager {
	public:
#pragma region Lifetime
		static void Init();
		static void Tick(btScalar deltaTime);
		static void SetTickrate(float tickrate);
		static const btCollisionObject** GetCollisionPairsArray(uint32_t& numPairs);
		static void Shutdown();
#pragma endregion

#pragma region World
		static void SetGravity(const btVector3& gravity);
		static btRigidBody* RayTestNearest(const btVector3& rayStart, const btVector3& rayEnd, btVector3* outHitPoint);
		static uint32_t RayTestAll(const btVector3& rayStart, const btVector3& rayEnd, RayTestCollisionDesc* outCollisionDescArr, uint32_t arrLen);
#pragma endregion

#pragma region Shape Creation
		static btBoxShape* CreateBoxShape(const btVector3& halfExtents, const CollisionShapeOptionsDesc& shapeOptions);
		static btSphereShape* CreateSimpleSphereShape(btScalar radius, const CollisionShapeOptionsDesc& shapeOptions);
		static btMultiSphereShape* CreateScaledSphereShape(btScalar radius, const btVector3& scaling, const CollisionShapeOptionsDesc& shapeOptions);
		static btConeShape* CreateConeShape(btScalar radius, btScalar height, const CollisionShapeOptionsDesc& shapeOptions);
		static btCylinderShape* CreateCylinderShape(btScalar radius, btScalar height, const CollisionShapeOptionsDesc& shapeOptions);
		static btConvexHullShape* CreateConvexHullShape(const btScalar* const vertexComponentArr, int numVertices, const CollisionShapeOptionsDesc& shapeOptions);
		static btCompoundShape* CreateConcaveHullShape(const btScalar* const vertexComponentArr, int numVertices, const int* const indices, const int numIndices, const CollisionShapeOptionsDesc& shapeOptions, const char* acdFilePath);
		static btCompoundShape* CreateCompoundCurveShape(const btScalar* const vertexComponentArr, const uint32_t numTrapPrisms, const CollisionShapeOptionsDesc& shapeOptions);
		static void DestroyShape(btCollisionShape* shape);
#pragma endregion

#pragma region Body Creation
		static btRigidBody* CreateRigidBody(btVector3* const translationPtr, btQuaternion* const rotationPtr, btVector3* translationOffsetPtr, btCollisionShape* const collisionShape, btScalar bodyMass, bool alwaysActive, bool forceIntransigence, bool worldColOnly, bool nonWallCol);
		static void SetBodyProperties(btRigidBody* const body, btScalar restitution, btScalar linearDamping, btScalar angularDamping, btScalar friction, btScalar rollingFriction);
		static void SetBodyCCD(btRigidBody* const body, btScalar minSpeed, btScalar ccdRadius);
		static void DestroyRigidBody(btRigidBody* const body);
#pragma endregion

#pragma region Body Manipulation
		static void UpdateBodyTransform(btRigidBody* const body);
		static void AddForceToBody(btRigidBody* const body, const btVector3& force);
		static void AddTorqueToBody(btRigidBody* const body, const btVector3& torque);
		static void AddForceImpulseToBody(btRigidBody* const body, const btVector3& forceImpulse);
		static void AddTorqueImpulseToBody(btRigidBody* const body, const btVector3& torqueImpulse);
		static void RemoveAllForceAndTorqueFromBody(btRigidBody* const body);
		static void GetBodyLinearVelocity(btRigidBody* const body, btVector3& refVelocity);
		static void GetBodyAngularVelocity(btRigidBody* const body, btVector3& refVelocity);
		static void SetBodyLinearVelocity(btRigidBody* const body, const btVector3& velocity);
		static void SetBodyAngularVelocity(btRigidBody* const body, const btVector3& velocity);
		static void ReactivateBody(btRigidBody* const body);
		static void SetBodyMass(btRigidBody* const body, float_t newMass);
		static void SetBodyGravity(btRigidBody* const body, const btVector3& gravity);
#pragma endregion

#pragma region Constraints
		static btFixedConstraint* CreateFixedConstraint(btRigidBody& parent, btRigidBody& child, const btTransform& parentInitialTransform, const btTransform& childInitialTransform);
		static void DestroyConstraint(btFixedConstraint* constraint);
#pragma endregion
	};
}