using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Ophidian.Losgap;

namespace Egodystonic.EscapeLizards.Editor {
	static class EntryPoint {
		[STAThread]
		public static void Main(string[] args) {
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new EditorToolbar());
		}
	}
}
