// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 08 01 2015 at 15:00 by Ben Bowen

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Rendering {
	internal abstract class RenderCommandQueue { // Assumed to be thread-local
		protected const uint INITIAL_LIST_SIZE = 32U;
		protected const uint LIST_SIZE_INCREMENT = 50U;
		protected const long LIST_ALIGNMENT = 7L; // TODO see if misalignment is beneficial
		protected readonly List<KeyValuePair<uint, Action>> DeferredActions = new List<KeyValuePair<uint, Action>>();
		protected AlignedAllocation<RenderCommand> RenderCommandList = AlignedAllocation<RenderCommand>.AllocArray(LIST_ALIGNMENT, INITIAL_LIST_SIZE);
		protected uint CurListLen = INITIAL_LIST_SIZE;
		protected uint CurListIndex = 0U;

		public uint NumCommandsQueued {
			get {
				return CurListIndex;
			}
		}

		public unsafe string CommandTypeBreakdown {
			get {
				return Enumerable.Range(0, (int) CurListIndex)
					.Select(i => ((RenderCommand*) RenderCommandList.AlignedPointer)[i])
					.Select(command => command.Instruction.ToString())
					.ToStringOfContents();
			}
		}

		protected unsafe RenderCommandQueue() {
			GC.AddMemoryPressure(sizeof(RenderCommand) * INITIAL_LIST_SIZE);
		}

		unsafe ~RenderCommandQueue() {
			GC.RemoveMemoryPressure(sizeof(RenderCommand) * CurListLen);
			RenderCommandList.Dispose();
		}

		public unsafe void QueueCommand(RenderCommand command) {
			if (CurListIndex == CurListLen) ResizeList();
			((RenderCommand*) RenderCommandList.AlignedPointer)[CurListIndex++] = command;
		}

		public unsafe void QueueCommand(uint reservedCommandSlot, RenderCommand command) {
			Assure.LessThan(reservedCommandSlot, CurListIndex, "Reserved command slot is outside the range of the list.");
			((RenderCommand*) RenderCommandList.AlignedPointer)[reservedCommandSlot] = command;
		}

		public void QueueAction(Action action) {
			DeferredActions.Add(new KeyValuePair<uint, Action>(CurListIndex, action));
		}

		public uint ReserveCommandSlot() {
			if (CurListIndex == CurListLen) ResizeList();
			uint result = CurListIndex;
#if DEBUG
			unsafe {
				((RenderCommand*) RenderCommandList.AlignedPointer)[result] 
					= new RenderCommand(RenderCommandInstruction.NoOperation, 12345678, 12345678, 12345678);
			}
#endif
			++CurListIndex;
			return result;
		}

		public unsafe RCQItem[] GetCurrentQueue() {
			RCQItem[] result = new RCQItem[CurListIndex + DeferredActions.Count];
			int resultIndex = 0;
			uint queueOffset = 0U;

			for (int i = 0; i < DeferredActions.Count; i++) {
				KeyValuePair<uint, Action> curAction = DeferredActions[i];
				while (queueOffset < curAction.Key) {
					result[resultIndex++] = new RCQItem(((RenderCommand*) RenderCommandList.AlignedPointer)[queueOffset++]);
				}
				result[resultIndex++] = new RCQItem(curAction.Value);
			}

			while (queueOffset < CurListIndex) {
				result[resultIndex++] = new RCQItem(((RenderCommand*) RenderCommandList.AlignedPointer)[queueOffset++]);
			}

			return result;
		}

		public abstract void Flush();

		private unsafe void ResizeList() {
			uint newListLen = CurListLen + LIST_SIZE_INCREMENT;
			GC.RemoveMemoryPressure(sizeof(RenderCommand) * CurListLen);
			AlignedAllocation<RenderCommand> newListSpace = AlignedAllocation<RenderCommand>.AllocArray(LIST_ALIGNMENT, newListLen);
			GC.AddMemoryPressure(sizeof(RenderCommand) * CurListLen);
			UnsafeUtils.MemCopy(RenderCommandList.AlignedPointer, newListSpace.AlignedPointer, (uint) sizeof(RenderCommand) * CurListLen);
			CurListLen = newListLen;

			RenderCommandList.Dispose();

			RenderCommandList = newListSpace;
		}
	}
}