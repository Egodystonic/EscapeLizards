// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 29 10 2014 at 14:41 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// Metadata attribute applied to <see cref="IResource"/> implementers. Used to determine if any given <see cref="GPUBindings"/>
	/// member is permitted for the target resource type.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public sealed class SupportedGPUBindingsAttribute : Attribute {
		/// <summary>
		/// A bitwise combination of permitted <see cref="GPUBindings"/>s.
		/// </summary>
		public readonly GPUBindings PermittedBindings;

		/// <summary>
		/// Constructs a new supported GPU bindings attribute.
		/// </summary>
		/// <param name="permittedBindings">A bitwise combination of permitted <see cref="GPUBindings"/>s.</param>
		public SupportedGPUBindingsAttribute(GPUBindings permittedBindings) {
			PermittedBindings = permittedBindings;
		}
	}
}