// All code copyright (c) 2016 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 29 03 2016 at 16:54 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using Ophidian.Losgap.Audio;

namespace Egodystonic.EscapeLizards {
	public enum HUDSound {
		PostPassTime,
		PostPassStarBronze,
		PostPassStarSilver,
		PostPassStarGold,
		PostPassMedalBronze,
		PostPassMedalSilver,
		PostPassMedalGold,
		PostPassPersonalBest,
		PostPassLineReveal,
		PostPassLeaderboard,
		PostPassEggPop,
		PostPassSmashedEgg,
		PostPassIntactEgg,
		PostPassAllEggs,
		PostPassShowMenu,
		PostPassFreshCoin,
		PostPassFinalCoin,
		PostPassStaleCoin,
		UIPreviousOption,
		UINextOption,
		UISelectOption,
		UISelectNegativeOption,
		UIUnavailable,
		IntroReady,
		IntroGo,

		XMenuLoad,
		XAnnounceMain,
		XAnnounceWorldSelect,
		XAnnounceLevelSelect,
		XAnnounceMedals,
		XAnnounceOptions,
		XAnnounceExit,
	}

	public static class HUDSoundExtensions {
		public static string SoundFile(this HUDSound extended) {
			switch (extended) {
				case HUDSound.PostPassTime: return AssetLocator.PostPassTimeSound;
				case HUDSound.PostPassStarBronze: return AssetLocator.PostPassBronzeStarSound;
				case HUDSound.PostPassStarSilver: return AssetLocator.PostPassSilverStarSound;
				case HUDSound.PostPassStarGold: return AssetLocator.PostPassGoldStarSound;
				case HUDSound.PostPassMedalBronze: return AssetLocator.PostPassBronzeMedalSound;
				case HUDSound.PostPassMedalSilver: return AssetLocator.PostPassSilverMedalSound;
				case HUDSound.PostPassMedalGold: return AssetLocator.PostPassGoldMedalSound;
				case HUDSound.PostPassPersonalBest: return AssetLocator.PostPassPersonalBestSound;
				case HUDSound.PostPassLineReveal: return AssetLocator.PostPassLineRevealSound;
				case HUDSound.PostPassLeaderboard: return AssetLocator.PostPassLeaderboardSound;
				case HUDSound.PostPassEggPop: return AssetLocator.PostPassEggPopSound;
				case HUDSound.PostPassSmashedEgg: return AssetLocator.PostPassSmashedEggSound;
				case HUDSound.PostPassIntactEgg: return AssetLocator.PostPassIntactEggSound;
				case HUDSound.PostPassAllEggs: return AssetLocator.PostPassAllEggsSound;
				case HUDSound.PostPassShowMenu: return AssetLocator.PostPassShowMenuSound;
				case HUDSound.PostPassFreshCoin: return AssetLocator.CoinSound;
				case HUDSound.PostPassFinalCoin: return AssetLocator.FinalCoinSound;
				case HUDSound.PostPassStaleCoin: return AssetLocator.StaleCoinSound;
				case HUDSound.UINextOption: return AssetLocator.UIOptionChangeSound;
				case HUDSound.UIPreviousOption: return AssetLocator.UIOptionChangeSound;
				case HUDSound.UISelectOption: return AssetLocator.UIOptionSelectSound;
				case HUDSound.UISelectNegativeOption: return AssetLocator.UIOptionSelectNegativeSound;
				case HUDSound.UIUnavailable: return AssetLocator.UnavailableSound;
				case HUDSound.IntroReady: return AssetLocator.IntroReadySound;
				case HUDSound.IntroGo: return AssetLocator.IntroGoSound;

				case HUDSound.XMenuLoad: return AssetLocator.XMenuLoad;
				case HUDSound.XAnnounceMain: return AssetLocator.XAnnounceMain;
				case HUDSound.XAnnounceWorldSelect: return AssetLocator.XAnnounceWorldSelect;
				case HUDSound.XAnnounceLevelSelect: return AssetLocator.XAnnounceLevelSelect;
				case HUDSound.XAnnounceMedals: return AssetLocator.XAnnounceMedals;
				case HUDSound.XAnnounceOptions: return AssetLocator.XAnnounceOptions;
				case HUDSound.XAnnounceExit: return AssetLocator.XAnnounceExit;
				default: throw new KeyNotFoundException();
			}
		}

		public static float VolumeModifier(this HUDSound extended) {
			switch (extended) {
				case HUDSound.PostPassStaleCoin: return 0.33f;
				case HUDSound.PostPassTime: return 0.4f;
				case HUDSound.PostPassPersonalBest: return 0.85f;
				case HUDSound.PostPassLineReveal: return 0.5f;
				case HUDSound.PostPassSmashedEgg: return 0.5f;
				case HUDSound.PostPassIntactEgg: return 0.5f;
				case HUDSound.PostPassAllEggs: return 0.75f;
				case HUDSound.PostPassShowMenu: return 0.35f;
				case HUDSound.UINextOption:
				case HUDSound.UIPreviousOption:
					return 0.15f;
				case HUDSound.UISelectOption:
				case HUDSound.UISelectNegativeOption:
					return 0.4f;
				case HUDSound.PostPassFinalCoin:
					return 1.2f;
				default: return 1f;
			}
		}

		public static float PitchModifier(this HUDSound extended) {
			switch (extended) {
				default: return 1f;
			}
		}

		public static void Play(this HUDSound sound, float volume = 1f, float pitch = 1f) {
			int instanceId = AudioModule.CreateSoundInstance(sound.SoundFile());
			AudioModule.PlaySoundInstance(
				instanceId,
				false,
				Config.HUDVolume * sound.VolumeModifier() * volume,
				sound.PitchModifier() * pitch
				);
		}
	}
}