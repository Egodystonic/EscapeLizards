// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 10 03 2015 at 14:40 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// An enumeration of topology types. These types describe how the data provided to the GPU via a vertex buffer should be interpreted in the
	/// input assembler.
	/// </summary>
	public enum PrimitiveTopology {
		/// <summary>
		/// The topology is undefined.
		/// </summary>
		Undefined = 0,
		/// <summary>
		/// The data in the vertex buffer(s) is a list of points.
		/// </summary>
		PointList = 1,
		/// <summary>
		/// The data in the vertex buffer(s) is a list of point-pairs that make lines.
		/// </summary>
		LineList = 2,
		/// <summary>
		/// The data in the vertex buffer(s) is a list of points that describe connected lines.
		/// </summary>
		LineStrip = 3,
		/// <summary>
		/// The data in the vertex buffer(s) is a list of three-point-groups that describe triangle polygons.
		/// </summary>
		TriangleList = 4,
		/// <summary>
		/// The data in the vertex buffer(s) is a list of points that describe connected triangles.
		/// </summary>
		TriangleStrip = 5
	}
}