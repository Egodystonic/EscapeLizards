// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 16 07 2015 at 01:07 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using Ophidian.Losgap;
using Ophidian.Losgap.Entities;
using Ophidian.Losgap.Rendering;

namespace Egodystonic.EscapeLizards {
	public sealed class LizardCoinEntity : GroundableEntity {
		private const float ROT_PER_SEC = MathUtils.PI_OVER_TWO;
		private const float TAKEN_RISE_TIME = 0.25f;
		private const float TAKEN_FALL_TIME = 0.33f;
		private const float TAKEN_ROT_MULT = 20f;
		private const float TAKEN_SCALE_INFLATION = 1.65f;
		private const float TAKEN_RISE_PER_SEC = PhysicsManager.ONE_METRE_SCALED * 0.9f;
		private const float INITIAL_LIGHT_RADIUS = PhysicsManager.ONE_METRE_SCALED * 0.35f;
		private const float TAKEN_LIGHT_BRIGHTNESS_MULT = 1.65f;
		private static readonly Vector3 EDITOR_LIGHT_OFFSET = Vector3.UP * GameplayConstants.EGG_COLLISION_RADIUS;
		private static readonly Vector3 INITIAL_LIGHT_COLOR = new Vector3(1f, 1f, 0f) * 3f;
		private float takenTime = 0f;
		private Vector3 scaleAtTakenTime;
		private Vector3 boardDownDirAtTakenTime;
		private readonly float rotMod;
		private readonly Light coinLight;
		private bool isTaken = false;
		private int coinIndex;
		private float curRot = 0f;

		public int CoinIndex {
			get {
				lock (InstanceMutationLock) {
					return coinIndex;
				}
			}
			set {
				lock (InstanceMutationLock) {
					coinIndex = value;
				}
			}
		}

		public bool IsTaken {
			get {
				lock (InstanceMutationLock) {
					return isTaken;
				}
			}
			set {
				lock (InstanceMutationLock) {
					isTaken = value;
				}
			}
		}

		public LizardCoinEntity() : this(Transform.DEFAULT_TRANSFORM) { }

		public LizardCoinEntity(Transform initialTransform) : base(initialTransform) {
			coinLight = new Light(initialTransform.Translation, INITIAL_LIGHT_RADIUS, INITIAL_LIGHT_COLOR);
			AssetLocator.LightPass.AddLight(coinLight);
			SetScale(initialTransform.Scale * 2.2f);
			rotMod = (((int) initialTransform.Translation.LengthSquared) & 1) == 1 ? 1f : -1f;
			if (EntryPoint.InEditor) {
				coinLight.Position += EDITOR_LIGHT_OFFSET;
				this.TranslateBy(EDITOR_LIGHT_OFFSET);
			}
		}

		protected override void TransformChanged() {
			base.TransformChanged();
			if (IsDisposed) return;
			if (coinLight != null) {
				coinLight.Position = Transform.Translation;
				if (EntryPoint.InEditor) coinLight.Position += EDITOR_LIGHT_OFFSET;
			}
		}

		public override void Dispose() {
			base.Dispose();
			AssetLocator.LightPass.RemoveLight(coinLight);
		}

		public void SetPersistenceData(int index, bool taken) {
			lock (InstanceMutationLock) {
				coinIndex = index;
				if (taken) {
					SetModelInstance(AssetLocator.GameAlphaLayer, AssetLocator.LizardCoinModel, AssetLocator.LizardCoinAlphaMaterial);
				}
			}
		}

		protected override void Tick(float deltaTimeSeconds) {
			base.Tick(deltaTimeSeconds);
			if (EntryPoint.InEditor) return;
			float rotAmount = ROT_PER_SEC * deltaTimeSeconds * rotMod;
			if (isTaken) {
				rotAmount *= TAKEN_ROT_MULT;
				takenTime += deltaTimeSeconds;
				if (takenTime <= TAKEN_RISE_TIME) {
					float riseTimeFracComplete = takenTime / TAKEN_RISE_TIME;
					SetScale(scaleAtTakenTime * (1f + (TAKEN_SCALE_INFLATION - 1f) * riseTimeFracComplete));
					TranslateBy(-boardDownDirAtTakenTime.WithLength(TAKEN_RISE_PER_SEC * deltaTimeSeconds));
					coinLight.Color = INITIAL_LIGHT_COLOR * (1f + (TAKEN_LIGHT_BRIGHTNESS_MULT - 1f) * riseTimeFracComplete);
					coinLight.Radius = INITIAL_LIGHT_RADIUS * (1f + (TAKEN_LIGHT_BRIGHTNESS_MULT - 1f) * riseTimeFracComplete);
				}
				else if (takenTime <= TAKEN_RISE_TIME + TAKEN_FALL_TIME) {
					float fallTimeFracComplete = (takenTime - TAKEN_RISE_TIME) / TAKEN_FALL_TIME;
					SetScale(scaleAtTakenTime * TAKEN_SCALE_INFLATION * (1f - fallTimeFracComplete));
					TranslateBy(boardDownDirAtTakenTime.WithLength(TAKEN_RISE_PER_SEC * deltaTimeSeconds));
					coinLight.Color = INITIAL_LIGHT_COLOR * TAKEN_LIGHT_BRIGHTNESS_MULT * (1f - fallTimeFracComplete);
					coinLight.Radius = INITIAL_LIGHT_RADIUS * TAKEN_LIGHT_BRIGHTNESS_MULT * (1f - fallTimeFracComplete);
				}
				else {
					SetScale(Vector3.ZERO);
					rotAmount = 0f;
					coinLight.Radius = 0f;
				}
			}
			curRot += rotAmount;
			Quaternion boardDownRot = Quaternion.FromVectorTransition(Vector3.DOWN, GameCoordinator.BoardDownDir);
			Quaternion rotQuat = Quaternion.FromAxialRotation(GameCoordinator.BoardDownDir, curRot);
			SetRotation(InitialTransform.Rotation * boardDownRot * rotQuat * GameCoordinator.CurrentGameboardTilt);
		}

		public void BeTaken() {
			lock (InstanceMutationLock) {
				isTaken = true;
				DisposePhysicsBody();
				scaleAtTakenTime = Transform.Scale;
				boardDownDirAtTakenTime = GameCoordinator.BoardDownDir;
			}
		}
	}
}