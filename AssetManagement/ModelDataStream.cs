// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 08 03 2015 at 22:22 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace Ophidian.Losgap.AssetManagement {
	public sealed class ModelDataStream : IDataStream {
		private readonly object instanceMutationLock = new object();
		private readonly ModelVertex[] Vertices;
		private readonly Vector3[] Positions;
		private readonly Vector2[] TexCoords;
		private readonly Vector3[] Normals;
		private readonly uint[] Indices;

		public uint NumVertices {
			get {
				return (uint) Vertices.Length;
			}
		}

		internal ModelDataStream(ModelVertex[] vertices, Vector3[] positions, Vector2[] texCoords, Vector3[] normals, uint[] indices) {
			Vertices = vertices;
			Positions = positions;
			TexCoords = texCoords;
			Normals = normals;
			Indices = indices;
		}

		public uint[] GetIndices() {
			return Indices;
		}
		public TVertex[] GetVerticesWithoutTangents<TVertex>(ConstructorInfo positionNormalTexCoordCtor) where TVertex : struct {
			Assure.NotNull(positionNormalTexCoordCtor);

			TVertex[] result = new TVertex[NumVertices];
			for (int i = 0; i < NumVertices; ++i) {
				result[i] = (TVertex) positionNormalTexCoordCtor.Invoke(new object[] {
					Positions[Vertices[i].PositionIndex], 
					Normals.Length > 0 ? Normals[Vertices[i].NormalIndex] : Vector3.ZERO,
					TexCoords.Length > 0 ? TexCoords[Vertices[i].TexCoordIndex] : Vector2.ZERO
				});
			}
			return result;
		}

		public TVertex[] GetVertices<TVertex>(ConstructorInfo positionNormalTangentTexCoordCtor) where TVertex : struct {
			Assure.NotNull(positionNormalTangentTexCoordCtor);
			Assure.Equal(Indices.Length % 3, 0);
			Assure.NotEqual(TexCoords.Length, 0);

			TVertex[] result = new TVertex[NumVertices];

			for (int tri = 0; tri < Indices.Length / 3; ++tri) {
				var c1 = Vertices[Indices[tri * 3 + 0]];
				var c2 = Vertices[Indices[tri * 3 + 1]];
				var c3 = Vertices[Indices[tri * 3 + 2]];
				Vector3 v1 = Positions[c1.PositionIndex];
				Vector2 w1 = TexCoords[c1.TexCoordIndex];
				Vector3 v2 = Positions[c2.PositionIndex];
				Vector2 w2 = TexCoords[c2.TexCoordIndex];
				Vector3 v3 = Positions[c3.PositionIndex];
				Vector2 w3 = TexCoords[c3.TexCoordIndex];
				float x1 = v2.X - v1.X;
				float x2 = v3.X - v1.X;
				float y1 = v2.Y - v1.Y;
				float y2 = v3.Y - v1.Y;
				float z1 = v2.Z - v1.Z;
				float z2 = v3.Z - v1.Z;

				float s1 = w2.X - w1.X;
				float s2 = w3.X - w1.X;
				float t1 = w2.Y - w1.Y;
				float t2 = w3.Y - w1.Y;

				float r = 1f / (s1 * t2 - s2 * t1);
				Vector3 tan = new Vector3(
					(t2 * x1 - t1 * x2) * r,
					(t2 * y1 - t1 * y2) * r,
					(t2 * z1 - t1 * z2) * r
				);

				result[tri * 3 + 0] = (TVertex) positionNormalTangentTexCoordCtor.Invoke(new object[] {
					v1, 
					Normals.Length > 0 ? Normals[c1.NormalIndex] : Vector3.BACKWARD,
					tan,
					w1
				});
				result[tri * 3 + 1] = (TVertex) positionNormalTangentTexCoordCtor.Invoke(new object[] {
					v2, 
					Normals.Length > 0 ? Normals[c2.NormalIndex] : Vector3.BACKWARD,
					tan,
					w2
				});
				result[tri * 3 + 2] = (TVertex) positionNormalTangentTexCoordCtor.Invoke(new object[] {
					v3, 
					Normals.Length > 0 ? Normals[c3.NormalIndex] : Vector3.BACKWARD,
					tan,
					w3
				});
			}

			return result;
		}
	}
}