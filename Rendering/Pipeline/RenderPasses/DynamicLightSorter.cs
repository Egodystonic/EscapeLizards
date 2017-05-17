// All code copyright (c) 2017 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 07 02 2017 at 10:56 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;

namespace Ophidian.Losgap.Rendering {
	public sealed class DynamicLightSorter : IComparer<LightProperties> {
		public Vector3 CameraPosition = Vector3.ZERO;

		public int Compare(LightProperties x, LightProperties y) {
			var xDist = Math.Max(0f, Vector3.DistanceSquared(CameraPosition, x.Position) - x.Radius * x.Radius);
			var yDist = Math.Max(0f, Vector3.DistanceSquared(CameraPosition, y.Position) - y.Radius * y.Radius);
			return xDist.CompareTo(yDist);
		}
	}
}