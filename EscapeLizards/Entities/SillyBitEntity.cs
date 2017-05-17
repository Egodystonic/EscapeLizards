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
	public sealed class SillyBitEntity : Entity {
		private readonly Light attachedLight;
		public readonly int BitIndex;

		public SillyBitEntity(int bitIndex) {
			BitIndex = bitIndex;
			if (bitIndex < 0 || bitIndex >= AssetLocator.SillyBitsHandles.Length) {
				var materialArr = Enum.GetValues(typeof(LizardEggMaterial));
				var material = (LizardEggMaterial) materialArr.GetValue(RandomProvider.Next(0, materialArr.GetLength(0)));
				SetModelInstance(AssetLocator.GameLayer, AssetLocator.LizardEggModels[0], material.Material());
				SetScale(Vector3.ONE * 0.6f);
				SetPhysicsShape(
					AssetLocator.LizardEggPhysicsShape,
					Vector3.ZERO,
					0.1f,
					restitution: 0.5f,
					friction: 0.2f,
					rollingFriction: 0.2f,
					linearDamping: 0.2f,
					angularDamping: 0.2f
				);

				attachedLight = new Light(Vector3.ZERO, Config.SillyBitsIlluminationRadius, material.CameraIlluminationColor());
			}
			else {
				SetModelInstance(AssetLocator.GameLayer, AssetLocator.SillyBitsHandles[bitIndex], AssetLocator.SillyBitsMaterials[bitIndex]);
				SetPhysicsShape(
					AssetLocator.SillyBitsPhysicsShapes[bitIndex],
					Vector3.ZERO,
					0.1f,
					restitution: 1f,
					friction: 0.2f,
					rollingFriction: 0.2f,
					linearDamping: 0.2f,
					angularDamping: 0.2f
				);

				attachedLight = new Light(Vector3.ZERO, Config.SillyBitsIlluminationRadius, Config.SillyBitsIlluminationColors[bitIndex]);
			}
			
			//EnableContinuousCollisionDetection(GameplayConstants.MIN_SPEED_FOR_EGG_CCD, GameplayConstants.EGG_COLLISION_RADIUS * 0.01f);

			if (Config.DynamicLightCap == null) AssetLocator.LightPass.AddLight(attachedLight);
		}

		public void SetLightRadius(float newRadius) {
			attachedLight.Radius = newRadius;
		}

		protected override void Tick(float deltaTimeSeconds) {
			base.Tick(deltaTimeSeconds);
			attachedLight.Position = Transform.Translation;
		}

		public override void Dispose() {
			lock (InstanceMutationLock) {
				if (IsDisposed) return;
				base.Dispose();
				AssetLocator.LightPass.RemoveLight(attachedLight);
			}
		}

	}
}