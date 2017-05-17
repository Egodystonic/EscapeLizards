// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created by Ben Bowen


#include "PhysicsManager.h"
#include "LosgapMotionState.h"
#include "..\bullet3-2.83.5\v-hacd-master\v-hacd-master\src\VHACD_Lib\public\VHACD.h"
#include <mutex>
#include <vector>
#include <algorithm>
#include <Windows.h>
#include <fstream>

namespace losgap {
#ifdef DEBUG
	const float TICK_RATE_INTERNAL = 40.0f;
#else
	const float TICK_RATE_INTERNAL = 300.0f;
#endif
	const int MAX_STEP_SUBSTEPS = TICK_RATE_INTERNAL / 20;
	btBroadphaseInterface* broadphaseInstance = nullptr;
	btCollisionConfiguration* collisionConfig = nullptr;
	btCollisionDispatcher* collisionDispatcher = nullptr;
	btConstraintSolver* constraintSolver = nullptr;
	btDynamicsWorld* dynamicsWorld = nullptr;
	const btVector3 ZERO_VECTOR { 0.0f, 0.0f, 0.0f };
	float tickrate = TICK_RATE_INTERNAL;
	float substeps = tickrate / 20.0f;

	std::mutex globalCompoundShapeLock;
	std::vector<btCollisionShape*> liveCompoundShapes;

	std::vector<const btCollisionObject*> collisionList;

	VHACD::IVHACD* convexDecompositionInterface = VHACD::CreateVHACD();

#pragma region Lifetime
	void TickCallback(btDynamicsWorld* world, btScalar timeStep) {
		int numManifolds = world->getDispatcher()->getNumManifolds();
		for (int i = 0; i < numManifolds; i++) {
			btPersistentManifold* contactManifold = world->getDispatcher()->getManifoldByIndexInternal(i);

			int numContacts = contactManifold->getNumContacts();
			for (int j = 0; j < numContacts; ++j) {
				btManifoldPoint& pt = contactManifold->getContactPoint(j);
				if (pt.getDistance() <= 0.5f) {
					collisionList.push_back(contactManifold->getBody0());
					collisionList.push_back(contactManifold->getBody1());
					break;
				}
			}
		}
	}

	void PhysicsManager::Init() {
		broadphaseInstance = new btDbvtBroadphase { };
		collisionConfig = new btDefaultCollisionConfiguration { };
		collisionDispatcher = new btCollisionDispatcher { collisionConfig };
		constraintSolver = new btSequentialImpulseConstraintSolver { };
		dynamicsWorld = new btDiscreteDynamicsWorld {
			collisionDispatcher,
			broadphaseInstance,
			constraintSolver,
			collisionConfig
		};

		dynamicsWorld->setInternalTickCallback(TickCallback);

		btContactSolverInfo& contactSolver = dynamicsWorld->getSolverInfo();
		contactSolver.m_numIterations = 750;
		contactSolver.m_splitImpulse = 1;
		contactSolver.m_splitImpulsePenetrationThreshold = 0.0f;
	}
	EXPORT(PhysicsManager_Init) {
		PhysicsManager::Init();
		EXPORT_END;
	}

	const btCollisionObject** PhysicsManager::GetCollisionPairsArray(uint32_t& numPairs) {
		if (collisionList.empty()) {
			numPairs = 0U;
			return nullptr;
		}
		numPairs = static_cast<uint32_t>(collisionList.size() / 2);
		return &collisionList.front();
	}
	EXPORT(PhysicsManager_GetCollisionPairsArray, const btCollisionObject*** outPairsArr, uint32_t* outNumPairs) {
		*outPairsArr = PhysicsManager::GetCollisionPairsArray(*outNumPairs);
		EXPORT_END;
	}

	void PhysicsManager::Tick(btScalar deltaTime) {
		collisionList.clear();
		dynamicsWorld->stepSimulation(deltaTime, substeps, 1.0f / tickrate);
	}
	EXPORT(PhysicsManager_Tick, float_t deltaTime) {
		PhysicsManager::Tick(deltaTime);
		EXPORT_END;
	}

	void PhysicsManager::SetTickrate(float tickrate) {
		losgap::tickrate = tickrate;
		losgap::substeps = tickrate / 20.0f;
	}
	EXPORT(PhysicsManager_SetTickrate, float_t tickrate) {
		PhysicsManager::SetTickrate(tickrate);
		EXPORT_END;
	}


	void PhysicsManager::Shutdown() {
		SAFE_DELETE(dynamicsWorld);
		SAFE_DELETE(constraintSolver);
		SAFE_DELETE(collisionDispatcher);
		SAFE_DELETE(collisionConfig);
		SAFE_DELETE(broadphaseInstance);
	}
	EXPORT(PhysicsManager_Shutdown) {
		PhysicsManager::Shutdown();
		EXPORT_END;
	}
#pragma endregion

#pragma region World
	void PhysicsManager::SetGravity(const btVector3& gravity) {
		dynamicsWorld->setGravity(gravity);
	}
	EXPORT(PhysicsManager_SetGravity, const btVector3& gravity) {
		PhysicsManager::SetGravity(gravity);
		EXPORT_END;
	}

	btRigidBody* PhysicsManager::RayTestNearest(const btVector3& rayStart, const btVector3& rayEnd, btVector3* outHitPoint) {
		btCollisionWorld::ClosestRayResultCallback crrc { rayStart, rayEnd };
		dynamicsWorld->rayTest(rayStart, rayEnd, crrc);
		if (!crrc.hasHit()) return nullptr;
		*outHitPoint = crrc.m_hitPointWorld;
		return const_cast<btRigidBody*>(static_cast<const btRigidBody*>(crrc.m_collisionObject));
	}
	EXPORT(PhysicsManager_RayTestNearest, btVector3& rayStart, btVector3& rayEnd, btRigidBody** outRigidyBodyPtr, btVector3* outHitPoint) {
		*outRigidyBodyPtr = PhysicsManager::RayTestNearest(rayStart, rayEnd, outHitPoint);
		EXPORT_END;
	}

	uint32_t PhysicsManager::RayTestAll(const btVector3& rayStart, const btVector3& rayEnd, RayTestCollisionDesc* const outCollisionDescArr, uint32_t arrLen) {
		btCollisionWorld::AllHitsRayResultCallback ahrrc { rayStart, rayEnd };
		dynamicsWorld->rayTest(rayStart, rayEnd, ahrrc);
		auto collisionObjects = ahrrc.m_collisionObjects;
		auto hitPoints = ahrrc.m_hitPointWorld;
		uint32_t copyLimit = static_cast<uint32_t>(collisionObjects.size());
		if (arrLen < copyLimit) copyLimit = arrLen;
		for (uint32_t i = 0; i < copyLimit; ++i) {
			outCollisionDescArr[i] = RayTestCollisionDesc { 
				const_cast<btRigidBody*>(static_cast<const btRigidBody*>(collisionObjects[static_cast<int>(i)])),
				hitPoints[i]
			};
		}
		return copyLimit;
	}
	EXPORT(PhysicsManager_RayTestAll, btVector3& rayStart, btVector3& rayEnd, RayTestCollisionDesc* outCollisionDescArr, uint32_t arrLen, uint32_t* outNumCollisions) {
		*outNumCollisions = PhysicsManager::RayTestAll(rayStart, rayEnd, outCollisionDescArr, arrLen);
		EXPORT_END;
	}
#pragma endregion

#pragma region Shape Creation
	void SetShapeOptions(btCollisionShape& collisionShape, const CollisionShapeOptionsDesc& shapeOptions) {
		collisionShape.setLocalScaling(shapeOptions.Scaling);
	}
	
	btBoxShape* PhysicsManager::CreateBoxShape(const btVector3& halfExtents, const CollisionShapeOptionsDesc& shapeOptions) {
		btBoxShape* result = new btBoxShape { halfExtents };
		SetShapeOptions(*result, shapeOptions);
		return result;
	}
	EXPORT(PhysicsManager_CreateBoxShape, const btVector3& halfExtents, CollisionShapeOptionsDesc* shapeOptions, btBoxShape** outShapePtr) {
		*outShapePtr = PhysicsManager::CreateBoxShape(halfExtents, *shapeOptions);
		EXPORT_END;
	}

	btSphereShape* PhysicsManager::CreateSimpleSphereShape(btScalar radius, const CollisionShapeOptionsDesc& shapeOptions) {
		btSphereShape* result = new btSphereShape { radius };
		SetShapeOptions(*result, shapeOptions);
		return result;
	}
	EXPORT(PhysicsManager_CreateSimpleSphereShape, float_t radius, CollisionShapeOptionsDesc* shapeOptions, btSphereShape** outShapePtr) {
		*outShapePtr = PhysicsManager::CreateSimpleSphereShape(radius, *shapeOptions);
		EXPORT_END;
	}

	btMultiSphereShape* PhysicsManager::CreateScaledSphereShape(btScalar radius, const btVector3& scaling, const CollisionShapeOptionsDesc& shapeOptions) {
		btMultiSphereShape* result = new btMultiSphereShape { &ZERO_VECTOR, &radius, 1 };
		SetShapeOptions(*result, shapeOptions);
		return result;
	}
	EXPORT(PhysicsManager_CreateScaledSphereShape, float_t radius, const btVector3& scaling, CollisionShapeOptionsDesc* shapeOptions, btMultiSphereShape** outShapePtr) {
		*outShapePtr = PhysicsManager::CreateScaledSphereShape(radius, scaling, *shapeOptions);
		EXPORT_END;
	}

	btConeShape* PhysicsManager::CreateConeShape(btScalar radius, btScalar height, const CollisionShapeOptionsDesc& shapeOptions) {
		btConeShape* result = new btConeShape { radius, height };
		SetShapeOptions(*result, shapeOptions);
		return result;
	}
	EXPORT(PhysicsManager_CreateConeShape, float_t radius, float_t height, CollisionShapeOptionsDesc* shapeOptions, btConeShape** outShapePtr) {
		*outShapePtr = PhysicsManager::CreateConeShape(radius, height, *shapeOptions);
		EXPORT_END;
	}

	btCylinderShape* PhysicsManager::CreateCylinderShape(btScalar radius, btScalar height, const CollisionShapeOptionsDesc& shapeOptions) {
		btVector3 shapeDesc { radius, height * 0.5f, radius };
		btCylinderShape* result = new btCylinderShape { shapeDesc };
		SetShapeOptions(*result, shapeOptions);
		return result;
	}
	EXPORT(PhysicsManager_CreateCylinderShape, float_t radius, float_t height, CollisionShapeOptionsDesc* shapeOptions, btCylinderShape** outShapePtr) {
		*outShapePtr = PhysicsManager::CreateCylinderShape(radius, height, *shapeOptions);
		EXPORT_END;
	}

	btConvexHullShape* PhysicsManager::CreateConvexHullShape(const btScalar* const vertexComponentArr, int numVertices, const CollisionShapeOptionsDesc& shapeOptions) {
		btConvexHullShape* result = new btConvexHullShape { vertexComponentArr, numVertices / 3, 3 * sizeof(btScalar) };
		//auto x = CollisionShapeOptionsDesc { }; // for some reason this is necessary... compiler bug or C++ corner case? Who fucking knows, fuck C++
		//x.Scaling = shapeOptions.Scaling;
		//SetShapeOptions(*result, x);
		SetShapeOptions(*result, shapeOptions);
		return result;
	}
	EXPORT(PhysicsManager_CreateConvexHullShape, btScalar* vertexComponentArr, int numVertices, CollisionShapeOptionsDesc* shapeOptions, btConvexHullShape** outShapePtr) {
		*outShapePtr = PhysicsManager::CreateConvexHullShape(vertexComponentArr, numVertices, *shapeOptions);
		EXPORT_END;
	}

	btCompoundShape* PhysicsManager::CreateCompoundCurveShape(const btScalar* const vertexComponentArr, const uint32_t numTrapPrisms, const CollisionShapeOptionsDesc& shapeOptions) {
		btCompoundShape* result = new btCompoundShape { };
		SetShapeOptions(*result, shapeOptions);
		btTransform defaultTransform { };
		defaultTransform.setIdentity();
		CollisionShapeOptionsDesc bulletConvexHullOptions { };

		btScalar trapPrismPoints[8U * 3U];

		for (uint32_t i = 0U; i < numTrapPrisms; ++i) {
			for (unsigned int p = 0U; p < 8U * 3U; ++p) {
				trapPrismPoints[p] = vertexComponentArr[i * 8 * 3 + p];
			}

			result->addChildShape(defaultTransform, CreateConvexHullShape(trapPrismPoints, 8 * 3, bulletConvexHullOptions));
		}

		std::lock_guard<std::mutex> { globalCompoundShapeLock };
		liveCompoundShapes.push_back(result);

		return result;
	}
	EXPORT(PhysicsManager_CreateCompoundCurveShape, const btScalar* const vertexComponentArr, const uint32_t numTrapPrisms, const CollisionShapeOptionsDesc& shapeOptions, btCompoundShape** outShapePtr) {
		*outShapePtr = PhysicsManager::CreateCompoundCurveShape(vertexComponentArr, numTrapPrisms, shapeOptions);
		EXPORT_END;
	}

	btCompoundShape* PhysicsManager::CreateConcaveHullShape(const btScalar* const vertexComponentArr, const int numVertices, const int* const indices, const int numIndices, const CollisionShapeOptionsDesc& shapeOptions, const char* acdFilePath) {
		btCompoundShape* result = new btCompoundShape { };
		CollisionShapeOptionsDesc bulletConvexHullOptions { };
		btTransform defaultTransform { };
		defaultTransform.setIdentity();

		std::ifstream acdFileR;
		std::ofstream acdFileW;

		bool readableFile = false;
		if (acdFilePath != nullptr) {
			acdFileR.open(acdFilePath, std::ifstream::binary);
			readableFile = !acdFileR.fail();
		}

		if (readableFile) {
			unsigned int numConvexApproximations;
			unsigned int numPoints;
			double point;

			acdFileR.read(reinterpret_cast<char*>(&numConvexApproximations), sizeof(numConvexApproximations));

			for (unsigned int convexHullIndex = 0U; convexHullIndex < numConvexApproximations; ++convexHullIndex) {
				acdFileR.read(reinterpret_cast<char*>(&numPoints), sizeof(numPoints));
				btScalar* pointsArr = new btScalar[numPoints * 3];

				for (unsigned int pointIndex = 0U; pointIndex < numPoints; ++pointIndex) {
					acdFileR.read(reinterpret_cast<char*>(&point), sizeof(point));
					pointsArr[pointIndex * 3U + 0U] = static_cast<btScalar>(point);

					acdFileR.read(reinterpret_cast<char*>(&point), sizeof(point));
					pointsArr[pointIndex * 3U + 1U] = static_cast<btScalar>(point);

					acdFileR.read(reinterpret_cast<char*>(&point), sizeof(point));
					pointsArr[pointIndex * 3U + 2U] = static_cast<btScalar>(point);
				}

				result->addChildShape(defaultTransform, CreateConvexHullShape(pointsArr, numPoints * 3U, bulletConvexHullOptions));

				delete[] pointsArr;
			}
		}
		else {
			if (acdFilePath != nullptr) {
				acdFileW.open(acdFilePath, std::ofstream::binary);
				if (!acdFileW) throw LosgapException { L"Could not open acd file for writing." };
			}

			VHACD::IVHACD::Parameters convexDecompositionParams { };

			bool decompSuccess = convexDecompositionInterface->Compute(vertexComponentArr, 3U, numVertices, indices, 3U, numIndices / 3, convexDecompositionParams);
			if (!decompSuccess) throw LosgapException { L"Could not decompose given concave mesh in to convex approximations." };

			unsigned int numConvexApproximations = convexDecompositionInterface->GetNConvexHulls();
			VHACD::IVHACD::ConvexHull convexHull;

			if (acdFilePath != nullptr) acdFileW.write(reinterpret_cast<char*>(&numConvexApproximations), sizeof(numConvexApproximations));

			for (unsigned int convexHullIndex = 0U; convexHullIndex < numConvexApproximations; ++convexHullIndex) {
				convexDecompositionInterface->GetConvexHull(convexHullIndex, convexHull);

				btScalar* pointsArr = new btScalar[convexHull.m_nPoints * 3];
				if (acdFilePath != nullptr) acdFileW.write(reinterpret_cast<char*>(&convexHull.m_nPoints), sizeof(convexHull.m_nPoints));

				for (unsigned int pointIndex = 0U; pointIndex < convexHull.m_nPoints; ++pointIndex) {
					pointsArr[pointIndex * 3U + 0U] = static_cast<btScalar>(convexHull.m_points[pointIndex * 3U + 0U]);
					pointsArr[pointIndex * 3U + 1U] = static_cast<btScalar>(convexHull.m_points[pointIndex * 3U + 1U]);
					pointsArr[pointIndex * 3U + 2U] = static_cast<btScalar>(convexHull.m_points[pointIndex * 3U + 2U]);
				}

				if (acdFilePath != nullptr) acdFileW.write(reinterpret_cast<char*>(convexHull.m_points), sizeof(double) * 3 * convexHull.m_nPoints);

				result->addChildShape(defaultTransform, CreateConvexHullShape(pointsArr, convexHull.m_nPoints * 3U, bulletConvexHullOptions));

				delete[] pointsArr;
			}
		}

		SetShapeOptions(*result, shapeOptions);

		std::lock_guard<std::mutex> { globalCompoundShapeLock };
		liveCompoundShapes.push_back(result);

		return result;
	}
	EXPORT(PhysicsManager_CreateConcaveHullShape, const btScalar* const vertexComponentArr, int numVertices, const int* const indices, const int numIndices, const CollisionShapeOptionsDesc& shapeOptions, INTEROP_STRING acdFilePath, btCompoundShape** outShapePtr) {
		if (acdFilePath == nullptr) {
			*outShapePtr = PhysicsManager::CreateConcaveHullShape(vertexComponentArr, numVertices, indices, numIndices, shapeOptions, nullptr);
		}
		else {
			auto stringPtr = LosgapString::AsNewCString(acdFilePath);
			*outShapePtr = PhysicsManager::CreateConcaveHullShape(vertexComponentArr, numVertices, indices, numIndices, shapeOptions, stringPtr.get());
		}
		EXPORT_END;
	}

	void PhysicsManager::DestroyShape(btCollisionShape* shape) {
		std::lock_guard<std::mutex> { globalCompoundShapeLock };
		auto liveShapeIndex = std::find(liveCompoundShapes.begin(), liveCompoundShapes.end(), shape);
		if (liveShapeIndex != liveCompoundShapes.end()) {
			btCompoundShape* shapeAsCompound = static_cast<btCompoundShape*>(shape);
			int numChildShapes = shapeAsCompound->getNumChildShapes();
			btCollisionShape** collisionShapes = new btCollisionShape*[numChildShapes];
			for (int i = 0; i < numChildShapes; ++i) {
				collisionShapes[i] = shapeAsCompound->getChildShape(i);
			}
			liveCompoundShapes.erase(liveShapeIndex);

			delete shape;

			for (int i = 0; i < numChildShapes; ++i) {
				delete collisionShapes[i];
			}
			delete[] collisionShapes;
		}
		else delete shape;
	}
	EXPORT(PhysicsManager_DestroyShape, btCollisionShape* shape) {
		PhysicsManager::DestroyShape(shape);
		EXPORT_END;
	}
#pragma endregion

#pragma region Body Creation
	#define BIT(x) (1<<(x))
	enum CollisionType : short {
		COL_NOTHING = 0,
		COL_EVERYTHING_ELSE = BIT(0),
		COL_WALL = BIT(1),
		COL_WORLD_COL_ONLY = BIT(2),
	};

	btRigidBody* CreateAndAddStandardBody(btDynamicsWorld* const dynamicsWorld, const btRigidBody::btRigidBodyConstructionInfo& ctorInfo) {
		btRigidBody* result = new btRigidBody { ctorInfo };
		dynamicsWorld->addRigidBody(result, COL_EVERYTHING_ELSE, COL_WALL | COL_EVERYTHING_ELSE);
		return result;
	}

	btRigidBody* CreateAndAddWorldColOnlyBody(btDynamicsWorld* const dynamicsWorld, const btRigidBody::btRigidBodyConstructionInfo& ctorInfo) {
		btRigidBody* result = new btRigidBody { ctorInfo };
		dynamicsWorld->addRigidBody(result, COL_WORLD_COL_ONLY, COL_WALL);
		return result;
	}

	btRigidBody* CreateAndAddNonWallColBody(btDynamicsWorld* const dynamicsWorld, const btRigidBody::btRigidBodyConstructionInfo& ctorInfo) {
		btRigidBody* result = new btRigidBody { ctorInfo };
		dynamicsWorld->addRigidBody(result, COL_EVERYTHING_ELSE, COL_EVERYTHING_ELSE);
		return result;
	}

	btRigidBody* CreateAndAddIntransigentBody(btDynamicsWorld* const dynamicsWorld, const btRigidBody::btRigidBodyConstructionInfo& ctorInfo) {
		btRigidBody* result = new btRigidBody { ctorInfo };
		dynamicsWorld->addRigidBody(result, COL_WALL, COL_EVERYTHING_ELSE | COL_WORLD_COL_ONLY);
		result->setGravity(ZERO_VECTOR);
		result->setCollisionFlags(result->getCollisionFlags() | btCollisionObject::CF_KINEMATIC_OBJECT);
		return result;
	}

	btRigidBody* PhysicsManager::CreateRigidBody(btVector3* const translationPtr, btQuaternion* const rotationPtr, btVector3* translationOffsetPtr, btCollisionShape* const collisionShape, btScalar bodyMass, bool alwaysActive, bool forceIntransigence, bool worldColOnly, bool nonWallCol) {
#if DEBUG
		if (((unsigned long) translationPtr & 15) != 0) throw LosgapException { "Translation pointer must point to a 16-byte aligned Vector4" };
		if (((unsigned long) rotationPtr & 15) != 0) throw LosgapException { "Rotation pointer must point to a 16-byte aligned Quaternion" };
#endif
		btMotionState* motionState = new LosgapMotionState { translationPtr, rotationPtr, translationOffsetPtr };
		btVector3 inertia { 0.0f, 0.0f, 0.0f };
		if (bodyMass > 0.0f) collisionShape->calculateLocalInertia(bodyMass, inertia);
		btRigidBody* result = nullptr;
		btRigidBody::btRigidBodyConstructionInfo ctorInfo { bodyMass, motionState, collisionShape, inertia };
		if (forceIntransigence) result = CreateAndAddIntransigentBody(dynamicsWorld, ctorInfo);
		else if (nonWallCol) result = CreateAndAddNonWallColBody(dynamicsWorld, ctorInfo);
		else if (worldColOnly) result = CreateAndAddWorldColOnlyBody(dynamicsWorld, ctorInfo);
		else result = CreateAndAddStandardBody(dynamicsWorld, ctorInfo);
		/*if (alwaysActive) {*/
			result->setActivationState(DISABLE_DEACTIVATION);
			result->setContactProcessingThreshold(-0.00000005f);
		/*}*/
		return result;
	}
	EXPORT(PhysicsManager_CreateRigidBody, btVector3* translationPtr, btQuaternion* rotationPtr, btVector3* translationOffsetPtr, btCollisionShape* collisionShape, float_t bodyMass, INTEROP_BOOL alwaysActive, INTEROP_BOOL forceIntransigence, INTEROP_BOOL worldColOnly, INTEROP_BOOL nonWallCol, btRigidBody** outRigidBodyPtr) {
		*outRigidBodyPtr = PhysicsManager::CreateRigidBody(translationPtr, rotationPtr, translationOffsetPtr, collisionShape, bodyMass, INTEROP_BOOL_TO_CBOOL(alwaysActive), INTEROP_BOOL_TO_CBOOL(forceIntransigence), INTEROP_BOOL_TO_CBOOL(worldColOnly), INTEROP_BOOL_TO_CBOOL(nonWallCol));
		EXPORT_END;
	}

	void PhysicsManager::SetBodyProperties(btRigidBody* const body, btScalar restitution, btScalar linearDamping, btScalar angularDamping, btScalar friction, btScalar rollingFriction) {
		body->setRestitution(restitution);
		body->setDamping(linearDamping, angularDamping);
		body->setFriction(friction);
		body->setRollingFriction(rollingFriction);
	}
	EXPORT(PhysicsManager_SetBodyProperties, btRigidBody* const body, float_t restitution, float_t linearDamping, float_t angularDamping, float_t friction, float_t rollingFriction) {
		PhysicsManager::SetBodyProperties(body, restitution, linearDamping, angularDamping, friction, rollingFriction);
		EXPORT_END;
	}

	void PhysicsManager::SetBodyCCD(btRigidBody* const body, btScalar minSpeed, btScalar ccdRadius) {
		body->setCcdMotionThreshold(minSpeed);
		body->setCcdSweptSphereRadius(ccdRadius);
	}
	EXPORT(PhysicsManager_SetBodyCCD, btRigidBody* const body, float_t minSpeed, float_t ccdRadius) {
		PhysicsManager::SetBodyCCD(body, minSpeed, ccdRadius);
		EXPORT_END;
	}

	void PhysicsManager::DestroyRigidBody(btRigidBody* const body) {
		dynamicsWorld->removeRigidBody(body);
		delete body;
	}
	EXPORT(PhysicsManager_DestroyRigidBody, btRigidBody* body) {
		PhysicsManager::DestroyRigidBody(body);
		EXPORT_END;
	}
#pragma endregion

#pragma region Body Manipulation
	void PhysicsManager::UpdateBodyTransform(btRigidBody* const body) {
		btTransform transform;
		body->getMotionState()->getWorldTransform(transform);
		body->setWorldTransform(transform);
	}
	EXPORT(PhysicsManager_UpdateBodyTransform, btRigidBody* body) {
		PhysicsManager::UpdateBodyTransform(body);
		EXPORT_END;
	}

	void PhysicsManager::AddForceToBody(btRigidBody* const body, const btVector3& force) {
		body->applyCentralForce(force);
	}
	EXPORT(PhysicsManager_AddForceToBody, btRigidBody* const body, const btVector3& force) {
		PhysicsManager::AddForceToBody(body, force);
		EXPORT_END;
	}

	void PhysicsManager::AddTorqueToBody(btRigidBody* const body, const btVector3& torque) {
		body->applyTorque(torque);
	}
	EXPORT(PhysicsManager_AddTorqueToBody, btRigidBody* const body, const btVector3& torque) {
		PhysicsManager::AddTorqueToBody(body, torque);
		EXPORT_END;
	}

	void PhysicsManager::AddForceImpulseToBody(btRigidBody* const body, const btVector3& force) {
		body->applyCentralImpulse(force);
	}
	EXPORT(PhysicsManager_AddForceImpulseToBody, btRigidBody* const body, const btVector3& force) {
		PhysicsManager::AddForceImpulseToBody(body, force);
		EXPORT_END;
	}

	void PhysicsManager::AddTorqueImpulseToBody(btRigidBody* const body, const btVector3& torque) {
		body->applyTorqueImpulse(torque);
	}
	EXPORT(PhysicsManager_AddTorqueImpulseToBody, btRigidBody* const body, const btVector3& torque) {
		PhysicsManager::AddTorqueImpulseToBody(body, torque);
		EXPORT_END;
	}

	void PhysicsManager::RemoveAllForceAndTorqueFromBody(btRigidBody* const body) {
		body->clearForces();
	}
	EXPORT(PhysicsManager_RemoveAllForceAndTorqueFromBody, btRigidBody* const body) {
		PhysicsManager::RemoveAllForceAndTorqueFromBody(body);
		EXPORT_END;
	}

	void PhysicsManager::GetBodyLinearVelocity(btRigidBody* const body, btVector3& refVelocity) {
		refVelocity = body->getLinearVelocity();
	}
	EXPORT(PhysicsManager_GetBodyLinearVelocity, btRigidBody* const body, btVector3& refVelocity) {
		PhysicsManager::GetBodyLinearVelocity(body, refVelocity);
		EXPORT_END;
	}

	void PhysicsManager::GetBodyAngularVelocity(btRigidBody* const body, btVector3& refVelocity) {
		refVelocity = body->getAngularVelocity();
	}
	EXPORT(PhysicsManager_GetBodyAngularVelocity, btRigidBody* const body, btVector3& refVelocity) {
		PhysicsManager::GetBodyAngularVelocity(body, refVelocity);
		EXPORT_END;
	}

	void PhysicsManager::SetBodyLinearVelocity(btRigidBody* const body, const btVector3& velocity) {
		body->setLinearVelocity(velocity);
	}
	EXPORT(PhysicsManager_SetBodyLinearVelocity, btRigidBody* const body, btVector3& velocity) {
		PhysicsManager::SetBodyLinearVelocity(body, velocity);
		EXPORT_END;
	}

	void PhysicsManager::SetBodyAngularVelocity(btRigidBody* const body, const btVector3& velocity) {
		body->setAngularVelocity(velocity);
	}
	EXPORT(PhysicsManager_SetBodyAngularVelocity, btRigidBody* const body, btVector3& velocity) {
		PhysicsManager::SetBodyAngularVelocity(body, velocity);
		EXPORT_END;
	}

	void PhysicsManager::ReactivateBody(btRigidBody* const body) {
		body->activate(true);
	}
	EXPORT(PhysicsManager_ReactivateBody, btRigidBody* const body) {
		PhysicsManager::ReactivateBody(body);
		EXPORT_END;
	}

	void PhysicsManager::SetBodyMass(btRigidBody* const body, float_t newMass) {
		btVector3 inertia;
		body->getCollisionShape()->calculateLocalInertia(newMass, inertia);
		body->setMassProps(newMass, inertia);
	}
	EXPORT(PhysicsManager_SetBodyMass, btRigidBody* const body, float_t newMass) {
		PhysicsManager::SetBodyMass(body, newMass);
		EXPORT_END;
	}

	void PhysicsManager::SetBodyGravity(btRigidBody* const body, const btVector3& gravity) {
		body->setGravity(gravity);
	}
	EXPORT(PhysicsManager_SetBodyGravity, btRigidBody* const body, const btVector3& gravity) {
		PhysicsManager::SetBodyGravity(body, gravity);
		EXPORT_END;
	}
#pragma endregion

#pragma region Constraints
	btFixedConstraint* PhysicsManager::CreateFixedConstraint(btRigidBody& parent, btRigidBody& child, const btTransform& parentInitialTransform, const btTransform& childInitialTransform) {
		return new btFixedConstraint { parent, child, parentInitialTransform, childInitialTransform };
	}
	EXPORT(PhysicsManager_CreateFixedConstraint,
		btRigidBody* parent, btRigidBody* child,
		btVector3& parentInitialTranslation, btQuaternion& parentInitialRotation,
		btVector3& childInitialTranslation, btQuaternion& childInitialRotation,
		btFixedConstraint** outConstraint) {
		btTransform initialParentTransform { parentInitialRotation, parentInitialTranslation };
		btTransform initialChildTransform { childInitialRotation, childInitialTranslation };
		*outConstraint = PhysicsManager::CreateFixedConstraint(*parent, *child, initialParentTransform, initialChildTransform);
		EXPORT_END;
	}

	void PhysicsManager::DestroyConstraint(btFixedConstraint* constraint) {
		delete constraint;
	}
	EXPORT(PhysicsManager_DestroyConstraint, btFixedConstraint* constraint) {
		PhysicsManager::DestroyConstraint(constraint);
		EXPORT_END;
	}
#pragma endregion
}