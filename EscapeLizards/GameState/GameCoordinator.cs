// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 08 07 2015 at 15:03 by Ben Bowen

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace Egodystonic.EscapeLizards {
	/// <summary>
	/// A static class that co-ordinates where we're at in the game (e.g. menu, level, world select screen, etc)
	/// </summary>
	public static partial class GameCoordinator {
		private static readonly object staticMutationLock = new object();
		private static OverallGameState currentGameState = OverallGameState.InMenuSystem;

		public static OverallGameState CurrentGameState {
			get {
				lock (staticMutationLock) {
					return currentGameState;
				}
			}
		}

		public static void Init() {
			lock (staticMutationLock) {
				InitLevelCoordination();
			}
		}

		public enum OverallGameState {
			LevelIntroduction,
			LevelPlaying,
			LevelFailed,
			LevelPassed,
			LevelPaused,
			InMenuSystem
		}
	}
}