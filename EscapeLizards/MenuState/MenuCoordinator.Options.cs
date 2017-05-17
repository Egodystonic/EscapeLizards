// All code copyright (c) 2016 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 20 07 2016 at 15:14 by Ben Bowen

using System;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using System.Security.Claims;
using Ophidian.Losgap;
using Ophidian.Losgap.Entities;
using Ophidian.Losgap.Input;
using Ophidian.Losgap.Rendering;

namespace Egodystonic.EscapeLizards {
	public static partial class MenuCoordinator {
		private class OptionsLine {
			public readonly FontString LabelString;
			public readonly FontString ValueString;
			public readonly HUDTexture SeparatorBelow;
			public readonly bool IsHeader;

			public OptionsLine(FontString labelString, FontString valueString, HUDTexture separatorBelow, bool isHeader) {
				LabelString = labelString;
				ValueString = valueString;
				SeparatorBelow = separatorBelow;
				IsHeader = isHeader;
			}

			public void SetAlpha(float alpha) {
				if (LabelString != null) LabelString.SetAlpha(alpha);
				if (ValueString != null) ValueString.SetAlpha(alpha);
				if (SeparatorBelow != null) SeparatorBelow.SetAlpha(Math.Min(alpha, OPTIONS_SEPARATOR_ALPHA));
			}

			public float GetAlpha() {
				if (LabelString != null) return LabelString.Color.W;
				if (ValueString != null) return ValueString.Color.W;
				if (SeparatorBelow != null) return SeparatorBelow.Color.W;
				return 0f;
			}

			public void AdjustAlpha(float adjustment) {
				if (LabelString != null) LabelString.AdjustAlpha(adjustment);
				if (ValueString != null) ValueString.AdjustAlpha(adjustment);
				if (SeparatorBelow != null) {
					SeparatorBelow.AdjustAlpha(adjustment);
					if (SeparatorBelow.Color.W > OPTIONS_SEPARATOR_ALPHA) SeparatorBelow.SetAlpha(OPTIONS_SEPARATOR_ALPHA);
				}
			}

			public void Shift(float amount) {
				if (LabelString != null) LabelString.AnchorOffset = new Vector2(LabelString.AnchorOffset.X, LabelString.AnchorOffset.Y + amount);
				if (ValueString != null) ValueString.AnchorOffset = new Vector2(ValueString.AnchorOffset.X, ValueString.AnchorOffset.Y + amount);
				if (SeparatorBelow != null) SeparatorBelow.AnchorOffset = new Vector2(SeparatorBelow.AnchorOffset.X, SeparatorBelow.AnchorOffset.Y + amount);
			}

			public void SetAbsolutePosition(int position) {
				float heightOffset = OPTIONS_STARTING_HEIGHT + OPTIONS_LINE_TOTAL_HEIGHT * position;
				if (LabelString != null) LabelString.AnchorOffset = new Vector2(LabelString.AnchorOffset.X, heightOffset + OPTIONS_LABEL_Y_OFFSET);
				if (ValueString != null) ValueString.AnchorOffset = new Vector2(ValueString.AnchorOffset.X, heightOffset + OPTIONS_VALUE_Y_OFFSET);
				if (SeparatorBelow != null) SeparatorBelow.AnchorOffset = new Vector2(SeparatorBelow.AnchorOffset.X, heightOffset + OPTIONS_SEPARATOR_Y_OFFSET);
			}

			public void Dispose() {
				if (LabelString != null) LabelString.Dispose();
				if (ValueString != null) ValueString.Dispose();
				if (SeparatorBelow != null) SeparatorBelow.Dispose();
			}

			public float GetNormalizedDistanceFromActiveArea() {
				const float STARTING_HEIGHT = OPTIONS_STARTING_HEIGHT;
				const float ENDING_HEIGHT = OPTIONS_STARTING_HEIGHT + (OPTIONS_NUM_LINES_ON_SCREEN - 1) * OPTIONS_LINE_TOTAL_HEIGHT;

				if (LabelString == null) return 1f;
				else if (LabelString.AnchorOffset.Y < STARTING_HEIGHT) return Math.Min(1f, (STARTING_HEIGHT - LabelString.AnchorOffset.Y) / OPTIONS_LINE_TOTAL_HEIGHT);
				else if (LabelString.AnchorOffset.Y > ENDING_HEIGHT) return Math.Min(1f, (LabelString.AnchorOffset.Y - ENDING_HEIGHT) / OPTIONS_LINE_TOTAL_HEIGHT);
				else return 0f;
			}
		}

		private const float OPTIONS_TRANSITION_IN_TIME = 0.3f;
		private const float OPTIONS_TRANSITION_OUT_TIME = 0.2f;
		private static readonly List<OptionsLine> optionsLines = new List<OptionsLine>();
		private const float OPTIONS_LINE_TOTAL_HEIGHT = 0.088f;
		private const int OPTIONS_NUM_LINES_ON_SCREEN = (int) (0.85f / OPTIONS_LINE_TOTAL_HEIGHT);
		private static readonly Vector3 OPTIONS_HEADER_COLOR = new Vector3(255f / 255f, 243f / 255f, 170f / 255f);
		private static readonly Vector3 OPTIONS_LABEL_COLOR = new Vector3(187f / 255f, 255f / 255f, 158f / 255f);
		private static readonly Vector3 OPTIONS_VALUE_COLOR = new Vector3(249 / 255f, 255f / 255f, 168f / 255f);
		private static readonly Vector2 OPTIONS_HEADER_SCALE = Vector2.ONE * 0.7f;
		private static readonly Vector2 OPTIONS_LABEL_SCALE = Vector2.ONE * 0.65f;
		private static readonly Vector2 OPTIONS_VALUE_SCALE = OPTIONS_LABEL_SCALE;
		private const float OPTIONS_LABEL_Y_OFFSET = -0.035f;
		private const float OPTIONS_VALUE_Y_OFFSET = -0.03f;
		private const float OPTIONS_STARTING_HEIGHT = 0.15f;
		private const float OPTIONS_HEADER_X = 0.035f;
		private const float OPTIONS_HEADER_Y_OFFSET = -0.04f;
		private const float OPTIONS_LABEL_X = 0.08f;
		private const float OPTIONS_VALUE_X = 0.0825f;
		private const float OPTIONS_SEPARATOR_Y_OFFSET = 0.01f;
		private const float OPTIONS_SEPARATOR_ALPHA = 0.8f;
		private static readonly Vector2 OPTIONS_SEPARATOR_SCALE = new Vector2(0.9f, 0.01f);
		private static int optionsTopLineIndex;
		private static int optionsRelativeSelectedLineIndex;
		private static int optionsAbsoluteSelectedLineIndex;
		private static HUDTexture optionsChevronsLeft, optionsChevronsRight, optionsChevronsUp, optionsChevronsDown;
		private const float OPTIONS_UP_CHEVRON_Y = 0.065f;
		private const float OPTIONS_DOWN_CHEVRON_Y = 0.035f;
		private static HUDTexture optionsSelectedLineFillTex;
		private const float OPTIONS_SELECTION_FILL_OFFSET_Y = -0.041f;
		private const float OPTIONS_SELECTION_FILL_ALPHA = 0.6f;
		private static float optionsLinesToBeMovedUp = 0f;
		private const float OPTIONS_LINE_MOVEMENT_PER_SEC = 8f;
		private const float OPTIONS_CHEVRON_MARGIN_X = 0.02f;
		private const float OPTIONS_CHEVRON_OFFSET_X = -0.02f;
		private const float OPTIONS_CHEVRON_OFFSET_Y = 0.02f;
		private static float optionsLRChevronOffset = 0f;
		private const float OPTIONS_CHEVRON_OFFSET_PER_SEC = 0.014f;
		private const float OPTIONS_CHEVRON_OFFSET_MAX = 0.01f;
		private static float optionsUDChevronAlpha = 0f;
		private const float OPTIONS_CHEVRON_ALPHA_PER_SEC = 3f;
		private static readonly Vector3 OPTIONS_CONFIRM_COLOR = new Vector3(0.8f, 1f, 0.8f);
		private static readonly Vector3 OPTIONS_CANCEL_COLOR = new Vector3(1f, 0.8f, 0.8f);
		private const float OPTIONS_CONFIRM_CANCEL_OFFSET = -0.036f;
		private const float OPTIONS_CONFIRM_CANCEL_SELECTION_FILL_WIDTH = 0.25f;
		private const float OPTIONS_FILL_RESCALE_MULTIPLIER = 10f;
		private static string[] curLineOptions;
		private static int curLineSelectedOptionIndex;

		private static void OptionsTransitionIn(bool createComponents = true) {
			if (!inDeferenceMode) HUDSound.XAnnounceOptions.Play();

			CurrentMenuState = MenuState.OptionsMenu;
			if (createComponents) OptionsCreateComponents();
			OptionsSetMenuPositions(1, 0);

			AddTransitionTickEvent(OptionsTickTransitionIn);
			BeginTransition(OPTIONS_TRANSITION_IN_TIME);

			menuIdentTex.Texture = AssetLocator.MainMenuOptionsButton;
			menuIdentString.Text = "OPTIONS";
		}

		private static void OptionsTransitionOut(bool disposeComponents = true) {
			AddTransitionTickEvent(OptionsTickTransitionOut);
			BeginTransition(OPTIONS_TRANSITION_OUT_TIME);
			if (disposeComponents) currentTransitionComplete += OptionsDisposeComponents;
		}

		private static void OptionsTickTransitionIn(float deltaTime, float fracComplete) {
			for (int i = 0; i < OPTIONS_NUM_LINES_ON_SCREEN; ++i) {
				float alphaBoost = (OPTIONS_NUM_LINES_ON_SCREEN - (i + 1)) * 0.1f;
				optionsLines[i].SetAlpha(Math.Min(1f, fracComplete * (1f + alphaBoost)));
			}

			optionsChevronsDown.SetAlpha(fracComplete);
			optionsSelectedLineFillTex.SetAlpha(Math.Min(fracComplete, OPTIONS_SELECTION_FILL_ALPHA));

			SetIdentTransitionAmount(fracComplete, false);
		}

		private static void OptionsTickTransitionOut(float deltaTime, float fracComplete) {
			for (int i = 0; i < optionsLines.Count; ++i) {
				optionsLines[i].SetAlpha(Math.Min(1f - fracComplete, optionsLines[i].GetAlpha()));
			}

			optionsChevronsUp.SetAlpha(1f - fracComplete);
			optionsChevronsDown.SetAlpha(1f - fracComplete);
			optionsChevronsLeft.SetAlpha(1f - fracComplete);
			optionsChevronsRight.SetAlpha(1f - fracComplete);
			optionsSelectedLineFillTex.SetAlpha(1f - fracComplete);

			SetIdentTransitionAmount(fracComplete, true);
		}

		private static void OptionsSetMenuPositions(int selected, int topLine) {
			if (optionsRelativeSelectedLineIndex == selected && optionsTopLineIndex == topLine) return;

			optionsLinesToBeMovedUp = optionsTopLineIndex - topLine;
			optionsSelectedLineFillTex.SetAlpha(0f);
			float fillTexY = OPTIONS_STARTING_HEIGHT + OPTIONS_SEPARATOR_Y_OFFSET + OPTIONS_LINE_TOTAL_HEIGHT * selected + OPTIONS_SELECTION_FILL_OFFSET_Y;
			if (topLine + selected >= optionsLines.Count - 2) fillTexY -= 0.035f;
			optionsSelectedLineFillTex.AnchorOffset = new Vector2(optionsSelectedLineFillTex.AnchorOffset.X, fillTexY);

			optionsRelativeSelectedLineIndex = selected;
			optionsTopLineIndex = topLine;

			optionsAbsoluteSelectedLineIndex = optionsTopLineIndex + optionsRelativeSelectedLineIndex;
			var selectedLine = optionsLines[optionsAbsoluteSelectedLineIndex];

			if (selectedLine.ValueString == null) {
				optionsChevronsLeft.SetAlpha(0f);
				optionsChevronsRight.SetAlpha(0f);
			}
			else {
				OptionsRefreshLRChevronPositions();

				curLineOptions = OptionsGetOptionsForIndex(optionsAbsoluteSelectedLineIndex).Split('~');
				curLineSelectedOptionIndex = curLineOptions.IndexOf(selectedLine.ValueString.Text);

				optionsChevronsLeft.SetAlpha(curLineSelectedOptionIndex <= 0 ? 0f : 1f);
				optionsChevronsRight.SetAlpha(curLineSelectedOptionIndex <= 0 ? 0f : 1f);
			}
		}

		private static void OptionsCreateComponents() {
			OptionsDisposeComponents();

			tick += OptionsTick;
			triggerUp += OptionsMoveUp;
			triggerDown += OptionsMoveDown;
			triggerBackOut += OptionsBackOut;
			triggerConfirm += OptionsConfirm;
			triggerLeft += OptionsDecreaseValue;
			triggerRight += OptionsIncreaseValue;

			float heightOffset = OPTIONS_STARTING_HEIGHT - OPTIONS_LINE_TOTAL_HEIGHT;

			optionsLines.Clear();

			heightOffset += OPTIONS_LINE_TOTAL_HEIGHT;
			optionsLines.Add(OptionsCreateHeader(heightOffset, "Video"));
			heightOffset += OPTIONS_LINE_TOTAL_HEIGHT;
			optionsLines.Add(OptionsCreateField(heightOffset, "Framerate Cap", (Config.MaxFrameRateHz == 0L ? "None" : Config.MaxFrameRateHz.ToString())));
			heightOffset += OPTIONS_LINE_TOTAL_HEIGHT;
			optionsLines.Add(OptionsCreateField(heightOffset, "Resolution", AssetLocator.MainWindow.Width + " x " + AssetLocator.MainWindow.Height));
			heightOffset += OPTIONS_LINE_TOTAL_HEIGHT;
			optionsLines.Add(OptionsCreateField(heightOffset, "Fullscreen Mode", OptionsGetFullscreenStateValue(AssetLocator.MainWindow.FullscreenState)));
			heightOffset += OPTIONS_LINE_TOTAL_HEIGHT;
			optionsLines.Add(OptionsCreateField(heightOffset, "Anisotropy", ((int) Config.DefaultSamplingAnisoLevel) + "x"));
			heightOffset += OPTIONS_LINE_TOTAL_HEIGHT;
			optionsLines.Add(OptionsCreateField(heightOffset, "Depth of Field", Single.IsPositiveInfinity(Config.DofLensMaxBlurDist) ? "Off" : (Config.DofLensMaxBlurDist == PhysicsManager.ONE_METRE_SCALED * 240f ? "On" : "Custom")));
			heightOffset += OPTIONS_LINE_TOTAL_HEIGHT;
			optionsLines.Add(OptionsCreateField(heightOffset, "Outlining", !Config.EnableOutlining ? "Off" : "On"));
			heightOffset += OPTIONS_LINE_TOTAL_HEIGHT;
			optionsLines.Add(OptionsCreateField(heightOffset, "Physics Quality", OptionsGetPhysicsQualityValue(Config.PhysicsLevel)));
			heightOffset += OPTIONS_LINE_TOTAL_HEIGHT;
			optionsLines.Add(OptionsCreateField(heightOffset, "Max Dynamic Lights", OptionsGetMaxDynLightsValue(Config.DynamicLightCap)));

			heightOffset += OPTIONS_LINE_TOTAL_HEIGHT;
			optionsLines.Add(OptionsCreateHeader(heightOffset, String.Empty));
			heightOffset += OPTIONS_LINE_TOTAL_HEIGHT;
			optionsLines.Add(OptionsCreateHeader(heightOffset, "Audio"));
			heightOffset += OPTIONS_LINE_TOTAL_HEIGHT;
			optionsLines.Add(OptionsCreateField(heightOffset, "Sound Effects Vol", OptionsGetVolumeValue(Config.SoundEffectVolume)));
			heightOffset += OPTIONS_LINE_TOTAL_HEIGHT;
			optionsLines.Add(OptionsCreateField(heightOffset, "Menu/HUD Sounds Vol", OptionsGetVolumeValue(Config.HUDVolume)));
			heightOffset += OPTIONS_LINE_TOTAL_HEIGHT;
			optionsLines.Add(OptionsCreateField(heightOffset, "Music Vol", OptionsGetVolumeValue(Config.MusicVolume)));

			heightOffset += OPTIONS_LINE_TOTAL_HEIGHT;
			optionsLines.Add(OptionsCreateHeader(heightOffset, String.Empty));
			heightOffset += OPTIONS_LINE_TOTAL_HEIGHT;
			optionsLines.Add(OptionsCreateHeader(heightOffset, "Controller"));
			heightOffset += OPTIONS_LINE_TOTAL_HEIGHT;
			optionsLines.Add(OptionsCreateField(heightOffset, "LS Tilt Deadzone", OptionsGetLSDeadzoneValue(Config.BoardTiltDeadZone)));
			heightOffset += OPTIONS_LINE_TOTAL_HEIGHT;
			optionsLines.Add(OptionsCreateField(heightOffset, "RS Cam Deadzone", OptionsGetRSDeadzoneValue(Config.CamOrientDeadZone)));
			heightOffset += OPTIONS_LINE_TOTAL_HEIGHT;
			optionsLines.Add(OptionsCreateField(heightOffset, "Invert RS Cam Up/Down", Config.InvertCamControlY ? "Yes" : "No"));
			heightOffset += OPTIONS_LINE_TOTAL_HEIGHT;
			optionsLines.Add(OptionsCreateField(heightOffset, "Camera Height Adjust", "+" + (Config.CameraHeightOffset / PhysicsManager.ONE_METRE_SCALED).ToString(2) + "m"));
			heightOffset += OPTIONS_LINE_TOTAL_HEIGHT;
			optionsLines.Add(OptionsCreateField(heightOffset, "Camera Distance Adjust", "+" + (Config.CameraDistanceOffset / PhysicsManager.ONE_METRE_SCALED).ToString(2) + "m"));

			heightOffset += OPTIONS_LINE_TOTAL_HEIGHT;
			optionsLines.Add(OptionsCreateHeader(heightOffset, String.Empty));
			heightOffset += OPTIONS_LINE_TOTAL_HEIGHT;
			optionsLines.Add(OptionsCreateHeader(heightOffset, "Keyboard"));
			heightOffset += OPTIONS_LINE_TOTAL_HEIGHT;
			optionsLines.Add(OptionsCreateField(heightOffset, "Tilt Forward", Config.Key_TiltGameboardForward.ToString()));
			heightOffset += OPTIONS_LINE_TOTAL_HEIGHT;
			optionsLines.Add(OptionsCreateField(heightOffset, "Tilt Backward", Config.Key_TiltGameboardBackward.ToString()));
			heightOffset += OPTIONS_LINE_TOTAL_HEIGHT;
			optionsLines.Add(OptionsCreateField(heightOffset, "Tilt Left", Config.Key_TiltGameboardLeft.ToString()));
			heightOffset += OPTIONS_LINE_TOTAL_HEIGHT;
			optionsLines.Add(OptionsCreateField(heightOffset, "Tilt Right", Config.Key_TiltGameboardRight.ToString()));
			heightOffset += OPTIONS_LINE_TOTAL_HEIGHT;
			optionsLines.Add(OptionsCreateField(heightOffset, "Flip Clockwise", Config.Key_FlipGameboardLeft.ToString()));
			heightOffset += OPTIONS_LINE_TOTAL_HEIGHT;
			optionsLines.Add(OptionsCreateField(heightOffset, "Flip Anticlockwise", Config.Key_FlipGameboardRight.ToString()));
			heightOffset += OPTIONS_LINE_TOTAL_HEIGHT;
			optionsLines.Add(OptionsCreateField(heightOffset, "Hop", Config.Key_HopEgg.ToString()));
			heightOffset += OPTIONS_LINE_TOTAL_HEIGHT;
			optionsLines.Add(OptionsCreateField(heightOffset, "Fast Restart Level", Config.Key_FastRestart.ToString()));

			heightOffset += OPTIONS_LINE_TOTAL_HEIGHT;
			optionsLines.Add(OptionsCreateHeader(heightOffset, String.Empty));
			heightOffset += OPTIONS_LINE_TOTAL_HEIGHT;
			var confirmLine = new OptionsLine(
				AssetLocator.TitleFont.AddString(
					AssetLocator.HudLayer,
					AssetLocator.MainWindow,
					ViewportAnchoring.TopCentered,
					new Vector2(0f, heightOffset),
					OPTIONS_HEADER_SCALE
				),
				null,
				null,
				false
			);
			confirmLine.LabelString.Text = "CONFIRM";
			confirmLine.LabelString.Color = OPTIONS_CONFIRM_COLOR;
			optionsLines.Add(confirmLine);
			heightOffset += OPTIONS_LINE_TOTAL_HEIGHT;
			var cancelLine = new OptionsLine(
				AssetLocator.TitleFont.AddString(
					AssetLocator.HudLayer,
					AssetLocator.MainWindow,
					ViewportAnchoring.TopCentered,
					new Vector2(0f, heightOffset),
					OPTIONS_HEADER_SCALE
				),
				null,
				null,
				false
			);
			cancelLine.LabelString.Text = "CANCEL";
			cancelLine.LabelString.Color = OPTIONS_CANCEL_COLOR;
			optionsLines.Add(cancelLine);

		

			optionsSelectedLineFillTex = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = ViewportAnchoring.TopCentered,
				AnchorOffset = new Vector2(0f, OPTIONS_STARTING_HEIGHT + OPTIONS_SEPARATOR_Y_OFFSET + OPTIONS_LINE_TOTAL_HEIGHT + OPTIONS_SELECTION_FILL_OFFSET_Y),
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.None,
				Color = new Vector4(1f, 0.7f, 0.4f, OPTIONS_SELECTION_FILL_ALPHA),
				Rotation = 0f,
				Scale = new Vector2(OPTIONS_SEPARATOR_SCALE.X, OPTIONS_LINE_TOTAL_HEIGHT * 0.8f),
				Texture = AssetLocator.LineSelectionFill,
				ZIndex = optionsLines[1].SeparatorBelow.ZIndex - 1
			};

			optionsChevronsLeft = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = ViewportAnchoring.TopRight,
				AnchorOffset = Vector2.ZERO,
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.PreserveHorizontalScaling,
				Color = Vector3.ONE,
				Rotation = MathUtils.PI,
				Scale = new Vector2(0.04f, 0.066f),
				Texture = AssetLocator.Chevrons,
				ZIndex = 4
			};
			optionsChevronsRight = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = ViewportAnchoring.TopRight,
				AnchorOffset = Vector2.ZERO,
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.PreserveHorizontalScaling,
				Color = Vector3.ONE,
				Rotation = 0f,
				Scale = optionsChevronsLeft.Scale,
				Texture = AssetLocator.Chevrons,
				ZIndex = 4
			};
			optionsChevronsUp = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = ViewportAnchoring.TopCentered,
				AnchorOffset = new Vector2(0f, OPTIONS_UP_CHEVRON_Y),
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.PreserveHorizontalScaling,
				Color = Vector3.ONE,
				Rotation = MathUtils.THREE_PI_OVER_TWO,
				Scale = new Vector2(0.06f, 0.1f),
				Texture = AssetLocator.Chevrons,
				ZIndex = optionsSelectedLineFillTex.ZIndex
			};
			optionsChevronsDown = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = ViewportAnchoring.BottomCentered,
				AnchorOffset = new Vector2(0f, OPTIONS_DOWN_CHEVRON_Y),
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.PreserveHorizontalScaling,
				Color = Vector3.ONE,
				Rotation = MathUtils.PI_OVER_TWO,
				Scale = optionsChevronsUp.Scale,
				Texture = AssetLocator.Chevrons,
				ZIndex = optionsSelectedLineFillTex.ZIndex
			};
		}

		private static void OptionsDisposeComponents() {
			tick -= OptionsTick;
			triggerUp -= OptionsMoveUp;
			triggerDown -= OptionsMoveDown;
			triggerBackOut -= OptionsBackOut;
			triggerConfirm -= OptionsConfirm;
			triggerLeft -= OptionsDecreaseValue;
			triggerRight -= OptionsIncreaseValue;

			foreach (var line in optionsLines) {
				line.Dispose();
			}

			optionsLines.Clear();

			if (optionsChevronsDown != null) {
				optionsChevronsDown.Dispose();
				optionsChevronsDown = null;
			}
			if (optionsChevronsUp != null) {
				optionsChevronsUp.Dispose();
				optionsChevronsUp = null;
			}
			if (optionsChevronsLeft != null) {
				optionsChevronsLeft.Dispose();
				optionsChevronsLeft = null;
			}
			if (optionsChevronsRight != null) {
				optionsChevronsRight.Dispose();
				optionsChevronsRight = null;
			}
			if (optionsSelectedLineFillTex != null) {
				optionsSelectedLineFillTex.Dispose();
				optionsSelectedLineFillTex = null;
			}
		}

		private static string OptionsGetFullscreenStateValue(WindowFullscreenState state) {
			switch (state) {
				case WindowFullscreenState.StandardFullscreen: return "Standard";
				case WindowFullscreenState.BorderlessFullscreen: return "Borderless";
				default: return "Windowed";
			}
		}

		private static string OptionsGetPhysicsQualityValue(uint numSillyBitsCopies) {
			switch (numSillyBitsCopies) {
				case 5U: return "Ultra";
				case 4U: return "High";
				case 3U: return "Standard";
				case 2U: return "Low";
				case 1U: return "Very Low";
				default: return "Custom";
			}
		}

		private static string OptionsGetMaxDynLightsValue(int? dynamicLightCap) {
			if (dynamicLightCap == null) return "Uncapped";
			switch (dynamicLightCap.Value) {
				case 16: return "Very Low";
				case 32: return "Low";
				case 48: return "Medium";
				case 64: return "High";
				case 96: return "Very High";
				default: return "Custom";
			}
		}

		private static string OptionsGetVolumeValue(float volume) {
			return ((int) (volume * 100f)).ToString("0") + "%";
		}

		private static string OptionsGetLSDeadzoneValue(short deadzone) {
			switch (deadzone) {
				case 32768 / 18: return "Very Small";
				case 32768 / 12: return "Small";
				case 32768 / 8: return "Moderate";
				case 32768 / 5: return "Large";
				case 32768 / 3: return "Very Large";
				default: return "Custom";
			}
		}

		private static string OptionsGetRSDeadzoneValue(short deadzone) {
			switch (deadzone) {
				case 32768 / 12: return "Very Small";
				case 32768 / 8: return "Small";
				case 32768 / 5: return "Moderate";
				case 32768 / 3: return "Large";
				case 32768 / 2: return "Very Large";
				default: return "Custom";
			}
		}

		private static OptionsLine OptionsCreateField(float heightOffset, string label, string value) {
			var result = new OptionsLine(
				AssetLocator.MainFont.AddString(
					AssetLocator.HudLayer,
					AssetLocator.MainWindow,
					ViewportAnchoring.TopLeft,
					new Vector2(OPTIONS_LABEL_X, heightOffset + OPTIONS_LABEL_Y_OFFSET),
					OPTIONS_LABEL_SCALE
				),
				AssetLocator.MainFont.AddString(
					AssetLocator.HudLayer,
					AssetLocator.MainWindow,
					ViewportAnchoring.TopRight,
					new Vector2(OPTIONS_VALUE_X, heightOffset + OPTIONS_VALUE_Y_OFFSET),
					OPTIONS_VALUE_SCALE
				),
				new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
					Anchoring = ViewportAnchoring.TopCentered,
					AnchorOffset = new Vector2(0f, heightOffset + OPTIONS_SEPARATOR_Y_OFFSET),
					AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.PreserveHorizontalScaling,
					Color = Vector3.ONE,
					Rotation = 0f,
					Scale = OPTIONS_SEPARATOR_SCALE,
					Texture = AssetLocator.LineSeparator,
					ZIndex = 3
				},
				false
			);

			result.LabelString.Text = label;
			result.LabelString.Color = OPTIONS_LABEL_COLOR;

			result.ValueString.Text = value;
			result.ValueString.Color = OPTIONS_VALUE_COLOR;

			return result;
		}

		private static OptionsLine OptionsCreateHeader(float heightOffset, string header) {
			var result = new OptionsLine(
				AssetLocator.TitleFont.AddString(
					AssetLocator.HudLayer,
					AssetLocator.MainWindow,
					ViewportAnchoring.TopLeft,
					new Vector2(OPTIONS_HEADER_X, heightOffset + OPTIONS_HEADER_Y_OFFSET),
					OPTIONS_HEADER_SCALE
				),
				null,
				null,
				true
			);
			result.LabelString.Text = header;
			result.LabelString.Color = OPTIONS_HEADER_COLOR;

			return result;
		}

		private static void OptionsTick(float deltaTime) {
			var selectedLine = optionsLines[optionsAbsoluteSelectedLineIndex];
			bool selectedIsConfirmOrCancel = optionsAbsoluteSelectedLineIndex >= optionsLines.Count - 2;

			if (optionsLinesToBeMovedUp != 0f) {
				float moveFracThisTick = OPTIONS_LINE_MOVEMENT_PER_SEC * deltaTime;
				if (optionsLinesToBeMovedUp < 0f) moveFracThisTick = -moveFracThisTick;
				if (Math.Abs(optionsLinesToBeMovedUp) > 1f) moveFracThisTick *= (float) Math.Floor(Math.Abs(optionsLinesToBeMovedUp)) * 3f;

				if (Math.Abs(moveFracThisTick) >= Math.Abs(optionsLinesToBeMovedUp)) {
					optionsLinesToBeMovedUp = 0f;
					for (int i = 0; i < optionsLines.Count; ++i) {
						optionsLines[i].SetAbsolutePosition(i - optionsTopLineIndex);
						if (i >= optionsLines.Count - 2) optionsLines[i].Shift(OPTIONS_CONFIRM_CANCEL_OFFSET);
						if (i >= optionsTopLineIndex && i < optionsTopLineIndex + OPTIONS_NUM_LINES_ON_SCREEN) {
							optionsLines[i].SetAlpha(1f);
						}
						else {
							optionsLines[i].SetAlpha(0f);
						}
					}
					optionsSelectedLineFillTex.SetAlpha(OPTIONS_SELECTION_FILL_ALPHA);

					optionsChevronsLeft.SetAlpha(selectedIsConfirmOrCancel || curLineSelectedOptionIndex == 0 ? 0f : 1f);
					optionsChevronsRight.SetAlpha(selectedIsConfirmOrCancel || curLineSelectedOptionIndex == curLineOptions.Length - 1 ? 0f : 1f);
				}
				else {
					optionsLinesToBeMovedUp -= moveFracThisTick;
					for (int i = 0; i < optionsLines.Count; ++i) {
						optionsLines[i].Shift(moveFracThisTick * OPTIONS_LINE_TOTAL_HEIGHT);
						optionsLines[i].SetAlpha(1f - optionsLines[i].GetNormalizedDistanceFromActiveArea());
					}

					optionsSelectedLineFillTex.SetAlpha((float) MathUtils.Clamp(1f - Math.Abs(optionsLinesToBeMovedUp), 0f, OPTIONS_SELECTION_FILL_ALPHA));
					if (selectedIsConfirmOrCancel) {
						optionsChevronsLeft.SetAlpha(Math.Abs(optionsLinesToBeMovedUp));
						optionsChevronsRight.SetAlpha(Math.Abs(optionsLinesToBeMovedUp));
					}
					else {
						optionsChevronsLeft.SetAlpha(1f - Math.Abs(optionsLinesToBeMovedUp) * (curLineSelectedOptionIndex <= 0 ? -1f : 1f));
						optionsChevronsRight.SetAlpha(1f - Math.Abs(optionsLinesToBeMovedUp) * (curLineSelectedOptionIndex == curLineOptions.Length ? -1f : 1f));	
					}
				}

				if (selectedLine.ValueString != null) {
					optionsChevronsLeft.AnchorOffset = new Vector2(
						selectedLine.ValueString.AnchorOffset.X + selectedLine.ValueString.Dimensions.X + OPTIONS_CHEVRON_MARGIN_X + OPTIONS_CHEVRON_OFFSET_X,
						selectedLine.ValueString.AnchorOffset.Y + OPTIONS_CHEVRON_OFFSET_Y
					);
					optionsChevronsRight.AnchorOffset = new Vector2(
						selectedLine.ValueString.AnchorOffset.X - OPTIONS_CHEVRON_MARGIN_X + OPTIONS_CHEVRON_OFFSET_X,
						selectedLine.ValueString.AnchorOffset.Y + OPTIONS_CHEVRON_OFFSET_Y
					);
				}


			}
			else {
				if (optionsSelectedLineFillTex.Color.W != OPTIONS_SELECTION_FILL_ALPHA) {
					optionsSelectedLineFillTex.AdjustAlpha(deltaTime * OPTIONS_LINE_MOVEMENT_PER_SEC);
					if (optionsSelectedLineFillTex.Color.W > OPTIONS_SELECTION_FILL_ALPHA) optionsSelectedLineFillTex.SetAlpha(OPTIONS_SELECTION_FILL_ALPHA);
				}

				if (selectedIsConfirmOrCancel) {
					optionsChevronsLeft.AdjustAlpha(-deltaTime * OPTIONS_LINE_MOVEMENT_PER_SEC);
					optionsChevronsRight.AdjustAlpha(-deltaTime * OPTIONS_LINE_MOVEMENT_PER_SEC);
				}
				else {
					optionsChevronsLeft.AdjustAlpha(deltaTime * OPTIONS_LINE_MOVEMENT_PER_SEC * (curLineSelectedOptionIndex <= 0 ? -1f : 1f));
					optionsChevronsRight.AdjustAlpha(deltaTime * OPTIONS_LINE_MOVEMENT_PER_SEC * (curLineSelectedOptionIndex == curLineOptions.Length - 1 ? -1f : 1f));
				}

				optionsLRChevronOffset += deltaTime * OPTIONS_CHEVRON_OFFSET_PER_SEC;
				while (optionsLRChevronOffset > OPTIONS_CHEVRON_OFFSET_MAX) optionsLRChevronOffset -= OPTIONS_CHEVRON_OFFSET_MAX;

				if (selectedLine.ValueString != null) {
					optionsChevronsLeft.AnchorOffset = new Vector2(
						selectedLine.ValueString.AnchorOffset.X + selectedLine.ValueString.Dimensions.X + OPTIONS_CHEVRON_MARGIN_X + OPTIONS_CHEVRON_OFFSET_X + optionsLRChevronOffset,
						selectedLine.ValueString.AnchorOffset.Y + OPTIONS_CHEVRON_OFFSET_Y
					);
					optionsChevronsRight.AnchorOffset = new Vector2(
						selectedLine.ValueString.AnchorOffset.X - OPTIONS_CHEVRON_MARGIN_X + OPTIONS_CHEVRON_OFFSET_X - optionsLRChevronOffset,
						selectedLine.ValueString.AnchorOffset.Y + OPTIONS_CHEVRON_OFFSET_Y
					);
				}
			}

			if (selectedIsConfirmOrCancel && optionsSelectedLineFillTex.Scale.X != OPTIONS_CONFIRM_CANCEL_SELECTION_FILL_WIDTH) {
				float diff = OPTIONS_SEPARATOR_SCALE.X - OPTIONS_CONFIRM_CANCEL_SELECTION_FILL_WIDTH;
				float xScale = optionsSelectedLineFillTex.Scale.X - diff * deltaTime * OPTIONS_FILL_RESCALE_MULTIPLIER;
				if (xScale < OPTIONS_CONFIRM_CANCEL_SELECTION_FILL_WIDTH) xScale = OPTIONS_CONFIRM_CANCEL_SELECTION_FILL_WIDTH;
				optionsSelectedLineFillTex.Scale = new Vector2(xScale, optionsSelectedLineFillTex.Scale.Y);
			}
			else if (!selectedIsConfirmOrCancel && optionsSelectedLineFillTex.Scale.X != OPTIONS_SEPARATOR_SCALE.X) {
				float diff = OPTIONS_CONFIRM_CANCEL_SELECTION_FILL_WIDTH - OPTIONS_SEPARATOR_SCALE.X;
				float xScale = optionsSelectedLineFillTex.Scale.X - diff * deltaTime * OPTIONS_FILL_RESCALE_MULTIPLIER;
				if (xScale > OPTIONS_SEPARATOR_SCALE.X) xScale = OPTIONS_SEPARATOR_SCALE.X;
				optionsSelectedLineFillTex.Scale = new Vector2(xScale, optionsSelectedLineFillTex.Scale.Y);
			}

			optionsUDChevronAlpha += OPTIONS_CHEVRON_ALPHA_PER_SEC * deltaTime;
			float actualAlpha = ((int) optionsUDChevronAlpha & 1) == 1 ? optionsUDChevronAlpha % 1f : (1f - optionsUDChevronAlpha % 1f);
			optionsChevronsDown.SetAlpha(optionsTopLineIndex < optionsLines.Count - OPTIONS_NUM_LINES_ON_SCREEN ? actualAlpha : 0f);
			optionsChevronsUp.SetAlpha(optionsTopLineIndex == 0 ? 0f : actualAlpha);
		}

		private static void OptionsMoveDown() {
			if (optionsLinesToBeMovedUp != 0f) return;
			int curSelectedLineIndexAbsolute = optionsTopLineIndex + optionsRelativeSelectedLineIndex;
			bool topLineCanNotMove = optionsTopLineIndex >= optionsLines.Count - OPTIONS_NUM_LINES_ON_SCREEN;
			bool selectionLineCanNotMove = optionsRelativeSelectedLineIndex == OPTIONS_NUM_LINES_ON_SCREEN - 1 || curSelectedLineIndexAbsolute == optionsLines.Count - 1;
			if (topLineCanNotMove && selectionLineCanNotMove) {
				HUDSound.UIUnavailable.Play();
				return;
			}

			int minMovesDown = 1;
			while (optionsLines[curSelectedLineIndexAbsolute + minMovesDown].IsHeader) {
				if (curSelectedLineIndexAbsolute + minMovesDown == optionsLines.Count - 1) {
					HUDSound.UIUnavailable.Play();
					return;
				}
				++minMovesDown;
			}

			if (topLineCanNotMove) {
				OptionsSetMenuPositions(optionsRelativeSelectedLineIndex + minMovesDown, optionsTopLineIndex);
			}
			else if (selectionLineCanNotMove) {
				OptionsSetMenuPositions(optionsRelativeSelectedLineIndex, optionsTopLineIndex + minMovesDown);
			}
			else if (optionsRelativeSelectedLineIndex + minMovesDown > OPTIONS_NUM_LINES_ON_SCREEN - 2) {
				OptionsSetMenuPositions(optionsRelativeSelectedLineIndex, optionsTopLineIndex + minMovesDown);
			}
			else {
				OptionsSetMenuPositions(optionsRelativeSelectedLineIndex + minMovesDown, optionsTopLineIndex);
			}

			HUDSound.UINextOption.Play();
		}

		private static void OptionsMoveUp() {
			if (optionsLinesToBeMovedUp != 0f) return;
			bool topLineCanNotMove = optionsTopLineIndex <= 0;
			bool selectionLineCanNotMove = optionsRelativeSelectedLineIndex == 0;
			if (topLineCanNotMove && selectionLineCanNotMove) {
				HUDSound.UIUnavailable.Play();
				return;
			}

			int minMovesUp = 1;
			int curSelectedLineIndexAbsolute = optionsTopLineIndex + optionsRelativeSelectedLineIndex;
			while (optionsLines[curSelectedLineIndexAbsolute - minMovesUp].IsHeader) {
				if (curSelectedLineIndexAbsolute - minMovesUp == 0) {
					HUDSound.UIUnavailable.Play();
					return;
				}
				++minMovesUp;
			}

			if (topLineCanNotMove) {
				OptionsSetMenuPositions(optionsRelativeSelectedLineIndex - minMovesUp, optionsTopLineIndex);
			}
			else if (selectionLineCanNotMove) {
				OptionsSetMenuPositions(optionsRelativeSelectedLineIndex, optionsTopLineIndex - minMovesUp);
			}
			else if (optionsRelativeSelectedLineIndex - minMovesUp < 2) {
				OptionsSetMenuPositions(optionsRelativeSelectedLineIndex, optionsTopLineIndex - minMovesUp);
			}
			else {
				OptionsSetMenuPositions(optionsRelativeSelectedLineIndex - minMovesUp, optionsTopLineIndex);
			}

			HUDSound.UINextOption.Play();
		}

		private static void OptionsBackOut() {
			HUDSound.UISelectNegativeOption.Play();
			OptionsSetMenuPositions(OPTIONS_NUM_LINES_ON_SCREEN - 2, optionsLines.Count - OPTIONS_NUM_LINES_ON_SCREEN);
		}

		private static void OptionsConfirm() {
			if (optionsAbsoluteSelectedLineIndex < optionsLines.Count - 2) return;

			if (optionsAbsoluteSelectedLineIndex == optionsLines.Count - 2) {
				HUDSound.UISelectOption.Play();
				OptionsSave();
			}
			else HUDSound.UISelectNegativeOption.Play();
			OptionsTransitionOut();
			if (!inDeferenceMode) currentTransitionComplete += () => MainMenuTransitionIn();
		}

		private static void OptionsDecreaseValue() {
			OptionsChangeValue(false);
		}

		private static void OptionsIncreaseValue() {
			OptionsChangeValue(true);
		}

		private static void OptionsChangeValue(bool increase) {
			int targetIndex = curLineSelectedOptionIndex + (increase ? 1 : -1);
			if (targetIndex < 0 || targetIndex >= curLineOptions.Length) {
				HUDSound.UIUnavailable.Play();
				return;
			}

			curLineSelectedOptionIndex = targetIndex;
			var selectedLine = optionsLines[optionsAbsoluteSelectedLineIndex];
			if (selectedLine.ValueString == null) return;
			selectedLine.ValueString.Text = curLineOptions[curLineSelectedOptionIndex];
			OptionsRefreshLRChevronPositions();

			if (increase) HUDSound.PostPassEggPop.Play(pitch: 1.1f);
			else HUDSound.PostPassEggPop.Play(pitch: 0.9f);
		}

		private static void OptionsRefreshLRChevronPositions() {
			var selectedLine = optionsLines[optionsAbsoluteSelectedLineIndex];

			optionsChevronsLeft.AnchorOffset = new Vector2(
					selectedLine.ValueString.AnchorOffset.X + selectedLine.ValueString.Dimensions.X + OPTIONS_CHEVRON_MARGIN_X + OPTIONS_CHEVRON_OFFSET_X,
					selectedLine.ValueString.AnchorOffset.Y + OPTIONS_CHEVRON_OFFSET_Y
				);
			optionsChevronsRight.AnchorOffset = new Vector2(
				selectedLine.ValueString.AnchorOffset.X - OPTIONS_CHEVRON_MARGIN_X + OPTIONS_CHEVRON_OFFSET_X,
				selectedLine.ValueString.AnchorOffset.Y + OPTIONS_CHEVRON_OFFSET_Y
			);
			optionsLRChevronOffset = 0f;
		}

		private static string OptionsGetOptionsForIndex(int index) {
			switch (index) {
				case 1: 
					return "30~60~90~144~None";
				case 2:
					return String.Join(
						"~", 
						RenderingModule.SelectedOutputDisplay.NativeResolutions
							.Distinct((r1, r2) => r1.ResolutionHeight == r2.ResolutionHeight && r1.ResolutionWidth == r2.ResolutionWidth)
							.Select(nor => nor.ResolutionWidth + " x " + nor.ResolutionHeight)
						);
				case 3:
					return "Standard~Borderless~Windowed";
				case 4:
					return "0x~2x~4x~8x~16x";
				case 5:
				case 6:
					return "Off~On";
				case 7:
					return "Very Low~Low~Standard~High~Ultra";
				case 8:
					return "Very Low~Low~Medium~High~Very High~Uncapped";
				case 11:
				case 12:
				case 13:
					return "0%~5%~10%~15%~20%~30%~40%~50%~60%~70%~80%~85%~90%~95%~100%";
				case 16:
				case 17:
					return "Very Small~Small~Moderate~Large~Very Large";
				case 18:
					return "No~Yes";
				case 19:
				case 20:
					return "+0.00m~+0.25m~+0.50m~+0.75m~+1.00m~+1.25m~+1.50m";
				case 23:
				case 24:
				case 25:
				case 26:
				case 27:
				case 28:
				case 29:
				case 30:
					return String.Join("~", Enum.GetValues(typeof(VirtualKey)).Cast<VirtualKey>());
				default:
					return String.Empty;
			}
		}

		private static void OptionsSave() {
			for (int i = 0; i < optionsLines.Count; ++i) {
				string valueAsString = optionsLines[i].ValueString != null ? optionsLines[i].ValueString.Text : null;
				if (valueAsString == null) continue;
				switch (i) {
					#region Framerate Cap
					case 1:
						bool inputRateWasSame = Config.InputStateUpdateRateHz == Config.MaxFrameRateHz;
						bool tickRateWasSame = Config.TickRateHz == Config.MaxFrameRateHz;
						switch (valueAsString) {
							case "30": Config.MaxFrameRateHz = 30L; break;
							case "60": Config.MaxFrameRateHz = 60L; break;
							case "90": Config.MaxFrameRateHz = 90L; break;
							case "144": Config.MaxFrameRateHz = 144L; break;
							default:
								long framerateCap;
								bool isNumber = Int64.TryParse(valueAsString, out framerateCap);
								Config.MaxFrameRateHz = isNumber ? framerateCap : 0L;
								break;

						}
						if (inputRateWasSame) Config.InputStateUpdateRateHz = Config.MaxFrameRateHz;
						if (tickRateWasSame) Config.TickRateHz = Config.MaxFrameRateHz;
						break;
					#endregion
					#region Resolution
					case 2:
						var resStringSplit = valueAsString.Split(new[] { 'x', ' ' }, StringSplitOptions.RemoveEmptyEntries);
						uint width, height;
						if (resStringSplit.Length != 2
							|| !UInt32.TryParse(resStringSplit[0], out width) 
							|| !UInt32.TryParse(resStringSplit[1], out height)
						) {
							width = RenderingModule.SelectedOutputDisplay.HighestResolution.ResolutionWidth;
							height = RenderingModule.SelectedOutputDisplay.HighestResolution.ResolutionHeight;
						}

						Config.DisplayWindowWidth = width;
						Config.DisplayWindowHeight = height;
						break;
					#endregion
					#region Fullscreen Mode
					case 3:
						switch (valueAsString) {
							case "Standard": Config.DisplayWindowFullscreenState = WindowFullscreenState.StandardFullscreen; break;
							case "Borderless": Config.DisplayWindowFullscreenState = WindowFullscreenState.BorderlessFullscreen; break;
							default: Config.DisplayWindowFullscreenState = WindowFullscreenState.NotFullscreen; break;
						}
						break;
					#endregion
					#region Anisotropy
					case 4:
						int anisotropy;
						if (!Int32.TryParse(valueAsString.Split('x')[0], out anisotropy)) anisotropy = 0;
						switch (anisotropy) {
							case 2: Config.DefaultSamplingAnisoLevel = AnisotropicFilteringLevel.TwoTimes; break;
							case 4: Config.DefaultSamplingAnisoLevel = AnisotropicFilteringLevel.FourTimes; break;
							case 8: Config.DefaultSamplingAnisoLevel = AnisotropicFilteringLevel.EightTimes; break;
							case 16: Config.DefaultSamplingAnisoLevel = AnisotropicFilteringLevel.SixteenTimes; break;
							default: Config.DefaultSamplingAnisoLevel = AnisotropicFilteringLevel.None; break;
						}
						break;
					#endregion
					#region DoF
					case 5:
						switch (valueAsString) {
							case "Off":
								Config.DofLensMaxBlurDist = Single.PositiveInfinity;
								Config.DofLensFocalDist = 0f;
								break;
							case "On":
								Config.DofLensMaxBlurDist = PhysicsManager.ONE_METRE_SCALED * 240f;
								Config.DofLensFocalDist = PhysicsManager.ONE_METRE_SCALED * 20f;							
								break;
						}
						break;
					#endregion
					#region Enable Outlining
					case 6:
						switch (valueAsString) {
							case "Off":
								Config.EnableOutlining = false;
								break;
							default:
								Config.EnableOutlining = true;
								break;
						}
						break;
					#endregion
					#region Physics Quality
					case 7:
						switch (valueAsString) {
							case "Very Low": Config.PhysicsLevel = 1U; break;
							case "Low": Config.PhysicsLevel = 2U; break;
							case "Standard": Config.PhysicsLevel = 3U; break;
							case "High": Config.PhysicsLevel = 4U; break;
							case "Ultra": Config.PhysicsLevel = 5U; break;
						}
						break;
					#endregion
					#region Dynamic Light Cap
					case 8:
						switch (valueAsString) {
							case "Very Low": Config.DynamicLightCap = 16; break;
							case "Low": Config.DynamicLightCap = 32; break;
							case "Medium": Config.DynamicLightCap = 48; break;
							case "High": Config.DynamicLightCap = 64; break;
							case "Very High": Config.DynamicLightCap = 96; break;
							case "Uncapped": Config.DynamicLightCap = null; break;
						}
						break;
					#endregion
					#region Sound Effects Vol
					case 11:
						int seVol;
						if (!Int32.TryParse(valueAsString.Split('%')[0], out seVol)) seVol = 100;
						Config.SoundEffectVolume = seVol / 100f;
						break;
					#endregion
					#region HUD Vol
					case 12:
						int hudVol;
						if (!Int32.TryParse(valueAsString.Split('%')[0], out hudVol)) hudVol = 100;
						Config.HUDVolume = hudVol / 100f;
						break;
					#endregion
					#region Music Vol
					case 13:
						int musicVol;
						if (!Int32.TryParse(valueAsString.Split('%')[0], out musicVol)) musicVol = 100;
						Config.MusicVolume = musicVol / 100f;
						break;
					#endregion
					#region Board Tilt Deadzone
					case 16:
						switch (valueAsString) {
							case "Very Small": Config.BoardTiltDeadZone = 32768 / 18; break;
							case "Small": Config.BoardTiltDeadZone = 32768 / 12; break;
							case "Moderate": Config.BoardTiltDeadZone = 32768 / 8; break;
							case "Large": Config.BoardTiltDeadZone = 32768 / 5; break;
							case "Very Large": Config.BoardTiltDeadZone = 32768 / 3; break;
						}
						break;
					#endregion
					#region Cam Orient Deadzone
					case 17:
						switch (valueAsString) {
							case "Very Small": Config.CamOrientDeadZone = 32768 / 12; break;
							case "Small": Config.CamOrientDeadZone = 32768 / 8; break;
							case "Moderate": Config.CamOrientDeadZone = 32768 / 5; break;
							case "Large": Config.CamOrientDeadZone = 32768 / 3; break;
							case "Very Large": Config.CamOrientDeadZone = 32768 / 2; break;
						}
						break;
					#endregion
					#region Cam Height Adjust
					case 19: {
						float valueAsFloat;
						bool validFloat = Single.TryParse(valueAsString.Replace("m", ""), NumberStyles.Any, CultureInfo.InvariantCulture, out valueAsFloat);
						if (!validFloat) valueAsFloat = 0f;
						Config.CameraHeightOffset = PhysicsManager.ONE_METRE_SCALED * valueAsFloat;
						break;	
					}
					#endregion
					#region Cam Distance Adjust
					case 20: {
						float valueAsFloat;
						bool validFloat = Single.TryParse(valueAsString.Replace("m", ""), NumberStyles.Any, CultureInfo.InvariantCulture, out valueAsFloat);
						if (!validFloat) valueAsFloat = 0f;
						Config.CameraDistanceOffset = PhysicsManager.ONE_METRE_SCALED * valueAsFloat;
						break;
					}
					#endregion
					#region Invert Cam
					case 18:
						Config.InvertCamControlY = valueAsString == "Yes";
						break;
					#endregion
					#region Keybindings
					case 23:
					case 24:
					case 25:
					case 26:
					case 27:
					case 28:
					case 29:
					case 30:
						VirtualKey key;
						if (!Enum.TryParse(valueAsString, out key)) key = VirtualKey.None;
						switch (i) {
							case 23: Config.Key_TiltGameboardForward = key; break;
							case 24: Config.Key_TiltGameboardBackward = key; break;
							case 25: Config.Key_TiltGameboardLeft = key; break;
							case 26: Config.Key_TiltGameboardRight = key; break;
							case 27: Config.Key_FlipGameboardLeft = key; break;
							case 28: Config.Key_FlipGameboardRight = key; break;
							case 29: Config.Key_HopEgg = key; break;
							case 30: Config.Key_FastRestart = key; break;
						}
						break;
					#endregion
				}
			}


			Config.Save();
			Config.ForceRefresh();
		}
	}
}