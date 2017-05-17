// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 15 12 2014 at 16:43 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// An enumeration of possible 'fullscreen' states that any given <see cref="Window"/> may be in.
	/// These options control how the window takes up the available monitor resolution.
	/// </summary>
	public enum WindowFullscreenState : byte {
		/// <summary>
		/// Indicates that the window is not fullscreen, and has a resizable, draggable border.
		/// </summary>
		NotFullscreen = 0,
		/// <summary>
		/// Indicates that the window is in fullscreen mode, and consumes its entire monitor.
		/// This state is the most performant fullscreen version, but makes task-switching and multi-monitor setups harder to use.
		/// </summary>
		StandardFullscreen = 1,
		/// <summary>
		/// Indicates that the window has its resize/move/title border removed, and has been sized programmatically to fit the entire
		/// contents of its parent window.
		/// This state is less performant than the <see cref="StandardFullscreen"/> mode, but accommodates multi-tasking and multi-monitor
		/// use a lot better.
		/// </summary>
		BorderlessFullscreen = 2
	}
}