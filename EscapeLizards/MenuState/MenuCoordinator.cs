// All code copyright (c) 2016 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 20 07 2016 at 15:14 by Ben Bowen

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Ophidian.Losgap;
using Ophidian.Losgap.AssetManagement;
using Ophidian.Losgap.Entities;
using Ophidian.Losgap.Input;
using Ophidian.Losgap.Input.XInput;
using Ophidian.Losgap.Rendering;

namespace Egodystonic.EscapeLizards {
	public static partial class MenuCoordinator {
		private static readonly object staticMutationLock = new object();
		private static MenuState currentMenuState = MenuState.MainMenu;
		private const float BACKDROP_CAM_ROT_PER_SEC = MathUtils.PI * 0.025f;
		private const float BACKDROP_CAM_UPWARD_TILT = MathUtils.PI_OVER_TWO / -5f;
		private static readonly Quaternion BACKDROP_ROT_QUAT = Quaternion.FromAxialRotation(Vector3.DOWN, BACKDROP_CAM_ROT_PER_SEC);
		private const float BACKDROP_CAM_VERTICAL_OFFSET = PhysicsManager.ONE_METRE_SCALED * 22f;
		private const float SILLY_BIT_DROP_INTERVAL = 0.2f;
		private const float SILLY_BIT_DROP_FORWARD_OFFSET = PhysicsManager.ONE_METRE_SCALED * 0.75f;
		private const float SILLY_BIT_DROP_CONE = MathUtils.PI_OVER_TWO * 1f;
		private const float SILLY_BIT_DROP_HEIGHT = PhysicsManager.ONE_METRE_SCALED * 0.35f;
		private const float SILLY_BIT_CAM_ROT_CONE_BIAS = 0.66f;
		private const float SILLY_BIT_EXPLOSION_INTERVAL = 0.8f;
		private const int SILLY_BIT_EXPLOSION_SIZE = 8;
		private const float SILLY_BIT_IS_EGG_CHANCE = 0f;
		private static float lastSillyBitDrop = 0f;
		private static float lastSillyBitExplosion = 0f;
		private static readonly List<SillyBitEntity> activeSillyBits = new List<SillyBitEntity>();
		private static bool isTransitioning = false;
		private static float transitionTimeElapsed = 0f;
		private static float transitionTime = 0f;
		private static HUDTexture menuELLogoTex;
		private static HUDTexture menuIdentTex;
		private static FontString menuIdentString;
		private const float EL_LOGO_TEX_TARGET_X = -0.01f;
		private const float EL_IDENT_TEX_TARGET_X = 0.01f;
		private const float EL_IDENT_STRING_TARGET_Y = 0.023f;
		private const float IDENT_TEX_STARTING_X_OFFSET = -0.1f;
		private const float IDENT_STRING_STARTING_Y_OFFSET = -EL_IDENT_STRING_TARGET_Y - 0.05f;
		private const float DOF_FOCAL_DIST = PhysicsManager.ONE_METRE_SCALED * 55f;
		private const float DOF_MAX_DIST = DOF_FOCAL_DIST * 2.2f;
		private static HUDTexture worldIconTex;
		private const float WORLD_ICON_TARGET_ALPHA = 0.65f;
		private const float WORLD_ICON_ALPHA_PER_SEC = 0.2f;
		private static FontString versionString;
		private static Light cameraLight;
		private const float CAMERA_LIGHT_RADIUS = PhysicsManager.ONE_METRE_SCALED * 3f;
		private const float CAMERA_LIGHT_INTENSITY = 4f;
		private static float sillyBitSpawnRandomExtra = 0f;
		private const float MAX_RANDOM_SPAWN_EXTRA_TIME = 0.35f;
		private static float sillyBitExplosionRandomExtra = 0f;
		private const float MAX_RANDOM_EXPLOSION_EXTRA_TIME = 2f;

		private struct MenuOptionKeyState {
			public bool IsDown;
			public float LastDepress;
			public float LastTrigger;
		}

		private static MenuOptionKeyState downKeyState, downLSState;
		private static MenuOptionKeyState upKeyState, upLSState;
		private static MenuOptionKeyState leftKeyState, leftLSState;
		private static MenuOptionKeyState rightKeyState, rightLSState;

		private static event Action triggerDown;
		private static event Action triggerUp;
		private static event Action triggerLeft;
		private static event Action triggerRight;
		private static event Action triggerConfirm;
		private static event Action triggerBackOut;
		private static event Action<float> tick;
		private static event Action<float, float> transitionTick;
		private static event Action __currentTransitionComplete;
		private static event Action currentTransitionComplete {
			add {
				if (!isTransitioning) value();
				else __currentTransitionComplete += value;
			}
			remove {
				throw new NotSupportedException();
			}
		}
		private static SkyLevelDescription loadedMenuBackdrop;

		public static void LoadMenuFrontend(SkyLevelDescription preloadedSky = null, MenuState entryPointMenu = MenuState.MainMenu, Action deferenceModeAction = null) {
			lock (staticMutationLock) {
				MenuCoordinator.deferenceModeAction = deferenceModeAction;
				inDeferenceMode = deferenceModeAction != null;
				if (!inDeferenceMode) WorldModelCache.ClearAndDisposeCache();
				LoadMenuBackdrop(preloadedSky ?? loadedMenuBackdrop);
				switch (entryPointMenu) {
					case MenuState.PlayMenu: PlayMenuTransitionIn(); break;
					case MenuState.MedalsMenu: MedalsMenuTransitionIn(); break;
					case MenuState.OptionsMenu: OptionsTransitionIn(); break;
					default: MainMenuTransitionIn(); break;
				}
			}
		}

		private static void AddTransitionTickEvent(Action<float, float> tick) {
			transitionTick += tick;
		}

		private static void BeginTransition(float transitionTime) {
			isTransitioning = true;
			MenuCoordinator.transitionTime = transitionTime;
			transitionTimeElapsed = 0f;
			downKeyState.IsDown = false;
			upKeyState.IsDown = false;
			leftKeyState.IsDown = false;
			rightKeyState.IsDown = false;
			downLSState.IsDown = false;
			upLSState.IsDown = false;
			leftLSState.IsDown = false;
			rightLSState.IsDown = false;
		}

		private static void EndTransition(float lastDelta) {
			var transitionTickLocal = transitionTick;
			if (transitionTickLocal != null) transitionTickLocal(lastDelta, 1f);
			transitionTick = null;

			isTransitioning = false;
			var __currentTransitionCompleteLocal = __currentTransitionComplete;
			if (__currentTransitionCompleteLocal != null) {
				if (inDeferenceMode) {
					UnloadMenuBackdrop();
					CurrentMenuState = MenuState.InGame;

					deferenceModeAction();
					deferenceModeAction = null;
					inDeferenceMode = false;
				}
				__currentTransitionCompleteLocal();
			}
			__currentTransitionComplete = null;
		}

		private static bool inDeferenceMode;
		private static Action deferenceModeAction;

		public static MenuState CurrentMenuState {
			get {
				lock (staticMutationLock) {
					return currentMenuState;
				}
			}
			set {
				lock (staticMutationLock) {
					currentMenuState = value;
				}
			}
		}

		private static void SetIdentTransitionAmount(float transitionAmount, bool goingDown) {
			if (goingDown) transitionAmount = Math.Min(menuELLogoTex.Color.W, 1f - transitionAmount);
			else transitionAmount = Math.Max(menuELLogoTex.Color.W, transitionAmount);

			menuELLogoTex.SetAlpha(transitionAmount);
			menuELLogoTex.AnchorOffset = new Vector2(EL_LOGO_TEX_TARGET_X + IDENT_TEX_STARTING_X_OFFSET * (1f - transitionAmount), menuELLogoTex.AnchorOffset.Y);

			menuIdentTex.SetAlpha(transitionAmount);
			menuIdentTex.AnchorOffset = new Vector2(EL_IDENT_TEX_TARGET_X + IDENT_TEX_STARTING_X_OFFSET * (1f - transitionAmount), menuIdentTex.AnchorOffset.Y);

			menuIdentString.SetAlpha(transitionAmount);
			menuIdentString.AnchorOffset = new Vector2(menuIdentString.AnchorOffset.X, EL_IDENT_STRING_TARGET_Y + IDENT_STRING_STARTING_Y_OFFSET * (1f - transitionAmount));

			worldIconTex.SetAlpha(Math.Min(1f - transitionAmount, WORLD_ICON_TARGET_ALPHA));
		}

		public static void Init() {
			EntityModule.PostTick += Tick;
			Config.ConfigRefresh += ConfigRefresh;

			menuELLogoTex = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = ViewportAnchoring.TopRight,
				AnchorOffset = new Vector2(EL_LOGO_TEX_TARGET_X, 0f),
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = new Vector2(0.2f, 0.15f),
				Texture = AssetLocator.ELLogo,
				ZIndex = 9
			};

			menuIdentTex = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = ViewportAnchoring.TopLeft,
				AnchorOffset = new Vector2(EL_IDENT_TEX_TARGET_X, EL_IDENT_TEX_TARGET_X),
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = new Vector2(0.07f, 0.05f),
				Texture = AssetLocator.MainMenuPlayButton,
				ZIndex = 9
			};

			menuIdentString = AssetLocator.TitleFont.AddString(
				AssetLocator.HudLayer, 
				AssetLocator.MainWindow, 
				ViewportAnchoring.TopLeft,
				new Vector2(0.06f, EL_IDENT_STRING_TARGET_Y), 
				Vector2.ONE * 0.575f
			);
			menuIdentString.Text = String.Empty;
			menuIdentString.Color = new Vector3(95f, 145f, 46f) / 255f;

			worldIconTex = new HUDTexture(AssetLocator.HUDFragmentShader, AssetLocator.HudLayer, AssetLocator.MainWindow) {
				Anchoring = ViewportAnchoring.BottomLeft,
				AnchorOffset = new Vector2(-0.015f, -0.015f),
				AspectCorrectionStrategy = HUDTexture.AspectRatioCorrectionStrategy.UseBestUniformScaling,
				Color = new Vector4(1f, 1f, 1f, 0f),
				Rotation = 0f,
				Scale = new Vector2(0.15f, 0.25f),
				Texture = AssetLocator.WorldIcons[0],
				ZIndex = 2
			};

			var version = Assembly.GetExecutingAssembly().GetName().Version;
			versionString = AssetLocator.MainFont.AddString(
				AssetLocator.HudLayer,
				AssetLocator.MainWindow,
				ViewportAnchoring.BottomLeft,
				new Vector2(0.005f, 0.005f),
				Vector2.ONE * 0.2f
			);
			versionString.Text = "EL Build " + version.Build + "." + version.Revision;
			versionString.Color = new Vector4(1f, 1f, 1f, 0f);

			var hudIPB = AssetLocator.HudLayer.GetResource<InputBindingRegistry>();
			hudIPB.RegisterLeftStickEvent((x, y, _) => {
				lock (staticMutationLock) {
					if (currentMenuState == MenuState.InGame) return;
					float threshold = Config.OptionSelectThreshold;

					if (y <= -threshold) {
						if (!downLSState.IsDown) {
							downLSState.IsDown = true;
							downLSState.LastDepress = EntityModule.ElapsedTime;
							downLSState.LastTrigger = EntityModule.ElapsedTime;
							var triggerDownLocal = triggerDown;
							if (triggerDownLocal != null && !isTransitioning) triggerDownLocal();
						}
					}
					else downLSState.IsDown = false;

					if (y >= threshold) {
						if (!upLSState.IsDown) {
							upLSState.IsDown = true;
							upLSState.LastDepress = EntityModule.ElapsedTime;
							upLSState.LastTrigger = EntityModule.ElapsedTime;
							var triggerUpLocal = triggerUp;
							if (triggerUpLocal != null && !isTransitioning) triggerUpLocal();
						}
					}
					else upLSState.IsDown = false;

					if (x <= -threshold) {
						if (!leftLSState.IsDown) {
							leftLSState.IsDown = true;
							leftLSState.LastDepress = EntityModule.ElapsedTime;
							leftLSState.LastTrigger = EntityModule.ElapsedTime;
							var triggerLeftLocal = triggerLeft;
							if (triggerLeftLocal != null && !isTransitioning) triggerLeftLocal();
						}
					}
					else leftLSState.IsDown = false;

					if (x >= threshold) {
						if (!rightLSState.IsDown) {
							rightLSState.IsDown = true;
							rightLSState.LastDepress = EntityModule.ElapsedTime;
							rightLSState.LastTrigger = EntityModule.ElapsedTime;
							var triggerRightLocal = triggerRight;
							if (triggerRightLocal != null && !isTransitioning) triggerRightLocal();
						}
					}
					else rightLSState.IsDown = false;
				}
			});
			hudIPB.RegisterButtonEvent(
				new HashSet<ButtonFlags> { ButtonFlags.XINPUT_GAMEPAD_A },
				VirtualKeyState.KeyDown,
				(_, __, ___) => {
					lock (staticMutationLock) {
						if (currentMenuState == MenuState.InGame) return;
						var triggerConfirmLocal = triggerConfirm;
						if (triggerConfirmLocal != null && !isTransitioning) triggerConfirmLocal();
					}
				}
			);
			hudIPB.RegisterButtonEvent(
				new HashSet<ButtonFlags> { ButtonFlags.XINPUT_GAMEPAD_B },
				VirtualKeyState.KeyDown,
				(_, __, ___) => {
					lock (staticMutationLock) {
						if (currentMenuState == MenuState.InGame) return;
						var triggerBackOutLocal = triggerBackOut;
						if (triggerBackOutLocal != null && !isTransitioning) triggerBackOutLocal();
					}
				}
			);
			hudIPB.RegisterButtonEvent(
				new HashSet<ButtonFlags> { ButtonFlags.XINPUT_GAMEPAD_DPAD_DOWN, ButtonFlags.XINPUT_GAMEPAD_DPAD_UP, ButtonFlags.XINPUT_GAMEPAD_DPAD_LEFT, ButtonFlags.XINPUT_GAMEPAD_DPAD_RIGHT },
				VirtualKeyState.KeyDown | VirtualKeyState.KeyUp,
				(key, state, ___) => {
					lock (staticMutationLock) {
						if (currentMenuState == MenuState.InGame) return;
						if (state == VirtualKeyState.KeyDown) {
							switch (key) {
								case ButtonFlags.XINPUT_GAMEPAD_DPAD_DOWN:
									downKeyState.IsDown = true;
									downKeyState.LastDepress = EntityModule.ElapsedTime;
									downKeyState.LastTrigger = EntityModule.ElapsedTime;
									var triggerDownLocal = triggerDown;
									if (triggerDownLocal != null && !isTransitioning) triggerDownLocal();
									break;
								case ButtonFlags.XINPUT_GAMEPAD_DPAD_UP:
									upKeyState.IsDown = true;
									upKeyState.LastDepress = EntityModule.ElapsedTime;
									upKeyState.LastTrigger = EntityModule.ElapsedTime;
									var triggerUpLocal = triggerUp;
									if (triggerUpLocal != null && !isTransitioning) triggerUpLocal();
									break;
								case ButtonFlags.XINPUT_GAMEPAD_DPAD_LEFT:
									leftKeyState.IsDown = true;
									leftKeyState.LastDepress = EntityModule.ElapsedTime;
									leftKeyState.LastTrigger = EntityModule.ElapsedTime;
									var triggerLeftLocal = triggerLeft;
									if (triggerLeftLocal != null && !isTransitioning) triggerLeftLocal();
									break;
								case ButtonFlags.XINPUT_GAMEPAD_DPAD_RIGHT:
									rightKeyState.IsDown = true;
									rightKeyState.LastDepress = EntityModule.ElapsedTime;
									rightKeyState.LastTrigger = EntityModule.ElapsedTime;
									var triggerRightLocal = triggerRight;
									if (triggerRightLocal != null && !isTransitioning) triggerRightLocal();
									break;
							}
						}
						else {
							switch (key) {
								case ButtonFlags.XINPUT_GAMEPAD_DPAD_DOWN:
									downKeyState.IsDown = false;
									break;
								case ButtonFlags.XINPUT_GAMEPAD_DPAD_UP:
									upKeyState.IsDown = false;
									break;
								case ButtonFlags.XINPUT_GAMEPAD_DPAD_LEFT:
									leftKeyState.IsDown = false;
									break;
								case ButtonFlags.XINPUT_GAMEPAD_DPAD_RIGHT:
									rightKeyState.IsDown = false;
									break;
							}
						}
					}
				}
			);
			hudIPB.RegisterKeyEvent(
				new HashSet<VirtualKey> { VirtualKey.Enter, VirtualKey.Space },
				VirtualKeyState.KeyDown,
				(_, __) => {
					lock (staticMutationLock) {
						if (currentMenuState == MenuState.InGame) return;
						var triggerConfirmLocal = triggerConfirm;
						if (triggerConfirmLocal != null && !isTransitioning) triggerConfirmLocal();
					}
				}
			);
			hudIPB.RegisterKeyEvent(
				new HashSet<VirtualKey> { VirtualKey.Back, VirtualKey.Escape },
				VirtualKeyState.KeyDown,
				(_, __) => {
					lock (staticMutationLock) {
						if (currentMenuState == MenuState.InGame) return;
						var triggerBackOutLocal = triggerBackOut;
						if (triggerBackOutLocal != null && !isTransitioning) triggerBackOutLocal();
					}
				}
			);
			hudIPB.RegisterKeyEvent(
				new HashSet<VirtualKey> { VirtualKey.Down, VirtualKey.Up, VirtualKey.Left, VirtualKey.Right },
				VirtualKeyState.KeyDown | VirtualKeyState.KeyUp,
				(key, state) => {
					lock (staticMutationLock) {
						if (currentMenuState == MenuState.InGame) return;
						if (state == VirtualKeyState.KeyDown) {
							switch (key) {
								case VirtualKey.Down:
									downKeyState.IsDown = true;
									downKeyState.LastDepress = EntityModule.ElapsedTime;
									downKeyState.LastTrigger = EntityModule.ElapsedTime;
									var triggerDownLocal = triggerDown;
									if (triggerDownLocal != null && !isTransitioning) triggerDownLocal();
									break;
								case VirtualKey.Up:
									upKeyState.IsDown = true;
									upKeyState.LastDepress = EntityModule.ElapsedTime;
									upKeyState.LastTrigger = EntityModule.ElapsedTime;
									var triggerUpLocal = triggerUp;
									if (triggerUpLocal != null && !isTransitioning) triggerUpLocal();
									break;
								case VirtualKey.Left:
									leftKeyState.IsDown = true;
									leftKeyState.LastDepress = EntityModule.ElapsedTime;
									leftKeyState.LastTrigger = EntityModule.ElapsedTime;
									var triggerLeftLocal = triggerLeft;
									if (triggerLeftLocal != null && !isTransitioning) triggerLeftLocal();
									break;
								case VirtualKey.Right:
									rightKeyState.IsDown = true;
									rightKeyState.LastDepress = EntityModule.ElapsedTime;
									rightKeyState.LastTrigger = EntityModule.ElapsedTime;
									var triggerRightLocal = triggerRight;
									if (triggerRightLocal != null && !isTransitioning) triggerRightLocal();
									break;
							}
						}
						else {
							switch (key) {
								case VirtualKey.Down:
									downKeyState.IsDown = false;
									break;
								case VirtualKey.Up:
									upKeyState.IsDown = false;
									break;
								case VirtualKey.Left:
									leftKeyState.IsDown = false;
									break;
								case VirtualKey.Right:
									rightKeyState.IsDown = false;
									break;
							}
						}
					}
				}
			);
		}

		private static void Tick(float deltaTime) {
			if (deltaTime > 1f) return;
			lock (staticMutationLock) {
				if (currentMenuState == MenuState.InGame) return;
				if (cameraLight != null) cameraLight.Position = AssetLocator.MainCamera.Position;
				if (isTransitioning) {
					transitionTimeElapsed += deltaTime;
					if (transitionTimeElapsed >= transitionTime) EndTransition(deltaTime - (transitionTimeElapsed - transitionTime));
					if (currentMenuState == MenuState.InGame) return;
				}
				else {
					if (downKeyState.IsDown) {
						float timeSinceDepression = EntityModule.ElapsedTime - downKeyState.LastDepress;
						if (timeSinceDepression >= Config.InitialRepeatTimeDelay) {
							float timeSinceLastTrigger = EntityModule.ElapsedTime - downKeyState.LastTrigger;
							if (timeSinceLastTrigger >= Config.SuccessiveRepeatTimeDelay) {
								downKeyState.LastTrigger = EntityModule.ElapsedTime;
								var triggerDownLocal = triggerDown;
								if (triggerDownLocal != null) triggerDownLocal();
							}
						}
					}
					if (upKeyState.IsDown) {
						float timeSinceDepression = EntityModule.ElapsedTime - upKeyState.LastDepress;
						if (timeSinceDepression >= Config.InitialRepeatTimeDelay) {
							float timeSinceLastTrigger = EntityModule.ElapsedTime - upKeyState.LastTrigger;
							if (timeSinceLastTrigger >= Config.SuccessiveRepeatTimeDelay) {
								upKeyState.LastTrigger = EntityModule.ElapsedTime;
								var triggerUpLocal = triggerUp;
								if (triggerUpLocal != null) triggerUpLocal();
							}
						}
					}
					if (leftKeyState.IsDown) {
						float timeSinceDepression = EntityModule.ElapsedTime - leftKeyState.LastDepress;
						if (timeSinceDepression >= Config.InitialRepeatTimeDelay) {
							float timeSinceLastTrigger = EntityModule.ElapsedTime - leftKeyState.LastTrigger;
							if (timeSinceLastTrigger >= Config.SuccessiveRepeatTimeDelay) {
								leftKeyState.LastTrigger = EntityModule.ElapsedTime;
								var triggerLeftLocal = triggerLeft;
								if (triggerLeftLocal != null) triggerLeftLocal();
							}
						}
					}
					if (rightKeyState.IsDown) {
						float timeSinceDepression = EntityModule.ElapsedTime - rightKeyState.LastDepress;
						if (timeSinceDepression >= Config.InitialRepeatTimeDelay) {
							float timeSinceLastTrigger = EntityModule.ElapsedTime - rightKeyState.LastTrigger;
							if (timeSinceLastTrigger >= Config.SuccessiveRepeatTimeDelay) {
								rightKeyState.LastTrigger = EntityModule.ElapsedTime;
								var triggerRightLocal = triggerRight;
								if (triggerRightLocal != null) triggerRightLocal();
							}
						}
					}
					if (downLSState.IsDown) {
						float timeSinceDepression = EntityModule.ElapsedTime - downLSState.LastDepress;
						if (timeSinceDepression >= Config.InitialRepeatTimeDelay) {
							float timeSinceLastTrigger = EntityModule.ElapsedTime - downLSState.LastTrigger;
							if (timeSinceLastTrigger >= Config.SuccessiveRepeatTimeDelay) {
								downLSState.LastTrigger = EntityModule.ElapsedTime;
								var triggerDownLocal = triggerDown;
								if (triggerDownLocal != null) triggerDownLocal();
							}
						}
					}
					if (upLSState.IsDown) {
						float timeSinceDepression = EntityModule.ElapsedTime - upLSState.LastDepress;
						if (timeSinceDepression >= Config.InitialRepeatTimeDelay) {
							float timeSinceLastTrigger = EntityModule.ElapsedTime - upLSState.LastTrigger;
							if (timeSinceLastTrigger >= Config.SuccessiveRepeatTimeDelay) {
								upLSState.LastTrigger = EntityModule.ElapsedTime;
								var triggerUpLocal = triggerUp;
								if (triggerUpLocal != null) triggerUpLocal();
							}
						}
					}
					if (leftLSState.IsDown) {
						float timeSinceDepression = EntityModule.ElapsedTime - leftLSState.LastDepress;
						if (timeSinceDepression >= Config.InitialRepeatTimeDelay) {
							float timeSinceLastTrigger = EntityModule.ElapsedTime - leftLSState.LastTrigger;
							if (timeSinceLastTrigger >= Config.SuccessiveRepeatTimeDelay) {
								leftLSState.LastTrigger = EntityModule.ElapsedTime;
								var triggerLeftLocal = triggerLeft;
								if (triggerLeftLocal != null) triggerLeftLocal();
							}
						}
					}
					if (rightLSState.IsDown) {
						float timeSinceDepression = EntityModule.ElapsedTime - rightLSState.LastDepress;
						if (timeSinceDepression >= Config.InitialRepeatTimeDelay) {
							float timeSinceLastTrigger = EntityModule.ElapsedTime - rightLSState.LastTrigger;
							if (timeSinceLastTrigger >= Config.SuccessiveRepeatTimeDelay) {
								rightLSState.LastTrigger = EntityModule.ElapsedTime;
								var triggerRightLocal = triggerRight;
								if (triggerRightLocal != null) triggerRightLocal();
							}
						}
					}
				}

				if (!inDeferenceMode) {
					AssetLocator.MainCamera.Rotate(BACKDROP_ROT_QUAT.Subrotation(deltaTime));

					var deadBits = activeSillyBits.RemoveWhere(sb => sb.Transform.Translation.Y < AssetLocator.MainCamera.Position.Y - SILLY_BIT_DROP_HEIGHT);
					foreach (var deadBit in deadBits) deadBit.Dispose();

					float timeSinceLastBitDrop = EntityModule.ElapsedTime - lastSillyBitDrop;
					if (timeSinceLastBitDrop >= SILLY_BIT_DROP_INTERVAL / Config.PhysicsLevel + sillyBitSpawnRandomExtra) {
						int sillyBitIndex = RandomProvider.Next(0f, 1f) <= SILLY_BIT_IS_EGG_CHANCE ? -1 : RandomProvider.Next(0, AssetLocator.SillyBitsHandles.Length);
						SillyBitEntity newBit = new SillyBitEntity(sillyBitIndex);
						newBit.SetGravity(GameplayConstants.GRAVITY_ACCELERATION * RandomProvider.Next(0.025f, 0.175f) * Vector3.DOWN);
						newBit.AngularVelocity = new Vector3(
							RandomProvider.Next(-MathUtils.PI, MathUtils.PI),
							RandomProvider.Next(-MathUtils.PI, MathUtils.PI),
							RandomProvider.Next(-MathUtils.PI, MathUtils.PI)
						);
						Vector3 lateralOffset =
							AssetLocator.MainCamera.Orientation
							* SILLY_BIT_DROP_FORWARD_OFFSET
							* Quaternion.FromAxialRotation(
								Vector3.DOWN,
								RandomProvider.Next(
									-SILLY_BIT_DROP_CONE * (1f - SILLY_BIT_CAM_ROT_CONE_BIAS),
									SILLY_BIT_DROP_CONE * SILLY_BIT_CAM_ROT_CONE_BIAS
								)
							);
						newBit.SetTranslation(AssetLocator.MainCamera.Position + Vector3.UP * SILLY_BIT_DROP_HEIGHT + lateralOffset);

						activeSillyBits.Add(newBit);

						HUDSound.PostPassEggPop.Play(0.2f, RandomProvider.Next(0.5f, 1.5f));
						lastSillyBitDrop = EntityModule.ElapsedTime;
						sillyBitSpawnRandomExtra = RandomProvider.Next(0f, MAX_RANDOM_SPAWN_EXTRA_TIME);
					}

					float timeSinceLastBitExplosion = EntityModule.ElapsedTime - lastSillyBitExplosion;
					if (timeSinceLastBitExplosion >= SILLY_BIT_EXPLOSION_INTERVAL / Config.PhysicsLevel + sillyBitExplosionRandomExtra && activeSillyBits.Any()) {
						var nonEggBits = activeSillyBits.Except(bit => bit.BitIndex == -1);
						if (nonEggBits.Any()) {
							var targetBit = nonEggBits.ElementAt(RandomProvider.Next(0, nonEggBits.Count()));
							for (int i = 0; i < SILLY_BIT_EXPLOSION_SIZE; ++i) {
								Vector3 targetDir = new Vector3(
									RandomProvider.Next(-1f, 1f),
									RandomProvider.Next(-1f, 1f),
									RandomProvider.Next(-1f, 1f)
								);
								if (targetDir == Vector3.ZERO) targetDir = AssetLocator.MainCamera.Orientation;

								SillyBitEntity newBit = new SillyBitEntity(targetBit.BitIndex);
								newBit.AngularVelocity = new Vector3(
									RandomProvider.Next(-MathUtils.PI, MathUtils.PI),
									RandomProvider.Next(-MathUtils.PI, MathUtils.PI),
									RandomProvider.Next(-MathUtils.PI, MathUtils.PI)
								);
								newBit.SetTranslation(targetBit.Transform.Translation + targetDir.WithLength(PhysicsManager.ONE_METRE_SCALED * 0.1f));
								newBit.Velocity = targetDir.WithLength(PhysicsManager.ONE_METRE_SCALED * 0.5f);
								newBit.SetGravity(GameplayConstants.GRAVITY_ACCELERATION * RandomProvider.Next(0.025f, 0.175f) * Vector3.DOWN);

								activeSillyBits.Add(newBit);
							}
							HUDSound.PostPassShowMenu.Play(RandomProvider.Next(0f, 0.5f), RandomProvider.Next(1.5f, 0.5f));
						}


						lastSillyBitExplosion = EntityModule.ElapsedTime;
						sillyBitExplosionRandomExtra = RandomProvider.Next(0f, MAX_RANDOM_EXPLOSION_EXTRA_TIME);
					}

					if (menuIdentTex.Color.W == 0f && worldIconTex.Color.W < WORLD_ICON_TARGET_ALPHA) {
						worldIconTex.AdjustAlpha(WORLD_ICON_ALPHA_PER_SEC * deltaTime);
						if (worldIconTex.Color.W > WORLD_ICON_TARGET_ALPHA) worldIconTex.SetAlpha(WORLD_ICON_TARGET_ALPHA);
					}
				}

				if (versionString.Color.W < WORLD_ICON_TARGET_ALPHA) {
					versionString.AdjustAlpha(WORLD_ICON_ALPHA_PER_SEC * deltaTime);
					if (versionString.Color.W > WORLD_ICON_TARGET_ALPHA) versionString.SetAlpha(WORLD_ICON_TARGET_ALPHA);
				}

				var transitionTickLocal = transitionTick;
				if (transitionTickLocal != null) transitionTickLocal(deltaTime, transitionTimeElapsed / transitionTime);
				var tickLocal = tick;
				if (tickLocal != null) tickLocal(deltaTime);
			}
		}

		private static void ConfigRefresh() {
			if (!inDeferenceMode) {
				if (!Single.IsPositiveInfinity(Config.DofLensMaxBlurDist)) AssetLocator.LightPass.SetLensProperties(DOF_FOCAL_DIST, DOF_MAX_DIST);
				else AssetLocator.LightPass.SetLensProperties(Config.DofLensFocalDist, Config.DofLensMaxBlurDist);	
			}
		}

		private static bool mainMenuSinceLoad = true;
		private static void LoadMenuBackdrop(SkyLevelDescription preloadedSky) {
			UnloadMenuBackdrop();

			if (!inDeferenceMode) {
				cameraLight = new Light(Vector3.ZERO, CAMERA_LIGHT_RADIUS, Vector3.ONE * CAMERA_LIGHT_INTENSITY);
				AssetLocator.LightPass.AddLight(cameraLight);
				//AssetLoader.ClearCache(); // TODO work out whether we really need this line?
				if (!Single.IsPositiveInfinity(Config.DofLensMaxBlurDist)) AssetLocator.LightPass.SetLensProperties(DOF_FOCAL_DIST, DOF_MAX_DIST);
				AssetLocator.MainCamera.Position = Vector3.ZERO + Vector3.DOWN * BACKDROP_CAM_VERTICAL_OFFSET;
				AssetLocator.ShadowcasterCamera.Position = Vector3.UP * SILLY_BIT_DROP_HEIGHT * 1.1f;
				AssetLocator.ShadowcasterCamera.LookAt(Vector3.DOWN, Vector3.FORWARD);
				AssetLocator.ShadowcasterCamera.OrthographicDimensions = new Vector3(100f, 100f, PhysicsManager.ONE_METRE_SCALED * 3f);
				Quaternion downTilt = Quaternion.FromAxialRotation(Vector3.LEFT, BACKDROP_CAM_UPWARD_TILT);
				AssetLocator.MainCamera.Orient(Vector3.FORWARD * downTilt, Vector3.UP * downTilt);
				BGMManager.CrossFadeToTitleMusic();
				mainMenuSinceLoad = false;
			}

			if (preloadedSky != null) {
				loadedMenuBackdrop = preloadedSky;
			}
			else {
				int worldIndex = RandomProvider.Next(0, PersistedWorldData.GetFurthestUnlockedWorldIndex() + 1);
				loadedMenuBackdrop = (SkyLevelDescription) LevelDescription.Load(
					Path.Combine(AssetLocator.LevelsDir, LevelDatabase.SkyboxFileNames[worldIndex])
				);
				loadedMenuBackdrop.ReinitializeAll();
				worldIconTex.Texture = AssetLocator.WorldIcons[worldIndex];
			}
		}

		private static void UnloadMenuBackdrop() {
			if (cameraLight != null) {
				AssetLocator.LightPass.RemoveLight(cameraLight);
				cameraLight = null;
			}

			if (!inDeferenceMode) {
				if (loadedMenuBackdrop != null) GameCoordinator.PassMenuBackdropAsSkybox(loadedMenuBackdrop);
				loadedMenuBackdrop = null;
				AssetLocator.LightPass.SetLensProperties(Config.DofLensFocalDist, Config.DofLensMaxBlurDist);
				AssetLocator.ShadowcasterCamera.OrthographicDimensions = new Vector3(1000f, 1000f, PhysicsManager.ONE_METRE_SCALED * 100f);
			}
			
			foreach (var bit in activeSillyBits) {
				bit.Dispose();
			}
			activeSillyBits.Clear();
			worldIconTex.SetAlpha(0f);
			versionString.SetAlpha(0f);
		}

		public enum MenuState {
			MainMenu,
			PlayMenu,
			MedalsMenu,
			OptionsMenu,
			InGame
		}
	}
}