// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 03 11 2014 at 13:37 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;

namespace Ophidian.Losgap.Rendering {
	[AttributeUsage(AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
	public sealed class TexelFormatMetadataAttribute : Attribute {
		public readonly int ResourceFormatIndex;
		public readonly uint FormatSizeBytes;

		public TexelFormatMetadataAttribute(int resourceFormatIndex, uint formatSizeBytes) {
			ResourceFormatIndex = resourceFormatIndex;
			FormatSizeBytes = formatSizeBytes;
		}
	}
}