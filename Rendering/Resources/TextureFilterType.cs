// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 27 10 2014 at 11:20 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// An enumeration of filtering options that can be applied to texture sampling operations.
	/// </summary>
	public enum TextureFilterType {
		/// <summary>
		/// No filtering applied. <see cref="TextureSampler"/>s using this filter type will simply map the requested texture sample
		/// straight on to the projected surface. This is the most performant option.
		/// </summary>
		None = 0,
		/// <summary>
		/// Applies a spatial filter that uses an averaging technique to apply antialiasing to texture samples, according to distance
		/// from the camera. This is the most performant filtering option, other than <see cref="None"/>.
		/// </summary>
		Bilinear = 0x14,
		/// <summary>
		/// Applies a similar algorithm to <see cref="Bilinear"/> filtering, but with additional averaging done between mip levels as the
		/// camera distance changes, to prevent aliasing 'jumps'. Requires more processing compared to <see cref="Bilinear"/>.
		/// </summary>
		Trilinear = 0x15,
		/// <summary>
		/// The most performance-heavy option. Uses a complex algorithm to more accurately represent textures than all other filtering
		/// options; especially those mapped to surfaces at near-parallel angles with respect to the camera.
		/// </summary>
		Anisotropic = 0x55,

		MinMagMipLinear = 0x15,
		MinMagMipPoint = 0x0,

	}
}