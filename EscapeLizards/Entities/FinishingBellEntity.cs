// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 16 07 2015 at 01:07 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using Ophidian.Losgap;
using Ophidian.Losgap.Entities;

namespace Egodystonic.EscapeLizards {
	public sealed class FinishingBellEntity : GroundableEntity {
		public FinishingBellEntity() : this(Transform.DEFAULT_TRANSFORM) { }

		public FinishingBellEntity(Transform initialTransform) : base(initialTransform) { }
	}
}