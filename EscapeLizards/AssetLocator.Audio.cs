// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 13 07 2015 at 17:32 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using Ophidian.Losgap.Entities;
using Ophidian.Losgap.Rendering;

namespace Egodystonic.EscapeLizards {
	public static partial class AssetLocator {
		private static string acuteBounceSound;
		private static string obtuseBounceSound;
		private static string[] allBounceSounds;
		private static LoopingSound rollSound;
		private static string[] impactSounds;
		private static string[] highSpeedUpSounds;
		private static string[] highSpeedDownSounds;
		private static string[] highSpeedParallelSounds;
		private static string[] emotePassSounds;
		private static string[] emoteFailSounds;
		private static string[] failAwwSounds;
		private static string failFallSound;
		private static string failTimeoutSound;
		private static LoopingSound countdownLoopSound;
		private static string passSound;
		private static string passGoldSound;
		private static string bellSound;
		private static string tickSound;
		private static string eggCrunchSound;
		private static string vultureCrySound;
		private static string worldFlipSound;
		private static string starFallSound;
		private static string postPassTimeSound;
		private static string postPassBronzeStarSound;
		private static string postPassSilverStarSound;
		private static string postPassGoldStarSound;
		private static string postPassBronzeMedalSound;
		private static string postPassSilverMedalSound;
		private static string postPassGoldMedalSound;
		private static string postPassPersonalBestSound;
		private static string postPassLineRevealSound;
		private static string postPassLeaderboardSound;
		private static string postPassShowMenuSound;
		private static string postPassEggPopSound;
		private static string postPassSmashedEggSound;
		private static string postPassAllEggsSound;
		private static string postPassIntactEggSound;
		private static string postPassStarSpawnSound;
		private static LoopingSound postPassGoldApplauseSound;
		private static LoopingSound postPassSilverApplauseSound;
		private static LoopingSound postPassBronzeApplauseSound;
		private static string uiOptionChangeSound;
		private static string uiOptionSelectSound;
		private static string uiOptionSelectNegativeSound;
		private static string introReadySound;
		private static string introGoSound;
		private static string unavailableSound;
		private static string hopSound;
		private static string coinSound;
		private static string finalCoinSound;
		private static string staleCoinSound;
		private static LoopingSound[] backgroundMusic;
		private static string xMenuLoad;
		private static string xAnnounceMain;
		private static string xAnnounceWorldSelect;
		private static string xAnnounceLevelSelect;
		private static string xAnnounceMedals;
		private static string xAnnounceOptions;
		private static string xAnnounceExit;
		private static string showHopSound;

		public static string AcuteBounceSound {
			get {
				lock (staticMutationLock) {
					return acuteBounceSound;
				}
			}
			set {
				lock (staticMutationLock) {
					acuteBounceSound = value;
				}
			}
		}

		public static string ObtuseBounceSound {
			get {
				lock (staticMutationLock) {
					return obtuseBounceSound;
				}
			}
			set {
				lock (staticMutationLock) {
					obtuseBounceSound = value;
				}
			}
		}

		public static string[] AllBounceSounds {
			get {
				lock (staticMutationLock) {
					return allBounceSounds;
				}
			}
			set {
				lock (staticMutationLock) {
					allBounceSounds = value;
				}
			}
		}

		public static LoopingSound RollSound {
			get {
				lock (staticMutationLock) {
					return rollSound;
				}
			}
			set {
				lock (staticMutationLock) {
					rollSound = value;
				}
			}
		}

		public static string[] ImpactSounds {
			get {
				lock (staticMutationLock) {
					return impactSounds;
				}
			}
			set {
				lock (staticMutationLock) {
					impactSounds = value;
				}
			}
		}

		public static string[] HighSpeedUpSounds {
			get {
				lock (staticMutationLock) {
					return highSpeedUpSounds;
				}
			}
			set {
				lock (staticMutationLock) {
					highSpeedUpSounds = value;
				}
			}
		}

		public static string[] HighSpeedDownSounds {
			get {
				lock (staticMutationLock) {
					return highSpeedDownSounds;
				}
			}
			set {
				lock (staticMutationLock) {
					highSpeedDownSounds = value;
				}
			}
		}

		public static string[] HighSpeedParallelSounds {
			get {
				lock (staticMutationLock) {
					return highSpeedParallelSounds;
				}
			}
			set {
				lock (staticMutationLock) {
					highSpeedParallelSounds = value;
				}
			}
		}

		public static string[] EmotePassSounds {
			get {
				lock (staticMutationLock) {
					return emotePassSounds;
				}
			}
			set {
				lock (staticMutationLock) {
					emotePassSounds = value;
				}
			}
		}

		public static string[] EmoteFailSounds {
			get {
				lock (staticMutationLock) {
					return emoteFailSounds;
				}
			}
			set {
				lock (staticMutationLock) {
					emoteFailSounds = value;
				}
			}
		}

		public static string[] FailAwwSounds {
			get {
				lock (staticMutationLock) {
					return failAwwSounds;
				}
			}
			set {
				lock (staticMutationLock) {
					failAwwSounds = value;
				}
			}
		}

		public static string FailFallSound {
			get {
				lock (staticMutationLock) {
					return failFallSound;
				}
			}
			set {
				lock (staticMutationLock) {
					failFallSound = value;
				}
			}
		}

		public static string FailTimeoutSound {
			get {
				lock (staticMutationLock) {
					return failTimeoutSound;
				}
			}
			set {
				lock (staticMutationLock) {
					failTimeoutSound = value;
				}
			}
		}

		public static LoopingSound CountdownLoopSound {
			get {
				lock (staticMutationLock) {
					return countdownLoopSound;
				}
			}
			set {
				lock (staticMutationLock) {
					countdownLoopSound = value;
				}
			}
		}

		public static string PassSound {
			get {
				lock (staticMutationLock) {
					return passSound;
				}
			}
			set {
				lock (staticMutationLock) {
					passSound = value;
				}
			}
		}

		public static string PassGoldSound {
			get {
				lock (staticMutationLock) {
					return passGoldSound;
				}
			}
			set {
				lock (staticMutationLock) {
					passGoldSound = value;
				}
			}
		}

		public static string BellSound {
			get {
				lock (staticMutationLock) {
					return bellSound;
				}
			}
			set {
				lock (staticMutationLock) {
					bellSound = value;
				}
			}
		}

		public static string TickSound {
			get {
				lock (staticMutationLock) {
					return tickSound;
				}
			}
			set {
				lock (staticMutationLock) {
					tickSound = value;
				}
			}
		}

		public static string EggCrunchSound {
			get {
				lock (staticMutationLock) {
					return eggCrunchSound;
				}
			}
			set {
				lock (staticMutationLock) {
					eggCrunchSound = value;
				}
			}
		}

		public static string VultureCrySound {
			get {
				lock (staticMutationLock) {
					return vultureCrySound;
				}
			}
			set {
				lock (staticMutationLock) {
					vultureCrySound = value;
				}
			}
		}

		public static string WorldFlipSound {
			get {
				lock (staticMutationLock) {
					return worldFlipSound;
				}
			}
			set {
				lock (staticMutationLock) {
					worldFlipSound = value;
				}
			}
		}

		public static string StarFallSound {
			get {
				lock (staticMutationLock) {
					return starFallSound;
				}
			}
			set {
				lock (staticMutationLock) {
					starFallSound = value;
				}
			}
		}

		public static string PostPassTimeSound {
			get {
				lock (staticMutationLock) {
					return postPassTimeSound;
				}
			}
			set {
				lock (staticMutationLock) {
					postPassTimeSound = value;
				}
			}
		}

		public static string PostPassBronzeStarSound {
			get {
				lock (staticMutationLock) {
					return postPassBronzeStarSound;
				}
			}
			set {
				lock (staticMutationLock) {
					postPassBronzeStarSound = value;
				}
			}
		}
		public static string PostPassSilverStarSound {
			get {
				lock (staticMutationLock) {
					return postPassSilverStarSound;
				}
			}
			set {
				lock (staticMutationLock) {
					postPassSilverStarSound = value;
				}
			}
		}
		public static string PostPassGoldStarSound {
			get {
				lock (staticMutationLock) {
					return postPassGoldStarSound;
				}
			}
			set {
				lock (staticMutationLock) {
					postPassGoldStarSound = value;
				}
			}
		}

		public static string PostPassBronzeMedalSound {
			get {
				lock (staticMutationLock) {
					return postPassBronzeMedalSound;
				}
			}
			set {
				lock (staticMutationLock) {
					postPassBronzeMedalSound = value;
				}
			}
		}
		public static string PostPassSilverMedalSound {
			get {
				lock (staticMutationLock) {
					return postPassSilverMedalSound;
				}
			}
			set {
				lock (staticMutationLock) {
					postPassSilverMedalSound = value;
				}
			}
		}
		public static string PostPassGoldMedalSound {
			get {
				lock (staticMutationLock) {
					return postPassGoldMedalSound;
				}
			}
			set {
				lock (staticMutationLock) {
					postPassGoldMedalSound = value;
				}
			}
		}

		public static string PostPassPersonalBestSound {
			get {
				lock (staticMutationLock) {
					return postPassPersonalBestSound;
				}
			}
			set {
				lock (staticMutationLock) {
					postPassPersonalBestSound = value;
				}
			}
		}

		public static string PostPassLineRevealSound {
			get {
				lock (staticMutationLock) {
					return postPassLineRevealSound;
				}
			}
			set {
				lock (staticMutationLock) {
					postPassLineRevealSound = value;
				}
			}
		}

		public static string PostPassLeaderboardSound {
			get {
				lock (staticMutationLock) {
					return postPassLeaderboardSound;
				}
			}
			set {
				lock (staticMutationLock) {
					postPassLeaderboardSound = value;
				}
			}
		}

		public static string PostPassShowMenuSound {
			get {
				lock (staticMutationLock) {
					return postPassShowMenuSound;
				}
			}
			set {
				lock (staticMutationLock) {
					postPassShowMenuSound = value;
				}
			}
		}

		public static string PostPassEggPopSound {
			get {
				lock (staticMutationLock) {
					return postPassEggPopSound;
				}
			}
			set {
				lock (staticMutationLock) {
					postPassEggPopSound = value;
				}
			}
		}

		public static string PostPassSmashedEggSound {
			get {
				lock (staticMutationLock) {
					return postPassSmashedEggSound;
				}
			}
			set {
				lock (staticMutationLock) {
					postPassSmashedEggSound = value;
				}
			}
		}

		public static string PostPassIntactEggSound {
			get {
				lock (staticMutationLock) {
					return postPassIntactEggSound;
				}
			}
			set {
				lock (staticMutationLock) {
					postPassIntactEggSound = value;
				}
			}
		}

		public static string PostPassAllEggsSound {
			get {
				lock (staticMutationLock) {
					return postPassAllEggsSound;
				}
			}
			set {
				lock (staticMutationLock) {
					postPassAllEggsSound = value;
				}
			}
		}

		public static string PostPassStarSpawnSound {
			get {
				lock (staticMutationLock) {
					return postPassStarSpawnSound;
				}
			}
			set {
				lock (staticMutationLock) {
					postPassStarSpawnSound = value;
				}
			}
		}

		public static LoopingSound PostPassGoldApplauseSound
		{
			get {
				lock (staticMutationLock) {
					return postPassGoldApplauseSound;
				}
			}
			set {
				lock (staticMutationLock) {
					postPassGoldApplauseSound = value;
				}
			}
		}

		public static LoopingSound PostPassSilverApplauseSound {
			get {
				lock (staticMutationLock) {
					return postPassSilverApplauseSound;
				}
			}
			set {
				lock (staticMutationLock) {
					postPassSilverApplauseSound = value;
				}
			}
		}

		public static LoopingSound PostPassBronzeApplauseSound {
			get {
				lock (staticMutationLock) {
					return postPassBronzeApplauseSound;
				}
			}
			set {
				lock (staticMutationLock) {
					postPassBronzeApplauseSound = value;
				}
			}
		}

		public static string UIOptionChangeSound {
			get {
				lock (staticMutationLock) {
					return uiOptionChangeSound;
				}
			}
			set {
				lock (staticMutationLock) {
					uiOptionChangeSound = value;
				}
			}
		}

		public static string UIOptionSelectSound {
			get {
				lock (staticMutationLock) {
					return uiOptionSelectSound;
				}
			}
			set {
				lock (staticMutationLock) {
					uiOptionSelectSound = value;
				}
			}
		}

		public static string UIOptionSelectNegativeSound {
			get {
				lock (staticMutationLock) {
					return uiOptionSelectNegativeSound;
				}
			}
			set {
				lock (staticMutationLock) {
					uiOptionSelectNegativeSound = value;
				}
			}
		}

		public static string IntroReadySound {
			get {
				lock (staticMutationLock) {
					return introReadySound;
				}
			}
			set {
				lock (staticMutationLock) {
					introReadySound = value;
				}
			}
		}

		public static string IntroGoSound {
			get {
				lock (staticMutationLock) {
					return introGoSound;
				}
			}
			set {
				lock (staticMutationLock) {
					introGoSound = value;
				}
			}
		}

		public static string UnavailableSound {
			get {
				lock (staticMutationLock) {
					return unavailableSound;
				}
			}
			set {
				lock (staticMutationLock) {
					unavailableSound = value;
				}
			}
		}

		public static string HopSound {
			get {
				lock (staticMutationLock) {
					return hopSound;
				}
			}
			set {
				lock (staticMutationLock) {
					hopSound = value;
				}
			}
		}

		public static string CoinSound {
			get {
				lock (staticMutationLock) {
					return coinSound;
				}
			}
			set {
				lock (staticMutationLock) {
					coinSound = value;
				}
			}
		}

		public static string FinalCoinSound {
			get {
				lock (staticMutationLock) {
					return finalCoinSound;
				}
			}
			set {
				lock (staticMutationLock) {
					finalCoinSound = value;
				}
			}
		}

		public static string StaleCoinSound {
			get {
				lock (staticMutationLock) {
					return staleCoinSound;
				}
			}
			set {
				lock (staticMutationLock) {
					staleCoinSound = value;
				}
			}
		}

		public static LoopingSound[] BackgroundMusic {
			get {
				lock (staticMutationLock) {
					return backgroundMusic;
				}
			}
			set {
				lock (staticMutationLock) {
					backgroundMusic = value;
				}
			}
		}

		public static string XMenuLoad {
			get {
				lock (staticMutationLock) {
					return xMenuLoad;
				}
			}
			set {
				lock (staticMutationLock) {
					xMenuLoad = value;
				}
			}
		}

		public static string XAnnounceMain {
			get {
				lock (staticMutationLock) {
					return xAnnounceMain;
				}
			}
			set {
				lock (staticMutationLock) {
					xAnnounceMain = value;
				}
			}
		}

		public static string XAnnounceWorldSelect {
			get {
				lock (staticMutationLock) {
					return xAnnounceWorldSelect;
				}
			}
			set {
				lock (staticMutationLock) {
					xAnnounceWorldSelect = value;
				}
			}
		}

		public static string XAnnounceLevelSelect {
			get {
				lock (staticMutationLock) {
					return xAnnounceLevelSelect;
				}
			}
			set {
				lock (staticMutationLock) {
					xAnnounceLevelSelect = value;
				}
			}
		}

		public static string XAnnounceMedals {
			get {
				lock (staticMutationLock) {
					return xAnnounceMedals;
				}
			}
			set {
				lock (staticMutationLock) {
					xAnnounceMedals = value;
				}
			}
		}

		public static string XAnnounceOptions {
			get {
				lock (staticMutationLock) {
					return xAnnounceOptions;
				}
			}
			set {
				lock (staticMutationLock) {
					xAnnounceOptions = value;
				}
			}
		}

		public static string XAnnounceExit {
			get {
				lock (staticMutationLock) {
					return xAnnounceExit;
				}
			}
			set {
				lock (staticMutationLock) {
					xAnnounceExit = value;
				}
			}
		}

		public static string ShowHopSound {
			get {
				lock (staticMutationLock) {
					return showHopSound;
				}
			}
			set {
				lock (staticMutationLock) {
					showHopSound = value;
				}
			}
		}
	}
}