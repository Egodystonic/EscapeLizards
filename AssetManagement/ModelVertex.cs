using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ophidian.Losgap.AssetManagement {
	internal struct ModelVertex {
		public readonly uint PositionIndex;
		public readonly uint TexCoordIndex;
		public readonly uint NormalIndex;

		public ModelVertex(uint positionIndex, uint texCoordIndex, uint normalIndex) {
			PositionIndex = positionIndex;
			TexCoordIndex = texCoordIndex;
			NormalIndex = normalIndex;
		}
	}
}
