// All code copyright (c) 2016 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 15 07 2016 at 21:01 by Ben Bowen

using System;
using System.Collections.Generic;
using Ophidian.Losgap;
using Ophidian.Losgap.Rendering;

namespace Egodystonic.EscapeLizards {
	public class FreshCoin {
		public readonly HUDTexture CoinTex;
		public float AliveTime = 0f;
		public Vector2 CurrentVelocity = new Vector2(RandomProvider.Next(-0.3f, 0.3f), RandomProvider.Next(-0.12f, -0.6f));
		public bool Active = false;

		public FreshCoin(HUDTexture coinTex) {
			CoinTex = coinTex;
		}
	}
}