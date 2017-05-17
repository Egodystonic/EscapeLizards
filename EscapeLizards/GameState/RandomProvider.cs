// All code copyright (c) 2016 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 17 03 2016 at 16:53 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;

namespace Egodystonic.EscapeLizards {
	public static class RandomProvider {
		[ThreadStatic]
		private static Random threadLocalRandom;

		public static int Next(int minInc, int maxEx) {
			if (threadLocalRandom == null) threadLocalRandom = new Random();
			return threadLocalRandom.Next(minInc, maxEx);
		}

		public static float Next(float minInc, float maxInc) {
			if (threadLocalRandom == null) threadLocalRandom = new Random();
			float delta = maxInc - minInc;
			return (float) (minInc + threadLocalRandom.NextDouble() * delta);
		}
	}
}