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
	public partial class ListSelect : Form {
		private readonly object instanceMutationLock = new object();
		private object chosenObject;
		private bool wasCancelled;

		public object ChosenObject {
			get {
				lock (instanceMutationLock) {
					return chosenObject;
				}
			}
		}

		public bool WasCancelled {
			get {
				lock (instanceMutationLock) {
					return wasCancelled;
				}
			}
		}

		private ListSelect(IEnumerable<object> items, object selected) {
			InitializeComponent();
			items.ForEach(i => listBox.Items.Add(i));
			if (selected != null) listBox.SelectedItem = selected;
			chosenObject = selected;

			okButton.MouseClick += (sender, args) => {
				lock (instanceMutationLock) {
					chosenObject = listBox.SelectedItem;
					Close();
				}
			};
			cancelButton.MouseClick += (sender, args) => {
				lock (instanceMutationLock) {
					wasCancelled = true;
					Close();
				}
			};
		}

		public static T GetSelection<T>(IEnumerable<T> options, T initialSelection = default(T), bool throwOnCancellation = false) {
			ListSelect ls = new ListSelect(options.Select(t => (object) t), initialSelection);
			ls.ShowDialog();
			if (ls.WasCancelled && throwOnCancellation) throw new OperationCanceledException("User cancelled the list select.");
			return (T) ls.ChosenObject;
		}
	}
}
