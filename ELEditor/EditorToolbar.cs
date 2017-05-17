using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Ophidian.Losgap;
using Ophidian.Losgap.AssetManagement;
using Ophidian.Losgap.CSG;
using Ophidian.Losgap.Entities;
using Ophidian.Losgap.Rendering;

namespace Egodystonic.EscapeLizards.Editor {
	public partial class EditorToolbar : Form {
		private const float PICK_FLASH_TIME_SECS = 0.2f;
		private const float SKY_LIGHT_DISTANCE_IN_FRONT_OF_CAMERA = PhysicsManager.ONE_METRE_SCALED * 0.25f;
		private static readonly EditHistoryForm historyForm = new EditHistoryForm();
		private static readonly object staticMutationLock = new object();
		private static GeometryCache editorGeomCache;
		private static ModelHandle translationArrowModelHandle, rotationArrowModelHandle, scaleArrowModelHandle;
		private static readonly Dictionary<LevelGeometryEntity, ArrowVect> activeEntityArrows = new Dictionary<LevelGeometryEntity, ArrowVect>();
		private static readonly Dictionary<LevelGeometryEntity, EntityEditForm> openEntityForms = new Dictionary<LevelGeometryEntity, EntityEditForm>();
		private static Material redMat, greenMat, blueMat;
		private static ShaderResourceView redMatSRV, greenMatSRV, blueMatSRV;
		private static bool isClosed = false;
		private static EditorToolbar instance;
		private static LevelDescription currentLevel = new GameLevelDescription("Untitled");
		private static SkyLevelDescription currentSky = null;
		private static string currentFilename = "untitled";
		private static string currentSkyFilename = null;
		private static GameItemForm openMiscForm = null;
		private static SkyLightListForm openSkyLightForm = null;
		private static readonly Dictionary<GeometryEntity, float> entityFlashes = new Dictionary<GeometryEntity, float>();
		private static readonly Dictionary<LevelGeometryEntity, float[]> entityArrowDataCache = new Dictionary<LevelGeometryEntity, float[]>();
		private static readonly Dictionary<LevelGeometryEntity, Transform> entityArrowCacheTransforms = new Dictionary<LevelGeometryEntity, Transform>();
		private static bool pauseMovers = false;

		private static ReversionStepType reversionStepType;
		private static Vector4 reversionTotal = new Vector4(0f, 0f, 0f, 1f);
		private static Action reversionAction;
		private static string reversionActionDesc;

		public static EditorToolbar Instance {
			get {
				lock (staticMutationLock) {
					return instance;
				}
			}
			set {
				lock (staticMutationLock) {
					instance = value;
				}
			}
		}

		public static bool LockTranslationsToGrid {
			get {
				lock (staticMutationLock) {
					return !instance.disableGridsCheckBox.Checked;
				}
			}
		}

		public EditorToolbar() {
			Instance = this;
			InitializeComponent();
			historyForm.Show();
			EditorMainWindowManager.Init();

			using (RenderingModule.RenderStateBarrier.AcquirePermit(withLock: staticMutationLock)) {
				GeometryCacheBuilder<DefaultVertex> editorGCB = new GeometryCacheBuilder<DefaultVertex>();
				var trnArrowModel = AssetLoader.LoadModel(Path.Combine(AssetLocator.ModelsDir, "Arrow.obj"));
				translationArrowModelHandle = editorGCB.AddModel(
					"Translation Arrow",
					trnArrowModel.GetVertices<DefaultVertex>(typeof(DefaultVertex).GetConstructor(new[] { typeof(Vector3), typeof(Vector3), typeof(Vector3), typeof(Vector2) })).ToList(),
					trnArrowModel.GetIndices().ToList()
				);
				var rotArrowModel = AssetLoader.LoadModel(Path.Combine(AssetLocator.ModelsDir, "Rotaarrow.obj"));
				rotationArrowModelHandle = editorGCB.AddModel(
					"Rotation Arrow",
					rotArrowModel.GetVertices<DefaultVertex>(typeof(DefaultVertex).GetConstructor(new[] { typeof(Vector3), typeof(Vector3), typeof(Vector3), typeof(Vector2) })).ToList(),
					rotArrowModel.GetIndices().ToList()
				);
				var scaArrowModel = AssetLoader.LoadModel(Path.Combine(AssetLocator.ModelsDir, "ScaleArrow.obj"));
				scaleArrowModelHandle = editorGCB.AddModel(
					"Scaling Arrow",
					scaArrowModel.GetVerticesWithoutTangents<DefaultVertex>(typeof(DefaultVertex).GetConstructor(new[] { typeof(Vector3), typeof(Vector3), typeof(Vector2) })).ToList(),
					scaArrowModel.GetIndices().ToList()
				);
				editorGeomCache = editorGCB.Build();
				AssetLocator.MainGeometryPass.SetVSForCache(editorGeomCache, AssetLocator.MainGeometryVertexShader);

				FragmentShader fs = AssetLocator.GeometryFragmentShader;
				redMat = new Material("Plain Red", fs);
				redMatSRV = AssetLocator.LoadTexture("red.bmp").CreateView();
				redMat.SetMaterialResource((ResourceViewBinding) fs.GetBindingByIdentifier("DiffuseMap"), redMatSRV);
				greenMat = new Material("Plain Green", fs);
				greenMatSRV = AssetLocator.LoadTexture("green.bmp").CreateView();
				greenMat.SetMaterialResource((ResourceViewBinding) fs.GetBindingByIdentifier("DiffuseMap"), greenMatSRV);
				blueMat = new Material("Plain Blue", fs);
				blueMatSRV = AssetLocator.LoadTexture("blue.bmp").CreateView();
				blueMat.SetMaterialResource((ResourceViewBinding) fs.GetBindingByIdentifier("DiffuseMap"), blueMatSRV);
				if (fs.ContainsBinding("MaterialProperties")) {
					redMat.SetMaterialConstantValue((ConstantBufferBinding) fs.GetBindingByIdentifier("MaterialProperties"), new Vector4(0f, 0f, 0f, 1f));
					greenMat.SetMaterialConstantValue((ConstantBufferBinding) fs.GetBindingByIdentifier("MaterialProperties"), new Vector4(0f, 0f, 0f, 1f));
					blueMat.SetMaterialConstantValue((ConstantBufferBinding) fs.GetBindingByIdentifier("MaterialProperties"), new Vector4(0f, 0f, 0f, 1f));
				}

				EntityModule.PostTick += deltaTimeSecs => PostTick(deltaTimeSecs);
			}
		}

		public static void StartNewReversionStep() {
			lock (staticMutationLock) {
				if (reversionStepType == ReversionStepType.Action) {
					if (reversionAction != null) historyForm.AddStep(reversionActionDesc, reversionAction);
				}
				else if (!reversionTotal.EqualsExactly(new Vector4(0f, 0f, 0f, 1f))) {
					List<LevelGeometryEntity> revertibleEntities = instance.entityList.SelectedItems.Cast<LevelGeometryEntity>().ToList();
					Vector4 amountLocal;
					switch (reversionStepType) {
						case ReversionStepType.Translation:
							amountLocal = new Vector4(reversionTotal, w: 0f);
							string translationValue;
							if (reversionTotal.X != 0f) translationValue = "X: " + reversionTotal.X.ToString(2);
							else if (reversionTotal.Y != 0f) translationValue = "Y: " + reversionTotal.Y.ToString(2);
							else translationValue = "Z: " + reversionTotal.Z.ToString(2);
							historyForm.AddStep(
								"Translate entities (" + translationValue + " units)",
								() => {
									TranslateEntities(-((Vector3) amountLocal), revertibleEntities);
									Logger.Debug("Undo: Translation of " + amountLocal);
								}
							);
							break;
						case ReversionStepType.Scaling:
							amountLocal = new Vector4(reversionTotal, w: 0f);
							string scalingValue;
							if (reversionTotal.X != 1f) scalingValue = "X: " + reversionTotal.X.ToString(2);
							else if (reversionTotal.Y != 1f) scalingValue = "Y: " + reversionTotal.Y.ToString(2);
							else scalingValue = "Z: " + reversionTotal.Z.ToString(2);
							historyForm.AddStep(
								"Scale entities (" + scalingValue + "x)",
								() => {
									ScaleEntities(~((Vector3) amountLocal), revertibleEntities);
									Logger.Debug("Undo: Scaling of " + amountLocal);
								}
							);
							break;
						case ReversionStepType.Rotation:
							amountLocal = reversionTotal;
							Quaternion amountAsQuat = (Quaternion) amountLocal;
							historyForm.AddStep(
								"Rotate entities (" + amountAsQuat.RotAroundX.ToString(2) + "/" + amountAsQuat.RotAroundY.ToString(2) + "/" + amountAsQuat.RotAroundZ.ToString(2) + " rad)",
								() => {
									RotateEntities(-amountAsQuat, revertibleEntities);
									Logger.Debug("Undo: Rotation of " + amountAsQuat.ToStringAsEuler());
								}
							);
							break;
					}
				}
				reversionActionDesc = null;
				reversionTotal = new Vector4(0f, 0f, 0f, 1f);
				reversionAction = null;
			}
		}

		public static void TranslateEntities(Vector3 amount, IEnumerable<LevelGeometryEntity> entities = null) {
			if (entities == null) {
				reversionTotal += amount;
				reversionStepType = ReversionStepType.Translation;
			}
			entities = entities ?? (IEnumerable<LevelGeometryEntity>) instance.Invoke((Func<object>) (() => instance.entityList.SelectedItems.Cast<LevelGeometryEntity>()));
			lock (staticMutationLock) {
				foreach (LevelGeometryEntity levelEntity in entities) {
					Entity e = currentLevel.GetEntityRepresentation(levelEntity);
					if (e == null) continue; // Can happen when undoing something that we since deleted
					e.TranslateBy(amount);
					if (activeEntityArrows.ContainsKey(levelEntity)) activeEntityArrows[levelEntity].TranslateBy(amount);
					if (EntityIsBeingEdited(levelEntity)) {
						LevelEntityMovementStep oldLems = openEntityForms[levelEntity].SelectedMovementStep;
						openEntityForms[levelEntity].ReplaceSelectedMovementStep(
							new LevelEntityMovementStep(oldLems.Transform.TranslateBy(amount), oldLems.TravelTime, oldLems.SmoothTransition)
						);
					}
					else {
						foreach (LevelEntityMovementStep lems in levelEntity.MovementSteps) {
							levelEntity.ReplaceMovementStep(
								lems,
								new LevelEntityMovementStep(lems.Transform.TranslateBy(amount), lems.TravelTime, lems.SmoothTransition)
							);
						}

						if (currentLevel is GameLevelDescription) {
							foreach (LevelGameObject lgo in ((GameLevelDescription) currentLevel).GameObjects.Where(lgo => lgo.GroundingGeometryEntity == levelEntity)) {
								lgo.Transform = lgo.Transform.TranslateBy(amount);
								((GameLevelDescription) currentLevel).ResetGameObject(lgo);
							}
						}

						if (levelEntity.MovementSteps.Count > 1) currentLevel.ResetEntity(levelEntity);
					}
				}
			}
		}

		public static void RotateEntities(Quaternion amount, IEnumerable<LevelGeometryEntity> entities = null) {
			if (entities == null) {
				reversionTotal = (Vector4) (((Quaternion) reversionTotal) * amount);
				reversionStepType = ReversionStepType.Rotation;
			}
			entities = entities ?? (IEnumerable<LevelGeometryEntity>) instance.Invoke((Func<object>) (() => instance.entityList.SelectedItems.Cast<LevelGeometryEntity>()));
			lock (staticMutationLock) {
				IEnumerable<Entity> actualEntities = entities.Select(lge => currentLevel.GetEntityRepresentation(lge)).Where(ae => ae != null);
				Vector3 rotPivot = Vector3.ZERO;
				int count = 0;
				foreach (Entity entity in actualEntities) {
					rotPivot += entity.Transform.Translation;
					++count;
				}
				if (count == 0) return;
				rotPivot /= count;
				foreach (LevelGeometryEntity levelEntity in entities) {
					Entity e = currentLevel.GetEntityRepresentation(levelEntity);
					if (e == null) continue; // Can happen when undoing something that we since deleted
					e.RotateAround(rotPivot, amount);
					if (EntityIsBeingEdited(levelEntity)) {
						LevelEntityMovementStep oldLems = openEntityForms[levelEntity].SelectedMovementStep;
						openEntityForms[levelEntity].ReplaceSelectedMovementStep(
							new LevelEntityMovementStep(oldLems.Transform.RotateAround(rotPivot, amount), oldLems.TravelTime, oldLems.SmoothTransition)
						);
					}
					else {
						foreach (LevelEntityMovementStep lems in levelEntity.MovementSteps) {
							levelEntity.ReplaceMovementStep(
								lems,
								new LevelEntityMovementStep(lems.Transform.RotateAround(rotPivot, amount), lems.TravelTime, lems.SmoothTransition)
							);
						}

						if (currentLevel is GameLevelDescription) {
							foreach (LevelGameObject lgo in ((GameLevelDescription) currentLevel).GameObjects.Where(lgo => lgo.GroundingGeometryEntity == levelEntity)) {
								lgo.Transform = lgo.Transform.RotateAround(levelEntity.InitialMovementStep.Transform.Translation, amount);
								((GameLevelDescription) currentLevel).ResetGameObject(lgo);
							}
						}

						if (levelEntity.MovementSteps.Count > 1) currentLevel.ResetEntity(levelEntity);
					}
				}
			}
		}

		public static void ScaleEntities(Vector3 amount, IEnumerable<LevelGeometryEntity> entities = null) {
			if (entities == null) {
				if (reversionTotal.EqualsExactly(new Vector4(0f, 0f, 0f, 1f))) reversionTotal = Vector4.ONE;
				reversionTotal = reversionTotal.Scale(amount);
				reversionStepType = ReversionStepType.Scaling;
			}
			entities = entities ?? (IEnumerable<LevelGeometryEntity>) instance.Invoke((Func<object>) (() => instance.entityList.SelectedItems.Cast<LevelGeometryEntity>()));
			lock (staticMutationLock) {
				foreach (LevelGeometryEntity levelEntity in entities) {
					Entity e = currentLevel.GetEntityRepresentation(levelEntity);
					if (e == null) continue; // Can happen when undoing something that we since deleted
					e.ScaleBy(amount);
					if (EntityIsBeingEdited(levelEntity)) {
						LevelEntityMovementStep oldLems = openEntityForms[levelEntity].SelectedMovementStep;
						openEntityForms[levelEntity].ReplaceSelectedMovementStep(
							new LevelEntityMovementStep(oldLems.Transform.ScaleBy(amount), oldLems.TravelTime, oldLems.SmoothTransition)
						);
					}
					else {
						foreach (LevelEntityMovementStep lems in levelEntity.MovementSteps) {
							levelEntity.ReplaceMovementStep(
								lems,
								new LevelEntityMovementStep(lems.Transform.ScaleBy(amount), lems.TravelTime, lems.SmoothTransition)
							);
						}

						if (currentLevel is GameLevelDescription) {
							foreach (LevelGameObject lgo in ((GameLevelDescription) currentLevel).GameObjects.Where(lgo => lgo.GroundingGeometryEntity == levelEntity)) {
								lgo.Transform = lgo.Transform.ScaleBy(amount);
								((GameLevelDescription) currentLevel).ResetGameObject(lgo);
							}
						}

						if (levelEntity.MovementSteps.Count > 1) currentLevel.ResetEntity(levelEntity);
					}
				}
			}
		}

		public static void SelectEntityViaRay(Ray ray) {
			lock (staticMutationLock) {
				if (!instance.enablePickingCheckBox.Checked) return;
				Vector3 _;
				GeometryEntity selectedEntity = EntityModule.RayTestNearest(ray, out _) as GeometryEntity;
				if (selectedEntity == null) return;
				LevelGeometryEntity selectedLevelGeometryEntity = currentLevel.GetLevelEntityRepresentation(selectedEntity);
				if (selectedLevelGeometryEntity != null) {
					instance.BeginInvoke((Action) (() => {
						if (!EditorMainWindowManager.EntityTranslationEnabled) {
							instance.entityList.SelectedItems.Clear();
							instance.entityList.SelectedItem = selectedLevelGeometryEntity;
						}
						else {
							instance.entityList.SelectedItems.Add(selectedLevelGeometryEntity);
						}
					}));
				}

				if (!EntityIsBeingEdited(selectedLevelGeometryEntity)) {
					Transform t = selectedEntity.Transform;
					currentLevel.ResetEntity(selectedLevelGeometryEntity, redMat);
					GeometryEntity replacement = currentLevel.GetEntityRepresentation(selectedLevelGeometryEntity);
					replacement.Transform = t;
					entityFlashes[replacement] = PICK_FLASH_TIME_SECS;
				}
			}
		}

		public static LevelSkyLight AddSkyLightAtCamera() {
			lock (staticMutationLock) {
				return ((SkyLevelDescription) currentLevel).AddSkyLight(
					AssetLocator.MainCamera.Position + AssetLocator.MainCamera.Orientation.WithLength(SKY_LIGHT_DISTANCE_IN_FRONT_OF_CAMERA)
				);
			}
		}

		public static void AddGameObject(Ray ray) {
			lock (staticMutationLock) {
				if (openMiscForm == null) return;
				Vector3 hitPoint;
				Entity selectedEntity = EntityModule.RayTestNearest(ray, out hitPoint);
				Transform objectTransform;
				if (selectedEntity == null) {
					objectTransform = Transform.DEFAULT_TRANSFORM.TranslateBy(
						AssetLocator.MainCamera.Position + AssetLocator.MainCamera.Orientation.WithLength(0.5f * PhysicsManager.ONE_METRE_SCALED)
						);
				}
				else objectTransform = Transform.DEFAULT_TRANSFORM.TranslateBy(hitPoint);

				openMiscForm.BeginInvoke((Action) (() => openMiscForm.CreateNewGameObject(objectTransform)));
			}
		}

		public static void SetEntityMovementStep(LevelGeometryEntity target, LevelEntityMovementStep selectedStep) {
			EditorToolbar localInstance;
			lock (staticMutationLock) {
				Entity e = currentLevel.GetEntityRepresentation(target);
				e.Transform = selectedStep.Transform;
				if (e is PresetMovementEntity) {
					((PresetMovementEntity) e).Pause(selectedStep);
				}
			}
		}

		public static void RealtimeUpdateGO(LevelGameObject go) {
			GameLevelDescription gld;
			lock (staticMutationLock) {
				gld = currentLevel as GameLevelDescription;
			}
			if (gld == null) return;
			gld.ResetGameObject(go);
		}

		public static void CloneSelectedEntities() {
			Instance.BeginInvoke((Action) (() =>
				Instance.ActionOnSelectedEntity(SelectedEditorItemAction.Clone)	
			));
			
		}

		public static void UndoLast() {
			lock (staticMutationLock) {
				historyForm.UndoLast();
			}
		}

		public static void BakeAll() {
			lock (staticMutationLock) {
				instance.BakeSelectedEntitiesToGeometry(currentLevel.LevelEntities);
			}
		}

		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);
			LosgapSystem.SystemExited += () => {
				lock (staticMutationLock) {
					if (!isClosed) BeginInvoke((MethodInvoker) Close);
				}
			};

			AddAutomatedControlProperties();
			TopMost = true;
			BringToFront();
			TopMost = false;
		}

		protected override void OnFormClosed(FormClosedEventArgs e) {
			base.OnFormClosed(e);

			historyForm.Close();

			using (RenderingModule.RenderStateBarrier.AcquirePermit(withLock: staticMutationLock)) {
				isClosed = true;
				redMatSRV.Resource.Dispose();
				redMatSRV.Dispose();
				greenMatSRV.Resource.Dispose();
				greenMatSRV.Dispose();
				blueMatSRV.Resource.Dispose();
				blueMatSRV.Dispose();
				redMat.Dispose();
				greenMat.Dispose();
				blueMat.Dispose();
				activeEntityArrows.Values.ForEach(x => x.Dispose());
				editorGeomCache.Dispose();
			}

			if (LosgapSystem.IsRunning) {
				LosgapSystem.InvokeOnMasterAsync(EditorMainWindowManager.Close);
			}
		}

		private void AddAutomatedControlProperties() {
			foreach (PictureBox pictureBox in Controls.OfType<PictureBox>()) {
				pictureBox.BackColor = Color.Transparent;
				pictureBox.MouseEnter += (sender, args) => {
					Controls.OfType<PictureBox>().ForEach(pb => {
						pb.BackColor = Color.Transparent;
						pb.BorderStyle = BorderStyle.None;
					});
					pictureBox.BackColor = Color.Beige;
					pictureBox.BorderStyle = BorderStyle.FixedSingle;
				};
				pictureBox.MouseLeave += (sender, args) => {
					pictureBox.BackColor = Color.Transparent;
					pictureBox.BorderStyle = BorderStyle.None;
				};
			}

			saveFilePB.MouseClick += (sender, args) => SaveCurrentFile();
			loadFilePB.MouseClick += (sender, args) => LoadFile();
			newFilePB.MouseClick += (sender, args) => ShowNewFileDialog();
			metaPB.MouseClick += (sender, args) => ShowMetaDialog();

			addCuboidPB.MouseClick += (sender, args) => AddCuboid();
			addConePB.MouseClick += (sender, args) => AddCone();
			addSpherePB.MouseClick += (sender, args) => AddSphere();
			addCurvePB.MouseClick += (sender, args) => AddCurve();
			addPlanePB.MouseClick += (sender, args) => AddPlane();
			addModelPB.MouseClick += (sender, args) => AddModel();

			geomDeleteButton.MouseClick += (sender, args) => ActionOnSelectedGeometry(SelectedEditorItemAction.Delete);
			geomEditButton.MouseClick += (sender, args) => ActionOnSelectedGeometry(SelectedEditorItemAction.Edit);
			geomCloneButton.MouseClick += (sender, args) => ActionOnSelectedGeometry(SelectedEditorItemAction.Clone);

			matDeleteButton.MouseClick += (sender, args) => ActionOnSelectedMaterial(SelectedEditorItemAction.Delete);
			matEditButton.MouseClick += (sender, args) => ActionOnSelectedMaterial(SelectedEditorItemAction.Edit);
			matCloneButton.MouseClick += (sender, args) => ActionOnSelectedMaterial(SelectedEditorItemAction.Clone);
			matBakeButton.MouseClick += (sender, args) => BakeSelectedMaterial();
			matSwapButton.MouseClick += (sender, args) => SwapSelectedMaterial();

			texturingPB.MouseClick += (sender, args) => AddMaterial();

			createEntityButton.MouseClick += (sender, args) => CreateEntity();

			entityDeleteButton.MouseClick += (sender, args) => ActionOnSelectedEntity(SelectedEditorItemAction.Delete);
			entityEditButton.MouseClick += (sender, args) => ActionOnSelectedEntity(SelectedEditorItemAction.Edit);
			entityCloneButton.MouseClick += (sender, args) => ActionOnSelectedEntity(SelectedEditorItemAction.Clone);

			setMatButton.MouseClick += (sender, args) => SetMatOnSelectedEntities();
			setGeomButton.MouseClick += (sender, args) => SetGeomOnSelectedEntities();

			reinitSceneButton.MouseClick += (sender, args) => {
				UpdateUI();
				ReinitializeScene();
			};
			cameraResetButton.MouseClick += (sender, args) => {
				AssetLocator.MainCamera.Position = Vector3.ZERO;
				AssetLocator.MainCamera.Orient(Vector3.FORWARD, Vector3.UP);
			};

			entityList.SelectedIndexChanged += (sender, args) => UpdateSelectedEntities();
			magicButton.MouseClick += (sender, args) => BakeSelectedEntitiesToGeometry();

			miscPB.MouseClick += (sender, args) => ShowMiscDialog();

			uvPanUNegButton.MouseClick += (sender, args) => PanSelectedEntities(UVPanDirection.NegativeU);
			uvPanUPosButton.MouseClick += (sender, args) => PanSelectedEntities(UVPanDirection.PositiveU);
			uvPanVNegButton.MouseClick += (sender, args) => PanSelectedEntities(UVPanDirection.NegativeV);
			uvPanVPosButton.MouseClick += (sender, args) => PanSelectedEntities(UVPanDirection.PositiveV);

			enableDoFCheckbox.CheckedChanged += (sender, args) => SetDoFEnabled();
			pauseMoversCheckBox.CheckedChanged += (sender, args) => TogglePauseMovers(pauseMoversCheckBox.Checked);
		}

		private static void TogglePauseMovers(bool pause) {
			lock (staticMutationLock) pauseMovers = pause;
			foreach (var entity in currentLevel.LevelEntities) {
				var entityRep = currentLevel.GetEntityRepresentation(entity);
				if (entityRep is PresetMovementEntity) {
					var mover = ((PresetMovementEntity) entityRep);
					if (pause) mover.Pause();
					else mover.Unpause();
				}
			}
		}


		#region Save / Load / Meta
		private void SaveCurrentFile() {
			lock (staticMutationLock) {
				string fullFilePath = Path.Combine(AssetLocator.LevelsDir, currentFilename + ".ell");
				currentLevel.Save(fullFilePath);
				MessageBox.Show(this, "File saved successfully to \"" + fullFilePath + "\".");
			}
		}

		private void LoadFile() {
			OpenFileDialog fileSelectorDialog = new OpenFileDialog();
			fileSelectorDialog.AutoUpgradeEnabled = false; // This line required to make it not crash the entire application, gg microsoft
			fileSelectorDialog.ShowHelp = true;
			fileSelectorDialog.Multiselect = false;
			fileSelectorDialog.DefaultExt = "*.ell";
			fileSelectorDialog.InitialDirectory = AssetLocator.LevelsDir;
			var dialogResult = fileSelectorDialog.ShowDialog(this);
			if (dialogResult != DialogResult.OK) return;

			using (RenderingModule.RenderStateBarrier.AcquirePermit()) {
				try {
					lock (staticMutationLock) {
						string ptdFilePath = Path.Combine(AssetLocator.LevelsDir, Path.GetFileNameWithoutExtension(fileSelectorDialog.FileName)) + ".ptd";
						if (File.Exists(ptdFilePath)) {
							Logger.Log("Found PTD file '" + ptdFilePath + "' for level '" + fileSelectorDialog.FileName + "'. File will be deleted.");
							File.Delete(ptdFilePath);
						}
						LevelDescription loadedLevelDescription = LevelDescription.Load(Path.Combine(AssetLocator.LevelsDir, fileSelectorDialog.FileName));
						if (currentLevel != null) {
							currentLevel.Dispose();
							currentLevel = null;
						}
						if (currentSky != null) {
							currentSky.Dispose();
							currentSky = null;
						}
						currentLevel = loadedLevelDescription;
						currentFilename = Path.GetFileNameWithoutExtension(fileSelectorDialog.FileName);
						metaTitleLabel.Text = loadedLevelDescription.Title;
						metaFilenameLabel.Text = currentFilename + ".ell";
						metaTypeLabel.Text = (loadedLevelDescription is GameLevelDescription ? "Game Level" : "Skybox");
					}
				}
				catch (Exception e) {
					MessageBox.Show(this, "Could not open file: " + e.GetAllMessages());
				}

				if (openMiscForm != null) openMiscForm.Close();
				if (openSkyLightForm != null) openSkyLightForm.Close();
				historyForm.ClearAllHistory();
				UpdateUI();
				ReinitializeScene();
			}
		}

		private void ShowMetaDialog() {
			lock (staticMutationLock) {
				LevelMetaEditForm metaForm = new LevelMetaEditForm((_, title, filename, skyFilename) => {
					metaTitleLabel.Text = currentLevel.Title = title;
					metaFilenameLabel.Text = (currentFilename = filename) + ".ell";
					if (currentLevel is GameLevelDescription) ((GameLevelDescription) currentLevel).SkyboxFileName = skyFilename;
				}, currentLevel, currentFilename);
				metaForm.ShowDialog();
			}
		}

		private void ShowNewFileDialog() {
			lock (staticMutationLock) {
				LevelMetaEditForm metaForm = new LevelMetaEditForm((isGameLevel, title, filename, skyFilename) => {
					if (currentLevel != null) currentLevel.Dispose();
					if (isGameLevel) currentLevel = new GameLevelDescription(title) { SkyboxFileName = skyFilename };
					else currentLevel = new SkyLevelDescription(title);
					currentFilename = filename;
					metaTitleLabel.Text = title;
					metaFilenameLabel.Text = filename + ".ell";
					metaTypeLabel.Text = (isGameLevel ? "Game Level" : "Skybox");
					historyForm.ClearAllHistory();
				});
				metaForm.ShowDialog();
				UpdateUI();
				ReinitializeScene();
			}
		}
		#endregion

		#region Geometry
		private void AddCuboid() {
			LevelGeometry_Cuboid modelData = GetCuboidParams(null);
			if (modelData != null) {
				lock (staticMutationLock) {
					currentLevel.AddGeometry(modelData);
				}
				UpdateGeometryList();
				ReinitializeScene();
			}
		}

		private void AddCone() {
			LevelGeometry_Cone modelData = GetConeParams(null);
			if (modelData != null) {
				lock (staticMutationLock) {
					currentLevel.AddGeometry(modelData);
				}
				UpdateGeometryList();
				ReinitializeScene();
			}
		}

		private void AddSphere() {
			LevelGeometry_Sphere modelData = GetSphereParams(null);
			if (modelData != null) {
				lock (staticMutationLock) {
					currentLevel.AddGeometry(modelData);
				}
				UpdateGeometryList();
				ReinitializeScene();
			}
		}

		private void AddCurve() {
			LevelGeometry_Curve modelData = GetCurveParams(null);
			if (modelData != null) {
				lock (staticMutationLock) {
					currentLevel.AddGeometry(modelData);
				}
				UpdateGeometryList();
				ReinitializeScene();
			}
		}

		private void AddPlane() {
			LevelGeometry_Plane modelData = GetPlaneParams(null);
			if (modelData != null) {
				lock (staticMutationLock) {
					currentLevel.AddGeometry(modelData);
				}
				UpdateGeometryList();
				ReinitializeScene();
			}
		}

		private void AddModel() {
			LevelGeometry_Model modelData = GetModelParams(null);
			if (modelData != null) {
				lock (staticMutationLock) {
					currentLevel.AddGeometry(modelData);
				}
				UpdateGeometryList();
				ReinitializeScene();
			}
		}

		private LevelGeometry_Cuboid GetCuboidParams(LevelGeometry_Cuboid existingData) {
			LevelGeometry_Cuboid result = null;

			Func<string[], Vector3, Vector3, Vector3, Vector2, bool, uint, bool> shapeCheckFunc = (props, scale, rotation, translation, texScaling, insideOut, extrapolation) => {
				Vector3 dimensions = new Vector3(
					float.Parse(props[0], CultureInfo.InvariantCulture),
					float.Parse(props[1], CultureInfo.InvariantCulture),
					float.Parse(props[2], CultureInfo.InvariantCulture)
				);

				if (dimensions.X <= 0f || dimensions.Y <= 0f || dimensions.Z <= 0f) return false;

				lock (staticMutationLock) {
					result = new LevelGeometry_Cuboid(
						texScaling,
						scale, rotation, translation,
						insideOut,
						new Cuboid(dimensions * -0.5f, dimensions.X, dimensions.Y, dimensions.Z),
						currentLevel.GetNewGeometryID()
						);
				}

				return true;
			};

			GeomDescForm addShapeForm = new GeomDescForm(shapeCheckFunc, null, "Width", "Height", "Depth");
			if (existingData != null) {
				addShapeForm.SetDefaultText("Width", existingData.ShapeDesc.Width.ToString());
				addShapeForm.SetDefaultText("Height", existingData.ShapeDesc.Height.ToString());
				addShapeForm.SetDefaultText("Depth", existingData.ShapeDesc.Depth.ToString());
				SetDefaultParamsForExistingGeom(addShapeForm, existingData);
			}
			addShapeForm.ShowDialog();

			return result;
		}
		private LevelGeometry_Cone GetConeParams(LevelGeometry_Cone existingData) {
			LevelGeometry_Cone result = null;

			Func<string[], Vector3, Vector3, Vector3, Vector2, bool, uint, bool> shapeCheckFunc = (props, scale, rotation, translation, texScaling, insideOut, extrapolation) => {
				float topRadius = float.Parse(props[0], CultureInfo.InvariantCulture);
				float bottomRadius = float.Parse(props[1], CultureInfo.InvariantCulture);
				float height = float.Parse(props[2], CultureInfo.InvariantCulture);

				if (topRadius == 0f && bottomRadius == 0f) return false;
				if (height <= 0f) return false;

				lock (staticMutationLock) {
					result = new LevelGeometry_Cone(
						texScaling,
						scale, rotation, translation,
						insideOut,
						new Cone(Vector3.ZERO, bottomRadius, height, topRadius),
						extrapolation,
						currentLevel.GetNewGeometryID()
						);
				}

				return true;
			};

			GeomDescForm addShapeForm = new GeomDescForm(shapeCheckFunc, "Num. Sides", "Top Radius", "Bottom Radius", "Height");
			if (existingData != null) {
				addShapeForm.SetExtrapolationValue(existingData.NumSides.ToString());
				addShapeForm.SetDefaultText("Top Radius", existingData.ShapeDesc.TopRadius.ToString());
				addShapeForm.SetDefaultText("Bottom Radius", existingData.ShapeDesc.BottomRadius.ToString());
				addShapeForm.SetDefaultText("Height", existingData.ShapeDesc.Height.ToString());
				SetDefaultParamsForExistingGeom(addShapeForm, existingData);
			}
			else {
				addShapeForm.SetExtrapolationValue("16");
				addShapeForm.SetDefaultText("Top Radius", "0.0");
			}
			addShapeForm.ShowDialog();

			return result;
		}
		private LevelGeometry_Sphere GetSphereParams(LevelGeometry_Sphere existingData) {
			LevelGeometry_Sphere result = null;

			Func<string[], Vector3, Vector3, Vector3, Vector2, bool, uint, bool> shapeCheckFunc = (props, scale, rotation, translation, texScaling, insideOut, extrapolation) => {
				float radius = float.Parse(props[0], CultureInfo.InvariantCulture);

				if (radius <= 0f) return false;

				lock (staticMutationLock) {
					result = new LevelGeometry_Sphere(
						texScaling,
						scale, rotation, translation,
						insideOut,
						new Sphere(Vector3.ZERO, radius),
						extrapolation,
						currentLevel.GetNewGeometryID()
						);
				}

				return true;
			};

			GeomDescForm addShapeForm = new GeomDescForm(shapeCheckFunc, "Extrapolation", "Radius");
			if (existingData != null) {
				addShapeForm.SetDefaultText("Radius", existingData.ShapeDesc.Radius.ToString());
				addShapeForm.SetExtrapolationValue(existingData.Extrapolation.ToString());
				SetDefaultParamsForExistingGeom(addShapeForm, existingData);
			}
			else addShapeForm.SetExtrapolationValue("3");
			addShapeForm.ShowDialog();

			return result;
		}
		private LevelGeometry_Curve GetCurveParams(LevelGeometry_Curve existingData) {
			LevelGeometry_Curve result = null;

			Func<string[], Vector3, Vector3, Vector3, Vector2, bool, uint, bool> shapeCheckFunc = (props, scale, rotation, translation, texScaling, insideOut, extrapolation) => {
				Vector3 startingCuboidDimensions = new Vector3(
					float.Parse(props[0], CultureInfo.InvariantCulture),
					float.Parse(props[1], CultureInfo.InvariantCulture),
					float.Parse(props[2], CultureInfo.InvariantCulture)
				);

				if (startingCuboidDimensions.X <= 0f || startingCuboidDimensions.Y <= 0f || startingCuboidDimensions.Z <= 0f) return false;

				float yawRot = MathUtils.DegToRad(float.Parse(props[3], CultureInfo.InvariantCulture));
				float pitchRot = MathUtils.DegToRad(float.Parse(props[4], CultureInfo.InvariantCulture));
				float rollRot = MathUtils.DegToRad(float.Parse(props[5], CultureInfo.InvariantCulture));

				if (extrapolation < 1U) return false;

				lock (staticMutationLock) {
					result = new LevelGeometry_Curve(
						texScaling,
						scale, rotation, translation,
						insideOut,
						new MeshGenerator.CurveDesc(
							new Cuboid(startingCuboidDimensions * -0.5f, startingCuboidDimensions.X, startingCuboidDimensions.Y, startingCuboidDimensions.Z), 
							yawRot, pitchRot, rollRot, extrapolation
						), 
						currentLevel.GetNewGeometryID()
					);
				}

				return true;
			};

			GeomDescForm addShapeForm = new GeomDescForm(shapeCheckFunc, "Num Segments", 
				"Width", "Height", "Depth",
				"Yaw (Deg)", "Pitch (Deg)", "Roll (Deg)"
			);
			if (existingData != null) {
				addShapeForm.SetDefaultText("Width", existingData.ShapeDesc.StartingCuboid.Width.ToString());
				addShapeForm.SetDefaultText("Height", existingData.ShapeDesc.StartingCuboid.Height.ToString());
				addShapeForm.SetDefaultText("Depth", existingData.ShapeDesc.StartingCuboid.Depth.ToString());
				addShapeForm.SetDefaultText("Yaw (Deg)", MathUtils.RadToDeg(existingData.ShapeDesc.YawRotationRads).ToString(0));
				addShapeForm.SetDefaultText("Pitch (Deg)", MathUtils.RadToDeg(existingData.ShapeDesc.PitchRotationRads).ToString(0));
				addShapeForm.SetDefaultText("Roll (Deg)", MathUtils.RadToDeg(existingData.ShapeDesc.RollRotationRads).ToString(0));
				addShapeForm.SetExtrapolationValue(existingData.ShapeDesc.NumSegments.ToString());
				SetDefaultParamsForExistingGeom(addShapeForm, existingData);
			}
			else {
				addShapeForm.SetDefaultText("Yaw (Deg)", "0");
				addShapeForm.SetDefaultText("Pitch (Deg)", "0");
				addShapeForm.SetDefaultText("Roll (Deg)", "0");
				addShapeForm.SetDefaultText("Yaw Radius", (PhysicsManager.ONE_METRE_SCALED * 4f).ToString(2));
				addShapeForm.SetDefaultText("Pitch Radius", (PhysicsManager.ONE_METRE_SCALED * 4f).ToString(2));
				addShapeForm.SetDefaultText("Roll Radius", (PhysicsManager.ONE_METRE_SCALED * 4f).ToString(2));
				addShapeForm.SetExtrapolationValue("4");
			}
			addShapeForm.ShowDialog();

			return result;
		}
		private LevelGeometry_Plane GetPlaneParams(LevelGeometry_Plane existingData) {
			LevelGeometry_Plane result = null;

			Func<string[], Vector3, Vector3, Vector3, Vector2, bool, uint, bool> shapeCheckFunc = (props, scale, rotation, translation, texScaling, insideOut, extrapolation) => {
				string[] vertStrings = props[0].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				Vector3[] verts = new Vector3[vertStrings.Length];
				if (verts.Length < 3) return false;
				for (int i = 0; i < vertStrings.Length; ++i) {
					string[] vertComponents = vertStrings[i].Split(',');
					verts[i] = new Vector3(
						float.Parse(vertComponents[0], CultureInfo.InvariantCulture),
						float.Parse(vertComponents[1], CultureInfo.InvariantCulture),
						float.Parse(vertComponents[2], CultureInfo.InvariantCulture)
					);
				}

				lock (staticMutationLock) {
					result = new LevelGeometry_Plane(
						texScaling,
						scale, rotation, translation,
						insideOut,
						verts,
						currentLevel.GetNewGeometryID()
						);
				}

				return true;
			};

			GeomDescForm addShapeForm = new GeomDescForm(shapeCheckFunc, null, "Vertices");
			if (existingData != null) {
				addShapeForm.SetDefaultText(
					"Vertices", 
					existingData.Corners.Aggregate(
						String.Empty, 
						(cur, corner) => cur + corner.X + "," + corner.Y + "," + corner.Z + " "
					).Trim()
				);
				SetDefaultParamsForExistingGeom(addShapeForm, existingData);
			}
			else addShapeForm.SetDefaultText("Vertices", "-1,-1,0 -1,1,0 1,1,0 1,-1,0");
			addShapeForm.ShowDialog();

			return result;
		}
		private LevelGeometry_Model GetModelParams(LevelGeometry_Model existingData) {
			OpenFileDialog fileSelectorDialog = new OpenFileDialog();
			fileSelectorDialog.AutoUpgradeEnabled = false; // This line required to make it not crash the entire application, gg microsoft
			fileSelectorDialog.ShowHelp = true;
			fileSelectorDialog.Multiselect = false;
			fileSelectorDialog.DefaultExt = "*.obj";
			fileSelectorDialog.InitialDirectory = AssetLocator.ModelsDir;
			if (existingData != null) fileSelectorDialog.FileName = existingData.ModelFileName;
			var dialogResult = fileSelectorDialog.ShowDialog(this);
			if (dialogResult != DialogResult.OK) return null;

			if (!fileSelectorDialog.CheckFileExists) {
				MessageBox.Show(this, "Invalid file (does not exist).");
				return null;
			}

			if (!fileSelectorDialog.FileName.EndsWith(".OBJ", StringComparison.OrdinalIgnoreCase)) {
				MessageBox.Show(this, "Only OBJ files supported currently.");
				return null;
			}

			DialogResult mirrorDiagRes = MessageBox.Show(this, "Would you like to mirror (flip) the model along the X-axis?", "Mirror Model", MessageBoxButtons.YesNo);

			lock (staticMutationLock) {
				return new LevelGeometry_Model(Path.GetFileName(fileSelectorDialog.FileName), mirrorDiagRes == DialogResult.Yes, currentLevel.GetNewGeometryID());
			}
		}

		private void SetDefaultParamsForExistingGeom(GeomDescForm form, LevelGeometry geom) {
			form.SetDefaultText("Translation (X)", geom.Transform.Translation.X.ToString());
			form.SetDefaultText("Translation (Y)", geom.Transform.Translation.Y.ToString());
			form.SetDefaultText("Translation (Z)", geom.Transform.Translation.Z.ToString());
			form.SetDefaultText("Scale (X)", geom.Transform.Scale.X.ToString());
			form.SetDefaultText("Scale (Y)", geom.Transform.Scale.Y.ToString());
			form.SetDefaultText("Scale (Z)", geom.Transform.Scale.Z.ToString());
			form.SetDefaultText("Rotation (X)", geom.EulerRotations.X.ToString());
			form.SetDefaultText("Rotation (Y)", geom.EulerRotations.Y.ToString());
			form.SetDefaultText("Rotation (Z)", geom.EulerRotations.Z.ToString());

			form.SetTexScaleValue(geom.TextureScaling);
			form.SetInsideOutValue(geom.IsInsideOut);
		}

		private void ActionOnSelectedGeometry(SelectedEditorItemAction action) {
			lock (staticMutationLock) {
				IEnumerable<LevelGeometry> selectedGeometry = geomList.SelectedItems.Cast<LevelGeometry>().ToList(); // To stop concurrent modification problems
				int numSelectedItems = selectedGeometry.Count();
				if (numSelectedItems > 0) {
					switch (action) {
						case SelectedEditorItemAction.Delete:
							DialogResult deleteResult = MessageBox.Show(
								this,
								"Are you sure you wish to DELETE " + numSelectedItems + " geometric primitives?"
								+ Environment.NewLine + Environment.NewLine + "Any entities using the deleted geometry will also be deleted.",
								"Delete Geometry",
								MessageBoxButtons.OKCancel
								);
							if (deleteResult == DialogResult.OK) {
								lock (staticMutationLock) {
									foreach (var levelGeometry in selectedGeometry) {
										currentLevel.DeleteGeometry(levelGeometry);
										foreach (LevelGeometryEntity levelEntity in currentLevel.LevelEntities.Where(lge => lge.Geometry == levelGeometry)) {
											currentLevel.DeleteEntity(levelEntity);
										}
									}
								}
								UpdateGeometryList();
								UpdateEntityList();
								ReinitializeScene();
							}
							break;
						case SelectedEditorItemAction.Clone:
						case SelectedEditorItemAction.Edit:
							foreach (var levelGeometry in selectedGeometry) {
								LevelGeometry editOrClone = null;
								if (levelGeometry is LevelGeometry_Cuboid) editOrClone = GetCuboidParams((LevelGeometry_Cuboid) levelGeometry);
								else if (levelGeometry is LevelGeometry_Cone) editOrClone = GetConeParams((LevelGeometry_Cone) levelGeometry);
								else if (levelGeometry is LevelGeometry_Sphere) editOrClone = GetSphereParams((LevelGeometry_Sphere) levelGeometry);
								else if (levelGeometry is LevelGeometry_Curve) editOrClone = GetCurveParams((LevelGeometry_Curve) levelGeometry);
								else if (levelGeometry is LevelGeometry_Plane) editOrClone = GetPlaneParams((LevelGeometry_Plane) levelGeometry);
								else if (levelGeometry is LevelGeometry_Model) editOrClone = GetModelParams((LevelGeometry_Model) levelGeometry);

								if (editOrClone != null) {
									lock (staticMutationLock) {
										if (action == SelectedEditorItemAction.Clone) currentLevel.AddGeometry(editOrClone);
										else currentLevel.ReplaceGeometry(levelGeometry, editOrClone);
									}
								}
							}
							UpdateGeometryList();
							ReinitializeScene();
							break;
					}
					
				}
				else MessageBox.Show(this, "Please select one or more items in the geometry list first.");
			}
		}
		#endregion

		#region Materials
		private void AddMaterial() {
			LevelMaterial result = null;

			MatDescForm matDescForm = new MatDescForm(
				(matName, textureFile, normalFile, specularFile, emissiveFile, specPower, mipGen) => {
					if (matName.IsNullOrWhiteSpace()) return false;
					if (!File.Exists(Path.Combine(AssetLocator.MaterialsDir, textureFile))) return false;

					lock (staticMutationLock) {
						result = new LevelMaterial(matName, textureFile, normalFile, specularFile, emissiveFile, specPower, mipGen, currentLevel.GetNewMaterialID());
					}
					return true;
				}
			);

			matDescForm.ShowDialog(this);

			if (result != null) {
				lock (staticMutationLock) {
					currentLevel.AddMaterial(result);
				}
				UpdateMaterialList();
				ReinitializeScene();
			}
		}

		private void BakeSelectedMaterial() {
			IEnumerable<LevelMaterial> selectedMaterials;
			lock (staticMutationLock) {
				selectedMaterials = materialList.SelectedItems.Cast<LevelMaterial>().ToList(); // To stop concurrent modification problems
			}
			BakeSelectedEntitiesToGeometry(currentLevel.LevelEntities.Where(e => selectedMaterials.Contains(e.Material)));
		}

		private void SwapSelectedMaterial() {
			IEnumerable<LevelMaterial> selectedMaterials;
			lock (staticMutationLock) {
				selectedMaterials = materialList.SelectedItems.Cast<LevelMaterial>().ToList(); // To stop concurrent modification problems
			}
			LevelMaterial selectedResult;
			try {
				selectedResult = ListSelect.GetSelection(materialList.Items.Cast<LevelMaterial>(), selectedMaterials.FirstOrDefault(), true);
			}
			catch (OperationCanceledException) {
				return;
			}
			Dictionary<LevelGeometryEntity, LevelMaterial> affectedLGEs = new Dictionary<LevelGeometryEntity, LevelMaterial>();
			foreach (var entity in currentLevel.LevelEntities) {
				if (selectedMaterials.Contains(entity.Material)) {
					affectedLGEs.Add(entity, entity.Material);
					entity.Material = selectedResult;
					currentLevel.ResetEntity(entity);
				}
			}

			reversionStepType = ReversionStepType.Action;
			reversionActionDesc = "Swap Material";
			reversionAction = () => {
				foreach (var kvp in affectedLGEs) {
					kvp.Key.Material = kvp.Value;
				}
				currentLevel.ResetEntities(false);
			};
			StartNewReversionStep();
		}

		private void ActionOnSelectedMaterial(SelectedEditorItemAction action) {
			lock (staticMutationLock) {
				IEnumerable<LevelMaterial> selectedMaterials = materialList.SelectedItems.Cast<LevelMaterial>().ToList(); // To stop concurrent modification problems
				int numSelectedItems = selectedMaterials.Count();
				if (numSelectedItems > 0) {
					switch (action) {
						case SelectedEditorItemAction.Delete:
							DialogResult deleteResult = MessageBox.Show(
								this,
								"Are you sure you wish to DELETE " + numSelectedItems + " materials?"
								+ Environment.NewLine + Environment.NewLine + "Any entities using the deleted materials will also be deleted.",
								"Delete Materials",
								MessageBoxButtons.OKCancel
								);
							if (deleteResult == DialogResult.OK) {
								lock (staticMutationLock) {
									foreach (var levelMaterial in selectedMaterials) {
										currentLevel.DeleteMaterial(levelMaterial);
										foreach (LevelGeometryEntity levelEntity in currentLevel.LevelEntities.Where(lge => lge.Material == levelMaterial)) {
											currentLevel.DeleteEntity(levelEntity);
										}
									}
								}
								UpdateMaterialList();
								UpdateEntityList();
								ReinitializeScene();
							}
							break;
						case SelectedEditorItemAction.Clone:
						case SelectedEditorItemAction.Edit:
							foreach (var levelMaterial in selectedMaterials) {
								LevelMaterial editOrClone = null;
								MatDescForm editOrCloneForm = new MatDescForm(
									levelMaterial.Name, 
									levelMaterial.TextureFileName, 
									levelMaterial.NormalFileName,
									levelMaterial.SpecularFileName,
									levelMaterial.EmissiveFileName,
									levelMaterial.SpecularPower,
									levelMaterial.GenerateMips,
									(matName, texFileName, normFileName, specFileName, emisFileName, specProps, genMips) => {
										if (matName.IsNullOrWhiteSpace()) return false;
										if (!File.Exists(Path.Combine(AssetLocator.MaterialsDir, texFileName))) return false;

										lock (staticMutationLock) {
											editOrClone = new LevelMaterial(matName, texFileName, normFileName, specFileName, emisFileName, specProps, genMips, currentLevel.GetNewMaterialID());
										}
										return true;
									}
								);

								editOrCloneForm.ShowDialog(this);

								if (editOrClone != null) {
									lock (staticMutationLock) {
										if (action == SelectedEditorItemAction.Clone) currentLevel.AddMaterial(editOrClone);
										else currentLevel.ReplaceMaterial(levelMaterial, editOrClone);
									}
								}
								UpdateMaterialList();
								ReinitializeScene();
							}
							break;
					}

				}
				else MessageBox.Show(this, "Please select one or more items in the geometry list first.");
			}
		}
		#endregion

		#region Entities
		private Transform GetDisplayedTransformForEntity(LevelGeometryEntity levelGeometryEntity) {
			lock (staticMutationLock) {
				if (openEntityForms.ContainsKey(levelGeometryEntity)) return openEntityForms[levelGeometryEntity].SelectedMovementStep.Transform;
				else return levelGeometryEntity.InitialMovementStep.Transform;
			}
		}

		public static bool EntityIsBeingEdited(LevelGeometryEntity levelGeometryEntity) {
			lock (staticMutationLock) {
				return openEntityForms.ContainsKey(levelGeometryEntity);
			}
		}

		private void CreateEntity() {
			LevelGeometry geom = geomList.SelectedItem as LevelGeometry;
			LevelMaterial mat = materialList.SelectedItem as LevelMaterial;
			if (geom == null || mat == null) {
				MessageBox.Show(this, "Please select a geometry primitive and a material from the lists on the left first.");
				return;
			}

			lock (staticMutationLock) {
				int id = currentLevel.GetNewEntityID();
				Vector3 initialPosition = Vector3.ZERO;
				var selectedEntity = entityList.SelectedItem as LevelGeometryEntity;
				if (selectedEntity != null) initialPosition = selectedEntity.InitialMovementStep.Transform.Translation;
				LevelEntityMovementStep lems = new LevelEntityMovementStep(Transform.DEFAULT_TRANSFORM.TranslateBy(initialPosition), 0f, false);
				currentLevel.AddEntity(new LevelGeometryEntity(currentLevel, "Entity " + id, geom, mat, id, lems));
			}
			
			UpdateEntityList();
			RedrawEntities();
		}

		private void ActionOnSelectedEntity(SelectedEditorItemAction action) {
			lock (staticMutationLock) {
				IEnumerable<LevelGeometryEntity> selectedEntities = entityList.SelectedItems.Cast<LevelGeometryEntity>().ToList(); // To stop concurrent modification problems
				int numSelectedItems = selectedEntities.Count();
				if (numSelectedItems > 0) {
					switch (action) {
						case SelectedEditorItemAction.Delete:
							DialogResult deleteResult = MessageBox.Show(
								this,
								"Are you sure you wish to DELETE " + numSelectedItems + " entities?",
								"Delete Entities",
								MessageBoxButtons.OKCancel
								);
							if (deleteResult == DialogResult.OK) {
								LosgapSystem.InvokeOnMasterAsync(() => {
									lock (staticMutationLock) {
										foreach (var levelEntity in selectedEntities) {
											currentLevel.DeleteEntity(levelEntity);
										}
									}
									UpdateEntityList();
									RedrawEntities();
								});
							}
							break;
						case SelectedEditorItemAction.Clone:
							List<LevelGeometryEntity> clones = new List<LevelGeometryEntity>();
							lock (staticMutationLock) {
								foreach (var levelEntity in selectedEntities) {
									LevelGeometryEntity clone = levelEntity.Clone(currentLevel.GetNewEntityID());
									clones.Add(clone);
									currentLevel.AddEntity(clone);
									if (currentLevel is GameLevelDescription) {
										GameLevelDescription curLevelAsGameLevel = (GameLevelDescription) currentLevel;
										foreach (LevelGameObject gameObject in curLevelAsGameLevel.GameObjects) {
											if (gameObject.GroundingGeometryEntity == levelEntity) {
												var goClone = gameObject.Clone(curLevelAsGameLevel.GetNewGameObjectID());
												goClone.GroundingGeometryEntity = clone;
												curLevelAsGameLevel.AddGameObject(goClone);
												if (openMiscForm != null) openMiscForm.UpdateObjectList();
											}
										}
									}
								}	
							}

							RedrawEntities();
							UpdateEntityList();
							BeginInvoke((Action) (() => {
								entityList.ClearSelected();
								foreach (LevelGeometryEntity clone in clones) {
									entityList.SelectedItems.Add(clone);
								}
							}));
							break;
						case SelectedEditorItemAction.Edit:
							lock (staticMutationLock) {
								foreach (var levelEntity in selectedEntities) {
									if (openEntityForms.ContainsKey(levelEntity)) {
										openEntityForms[levelEntity].BringToFront();
										continue;
									}
									EntityEditForm editForm = new EntityEditForm(levelEntity);
									openEntityForms.Add(levelEntity, editForm);
									editForm.FormClosed += (sender, args) => {
										LevelDescription currentLevelLocal;
										lock (staticMutationLock) {
											currentLevelLocal = currentLevel;
											openEntityForms.Remove(levelEntity);
										}
										currentLevelLocal.ResetEntity(levelEntity);
									};
									editForm.Show(this);
								}	
							}
							break;
					}

				}
				else MessageBox.Show(this, "Please select one or more items in the geometry list first.");
			}

		}

		public IEnumerable<LevelGeometryEntity> GetSelectedEntities() {
			lock (staticMutationLock) {
				return entityList.SelectedItems.Cast<LevelGeometryEntity>().ToList();
			}
		}

		private void SetMatOnSelectedEntities() {
			IEnumerable<LevelGeometryEntity> selectedEntities;
			lock (staticMutationLock) {
				selectedEntities = entityList.SelectedItems.Cast<LevelGeometryEntity>().ToList(); // To stop concurrent modification problems
			}
			LevelMaterial selectedResult;
			try {
				selectedResult = ListSelect.GetSelection(materialList.Items.Cast<LevelMaterial>(), null, true);
			}
			catch (OperationCanceledException) {
				return;
			}
			Dictionary<LevelGeometryEntity, LevelMaterial> affectedLGEs = new Dictionary<LevelGeometryEntity, LevelMaterial>();
			foreach (var entity in selectedEntities) {
				affectedLGEs.Add(entity, entity.Material);
				entity.Material = selectedResult;
				currentLevel.ResetEntity(entity);
			}

			reversionStepType = ReversionStepType.Action;
			reversionActionDesc = "Set Materials";
			reversionAction = () => {
				foreach (var kvp in affectedLGEs) {
					kvp.Key.Material = kvp.Value;
				}
				currentLevel.ResetEntities(false);
			};
			StartNewReversionStep();
		}

		private void SetGeomOnSelectedEntities() {
			IEnumerable<LevelGeometryEntity> selectedEntities;
			lock (staticMutationLock) {
				selectedEntities = entityList.SelectedItems.Cast<LevelGeometryEntity>().ToList(); // To stop concurrent modification problems
			}
			LevelGeometry selectedResult;
			try {
				selectedResult = ListSelect.GetSelection(geomList.Items.Cast<LevelGeometry>(), null, true);
			}
			catch (OperationCanceledException) {
				return;
			}
			Dictionary<LevelGeometryEntity, LevelGeometry> affectedLGEs = new Dictionary<LevelGeometryEntity, LevelGeometry>();
			foreach (var entity in selectedEntities) {
				affectedLGEs.Add(entity, entity.Geometry);
				entity.Geometry = selectedResult;
				currentLevel.ResetEntity(entity);
			}

			reversionStepType = ReversionStepType.Action;
			reversionActionDesc = "Set Geometry";
			reversionAction = () => {
				foreach (var kvp in affectedLGEs) {
					kvp.Key.Geometry = kvp.Value;
				}
				currentLevel.ResetEntities(false);
			};
			StartNewReversionStep();
		}

		private void BakeSelectedEntitiesToGeometry(IEnumerable<LevelGeometryEntity> selection = null) {
			List<object> newSelection = new List<object>();
			lock (staticMutationLock) {
				IEnumerable<LevelGeometryEntity> selectedEntities = selection ?? entityList.SelectedItems.Cast<LevelGeometryEntity>();
				if (!selectedEntities.Any()) {
					MessageBox.Show(this, "Please select at least one geometryEntity first.", "Magic Texture Scaling");
					return;
				}
				if (selectedEntities.Any(e => openEntityForms.ContainsKey(e))) {
					MessageBox.Show(this, "Please close the geometryEntity edit forms for the selected entities first.", "Magic Texture Scaling");
					return;
				}

				Vector2 texScaling;
				try {
					texScaling = ListSelect.GetSelection(
						new[] {
							Vector2.ONE * 0.25f, 
							Vector2.ONE * 0.5f, 
							Vector2.ONE * 0.75f, 
							Vector2.ONE,
							Vector2.ONE * 1.25f, 
							Vector2.ONE * 1.5f, 
							Vector2.ONE * 1.75f, 
							Vector2.ONE * 2f, 
							Vector2.ONE * 3f, 
							Vector2.ONE * 4f, 
							Vector2.ONE * 5f
						},
						Vector2.ONE,
						true
						);
				}
				catch (OperationCanceledException) {
					return;
				}

				foreach (LevelGeometryEntity selectedEntity in selectedEntities) {
					LevelGeometry curGeom = selectedEntity.Geometry;
					LevelGeometry newGeom = null;
					if (curGeom is LevelGeometry_Cuboid) {
						Cuboid shapeDesc = ((LevelGeometry_Cuboid) curGeom).ShapeDesc;
						Vector3 sdScale = curGeom.Transform.Scale.Scale(selectedEntity.InitialMovementStep.Transform.Scale);
						Cuboid newShapeDesc = new Cuboid(
							shapeDesc.FrontBottomLeft.Scale(sdScale),
							shapeDesc.Width * sdScale.X,
							shapeDesc.Height * sdScale.Y,
							shapeDesc.Depth * sdScale.Z
						);
						newGeom = new LevelGeometry_Cuboid(
							texScaling,
							Vector3.ONE,
							curGeom.EulerRotations,
							curGeom.Transform.Translation,
							curGeom.IsInsideOut,
							newShapeDesc,
							currentLevel.GetNewGeometryID()
						);
					}
					else if (curGeom is LevelGeometry_Cone) {
						Cone shapeDesc = ((LevelGeometry_Cone) curGeom).ShapeDesc;
						Vector3 sdScale = curGeom.Transform.Scale.Scale(selectedEntity.InitialMovementStep.Transform.Scale);
						Cone newShapeDesc = new Cone(
							shapeDesc.TopCenter,
							shapeDesc.BottomRadius,
							shapeDesc.Height * sdScale.Y,
							shapeDesc.TopRadius
						);
						newGeom = new LevelGeometry_Cone(
							texScaling,
							new Vector3(sdScale.X, 1f, sdScale.Z),
							curGeom.EulerRotations,
							curGeom.Transform.Translation,
							curGeom.IsInsideOut,
							newShapeDesc,
							((LevelGeometry_Cone) curGeom).NumSides,
							currentLevel.GetNewGeometryID()
						);
					}
					else if (curGeom is LevelGeometry_Sphere) {
						Sphere shapeDesc = ((LevelGeometry_Sphere) curGeom).ShapeDesc;
						Vector3 sdScale = curGeom.Transform.Scale.Scale(selectedEntity.InitialMovementStep.Transform.Scale);
						Sphere newShapeDesc = new Sphere(
							shapeDesc.Center, shapeDesc.Radius
						);
						newGeom = new LevelGeometry_Sphere(
							texScaling,
							sdScale,
							curGeom.EulerRotations,
							curGeom.Transform.Translation,
							curGeom.IsInsideOut,
							newShapeDesc,
							((LevelGeometry_Sphere) curGeom).Extrapolation,
							currentLevel.GetNewGeometryID()
						);
					}
					else if (curGeom is LevelGeometry_Curve) {
						Vector3 sdScale = curGeom.Transform.Scale.Scale(selectedEntity.InitialMovementStep.Transform.Scale);
						MeshGenerator.CurveDesc shapeDesc = ((LevelGeometry_Curve) curGeom).ShapeDesc;
						newGeom = new LevelGeometry_Curve(
							texScaling,
							Vector3.ONE,
							curGeom.EulerRotations,
							curGeom.Transform.Translation,
							curGeom.IsInsideOut,
							new MeshGenerator.CurveDesc(
								new Cuboid(
									shapeDesc.StartingCuboid.FrontBottomLeft.Scale(sdScale),
									shapeDesc.StartingCuboid.Width * sdScale.X,
									shapeDesc.StartingCuboid.Height * sdScale.Y,
									shapeDesc.StartingCuboid.Depth * sdScale.Z
								), 
								shapeDesc.YawRotationRads,
								shapeDesc.PitchRotationRads,
								shapeDesc.RollRotationRads,
								shapeDesc.NumSegments
							), 
							currentLevel.GetNewGeometryID()
						);
					}
					else if (curGeom is LevelGeometry_Plane) {
						Plane shapeDesc = ((LevelGeometry_Plane) curGeom).ShapeDesc;
						Vector3 sdScale = curGeom.Transform.Scale.Scale(selectedEntity.InitialMovementStep.Transform.Scale);
						IEnumerable<Vector3> newCorners = ((LevelGeometry_Plane) curGeom).Corners
							.Select(old => (old - shapeDesc.CentrePoint).Scale(sdScale));
						newGeom = new LevelGeometry_Plane(
							texScaling,
							Vector3.ONE,
							curGeom.EulerRotations,
							curGeom.Transform.Translation,
							curGeom.IsInsideOut,
							newCorners.ToList(),
							currentLevel.GetNewGeometryID()
						);
					}
					else if (curGeom is LevelGeometry_Model) {
						Vector3 newScale = curGeom.Transform.Scale.Scale(selectedEntity.InitialMovementStep.Transform.Scale);
						newGeom = new LevelGeometry_Model(
							((LevelGeometry_Model) curGeom).ModelFileName,
							((LevelGeometry_Model) curGeom).MirrorX,
							newScale,
							currentLevel.GetNewGeometryID()
						);
					}

					if (currentLevel.LevelEntities.Any(e => e.Geometry == curGeom && e != selectedEntity)) {
						currentLevel.AddGeometry(newGeom);
					}
					else {
						currentLevel.ReplaceGeometry(curGeom, newGeom);
					}

					LevelGeometryEntity newGeometryEntity = new LevelGeometryEntity(
						currentLevel,
						selectedEntity.Tag,
						newGeom,
						selectedEntity.Material,
						currentLevel.GetNewEntityID(),
						new LevelEntityMovementStep(
							new Transform(
								Vector3.ONE, 
								selectedEntity.InitialMovementStep.Transform.Rotation, 
								selectedEntity.InitialMovementStep.Transform.Translation
							),
							selectedEntity.InitialMovementStep.TravelTime,
							selectedEntity.InitialMovementStep.SmoothTransition
						)
					);
					newGeometryEntity.AlternatingMovementDirection = selectedEntity.AlternatingMovementDirection;
					newGeometryEntity.InitialDelay = selectedEntity.InitialDelay;

					foreach (LevelEntityMovementStep lems in selectedEntity.MovementSteps.Skip(1)) {
						Vector3 lemsScale = lems.Transform.Scale;
						Vector3 imsScale = selectedEntity.InitialMovementStep.Transform.Scale;
						newGeometryEntity.AddMovementStep(
							new LevelEntityMovementStep(
								lems.Transform.With(scale: new Vector3(lemsScale.X / imsScale.X, lemsScale.Y / imsScale.Y, lemsScale.Z / imsScale.Z)),
								lems.TravelTime,
								lems.SmoothTransition
							)
						);
					}

					if (currentLevel is GameLevelDescription) {
						foreach (LevelGameObject lgo in ((GameLevelDescription) currentLevel).GameObjects.Where(lgo => lgo.GroundingGeometryEntity == selectedEntity)) {
							lgo.GroundingGeometryEntity = newGeometryEntity;
						}
					}

					currentLevel.ReplaceEntity(selectedEntity, newGeometryEntity);
					newSelection.Add(newGeometryEntity);
				}
			}

			UpdateUI();
			ReinitializeScene();

			lock (staticMutationLock) {
				foreach (object newEntity in newSelection) entityList.SelectedItems.Add(newEntity);
			}
		}

		private void PanSelectedEntities(UVPanDirection panDir) {
			IEnumerable<LevelGeometryEntity> selectedEntities;
			lock (staticMutationLock) {
				selectedEntities = entityList.SelectedItems.Cast<LevelGeometryEntity>().ToList(); // To stop concurrent modification problems
			}

			Vector2 texPan;
			switch (panDir) {
				case UVPanDirection.NegativeU: texPan = new Vector2((float) -uvPanAmountPicker.Value, 0f); break;
				case UVPanDirection.NegativeV: texPan = new Vector2(0f, (float) -uvPanAmountPicker.Value); break;
				case UVPanDirection.PositiveU: texPan = new Vector2((float) uvPanAmountPicker.Value, 0f); break;
				case UVPanDirection.PositiveV: texPan = new Vector2(0f, (float) uvPanAmountPicker.Value); break;
				default: texPan = Vector2.ZERO; break;
			}

			foreach (LevelGeometryEntity levelGeometryEntity in selectedEntities) {
				levelGeometryEntity.TexPan += texPan;
			}

			currentLevel.RecreateGeometry(true);

			foreach (LevelGeometryEntity levelGeometryEntity in selectedEntities) {
				currentLevel.ResetEntities(false);
			}

			TogglePauseMovers(pauseMovers);
		}
		#endregion

		private static void PostTick(float deltaTimeSecs) {
			Thread.MemoryBarrier();
			List<KeyValuePair<GeometryEntity, float>> entityFlashPairs = entityFlashes.ToList();
			foreach (KeyValuePair<GeometryEntity, float> kvp in entityFlashPairs) {
				float timeRemaining = kvp.Value - deltaTimeSecs;
				if (timeRemaining < 0f) {
					if (kvp.Key.IsDisposed) {
						entityFlashes.Remove(kvp.Key);
						return;
					}
					Transform t = kvp.Key.Transform;
					LevelGeometryEntity le = currentLevel.GetLevelEntityRepresentation(kvp.Key);
					currentLevel.ResetEntity(le);
					currentLevel.GetEntityRepresentation(le).Transform = t;
					entityFlashes.Remove(kvp.Key);
				}
				else entityFlashes[kvp.Key] = timeRemaining;
			}
			Thread.MemoryBarrier();
		}

		private void ShowMiscDialog() {
			lock (staticMutationLock) {
				if (openMiscForm != null) {
					openMiscForm.Focus();
					return;
				}
				if (openSkyLightForm != null) {
					openSkyLightForm.Focus();
					return;
				}


				if (currentLevel is SkyLevelDescription) {
					openSkyLightForm = new SkyLightListForm((SkyLevelDescription) currentLevel);
					openSkyLightForm.FormClosed += (sender, args) => {
						lock (staticMutationLock) {
							openSkyLightForm = null;
						}
					};
					openSkyLightForm.Show(this);
				}
				else {
					openMiscForm = new GameItemForm((GameLevelDescription) currentLevel);
					openMiscForm.FormClosed += (sender, args) => {
						lock (staticMutationLock) {
							openMiscForm = null;
						}
					};
					openMiscForm.Show(this);
				}
			}
		}

		#region View Update
		private void ReinitHackAct() {
			currentLevel.FlushCaches();
			currentLevel.ReinitializeAll();
			UpdateSelectedEntities();
			if (currentLevel is GameLevelDescription) {
				string skyFileName = ((GameLevelDescription) currentLevel).SkyboxFileName;
				if (skyFileName != null && (currentSky == null || skyFileName != currentSkyFilename)) {
					string fullSkyFilePath = Path.Combine(AssetLocator.LevelsDir, skyFileName);
					if (File.Exists(fullSkyFilePath)) {
						try {
							if (currentSky != null) currentSky.Dispose();
							currentSky = (SkyLevelDescription) LevelDescription.Load(fullSkyFilePath);
							currentSky.ReinitializeAll();
						}
						catch (Exception e) {
							Logger.Warn("Could not load skybox.", e);
						}
					}
					currentSkyFilename = skyFileName;
				}
			}
			StartNewReversionStep();
			RecalculateEntityExtents();
		}

		public void ReinitializeScene() {
			Action<float> reinitHackActRef = null;
			reinitHackActRef = _ => {
				ReinitHackAct();
				EntityModule.PostTick -= reinitHackActRef;
			};
			EntityModule.PostTick += reinitHackActRef;
		}

		public void RedrawEntities() {
			lock (staticMutationLock) {
				currentLevel.ResetEntities(false);
			}
			UpdateSelectedEntities();

			RecalculateEntityExtents();
		}

		private void RecalculateEntityExtents() {
			var curEntities = currentLevel.LevelEntities;
			foreach (var kvp in entityArrowCacheTransforms) {
				if (!curEntities.Contains(kvp.Key) || GetDisplayedTransformForEntity(kvp.Key) != kvp.Value) entityArrowDataCache.Remove(kvp.Key);
			}
			entityArrowCacheTransforms.Clear();
			foreach (LevelGeometryEntity entity in curEntities) {
				entityArrowCacheTransforms.Add(entity, GetDisplayedTransformForEntity(entity));
				if (entityArrowDataCache.ContainsKey(entity)) continue;
				List<DefaultVertex> vertices;
				List<uint> indices;
				LevelGeometry geom = entity.Geometry;
				geom.GetVertexData(out vertices, out indices);

				Transform entityTransform = GetDisplayedTransformForEntity(entity);
				Matrix entityMatrix = entityTransform.With(translation: Vector3.ZERO).AsMatrix;

				float minX = Single.MaxValue, minY = Single.MaxValue, minZ = Single.MaxValue, maxX = Single.MinValue, maxY = Single.MinValue, maxZ = Single.MinValue;
				IEnumerable<Vector3> vertexPositions = vertices.Select(vert => vert.Position * entityMatrix);
				foreach (Vector3 position in vertexPositions) {
					if (position.X < minX) minX = position.X;
					if (position.Y < minY) minY = position.Y;
					if (position.Z < minZ) minZ = position.Z;
					if (position.X > maxX) maxX = position.X;
					if (position.Y > maxY) maxY = position.Y;
					if (position.Z > maxZ) maxZ = position.Z;
				}

				float xExtent = maxX - minX;
				float yExtent = maxY - minY;
				float zExtent = maxZ - minZ;

				entityArrowDataCache.Add(entity, new[] { minX, minY, minZ, maxX, maxY, maxZ, xExtent, yExtent, zExtent });
			}
		}

		public void UpdateUI() {
			UpdateGeometryList();
			UpdateMaterialList();
			UpdateEntityList();
			StartNewReversionStep();
		}

		public void UpdateGeometryList() {
			BeginInvoke((Action) (() => {
				geomList.Items.Clear();
				IEnumerable<LevelGeometry> currentLevelGeometry;
				lock (staticMutationLock) {
					currentLevelGeometry = currentLevel.Geometry;
				}
				foreach (LevelGeometry geometry in currentLevelGeometry) {
					geomList.Items.Add(geometry);
				}
			}));
		}

		public void UpdateMaterialList() {
			BeginInvoke((Action) (() => {
				materialList.Items.Clear();
				IEnumerable<LevelMaterial> currentLevelMaterials;
				lock (staticMutationLock) {
					currentLevelMaterials = currentLevel.Materials;
				}
				foreach (LevelMaterial material in currentLevelMaterials) {
					materialList.Items.Add(material);
				}
			}));
		}

		public void UpdateEntityList() {
			BeginInvoke((Action)(() => {
				entityList.Items.Clear();
				IEnumerable<LevelGeometryEntity> currentLevelEntities;
				lock (staticMutationLock) {
					currentLevelEntities = currentLevel.LevelEntities;
				}
				foreach (LevelGeometryEntity entity in currentLevelEntities) {
					entityList.Items.Add(entity);
				}
			}));
		}

		public void UpdateSelectedEntities() {
			const int MIN_X = 0;
			const int MIN_Y = 1;
			const int MIN_Z = 2;
			const int MAX_X = 3;
			const int MAX_Y = 4;
			const int MAX_Z = 5;
			const int X_EXT = 6;
			const int Y_EXT = 7;
			const int Z_EXT = 8;

			lock (staticMutationLock) {
				foreach (var kvp in activeEntityArrows) {
					kvp.Value.Dispose();
				}

				activeEntityArrows.Clear();

				bool showMovementArrows = EditorMainWindowManager.EntityTranslationEnabled;
				bool showRotationArrows = EditorMainWindowManager.EntityRotationEnabled;
				bool showScalingArrows = EditorMainWindowManager.EntityScalingEnabled;

				if (!showMovementArrows && !showRotationArrows && !showScalingArrows) return;
				ModelHandle arrowModel = showMovementArrows ? translationArrowModelHandle : (showRotationArrows ? rotationArrowModelHandle : scaleArrowModelHandle);

				foreach (LevelGeometryEntity entity in entityList.SelectedItems.Cast<LevelGeometryEntity>()) {
					if (!entityArrowDataCache.ContainsKey(entity)) RecalculateEntityExtents();
					float[] modelExtents = entityArrowDataCache[entity];
					float arrowScale = Math.Max(modelExtents[X_EXT], Math.Max(modelExtents[Y_EXT], modelExtents[Z_EXT]));

					Transform entityTransform = GetDisplayedTransformForEntity(entity);

					GeometryEntity x = new GeometryEntity {
						Transform = new Transform(
							new Vector3(1f, (float) MathUtils.Clamp(arrowScale * 0.033f, 0.4f, 10f), (float) MathUtils.Clamp(arrowScale * 0.033f, 0.4f, 2f)),
							Quaternion.FromAxialRotation(Vector3.FORWARD, MathUtils.PI_OVER_TWO) * (showScalingArrows ? entityTransform.Rotation : Quaternion.IDENTITY),
							new Vector3(modelExtents[MAX_X], modelExtents[Y_EXT] * 0.5f + modelExtents[MIN_Y], modelExtents[Z_EXT] * 0.5f + modelExtents[MIN_Z]) + entityTransform.Translation
						)
					};
					x.SetModelInstance(AssetLocator.GameLayer, arrowModel, redMat);

					GeometryEntity y = new GeometryEntity {
						Transform = new Transform(
							new Vector3(1f, (float) MathUtils.Clamp(arrowScale * 0.033f, 0.4f, 10f), (float) MathUtils.Clamp(arrowScale * 0.033f, 0.4f, 2f)),
							(showScalingArrows ? entityTransform.Rotation : Quaternion.IDENTITY),
							new Vector3(modelExtents[X_EXT] * 0.5f + modelExtents[MIN_X], modelExtents[MAX_Y], modelExtents[Z_EXT] * 0.5f + modelExtents[MIN_Z]) + entityTransform.Translation
						)
					};
					y.SetModelInstance(AssetLocator.GameLayer, arrowModel, greenMat);

					GeometryEntity z = new GeometryEntity {
						Transform = new Transform(
							new Vector3(1f, (float) MathUtils.Clamp(arrowScale * 0.033f, 0.4f, 10f), (float) MathUtils.Clamp(arrowScale * 0.033f, 0.4f, 2f)),
							Quaternion.FromAxialRotation(Vector3.LEFT, MathUtils.PI_OVER_TWO) * (showScalingArrows ? entityTransform.Rotation : Quaternion.IDENTITY),
							new Vector3(modelExtents[X_EXT] * 0.5f + modelExtents[MIN_X], modelExtents[Y_EXT] * 0.5f + modelExtents[MIN_Y], modelExtents[MAX_Z]) + entityTransform.Translation
						)
					};
					z.SetModelInstance(AssetLocator.GameLayer, arrowModel, blueMat);

					activeEntityArrows.Add(entity, new ArrowVect(x, y, z));
				}
			}
			StartNewReversionStep();
		}
		#endregion


		private void SetDoFEnabled() {
			if (enableDoFCheckbox.Checked) {
				AssetLocator.LightPass.SetLensProperties(Config.DofLensFocalDist, Config.DofLensMaxBlurDist);
			}
			else {
				AssetLocator.LightPass.SetLensProperties(0f, float.MaxValue);
			}
		}
	}

	public enum ReversionStepType {
		Translation,
		Rotation,
		Scaling,
		Action
	}
}
