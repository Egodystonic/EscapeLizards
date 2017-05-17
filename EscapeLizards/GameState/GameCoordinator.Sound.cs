// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 08 07 2015 at 16:14 by Ben Bowen

using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ophidian.Losgap;
using Ophidian.Losgap.AssetManagement;
using Ophidian.Losgap.Audio;
using Ophidian.Losgap.Entities;
using Ophidian.Losgap.Input;
using Ophidian.Losgap.Rendering;

namespace Egodystonic.EscapeLizards {
	public static partial class GameCoordinator {
		private struct HighspeedSoundMetadata {
			public bool SlowedSinceLastSound;
			public bool CollidedSinceLastSound;
			public int TimeAtLastSound;
		}

		private const float ROLL_VOLUME_MAX = 0.4f;
		private const float ROLL_VOLUME_MIN = 0f;
		private const float ROLL_FREQ_MAX = 66200f;
		private const float ROLL_FREQ_MIN = 22050f;
		private const float ROLL_MARGIN = PhysicsManager.ONE_METRE_SCALED * 0.05f;
		private const int ROLL_RAY_FAIL_MAX_TIME_BEFORE_NO_SOUND_MS = 100;
		private const int ROLL_RAY_FAIL_MAX_TIME_BEFORE_UNBROKEN_ROLL_RESET = 1000;
		private static int lastRollTime;
		private const int UNBROKEN_ROLL_TIME_BEFORE_ROLL_SOUND = 300;
		private static int unbrokenRollTime;
		private const float IMPACT_VOLUME = 1f;
		private const int IMPACT_SOUND_MIN_INTERVAL_MS = 250;
		private const int IMPACT_SOUND_MIN_INTERVAL_SAME_GEOM_MS = 1000;
		private const float IMPACT_PITCH_MIN = 1f;
		private const float IMPACT_PITCH_MAX = 1.5f;
		private static int lastImpactSoundTime;
		private static readonly Dictionary<GeometryEntity, float> ignoredEggCollisionGeoms = new Dictionary<GeometryEntity, float>();
		private const float BOUNCE_VOLUME = 0.6f;
		private const float BOUNCE_SOUND_MIN_INTERVAL = 1f;
		private const float ALL_BOUNCE_SOUND_MIN_INTERVAL = 0.1f;
		private const float BOUNCE_PITCH_MIN = 1f;
		private const float BOUNCE_PITCH_MAX = 1.25f;
		private static int lastBounceSoundTime;
		private static int lastAllBounceSoundTime;
		private const float MIN_SPEED_CHANGE_FOR_BOUNCE_SOUND = PhysicsManager.ONE_METRE_SCALED * 2f;
		private const float MIN_VELO_DIFFERENTIAL_ANGLE_FOR_BOUNCE_SOUND = MathUtils.PI_OVER_TWO * 1.25f;
		private const float MIN_SPEED_FOR_DIFFERENTIAL_ANGLE_BOUNCE_SOUND = PhysicsManager.ONE_METRE_SCALED * 1f;
		private static Vector3 velocityLastFrame;
		private const float HIGHSPEED_SOUND_MIN_SPEED = PhysicsManager.ONE_METRE_SCALED * 4f;
		private const float HIGHSPEED_SOUND_ADDITIONAL_SPEED_FOR_PARALLEL = PhysicsManager.ONE_METRE_SCALED * 4f;
		private const float PARALLEL_HIGHSPEED_DOWN_BUFFER = PhysicsManager.ONE_METRE_SCALED * 10f;
		private const int MIN_INTERVAL_BETWEEN_HIGHSPEED_SOUNDS_MS = 1000;
		private const float HIGHSPEED_VOLUME = 0.5f;
		private const float HIGHSPEED_PITCH_MIN = 0.75f;
		private const float HIGHSPEED_PITCH_MAX = 1.25f;
		private static HighspeedSoundMetadata highspeedUpMetadata;
		private static HighspeedSoundMetadata highspeedDownMetadata;
		private static HighspeedSoundMetadata highspeedParallelMetadata;
		private const float PASS_FAIL_VOLUME = 1f;
		private const float COUNTDOWN_VOLUME = 0.3f;
		private static bool playingCountdownTimer;
		private const int COUNTDOWN_TIMER_START_TIME_MS = 10000;
		private static readonly List<RayTestCollision> reusableRayTestResultsList = new List<RayTestCollision>();

		private static void Sounds_Collision(GeometryEntity geom) {
			if ((!ignoredEggCollisionGeoms.ContainsKey(geom) || ignoredEggCollisionGeoms[geom] - timeRemainingMs >= IMPACT_SOUND_MIN_INTERVAL_SAME_GEOM_MS) 
				&& lastImpactSoundTime - timeRemainingMs >= IMPACT_SOUND_MIN_INTERVAL_MS) {
					int instanceId = AudioModule.CreateSoundInstance(AssetLocator.ImpactSounds[RandomProvider.Next(0, AssetLocator.ImpactSounds.Length)]);
				AudioModule.PlaySoundInstance(
					instanceId,
					false,
					IMPACT_VOLUME * Config.SoundEffectVolume,
					RandomProvider.Next(IMPACT_PITCH_MIN, IMPACT_PITCH_MAX)
				);
				if (velocityLastFrame != Vector3.ZERO && egg.Velocity != Vector3.ZERO) {
					MaybePlayAllBounce(Math.Abs(velocityLastFrame.Length - egg.Velocity.Length));
				}
				lastImpactSoundTime = timeRemainingMs;
			}
			ignoredEggCollisionGeoms[geom] = timeRemainingMs;
			highspeedUpMetadata.CollidedSinceLastSound = highspeedDownMetadata.CollidedSinceLastSound = highspeedParallelMetadata.CollidedSinceLastSound = true;
		}

		private static void Sounds_LevelIntro() {
			ignoredEggCollisionGeoms.Clear();
			lastRollTime = lastAllBounceSoundTime = lastBounceSoundTime = lastImpactSoundTime = Int32.MaxValue;
			unbrokenRollTime = 0;
			highspeedUpMetadata.SlowedSinceLastSound = highspeedDownMetadata.SlowedSinceLastSound = highspeedParallelMetadata.SlowedSinceLastSound = true;
			highspeedUpMetadata.CollidedSinceLastSound = highspeedDownMetadata.CollidedSinceLastSound = highspeedParallelMetadata.CollidedSinceLastSound = true;
			highspeedUpMetadata.TimeAtLastSound = highspeedDownMetadata.TimeAtLastSound = highspeedParallelMetadata.TimeAtLastSound = Int32.MaxValue;
			playingCountdownTimer = false;
			AssetLocator.CountdownLoopSound.StopAllInstances();
			AssetLocator.RollSound.StopAllInstances();
			AssetLocator.PostPassBronzeApplauseSound.StopAllInstances();
			AssetLocator.PostPassSilverApplauseSound.StopAllInstances();
			AssetLocator.PostPassGoldApplauseSound.StopAllInstances();
			//AudioModule.StopSound(AssetLocator.CountdownLoopSound);
			//AudioModule.StopSound(AssetLocator.RollSound);
			//AudioModule.StopSound(AssetLocator.PostPassBronzeApplauseSound);
			//AudioModule.StopSound(AssetLocator.PostPassSilverApplauseSound);
			//AudioModule.StopSound(AssetLocator.PostPassGoldApplauseSound);
		}

		private static void Sounds_LevelProgress() {
			AssetLocator.CountdownLoopSound.StopAllInstances();
			AssetLocator.RollSound.StopAllInstances();
			AssetLocator.PostPassBronzeApplauseSound.StopAllInstances();
			AssetLocator.PostPassSilverApplauseSound.StopAllInstances();
			AssetLocator.PostPassGoldApplauseSound.StopAllInstances();
			playingCountdownTimer = false;
		}

		private static void Sounds_LevelStart() {
			int instanceId = AudioModule.CreateSoundInstance(AssetLocator.RollSound.File);
			AssetLocator.RollSound.AddInstance(instanceId);
			AudioModule.PlaySoundInstance(instanceId, true, 0f);
		}

		private static void Sounds_LevelPause() {
			AudioModule.PauseSoundInstance(AssetLocator.RollSound.SoundInstanceIds.First());
		}
		private static void Sounds_LevelUnpause() {
			AudioModule.PlaySoundInstance(AssetLocator.RollSound.SoundInstanceIds.First(), true, 0f);
		}

		private static void Sounds_LevelPass(LevelPassDetails details) {
			AssetLocator.RollSound.StopAllInstances();
			//AudioModule.StopSound(AssetLocator.RollSound);
			playingCountdownTimer = false;
			AssetLocator.CountdownLoopSound.StopAllInstances();
			//AudioModule.StopSound(AssetLocator.CountdownLoopSound);

			int instanceId = AudioModule.CreateSoundInstance(AssetLocator.BellSound);
			AudioModule.PlaySoundInstance(
				instanceId,
				false,
				PASS_FAIL_VOLUME * Config.SoundEffectVolume
			);
			instanceId = AudioModule.CreateSoundInstance(AssetLocator.PassSound);
			AudioModule.PlaySoundInstance(
				instanceId,
				false,
				PASS_FAIL_VOLUME * Config.SoundEffectVolume
			);
			instanceId = AudioModule.CreateSoundInstance(AssetLocator.EmotePassSounds[RandomProvider.Next(0, AssetLocator.EmotePassSounds.Length)]);
			AudioModule.PlaySoundInstance(
				instanceId,
				false,
				1.7f * Config.SoundEffectVolume,
				RandomProvider.Next(0.75f, 1.25f)
			);
			switch (details.Star) {
				case Star.Gold:
					HUDSoundExtensions.Play(HUDSound.PostPassStarGold, 1.3f);
					instanceId = AudioModule.CreateSoundInstance(AssetLocator.PassGoldSound);
					AudioModule.PlaySoundInstance(
						instanceId,
						false,
						PASS_FAIL_VOLUME * Config.SoundEffectVolume
					);
					instanceId = AudioModule.CreateSoundInstance(AssetLocator.PostPassGoldApplauseSound.File);
					AssetLocator.PostPassGoldApplauseSound.AddInstance(instanceId);
					AudioModule.PlaySoundInstance(instanceId, true, 0.55f * Config.HUDVolume);
					break;
				case Star.Silver: 
					HUDSoundExtensions.Play(HUDSound.PostPassStarGold, 1f);
					instanceId = AudioModule.CreateSoundInstance(AssetLocator.PostPassSilverApplauseSound.File);
					AssetLocator.PostPassSilverApplauseSound.AddInstance(instanceId);
					AudioModule.PlaySoundInstance(instanceId, true, 0.35f * Config.HUDVolume);
					break;
				case Star.Bronze: 
					HUDSoundExtensions.Play(HUDSound.PostPassStarGold, 0.7f);
					instanceId = AudioModule.CreateSoundInstance(AssetLocator.PostPassBronzeApplauseSound.File);
					AssetLocator.PostPassBronzeApplauseSound.AddInstance(instanceId);
					AudioModule.PlaySoundInstance(instanceId, true, 0.3f * Config.HUDVolume);
					break;
			}
		}

		private static void Sounds_LevelFail(LevelFailReason reason) {
			Sounds_LevelProgress();

			if (reason == LevelFailReason.GameCancelled) return;

			int instanceId = AudioModule.CreateSoundInstance(AssetLocator.EmoteFailSounds[RandomProvider.Next(0, AssetLocator.EmoteFailSounds.Length)]);
			AudioModule.PlaySoundInstance(
				instanceId,
				false,
				1.3f * Config.SoundEffectVolume,
				RandomProvider.Next(0.75f, 1.25f)
			);

			instanceId = AudioModule.CreateSoundInstance(AssetLocator.FailAwwSounds[RandomProvider.Next(0, AssetLocator.FailAwwSounds.Length)]);
			AudioModule.PlaySoundInstance(
				instanceId,
				false,
				0.575f * Config.SoundEffectVolume,
				RandomProvider.Next(0.75f, 1.25f)
			);

			switch (reason) {
				case LevelFailReason.Dropped:
					instanceId = AudioModule.CreateSoundInstance(AssetLocator.FailFallSound);
					AudioModule.PlaySoundInstance(
						instanceId,
						false,
						PASS_FAIL_VOLUME * Config.SoundEffectVolume
					);
					break;
				case LevelFailReason.TimeUp:
					instanceId = AudioModule.CreateSoundInstance(AssetLocator.FailTimeoutSound);
					AudioModule.PlaySoundInstance(
						instanceId,
						false,
						PASS_FAIL_VOLUME * Config.SoundEffectVolume
					);
					break;
			}
		}

		private static void TickSound(float deltaTime) {
			Vector3 ballVelo = egg.Velocity;
			float ballSpeed = ballVelo.Length;
			float speedLastFrame = velocityLastFrame.Length;

			const int TICK_SOUND_EARLINESS_MS = 200;

			// Tick
			int adjustedTimeRemainingMs = timeRemainingMs - TICK_SOUND_EARLINESS_MS;
			int timeRemainingSecs = adjustedTimeRemainingMs / 1000;
			if (timeRemainingSecs != 0 && timeRemainingSecs != (int) (adjustedTimeRemainingMs + deltaTime * 1000f) / 1000) {
				int instanceId = AudioModule.CreateSoundInstance(AssetLocator.TickSound);
				AudioModule.PlaySoundInstance(
					instanceId,
					false,
					0.2f * Config.SoundEffectVolume,
					adjustedTimeRemainingMs > Config.TimePitchRaiseMs ? 1f : (adjustedTimeRemainingMs > Config.TimeWarningMs ? 1.1f : 1.2f)
				);
			}

			// Bounce
			if (velocityLastFrame != Vector3.ZERO && ballSpeed >= MIN_SPEED_FOR_DIFFERENTIAL_ANGLE_BOUNCE_SOUND && Vector3.AngleBetween(ballVelo, velocityLastFrame) >= MIN_VELO_DIFFERENTIAL_ANGLE_FOR_BOUNCE_SOUND) {
				if (lastBounceSoundTime - timeRemainingMs >= BOUNCE_SOUND_MIN_INTERVAL) {
					int instanceId = AudioModule.CreateSoundInstance(AssetLocator.ObtuseBounceSound);
					AudioModule.PlaySoundInstance(
						instanceId,
						false,
						BOUNCE_VOLUME * Config.SoundEffectVolume,
						RandomProvider.Next(BOUNCE_PITCH_MIN, BOUNCE_PITCH_MAX)
					);
					instanceId = AudioModule.CreateSoundInstance(AssetLocator.ImpactSounds[RandomProvider.Next(0, AssetLocator.ImpactSounds.Length)]);
					AudioModule.PlaySoundInstance(
						instanceId,
						false,
						IMPACT_VOLUME * Config.SoundEffectVolume,
						RandomProvider.Next(IMPACT_PITCH_MIN, IMPACT_PITCH_MAX)
					);
					lastBounceSoundTime = timeRemainingMs;
				}

				float speedDiff = Math.Abs(speedLastFrame - ballSpeed);
				int baseBitsCount = 3;
				if (speedDiff >= PhysicsManager.ONE_METRE_SCALED * 10f) baseBitsCount = 13;
				else if (speedDiff >= PhysicsManager.ONE_METRE_SCALED * 8f) baseBitsCount = 11;
				else if (speedDiff >= PhysicsManager.ONE_METRE_SCALED * 6f) baseBitsCount = 7;
				else if (speedDiff >= PhysicsManager.ONE_METRE_SCALED * 4f) baseBitsCount = 5;
				CollisionBitPool.DisseminateBits(egg.Transform.Translation, -boardDownDir.WithLength(speedDiff * 0.65f), baseBitsCount * (int) Config.PhysicsLevel);
				MaybePlayAllBounce(speedDiff);
			}
			else if (Math.Abs(ballSpeed - speedLastFrame) >= MIN_SPEED_CHANGE_FOR_BOUNCE_SOUND) { // bounce in same direction
				if (lastBounceSoundTime - timeRemainingMs >= BOUNCE_SOUND_MIN_INTERVAL) {
					int instanceId = AudioModule.CreateSoundInstance(AssetLocator.AcuteBounceSound);
					AudioModule.PlaySoundInstance(
						instanceId,
						false,
						BOUNCE_VOLUME * Config.SoundEffectVolume,
						RandomProvider.Next(BOUNCE_PITCH_MIN, BOUNCE_PITCH_MAX)
						);
					instanceId = AudioModule.CreateSoundInstance(AssetLocator.ImpactSounds[RandomProvider.Next(0, AssetLocator.ImpactSounds.Length)]);
					AudioModule.PlaySoundInstance(
						instanceId,
						false,
						IMPACT_VOLUME * Config.SoundEffectVolume,
						RandomProvider.Next(IMPACT_PITCH_MIN, IMPACT_PITCH_MAX)
						);
					lastBounceSoundTime = timeRemainingMs;
				}

				float speedDiff = Math.Abs(speedLastFrame - ballSpeed);
				int baseBitsCount = 2;
				if (speedDiff >= PhysicsManager.ONE_METRE_SCALED * 10f) baseBitsCount = 13;
				else if (speedDiff >= PhysicsManager.ONE_METRE_SCALED * 8f) baseBitsCount = 11;
				else if (speedDiff >= PhysicsManager.ONE_METRE_SCALED * 6f) baseBitsCount = 7;
				else if (speedDiff >= PhysicsManager.ONE_METRE_SCALED * 4f) baseBitsCount = 3;
				CollisionBitPool.DisseminateBits(egg.Transform.Translation, -boardDownDir.WithLength(speedDiff * 0.65f), baseBitsCount * (int) Config.PhysicsLevel);
				MaybePlayAllBounce(speedDiff);
			}

			// Roll
			float rollVolFraction = (ballSpeed - GameplayConstants.EGG_SPEED_FOR_MIN_ROLL_VOL) / (GameplayConstants.EGG_SPEED_FOR_MAX_ROLL_VOL - GameplayConstants.EGG_SPEED_FOR_MIN_ROLL_VOL);
			float rollFreqFraction = (ballSpeed - GameplayConstants.EGG_SPEED_FOR_MIN_ROLL_FREQ) / (GameplayConstants.EGG_SPEED_FOR_MAX_ROLL_FREQ - GameplayConstants.EGG_SPEED_FOR_MIN_ROLL_FREQ);
			Vector3 eggPos = egg.Transform.Translation;
			Ray rollTestRay = Ray.FromStartAndEndPoint(
				eggPos,
				eggPos + boardDownDir.WithLength(GameplayConstants.EGG_COLLISION_RADIUS + ROLL_MARGIN)
			);
			EntityModule.RayTestAllLessGarbage(rollTestRay, reusableRayTestResultsList);
			bool isRolling = false;
			for (int i = 0; i < reusableRayTestResultsList.Count; ++i) {
				if (reusableRayTestResultsList[i].Entity != egg) {
					isRolling = true;
					break;
				}
			}
			if (isRolling) {
				if (unbrokenRollTime == 0) unbrokenRollTime = (int) (deltaTime * 1000f);
				else unbrokenRollTime += lastRollTime - timeRemainingMs;
				lastRollTime = timeRemainingMs;
			}
			else {
				if (unbrokenRollTime < UNBROKEN_ROLL_TIME_BEFORE_ROLL_SOUND
					|| lastRollTime - timeRemainingMs > ROLL_RAY_FAIL_MAX_TIME_BEFORE_UNBROKEN_ROLL_RESET) unbrokenRollTime = 0;
			}

			if (unbrokenRollTime < UNBROKEN_ROLL_TIME_BEFORE_ROLL_SOUND
				|| lastRollTime - timeRemainingMs > ROLL_RAY_FAIL_MAX_TIME_BEFORE_NO_SOUND_MS 
				|| rollVolFraction <= 0f 
				|| rollFreqFraction <= 0f) {
				AudioModule.SetSoundInstanceVolume(AssetLocator.RollSound.SoundInstanceIds.First(), 0f);
			}
			else {
				if (rollVolFraction > 1f) rollVolFraction = 1f;
				if (rollFreqFraction > 1f) rollFreqFraction = 1f;
				AudioModule.SetSoundInstanceFrequency(AssetLocator.RollSound.SoundInstanceIds.First(), ROLL_FREQ_MIN + (ROLL_FREQ_MAX - ROLL_FREQ_MIN) * rollFreqFraction);
				AudioModule.SetSoundInstanceVolume(AssetLocator.RollSound.SoundInstanceIds.First(), ROLL_VOLUME_MIN + (ROLL_VOLUME_MAX - ROLL_VOLUME_MIN) * rollVolFraction * Config.SoundEffectVolume);	
			}

			// Highspeed
			if (ballSpeed >= HIGHSPEED_SOUND_MIN_SPEED) {
				float angleToUp = Vector3.AngleBetween(ballVelo, -boardDownDir);
				float angleToDown = Vector3.AngleBetween(ballVelo, boardDownDir);

				if (angleToUp <= MathUtils.PI_OVER_TWO * 0.5f) {
					if (highspeedUpMetadata.TimeAtLastSound - timeRemainingMs >= MIN_INTERVAL_BETWEEN_HIGHSPEED_SOUNDS_MS
						&& highspeedUpMetadata.CollidedSinceLastSound && highspeedUpMetadata.SlowedSinceLastSound) {
							int instanceId = AudioModule.CreateSoundInstance(AssetLocator.HighSpeedUpSounds[RandomProvider.Next(0, AssetLocator.HighSpeedUpSounds.Length)]);
						AudioModule.PlaySoundInstance(
							instanceId,
							false,
							HIGHSPEED_VOLUME * Config.SoundEffectVolume,
							RandomProvider.Next(HIGHSPEED_PITCH_MIN, HIGHSPEED_PITCH_MAX)
						);
						highspeedUpMetadata.CollidedSinceLastSound = highspeedUpMetadata.SlowedSinceLastSound = false;
						highspeedUpMetadata.TimeAtLastSound = timeRemainingMs;
					}
				}
				else if (angleToDown <= MathUtils.PI_OVER_TWO * 0.5f) {
					Ray downRay = Ray.FromStartAndEndPoint(eggPos, eggPos + boardDownDir.WithLength(PARALLEL_HIGHSPEED_DOWN_BUFFER));
					bool somethingBeneath = EntityModule.RayTestAll(downRay).Any(rtc => rtc.Entity != egg);
					if (highspeedDownMetadata.TimeAtLastSound - timeRemainingMs >= MIN_INTERVAL_BETWEEN_HIGHSPEED_SOUNDS_MS
						&& highspeedDownMetadata.CollidedSinceLastSound && highspeedDownMetadata.SlowedSinceLastSound
						&& !somethingBeneath) {
							int instanceId = AudioModule.CreateSoundInstance(AssetLocator.HighSpeedDownSounds[RandomProvider.Next(0, AssetLocator.HighSpeedDownSounds.Length)]);
						AudioModule.PlaySoundInstance(
							instanceId,
							false,
							HIGHSPEED_VOLUME * Config.SoundEffectVolume,
							RandomProvider.Next(HIGHSPEED_PITCH_MIN, HIGHSPEED_PITCH_MAX)
							);
						highspeedDownMetadata.CollidedSinceLastSound = highspeedDownMetadata.SlowedSinceLastSound = false;
						highspeedDownMetadata.TimeAtLastSound = timeRemainingMs;
					}
				}
				else if (ballSpeed >= HIGHSPEED_SOUND_MIN_SPEED + HIGHSPEED_SOUND_ADDITIONAL_SPEED_FOR_PARALLEL) {
					if (highspeedParallelMetadata.TimeAtLastSound - timeRemainingMs >= MIN_INTERVAL_BETWEEN_HIGHSPEED_SOUNDS_MS
						&& highspeedParallelMetadata.CollidedSinceLastSound && highspeedParallelMetadata.SlowedSinceLastSound) {
							int instanceId = AudioModule.CreateSoundInstance(AssetLocator.HighSpeedParallelSounds[RandomProvider.Next(0, AssetLocator.HighSpeedParallelSounds.Length)]);
						AudioModule.PlaySoundInstance(
							instanceId,
							false,
							HIGHSPEED_VOLUME * Config.SoundEffectVolume,
							RandomProvider.Next(HIGHSPEED_PITCH_MIN, HIGHSPEED_PITCH_MAX)
						);
						highspeedParallelMetadata.CollidedSinceLastSound = highspeedParallelMetadata.SlowedSinceLastSound = false;
						highspeedParallelMetadata.TimeAtLastSound = timeRemainingMs;
					}
				}
			}
			else {
				highspeedUpMetadata.SlowedSinceLastSound = highspeedDownMetadata.SlowedSinceLastSound = highspeedParallelMetadata.SlowedSinceLastSound = true;
			}

			// Countdown Timer
			if (!playingCountdownTimer && timeRemainingMs <= COUNTDOWN_TIMER_START_TIME_MS) {
				int instanceId = AudioModule.CreateSoundInstance(AssetLocator.CountdownLoopSound.File);
				AssetLocator.CountdownLoopSound.AddInstance(instanceId);
				AudioModule.PlaySoundInstance(instanceId, true, COUNTDOWN_VOLUME);
				playingCountdownTimer = true;
			}

			velocityLastFrame = ballVelo;
		}

		private static void MaybePlayAllBounce(float speedDifference) {
			if (lastAllBounceSoundTime - timeRemainingMs < ALL_BOUNCE_SOUND_MIN_INTERVAL) return;
			int instanceId = AudioModule.CreateSoundInstance(AssetLocator.AllBounceSounds[RandomProvider.Next(0, AssetLocator.AllBounceSounds.Length)]);
			AudioModule.PlaySoundInstance(
				instanceId,
				false,
				(float) MathUtils.Clamp((speedDifference / PhysicsManager.ONE_METRE_SCALED), 0f, 0.5f) * Config.SoundEffectVolume,
				RandomProvider.Next(0.5f, 1f)
			);
			lastAllBounceSoundTime = timeRemainingMs;
		}
	}
}