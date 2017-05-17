// All code copyright (c) 2016 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 10 10 2016 at 16:00 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;

namespace Ophidian.Losgap.Entities {
	internal sealed class RTCDComparer : IComparer<RayTestCollisionDesc> {
		public Vector3 RayOrigin;

		public int Compare(RayTestCollisionDesc x, RayTestCollisionDesc y) {
			return
				Vector3.DistanceSquared((Vector3) x.Position, RayOrigin)
				.CompareTo(
					Vector3.DistanceSquared((Vector3) y.Position, RayOrigin)
				);
		}
	}
}