// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 16 10 2014 at 17:03 by Ben Bowen

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// Represents the LOSGAP framework Rendering module. Call <see cref="LosgapSystem.AddModule"/> with <c>typeof(RenderingModule)</c> 
	/// to enable rendering functionality.
	/// </summary>
	public sealed partial class RenderingModule : ILosgapModule {
		/// <summary>
		/// The default frame rate, in hz (updates/frames per second).
		/// </summary>
		public const long DEFAULT_FRAME_RATE_HZ = 60L;
		// A lock object used to prevent updates to the render pipeline while a frame is being rendered.
		public static readonly StateMutationBarrier RenderStateBarrier = new StateMutationBarrier();
		private static readonly object staticMutationLock = new object();
		private static long desiredTickIntervalMs = MathUtils.MILLIS_IN_ONE_SECOND / DEFAULT_FRAME_RATE_HZ;

		long ILosgapModule.TickIntervalMs { 
			get { return Interlocked.Read(ref desiredTickIntervalMs); }
		}

		/// <summary>
		/// The desired framerate (frames-per-second). Change this to set a new maximum framerate cap, or set to <c>null</c> to allow the renderer to
		/// run unlimited.
		/// </summary>
		public static long? MaxFrameRateHz {
			get {
				long desiredTickIntervalMsLocal = Interlocked.Read(ref desiredTickIntervalMs);
				return desiredTickIntervalMsLocal == 0L ? (long?) null : MathUtils.MILLIS_IN_ONE_SECOND / desiredTickIntervalMsLocal;
			}
			set {
				if (value != null && value.Value <= 0L) {
					Logger.Warn("Invalid desired framerate '" + value.Value + "', setting to default value of " + DEFAULT_FRAME_RATE_HZ + "fps.");
					value = DEFAULT_FRAME_RATE_HZ;
				}
				Interlocked.Exchange(ref desiredTickIntervalMs, value == null ? 0L : MathUtils.MILLIS_IN_ONE_SECOND / value.Value);
				Logger.Log("Set framerate cap to " + value + "Hz.");
			}
		}

		/// <summary>
		/// The amount of (multisample) anti-aliasing to apply to rendered scenes. Antialiasing reduces 'staggered' edges on diagonal
		/// lines, resulting in a smoother, more realistic look, at the cost of framerate.
		/// </summary>
		public static MSAALevel AntialiasingLevel {
			get {
				lock (staticMutationLock) {
					uint outMSAALevel;
					unsafe {
						InteropUtils.CallNative(
							NativeMethods.DeviceFactory_GetMSAALevel,
							(IntPtr) (&outMSAALevel)
						).ThrowOnFailure();
					}
					if (outMSAALevel == 1U) return MSAALevel.None; // Special case
					return (MSAALevel) outMSAALevel;
				}
			}
			set {
				using (RenderStateBarrier.AcquirePermit(withLock: staticMutationLock)) {
					InteropUtils.CallNative(
						NativeMethods.DeviceFactory_SetMSAALevel,
						(uint) value
					).ThrowOnFailure();

					// Now, we refresh each window so that they'll recreate their backbuffers, resulting in them picking up the
					// new MSAA level
					foreach (Window window in Window.OpenWindows) {
						window.SetResolution(window.Width, window.Height);
					}
					Logger.Log("Set anti-aliasing level to " + value + ".");
				}
			}
		}

		/// <summary>
		/// Whether or not to enable vertical syncing (vsync). VSync prohibits the display from being updated on a fullscreen
		/// <see cref="Window"/> while the <see cref="SelectedOutputDisplay"/> is refreshing. This results in less 'tearing', where
		/// a frame is half-written over the previous. However, vsync can cause the illusion of input lag when enabled (though this can
		/// be ameliorated somewhat by enabling <see cref="MultibufferLevel.Triple">triple buffering</see> on your window).
		/// </summary>
		public static bool VSyncEnabled {
			get {
				lock (staticMutationLock) {
					InteropBool outVSyncEnabled;
					unsafe {
						InteropUtils.CallNative(
							NativeMethods.DeviceFactory_GetVSyncEnabled,
							(IntPtr) (&outVSyncEnabled)
						).ThrowOnFailure();
					}
					return outVSyncEnabled;
				}
			}
			set {
				using (RenderStateBarrier.AcquirePermit(withLock: staticMutationLock)) {
					InteropUtils.CallNative(
						NativeMethods.DeviceFactory_SetVSyncEnabled,
						(InteropBool) value
					).ThrowOnFailure();

					// Now, we refresh each window so that they'll recreate their backbuffers, resulting in them picking up the
					// new vsync settings
					foreach (Window window in Window.OpenWindows) {
						window.SetResolution(window.Width, window.Height);
					}
					Logger.Log("Set vsync " + (value ? "enabled" : "disabled") + ".");
				}
			}
		}

		private RenderingModule() { }

		void ILosgapModule.ModuleAdded() {
			LosgapSystem.SystemStarting += OnSystemStarting;
			LosgapSystem.SystemExited += OnSystemExited;
			Scene.LayerCreated += LayerCreated;

			InitDeviceFactory();
		}
	}
}