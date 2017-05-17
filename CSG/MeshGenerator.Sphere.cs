// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 13 03 2015 at 15:18 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;

namespace Ophidian.Losgap.CSG {
	public static partial class MeshGenerator {
		private const float SPHERE_COORD_RADIUS_DENOMINATOR = 2f * 0.80901699f; // From cos(pi / 5), ratio of midradius to edge length

		public static void GenerateSphere(Sphere shapeDesc, Transform transform, Vector2 texScale, uint extrapolation, bool insideOut, out IList<CSGVertex> vertices, out IList<uint> indices) {
			if (shapeDesc.Radius < MathUtils.FlopsErrorMargin) {
				throw new ArgumentException("Sphere is too small.", "shapeDesc");
			}

			List<CSGPlane> planes = new List<CSGPlane>();
			Vector2 invTexScale = new Vector2(1f / texScale.X, 1f / texScale.Y);

			#region Starting Triangles
			Vector3[] startingVerts = new Vector3[12];
			startingVerts[0] = new Vector3(-1f, MathUtils.GOLDEN_RATIO, 0f);
			startingVerts[1] = new Vector3(1f, MathUtils.GOLDEN_RATIO, 0f);
			startingVerts[2] = new Vector3(-1f, -MathUtils.GOLDEN_RATIO, 0f);
			startingVerts[3] = new Vector3(1f, -MathUtils.GOLDEN_RATIO, 0f);

			for (int i = 0; i < 4; ++i) {
				startingVerts[i + 4] = startingVerts[i][2, 0, 1];
				startingVerts[i + 8] = startingVerts[i][1, 2, 0];
			}

			for (int i = 0; i < startingVerts.Length; ++i) {
				startingVerts[i] = startingVerts[i] * (shapeDesc.Radius / SPHERE_COORD_RADIUS_DENOMINATOR) + shapeDesc.Center;
			}

			Vector3[] planeSets = new Vector3[3 * 20] {
				startingVerts[0], startingVerts[5], startingVerts[11],
				startingVerts[0], startingVerts[1], startingVerts[5],
				startingVerts[0], startingVerts[7], startingVerts[1],
				startingVerts[0], startingVerts[10], startingVerts[7],
				startingVerts[0], startingVerts[11], startingVerts[10],

				startingVerts[1], startingVerts[9], startingVerts[5],
				startingVerts[5], startingVerts[4], startingVerts[11],
				startingVerts[11], startingVerts[2], startingVerts[10],
				startingVerts[10], startingVerts[6], startingVerts[7],
				startingVerts[7], startingVerts[8], startingVerts[1],

				startingVerts[3], startingVerts[4], startingVerts[9],
				startingVerts[3], startingVerts[2], startingVerts[4],
				startingVerts[3], startingVerts[6], startingVerts[2],
				startingVerts[3], startingVerts[8], startingVerts[6],
				startingVerts[3], startingVerts[9], startingVerts[8],

				startingVerts[4], startingVerts[5], startingVerts[9],
				startingVerts[2], startingVerts[11], startingVerts[4],
				startingVerts[6], startingVerts[10], startingVerts[2],
				startingVerts[8], startingVerts[7], startingVerts[6],
				startingVerts[9], startingVerts[1], startingVerts[8],
			};
			for (int i = 0; i < 20 * 3; i += 3) {
				planes.Add(MakePlane(shapeDesc, invTexScale, planeSets[i], planeSets[i + 1], planeSets[i + 2]));
			}
			#endregion

			#region Extrapolation
			for (uint i = 0; i < extrapolation; ++i) {
				List<CSGPlane> newPlanes = new List<CSGPlane>();
				foreach (CSGPlane oldPlane in planes) {
					Vector3 a = oldPlane.Vertices[0].Position;
					Vector3 b = oldPlane.Vertices[1].Position;
					Vector3 c = oldPlane.Vertices[2].Position;

					Vector3 ab = (a + b) * 0.5f;
					Vector3 bc = (b + c) * 0.5f;
					Vector3 ca = (c + a) * 0.5f;

					a = shapeDesc.PointProjection(a);
					b = shapeDesc.PointProjection(b);
					c = shapeDesc.PointProjection(c);
					ab = shapeDesc.PointProjection(ab);
					bc = shapeDesc.PointProjection(bc);
					ca = shapeDesc.PointProjection(ca);

					newPlanes.Add(MakePlane(shapeDesc, invTexScale, a, ab, ca));
					newPlanes.Add(MakePlane(shapeDesc, invTexScale, b, bc, ab));
					newPlanes.Add(MakePlane(shapeDesc, invTexScale, c, ca, bc));
					newPlanes.Add(MakePlane(shapeDesc, invTexScale, ab, bc, ca));
				}
				planes = newPlanes;
			}
			
			#endregion

			for (int i = 0; i < planes.Count; ++i) {
				planes[i] = planes[i].Transform(transform, shapeDesc.Center);
			}

			CreateVBAndIB(planes, out vertices, out indices, insideOut);
		}

		private static CSGPlane MakePlane(Sphere shapeDesc, Vector2 invTexScale, Vector3 a, Vector3 b, Vector3 c) {
			Vector3 normal = (TriCentrePoint(a, b, c) - shapeDesc.Center).ToUnit();

			return new CSGPlane(
				new Plane(normal, a),
				new[] { a, b, c }
					.Select(vert => {
						float distToUpDownAxis = Math.Min(Vector3.AngleBetween(Vector3.UP, normal), Vector3.AngleBetween(Vector3.DOWN, normal));
						float distToForwardBackAxis = Math.Min(Vector3.AngleBetween(Vector3.FORWARD, normal), Vector3.AngleBetween(Vector3.BACKWARD, normal));
						float distToLeftRightAxis = Math.Min(Vector3.AngleBetween(Vector3.LEFT, normal), Vector3.AngleBetween(Vector3.RIGHT, normal));
						Vector2 texUV;
						if (distToUpDownAxis < distToForwardBackAxis && distToUpDownAxis < distToLeftRightAxis) {
							texUV = new Vector2(
								(MathUtils.GOLDEN_RATIO + vert.X) / (2f * MathUtils.GOLDEN_RATIO),
								(MathUtils.GOLDEN_RATIO + vert.Z) / (2f * MathUtils.GOLDEN_RATIO)
							) * MathUtils.GOLDEN_RATIO * 2f;
						}
						else {
							texUV = new Vector2(
								shapeDesc.Radius * Vector2.AngleBetween(new Vector2(vert.X, vert.Z), new Vector2(0f, MathUtils.GOLDEN_RATIO)) / MathUtils.PI_OVER_TWO,
								(MathUtils.GOLDEN_RATIO + vert.Y) / MathUtils.GOLDEN_RATIO
							);
						}

						return new CSGVertex(vert, normal, texUV.Scale(invTexScale));
					})
					.ToArray()
			);
		}

		private static Vector3 TriCentrePoint(Vector3 a, Vector3 b, Vector3 c) {
			return (a + b + c) * 0.3333333f;
		}
	}
}