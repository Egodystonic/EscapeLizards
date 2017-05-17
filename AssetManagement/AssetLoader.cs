using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ophidian.Losgap.AssetManagement {
	public static partial class AssetLoader {
		private static readonly object staticMutationLock = new object();
		private static readonly Dictionary<string, ModelDataStream> preloadedModels = new Dictionary<string, ModelDataStream>();

		public static ModelDataStream LoadModel(string fileName) {
			Assure.NotNull(fileName);
			Assure.True(IOUtils.IsValidFilePath(fileName));

			lock (staticMutationLock) {
				if (preloadedModels.ContainsKey(fileName)) return preloadedModels[fileName];
			}

			string fullFileName = Path.Combine(LosgapSystem.InstallationDirectory.ToString(), fileName);

			if (!File.Exists(fullFileName)) throw new FileNotFoundException("Could not load model: Given file ('" + fullFileName + "') not found.", fullFileName);

			string fileExtToUpper = Path.GetExtension(fullFileName).ToUpper();
			ModelDataStream result;
			switch (fileExtToUpper) {
				case ".OBJ": result = LoadObj(fullFileName); break;
				default: throw new AssetFormatNotSupportedException("The model filetype " + fileExtToUpper + " is not supported");
			}

			lock (staticMutationLock) {
				preloadedModels[fileName] = result;
			}

			return result;
		}

		public static void ClearCache() {
			lock (staticMutationLock) {
				preloadedModels.Clear();
			}
		}
	}
}
