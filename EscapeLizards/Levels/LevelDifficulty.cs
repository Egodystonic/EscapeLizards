// All code copyright (c) 2017 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 30 03 2017 at 00:37 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using Ophidian.Losgap;
using Ophidian.Losgap.Rendering;

namespace Egodystonic.EscapeLizards {
	public enum LevelDifficulty {
		Easy,
		Tricky,
		Hard,
		VeryHard
	}

	public static class LevelDifficultyExtensions {
		public static Vector3 Color(this LevelDifficulty @this) {
			switch (@this) {
				case LevelDifficulty.Easy:
					return new Vector3(0.7f, 1f, 0.7f);
				case LevelDifficulty.Tricky:
					return new Vector3(1f, 1f, 0.7f);
				case LevelDifficulty.Hard:
					return new Vector3(1f, 0.7f, 0.4f);
				case LevelDifficulty.VeryHard:
					return new Vector3(1f, 0.4f, 0.4f);
				default:
					throw new ArgumentOutOfRangeException("this");
			}
		}

		public static ITexture2D Tex(this LevelDifficulty @this) {
			switch (@this) {
				case LevelDifficulty.Easy:
					return AssetLocator.DifficultyEasyIcon;
				case LevelDifficulty.Tricky:
					return AssetLocator.DifficultyTrickyIcon;
				case LevelDifficulty.Hard:
					return AssetLocator.DifficultyHardIcon;
				case LevelDifficulty.VeryHard:
					return AssetLocator.DifficultyVeryHardIcon;
				default:
					throw new ArgumentOutOfRangeException("this");
			}
		}

		public static string FriendlyName(this LevelDifficulty @this) {
			switch (@this) {
				case LevelDifficulty.Easy:
					return "Easy";
				case LevelDifficulty.Tricky:
					return "Tricky";
				case LevelDifficulty.Hard:
					return "Hard";
				case LevelDifficulty.VeryHard:
					return "Very Hard";
				default:
					throw new ArgumentOutOfRangeException("this");
			}
		}
	}
}