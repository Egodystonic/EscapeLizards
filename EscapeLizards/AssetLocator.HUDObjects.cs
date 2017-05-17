// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 13 07 2015 at 17:32 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Ophidian.Losgap.Entities;
using Ophidian.Losgap.Rendering;

namespace Egodystonic.EscapeLizards {
	public static partial class AssetLocator {
		private static ITexture2D eggCounterBackdrop;
		private static ITexture2D vultureEgg;
		private static ITexture2D vultureEggInvert;
		private static ITexture2D vultureEggDimmed;
		private static ITexture2D vultureEggCross;
		private static ITexture2D timerBackdrop;
		private static ITexture2D timerBackdropGlass;
		private static ITexture2D[] timerFigures;
		private static ITexture2D levelNameBackdrop;
		private static ITexture2D bronzeStarTimeBackdrop;
		private static ITexture2D silverStarTimeBackdrop;
		private static ITexture2D goldStarTimeBackdrop;
		private static ITexture2D bronzeStar;
		private static ITexture2D silverStar;
		private static ITexture2D goldStar;
		private static ITexture2D personalBestTimeBackdrop;
		private static ITexture2D friendsGoldTimeBackdrop;
		private static ITexture2D friendsSilverTimeBackdrop;
		private static ITexture2D friendsBronzeTimeBackdrop;
		private static ITexture2D finishingBell;
		private static ITexture2D friendsGoldTimeToken;
		private static ITexture2D friendsSilverTimeToken;
		private static ITexture2D friendsBronzeTimeToken;
		private static ITexture2D passMessageTex;
		private static ITexture2D failMessageTex;
		private static ITexture2D timeUpMessageTex;
		private static ITexture2D droppedMessageTex;
		private static ITexture2D readyTex;
		private static ITexture2D goTex;
		private static ITexture2D coinBackdrop;
		private static ITexture2D[] coinFrames;
		private static ITexture2D elLogo;
		private static ITexture2D mainMenuPlayButton;
		private static ITexture2D mainMenuMedalsButton;
		private static ITexture2D mainMenuOptionsButton;
		private static ITexture2D mainMenuExitButton;
		private static ITexture2D mainMenuPlayButtonFront;
		private static ITexture2D mainMenuMedalsButtonFront;
		private static ITexture2D mainMenuOptionsButtonFront;
		private static ITexture2D mainMenuExitButtonFront;
		private static ITexture2D mainMenuButtonRing;
		private static ITexture2D lineSeparator;
		private static ITexture2D lineSelectionFill;
		private static ITexture2D chevrons;
		private static ITexture2D[] worldIcons;
		private static ITexture2D[] worldIconsGreyed;
		private static ITexture2D[] lockedWorldIcons;
		private static ITexture2D[] unlockedWorldIcons;
		private static ITexture2D playMenuMainPanel;
		private static ITexture2D playMenuSecondaryPanel;
		private static ITexture2D playMenuLevelBar;
		private static ITexture2D playMenuLevelBarHighlight;
		private static ITexture2D playMenuWorldButtonHighlight;
		private static ITexture2D playMenuWorldButtonUnlockedHighlight;
		private static ITexture2D starIndent;
		private static ITexture2D eggIndent;
		private static ITexture2D coinIndent;
		private static ITexture2D previewImageBracket;
		private static ITexture2D previewIndent;
		private static ITexture2D loadingScreen;
		private static ITexture2D medalLevelBackdrop;
		private static ITexture2D white1x1;
		private static ITexture2D tutorialArrow;
		private static ITexture2D hopArrow;
		private static ITexture2D tick;
		private static ITexture2D introGoalIndicatorCircle;
		private static ITexture2D introGoalIndicatorArrow;
		private static ITexture2D introSpawnIndicatorCircle;
		private static ITexture2D introSpawnIndicatorArrow;
		private static ITexture2D reloadingIcon;
		private static ITexture2D difficultyEasyIcon;
		private static ITexture2D difficultyTrickyIcon;
		private static ITexture2D difficultyHardIcon;
		private static ITexture2D difficultyVeryHardIcon;

		public static ITexture2D EggCounterBackdrop {
			get {
				lock (staticMutationLock) {
					return eggCounterBackdrop;
				}
			}
			set {
				lock (staticMutationLock) {
					eggCounterBackdrop = value;
				}
			}
		}

		public static ITexture2D VultureEgg {
			get {
				lock (staticMutationLock) {
					return vultureEgg;
				}
			}
			set {
				lock (staticMutationLock) {
					vultureEgg = value;
				}
			}
		}

		public static ITexture2D VultureEggInvert {
			get {
				lock (staticMutationLock) {
					return vultureEggInvert;
				}
			}
			set {
				lock (staticMutationLock) {
					vultureEggInvert = value;
				}
			}
		}

		public static ITexture2D VultureEggDimmed {
			get {
				lock (staticMutationLock) {
					return vultureEggDimmed;
				}
			}
			set {
				lock (staticMutationLock) {
					vultureEggDimmed = value;
				}
			}
		}

		public static ITexture2D VultureEggCross {
			get {
				lock (staticMutationLock) {
					return vultureEggCross;
				}
			}
			set {
				lock (staticMutationLock) {
					vultureEggCross = value;
				}
			}
		}

		public static ITexture2D TimerBackdrop {
			get {
				lock (staticMutationLock) {
					return timerBackdrop;
				}
			}
			set {
				lock (staticMutationLock) {
					timerBackdrop = value;
				}
			}
		}

		public static ITexture2D TimerBackdropGlass {
			get {
				lock (staticMutationLock) {
					return timerBackdropGlass;
				}
			}
			set {
				lock (staticMutationLock) {
					timerBackdropGlass = value;
				}
			}
		}

		public static ITexture2D[] TimerFigures {
			get {
				lock (staticMutationLock) {
					return timerFigures;
				}
			}
			set {
				lock (staticMutationLock) {
					timerFigures = value;
				}
			}
		}

		// [9 to 8][8 to 7][..][0 to 9]; 15 in each block, 10 blocks in total
		public static ITexture2D GetTimerFigure(int startingFigure, float distanceToNext) {
			var actualIndex = GetTimerFigureIndex(startingFigure, distanceToNext);
			lock (staticMutationLock) {
				return timerFigures[actualIndex];
			}
		}

		public static int GetTimerFigureIndex(int startingFigure, float distanceToNext) {
			while (distanceToNext >= 1f) {
				--startingFigure;
				if (startingFigure < 0) startingFigure = 9;
				distanceToNext -= 1f;
			}
			int startingIndex = (9 - startingFigure) * 15;
			return startingIndex + (int) (15f * distanceToNext);
		}

		public static ITexture2D LevelNameBackdrop {
			get {
				lock (staticMutationLock) {
					return levelNameBackdrop;
				}
			}
			set {
				lock (staticMutationLock) {
					levelNameBackdrop = value;
				}
			}
		}

		public static ITexture2D BronzeStarTimeBackdrop {
			get {
				lock (staticMutationLock) {
					return bronzeStarTimeBackdrop;
				}
			}
			set {
				lock (staticMutationLock) {
					bronzeStarTimeBackdrop = value;
				}
			}
		}

		public static ITexture2D SilverStarTimeBackdrop {
			get {
				lock (staticMutationLock) {
					return silverStarTimeBackdrop;
				}
			}
			set {
				lock (staticMutationLock) {
					silverStarTimeBackdrop = value;
				}
			}
		}

		public static ITexture2D GoldStarTimeBackdrop {
			get {
				lock (staticMutationLock) {
					return goldStarTimeBackdrop;
				}
			}
			set {
				lock (staticMutationLock) {
					goldStarTimeBackdrop = value;
				}
			}
		}

		public static ITexture2D BronzeStar {
			get {
				lock (staticMutationLock) {
					return bronzeStar;
				}
			}
			set {
				lock (staticMutationLock) {
					bronzeStar = value;
				}
			}
		}

		public static ITexture2D SilverStar {
			get {
				lock (staticMutationLock) {
					return silverStar;
				}
			}
			set {
				lock (staticMutationLock) {
					silverStar = value;
				}
			}
		}

		public static ITexture2D GoldStar {
			get {
				lock (staticMutationLock) {
					return goldStar;
				}
			}
			set {
				lock (staticMutationLock) {
					goldStar = value;
				}
			}
		}

		public static ITexture2D PersonalBestTimeBackdrop {
			get {
				lock (staticMutationLock) {
					return personalBestTimeBackdrop;
				}
			}
			set {
				lock (staticMutationLock) {
					personalBestTimeBackdrop = value;
				}
			}
		}

		public static ITexture2D FriendsGoldTimeBackdrop {
			get {
				lock (staticMutationLock) {
					return friendsGoldTimeBackdrop;
				}
			}
			set {
				lock (staticMutationLock) {
					friendsGoldTimeBackdrop = value;
				}
			}
		}

		public static ITexture2D FriendsSilverTimeBackdrop {
			get {
				lock (staticMutationLock) {
					return friendsSilverTimeBackdrop;
				}
			}
			set {
				lock (staticMutationLock) {
					friendsSilverTimeBackdrop = value;
				}
			}
		}

		public static ITexture2D FriendsBronzeTimeBackdrop {
			get {
				lock (staticMutationLock) {
					return friendsBronzeTimeBackdrop;
				}
			}
			set {
				lock (staticMutationLock) {
					friendsBronzeTimeBackdrop = value;
				}
			}
		}

		public static ITexture2D FinishingBell {
			get {
				lock (staticMutationLock) {
					return finishingBell;
				}
			}
			set {
				lock (staticMutationLock) {
					finishingBell = value;
				}
			}
		}

		public static ITexture2D FriendsGoldTimeToken {
			get {
				lock (staticMutationLock) {
					return friendsGoldTimeToken;
				}
			}
			set {
				lock (staticMutationLock) {
					friendsGoldTimeToken = value;
				}
			}
		}

		public static ITexture2D FriendsSilverTimeToken {
			get {
				lock (staticMutationLock) {
					return friendsSilverTimeToken;
				}
			}
			set {
				lock (staticMutationLock) {
					friendsSilverTimeToken = value;
				}
			}
		}

		public static ITexture2D FriendsBronzeTimeToken {
			get {
				lock (staticMutationLock) {
					return friendsBronzeTimeToken;
				}
			}
			set {
				lock (staticMutationLock) {
					friendsBronzeTimeToken = value;
				}
			}
		}

		public static ITexture2D PassMessageTex {
			get {
				lock (staticMutationLock) {
					return passMessageTex;
				}
			}
			set {
				lock (staticMutationLock) {
					passMessageTex = value;
				}
			}
		}

		public static ITexture2D FailMessageTex {
			get {
				lock (staticMutationLock) {
					return failMessageTex;
				}
			}
			set {
				lock (staticMutationLock) {
					failMessageTex = value;
				}
			}
		}

		public static ITexture2D DroppedMessageTex {
			get {
				lock (staticMutationLock) {
					return droppedMessageTex;
				}
			}
			set {
				lock (staticMutationLock) {
					droppedMessageTex = value;
				}
			}
		}

		public static ITexture2D TimeUpMessageTex {
			get {
				lock (staticMutationLock) {
					return timeUpMessageTex;
				}
			}
			set {
				lock (staticMutationLock) {
					timeUpMessageTex = value;
				}
			}
		}

		public static ITexture2D ReadyTex {
			get {
				lock (staticMutationLock) {
					return readyTex;
				}
			}
			set {
				lock (staticMutationLock) {
					readyTex = value;
				}
			}
		}

		public static ITexture2D GoTex {
			get {
				lock (staticMutationLock) {
					return goTex;
				}
			}
			set {
				lock (staticMutationLock) {
					goTex = value;
				}
			}
		}

		public static ITexture2D CoinBackdrop {
			get {
				lock (staticMutationLock) {
					return coinBackdrop;
				}
			}
			set {
				lock (staticMutationLock) {
					coinBackdrop = value;
				}
			}
		}

		public static ITexture2D[] CoinFrames {
			get {
				lock (staticMutationLock) {
					return coinFrames;
				}
			}
			set {
				lock (staticMutationLock) {
					coinFrames = value;
				}
			}
		}

		public static ITexture2D ELLogo {
			get {
				lock (staticMutationLock) {
					return elLogo;
				}
			}
			set {
				lock (staticMutationLock) {
					elLogo = value;
				}
			}
		}

		public static ITexture2D MainMenuPlayButton {
			get {
				lock (staticMutationLock) {
					return mainMenuPlayButton;
				}
			}
			set {
				lock (staticMutationLock) {
					mainMenuPlayButton = value;
				}
			}
		}

		public static ITexture2D MainMenuMedalsButton {
			get {
				lock (staticMutationLock) {
					return mainMenuMedalsButton;
				}
			}
			set {
				lock (staticMutationLock) {
					mainMenuMedalsButton = value;
				}
			}
		}

		public static ITexture2D MainMenuOptionsButton {
			get {
				lock (staticMutationLock) {
					return mainMenuOptionsButton;
				}
			}
			set {
				lock (staticMutationLock) {
					mainMenuOptionsButton = value;
				}
			}
		}

		public static ITexture2D MainMenuExitButton {
			get {
				lock (staticMutationLock) {
					return mainMenuExitButton;
				}
			}
			set {
				lock (staticMutationLock) {
					mainMenuExitButton = value;
				}
			}
		}

		public static ITexture2D MainMenuPlayButtonFront {
			get {
				lock (staticMutationLock) {
					return mainMenuPlayButtonFront;
				}
			}
			set {
				lock (staticMutationLock) {
					mainMenuPlayButtonFront = value;
				}
			}
		}

		public static ITexture2D MainMenuMedalsButtonFront {
			get {
				lock (staticMutationLock) {
					return mainMenuMedalsButtonFront;
				}
			}
			set {
				lock (staticMutationLock) {
					mainMenuMedalsButtonFront = value;
				}
			}
		}

		public static ITexture2D MainMenuOptionsButtonFront {
			get {
				lock (staticMutationLock) {
					return mainMenuOptionsButtonFront;
				}
			}
			set {
				lock (staticMutationLock) {
					mainMenuOptionsButtonFront = value;
				}
			}
		}

		public static ITexture2D MainMenuExitButtonFront {
			get {
				lock (staticMutationLock) {
					return mainMenuExitButtonFront;
				}
			}
			set {
				lock (staticMutationLock) {
					mainMenuExitButtonFront = value;
				}
			}
		}

		public static ITexture2D MainMenuButtonRing {
			get {
				lock (staticMutationLock) {
					return mainMenuButtonRing;
				}
			}
			set {
				lock (staticMutationLock) {
					mainMenuButtonRing = value;
				}
			}
		}

		public static ITexture2D LineSeparator {
			get {
				lock (staticMutationLock) {
					return lineSeparator;
				}
			}
			set {
				lock (staticMutationLock) {
					lineSeparator = value;
				}
			}
		}

		public static ITexture2D LineSelectionFill {
			get {
				lock (staticMutationLock) {
					return lineSelectionFill;
				}
			}
			set {
				lock (staticMutationLock) {
					lineSelectionFill = value;
				}
			}
		}

		public static ITexture2D Chevrons {
			get {
				lock (staticMutationLock) {
					return chevrons;
				}
			}
			set {
				lock (staticMutationLock) {
					chevrons = value;
				}
			}
		}

		public static ITexture2D[] WorldIcons {
			get {
				lock (staticMutationLock) {
					return worldIcons;
				}
			}
			set {
				lock (staticMutationLock) {
					worldIcons = value;
				}
			}
		}

		public static ITexture2D[] WorldIconsGreyed {
			get {
				lock (staticMutationLock) {
					return worldIconsGreyed;
				}
			}
			set {
				lock (staticMutationLock) {
					worldIconsGreyed = value;
				}
			}
		}

		public static ITexture2D[] LockedWorldIcons {
			get {
				lock (staticMutationLock) {
					return lockedWorldIcons;
				}
			}
			set {
				lock (staticMutationLock) {
					lockedWorldIcons = value;
				}
			}
		}

		public static ITexture2D[] UnlockedWorldIcons {
			get {
				lock (staticMutationLock) {
					return unlockedWorldIcons;
				}
			}
			set {
				lock (staticMutationLock) {
					unlockedWorldIcons = value;
				}
			}
		}

		public static ITexture2D PlayMenuMainPanel {
			get {
				lock (staticMutationLock) {
					return playMenuMainPanel;
				}
			}
			set {
				lock (staticMutationLock) {
					playMenuMainPanel = value;
				}
			}
		}

		public static ITexture2D PlayMenuSecondaryPanel {
			get {
				lock (staticMutationLock) {
					return playMenuSecondaryPanel;
				}
			}
			set {
				lock (staticMutationLock) {
					playMenuSecondaryPanel = value;
				}
			}
		}

		public static ITexture2D PlayMenuLevelBar {
			get {
				lock (staticMutationLock) {
					return playMenuLevelBar;
				}
			}
			set {
				lock (staticMutationLock) {
					playMenuLevelBar = value;
				}
			}
		}

		public static ITexture2D PlayMenuLevelBarHighlight {
			get {
				lock (staticMutationLock) {
					return playMenuLevelBarHighlight;
				}
			}
			set {
				lock (staticMutationLock) {
					playMenuLevelBarHighlight = value;
				}
			}
		}

		public static ITexture2D PlayMenuWorldButtonHighlight {
			get {
				lock (staticMutationLock) {
					return playMenuWorldButtonHighlight;
				}
			}
			set {
				lock (staticMutationLock) {
					playMenuWorldButtonHighlight = value;
				}
			}
		}

		public static ITexture2D PlayMenuWorldButtonUnlockedHighlight {
			get {
				lock (staticMutationLock) {
					return playMenuWorldButtonUnlockedHighlight;
				}
			}
			set {
				lock (staticMutationLock) {
					playMenuWorldButtonUnlockedHighlight = value;
				}
			}
		}

		public static ITexture2D StarIndent {
			get {
				lock (staticMutationLock) {
					return starIndent;
				}
			}
			set {
				lock (staticMutationLock) {
					starIndent = value;
				}
			}
		}

		public static ITexture2D EggIndent {
			get {
				lock (staticMutationLock) {
					return eggIndent;
				}
			}
			set {
				lock (staticMutationLock) {
					eggIndent = value;
				}
			}
		}

		public static ITexture2D CoinIndent {
			get {
				lock (staticMutationLock) {
					return coinIndent;
				}
			}
			set {
				lock (staticMutationLock) {
					coinIndent = value;
				}
			}
		}

		public static ITexture2D PreviewImageBracket {
			get {
				lock (staticMutationLock) {
					return previewImageBracket;
				}
			}
			set {
				lock (staticMutationLock) {
					previewImageBracket = value;
				}
			}
		}

		public static ITexture2D PreviewIndent {
			get {
				lock (staticMutationLock) {
					return previewIndent;
				}
			}
			set {
				lock (staticMutationLock) {
					previewIndent = value;
				}
			}
		}

		public static ITexture2D LoadingScreen {
			get {
				lock (staticMutationLock) {
					return loadingScreen;
				}
			}
			set {
				lock (staticMutationLock) {
					loadingScreen = value;
				}
			}
		}

		public static ITexture2D MedalLevelBackdrop {
			get {
				lock (staticMutationLock) {
					return medalLevelBackdrop;
				}
			}
			set {
				lock (staticMutationLock) {
					medalLevelBackdrop = value;
				}
			}
		}

		public static ITexture2D White1x1 {
			get {
				lock (staticMutationLock) {
					return white1x1;
				}
			}
			set {
				lock (staticMutationLock) {
					white1x1 = value;
				}
			}
		}

		public static ITexture2D TutorialArrow {
			get {
				lock (staticMutationLock) {
					return tutorialArrow;
				}
			}
			set {
				lock (staticMutationLock) {
					tutorialArrow = value;
				}
			}
		}

		public static ITexture2D HopArrow {
			get {
				lock (staticMutationLock) {
					return hopArrow;
				}
			}
			set {
				lock (staticMutationLock) {
					hopArrow = value;
				}
			}
		}

		public static ITexture2D Tick {
			get {
				lock (staticMutationLock) {
					return tick;
				}
			}
			set {
				lock (staticMutationLock) {
					tick = value;
				}
			}
		}

		public static ITexture2D IntroGoalIndicatorCircle {
			get {
				lock (staticMutationLock) {
					return introGoalIndicatorCircle;
				}
			}
			set {
				lock (staticMutationLock) {
					introGoalIndicatorCircle = value;
				}
			}
		}

		public static ITexture2D IntroGoalIndicatorArrow {
			get {
				lock (staticMutationLock) {
					return introGoalIndicatorArrow;
				}
			}
			set {
				lock (staticMutationLock) {
					introGoalIndicatorArrow = value;
				}
			}
		}

		public static ITexture2D IntroSpawnIndicatorCircle {
			get {
				lock (staticMutationLock) {
					return introSpawnIndicatorCircle;
				}
			}
			set {
				lock (staticMutationLock) {
					introSpawnIndicatorCircle = value;
				}
			}
		}

		public static ITexture2D IntroSpawnIndicatorArrow {
			get {
				lock (staticMutationLock) {
					return introSpawnIndicatorArrow;
				}
			}
			set {
				lock (staticMutationLock) {
					introSpawnIndicatorArrow = value;
				}
			}
		}

		public static ITexture2D ReloadingIcon {
			get {
				lock (staticMutationLock) {
					return reloadingIcon;
				}
			}
			set {
				lock (staticMutationLock) {
					reloadingIcon = value;
				}
			}
		}

		public static ITexture2D DifficultyEasyIcon {
			get {
				lock (staticMutationLock) {
					return difficultyEasyIcon;
				}
			}
			set {
				lock (staticMutationLock) {
					difficultyEasyIcon = value;
				}
			}
		}

		public static ITexture2D DifficultyTrickyIcon {
			get {
				lock (staticMutationLock) {
					return difficultyTrickyIcon;
				}
			}
			set {
				lock (staticMutationLock) {
					difficultyTrickyIcon = value;
				}
			}
		}

		public static ITexture2D DifficultyHardIcon {
			get {
				lock (staticMutationLock) {
					return difficultyHardIcon;
				}
			}
			set {
				lock (staticMutationLock) {
					difficultyHardIcon = value;
				}
			}
		}

		public static ITexture2D DifficultyVeryHardIcon {
			get {
				lock (staticMutationLock) {
					return difficultyVeryHardIcon;
				}
			}
			set {
				lock (staticMutationLock) {
					difficultyVeryHardIcon = value;
				}
			}
		}
	}
}