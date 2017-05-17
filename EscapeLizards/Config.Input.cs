// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 08 07 2015 at 16:53 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using Ophidian.Losgap.Input;

namespace Egodystonic.EscapeLizards {
	public static partial class Config {
		private static VirtualKey tiltGameboardForward = VirtualKey.Up;
		private static VirtualKey tiltGameboardBackward = VirtualKey.Down;
		private static VirtualKey tiltGameboardLeft = VirtualKey.Left;
		private static VirtualKey tiltGameboardRight = VirtualKey.Right;
		private static VirtualKey flipGameboardLeft = VirtualKey.D;
		private static VirtualKey flipGameboardRight = VirtualKey.S;
		private static VirtualKey hopEgg = VirtualKey.Space;
		private static VirtualKey fastRestart = VirtualKey.R;
		private static short optionSelectThreshold = (32768 / 3) * 2;
		private static bool invertCamControlY = false;
		private static short camOrientDeadZone = 32767 / 3;
		private static short boardTiltDeadZone = 32767 / 12;
		private static float initialRepeatTimeDelay = 0.5f;
		private static float successiveRepeatTimeDelay = 0.1f;

		public static VirtualKey Key_AccelerateLevelIntroduction {
			get {
				lock (staticMutationLock) {
					return hopEgg;
				}
			}
		}

		public static VirtualKey Key_TiltGameboardForward {
			get {
				lock (staticMutationLock) {
					return tiltGameboardForward;
				}
			}
			set {
				lock (staticMutationLock) {
					tiltGameboardForward = value;
				}
			}
		}

		public static VirtualKey Key_TiltGameboardBackward {
			get {
				lock (staticMutationLock) {
					return tiltGameboardBackward;
				}
			}
			set {
				lock (staticMutationLock) {
					tiltGameboardBackward = value;
				}
			}
		}

		public static VirtualKey Key_TiltGameboardLeft {
			get {
				lock (staticMutationLock) {
					return tiltGameboardLeft;
				}
			}
			set {
				lock (staticMutationLock) {
					tiltGameboardLeft = value;
				}
			}
		}

		public static VirtualKey Key_TiltGameboardRight {
			get {
				lock (staticMutationLock) {
					return tiltGameboardRight;
				}
			}
			set {
				lock (staticMutationLock) {
					tiltGameboardRight = value;
				}
			}
		}

		public static VirtualKey Key_FlipGameboardLeft {
			get {
				lock (staticMutationLock) {
					return flipGameboardLeft;
				}
			}
			set {
				lock (staticMutationLock) {
					flipGameboardLeft = value;
				}
			}
		}

		public static VirtualKey Key_FlipGameboardRight {
			get {
				lock (staticMutationLock) {
					return flipGameboardRight;
				}
			}
			set {
				lock (staticMutationLock) {
					flipGameboardRight = value;
				}
			}
		}

		public static VirtualKey Key_HopEgg {
			get {
				lock (staticMutationLock) {
					return hopEgg;
				}
			}
			set {
				lock (staticMutationLock) {
					hopEgg = value;
				}
			}
		}

		public static VirtualKey Key_FastRestart {
			get {
				lock (staticMutationLock) {
					return fastRestart;
				}
			}
			set {
				lock (staticMutationLock) {
					fastRestart = value;
				}
			}
		}

		public static short OptionSelectThreshold {
			get {
				lock (staticMutationLock) {
					return optionSelectThreshold;
				}
			}
			set {
				lock (staticMutationLock) {
					optionSelectThreshold = value;
				}
			}
		}

		public static bool InvertCamControlY {
			get {
				lock (staticMutationLock) {
					return invertCamControlY;
				}
			}
			set {
				lock (staticMutationLock) {
					invertCamControlY = value;
				}
			}
		}

		public static short CamOrientDeadZone {
			get {
				lock (staticMutationLock) {
					return camOrientDeadZone;
				}
			}
			set {
				lock (staticMutationLock) {
					camOrientDeadZone = value;
				}
			}
		}

		public static short BoardTiltDeadZone {
			get {
				lock (staticMutationLock) {
					return boardTiltDeadZone;
				}
			}
			set {
				lock (staticMutationLock) {
					boardTiltDeadZone = value;
				}
			}
		}

		public static float InitialRepeatTimeDelay {
			get {
				lock (staticMutationLock) {
					return initialRepeatTimeDelay;
				}
			}
			set {
				lock (staticMutationLock) {
					initialRepeatTimeDelay = value;
				}
			}
		}

		public static float SuccessiveRepeatTimeDelay {
			get {
				lock (staticMutationLock) {
					return successiveRepeatTimeDelay;
				}
			}
			set {
				lock (staticMutationLock) {
					successiveRepeatTimeDelay = value;
				}
			}
		}
	}
}