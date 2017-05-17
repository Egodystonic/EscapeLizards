// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 16 07 2015 at 01:07 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using Ophidian.Losgap;
using Ophidian.Losgap.Entities;

namespace Egodystonic.EscapeLizards {
	public sealed class ShadowcasterEntity : GroundableEntity {
		public ShadowcasterEntity() : this(Transform.DEFAULT_TRANSFORM, Shadowcaster.DEFAULT_ORTHOGRAPHIC_DIMENSIONS) { }

		public ShadowcasterEntity(Transform initialTransform, Vector3 orthographicDimensions) : base(initialTransform) {
			AssetLocator.ShadowcasterCamera.OrthographicDimensions = orthographicDimensions;
		}

		protected override void Tick(float deltaTimeSeconds) {
			base.Tick(deltaTimeSeconds);
			AssetLocator.ShadowcasterCamera.Position = Transform.Translation;
			AssetLocator.ShadowcasterCamera.Orient(Vector3.UP * Transform.Rotation, Vector3.BACKWARD * Transform.Rotation);
		}
	}
}