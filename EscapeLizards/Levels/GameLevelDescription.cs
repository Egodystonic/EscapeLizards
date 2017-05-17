// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 15 05 2015 at 15:45 by Ben Bowen

using System;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Ophidian.Losgap;
using Ophidian.Losgap.Entities;
using Ophidian.Losgap.Rendering;

namespace Egodystonic.EscapeLizards {
	public sealed class GameLevelDescription : LevelDescription {
		private const string FILE_ELEMENT_NAME_META_SKYBOX_FILE = "skybox";
		private const string FILE_ELEMENT_NAME_META_GO_ID_COUNTER = "goIDCounter";
		private const string FILE_ELEMENT_NAME_META_GOLD_TIME = "goldTime";
		private const string FILE_ELEMENT_NAME_META_SILVER_TIME = "silverTime";
		private const string FILE_ELEMENT_NAME_META_BRONZE_TIME = "bronzeTime";
		private const string FILE_ELEMENT_NAME_META_MAX_TIME = "maxTime";
		private const string FILE_ELEMENT_NAME_META_NUM_HOPS = "numHops";
		private const string FILE_ELEMENT_NAME_GAME_OBJECTS = "gameObjects";
		private readonly Dictionary<LevelGeometryEntity, PhysicsShapeHandle> activePhysicsShapes = new Dictionary<LevelGeometryEntity, PhysicsShapeHandle>();
		private readonly Dictionary<LevelGameObject, GroundableEntity> currentGameObjects = new Dictionary<LevelGameObject, GroundableEntity>();
		private readonly List<LevelGameObject> gameObjects = new List<LevelGameObject>();
		private string skyboxFileName;
		private int 
			levelTimerMaxMs = 60000, 
			levelTimerBronzeMs = 30000, 
			levelTimerSilverMs = 20000, 
			levelTimerGoldMs = 10000;
		private int numHops = 0;
		private int goIDCounter = 0;

		public string SkyboxFileName {
			get {
				lock (instanceMutationLock) {
					return skyboxFileName;
				}
			}
			set {
				lock (instanceMutationLock) {
					skyboxFileName = value;
				}
			}
		}

		public int LevelTimerMaxMs {
			get {
				lock (instanceMutationLock) {
					return levelTimerMaxMs;
				}
			}
			set {
				lock (instanceMutationLock) {
					levelTimerMaxMs = value;
				}
			}
		}

		public int LevelTimerBronzeMs {
			get {
				lock (instanceMutationLock) {
					return levelTimerBronzeMs;
				}
			}
			set {
				lock (instanceMutationLock) {
					levelTimerBronzeMs = value;
				}
			}
		}

		public int LevelTimerSilverMs {
			get {
				lock (instanceMutationLock) {
					return levelTimerSilverMs;
				}
			}
			set {
				lock (instanceMutationLock) {
					levelTimerSilverMs = value;
				}
			}
		}

		public int LevelTimerGoldMs {
			get {
				lock (instanceMutationLock) {
					return levelTimerGoldMs;
				}
			}
			set {
				lock (instanceMutationLock) {
					levelTimerGoldMs = value;
				}
			}
		}

		public int NumHops {
			get {
				lock (instanceMutationLock) {
					return numHops;
				}
			}
			set {
				lock (instanceMutationLock) {
					numHops = value;
				}
			}
		}

		public IReadOnlyList<LevelGameObject> GameObjects {
			get {
				lock (instanceMutationLock) {
					return new List<LevelGameObject>(gameObjects); // New list to make it threadsafe
				}
			}
		}

		public GameLevelDescription(string title) : base(title) { }

		protected override void ChildSerialize(XDocument root) {
			XElement gameObjectsElement = new XElement(FILE_ELEMENT_NAME_GAME_OBJECTS);
			root.Root.Add(gameObjectsElement);
			SerializeGameObjects(gameObjectsElement);
		}

		protected override void ChildDeserialize(XDocument root) {
			XElement gameObjectsElement = root.Root.Element(FILE_ELEMENT_NAME_GAME_OBJECTS);
			DeserializeGameObjects(gameObjectsElement);
		}

		protected override void SerializeMeta(XElement metaElement) {
			base.SerializeMeta(metaElement);
			metaElement.Add(new XElement(FILE_ELEMENT_NAME_META_TYPE, FILE_ELEMENT_VALUE_META_TYPE_GAME));
			metaElement.Add(new XElement(FILE_ELEMENT_NAME_META_SKYBOX_FILE, skyboxFileName ?? String.Empty));
			metaElement.Add(new XElement(FILE_ELEMENT_NAME_META_GO_ID_COUNTER, goIDCounter.ToString(CultureInfo.InvariantCulture)));
			metaElement.Add(new XElement(FILE_ELEMENT_NAME_META_GOLD_TIME, levelTimerGoldMs.ToString(CultureInfo.InvariantCulture)));
			metaElement.Add(new XElement(FILE_ELEMENT_NAME_META_SILVER_TIME, levelTimerSilverMs.ToString(CultureInfo.InvariantCulture)));
			metaElement.Add(new XElement(FILE_ELEMENT_NAME_META_BRONZE_TIME, levelTimerBronzeMs.ToString(CultureInfo.InvariantCulture)));
			metaElement.Add(new XElement(FILE_ELEMENT_NAME_META_MAX_TIME, levelTimerMaxMs.ToString(CultureInfo.InvariantCulture)));
			metaElement.Add(new XElement(FILE_ELEMENT_NAME_META_NUM_HOPS, numHops.ToString(CultureInfo.InvariantCulture)));
		}

		protected override void DeserializeMeta(XElement metaElement) {
			base.DeserializeMeta(metaElement);
			skyboxFileName = metaElement.Element(FILE_ELEMENT_NAME_META_SKYBOX_FILE).Value;
			if (skyboxFileName == String.Empty) skyboxFileName = null;
			goIDCounter = int.Parse(metaElement.Element(FILE_ELEMENT_NAME_META_GO_ID_COUNTER).Value, CultureInfo.InvariantCulture);
			levelTimerGoldMs = int.Parse(metaElement.Element(FILE_ELEMENT_NAME_META_GOLD_TIME).Value, CultureInfo.InvariantCulture);
			levelTimerSilverMs = int.Parse(metaElement.Element(FILE_ELEMENT_NAME_META_SILVER_TIME).Value, CultureInfo.InvariantCulture);
			levelTimerBronzeMs = int.Parse(metaElement.Element(FILE_ELEMENT_NAME_META_BRONZE_TIME).Value, CultureInfo.InvariantCulture);
			levelTimerMaxMs = int.Parse(metaElement.Element(FILE_ELEMENT_NAME_META_MAX_TIME).Value, CultureInfo.InvariantCulture);
			var numHopsElement = metaElement.Element(FILE_ELEMENT_NAME_META_NUM_HOPS);
			numHops = numHopsElement == null ? 0 : int.Parse(metaElement.Element(FILE_ELEMENT_NAME_META_NUM_HOPS).Value, CultureInfo.InvariantCulture);
		}

		private void SerializeGameObjects(XElement gameObjectsElement) {
			foreach (LevelGameObject levelGameObject in gameObjects) {
				gameObjectsElement.Add(levelGameObject.Serialize());
			}
		}

		private void DeserializeGameObjects(XElement gameObjectsElement) {
			gameObjects.Clear();
			foreach (XElement element in gameObjectsElement.Elements()) {
				gameObjects.Add(LevelGameObject.Deserialize(this, element));
			}
			if (!GameObjects.Any(go => go is Shadowcaster)) {
				var shadowcaster = new Shadowcaster(this, GetNewGameObjectID());
				shadowcaster.Transform = new Transform(
					Vector3.ONE,
					Quaternion.FromVectorTransition(Vector3.UP, Vector3.DOWN),
					Vector3.UP * PhysicsManager.ONE_METRE_SCALED * 20f
				);
				gameObjects.Insert(0, shadowcaster);
			}
		}

		protected override FragmentShader GetRecommendedFragmentShader() {
			return AssetLocator.GeometryFragmentShader;
		}

		protected override SceneLayer GetRecommendedSceneLayer() {
			return AssetLocator.GameLayer;
		}

		protected override VertexShader GetRecommendedVertexShader() {
			return AssetLocator.MainGeometryVertexShader;
		}

		public override void ResetEntityPhysics(bool fullReset) {
			LosgapSystem.InvokeOnMasterAsync(() => { // Because creating geometryEntity shapes invoke on master and it can create locking deadlocks
				Vector3 physicsShapeOffset;
				lock (instanceMutationLock) {
					if (fullReset) {
						foreach (PhysicsShapeHandle shapeHandle in activePhysicsShapes.Values.Distinct()) {
							if (!WorldModelCache.CacheContainsHandle(shapeHandle)) shapeHandle.Dispose();
						}
						activePhysicsShapes.Clear();
					}
					List<PhysicsShapeHandle> unusedHandles = activePhysicsShapes.Values.Distinct().ToList();
					foreach (KeyValuePair<LevelGeometryEntity, GeometryEntity> kvp in currentGeometryEntities) {
						PhysicsShapeHandle physicsShape = PhysicsShapeHandle.NULL;
						if (kvp.Key.Geometry is LevelGeometry_Model) {
							LevelGeometry_Model geomAsModel = (LevelGeometry_Model)kvp.Key.Geometry;
							var matchingEntity = activePhysicsShapes.Keys.FirstOrDefault(e => e.Geometry is LevelGeometry_Model && ((LevelGeometry_Model)e.Geometry).HasIdenticalPhysicsShape(geomAsModel));
							if (matchingEntity != null) physicsShape = activePhysicsShapes[matchingEntity];
						}
						else {
							if (activePhysicsShapes.ContainsKey(kvp.Key)) physicsShape = activePhysicsShapes[kvp.Key];
						}
						if (physicsShape == PhysicsShapeHandle.NULL) physicsShape = kvp.Key.Geometry.CreatePhysicsShape(out physicsShapeOffset);
						if (physicsShape == PhysicsShapeHandle.NULL) continue; // Some geometry is non-collidable (i.e. planes)
						kvp.Value.SetPhysicsShape(physicsShape, Vector3.ZERO, LevelGeometry.GEOMETRY_MASS, forceIntransigence: true);
						activePhysicsShapes[kvp.Key] = physicsShape;
						if (!fullReset && unusedHandles.Contains(physicsShape)) unusedHandles.Remove(physicsShape);
					}

					if (!fullReset) {
						foreach (var unusedHandle in unusedHandles) {
							activePhysicsShapes.RemoveWhere(kvp => kvp.Value == unusedHandle);
							if (!WorldModelCache.CacheContainsHandle(unusedHandle)) unusedHandle.Dispose();
						}
					}
				}
			});
		}

		public override void Dispose() {
			base.Dispose();
			lock (instanceMutationLock) {
				foreach (PhysicsShapeHandle shapeHandle in activePhysicsShapes.Values.Distinct()) {
					if (!WorldModelCache.CacheContainsHandle(shapeHandle)) shapeHandle.Dispose();
				}
				activePhysicsShapes.Clear();
				currentGameObjects.Values.ForEach(e => e.Dispose());
				currentGameObjects.Clear();
			}
		}

		public override void ResetEntity(LevelGeometryEntity levelGeometryEntity, Material overrideMaterial = null) {
			base.ResetEntity(levelGeometryEntity, overrideMaterial);
			Entity rep = GetEntityRepresentation(levelGeometryEntity);
			PhysicsShapeHandle physicsShape = activePhysicsShapes[levelGeometryEntity];
			if (physicsShape == PhysicsShapeHandle.NULL) return; // Some geometry is non-collidable (i.e. planes)
			rep.SetPhysicsShape(physicsShape, Vector3.ZERO, LevelGeometry.GEOMETRY_MASS, forceIntransigence: true);

			if (rep is PresetMovementEntity) {
				foreach (LevelGameObject levelGameObject in GameObjects.Where(go => go.GroundingGeometryEntity == levelGeometryEntity)) {
					GroundableEntity goRep = GetGameObjectRepresentation(levelGameObject);
					goRep.Ground((PresetMovementEntity) rep);
				}
			}
		}

		#region Game Objects
		public void AddGameObject(LevelGameObject gameObject) {
			Assure.NotNull(gameObject);
			lock (instanceMutationLock) {
				gameObjects.Add(gameObject);
			}
		}

		public void DeleteGameObject(LevelGameObject gameObject) {
			Assure.NotNull(gameObject);
			lock (instanceMutationLock) {
				gameObjects.Remove(gameObject);
			}
		}

		public LevelGameObject GetGameObjectByID(int id) {
			lock (instanceMutationLock) {
				return gameObjects.Single(lgo => lgo.ID == id);
			}
		}

		public IEnumerable<T> GetGameObjectsByType<T>() where T : LevelGameObject {
			lock (instanceMutationLock) {
				return gameObjects.OfType<T>();
			}
		}

		public int GetNewGameObjectID() {
			lock (instanceMutationLock) {
				return goIDCounter++;
			}
		}

		public GroundableEntity GetGameObjectRepresentation(LevelGameObject lgo) {
			lock (instanceMutationLock) {
				return currentGameObjects[lgo];
			}
		}

		public void ResetGameObjects() {
			currentGameObjects.Values.ForEach(e => e.Dispose());
			currentGameObjects.Clear();
			foreach (LevelGameObject levelGameObject in gameObjects) {
				currentGameObjects.Add(levelGameObject, levelGameObject.CreateEntity());
			}
		}

		public void ResetGameObject(LevelGameObject lgo) {
			if (currentGameObjects.ContainsKey(lgo)) currentGameObjects[lgo].Dispose();
			currentGameObjects[lgo] = lgo.CreateEntity();
		}

		public override void ReinitializeAll() {
			base.ReinitializeAll();
			ResetGameObjects();
		}
		#endregion

		#region Calculations
		private Vector3 cameraAttracterPosition;
		private Vector3 startFlagPosition;
		private Vector3 startFlagOrientation;
		private Vector3 gameplayStartCameraPos;
		private Vector3 gameplayStartCameraOrientation;
		private Entity[] tiltableEntities;
		private GroundableEntity[] groundableEntities;
		private Vector3 gameZoneRadii;

		public void PerformCalculations() {
			lock (instanceMutationLock) {
				cameraAttracterPosition = GetGameObjectsByType<IntroCameraAttracter>().Single().Transform.Translation;
				
				startFlagPosition = GetGameObjectsByType<StartFlag>().Single().Transform.Translation;

				startFlagOrientation = GetGameObjectsByType<StartFlag>().Single().Transform.Rotation * Vector3.BACKWARD;

				gameplayStartCameraPos = startFlagPosition
					- startFlagOrientation.WithLength(GameplayConstants.CAMERA_MIN_VELOCITY_BASED_DIST_TO_EGG + Config.CameraDistanceOffset)
					+ Vector3.UP * (GameplayConstants.CAMERA_HEIGHT_ADDITION + Config.CameraHeightOffset);

				gameplayStartCameraOrientation = (startFlagPosition - gameplayStartCameraPos).ToUnit();

				CalculateTiltables();

				var vertexDict = new Dictionary<LevelGeometry, IEnumerable<Vector3>>();
				List<DefaultVertex> outVertices;
				List<uint> outIndices;
				foreach (LevelGeometry geom in Geometry) {
					if (precalculatedTriangles.ContainsKey(geom.ID)) {
						outVertices = precalculatedTriangles[geom.ID].Item1;
					}
					else {
						geom.GetVertexData(out outVertices, out outIndices);
					}
					vertexDict.Add(geom, outVertices.Select(v => v.Position));
				}

				gameZoneRadii = Vector3.ZERO;
				foreach (LevelGeometryEntity levelGeometryEntity in levelGeometryEntities) {
					IEnumerable<Vector3> geomVertices = vertexDict[levelGeometryEntity.Geometry];
					foreach (LevelEntityMovementStep movementStep in levelGeometryEntity.MovementSteps) {
						gameZoneRadii = new Vector3(
							Math.Max(gameZoneRadii.X, geomVertices.Max(vert => Math.Abs((movementStep.Transform * vert).X - cameraAttracterPosition.X))),
							Math.Max(gameZoneRadii.Y, geomVertices.Max(vert => Math.Abs((movementStep.Transform * vert).Y - cameraAttracterPosition.Y))),
							Math.Max(gameZoneRadii.Z, geomVertices.Max(vert => Math.Abs((movementStep.Transform * vert).Z - cameraAttracterPosition.Z)))
						);
					}
				}
			}
		}

		public void CalculateTiltables() {
			lock (instanceMutationLock) {
				tiltableEntities = currentGeometryEntities.Values.Cast<Entity>().Concat(currentGameObjects.Values).ToArray();
				groundableEntities = tiltableEntities.OfType<GroundableEntity>().ToArray();
			}
		}

		public Vector3 CameraAttracterPosition {
			get {
				lock (instanceMutationLock) {
					return cameraAttracterPosition;
				}
			}
		}

		public Vector3 StartFlagPosition {
			get {
				lock (instanceMutationLock) {
					return startFlagPosition;
				}
			}
		}

		public Vector3 StartFlagOrientation {
			get {
				lock (instanceMutationLock) {
					return startFlagOrientation;
				}
			}
		}

		public Vector3 GameplayStartCameraPos {
			get {
				lock (instanceMutationLock) {
					return gameplayStartCameraPos;
				}
			}
		}

		public Vector3 GameplayStartCameraOrientation {
			get {
				lock (instanceMutationLock) {
					return gameplayStartCameraOrientation;
				}
			}
		}

		public Entity[] TiltableEntities {
			get {
				lock (instanceMutationLock) {
					return tiltableEntities;
				}
			}
		}

		public GroundableEntity[] GroundableEntities {
			get {
				lock (instanceMutationLock) {
					return groundableEntities;
				}
			}
		}

		public Vector3 GameZoneRadii {
			get {
				lock (instanceMutationLock) {
					return gameZoneRadii;
				}
			}
		}
		#endregion


		public override void DeleteEntity(LevelGeometryEntity geometryEntity) {
			lock (instanceMutationLock) {
				var matchingLGOs = gameObjects.Where(lgo => lgo.GroundingGeometryEntity == geometryEntity);
				foreach (LevelGameObject matchingLgO in matchingLGOs) {
					matchingLgO.GroundingGeometryEntity = null;
				}
				base.DeleteEntity(geometryEntity);
				foreach (LevelGameObject matchingLgO in matchingLGOs) {
					ResetGameObject(matchingLgO);
				}
			}
		}
	}
}