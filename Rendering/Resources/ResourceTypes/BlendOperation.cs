// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 09 03 2015 at 15:27 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;

namespace Ophidian.Losgap.Rendering {
	public enum BlendOperation {
		None,
		Additive,
		Subtractive,
		ReverseSubtractive,
		Minimal,
		Maximal,
		AlphaAwareSrcOverride = Additive | 0x10000 // Acts like "None" when src pixel alpha is 1.0, or blends src + dest for any other value
	}
}