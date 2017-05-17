// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 13 03 2015 at 15:18 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;

namespace Ophidian.Losgap.CSG {
	public static partial class MeshGenerator {
		public static void GeneratePlane(IList<Vector3> coplanarPoints, Transform transform, Vector2 texScale, bool doubleSided, out IList<CSGVertex> vertices, out IList<uint> indices) {
			if (coplanarPoints.Count < 2) throw new ArgumentException("Must supply at least 3 points.", "coplanarPoints");
			CSGPlane[] planes = new CSGPlane[2];

			Vector3 barycentre = Vector3.ZERO;
			coplanarPoints.ForEach(point => barycentre += point);
			barycentre /= coplanarPoints.Count;
			CSGVertex[] csgVertices;
			Plane plane = Plane.FromPoints(coplanarPoints[0], coplanarPoints[1], coplanarPoints[2]);
			Vector3 uvX = (coplanarPoints[0] - barycentre).ToUnit();
			Vector3 uvY = uvX.Cross(plane.Normal);
			Vector2 invTexDimensions = new Vector2(1f / texScale.X, 1f / texScale.Y);

			#region Plane A
			csgVertices = new CSGVertex[coplanarPoints.Count];
			plane = -plane;
			for (int i = 0; i < coplanarPoints.Count; ++i) {
				Vector3 centreOffset = coplanarPoints[i] - barycentre;

				csgVertices[csgVertices.Length - (i + 1)] = new CSGVertex(
					coplanarPoints[i],
					plane.Normal,
					new Vector2(centreOffset.Dot(uvX), centreOffset.Dot(uvY)).Scale(invTexDimensions)
					);
			}

			planes[0] = new CSGPlane(plane, csgVertices, barycentre);
			#endregion

			#region Plane B
			if (doubleSided) {
				csgVertices = new CSGVertex[coplanarPoints.Count];
				for (int i = 0; i < coplanarPoints.Count; ++i) {
					Vector3 centreOffset = coplanarPoints[i] - barycentre;

					csgVertices[i] = new CSGVertex(
						coplanarPoints[i],
						plane.Normal,
						new Vector2(centreOffset.Dot(uvX), centreOffset.Dot(uvY)).Scale(invTexDimensions)
					);
				}

				planes[1] = new CSGPlane(plane, csgVertices, barycentre);
			}
			#endregion

			for (int i = 0; i < (doubleSided ? 2 : 1); ++i) {
				planes[i] = planes[i].Transform(transform, planes[i].Center);
			}

			CreateVBAndIB(planes.Take(doubleSided ? 2 : 1).ToList(), out vertices, out indices, false);
		}
	}
}