// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 13 03 2015 at 14:11 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Policy;

namespace Ophidian.Losgap.CSG {
	public static partial class MeshGenerator {
		private static void CreateVBAndIB(IList<CSGPlane> planes, out IList<CSGVertex> vertices, out IList<uint> indices, bool insideOut) {
			vertices = new List<CSGVertex>();
			indices = new List<uint>();

			foreach (CSGPlane plane in planes) {
				IList<uint> planeIndices = TriangulatePlane(plane, insideOut);
				foreach (uint index in planeIndices) {
					indices.Add(index + (uint) vertices.Count);
				}
				(vertices as List<CSGVertex>).AddRange(plane.Vertices);
			}
		}

		private static IList<uint> TriangulatePlane(CSGPlane plane, bool insideOut) {
			CSGVertexConnectionGraph connectionGraph = new CSGVertexConnectionGraph();
			
			// Add the initial connections
			Ray[] initialConnectionRays = new Ray[plane.NumVertices];
			for (uint i = 0U; i < plane.NumVertices; ++i) {
				connectionGraph.AddConnection(plane[i], plane[(i + 1) % plane.NumVertices]);
				initialConnectionRays[i] = Ray.FromStartAndEndPoint(plane[i].Position, plane[(i + 1) % plane.NumVertices].Position);
			}

			// Add the algorithmic connections
			for (uint a = 0U; a < plane.NumVertices; ++a) {
				for (uint b = 0U; b < plane.NumVertices; ++b) {
					if (a == b) continue;
					CSGVertex vertA = plane[a];
					CSGVertex vertB = plane[b];

					// Skip existing connections
					if (connectionGraph.CheckForConnection(vertA, vertB)) continue;
					
					// Is a triangle formable by drawing a connection between A and B?
					if (!connectionGraph
						.GetConnections(vertA)
						.Select(conn => conn.B)
						.Intersect(connectionGraph.GetConnections(vertB).Select(conn => conn.B))
						.Any()) continue;
					
					// Does the connection go outside the polygon? If so, it's no good.
					CSGConnection potentialConnection = new CSGConnection(vertA, vertB);
					Vector3 connectionCentre = potentialConnection.Ray.StartPoint + (potentialConnection.Ray.EndPoint.Value - potentialConnection.Ray.StartPoint) / 2f;
					if (connectionCentre != plane.Center) {
						Ray connectionToCentreRay = new Ray(connectionCentre, plane.Center - connectionCentre);
						if ((initialConnectionRays.Count(ray => {
							Vector3? potentialIntersection = ray.IntersectionWith(connectionToCentreRay);
							// We check that we're not intersecting the start point because that means this ray is going through a vertex;
							// so by rejecting intersections through the start point we're not accidentally bumping the number of edge crossings
							// by 2
							return potentialIntersection.HasValue && potentialIntersection != ray.StartPoint;
						}) & 1) == 0) continue;	
					}

					// Does the connection intersect any other connection? If it does, we would have to create more vertices, so ignore it.
					if (connectionGraph.GetAllConnections().Any(conn => {
						Vector3? intersectionPoint = conn.Ray.IntersectionWith(potentialConnection.Ray);
						if (intersectionPoint.HasValue) {
							float minDist = Math.Min(
								Vector3.DistanceSquared(intersectionPoint.Value, potentialConnection.A.Position), 
								Vector3.DistanceSquared(intersectionPoint.Value, potentialConnection.B.Position)
							);
							if (minDist > MathUtils.FlopsErrorMargin) return true;
							return conn == potentialConnection;
						}

						return false;
					})) continue;

					// Add this connection.
					connectionGraph.AddConnection(potentialConnection);
				}
			}
			
			// Compose the triangles by looking for three-cycles
			List<CSGTriangle> formulatedTriangles = new List<CSGTriangle>();

			for (uint v = 0U; v < plane.NumVertices; ++v) {
				CSGVertex sourceVertex = plane[v];
				IEnumerable<CSGConnection> primaryConnections = connectionGraph.GetConnections(sourceVertex, true);

				foreach (CSGConnection primary in primaryConnections) {
					IEnumerable<CSGConnection> secondaryConnections = connectionGraph.GetConnections(primary.B, true)
						.Where(sConn => sConn.B != sourceVertex && primaryConnections.Any(pConn => pConn.B == sConn.B));

					// Now we have a set of triangles (source -> pConn.B -> sConn.B), let's find the ones we haven't already
					// added and add those!
					foreach (CSGConnection secondary in secondaryConnections) {
						CSGTriangle potentialTriangle = new CSGTriangle(v, plane.IndexOf(secondary.A), plane.IndexOf(secondary.B));
						if (!formulatedTriangles.Contains(potentialTriangle)) formulatedTriangles.Add(potentialTriangle);
					}
				}
			}

			List<uint> result = new List<uint>();
			foreach (CSGTriangle triangle in formulatedTriangles) {
				Vector3 aPos = plane[triangle.A].Position;
				Vector3 bPos = plane[triangle.B].Position;
				Vector3 triCentre = (aPos + bPos + plane[triangle.C].Position) / 3f;
				bool triIsClockwise = plane.Plane.Normal.Dot(Vector3.Cross(aPos - triCentre, bPos - triCentre)) > 0f;
				if (insideOut) triIsClockwise = !triIsClockwise;

				result.Add(triangle.A);

				if (triIsClockwise) {
					result.Add(triangle.B);
					result.Add(triangle.C);
				}
				else {
					result.Add(triangle.C);
					result.Add(triangle.B);
				}
			}

			return result;
		}
	}
}