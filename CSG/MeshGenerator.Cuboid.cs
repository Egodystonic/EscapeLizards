// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 13 03 2015 at 15:18 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;

namespace Ophidian.Losgap.CSG {
	public static partial class MeshGenerator {
		public static void GenerateCuboid(Cuboid shapeDesc, Transform transform, Vector2 texScale, bool insideOut, out IList<CSGVertex> vertices, out IList<uint> indices) {
			CSGPlane[] planes = new CSGPlane[6];

			#region Construct Non-Transformed Planes
			Plane facePlane;
			CSGVertex[] faceVertices;
			Vector3 faceNormal;

			// Top face
			faceNormal = Vector3.UP;
			facePlane = new Plane(faceNormal, shapeDesc.GetCorner(Cuboid.CuboidCorner.FrontTopLeft));
			faceVertices = new[] {
				new CSGVertex(
					shapeDesc.GetCorner(Cuboid.CuboidCorner.BackTopLeft),
					faceNormal,
					new Vector2(0f, 0f)
				),
				new CSGVertex(
					shapeDesc.GetCorner(Cuboid.CuboidCorner.BackTopRight),
					faceNormal,
					new Vector2(shapeDesc.Width / texScale.X, 0f)
				),
				new CSGVertex(
					shapeDesc.GetCorner(Cuboid.CuboidCorner.FrontTopRight),
					faceNormal,
					new Vector2(shapeDesc.Width / texScale.X, shapeDesc.Depth / texScale.Y)
				),
				new CSGVertex(
					shapeDesc.GetCorner(Cuboid.CuboidCorner.FrontTopLeft),
					faceNormal,
					new Vector2(0f, shapeDesc.Depth / texScale.Y)
				),
			};
			planes[0] = new CSGPlane(facePlane, faceVertices);

			// Bottom face
			faceNormal = Vector3.DOWN;
			facePlane = new Plane(faceNormal, shapeDesc.GetCorner(Cuboid.CuboidCorner.FrontBottomLeft));
			faceVertices = new[] {
				new CSGVertex(
					shapeDesc.GetCorner(Cuboid.CuboidCorner.FrontBottomRight),
					faceNormal,
					new Vector2(0f, 0f)
				),
				new CSGVertex(
					shapeDesc.GetCorner(Cuboid.CuboidCorner.BackBottomRight),
					faceNormal,
					new Vector2(0f, shapeDesc.Depth / texScale.Y)
				),
				new CSGVertex(
					shapeDesc.GetCorner(Cuboid.CuboidCorner.BackBottomLeft),
					faceNormal,
					new Vector2(shapeDesc.Width / texScale.X, shapeDesc.Depth / texScale.Y)
				),
				new CSGVertex(
					shapeDesc.GetCorner(Cuboid.CuboidCorner.FrontBottomLeft),
					faceNormal,
					new Vector2(shapeDesc.Width / texScale.X, 0f)
				),
			};
			planes[1] = new CSGPlane(facePlane, faceVertices);

			// Front face
			faceNormal = Vector3.BACKWARD;
			facePlane = new Plane(faceNormal, shapeDesc.GetCorner(Cuboid.CuboidCorner.FrontBottomLeft));
			faceVertices = new[] {
				new CSGVertex(
					shapeDesc.GetCorner(Cuboid.CuboidCorner.FrontTopLeft),
					faceNormal,
					new Vector2(0f, 0f)
				),
				new CSGVertex(
					shapeDesc.GetCorner(Cuboid.CuboidCorner.FrontTopRight),
					faceNormal,
					new Vector2(shapeDesc.Width / texScale.X, 0f)
				),
				new CSGVertex(
					shapeDesc.GetCorner(Cuboid.CuboidCorner.FrontBottomRight),
					faceNormal,
					new Vector2(shapeDesc.Width / texScale.X, shapeDesc.Height / texScale.Y)
				),
				new CSGVertex(
					shapeDesc.GetCorner(Cuboid.CuboidCorner.FrontBottomLeft),
					faceNormal,
					new Vector2(0f, shapeDesc.Height / texScale.Y)
				),
			};
			planes[2] = new CSGPlane(facePlane, faceVertices);

			// Back face
			faceNormal = Vector3.FORWARD;
			facePlane = new Plane(faceNormal, shapeDesc.GetCorner(Cuboid.CuboidCorner.BackBottomLeft));
			faceVertices = new[] {
				new CSGVertex(
					shapeDesc.GetCorner(Cuboid.CuboidCorner.BackTopRight),
					faceNormal,
					new Vector2(0f, 0f)
				),
				new CSGVertex(
					shapeDesc.GetCorner(Cuboid.CuboidCorner.BackTopLeft),
					faceNormal,
					new Vector2(shapeDesc.Width / texScale.X, 0f)
				),
				new CSGVertex(
					shapeDesc.GetCorner(Cuboid.CuboidCorner.BackBottomLeft),
					faceNormal,
					new Vector2(shapeDesc.Width / texScale.X, shapeDesc.Height / texScale.Y)
				),
				new CSGVertex(
					shapeDesc.GetCorner(Cuboid.CuboidCorner.BackBottomRight),
					faceNormal,
					new Vector2(0f, shapeDesc.Height / texScale.Y)
				),
			};
			planes[3] = new CSGPlane(facePlane, faceVertices);

			// Left face
			faceNormal = Vector3.LEFT;
			facePlane = new Plane(faceNormal, shapeDesc.GetCorner(Cuboid.CuboidCorner.BackBottomLeft));
			faceVertices = new[] {
				new CSGVertex(
					shapeDesc.GetCorner(Cuboid.CuboidCorner.BackTopLeft),
					faceNormal,
					new Vector2(0f, 0f)
				),
				new CSGVertex(
					shapeDesc.GetCorner(Cuboid.CuboidCorner.FrontTopLeft),
					faceNormal,
					new Vector2(shapeDesc.Depth / texScale.X, 0f)
				),
				new CSGVertex(
					shapeDesc.GetCorner(Cuboid.CuboidCorner.FrontBottomLeft),
					faceNormal,
					new Vector2(shapeDesc.Depth / texScale.X, shapeDesc.Height / texScale.Y)
				),
				new CSGVertex(
					shapeDesc.GetCorner(Cuboid.CuboidCorner.BackBottomLeft),
					faceNormal,
					new Vector2(0f, shapeDesc.Height / texScale.Y)
				),
			};
			planes[4] = new CSGPlane(facePlane, faceVertices);

			// Right face
			faceNormal = Vector3.RIGHT;
			facePlane = new Plane(faceNormal, shapeDesc.GetCorner(Cuboid.CuboidCorner.BackBottomRight));
			faceVertices = new[] {
				new CSGVertex(
					shapeDesc.GetCorner(Cuboid.CuboidCorner.FrontTopRight),
					faceNormal,
					new Vector2(0f, 0f)
				),
				new CSGVertex(
					shapeDesc.GetCorner(Cuboid.CuboidCorner.BackTopRight),
					faceNormal,
					new Vector2(shapeDesc.Depth / texScale.X, 0f)
				),
				new CSGVertex(
					shapeDesc.GetCorner(Cuboid.CuboidCorner.BackBottomRight),
					faceNormal,
					new Vector2(shapeDesc.Depth / texScale.X, shapeDesc.Height / texScale.Y)
				),
				new CSGVertex(
					shapeDesc.GetCorner(Cuboid.CuboidCorner.FrontBottomRight),
					faceNormal,
					new Vector2(0f, shapeDesc.Height / texScale.Y)
				),
			};
			planes[5] = new CSGPlane(facePlane, faceVertices);
			#endregion

			for (int i = 0; i < planes.Length; ++i) {
				planes[i] = planes[i].Transform(transform, shapeDesc.CenterPoint);
			}

			CreateVBAndIB(planes, out vertices, out indices, insideOut);
		}
	}
}