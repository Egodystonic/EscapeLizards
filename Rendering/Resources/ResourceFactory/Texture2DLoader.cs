// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 12 03 2015 at 02:22 by Ben Bowen

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Rendering {
	public sealed class Texture2DLoader : IResourceBuilder {
		private readonly string filePath;
		private readonly ResourceUsage usage;
		private readonly GPUBindings permittedBindings;
		private readonly bool mipGenerationTarget;
		private readonly bool mipAllocation;
		private readonly bool dynamicDetail;

		internal Texture2DLoader() 
			: this(null, ResourceUsage.Immutable, GPUBindings.None, false, false, false) { }

		internal Texture2DLoader(string filePath, ResourceUsage usage, GPUBindings permittedBindings, bool mipGenerationTarget, bool mipAllocation, bool dynamicDetail) {
			this.filePath = filePath;
			this.usage = usage;
			this.permittedBindings = permittedBindings;
			this.mipGenerationTarget = mipGenerationTarget;
			this.mipAllocation = mipAllocation;
			this.dynamicDetail = dynamicDetail;
		}

		/// <summary>
		/// Sets the <see cref="ResourceUsage"/> to use when creating the <see cref="Texture2D{TTexel}"/>.
		/// See the <see cref="SupportedUsagesAttribute"/> on <see cref="Texture2D{TTexel}"/> for a list of permitted usages.
		/// </summary>
		/// <param name="usage">The usage to use/set.</param>
		/// <returns>An identical Texture2DBuilder with the specified <paramref name="usage"/> parameter.</returns>
		public Texture2DLoader WithUsage(ResourceUsage usage) {
			return new Texture2DLoader(filePath, usage, permittedBindings, mipGenerationTarget, mipAllocation, dynamicDetail);
		}

		public Texture2DLoader WithFilePath(string filePath) {
			return new Texture2DLoader(filePath, usage, permittedBindings, mipGenerationTarget, mipAllocation, dynamicDetail);
		}

		public Texture2DLoader WithPermittedBindings(GPUBindings permittedBindings) {
			return new Texture2DLoader(filePath, usage, permittedBindings, mipGenerationTarget, mipAllocation, dynamicDetail);
		}

		public Texture2DLoader WithMipGenerationTarget(bool mipGenerationTarget) {
			return new Texture2DLoader(filePath, usage, permittedBindings, mipGenerationTarget, mipAllocation, dynamicDetail);
		}

		public Texture2DLoader WithMipAllocation(bool mipAllocation) {
			return new Texture2DLoader(filePath, usage, permittedBindings, mipGenerationTarget, mipAllocation, dynamicDetail);
		}

		public Texture2DLoader WithDynamicDetail(bool dynamicDetail) {
			return new Texture2DLoader(filePath, usage, permittedBindings, mipGenerationTarget, mipAllocation, dynamicDetail);
		}


		/// <summary>
		/// Sets the <see cref="ResourceUsage"/> that resources created with this builder will have applied.
		/// </summary>
		/// <param name="usage">The usage type. Not all resources support all usages. All resources have a 
		/// <see cref="SupportedUsagesAttribute"/> applied that provides more information.</param>
		/// <returns>A new <see cref="IResourceBuilder"/> with the usage set to <paramref name="usage"/>.</returns>
		IResourceBuilder IResourceBuilder.WithUsage(ResourceUsage usage) {
			return WithUsage(usage);
		}

		/// <summary>
		/// Sets the initial data for the created resource(s). The data must be of a valid format for the actual type of builder
		/// that you are accessing. If you do not provide a valid object, an exception may be thrown.
		/// </summary>
		/// <param name="data">The data to set. May be null where applicable.</param>
		/// <returns>A new <see cref="IResourceBuilder"/> with the initial data set to <paramref name="data"/>.</returns>
		IResourceBuilder IResourceBuilder.WithInitialData(object data) {
			return WithFilePath(data.ToString());
		}

		/// <summary>
		/// Creates a new <see cref="IResource"/> according to the parameters set on this builder.
		/// </summary>
		/// <returns>An <see cref="IResource"/> according to the type of builder this is.</returns>
		IResource IResourceBuilder.Create() {
			return Create();
		}

		public unsafe ITexture2D Create() {
			Assure.NotNull(filePath, "You must supply a file path to load a texture.");
			string fullFilePath = Path.Combine(Path.GetFullPath(LosgapSystem.InstallationDirectory.ToString()), filePath);
			Assure.True(IOUtils.IsValidFilePath(fullFilePath), "Invalid file path: " + fullFilePath);
			Assure.True(File.Exists(fullFilePath), "File does not exist: " + fullFilePath);
			Assure.False(
				(usage == ResourceUsage.Immutable || usage == ResourceUsage.DiscardWrite) && permittedBindings == GPUBindings.None,
				"An immutable or discard-write resource with no permitted bindings is useless."
			);
			Assure.False(
				usage.GetUsage() == 0x3 && permittedBindings != GPUBindings.None,
				"Staging resources can not be bound to the pipeline."
			);
			Assure.False((usage == ResourceUsage.DiscardWrite || usage == ResourceUsage.Immutable) &&
				((permittedBindings & GPUBindings.RenderTarget) > 0
				|| (permittedBindings & GPUBindings.DepthStencilTarget) > 0
				|| (permittedBindings & GPUBindings.WritableShaderResource) > 0),
				"Can not bind an immutable or discard-write texture as a render target or depth stencil target, or as a GPU-writeable shader resource."
			);

			Texture2DResourceHandle outResHandle;
			uint outWidth;
			uint outHeight;
			ResourceFormat outFormat;

			
			InteropUtils.CallNative(NativeMethods.ResourceFactory_LoadTexture2D,
				RenderingModule.Device,
				fullFilePath,
				(InteropBool) mipAllocation,
				usage.GetUsage(),
				usage.GetCPUUsage(),
				(PipelineBindings) permittedBindings,
				(InteropBool) mipGenerationTarget,
				(InteropBool) dynamicDetail,
				(IntPtr) (&outResHandle),
				(IntPtr) (&outWidth),
				(IntPtr) (&outHeight),
				(IntPtr) (&outFormat)
			).ThrowOnFailure();

			Type texelFormat = null;
			foreach (Type texelType in TexelFormat.AllFormats.Keys.Except(typeof(TexelFormat.RenderTarget), typeof(TexelFormat.DepthStencil))) {
				if ((ResourceFormat)texelType.GetCustomAttribute<TexelFormatMetadataAttribute>().ResourceFormatIndex == outFormat) {
					texelFormat = texelType;
					break;
				}
			}
			if (texelFormat == null) {
				throw new InvalidOperationException("The format for the loaded texture (" + outFormat + ") is not supported.");
			}

			Type tex2DType = typeof(Texture2D<>).MakeGenericType(texelFormat);
			uint texelSizeBytes = texelFormat.GetCustomAttribute<TexelFormatMetadataAttribute>().FormatSizeBytes;
			try {
				return Activator.CreateInstance(
					tex2DType, BindingFlags.NonPublic | BindingFlags.Instance, null,
					new object[] {
						(ResourceHandle) outResHandle, 
						usage, 
						(ByteSize) (outWidth * outHeight * texelSizeBytes),
						permittedBindings,
						false,
						mipGenerationTarget,
						dynamicDetail,
						outWidth,
						outHeight,
						texelSizeBytes,
						mipAllocation,
						mipAllocation ? TextureUtils.GetNumMips(outWidth, outHeight) : 1U,
						0U,
						false
					},
					CultureInfo.InvariantCulture
				) as ITexture2D;
			}
			catch (Exception e) {
				throw new InvalidOperationException("Could not load texture as a " + texelFormat.Name + " due to an instantiation error.", e);
			}
		}
	}
}