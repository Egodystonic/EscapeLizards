// All code copyright (c) 2017 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 06 02 2017 at 10:11 by Ben Bowen

using System;
using System.Collections.Generic;

namespace Ophidian.Losgap.Rendering {
	// normals point inside the frustum
	public struct CameraFrustum {
		public readonly Plane NearPlane;
		public readonly Plane FarPlane;
		public readonly Plane LeftPlane;
		public readonly Plane RightPlane;
		public readonly Plane TopPlane;
		public readonly Plane BottomPlane;

		public bool IsWithinFrustum(Sphere sphere) {
			var threshold = -sphere.Radius;
			return NearPlane.SignedDistanceFrom(sphere.Center) >= threshold
				&& FarPlane.SignedDistanceFrom(sphere.Center) >= threshold
				&& LeftPlane.SignedDistanceFrom(sphere.Center) >= threshold
				&& RightPlane.SignedDistanceFrom(sphere.Center) >= threshold
				&& TopPlane.SignedDistanceFrom(sphere.Center) >= threshold
				&& BottomPlane.SignedDistanceFrom(sphere.Center) >= threshold;
		}

		public CameraFrustum(Plane nearPlane, Plane farPlane, Plane leftPlane, Plane rightPlane, Plane topPlane, Plane bottomPlane) {
			this.NearPlane = nearPlane;
			this.FarPlane = farPlane;
			this.LeftPlane = leftPlane;
			this.RightPlane = rightPlane;
			this.TopPlane = topPlane;
			this.BottomPlane = bottomPlane;
		}
	}
}