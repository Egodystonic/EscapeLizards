// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 09 01 2015 at 17:49 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Rendering {
	public sealed partial class RenderingModule {
		private static ThreadLocal<DeviceContextHandle> threadLocalDeviceContext = null;
		private static DeviceHandle device;
		private static bool deviceSupportsMTRendering;
		internal static DeviceHandle Device {
			get {
				lock (staticMutationLock) {
					if (device == DeviceHandle.NULL) CreateDeviceIfNecessary();
					return device;
				}
			}
			private set {
				using (RenderStateBarrier.AcquirePermit(withLock: staticMutationLock)) {
					device = value;
				}
			}
		}
		internal static DeviceContextHandle DeviceContext {
			get {
				lock (staticMutationLock) {
					if (threadLocalDeviceContext == null) CreateDeviceIfNecessary();
					// ReSharper disable once PossibleNullReferenceException
					return threadLocalDeviceContext.Value;
				}
			}
		}
		internal static bool MtRenderingSupported {
			get {
				lock (staticMutationLock) {
					return deviceSupportsMTRendering;
				}
			}
		}

		private unsafe static void OnSystemStarting() {
			// Lock here because this method is called from a triggered public event
			using (RenderStateBarrier.AcquirePermit(withLock: staticMutationLock)) {
				CreateDeviceIfNecessary();
			}
		}

		private unsafe static void CreateDeviceIfNecessary() {
			using (RenderStateBarrier.AcquirePermit()) { 
				// Create device
				if (device == DeviceHandle.NULL) {
					Logger.Debug("Creating device...");
					DeviceHandle outDeviceHandle;
					InteropBool outSupportsMT;

					InteropUtils.CallNative(
						NativeMethods.DeviceFactory_CreateDevice,
						SelectedGPU.Index,
						SelectedOutputGPU.Index,
						SelectedOutputDisplay.Index,
						(IntPtr)(&outDeviceHandle),
						(IntPtr)(&outSupportsMT)
						).ThrowOnFailure();

					Device = outDeviceHandle;
					deviceSupportsMTRendering = outSupportsMT;
					if (!deviceSupportsMTRendering) {
						Logger.Warn("Selected adapter/device does not support multithreaded command lists. " +
							"Multithreaded rendering will be disabled, resulting in a potentially significant performance penalty. " +
							"You are strongly recommended to upgrade your graphics drivers to the latest version.");
					}
				}

				// Create thread-local device context wrapper
				if (threadLocalDeviceContext == null) {
					Logger.Debug("Creating thread-local device context wrapper...");
					threadLocalDeviceContext = new ThreadLocal<DeviceContextHandle>(CreateThreadLocalDeviceContext, true);
				}
			}
		}

		private unsafe static void OnSystemExited() {
			// Lock here because this method is called from a triggered public event
			using (RenderStateBarrier.AcquirePermit(withLock: staticMutationLock)) {
				// Close windows
				Logger.Debug("Closing windows...");
				HashSet<Window> openWindowsCopy = new HashSet<Window>();
				Window.OpenWindows.CopyTo(openWindowsCopy);
				foreach (Window window in openWindowsCopy) {
					window.Close();
				}

				// Destroy device contexts
				Logger.Debug("Destroying device contexts...");
				threadLocalDeviceContext.Values.ForEach(dc => InteropUtils.CallNative(NativeMethods.ContextFactory_ReleaseContext, dc));
				threadLocalDeviceContext = null;

				// Destroy device
				Logger.Debug("Destroying device...");
				InteropUtils.CallNative(NativeMethods.DeviceFactory_ReleaseDevice, Device).ThrowOnFailure();
				Device = DeviceHandle.NULL;
			}
		}

		private static unsafe DeviceContextHandle CreateThreadLocalDeviceContext() {
			Logger.Debug("Obtaining thread-local device context...");
			if (Thread.CurrentThread == LosgapSystem.MasterThread) {
				Logger.Debug("Getting immediate context...");
				return GetImmediateContext();
			}
			else {
				DeviceContextHandle outDeviceContextHandle;

				Logger.Debug("Creating deferred context...");
				InteropUtils.CallNative(
					NativeMethods.ContextFactory_CreateDeferredContext,
					device,
					(IntPtr) (&outDeviceContextHandle)
				).ThrowOnFailure();

				return outDeviceContextHandle;
			}
		}

		private static unsafe DeviceContextHandle GetImmediateContext() {
			DeviceContextHandle outImmediateContext;
			InteropUtils.CallNative(
				NativeMethods.ContextFactory_GetImmediateContext,
				device,
				(IntPtr) (&outImmediateContext)
			).ThrowOnFailure();
			return outImmediateContext;
		}
	}
}