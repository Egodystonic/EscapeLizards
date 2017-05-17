// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 22 01 2015 at 17:15 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Rendering {
	internal sealed class ImmediateRCQ : RenderCommandQueue {
		public override unsafe void Flush() {
			uint offset = 0U;
			bool success;

			for (int i = 0; i < DeferredActions.Count; i++) {
				KeyValuePair<uint, Action> curAction = DeferredActions[i];

				char* failReason = stackalloc char[InteropUtils.MAX_INTEROP_FAIL_REASON_STRING_LENGTH + 1];
				success = NativeMethods.RenderPassManager_FlushInstructions(
					(IntPtr) failReason,
					RenderingModule.DeviceContext,
					RenderCommandList.AlignedPointer + (int) offset * sizeof(RenderCommand),
					curAction.Key - offset
				);
				if (!success) throw new NativeOperationFailedException(Marshal.PtrToStringUni((IntPtr) failReason));

				offset = curAction.Key;
				curAction.Value();
			}

			char* failReason2 = stackalloc char[InteropUtils.MAX_INTEROP_FAIL_REASON_STRING_LENGTH + 1];
			success = NativeMethods.RenderPassManager_FlushInstructions(
				(IntPtr) failReason2,
				RenderingModule.DeviceContext,
				RenderCommandList.AlignedPointer + (int) offset * sizeof(RenderCommand),
				CurListIndex - offset
			);
			if (!success) throw new NativeOperationFailedException(Marshal.PtrToStringUni((IntPtr) failReason2));

			CurListIndex = 0U;
			DeferredActions.Clear();

			RenderCommandTempMemPool.GetLocalPool().FreeAll();
		}
	}
}