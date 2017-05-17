// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 17 08 2015 at 11:09 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using Ophidian.Losgap;
using Ophidian.Losgap.Entities;
using Ophidian.Losgap.Rendering;

namespace Egodystonic.EscapeLizards {
	public sealed class StarEntity : Entity {
		private readonly Vector3 rotatedUpAxis;
		private readonly bool autoRotate;

		public StarEntity(Star star, Quaternion upAxisRotation, bool autoRotate = true) {
			switch (star) {
				case Star.Gold:
					SetModelInstance(AssetLocator.GameLayer, AssetLocator.StarModel, AssetLocator.StarMaterialGold);
					SetScale(Vector3.ONE * 2f);
					break;
				case Star.Silver:
					SetModelInstance(AssetLocator.GameLayer, AssetLocator.StarModel, AssetLocator.StarMaterialSilver);
					SetScale(Vector3.ONE * 2f);
					break;
				case Star.Bronze:
					SetModelInstance(AssetLocator.GameLayer, AssetLocator.StarModel, AssetLocator.StarMaterialBronze);
					SetScale(Vector3.ONE * 2f);
					break;
			}

			this.rotatedUpAxis = Vector3.UP * upAxisRotation;
			this.autoRotate = autoRotate;
			SetRotation(upAxisRotation);
		}

		protected override void Tick(float deltaTimeSeconds) {
			base.Tick(deltaTimeSeconds);

			if (!autoRotate) return;

			RotateBy(GameplayConstants.STAR_SPIN * deltaTimeSeconds, rotatedUpAxis);
		}
	}
}