// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 16 10 2014 at 17:11 by Ben Bowen

using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Rendering {
	[SuppressUnmanagedCodeSecurity]
	internal static partial class NativeMethods {
		private const string NATIVE_DLL_NAME = "RenderingNative.dll";

		#region DeviceFactory
		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
		EntryPoint = "DeviceFactory_Init")]
		public static extern InteropBool DeviceFactory_Init(
			IntPtr failReason,
			IntPtr outGPUArrPtr,
			IntPtr outNumGPUs
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
		EntryPoint = "DeviceFactory_CreateDevice")]
		public static extern InteropBool DeviceFactory_CreateDevice(
			IntPtr failReason,
			int selectedGPUIndex,
			int selectedOutputGPUIndex,
			int selectedOutputIndex,
			IntPtr outDeviceHandle, // DeviceHandle*
			IntPtr outSupportsMTRendering // InteropBool*
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
		EntryPoint = "DeviceFactory_GetMSAALevel")]
		public static extern InteropBool DeviceFactory_GetMSAALevel(
			IntPtr failReason,
			IntPtr outMSAALevel // uint*
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
		EntryPoint = "DeviceFactory_SetMSAALevel")]
		public static extern InteropBool DeviceFactory_SetMSAALevel(
			IntPtr failReason,
			uint msaaLevel
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
		EntryPoint = "DeviceFactory_GetVSyncEnabled")]
		public static extern InteropBool DeviceFactory_GetVSyncEnabled(
			IntPtr failReason,
			IntPtr outVsyncEnabled // InteropBool*
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
		EntryPoint = "DeviceFactory_SetVSyncEnabled")]
		public static extern InteropBool DeviceFactory_SetVSyncEnabled(
			IntPtr failReason,
			InteropBool vsyncEnabled
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
		EntryPoint = "DeviceFactory_ReleaseDevice")]
		public static extern InteropBool DeviceFactory_ReleaseDevice(
			IntPtr failReason,
			DeviceHandle deviceHandle
		);
		#endregion

		#region ContextFactory
		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "ContextFactory_GetImmediateContext")]
		public static extern InteropBool ContextFactory_GetImmediateContext(
			IntPtr failReason,
			DeviceHandle deviceHandle,
			IntPtr outContextPtr
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "ContextFactory_CreateDeferredContext")]
		public static extern InteropBool ContextFactory_CreateDeferredContext(
			IntPtr failReason,
			DeviceHandle deviceHandle,
			IntPtr outContextPtr
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "ContextFactory_ExecuteDeferredCommandLists")]
		public static extern InteropBool ContextFactory_ExecuteDeferredCommandLists(
			IntPtr failReason,
			DeviceContextHandle immediateContextHandle,
			IntPtr deferredContextHandleArrayPtr,
			uint deferredContextArrayLen
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
		EntryPoint = "ContextFactory_ReleaseContext")]
		public static extern InteropBool ContextFactory_ReleaseContext(
			IntPtr failReason,
			DeviceContextHandle deviceContextHandle
		);
		#endregion

		#region WindowFactory
		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "WindowFactory_CreateWindow", CharSet = InteropUtils.DEFAULT_CHAR_SET, BestFitMapping = true)]
		public static extern InteropBool WindowFactory_CreateWindow(
			IntPtr failReason,
			[MarshalAs(InteropUtils.INTEROP_STRING_TYPE)] string iconFilePath,
			DeviceHandle deviceHandle,
			UInt32 bufferCount,
			IntPtr outWindowHandle,
			IntPtr outWindowClosureFlagPtr // InteropBool**
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "WindowFactory_GetClientWidth")]
		public static extern InteropBool WindowFactory_GetClientWidth(
			IntPtr failReason,
			WindowHandle windowHandle,
			IntPtr outWidthPx // uint*
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "WindowFactory_GetClientHeight")]
		public static extern InteropBool WindowFactory_GetClientHeight(
			IntPtr failReason,
			WindowHandle windowHandle,
			IntPtr outHeightPx // uint*
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "WindowFactory_GetClientPosition")]
		public static extern InteropBool WindowFactory_GetClientPosition(
			IntPtr failReason,
			WindowHandle windowHandle,
			IntPtr outX, // int*
			IntPtr outY // int*
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "WindowFactory_SetResolution")]
		public static extern InteropBool WindowFactory_SetResolution(
			IntPtr failReason,
			WindowHandle windowHandle,
			UInt32 widthPx,
			UInt32 heightPx
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "WindowFactory_GetFullscreenState")]
		public static extern InteropBool WindowFactory_GetFullscreenState(
			IntPtr failReason,
			WindowHandle windowHandle,
			IntPtr outFullscreenState // WindowFullscreenState*
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "WindowFactory_SetFullscreenState")]
		public static extern InteropBool WindowFactory_SetFullscreenState(
			IntPtr failReason,
			WindowHandle windowHandle,
			WindowFullscreenState fullscreenState
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "WindowFactory_GetCursorVisibility")]
		public static extern InteropBool WindowFactory_GetCursorVisibility(
			IntPtr failReason,
			WindowHandle windowHandle,
			IntPtr outVisibilityState // InteropBool*
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "WindowFactory_SetCursorVisibility")]
		public static extern InteropBool WindowFactory_SetCursorVisibility(
			IntPtr failReason,
			WindowHandle windowHandle,
			InteropBool visibilityState
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "WindowFactory_SetWindowTitle")]
		public static extern InteropBool WindowFactory_SetWindowTitle(
			IntPtr failReason,
			WindowHandle windowHandle,
			[MarshalAs(InteropUtils.INTEROP_STRING_TYPE)] string windowTitle
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "WindowFactory_GetFocusedWindowHandle")]
		public static extern InteropBool WindowFactory_GetFocusedWindowHandle(
			IntPtr failReason,
			IntPtr outWindowHandle // WindowHandle*
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "WindowFactory_CreateViewport")]
		public static extern InteropBool WindowFactory_CreateViewport(
			IntPtr failReason,
			IntPtr outViewportHandle // ViewportHandle*
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "WindowFactory_AlterViewport")]
		public static extern InteropBool WindowFactory_AlterViewport(
			IntPtr failReason,
			ViewportHandle viewportHandle,
			uint topLeftX,
			uint topLeftY,
			uint widthPx,
			uint heightPx
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "WindowFactory_GetWindowBackBufferRTVAndDSV")]
		public static extern InteropBool WindowFactory_GetWindowBackBufferRTVAndDSV(
			IntPtr failReason,
			WindowHandle windowHandle,
			IntPtr outRTVPtr, // RenderTargetViewHandle*
			IntPtr outDSVPtr // DepthStencilViewHandle*
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "WindowFactory_GetWindowSwapChain")]
		public static extern InteropBool WindowFactory_GetWindowSwapChain(
			IntPtr failReason,
			WindowHandle windowHandle,
			IntPtr outSwapChainPtr // SwapChainHandle*
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "WindowFactory_DestroyViewport")]
		public static extern InteropBool WindowFactory_DestroyViewport(
			IntPtr failReason,
			ViewportHandle viewportHandle
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "WindowFactory_HydrateMessagePump")]
		public static extern InteropBool WindowFactory_HydrateMessagePump(
			IntPtr failReason,
			WindowHandle windowHandle,
			IntPtr outWindowResized // InteropBool*
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "WindowFactory_ClearWindow")]
		public static extern InteropBool WindowFactory_ClearWindow(
			IntPtr failReason,
			DeviceContextHandle deviceContext,
			WindowHandle windowHandle
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "WindowFactory_CloseWindow")]
		public static extern InteropBool WindowFactory_CloseWindow(
			IntPtr failReason,
			WindowHandle windowHandle
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "WindowFactory_CleanUpWindowResources")]
		public static extern InteropBool WindowFactory_CleanUpWindowResources(
			IntPtr failReason,
			WindowHandle windowHandle
		);
		#endregion

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "RenderPassManager_FlushInstructions")]
		public static extern InteropBool RenderPassManager_FlushInstructions(
			IntPtr failReason,
			DeviceContextHandle deviceContext,
			IntPtr commandListStart,
			uint numCommands
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "RenderPassManager_ExecuteCommandList")]
		public static extern InteropBool RenderPassManager_ExecuteCommandList(
			IntPtr failReason,
			DeviceContextHandle immediateContextHandle,
			IntPtr commandList
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "RenderPassManager_PresentBackBuffer")]
		public static extern InteropBool RenderPassManager_PresentBackBuffer(
			IntPtr failReason,
			SwapChainHandle swapChainHandle
		);
	}
}