// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 25 06 2015 at 02:36 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Entities {
	public static class PhysicsManager {
		public const float ONE_METRE_SCALED = 50f;
		public const float DEFAULT_RESTITUTION = 1f;
		public const float DEFAULT_LINEAR_DAMPING = 0.05f;
		public const float DEFAULT_ANGULAR_DAMPING = 0.05f;
		public const float DEFAULT_FRICTION = 0.05f;
		public const float DEFAULT_ROLLING_FRICTION = 0.05f;

		public static unsafe void SetGravityOnAllBodies(Vector3 gravity) {
			LosgapSystem.InvokeOnMasterAsync(() => {
				AlignedAllocation<Vector4> vec4Aligned = new AlignedAllocation<Vector4>(16L, (uint) sizeof(Vector4));
				*((Vector4*) vec4Aligned.AlignedPointer) = gravity;
				try {
					InteropUtils.CallNative(
						NativeMethods.PhysicsManager_SetGravity,
						vec4Aligned.AlignedPointer
						).ThrowOnFailure();
				}
				finally {
					vec4Aligned.Dispose();
				}
			});
		}

		public static unsafe PhysicsShapeHandle CreateBoxShape(float width, float height, float depth, CollisionShapeOptionsDesc shapeOptions) {
			return LosgapSystem.InvokeOnMaster(() => {
				AlignedAllocation<CollisionShapeOptionsDesc> shapeOptionsAligned = new AlignedAllocation<CollisionShapeOptionsDesc>(16L, (uint) sizeof(CollisionShapeOptionsDesc));
				*((CollisionShapeOptionsDesc*) shapeOptionsAligned.AlignedPointer) = shapeOptions;
				AlignedAllocation<Vector4> boxExtents = new AlignedAllocation<Vector4>(16L, (uint) sizeof(Vector4));
				*((Vector4*) boxExtents.AlignedPointer) = new Vector4(width * 0.5f, height * 0.5f, depth * 0.5f, 0f);
				PhysicsShapeHandle result;
				InteropUtils.CallNative(
					NativeMethods.PhysicsManager_CreateBoxShape,
					boxExtents.AlignedPointer,
					shapeOptionsAligned.AlignedPointer,
					(IntPtr) (&result)
				).ThrowOnFailure();
				shapeOptionsAligned.Dispose();
				boxExtents.Dispose();
				return result;
			});
		}

		public static unsafe PhysicsShapeHandle CreateSimpleSphereShape(float radius, CollisionShapeOptionsDesc shapeOptions) {
			return LosgapSystem.InvokeOnMaster(() => {
				AlignedAllocation<CollisionShapeOptionsDesc> shapeOptionsAligned = new AlignedAllocation<CollisionShapeOptionsDesc>(16L, (uint) sizeof(CollisionShapeOptionsDesc));
				*((CollisionShapeOptionsDesc*) shapeOptionsAligned.AlignedPointer) = shapeOptions;
				PhysicsShapeHandle result;
				InteropUtils.CallNative(
					NativeMethods.PhysicsManager_CreateSimpleSphereShape,
					radius,
					shapeOptionsAligned.AlignedPointer,
					(IntPtr) (&result)
				).ThrowOnFailure();
				shapeOptionsAligned.Dispose();
				return result;
			});
		}

		public static unsafe PhysicsShapeHandle CreateScaledSphereShape(float radius, Vector3 scaling, CollisionShapeOptionsDesc shapeOptions) {
			return LosgapSystem.InvokeOnMaster(() => {
				AlignedAllocation<CollisionShapeOptionsDesc> shapeOptionsAligned = new AlignedAllocation<CollisionShapeOptionsDesc>(16L, (uint) sizeof(CollisionShapeOptionsDesc));
				*((CollisionShapeOptionsDesc*) shapeOptionsAligned.AlignedPointer) = shapeOptions;
				AlignedAllocation<Vector4> vec4Aligned = new AlignedAllocation<Vector4>(16L, (uint) sizeof(Vector4));
				*((Vector4*) vec4Aligned.AlignedPointer) = scaling;
				PhysicsShapeHandle result;
				InteropUtils.CallNative(
					NativeMethods.PhysicsManager_CreateScaledSphereShape,
					radius,
					vec4Aligned.AlignedPointer,
					shapeOptionsAligned.AlignedPointer,
					(IntPtr) (&result)
				).ThrowOnFailure();
				shapeOptionsAligned.Dispose();
				vec4Aligned.Dispose();
				return result;
			});
		}

		public static unsafe PhysicsShapeHandle CreateConeShape(float radius, float height, CollisionShapeOptionsDesc shapeOptions, out Vector3 translationOffset) {
			//translationOffset = Vector3.DOWN * (height * 0.5f);
			translationOffset = Vector3.ZERO;
			return LosgapSystem.InvokeOnMaster(() => {
				AlignedAllocation<CollisionShapeOptionsDesc> shapeOptionsAligned = new AlignedAllocation<CollisionShapeOptionsDesc>(16L, (uint) sizeof(CollisionShapeOptionsDesc));
				*((CollisionShapeOptionsDesc*) shapeOptionsAligned.AlignedPointer) = shapeOptions;
				PhysicsShapeHandle result;
				InteropUtils.CallNative(
					NativeMethods.PhysicsManager_CreateConeShape,
					radius,
					height,
					shapeOptionsAligned.AlignedPointer,
					(IntPtr) (&result)
				).ThrowOnFailure();
				shapeOptionsAligned.Dispose();
				return result;
			});
		}

		public static unsafe PhysicsShapeHandle CreateCylinderShape(float radius, float height, CollisionShapeOptionsDesc shapeOptions, out Vector3 translationOffset) {
			//translationOffset = Vector3.DOWN * (height * 0.5f);
			translationOffset = Vector3.ZERO;
			return LosgapSystem.InvokeOnMaster(() => {
				AlignedAllocation<CollisionShapeOptionsDesc> shapeOptionsAligned = new AlignedAllocation<CollisionShapeOptionsDesc>(16L, (uint) sizeof(CollisionShapeOptionsDesc));
				*((CollisionShapeOptionsDesc*) shapeOptionsAligned.AlignedPointer) = shapeOptions;
				PhysicsShapeHandle result;
				InteropUtils.CallNative(
					NativeMethods.PhysicsManager_CreateCylinderShape,
					radius,
					height,
					shapeOptionsAligned.AlignedPointer,
					(IntPtr) (&result)
				).ThrowOnFailure();
				shapeOptionsAligned.Dispose();
				return result;
			});
		}

		public static unsafe PhysicsShapeHandle CreateConvexHullShape(IEnumerable<Vector3> vertices, CollisionShapeOptionsDesc shapeOptions) {
			return LosgapSystem.InvokeOnMaster(() => {
				AlignedAllocation<CollisionShapeOptionsDesc> shapeOptionsAligned = new AlignedAllocation<CollisionShapeOptionsDesc>(16L, (uint) sizeof(CollisionShapeOptionsDesc));
				*((CollisionShapeOptionsDesc*) shapeOptionsAligned.AlignedPointer) = shapeOptions;
				Vector3* verticesLocal = stackalloc Vector3[vertices.Count()];
				int numVertices = 0;
				foreach (Vector3 vertex in vertices) {
					verticesLocal[numVertices++] = vertex;
				}
				PhysicsShapeHandle result;
				InteropUtils.CallNative(
					NativeMethods.PhysicsManager_CreateConvexHullShape,
					(IntPtr) verticesLocal,
					numVertices,
					shapeOptionsAligned.AlignedPointer,
					(IntPtr) (&result)
				).ThrowOnFailure();
				shapeOptionsAligned.Dispose();
				return result;
			});
		}

		public static unsafe PhysicsShapeHandle CreateCompoundCurveShape(IEnumerable<Vector3> vertices, CollisionShapeOptionsDesc shapeOptions) {
			return LosgapSystem.InvokeOnMaster(() => {
				AlignedAllocation<CollisionShapeOptionsDesc> shapeOptionsAligned = new AlignedAllocation<CollisionShapeOptionsDesc>(16L, (uint) sizeof(CollisionShapeOptionsDesc));
				*((CollisionShapeOptionsDesc*) shapeOptionsAligned.AlignedPointer) = shapeOptions;
				Vector3* verticesLocal = stackalloc Vector3[vertices.Count()];
				int numVertices = 0;
				foreach (Vector3 vertex in vertices) {
					verticesLocal[numVertices++] = vertex;
				}
				PhysicsShapeHandle result;
				InteropUtils.CallNative(
					NativeMethods.PhysicsManager_CreateCompoundCurveShape,
					(IntPtr) verticesLocal,
					(uint) numVertices / 8U,
					shapeOptionsAligned.AlignedPointer,
					(IntPtr) (&result)
				).ThrowOnFailure();
				shapeOptionsAligned.Dispose();
				return result;
			});
		}

		public static unsafe PhysicsShapeHandle CreateConcaveHullShape(IEnumerable<Vector3> vertices, IEnumerable<int> indices, CollisionShapeOptionsDesc shapeOptions, string acdFilePath) {
			return LosgapSystem.InvokeOnMaster(() => {
				AlignedAllocation<CollisionShapeOptionsDesc> shapeOptionsAligned = new AlignedAllocation<CollisionShapeOptionsDesc>(16L, (uint) sizeof(CollisionShapeOptionsDesc));
				*((CollisionShapeOptionsDesc*) shapeOptionsAligned.AlignedPointer) = shapeOptions;
				Vector3* verticesLocal = stackalloc Vector3[vertices.Count()];
				int* indicesLocal = stackalloc int[indices.Count()];
				int numVertices = 0;
				int numIndices = 0;
				foreach (Vector3 vertex in vertices) {
					verticesLocal[numVertices++] = vertex;
				}
				foreach (int index in indices) {
					indicesLocal[numIndices++] = index;
				}
				PhysicsShapeHandle result;
				InteropUtils.CallNative(
					NativeMethods.PhysicsManager_CreateConcaveHullShape,
					(IntPtr) verticesLocal,
					numVertices,
					(IntPtr) indicesLocal,
					numIndices,
					shapeOptionsAligned.AlignedPointer,
					acdFilePath,
					(IntPtr) (&result)
				).ThrowOnFailure();
				shapeOptionsAligned.Dispose();
				return result;
			});
		}

		internal static unsafe void GetCollisionPairs(List<KVP<PhysicsBodyHandle, PhysicsBodyHandle>> pairsList) {
			pairsList.Clear();
			PhysicsBodyHandle* outPBHArr;
			uint outNumPairs;

			// WARNING: No longer thread-safe
			unsafe {
				char* failReason = stackalloc char[InteropUtils.MAX_INTEROP_FAIL_REASON_STRING_LENGTH + 1];
				bool success = NativeMethods.PhysicsManager_GetCollisionPairsArray(
					(IntPtr) failReason,
					(IntPtr) (&outPBHArr),
					(IntPtr) (&outNumPairs)
				);
				if (!success) throw new NativeOperationFailedException(Marshal.PtrToStringUni((IntPtr) failReason));
			}

			for (uint i = 0U; i < outNumPairs; ++i) {
				var newValue = new KVP<PhysicsBodyHandle, PhysicsBodyHandle>(
					outPBHArr[i << 1], outPBHArr[(i << 1) + 1]
				);
				var inverse = new KVP<PhysicsBodyHandle, PhysicsBodyHandle>(
					newValue.Value, newValue.Key
				);
				if (!pairsList.Contains(newValue) && !pairsList.Contains(inverse)) pairsList.Add(newValue);
			}
		}

		internal static unsafe PhysicsBodyHandle RayTestNearest(Vector3 startPoint, Vector3 endPoint, out Vector3 hitPoint) {
			AlignedAllocation<Vector4> hitPointAligned = new AlignedAllocation<Vector4>(16L, (uint) sizeof(Vector4));
			Vector4* hitPoint4Ptr = (Vector4*) hitPointAligned.AlignedPointer;
			var result = LosgapSystem.InvokeOnMaster(() => {
				AlignedAllocation<Vector4> startPointAligned = new AlignedAllocation<Vector4>(16L, (uint) sizeof(Vector4));
				*((Vector4*) startPointAligned.AlignedPointer) = startPoint;
				AlignedAllocation<Vector4> endPointAligned = new AlignedAllocation<Vector4>(16L, (uint) sizeof(Vector4));
				*((Vector4*) endPointAligned.AlignedPointer) = endPoint;
				Vector4* hitPoint4PtrLocal = hitPoint4Ptr;
				PhysicsBodyHandle outPBH;
				InteropUtils.CallNative(NativeMethods.PhysicsManager_RayTestNearest,
					startPointAligned.AlignedPointer,
					endPointAligned.AlignedPointer,
					(IntPtr) (&outPBH),
					(IntPtr) (hitPoint4PtrLocal)
				).ThrowOnFailure();
				startPointAligned.Dispose();
				endPointAligned.Dispose();
				return outPBH;
			});
			try {
				if (result != PhysicsBodyHandle.NULL) hitPoint = (Vector3) (*((Vector4*) hitPointAligned.AlignedPointer));
				else hitPoint = Vector3.ZERO;
				return result;
			}
			finally {
				hitPointAligned.Dispose();
			}
		}

		internal static unsafe IEnumerable<RayTestCollisionDesc> RayTestAll(Vector3 startPoint, Vector3 endPoint, uint maxCollisions) {
			RayTestCollisionDesc* collisionArr = stackalloc RayTestCollisionDesc[(int) maxCollisions];
			uint maxCollisionsLocal = maxCollisions;
			RayTestCollisionDesc* collisionArrLocal = collisionArr;
			uint outNumCollisions;

			AlignedAllocation<Vector4> startPointAligned = new AlignedAllocation<Vector4>(16L, (uint) sizeof(Vector4));
			*((Vector4*) startPointAligned.AlignedPointer) = startPoint;
			AlignedAllocation<Vector4> endPointAligned = new AlignedAllocation<Vector4>(16L, (uint) sizeof(Vector4));
			*((Vector4*) endPointAligned.AlignedPointer) = endPoint;

			// WARNING: No longer thread-safe
			unsafe {
				char* failReason = stackalloc char[InteropUtils.MAX_INTEROP_FAIL_REASON_STRING_LENGTH + 1];
				bool success = NativeMethods.PhysicsManager_RayTestAll(
					(IntPtr) failReason,
					startPointAligned.AlignedPointer,
					endPointAligned.AlignedPointer,
					(IntPtr) collisionArrLocal,
					maxCollisionsLocal,
					(IntPtr) (&outNumCollisions)
				);
				if (!success) throw new NativeOperationFailedException(Marshal.PtrToStringUni((IntPtr) failReason));
			}

			List<RayTestCollisionDesc> result = new List<RayTestCollisionDesc>((int) outNumCollisions);
			for (uint i = 0U; i < outNumCollisions; ++i) result.Add(collisionArr[i]);

			startPointAligned.Dispose();
			endPointAligned.Dispose();

			return result;
		}

		internal static unsafe void RayTestAllLessGarbage(Vector3 startPoint, Vector3 endPoint, uint maxCollisions, List<RayTestCollisionDesc> reusableResultsList) {
			RayTestCollisionDesc* collisionArr = stackalloc RayTestCollisionDesc[(int) maxCollisions];
			uint maxCollisionsLocal = maxCollisions;
			RayTestCollisionDesc* collisionArrLocal = collisionArr;
			uint outNumCollisions;

			AlignedAllocation<Vector4> startPointAligned = new AlignedAllocation<Vector4>(16L, (uint) sizeof(Vector4));
			*((Vector4*) startPointAligned.AlignedPointer) = startPoint;
			AlignedAllocation<Vector4> endPointAligned = new AlignedAllocation<Vector4>(16L, (uint) sizeof(Vector4));
			*((Vector4*) endPointAligned.AlignedPointer) = endPoint;

			// WARNING: No longer thread-safe
			unsafe {
				char* failReason = stackalloc char[InteropUtils.MAX_INTEROP_FAIL_REASON_STRING_LENGTH + 1];
				bool success = NativeMethods.PhysicsManager_RayTestAll(
					(IntPtr) failReason,
					startPointAligned.AlignedPointer,
					endPointAligned.AlignedPointer,
					(IntPtr) collisionArrLocal,
					maxCollisionsLocal,
					(IntPtr) (&outNumCollisions)
				);
				if (!success) throw new NativeOperationFailedException(Marshal.PtrToStringUni((IntPtr) failReason));
			}

			startPointAligned.Dispose();
			endPointAligned.Dispose();

			reusableResultsList.Clear();
			for (uint i = 0U; i < outNumCollisions; ++i) reusableResultsList.Add(collisionArr[i]);
		}

		internal static void DestroyShape(PhysicsShapeHandle shape) {
			LosgapSystem.InvokeOnMasterAsync(() => InteropUtils.CallNative(
				NativeMethods.PhysicsManager_DestroyShape,
				shape
			).ThrowOnFailure());
		}

		internal static void DestroyConstraint(FixedConstraintHandle constraint) {
			LosgapSystem.InvokeOnMasterAsync(() => InteropUtils.CallNative(
				NativeMethods.PhysicsManager_DestroyConstraint,
				constraint
			).ThrowOnFailure());
		}

		internal static void ReactivateBody(PhysicsBodyHandle body) {
			LosgapSystem.InvokeOnMasterAsync(() => InteropUtils.CallNative(
				NativeMethods.PhysicsManager_ReactivateBody,
				body
			).ThrowOnFailure());
		}

		internal static void DestroyRigidBody(PhysicsBodyHandle body) {
			LosgapSystem.InvokeOnMasterAsync(() => InteropUtils.CallNative(
				NativeMethods.PhysicsManager_DestroyRigidBody,
				body
			).ThrowOnFailure());
		}

		internal static void SetBodyMass(PhysicsBodyHandle body, float newMass) {
			LosgapSystem.InvokeOnMasterAsync(() => InteropUtils.CallNative(
				NativeMethods.PhysicsManager_SetBodyMass,
				body,
				newMass
			).ThrowOnFailure());
		}

		internal unsafe static FixedConstraintHandle CreateFixedConstraint(
			PhysicsBodyHandle parentBody, PhysicsBodyHandle childBody,
			Vector3 parentInitialTranslation, Quaternion parentInitialRotation,
			Vector3 childInitialTranslation, Quaternion childInitialRotation) {
			return LosgapSystem.InvokeOnMaster(() => {
				Vector3 parentInitTransLocal = parentInitialTranslation;
				Quaternion parentInitRotLocal = parentInitialRotation;
				Vector3 childInitTransLocal = childInitialTranslation;
				Quaternion childInitRotLocal = childInitialRotation;
				FixedConstraintHandle result;
				InteropUtils.CallNative(NativeMethods.PhysicsManager_CreateFixedConstraint,
					parentBody,
					childBody,
					(IntPtr) (&parentInitTransLocal),
					(IntPtr) (&parentInitRotLocal),
					(IntPtr) (&childInitTransLocal),
					(IntPtr) (&childInitRotLocal),
					(IntPtr) (&result)
				);
				return result;
			});
		}

		internal unsafe static PhysicsBodyHandle CreateRigidBody(PhysicsShapeHandle shapeHandle, float mass, bool alwaysActive, bool forceIntransigence, bool collideOnlyAgainstWorld, bool collideAgainstDynamicsOnly, IntPtr translationPtr, IntPtr rotationPtr, IntPtr shapeOffsetPtr) {
			Assure.GreaterThanOrEqualTo(mass, 0f);
			if (collideOnlyAgainstWorld && collideAgainstDynamicsOnly) throw new ArgumentException("Can't collide against world and only dynamics simultaneously.");
			return LosgapSystem.InvokeOnMaster(() => {
				PhysicsBodyHandle result;
				InteropUtils.CallNative(
					NativeMethods.PhysicsManager_CreateRigidBody,
					translationPtr,
					rotationPtr,
					shapeOffsetPtr,
					shapeHandle,
					mass,
					(InteropBool) alwaysActive,
					(InteropBool) forceIntransigence,
					(InteropBool) collideOnlyAgainstWorld,
					(InteropBool) collideAgainstDynamicsOnly,
					(IntPtr) (&result)
				).ThrowOnFailure();
				return result;
			});
		}

		internal static void SetBodyProperties(PhysicsBodyHandle body,
			float restitution,
			float linearDamping,
			float angularDamping,
			float friction,
			float rollingFriction) {
				LosgapSystem.InvokeOnMasterAsync(() => InteropUtils.CallNative(
					NativeMethods.PhysicsManager_SetBodyProperties,
					body,
					restitution,
					linearDamping,
					angularDamping,
					friction,
					rollingFriction
				).ThrowOnFailure());
		}

		internal static void SetBodyCCD(PhysicsBodyHandle body, float minSpeed, float ccdRadius) {
			LosgapSystem.InvokeOnMasterAsync(() => InteropUtils.CallNative(
				NativeMethods.PhysicsManager_SetBodyCCD,
				body,
				minSpeed,
				ccdRadius
			));
		}

		internal unsafe static void AddForceToBody(PhysicsBodyHandle body, Vector3 force) {
			LosgapSystem.InvokeOnMasterAsync(() => {
				AlignedAllocation<Vector4> vec4Aligned = new AlignedAllocation<Vector4>(16L, (uint) sizeof(Vector4));
				*((Vector4*) vec4Aligned.AlignedPointer) = force;
				InteropUtils.CallNative(NativeMethods.PhysicsManager_AddForceToBody,
					body,
					vec4Aligned.AlignedPointer
				).ThrowOnFailure();
				vec4Aligned.Dispose();
			});
		}

		internal unsafe static void AddTorqueToBody(PhysicsBodyHandle body, Vector3 torque) {
			LosgapSystem.InvokeOnMasterAsync(() => {
				AlignedAllocation<Vector4> vec4Aligned = new AlignedAllocation<Vector4>(16L, (uint) sizeof(Vector4));
				*((Vector4*) vec4Aligned.AlignedPointer) = torque;
				InteropUtils.CallNative(NativeMethods.PhysicsManager_AddTorqueToBody,
					body,
					vec4Aligned.AlignedPointer
				).ThrowOnFailure();
				vec4Aligned.Dispose();
			});
		}

		internal unsafe static void AddForceImpulseToBody(PhysicsBodyHandle body, Vector3 force) {
			LosgapSystem.InvokeOnMasterAsync(() => {
				AlignedAllocation<Vector4> vec4Aligned = new AlignedAllocation<Vector4>(16L, (uint) sizeof(Vector4));
				*((Vector4*) vec4Aligned.AlignedPointer) = force;
				InteropUtils.CallNative(NativeMethods.PhysicsManager_AddForceImpulseToBody,
					body,
					vec4Aligned.AlignedPointer
				).ThrowOnFailure();
				vec4Aligned.Dispose();
			});
		}

		internal unsafe static void AddTorqueImpulseToBody(PhysicsBodyHandle body, Vector3 torque) {
			LosgapSystem.InvokeOnMasterAsync(() => {
				AlignedAllocation<Vector4> vec4Aligned = new AlignedAllocation<Vector4>(16L, (uint) sizeof(Vector4));
				*((Vector4*) vec4Aligned.AlignedPointer) = torque;
				InteropUtils.CallNative(NativeMethods.PhysicsManager_AddTorqueImpulseToBody,
					body,
					vec4Aligned.AlignedPointer
				).ThrowOnFailure();
				vec4Aligned.Dispose();
			});
		}

		internal unsafe static void RemoveAllForceAndTorqueFromBody(PhysicsBodyHandle body) {
			LosgapSystem.InvokeOnMasterAsync(() => {
				InteropUtils.CallNative(NativeMethods.PhysicsManager_RemoveAllForceAndTorqueFromBody,
					body
				).ThrowOnFailure();
			});
		}

		internal unsafe static Vector3 GetBodyLinearVelocity(PhysicsBodyHandle body) {
			AlignedAllocation<Vector4> vec4Aligned = new AlignedAllocation<Vector4>(16L, (uint) sizeof(Vector4));

			// WARNING: No longer thread-safe
			unsafe {
				char* failReason = stackalloc char[InteropUtils.MAX_INTEROP_FAIL_REASON_STRING_LENGTH + 1];
				bool success = NativeMethods.PhysicsManager_GetBodyLinearVelocity((IntPtr) failReason, body, vec4Aligned.AlignedPointer);
				if (!success) throw new NativeOperationFailedException(Marshal.PtrToStringUni((IntPtr) failReason));
			}

			try {
				return (Vector3) (*((Vector4*) vec4Aligned.AlignedPointer));
			}
			finally {
				vec4Aligned.Dispose();
			}
		}

		internal unsafe static Vector3 GetBodyAngularVelocity(PhysicsBodyHandle body) {
			return LosgapSystem.InvokeOnMaster(() => {
				AlignedAllocation<Vector4> vec4Aligned = new AlignedAllocation<Vector4>(16L, (uint) sizeof(Vector4));
				InteropUtils.CallNative(NativeMethods.PhysicsManager_GetBodyAngularVelocity,
					body,
					vec4Aligned.AlignedPointer
				).ThrowOnFailure();
				try {
					return (Vector3) (*((Vector4*) vec4Aligned.AlignedPointer));
				}
				finally {
					vec4Aligned.Dispose();
				}
			});
		}

		internal unsafe static void SetBodyAngularVelocity(PhysicsBodyHandle body, Vector3 velocity) {
			AlignedAllocation<Vector4> vec4Aligned = new AlignedAllocation<Vector4>(16L, (uint) sizeof(Vector4));
			*((Vector4*) vec4Aligned.AlignedPointer) = velocity;

			// WARNING: No longer thread-safe
			try {
				unsafe {
					char* failReason = stackalloc char[InteropUtils.MAX_INTEROP_FAIL_REASON_STRING_LENGTH + 1];
					bool success = NativeMethods.PhysicsManager_SetBodyAngularVelocity((IntPtr) failReason, body, vec4Aligned.AlignedPointer);
					if (!success) throw new NativeOperationFailedException(Marshal.PtrToStringUni((IntPtr) failReason));
				}
			}
			finally {
				vec4Aligned.Dispose();
			}
		}

		internal unsafe static void SetBodyLinearVelocity(PhysicsBodyHandle body, Vector3 velocity) {
			AlignedAllocation<Vector4> vec4Aligned = new AlignedAllocation<Vector4>(16L, (uint) sizeof(Vector4));
			*((Vector4*) vec4Aligned.AlignedPointer) = velocity;

			// WARNING: No longer thread-safe
			try {
				unsafe {
					char* failReason = stackalloc char[InteropUtils.MAX_INTEROP_FAIL_REASON_STRING_LENGTH + 1];
					bool success = NativeMethods.PhysicsManager_SetBodyLinearVelocity((IntPtr) failReason, body, vec4Aligned.AlignedPointer);
					if (!success) throw new NativeOperationFailedException(Marshal.PtrToStringUni((IntPtr) failReason));
				}
			}
			finally {
				vec4Aligned.Dispose();
			}
		}

		internal unsafe static void SetBodyGravity(PhysicsBodyHandle body, Vector3 gravity) {
			AlignedAllocation<Vector4> gravityAligned = new AlignedAllocation<Vector4>(16L, (uint) sizeof(Vector4));
			*((Vector4*) gravityAligned.AlignedPointer) = gravity;

			// WARNING: No longer thread-safe
			try {
				unsafe {
					char* failReason = stackalloc char[InteropUtils.MAX_INTEROP_FAIL_REASON_STRING_LENGTH + 1];
					bool success = NativeMethods.PhysicsManager_SetBodyGravity((IntPtr) failReason, body, gravityAligned.AlignedPointer);
					if (!success) throw new NativeOperationFailedException(Marshal.PtrToStringUni((IntPtr) failReason));
				}
			}
			finally {
				gravityAligned.Dispose();
			}
		}
			
		internal static void EngineStart() {
			LosgapSystem.InvokeOnMaster(() => InteropUtils.CallNative(
				NativeMethods.PhysicsManager_Init
			).ThrowOnFailure());
		}

		internal static void EngineStop() {
			LosgapSystem.InvokeOnMaster(() => InteropUtils.CallNative(
				NativeMethods.PhysicsManager_Shutdown
			).ThrowOnFailure());
		}

		internal static void Tick(float deltaTime) {
			// WARNING: No longer thread-safe
			unsafe {
				char* failReason = stackalloc char[InteropUtils.MAX_INTEROP_FAIL_REASON_STRING_LENGTH + 1];
				bool success = NativeMethods.PhysicsManager_Tick((IntPtr) failReason, deltaTime);
				if (!success) throw new NativeOperationFailedException(Marshal.PtrToStringUni((IntPtr) failReason));
			}
		}

		internal static void UpdateBodyTransform(PhysicsBodyHandle bodyHandle) {
			// WARNING: No longer thread-safe
			unsafe {
				char* failReason = stackalloc char[InteropUtils.MAX_INTEROP_FAIL_REASON_STRING_LENGTH + 1];
				bool success = NativeMethods.PhysicsManager_UpdateBodyTransform((IntPtr) failReason, bodyHandle);
				if (!success) throw new NativeOperationFailedException(Marshal.PtrToStringUni((IntPtr) failReason));
			}
		}

		public static void SetPhysicsTickrate(float tickrateHz) {
			unsafe {
				char* failReason = stackalloc char[InteropUtils.MAX_INTEROP_FAIL_REASON_STRING_LENGTH + 1];
				bool success = NativeMethods.PhysicsManager_SetTickrate((IntPtr) failReason, tickrateHz);
				if (!success) throw new NativeOperationFailedException(Marshal.PtrToStringUni((IntPtr) failReason));
			}
		}
	}
}