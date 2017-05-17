// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 29 05 2015 at 17:25 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using Ophidian.Losgap;
using Ophidian.Losgap.Rendering;

namespace Egodystonic.EscapeLizards {
	public class IlluminatingCamera : Camera {
		public readonly Light Light;

		public IlluminatingCamera(float lightRadius, Vector3 lightColor) {
			Light = new Light(Vector3.ZERO, lightRadius, lightColor);
			AssetLocator.LightPass.AddLight(Light);
		}

		public override unsafe Vector3 Position {
			get {
				return base.Position;
			}
			set {
				lock (InstanceMutationLock) {
					base.Position = value;
					Light.Position = value;
				}
			}
		}

		public override unsafe void Move(Vector3 distance) {
			lock (InstanceMutationLock) {
				base.Move(distance);
				Light.Position = _Position;
			}
		}

		public override unsafe void Dispose() {
			lock (InstanceMutationLock) {
				if (!IsDisposed) AssetLocator.LightPass.RemoveLight(Light);
				base.Dispose();	
			}
		}
	}
}