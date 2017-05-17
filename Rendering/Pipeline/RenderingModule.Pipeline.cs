// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 09 01 2015 at 17:33 by Ben Bowen

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

namespace Ophidian.Losgap.Rendering {
	public sealed partial class RenderingModule {
		private readonly static List<RenderPass> activePasses = new List<RenderPass>();
		private readonly static List<Window> openWindowCopySpace = new List<Window>(); 

		/// <summary>
		/// A read-only list of all <see cref="RenderPass"/>es that have been added to the renderer.
		/// </summary>
		public static IReadOnlyList<RenderPass> AddedPasses {
			get {
				lock (staticMutationLock) {
					return new ReadOnlyCollection<RenderPass>(activePasses);
				}
			}
		}

		/// <summary>
		/// Add a new <see cref="RenderPass"/> to the renderer. The pass will be executed on each frame, after all previously-added
		/// passes. You can insert the new pass ahead of others in the list instead using <see cref="InsertRenderPass"/>.
		/// </summary>
		/// <param name="renderPass">The render pass to add. Must not be null or already added.</param>
		public static void AddRenderPass(RenderPass renderPass) {
			if (renderPass == null) throw new ArgumentNullException("renderPass");

			lock (staticMutationLock) {
				if (activePasses.Contains(renderPass)) throw new InvalidOperationException("Render pass already added.");
				using (RenderStateBarrier.AcquirePermit()) activePasses.Add(renderPass);
				Logger.Log("Added " + renderPass + ".");
			}
			
		}

		/// <summary>
		/// Removes a previously-added <see cref="RenderPass"/> from the renderer. The pass will no longer be executed on each frame.
		/// </summary>
		/// <param name="renderPass">The pass to remove. Must not be null, and must currently be added.</param>
		public static void RemoveRenderPass(RenderPass renderPass) {
			if (renderPass == null) throw new ArgumentNullException("renderPass");

			using (RenderStateBarrier.AcquirePermit(withLock: staticMutationLock)) {
				if (!activePasses.Remove(renderPass)) throw new InvalidOperationException("Render pass not currently added.");
				Logger.Log("Removed " + renderPass + ".");
			}
		}

		/// <summary>
		/// Inserts a new <see cref="RenderPass"/> in to the rendering pipeline, to be executed on each frame before <paramref name="insertionKey"/>.
		/// </summary>
		/// <param name="renderPass">The pass to add. Must not be null or already added.</param>
		/// <param name="insertionKey">The already-added pass to insert <paramref name="renderPass"/> in front of. Must not be null, and must
		/// currently be added.</param>
		public static void InsertRenderPass(RenderPass renderPass, RenderPass insertionKey) {
			if (renderPass == null) throw new ArgumentNullException("renderPass");
			if (insertionKey == null) throw new ArgumentNullException("insertionKey");

			lock (staticMutationLock) {
				if (activePasses.Contains(renderPass)) throw new InvalidOperationException("Render pass already added.");
				else if (!activePasses.Contains(insertionKey)) throw new InvalidOperationException("Insertion key pass not currently added.");
				using (RenderStateBarrier.AcquirePermit()) activePasses.Insert(activePasses.IndexOf(insertionKey), renderPass);
				Logger.Log("Added " + renderPass + ".");
			}
		}

		/// <summary>
		/// Remove all <see cref="RenderPass"/>es from the pipeline.
		/// </summary>
		public static void ClearPasses() {
			using (RenderStateBarrier.AcquirePermit(withLock: staticMutationLock)) {
				activePasses.Clear();
			}
			Logger.Log("Removed all render passes.");
		}

		private static void LayerCreated(SceneLayer newLayer) {
			newLayer.SetRenderingEnabled(true);
			newLayer.LayerDisposed += CleanUpLayer;
		}

		private static void CleanUpLayer(SceneLayer disposedLayer) {
			disposedLayer.SetRenderingEnabled(false);
		}

		//private static readonly Dictionary<RenderPass, int> passCountDict = new Dictionary<RenderPass, int>();
		//private static readonly Dictionary<RenderPass, double> passTimeDict = new Dictionary<RenderPass, double>();
		//private static readonly Stopwatch timer = Stopwatch.StartNew();

		unsafe void ILosgapModule.PipelineIterate(ParallelizationProvider parallelizationProvider, long deltaMs) {
#if CODE_ANALYSIS // Required because for some reason we can't ignore this one in source. Justification: parallelizationProvider will never be null.
			if (parallelizationProvider == null) throw new ArgumentNullException("parallelizationProvider");
#endif
			openWindowCopySpace.Clear();
			for (int i = 0; i < Window.OpenWindows.Count; ++i) {
				openWindowCopySpace.Add(Window.OpenWindows[i]);
			}
			foreach (var win in openWindowCopySpace) {
				win.HydrateMessagePump();
				if (!win.IsClosed) win.Clear(); // If check because HydrateMessagePump can close the window
			}
			
			RenderStateBarrier.FreezeMutations();
			bool singleThreadedModeEnabled = parallelizationProvider.ForceSingleThreadedMode;
			try {
				if (!MtRenderingSupported) parallelizationProvider.ForceSingleThreadedMode = true;
				for (int p = 0; p < activePasses.Count; ++p) {
					RenderPass currentPass = activePasses[p];
					if (!currentPass.IsEnabled) continue;
					if (!currentPass.IsValid) {
						Logger.Debug(currentPass + " will be skipped as it not in a valid configuration.");
						continue;
					}

					//var timeBefore = timer.Elapsed;
					currentPass.OnPrePass();
					currentPass.Execute(parallelizationProvider);
					currentPass.OnPostPass();
					//if (!passCountDict.ContainsKey(currentPass)) {
					//	passCountDict.Add(currentPass, 0);
					//	passTimeDict.Add(currentPass, 0d);
					//}
					//passTimeDict[currentPass] += (timer.Elapsed - timeBefore).TotalMilliseconds;
					//if (++passCountDict[currentPass] == 1000) {
					//	Logger.Log("Pass '" + currentPass + "' avg time = " + (passTimeDict[currentPass] / 1000d) + "ms");
					//	passCountDict[currentPass] = 0;
					//	passTimeDict[currentPass] = 0d;
					//}
				}
			}
			finally {
				parallelizationProvider.ForceSingleThreadedMode = singleThreadedModeEnabled;
				RenderStateBarrier.UnfreezeMutations();
			}
		}
	}
}