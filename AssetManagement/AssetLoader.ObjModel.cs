using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;

namespace Ophidian.Losgap.AssetManagement {
	public static partial class AssetLoader {
		private static unsafe ModelDataStream LoadObj(string objFile) {
			List<Vector3> positions = new List<Vector3>();
			List<Vector2> texCoords = new List<Vector2>();
			List<Vector3> normals = new List<Vector3>();
			List<ModelVertex> vertices = new List<ModelVertex>(); // A list of vertices in the order required to construct each face
			List<uint> indices = new List<uint>();

			Assure.NotNull(objFile);
			Assure.True(IOUtils.IsValidFilePath(objFile));

			// Read file into String Array
			string[] objFileContent = File.ReadAllLines(objFile);

			// Process File
			uint[] pIndices = new uint[10];
			uint[] nIndices = new uint[10];
			uint[] tIndices = new uint[10];
			int indexArrLen = 10;

			

			foreach (string[] lineParts in objFileContent.Select(line => line.ToUpper().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))) {
				if (lineParts.Length == 0) continue;

				switch (lineParts[0]) {
					case "VT": // Tex coord line - Reads it properly. Not the cause of the UV problem. - UV Problems FIXED. V Axis Mirrored
						texCoords.Add(new Vector2(
							float.Parse(lineParts[1], CultureInfo.InvariantCulture),
							float.Parse(lineParts[2], CultureInfo.InvariantCulture)
							));
						break;
						
					case "VN": // Normal line
						normals.Add(new Vector3(
							float.Parse(lineParts[1], CultureInfo.InvariantCulture),
							float.Parse(lineParts[2], CultureInfo.InvariantCulture),
							float.Parse(lineParts[3], CultureInfo.InvariantCulture)
						));
						break;
					case "V": // Positions line
						positions.Add(new Vector3(
							float.Parse(lineParts[1], CultureInfo.InvariantCulture),
							float.Parse(lineParts[2], CultureInfo.InvariantCulture),
							float.Parse(lineParts[3], CultureInfo.InvariantCulture)
						));
						break;
					case "F": // Face
						int numVertices = lineParts.Length - 1;
						if (indexArrLen < numVertices) {
							indexArrLen = numVertices * 2;
							pIndices = new uint[indexArrLen];
							nIndices = new uint[indexArrLen];
							tIndices = new uint[indexArrLen];
						}

						for (int vertexIndex = 0; vertexIndex < numVertices; ++vertexIndex) {
							string[] indexParts = lineParts[vertexIndex + 1].Split('/');

							pIndices[vertexIndex] = UInt32.Parse(indexParts[0], CultureInfo.InvariantCulture) - 1U;

							if (indexParts.Length >= 2 && indexParts[1] != String.Empty) {
								tIndices[vertexIndex] = UInt32.Parse(indexParts[1], CultureInfo.InvariantCulture) - 1U;
							}
							else tIndices[vertexIndex] = 0;

							if (indexParts.Length >= 3 && indexParts[2] != String.Empty) {
								nIndices[vertexIndex] = UInt32.Parse(indexParts[2], CultureInfo.InvariantCulture) - 1U;
							}
							else nIndices[vertexIndex] = 0;
						}


						
						IEnumerable<ModelVertex> triangulatedVertices = TriangulateFace(numVertices, pIndices, tIndices, nIndices);
						foreach (ModelVertex mv in triangulatedVertices) {
							vertices.Add(mv);
						}
						break;
				}

			}

			texCoords = TexCoordsMirror(texCoords);

			uint[] indexArray = new uint[vertices.Count];
			for (uint i = 0U; i < indexArray.Length; ++i) {
				indexArray[i] = i;
			}
			GC.Collect();
			return new ModelDataStream(vertices.ToArray(), positions.ToArray(), texCoords.ToArray(), normals.ToArray(), indexArray);
		}

		private static List<Vector2> TexCoordsMirror(List<Vector2> listInput) {
			return listInput.Select(input => new Vector2(input.X, 0.5f + (0.5f - input.Y))).ToList();
		}

		private static unsafe IEnumerable<ModelVertex> TriangulateFace(int numVertices, uint[] pIndices, uint[] tIndices, uint[] nIndices) {
			Assure.GreaterThan(numVertices, 2);
			ModelVertex[] result = new ModelVertex[(numVertices - 2) * 3];

			for (int i = 2, resultIndex = 0; i < numVertices; i++) {
				result[resultIndex++] = new ModelVertex(pIndices[0], tIndices[0], nIndices[0]);
				result[resultIndex++] = new ModelVertex(pIndices[i - 1], tIndices[i - 1], nIndices[i - 1]);
				result[resultIndex++] = new ModelVertex(pIndices[i], tIndices[i], nIndices[i]);
			}

			return result;
		}
	}
}
