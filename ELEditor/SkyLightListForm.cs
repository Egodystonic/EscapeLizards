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
	public partial class SkyLightListForm : Form {
		private const float POSITION_ADJUSTMENT_AMOUNT = PhysicsManager.ONE_METRE_SCALED * 0.25f;
		private readonly object instanceMutationLock = new object();
		private readonly SkyLevelDescription skyLevel;
		private readonly ColorDialog colorPickerDialog = new ColorDialog();
		private readonly Dictionary<LevelSkyLight, SkyLightVisualizationEntity> skyLightVisualizationEntities = new Dictionary<LevelSkyLight, SkyLightVisualizationEntity>();

		public SkyLightListForm(SkyLevelDescription skyLevel) {
			Assure.NotNull(skyLevel);
			InitializeComponent();

			this.skyLevel = skyLevel;
		}

		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);

			radiusUpDown.Minimum = (decimal) PhysicsManager.ONE_METRE_SCALED * 0.1m;
			radiusUpDown.Maximum = (decimal) PhysicsManager.ONE_METRE_SCALED * 7m;
			radiusUpDown.Increment = (decimal) PhysicsManager.ONE_METRE_SCALED * 0.1m;

			lightList.SelectedIndexChanged += (sender, args) => UpdateSelectedItem();
			RefreshSkyLightList();

			addButton.MouseClick += (sender, args) => {
				LevelSkyLight newLight = EditorToolbar.AddSkyLightAtCamera();
				RefreshSkyLightList();
				lightList.SelectedItem = newLight;
			};

			colorButton.MouseClick += (sender, args) => {
				colorPickerDialog.Color = colorButton.BackColor;
				colorPickerDialog.ShowDialog(this);
				colorButton.BackColor = colorPickerDialog.Color;
				ReplaceCurrentLight(Vector3.ZERO);
			};

			radiusUpDown.ValueChanged += (sender, args) => ReplaceCurrentLight(Vector3.ZERO);
			lumMulUpDown.ValueChanged += (sender, args) => ReplaceCurrentLight(Vector3.ZERO);

			leftButton.MouseClick += (sender, args) => ReplaceCurrentLight(Vector3.LEFT * POSITION_ADJUSTMENT_AMOUNT);
			rightButton.MouseClick += (sender, args) => ReplaceCurrentLight(Vector3.RIGHT * POSITION_ADJUSTMENT_AMOUNT);
			upButton.MouseClick += (sender, args) => ReplaceCurrentLight(Vector3.UP * POSITION_ADJUSTMENT_AMOUNT);
			downButton.MouseClick += (sender, args) => ReplaceCurrentLight(Vector3.DOWN * POSITION_ADJUSTMENT_AMOUNT);
			backButton.MouseClick += (sender, args) => ReplaceCurrentLight(Vector3.BACKWARD * POSITION_ADJUSTMENT_AMOUNT);
			foreButton.MouseClick += (sender, args) => ReplaceCurrentLight(Vector3.FORWARD * POSITION_ADJUSTMENT_AMOUNT);

			deleteButton.MouseClick += (sender, args) => {
				LevelSkyLight selectedSkyLight = lightList.SelectedItem as LevelSkyLight;
				if (selectedSkyLight == null) {
					Logger.Warn("Tried to delete null skylight.");
					return;
				}
				if (MessageBox.Show(this, "Are you sure you wish to delete this skylight?", "Delete light?", MessageBoxButtons.OKCancel) == DialogResult.Cancel) return;
				skyLevel.RemoveSkyLight(selectedSkyLight);
				RefreshSkyLightList();
				UpdateSelectedItem();
			};

			UpdateSelectedItem();
		}

		protected override void OnClosed(EventArgs e) {
			base.OnClosed(e);

			skyLightVisualizationEntities.Values.ForEach(ve => ve.Dispose());
		}

		public void RefreshSkyLightList() {
			lock (instanceMutationLock) {
				lightList.Items.Clear();
				skyLightVisualizationEntities.Values.ForEach(ve => ve.Dispose());
				skyLightVisualizationEntities.Clear();
				foreach (LevelSkyLight skyLight in skyLevel.SkyLights) {
					lightList.Items.Add(skyLight);
					skyLightVisualizationEntities[skyLight] = CreateVisualizationEntity(skyLight);
				}
			}
		}

		private SkyLightVisualizationEntity CreateVisualizationEntity(LevelSkyLight skyLight) {
			SkyLightVisualizationEntity result = new SkyLightVisualizationEntity();
			result.SetModelInstance(AssetLocator.SkyLayer, AssetLocator.DynamicLightModel, AssetLocator.DynamicLightMaterial);
			result.SetTranslation(skyLight.Position);
			result.SetScale(Vector3.ONE * 100f);
			return result;
		}

		public void UpdateSelectedItem() {
			lock (instanceMutationLock) {
				LevelSkyLight selectedSkyLight = lightList.SelectedItem as LevelSkyLight;
				if (selectedSkyLight == null) {
					colorButton.BackColor = Color.Transparent;
					colorButton.Enabled = false;
					radiusUpDown.Value = radiusUpDown.Minimum;
					radiusUpDown.Enabled = false;
					lumMulUpDown.Value = lumMulUpDown.Minimum;
					lumMulUpDown.Enabled = false;
					leftButton.Enabled = rightButton.Enabled =
					upButton.Enabled = downButton.Enabled =
					backButton.Enabled = foreButton.Enabled = false;
					deleteButton.Enabled = false;
				}
				else {
					colorButton.Enabled = true;
					radiusUpDown.Enabled = true;
					lumMulUpDown.Enabled = true;
					leftButton.Enabled = rightButton.Enabled =
					upButton.Enabled = downButton.Enabled =
					backButton.Enabled = foreButton.Enabled = true;
					deleteButton.Enabled = true;

					colorButton.BackColor = Color.FromArgb((int) (selectedSkyLight.Color.X * 255f), (int) (selectedSkyLight.Color.Y * 255f), (int) (selectedSkyLight.Color.Z * 255f));
					radiusUpDown.Value = (decimal) selectedSkyLight.Radius;
					lumMulUpDown.Value = (decimal) selectedSkyLight.LuminanceMultiplier;
				}
			}
		}

		public void ReplaceCurrentLight(Vector3 positionOffset) {
			lock (instanceMutationLock) {
				LevelSkyLight selectedSkyLight = lightList.SelectedItem as LevelSkyLight;
				if (selectedSkyLight == null) {
					Logger.Warn("Tried to replace null skylight.");
					return;
				}

				SkyLightVisualizationEntity visEnt = skyLightVisualizationEntities.Pop(selectedSkyLight);
				visEnt.Dispose();

				if (x4Radio.Checked) positionOffset *= 4f;
				else if (x20Radio.Checked) positionOffset *= 20f;

				LevelSkyLight replacement = new LevelSkyLight(
					selectedSkyLight.Position + positionOffset,
					new Vector3(colorButton.BackColor.R / 255f, colorButton.BackColor.G / 255f, colorButton.BackColor.B / 255f),
					(float) radiusUpDown.Value,
					(float) lumMulUpDown.Value
				);

				skyLevel.ReplaceSkyLight(selectedSkyLight, replacement);

				RefreshSkyLightList();

				lightList.SelectedItem = replacement;
			}
		}
	}
}
