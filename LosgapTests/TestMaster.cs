// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 22 10 2014 at 14:22 by Ben Bowen

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ophidian.Losgap.Rendering;

namespace Ophidian.Losgap {
	[TestClass]
	public static class TestMaster {
		private static Thread masterThread;

		[AssemblyInitialize]
		public static void InitializeTests(TestContext ctx) {
			string[] paths = {
				@"..\",
			};
			var path = new[] { Environment.GetEnvironmentVariable("PATH") ?? String.Empty };
			string newPath = String.Join(Path.PathSeparator.ToString(), path.Concat(paths));
			Environment.SetEnvironmentVariable("PATH", newPath);

			LosgapSystem.ApplicationName = "Losgap Unit Tests";
			LosgapSystem.InstallationDirectory = new DirectoryInfo(@"..\..\..\..\TestDir\Inst\");
			LosgapSystem.MutableDataDirectory = new DirectoryInfo(@"..\..\..\..\TestDir\Data\");
			Logger.LoggingProvider = new TestLoggingProvider();

			//Logger.LoggingProvider = new TestLoggingProvider();

			masterThread = new Thread(MasterLoop) {
				IsBackground = true
			};
			masterThread.Start();
			SpinWait.SpinUntil(() => LosgapSystem.IsRunning);
		}

		[AssemblyCleanup]
		public static void ShutdownTests() {
			bool exited = false;
			LosgapSystem.SystemExited += () => exited = true;
			LosgapSystem.Exit();
			SpinWait.SpinUntil(() => exited);
			if (((TestLoggingProvider)Logger.LoggingProvider).SuppressionActive) {
				LosgapSystem.ExitWithError("Logging provider was still suppressed at test finish.");
			}
		}

		private static void MasterLoop() {
			try {
				LosgapSystem.AddModule(typeof(RenderingModule));

				LosgapSystem.Start();
			}
			catch (ThreadAbortException) {
				Thread.ResetAbort();
			}
			catch (Exception e) {
				Logger.Fatal("Test master loop has failed.", e);
			}
		}
	}
}