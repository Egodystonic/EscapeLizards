// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 08 06 2015 at 15:32 by Ben Bowen

using System;
using System.Collections.Generic;
using Ophidian.Losgap;
using Ophidian.Losgap.Entities;

namespace Egodystonic.EscapeLizards.Editor {
	public struct ArrowVect {
		public readonly Entity XArrow, YArrow, ZArrow;

		public ArrowVect(Entity xArrow, Entity yArrow, Entity zArrow) {
			XArrow = xArrow;
			YArrow = yArrow;
			ZArrow = zArrow;
		}

		public void TranslateBy(Vector3 amount) {
			XArrow.TranslateBy(amount);
			YArrow.TranslateBy(amount);
			ZArrow.TranslateBy(amount);
		}

		public void Dispose() {
			XArrow.Dispose();
			YArrow.Dispose();
			ZArrow.Dispose();
		}
	}
}