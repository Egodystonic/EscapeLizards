// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 03 02 2015 at 14:04 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// An <see cref="IShaderResourceBinding"/> that facilitates the binding of <see cref="TextureSampler"/>s to slots on the owning shader.
	/// </summary>
	public sealed class TextureSamplerBinding : BaseShaderResourceBinding<TextureSampler> {
		/// <summary>
		/// Used by the <see cref="BaseShaderResourceBinding{T}.ToString"/> implementation in <see cref="BaseShaderResourceBinding{TResource}"/>.
		/// </summary>
		/// <returns>Returns "s".</returns>
		protected override string RegisterIdentifier {
			get {
				return "s";
			}
		}

		/// <summary>
		/// Creates a new <see cref="TextureSampler"/> binding.
		/// </summary>
		/// <param name="slotIndex">The texture sampler register this binding applies to.</param>
		/// <param name="identifier">The unique identifier for this texture sampler binding.</param>
		public TextureSamplerBinding(uint slotIndex, string identifier) : base(slotIndex, identifier) { }
	}
}