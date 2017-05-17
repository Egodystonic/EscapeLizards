// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 05 11 2014 at 10:59 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// Represents an array of 2-dimensional textures.
	/// </summary>
	/// <remarks>
	/// You can create 2D texture arrays by using the <see cref="TextureFactory"/> class, and setting parameters on the returned
	/// <see cref="Texture2DBuilder{TTexel}"/> before calling <see cref="Texture2DBuilder{TTexel}.CreateArray"/>.
	/// </remarks>
	/// <typeparam name="TTexel">A <see cref="Extensions.IsBlittable">blittable</see> struct type that represents a single texel.
	/// Must be a valid <see cref="ITexel"/>.</typeparam>
	public sealed class Texture2DArray<TTexel> 
		: BaseTextureArrayResource<TTexel, Texture2D<TTexel>, Texture2DArray<TTexel>, Texture2DBuilder<TTexel>>, ITexture2DArray
		where TTexel : struct, ITexel {
		private readonly uint width, height;
		private readonly bool isMultisampled;

		/// <summary>
		/// The width, in texels, of each texture in this array.
		/// </summary>
		public uint Width {
			get { return width; }
		}

		/// <summary>
		/// The height, in texels, of each texture in this array.
		/// </summary>
		public uint Height {
			get { return height; }
		}

		/// <summary>
		/// Whether or not the textures in this array are multisampled.
		/// </summary>
		public bool IsMultisampled {
			get { return isMultisampled; }
		}

		internal Texture2DArray(ResourceHandle resourceHandle, ResourceUsage usage, ByteSize size, GPUBindings permittedBindings,
			uint length, bool isMipGenTarget, bool allowsDynamicDetail, uint width, uint height,
			uint texelSizeBytes, bool isMipmapped, uint numMips, bool isMultisampled)
			: base(resourceHandle, usage, size, permittedBindings,
				length, isMipGenTarget, allowsDynamicDetail,
				texelSizeBytes, isMipmapped, numMips,
				Enumerable.Range(0, (int)length).Select(i => new Texture2D<TTexel>(
					resourceHandle,
					usage,
					size / length,
					permittedBindings,
					true,
					isMipGenTarget,
					allowsDynamicDetail,
					width,
					height,
					texelSizeBytes,
					isMipmapped,
					numMips,
					(uint)i,
					isMultisampled
				)).ToArray()
			) {
			this.width = width;
			this.height = height;
			this.isMultisampled = isMultisampled;
		}

		/// <summary>
		/// The width, in texels, of the requested mip; for any given texture in this array.
		/// </summary>
		/// <param name="mipIndex">The index of the mip level whose width it is you wish to ascertain.</param>
		/// <returns>The width, in texels, of the given mip index.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public uint MipWidth(uint mipIndex) {
			Assure.LessThan(mipIndex, NumMips, "Mip index out of bounds.");
			return TextureUtils.GetDimensionForMipLevel(width, mipIndex);
		}

		/// <summary>
		/// The height, in texels, of the requested mip; for any given texture in this array.
		/// </summary>
		/// <param name="mipIndex">The index of the mip level whose width it is you wish to ascertain.</param>
		/// <returns>The height, in texels, of the given mip index.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public uint MipHeight(uint mipIndex) {
			Assure.LessThan(mipIndex, NumMips, "Mip index out of bounds.");
			return TextureUtils.GetDimensionForMipLevel(height, mipIndex);
		}

		/// <summary>
		/// Creates a new view to this texture array.
		/// </summary>
		/// <param name="firstMipIndex">The index of the first mip-level in each texture to expose via the returned view.</param>
		/// <param name="numMips">The number of mips in each texture to expose via the returned view.</param>
		/// <param name="firstArrayIndex">The index of the first texture in the array to expose via the returned view.</param>
		/// <param name="numArrayElements">The number of textures in the array to expose via the returned view.</param>
		/// <returns>A new resource view that permits reading data from this texture array.</returns>
		public unsafe override ShaderTextureArrayResourceView CreateView(uint firstMipIndex, uint numMips, uint firstArrayIndex, uint numArrayElements) {
			if (firstMipIndex + numMips > NumMips || numMips == 0U) throw new ArgumentOutOfRangeException("numMips");
			if (firstArrayIndex + numArrayElements > ArrayLength || numArrayElements == 0U) throw new ArgumentOutOfRangeException("numArrayElements");
			if ((PermittedBindings & GPUBindings.ReadableShaderResource) != GPUBindings.ReadableShaderResource) {
				throw new InvalidOperationException("Can not create an shader resource view to a resource that was created without the "
					+ GPUBindings.ReadableShaderResource + " binding.");
			}

			ShaderResourceViewHandle outViewHandle;

			InteropUtils.CallNative(
				NativeMethods.ResourceFactory_CreateSRVToTexture2DArray,
				RenderingModule.Device,
				(Texture2DResourceHandle) ResourceHandle,
				GetFormatForType(TexelFormat),
				firstMipIndex,
				numMips,
				firstArrayIndex,
				numArrayElements,
				(IntPtr) (&outViewHandle)
			).ThrowOnFailure();

			return new ShaderTextureArrayResourceView(outViewHandle, this, firstMipIndex, numMips, firstArrayIndex, numArrayElements);
		}

		/// <summary>
		/// Creates a new unordered access view to this texture array.
		/// </summary>
		/// <param name="mipIndex">The mip level that will be exposed in every exposed texture in the array.
		/// Only one mip-level can be exposed in an <see cref="UnorderedAccessView"/>.</param>
		/// <param name="firstArrayIndex">The index of the first texture in the array to expose via the returned view.</param>
		/// <param name="numArrayElements">The number of textures in the array to expose via the returned view.</param>
		/// <returns>A new resource view that permits reading and writing data from/to this texture array.</returns>
		public unsafe override UnorderedTextureArrayAccessView CreateUnorderedAccessView(uint mipIndex, uint firstArrayIndex, uint numArrayElements) {
			if (firstArrayIndex + numArrayElements > ArrayLength || numArrayElements == 0U) throw new ArgumentOutOfRangeException("numArrayElements");
			if (mipIndex >= NumMips) throw new ArgumentOutOfRangeException("mipIndex");
			if ((PermittedBindings & GPUBindings.WritableShaderResource) != GPUBindings.WritableShaderResource) {
				throw new InvalidOperationException("Can not create an unordered access view to a resource that was created without the "
					+ GPUBindings.WritableShaderResource + " binding.");
			}

			UnorderedAccessViewHandle outViewHandle;

			InteropUtils.CallNative(
				NativeMethods.ResourceFactory_CreateUAVToTexture2DArray,
				RenderingModule.Device,
				(Texture2DResourceHandle) ResourceHandle,
				GetFormatForType(TexelFormat),
				mipIndex,
				firstArrayIndex,
				numArrayElements,
				(IntPtr) (&outViewHandle)
			).ThrowOnFailure();

			return new UnorderedTextureArrayAccessView(outViewHandle, this, mipIndex, firstArrayIndex, numArrayElements);
		}

		/// <summary>
		/// Used by the <see cref="BaseResource.ToString"/> implementation in <see cref="BaseResource"/> to create a child-class-specific
		/// string that provides additional data about this resource.
		/// </summary>
		/// <returns>A string in the format DATUM1 + <see cref="BaseResource.DESC_VALUE_SEPARATOR"/> + DATUM2 + 
		/// <see cref="BaseResource.DESC_VALUE_SEPARATOR"/> + DATUM3 etc</returns>
		protected override unsafe string CreateResourceDescString() {
			return
				"Tex2DArray<" + TexelFormat.Name + ">" + DESC_TYPE_SEPARATOR
				+ ArrayLength + " len" + DESC_VALUE_SEPARATOR
				+ Width + "x" + Height + "tx" + DESC_VALUE_SEPARATOR
				+ (IsMultisampled ? "Multisampled" + DESC_VALUE_SEPARATOR : String.Empty)
				+ (IsMipmapped ? "Mipmap" + DESC_VALUE_SEPARATOR : String.Empty)
				+ (IsGlobalDetailTarget ? "DynDetail" + DESC_VALUE_SEPARATOR : String.Empty)
				+ (IsMipGenTarget ? "MipGenTarget" + DESC_VALUE_SEPARATOR : String.Empty)
				;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		ITexture2D ITexture2DArray.ElementAt(uint index) {
			return (ITexture2D) base.ElementAt(index);
		}
	}
}