// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 13 03 2015 at 15:18 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;

namespace Ophidian.Losgap.CSG {
	public static partial class MeshGenerator {
		public static void GenerateCone(Cone shapeDesc, Transform transform, Vector2 texScale, uint extrapolation, bool insideOut, out IList<CSGVertex> vertices, out IList<uint> indices) {
			if (extrapolation < 3U) throw new ArgumentOutOfRangeException("extrapolation", extrapolation, "Value must be at least 3.");
			if (shapeDesc.TopRadius < MathUtils.FlopsErrorMargin && shapeDesc.BottomRadius < MathUtils.FlopsErrorMargin) {
				throw new ArgumentException("Cone is too thin.", "shapeDesc");
			}

			uint numCircularFaces = (shapeDesc.TopRadius > 0f ? 1U : 0U) + (shapeDesc.BottomRadius > 0f ? 1U : 0U);
			CSGPlane[] planes = new CSGPlane[numCircularFaces + extrapolation];

			Vector3[] topPoints = new Vector3[extrapolation];
			Vector3[] bottomPoints = new Vector3[extrapolation];
			float radiusCoveredPerFace = MathUtils.TWO_PI / extrapolation;
			float radialOffset = 0f;
			for (uint i = 0U; i < extrapolation; ++i, radialOffset += radiusCoveredPerFace) {
				Vector3 rotVec = (Vector3.FORWARD * Quaternion.FromAxialRotation(Vector3.DOWN, radialOffset)).ToUnit();
				topPoints[i] = shapeDesc.TopCenter + shapeDesc.TopRadius * rotVec;
				bottomPoints[i] = shapeDesc.BottomCenter + shapeDesc.BottomRadius * rotVec;
			}
			Vector2 invTexScale = new Vector2(1f / texScale.X, 1f / texScale.Y);

			#region Side planes
			float topCirc = new Circle(Vector2.ZERO, shapeDesc.TopRadius).Circumference;
			float bottomCirc = new Circle(Vector2.ZERO, shapeDesc.BottomRadius).Circumference;

			for (uint side = 0; side < extrapolation; ++side) {
				uint advSide = (side + 1) % extrapolation;
				Vector3 normal = bottomPoints[side] + (bottomPoints[advSide] - bottomPoints[side]) * 0.5f - shapeDesc.BottomCenter;
				if (normal == Vector3.ZERO) normal = topPoints[side] + (topPoints[advSide] - topPoints[side]) * 0.5f - shapeDesc.TopCenter;
				normal = normal != Vector3.ZERO ? normal.ToUnit() : Vector3.UP;

				List<CSGVertex> sideVertices = new List<CSGVertex>();

				Vector2 sideTexPoint =
					Vector3.Distance(shapeDesc.TopCenter, topPoints[side]) > Vector3.Distance(shapeDesc.BottomCenter, bottomPoints[side])
					? new Vector2(topPoints[side].X, topPoints[side].Z) : new Vector2(bottomPoints[side].X, bottomPoints[side].Z);
				float sideTexU = MathUtils.NormalizeRadians(
						Vector2.AngleBetween(Vector2.UP, sideTexPoint),
						MathUtils.RadianNormalizationRange.PositiveFullRange
					) / MathUtils.TWO_PI;
				Vector2 advSideTexPoint =
					Vector3.Distance(shapeDesc.TopCenter, topPoints[advSide]) > Vector3.Distance(shapeDesc.BottomCenter, bottomPoints[advSide])
					? new Vector2(topPoints[advSide].X, topPoints[advSide].Z) : new Vector2(bottomPoints[advSide].X, bottomPoints[advSide].Z);
				float advSideTexU = MathUtils.NormalizeRadians(
						Vector2.AngleBetween(Vector2.UP, advSideTexPoint),
						MathUtils.RadianNormalizationRange.PositiveFullRange
					) / MathUtils.TWO_PI;

				if (shapeDesc.TopRadius > 0f) {
					sideVertices.Add(
						new CSGVertex(topPoints[advSide], normal, new Vector2(advSideTexU * topCirc, 0f).Scale(invTexScale))
					);

					sideVertices.Add(
						new CSGVertex(topPoints[side], normal, new Vector2(sideTexU * topCirc, 0f).Scale(invTexScale))
					);

					sideVertices.Add(
						new CSGVertex(bottomPoints[side], normal, new Vector2(sideTexU * bottomCirc, shapeDesc.Height).Scale(invTexScale))
					);

					if (shapeDesc.BottomRadius > 0f) {
						sideVertices.Add(
							new CSGVertex(bottomPoints[advSide], normal, new Vector2(advSideTexU * bottomCirc, shapeDesc.Height).Scale(invTexScale))
						);
					}
				}
				else {
					sideVertices.Add(
						new CSGVertex(topPoints[side], normal, new Vector2(sideTexU * topCirc, 0f).Scale(invTexScale))
					);

					sideVertices.Add(
						new CSGVertex(bottomPoints[side], normal, new Vector2(sideTexU * bottomCirc, shapeDesc.Height).Scale(invTexScale))
					);

					sideVertices.Add(
						new CSGVertex(bottomPoints[advSide], normal, new Vector2(advSideTexU * bottomCirc, shapeDesc.Height).Scale(invTexScale))
					);
				}
				
				planes[side] = new CSGPlane(new Plane(normal, topPoints[side]), sideVertices.ToArray());
			}
			#endregion

			#region Top and bottom planes
			if (shapeDesc.TopRadius > 0f) {
				planes[extrapolation] = new CSGPlane(new Plane(Vector3.UP, shapeDesc.TopCenter), topPoints.Select(point => {
					Vector2 uv = new Vector2(
						point.X - shapeDesc.TopCenter.X,
						point.Z - shapeDesc.TopCenter.Z
					).Scale(invTexScale);
					return new CSGVertex(point, Vector3.UP, uv);
				}).ToArray());
			}
			if (shapeDesc.BottomRadius > 0f) {
				planes[extrapolation + numCircularFaces - 1U] = new CSGPlane(new Plane(Vector3.DOWN, shapeDesc.BottomCenter), bottomPoints.Reverse().Select(point => {
					Vector2 uv = new Vector2(
						point.X - shapeDesc.BottomCenter.X,
						point.Z - shapeDesc.BottomCenter.Z
					).Scale(invTexScale);
					return new CSGVertex(point, Vector3.DOWN, uv);
				}).ToArray());
			}
			#endregion
			

			for (int i = 0; i < planes.Length; ++i) {
				// Translate by half the height to make the centre/origin the centre of the shape
				planes[i] = planes[i].Transform(transform.TranslateBy(Vector3.UP * (shapeDesc.Height * 0.5f)), new Vector3(shapeDesc.BottomCenter, y: shapeDesc.BottomCenter.Y + shapeDesc.Height / 2f));
			}

			CreateVBAndIB(planes, out vertices, out indices, insideOut);
		}
	}
}