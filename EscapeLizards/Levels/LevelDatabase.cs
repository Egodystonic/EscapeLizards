// All code copyright (c) 2016 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 15 01 2016 at 16:32 by Ben Bowen

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Ophidian.Losgap;

namespace Egodystonic.EscapeLizards {
	public static class LevelDatabase {
		private struct LevelMetadata {
			public readonly string Title;
			public readonly int MaxStarTimeMs;
			public readonly int BronzeStarTimeMs;
			public readonly int SilverStarTimeMs;
			public readonly int GoldStarTimeMs;
			public readonly int TotalCoinsInLevel;

			public LevelMetadata(string title, int maxStarTimeMs, int bronzeStarTimeMs, int silverStarTimeMs, int goldStarTimeMs, int totalCoinsInLevel) {
				Title = title;
				MaxStarTimeMs = maxStarTimeMs;
				BronzeStarTimeMs = bronzeStarTimeMs;
				SilverStarTimeMs = silverStarTimeMs;
				GoldStarTimeMs = goldStarTimeMs;
				TotalCoinsInLevel = totalCoinsInLevel;
			}
		}

		public struct WorldUnlockCriteria {
			public readonly int BronzeStarsRequired;
			public readonly int SilverStarsRequired;
			public readonly int GoldStarsRequired;
			public readonly int CoinsRequired;

			public WorldUnlockCriteria(int bronzeStarsRequired, int silverStarsRequired, int goldStarsRequired, int coinsRequired) {
				BronzeStarsRequired = bronzeStarsRequired;
				SilverStarsRequired = silverStarsRequired;
				GoldStarsRequired = goldStarsRequired;
				CoinsRequired = coinsRequired;
			}
		}

		private static readonly object staticMutationLock = new object();

		private static readonly LevelMetadata[] loadedMetadata = new LevelMetadata[11 * 10];
		private static int totalCoinsInGame;
		private static readonly int[] totalCoinsPerWorld = new int[11];

		private static KVP<string, LevelDifficulty>[][] filenameMap = {
			// Home Forest
			new[] {
				new KVP<string, LevelDifficulty>("beginnings", LevelDifficulty.Easy),
				new KVP<string, LevelDifficulty>("camber", LevelDifficulty.Easy),
				new KVP<string, LevelDifficulty>("inoffensive", LevelDifficulty.Easy),
				new KVP<string, LevelDifficulty>("gardenPath", LevelDifficulty.Tricky),
				new KVP<string, LevelDifficulty>("knolls", LevelDifficulty.Tricky),
				new KVP<string, LevelDifficulty>("flumes", LevelDifficulty.Easy),
				new KVP<string, LevelDifficulty>("sBends", LevelDifficulty.Tricky),
				new KVP<string, LevelDifficulty>("pigPens", LevelDifficulty.Easy),
				new KVP<string, LevelDifficulty>("skySpeedway", LevelDifficulty.Tricky),
				new KVP<string, LevelDifficulty>("switches", LevelDifficulty.Hard)
			}, 

			// Human Town
			new[] {
				new KVP<string, LevelDifficulty>("crazyGolf", LevelDifficulty.Tricky),
				new KVP<string, LevelDifficulty>("squareRing", LevelDifficulty.Tricky),
				new KVP<string, LevelDifficulty>("xFoils", LevelDifficulty.Easy),
				new KVP<string, LevelDifficulty>("curls", LevelDifficulty.Easy),
				new KVP<string, LevelDifficulty>("rotators", LevelDifficulty.Hard),
				new KVP<string, LevelDifficulty>("muscular", LevelDifficulty.Hard),
				new KVP<string, LevelDifficulty>("interlinked", LevelDifficulty.Tricky),
				new KVP<string, LevelDifficulty>("spinningPlates", LevelDifficulty.Tricky),
				new KVP<string, LevelDifficulty>("speedBowls", LevelDifficulty.Tricky),
				new KVP<string, LevelDifficulty>("contorchestra", LevelDifficulty.VeryHard),
			},

			// Chinatown
			new[] {
				new KVP<string, LevelDifficulty>("3dSnake", LevelDifficulty.Easy),
				new KVP<string, LevelDifficulty>("noodles", LevelDifficulty.Tricky),
				new KVP<string, LevelDifficulty>("organicFlips", LevelDifficulty.Hard),
				new KVP<string, LevelDifficulty>("coinyGravity", LevelDifficulty.Hard),
				new KVP<string, LevelDifficulty>("reachingFlips", LevelDifficulty.Tricky),
				new KVP<string, LevelDifficulty>("piandaos", LevelDifficulty.Hard),
				new KVP<string, LevelDifficulty>("hurdles", LevelDifficulty.Easy),
				new KVP<string, LevelDifficulty>("steps", LevelDifficulty.VeryHard),
				new KVP<string, LevelDifficulty>("portals", LevelDifficulty.Hard),
				new KVP<string, LevelDifficulty>("chessboards", LevelDifficulty.VeryHard)
			},

			// Workshop
			new[] {
				new KVP<string, LevelDifficulty>("twizzles", LevelDifficulty.Easy),
				new KVP<string, LevelDifficulty>("oneHop", LevelDifficulty.Easy),
				new KVP<string, LevelDifficulty>("concentric", LevelDifficulty.Hard),
				new KVP<string, LevelDifficulty>("detour", LevelDifficulty.Hard),
				new KVP<string, LevelDifficulty>("untidy", LevelDifficulty.Tricky),
				new KVP<string, LevelDifficulty>("fanShaft", LevelDifficulty.Tricky),
				new KVP<string, LevelDifficulty>("switchboard", LevelDifficulty.Hard),
				new KVP<string, LevelDifficulty>("cogs", LevelDifficulty.Hard),
				new KVP<string, LevelDifficulty>("fluxGrid", LevelDifficulty.Hard),
				new KVP<string, LevelDifficulty>("launchPads", LevelDifficulty.VeryHard)
			},

			// Factory
			new[] {
				new KVP<string, LevelDifficulty>("squareMaze", LevelDifficulty.Easy),
				new KVP<string, LevelDifficulty>("river", LevelDifficulty.Tricky),
				new KVP<string, LevelDifficulty>("railBlockers", LevelDifficulty.Easy),
				new KVP<string, LevelDifficulty>("undulance", LevelDifficulty.Tricky),
				new KVP<string, LevelDifficulty>("fairways", LevelDifficulty.Hard),
				new KVP<string, LevelDifficulty>("ledges", LevelDifficulty.Tricky),
				new KVP<string, LevelDifficulty>("pistonHeads", LevelDifficulty.Hard),
				new KVP<string, LevelDifficulty>("longTilters", LevelDifficulty.VeryHard),
				new KVP<string, LevelDifficulty>("twists", LevelDifficulty.Hard),
				new KVP<string, LevelDifficulty>("platforms", LevelDifficulty.VeryHard),
			},

			// Desert
			new[] {
				new KVP<string, LevelDifficulty>("dunes", LevelDifficulty.Easy),
				new KVP<string, LevelDifficulty>("warpedDNA", LevelDifficulty.Tricky),
				new KVP<string, LevelDifficulty>("fearful", LevelDifficulty.Tricky),
				new KVP<string, LevelDifficulty>("sandySteps", LevelDifficulty.Tricky),
				new KVP<string, LevelDifficulty>("shortcuts", LevelDifficulty.VeryHard),
				new KVP<string, LevelDifficulty>("humps", LevelDifficulty.Easy),
				new KVP<string, LevelDifficulty>("fountain", LevelDifficulty.Tricky),
				new KVP<string, LevelDifficulty>("livingMaze", LevelDifficulty.Hard),
				new KVP<string, LevelDifficulty>("planets", LevelDifficulty.Hard),
				new KVP<string, LevelDifficulty>("pyramids", LevelDifficulty.VeryHard),
			},

			// Dark City
			new[] {
				new KVP<string, LevelDifficulty>("honeyed", LevelDifficulty.Tricky),
				new KVP<string, LevelDifficulty>("satellites", LevelDifficulty.Easy),
				new KVP<string, LevelDifficulty>("quilts", LevelDifficulty.Easy),
				new KVP<string, LevelDifficulty>("organicHops", LevelDifficulty.Hard),
				new KVP<string, LevelDifficulty>("turnblades", LevelDifficulty.Easy),
				new KVP<string, LevelDifficulty>("jumpTime", LevelDifficulty.Easy),
				new KVP<string, LevelDifficulty>("triangulation", LevelDifficulty.VeryHard),
				new KVP<string, LevelDifficulty>("radioactive", LevelDifficulty.Tricky),
				new KVP<string, LevelDifficulty>("lineage", LevelDifficulty.Hard),
				new KVP<string, LevelDifficulty>("shakenUp", LevelDifficulty.VeryHard),
			},

			// Rusty Sewer
			new[] {
				new KVP<string, LevelDifficulty>("zigzags", LevelDifficulty.Hard),
				new KVP<string, LevelDifficulty>("slingshots", LevelDifficulty.Tricky),
				new KVP<string, LevelDifficulty>("dangerousPath", LevelDifficulty.Easy),
				new KVP<string, LevelDifficulty>("tricktrack", LevelDifficulty.Tricky),
				new KVP<string, LevelDifficulty>("zebratic", LevelDifficulty.Tricky),
				new KVP<string, LevelDifficulty>("ceilingDanger", LevelDifficulty.Hard),
				new KVP<string, LevelDifficulty>("cups", LevelDifficulty.Hard),
				new KVP<string, LevelDifficulty>("octodots", LevelDifficulty.VeryHard),
				new KVP<string, LevelDifficulty>("trickyFlips", LevelDifficulty.Hard),
				new KVP<string, LevelDifficulty>("contorchestra2", LevelDifficulty.VeryHard),
			},

			// Icy Plain
			new[] {
				new KVP<string, LevelDifficulty>("glacialRamps", LevelDifficulty.VeryHard),
				new KVP<string, LevelDifficulty>("rollercoaster", LevelDifficulty.Tricky),
				new KVP<string, LevelDifficulty>("downwardSpiral", LevelDifficulty.Easy),
				new KVP<string, LevelDifficulty>("halfPipes", LevelDifficulty.Tricky),
				new KVP<string, LevelDifficulty>("tunnel", LevelDifficulty.Tricky),
				new KVP<string, LevelDifficulty>("ringFences", LevelDifficulty.Hard),
				new KVP<string, LevelDifficulty>("tiers", LevelDifficulty.Easy),
				new KVP<string, LevelDifficulty>("traps", LevelDifficulty.Easy),
				new KVP<string, LevelDifficulty>("gauntlet", LevelDifficulty.Hard),
				new KVP<string, LevelDifficulty>("acrobatics", LevelDifficulty.VeryHard),
			},

			// Pearly Path
			new[] {
				new KVP<string, LevelDifficulty>("mixedHops", LevelDifficulty.Easy),
				new KVP<string, LevelDifficulty>("cubeDrop", LevelDifficulty.Tricky),
				new KVP<string, LevelDifficulty>("trailingOff", LevelDifficulty.Easy),
				new KVP<string, LevelDifficulty>("showcases", LevelDifficulty.Easy),
				new KVP<string, LevelDifficulty>("spaghetti", LevelDifficulty.Hard),
				new KVP<string, LevelDifficulty>("pushers", LevelDifficulty.Easy),
				new KVP<string, LevelDifficulty>("bridges", LevelDifficulty.Hard),
				new KVP<string, LevelDifficulty>("eusocial", LevelDifficulty.Hard),
				new KVP<string, LevelDifficulty>("corridors", LevelDifficulty.Tricky),
				new KVP<string, LevelDifficulty>("rotatingLinks", LevelDifficulty.VeryHard),
			},

			// Elysian Fields
			new[] {
				new KVP<string, LevelDifficulty>("boxing", LevelDifficulty.Easy),
				new KVP<string, LevelDifficulty>("misaligned", LevelDifficulty.VeryHard),
				new KVP<string, LevelDifficulty>("quickFlips", LevelDifficulty.Hard),
				new KVP<string, LevelDifficulty>("kerplunk", LevelDifficulty.Easy),
				new KVP<string, LevelDifficulty>("giantMaze", LevelDifficulty.Tricky),
				new KVP<string, LevelDifficulty>("theCube", LevelDifficulty.VeryHard),
				new KVP<string, LevelDifficulty>("spiral", LevelDifficulty.Tricky),
				new KVP<string, LevelDifficulty>("alternators", LevelDifficulty.VeryHard),
				new KVP<string, LevelDifficulty>("3D", LevelDifficulty.Hard),
				new KVP<string, LevelDifficulty>("valhalla", LevelDifficulty.VeryHard),
			},
		};

		public static readonly string[] SkyboxFileNames = {
			"skybox_HomeForest.ell",
			"skybox_HumanTown.ell",
			"skybox_Chinatown.ell",
			"skybox_Workshop.ell",
			"skybox_Factory.ell",
			"skybox_Desert.ell",
			"skybox_DarkCity.ell",
			"skybox_RustySewer.ell",
			"skybox_IcyPlain.ell",
			"skybox_PearlyPath.ell",
			"skybox_ElysianFields.ell"
		};

		public static string GetWorldName(byte worldIndex) {
			switch (worldIndex) {
				case 0: return "Home Forest";
				case 1: return "Human Town";
				case 2: return "Chinatown";
				case 3: return "Workshop";
				case 4: return "Factory";
				case 5: return "Desert";
				case 6: return "Dark City";
				case 7: return "Rusty Sewer";
				case 8: return "Icy Plain";
				case 9: return "The Pearly Path";
				case 10: return "Elysian Fields";
				default: throw new ArgumentOutOfRangeException("worldIndex");
			}
		}

		public static WorldUnlockCriteria GetWorldUnlockCriteria(byte worldIndex) {
			int bronzeStarCount = worldIndex * 9;
			int silverStarCount = Math.Max(0, worldIndex - 1) * 7;
			int goldStarCount = Math.Max(0, worldIndex - 2) * 5;
			int coinCount = 0;

			for (int i = 0; i < worldIndex; ++i) {
				coinCount += (int) (totalCoinsPerWorld[i] * 0.33f);
			}

			return new WorldUnlockCriteria(bronzeStarCount, silverStarCount, goldStarCount, coinCount);
		}

		public static Vector3 GetWorldColor(byte worldIndex) {
			switch (worldIndex) {
				case 0: return new Vector3(91f, 163f, 0f) / 255f;
				case 1: return new Vector3(6f, 247f, 142f) / 255f;
				case 2: return new Vector3(255f, 18f, 10f) / 255f;
				case 3: return new Vector3(251f, 185f, 1f) / 255f;
				case 4: return new Vector3(116f, 130f, 163f) / 255f;
				case 5: return new Vector3(243f, 117f, 0f) / 255f;
				case 6: return new Vector3(0f, 38f, 95f) / 255f;
				case 7: return new Vector3(100f, 29f, 9f) / 255f;
				case 8: return new Vector3(217f, 235f, 249f) / 255f;
				case 9: return new Vector3(255f, 215f, 107f) / 255f;
				case 10: return new Vector3(130f, 86f, 152f) / 255f;
				default: throw new ArgumentOutOfRangeException("worldIndex");
			}
		}

		public static string GetLevelFileName(LevelID levelID) {
			return filenameMap[levelID.WorldIndex][levelID.LevelIndex].Key + ".ell";
		}

		public static bool LevelExists(LevelID levelID) {
			return filenameMap.Length > levelID.WorldIndex && filenameMap[levelID.WorldIndex].Length > levelID.LevelIndex;
		}

		public static List<LevelFilePair> GetAllLevelFilesInOrder() {
			List<LevelFilePair> result = new List<LevelFilePair>();
			for (int s = 0; s < filenameMap.Length; ++s) {
				if (s >= SkyboxFileNames.Length) break;
				for (int l = 0; l < filenameMap[s].Length; ++l) {
					result.Add(new LevelFilePair(
						new LevelID((byte) s, (byte) l), 
						SkyboxFileNames[s],
						filenameMap[s][l].Key
					));
				}
			}
			return result;
		}

		public static void LoadAllMetadata() {
			lock (staticMutationLock) {
				for (int worldIndex = 0; worldIndex < 11; ++worldIndex) {
					for (int levelIndex = 0; levelIndex < 10; ++levelIndex) {
						LevelID lid = new LevelID((byte) worldIndex, (byte) levelIndex);
						string filename = String.Empty;
						if (LevelExists(lid)) filename = Path.Combine(AssetLocator.LevelsDir, GetLevelFileName(lid));
						if (filename == String.Empty || !File.Exists(filename)) {
							StringBuilder randomStringBuilder = new StringBuilder();
							int strLen = RandomProvider.Next(3, 16);
							for (int c = 0; c < strLen; ++c) {
								char x = (char) RandomProvider.Next(65, 92);
								if (x == (char) 91) x = ' ';
								randomStringBuilder.Append(x);
							}
							var maxTime = 30000 + RandomProvider.Next(1, 5) * 15000;
							var bronzeTime = maxTime - RandomProvider.Next(1, 4) * 2500;
							var silverTime = bronzeTime - RandomProvider.Next(1, 4) * 2500;
							var goldTime = silverTime - RandomProvider.Next(1, 4) * 2500;
							loadedMetadata[worldIndex * 10 + levelIndex] = new LevelMetadata(
								randomStringBuilder.ToString(),
								maxTime,
								bronzeTime,
								silverTime,
								goldTime,
								RandomProvider.Next(1, 51)
							);
						}
						else {
							var levelDesc = (GameLevelDescription) LevelDescription.Load(filename, false);
							loadedMetadata[worldIndex * 10 + levelIndex] = new LevelMetadata(
								levelDesc.Title,
								levelDesc.LevelTimerMaxMs,
								levelDesc.LevelTimerBronzeMs,
								levelDesc.LevelTimerSilverMs,
								levelDesc.LevelTimerGoldMs,
								levelDesc.GetGameObjectsByType<LizardCoin>().Count()
							);
							levelDesc.Dispose();
						}
						var coinsInLevel = loadedMetadata[worldIndex * 10 + levelIndex].TotalCoinsInLevel;
						totalCoinsInGame += coinsInLevel;
						totalCoinsPerWorld[worldIndex] += coinsInLevel;
					}
				}
					
			}
		}

		public static string GetLevelTitle(LevelID lid) {
			lock (staticMutationLock) {
				return loadedMetadata[lid.WorldIndex * 10 + lid.LevelIndex].Title;
			}
		}

		public static LevelDifficulty GetLevelDifficulty(LevelID lid) {
			lock (staticMutationLock) {
				return filenameMap[lid.WorldIndex][lid.LevelIndex].Value;
			}
		}

		public static int GetLevelBronzeTime(LevelID lid) {
			lock (staticMutationLock) {
				return loadedMetadata[lid.WorldIndex * 10 + lid.LevelIndex].MaxStarTimeMs - loadedMetadata[lid.WorldIndex * 10 + lid.LevelIndex].BronzeStarTimeMs;
			}
		}
		public static int GetLevelSilverTime(LevelID lid) {
			lock (staticMutationLock) {
				return loadedMetadata[lid.WorldIndex * 10 + lid.LevelIndex].MaxStarTimeMs - loadedMetadata[lid.WorldIndex * 10 + lid.LevelIndex].SilverStarTimeMs;
			}
		}
		public static int GetLevelGoldTime(LevelID lid) {
			lock (staticMutationLock) {
				return loadedMetadata[lid.WorldIndex * 10 + lid.LevelIndex].MaxStarTimeMs - loadedMetadata[lid.WorldIndex * 10 + lid.LevelIndex].GoldStarTimeMs;
			}
		}
		public static int GetLevelMaxTime(LevelID lid) {
			lock (staticMutationLock) {
				return loadedMetadata[lid.WorldIndex * 10 + lid.LevelIndex].MaxStarTimeMs;
			}
		}
		public static int GetLevelTotalCoins(LevelID lid) {
			lock (staticMutationLock) {
				return loadedMetadata[lid.WorldIndex * 10 + lid.LevelIndex].TotalCoinsInLevel;
			}
		}
		public static string GetLevelBronzeTimeAsString(LevelID lid) {
			return MakeTimeString(GetLevelBronzeTime(lid));
		}
		public static string GetLevelSilverTimeAsString(LevelID lid) {
			return MakeTimeString(GetLevelSilverTime(lid));
		}
		public static string GetLevelGoldTimeAsString(LevelID lid) {
			return MakeTimeString(GetLevelGoldTime(lid));
		}
		public static string GetLevelMaxTimeAsString(LevelID lid) {
			return MakeTimeString(GetLevelMaxTime(lid));
		}

		public static int GetWorldTotalCoins(byte worldIndex) {
			lock (staticMutationLock) {
				int total = 0;
				for (byte l = 0; l < 10; ++l) {
					total += GetLevelTotalCoins(new LevelID(worldIndex, l));
				}
				return total;
			}
		}

		private static string MakeTimeString(int numMillis) {
			int numFullSeconds = numMillis / 1000;
			int numHundredths = (numMillis - numFullSeconds * 1000) / 10;

			return numFullSeconds.ToString("00") + ":" + numHundredths.ToString("00");
		}
	}
}