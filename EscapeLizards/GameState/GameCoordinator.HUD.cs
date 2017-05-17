// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 08 07 2015 at 16:14 by Ben Bowen

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Ophidian.Losgap;
using Ophidian.Losgap.Audio;
using Ophidian.Losgap.Entities;
using Ophidian.Losgap.Rendering;

namespace Egodystonic.EscapeLizards {
	public static partial class GameCoordinator {
		#region HUD Components
		private static readonly ShaderResourceView[] cachedTimerFigures = new ShaderResourceView[10 * 15];

		private static FontString kmhLabel;

		private static HUDTexture eggCounterBackdrop;
		private static readonly HUDTexture[] vultureEggs = new HUDTexture[5];
		private static readonly HUDTexture[] vultureEggCrosses = new HUDTexture[5];

		private static HUDTexture coinBackdrop;
		private static HUDTexture spinningCoin;
		private static FontString numCoinsString;
		private const float TIME_PER_COIN_FRAME = 0.45f / 64f;
		private static float timeOnCurrentCoinFrame = 0f;
		private static int curCoinTexIndex = -1;
		private static readonly Vector2 DEFAULT_COIN_SCALE = new Vector2(0.08f, 0.1f) * 0.43f;

		private static HUDTexture timerBackdrop;
		private static HUDTexture timerBackdropGlass;
		private static readonly HUDTexture[] timerFigures = new HUDTexture[4];

		private static HUDTexture[] starTimeBackdrops = new HUDTexture[3];
		private static HUDTexture[] starTimeStars = new HUDTexture[3];
		private static FontString[] starTimeStrings = new FontString[3];

		private const float WORLD_NAME_BACKDROP_MIN_X_SCALE = 0.275f;
		private const float WORLD_NAME_BACKDROP_ICON_BUFFER = 0.0525f;
		private const float WORLD_NAME_STRING_X_BUFFER = 0.004f;
		private const float LEVEL_NAME_BACKDROP_MIN_X_SCALE = 0.325f;
		private const float LEVEL_NAME_STRING_X_BUFFER = 0.014f;
		private static HUDTexture worldNameBackdrop;
		private static HUDTexture levelNameBackdrop;
		private static FontString worldNameString;
		private static FontString levelNameString;
		private static FontString levelNumberString;
		private static HUDTexture worldIcon;

		private static HUDTexture personalBestTimeBackdrop;
		private static HUDTexture goldFriendsTimeBackdrop;
		private static HUDTexture silverFriendsTimeBackdrop;
		private static HUDTexture bronzeFriendsTimeBackdrop;

		private static HUDTexture personalBestBell;
		private static HUDTexture goldFriendsToken;
		private static HUDTexture silverFriendsToken;
		private static HUDTexture bronzeFriendsToken;
		private static FontString personalBestString;
		private static FontString goldFriendTimeString;
		private static FontString silverFriendTimeString;
		private static FontString bronzeFriendTimeString;
		private static FontString goldFriendNameString;
		private static FontString silverFriendNameString;
		private static FontString bronzeFriendNameString;

		private readonly static Vector2[] starTimeStarVelocities = new Vector2[3];
		private static readonly Vector2 INITIAL_STAR_VELOCITY = Vector2.DOWN * 0.33f + Vector2.RIGHT * 0.17f;

		private static readonly List<HUDTexture> eggHopTextures = new List<HUDTexture>();
		private static readonly List<HUDItemExciterEntity> eggHopExciters = new List<HUDItemExciterEntity>();
		private static readonly Vector2 EGG_HOP_TEX_TARGET_SCALE = new Vector2(0.04f, 0.08f) * 1.4f;
		private static readonly Vector2 EGG_HOP_TEX_USE_TARGET_SCALE = EGG_HOP_TEX_TARGET_SCALE * 1f;
		private static readonly Vector4 EGG_HOP_TEX_USE_COLOR = new Vector4(167f, 225f, 49f, 255f) / 255f;
		private static readonly List<HUDTexture> tickTextures = new List<HUDTexture>();
		private static HUDTexture goldTick, silverTick, bronzeTick;

		private static readonly Vector2 HUD_TICK_SCALE = Vector2.ONE * 0.06f;
		private static readonly Vector4 HUD_TICK_COLOR = new Vector4(122f / 255f, 198f / 255f, 0f / 255f, 255f / 255f);

		private static HUDTexture difficultyIcon;
		private static FontString difficultyString;
		#endregion

		#region Intro HUD
		private const float READY_TEXTURE_FADE_IN_REMAINING_TIME = 5f;
		private const float READY_TEXTURE_FADE_IN_DURATION = 1.3f;
		private const float READY_TEXTURE_MAX_ALPHA = 0.85f;
		private const float GO_TEXTURE_FADE_IN_DURATION = 0.1333f;
		private const float GO_TEXTURE_LINGER_DURATION = 0.35f;
		private const float GO_TEXTURE_FADE_OUT_DURATION = 0.175f;
		private static bool playedReadySound;
		private static HUDTexture readyTexture;
		private static HUDTexture goTexture;
		private static readonly Vector2 readyTexScale = new Vector2(0.37f, 0.3f);
		private static readonly Vector2 goTexTargetScale = new Vector2(0.27f, 0.3f);
		private const float WORLD_AND_LEVEL_FADE_IN_REMAINING_TIME = 6f;
		private const float WORLD_AND_LEVEL_FADE_IN_DURATION = 2f;
		private const float WORLD_AND_LEVEL_LETTER_FADE_IN_DURATION = 0.6f;
		private const float WORLD_AND_LEVEL_LETTER_MAX_ALPHA = 0.75f;
		private static readonly Vector3 introWorldCharColor = new Vector3(255f / 255f, 186f / 255f, 116f / 255f);
		private static readonly Vector3 introLevelCharColor = new Vector3(218f / 255f, 255f / 255f, 127f / 255f);
		private static readonly List<FontString> introWorldChars = new List<FontString>();
		private static readonly List<FontString> introLevelChars = new List<FontString>();
		private static readonly List<HUDTexture> goalIndicatorCircles = new List<HUDTexture>();
		private static readonly List<HUDTexture> goalIndicatorArrows = new List<HUDTexture>();
		private static HUDTexture spawnIndicatorCircle, spawnIndicatorArrow;
		private static List<Vector3> goalLocations = new List<Vector3>();
		private static Vector3 spawnLocation;
		private const float INDICATOR_FADE_IN_RATE = 1f;
		private static readonly Vector2 introIndicatorCircleScale = Vector2.ONE * 0.08f;
		private static readonly Vector2 introIndicatorArrowScale = new Vector2(introIndicatorCircleScale.X, introIndicatorCircleScale.Y * (170f / 300f));
		private const float INTRO_INDICATOR_CIRCLE_MARGIN = 0.07f;
		private const float INTRO_INDICATOR_ARROW_MARGIN_FRAC = 0.33f;
		private const float CHAR_FADE_OUT_TIME = 0.3f;
		private const float CHAR_GRAVITY = 3f;
		private static readonly Dictionary<FontString, Vector2> charVelocities = new Dictionary<FontString, Vector2>();
		private static float postIntroHUDTimeElapsed;
		private static LevelID lastIntroID = new LevelID(255, 255);
		private const float INTRO_FRACTION_REMAINING_AT_HOP_DISPLAY_START = 0.65f;
		private const float INTRO_FRACTION_REMAINING_AT_HOP_DISPLAY_END = 0.2f;

		private static HUDTexture introDifficultyIconLeft, introDifficultyIconRight;
		private static FontString introDifficultyString;
		private const float INTRO_DIFF_ICON_BUFFER = 0.02f;
		private const float INTRO_DIFF_MAX_ALPHA = 0.66f;
		private const float INTRO_DIFF_INITIAL_Y = (1f - 0.635f) - 0.22f;

		private static void ClearHUDIntroElements() {
			if (readyTexture != null) {
				readyTexture.Dispose();
				readyTexture = null;
			}
			playedReadySound = false;

			if (goTexture != null) {
				goTexture.Dispose();
				goTexture = null;
			}

			foreach (var c in introWorldChars) {
				c.Dispose();
			}
			introWorldChars.Clear();

			foreach (var c in introLevelChars) {
				c.Dispose();
			}
			introLevelChars.Clear();

			charVelocities.Clear();

			foreach (var indicator in goalIndicatorCircles) {
				indicator.SetAlpha(0f);
			}
			foreach (var indicator in goalIndicatorArrows) {
				indicator.SetAlpha(0f);
			}
			if (spawnIndicatorCircle != null) spawnIndicatorCircle.SetAlpha(0f);
			if (spawnIndicatorArrow != null) spawnIndicatorArrow.SetAlpha(0f);

			postIntroHUDTimeElapsed = 0f;

			if (introDifficultyIconLeft != null) {
				introDifficultyIconLeft.Dispose();
				introDifficultyIconLeft = null;
			}
			if (introDifficultyIconRight != null) {
				introDifficultyIconRight.Dispose();
				introDifficultyIconRight = null;
			}
			if (introDifficultyString != null) {
				introDifficultyString.Dispose();
				introDifficultyString = null;
			}
		}

		private static void HUD_StartIntro() {
			ClearHUDIntroElements();

			goalLocations = CurrentlyLoadedLevel.GetGameObjectsByType<FinishingBell>().Select(goal => goal.Transform.Translation).ToList();
			spawnLocation = CurrentlyLoadedLevel.GetGameObjectsByType<StartFlag>().Single().Transform.Translation;
			var numExistingIndicators = goalIndicatorCircles.Count;
			for (int i = 0; i < goalLocations.Count - numExistingIndicators; ++i) {
				goalIndicatorCircles.Add(
					new HUDTexture(
						AssetLocator.HUDFragmentShader,
						AssetLocator.HudLayer,
						AssetLocator.MainWindow
					) {
						Anchoring = ViewportAnchoring.Centered,
						AnchorOffset = new Vector2(0f, 0f),
						AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
						Color = new Vector4(1f, 1f, 1f, 0f),
						Rotation = 0f,
						Scale = introIndicatorCircleScale,
						Texture = AssetLocator.IntroGoalIndicatorCircle,
						ZIndex = -1
					}
				);

				goalIndicatorArrows.Add(
					new HUDTexture(
						AssetLocator.HUDFragmentShader,
						AssetLocator.HudLayer,
						AssetLocator.MainWindow
					) {
						Anchoring = ViewportAnchoring.Centered,
						AnchorOffset = new Vector2(0f, 0f),
						AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
						Color = new Vector4(1f, 1f, 1f, 0f),
						Rotation = 0f,
						Scale = introIndicatorArrowScale,
						Texture = AssetLocator.IntroGoalIndicatorArrow,
						ZIndex = -2
					}
				);
			}

			if (spawnIndicatorCircle == null) {
				spawnIndicatorCircle = new HUDTexture(
					AssetLocator.HUDFragmentShader,
					AssetLocator.HudLayer,
					AssetLocator.MainWindow
				) {
					Anchoring = ViewportAnchoring.Centered,
					AnchorOffset = new Vector2(0f, 0f),
					AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
					Color = new Vector4(1f, 1f, 1f, 0f),
					Rotation = 0f,
					Scale = introIndicatorCircleScale,
					Texture = AssetLocator.IntroSpawnIndicatorCircle,
					ZIndex = -3
				};
				spawnIndicatorArrow = new HUDTexture(
					AssetLocator.HUDFragmentShader,
					AssetLocator.HudLayer,
					AssetLocator.MainWindow
				) {
					Anchoring = ViewportAnchoring.Centered,
					AnchorOffset = new Vector2(0f, 0f),
					AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
					Color = new Vector4(1f, 1f, 1f, 0f),
					Rotation = 0f,
					Scale = introIndicatorArrowScale,
					Texture = AssetLocator.IntroSpawnIndicatorArrow,
					ZIndex = -4
				};
			}

			readyTexture = new HUDTexture(
				AssetLocator.HUDFragmentShader,
				AssetLocator.HudLayer,
				AssetLocator.MainWindow
			) {
				Anchoring = ViewportAnchoring.TopCentered,
				AnchorOffset = new Vector2(0f, 0.07f),
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.None,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = readyTexScale,
				Texture = AssetLocator.ReadyTex,
				ZIndex = 0
			};
			goTexture = new HUDTexture(
				AssetLocator.HUDFragmentShader,
				AssetLocator.HudLayer,
				AssetLocator.MainWindow
			) {
				Anchoring = ViewportAnchoring.TopCentered,
				AnchorOffset = readyTexture.AnchorOffset + Vector2.DOWN * 0.024f, // nb: down is up x.x
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.None,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = Vector2.ZERO,
				Texture = AssetLocator.GoTex,
				ZIndex = 0
			};

			if (lastIntroID == currentlyLoadedLevelID) return;


			const float X_OFFSET_PER_WORLD_LETTER = 0.026f;
			const float X_OFFSET_PER_LEVEL_LETTER = 0.034f;

			string worldName = LevelDatabase.GetWorldName(currentlyLoadedLevelID.WorldIndex) + " " + (currentlyLoadedLevelID.LevelIndex + 1);
			string levelName = ((char) 167) + " " + CurrentlyLoadedLevel.Title + " " + ((char) 167);
			float worldStringStartX = 0.5f - ((worldName.Length - 1) / 2f) * X_OFFSET_PER_WORLD_LETTER;
			float levelStringStartX = 0.5f - ((levelName.Length - 1) / 2f) * X_OFFSET_PER_LEVEL_LETTER;
			for (int i = 0; i < worldName.Length; ++i) {
				FontString letter = AssetLocator.TitleFont.AddString(
					AssetLocator.HudLayer,
					AssetLocator.MainWindow,
					ViewportAnchoring.TopLeft,
					new Vector2(worldStringStartX + i * X_OFFSET_PER_WORLD_LETTER, 0.57f),
					Vector2.ONE * 0.44f
				);
				letter.Text = worldName[i].ToString();
				letter.Color = new Vector4(introWorldCharColor, w: 0f);
				introWorldChars.Add(letter);
			}

			for (int i = 0; i < levelName.Length; ++i) {
				FontString letter = AssetLocator.TitleFont.AddString(
					AssetLocator.HudLayer,
					AssetLocator.MainWindow,
					ViewportAnchoring.TopLeft,
					new Vector2(levelStringStartX + i * X_OFFSET_PER_LEVEL_LETTER, 0.635f),
					Vector2.ONE * 0.57f
				);
				letter.Text = levelName[i].ToString();
				letter.Color = new Vector4(introLevelCharColor, w: 0f);
				introLevelChars.Add(letter);
			}

			introDifficultyString = AssetLocator.TitleFont.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.BottomCentered,
				new Vector2(0f, INTRO_DIFF_INITIAL_Y),
				Vector2.ONE * 0.44f * 1.0f
			);
			introDifficultyString.Text = LevelDatabase.GetLevelDifficulty(CurrentlyLoadedLevelID).FriendlyName();
			introDifficultyString.Color = LevelDatabase.GetLevelDifficulty(CurrentlyLoadedLevelID).Color();

			introDifficultyIconLeft = new HUDTexture(
				AssetLocator.HUDFragmentShader,
				AssetLocator.HudLayer,
				AssetLocator.MainWindow
			) {
				Anchoring = ViewportAnchoring.BottomCentered,
				AnchorOffset = new Vector2(introDifficultyString.Dimensions.X * -0.5f - INTRO_DIFF_ICON_BUFFER, introDifficultyString.AnchorOffset.Y - 0.01f),
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = LevelDatabase.GetLevelDifficulty(CurrentlyLoadedLevelID).Color(),
				Rotation = 0f,
				Scale = Vector2.ONE * 0.05f,
				Texture = LevelDatabase.GetLevelDifficulty(CurrentlyLoadedLevelID).Tex(),
				ZIndex = 3
			};
			introDifficultyIconRight = new HUDTexture(
				AssetLocator.HUDFragmentShader,
				AssetLocator.HudLayer,
				AssetLocator.MainWindow
			) {
				Anchoring = ViewportAnchoring.BottomCentered,
				AnchorOffset = new Vector2(introDifficultyString.Dimensions.X * 0.5f + INTRO_DIFF_ICON_BUFFER, introDifficultyString.AnchorOffset.Y - 0.01f),
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = LevelDatabase.GetLevelDifficulty(CurrentlyLoadedLevelID).Color(),
				Rotation = 0f,
				Scale = introDifficultyIconLeft.Scale,
				Texture = LevelDatabase.GetLevelDifficulty(CurrentlyLoadedLevelID).Tex(),
				ZIndex = 3
			};
		}

		private static unsafe void HUD_TickIntro(float deltaTime, float introTimeRemaining) {
			HUD_TickIndicators(deltaTime);

			if (currentGameLevel.NumHops > 0) {
				float timeRemainingAtStart = Config.LevelIntroductionTime * INTRO_FRACTION_REMAINING_AT_HOP_DISPLAY_START;
				float timeRemainingAtEnd = Config.LevelIntroductionTime * INTRO_FRACTION_REMAINING_AT_HOP_DISPLAY_END;
				float timeDiff = timeRemainingAtEnd - timeRemainingAtStart;
				float diffPerHop = currentGameLevel.NumHops > 1 ? timeDiff / (currentGameLevel.NumHops - 1) : timeDiff;
				for (int i = 0; i < currentGameLevel.NumHops; ++i) {
					float revealTime = timeRemainingAtStart + diffPerHop * i;
					if (introTimeRemaining < revealTime) {
						float elapsedTime = deltaTime * (accelerateIntroductions ? Config.LevelIntroAccelerationModifier : 1f);
						if (introTimeRemaining + elapsedTime >= revealTime) {
							eggHopTextures[i].Scale = EGG_HOP_TEX_TARGET_SCALE * 2f;
							int instanceId = AudioModule.CreateSoundInstance(AssetLocator.ShowHopSound);
							AudioModule.PlaySoundInstance(
								instanceId,
								false,
								0.33f * Config.HUDVolume,
								0.5f + i * 0.1f
							);
						}
						else {
							float scaleInTime = timeRemainingAtEnd * 0.9f;
							float completionFrac = Math.Min((revealTime - introTimeRemaining) / scaleInTime, 1f);
							eggHopTextures[i].SetAlpha(completionFrac);
							eggHopTextures[i].Scale = EGG_HOP_TEX_TARGET_SCALE * (2f - completionFrac);
						}
					}

				}
			}

			if (introTimeRemaining <= READY_TEXTURE_FADE_IN_REMAINING_TIME) {
				if (!playedReadySound) {
					HUDSoundExtensions.Play(HUDSound.IntroReady);
					playedReadySound = true;
				}
				float alpha = Math.Min((READY_TEXTURE_FADE_IN_REMAINING_TIME - introTimeRemaining) / READY_TEXTURE_FADE_IN_DURATION, 1f) * READY_TEXTURE_MAX_ALPHA;
				readyTexture.Color = new Vector4(readyTexture.Color, w: alpha);
			}

			if (introTimeRemaining <= GO_TEXTURE_FADE_IN_DURATION) {
				readyTexture.Color = Vector4.ZERO;
				float proportionComplete = Math.Min((GO_TEXTURE_FADE_IN_DURATION - introTimeRemaining) / GO_TEXTURE_FADE_IN_DURATION, 1f);
				goTexture.Color = new Vector4(1f, 1f, 1f, proportionComplete * READY_TEXTURE_MAX_ALPHA);
				goTexture.Scale = goTexTargetScale * proportionComplete;
			}

			if (lastIntroID == currentlyLoadedLevelID) return;

			if (introTimeRemaining <= WORLD_AND_LEVEL_FADE_IN_REMAINING_TIME) {
				float startOffsetPerWorldLetter = (WORLD_AND_LEVEL_FADE_IN_DURATION - WORLD_AND_LEVEL_LETTER_FADE_IN_DURATION) / introWorldChars.Count;
				float startOffsetPerLevelLetter = (WORLD_AND_LEVEL_FADE_IN_DURATION - WORLD_AND_LEVEL_LETTER_FADE_IN_DURATION) / introLevelChars.Count;
				float introStringDurationSoFar = WORLD_AND_LEVEL_FADE_IN_REMAINING_TIME - introTimeRemaining;

				for (int i = 0; i < introWorldChars.Count; ++i) {
					float letterStartTime = startOffsetPerWorldLetter * i;
					if (letterStartTime > introStringDurationSoFar) break;
					float letterDurationSoFar = introStringDurationSoFar - letterStartTime;
					float alpha = Math.Min(letterDurationSoFar / WORLD_AND_LEVEL_LETTER_FADE_IN_DURATION, 1f) * WORLD_AND_LEVEL_LETTER_MAX_ALPHA;
					introWorldChars[i].Color = new Vector4(introWorldChars[i].Color, w: alpha);
				}

				for (int i = 0; i < introLevelChars.Count; ++i) {
					float letterStartTime = startOffsetPerLevelLetter * i;
					if (letterStartTime > introStringDurationSoFar) break;
					float letterDurationSoFar = introStringDurationSoFar - letterStartTime;
					float alpha = Math.Min(letterDurationSoFar / WORLD_AND_LEVEL_LETTER_FADE_IN_DURATION, 1f) * WORLD_AND_LEVEL_LETTER_MAX_ALPHA;
					introLevelChars[introLevelChars.Count - (1 + i)].Color = new Vector4(introLevelChars[introLevelChars.Count - (1 + i)].Color, w: alpha);
				}

				float diffFadeInCompletion = (WORLD_AND_LEVEL_FADE_IN_REMAINING_TIME - introTimeRemaining) / (WORLD_AND_LEVEL_FADE_IN_DURATION * 1.5f);
				if (diffFadeInCompletion > 1f) diffFadeInCompletion = 1f;

				introDifficultyIconLeft.SetAlpha(diffFadeInCompletion * INTRO_DIFF_MAX_ALPHA);
				introDifficultyIconRight.SetAlpha(diffFadeInCompletion * INTRO_DIFF_MAX_ALPHA);
				introDifficultyString.SetAlpha(diffFadeInCompletion * INTRO_DIFF_MAX_ALPHA);
			}


		}

		private static void HUD_EndIntro() {
			//HUDItemExciterEntity.ClearExciterMaterials();

			readyTexture.Color = Vector4.ZERO;
			goTexture.Color = new Vector4(1f, 1f, 1f, READY_TEXTURE_MAX_ALPHA);
			HUDSoundExtensions.Play(HUDSound.IntroGo);

			if (lastIntroID == currentlyLoadedLevelID) return;

			foreach (var c in introLevelChars) {
				charVelocities.Add(c, new Vector2(RandomProvider.Next(-1f, 1f), RandomProvider.Next(-1f, 1f)));
			}
			foreach (var c in introWorldChars) {
				charVelocities.Add(c, new Vector2(RandomProvider.Next(-1f, 1f), RandomProvider.Next(-1f, 1f)));
			}

			foreach (var indicator in goalIndicatorCircles) {
				indicator.SetAlpha(0f);
			}
			foreach (var indicator in goalIndicatorArrows) {
				indicator.SetAlpha(0f);
			}
			if (spawnIndicatorCircle != null) spawnIndicatorCircle.SetAlpha(0f);
			if (spawnIndicatorArrow != null) spawnIndicatorArrow.SetAlpha(0f);
		}

		private static void HUD_TickPostIntro(float deltaTime) {
			postIntroHUDTimeElapsed += deltaTime;

			if (goTexture != null) {
				if (postIntroHUDTimeElapsed > GO_TEXTURE_LINGER_DURATION + GO_TEXTURE_FADE_OUT_DURATION) {
					goTexture.Dispose();
					goTexture = null;
				}
				else if (postIntroHUDTimeElapsed > GO_TEXTURE_LINGER_DURATION) {
					float goAlpha = Math.Max(0f, 1f - ((postIntroHUDTimeElapsed - GO_TEXTURE_LINGER_DURATION) / GO_TEXTURE_FADE_OUT_DURATION)) * READY_TEXTURE_MAX_ALPHA;
					goTexture.Color = new Vector4(1f, 1f, 1f, goAlpha);
					goTexture.Scale = new Vector2(goTexTargetScale.X * 1f + ((1f - goAlpha) * 1f), goAlpha * goTexTargetScale.Y);
					goTexture.AnchorOffset = new Vector2(goTexture.AnchorOffset, y: goTexture.AnchorOffset.Y + deltaTime * (0.135f / GO_TEXTURE_FADE_OUT_DURATION));
				}
			}

			if (lastIntroID == currentlyLoadedLevelID) return;

			if (postIntroHUDTimeElapsed >= CHAR_FADE_OUT_TIME) {
				if (charVelocities.Any()) {
					foreach (var c in charVelocities.Keys) c.Color = Vector4.ZERO;
					charVelocities.Clear();
				}
				lastIntroID = currentlyLoadedLevelID;
			}
			else {
				float charAlpha = WORLD_AND_LEVEL_LETTER_MAX_ALPHA * Math.Max(0f, 1f - (postIntroHUDTimeElapsed / CHAR_FADE_OUT_TIME));
				foreach (KeyValuePair<FontString, Vector2> kvp in charVelocities) {
					kvp.Key.Color = new Vector4(kvp.Key.Color, w: charAlpha);
					Vector2 actualVelo = kvp.Value + Vector2.UP * CHAR_GRAVITY * postIntroHUDTimeElapsed;
					kvp.Key.AnchorOffset = kvp.Key.AnchorOffset + actualVelo * deltaTime;
				}

				float diffAlpha = INTRO_DIFF_MAX_ALPHA * Math.Max(0f, 1f - ((postIntroHUDTimeElapsed * 2f) / CHAR_FADE_OUT_TIME));

				introDifficultyIconLeft.SetAlpha(diffAlpha);
				introDifficultyIconRight.SetAlpha(diffAlpha);
				introDifficultyString.SetAlpha(diffAlpha);
			}
		}

		private static void HUD_TickIndicators(float deltaTime) {
			if (AssetLocator.MainWindow.AddedViewports.Count == 0 || AssetLocator.MainWindow.AddedViewports[0].IsDisposed) return;

			bool windingDown = introductionCountdownCounter < GameplayConstants.INTRO_EGG_SPAWN_TIME + (1f / INDICATOR_FADE_IN_RATE);

			for (int i = 0; i < goalLocations.Count; ++i) {
				goalIndicatorCircles[i].AdjustAlpha(INDICATOR_FADE_IN_RATE * (windingDown ? -deltaTime : deltaTime));
				goalIndicatorArrows[i].AdjustAlpha(INDICATOR_FADE_IN_RATE * (windingDown ? -deltaTime : deltaTime));

				var topLeftOffset = AssetLocator.MainCamera.WorldToScreenNormalizedClipped(AssetLocator.MainWindow, goalLocations[i]);
				var centralized = new Vector2(topLeftOffset.X - 0.5f, topLeftOffset.Y - 0.5f);
				float angleAroundCenter;
				if (centralized == Vector2.ZERO) angleAroundCenter = 0f;
				else angleAroundCenter = (float) Math.Atan2(centralized.Y, centralized.X) + MathUtils.PI_OVER_TWO;

				Vector2 circleMargin = ((Vector2) (Vector3.DOWN * Quaternion.FromAxialRotation(Vector3.FORWARD, angleAroundCenter))).WithLength(-INTRO_INDICATOR_CIRCLE_MARGIN);
				circleMargin = new Vector2(-circleMargin.X, circleMargin.Y);

				goalIndicatorCircles[i].AnchorOffset = centralized + circleMargin;
				goalIndicatorArrows[i].Rotation = angleAroundCenter + MathUtils.PI;
				var arrowOffset = circleMargin * INTRO_INDICATOR_ARROW_MARGIN_FRAC;
				arrowOffset = new Vector2(arrowOffset.X, arrowOffset.Y / AssetLocator.MainWindow.AddedViewports[0].AspectRatio);
				goalIndicatorArrows[i].AnchorOffset = centralized + arrowOffset;
			}

			{
				spawnIndicatorCircle.AdjustAlpha(INDICATOR_FADE_IN_RATE * (windingDown ? -deltaTime : deltaTime));
				spawnIndicatorArrow.AdjustAlpha(INDICATOR_FADE_IN_RATE * (windingDown ? -deltaTime : deltaTime));

				var topLeftOffset = AssetLocator.MainCamera.WorldToScreenNormalizedClipped(AssetLocator.MainWindow, spawnLocation);
				var centralized = new Vector2(topLeftOffset.X - 0.5f, topLeftOffset.Y - 0.5f);
				float angleAroundCenter;
				if (centralized == Vector2.ZERO) angleAroundCenter = 0f;
				else angleAroundCenter = (float) Math.Atan2(centralized.Y, centralized.X) + MathUtils.PI_OVER_TWO;

				Vector2 circleMargin = ((Vector2) (Vector3.DOWN * Quaternion.FromAxialRotation(Vector3.FORWARD, angleAroundCenter))).WithLength(-INTRO_INDICATOR_CIRCLE_MARGIN);
				circleMargin = new Vector2(-circleMargin.X, circleMargin.Y);

				spawnIndicatorCircle.AnchorOffset = centralized + circleMargin;
				spawnIndicatorArrow.Rotation = angleAroundCenter + MathUtils.PI;
				var arrowOffset = circleMargin * INTRO_INDICATOR_ARROW_MARGIN_FRAC;
				arrowOffset = new Vector2(arrowOffset.X, arrowOffset.Y / AssetLocator.MainWindow.AddedViewports[0].AspectRatio);
				spawnIndicatorArrow.AnchorOffset = centralized + arrowOffset;
			}
		}
		#endregion

		#region HUD Setup / Disposal
		private static void SetUpHUD() {
			EnsureCachedTextureViewsAreLoaded();
			DisposeHUD();

			// kmhLabel
			kmhLabel = AssetLocator.MainFont.AddString(
					AssetLocator.HudLayer,
					AssetLocator.MainWindow,
					ViewportAnchoring.BottomLeft,
					new Vector2(0.0055f, 0.19f),
					Vector2.ONE * 0.5f
				);
			kmhLabel.Text = "km/h: 00";
			kmhLabel.Color = Vector4.ONE * 0.8f;

			// eggCounterBackdrop
			eggCounterBackdrop = new HUDTexture(
				AssetLocator.HUDFragmentShader,
				AssetLocator.HudLayer,
				AssetLocator.MainWindow
			) {
				Anchoring = ViewportAnchoring.BottomLeft,
				AnchorOffset = new Vector2(-0.01f, 0.015f),
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.None,
				Color = Vector4.ONE,
				Rotation = 0f,
				Scale = new Vector2(0.21f, 0.1f),
				Texture = AssetLocator.EggCounterBackdrop,
				ZIndex = 0
			};

			// vultureEggs + vultureEggCrosses
			for (int i = 0; i < 5; ++i) {
				vultureEggs[i] = new HUDTexture(
					AssetLocator.HUDFragmentShader,
					AssetLocator.HudLayer,
					AssetLocator.MainWindow
				) {
					Anchoring = ViewportAnchoring.BottomLeft,
					AnchorOffset = new Vector2(-0.002f + i * 0.033f, 0.029f),
					AspectCorrectionStrategy = eggCounterBackdrop.AspectCorrectionStrategy,
					Color = Vector4.ONE,
					Rotation = 0f,
					Scale = new Vector2(0.0395f, 0.0725f),
					Texture = (i < vultureEggEntities.Count ? AssetLocator.VultureEgg : AssetLocator.VultureEggDimmed),
					ZIndex = 1
				};

				if (i >= vultureEggEntities.Count) {
					vultureEggCrosses[i] = new HUDTexture(
						AssetLocator.HUDFragmentShader,
						AssetLocator.HudLayer,
						AssetLocator.MainWindow
					) {
						Anchoring = ViewportAnchoring.BottomLeft,
						AnchorOffset = vultureEggs[i].AnchorOffset,
						AspectCorrectionStrategy = eggCounterBackdrop.AspectCorrectionStrategy,
						Color = new Vector4(0.5f, 0.5f, 1f, 1f),
						Rotation = 0f,
						Scale = vultureEggs[i].Scale,
						Texture = AssetLocator.VultureEggCross,
						ZIndex = 2
					};
				}
				else vultureEggCrosses[i] = null;
			}

			// coinBackdrop
			coinBackdrop = new HUDTexture(
				AssetLocator.HUDFragmentShader,
				AssetLocator.HudLayer,
				AssetLocator.MainWindow
			) {
				Anchoring = ViewportAnchoring.BottomLeft,
				AnchorOffset = new Vector2(-0.05f, 0.1035f),
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.None,
				Color = Vector4.ONE,
				Rotation = 0f,
				Scale = new Vector2(0.2f, 0.081f),
				Texture = AssetLocator.CoinBackdrop,
				ZIndex = 0
			};

			// spinningCoin
			curCoinTexIndex = -1;
			spinningCoin = new HUDTexture(
				AssetLocator.HUDFragmentShader,
				AssetLocator.HudLayer,
				AssetLocator.MainWindow
			) {
				Anchoring = ViewportAnchoring.BottomLeft,
				AnchorOffset = new Vector2(0.1125f, 0.118f),
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = Vector4.ONE,
				Rotation = 0f,
				Scale = DEFAULT_COIN_SCALE,
				Texture = AssetLocator.CoinFrames[0],
				ZIndex = coinBackdrop.ZIndex + 1
			};

			// numCoinsString
			numCoinsString = AssetLocator.TitleFont.AddString(
					AssetLocator.HudLayer,
					AssetLocator.MainWindow,
					ViewportAnchoring.BottomLeft,
					new Vector2(0.0055f, 0.12f),
					Vector2.ONE * 0.6f * 0.75f
				);
			numCoinsString.Text = PersistedWorldData.GetTakenCoinIndices(currentlyLoadedLevelID).Count().ToString("00") + " / " + LevelDatabase.GetLevelTotalCoins(currentlyLoadedLevelID).ToString("00");
			numCoinsString.Color = new Vector4(1f, 0.6f, 0f, 1f);

			// timerBackdrop
			timerBackdrop = new HUDTexture(
				AssetLocator.HUDFragmentShader,
				AssetLocator.HudLayer,
				AssetLocator.MainWindow
			) {
				Anchoring = ViewportAnchoring.TopLeft,
				AnchorOffset = new Vector2(0f, 0f),
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.None,
				Color = Vector4.ONE,
				Rotation = 0f,
				Scale = new Vector2(0.25f, 0.3f),
				Texture = AssetLocator.TimerBackdrop,
				ZIndex = 0
			};

			// timerBackdropGlass
			timerBackdropGlass = new HUDTexture(
				AssetLocator.HUDFragmentShader,
				AssetLocator.HudLayer,
				AssetLocator.MainWindow
			) {
				Anchoring = ViewportAnchoring.TopLeft,
				AnchorOffset = new Vector2(0f, 0f),
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.None,
				Color = Vector4.ONE,
				Rotation = 0f,
				Scale = new Vector2(0.25f, 0.3f),
				Texture = AssetLocator.TimerBackdropGlass,
				ZIndex = 2
			};

			// timerFigures
			for (int i = 0; i < 4; ++i) {
				float offset = 0f;
				switch (i) {
					case 1: offset = 0.028f; break;
					case 2: offset = 0.066f; break;
					case 3: offset = 0.066f + 0.028f; break;
				}
				timerFigures[i] = new HUDTexture(
					AssetLocator.HUDFragmentShader,
					AssetLocator.HudLayer,
					AssetLocator.MainWindow
				) {
					Anchoring = ViewportAnchoring.TopLeft,
					AnchorOffset = new Vector2(0.064f + offset, 0.108f),
					AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.None,
					Color = Vector4.ONE,
					Rotation = 0f,
					Scale = new Vector2(0.029f, 0.085f),
					Texture = AssetLocator.TimerFigures[0],
					ZIndex = 1
				};
			}

			// starTimeBackdrops + starTimeStars + starTimeStrings
			for (int i = 0; i < 3; ++i) {
				ITexture2D backdropTexture, starTexture;
				Vector4 starTimeColor;
				string starTimeString;
				switch (i) {
					case 0:
						backdropTexture = AssetLocator.BronzeStarTimeBackdrop;
						starTexture = AssetLocator.BronzeStar;
						starTimeColor = new Vector4(0.85f, 0.64f, 0.33f, 0.9f);
						starTimeString = MakeTimeString(CurrentlyLoadedLevel.LevelTimerMaxMs - CurrentlyLoadedLevel.LevelTimerBronzeMs);
						break;
					case 1:
						backdropTexture = AssetLocator.SilverStarTimeBackdrop;
						starTexture = AssetLocator.SilverStar;
						starTimeColor = new Vector4(0.74f, 0.77f, 0.8f, 0.9f);
						starTimeString = MakeTimeString(CurrentlyLoadedLevel.LevelTimerMaxMs - CurrentlyLoadedLevel.LevelTimerSilverMs);
						break;
					default:
						backdropTexture = AssetLocator.GoldStarTimeBackdrop;
						starTexture = AssetLocator.GoldStar;
						starTimeColor = new Vector4(0.84f, 0.74f, 0.48f, 0.9f);
						starTimeString = MakeTimeString(CurrentlyLoadedLevel.LevelTimerMaxMs - CurrentlyLoadedLevel.LevelTimerGoldMs);
						break;
				}
				starTimeBackdrops[i] = new HUDTexture(
					AssetLocator.HUDFragmentShader,
					AssetLocator.HudLayer,
					AssetLocator.MainWindow
				) {
					Anchoring = ViewportAnchoring.TopLeft,
					AnchorOffset = new Vector2(0.0375f, 0.1825f + i * 0.0815f),
					AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.None,
					Color = Vector4.ONE,
					Rotation = 0f,
					Scale = new Vector2(0.175f, 0.2f),
					Texture = backdropTexture,
					ZIndex = i
				};
				starTimeStars[i] = new HUDTexture(
					AssetLocator.HUDFragmentShader,
					AssetLocator.HudLayer,
					AssetLocator.MainWindow
				) {
					Anchoring = ViewportAnchoring.TopLeft,
					AnchorOffset = new Vector2(0.0555f, 0.246f + i * 0.079f),
					AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.None,
					Color = Vector4.ONE,
					Rotation = 0f,
					Scale = new Vector2(0.0376f, 0.0717f),
					Texture = starTexture,
					ZIndex = i + 1
				};
				starTimeStrings[i] = AssetLocator.TitleFont.AddString(
					AssetLocator.HudLayer,
					AssetLocator.MainWindow,
					ViewportAnchoring.TopLeft,
					starTimeStars[i].AnchorOffset + new Vector2(0.042f, 0.008f + 0.001f * i),
					Vector2.ONE * 0.5f
				);
				starTimeStrings[i].Text = starTimeString;
				starTimeStrings[i].Color = starTimeColor;
			}

			// worldNameString
			worldNameString = AssetLocator.TitleFont.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.BottomRight,
				new Vector2(WORLD_NAME_STRING_X_BUFFER, 0.1075f),
				Vector2.ONE * 0.4f
			);
			worldNameString.Text = LevelDatabase.GetWorldName(CurrentlyLoadedLevelID.WorldIndex);
			//worldNameString.Color = new Vector4(0.84f, 0.705f, 0f, 0.75f);
			worldNameString.Color = new Vector4(Vector3.ONE * 0.5f + LevelDatabase.GetWorldColor(currentlyLoadedLevelID.WorldIndex) * 0.5f, w: 0.9f);

			// worldNameBackdrop
			var worldNameStringSizeX = worldNameString.Dimensions.X + WORLD_NAME_STRING_X_BUFFER * 2f + WORLD_NAME_BACKDROP_ICON_BUFFER;
			var worldNameBackdropScaleX = Math.Max(WORLD_NAME_BACKDROP_MIN_X_SCALE, worldNameStringSizeX);
			var worldNameBackdropOffsetX = worldNameBackdropScaleX > WORLD_NAME_BACKDROP_MIN_X_SCALE ?
				-0.01f :
				-0.01f - (WORLD_NAME_BACKDROP_MIN_X_SCALE - worldNameStringSizeX);
			worldNameBackdrop = new HUDTexture(
				AssetLocator.HUDFragmentShader,
				AssetLocator.HudLayer,
				AssetLocator.MainWindow
			) {
				Anchoring = ViewportAnchoring.BottomRight,
				AnchorOffset = new Vector2(worldNameBackdropOffsetX, 0.09f),
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.None,
				Color = Vector4.ONE,
				Rotation = 0f,
				Scale = new Vector2(worldNameBackdropScaleX, 0.081f),
				Texture = AssetLocator.LevelNameBackdrop,
				ZIndex = 0
			};

			// worldIcon 
			worldIcon = new HUDTexture(
				AssetLocator.HUDFragmentShader,
				AssetLocator.HudLayer,
				AssetLocator.MainWindow
			) {
				Anchoring = ViewportAnchoring.BottomRight,
				AnchorOffset = new Vector2(
					worldNameStringSizeX - (WORLD_NAME_STRING_X_BUFFER + WORLD_NAME_BACKDROP_ICON_BUFFER) - 0.0036f,
					worldNameBackdrop.AnchorOffset.Y + 0.0085f
				),
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.None,
				Color = new Vector4(1f, 1f, 1f, 0.825f),
				Rotation = 0f,
				Scale = new Vector2(0.05f, 0.077f) * 0.85f,
				Texture = AssetLocator.WorldIcons[currentlyLoadedLevelID.WorldIndex],
				ZIndex = 3
			};

			// levelNameString
			levelNameString = AssetLocator.TitleFont.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.BottomRight,
				new Vector2(0.003f, 0.032f),
				Vector2.ONE * 0.46f
			);
			levelNameString.Text = CurrentlyLoadedLevel.Title;
			levelNameString.Color = new Vector4(0.72f, 1f, 0f, 0.9f);

			// levelNumberString
			var levelNameStringSizeX = levelNameString.Dimensions.X + LEVEL_NAME_STRING_X_BUFFER * 2f;
			levelNumberString = AssetLocator.TitleFont.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.BottomRight,
				new Vector2(levelNameStringSizeX - (LEVEL_NAME_STRING_X_BUFFER + 0.0035f), 0.0375f),
				Vector2.ONE * 0.4f
			);
			levelNumberString.Text = (CurrentlyLoadedLevelID.WorldIndex + 1) + "-" + (CurrentlyLoadedLevelID.LevelIndex + 1);
			levelNumberString.Color = new Vector4(0.56f, 0.65f, 0f, 0.825f);

			// levelNameBackdrop 
			var levelNameAndNumberStringSizeX = levelNameStringSizeX + levelNumberString.Dimensions.X;
			var levelNameBackdropScaleX = Math.Max(LEVEL_NAME_BACKDROP_MIN_X_SCALE, levelNameAndNumberStringSizeX);
			var levelNameBackdropOffsetX = levelNameBackdropScaleX > LEVEL_NAME_BACKDROP_MIN_X_SCALE ?
				0f :
				0f - (LEVEL_NAME_BACKDROP_MIN_X_SCALE - levelNameAndNumberStringSizeX);
			levelNameBackdrop = new HUDTexture(
				AssetLocator.HUDFragmentShader,
				AssetLocator.HudLayer,
				AssetLocator.MainWindow
			) {
				Anchoring = ViewportAnchoring.BottomRight,
				AnchorOffset = new Vector2(levelNameBackdropOffsetX, 0.012f),
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.None,
				Color = Vector4.ONE,
				Rotation = 0f,
				Scale = new Vector2(levelNameBackdropScaleX, 0.09f),
				Texture = AssetLocator.LevelNameBackdrop,
				ZIndex = 0
			};


			// personalBestTimeBackdrop
			personalBestTimeBackdrop = new HUDTexture(
				AssetLocator.HUDFragmentShader,
				AssetLocator.HudLayer,
				AssetLocator.MainWindow
			) {
				Anchoring = ViewportAnchoring.TopRight,
				AnchorOffset = new Vector2(0.006f, 0.01f),
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.None,
				Color = Vector4.ONE,
				Rotation = 0f,
				Scale = new Vector2(0.175f, 0.095f),
				Texture = AssetLocator.PersonalBestTimeBackdrop,
				ZIndex = 0
			};

			// personalBestBell
			personalBestBell = new HUDTexture(
				AssetLocator.HUDFragmentShader,
				AssetLocator.HudLayer,
				AssetLocator.MainWindow
			) {
				Anchoring = ViewportAnchoring.TopRight,
				AnchorOffset = personalBestTimeBackdrop.AnchorOffset + new Vector2(0.1258f, 0.0245f),
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.None,
				Color = Vector4.ONE,
				Rotation = 0f,
				Scale = new Vector2(0.03f, 0.049f),
				Texture = AssetLocator.FinishingBell,
				ZIndex = 1
			};

			// personalBestString
			personalBestString = AssetLocator.TitleFont.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.TopRight,
				personalBestTimeBackdrop.AnchorOffset + new Vector2(0.026f, 0.0175f),
				Vector2.ONE * 0.5f
			);
			personalBestString.Text =
				currentLevelLeaderboardDetails.PersonalBestTimeMs == null
				? "--:--"
				: MakeTimeString(currentLevelLeaderboardDetails.PersonalBestTimeMs.Value);
			personalBestString.Color = new Vector4(224f / 255f, 201f / 255f, 127f / 255f, 1f);

			if (currentLevelLeaderboardDetails.GoldFriend != null) {
				// goldFriendsTimeBackdrop
				goldFriendsTimeBackdrop = new HUDTexture(
					AssetLocator.HUDFragmentShader,
					AssetLocator.HudLayer,
					AssetLocator.MainWindow
				) {
					Anchoring = ViewportAnchoring.TopRight,
					AnchorOffset = personalBestTimeBackdrop.AnchorOffset + new Vector2(0f, 0.0705f),
					AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.None,
					Color = Vector4.ONE,
					Rotation = 0f,
					Scale = personalBestTimeBackdrop.Scale,
					Texture = AssetLocator.FriendsGoldTimeBackdrop,
					ZIndex = 0
				};

				// goldFriendsToken
				goldFriendsToken = new HUDTexture(
					AssetLocator.HUDFragmentShader,
					AssetLocator.HudLayer,
					AssetLocator.MainWindow
				) {
					Anchoring = ViewportAnchoring.TopRight,
					AnchorOffset = goldFriendsTimeBackdrop.AnchorOffset + new Vector2(0.02f, 0.02f),
					AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.None,
					Color = Vector4.ONE,
					Rotation = 0f,
					Scale = new Vector2(0.03f, 0.052f),
					Texture = AssetLocator.FriendsGoldTimeToken,
					ZIndex = 1
				};

				// goldFriendTimeString
				goldFriendTimeString = AssetLocator.MainFont.AddString(
					AssetLocator.HudLayer,
					AssetLocator.MainWindow,
					ViewportAnchoring.TopRight,
					goldFriendsTimeBackdrop.AnchorOffset + new Vector2(0.057f, -0.005f),
					Vector2.ONE * 0.5f
				);
				goldFriendTimeString.Text = MakeTimeString(currentLevelLeaderboardDetails.GoldFriendTimeMs.Value);
				goldFriendTimeString.Color = new Vector4(255f / 255f, 210f / 255f, 79f / 255f, 0.9f);

				// goldFriendNameString
				goldFriendNameString = AssetLocator.MainFont.AddString(
					AssetLocator.HudLayer,
					AssetLocator.MainWindow,
					ViewportAnchoring.TopRight,
					goldFriendsTimeBackdrop.AnchorOffset + new Vector2(0.057f, 0.034f),
					Vector2.ONE * 0.35f
				);
				goldFriendNameString.Text = currentLevelLeaderboardDetails.GoldFriend.Value.Name.WithMaxLength(10);
				goldFriendNameString.Color = new Vector4(0.75f, 0.65f, 0.42f, 0.9f);
			}

			if (currentLevelLeaderboardDetails.SilverFriend != null) {
				// silverFriendsTimeBackdrop
				silverFriendsTimeBackdrop = new HUDTexture(
					AssetLocator.HUDFragmentShader,
					AssetLocator.HudLayer,
					AssetLocator.MainWindow
				) {
					Anchoring = ViewportAnchoring.TopRight,
					AnchorOffset = personalBestTimeBackdrop.AnchorOffset + new Vector2(0f, 0.0705f * 2f),
					AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.None,
					Color = Vector4.ONE,
					Rotation = 0f,
					Scale = personalBestTimeBackdrop.Scale,
					Texture = AssetLocator.FriendsSilverTimeBackdrop,
					ZIndex = 0
				};

				// silverFriendsToken
				silverFriendsToken = new HUDTexture(
					AssetLocator.HUDFragmentShader,
					AssetLocator.HudLayer,
					AssetLocator.MainWindow
				) {
					Anchoring = ViewportAnchoring.TopRight,
					AnchorOffset = silverFriendsTimeBackdrop.AnchorOffset + new Vector2(0.02f, 0.02f),
					AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.None,
					Color = Vector4.ONE,
					Rotation = 0f,
					Scale = goldFriendsToken.Scale,
					Texture = AssetLocator.FriendsSilverTimeToken,
					ZIndex = 1
				};

				// silverFriendTimeString
				silverFriendTimeString = AssetLocator.MainFont.AddString(
					AssetLocator.HudLayer,
					AssetLocator.MainWindow,
					ViewportAnchoring.TopRight,
					silverFriendsTimeBackdrop.AnchorOffset + new Vector2(0.057f, -0.005f),
					Vector2.ONE * 0.5f
					);
				silverFriendTimeString.Text = MakeTimeString(currentLevelLeaderboardDetails.SilverFriendTimeMs.Value);
				silverFriendTimeString.Color = new Vector4(204f / 255f, 204f / 255f, 204f / 255f, 0.9f);

				// silverFriendNameString
				silverFriendNameString = AssetLocator.MainFont.AddString(
					AssetLocator.HudLayer,
					AssetLocator.MainWindow,
					ViewportAnchoring.TopRight,
					silverFriendsTimeBackdrop.AnchorOffset + new Vector2(0.057f, 0.034f),
					Vector2.ONE * 0.35f
				);
				silverFriendNameString.Text = currentLevelLeaderboardDetails.SilverFriend.Value.Name.WithMaxLength(10);
				silverFriendNameString.Color = new Vector4(0.65f, 0.65f, 0.52f, 0.9f);
			}

			if (currentLevelLeaderboardDetails.BronzeFriend != null) {
				// bronzeFriendsTimeBackdrop
				bronzeFriendsTimeBackdrop = new HUDTexture(
					AssetLocator.HUDFragmentShader,
					AssetLocator.HudLayer,
					AssetLocator.MainWindow
				) {
					Anchoring = ViewportAnchoring.TopRight,
					AnchorOffset = personalBestTimeBackdrop.AnchorOffset + new Vector2(0f, 0.0705f * 3f),
					AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.None,
					Color = Vector4.ONE,
					Rotation = 0f,
					Scale = personalBestTimeBackdrop.Scale,
					Texture = AssetLocator.FriendsBronzeTimeBackdrop,
					ZIndex = 0
				};

				// bronzeFriendsToken
				bronzeFriendsToken = new HUDTexture(
					AssetLocator.HUDFragmentShader,
					AssetLocator.HudLayer,
					AssetLocator.MainWindow
				) {
					Anchoring = ViewportAnchoring.TopRight,
					AnchorOffset = bronzeFriendsTimeBackdrop.AnchorOffset + new Vector2(0.02f, 0.02f),
					AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.None,
					Color = Vector4.ONE,
					Rotation = 0f,
					Scale = goldFriendsToken.Scale,
					Texture = AssetLocator.FriendsBronzeTimeToken,
					ZIndex = 1
				};

				// bronzeFriendTimeString
				bronzeFriendTimeString = AssetLocator.MainFont.AddString(
					AssetLocator.HudLayer,
					AssetLocator.MainWindow,
					ViewportAnchoring.TopRight,
					bronzeFriendsTimeBackdrop.AnchorOffset + new Vector2(0.057f, -0.005f),
					Vector2.ONE * 0.5f
					);
				bronzeFriendTimeString.Text = MakeTimeString(currentLevelLeaderboardDetails.BronzeFriendTimeMs.Value);
				bronzeFriendTimeString.Color = new Vector4(255f / 255f, 169f / 255f, 79f / 255f, 0.9f);

				// bronzeFriendNameString
				bronzeFriendNameString = AssetLocator.MainFont.AddString(
					AssetLocator.HudLayer,
					AssetLocator.MainWindow,
					ViewportAnchoring.TopRight,
					bronzeFriendsTimeBackdrop.AnchorOffset + new Vector2(0.057f, 0.034f),
					Vector2.ONE * 0.35f
				);
				bronzeFriendNameString.Text = currentLevelLeaderboardDetails.BronzeFriend.Value.Name.WithMaxLength(10);
				bronzeFriendNameString.Color = new Vector4(0.75f, 0.55f, 0.32f, 0.9f);
			}

			starTimeStarVelocities[0] = starTimeStarVelocities[1] = starTimeStarVelocities[2] = INITIAL_STAR_VELOCITY;

			for (int i = 0; i < currentGameLevel.NumHops; ++i) {
				eggHopTextures.Add(new HUDTexture(
					AssetLocator.HUDFragmentShader,
					AssetLocator.HudLayer,
					AssetLocator.MainWindow
				) {
					Anchoring = ViewportAnchoring.BottomRight,
					AnchorOffset = new Vector2(i * 0.045f - 0.0125f, 0.195f),
					AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.None,
					Color = new Vector4(232f, 175f, 90f, 0f) / 255f,
					Rotation = 0f,
					Scale = EGG_HOP_TEX_TARGET_SCALE,
					Texture = AssetLocator.HopArrow,
					ZIndex = 0
				});

				eggHopExciters.Add(
					new HUDItemExciterEntity(eggHopTextures[i]) {
						CountPerSec = 20,
						OpacityMultiplier = 0.1f,
						Lifetime = 1f
					}
				);
			}

			if (PersistedWorldData.GoldenEggTakenForLevel(currentlyLoadedLevelID)) {
				var newTex = new HUDTexture(
					AssetLocator.HUDFragmentShader,
					AssetLocator.HudLayer,
					AssetLocator.MainWindow
				) {
					Anchoring = ViewportAnchoring.BottomLeft,
					AnchorOffset = new Vector2(0.16f, 0.016f),
					AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
					Color = HUD_TICK_COLOR,
					Rotation = 0f,
					Scale = HUD_TICK_SCALE * 1.5f,
					Texture = AssetLocator.Tick,
					ZIndex = 10
				};

				tickTextures.Add(newTex);
			}

			if (PersistedWorldData.GetTakenCoinIndices(currentlyLoadedLevelID).Count() == LevelDatabase.GetLevelTotalCoins(currentlyLoadedLevelID)) {
				var newTex = new HUDTexture(
					AssetLocator.HUDFragmentShader,
					AssetLocator.HudLayer,
					AssetLocator.MainWindow
				) {
					Anchoring = ViewportAnchoring.BottomLeft,
					AnchorOffset = new Vector2(0.12f, 0.105f),
					AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
					Color = HUD_TICK_COLOR,
					Rotation = 0f,
					Scale = HUD_TICK_SCALE * 1.15f,
					Texture = AssetLocator.Tick,
					ZIndex = 10
				};

				tickTextures.Add(newTex);
			}

			var star = PersistedWorldData.GetStarForLevel(currentlyLoadedLevelID);
			const float STAR_TICK_X = 0.025f;
			switch (star) {
				case Star.Gold: {
						goldTick = new HUDTexture(
							AssetLocator.HUDFragmentShader,
							AssetLocator.HudLayer,
							AssetLocator.MainWindow
						) {
							Anchoring = ViewportAnchoring.TopLeft,
							AnchorOffset = new Vector2(STAR_TICK_X, 0.4f),
							AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
							Color = HUD_TICK_COLOR,
							Rotation = 0f,
							Scale = HUD_TICK_SCALE * 1.15f,
							Texture = AssetLocator.Tick,
							ZIndex = 10
						};

						tickTextures.Add(goldTick);
						goto case Star.Silver;
					}
				case Star.Silver: {
						silverTick = new HUDTexture(
							AssetLocator.HUDFragmentShader,
							AssetLocator.HudLayer,
							AssetLocator.MainWindow
						) {
							Anchoring = ViewportAnchoring.TopLeft,
							AnchorOffset = new Vector2(STAR_TICK_X, 0.32f),
							AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
							Color = HUD_TICK_COLOR,
							Rotation = 0f,
							Scale = HUD_TICK_SCALE * 1.15f,
							Texture = AssetLocator.Tick,
							ZIndex = 10
						};

						tickTextures.Add(silverTick);
						goto case Star.Bronze;
					}
				case Star.Bronze: {
						bronzeTick = new HUDTexture(
							AssetLocator.HUDFragmentShader,
							AssetLocator.HudLayer,
							AssetLocator.MainWindow
						) {
							Anchoring = ViewportAnchoring.TopLeft,
							AnchorOffset = new Vector2(STAR_TICK_X, 0.24f),
							AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
							Color = HUD_TICK_COLOR,
							Rotation = 0f,
							Scale = HUD_TICK_SCALE * 1.15f,
							Texture = AssetLocator.Tick,
							ZIndex = 10
						};

						tickTextures.Add(bronzeTick);
						break;
					}
			}

			// difficultyIcon & difficultyString
			difficultyIcon = new HUDTexture(
				AssetLocator.HUDFragmentShader,
				AssetLocator.HudLayer,
				AssetLocator.MainWindow
			) {
				Anchoring = ViewportAnchoring.BottomRight,
				AnchorOffset = worldNameString.AnchorOffset + new Vector2(-0.005f, 0.055f),
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = new Vector4(LevelDatabase.GetLevelDifficulty(CurrentlyLoadedLevelID).Color(), w: 0.7f),
				Rotation = 0f,
				Scale = Vector2.ONE * 0.035f,
				Texture = LevelDatabase.GetLevelDifficulty(CurrentlyLoadedLevelID).Tex(),
				ZIndex = 3
			};

			difficultyString = AssetLocator.TitleFont.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.BottomRight,
				difficultyIcon.AnchorOffset + new Vector2(0.0315f, 0.007f),
				Vector2.ONE * 0.32f
			);
			difficultyString.Text = LevelDatabase.GetLevelDifficulty(CurrentlyLoadedLevelID).FriendlyName();
			difficultyString.Color = new Vector4(LevelDatabase.GetLevelDifficulty(CurrentlyLoadedLevelID).Color(), w: 0.7f);
		}

		private static void DisposeHUD(bool includePassFailElements = true) {
			ClearHUDIntroElements();

			if (kmhLabel != null) {
				kmhLabel.Dispose();
				kmhLabel = null;
			}
			if (eggCounterBackdrop != null) {
				eggCounterBackdrop.Dispose();
				eggCounterBackdrop = null;
			}
			for (int i = 0; i < 5; ++i) {
				if (vultureEggs[i] != null) {
					vultureEggs[i].Dispose();
					vultureEggs[i] = null;
				}
				if (vultureEggCrosses[i] != null) {
					vultureEggCrosses[i].Dispose();
					vultureEggCrosses[i] = null;
				}
			}

			if (coinBackdrop != null) {
				coinBackdrop.Dispose();
				coinBackdrop = null;
			}
			if (spinningCoin != null) {
				spinningCoin.Dispose();
				spinningCoin = null;
			}
			if (numCoinsString != null) {
				numCoinsString.Dispose();
				numCoinsString = null;
			}

			if (timerBackdrop != null) {
				timerBackdrop.Dispose();
				timerBackdrop = null;
			}
			if (timerBackdropGlass != null) {
				timerBackdropGlass.Dispose();
				timerBackdropGlass = null;
			}

			for (int i = 0; i < 4; ++i) {
				if (timerFigures[i] != null) {
					timerFigures[i].Dispose();
					timerFigures[i] = null;
				}
			}

			for (int i = 0; i < 3; ++i) {
				if (starTimeBackdrops[i] != null) {
					starTimeBackdrops[i].Dispose();
					starTimeBackdrops[i] = null;
				}
				if (starTimeStars[i] != null) {
					starTimeStars[i].Dispose();
					starTimeStars[i] = null;
				}
				if (starTimeStrings[i] != null) {
					starTimeStrings[i].Dispose();
					starTimeStrings[i] = null;
				}
			}

			if (worldNameBackdrop != null) {
				worldNameBackdrop.Dispose();
				worldNameBackdrop = null;
			}
			if (worldNameString != null) {
				worldNameString.Dispose();
				worldNameString = null;
			}
			if (worldIcon != null) {
				worldIcon.Dispose();
				worldIcon = null;
			}
			if (levelNameBackdrop != null) {
				levelNameBackdrop.Dispose();
				levelNameBackdrop = null;
			}
			if (levelNameString != null) {
				levelNameString.Dispose();
				levelNameString = null;
			}
			if (levelNumberString != null) {
				levelNumberString.Dispose();
				levelNumberString = null;
			}

			if (personalBestTimeBackdrop != null) {
				personalBestTimeBackdrop.Dispose();
				personalBestTimeBackdrop = null;
			}
			if (goldFriendsTimeBackdrop != null) {
				goldFriendsTimeBackdrop.Dispose();
				goldFriendsTimeBackdrop = null;
			}
			if (silverFriendsTimeBackdrop != null) {
				silverFriendsTimeBackdrop.Dispose();
				silverFriendsTimeBackdrop = null;
			}
			if (bronzeFriendsTimeBackdrop != null) {
				bronzeFriendsTimeBackdrop.Dispose();
				bronzeFriendsTimeBackdrop = null;
			}

			if (personalBestBell != null) {
				personalBestBell.Dispose();
				personalBestBell = null;
			}
			if (goldFriendsToken != null) {
				goldFriendsToken.Dispose();
				goldFriendsToken = null;
			}
			if (silverFriendsToken != null) {
				silverFriendsToken.Dispose();
				silverFriendsToken = null;
			}
			if (bronzeFriendsToken != null) {
				bronzeFriendsToken.Dispose();
				bronzeFriendsToken = null;
			}

			if (personalBestString != null) {
				personalBestString.Dispose();
				personalBestString = null;
			}
			if (goldFriendTimeString != null) {
				goldFriendTimeString.Dispose();
				goldFriendTimeString = null;
			}
			if (silverFriendTimeString != null) {
				silverFriendTimeString.Dispose();
				silverFriendTimeString = null;
			}
			if (bronzeFriendTimeString != null) {
				bronzeFriendTimeString.Dispose();
				bronzeFriendTimeString = null;
			}

			if (goldFriendNameString != null) {
				goldFriendNameString.Dispose();
				goldFriendNameString = null;
			}
			if (silverFriendNameString != null) {
				silverFriendNameString.Dispose();
				silverFriendNameString = null;
			}
			if (bronzeFriendNameString != null) {
				bronzeFriendNameString.Dispose();
				bronzeFriendNameString = null;
			}

			if (includePassFailElements) {
				DisposePassHUD();
				DisposeFailHUD();
			}

			for (int i = 0; i < eggHopTextures.Count; ++i) {
				eggHopTextures[i].Dispose();
			}
			eggHopTextures.Clear();

			for (int i = 0; i < eggHopExciters.Count; ++i) {
				eggHopExciters[i].Dispose();
			}
			eggHopExciters.Clear();

			foreach (var tick in tickTextures) {
				tick.Dispose();
			}
			tickTextures.Clear();

			goldTick = silverTick = bronzeTick = null;

			if (difficultyIcon != null) {
				difficultyIcon.Dispose();
				difficultyIcon = null;
			}
			if (difficultyString != null) {
				difficultyString.Dispose();
				difficultyString = null;
			}
		}

		private static void EnsureCachedTextureViewsAreLoaded() {
			if (cachedTimerFigures[0] == null) {
				for (int i = 0; i < 10; ++i) {
					for (int j = 1; j <= 15; ++j) {
						var index = i * 15 + (j - 1);
						cachedTimerFigures[index] = AssetLocator.TimerFigures[index].CreateView();
					}
				}
			}
		}
		#endregion

		#region HUD Tick
		private static unsafe void TickHUD(float deltaTime) {
			if (tutorialPanelActive) TickTutorialHUD(deltaTime);

			if (currentGameState == OverallGameState.LevelPassed) {
				TickHUD_Passed(deltaTime);
				return;
			}
			else if (currentGameState == OverallGameState.LevelFailed) {
				TickHUD_Failed(deltaTime);
				return;
			}

			// hops
			if (currentGameState != OverallGameState.LevelIntroduction) {
				if (hopActive && !usedDoubleHop && eggHopTextures[numHopsRemaining].Color != EGG_HOP_TEX_USE_COLOR) {
					eggHopTextures[numHopsRemaining].Color = EGG_HOP_TEX_USE_COLOR;
					eggHopTextures[numHopsRemaining].Scale = EGG_HOP_TEX_USE_TARGET_SCALE * 2f;
				}
				for (int i = 0; i < eggHopTextures.Count; ++i) {
					if (i > numHopsRemaining || (i == numHopsRemaining && (!hopActive || usedDoubleHop))) {
						eggHopTextures[i].AdjustAlpha(-deltaTime * 5f);
					}

					if (eggHopTextures[i].Scale.LengthSquared > EGG_HOP_TEX_USE_TARGET_SCALE.LengthSquared) {
						eggHopTextures[i].Scale = eggHopTextures[i].Scale * (1f - 2.5f * deltaTime);
						if (eggHopTextures[i].Scale.LengthSquared <= EGG_HOP_TEX_USE_TARGET_SCALE.LengthSquared) eggHopTextures[i].Scale = EGG_HOP_TEX_USE_TARGET_SCALE;
					}
				}
			}

			// kmh label
			if (currentGameState == OverallGameState.LevelPlaying) {
				float metresPerSec = egg.TrueVelocity.Length / PhysicsManager.ONE_METRE_SCALED;
				float metresPerHour = metresPerSec * 60f * 60f;
				float kilometresPerHour = metresPerHour / 1000f;
				kmhLabel.Text = "km/h: " + kilometresPerHour.ToString("00");
				if (kilometresPerHour <= 20f) kmhLabel.Color = new Vector4(0.8f, 0.8f, 0.8f - kilometresPerHour * (0.8f / 20f), 0.8f);
				else if (kilometresPerHour <= 40f) kmhLabel.Color = new Vector4(0.8f, 0.8f - (kilometresPerHour - 20f) * (0.8f / 20f), 0.0f, 0.8f);
				else kmhLabel.Color = new Vector4(0.8f, 0.0f, (kilometresPerHour - 40f) * (0.5f / 20f), 0.8f);
			}
			else {
				kmhLabel.Text = "km/h: 00";
			}

			// vulture eggs
			int numVultureEggs = vultureEggEntities.Count;
			int numVultureEggsDestroyed = 0;
			foreach (var kvp in vultureEggEntities) {
				if (kvp.Key.Transform.Scale == Vector3.ZERO) ++numVultureEggsDestroyed;
			}
			for (int i = 0; i < numVultureEggsDestroyed; ++i) {
				int textureIndex = (numVultureEggs - 1) - i;
				if (vultureEggCrosses[textureIndex] == null) {
					vultureEggCrosses[textureIndex] = new HUDTexture(
						AssetLocator.HUDFragmentShader,
						AssetLocator.HudLayer,
						AssetLocator.MainWindow
					) {
						Anchoring = ViewportAnchoring.BottomLeft,
						AnchorOffset = vultureEggs[textureIndex].AnchorOffset,
						AspectCorrectionStrategy = eggCounterBackdrop.AspectCorrectionStrategy,
						Color = Vector4.ONE,
						Rotation = 0f,
						Scale = vultureEggs[textureIndex].Scale * 2f,
						Texture = AssetLocator.VultureEggCross,
						ZIndex = 2
					};
				}
				else if (vultureEggCrosses[textureIndex].Scale > vultureEggs[textureIndex].Scale) {
					vultureEggCrosses[textureIndex].Scale *= 1f - (2.2f * deltaTime);
					if (vultureEggCrosses[textureIndex].Scale < vultureEggs[textureIndex].Scale) {
						vultureEggCrosses[textureIndex].Scale = vultureEggs[textureIndex].Scale;
					}
				}
			}

			// spinning coin
			if (curCoinTexIndex >= 0) {
				timeOnCurrentCoinFrame += deltaTime;
				while (timeOnCurrentCoinFrame >= TIME_PER_COIN_FRAME) {
					++curCoinTexIndex;
					spinningCoin.Texture = AssetLocator.CoinFrames[curCoinTexIndex & 31];
					spinningCoin.Scale = DEFAULT_COIN_SCALE * (1f + (32 - Math.Abs(32 - curCoinTexIndex)) / 16f);
					timeOnCurrentCoinFrame -= TIME_PER_COIN_FRAME;
				}
				if (curCoinTexIndex >= 64) {
					spinningCoin.Texture = AssetLocator.CoinFrames[0];
					spinningCoin.Scale = DEFAULT_COIN_SCALE;
					curCoinTexIndex = -1;
				}
			}

			// timer figures
			int fullSecondsRemaining = timeRemainingMs / 1000;
			int fullSecondsTensRemaining = fullSecondsRemaining / 10;
			int fullSecondsUnitsRemaining = fullSecondsRemaining - fullSecondsTensRemaining * 10;
			int remainingMillisRemaining = timeRemainingMs - (fullSecondsRemaining * 1000);
			int fullTenthsRemaining = remainingMillisRemaining / 100;
			int fullHundredthsRemaining = (remainingMillisRemaining - (fullTenthsRemaining * 100)) / 10;

			float distanceToNextUnitSecond = 0f;
			float distanceToNextTensSecond = 0f;
			float distanceToNextTenth = 0f;
			float distanceToNextHundredth = 0f;
			if (currentGameState == OverallGameState.LevelPlaying) {
				if (fullSecondsRemaining > 0) {
					if (fullTenthsRemaining < 5) {
						distanceToNextUnitSecond = 1f - (0.2f * fullTenthsRemaining + 0.02f * fullHundredthsRemaining);
						if (fullSecondsUnitsRemaining == 0) distanceToNextTensSecond = distanceToNextUnitSecond;
					}

					distanceToNextTenth = 1f - 0.1f * fullHundredthsRemaining;
					distanceToNextHundredth = 1f - 0.1f * (timeRemainingMs % 10);
				}
			}

			timerFigures[0].SetTextureFromCachedView(cachedTimerFigures[AssetLocator.GetTimerFigureIndex(fullSecondsTensRemaining, distanceToNextTensSecond)]);
			timerFigures[1].SetTextureFromCachedView(cachedTimerFigures[AssetLocator.GetTimerFigureIndex(fullSecondsUnitsRemaining, distanceToNextUnitSecond)]);
			timerFigures[2].SetTextureFromCachedView(cachedTimerFigures[AssetLocator.GetTimerFigureIndex(fullTenthsRemaining, distanceToNextTenth)]);
			timerFigures[3].SetTextureFromCachedView(cachedTimerFigures[AssetLocator.GetTimerFigureIndex(fullHundredthsRemaining, distanceToNextHundredth)]);


			// timer glass
			if (timeRemainingMs < Config.TimeWarningMs) {
				float nonRedComponent = (float) Math.Abs(Math.Cos((Config.TimeWarningMs - timeRemainingMs) / 200f));
				timerBackdropGlass.Color = new Vector4(1f, nonRedComponent, nonRedComponent, 1f);
			}

			// dropped stars
			const float STAR_GRAVITY = 1f;
			const int ANIMATION_CUTOFF_TIME = -6000;
			int* starTimesRemainingMs = stackalloc int[3];
			starTimesRemainingMs[0] = timeRemainingMs - (CurrentlyLoadedLevel.LevelTimerMaxMs - CurrentlyLoadedLevel.LevelTimerBronzeMs);
			starTimesRemainingMs[1] = timeRemainingMs - (CurrentlyLoadedLevel.LevelTimerMaxMs - CurrentlyLoadedLevel.LevelTimerSilverMs);
			starTimesRemainingMs[2] = timeRemainingMs - (CurrentlyLoadedLevel.LevelTimerMaxMs - CurrentlyLoadedLevel.LevelTimerGoldMs);
			for (int i = 0; i < 3; ++i) {
				int remainingTime = starTimesRemainingMs[i];
				if (remainingTime > 0 || remainingTime < ANIMATION_CUTOFF_TIME) continue;

				if (starTimeStarVelocities[i] == INITIAL_STAR_VELOCITY) {
					int instanceId = AudioModule.CreateSoundInstance(AssetLocator.StarFallSound);
					AudioModule.PlaySoundInstance(
						instanceId,
						false,
						0.45f * Config.HUDVolume,
						i == 2 ? 0.8f : i == 1 ? 0.6f : 1f
					);
				}

				starTimeStarVelocities[i] += Vector2.UP * STAR_GRAVITY * deltaTime;
				starTimeStars[i].AnchorOffset += starTimeStarVelocities[i] * deltaTime;
				starTimeStars[i].Rotation += MathUtils.PI * deltaTime;
				starTimeStars[i].Scale *= 1f + (remainingTime + 300) * 0.005f * deltaTime;
				starTimeStars[i].Color = new Vector4(starTimeStars[i].Color, w: starTimeStars[i].Color.W * (1f - deltaTime * 0.5f));

				starTimeStrings[i].Color = new Vector4(starTimeStrings[i].Color, w: starTimeStrings[i].Color.W - deltaTime);

				starTimeBackdrops[i].AnchorOffset += Vector2.UP * deltaTime;
				starTimeBackdrops[i].Rotation += ((i & 1) == 1 ? MathUtils.PI : -MathUtils.PI) * deltaTime;
				starTimeBackdrops[i].Color = new Vector4(starTimeBackdrops[i].Color, w: starTimeBackdrops[i].Color.W - deltaTime * 2f);

				switch (i) {
					case 0:
						if (bronzeTick != null) bronzeTick.AdjustAlpha(-deltaTime);
						break;
					case 1:
						if (silverTick != null) silverTick.AdjustAlpha(-deltaTime);
						break;
					case 2:
						if (goldTick != null) goldTick.AdjustAlpha(-deltaTime);
						break;
				}
			}
		}

		private static void CoinTaken(bool fresh, bool lastFresh) {
			if (fresh) curCoinTexIndex = 0;
			int instanceId = AudioModule.CreateSoundInstance(fresh ? (lastFresh ? AssetLocator.FinalCoinSound : AssetLocator.CoinSound) : AssetLocator.StaleCoinSound);
			AudioModule.PlaySoundInstance(
				instanceId,
				false,
				0.45f * Config.SoundEffectVolume * (lastFresh ? 1.2f : 1f) * (fresh ? 1f : 0.33f),
				1f
			);
			numCoinsString.Text = (numCoinsTakenBeforeStart + numFreshCoinsTaken).ToString("00") + " / " + numCoinsInLevel.ToString("00");
		}
		#endregion

		#region Pass / Fail HUD
		// Both
		private static FontString selectedOptionFontString;
		private static readonly Vector3 selectedOptionFontColorMin = new Vector3(130f / 255f, 190f / 255f, 55f / 255f);
		private static readonly Vector3 selectedOptionFontColorMax = new Vector3(240f / 255f, 220f / 255f, 80f / 255f);
		private static readonly Vector2 passFailOptionsTextScale = new Vector2(0.4f, 0.35f);
		private static readonly Vector2 selectedOptionFontScaleMin = passFailOptionsTextScale * 1.2f;
		private static readonly Vector2 selectedOptionFontScaleMax = passFailOptionsTextScale * 1.6f;
		private static readonly Vector2 selectedOptionFontScalePerSec = (selectedOptionFontScaleMax - selectedOptionFontScaleMin) * 2.2f;
		private static bool optionsStringExpanding;
		const float OPTIONS_TEXT_FADE_IN_TIME = 0.3f;
		const string OPTIONS_TEXT_RETRY = "Retry     Level";
		const string OPTIONS_TEXT_LEVEL_SELECT = "Level     Select";
		const string OPTIONS_TEXT_NEXT_LEVEL = "Next     Level";
		private static bool canSelectOptions;

		// Pass
		private static readonly Vector3 passGoldColor = new Vector3(1f, 0.9f, 0.48f);
		private static readonly Vector3 passSilverColor = new Vector3(0.74f, 0.77f, 0.8f);
		private static readonly Vector3 passBronzeColor = new Vector3(0.64f, 0.33f, 0.15f);
		private static HUDTexture passTextTexture;
		private static FontString passNextLevelString;
		private static FontString passRetryString;
		private static FontString passLevelSelectString;
		private static readonly Vector2 passNextLevelOffset = new Vector2(0f, 0.18f);
		private static readonly Vector2 passRetryOffset = new Vector2(0f, 0.11f);
		private static readonly Vector2 passLevelSelectOffset = new Vector2(0f, 0.04f);
		private static readonly Vector2 passTextTargetScale = new Vector2(0.54f, 0.255f) * 0.75f;
		private static int selectedPassOption;
		const float OPTIONS_TEXT_FADE_IN_DELAY_TIME_PASS = 0.4f;
		const float PASS_TEXT_SCALE = 4f;
		const float PASS_TEXT_SHRINK_TIME = 0.33f;
		const float PASS_DETAILS_FADE_IN_TIME = 1f;
		private enum PassScreenStage { FadeInPassTexture, ShowTime, ShowFriends, ShowEggs, ShowCoins, FadeInOptions, Complete }
		private static PassScreenStage passScreenStage;
		private static readonly Vector2 passLabelScale = Vector2.ONE * 0.6f;
		private static readonly Vector2 passValueScale = Vector2.ONE * 0.6f;
		private static readonly Vector3 passLabelColor = new Vector3(239f / 255f, 161f / 255f, 59f / 255f);
		private static readonly Vector3 passValueColor = new Vector3(180f / 255f, 227f / 255f, 140f / 255f);
		private static readonly Vector3 passEggTotalColor = new Vector3(133f / 255f, 181f / 255f, 61f / 255f);
		private static readonly Vector3 passCoinTotalColor = new Vector3(234f / 255f, 222f / 255f, 58f / 255f);
		private const int PASS_TIME_MS_PER_SEC_REDUCTION = 10000;
		private static FontString passTimeLabel;
		private static FontString passTimeValue;
		private static readonly Dictionary<HUDTexture, bool> passAwardsTextures = new Dictionary<HUDTexture, bool>();
		private static readonly Dictionary<HUDTexture, FontString> passAwardsTimeHeaders = new Dictionary<HUDTexture, FontString>();
		private static readonly List<HUDTexture> passAwardsWorkspace = new List<HUDTexture>();
		private static readonly Dictionary<FontString, Vector3> passAwardTimeTexts = new Dictionary<FontString, Vector3>();
		private static readonly Vector2 passAwardHeaderTextOffset = new Vector2(0.0025f, -0.0415f);
		private const float PASS_AWARD_HEADER_TEXT_MAX_ALPHA = 0.5f;
		private const float PASS_AWARD_HEADER_TEXT_ALPHA_PER_SEC = PASS_AWARD_HEADER_TEXT_MAX_ALPHA * 3f;
		private static readonly Vector2 passAwardHeaderTextScale = Vector2.ONE * 0.4f;
		private const float PASS_AWARD_TIME_TEXT_FADE_PER_SEC = 1f;
		private const float PASS_AWARD_TIME_TEXT_FALL_PER_SEC = -0.2f;
		private static readonly Vector2 passAwardTimeTextScalePerSec = passValueScale * 3f;
		private static readonly Vector2 passAwardTargetScale = new Vector2(0.0376f, 0.0717f) * 1.2f;
		private static readonly Vector2 passAwardMaxScale = passAwardTargetScale * 1.66f;
		private static readonly Vector2 passAwardScaleDeltaPerSec = passAwardTargetScale * 10f;
		private static int curPassTimeValueMs;
		private static FontString passFriendsLabel;
		private static FontString passEggsLabel;
		private static float passTimeElapsedAtFriendsBegin;
		private const float FRIENDS_PASS_DETAILS_FADE_IN_RATE = 1.2f;
		private const float FRIENDS_PASS_DETAILS_FADE_IN_ROW_DELAY = 0.6f;
		private const float FRIENDS_PASS_NAME_TARGET_ALPHA = 0.86f;
		private static Vector2 passFriendsNamesValueScale = passValueScale * 0.75f;
		private static Vector3 passFriendsNamesSelfValueColor = passValueColor;
		private static Vector2 passFriendsTokenScale = new Vector2(0.0376f, 0.0717f) * 0.8f;
		private static Vector2 passFriendsSelfTokenAdditionalScale = passFriendsTokenScale;
		private const int MAX_FRIEND_NAME_LEN = 10;
		private static HUDTexture passFriendsGoldToken;
		private static HUDTexture passFriendsSilverToken;
		private static HUDTexture passFriendsBronzeToken;
		private static FontString passFriendsNameGoldValue;
		private static FontString passFriendsNameSilverValue;
		private static FontString passFriendsNameBronzeValue;
		private static FontString passFriendsTimeGoldValue;
		private static FontString passFriendsTimeSilverValue;
		private static FontString passFriendsTimeBronzeValue;
		private static float passTimeElapsedAtEggsBegin;
		private const float EGGS_PASS_DETAILS_FADE_IN_RATE = 1.8f;
		private const float EGGS_PASS_DETAILS_FADE_IN_ELEMENT_DELAY = 0.2f;
		private const float PASS_REMAINING_EGG_ALPHA = 0.5f;
		private static readonly List<HUDTexture> passEggTextures = new List<HUDTexture>();
		private static FontString passGoldenEggReceiptText;
		private const float PASS_GOLDEN_EGG_TEXT_SCALE_PER_SEC = 2f;
		private const float PASS_GOLDEN_EGG_TEXT_ALPHA_PER_SEC = 0.5f;
		private static HUDTexture passEggInvertTexture;
		private static Vector2 passEggScale = new Vector2(0.0376f, 0.0717f) * 0.75f;
		private static FontString passEggsValue, passEggsValueTotal, passEggsValueTotalX;
		private static float passTimeElapsedAtOptionsBegin;
		private static readonly Vector2 passLabelScaleSmaller = Vector2.ONE * 0.45f;
		private static readonly Vector2 passValueScaleSmaller = Vector2.ONE * 0.45f;
		private const float COINS_PASS_DETAILS_FADE_IN_RATE = 1.8f;
		private const float COINS_ADDITION_INTERVAL = 0.33f;
		private static readonly Vector2 COINS_PASS_VALUE_ADDITION_SCALE = passValueScaleSmaller * 1.4f;
		private static readonly Vector2 COINS_PASS_VALUE_SCALE_REDUCTION_PER_SEC = (COINS_PASS_VALUE_ADDITION_SCALE - passValueScaleSmaller) * 2.3f;
		private static int numCoinsAdded;
		private static int hudTotalCoinValue;
		private static float timeSinceLastCoinAddition;
		private static FontString passCoinsLabel, passCoinsValue, passCoinsLevelTotalValue, passCoinsTotalValueX, passCoinsTotalValue;
		private const float PASS_COIN_TEXTURE_TARGET_Y = 0.6685f;
		private const float PASS_COIN_TEXTURE_STARTING_Y = PASS_COIN_TEXTURE_TARGET_Y - 0.03f;
		private const float PASS_COIN_TEXTURE_DROP_PER_SEC = (PASS_COIN_TEXTURE_TARGET_Y - PASS_COIN_TEXTURE_STARTING_Y) * 2f;
		private static readonly List<HUDTexture> passCoinTextures = new List<HUDTexture>();
		private static HUDTexture passBigCoin;
		private const float FRESH_COIN_Y_OFFSET = 0.05f;
		private const float BIG_COIN_TARGET_Y = 0.66f;
		private const float BIG_COIN_FALL_PER_SEC = 0.13f;
		private const float FRESH_COIN_LIFETIME = 3f;
		private const float FRESH_COIN_FRAME_LIFE = 1f / 32f;
		private static readonly List<FreshCoin> freshCoinTextures = new List<FreshCoin>();
		private static HUDItemExciterEntity selfMedalExciter;

		// Fail
		private static HUDTexture failTextTexture;
		private static HUDTexture failReasonTextTexture;
		private static FontString failRetryString;
		private static FontString failLevelSelectString;
		private static readonly Vector2 failRetryOffset = new Vector2(0f, 0.3f);
		private static readonly Vector2 failLevelSelectOffset = new Vector2(0f, 0.23f);
		private static readonly Vector2 failTextTargetScale = new Vector2(0.54f, 0.255f);
		private static bool retrySelected;
		const float OPTIONS_TEXT_FADE_IN_DELAY_TIME_FAIL = 1.35f;
		const float FAIL_TEXT_SCALE = 4f;
		const float FAIL_TEXT_SHRINK_TIME = 0.33f;
		const float REASON_TEXT_FADE_IN_DELAY_TIME = 0.5f;
		const float REASON_TEXT_FADE_IN_TIME = 0.5f;
		private static bool failTransitionSpeedUp;

		private static void SwitchPostGameOption(bool moveDown) {
			if (!canSelectOptions) return;
			if (moveDown) HUDSoundExtensions.Play(HUDSound.UINextOption);
			else HUDSoundExtensions.Play(HUDSound.UIPreviousOption);
			switch (currentGameState) {
				case OverallGameState.LevelFailed:
					if (retrySelected) {
						retrySelected = false;
						failRetryString.Color = Vector4.ONE;
						failLevelSelectString.Color = Vector4.ZERO;
						selectedOptionFontString.AnchorOffset = failLevelSelectOffset;
						selectedOptionFontString.Text = OPTIONS_TEXT_LEVEL_SELECT;
					}
					else {
						retrySelected = true;
						failLevelSelectString.Color = Vector4.ONE;
						failRetryString.Color = Vector4.ZERO;
						selectedOptionFontString.AnchorOffset = failRetryOffset;
						selectedOptionFontString.Text = OPTIONS_TEXT_RETRY;
					}
					break;
				case OverallGameState.LevelPassed:
					if (passDetails.WasLastLevelInWorld) {
						if (selectedPassOption == 1) selectedPassOption = 0;
						else selectedPassOption = 1;
					}
					else {
						if (moveDown) selectedPassOption = (selectedPassOption + 1) % 3;
						else selectedPassOption = (selectedPassOption - 1) % 3;
						if (selectedPassOption == -1) selectedPassOption = 2;
					}

					if (selectedPassOption == 0) {
						passRetryString.Color = Vector4.ONE;
						passLevelSelectString.Color = Vector4.ONE;
						passNextLevelString.Color = Vector4.ZERO;
						selectedOptionFontString.AnchorOffset = passNextLevelOffset;
						selectedOptionFontString.Text = passNextLevelString.Text;
					}
					else if (selectedPassOption == 1) {
						passNextLevelString.Color = Vector4.ONE;
						passLevelSelectString.Color = Vector4.ONE;
						passRetryString.Color = Vector4.ZERO;
						selectedOptionFontString.AnchorOffset = passRetryOffset;
						selectedOptionFontString.Text = passRetryString.Text;
					}
					else {
						passNextLevelString.Color = Vector4.ONE;
						passRetryString.Color = Vector4.ONE;
						passLevelSelectString.Color = Vector4.ZERO;
						selectedOptionFontString.AnchorOffset = passLevelSelectOffset;
						selectedOptionFontString.Text = passLevelSelectString.Text;
					}
					break;
			}
		}

		private static void SelectPostGameOption() {
			if (canSelectOptions) {
				switch (currentGameState) {
					case OverallGameState.LevelFailed:
						if (retrySelected) {
							HUDSoundExtensions.Play(HUDSound.UISelectOption);
							StartLevelIntroduction();
						}
						else {
							HUDSoundExtensions.Play(HUDSound.UISelectNegativeOption);
							UnloadToMainMenu(MenuCoordinator.MenuState.PlayMenu);
						}
						break;
					case OverallGameState.LevelPassed:
						if (selectedPassOption == 0) {
							if (passDetails.WasLastLevelInWorld) {
								HUDSoundExtensions.Play(HUDSound.UISelectNegativeOption);
								UnloadToMainMenu(MenuCoordinator.MenuState.PlayMenu);
							}
							else {
								HUDSoundExtensions.Play(HUDSound.UISelectOption);
								ProgressToNextLevel();
							}
						}
						else if (selectedPassOption == 1) {
							HUDSoundExtensions.Play(HUDSound.UISelectOption);
							StartLevelIntroduction();
						}
						else {
							HUDSoundExtensions.Play(HUDSound.UISelectNegativeOption);
							UnloadToMainMenu(MenuCoordinator.MenuState.PlayMenu);
						}
						break;
				}
			}
			else {
				switch (currentGameState) {
					case OverallGameState.LevelPassed:
						AdvancePassHUD();
						break;
					case OverallGameState.LevelFailed:
						failTransitionSpeedUp = true;
						failTextTexture.Scale = failTextTargetScale;
						failTextTexture.Color = Vector4.ONE;

						failReasonTextTexture.Color = Vector4.ONE;
						selectedOptionFontString.Color = new Vector4(selectedOptionFontColorMin, w: 1f);

						if (retrySelected) {
							failRetryString.Color = Vector4.ZERO;
							failLevelSelectString.Color = Vector4.ONE;
						}
						else {
							failLevelSelectString.Color = Vector4.ZERO;
							failRetryString.Color = Vector4.ONE;
						}
						canSelectOptions = true;
						break;
				}
			}
		}

		private static void SetUpPassHUD() {
			passTextTexture = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				AnchorOffset = new Vector2(0f, 0.01f),
				Anchoring = ViewportAnchoring.TopCentered,
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = passTextTargetScale * PASS_TEXT_SCALE,
				Texture = AssetLocator.PassMessageTex,
				ZIndex = 1
			};

			passNextLevelString = AssetLocator.TitleFont.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.BottomCentered,
				passNextLevelOffset,
				passFailOptionsTextScale
			);
			passNextLevelString.Text = (passDetails.WasLastLevelInWorld ? OPTIONS_TEXT_LEVEL_SELECT : OPTIONS_TEXT_NEXT_LEVEL);
			passNextLevelString.Color = new Vector4(1f, 1f, 1f, 0f);

			passRetryString = AssetLocator.TitleFont.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.BottomCentered,
				passRetryOffset,
				passFailOptionsTextScale
			);
			passRetryString.Text = OPTIONS_TEXT_RETRY;
			passRetryString.Color = new Vector4(1f, 1f, 1f, 0f);

			passLevelSelectString = AssetLocator.TitleFont.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.BottomCentered,
				passLevelSelectOffset,
				passFailOptionsTextScale
			);
			passLevelSelectString.Text = (passDetails.WasLastLevelInWorld ? String.Empty : OPTIONS_TEXT_LEVEL_SELECT);
			passLevelSelectString.Color = new Vector4(1f, 1f, 1f, 0f);

			selectedOptionFontString = AssetLocator.TitleFont.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.BottomCentered,
				passNextLevelOffset,
				selectedOptionFontScaleMin
			);
			selectedOptionFontString.Text = passNextLevelString.Text;
			selectedOptionFontString.Color = new Vector4(selectedOptionFontColorMin, w: 0f);

			passTimeLabel = AssetLocator.TitleFont.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.TopRight,
				new Vector2(0.565f, 0.3f),
				passLabelScale
			);
			passTimeLabel.Text = "CLOCK    TIME:";
			passTimeLabel.Color = new Vector4(passLabelColor, w: 0f);

			passTimeValue = AssetLocator.TitleFont.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.TopCentered,
				new Vector2(0f, 0.3f),
				passValueScale
			);
			passTimeValue.Text = MakeTimeString(0);
			passTimeValue.Color = new Vector4(passValueColor, w: 0f);

			passFriendsLabel = AssetLocator.TitleFont.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.TopRight,
				new Vector2(0.565f, 0.4f),
				passLabelScaleSmaller
			);
			passFriendsLabel.Text = "FRIENDS:";
			passFriendsLabel.Color = new Vector4(passLabelColor, w: 0f);

			if (passDetails.GoldFriend != null) {
				passFriendsGoldToken = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
					AnchorOffset = new Vector2(0.46f, 0.395f),
					Anchoring = ViewportAnchoring.TopLeft,
					AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
					Color = new Vector4(1f, 1f, 1f, 0f),
					Rotation = 0f,
					Scale = passFriendsTokenScale,
					Texture = AssetLocator.FriendsGoldTimeToken,
					ZIndex = 1
				};

				passFriendsNameGoldValue = AssetLocator.TitleFont.AddString(
					AssetLocator.HudLayer,
					AssetLocator.MainWindow,
					ViewportAnchoring.TopLeft,
					new Vector2(0.5f, 0.4f),
					passValueScaleSmaller
				);
				passFriendsNameGoldValue.Text = passDetails.GoldFriend.Value.Name.WithMaxLength(passDetails.Medal == Medal.Gold ? MAX_FRIEND_NAME_LEN - 0 : MAX_FRIEND_NAME_LEN);
				passFriendsNameGoldValue.Color = passDetails.GoldFriend == LeaderboardManager.LocalPlayerDetails ? new Vector4(passFriendsNamesSelfValueColor, w: 0f) : new Vector4(passValueColor, w: 0f);

				passFriendsTimeGoldValue = AssetLocator.TitleFont.AddString(
					AssetLocator.HudLayer,
					AssetLocator.MainWindow,
					ViewportAnchoring.TopLeft,
					new Vector2(0.75f, 0.4f),
					passValueScaleSmaller
				);
				passFriendsTimeGoldValue.Text = MakeTimeString(passDetails.GoldFriendTimeRemainingMs.Value);
				passFriendsTimeGoldValue.Color = new Vector4(passGoldColor, w: 0f);

				if (passDetails.GoldFriend == LeaderboardManager.LocalPlayerDetails) selfMedalExciter = new HUDItemExciterEntity(passFriendsNameGoldValue) {
					ColorOverride = new Vector3(227f / 255f, 227f / 255f, 140f / 255f),
					Lifetime = 0.4f
				};
			}
			if (passDetails.SilverFriend != null) {
				passFriendsSilverToken = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
					AnchorOffset = new Vector2(0.46f, 0.475f - 0.02f),
					Anchoring = ViewportAnchoring.TopLeft,
					AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
					Color = new Vector4(1f, 1f, 1f, 0f),
					Rotation = 0f,
					Scale = passFriendsTokenScale,
					Texture = AssetLocator.FriendsSilverTimeToken,
					ZIndex = 1
				};

				passFriendsNameSilverValue = AssetLocator.TitleFont.AddString(
					AssetLocator.HudLayer,
					AssetLocator.MainWindow,
					ViewportAnchoring.TopLeft,
					new Vector2(0.5f, 0.48f - 0.02f),
					passValueScaleSmaller
				);
				passFriendsNameSilverValue.Text = passDetails.SilverFriend.Value.Name.WithMaxLength(passDetails.Medal == Medal.Silver ? MAX_FRIEND_NAME_LEN - 0 : MAX_FRIEND_NAME_LEN);
				passFriendsNameSilverValue.Color = passDetails.SilverFriend == LeaderboardManager.LocalPlayerDetails ? new Vector4(passFriendsNamesSelfValueColor, w: 0f) : new Vector4(passValueColor, w: 0f);

				passFriendsTimeSilverValue = AssetLocator.TitleFont.AddString(
					AssetLocator.HudLayer,
					AssetLocator.MainWindow,
					ViewportAnchoring.TopLeft,
					new Vector2(0.75f, 0.48f - 0.02f),
					passValueScaleSmaller
				);
				passFriendsTimeSilverValue.Text = MakeTimeString(passDetails.SilverFriendTimeRemainingMs.Value);
				passFriendsTimeSilverValue.Color = new Vector4(passSilverColor, w: 0f);

				if (passDetails.SilverFriend == LeaderboardManager.LocalPlayerDetails) selfMedalExciter = new HUDItemExciterEntity(passFriendsNameSilverValue) {
					ColorOverride = new Vector3(227f / 255f, 227f / 255f, 140f / 255f),
					Lifetime = 0.4f
				};
			}
			if (passDetails.BronzeFriend != null) {
				passFriendsBronzeToken = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
					AnchorOffset = new Vector2(0.46f, 0.5525f - 0.04f),
					Anchoring = ViewportAnchoring.TopLeft,
					AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
					Color = new Vector4(1f, 1f, 1f, 0f),
					Rotation = 0f,
					Scale = passFriendsTokenScale,
					Texture = AssetLocator.FriendsBronzeTimeToken,
					ZIndex = 1
				};

				passFriendsNameBronzeValue = AssetLocator.TitleFont.AddString(
					AssetLocator.HudLayer,
					AssetLocator.MainWindow,
					ViewportAnchoring.TopLeft,
					new Vector2(0.5f, 0.56f - 0.04f),
					passValueScaleSmaller
				);
				passFriendsNameBronzeValue.Text = passDetails.BronzeFriend.Value.Name.WithMaxLength(passDetails.Medal == Medal.Bronze ? MAX_FRIEND_NAME_LEN - 0 : MAX_FRIEND_NAME_LEN);
				passFriendsNameBronzeValue.Color = passDetails.BronzeFriend == LeaderboardManager.LocalPlayerDetails ? new Vector4(passFriendsNamesSelfValueColor, w: 0f) : new Vector4(passValueColor, w: 0f);

				passFriendsTimeBronzeValue = AssetLocator.TitleFont.AddString(
					AssetLocator.HudLayer,
					AssetLocator.MainWindow,
					ViewportAnchoring.TopLeft,
					new Vector2(0.75f, 0.56f - 0.04f),
					passValueScaleSmaller
				);
				passFriendsTimeBronzeValue.Text = MakeTimeString(passDetails.BronzeFriendTimeRemainingMs.Value);
				passFriendsTimeBronzeValue.Color = new Vector4(passBronzeColor, w: 0f);

				if (passDetails.BronzeFriend == LeaderboardManager.LocalPlayerDetails) selfMedalExciter = new HUDItemExciterEntity(passFriendsNameBronzeValue) {
					ColorOverride = new Vector3(227f / 255f, 227f / 255f, 140f / 255f),
					Lifetime = 0.4f
				};
			}

			passEggsLabel = AssetLocator.TitleFont.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.TopRight,
				new Vector2(0.565f, 0.66f - 0.054f),
				passLabelScaleSmaller
			);
			passEggsLabel.Text = "VULTURE    EGGS:";
			passEggsLabel.Color = new Vector4(passLabelColor, w: 0f);

			for (int i = 0; i < passDetails.VultureEggsDestroyed + passDetails.VultureEggsRemaining; ++i) {
				var eggTex = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
					AnchorOffset = new Vector2(0.525f + 0.03f * i, 0.647f - 0.049f),
					Anchoring = ViewportAnchoring.TopLeft,
					AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
					Color = i < passDetails.VultureEggsDestroyed ? new Vector4(1f, 1f, 1f, 0f) : new Vector4(0.7f, 0.5f, 0.5f, 0f),
					Rotation = 0f,
					Scale = passEggScale,
					Texture = AssetLocator.VultureEgg,
					ZIndex = 1
				};

				passEggTextures.Add(eggTex);
			}

			passEggsValue = AssetLocator.TitleFont.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.TopLeft,
				new Vector2(0.455f, 0.655f - 0.0515f),
				passValueScaleSmaller
			);
			passEggsValue.Text = "0 / " + (passDetails.VultureEggsDestroyed + passDetails.VultureEggsRemaining);
			passEggsValue.Color = new Vector4(passValueColor, w: 0f);

			passEggInvertTexture = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				AnchorOffset = new Vector2(0.74f, 0.655f - 0.0755f),
				Anchoring = ViewportAnchoring.TopLeft,
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = passEggScale * 1.4f,
				Texture = AssetLocator.VultureEggInvert,
				ZIndex = 1
			};

			passEggsValueTotalX = AssetLocator.TitleFont.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.TopLeft,
				new Vector2(0.783f, 0.655f - 0.0453f),
				passValueScaleSmaller
			);
			passEggsValueTotalX.Text = "x";
			passEggsValueTotalX.Color = new Vector4(passEggTotalColor, w: 0f);

			passEggsValueTotal = AssetLocator.TitleFont.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.TopLeft,
				new Vector2(0.808f, 0.655f - 0.063f),
				passValueScale
			);
			passEggsValueTotal.Text = (passDetails.GoldenEggReceived ? passDetails.NumGoldenEggs - 1 : passDetails.NumGoldenEggs).ToString();
			passEggsValueTotal.Color = new Vector4(passEggTotalColor, w: 0f);

			if (passDetails.GoldenEggReceived) {
				passGoldenEggReceiptText = AssetLocator.TitleFont.AddString(
					AssetLocator.HudLayer,
					AssetLocator.MainWindow,
					ViewportAnchoring.TopLeft,
					passEggsValueTotal.AnchorOffset,
					passEggsValueTotal.Scale * 2f
				);
				passGoldenEggReceiptText.Text = passDetails.NumGoldenEggs.ToString();
				passGoldenEggReceiptText.Color = passEggsValueTotal.Color;
			}

			passCoinsLabel = AssetLocator.TitleFont.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.TopRight,
				new Vector2(0.565f, 0.68f),
				passLabelScaleSmaller
			);
			passCoinsLabel.Text = "LIZARD   COINS:";
			passCoinsLabel.Color = new Vector4(passLabelColor, w: 0f);


			passCoinsValue = AssetLocator.TitleFont.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.TopLeft,
				new Vector2(0.455f, 0.675f),
				passValueScaleSmaller
			);
			passCoinsValue.Text = numCoinsTakenBeforeStart.ToString("00");
			passCoinsValue.Color = new Vector4(passValueColor, w: 0f);

			passCoinsLevelTotalValue = AssetLocator.TitleFont.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.TopLeft,
				new Vector2(0.5f, 0.675f),
				passValueScaleSmaller
			);
			passCoinsLevelTotalValue.Text = "/ " + passDetails.TotalCoinsInLevel.ToString("00");
			passCoinsLevelTotalValue.Color = new Vector4(passValueColor, w: 0f);

			passCoinsTotalValueX = AssetLocator.TitleFont.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.TopLeft,
				new Vector2(0.783f, 0.685f),
				passValueScaleSmaller
			);
			passCoinsTotalValueX.Text = "X";
			passCoinsTotalValueX.Color = new Vector4(passCoinTotalColor, w: 0f);

			passCoinsTotalValue = AssetLocator.TitleFont.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.TopLeft,
				new Vector2(0.808f, 0.685f - 0.0177f),
				passValueScale
			);
			hudTotalCoinValue = passDetails.CoinsTakenTotal - passDetails.FreshCoinsTakenInGame;
			passCoinsTotalValue.Text = hudTotalCoinValue.ToString("0000");
			passCoinsTotalValue.Color = new Vector4(passCoinTotalColor, w: 0f);

			const float PASS_COIN_X_MIN = 0.575f;
			const float PASS_COIN_X_MAX = 0.67f;
			float xPerCoin = (PASS_COIN_X_MAX - PASS_COIN_X_MIN) / passDetails.TotalCoinsInLevel;
			for (int i = 0; i < passDetails.TotalCoinsInLevel; ++i) {
				passCoinTextures.Add(
					new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
						AnchorOffset = new Vector2(PASS_COIN_X_MIN + xPerCoin * (((i << 1) + 1) / 2f), PASS_COIN_TEXTURE_STARTING_Y),
						Anchoring = ViewportAnchoring.TopLeft,
						AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
						Color = new Vector4(1f, 1f, 1f, 0f),
						Rotation = 0f,
						Scale = new Vector2(0.0375f, 0.0375f),
						Texture = AssetLocator.CoinFrames[24],
						ZIndex = 1 + i
					}
				);
			}

			passBigCoin = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				AnchorOffset = new Vector2(0.74f, BIG_COIN_TARGET_Y),
				Anchoring = ViewportAnchoring.TopLeft,
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = new Vector2(0.0376f, 0.0717f) * 0.75f * 1.3f,
				Texture = AssetLocator.CoinFrames[0],
				ZIndex = 3
			};

			for (int i = 0; i < passDetails.FreshCoinsTakenInGame; ++i) {
				freshCoinTextures.Add(new FreshCoin(
					new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
						AnchorOffset = passBigCoin.AnchorOffset - new Vector2(0f, FRESH_COIN_Y_OFFSET),
						Anchoring = ViewportAnchoring.TopLeft,
						AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
						Color = new Vector4(1f, 1f, 1f, 0f),
						Rotation = 0f,
						Scale = passBigCoin.Scale,
						Texture = AssetLocator.CoinFrames[0],
						ZIndex = 2
					}
				));
			}

			passScreenStage = PassScreenStage.FadeInPassTexture;
			optionsStringExpanding = true;
			selectedPassOption = 0;
			curPassTimeValueMs = 0;
			canSelectOptions = false;
			timeSinceLastCoinAddition = 0f;
			numCoinsAdded = 0;
		}
		private static void SetUpFailHUD() {
			failTextTexture = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				AnchorOffset = new Vector2(0f, 0.01f),
				Anchoring = ViewportAnchoring.TopCentered,
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = failTextTargetScale * FAIL_TEXT_SCALE,
				Texture = AssetLocator.FailMessageTex,
				ZIndex = 1
			};
			failReasonTextTexture = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				AnchorOffset = new Vector2(0f, 0.325f),
				Anchoring = ViewportAnchoring.TopCentered,
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = new Vector2(0.25f, 0.14f),
				Texture = (failReason == LevelFailReason.Dropped ? AssetLocator.DroppedMessageTex : AssetLocator.TimeUpMessageTex),
				ZIndex = 0
			};

			failRetryString = AssetLocator.TitleFont.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.BottomCentered,
				failRetryOffset,
				passFailOptionsTextScale
			);
			failRetryString.Text = OPTIONS_TEXT_RETRY;
			failRetryString.Color = new Vector4(1f, 1f, 1f, 0f);

			failLevelSelectString = AssetLocator.TitleFont.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.BottomCentered,
				failLevelSelectOffset,
				passFailOptionsTextScale
			);
			failLevelSelectString.Text = OPTIONS_TEXT_LEVEL_SELECT;
			failLevelSelectString.Color = new Vector4(1f, 1f, 1f, 0f);

			selectedOptionFontString = AssetLocator.TitleFont.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.BottomCentered,
				failRetryOffset,
				selectedOptionFontScaleMin
			);
			selectedOptionFontString.Text = OPTIONS_TEXT_RETRY;
			selectedOptionFontString.Color = new Vector4(selectedOptionFontColorMin, w: 0f);

			failTransitionSpeedUp = false;
			optionsStringExpanding = true;
			retrySelected = true;
			canSelectOptions = false;
		}

		private static void DisposePassHUD() {
			if (passTextTexture != null) {
				passTextTexture.Dispose();
				passTextTexture = null;
			}
			if (passNextLevelString != null) {
				passNextLevelString.Dispose();
				passNextLevelString = null;
			}
			if (passRetryString != null) {
				passRetryString.Dispose();
				passRetryString = null;
			}
			if (passLevelSelectString != null) {
				passLevelSelectString.Dispose();
				passLevelSelectString = null;
			}
			if (selectedOptionFontString != null) {
				selectedOptionFontString.Dispose();
				selectedOptionFontString = null;
			}
			if (passTimeLabel != null) {
				passTimeLabel.Dispose();
				passTimeLabel = null;
			}
			if (passTimeValue != null) {
				passTimeValue.Dispose();
				passTimeValue = null;
			}
			foreach (HUDTexture passAwardsTexture in passAwardsTextures.Keys) {
				passAwardsTexture.Dispose();
			}
			passAwardsTextures.Clear();
			foreach (FontString passAwardTimeText in passAwardTimeTexts.Keys) {
				passAwardTimeText.Dispose();
			}
			passAwardTimeTexts.Clear();
			foreach (FontString passAwardTimeHeader in passAwardsTimeHeaders.Values) {
				passAwardTimeHeader.Dispose();
			}
			passAwardsTimeHeaders.Clear();
			if (passFriendsLabel != null) {
				passFriendsLabel.Dispose();
				passFriendsLabel = null;
			}
			if (passEggsLabel != null) {
				passEggsLabel.Dispose();
				passEggsLabel = null;
			}
			if (passFriendsNameGoldValue != null) {
				passFriendsNameGoldValue.Dispose();
				passFriendsNameGoldValue = null;
			}
			if (passFriendsNameSilverValue != null) {
				passFriendsNameSilverValue.Dispose();
				passFriendsNameSilverValue = null;
			}
			if (passFriendsNameBronzeValue != null) {
				passFriendsNameBronzeValue.Dispose();
				passFriendsNameBronzeValue = null;
			}
			if (passFriendsGoldToken != null) {
				passFriendsGoldToken.Dispose();
				passFriendsGoldToken = null;
			}
			if (passFriendsSilverToken != null) {
				passFriendsSilverToken.Dispose();
				passFriendsSilverToken = null;
			}
			if (passFriendsBronzeToken != null) {
				passFriendsBronzeToken.Dispose();
				passFriendsBronzeToken = null;
			}
			if (passFriendsTimeGoldValue != null) {
				passFriendsTimeGoldValue.Dispose();
				passFriendsTimeGoldValue = null;
			}
			if (passFriendsTimeSilverValue != null) {
				passFriendsTimeSilverValue.Dispose();
				passFriendsTimeSilverValue = null;
			}
			if (passFriendsTimeBronzeValue != null) {
				passFriendsTimeBronzeValue.Dispose();
				passFriendsTimeBronzeValue = null;
			}
			foreach (HUDTexture passEggTexture in passEggTextures) {
				passEggTexture.Dispose();
			}
			passEggTextures.Clear();
			if (passEggsValue != null) {
				passEggsValue.Dispose();
				passEggsValue = null;
			}
			if (passEggsValueTotal != null) {
				passEggsValueTotal.Dispose();
				passEggsValueTotal = null;
			}
			if (passEggsValueTotalX != null) {
				passEggsValueTotalX.Dispose();
				passEggsValueTotalX = null;
			}
			if (passEggInvertTexture != null) {
				passEggInvertTexture.Dispose();
				passEggInvertTexture = null;
			}
			if (passGoldenEggReceiptText != null) {
				passGoldenEggReceiptText.Dispose();
				passGoldenEggReceiptText = null;
			}

			if (passCoinsLabel != null) {
				passCoinsLabel.Dispose();
				passCoinsLabel = null;
			}
			if (passCoinsValue != null) {
				passCoinsValue.Dispose();
				passCoinsValue = null;
			}
			if (passCoinsLevelTotalValue != null) {
				passCoinsLevelTotalValue.Dispose();
				passCoinsLevelTotalValue = null;
			}
			if (passCoinsTotalValue != null) {
				passCoinsTotalValue.Dispose();
				passCoinsTotalValue = null;
			}
			if (passCoinsTotalValueX != null) {
				passCoinsTotalValueX.Dispose();
				passCoinsTotalValueX = null;
			}
			if (selfMedalExciter != null) {
				selfMedalExciter.Dispose();
				selfMedalExciter = null;
			}

			if (passBigCoin != null) {
				passBigCoin.Dispose();
				passBigCoin = null;
			}

			foreach (var coinTex in passCoinTextures) {
				coinTex.Dispose();
			}
			passCoinTextures.Clear();

			foreach (var coinTex in freshCoinTextures) {
				coinTex.CoinTex.Dispose();
			}
			freshCoinTextures.Clear();

		}

		private static void DisposeFailHUD() {
			if (failTextTexture != null) {
				failTextTexture.Dispose();
				failTextTexture = null;
			}
			if (failReasonTextTexture != null) {
				failReasonTextTexture.Dispose();
				failReasonTextTexture = null;
			}
			if (failRetryString != null) {
				failRetryString.Dispose();
				failRetryString = null;
			}
			if (failLevelSelectString != null) {
				failLevelSelectString.Dispose();
				failLevelSelectString = null;
			}
			if (selectedOptionFontString != null) {
				selectedOptionFontString.Dispose();
				selectedOptionFontString = null;
			}
		}

		private static unsafe void TickHUD_Passed(float deltaTime) {
			DoSlideOut(deltaTime, passTimeElapsed);

			if (optionsStringExpanding) {
				Vector2 newScale = selectedOptionFontString.Scale + selectedOptionFontScalePerSec * deltaTime;
				if (newScale.LengthSquared > selectedOptionFontScaleMax.LengthSquared) {
					newScale = selectedOptionFontScaleMax;
					optionsStringExpanding = false;
				}
				selectedOptionFontString.Scale = newScale;
			}
			else {
				Vector2 newScale = selectedOptionFontString.Scale - selectedOptionFontScalePerSec * deltaTime;
				if (newScale.LengthSquared < selectedOptionFontScaleMin.LengthSquared || newScale.X < 0f || newScale.Y < 0f) {
					newScale = selectedOptionFontScaleMin;
					optionsStringExpanding = true;
				}
				selectedOptionFontString.Scale = newScale;
			}
			float scaleOfMax = (selectedOptionFontString.Scale.Length - selectedOptionFontScaleMin.Length) / (selectedOptionFontScaleMax.Length - selectedOptionFontScaleMin.Length);
			selectedOptionFontString.Color = new Vector4(Vector3.Lerp(selectedOptionFontColorMin, selectedOptionFontColorMax, scaleOfMax), w: selectedOptionFontString.Color.W);

			switch (passScreenStage) {
				case PassScreenStage.FadeInPassTexture:
					if (passTimeElapsed < PASS_TEXT_SHRINK_TIME) {
						float fractionRemaining = (PASS_TEXT_SHRINK_TIME - passTimeElapsed) / PASS_TEXT_SHRINK_TIME;
						passTextTexture.Scale = passTextTargetScale * (1f + (PASS_TEXT_SCALE - 1f) * fractionRemaining);
						passTextTexture.Color = new Vector4(1f, 1f, 1f, 1f - fractionRemaining);
					}
					else AdvancePassHUD();
					break; // break FadeInPassTexture
				case PassScreenStage.ShowTime:
					if (passTimeElapsed < PASS_TEXT_SHRINK_TIME + PASS_DETAILS_FADE_IN_TIME) {
						float fractionRemaining = (PASS_DETAILS_FADE_IN_TIME - (passTimeElapsed - PASS_TEXT_SHRINK_TIME)) / PASS_DETAILS_FADE_IN_TIME;
						passTimeLabel.Color = new Vector4(passLabelColor, w: 1f - fractionRemaining);
						passTimeValue.Color = new Vector4(passValueColor, w: 1f - fractionRemaining);
					}
					else {
						passTimeLabel.Color = new Vector4(passLabelColor, w: 1f);
						passTimeValue.Color = new Vector4(passValueColor, w: 1f);
						if (passDetails.PreviousLeaderboardDetails.GoldFriend == null && !passAwardsTextures.Any(tex => tex.Key.Texture == AssetLocator.FriendsGoldTimeToken)) {
							ShowPassAward(AssetLocator.FriendsGoldTimeToken);
						}
						else if (passDetails.PreviousLeaderboardDetails.SilverFriend == null && !passAwardsTextures.Any(tex => tex.Key.Texture == AssetLocator.FriendsGoldTimeToken || tex.Key.Texture == AssetLocator.FriendsSilverTimeToken)) {
							ShowPassAward(AssetLocator.FriendsSilverTimeToken);
						}
						else if (passDetails.PreviousLeaderboardDetails.BronzeFriend == null && !passAwardsTextures.Any(tex => tex.Key.Texture == AssetLocator.FriendsGoldTimeToken || tex.Key.Texture == AssetLocator.FriendsSilverTimeToken || tex.Key.Texture == AssetLocator.FriendsBronzeTimeToken)) {
							ShowPassAward(AssetLocator.FriendsBronzeTimeToken);
						}

						if (passDetails.PreviousPersonalBestMs == null && !passAwardsTextures.Any(tex => tex.Key.Texture == AssetLocator.FinishingBell)) {
							ShowPassAward(AssetLocator.FinishingBell);
						}

						int timeValDelta = (int) (PASS_TIME_MS_PER_SEC_REDUCTION * deltaTime);
						int newTimeVal = curPassTimeValueMs + timeValDelta;
						if (newTimeVal / 1000 != curPassTimeValueMs / 1000) HUDSoundExtensions.Play(HUDSound.PostPassTime);
						if (newTimeVal >= passDetails.TimeRemainingMs) AdvancePassHUD();
						else {
							curPassTimeValueMs = newTimeVal;
							passTimeValue.Text = MakeTimeString(newTimeVal);
							int passTimeAsTimeTaken = CurrentlyLoadedLevel.LevelTimerMaxMs - curPassTimeValueMs;
							if (passTimeAsTimeTaken <= CurrentlyLoadedLevel.LevelTimerBronzeMs
								&& passTimeAsTimeTaken + timeValDelta > CurrentlyLoadedLevel.LevelTimerBronzeMs) {
								ShowPassAward(
									AssetLocator.BronzeStar,
									MakeTimeString(CurrentlyLoadedLevel.LevelTimerMaxMs - CurrentlyLoadedLevel.LevelTimerBronzeMs),
									passBronzeColor
								);
							}
							if (currentLevelLeaderboardDetails.BronzeFriendTimeMs != null
								&& curPassTimeValueMs >= currentLevelLeaderboardDetails.BronzeFriendTimeMs
								&& curPassTimeValueMs - timeValDelta < currentLevelLeaderboardDetails.BronzeFriendTimeMs) {
								ShowPassAward(AssetLocator.FriendsBronzeTimeToken);
							}
							if (passTimeAsTimeTaken <= CurrentlyLoadedLevel.LevelTimerSilverMs
								&& passTimeAsTimeTaken + timeValDelta > CurrentlyLoadedLevel.LevelTimerSilverMs) {
								ShowPassAward(
									AssetLocator.SilverStar,
									MakeTimeString(CurrentlyLoadedLevel.LevelTimerMaxMs - CurrentlyLoadedLevel.LevelTimerSilverMs),
									passSilverColor
								);
							}
							if (currentLevelLeaderboardDetails.SilverFriendTimeMs != null
								&& curPassTimeValueMs >= currentLevelLeaderboardDetails.SilverFriendTimeMs
								&& curPassTimeValueMs - timeValDelta < currentLevelLeaderboardDetails.SilverFriendTimeMs) {
								ShowPassAward(AssetLocator.FriendsSilverTimeToken);
							}
							if (passTimeAsTimeTaken <= CurrentlyLoadedLevel.LevelTimerGoldMs
								&& passTimeAsTimeTaken + timeValDelta > CurrentlyLoadedLevel.LevelTimerGoldMs) {
								ShowPassAward(
									AssetLocator.GoldStar,
									MakeTimeString(CurrentlyLoadedLevel.LevelTimerMaxMs - CurrentlyLoadedLevel.LevelTimerGoldMs),
									passGoldColor
								);
							}
							if (currentLevelLeaderboardDetails.GoldFriendTimeMs != null
								&& curPassTimeValueMs >= currentLevelLeaderboardDetails.GoldFriendTimeMs
								&& curPassTimeValueMs - timeValDelta < currentLevelLeaderboardDetails.GoldFriendTimeMs) {
								ShowPassAward(AssetLocator.FriendsGoldTimeToken);
							}
							if (passDetails.NewPersonalBest
								&& curPassTimeValueMs > passDetails.PreviousPersonalBestMs
								&& curPassTimeValueMs - timeValDelta <= passDetails.PreviousPersonalBestMs) {
								ShowPassAward(AssetLocator.FinishingBell);
							}
						}
					}
					break; // break ShowTime
				case PassScreenStage.ShowFriends:
					float friendsTimeElapsed = passTimeElapsed - passTimeElapsedAtFriendsBegin;
					int lineNum = (int) (friendsTimeElapsed / FRIENDS_PASS_DETAILS_FADE_IN_ROW_DELAY);
					float alphaDelta = FRIENDS_PASS_DETAILS_FADE_IN_RATE * deltaTime;
					if (lineNum >= 0) {
						if (passFriendsGoldToken.Color.W == 0f) {
							HUDSoundExtensions.Play(HUDSound.PostPassLineReveal);
							if (passDetails.GoldFriend == LeaderboardManager.LocalPlayerDetails) HUDSoundExtensions.Play(HUDSound.PostPassLeaderboard, 0.5f, 1f);
						}
						if (passFriendsLabel.Color.W != 1f) passFriendsLabel.Color = new Vector4(passFriendsLabel.Color, w: Math.Min(passFriendsLabel.Color.W + alphaDelta * 2f, 1f));
						if (passFriendsGoldToken.Color.W != 1f) passFriendsGoldToken.Color = new Vector4(passFriendsGoldToken.Color, w: Math.Min(passFriendsGoldToken.Color.W + alphaDelta, 1f));
						if (passFriendsNameGoldValue.Color.W != FRIENDS_PASS_NAME_TARGET_ALPHA) passFriendsNameGoldValue.Color = new Vector4(passFriendsNameGoldValue.Color, w: Math.Min(passFriendsNameGoldValue.Color.W + alphaDelta, FRIENDS_PASS_NAME_TARGET_ALPHA));
						if (passFriendsTimeGoldValue.Color.W != FRIENDS_PASS_NAME_TARGET_ALPHA) passFriendsTimeGoldValue.Color = new Vector4(passFriendsTimeGoldValue.Color, w: Math.Min(passFriendsTimeGoldValue.Color.W + alphaDelta, FRIENDS_PASS_NAME_TARGET_ALPHA));

						if (passDetails.GoldFriend == LeaderboardManager.LocalPlayerDetails) {
							float tokenStuffCompletionFraction = Math.Min(friendsTimeElapsed / (1f / FRIENDS_PASS_DETAILS_FADE_IN_RATE) * 2f, 1f);
							passFriendsGoldToken.Scale = passFriendsTokenScale + passFriendsSelfTokenAdditionalScale * (0.5f - Math.Abs(0.5f - tokenStuffCompletionFraction)) * 2f;
							passFriendsGoldToken.Rotation = tokenStuffCompletionFraction * MathUtils.TWO_PI;
						}
					}
					if (lineNum >= 1 && passFriendsSilverToken != null) {
						if (passFriendsSilverToken.Color.W == 0f) {
							HUDSoundExtensions.Play(HUDSound.PostPassLineReveal);
							if (passDetails.SilverFriend == LeaderboardManager.LocalPlayerDetails) HUDSoundExtensions.Play(HUDSound.PostPassLeaderboard, 0.35f, 0.9f);
						}
						if (passFriendsSilverToken.Color.W != 1f) passFriendsSilverToken.Color = new Vector4(passFriendsSilverToken.Color, w: Math.Min(passFriendsSilverToken.Color.W + alphaDelta, 1f));
						if (passFriendsNameSilverValue.Color.W != FRIENDS_PASS_NAME_TARGET_ALPHA) passFriendsNameSilverValue.Color = new Vector4(passFriendsNameSilverValue.Color, w: Math.Min(passFriendsNameSilverValue.Color.W + alphaDelta, FRIENDS_PASS_NAME_TARGET_ALPHA));
						if (passFriendsTimeSilverValue.Color.W != FRIENDS_PASS_NAME_TARGET_ALPHA) passFriendsTimeSilverValue.Color = new Vector4(passFriendsTimeSilverValue.Color, w: Math.Min(passFriendsTimeSilverValue.Color.W + alphaDelta, FRIENDS_PASS_NAME_TARGET_ALPHA));

						if (passDetails.SilverFriend == LeaderboardManager.LocalPlayerDetails) {
							float tokenStuffCompletionFraction = Math.Min((friendsTimeElapsed - FRIENDS_PASS_DETAILS_FADE_IN_ROW_DELAY) / (1f / FRIENDS_PASS_DETAILS_FADE_IN_RATE) * 2f, 1f);
							passFriendsSilverToken.Scale = passFriendsTokenScale + passFriendsSelfTokenAdditionalScale * (0.5f - Math.Abs(0.5f - tokenStuffCompletionFraction)) * 2f;
							passFriendsSilverToken.Rotation = tokenStuffCompletionFraction * MathUtils.TWO_PI;
						}
					}
					if (lineNum >= 2 && passFriendsBronzeToken != null) {
						if (passFriendsBronzeToken.Color.W == 0f) {
							HUDSoundExtensions.Play(HUDSound.PostPassLineReveal);
							if (passDetails.BronzeFriend == LeaderboardManager.LocalPlayerDetails) HUDSoundExtensions.Play(HUDSound.PostPassLeaderboard, 0.2f, 0.8f);
						}
						if (passFriendsBronzeToken.Color.W != 1f) passFriendsBronzeToken.Color = new Vector4(passFriendsBronzeToken.Color, w: Math.Min(passFriendsBronzeToken.Color.W + alphaDelta, 1f));
						if (passFriendsNameBronzeValue.Color.W != FRIENDS_PASS_NAME_TARGET_ALPHA) passFriendsNameBronzeValue.Color = new Vector4(passFriendsNameBronzeValue.Color, w: Math.Min(passFriendsNameBronzeValue.Color.W + alphaDelta, FRIENDS_PASS_NAME_TARGET_ALPHA));
						if (passFriendsTimeBronzeValue.Color.W != FRIENDS_PASS_NAME_TARGET_ALPHA) passFriendsTimeBronzeValue.Color = new Vector4(passFriendsTimeBronzeValue.Color, w: Math.Min(passFriendsTimeBronzeValue.Color.W + alphaDelta, FRIENDS_PASS_NAME_TARGET_ALPHA));

						if (passDetails.BronzeFriend == LeaderboardManager.LocalPlayerDetails) {
							float tokenStuffCompletionFraction = Math.Min((friendsTimeElapsed - FRIENDS_PASS_DETAILS_FADE_IN_ROW_DELAY * 2f) / (1f / FRIENDS_PASS_DETAILS_FADE_IN_RATE) * 2f, 1f);
							passFriendsBronzeToken.Scale = passFriendsTokenScale + passFriendsSelfTokenAdditionalScale * (0.5f - Math.Abs(0.5f - tokenStuffCompletionFraction)) * 2f;
							passFriendsBronzeToken.Rotation = tokenStuffCompletionFraction * MathUtils.TWO_PI;
						}
					}
					if (passFriendsBronzeToken != null) {
						if (passFriendsBronzeToken.Color.W >= 1f) AdvancePassHUD();
					}
					else if (passFriendsSilverToken != null) {
						if (passFriendsSilverToken.Color.W >= 1f) AdvancePassHUD();
					}
					else if (passFriendsGoldToken.Color.W >= 1f) AdvancePassHUD();
					break; // break ShowFriends
				case PassScreenStage.ShowEggs:
					float eggsTimeElapsed = passTimeElapsed - passTimeElapsedAtEggsBegin;
					int elementNum = (int) (eggsTimeElapsed / EGGS_PASS_DETAILS_FADE_IN_ELEMENT_DELAY) - 1;
					alphaDelta = EGGS_PASS_DETAILS_FADE_IN_RATE * deltaTime;
					if (passEggsLabel.Color.W != 1f) passEggsLabel.Color = new Vector4(passEggsLabel.Color, w: Math.Min(passEggsLabel.Color.W + alphaDelta * 2f, 1f));
					if (passEggsValue.Color.W != 1f) passEggsValue.Color = new Vector4(passEggsValue.Color, w: Math.Min(passEggsValue.Color.W + alphaDelta, 1f));
					if (passEggsValueTotal.Color.W != 1f) {
						passEggsValueTotal.Color = new Vector4(passEggsValueTotal.Color, w: Math.Min(passEggsValueTotal.Color.W + alphaDelta, 1f));
						passEggsValueTotalX.Color = passEggsValueTotal.Color;
						passEggInvertTexture.Color = new Vector4(passEggInvertTexture.Color, w: Math.Min(passEggInvertTexture.Color.W + alphaDelta * 2f, 1f));
					}

					for (int i = 0; i < elementNum; ++i) {
						if (i >= passDetails.VultureEggsDestroyed + passDetails.VultureEggsRemaining) {
							AdvancePassHUD();
							break;
						}

						var eggTex = passEggTextures[i];

						if (i < passDetails.VultureEggsDestroyed) {
							if (eggTex.Color.W == 0f) {
								HUDSoundExtensions.Play(HUDSound.PostPassEggPop, pitch: 1f);
								HUDSoundExtensions.Play(HUDSound.PostPassSmashedEgg);
								if (i == passDetails.VultureEggsDestroyed - 1 && passDetails.VultureEggsRemaining == 0) HUDSoundExtensions.Play(HUDSound.PostPassAllEggs);
								passEggsValue.Text = (i + 1) + " / " + (passDetails.VultureEggsDestroyed + passDetails.VultureEggsRemaining);
							}
							if (eggTex.Color.W != 1f) eggTex.Color = new Vector4(eggTex.Color, w: Math.Min(eggTex.Color.W + alphaDelta, 1f));
						}
						else {
							if (eggTex.Color.W == 0f) HUDSoundExtensions.Play(HUDSound.PostPassEggPop, pitch: 0.6f);
							if (eggTex.Color.W != PASS_REMAINING_EGG_ALPHA) eggTex.Color = new Vector4(eggTex.Color, w: Math.Min(eggTex.Color.W + alphaDelta, PASS_REMAINING_EGG_ALPHA));
						}
					}
					break; // break ShowEggs
				case PassScreenStage.ShowCoins:
					alphaDelta = COINS_PASS_DETAILS_FADE_IN_RATE * deltaTime;
					if (passCoinsLabel.Color.W != 1f) {
						passCoinsLabel.Color = new Vector4(passCoinsLabel.Color, w: Math.Min(1f, passCoinsLabel.Color.W + alphaDelta));
						passCoinsValue.Color = new Vector4(passCoinsValue.Color, w: passCoinsLabel.Color.W);
						passCoinsLevelTotalValue.Color = new Vector4(passCoinsLevelTotalValue.Color, w: passCoinsLabel.Color.W);
						passCoinsTotalValue.Color = new Vector4(passCoinsTotalValue.Color, w: passCoinsLabel.Color.W);
						passCoinsTotalValueX.Color = new Vector4(passCoinsTotalValueX.Color, w: passCoinsLabel.Color.W);
						passBigCoin.Color = new Vector4(passBigCoin.Color, w: passCoinsLabel.Color.W);
					}
					else {
						timeSinceLastCoinAddition += deltaTime;
						float adjustedAdditionInterval = COINS_ADDITION_INTERVAL;
						if (passDetails.CoinsTakenInGame - numCoinsAdded >= 8) adjustedAdditionInterval *= 0.5f;
						if (passDetails.CoinsTakenInGame - numCoinsAdded >= 16) adjustedAdditionInterval *= 0.5f;
						if (passDetails.CoinsTakenInGame - numCoinsAdded >= 24) adjustedAdditionInterval *= 0.5f;
						while (timeSinceLastCoinAddition >= adjustedAdditionInterval && numCoinsAdded < passDetails.CoinsTakenInGame) {
							if (numCoinsAdded < (passDetails.CoinsTakenInGame - passDetails.FreshCoinsTakenInGame)) {
								HUDSoundExtensions.Play(HUDSound.PostPassStaleCoin);
							}
							else {
								passCoinsValue.Scale = COINS_PASS_VALUE_ADDITION_SCALE;
								if (numCoinsAdded == passDetails.TotalCoinsInLevel - 1) HUDSoundExtensions.Play(HUDSound.PostPassFinalCoin);
								else HUDSoundExtensions.Play(HUDSound.PostPassFreshCoin);
								++hudTotalCoinValue;
								passCoinsTotalValue.Text = hudTotalCoinValue.ToString("0000");
								var fc = freshCoinTextures[numCoinsAdded - (passDetails.CoinsTakenInGame - passDetails.FreshCoinsTakenInGame)];
								fc.Active = true;
								fc.CoinTex.Color = new Vector4(fc.CoinTex.Color, w: 1f);
								passBigCoin.AnchorOffset = new Vector2(passBigCoin.AnchorOffset.X, BIG_COIN_TARGET_Y - FRESH_COIN_Y_OFFSET);
								passCoinsValue.Text = (Int32.Parse(passCoinsValue.Text) + 1).ToString("00");
							}
							++numCoinsAdded;
							timeSinceLastCoinAddition -= adjustedAdditionInterval;
							adjustedAdditionInterval = COINS_ADDITION_INTERVAL;
							if (passDetails.CoinsTakenInGame - numCoinsAdded >= 8) adjustedAdditionInterval *= 0.5f;
							if (passDetails.CoinsTakenInGame - numCoinsAdded >= 16) adjustedAdditionInterval *= 0.5f;
							if (passDetails.CoinsTakenInGame - numCoinsAdded >= 24) adjustedAdditionInterval *= 0.5f;
						}

						if (numCoinsAdded == passDetails.CoinsTakenInGame) AdvancePassHUD();
					}
					break;
				case PassScreenStage.FadeInOptions:
					float optionsTimeElapsed = passTimeElapsed - passTimeElapsedAtOptionsBegin;
					if (optionsTimeElapsed < OPTIONS_TEXT_FADE_IN_DELAY_TIME_PASS + OPTIONS_TEXT_FADE_IN_TIME) {
						float fractionRemaining = (OPTIONS_TEXT_FADE_IN_TIME - (optionsTimeElapsed - OPTIONS_TEXT_FADE_IN_DELAY_TIME_PASS)) / OPTIONS_TEXT_FADE_IN_TIME;
						float fractionThisTick = (OPTIONS_TEXT_FADE_IN_TIME - (deltaTime - OPTIONS_TEXT_FADE_IN_DELAY_TIME_PASS)) / OPTIONS_TEXT_FADE_IN_TIME;

						if (fractionRemaining < 0.5f && fractionRemaining + fractionThisTick >= 0.5f) HUDSoundExtensions.Play(HUDSound.PostPassShowMenu);

						selectedOptionFontString.Color = new Vector4(selectedOptionFontString.Color, w: 1f - fractionRemaining);
						switch (selectedPassOption) {
							case 0:
								passNextLevelString.Color = Vector4.ZERO;
								passRetryString.Color = new Vector4(1f, 1f, 1f, 1f - fractionRemaining);
								passLevelSelectString.Color = new Vector4(1f, 1f, 1f, 1f - fractionRemaining);
								break;
							case 1:
								passNextLevelString.Color = new Vector4(1f, 1f, 1f, 1f - fractionRemaining);
								passRetryString.Color = Vector4.ZERO;
								passLevelSelectString.Color = new Vector4(1f, 1f, 1f, 1f - fractionRemaining);
								break;
							case 2:
								passNextLevelString.Color = new Vector4(1f, 1f, 1f, 1f - fractionRemaining);
								passRetryString.Color = new Vector4(1f, 1f, 1f, 1f - fractionRemaining);
								passLevelSelectString.Color = Vector4.ZERO;
								break;
						}
					}
					else AdvancePassHUD();
					break; // break FadeInOptions
			}

			if (passGoldenEggReceiptText != null && passGoldenEggReceiptText.Color.W != 0f) {
				passGoldenEggReceiptText.Scale += passValueScale * PASS_GOLDEN_EGG_TEXT_SCALE_PER_SEC * deltaTime;
				passGoldenEggReceiptText.Color = new Vector4(passGoldenEggReceiptText.Color, w: Math.Max(0f, passGoldenEggReceiptText.Color.W - PASS_GOLDEN_EGG_TEXT_ALPHA_PER_SEC * deltaTime));
			}
			if (passEggsValueTotal.Scale != passValueScale) {
				passEggsValueTotal.Scale -= passValueScale * PASS_GOLDEN_EGG_TEXT_SCALE_PER_SEC * 2f * deltaTime;
				if (passEggsValueTotal.Scale.LengthSquared <= passValueScale.LengthSquared) passEggsValueTotal.Scale = passValueScale;
			}

			if (passCoinsValue.Scale != passValueScaleSmaller) {
				passCoinsValue.Scale -= COINS_PASS_VALUE_SCALE_REDUCTION_PER_SEC * deltaTime;
				if (passCoinsValue.Scale.LengthSquared < passValueScaleSmaller.LengthSquared) passCoinsValue.Scale = passValueScaleSmaller;
			}

			float coinDropDelta = PASS_COIN_TEXTURE_DROP_PER_SEC * deltaTime;
			for (int i = 0; i < numCoinsAdded; ++i) {
				if (passCoinTextures[i].AnchorOffset.Y != PASS_COIN_TEXTURE_TARGET_Y) {
					passCoinTextures[i].AnchorOffset = new Vector2(passCoinTextures[i].AnchorOffset.X, Math.Min(passCoinTextures[i].AnchorOffset.Y + coinDropDelta, PASS_COIN_TEXTURE_TARGET_Y));
					Vector3 rgb = (i < (passDetails.CoinsTakenInGame - passDetails.FreshCoinsTakenInGame) ? 0.75f : 1f) * Vector3.ONE;
					float alphaFrac = (passCoinTextures[i].AnchorOffset.Y - PASS_COIN_TEXTURE_STARTING_Y) / (PASS_COIN_TEXTURE_TARGET_Y - PASS_COIN_TEXTURE_STARTING_Y);
					passCoinTextures[i].Color = new Vector4(rgb, w: rgb.X * alphaFrac);
				}
			}

			foreach (var fc in freshCoinTextures) {
				if (!fc.Active) continue;
				if (fc.AliveTime >= FRESH_COIN_LIFETIME) {
					fc.Active = false;
					fc.CoinTex.Color = Vector4.ZERO;
					continue;
				}
				fc.CurrentVelocity += new Vector2(0f, 0.981f) * deltaTime;
				fc.CoinTex.AnchorOffset += fc.CurrentVelocity * deltaTime;
				int frameNum = (int) (fc.AliveTime / FRESH_COIN_FRAME_LIFE) & 31;
				float alpha = 1f - fc.AliveTime / FRESH_COIN_LIFETIME;
				fc.CoinTex.Color = new Vector4(fc.CoinTex.Color, w: alpha);
				fc.CoinTex.Texture = AssetLocator.CoinFrames[frameNum];
				fc.AliveTime += deltaTime;
			}

			if (passBigCoin.AnchorOffset.Y != BIG_COIN_TARGET_Y) {
				passBigCoin.AnchorOffset = new Vector2(passBigCoin.AnchorOffset.X, Math.Min(passBigCoin.AnchorOffset.Y + BIG_COIN_FALL_PER_SEC * deltaTime, BIG_COIN_TARGET_Y));
			}

			passAwardsWorkspace.Clear();
			foreach (KeyValuePair<HUDTexture, bool> awardKVP in passAwardsTextures) {
				if (awardKVP.Value) {
					Vector2 newScale = awardKVP.Key.Scale + passAwardScaleDeltaPerSec * deltaTime;
					if (newScale.LengthSquared >= passAwardMaxScale.LengthSquared) {
						passAwardsWorkspace.Add(awardKVP.Key);
						newScale = passAwardMaxScale;
					}
					awardKVP.Key.Scale = newScale;
				}
				else if (awardKVP.Key.Scale != passAwardTargetScale) {
					Vector2 newScale = awardKVP.Key.Scale - passAwardScaleDeltaPerSec * deltaTime;
					if (awardKVP.Key.Texture == AssetLocator.FriendsSilverTimeToken || awardKVP.Key.Texture == AssetLocator.FriendsGoldTimeToken) {
						newScale = awardKVP.Key.Scale - passAwardScaleDeltaPerSec * deltaTime * 0.25f;
					}
					if (newScale.LengthSquared <= passAwardTargetScale.LengthSquared) newScale = passAwardTargetScale;
					awardKVP.Key.Scale = newScale;
				}
			}
			foreach (FontString awardHeader in passAwardsTimeHeaders.Values) {
				var curAlpha = awardHeader.Color.W;
				if (curAlpha < PASS_AWARD_HEADER_TEXT_MAX_ALPHA) {
					awardHeader.SetAlpha(Math.Min(PASS_AWARD_HEADER_TEXT_MAX_ALPHA, deltaTime * PASS_AWARD_HEADER_TEXT_ALPHA_PER_SEC + curAlpha));
				}
			}
			foreach (HUDTexture hudTexture in passAwardsWorkspace) {
				passAwardsTextures[hudTexture] = false;
			}

			foreach (var kvp in passAwardTimeTexts) {
				FontString timeText = kvp.Key;
				if (timeText.Color.W <= 0f) continue;
				timeText.Color = new Vector4(Vector3.Lerp(passValueColor, kvp.Value, (float) MathUtils.Clamp((1f - timeText.Color.W) * 1.85f, 0f, 1f)), w: timeText.Color.W - deltaTime * PASS_AWARD_TIME_TEXT_FADE_PER_SEC);
				timeText.AnchorOffset = new Vector2(timeText.AnchorOffset, y: timeText.AnchorOffset.Y + deltaTime * PASS_AWARD_TIME_TEXT_FALL_PER_SEC);
				timeText.Scale += passAwardTimeTextScalePerSec * deltaTime;
			}
		}

		private static unsafe void TickHUD_Failed(float deltaTime) {
			DoSlideOut(deltaTime, failTimeElapsed);

			if (optionsStringExpanding) {
				Vector2 newScale = selectedOptionFontString.Scale + selectedOptionFontScalePerSec * deltaTime;
				if (newScale.LengthSquared > selectedOptionFontScaleMax.LengthSquared) {
					newScale = selectedOptionFontScaleMax;
					optionsStringExpanding = false;
				}
				selectedOptionFontString.Scale = newScale;
			}
			else {
				Vector2 newScale = selectedOptionFontString.Scale - selectedOptionFontScalePerSec * deltaTime;
				if (newScale.LengthSquared < selectedOptionFontScaleMin.LengthSquared || newScale.X < 0f || newScale.Y < 0f) {
					newScale = selectedOptionFontScaleMin;
					optionsStringExpanding = true;
				}
				selectedOptionFontString.Scale = newScale;
			}
			float scaleOfMax = (selectedOptionFontString.Scale.Length - selectedOptionFontScaleMin.Length) / (selectedOptionFontScaleMax.Length - selectedOptionFontScaleMin.Length);
			selectedOptionFontString.Color = new Vector4(Vector3.Lerp(selectedOptionFontColorMin, selectedOptionFontColorMax, scaleOfMax), w: selectedOptionFontString.Color.W);

			if (!failTransitionSpeedUp) {
				if (failTimeElapsed < FAIL_TEXT_SHRINK_TIME) {
					float fractionRemaining = (FAIL_TEXT_SHRINK_TIME - failTimeElapsed) / FAIL_TEXT_SHRINK_TIME;
					failTextTexture.Scale = failTextTargetScale * (1f + (FAIL_TEXT_SCALE - 1f) * fractionRemaining);
					failTextTexture.Color = new Vector4(1f, 1f, 1f, 1f - fractionRemaining);
				}
				else if (failTextTexture.Scale != failTextTargetScale) {
					failTextTexture.Scale = failTextTargetScale;
					failTextTexture.Color = Vector4.ONE;
				}

				if (failTimeElapsed >= REASON_TEXT_FADE_IN_DELAY_TIME) {
					if (failTimeElapsed < REASON_TEXT_FADE_IN_DELAY_TIME + REASON_TEXT_FADE_IN_TIME) {
						float fractionRemaining = (REASON_TEXT_FADE_IN_TIME - (failTimeElapsed - REASON_TEXT_FADE_IN_DELAY_TIME)) / REASON_TEXT_FADE_IN_TIME;
						failReasonTextTexture.Color = new Vector4(1f, 1f, 1f, 1f - fractionRemaining);
					}
					else if (failReasonTextTexture.Color != Vector4.ONE) {
						failReasonTextTexture.Color = Vector4.ONE;
					}
				}


				if (failTimeElapsed >= OPTIONS_TEXT_FADE_IN_DELAY_TIME_FAIL) {
					if (failTimeElapsed < OPTIONS_TEXT_FADE_IN_DELAY_TIME_FAIL + OPTIONS_TEXT_FADE_IN_TIME) {
						float fractionRemaining = (OPTIONS_TEXT_FADE_IN_TIME - (failTimeElapsed - OPTIONS_TEXT_FADE_IN_DELAY_TIME_FAIL)) / OPTIONS_TEXT_FADE_IN_TIME;
						selectedOptionFontString.Color = new Vector4(selectedOptionFontString.Color, w: 1f - fractionRemaining);
						if (retrySelected) {
							failRetryString.Color = Vector4.ZERO;
							failLevelSelectString.Color = new Vector4(1f, 1f, 1f, 1f - fractionRemaining);
						}
						else {
							failLevelSelectString.Color = Vector4.ZERO;
							failRetryString.Color = new Vector4(1f, 1f, 1f, 1f - fractionRemaining);
						}
					}
					else {
						if (retrySelected) {
							if (failLevelSelectString.Color != Vector4.ONE) failLevelSelectString.Color = Vector4.ONE;
							if (failRetryString.Color != Vector4.ZERO) failRetryString.Color = Vector4.ZERO;
						}
						else {
							if (failLevelSelectString.Color != Vector4.ZERO) failLevelSelectString.Color = Vector4.ZERO;
							if (failRetryString.Color != Vector4.ONE) failRetryString.Color = Vector4.ONE;
						}
						selectedOptionFontString.Color = new Vector4(selectedOptionFontString.Color, w: 1f);
						canSelectOptions = true;
					}
				}
			}
		}

		private static void AdvancePassHUD() {
			switch (passScreenStage) {
				case PassScreenStage.FadeInPassTexture:
					HUDSoundExtensions.Play(HUDSound.PostPassLineReveal);
					passTextTexture.Scale = passTextTargetScale;
					passTextTexture.Color = Vector4.ONE;
					passScreenStage = PassScreenStage.ShowTime;
					break;
				case PassScreenStage.ShowTime:
					passTimeLabel.Color = new Vector4(passLabelColor, w: 1f);
					passTimeValue.Color = new Vector4(passValueColor, w: 1f);
					passTimeValue.Text = MakeTimeString(passDetails.TimeRemainingMs);

					if ((passDetails.Star == Star.Gold || passDetails.Star == Star.Silver || passDetails.Star == Star.Bronze) && !passAwardsTextures.Any(tex => tex.Key.Texture == AssetLocator.BronzeStar)) {
						ShowPassAward(AssetLocator.BronzeStar);
					}
					if ((passDetails.Star == Star.Gold || passDetails.Star == Star.Silver) && !passAwardsTextures.Any(tex => tex.Key.Texture == AssetLocator.SilverStar)) {
						ShowPassAward(AssetLocator.SilverStar);
					}
					if (passDetails.Star == Star.Gold && !passAwardsTextures.Any(tex => tex.Key.Texture == AssetLocator.GoldStar)) {
						ShowPassAward(AssetLocator.GoldStar);
					}

					if (passDetails.Medal == Medal.Gold && !passAwardsTextures.Any(tex => tex.Key.Texture == AssetLocator.FriendsGoldTimeToken)) {
						ShowPassAward(AssetLocator.FriendsGoldTimeToken);
					}
					else if (passDetails.Medal == Medal.Silver && !passAwardsTextures.Any(tex => tex.Key.Texture == AssetLocator.FriendsSilverTimeToken)) {
						ShowPassAward(AssetLocator.FriendsSilverTimeToken);
					}
					else if (passDetails.Medal == Medal.Bronze && !passAwardsTextures.Any(tex => tex.Key.Texture == AssetLocator.FriendsBronzeTimeToken)) {
						ShowPassAward(AssetLocator.FriendsBronzeTimeToken);
					}

					if (passDetails.NewPersonalBest && !passAwardsTextures.Any(tex => tex.Key.Texture == AssetLocator.FinishingBell)) {
						ShowPassAward(AssetLocator.FinishingBell);
					}

					passScreenStage = PassScreenStage.ShowFriends;
					passTimeElapsedAtFriendsBegin = passTimeElapsed;
					break;
				case PassScreenStage.ShowFriends:
					passFriendsLabel.Color = new Vector4(passLabelColor, w: 1f);
					passFriendsGoldToken.Color = passDetails.GoldFriend != null ? Vector4.ONE : Vector4.ZERO;
					passFriendsGoldToken.Rotation = 0f;
					passFriendsGoldToken.Scale = passFriendsTokenScale;
					passFriendsNameGoldValue.Color = passDetails.GoldFriend != null ? new Vector4(passDetails.GoldFriend == LeaderboardManager.LocalPlayerDetails ? passFriendsNamesSelfValueColor : passValueColor, w: FRIENDS_PASS_NAME_TARGET_ALPHA) : Vector4.ZERO;
					passFriendsTimeGoldValue.Color = passDetails.GoldFriend != null ? new Vector4(passGoldColor, w: FRIENDS_PASS_NAME_TARGET_ALPHA) : Vector4.ZERO;
					if (passFriendsSilverToken != null) {
						passFriendsSilverToken.Color = passDetails.SilverFriend != null ? Vector4.ONE : Vector4.ZERO;
						passFriendsNameSilverValue.Color = passDetails.SilverFriend != null ? new Vector4(passDetails.SilverFriend == LeaderboardManager.LocalPlayerDetails ? passFriendsNamesSelfValueColor : passValueColor, w: FRIENDS_PASS_NAME_TARGET_ALPHA) : Vector4.ZERO;
						passFriendsTimeSilverValue.Color = passDetails.SilverFriend != null ? new Vector4(passSilverColor, w: FRIENDS_PASS_NAME_TARGET_ALPHA) : Vector4.ZERO;
						passFriendsSilverToken.Rotation = 0f;
						passFriendsSilverToken.Scale = passFriendsTokenScale;
					}
					if (passFriendsBronzeToken != null) {
						passFriendsBronzeToken.Color = passDetails.BronzeFriend != null ? Vector4.ONE : Vector4.ZERO;
						passFriendsNameBronzeValue.Color = passDetails.BronzeFriend != null ? new Vector4(passDetails.BronzeFriend == LeaderboardManager.LocalPlayerDetails ? passFriendsNamesSelfValueColor : passValueColor, w: FRIENDS_PASS_NAME_TARGET_ALPHA) : Vector4.ZERO;
						passFriendsTimeBronzeValue.Color = passDetails.BronzeFriend != null ? new Vector4(passBronzeColor, w: FRIENDS_PASS_NAME_TARGET_ALPHA) : Vector4.ZERO;
						passFriendsBronzeToken.Rotation = 0f;
						passFriendsBronzeToken.Scale = passFriendsTokenScale;
					}

					HUDSoundExtensions.Play(HUDSound.PostPassLineReveal);

					passScreenStage = PassScreenStage.ShowEggs;
					passTimeElapsedAtEggsBegin = passTimeElapsed;
					break;
				case PassScreenStage.ShowEggs:
					for (int i = 0; i < passEggTextures.Count; ++i) {
						if (i < passDetails.VultureEggsDestroyed) passEggTextures[i].Color = Vector4.ONE;
						else passEggTextures[i].Color = new Vector4(passEggTextures[i].Color, w: PASS_REMAINING_EGG_ALPHA);
					}

					if (passGoldenEggReceiptText != null) {
						passGoldenEggReceiptText.Color = new Vector4(passGoldenEggReceiptText.Color, w: 1f);
						passEggsValueTotal.Text = passDetails.NumGoldenEggs.ToString();
						passEggsValueTotal.Scale = passValueScale * 2f;
					}

					passEggsValue.Text = passDetails.VultureEggsDestroyed + " / " + (passDetails.VultureEggsDestroyed + passDetails.VultureEggsRemaining);
					passEggsLabel.Color = new Vector4(passLabelColor, w: 1f);
					passEggsValue.Color = new Vector4(passValueColor, w: 1f);
					passEggsValueTotal.Color = new Vector4(passEggsValueTotal.Color, w: 1f);
					passEggsValueTotalX.Color = new Vector4(passEggsValueTotalX.Color, w: 1f);
					passEggInvertTexture.Color = new Vector4(passEggInvertTexture.Color, w: 1f);

					passScreenStage = PassScreenStage.ShowCoins;
					break;
				case PassScreenStage.ShowCoins:
					passCoinsLabel.Color = new Vector4(passCoinsLabel.Color, w: 1f);
					passCoinsValue.Color = new Vector4(passCoinsValue.Color, w: 1f);
					passCoinsLevelTotalValue.Color = new Vector4(passCoinsLevelTotalValue.Color, w: 1f);
					passCoinsTotalValue.Color = new Vector4(passCoinsTotalValue.Color, w: 1f);
					passCoinsTotalValueX.Color = new Vector4(passCoinsTotalValueX.Color, w: 1f);
					passBigCoin.Color = new Vector4(passBigCoin.Color, w: 1f);

					numCoinsAdded = passDetails.CoinsTakenInGame;
					hudTotalCoinValue = passDetails.CoinsTakenTotal;
					passCoinsTotalValue.Text = hudTotalCoinValue.ToString("0000");
					passCoinsValue.Text = (numCoinsTakenBeforeStart + numFreshCoinsTaken).ToString("00");

					passScreenStage = PassScreenStage.FadeInOptions;
					passTimeElapsedAtOptionsBegin = passTimeElapsed;
					break;
				case PassScreenStage.FadeInOptions:
					switch (selectedPassOption) {
						case 0:
							if (passNextLevelString.Color != Vector4.ZERO) passNextLevelString.Color = Vector4.ZERO;
							if (passRetryString.Color != Vector4.ONE) passRetryString.Color = Vector4.ONE;
							if (passLevelSelectString.Color != Vector4.ONE) passLevelSelectString.Color = Vector4.ONE;
							break;
						case 1:
							if (passNextLevelString.Color != Vector4.ONE) passNextLevelString.Color = Vector4.ONE;
							if (passRetryString.Color != Vector4.ZERO) passRetryString.Color = Vector4.ZERO;
							if (passLevelSelectString.Color != Vector4.ONE) passLevelSelectString.Color = Vector4.ONE;
							break;
						case 2:
							if (passNextLevelString.Color != Vector4.ONE) passNextLevelString.Color = Vector4.ONE;
							if (passRetryString.Color != Vector4.ONE) passRetryString.Color = Vector4.ONE;
							if (passLevelSelectString.Color != Vector4.ZERO) passLevelSelectString.Color = Vector4.ZERO;
							break;
					}



					selectedOptionFontString.Color = new Vector4(selectedOptionFontString.Color, w: 1f);
					passScreenStage = PassScreenStage.Complete;
					canSelectOptions = true;
					break;
			}
		}

		private static void ShowPassAward(ITexture2D texture) {
			ShowPassAward(texture, null, Vector3.ZERO);
		}

		private static void ShowPassAward(ITexture2D texture, string timeText, Vector3 timeTextColor) {
			string timeHeaderText = String.Empty;
			Vector3 timeHeaderTextColor = Vector3.ONE;

			if (texture == AssetLocator.GoldStar) {
				HUDSoundExtensions.Play(HUDSound.PostPassStarGold);
				timeHeaderText = MakeTimeString(CurrentlyLoadedLevel.LevelTimerMaxMs - CurrentlyLoadedLevel.LevelTimerGoldMs);
				timeHeaderTextColor = passGoldColor;
			}
			else if (texture == AssetLocator.SilverStar) {
				HUDSoundExtensions.Play(HUDSound.PostPassStarSilver);
				timeHeaderText = MakeTimeString(CurrentlyLoadedLevel.LevelTimerMaxMs - CurrentlyLoadedLevel.LevelTimerSilverMs);
				timeHeaderTextColor = passSilverColor;
			}
			else if (texture == AssetLocator.BronzeStar) {
				HUDSoundExtensions.Play(HUDSound.PostPassStarBronze);
				timeHeaderText = MakeTimeString(CurrentlyLoadedLevel.LevelTimerMaxMs - CurrentlyLoadedLevel.LevelTimerBronzeMs);
				timeHeaderTextColor = passBronzeColor;
			}
			else if (texture == AssetLocator.FriendsGoldTimeToken) {
				HUDSoundExtensions.Play(HUDSound.PostPassMedalGold);
			}
			else if (texture == AssetLocator.FriendsSilverTimeToken) {
				HUDSoundExtensions.Play(HUDSound.PostPassMedalSilver);
			}
			else if (texture == AssetLocator.FriendsBronzeTimeToken) {
				HUDSoundExtensions.Play(HUDSound.PostPassMedalBronze);
			}
			else if (texture == AssetLocator.FinishingBell) {
				HUDSoundExtensions.Play(HUDSound.PostPassPersonalBest);
			}

			HUDSoundExtensions.Play(HUDSound.PostPassLineReveal);


			HUDTexture awardTex = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				AnchorOffset = new Vector2(0.6f + passAwardsTextures.Count * 0.06f, 0.28f),
				Anchoring = ViewportAnchoring.TopLeft,
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = new Vector4(1f, 1f, 1f, 1f),
				Rotation = 0f,
				Scale = Vector2.ZERO,
				Texture = texture,
				ZIndex = 1
			};

			if (timeHeaderText != null) {
				FontString timeHeaderString = AssetLocator.MainFont.AddString(
					AssetLocator.HudLayer,
					AssetLocator.MainWindow,
					awardTex.Anchoring,
					awardTex.AnchorOffset + passAwardHeaderTextOffset,
					passAwardHeaderTextScale
				);
				timeHeaderString.Text = timeHeaderText;
				timeHeaderString.Color = new Vector4(timeHeaderTextColor, w: 0f);

				passAwardsTimeHeaders[awardTex] = timeHeaderString;
			}

			if (texture == AssetLocator.FriendsSilverTimeToken) {
				var prev = passAwardsTextures.Keys.FirstOrDefault(key => key.Texture == AssetLocator.FriendsBronzeTimeToken);
				if (prev != null) {
					awardTex.Dispose();
					prev.Texture = texture;
					prev.Scale = passAwardMaxScale;
					if (passAwardsTimeHeaders.ContainsKey(awardTex)) {
						passAwardsTimeHeaders[prev].Color = passAwardsTimeHeaders[awardTex].Color;
						passAwardsTimeHeaders[prev].Text = passAwardsTimeHeaders[awardTex].Text;
						passAwardsTimeHeaders[awardTex].Dispose();
						passAwardsTimeHeaders.Remove(awardTex);
					}
				}
				else passAwardsTextures.Add(awardTex, true);
			}
			else if (texture == AssetLocator.FriendsGoldTimeToken) {
				var prev = passAwardsTextures.Keys.FirstOrDefault(key => key.Texture == AssetLocator.FriendsSilverTimeToken);
				if (prev != null) {
					awardTex.Dispose();
					prev.Texture = texture;
					prev.Scale = passAwardMaxScale;
					if (passAwardsTimeHeaders.ContainsKey(awardTex)) {
						passAwardsTimeHeaders[prev].Color = passAwardsTimeHeaders[awardTex].Color;
						passAwardsTimeHeaders[prev].Text = passAwardsTimeHeaders[awardTex].Text;
						passAwardsTimeHeaders[awardTex].Dispose();
						passAwardsTimeHeaders.Remove(awardTex);
					}
				}
				else {
					var prevBronze = passAwardsTextures.Keys.FirstOrDefault(key => key.Texture == AssetLocator.FriendsBronzeTimeToken);
					if (prevBronze != null) {
						awardTex.Dispose();
						prevBronze.Texture = texture;
						prevBronze.Scale = passAwardMaxScale;
						if (passAwardsTimeHeaders.ContainsKey(awardTex)) {
							passAwardsTimeHeaders[prevBronze].Color = passAwardsTimeHeaders[awardTex].Color;
							passAwardsTimeHeaders[prevBronze].Text = passAwardsTimeHeaders[awardTex].Text;
							passAwardsTimeHeaders[awardTex].Dispose();
							passAwardsTimeHeaders.Remove(awardTex);
						}
					}
					else passAwardsTextures.Add(awardTex, true);
				}
			}
			else passAwardsTextures.Add(awardTex, true);

			if (timeText != null) {
				FontString text = AssetLocator.TitleFont.AddString(
					AssetLocator.HudLayer,
					AssetLocator.MainWindow,
					ViewportAnchoring.TopCentered,
					new Vector2(0f, 0.3f),
					passValueScale
				);
				text.Text = timeText;
				text.Color = new Vector4(passValueColor, w: 1f);

				passAwardTimeTexts.Add(text, timeTextColor);
			}
		}

		private const float SLIDE_OUT_RATE = 0.32f;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void SlideOut(HUDTexture texture, float deltaTime) {
			if (texture == null) return;
			texture.AnchorOffset = new Vector2(texture.AnchorOffset.X - SLIDE_OUT_RATE * deltaTime, texture.AnchorOffset.Y);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void SlideOut(FontString fString, float deltaTime) {
			if (fString == null) return;
			fString.AnchorOffset = new Vector2(fString.AnchorOffset.X - SLIDE_OUT_RATE * deltaTime, fString.AnchorOffset.Y);
		}

		private static void DoSlideOut(float deltaTime, float timeElapsed) {
			const float DISPOSE_TIME = 4f;

			if (timeElapsed < DISPOSE_TIME) {
				SlideOut(kmhLabel, deltaTime);

				SlideOut(eggCounterBackdrop, deltaTime);
				for (int i = 0; i < 5; ++i) {
					SlideOut(vultureEggs[i], deltaTime);
					SlideOut(vultureEggCrosses[i], deltaTime);
					if (i < 4) SlideOut(timerFigures[i], deltaTime);

					if (i < 3) {
						SlideOut(starTimeBackdrops[i], deltaTime);
						SlideOut(starTimeStars[i], deltaTime);
						SlideOut(starTimeStrings[i], deltaTime);
					}
				}

				SlideOut(coinBackdrop, deltaTime);
				SlideOut(spinningCoin, deltaTime);
				SlideOut(numCoinsString, deltaTime);

				SlideOut(timerBackdrop, deltaTime);
				SlideOut(timerBackdropGlass, deltaTime);

				SlideOut(worldNameBackdrop, deltaTime);
				SlideOut(worldNameString, deltaTime);
				SlideOut(worldIcon, deltaTime);
				SlideOut(levelNameBackdrop, deltaTime);
				SlideOut(levelNameString, deltaTime);
				SlideOut(levelNumberString, deltaTime);

				SlideOut(personalBestTimeBackdrop, deltaTime);
				SlideOut(goldFriendsTimeBackdrop, deltaTime);
				SlideOut(silverFriendsTimeBackdrop, deltaTime);
				SlideOut(bronzeFriendsTimeBackdrop, deltaTime);

				SlideOut(personalBestBell, deltaTime);
				SlideOut(goldFriendsToken, deltaTime);
				SlideOut(silverFriendsToken, deltaTime);
				SlideOut(bronzeFriendsToken, deltaTime);

				SlideOut(personalBestString, deltaTime);
				SlideOut(goldFriendTimeString, deltaTime);
				SlideOut(silverFriendTimeString, deltaTime);
				SlideOut(bronzeFriendTimeString, deltaTime);
				SlideOut(goldFriendNameString, deltaTime);
				SlideOut(silverFriendNameString, deltaTime);
				SlideOut(bronzeFriendNameString, deltaTime);

				foreach (var tick in tickTextures) {
					SlideOut(tick, deltaTime);
				}

				foreach (var eggHopTexture in eggHopTextures) {
					SlideOut(eggHopTexture, deltaTime);
				}

				SlideOut(difficultyIcon, deltaTime);
				SlideOut(difficultyString, deltaTime);
			}
			else if (timeElapsed > DISPOSE_TIME && timeElapsed - deltaTime <= DISPOSE_TIME) DisposeHUD(false);
		}
		#endregion

		#region Pause Menu
		private static FontString pauseTitleString;
		private static FontString pauseResumeString;
		private static FontString pauseRestartString;
		private static FontString pauseOptionsString;
		private static FontString pauseMenuString;
		private static FontString pauseExitString;
		private static int pauseSelectedIndex;
		private static HUDTexture pauseLeftEgg;
		private static HUDTexture pauseRightEgg;
		private static HUDItemExciterEntity pauseExciter;
		private static readonly Dictionary<IHUDObject, float> pauseOriginalAlphaDict = new Dictionary<IHUDObject, float>();
		private const float PAUSE_MENU_FADE_IN_TIME = 0.6f;
		private const float PAUSE_MENU_EGG_ROT = MathUtils.PI_OVER_TWO * 0.3f;
		private const float PAUSE_MENU_EGG_ROT_OFFSET = MathUtils.PI_OVER_TWO * 0.1f;
		private const float PAUSE_MENU_EGG_BUFFER = 0.04f;
		private const float PAUSE_MENU_EGG_Y_OFFSET = -0.01f;
		private const float PAUSE_MENU_EGG_ROT_TIME = 0.45f;
		private static float pauseMenuCurEggRotTime;
		private static FontString pauseConfirmTitleString;
		private static FontString pauseConfirmYesString;
		private static FontString pauseConfirmNoString;
		private static bool pauseOnExitConfirm;
		private static bool pauseExitConfirmYesSelected;
		private static float pauseSwapModeTime;
		private static float pauseTime;

		private static void ActivatePauseMenu() {
			GCSettings.LatencyMode = GCLatencyMode.Interactive;
			currentGameState = OverallGameState.LevelPaused;
			EntityModule.PausePhysics = true;
			pauseSelectedIndex = 0;
			pauseOriginalAlphaDict.Clear();
			pauseMenuCurEggRotTime = 0f;
			pauseOnExitConfirm = false;
			pauseExitConfirmYesSelected = true;
			pauseSwapModeTime = EntityModule.ElapsedTime;
			pauseTime = 0f;
			Sounds_LevelPause();
			HUDSound.UISelectOption.Play();
			ClearHUDIntroElements();

			float y = 0.21f;
			const float OFFSET_PER_LINE = 0.087f;
			const float SCALE_DIFF = 0.8f * 0.7f;
			pauseTitleString = AssetLocator.TitleFontGlow.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.TopCentered,
				new Vector2(0f, y),
				Vector2.ONE * 1f
			);
			pauseTitleString.Text = "PAUSED";
			pauseTitleString.Color = new Vector4(passLabelColor, w: 0f);

			y += OFFSET_PER_LINE;

			pauseResumeString = AssetLocator.TitleFont.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.TopCentered,
				new Vector2(0f, (y += OFFSET_PER_LINE)),
				pauseTitleString.Scale * SCALE_DIFF
			);
			pauseResumeString.Text = "RESUME";
			pauseResumeString.Color = new Vector4(passValueColor, w: 0f);

			pauseRestartString = AssetLocator.TitleFont.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.TopCentered,
				new Vector2(0f, (y += OFFSET_PER_LINE)),
				pauseTitleString.Scale * SCALE_DIFF
			);
			pauseRestartString.Text = "RESTART LEVEL";
			pauseRestartString.Color = new Vector4(passValueColor, w: 0f);

			pauseOptionsString = AssetLocator.TitleFont.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.TopCentered,
				new Vector2(0f, (y += OFFSET_PER_LINE)),
				pauseTitleString.Scale * SCALE_DIFF
			);
			pauseOptionsString.Text = "OPTIONS";
			pauseOptionsString.Color = new Vector4(passValueColor, w: 0f);

			pauseMenuString = AssetLocator.TitleFont.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.TopCentered,
				new Vector2(0f, (y += OFFSET_PER_LINE)),
				pauseTitleString.Scale * SCALE_DIFF
			);
			pauseMenuString.Text = "MAIN MENU";
			pauseMenuString.Color = new Vector4(passValueColor, w: 0f);

			pauseExitString = AssetLocator.TitleFont.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.TopCentered,
				new Vector2(0f, (y += OFFSET_PER_LINE)),
				pauseTitleString.Scale * SCALE_DIFF
			);
			pauseExitString.Text = "QUIT GAME";
			pauseExitString.Color = new Vector4(passValueColor, w: 0f);

			pauseLeftEgg = new HUDTexture(
				AssetLocator.HUDFragmentShader,
				AssetLocator.HudLayer,
				AssetLocator.MainWindow
			) {
				Anchoring = ViewportAnchoring.TopLeft,
				AnchorOffset = new Vector2(0f, 0f),
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = Vector3.ONE,
				Rotation = -PAUSE_MENU_EGG_ROT + PAUSE_MENU_EGG_ROT_OFFSET,
				Scale = Vector2.ONE * 0.05f,
				Texture = AssetLocator.VultureEgg,
				ZIndex = 1
			};
			pauseRightEgg = new HUDTexture(
				AssetLocator.HUDFragmentShader,
				AssetLocator.HudLayer,
				AssetLocator.MainWindow
			) {
				Anchoring = ViewportAnchoring.TopRight,
				AnchorOffset = new Vector2(0f, 0f),
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = Vector3.ONE,
				Rotation = pauseLeftEgg.Rotation,
				Scale = pauseLeftEgg.Scale,
				Texture = AssetLocator.VultureEgg,
				ZIndex = 1
			};
			pauseExciter = new HUDItemExciterEntity(null) {
				Speed = 0.0025f,
				Lifetime = 0.65f,
				CountPerSec = 15,
				OpacityMultiplier = 0.85f,
				ColorOverride = passValueColor * 1.6f
			};

			pauseConfirmTitleString = AssetLocator.TitleFontGlow.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.TopCentered,
				new Vector2(0f, 0.35f),
				Vector2.ONE * 1f
			);
			pauseConfirmTitleString.Text = "EXIT GAME?";
			pauseConfirmTitleString.Color = new Vector4(passLabelColor, w: 0f);

			pauseConfirmYesString = AssetLocator.TitleFont.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.TopRight,
				new Vector2(0.6f, 0.6f),
				Vector2.ONE * 0.75f
			);
			pauseConfirmYesString.Text = "YES";
			pauseConfirmYesString.Color = new Vector4(passValueColor, w: 0f);

			pauseConfirmNoString = AssetLocator.TitleFont.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.TopLeft,
				new Vector2(0.6f, 0.6f),
				Vector2.ONE * 0.75f
			);
			pauseConfirmNoString.Text = "NO";
			pauseConfirmNoString.Color = new Vector4(passValueColor, w: 0f);




			RecordPauseAlpha(kmhLabel);

			RecordPauseAlpha(eggCounterBackdrop);
			for (int i = 0; i < 5; ++i) {
				RecordPauseAlpha(vultureEggs[i]);
				RecordPauseAlpha(vultureEggCrosses[i]);
				if (i < 4) RecordPauseAlpha(timerFigures[i]);

				if (i < 3) {
					RecordPauseAlpha(starTimeBackdrops[i]);
					RecordPauseAlpha(starTimeStars[i]);
					RecordPauseAlpha(starTimeStrings[i]);
				}
			}

			RecordPauseAlpha(coinBackdrop);
			RecordPauseAlpha(spinningCoin);
			RecordPauseAlpha(numCoinsString);

			RecordPauseAlpha(timerBackdrop);
			RecordPauseAlpha(timerBackdropGlass);

			RecordPauseAlpha(worldNameBackdrop);
			RecordPauseAlpha(levelNameBackdrop);
			RecordPauseAlpha(worldNameString);
			RecordPauseAlpha(worldIcon);
			RecordPauseAlpha(levelNameString);
			RecordPauseAlpha(levelNumberString);

			RecordPauseAlpha(personalBestTimeBackdrop);
			RecordPauseAlpha(goldFriendsTimeBackdrop);
			RecordPauseAlpha(silverFriendsTimeBackdrop);
			RecordPauseAlpha(bronzeFriendsTimeBackdrop);

			RecordPauseAlpha(personalBestBell);
			RecordPauseAlpha(goldFriendsToken);
			RecordPauseAlpha(silverFriendsToken);
			RecordPauseAlpha(bronzeFriendsToken);

			RecordPauseAlpha(personalBestString);
			RecordPauseAlpha(goldFriendTimeString);
			RecordPauseAlpha(silverFriendTimeString);
			RecordPauseAlpha(bronzeFriendTimeString);
			RecordPauseAlpha(goldFriendNameString);
			RecordPauseAlpha(silverFriendNameString);
			RecordPauseAlpha(bronzeFriendNameString);

			RecordPauseAlpha(difficultyIcon);
			RecordPauseAlpha(difficultyString);

			if (tutorialPanelActive) {
				RecordPauseAlpha(tutorialPanelBracketLeft);
				RecordPauseAlpha(tutorialPanelBracketRight);
				RecordPauseAlpha(tutorialPanelBackdrop);
				RecordPauseAlpha(tutorialPanelTitleString);
				foreach (var tutLine in tutorialPanelTextStrings) RecordPauseAlpha(tutLine);
			}

			foreach (var tickTexture in tickTextures) {
				RecordPauseAlpha(tickTexture);
			}

			foreach (var hopTexture in eggHopTextures) {
				RecordPauseAlpha(hopTexture);
			}

			AssetLocator.LightPass.SetLensProperties(PhysicsManager.ONE_METRE_SCALED * 0.1f, PhysicsManager.ONE_METRE_SCALED * 1f);
			PauseSetSelectedIndex(0);
		}

		private static void DeactivatePauseMenu() {
			currentGameState = OverallGameState.LevelPlaying;
			EntityModule.PausePhysics = false;
			AssetLocator.LightPass.SetLensProperties(Config.DofLensFocalDist, Config.DofLensMaxBlurDist);
			Sounds_LevelUnpause();

			pauseExciter.Dispose();
			pauseTitleString.Dispose();
			pauseResumeString.Dispose();
			pauseRestartString.Dispose();
			pauseOptionsString.Dispose();
			pauseMenuString.Dispose();
			pauseExitString.Dispose();
			pauseLeftEgg.Dispose();
			pauseRightEgg.Dispose();
			pauseConfirmTitleString.Dispose();
			pauseConfirmYesString.Dispose();
			pauseConfirmNoString.Dispose();

			foreach (var kvp in pauseOriginalAlphaDict) {
				kvp.Key.SetAlpha(kvp.Value);
			}

			GCSettings.LatencyMode = GCLatencyMode.Interactive;
		}

		private static void TickPauseMenu(float deltaTime) {
			pauseTime += deltaTime;
			float hudFadeOutComplete = Math.Min(pauseTime / PAUSE_MENU_FADE_IN_TIME, 1f);
			foreach (var kvp in pauseOriginalAlphaDict) {
				kvp.Key.SetAlpha(kvp.Value * (1f - (0.5f * hudFadeOutComplete)));
			}

			float timeSinceSwap = EntityModule.ElapsedTime - pauseSwapModeTime;
			float fadeInComplete = Math.Min(timeSinceSwap / PAUSE_MENU_FADE_IN_TIME, 1f);
			float mainPauseElementsAlpha = pauseOnExitConfirm ? (1f - fadeInComplete) : fadeInComplete;
			pauseTitleString.SetAlpha(mainPauseElementsAlpha);
			pauseResumeString.SetAlpha(mainPauseElementsAlpha);
			pauseRestartString.SetAlpha(mainPauseElementsAlpha);
			pauseOptionsString.SetAlpha(mainPauseElementsAlpha);
			pauseMenuString.SetAlpha(mainPauseElementsAlpha);
			pauseExitString.SetAlpha(mainPauseElementsAlpha);
			pauseLeftEgg.SetAlpha(mainPauseElementsAlpha);
			pauseRightEgg.SetAlpha(mainPauseElementsAlpha);

			if (!pauseOnExitConfirm) {
				pauseConfirmTitleString.SetAlpha(Math.Min(pauseConfirmTitleString.Color.W, 1f - mainPauseElementsAlpha));
				pauseConfirmYesString.SetAlpha(Math.Min(pauseConfirmYesString.Color.W, 1f - mainPauseElementsAlpha));
				pauseConfirmNoString.SetAlpha(Math.Min(pauseConfirmNoString.Color.W, 1f - mainPauseElementsAlpha));
			}
			else {
				pauseConfirmTitleString.SetAlpha(1f - mainPauseElementsAlpha);
				pauseConfirmYesString.SetAlpha(1f - mainPauseElementsAlpha);
				pauseConfirmNoString.SetAlpha(1f - mainPauseElementsAlpha);
			}

			pauseMenuCurEggRotTime += deltaTime;
			while (pauseMenuCurEggRotTime > PAUSE_MENU_EGG_ROT_TIME) {
				pauseMenuCurEggRotTime -= PAUSE_MENU_EGG_ROT_TIME;
				if (pauseLeftEgg.Rotation < 0f) {
					pauseLeftEgg.Rotation += PAUSE_MENU_EGG_ROT * 2f;
					pauseRightEgg.Rotation += PAUSE_MENU_EGG_ROT * 2f;
				}
				else {
					pauseLeftEgg.Rotation -= PAUSE_MENU_EGG_ROT * 2f;
					pauseRightEgg.Rotation -= PAUSE_MENU_EGG_ROT * 2f;
				}
			}
		}

		private static void RecordPauseAlpha(IHUDObject o) {
			if (o == null) return;
			pauseOriginalAlphaDict.Add(o, o.Color.W);
		}

		private static void PauseSwitchOption(bool down) {
			if (pauseOnExitConfirm) return;
			if (down) {
				HUDSound.UINextOption.Play();
				PauseSetSelectedIndex(pauseSelectedIndex == 4 ? 0 : pauseSelectedIndex + 1);
			}
			else {
				HUDSound.UIPreviousOption.Play();
				PauseSetSelectedIndex(pauseSelectedIndex == 0 ? 4 : pauseSelectedIndex - 1);
			}
		}

		private static void PauseLeftRight() {
			if (!pauseOnExitConfirm) return;
			HUDSound.UINextOption.Play();
			if (pauseExciter.TargetObj == pauseConfirmYesString) pauseExciter.TargetObj = pauseConfirmNoString;
			else pauseExciter.TargetObj = pauseConfirmYesString;
		}

		private static void PauseSetSelectedIndex(int newIndex) {
			switch (newIndex) {
				case 0:
					pauseRightEgg.AnchorOffset = pauseLeftEgg.AnchorOffset =
						new Vector2(
							0.5f - (pauseResumeString.Dimensions.X * 0.5f + PAUSE_MENU_EGG_BUFFER),
							pauseResumeString.AnchorOffset.Y + PAUSE_MENU_EGG_Y_OFFSET
						);
					pauseExciter.TargetObj = pauseResumeString;
					break;
				case 1:
					pauseRightEgg.AnchorOffset = pauseLeftEgg.AnchorOffset =
						new Vector2(
							0.5f - (pauseRestartString.Dimensions.X * 0.5f + PAUSE_MENU_EGG_BUFFER),
							pauseRestartString.AnchorOffset.Y + PAUSE_MENU_EGG_Y_OFFSET
						);
					pauseExciter.TargetObj = pauseRestartString;
					break;
				case 2:
					pauseRightEgg.AnchorOffset = pauseLeftEgg.AnchorOffset =
						new Vector2(
							0.5f - (pauseOptionsString.Dimensions.X * 0.5f + PAUSE_MENU_EGG_BUFFER),
							pauseOptionsString.AnchorOffset.Y + PAUSE_MENU_EGG_Y_OFFSET
						);
					pauseExciter.TargetObj = pauseOptionsString;
					break;
				case 3:
					pauseRightEgg.AnchorOffset = pauseLeftEgg.AnchorOffset =
						new Vector2(
							0.5f - (pauseMenuString.Dimensions.X * 0.5f + PAUSE_MENU_EGG_BUFFER),
							pauseMenuString.AnchorOffset.Y + PAUSE_MENU_EGG_Y_OFFSET
						);
					pauseExciter.TargetObj = pauseMenuString;
					break;
				case 4:
					pauseRightEgg.AnchorOffset = pauseLeftEgg.AnchorOffset =
						new Vector2(
							0.5f - (pauseExitString.Dimensions.X * 0.5f + PAUSE_MENU_EGG_BUFFER),
							pauseExitString.AnchorOffset.Y + PAUSE_MENU_EGG_Y_OFFSET
						);
					pauseExciter.TargetObj = pauseExitString;
					break;
			}

			pauseSelectedIndex = newIndex;
		}

		private static void PauseBackOut() {
			HUDSound.UISelectNegativeOption.Play();
			if (pauseOnExitConfirm) {
				pauseOnExitConfirm = false;
				pauseExciter.TargetObj = pauseExitString;
				pauseSwapModeTime = EntityModule.ElapsedTime;
			}
			else DeactivatePauseMenu();
		}

		private static void PauseActionSelectedOption() {
			switch (pauseSelectedIndex) {
				case 0:
					HUDSound.UISelectOption.Play();
					DeactivatePauseMenu();
					break;
				case 1:
					HUDSound.UISelectOption.Play();
					DeactivatePauseMenu();
					StartLevelIntroduction();
					break;
				case 2:
					HUDSound.UISelectNegativeOption.Play();
					foreach (var kvp in pauseOriginalAlphaDict) {
						kvp.Key.SetAlpha(0f);
					}
					pauseTitleString.SetAlpha(0f);
					pauseResumeString.SetAlpha(0f);
					pauseRestartString.SetAlpha(0f);
					pauseOptionsString.SetAlpha(0f);
					pauseMenuString.SetAlpha(0f);
					pauseExitString.SetAlpha(0f);
					pauseLeftEgg.SetAlpha(0f);
					pauseRightEgg.SetAlpha(0f);
					DeferToMainMenu(PauseRevertFromDeferredMenu, MenuCoordinator.MenuState.OptionsMenu);
					break;
				case 3:
					HUDSound.UISelectNegativeOption.Play();
					DeactivatePauseMenu();
					UnloadToMainMenu();
					break;
				case 4:
					if (!pauseOnExitConfirm) {
						HUDSound.UISelectOption.Play();
						pauseOnExitConfirm = true;
						pauseExciter.TargetObj = pauseConfirmYesString;
						pauseSwapModeTime = EntityModule.ElapsedTime;
					}
					else if (pauseExciter.TargetObj != pauseConfirmYesString) {
						pauseOnExitConfirm = false;
						pauseExciter.TargetObj = pauseExitString;
						pauseSwapModeTime = EntityModule.ElapsedTime;
					}
					else {
						LosgapSystem.Exit();
					}
					break;
			}
		}

		private static void PauseRevertFromDeferredMenu() {

		}
		#endregion

		#region Tutorial Panel
		private static bool tutorialPanelActive = false;
		private static bool tutorialPanelClosing = false;
		private static bool tutorialPanelDiminished = false;
		private static HUDTexture tutorialPanelBracketLeft, tutorialPanelBracketRight;
		private static HUDTexture tutorialPanelBackdrop;
		private const float TUTORIAL_PANEL_BACKDROP_TARGET_ALPHA = 0.6f * 1.3f;
		private const float TUTORIAL_PANEL_REVEAL_TIME = 0.2f;
		private const float TUTORIAL_PANEL_TEXT_REVEAL_TIME = 2f;
		private const float TUTORIAL_PANEL_TEXT_REVEAL_TOP_LINE_BOOST = 1.5f;
		private const float TUTORIAL_BRACKET_STARTING_OFFSET = 0.5f;
		private const float TUTORIAL_BRACKET_TARGET_OFFSET = 0.275f;
		private const float TUTORIAL_BRACKET_TARGET_Y_SCALE = 0.3f * 1.25f;
		private static float tutorialPanelTime;
		private static float tutorialPanelCloseTime;
		private static float tutorialPanelDiminishTime;
		private static FontString tutorialPanelTitleString;
		private static readonly List<FontString> tutorialPanelTextStrings = new List<FontString>();
		private const float TUTORIAL_TEXT_START_Y = 0.106f;
		private const float TUTORIAL_TEXT_Y_PER_LINE = 0.0225f;
		private const float TUTORIAL_TEXT_TARGET_ALPHA = 0.8f;
		private const float TUTORIAL_PANEL_CLOSE_TIME = 1f;
		private const float TUTORIAL_PANEL_DIMINISH_TIME = 0.25f;
		private const float TUTORIAL_PANEL_DIMINISH_ALPHA = 0.1f;
		private static bool tutorialPanelDiminishedSinceCreation = false;

		private static void DisplayTutorialPanel() {
			DisposeTutorialPanel();
			tutorialPanelClosing = false;
			tutorialPanelDiminished = false;
			tutorialPanelCloseTime = 0f;
			tutorialPanelDiminishTime = 0f;
			HUDSound.PostPassLineReveal.Play(volume: 2f, pitch: 1.4f);
			tutorialPanelTime = 0f;
			tutorialPanelBracketLeft = new HUDTexture(
				AssetLocator.HUDFragmentShader,
				AssetLocator.HudLayer,
				AssetLocator.MainWindow
			) {
				Anchoring = ViewportAnchoring.TopLeft,
				AnchorOffset = new Vector2(TUTORIAL_BRACKET_STARTING_OFFSET, 0.04f),
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.None,
				Color = Vector3.ONE,
				Rotation = 0f,
				Scale = new Vector2(0.07f, 0.0f) * 1.2f,
				Texture = AssetLocator.PreviewImageBracket,
				ZIndex = 300
			};
			tutorialPanelBracketRight = new HUDTexture(
				AssetLocator.HUDFragmentShader,
				AssetLocator.HudLayer,
				AssetLocator.MainWindow
			) {
				Anchoring = ViewportAnchoring.TopRight,
				AnchorOffset = tutorialPanelBracketLeft.AnchorOffset,
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.None,
				Color = Vector3.ONE,
				Rotation = MathUtils.PI,
				Scale = tutorialPanelBracketLeft.Scale,
				Texture = AssetLocator.PreviewImageBracket,
				ZIndex = tutorialPanelBracketLeft.ZIndex
			};
			Vector2 pureScale = new Vector2(1f - TUTORIAL_BRACKET_TARGET_OFFSET * 2f, TUTORIAL_BRACKET_TARGET_Y_SCALE);
			tutorialPanelBackdrop = new HUDTexture(
				AssetLocator.HUDFragmentShader,
				AssetLocator.HudLayer,
				AssetLocator.MainWindow
			) {
				Anchoring = ViewportAnchoring.TopLeft,
				AnchorOffset = tutorialPanelBracketLeft.AnchorOffset + new Vector2(pureScale.X * 0.0f, pureScale.Y * 0.075f),
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.None,
				Color = new Vector4(0.5f, 0.6f, 0.2f, 0.0f) * 1.3f,
				Rotation = 0f,
				Scale = new Vector2(pureScale.X * 1f, pureScale.Y * 0.85f),
				Texture = AssetLocator.PlayMenuMainPanel,
				ZIndex = tutorialPanelBracketLeft.ZIndex - 2
			};

			tutorialPanelTitleString = AssetLocator.TitleFontGlow.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.TopCentered,
				new Vector2(0.0f, 0.05f),
				Vector2.ONE * 0.62f
			);
			tutorialPanelTitleString.Text = TutorialManager.GetTutorialTitle(currentlyLoadedLevelID.LevelIndex);
			tutorialPanelTitleString.Color = new Vector4(0.8f, 1f, 0.5f, 0.0f);

			string[] tutorialPanelText = TutorialManager.GetTutorialText(currentlyLoadedLevelID.LevelIndex).Split(TutorialManager.TUTORIAL_TEXT_LINE_SEPARATOR);
			for (int i = 0; i < tutorialPanelText.Length; ++i) {
				var nextLine = AssetLocator.MainFont.AddString(
					AssetLocator.HudLayer,
					AssetLocator.MainWindow,
					ViewportAnchoring.TopCentered,
					new Vector2(0.0f, TUTORIAL_TEXT_START_Y + TUTORIAL_TEXT_Y_PER_LINE * i),
					Vector2.ONE * 0.3f
				);
				nextLine.Text = tutorialPanelText[i];
				nextLine.Color = new Vector4(1f, 1f, 0.5f, 0.0f);

				tutorialPanelTextStrings.Add(nextLine);
			}

			tutorialPanelActive = true;
			tutorialPanelDiminishedSinceCreation = false;
		}

		private static void DisposeTutorialPanel() {
			tutorialPanelActive = false;

			if (tutorialPanelBracketLeft != null) {
				tutorialPanelBracketLeft.Dispose();
				tutorialPanelBracketLeft = null;
			}
			if (tutorialPanelBracketRight != null) {
				tutorialPanelBracketRight.Dispose();
				tutorialPanelBracketRight = null;
			}
			if (tutorialPanelBackdrop != null) {
				tutorialPanelBackdrop.Dispose();
				tutorialPanelBackdrop = null;
			}
			if (tutorialPanelTitleString != null) {
				tutorialPanelTitleString.Dispose();
				tutorialPanelTitleString = null;
			}
			foreach (var textString in tutorialPanelTextStrings) {
				textString.Dispose();
			}
			tutorialPanelTextStrings.Clear();
		}

		private static void CloseTutorialPanel() {
			tutorialPanelClosing = true;
			tutorialPanelCloseTime = 0f;
		}

		private static void DiminishTutorialPanel() {
			tutorialPanelDiminished = true;
			tutorialPanelDiminishedSinceCreation = true;
			tutorialPanelDiminishTime = Math.Max(0f, tutorialPanelDiminishTime);
		}
		private static void UndiminishTutorialPanel() {
			tutorialPanelDiminished = false;
			tutorialPanelDiminishTime = Math.Min(TUTORIAL_PANEL_DIMINISH_TIME, tutorialPanelDiminishTime);
		}

		private static void TickTutorialHUD(float deltaTime) {
			if (currentGameState == OverallGameState.LevelPaused) return;
			if (tutorialPanelClosing) {
				tutorialPanelCloseTime += deltaTime;
				float closeFracComplete = Math.Min(tutorialPanelCloseTime, TUTORIAL_PANEL_CLOSE_TIME) / TUTORIAL_PANEL_CLOSE_TIME;
				if (closeFracComplete >= 1f) {
					DisposeTutorialPanel();
				}
				else {
					tutorialPanelBracketLeft.SetAlpha(Math.Min(1f - closeFracComplete, tutorialPanelBracketLeft.Color.W));
					tutorialPanelBracketRight.SetAlpha(Math.Min(1f - closeFracComplete, tutorialPanelBracketRight.Color.W));
					tutorialPanelBackdrop.SetAlpha(Math.Min((1f - closeFracComplete) * TUTORIAL_PANEL_BACKDROP_TARGET_ALPHA, tutorialPanelBackdrop.Color.W));
					tutorialPanelTitleString.SetAlpha(Math.Min((1f - closeFracComplete) * TUTORIAL_TEXT_TARGET_ALPHA, tutorialPanelTitleString.Color.W));
					for (int i = 0; i < tutorialPanelTextStrings.Count; ++i) {
						tutorialPanelTextStrings[i].SetAlpha(Math.Min((1f - closeFracComplete) * TUTORIAL_TEXT_TARGET_ALPHA, tutorialPanelTextStrings[i].Color.W));
					}
				}
				return;
			}
			else if (tutorialPanelDiminished || tutorialPanelDiminishTime > 0f) {
				float undiminishOffset;
				if (tutorialPanelDiminishTime > 0.2f) undiminishOffset = 0.05f;
				else if (tutorialPanelDiminishTime > 0.15f) undiminishOffset = 0.1f;
				else undiminishOffset = 0.5f;
				tutorialPanelDiminishTime += deltaTime * (tutorialPanelDiminished ? 1f : (-1f * undiminishOffset));
				float dimFracComplete = (float) MathUtils.Clamp(tutorialPanelDiminishTime, 0f, TUTORIAL_PANEL_DIMINISH_TIME) / TUTORIAL_PANEL_DIMINISH_TIME;
				float dimAlpha = 1f - (1f - TUTORIAL_PANEL_DIMINISH_ALPHA) * dimFracComplete;
				tutorialPanelBracketLeft.SetAlpha(dimAlpha);
				tutorialPanelBracketRight.SetAlpha(dimAlpha);
				tutorialPanelBackdrop.SetAlpha(dimAlpha * TUTORIAL_PANEL_BACKDROP_TARGET_ALPHA);
				tutorialPanelTitleString.SetAlpha(dimAlpha * TUTORIAL_TEXT_TARGET_ALPHA);
				for (int i = 0; i < tutorialPanelTextStrings.Count; ++i) {
					tutorialPanelTextStrings[i].SetAlpha(dimAlpha * TUTORIAL_TEXT_TARGET_ALPHA);
				}
			}

			tutorialPanelTime += deltaTime;
			float revealFracComplete = Math.Min(tutorialPanelTime, TUTORIAL_PANEL_REVEAL_TIME) / TUTORIAL_PANEL_REVEAL_TIME;
			float textRevealFracComplete = 0f;
			if (revealFracComplete >= 1f) {
				textRevealFracComplete = Math.Min(tutorialPanelTime - TUTORIAL_PANEL_REVEAL_TIME, TUTORIAL_PANEL_TEXT_REVEAL_TIME) / TUTORIAL_PANEL_TEXT_REVEAL_TIME;
			}

			if (!tutorialPanelDiminished && tutorialPanelDiminishTime <= 0f) tutorialPanelBackdrop.SetAlpha(TUTORIAL_PANEL_BACKDROP_TARGET_ALPHA * revealFracComplete);
			if (!tutorialPanelDiminished && tutorialPanelDiminishTime <= 0f) tutorialPanelBracketLeft.SetAlpha(revealFracComplete);
			if (!tutorialPanelDiminished && tutorialPanelDiminishTime <= 0f) tutorialPanelBracketRight.SetAlpha(revealFracComplete);

			float anchorOffset = TUTORIAL_BRACKET_STARTING_OFFSET + (TUTORIAL_BRACKET_TARGET_OFFSET - TUTORIAL_BRACKET_STARTING_OFFSET) * revealFracComplete;
			float scale = TUTORIAL_BRACKET_TARGET_Y_SCALE * revealFracComplete;

			tutorialPanelBracketLeft.AnchorOffset = new Vector2(anchorOffset, tutorialPanelBracketLeft.AnchorOffset.Y);
			tutorialPanelBracketRight.AnchorOffset = tutorialPanelBracketLeft.AnchorOffset;
			tutorialPanelBracketLeft.Scale = new Vector2(tutorialPanelBracketLeft.Scale.X, scale);
			tutorialPanelBracketRight.Scale = tutorialPanelBracketLeft.Scale;
			Vector2 pureScale = new Vector2(1f - anchorOffset * 2f, tutorialPanelBracketLeft.Scale.Y);
			tutorialPanelBackdrop.AnchorOffset = tutorialPanelBracketLeft.AnchorOffset + new Vector2(pureScale.X * 0.0f, pureScale.Y * 0.075f);
			tutorialPanelBackdrop.Scale = new Vector2(pureScale.X * 1f, pureScale.Y * 0.85f);

			if (!tutorialPanelDiminished && tutorialPanelDiminishTime <= 0f) tutorialPanelTitleString.SetAlpha(TUTORIAL_TEXT_TARGET_ALPHA * revealFracComplete);

			int lineCount = tutorialPanelTextStrings.Count;
			float boostPerLine = (TUTORIAL_PANEL_TEXT_REVEAL_TOP_LINE_BOOST * textRevealFracComplete) / lineCount;
			for (int i = 0; i < lineCount; ++i) {
				float alphaFrac = textRevealFracComplete + boostPerLine * i;
				if (!tutorialPanelDiminished && tutorialPanelDiminishTime <= 0f) tutorialPanelTextStrings[lineCount - (i + 1)].SetAlpha(Math.Min(alphaFrac, 1f) * TUTORIAL_TEXT_TARGET_ALPHA);
			}
		}
		#endregion

		private static string MakeTimeString(int numMillis) {
			int numFullSeconds = numMillis / 1000;
			int numHundredths = (numMillis - numFullSeconds * 1000) / 10;

			return numFullSeconds.ToString("00") + ":" + numHundredths.ToString("00");
		}
	}
}