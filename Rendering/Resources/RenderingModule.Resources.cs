// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 09 01 2015 at 17:21 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Rendering {
	public sealed partial class RenderingModule {
		internal static ThreadLocal<ICollection<WeakReference<IResource>>> DynamicDetailTextures
			= new ThreadLocal<ICollection<WeakReference<IResource>>>(() => new HashSet<WeakReference<IResource>>(), true);

		/// <summary>
		/// Sets the global texture detail level reduction factor. A higher value corresponds to a lower overall texture detail. Lowering
		/// the detail level can help increase performance at the cost of visual fidelity.
		/// </summary>
		/// <remarks>
		/// Setting this <paramref name="reductionLevel"/> to anything higher than
		/// <c>0U</c> will result in the corresponding number of highest-level mips being excluded from all texture calculations. All
		/// textures that were created with the <see cref="ITexture.IsGlobalDetailTarget"/>/<c>AllowDynamicDetail</c> flag will be 
		/// affected.
		/// <para>
		/// Accordingly, each increment in <paramref name="reductionLevel"/> corresponds to a 50% decrease in texture detail.
		/// </para>
		/// <para>
		/// This function call is reasonably high-impact, and should not be used as a part of a dynamic detail selection algorithm.
		/// </para>
		/// </remarks>
		/// <param name="reductionLevel">The reduction factor to apply to all dynamic textures. A higher value results in lower
		/// global detail. A value of <c>0U</c> leaves all textures at their default (highest) detail level.</param>
		public static void SetTextureDetailReduction(uint reductionLevel) {
			foreach (ICollection<WeakReference<IResource>> textureResourceCollection in DynamicDetailTextures.Values) {
				IList<WeakReference<IResource>> missingReferences = new List<WeakReference<IResource>>();
				foreach (WeakReference<IResource> texRef in textureResourceCollection) {
					IResource resource;
					bool referenceValid = texRef.TryGetTarget(out resource);

					if (!referenceValid || resource.IsDisposed) missingReferences.Add(texRef);
					else {
						Assure.True(
							(resource is ITexture && (resource as ITexture).IsGlobalDetailTarget)
							|| (resource is ITextureArray && (resource as ITextureArray).IsGlobalDetailTarget)
						);

						if (resource is ITexture1D || resource is ITexture1DArray) {
							InteropUtils.CallNative(
								NativeMethods.ResourceFactory_SetTextureQualityOffset,
								DeviceContext,
								(Texture1DResourceHandle) resource.ResourceHandle,
								reductionLevel
							);
							break;
						}
						else if (resource is ITexture2D || resource is ITexture2DArray) {
							InteropUtils.CallNative(
								NativeMethods.ResourceFactory_SetTextureQualityOffset,
								DeviceContext,
								(Texture2DResourceHandle) resource.ResourceHandle,
								reductionLevel
							);
							break;
						}
						else if (resource is ITexture3D) {
							InteropUtils.CallNative(
								NativeMethods.ResourceFactory_SetTextureQualityOffset,
								DeviceContext,
								(Texture3DResourceHandle) resource.ResourceHandle,
								reductionLevel
							);
							break;
						}
					}
				}
				textureResourceCollection.RemoveWhere(missingReferences.Contains);
			}
		}
	}
}