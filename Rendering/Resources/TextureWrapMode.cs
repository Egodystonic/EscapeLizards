// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 27 10 2014 at 11:24 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// An enumeration of actions that can be taken by a <see cref="TextureSampler"/> when a texture is sampled outside its bounds.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue",
		Justification = "Unfortunately, that would just be pushing the problem on the the native/D3D side: " +
			"Basically, there is no '0' value for D3D11_TEXTURE_ADDRESS_MODE.")]
	public enum TextureWrapMode {
		/// <summary>
		/// Indicates that the texture should simply repeat (similar to doing a modulus operation) across the boundary.
		/// </summary>
		Wrap = 1,
		/// <summary>
		/// Indicates that the texture sampler should sample from the 'mirrored' co-ordinate across the boundary.
		/// </summary>
		Mirror = 2,
		/// <summary>
		/// Indicates that all sample values should be clamped to the texture bounds.
		/// </summary>
		Clamp = 3,
		/// <summary>
		/// Indicates that all out-of-boundary samples should be coloured in a universal colour. The colour can be specified
		/// in the <see cref="TextureSampler"/> with the <see cref="TextureSampler.BorderColor"/> field.
		/// </summary>
		Border = 4,
		/// <summary>
		/// Mirrors the co-ordinates once, (see <see cref="Mirror"/>), but clamps them if they exceed the boundary in any dimension
		/// by double.
		/// </summary>
		MirrorOnce = 5
	}
}