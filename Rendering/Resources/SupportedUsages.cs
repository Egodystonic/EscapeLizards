// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 29 10 2014 at 14:41 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// Metadata attribute applied to <see cref="IResource"/> implementers. Used to determine if any given <see cref="ResourceUsage"/>
	/// is permitted for the target resource type.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public sealed class SupportedUsagesAttribute : Attribute {
		/// <summary>
		/// An array of permitted <see cref="ResourceUsage"/>s.
		/// </summary>
		public readonly ResourceUsage[] PermittedUsages;

		/// <summary>
		/// Constructs a new supported usages attribute.
		/// </summary>
		/// <param name="permittedUsages">An array of permitted <see cref="ResourceUsage"/>s. Must not be null.</param>
		public SupportedUsagesAttribute(params ResourceUsage[] permittedUsages) {
			Assure.NotNull(permittedUsages);
			PermittedUsages = permittedUsages;
		}
	}
}