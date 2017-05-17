// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 27 10 2014 at 11:04 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;

namespace Ophidian.Losgap.Rendering {
	[Flags]
	internal enum PipelineBindings {
		VertexBuffer = 0x1,
		IndexBuffer = 0x2,
		ConstantBuffer = 0x4,
		ShaderResource = 0x8,
		StreamOutput = 0x10,
		RenderTarget = 0x20,
		DepthStencil = 0x40,
		UnorderedAccess = 0x80,
		Decoder = 0x200,
		VideoEncoder = 0x400
	}
}