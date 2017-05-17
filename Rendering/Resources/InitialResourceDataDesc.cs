// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 05 11 2014 at 15:34 by Ben Bowen

using System;
using System.Collections.Generic;

namespace Ophidian.Losgap.Rendering {
	internal struct InitialResourceDataDesc {
		public readonly IntPtr Data;
		public readonly uint DataRowStrideBytes;
		public readonly uint DataSliceStrideBytes;

		public InitialResourceDataDesc(IntPtr data, uint dataRowStrideBytes, uint dataSliceStrideBytes) {
			Data = data;
			DataRowStrideBytes = dataRowStrideBytes;
			DataSliceStrideBytes = dataSliceStrideBytes;
		}

		public static InitialResourceDataDesc[] CreateDataArr(IntPtr dataStart, uint numTextures, uint numMipsPerTex,
			uint texWidth, uint texHeight, uint texDepth, uint texelSizeBytes) {
			Assure.True(MathUtils.IsPowerOfTwo(texWidth) || numMipsPerTex == 1);
			Assure.True(MathUtils.IsPowerOfTwo(texHeight) || numMipsPerTex == 1);
			Assure.True(MathUtils.IsPowerOfTwo(texDepth) || numMipsPerTex == 1);

			InitialResourceDataDesc[] result = new InitialResourceDataDesc[numTextures * numMipsPerTex];

			IntPtr curData = dataStart;
			for (uint texIndex = 0; texIndex < numTextures; ++texIndex) {
				uint mip0Index = texIndex * numMipsPerTex;
				for (int mipIndex = 0; mipIndex < numMipsPerTex; ++mipIndex) {
					uint mipWidth = Math.Max(texWidth >> mipIndex, 1U);
					uint mipHeight = Math.Max(texHeight >> mipIndex, 1U);
					uint mipDepth = Math.Max(texDepth >> mipIndex, 1U);
					result[mip0Index + mipIndex] = new InitialResourceDataDesc(
						curData,
						mipWidth * texelSizeBytes,
						mipWidth * mipHeight * texelSizeBytes
					);

					curData += (int) (mipWidth * mipHeight * mipDepth * texelSizeBytes);
				}
			}

			return result;
		}
	}
}