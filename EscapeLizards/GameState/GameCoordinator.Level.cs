// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 08 07 2015 at 16:14 by Ben Bowen

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime;
using System.Threading;
using System.Threading.Tasks;
using Ophidian.Losgap;
using Ophidian.Losgap.AssetManagement;
using Ophidian.Losgap.Audio;
using Ophidian.Losgap.Entities;
using Ophidian.Losgap.Input;
using Ophidian.Losgap.Input.XInput;
using Ophidian.Losgap.Rendering;

namespace Egodystonic.EscapeLizards {
	public static partial class GameCoordinator {
		#region Init + Tick
		private static KeyEvent currentRegisteredLevelIntroAccelerationKE = null;
		private static ButtonEvent currentRegisteredLevelIntroAccelerationKEXB = null;
		private static KeyEvent[] currentRegisteredGameplayKeyEvents = null;
		private static ButtonEvent[] currentRegisteredGameplayButtonEvents = null;
		private static StickEvent currentRegisteredGameplayLeftStickEvent = null;
		private static StickEvent currentRegisteredGameplayRightStickEvent = null;
		private static short lastUpdateLSX, lastUpdateLSY;
		private static HUDTexture quickLoadingScreenTop;
		private static HUDTexture quickLoadingScreenBottom;
		private static HUDTexture quickLoadingScreenLogo; private static readonly Vector2 LS_LOGO_OFFSET = new Vector2(0.0f, 0.02f);
		private static HUDTexture quickLoadingScreenWorldIcon; private static readonly Vector2 LS_WORLDICON_OFFSET = new Vector2(0.0f, 0.0f);
		private static bool quickLoadingScreenEnabled = false;
		private static HUDTexture longLoadingScreenImage;
		private static FontString loadingConfirmProgressionString;
		private static HUDTexture loadingConfirmProgressionChevronL;
		private static HUDTexture loadingConfirmProgressionChevronR;
		private static bool longLoadingScreenEnabled = false;
		private static bool longLoadingScreenAwaitingConfirmation = false;
		private static bool loadingChevronsReverseDir = false;
		private static Action loadingScreenTransitionCompleteAction;
		private const float QUICK_LOADING_SCREEN_MOVE_PER_SEC = 2.33f;
		private const float LONG_LOADING_SCREEN_ALPHA_PER_SEC = 1f;
		private const float LOADING_CHEVRON_MIN_OFFSET = 0.25f + 0.085f;
		private const float LOADING_CHEVRON_MAX_OFFSET = 0.275f + 0.085f;
		private const float LOADING_CHATTEL_MAX_ALPHA = 0.88f;

		private static void InitLevelCoordination() {
			Config.ConfigRefresh += () => {
				lock (staticMutationLock) {
					RegisterIntroductionInput();
					RegisterGameplayInput();
					UpdateMiscSettings();
				}
			};
			RegisterIntroductionInput();
			RegisterGameplayInput();
			RegisterInitInput();

			EntityModule.PostTick += PostTick;

			quickLoadingScreenTop = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = ViewportAnchoring.TopCentered,
				AnchorOffset = new Vector2(0f, -1f),
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.None,
				Color = new Vector4(1f, 1f, 1f, 1f),
				Rotation = MathUtils.PI,
				Scale = new Vector2(1f, 1f),
				Texture = AssetLocator.LoadingScreen,
				ZIndex = 100
			};
			quickLoadingScreenBottom = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = ViewportAnchoring.BottomCentered,
				AnchorOffset = new Vector2(0f, -1f),
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.None,
				Color = new Vector4(1f, 1f, 1f, 1f),
				Rotation = 0f,
				Scale = new Vector2(1f, 1f),
				Texture = AssetLocator.LoadingScreen,
				ZIndex = 100
			};

			quickLoadingScreenLogo = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = ViewportAnchoring.TopLeft,
				AnchorOffset = quickLoadingScreenTop.AnchorOffset + LS_LOGO_OFFSET,
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = new Vector4(1f, 1f, 1f, 0.8f),
				Rotation = 0f,
				Scale = Vector2.ONE * 0.4f,
				Texture = AssetLocator.ELLogo,
				ZIndex = quickLoadingScreenTop.ZIndex + 1
			};

			quickLoadingScreenWorldIcon = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = ViewportAnchoring.BottomRight,
				AnchorOffset = quickLoadingScreenBottom.AnchorOffset + LS_WORLDICON_OFFSET,
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = new Vector4(1f, 1f, 1f, 0.8f),
				Rotation = 0f,
				Scale = Vector2.ONE * 0.45f,
				Texture = AssetLocator.WorldIcons[0],
				ZIndex = quickLoadingScreenBottom.ZIndex + 1
			};

			longLoadingScreenImage = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = ViewportAnchoring.Centered,
				AnchorOffset = new Vector2(0f, 0f),
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.None,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = new Vector2(1f, 1f),
				ZIndex = 100
			};

			loadingConfirmProgressionString = AssetLocator.TitleFont.AddString(
				AssetLocator.HudLayer, 
				AssetLocator.MainWindow,
				ViewportAnchoring.BottomCentered, 
				new Vector2(0f, 0.075f), 
				Vector2.ONE * 0.5f
			);
			loadingConfirmProgressionString.Color = Vector4.ZERO;
			loadingConfirmProgressionString.Text = "CONTINUE";

			loadingConfirmProgressionChevronL = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = ViewportAnchoring.BottomLeft,
				AnchorOffset = new Vector2(LOADING_CHEVRON_MIN_OFFSET, 0.063f),
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = Vector2.ONE * 0.06f,
				Texture = AssetLocator.Chevrons,
				ZIndex = 101
			};
			loadingConfirmProgressionChevronR = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = ViewportAnchoring.BottomRight,
				AnchorOffset = loadingConfirmProgressionChevronL.AnchorOffset,
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = MathUtils.PI,
				Scale = loadingConfirmProgressionChevronL.Scale,
				Texture = AssetLocator.Chevrons,
				ZIndex = 101
			};
		}



		public static void EnableQuickLoadingScreen(byte worldIndex, Action completionAction = null) {
			lock (staticMutationLock) {
				quickLoadingScreenWorldIcon.Texture = AssetLocator.WorldIcons[worldIndex];
				quickLoadingScreenEnabled = true;
				loadingScreenTransitionCompleteAction = completionAction;
			}
		}
		public static void DisableQuickLoadingScreen(Action completionAction = null) {
			lock (staticMutationLock) {
				quickLoadingScreenEnabled = false;
				loadingScreenTransitionCompleteAction = completionAction;
			}
		}

		public static void EnableLongLoadingScreen(ITexture2D backdrop, Action completionAction = null) {
			lock (staticMutationLock) {
				longLoadingScreenImage.Texture = backdrop;
				longLoadingScreenEnabled = true;
				longLoadingScreenAwaitingConfirmation = false;
				loadingScreenTransitionCompleteAction = completionAction;
			}
		}
		public static void DisableLongLoadingScreen(Vector3 continueStringColor, Action completionAction = null) {
			lock (staticMutationLock) {
				loadingConfirmProgressionString.Color = continueStringColor;
				longLoadingScreenAwaitingConfirmation = true;
				loadingScreenTransitionCompleteAction = completionAction;

				int instanceId = AudioModule.CreateSoundInstance(AssetLocator.PostPassAllEggsSound);
				AudioModule.PlaySoundInstance(
					instanceId,
					false,
					1f * Config.HUDVolume,
					1f
				);
			}
		}

		private static void TickLoadingScreen(float deltaTime) {
			if (deltaTime > 0.05f) deltaTime = 0.05f;
			bool quickTransitionStillActive = false;
			if (quickLoadingScreenEnabled) {
				bool bothHalvesComplete = false;

				if (quickLoadingScreenTop.AnchorOffset.Y < 0f) {
					quickTransitionStillActive = true;
					quickLoadingScreenTop.AnchorOffset = new Vector2(0f, quickLoadingScreenTop.AnchorOffset.Y + QUICK_LOADING_SCREEN_MOVE_PER_SEC * deltaTime);
					if (quickLoadingScreenTop.AnchorOffset.Y > 0f) {
						bothHalvesComplete = true;
						quickLoadingScreenTop.AnchorOffset = new Vector2(0f, 0f);
					}
				}
				if (quickLoadingScreenBottom.AnchorOffset.Y < 0f) {
					quickTransitionStillActive = true;
					quickLoadingScreenBottom.AnchorOffset = new Vector2(0f, quickLoadingScreenBottom.AnchorOffset.Y + QUICK_LOADING_SCREEN_MOVE_PER_SEC * deltaTime);
					if (quickLoadingScreenBottom.AnchorOffset.Y > 0f) {
						quickLoadingScreenBottom.AnchorOffset = new Vector2(0f, 0f);
					}
					else bothHalvesComplete = false;
				}

				if (quickTransitionStillActive && bothHalvesComplete) {
					var completionActionLocal = loadingScreenTransitionCompleteAction;
					loadingScreenTransitionCompleteAction = null;
					if (completionActionLocal != null) completionActionLocal();
				}
			}
			else {
				bool bothHalvesComplete = false;

				if (quickLoadingScreenTop.AnchorOffset.Y > -1f) {
					quickTransitionStillActive = true;
					quickLoadingScreenTop.AnchorOffset = new Vector2(0f, quickLoadingScreenTop.AnchorOffset.Y - QUICK_LOADING_SCREEN_MOVE_PER_SEC * deltaTime);
					if (quickLoadingScreenTop.AnchorOffset.Y < -1f) {
						bothHalvesComplete = true;
						quickLoadingScreenTop.AnchorOffset = new Vector2(0f, -1f);
					}
				}
				if (quickLoadingScreenBottom.AnchorOffset.Y > -1f) {
					quickTransitionStillActive = true;
					quickLoadingScreenBottom.AnchorOffset = new Vector2(0f, quickLoadingScreenBottom.AnchorOffset.Y - QUICK_LOADING_SCREEN_MOVE_PER_SEC * deltaTime);
					if (quickLoadingScreenBottom.AnchorOffset.Y < -1f) {
						quickLoadingScreenBottom.AnchorOffset = new Vector2(0f, -1f);
					}
					else bothHalvesComplete = false;
				}

				if (quickTransitionStillActive && bothHalvesComplete) {
					var completionActionLocal = loadingScreenTransitionCompleteAction;
					loadingScreenTransitionCompleteAction = null;
					if (completionActionLocal != null) completionActionLocal();
				}
			}

			if (quickTransitionStillActive) {
				quickLoadingScreenLogo.AnchorOffset = quickLoadingScreenTop.AnchorOffset + LS_LOGO_OFFSET;
				quickLoadingScreenWorldIcon.AnchorOffset = quickLoadingScreenBottom.AnchorOffset + LS_WORLDICON_OFFSET;
			}

			if (longLoadingScreenEnabled) {
				if (longLoadingScreenImage.Color.W < 1f) {
					longLoadingScreenImage.AdjustAlpha(LONG_LOADING_SCREEN_ALPHA_PER_SEC * deltaTime);
					if (longLoadingScreenImage.Color.W >= 1f) {
						var completionActionLocal = loadingScreenTransitionCompleteAction;
						loadingScreenTransitionCompleteAction = null;
						if (completionActionLocal != null) completionActionLocal();
					}
				}
			}
			else {
				if (longLoadingScreenImage.Color.W > 0f) {
					longLoadingScreenImage.AdjustAlpha(-LONG_LOADING_SCREEN_ALPHA_PER_SEC * deltaTime);
					if (longLoadingScreenImage.Color.W <= 0f) {
						longLoadingScreenImage.Texture = null;
					}

					loadingConfirmProgressionString.AdjustAlpha(-LONG_LOADING_SCREEN_ALPHA_PER_SEC * deltaTime * 2f);
					loadingConfirmProgressionChevronL.SetAlpha(loadingConfirmProgressionString.Color.W);
					loadingConfirmProgressionChevronR.SetAlpha(loadingConfirmProgressionString.Color.W);

					float chevronAdjust = deltaTime * 0.1f;
					float newX;
					if (loadingChevronsReverseDir) {
						newX = loadingConfirmProgressionChevronL.AnchorOffset.X - chevronAdjust;
						if (newX < LOADING_CHEVRON_MIN_OFFSET) {
							newX += (LOADING_CHEVRON_MIN_OFFSET - newX) * 2f;
							loadingChevronsReverseDir = !loadingChevronsReverseDir;
						}
					}
					else {
						newX = loadingConfirmProgressionChevronL.AnchorOffset.X + chevronAdjust;
						if (newX > LOADING_CHEVRON_MAX_OFFSET) {
							newX -= (newX - LOADING_CHEVRON_MAX_OFFSET) * 2f;
							loadingChevronsReverseDir = !loadingChevronsReverseDir;
						}
					}
					
					loadingConfirmProgressionChevronL.AnchorOffset = new Vector2(newX, loadingConfirmProgressionChevronL.AnchorOffset.Y);
					loadingConfirmProgressionChevronR.AnchorOffset = new Vector2(newX, loadingConfirmProgressionChevronR.AnchorOffset.Y);
				}
			}

			if (longLoadingScreenAwaitingConfirmation) {
				float alphaAdjust = deltaTime * 2f;
				if (loadingConfirmProgressionString.Color.W < LOADING_CHATTEL_MAX_ALPHA) {
					loadingConfirmProgressionString.AdjustAlpha(alphaAdjust);
					if (loadingConfirmProgressionString.Color.W > LOADING_CHATTEL_MAX_ALPHA) loadingConfirmProgressionString.SetAlpha(LOADING_CHATTEL_MAX_ALPHA);
					loadingConfirmProgressionChevronL.SetAlpha(loadingConfirmProgressionString.Color.W);
					loadingConfirmProgressionChevronR.SetAlpha(loadingConfirmProgressionString.Color.W);
				}

				float chevronAdjust = deltaTime * 0.1f;
				float newX;
				if (loadingChevronsReverseDir) {
					newX = loadingConfirmProgressionChevronL.AnchorOffset.X - chevronAdjust;
					if (newX < LOADING_CHEVRON_MIN_OFFSET) {
						newX += (LOADING_CHEVRON_MIN_OFFSET - newX) * 2f;
						loadingChevronsReverseDir = !loadingChevronsReverseDir;
					}
				}
				else {
					newX = loadingConfirmProgressionChevronL.AnchorOffset.X + chevronAdjust;
					if (newX > LOADING_CHEVRON_MAX_OFFSET) {
						newX -= (newX - LOADING_CHEVRON_MAX_OFFSET) * 2f;
						loadingChevronsReverseDir = !loadingChevronsReverseDir;
					}
				}

				loadingConfirmProgressionChevronL.AnchorOffset = new Vector2(newX, loadingConfirmProgressionChevronL.AnchorOffset.Y);
				loadingConfirmProgressionChevronR.AnchorOffset = new Vector2(newX, loadingConfirmProgressionChevronR.AnchorOffset.Y);
			}
		}

		private static void ContinueLongLoadingScreen() {
			if (longLoadingScreenAwaitingConfirmation) {
				longLoadingScreenAwaitingConfirmation = false;
				longLoadingScreenEnabled = false;
				var actionLocal = loadingScreenTransitionCompleteAction;
				if (actionLocal != null) actionLocal();
				loadingScreenTransitionCompleteAction = null;

				int instanceId = AudioModule.CreateSoundInstance(AssetLocator.UIOptionSelectSound);
				AudioModule.PlaySoundInstance(
					instanceId,
					false,
					1f * Config.HUDVolume,
					1f
				);
			}
		}

		private static void RegisterIntroductionInput() {
			var hudIPB = AssetLocator.HudLayer.GetResource<InputBindingRegistry>();
			if (currentRegisteredLevelIntroAccelerationKE != null) hudIPB.UnregisterKeyEvent(currentRegisteredLevelIntroAccelerationKE);
			currentRegisteredLevelIntroAccelerationKE = hudIPB.RegisterKeyEvent(
				new HashSet<VirtualKey> {
					Config.Key_AccelerateLevelIntroduction
				},
				VirtualKeyState.KeyDown | VirtualKeyState.KeyUp,
				(_, state) => {
					lock (staticMutationLock) {
						if (currentGameState != OverallGameState.LevelIntroduction) return;
						accelerateIntroductions = state == VirtualKeyState.KeyDown;
					}
				}
			);

			if (currentRegisteredLevelIntroAccelerationKEXB != null) hudIPB.UnregisterButtonEvent(currentRegisteredLevelIntroAccelerationKEXB);
			currentRegisteredLevelIntroAccelerationKEXB = hudIPB.RegisterButtonEvent(
				new HashSet<ButtonFlags> {
					ButtonFlags.XINPUT_GAMEPAD_A, ButtonFlags.XINPUT_GAMEPAD_B
				},
				VirtualKeyState.KeyDown | VirtualKeyState.KeyUp,
				(_, state, __) => {
					lock (staticMutationLock) {
						if (currentGameState != OverallGameState.LevelIntroduction) return;
						accelerateIntroductions = state == VirtualKeyState.KeyDown;
					}
				}
			);
		}

		private static void RegisterInitInput() {
			var hudIPB = AssetLocator.HudLayer.GetResource<InputBindingRegistry>();
			hudIPB.RegisterKeyEvent(
				new HashSet<VirtualKey> {
					VirtualKey.Space, VirtualKey.Enter
				},
				VirtualKeyState.KeyDown,
				(_, __) => {
					lock (staticMutationLock) {
						if (currentGameState != OverallGameState.InMenuSystem) return;
						ContinueLongLoadingScreen();
					}
				}
			);
			hudIPB.RegisterButtonEvent(
				new HashSet<ButtonFlags> {
					ButtonFlags.XINPUT_GAMEPAD_A, ButtonFlags.XINPUT_GAMEPAD_B, ButtonFlags.XINPUT_GAMEPAD_START
				},
				VirtualKeyState.KeyDown,
				(_, __, ___) => {
					lock (staticMutationLock) {
						if (currentGameState != OverallGameState.InMenuSystem) return;
						ContinueLongLoadingScreen();
					}
				}
			);
		}

		private static void RegisterGameplayInput() {
			var gameIPB = AssetLocator.GameLayer.GetResource<InputBindingRegistry>();

			#region Keyboard
			if (currentRegisteredGameplayKeyEvents != null) {
				foreach (var gameplayKeyEvent in currentRegisteredGameplayKeyEvents) {
					gameIPB.UnregisterKeyEvent(gameplayKeyEvent);
				}
			}
			currentRegisteredGameplayKeyEvents = new[] {
				gameIPB.RegisterKeyEvent(
					new HashSet<VirtualKey> {
						Config.Key_TiltGameboardForward,
						Config.Key_TiltGameboardBackward,
						Config.Key_TiltGameboardRight,
						Config.Key_TiltGameboardLeft
					},
					VirtualKeyState.KeyDown | VirtualKeyState.KeyUp,
					(key, state) => {
						lock (staticMutationLock) {
							if (currentGameState != OverallGameState.LevelPlaying) return;
							bool reversal = state == VirtualKeyState.KeyUp;
							if (key == Config.Key_TiltGameboardForward) TiltGameboard(GameboardTiltDirection.Forward, reversal);
							else if (key == Config.Key_TiltGameboardBackward) TiltGameboard(GameboardTiltDirection.Backward, reversal);
							else if (key == Config.Key_TiltGameboardLeft) TiltGameboard(GameboardTiltDirection.Left, reversal);
							else if (key == Config.Key_TiltGameboardRight) TiltGameboard(GameboardTiltDirection.Right, reversal);
						}
					}
				),
				gameIPB.RegisterKeyEvent(
					new HashSet<VirtualKey> {
						Config.Key_FlipGameboardLeft,
						Config.Key_FlipGameboardRight
					},
					VirtualKeyState.KeyDown,
					(key, state) => {
						lock (staticMutationLock) {
							if (currentGameState != OverallGameState.LevelPlaying) return;
							FlipGameboard(key == Config.Key_FlipGameboardRight);
						}
					}
				),
				gameIPB.RegisterKeyEvent(
					new HashSet<VirtualKey> {
						Config.Key_HopEgg
					},
					VirtualKeyState.KeyDown,
					(key, state) => {
						lock (staticMutationLock) {
							if (currentGameState != OverallGameState.LevelPlaying) return;
							HopEgg();
						}
					}
				),
				gameIPB.RegisterKeyEvent(
					new HashSet<VirtualKey> { Config.Key_FastRestart }, VirtualKeyState.KeyDown, (key, state) => {
						lock (staticMutationLock) {
							switch (currentGameState) {
								case OverallGameState.InMenuSystem: return;
								case OverallGameState.LevelPaused: return;
								case OverallGameState.LevelIntroduction: break;
								default:
									StartLevelIntroduction();
									break;
							}

							SkipToIntroEnd();
						}
					}
				),
				gameIPB.RegisterKeyEvent(
					new HashSet<VirtualKey> { Config.Key_TiltGameboardForward, Config.Key_TiltGameboardBackward }, VirtualKeyState.KeyDown, (key, state) => {
						lock (staticMutationLock) {
							switch (currentGameState) {
								case OverallGameState.LevelPaused:
									PauseSwitchOption(key == Config.Key_TiltGameboardBackward);
									break;
								case OverallGameState.LevelPassed:
								case OverallGameState.LevelFailed:
									SwitchPostGameOption(key == Config.Key_TiltGameboardBackward);
									break;
							}
						}
					}
				),
				gameIPB.RegisterKeyEvent(
					new HashSet<VirtualKey> { Config.Key_TiltGameboardLeft, Config.Key_TiltGameboardRight }, VirtualKeyState.KeyDown, (key, state) => {
						lock (staticMutationLock) {
							switch (currentGameState) {
								case OverallGameState.LevelPaused:
									PauseLeftRight();
									break;
							}
						}
					}
				),
				gameIPB.RegisterKeyEvent(
					new HashSet<VirtualKey> { Config.Key_AccelerateLevelIntroduction, VirtualKey.Enter }, VirtualKeyState.KeyDown, (key, state) => {
						lock (staticMutationLock) {
							switch (currentGameState) {
								case OverallGameState.LevelPaused:
									PauseActionSelectedOption();
									break;
								case OverallGameState.LevelPassed:
								case OverallGameState.LevelFailed:
									SelectPostGameOption();
									break;
							}
						}
					}
				),
				gameIPB.RegisterKeyEvent(
					new HashSet<VirtualKey> { VirtualKey.Escape, VirtualKey.Back }, VirtualKeyState.KeyDown, (key, state) => {
						lock (staticMutationLock) {
							switch (currentGameState) {
								case OverallGameState.LevelPlaying: ActivatePauseMenu(); break;
								case OverallGameState.LevelPaused: PauseBackOut(); break;
							}
						}
					}
				),
			};
			#endregion

			#region XBOX
			if (currentRegisteredGameplayButtonEvents != null) {
				foreach (var gameplayKeyEvent in currentRegisteredGameplayButtonEvents) {
					gameIPB.UnregisterButtonEvent(gameplayKeyEvent);
				}
			}
			if (currentRegisteredGameplayLeftStickEvent != null) {
				gameIPB.UnregisterLeftStickEvent();
			}
			if (currentRegisteredGameplayRightStickEvent != null) {
				gameIPB.UnregisterRightStickEvent();
			}

			currentRegisteredGameplayLeftStickEvent = gameIPB.RegisterLeftStickEvent((x, y, _) => {
				switch (currentGameState) {
					case OverallGameState.LevelIntroduction:
						lastUpdateLSX = x;
						lastUpdateLSY = y;
						break;
					case OverallGameState.LevelPlaying:
						float tiltDeadzone = Config.BoardTiltDeadZone;
						float maxAxialTilt = 32767f - tiltDeadzone;
						bool xDead = false;
						bool yDead = false;
						if (x < -tiltDeadzone) {
							TiltGameboard(GameboardTiltDirection.Left, (x + tiltDeadzone) / -maxAxialTilt);
						}
						else if (x > tiltDeadzone) {
							TiltGameboard(GameboardTiltDirection.Right, (x - tiltDeadzone) / maxAxialTilt);
						}
						else xDead = true;
						if (y < -tiltDeadzone) {
							TiltGameboard(GameboardTiltDirection.Backward, (y + tiltDeadzone) / -maxAxialTilt);
						}
						else if (y > tiltDeadzone) {
							TiltGameboard(GameboardTiltDirection.Forward, (y - tiltDeadzone) / maxAxialTilt); ;
						}
						else yDead = true;

						if (xDead && yDead) EraseTilt();

						lastUpdateLSX = x;
						lastUpdateLSY = y;
						break;
					case OverallGameState.LevelPassed:
					case OverallGameState.LevelFailed:
						if (Math.Abs((float) y) >= Config.OptionSelectThreshold && Math.Abs((float) lastUpdateLSY) < Config.OptionSelectThreshold) {
							SwitchPostGameOption(y < 0);
						}

						lastUpdateLSX = x;
						lastUpdateLSY = y;
						break;
					case OverallGameState.LevelPaused:
						if (Math.Abs((float) y) >= Config.OptionSelectThreshold && Math.Abs((float) lastUpdateLSY) < Config.OptionSelectThreshold) {
							PauseSwitchOption(y < 0);
						}
						if (Math.Abs((float) x) >= Config.OptionSelectThreshold && Math.Abs((float) lastUpdateLSX) < Config.OptionSelectThreshold) {
							PauseLeftRight();
						}

						lastUpdateLSX = x;
						lastUpdateLSY = y;
						break;
				}
			});

			currentRegisteredGameplayRightStickEvent = gameIPB.RegisterRightStickEvent((x, y, _) => {
				if (currentGameState != OverallGameState.LevelPlaying) return;

				float camOrientDeadzone = Config.CamOrientDeadZone;

				if (Math.Abs((float) x) <= camOrientDeadzone) x = 0;
				else x = (short) (x - (x < 0 ? -camOrientDeadzone : camOrientDeadzone));

				if (Math.Abs((float) y) <= camOrientDeadzone) y = 0;
				else y = (short) (y - (y < 0 ? -camOrientDeadzone : camOrientDeadzone));

				float xAmount = x / (32768f - camOrientDeadzone);
				float yAmount = y / (32768f - camOrientDeadzone);

				if (Math.Abs(xAmount) >= 0.9f) xAmount *= 3f;
				else if (Math.Abs(xAmount) >= 0.7f) xAmount *= 1.5f;
				if (Math.Abs(yAmount) >= 0.9f) yAmount *= 3f;
				else if (Math.Abs(yAmount) >= 0.7f) yAmount *= 1.5f;

				eggBoom.SetCamOrientationAlteration(-xAmount, yAmount * (Config.InvertCamControlY ? 1f : -1f));
			});

			currentRegisteredGameplayButtonEvents = new[] {
				gameIPB.RegisterButtonEvent(
					new HashSet<ButtonFlags>() {
						ButtonFlags.XINPUT_GAMEPAD_LEFT_SHOULDER,
						ButtonFlags.XINPUT_GAMEPAD_RIGHT_SHOULDER
					},
					VirtualKeyState.KeyDown,
					(button, _, __) => {
						if (currentGameState != OverallGameState.LevelPlaying) return;
						FlipGameboard(button == ButtonFlags.XINPUT_GAMEPAD_LEFT_SHOULDER);
					}
				),
				gameIPB.RegisterButtonEvent(
					new HashSet<ButtonFlags>() {
						ButtonFlags.XINPUT_GAMEPAD_A
					},
					VirtualKeyState.KeyDown,
					(button, _, __) => {
						if (currentGameState != OverallGameState.LevelPlaying) return;
						HopEgg();
					}
				),
				gameIPB.RegisterButtonEvent(
					new HashSet<ButtonFlags> { ButtonFlags.XINPUT_GAMEPAD_A }, VirtualKeyState.KeyDown, (_, __, ___) => {
						lock (staticMutationLock) {
							switch (currentGameState) {
								case OverallGameState.LevelPaused:
									PauseActionSelectedOption();
									break;
								case OverallGameState.LevelPassed:
								case OverallGameState.LevelFailed:
									SelectPostGameOption();
									break;
							}
						}
					}
				),
				gameIPB.RegisterButtonEvent(
					new HashSet<ButtonFlags> { ButtonFlags.XINPUT_GAMEPAD_B }, VirtualKeyState.KeyDown, (_, __, ___) => {
						lock (staticMutationLock) {
							switch (currentGameState) {
								case OverallGameState.InMenuSystem: return;
								case OverallGameState.LevelPaused:
									PauseBackOut();
									return;
								case OverallGameState.LevelIntroduction: break;
								default:
									StartLevelIntroduction();
									break;
							}

							SkipToIntroEnd();
						}
					}
				),
				gameIPB.RegisterButtonEvent(
					new HashSet<ButtonFlags> { ButtonFlags.XINPUT_GAMEPAD_DPAD_DOWN, ButtonFlags.XINPUT_GAMEPAD_DPAD_UP }, VirtualKeyState.KeyDown, (button, __, ___) => {
						lock (staticMutationLock) {
							switch (currentGameState) {
								case OverallGameState.LevelPaused: 
									PauseSwitchOption(button == ButtonFlags.XINPUT_GAMEPAD_DPAD_DOWN);
									break;
								case OverallGameState.LevelPassed:
								case OverallGameState.LevelFailed:
									SwitchPostGameOption(button == ButtonFlags.XINPUT_GAMEPAD_DPAD_DOWN);
									break;
							}
						}
					}
				),
				gameIPB.RegisterButtonEvent(
					new HashSet<ButtonFlags> { ButtonFlags.XINPUT_GAMEPAD_DPAD_LEFT, ButtonFlags.XINPUT_GAMEPAD_DPAD_RIGHT }, VirtualKeyState.KeyDown, (button, __, ___) => {
						lock (staticMutationLock) {
							switch (currentGameState) {
								case OverallGameState.LevelPaused: 
									PauseLeftRight();
									break;
							}
						}
					}
				),
				gameIPB.RegisterButtonEvent(
					new HashSet<ButtonFlags> { ButtonFlags.XINPUT_GAMEPAD_START }, VirtualKeyState.KeyDown, (button, __, ___) => {
						lock (staticMutationLock) {
							switch (currentGameState) {
								case OverallGameState.LevelPlaying: ActivatePauseMenu(); break;
								case OverallGameState.LevelPaused: PauseBackOut(); break;
							}
						}
					}
				)
			};
			#endregion
		}

		private static void PostTick(float deltaTime) {
			lock (staticMutationLock) { // Lock because it's called from an event
				TickLoadingScreen(deltaTime);
				switch (currentGameState) {
					case OverallGameState.InMenuSystem: return;
					case OverallGameState.LevelIntroduction: TickIntroduction(deltaTime); break;
					case OverallGameState.LevelPlaying: 
						TickBoardManipulation(deltaTime);
						TickPhysicsHacks(deltaTime);
						TickGame(deltaTime);
						HUD_TickPostIntro(deltaTime);
						break;
					case OverallGameState.LevelPassed:
						TickBoardManipulation(deltaTime);
						TickPostPass(deltaTime);
						TickPhysicsHacks(deltaTime);
						HUD_TickPostIntro(deltaTime);
						break;
					case OverallGameState.LevelFailed:
						TickBoardManipulation(deltaTime);
						TickPostFail(deltaTime);
						TickPhysicsHacks(deltaTime);
						HUD_TickPostIntro(deltaTime);
						break;
					case OverallGameState.LevelPaused:
						TickPauseMenu(deltaTime);
						break;
				}
				TickHUD(deltaTime);
			}
		}

		private static void UpdateMiscSettings() {
			lock (staticMutationLock) {
				if (eggBoom != null && !eggBoom.IsDisposed) {
					AssetLocator.LightPass.DynamicLightCap = Config.DynamicLightCap;

					eggBoom.Dispose();
					eggBoom = new EggCameraBoom(
						egg, 
						AssetLocator.MainCamera,
						GameplayConstants.CAMERA_HEIGHT_ADDITION + Config.CameraHeightOffset,
						GameplayConstants.CAMERA_MIN_VELOCITY_BASED_DIST_TO_EGG + Config.CameraDistanceOffset
					);
				}
			}
		}
		#endregion

		#region Load / Unload
		private static LevelID currentlyLoadedLevelID = new LevelID(255, 255);
		private static GameLevelDescription currentGameLevel = null;
		private static SkyLevelDescription currentSkyLevel = null;
		private static bool currentLevelDataIsBaked = false;
		private static bool currentLevelIsEusocial = false;
		private static readonly List<EusocialMovementTrack> eusocialMovementTracks = new List<EusocialMovementTrack>();
		private static readonly List<EusocialMovementTrack> eusocialWorkspace = new List<EusocialMovementTrack>();
		private const int EUSOCIAL_MOVEMENT_WAVE_TIME = 1;
		private const float EUSOCIAL_MOVEMENT_TIME = 1f;
		private const int EUSOCIAL_MOVEMENT_WAVE_COUNT = 15;
		private const int EUSOCIAL_PRNG_SEED = 56712;
		private static Random eusocialPRNG;
		private const float EUSOCIAL_MOVEMENT_DISTANCE = PhysicsManager.ONE_METRE_SCALED * 7f;
		private static readonly List<GeometryEntity> eusocialMovableEntities = new List<GeometryEntity>();
		private static Vector3[] MAJOR_AXES = { Vector3.UP, Vector3.DOWN, Vector3.LEFT, Vector3.RIGHT, Vector3.FORWARD, Vector3.BACKWARD };

		public static GameLevelDescription CurrentlyLoadedLevel {
			get {
				lock (staticMutationLock) {
					return currentGameLevel;
				}
			}
		}

		public static LevelID CurrentlyLoadedLevelID {
			get {
				lock (staticMutationLock) {
					return currentlyLoadedLevelID;
				}
			}
		}

		public static void ProgressToNextLevel() {
			lock (staticMutationLock) {
				LevelID? nextLevelID = CurrentlyLoadedLevelID.NextLevelInWorld;
				if (nextLevelID == null) throw new InvalidOperationException("Can not progress: Current level is last in world.");

				currentGameState = OverallGameState.InMenuSystem;
				Sounds_LevelProgress();
				DisposeHUD(true);

				EnableQuickLoadingScreen(nextLevelID.Value.WorldIndex, () => EntityModule.AddTimedAction(() => {
							LoadLevel(nextLevelID.Value);
							StartLevelIntroduction();
							DisableQuickLoadingScreen();
						}, 
						0.5f
					)
				);
			}
		}

		public static void PassMenuBackdropAsSkybox(SkyLevelDescription sld) {
			lock (staticMutationLock) {
				if (currentSkyLevel != null && currentSkyLevel != sld) currentSkyLevel.Dispose();
				currentSkyLevel = sld;
			}
		}

		public static void LoadLevel(LevelID id) {
			lock (staticMutationLock) {
				PhysicsManager.SetPhysicsTickrate(5f); // Lowers loading times, weirdly

				string currentLevelSkyFileName = null;
				if (currentGameLevel != null) {
					currentLevelSkyFileName = currentGameLevel.SkyboxFileName;
					currentGameLevel.Dispose();
				}

				string fileName = LevelDatabase.GetLevelFileName(id);
				currentLevelDataIsBaked = false;
				currentlyLoadedLevelID = id;
				Task<LevelLeaderboardDetails> leaderboardInfoTask = AsyncLoadSocialMetadata(id);

				try {
					currentGameLevel = (GameLevelDescription) LevelDescription.Load(Path.Combine(AssetLocator.LevelsDir, fileName));
				}
				catch (Exception e) {
					throw new FileNotFoundException("Could not find or load the game level '" + fileName + "': Please verify your game's installation/cache.", e);
				}

				currentLevelIsEusocial = currentGameLevel.Title == "Eusocial";

				try {
					if (currentLevelSkyFileName != currentGameLevel.SkyboxFileName) {
						if (currentSkyLevel != null) currentSkyLevel.Dispose();
						AssetLoader.ClearCache();
						WorldModelCache.ClearAndDisposeCache();
						currentBoardDrift = Vector3.ZERO;
						currentSkyLevel = (SkyLevelDescription) LevelDescription.Load(Path.Combine(AssetLocator.LevelsDir, currentGameLevel.SkyboxFileName));
					}
				}
				catch (Exception e) {
					throw new FileNotFoundException("Could not find or load the skybox '" + currentGameLevel.SkyboxFileName + "': Please verify your game's installation/cache.", e);
				}

				ResetGameboard();

				currentLevelLeaderboardDetails = leaderboardInfoTask.Result;

				PhysicsManager.SetPhysicsTickrate(Config.GetTickrateForPhysicsLevel(Config.PhysicsLevel));

				GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
				GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
			}
		}

		private static void UnloadToMainMenu(MenuCoordinator.MenuState targetMenu = MenuCoordinator.MenuState.MainMenu) {
			ResetGameboard();
			AssetLocator.LightPass.DynamicLightCap = null;
			currentGameState = OverallGameState.InMenuSystem;
			if (currentGameLevel != null) {
				currentGameLevel.Dispose();
				currentGameLevel = null;
			}
			currentLevelDataIsBaked = false;
			lastIntroID = new LevelID(255, 255);

			Sounds_LevelFail(LevelFailReason.GameCancelled);
			DisposeHUD();
			DisposeTutorialPanel();

			MenuCoordinator.LoadMenuFrontend(currentSkyLevel, targetMenu);
		}

		private static void DeferToMainMenu(Action menuCompletionCallback, MenuCoordinator.MenuState targetMenu = MenuCoordinator.MenuState.MainMenu) {
			ClearHUDIntroElements();
			currentGameState = OverallGameState.InMenuSystem;
			
			MenuCoordinator.LoadMenuFrontend(currentSkyLevel, targetMenu, () => {
				currentGameState = OverallGameState.LevelPaused;
				menuCompletionCallback();
			});
		}

		public static bool BakeLoadedLevelIfNecessary() {
			if (currentLevelDataIsBaked) return false;
			if (currentGameLevel == null || currentSkyLevel == null) throw new InvalidOperationException("Can not bake level: No level or no skybox loaded.");

			currentSkyLevel.ReinitializeAll();
			currentGameLevel.ReinitializeAll();
			currentGameLevel.PerformCalculations();	

			currentLevelDataIsBaked = true;

			return true;
		}
		#endregion

		#region Board Control
		private static readonly Vector3[] primaryAxes = {
			Vector3.UP, Vector3.DOWN, Vector3.LEFT, Vector3.FORWARD, Vector3.RIGHT, Vector3.BACKWARD
		};
		private static readonly Vector3[] primaryOrthogonals = {
			Vector3.LEFT, Vector3.RIGHT, Vector3.FORWARD, Vector3.UP, Vector3.BACKWARD, Vector3.DOWN
		};
		private static readonly List<GameboardTiltData> activeTilts = new List<GameboardTiltData>();
		private static readonly List<GameboardTiltData> activeTiltRemovalWorkspace = new List<GameboardTiltData>();
		private static readonly Sphere vultureEggSphere = new Sphere(Vector3.ZERO, GameplayConstants.VULTURE_EGG_COLLISION_RADIUS_MULTIPLIER * GameplayConstants.EGG_COLLISION_RADIUS);
		private static Quaternion targetTilt;
		private static Quaternion currentTilt;
		private static EggEntity egg;
		private static EggCameraBoom eggBoom;
		private static Vector3 boardDownDir = Vector3.DOWN;
		private static Vector3[] ballGroundPlanarProbeVectors = new Vector3[3];
		private static Quaternion boardForwardToRightQuat;
		private static Plane groundPlane;
		private static GameboardFlipData? currentFlipInProgress;
		private static float currentFlipElapsedTime;
		private static IntroCameraAttracterEntity cameraAttracterEntity;
		private static Vector3 initialCamAttracterPos;
		private static Vector3 currentBoardDrift;
		private static Vector3 currentCamAttracterPos;
		private static Cuboid fallOutCuboid;
		private static readonly Dictionary<VultureEggEntity, Vector3> vultureEggEntities = new Dictionary<VultureEggEntity, Vector3>();
		private static readonly Dictionary<VultureEggEntity, Vector3> newPosWorkspace = new Dictionary<VultureEggEntity, Vector3>();
		private static readonly Dictionary<FinishingBellEntity, FinishingBellTopEntity> finishingBells = new Dictionary<FinishingBellEntity, FinishingBellTopEntity>();
		private static readonly Dictionary<VultureEggEntity, Vector3> vultureEggStaticPos = new Dictionary<VultureEggEntity, Vector3>();
		private const float FLIP_ARROW_SCALE = 2f;
		private const float FLIP_ARROW_FORWARD_START_Y_SCALE = 1f;
		private const float FLIP_ARROW_FORWARD_END_Y_SCALE = 4f;
		private static readonly List<KeyValuePair<Entity, Material>> flipArrows = new List<KeyValuePair<Entity, Material>>();
		private const int FLIP_ARROW_ROW_HALF_LEN = 5;
		private const float FLIP_ARROW_SPACING = PhysicsManager.ONE_METRE_SCALED * 0.15f;
		private const float FLIP_TIME = GameplayConstants.BOARD_FLIP_TIME * 2f;
		private const float FLIP_ARROW_MIN_FADEOUT_TIME = 0.45f;
		private const float MAX_FLIP_ARROW_ALPHA = 0.5f;
		private static Vector3 eggPosLastFrame;
		private static float timeSinceLastFlip;
		private static GameboardFlipData lastFlipData;
		private static LizardEggMaterial userSelectedEggMat = LizardEggMaterial.LizardEgg;
		private static readonly List<int> takenCoinIndices = new List<int>();
		private static int numCoinsTaken;
		private static int numCoinsTakenBeforeStart;
		private static int numFreshCoinsTaken;
		private static int numCoinsInLevel;
		private static readonly Dictionary<LizardCoinEntity, Vector3> lizardCoinStaticPos = new Dictionary<LizardCoinEntity, Vector3>();
		private static IEnumerable<int> previouslyTakenCoinIndices;
		private static int numHopsRemaining;
		private static bool hopActive;
		private static bool usedDoubleHop;
		private static readonly List<GameboardTiltData> tiltRecalcWorkspace = new List<GameboardTiltData>();

		public static bool HopInProgress {
			get {
				lock (staticMutationLock) {
					return hopActive;
				}
			}
		}

		public static Vector3 BoardDownDir {
			get {
				lock (staticMutationLock) {
					return boardDownDir;
				}
			}
		}
		public static Plane GroundPlane {
			get {
				lock (staticMutationLock) {
					return groundPlane;
				}
			}
		}

		public static Quaternion CurrentGameboardTilt {
			get {
				lock (staticMutationLock) {
					return currentTilt;
				}
			}
		}

		public static EggEntity Egg {
			get {
				lock (staticMutationLock) {
					return egg;
				}
			}
		}

		public static Vector3 CurrentBoardDrift {
			get {
				lock (staticMutationLock) {
					return currentBoardDrift;
				}
			}
		}

		private static void StartGameplay() {
			if (currentGameLevel == null) throw new InvalidOperationException("Can not begin level: No level is loaded.");
			switch (currentGameState) {
				case OverallGameState.LevelPlaying: return;
			}
			currentGameState = OverallGameState.LevelPlaying;
			Assure.False(egg == null || egg.IsDisposed);

			if (InputModule.KeyIsDown(Config.Key_TiltGameboardForward)) TiltGameboard(GameboardTiltDirection.Forward, false);
			if (InputModule.KeyIsDown(Config.Key_TiltGameboardBackward)) TiltGameboard(GameboardTiltDirection.Backward, false);
			if (InputModule.KeyIsDown(Config.Key_TiltGameboardLeft)) TiltGameboard(GameboardTiltDirection.Left, false);
			if (InputModule.KeyIsDown(Config.Key_TiltGameboardRight)) TiltGameboard(GameboardTiltDirection.Right, false);


			var x = lastUpdateLSX;
			var y = lastUpdateLSY;
			float tiltDeadzone = Config.BoardTiltDeadZone;
			float maxAxialTilt = 32767f - tiltDeadzone;
			bool xDead = false;
			bool yDead = false;
			if (x < -tiltDeadzone) {
				TiltGameboard(GameboardTiltDirection.Left, (x + tiltDeadzone) / -maxAxialTilt);
			}
			else if (x > tiltDeadzone) {
				TiltGameboard(GameboardTiltDirection.Right, (x - tiltDeadzone) / maxAxialTilt);
			}
			else xDead = true;
			if (y < -tiltDeadzone) {
				TiltGameboard(GameboardTiltDirection.Backward, (y + tiltDeadzone) / -maxAxialTilt);
			}
			else if (y > tiltDeadzone) {
				TiltGameboard(GameboardTiltDirection.Forward, (y - tiltDeadzone) / maxAxialTilt); ;
			}
			else yDead = true;

			if (xDead && yDead) EraseTilt();



			Sounds_LevelStart();
			HUD_EndIntro();
			AssetLocator.LightPass.DynamicLightCap = Config.DynamicLightCap;

			GCSettings.LatencyMode = GCLatencyMode.Interactive;
		}

		private static void TiltGameboard(GameboardTiltDirection tiltDir, bool releaseTilt) {
			activeTilts.RemoveWhere(tilt => tilt.TiltDir == tiltDir);
			if (!releaseTilt) {
				Vector3 forwardVector = groundPlane.OrientationProjection(eggBoom.UnalteredCamOrientation).ToUnit();
				Vector3 tiltAxis;
				float tiltRadians;
				switch (tiltDir) {
					case GameboardTiltDirection.Forward:
						tiltAxis = forwardVector * boardForwardToRightQuat;
						tiltRadians = -GameplayConstants.BOARD_TILT_RADIANS;
						break;
					case GameboardTiltDirection.Left:
						tiltAxis = forwardVector;
						tiltRadians = -GameplayConstants.BOARD_TILT_RADIANS;
						break;
					case GameboardTiltDirection.Right:
						tiltAxis = forwardVector;
						tiltRadians = GameplayConstants.BOARD_TILT_RADIANS;
						break;
					case GameboardTiltDirection.Backward:
						tiltAxis = forwardVector * boardForwardToRightQuat;
						tiltRadians = GameplayConstants.BOARD_TILT_RADIANS;
						break;
					default:
						throw new InvalidOperationException("Unrecognised tilt direction '" + tiltDir + "'.");
				}
				activeTilts.Add(new GameboardTiltData(tiltDir, Quaternion.FromAxialRotation(tiltAxis, tiltRadians), 1f));
			}

			targetTilt = Quaternion.IDENTITY;
			foreach (GameboardTiltData activeTilt in activeTilts) {
				targetTilt *= activeTilt.TargetRot;
			}
		}

		private static void TiltGameboard(GameboardTiltDirection tiltDir, float tiltAmount) {
			activeTiltRemovalWorkspace.Clear();
			for (int i = 0; i < activeTilts.Count; ++i) {
				if (activeTilts[i].TiltDir == tiltDir) activeTiltRemovalWorkspace.Add(activeTilts[i]);
			}
			for (int i = 0; i < activeTiltRemovalWorkspace.Count; ++i) {
				activeTilts.Remove(activeTiltRemovalWorkspace[i]);
			}
			Vector3 forwardVector = groundPlane.OrientationProjection(eggBoom.UnalteredCamOrientation).ToUnit();
			Vector3 tiltAxis;
			float tiltRadians;
			switch (tiltDir) {
				case GameboardTiltDirection.Forward:
					tiltAxis = forwardVector * boardForwardToRightQuat;
					tiltRadians = -GameplayConstants.BOARD_TILT_RADIANS * tiltAmount;
					break;
				case GameboardTiltDirection.Left:
					tiltAxis = forwardVector;
					tiltRadians = -GameplayConstants.BOARD_TILT_RADIANS * tiltAmount;
					break;
				case GameboardTiltDirection.Right:
					tiltAxis = forwardVector;
					tiltRadians = GameplayConstants.BOARD_TILT_RADIANS * tiltAmount;
					break;
				case GameboardTiltDirection.Backward:
					tiltAxis = forwardVector * boardForwardToRightQuat;
					tiltRadians = GameplayConstants.BOARD_TILT_RADIANS * tiltAmount;
					break;
				default:
					throw new InvalidOperationException("Unrecognised tilt direction '" + tiltDir + "'.");
			}
			activeTilts.Add(new GameboardTiltData(tiltDir, Quaternion.FromAxialRotation(tiltAxis, tiltRadians), tiltAmount));

			targetTilt = Quaternion.IDENTITY;
			foreach (GameboardTiltData activeTilt in activeTilts) {
				targetTilt *= activeTilt.TargetRot;
			}
		}

		private static void EraseTilt() {
			activeTilts.Clear();
			targetTilt = Quaternion.IDENTITY;
		}

		private static void FlipGameboard(bool clockwise) {
			if (currentFlipInProgress != null) return;
			Vector3 eggVelo = egg.Velocity;
			
			// If the ball is moving too slowly, you're not allowed to flip
			if (eggVelo.LengthSquared < GameplayConstants.MIN_SPEED_FOR_BOARD_FLIP_SQUARED) {
				int instanceId = AudioModule.CreateSoundInstance(AssetLocator.UnavailableSound);
				AudioModule.PlaySoundInstance(
					instanceId,
					false,
					1f * Config.SoundEffectVolume,
					RandomProvider.Next(0.85f, 1.15f)
				);
				return;
			}

			// If the ball's velocity is mostly up or down, you're not allowed to flip
			Vector3 rotAxis = eggVelo.ToUnit();
			if (Math.Abs(rotAxis.Dot(boardDownDir)) > GameplayConstants.MAX_PERMISSIBLE_VERTICAL_VELO_FRACTION_FOR_FLIP) {
				int instanceId = AudioModule.CreateSoundInstance(AssetLocator.UnavailableSound);
				AudioModule.PlaySoundInstance(
					instanceId,
					false,
					1f * Config.SoundEffectVolume,
					RandomProvider.Next(0.85f, 1.15f)
				);
				return;
			}

			// Project the rot axis on to the ground plane, and then find the closest axis
			rotAxis = groundPlane.OrientationProjection(rotAxis);
			rotAxis = primaryAxes.OrderByDescending(pa => pa.Dot(rotAxis)).First();

			Quaternion boardRot = Quaternion.FromAxialRotation(rotAxis, MathUtils.PI_OVER_TWO * (clockwise ? 1f : -1f));
			currentFlipInProgress = new GameboardFlipData(boardDownDir, boardRot);
			currentFlipElapsedTime = 0f;

			int instanceId2 = AudioModule.CreateSoundInstance(AssetLocator.WorldFlipSound);
			AudioModule.PlaySoundInstance(
				instanceId2,
				false,
				Config.SoundEffectVolume,
				RandomProvider.Next(0.75f, 1.25f)
			);

			foreach (var kvp in flipArrows) {
				kvp.Key.Dispose();
				if (!kvp.Value.IsDisposed) kvp.Value.Dispose();
			}
			flipArrows.Clear();

			Vector3 leftVec = rotAxis * Quaternion.FromAxialRotation(boardDownDir, -MathUtils.PI_OVER_TWO);
			timeSinceLastFlip = 0f;
			lastFlipData = currentFlipInProgress.Value;

			for (int i = FLIP_ARROW_ROW_HALF_LEN; i >= -FLIP_ARROW_ROW_HALF_LEN; --i) {
				var offset = rotAxis.WithLength(FLIP_ARROW_SPACING * i);

				var pairMat = new Material("Game Object Mat: Staggered Arrow", AssetLocator.AlphaFragmentShader);
				pairMat.SetMaterialResource(
					(ResourceViewBinding) AssetLocator.AlphaFragmentShader.GetBindingByIdentifier("DiffuseMap"),
					userSelectedEggMat.Material().FragmentShaderResourcePackage.GetValue((ResourceViewBinding) AssetLocator.GeometryFragmentShader.GetBindingByIdentifier("DiffuseMap"))
				);
				//pairMat.SetMaterialConstantValue(alphaMatColorBinding, new Vector4(1f, 1f, 1f, 1f - (i + FLIP_ARROW_ROW_HALF_LEN) / (FLIP_ARROW_ROW_HALF_LEN * 2f + 1f)));
				pairMat.SetMaterialConstantValue(alphaMatColorBinding, new Vector4(1f, 1f, 1f, 0f));

				var flipArrowLeft = new ArrowEntity();
				flipArrowLeft.SetModelInstance(AssetLocator.GameAlphaLayer, AssetLocator.WorldFlipArrowModel, pairMat);
				flipArrowLeft.Transform = new Transform(
					new Vector3(FLIP_ARROW_SCALE, (clockwise ? FLIP_ARROW_FORWARD_START_Y_SCALE : -FLIP_ARROW_FORWARD_START_Y_SCALE), FLIP_ARROW_SCALE),
					Quaternion.FromAxialRotation(rotAxis, MathUtils.PI) * Quaternion.FromVectorTransition(Vector3.DOWN, boardDownDir) * Quaternion.FromVectorTransition(rotAxis, leftVec),
					egg.Transform.Translation + leftVec.WithLength(PhysicsManager.ONE_METRE_SCALED * 0.2f) - boardDownDir.WithLength(PhysicsManager.ONE_METRE_SCALED * 0.05f * FLIP_ARROW_SCALE * (clockwise ? 1f : -1f)) + offset
				);

				var flipArrowRight = new ArrowEntity();
				flipArrowRight.SetModelInstance(AssetLocator.GameAlphaLayer, AssetLocator.WorldFlipArrowModel, pairMat);
				flipArrowRight.Transform = new Transform(
					new Vector3(FLIP_ARROW_SCALE, (clockwise ? FLIP_ARROW_FORWARD_START_Y_SCALE : -FLIP_ARROW_FORWARD_START_Y_SCALE), FLIP_ARROW_SCALE),
					Quaternion.IDENTITY * Quaternion.FromVectorTransition(Vector3.DOWN, boardDownDir) * Quaternion.FromVectorTransition(rotAxis, leftVec),
					egg.Transform.Translation - leftVec.WithLength(PhysicsManager.ONE_METRE_SCALED * 0.2f) + boardDownDir.WithLength(PhysicsManager.ONE_METRE_SCALED * 0.05f * FLIP_ARROW_SCALE * (clockwise ? 1f : -1f)) + offset
				);

				flipArrows.Insert(0, new KeyValuePair<Entity, Material>(flipArrowRight, pairMat));
				flipArrows.Insert(0, new KeyValuePair<Entity, Material>(flipArrowLeft, pairMat));
			}
		}

		private static void HopEgg() {
			if (currentGameState != OverallGameState.LevelPlaying) return;

			bool doHop = false;
			if (hopActive) {
				if (!usedDoubleHop) doHop = usedDoubleHop = true;
			}
			else if (numHopsRemaining > 0) {
				--numHopsRemaining;
				hopActive = doHop = true;
				usedDoubleHop = false;
			}

			if (doHop) {
				Vector3 lateralHopImpulse = Vector3.ZERO;
				Vector3 tiltDir = -boardDownDir * targetTilt;
				if (Vector3.AngleBetween(-boardDownDir, tiltDir) > MathUtils.DegToRad(5f)) {
					lateralHopImpulse = groundPlane.OrientationProjection(tiltDir).WithLength(GameplayConstants.EGG_HOP_LATERAL_FORCE);
				}

				egg.AddImpulse(lateralHopImpulse + -boardDownDir.WithLength(GameplayConstants.EGG_HOP_VERTICAL_FORCE));

				int instanceId = AudioModule.CreateSoundInstance(AssetLocator.HopSound);
				AudioModule.PlaySoundInstance(
					instanceId,
					false,
					1f * Config.SoundEffectVolume,
					RandomProvider.Next(0.8f, 1.4f)
				);

				if (!usedDoubleHop) {
					instanceId = AudioModule.CreateSoundInstance(AssetLocator.ShowHopSound);
					AudioModule.PlaySoundInstance(
						instanceId,
						false,
						0.2f * Config.SoundEffectVolume,
						0.55f
					);
				}

				CollisionBitPool.DisseminateBits(egg.Transform.Translation, lateralHopImpulse - boardDownDir.WithLength(GameplayConstants.EGG_HOP_VERTICAL_FORCE * 0.5f), 2 * (int) Config.PhysicsLevel);
			}
			else {
				int instanceId = AudioModule.CreateSoundInstance(AssetLocator.UnavailableSound);
				AudioModule.PlaySoundInstance(
					instanceId,
					false,
					1f * Config.SoundEffectVolume,
					RandomProvider.Next(0.85f, 1.15f)
				);
			}
		}

		private static void TickBoardManipulation(float deltaTime) {
			// Board flip
			if (currentFlipInProgress != null) {
				GameboardFlipData flipData = currentFlipInProgress.Value;
				currentFlipElapsedTime += deltaTime;

				float flipFractionComplete = Math.Min(currentFlipElapsedTime / GameplayConstants.BOARD_FLIP_TIME, 1f);
				boardDownDir = (flipData.previousDownDir * flipData.rotationToTargetDownDir.Subrotation(flipFractionComplete)).ToUnit();
				Vector3 downPerpVec = boardDownDir.AnyPerpendicular();
				Quaternion groundPlaneTripodSeparationQuat = Quaternion.FromVectorTransition(boardDownDir, downPerpVec).Subrotation(0.15f);
				ballGroundPlanarProbeVectors[0] = boardDownDir * groundPlaneTripodSeparationQuat;
				Quaternion groundPlaneTripodRotationQuat = Quaternion.FromAxialRotation(boardDownDir, MathUtils.TWO_PI);
				ballGroundPlanarProbeVectors[1] = ballGroundPlanarProbeVectors[0] * groundPlaneTripodRotationQuat.Subrotation(0.33f);
				ballGroundPlanarProbeVectors[2] = ballGroundPlanarProbeVectors[0] * groundPlaneTripodRotationQuat.Subrotation(0.66f);
				groundPlane = new Plane(-boardDownDir, Vector3.ZERO);
				boardForwardToRightQuat = Quaternion.FromAxialRotation(boardDownDir, MathUtils.PI_OVER_TWO);

				if (currentFlipElapsedTime >= GameplayConstants.BOARD_FLIP_TIME + GameplayConstants.MULTIPLE_BOARD_FLIP_DELAY) {
					currentFlipInProgress = null;
				}

				PhysicsManager.SetGravityOnAllBodies(boardDownDir * GameplayConstants.GRAVITY_ACCELERATION);
			}

			if (flipArrows.Count > 0) {
				timeSinceLastFlip += deltaTime;
				if (timeSinceLastFlip >= FLIP_TIME) {
					foreach (var kvp in flipArrows) {
						kvp.Key.Dispose();
						if (!kvp.Value.IsDisposed) kvp.Value.Dispose();
					}
					flipArrows.Clear();
				}
				else {
					float flipFractionComplete = Math.Min(timeSinceLastFlip / GameplayConstants.BOARD_FLIP_TIME, 1f);
					for (int i = 0; i < flipArrows.Count; ++i) {
						const float MAX_ALPHA_STEPS = (FLIP_TIME - FLIP_ARROW_MIN_FADEOUT_TIME) / (FLIP_ARROW_ROW_HALF_LEN * 2);
						var maxAlphaPoint = (i / 2) * MAX_ALPHA_STEPS;
						var alpha = (float) MathUtils.Clamp(1f - ((Math.Abs(maxAlphaPoint - timeSinceLastFlip) / FLIP_ARROW_MIN_FADEOUT_TIME) * 2f), 0f, 1f) * MAX_FLIP_ARROW_ALPHA;
						bool clampPosition = timeSinceLastFlip < maxAlphaPoint;
						
						flipArrows[i].Key.SetScale(new Vector3(flipArrows[i].Key.Transform.Scale, y: (FLIP_ARROW_FORWARD_START_Y_SCALE + (FLIP_ARROW_FORWARD_END_Y_SCALE - FLIP_ARROW_FORWARD_START_Y_SCALE) * flipFractionComplete) * (flipArrows[i].Key.Transform.Scale.Y < 0f ? -1f : 1f)));
						flipArrows[i].Key.RotateAround(egg.Transform.Translation, -lastFlipData.rotationToTargetDownDir.Subrotation((0.04f * deltaTime / GameplayConstants.BOARD_FLIP_TIME) * i));
						if (clampPosition) flipArrows[i].Key.TranslateBy(egg.Transform.Translation - eggPosLastFrame);
						flipArrows[i].Value.SetMaterialConstantValue(alphaMatColorBinding, new Vector4(1f, 1f, 1f, alpha));
					}
				}
			}


			// Board tilt
			RecalculateTiltsAccordingToCamera();
			var tiltableEntities = CurrentlyLoadedLevel.TiltableEntities;
			if (!targetTilt.EqualsWithTolerance(currentTilt, MathUtils.FlopsErrorMargin)) {
				Quaternion actualToTargetRot;
				float actualToTargetDelta = Quaternion.DistanceBetween(currentTilt, targetTilt, out actualToTargetRot);
				float maxTiltRads = (GameplayConstants.BOARD_TILT_SPEED_RADIANS * deltaTime);
				if (maxTiltRads > actualToTargetDelta) maxTiltRads = actualToTargetDelta;
				float tiltFraction = maxTiltRads / actualToTargetDelta;
				Quaternion tiltThisFrame = actualToTargetRot.Subrotation(tiltFraction);

				Vector3 pivotPoint = egg.Transform.Translation + boardDownDir.WithLength(GameplayConstants.EGG_COLLISION_RADIUS);

				foreach (Entity entity in tiltableEntities) {
					if (entity is VultureEggEntity) {
						VultureEggEntity entityAsVultureEgg = (VultureEggEntity) entity;
						if (vultureEggStaticPos.ContainsKey(entityAsVultureEgg)) {
							Vector3 oldPos = vultureEggStaticPos[entityAsVultureEgg];
							Vector3 newPos = pivotPoint + (oldPos - pivotPoint) * tiltThisFrame;
							vultureEggStaticPos[entityAsVultureEgg] = newPos;
							if (entityAsVultureEgg.IsGrounded) {
								entityAsVultureEgg.MoveGroundedMassless(newPos, tiltThisFrame);
							}
							else {
								entityAsVultureEgg.SetTranslation(newPos);
								entityAsVultureEgg.RotateBy(tiltThisFrame);
							}
						}
						continue;
					}
					else if (entity is LizardCoinEntity) {
						LizardCoinEntity entityAsLizardCoin = (LizardCoinEntity) entity;
						if (lizardCoinStaticPos.ContainsKey(entityAsLizardCoin)) {
							Vector3 oldPos = lizardCoinStaticPos[entityAsLizardCoin];
							Vector3 newPos = pivotPoint + (oldPos - pivotPoint) * tiltThisFrame;
							lizardCoinStaticPos[entityAsLizardCoin] = newPos;
							if (entityAsLizardCoin.IsGrounded) {
								entityAsLizardCoin.MoveGroundedMassless(newPos, tiltThisFrame);
							}
							else {
								entityAsLizardCoin.SetTranslation(newPos);
								entityAsLizardCoin.RotateBy(tiltThisFrame);
							}
						}
						continue;
					}
					
					if (entity is PresetMovementEntity) ((PresetMovementEntity) entity).Tilt(pivotPoint, tiltThisFrame);
					else entity.RotateAround(pivotPoint, tiltThisFrame);
				}
				currentTilt *= tiltThisFrame;

				Vector3 newDrift = cameraAttracterEntity.Transform.Translation - initialCamAttracterPos;
				Vector3 driftDelta = newDrift - currentBoardDrift;
				currentBoardDrift = newDrift;
				currentCamAttracterPos = initialCamAttracterPos + newDrift;

				currentSkyLevel.AdjustSkyboxAccordingToDrift(driftDelta);

				if (tutorialPanelActive) {
					if (targetTilt != Quaternion.IDENTITY && !tutorialPanelDiminished) DiminishTutorialPanel();
					else if (targetTilt == Quaternion.IDENTITY && tutorialPanelDiminished) UndiminishTutorialPanel();
				}
			}

			// Reground grounded entities
			foreach (GroundableEntity groundable in CurrentlyLoadedLevel.GroundableEntities) {
				groundable.RealignGrounding();
			}
		}

		private static void RecalculateTiltsAccordingToCamera() {
			tiltRecalcWorkspace.Clear();

			for (int i = 0; i < activeTilts.Count; ++i) {
				tiltRecalcWorkspace.Add(activeTilts[i]);
			}

			for (int i = 0; i < tiltRecalcWorkspace.Count; ++i) {
				TiltGameboard(tiltRecalcWorkspace[i].TiltDir, tiltRecalcWorkspace[i].TiltAmount);
			}
		}

		private static void ResetGameboard() {
			HUDItemExciterEntity.ClearExciterMaterials();
			velocityLastFrame = Vector3.ZERO;
			AssetLocator.LightPass.SetLensProperties(Config.DofLensFocalDist, Config.DofLensMaxBlurDist);
			numHopsRemaining = currentGameLevel.NumHops;
			hopActive = false;
			usedDoubleHop = false;
			boardDownDir = Vector3.DOWN;
			//ballGroundPlanarProbeVectors[0] = 
			groundPlane = new Plane(-boardDownDir, Vector3.ZERO);
			boardForwardToRightQuat = Quaternion.FromAxialRotation(boardDownDir, MathUtils.PI_OVER_TWO);
			currentFlipInProgress = null;
			currentFlipElapsedTime = 0f;
			activeTilts.Clear();
			targetTilt = Quaternion.IDENTITY;
			currentTilt = Quaternion.IDENTITY;
			currentSkyLevel.AdjustSkyboxAccordingToDrift(-currentBoardDrift);
			currentBoardDrift = Vector3.ZERO;
			PhysicsManager.SetGravityOnAllBodies(Vector3.DOWN * GameplayConstants.GRAVITY_ACCELERATION);
			if (!BakeLoadedLevelIfNecessary()) {
				CurrentlyLoadedLevel.ResetEntities(false);
				CurrentlyLoadedLevel.ResetGameObjects();
				CurrentlyLoadedLevel.CalculateTiltables();
			}
			cameraAttracterEntity = (IntroCameraAttracterEntity) CurrentlyLoadedLevel.GetGameObjectRepresentation(
				CurrentlyLoadedLevel.GetGameObjectsByType<IntroCameraAttracter>().Single()
			);
			currentCamAttracterPos = initialCamAttracterPos = cameraAttracterEntity.Transform.Translation;
			Vector3 cuboidDimensions = (CurrentlyLoadedLevel.GameZoneRadii + Vector3.ONE * GameplayConstants.FALL_OUT_RADIUS_BUFFER) * 2f;
			fallOutCuboid = new Cuboid(Vector3.ZERO, cuboidDimensions.X, cuboidDimensions.Y, cuboidDimensions.Z);

			vultureEggEntities.Clear();
			vultureEggStaticPos.Clear();
			var entityKeys = CurrentlyLoadedLevel
				.GetGameObjectsByType<VultureEgg>()
				.Select(ve => CurrentlyLoadedLevel.GetGameObjectRepresentation(ve))
				.Cast<VultureEggEntity>();
			foreach (VultureEggEntity entityKey in entityKeys) {
				vultureEggEntities.Add(entityKey, entityKey.Transform.Translation);
				vultureEggStaticPos[entityKey] = entityKey.Transform.Translation;
			}

			newPosWorkspace.Clear();
			foreach (var vultureEgg in vultureEggEntities.Keys) {
				newPosWorkspace.Add(vultureEgg, Vector3.ZERO);
			}

			AssetLocator.MainGeometryPass._SET_VEGG_INST(vultureEggEntities.Keys.Select(e => e._MI));

			foreach (VultureEggEntity vultureEgg in vultureEggEntities.Keys) {
				Vector3 vEggPos = vultureEgg.Transform.Translation;
				
				vultureEgg.SetGravity(Vector3.ZERO);

				GeometryEntity groundingEntity = null;
				if (vultureEgg.IsGrounded) {
					groundingEntity = vultureEgg.GroundingEntity;
					vultureEgg.Unground(GameplayConstants.VULTURE_EGG_MASS, Vector3.ZERO);
				}
				vultureEgg.SetTranslation(GetPlacementForGameObject(vEggPos, GameplayConstants.VULTURE_EGG_DISTANCE_FROM_GEOM));
				vultureEggStaticPos[vultureEgg] = vultureEgg.Transform.Translation;
				if (groundingEntity != null) vultureEgg.Ground(groundingEntity);

				vultureEgg.CollisionDetected += (_, other) => {
					if (other is EggEntity && vultureEggStaticPos.ContainsKey(vultureEgg)) vultureEggStaticPos.Remove(vultureEgg);
					else if (other is VultureEggEntity && !vultureEggStaticPos.ContainsKey((VultureEggEntity) other) && vultureEggStaticPos.ContainsKey(vultureEgg)) vultureEggStaticPos.Remove(vultureEgg);
					else return;
					vultureEgg.Unground(GameplayConstants.VULTURE_EGG_MASS, (boardDownDir * currentTilt).WithLength(GameplayConstants.GRAVITY_ACCELERATION));
					vultureEggEntities[vultureEgg] = vultureEgg.Transform.Translation;
				};
			}
			
			lizardCoinStaticPos.Clear();
			takenCoinIndices.Clear();
			previouslyTakenCoinIndices = PersistedWorldData.GetTakenCoinIndices(currentlyLoadedLevelID);
			numCoinsTaken = numFreshCoinsTaken = 0;
			numCoinsTakenBeforeStart = previouslyTakenCoinIndices.Count();
			var allCoinEntities = currentGameLevel.GetGameObjectsByType<LizardCoin>().Select(lc => (LizardCoinEntity) currentGameLevel.GetGameObjectRepresentation(lc));
			numCoinsInLevel = allCoinEntities.Count();
			int lceIndex = 0;
			int numFreshCoinsRemaining = numCoinsInLevel - previouslyTakenCoinIndices.Count();
			foreach (LizardCoinEntity lce in allCoinEntities) {
				const float ORTHO_OFFSET = GameplayConstants.VULTURE_EGG_COLLISION_RADIUS_MULTIPLIER * GameplayConstants.EGG_COLLISION_RADIUS * 2f;

				Vector3 lcePos = lce.Transform.Translation;

				lce.SetPersistenceData(lceIndex, previouslyTakenCoinIndices.Contains(lceIndex));
				++lceIndex;

				lce.SetGravity(Vector3.ZERO);

				GeometryEntity groundingEntity = null;
				if (lce.IsGrounded) {
					groundingEntity = lce.GroundingEntity;
					lce.Unground(GameplayConstants.VULTURE_EGG_MASS, Vector3.ZERO);
				}
				lce.SetTranslation(GetPlacementForGameObject(lcePos, GameplayConstants.VULTURE_EGG_DISTANCE_FROM_GEOM));
				if (groundingEntity != null) lce.Ground(groundingEntity);

				lizardCoinStaticPos.Add(lce, lce.Transform.Translation);

				lce.CollisionDetected += (_, other) => {
					if (currentGameState != OverallGameState.LevelPlaying) return;
					if (!(other is EggEntity)) return;
					if (lce.IsTaken) return;
					lce.BeTaken();
					takenCoinIndices.Add(lce.CoinIndex);
					++numCoinsTaken;
					bool isFresh = !previouslyTakenCoinIndices.Contains(lce.CoinIndex);
					if (isFresh) ++numFreshCoinsTaken;
					CoinTaken(isFresh, numFreshCoinsTaken == numFreshCoinsRemaining);
				};
			}

			foreach (FinishingBellTopEntity finishingBellTopEntity in finishingBells.Values) {
				finishingBellTopEntity.Dispose();
			}
			finishingBells.Clear();
			var finishingBellObjects = CurrentlyLoadedLevel.GetGameObjectsByType<FinishingBell>();
			foreach (FinishingBell bellObject in finishingBellObjects) {
				FinishingBellEntity fbe = (FinishingBellEntity) CurrentlyLoadedLevel.GetGameObjectRepresentation(bellObject);
				FinishingBellTopEntity fbte = new FinishingBellTopEntity(fbe.Transform);
				fbte.SetPhysicsShape(
					AssetLocator.FinishingBellTopPhysicsShape, Vector3.ZERO, 1f, 
					disablePerformanceDeactivation: true,
					restitution: 1f,
					friction: 1f,
					rollingFriction: 1f,
					linearDamping: 0.8f,
					angularDamping: 0.5f
				);
				fbte.SetGravity(Vector3.ZERO);
				fbte.SetModelInstance(AssetLocator.GameLayer, AssetLocator.FinishingBellTopModel, AssetLocator.FinishingBellTopMaterial);
				fbte.Transform = bellObject.Transform.TranslateBy(bellObject.Transform.Rotation * Vector3.UP * GameplayConstants.BELL_TOP_DISTANCE);
				fbte.CollisionDetected += (bellTop, other) => { if (other is EggEntity) EggCollision(other, bellTop); };
				finishingBells.Add(fbe, fbte);
			}
			hitBell = null;

			foreach (SillyBitEntity sillyBit in currentSillyBits) {
				sillyBit.Dispose();
			}
			currentSillyBits.Clear();

			passTimeElapsed = 0f;
			foreach (var kvp in alphaFadeMaterials) {
				if (!kvp.Key.IsDisposed) kvp.Key.Dispose();
				if (!kvp.Value.IsDisposed) kvp.Value.Dispose();
			}
			alphaFadeMaterials.Clear();
			if (failAlphaBallMaterial != null && !failAlphaBallMaterial.IsDisposed) failAlphaBallMaterial.Dispose();
			failAlphaBallMaterial = null;

			if (finishLight != null) AssetLocator.LightPass.RemoveLight(finishLight);
			finishLight = null;
			if (finishStar != null && !finishStar.IsDisposed) finishStar.Dispose();
			finishStar = null;

			foreach (Light light in finishLightRing) {
				AssetLocator.LightPass.RemoveLight(light);
			}
			finishLightRing.Clear();
			foreach (StarEntity star in finishStarRing) {
				star.Dispose();
			}
			finishStarRing.Clear();
			foreach (StarEntity star in fallingStars.Keys) {
				star.Dispose();
			}
			fallingStars.Clear();

			foreach (var kvp in flipArrows) {
				kvp.Key.Dispose();
				if (!kvp.Value.IsDisposed) kvp.Value.Dispose();
			}
			flipArrows.Clear();

			CollisionBitPool.ClearAll();

			
			eusocialMovableEntities.Clear();
			if (currentLevelIsEusocial) {
				foreach (var le in CurrentlyLoadedLevel.LevelEntities) {
					if (le.Material.ID != 17 && le.Material.ID != 19) continue;
					eusocialMovableEntities.Add(CurrentlyLoadedLevel.GetEntityRepresentation(le));
				}
				eusocialPRNG = new Random(EUSOCIAL_PRNG_SEED);
			}
		}

		private static readonly List<RayTestCollision> placementRayTestWorkspace = new List<RayTestCollision>();
		private static unsafe Vector3 GetPlacementForGameObject(Vector3 goPosition, float buffer) {
			float ORTHO_OFFSET = buffer * 0.33f;

			float closestHitDistanceSquared = Single.PositiveInfinity;
			Vector3 closestHitAxis = Vector3.ZERO;
			Vector3 closestHitPoint = Vector3.ZERO;

			for (int i = 0; i < primaryAxes.Length; ++i) {
				var axis = primaryAxes[i];
				var orthoA = primaryOrthogonals[i].WithLength(ORTHO_OFFSET);
				var orthoB = axis.Cross(orthoA).WithLength(ORTHO_OFFSET);

				// The four 'spread out' test rays
				EntityModule.RayTestAllLessGarbage(new Ray(goPosition - axis.WithLength(ORTHO_OFFSET) + orthoA, axis, ORTHO_OFFSET * 2f), placementRayTestWorkspace);
				if (!WorkspaceContainsGeomEntity()) continue;
				EntityModule.RayTestAllLessGarbage(new Ray(goPosition - axis.WithLength(ORTHO_OFFSET) - orthoA, axis, ORTHO_OFFSET * 2f), placementRayTestWorkspace);
				if (!WorkspaceContainsGeomEntity()) continue;
				EntityModule.RayTestAllLessGarbage(new Ray(goPosition - axis.WithLength(ORTHO_OFFSET) + orthoB, axis, ORTHO_OFFSET * 2f), placementRayTestWorkspace);
				if (!WorkspaceContainsGeomEntity()) continue;
				EntityModule.RayTestAllLessGarbage(new Ray(goPosition - axis.WithLength(ORTHO_OFFSET) - orthoB, axis, ORTHO_OFFSET * 2f), placementRayTestWorkspace);
				if (!WorkspaceContainsGeomEntity()) continue;

				// The actual hitpoint ray
				EntityModule.RayTestAllLessGarbage(new Ray(goPosition - axis.WithLength(ORTHO_OFFSET * 2f), axis), placementRayTestWorkspace);
				for (int j = 0; j < placementRayTestWorkspace.Count; ++j) {
					if (placementRayTestWorkspace[j].Entity is GeometryEntity) {
						var distanceSq = Vector3.DistanceSquared(goPosition, placementRayTestWorkspace[j].HitPoint);
						if (distanceSq < closestHitDistanceSquared) {
							closestHitDistanceSquared = distanceSq;
							closestHitAxis = axis;
							closestHitPoint = placementRayTestWorkspace[j].HitPoint;
						}
						j = placementRayTestWorkspace.Count;
					}
				}
			}

			if (Single.IsPositiveInfinity(closestHitDistanceSquared)) return goPosition;

			return closestHitPoint - closestHitAxis.WithLength(buffer);
		}

		private static bool WorkspaceContainsGeomEntity() {
			for (int i = 0; i < placementRayTestWorkspace.Count; ++i) {
				if (placementRayTestWorkspace[i].Entity is GeometryEntity) return true;
			}

			return false;
		}
		#endregion

		#region Level Meta
		private static int timeRemainingMs;
		private const float TIME_BEFORE_TUTORIAL_DISPLAY = 0.5f;
		private static float elapsedGameTime;
		private const float EGG_DISTANCE_FROM_START_FOR_TUTORIAL_CLOSE = PhysicsManager.ONE_METRE_SCALED * 7.5f;
		private const float EGG_DISTANCE_FROM_START_FOR_TUTORIAL_CLOSE_SQ = EGG_DISTANCE_FROM_START_FOR_TUTORIAL_CLOSE * EGG_DISTANCE_FROM_START_FOR_TUTORIAL_CLOSE;
		private const float EGG_DISTANCE_FROM_START_FOR_TIMER_SUSPEND = PhysicsManager.ONE_METRE_SCALED * 0.75f;
		private const float EGG_DISTANCE_FROM_START_FOR_TIMER_SUSPEND_SQ = EGG_DISTANCE_FROM_START_FOR_TIMER_SUSPEND * EGG_DISTANCE_FROM_START_FOR_TIMER_SUSPEND;
		private static readonly List<RayTestCollision> reusableHopRayTestList = new List<RayTestCollision>();

		private static void EggCollision(Entity egg, Entity other) {
			if (currentGameState == OverallGameState.LevelPlaying && (other is FinishingBellEntity || other is FinishingBellTopEntity)) {
				PassLevel(other as FinishingBellTopEntity ?? finishingBells[(FinishingBellEntity) other]);
			}
			else if (other is GeometryEntity) {
				Sounds_Collision((GeometryEntity) other);
			}
			else if (other is VultureEggEntity) {
				int instanceId = AudioModule.CreateSoundInstance(AssetLocator.PostPassEggPopSound);
				AudioModule.PlaySoundInstance(
					instanceId,
					false,
					1f * Config.SoundEffectVolume,
					2f
				);
				instanceId = AudioModule.CreateSoundInstance(AssetLocator.ImpactSounds[RandomProvider.Next(0, AssetLocator.ImpactSounds.Length)]);
				AudioModule.PlaySoundInstance(
					instanceId,
					false,
					IMPACT_VOLUME * Config.SoundEffectVolume,
					RandomProvider.Next(IMPACT_PITCH_MIN, IMPACT_PITCH_MAX)
				);
				MaybePlayAllBounce(PhysicsManager.ONE_METRE_SCALED);
			}
		}

		private static void ResetGame() {
			timeRemainingMs = CurrentlyLoadedLevel.LevelTimerMaxMs;
			elapsedGameTime = 0f;
			eusocialMovementTracks.Clear();
		}

		private static unsafe void TickGame(float deltaTime) {
			elapsedGameTime += deltaTime;
			if (elapsedGameTime >= TIME_BEFORE_TUTORIAL_DISPLAY && (elapsedGameTime - deltaTime) < TIME_BEFORE_TUTORIAL_DISPLAY) {
				if (TutorialManager.ShouldDisplayTutorial(currentlyLoadedLevelID)) DisplayTutorialPanel();
			}

			if (!tutorialPanelActive || tutorialPanelDiminishedSinceCreation) timeRemainingMs -= (int) (deltaTime * 1000f);
			if (timeRemainingMs < 0) {
				timeRemainingMs = 0;
				FailLevel(LevelFailReason.TimeUp);
				return;
			}

			if (tutorialPanelActive) {
				if (!tutorialPanelClosing) {
					float distFromStartSq = Vector3.DistanceSquared(CurrentlyLoadedLevel.StartFlagPosition, egg.Transform.Translation);
					if (distFromStartSq >= EGG_DISTANCE_FROM_START_FOR_TUTORIAL_CLOSE_SQ) {
						CloseTutorialPanel();
					}
					else if (distFromStartSq < EGG_DISTANCE_FROM_START_FOR_TIMER_SUSPEND_SQ && timeRemainingMs <= 29000) {
						timeRemainingMs = 30000;
					}
				}
			}

			if (hopActive) {
				var testRay = new Ray(egg.Transform.Translation, boardDownDir, GameplayConstants.EGG_HOP_RESET_MAX_DIST);
				EntityModule.RayTestAllLessGarbage(testRay, reusableHopRayTestList);
				for (int i = 0; i < reusableHopRayTestList.Count; ++i) {
					var coll = reusableHopRayTestList[i];
					if (coll.Entity != null && coll.Entity != egg) hopActive = false;
				}
			}

			Vector3 displacement = currentCamAttracterPos - fallOutCuboid.CenterPoint;
			fallOutCuboid = new Cuboid(
				fallOutCuboid.FrontBottomLeft + displacement,
				fallOutCuboid.Width,
				fallOutCuboid.Height,
				fallOutCuboid.Depth
			);

			Vector3 eggPos = egg.Transform.Translation;
			Vector3 camAttToEgg = eggPos - currentCamAttracterPos;
			Vector3 adjustedEggPos = currentCamAttracterPos + camAttToEgg * -currentTilt;
			if (!fallOutCuboid.Contains(adjustedEggPos)) {
				Vector3 attracterToEgg = eggPos - currentCamAttracterPos;
				if (Vector3.AngleBetween(-boardDownDir, attracterToEgg) > MathUtils.PI_OVER_TWO) {
					FailLevel(LevelFailReason.Dropped);
					return;
				}
			}

			foreach (var kvp in lizardCoinStaticPos) {
				if (!kvp.Key.IsTaken) {
					kvp.Key.SetTranslation(kvp.Value);
					kvp.Key.Velocity = Vector3.ZERO;
					kvp.Key.AngularVelocity = Vector3.ZERO;
					continue;
				}
			}

			foreach (KeyValuePair<VultureEggEntity, Vector3> kvp in vultureEggEntities) {
				if (vultureEggStaticPos.ContainsKey(kvp.Key) || kvp.Key.Transform.Scale == Vector3.ZERO || kvp.Key.IsDisposed) {
					if (vultureEggStaticPos.ContainsKey(kvp.Key)) {
						kvp.Key.SetTranslation(vultureEggStaticPos[kvp.Key]);
						kvp.Key.Velocity = Vector3.ZERO;
						kvp.Key.AngularVelocity = Vector3.ZERO;
					}
					continue;
				}
				Vector3 curPosition = kvp.Key.Transform.Translation;
				kvp.Key.SetGravity((boardDownDir * currentTilt).WithLength(GameplayConstants.GRAVITY_ACCELERATION));
				newPosWorkspace[kvp.Key] = curPosition;

				camAttToEgg = curPosition - currentCamAttracterPos;
				adjustedEggPos = currentCamAttracterPos + camAttToEgg * -currentTilt;
				if (!fallOutCuboid.Contains(adjustedEggPos)) {
					Vector3 attracterToEgg = curPosition - currentCamAttracterPos;
					if (Vector3.AngleBetween(-boardDownDir, attracterToEgg) > MathUtils.PI_OVER_TWO) {
						kvp.Key.SetScale(Vector3.ZERO);	
						kvp.Key.SetMass(0f);
						kvp.Key.SetGravity(Vector3.ZERO);
						int instanceId = AudioModule.CreateSoundInstance(AssetLocator.EggCrunchSound);
						AudioModule.PlaySoundInstance(
							instanceId,
							false,
							1.8f * Config.SoundEffectVolume
						);
						instanceId = AudioModule.CreateSoundInstance(AssetLocator.VultureCrySound);
						AudioModule.PlaySoundInstance(
							instanceId,
							false,
							1.4f * Config.SoundEffectVolume
						);
					}
				}
			}
			foreach (KeyValuePair<VultureEggEntity, Vector3> kvp in newPosWorkspace) {
				vultureEggEntities[kvp.Key] = kvp.Value;
			}

			foreach (KeyValuePair<FinishingBellEntity, FinishingBellTopEntity> kvp in finishingBells) {
				kvp.Value.Transform = kvp.Key.Transform.TranslateBy(kvp.Key.Transform.Rotation * Vector3.UP * GameplayConstants.BELL_TOP_DISTANCE);
				kvp.Value.Velocity = Vector3.ZERO;
				kvp.Value.AngularVelocity = Vector3.ZERO;
			}

			//Vector3 hitPoint;
			//var eggFloorRayTestEntity = EntityModule.RayTestNearest(new Ray(eggPos, boardDownDir, GameplayConstants.EGG_FLOAT_TEST_RADIUS), out hitPoint);

			//if (eggFloorRayTestEntity != null && eggFloorRayTestEntity != egg) {
			//	egg.SetGravity(Vector3.ZERO);
			//	egg.SetTranslation(hitPoint - boardDownDir.WithLength(GameplayConstants.EGG_FLOAT_TEST_RADIUS));
			//}
			//else egg.SetGravity(boardDownDir * GameplayConstants.GRAVITY_ACCELERATION);			

			//float eggSpeed = egg.TrueVelocity.Length;
			//float boostFraction = 1f - (eggSpeed / GameplayConstants.EGG_MAX_SPEED_FOR_BOOST);
			//if (boostFraction > 0f) {
			//	Vector3 boosterForce = groundPlane.OrientationProjection(currentTilt * -boardDownDir);

			//	if (boosterForce != Vector3.ZERO) {
			//		egg.AddForce(boosterForce.WithLength(GameplayConstants.EGG_FLOAT_BOOST_FORCE_MAX * boostFraction * deltaTime));
			//	}
			//}

			

			TickSound(deltaTime);
			eggPosLastFrame = eggPos;

			CollisionBitPool.Tick(deltaTime);

			if (currentLevelIsEusocial) {
				float timeBefore = elapsedGameTime - deltaTime;
				//if (((int)timeBefore) % EUSOCIAL_MOVEMENT_WAVE_TIME != 0 && ((int)elapsedGameTime) % EUSOCIAL_MOVEMENT_WAVE_TIME == 0) {
				if (((int) timeBefore) != ((int) elapsedGameTime)) {
					for (int i = 0; i < EUSOCIAL_MOVEMENT_WAVE_COUNT; ++i) {
						int nextCubeIndex = eusocialPRNG.Next(0, eusocialMovableEntities.Count);
						int movementAxisIndex = eusocialPRNG.Next(0, 6);
						Vector3 startPoint = eusocialMovableEntities[nextCubeIndex].Transform.Translation;
						eusocialMovementTracks.Add(new EusocialMovementTrack(
							eusocialMovableEntities[nextCubeIndex],
							elapsedGameTime,
							startPoint,
							startPoint + MAJOR_AXES[movementAxisIndex].WithLength(EUSOCIAL_MOVEMENT_DISTANCE)
						));
					}
				}

				foreach (var track in eusocialMovementTracks) {
					float timeElapsed = elapsedGameTime - track.StartTime;
					if (timeElapsed >= EUSOCIAL_MOVEMENT_TIME) {
						float fracPassed = deltaTime - (timeElapsed - EUSOCIAL_MOVEMENT_TIME) / EUSOCIAL_MOVEMENT_TIME;
						track.Entity.TranslateBy((track.EndPoint - track.StartPoint) * currentTilt * fracPassed);
						eusocialWorkspace.Add(track);
					}
					else {
						float fracPassed = deltaTime / EUSOCIAL_MOVEMENT_TIME;
						track.Entity.TranslateBy((track.EndPoint - track.StartPoint) * currentTilt * fracPassed);
					}
				}

				foreach (var track in eusocialWorkspace) {
					eusocialMovementTracks.Remove(track);
				}
				eusocialWorkspace.Clear();
			}
		}

		private static void TickPhysicsHacks(float deltaTime) {
			foreach (KeyValuePair<VultureEggEntity, Vector3> kvp in vultureEggEntities) {
				if (vultureEggStaticPos.ContainsKey(kvp.Key) || kvp.Key.Transform.Scale == Vector3.ZERO || kvp.Key.IsDisposed) {
					continue;
				}
				Vector3 curPosition = kvp.Key.Transform.Translation;
				Vector3 distanceSinceLastTick = groundPlane.OrientationProjection(curPosition - kvp.Value);
				if (distanceSinceLastTick != Vector3.ZERO) {
					Quaternion angularDisplacement = Quaternion.FromAxialRotation(
						Quaternion.FromAxialRotation(boardDownDir, MathUtils.PI_OVER_TWO) * distanceSinceLastTick,
						-MathUtils.TWO_PI * (distanceSinceLastTick.Length / vultureEggSphere.Circumference)
						);
					AssetLocator.MainGeometryPass._ADD_VEGG_ROT(kvp.Key._MI, angularDisplacement);
				}
			}
		}
		#endregion

		#region Level Introduction
		private static float introductionCountdownCounter;
		private static bool accelerateIntroductions;
		private static Vector3 introEndingCamOrientation;
		private static Vector3 introEndingCamPosition;
		private static Vector3 introEndingCamTranslation;

		private static float levelIntroTime {
			get {
				return Math.Max(Config.LevelIntroductionTime, GameplayConstants.INTRO_EGG_SPAWN_TIME * 2f);
			}
		}

		public static void StartLevelIntroduction() {
			lock (staticMutationLock) {
				lastUpdateLSX = 0;
				lastUpdateLSY = 0;
				if (currentGameLevel == null || currentSkyLevel == null) {
					throw new InvalidOperationException("Can not start level introduction: No level or no skybox is loaded.");
				}
				ResetGameboard();
				ResetGame();
				CloseTutorialPanel();
				SetUpHUD();
				currentGameState = OverallGameState.LevelIntroduction;
				introductionCountdownCounter = levelIntroTime;
				accelerateIntroductions = false;
				DestroyEgg();
				Sounds_LevelIntro();
				HUD_StartIntro();
				AssetLocator.LightPass.DynamicLightCap = null;
				GC.Collect(2, GCCollectionMode.Forced, true);
				GC.WaitForPendingFinalizers();
			}
		}

		private static void SkipToIntroEnd() {
			if (currentGameState != OverallGameState.LevelIntroduction) return;
			float jumpTime = introductionCountdownCounter - GameplayConstants.INTRO_EGG_SPAWN_TIME;
			if (jumpTime <= 0f) return;
			TickIntroduction(jumpTime, allowLargeTimeJump: true);
			foreach (var movingEntity in CurrentlyLoadedLevel.TiltableEntities.OfType<PresetMovementEntity>()) {
				movingEntity.AdvanceMovement(jumpTime);
			}
		}

		private static void TickIntroduction(float deltaTime, bool allowLargeTimeJump = false) {
			if (!allowLargeTimeJump) deltaTime = Math.Min(deltaTime, 0.05f); // It starts with a giant leap because the level loading takes a while, d'oh
			if (introductionCountdownCounter <= 0f) return;

			bool introEnding = introductionCountdownCounter <= GameplayConstants.INTRO_EGG_SPAWN_TIME;
			bool accelerateThisFrame = accelerateIntroductions && !introEnding;
			introductionCountdownCounter -= deltaTime * (accelerateThisFrame ? Config.LevelIntroAccelerationModifier : 1f);
			if (introductionCountdownCounter < 0f) introductionCountdownCounter = 0f;
			if (introEnding && (egg == null || egg.IsDisposed)) {
				introEndingCamOrientation = AssetLocator.MainCamera.Orientation;
				introEndingCamPosition = AssetLocator.MainCamera.Position;
				introEndingCamTranslation = CurrentlyLoadedLevel.GameplayStartCameraPos - AssetLocator.MainCamera.Position;
				SpawnEgg();
			}

			float adjustedSpawnValue = GameplayConstants.INTRO_EGG_SPAWN_TIME * Config.LevelIntroPreSpawnModifier;
			float introTimeBeforeEggSpawn = levelIntroTime - adjustedSpawnValue;
			float fractionOfIntroComplete = (introTimeBeforeEggSpawn - (introductionCountdownCounter - adjustedSpawnValue)) / introTimeBeforeEggSpawn;
			float fractionOfIntroRemaining = 1f - fractionOfIntroComplete;

			if (accelerateThisFrame) {
				foreach (var movingEntity in CurrentlyLoadedLevel.TiltableEntities.OfType<PresetMovementEntity>()) {
					movingEntity.AdvanceMovement(deltaTime * (Config.LevelIntroAccelerationModifier - 1f));
				}
			}

			/* 
				* There are four components to the intro camera's final Position/orientation:
				*		1: Height above the gameplay start height
				*		2: Distance further than the gameplay start Position (XZ) from the camera attracter
				*		3: Rotation around camera attracter 
				*		4: Orientation
				*/

			// 1: Height above the gameplay start height
			float startingHeight = CurrentlyLoadedLevel.StartFlagPosition.Y + GameplayConstants.CAMERA_HEIGHT_ADDITION + Config.CameraHeightOffset;
			float targetHeight = startingHeight + fractionOfIntroRemaining * Config.LevelIntroExtraCameraHeight;

			// 2: Distance further than the gameplay start Position (XZ) from the camera attracter
			float startingDistance = Vector3.Distance(
				new Vector3(CurrentlyLoadedLevel.StartFlagPosition, y: 0f),
				new Vector3(CurrentlyLoadedLevel.CameraAttracterPosition, y: 0f)
			) + GameplayConstants.CAMERA_MIN_VELOCITY_BASED_DIST_TO_EGG + Config.CameraDistanceOffset;
			float targetDistance = startingDistance + fractionOfIntroRemaining * Config.LevelIntroExtraCameraDistance;

			// 3: Rotation around camera attracter
			Vector3 attracterToGameplayStartCam = new Vector3(CurrentlyLoadedLevel.GameplayStartCameraPos, y: 0f)
				- new Vector3(CurrentlyLoadedLevel.CameraAttracterPosition, y: 0f);
			float cameraPanRadians = Config.LevelIntroCameraPanAmountRadians * fractionOfIntroRemaining;

			// Now set the camera Position
			if (introEnding) {
				float endingFraction = (float) Math.Atan((introductionCountdownCounter / GameplayConstants.INTRO_EGG_SPAWN_TIME - 0.5f) * X_AXIS_SMOOTHING_EXPANSION_FACTOR) / (MathUtils.PI - ASYMPTOTIC_CORRECTION) + 0.5f;
				AssetLocator.MainCamera.Position = introEndingCamPosition + introEndingCamTranslation * (1f - endingFraction);
			}
			else {
				AssetLocator.MainCamera.Position =
				new Vector3(CurrentlyLoadedLevel.CameraAttracterPosition, y: 0f) +
				(attracterToGameplayStartCam * Quaternion.FromAxialRotation(Vector3.DOWN, cameraPanRadians)).WithLength(targetDistance)
				+ Vector3.UP * targetHeight;
			}

			// 4: Orientation
			if (introEnding) {
				AssetLocator.MainCamera.Orient(
					introEndingCamOrientation * Quaternion.FromVectorTransition(
						introEndingCamOrientation,
						CurrentlyLoadedLevel.GameplayStartCameraOrientation
					).Subrotation(1f - (introductionCountdownCounter / GameplayConstants.INTRO_EGG_SPAWN_TIME)),
					Vector3.UP
				);
			}
			else {
				AssetLocator.MainCamera.LookAt(CurrentlyLoadedLevel.CameraAttracterPosition, Vector3.UP);
			}
			
			HUD_TickIntro(deltaTime, introductionCountdownCounter);

			// ReSharper disable once CompareOfFloatsByEqualityOperator Exact value is set above
			if (introductionCountdownCounter == 0f) StartGameplay();

		}

		private static void SpawnEgg() {
			DestroyEgg();

			userSelectedEggMat = Config.EggMaterial;
			if (!PersistedWorldData.EggMaterialIsUnlocked(userSelectedEggMat)) {
				var eggMats = LizardEggMaterialExtensions.MaterialsOrderedByUnlockCost;
				var numGoldenEggs = PersistedWorldData.GetTotalGoldenEggs();
				var curEggIndex = 0;
				while (eggMats[curEggIndex].GoldenEggCost() <= numGoldenEggs) ++curEggIndex;
				userSelectedEggMat = eggMats[curEggIndex - 1];
			}
			egg = new EggEntity(userSelectedEggMat) {
				Transform = Transform.DEFAULT_TRANSFORM.TranslateBy(CurrentlyLoadedLevel.StartFlagPosition + GameplayConstants.EGG_SPAWN_HEIGHT * Vector3.UP)
			};
			eggBoom = new EggCameraBoom(
				egg,
				AssetLocator.MainCamera,
				GameplayConstants.CAMERA_HEIGHT_ADDITION + Config.CameraHeightOffset,
				GameplayConstants.CAMERA_MIN_VELOCITY_BASED_DIST_TO_EGG + Config.CameraDistanceOffset
			);			

			egg.CollisionDetected += EggCollision;
		}

		private static void DestroyEgg() {
			if (egg != null && !egg.IsDisposed) egg.Dispose();
			if (eggBoom != null && !eggBoom.IsDisposed) eggBoom.Dispose();

			egg = null;
			eggBoom = null;
		}
		#endregion

		#region Pass / Fail
		private static LevelPassDetails passDetails;
		private static readonly List<SillyBitEntity> currentSillyBits = new List<SillyBitEntity>();
		private static FinishingBellEntity hitBell = null;
		private static float passTimeElapsed = 0f;
		private static readonly Dictionary<Material, Entity> alphaFadeMaterials = new Dictionary<Material, Entity>();
		private static readonly ConstantBufferBinding alphaMatColorBinding = (ConstantBufferBinding) AssetLocator.AlphaFragmentShader.GetBindingByIdentifier("MaterialProperties");
		private static readonly ResourceViewBinding alphaMatTextureBinding = (ResourceViewBinding) AssetLocator.AlphaFragmentShader.GetBindingByIdentifier("DiffuseMap");
		private static Light finishLight = null;
		private static Vector3 finishLightColor;
		private static StarEntity finishStar;
		private static readonly List<Light> finishLightRing = new List<Light>();
		private static readonly List<StarEntity> finishStarRing = new List<StarEntity>();
		private static Vector3 ringCentre;
		private static Vector3 centreToLightVec;
		private static Quaternion rotPerLight;
		private static float timeUntilNextStarSpawn;
		private static float starSpawnInterval;
		private static Material fallingStarsMaterialTemplate;
		private static readonly Dictionary<StarEntity, float> fallingStars = new Dictionary<StarEntity, float>();
		private static readonly List<StarEntity> fallingStarsRemovalWorkspace = new List<StarEntity>();
		private static int numSillyBitsRemaining, totalSillyBits;
		private static float timeSinceLastSillyBit;
		private static float timeBetweenSillyBits;
		private static Vector3 sillyBitsPerpVector;
		private static Quaternion sillyBitsRotPerBit;
		private static Vector3 sillyBitsImpulseVector;

		public static void PassLevel(FinishingBellTopEntity hitBellTop) {
			GCSettings.LatencyMode = GCLatencyMode.Interactive;
			AssetLocator.LightPass.DynamicLightCap = null;
			lock (staticMutationLock) {
				LevelLeaderboardDetails leaderboardDetailsBeforeLevelCompletion = currentLevelLeaderboardDetails.Copy();

				Medal oldMedal = Medal.None;
				if (currentLevelLeaderboardDetails.GoldFriend == LeaderboardManager.LocalPlayerDetails) oldMedal = Medal.Gold;
				else if (currentLevelLeaderboardDetails.SilverFriend == LeaderboardManager.LocalPlayerDetails) oldMedal = Medal.Silver;
				else if (currentLevelLeaderboardDetails.BronzeFriend == LeaderboardManager.LocalPlayerDetails) oldMedal = Medal.Bronze;

				Star star = Star.None;
				unsafe {
					int* starTimesRemainingMs = stackalloc int[3];
					starTimesRemainingMs[0] = timeRemainingMs - (CurrentlyLoadedLevel.LevelTimerMaxMs - CurrentlyLoadedLevel.LevelTimerBronzeMs);
					starTimesRemainingMs[1] = timeRemainingMs - (CurrentlyLoadedLevel.LevelTimerMaxMs - CurrentlyLoadedLevel.LevelTimerSilverMs);
					starTimesRemainingMs[2] = timeRemainingMs - (CurrentlyLoadedLevel.LevelTimerMaxMs - CurrentlyLoadedLevel.LevelTimerGoldMs);
					if (starTimesRemainingMs[2] >= 0) star = Star.Gold;
					else if (starTimesRemainingMs[1] >= 0) star = Star.Silver;
					else if (starTimesRemainingMs[0] >= 0) star = Star.Bronze;
				}
				Medal medal = Medal.None;
				unsafe {
					int* friendTimesRemainingMs = stackalloc int[3];
					if (currentLevelLeaderboardDetails.BronzeFriendTimeMs == null) friendTimesRemainingMs[0] = 0;
					else friendTimesRemainingMs[0] = currentLevelLeaderboardDetails.BronzeFriendTimeMs.Value;
					if (currentLevelLeaderboardDetails.SilverFriendTimeMs == null) friendTimesRemainingMs[1] = 0;
					else friendTimesRemainingMs[1] = currentLevelLeaderboardDetails.SilverFriendTimeMs.Value;
					if (currentLevelLeaderboardDetails.GoldFriendTimeMs == null) friendTimesRemainingMs[2] = 0;
					else friendTimesRemainingMs[2] = currentLevelLeaderboardDetails.GoldFriendTimeMs.Value;
					if (timeRemainingMs > friendTimesRemainingMs[2]) medal = Medal.Gold;
					else if (timeRemainingMs > friendTimesRemainingMs[1]) medal = Medal.Silver;
					else if (timeRemainingMs > friendTimesRemainingMs[0]) medal = Medal.Bronze;
				}

				if (currentLevelLeaderboardDetails.GoldFriend == LeaderboardManager.LocalPlayerDetails) medal = Medal.Gold;
				else if (currentLevelLeaderboardDetails.SilverFriend == LeaderboardManager.LocalPlayerDetails) medal = (medal == Medal.Gold ? medal : Medal.Silver);
				else if (currentLevelLeaderboardDetails.BronzeFriend == LeaderboardManager.LocalPlayerDetails) medal = (medal == Medal.None ? Medal.Bronze : medal);

				AsyncUpdateSocialMetadata(currentlyLoadedLevelID, timeRemainingMs, oldMedal, medal);
				

				foreach (var kvp in vultureEggEntities) {
					if (kvp.Key.Transform.Scale == Vector3.ZERO) continue;
					if (vultureEggStaticPos.ContainsKey(kvp.Key)) continue;
					Ray testRay = new Ray(kvp.Key.Transform.Translation, boardDownDir);
					Vector3 _;
					var hit = EntityModule.RayTestNearest(testRay, out _);
					if (hit == null) {
						kvp.Key.SetScale(Vector3.ZERO);
						kvp.Key.SetMass(0f);
						kvp.Key.SetGravity(Vector3.ZERO);
						int instanceId = AudioModule.CreateSoundInstance(AssetLocator.EggCrunchSound);
						AudioModule.PlaySoundInstance(
							instanceId,
							false,
							1.8f * Config.SoundEffectVolume
						);
						instanceId = AudioModule.CreateSoundInstance(AssetLocator.VultureCrySound);
						AudioModule.PlaySoundInstance(
							instanceId,
							false,
							1.4f * Config.SoundEffectVolume
						);
					}
				}

				int numVultureEggs = vultureEggEntities.Count;
				int numVultureEggsDestroyed = vultureEggEntities.Keys.Count(ve => ve.Transform.Scale == Vector3.ZERO);
				bool goldenEggAlreadyClaimed = PersistedWorldData.GoldenEggTakenForLevel(currentlyLoadedLevelID);
				bool goldenEggReceived = numVultureEggsDestroyed == numVultureEggs && !goldenEggAlreadyClaimed;
				if (goldenEggReceived) PersistedWorldData.AddGoldenEgg(currentlyLoadedLevelID);
				var freshTakenCoins = takenCoinIndices.Except(previouslyTakenCoinIndices);
				PersistedWorldData.AddTakenCoins(currentlyLoadedLevelID, freshTakenCoins);
				int? prevBestTime = PersistedWorldData.GetBestTimeForLevel(currentlyLoadedLevelID);
				
				passDetails = new LevelPassDetails(
					star,
					timeRemainingMs,
					medal,
					numVultureEggsDestroyed,
					numVultureEggs - numVultureEggsDestroyed,
					goldenEggReceived,
					PersistedWorldData.GetTotalGoldenEggs(),
					PersistedWorldData.GetBestTimeForLevel(currentlyLoadedLevelID) == null || timeRemainingMs > PersistedWorldData.GetBestTimeForLevel(currentlyLoadedLevelID),
					prevBestTime,
					CurrentlyLoadedLevel.LevelTimerMaxMs - timeRemainingMs,
					leaderboardDetailsBeforeLevelCompletion,
					numCoinsTaken,
					PersistedWorldData.GetTotalTakenCoins(),
					freshTakenCoins.Count(),
					numCoinsInLevel,
					currentlyLoadedLevelID.NextLevelInWorld == null
				);

				if (prevBestTime == null || prevBestTime.Value < timeRemainingMs) {
					PersistedWorldData.SetBestTimeForLevel(currentlyLoadedLevelID, timeRemainingMs);
				}

				if (currentGameState != OverallGameState.LevelPlaying) throw new InvalidOperationException("Tried to pass level when not being played.");
				currentGameState = OverallGameState.LevelPassed;

				hitBell = finishingBells.First(kvp => kvp.Value == hitBellTop).Key;

				int numSillyBits = AssetLocator.SillyBitsHandles.Length;
				numSillyBitsRemaining = numSillyBits;
				switch (Config.PhysicsLevel) {
					case 2U: numSillyBitsRemaining *= 2; break;
					case 3U:
					case 4U:
					case 5U: 
						numSillyBitsRemaining *= 8; break;
				}
				totalSillyBits = numSillyBitsRemaining;
				timeBetweenSillyBits = 0.4f / numSillyBitsRemaining;
				timeSinceLastSillyBit = timeBetweenSillyBits;
				Vector3 bellOutwardVector = Vector3.UP * hitBellTop.Transform.Rotation;
				sillyBitsPerpVector = bellOutwardVector.AnyPerpendicular().WithLength(6.5f);
				sillyBitsRotPerBit = Quaternion.FromAxialRotation(bellOutwardVector, (MathUtils.TWO_PI / numSillyBitsRemaining) * 4f);
				sillyBitsImpulseVector = (bellOutwardVector * Quaternion.FromVectorTransition(bellOutwardVector, sillyBitsPerpVector).Subrotation(0.05f)).WithLength(20f);

				hitBellTop.SetGravity(boardDownDir * GameplayConstants.GRAVITY_ACCELERATION * 0.5f);
				hitBellTop.AddImpulse(bellOutwardVector.WithLength(150f) + sillyBitsImpulseVector.WithLength(50f));

				switch (passDetails.Star) {
					case Star.Gold: finishLightColor = Config.FinishLightColorGold; break;
					case Star.Silver: finishLightColor = Config.FinishLightColorSilver; break;
					case Star.Bronze: finishLightColor = Config.FinishLightColorBronze; break;
					case Star.None: finishLightColor = Config.FinishLightColorNoStar; break;
				}
				finishLight = new Light(hitBell.Transform.Translation + -boardDownDir.WithLength(PhysicsManager.ONE_METRE_SCALED * 0.35f), Config.FinishLightRadius, finishLightColor * 2f);
				AssetLocator.LightPass.AddLight(finishLight);

				finishStar = new StarEntity(passDetails.Star, Quaternion.FromVectorTransition(Vector3.UP, bellOutwardVector * -currentTilt));
				finishStar.SetTranslation(hitBell.Transform.Translation + (bellOutwardVector * -currentTilt).WithLength(PhysicsManager.ONE_METRE_SCALED * 0.4f));

				int numRingLights;
				Vector3 starScale;
				switch (passDetails.Star) {
					case Star.Gold: 
						numRingLights = Config.FinishLightRingNumLightsGold;
						starSpawnInterval = timeUntilNextStarSpawn = Config.FinishLightRingStarSpawnRateGold;
						fallingStarsMaterialTemplate = AssetLocator.StarMaterialGold;
						starScale = Vector3.ONE * 1f;
						break;
					case Star.Silver: 
						numRingLights = Config.FinishLightRingNumLightsSilver;
						starSpawnInterval = timeUntilNextStarSpawn = Config.FinishLightRingStarSpawnRateSilver;
						fallingStarsMaterialTemplate = AssetLocator.StarMaterialSilver;
						starScale = Vector3.ONE * 1.3333f;
						break;
					case Star.Bronze: 
						numRingLights = Config.FinishLightRingNumLightsBronze;
						starSpawnInterval = timeUntilNextStarSpawn = Config.FinishLightRingStarSpawnRateBronze;
						fallingStarsMaterialTemplate = AssetLocator.StarMaterialBronze;
						starScale = Vector3.ONE * 1.6666f;
						break;
					default: 
						numRingLights = 0;
						starSpawnInterval = timeUntilNextStarSpawn = Single.PositiveInfinity;
						fallingStarsMaterialTemplate = null;
						starScale = Vector3.ONE * 0f;
						break;
				}

				bellOutwardVector *= -currentTilt;
				ringCentre = hitBellTop.Transform.Translation + bellOutwardVector.WithLength(PhysicsManager.ONE_METRE_SCALED * 0.3f);
				centreToLightVec = sillyBitsPerpVector.WithLength(Config.FinishLightRingRadius);
				rotPerLight = Quaternion.FromAxialRotation(bellOutwardVector, MathUtils.TWO_PI / numRingLights);
				for (int i = 0; i < numRingLights; ++i) {
					Light ringLight = new Light(
						ringCentre + centreToLightVec * rotPerLight.Subrotation(i), 
						Config.FinishLightRingLightRadius, 
						finishLightColor
					);
					finishLightRing.Add(ringLight);
					AssetLocator.LightPass.AddLight(ringLight);

					StarEntity ringStar = new StarEntity(
						passDetails.Star,
						Quaternion.FromVectorTransition(Vector3.DOWN, bellOutwardVector * -currentTilt),
						false
					);
					finishStarRing.Add(ringStar);
					ringStar.SetTranslation(ringCentre + (centreToLightVec * 0.95f) * rotPerLight.Subrotation(i));
					ringStar.SetScale(starScale);
				}

				activeTilts.Clear();
				targetTilt = Quaternion.IDENTITY;

				CloseTutorialPanel();
				SetUpPassHUD();
				Sounds_LevelPass(passDetails);

				PersistedWorldData.SaveToDisk();

				AchievementsManager.NotifyLevelCompleted(currentlyLoadedLevelID, passDetails);
			}
		}

		private static void TickPostPass(float deltaTime) {
			if (egg != null && !egg.IsDisposed) egg.RotateBy(egg.GetAngularDisplacementForTick(deltaTime));
			Camera cam = AssetLocator.MainCamera;
			Vector3 hitBellPos = hitBell.Transform.Translation;
			Vector3 bellToCam = cam.Position - hitBellPos;
			if (bellToCam.Length < Config.PassCamMaxDist) bellToCam = bellToCam.WithLength(bellToCam.Length + deltaTime * Config.PassCamZoomOutSpeed);
			cam.Position = hitBellPos + bellToCam * Quaternion.FromAxialRotation(boardDownDir, Config.PassCamSpin * deltaTime);
			cam.LookAt(hitBellPos, -boardDownDir);

			SetPassFailDoFValues(Math.Min(((cam.Position - hitBellPos).Length / Config.PassCamMaxDist) * 1.5f, 1f));

			passTimeElapsed += deltaTime;

			if (numSillyBitsRemaining > 0 && passTimeElapsed < Config.PassActorPreFadeLife) {
				timeSinceLastSillyBit += deltaTime;
				while (timeSinceLastSillyBit >= timeBetweenSillyBits && numSillyBitsRemaining > 0) {
					timeSinceLastSillyBit -= timeBetweenSillyBits;
					--numSillyBitsRemaining;

					SillyBitEntity newBit = new SillyBitEntity(RandomProvider.Next(0, AssetLocator.SillyBitsHandles.Length));
					Quaternion subrotation = sillyBitsRotPerBit.Subrotation(numSillyBitsRemaining);
					newBit.SetTranslation(hitBellPos + subrotation * sillyBitsPerpVector);
					newBit.AddImpulse(sillyBitsImpulseVector * subrotation);
					newBit.SetGravity(boardDownDir.WithLength(GameplayConstants.GRAVITY_ACCELERATION * 0.5f));

					currentSillyBits.Add(newBit);
				}
			}

			if (passTimeElapsed >= Config.PassActorPreFadeLife) {
				if (passTimeElapsed >= Config.PassActorPreFadeLife + Config.PassActorFadeTime) {
					foreach (KeyValuePair<Material, Entity> kvp in alphaFadeMaterials) {
						if (!kvp.Key.IsDisposed) kvp.Key.Dispose();
						if (!kvp.Value.IsDisposed) {
							kvp.Value.Dispose();
							if (kvp.Value is SillyBitEntity) currentSillyBits.Remove((SillyBitEntity) kvp.Value);
						}
					}
				}
				else {
					if (!alphaFadeMaterials.Any()) {
						Material eggFadeMaterial = new Material("Egg Fade Material", AssetLocator.AlphaFragmentShader);
						alphaFadeMaterials.Add(eggFadeMaterial, Egg);
						Material eggNonFadeMat = Config.EggMaterial.Material();
						eggFadeMaterial.SetMaterialResource(alphaMatTextureBinding, eggNonFadeMat.FragmentShaderResourcePackage.GetValue((ResourceViewBinding) AssetLocator.GeometryFragmentShader.GetBindingByIdentifier("DiffuseMap")));
						Egg.SetModelInstance(AssetLocator.GameAlphaLayer, Config.EggMaterial.Model(), eggFadeMaterial);

						Material bellTopFadeMaterial = new Material("Bell Top Fade Material", AssetLocator.AlphaFragmentShader);
						alphaFadeMaterials.Add(bellTopFadeMaterial, finishingBells[hitBell]);
						Material bellTopNonFadeMat = AssetLocator.FinishingBellTopMaterial;
						bellTopFadeMaterial.SetMaterialResource(alphaMatTextureBinding, bellTopNonFadeMat.FragmentShaderResourcePackage.GetValue((ResourceViewBinding) AssetLocator.GeometryFragmentShader.GetBindingByIdentifier("DiffuseMap")));
						finishingBells[hitBell].SetModelInstance(AssetLocator.GameAlphaLayer, AssetLocator.FinishingBellTopModel, bellTopFadeMaterial);

						for (int i = 0; i < currentSillyBits.Count; ++i) {
							int sillyBitIndex = i % AssetLocator.SillyBitsMaterials.Length;
							Material sillyBitFadeMaterial = new Material("Silly Bit Fade Material #" + i, AssetLocator.AlphaFragmentShader);
							alphaFadeMaterials.Add(sillyBitFadeMaterial, currentSillyBits[i]);
							Material sillyBitNonFadeMat = AssetLocator.SillyBitsMaterials[sillyBitIndex];
							sillyBitFadeMaterial.SetMaterialResource(alphaMatTextureBinding, sillyBitNonFadeMat.FragmentShaderResourcePackage.GetValue((ResourceViewBinding) AssetLocator.GeometryFragmentShader.GetBindingByIdentifier("DiffuseMap")));
							currentSillyBits[i].SetModelInstance(AssetLocator.GameAlphaLayer, AssetLocator.SillyBitsHandles[sillyBitIndex], sillyBitFadeMaterial);
						}
					}
					float alpha = 1f - ((passTimeElapsed - Config.PassActorPreFadeLife) / Config.PassActorFadeTime);
					foreach (var kvp in alphaFadeMaterials) {
						kvp.Key.SetMaterialConstantValue(alphaMatColorBinding, new Vector4(
							1f,
							1f,
							1f,
							alpha
						));
						if (kvp.Value is SillyBitEntity) {
							SillyBitEntity sbe = (SillyBitEntity) kvp.Value;
							sbe.SetLightRadius(Config.SillyBitsIlluminationRadius * 1.33f * alpha);
						}
					}
				}
			}

			Vector3 bellOutwardVector = Vector3.UP * hitBell.Transform.Rotation;
			finishStar.SetTranslation(hitBell.Transform.Translation + (bellOutwardVector * -currentTilt).WithLength(PhysicsManager.ONE_METRE_SCALED * 0.4f));

			float rotAddition = passTimeElapsed;
			for (int i = 0; i < finishLightRing.Count; ++i) {
				Light ringLight = finishLightRing[i];
				ringLight.Position = ringCentre + centreToLightVec * rotPerLight.Subrotation(i + rotAddition);

				StarEntity ringStar = finishStarRing[i];
				ringStar.SetTranslation(ringCentre + (centreToLightVec * 0.95f) * rotPerLight.Subrotation(i + rotAddition));
				ringStar.RotateBy(rotPerLight.Subrotation(((i % 3) + 1) * deltaTime * 3f));
			}

			float pitchMin;
			float pitchMax;

			switch (passDetails.Star) {
				case Star.Gold:
					pitchMin = 0.8f;
					pitchMax = 1.4f;
					break;
				case Star.Silver:
					pitchMin = 0.6f;
					pitchMax = 1.2f;
					break;
				default:
					pitchMin = 0.4f;
					pitchMax = 1f;
					break;
			}

			timeUntilNextStarSpawn -= deltaTime;
			while (timeUntilNextStarSpawn <= 0f) {
				timeUntilNextStarSpawn += starSpawnInterval;

				int starIndex = RandomProvider.Next(0, finishStarRing.Count);

				StarEntity newFallingStar = new StarEntity(passDetails.Star, Quaternion.FromAxialRotation(-boardDownDir, 0f), false);
				newFallingStar.Transform = finishStarRing[starIndex].Transform;
				newFallingStar.SetScale(Vector3.ONE * 0.75f * 1.6f);
				newFallingStar.SetModelInstance(AssetLocator.GameLayer, AssetLocator.StarModel, fallingStarsMaterialTemplate);
				newFallingStar.SetPhysicsShape(
					AssetLocator.StarPhysicsShape,
					Vector3.ZERO,
					1f,
					forceIntransigence: false,
					disablePerformanceDeactivation: false,
					collideOnlyWithWorld: true,
					restitution: 0.95f,
					linearDamping: 0.01f,
					angularDamping: 0.01f,
					friction: 0.01f,
					rollingFriction: 0.01f
				);
				newFallingStar.SetGravity(boardDownDir.WithLength(GameplayConstants.GRAVITY_ACCELERATION * 0.2f));
				newFallingStar.AddImpulse((newFallingStar.Transform.Translation - hitBellPos).WithLength(50f) + -boardDownDir.WithLength(100f));
				newFallingStar.AddTorqueImpulse(new Vector3(RandomProvider.Next(-150f, 150f), RandomProvider.Next(-150f, 150f), RandomProvider.Next(-150f, 150f)));
				fallingStars.Add(newFallingStar, passTimeElapsed);

				int instanceId = AudioModule.CreateSoundInstance(AssetLocator.PostPassStarSpawnSound);
				AudioModule.PlaySoundInstance(
					instanceId,
					false,
					0.025f * Config.SoundEffectVolume, 
					RandomProvider.Next(pitchMin, pitchMax)
				);
			}

			float lifetime = Config.FinishLightRingFallingStarLifetime;
			fallingStarsRemovalWorkspace.Clear();
			foreach (var kvp in fallingStars) {
				if (passTimeElapsed - kvp.Value >= lifetime) fallingStarsRemovalWorkspace.Add(kvp.Key);
			}

			foreach (StarEntity toBeRemoved in fallingStarsRemovalWorkspace) {
				fallingStars.Remove(toBeRemoved);
				toBeRemoved.Dispose();
			}

			foreach (var kvp in lizardCoinStaticPos) {
				kvp.Key.SetTranslation(kvp.Value);
				kvp.Key.SetGravity(Vector3.ZERO);
			}
			foreach (var kvp in vultureEggStaticPos) {
				kvp.Key.SetTranslation(kvp.Value);
				kvp.Key.SetGravity(Vector3.ZERO);
			}
		}


		private static LevelFailReason failReason;
		private static float failTimeElapsed = 0f;
		private static Quaternion failCamQuat;
		private static Vector3 originalFailCamOrientation;
		private const float FAIL_CAM_ORIENT_TIME = 4f;
		private const float X_AXIS_SMOOTHING_EXPANSION_FACTOR = 5.6539f;
		private const float ASYMPTOTIC_CORRECTION = 0.68f;
		private static Material failAlphaBallMaterial;
		public static void FailLevel(LevelFailReason reason) {
			AssetLocator.LightPass.DynamicLightCap = null;
			GCSettings.LatencyMode = GCLatencyMode.Interactive;
			lock (staticMutationLock) {
				if (currentGameState != OverallGameState.LevelPlaying) throw new InvalidOperationException("Tried to fail level when not being played.");
				failReason = reason;
				currentGameState = OverallGameState.LevelFailed;
				failTimeElapsed = 0f;

				activeTilts.Clear();
				targetTilt = Quaternion.IDENTITY;

				originalFailCamOrientation = AssetLocator.MainCamera.Orientation;
				Quaternion curOrientToUpOrient = Quaternion.FromVectorTransition(AssetLocator.MainCamera.Orientation, Vector3.UP);
				failCamQuat = curOrientToUpOrient.Subrotation(0.85f);

				CloseTutorialPanel();
				SetUpFailHUD();
				Sounds_LevelFail(reason);
			}
		}

		private static void TickPostFail(float deltaTime) {
			if (egg != null && !egg.IsDisposed) egg.RotateBy(egg.GetAngularDisplacementForTick(deltaTime));
			failTimeElapsed += deltaTime;

			SetPassFailDoFValues(Math.Min((failTimeElapsed / FAIL_CAM_ORIENT_TIME) * 3f, 1f));

			float subRotAmount = (float) Math.Atan((failTimeElapsed / FAIL_CAM_ORIENT_TIME - 0.5f) * X_AXIS_SMOOTHING_EXPANSION_FACTOR) / (MathUtils.PI - ASYMPTOTIC_CORRECTION) + 0.5f;
			subRotAmount = Math.Min(subRotAmount, 1f);
			AssetLocator.MainCamera.Orient(originalFailCamOrientation * failCamQuat.Subrotation(subRotAmount), -boardDownDir);

			if (failTimeElapsed >= Config.PassActorFadeTime) {
				if (failAlphaBallMaterial != null && !failAlphaBallMaterial.IsDisposed) failAlphaBallMaterial.Dispose();
				DestroyEgg();
			}
			else {
				if (failAlphaBallMaterial == null) {
					failAlphaBallMaterial = new Material("Egg Fade Material", AssetLocator.AlphaFragmentShader);
					Material eggNonFadeMat = Config.EggMaterial.Material();
					failAlphaBallMaterial.SetMaterialResource(alphaMatTextureBinding, eggNonFadeMat.FragmentShaderResourcePackage.GetValue((ResourceViewBinding) AssetLocator.GeometryFragmentShader.GetBindingByIdentifier("DiffuseMap")));
					Egg.SetModelInstance(AssetLocator.GameAlphaLayer, Config.EggMaterial.Model(), failAlphaBallMaterial);
				}
				float alpha = 1f - passTimeElapsed / Config.PassActorFadeTime;

				failAlphaBallMaterial.SetMaterialConstantValue(alphaMatColorBinding, new Vector4(
					0.6f,
					0.6f,
					0.6f,
					alpha
				));
			}

			foreach (KeyValuePair<FinishingBellEntity, FinishingBellTopEntity> kvp in finishingBells) {
				kvp.Value.Transform = kvp.Key.Transform.TranslateBy(kvp.Key.Transform.Rotation * Vector3.UP * GameplayConstants.BELL_TOP_DISTANCE);
				kvp.Value.Velocity = Vector3.ZERO;
				kvp.Value.AngularVelocity = Vector3.ZERO;
			}

			foreach (var kvp in lizardCoinStaticPos) {
				kvp.Key.SetTranslation(kvp.Value);
				kvp.Key.SetGravity(Vector3.ZERO);
			}
			foreach (var kvp in vultureEggStaticPos) {
				kvp.Key.SetTranslation(kvp.Value);
				kvp.Key.SetGravity(Vector3.ZERO);
			}
		}

		private static void SetPassFailDoFValues(float fracComplete) {
			const float DOF_FOCAL_DIST = PhysicsManager.ONE_METRE_SCALED;
			const float DOF_MAX_DIST = DOF_FOCAL_DIST * 2f;

			if (Single.IsPositiveInfinity(Config.DofLensMaxBlurDist)) return;

			float focalDistDiff = Config.DofLensFocalDist - DOF_FOCAL_DIST;
			float maxDistDiff = Config.DofLensMaxBlurDist - DOF_MAX_DIST;
			AssetLocator.LightPass.SetLensProperties(DOF_FOCAL_DIST + focalDistDiff * (1f - fracComplete), DOF_MAX_DIST + maxDistDiff * (1f - fracComplete));
		}
		#endregion

		#region Level Social
		private static LevelLeaderboardDetails currentLevelLeaderboardDetails = null;

		public static LevelLeaderboardDetails CurrentLevelLeaderboardDetails {
			get {
				lock (staticMutationLock) {
					return currentLevelLeaderboardDetails;
				}
			}
		}

		private static Task<LevelLeaderboardDetails> AsyncLoadSocialMetadata(LevelID id) {
			return Task.Run(() => LeaderboardManager.GetLeaderboardDetailsForLevel(id));
		}

		private static Task AsyncUpdateSocialMetadata(LevelID id, int timeRemainingMs, Medal oldMedal, Medal newMedal) {
			// do a local update too so if we retry the level, it's up-to-date

			int bestTime = PersistedWorldData.GetBestTimeForLevel(id) ?? timeRemainingMs;
			if (timeRemainingMs > bestTime) bestTime = timeRemainingMs;

			var oldDetails = currentLevelLeaderboardDetails;
			KVP<LeaderboardPlayer?, int?> gold = new KVP<LeaderboardPlayer?, int?>(oldDetails.GoldFriend, oldDetails.GoldFriendTimeMs);
			KVP<LeaderboardPlayer?, int?> silver = new KVP<LeaderboardPlayer?, int?>(oldDetails.SilverFriend, oldDetails.SilverFriendTimeMs);
			KVP<LeaderboardPlayer?, int?> bronze = new KVP<LeaderboardPlayer?, int?>(oldDetails.BronzeFriend, oldDetails.BronzeFriendTimeMs);

			switch (oldMedal) {
				case Medal.Gold:
					gold = new KVP<LeaderboardPlayer?, int?>(oldDetails.GoldFriend, bestTime);
					break;
				case Medal.Silver:
					switch (newMedal) {
						case Medal.Gold:
							gold = new KVP<LeaderboardPlayer?, int?>(LeaderboardManager.LocalPlayerDetails, bestTime);
							silver = new KVP<LeaderboardPlayer?, int?>(oldDetails.GoldFriend, oldDetails.GoldFriendTimeMs);
							bronze = new KVP<LeaderboardPlayer?, int?>(oldDetails.BronzeFriend, oldDetails.BronzeFriendTimeMs);
							break;
						case Medal.Silver:
							silver = new KVP<LeaderboardPlayer?, int?>(oldDetails.SilverFriend, bestTime);
							break;
					}
					break;
				case Medal.Bronze:
					switch (newMedal) {
						case Medal.Gold:
							gold = new KVP<LeaderboardPlayer?, int?>(LeaderboardManager.LocalPlayerDetails, bestTime);
							silver = new KVP<LeaderboardPlayer?, int?>(oldDetails.GoldFriend, oldDetails.GoldFriendTimeMs);
							bronze = new KVP<LeaderboardPlayer?, int?>(oldDetails.SilverFriend, oldDetails.SilverFriendTimeMs);
							break;
						case Medal.Silver:
							silver = new KVP<LeaderboardPlayer?, int?>(LeaderboardManager.LocalPlayerDetails, bestTime);
							bronze = new KVP<LeaderboardPlayer?, int?>(oldDetails.SilverFriend, oldDetails.SilverFriendTimeMs);
							break;
						case Medal.Bronze:
							bronze = new KVP<LeaderboardPlayer?, int?>(LeaderboardManager.LocalPlayerDetails, bestTime);
							break;
					}
					break;
				default:
					switch (newMedal) {
						case Medal.Gold:
							gold = new KVP<LeaderboardPlayer?, int?>(LeaderboardManager.LocalPlayerDetails, bestTime);
							silver = new KVP<LeaderboardPlayer?, int?>(oldDetails.GoldFriend, oldDetails.GoldFriendTimeMs);
							bronze = new KVP<LeaderboardPlayer?, int?>(oldDetails.SilverFriend, oldDetails.SilverFriendTimeMs);
							break;
						case Medal.Silver:
							silver = new KVP<LeaderboardPlayer?, int?>(LeaderboardManager.LocalPlayerDetails, bestTime);
							bronze = new KVP<LeaderboardPlayer?, int?>(oldDetails.SilverFriend, oldDetails.SilverFriendTimeMs);
							break;
						case Medal.Bronze:
							bronze = new KVP<LeaderboardPlayer?, int?>(LeaderboardManager.LocalPlayerDetails, bestTime);
							break;
					}
					break;
			}

			currentLevelLeaderboardDetails = new LevelLeaderboardDetails(
				bestTime,
				gold.Key,
				gold.Value,
				silver.Key,
				silver.Value,
				bronze.Key,
				bronze.Value
			);

			return Task.Run(() => LeaderboardManager.UpdatePlayerScoreForLevel(id, timeRemainingMs));
		}
		#endregion
	}
}