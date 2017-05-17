// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 20 10 2014 at 16:20 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Rendering {
	internal static partial class NativeMethods {
		#region Resource View Creation
		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "ResourceFactory_CreateRTV")]
		public static extern InteropBool ResourceFactory_CreateRTV(
			IntPtr failReason,
			DeviceHandle deviceHandle,
			Texture2DResourceHandle texture2DHandle,
			uint mipIndex,
			ResourceFormat format,
			InteropBool isMultisampled,
			IntPtr outRTVHandle // RenderTargetViewHandle*
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "ResourceFactory_CreateRTVToTexArr")]
		public static extern InteropBool ResourceFactory_CreateRTVToTexArr(
			IntPtr failReason,
			DeviceHandle deviceHandle,
			Texture2DResourceHandle texture2DHandle,
			uint mipIndex,
			uint arrayIndex,
			ResourceFormat format,
			InteropBool isMultisampled,
			IntPtr outRTVHandle // RenderTargetViewHandle*
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "ResourceFactory_CreateDSV")]
		public static extern InteropBool ResourceFactory_CreateDSV(
			IntPtr failReason,
			DeviceHandle deviceHandle,
			Texture2DResourceHandle texture2DHandle,
			uint mipIndex,
			ResourceFormat format,
			InteropBool isMultisampled,
			IntPtr outDSVHandle // DepthStencilViewHandle*
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "ResourceFactory_CreateDSVToTexArr")]
		public static extern InteropBool ResourceFactory_CreateDSVToTexArr(
			IntPtr failReason,
			DeviceHandle deviceHandle,
			Texture2DResourceHandle texture2DHandle,
			uint mipIndex,
			uint arrayIndex,
			ResourceFormat format,
			InteropBool isMultisampled,
			IntPtr outDSVHandle // DepthStencilViewHandle*
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "ResourceFactory_CreateSRVToBuffer")]
		public static extern InteropBool ResourceFactory_CreateSRVToBuffer(
			IntPtr failReason,
			DeviceHandle deviceHandle,
			BufferResourceHandle bufferHandle,
			ResourceFormat format,
			uint firstElement,
			uint numElements,
			IntPtr outSRVHandle
		);
		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "ResourceFactory_CreateSRVToRawBuffer")]
		public static extern InteropBool ResourceFactory_CreateSRVToRawBuffer(
			IntPtr failReason,
			DeviceHandle deviceHandle,
			BufferResourceHandle bufferHandle,
			uint firstElement,
			uint numElements,
			IntPtr outSRVHandle
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "ResourceFactory_CreateSRVToTexture1D")]
		public static extern InteropBool ResourceFactory_CreateSRVToTexture1D(
			IntPtr failReason,
			DeviceHandle deviceHandle,
			Texture1DResourceHandle texture1DHandle,
			ResourceFormat format,
			uint firstMipIndex,
			uint numMips,
			IntPtr outSRVHandle
		);
		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "ResourceFactory_CreateSRVToTexture1DArray")]
		public static extern InteropBool ResourceFactory_CreateSRVToTexture1DArray(
			IntPtr failReason,
			DeviceHandle deviceHandle,
			Texture1DResourceHandle texture1DHandle,
			ResourceFormat format,
			uint firstMipIndex,
			uint numMips,
			uint firstElementIndex,
			uint numElements,
			IntPtr outSRVHandle
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "ResourceFactory_CreateSRVToTexture2D")]
		public static extern InteropBool ResourceFactory_CreateSRVToTexture2D(
			IntPtr failReason,
			DeviceHandle deviceHandle,
			Texture2DResourceHandle texture2DHandle,
			ResourceFormat format,
			uint firstMipIndex,
			uint numMips,
			IntPtr outSRVHandle
		);
		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "ResourceFactory_CreateSRVToTexture2DArray")]
		public static extern InteropBool ResourceFactory_CreateSRVToTexture2DArray(
			IntPtr failReason,
			DeviceHandle deviceHandle,
			Texture2DResourceHandle texture2DHandle,
			ResourceFormat format,
			uint firstMipIndex,
			uint numMips,
			uint firstElementIndex,
			uint numElements,
			IntPtr outSRVHandle
		);
		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "ResourceFactory_CreateSRVToTexture2DMS")]
		public static extern InteropBool ResourceFactory_CreateSRVToTexture2DMS(
			IntPtr failReason,
			DeviceHandle deviceHandle,
			Texture2DResourceHandle texture2DHandle,
			ResourceFormat format,
			IntPtr outSRVHandle
		);
		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "ResourceFactory_CreateSRVToTexture2DMSArray")]
		public static extern InteropBool ResourceFactory_CreateSRVToTexture2DMSArray(
			IntPtr failReason,
			DeviceHandle deviceHandle,
			Texture2DResourceHandle texture2DHandle,
			ResourceFormat format,
			uint firstElementIndex,
			uint numElements,
			IntPtr outSRVHandle
		);
		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "ResourceFactory_CreateSRVToTexture2DCube")]
		public static extern InteropBool ResourceFactory_CreateSRVToTexture2DCube(
			IntPtr failReason,
			DeviceHandle deviceHandle,
			Texture2DResourceHandle texture2DHandle,
			ResourceFormat format,
			uint firstMipIndex,
			uint numMips,
			IntPtr outSRVHandle
		);
		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "ResourceFactory_CreateSRVToTexture2DCubeArray")]
		public static extern InteropBool ResourceFactory_CreateSRVToTexture2DCubeArray(
			IntPtr failReason,
			DeviceHandle deviceHandle,
			Texture2DResourceHandle texture2DHandle,
			ResourceFormat format,
			uint firstMipIndex,
			uint numMips,
			uint firstFaceIndex,
			uint numCubes,
			IntPtr outSRVHandle
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "ResourceFactory_CreateSRVToTexture3D")]
		public static extern InteropBool ResourceFactory_CreateSRVToTexture3D(
			IntPtr failReason,
			DeviceHandle deviceHandle,
			Texture3DResourceHandle texture3DHandle,
			ResourceFormat format,
			uint firstMipIndex,
			uint numMips,
			IntPtr outSRVHandle
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "ResourceFactory_CreateUAVToBuffer")]
		public static extern InteropBool ResourceFactory_CreateUAVToBuffer(
			IntPtr failReason,
			DeviceHandle deviceHandle,
			BufferResourceHandle bufferHandle,
			ResourceFormat format,
			uint firstElement,
			uint numElements,
			InteropBool appendConsume,
			InteropBool includeCounter,
			IntPtr outUAVHandle
		);
		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "ResourceFactory_CreateUAVToRawBuffer")]
		public static extern InteropBool ResourceFactory_CreateUAVToRawBuffer(
			IntPtr failReason,
			DeviceHandle deviceHandle,
			BufferResourceHandle bufferHandle,
			uint firstElement,
			uint numElements,
			InteropBool appendConsume,
			InteropBool includeCounter,
			IntPtr outUAVHandle
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "ResourceFactory_CreateUAVToTexture1D")]
		public static extern InteropBool ResourceFactory_CreateUAVToTexture1D(
			IntPtr failReason,
			DeviceHandle deviceHandle,
			Texture1DResourceHandle texture1DHandle,
			ResourceFormat format,
			uint mipIndex,
			IntPtr outUAVHandle
		);
		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
				EntryPoint = "ResourceFactory_CreateUAVToTexture1DArray")]
		public static extern InteropBool ResourceFactory_CreateUAVToTexture1DArray(
			IntPtr failReason,
			DeviceHandle deviceHandle,
			Texture1DResourceHandle texture1DHandle,
			ResourceFormat format,
			uint mipIndex,
			uint firstElementIndex,
			uint numElements,
			IntPtr outUAVHandle
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "ResourceFactory_CreateUAVToTexture2D")]
		public static extern InteropBool ResourceFactory_CreateUAVToTexture2D(
			IntPtr failReason,
			DeviceHandle deviceHandle,
			Texture2DResourceHandle texture1DHandle,
			ResourceFormat format,
			uint mipIndex,
			IntPtr outUAVHandle
		);
		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
					EntryPoint = "ResourceFactory_CreateUAVToTexture2DArray")]
		public static extern InteropBool ResourceFactory_CreateUAVToTexture2DArray(
			IntPtr failReason,
			DeviceHandle deviceHandle,
			Texture2DResourceHandle texture1DHandle,
			ResourceFormat format,
			uint mipIndex,
			uint firstElementIndex,
			uint numElements,
			IntPtr outUAVHandle
		);
		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "ResourceFactory_CreateUAVToTexture3D")]
		public static extern InteropBool ResourceFactory_CreateUAVToTexture3D(
			IntPtr failReason,
			DeviceHandle deviceHandle,
			Texture3DResourceHandle texture1DHandle,
			ResourceFormat format,
			uint mipIndex,
			uint firstWCoordOffset,
			uint wCoordRange,
			IntPtr outUAVHandle
		);
		#endregion

		#region Resource Creation
		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "ResourceFactory_CreateVertexBuffer")]
		public static extern InteropBool ResourceFactory_CreateVertexBuffer(
			IntPtr failReason,
			DeviceHandle deviceHandle,
			uint vertexStructSizeBytes,
			uint numVertices,
			int usage,
			int cpuUsage,
			IntPtr initialDataPtr,
			IntPtr outBufferHandle
		);
		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "ResourceFactory_CreateIndexBuffer")]
		public static extern InteropBool ResourceFactory_CreateIndexBuffer(
			IntPtr failReason,
			DeviceHandle deviceHandle,
			uint numIndices,
			int usage,
			int cpuUsage,
			IntPtr initialDataPtr,
			IntPtr outBufferHandle
		);
		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "ResourceFactory_CreateConstantBuffer")]
		public static extern InteropBool ResourceFactory_CreateConstantBuffer(
			IntPtr failReason,
			DeviceHandle deviceHandle,
			uint numBytes,
			int usage,
			int cpuUsage,
			IntPtr initialDataPtr,
			IntPtr outBufferHandle
		);
		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "ResourceFactory_CreateBuffer")]
		public static extern InteropBool ResourceFactory_CreateBuffer(
			IntPtr failReason,
			DeviceHandle deviceHandle,
			uint elementSizeBytes,
			uint numElements,
			int usage,
			int cpuUsage,
			PipelineBindings pipelineBindings,
			InteropBool isStructured,
			InteropBool allowRawAccess,
			IntPtr initialDataPtr,
			IntPtr outBufferHandle
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "ResourceFactory_CreateTexture1DArray")]
		public static extern InteropBool ResourceFactory_CreateTexture1DArray(
			IntPtr failReason,
			DeviceHandle deviceHandle,
			uint widthTx,
			uint numTextures,
			InteropBool allocateMipmaps,
			ResourceFormat format,
			int usage,
			int cpuUsage,
			PipelineBindings pipelineBindings,
			InteropBool allowMipGeneration,
			InteropBool allowLODClamping,
			IntPtr initialDataArrayPtr, // InitialResourceDataDesc[]
			uint initialDataArrayLen,
			IntPtr outTexture1DHandle
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "ResourceFactory_CreateTexture2DArray")]
		public static extern InteropBool ResourceFactory_CreateTexture2DArray(
			IntPtr failReason,
			DeviceHandle deviceHandle,
			uint widthTx,
			uint heightTx,
			uint numTextures,
			InteropBool allocateMipmaps,
			InteropBool multisampled,
			ResourceFormat format,
			int usage,
			int cpuUsage,
			PipelineBindings pipelineBindings,
			InteropBool allowMipGeneration,
			InteropBool allowLODClamping,
			IntPtr initialDataPtr, // InitialResourceDataDesc[]
			uint initialDataArrayLen,
			IntPtr outTexture2DHandle
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "ResourceFactory_LoadTexture2D")]
		public static extern InteropBool ResourceFactory_LoadTexture2D(
			IntPtr failReason,
			DeviceHandle deviceHandle,
			[MarshalAs(InteropUtils.INTEROP_STRING_TYPE)] string filePath,
			InteropBool allocateMipmaps,
			int usage,
			int cpuUsage,
			PipelineBindings pipelineBindings,
			InteropBool allowMipGeneration,
			InteropBool allowLODClamping,
			IntPtr outTexture2DHandle, // Texture2DResourceHandle*
			IntPtr outWidth, // uint*
			IntPtr outHeight, // uint*
			IntPtr outFormat // ResourceFormat*
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "ResourceFactory_CreateTexture2DCubeArray")]
		public static extern InteropBool ResourceFactory_CreateTexture2DCubeArray(
			IntPtr failReason,
			DeviceHandle deviceHandle,
			uint widthTx,
			uint heightTx,
			uint numCubes,
			InteropBool allocateMipmaps,
			InteropBool multisampled,
			ResourceFormat format,
			int usage,
			int cpuUsage,
			PipelineBindings pipelineBindings,
			InteropBool allowMipGeneration,
			InteropBool allowLODClamping,
			IntPtr initialDataPtr, // InitialResourceDataDesc[]
			uint initialDataArrayLen,
			IntPtr outTexture2DHandle
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "ResourceFactory_CreateTexture3D")]
		public static extern InteropBool ResourceFactory_CreateTexture3D(
			IntPtr failReason,
			DeviceHandle deviceHandle,
			uint widthTx,
			uint heightTx,
			uint depthTx,
			InteropBool allocateMipmaps,
			ResourceFormat format,
			int usage,
			int cpuUsage,
			PipelineBindings pipelineBindings,
			InteropBool allowMipGeneration,
			InteropBool allowLODClamping,
			IntPtr initialDataPtr, // InitialResourceDataDesc[]
			uint initialDataArrayLen,
			IntPtr outTexture3DHandle
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "ResourceFactory_CreateSSO")]
		public static extern InteropBool ResourceFactory_CreateSSO(
			IntPtr failReason,
			DeviceHandle deviceHandle,
			TextureFilterType filterType,
			TextureWrapMode wrapMode,
			AnisotropicFilteringLevel maxAnisotropy,
			float borderColorR,
			float borderColorG,
			float borderColorB,
			float borderColorA,
			IntPtr outSamplerStateHandle
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "ResourceFactory_CreateRSState")]
		public static extern InteropBool ResourceFactory_CreateRSState(
			IntPtr failReason,
			DeviceHandle deviceHandle,
			int fillMode,
			int cullMode,
			InteropBool flipFaces,
			int depthBias,
			float depthBiasClamp,
			float slopeScaledDepthBias,
			InteropBool enableZClipping,
			IntPtr outRasterizerStateHandle
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "ResourceFactory_CreateDSState")]
		public static extern InteropBool ResourceFactory_CreateDSState(
			IntPtr failReason,
			DeviceHandle deviceHandle,
			InteropBool enableDepthTesting,
			DepthComparisonFunction depthComparisonFunction,
			IntPtr outDepthStencilStateHandle
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "ResourceFactory_CreateBlendState")]
		public static extern InteropBool ResourceFactory_CreateBlendState(
			IntPtr failReason,
			DeviceHandle deviceHandle,
			InteropBool enableBlending,
			BlendOperation blendOperation,
			IntPtr outBlendStateHandle
		);
		#endregion

		#region ResourceManipulation
		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "ResourceFactory_CopyResource")]
		public static extern InteropBool ResourceFactory_CopyResource(
			IntPtr failReason,
			DeviceContextHandle contextHandle,
			ResourceHandle sourceResourceHandle,
			ResourceHandle destinationResourceHandle
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "ResourceFactory_MapSubresource")]
		public static extern InteropBool ResourceFactory_MapSubresource(
			IntPtr failReason,
			DeviceContextHandle contextHandle,
			ResourceHandle resourceHandle,
			uint subresourceIndex,
			ResourceMapping mapType,
			IntPtr outResDataPtr, // void**
			IntPtr outResDataRowLenBytes, // uint*
			IntPtr outResDataSliceLenBytes // uint*
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "ResourceFactory_UnmapSubresource")]
		public static extern InteropBool ResourceFactory_UnmapSubresource(
			IntPtr failReason,
			DeviceContextHandle contextHandle,
			ResourceHandle resourceHandle,
			uint subresourceIndex
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "ResourceFactory_UpdateSubresourceRegion")]
		public static extern InteropBool ResourceFactory_UpdateSubresourceRegion(
			IntPtr failReason,
			DeviceContextHandle contextHandle,
			ResourceHandle resourceHandle,
			uint subresourceIndex,
			IntPtr subresourceBoxPtr, // SubresourceBox*
			IntPtr data,
			uint dataRowDistanceBytes,
			uint dataSliceDistanceBytes
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "ResourceFactory_CopySubresourceRegion")]
		public static extern InteropBool ResourceFactory_CopySubresourceRegion(
			IntPtr failReason,
			DeviceContextHandle contextHandle,
			ResourceHandle sourceResourceHandle,
			uint sourceSubresourceIndex,
			IntPtr sourceSubresourceBoxPtr, // SubresourceBox*
			ResourceHandle destResourceHandle,
			uint destSubresourceIndex,
			uint destX,
			uint destY,
			uint destZ
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "ResourceFactory_GenerateMips")]
		public static extern InteropBool ResourceFactory_GenerateMips(
			IntPtr failReason,
			DeviceContextHandle contextHandle,
			ShaderResourceViewHandle srvToTexture
		);
		#endregion

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "ResourceFactory_ReleaseResourceOrView")]
		public static extern InteropBool ResourceFactory_ReleaseResource(
			IntPtr failReason,
			ResourceHandle resourceHandle
		);
		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "ResourceFactory_ReleaseResourceOrView")]
		public static extern InteropBool ResourceFactory_ReleaseResource(
			IntPtr failReason,
			ResourceViewHandle resourceViewHandle
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "ResourceFactory_SetTextureQualityOffset")]
		public static extern InteropBool ResourceFactory_SetTextureQualityOffset(
			IntPtr failReason,
			DeviceContextHandle deviceContextHandle,
			Texture1DResourceHandle texture1DHandle,
			uint qualityReductionLevel
		);
		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "ResourceFactory_SetTextureQualityOffset")]
		public static extern InteropBool ResourceFactory_SetTextureQualityOffset(
			IntPtr failReason,
			DeviceContextHandle deviceContextHandle,
			Texture2DResourceHandle texture2DHandle,
			uint qualityReductionLevel
		);
		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "ResourceFactory_SetTextureQualityOffset")]
		public static extern InteropBool ResourceFactory_SetTextureQualityOffset(
			IntPtr failReason,
			DeviceContextHandle deviceContextHandle,
			Texture3DResourceHandle texture3DHandle,
			uint qualityReductionLevel
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "ResourceFactory_GetNumMips1D")]
		public static extern InteropBool ResourceFactory_GetNumMips(
			IntPtr failReason,
			Texture1DResourceHandle texture1DHandle,
			IntPtr outNumMips // uint*
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "ResourceFactory_GetNumMips2D")]
		public static extern InteropBool ResourceFactory_GetNumMips(
			IntPtr failReason,
			Texture2DResourceHandle texture1DHandle,
			IntPtr outNumMips // uint*
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "ResourceFactory_GetNumMips3D")]
		public static extern InteropBool ResourceFactory_GetNumMips(
			IntPtr failReason,
			Texture3DResourceHandle texture1DHandle,
			IntPtr outNumMips // uint*
		);
	}
}