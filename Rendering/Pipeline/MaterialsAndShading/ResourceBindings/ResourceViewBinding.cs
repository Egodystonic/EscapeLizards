// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 03 02 2015 at 14:04 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// An <see cref="IShaderResourceBinding"/> that facilitates the binding of <see cref="IResourceView"/>s to slots on the owning shader.
	/// </summary>
	public sealed class ResourceViewBinding : BaseShaderResourceBinding<IResourceView> {
		/// <summary>
		/// Used by the <see cref="BaseShaderResourceBinding{T}.ToString"/> implementation in <see cref="BaseShaderResourceBinding{TResource}"/>.
		/// </summary>
		/// <returns>Returns "t".</returns>
		protected override string RegisterIdentifier {
			get {
				return "t";
			}
		}

		/// <summary>
		/// Creates a new <see cref="IResourceView"/> binding.
		/// </summary>
		/// <param name="slotIndex">The resource register this binding applies to.</param>
		/// <param name="identifier">The unique identifier for this resource binding.</param>
		public ResourceViewBinding(uint slotIndex, string identifier) : base(slotIndex, identifier) { }

		/// <summary>
		/// Bind the given <paramref name="resource"/> to the owning shader at the given 
		/// <see cref="IShaderResourceBinding.SlotIndex"/>/<see cref="IShaderResourceBinding.Identifier"/>.
		/// </summary>
		/// <param name="resource">The resource to bind. May be null (to 'unbind' the resource slot).</param>
		public override void Bind(IResourceView resource) {
			Assure.True(resource == null || resource is BaseResourceView, "Resource must inherit from BaseResourceView.");
			base.Bind(resource);
		}

		internal BaseResourceView GetBoundResourceAsBaseResourceView() {
			return (BaseResourceView) GetBoundResource();
		}
	}
}