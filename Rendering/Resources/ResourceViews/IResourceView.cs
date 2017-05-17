// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 12 11 2014 at 10:03 by Ben Bowen

using System;
using System.Collections.Generic;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// Represents a 'view' on to the whole or a subsection of a <see cref="IResource"/>. These views can then be used by shader programs and
	/// various miscellaneous parts of the pipeline to access resource data.
	/// </summary>
	public interface IResourceView : IDisposable {
		/// <summary>
		/// The resource that this view is connected to. 
		/// </summary>
		IResource Resource { get; }

		/// <summary>
		/// Whether or not this resource view has been disposed.
		/// </summary>
		bool IsDisposed { get; }

		/// <summary>
		/// True if this resource view, its <see cref="Resource"/>, or both have been disposed.
		/// </summary>
		bool ResourceOrViewDisposed { get; }

		ResourceViewHandle ResourceViewHandle { get; }
	}
}