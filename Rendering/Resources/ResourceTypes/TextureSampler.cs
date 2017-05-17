// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 14 11 2014 at 16:53 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// Texture Samplers are used by Losgap to describe the way the shader programs should 'sample' (read) textures.
	/// </summary>
	/// <remarks>
	/// Texture Samplers are immutable.
	/// </remarks>
	public sealed class TextureSampler : BaseResource {
		private const uint SSO_SIZE_BYTES = 52U;
		/// <summary>
		/// The filter method to use.
		/// </summary>
		public readonly TextureFilterType FilterType;
		/// <summary>
		/// The wrap mode to use.
		/// </summary>
		public readonly TextureWrapMode WrapMode;
		/// <summary>
		/// If <see cref="FilterType"/> is set to <see cref="TextureFilterType.Anisotropic"/>, this field is used to determine
		/// the maximum filter level for anisotropic filtering.
		/// </summary>
		public readonly AnisotropicFilteringLevel MaxAnisotropy;
		/// <summary>
		/// If <see cref="WrapMode"/> is set to <see cref="TextureWrapMode.Border"/>, this field is used to determine the
		/// out-of-bounds colour around the border of mapped textures.
		/// </summary>
		/// <remarks>
		/// The range for each element in the <see cref="TexelFormat.RGB32Float"/> structure is <c>0f</c> to <c>1f</c>.
		/// </remarks>
		public readonly TexelFormat.RGB32Float BorderColor;

		/// <summary>
		/// Gets the set of permitted bindings to the GPU for this resource that determine where in the rendering pipeline this resource
		/// may be used.
		/// </summary>
		/// <remarks>
		/// Texture Sampler objects always return <see cref="GPUBindings.None"/>, as they can not be bound in a traditional manner.
		/// </remarks>
		public override unsafe GPUBindings PermittedBindings {
			get { return GPUBindings.None; }
		}

		/// <summary>
		/// Create a new Texture Sampler object.
		/// </summary>
		/// <param name="filterType">The filter method to use.</param>
		/// <param name="wrapMode">The wrap mode to use.</param>
		/// <param name="maxAnisotropy">If <paramref name="filterType"/> is set to <see cref="TextureFilterType.Anisotropic"/>, 
		/// this field is used to determine the maximum filter level for anisotropic filtering.</param>
		/// <param name="borderColor">If <paramref name="wrapMode"/> is set to <see cref="TextureWrapMode.Border"/>, 
		/// this field is used to determine the out-of-bounds colour around the border of mapped textures.
		/// The range for each element in the <see cref="TexelFormat.RGB32Float"/> structure is <c>0f</c> to <c>1f</c>.</param>
		public TextureSampler(TextureFilterType filterType, TextureWrapMode wrapMode, 
			AnisotropicFilteringLevel maxAnisotropy, TexelFormat.RGB32Float borderColor = new TexelFormat.RGB32Float()) 
			: base(CreateTextureSampler(filterType, wrapMode, maxAnisotropy, borderColor), ResourceUsage.Immutable, SSO_SIZE_BYTES) {
			FilterType = filterType;
			WrapMode = wrapMode;
			MaxAnisotropy = maxAnisotropy;
			BorderColor = borderColor;
			Assure.BetweenOrEqualTo(borderColor.R, 0f, 1f, "Each element in the given border colour must lie between 0f and 1f.");
			Assure.BetweenOrEqualTo(borderColor.G, 0f, 1f, "Each element in the given border colour must lie between 0f and 1f.");
			Assure.BetweenOrEqualTo(borderColor.B, 0f, 1f, "Each element in the given border colour must lie between 0f and 1f.");
		}

		/// <summary>
		/// Used by the <see cref="BaseResource.ToString"/> implementation in <see cref="BaseResource"/> to create a child-class-specific
		/// string that provides additional data about this resource.
		/// </summary>
		/// <returns>A string in the format DATUM1 + <see cref="BaseResource.DESC_VALUE_SEPARATOR"/> + DATUM2 +
		/// <see cref="BaseResource.DESC_VALUE_SEPARATOR"/> + DATUM3 etc</returns>
		protected override unsafe string CreateResourceDescString() {
			return
				"TexSampler" + DESC_TYPE_SEPARATOR
				+ FilterType + DESC_VALUE_SEPARATOR
				+ WrapMode + DESC_VALUE_SEPARATOR
				+ MaxAnisotropy + DESC_VALUE_SEPARATOR;
		}

		private static unsafe SamplerStateResourceHandle CreateTextureSampler(TextureFilterType filterType, TextureWrapMode wrapMode,
			AnisotropicFilteringLevel maxAnisotropy, TexelFormat.RGB32Float borderColor) {
			SamplerStateResourceHandle outResourceHandle;
			InteropUtils.CallNative(
				NativeMethods.ResourceFactory_CreateSSO,
				RenderingModule.Device,
				filterType,
				wrapMode,
				maxAnisotropy,
				borderColor.R,
				borderColor.G,
				borderColor.B,
				1f,
				(IntPtr) (&outResourceHandle)
			).ThrowOnFailure();

			return outResourceHandle;
		}
	}
}