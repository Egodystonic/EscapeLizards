// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 15 05 2015 at 15:45 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Linq;
using Ophidian.Losgap;
using Ophidian.Losgap.Entities;
using Ophidian.Losgap.Rendering;

namespace Egodystonic.EscapeLizards {
	public sealed class SkyLevelDescription : LevelDescription {
		private const string SKY_LIGHTS_ELEMENT_NAME = "skyLights";
		private const float SKY_SCALE = 300.0f;
		private readonly List<LevelSkyLight> skyLights = new List<LevelSkyLight>();
		private readonly Dictionary<LevelSkyLight, Light> activeSkyLights = new Dictionary<LevelSkyLight, Light>();

		public IReadOnlyList<LevelSkyLight> SkyLights {
			get {
				lock (instanceMutationLock) {
					return skyLights.AsReadOnly();
				}
			}
		}

		public SkyLevelDescription(string title) : base(title) { }

		public LevelSkyLight AddSkyLight(Vector3 position) {
			lock (instanceMutationLock) {
				LevelSkyLight newLight = new LevelSkyLight(position, Vector3.ONE, PhysicsManager.ONE_METRE_SCALED * 0.15f, 1.0f);
				skyLights.Add(newLight);
				ResetSkyLight(newLight);
				return newLight;
			}
		}

		public void RemoveSkyLight(LevelSkyLight light) {
			lock (instanceMutationLock) {
				if (!skyLights.Contains(light)) throw new KeyNotFoundException("Given light is not added to this skylevel.");
				skyLights.Remove(light);
				ResetSkyLight(light);
			}
		}

		public void ReplaceSkyLight(LevelSkyLight oldLight, LevelSkyLight newLight) {
			lock (instanceMutationLock) {
				skyLights.Replace(oldLight, newLight);
				ResetSkyLight(oldLight);
				ResetSkyLight(newLight);
			}
		}

		public void AdjustSkyboxAccordingToDrift(Vector3 driftDelta) {
			driftDelta /= SKY_SCALE;
			lock (instanceMutationLock) {
				foreach (Light activeSkyLight in activeSkyLights.Values) {
					activeSkyLight.Position += driftDelta;
				}

				foreach (GeometryEntity activeGeometryEntity in currentGeometryEntities.Values) {
					activeGeometryEntity.TranslateBy(driftDelta);
				}
			}
		}

		protected override void SerializeMeta(XElement metaElement) {
			base.SerializeMeta(metaElement);
			metaElement.Add(new XElement(FILE_ELEMENT_NAME_META_TYPE, FILE_ELEMENT_VALUE_META_TYPE_SKY));
		}

		protected override FragmentShader GetRecommendedFragmentShader() {
			return AssetLocator.SkyGeometryFragmentShader;
		}

		protected override SceneLayer GetRecommendedSceneLayer() {
			return AssetLocator.SkyLayer;
		}

		protected override VertexShader GetRecommendedVertexShader() {
			return AssetLocator.SkyVertexShader;
		}

		public override void ReinitializeAll() {
			base.ReinitializeAll();
			ResetSkyLights();
		}

		public void ResetSkyLights() {
			lock (instanceMutationLock) {
				foreach (LevelSkyLight levelSkyLight in skyLights) {
					ResetSkyLight(levelSkyLight);
				}
			}
		}

		public void ResetSkyLight(LevelSkyLight lsl) {
			using (RenderingModule.RenderStateBarrier.AcquirePermit(withLock: instanceMutationLock)) {
				if (activeSkyLights.ContainsKey(lsl)) {
					AssetLocator.LightPass.RemoveLight(activeSkyLights.Pop(lsl));
				}
				if (skyLights.Contains(lsl)) {
					activeSkyLights.Add(lsl, new Light(lsl.Position, lsl.Radius * SKY_SCALE, lsl.Color * lsl.LuminanceMultiplier));
					AssetLocator.LightPass.AddLight(activeSkyLights[lsl]);	
				}
			}
		}

		public override void ResetEntityPhysics(bool fullReset) {
			// Do nothing
		}

		protected override void ChildSerialize(XDocument root) {
			XElement skyLightsElement = new XElement(SKY_LIGHTS_ELEMENT_NAME);
			foreach (LevelSkyLight levelSkyLight in skyLights) {
				skyLightsElement.Add(levelSkyLight.Serialize());
			}
			root.Root.Add(skyLightsElement);
		}

		protected override void ChildDeserialize(XDocument root) {
			skyLights.Clear();
			foreach (XElement skyLightElement in root.Root.Element(SKY_LIGHTS_ELEMENT_NAME).Elements()) {
				skyLights.Add(LevelSkyLight.Deserialize(skyLightElement));
			}
		}

		public override void Dispose() {
			base.Dispose();
			lock (instanceMutationLock) {
				activeSkyLights.Values.ForEach(AssetLocator.LightPass.RemoveLight);
			}
		}
	}
}