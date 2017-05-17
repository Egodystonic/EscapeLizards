// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 01 09 2015 at 11:49 by Ben Bowen

using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using Ophidian.Losgap;
using Ophidian.Losgap.Entities;
using Ophidian.Losgap.Rendering;

namespace Egodystonic.EscapeLizards {
	public sealed class EggCameraBoom : Entity {
		private struct HiddenEntityMetadata {
			public readonly Material Material;
			public readonly ShaderResourceView TexSRV;
			public readonly LevelGeometryEntity LevelRepresentation;
			private readonly ConstantBufferBinding alphaMatColorBinding;

			public unsafe float Alpha {
				get {
					return ((float*) Material.FragmentShaderResourcePackage.GetValue(alphaMatColorBinding))[3];
				}
				set {
					*(((float*) Material.FragmentShaderResourcePackage.GetValue(alphaMatColorBinding)) + 3) = value;
				}
			}

			public HiddenEntityMetadata(Material material, ShaderResourceView texSrv, LevelGeometryEntity levelRepresentation, ConstantBufferBinding alphaMatColorBinding) {
				Material = material;
				TexSRV = texSrv;
				LevelRepresentation = levelRepresentation;
				this.alphaMatColorBinding = alphaMatColorBinding;
			}
		}

		public readonly EggEntity Egg;
		public readonly Camera Camera;
		private readonly Dictionary<GeometryEntity, HiddenEntityMetadata> hiddenEntities = new Dictionary<GeometryEntity, HiddenEntityMetadata>();
		private readonly List<GeometryEntity> hiddenEntityAccountingSpace = new List<GeometryEntity>();
		private readonly ConstantBufferBinding alphaMatColorBinding = (ConstantBufferBinding) AssetLocator.AlphaFragmentShader.GetBindingByIdentifier("MaterialProperties");
		private readonly ResourceViewBinding alphaMatTextureBinding = (ResourceViewBinding) AssetLocator.AlphaFragmentShader.GetBindingByIdentifier("DiffuseMap");
		private float camOrientAltXTarget = 0f;
		private float camOrientAltYTarget = 0f;
		private float camOrientAltXActual = 0f;
		private float camOrientAltYActual = 0f;
		private Vector3 unalteredCamOrientation = Vector3.ZERO;
		private Vector3? unalteredCamPosition = null;
		private readonly float cameraHeight, cameraDistance;

		private static readonly List<RayTestCollision> reusableRTCList = new List<RayTestCollision>();

		public Vector3 UnalteredCamOrientation {
			get {
				lock (InstanceMutationLock) {
					return unalteredCamOrientation == Vector3.ZERO ? Camera.Orientation : unalteredCamOrientation;
				}
			}
		}

		public Vector3 UnalteredCamPosition {
			get {
				lock (InstanceMutationLock) {
					return unalteredCamPosition ?? Camera.Position;
				}
			}
		}

		public EggCameraBoom(EggEntity egg, Camera camera, float cameraHeight, float cameraDistance) {
			Assure.NotNull(egg);
			Assure.NotNull(camera);
			Assure.False(egg.IsDisposed);
			Assure.False(camera.IsDisposed);
			Egg = egg;
			Camera = camera;
			this.cameraHeight = cameraHeight;
			this.cameraDistance = cameraDistance;
		}

		public void SetCamOrientationAlteration(float amountX, float amountY) {
			lock (InstanceMutationLock) {
				camOrientAltXTarget = amountX * GameplayConstants.CAM_RS_ALTERATION_FACTOR;
				camOrientAltYTarget = amountY * GameplayConstants.CAM_RS_ALTERATION_FACTOR;
			}
		}

		protected override void Tick(float deltaTimeSeconds) {
			base.Tick(deltaTimeSeconds);
			if (Egg.IsDisposed || Camera.IsDisposed || GameCoordinator.CurrentGameState != GameCoordinator.OverallGameState.LevelPlaying) return;

			// Declare some variables that we'll use below
			Vector3 eggPos = Egg.Transform.Translation;
			Vector3 eggVelo = Egg.TrueVelocity;
			Vector3 camPos = UnalteredCamPosition;
			Vector3 camOrientation = UnalteredCamOrientation;
			Vector3 boardDownDir = GameCoordinator.BoardDownDir;
			Plane groundPlane = GameCoordinator.GroundPlane;
			Quaternion boardTilt = GameCoordinator.CurrentGameboardTilt;
			float flopsErrorMargin = MathUtils.FlopsErrorMargin;
			Vector3 verticalEggPos = eggPos.ProjectedOnto(boardDownDir);
			Vector3 horizontalEggPos = groundPlane.PointProjection(eggPos);
			Vector3 verticalEggVelo = eggVelo.ProjectedOnto(boardDownDir);
			Vector3 horizontalEggVelo = groundPlane.OrientationProjection(eggVelo);
			Vector3 verticalCamPos = camPos.ProjectedOnto(boardDownDir);
			Vector3 horizontalCamPos = groundPlane.PointProjection(camPos);
			Vector3 horizontalCamOrientation = groundPlane.OrientationProjection(camOrientation);
			Vector3 verticalCamOrientation = camOrientation.ProjectedOnto(boardDownDir);
			Vector3 curUpDir = Camera.UpDirection;
			Vector3 targetUpDir = -boardDownDir;
			Camera.Position = UnalteredCamPosition;

			// Find where the camera "wants" to be (only according to the egg velo)
			Vector3 reverseEggVelo = horizontalEggVelo.LengthSquared > GameplayConstants.MIN_EGG_SPEED_FOR_VELO_BASED_CAM_SQ ? -horizontalEggVelo : -horizontalCamOrientation;
			Vector3 targetPos = eggPos + reverseEggVelo.WithLength(cameraDistance);

			// Add the height addition to the target pos
			float verticalAddition =
				verticalEggVelo.LengthSquared > horizontalEggVelo.LengthSquared 
				&& verticalEggVelo.LengthSquared > GameplayConstants.MIN_FALL_SPEED_FOR_FALLING_CAM_SQ
				? cameraHeight + GameplayConstants.CAMERA_FALLING_HEIGHT_EXTRA
				: cameraHeight;
			targetPos += -boardDownDir.WithLength(verticalAddition);
			
			// Calculate the rotation around the egg that the boom is making (in 3 dimensions!), and calculate the amount to rotate this frame
			Vector3 eggToTarget = targetPos - eggPos;
			eggToTarget *= boardTilt.Subrotation(GameplayConstants.CAM_BOARD_TILT_FACTOR);
			Vector3 eggToCam = camPos - eggPos;
			float eggToTargetLen = eggToTarget.Length;
			Quaternion cameraRot = Quaternion.FromVectorTransition(eggToCam.ToUnit(), eggToTarget.ToUnit());
			eggToTarget = eggToCam * cameraRot.Subrotation(Math.Min(GameplayConstants.CAM_MOVE_SPEED_ANGULAR * deltaTimeSeconds, 1f));
			targetPos = eggPos + (eggToTargetLen > flopsErrorMargin ? eggToTarget.WithLength(eggToTargetLen) : Vector3.ZERO);

			// Get the distance from the current pos to the target pos, calculate the amount to move closer this frame, and move the camera
			Vector3 camToTarget = targetPos - camPos;
			float movementDist =
				(float) Math.Pow(camToTarget.LengthSquared, GameplayConstants.CAM_MOVE_SPEED_EXPONENT_LINEAR)
				* deltaTimeSeconds;
			if (camToTarget.LengthSquared > flopsErrorMargin) Camera.Move(camToTarget.WithLength(Math.Min(movementDist, camToTarget.Length)));

			// Calculate the rotation of orientation, and then rotate the camera according to the rotation speed
			Vector3 newOrientation = (eggPos - camPos).ToUnit();
			Quaternion orientRot = Quaternion.FromVectorTransition(camOrientation, newOrientation);
			Vector3 newCamUp;
			if (curUpDir == targetUpDir) newCamUp = targetUpDir;
			else {
				Quaternion curToTargetRot = Quaternion.FromVectorTransition(curUpDir, targetUpDir);
				newCamUp = curUpDir * curToTargetRot.Subrotation(GameplayConstants.CAM_UP_SUBROT_PER_SEC * deltaTimeSeconds);
			}
			Camera.Orient(
				camOrientation * orientRot.Subrotation(Math.Min(GameplayConstants.CAM_ORIENT_SPEED * deltaTimeSeconds, 1f)),
				newCamUp
			);

			// Adjust camera orientation and position according to right-stick
			float xDiff = camOrientAltXTarget - camOrientAltXActual;
			float xDelta = xDiff * GameplayConstants.CAM_ORIENTATION_ADJUSTMENT_DELTA_FRAC_PER_SEC_X * deltaTimeSeconds;
			if (Math.Abs(camOrientAltXTarget) < Math.Abs(camOrientAltXActual)) {
				xDelta = xDiff * GameplayConstants.CAM_ORIENTATION_ADJUSTMENT_DELTA_FRAC_PER_SEC_X_STOPPING * deltaTimeSeconds;
			}
			else {
				xDelta = Math.Max(
					Math.Abs(xDelta),
					Math.Min(
						GameplayConstants.CAM_ORIENTATION_ADJUSTMENT_DELTA_ABS_MIN_PER_SEC_X * deltaTimeSeconds,
						Math.Abs(xDiff)
					)
				) * (xDiff < 0f ? -1f : 1f);	
			}
			camOrientAltXActual = camOrientAltXActual + xDelta;

			float yDiff = camOrientAltYTarget - camOrientAltYActual;
			float yDelta = yDiff * GameplayConstants.CAM_ORIENTATION_ADJUSTMENT_DELTA_FRAC_PER_SEC_Y * deltaTimeSeconds;
			if (Math.Abs(camOrientAltYTarget) < Math.Abs(camOrientAltYActual)) {
				yDelta = yDiff * GameplayConstants.CAM_ORIENTATION_ADJUSTMENT_DELTA_FRAC_PER_SEC_Y_STOPPING * deltaTimeSeconds;
			}
			else {
				yDelta = Math.Max(
					Math.Abs(yDelta),
					Math.Min(
						GameplayConstants.CAM_ORIENTATION_ADJUSTMENT_DELTA_ABS_MIN_PER_SEC_Y * deltaTimeSeconds,
						Math.Abs(yDiff)
						)
					) * (yDiff < 0f ? -1f : 1f);
			}
			camOrientAltYActual = camOrientAltYActual + yDelta;

			unalteredCamOrientation = Camera.Orientation;
			Vector3 alteredOrientation = unalteredCamOrientation;

			Quaternion forwardToUpRot = Quaternion
				.FromVectorTransition(alteredOrientation, -boardDownDir)
				.Subrotation(GameplayConstants.MAX_CAM_ORIENTATION_ADJUSTMENT_Y * camOrientAltYActual);

			var verticalOrient = alteredOrientation * forwardToUpRot;
			var angleToFloor = new Plane(-boardDownDir, eggPos).IncidentAngleWith(new Ray(camPos, verticalOrient)); // not sure why the 0.2f is needed but it is...
			bool useVerticalOrient = 
				angleToFloor >= GameplayConstants.CAM_Y_ADJUSTMENT_HORIZONTAL_BUFFER_RADIANS_BOTTOM
				&& angleToFloor <= MathUtils.PI_OVER_TWO - GameplayConstants.CAM_Y_ADJUSTMENT_HORIZONTAL_BUFFER_RADIANS_TOP
				&& alteredOrientation.ProjectedOnto(-boardDownDir).ToUnit() != -boardDownDir;

			Quaternion forwardToLeftRot = Quaternion
				.FromAxialRotation(-boardDownDir, -MathUtils.PI_OVER_TWO)
				.Subrotation(GameplayConstants.MAX_CAM_ORIENTATION_ADJUSTMENT_X * camOrientAltXActual);

			alteredOrientation = (useVerticalOrient ? verticalOrient : unalteredCamOrientation) * forwardToLeftRot;
			Camera.Orient(alteredOrientation, useVerticalOrient ? Camera.UpDirection * forwardToUpRot : Camera.UpDirection);

			unalteredCamPosition = Camera.Position;
			Vector3 alteredPosition = unalteredCamPosition.Value;
			Vector3 downVec = boardDownDir;
			Vector3 leftVec = groundPlane.OrientationProjection(unalteredCamOrientation).ToUnit() * Quaternion.FromAxialRotation(boardDownDir, -MathUtils.PI_OVER_TWO);
			if (useVerticalOrient) alteredPosition += downVec.WithLength(GameplayConstants.CAM_ADJUSTMENT_TRANSLATION_MAX_Y * camOrientAltYActual);
			alteredPosition += leftVec.WithLength(GameplayConstants.CAM_ADJUSTMENT_TRANSLATION_MAX_X * camOrientAltXActual);

			Camera.Position = alteredPosition;

			unalteredCamPosition = alteredPosition;
			unalteredCamOrientation = alteredOrientation;

			// Finally, make any geometry entities that are obscuring the view of the egg transparent
			var curLevel = GameCoordinator.CurrentlyLoadedLevel;
			hiddenEntityAccountingSpace.Clear();
			foreach (var key in hiddenEntities.Keys) {
				hiddenEntityAccountingSpace.Add(key);
			}
			EntityModule.RayTestAllLessGarbage(Ray.FromStartAndEndPoint(Camera.Position, eggPos), reusableRTCList);
			foreach (RayTestCollision rtc in reusableRTCList) {
				var geomEntity = rtc.Entity as GeometryEntity;
				if (geomEntity == null) continue;
				
				if (hiddenEntities.ContainsKey(geomEntity)) {
					hiddenEntityAccountingSpace.Remove(geomEntity);
					continue;
				}
				LevelGeometryEntity entityLevelRep = curLevel.GetLevelEntityRepresentation(geomEntity);
				Material alphaMat = new Material(entityLevelRep.Material.Name + " + Alpha", AssetLocator.AlphaFragmentShader);
				alphaMat.SetMaterialConstantValue(alphaMatColorBinding, new Vector4(
					GameplayConstants.OBSCURING_GEOM_BRIGHTNESS, 
					GameplayConstants.OBSCURING_GEOM_BRIGHTNESS, 
					GameplayConstants.OBSCURING_GEOM_BRIGHTNESS,
					GameplayConstants.OBSCURING_GEOM_MIN_ALPHA
				));
				ShaderResourceView texSRV = AssetLocator.LoadTexture(entityLevelRep.Material.TextureFileName).CreateView();
				alphaMat.SetMaterialResource(alphaMatTextureBinding, texSRV);
				hiddenEntities.Add(geomEntity, new HiddenEntityMetadata(
					alphaMat, texSRV, entityLevelRep, alphaMatColorBinding
				));
				geomEntity.SetModelInstance(
					AssetLocator.GameAlphaLayer,
					curLevel.GetModelHandleForGeometry(entityLevelRep.Geometry),
					alphaMat
				);
			}

			foreach (KeyValuePair<GeometryEntity, HiddenEntityMetadata> kvp in hiddenEntities) {
				HiddenEntityMetadata metadata = kvp.Value;
				if (hiddenEntityAccountingSpace.Contains(kvp.Key)) {
					metadata.Alpha += GameplayConstants.OBSCURING_GEOM_ALPHA_REGAIN_SPEED * deltaTimeSeconds;
					if (metadata.Alpha < GameplayConstants.OBSCURING_GEOM_MAX_ALPHA) hiddenEntityAccountingSpace.Remove(kvp.Key);
				}
				else if (metadata.Alpha > GameplayConstants.OBSCURING_GEOM_MIN_ALPHA) {
					metadata.Alpha -= GameplayConstants.OBSCURING_GEOM_ALPHA_REGAIN_SPEED * deltaTimeSeconds;
				}
			}

			foreach (GeometryEntity hiddenEntity in hiddenEntityAccountingSpace) {
				var hiddenEntityMetadata = hiddenEntities.Pop(hiddenEntity);
				AssetLocator.UnloadTexture(hiddenEntityMetadata.LevelRepresentation.Material.TextureFileName);
				hiddenEntityMetadata.TexSRV.Dispose();
				hiddenEntityMetadata.Material.Dispose();

				hiddenEntity.SetModelInstance(
					AssetLocator.GameLayer,
					curLevel.GetModelHandleForGeometry(hiddenEntityMetadata.LevelRepresentation.Geometry),
					curLevel.GetLoadedMaterialForLevelMaterial(hiddenEntityMetadata.LevelRepresentation.Material)
				);
			}
		}

	}
}