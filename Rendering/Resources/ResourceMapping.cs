// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 27 10 2014 at 17:42 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;

namespace Ophidian.Losgap.Rendering {
	internal enum ResourceMapping {
		Read = 1,
		Write = 2,
		ReadWrite = 3,
		WriteDiscard = 4,
		WriteNoOverwrite = 5
	}
}