// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 07 01 2015 at 18:20 by Ben Bowen

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Rendering {
	[StructLayout(LayoutKind.Sequential, Pack = (int) InteropUtils.StructPacking.Safe)]
	internal struct InputElementDesc {
		[MarshalAs(InteropUtils.INTEROP_STRING_TYPE)]
		public readonly string SemanticName;
		public readonly uint SemanticIndex;
		public readonly ResourceFormat ElementFormat;
		public readonly uint InputSlot;
		public readonly InteropBool IsPerVertexData;

		public InputElementDesc(string semanticName, uint semanticIndex, ResourceFormat elementFormat, uint inputSlot, bool isPerVertexData) {
			SemanticName = semanticName;
			SemanticIndex = semanticIndex;
			ElementFormat = elementFormat;
			InputSlot = inputSlot;
			IsPerVertexData = isPerVertexData;
		}

		public override string ToString() {
			return "[InputElement '" + SemanticName + (SemanticIndex > 0U ? SemanticIndex.ToString() : String.Empty) + "': " +
				"Slot " + InputSlot + ", Format Index " + ((int) ElementFormat) + ", " + (IsPerVertexData ? "Per-vertex" : "Per-instance")
				+ "]";
		}
	}
}