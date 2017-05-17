// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 03 11 2014 at 13:33 by Ben Bowen

using System;
using System.Collections.Generic;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// Represents a texture <see cref="IResource">resource</see>. Texture resources are essentially n-dimensional arrays of
	/// <see cref="ITexel">texels</see> that support GPU-assisted operations such as filtering, sampling, etc.
	/// </summary>
	public interface ITexture : IResource {
		/// <summary>
		/// If true, this texture contains memory allocated for a texture 'mipmap', which is used in 
		/// <see cref="TextureFilterType.Bilinear"/>, <see cref="TextureFilterType.Trilinear"/>, and 
		/// <see cref="TextureFilterType.Anisotropic"/> filtering.
		/// </summary>
		bool IsMipmapped { get; }
		
		/// <summary>
		/// The number of mips included in this texture resource. If <see cref="IsMipmapped"/> is <c>false</c>, this property will
		/// always return <c>1U</c>.
		/// </summary>
		uint NumMips { get; }

		/// <summary>
		/// The texel format used by this texture (see also: <see cref="ITexel"/>).
		/// </summary>
		Type TexelFormat { get; }

		/// <summary>
		/// The size, in bytes, of a single texel in this texture.
		/// </summary>
		uint TexelSizeBytes { get; }

		/// <summary>
		/// The number of texels in this texture (including texels allocated for mips).
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods",
			Justification = "It's right, really. But this will stay the same for consistency's sake.")]
		uint SizeTexels { get; }

		/// <summary>
		/// If true, this texture is part of an <see cref="ITextureArray"/>. Array textures should not be disposed (rather, their
		/// parent arrays should be).
		/// </summary>
		bool IsArrayTexture { get; }

		/// <summary>
		/// If <see cref="IsArrayTexture"/> is true, returns the index of this texture in its owning array.
		/// </summary>
		uint ArrayIndex { get; }

		/// <summary>
		/// If true, mips can be automatically generated for this texture by calling <see cref="GenerateMips"/>.
		/// </summary>
		bool IsMipGenTarget { get; }

		/// <summary>
		/// If true, the maximum detail of this texture (regardless of distance to camera) can be limited by changing the 
		/// global detail level reduction (see: <see cref="RenderingModule.SetTextureDetailReduction"/>).
		/// </summary>
		bool IsGlobalDetailTarget { get; }

		/// <summary>
		/// Returns a subresource index that is used to internally identify a mip level. For non-array textures, the subresource
		/// index is equal to the given <paramref name="mipIndex"/>, but for textures where <see cref="IsArrayTexture"/> returns
		/// <c>true</c>; this method must be used to uniquely identify a single mip level.
		/// </summary>
		/// <param name="mipIndex">The mip level to get the subresource identifier for, where <c>0</c> is the top-level mip.</param>
		/// <returns></returns>
		/// <exception cref="AssuranceFailedException">(Debug only) Thrown if <paramref name="mipIndex"/> is greater than or equal to
		/// <see cref="NumMips"/>.</exception>
		uint GetSubresourceIndex(uint mipIndex);

		/// <summary>
		/// Returns the space required for a given mip for this texture, where <c>0</c> is the top-level mip.
		/// </summary>
		/// <param name="mipIndex">The mip level to get the size of.</param>
		/// <returns>The size required in memory for the given mip level.</returns>
		/// <exception cref="AssuranceFailedException">(Debug only) Thrown if <paramref name="mipIndex"/> is greater than or equal to
		/// <see cref="NumMips"/>.</exception>
		ByteSize GetSize(uint mipIndex);

		/// <summary>
		/// Returns the number of texels in a given mip for this texture, where <c>0</c> is the top-level mip.
		/// </summary>
		/// <param name="mipIndex">The mip level to get the size of.</param>
		/// <returns>The number of texels that makes up the given <paramref name="mipIndex"/>.</returns>
		/// <exception cref="AssuranceFailedException">(Debug only) Thrown if <paramref name="mipIndex"/> is greater than or equal to
		/// <see cref="NumMips"/>.</exception>
		uint GetSizeTexels(uint mipIndex);

		/// <summary>
		/// Uses the graphics pipeline to generate automatic mip levels for this texture.
		/// The mips will be added directly to the resource data. The texture must have been created with the
		/// <see cref="IsMipGenTarget"/> flag set to <c>true</c>.
		/// </summary>
		void GenerateMips();

		/// <summary>
		/// Creates a new view to this texture.
		/// </summary>
		/// <returns>A new shader resource view that exposes the entirety of this texture.</returns>
		ShaderTextureResourceView CreateView();

		/// <summary>
		/// Creates a new view to this texture.
		/// </summary>
		/// <param name="firstMipIndex">The index of the first mip-level in the texture that will be accessible through the returned view.</param>
		/// <param name="numMips">The number of mip-levels in this texture that will be accessible through the returned view.</param>
		/// <returns>A new resource view that permits reading data from this texture.</returns>
		ShaderTextureResourceView CreateView(uint firstMipIndex, uint numMips);

		/// <summary>
		/// Creates a new unordered access view to this texture.
		/// </summary>
		/// <param name="mipIndex">The mip-level to create a view to. Only one mip-level can be exposed in an <see cref="UnorderedAccessView"/>.</param>
		/// <returns>A new resource view that permits reading and writing data from/to this texture.</returns>
		UnorderedTextureAccessView CreateUnorderedAccessView(uint mipIndex);
	}


	/// <summary>
	/// Represents an array of <see cref="ITexture"/> resources.
	/// </summary>
	/// <remarks>
	/// Often, when grouping commonly-used-together textures, using an <see cref="ITextureArray"/> can be more performant than using an
	/// explicit array of <see cref="ITexture"/> objects (partially because they will be allocated together in video memory).
	/// </remarks>
	public interface ITextureArray : IResource {
		/// <summary>
		/// If true, the textures in this array contain memory allocated for texture 'mipmaps', which are used in 
		/// <see cref="TextureFilterType.Bilinear"/>, <see cref="TextureFilterType.Trilinear"/>, and 
		/// <see cref="TextureFilterType.Anisotropic"/> filtering.
		/// </summary>
		bool IsMipmapped { get; }
		
		/// <summary>
		/// The number of mips included in each texture in this texture array. 
		/// If <see cref="IsMipmapped"/> is <c>false</c>, this property will always return <c>1U</c>.
		/// </summary>
		uint NumMips { get; }

		/// <summary>
		/// The type of <see cref="ITexture"/> of each element in this array.
		/// </summary>
		Type TextureType { get; }

		/// <summary>
		/// The texel format used by each texture in this array (see also: <see cref="ITexel"/>).
		/// </summary>
		Type TexelFormat { get; }

		/// <summary>
		/// The size, in bytes, of a single texel in this texture.
		/// </summary>
		uint TexelSizeBytes { get; }

		/// <summary>
		/// The number of texels in this entire array (e.g. the texel size of a single element, multiplied by <see cref="ArrayLength"/>).
		/// </summary>
		/// <remarks>
		/// To get the size in texels of any single element, divide this value by <see cref="ArrayLength"/>.
		/// </remarks>
		uint SizeTexels { get; }

		/// <summary>
		/// The number of textures in this array.
		/// </summary>
		uint ArrayLength { get; }

		/// <summary>
		/// If true, mips can be automatically generated for textures in this array by calling 
		/// <see cref="GenerateMips"/>.
		/// </summary>
		bool IsMipGenTarget { get; }

		/// <summary>
		/// If true, the maximum detail of textures in this array (regardless of distance to camera) can be limited by changing the 
		/// global detail level reduction (see: <see cref="RenderingModule.SetTextureDetailReduction"/>).
		/// </summary>
		bool IsGlobalDetailTarget { get; }

		/// <summary>
		/// Returns a subresource index that is used to internally identify a mip level on a specific array element.
		/// </summary>
		/// <param name="arrayIndex">The array element whose mip it is you wish to identify, where <c>0</c> is the first element.</param>
		/// <param name="mipIndex">The mip level to get the subresource identifier for, where <c>0</c> is the top-level mip.</param>
		/// <returns></returns>
		/// <exception cref="AssuranceFailedException">(Debug only) Thrown if <paramref name="mipIndex"/> is greater than or equal to
		/// <see cref="NumMips"/>, or <paramref name="arrayIndex"/> is greater than or equal to <see cref="ArrayLength"/>.</exception>
		uint GetSubresourceIndex(uint arrayIndex, uint mipIndex);

		/// <summary>
		/// Returns the <see cref="ITexture"/> at the given array <paramref name="index"/>.
		/// </summary>
		/// <param name="index">The index into the array.</param>
		/// <returns>The texture at the requested index.</returns>
		/// <exception cref="IndexOutOfRangeException">Thrown if <paramref name="index"/> is greater than or equal to
		/// <see cref="ArrayLength"/>.</exception>
		ITexture ElementAt(uint index);

		/// <summary>
		/// Uses the graphics pipeline to generate automatic mip levels for each texture in this array.
		/// The mips will be added directly to the resource data. The texture must have been created with the
		/// <see cref="IsMipGenTarget"/> flag set to <c>true</c>.
		/// </summary>
		void GenerateMips();

		/// <summary>
		/// Creates a new view to this texture array.
		/// </summary>
		/// <returns>A new shader resource view that exposes the entirety of every texture in the array.</returns>
		ShaderTextureArrayResourceView CreateView();

		/// <summary>
		/// Creates a new view to this texture array.
		/// </summary>
		/// <param name="firstMipIndex">The index of the first mip-level in each texture to expose via the returned view.</param>
		/// <param name="numMips">The number of mips in each texture to expose via the returned view.</param>
		/// <param name="firstArrayIndex">The index of the first texture in the array to expose via the returned view.</param>
		/// <param name="numArrayElements">The number of textures in the array to expose via the returned view.</param>
		/// <returns>A new resource view that permits reading data from this texture array.</returns>
		ShaderTextureArrayResourceView CreateView(uint firstMipIndex, uint numMips, uint firstArrayIndex, uint numArrayElements);

		/// <summary>
		/// Creates a new unordered access view to this texture array.
		/// </summary>
		/// <param name="mipIndex">The mip level that will be exposed in every exposed texture in the array.
		/// Only one mip-level can be exposed in an <see cref="UnorderedAccessView"/>.</param>
		/// <param name="firstArrayIndex">The index of the first texture in the array to expose via the returned view.</param>
		/// <param name="numArrayElements">The number of textures in the array to expose via the returned view.</param>
		/// <returns>A new resource view that permits reading and writing data from/to this texture array.</returns>
		UnorderedTextureArrayAccessView CreateUnorderedAccessView(uint mipIndex, uint firstArrayIndex, uint numArrayElements);
	}

	/// <summary>
	/// An <see cref="ITexture"/> with 1 dimension (width).
	/// </summary>
	public interface ITexture1D : ITexture {
		/// <summary>
		/// The width, in texels, of this texture.
		/// </summary>
		uint Width { get; }

		/// <summary>
		/// The width, in texels, of the requested mip.
		/// </summary>
		/// <param name="mipIndex">The index of the mip level whose width it is you wish to ascertain.</param>
		/// <returns>The width, in texels, of the given mip index.</returns>
		uint MipWidth(uint mipIndex);
	}

	/// <summary>
	/// An <see cref="ITextureArray"/> of 1-dimensional textures.
	/// </summary>
	public interface ITexture1DArray : ITextureArray {
		/// <summary>
		/// The width, in texels, of each texture in this array.
		/// </summary>
		uint Width { get; }

		/// <summary>
		/// The width, in texels, of the requested mip; for any given texture in this array.
		/// </summary>
		/// <param name="mipIndex">The index of the mip level whose width it is you wish to ascertain.</param>
		/// <returns>The width, in texels, of the given mip index.</returns>
		uint MipWidth(uint mipIndex);

		/// <summary>
		/// Returns the <see cref="ITexture1D"/> at the given array <paramref name="index"/>.
		/// </summary>
		/// <param name="index">The index into the array.</param>
		/// <returns>The texture at the requested index.</returns>
		new ITexture1D ElementAt(uint index);
	}

	/// <summary>
	/// An <see cref="ITexture"/> with 2 dimensions (width &amp; height).
	/// </summary>
	public interface ITexture2D : ITexture {
		/// <summary>
		/// The width, in texels, of this texture.
		/// </summary>
		uint Width { get; }

		/// <summary>
		/// The height, in texels, of this texture.
		/// </summary>
		uint Height { get; }

		/// <summary>
		/// Returns true if this texture is multisampled.
		/// </summary>
		bool IsMultisampled { get; }

		/// <summary>
		/// The width, in texels, of the requested mip.
		/// </summary>
		/// <param name="mipIndex">The index of the mip level whose width it is you wish to ascertain.</param>
		/// <returns>The width, in texels, of the given mip index.</returns>
		uint MipWidth(uint mipIndex);

		/// <summary>
		/// The height, in texels, of the requested mip.
		/// </summary>
		/// <param name="mipIndex">The index of the mip level whose height it is you wish to ascertain.</param>
		/// <returns>The height, in texels, of the given mip index.</returns>
		uint MipHeight(uint mipIndex);

		/// <summary>
		/// Creates a new view to this resource that is capable of using the texture as a render target.
		/// </summary>
		/// <remarks>
		/// The texture's <see cref="ITexture.TexelFormat"/> must be <see cref="TexelFormat.RenderTarget"/>.
		/// This texture must not be mipmapped.
		/// The texture must have been created with the <see cref="GPUBindings.RenderTarget"/> GPU binding.
		/// </remarks>
		/// <param name="mipIndex">The mip level to create a render-target-view to.</param>
		/// <returns>A new render-target-view to the entirety of the texture.</returns>
		RenderTargetView CreateRenderTargetView(uint mipIndex);
		/// <summary>
		/// Creates a new view to this resource that is capable of using the texture as a depth/stencil buffer.
		/// </summary>
		/// <remarks>
		/// The texture's <see cref="ITexture.TexelFormat"/> must be <see cref="TexelFormat.DepthStencil"/>.
		/// This texture must not be mipmapped.
		/// The texture must have been created with the <see cref="GPUBindings.DepthStencilTarget"/> GPU binding.
		/// </remarks>
		/// <param name="mipIndex">The mip level to create a depth-stencil-view to.</param>
		/// <returns>A new depth-stencil-view to the entirety of the texture.</returns>
		DepthStencilView CreateDepthStencilView(uint mipIndex);

		byte[] ReadRaw();
	}

	/// <summary>
	/// An <see cref="ITextureArray"/> of 2-dimensional textures.
	/// </summary>
	public interface ITexture2DArray : ITextureArray {
		/// <summary>
		/// The width, in texels, of each texture in this array.
		/// </summary>
		uint Width { get; }

		/// <summary>
		/// The height, in texels, of each texture in this array.
		/// </summary>
		uint Height { get; }

		/// <summary>
		/// Whether or not the textures in this array are multisampled.
		/// </summary>
		bool IsMultisampled { get; }

		/// <summary>
		/// The width, in texels, of the requested mip; for any given texture in this array.
		/// </summary>
		/// <param name="mipIndex">The index of the mip level whose width it is you wish to ascertain.</param>
		/// <returns>The width, in texels, of the given mip index.</returns>
		uint MipWidth(uint mipIndex);

		/// <summary>
		/// The height, in texels, of the requested mip; for any given texture in this array.
		/// </summary>
		/// <param name="mipIndex">The index of the mip level whose width it is you wish to ascertain.</param>
		/// <returns>The height, in texels, of the given mip index.</returns>
		uint MipHeight(uint mipIndex);

		/// <summary>
		/// Returns the <see cref="ITexture2D"/> at the given array <paramref name="index"/>.
		/// </summary>
		/// <param name="index">The index into the array.</param>
		/// <returns>The texture at the requested index.</returns>
		new ITexture2D ElementAt(uint index);
	}

	/// <summary>
	/// An <see cref="ITexture"/> with 3 dimensions (width &amp; height &amp; depth).
	/// </summary>
	public interface ITexture3D : ITexture {
		/// <summary>
		/// The width, in texels, of this texture.
		/// </summary>
		uint Width { get; }

		/// <summary>
		/// The height, in texels, of this texture.
		/// </summary>
		uint Height { get; }

		/// <summary>
		/// The depth, in texels, of this texture.
		/// </summary>
		uint Depth { get; }

		/// <summary>
		/// The width, in texels, of the requested mip.
		/// </summary>
		/// <param name="mipIndex">The index of the mip level whose width it is you wish to ascertain.</param>
		/// <returns>The width, in texels, of the given mip index.</returns>
		uint MipWidth(uint mipIndex);

		/// <summary>
		/// The height, in texels, of the requested mip.
		/// </summary>
		/// <param name="mipIndex">The index of the mip level whose width it is you wish to ascertain.</param>
		/// <returns>The height, in texels, of the given mip index.</returns>
		uint MipHeight(uint mipIndex);

		/// <summary>
		/// The depth, in texels, of the requested mip.
		/// </summary>
		/// <param name="mipIndex">The index of the mip level whose width it is you wish to ascertain.</param>
		/// <returns>The depth, in texels, of the given mip index.</returns>
		uint MipDepth(uint mipIndex);

		/// <summary>
		/// Creates a new unordered access view to this texture.
		/// </summary>
		/// <param name="mipIndex">The mip-level to create a view to. Only one mip-level can be exposed in an <see cref="UnorderedAccessView"/>.</param>
		/// <param name="firstSliceIndex">The index of the first slice of this 3D texture to expose through the returned view.</param>
		/// <param name="numSlices">The number of slices of this 3D texture to expose through the returned view.</param>
		/// <returns>A new resource view that permits reading and writing data from/to this texture.</returns>
		UnorderedTextureAccessView CreateUnorderedAccessView(uint mipIndex, uint firstSliceIndex, uint numSlices);
	}
}