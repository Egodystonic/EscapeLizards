// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 14 05 2015 at 16:02 by Ben Bowen

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Xml.Linq;
using Ophidian.Losgap;
using Ophidian.Losgap.Entities;
using Ophidian.Losgap.Rendering;

namespace Egodystonic.EscapeLizards {
	public abstract class LevelDescription : IDisposable {
		protected const string FILE_ELEMENT_NAME_META_TYPE = "type";
		protected const string FILE_ELEMENT_VALUE_META_TYPE_GAME = "game";
		protected const string FILE_ELEMENT_VALUE_META_TYPE_SKY = "sky";
		private const string FILE_ELEMENT_NAME_ROOT = "level";
		private const string FILE_ELEMENT_NAME_META = "meta";
		private const string FILE_ELEMENT_NAME_META_TITLE = "title";
		private const string FILE_ELEMENT_NAME_META_GEOM_ID_COUNTER = "geomIDCounter";
		private const string FILE_ELEMENT_NAME_META_MAT_ID_COUNTER = "matIDCounter";
		private const string FILE_ELEMENT_NAME_META_ENTITY_ID_COUNTER = "entityIDCounter";
		private const string FILE_ELEMENT_NAME_GEOMETRY = "geometry";
		private const string FILE_ELEMENT_NAME_MATERIALS = "materials";
		private const string FILE_ELEMENT_NAME_GEOM_ENTITIES = "geomEntities";

		protected readonly object instanceMutationLock = new object();
		protected readonly List<LevelGeometry> geometry = new List<LevelGeometry>();
		protected readonly List<LevelMaterial> materials = new List<LevelMaterial>();
		protected readonly List<LevelGeometryEntity> levelGeometryEntities = new List<LevelGeometryEntity>();
		protected string title;
		protected int geomIDCounter = 0;
		protected int matIDCounter = 0;
		protected int entityIDCounter = 0;

		protected GeometryCache currentCache;
		private GeometryCache currentSkydomeCache;
		private bool cacheInvalid = true;
		private bool materialsOutOfDate = true;
		protected readonly Dictionary<LevelGeometry, ModelHandle> currentModelHandles = new Dictionary<LevelGeometry, ModelHandle>();
		protected readonly Dictionary<string, ModelHandle> cachedDefaultModelHandles = new Dictionary<string, ModelHandle>();
		protected readonly Dictionary<LevelMaterial, Material> currentMaterials = new Dictionary<LevelMaterial, Material>();
		protected readonly Dictionary<string, ShaderResourceView> textureViews = new Dictionary<string, ShaderResourceView>();
		protected readonly Dictionary<LevelGeometryEntity, GeometryEntity> currentGeometryEntities = new Dictionary<LevelGeometryEntity, GeometryEntity>();	

		protected readonly Dictionary<int, Tuple<List<DefaultVertex>, List<uint>>> precalculatedTriangles = new Dictionary<int, Tuple<List<DefaultVertex>, List<uint>>>();

		public string Title {
			get {
				lock (instanceMutationLock) {
					return title;
				}
			}
			set {
				lock (instanceMutationLock) {
					title = value;
				}
			}
		}

		public IReadOnlyList<LevelGeometry> Geometry {
			get {
				lock (instanceMutationLock) {
					return new List<LevelGeometry>(geometry); // New list to make it threadsafe
				}
			}
		}

		public IReadOnlyList<LevelMaterial> Materials {
			get {
				lock (instanceMutationLock) {
					return new List<LevelMaterial>(materials); // New list to make it threadsafe
				}
			}
		}

		public IReadOnlyList<LevelGeometryEntity> LevelEntities {
			get {
				lock (instanceMutationLock) {
					return new List<LevelGeometryEntity>(levelGeometryEntities); // New list to make it threadsafe
				}
			}
		}

		public IEnumerable<GeometryEntity> CurrentGeometryEntities {
			get {
				lock (instanceMutationLock) {
					return currentGeometryEntities.Values;
				}
			}
		}

		protected LevelDescription(string title) {
			this.title = title;
		}

		public static LevelDescription Load(string fullFilePath, bool includePTD = true) {
			if (!IOUtils.IsValidFilePath(fullFilePath)) throw new ArgumentException("File path is not valid.", "fullFilePath");
			XDocument loadFile = XDocument.Load(fullFilePath);
			XElement metaElement = loadFile.Root.Element(FILE_ELEMENT_NAME_META);
			XElement geometryElement = loadFile.Root.Element(FILE_ELEMENT_NAME_GEOMETRY);
			XElement materialsElement = loadFile.Root.Element(FILE_ELEMENT_NAME_MATERIALS);
			XElement entitiesElement = loadFile.Root.Element(FILE_ELEMENT_NAME_GEOM_ENTITIES);
			LevelDescription loadedLevel;
			string fileType = metaElement.Element(FILE_ELEMENT_NAME_META_TYPE).Value;
			string title = metaElement.Element(FILE_ELEMENT_NAME_META_TITLE).Value;
			if (fileType == FILE_ELEMENT_VALUE_META_TYPE_SKY) loadedLevel = new SkyLevelDescription(title);
			else loadedLevel = new GameLevelDescription(title);

			lock (loadedLevel.instanceMutationLock) {
				loadedLevel.DeserializeMeta(metaElement);
				loadedLevel.DeserializeGeometry(geometryElement);
				loadedLevel.DeserializeMaterials(materialsElement);
				loadedLevel.DeserializeGeomEntities(entitiesElement);
				loadedLevel.ChildDeserialize(loadFile);
				var fullPTDFilePath = Path.Combine(AssetLocator.LevelsDir, Path.GetFileNameWithoutExtension(fullFilePath)) + ".ptd";
				if (!File.Exists(fullPTDFilePath)) {
					if (!EntryPoint.InEditor && loadedLevel is GameLevelDescription) {
						Logger.Warn("No pretriangulation data found for level '" + fullFilePath + "' -> '" + fullPTDFilePath + "'. Loading time will be longer.");
					}
				}
				else if (includePTD) {
					GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
					GC.Collect(2, GCCollectionMode.Forced, true);

					using (FileStream fileStream = File.Open(fullPTDFilePath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
						BinaryReader br = new BinaryReader(fileStream);

						int geomCount = br.ReadInt32();

						for (int g = 0; g < geomCount; ++g) {
							int geomID = br.ReadInt32();
							int numVertices = br.ReadInt32();
							int numIndices = br.ReadInt32();
							List<DefaultVertex> vertexList = new List<DefaultVertex>(numVertices);
							List<uint> indexList = new List<uint>(numIndices);
							for (int i = 0; i < numVertices; ++i) {
								DefaultVertex vert = new DefaultVertex(
									position: new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()),
									normal: new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()),
									texUv: new Vector2(br.ReadSingle(), br.ReadSingle()),
									tangent: new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle())
								);
								vertexList.Add(vert);
							}
							for (int i = 0; i < numIndices; ++i) {
								indexList.Add(br.ReadUInt32());
							}

							loadedLevel.precalculatedTriangles.Add(geomID, Tuple.Create(vertexList, indexList));
						}
					}

					GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
					GC.Collect(2, GCCollectionMode.Forced, true);
				}
			}

			return loadedLevel;
		}

		public void Save(string fullFilePath) {
			if (!IOUtils.IsValidFilePath(fullFilePath)) throw new ArgumentException("File path is not valid.", "fullFilePath");
			XDocument saveFile = new XDocument(new XElement(FILE_ELEMENT_NAME_ROOT));
			XElement metaElement = new XElement(FILE_ELEMENT_NAME_META);
			saveFile.Root.Add(metaElement);
			XElement geometryElement = new XElement(FILE_ELEMENT_NAME_GEOMETRY);
			saveFile.Root.Add(geometryElement);
			XElement materialsElement = new XElement(FILE_ELEMENT_NAME_MATERIALS);
			saveFile.Root.Add(materialsElement);
			XElement entitiesElement = new XElement(FILE_ELEMENT_NAME_GEOM_ENTITIES);
			saveFile.Root.Add(entitiesElement);

			lock (instanceMutationLock) {
				SerializeMeta(metaElement);
				SerializeGeometry(geometryElement);
				SerializeMaterials(materialsElement);
				SerializeGeomEntities(entitiesElement);
				ChildSerialize(saveFile);
			}

			saveFile.Save(fullFilePath);
		}

		protected abstract void ChildSerialize(XDocument root);
		protected abstract void ChildDeserialize(XDocument root);

		protected virtual void SerializeMeta(XElement metaElement) {
			metaElement.Add(new XElement(FILE_ELEMENT_NAME_META_TITLE, title));
			metaElement.Add(new XElement(FILE_ELEMENT_NAME_META_GEOM_ID_COUNTER, geomIDCounter.ToString(CultureInfo.InvariantCulture)));
			metaElement.Add(new XElement(FILE_ELEMENT_NAME_META_MAT_ID_COUNTER, matIDCounter.ToString(CultureInfo.InvariantCulture)));
			metaElement.Add(new XElement(FILE_ELEMENT_NAME_META_ENTITY_ID_COUNTER, entityIDCounter.ToString(CultureInfo.InvariantCulture)));
		}

		protected virtual void SerializeGeometry(XElement geometryElement) {
			foreach (LevelGeometry levelGeometry in geometry) {
				geometryElement.Add(levelGeometry.Serialize());
			}
		}

		protected virtual void SerializeMaterials(XElement materialsElement) {
			foreach (LevelMaterial levelMaterial in materials) {
				materialsElement.Add(levelMaterial.Serialize());
			}
		}

		protected virtual void SerializeGeomEntities(XElement entitiesElement) {
			foreach (LevelGeometryEntity levelEntity in levelGeometryEntities) {
				entitiesElement.Add(levelEntity.Serialize());
			}
		}

		protected virtual void DeserializeMeta(XElement metaElement) {
			geomIDCounter = int.Parse(metaElement.Element(FILE_ELEMENT_NAME_META_GEOM_ID_COUNTER).Value, CultureInfo.InvariantCulture);
			matIDCounter = int.Parse(metaElement.Element(FILE_ELEMENT_NAME_META_MAT_ID_COUNTER).Value, CultureInfo.InvariantCulture);
			entityIDCounter = int.Parse(metaElement.Element(FILE_ELEMENT_NAME_META_ENTITY_ID_COUNTER).Value, CultureInfo.InvariantCulture);
		}

		protected virtual void DeserializeGeometry(XElement geometryElement) {
			geometry.Clear();
			foreach (XElement element in geometryElement.Elements()) {
				geometry.Add(LevelGeometry.Deserialize(element));
			}
		}

		protected virtual void DeserializeMaterials(XElement materialsElement) {
			materials.Clear();
			foreach (XElement element in materialsElement.Elements()) {
				materials.Add(new LevelMaterial(element));
			}
		}

		protected virtual void DeserializeGeomEntities(XElement entitiesElement) {
			levelGeometryEntities.Clear();
			foreach (XElement element in entitiesElement.Elements()) {
				levelGeometryEntities.Add(new LevelGeometryEntity(this, element));
			}
		}

		public ModelHandle GetModelHandleForGeometry(LevelGeometry geom) {
			lock (instanceMutationLock) {
				return currentModelHandles[geom];
			}
		}

		public Material GetLoadedMaterialForLevelMaterial(LevelMaterial levelMat) {
			lock (instanceMutationLock) {
				return currentMaterials[levelMat];
			}
		}

		public void FlushCaches() {
			lock (instanceMutationLock) {
				cacheInvalid = true;
				materialsOutOfDate = true;
			}
		}

		#region Geometry
		public void AddGeometry(LevelGeometry geometry) {
			Assure.NotNull(geometry);
			lock (instanceMutationLock) {
				this.geometry.Add(geometry);
				cacheInvalid = true;
			}
		}

		public void DeleteGeometry(LevelGeometry geometry) {
			Assure.NotNull(geometry);
			lock (instanceMutationLock) {
				Assure.True(this.geometry.Contains(geometry));
				levelGeometryEntities.RemoveWhere(e => e.Geometry == geometry);
				this.geometry.Remove(geometry);
			}
		}

		public void ReplaceGeometry(LevelGeometry existing, LevelGeometry replacement) {
			Assure.NotNull(existing);
			Assure.NotNull(replacement);
			lock (instanceMutationLock) {
				Assure.True(this.geometry.Contains(existing));
				levelGeometryEntities.Where(e => e.Geometry == existing).ForEach(e => e.Geometry = replacement);
				this.geometry.Replace(existing, replacement);
				cacheInvalid = true;
			}
		}

		public LevelGeometry GetGeometryByID(int id) {
			lock (instanceMutationLock) {
				for (int i = 0; i < geometry.Count; ++i) {
					if (geometry[i].ID == id) return geometry[i];
				}
			}
			throw new KeyNotFoundException("No level geom with given ID found.");
		}

		public int GetNewGeometryID() {
			lock (instanceMutationLock) {
				return geomIDCounter++;
			}
		}
		#endregion

		#region Materials
		public void AddMaterial(LevelMaterial material) {
			Assure.NotNull(material);
			lock (instanceMutationLock) {
				this.materials.Add(material);
				materialsOutOfDate = true;
			}
		}

		public void DeleteMaterial(LevelMaterial material) {
			Assure.NotNull(geometry);
			lock (instanceMutationLock) {
				Assure.True(this.materials.Contains(material));
				levelGeometryEntities.RemoveWhere(e => e.Material == material);
				this.materials.Remove(material);
			}
		}

		public void ReplaceMaterial(LevelMaterial existing, LevelMaterial replacement) {
			Assure.NotNull(existing);
			Assure.NotNull(replacement);
			lock (instanceMutationLock) {
				Assure.True(this.materials.Contains(existing));
				levelGeometryEntities.Where(e => e.Material == existing).ForEach(e => e.Material = replacement);
				this.materials.Replace(existing, replacement);
				materialsOutOfDate = true;
			}
		}

		public LevelMaterial GetMaterialByID(int id) {
			lock (instanceMutationLock) {
				return materials.Single(mat => mat.ID == id);
			}
		}

		public int GetNewMaterialID() {
			lock (instanceMutationLock) {
				return matIDCounter++;
			}
		}
		#endregion

		#region Entities
		public void AddEntity(LevelGeometryEntity geometryEntity) {
			Assure.NotNull(geometryEntity);
			lock (instanceMutationLock) {
				this.levelGeometryEntities.Add(geometryEntity);
			}
		}	

		public virtual void DeleteEntity(LevelGeometryEntity geometryEntity) {
			Assure.NotNull(geometry);
			lock (instanceMutationLock) {
				Assure.True(this.levelGeometryEntities.Contains(geometryEntity));
				this.levelGeometryEntities.Remove(geometryEntity);
			}
		}

		public void ReplaceEntity(LevelGeometryEntity existing, LevelGeometryEntity replacement) {
			Assure.NotNull(existing);
			Assure.NotNull(replacement);
			lock (instanceMutationLock) {
				Assure.True(this.levelGeometryEntities.Contains(existing));
				levelGeometryEntities.Replace(existing, replacement);
			}
		}

		public LevelGeometryEntity GetEntityByID(int id) {
			lock (instanceMutationLock) {
				return levelGeometryEntities.Single(entity => entity.ID == id);
			}
		}

		public int GetNewEntityID() {
			lock (instanceMutationLock) {
				return entityIDCounter++;
			}
		}

		public GeometryEntity GetEntityRepresentation(LevelGeometryEntity le) {
			lock (instanceMutationLock) {
				return currentGeometryEntities.ValueOrDefault(le);
			}
		}

		public LevelGeometryEntity GetLevelEntityRepresentation(GeometryEntity e) {
			lock (instanceMutationLock) {
				try {
					return currentGeometryEntities.First(kvp => kvp.Value == e).Key;
				}
				catch (InvalidOperationException) {
					return null;
				}
			}
		}
		#endregion

		#region Display
		public virtual void ReinitializeAll() {
			RecreateGeometry();
			RecreateMaterials();
			ResetEntities(true);
		}

		public void RecreateGeometry(bool forceInvalidation = false) {
			lock (instanceMutationLock) {
				if (!cacheInvalid && !forceInvalidation) return;
			}
			using (RenderingModule.RenderStateBarrier.AcquirePermit(withLock: instanceMutationLock)) {
				currentGeometryEntities.Values.ForEach(e => e.Dispose());
				currentGeometryEntities.Clear();
				if (currentCache != null) {
					AssetLocator.MainGeometryPass.SetVSForCache(currentCache, null);
					currentCache.Dispose();
				}
				if (currentSkydomeCache != null) {
					AssetLocator.MainGeometryPass.SetVSForCache(currentSkydomeCache, null);
					currentSkydomeCache.Dispose();
				}
				currentModelHandles.Clear();
				cachedDefaultModelHandles.Clear();
				if (geometry.Count == 0) return;

				LevelGeometry skydome = null;
				GeometryCacheBuilder<DefaultVertex> gcb = new GeometryCacheBuilder<DefaultVertex>();
				if (this is GameLevelDescription) gcb.OrderFirst = true;
				for (int i = 0; i < geometry.Count; ++i) {
					LevelGeometry thisGeom = geometry[i];
					if (thisGeom.IsSkydome) {
						skydome = thisGeom;
						continue;
					}
					List<DefaultVertex> outVertices;
					List<uint> outIndices;
					if (precalculatedTriangles.ContainsKey(thisGeom.ID)) {
						outVertices = precalculatedTriangles[thisGeom.ID].Item1;
						outIndices = precalculatedTriangles[thisGeom.ID].Item2;
					}
					else {
						thisGeom.GetVertexData(out outVertices, out outIndices);
						if (this is GameLevelDescription) {
							var singleMatchingEntity = levelGeometryEntities.FindSingleOrDefault(lge => lge.Geometry == thisGeom);
							if (singleMatchingEntity != null) {
								outVertices = outVertices.Select(v => new DefaultVertex(
									v.Position,
									v.Normal,
									v.Tangent,
									v.TexUV + singleMatchingEntity.TexPan
								)).ToList();
							}
						}
					}

					if (thisGeom is LevelGeometry_Model && thisGeom.Transform == Transform.DEFAULT_TRANSFORM) {
						string filename = ((LevelGeometry_Model)thisGeom).ModelFileName;
						if (cachedDefaultModelHandles.ContainsKey(filename)) currentModelHandles.Add(thisGeom, cachedDefaultModelHandles[filename]);
						else {
							var newHandle = gcb.AddModel(Title + "_Geometry_" + i + "_[" + thisGeom + "]", outVertices, outIndices);
							currentModelHandles.Add(thisGeom, newHandle);
							cachedDefaultModelHandles.Add(filename, newHandle);
						}
					}
					else {
						currentModelHandles.Add(thisGeom, gcb.AddModel(Title + "_Geometry_" + i + "_[" + thisGeom + "]", outVertices, outIndices));	
					}
					
					
				}
				currentCache = gcb.Build();
				currentSkydomeCache = null;
				if (skydome != null) {
					gcb = new GeometryCacheBuilder<DefaultVertex>();
					List<DefaultVertex> outVertices;
					List<uint> outIndices;
					if (precalculatedTriangles.ContainsKey(skydome.ID)) {
						outVertices = precalculatedTriangles[skydome.ID].Item1;
						outIndices = precalculatedTriangles[skydome.ID].Item2;
					}
					else {
						skydome.GetVertexData(out outVertices, out outIndices);
						if (this is GameLevelDescription) {
							var singleMatchingEntity = levelGeometryEntities.FindSingleOrDefault(lge => lge.Geometry == skydome);
							if (singleMatchingEntity != null) {
								outVertices = outVertices.Select(v => new DefaultVertex(
									v.Position,
									v.Normal,
									v.Tangent,
									v.TexUV + singleMatchingEntity.TexPan
								)).ToList();
							}
						}
					}

					if (skydome is LevelGeometry_Model && skydome.Transform == Transform.DEFAULT_TRANSFORM) {
						string filename = ((LevelGeometry_Model) skydome).ModelFileName;
						if (cachedDefaultModelHandles.ContainsKey(filename)) currentModelHandles.Add(skydome, cachedDefaultModelHandles[filename]);
						else {
							var newHandle = gcb.AddModel(Title + "_Skydome_" + skydome + "_[" + skydome + "]", outVertices, outIndices);
							currentModelHandles.Add(skydome, newHandle);
							cachedDefaultModelHandles.Add(filename, newHandle);
						}
					}
					else {
						currentModelHandles.Add(skydome, gcb.AddModel(Title + "_Geometry_" + "_[" + skydome + "]", outVertices, outIndices));
					}
					currentSkydomeCache = gcb.Build();
				}
				cacheInvalid = false;

				AssetLocator.MainGeometryPass.SetVSForCache(currentCache, GetRecommendedVertexShader());
				if (currentSkydomeCache != null) AssetLocator.MainGeometryPass.SetVSForCache(currentSkydomeCache, GetRecommendedVertexShader());
			}
		}

		public Dictionary<int, Tuple<List<DefaultVertex>, List<uint>>> PrecalculateGeometryTriangulation() {
			Dictionary<int, Tuple<List<DefaultVertex>, List<uint>>> result = new Dictionary<int, Tuple<List<DefaultVertex>, List<uint>>>();
			for (int i = 0; i < geometry.Count; ++i) {
				LevelGeometry thisGeom = geometry[i];
				List<DefaultVertex> outVertices;
				List<uint> outIndices;
				thisGeom.GetVertexData(out outVertices, out outIndices);
				if (this is GameLevelDescription) {
					var singleMatchingEntity = levelGeometryEntities.FindSingleOrDefault(lge => lge.Geometry == thisGeom);
					if (singleMatchingEntity != null) {
						outVertices = outVertices.Select(v => new DefaultVertex(
							v.Position,
							v.Normal,
							v.Tangent,
							v.TexUV + singleMatchingEntity.TexPan
						)).ToList();
					}
				}

				result.Add(thisGeom.ID, Tuple.Create(outVertices, outIndices));
			}

			return result;
		}

		public void RecreateMaterials() {
			lock (instanceMutationLock) {
				if (!materialsOutOfDate) return;
			}

			FragmentShader fs = GetRecommendedFragmentShader();

			using (RenderingModule.RenderStateBarrier.AcquirePermit(withLock: instanceMutationLock)) {
				currentMaterials.ForEach(kvp => kvp.Value.Dispose());
				currentMaterials.Clear();
				textureViews.ForEach(kvp => {
					AssetLocator.UnloadTexture(kvp.Key);
					kvp.Value.Dispose();
				});
				textureViews.Clear();

				foreach (LevelMaterial levelMaterial in materials) {
					Material material = new Material(levelMaterial.Name + " [" + levelMaterial.ID + "]", fs);
					ShaderResourceView srv;
					if (textureViews.ContainsKey(levelMaterial.TextureFileName)) {
						srv = textureViews[levelMaterial.TextureFileName];
					}
					else {
						ITexture2D loadedTexture = AssetLocator.LoadTexture(levelMaterial.TextureFileName, levelMaterial.GenerateMips);
						srv = loadedTexture.CreateView();
						textureViews[levelMaterial.TextureFileName] = srv;
					}
					material.SetMaterialResource((ResourceViewBinding) fs.GetBindingByIdentifier("DiffuseMap"), srv);
					if (fs.ContainsBinding("MaterialProperties")) {
						material.SetMaterialConstantValue(
							(ConstantBufferBinding) fs.GetBindingByIdentifier("MaterialProperties"),
							new Vector4(0f, 0f, 0f, levelMaterial.SpecularPower)
						);
					}

					if (fs.ContainsBinding("NormalMap")) {
						if (levelMaterial.NormalFileName != null) {
							var loadedNormalMap = AssetLocator.LoadTexture(levelMaterial.NormalFileName, levelMaterial.GenerateMips);
							srv = loadedNormalMap.CreateView();
							textureViews[levelMaterial.NormalFileName] = srv;
						}
						else srv = AssetLocator.DefaultNormalMapView;
						material.SetMaterialResource((ResourceViewBinding) fs.GetBindingByIdentifier("NormalMap"), srv);
					}

					if (fs.ContainsBinding("SpecularMap")) {
						if (levelMaterial.SpecularFileName != null) {
							var loadedSpecularMap = AssetLocator.LoadTexture(levelMaterial.SpecularFileName, levelMaterial.GenerateMips);
							srv = loadedSpecularMap.CreateView();
							textureViews[levelMaterial.SpecularFileName] = srv;
						}
						else srv = AssetLocator.DefaultSpecularMapView;
						material.SetMaterialResource((ResourceViewBinding) fs.GetBindingByIdentifier("SpecularMap"), srv);
					}

					if (fs.ContainsBinding("EmissiveMap")) {
						if (levelMaterial.EmissiveFileName != null) {
							var loadedEmissiveMap = AssetLocator.LoadTexture(levelMaterial.EmissiveFileName, levelMaterial.GenerateMips);
							srv = loadedEmissiveMap.CreateView();
							textureViews[levelMaterial.EmissiveFileName] = srv;
						}
						else srv = AssetLocator.DefaultEmissiveMapView;
						material.SetMaterialResource((ResourceViewBinding) fs.GetBindingByIdentifier("EmissiveMap"), srv);
					}

					currentMaterials.Add(levelMaterial, material);
				}

				materialsOutOfDate = false;
			}
		}

		public void ResetEntities(bool fullReset) {
			SceneLayer sl = GetRecommendedSceneLayer();
			lock (instanceMutationLock) {
				currentGeometryEntities.Values.ForEach(e => e.Dispose());
				currentGeometryEntities.Clear();
				foreach (LevelGeometryEntity levelEntity in levelGeometryEntities) {
					GeometryEntity entity;
					if (levelEntity.IsStatic) {
						entity = new GeometryEntity {
							Transform = levelEntity.InitialMovementStep.Transform
						};
					}
					else {
						entity = new PresetMovementEntity(levelEntity.MovementSteps.ToArray(), levelEntity.AlternatingMovementDirection, levelEntity.InitialDelay) {
							Transform = levelEntity.InitialMovementStep.Transform
						};
					}
					entity.SetModelInstance(sl, currentModelHandles[levelEntity.Geometry], currentMaterials[levelEntity.Material]);
					currentGeometryEntities.Add(levelEntity, entity);
				}
			}
			ResetEntityPhysics(fullReset);
		}

		public abstract void ResetEntityPhysics(bool fullReset);

		public virtual void ResetEntity(LevelGeometryEntity levelGeometryEntity, Material overrideMaterial = null) {
			SceneLayer sl = GetRecommendedSceneLayer();
			using (RenderingModule.RenderStateBarrier.AcquirePermit(withLock: instanceMutationLock)) {
				Entity rep = GetEntityRepresentation(levelGeometryEntity);
				rep.Dispose();
				GeometryEntity entity;
				if (levelGeometryEntity.IsStatic) {
					entity = new GeometryEntity {
						Transform = levelGeometryEntity.InitialMovementStep.Transform
					};
				}
				else {
					entity = new PresetMovementEntity(levelGeometryEntity.MovementSteps.ToArray(), levelGeometryEntity.AlternatingMovementDirection, levelGeometryEntity.InitialDelay) {
						Transform = levelGeometryEntity.InitialMovementStep.Transform
					};
				}
				entity.SetModelInstance(sl, currentModelHandles[levelGeometryEntity.Geometry], overrideMaterial ?? currentMaterials[levelGeometryEntity.Material]);
				currentGeometryEntities[levelGeometryEntity] = entity;
			}
		}

		protected abstract FragmentShader GetRecommendedFragmentShader();
		protected abstract VertexShader GetRecommendedVertexShader();
		protected abstract SceneLayer GetRecommendedSceneLayer();

		public virtual void Dispose() {
			using (RenderingModule.RenderStateBarrier.AcquirePermit(withLock: instanceMutationLock)) {
				currentGeometryEntities.Values.ForEach(e => e.Dispose());
				currentMaterials.ForEach(kvp => kvp.Value.Dispose());
				if (currentCache != null) currentCache.Dispose();
				if (currentSkydomeCache != null) currentSkydomeCache.Dispose();

				textureViews.ForEach(kvp => {
					AssetLocator.UnloadTexture(kvp.Key);
					kvp.Value.Dispose();
				});
				textureViews.Clear();

				precalculatedTriangles.Clear();
			}

			GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
			GC.Collect(2, GCCollectionMode.Forced, true);
		}
		#endregion
	}
}