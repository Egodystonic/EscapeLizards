// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 07 01 2015 at 13:19 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// Attribute that must be applied to all fields in a struct that is being used to represent a vertex.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
	public sealed class VertexComponentAttribute : Attribute {
		/// <summary>
		/// The semantic name of the field. Linked to the component semantic in the vertex shader.
		/// </summary>
		public readonly string SemanticName;
		/// <summary>
		/// The resource format identifier. Optional for most field types. Maps to DXGI_FORMAT.
		/// </summary>
		public int D3DResourceFormat = (int) ResourceFormat.Unknown;

		/// <summary>
		/// Constructs a new Vertex Component Attribute.
		/// </summary>
		/// <param name="semanticName">The semantic name of the field. Linked to the component semantic in the vertex shader.</param>
		public VertexComponentAttribute(string semanticName) {
			SemanticName = semanticName;
		}

	}
}