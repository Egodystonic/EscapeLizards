using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ophidian.Losgap;
using Ophidian.Losgap.Entities;

namespace Egodystonic.EscapeLizards.Editor {
	public partial class GameItemForm : Form {
		private const decimal DEFAULT_TRANSLATION_ADJUSTMENT = 0.35m * (decimal) PhysicsManager.ONE_METRE_SCALED;
		private const decimal DEFAULT_SCALING_ADJUSTMENT = 0.2m;
		private const decimal DEFAULT_ROTATION_ADJUSTMENT = (decimal) MathUtils.PI_OVER_TWO * 0.25m;
		private const decimal MIN_TRANSLATION_ADJUSTMENT = 0.01m * (decimal) PhysicsManager.ONE_METRE_SCALED;
		private const decimal MIN_SCALING_ADJUSTMENT = 0.1m;
		private const decimal MIN_ROTATION_ADJUSTMENT = (decimal) MathUtils.PI_OVER_TWO * 0.125m * 0.25m;
		private const decimal MAX_TRANSLATION_ADJUSTMENT = 4.89m * (decimal) PhysicsManager.ONE_METRE_SCALED;
		private const decimal MAX_SCALING_ADJUSTMENT = 1m;
		private const decimal MAX_ROTATION_ADJUSTMENT = (decimal) MathUtils.PI_OVER_TWO;
		private const decimal TRANSLATION_ADJUSTMENT_STEP = 0.085m * (decimal) PhysicsManager.ONE_METRE_SCALED;
		private const decimal SCALING_ADJUSTMENT_STEP = 0.1m;
		private const decimal ROTATION_ADJUSTMENT_STEP = (decimal) MathUtils.PI_OVER_TWO * 0.125m * 0.25m;
		private const int ADJUSTMENT_PICKER_NUM_DP = 3;
		private readonly object instanceMutationLock = new object();
		private readonly ColorDialog colorDialog = new ColorDialog();
		private readonly GameLevelDescription currentLevel;
		private LevelGameObject previouslySelectedLGO;

		public GameItemForm(GameLevelDescription currentLevel) {
			this.currentLevel = currentLevel;
			InitializeComponent();

			translationAmountPicker.DecimalPlaces = scalingAmountPicker.DecimalPlaces = rotationAmountPicker.DecimalPlaces = ADJUSTMENT_PICKER_NUM_DP;
			translationAmountPicker.Minimum = MIN_TRANSLATION_ADJUSTMENT;
			translationAmountPicker.Maximum = MAX_TRANSLATION_ADJUSTMENT;
			translationAmountPicker.Value = DEFAULT_TRANSLATION_ADJUSTMENT;
			translationAmountPicker.Increment = TRANSLATION_ADJUSTMENT_STEP;
			scalingAmountPicker.Minimum = MIN_SCALING_ADJUSTMENT;
			scalingAmountPicker.Maximum = MAX_SCALING_ADJUSTMENT;
			scalingAmountPicker.Value = DEFAULT_SCALING_ADJUSTMENT;
			scalingAmountPicker.Increment = SCALING_ADJUSTMENT_STEP;
			rotationAmountPicker.Minimum = (decimal) MathUtils.RadToDeg((float) MIN_ROTATION_ADJUSTMENT);
			rotationAmountPicker.Maximum = (decimal) MathUtils.RadToDeg((float) MAX_ROTATION_ADJUSTMENT);
			rotationAmountPicker.Value = (decimal) MathUtils.RadToDeg((float) DEFAULT_ROTATION_ADJUSTMENT);
			rotationAmountPicker.Increment = (decimal) MathUtils.RadToDeg((float) ROTATION_ADJUSTMENT_STEP);

			SetGOControlEnabledState(false);
			objectList.SelectedValueChanged += (sender, args) => UpdateSelectedObject();
			UpdateObjectList();

			bindButton.MouseClick += (sender, args) => BindSelectedGO();
			simpleBindingCheck.MouseClick += (sender, args) => ChangeBindingSimplicity();
			cloneButton.MouseClick += (sender, args) => CloneSelectedGO();
			deleteButton.MouseClick += (sender, args) => DeleteSelectedGO();

			tfmBtn_trXNeg.MouseClick += (_, __) => AdjustSelectedGOTransform(TransformAdjustmentType.Translation, TransformAdjustmentAxis.X, true);
			tfmBtn_trXPos.MouseClick += (_, __) => AdjustSelectedGOTransform(TransformAdjustmentType.Translation, TransformAdjustmentAxis.X, false);
			tfmBtn_trYNeg.MouseClick += (_, __) => AdjustSelectedGOTransform(TransformAdjustmentType.Translation, TransformAdjustmentAxis.Y, true);
			tfmBtn_trYPos.MouseClick += (_, __) => AdjustSelectedGOTransform(TransformAdjustmentType.Translation, TransformAdjustmentAxis.Y, false);
			tfmBtn_trZNeg.MouseClick += (_, __) => AdjustSelectedGOTransform(TransformAdjustmentType.Translation, TransformAdjustmentAxis.Z, true);
			tfmBtn_trZPos.MouseClick += (_, __) => AdjustSelectedGOTransform(TransformAdjustmentType.Translation, TransformAdjustmentAxis.Z, false);

			tfmBtn_scXDec.MouseClick += (_, __) => AdjustSelectedGOTransform(TransformAdjustmentType.Scale, TransformAdjustmentAxis.X, true);
			tfmBtn_scXInc.MouseClick += (_, __) => AdjustSelectedGOTransform(TransformAdjustmentType.Scale, TransformAdjustmentAxis.X, false);
			tfmBtn_scYDec.MouseClick += (_, __) => AdjustSelectedGOTransform(TransformAdjustmentType.Scale, TransformAdjustmentAxis.Y, true);
			tfmBtn_scYInc.MouseClick += (_, __) => AdjustSelectedGOTransform(TransformAdjustmentType.Scale, TransformAdjustmentAxis.Y, false);
			tfmBtn_scZDec.MouseClick += (_, __) => AdjustSelectedGOTransform(TransformAdjustmentType.Scale, TransformAdjustmentAxis.Z, true);
			tfmBtn_scZInc.MouseClick += (_, __) => AdjustSelectedGOTransform(TransformAdjustmentType.Scale, TransformAdjustmentAxis.Z, false);

			tfmBtn_rotXAnt.MouseClick += (_, __) => AdjustSelectedGOTransform(TransformAdjustmentType.Rotation, TransformAdjustmentAxis.X, true);
			tfmBtn_rotXClo.MouseClick += (_, __) => AdjustSelectedGOTransform(TransformAdjustmentType.Rotation, TransformAdjustmentAxis.X, false);
			tfmBtn_rotYAnt.MouseClick += (_, __) => AdjustSelectedGOTransform(TransformAdjustmentType.Rotation, TransformAdjustmentAxis.Y, true);
			tfmBtn_rotYClo.MouseClick += (_, __) => AdjustSelectedGOTransform(TransformAdjustmentType.Rotation, TransformAdjustmentAxis.Y, false);
			tfmBtn_rotZAnt.MouseClick += (_, __) => AdjustSelectedGOTransform(TransformAdjustmentType.Rotation, TransformAdjustmentAxis.Z, true);
			tfmBtn_rotZClo.MouseClick += (_, __) => AdjustSelectedGOTransform(TransformAdjustmentType.Rotation, TransformAdjustmentAxis.Z, false);

			snapToCameraButton.MouseClick += (_, __) => SnapSelectedToCamera();
			moveCamToObjButton.MouseClick += (_, __) => MoveCameraToSelectedItem();

			FormClosing += (_, __) => objectList.SelectedIndex = -1;
		}

		public void UpdateObjectList() {
			lock (instanceMutationLock) {
				objectList.Items.Clear();
				foreach (LevelGameObject lgo in currentLevel.GameObjects) {
					objectList.Items.Add(lgo);
				}
			}
			UpdateSelectedObject();
		}

		public void CreateNewGameObject(Transform transform) {
			LevelGameObject result = null;
			lock (instanceMutationLock) {
				if (objTypRad_startFlag.Checked) {
					result = new StartFlag(currentLevel, currentLevel.GetNewGameObjectID());
				}
				else if (objTypRad_dynamicLight.Checked) {
					result = new DynamicLight(currentLevel, currentLevel.GetNewGameObjectID());
				}
				else if (objTypRad_introCam.Checked) {
					result = new IntroCameraAttracter(currentLevel, currentLevel.GetNewGameObjectID());
				}
				else if (objTypRad_finishingBell.Checked) {
					result = new FinishingBell(currentLevel, currentLevel.GetNewGameObjectID());
				}
				else if (objTypRad_vultureEgg.Checked) {
					result = new VultureEgg(currentLevel, currentLevel.GetNewGameObjectID());
				}
				else if (objTypRad_lizardCoin.Checked) {
					result = new LizardCoin(currentLevel, currentLevel.GetNewGameObjectID());
				}
				else if (objTypRad_Nothing.Checked) {
					return;
				}

				Assure.NotNull(result);

				result.Transform = transform;
				currentLevel.AddGameObject(result);
				currentLevel.ResetGameObjects();
				UpdateObjectList();
				objectList.SelectedItem = result;
			}
		}

		private void SetGOControlEnabledState(bool enabled) {
			lock (instanceMutationLock) {
				tfmBtn_trXNeg.Enabled = tfmBtn_trXPos.Enabled =
				tfmBtn_trYNeg.Enabled = tfmBtn_trYPos.Enabled =
				tfmBtn_trZNeg.Enabled = tfmBtn_trZPos.Enabled =
				tfmBtn_scXInc.Enabled = tfmBtn_scXDec.Enabled =
				tfmBtn_scYInc.Enabled = tfmBtn_scYDec.Enabled =
				tfmBtn_scZInc.Enabled = tfmBtn_scZDec.Enabled =
				tfmBtn_rotXClo.Enabled = tfmBtn_rotXAnt.Enabled =
				tfmBtn_rotYClo.Enabled = tfmBtn_rotYAnt.Enabled =
				tfmBtn_rotZClo.Enabled = tfmBtn_rotZAnt.Enabled =
				simpleBindingCheck.Enabled = bindButton.Enabled = cloneButton.Enabled = deleteButton.Enabled = 
				translationAmountPicker.Enabled = scalingAmountPicker.Enabled = rotationAmountPicker.Enabled =
				moveCamToObjButton.Enabled =
				enabled;

				if (objectList.SelectedItem is Shadowcaster) {
					simpleBindingCheck.Enabled = bindButton.Enabled = cloneButton.Enabled = deleteButton.Enabled = false;
				}
			}
		}

		private void BindSelectedGO() {
			LevelGameObject selectedGO = objectList.SelectedItem as LevelGameObject;
			if (selectedGO == null) {
				MessageBox.Show(this, "Please select a game object from the list first.", "Bind geometryEntity to game object");
				return;
			}
			if (selectedGO is Shadowcaster) return;

			if (selectedGO.GroundingGeometryEntity != null) {
				var dialogResult = MessageBox.Show(
					this,
					"Are you sure you wish to unbind the selected " + selectedGO + " " +
						"from its grounding geometryEntity '" + selectedGO.GroundingGeometryEntity.Tag + "'?",
						"Unbind geometryEntity from game object", 
						MessageBoxButtons.OKCancel
					);
				if (dialogResult == DialogResult.OK) {
					selectedGO.GroundingGeometryEntity = null;
					UpdateSelectedObject();
				}
				return;
			}

			var currentlySelectedEntities = EditorToolbar.Instance.GetSelectedEntities();
			if (!currentlySelectedEntities.Any()) {
				MessageBox.Show(this, "Please select an geometryEntity in the toolbar first.", "Bind geometryEntity to game object");
				return;
			}
			
			LevelGeometryEntity selectedGeometryEntity;
			if (currentlySelectedEntities.Count() == 1) selectedGeometryEntity = currentlySelectedEntities.Single();
			else {
				try {
					selectedGeometryEntity = ListSelect.GetSelection(currentlySelectedEntities, throwOnCancellation: true);
				}
				catch (OperationCanceledException) {
					return;
				}
			}

			var diagResult = MessageBox.Show(
				this,
				"Are you sure you wish to bind the selected " + selectedGO + " " +
					"to the geometryEntity '" + selectedGeometryEntity.Tag + "'?",
					"Bind geometryEntity to game object",
					MessageBoxButtons.OKCancel
				);
			if (diagResult == DialogResult.OK) {
				selectedGO.GroundingGeometryEntity = selectedGeometryEntity;
				selectedGO.UseSimpleGrounding = simpleBindingCheck.Checked;
				currentLevel.ResetGameObjects();
				UpdateSelectedObject();
			}
		}

		private void ChangeBindingSimplicity() {
			LevelGameObject selectedGO = objectList.SelectedItem as LevelGameObject;
			if (selectedGO == null) {
				MessageBox.Show(this, "Please select a game object from the list first.", "Bind geometryEntity to game object");
				return;
			}
			if (selectedGO is Shadowcaster) return;

			selectedGO.UseSimpleGrounding = simpleBindingCheck.Checked;
			UpdateSelectedObject();
		}

		private void CloneSelectedGO() {
			LevelGameObject selectedGO = objectList.SelectedItem as LevelGameObject;
			if (selectedGO == null) {
				MessageBox.Show(this, "Please select a game object from the list first.", "Clone game object");
				return;
			}
			if (selectedGO is Shadowcaster) return;

			var diagResult = MessageBox.Show(
				this,
				"Are you sure you wish to clone the selected " + selectedGO + "?",
					"Clone game object",
					MessageBoxButtons.OKCancel
				);
			if (diagResult == DialogResult.OK) {
				LevelGameObject clone = selectedGO.Clone(currentLevel.GetNewGameObjectID());
				currentLevel.AddGameObject(clone);
				currentLevel.ResetGameObjects();
				UpdateObjectList();
				objectList.SelectedItem = clone;
			}
		}

		private void DeleteSelectedGO() {
			LevelGameObject selectedGO = objectList.SelectedItem as LevelGameObject;
			if (selectedGO == null) {
				MessageBox.Show(this, "Please select a game object from the list first.", "Delete game object");
				return;
			}
			if (selectedGO is Shadowcaster) return;

			var diagResult = MessageBox.Show(
				this,
				"Are you sure you wish to delete the selected " + selectedGO + "?",
					"Delete game object",
					MessageBoxButtons.OKCancel
				);
			if (diagResult == DialogResult.OK) {
				currentLevel.DeleteGameObject(selectedGO);
				currentLevel.ResetGameObjects();
				UpdateObjectList();
			}
		}

		private void UpdateSelectedObject() {
			lock (instanceMutationLock) {
				if (previouslySelectedLGO != null && previouslySelectedLGO.GroundingGeometryEntity != null) {
					Entity prevEntity = currentLevel.GetEntityRepresentation(previouslySelectedLGO.GroundingGeometryEntity);
					if (prevEntity is PresetMovementEntity) ((PresetMovementEntity) prevEntity).Unpause();
				}
				LevelGameObject previousGO = previouslySelectedLGO;
				LevelGameObject selectedGO = objectList.SelectedItem as LevelGameObject;
				if (selectedGO == null) {
					curObjDescTextBox.Text = String.Empty;
					SetGOControlEnabledState(false);
					previouslySelectedLGO = null;
					return;
				}
				if (selectedGO.GroundingGeometryEntity != null && !selectedGO.GroundingGeometryEntity.IsStatic) {
					PresetMovementEntity pme = (PresetMovementEntity) currentLevel.GetEntityRepresentation(selectedGO.GroundingGeometryEntity);
					pme.Pause(selectedGO.GroundingGeometryEntity.InitialMovementStep);
				}

				previouslySelectedLGO = selectedGO;
				curObjDescTextBox.Text = selectedGO.LongDescription;
				SetGOControlEnabledState(true);
				bindButton.Text = selectedGO.GroundingGeometryEntity == null ? "Bind" : "Unbind";
				simpleBindingCheck.Checked = selectedGO.UseSimpleGrounding;

				UpdateCustomPropertiesBox(previousGO, selectedGO);
			}
		}

		private void UpdateCustomPropertiesBox(LevelGameObject previous, LevelGameObject current) {
			ApplyCustomProperties(previous);

			specPropTable.Controls.Clear();
			specPropTable.RowStyles.Clear();

			if (current is DynamicLight) {
				Vector3 colorAsVec = (current as DynamicLight).LightColor;
				Color lightColor = Color.FromArgb((int) (colorAsVec.X * 255f), (int) (colorAsVec.Y * 255f), (int) (colorAsVec.Z * 255f));
				specPropTable.Controls.AddRange(new Control[] {
					new Label {
						Text = "Light Radius"
					}, 
					new NumericUpDown {
						Minimum = (decimal) (PhysicsManager.ONE_METRE_SCALED * 0.5f), 
						Maximum = (decimal) (PhysicsManager.ONE_METRE_SCALED * 10.0f),
						Increment = (decimal) (PhysicsManager.ONE_METRE_SCALED * 0.5f),
						Value = (decimal) MathUtils.Clamp((current as DynamicLight).LightRadius, (PhysicsManager.ONE_METRE_SCALED * 0.5f), (PhysicsManager.ONE_METRE_SCALED * 10.0f))
					},

					new Label {
						Text = "Light Colour"
					},
					new Button {
						FlatStyle = FlatStyle.Flat,
						BackColor = lightColor
					},

					new Label {
						Text = "Lum. Mult."
					},
					new NumericUpDown {
						Minimum = 1.0m, 
						Maximum = 100.0m,
						Increment = 0.3m,
						Value = (decimal) (current as DynamicLight).LuminanceMultiplier,
						DecimalPlaces = 1
					},
				});
				((NumericUpDown) specPropTable.GetControlFromPosition(1, 0)).ValueChanged += (sender, args) => ApplyCustomProperties(current);
				Button lightColorButton = (Button) specPropTable.GetControlFromPosition(1, 1);
				((NumericUpDown) specPropTable.GetControlFromPosition(1, 2)).ValueChanged += (sender, args) => ApplyCustomProperties(current);
				lightColorButton.MouseClick += (sender, args) => {
					colorDialog.Color = lightColorButton.BackColor;
					colorDialog.ShowDialog(this);
					lightColorButton.BackColor = colorDialog.Color;
					ApplyCustomProperties(current);
				};

			}
			else if (current is Shadowcaster) {
				specPropTable.Controls.AddRange(new Control[] {
					new Label {
						Text = "Res. Mult."
					}, 
					new NumericUpDown {
						Minimum = (decimal) 0.1f, 
						Maximum = (decimal) 2f, 
						Increment = (decimal) 0.1f,
						Value = (decimal) (current as Shadowcaster).ResolutionMultiplier,
						DecimalPlaces = 1
					},
					new Label {
						Text = "Depth"
					}, 
					new TrackBar {
						Minimum = 1,
						Maximum = 300, 
						Value = (int) (current as Shadowcaster).DepthOffset
					},
				});
				((NumericUpDown) specPropTable.GetControlFromPosition(1, 0)).ValueChanged += (sender, args) => ApplyCustomProperties(current);
				((TrackBar) specPropTable.GetControlFromPosition(1, 1)).ValueChanged += (sender, args) => ApplyCustomProperties(current);
			}

			EditorToolbar.RealtimeUpdateGO(current);
		}

		private void ApplyCustomProperties(LevelGameObject selectedGO) {
			if (selectedGO == null) return;
			if (selectedGO is DynamicLight) {
				(selectedGO as DynamicLight).LightRadius = (float) ((NumericUpDown) specPropTable.GetControlFromPosition(1, 0)).Value;
				Color buttonBackColor = specPropTable.GetControlFromPosition(1, 1).BackColor;
				(selectedGO as DynamicLight).LightColor = new Vector3(buttonBackColor.R / 255f, buttonBackColor.G / 255f, buttonBackColor.B / 255f);
				(selectedGO as DynamicLight).LuminanceMultiplier = (float) ((NumericUpDown) specPropTable.GetControlFromPosition(1, 2)).Value;
			}
			else if (selectedGO is Shadowcaster) {
				(selectedGO as Shadowcaster).ResolutionMultiplier = (float) ((NumericUpDown) specPropTable.GetControlFromPosition(1, 0)).Value;
				(selectedGO as Shadowcaster).DepthOffset = ((TrackBar) specPropTable.GetControlFromPosition(1, 1)).Value;
			}
			EditorToolbar.RealtimeUpdateGO(selectedGO);
		}

		private void MoveCameraToSelectedItem() {
			LevelGameObject selectedGO = objectList.SelectedItem as LevelGameObject;
			if (selectedGO == null) {
				MessageBox.Show(this, "Please select a game object from the list first.", "Snap game object");
				return;
			}

			GroundableEntity entityRep = currentLevel.GetGameObjectRepresentation(selectedGO);
			Vector3 entityForwardOrient = Vector3.FORWARD * entityRep.Transform.Rotation;
			AssetLocator.MainCamera.Position = entityRep.Transform.Translation - entityForwardOrient.WithLength(PhysicsManager.ONE_METRE_SCALED * 0.2f);
			AssetLocator.MainCamera.Orient(entityForwardOrient, Vector3.UP);
		}

		private void SnapSelectedToCamera() {
			LevelGameObject selectedGO = objectList.SelectedItem as LevelGameObject;
			if (selectedGO == null) {
				MessageBox.Show(this, "Please select a game object from the list first.", "Snap game object");
				return;
			}

			GroundableEntity entityRep = currentLevel.GetGameObjectRepresentation(selectedGO);
			entityRep.Transform = selectedGO.Transform = selectedGO.Transform.With(
				rotation: Quaternion.FromVectorTransition(Vector3.UP, AssetLocator.MainCamera.Orientation),
				translation: AssetLocator.MainCamera.Position
			);

			if (selectedGO.GroundingGeometryEntity != null) currentLevel.ResetGameObject(selectedGO);
			UpdateSelectedObject();
		}

		private void AdjustSelectedGOTransform(TransformAdjustmentType type, TransformAdjustmentAxis axis, bool negative) {
			LevelGameObject selectedGO = objectList.SelectedItem as LevelGameObject;
			if (selectedGO == null) return;
			GroundableEntity entityRep = currentLevel.GetGameObjectRepresentation(selectedGO);
			float amount;
			switch (type) {
				case TransformAdjustmentType.Translation:
					amount = (float) translationAmountPicker.Value * (negative ? -1f : 1f);
					switch (axis) {
						case TransformAdjustmentAxis.X:
							entityRep.Transform = selectedGO.Transform = selectedGO.Transform.TranslateBy(new Vector3(amount, 0f, 0f));
							break;
						case TransformAdjustmentAxis.Y:
							entityRep.Transform = selectedGO.Transform = selectedGO.Transform.TranslateBy(new Vector3(0f, amount, 0f));
							break;
						case TransformAdjustmentAxis.Z:
							entityRep.Transform = selectedGO.Transform = selectedGO.Transform.TranslateBy(new Vector3(0f, 0f, amount));
							break;
					}
					break;
				case TransformAdjustmentType.Rotation:
					amount = MathUtils.DegToRad((float) rotationAmountPicker.Value * (negative ? -1f : 1f));
					switch (axis) {
						case TransformAdjustmentAxis.X:
							entityRep.Transform = selectedGO.Transform = selectedGO.Transform.RotateBy(Quaternion.FromAxialRotation(Vector3.RIGHT, amount));
							break;
						case TransformAdjustmentAxis.Y:
							entityRep.Transform = selectedGO.Transform = selectedGO.Transform.RotateBy(Quaternion.FromAxialRotation(Vector3.UP, amount));
							break;
						case TransformAdjustmentAxis.Z:
							entityRep.Transform = selectedGO.Transform = selectedGO.Transform.RotateBy(Quaternion.FromAxialRotation(Vector3.FORWARD, amount));
							break;
					}
					break;
				case TransformAdjustmentType.Scale:
					amount = (float) scalingAmountPicker.Value * (negative ? -1f : 1f);
					switch (axis) {
						case TransformAdjustmentAxis.X:
							entityRep.Transform = selectedGO.Transform = selectedGO.Transform.With(scale: selectedGO.Transform.Scale + new Vector3(amount, 0f, 0f));
							break;
						case TransformAdjustmentAxis.Y:
							entityRep.Transform = selectedGO.Transform = selectedGO.Transform.With(scale: selectedGO.Transform.Scale + new Vector3(0f, amount, 0f));
							break;
						case TransformAdjustmentAxis.Z:
							entityRep.Transform = selectedGO.Transform = selectedGO.Transform.With(scale: selectedGO.Transform.Scale + new Vector3(0f, 0f, amount));
							break;
					}
					break;
			}
			if (selectedGO.GroundingGeometryEntity != null) currentLevel.ResetGameObject(selectedGO);
			UpdateSelectedObject();
		}

		private enum TransformAdjustmentType {
			Scale,
			Rotation,
			Translation
		}

		private enum TransformAdjustmentAxis {
			X, Y, Z
		}
	}
}
