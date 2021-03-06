﻿// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
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
	/// An <see cref="IResourceBuilder"/> that can create <see cref="Texture3D{TTexel}">3D texture</see>s.
	/// </summary>
	/// <typeparam name="TTexel">A <see cref="Extensions.IsBlittable">blittable</see> struct type that represents a single texel. Must be
	/// a valid <see cref="ITexel"/>.
	/// </typeparam>
	public sealed class Texture3DBuilder<TTexel> : BaseResourceBuilder<Texture3DBuilder<TTexel>, Texture3D<TTexel>, ArraySlice<TTexel>?> 
	where TTexel : struct, ITexel {
		private readonly GPUBindings permittedBindings;
		private readonly uint width;
		private readonly uint height;
		private readonly uint depth;
		private readonly bool mipAllocation;
		private readonly bool dynamicDetail;
		private readonly bool mipGenerationTarget;
		private readonly uint texelSizeBytes = typeof(TTexel).GetCustomAttribute<TexelFormatMetadataAttribute>().FormatSizeBytes;
		private readonly ResourceFormat texelFormat = (ResourceFormat) typeof(TTexel).GetCustomAttribute<TexelFormatMetadataAttribute>().ResourceFormatIndex;
		private readonly uint numMips;

		internal Texture3DBuilder()
			: this(ResourceUsage.Immutable, null, GPUBindings.ReadableShaderResource, 0U, 0U, 0U, false, false, false) { }

		internal Texture3DBuilder(ResourceUsage usage, ArraySlice<TTexel>? initialData, GPUBindings permittedBindings, 
			uint width, uint height, uint depth, bool mipAllocation, bool mipGenerationTarget, bool dynamicDetail) 
			: base(usage, initialData) {
			Assure.True(typeof(TTexel).IsBlittable());
			Assure.GreaterThan(UnsafeUtils.SizeOf<TTexel>(), 0);
			this.permittedBindings = permittedBindings;
			this.width = width;
			this.height = height;
			this.depth = depth;
			this.mipAllocation = mipAllocation;
			this.mipGenerationTarget = mipGenerationTarget;
			this.dynamicDetail = dynamicDetail;
			this.numMips = mipAllocation ? TextureUtils.GetNumMips(width, height) : 1U;
		}

		/// <summary>
		/// Sets the <see cref="ResourceUsage"/> to use when creating the <see cref="Texture3D{TTexel}"/>.
		/// See the <see cref="SupportedUsagesAttribute"/> on <see cref="Texture3D{TTexel}"/> for a list of permitted usages.
		/// </summary>
		/// <param name="usage">The usage to use/set.</param>
		/// <returns>An identical Texture3DBuilder with the specified <paramref name="usage"/> parameter.</returns>
		public override Texture3DBuilder<TTexel> WithUsage(ResourceUsage usage) {
			return new Texture3DBuilder<TTexel>(usage, InitialData, permittedBindings, width, height, depth, mipAllocation, mipGenerationTarget, dynamicDetail);
		}

		/// <summary>
		/// Sets the initial data to use when creating the <see cref="Texture3D{TTexel}"/>.
		/// </summary>
		/// <param name="initialData">The initial data.
		/// The data's <see cref="ArraySlice{T}.Length"/>
		/// property must be a specific value, depending on the other parameters of the builder:
		/// <para>
		/// If the textures are being created WITHOUT mip allocation, the data length should be equal to <c>width * height * depth</c>.
		/// </para>
		/// <para>
		/// If the textures are being created WITH mip allocation, the data length should be equal to
		/// <see cref="TextureUtils.GetSizeTexels">GetSizeTexels</see><c>(true, width, height, depth)</c>.
		/// </para>
		/// </param>
		/// <returns>An identical Texture3DBuilder with the specified <paramref name="initialData"/>.</returns>
		public override Texture3DBuilder<TTexel> WithInitialData(ArraySlice<TTexel>? initialData) {
			return new Texture3DBuilder<TTexel>(Usage, initialData, permittedBindings, width, height, depth, mipAllocation, mipGenerationTarget, dynamicDetail);
		}

		/// <summary>
		/// Sets the permitted <see cref="GPUBindings"/> to use when creating the <see cref="Texture3D{TTexel}"/>.
		/// See the <see cref="SupportedGPUBindingsAttribute"/> on <see cref="Texture3D{TTexel}"/> for a list of supported options.
		/// </summary>
		/// <param name="permittedBindings">The bindings to use/set.</param>
		/// <returns>An identical Texture3DBuilder with the specified <paramref name="permittedBindings"/>.</returns>
		public Texture3DBuilder<TTexel> WithPermittedBindings(GPUBindings permittedBindings) {
			return new Texture3DBuilder<TTexel>(Usage, InitialData, permittedBindings, width, height, depth, mipAllocation, mipGenerationTarget, dynamicDetail);
		}

		/// <summary>
		/// Changes the <see cref="ITexel">texel format</see> to use when creating the <see cref="Texture3D{TTexel}"/>.
		/// </summary>
		/// <typeparam name="TTexelNew">A <see cref="Extensions.IsBlittable">blittable</see> struct type that represents a single texel.
		/// Must be a valid <see cref="ITexel"/>.</typeparam>
		/// <returns>An identical Texture3DBuilder with the specified texel format.</returns>
		public Texture3DBuilder<TTexelNew> WithTexelFormat<TTexelNew>() where TTexelNew : struct, ITexel {
			return new Texture3DBuilder<TTexelNew>(Usage, null, permittedBindings, width, height, depth, mipAllocation, mipGenerationTarget, dynamicDetail);
		}

		/// <summary>
		/// Sets the <paramref name="width"/> to use when creating the <see cref="Texture3D{TTexel}"/>.
		/// </summary>
		/// <param name="width">The new width, in texels.</param>
		/// <returns>An identical Texture3DBuilder with the specified <paramref name="width"/>.</returns>
		public Texture3DBuilder<TTexel> WithWidth(uint width) {
			return new Texture3DBuilder<TTexel>(Usage, InitialData, permittedBindings, width, height, depth, mipAllocation, mipGenerationTarget, dynamicDetail);
		}

		/// <summary>
		/// Sets the <paramref name="height"/> to use when creating the <see cref="Texture3D{TTexel}"/>.
		/// </summary>
		/// <param name="height">The new height, in texels.</param>
		/// <returns>An identical Texture3DBuilder with the specified <paramref name="height"/>.</returns>
		public Texture3DBuilder<TTexel> WithHeight(uint height) {
			return new Texture3DBuilder<TTexel>(Usage, InitialData, permittedBindings, width, height, depth, mipAllocation, mipGenerationTarget, dynamicDetail);
		}

		/// <summary>
		/// Sets the <paramref name="depth"/> to use when creating the <see cref="Texture3D{TTexel}"/>.
		/// </summary>
		/// <param name="depth">The new depth, in texels.</param>
		/// <returns>An identical Texture3DBuilder with the specified <paramref name="depth"/>.</returns>
		public Texture3DBuilder<TTexel> WithDepth(uint depth) {
			return new Texture3DBuilder<TTexel>(Usage, InitialData, permittedBindings, width, height, depth, mipAllocation, mipGenerationTarget, dynamicDetail);
		}

		/// <summary>
		/// Sets whether or not to allocate memory for a texture mipmap when creating the <see cref="Texture3D{TTexel}"/>. Mip maps are
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
		/// <returns>An identical Texture3DBuilder with the specified <paramref name="mipAllocation"/> parameter.</returns>
		public Texture3DBuilder<TTexel> WithMipAllocation(bool mipAllocation) {
			return new Texture3DBuilder<TTexel>(Usage, InitialData, permittedBindings, width, height, depth, mipAllocation, mipGenerationTarget, dynamicDetail);
		}

		/// <summary>
		/// Sets whether or not the created <see cref="Texture3D{TTexel}"/> will have its detail level controllable via 
		/// <see cref="RenderingModule.SetTextureDetailReduction"/>. If true, <see cref="WithMipAllocation"/> must have been called with 
		/// <c>mipAllocation</c> set to true. If false, the texture detail will remain constant, regardless of the texture detail reduction
		/// level; though it may still be filtered if mipmapped (according to camera distance).
		/// </summary>
		/// <param name="dynamicDetail">Whether or not to allow this texture's detail level to be globally reduced.</param>
		/// <returns>An identical Texture3DBuilder with the specified <paramref name="dynamicDetail"/> parameter.</returns>
		public Texture3DBuilder<TTexel> WithDynamicDetail(bool dynamicDetail) {
			return new Texture3DBuilder<TTexel>(Usage, InitialData, permittedBindings, width, height, depth, mipAllocation, mipGenerationTarget, dynamicDetail);
		}

		/// <summary>
		/// Sets whether or not this texture's mips can be generated automatically by the graphics card (by calling 
		/// <see cref="ITexture.GenerateMips"/>). If true, <see cref="WithMipAllocation"/> 
		/// must have also been called with <c>mipAllocation</c> set to true. Additionally, the resource must have the 
		/// <see cref="GPUBindings.RenderTarget"/> and <see cref="GPUBindings.ReadableShaderResource"/> bindings permitted, and can
		/// not have any initial data supplied (instead, you must write the top-level mip with one of the write methods).
		/// </summary>
		/// <param name="mipGenerationTarget">If true, this texture's mipmap may be generated algorithmically from the top-level mip.</param>
		/// <returns>An identical Texture3DBuilder with the specified <paramref name="mipGenerationTarget"/> parameter.</returns>
		public Texture3DBuilder<TTexel> WithMipGenerationTarget(bool mipGenerationTarget) {
			return new Texture3DBuilder<TTexel>(Usage, InitialData, permittedBindings, width, height, depth, mipAllocation, mipGenerationTarget, dynamicDetail);
		}

		/// <summary>
		/// Creates a new <see cref="Texture3D{TTexel}"/> with the supplied builder parameters.
		/// </summary>
		/// <remarks>
		/// In debug mode, this method will check a large number of <see cref="Assure">assurances</see> 
		/// on the builder parameters before creating the resource.
		/// </remarks>
		/// <returns>A new <see cref="Texture3D{TTexel}"/>.</returns>
		public unsafe override Texture3D<TTexel> Create() {
			Assure.True(Usage != ResourceUsage.Immutable || InitialData != null, "You must supply initial data to an immutable resource.");
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

			Assure.GreaterThan(width, 0U, "Please specify a width for the texture.");
			Assure.GreaterThan(height, 0U, "Please specify a height for the texture.");
			Assure.GreaterThan(depth, 0U, "Please specify a depth for the texture.");

			Assure.False(
				mipAllocation && !MathUtils.IsPowerOfTwo(width),
				"Can not create mipmapped texture with any non-power-of-two (NPOT) dimension. " +
					"Dimensions: " + width + "x" + height + "x" + depth + "."
			);
			Assure.False(
				mipAllocation && !MathUtils.IsPowerOfTwo(height),
				"Can not create mipmapped texture with any non-power-of-two (NPOT) dimension. " +
					"Dimensions: " + width + "x" + height + "x" + depth + "."
			);
			Assure.False(
				mipAllocation && !MathUtils.IsPowerOfTwo(depth),
				"Can not create mipmapped texture with any non-power-of-two (NPOT) dimension. " +
					"Dimensions: " + width + "x" + height + "x" + depth + "."
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
				mipGenerationTarget && InitialData != null,
				"Can not supply initial data to a mip generation target."
			);
			Assure.True(
				InitialData == null
				|| (InitialData.Value.Length == TextureUtils.GetSizeTexels(mipAllocation, width, height, depth)),
				"Initial data is of incorrect length (" + (InitialData != null ? InitialData.Value.Length : 0) + ") for this resource. " +
					"It should have a length of: " + TextureUtils.GetSizeTexels(mipAllocation, width, height, depth) + "."
			);
			Assure.False(dynamicDetail && Usage.GetUsage() == 0x3, "Can not create a dynamic-detail staging resource.");
			Assure.False(
				(permittedBindings & GPUBindings.DepthStencilTarget) == GPUBindings.DepthStencilTarget
				&& texelFormat != TexelFormat.DSV_FORMAT_CODE,
				"Can not create a depth-stencil target with any texel format other than " + typeof(TexelFormat.DepthStencil).Name + "."
			);

			Texture3DResourceHandle outResourceHandle;
			GCHandle? pinnedInitData = null;
			GCHandle? pinnedDataHandle = null;
			IntPtr initialDataPtr = IntPtr.Zero;
			InitialResourceDataDesc[] dataArr = null;

			if (InitialData != null) {
				pinnedInitData = GCHandle.Alloc(InitialData.Value.ContainingArray, GCHandleType.Pinned);
				
				dataArr = InitialResourceDataDesc.CreateDataArr(
					pinnedInitData.Value.AddrOfPinnedObject() + (int) (InitialData.Value.Offset * texelSizeBytes),
					1U,
					numMips,
					width,
					height,
					depth,
					texelSizeBytes
				);

				pinnedDataHandle = GCHandle.Alloc(dataArr, GCHandleType.Pinned);
				initialDataPtr = pinnedDataHandle.Value.AddrOfPinnedObject();
			}

			try {
				InteropUtils.CallNative(NativeMethods.ResourceFactory_CreateTexture3D,
					RenderingModule.Device,
					width,
					height,
					depth,
					(InteropBool) mipAllocation,
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

			return new Texture3D<TTexel>(
				outResourceHandle,
				Usage,
				TextureUtils.GetSize(texelSizeBytes, mipAllocation, width, height, depth),
				permittedBindings,
				mipGenerationTarget,
				dynamicDetail,
				width,
				height,
				depth,
				texelSizeBytes,
				mipAllocation,
				numMips,
				0U
			);
		}
	}
}