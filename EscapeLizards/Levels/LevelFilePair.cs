// All code copyright (c) 2016 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 14 10 2016 at 15:58 by Ben Bowen

using System;
using System.Collections.Generic;

namespace Egodystonic.EscapeLizards {
	public struct LevelFilePair {
		public readonly LevelID LevelID;
		public readonly string SkyboxFileName;
		public readonly string LevelFileName;

		internal LevelFilePair(LevelID levelId, string skyboxFileName, string levelFileName) {
			LevelID = levelId;
			SkyboxFileName = skyboxFileName;
			LevelFileName = levelFileName;
		}
	}
}