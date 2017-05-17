// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 06 01 2015 at 14:36 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Rendering {
	internal static partial class NativeMethods {
		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "ShaderManager_LoadShader", CharSet = InteropUtils.DEFAULT_CHAR_SET, BestFitMapping = true)]
		public static extern InteropBool ShaderManager_LoadShader(
			IntPtr failReason,
			DeviceHandle deviceHandle,
			[MarshalAs(InteropUtils.INTEROP_STRING_TYPE)] string shaderFilePath,
			ShaderType type,
			IntPtr outShaderPtr // ShaderHandle*
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "ShaderManager_UnloadShader", CharSet = InteropUtils.DEFAULT_CHAR_SET, BestFitMapping = true)]
		public static extern InteropBool ShaderManager_UnloadShader(
			IntPtr failReason,
			ShaderHandle shaderHandle,
			ShaderType type
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "ShaderManager_CreateInputLayout", CharSet = InteropUtils.DEFAULT_CHAR_SET, BestFitMapping = true)]
		public static extern InteropBool ShaderManager_CreateInputLayout(
			IntPtr failReason,
			DeviceHandle deviceHandle,
			ShaderHandle vertexShaderHandle,
			InputElementDesc[] inputElementDescArr,
			uint inputElementDescArrLen,
			IntPtr outInputLayoutPtr // InputLayoutHandle*
		);
	}
}