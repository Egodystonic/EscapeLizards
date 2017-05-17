// All code copyright (c) 2016 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 28 09 2016 at 18:05 by Ben Bowen

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Ophidian.Losgap.Audio;
using Ophidian.Losgap.Entities;

namespace Egodystonic.EscapeLizards {
	public static class BGMManager {
		private static readonly object staticMutationLock = new object();
		private const string TITLE_MUSIC_FILE = "title.mp3";
		private static int? curMusicInstanceID = null;
		private static int? prevMusicInstanceID = null;
		private static float curMusicVol = 0f;
		private static float prevMusicVol = 0f;
		private static LoopingSound curSound;
		private static LoopingSound prevSound;
		private const float MUSIC_FADE_PER_SEC = 0.5f;

		private static event Action crossfadeComplete;

		public static event Action CrossfadeComplete {
			add {
				lock (staticMutationLock) {
					if (curMusicVol >= Config.MusicVolume) value();
					else crossfadeComplete += value;
				}
			}
			remove {
				lock (staticMutationLock) crossfadeComplete -= value;
			}
		}

		internal static void Init() {
			EntityModule.PostTick += BGMTick;
			Config.ConfigRefresh += BGMConfigRefresh;
		}

		public static void StartTitleMusic() {
			lock (staticMutationLock) {
				var soundToLoad = AssetLocator.BackgroundMusic[11];
				if (soundToLoad == prevSound) return;

				string musicFile = Path.Combine(AssetLocator.AudioDir, @"BGM\") + TITLE_MUSIC_FILE;

				prevSound = curSound;
				curSound = soundToLoad;

				int instanceId = AudioModule.CreateSoundInstance(musicFile);
				curSound.AddInstance(instanceId);
				AudioModule.PlaySoundInstance(instanceId, true, Config.MusicVolume);

				prevMusicInstanceID = curMusicInstanceID;
				curMusicInstanceID = instanceId;

				prevMusicVol = 0f;
				curMusicVol = Config.MusicVolume;

				if (prevSound != null) prevSound.StopAllInstances();
			}
		}

		public static void CrossFadeToTitleMusic() {
			lock (staticMutationLock) {
				if (prevSound == null) {
					StartTitleMusic();
					return;
				}

				var soundToLoad = AssetLocator.BackgroundMusic[11];
				if (soundToLoad == curSound) return;

				if (prevSound != null) prevSound.StopAllInstances();

				string musicFile = Path.Combine(AssetLocator.AudioDir, @"BGM\") + TITLE_MUSIC_FILE;

				prevSound = curSound;
				curSound = soundToLoad;

				int instanceId = AudioModule.CreateSoundInstance(musicFile);
				curSound.AddInstance(instanceId);
				AudioModule.PlaySoundInstance(instanceId, true, 0f);

				prevMusicInstanceID = curMusicInstanceID;
				curMusicInstanceID = instanceId;

				prevMusicVol = curMusicVol;
				curMusicVol = 0f;
			}
		}

		public static void CrossFadeToWorldMusic(byte worldIndex) {
			lock (staticMutationLock) {
				var soundToLoad = AssetLocator.BackgroundMusic[worldIndex];
				if (soundToLoad == curSound) return;

				if (prevSound != null) prevSound.StopAllInstances();

				string musicFile = Path.Combine(AssetLocator.AudioDir, @"BGM\") + worldIndex + ".mp3";

				prevSound = curSound;
				curSound = soundToLoad;

				int instanceId = AudioModule.CreateSoundInstance(musicFile);
				curSound.AddInstance(instanceId);
				AudioModule.PlaySoundInstance(instanceId, true, 0f);

				prevMusicInstanceID = curMusicInstanceID;
				curMusicInstanceID = instanceId;

				prevMusicVol = curMusicVol;
				curMusicVol = 0f;
			}
		}

		private static void BGMTick(float deltaTime) {
			lock (staticMutationLock) {
				if (deltaTime > 1f) return;
				float cap = Config.MusicVolume;
				float adjustedFadePerSec = MUSIC_FADE_PER_SEC * cap;

				if (curMusicInstanceID != null && curMusicVol < cap) {
					curMusicVol += adjustedFadePerSec * deltaTime;
					if (curMusicVol > cap) curMusicVol = cap;
					AudioModule.SetSoundInstanceVolume(curMusicInstanceID.Value, curMusicVol);
				}

				if (prevMusicInstanceID != null && prevMusicVol > 0f) {
					prevMusicVol -= adjustedFadePerSec * deltaTime;
					if (prevMusicVol <= 0f) {
						prevSound.StopAllInstances();
					}
					else AudioModule.SetSoundInstanceVolume(prevMusicInstanceID.Value, prevMusicVol);
				}

				if (curMusicVol >= cap && prevMusicVol <= 0f) {
					if (crossfadeComplete != null) crossfadeComplete();
					crossfadeComplete = null;
				}
			}
		}

		private static void BGMConfigRefresh() {
			lock (staticMutationLock) {
				if (curMusicVol > Config.MusicVolume) {
					curMusicVol = Config.MusicVolume;
					AudioModule.SetSoundInstanceVolume(curMusicInstanceID.Value, curMusicVol);
				}
			}
		}
	}
}