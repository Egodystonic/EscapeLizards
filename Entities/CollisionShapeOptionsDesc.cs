// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 26 06 2015 at 16:04 by Ben Bowen

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Entities {
	[StructLayout(LayoutKind.Sequential, Pack = (int) InteropUtils.StructPacking.Safe)]
	public struct CollisionShapeOptionsDesc {
		private readonly Vector4 scaling;

		public Vector3 Scaling {
			get {
				return (Vector3) scaling;
			}
		}

		public CollisionShapeOptionsDesc(Vector3 scaling) {
			this.scaling = scaling;
		}
	}
}