// All code copyright (c) 2016 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 20 07 2016 at 15:14 by Ben Bowen

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using FMOD;
using Ophidian.Losgap;
using Ophidian.Losgap.Entities;
using Ophidian.Losgap.Rendering;

namespace Egodystonic.EscapeLizards {
	public static partial class MenuCoordinator {
		private const float PLAY_MENU_TRANSITION_IN_TIME = 0.3f;
		private const float PLAY_MENU_TRANSITION_OUT_TIME = 0.2f;
		private static readonly HUDTexture[] playMenuWorldSelectIcons = new HUDTexture[11]; // TODO replace with constant in LevelDatabase (and all occurrences in solution of '11')
		private static HUDTexture playMenuWorldIconHighlight;
		private static HUDItemExciterEntity playMenuWorldIconHighlightExciter;
		private static int playMenuSelectedWorldIndex;
		private static int playMenuSelectedLevelIndex;
		private const float PLAY_MENU_WORLD_ICON_COLUMN_A_X = 0.03f;
		private const float PLAY_MENU_WORLD_ICON_COLUMN_B_X = 0.15f;
		private const float PLAY_MENU_WORLD_ICON_COLUMN_C_X = PLAY_MENU_WORLD_ICON_COLUMN_B_X + (PLAY_MENU_WORLD_ICON_COLUMN_B_X - PLAY_MENU_WORLD_ICON_COLUMN_A_X);
		private const float PLAY_MENU_WORLD_ICON_WORLD_SELECT_X_OFFSET = -0.155f * 2f;
		private const float PLAY_MENU_WORLD_ICON_ROW_Y = 0.18f;
		private const float PLAY_MENU_WORLD_ICON_Y_START = 0.16f;
		private static readonly Vector2 PLAY_MENU_WORLD_ICON_SCALE = new Vector2(0.1f, 0.07f) * 1.65f;
		private static HUDTexture playMenuMainPanel;
		private const float PLAY_MENU_MAIN_PANEL_STARTING_X = 1.2f;
		private const float PLAY_MENU_MAIN_PANEL_FIRST_TARGET_X = 0.375f;
		private const float PLAY_MENU_MAIN_PANEL_SECOND_TARGET_X = 0.02f;
		private static readonly HUDTexture[] playMenuLevelBars = new HUDTexture[10];
		private static readonly FontString[] playMenuLevelNumberStrings = new FontString[10];
		private static readonly FontString[] playMenuLevelNameStrings = new FontString[10];
		private static readonly HUDTexture[] playMenuLevelDifficultyIcons = new HUDTexture[10];
		private const float PLAY_MENU_LEVEL_DIFF_NAME_BUFFER = 0.02f;
		private static readonly Vector2 PLAY_MENU_LEVEL_DIFF_ICON_OFFSET_ADDITIONAL = new Vector2(-0.0025f, -0.005f);
		private static readonly List<HUDTexture> playMenuLevelBarAchievementTextures = new List<HUDTexture>();
		private const float PLAY_MENU_LEVEL_BAR_ACHEIVEMENT_MAX_ALPHA = 0.75f;
		private const float PLAY_MENU_LEVEL_BAR_STARTING_X = 0.975f;
		private const float PLAY_MENU_LEVEL_BAR_TARGET_X = PLAY_MENU_LEVEL_BAR_STARTING_X + PLAY_MENU_MAIN_PANEL_SECOND_TARGET_X - PLAY_MENU_MAIN_PANEL_FIRST_TARGET_X;
		private const float PLAY_MENU_LEVEL_BAR_STARTING_Y = 0.18f;
		private const float PLAY_MENU_LEVEL_BAR_Y_PER_BAR = 0.055f;
		private const float PLAY_MENU_PROGRESSION_TRANSITION_TIME = 0.4f;
		private static readonly Vector2 PLAY_MENU_LEVEL_BAR_STRING_SCALE = Vector2.ONE * 0.32f;
		private static readonly Vector2 PLAY_MENU_LEVEL_BAR_NUMBER_STRING_OFFSET = new Vector2(0.01f, 0.011f);
		private static readonly Vector3 PLAY_MENU_LEVEL_BAR_NUMBER_STRING_COLOR = new Vector3(0.7f, 0.5f, 0.2f);
		private static readonly Vector2 PLAY_MENU_LEVEL_BAR_NAME_STRING_OFFSET = new Vector2(0.04f + PLAY_MENU_LEVEL_DIFF_NAME_BUFFER, 0.0105f);
		private static readonly Vector3 PLAY_MENU_LEVEL_BAR_NAME_STRING_COLOR = new Vector3(0.6f, 0.85f, 0.4f);
		private const float PLAY_MENU_LEVEL_BAR_STRING_MAX_ALPHA = 0.7f;
		private const float PLAY_MENU_LEVEL_DIFF_MAX_ALPHA = 0.5f;
		private static bool playMenuOnLevelSelect;
		private static HUDTexture playMenuLevelBarHighlight;
		private static HUDItemExciterEntity playMenuLevelNumberExciter, playMenuLevelNameExciter, playMenuLevelBarExciter;
		private static HUDTexture playMenuMPWorldIcon;
		private static FontString playMenuMPTitleString;
		private static FontString playMenuMPCompletionTitleString;
		private static readonly Vector2 PLAY_MENU_MP_WORLD_ICON_OFFSET = new Vector2(0.005f, 0.01f);
		private static readonly Vector2 PLAY_MENU_MP_WORLD_ICON_SCALE = new Vector2(0.14f, 0.14f);
		private static readonly Vector2 PLAY_MENU_MP_TITLE_POSITION = new Vector2(0.475f, 0.205f);
		private static readonly Vector2 PLAY_MENU_MP_TITLE_SCALE = Vector2.ONE * 0.85f;
		private const int PLAY_MENU_REDUCED_TITLE_SCALE_STR_LEN = 13;
		private static readonly Vector2 PLAY_MENU_MP_TITLE_SCALE_REDUCED = Vector2.ONE * 0.6f;
		private const float PLAY_MENU_MP_REDUCED_TITLE_Y_OFFSET = 0.011f;
		private static readonly Vector3 PLAY_MENU_MP_TITLE_COLOR = new Vector3(0.7f, 1f, 0.5f);
		private static readonly Vector2 PLAY_MENU_MP_COMP_STR_POSITION = new Vector2(0.405f, 0.600f);
		private static readonly Vector2 PLAY_MENU_MP_COMP_STR_SCALE = Vector2.ONE * 0.37f;
		private static readonly Vector3 PLAY_MENU_MP_COMP_STR_COLOR = new Vector3(0.85f, 0.85f, 0.5f);
		private const float PLAY_MENU_MP_TITLE_MAX_ALPHA = 1f;
		private const float PLAY_MENU_MP_COMP_STR_MAX_ALPHA = 0.7f;
		private static HUDTexture playMenuMPWorldCompGoldStar;
		private static HUDTexture playMenuMPWorldCompSilverStar;
		private static HUDTexture playMenuMPWorldCompBronzeStar;
		private static HUDTexture playMenuMPWorldCompCoin;
		private static HUDTexture playMenuMPWorldCompEgg;
		private static readonly Vector2 PLAY_MENU_MP_WC_GOLD_STAR_OFFSET = new Vector2(0.025f, 0.482f);
		private static readonly Vector2 PLAY_MENU_MP_WC_SILVER_STAR_OFFSET = new Vector2(PLAY_MENU_MP_WC_GOLD_STAR_OFFSET.X + 0.11f, PLAY_MENU_MP_WC_GOLD_STAR_OFFSET.Y);
		private static readonly Vector2 PLAY_MENU_MP_WC_BRONZE_STAR_OFFSET = new Vector2(PLAY_MENU_MP_WC_GOLD_STAR_OFFSET.X + 0.22f, PLAY_MENU_MP_WC_GOLD_STAR_OFFSET.Y);
		private static readonly Vector2 PLAY_MENU_MP_WC_COIN_OFFSET = new Vector2(PLAY_MENU_MP_WC_GOLD_STAR_OFFSET.X + 0.34f, PLAY_MENU_MP_WC_GOLD_STAR_OFFSET.Y + 0.011f);
		private static readonly Vector2 PLAY_MENU_MP_WC_EGG_OFFSET = new Vector2(PLAY_MENU_MP_WC_GOLD_STAR_OFFSET.X + 0.45f, PLAY_MENU_MP_WC_GOLD_STAR_OFFSET.Y + 0.0125f);
		private static readonly Vector2 PLAY_MENU_MP_WC_ICON_SCALE = new Vector2(0.055f, 0.055f);
		private const float PLAY_MENU_MP_WC_ICON_ALPHA = 0.8f;
		private static FontString playMenuMPWorldCompGoldStarString;
		private static FontString playMenuMPWorldCompSilverStarString;
		private static FontString playMenuMPWorldCompBronzeStarString;
		private static FontString playMenuMPWorldCompCoinString;
		private static FontString playMenuMPWorldCompEggString;
		private static readonly Vector2 PLAY_MENU_MP_WC_STRING_SCALE = Vector2.ONE * 0.725f;
		private static readonly Vector2 PLAY_MENU_MP_WC_GOLD_STAR_STRING_POS = new Vector2(0.44f, 0.628f);
		private static readonly Vector2 PLAY_MENU_MP_WC_SILVER_STAR_STRING_POS = new Vector2(PLAY_MENU_MP_WC_GOLD_STAR_STRING_POS.X + 0.11f, PLAY_MENU_MP_WC_GOLD_STAR_STRING_POS.Y);
		private static readonly Vector2 PLAY_MENU_MP_WC_BRONZE_STAR_STRING_POS = new Vector2(PLAY_MENU_MP_WC_GOLD_STAR_STRING_POS.X + 0.22f, PLAY_MENU_MP_WC_GOLD_STAR_STRING_POS.Y);
		private static readonly Vector2 PLAY_MENU_MP_WC_COIN_STRING_POS = new Vector2(PLAY_MENU_MP_WC_GOLD_STAR_STRING_POS.X + 0.3375f, PLAY_MENU_MP_WC_GOLD_STAR_STRING_POS.Y);
		private static readonly Vector2 PLAY_MENU_MP_WC_EGG_STRING_POS = new Vector2(PLAY_MENU_MP_WC_GOLD_STAR_STRING_POS.X + 0.4425f, PLAY_MENU_MP_WC_GOLD_STAR_STRING_POS.Y);
		private const float PLAY_MENU_MP_WC_STRING_MAX_ALPHA = 0.7f;
		private static HUDTexture playMenuMPLevelCompGoldStarIndent;
		private static HUDTexture playMenuMPLevelCompSilverStarIndent;
		private static HUDTexture playMenuMPLevelCompBronzeStarIndent;
		private static HUDTexture playMenuMPLevelCompEggIndent;
		private static HUDTexture playMenuMPLevelCompCoinIndent;
		private static HUDTexture playMenuMPLevelCompGoldStar;
		private static HUDTexture playMenuMPLevelCompSilverStar;
		private static HUDTexture playMenuMPLevelCompBronzeStar;
		private static HUDTexture playMenuMPLevelCompEgg;
		private static readonly List<HUDTexture> playMenuMPLevelCompCoins = new List<HUDTexture>();
		private static FontString playMenuMPLevelCompTimesTitle;
		private static FontString playMenuMPLevelCompTimesGoldString;
		private static FontString playMenuMPLevelCompTimesSilverString;
		private static FontString playMenuMPLevelCompTimesBronzeString;
		private static FontString playMenuMPLevelCompTimesPersonalString;
		private static HUDTexture playMenuMPLevelCompTimesGoldStar;
		private static HUDTexture playMenuMPLevelCompTimesSilverStar;
		private static HUDTexture playMenuMPLevelCompTimesBronzeStar;
		private static HUDTexture playMenuMPLevelCompTimesPersonalBell;
		private static readonly Vector2 PLAY_MENU_MP_LC_COMP_TIMES_ICON_OFFSET = new Vector2(-0.026f, -0.01f);
		private static readonly Vector2 PLAY_MENU_MP_LC_GOLD_STAR_OFFSET = new Vector2(0.03f, 0.4825f);
		private static readonly Vector2 PLAY_MENU_MP_LC_SILVER_STAR_OFFSET = new Vector2(PLAY_MENU_MP_LC_GOLD_STAR_OFFSET.X + 0.05f, PLAY_MENU_MP_LC_GOLD_STAR_OFFSET.Y);
		private static readonly Vector2 PLAY_MENU_MP_LC_BRONZE_STAR_OFFSET = new Vector2(PLAY_MENU_MP_LC_GOLD_STAR_OFFSET.X + 0.1f, PLAY_MENU_MP_LC_GOLD_STAR_OFFSET.Y);
		private static readonly Vector2 PLAY_MENU_MP_LC_EGG_OFFSET = new Vector2(PLAY_MENU_MP_LC_GOLD_STAR_OFFSET.X + 0.16f, PLAY_MENU_MP_LC_GOLD_STAR_OFFSET.Y + 0.01f);
		private static readonly Vector2 PLAY_MENU_MP_LC_COIN_OFFSET_MIN = new Vector2(PLAY_MENU_MP_LC_GOLD_STAR_OFFSET.X + 0.205f, PLAY_MENU_MP_LC_GOLD_STAR_OFFSET.Y + 0.01f);
		private static readonly Vector2 PLAY_MENU_MP_LC_COIN_INDENT_OFFSET = PLAY_MENU_MP_LC_COIN_OFFSET_MIN + new Vector2(-0.024f, -0.01275f);
		private static readonly Vector2 PLAY_MENU_MP_LC_GOLD_TIME_STRING_OFFSET = new Vector2(0.42f, 0.645f);
		private static readonly Vector2 PLAY_MENU_MP_LC_SILVER_TIME_STRING_OFFSET = new Vector2(PLAY_MENU_MP_LC_GOLD_TIME_STRING_OFFSET.X, PLAY_MENU_MP_LC_GOLD_TIME_STRING_OFFSET.Y + 0.035f);
		private static readonly Vector2 PLAY_MENU_MP_LC_BRONZE_TIME_STRING_OFFSET = new Vector2(PLAY_MENU_MP_LC_GOLD_TIME_STRING_OFFSET.X + 0.105f, PLAY_MENU_MP_LC_GOLD_TIME_STRING_OFFSET.Y);
		private static readonly Vector2 PLAY_MENU_MP_LC_PERSONAL_TIME_STRING_OFFSET = new Vector2(PLAY_MENU_MP_LC_BRONZE_TIME_STRING_OFFSET.X, PLAY_MENU_MP_LC_SILVER_TIME_STRING_OFFSET.Y);
		private const float PLAY_MENU_MP_LC_COIN_OFFSET_DIFF = 0.08f;
		private static HUDTexture playMenuSecondaryPanel;
		private const float PLAY_MENU_SECONDARY_PANEL_VERT_OFFSET = 0.55f;
		private static FontString playMenuCareerStatsTitleString;
		private static readonly Vector2 PLAY_MENU_CAREER_TITLE_OFFSET = new Vector2(0.025f, 0.065f);
		private static HUDTexture playMenuCareerGoldStar;
		private static HUDTexture playMenuCareerSilverStar;
		private static HUDTexture playMenuCareerBronzeStar;
		private static HUDTexture playMenuCareerCoin;
		private static HUDTexture playMenuCareerEgg;
		private static FontString playMenuCareerGoldStarXString;
		private static FontString playMenuCareerGoldStarString;
		private static FontString playMenuCareerSilverStarXString;
		private static FontString playMenuCareerSilverStarString;
		private static FontString playMenuCareerBronzeStarXString;
		private static FontString playMenuCareerBronzeStarString;
		private static FontString playMenuCareerCoinString;
		private static FontString playMenuCareerEggString;
		private static readonly Vector2 PLAY_MENU_CAREER_GOLD_STAR_OFFSET = new Vector2(0.02f, 0.108f);
		private static readonly Vector2 PLAY_MENU_CAREER_SILVER_STAR_OFFSET = new Vector2(PLAY_MENU_CAREER_GOLD_STAR_OFFSET.X + 0.135f, PLAY_MENU_CAREER_GOLD_STAR_OFFSET.Y);
		private static readonly Vector2 PLAY_MENU_CAREER_BRONZE_STAR_OFFSET = new Vector2(PLAY_MENU_CAREER_GOLD_STAR_OFFSET.X + 0.135f * 2f, PLAY_MENU_CAREER_GOLD_STAR_OFFSET.Y);
		private static readonly Vector2 PLAY_MENU_CAREER_COIN_OFFSET = new Vector2(0.535f, 0.061f);
		private static readonly Vector2 PLAY_MENU_CAREER_EGG_OFFSET = new Vector2(PLAY_MENU_CAREER_COIN_OFFSET.X + 0f, 0.1275f);
		private static readonly Vector2 PLAY_MENU_CAREER_STAR_STRING_X_OFFSET = new Vector2(0.038f, 0.0395f);
		private static readonly Vector2 PLAY_MENU_CAREER_STAR_STRING_OFFSET = new Vector2(0.05775f, 0.0095f);
		private static readonly Vector2 PLAY_MENU_CAREER_COIN_STRING_OFFSET = new Vector2(-0.095f, 0.0065f);
		private static readonly Vector2 PLAY_MENU_CAREER_EGG_STRING_OFFSET = new Vector2(PLAY_MENU_CAREER_COIN_STRING_OFFSET.X + 0.0225f, PLAY_MENU_CAREER_COIN_STRING_OFFSET.Y);
		private const float PLAY_MENU_CAREER_COIN_EGG_STR_ALPHA = 0.7f;
		private static HUDTexture playMenuPreviewBracketLeft, playMenuPreviewBracketRight;
		private static readonly Vector2 PLAY_MENU_BRACKET_SCALE = new Vector2(0.078f, 0.34f);
		private static readonly Vector2 PLAY_MENU_BRACKET_LEFT_OFFSET = new Vector2(0.04f, 0.12f);
		private static readonly Vector2 PLAY_MENU_BRACKET_RIGHT_OFFSET = new Vector2(PLAY_MENU_BRACKET_LEFT_OFFSET.X + 0.45f, PLAY_MENU_BRACKET_LEFT_OFFSET.Y);
		private static HUDTexture playMenuPreviewIndent, playMenuPreviewFront, playMenuPreviewBack;
		private static readonly Vector2 PLAY_MENU_PREVIEW_OFFSET = PLAY_MENU_BRACKET_LEFT_OFFSET + new Vector2(0.0224f, 0.03f);
		private static readonly Vector2 PLAY_MENU_PREVIEW_SCALE = new Vector2(0.4825f, 0.28f);
		private const float PLAY_MENU_PREVIEW_TIME_PER_IMAGE = 1.5f;
		private const float PLAY_MENU_PREVIEW_IMAGE_SWAP_TIME = 0.6f;
		private static float playMenuWorldPreviewTime;
		private static string playMenuTargetHeaderString;
		private const float PLAY_MENU_HEADER_STRING_CHAR_WIPE_TIME = 0.125f * 0.15f;
		private static float playMenuTimeSinceLastHeaderCharChange;
		private static float playMenuLevelPreviewSwapCompletion;
		private const float PLAY_MENU_LEVEL_PREVIEW_CHANGE_SPEED = 2f;
		private const float PLAY_MENU_MP_TITLE_ALPHA_GROW_PER_SEC = 1.7f;
		private static readonly Vector2[] PLAY_MENU_WORLD_ICON_OFFSETS = new Vector2[11] {
			new Vector2(0f, 0f), new Vector2(0.005f, -0.005f), 
			new Vector2(0f, 0f), new Vector2(0.005f, 0.005f), 
			new Vector2(0.008f, 0.01f), new Vector2(0.01f, 0.005f), 
			new Vector2(0.005f, 0.01f), new Vector2(0.01f, 0.01f), 
			new Vector2(0.01f, 0.005f), new Vector2(0f, 0f), new Vector2(0.005f, 0.0025f), 
		};
		private static readonly float[] PLAY_MENU_WORLD_ICON_SCALE_ADJUSTMENTS = new float[11] {
			1f, 0.9f, 1f, 0.9f, 0.825f,
			0.825f, 0.85f, 0.85f, 0.85f, 1f, 0.9f
		};
		private static bool playMenuCurWorldIsUnlocked = true;
		private static readonly Dictionary<IHUDObject, float> playMenuIncreasingAlphaTextures = new Dictionary<IHUDObject, float>();
		private static readonly Dictionary<IHUDObject, float> playMenuDecreasingAlphaTextures = new Dictionary<IHUDObject, float>();
		private static readonly List<IHUDObject> playMenuIncDecTexRemovalWorkspace = new List<IHUDObject>();
		private static FontString playMenuLockedTitleString;
		private static FontString playMenuLockedCriteriaTitleString;
		private static FontString playMenuLockedGoldStarString;
		private static FontString playMenuLockedSilverStarString;
		private static FontString playMenuLockedBronzeStarString;
		private static FontString playMenuLockedCoinString;
		private static HUDTexture playMenuLockedGoldStarTex;
		private static HUDTexture playMenuLockedSilverStarTex;
		private static HUDTexture playMenuLockedBronzeStarTex;
		private static HUDTexture playMenuLockedCoinTex;
		private static HUDTexture playMenuLockedGoldStarTick;
		private static HUDTexture playMenuLockedSilverStarTick;
		private static HUDTexture playMenuLockedBronzeStarTick;
		private static HUDTexture playMenuLockedCoinTick;
		private static readonly Vector2 PLAY_MENU_LOCKED_TEX_TICK_OFFSET = new Vector2(0.015f, 0.015f);
		private static FontString playMenuEggSelectTitleString;
		private static FontString playMenuEggSelectMaterialNameString;
		private static HUDTexture playMenuEggSelectLeftChevron;
		private static HUDTexture playMenuEggSelectRightChevron;
		private static HUDTexture playMenuEggSelectLockedGoldenEggTex;
		private static FontString playMenuEggSelectLockedGoldenEggString;
		private static readonly Vector3 PLAY_MENU_EGG_SELECT_MAT_TITLE_COLOR = new Vector3(1f, 0.85f, 0.55f);
		private static readonly Vector3 PLAY_MENU_EGG_SELECT_MAT_TITLE_COLOR_LOCKED = new Vector3(1f, 0.55f, 0.15f);
		private const float PLAY_MENU_EGG_SELECT_LEFT_CHEVRON_STARTING_X = 0.3f;
		private const float PLAY_MENU_EGG_SELECT_RIGHT_CHEVRON_STARTING_X = 0.03f;
		private const float PLAY_MENU_EGG_SELECT_CHEVRON_OFFSET_MAX = 0.025f;
		private const float PLAY_MENU_EGG_SELECT_CHEVRON_OFFSET_PER_SEC = 0.035f;
		private static float playMenuEggSelectCurChevronOffset = 0f;
		private static LizardEggMaterial playMenuSelectedEggMaterial;
		private static Entity playMenuSelectedEggEntity;
		private const float PLAY_MENU_SELECTED_EGG_ENTITY_ROT_PER_SEC = MathUtils.PI_OVER_TWO;
		private const float PLAY_MENU_EGG_ENTITY_CAM_UPDOWN_ROT = MathUtils.PI_OVER_TWO * -0.31f;
		private const float PLAY_MENU_EGG_ENTITY_CAM_GROUNDPLANE_ROT = MathUtils.PI_OVER_TWO * -0.25f;
		private const float PLAY_MENU_EGG_ENTITY_CAM_OFFSET_DIST = PhysicsManager.ONE_METRE_SCALED * 0.2f;
		private static int startGateCounter;
		private static ITexture2D longLoadBackdrop = null;
		private static HUDItemExciterEntity playMenuLockedTitleExciter;
		private static float playMenuLockedTitleExciteTimeRemaining;
		private const float PLAY_MENU_LOCKED_EXCITE_TIME = 0.35f;

		private static void PlayMenuTransitionIn(bool createComponents = true) {
			CurrentMenuState = MenuState.PlayMenu;
			if (createComponents) PlayMenuCreateComponents();
			AddTransitionTickEvent(PlayMenuTickTransitionIn);
			BeginTransition(PLAY_MENU_TRANSITION_IN_TIME);
			PlayMenuSetSelectedWorldIndex(0);

			menuIdentString.Text = "WORLD   SELECT";
			menuIdentTex.Texture = AssetLocator.MainMenuPlayButton;

			if (!inDeferenceMode) HUDSound.XAnnounceWorldSelect.Play();
		}

		private static void PlayMenuTransitionOut(bool disposeComponents = true) {
			AddTransitionTickEvent(PlayMenuTickTransitionOut);
			BeginTransition(PLAY_MENU_TRANSITION_OUT_TIME);
			if (disposeComponents) currentTransitionComplete += PlayMenuDisposeComponents;
		}

		private static void PlayMenuTick(float deltaTime) {
			if (playMenuOnLevelSelect) {
				if (playMenuTargetHeaderString != playMenuMPTitleString.Text) {
					playMenuTimeSinceLastHeaderCharChange += deltaTime;
					while (playMenuTimeSinceLastHeaderCharChange >= PLAY_MENU_HEADER_STRING_CHAR_WIPE_TIME && playMenuTargetHeaderString != playMenuMPTitleString.Text) {
						bool stillRemovingChars = playMenuMPTitleString.Text.Length > playMenuTargetHeaderString.Length;
						for (int c = 0; c < playMenuMPTitleString.Text.Length && !stillRemovingChars; ++c) {
							if (playMenuMPTitleString.Text[c] != playMenuTargetHeaderString[c]) stillRemovingChars = true;
						}

						if (stillRemovingChars) playMenuMPTitleString.Text = playMenuMPTitleString.Text.Substring(0, playMenuMPTitleString.Text.Length - 1);
						else {
							if (playMenuTargetHeaderString.Length >= PLAY_MENU_REDUCED_TITLE_SCALE_STR_LEN) {
								if (playMenuMPTitleString.Scale != PLAY_MENU_MP_TITLE_SCALE_REDUCED) {
									playMenuMPTitleString.Scale = PLAY_MENU_MP_TITLE_SCALE_REDUCED;
									playMenuMPTitleString.AnchorOffset += new Vector2(0f, PLAY_MENU_MP_REDUCED_TITLE_Y_OFFSET);
								}
							}
							else if (playMenuMPTitleString.Scale != PLAY_MENU_MP_TITLE_SCALE) {
								playMenuMPTitleString.Scale = PLAY_MENU_MP_TITLE_SCALE;
								playMenuMPTitleString.AnchorOffset -= new Vector2(0f, PLAY_MENU_MP_REDUCED_TITLE_Y_OFFSET);
							}
							playMenuMPTitleString.Text = playMenuMPTitleString.Text + playMenuTargetHeaderString[playMenuMPTitleString.Text.Length];
						}

						playMenuTimeSinceLastHeaderCharChange -= PLAY_MENU_HEADER_STRING_CHAR_WIPE_TIME;
					}
				}

				if (playMenuLevelPreviewSwapCompletion < 1f) {
					playMenuLevelPreviewSwapCompletion += deltaTime * PLAY_MENU_LEVEL_PREVIEW_CHANGE_SPEED;
					if (playMenuLevelPreviewSwapCompletion >= 1f) {
						playMenuPreviewBack.Texture = playMenuPreviewFront.Texture;
						playMenuPreviewBack.SetAlpha(1f);
						playMenuPreviewFront.SetAlpha(0f);
					}
					else {
						playMenuPreviewFront.SetAlpha(playMenuLevelPreviewSwapCompletion);
					}
				}

				playMenuEggSelectCurChevronOffset += PLAY_MENU_EGG_SELECT_CHEVRON_OFFSET_PER_SEC * deltaTime;
				while (playMenuEggSelectCurChevronOffset > PLAY_MENU_EGG_SELECT_CHEVRON_OFFSET_MAX) playMenuEggSelectCurChevronOffset -= PLAY_MENU_EGG_SELECT_CHEVRON_OFFSET_MAX;
				playMenuEggSelectLeftChevron.AnchorOffset = new Vector2(
					PLAY_MENU_EGG_SELECT_LEFT_CHEVRON_STARTING_X + playMenuEggSelectCurChevronOffset,
					playMenuEggSelectLeftChevron.AnchorOffset.Y
				);
				playMenuEggSelectRightChevron.AnchorOffset = new Vector2(
					PLAY_MENU_EGG_SELECT_RIGHT_CHEVRON_STARTING_X - playMenuEggSelectCurChevronOffset,
					playMenuEggSelectRightChevron.AnchorOffset.Y
				);

				playMenuEggSelectMaterialNameString.AdjustAlpha(deltaTime * 2f);
				playMenuSelectedEggEntity.RotateBy(Quaternion.FromAxialRotation(AssetLocator.MainCamera.UpDirection, PLAY_MENU_SELECTED_EGG_ENTITY_ROT_PER_SEC * deltaTime));
				var playMenuEggEntityCamUpdownQuat = Quaternion.FromAxialRotation(AssetLocator.MainCamera.UpDirection, PLAY_MENU_EGG_ENTITY_CAM_UPDOWN_ROT);
				var playMenuEggEntityCamGroundplaneQuat = Quaternion.FromAxialRotation(Vector3.Cross(AssetLocator.MainCamera.UpDirection, AssetLocator.MainCamera.Orientation), PLAY_MENU_EGG_ENTITY_CAM_GROUNDPLANE_ROT);
				var offset = (AssetLocator.MainCamera.Orientation * playMenuEggEntityCamUpdownQuat * playMenuEggEntityCamGroundplaneQuat).WithLength(PLAY_MENU_EGG_ENTITY_CAM_OFFSET_DIST);
				playMenuSelectedEggEntity.SetTranslation(AssetLocator.MainCamera.Position + offset);
			}
			else {
				if (playMenuCurWorldIsUnlocked) {
					const float PREVIEW_TOTAL_IMAGE_TIME = PLAY_MENU_PREVIEW_TIME_PER_IMAGE + PLAY_MENU_PREVIEW_IMAGE_SWAP_TIME;
					playMenuWorldPreviewTime += deltaTime;
					while (playMenuWorldPreviewTime >= PREVIEW_TOTAL_IMAGE_TIME) {
						playMenuPreviewBack.Texture = playMenuPreviewFront.Texture;
						playMenuPreviewFront.SetAlpha(0f);
						playMenuPreviewFront.Texture = MenuPreviewManager.GetNextLoadedTexture();
						playMenuWorldPreviewTime -= PREVIEW_TOTAL_IMAGE_TIME;
					}

					if (playMenuWorldPreviewTime > PLAY_MENU_PREVIEW_TIME_PER_IMAGE) {
						float swapTimeComplete = (playMenuWorldPreviewTime - PLAY_MENU_PREVIEW_TIME_PER_IMAGE) / PLAY_MENU_PREVIEW_IMAGE_SWAP_TIME;
						playMenuPreviewFront.SetAlpha(swapTimeComplete);
					}	
				}

				if (playMenuMPTitleString.Color.W < PLAY_MENU_MP_TITLE_MAX_ALPHA) {
					playMenuMPTitleString.AdjustAlpha(PLAY_MENU_MP_TITLE_ALPHA_GROW_PER_SEC * deltaTime);
					if (playMenuMPTitleString.Color.W > PLAY_MENU_MP_TITLE_MAX_ALPHA) playMenuMPTitleString.SetAlpha(PLAY_MENU_MP_TITLE_MAX_ALPHA);
				}
				if (playMenuMPWorldIcon.Color.W < 1f) {
					playMenuMPWorldIcon.AdjustAlpha(PLAY_MENU_MP_TITLE_ALPHA_GROW_PER_SEC * deltaTime);
				}
			}

			playMenuIncDecTexRemovalWorkspace.Clear();
			foreach (var kvp in playMenuIncreasingAlphaTextures) {
				kvp.Key.AdjustAlpha(deltaTime * kvp.Value);
				if (kvp.Key.Color.W >= 1f) playMenuIncDecTexRemovalWorkspace.Add(kvp.Key);
			}
			foreach (var toBeRemoved in playMenuIncDecTexRemovalWorkspace) {
				playMenuIncreasingAlphaTextures.Remove(toBeRemoved);
			}
			playMenuIncDecTexRemovalWorkspace.Clear();
			foreach (var kvp in playMenuDecreasingAlphaTextures) {
				kvp.Key.AdjustAlpha(-deltaTime * kvp.Value);
				if (kvp.Key.Color.W <= 0f) playMenuIncDecTexRemovalWorkspace.Add(kvp.Key);
			}
			foreach (var toBeRemoved in playMenuIncDecTexRemovalWorkspace) {
				playMenuDecreasingAlphaTextures.Remove(toBeRemoved);
			}

			if (playMenuLockedTitleExciteTimeRemaining > 0f) {
				playMenuLockedTitleExciteTimeRemaining -= deltaTime;
				if (playMenuLockedTitleExciteTimeRemaining <= 0f) playMenuLockedTitleExciter.TargetObj = null;
			}
		}

		private static void PlayMenuCreateComponents() {
			PlayMenuDisposeComponents();

			triggerUp += PlayMenuChangeOptionUp;
			triggerDown += PlayMenuChangeOptionDown;
			triggerLeft += PlayMenuChangeOptionLeft;
			triggerRight += PlayMenuChangeOptionRight;
			triggerConfirm += PlayMenuConfirmOption;
			triggerBackOut += PlayMenuBackOut;
			tick += PlayMenuTick;

			for (int i = 0; i < 11; ++i) {
				float xPos;
				if (i < 9) {
					switch (i % 3) {
						case 0:
							xPos = PLAY_MENU_WORLD_ICON_COLUMN_A_X;
							break;
						case 1:
							xPos = PLAY_MENU_WORLD_ICON_COLUMN_B_X;
							break;
						default:
							xPos = PLAY_MENU_WORLD_ICON_COLUMN_C_X;
							break;
					}
				}
				else if (i == 9) xPos = PLAY_MENU_WORLD_ICON_COLUMN_A_X + (PLAY_MENU_WORLD_ICON_COLUMN_B_X - PLAY_MENU_WORLD_ICON_COLUMN_A_X) * 0.5f;
				else xPos = PLAY_MENU_WORLD_ICON_COLUMN_B_X + (PLAY_MENU_WORLD_ICON_COLUMN_C_X - PLAY_MENU_WORLD_ICON_COLUMN_B_X) * 0.5f;

				playMenuWorldSelectIcons[i] = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
					Anchoring = ViewportAnchoring.TopLeft,
					AnchorOffset = new Vector2(xPos, 0f),
					AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
					Color = new Vector4(1f, 1f, 1f, 0f),
					Rotation = 0f,
					Scale = PLAY_MENU_WORLD_ICON_SCALE,
					Texture = i <= PersistedWorldData.GetFurthestUnlockedWorldIndex() ? AssetLocator.UnlockedWorldIcons[i] : AssetLocator.LockedWorldIcons[i],
					ZIndex = 2
				};
			}

			playMenuMainPanel = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = ViewportAnchoring.TopLeft,
				AnchorOffset = new Vector2(PLAY_MENU_MAIN_PANEL_STARTING_X, 0.155f),
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.None,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = new Vector2(0.6f, 0.6f),
				Texture = AssetLocator.PlayMenuMainPanel,
				ZIndex = 2
			};

			for (int i = 0; i < 10; ++i) {
				playMenuLevelBars[i] = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
					Anchoring = ViewportAnchoring.TopLeft,
					AnchorOffset = new Vector2(PLAY_MENU_LEVEL_BAR_STARTING_X, PLAY_MENU_LEVEL_BAR_STARTING_Y),
					AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.None,
					Color = new Vector4(1f, 1f, 1f, 0f),
					Rotation = 0f,
					Scale = new Vector2(0.35f, playMenuMainPanel.Scale.Y * 0.085f),
					Texture = AssetLocator.PlayMenuLevelBar,
					ZIndex = 2
				};

				playMenuLevelNumberStrings[i] = AssetLocator.TitleFont.AddString(
					AssetLocator.HudLayer,
					AssetLocator.MainWindow,
					ViewportAnchoring.TopLeft,
					playMenuLevelBars[i].AnchorOffset + PLAY_MENU_LEVEL_BAR_NUMBER_STRING_OFFSET,
					PLAY_MENU_LEVEL_BAR_STRING_SCALE
				);
				playMenuLevelNumberStrings[i].Color = new Vector4(PLAY_MENU_LEVEL_BAR_NUMBER_STRING_COLOR, w: 0f);
				playMenuLevelNumberStrings[i].Text = String.Empty;

				playMenuLevelNameStrings[i] = AssetLocator.TitleFont.AddString(
					AssetLocator.HudLayer,
					AssetLocator.MainWindow,
					ViewportAnchoring.TopRight,
					PLAY_MENU_LEVEL_BAR_NAME_STRING_OFFSET,
					PLAY_MENU_LEVEL_BAR_STRING_SCALE
				);
				playMenuLevelNameStrings[i].Color = new Vector4(PLAY_MENU_LEVEL_BAR_NAME_STRING_COLOR, w: 0f);
				playMenuLevelNameStrings[i].Text = String.Empty;

				playMenuLevelDifficultyIcons[i] = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
					Anchoring = ViewportAnchoring.TopRight,
					AnchorOffset = new Vector2(PLAY_MENU_LEVEL_BAR_NAME_STRING_OFFSET.X - PLAY_MENU_LEVEL_DIFF_NAME_BUFFER, PLAY_MENU_LEVEL_BAR_NAME_STRING_OFFSET.Y) + PLAY_MENU_LEVEL_DIFF_ICON_OFFSET_ADDITIONAL,
					AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.None,
					Color = Vector4.ZERO,
					Rotation = 0f,
					Scale = new Vector2(0.0225f, 0.0375f) * 1.08f,
					ZIndex = playMenuLevelBars[i].ZIndex + 3
				};
			}

			playMenuLevelNumberExciter = new HUDItemExciterEntity(null) {
				Speed = 0.0025f,
				Lifetime = 0.65f,
				CountPerSec = 15,
				OpacityMultiplier = 0.85f,
				ColorOverride = PLAY_MENU_LEVEL_BAR_NUMBER_STRING_COLOR * 2f
			};
			playMenuLevelNameExciter = new HUDItemExciterEntity(null) {
				Speed = 0.0025f,
				Lifetime = 0.65f,
				CountPerSec = 15,
				OpacityMultiplier = 0.85f,
				ColorOverride = PLAY_MENU_LEVEL_BAR_NAME_STRING_COLOR * 2f
			};

			playMenuLevelBarHighlight = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = playMenuLevelBars[0].Anchoring,
				AnchorOffset = playMenuLevelBars[0].AnchorOffset,
				AspectCorrectionStrategy = playMenuLevelBars[0].AspectCorrectionStrategy,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = playMenuLevelBars[0].Scale,
				Texture = AssetLocator.PlayMenuLevelBarHighlight,
				ZIndex = playMenuLevelBars[0].ZIndex + 1
			};

			playMenuLevelBarExciter = new HUDItemExciterEntity(playMenuLevelBarHighlight) {
				Speed = 0.02f,
				Lifetime = 0.3f,
				ColorOverride = (Vector3) playMenuLevelBarHighlight.Color * 2f
			};

			playMenuWorldIconHighlight = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = playMenuWorldSelectIcons[0].Anchoring,
				AnchorOffset = playMenuWorldSelectIcons[0].AnchorOffset,
				AspectCorrectionStrategy = playMenuWorldSelectIcons[0].AspectCorrectionStrategy,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = PLAY_MENU_WORLD_ICON_SCALE,
				Texture = AssetLocator.PlayMenuWorldButtonHighlight,
				ZIndex = playMenuWorldSelectIcons[0].ZIndex + 1
			};

			playMenuWorldIconHighlightExciter = new HUDItemExciterEntity(playMenuWorldIconHighlight) {
				Speed = 0.02f,
				Lifetime = 0.3f
			};

			playMenuMPWorldIcon = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = playMenuMainPanel.Anchoring,
				AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_WORLD_ICON_OFFSET,
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = PLAY_MENU_MP_WORLD_ICON_SCALE,
				Texture = AssetLocator.WorldIcons[0],
				ZIndex = playMenuMainPanel.ZIndex + 5
			};

			playMenuMPTitleString = AssetLocator.TitleFont.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.TopLeft,
				PLAY_MENU_MP_TITLE_POSITION,
				PLAY_MENU_MP_TITLE_SCALE
			);
			playMenuMPTitleString.Color = new Vector4(PLAY_MENU_MP_TITLE_COLOR, w: 0f);
			playMenuMPTitleString.Text = "Home Forest";

			playMenuMPCompletionTitleString = AssetLocator.TitleFont.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.TopLeft,
				PLAY_MENU_MP_COMP_STR_POSITION,
				PLAY_MENU_MP_COMP_STR_SCALE
			);
			playMenuMPCompletionTitleString.Color = new Vector4(PLAY_MENU_MP_COMP_STR_COLOR, w: 0f);
			playMenuMPCompletionTitleString.Text = "World Completion:";

			playMenuMPWorldCompGoldStar = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = playMenuMainPanel.Anchoring,
				AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_WC_GOLD_STAR_OFFSET,
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = PLAY_MENU_MP_WC_ICON_SCALE,
				Texture = AssetLocator.GoldStar,
				ZIndex = playMenuMainPanel.ZIndex + 1
			};
			playMenuMPWorldCompSilverStar = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = playMenuMainPanel.Anchoring,
				AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_WC_SILVER_STAR_OFFSET,
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = PLAY_MENU_MP_WC_ICON_SCALE,
				Texture = AssetLocator.SilverStar,
				ZIndex = playMenuMainPanel.ZIndex + 1
			};
			playMenuMPWorldCompBronzeStar = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = playMenuMainPanel.Anchoring,
				AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_WC_BRONZE_STAR_OFFSET,
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = PLAY_MENU_MP_WC_ICON_SCALE,
				Texture = AssetLocator.BronzeStar,
				ZIndex = playMenuMainPanel.ZIndex + 1
			};
			playMenuMPWorldCompCoin = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = playMenuMainPanel.Anchoring,
				AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_WC_COIN_OFFSET,
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = PLAY_MENU_MP_WC_ICON_SCALE * 0.8f,
				Texture = AssetLocator.CoinFrames[0],
				ZIndex = playMenuMainPanel.ZIndex + 1
			};
			playMenuMPWorldCompEgg = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = playMenuMainPanel.Anchoring,
				AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_WC_EGG_OFFSET,
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = PLAY_MENU_MP_WC_ICON_SCALE * 0.8f,
				Texture = AssetLocator.VultureEggInvert,
				ZIndex = playMenuMainPanel.ZIndex + 1
			};

			playMenuMPWorldCompGoldStarString = AssetLocator.MainFont.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.TopLeft,
				PLAY_MENU_MP_WC_GOLD_STAR_STRING_POS,
				PLAY_MENU_MP_WC_STRING_SCALE
			);
			playMenuMPWorldCompGoldStarString.Color = new Vector4(0.9f, 0.75f, 0.2f, 0f);
			playMenuMPWorldCompGoldStarString.Text = "100%";

			playMenuMPWorldCompSilverStarString = AssetLocator.MainFont.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.TopLeft,
				PLAY_MENU_MP_WC_SILVER_STAR_STRING_POS,
				PLAY_MENU_MP_WC_STRING_SCALE
			);
			playMenuMPWorldCompSilverStarString.Color = new Vector4(0.7f, 0.7f, 0.8f, 0f);
			playMenuMPWorldCompSilverStarString.Text = "100%";

			playMenuMPWorldCompBronzeStarString = AssetLocator.MainFont.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.TopLeft,
				PLAY_MENU_MP_WC_BRONZE_STAR_STRING_POS,
				PLAY_MENU_MP_WC_STRING_SCALE
			);
			playMenuMPWorldCompBronzeStarString.Color = new Vector4(0.6f, 0.2f, 0.0f, 0f);
			playMenuMPWorldCompBronzeStarString.Text = "100%";

			playMenuMPWorldCompCoinString = AssetLocator.MainFont.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.TopLeft,
				PLAY_MENU_MP_WC_COIN_STRING_POS,
				PLAY_MENU_MP_WC_STRING_SCALE
			);
			playMenuMPWorldCompCoinString.Color = new Vector4(239f, 155f, 8f, 0f) / 255f;
			playMenuMPWorldCompCoinString.Text = "100%";

			playMenuMPWorldCompEggString = AssetLocator.MainFont.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.TopLeft,
				PLAY_MENU_MP_WC_EGG_STRING_POS,
				PLAY_MENU_MP_WC_STRING_SCALE
			);
			playMenuMPWorldCompEggString.Color = new Vector4(0.7f, 1f, 0.7f, 0f);
			playMenuMPWorldCompEggString.Text = "100%";


			playMenuMPLevelCompBronzeStarIndent = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = playMenuMainPanel.Anchoring,
				AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_LC_BRONZE_STAR_OFFSET,
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = PLAY_MENU_MP_WC_ICON_SCALE,
				Texture = AssetLocator.StarIndent,
				ZIndex = playMenuMainPanel.ZIndex + 1
			};
			playMenuMPLevelCompSilverStarIndent = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = playMenuMainPanel.Anchoring,
				AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_LC_SILVER_STAR_OFFSET,
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = PLAY_MENU_MP_WC_ICON_SCALE,
				Texture = AssetLocator.StarIndent,
				ZIndex = playMenuMainPanel.ZIndex + 1
			};
			playMenuMPLevelCompGoldStarIndent = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = playMenuMainPanel.Anchoring,
				AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_LC_GOLD_STAR_OFFSET,
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = PLAY_MENU_MP_WC_ICON_SCALE,
				Texture = AssetLocator.StarIndent,
				ZIndex = playMenuMainPanel.ZIndex + 1
			};
			playMenuMPLevelCompEggIndent = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = playMenuMainPanel.Anchoring,
				AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_LC_EGG_OFFSET,
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = PLAY_MENU_MP_WC_ICON_SCALE * 0.9f,
				Texture = AssetLocator.EggIndent,
				ZIndex = playMenuMainPanel.ZIndex + 1
			};
			playMenuMPLevelCompCoinIndent = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = playMenuMainPanel.Anchoring,
				AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_LC_COIN_INDENT_OFFSET,
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.None,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = new Vector2(PLAY_MENU_MP_WC_ICON_SCALE.X + PLAY_MENU_MP_LC_COIN_OFFSET_DIFF + 0.0055f, PLAY_MENU_MP_WC_ICON_SCALE.Y + 0.0225f) * 1.2f,
				Texture = AssetLocator.CoinIndent,
				ZIndex = playMenuMainPanel.ZIndex + 1
			};

			playMenuMPLevelCompBronzeStar = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = playMenuMainPanel.Anchoring,
				AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_LC_GOLD_STAR_OFFSET,
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = PLAY_MENU_MP_WC_ICON_SCALE,
				Texture = AssetLocator.BronzeStar,
				ZIndex = playMenuMainPanel.ZIndex + 2
			};
			playMenuMPLevelCompSilverStar = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = playMenuMainPanel.Anchoring,
				AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_LC_SILVER_STAR_OFFSET,
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = PLAY_MENU_MP_WC_ICON_SCALE,
				Texture = AssetLocator.SilverStar,
				ZIndex = playMenuMainPanel.ZIndex + 2
			};
			playMenuMPLevelCompGoldStar = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = playMenuMainPanel.Anchoring,
				AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_LC_BRONZE_STAR_OFFSET,
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = PLAY_MENU_MP_WC_ICON_SCALE,
				Texture = AssetLocator.GoldStar,
				ZIndex = playMenuMainPanel.ZIndex + 2
			};
			playMenuMPLevelCompEgg = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = playMenuMainPanel.Anchoring,
				AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_LC_EGG_OFFSET,
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = PLAY_MENU_MP_WC_ICON_SCALE * 0.9f,
				Texture = AssetLocator.VultureEggInvert,
				ZIndex = playMenuMainPanel.ZIndex + 2
			};

			playMenuMPLevelCompTimesTitle = AssetLocator.TitleFont.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.TopLeft,
				new Vector2(0.415f, 0.600f),
				PLAY_MENU_MP_COMP_STR_SCALE
			);
			playMenuMPLevelCompTimesTitle.Color = new Vector4(PLAY_MENU_MP_COMP_STR_COLOR, w: 0f);
			playMenuMPLevelCompTimesTitle.Text = "Level Times";

			playMenuMPLevelCompTimesGoldString = AssetLocator.TitleFont.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.TopLeft,
				PLAY_MENU_MP_LC_GOLD_TIME_STRING_OFFSET,
				PLAY_MENU_MP_COMP_STR_SCALE * 0.9f
			);
			playMenuMPLevelCompTimesGoldString.Color = new Vector4(playMenuMPWorldCompGoldStarString.Color, w: 0f);
			playMenuMPLevelCompTimesGoldString.Text = "00:00";

			playMenuMPLevelCompTimesSilverString = AssetLocator.TitleFont.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.TopLeft,
				PLAY_MENU_MP_LC_SILVER_TIME_STRING_OFFSET,
				PLAY_MENU_MP_COMP_STR_SCALE * 0.9f
			);
			playMenuMPLevelCompTimesSilverString.Color = new Vector4(playMenuMPWorldCompSilverStarString.Color, w: 0f);
			playMenuMPLevelCompTimesSilverString.Text = "00:00";

			playMenuMPLevelCompTimesBronzeString = AssetLocator.TitleFont.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.TopLeft,
				PLAY_MENU_MP_LC_BRONZE_TIME_STRING_OFFSET,
				PLAY_MENU_MP_COMP_STR_SCALE * 0.9f
			);
			playMenuMPLevelCompTimesBronzeString.Color = new Vector4(playMenuMPWorldCompBronzeStarString.Color, w: 0f);
			playMenuMPLevelCompTimesBronzeString.Text = "00:00";

			playMenuMPLevelCompTimesPersonalString = AssetLocator.TitleFont.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.TopLeft,
				PLAY_MENU_MP_LC_PERSONAL_TIME_STRING_OFFSET,
				PLAY_MENU_MP_COMP_STR_SCALE * 0.9f
			);
			playMenuMPLevelCompTimesPersonalString.Color = new Vector4(1f, 1f, 1f, 0f);
			playMenuMPLevelCompTimesPersonalString.Text = "--:--";

			playMenuMPLevelCompTimesGoldStar = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = playMenuMainPanel.Anchoring,
				AnchorOffset = playMenuMPLevelCompTimesGoldString.AnchorOffset + PLAY_MENU_MP_LC_COMP_TIMES_ICON_OFFSET,
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = PLAY_MENU_MP_WC_ICON_SCALE * 0.65f,
				Texture = AssetLocator.GoldStar,
				ZIndex = playMenuMainPanel.ZIndex + 2
			};
			playMenuMPLevelCompTimesSilverStar = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = playMenuMainPanel.Anchoring,
				AnchorOffset = playMenuMPLevelCompTimesSilverString.AnchorOffset + PLAY_MENU_MP_LC_COMP_TIMES_ICON_OFFSET,
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = PLAY_MENU_MP_WC_ICON_SCALE * 0.65f,
				Texture = AssetLocator.SilverStar,
				ZIndex = playMenuMainPanel.ZIndex + 2
			};
			playMenuMPLevelCompTimesBronzeStar = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = playMenuMainPanel.Anchoring,
				AnchorOffset = playMenuMPLevelCompTimesBronzeString.AnchorOffset + PLAY_MENU_MP_LC_COMP_TIMES_ICON_OFFSET,
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = PLAY_MENU_MP_WC_ICON_SCALE * 0.65f,
				Texture = AssetLocator.BronzeStar,
				ZIndex = playMenuMainPanel.ZIndex + 2
			};
			playMenuMPLevelCompTimesPersonalBell = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = playMenuMainPanel.Anchoring,
				AnchorOffset = playMenuMPLevelCompTimesPersonalString.AnchorOffset + PLAY_MENU_MP_LC_COMP_TIMES_ICON_OFFSET + new Vector2(0.0045f, 0.009f),
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = PLAY_MENU_MP_WC_ICON_SCALE * 0.45f,
				Texture = AssetLocator.FinishingBell,
				ZIndex = playMenuMainPanel.ZIndex + 2
			};

			playMenuSecondaryPanel = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = ViewportAnchoring.TopLeft,
				AnchorOffset = playMenuMainPanel.AnchorOffset,
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.None,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = new Vector2(playMenuMainPanel.Scale.X, 0.45f),
				Texture = AssetLocator.PlayMenuSecondaryPanel,
				ZIndex = playMenuMainPanel.ZIndex - 1
			};

			playMenuCareerStatsTitleString = AssetLocator.TitleFont.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.TopLeft,
				playMenuSecondaryPanel.AnchorOffset + PLAY_MENU_CAREER_TITLE_OFFSET,
				Vector2.ONE * 0.55f
			);
			playMenuCareerStatsTitleString.Color = new Vector4(0.85f, 1f, 0.7f, 0f);
			playMenuCareerStatsTitleString.Text = "Career      Progress";

			playMenuCareerGoldStar = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = playMenuSecondaryPanel.Anchoring,
				AnchorOffset = playMenuSecondaryPanel.AnchorOffset + PLAY_MENU_CAREER_GOLD_STAR_OFFSET,
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = PLAY_MENU_MP_WC_ICON_SCALE * 1.15f,
				Texture = AssetLocator.GoldStar,
				ZIndex = playMenuSecondaryPanel.ZIndex + 2
			};
			playMenuCareerSilverStar = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = playMenuSecondaryPanel.Anchoring,
				AnchorOffset = playMenuSecondaryPanel.AnchorOffset + PLAY_MENU_CAREER_SILVER_STAR_OFFSET,
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = PLAY_MENU_MP_WC_ICON_SCALE * 1.15f,
				Texture = AssetLocator.SilverStar,
				ZIndex = playMenuSecondaryPanel.ZIndex + 2
			};
			playMenuCareerBronzeStar = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = playMenuSecondaryPanel.Anchoring,
				AnchorOffset = playMenuSecondaryPanel.AnchorOffset + PLAY_MENU_CAREER_BRONZE_STAR_OFFSET,
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = PLAY_MENU_MP_WC_ICON_SCALE * 1.15f,
				Texture = AssetLocator.BronzeStar,
				ZIndex = playMenuSecondaryPanel.ZIndex + 2
			};
			playMenuCareerCoin = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = playMenuSecondaryPanel.Anchoring,
				AnchorOffset = playMenuSecondaryPanel.AnchorOffset + PLAY_MENU_CAREER_COIN_OFFSET,
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = PLAY_MENU_MP_WC_ICON_SCALE * 0.9f,
				Texture = AssetLocator.CoinFrames[0],
				ZIndex = playMenuSecondaryPanel.ZIndex + 2
			};
			playMenuCareerEgg = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = playMenuSecondaryPanel.Anchoring,
				AnchorOffset = playMenuSecondaryPanel.AnchorOffset + PLAY_MENU_CAREER_EGG_OFFSET,
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = PLAY_MENU_MP_WC_ICON_SCALE * 0.9f,
				Texture = AssetLocator.VultureEggInvert,
				ZIndex = playMenuSecondaryPanel.ZIndex + 2
			};

			playMenuCareerGoldStarXString = AssetLocator.TitleFontGlow.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.TopLeft,
				playMenuCareerGoldStar.AnchorOffset + PLAY_MENU_CAREER_STAR_STRING_X_OFFSET,
				Vector2.ONE * 0.4f
			);
			playMenuCareerGoldStarXString.Color = playMenuMPWorldCompGoldStarString.Color;
			playMenuCareerGoldStarXString.Text = "x";
			playMenuCareerGoldStarString = AssetLocator.TitleFontGlow.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.TopLeft,
				playMenuCareerGoldStar.AnchorOffset + PLAY_MENU_CAREER_STAR_STRING_OFFSET,
				Vector2.ONE * 0.7f
			);
			playMenuCareerGoldStarString.Color = playMenuMPWorldCompGoldStarString.Color;
			playMenuCareerGoldStarString.Text = PersistedWorldData.GetTotalGoldStars().ToString("00");

			playMenuCareerSilverStarXString = AssetLocator.TitleFontGlow.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.TopLeft,
				playMenuCareerSilverStar.AnchorOffset + PLAY_MENU_CAREER_STAR_STRING_X_OFFSET,
				Vector2.ONE * 0.4f
			);
			playMenuCareerSilverStarXString.Color = playMenuMPWorldCompSilverStarString.Color;
			playMenuCareerSilverStarXString.Text = "x";
			playMenuCareerSilverStarString = AssetLocator.TitleFontGlow.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.TopLeft,
				playMenuCareerSilverStar.AnchorOffset + PLAY_MENU_CAREER_STAR_STRING_OFFSET,
				Vector2.ONE * 0.7f
			);
			playMenuCareerSilverStarString.Color = playMenuMPWorldCompSilverStarString.Color;
			playMenuCareerSilverStarString.Text = PersistedWorldData.GetTotalSilverStars().ToString("00");

			playMenuCareerBronzeStarXString = AssetLocator.TitleFontGlow.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.TopLeft,
				playMenuCareerBronzeStar.AnchorOffset + PLAY_MENU_CAREER_STAR_STRING_X_OFFSET,
				Vector2.ONE * 0.4f
			);
			playMenuCareerBronzeStarXString.Color = playMenuMPWorldCompBronzeStarString.Color;
			playMenuCareerBronzeStarXString.Text = "x";
			playMenuCareerBronzeStarString = AssetLocator.TitleFontGlow.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.TopLeft,
				playMenuCareerBronzeStar.AnchorOffset + PLAY_MENU_CAREER_STAR_STRING_OFFSET,
				Vector2.ONE * 0.7f
			);
			playMenuCareerBronzeStarString.Color = playMenuMPWorldCompBronzeStarString.Color;
			playMenuCareerBronzeStarString.Text = PersistedWorldData.GetTotalBronzeStars().ToString("00");

			playMenuCareerCoinString = AssetLocator.TitleFontGlow.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.TopLeft,
				playMenuCareerCoin.AnchorOffset + PLAY_MENU_CAREER_COIN_STRING_OFFSET,
				Vector2.ONE * 0.5f
			);
			playMenuCareerCoinString.Color = playMenuMPWorldCompCoinString.Color;
			playMenuCareerCoinString.Text = PersistedWorldData.GetTotalTakenCoins().ToString("0000");

			playMenuCareerEggString = AssetLocator.TitleFontGlow.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.TopLeft,
				playMenuCareerEgg.AnchorOffset + PLAY_MENU_CAREER_EGG_STRING_OFFSET,
				Vector2.ONE * 0.5f
			);
			playMenuCareerEggString.Color = playMenuMPWorldCompEggString.Color;
			playMenuCareerEggString.Text = PersistedWorldData.GetTotalGoldenEggs().ToString("000");

			playMenuPreviewBracketLeft = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = playMenuMainPanel.Anchoring,
				AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_BRACKET_LEFT_OFFSET,
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.None,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = PLAY_MENU_BRACKET_SCALE,
				Texture = AssetLocator.PreviewImageBracket,
				ZIndex = playMenuMainPanel.ZIndex + 4
			};
			playMenuPreviewBracketRight = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = playMenuMainPanel.Anchoring,
				AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_BRACKET_RIGHT_OFFSET,
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.None,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = MathUtils.PI,
				Scale = PLAY_MENU_BRACKET_SCALE,
				Texture = AssetLocator.PreviewImageBracket,
				ZIndex = playMenuMainPanel.ZIndex + 4
			};
			playMenuPreviewIndent = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = playMenuMainPanel.Anchoring,
				AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_PREVIEW_OFFSET,
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.None,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = PLAY_MENU_PREVIEW_SCALE,
				Texture = AssetLocator.PreviewIndent,
				ZIndex = playMenuMainPanel.ZIndex + 3
			};
			playMenuPreviewFront = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = playMenuMainPanel.Anchoring,
				AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_PREVIEW_OFFSET,
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.None,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = PLAY_MENU_PREVIEW_SCALE,
				ZIndex = playMenuMainPanel.ZIndex + 2
			};
			playMenuPreviewBack = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = playMenuMainPanel.Anchoring,
				AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_PREVIEW_OFFSET,
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.None,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = PLAY_MENU_PREVIEW_SCALE,
				ZIndex = playMenuMainPanel.ZIndex + 1
			};

			Vector2 previewAreaTargetOffset = new Vector2(playMenuMainPanel.AnchorOffset, x: PLAY_MENU_MAIN_PANEL_FIRST_TARGET_X) + PLAY_MENU_PREVIEW_OFFSET;
			playMenuLockedTitleString = AssetLocator.TitleFontGlow.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.TopLeft,
				previewAreaTargetOffset + new Vector2(0.044f, 0.005f),
				Vector2.ONE * 0.7f
			);
			playMenuLockedTitleString.Color = new Vector4(0.8f, 0.4f, 0.4f, 0f);
			playMenuLockedTitleString.Text = "World Locked!";

			playMenuLockedTitleExciter = new HUDItemExciterEntity(null) {
				CountPerSec = 100f,
				Lifetime = 0.5f
			};

			playMenuLockedTitleExciteTimeRemaining = 0f;

			playMenuLockedCriteriaTitleString = AssetLocator.TitleFont.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.TopLeft,
				previewAreaTargetOffset + new Vector2(0.0715f, 0.0875f),
				Vector2.ONE * 0.4f
			);
			playMenuLockedCriteriaTitleString.Color = new Vector4(0.8f, 0.6f, 0.4f, 0f);
			playMenuLockedCriteriaTitleString.Text = "Unlock   Requirements:";

			playMenuLockedGoldStarString = AssetLocator.MainFont.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.TopLeft,
				previewAreaTargetOffset + new Vector2(0f, 0.125f),
				Vector2.ONE * 0.8f
			);
			playMenuLockedGoldStarString.Color = new Vector4((Vector3) playMenuCareerGoldStarString.Color * 0.35f + Vector3.ONE * 0.35f, w: 0f);
			playMenuLockedGoldStarString.Text = "00x";

			playMenuLockedSilverStarString = AssetLocator.MainFont.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.TopLeft,
				new Vector2(previewAreaTargetOffset.X, playMenuLockedGoldStarString.AnchorOffset.Y),
				Vector2.ONE * 0.8f
			);
			playMenuLockedSilverStarString.Color = new Vector4((Vector3) playMenuCareerSilverStarString.Color * 0.35f + Vector3.ONE * 0.35f, w: 0f);
			playMenuLockedSilverStarString.Text = "00x";

			playMenuLockedBronzeStarString = AssetLocator.MainFont.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.TopLeft,
				new Vector2(previewAreaTargetOffset.X, playMenuLockedGoldStarString.AnchorOffset.Y),
				Vector2.ONE * 0.8f
			);
			playMenuLockedBronzeStarString.Color = new Vector4((Vector3) playMenuCareerBronzeStarString.Color * 0.35f + Vector3.ONE * 0.35f, w: 0f);
			playMenuLockedBronzeStarString.Text = "00x";

			playMenuLockedCoinString = AssetLocator.MainFont.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.TopLeft,
				new Vector2(previewAreaTargetOffset.X, playMenuLockedGoldStarString.AnchorOffset.Y),
				Vector2.ONE * 0.8f
			);
			playMenuLockedCoinString.Color = new Vector4((Vector3) playMenuCareerCoinString.Color * 0.35f + Vector3.ONE * 0.35f, w: 0f);
			playMenuLockedCoinString.Text = "0000x";

			playMenuLockedGoldStarTex = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = playMenuMainPanel.Anchoring,
				AnchorOffset = new Vector2(0f, playMenuLockedGoldStarString.AnchorOffset.Y + 0.02f),
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.None,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = new Vector2(0.045f, 0.07f),
				Texture = AssetLocator.GoldStar,
				ZIndex = playMenuMainPanel.ZIndex + 1
			};
			playMenuLockedGoldStarTick = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = playMenuMainPanel.Anchoring,
				AnchorOffset = Vector2.ZERO,
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.None,
				Color = new Vector4(0.4f, 1f, 0.4f, 0f),
				Rotation = 0f,
				Scale = playMenuLockedGoldStarTex.Scale * 0.9f,
				Texture = AssetLocator.Tick,
				ZIndex = playMenuMainPanel.ZIndex + 2
			};
			playMenuLockedSilverStarTex = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = playMenuMainPanel.Anchoring,
				AnchorOffset = new Vector2(0f, playMenuLockedSilverStarString.AnchorOffset.Y + 0.02f),
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.None,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = playMenuLockedGoldStarTex.Scale,
				Texture = AssetLocator.SilverStar,
				ZIndex = playMenuMainPanel.ZIndex + 1
			};
			playMenuLockedSilverStarTick = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = playMenuMainPanel.Anchoring,
				AnchorOffset = Vector2.ZERO,
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.None,
				Color = new Vector4(0.4f, 1f, 0.4f, 0f),
				Rotation = 0f,
				Scale = playMenuLockedGoldStarTex.Scale * 0.9f,
				Texture = AssetLocator.Tick,
				ZIndex = playMenuMainPanel.ZIndex + 2
			};
			playMenuLockedBronzeStarTex = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = playMenuMainPanel.Anchoring,
				AnchorOffset = new Vector2(0f, playMenuLockedBronzeStarString.AnchorOffset.Y + 0.02f),
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.None,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = playMenuLockedGoldStarTex.Scale,
				Texture = AssetLocator.BronzeStar,
				ZIndex = playMenuMainPanel.ZIndex + 1
			};
			playMenuLockedBronzeStarTick = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = playMenuMainPanel.Anchoring,
				AnchorOffset = Vector2.ZERO,
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.None,
				Color = new Vector4(0.4f, 1f, 0.4f, 0f),
				Rotation = 0f,
				Scale = playMenuLockedGoldStarTex.Scale * 0.9f,
				Texture = AssetLocator.Tick,
				ZIndex = playMenuMainPanel.ZIndex + 2
			};
			playMenuLockedCoinTex = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = playMenuMainPanel.Anchoring,
				AnchorOffset = new Vector2(0f, playMenuLockedCoinString.AnchorOffset.Y + 0.025f),
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.None,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = playMenuLockedGoldStarTex.Scale * 0.875f,
				Texture = AssetLocator.CoinFrames[0],
				ZIndex = playMenuMainPanel.ZIndex + 1
			};
			playMenuLockedCoinTick = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = playMenuMainPanel.Anchoring,
				AnchorOffset = Vector2.ZERO,
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.None,
				Color = new Vector4(0.4f, 1f, 0.4f, 0f),
				Rotation = 0f,
				Scale = playMenuLockedGoldStarTex.Scale * 0.9f,
				Texture = AssetLocator.Tick,
				ZIndex = playMenuMainPanel.ZIndex + 2
			};

			playMenuEggSelectTitleString = AssetLocator.TitleFont.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.BottomRight,
				new Vector2(0.07f, 0.18f),
				Vector2.ONE * 0.72f
			);
			playMenuEggSelectTitleString.Color = new Vector4(0.85f * 0.4f, 1f * 0.55f, 0.85f * 0.2f, 0f);
			playMenuEggSelectTitleString.Text = "Egg Select";

			playMenuEggSelectMaterialNameString = AssetLocator.TitleFontGlow.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.BottomRight,
				new Vector2(0.02f, 0.15f),
				Vector2.ONE * 0.45f
			);
			playMenuEggSelectMaterialNameString.Color = new Vector4(PLAY_MENU_EGG_SELECT_MAT_TITLE_COLOR, w: 0f);
			playMenuEggSelectMaterialNameString.Text = "Lizard Egg";

			playMenuEggSelectLeftChevron = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = ViewportAnchoring.BottomRight,
				AnchorOffset = new Vector2(PLAY_MENU_EGG_SELECT_LEFT_CHEVRON_STARTING_X, 0.03f),
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = MathUtils.PI,
				Scale = new Vector2(0.08f, 0.12f),
				Texture = AssetLocator.Chevrons,
				ZIndex = 1
			};
			playMenuEggSelectRightChevron = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = ViewportAnchoring.BottomRight,
				AnchorOffset = new Vector2(PLAY_MENU_EGG_SELECT_RIGHT_CHEVRON_STARTING_X, playMenuEggSelectLeftChevron.AnchorOffset.Y),
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = playMenuEggSelectLeftChevron.Scale,
				Texture = AssetLocator.Chevrons,
				ZIndex = 1
			};
			playMenuEggSelectLockedGoldenEggTex = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = ViewportAnchoring.BottomRight,
				AnchorOffset = new Vector2(0.225f, 0.055f),
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = new Vector2(0.08f, 0.12f) * 0.65f,
				Texture = AssetLocator.VultureEggInvert,
			};
			playMenuEggSelectLockedGoldenEggString = AssetLocator.MainFont.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.BottomRight,
				new Vector2(0.135f, 0.055f),
				Vector2.ONE * 1f
			);
			playMenuEggSelectLockedGoldenEggString.Color = new Vector4(1f, 0.85f, 0.55f, 0f);
			playMenuEggSelectLockedGoldenEggString.Text = "x 110";

			playMenuSelectedEggEntity = new GeometryEntity();
			playMenuSelectedEggEntity.SetScale(Vector3.ONE * 0.33f);
			playMenuSelectedEggEntity.SetTranslation(AssetLocator.MainCamera.Position + Vector3.UP * 100f * PhysicsManager.ONE_METRE_SCALED);

			playMenuWorldPreviewTime = 0f;
			playMenuSelectedWorldIndex = 0;
			playMenuSelectedLevelIndex = 0;
			playMenuOnLevelSelect = false;
			playMenuTimeSinceLastHeaderCharChange = 0f;
		}

		private static void PlayMenuDisposeComponents() {
			triggerUp -= PlayMenuChangeOptionUp;
			triggerDown -= PlayMenuChangeOptionDown;
			triggerLeft -= PlayMenuChangeOptionLeft;
			triggerRight -= PlayMenuChangeOptionRight;
			triggerConfirm -= PlayMenuConfirmOption;
			triggerBackOut -= PlayMenuBackOut;
			tick -= PlayMenuTick;

			for (int i = 0; i < playMenuWorldSelectIcons.Length; ++i) {
				if (playMenuWorldSelectIcons[i] != null) playMenuWorldSelectIcons[i].Dispose();
				playMenuWorldSelectIcons[i] = null;
			}

			if (playMenuWorldIconHighlight != null) {
				playMenuWorldIconHighlight.Dispose();
				playMenuWorldIconHighlight = null;
			}
			if (playMenuWorldIconHighlightExciter != null) {
				playMenuWorldIconHighlightExciter.Dispose();
				playMenuWorldIconHighlightExciter = null;
			}
			if (playMenuMainPanel != null) {
				playMenuMainPanel.Dispose();
				playMenuMainPanel = null;
			}
			for (int i = 0; i < playMenuLevelBars.Length; ++i) {
				if (playMenuLevelBars[i] != null) playMenuLevelBars[i].Dispose();
				playMenuLevelBars[i] = null;
			}
			for (int i = 0; i < playMenuLevelNumberStrings.Length; ++i) {
				if (playMenuLevelNumberStrings[i] != null) playMenuLevelNumberStrings[i].Dispose();
				playMenuLevelNumberStrings[i] = null;
			}
			for (int i = 0; i < playMenuLevelNameStrings.Length; ++i) {
				if (playMenuLevelNameStrings[i] != null) playMenuLevelNameStrings[i].Dispose();
				playMenuLevelNameStrings[i] = null;
			}
			for (int i = 0; i < playMenuLevelDifficultyIcons.Length; ++i) {
				if (playMenuLevelDifficultyIcons[i] != null) playMenuLevelDifficultyIcons[i].Dispose();
				playMenuLevelDifficultyIcons[i] = null;
			}
			foreach (var tex in playMenuLevelBarAchievementTextures) {
				tex.Dispose();
			}
			playMenuLevelBarAchievementTextures.Clear();

			if (playMenuLevelBarHighlight != null) {
				playMenuLevelBarHighlight.Dispose();
				playMenuLevelBarHighlight = null;
			}
			if (playMenuLevelNumberExciter != null) {
				playMenuLevelNumberExciter.Dispose();
				playMenuLevelNumberExciter = null;
			}
			if (playMenuLevelNameExciter != null) {
				playMenuLevelNameExciter.Dispose();
				playMenuLevelNameExciter = null;
			}
			if (playMenuLevelBarExciter != null) {
				playMenuLevelBarExciter.Dispose();
				playMenuLevelBarExciter = null;
			}
			if (playMenuMPWorldIcon != null) {
				playMenuMPWorldIcon.Dispose();
				playMenuMPWorldIcon = null;
			}
			if (playMenuMPTitleString != null) {
				playMenuMPTitleString.Dispose();
				playMenuMPTitleString = null;
			}
			if (playMenuMPCompletionTitleString != null) {
				playMenuMPCompletionTitleString.Dispose();
				playMenuMPCompletionTitleString = null;
			}

			if (playMenuMPWorldCompGoldStar != null) {
				playMenuMPWorldCompGoldStar.Dispose();
				playMenuMPWorldCompGoldStar = null;
			}
			if (playMenuMPWorldCompSilverStar != null) {
				playMenuMPWorldCompSilverStar.Dispose();
				playMenuMPWorldCompSilverStar = null;
			}
			if (playMenuMPWorldCompBronzeStar != null) {
				playMenuMPWorldCompBronzeStar.Dispose();
				playMenuMPWorldCompBronzeStar = null;
			}
			if (playMenuMPWorldCompCoin != null) {
				playMenuMPWorldCompCoin.Dispose();
				playMenuMPWorldCompCoin = null;
			}
			if (playMenuMPWorldCompEgg != null) {
				playMenuMPWorldCompEgg.Dispose();
				playMenuMPWorldCompEgg = null;
			}
			if (playMenuMPWorldCompGoldStarString != null) {
				playMenuMPWorldCompGoldStarString.Dispose();
				playMenuMPWorldCompGoldStarString = null;
			}
			if (playMenuMPWorldCompSilverStarString != null) {
				playMenuMPWorldCompSilverStarString.Dispose();
				playMenuMPWorldCompSilverStarString = null;
			}
			if (playMenuMPWorldCompBronzeStarString != null) {
				playMenuMPWorldCompBronzeStarString.Dispose();
				playMenuMPWorldCompBronzeStarString = null;
			}
			if (playMenuMPWorldCompCoinString != null) {
				playMenuMPWorldCompCoinString.Dispose();
				playMenuMPWorldCompCoinString = null;
			}
			if (playMenuMPWorldCompEggString != null) {
				playMenuMPWorldCompEggString.Dispose();
				playMenuMPWorldCompEggString = null;
			}

			if (playMenuMPLevelCompGoldStarIndent != null) {
				playMenuMPLevelCompGoldStarIndent.Dispose();
				playMenuMPLevelCompGoldStarIndent = null;
			}
			if (playMenuMPLevelCompSilverStarIndent != null) {
				playMenuMPLevelCompSilverStarIndent.Dispose();
				playMenuMPLevelCompSilverStarIndent = null;
			}
			if (playMenuMPLevelCompBronzeStarIndent != null) {
				playMenuMPLevelCompBronzeStarIndent.Dispose();
				playMenuMPLevelCompBronzeStarIndent = null;
			}
			if (playMenuMPLevelCompEggIndent != null) {
				playMenuMPLevelCompEggIndent.Dispose();
				playMenuMPLevelCompEggIndent = null;
			}
			if (playMenuMPLevelCompCoinIndent != null) {
				playMenuMPLevelCompCoinIndent.Dispose();
				playMenuMPLevelCompCoinIndent = null;
			}
			if (playMenuMPLevelCompGoldStar != null) {
				playMenuMPLevelCompGoldStar.Dispose();
				playMenuMPLevelCompGoldStar = null;
			}
			if (playMenuMPLevelCompSilverStar != null) {
				playMenuMPLevelCompSilverStar.Dispose();
				playMenuMPLevelCompSilverStar = null;
			}
			if (playMenuMPLevelCompBronzeStar != null) {
				playMenuMPLevelCompBronzeStar.Dispose();
				playMenuMPLevelCompBronzeStar = null;
			}
			if (playMenuMPLevelCompEgg != null) {
				playMenuMPLevelCompEgg.Dispose();
				playMenuMPLevelCompEgg = null;
			}
			foreach (var tex in playMenuMPLevelCompCoins) {
				tex.Dispose();
			}
			playMenuMPLevelCompCoins.Clear();

			if (playMenuMPLevelCompTimesTitle != null) {
				playMenuMPLevelCompTimesTitle.Dispose();
				playMenuMPLevelCompTimesTitle = null;
			}
			if (playMenuMPLevelCompTimesGoldString != null) {
				playMenuMPLevelCompTimesGoldString.Dispose();
				playMenuMPLevelCompTimesGoldString = null;
			}
			if (playMenuMPLevelCompTimesSilverString != null) {
				playMenuMPLevelCompTimesSilverString.Dispose();
				playMenuMPLevelCompTimesSilverString = null;
			}
			if (playMenuMPLevelCompTimesBronzeString != null) {
				playMenuMPLevelCompTimesBronzeString.Dispose();
				playMenuMPLevelCompTimesBronzeString = null;
			}
			if (playMenuMPLevelCompTimesPersonalString != null) {
				playMenuMPLevelCompTimesPersonalString.Dispose();
				playMenuMPLevelCompTimesPersonalString = null;
			}
			if (playMenuMPLevelCompTimesGoldStar != null) {
				playMenuMPLevelCompTimesGoldStar.Dispose();
				playMenuMPLevelCompTimesGoldStar = null;
			}
			if (playMenuMPLevelCompTimesSilverStar != null) {
				playMenuMPLevelCompTimesSilverStar.Dispose();
				playMenuMPLevelCompTimesSilverStar = null;
			}
			if (playMenuMPLevelCompTimesBronzeStar != null) {
				playMenuMPLevelCompTimesBronzeStar.Dispose();
				playMenuMPLevelCompTimesBronzeStar = null;
			}
			if (playMenuMPLevelCompTimesPersonalBell != null) {
				playMenuMPLevelCompTimesPersonalBell.Dispose();
				playMenuMPLevelCompTimesPersonalBell = null;
			}

			if (playMenuSecondaryPanel != null) {
				playMenuSecondaryPanel.Dispose();
				playMenuSecondaryPanel = null;
			}
			if (playMenuCareerStatsTitleString != null) {
				playMenuCareerStatsTitleString.Dispose();
				playMenuCareerStatsTitleString = null;
			}
			if (playMenuCareerGoldStar != null) {
				playMenuCareerGoldStar.Dispose();
				playMenuCareerGoldStar = null;
			}
			if (playMenuCareerSilverStar != null) {
				playMenuCareerSilverStar.Dispose();
				playMenuCareerSilverStar = null;
			}
			if (playMenuCareerBronzeStar != null) {
				playMenuCareerBronzeStar.Dispose();
				playMenuCareerBronzeStar = null;
			}
			if (playMenuCareerCoin != null) {
				playMenuCareerCoin.Dispose();
				playMenuCareerCoin = null;
			}
			if (playMenuCareerEgg != null) {
				playMenuCareerEgg.Dispose();
				playMenuCareerEgg = null;
			}
			if (playMenuCareerGoldStarXString != null) {
				playMenuCareerGoldStarXString.Dispose();
				playMenuCareerGoldStarXString = null;
			}
			if (playMenuCareerGoldStarString != null) {
				playMenuCareerGoldStarString.Dispose();
				playMenuCareerGoldStarString = null;
			}
			if (playMenuCareerSilverStarXString != null) {
				playMenuCareerSilverStarXString.Dispose();
				playMenuCareerSilverStarXString = null;
			}
			if (playMenuCareerSilverStarString != null) {
				playMenuCareerSilverStarString.Dispose();
				playMenuCareerSilverStarString = null;
			}
			if (playMenuCareerBronzeStarXString != null) {
				playMenuCareerBronzeStarXString.Dispose();
				playMenuCareerBronzeStarXString = null;
			}
			if (playMenuCareerBronzeStarString != null) {
				playMenuCareerBronzeStarString.Dispose();
				playMenuCareerBronzeStarString = null;
			}
			if (playMenuCareerCoinString != null) {
				playMenuCareerCoinString.Dispose();
				playMenuCareerCoinString = null;
			}
			if (playMenuCareerEggString != null) {
				playMenuCareerEggString.Dispose();
				playMenuCareerEggString = null;
			}

			if (playMenuPreviewBracketLeft != null) {
				playMenuPreviewBracketLeft.Dispose();
				playMenuPreviewBracketLeft = null;
			}
			if (playMenuPreviewBracketRight != null) {
				playMenuPreviewBracketRight.Dispose();
				playMenuPreviewBracketRight = null;
			}
			if (playMenuPreviewIndent != null) {
				playMenuPreviewIndent.Dispose();
				playMenuPreviewIndent = null;
			}
			if (playMenuPreviewFront != null) {
				playMenuPreviewFront.Dispose();
				playMenuPreviewFront = null;
			}
			if (playMenuPreviewBack != null) {
				playMenuPreviewBack.Dispose();
				playMenuPreviewBack = null;
			}

			playMenuIncreasingAlphaTextures.Clear();
			playMenuDecreasingAlphaTextures.Clear();
			playMenuIncDecTexRemovalWorkspace.Clear();

			if (playMenuLockedTitleString != null) {
				playMenuLockedTitleString.Dispose();
				playMenuLockedTitleString = null;
			}
			if (playMenuLockedTitleExciter != null) {
				playMenuLockedTitleExciter.Dispose();
				playMenuLockedTitleExciter = null;
			}
			if (playMenuLockedCriteriaTitleString != null) {
				playMenuLockedCriteriaTitleString.Dispose();
				playMenuLockedCriteriaTitleString = null;
			}
			if (playMenuLockedGoldStarString != null) {
				playMenuLockedGoldStarString.Dispose();
				playMenuLockedGoldStarString = null;
			}
			if (playMenuLockedSilverStarString != null) {
				playMenuLockedSilverStarString.Dispose();
				playMenuLockedSilverStarString = null;
			}
			if (playMenuLockedBronzeStarString != null) {
				playMenuLockedBronzeStarString.Dispose();
				playMenuLockedBronzeStarString = null;
			}
			if (playMenuLockedCoinString != null) {
				playMenuLockedCoinString.Dispose();
				playMenuLockedCoinString = null;
			}
			if (playMenuLockedGoldStarTex != null) {
				playMenuLockedGoldStarTex.Dispose();
				playMenuLockedGoldStarTex = null;
			}
			if (playMenuLockedSilverStarTex != null) {
				playMenuLockedSilverStarTex.Dispose();
				playMenuLockedSilverStarTex = null;
			}
			if (playMenuLockedBronzeStarTex != null) {
				playMenuLockedBronzeStarTex.Dispose();
				playMenuLockedBronzeStarTex = null;
			}
			if (playMenuLockedCoinTex != null) {
				playMenuLockedCoinTex.Dispose();
				playMenuLockedCoinTex = null;
			}
			if (playMenuLockedGoldStarTick != null) {
				playMenuLockedGoldStarTick.Dispose();
				playMenuLockedGoldStarTick = null;
			}
			if (playMenuLockedSilverStarTick != null) {
				playMenuLockedSilverStarTick.Dispose();
				playMenuLockedSilverStarTick = null;
			}
			if (playMenuLockedBronzeStarTick != null) {
				playMenuLockedBronzeStarTick.Dispose();
				playMenuLockedBronzeStarTick = null;
			}
			if (playMenuLockedCoinTick != null) {
				playMenuLockedCoinTick.Dispose();
				playMenuLockedCoinTick = null;
			}

			if (playMenuEggSelectTitleString != null) {
				playMenuEggSelectTitleString.Dispose();
				playMenuEggSelectTitleString = null;
			}
			if (playMenuEggSelectMaterialNameString != null) {
				playMenuEggSelectMaterialNameString.Dispose();
				playMenuEggSelectMaterialNameString = null;
			}
			if (playMenuEggSelectLeftChevron != null) {
				playMenuEggSelectLeftChevron.Dispose();
				playMenuEggSelectLeftChevron = null;
			}
			if (playMenuEggSelectRightChevron != null) {
				playMenuEggSelectRightChevron.Dispose();
				playMenuEggSelectRightChevron = null;
			}
			if (playMenuEggSelectLockedGoldenEggTex != null) {
				playMenuEggSelectLockedGoldenEggTex.Dispose();
				playMenuEggSelectLockedGoldenEggTex = null;
			}
			if (playMenuEggSelectLockedGoldenEggString != null) {
				playMenuEggSelectLockedGoldenEggString.Dispose();
				playMenuEggSelectLockedGoldenEggString = null;
			}
			if (playMenuSelectedEggEntity != null && !playMenuSelectedEggEntity.IsDisposed) {
				playMenuSelectedEggEntity.Dispose();
				playMenuSelectedEggEntity = null;
			}
		}

		private static void PlayMenuTickTransitionIn(float deltaTime, float fracComplete) {
			SetIdentTransitionAmount(fracComplete, false);

			for (int i = 0; i < 11; ++i) {
				float targetY = PLAY_MENU_WORLD_ICON_Y_START + PLAY_MENU_WORLD_ICON_ROW_Y * (i / 3);
				playMenuWorldSelectIcons[i].AnchorOffset = new Vector2(playMenuWorldSelectIcons[i].AnchorOffset.X, targetY * fracComplete);
				playMenuWorldSelectIcons[i].SetAlpha(fracComplete);
			}

			playMenuMainPanel.AnchorOffset = new Vector2(
				PLAY_MENU_MAIN_PANEL_STARTING_X + (PLAY_MENU_MAIN_PANEL_FIRST_TARGET_X - PLAY_MENU_MAIN_PANEL_STARTING_X) * fracComplete, 
				playMenuMainPanel.AnchorOffset.Y
			);
			playMenuMainPanel.SetAlpha(fracComplete);

			if (fracComplete >= 1f) {
				playMenuWorldIconHighlight.SetAlpha(1f);
				playMenuWorldIconHighlight.AnchorOffset = playMenuWorldSelectIcons[0].AnchorOffset;
			}

			playMenuMPWorldIcon.AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_WORLD_ICON_OFFSET;

			playMenuMPWorldIcon.SetAlpha(fracComplete);
			playMenuMPTitleString.SetAlpha(Math.Max(fracComplete - 0.8f, 0f) * 5f * PLAY_MENU_MP_TITLE_MAX_ALPHA);
			playMenuMPCompletionTitleString.SetAlpha(Math.Max(fracComplete - 0.8f, 0f) * 5f * PLAY_MENU_MP_COMP_STR_MAX_ALPHA);
			playMenuPreviewBracketLeft.SetAlpha(fracComplete);
			playMenuPreviewBracketRight.SetAlpha(fracComplete);
			playMenuPreviewIndent.SetAlpha(fracComplete);
			playMenuPreviewBack.SetAlpha(fracComplete);
			playMenuPreviewBracketLeft.AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_BRACKET_LEFT_OFFSET;
			playMenuPreviewBracketRight.AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_BRACKET_RIGHT_OFFSET;
			playMenuPreviewFront.AnchorOffset = playMenuPreviewBack.AnchorOffset = playMenuPreviewIndent.AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_PREVIEW_OFFSET;

			playMenuMPWorldCompGoldStar.SetAlpha(Math.Max(fracComplete - 0.8f, 0f) * 5f * PLAY_MENU_MP_WC_ICON_ALPHA);
			playMenuMPWorldCompSilverStar.SetAlpha(Math.Max(fracComplete - 0.8f, 0f) * 5f * PLAY_MENU_MP_WC_ICON_ALPHA);
			playMenuMPWorldCompBronzeStar.SetAlpha(Math.Max(fracComplete - 0.8f, 0f) * 5f * PLAY_MENU_MP_WC_ICON_ALPHA);
			playMenuMPWorldCompCoin.SetAlpha(Math.Max(fracComplete - 0.8f, 0f) * 5f * PLAY_MENU_MP_WC_ICON_ALPHA);
			playMenuMPWorldCompEgg.SetAlpha(Math.Max(fracComplete - 0.8f, 0f) * 5f * PLAY_MENU_MP_WC_ICON_ALPHA);

			playMenuMPWorldCompGoldStarString.SetAlpha(Math.Max(fracComplete - 0.8f, 0f) * 5f * PLAY_MENU_MP_WC_STRING_MAX_ALPHA);
			playMenuMPWorldCompSilverStarString.SetAlpha(Math.Max(fracComplete - 0.8f, 0f) * 5f * PLAY_MENU_MP_WC_STRING_MAX_ALPHA);
			playMenuMPWorldCompBronzeStarString.SetAlpha(Math.Max(fracComplete - 0.8f, 0f) * 5f * PLAY_MENU_MP_WC_STRING_MAX_ALPHA);
			playMenuMPWorldCompCoinString.SetAlpha(Math.Max(fracComplete - 0.8f, 0f) * 5f * PLAY_MENU_MP_WC_STRING_MAX_ALPHA);
			playMenuMPWorldCompEggString.SetAlpha(Math.Max(fracComplete - 0.8f, 0f) * 5f * PLAY_MENU_MP_WC_STRING_MAX_ALPHA);

			playMenuMPWorldCompGoldStar.AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_WC_GOLD_STAR_OFFSET;
			playMenuMPWorldCompSilverStar.AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_WC_SILVER_STAR_OFFSET;
			playMenuMPWorldCompBronzeStar.AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_WC_BRONZE_STAR_OFFSET;
			playMenuMPWorldCompCoin.AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_WC_COIN_OFFSET;
			playMenuMPWorldCompEgg.AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_WC_EGG_OFFSET;

			playMenuSecondaryPanel.SetAlpha(fracComplete);
			playMenuSecondaryPanel.AnchorOffset = playMenuMainPanel.AnchorOffset + new Vector2(0f, PLAY_MENU_SECONDARY_PANEL_VERT_OFFSET * fracComplete);

			playMenuCareerStatsTitleString.SetAlpha(fracComplete);
			playMenuCareerGoldStar.SetAlpha(fracComplete);
			playMenuCareerSilverStar.SetAlpha(fracComplete);
			playMenuCareerBronzeStar.SetAlpha(fracComplete);
			playMenuCareerCoin.SetAlpha(fracComplete);
			playMenuCareerEgg.SetAlpha(fracComplete);
			playMenuCareerGoldStarXString.SetAlpha(fracComplete);
			playMenuCareerGoldStarString.SetAlpha(fracComplete);
			playMenuCareerSilverStarXString.SetAlpha(fracComplete);
			playMenuCareerSilverStarString.SetAlpha(fracComplete);
			playMenuCareerBronzeStarXString.SetAlpha(fracComplete);
			playMenuCareerBronzeStarString.SetAlpha(fracComplete);
			playMenuCareerCoinString.SetAlpha(fracComplete * PLAY_MENU_CAREER_COIN_EGG_STR_ALPHA);
			playMenuCareerEggString.SetAlpha(fracComplete * PLAY_MENU_CAREER_COIN_EGG_STR_ALPHA);
			playMenuCareerStatsTitleString.AnchorOffset = playMenuSecondaryPanel.AnchorOffset + PLAY_MENU_CAREER_TITLE_OFFSET;
			playMenuCareerGoldStar.AnchorOffset = playMenuSecondaryPanel.AnchorOffset + PLAY_MENU_CAREER_GOLD_STAR_OFFSET;
			playMenuCareerSilverStar.AnchorOffset = playMenuSecondaryPanel.AnchorOffset + PLAY_MENU_CAREER_SILVER_STAR_OFFSET;
			playMenuCareerBronzeStar.AnchorOffset = playMenuSecondaryPanel.AnchorOffset + PLAY_MENU_CAREER_BRONZE_STAR_OFFSET;
			playMenuCareerCoin.AnchorOffset = playMenuSecondaryPanel.AnchorOffset + PLAY_MENU_CAREER_COIN_OFFSET;
			playMenuCareerEgg.AnchorOffset = playMenuSecondaryPanel.AnchorOffset + PLAY_MENU_CAREER_EGG_OFFSET;
			playMenuCareerGoldStarXString.AnchorOffset = playMenuCareerGoldStar.AnchorOffset + PLAY_MENU_CAREER_STAR_STRING_X_OFFSET;
			playMenuCareerGoldStarString.AnchorOffset = playMenuCareerGoldStar.AnchorOffset + PLAY_MENU_CAREER_STAR_STRING_OFFSET;
			playMenuCareerSilverStarXString.AnchorOffset = playMenuCareerSilverStar.AnchorOffset + PLAY_MENU_CAREER_STAR_STRING_X_OFFSET;
			playMenuCareerSilverStarString.AnchorOffset = playMenuCareerSilverStar.AnchorOffset + PLAY_MENU_CAREER_STAR_STRING_OFFSET;
			playMenuCareerBronzeStarXString.AnchorOffset = playMenuCareerBronzeStar.AnchorOffset + PLAY_MENU_CAREER_STAR_STRING_X_OFFSET;
			playMenuCareerBronzeStarString.AnchorOffset = playMenuCareerBronzeStar.AnchorOffset + PLAY_MENU_CAREER_STAR_STRING_OFFSET;
			playMenuCareerCoinString.AnchorOffset = playMenuCareerCoin.AnchorOffset + PLAY_MENU_CAREER_COIN_STRING_OFFSET;
			playMenuCareerEggString.AnchorOffset = playMenuCareerEgg.AnchorOffset + PLAY_MENU_CAREER_EGG_STRING_OFFSET;
		}

		private static void PlayMenuTickTransitionOut(float deltaTime, float fracComplete) {
			SetIdentTransitionAmount(fracComplete, true);

			for (int i = 0; i < 11; ++i) {
				playMenuWorldSelectIcons[i].AnchorOffset = new Vector2(playMenuWorldSelectIcons[i].AnchorOffset.X, PLAY_MENU_WORLD_ICON_Y_START + (PLAY_MENU_WORLD_ICON_ROW_Y * (i / 3)) * (1f - fracComplete));
				playMenuWorldSelectIcons[i].SetAlpha(1f - fracComplete);
			}

			playMenuMainPanel.AnchorOffset = new Vector2(
				PLAY_MENU_MAIN_PANEL_STARTING_X + (PLAY_MENU_MAIN_PANEL_FIRST_TARGET_X - PLAY_MENU_MAIN_PANEL_STARTING_X) * (1f - fracComplete),
				playMenuMainPanel.AnchorOffset.Y
			);
			playMenuMainPanel.SetAlpha(1f - fracComplete);

			playMenuCareerStatsTitleString.SetAlpha(1f - fracComplete);
			playMenuCareerGoldStar.SetAlpha(1f - fracComplete);
			playMenuCareerSilverStar.SetAlpha(1f - fracComplete);
			playMenuCareerBronzeStar.SetAlpha(1f - fracComplete);
			playMenuCareerCoin.SetAlpha(1f - fracComplete);
			playMenuCareerEgg.SetAlpha(1f - fracComplete);
			playMenuCareerGoldStarXString.SetAlpha(1f - fracComplete);
			playMenuCareerGoldStarString.SetAlpha(1f - fracComplete);
			playMenuCareerSilverStarXString.SetAlpha(1f - fracComplete);
			playMenuCareerSilverStarString.SetAlpha(1f - fracComplete);
			playMenuCareerBronzeStarXString.SetAlpha(1f - fracComplete);
			playMenuCareerBronzeStarString.SetAlpha(1f - fracComplete);
			playMenuCareerCoinString.SetAlpha((1f - fracComplete) * PLAY_MENU_CAREER_COIN_EGG_STR_ALPHA);
			playMenuCareerEggString.SetAlpha((1f - fracComplete) * PLAY_MENU_CAREER_COIN_EGG_STR_ALPHA);

			playMenuWorldIconHighlight.SetAlpha(0f);



			playMenuMPWorldIcon.SetAlpha(1f - fracComplete);
			playMenuMPTitleString.SetAlpha(Math.Max((1f - fracComplete) - 0.8f, 0f) * 5f * PLAY_MENU_MP_TITLE_MAX_ALPHA);
			playMenuMPCompletionTitleString.SetAlpha(Math.Max((1f - fracComplete) - 0.8f, 0f) * 5f * PLAY_MENU_MP_COMP_STR_MAX_ALPHA);
			playMenuPreviewBracketLeft.SetAlpha(1f - fracComplete);
			playMenuPreviewBracketRight.SetAlpha(1f - fracComplete);
			playMenuPreviewIndent.SetAlpha(1f - fracComplete);
			playMenuPreviewBack.SetAlpha(1f - fracComplete);

			playMenuMPWorldCompGoldStar.SetAlpha(Math.Max((1f - fracComplete) - 0.8f, 0f) * 5f * PLAY_MENU_MP_WC_ICON_ALPHA);
			playMenuMPWorldCompSilverStar.SetAlpha(Math.Max((1f - fracComplete) - 0.8f, 0f) * 5f * PLAY_MENU_MP_WC_ICON_ALPHA);
			playMenuMPWorldCompBronzeStar.SetAlpha(Math.Max((1f - fracComplete) - 0.8f, 0f) * 5f * PLAY_MENU_MP_WC_ICON_ALPHA);
			playMenuMPWorldCompCoin.SetAlpha(Math.Max((1f - fracComplete) - 0.8f, 0f) * 5f * PLAY_MENU_MP_WC_ICON_ALPHA);
			playMenuMPWorldCompEgg.SetAlpha(Math.Max((1f - fracComplete) - 0.8f, 0f) * 5f * PLAY_MENU_MP_WC_ICON_ALPHA);

			playMenuMPWorldCompGoldStarString.SetAlpha(Math.Max((1f - fracComplete) - 0.8f, 0f) * 5f * PLAY_MENU_MP_WC_STRING_MAX_ALPHA);
			playMenuMPWorldCompSilverStarString.SetAlpha(Math.Max((1f - fracComplete) - 0.8f, 0f) * 5f * PLAY_MENU_MP_WC_STRING_MAX_ALPHA);
			playMenuMPWorldCompBronzeStarString.SetAlpha(Math.Max((1f - fracComplete) - 0.8f, 0f) * 5f * PLAY_MENU_MP_WC_STRING_MAX_ALPHA);
			playMenuMPWorldCompCoinString.SetAlpha(Math.Max((1f - fracComplete) - 0.8f, 0f) * 5f * PLAY_MENU_MP_WC_STRING_MAX_ALPHA);
			playMenuMPWorldCompEggString.SetAlpha(Math.Max((1f - fracComplete) - 0.8f, 0f) * 5f * PLAY_MENU_MP_WC_STRING_MAX_ALPHA);

			playMenuSecondaryPanel.SetAlpha(1f - fracComplete);
			playMenuSecondaryPanel.AnchorOffset = playMenuMainPanel.AnchorOffset + new Vector2(0f, PLAY_MENU_SECONDARY_PANEL_VERT_OFFSET * (1f - fracComplete));

			playMenuPreviewFront.AnchorOffset = playMenuPreviewBack.AnchorOffset = playMenuPreviewIndent.AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_PREVIEW_OFFSET;
			playMenuPreviewBracketLeft.AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_BRACKET_LEFT_OFFSET;
			playMenuPreviewBracketRight.AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_BRACKET_RIGHT_OFFSET;
			playMenuMPWorldCompGoldStar.AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_WC_GOLD_STAR_OFFSET;
			playMenuMPWorldCompSilverStar.AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_WC_SILVER_STAR_OFFSET;
			playMenuMPWorldCompBronzeStar.AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_WC_BRONZE_STAR_OFFSET;
			playMenuMPWorldCompCoin.AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_WC_COIN_OFFSET;
			playMenuMPWorldCompEgg.AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_WC_EGG_OFFSET;
			playMenuMPWorldIcon.AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_WORLD_ICON_OFFSET;
			playMenuCareerStatsTitleString.AnchorOffset = playMenuSecondaryPanel.AnchorOffset + PLAY_MENU_CAREER_TITLE_OFFSET;
			playMenuCareerGoldStar.AnchorOffset = playMenuSecondaryPanel.AnchorOffset + PLAY_MENU_CAREER_GOLD_STAR_OFFSET;
			playMenuCareerSilverStar.AnchorOffset = playMenuSecondaryPanel.AnchorOffset + PLAY_MENU_CAREER_SILVER_STAR_OFFSET;
			playMenuCareerBronzeStar.AnchorOffset = playMenuSecondaryPanel.AnchorOffset + PLAY_MENU_CAREER_BRONZE_STAR_OFFSET;
			playMenuCareerCoin.AnchorOffset = playMenuSecondaryPanel.AnchorOffset + PLAY_MENU_CAREER_COIN_OFFSET;
			playMenuCareerEgg.AnchorOffset = playMenuSecondaryPanel.AnchorOffset + PLAY_MENU_CAREER_EGG_OFFSET;
			playMenuCareerGoldStarXString.AnchorOffset = playMenuCareerGoldStar.AnchorOffset + PLAY_MENU_CAREER_STAR_STRING_X_OFFSET;
			playMenuCareerGoldStarString.AnchorOffset = playMenuCareerGoldStar.AnchorOffset + PLAY_MENU_CAREER_STAR_STRING_OFFSET;
			playMenuCareerSilverStarXString.AnchorOffset = playMenuCareerSilverStar.AnchorOffset + PLAY_MENU_CAREER_STAR_STRING_X_OFFSET;
			playMenuCareerSilverStarString.AnchorOffset = playMenuCareerSilverStar.AnchorOffset + PLAY_MENU_CAREER_STAR_STRING_OFFSET;
			playMenuCareerBronzeStarXString.AnchorOffset = playMenuCareerBronzeStar.AnchorOffset + PLAY_MENU_CAREER_STAR_STRING_X_OFFSET;
			playMenuCareerBronzeStarString.AnchorOffset = playMenuCareerBronzeStar.AnchorOffset + PLAY_MENU_CAREER_STAR_STRING_OFFSET;
			playMenuCareerCoinString.AnchorOffset = playMenuCareerCoin.AnchorOffset + PLAY_MENU_CAREER_COIN_STRING_OFFSET;
			playMenuCareerEggString.AnchorOffset = playMenuCareerEgg.AnchorOffset + PLAY_MENU_CAREER_EGG_STRING_OFFSET;

			playMenuEggSelectTitleString.SetAlpha(Math.Min(playMenuEggSelectTitleString.Color.W, 1f - fracComplete));
			playMenuEggSelectMaterialNameString.SetAlpha(Math.Min(playMenuEggSelectMaterialNameString.Color.W, 1f - fracComplete));
			playMenuEggSelectLeftChevron.SetAlpha(Math.Min(playMenuEggSelectLeftChevron.Color.W, 1f - fracComplete));
			playMenuEggSelectRightChevron.SetAlpha(Math.Min(playMenuEggSelectRightChevron.Color.W, 1f - fracComplete));
			playMenuEggSelectLockedGoldenEggTex.SetAlpha(Math.Min(playMenuEggSelectLockedGoldenEggTex.Color.W, 1f - fracComplete));
			playMenuEggSelectLockedGoldenEggString.SetAlpha(Math.Min(playMenuEggSelectLockedGoldenEggString.Color.W, 1f - fracComplete));
		}

		private static void PlayMenuProgressToLevelSelect() {
			//if (playMenuSelectedWorldIndex == 2 || playMenuSelectedWorldIndex == 6) {
			//	HUDSound.UIUnavailable.Play();
			//	return;
			//}

			if (!inDeferenceMode) HUDSound.XAnnounceLevelSelect.Play();

			for (int i = 0; i < 10; ++i) {
				playMenuLevelNumberStrings[i].Text = (playMenuSelectedWorldIndex + 1) + ":" + (i + 1);
				playMenuLevelNameStrings[i].Text = LevelDatabase.GetLevelTitle(new LevelID((byte) playMenuSelectedWorldIndex, (byte) i));
				playMenuLevelDifficultyIcons[i].Texture = LevelDatabase.GetLevelDifficulty(new LevelID((byte) playMenuSelectedWorldIndex, (byte) i)).Tex();
				playMenuLevelDifficultyIcons[i].Color = LevelDatabase.GetLevelDifficulty(new LevelID((byte) playMenuSelectedWorldIndex, (byte) i)).Color();
			}
			playMenuOnLevelSelect = true;
			AddTransitionTickEvent(PlayMenuProgressionTransitionIn);
			BeginTransition(PLAY_MENU_PROGRESSION_TRANSITION_TIME);
			PlayMenuSetSelectedLevelIndex(0);
			playMenuWorldIconHighlightExciter.TargetObj = null;
			playMenuWorldIconHighlightExciter.ClearAll();

			playMenuLevelBarExciter.TargetObj = playMenuLevelBarHighlight;

			playMenuMPWorldIcon.SetAlpha(1f);
			playMenuMPTitleString.SetAlpha(PLAY_MENU_MP_TITLE_MAX_ALPHA);

			foreach (HUDTexture tex in playMenuLevelBarAchievementTextures) {
				tex.Dispose();
			}
			playMenuLevelBarAchievementTextures.Clear();

			for (int i = 0; i < 10; ++i) {
				const float DEFAULT_OFFSET = 0.004f;
				const float OFFSET_PER_ICON = 0.022f;
				Vector2 levelNameTextLeftEnd = new Vector2(
					PLAY_MENU_LEVEL_BAR_NAME_STRING_OFFSET.X + playMenuLevelNameStrings[i].Dimensions.X, 
					PLAY_MENU_LEVEL_BAR_Y_PER_BAR * i + PLAY_MENU_LEVEL_BAR_STARTING_Y + PLAY_MENU_LEVEL_BAR_NAME_STRING_OFFSET.Y
				);

				float curOffset = DEFAULT_OFFSET;
				LevelID lid = new LevelID((byte) playMenuSelectedWorldIndex, (byte) i);

				var acquiredStar = PersistedWorldData.GetStarForLevel(lid);
				if (acquiredStar != Star.None) {
					ITexture2D tex2D;
					switch (acquiredStar) {
						case Star.Gold: tex2D = AssetLocator.GoldStar; break;
						case Star.Silver: tex2D = AssetLocator.SilverStar; break;
						default: tex2D = AssetLocator.BronzeStar; break;
					}
					var starTex = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
						Anchoring = ViewportAnchoring.TopRight,
						AnchorOffset = levelNameTextLeftEnd + new Vector2(curOffset, -0.005f),
						AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.None,
						Color = new Vector4(1f, 1f, 1f, 0f),
						Rotation = 0f,
						Scale = new Vector2(0.0225f, 0.0375f),
						Texture = tex2D,
						ZIndex = playMenuLevelBars[i].ZIndex + 3
					};
					playMenuLevelBarAchievementTextures.Add(starTex);
					curOffset += OFFSET_PER_ICON;
				}

				if (PersistedWorldData.GetTakenCoinIndices(lid).Count() == LevelDatabase.GetLevelTotalCoins(lid)) {
					var coinTex = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
						Anchoring = ViewportAnchoring.TopRight,
						AnchorOffset = levelNameTextLeftEnd + new Vector2(curOffset, -0.002f),
						AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.None,
						Color = new Vector4(1f, 1f, 1f, 0f),
						Rotation = 0f,
						Scale = new Vector2(0.02f, 0.032f),
						Texture = AssetLocator.CoinFrames[26],
						ZIndex = playMenuLevelBars[i].ZIndex + 3
					};
					playMenuLevelBarAchievementTextures.Add(coinTex);
					curOffset += OFFSET_PER_ICON;
				}

				if (PersistedWorldData.GoldenEggTakenForLevel(lid)) {
					var eggTex = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
						Anchoring = ViewportAnchoring.TopRight,
						AnchorOffset = levelNameTextLeftEnd + new Vector2(curOffset, 0f),
						AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.None,
						Color = new Vector4(1f, 1f, 1f, 0f),
						Rotation = 0f,
						Scale = new Vector2(0.0225f, 0.032f),
						Texture = AssetLocator.VultureEggInvert,
						ZIndex = playMenuLevelBars[i].ZIndex + 3
					};
					playMenuLevelBarAchievementTextures.Add(eggTex);
				}
			}

			PlayMenuSetSelectedEggMaterial(Config.EggMaterial);
		}

		private static void PlayMenuRevertToWorldSelect() {
			if (!inDeferenceMode) HUDSound.XAnnounceWorldSelect.Play();

			playMenuOnLevelSelect = false;
			AddTransitionTickEvent(PlayMenuProgressionTransitionOut);
			BeginTransition(PLAY_MENU_PROGRESSION_TRANSITION_TIME);
			PlayMenuSetSelectedWorldIndex(0);
			if (playMenuMPTitleString.Scale != PLAY_MENU_MP_TITLE_SCALE) {
				playMenuMPTitleString.Scale = PLAY_MENU_MP_TITLE_SCALE;
				playMenuMPTitleString.AnchorOffset -= new Vector2(0f, PLAY_MENU_MP_REDUCED_TITLE_Y_OFFSET);
			}
			playMenuLevelNumberExciter.TargetObj = null;
			playMenuLevelNumberExciter.ClearAll();
			playMenuLevelNameExciter.TargetObj = null;
			playMenuLevelNameExciter.ClearAll();
			playMenuLevelBarExciter.TargetObj = null;
			playMenuLevelBarExciter.ClearAll();

			playMenuWorldIconHighlightExciter.TargetObj = playMenuWorldIconHighlight;
		
			playMenuSelectedEggEntity.SetTranslation(AssetLocator.MainCamera.Position + Vector3.UP * 100f * PhysicsManager.ONE_METRE_SCALED);
		}

		private static void PlayMenuProgressionTransitionIn(float deltaTime, float fracComplete) {
			float firstHalfComplete = Math.Min(fracComplete * 2f, 1f);

			if (firstHalfComplete <= 0.5f) {
				menuIdentString.SetAlpha(1f - firstHalfComplete * 2f);
				menuIdentString.AnchorOffset = new Vector2(menuIdentString.AnchorOffset.X, EL_IDENT_STRING_TARGET_Y + IDENT_STRING_STARTING_Y_OFFSET * (firstHalfComplete * 2f));
				playMenuMPCompletionTitleString.SetAlpha((1f - firstHalfComplete) * PLAY_MENU_MP_COMP_STR_MAX_ALPHA);
			}
			else {
				menuIdentString.Text = "LEVEL   SELECT";
				menuIdentString.SetAlpha((firstHalfComplete - 0.5f) * 2f);
				menuIdentString.AnchorOffset = new Vector2(menuIdentString.AnchorOffset.X, EL_IDENT_STRING_TARGET_Y + IDENT_STRING_STARTING_Y_OFFSET * (1f - (firstHalfComplete - 0.5f) * 2f));
				playMenuMPCompletionTitleString.Text = "Level Completion";
				playMenuMPCompletionTitleString.SetAlpha((fracComplete - 0.5f) * 2f * PLAY_MENU_MP_COMP_STR_MAX_ALPHA);
			}

			float mainPanelX = PLAY_MENU_MAIN_PANEL_FIRST_TARGET_X + (PLAY_MENU_MAIN_PANEL_SECOND_TARGET_X - PLAY_MENU_MAIN_PANEL_FIRST_TARGET_X) * firstHalfComplete;
			float mainPanelPrevX = playMenuMainPanel.AnchorOffset.X;
			playMenuMainPanel.AnchorOffset = new Vector2(mainPanelX, playMenuMainPanel.AnchorOffset.Y);

			for (int i = 0; i < playMenuWorldSelectIcons.Length; ++i) {
				float xPos;
				if (i < 9) {
					switch (i % 3) {
						case 0:
							xPos = PLAY_MENU_WORLD_ICON_COLUMN_A_X;
							break;
						case 1:
							xPos = PLAY_MENU_WORLD_ICON_COLUMN_B_X;
							break;
						default:
							xPos = PLAY_MENU_WORLD_ICON_COLUMN_C_X;
							break;
					}
				}
				else if (i == 9) xPos = PLAY_MENU_WORLD_ICON_COLUMN_A_X + (PLAY_MENU_WORLD_ICON_COLUMN_B_X - PLAY_MENU_WORLD_ICON_COLUMN_A_X) * 0.5f;
				else xPos = PLAY_MENU_WORLD_ICON_COLUMN_B_X + (PLAY_MENU_WORLD_ICON_COLUMN_C_X - PLAY_MENU_WORLD_ICON_COLUMN_B_X) * 0.5f;
				playMenuWorldSelectIcons[i].AnchorOffset = new Vector2(
					xPos + PLAY_MENU_WORLD_ICON_WORLD_SELECT_X_OFFSET * firstHalfComplete,
					playMenuWorldSelectIcons[i].AnchorOffset.Y
				);
				playMenuWorldSelectIcons[i].SetAlpha(1f - firstHalfComplete);
			}

			const float LEVEL_BAR_X_DIFF = PLAY_MENU_LEVEL_BAR_TARGET_X - PLAY_MENU_LEVEL_BAR_STARTING_X;
			for (int i = 0; i < 10; ++i) {
				playMenuLevelBars[i].AnchorOffset = new Vector2(PLAY_MENU_LEVEL_BAR_STARTING_X + LEVEL_BAR_X_DIFF * firstHalfComplete, playMenuLevelBars[i].AnchorOffset.Y);
			}
			playMenuLevelBarHighlight.SetAlpha(fracComplete);
			playMenuLevelBarHighlight.AnchorOffset = playMenuLevelBars[0].AnchorOffset;

			foreach (HUDTexture tex in playMenuLevelBarAchievementTextures) {
				tex.SetAlpha(Math.Max(fracComplete - 0.8f, 0f) * 5f * PLAY_MENU_LEVEL_BAR_ACHEIVEMENT_MAX_ALPHA);
			}

			if (firstHalfComplete >= 1f) {
				float secondHalfComplete = (fracComplete - 0.5f) * 2f;
				for (int i = 0; i < 10; ++i) {
					playMenuLevelBars[i].SetAlpha(secondHalfComplete);
					float targetDelta = PLAY_MENU_LEVEL_BAR_Y_PER_BAR * i;
					playMenuLevelBars[i].AnchorOffset = new Vector2(playMenuLevelBars[i].AnchorOffset.X, PLAY_MENU_LEVEL_BAR_STARTING_Y + targetDelta * secondHalfComplete);
					playMenuLevelNumberStrings[i].AnchorOffset = playMenuLevelBars[i].AnchorOffset + PLAY_MENU_LEVEL_BAR_NUMBER_STRING_OFFSET;
					playMenuLevelNumberStrings[i].SetAlpha(secondHalfComplete * PLAY_MENU_LEVEL_BAR_STRING_MAX_ALPHA);
					playMenuLevelNameStrings[i].AnchorOffset = new Vector2(PLAY_MENU_LEVEL_BAR_NAME_STRING_OFFSET.X, playMenuLevelBars[i].AnchorOffset.Y + PLAY_MENU_LEVEL_BAR_NAME_STRING_OFFSET.Y);
					playMenuLevelNameStrings[i].SetAlpha(secondHalfComplete * PLAY_MENU_LEVEL_BAR_STRING_MAX_ALPHA);
					playMenuLevelDifficultyIcons[i].AnchorOffset = new Vector2(PLAY_MENU_LEVEL_BAR_NAME_STRING_OFFSET.X - PLAY_MENU_LEVEL_DIFF_NAME_BUFFER, playMenuLevelBars[i].AnchorOffset.Y + PLAY_MENU_LEVEL_BAR_NAME_STRING_OFFSET.Y) + PLAY_MENU_LEVEL_DIFF_ICON_OFFSET_ADDITIONAL;
					playMenuLevelDifficultyIcons[i].SetAlpha(secondHalfComplete * PLAY_MENU_LEVEL_DIFF_MAX_ALPHA);
				}
			}

			playMenuWorldIconHighlight.SetAlpha(playMenuWorldSelectIcons[playMenuSelectedWorldIndex].Color.W);
			playMenuWorldIconHighlight.AnchorOffset = playMenuWorldSelectIcons[playMenuSelectedWorldIndex].AnchorOffset;

			playMenuMPWorldIcon.AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_WORLD_ICON_OFFSET + PLAY_MENU_WORLD_ICON_OFFSETS[playMenuSelectedWorldIndex];
			float mainPanelDelta = mainPanelX - mainPanelPrevX;
			playMenuMPTitleString.AnchorOffset = new Vector2(playMenuMPTitleString.AnchorOffset.X + mainPanelDelta, playMenuMPTitleString.AnchorOffset.Y);
			playMenuMPCompletionTitleString.AnchorOffset = new Vector2(playMenuMPCompletionTitleString.AnchorOffset.X + mainPanelDelta, playMenuMPCompletionTitleString.AnchorOffset.Y);
			playMenuPreviewBracketLeft.AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_BRACKET_LEFT_OFFSET;
			playMenuPreviewBracketRight.AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_BRACKET_RIGHT_OFFSET;
			playMenuPreviewFront.AnchorOffset = playMenuPreviewBack.AnchorOffset = playMenuPreviewIndent.AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_PREVIEW_OFFSET;

			playMenuMPWorldCompGoldStar.AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_WC_GOLD_STAR_OFFSET;
			playMenuMPWorldCompSilverStar.AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_WC_SILVER_STAR_OFFSET;
			playMenuMPWorldCompBronzeStar.AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_WC_BRONZE_STAR_OFFSET;
			playMenuMPWorldCompCoin.AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_WC_COIN_OFFSET;
			playMenuMPWorldCompEgg.AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_WC_EGG_OFFSET;

			playMenuMPWorldCompGoldStarString.AnchorOffset = new Vector2(playMenuMPWorldCompGoldStarString.AnchorOffset.X + mainPanelDelta, playMenuMPWorldCompGoldStarString.AnchorOffset.Y);
			playMenuMPWorldCompSilverStarString.AnchorOffset = new Vector2(playMenuMPWorldCompSilverStarString.AnchorOffset.X + mainPanelDelta, playMenuMPWorldCompSilverStarString.AnchorOffset.Y);
			playMenuMPWorldCompBronzeStarString.AnchorOffset = new Vector2(playMenuMPWorldCompBronzeStarString.AnchorOffset.X + mainPanelDelta, playMenuMPWorldCompBronzeStarString.AnchorOffset.Y);
			playMenuMPWorldCompCoinString.AnchorOffset = new Vector2(playMenuMPWorldCompCoinString.AnchorOffset.X + mainPanelDelta, playMenuMPWorldCompCoinString.AnchorOffset.Y);
			playMenuMPWorldCompEggString.AnchorOffset = new Vector2(playMenuMPWorldCompEggString.AnchorOffset.X + mainPanelDelta, playMenuMPWorldCompEggString.AnchorOffset.Y);

			playMenuMPWorldCompGoldStar.SetAlpha(1f - fracComplete);
			playMenuMPWorldCompSilverStar.SetAlpha(1f - fracComplete);
			playMenuMPWorldCompBronzeStar.SetAlpha(1f - fracComplete);
			playMenuMPWorldCompCoin.SetAlpha(1f - fracComplete);
			playMenuMPWorldCompEgg.SetAlpha(1f - fracComplete);
			playMenuMPWorldCompGoldStarString.SetAlpha((1f - fracComplete) * PLAY_MENU_MP_WC_STRING_MAX_ALPHA);
			playMenuMPWorldCompSilverStarString.SetAlpha((1f - fracComplete) * PLAY_MENU_MP_WC_STRING_MAX_ALPHA);
			playMenuMPWorldCompBronzeStarString.SetAlpha((1f - fracComplete) * PLAY_MENU_MP_WC_STRING_MAX_ALPHA);
			playMenuMPWorldCompCoinString.SetAlpha((1f - fracComplete) * PLAY_MENU_MP_WC_STRING_MAX_ALPHA);
			playMenuMPWorldCompEggString.SetAlpha((1f - fracComplete) * PLAY_MENU_MP_WC_STRING_MAX_ALPHA);

			playMenuMPLevelCompGoldStarIndent.AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_LC_GOLD_STAR_OFFSET;
			playMenuMPLevelCompSilverStarIndent.AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_LC_SILVER_STAR_OFFSET;
			playMenuMPLevelCompBronzeStarIndent.AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_LC_BRONZE_STAR_OFFSET;
			playMenuMPLevelCompEggIndent.AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_LC_EGG_OFFSET;
			playMenuMPLevelCompCoinIndent.AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_LC_COIN_INDENT_OFFSET;
			playMenuMPLevelCompGoldStar.AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_LC_BRONZE_STAR_OFFSET;
			playMenuMPLevelCompSilverStar.AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_LC_SILVER_STAR_OFFSET;
			playMenuMPLevelCompBronzeStar.AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_LC_GOLD_STAR_OFFSET;
			playMenuMPLevelCompEgg.AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_LC_EGG_OFFSET;

			playMenuMPLevelCompGoldStarIndent.SetAlpha(fracComplete);
			playMenuMPLevelCompSilverStarIndent.SetAlpha(fracComplete);
			playMenuMPLevelCompBronzeStarIndent.SetAlpha(fracComplete);
			playMenuMPLevelCompEggIndent.SetAlpha(fracComplete);
			playMenuMPLevelCompCoinIndent.SetAlpha(fracComplete);
			playMenuMPLevelCompTimesTitle.SetAlpha(Math.Max(fracComplete - 0.8f, 0f) * 5f * PLAY_MENU_MP_COMP_STR_MAX_ALPHA);

			playMenuMPLevelCompTimesGoldString.SetAlpha(fracComplete * PLAY_MENU_MP_COMP_STR_MAX_ALPHA);
			playMenuMPLevelCompTimesSilverString.SetAlpha(fracComplete * PLAY_MENU_MP_COMP_STR_MAX_ALPHA);
			playMenuMPLevelCompTimesBronzeString.SetAlpha(fracComplete * PLAY_MENU_MP_COMP_STR_MAX_ALPHA);
			playMenuMPLevelCompTimesPersonalString.SetAlpha(fracComplete * PLAY_MENU_MP_COMP_STR_MAX_ALPHA);

			playMenuMPLevelCompTimesGoldStar.SetAlpha(fracComplete * PLAY_MENU_MP_WC_ICON_ALPHA);
			playMenuMPLevelCompTimesSilverStar.SetAlpha(fracComplete * PLAY_MENU_MP_WC_ICON_ALPHA);
			playMenuMPLevelCompTimesBronzeStar.SetAlpha(fracComplete * PLAY_MENU_MP_WC_ICON_ALPHA);
			playMenuMPLevelCompTimesPersonalBell.SetAlpha(fracComplete * PLAY_MENU_MP_WC_ICON_ALPHA);


			playMenuSecondaryPanel.AnchorOffset = playMenuMainPanel.AnchorOffset + new Vector2(0f, PLAY_MENU_SECONDARY_PANEL_VERT_OFFSET);
			playMenuCareerStatsTitleString.AnchorOffset = playMenuSecondaryPanel.AnchorOffset + PLAY_MENU_CAREER_TITLE_OFFSET;
			playMenuCareerGoldStar.AnchorOffset = playMenuSecondaryPanel.AnchorOffset + PLAY_MENU_CAREER_GOLD_STAR_OFFSET;
			playMenuCareerSilverStar.AnchorOffset = playMenuSecondaryPanel.AnchorOffset + PLAY_MENU_CAREER_SILVER_STAR_OFFSET;
			playMenuCareerBronzeStar.AnchorOffset = playMenuSecondaryPanel.AnchorOffset + PLAY_MENU_CAREER_BRONZE_STAR_OFFSET;
			playMenuCareerCoin.AnchorOffset = playMenuSecondaryPanel.AnchorOffset + PLAY_MENU_CAREER_COIN_OFFSET;
			playMenuCareerEgg.AnchorOffset = playMenuSecondaryPanel.AnchorOffset + PLAY_MENU_CAREER_EGG_OFFSET;
			playMenuCareerGoldStarXString.AnchorOffset = playMenuCareerGoldStar.AnchorOffset + PLAY_MENU_CAREER_STAR_STRING_X_OFFSET;
			playMenuCareerGoldStarString.AnchorOffset = playMenuCareerGoldStar.AnchorOffset + PLAY_MENU_CAREER_STAR_STRING_OFFSET;
			playMenuCareerSilverStarXString.AnchorOffset = playMenuCareerSilverStar.AnchorOffset + PLAY_MENU_CAREER_STAR_STRING_X_OFFSET;
			playMenuCareerSilverStarString.AnchorOffset = playMenuCareerSilverStar.AnchorOffset + PLAY_MENU_CAREER_STAR_STRING_OFFSET;
			playMenuCareerBronzeStarXString.AnchorOffset = playMenuCareerBronzeStar.AnchorOffset + PLAY_MENU_CAREER_STAR_STRING_X_OFFSET;
			playMenuCareerBronzeStarString.AnchorOffset = playMenuCareerBronzeStar.AnchorOffset + PLAY_MENU_CAREER_STAR_STRING_OFFSET;
			playMenuCareerCoinString.AnchorOffset = playMenuCareerCoin.AnchorOffset + PLAY_MENU_CAREER_COIN_STRING_OFFSET;
			playMenuCareerEggString.AnchorOffset = playMenuCareerEgg.AnchorOffset + PLAY_MENU_CAREER_EGG_STRING_OFFSET;

			playMenuEggSelectTitleString.SetAlpha(Math.Max(fracComplete - 0.8f, 0f) * 5f);
			playMenuEggSelectMaterialNameString.SetAlpha(Math.Max(fracComplete - 0.8f, 0f) * 5f);
			if (LizardEggMaterialExtensions.MaterialsOrderedByUnlockCost.IndexOf(playMenuSelectedEggMaterial) > 0) playMenuEggSelectLeftChevron.SetAlpha(Math.Max(fracComplete - 0.8f, 0f) * 5f);
			else playMenuEggSelectLeftChevron.SetAlpha(0f);
			if (LizardEggMaterialExtensions.MaterialsOrderedByUnlockCost.IndexOf(playMenuSelectedEggMaterial) < LizardEggMaterialExtensions.MaterialsOrderedByUnlockCost.Count - 1) playMenuEggSelectRightChevron.SetAlpha(Math.Max(fracComplete - 0.8f, 0f) * 5f);
			else playMenuEggSelectRightChevron.SetAlpha(0f);
			if (playMenuSelectedEggMaterial.GoldenEggCost() > PersistedWorldData.GetTotalGoldenEggs()) {
				playMenuEggSelectLockedGoldenEggTex.SetAlpha(Math.Max(fracComplete - 0.8f, 0f) * 5f);
				playMenuEggSelectLockedGoldenEggString.SetAlpha(Math.Max(fracComplete - 0.8f, 0f) * 5f);
			}
			else {
				playMenuEggSelectLockedGoldenEggTex.SetAlpha(0f);
				playMenuEggSelectLockedGoldenEggString.SetAlpha(0f);
			}

			var numCoins = LevelDatabase.GetLevelTotalCoins(new LevelID((byte) playMenuSelectedWorldIndex, 0));
			float diffPerCoin = PLAY_MENU_MP_LC_COIN_OFFSET_DIFF / (numCoins > 1 ? (numCoins - 1) : 1f);
			for (int i = 0; i < playMenuMPLevelCompCoins.Count; ++i) {
				playMenuMPLevelCompCoins[i].AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_LC_COIN_OFFSET_MIN + new Vector2(diffPerCoin * i + 0.0015f, 0.0015f);
			}
		}

		private static void PlayMenuProgressionTransitionOut(float deltaTime, float fracComplete) {
			if (fracComplete <= 0.5f) {
				menuIdentString.SetAlpha(1f - fracComplete * 2f);
				menuIdentString.AnchorOffset = new Vector2(menuIdentString.AnchorOffset.X, EL_IDENT_STRING_TARGET_Y + IDENT_STRING_STARTING_Y_OFFSET * (fracComplete * 2f));
				playMenuMPCompletionTitleString.SetAlpha((1f - fracComplete * 2f) * PLAY_MENU_MP_COMP_STR_MAX_ALPHA);
			}
			else {
				menuIdentString.Text = "WORLD   SELECT";
				menuIdentString.SetAlpha((fracComplete - 0.5f) * 2f);
				menuIdentString.AnchorOffset = new Vector2(menuIdentString.AnchorOffset.X, EL_IDENT_STRING_TARGET_Y + IDENT_STRING_STARTING_Y_OFFSET * (1f - (fracComplete - 0.5f) * 2f));
				playMenuMPCompletionTitleString.Text = "World Completion:";
				playMenuMPCompletionTitleString.SetAlpha((fracComplete - 0.5f) * 2f * PLAY_MENU_MP_COMP_STR_MAX_ALPHA);
			}

			float mainPanelX = PLAY_MENU_MAIN_PANEL_FIRST_TARGET_X + (PLAY_MENU_MAIN_PANEL_SECOND_TARGET_X - PLAY_MENU_MAIN_PANEL_FIRST_TARGET_X) * (1f - fracComplete);
			float mainPanelPrevX = playMenuMainPanel.AnchorOffset.X;
			playMenuMainPanel.AnchorOffset = new Vector2(mainPanelX, playMenuMainPanel.AnchorOffset.Y);

			for (int i = 0; i < playMenuWorldSelectIcons.Length; ++i) {
				float xPos;
				if (i < 9) {
					switch (i % 3) {
						case 0:
							xPos = PLAY_MENU_WORLD_ICON_COLUMN_A_X;
							break;
						case 1:
							xPos = PLAY_MENU_WORLD_ICON_COLUMN_B_X;
							break;
						default:
							xPos = PLAY_MENU_WORLD_ICON_COLUMN_C_X;
							break;
					}
				}
				else if (i == 9) xPos = PLAY_MENU_WORLD_ICON_COLUMN_A_X + (PLAY_MENU_WORLD_ICON_COLUMN_B_X - PLAY_MENU_WORLD_ICON_COLUMN_A_X) * 0.5f;
				else xPos = PLAY_MENU_WORLD_ICON_COLUMN_B_X + (PLAY_MENU_WORLD_ICON_COLUMN_C_X - PLAY_MENU_WORLD_ICON_COLUMN_B_X) * 0.5f;
				playMenuWorldSelectIcons[i].AnchorOffset = new Vector2(
					xPos + PLAY_MENU_WORLD_ICON_WORLD_SELECT_X_OFFSET * (1f - fracComplete),
					playMenuWorldSelectIcons[i].AnchorOffset.Y
				);
				playMenuWorldSelectIcons[i].SetAlpha(fracComplete);
			}

			const float LEVEL_BAR_X_DIFF = PLAY_MENU_LEVEL_BAR_TARGET_X - PLAY_MENU_LEVEL_BAR_STARTING_X;
			for (int i = 0; i < 10; ++i) {
				float y = fracComplete >= 1f ? PLAY_MENU_LEVEL_BAR_STARTING_Y : playMenuLevelBars[i].AnchorOffset.Y;
				playMenuLevelBars[i].AnchorOffset = new Vector2(PLAY_MENU_LEVEL_BAR_STARTING_X + LEVEL_BAR_X_DIFF * (1f - fracComplete), y);
				playMenuLevelBars[i].SetAlpha(1f - fracComplete);
				playMenuLevelNumberStrings[i].AnchorOffset = playMenuLevelBars[i].AnchorOffset + new Vector2(PLAY_MENU_LEVEL_BAR_NUMBER_STRING_OFFSET.X * 1f, PLAY_MENU_LEVEL_BAR_NUMBER_STRING_OFFSET.Y);
				playMenuLevelNumberStrings[i].SetAlpha((1f - fracComplete) * PLAY_MENU_LEVEL_BAR_STRING_MAX_ALPHA);
				playMenuLevelNameStrings[i].AnchorOffset = new Vector2(PLAY_MENU_LEVEL_BAR_NAME_STRING_OFFSET.X, playMenuLevelBars[i].AnchorOffset.Y + PLAY_MENU_LEVEL_BAR_NAME_STRING_OFFSET.Y);
				playMenuLevelNameStrings[i].SetAlpha((1f - fracComplete) * PLAY_MENU_LEVEL_BAR_STRING_MAX_ALPHA);
				playMenuLevelDifficultyIcons[i].AnchorOffset = new Vector2(PLAY_MENU_LEVEL_BAR_NAME_STRING_OFFSET.X - PLAY_MENU_LEVEL_DIFF_NAME_BUFFER, playMenuLevelBars[i].AnchorOffset.Y + PLAY_MENU_LEVEL_BAR_NAME_STRING_OFFSET.Y) + PLAY_MENU_LEVEL_DIFF_ICON_OFFSET_ADDITIONAL;
				playMenuLevelDifficultyIcons[i].SetAlpha((1f - fracComplete) * PLAY_MENU_LEVEL_DIFF_MAX_ALPHA);
			}
			playMenuLevelBarHighlight.SetAlpha(1f - fracComplete);
			foreach (HUDTexture tex in playMenuLevelBarAchievementTextures) {
				tex.SetAlpha((1f - Math.Min(fracComplete * 5f, 1f)) * PLAY_MENU_LEVEL_BAR_ACHEIVEMENT_MAX_ALPHA);
			}

			playMenuWorldIconHighlight.SetAlpha(playMenuWorldSelectIcons[playMenuSelectedWorldIndex].Color.W);
			playMenuWorldIconHighlight.AnchorOffset = playMenuWorldSelectIcons[playMenuSelectedWorldIndex].AnchorOffset;

			playMenuMPWorldIcon.AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_WORLD_ICON_OFFSET + PLAY_MENU_WORLD_ICON_OFFSETS[playMenuSelectedWorldIndex];
			float mainPanelDelta = mainPanelX - mainPanelPrevX;
			playMenuMPTitleString.AnchorOffset = new Vector2(playMenuMPTitleString.AnchorOffset.X + mainPanelDelta, playMenuMPTitleString.AnchorOffset.Y);
			playMenuMPCompletionTitleString.AnchorOffset = new Vector2(playMenuMPCompletionTitleString.AnchorOffset.X + mainPanelDelta, playMenuMPCompletionTitleString.AnchorOffset.Y);
			playMenuPreviewBracketLeft.AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_BRACKET_LEFT_OFFSET;
			playMenuPreviewBracketRight.AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_BRACKET_RIGHT_OFFSET;
			playMenuPreviewFront.AnchorOffset = playMenuPreviewBack.AnchorOffset = playMenuPreviewIndent.AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_PREVIEW_OFFSET;

			playMenuMPWorldCompGoldStar.AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_WC_GOLD_STAR_OFFSET;
			playMenuMPWorldCompSilverStar.AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_WC_SILVER_STAR_OFFSET;
			playMenuMPWorldCompBronzeStar.AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_WC_BRONZE_STAR_OFFSET;
			playMenuMPWorldCompCoin.AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_WC_COIN_OFFSET;
			playMenuMPWorldCompEgg.AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_WC_EGG_OFFSET;

			playMenuMPWorldCompGoldStarString.AnchorOffset = new Vector2(playMenuMPWorldCompGoldStarString.AnchorOffset.X + mainPanelDelta, playMenuMPWorldCompGoldStarString.AnchorOffset.Y);
			playMenuMPWorldCompSilverStarString.AnchorOffset = new Vector2(playMenuMPWorldCompSilverStarString.AnchorOffset.X + mainPanelDelta, playMenuMPWorldCompSilverStarString.AnchorOffset.Y);
			playMenuMPWorldCompBronzeStarString.AnchorOffset = new Vector2(playMenuMPWorldCompBronzeStarString.AnchorOffset.X + mainPanelDelta, playMenuMPWorldCompBronzeStarString.AnchorOffset.Y);
			playMenuMPWorldCompCoinString.AnchorOffset = new Vector2(playMenuMPWorldCompCoinString.AnchorOffset.X + mainPanelDelta, playMenuMPWorldCompCoinString.AnchorOffset.Y);
			playMenuMPWorldCompEggString.AnchorOffset = new Vector2(playMenuMPWorldCompEggString.AnchorOffset.X + mainPanelDelta, playMenuMPWorldCompEggString.AnchorOffset.Y);

			playMenuMPWorldCompGoldStar.SetAlpha(fracComplete * PLAY_MENU_MP_WC_ICON_ALPHA);
			playMenuMPWorldCompSilverStar.SetAlpha(fracComplete * PLAY_MENU_MP_WC_ICON_ALPHA);
			playMenuMPWorldCompBronzeStar.SetAlpha(fracComplete * PLAY_MENU_MP_WC_ICON_ALPHA);
			playMenuMPWorldCompCoin.SetAlpha(fracComplete * PLAY_MENU_MP_WC_ICON_ALPHA);
			playMenuMPWorldCompEgg.SetAlpha(fracComplete * PLAY_MENU_MP_WC_ICON_ALPHA);
			playMenuMPWorldCompGoldStarString.SetAlpha(fracComplete * PLAY_MENU_MP_WC_STRING_MAX_ALPHA);
			playMenuMPWorldCompSilverStarString.SetAlpha(fracComplete * PLAY_MENU_MP_WC_STRING_MAX_ALPHA);
			playMenuMPWorldCompBronzeStarString.SetAlpha(fracComplete * PLAY_MENU_MP_WC_STRING_MAX_ALPHA);
			playMenuMPWorldCompCoinString.SetAlpha(fracComplete * PLAY_MENU_MP_WC_STRING_MAX_ALPHA);
			playMenuMPWorldCompEggString.SetAlpha(fracComplete * PLAY_MENU_MP_WC_STRING_MAX_ALPHA);

			playMenuMPLevelCompGoldStarIndent.AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_LC_GOLD_STAR_OFFSET;
			playMenuMPLevelCompSilverStarIndent.AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_LC_SILVER_STAR_OFFSET;
			playMenuMPLevelCompBronzeStarIndent.AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_LC_BRONZE_STAR_OFFSET;
			playMenuMPLevelCompEggIndent.AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_LC_EGG_OFFSET;
			playMenuMPLevelCompCoinIndent.AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_LC_COIN_INDENT_OFFSET;
			playMenuMPLevelCompGoldStar.AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_LC_GOLD_STAR_OFFSET;
			playMenuMPLevelCompSilverStar.AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_LC_SILVER_STAR_OFFSET;
			playMenuMPLevelCompBronzeStar.AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_LC_BRONZE_STAR_OFFSET;
			playMenuMPLevelCompEgg.AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_LC_EGG_OFFSET;

			playMenuMPLevelCompGoldStarIndent.SetAlpha(1f - fracComplete);
			playMenuMPLevelCompSilverStarIndent.SetAlpha(1f - fracComplete);
			playMenuMPLevelCompBronzeStarIndent.SetAlpha(1f - fracComplete);
			playMenuMPLevelCompEggIndent.SetAlpha(1f - fracComplete);
			playMenuMPLevelCompCoinIndent.SetAlpha(1f - fracComplete);
			playMenuMPLevelCompGoldStar.SetAlpha(1f - fracComplete);
			playMenuMPLevelCompSilverStar.SetAlpha(1f - fracComplete);
			playMenuMPLevelCompBronzeStar.SetAlpha(1f - fracComplete);
			playMenuMPLevelCompEgg.SetAlpha(1f - fracComplete);
			for (int i = 0; i < playMenuMPLevelCompCoins.Count; ++i) {
				playMenuMPLevelCompCoins[i].SetAlpha(Math.Min(playMenuMPLevelCompCoins[i].Color.W, 1f - fracComplete));
			}

			playMenuMPLevelCompTimesTitle.SetAlpha(PLAY_MENU_MP_WC_ICON_ALPHA - (Math.Min(fracComplete, 0.2f) * 5f * PLAY_MENU_MP_WC_ICON_ALPHA));

			playMenuMPLevelCompTimesGoldString.SetAlpha((1f - (Math.Min(fracComplete, 0.2f) * 5f)) * PLAY_MENU_MP_COMP_STR_MAX_ALPHA);
			playMenuMPLevelCompTimesSilverString.SetAlpha((1f - (Math.Min(fracComplete, 0.2f) * 5f)) * PLAY_MENU_MP_COMP_STR_MAX_ALPHA);
			playMenuMPLevelCompTimesBronzeString.SetAlpha((1f - (Math.Min(fracComplete, 0.2f) * 5f)) * PLAY_MENU_MP_COMP_STR_MAX_ALPHA);
			playMenuMPLevelCompTimesPersonalString.SetAlpha((1f - (Math.Min(fracComplete, 0.2f) * 5f)) * PLAY_MENU_MP_COMP_STR_MAX_ALPHA);

			playMenuMPLevelCompTimesGoldStar.SetAlpha((1f - (Math.Min(fracComplete, 0.2f) * 5f)) * PLAY_MENU_MP_WC_ICON_ALPHA);
			playMenuMPLevelCompTimesSilverStar.SetAlpha((1f - (Math.Min(fracComplete, 0.2f) * 5f)) * PLAY_MENU_MP_WC_ICON_ALPHA);
			playMenuMPLevelCompTimesBronzeStar.SetAlpha((1f - (Math.Min(fracComplete, 0.2f) * 5f)) * PLAY_MENU_MP_WC_ICON_ALPHA);
			playMenuMPLevelCompTimesPersonalBell.SetAlpha((1f - (Math.Min(fracComplete, 0.2f) * 5f)) * PLAY_MENU_MP_WC_ICON_ALPHA);

			playMenuSecondaryPanel.AnchorOffset = playMenuMainPanel.AnchorOffset + new Vector2(0f, PLAY_MENU_SECONDARY_PANEL_VERT_OFFSET);
			playMenuCareerStatsTitleString.AnchorOffset = playMenuSecondaryPanel.AnchorOffset + PLAY_MENU_CAREER_TITLE_OFFSET;
			playMenuCareerGoldStar.AnchorOffset = playMenuSecondaryPanel.AnchorOffset + PLAY_MENU_CAREER_GOLD_STAR_OFFSET;
			playMenuCareerSilverStar.AnchorOffset = playMenuSecondaryPanel.AnchorOffset + PLAY_MENU_CAREER_SILVER_STAR_OFFSET;
			playMenuCareerBronzeStar.AnchorOffset = playMenuSecondaryPanel.AnchorOffset + PLAY_MENU_CAREER_BRONZE_STAR_OFFSET;
			playMenuCareerCoin.AnchorOffset = playMenuSecondaryPanel.AnchorOffset + PLAY_MENU_CAREER_COIN_OFFSET;
			playMenuCareerEgg.AnchorOffset = playMenuSecondaryPanel.AnchorOffset + PLAY_MENU_CAREER_EGG_OFFSET;
			playMenuCareerGoldStarXString.AnchorOffset = playMenuCareerGoldStar.AnchorOffset + PLAY_MENU_CAREER_STAR_STRING_X_OFFSET;
			playMenuCareerGoldStarString.AnchorOffset = playMenuCareerGoldStar.AnchorOffset + PLAY_MENU_CAREER_STAR_STRING_OFFSET;
			playMenuCareerSilverStarXString.AnchorOffset = playMenuCareerSilverStar.AnchorOffset + PLAY_MENU_CAREER_STAR_STRING_X_OFFSET;
			playMenuCareerSilverStarString.AnchorOffset = playMenuCareerSilverStar.AnchorOffset + PLAY_MENU_CAREER_STAR_STRING_OFFSET;
			playMenuCareerBronzeStarXString.AnchorOffset = playMenuCareerBronzeStar.AnchorOffset + PLAY_MENU_CAREER_STAR_STRING_X_OFFSET;
			playMenuCareerBronzeStarString.AnchorOffset = playMenuCareerBronzeStar.AnchorOffset + PLAY_MENU_CAREER_STAR_STRING_OFFSET;
			playMenuCareerCoinString.AnchorOffset = playMenuCareerCoin.AnchorOffset + PLAY_MENU_CAREER_COIN_STRING_OFFSET;
			playMenuCareerEggString.AnchorOffset = playMenuCareerEgg.AnchorOffset + PLAY_MENU_CAREER_EGG_STRING_OFFSET;

			playMenuEggSelectTitleString.SetAlpha(Math.Max((1f - fracComplete) - 0.8f, 0f) * 5f);
			playMenuEggSelectMaterialNameString.SetAlpha(Math.Max((1f - fracComplete) - 0.8f, 0f) * 5f);
			playMenuEggSelectLeftChevron.SetAlpha(Math.Min(playMenuEggSelectLeftChevron.Color.W, Math.Max((1f - fracComplete) - 0.8f, 0f) * 5f));
			playMenuEggSelectRightChevron.SetAlpha(Math.Min(playMenuEggSelectLeftChevron.Color.W, Math.Max((1f - fracComplete) - 0.8f, 0f) * 5f));
			playMenuEggSelectLockedGoldenEggTex.SetAlpha(Math.Min(playMenuEggSelectLeftChevron.Color.W, Math.Max((1f - fracComplete) - 0.8f, 0f) * 5f));
			playMenuEggSelectLockedGoldenEggString.SetAlpha(Math.Min(playMenuEggSelectLeftChevron.Color.W, Math.Max((1f - fracComplete) - 0.8f, 0f) * 5f));

			var numCoins = LevelDatabase.GetLevelTotalCoins(new LevelID((byte) playMenuSelectedWorldIndex, 0));
			float diffPerCoin = PLAY_MENU_MP_LC_COIN_OFFSET_DIFF / (numCoins > 1 ? (numCoins - 1) : 1f);
			for (int i = 0; i < playMenuMPLevelCompCoins.Count; ++i) {
				playMenuMPLevelCompCoins[i].AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_LC_COIN_OFFSET_MIN + new Vector2(diffPerCoin * i + 0.0015f, 0.0015f);
			}
		}

		private static void PlayMenuChangeOptionUp() {
			if (playMenuOnLevelSelect) {
				PlayMenuSetSelectedLevelIndex(playMenuSelectedLevelIndex <= 0 ? 9 : playMenuSelectedLevelIndex - 1);
			}
			else {
				if (playMenuSelectedWorldIndex == 9) PlayMenuSetSelectedWorldIndex(6);
				else if (playMenuSelectedWorldIndex == 10) PlayMenuSetSelectedWorldIndex(8);
				else if (playMenuSelectedWorldIndex > 2) PlayMenuSetSelectedWorldIndex(playMenuSelectedWorldIndex - 3);
			}
		}
		private static void PlayMenuChangeOptionDown() {
			if (playMenuOnLevelSelect) {
				PlayMenuSetSelectedLevelIndex((playMenuSelectedLevelIndex + 1) % 10);
			}
			else {
				if (playMenuSelectedWorldIndex < 6) PlayMenuSetSelectedWorldIndex(playMenuSelectedWorldIndex + 3);
				else if (playMenuSelectedWorldIndex == 8) PlayMenuSetSelectedWorldIndex(10);
				else if (playMenuSelectedWorldIndex < 8) PlayMenuSetSelectedWorldIndex(9);
			}
		}
		private static void PlayMenuChangeOptionLeft() {
			if (playMenuOnLevelSelect) {
				var curIndex = LizardEggMaterialExtensions.MaterialsOrderedByUnlockCost.IndexOf(playMenuSelectedEggMaterial);
				if (curIndex == 0) return;
				var newMat = LizardEggMaterialExtensions.MaterialsOrderedByUnlockCost[curIndex - 1];
				HUDSound.PostPassEggPop.Play(pitch: 0.9f);
				PlayMenuSetSelectedEggMaterial(newMat);
			}
			else {
				if (playMenuSelectedWorldIndex > 0) PlayMenuSetSelectedWorldIndex(playMenuSelectedWorldIndex - 1);
			}
		}
		private static void PlayMenuChangeOptionRight() {
			if (playMenuOnLevelSelect) {
				var curIndex = LizardEggMaterialExtensions.MaterialsOrderedByUnlockCost.IndexOf(playMenuSelectedEggMaterial);
				if (curIndex == LizardEggMaterialExtensions.MaterialsOrderedByUnlockCost.Count - 1) return;
				var newMat = LizardEggMaterialExtensions.MaterialsOrderedByUnlockCost[curIndex + 1];
				HUDSound.PostPassEggPop.Play(pitch: 1.1f);
				PlayMenuSetSelectedEggMaterial(newMat);
			}
			else {
				if (playMenuSelectedWorldIndex < 10) PlayMenuSetSelectedWorldIndex(playMenuSelectedWorldIndex + 1);
			}
		}
		private static void PlayMenuConfirmOption() {
			if (playMenuOnLevelSelect) {
				HUDSound.UISelectOption.Play();
				var oldMaterial = Config.EggMaterial;
				Config.EggMaterial = playMenuSelectedEggMaterial;
				if (oldMaterial != playMenuSelectedEggMaterial) Config.Save();
				startGateCounter = 0;
				Thread.MemoryBarrier();
				BGMManager.CrossFadeToWorldMusic((byte) playMenuSelectedWorldIndex);
				PlayMenuTransitionOut(true);
				BGMManager.CrossfadeComplete += PlayMenuStartLevelGate;
				currentTransitionComplete += PlayMenuStartLevelGate;
				longLoadBackdrop = AssetLocator.LoadTexture(Path.Combine(AssetLocator.MaterialsDir, @"Previews\") + "load" + playMenuSelectedWorldIndex + ".png");
				GameCoordinator.EnableLongLoadingScreen(longLoadBackdrop, PlayMenuStartLevelGate);
			}
			else {
				if (playMenuCurWorldIsUnlocked) {
					PlayMenuProgressToLevelSelect();
					HUDSound.UISelectOption.Play();
				}
				else {
					playMenuLockedTitleExciter.TargetObj = playMenuLockedTitleString;
					playMenuLockedTitleExciteTimeRemaining = PLAY_MENU_LOCKED_EXCITE_TIME;
					HUDSound.UIUnavailable.Play();
				}
			}
		}
		private static void PlayMenuBackOut() {
			HUDSound.UISelectNegativeOption.Play();
			if (playMenuOnLevelSelect) {
				PlayMenuRevertToWorldSelect();
			}
			else {
				PlayMenuTransitionOut();
				if (!inDeferenceMode) currentTransitionComplete += () => MainMenuTransitionIn();
			}
		}

		private static void PlayMenuSetSelectedWorldIndex(int newIndex) {
			if (playMenuSelectedWorldIndex != newIndex) HUDSound.UINextOption.Play();
			playMenuSelectedWorldIndex = newIndex;
			playMenuCurWorldIsUnlocked = PersistedWorldData.GetFurthestUnlockedWorldIndex() >= newIndex;
			playMenuWorldIconHighlight.AnchorOffset = playMenuWorldSelectIcons[newIndex].AnchorOffset;
			if (playMenuCurWorldIsUnlocked) playMenuWorldIconHighlight.Texture = AssetLocator.PlayMenuWorldButtonUnlockedHighlight;
			else playMenuWorldIconHighlight.Texture = AssetLocator.PlayMenuWorldButtonHighlight;

			if (playMenuCurWorldIsUnlocked) {
				MenuPreviewManager.LoadWorld((byte) newIndex);
				playMenuPreviewBack.Texture = MenuPreviewManager.GetNextLoadedTexture();
				playMenuPreviewFront.Texture = MenuPreviewManager.GetNextLoadedTexture();
				playMenuPreviewBack.SetAlpha(1f);
			}
			playMenuPreviewFront.SetAlpha(0f);
			playMenuWorldPreviewTime = 0f;

			playMenuMPTitleString.Text = playMenuCurWorldIsUnlocked ? LevelDatabase.GetWorldName((byte) newIndex) : "???????????";
			playMenuMPTitleString.SetAlpha(0f);
			if (playMenuMPTitleString.Text.Length >= PLAY_MENU_REDUCED_TITLE_SCALE_STR_LEN) {
				if (playMenuMPTitleString.Scale != PLAY_MENU_MP_TITLE_SCALE_REDUCED) {
					playMenuMPTitleString.Scale = PLAY_MENU_MP_TITLE_SCALE_REDUCED;
					playMenuMPTitleString.AnchorOffset += new Vector2(0f, PLAY_MENU_MP_REDUCED_TITLE_Y_OFFSET);
				}
			}
			else if (playMenuMPTitleString.Scale != PLAY_MENU_MP_TITLE_SCALE) {
				playMenuMPTitleString.Scale = PLAY_MENU_MP_TITLE_SCALE;
				playMenuMPTitleString.AnchorOffset -= new Vector2(0f, PLAY_MENU_MP_REDUCED_TITLE_Y_OFFSET);
			}
			playMenuMPTitleString.Text = playMenuMPTitleString.Text.Replace(" ", "    ");
			var mixedColor = LevelDatabase.GetWorldColor((byte) newIndex) * 0.35f + Vector3.ONE * 0.65f;
			playMenuMPTitleString.Color = new Vector4(mixedColor, w: 0f);
			if (!playMenuCurWorldIsUnlocked) playMenuMPTitleString.Color = Vector3.ONE * 0.8f;

			playMenuMPWorldIcon.Texture = playMenuCurWorldIsUnlocked ? AssetLocator.WorldIcons[newIndex] : AssetLocator.WorldIconsGreyed[newIndex];
			playMenuMPWorldIcon.SetAlpha(0f);
			playMenuMPWorldIcon.AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_WORLD_ICON_OFFSET + PLAY_MENU_WORLD_ICON_OFFSETS[newIndex];
			playMenuMPWorldIcon.Scale = PLAY_MENU_MP_WORLD_ICON_SCALE * PLAY_MENU_WORLD_ICON_SCALE_ADJUSTMENTS[newIndex];

			playMenuMPWorldCompGoldStarString.Text = (PersistedWorldData.GetAcquiredGoldStarsForWorld((byte) newIndex) * 10) + "%";
			playMenuMPWorldCompSilverStarString.Text = (PersistedWorldData.GetAcquiredSilverStarsForWorld((byte) newIndex) * 10) + "%";
			playMenuMPWorldCompBronzeStarString.Text = (PersistedWorldData.GetAcquiredBronzeStarsForWorld((byte) newIndex) * 10) + "%";
			playMenuMPWorldCompCoinString.Text = (int) (PersistedWorldData.GetAcquiredCoinsForWorld((byte) newIndex) / (float) LevelDatabase.GetWorldTotalCoins((byte) newIndex) * 100f) + "%";
			playMenuMPWorldCompEggString.Text = (PersistedWorldData.GetAcquiredEggsForWorld((byte) newIndex) * 10) + "%";

			playMenuLockedGoldStarString.SetAlpha(0f);
			playMenuLockedSilverStarString.SetAlpha(0f);
			playMenuLockedBronzeStarString.SetAlpha(0f);
			playMenuLockedCoinString.SetAlpha(0f);
			playMenuLockedGoldStarTex.SetAlpha(0f);
			playMenuLockedSilverStarTex.SetAlpha(0f);
			playMenuLockedBronzeStarTex.SetAlpha(0f);
			playMenuLockedCoinTex.SetAlpha(0f);
			playMenuLockedGoldStarTick.SetAlpha(0f);
			playMenuLockedSilverStarTick.SetAlpha(0f);
			playMenuLockedBronzeStarTick.SetAlpha(0f);
			playMenuLockedCoinTick.SetAlpha(0f);
			playMenuLockedTitleString.SetAlpha(0f);
			playMenuLockedCriteriaTitleString.SetAlpha(0f);

			if (playMenuIncreasingAlphaTextures.ContainsKey(playMenuLockedGoldStarString)) playMenuIncreasingAlphaTextures.Remove(playMenuLockedGoldStarString);
			if (playMenuIncreasingAlphaTextures.ContainsKey(playMenuLockedSilverStarString)) playMenuIncreasingAlphaTextures.Remove(playMenuLockedSilverStarString);
			if (playMenuIncreasingAlphaTextures.ContainsKey(playMenuLockedBronzeStarString)) playMenuIncreasingAlphaTextures.Remove(playMenuLockedBronzeStarString);
			if (playMenuIncreasingAlphaTextures.ContainsKey(playMenuLockedCoinString)) playMenuIncreasingAlphaTextures.Remove(playMenuLockedCoinString);
			if (playMenuIncreasingAlphaTextures.ContainsKey(playMenuLockedGoldStarTex)) playMenuIncreasingAlphaTextures.Remove(playMenuLockedGoldStarTex);
			if (playMenuIncreasingAlphaTextures.ContainsKey(playMenuLockedSilverStarTex)) playMenuIncreasingAlphaTextures.Remove(playMenuLockedSilverStarTex);
			if (playMenuIncreasingAlphaTextures.ContainsKey(playMenuLockedBronzeStarTex)) playMenuIncreasingAlphaTextures.Remove(playMenuLockedBronzeStarTex);
			if (playMenuIncreasingAlphaTextures.ContainsKey(playMenuLockedCoinTex)) playMenuIncreasingAlphaTextures.Remove(playMenuLockedCoinTex);
			if (playMenuIncreasingAlphaTextures.ContainsKey(playMenuLockedGoldStarTick)) playMenuIncreasingAlphaTextures.Remove(playMenuLockedGoldStarTick);
			if (playMenuIncreasingAlphaTextures.ContainsKey(playMenuLockedSilverStarTick)) playMenuIncreasingAlphaTextures.Remove(playMenuLockedSilverStarTick);
			if (playMenuIncreasingAlphaTextures.ContainsKey(playMenuLockedBronzeStarTick)) playMenuIncreasingAlphaTextures.Remove(playMenuLockedBronzeStarTick);
			if (playMenuIncreasingAlphaTextures.ContainsKey(playMenuLockedCoinTick)) playMenuIncreasingAlphaTextures.Remove(playMenuLockedCoinTick);
			if (playMenuIncreasingAlphaTextures.ContainsKey(playMenuLockedTitleString)) playMenuIncreasingAlphaTextures.Remove(playMenuLockedTitleString);
			if (playMenuIncreasingAlphaTextures.ContainsKey(playMenuLockedCriteriaTitleString)) playMenuIncreasingAlphaTextures.Remove(playMenuLockedCriteriaTitleString);

			if (playMenuCurWorldIsUnlocked) {
				AddIncreasingTexture(playMenuPreviewBack, 3f);
			}
			else {
				AddIncreasingTexture(playMenuLockedTitleString, 2f);
				AddIncreasingTexture(playMenuLockedCriteriaTitleString, 2f);

				var unlockCriteria = LevelDatabase.GetWorldUnlockCriteria((byte) newIndex);
				playMenuLockedCoinString.Text = unlockCriteria.CoinsRequired.ToString("0000") + "x";
				playMenuLockedGoldStarString.Text = unlockCriteria.GoldStarsRequired.ToString("00") + "x";
				playMenuLockedSilverStarString.Text = unlockCriteria.SilverStarsRequired.ToString("00") + "x";
				playMenuLockedBronzeStarString.Text = unlockCriteria.BronzeStarsRequired.ToString("00") + "x";
				if (unlockCriteria.GoldStarsRequired > 0) {
					playMenuLockedBronzeStarString.AnchorOffset = new Vector2(0.445f, playMenuLockedBronzeStarString.AnchorOffset.Y);
					playMenuLockedBronzeStarTex.AnchorOffset = new Vector2(playMenuLockedBronzeStarString.AnchorOffset.X + 0.052f, playMenuLockedBronzeStarTex.AnchorOffset.Y);
					playMenuLockedSilverStarString.AnchorOffset = new Vector2(0.555f, playMenuLockedSilverStarString.AnchorOffset.Y);
					playMenuLockedSilverStarTex.AnchorOffset = new Vector2(playMenuLockedSilverStarString.AnchorOffset.X + 0.052f, playMenuLockedSilverStarTex.AnchorOffset.Y);
					playMenuLockedGoldStarString.AnchorOffset = new Vector2(0.665f, playMenuLockedGoldStarString.AnchorOffset.Y);
					playMenuLockedGoldStarTex.AnchorOffset = new Vector2(playMenuLockedGoldStarString.AnchorOffset.X + 0.052f, playMenuLockedGoldStarTex.AnchorOffset.Y);
					playMenuLockedCoinString.AnchorOffset = new Vector2(0.775f, playMenuLockedCoinString.AnchorOffset.Y);
					playMenuLockedCoinTex.AnchorOffset = new Vector2(playMenuLockedCoinString.AnchorOffset.X + 0.0955f, playMenuLockedCoinTex.AnchorOffset.Y);

					AddIncreasingTexture(playMenuLockedCoinString, 1f);
					AddIncreasingTexture(playMenuLockedCoinTex, 1f);
					AddIncreasingTexture(playMenuLockedGoldStarString, 1f);
					AddIncreasingTexture(playMenuLockedGoldStarTex, 1f);
					AddIncreasingTexture(playMenuLockedSilverStarString, 1f);
					AddIncreasingTexture(playMenuLockedSilverStarTex, 1f);
					AddIncreasingTexture(playMenuLockedBronzeStarString, 1f);
					AddIncreasingTexture(playMenuLockedBronzeStarTex, 1f);
				}
				else if (unlockCriteria.SilverStarsRequired > 0) {
					playMenuLockedBronzeStarString.AnchorOffset = new Vector2(0.49f, playMenuLockedBronzeStarString.AnchorOffset.Y);
					playMenuLockedBronzeStarTex.AnchorOffset = new Vector2(playMenuLockedBronzeStarString.AnchorOffset.X + 0.052f, playMenuLockedBronzeStarTex.AnchorOffset.Y);
					playMenuLockedSilverStarString.AnchorOffset = new Vector2(0.63f, playMenuLockedSilverStarString.AnchorOffset.Y);
					playMenuLockedSilverStarTex.AnchorOffset = new Vector2(playMenuLockedSilverStarString.AnchorOffset.X + 0.052f, playMenuLockedSilverStarTex.AnchorOffset.Y);
					playMenuLockedCoinString.AnchorOffset = new Vector2(0.77f, playMenuLockedCoinString.AnchorOffset.Y);
					playMenuLockedCoinTex.AnchorOffset = new Vector2(playMenuLockedCoinString.AnchorOffset.X + 0.0955f, playMenuLockedCoinTex.AnchorOffset.Y);

					AddIncreasingTexture(playMenuLockedCoinString, 1f);
					AddIncreasingTexture(playMenuLockedCoinTex, 1f);
					AddIncreasingTexture(playMenuLockedSilverStarString, 1f);
					AddIncreasingTexture(playMenuLockedSilverStarTex, 1f);
					AddIncreasingTexture(playMenuLockedBronzeStarString, 1f);
					AddIncreasingTexture(playMenuLockedBronzeStarTex, 1f);
				}
				else {
					playMenuLockedBronzeStarString.AnchorOffset = new Vector2(0.56f, playMenuLockedBronzeStarString.AnchorOffset.Y);
					playMenuLockedBronzeStarTex.AnchorOffset = new Vector2(playMenuLockedBronzeStarString.AnchorOffset.X + 0.052f, playMenuLockedBronzeStarTex.AnchorOffset.Y);
					playMenuLockedCoinString.AnchorOffset = new Vector2(0.70f, playMenuLockedCoinString.AnchorOffset.Y);
					playMenuLockedCoinTex.AnchorOffset = new Vector2(playMenuLockedCoinString.AnchorOffset.X + 0.0955f, playMenuLockedCoinTex.AnchorOffset.Y);

					AddIncreasingTexture(playMenuLockedCoinString, 1f);
					AddIncreasingTexture(playMenuLockedCoinTex, 1f);
					AddIncreasingTexture(playMenuLockedBronzeStarString, 1f);
					AddIncreasingTexture(playMenuLockedBronzeStarTex, 1f);
				}

				if (PersistedWorldData.GetTotalGoldStars() >= unlockCriteria.GoldStarsRequired && unlockCriteria.GoldStarsRequired > 0) {
					playMenuLockedGoldStarTick.AnchorOffset = playMenuLockedGoldStarTex.AnchorOffset + PLAY_MENU_LOCKED_TEX_TICK_OFFSET;
					AddIncreasingTexture(playMenuLockedGoldStarTick, 1f);
				}
				if (PersistedWorldData.GetTotalSilverStars() >= unlockCriteria.SilverStarsRequired && unlockCriteria.SilverStarsRequired > 0) {
					playMenuLockedSilverStarTick.AnchorOffset = playMenuLockedSilverStarTex.AnchorOffset + PLAY_MENU_LOCKED_TEX_TICK_OFFSET;
					AddIncreasingTexture(playMenuLockedSilverStarTick, 1f);
				}
				if (PersistedWorldData.GetTotalBronzeStars() >= unlockCriteria.BronzeStarsRequired && unlockCriteria.BronzeStarsRequired > 0) {
					playMenuLockedBronzeStarTick.AnchorOffset = playMenuLockedBronzeStarTex.AnchorOffset + PLAY_MENU_LOCKED_TEX_TICK_OFFSET;
					AddIncreasingTexture(playMenuLockedBronzeStarTick, 1f);
				}
				if (PersistedWorldData.GetTotalTakenCoins() >= unlockCriteria.CoinsRequired && unlockCriteria.CoinsRequired > 0) {
					playMenuLockedCoinTick.AnchorOffset = playMenuLockedCoinTex.AnchorOffset + PLAY_MENU_LOCKED_TEX_TICK_OFFSET;
					AddIncreasingTexture(playMenuLockedCoinTick, 1f);
				}

				AddDecreasingTexture(playMenuPreviewBack, 3f);
			}
		}

		private static void PlayMenuSetSelectedLevelIndex(int newIndex) {
			if (playMenuSelectedLevelIndex != newIndex) HUDSound.UINextOption.Play();
			playMenuSelectedLevelIndex = newIndex;
			playMenuLevelNumberExciter.TargetObj = playMenuLevelNumberStrings[newIndex];
			playMenuLevelNameExciter.TargetObj = playMenuLevelNameStrings[newIndex];
			playMenuLevelBarHighlight.AnchorOffset = playMenuLevelBars[newIndex].AnchorOffset;
			playMenuTargetHeaderString = playMenuLevelNameStrings[newIndex].Text;

			MenuPreviewManager.LoadLevel(new LevelID((byte) playMenuSelectedWorldIndex, (byte) newIndex));
			if (playMenuLevelPreviewSwapCompletion < 1f) {
				playMenuPreviewBack.Texture = playMenuPreviewFront.Texture;
			}
			playMenuPreviewBack.SetAlpha(1f);
			playMenuPreviewFront.SetAlpha(0f);
			playMenuPreviewFront.Texture = MenuPreviewManager.GetNextLoadedTexture();
			playMenuLevelPreviewSwapCompletion = 0f;

			LevelID lid = new LevelID((byte) playMenuSelectedWorldIndex, (byte) newIndex);
			playMenuMPLevelCompTimesBronzeString.Text = LevelDatabase.GetLevelBronzeTimeAsString(lid);
			playMenuMPLevelCompTimesSilverString.Text = LevelDatabase.GetLevelSilverTimeAsString(lid);
			playMenuMPLevelCompTimesGoldString.Text = LevelDatabase.GetLevelGoldTimeAsString(lid);
			var bestTime = PersistedWorldData.GetBestTimeForLevel(lid);
			playMenuMPLevelCompTimesPersonalString.Text = bestTime.HasValue ? MakeTimeString(bestTime.Value) : "--:--";

			switch (PersistedWorldData.GetStarForLevel(lid)) {
				case Star.Gold:
					AddIncreasingTexture(playMenuMPLevelCompGoldStar, 4f);
					AddIncreasingTexture(playMenuMPLevelCompSilverStar, 4f);
					AddIncreasingTexture(playMenuMPLevelCompBronzeStar, 4f);
					break;
				case Star.Silver:
					AddIncreasingTexture(playMenuMPLevelCompSilverStar, 4f);
					AddIncreasingTexture(playMenuMPLevelCompBronzeStar, 4f);
					AddDecreasingTexture(playMenuMPLevelCompGoldStar, 4f);
					break;
				case Star.Bronze:
					AddIncreasingTexture(playMenuMPLevelCompBronzeStar, 4f);
					AddDecreasingTexture(playMenuMPLevelCompGoldStar, 4f);
					AddDecreasingTexture(playMenuMPLevelCompSilverStar, 4f);
					break;
				case Star.None:
					AddDecreasingTexture(playMenuMPLevelCompGoldStar, 4f);
					AddDecreasingTexture(playMenuMPLevelCompSilverStar, 4f);
					AddDecreasingTexture(playMenuMPLevelCompBronzeStar, 4f);
					break;
			}

			playMenuIncDecTexRemovalWorkspace.Clear();
			foreach (var kvp in playMenuIncreasingAlphaTextures) {
				if (!(kvp.Key is HUDTexture)) continue;
				if (((HUDTexture) kvp.Key).Texture == AssetLocator.CoinFrames[24]) playMenuIncDecTexRemovalWorkspace.Add(kvp.Key);
			}
			foreach (var toBeRemoved in playMenuIncDecTexRemovalWorkspace) {
				playMenuIncreasingAlphaTextures.Remove(toBeRemoved);
			}
			playMenuIncDecTexRemovalWorkspace.Clear();
			foreach (var kvp in playMenuDecreasingAlphaTextures) {
				if (!(kvp.Key is HUDTexture)) continue;
				if (((HUDTexture) kvp.Key).Texture == AssetLocator.CoinFrames[24]) playMenuIncDecTexRemovalWorkspace.Add(kvp.Key);
			}
			foreach (var toBeRemoved in playMenuIncDecTexRemovalWorkspace) {
				playMenuDecreasingAlphaTextures.Remove(toBeRemoved);
			}
			var numCoins = LevelDatabase.GetLevelTotalCoins(lid);
			var coinsAcquired = PersistedWorldData.GetTakenCoinIndices(lid).Count();
			var curCount = playMenuMPLevelCompCoins.Count;
			for (int i = 0; i < (numCoins - curCount); ++i) {
				var coin = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
					Anchoring = playMenuMainPanel.Anchoring,
					AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.None,
					Color = new Vector4(1f, 1f, 1f, 1f),
					Rotation = 0f,
					Scale = (PLAY_MENU_MP_WC_ICON_SCALE + new Vector2(0f, 0.027f)) * 0.77f,
					Texture = AssetLocator.CoinFrames[24]
				};
				playMenuMPLevelCompCoins.Add(coin);
			}
			float diffPerCoin = PLAY_MENU_MP_LC_COIN_OFFSET_DIFF / (numCoins > 1 ? (numCoins - 1) : 1f);
			for (int i = 0; i < playMenuMPLevelCompCoins.Count; ++i) {
				playMenuMPLevelCompCoins[i].AnchorOffset = playMenuMainPanel.AnchorOffset + PLAY_MENU_MP_LC_COIN_OFFSET_MIN + new Vector2(diffPerCoin * i + 0.0015f, 0.0015f);
				playMenuMPLevelCompCoins[i].ZIndex = playMenuMainPanel.ZIndex + 2 + i;
				if (i < coinsAcquired) {
					playMenuMPLevelCompCoins[i].Color = new Vector4(1f, 1f, 1f, 0f);
					AddIncreasingTexture(playMenuMPLevelCompCoins[i], 10f - 9f * (i / (float) coinsAcquired));
				}
				else {
					playMenuMPLevelCompCoins[i].Color = new Vector4(1f, 1f, 1f, 0f);
				}
			}

			if (PersistedWorldData.GoldenEggTakenForLevel(lid)) playMenuMPLevelCompEgg.SetAlpha(1f);
			else playMenuMPLevelCompEgg.SetAlpha(0f);
		}

		private static void PlayMenuSetSelectedEggMaterial(LizardEggMaterial mat) {
			if (!PersistedWorldData.EggMaterialIsUnlocked(mat)) {
				playMenuEggSelectMaterialNameString.Color = new Vector4(PLAY_MENU_EGG_SELECT_MAT_TITLE_COLOR_LOCKED, w: 0f);
				playMenuEggSelectMaterialNameString.Text = "Locked!";
				playMenuEggSelectLockedGoldenEggTex.SetAlpha(1f);
				playMenuEggSelectLockedGoldenEggString.SetAlpha(1f);
				playMenuEggSelectLockedGoldenEggString.Text = "x" + mat.GoldenEggCost().ToString("00");
				playMenuSelectedEggEntity.SetModelInstance(AssetLocator.GameLayer, mat.Model(), AssetLocator.GreyedOutMaterial);
			}
			else {
				playMenuEggSelectMaterialNameString.Color = new Vector4(PLAY_MENU_EGG_SELECT_MAT_TITLE_COLOR, w: 0f);
				playMenuEggSelectMaterialNameString.Text = mat.Title();
				playMenuEggSelectLockedGoldenEggTex.SetAlpha(0f);
				playMenuEggSelectLockedGoldenEggString.SetAlpha(0f);
				playMenuSelectedEggEntity.SetModelInstance(AssetLocator.GameLayer, mat.Model(), mat.Material());
			}

			if (LizardEggMaterialExtensions.MaterialsOrderedByUnlockCost.IndexOf(mat) == LizardEggMaterialExtensions.MaterialsOrderedByUnlockCost.Count - 1) playMenuEggSelectRightChevron.SetAlpha(0f);
			else playMenuEggSelectRightChevron.SetAlpha(1f);

			if (LizardEggMaterialExtensions.MaterialsOrderedByUnlockCost.IndexOf(mat) == 0) playMenuEggSelectLeftChevron.SetAlpha(0f);
			else playMenuEggSelectLeftChevron.SetAlpha(1f);

			playMenuSelectedEggMaterial = mat;
		}

		private static void AddIncreasingTexture(IHUDObject t, float modifier = 1f) {
			if (playMenuDecreasingAlphaTextures.ContainsKey(t)) playMenuDecreasingAlphaTextures.Remove(t);
			else if (!playMenuIncreasingAlphaTextures.ContainsKey(t)) playMenuIncreasingAlphaTextures.Add(t, modifier);
		}
		private static void AddDecreasingTexture(IHUDObject t, float modifier = 1f) {
			if (playMenuIncreasingAlphaTextures.ContainsKey(t)) playMenuIncreasingAlphaTextures.Remove(t);
			else if (!playMenuDecreasingAlphaTextures.ContainsKey(t)) playMenuDecreasingAlphaTextures.Add(t, modifier);
		}

		private static void PlayMenuStartLevelGate() {
			lock (staticMutationLock) {
				if (++startGateCounter == 3) {
					EntityModule.AddTimedAction(PlayMenuLoadLevel, 0.5f);
				}
			}
		}

		private static void PlayMenuLoadLevel() {
			lock (staticMutationLock) {
				UnloadMenuBackdrop();
				CurrentMenuState = MenuState.InGame;
				
				var selectedLevelID = new LevelID((byte) playMenuSelectedWorldIndex, (byte) playMenuSelectedLevelIndex);
				GameCoordinator.LoadLevel(selectedLevelID);
				EntityModule.AddTimedAction(PlayMenuStartLevel, 0.5f);
			}
		}

		private static void PlayMenuStartLevel() {
			lock (staticMutationLock) {
				GameCoordinator.DisableLongLoadingScreen(
					LevelDatabase.GetWorldColor((byte) playMenuSelectedWorldIndex) * MEDALS_MENU_WORLD_NAME_STRING_WORLD_COLOR_WEIGHT + Vector3.ONE * (1f - MEDALS_MENU_WORLD_NAME_STRING_WORLD_COLOR_WEIGHT), 
					() => {
						AssetLocator.UnloadTexture(Path.Combine(AssetLocator.MaterialsDir, @"Previews\") + "load" + playMenuSelectedWorldIndex + ".png");
						GameCoordinator.StartLevelIntroduction();
					}
				);
			}
		}

		private static string MakeTimeString(int numMillis) {
			int numFullSeconds = numMillis / 1000;
			int numHundredths = (numMillis - numFullSeconds * 1000) / 10;

			return numFullSeconds.ToString("00") + ":" + numHundredths.ToString("00");
		}
	}
}