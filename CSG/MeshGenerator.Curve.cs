// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 13 03 2015 at 15:18 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Ophidian.Losgap.CSG {
	public static partial class MeshGenerator {
		public struct CurveDesc {
			public readonly Cuboid StartingCuboid;
			public readonly float YawRotationRads;
			public readonly float PitchRotationRads;
			public readonly float RollRotationRads;
			public readonly uint NumSegments;

			public CurveDesc(Cuboid startingCuboid, 
				float yawRotationRads, float pitchRotationRads, float rollRotationRads,
				uint numSegments) {
				StartingCuboid = startingCuboid;
				YawRotationRads = yawRotationRads;
				PitchRotationRads = pitchRotationRads;
				RollRotationRads = rollRotationRads;
				NumSegments = numSegments;
			}
		}

		private struct CurveSegment {
			private Vector3[] points;

			public CurveSegment(object _) {
				points = new Vector3[8];
			}

			public CurveSegment(Vector3 frontBottomLeft, float width, float height, float depth) {
				points = new Vector3[8] {
					frontBottomLeft + Vector3.UP * height,
					frontBottomLeft + Vector3.UP * height + Vector3.RIGHT * width,
					frontBottomLeft,
					frontBottomLeft + Vector3.RIGHT * width,
					Vector3.BACKWARD * depth + frontBottomLeft + Vector3.UP * height,
					Vector3.BACKWARD * depth + frontBottomLeft + Vector3.UP * height + Vector3.RIGHT * width,
					Vector3.BACKWARD * depth + frontBottomLeft,
					Vector3.BACKWARD * depth + frontBottomLeft + Vector3.RIGHT * width,
				};
			}

			public Vector3 FrontTopLeft {
				get {
					return GetCorner(Cuboid.CuboidCorner.FrontTopLeft);
				}
				set {
					SetCorner(Cuboid.CuboidCorner.FrontTopLeft, value);
				}
			}
			public Vector3 FrontTopRight {
				get {
					return GetCorner(Cuboid.CuboidCorner.FrontTopRight);
				}
				set {
					SetCorner(Cuboid.CuboidCorner.FrontTopRight, value);
				}
			}
			public Vector3 FrontBottomLeft {
				get {
					return GetCorner(Cuboid.CuboidCorner.FrontBottomLeft);
				}
				set {
					SetCorner(Cuboid.CuboidCorner.FrontBottomLeft, value);
				}
			}
			public Vector3 FrontBottomRight {
				get {
					return GetCorner(Cuboid.CuboidCorner.FrontBottomRight);
				}
				set {
					SetCorner(Cuboid.CuboidCorner.FrontBottomRight, value);
				}
			}
			public Vector3 BackTopLeft {
				get {
					return GetCorner(Cuboid.CuboidCorner.BackTopLeft);
				}
				set {
					SetCorner(Cuboid.CuboidCorner.BackTopLeft, value);
				}
			}
			public Vector3 BackTopRight {
				get {
					return GetCorner(Cuboid.CuboidCorner.BackTopRight);
				}
				set {
					SetCorner(Cuboid.CuboidCorner.BackTopRight, value);
				}
			}
			public Vector3 BackBottomLeft {
				get {
					return GetCorner(Cuboid.CuboidCorner.BackBottomLeft);
				}
				set {
					SetCorner(Cuboid.CuboidCorner.BackBottomLeft, value);
				}
			}
			public Vector3 BackBottomRight {
				get {
					return GetCorner(Cuboid.CuboidCorner.BackBottomRight);
				}
				set {
					SetCorner(Cuboid.CuboidCorner.BackBottomRight, value);
				}
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private Vector3 Get(int index) {
				return points[index];
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private void Set(int index, Vector3 v) {
				points[index] = v;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Vector3 GetCorner(Cuboid.CuboidCorner corner) {
				return Get((int) corner);
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void SetCorner(Cuboid.CuboidCorner corner, Vector3 v) {
				Set((int) corner, v);
			}
		}

		private struct IntersectionFace {
			public readonly Vector3 TopLeft;
			public readonly Vector3 TopRight;
			public readonly Vector3 BottomLeft;
			public readonly Vector3 BottomRight;

			public IntersectionFace(Vector3 topLeft, Vector3 topRight, Vector3 bottomLeft, Vector3 bottomRight) {
				TopLeft = topLeft;
				TopRight = topRight;
				BottomLeft = bottomLeft;
				BottomRight = bottomRight;
			}

			public IntersectionFace ApplyRot(Quaternion rot) {
				return new IntersectionFace(
					TopLeft * rot,
					TopRight * rot,
					BottomLeft * rot,
					BottomRight * rot
				);
			}

			public IntersectionFace Move(Vector3 translation) {
				return new IntersectionFace(
					TopLeft + translation,
					TopRight + translation,
					BottomLeft + translation,
					BottomRight + translation
				);
			}
		}

		public static void GenerateCurve(CurveDesc shapeDesc, Transform transform, Vector2 texScale, bool insideOut, out IList<CSGVertex> vertices, out IList<uint> indices) {
			CurveSegment[] segments = GenerateSegments(shapeDesc);
			CSGPlane[] planes = GeneratePlanes(shapeDesc, transform, texScale, segments);

			CreateVBAndIB(planes, out vertices, out indices, insideOut);
		}

		public static IEnumerable<Vector3> GetTrapezoidalPrismVertices(CurveDesc shapeDesc, Transform transform) {
			CSGPlane[] planes = GeneratePlanes(shapeDesc, transform, Vector2.ONE, GenerateSegments(shapeDesc));
			Vector3[] result = new Vector3[shapeDesc.NumSegments * 8];
			for (int pI = 1, pN = 0; pI < planes.Length - 1; pI += 8, ++pN) {
				var topVertsA = planes[pI].Vertices;
				var topVertsB = planes[pI + 1].Vertices;
				result[pN * 8 + 0] = topVertsA[0].Position;
				result[pN * 8 + 1] = topVertsA[1].Position;
				result[pN * 8 + 2] = topVertsA[2].Position;
				result[pN * 8 + 3] = topVertsB[1].Position;

				var bottomVertsA = planes[pI + 2].Vertices;
				var bottomVertsB = planes[pI + 3].Vertices;
				result[pN * 8 + 4] = bottomVertsA[0].Position;
				result[pN * 8 + 5] = bottomVertsA[1].Position;
				result[pN * 8 + 6] = bottomVertsA[2].Position;
				result[pN * 8 + 7] = bottomVertsB[1].Position;
			}

			return result;
		}

		private static unsafe CSGPlane[] GeneratePlanes(CurveDesc shapeDesc, Transform transform, Vector2 texScale, CurveSegment[] segments) {
			float depthPerSegment = shapeDesc.StartingCuboid.Depth / shapeDesc.NumSegments;
			CSGPlane[] planes = new CSGPlane[shapeDesc.NumSegments * 8 + 2];

			#region UV Planes
			Vector2 invTexScale = 1f / texScale;
			float minX, minY, minZ, maxX, maxY, maxZ;
			minX = minY = minZ = Single.MaxValue;
			maxX = maxY = maxZ = Single.MinValue;
			Action<Vector3> testAction = vec => {
				if (vec.X < minX) minX = vec.X;
				if (vec.X > maxX) maxX = vec.X;
				if (vec.Y < minY) minY = vec.Y;
				if (vec.Y > maxY) maxY = vec.Y;
				if (vec.Z < minZ) minZ = vec.Z;
				if (vec.Z > maxZ) maxZ = vec.Z;
			};
			foreach (var segment in segments) {
				testAction(segment.FrontTopLeft);
				testAction(segment.BackTopLeft);
				testAction(segment.FrontBottomLeft);
				testAction(segment.BackBottomLeft);
				testAction(segment.FrontTopRight);
				testAction(segment.BackTopRight);
				testAction(segment.FrontBottomRight);
				testAction(segment.BackBottomRight);
			}
			float xMid = minX + (maxX - minX) * 0.5f;
			float yMid = minY + (maxY - minY) * 0.5f;
			float zMid = minZ + (maxZ - minZ) * 0.5f;
			#endregion

			for (int segmentIndex = 0, planeIndex = 0; segmentIndex < shapeDesc.NumSegments; ++segmentIndex) {
				CurveSegment segment = segments[segmentIndex];

				Plane facePlane;
				CSGVertex[] faceVertices;
				Vector3 faceNormal;

				#region Back
				if (segmentIndex == 0) {
					facePlane = Plane.FromPoints(segment.BackBottomLeft, segment.BackTopRight, segment.BackBottomRight);
					faceNormal = facePlane.Normal;
					faceVertices = new[] {
						new CSGVertex(
							segment.BackBottomLeft,
							faceNormal,
							new Vector2(segment.BackBottomLeft.X - xMid, segment.BackBottomLeft.Y - yMid).Scale(invTexScale)
						),
						new CSGVertex(
							segment.BackBottomRight,
							faceNormal,
							new Vector2(segment.BackBottomRight.X - xMid, segment.BackBottomRight.Y - yMid).Scale(invTexScale)
						),
						new CSGVertex(
							segment.BackTopRight,
							faceNormal,
							new Vector2(segment.BackTopRight.X - xMid, segment.BackTopRight.Y - yMid).Scale(invTexScale)
						),
						new CSGVertex(
							segment.BackTopLeft,
							faceNormal,
							new Vector2(segment.BackTopLeft.X - xMid, segment.BackTopLeft.Y - yMid).Scale(invTexScale)
						),
					};
					planes[planeIndex++] = new CSGPlane(facePlane, faceVertices);
				}
				#endregion

				#region Top
				facePlane = Plane.FromPoints(segment.BackTopLeft, segment.FrontTopLeft, segment.FrontTopRight);
				faceNormal = facePlane.Normal;
				faceVertices = new[] {
					new CSGVertex(
						segment.BackTopLeft,
						faceNormal,
						new Vector2(segment.BackTopLeft.X - xMid, segment.BackTopLeft.Z - zMid).Scale(invTexScale)
					),
					new CSGVertex(
						segment.FrontTopLeft,
						faceNormal,
						new Vector2(segment.FrontTopLeft.X - xMid, segment.FrontTopLeft.Z - zMid).Scale(invTexScale)
					),
					new CSGVertex(
						segment.FrontTopRight,
						faceNormal,
						new Vector2(segment.FrontTopRight.X - xMid, segment.FrontTopRight.Z - zMid).Scale(invTexScale)
					),
				};
				planes[planeIndex++] = new CSGPlane(facePlane, faceVertices);

				facePlane = Plane.FromPoints(segment.FrontTopRight, segment.BackTopRight, segment.BackTopLeft);
				faceNormal = facePlane.Normal;
				faceVertices = new[] {
					new CSGVertex(
						segment.FrontTopRight,
						faceNormal,
						new Vector2(segment.FrontTopRight.X - xMid, segment.FrontTopRight.Z - zMid).Scale(invTexScale)
					),
					new CSGVertex(
						segment.BackTopRight,
						faceNormal,
						new Vector2(segment.BackTopRight.X - xMid, segment.BackTopRight.Z - zMid).Scale(invTexScale)
					),
					new CSGVertex(
						segment.BackTopLeft,
						faceNormal,
						new Vector2(segment.BackTopLeft.X - xMid, segment.BackTopLeft.Z - zMid).Scale(invTexScale)
					),
				};
				planes[planeIndex++] = new CSGPlane(facePlane, faceVertices);
				#endregion

				#region Bottom
				facePlane = Plane.FromPoints(segment.FrontBottomRight, segment.FrontBottomLeft, segment.BackBottomLeft);
				faceNormal = facePlane.Normal;
				faceVertices = new[] {
					new CSGVertex(
						segment.FrontBottomRight,
						faceNormal,
						new Vector2(segment.FrontBottomRight.X - xMid, segment.FrontBottomRight.Z - zMid).Scale(invTexScale)
					),
					new CSGVertex(
						segment.FrontBottomLeft,
						faceNormal,
						new Vector2(segment.FrontBottomLeft.X - xMid, segment.FrontBottomLeft.Z - zMid).Scale(invTexScale)
					),
					new CSGVertex(
						segment.BackBottomLeft,
						faceNormal,
						new Vector2(segment.BackBottomLeft.X - xMid, segment.BackBottomLeft.Z - zMid).Scale(invTexScale)
					),
				};
				planes[planeIndex++] = new CSGPlane(facePlane, faceVertices);

				facePlane = Plane.FromPoints(segment.BackBottomLeft, segment.BackBottomRight, segment.FrontBottomRight);
				faceNormal = facePlane.Normal;
				faceVertices = new[] {
					new CSGVertex(
						segment.BackBottomLeft,
						faceNormal,
						new Vector2(segment.BackBottomLeft.X - xMid, segment.BackBottomLeft.Z - zMid).Scale(invTexScale)
					),
					new CSGVertex(
						segment.BackBottomRight,
						faceNormal,
						new Vector2(segment.BackBottomRight.X - xMid, segment.BackBottomRight.Z - zMid).Scale(invTexScale)
					),
					new CSGVertex(
						segment.FrontBottomRight,
						faceNormal,
						new Vector2(segment.FrontBottomRight.X - xMid, segment.FrontBottomRight.Z - zMid).Scale(invTexScale)
					),
				};
				planes[planeIndex++] = new CSGPlane(facePlane, faceVertices);
				#endregion

				#region Left
				facePlane = Plane.FromPoints(segment.BackBottomLeft, segment.FrontBottomLeft, segment.FrontTopLeft);
				faceNormal = facePlane.Normal;
				faceVertices = new[] {
					new CSGVertex(
						segment.BackBottomLeft,
						faceNormal,
						new Vector2(segment.BackBottomLeft.Y - yMid, segment.BackBottomLeft.Z - zMid).Scale(invTexScale)
					),
					new CSGVertex(
						segment.FrontBottomLeft,
						faceNormal,
						new Vector2(segment.FrontBottomLeft.Y - yMid, segment.FrontBottomLeft.Z - zMid).Scale(invTexScale)
					),
					new CSGVertex(
						segment.FrontTopLeft,
						faceNormal,
						new Vector2(segment.FrontTopLeft.Y - yMid, segment.FrontTopLeft.Z - zMid).Scale(invTexScale)
					),
					
				};
				planes[planeIndex++] = new CSGPlane(facePlane, faceVertices);

				facePlane = Plane.FromPoints(segment.FrontTopLeft, segment.BackTopLeft, segment.BackBottomLeft);
				faceNormal = facePlane.Normal;
				faceVertices = new[] {
					new CSGVertex(
						segment.FrontTopLeft,
						faceNormal,
						new Vector2(segment.FrontTopLeft.Y - yMid, segment.FrontTopLeft.Z - zMid).Scale(invTexScale)
					),
					new CSGVertex(
						segment.BackTopLeft,
						faceNormal,
						new Vector2(segment.BackTopLeft.Y - yMid, segment.BackTopLeft.Z - zMid).Scale(invTexScale)
					),
					new CSGVertex(
						segment.BackBottomLeft,
						faceNormal,
						new Vector2(segment.BackBottomLeft.Y - yMid, segment.BackBottomLeft.Z - zMid).Scale(invTexScale)
					),
				};
				planes[planeIndex++] = new CSGPlane(facePlane, faceVertices);
				#endregion

				#region Right
				facePlane = Plane.FromPoints(segment.FrontTopRight, segment.FrontBottomRight, segment.BackBottomRight);
				faceNormal = facePlane.Normal;
				faceVertices = new[] {
					new CSGVertex(
						segment.FrontTopRight,
						faceNormal,
						new Vector2(segment.FrontTopRight.Y - yMid, segment.FrontTopRight.Z - zMid).Scale(invTexScale)
					),
					new CSGVertex(
						segment.FrontBottomRight,
						faceNormal,
						new Vector2(segment.FrontBottomRight.Y - yMid, segment.FrontBottomRight.Z - zMid).Scale(invTexScale)
					),
					new CSGVertex(
						segment.BackBottomRight,
						faceNormal,
						new Vector2(segment.BackBottomRight.Y - yMid, segment.BackBottomRight.Z - zMid).Scale(invTexScale)
					),
				};
				planes[planeIndex++] = new CSGPlane(facePlane, faceVertices);

				facePlane = Plane.FromPoints(segment.BackBottomRight, segment.BackTopRight, segment.FrontTopRight);
				faceNormal = facePlane.Normal;
				faceVertices = new[] {
					new CSGVertex(
						segment.BackBottomRight,
						faceNormal,
						new Vector2(segment.BackBottomRight.Y - yMid, segment.BackBottomRight.Z - zMid).Scale(invTexScale)
					),
					new CSGVertex(
						segment.BackTopRight,
						faceNormal,
						new Vector2(segment.BackTopRight.Y - yMid, segment.BackTopRight.Z - zMid).Scale(invTexScale)
					),
					new CSGVertex(
						segment.FrontTopRight,
						faceNormal,
						new Vector2(segment.FrontTopRight.Y - yMid, segment.FrontTopRight.Z - zMid).Scale(invTexScale)
					),
				};
				planes[planeIndex++] = new CSGPlane(facePlane, faceVertices);
				#endregion

				#region Front
				if (segmentIndex == segments.Length - 1) {
					facePlane = Plane.FromPoints(segment.FrontBottomRight, segment.FrontTopLeft, segment.FrontBottomLeft);
					faceNormal = facePlane.Normal;
					faceVertices = new[] {
						new CSGVertex(
							segment.FrontBottomRight,
							faceNormal,
							new Vector2(segment.FrontBottomRight.X - xMid, segment.FrontBottomRight.Y - yMid).Scale(invTexScale)
						),
						new CSGVertex(
							segment.FrontBottomLeft,
							faceNormal,
							new Vector2(segment.FrontBottomLeft.X - xMid, segment.FrontBottomLeft.Y - yMid).Scale(invTexScale)
						),
						new CSGVertex(
							segment.FrontTopLeft,
							faceNormal,
							new Vector2(segment.FrontTopLeft.X - xMid, segment.FrontTopLeft.Y - yMid).Scale(invTexScale)
						),
						new CSGVertex(
							segment.FrontTopRight,
							faceNormal,
							new Vector2(segment.FrontTopRight.X - xMid, segment.FrontTopRight.Y - yMid).Scale(invTexScale)
						)
					};
					planes[planeIndex++] = new CSGPlane(facePlane, faceVertices);
				}
				#endregion
			}

			for (int i = 0; i < planes.Length; ++i) {
				planes[i] = planes[i].Transform(transform, shapeDesc.StartingCuboid.CenterPoint);
			}

			return planes;
		}

		private static CurveSegment[] GenerateSegments(CurveDesc shapeDesc) {
			CurveSegment[] segments = new CurveSegment[shapeDesc.NumSegments];
			IntersectionFace[] faces = new IntersectionFace[shapeDesc.NumSegments + 1];

			float depthPerSegment = shapeDesc.StartingCuboid.Depth / shapeDesc.NumSegments;

			Quaternion yawPerSegment = Quaternion.FromAxialRotation(Vector3.DOWN, shapeDesc.YawRotationRads / shapeDesc.NumSegments);
			Quaternion pitchPerSegment = Quaternion.FromAxialRotation(Vector3.LEFT, shapeDesc.PitchRotationRads / shapeDesc.NumSegments);
			Quaternion rollPerSegment = Quaternion.FromAxialRotation(Vector3.FORWARD, shapeDesc.RollRotationRads / shapeDesc.NumSegments);
			Quaternion yawPitchRoll = yawPerSegment * pitchPerSegment * rollPerSegment;

			faces[0] = new IntersectionFace(
				new Vector3(shapeDesc.StartingCuboid.GetCorner(Cuboid.CuboidCorner.FrontTopLeft), z: 0f),
				new Vector3(shapeDesc.StartingCuboid.GetCorner(Cuboid.CuboidCorner.FrontTopRight), z: 0f),
				new Vector3(shapeDesc.StartingCuboid.GetCorner(Cuboid.CuboidCorner.FrontBottomLeft), z: 0f),
				new Vector3(shapeDesc.StartingCuboid.GetCorner(Cuboid.CuboidCorner.FrontBottomRight), z: 0f)
			);
			for (int s = 1; s <= shapeDesc.NumSegments; ++s) {
				faces[s] = faces[s - 1].ApplyRot(yawPitchRoll);
			}

			Vector3 defaultMovement = Vector3.FORWARD * depthPerSegment;
			for (int s = 1; s <= shapeDesc.NumSegments; ++s) {
				for (int m = 0; m < s; ++m) faces[s] = faces[s].Move(defaultMovement * yawPerSegment.Subrotation(m + 1) * pitchPerSegment.Subrotation(m + 1));
			}

			for (int s = 0; s < shapeDesc.NumSegments; ++s) {
				segments[s] = new CurveSegment(null);
				IntersectionFace backFace = faces[s];
				IntersectionFace frontFace = faces[s + 1];
				segments[s].BackTopLeft = backFace.TopLeft;
				segments[s].BackTopRight = backFace.TopRight;
				segments[s].BackBottomLeft = backFace.BottomLeft;
				segments[s].BackBottomRight = backFace.BottomRight;
				segments[s].FrontTopLeft = frontFace.TopLeft;
				segments[s].FrontTopRight = frontFace.TopRight;
				segments[s].FrontBottomLeft = frontFace.BottomLeft;
				segments[s].FrontBottomRight = frontFace.BottomRight;
			}

			return segments;
		}
	}
}