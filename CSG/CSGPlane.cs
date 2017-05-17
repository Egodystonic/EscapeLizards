// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 13 03 2015 at 14:29 by Ben Bowen

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ophidian.Losgap.CSG {
	public struct CSGPlane {
		public readonly Plane Plane;
		public readonly Vector3 Center;
		private readonly CSGVertex[] vertices;

		public IReadOnlyList<CSGVertex> Vertices {
			get {
				return new ReadOnlyCollection<CSGVertex>(vertices);
			}
		}

		public uint NumVertices {
			get {
				return (uint) Vertices.Count;
			}
		}

		public CSGPlane(Plane plane, CSGVertex[] vertices) {
			Plane = plane;
			this.vertices = vertices;
			Vector3 sum = Vector3.ZERO;
			vertices.ForEach(vertex => sum += vertex.Position);
			Center = sum / vertices.Length;
		}

		public CSGPlane(Plane plane, CSGVertex[] vertices, Vector3 center) {
			Plane = plane;
			this.vertices = vertices;
			Center = center;
		}

		public CSGPlane Transform(Transform transform, Vector3 shapeOrigin) {
			CSGVertex[] transformedVerts = new CSGVertex[vertices.Length];
			for (int i = 0; i < vertices.Length; ++i) {
				CSGVertex originalVert = vertices[i];
				Vector3 scaledPosition = (originalVert.Position - shapeOrigin).Scale(transform.Scale) + shapeOrigin;
				Vector3 scaledPosFromOrigin = scaledPosition - shapeOrigin;
				Vector3 scaledAndRotatedPosition = shapeOrigin + scaledPosFromOrigin * transform.Rotation;
				transformedVerts[i] = new CSGVertex(
					scaledAndRotatedPosition + transform.Translation,
					originalVert.Normal * transform.Rotation,
					originalVert.TexUV
				);
			}

			Plane newPlane = new Plane(transform.Rotation * Plane.Normal, vertices[0].Position);

			return new CSGPlane(newPlane, transformedVerts);
		}

		public uint IndexOf(CSGVertex vertex) {
			return (uint) vertices.IndexOf(vertex);
		}

		public CSGVertex this[uint index] {
			get {
				return vertices[(int) index];
			}
		}
	}
}