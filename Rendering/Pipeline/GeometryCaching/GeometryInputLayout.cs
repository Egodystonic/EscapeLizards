// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 07 01 2015 at 17:33 by Ben Bowen

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// Represents the binding of a <see cref="GeometryCache"/> to a <see cref="VertexShader"/> by defining the mapping of each
	/// <see cref="VertexInputBinding"/> in the <see cref="VertexShader"/> to an <see cref="IVertexBuffer"/> in the <see cref="GeometryCache"/>.
	/// </summary>
	public sealed class GeometryInputLayout : BaseResource {
		private const long APPROX_INPUT_ELEMENT_DESC_SIZE_BYTES = 21;
		/// <summary>
		/// The vertex shader whose <see cref="VertexShader.InputBindings"/> are connected through this input layout object.
		/// </summary>
		public readonly VertexShader AssociatedShader;
		/// <summary>
		/// The geometry cache whose <see cref="GeometryCache.VertexBuffers"/> are connected to the <see cref="AssociatedShader"/>.
		/// </summary>
		public readonly GeometryCache AssociatedCache;
		/// <summary>
		/// The array of bindings that map each <see cref="VertexInputBinding"/> in the <see cref="AssociatedShader"/> to the matching
		/// <see cref="IVertexBuffer"/> in the <see cref="AssociatedCache"/>.
		/// </summary>
		public readonly KeyValuePair<VertexInputBinding, IVertexBuffer>[] BoundComponentBuffers;
		/// <summary>
		/// A shader-resource package that can be used to bulk-update the appropriate <see cref="VertexInputBinding"/>s on
		/// the <see cref="AssociatedShader"/> with their respective <see cref="IVertexBuffer"/>s.
		/// </summary>
		public readonly ShaderResourcePackage ResourcePackage;

		/// <summary>
		/// Gets the set of permitted bindings to the GPU for this resource that determine where in the rendering pipeline this resource
		/// may be used.
		/// </summary>
		/// <remarks>
		/// Always returns <see cref="GPUBindings.None"/> for a <see cref="GeometryInputLayout"/>.
		/// </remarks>
		public override unsafe GPUBindings PermittedBindings {
			get { return GPUBindings.None; }
		}

		internal unsafe GeometryInputLayout(
			InputLayoutHandle inputLayoutHandle, 
			KeyValuePair<VertexInputBinding, IVertexBuffer>[] boundComponentBuffers, 
			VertexShader associatedShader, 
			GeometryCache associatedCache
		) : base(inputLayoutHandle, ResourceUsage.Immutable, APPROX_INPUT_ELEMENT_DESC_SIZE_BYTES * boundComponentBuffers.Length) {
			BoundComponentBuffers = boundComponentBuffers;
			AssociatedShader = associatedShader;
			AssociatedCache = associatedCache;

			ResourcePackage = new ShaderResourcePackage();
			foreach (KeyValuePair<VertexInputBinding, IVertexBuffer> pair in boundComponentBuffers) {
				ResourcePackage.SetValue(pair.Key, pair.Value);
			}
		}

		/// <summary>
		/// Used by the <see cref="BaseResource.ToString"/> implementation in <see cref="BaseResource"/> to create a child-class-specific
		/// string that provides additional data about this resource.
		/// </summary>
		/// <returns>A string in the format DATUM1 + <see cref="BaseResource.DESC_VALUE_SEPARATOR"/> + DATUM2 +
		/// <see cref="BaseResource.DESC_VALUE_SEPARATOR"/> + DATUM3 etc</returns>
		protected override unsafe string CreateResourceDescString() {
			return
				"GIL" + DESC_TYPE_SEPARATOR
				+ BoundComponentBuffers.Select(kvp => kvp.Key + "=>" + kvp.Value).ToStringOfContents() + DESC_VALUE_SEPARATOR;
		}
	}
}