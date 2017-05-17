// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 30 10 2014 at 15:22 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// An enumeration of possible places on the rendering pipeline that an <see cref="IResource"/> can be 'bound' to. These values
	/// may be combined as bitflags (e.g. <c>ReadableShaderResource | WritableShaderResource</c>).
	/// </summary>
	[Flags]
	public enum GPUBindings {
		/// <summary>
		/// No bindings.
		/// </summary>
		None = 0x0,
		/// <summary>
		/// Resource may be read by a shader program.
		/// </summary>
		ReadableShaderResource = 0x8,
		/// <summary>
		/// Resource may be written to by a shader program.
		/// </summary>
		WritableShaderResource = 0x80,
		/// <summary>
		/// Resource is a render target, and should accept output from the rendering pipeline (output merger).
		/// </summary>
		RenderTarget = 0x20,
		/// <summary>
		/// Resource is a depth/stencil target, and should accept output from the rendering pipeline (output merger).
		/// </summary>
		DepthStencilTarget = 0x40,
	}
}