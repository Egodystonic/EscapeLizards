using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ophidian.Losgap;
using Ophidian.Losgap.Entities;

namespace Egodystonic.EscapeLizards.Editor {
	public partial class EntityEditForm : Form {
		private readonly object instanceMutationLock = new object();
		private readonly LevelGeometryEntity editTarget;
		private bool ignoreStepChanges = false;
		private LevelEntityMovementStep selectedMovementStep;

		public LevelEntityMovementStep SelectedMovementStep {
			get {
				lock (instanceMutationLock) {
					return selectedMovementStep;
				}
			}
		}

		public EntityEditForm(LevelGeometryEntity editTarget) {
			Assure.NotNull(editTarget);
			this.editTarget = editTarget;
			InitializeComponent();
			tagField.Text = editTarget.Tag;
			geometryButton.Text = editTarget.Geometry.GetShortDesc();
			materialButton.Text = editTarget.Material.Name;
			this.Text = "Entity '" + editTarget.Tag + "'";
			alternateDirectionCheckbox.Checked = editTarget.AlternatingMovementDirection;
			initDelayBox.Value = (decimal) editTarget.InitialDelay;

			movementFrameList.DrawMode = DrawMode.OwnerDrawVariable;
			movementFrameList.MeasureItem += lst_MeasureItem;
			movementFrameList.DrawItem += lst_DrawItem;

			UpdateMovementData();

			movementFrameList.SelectedIndexChanged += (sender, args) => MovementListSelectionChange();

			movementFrameList.SelectedIndex = 0;

			addMovementFrameButton.MouseClick += (sender, args) => AddMovementFrame();
			deleteMovementFrameButton.MouseClick += (sender, args) => DeleteMovementFrame();

			travelTimeField.ValueChanged += (sender, args) => TravelTimeOrSmoothingChanged();
			smoothingCheckbox.CheckedChanged += (sender, args) => TravelTimeOrSmoothingChanged();
			initDelayBox.ValueChanged += (sender, args) => InitDelayChanged();

			alternateDirectionCheckbox.CheckedChanged += (sender, args) => AlternateDirectionChanged();

			geometryButton.MouseClick += (sender, args) => SelectGeometry();
			materialButton.MouseClick += (sender, args) => SelectMaterial();

			tagField.TextChanged += (sender, args) => {
				editTarget.Tag = tagField.Text;
			};

			this.FormClosed += (sender, args) => EditorToolbar.Instance.UpdateEntityList();
		}

		public void ReplaceSelectedMovementStep(LevelEntityMovementStep newLems) {
			lock (instanceMutationLock) {
				ignoreStepChanges = true;
				editTarget.ReplaceMovementStep(selectedMovementStep, newLems);
			}
			Invoke(new Action(() => {
				UpdateMovementData();
				movementFrameList.SelectedItem = newLems;
			}));
			lock (instanceMutationLock) {
				ignoreStepChanges = false;
			}
		}

		private void UpdateMovementData() {
			movementFrameList.Items.Clear();
			foreach (LevelEntityMovementStep lems in editTarget.MovementSteps) {
				movementFrameList.Items.Add(lems);
			}
		}

		private void MovementListSelectionChange() {
			lock (instanceMutationLock) {
				selectedMovementStep = ((LevelEntityMovementStep) movementFrameList.SelectedItem);
				travelTimeField.Value = (decimal) selectedMovementStep.TravelTime;
				smoothingCheckbox.Checked = selectedMovementStep.SmoothTransition;
				if (!ignoreStepChanges) EditorToolbar.SetEntityMovementStep(editTarget, selectedMovementStep);
			}
		}

		private void AddMovementFrame() {
			var selectedStep = movementFrameList.SelectedItem as LevelEntityMovementStep;
			if (selectedStep == null) selectedStep = editTarget.InitialMovementStep;
			LevelEntityMovementStep newStep = new LevelEntityMovementStep(selectedStep.Transform, 1f, false);
			editTarget.AddMovementStep(newStep, movementFrameList.SelectedItem as LevelEntityMovementStep);
			UpdateMovementData();
			movementFrameList.SelectedItem = newStep;
		}

		private void DeleteMovementFrame() {
			if (editTarget.IsStatic) {
				MessageBox.Show(this, "Can not delete the last movement step.");
				return;
			}
			LevelEntityMovementStep selectedStep = movementFrameList.SelectedItem as LevelEntityMovementStep;
			if (selectedStep == null) {
				MessageBox.Show(this, "Please select a movement step first.");
				return;
			}
			var dialogResult = MessageBox.Show(this, "Are you sure you wish to delete the selected movement frame?", "Delete movement frame", MessageBoxButtons.OKCancel);
			if (dialogResult == DialogResult.Cancel) return;
			editTarget.RemoveMovementStep(selectedStep);
			UpdateMovementData();
			movementFrameList.SelectedIndex = 0;			
		}

		private void TravelTimeOrSmoothingChanged() {
			var selectedStep = movementFrameList.SelectedItem as LevelEntityMovementStep;
			if (selectedStep == null) {
				movementFrameList.SelectedIndex = 0;
				return;
			}
			LevelEntityMovementStep replacement = new LevelEntityMovementStep(selectedStep.Transform, (float) travelTimeField.Value, smoothingCheckbox.Checked);
			editTarget.ReplaceMovementStep(selectedStep, replacement);
			UpdateMovementData();
			movementFrameList.SelectedItem = replacement;
		}

		private void InitDelayChanged() {
			editTarget.InitialDelay = (float) initDelayBox.Value;
		}

		private void AlternateDirectionChanged() {
			editTarget.AlternatingMovementDirection = alternateDirectionCheckbox.Checked;
		}

		private void SelectGeometry() {
			LevelGeometry newGeometry = ListSelect.GetSelection(editTarget.ParentLevel.Geometry, editTarget.Geometry);
			editTarget.Geometry = newGeometry;
			geometryButton.Text = newGeometry.GetShortDesc();
		}

		private void SelectMaterial() {
			LevelMaterial newMaterial = ListSelect.GetSelection(editTarget.ParentLevel.Materials, editTarget.Material);
			editTarget.Material = newMaterial;
			materialButton.Text = newMaterial.Name;
		}

		private void lst_MeasureItem(object sender, MeasureItemEventArgs e) {
			e.ItemHeight = (int) e.Graphics.MeasureString(movementFrameList.Items[e.Index].ToString(), movementFrameList.Font, movementFrameList.Width).Height;
		}

		private void lst_DrawItem(object sender, DrawItemEventArgs e) {
			if (e.Index < 0) return;
			e.DrawBackground();
			e.DrawFocusRectangle();
			e.Graphics.DrawString(movementFrameList.Items[e.Index].ToString(), e.Font, new SolidBrush(e.ForeColor), e.Bounds);
		}
	}
}
