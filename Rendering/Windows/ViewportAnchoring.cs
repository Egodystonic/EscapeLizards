// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 17 12 2014 at 18:07 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// An enumeration of potential anchor locations at which to 'dock' a <see cref="SceneViewport"/> to its <see cref="SceneViewport.TargetWindow"/>.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1027:MarkEnumsWithFlags",
		Justification = "Not a flag enum, just using bitfields for quicker branching")]
	public enum ViewportAnchoring {
		/* 
		 * Bitfield scheme: = 0xVH
		 * V = Vertical: Top/Bottom/Centered = 0/1/2
		 * H = Horizontal: Left/Right/Centered = 0/1/2
		*/
		/// <summary>
		/// Anchors the viewport to the top-left corner.
		/// </summary>
		TopLeft = 0x00,
		/// <summary>
		/// Anchors the viewport to the top-right corner.
		/// </summary>
		TopRight = 0x01,
		/// <summary>
		/// Anchors the viewport to the top side of the window, centered horizontally.
		/// </summary>
		TopCentered = 0x02,
		/// <summary>
		/// Anchors the viewport to the bottom-left corner.
		/// </summary>
		BottomLeft = 0x10,
		/// <summary>
		/// Anchors the viewport to the bottom-right corner.
		/// </summary>
		BottomRight = 0x11,
		/// <summary>
		/// Anchors the viewport to the bottom side of the window, centered horizontally.
		/// </summary>
		BottomCentered = 0x12,
		/// <summary>
		/// Anchors the viewport to the left side of the window, centered vertically.
		/// </summary>
		CenteredLeft = 0x20,
		/// <summary>
		/// Anchors the viewport to the right side of the window, centered vertically.
		/// </summary>
		CenteredRight = 0x21,
		/// <summary>
		/// Anchors the viewport to the centre of the window, both horizontally and vertically.
		/// </summary>
		Centered = 0x22
	}
}