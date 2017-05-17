// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 03 02 2015 at 14:04 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// An <see cref="IShaderResourceBinding"/> that facilitates the binding of vertex data components to input members in the 
	/// vertex shader function.
	/// </summary>
	public sealed class VertexInputBinding : BaseShaderResourceBinding<IVertexBuffer> {
		/// <summary>
		/// Used by the <see cref="BaseShaderResourceBinding{T}.ToString"/> implementation in <see cref="BaseShaderResourceBinding{TResource}"/>.
		/// </summary>
		/// <returns>Returns "v".</returns>
		protected override string RegisterIdentifier {
			get {
				return "v";
			}
		}

		/// <summary>
		/// Creates a new <see cref="IVertexBuffer"/> binding.
		/// </summary>
		/// <param name="slotIndex">The vertex buffer register this binding applies to.</param>
		/// <param name="identifier">The unique identifier for this vertex buffer binding.</param>
		public VertexInputBinding(uint slotIndex, string identifier) : base(slotIndex, identifier) { }
	}
}