// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 09 03 2015 at 15:27 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// An enumeration of functions that determine which fragments to keep after a depth-comparison is made.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue",
		Justification = "The enumeration maps to D3D11_COMPARISON_FUNC, which has no 0-value.")]
	public enum DepthComparisonFunction {
		/// <summary>
		/// Keep no fragments, no matter what their depth.
		/// </summary>
		PassNone = 1, // NEVER
		/// <summary>
		/// Keep the fragments nearest to the camera (this is the default).
		/// </summary>
		PassNearestElements, // LESS
		/// <summary>
		/// When two fragments have the same distance to the camera, keep the newest one.
		/// </summary>
		PassCoplanarElements, // EQUAL
		/// <summary>
		/// Combination of <see cref="PassNearestElements"/> and <see cref="PassCoplanarElements"/>.
		/// </summary>
		PassNearestOrCoplanarElements, // LESS_EQUAL
		/// <summary>
		/// Keep the fragments furthest from the camera.
		/// </summary>
		PassFurthestElements, // GREATER
		/// <summary>
		/// Keep the newest fragment in a comparison unless it has the same distance to the camera as the existing fragment.
		/// </summary>
		PassNonCoplanarElements, // NOT_EQUAL
		/// <summary>
		/// Combination of <see cref="PassFurthestElements"/> and <see cref="PassCoplanarElements"/>.
		/// </summary>
		PassFurthestOrCoplanarElements, // GREATER_EQUAL
		/// <summary>
		/// Always keep the newest fragments.
		/// </summary>
		PassAll // ALWAYS
	}
}