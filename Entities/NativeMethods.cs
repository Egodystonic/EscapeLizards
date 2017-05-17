// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 25 06 2015 at 02:36 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Entities {
	[SuppressUnmanagedCodeSecurity]
	internal static class NativeMethods {
		private const string NATIVE_DLL_NAME = "PhysicsNative.dll";

		#region Lifetime
		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "PhysicsManager_Init")]
		public static extern InteropBool PhysicsManager_Init(
			IntPtr failReason
			);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "PhysicsManager_Tick")]
		public static extern InteropBool PhysicsManager_Tick(
			IntPtr failReason,
			float deltaTime
			);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "PhysicsManager_SetTickrate")]
		public static extern InteropBool PhysicsManager_SetTickrate(
			IntPtr failReason,
			float tickrate
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "PhysicsManager_GetCollisionPairsArray")]
		public static extern InteropBool PhysicsManager_GetCollisionPairsArray(
			IntPtr failReason,
			IntPtr outPairsArr, // PhysicsBodyHandle**
			IntPtr outNumPairs // uint*
			);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "PhysicsManager_Shutdown")]
		public static extern InteropBool PhysicsManager_Shutdown(
			IntPtr failReason
			);
		#endregion

		#region World
		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "PhysicsManager_SetGravity")]
		public static extern InteropBool PhysicsManager_SetGravity(
			IntPtr failReason,
			IntPtr gravity // Vector4*
			);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "PhysicsManager_RayTestNearest")]
		public static extern InteropBool PhysicsManager_RayTestNearest(
			IntPtr failReason,
			IntPtr rayStart, // Vector4*
			IntPtr rayEnd, // Vector4*
			IntPtr outPBH, // PhysicsBodyHandle*
			IntPtr outHitPoint // Vector4*
			);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "PhysicsManager_RayTestAll")]
		public static extern InteropBool PhysicsManager_RayTestAll(
			IntPtr failReason,
			IntPtr rayStart, // Vector4*
			IntPtr rayEnd, // Vector4*
			IntPtr outRayTestArr, // RayTestCollisionDesc*
			uint arrLen,
			IntPtr outNumHits // uint*
			);
		#endregion

		#region Shape Creation
		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "PhysicsManager_CreateBoxShape")]
		public static extern InteropBool PhysicsManager_CreateBoxShape(
			IntPtr failReason,
			IntPtr halfExtents, // Vector4*
			IntPtr shapeOptions, // ShapeOptionsDesc*
			IntPtr outShapeHandle // PhysicsShapeHandle*
			);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "PhysicsManager_CreateSimpleSphereShape")]
		public static extern InteropBool PhysicsManager_CreateSimpleSphereShape(
			IntPtr failReason,
			float radius,
			IntPtr shapeOptions, // ShapeOptionsDesc*
			IntPtr outShapeHandle // PhysicsShapeHandle*
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "PhysicsManager_CreateScaledSphereShape")]
		public static extern InteropBool PhysicsManager_CreateScaledSphereShape(
			IntPtr failReason,
			float radius,
			IntPtr scaling, // Vector4*
			IntPtr shapeOptions, // ShapeOptionsDesc*
			IntPtr outShapeHandle // PhysicsShapeHandle*
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "PhysicsManager_CreateConeShape")]
		public static extern InteropBool PhysicsManager_CreateConeShape(
			IntPtr failReason,
			float radius,
			float height,
			IntPtr shapeOptions, // ShapeOptionsDesc*
			IntPtr outShapeHandle // PhysicsShapeHandle*
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "PhysicsManager_CreateCylinderShape")]
		public static extern InteropBool PhysicsManager_CreateCylinderShape(
			IntPtr failReason,
			float radius,
			float height,
			IntPtr shapeOptions, // ShapeOptionsDesc*
			IntPtr outShapeHandle // PhysicsShapeHandle*
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "PhysicsManager_CreateConvexHullShape")]
		public static extern InteropBool PhysicsManager_CreateConvexHullShape(
			IntPtr failReason,
			IntPtr vertices, // Vector3*
			int numVertices,
			IntPtr shapeOptions, // ShapeOptionsDesc*
			IntPtr outShapeHandle // PhysicsShapeHandle*
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "PhysicsManager_CreateCompoundCurveShape")]
		public static extern InteropBool PhysicsManager_CreateCompoundCurveShape(
			IntPtr failReason,
			IntPtr vertices, // Vector3*
			uint numTrapezoids,
			IntPtr shapeOptions, // ShapeOptionsDesc*
			IntPtr outShapeHandle // PhysicsShapeHandle*
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "PhysicsManager_CreateConcaveHullShape")]
		public static extern InteropBool PhysicsManager_CreateConcaveHullShape(
			IntPtr failReason,
			IntPtr vertices, // Vector3*
			int numVertices,
			IntPtr indices, // int*
			int numIndices,
			IntPtr shapeOptions, // ShapeOptionsDesc*
			[MarshalAs(InteropUtils.INTEROP_STRING_TYPE)] string acdFilePath,
			IntPtr outShapeHandle // PhysicsShapeHandle*
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "PhysicsManager_DestroyShape")]
		public static extern InteropBool PhysicsManager_DestroyShape(
			IntPtr failReason,
			PhysicsShapeHandle shape
			);
		#endregion

		#region Body Creation
		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "PhysicsManager_CreateRigidBody")]
		public static extern InteropBool PhysicsManager_CreateRigidBody(
			IntPtr failReason,
			IntPtr translationPtr, // Vector4*
			IntPtr rotationPtr, // Vector4*
			IntPtr physicsShapeOffsetPtr, // Vector4*
			PhysicsShapeHandle collisionShapeHandle,
			float bodyMass,
			InteropBool alwaysActive,
			InteropBool forceIntransigence,
			InteropBool collideAgainstWorldOnly,
			InteropBool collideAgainstDynamicsOnly,
			IntPtr outBodyHandle // PhysicsBodyHandle*
			);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "PhysicsManager_SetBodyProperties")]
		public static extern InteropBool PhysicsManager_SetBodyProperties(
			IntPtr failReason,
			PhysicsBodyHandle body,
			float restitution,
			float linearDamping,
			float angularDamping,
			float friction,
			float rollingFriction
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "PhysicsManager_SetBodyCCD")]
		public static extern InteropBool PhysicsManager_SetBodyCCD(
			IntPtr failReason,
			PhysicsBodyHandle body,
			float minSpeed,
			float ccdRadius
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "PhysicsManager_DestroyRigidBody")]
		public static extern InteropBool PhysicsManager_DestroyRigidBody(
			IntPtr failReason,
			PhysicsBodyHandle body
			);
		#endregion

		#region Body Manipulation
		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "PhysicsManager_UpdateBodyTransform")]
		public static extern InteropBool PhysicsManager_UpdateBodyTransform(
			IntPtr failReason,
			PhysicsBodyHandle body
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "PhysicsManager_AddForceToBody")]
		public static extern InteropBool PhysicsManager_AddForceToBody(
			IntPtr failReason,
			PhysicsBodyHandle body,
			IntPtr force // Vector4*
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "PhysicsManager_AddTorqueToBody")]
		public static extern InteropBool PhysicsManager_AddTorqueToBody(
			IntPtr failReason,
			PhysicsBodyHandle body,
			IntPtr torque // Vector4*
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "PhysicsManager_AddForceImpulseToBody")]
		public static extern InteropBool PhysicsManager_AddForceImpulseToBody(
			IntPtr failReason,
			PhysicsBodyHandle body,
			IntPtr force // Vector4*
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "PhysicsManager_AddTorqueImpulseToBody")]
		public static extern InteropBool PhysicsManager_AddTorqueImpulseToBody(
			IntPtr failReason,
			PhysicsBodyHandle body,
			IntPtr torque // Vector4*
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "PhysicsManager_RemoveAllForceAndTorqueFromBody")]
		public static extern InteropBool PhysicsManager_RemoveAllForceAndTorqueFromBody(
			IntPtr failReason,
			PhysicsBodyHandle body
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "PhysicsManager_GetBodyLinearVelocity")]
		public static extern InteropBool PhysicsManager_GetBodyLinearVelocity(
			IntPtr failReason,
			PhysicsBodyHandle body,
			IntPtr outVelocity // Vector4*
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "PhysicsManager_GetBodyAngularVelocity")]
		public static extern InteropBool PhysicsManager_GetBodyAngularVelocity(
			IntPtr failReason,
			PhysicsBodyHandle body,
			IntPtr outVelocity // Vector4*
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "PhysicsManager_SetBodyLinearVelocity")]
		public static extern InteropBool PhysicsManager_SetBodyLinearVelocity(
			IntPtr failReason,
			PhysicsBodyHandle body,
			IntPtr velocity // Vector4*
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "PhysicsManager_SetBodyAngularVelocity")]
		public static extern InteropBool PhysicsManager_SetBodyAngularVelocity(
			IntPtr failReason,
			PhysicsBodyHandle body,
			IntPtr velocity // Vector4*
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "PhysicsManager_ReactivateBody")]
		public static extern InteropBool PhysicsManager_ReactivateBody(
			IntPtr failReason,
			PhysicsBodyHandle body
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "PhysicsManager_SetBodyMass")]
		public static extern InteropBool PhysicsManager_SetBodyMass(
			IntPtr failReason,
			PhysicsBodyHandle body,
			float mass
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "PhysicsManager_SetBodyGravity")]
		public static extern InteropBool PhysicsManager_SetBodyGravity(
			IntPtr failReason,
			PhysicsBodyHandle body,
			IntPtr gravity // Vector4*
		);
		#endregion

		#region Constraints
		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "PhysicsManager_CreateFixedConstraint")]
		public static extern InteropBool PhysicsManager_CreateFixedConstraint(
			IntPtr failReason,
			PhysicsBodyHandle parentBodyHandle,
			PhysicsBodyHandle childBodyHandle,
			IntPtr parentInitTranslation, // Vector4*
			IntPtr parentInitRotation, // Quaternion*
			IntPtr childInitTranslation, // Vector4*
			IntPtr childInitRotation, // Quaternion*
			IntPtr outConstraintHandle // FixedConstraintHandle*
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "PhysicsManager_DestroyConstraint")]
		public static extern InteropBool PhysicsManager_DestroyConstraint(
			IntPtr failReason,
			FixedConstraintHandle constraint
			);
		#endregion

	}
}