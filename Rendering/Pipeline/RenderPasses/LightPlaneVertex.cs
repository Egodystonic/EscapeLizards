// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 27 04 2015 at 19:22 by Ben Bowen

using System;
using System.Collections.Generic;

namespace Ophidian.Losgap.Rendering {
	internal struct LightPlaneVertex {
		[VertexComponent("POSITION")]
		public readonly Vector3 Position;
		[VertexComponent("TEXCOORD")]
		public readonly Vector2 TexCoord;

		public LightPlaneVertex(Vector3 position, Vector2 texCoord) {
			Position = position;
			TexCoord = texCoord;
		}
	}
}