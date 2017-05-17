// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 17 08 2015 at 11:09 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using Ophidian.Losgap;
using Ophidian.Losgap.Entities;
using Ophidian.Losgap.Rendering;

namespace Egodystonic.EscapeLizards {
	public sealed class EggEntity : Entity {
		private const float TRUE_VELOCITY_CALCULATION_WINDOW_SIZE_SECS = 0.1f;
		private static readonly Sphere EGG_SPHERE = new Sphere(Vector3.ZERO, GameplayConstants.EGG_COLLISION_RADIUS);
		private readonly Light eggIllumination;
		private readonly Vector3[] lastPositions;
		private readonly float[] elapsedTimes;
		private uint nextPositionSlot;
		private GeometryEntity lastTouchedGeom = null;
		private float lastTouchedGeomTime = 0f;
		private float lastTouchedGeomFirstTime = 0f;
		private int lastTouchedGeomConsecutiveInTimeLimit = 0;
		private Vector3 velocityLastFrame = Vector3.ZERO;
		private const float BULLET_BUG_MIN_ANGLE_DIFF_FOR_CORRECTION = MathUtils.PI_OVER_TWO * 0.25f;
		private const float BULLET_BUG_MIN_ROLL_TIME = 0.05f;
		private const int BULLET_BUG_MIN_ADDITIONAL_TOUCHES_FOR_ROLL = 2;
		private readonly List<RayTestCollision> bulletBugRayTestReusableList = new List<RayTestCollision>();
		private const float BULLET_BUG_MIN_SPEED = PhysicsManager.ONE_METRE_SCALED * 0.65f;
		private const float BULLET_BUG_MIN_SPEED_SQ = BULLET_BUG_MIN_SPEED * BULLET_BUG_MIN_SPEED;
		private float bulletBugLastCollTime = 0f;
		private float airtime = 0f;
		private float topSpeed = 0f;

		public Vector3 TrueVelocity {
			get {
				lock (InstanceMutationLock) {
					if (nextPositionSlot < elapsedTimes.Length) return Vector3.ZERO;
					return (lastPositions[((nextPositionSlot - 1U) % lastPositions.Length)] - lastPositions[(nextPositionSlot % lastPositions.Length)])
						/ (elapsedTimes[((nextPositionSlot - 1U) % lastPositions.Length)] - elapsedTimes[(nextPositionSlot % lastPositions.Length)]);
				}
			}
		}

		public EggEntity(LizardEggMaterial material = LizardEggMaterial.LizardEgg) {
			SetModelInstance(AssetLocator.GameLayer, material.Model(), material.Material());
			SetPhysicsShape(
				AssetLocator.LizardEggPhysicsShape, 
				Vector3.ZERO, 
				GameplayConstants.EGG_MASS,
				disablePerformanceDeactivation: true,
				restitution: GameplayConstants.EGG_RESTITUTION,
				linearDamping: GameplayConstants.EGG_DAMPING_LINEAR,
				angularDamping: GameplayConstants.EGG_DAMPING_ANGULAR,
				friction: GameplayConstants.EGG_FRICTION,
				rollingFriction: GameplayConstants.EGG_FRICTION_ROLLING
			);
			EnableContinuousCollisionDetection(GameplayConstants.MIN_SPEED_FOR_EGG_CCD, GameplayConstants.EGG_COLLISION_RADIUS * 0.01f);

			AssetLocator.MainGeometryPass._EGGHACK_MH = material.Model();

			eggIllumination = new Light(Vector3.ZERO, GameplayConstants.EGG_ILLUMINATION_RADIUS, material.CameraIlluminationColor());
			AssetLocator.LightPass.AddLight(eggIllumination);

			lastPositions = new Vector3[(int) (TRUE_VELOCITY_CALCULATION_WINDOW_SIZE_SECS / (1f / (EntityModule.TickRateHz ?? 300f)))];
			elapsedTimes = new float[lastPositions.Length];
			nextPositionSlot = 0;

			EntityModule.PostTick += PostTick;
			CollisionDetected += ColDet;
		}

		public Quaternion GetAngularDisplacementForTick(float deltaTimeSeconds) {
			Vector3 velocity = GameCoordinator.GroundPlane.OrientationProjection(Velocity * deltaTimeSeconds);
			if (velocity != Vector3.ZERO && !EntityModule.PausePhysics) {
				Quaternion angularDisplacement = Quaternion.FromAxialRotation(
					Quaternion.FromAxialRotation(GameCoordinator.BoardDownDir, MathUtils.PI_OVER_TWO) * velocity,
					-MathUtils.TWO_PI * (velocity.Length / EGG_SPHERE.Circumference)
				);
				return angularDisplacement;
			}
			return Quaternion.IDENTITY;
		}

		protected override void Tick(float deltaTimeSeconds) {
			base.Tick(deltaTimeSeconds);
			var angularDisplacement = GetAngularDisplacementForTick(deltaTimeSeconds);
			AssetLocator.MainGeometryPass._EGGHACK_ROT *= angularDisplacement;
			//AngularVelocity = new Vector3(angularDisplacement.RotAroundX, angularDisplacement.RotAroundY, angularDisplacement.RotAroundZ);
			AngularVelocity = Vector3.ZERO;

			//float downwardVeloComponent = Velocity.ProjectedOnto(GameCoordinator.BoardDownDir).Length;
			//Velocity -= GameCoordinator.BoardDownDir.WithLength(downwardVeloComponent);

			// OPTION 1
			//var veloChangeAngle = Vector3.AngleBetween(
			//	velocityLastFrame.ProjectedOnto(GameCoordinator.BoardDownDir),
			//	Velocity.ProjectedOnto(GameCoordinator.BoardDownDir)
			//);

			//if (veloChangeAngle > MathUtils.PI_OVER_TWO) {
			//	if (Vector3.AngleBetween(velocityLastFrame, GameCoordinator.BoardDownDir) < Vector3.AngleBetween(Velocity, GameCoordinator.BoardDownDir)) {
			//		SetTranslation(lastPositions[((nextPositionSlot - 2) % lastPositions.Length)]);
			//		Velocity = TrueVelocity;
			//		HUDSound.UISelectNegativeOption.Play();
			//	}
			//}
			
			// OPTION 2

			var downDir = GameCoordinator.BoardDownDir;
			Ray rollTestRay = new Ray(
				Transform.Translation - downDir.WithLength(GameplayConstants.EGG_COLLISION_RADIUS), 
				downDir, 
				GameplayConstants.EGG_COLLISION_RADIUS * 3f
			);
			EntityModule.RayTestAllLessGarbage(rollTestRay, bulletBugRayTestReusableList, 5U);
			for (int i = 0; i < bulletBugRayTestReusableList.Count; ++i) {
				GeometryEntity touchedEntity = bulletBugRayTestReusableList[i].Entity as GeometryEntity;
				if (touchedEntity != null) BulletBugRayHandle(touchedEntity);
			}

			CheckForAndCorrectBulletBug(deltaTimeSeconds);

			lastPositions[(nextPositionSlot % lastPositions.Length)] = Transform.Translation;
			elapsedTimes[(nextPositionSlot % lastPositions.Length)] = EntityModule.ElapsedTime;
			++nextPositionSlot;

			velocityLastFrame = Velocity;

			if (GameCoordinator.CurrentGameState == GameCoordinator.OverallGameState.LevelPlaying) {
				airtime += deltaTimeSeconds;
				topSpeed = Math.Max(topSpeed, TrueVelocity.Length);
			}
		}

		public override void Dispose() {
			base.Dispose();
			lock (InstanceMutationLock) {
				AssetLocator.LightPass.RemoveLight(eggIllumination);
				EntityModule.PostTick -= PostTick;
			}
		}

		private void PostTick(float deltaTime) {
			eggIllumination.Position = Transform.Translation - GameCoordinator.BoardDownDir.WithLength(GameplayConstants.EGG_LIGHT_HEIGHT);
		}

		private void BulletBugRayHandle(GeometryEntity touchedGeom) {
			EggLanded();

			if (touchedGeom is PresetMovementEntity) {
				lastTouchedGeom = null;
				return;
			}

			var timeNow = EntityModule.ElapsedTime;
			if (lastTouchedGeom == touchedGeom) {
				float timeSinceLastTouch = timeNow - lastTouchedGeomTime;
				if (timeSinceLastTouch > BULLET_BUG_MIN_ROLL_TIME) {					
					lastTouchedGeomConsecutiveInTimeLimit = 0;
					lastTouchedGeomFirstTime = timeNow;
				}	
				else ++lastTouchedGeomConsecutiveInTimeLimit;
				lastTouchedGeomTime = timeNow;
			}
			else {
				lastTouchedGeom = touchedGeom;
				lastTouchedGeomFirstTime = lastTouchedGeomTime = timeNow;
				lastTouchedGeomConsecutiveInTimeLimit = 0;
			}
		}

		private void ColDet(Entity a, Entity b) {
			if (a == lastTouchedGeom || b == lastTouchedGeom) return;
			if (!(a is GeometryEntity) && !(b is GeometryEntity)) return;

			bulletBugLastCollTime = EntityModule.ElapsedTime;
		}

		private void CheckForAndCorrectBulletBug(float deltaTime) {
			if (velocityLastFrame == Vector3.ZERO || Velocity == Vector3.ZERO) return;
			var boardDownDir = GameCoordinator.BoardDownDir;
			float angleLastFrame = Vector3.AngleBetween(velocityLastFrame, boardDownDir);
			float angleThisFrame = Vector3.AngleBetween(Velocity, boardDownDir);
			var angleDiff = Math.Abs(angleLastFrame - angleThisFrame);

			if (angleDiff < BULLET_BUG_MIN_ANGLE_DIFF_FOR_CORRECTION) return; // Check that we've hit a 'fake bump'
			//Vector3 differenceInVerticalVelocity = (Velocity - velocityLastFrame).ProjectedOnto(boardDownDir);

			//HUDSound.UIUnavailable.Play();

			//Logger.Log("=----------------------------------------------------------------------=");
			//Logger.Log(angleThisFrame + " vs " + angleLastFrame);
			//Logger.Log(lastTouchedGeomConsecutiveInTimeLimit + " touches");
			//Logger.Log(EntityModule.ElapsedTime - lastTouchedGeomTime + "s since last touch");
			//Logger.Log(EntityModule.ElapsedTime - lastTouchedGeomFirstTime + "s since first touch");
			//Logger.Log(EntityModule.ElapsedTime - bulletBugLastCollTime + "s since last coll");
			//Logger.Log((Velocity.Length / PhysicsManager.ONE_METRE_SCALED) + "m/s velo (" + (((Velocity.Length / PhysicsManager.ONE_METRE_SCALED) * 60f * 60f) / 1000f) + " km/h)");

			if (angleThisFrame < angleLastFrame) return; // Check that we've gone 'up'
			if (lastTouchedGeomConsecutiveInTimeLimit < BULLET_BUG_MIN_ADDITIONAL_TOUCHES_FOR_ROLL) return; // Check that we are rolling
			if (EntityModule.ElapsedTime - lastTouchedGeomTime > BULLET_BUG_MIN_ROLL_TIME) return; // Check that we are still rolling
			if (EntityModule.ElapsedTime - lastTouchedGeomFirstTime < BULLET_BUG_MIN_ROLL_TIME) return; // Check that we've been rolling 'long enough'
			if (EntityModule.ElapsedTime - bulletBugLastCollTime < BULLET_BUG_MIN_ROLL_TIME) return; // Check that we haven't just hit something other than what we're rolling on
			if (GameCoordinator.HopInProgress) return; // If we just hopped then obviously we're expecting to fly up in the air
			if (Velocity.LengthSquared < BULLET_BUG_MIN_SPEED_SQ) return; // Ignore little things

			//Logger.Log("Success");

			//SetTranslation(lastPositions[((nextPositionSlot - 1) % lastPositions.Length)] + TrueVelocity * deltaTime);
			Velocity = TrueVelocity;
			//HUDSound.PostPassLeaderboard.Play();
		}

		private void EggLanded() {
			CheckAirtimeAndTopSpeedForAchievements();
			airtime = 0f;
			topSpeed = 0f;
		}

		private void CheckAirtimeAndTopSpeedForAchievements() {
			if (GameCoordinator.CurrentGameState != GameCoordinator.OverallGameState.LevelPlaying) return;
			
			if (airtime >= 4f) AchievementsManager.NotifyAchievementCriteriaMet(SteamAchievement.Land4SecAirtime);
			if (airtime >= 2f) AchievementsManager.NotifyAchievementCriteriaMet(SteamAchievement.Land2SecAirtime);

			float metresPerSec = topSpeed / PhysicsManager.ONE_METRE_SCALED;
			float metresPerHour = metresPerSec * 60f * 60f;
			float kilometresPerHour = metresPerHour / 1000f;

			if (kilometresPerHour >= 80f) AchievementsManager.NotifyAchievementCriteriaMet(SteamAchievement.Reach80KMH);
			if (kilometresPerHour >= 60f) AchievementsManager.NotifyAchievementCriteriaMet(SteamAchievement.Reach60KMH);
			if (kilometresPerHour >= 40f) AchievementsManager.NotifyAchievementCriteriaMet(SteamAchievement.Reach40KMH);
		}
	}
}