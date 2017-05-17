// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 08 07 2015 at 16:53 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using Ophidian.Losgap.Input;

namespace Egodystonic.EscapeLizards {
	public static partial class Config {
		private static float soundEffectVolume = 0.3f;
		private static float hudVolume = 0.3f;
		private static float musicVolume = 0.2f;

		public static float SoundEffectVolume {
			get {
				lock (staticMutationLock) {
					return soundEffectVolume;
				}
			}
			set {
				lock (staticMutationLock) {
					soundEffectVolume = value;
				}
			}
		}

		public static float HUDVolume {
			get {
				lock (staticMutationLock) {
					return hudVolume;
				}
			}
			set {
				lock (staticMutationLock) {
					hudVolume = value;
				}
			}
		}

		public static float MusicVolume {
			get {
				lock (staticMutationLock) {
					return musicVolume;
				}
			}
			set {
				lock (staticMutationLock) {
					musicVolume = value;
				}
			}
		}
	}
}