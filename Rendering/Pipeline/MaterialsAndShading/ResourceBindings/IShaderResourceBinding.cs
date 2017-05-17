// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 03 02 2015 at 14:09 by Ben Bowen

using System;
using System.Collections.Generic;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// Represents a named (identified) slot that can be used to bind resources (e.g. textures, samplers, buffers, state objects etc.)
	/// to a <see cref="Shader"/>.
	/// </summary>
	public interface IShaderResourceBinding {
		/// <summary>
		/// Resource binding identifier. Unique within the owning shader.
		/// </summary>
		string Identifier { get; }
		/// <summary>
		/// The register input slot that is used in the shader program to access the resources set via this binding.
		/// </summary>
		uint SlotIndex { get; }

		/// <summary>
		/// Bind the given <paramref name="resource"/> to the owning shader at the given <see cref="SlotIndex"/>/<see cref="Identifier"/>.
		/// </summary>
		/// <param name="resource">The resource to bind. May be null (to 'unbind' the resource slot). Must be the right type of 
		/// resource for the binding (otherwise an exception will be thrown).</param>
		void Bind(object resource);

		/// <summary>
		/// Get the resource currently bound to the owning shader through this binding.
		/// </summary>
		/// <returns>The currently bound resource. May be null if no resource is bound.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
			Justification = "Will remain a method to keep in line with more reified overloads on implementing types, and also " +
				"with the Bind() method.")]
		object GetBoundResource();
	}
}