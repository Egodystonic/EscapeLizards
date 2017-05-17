// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 28 01 2015 at 16:47 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// An enumeration of approaches to culling triangles from the scene.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue",
		Justification = "Enum maps to D3D11_CULL_MODE, so a zero-value makes no sense, unfortunately.")]
	public enum TriangleCullMode {
		/// <summary>
		/// Cull all triangles that face away from the camera. This is the default option.
		/// </summary>
		BackfaceCulling = 3,
		/// <summary>
		/// Cull all triangles that face towards the camera.
		/// </summary>
		FrontfaceCulling = 2,
		/// <summary>
		/// Do not cull triangles.
		/// </summary>
		NoCulling = 1
	}
}