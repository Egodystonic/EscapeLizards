// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 09 01 2015 at 17:47 by Ben Bowen

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Rendering {
	public sealed partial class RenderingModule {
		private static GraphicsProcessingUnit[] installedGPUs = null;
		private static int? selectedGPUIndex = null;
		private static int? selectedOutputGPUIndex = null;
		private static int? selectedOutputIndex = null;

		/// <summary>
		/// An enumeration of all <see cref="GraphicsProcessingUnit"/>s installed on this system.
		/// </summary>
		public static IEnumerable<GraphicsProcessingUnit> InstalledGPUs {
			get {
				lock (staticMutationLock) {
					if (installedGPUs == null) throw new InvalidOperationException("Can not access hardware before module has been added.");
					return new ReadOnlyCollection<GraphicsProcessingUnit>(installedGPUs);
				}
			}
			internal set {
				using (RenderStateBarrier.AcquirePermit(withLock: staticMutationLock)) {
					installedGPUs = value.ToArray();
				}
			}
		}

		/// <summary>
		/// The <see cref="GraphicsProcessingUnit"/> that will be used for rendering on this system. Can be set with
		/// <see cref="SetHardware(Ophidian.Losgap.Rendering.GraphicsProcessingUnit,Ophidian.Losgap.Rendering.OutputDisplay)"/>.
		/// </summary>
		public static GraphicsProcessingUnit SelectedGPU {
			get {
				using (RenderStateBarrier.AcquirePermit(withLock: staticMutationLock)) {
					if (selectedGPUIndex == null) SetSelectedHardwareToRecommendedValues();
					return installedGPUs[selectedGPUIndex.Value];
				}
			}
		}

		public static GraphicsProcessingUnit SelectedOutputGPU {
			get {
				using (RenderStateBarrier.AcquirePermit(withLock: staticMutationLock)) {
					if (selectedOutputGPUIndex == null) SetSelectedHardwareToRecommendedValues();
					return installedGPUs[selectedOutputGPUIndex.Value];
				}
			}
		}

		/// <summary>
		/// The <see cref="OutputDisplay"/> that will be used for displaying rendered content on this system. Can be set with
		/// <see cref="SetHardware(Ophidian.Losgap.Rendering.GraphicsProcessingUnit,Ophidian.Losgap.Rendering.OutputDisplay)"/>.
		/// </summary>
		public static OutputDisplay SelectedOutputDisplay {
			get {
				using (RenderStateBarrier.AcquirePermit(withLock: staticMutationLock)) {
					if (selectedGPUIndex == null || selectedOutputIndex == null) SetSelectedHardwareToRecommendedValues();
					return installedGPUs[selectedOutputGPUIndex.Value].Outputs.ElementAt(selectedOutputIndex.Value);
				}
			}
		}

		/// <summary>
		/// Attempts to set the hardware that will be used for rendering by the system. You do not need to call this method before 
		/// starting the system: Reasonable defaults will be selected for you.
		/// </summary>
		/// <param name="gpuIndex">The <see cref="GraphicsProcessingUnit.Index">index</see> slot of the gpu to use.</param>
		/// <param name="displayIndex">The <see cref="OutputDisplay.Index">index</see> slot of the output to use.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if </exception>
		/// <exception cref="InvalidOperationException">Thrown if you attempt to set the hardware while the
		/// <see cref="LosgapSystem">system</see> is running (you must stop the system to change the hardware).</exception>
		/// <seealso cref="SetHardware(Ophidian.Losgap.Rendering.GraphicsProcessingUnit,Ophidian.Losgap.Rendering.OutputDisplay)"/>
		public static void SetHardware(int gpuIndex, int displayGPUIndex, int? displayIndex) {
			using (RenderStateBarrier.AcquirePermit(withLock: staticMutationLock)) {
				if (installedGPUs == null) throw new InvalidOperationException("Can not set hardware before module has been added.");
				if (LosgapSystem.IsRunning) throw new InvalidOperationException("Can not set hardware while system is running.");
				if (gpuIndex >= installedGPUs.Length || gpuIndex < 0) throw new ArgumentOutOfRangeException("gpuIndex");
				if (displayIndex == null) displayIndex = 0;

				selectedGPUIndex = gpuIndex;
				selectedOutputGPUIndex = displayGPUIndex;
				selectedOutputIndex = displayIndex;

				Logger.Log("Selected GPU set to " + SelectedGPU);
				Logger.Log("Selected output GPU set to " + SelectedOutputGPU);
				Logger.Log("Selected output set to " + SelectedOutputDisplay);
			}
		}
		/// <summary>
		/// Calls <see cref="SetHardware(Ophidian.Losgap.Rendering.GraphicsProcessingUnit,Ophidian.Losgap.Rendering.OutputDisplay)"/> with
		/// the <see cref="GraphicsProcessingUnit.Index"/> of the supplied <paramref name="gpu"/> and the
		/// <see cref="OutputDisplay.Index"/> of the supplied <paramref name="outputDisplay"/>.
		/// </summary>
		/// <param name="gpu">The gpu whose index slot you wish you set.</param>
		/// <param name="outputDisplay">The output whose index slot you wish to set.</param>
		/// <seealso cref="SetHardware(int,int)"/>
		public static void SetHardware(GraphicsProcessingUnit gpu, OutputDisplay outputDisplay) {
			SetHardware(gpu.Index, installedGPUs.First(g => g.Outputs.Contains(outputDisplay)).Index, outputDisplay.Index);
		}

		private static unsafe void InitDeviceFactory() {
			GraphicsProcessingUnit* outGPUArrPtr;
			int outGPUArrLen;
			InteropUtils.CallNative(NativeMethods.DeviceFactory_Init, (IntPtr) (&outGPUArrPtr), (IntPtr) (&outGPUArrLen)).ThrowOnFailure();

			InstalledGPUs = new GraphicsProcessingUnit[outGPUArrLen];
			for (int i = 0; i < outGPUArrLen; ++i) {
				installedGPUs[i] = outGPUArrPtr[i];
				Logger.Log("Installed GPU: " + outGPUArrPtr[i]);
			}
		}

		public static void GetRecommendedHardwareIndices(out int gpuIndex, out int outputGPUIndex, out int outputIndex) {
			if (installedGPUs == null) throw new InvalidOperationException("Can not view hardware before module has been added.");

			GraphicsProcessingUnit recommendedGPU = installedGPUs
				.OrderByDescending(gpu => gpu.DedicatedVideoMemory).First();

			OutputDisplay recommendedOutputDisplay;
			if (!recommendedGPU.Outputs.Any()) {
				var allOutputs = installedGPUs.SelectMany(gpu => gpu.Outputs);
				if (allOutputs.Any(output => output.IsPrimaryOutput)) {
					recommendedOutputDisplay = allOutputs.First(output => output.IsPrimaryOutput);
				}
				else if (!allOutputs.Any()) {
					throw new ApplicationException("No output detected.");
				}
				else {
					recommendedOutputDisplay = allOutputs
						.OrderByDescending(output => output.HighestResolution.ResolutionWidth * output.HighestResolution.ResolutionHeight).First();
				}
			}
			else if (recommendedGPU.Outputs.Any(output => output.IsPrimaryOutput)) {
				recommendedOutputDisplay = recommendedGPU.Outputs.First(output => output.IsPrimaryOutput);
				outputGPUIndex = recommendedGPU.Index;
			}
			else {
				recommendedOutputDisplay = recommendedGPU.Outputs
					.OrderByDescending(output => output.NativeResolutions
						.Select(res => res.ResolutionWidth * res.ResolutionHeight)
						.OrderByDescending(res => res).First()
					)
					.First();
				outputGPUIndex = recommendedGPU.Index;
			}

			gpuIndex = recommendedGPU.Index;
			outputGPUIndex = installedGPUs.First(gpu => gpu.Outputs.Contains(recommendedOutputDisplay)).Index;
			outputIndex = recommendedOutputDisplay.Index;
		}

		private static void SetSelectedHardwareToRecommendedValues() {
			int outGPUIndex, outOutputGPUIndex, outOutputIndex;

			GetRecommendedHardwareIndices(out outGPUIndex, out outOutputGPUIndex, out outOutputIndex);

			SetHardware(outGPUIndex, outOutputGPUIndex, outOutputIndex);
		}
	}
}