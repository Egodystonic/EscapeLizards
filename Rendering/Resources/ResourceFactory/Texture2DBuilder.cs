// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 05 11 2014 at 10:54 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// An <see cref="IResourceBuilder"/> that can create <see cref="Texture2D{TTexel}">2D texture</see>s.
	/// </summary>
	/// <typeparam name="TTexel">A <see cref="Extensions.IsBlittable">blittable</see> struct type that represents a single texel. Must be
	/// a valid <see cref="ITexel"/>.
	/// </typeparam>
	public sealed class Texture2DBuilder<TTexel> : BaseResourceBuilder<Texture2DBuilder<TTexel>, Texture2D<TTexel>, ArraySlice<TTexel>?> 
	where TTexel : struct, ITexel {
		private readonly GPUBindings permittedBindings;
		private readonly uint width;
		private readonly uint height;
		private readonly bool mipAllocation;
		private readonly bool dynamicDetail;
		private readonly bool mipGenerationTarget;
		private readonly uint texelSizeBytes = typeof(TTexel).GetCustomAttribute<TexelFormatMetadataAttribute>().FormatSizeBytes;
		private readonly ResourceFormat texelFormat = (ResourceFormat) typeof(TTexel).GetCustomAttribute<TexelFormatMetadataAttribute>().ResourceFormatIndex;
		private readonly uint numMips;
		private readonly bool multisampling;

		internal Texture2DBuilder()
			: this(ResourceUsage.Immutable, null, GPUBindings.ReadableShaderResource, 0U, 0U, false, false, false, false) { }

		internal Texture2DBuilder(ResourceUsage usage, ArraySlice<TTexel>? initialData, GPUBindings permittedBindings, 
			uint width, uint height, bool mipAllocation, bool mipGenerationTarget, bool dynamicDetail, bool multisampling) 
			: base(usage, initialData) {
			Assure.True(typeof(TTexel).IsBlittable());
			Assure.GreaterThan(UnsafeUtils.SizeOf<TTexel>(), 0);
			this.permittedBindings = permittedBindings;
			this.width = width;
			this.height = height;
			this.mipAllocation = mipAllocation;
			this.mipGenerationTarget = mipGenerationTarget;
			this.dynamicDetail = dynamicDetail;
			this.multisampling = multisampling;
			this.numMips = mipAllocation ? TextureUtils.GetNumMips(width, height) : 1U;
		}

		/// <summary>
		/// Sets the <see cref="ResourceUsage"/> to use when creating the <see cref="Texture2D{TTexel}"/>.
		/// See the <see cref="SupportedUsagesAttribute"/> on <see cref="Texture2D{TTexel}"/> for a list of permitted usages.
		/// </summary>
		/// <param name="usage">The usage to use/set.</param>
		/// <returns>An identical Texture2DBuilder with the specified <paramref name="usage"/> parameter.</returns>
		public override Texture2DBuilder<TTexel> WithUsage(ResourceUsage usage) {
			return new Texture2DBuilder<TTexel>(usage, InitialData, permittedBindings, width, height, mipAllocation, mipGenerationTarget, dynamicDetail, multisampling);
		}

		/// <summary>
		/// Sets the initial data to use when creating the <see cref="Texture2D{TTexel}"/>.
		/// </summary>
		/// <param name="initialData">The initial data.
		/// The data's <see cref="ArraySlice{T}.Length"/>
		/// property must be a specific value, depending on the other parameters of the builder:
		/// <para>
		/// If the textures are being created WITHOUT mip allocation, the data length should be equal to <c>width * height</c>.
		/// </para>
		/// <para>
		/// If the textures are being created WITH mip allocation, the data length should be equal to
		/// <see cref="TextureUtils.GetSizeTexels">GetSizeTexels</see><c>(true, width)</c>.
		/// </para>
		/// <para>
		/// If you create an array of textures (with <see cref="CreateArray"/>), the initial data length must correspond to the rules
		/// above, but multiplied by the array length (the start of the data for each texture should follow the
		/// end of the data for the texture before it).
		/// </para>
		/// </param>
		/// <returns>An identical Texture2DBuilder with the specified <paramref name="initialData"/>.</returns>
		public override Texture2DBuilder<TTexel> WithInitialData(ArraySlice<TTexel>? initialData) {
			return new Texture2DBuilder<TTexel>(Usage, initialData, permittedBindings, width, height, mipAllocation, mipGenerationTarget, dynamicDetail, multisampling);
		}

		/// <summary>
		/// Sets the permitted <see cref="GPUBindings"/> to use when creating the <see cref="Texture2D{TTexel}"/>.
		/// See the <see cref="SupportedGPUBindingsAttribute"/> on <see cref="Texture2D{TTexel}"/> for a list of supported options.
		/// </summary>
		/// <param name="permittedBindings">The bindings to use/set.</param>
		/// <returns>An identical Texture2DBuilder with the specified <paramref name="permittedBindings"/>.</returns>
		public Texture2DBuilder<TTexel> WithPermittedBindings(GPUBindings permittedBindings) {
			return new Texture2DBuilder<TTexel>(Usage, InitialData, permittedBindings, width, height, mipAllocation, mipGenerationTarget, dynamicDetail, multisampling);
		}

		/// <summary>
		/// Changes the <see cref="ITexel">texel format</see> to use when creating the <see cref="Texture2D{TTexel}"/>.
		/// </summary>
		/// <typeparam name="TTexelNew">A <see cref="Extensions.IsBlittable">blittable</see> struct type that represents a single texel.
		/// Must be a valid <see cref="ITexel"/>.</typeparam>
		/// <returns>An identical Texture2DBuilder with the specified texel format.</returns>
		public Texture2DBuilder<TTexelNew> WithTexelFormat<TTexelNew>() where TTexelNew : struct, ITexel {
			return new Texture2DBuilder<TTexelNew>(Usage, null, permittedBindings, width, height, mipAllocation, mipGenerationTarget, dynamicDetail, multisampling);
		}

		/// <summary>
		/// Sets the <paramref name="width"/> to use when creating the <see cref="Texture2D{TTexel}"/>.
		/// </summary>
		/// <param name="width">The new width, in texels.</param>
		/// <returns>An identical Texture2DBuilder with the specified <paramref name="width"/>.</returns>
		public Texture2DBuilder<TTexel> WithWidth(uint width) {
			return new Texture2DBuilder<TTexel>(Usage, InitialData, permittedBindings, width, height, mipAllocation, mipGenerationTarget, dynamicDetail, multisampling);
		}

		/// <summary>
		/// Sets the <paramref name="height"/> to use when creating the <see cref="Texture2D{TTexel}"/>.
		/// </summary>
		/// <param name="height">The new height, in texels.</param>
		/// <returns>An identical Texture2DBuilder with the specified <paramref name="height"/>.</returns>
		public Texture2DBuilder<TTexel> WithHeight(uint height) {
			return new Texture2DBuilder<TTexel>(Usage, InitialData, permittedBindings, width, height, mipAllocation, mipGenerationTarget, dynamicDetail, multisampling);
		}

		/// <summary>
		/// Sets whether or not to allocate memory for a texture mipmap when creating the <see cref="Texture2D{TTexel}"/>. Mip maps are
		/// used in <see cref="TextureFilterType.Bilinear"/>, <see cref="TextureFilterType.Trilinear"/>, and 
		/// <see cref="TextureFilterType.Anisotropic"/> filtering algorithms; 
		/// but require extra memory (see: <see cref="TextureUtils.GetSizeTexels"/>).
		/// <para>
		/// Additionally, if allocating mips and providing initial data, the initial data must be of correct size to fill the original
		/// texture and the corresponding mipmap. See the notes on <see cref="WithInitialData"/>.
		/// </para>
		/// </summary>
		/// <param name="mipAllocation">If true, the texture will be created with provision for mipped data; from the original dimensions
		/// down to a 1x1x1 texture.</param>
		/// <returns>An identical Texture2DBuilder with the specified <paramref name="mipAllocation"/> parameter.</returns>
		public Texture2DBuilder<TTexel> WithMipAllocation(bool mipAllocation) {
			return new Texture2DBuilder<TTexel>(Usage, InitialData, permittedBindings, width, height, mipAllocation, mipGenerationTarget, dynamicDetail, multisampling);
		}

		/// <summary>
		/// Sets whether or not the created <see cref="Texture2D{TTexel}"/> will have its detail level controllable via 
		/// <see cref="RenderingModule.SetTextureDetailReduction"/>. If true, <see cref="WithMipAllocation"/> must have been called with 
		/// <c>mipAllocation</c> set to true. If false, the texture detail will remain constant, regardless of the texture detail reduction
		/// level; though it may still be filtered if mipmapped (according to camera distance).
		/// </summary>
		/// <param name="dynamicDetail">Whether or not to allow this texture's detail level to be globally reduced.</param>
		/// <returns>An identical Texture2DBuilder with the specified <paramref name="dynamicDetail"/> parameter.</returns>
		public Texture2DBuilder<TTexel> WithDynamicDetail(bool dynamicDetail) {
			return new Texture2DBuilder<TTexel>(Usage, InitialData, permittedBindings, width, height, mipAllocation, mipGenerationTarget, dynamicDetail, multisampling);
		}

		/// <summary>
		/// Sets whether or not this texture's mips can be generated automatically by the graphics card (by calling 
		/// <see cref="ITexture.GenerateMips"/>). If true, <see cref="WithMipAllocation"/> 
		/// must have also been called with <c>mipAllocation</c> set to true. Additionally, the resource must have the 
		/// <see cref="GPUBindings.RenderTarget"/> and <see cref="GPUBindings.ReadableShaderResource"/> bindings permitted, and can
		/// not have any initial data supplied (instead, you must write the top-level mip with one of the write methods).
		/// </summary>
		/// <param name="mipGenerationTarget">If true, this texture's mipmap may be generated algorithmically from the top-level mip.</param>
		/// <returns>An identical Texture2DBuilder with the specified <paramref name="mipGenerationTarget"/> parameter.</returns>
		public Texture2DBuilder<TTexel> WithMipGenerationTarget(bool mipGenerationTarget) {
			return new Texture2DBuilder<TTexel>(Usage, InitialData, permittedBindings, width, height, mipAllocation, mipGenerationTarget, dynamicDetail, multisampling);
		}

		/// <summary>
		/// Sets whether or not this texture will support D3D11 MSAA (multisample anti-aliasing), a technique that supersizes the texture
		/// and then performs an aggregation function to reduce it back to the target size. 
		/// This technique can be used to reduce 'aliasing', an artifact of drawing lines and edges in discrete samples.
		/// </summary>
		/// <remarks>
		/// You may not enable multisampling on a texture that also has mips allocated (see: <see cref="WithMipAllocation"/>), and 
		/// multisampled textures may not have initial data provided (see: <see cref="WithInitialData"/>).
		/// <para>
		/// Only 2D textures may be multisampled.
		/// </para>
		/// </remarks>
		/// <param name="multisampling">Whether or not this texture will be multisampled.</param>
		/// <returns>An identical Texture2DBuilder with the specified <paramref name="multisampling"/> parameter.</returns>
		public Texture2DBuilder<TTexel> WithMultisampling(bool multisampling) {
			return new Texture2DBuilder<TTexel>(Usage, InitialData, permittedBindings, width, height, mipAllocation, mipGenerationTarget, dynamicDetail, multisampling);
		}

		public Texture2DBuilder<TTexel> WithInitialData(ArraySlice<byte> initialDataRaw) {
			var handle = GCHandle.Alloc(initialDataRaw.ContainingArray, GCHandleType.Pinned);
			TTexel[] actualInitData = new TTexel[TextureUtils.GetSizeTexels(mipAllocation, width, height)];

			try {
				UnsafeUtils.CopyGenericArray<TTexel>(handle.AddrOfPinnedObject() + (int) initialDataRaw.Offset, actualInitData, texelSizeBytes);
			}
			finally {
				handle.Free();
			}

			return WithInitialData(actualInitData);
		} 

		/// <summary>
		/// Creates a new <see cref="Texture2D{TTexel}"/> with the supplied builder parameters.
		/// </summary>
		/// <remarks>
		/// In debug mode, this method will check a large number of <see cref="Assure">assurances</see> 
		/// on the builder parameters before creating the resource.
		/// </remarks>
		/// <returns>A new <see cref="Texture2D{TTexel}"/>.</returns>
		public override Texture2D<TTexel> Create() {
			Texture2D<TTexel> result = new Texture2D<TTexel>(
				CreateTexture2D(1U, InitialData), 
				Usage, 
				TextureUtils.GetSize(texelSizeBytes, mipAllocation, width, height),
				permittedBindings, 
				false,
				mipGenerationTarget,
				dynamicDetail,
				width,
				height,
				texelSizeBytes, 
				mipAllocation,
				numMips, 
				0U,
				multisampling
			);

			if (dynamicDetail) RenderingModule.DynamicDetailTextures.Value.Add(new WeakReference<IResource>(result));
			return result;
		}

		/// <summary>
		/// Creates an array of <see cref="Texture2D{TTexel}"/>s; encapsulated as a <see cref="Texture2DArray{TTexel}"/> object.
		/// By grouping textures that are used together frequently in to an array, you can gain a performance improvement.
		/// You can not create an array of textures with usage '<see cref="ResourceUsage.DiscardWrite"/>'.
		/// </summary>
		/// <param name="numTextures">The number of textures in the array. Must not be zero.</param>
		/// <returns>A new <see cref="Texture2DArray{TTexel}"/>.</returns>
		public Texture2DArray<TTexel> CreateArray(uint numTextures) {
			Assure.False(Usage == ResourceUsage.DiscardWrite, "Can not create an array of discard-write textures.");

			Texture2DArray<TTexel> result = new Texture2DArray<TTexel>(
				CreateTexture2D(numTextures, InitialData),
				Usage,
				TextureUtils.GetSize(texelSizeBytes, mipAllocation, width, height) * numTextures,
				permittedBindings,
				numTextures,
				mipGenerationTarget,
				dynamicDetail,
				width,
				height,
				texelSizeBytes,
				mipAllocation,
				numMips,
				multisampling
			);

			if (dynamicDetail) RenderingModule.DynamicDetailTextures.Value.Add(new WeakReference<IResource>(result));
			return result;
		}

		private unsafe Texture2DResourceHandle CreateTexture2D(uint arrayLen, ArraySlice<TTexel>? initialData) {
			Assure.True(Usage != ResourceUsage.Immutable || initialData != null, "You must supply initial data to an immutable resource.");
			Assure.False(
				(Usage == ResourceUsage.Immutable || Usage == ResourceUsage.DiscardWrite) && permittedBindings == GPUBindings.None,
				"An immutable or discard-write resource with no permitted bindings is useless."
			);
			Assure.False(
				Usage.GetUsage() == 0x3 && permittedBindings != GPUBindings.None,
				"Staging resources can not be bound to the pipeline."
			);
			Assure.False((Usage == ResourceUsage.DiscardWrite || Usage == ResourceUsage.Immutable) && 
				((permittedBindings & GPUBindings.RenderTarget) > 0
				|| (permittedBindings & GPUBindings.DepthStencilTarget) > 0
				|| (permittedBindings & GPUBindings.WritableShaderResource) > 0),
				"Can not bind an immutable or discard-write texture as a render target or depth stencil target, or as a GPU-writeable shader resource."
			);

			Assure.GreaterThan(arrayLen, 0U, "Can not create an array of 0 textures.");

			Assure.GreaterThan(width, 0U, "Please specify a width for the texture.");
			Assure.GreaterThan(height, 0U, "Please specify a height for the texture.");

			Assure.False(
				mipAllocation && !MathUtils.IsPowerOfTwo(width),
				"Can not create mipmapped texture with any non-power-of-two (NPOT) dimension. Dimensions: " + width + "x" + height + "."
			);
			Assure.False(
				mipAllocation && !MathUtils.IsPowerOfTwo(height),
				"Can not create mipmapped texture with any non-power-of-two (NPOT) dimension. Dimensions: " + width + "x" + height + "."
			);
			Assure.False(
				mipAllocation && Usage == ResourceUsage.DiscardWrite,
				"Can not allocate mips on a discard-write texture."
			);
			Assure.False(
				mipGenerationTarget && !mipAllocation,
				"Can not generate mips without allocating space for them."
			);
			Assure.False(
				mipGenerationTarget && 
				((permittedBindings & GPUBindings.RenderTarget) == 0x0 || (permittedBindings & GPUBindings.ReadableShaderResource) == 0x0),
				"To make a texture a viable mip generation target, it must be created with the RenderTarget and ReadableShaderResource GPU bindings."
			);
			Assure.False(
				mipGenerationTarget && initialData != null,
				"Can not supply initial data to a mip generation target."
			);
			Assure.True(
				initialData == null
				|| (initialData.Value.Length == TextureUtils.GetSizeTexels(mipAllocation, width, height) * arrayLen),
				"Initial data is of incorrect length (" + (initialData != null ? initialData.Value.Length : 0) + ") for this resource. " +
					"It should have a length of: " + TextureUtils.GetSizeTexels(mipAllocation, width, height) * arrayLen + "."
			);
			Assure.False(dynamicDetail && Usage.GetUsage() == 0x3, "Can not create a dynamic-detail staging resource.");
			Assure.False(multisampling && mipAllocation, "Can not create mipmapped multisampled texture.");
			Assure.False(multisampling && initialData != null, "Can not supply initial data to a multisampled texture.");

			Texture2DResourceHandle outResourceHandle;
			GCHandle? pinnedInitData = null;
			GCHandle? pinnedDataHandle = null;
			IntPtr initialDataPtr = IntPtr.Zero;
			InitialResourceDataDesc[] dataArr = null;

			if (initialData != null) {
				pinnedInitData = GCHandle.Alloc(initialData.Value.ContainingArray, GCHandleType.Pinned);
				
				dataArr = InitialResourceDataDesc.CreateDataArr(
					pinnedInitData.Value.AddrOfPinnedObject() + (int) (initialData.Value.Offset * texelSizeBytes),
					arrayLen,
					numMips,
					width,
					height,
					1U,
					texelSizeBytes
				);

				pinnedDataHandle = GCHandle.Alloc(dataArr, GCHandleType.Pinned);
				initialDataPtr = pinnedDataHandle.Value.AddrOfPinnedObject();
			}

			try {
				InteropUtils.CallNative(NativeMethods.ResourceFactory_CreateTexture2DArray,
					RenderingModule.Device,
					width,
					height,
					arrayLen,
					(InteropBool) mipAllocation,
					(InteropBool) multisampling,
					texelFormat,
					Usage.GetUsage(),
					Usage.GetCPUUsage(),
					(PipelineBindings) permittedBindings,
					(InteropBool) mipGenerationTarget,
					(InteropBool) dynamicDetail,
					initialDataPtr,
					dataArr != null ? (uint) dataArr.Length : 0U,
					(IntPtr) (&outResourceHandle)
				).ThrowOnFailure();
			}
			finally {
				if (pinnedDataHandle != null) pinnedDataHandle.Value.Free();
				if (pinnedInitData != null) pinnedInitData.Value.Free();
			}

			return outResourceHandle;
		}
	}
}