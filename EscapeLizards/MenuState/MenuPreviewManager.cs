// All code copyright (c) 2016 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 29 08 2016 at 17:40 by Ben Bowen

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Ophidian.Losgap.Rendering;

namespace Egodystonic.EscapeLizards {
	internal static class MenuPreviewManager {
		private const int NUM_IMAGES_PER_WORLD = 3;
		private static readonly object staticMutationLock = new object();
		private static readonly List<KeyValuePair<ITexture2D, string>> loadedTextures = new List<KeyValuePair<ITexture2D, string>>();
		private static int curTexIndex;

		public static ITexture2D GetNextLoadedTexture() {
			lock (staticMutationLock) {
				if (loadedTextures.Count == 0) throw new ApplicationException("No textures loaded.");
				curTexIndex = (curTexIndex + 1) % loadedTextures.Count;
				return loadedTextures[curTexIndex].Key;
			}
		}

		public static void LoadWorld(byte worldIndex) {
			lock (staticMutationLock) {
				UnloadCurrentlyLoaded();

				for (int i = 0; i < NUM_IMAGES_PER_WORLD; ++i) {
					string filename = Path.Combine("Previews\\", "world" + worldIndex + "-" + i + ".png");
					if (!File.Exists(Path.Combine(AssetLocator.MaterialsDir, filename))) {
						string stubfile = "blue.bmp";
						if (i % 3 == 1) stubfile = "green.bmp";
						if (i % 3 == 2) stubfile = "red.bmp";
						loadedTextures.Add(new KeyValuePair<ITexture2D, string>(
							AssetLocator.LoadTexture(stubfile, false),
							stubfile
						));
					}
					else {
						loadedTextures.Add(new KeyValuePair<ITexture2D, string>(
							AssetLocator.LoadTexture(filename, false),
							filename
						));
					}
				}

				curTexIndex = loadedTextures.Count - 1;
			}
		}

		public static void LoadLevel(LevelID levelID) {
			lock (staticMutationLock) {
				UnloadCurrentlyLoaded();

				string filename = Path.Combine("Previews\\", "level" + levelID.WorldIndex + "-" + levelID.LevelIndex + ".png");
				if (!File.Exists(Path.Combine(AssetLocator.MaterialsDir, filename))) {
					loadedTextures.Add(new KeyValuePair<ITexture2D, string>(
						AssetLocator.LoadTexture("blue.bmp", false),
						"blue.bmp"
					));
				}
				else {
					loadedTextures.Add(new KeyValuePair<ITexture2D, string>(
						AssetLocator.LoadTexture(filename, false),
						filename
					));
				}

				curTexIndex = 0;
			}
		}

		public static void UnloadAll() {
			lock (staticMutationLock) {
				UnloadCurrentlyLoaded();
			}
		}

		private static void UnloadCurrentlyLoaded() {
			foreach (var kvp in loadedTextures) {
				AssetLocator.UnloadTexture(kvp.Value);
			}

			loadedTextures.Clear();
		}
	}
}