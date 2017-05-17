using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ophidian.Losgap;

namespace Egodystonic.EscapeLizards.Editor {
	public partial class MatDescForm : Form {
		public MatDescForm(string materialName, string textureFileName, string normalFileName, string specularFileName, string emissiveFileName, float specularPower, bool genMips, Func<string, string, string, string, string, float, bool, bool> okButtonClickedAction) {
			InitializeComponent();

			this.matNameField.Text = materialName;
			this.textureFileButton.Text = textureFileName;
			this.normalFileButton.Text = normalFileName;
			this.specularFileButton.Text = specularFileName;
			this.emissiveFileButton.Text = emissiveFileName;
			this.specPowerField.Value = (decimal) specularPower;
			this.generateMipsCheckBox.Checked = genMips;

			cancelButton.MouseClick += (sender, args) => Close();

			textureFileButton.MouseClick += (sender, args) => {
				OpenFileDialog fileSelectorDialog = new OpenFileDialog();
				fileSelectorDialog.AutoUpgradeEnabled = false; // This line required to make it not crash the entire application, gg microsoft
				fileSelectorDialog.ShowHelp = true;
				fileSelectorDialog.Multiselect = false;
				fileSelectorDialog.Filter = "Image Files (*.bmp, *.jpg, *.png, *.gif)|*.bmp;*.jpg;*.png;*.gif";
				fileSelectorDialog.InitialDirectory = AssetLocator.MaterialsDir;
				if (textureFileName != String.Empty) fileSelectorDialog.FileName = textureFileName;
				var dialogResult = fileSelectorDialog.ShowDialog(this);
				if (dialogResult != DialogResult.OK) return;

				if (!fileSelectorDialog.CheckFileExists) {
					MessageBox.Show(this, "Invalid file (does not exist).");
					return;
				}

				this.textureFileButton.Text = Path.GetFileName(fileSelectorDialog.FileName);

				string textureRoot = Path.Combine(
					Path.GetDirectoryName(this.textureFileButton.Text),
					Path.GetFileNameWithoutExtension(this.textureFileButton.Text)
				);
				string possibleN = textureRoot + "_n" + Path.GetExtension(this.textureFileButton.Text);
				string possibleS = textureRoot + "_s" + Path.GetExtension(this.textureFileButton.Text);
				string possibleE = textureRoot + "_e" + Path.GetExtension(this.textureFileButton.Text);
				if (normalFileButton.Text == String.Empty && File.Exists(possibleN)) normalFileButton.Text = possibleN;
				if (specularFileButton.Text == String.Empty && File.Exists(possibleS)) specularFileButton.Text = possibleS;
				if (emissiveFileButton.Text == String.Empty && File.Exists(possibleE)) emissiveFileButton.Text = possibleE;
			};

			normalFileButton.MouseClick += (sender, args) => {
				OpenFileDialog fileSelectorDialog = new OpenFileDialog();
				fileSelectorDialog.AutoUpgradeEnabled = false; // This line required to make it not crash the entire application, gg microsoft
				fileSelectorDialog.ShowHelp = true;
				fileSelectorDialog.Multiselect = false;
				fileSelectorDialog.Filter = "Image Files (*.bmp, *.jpg, *.png, *.gif)|*.bmp;*.jpg;*.png;*.gif";
				fileSelectorDialog.InitialDirectory = AssetLocator.MaterialsDir;
				if (normalFileName != String.Empty) fileSelectorDialog.FileName = normalFileName;
				var dialogResult = fileSelectorDialog.ShowDialog(this);
				if (dialogResult != DialogResult.OK) return;

				if (!fileSelectorDialog.CheckFileExists) {
					MessageBox.Show(this, "Invalid file (does not exist).");
					return;
				}

				this.normalFileButton.Text = Path.GetFileName(fileSelectorDialog.FileName);
			};

			specularFileButton.MouseClick += (sender, args) => {
				OpenFileDialog fileSelectorDialog = new OpenFileDialog();
				fileSelectorDialog.AutoUpgradeEnabled = false; // This line required to make it not crash the entire application, gg microsoft
				fileSelectorDialog.ShowHelp = true;
				fileSelectorDialog.Multiselect = false;
				fileSelectorDialog.Filter = "Image Files (*.bmp, *.jpg, *.png, *.gif)|*.bmp;*.jpg;*.png;*.gif";
				fileSelectorDialog.InitialDirectory = AssetLocator.MaterialsDir;
				if (specularFileName != String.Empty) fileSelectorDialog.FileName = specularFileName;
				var dialogResult = fileSelectorDialog.ShowDialog(this);
				if (dialogResult != DialogResult.OK) return;

				if (!fileSelectorDialog.CheckFileExists) {
					MessageBox.Show(this, "Invalid file (does not exist).");
					return;
				}

				this.specularFileButton.Text = Path.GetFileName(fileSelectorDialog.FileName);
			};

			emissiveFileButton.MouseClick += (sender, args) => {
				OpenFileDialog fileSelectorDialog = new OpenFileDialog();
				fileSelectorDialog.AutoUpgradeEnabled = false; // This line required to make it not crash the entire application, gg microsoft
				fileSelectorDialog.ShowHelp = true;
				fileSelectorDialog.Multiselect = false;
				fileSelectorDialog.Filter = "Image Files (*.bmp, *.jpg, *.png, *.gif)|*.bmp;*.jpg;*.png;*.gif";
				fileSelectorDialog.InitialDirectory = AssetLocator.MaterialsDir;
				if (specularFileName != String.Empty) fileSelectorDialog.FileName = emissiveFileName;
				var dialogResult = fileSelectorDialog.ShowDialog(this);
				if (dialogResult != DialogResult.OK) return;

				if (!fileSelectorDialog.CheckFileExists) {
					MessageBox.Show(this, "Invalid file (does not exist).");
					return;
				}

				this.emissiveFileButton.Text = Path.GetFileName(fileSelectorDialog.FileName);
			};

			okButton.MouseClick += (sender, args) => {
				bool inputsAccepted = okButtonClickedAction(
					matNameField.Text,
					textureFileButton.Text, 
					normalFileButton.Text == String.Empty ? null : normalFileButton.Text,
					specularFileButton.Text == String.Empty ? null : specularFileButton.Text,
					emissiveFileButton.Text == String.Empty ? null : emissiveFileButton.Text,
					(float) specPowerField.Value,
					generateMipsCheckBox.Checked
				);

				if (!inputsAccepted) MessageBox.Show(this, "One or more values are invalid. Please correct any mistakes and try again.");
				else Close();
			};
		}

		public MatDescForm(Func<string, string, string, string, string, float, bool, bool> okButtonClickedAction) : this(String.Empty, String.Empty, String.Empty, String.Empty, String.Empty, 0.8f, true, okButtonClickedAction) { }
	}
}
