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
	/// Represents an array of 1-dimensional textures.
	/// </summary>
	/// <remarks>
	/// You can create 1D texture arrays by using the <see cref="TextureFactory"/> class, and setting parameters on the returned
	/// <see cref="Texture1DBuilder{TTexel}"/> before calling <see cref="Texture1DBuilder{TTexel}.CreateArray"/>.
	/// </remarks>
	/// <typeparam name="TTexel">A <see cref="Extensions.IsBlittable">blittable</see> struct type that represents a single texel.
	/// Must be a valid <see cref="ITexel"/>.</typeparam>
	public sealed class Texture1DArray<TTexel> 
		: BaseTextureArrayResource<TTexel, Texture1D<TTexel>, Texture1DArray<TTexel>, Texture1DBuilder<TTexel>>, ITexture1DArray 
		where TTexel : struct, ITexel {
		private readonly uint width;

		/// <summary>
		/// The width, in texels, of each texture in this array.
		/// </summary>
		public uint Width {
			get { return width; }
		}

		internal Texture1DArray(ResourceHandle resourceHandle, ResourceUsage usage, ByteSize size, GPUBindings permittedBindings,
			uint length, bool isMipGenTarget, bool allowsDynamicDetail, uint width,
			uint texelSizeBytes, bool isMipmapped, uint numMips)
			: base(resourceHandle, usage, size, permittedBindings,
				length, isMipGenTarget, allowsDynamicDetail,
				texelSizeBytes, isMipmapped, numMips,
				Enumerable.Range(0, (int)length).Select(i => new Texture1D<TTexel>(
					resourceHandle,
					usage,
					size / length,
					permittedBindings,
					true,
					isMipGenTarget,
					allowsDynamicDetail,
					width,
					texelSizeBytes,
					isMipmapped,
					numMips,
					(uint)i
					)).ToArray()
				) {
					this.width = width;
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
				NativeMethods.ResourceFactory_CreateSRVToTexture1DArray,
				RenderingModule.Device,
				(Texture1DResourceHandle) ResourceHandle,
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
				NativeMethods.ResourceFactory_CreateUAVToTexture1DArray,
				RenderingModule.Device,
				(Texture1DResourceHandle) ResourceHandle,
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
				"Tex1DArray<" + TexelFormat.Name + ">" + DESC_TYPE_SEPARATOR
				+ ArrayLength + " len" + DESC_VALUE_SEPARATOR
				+ Width + "tx" + DESC_VALUE_SEPARATOR
				+ (IsMipmapped ? "Mipmap" + DESC_VALUE_SEPARATOR : String.Empty)
				+ (IsGlobalDetailTarget ? "DynDetail" + DESC_VALUE_SEPARATOR : String.Empty)
				+ (IsMipGenTarget ? "MipGenTarget" + DESC_VALUE_SEPARATOR : String.Empty)
				;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		ITexture1D ITexture1DArray.ElementAt(uint index) {
			return (ITexture1D) base.ElementAt(index);
		}
	}
}