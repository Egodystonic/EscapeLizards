// All code copyright (c) 2016 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 20 07 2016 at 15:14 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using Ophidian.Losgap;
using Ophidian.Losgap.Rendering;

namespace Egodystonic.EscapeLizards {
	public static partial class MenuCoordinator {
		private const float MAIN_MENU_TRANSITION_IN_TIME = 0.3f;
		private const float MAIN_MENU_TRANSITION_OUT_TIME = 0.2f;
		private const float MAIN_MENU_EXIT_CONFIRM_TRANSITION_IN_TIME = 0.5f;
		private const float MAIN_MENU_EXIT_CONFIRM_TRANSITION_OUT_TIME = 0.15f;
		private static HUDTexture mainMenuELLogo;
		private static readonly Vector2 MAIN_MENU_EL_LOGO_TARGET_SCALE = new Vector2(0.35f, 0.35f);
		private static readonly Dictionary<HUDTexture, Vector4> mainMenuELLogoConfetti = new Dictionary<HUDTexture, Vector4>();
		private static readonly Dictionary<HUDTexture, Vector4> mainMenuELLogoConfettiWorkspace = new Dictionary<HUDTexture, Vector4>(); 
		private const int MAIN_MENU_EL_LOGO_LOW_CONFETTI_COUNT = 7;
		private static readonly Vector2 MAIN_MENU_BUTTON_SCALE = new Vector2(0.2f, 0.4f) * 0.4f;
		private static HUDTexture mainMenuPlayButtonL, mainMenuPlayButtonR;
		private static HUDTexture mainMenuMedalsButtonL, mainMenuMedalsButtonR;
		private static HUDTexture mainMenuOptionsButtonL, mainMenuOptionsButtonR;
		private static HUDTexture mainMenuExitButtonL, mainMenuExitButtonR;
		private static HUDTexture mainMenuButtonFrontL, mainMenuButtonFrontR;
		private static HUDTexture mainMenuButtonRingL, mainMenuButtonRingR;
		private static HUDItemExciterEntity mainMenuButtonRingExciterL, mainMenuButtonRingExciterR;
		private const float MAIN_MENU_SELECTED_OPTION_ADORNMENTS_ALPHA = 0.85f;
		private const float MAIN_MENU_BUTTONS_START_HEIGHT = 0.32f;
		private const float MAIN_MENU_BUTTONS_ROW_HEIGHT = 0.16f;
		private const float MAIN_MENU_BUTTON_TRANSITION_HORIZONTAL_OFFSET = 0.3f;
		private const float MAIN_MENU_TEXT_OFFSET_Y = 0.027f;
		private static readonly Vector2 MAIN_MENU_TEXT_SCALE = Vector2.ONE * 0.85f;
		private static FontString mainMenuPlayString;
		private static FontString mainMenuMedalsString;
		private static FontString mainMenuOptionsString;
		private static FontString mainMenuExitString;
		private static HUDItemExciterEntity mainMenuStringExciter;
		private static readonly Vector4 MAIN_MENU_STRING_SELECTED_COLOR = new Vector4(219f / 255f, 193f / 255f, 116f / 255f, 0.9f);
		private static readonly Vector4 MAIN_MENU_STRING_COLOR = new Vector4(184f / 255f, 170f / 255f, 35f / 255f, 0.9f);
		private static int mainMenuSelectedOption = 0;
		private static FontString mainMenuExitConfirmQuestionString;
		private static FontString mainMenuExitConfirmYesString;
		private static FontString mainMenuExitConfirmNoString;
		private static readonly Vector3 MAIN_MENU_EXIT_QUESTION_COLOR = new Vector3(1f, 1f, 1f);
		private static readonly Vector3 MAIN_MENU_EXIT_ANSWER_COLOR = new Vector3(0.85f, 1f, 0.7f);
		private static readonly Vector2 MAIN_MENU_EXIT_ANSWER_SCALE = Vector2.ONE * 0.65f;
		private static readonly Vector2 MAIN_MENU_EXIT_SELECTED_ANSWER_SCALE = MAIN_MENU_EXIT_ANSWER_SCALE * 1.3f;
		private static bool mainMenuInExitConfirmationScreen = false;
		private static bool mainMenuExitYesSelected = true;

		private static void MainMenuTransitionIn(bool createComponents = true) {
			CurrentMenuState = MenuState.MainMenu;
			if (createComponents) MainMenuCreateComponents();
			AddTransitionTickEvent(MainMenuTickTransitionIn);
			BeginTransition(MAIN_MENU_TRANSITION_IN_TIME);
			HUDSound.PostPassShowMenu.Play();
			if (!mainMenuSinceLoad) {
				mainMenuSinceLoad = true;
				HUDSound.XMenuLoad.Play();
				HUDSound.XAnnounceMain.Play();
			}
		}

		private static void MainMenuTransitionOut(bool disposeComponents = true) {
			AddTransitionTickEvent(MainMenuTickTransitionOut);
			BeginTransition(MAIN_MENU_TRANSITION_OUT_TIME);
			if (disposeComponents) currentTransitionComplete += MainMenuDisposeComponents;
		}

		private static void MainMenuTick(float deltaTime) {
			mainMenuELLogoConfettiWorkspace.Clear();
			foreach (var confetto in mainMenuELLogoConfetti.Keys) {
				confetto.AdjustAlpha(deltaTime * 0.5f);
				Vector4 metaDetails = mainMenuELLogoConfetti[confetto];
				metaDetails += Vector2.UP * 0.9f * deltaTime;
				confetto.AnchorOffset += metaDetails[0, 1] * deltaTime;
				confetto.Rotation += metaDetails.Z * deltaTime;
				if (metaDetails.W > 0f) {
					metaDetails += new Vector4(0f, 0f, 0f, 1f);
					int coinFrame = (int) metaDetails.W & 31;
					confetto.Texture = AssetLocator.CoinFrames[coinFrame];
				}
				mainMenuELLogoConfettiWorkspace.Add(confetto, metaDetails);
			}
			foreach (var kvp in mainMenuELLogoConfettiWorkspace) {
				if (kvp.Key.Color.W <= 0f) {
					mainMenuELLogoConfetti.Remove(kvp.Key);
					kvp.Key.Dispose();
				}
				else mainMenuELLogoConfetti[kvp.Key] = kvp.Value;
			}
		}

		private static void MainMenuCreateComponents() {
			MainMenuDisposeComponents();

			triggerUp += MainMenuChangeOptionUp;
			triggerRight += MainMenuChangeOptionUp;
			triggerDown += MainMenuChangeOptionDown;
			triggerLeft += MainMenuChangeOptionDown;
			triggerConfirm += MainMenuConfirmOption;
			triggerBackOut += MainMenuBackOut;
			tick += MainMenuTick;

			mainMenuInExitConfirmationScreen = false;

			mainMenuELLogo = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = ViewportAnchoring.TopCentered,
				AnchorOffset = new Vector2(0f, 0.01f),
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = new Vector4(1f, 1f, 1f, 1f),
				Rotation = 0f,
				Scale = new Vector2(0f, 0f),
				Texture = AssetLocator.ELLogo,
				ZIndex = 3
			};

			for (int i = 0; i < MAIN_MENU_EL_LOGO_LOW_CONFETTI_COUNT * Config.PhysicsLevel; ++i) {
				ITexture2D texture;
				switch (i % 4) {
					case 0: texture = AssetLocator.GoldStar; break;
					case 1: texture = AssetLocator.SilverStar; break;
					case 2: texture = AssetLocator.BronzeStar; break;
					default: texture = AssetLocator.CoinFrames[0]; break;
				}
				var confetto = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
					Anchoring = ViewportAnchoring.TopLeft,
					AnchorOffset = new Vector2(0.5f, mainMenuELLogo.AnchorOffset.Y),
					AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
					Color = new Vector4(1f, 1f, 1f, 1f),
					Rotation = 0f,
					Scale = new Vector2(0.1f, 0.1f),
					Texture = texture,
					ZIndex = mainMenuELLogo.ZIndex - 1
				};
				Vector2 initialVelo = new Vector2(
					RandomProvider.Next(-0.5f, 0.5f),
					RandomProvider.Next(-0.5f, 0.5f)
				);
				float initialSpin = RandomProvider.Next(-MathUtils.PI, MathUtils.PI);
				mainMenuELLogoConfetti.Add(confetto, new Vector4(initialVelo, z: initialSpin, w: texture == AssetLocator.CoinFrames[0] ? 32f : 0f));
			}

			mainMenuPlayButtonL = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = ViewportAnchoring.TopRight,
				AnchorOffset = new Vector2(0.37f + MAIN_MENU_BUTTON_TRANSITION_HORIZONTAL_OFFSET, MAIN_MENU_BUTTONS_START_HEIGHT + MAIN_MENU_BUTTONS_ROW_HEIGHT * 0),
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = MAIN_MENU_BUTTON_SCALE,
				Texture = AssetLocator.MainMenuPlayButton,
				ZIndex = 3
			};
			mainMenuPlayButtonR = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = ViewportAnchoring.TopLeft,
				AnchorOffset = new Vector2(0.37f + MAIN_MENU_BUTTON_TRANSITION_HORIZONTAL_OFFSET, MAIN_MENU_BUTTONS_START_HEIGHT + MAIN_MENU_BUTTONS_ROW_HEIGHT * 0),
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = MAIN_MENU_BUTTON_SCALE,
				Texture = AssetLocator.MainMenuPlayButton,
				ZIndex = 3
			};

			mainMenuMedalsButtonL = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = ViewportAnchoring.TopRight,
				AnchorOffset = new Vector2(0.41f + MAIN_MENU_BUTTON_TRANSITION_HORIZONTAL_OFFSET, MAIN_MENU_BUTTONS_START_HEIGHT + MAIN_MENU_BUTTONS_ROW_HEIGHT * 1),
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = MAIN_MENU_BUTTON_SCALE,
				Texture = AssetLocator.MainMenuMedalsButton,
				ZIndex = 3
			};
			mainMenuMedalsButtonR = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = ViewportAnchoring.TopLeft,
				AnchorOffset = new Vector2(0.41f + MAIN_MENU_BUTTON_TRANSITION_HORIZONTAL_OFFSET, MAIN_MENU_BUTTONS_START_HEIGHT + MAIN_MENU_BUTTONS_ROW_HEIGHT * 1),
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = MAIN_MENU_BUTTON_SCALE,
				Texture = AssetLocator.MainMenuMedalsButton,
				ZIndex = 3
			};

			mainMenuOptionsButtonL = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = ViewportAnchoring.TopRight,
				AnchorOffset = new Vector2(0.42f + MAIN_MENU_BUTTON_TRANSITION_HORIZONTAL_OFFSET, MAIN_MENU_BUTTONS_START_HEIGHT + MAIN_MENU_BUTTONS_ROW_HEIGHT * 2),
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = MAIN_MENU_BUTTON_SCALE,
				Texture = AssetLocator.MainMenuOptionsButton,
				ZIndex = 3
			};
			mainMenuOptionsButtonR = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = ViewportAnchoring.TopLeft,
				AnchorOffset = new Vector2(0.42f + MAIN_MENU_BUTTON_TRANSITION_HORIZONTAL_OFFSET, MAIN_MENU_BUTTONS_START_HEIGHT + MAIN_MENU_BUTTONS_ROW_HEIGHT * 2),
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = MAIN_MENU_BUTTON_SCALE,
				Texture = AssetLocator.MainMenuOptionsButton,
				ZIndex = 3
			};

			mainMenuExitButtonL = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = ViewportAnchoring.TopRight,
				AnchorOffset = new Vector2(0.37f + MAIN_MENU_BUTTON_TRANSITION_HORIZONTAL_OFFSET, MAIN_MENU_BUTTONS_START_HEIGHT + MAIN_MENU_BUTTONS_ROW_HEIGHT * 3),
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = MAIN_MENU_BUTTON_SCALE,
				Texture = AssetLocator.MainMenuExitButton,
				ZIndex = 3
			};
			mainMenuExitButtonR = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = ViewportAnchoring.TopLeft,
				AnchorOffset = new Vector2(0.37f + MAIN_MENU_BUTTON_TRANSITION_HORIZONTAL_OFFSET, MAIN_MENU_BUTTONS_START_HEIGHT + MAIN_MENU_BUTTONS_ROW_HEIGHT * 3),
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = MAIN_MENU_BUTTON_SCALE,
				Texture = AssetLocator.MainMenuExitButton,
				ZIndex = 3
			};



			mainMenuButtonFrontL = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = mainMenuPlayButtonL.Anchoring,
				AnchorOffset = mainMenuPlayButtonL.AnchorOffset,
				AspectCorrectionStrategy = mainMenuPlayButtonL.AspectCorrectionStrategy,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = MAIN_MENU_BUTTON_SCALE,
				Texture = AssetLocator.MainMenuPlayButtonFront,
				ZIndex = 4
			};
			mainMenuButtonFrontR = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = mainMenuPlayButtonR.Anchoring,
				AnchorOffset = mainMenuPlayButtonR.AnchorOffset,
				AspectCorrectionStrategy = mainMenuPlayButtonR.AspectCorrectionStrategy,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = MAIN_MENU_BUTTON_SCALE,
				Texture = AssetLocator.MainMenuPlayButtonFront,
				ZIndex = 4
			};

			mainMenuButtonRingL = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = mainMenuPlayButtonL.Anchoring,
				AnchorOffset = mainMenuPlayButtonL.AnchorOffset,
				AspectCorrectionStrategy = mainMenuPlayButtonL.AspectCorrectionStrategy,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = MAIN_MENU_BUTTON_SCALE,
				Texture = AssetLocator.MainMenuButtonRing,
				ZIndex = 2
			};
			mainMenuButtonRingR = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = mainMenuPlayButtonR.Anchoring,
				AnchorOffset = mainMenuPlayButtonR.AnchorOffset,
				AspectCorrectionStrategy = mainMenuPlayButtonR.AspectCorrectionStrategy,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = MAIN_MENU_BUTTON_SCALE,
				Texture = AssetLocator.MainMenuButtonRing,
				ZIndex = 2
			};

			mainMenuButtonRingExciterL = new HUDItemExciterEntity(mainMenuButtonRingL) {
				OpacityMultiplier = 0.066f,
				Lifetime = 0.8f,
				Speed = 0.06f,
				CountPerSec = 60
			};
			mainMenuButtonRingExciterR = new HUDItemExciterEntity(mainMenuButtonRingR) {
				OpacityMultiplier = mainMenuButtonRingExciterL.OpacityMultiplier,
				Lifetime = mainMenuButtonRingExciterL.Lifetime,
				Speed = mainMenuButtonRingExciterL.Speed,
				CountPerSec = mainMenuButtonRingExciterL.CountPerSec
			};

			mainMenuPlayString = AssetLocator.TitleFontGlow.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.TopCentered,
				new Vector2(0.0f, MAIN_MENU_BUTTONS_START_HEIGHT + MAIN_MENU_BUTTONS_ROW_HEIGHT * 0 + MAIN_MENU_TEXT_OFFSET_Y),
				MAIN_MENU_TEXT_SCALE
			);
			mainMenuPlayString.Color = new Vector4(MAIN_MENU_STRING_COLOR, w: 0f);
			mainMenuPlayString.Text = "PLAY";

			mainMenuMedalsString = AssetLocator.TitleFontGlow.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.TopCentered,
				new Vector2(0.0f, MAIN_MENU_BUTTONS_START_HEIGHT + MAIN_MENU_BUTTONS_ROW_HEIGHT * 1 + MAIN_MENU_TEXT_OFFSET_Y),
				MAIN_MENU_TEXT_SCALE
			);
			mainMenuMedalsString.Color = new Vector4(MAIN_MENU_STRING_COLOR, w: 0f);
			mainMenuMedalsString.Text = "MEDALS";

			mainMenuOptionsString = AssetLocator.TitleFontGlow.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.TopCentered,
				new Vector2(0.0f, MAIN_MENU_BUTTONS_START_HEIGHT + MAIN_MENU_BUTTONS_ROW_HEIGHT * 2 + MAIN_MENU_TEXT_OFFSET_Y),
				MAIN_MENU_TEXT_SCALE
			);
			mainMenuOptionsString.Color = new Vector4(MAIN_MENU_STRING_COLOR, w: 0f);
			mainMenuOptionsString.Text = "OPTIONS";

			mainMenuExitString = AssetLocator.TitleFontGlow.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.TopCentered,
				new Vector2(0.0f, MAIN_MENU_BUTTONS_START_HEIGHT + MAIN_MENU_BUTTONS_ROW_HEIGHT * 3 + MAIN_MENU_TEXT_OFFSET_Y),
				MAIN_MENU_TEXT_SCALE
			);
			mainMenuExitString.Color = new Vector4(MAIN_MENU_STRING_COLOR, w: 0f);
			mainMenuExitString.Text = "QUIT";

			mainMenuStringExciter = new HUDItemExciterEntity(null);

			mainMenuExitConfirmQuestionString = AssetLocator.TitleFont.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.TopCentered,
				new Vector2(0f, 0.475f),
				MAIN_MENU_EXIT_ANSWER_SCALE
			);
			mainMenuExitConfirmQuestionString.Color = new Vector4(MAIN_MENU_EXIT_QUESTION_COLOR, w: 0f);
			mainMenuExitConfirmQuestionString.Text = "Exit Game?";

			mainMenuExitConfirmYesString = AssetLocator.TitleFont.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.TopRight,
				new Vector2(0.6f, 0.6f),
				MAIN_MENU_EXIT_SELECTED_ANSWER_SCALE
			);
			mainMenuExitConfirmYesString.Color = new Vector4(MAIN_MENU_EXIT_ANSWER_COLOR, w: 0f);
			mainMenuExitConfirmYesString.Text = "YES";

			mainMenuExitConfirmNoString = AssetLocator.TitleFont.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.TopLeft,
				new Vector2(0.64f, 0.6f),
				MAIN_MENU_EXIT_ANSWER_SCALE
			);
			mainMenuExitConfirmNoString.Color = new Vector4(MAIN_MENU_EXIT_ANSWER_COLOR, w: 0f);
			mainMenuExitConfirmNoString.Text = "NO";
		}

		private static void MainMenuDisposeComponents() {
			triggerUp -= MainMenuChangeOptionUp;
			triggerRight -= MainMenuChangeOptionUp;
			triggerDown -= MainMenuChangeOptionDown;
			triggerLeft -= MainMenuChangeOptionDown;
			triggerConfirm -= MainMenuConfirmOption;
			triggerBackOut -= MainMenuBackOut;
			tick -= MainMenuTick;

			if (mainMenuELLogo != null) {
				mainMenuELLogo.Dispose();
				mainMenuELLogo = null;
			}
			foreach (var confetto in mainMenuELLogoConfetti.Keys) {
				confetto.Dispose();
			}
			mainMenuELLogoConfetti.Clear();

			if (mainMenuPlayButtonL != null) {
				mainMenuPlayButtonL.Dispose();
				mainMenuPlayButtonL = null;
			}
			if (mainMenuPlayButtonR != null) {
				mainMenuPlayButtonR.Dispose();
				mainMenuPlayButtonR = null;
			}
			if (mainMenuMedalsButtonL != null) {
				mainMenuMedalsButtonL.Dispose();
				mainMenuMedalsButtonL = null;
			}
			if (mainMenuMedalsButtonR != null) {
				mainMenuMedalsButtonR.Dispose();
				mainMenuMedalsButtonR = null;
			}
			if (mainMenuOptionsButtonL != null) {
				mainMenuOptionsButtonL.Dispose();
				mainMenuOptionsButtonL = null;
			}
			if (mainMenuOptionsButtonR != null) {
				mainMenuOptionsButtonR.Dispose();
				mainMenuOptionsButtonR = null;
			}
			if (mainMenuExitButtonL != null) {
				mainMenuExitButtonL.Dispose();
				mainMenuExitButtonL = null;
			}
			if (mainMenuExitButtonR != null) {
				mainMenuExitButtonR.Dispose();
				mainMenuExitButtonR = null;
			}
			if (mainMenuButtonFrontL != null) {
				mainMenuButtonFrontL.Dispose();
				mainMenuButtonFrontL = null;
			}
			if (mainMenuButtonFrontR != null) {
				mainMenuButtonFrontR.Dispose();
				mainMenuButtonFrontR = null;
			}
			if (mainMenuButtonRingL != null) {
				mainMenuButtonRingL.Dispose();
				mainMenuButtonRingL = null;
			}
			if (mainMenuButtonRingR != null) {
				mainMenuButtonRingR.Dispose();
				mainMenuButtonRingR = null;
			}
			if (mainMenuButtonRingExciterL != null) {
				mainMenuButtonRingExciterL.Dispose();
				mainMenuButtonRingExciterL = null;
			}
			if (mainMenuButtonRingExciterR != null) {
				mainMenuButtonRingExciterR.Dispose();
				mainMenuButtonRingExciterR = null;
			}
			if (mainMenuPlayString != null) {
				mainMenuPlayString.Dispose();
				mainMenuPlayString = null;
			}
			if (mainMenuMedalsString != null) {
				mainMenuMedalsString.Dispose();
				mainMenuMedalsString = null;
			}
			if (mainMenuOptionsString != null) {
				mainMenuOptionsString.Dispose();
				mainMenuOptionsString = null;
			}
			if (mainMenuExitString != null) {
				mainMenuExitString.Dispose();
				mainMenuExitString = null;
			}
			if (mainMenuStringExciter != null) {
				mainMenuStringExciter.Dispose();
				mainMenuStringExciter = null;
			}

			if (mainMenuExitConfirmQuestionString != null) {
				mainMenuExitConfirmQuestionString.Dispose();
				mainMenuExitConfirmQuestionString = null;
			}
			if (mainMenuExitConfirmYesString != null) {
				mainMenuExitConfirmYesString.Dispose();
				mainMenuExitConfirmYesString = null;
			}
			if (mainMenuExitConfirmNoString != null) {
				mainMenuExitConfirmNoString.Dispose();
				mainMenuExitConfirmNoString = null;
			}
		}

		private static void MainMenuTickTransitionIn(float deltaTime, float fracComplete) {
			SetIdentTransitionAmount(1f - fracComplete, true);

			mainMenuELLogo.Scale = MAIN_MENU_EL_LOGO_TARGET_SCALE * Math.Min(fracComplete * 2f, 1f);

			float transition = MAIN_MENU_BUTTON_TRANSITION_HORIZONTAL_OFFSET * deltaTime;
			mainMenuPlayButtonL.AnchorOffset = new Vector2(mainMenuPlayButtonL.AnchorOffset.X - transition, mainMenuPlayButtonL.AnchorOffset.Y);
			mainMenuPlayButtonR.AnchorOffset = new Vector2(mainMenuPlayButtonR.AnchorOffset.X - transition, mainMenuPlayButtonR.AnchorOffset.Y);
			mainMenuMedalsButtonL.AnchorOffset = new Vector2(mainMenuMedalsButtonL.AnchorOffset.X - transition, mainMenuMedalsButtonL.AnchorOffset.Y);
			mainMenuMedalsButtonR.AnchorOffset = new Vector2(mainMenuMedalsButtonR.AnchorOffset.X - transition, mainMenuMedalsButtonR.AnchorOffset.Y);
			mainMenuOptionsButtonL.AnchorOffset = new Vector2(mainMenuOptionsButtonL.AnchorOffset.X - transition, mainMenuOptionsButtonL.AnchorOffset.Y);
			mainMenuOptionsButtonR.AnchorOffset = new Vector2(mainMenuOptionsButtonR.AnchorOffset.X - transition, mainMenuOptionsButtonR.AnchorOffset.Y);
			mainMenuExitButtonL.AnchorOffset = new Vector2(mainMenuExitButtonL.AnchorOffset.X - transition, mainMenuExitButtonL.AnchorOffset.Y);
			mainMenuExitButtonR.AnchorOffset = new Vector2(mainMenuExitButtonR.AnchorOffset.X - transition, mainMenuExitButtonR.AnchorOffset.Y);

			mainMenuPlayButtonL.SetAlpha(fracComplete);
			mainMenuPlayButtonR.SetAlpha(fracComplete);
			mainMenuMedalsButtonL.SetAlpha(fracComplete);
			mainMenuMedalsButtonR.SetAlpha(fracComplete);
			mainMenuOptionsButtonL.SetAlpha(fracComplete);
			mainMenuOptionsButtonR.SetAlpha(fracComplete);
			mainMenuExitButtonL.SetAlpha(fracComplete);
			mainMenuExitButtonR.SetAlpha(fracComplete);

			if (fracComplete >= 0.9f) {
				float finalTenthComplete = (fracComplete - 0.9f) * 10f;

				mainMenuPlayString.SetAlpha(MAIN_MENU_STRING_COLOR.W * finalTenthComplete);
				mainMenuMedalsString.SetAlpha(MAIN_MENU_STRING_COLOR.W * finalTenthComplete);
				mainMenuOptionsString.SetAlpha(MAIN_MENU_STRING_COLOR.W * finalTenthComplete);
				mainMenuExitString.SetAlpha(MAIN_MENU_STRING_COLOR.W * finalTenthComplete);
			}
			if (fracComplete >= 1f) {
				mainMenuButtonRingL.SetAlpha(MAIN_MENU_SELECTED_OPTION_ADORNMENTS_ALPHA);
				mainMenuButtonRingR.SetAlpha(MAIN_MENU_SELECTED_OPTION_ADORNMENTS_ALPHA);
				mainMenuButtonFrontL.SetAlpha(MAIN_MENU_SELECTED_OPTION_ADORNMENTS_ALPHA);
				mainMenuButtonFrontR.SetAlpha(MAIN_MENU_SELECTED_OPTION_ADORNMENTS_ALPHA);
				MainMenuSetSelectedButton(0);

				mainMenuStringExciter.CountPerSec = 15;
				mainMenuStringExciter.OpacityMultiplier = 0.85f;
				mainMenuStringExciter.Speed = 0.0025f;
				mainMenuStringExciter.ColorOverride = (Vector3) MAIN_MENU_STRING_SELECTED_COLOR;
				mainMenuStringExciter.Lifetime = 0.65f;
			}
		}

		private static void MainMenuTickTransitionOut(float deltaTime, float fracComplete) {
			mainMenuELLogo.SetAlpha(1f - fracComplete);

			mainMenuStringExciter.TargetObj = null;
			mainMenuButtonRingExciterL.TargetObj = null;
			mainMenuButtonRingExciterR.TargetObj = null;

			float transition = -MAIN_MENU_BUTTON_TRANSITION_HORIZONTAL_OFFSET * deltaTime;
			mainMenuPlayButtonL.AnchorOffset = new Vector2(mainMenuPlayButtonL.AnchorOffset.X - transition, mainMenuPlayButtonL.AnchorOffset.Y);
			mainMenuPlayButtonR.AnchorOffset = new Vector2(mainMenuPlayButtonR.AnchorOffset.X - transition, mainMenuPlayButtonR.AnchorOffset.Y);
			mainMenuMedalsButtonL.AnchorOffset = new Vector2(mainMenuMedalsButtonL.AnchorOffset.X - transition, mainMenuMedalsButtonL.AnchorOffset.Y);
			mainMenuMedalsButtonR.AnchorOffset = new Vector2(mainMenuMedalsButtonR.AnchorOffset.X - transition, mainMenuMedalsButtonR.AnchorOffset.Y);
			mainMenuOptionsButtonL.AnchorOffset = new Vector2(mainMenuOptionsButtonL.AnchorOffset.X - transition, mainMenuOptionsButtonL.AnchorOffset.Y);
			mainMenuOptionsButtonR.AnchorOffset = new Vector2(mainMenuOptionsButtonR.AnchorOffset.X - transition, mainMenuOptionsButtonR.AnchorOffset.Y);
			mainMenuExitButtonL.AnchorOffset = new Vector2(mainMenuExitButtonL.AnchorOffset.X - transition, mainMenuExitButtonL.AnchorOffset.Y);
			mainMenuExitButtonR.AnchorOffset = new Vector2(mainMenuExitButtonR.AnchorOffset.X - transition, mainMenuExitButtonR.AnchorOffset.Y);

			mainMenuPlayButtonL.SetAlpha(1f - fracComplete);
			mainMenuPlayButtonR.SetAlpha(1f - fracComplete);
			mainMenuMedalsButtonL.SetAlpha(1f - fracComplete);
			mainMenuMedalsButtonR.SetAlpha(1f - fracComplete);
			mainMenuOptionsButtonL.SetAlpha(1f - fracComplete);
			mainMenuOptionsButtonR.SetAlpha(1f - fracComplete);
			mainMenuExitButtonL.SetAlpha(1f - fracComplete);
			mainMenuExitButtonR.SetAlpha(1f - fracComplete);

			mainMenuPlayString.SetAlpha(MAIN_MENU_STRING_COLOR.W * (1f - fracComplete));
			mainMenuMedalsString.SetAlpha(MAIN_MENU_STRING_COLOR.W * (1f - fracComplete));
			mainMenuOptionsString.SetAlpha(MAIN_MENU_STRING_COLOR.W * (1f - fracComplete));
			mainMenuExitString.SetAlpha(MAIN_MENU_STRING_COLOR.W * (1f - fracComplete));
			mainMenuButtonRingL.SetAlpha(1f - fracComplete);
			mainMenuButtonRingR.SetAlpha(1f - fracComplete);
			mainMenuButtonFrontL.SetAlpha(1f - fracComplete);
			mainMenuButtonFrontR.SetAlpha(1f - fracComplete);
		}

		private static void MainMenuSetSelectedButton(int button) {
			mainMenuSelectedOption = button;
			switch (button) {
				case 3:
					mainMenuButtonRingL.AnchorOffset = mainMenuExitButtonL.AnchorOffset;
					mainMenuButtonRingR.AnchorOffset = mainMenuExitButtonL.AnchorOffset;
					mainMenuButtonFrontL.AnchorOffset = mainMenuExitButtonL.AnchorOffset;
					mainMenuButtonFrontR.AnchorOffset = mainMenuExitButtonL.AnchorOffset;
					mainMenuButtonFrontL.Texture = AssetLocator.MainMenuExitButtonFront;
					mainMenuButtonFrontR.Texture = AssetLocator.MainMenuExitButtonFront;
					mainMenuExitString.Color = MAIN_MENU_STRING_SELECTED_COLOR;
					mainMenuOptionsString.Color = MAIN_MENU_STRING_COLOR;
					mainMenuMedalsString.Color = MAIN_MENU_STRING_COLOR;
					mainMenuPlayString.Color = MAIN_MENU_STRING_COLOR;
					mainMenuStringExciter.TargetObj = mainMenuExitString;
					break;
				case 2:
					mainMenuButtonRingL.AnchorOffset = mainMenuOptionsButtonL.AnchorOffset;
					mainMenuButtonRingR.AnchorOffset = mainMenuOptionsButtonL.AnchorOffset;
					mainMenuButtonFrontL.AnchorOffset = mainMenuOptionsButtonL.AnchorOffset;
					mainMenuButtonFrontR.AnchorOffset = mainMenuOptionsButtonL.AnchorOffset;
					mainMenuButtonFrontL.Texture = AssetLocator.MainMenuOptionsButtonFront;
					mainMenuButtonFrontR.Texture = AssetLocator.MainMenuOptionsButtonFront;
					mainMenuOptionsString.Color = MAIN_MENU_STRING_SELECTED_COLOR;
					mainMenuExitString.Color = MAIN_MENU_STRING_COLOR;
					mainMenuMedalsString.Color = MAIN_MENU_STRING_COLOR;
					mainMenuPlayString.Color = MAIN_MENU_STRING_COLOR;
					mainMenuStringExciter.TargetObj = mainMenuOptionsString;
					break;
				case 1:
					mainMenuButtonRingL.AnchorOffset = mainMenuMedalsButtonL.AnchorOffset;
					mainMenuButtonRingR.AnchorOffset = mainMenuMedalsButtonL.AnchorOffset;
					mainMenuButtonFrontL.AnchorOffset = mainMenuMedalsButtonL.AnchorOffset;
					mainMenuButtonFrontR.AnchorOffset = mainMenuMedalsButtonL.AnchorOffset;
					mainMenuButtonFrontL.Texture = AssetLocator.MainMenuMedalsButtonFront;
					mainMenuButtonFrontR.Texture = AssetLocator.MainMenuMedalsButtonFront;
					mainMenuMedalsString.Color = MAIN_MENU_STRING_SELECTED_COLOR;
					mainMenuOptionsString.Color = MAIN_MENU_STRING_COLOR;
					mainMenuExitString.Color = MAIN_MENU_STRING_COLOR;
					mainMenuPlayString.Color = MAIN_MENU_STRING_COLOR;
					mainMenuStringExciter.TargetObj = mainMenuMedalsString;
					break;
				default:
					mainMenuButtonRingL.AnchorOffset = mainMenuPlayButtonL.AnchorOffset;
					mainMenuButtonRingR.AnchorOffset = mainMenuPlayButtonL.AnchorOffset;
					mainMenuButtonFrontL.AnchorOffset = mainMenuPlayButtonL.AnchorOffset;
					mainMenuButtonFrontR.AnchorOffset = mainMenuPlayButtonL.AnchorOffset;
					mainMenuButtonFrontL.Texture = AssetLocator.MainMenuPlayButtonFront;
					mainMenuButtonFrontR.Texture = AssetLocator.MainMenuPlayButtonFront;
					mainMenuPlayString.Color = MAIN_MENU_STRING_SELECTED_COLOR;
					mainMenuOptionsString.Color = MAIN_MENU_STRING_COLOR;
					mainMenuMedalsString.Color = MAIN_MENU_STRING_COLOR;
					mainMenuExitString.Color = MAIN_MENU_STRING_COLOR;
					mainMenuStringExciter.TargetObj = mainMenuPlayString;
					break;
			}
		}

		private static void MainMenuChangeOptionUp() {
			HUDSound.UIPreviousOption.Play();
			if (mainMenuInExitConfirmationScreen) {
				MainMenuSwitchExitConfirmOption();
			}
			else {
				if (mainMenuSelectedOption == 0) MainMenuSetSelectedButton(3);
				else MainMenuSetSelectedButton((mainMenuSelectedOption - 1) % 4);
			}
		}
		private static void MainMenuChangeOptionDown() {
			HUDSound.UINextOption.Play();
			if (mainMenuInExitConfirmationScreen) {
				MainMenuSwitchExitConfirmOption();
			}
			else {
				MainMenuSetSelectedButton((mainMenuSelectedOption + 1) % 4);
			}
		}

		private static void MainMenuConfirmOption() {
			if (mainMenuInExitConfirmationScreen) {
				MainMenuExitConfirm();
				return;
			}

			HUDSound.UISelectOption.Play();

			switch (mainMenuSelectedOption) {
				case 0:
					MainMenuTransitionOut();
					currentTransitionComplete += () => PlayMenuTransitionIn();
					break;
				case 1:
					MainMenuTransitionOut();
					currentTransitionComplete += () => MedalsMenuTransitionIn();
					break;
				case 2:
					MainMenuTransitionOut();
					currentTransitionComplete += () => OptionsTransitionIn();
					break;
				case 3:
					MainMenuTransitionOut(disposeComponents: false);
					currentTransitionComplete += MainMenuSetUpExitConfirmation;
					break;
			}
		}

		private static void MainMenuBackOut() {
			if (mainMenuInExitConfirmationScreen) {
				MainMenuExitBackOut();
			}
			else {
				HUDSound.UISelectNegativeOption.Play();
				MainMenuTransitionOut(disposeComponents: false);
				currentTransitionComplete += MainMenuSetUpExitConfirmation;
			}
		}

		private static void MainMenuSetUpExitConfirmation() {
			HUDSound.XAnnounceExit.Play();
			mainMenuInExitConfirmationScreen = true;
			mainMenuExitYesSelected = true;
			AddTransitionTickEvent(MainMenuTickTransitionInExitConfirm);
			BeginTransition(MAIN_MENU_EXIT_CONFIRM_TRANSITION_IN_TIME);
			mainMenuStringExciter.TargetObj = mainMenuExitConfirmYesString;
			mainMenuStringExciter.CountPerSec = 15f;
			mainMenuStringExciter.OpacityMultiplier = 0.2f;
			mainMenuStringExciter.Speed = 0.015f;
			mainMenuStringExciter.ColorOverride = new Vector3(1f, 1f, 0.7f);
			mainMenuStringExciter.Lifetime = 1.1f;

			menuIdentTex.Texture = AssetLocator.MainMenuExitButton;
			menuIdentString.Text = "QUIT";
		}

		private static void MainMenuTickTransitionInExitConfirm(float deltaTime, float fracComplete) {
			mainMenuExitConfirmQuestionString.SetAlpha(fracComplete);
			mainMenuExitConfirmYesString.SetAlpha(fracComplete);
			mainMenuExitConfirmNoString.SetAlpha(fracComplete);

			SetIdentTransitionAmount(fracComplete, false);
		}

		private static void MainMenuSwitchExitConfirmOption() {
			mainMenuExitYesSelected = !mainMenuExitYesSelected;
			if (mainMenuExitYesSelected) {
				mainMenuStringExciter.TargetObj = mainMenuExitConfirmYesString;
				mainMenuExitConfirmYesString.Scale = MAIN_MENU_EXIT_SELECTED_ANSWER_SCALE;
				mainMenuExitConfirmNoString.Scale = MAIN_MENU_EXIT_ANSWER_SCALE;
			}
			else {
				mainMenuStringExciter.TargetObj = mainMenuExitConfirmNoString;
				mainMenuExitConfirmYesString.Scale = MAIN_MENU_EXIT_ANSWER_SCALE;
				mainMenuExitConfirmNoString.Scale = MAIN_MENU_EXIT_SELECTED_ANSWER_SCALE;
			}
		}

		private static void MainMenuExitConfirm() {
			Logger.Log("Exiting due to user request.");
			if (mainMenuExitYesSelected) LosgapSystem.Exit();
			else MainMenuExitBackOut();
		}

		private static void MainMenuExitBackOut() {
			HUDSound.UISelectNegativeOption.Play();
			mainMenuInExitConfirmationScreen = false;
			AddTransitionTickEvent(MainMenuTickTransitionOutExitConfirm);
			BeginTransition(MAIN_MENU_EXIT_CONFIRM_TRANSITION_OUT_TIME);
			currentTransitionComplete += () => MainMenuTransitionIn(true);
		}

		private static void MainMenuTickTransitionOutExitConfirm(float deltaTime, float fracComplete) {
			mainMenuStringExciter.TargetObj = null;
			mainMenuExitConfirmQuestionString.SetAlpha(1f - fracComplete);
			mainMenuExitConfirmYesString.SetAlpha(1f - fracComplete);
			mainMenuExitConfirmNoString.SetAlpha(1f - fracComplete);

			SetIdentTransitionAmount(fracComplete, true);
		}
	}
}