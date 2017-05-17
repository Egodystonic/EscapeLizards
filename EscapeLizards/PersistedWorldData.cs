// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 17 08 2015 at 16:43 by Ben Bowen

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using FMOD;
using Ophidian.Losgap;

namespace Egodystonic.EscapeLizards {
	public static class PersistedWorldData {
		private static readonly object staticMutationLock = new object();
		private const string FILE_NAME_SUFFIX = ".els";

		public static void LoadFromDisk() {
			lock (staticMutationLock) {
				try {
					CreateBlankSaveData();
					var userID = LeaderboardManager.LocalPlayerDetails.SteamID;
					var userKey = userID >> 1;
					Logger.Log("Loading data for userID " + userID + "...");
					string filepath = Path.Combine(LosgapSystem.MutableDataDirectory.FullName, userID + FILE_NAME_SUFFIX);
					if (!File.Exists(filepath)) {
						Logger.Log("No save data found for userID '" + userID + "' (at " + filepath + "): Using blank save data.");
						return;
					}

					try {
						string unencryptedFile = UnencryptData(File.ReadAllText(filepath), userKey);
						string[] lines = unencryptedFile.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
						foreach (var line in lines) {
							string[] lineSplit = line.Split(' ');
							if (lineSplit.Length != 2) {
								Logger.Warn("Invalid save file line: " + line);
								continue;
							}
							var header = lineSplit[0];
							string[] headerSplit = header.Split(':');
							if (headerSplit.Length != 2) {
								Logger.Warn("Invalid save file line: " + line);
								continue;
							}
							int worldIndex, levelIndex;
							bool validWorldIndex = Int32.TryParse(headerSplit[0], out worldIndex);
							bool validLevelIndex = Int32.TryParse(headerSplit[1], out levelIndex);
							if (!validWorldIndex || !validLevelIndex || worldIndex < 0 || worldIndex > 10 || levelIndex < 0 || levelIndex > 9) {
								Logger.Warn("Invalid save file line: " + line);
								continue;
							}

							var contentSplit = lineSplit[1].Split('/');
							if (contentSplit.Length != 3) {
								Logger.Warn("Invalid save file line: " + line);
								continue;
							}

							bool eggClaimed;
							bool validEggClaimed = Boolean.TryParse(contentSplit[0], out eggClaimed);
							if (!validEggClaimed) {
								Logger.Warn("Invalid save file line: " + line);
								continue;
							}

							int? bestTimeMs = null;
							if (contentSplit[1] != String.Empty) {
								int outBestTime;
								bool validTime = Int32.TryParse(contentSplit[1], out outBestTime);
								if (!validTime) {
									Logger.Warn("Invalid save file line: " + line);
									continue;
								}
								else bestTimeMs = outBestTime;
							}

							string[] unlockedCoinStrings = contentSplit[2].Split('-', StringSplitOptions.RemoveEmptyEntries);
							int[] unlockedCoinIndices = new int[unlockedCoinStrings.Length];
							for (int i = 0; i < unlockedCoinStrings.Length; ++i) {
								int index;
								bool validIndex = Int32.TryParse(unlockedCoinStrings[i], out index);
								if (!validIndex) {
									Logger.Warn("Invalid save file line: " + line);
									goto innerLoopBreak;
								}
								unlockedCoinIndices[i] = index;
							}

							LevelCompletionDetails lcd = new LevelCompletionDetails(eggClaimed, bestTimeMs, unlockedCoinIndices);
							SetDetails(worldIndex, levelIndex, lcd);

							innerLoopBreak:
							Logger.Debug("");
						}
					}
					catch (Exception e) {
						Logger.Warn("Could not open save data for Steam ID '" + userKey + "'. See associated error information. Creating blank save data.", e);
						CreateBlankSaveData();
					}
				}
				finally {
					RecalculateSecondHandState();
				}
			}
		}

		public static void SaveToDisk() {
			lock (staticMutationLock) {
				var userID = LeaderboardManager.LocalPlayerDetails.SteamID;
				var userKey = userID >> 1;
				Logger.Log("Saving data for userID '" + userID + "'...");
				string filepath = Path.Combine(LosgapSystem.MutableDataDirectory.FullName, userID + FILE_NAME_SUFFIX);

				try {
					StringBuilder unencryptedTextBuilder = new StringBuilder(11 * 10 * 50);
					for (int w = 0; w < 11; ++w) {
						for (int l = 0; l < 10; ++l) {
							unencryptedTextBuilder.Append(w.ToString());
							unencryptedTextBuilder.Append(':');
							unencryptedTextBuilder.Append(l.ToString());
							unencryptedTextBuilder.Append(' ');
							unencryptedTextBuilder.Append(GetDetails(w, l).GenerateFileString());
							unencryptedTextBuilder.Append('\r');
							unencryptedTextBuilder.Append('\n');
						}
					}
					File.WriteAllText(filepath, EncryptData(unencryptedTextBuilder.ToString(), userKey));
				}
				catch (Exception e) {
					Logger.Warn("Could not write save data for Steam ID '" + userKey + "'. See associated error information.", e);
				}
			}
		}

		private static string UnencryptData(string data, ulong userKey) {
			StringBuilder result = new StringBuilder();
			ulong prevValue = 0UL;
			foreach (string s in data.Split(' ', StringSplitOptions.RemoveEmptyEntries)) {
				ulong l = UInt64.Parse(s);
				ulong l2 = l - (prevValue >> 1);
				prevValue = l;
				result.Append((char) (l2 - userKey));
			}
			return result.ToString();
		}

		private static string EncryptData(string data, ulong userKey) {
			StringBuilder result = new StringBuilder();
			ulong prevValue = 0UL;
			for (int c = 0; c < data.Length; ++c) {
				prevValue = (prevValue >> 1) + (data[c] + userKey);
				result.Append(prevValue);
				result.Append(' ');
			}
			return result.ToString();
		}

		#region State
		private struct LevelCompletionDetails {
			public readonly bool GoldenEggClaimed;
			public readonly int? BestTimeRemainingMs;
			private readonly int[] coinIndicesCollected;

			public IEnumerable<int> CoinIndicesCollected {
				get {
					return coinIndicesCollected;
				}
			}

			public LevelCompletionDetails(bool goldenEggClaimed, int? bestTimeRemainingMs, int[] coinIndicesCollected) {
				GoldenEggClaimed = goldenEggClaimed;
				BestTimeRemainingMs = bestTimeRemainingMs;
				this.coinIndicesCollected = coinIndicesCollected;
			}

			public string GenerateFileString() {
				return GoldenEggClaimed + "/" + (BestTimeRemainingMs.HasValue ? BestTimeRemainingMs.ToString() : String.Empty) + "/" + coinIndicesCollected.Select(index => index.ToString()).Join("-");
			}
		}

		private static readonly LevelCompletionDetails[] rawState = new LevelCompletionDetails[10 * 11];

		private static LevelCompletionDetails GetDetails(LevelID lid) {
			return GetDetails(lid.WorldIndex, lid.LevelIndex);
		}
		private static LevelCompletionDetails GetDetails(int worldIndex, int levelIndex) {
			return rawState[worldIndex * 10 + levelIndex];
		}
		private static void SetDetails(int worldIndex, int levelIndex, LevelCompletionDetails details) {
			rawState[worldIndex * 10 + levelIndex] = details;
		}
		private static void SetDetails(LevelID lid, LevelCompletionDetails details) {
			SetDetails(lid.WorldIndex, lid.LevelIndex, details);
		}

		private static void CreateBlankSaveData() {
			for (int w = 0; w < 11; ++w) {
				for (int l = 0; l < 10; ++l) {
					SetDetails(w, l, new LevelCompletionDetails(false, null, new int[0]));
				}
			}
		}

		private static void RecalculateSecondHandState() {
			totalTakenCoins = 0;
			totalGoldenEggs = 0;
			totalGoldStars = 0;
			totalSilverStars = 0;
			totalBronzeStars = 0;
			for (int w = 0; w < 11; ++w) {
				for (int l = 0; l < 10; ++l) {
					var lcd = GetDetails(w, l);
					totalTakenCoins += lcd.CoinIndicesCollected.Count();
					if (lcd.GoldenEggClaimed) ++totalGoldenEggs;
					if (lcd.BestTimeRemainingMs.HasValue) {
						LevelID lid = new LevelID((byte) w, (byte) l);
						if (lcd.BestTimeRemainingMs >= LevelDatabase.GetLevelGoldTime(lid)) stars[w * 10 + l] = Star.Gold;
						else if (lcd.BestTimeRemainingMs >= LevelDatabase.GetLevelSilverTime(lid)) stars[w * 10 + l] = Star.Silver;
						else if (lcd.BestTimeRemainingMs >= LevelDatabase.GetLevelBronzeTime(lid)) stars[w * 10 + l] = Star.Bronze;
						else stars[w * 10 + l] = Star.None;

						switch (stars[w * 10 + l]) {
							case Star.Gold:
								++totalGoldStars;
								goto case Star.Silver;
							case Star.Silver:
								++totalSilverStars;
								goto case Star.Bronze;
							case Star.Bronze:
								++totalBronzeStars;
								break;
						}
					}
				}
			}
		}

		// Second hand state
		private static int totalTakenCoins = 0;
		private static int totalGoldenEggs = 0;
		private static readonly Star[] stars = new Star[11 * 10];
		private static int totalGoldStars = 0;
		private static int totalSilverStars = 0;
		private static int totalBronzeStars = 0;
		#endregion

		#region Get / Set State
		public static int? GetBestTimeForLevel(LevelID lid) {
			lock (staticMutationLock) {
				return GetDetails(lid).BestTimeRemainingMs;
			}
		}

		public static void SetBestTimeForLevel(LevelID lid, int timeRemainingMs) {
			lock (staticMutationLock) {
				var oldDetails = GetDetails(lid);
				if (oldDetails.BestTimeRemainingMs != null && timeRemainingMs <= oldDetails.BestTimeRemainingMs) {
					throw new ApplicationException("New time is not better than previous.");
				}

				SetDetails(lid, new LevelCompletionDetails(oldDetails.GoldenEggClaimed, timeRemainingMs, oldDetails.CoinIndicesCollected.ToArray()));
				RecalculateSecondHandState();
			}
		}

		public static IEnumerable<int> GetTakenCoinIndices(LevelID lid) {
			lock (staticMutationLock) {
				return GetDetails(lid).CoinIndicesCollected;
			}
		}

		public static void AddTakenCoins(LevelID lid, IEnumerable<int> indices) {
			lock (staticMutationLock) {
				LevelCompletionDetails oldDetails = GetDetails(lid);
				foreach (var index in indices) {
					if (oldDetails.CoinIndicesCollected.Contains(index)) throw new ApplicationException("Error: Can not add coin index '" + index + "' twice.");
				}
				int[] newIndicesArr = new int[oldDetails.CoinIndicesCollected.Count() + indices.Count()];
				for (int i = 0; i < newIndicesArr.Length; ++i) {
					if (i < oldDetails.CoinIndicesCollected.Count()) newIndicesArr[i] = oldDetails.CoinIndicesCollected.ElementAt(i);
					else newIndicesArr[i] = indices.ElementAt(i - oldDetails.CoinIndicesCollected.Count());
				}
				LevelCompletionDetails newDetails = new LevelCompletionDetails(
					oldDetails.GoldenEggClaimed,
					oldDetails.BestTimeRemainingMs,
					newIndicesArr
					);
				SetDetails(lid, newDetails);
				RecalculateSecondHandState();
			}
		}

		public static int GetTotalTakenCoins() {
			lock (staticMutationLock) {
				return totalTakenCoins;
			}
		}

		public static bool GoldenEggTakenForLevel(LevelID lid) {
			lock (staticMutationLock) {
				return GetDetails(lid).GoldenEggClaimed;
			}
		}

		public static Star GetStarForLevel(LevelID lid) {
			lock (staticMutationLock) {
				return stars[lid.WorldIndex * 10 + lid.LevelIndex];
			}
		}

		public static void AddGoldenEgg(LevelID lid) {
			lock (staticMutationLock) {
				var oldDetails = GetDetails(lid);
				if (oldDetails.GoldenEggClaimed) throw new ApplicationException("Golden egg already claimed for level.");
				SetDetails(lid, new LevelCompletionDetails(true, oldDetails.BestTimeRemainingMs, oldDetails.CoinIndicesCollected.ToArray()));
				RecalculateSecondHandState();
			}
		}

		public static int GetTotalGoldenEggs() {
			lock (staticMutationLock) {
				return totalGoldenEggs;
			}
		}

		public static int GetTotalGoldStars() {
			lock (staticMutationLock) {
				return totalGoldStars;
			}
		}
		public static int GetTotalSilverStars() {
			lock (staticMutationLock) {
				return totalSilverStars;
			}
		}
		public static int GetTotalBronzeStars() {
			lock (staticMutationLock) {
				return totalBronzeStars;
			}
		}

		public static int GetAcquiredGoldStarsForWorld(byte worldIndex) {
			lock (staticMutationLock) {
				int total = 0;
				for (int l = 0; l < 10; ++l) {
					if (stars[worldIndex * 10 + l] == Star.Gold) ++total;
				}
				return total;
			}
		}
		public static int GetAcquiredSilverStarsForWorld(byte worldIndex) {
			lock (staticMutationLock) {
				int total = 0;
				for (int l = 0; l < 10; ++l) {
					if (stars[worldIndex * 10 + l] >= Star.Silver) ++total;
				}
				return total;
			}
		}
		public static int GetAcquiredBronzeStarsForWorld(byte worldIndex) {
			lock (staticMutationLock) {
				int total = 0;
				for (int l = 0; l < 10; ++l) {
					if (stars[worldIndex * 10 + l] >= Star.Bronze) ++total;
				}
				return total;
			}
		}
		public static int GetAcquiredCoinsForWorld(byte worldIndex) {
			lock (staticMutationLock) {
				int total = 0;
				for (int l = 0; l < 10; ++l) {
					total += rawState[worldIndex * 10 + l].CoinIndicesCollected.Count();
				}
				return total;
			}
		}
		public static int GetAcquiredEggsForWorld(byte worldIndex) {
			lock (staticMutationLock) {
				int total = 0;
				for (int l = 0; l < 10; ++l) {
					if (rawState[worldIndex * 10 + l].GoldenEggClaimed) ++total;
				}
				return total;
			}
		}

		public static bool IsWorldUnlocked(byte worldIndex) {
			lock (staticMutationLock) {
				var unlockCriteria = LevelDatabase.GetWorldUnlockCriteria(worldIndex);
				return totalBronzeStars >= unlockCriteria.BronzeStarsRequired
					&& totalSilverStars >= unlockCriteria.SilverStarsRequired
					&& totalGoldStars >= unlockCriteria.GoldStarsRequired
					&& totalTakenCoins >= unlockCriteria.CoinsRequired;
			}
		}

		public static int GetFurthestUnlockedWorldIndex() {
			lock (staticMutationLock) {
				for (byte w = 0; w < 11; ++w) {
					if (!IsWorldUnlocked(w)) return w - 1;
				}
				return 10;
			}
		}

		public static bool EggMaterialIsUnlocked(LizardEggMaterial mat) {
			lock (staticMutationLock) {
				return totalGoldenEggs >= mat.GoldenEggCost();
			}
		}
		#endregion
	}
}