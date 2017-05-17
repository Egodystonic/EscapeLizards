// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 10 11 2014 at 11:40 by Ben Bowen

using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// Abstract class that provides some common functionality for texture array resources.
	/// </summary>
	/// <typeparam name="TTexel">The <see cref="ITexel"/> for texels in this texture.</typeparam>
	/// <typeparam name="TTexture">The actual texture type.</typeparam>
	/// <typeparam name="TBuilder">The <see cref="IResourceBuilder"/> that is used to create textures of
	/// type <typeparamref name="TTexture"/>.</typeparam>
	/// <typeparam name="TTextureArray">The actual texture array type.</typeparam>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes",
		Justification = "See justification on BaseBuilder")]
	public abstract class BaseTextureArrayResource<TTexel, TTexture, TTextureArray, TBuilder> 
		: BaseResource, ITextureArray, IEnumerable<TTexture>
		where TTexel : struct, ITexel
		where TTexture : BaseResource, ITexture 
		where TTextureArray : BaseResource, ITextureArray
		where TBuilder : IResourceBuilder {
		private readonly bool isMipGenTarget;
		private readonly bool allowsDynamicDetail;
		private readonly uint length;
		private readonly GPUBindings permittedBindings;
		private readonly TTexture[] texArray;
		private readonly uint texelSizeBytes;
		private readonly bool isMipmapped;
		private readonly uint numMips;

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
		/// If true, the textures in this array contain memory allocated for texture 'mipmaps', which are used in 
		/// <see cref="TextureFilterType.Bilinear"/>, <see cref="TextureFilterType.Trilinear"/>, and 
		/// <see cref="TextureFilterType.Anisotropic"/> filtering.
		/// </summary>
		public bool IsMipmapped {
			get { return isMipmapped; }
		}

		/// <summary>
		/// The number of mips included in each texture in this texture array. 
		/// If <see cref="ITextureArray.IsMipmapped"/> is <c>false</c>, this property will always return <c>1U</c>.
		/// </summary>
		public uint NumMips {
			get { return numMips; }
		}

		/// <summary>
		/// The type of <see cref="ITexture"/> of each element in this array.
		/// </summary>
		public Type TextureType {
			get { return typeof(TTexture);  }
		}

		/// <summary>
		/// The texel format used by each texture in this array (see also: <see cref="ITexel"/>).
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
		/// The number of texels in this entire array (e.g. the texel size of a single element, multiplied by <see cref="ITextureArray.ArrayLength"/>).
		/// </summary>
		/// <remarks>
		/// To get the size in texels of any single element, divide this value by <see cref="ITextureArray.ArrayLength"/>.
		/// </remarks>
		public uint SizeTexels {
			get { return length * this[0].SizeTexels; }
		}

		/// <summary>
		/// The number of textures in this array.
		/// </summary>
		public uint ArrayLength {
			get { return length; }
		}

		/// <summary>
		/// If true, mips can be automatically generated for textures in this array by calling 
		/// <see cref="ITexture.GenerateMips"/>.
		/// </summary>
		public bool IsMipGenTarget {
			get { return isMipGenTarget; }
		}

		/// <summary>
		/// If true, the maximum detail of textures in this array (regardless of distance to camera) can be limited by changing the 
		/// global detail level reduction (see: <see cref="RenderingModule.SetTextureDetailReduction"/>).
		/// </summary>
		public bool IsGlobalDetailTarget {
			get { return allowsDynamicDetail; }
		}

		internal BaseTextureArrayResource(ResourceHandle resourceHandle, ResourceUsage usage, ByteSize size, GPUBindings permittedBindings, 
			uint length, bool isMipGenTarget, bool allowsDynamicDetail,
			uint texelSizeBytes, bool isMipmapped, uint numMips, TTexture[] texArray) : base(resourceHandle, usage, size) {
			Assure.NotNull(texArray);
			this.permittedBindings = permittedBindings;
			this.length = length;
			this.isMipGenTarget = isMipGenTarget;
			this.allowsDynamicDetail = allowsDynamicDetail;
			this.texelSizeBytes = texelSizeBytes;
			this.isMipmapped = isMipmapped;
			this.numMips = numMips;
			this.texArray = texArray;
		}

		/// <summary>
		/// Returns an <see cref="IResourceBuilder"/> of type <typeparamref name="TBuilder"/> that has all its properties set
		/// to the same values as are used by textures in this array. A clone of this texture array can then be created by calling
		/// <c>CreateArray</c> on the returned builder.
		/// </summary>
		/// <param name="includeData">If true, the <see cref="IResourceBuilder.WithInitialData">initial data</see> of the 
		/// returned resource builder will be set to a concatenated copy of the data contained within each element in this resource.
		/// Requires that this resource's <see cref="BaseResource.CanRead"/> property returns <c>true</c>.</param>
		/// <returns>A new <typeparamref name="TBuilder"/>.</returns>
		/// <exception cref="ResourceOperationUnavailableException">Thrown if <paramref name="includeData"/> is <c>true</c>,
		/// but <see cref="BaseResource.CanRead"/> is <c>false</c>.</exception>
		public TBuilder Clone(bool includeData = false) {
			Assure.True(this[0] is BaseTextureResource<TTexel, TTexture, TBuilder>);
			// ReSharper disable PossibleNullReferenceException It's checked in the line above
			if (!includeData) {
				return (this[0] as BaseTextureResource<TTexel, TTexture, TBuilder>).Clone(false);
			}
			else {
				return (TBuilder) (this[0] as BaseTextureResource<TTexel, TTexture, TBuilder>).Clone(false).WithInitialData(
					new ArraySlice<TTexel>(
						this
						.Cast<BaseTextureResource<TTexel, TTexture, TBuilder>>()
						.SelectMany(tex => tex.ReadAll())
						.ToArray()
					)
				);
			}
			// ReSharper restore PossibleNullReferenceException
		}

		/// <summary>
		/// Copies this texture array's data to the destination texture array. Both arrays must have equal length.
		/// </summary>
		/// <param name="dest">The destination resource. Must not be null.</param>
		/// <exception cref="AssuranceFailedException">(Debug only) Thrown if both resources' <see cref="BaseResource.Size"/>s or 
		/// <see cref="object.GetType">type</see>s are not equal; or if <c>this == dest</c>.</exception>
		/// <exception cref="ResourceOperationUnavailableException">Thrown if dest returns <c>false</c> for 
		/// <see cref="BaseResource.CanBeCopyDestination"/>.</exception>
		public void CopyTo(TTextureArray dest) {
			base.CopyTo(dest);
		}

		/// <summary>
		/// Returns a subresource index that is used to internally identify a mip level on a specific array element.
		/// </summary>
		/// <param name="arrayIndex">The array element whose mip it is you wish to identify, where <c>0</c> is the first element.</param>
		/// <param name="mipIndex">The mip level to get the subresource identifier for, where <c>0</c> is the top-level mip.</param>
		/// <returns></returns>
		/// <exception cref="AssuranceFailedException">(Debug only) Thrown if <paramref name="mipIndex"/> is greater than or equal to
		/// <see cref="ITextureArray.NumMips"/>, or <paramref name="arrayIndex"/> is greater than or equal to <see cref="ITextureArray.ArrayLength"/>.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public uint GetSubresourceIndex(uint arrayIndex, uint mipIndex) {
			Assure.LessThan(arrayIndex, length, "Array index out of bounds.");
			Assure.LessThan(mipIndex, numMips, "Mip index out of bounds.");
			return mipIndex + arrayIndex * numMips;
		}

		/// <summary>
		/// Returns the <see cref="ITexture"/> at the given array <paramref name="index"/>.
		/// </summary>
		/// <param name="index">The index into the array.</param>
		/// <returns>The texture at the requested index.</returns>
		/// <exception cref="IndexOutOfRangeException">Thrown if <paramref name="index"/> is greater than or equal to
		/// <see cref="ArrayLength"/>.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ITexture ElementAt(uint index) {
			return this[(int) index];
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
		/// Disposes this resource; deleting its data from the graphics card, and invalidating any further usage. If the resource has
		/// already been disposed, no action is taken.
		/// </summary>
		public override unsafe void Dispose() {
			foreach (TTexture texture in texArray) {
				texture.IsDisposed = true;
			}
			base.Dispose();
		}

		/// <summary>
		/// Creates a new view to this texture array.
		/// </summary>
		/// <returns>A new shader resource view that exposes the entirety of every texture in the array.</returns>
		public ShaderTextureArrayResourceView CreateView() {
			return CreateView(0U, NumMips, 0U, ArrayLength);
		}

		/// <summary>
		/// Creates a new unordered access view to this texture array.
		/// </summary>
		/// <param name="mipIndex">The mip level that will be exposed in every exposed texture in the array.
		/// Only one mip-level can be exposed in an <see cref="UnorderedAccessView"/>.</param>
		/// <param name="firstArrayIndex">The index of the first texture in the array to expose via the returned view.</param>
		/// <param name="numArrayElements">The number of textures in the array to expose via the returned view.</param>
		/// <returns>A new resource view that permits reading and writing data from/to this texture array.</returns>
		public abstract UnorderedTextureArrayAccessView CreateUnorderedAccessView(uint mipIndex, uint firstArrayIndex, uint numArrayElements);

		/// <summary>
		/// Creates a new view to this texture array.
		/// </summary>
		/// <param name="firstMipIndex">The index of the first mip-level in each texture to expose via the returned view.</param>
		/// <param name="numMips">The number of mips in each texture to expose via the returned view.</param>
		/// <param name="firstArrayIndex">The index of the first texture in the array to expose via the returned view.</param>
		/// <param name="numArrayElements">The number of textures in the array to expose via the returned view.</param>
		/// <returns>A new resource view that permits reading data from this texture array.</returns>
		public abstract ShaderTextureArrayResourceView CreateView(uint firstMipIndex, uint numMips, uint firstArrayIndex, uint numArrayElements);

		/// <summary>
		/// Returns the <see cref="ITexture"/> at the given array <paramref name="index"/>.
		/// </summary>
		/// <param name="index">The index into the array.</param>
		/// <returns>The texture at the requested index.</returns>
		/// <exception cref="IndexOutOfRangeException">Thrown if <paramref name="index"/> is greater than or equal to
		/// <see cref="ArrayLength"/>.</exception>
		public TTexture this[int index] {
			get {
				return texArray[index];
			}
		}
		/// <summary>
		/// Returns the <see cref="ITexture"/> at the given array <paramref name="index"/>.
		/// </summary>
		/// <param name="index">The index into the array.</param>
		/// <returns>The texture at the requested index.</returns>
		/// <exception cref="IndexOutOfRangeException">Thrown if <paramref name="index"/> is greater than or equal to
		/// <see cref="ArrayLength"/>.</exception>
		public TTexture this[uint index] {
			get {
				return texArray[index];
			}
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		/// <summary>
		/// Returns an enumerator that iterates through the contained textures.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
		/// </returns>
		/// <filterpriority>1</filterpriority>
		public IEnumerator<TTexture> GetEnumerator() {
			return (texArray as IEnumerable<TTexture>).GetEnumerator();
		}
	}
}