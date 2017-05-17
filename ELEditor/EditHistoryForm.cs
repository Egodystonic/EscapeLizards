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

namespace Egodystonic.EscapeLizards.Editor {
	public partial class EditHistoryForm : Form {
		public const int MAX_STEPS_TO_RETAIN = 60;
		private const int CP_NOCLOSE_BUTTON = 0x200;
		
		protected override CreateParams CreateParams {
			get {
				CreateParams myCp = base.CreateParams;
				myCp.ClassStyle = myCp.ClassStyle | CP_NOCLOSE_BUTTON;
				return myCp;
			}
		}
		
		public EditHistoryForm() {
			InitializeComponent();
			ControlBox = false;
			undoButton.MouseClick += (sender, args) => UndoUpToSelected();
		}

		public void ClearAllHistory() {
			historyList.Items.Clear();
		}

		public void AddStep(string stepDescription, Action reversionAction) {
			BeginInvoke((Action) (() => {
				EditReversionStep newStep = new EditReversionStep(stepDescription, reversionAction);
				historyList.Items.Add(newStep);
				if (historyList.Items.Count > MAX_STEPS_TO_RETAIN) historyList.Items.RemoveAt(0);
				historyList.SelectedItem = newStep;
			}));
		}

		public void UndoLast() {
			BeginInvoke((Action) (() => {
				historyList.SelectedIndex = historyList.Items.Count - 1;
				UndoUpToSelected();
			}));
		}

		private void UndoUpToSelected() {
			List<EditReversionStep> stepsAsList = historyList.Items.Cast<EditReversionStep>().ToList();
			EditReversionStep selectedStep = historyList.SelectedItem as EditReversionStep;
			if (selectedStep == null) {
				MessageBox.Show(this, "Please select a step to revert to first.", "Undo");
				return;
			}
			EditReversionStep curStep;

			do {
				(curStep = stepsAsList.PopLast()).ReversionAction();
			} while (curStep != selectedStep);

			historyList.Items.Clear();
			foreach (EditReversionStep ers in stepsAsList) {
				historyList.Items.Add(ers);
			}
			historyList.SelectedIndex = historyList.Items.Count - 1;
		}
	}
}
