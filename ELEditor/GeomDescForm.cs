using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ophidian.Losgap;

namespace Egodystonic.EscapeLizards.Editor {
	public partial class GeomDescForm : Form {
		private readonly Func<string[], Vector3, Vector3, Vector3, Vector2, bool, uint, bool> shapeSubmittedAction;
		private readonly int numCustomProps;
		private readonly TextBox[] transformFields = new TextBox[9];

		public GeomDescForm(Func<string[], Vector3, Vector3, Vector3, Vector2, bool, uint, bool> shapeSubmittedAction, string extrapolationText, params string[] shapeProperties) {
			Assure.NotNull(shapeSubmittedAction);
			InitializeComponent();
			numCustomProps = shapeProperties.Length;
			this.shapeSubmittedAction = shapeSubmittedAction;
			foreach (string shapeProperty in shapeProperties) {
				shapePropertiesPanel.Controls.Add(new Label { Text = shapeProperty + "  ", Dock = DockStyle.Fill, TextAlign = ContentAlignment.TopRight });
				shapePropertiesPanel.Controls.Add(new TextBox { Dock = DockStyle.Fill });
			}

			for (int i = 0; i < transformFields.Length; ++i) transformFields[i] = new TextBox { Dock = DockStyle.Fill };

			transformFields.ForEach((tf, i) => tf.Text = (i >= 3 && i < 6 ? "1.0" : "0.0"));
			shapePropertiesPanel.Controls.Add(new Label { Text = "Translation (X)  ", Dock = DockStyle.Fill, TextAlign = ContentAlignment.TopRight });
			shapePropertiesPanel.Controls.Add(transformFields[0]);
			shapePropertiesPanel.Controls.Add(new Label { Text = "Translation (Y)  ", Dock = DockStyle.Fill, TextAlign = ContentAlignment.TopRight });
			shapePropertiesPanel.Controls.Add(transformFields[1]);
			shapePropertiesPanel.Controls.Add(new Label { Text = "Translation (Z)  ", Dock = DockStyle.Fill, TextAlign = ContentAlignment.TopRight });
			shapePropertiesPanel.Controls.Add(transformFields[2]);
			shapePropertiesPanel.Controls.Add(new Label { Text = "Scale (X)  ", Dock = DockStyle.Fill, TextAlign = ContentAlignment.TopRight });
			shapePropertiesPanel.Controls.Add(transformFields[3]);
			shapePropertiesPanel.Controls.Add(new Label { Text = "Scale (Y)  ", Dock = DockStyle.Fill, TextAlign = ContentAlignment.TopRight });
			shapePropertiesPanel.Controls.Add(transformFields[4]);
			shapePropertiesPanel.Controls.Add(new Label { Text = "Scale (Z)  ", Dock = DockStyle.Fill, TextAlign = ContentAlignment.TopRight });
			shapePropertiesPanel.Controls.Add(transformFields[5]);
			shapePropertiesPanel.Controls.Add(new Label { Text = "Rotation (X)  ", Dock = DockStyle.Fill, TextAlign = ContentAlignment.TopRight });
			shapePropertiesPanel.Controls.Add(transformFields[6]);
			shapePropertiesPanel.Controls.Add(new Label { Text = "Rotation (Y)  ", Dock = DockStyle.Fill, TextAlign = ContentAlignment.TopRight });
			shapePropertiesPanel.Controls.Add(transformFields[7]);
			shapePropertiesPanel.Controls.Add(new Label { Text = "Rotation (Z)  ", Dock = DockStyle.Fill, TextAlign = ContentAlignment.TopRight });
			shapePropertiesPanel.Controls.Add(transformFields[8]);

			if (extrapolationText == null) extrapolationLabel.Visible = extrapolationTextBox.Visible = false;
			else extrapolationLabel.Text = extrapolationText;

			createButton.MouseClick += createButton_MouseClick;
			cancelButton.MouseClick += (sender, args) => Close();
		}

		public void SetDefaultText(string propertyLabel, string defaultText) {
			for (int i = 0; i < shapePropertiesPanel.Controls.Count; ++i) {
				if (shapePropertiesPanel.Controls[i] is Label && shapePropertiesPanel.Controls[i].Text == propertyLabel + "  ") {
					shapePropertiesPanel.Controls[i + 1].Text = defaultText;
				}
			}
		}

		public void SetExtrapolationValue(string value) {
			extrapolationTextBox.Text = value;
		}

		public void SetTexScaleValue(Vector2 texScale) {
			texScalingXTextBox.Text = texScale.X.ToString();
			texScalingYTextBox.Text = texScale.Y.ToString();
		}

		public void SetInsideOutValue(bool isInsideOut) {
			insideOutCheckBox.Checked = isInsideOut;
		}

		void createButton_MouseClick(object sender, MouseEventArgs e) {
			string[] shapePropValues = shapePropertiesPanel.Controls
				.OfType<Control>()
				.Where((c, i) => (i & 1) == 1)
				.Cast<TextBox>()
				.Select(tb => tb.Text)
				.Take(numCustomProps)
				.ToArray();

			try {
				Vector3 transformTranslation = new Vector3(
					float.Parse(transformFields[0].Text, CultureInfo.InvariantCulture),
					float.Parse(transformFields[1].Text, CultureInfo.InvariantCulture),
					float.Parse(transformFields[2].Text, CultureInfo.InvariantCulture)
					);
				Vector3 transformScale = new Vector3(
					float.Parse(transformFields[3].Text, CultureInfo.InvariantCulture),
					float.Parse(transformFields[4].Text, CultureInfo.InvariantCulture),
					float.Parse(transformFields[5].Text, CultureInfo.InvariantCulture)
					);
				Vector3 transformRotation = new Vector3(
					float.Parse(transformFields[6].Text, CultureInfo.InvariantCulture),
					float.Parse(transformFields[7].Text, CultureInfo.InvariantCulture),
					float.Parse(transformFields[8].Text, CultureInfo.InvariantCulture)
					);

				// ReSharper disable CompareOfFloatsByEqualityOperator
				if (transformScale.X == 0f || transformScale.Y == 0f || transformScale.Z == 0f) throw new ApplicationException("0-scale");
				// ReSharper restore CompareOfFloatsByEqualityOperator

				bool isValidShape = shapeSubmittedAction(
					shapePropValues,
					transformScale, 
					transformRotation, 
					transformTranslation,
					new Vector2(float.Parse(texScalingXTextBox.Text, CultureInfo.InvariantCulture), float.Parse(texScalingYTextBox.Text, CultureInfo.InvariantCulture)),
					insideOutCheckBox.Checked,
					uint.Parse(extrapolationTextBox.Text)
					);

				if (isValidShape) {
					Close();
					return;
				}
			}
			catch { }
			MessageBox.Show(this, "One or more invalid properties.");
		}


	}
}
