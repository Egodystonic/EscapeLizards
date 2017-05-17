using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Egodystonic.EscapeLizards.Editor {
	public partial class TextInputForm : Form {
		private readonly Func<string, bool> okButtonPressedFunc;

		public TextInputForm(string inputLabel, Func<string, bool> okButtonPressedFunc) {
			InitializeComponent();
			this.label.Text = inputLabel;
			this.okButtonPressedFunc = okButtonPressedFunc;

			this.okButton.MouseClick += (sender, args) => {
				if (this.okButtonPressedFunc(textBox.Text)) Close();
				else MessageBox.Show(this, "Invalid entry.");
			};
			this.cancelButton.MouseClick += (sender, args) => Close();
		}
	}
}
