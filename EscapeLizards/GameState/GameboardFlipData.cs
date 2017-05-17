// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 18 08 2015 at 14:09 by Ben Bowen

using System;
using System.Collections.Generic;
using Ophidian.Losgap;

namespace Egodystonic.EscapeLizards {
	public struct GameboardFlipData {
		public readonly Vector3 previousDownDir;
		public readonly Quaternion rotationToTargetDownDir;

		public GameboardFlipData(Vector3 previousDownDir, Quaternion rotationToTargetDownDir) {
			this.previousDownDir = previousDownDir;
			this.rotationToTargetDownDir = rotationToTargetDownDir;
		}
	}
}