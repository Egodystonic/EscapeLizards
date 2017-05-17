// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 07 07 2015 at 18:00 by Ben Bowen

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Ophidian.Losgap;
using Ophidian.Losgap.Entities;
using Ophidian.Losgap.Input;
using Ophidian.Losgap.Rendering;

namespace Egodystonic.EscapeLizards {
	/// <summary>
	/// A static class containing configuration options for the game. Some of these should be user-editable at some point
	/// (though some should perhaps only be INI editable).
	/// </summary>
	public static partial class Config {
		private static readonly object staticMutationLock = new object();

		private static event Action configRefresh;
		public static event Action ConfigRefresh {
			add {
				lock (staticMutationLock) {
					configRefresh += value;
				}
			}
			remove {
				lock (staticMutationLock) {
					configRefresh -= value;
				}
			}
		}

		public static void Save() {
			lock (staticMutationLock) {
				StringBuilder iniFileBuilder = new StringBuilder(2000);
				iniFileBuilder.AppendLine("[Engine]");
				iniFileBuilder.Append("MaxThreadCount=");
				iniFileBuilder.AppendLine(maxThreadCount.ToString());
				iniFileBuilder.Append("SlaveThreadPriority=");
				iniFileBuilder.AppendLine(slaveThreadPriority.ToString());
				iniFileBuilder.Append("ThreadOversubscriptionFactor=");
				iniFileBuilder.AppendLine(threadOversubscriptionFactor.ToString());
				iniFileBuilder.Append("VSync=");
				iniFileBuilder.AppendLine(vsyncEnabled.ToString());
				iniFileBuilder.Append("MaxFrameRateHz=");
				iniFileBuilder.AppendLine(maxFrameRateHz.ToString());
				iniFileBuilder.Append("InputStateUpdateRateHz=");
				iniFileBuilder.AppendLine(maxFrameRateHz.ToString());
				iniFileBuilder.Append("TickRateHz=");
				iniFileBuilder.AppendLine(tickRateHz.ToString());

				iniFileBuilder.AppendLine();
				iniFileBuilder.AppendLine("[Renderer]");
				iniFileBuilder.Append("GPUSelectionOverride=");
				iniFileBuilder.AppendLine(gpuSelectionOverride == null ? "null" : gpuSelectionOverride.ToString());
				iniFileBuilder.Append("OutputGPUSelectionOverride=");
				iniFileBuilder.AppendLine(outputGPUSelectionOverride == null ? "null" : outputGPUSelectionOverride.ToString());
				iniFileBuilder.Append("OutputSelectionOverride=");
				iniFileBuilder.AppendLine(outputSelectionOverride == null ? "null" : outputSelectionOverride.ToString());
				iniFileBuilder.Append("DisplayWindowWidth=");
				iniFileBuilder.AppendLine(displayWindowWidth.ToString());
				iniFileBuilder.Append("DisplayWindowHeight=");
				iniFileBuilder.AppendLine(displayWindowHeight.ToString());
				iniFileBuilder.Append("DisplayWindowFullscreenState=");
				iniFileBuilder.AppendLine(displayWindowFullscreenState.ToString());
				iniFileBuilder.Append("DisplayWindowMultibufferLevel=");
				iniFileBuilder.AppendLine(displayWindowMultibufferLevel.ToString());
				iniFileBuilder.Append("DisplayWindowNearPlane=");
				iniFileBuilder.AppendLine(displayWindowNearPlane.ToString(3));
				iniFileBuilder.Append("DisplayWindowFarPlane=");
				iniFileBuilder.AppendLine(displayWindowFarPlane.ToString(3));
				iniFileBuilder.Append("DefaultSampling=");
				iniFileBuilder.AppendLine(defaultSamplingFilterType.ToString());
				iniFileBuilder.Append("DefaultSamplingAnisoLevel=");
				iniFileBuilder.AppendLine(defaultSamplingAnisoLevel.ToString());
				iniFileBuilder.Append("MainFontLineHeight=");
				iniFileBuilder.AppendLine(mainFontLineHeight.HasValue ? mainFontLineHeight.Value.ToString() : "null");
				iniFileBuilder.Append("MainFontKerning=");
				iniFileBuilder.AppendLine(mainFontKerning.HasValue ? mainFontKerning.Value.ToString() : "null");
				iniFileBuilder.Append("TitleFontLineHeight=");
				iniFileBuilder.AppendLine(titleFontLineHeight.HasValue ? titleFontLineHeight.Value.ToString() : "null");
				iniFileBuilder.Append("TitleFontKerning=");
				iniFileBuilder.AppendLine(titleFontKerning.HasValue ? titleFontKerning.Value.ToString() : "null");
				iniFileBuilder.Append("DoFLensFocalDist=");
				iniFileBuilder.AppendLine(dofLensFocalDist.ToString(3));
				iniFileBuilder.Append("DoFLensMaxBlurDist=");
				iniFileBuilder.AppendLine(dofLensMaxBlurDist.ToString(3));
				iniFileBuilder.Append("DynamicLightCap=");
				iniFileBuilder.AppendLine(dynamicLightCap.HasValue ? dynamicLightCap.Value.ToString() : "null");
				iniFileBuilder.Append("EnableOutlining=");
				iniFileBuilder.AppendLine(enableOutlining ? "True" : "False");

				iniFileBuilder.AppendLine();
				iniFileBuilder.AppendLine("[Game]");
				iniFileBuilder.Append("LevelIntroTime=");
				iniFileBuilder.AppendLine(levelIntroductionTime.ToString(2));
				iniFileBuilder.Append("LevelIntroAccelMod=");
				iniFileBuilder.AppendLine(levelIntroAccelerationModifier.ToString(3));
				iniFileBuilder.Append("LevelIntroCamDist=");
				iniFileBuilder.AppendLine(levelIntroExtraCameraDistance.ToString(3));
				iniFileBuilder.Append("LevelIntroCamHeight=");
				iniFileBuilder.AppendLine(levelIntroExtraCameraHeight.ToString(3));
				iniFileBuilder.Append("LevelIntroCamPanRads=");
				iniFileBuilder.AppendLine(levelIntroCameraPanAmountRadians.ToString(4));
				iniFileBuilder.Append("LevelIntroPreSpawnCamJerkModifier=");
				iniFileBuilder.AppendLine(levelIntroPreSpawnModifier.ToString(3));
				iniFileBuilder.Append("EggMaterial=");
				iniFileBuilder.AppendLine(eggMaterial.ToString());
				iniFileBuilder.Append("PhysicsLevel=");
				iniFileBuilder.AppendLine(physicsLevel.ToString());
				iniFileBuilder.Append("SillyBitsIlluminationRadius=");
				iniFileBuilder.AppendLine(sillyBitsIlluminationRadius.ToString(3));
				for (int i = 0; i < sillyBitsIlluminationColors.Length; ++i) {
					iniFileBuilder.Append("SillyBitsIlluminationColors");
					iniFileBuilder.Append(i);
					iniFileBuilder.Append("=");
					iniFileBuilder.AppendLine(sillyBitsIlluminationColors[i].ToString(3));
				}
				iniFileBuilder.Append("PassCamMaxDist=");
				iniFileBuilder.AppendLine(passCamMaxDist.ToString(3));
				iniFileBuilder.Append("PassCamZoomOutSpeed=");
				iniFileBuilder.AppendLine(passCamZoomOutSpeed.ToString(3));
				iniFileBuilder.Append("PassCamSpin=");
				iniFileBuilder.AppendLine(passCamSpin.ToString(4));
				iniFileBuilder.Append("PassActorPreFadeLife=");
				iniFileBuilder.AppendLine(passActorPreFadeLife.ToString(2));
				iniFileBuilder.Append("PassActorFadeTime=");
				iniFileBuilder.AppendLine(passActorFadeTime.ToString(2));
				iniFileBuilder.Append("FinishLightRadius=");
				iniFileBuilder.AppendLine(finishLightRadius.ToString(2));
				iniFileBuilder.Append("FinishLightColorGold=");
				iniFileBuilder.AppendLine(finishLightColorGold.ToString(3));
				iniFileBuilder.Append("FinishLightColorSilver=");
				iniFileBuilder.AppendLine(finishLightColorSilver.ToString(3));
				iniFileBuilder.Append("FinishLightColorBronze=");
				iniFileBuilder.AppendLine(finishLightColorBronze.ToString(3));
				iniFileBuilder.Append("FinishLightColorNoStar=");
				iniFileBuilder.AppendLine(finishLightColorNoStar.ToString(3));
				iniFileBuilder.Append("FinishLightRingNumLightsGold=");
				iniFileBuilder.AppendLine(finishLightRingNumLightsGold.ToString());
				iniFileBuilder.Append("FinishLightRingNumLightsSilver=");
				iniFileBuilder.AppendLine(finishLightRingNumLightsSilver.ToString());
				iniFileBuilder.Append("FinishLightRingNumLightsBronze=");
				iniFileBuilder.AppendLine(finishLightRingNumLightsBronze.ToString());
				iniFileBuilder.Append("FinishLightRingFallingStarLifetime=");
				iniFileBuilder.AppendLine(finishLightRingFallingStarLifetime.ToString());
				iniFileBuilder.Append("CameraHeightOffset=");
				iniFileBuilder.AppendLine(cameraHeightOffset.ToString(3));
				iniFileBuilder.Append("CameraDistanceOffset=");
				iniFileBuilder.AppendLine(cameraDistanceOffset.ToString(3));

				iniFileBuilder.AppendLine();
				iniFileBuilder.AppendLine("[Audio]");
				iniFileBuilder.Append("TimeWarningMs=");
				iniFileBuilder.AppendLine(timeWarningMs.ToString());
				iniFileBuilder.Append("TimePitchRaiseMs=");
				iniFileBuilder.AppendLine(timePitchRaiseMs.ToString());
				iniFileBuilder.Append("SoundEffectVol=");
				iniFileBuilder.AppendLine(soundEffectVolume.ToString(2));
				iniFileBuilder.Append("HUDVol=");
				iniFileBuilder.AppendLine(hudVolume.ToString(2));
				iniFileBuilder.Append("MusicVol=");
				iniFileBuilder.AppendLine(musicVolume.ToString(2));

				iniFileBuilder.AppendLine();
				iniFileBuilder.AppendLine("[Input]");
				iniFileBuilder.Append("KeyTiltGameboardForward=");
				iniFileBuilder.AppendLine(tiltGameboardForward.ToString());
				iniFileBuilder.Append("KeyTiltGameboardBackward=");
				iniFileBuilder.AppendLine(tiltGameboardBackward.ToString());
				iniFileBuilder.Append("KeyTiltGameboardLeft=");
				iniFileBuilder.AppendLine(tiltGameboardLeft.ToString());
				iniFileBuilder.Append("KeyTiltGameboardRight=");
				iniFileBuilder.AppendLine(tiltGameboardRight.ToString());
				iniFileBuilder.Append("KeyFlipGameboardLeft=");
				iniFileBuilder.AppendLine(flipGameboardLeft.ToString());
				iniFileBuilder.Append("KeyFlipGameboardRight=");
				iniFileBuilder.AppendLine(flipGameboardRight.ToString());
				iniFileBuilder.Append("KeyHopEgg=");
				iniFileBuilder.AppendLine(hopEgg.ToString());
				iniFileBuilder.Append("OptionSelectThreshold=");
				iniFileBuilder.AppendLine(optionSelectThreshold.ToString());
				iniFileBuilder.Append("InvertCamControlY=");
				iniFileBuilder.AppendLine(invertCamControlY.ToString());
				iniFileBuilder.Append("CamOrientDeadZone=");
				iniFileBuilder.AppendLine(camOrientDeadZone.ToString());
				iniFileBuilder.Append("BoardTiltDeadZone=");
				iniFileBuilder.AppendLine(boardTiltDeadZone.ToString());
				iniFileBuilder.Append("InitialRepeatTimeDelay=");
				iniFileBuilder.AppendLine(initialRepeatTimeDelay.ToString(3));
				iniFileBuilder.Append("SuccessiveRepeatTimeDelay=");
				iniFileBuilder.AppendLine(successiveRepeatTimeDelay.ToString(3));
				
				string filepath = Path.Combine(LosgapSystem.MutableDataDirectory.FullName, "ELSettings.ini");
				byte[] outputBytes = Encoding.Default.GetBytes(iniFileBuilder.ToString());
				using (var filestream = File.Open(filepath, FileMode.Create, FileAccess.Write, FileShare.Read)) {
					filestream.Write(outputBytes, 0, outputBytes.Length);
					filestream.Flush();
				}
			}
		}

		public static void Load() {
			lock (staticMutationLock) {
				string filepath = Path.Combine(LosgapSystem.MutableDataDirectory.FullName, "ELSettings.ini");
				if (!File.Exists(filepath)) return;

				byte[] bytes;
				using (var filestream = File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
					int numBytes = (int) filestream.Length; // If the file is longer than Int32.MaxValue bytes, then it's probably fucked anyway
					bytes = new byte[numBytes];
					filestream.Read(bytes, 0, numBytes);
				}

				string fileAsSingleString = Encoding.Default.GetString(bytes);
				foreach (var line in fileAsSingleString.Split(Environment.NewLine)) {
					if (!line.Contains('=')) continue;
					var split = line.SplitToN('=', 0);
					if (split.Length != 2) {
						Logger.Warn("No value found for INI key '" + split[0] + "'.");
						continue;
					}
					var key = split[0];
					var value = split[1];
					try {
						switch (key) {
							case "MaxThreadCount": maxThreadCount = UInt32.Parse(value); break;
							case "SlaveThreadPriority": slaveThreadPriority = (ThreadPriority) Enum.Parse(typeof(ThreadPriority), value); break;
							case "ThreadOversubscriptionFactor": threadOversubscriptionFactor = Int32.Parse(value); break;
							case "VSync": vsyncEnabled = Boolean.Parse(value); break;
							case "MaxFrameRateHz": maxFrameRateHz = Int64.Parse(value); break;
							case "InputStateUpdateRateHz": inputStateUpdateRateHz = Int64.Parse(value); break;
							case "TickRateHz": tickRateHz = Int64.Parse(value); break;

							case "GPUSelectionOverride": gpuSelectionOverride = value == "null" ? (uint?) null : UInt32.Parse(value); break;
							case "OutputGPUSelectionOverride": outputGPUSelectionOverride = value == "null" ? (uint?) null : UInt32.Parse(value); break;
							case "OutputSelectionOverride": outputSelectionOverride = value == "null" ? (uint?) null : UInt32.Parse(value); break;
							case "DisplayWindowWidth": displayWindowWidth = UInt32.Parse(value); break;
							case "DisplayWindowHeight": displayWindowHeight = UInt32.Parse(value); break;
							case "DisplayWindowFullscreenState": displayWindowFullscreenState = (WindowFullscreenState) Enum.Parse(typeof(WindowFullscreenState), value); break;
							case "DisplayWindowMultibufferLevel": displayWindowMultibufferLevel = (MultibufferLevel) Enum.Parse(typeof(MultibufferLevel), value); break;
							case "DisplayWindowNearPlane": displayWindowNearPlane = Single.Parse(value, CultureInfo.InvariantCulture); break;
							case "DisplayWindowFarPlane": displayWindowFarPlane = Single.Parse(value, CultureInfo.InvariantCulture); break;
							case "DefaultSampling": defaultSamplingFilterType = (TextureFilterType) Enum.Parse(typeof(TextureFilterType), value); break;
							case "DefaultSamplingAnisoLevel": defaultSamplingAnisoLevel = (AnisotropicFilteringLevel) Enum.Parse(typeof(AnisotropicFilteringLevel), value); break;
							case "MainFontLineHeight": mainFontLineHeight = value == "null" ? (uint?) null : UInt32.Parse(value); break;
							case "MainFontKerning": mainFontKerning = value == "null" ? (int?) null : Int32.Parse(value); break;
							case "TitleFontLineHeight": titleFontLineHeight = value == "null" ? (uint?) null : UInt32.Parse(value); break;
							case "TitleFontKerning": titleFontKerning = value == "null" ? (int?) null : Int32.Parse(value); break;
							case "DoFLensFocalDist": dofLensFocalDist = Single.Parse(value, CultureInfo.InvariantCulture); break;
							case "DoFLensMaxBlurDist": dofLensMaxBlurDist = Single.Parse(value, CultureInfo.InvariantCulture); break;
							case "DynamicLightCap": dynamicLightCap = value == "null" ? (int?) null : Int32.Parse(value); break;
							case "EnableOutlining": enableOutlining = Boolean.Parse(value); break;

							case "LevelIntroTime": levelIntroductionTime = Single.Parse(value, CultureInfo.InvariantCulture); break;
							case "LevelIntroAccelMod": levelIntroAccelerationModifier = Single.Parse(value, CultureInfo.InvariantCulture); break;
							case "LevelIntroCamDist": levelIntroExtraCameraDistance = Single.Parse(value, CultureInfo.InvariantCulture); break;
							case "LevelIntroCamHeight": levelIntroExtraCameraHeight = Single.Parse(value, CultureInfo.InvariantCulture); break;
							case "LevelIntroCamPanRads": levelIntroCameraPanAmountRadians = Single.Parse(value, CultureInfo.InvariantCulture); break;
							case "LevelIntroPreSpawnCamJerkModifier": levelIntroPreSpawnModifier = Single.Parse(value, CultureInfo.InvariantCulture); break;
							case "EggMaterial": eggMaterial = (LizardEggMaterial) Enum.Parse(typeof(LizardEggMaterial), value); break;
							case "PhysicsLevel": physicsLevel = UInt32.Parse(value); break;
							case "SillyBitsIlluminationRadius": sillyBitsIlluminationRadius = Single.Parse(value, CultureInfo.InvariantCulture); break;
							case "PassCamMaxDist": passCamMaxDist = Single.Parse(value, CultureInfo.InvariantCulture); break;
							case "PassCamZoomOutSpeed": passCamZoomOutSpeed = Single.Parse(value, CultureInfo.InvariantCulture); break;
							case "PassCamSpin": passCamSpin = Single.Parse(value, CultureInfo.InvariantCulture); break;
							case "PassActorPreFadeLife": passActorPreFadeLife = Single.Parse(value, CultureInfo.InvariantCulture); break;
							case "PassActorFadeTime": passActorFadeTime = Single.Parse(value, CultureInfo.InvariantCulture); break;
							case "FinishLightRadius": finishLightRadius = Single.Parse(value, CultureInfo.InvariantCulture); break;
							case "FinishLightColorGold": finishLightColorGold = Vector3.Parse(value); break;
							case "FinishLightColorSilver": finishLightColorSilver = Vector3.Parse(value); break;
							case "FinishLightColorBronze": finishLightColorBronze = Vector3.Parse(value); break;
							case "FinishLightColorNoStar": finishLightColorNoStar = Vector3.Parse(value); break;
							case "FinishLightRingNumLightsGold": finishLightRingNumLightsGold = Int32.Parse(value); break;
							case "FinishLightRingNumLightsSilver": finishLightRingNumLightsSilver = Int32.Parse(value); break;
							case "FinishLightRingNumLightsBronze": finishLightRingNumLightsBronze = Int32.Parse(value); break;
							case "FinishLightRingFallingStarLifetime": finishLightRingFallingStarLifetime = Single.Parse(value, CultureInfo.InvariantCulture); break;
							case "CameraHeightOffset": cameraHeightOffset = Single.Parse(value, CultureInfo.InvariantCulture); break;
							case "CameraDistanceOffset": cameraDistanceOffset = Single.Parse(value, CultureInfo.InvariantCulture); break;

							case "TimeWarningMs": timeWarningMs = Int32.Parse(value); break;
							case "TimePitchRaiseMs": timePitchRaiseMs = Int32.Parse(value); break;
							case "SoundEffectVol": soundEffectVolume = Single.Parse(value, CultureInfo.InvariantCulture); break;
							case "HUDVol": hudVolume = Single.Parse(value, CultureInfo.InvariantCulture); break;
							case "MusicVol": musicVolume = Single.Parse(value, CultureInfo.InvariantCulture); break;

							case "KeyTiltGameboardForward": tiltGameboardForward = (VirtualKey) Enum.Parse(typeof(VirtualKey), value); break;
							case "KeyTiltGameboardBackward": tiltGameboardBackward = (VirtualKey) Enum.Parse(typeof(VirtualKey), value); break;
							case "KeyTiltGameboardLeft": tiltGameboardLeft = (VirtualKey) Enum.Parse(typeof(VirtualKey), value); break;
							case "KeyTiltGameboardRight": tiltGameboardRight = (VirtualKey) Enum.Parse(typeof(VirtualKey), value); break;
							case "KeyFlipGameboardLeft": flipGameboardLeft = (VirtualKey) Enum.Parse(typeof(VirtualKey), value); break;
							case "KeyFlipGameboardRight": flipGameboardRight = (VirtualKey) Enum.Parse(typeof(VirtualKey), value); break;
							case "KeyHopEgg": hopEgg = (VirtualKey) Enum.Parse(typeof(VirtualKey), value); break;
							case "OptionSelectThreshold": optionSelectThreshold = Int16.Parse(value); break;
							case "InvertCamControlY": invertCamControlY = Boolean.Parse(value); break;
							case "CamOrientDeadZone": camOrientDeadZone = Int16.Parse(value); break;
							case "BoardTiltDeadZone": boardTiltDeadZone = Int16.Parse(value); break;
							case "InitialRepeatTimeDelay": initialRepeatTimeDelay = Single.Parse(value, CultureInfo.InvariantCulture); break;
							case "SuccessiveRepeatTimeDelay": successiveRepeatTimeDelay = Single.Parse(value, CultureInfo.InvariantCulture); break;
							default:
								if (key.StartsWith("SillyBitsIlluminationColors", false, CultureInfo.InvariantCulture)) {
									int index = Int32.Parse(key.SubstringFromEnd(0, 1));
									if (index < sillyBitsIlluminationColors.Length) {
										sillyBitsIlluminationColors[index] = Vector3.Parse(value);
										break;
									}
								}
								Logger.Warn("Unknown INI key/value pair '" + key + "'/'" + value + "'."); break;
						}
					}
					catch (FormatException e) {
						Logger.Warn("Could not parse value '" + value + "' for INI key '" + key + "': Reverting to default setting.", e);
					}
					catch (OverflowException e) {
						Logger.Warn("Could not parse value '" + value + "' for INI key '" + key + "': Reverting to default setting.", e);
					}
				}
			}
		}

		public static void ForceRefresh() {
			Action configRefreshLocal;
			lock (staticMutationLock) {
				configRefreshLocal = configRefresh;
			}
			if (configRefreshLocal != null) configRefreshLocal();
		}

		#region Engine Setup
		private static uint maxThreadCount = 1U;
		private static ThreadPriority slaveThreadPriority = ThreadPriority.Normal;
		private static int threadOversubscriptionFactor = 0;
		private static bool vsyncEnabled = false;
		private static long maxFrameRateHz = 90L;
		private static long inputStateUpdateRateHz = 90L;
		private static long tickRateHz = 90L;

		public static uint MaxThreadCount {
			get {
				lock (staticMutationLock) {
					return maxThreadCount;
				}
			}
			set {
				lock (staticMutationLock) {
					maxThreadCount = value;
				}
			}
		}

		public static ThreadPriority SlaveThreadPriority {
			get {
				lock (staticMutationLock) {
					return slaveThreadPriority;
				}
			}
			set {
				lock (staticMutationLock) {
					slaveThreadPriority = value;
				}
			}
		}

		public static int ThreadOversubscriptionFactor {
			get {
				lock (staticMutationLock) {
					return threadOversubscriptionFactor;
				}
			}
			set {
				lock (staticMutationLock) {
					threadOversubscriptionFactor = value;
				}
			}
		}

		public static bool VSyncEnabled {
			get {
				lock (staticMutationLock) {
					return vsyncEnabled;
				}
			}
			set {
				lock (staticMutationLock) {
					vsyncEnabled = value;
				}
			}
		}

		public static long MaxFrameRateHz {
			get {
				lock (staticMutationLock) {
					return maxFrameRateHz;
				}
			}
			set {
				lock (staticMutationLock) {
					maxFrameRateHz = value;
				}
			}
		}

		public static long InputStateUpdateRateHz {
			get {
				lock (staticMutationLock) {
					return inputStateUpdateRateHz;
				}
			}
			set {
				lock (staticMutationLock) {
					inputStateUpdateRateHz = value;
				}
			}
		}

		public static long TickRateHz {
			get {
				lock (staticMutationLock) {
					return tickRateHz;
				}
			}
			set {
				lock (staticMutationLock) {
					tickRateHz = value;
				}
			}
		}
		#endregion

		#region Rendering
		private static uint? gpuSelectionOverride = null;
		private static uint? outputGPUSelectionOverride = null;
		private static uint? outputSelectionOverride = null;
		private static uint displayWindowWidth = 0U;
		private static uint displayWindowHeight = 0U;
		private static WindowFullscreenState displayWindowFullscreenState = WindowFullscreenState.StandardFullscreen;
		private static MultibufferLevel displayWindowMultibufferLevel = MultibufferLevel.Single;
		private static float displayWindowNearPlane = PhysicsManager.ONE_METRE_SCALED * 0.1f;
		private static float displayWindowFarPlane = PhysicsManager.ONE_METRE_SCALED * 750f;
		private static TextureFilterType defaultSamplingFilterType = TextureFilterType.Anisotropic;
		private static AnisotropicFilteringLevel defaultSamplingAnisoLevel = AnisotropicFilteringLevel.FourTimes;
		private static uint? mainFontLineHeight = 0;
		private static int? mainFontKerning = 0;
		private static uint? titleFontLineHeight = 0;
		private static int? titleFontKerning = 0;
		private static float dofLensFocalDist = PhysicsManager.ONE_METRE_SCALED * 20f;
		private static float dofLensMaxBlurDist = PhysicsManager.ONE_METRE_SCALED * 240f;
		private static int? dynamicLightCap = null;
		private static bool enableOutlining = true;

		public static uint? GPUSelectionOverride {
			get {
				return gpuSelectionOverride;
			}
			set {
				gpuSelectionOverride = value;
			}
		}

		public static uint? OutputGPUSelectionOverride {
			get {
				return outputGPUSelectionOverride;
			}
			set {
				outputGPUSelectionOverride = value;
			}
		}

		public static uint? OutputSelectionOverride {
			get {
				return outputSelectionOverride;
			}
			set {
				outputSelectionOverride = value;
			}
		}

		public static uint DisplayWindowWidth {
			get {
				lock (staticMutationLock) {
					return displayWindowWidth;
				}
			}
			set {
				lock (staticMutationLock) {
					displayWindowWidth = value;
				}
			}
		}

		public static uint DisplayWindowHeight {
			get {
				lock (staticMutationLock) {
					return displayWindowHeight;
				}
			}
			set {
				lock (staticMutationLock) {
					displayWindowHeight = value;
				}
			}
		}

		public static WindowFullscreenState DisplayWindowFullscreenState {
			get {
				lock (staticMutationLock) {
					return displayWindowFullscreenState;
				}
			}
			set {
				lock (staticMutationLock) {
					displayWindowFullscreenState = value;
				}
			}
		}

		public static MultibufferLevel DisplayWindowMultibufferLevel {
			get {
				lock (staticMutationLock) {
					return displayWindowMultibufferLevel;
				}
			}
			set {
				lock (staticMutationLock) {
					displayWindowMultibufferLevel = value;
				}
			}
		}

		public static float DisplayWindowNearPlane {
			get {
				lock (staticMutationLock) {
					return displayWindowNearPlane;
				}
			}
			set {
				lock (staticMutationLock) {
					displayWindowNearPlane = value;
				}
			}
		}

		public static float DisplayWindowFarPlane {
			get {
				lock (staticMutationLock) {
					return displayWindowFarPlane;
				}
			}
			set {
				lock (staticMutationLock) {
					displayWindowFarPlane = value;
				}
			}
		}

		public static TextureFilterType DefaultSamplingFilterType {
			get {
				lock (staticMutationLock) {
					return defaultSamplingFilterType;
				}
			}
			set {
				lock (staticMutationLock) {
					defaultSamplingFilterType = value;
				}
			}
		}

		public static AnisotropicFilteringLevel DefaultSamplingAnisoLevel {
			get {
				lock (staticMutationLock) {
					return defaultSamplingAnisoLevel;
				}
			}
			set {
				lock (staticMutationLock) {
					defaultSamplingAnisoLevel = value;
				}
			}
		}

		public static uint? MainFontLineHeight {
			get {
				lock (staticMutationLock) {
					return mainFontLineHeight;
				}
			}
			set {
				lock (staticMutationLock) {
					mainFontLineHeight = value;
				}
			}
		}

		public static uint? TitleFontLineHeight {
			get {
				lock (staticMutationLock) {
					return titleFontLineHeight;
				}
			}
			set {
				lock (staticMutationLock) {
					titleFontLineHeight = value;
				}
			}
		}

		public static int? MainFontKerning {
			get {
				lock (staticMutationLock) {
					return mainFontKerning;
				}
			}
			set {
				lock (staticMutationLock) {
					mainFontKerning = value;
				}
			}
		}

		public static int? TitleFontKerning {
			get {
				lock (staticMutationLock) {
					return titleFontKerning;
				}
			}
			set {
				lock (staticMutationLock) {
					titleFontKerning = value;
				}
			}
		}

		public static float DofLensFocalDist {
			get {
				lock (staticMutationLock) {
					return dofLensFocalDist;
				}
			}
			set {
				lock (staticMutationLock) {
					dofLensFocalDist = value;
				}
			}
		}

		public static float DofLensMaxBlurDist {
			get {
				lock (staticMutationLock) {
					return dofLensMaxBlurDist;
				}
			}
			set {
				lock (staticMutationLock) {
					dofLensMaxBlurDist = value;
				}
			}
		}

		public static int? DynamicLightCap {
			get {
				lock (staticMutationLock) {
					return dynamicLightCap;
				}
			}
			set {
				lock (staticMutationLock) {
					dynamicLightCap = value;
				}
			}
		}

		public static bool EnableOutlining {
			get {
				lock (staticMutationLock) {
					return enableOutlining;
				}
			}
			set {
				lock (staticMutationLock) {
					enableOutlining = value;
				}
			}
		}
		#endregion

		#region Level Intro
		private static float levelIntroductionTime = 6.6f;
		private static float levelIntroAccelerationModifier = 3.0f;
		private static float levelIntroExtraCameraDistance = PhysicsManager.ONE_METRE_SCALED * 20f;
		private static float levelIntroExtraCameraHeight = PhysicsManager.ONE_METRE_SCALED * 7f;
		private static float levelIntroCameraPanAmountRadians = MathUtils.TWO_PI;
		private static float levelIntroPreSpawnModifier = 0.7f; // Between 0f and 1f, basically just adjusts the intro camera 'jerkiness' as the egg is spawning

		public static float LevelIntroductionTime {
			get {
				lock (staticMutationLock) {
					return levelIntroductionTime;
				}
			}
			set {
				lock (staticMutationLock) {
					levelIntroductionTime = value;
				}
			}
		}

		public static float LevelIntroAccelerationModifier {
			get {
				lock (staticMutationLock) {
					return levelIntroAccelerationModifier;
				}
			}
			set {
				lock (staticMutationLock) {
					levelIntroAccelerationModifier = value;
				}
			}
		}

		public static float LevelIntroExtraCameraDistance {
			get {
				lock (staticMutationLock) {
					return levelIntroExtraCameraDistance;
				}
			}
			set {
				lock (staticMutationLock) {
					levelIntroExtraCameraDistance = value;
				}
			}
		}

		public static float LevelIntroExtraCameraHeight {
			get {
				lock (staticMutationLock) {
					return levelIntroExtraCameraHeight;
				}
			}
			set {
				lock (staticMutationLock) {
					levelIntroExtraCameraHeight = value;
				}
			}
		}

		public static float LevelIntroCameraPanAmountRadians {
			get {
				lock (staticMutationLock) {
					return levelIntroCameraPanAmountRadians;
				}
			}
			set {
				lock (staticMutationLock) {
					levelIntroCameraPanAmountRadians = value;
				}
			}
		}

		public static float LevelIntroPreSpawnModifier {
			get {
				lock (staticMutationLock) {
					return levelIntroPreSpawnModifier;
				}
			}
			set {
				lock (staticMutationLock) {
					levelIntroPreSpawnModifier = value;
				}
			}
		}
		#endregion

		#region Gameplay
		private static LizardEggMaterial eggMaterial = LizardEggMaterial.LizardEgg;
		private static uint physicsLevel = 4U;
		private static float sillyBitsIlluminationRadius = PhysicsManager.ONE_METRE_SCALED * 0.2f;
		private static Vector3[] sillyBitsIlluminationColors = {
			10f * new Vector3(135f/255f, 79f/255f, 2f/255f), // Ball
			10f * new Vector3(66f/255f, 166f/255f, 32f/255f), // Cactus
			10f * new Vector3(14f/255f, 109f/255f, 187f/255f), // Fish
			10f * new Vector3(170f/255f, 147f/255f, 0f/255f), // Flower
			10f * new Vector3(68f/255f, 68f/255f, 68f/255f), // Hammer
			10f * new Vector3(202f/255f, 0f/255f, 0f/255f), // Mushroom
			10f * new Vector3(255f/255f, 255f/255f, 255f/255f), // MusicNote
			10f * new Vector3(68f/255f, 68f/255f, 68f/255f), // Spring
			10f * new Vector3(123f/255f, 237f/255f, 57f/255f), // Tail
		};
		private static float passCamMaxDist = PhysicsManager.ONE_METRE_SCALED * 10f;
		private static float passCamZoomOutSpeed = passCamMaxDist * 0.1f;
		private static float passCamSpin = MathUtils.PI_OVER_TWO * 0.25f;
		private static float passActorPreFadeLife = 7f;
		private static float passActorFadeTime = 4f;
		private static float finishLightRadius = PhysicsManager.ONE_METRE_SCALED * 10f;
		private static Vector3 finishLightColorGold = new Vector3(3f, 3f, 1.9f)			  * 1f;
		private static Vector3 finishLightColorSilver = new Vector3(1.6f, 1.6f, 2.5f)	  * 1f;
		private static Vector3 finishLightColorBronze = new Vector3(1.5f, 0.75f, 0.25f)	  * 0.66f;
		private static Vector3 finishLightColorNoStar = new Vector3(0.3f, 0.5f, 0.3f)     * 1f;
		private static int finishLightRingNumLightsGold = 15;
		private static int finishLightRingNumLightsSilver = 8;
		private static int finishLightRingNumLightsBronze = 3;
		private static float finishLightRingRadius = PhysicsManager.ONE_METRE_SCALED * 1f;
		private static float finishLightRingLightRadius = PhysicsManager.ONE_METRE_SCALED * 0.4f;
		private static float finishLightRingStarSpawnRateGold = 1f / 15f;
		private static float finishLightRingStarSpawnRateSilver = 1f / 8f;
		private static float finishLightRingStarSpawnRateBronze = 1f / 3f;
		private static float finishLightRingFallingStarLifetime = 4f;
		private static int timeWarningMs = 10000;
		private static int timePitchRaiseMs = 30000;
		private static float cameraHeightOffset = 1.25f * PhysicsManager.ONE_METRE_SCALED;
		private static float cameraDistanceOffset = 0.50f * PhysicsManager.ONE_METRE_SCALED;

		public static float GetTickrateForPhysicsLevel(uint physLevel) {
			switch (physLevel) {
				case 2U: return 140f; // Low
				case 3U: return 180f; // Medium
				case 4U: return 180f; // High
				case 5U: return 180f; // Ultra
				default: return 120f; // Very Low
			}
		}

		public static LizardEggMaterial EggMaterial {
			get {
				lock (staticMutationLock) {
					return eggMaterial;
				}
			}
			set {
				lock (staticMutationLock) {
					eggMaterial = value;
				}
			}
		}

		public static uint PhysicsLevel {
			get {
				lock (staticMutationLock) {
					return physicsLevel;
				}
			}
			set {
				lock (staticMutationLock) {
					physicsLevel = value;
				}
			}
		}

		public static float SillyBitsIlluminationRadius {
			get {
				lock (staticMutationLock) {
					return sillyBitsIlluminationRadius;
				}
			}
			set {
				lock (staticMutationLock) {
					sillyBitsIlluminationRadius = value;
				}
			}
		}

		public static Vector3[] SillyBitsIlluminationColors {
			get {
				lock (staticMutationLock) {
					return sillyBitsIlluminationColors;
				}
			}
			set {
				lock (staticMutationLock) {
					sillyBitsIlluminationColors = value;
				}
			}
		}

		public static float PassCamMaxDist {
			get {
				lock (staticMutationLock) {
					return passCamMaxDist;
				}
			}
			set {
				lock (staticMutationLock) {
					passCamMaxDist = value;
				}
			}
		}

		public static float PassCamZoomOutSpeed {
			get {
				lock (staticMutationLock) {
					return passCamZoomOutSpeed;
				}
			}
			set {
				lock (staticMutationLock) {
					passCamZoomOutSpeed = value;
				}
			}
		}

		public static float PassCamSpin {
			get {
				lock (staticMutationLock) {
					return passCamSpin;
				}
			}
			set {
				lock (staticMutationLock) {
					passCamSpin = value;
				}
			}
		}

		public static float PassActorPreFadeLife {
			get {
				lock (staticMutationLock) {
					return passActorPreFadeLife;
				}
			}
			set {
				lock (staticMutationLock) {
					passActorPreFadeLife = value;
				}
			}
		}

		public static float PassActorFadeTime {
			get {
				lock (staticMutationLock) {
					return passActorFadeTime;
				}
			}
			set {
				lock (staticMutationLock) {
					passActorFadeTime = value;
				}
			}
		}

		public static float FinishLightRadius {
			get {
				lock (staticMutationLock) {
					return finishLightRadius;
				}
			}
			set {
				lock (staticMutationLock) {
					finishLightRadius = value;
				}
			}
		}

		public static Vector3 FinishLightColorGold {
			get {
				lock (staticMutationLock) {
					return finishLightColorGold;
				}
			}
			set {
				lock (staticMutationLock) {
					finishLightColorGold = value;
				}
			}
		}

		public static Vector3 FinishLightColorSilver {
			get {
				lock (staticMutationLock) {
					return finishLightColorSilver;
				}
			}
			set {
				lock (staticMutationLock) {
					finishLightColorSilver = value;
				}
			}
		}

		public static Vector3 FinishLightColorBronze {
			get {
				lock (staticMutationLock) {
					return finishLightColorBronze;
				}
			}
			set {
				lock (staticMutationLock) {
					finishLightColorBronze = value;
				}
			}
		}

		public static Vector3 FinishLightColorNoStar {
			get {
				lock (staticMutationLock) {
					return finishLightColorNoStar;
				}
			}
			set {
				lock (staticMutationLock) {
					finishLightColorNoStar = value;
				}
			}
		}

		public static int FinishLightRingNumLightsGold {
			get {
				lock (staticMutationLock) {
					return finishLightRingNumLightsGold;
				}
			}
			set {
				lock (staticMutationLock) {
					finishLightRingNumLightsGold = value;
				}
			}
		}

		public static int FinishLightRingNumLightsSilver {
			get {
				lock (staticMutationLock) {
					return finishLightRingNumLightsSilver;
				}
			}
			set {
				lock (staticMutationLock) {
					finishLightRingNumLightsSilver = value;
				}
			}
		}

		public static int FinishLightRingNumLightsBronze {
			get {
				lock (staticMutationLock) {
					return finishLightRingNumLightsBronze;
				}
			}
			set {
				lock (staticMutationLock) {
					finishLightRingNumLightsBronze = value;
				}
			}
		}

		public static float FinishLightRingRadius {
			get {
				lock (staticMutationLock) {
					return finishLightRingRadius;
				}
			}
			set {
				lock (staticMutationLock) {
					finishLightRingRadius = value;
				}
			}
		}

		public static float FinishLightRingLightRadius {
			get {
				lock (staticMutationLock) {
					return finishLightRingLightRadius;
				}
			}
			set {
				lock (staticMutationLock) {
					finishLightRingLightRadius = value;
				}
			}
		}

		public static float FinishLightRingStarSpawnRateGold {
			get {
				lock (staticMutationLock) {
					return finishLightRingStarSpawnRateGold;
				}
			}
			set {
				lock (staticMutationLock) {
					finishLightRingStarSpawnRateGold = value;
				}
			}
		}

		public static float FinishLightRingStarSpawnRateSilver {
			get {
				lock (staticMutationLock) {
					return finishLightRingStarSpawnRateSilver;
				}
			}
			set {
				lock (staticMutationLock) {
					finishLightRingStarSpawnRateSilver = value;
				}
			}
		}

		public static float FinishLightRingStarSpawnRateBronze {
			get {
				lock (staticMutationLock) {
					return finishLightRingStarSpawnRateBronze;
				}
			}
			set {
				lock (staticMutationLock) {
					finishLightRingStarSpawnRateBronze = value;
				}
			}
		}

		public static float FinishLightRingFallingStarLifetime {
			get {
				lock (staticMutationLock) {
					return finishLightRingFallingStarLifetime;
				}
			}
			set {
				lock (staticMutationLock) {
					finishLightRingFallingStarLifetime = value;
				}
			}
		}

		public static int TimeWarningMs {
			get {
				lock (staticMutationLock) {
					return timeWarningMs;
				}
			}
			set {
				lock (staticMutationLock) {
					timeWarningMs = value;
				}
			}
		}

		public static int TimePitchRaiseMs {
			get {
				lock (staticMutationLock) {
					return timePitchRaiseMs;
				}
			}
			set {
				lock (staticMutationLock) {
					timePitchRaiseMs = value;
				}
			}
		}

		public static float CameraHeightOffset {
			get {
				lock (staticMutationLock) {
					return cameraHeightOffset;
				}
			}
			set {
				lock (staticMutationLock) {
					cameraHeightOffset = value;
				}
			}
		}

		public static float CameraDistanceOffset {
			get {
				lock (staticMutationLock) {
					return cameraDistanceOffset;
				}
			}
			set {
				lock (staticMutationLock) {
					cameraDistanceOffset = value;
				}
			}
		}
		#endregion
	}
}