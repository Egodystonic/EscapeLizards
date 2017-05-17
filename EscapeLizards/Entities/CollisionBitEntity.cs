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
	public sealed class CollisionBitEntity : Entity {
		private static readonly Vector3 SCALE = Vector3.ONE * 0.45f;
		private const float LIGHT_RADIUS = PhysicsManager.ONE_METRE_SCALED * 1.085f;
		private const float LIGHT_COLOR_MOD = 0.1f;
		private readonly Light attachedLight;

		public CollisionBitEntity() {
			int bitIndex = RandomProvider.Next(0, AssetLocator.SillyBitsHandles.Length);
			SetModelInstance(AssetLocator.GameLayer, AssetLocator.SillyBitsHandles[bitIndex], AssetLocator.SillyBitsMaterials[bitIndex]);
			SetPhysicsShape(
				AssetLocator.SillyBitsPhysicsShapes[bitIndex],
				Vector3.ZERO,
				0.1f,
				restitution: 0.4f,
				friction: 0.2f,
				rollingFriction: 0.2f,
				linearDamping: 0.2f,
				angularDamping: 0.2f,
				collideOnlyWithWorld: true
			);

			attachedLight = new Light(Vector3.ZERO, LIGHT_RADIUS, Config.SillyBitsIlluminationColors[bitIndex] * LIGHT_COLOR_MOD);

			Reveal();
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

		public void Reveal() {
			lock (InstanceMutationLock) {
				if (Config.DynamicLightCap == null) AssetLocator.LightPass.AddLight(attachedLight);
				SetGravity(GameCoordinator.BoardDownDir.WithLength(GameplayConstants.GRAVITY_ACCELERATION));
				SetScale(SCALE);
				AngularVelocity = new Vector3(
					RandomProvider.Next(-MathUtils.PI, MathUtils.PI),
					RandomProvider.Next(-MathUtils.PI, MathUtils.PI),
					RandomProvider.Next(-MathUtils.PI, MathUtils.PI)
				);
			}
		}

		public void Hide() {
			lock (InstanceMutationLock) {
				AssetLocator.LightPass.RemoveLight(attachedLight);
				SetGravity(Vector3.ZERO);
				SetScale(Vector3.ZERO);
				RemoveAllForceAndTorque();
			}
		}
	}
}