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
	public partial class LevelMetaEditForm : Form {
		private const string NO_SKYBOX_TEXT = "(none)";
		private readonly LevelDescription currentLevel;
		private readonly string currentLevelFile;
		private readonly Action<bool, string, string, string> commitChangesAction;

		public LevelMetaEditForm(Action<bool, string, string, string> commitChangesAction)
			: this(commitChangesAction, null, null) {
			Text = "Create New Level";
		}

		public LevelMetaEditForm(Action<bool, string, string, string> commitChangesAction, LevelDescription currentLevel, string currentLevelFile) {
			InitializeComponent();
			this.commitChangesAction = commitChangesAction;
			this.currentLevel = currentLevel;
			this.currentLevelFile = currentLevelFile;
			Text = "Edit Level Metadata";
		}

		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);

			if (currentLevel == null) {
				gameLevelRadio.Checked = true;
				skyboxButton.Text = NO_SKYBOX_TEXT;
			}
			else {
				if (currentLevel is GameLevelDescription) {
					gameLevelRadio.Checked = true;
					skyboxButton.Text = ((GameLevelDescription)currentLevel).SkyboxFileName ?? NO_SKYBOX_TEXT;
					goldSecsTextBox.Enabled = goldHundsTextBox.Enabled =
					silverSecsTextBox.Enabled = silverHundsTextBox.Enabled =
					bronzeSecsTextBox.Enabled = bronzeHundsTextBox.Enabled = true;
					UpdateTimeTextBoxes();
				}
				else {
					skyboxRadio.Checked = true;
					skyboxButton.Enabled = false;
					goldSecsTextBox.Enabled = goldHundsTextBox.Enabled =
					silverSecsTextBox.Enabled = silverHundsTextBox.Enabled =
					bronzeSecsTextBox.Enabled = bronzeHundsTextBox.Enabled = false;
				}
				gameLevelRadio.Enabled = skyboxRadio.Enabled = false;
				titleTextBox.Text = currentLevel.Title;
				filenameTextBox.Text = currentLevelFile;
			}

			okButton.MouseClick += okButton_MouseClick;
			cancelButton.MouseClick += (sender, args) => Close();
			skyboxButton.MouseClick += (sender, args) => SelectSkybox();
			gameLevelRadio.CheckedChanged += (sender, args) => skyboxButton.Enabled = gameLevelRadio.Checked;
		}

		void okButton_MouseClick(object sender, MouseEventArgs e) {
			if (!IOUtils.IsValidFilePath(filenameTextBox.Text)) {
				MessageBox.Show(this, "Invalid file name.");
				return;
			}
			if (titleTextBox.Text.IsNullOrWhiteSpace()) {
				MessageBox.Show(this, "Invalid title.");
				return;
			}

			GameLevelDescription curLevelAsGameLevel = currentLevel as GameLevelDescription;
			try {
				if (curLevelAsGameLevel != null) {
					curLevelAsGameLevel.LevelTimerGoldMs =
						(int) MathUtils.Clamp(Int32.Parse(goldSecsTextBox.Text), 0, 300) * 1000
						+ (int) MathUtils.Clamp(Int32.Parse(goldHundsTextBox.Text), 0, 100) * 10;

					curLevelAsGameLevel.LevelTimerSilverMs =
						(int) MathUtils.Clamp(Int32.Parse(silverSecsTextBox.Text), 0, 300) * 1000
						+ (int) MathUtils.Clamp(Int32.Parse(silverHundsTextBox.Text), 0, 100) * 10;

					curLevelAsGameLevel.LevelTimerBronzeMs =
						(int) MathUtils.Clamp(Int32.Parse(bronzeSecsTextBox.Text), 0, 300) * 1000
						+ (int) MathUtils.Clamp(Int32.Parse(bronzeHundsTextBox.Text), 0, 100) * 10;

					curLevelAsGameLevel.LevelTimerMaxMs =
						(int) MathUtils.Clamp(Int32.Parse(maxSecsTextBox.Text), 0, 300) * 1000
						+ (int) MathUtils.Clamp(Int32.Parse(maxHundsTextBox.Text), 0, 100) * 10;

					curLevelAsGameLevel.NumHops = (int) numHopsUpDown.Value;
				}
			}
			catch (FormatException ex) {
				MessageBox.Show(this, "Invalid value in one or more level timer text boxes." + Environment.NewLine + Environment.NewLine + ex.GetAllMessages(), "Can not apply changes!");
				return;
			}
			catch (OverflowException ex) {
				MessageBox.Show(this, "Invalid value in one or more level timer text boxes." + Environment.NewLine + Environment.NewLine + ex.GetAllMessages(), "Can not apply changes!");
				return;
			}
			finally {
				UpdateTimeTextBoxes();
			}
			commitChangesAction(gameLevelRadio.Checked, titleTextBox.Text, filenameTextBox.Text, skyboxButton.Text);
			Close();
		}

		private void UpdateTimeTextBoxes() {
			GameLevelDescription curLevelAsGameLevel = currentLevel as GameLevelDescription;
			if (curLevelAsGameLevel != null) {
				goldSecsTextBox.Text = (curLevelAsGameLevel.LevelTimerGoldMs / 1000).ToString("00");
				goldHundsTextBox.Text = ((curLevelAsGameLevel.LevelTimerGoldMs % 1000) / 10).ToString("00");

				silverSecsTextBox.Text = (curLevelAsGameLevel.LevelTimerSilverMs / 1000).ToString("00");
				silverHundsTextBox.Text = ((curLevelAsGameLevel.LevelTimerSilverMs % 1000) / 10).ToString("00");

				bronzeSecsTextBox.Text = (curLevelAsGameLevel.LevelTimerBronzeMs / 1000).ToString("00");
				bronzeHundsTextBox.Text = ((curLevelAsGameLevel.LevelTimerBronzeMs % 1000) / 10).ToString("00");

				maxSecsTextBox.Text = (curLevelAsGameLevel.LevelTimerMaxMs / 1000).ToString("00");
				maxHundsTextBox.Text = ((curLevelAsGameLevel.LevelTimerMaxMs % 1000) / 10).ToString("00");

				numHopsUpDown.Value = curLevelAsGameLevel.NumHops;
			}
		}

		private void SelectSkybox() {
			OpenFileDialog fileSelectorDialog = new OpenFileDialog();
			fileSelectorDialog.AutoUpgradeEnabled = false; // This line required to make it not crash the entire application, gg microsoft
			fileSelectorDialog.ShowHelp = true;
			fileSelectorDialog.Multiselect = false;
			fileSelectorDialog.Filter = "Escape Lizards Levels (*.ell)|*.ell";
			fileSelectorDialog.InitialDirectory = AssetLocator.LevelsDir;
			if (skyboxButton.Text != NO_SKYBOX_TEXT) fileSelectorDialog.FileName = skyboxButton.Text;
			var dialogResult = fileSelectorDialog.ShowDialog(this);
			if (dialogResult != DialogResult.OK) return;

			if (!fileSelectorDialog.CheckFileExists) {
				MessageBox.Show(this, "Invalid file (does not exist).");
				return;
			}

			this.skyboxButton.Text = Path.GetFileName(fileSelectorDialog.FileName);
		}
	}
}
