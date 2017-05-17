// All code copyright (c) 2016 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 20 07 2016 at 15:14 by Ben Bowen

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FMOD;
using Ophidian.Losgap;
using Ophidian.Losgap.Entities;
using Ophidian.Losgap.Rendering;

namespace Egodystonic.EscapeLizards {
	public static partial class MenuCoordinator {
		private const float MEDALS_MENU_TRANSITION_IN_TIME = 0.3f;
		private const float MEDALS_MENU_TRANSITION_OUT_TIME = 0.2f;
		private static FontString medalsMenuWorldNameString;
		private static HUDTexture medalsMenuWorldIconLeft;
		private static HUDTexture medalsMenuWorldIconRight;
		private static HUDTexture medalsMenuChevronsLeft;
		private static HUDTexture medalsMenuChevronsRight;
		private const float MEDALS_MENU_WORLD_NAME_STRING_START_OFFSET = 0.1f;
		private const float MEDALS_MENU_WORLD_NAME_STRING_END_OFFSET = 0.18f;
		private const float MEDALS_MENU_WORLD_NAME_STRING_WORLD_COLOR_WEIGHT = 0.5f;
		private const float MEDALS_MENU_WORLD_ICON_VERT_OFFSET = 0.034f;
		private const float MEDALS_MENU_WORLD_ICON_HORIZ_BUFFER = 0.095f;
		private const float MEDALS_MENU_WORLD_ICON_HORIZ_START_OFFSET = -0.1f;
		private static readonly Vector2 MEDALS_MENU_CHEVRON_POSITION = new Vector2(0.035f, 0.15f);
		private static readonly HUDTexture[] medalsMenuLevelBackgrounds = new HUDTexture[10];
		private static readonly FontString[] medalsMenuLevelNameStrings = new FontString[10];
		private static readonly HUDTexture[] medalsMenuLevelBarMedals = new HUDTexture[10];
		private static readonly List<IHUDObject> medalsMenuOldObjects = new List<IHUDObject>();
		private static readonly Vector2 MEDALS_MENU_LEVEL_BAR_MEDAL_OFFSET = new Vector2(0.33f, -0.02f);
		private static readonly List<IHUDObject> medalsMenuOldObjectsRemovalWorkspace = new List<IHUDObject>();
		private static readonly List<HUDTexture> medalsMenuReloadIcons = new List<HUDTexture>();
		private static readonly List<HUDTexture> medalsMenuActiveReloadIcons = new List<HUDTexture>();
		private static readonly Vector2 MEDALS_MENU_RELOAD_ICON_OFFSET = new Vector2(0.345f, 0.0035f);
		private const float MEDALS_MENU_RELOAD_ICON_MAX_ALPHA = 0.85f;
		private const float MEDALS_MENU_RELOAD_ICON_ALPHA_FADE_RATE = 1.5f * MEDALS_MENU_RELOAD_ICON_MAX_ALPHA;
		private const float MEDALS_MENU_RELOAD_ICON_SPIN = MathUtils.PI;
		private static HUDTexture medalsMenuLevelBackgroundHighlight;
		private static HUDItemExciterEntity medalsMenuLevelBackgroundHighlightExciter;
		private static HUDItemExciterEntity medalsMenuLevelNameExciter;
		private static float medalsMenuHeaderTransitionState;
		private static int medalsMenuSelectedWorldIndex;
		private static int medalsMenuSelectedLevelIndex;
		private const float MEDALS_MENU_LEVEL_LIST_START_Y = 0.36f;
		private const float MEDALS_MENU_LEVEL_LIST_Y_PER_ROW = 0.06f;
		private const float MEDALS_MENU_LEVEL_LIST_TARGET_X = 0.0175f;
		private const float MEDALS_MENU_LEVEL_LIST_WORLD_CHANGE_X_OFFSET = 0.3f;
		private const float MEDALS_MENU_LEVEL_LIST_WORLD_CHANGE_PER_SEC = 0.9f;
		private const float MEDALS_MENU_HEADER_TRANSITION_PER_SEC = 4f;
		private static readonly Vector2 MEDALS_MENU_LEVEL_BAR_SCALE = new Vector2(0.35f, 0.6f * 0.085f);
		private static readonly Vector2 MEDALS_MENU_LEVEL_BAR_STRING_SCALE = Vector2.ONE * 0.32f;
		private static readonly Vector2 MEDALS_MENU_LEVEL_BAR_STRING_OFFSET = new Vector2(0.01f, 0.011f);
		private static readonly Vector3 MEDALS_MENU_LEVEL_BAR_STRING_COLOR = new Vector3(0.6f, 0.85f, 0.4f);
		private static HUDTexture medalsMenuBackdrop;
		private static HUDTexture medalsMenuLevelPreview;
		private static string medalsMenuCurPreviewFilepath;
		private const int MEDALS_MENU_MAX_SCORE_ENTRIES = 8;
		private static readonly List<KeyValuePair<LeaderboardPlayer, int>>[] medalsMenuCurrentWorldTimes = new List<KeyValuePair<LeaderboardPlayer, int>>[10];
		private static readonly FontString[] medalsMenuCurrentLevelTimes = new FontString[MEDALS_MENU_MAX_SCORE_ENTRIES];
		private static readonly FontString[] medalsMenuCurrentLevelNames = new FontString[MEDALS_MENU_MAX_SCORE_ENTRIES];
		private const float MEDALS_MENU_MIN_LEVEL_TIME_ALPHA_PER_SEC = 0.6f;
		private const float MEDALS_MENU_MAX_LEVEL_TIME_ALPHA_PER_SEC = 1.5f;
		private const int MEDALS_MENU_NAME_MAX_LEN = 20;
		private static HUDTexture medalsMenuTimesGoldMedal;
		private static HUDTexture medalsMenuTimesSilverMedal;
		private static HUDTexture medalsMenuTimesBronzeMedal;
		private static HUDTexture medalsMenuTimesPersonalBestBell;
		private const float MEDALS_MENU_TIMES_MEDALS_COLUMN_POS = 0.475f;
		private const float MEDALS_MENU_TIMES_MEDALS_COLUMN_Y_OFFSET = -0.0055f + 0.025f;
		private const float MEDALS_MENU_TIMES_BELL_COLUMN_POS = 0.44f;
		private const float MEDALS_MENU_TIMES_BELL_COLUMN_Y_OFFSET = -0.0055f + 0.025f;
		private const float MEDALS_MENU_MAX_CHEVRON_OFFSET = -0.05f;
		private const float MEDALS_MENU_CHEVRON_OFFSET_PER_SEC = MEDALS_MENU_MAX_CHEVRON_OFFSET;

		private static void MedalsMenuTransitionIn(bool createComponents = true) {
			if (!inDeferenceMode) HUDSound.XAnnounceMedals.Play();

			CurrentMenuState = MenuState.MedalsMenu;
			if (createComponents) MedalsMenuCreateComponents();
			AddTransitionTickEvent(MedalsMenuTickTransitionIn);
			BeginTransition(MEDALS_MENU_TRANSITION_IN_TIME);

			menuIdentString.Text = "MEDALS    TABLE";
			menuIdentTex.Texture = AssetLocator.MainMenuMedalsButton;
		}

		private static void MedalsMenuTransitionOut(bool disposeComponents = true) {
			AddTransitionTickEvent(MedalsMenuTickTransitionOut);
			BeginTransition(MEDALS_MENU_TRANSITION_OUT_TIME);
			if (disposeComponents) currentTransitionComplete += MedalsMenuDisposeComponents;
		}

		private static void MedalsMenuTick(float deltaTime) {
			foreach (var reloadIcon in medalsMenuReloadIcons) {
				if (medalsMenuActiveReloadIcons.Contains(reloadIcon) && reloadIcon.Color.W < MEDALS_MENU_RELOAD_ICON_MAX_ALPHA) {
					reloadIcon.AdjustAlpha(deltaTime * MEDALS_MENU_RELOAD_ICON_ALPHA_FADE_RATE);
					if (reloadIcon.Color.W > MEDALS_MENU_RELOAD_ICON_MAX_ALPHA) reloadIcon.SetAlpha(MEDALS_MENU_RELOAD_ICON_MAX_ALPHA);
				}
				else if (reloadIcon.Color.W > 0f) reloadIcon.AdjustAlpha(deltaTime * -MEDALS_MENU_RELOAD_ICON_ALPHA_FADE_RATE);

				reloadIcon.Rotation += MEDALS_MENU_RELOAD_ICON_SPIN * deltaTime;
			}

			for (int i = 0; i < medalsMenuLevelBackgrounds.Length; ++i) {
				if (medalsMenuLevelBackgrounds[i].AnchorOffset.X > MEDALS_MENU_LEVEL_LIST_TARGET_X) {
					medalsMenuLevelBackgrounds[i].AnchorOffset = new Vector2(
						medalsMenuLevelBackgrounds[i].AnchorOffset.X - MEDALS_MENU_LEVEL_LIST_WORLD_CHANGE_PER_SEC * deltaTime,
						medalsMenuLevelBackgrounds[i].AnchorOffset.Y
					);

					if (medalsMenuLevelBackgrounds[i].AnchorOffset.X < MEDALS_MENU_LEVEL_LIST_TARGET_X) {
						medalsMenuLevelBackgrounds[i].AnchorOffset = new Vector2(
							MEDALS_MENU_LEVEL_LIST_TARGET_X,
							medalsMenuLevelBackgrounds[i].AnchorOffset.Y
						);
						medalsMenuLevelBackgrounds[i].SetAlpha(1f);
					}
					else {
						float fracRemaining = (medalsMenuLevelBackgrounds[i].AnchorOffset.X - MEDALS_MENU_LEVEL_LIST_TARGET_X) / MEDALS_MENU_LEVEL_LIST_WORLD_CHANGE_X_OFFSET;
						medalsMenuLevelBackgrounds[i].SetAlpha(1f - fracRemaining);
					}

					medalsMenuLevelNameStrings[i].AnchorOffset = medalsMenuLevelBackgrounds[i].AnchorOffset + MEDALS_MENU_LEVEL_BAR_STRING_OFFSET;
					medalsMenuLevelNameStrings[i].SetAlpha(medalsMenuLevelBackgrounds[i].Color.W);
				}

				medalsMenuLevelBarMedals[i].AnchorOffset = medalsMenuLevelBackgrounds[i].AnchorOffset + MEDALS_MENU_LEVEL_BAR_MEDAL_OFFSET;
				if (medalsMenuLevelBarMedals[i].Texture != null) medalsMenuLevelBarMedals[i].SetAlpha(medalsMenuLevelBackgrounds[i].Color.W);
			}

			medalsMenuLevelBackgroundHighlight.AnchorOffset = medalsMenuLevelBackgrounds[medalsMenuSelectedLevelIndex].AnchorOffset;

			if (medalsMenuHeaderTransitionState < 1f) {
				bool wasNegative = medalsMenuHeaderTransitionState < 0f;
				medalsMenuHeaderTransitionState += MEDALS_MENU_HEADER_TRANSITION_PER_SEC * deltaTime;
				if (medalsMenuHeaderTransitionState > 1f) medalsMenuHeaderTransitionState = 1f;

				if (wasNegative && medalsMenuHeaderTransitionState >= 0f) {
					medalsMenuWorldNameString.Text = LevelDatabase.GetWorldName((byte) medalsMenuSelectedWorldIndex);
					medalsMenuWorldNameString.Color = new Vector4(
						LevelDatabase.GetWorldColor((byte) medalsMenuSelectedWorldIndex) * MEDALS_MENU_WORLD_NAME_STRING_WORLD_COLOR_WEIGHT + Vector3.ONE * (1f - MEDALS_MENU_WORLD_NAME_STRING_WORLD_COLOR_WEIGHT),
						w: medalsMenuWorldNameString.Color.W
					);
					medalsMenuWorldIconRight.Texture = medalsMenuWorldIconLeft.Texture = AssetLocator.WorldIcons[medalsMenuSelectedWorldIndex];
				}
				
				MedalsMenuSetHeaderProgress(Math.Abs(medalsMenuHeaderTransitionState));
			}

			if (medalsMenuOldObjects.Any()) {
				medalsMenuOldObjectsRemovalWorkspace.Clear();
				foreach (var oldObject in medalsMenuOldObjects) {
					oldObject.AnchorOffset = new Vector2(
						oldObject.AnchorOffset.X - MEDALS_MENU_LEVEL_LIST_WORLD_CHANGE_PER_SEC * deltaTime,
						oldObject.AnchorOffset.Y
					);

					float transitionAmount = (MEDALS_MENU_LEVEL_LIST_TARGET_X - oldObject.AnchorOffset.X) / MEDALS_MENU_LEVEL_LIST_WORLD_CHANGE_X_OFFSET;
					if (transitionAmount > 1f) medalsMenuOldObjectsRemovalWorkspace.Add(oldObject);
					oldObject.SetAlpha(1f - transitionAmount);
				}

				foreach (var toBeRemoved in medalsMenuOldObjectsRemovalWorkspace) {
					medalsMenuOldObjects.Remove(toBeRemoved);
					toBeRemoved.Dispose();
				}
				medalsMenuOldObjectsRemovalWorkspace.Clear();
			}

			var levelTimes = medalsMenuCurrentWorldTimes[medalsMenuSelectedLevelIndex];
			if (medalsMenuCurrentLevelTimes[MEDALS_MENU_MAX_SCORE_ENTRIES - 1].Color.W < 1f) {
				for (int i = 0; i < MEDALS_MENU_MAX_SCORE_ENTRIES; ++i) {
					const float ALPHA_DIFF = MEDALS_MENU_MAX_LEVEL_TIME_ALPHA_PER_SEC - MEDALS_MENU_MIN_LEVEL_TIME_ALPHA_PER_SEC;
					const float ALPHA_DIFF_PER_LINE = ALPHA_DIFF / (MEDALS_MENU_MAX_SCORE_ENTRIES - 1);

					medalsMenuCurrentLevelTimes[i].AdjustAlpha((MEDALS_MENU_MIN_LEVEL_TIME_ALPHA_PER_SEC + ALPHA_DIFF_PER_LINE * ((MEDALS_MENU_MAX_SCORE_ENTRIES - 1) - i)) * deltaTime);
					medalsMenuCurrentLevelNames[i].SetAlpha(medalsMenuCurrentLevelTimes[i].Color.W);
					if (i < levelTimes.Count) {
						switch (i) {
							case 0: medalsMenuTimesGoldMedal.SetAlpha(medalsMenuCurrentLevelTimes[i].Color.W); break;
							case 1: medalsMenuTimesSilverMedal.SetAlpha(medalsMenuCurrentLevelTimes[i].Color.W); break;
							case 2: medalsMenuTimesBronzeMedal.SetAlpha(medalsMenuCurrentLevelTimes[i].Color.W); break;
						}
						if (levelTimes[i].Key == LeaderboardManager.LocalPlayerDetails) medalsMenuTimesPersonalBestBell.SetAlpha(medalsMenuCurrentLevelTimes[i].Color.W);
					}
				}
			}

			float newX = medalsMenuChevronsLeft.AnchorOffset.X + MEDALS_MENU_CHEVRON_OFFSET_PER_SEC * deltaTime;
			while (newX < MEDALS_MENU_CHEVRON_POSITION.X + MEDALS_MENU_MAX_CHEVRON_OFFSET) newX -= MEDALS_MENU_MAX_CHEVRON_OFFSET;
			medalsMenuChevronsLeft.AnchorOffset = new Vector2(newX, medalsMenuChevronsLeft.AnchorOffset.Y);
			medalsMenuChevronsRight.AnchorOffset = new Vector2(newX, medalsMenuChevronsRight.AnchorOffset.Y);
		}

		private static void MedalsMenuCreateComponents() {
			MedalsMenuDisposeComponents();

			triggerUp += MedalsMenuChangeOptionUp;
			triggerDown += MedalsMenuChangeOptionDown;
			triggerLeft += MedalsMenuChangeOptionLeft;
			triggerRight += MedalsMenuChangeOptionRight;
			triggerConfirm += MedalsMenuConfirm;
			triggerBackOut += MedalsMenuBackOut;
			tick += MedalsMenuTick;

			medalsMenuWorldNameString = AssetLocator.TitleFontGlow.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.TopCentered,
				new Vector2(0f, MEDALS_MENU_WORLD_NAME_STRING_START_OFFSET), 
				Vector2.ONE * 0.9f
			);
			medalsMenuWorldNameString.Color = new Vector4(LevelDatabase.GetWorldColor(0) * MEDALS_MENU_WORLD_NAME_STRING_WORLD_COLOR_WEIGHT + Vector3.ONE * (1f - MEDALS_MENU_WORLD_NAME_STRING_WORLD_COLOR_WEIGHT), w: 0f);
			medalsMenuWorldNameString.Text = LevelDatabase.GetWorldName(0).Replace(" ", "   ");

			medalsMenuWorldIconLeft = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = ViewportAnchoring.TopLeft,
				AnchorOffset = new Vector2(
					0.5f - (medalsMenuWorldNameString.Dimensions.X * 0.5f + MEDALS_MENU_WORLD_ICON_HORIZ_BUFFER) + MEDALS_MENU_WORLD_ICON_HORIZ_START_OFFSET,
					medalsMenuWorldNameString.AnchorOffset.Y + MEDALS_MENU_WORLD_ICON_VERT_OFFSET
				),
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = Vector2.ONE * 0.135f,
				Texture = AssetLocator.WorldIcons[0],
				ZIndex = 2
			};

			medalsMenuWorldIconRight = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = ViewportAnchoring.TopRight,
				AnchorOffset = new Vector2(medalsMenuWorldIconLeft.AnchorOffset.X, medalsMenuWorldIconLeft.AnchorOffset.Y),
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = medalsMenuWorldIconLeft.Scale,
				Texture = AssetLocator.WorldIcons[0],
				ZIndex = 2
			};

			medalsMenuChevronsLeft = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = ViewportAnchoring.TopLeft,
				AnchorOffset = MEDALS_MENU_CHEVRON_POSITION,
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = MathUtils.PI,
				Scale = Vector2.ONE * 0.12f,
				Texture = AssetLocator.Chevrons,
				ZIndex = 2
			};
			medalsMenuChevronsRight = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = ViewportAnchoring.TopRight,
				AnchorOffset = MEDALS_MENU_CHEVRON_POSITION,
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = medalsMenuChevronsLeft.Scale,
				Texture = AssetLocator.Chevrons,
				ZIndex = 2
			};

			for (int i = 0; i < 10; ++i) {
				medalsMenuLevelBackgrounds[i] = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
					Anchoring = ViewportAnchoring.TopLeft,
					AnchorOffset = new Vector2(
						MEDALS_MENU_LEVEL_LIST_TARGET_X + MEDALS_MENU_LEVEL_LIST_WORLD_CHANGE_X_OFFSET,
						MEDALS_MENU_LEVEL_LIST_START_Y + MEDALS_MENU_LEVEL_LIST_Y_PER_ROW * i
					),
					AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.None,
					Color = new Vector4(1f, 1f, 1f, 0f),
					Rotation = 0f,
					Scale = MEDALS_MENU_LEVEL_BAR_SCALE,
					Texture = AssetLocator.PlayMenuLevelBar,
					ZIndex = 3
				};

				medalsMenuLevelBarMedals[i] = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
					Anchoring = ViewportAnchoring.TopLeft,
					AnchorOffset = medalsMenuLevelBackgrounds[i].AnchorOffset + MEDALS_MENU_LEVEL_BAR_MEDAL_OFFSET,
					AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
					Color = new Vector4(1f, 1f, 1f, 0f),
					Rotation = MathUtils.PI_OVER_TWO * 0.4f,
					Scale = Vector2.ONE * 0.04f,
					Texture = AssetLocator.FriendsGoldTimeToken,
					ZIndex = medalsMenuLevelBackgrounds[i].ZIndex + 4
				};

				medalsMenuReloadIcons.Add(new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
					Anchoring = ViewportAnchoring.TopLeft,
					AnchorOffset = new Vector2(MEDALS_MENU_LEVEL_LIST_TARGET_X + MEDALS_MENU_RELOAD_ICON_OFFSET.X, medalsMenuLevelBackgrounds[i].AnchorOffset.Y),
					AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
					Color = new Vector4(0.6f, 1f, 0.6f, 0f),
					Rotation = 0f,
					Scale = Vector2.ONE * 0.035f,
					Texture = AssetLocator.ReloadingIcon,
					ZIndex = medalsMenuLevelBarMedals[i].ZIndex - 1
				});
			}

			for (int i = 0; i < MEDALS_MENU_MAX_SCORE_ENTRIES; ++i) {
				medalsMenuCurrentLevelTimes[i] = AssetLocator.MainFont.AddString(
					AssetLocator.HudLayer,
					AssetLocator.MainWindow,
					ViewportAnchoring.TopRight,
					new Vector2(0.44f, 0.625f + (0.31f / (MEDALS_MENU_MAX_SCORE_ENTRIES - 1)) * i),
					Vector2.ONE * 0.5f
				);
				switch (i) {
					case 0:
						medalsMenuCurrentLevelTimes[i].Color = new Vector4(0.9f, 0.75f, 0.2f, 0f);
						break;
					case 1:
						medalsMenuCurrentLevelTimes[i].Color = new Vector4(0.7f, 0.7f, 0.8f, 0f);
						break;
					case 2:
						medalsMenuCurrentLevelTimes[i].Color = new Vector4(0.6f, 0.2f, 0.0f, 0f);
						break;
					default:
						medalsMenuCurrentLevelTimes[i].Color = new Vector4(0.75f, 1f, 0.75f, 0f);
						break;
				}
				medalsMenuCurrentLevelTimes[i].Text = String.Empty;

				medalsMenuCurrentLevelNames[i] = AssetLocator.MainFont.AddString(
					AssetLocator.HudLayer,
					AssetLocator.MainWindow,
					ViewportAnchoring.TopLeft,
					new Vector2(0.5875f, medalsMenuCurrentLevelTimes[i].AnchorOffset.Y),
					medalsMenuCurrentLevelTimes[i].Scale
				);
				medalsMenuCurrentLevelNames[i].Color = medalsMenuCurrentLevelTimes[i].Color;
				medalsMenuCurrentLevelNames[i].Text = String.Empty;
			}

			medalsMenuTimesGoldMedal = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = ViewportAnchoring.TopLeft,
				AnchorOffset = new Vector2(MEDALS_MENU_TIMES_MEDALS_COLUMN_POS, medalsMenuCurrentLevelTimes[0].AnchorOffset.Y + MEDALS_MENU_TIMES_MEDALS_COLUMN_Y_OFFSET),
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = Vector2.ONE * 0.03f,
				Texture = AssetLocator.FriendsGoldTimeToken,
				ZIndex = 3
			};
			medalsMenuTimesSilverMedal = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = ViewportAnchoring.TopLeft,
				AnchorOffset = new Vector2(MEDALS_MENU_TIMES_MEDALS_COLUMN_POS, medalsMenuCurrentLevelTimes[1].AnchorOffset.Y + MEDALS_MENU_TIMES_MEDALS_COLUMN_Y_OFFSET),
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = medalsMenuTimesGoldMedal.Scale,
				Texture = AssetLocator.FriendsSilverTimeToken,
				ZIndex = 3
			};
			medalsMenuTimesBronzeMedal = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = ViewportAnchoring.TopLeft,
				AnchorOffset = new Vector2(MEDALS_MENU_TIMES_MEDALS_COLUMN_POS, medalsMenuCurrentLevelTimes[2].AnchorOffset.Y + MEDALS_MENU_TIMES_MEDALS_COLUMN_Y_OFFSET),
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = medalsMenuTimesGoldMedal.Scale,
				Texture = AssetLocator.FriendsBronzeTimeToken,
				ZIndex = 3
			};
			medalsMenuTimesPersonalBestBell = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = ViewportAnchoring.TopLeft,
				AnchorOffset = new Vector2(MEDALS_MENU_TIMES_BELL_COLUMN_POS, medalsMenuCurrentLevelTimes[0].AnchorOffset.Y + MEDALS_MENU_TIMES_BELL_COLUMN_Y_OFFSET),
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = Vector2.ONE * 0.03f,
				Texture = AssetLocator.FinishingBell,
				ZIndex = 3
			};

			medalsMenuLevelBackgroundHighlight = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = ViewportAnchoring.TopLeft,
				AnchorOffset = medalsMenuLevelBackgrounds[0].AnchorOffset,
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.None,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = MEDALS_MENU_LEVEL_BAR_SCALE,
				Texture = AssetLocator.PlayMenuLevelBarHighlight,
				ZIndex = medalsMenuLevelBackgrounds[0].ZIndex + 1
			};

			medalsMenuLevelBackgroundHighlightExciter = new HUDItemExciterEntity(medalsMenuLevelBackgroundHighlight) {
				Speed = 0.02f,
				Lifetime = 0.3f,
				ColorOverride = Vector3.ONE * 2f
			};

			medalsMenuLevelNameExciter = new HUDItemExciterEntity(medalsMenuLevelNameStrings[0]) {
				Speed = 0.0025f,
				Lifetime = 0.65f,
				CountPerSec = 15,
				OpacityMultiplier = 0.85f,
				ColorOverride = MEDALS_MENU_LEVEL_BAR_STRING_COLOR * 2f
			};

			medalsMenuBackdrop = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = ViewportAnchoring.TopLeft,
				AnchorOffset = new Vector2(0.4f, 0.35f),
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.None,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = new Vector2(0.55f, 0.68f),
				Texture = AssetLocator.MedalLevelBackdrop,
				ZIndex = 3
			};

			medalsMenuLevelPreview = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = ViewportAnchoring.TopLeft,
				AnchorOffset = new Vector2(0.435f, 0.365f),
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.None,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = new Vector2(0.48f, 0.27f),
				Texture = MedalsMenuGetPreviewImage(new LevelID(0, 0)),
				ZIndex = medalsMenuBackdrop.ZIndex - 1
			};

			medalsMenuHeaderTransitionState = 1f;
			medalsMenuSelectedWorldIndex = 0;
			medalsMenuSelectedLevelIndex = 0;
			MedalsMenuSetSelectedWorld(0);
		}

		private static void MedalsMenuDisposeComponents() {
			triggerUp -= MedalsMenuChangeOptionUp;
			triggerDown -= MedalsMenuChangeOptionDown;
			triggerLeft -= MedalsMenuChangeOptionLeft;
			triggerRight -= MedalsMenuChangeOptionRight;
			triggerConfirm -= MedalsMenuConfirm;
			triggerBackOut -= MedalsMenuBackOut;
			tick -= MedalsMenuTick;

			if (medalsMenuWorldNameString != null) {
				medalsMenuWorldNameString.Dispose();
				medalsMenuWorldNameString = null;
			}
			if (medalsMenuWorldIconLeft != null) {
				medalsMenuWorldIconLeft.Dispose();
				medalsMenuWorldIconLeft = null;
			}
			if (medalsMenuWorldIconRight != null) {
				medalsMenuWorldIconRight.Dispose();
				medalsMenuWorldIconRight = null;
			}
			if (medalsMenuChevronsLeft != null) {
				medalsMenuChevronsLeft.Dispose();
				medalsMenuChevronsLeft = null;
			}
			if (medalsMenuChevronsRight != null) {
				medalsMenuChevronsRight.Dispose();
				medalsMenuChevronsRight = null;
			}
			for (int i = 0; i < medalsMenuLevelBackgrounds.Length; ++i) {
				if (medalsMenuLevelBackgrounds[i] != null) {
					medalsMenuLevelBackgrounds[i].Dispose();
					medalsMenuLevelBackgrounds[i] = null;
				}
				if (medalsMenuLevelNameStrings[i] != null) {
					medalsMenuLevelNameStrings[i].Dispose();
					medalsMenuLevelNameStrings[i] = null;
				}
				if (medalsMenuLevelBarMedals[i] != null) {
					medalsMenuLevelBarMedals[i].Dispose();
					medalsMenuLevelBarMedals[i] = null;
				}
			}
			foreach (var oldObject in medalsMenuOldObjects) {
				oldObject.Dispose();
			}
			medalsMenuOldObjects.Clear();

			foreach (var reloadIcon in medalsMenuReloadIcons) {
				reloadIcon.Dispose();
			}
			medalsMenuReloadIcons.Clear();
			medalsMenuActiveReloadIcons.Clear();

			if (medalsMenuLevelBackgroundHighlight != null) {
				medalsMenuLevelBackgroundHighlight.Dispose();
				medalsMenuLevelBackgroundHighlight = null;
			}
			if (medalsMenuLevelBackgroundHighlightExciter != null) {
				medalsMenuLevelBackgroundHighlightExciter.Dispose();
				medalsMenuLevelBackgroundHighlightExciter = null;
			}
			if (medalsMenuLevelNameExciter != null) {
				medalsMenuLevelNameExciter.Dispose();
				medalsMenuLevelNameExciter = null;
			}
			if (medalsMenuBackdrop != null) {
				medalsMenuBackdrop.Dispose();
				medalsMenuBackdrop = null;
			}
			if (medalsMenuLevelPreview != null) {
				medalsMenuLevelPreview.Dispose();
				medalsMenuLevelPreview = null;
			}
			if (medalsMenuCurPreviewFilepath != null) {
				AssetLocator.UnloadTexture(medalsMenuCurPreviewFilepath);
				medalsMenuCurPreviewFilepath = null;
			}

			for (int i = 0; i < medalsMenuCurrentLevelTimes.Length; ++i) {
				if (medalsMenuCurrentLevelTimes[i] != null) {
					medalsMenuCurrentLevelTimes[i].Dispose();
					medalsMenuCurrentLevelTimes[i] = null;
				}
				if (medalsMenuCurrentLevelNames[i] != null) {
					medalsMenuCurrentLevelNames[i].Dispose();
					medalsMenuCurrentLevelNames[i] = null;
				}
			}

			if (medalsMenuTimesGoldMedal != null) {
				medalsMenuTimesGoldMedal.Dispose();
				medalsMenuTimesGoldMedal = null;
			}
			if (medalsMenuTimesSilverMedal != null) {
				medalsMenuTimesSilverMedal.Dispose();
				medalsMenuTimesSilverMedal = null;
			}
			if (medalsMenuTimesBronzeMedal != null) {
				medalsMenuTimesBronzeMedal.Dispose();
				medalsMenuTimesBronzeMedal = null;
			}
			if (medalsMenuTimesPersonalBestBell != null) {
				medalsMenuTimesPersonalBestBell.Dispose();
				medalsMenuTimesPersonalBestBell = null;
			}
		}

		private static void MedalsMenuTickTransitionIn(float deltaTime, float fracComplete) {
			SetIdentTransitionAmount(fracComplete, false);

			medalsMenuLevelBackgroundHighlight.SetAlpha(fracComplete);
			medalsMenuBackdrop.SetAlpha(fracComplete);
			medalsMenuLevelPreview.SetAlpha(fracComplete);

			MedalsMenuSetHeaderProgress(fracComplete);
		}

		private static void MedalsMenuTickTransitionOut(float deltaTime, float fracComplete) {
			SetIdentTransitionAmount(fracComplete, true);

			medalsMenuLevelBackgroundHighlight.SetAlpha(1f - fracComplete);
			for (int i = 0; i < medalsMenuLevelBackgrounds.Length; ++i) {
				medalsMenuLevelBackgrounds[i].SetAlpha(Math.Min(1f - fracComplete, medalsMenuLevelBackgrounds[i].Color.W));
				medalsMenuLevelNameStrings[i].SetAlpha(Math.Min(1f - fracComplete, medalsMenuLevelNameStrings[i].Color.W));
				medalsMenuLevelBarMedals[i].SetAlpha(Math.Min(1f - fracComplete, medalsMenuLevelBarMedals[i].Color.W));
			}
			medalsMenuBackdrop.SetAlpha(1f - fracComplete);
			medalsMenuLevelPreview.SetAlpha(1f - fracComplete);

			for (int i = 0; i < medalsMenuCurrentLevelTimes.Length; ++i) {
				medalsMenuCurrentLevelTimes[i].SetAlpha(Math.Min(medalsMenuCurrentLevelTimes[i].Color.W, 1f - fracComplete));
				medalsMenuCurrentLevelNames[i].SetAlpha(Math.Min(medalsMenuCurrentLevelNames[i].Color.W, 1f - fracComplete));
			}

			medalsMenuTimesGoldMedal.SetAlpha(Math.Min(medalsMenuTimesGoldMedal.Color.W, 1f - fracComplete));
			medalsMenuTimesSilverMedal.SetAlpha(Math.Min(medalsMenuTimesSilverMedal.Color.W, 1f - fracComplete));
			medalsMenuTimesBronzeMedal.SetAlpha(Math.Min(medalsMenuTimesBronzeMedal.Color.W, 1f - fracComplete));
			medalsMenuTimesPersonalBestBell.SetAlpha(Math.Min(medalsMenuTimesPersonalBestBell.Color.W, 1f - fracComplete));

			MedalsMenuSetHeaderProgress(1f - fracComplete);
		}

		private static void MedalsMenuSetHeaderProgress(float fracComplete) {
			const float WORLD_NAME_OFFSET_DIFF = MEDALS_MENU_WORLD_NAME_STRING_END_OFFSET - MEDALS_MENU_WORLD_NAME_STRING_START_OFFSET;
			medalsMenuWorldNameString.AnchorOffset = new Vector2(medalsMenuWorldNameString.AnchorOffset.X, MEDALS_MENU_WORLD_NAME_STRING_START_OFFSET + WORLD_NAME_OFFSET_DIFF * fracComplete);
			medalsMenuWorldNameString.SetAlpha(fracComplete);
			float worldIconTargetX = 0.5f - (medalsMenuWorldNameString.Dimensions.X * 0.5f + MEDALS_MENU_WORLD_ICON_HORIZ_BUFFER);
			medalsMenuWorldIconLeft.AnchorOffset = new Vector2(worldIconTargetX + MEDALS_MENU_WORLD_ICON_HORIZ_START_OFFSET * (1f - fracComplete), medalsMenuWorldIconLeft.AnchorOffset.Y);
			medalsMenuWorldIconLeft.SetAlpha(fracComplete);
			medalsMenuWorldIconRight.AnchorOffset = new Vector2(medalsMenuWorldIconLeft.AnchorOffset.X, medalsMenuWorldIconRight.AnchorOffset.Y);
			medalsMenuWorldIconRight.SetAlpha(fracComplete);

			if (medalsMenuSelectedWorldIndex == 0) {
				medalsMenuChevronsLeft.SetAlpha(Math.Min(fracComplete, medalsMenuChevronsLeft.Color.W));
			}
			else medalsMenuChevronsLeft.SetAlpha(fracComplete);
			if (medalsMenuSelectedWorldIndex == PersistedWorldData.GetFurthestUnlockedWorldIndex()) {
				medalsMenuChevronsRight.SetAlpha(Math.Min(fracComplete, medalsMenuChevronsRight.Color.W));
			}
			else medalsMenuChevronsRight.SetAlpha(fracComplete);
		}

		private static void MedalsMenuConfirm() {
			HUDSound.UISelectOption.Play();
			startGateCounter = 0;
			Thread.MemoryBarrier();
			BGMManager.CrossFadeToWorldMusic((byte) medalsMenuSelectedWorldIndex);
			MedalsMenuTransitionOut(true);
			currentTransitionComplete += MedalsMenuStartLevelGate;
			BGMManager.CrossfadeComplete += MedalsMenuStartLevelGate;
			longLoadBackdrop = AssetLocator.LoadTexture(Path.Combine(AssetLocator.MaterialsDir, @"Previews\") + "load" + medalsMenuSelectedWorldIndex + ".png");
			GameCoordinator.EnableLongLoadingScreen(longLoadBackdrop, MedalsMenuStartLevelGate);
		}

		private static void MedalsMenuBackOut() {
			HUDSound.UISelectNegativeOption.Play();
			MedalsMenuTransitionOut();
			if (!inDeferenceMode) currentTransitionComplete += () => MainMenuTransitionIn();
		}

		private static void MedalsMenuChangeOptionUp() {
			HUDSound.UIPreviousOption.Play();
			MedalsMenuSetSelectedLevel(medalsMenuSelectedLevelIndex - 1 < 0 ? 9 : medalsMenuSelectedLevelIndex - 1);
		}

		private static void MedalsMenuChangeOptionDown() {
			HUDSound.UINextOption.Play();
			MedalsMenuSetSelectedLevel((medalsMenuSelectedLevelIndex + 1) % 10);
		}

		private static void MedalsMenuChangeOptionLeft() {
			if (medalsMenuSelectedWorldIndex > 0) {
				MedalsMenuSetSelectedWorld(medalsMenuSelectedWorldIndex - 1);
				HUDSound.PostPassShowMenu.Play(pitch: 0.85f);
			}
			else HUDSound.UIUnavailable.Play();
		}

		private static void MedalsMenuChangeOptionRight() {
			if (medalsMenuSelectedWorldIndex < PersistedWorldData.GetFurthestUnlockedWorldIndex()) {
				MedalsMenuSetSelectedWorld(medalsMenuSelectedWorldIndex + 1);
				HUDSound.PostPassShowMenu.Play(pitch: 1.15f);
			}
			else HUDSound.UIUnavailable.Play();
		}

		private static void MedalsMenuSetSelectedLevel(int newIndex, bool requestUpdateFromSteam = true) {
			var thisLevel = new LevelID((byte) medalsMenuSelectedWorldIndex, (byte) newIndex);
			
			medalsMenuLevelNameExciter.TargetObj = medalsMenuLevelNameStrings[newIndex];
			medalsMenuLevelBackgroundHighlight.AnchorOffset = medalsMenuLevelBackgrounds[newIndex].AnchorOffset;


			medalsMenuLevelPreview.Texture = MedalsMenuGetPreviewImage(thisLevel);

			medalsMenuTimesGoldMedal.SetAlpha(0f);
			medalsMenuTimesSilverMedal.SetAlpha(0f);
			medalsMenuTimesBronzeMedal.SetAlpha(0f);
			medalsMenuTimesPersonalBestBell.SetAlpha(0f);
			var currentTimes = medalsMenuCurrentWorldTimes[newIndex];
			for (int i = 0; i < medalsMenuCurrentLevelTimes.Length; ++i) {
				medalsMenuCurrentLevelTimes[i].SetAlpha(0f);
				medalsMenuCurrentLevelNames[i].SetAlpha(0f);

				if (currentTimes.Count <= i) {
					medalsMenuCurrentLevelTimes[i].Text = String.Empty;
					medalsMenuCurrentLevelNames[i].Text = String.Empty;
				}
				else {
					medalsMenuCurrentLevelTimes[i].Text = MakeTimeString(currentTimes[i].Value);
					medalsMenuCurrentLevelNames[i].Text = currentTimes[i].Key.Name.WithMaxLength(MEDALS_MENU_NAME_MAX_LEN);
					if (currentTimes[i].Key == LeaderboardManager.LocalPlayerDetails) {
						medalsMenuTimesPersonalBestBell.AnchorOffset = new Vector2(
							MEDALS_MENU_TIMES_BELL_COLUMN_POS,
							medalsMenuCurrentLevelTimes[i].AnchorOffset.Y + MEDALS_MENU_TIMES_BELL_COLUMN_Y_OFFSET
						);
					}
				}
			}

			medalsMenuSelectedLevelIndex = newIndex;

			if (requestUpdateFromSteam) {
				var reloadIcon = medalsMenuReloadIcons[newIndex];
				if (!medalsMenuActiveReloadIcons.Contains(reloadIcon)) medalsMenuActiveReloadIcons.Add(reloadIcon);
				Task.Run(() => {
					var levelLocal = thisLevel;
					var updatedData = LeaderboardManager.GetTopNFriends(levelLocal, MEDALS_MENU_MAX_SCORE_ENTRIES);
					UpdateUIAfterFreshDataRead(levelLocal.WorldIndex, levelLocal.LevelIndex, updatedData);
					if (medalsMenuActiveReloadIcons.Contains(reloadIcon)) medalsMenuActiveReloadIcons.Remove(reloadIcon);
				});	
			}
		}

		private static void MedalsMenuSetSelectedWorld(int newIndex) {
			medalsMenuActiveReloadIcons.Clear();
			medalsMenuWorldNameString.Text = LevelDatabase.GetWorldName((byte) medalsMenuSelectedWorldIndex);
			medalsMenuWorldNameString.Color = new Vector4(
				LevelDatabase.GetWorldColor((byte) medalsMenuSelectedWorldIndex) * MEDALS_MENU_WORLD_NAME_STRING_WORLD_COLOR_WEIGHT + Vector3.ONE * (1f - MEDALS_MENU_WORLD_NAME_STRING_WORLD_COLOR_WEIGHT),
				w: medalsMenuWorldNameString.Color.W
			);
			medalsMenuWorldIconRight.Texture = medalsMenuWorldIconLeft.Texture = AssetLocator.WorldIcons[medalsMenuSelectedWorldIndex];

			medalsMenuSelectedWorldIndex = newIndex;

			medalsMenuHeaderTransitionState = -1f;
			for (int i = 0; i < 10; ++i) {
				var thisLevel = new LevelID((byte) newIndex, (byte) i);
				if (medalsMenuLevelBackgrounds[i] != null) medalsMenuOldObjects.Add(medalsMenuLevelBackgrounds[i]);
				if (medalsMenuLevelNameStrings[i] != null) medalsMenuOldObjects.Add(medalsMenuLevelNameStrings[i]);

				medalsMenuLevelBackgrounds[i] = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
					Anchoring = ViewportAnchoring.TopLeft,
					AnchorOffset = new Vector2(
						MEDALS_MENU_LEVEL_LIST_TARGET_X + MEDALS_MENU_LEVEL_LIST_WORLD_CHANGE_X_OFFSET,
						MEDALS_MENU_LEVEL_LIST_START_Y + MEDALS_MENU_LEVEL_LIST_Y_PER_ROW * i
					),
					AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.None,
					Color = new Vector4(1f, 1f, 1f, 0f),
					Rotation = 0f,
					Scale = MEDALS_MENU_LEVEL_BAR_SCALE,
					Texture = AssetLocator.PlayMenuLevelBar,
					ZIndex = 3
				};

				medalsMenuLevelNameStrings[i] = AssetLocator.TitleFont.AddString(
					AssetLocator.HudLayer,
					AssetLocator.MainWindow,
					ViewportAnchoring.TopLeft,
					medalsMenuLevelBackgrounds[i].AnchorOffset + MEDALS_MENU_LEVEL_BAR_STRING_OFFSET,
					MEDALS_MENU_LEVEL_BAR_STRING_SCALE
				);
				medalsMenuLevelNameStrings[i].Color = MEDALS_MENU_LEVEL_BAR_STRING_COLOR;
				medalsMenuLevelNameStrings[i].Text = LevelDatabase.GetLevelTitle(thisLevel);

				var topFriends = LeaderboardManager.GetTopNFriendsFromCache(thisLevel, MEDALS_MENU_MAX_SCORE_ENTRIES);
				var bestUserTime = PersistedWorldData.GetBestTimeForLevel(thisLevel);
				if (bestUserTime != null && !topFriends.Any(friend => friend.Key == LeaderboardManager.LocalPlayerDetails)) {
					topFriends[topFriends.Count - 1] = new KeyValuePair<LeaderboardPlayer, int>(LeaderboardManager.LocalPlayerDetails, bestUserTime.Value);
				}
				medalsMenuCurrentWorldTimes[i] = topFriends;

				medalsMenuLevelBarMedals[i].SetAlpha(0f);
				if (topFriends.Count >= 1 && topFriends[0].Key == LeaderboardManager.LocalPlayerDetails) {
					medalsMenuLevelBarMedals[i].Texture = AssetLocator.FriendsGoldTimeToken;
				}
				else if (topFriends.Count >= 2 && topFriends[1].Key == LeaderboardManager.LocalPlayerDetails) {
					medalsMenuLevelBarMedals[i].Texture = AssetLocator.FriendsSilverTimeToken;
				}
				else if (topFriends.Count >= 3 && topFriends[2].Key == LeaderboardManager.LocalPlayerDetails) {
					medalsMenuLevelBarMedals[i].Texture = AssetLocator.FriendsBronzeTimeToken;
				}
				else medalsMenuLevelBarMedals[i].Texture = null;
			}

			MedalsMenuSetSelectedLevel(medalsMenuSelectedLevelIndex);
		}

		private static void UpdateUIAfterFreshDataRead(int worldIndex, int levelIndex, List<KeyValuePair<LeaderboardPlayer, int>> updatedData) {
			EntityModule.SingleFireAfterNextTick += () => {
				lock (staticMutationLock) {
					if (medalsMenuSelectedWorldIndex != worldIndex) return;
					if (currentMenuState != MenuState.MedalsMenu) return;
					var currentData = medalsMenuCurrentWorldTimes[levelIndex];
					if (currentData.Count == updatedData.Count) {
						for (int i = 0; i < currentData.Count; ++i) {
							if (currentData[i].Key != updatedData[i].Key || currentData[i].Value != updatedData[i].Value) goto notIdentical;
						}
						return;
						notIdentical: ;
					}

					var thisLevel = new LevelID((byte) worldIndex, (byte) levelIndex);
					var bestUserTime = PersistedWorldData.GetBestTimeForLevel(thisLevel);
					if (bestUserTime != null && !updatedData.Any(friend => friend.Key == LeaderboardManager.LocalPlayerDetails)) {
						updatedData[updatedData.Count - 1] = new KeyValuePair<LeaderboardPlayer, int>(LeaderboardManager.LocalPlayerDetails, bestUserTime.Value);
					}
					medalsMenuCurrentWorldTimes[levelIndex] = updatedData;

					medalsMenuLevelBarMedals[levelIndex].SetAlpha(0f);
					if (updatedData.Count >= 1 && updatedData[0].Key == LeaderboardManager.LocalPlayerDetails) {
						medalsMenuLevelBarMedals[levelIndex].Texture = AssetLocator.FriendsGoldTimeToken;
					}
					else if (updatedData.Count >= 2 && updatedData[1].Key == LeaderboardManager.LocalPlayerDetails) {
						medalsMenuLevelBarMedals[levelIndex].Texture = AssetLocator.FriendsSilverTimeToken;
					}
					else if (updatedData.Count >= 3 && updatedData[2].Key == LeaderboardManager.LocalPlayerDetails) {
						medalsMenuLevelBarMedals[levelIndex].Texture = AssetLocator.FriendsBronzeTimeToken;
					}
					else medalsMenuLevelBarMedals[levelIndex].Texture = null;

					if (levelIndex == medalsMenuSelectedLevelIndex) MedalsMenuSetSelectedLevel(levelIndex, requestUpdateFromSteam: false);
				}
			};
		}

		private static ITexture2D MedalsMenuGetPreviewImage(LevelID lid) {
			string previewFilename = Path.Combine("Previews\\", "level" + lid.WorldIndex + "-" + lid.LevelIndex + ".png");
			if (!File.Exists(Path.Combine(AssetLocator.MaterialsDir, previewFilename))) {
				string stubfile = "green.bmp";
				var image = AssetLocator.LoadTexture(stubfile, false);
				if (medalsMenuCurPreviewFilepath != null) {
					AssetLocator.UnloadTexture(medalsMenuCurPreviewFilepath);
					medalsMenuCurPreviewFilepath = stubfile;
				}
				return image;
			}
			else {
				var image = AssetLocator.LoadTexture(previewFilename, false);
				if (medalsMenuCurPreviewFilepath != null) {
					AssetLocator.UnloadTexture(medalsMenuCurPreviewFilepath);
					medalsMenuCurPreviewFilepath = previewFilename;
				}
				return image;
			}
		}

		private static void MedalsMenuStartLevelGate() {
			lock (staticMutationLock) {
				if (++startGateCounter == 3) {
					EntityModule.AddTimedAction(MedalsMenuLoadLevel, 0.5f);
				}
			}
		}

		private static void MedalsMenuLoadLevel() {
			lock (staticMutationLock) {
				UnloadMenuBackdrop();
				CurrentMenuState = MenuState.InGame;

				var selectedLevelID = new LevelID((byte) medalsMenuSelectedWorldIndex, (byte) medalsMenuSelectedLevelIndex);
				GameCoordinator.LoadLevel(selectedLevelID);
				EntityModule.AddTimedAction(MedalsMenuStartLevel, 0.5f);
			}
		}

		private static void MedalsMenuStartLevel() {
			lock (staticMutationLock) {
				GameCoordinator.DisableLongLoadingScreen(
					LevelDatabase.GetWorldColor((byte) medalsMenuSelectedWorldIndex) * MEDALS_MENU_WORLD_NAME_STRING_WORLD_COLOR_WEIGHT + Vector3.ONE * (1f - MEDALS_MENU_WORLD_NAME_STRING_WORLD_COLOR_WEIGHT), 
					() => {
						AssetLocator.UnloadTexture(Path.Combine(AssetLocator.MaterialsDir, @"Previews\") + "load" + medalsMenuSelectedWorldIndex + ".png");
						GameCoordinator.StartLevelIntroduction();
					}
				);
			}
		}
	}
}