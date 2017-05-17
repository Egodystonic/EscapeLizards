// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 21 07 2015 at 15:24 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using Ophidian.Losgap;
using Ophidian.Losgap.Entities;
using Ophidian.Losgap.Rendering;

namespace Egodystonic.EscapeLizards {
	public sealed class IntroCameraAttracterEntity : GroundableEntity {
		public IntroCameraAttracterEntity() : this(Transform.DEFAULT_TRANSFORM) { }

		public IntroCameraAttracterEntity(Transform groundedTransform)
			: base(groundedTransform) {
		}
	}
}