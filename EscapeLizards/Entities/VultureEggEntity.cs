// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 16 07 2015 at 01:07 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using Ophidian.Losgap;
using Ophidian.Losgap.Entities;
using Ophidian.Losgap.Rendering;

namespace Egodystonic.EscapeLizards {
	public sealed class VultureEggEntity : GroundableEntity {
		internal ModelInstanceHandle _MI {
			get {
				return ModelInstance.Value;
			}
		}

		public VultureEggEntity() : this(Transform.DEFAULT_TRANSFORM) { }

		public VultureEggEntity(Transform initialTransform) : base(initialTransform) { }
	}
}