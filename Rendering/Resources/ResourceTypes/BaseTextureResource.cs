// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 10 11 2014 at 11:40 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// Abstract class that provides some common functionality for texture resources.
	/// </summary>
	/// <typeparam name="TTexel">The <see cref="ITexel"/> for texels in this texture.</typeparam>
	/// <typeparam name="TTexture">The actual texture type.</typeparam>
	/// <typeparam name="TBuilder">The <see cref="IResourceBuilder"/> that is used to create textures of
	/// type <typeparamref name="TTexture"/>.</typeparam>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes",
		Justification = "See justification on BaseBuilder.")]
	public abstract class BaseTextureResource<TTexel, TTexture, TBuilder> : BaseResource, ITexture 
		where TTexel : struct, ITexel
		where TTexture : BaseResource, ITexture 
		where TBuilder : IResourceBuilder {
		private readonly bool isMipGenTarget;
		private readonly bool allowsDynamicDetail;
		private readonly GPUBindings permittedBindings;
		private readonly bool isArrayTexture;
		private readonly uint texelSizeBytes;
		private readonly bool isMipmapped;
		private readonly uint numMips;
		private readonly uint arrayIndex;
		private readonly uint subresourceStartIndex;

		/// <summary>
		/// Gets the set of permitted bindings to the GPU for this resource that determine where in the rendering pipeline this resource
		/// may be used.
		/// </summary>
		/// <remarks>
		/// Depending on this value, resources may be bound to one or
		/// more parts of the rendering pipeline; and subsequently may have different optional use-cases. However, resources should be
		/// created only with the <see cref="GPUBindings"/> required for their intended application, for maximum performance (see also:
		/// <see cref="GPUBindings"/>).
		/// </remarks>
		public override unsafe GPUBindings PermittedBindings {
			get { return permittedBindings; }
		}

		/// <summary>
		/// If true, this texture contains memory allocated for a texture 'mipmap', which is used in 
		/// <see cref="TextureFilterType.Bilinear"/>, <see cref="TextureFilterType.Trilinear"/>, and 
		/// <see cref="TextureFilterType.Anisotropic"/> filtering.
		/// </summary>
		public bool IsMipmapped {
			get { return isMipmapped; }
		}

		/// <summary>
		/// The number of mips included in this texture resource. If <see cref="ITexture.IsMipmapped"/> is <c>false</c>, this property will
		/// always return <c>1U</c>.
		/// </summary>
		public uint NumMips {
			get { return numMips; }
		}

		/// <summary>
		/// The texel format used by this texture (see also: <see cref="ITexel"/>).
		/// </summary>
		public Type TexelFormat {
			get { return typeof(TTexel); }
		}

		/// <summary>
		/// The size, in bytes, of a single texel in this texture.
		/// </summary>
		public uint TexelSizeBytes {
			get { return texelSizeBytes; }
		}

		/// <summary>
		/// The size in texels of this texture.
		/// </summary>
		public abstract uint SizeTexels { get; }

		/// <summary>
		/// If true, this texture is part of an <see cref="ITextureArray"/>. Array textures should not be disposed (rather, their
		/// parent arrays should be).
		/// </summary>
		public bool IsArrayTexture {
			get { return isArrayTexture; }
		}

		/// <summary>
		/// If true, mips can be automatically generated for this texture by calling 
		/// <see cref="ITexture.GenerateMips"/>.
		/// </summary>
		public bool IsMipGenTarget {
			get { return isMipGenTarget; }
		}

		/// <summary>
		/// If true, the maximum detail of this texture (regardless of distance to camera) can be limited by changing the 
		/// global detail level reduction (see: <see cref="RenderingModule.SetTextureDetailReduction"/>).
		/// </summary>
		public bool IsGlobalDetailTarget {
			get { return allowsDynamicDetail; }
		}

		/// <summary>
		/// If <see cref="ITexture.IsArrayTexture"/> is true, returns the index of this texture in its owning array.
		/// </summary>
		public uint ArrayIndex {
			get { return arrayIndex; }
		}

		internal BaseTextureResource(ResourceHandle resourceHandle, ResourceUsage usage, ByteSize size, 
			GPUBindings permittedBindings, bool isArrayTexture, 
			bool isMipGenTarget, bool allowsDynamicDetail,
			uint texelSizeBytes, bool isMipmapped, uint numMips, uint arrayIndex) 
			: base(resourceHandle, usage, size) {
			this.permittedBindings = permittedBindings;
			this.isArrayTexture = isArrayTexture;
			this.isMipGenTarget = isMipGenTarget;
			this.allowsDynamicDetail = allowsDynamicDetail;
			this.texelSizeBytes = texelSizeBytes;
			this.isMipmapped = isMipmapped;
			this.numMips = numMips;
			this.subresourceStartIndex = arrayIndex * numMips;
			this.arrayIndex = arrayIndex;
		}

		/// <summary>
		/// Returns a subresource index that is used to internally identify a mip level. For non-array textures, the subresource
		/// index is equal to the given <paramref name="mipIndex"/>, but for textures where <see cref="ITexture.IsArrayTexture"/> returns
		/// <c>true</c>; this method must be used to uniquely identify a single mip level.
		/// </summary>
		/// <param name="mipIndex">The mip level to get the subresource identifier for, where <c>0</c> is the top-level mip.</param>
		/// <returns></returns>
		/// <exception cref="AssuranceFailedException">(Debug only) Thrown if <paramref name="mipIndex"/> is greater than or equal to
		/// <see cref="ITexture.NumMips"/>.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public uint GetSubresourceIndex(uint mipIndex) {
			Assure.LessThan(mipIndex, numMips, "Mip index out of bounds.");
			return mipIndex + subresourceStartIndex;
		}

		/// <summary>
		/// Disposes this resource; deleting its data from the graphics card, and invalidating any further usage. If the resource has
		/// already been disposed, no action is taken.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2215:Dispose methods should call base class dispose",
			Justification = "Usually yes, but the whole point of this override is to explicitly NOT dispose.")]
		public override unsafe void Dispose() {
			if (isArrayTexture) {
				Logger.Warn("Can not dispose an array-member-texture: " +
					"You must dispose the array instead.");
			}
			else base.Dispose();
		}

		/// <summary>
		/// Returns an <see cref="IResourceBuilder"/> of type <typeparamref name="TBuilder"/> that has all its properties set
		/// to the same values as are used by this texture. A clone of this texture can then be created by calling
		/// <see cref="IResourceBuilder.Create"/> on the returned builder.
		/// </summary>
		/// <param name="includeData">If true, the <see cref="IResourceBuilder.WithInitialData">initial data</see> of the 
		/// returned resource builder will be set to a copy of the data contained within this resource. Requires that this
		/// resource's <see cref="BaseResource.CanRead"/> property returns <c>true</c>.</param>
		/// <returns>A new <typeparamref name="TBuilder"/>.</returns>
		/// <exception cref="ResourceOperationUnavailableException">Thrown if <paramref name="includeData"/> is <c>true</c>,
		/// but <see cref="BaseResource.CanRead"/> is <c>false</c>.</exception>
		public abstract TBuilder Clone(bool includeData = false);

		/// <summary>
		/// Copies this texture's data to the destination texture.
		/// </summary>
		/// <param name="dest">The destination resource. Must not be null.</param>
		/// <exception cref="AssuranceFailedException">(Debug only) Thrown if both resources' <see cref="BaseResource.Size"/>s or 
		/// <see cref="object.GetType">type</see>s are not equal; or if <c>this == dest</c>.</exception>
		/// <exception cref="ResourceOperationUnavailableException">Thrown if dest returns <c>false</c> for 
		/// <see cref="BaseResource.CanBeCopyDestination"/>.</exception>
		public void CopyTo(TTexture dest) {
			base.CopyTo(dest);
		}


		/// <summary>
		/// Uses the graphics pipeline to generate automatic mip levels for this texture.
		/// The mips will be added directly to the resource data. The texture must have been created with the
		/// <see cref="ITexture.IsMipGenTarget"/> flag set to <c>true</c>.
		/// </summary>
		public void GenerateMips() {
			Assure.True(IsMipGenTarget, "Texture must have been created with Mip Generation parameter set to true.");
			using (ShaderResourceView srv = CreateView()) {
				InteropUtils.CallNative(
					NativeMethods.ResourceFactory_GenerateMips,
					RenderingModule.DeviceContext,
					srv.ResourceViewHandle
				);
			}
		}

		/// <summary>
		/// Copies the data from this texture to a new array of <typeparamref name="TTexel"/>s.
		/// The data is arranged in increasing mip-index (with the 0-index, and therefore largest, mip first). Within each
		/// mip, the data is arranged by slice, then by row.
		/// </summary>
		/// <returns>A copy of the entirety of the texture data (including mips).</returns>
		public abstract TTexel[] ReadAll();

		/// <summary>
		/// Gets the size of the given mip.
		/// </summary>
		/// <param name="mipIndex">The mip to get the size of. Must be less than <see cref="NumMips"/>.</param>
		/// <returns>The <see cref="ByteSize"/> of the given mip.</returns>
		public abstract ByteSize GetSize(uint mipIndex);

		/// <summary>
		/// Gets the number of texels in the given mip.
		/// </summary>
		/// <param name="mipIndex">The mip to get the size of. Must be less than <see cref="NumMips"/>.</param>
		/// <returns>The number of texels in the given mip (e.g width * height * depth, where applicable).</returns>
		public abstract uint GetSizeTexels(uint mipIndex);

		/// <summary>
		/// Creates a new view to this texture.
		/// </summary>
		/// <returns>A new shader resource view that exposes the entirety of this texture.</returns>
		public ShaderTextureResourceView CreateView() {
			return CreateView(0U, NumMips);
		}

		/// <summary>
		/// Creates a new view to this texture.
		/// </summary>
		/// <param name="firstMipIndex">The index of the first mip-level in the texture that will be accessible through the returned view.</param>
		/// <param name="numMips">The number of mip-levels in this texture that will be accessible through the returned view.</param>
		/// <returns>A new resource view that permits reading data from this texture.</returns>
		public abstract ShaderTextureResourceView CreateView(uint firstMipIndex, uint numMips);

		/// <summary>
		/// Creates a new unordered access view to this texture.
		/// </summary>
		/// <param name="mipIndex">The mip-level to create a view to. Only one mip-level can be exposed in an <see cref="UnorderedAccessView"/>.</param>
		/// <returns>A new resource view that permits reading and writing data from/to this texture.</returns>
		public abstract UnorderedTextureAccessView CreateUnorderedAccessView(uint mipIndex);
	}
}