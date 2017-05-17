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
	public sealed class DynamicLightEntity : GroundableEntity {
		public readonly Light AttachedLight;

		public DynamicLightEntity(Transform groundedTransform, float lightRadius, Vector3 lightColor, float luminanceMultiplier)
			: base(groundedTransform) {
			AttachedLight = new Light(Vector3.ZERO, lightRadius, lightColor * luminanceMultiplier);
			using (RenderingModule.RenderStateBarrier.AcquirePermit()) {
				AssetLocator.LightPass.AddLight(AttachedLight);	
			}
		}

		protected override void Tick(float deltaTimeSeconds) {
			base.Tick(deltaTimeSeconds);
			AttachedLight.Position = Transform.Translation;
		}

		public override void Dispose() {
			base.Dispose();
			using (RenderingModule.RenderStateBarrier.AcquirePermit(withLock: InstanceMutationLock)) {
				AssetLocator.LightPass.RemoveLight(AttachedLight);
			}
		}

		public override void RealignGrounding() {
			if (!IsGrounded || !(GroundingEntity is PresetMovementEntity)) return;
			base.RealignGrounding();
		}

		public override void MoveGroundedMassless(Vector3 translation, Quaternion rot) {
			if (!IsGrounded || !(GroundingEntity is PresetMovementEntity)) return;
			base.MoveGroundedMassless(translation, rot);
		}
	}
}