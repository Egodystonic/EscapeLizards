// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 07 11 2014 at 16:30 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// A static class that offers a set of utility methods for working with <see cref="ITexture">textures</see>.
	/// </summary>
	public static class TextureUtils {
		/// <summary>
		/// Gets the number of mips that would be generated for the given texture dimensions.
		/// </summary>
		/// <param name="widthTx">The width of the original texture.</param>
		/// <param name="heightTx">The height of the original texture. For a 1D texture, leave as <c>1U</c>.</param>
		/// <param name="depthTx">The depth of the original texture. For a 1D or 2D texture, leave as <c>1U</c>.</param>
		/// <returns>The number of mips that would be generated for a texture of the given size, including the top-level mip.
		/// If any given dimension is not a power of two, this method will always return <c>1U</c>.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint GetNumMips(uint widthTx, uint heightTx = 1U, uint depthTx = 1U) {
			if (MathUtils.IsPowerOfTwo(widthTx) && MathUtils.IsPowerOfTwo(heightTx) && MathUtils.IsPowerOfTwo(depthTx)) {
				return (uint) MathUtils.MostSignificantBit(Math.Max(Math.Max(widthTx, heightTx), depthTx));
			}
			else return 1U;	
		}

		/// <summary>
		/// Returns the unique subresource identifier for a texture or texture array with the given parameters. Subresource identifiers
		/// are mostly used to identify individual mips in a texture that may or may not be part of a texture array.
		/// </summary>
		/// <param name="numMips">The number of mips in the texture resource.</param>
		/// <param name="mipIndex">The mip index to identify.</param>
		/// <param name="arrayIndex">The array index of the texture, if it is part of an array. If it is not, leave as <c>0U</c>.</param>
		/// <returns>The subresource index of the requested mip level, for the texture.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint GetSubresourceIndex(uint numMips, uint mipIndex, uint arrayIndex = 0U) {
			if (numMips == 0U) numMips = 1U;
			Assure.LessThan(mipIndex, numMips, "Mip index should be less than numMips.");
			return mipIndex + (arrayIndex * numMips);
		}

		/// <summary>
		/// Returns the width, height, or depth of a texture at a given mip level, given the size of that dimension for the largest mip.
		/// </summary>
		/// <param name="levelZeroDimension">The width, height, or depth at mip 0.</param>
		/// <param name="mipLevel">The mip level.</param>
		/// <returns>The width, height, or depth of the texture at the requested <paramref name="mipLevel"/>.</returns>
		/// <exception cref="AssuranceFailedException">Thrown if <paramref name="levelZeroDimension"/> is not a power-of-two.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint GetDimensionForMipLevel(uint levelZeroDimension, uint mipLevel) {
			Assure.True(mipLevel == 0U || MathUtils.IsPowerOfTwo(levelZeroDimension), "Non-power-of-two dimensions can not be correctly mipped.");
			return Math.Max(levelZeroDimension >> (int) Math.Min(mipLevel, 0x1FU), 1U);
		}

		/// <summary>
		/// Returns the size of a texture created with the given parameters.
		/// </summary>
		/// <param name="texel">The texel format of the texture. See also: <see cref="TexelFormat"/>.</param>
		/// <param name="withMips">If true, the allocation of mips will be factored in to the size calculation.</param>
		/// <param name="widthTx">The width of the texture, in texels.</param>
		/// <param name="heightTx">The height of the texture, in texels. For a 1D texture, leave as <c>1U</c>.</param>
		/// <param name="depthTx">The depth of the texture, in texels. For a 1D or 2D texture, leave as <c>1U</c>.</param>
		/// <returns>The amount of memory required to create &amp; store a texture with the given parameters.</returns>
		/// <seealso cref="GetSize(uint,bool,uint,uint,uint)"/>
		/// <exception cref="AssuranceFailedException">(Debug only) Thrown if any dimension is not a power-of-two, and 
		/// <paramref name="withMips"/> is <c>true</c>.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ByteSize GetSize(ITexel texel, bool withMips, uint widthTx, uint heightTx = 1U, uint depthTx = 1U) {
			Assure.NotNull(texel);
			return GetSize((uint) texel.GetTexelSize(), withMips, widthTx, heightTx, depthTx);
		}

		/// <summary>
		/// Returns the size of a texture created with the given parameters.
		/// </summary>
		/// <param name="texelSizeBytes">The size, in bytes, of the texel format of the texture. See also: <see cref="TexelFormat"/>.</param>
		/// <param name="withMips">If true, the allocation of mips will be factored in to the size calculation.</param>
		/// <param name="widthTx">The width of the texture, in texels.</param>
		/// <param name="heightTx">The height of the texture, in texels. For a 1D texture, leave as <c>1U</c>.</param>
		/// <param name="depthTx">The depth of the texture, in texels. For a 1D or 2D texture, leave as <c>1U</c>.</param>
		/// <returns>The amount of memory required to create &amp; store a texture with the given parameters.</returns>
		/// <seealso cref="GetSize(ITexel,bool,uint,uint,uint)"/>
		/// <exception cref="AssuranceFailedException">(Debug only) Thrown if any dimension is not a power-of-two, and 
		/// <paramref name="withMips"/> is <c>true</c>.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ByteSize GetSize(uint texelSizeBytes, bool withMips, uint widthTx, uint heightTx = 1U, uint depthTx = 1U) {
			return GetSizeTexels(withMips, widthTx, heightTx, depthTx) * texelSizeBytes;
		}

		/// <summary>
		/// Returns the number of texels that a texture created with the given parameters would have.
		/// </summary>
		/// <param name="withMips">If true, the allocation of mips will be factored in to the size calculation.</param>
		/// <param name="widthTx">The width of the texture, in texels.</param>
		/// <param name="heightTx">The height of the texture, in texels. For a 1D texture, leave as <c>1U</c>.</param>
		/// <param name="depthTx">The depth of the texture, in texels. For a 1D or 2D texture, leave as <c>1U</c>.</param>
		/// <returns>The number of texels that the resultant texture would contain.</returns>
		/// <exception cref="AssuranceFailedException">(Debug only) Thrown if any dimension is not a power-of-two, and 
		/// <paramref name="withMips"/> is <c>true</c>.</exception>
		public static uint GetSizeTexels(bool withMips, uint widthTx, uint heightTx = 1U, uint depthTx = 1U) {
			Assure.True(MathUtils.IsPowerOfTwo(widthTx) || !withMips,
				"GetSize is inaccurate with non-power-of-two texture sizes and mips."
				);
			Assure.True(MathUtils.IsPowerOfTwo(heightTx) || !withMips,
				"GetSize is inaccurate with non-power-of-two texture sizes and mips."
				);
			Assure.True(MathUtils.IsPowerOfTwo(depthTx) || !withMips,
				"GetSize is inaccurate with non-power-of-two texture sizes and mips."
				);

			uint result = widthTx * heightTx * depthTx;

			if (withMips) {
				uint numMips = GetNumMips(widthTx, heightTx, depthTx);
				for (int i = 1; i < numMips; ++i) {
					result += Math.Max(1U, widthTx >> i) * Math.Max(1U, heightTx >> i) * Math.Max(1U, depthTx >> i);
				}
			}

			return result;
		}
	}
}