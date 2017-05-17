// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 20 10 2014 at 10:47 by Ben Bowen

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// A window (frame) into which scenes can be rendered.
	/// </summary>
	public sealed unsafe class Window : IDisposable {
		private const string DEFAULT_WINDOW_ICON = "losgap.ico";
		private const uint DEFAULT_WINDOW_WIDTH_PX = 800;
		private const uint DEFAULT_WINDOW_HEIGHT_PX = 600;
		private static readonly object staticMutationLock = new object();
		private static readonly List<Window> openWindows = new List<Window>(); 
		
		/// <summary>
		/// A (readonly) collection of Windows that are currently open.
		/// </summary>
		/// <remarks>
		/// Please note that as windows are closed, you must re-access this property to get an updated list.
		/// </remarks>
		public static List<Window> OpenWindows {
			get {
				lock (staticMutationLock) {
					return openWindows;
				}
			}
		}
		
		internal readonly object WindowMutationLock = new object();
		internal readonly WindowHandle WindowHandle;
		private readonly InteropBool* windowClosureFlagPtr; // Warning: Becomes invalid after disposal
		private readonly List<SceneViewport> addedViewports = new List<SceneViewport>();
		private bool isDisposed;
		private string lastTitle = String.Empty;
		private event Action<Window> windowClosed;
		private event Action<Window, uint, uint> windowResized;

		/// <summary>
		/// An event that is raised when this window is closed (either by the application via <see cref="Close"/> or by the user).
		/// </summary>
		public event Action<Window> WindowClosed {
			add {
				lock (WindowMutationLock) {
					windowClosed += value;
				}
			}
			remove {
				lock (WindowMutationLock) {
					windowClosed -= value;
				}
			}
		}

		/// <summary>
		/// An event that is raised when this window is resized (either by the application via <see cref="SetResolution(uint,uint)"/> etc.
		/// or by the user dragging the frame borders or clicking the maximize/minimize buttons).
		/// </summary>
		public event Action<Window, uint, uint> WindowResized {
			add {
				lock (WindowMutationLock) {
					windowResized += value;
				}
			}
			remove {
				lock (WindowMutationLock) {
					windowResized -= value;
				}
			}
		}

		public static Window FocusedWindow {
			get {
				WindowHandle outHwnd;

				unsafe {
					char* failReason = stackalloc char[InteropUtils.MAX_INTEROP_FAIL_REASON_STRING_LENGTH + 1];
					bool success = NativeMethods.WindowFactory_GetFocusedWindowHandle((IntPtr) failReason, (IntPtr) (&outHwnd));
					if (!success) throw new NativeOperationFailedException(Marshal.PtrToStringUni((IntPtr) failReason));
				}

				if (outHwnd == WindowHandle.NULL) return null;
				lock (staticMutationLock) {
					for (int i = 0; i < openWindows.Count; ++i) {
						if (openWindows[i].WindowHandle == outHwnd) return openWindows[i];
					}
					return null;
				}
			}
		}
		
		/// <summary>
		/// Gets or sets the width, in pixels, of the client area and render target of the window. When the window is fullscreen, 
		/// this property gets and sets the x-resolution of the window.
		/// </summary>
		/// <seealso cref="SetResolution(uint,uint)"/>
		public uint Width {
			get {
				lock (WindowMutationLock) {
					if (IsClosed) return 0U;
					uint outClientWidth;
					unsafe {
						char* failReason = stackalloc char[InteropUtils.MAX_INTEROP_FAIL_REASON_STRING_LENGTH + 1];
						bool success = NativeMethods.WindowFactory_GetClientWidth((IntPtr) failReason, WindowHandle, (IntPtr) (&outClientWidth));
						if (!success) throw new NativeOperationFailedException(Marshal.PtrToStringUni((IntPtr) failReason));
					}
					return outClientWidth;
				}
			}
			set {
				SetResolution(value, Height);
			}
		}
		
		/// <summary>
		/// Gets or sets the height, in pixels, of the client area and render target of the window. When the window is fullscreen, 
		/// this property gets and sets the y-resolution of the window.
		/// </summary>
		/// <seealso cref="SetResolution(uint,uint)"/>
		public uint Height {
			get {
				lock (WindowMutationLock) {
					if (IsClosed) return 0U;
					uint outClientHeight;
					unsafe {
						char* failReason = stackalloc char[InteropUtils.MAX_INTEROP_FAIL_REASON_STRING_LENGTH + 1];
						bool success = NativeMethods.WindowFactory_GetClientHeight((IntPtr) failReason, WindowHandle, (IntPtr) (&outClientHeight));
						if (!success) throw new NativeOperationFailedException(Marshal.PtrToStringUni((IntPtr) failReason));
					}
					return outClientHeight;
				}
			}
			set {
				SetResolution(Width, value);
			}
		}

		public Vector2 Position {
			get {
				lock (WindowMutationLock) {
					if (IsClosed) return Vector2.ZERO;
					int outPosX, outPosY;
					unsafe {
						InteropUtils.CallNative(
							NativeMethods.WindowFactory_GetClientPosition,
							WindowHandle,
							(IntPtr) (&outPosX),
							(IntPtr) (&outPosY)
						).ThrowOnFailure();
					}
					return new Vector2(outPosX, outPosY);
				}
			}
		}
		
		/// <summary>
		/// Gets or sets the fullscreen state of the window. In fullscreen mode, the window will take over the entire output display
		/// of the <see cref="RenderingModule.SelectedOutputDisplay"/>. The resolution for the window in fullscreen mode can be set with 
		/// <see cref="SetResolution(uint,uint)"/>.
		/// <remarks>
		/// If the window is not currently in the foreground, or does not have input focus, attempting to set this value to <c>true</c>
		/// will silently fail (you can check if it worked by getting the value of this property immediately after).
		/// <para>
		/// This is a feature of Windows, and can not be overridden.
		/// </para>
		/// </remarks>
		/// </summary>
		public WindowFullscreenState FullscreenState {
			get {
				return LosgapSystem.InvokeOnMaster(() => {
					lock (WindowMutationLock) {
						if (IsClosed) return WindowFullscreenState.NotFullscreen;
						WindowFullscreenState outFullscreenState;
						unsafe {
							InteropUtils.CallNative(
								NativeMethods.WindowFactory_GetFullscreenState,
								WindowHandle,
								(IntPtr) (&outFullscreenState)
							).ThrowOnFailure();
						}
						return outFullscreenState;
					}
				});
			}
			set {
				LosgapSystem.InvokeOnMaster(() => {
					lock (WindowMutationLock) {
						if (IsClosed) return;
						InteropUtils.CallNative(
							NativeMethods.WindowFactory_SetFullscreenState,
							WindowHandle,
							value
						).ThrowOnFailure();
					}
				});
			}
		}

		/// <summary>
		/// Whether or not the default system cursor should be displayed when over the window. Set to <c>false</c> if you intend to
		/// display your own cursor.
		/// </summary>
		public bool ShowCursor {
			get {
				lock (WindowMutationLock) {
					if (IsClosed) return true;
					InteropBool outVisibilityState;
					unsafe {
						InteropUtils.CallNative(
						NativeMethods.WindowFactory_GetCursorVisibility,
						WindowHandle,
						(IntPtr) (&outVisibilityState)
						).ThrowOnFailure();
					}
					return (bool) outVisibilityState;
				}
			}
			set {
				lock (WindowMutationLock) {
					if (IsClosed) return;
					InteropUtils.CallNative(
					NativeMethods.WindowFactory_SetCursorVisibility,
					WindowHandle,
					(InteropBool) value
					).ThrowOnFailure();
				}
			}
		}
		/// <summary>
		/// Whether or not the window has been closed. You can close the window manually with <see cref="Close"/>.
		/// </summary>
		public bool IsClosed {
			get {
				lock (WindowMutationLock) {
					return isDisposed || *windowClosureFlagPtr;
				}
			}
		}

		public bool HasFocus {
			get {
				return FocusedWindow == this;
			}
		}

		/// <summary>
		/// The text displayed in the titlebar above the window (and on the taskbar and task manager, etc).
		/// </summary>
		public string Title {
			get {
				lock (WindowMutationLock) {
					return lastTitle;
				}
			}
			set {
				if (value == null) throw new ArgumentNullException("value");

				LosgapSystem.InvokeOnMaster(() => {
					lock (WindowMutationLock) {
						if (IsClosed) return;
						InteropUtils.CallNative(
							NativeMethods.WindowFactory_SetWindowTitle,
							WindowHandle,
							value
							).ThrowOnFailure();

						lastTitle = value;
					}
				});
			}
		}

		/// <summary>
		/// A (readonly) collection of <see cref="SceneViewport"/>s that have been attached/added to this Window.
		/// </summary>
		/// <remarks>
		/// Please note that as viewports are added and removed from this window, you must re-access this property to get an
		/// updated list.
		/// </remarks>
		public List<SceneViewport> AddedViewports {
			get {
				lock (WindowMutationLock) {
					return addedViewports;
				}
			}
		}

		/// <summary>
		/// Creates a new Window and shows it on the user's desktop.
		/// </summary>
		/// <param name="windowTitle">The title of the window (shown in the titlebar). Must not be null.</param>
		/// <param name="width">The width (or resolution X-scale) of the window.</param>
		/// <param name="height">The height (or resolution Y-scale) of the window.</param>
		/// <param name="fullscreenState">The initial fullscreen state of the window.</param>
		/// <param name="bufferingLevel">The number of back buffers to connect to the window.
		/// <see cref="MultibufferLevel.Double">Double</see> or <see cref="MultibufferLevel.Triple">triple</see> buffering can help with
		/// <see cref="RenderingModule.VSyncEnabled">vsync</see> issues at the cost of slightly greater memory usage.</param>
		/// <param name="windowIconFilePath">A file path of an icon to load for the window. May be null to use a default icon.</param>
		/// <exception cref="ArgumentException">Thrown if <paramref name="windowIconFilePath"/> does not exist or is an invalid
		/// path.</exception>
		public unsafe Window(
			string windowTitle,
			uint width = DEFAULT_WINDOW_WIDTH_PX,
			uint height = DEFAULT_WINDOW_HEIGHT_PX,
			WindowFullscreenState fullscreenState = WindowFullscreenState.NotFullscreen,
			MultibufferLevel bufferingLevel = MultibufferLevel.Single,
			string windowIconFilePath = null) {
			if (windowIconFilePath == null) {
				windowIconFilePath = Path.Combine(LosgapSystem.InstallationDirectory.FullName, DEFAULT_WINDOW_ICON);
				if (!File.Exists(windowIconFilePath)) {
					using (FileStream iconStream = File.Open(windowIconFilePath, FileMode.CreateNew)) {
						Properties.Resources.DefaultWindowIcon.Save(iconStream);
					}
				}
			}

			if (!IOUtils.IsValidFilePath(windowIconFilePath)) {
				throw new ArgumentException("'" + windowIconFilePath + "' is not a valid file path.", windowIconFilePath);
			}
			if (!File.Exists(windowIconFilePath)) throw new ArgumentException("Icon file does not exist.", windowIconFilePath);
			if (windowTitle == null) throw new ArgumentNullException("windowTitle");


			var windowCreationResult = LosgapSystem.InvokeOnMaster(() => {
				WindowHandle outWindowHandle;
				InteropBool* outWindowClosureFlagPtr;

				InteropUtils.CallNative(
					NativeMethods.WindowFactory_CreateWindow,
					windowIconFilePath,
					RenderingModule.Device,
					((uint) bufferingLevel) + 1U,
					(IntPtr) (&outWindowHandle),
					(IntPtr) (&outWindowClosureFlagPtr)
				).ThrowOnFailure();

				return new {
					WindowHandle = outWindowHandle, 
					WindowClosureFlagPtr = (IntPtr) outWindowClosureFlagPtr
				};
			});

			WindowHandle = windowCreationResult.WindowHandle;
			windowClosureFlagPtr = (InteropBool*) windowCreationResult.WindowClosureFlagPtr;

			lock (staticMutationLock) {
				openWindows.Add(this);
			}

			Title = windowTitle;
			FullscreenState = fullscreenState;
			SetResolution(width, height);

			AddViewport(ViewportAnchoring.TopLeft, Vector2.ZERO, Vector2.ONE);
		}

#if DEBUG
		~Window() {
			if (!IsClosed) Logger.Warn(this + " was not closed before being finalized!");
		}
#endif

		/// <summary>
		/// In fullscreen mode, this function sets the resolution of the display. In windowed mode,
		/// this function sets the size of the client area of the window.
		/// </summary>
		/// <param name="resolution">The resolution to use.</param>
		/// <seealso cref="SetResolution(uint,uint)"/>
		public void SetResolution(NativeOutputResolution resolution) {
			SetResolution(resolution.ResolutionWidth, resolution.ResolutionHeight);
		}
		
		/// <summary>
		/// In fullscreen mode, this function sets the resolution of the display. In windowed mode,
		/// this function sets the size of the client area of the window.
		/// </summary>
		/// <param name="widthPx">The width, in pixels, of the new resolution.</param>
		/// <param name="heightPx">The height, in pixels, of the new resolution.</param>
		/// <seealso cref="SetResolution(Ophidian.Losgap.Rendering.NativeOutputResolution)"/>
		public void SetResolution(uint widthPx, uint heightPx) {
			LosgapSystem.InvokeOnMaster(() => {
				lock (WindowMutationLock) {
					if (IsClosed) return;
					InteropUtils.CallNative(
					NativeMethods.WindowFactory_SetResolution,
					WindowHandle,
					widthPx,
					heightPx
					).ThrowOnFailure();
				}
			});
		}

		/// <summary>
		/// Adds a new <see cref="SceneViewport"/> to this window.
		/// </summary>
		/// <remarks>
		/// All newly-created Windows have a default viewport that occupies their entirety added when they are created.
		/// Therefore, there is no need to add a viewport unless you wish to override this default configuration.
		/// </remarks>
		/// <param name="anchoring">The side or corner of this viewport that retains its relative position when this window is resized.
		/// See also: <see cref="SceneViewport.Anchoring"/>.</param>
		/// <param name="anchorOffset">The offset from the anchored corner or side of the window that this viewport resides at.
		/// Both the <see cref="Vector2.X"/> and <see cref="Vector2.Y"/> values must be in the range <c>0f</c> to <c>1f</c>, where
		/// <c>0f</c> indicates an offset of 0%, and <c>1f</c> indicates an offset of <c>100%</c>.
		/// See also: <see cref="SceneViewport.AnchorOffset"/>.</param>
		/// <param name="size">The size of this viewport, specified as a ratio of the window's resolution.
		/// Both the <see cref="Vector2.X"/> and <see cref="Vector2.Y"/> values must be in the range <c>0f</c> to <c>1f</c>, where
		/// <c>0f</c> indicates a size of 0%, and <c>1f</c> indicates a size of <c>100%</c>.
		/// The combination of size and <paramref name="anchorOffset"/> in either dimension must not exceed 100% (<c>1f</c>).
		/// See also: <see cref="SceneViewport.Size"/>.</param>
		/// <param name="nearPlaneDist">The minimum distance between any object and the <see cref="Camera"/> 
		/// rendering to this viewport permissible for an object to be drawn.</param>
		/// <param name="farPlaneDist">The maximum distance between any object and the <see cref="Camera"/> 
		/// rendering to this viewport permissible for an object to be drawn.</param>
		/// <returns>The newly created viewport that has been added to this window.</returns>
		public SceneViewport AddViewport(ViewportAnchoring anchoring, Vector2 anchorOffset, Vector2 size,
			float nearPlaneDist = 0.1f, float farPlaneDist = 1000f) {
			lock (WindowMutationLock) {
				SceneViewport result = new SceneViewport(this, anchoring, anchorOffset, size, nearPlaneDist, farPlaneDist);
				addedViewports.Add(result);
				return result;
			}
		}

		/// <summary>
		/// Removes all added <see cref="SceneViewport"/>s from this window (including the default viewport). Does not dispose them.
		/// </summary>
		/// <returns>An enumeration of all the viewports that were just removed.</returns>
		public IEnumerable<SceneViewport> ClearViewports() {
			lock (WindowMutationLock) {
				SceneViewport[] addedViewportsCopy = addedViewports.ToArray();
				addedViewports.Clear();
				return addedViewportsCopy;
			}
		}

		/// <summary>
		/// Clears the back buffer and depth/stencil buffer attached to this window to their default values.
		/// </summary>
		public void Clear() {
			unsafe {
				char* failReason = stackalloc char[InteropUtils.MAX_INTEROP_FAIL_REASON_STRING_LENGTH + 1];
				bool success = NativeMethods.WindowFactory_ClearWindow((IntPtr) failReason, RenderingModule.DeviceContext, WindowHandle);
				if (!success) throw new NativeOperationFailedException(Marshal.PtrToStringUni((IntPtr) failReason));
			}
		}

		/// <summary>
		/// Closes this window, removing it from the user's desktop.
		/// </summary>
		public void Close() {
			(this as IDisposable).Dispose();
		}

		/// <summary>
		/// Returns a string that represents the current Window.
		/// </summary>
		/// <returns>
		/// A string containing the title of this window.
		/// </returns>
		public override string ToString() {
			return "Window '" + Title + "'";
		}

		internal bool GetWindowRTVAndDSV(out RenderTargetViewHandle rtv, out DepthStencilViewHandle dsv) {
			RenderTargetViewHandle outWindowRTV;
			DepthStencilViewHandle outWindowDSV;

			unsafe {
				char* failReason = stackalloc char[InteropUtils.MAX_INTEROP_FAIL_REASON_STRING_LENGTH + 1];
				bool success = NativeMethods.WindowFactory_GetWindowBackBufferRTVAndDSV(
					(IntPtr) failReason,
					WindowHandle,
					(IntPtr) (&outWindowRTV),
					(IntPtr) (&outWindowDSV)
				);
				if (!success) throw new NativeOperationFailedException(Marshal.PtrToStringUni((IntPtr) failReason));
			}

			// If window was closed, rtv/dsv handles will be null
			if (outWindowRTV == RenderTargetViewHandle.NULL || outWindowDSV == DepthStencilViewHandle.NULL) {
				rtv = RenderTargetViewHandle.NULL;
				dsv = DepthStencilViewHandle.NULL;
				return false;
			}

			rtv = outWindowRTV;
			dsv = outWindowDSV;
			return true;
		}

		internal bool GetWindowSwapChain(out SwapChainHandle swapChain) {
			SwapChainHandle outSCPtr;

			unsafe {
				char* failReason = stackalloc char[InteropUtils.MAX_INTEROP_FAIL_REASON_STRING_LENGTH + 1];
				bool success = NativeMethods.WindowFactory_GetWindowSwapChain((IntPtr) failReason, WindowHandle, (IntPtr) (&outSCPtr));
				if (!success) throw new NativeOperationFailedException(Marshal.PtrToStringUni((IntPtr) failReason));
			}

			// If window was closed, sc handle will be null
			if (outSCPtr == SwapChainHandle.NULL) {
				swapChain = SwapChainHandle.NULL;
				return false;
			}

			swapChain = outSCPtr;
			return true;
		}

		internal void HydrateMessagePump() {
			Assure.True(Thread.CurrentThread == LosgapSystem.MasterThread, "Called HydrateMessagePump from wrong thread.");
			lock (WindowMutationLock) {
				CheckForAsyncClosure();
				if (IsClosed) return;
				InteropBool outWindowResized;
				unsafe {
					char* failReason = stackalloc char[InteropUtils.MAX_INTEROP_FAIL_REASON_STRING_LENGTH + 1];
					bool success = NativeMethods.WindowFactory_HydrateMessagePump((IntPtr) failReason, WindowHandle, (IntPtr) (&outWindowResized));
					if (!success) throw new NativeOperationFailedException(Marshal.PtrToStringUni((IntPtr) failReason));
				}
				if (outWindowResized) OnWindowResized();
			}
		}

		void IDisposable.Dispose() {
			lock (WindowMutationLock) {
				if (isDisposed) return;
				isDisposed = true;

				lock (staticMutationLock) {
					openWindows.Remove(this);
				}

				LosgapSystem.InvokeOnMasterAsync(() => {
					if (!*windowClosureFlagPtr) {
						InteropUtils.CallNative(NativeMethods.WindowFactory_CloseWindow, WindowHandle).ThrowOnFailure();
					}
					InteropUtils.CallNative(NativeMethods.WindowFactory_CleanUpWindowResources, WindowHandle).ThrowOnFailure();
				});

				ClearViewports().ForEach(vp => vp.Dispose());

				OnWindowClosed();
			}
		}

		private void CheckForAsyncClosure() {
			if (!isDisposed && *windowClosureFlagPtr) {
				(this as IDisposable).Dispose();
			}
		}

		private void OnWindowClosed() {
			if (windowClosed != null) windowClosed(this);
		}

		private void OnWindowResized() {
			if (windowResized != null) windowResized(this, Width, Height);
		}

		/// <summary>
		/// Allows using this Window as a <see cref="RenderPass"/> output by 
		/// returning the first <see cref="SceneViewport"/> attached to it.
		/// </summary>
		/// <param name="operand">The window to output to.</param>
		/// <returns>The first <see cref="SceneViewport"/> in <see cref="AddedViewports"/>.</returns>
		/// <exception cref="InvalidOperationException">Thrown if no <see cref="SceneViewport"/>s 
		/// are currently added to <paramref name="operand"/>.</exception>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations",
			Justification = "An exception is the best way to handle this case, I think.")]
		public static implicit operator SceneViewport(Window operand) {
			if (operand == null) throw new ArgumentNullException("operand");
			try {
				return operand.AddedViewports[0];
			}
			catch (InvalidOperationException e) {
				throw new InvalidOperationException("Can not use " + operand + " as viewport: Window has no viewports!", e);
			}
		}
	}
}