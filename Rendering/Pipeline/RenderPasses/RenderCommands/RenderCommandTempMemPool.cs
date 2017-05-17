// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 02 02 2015 at 16:37 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using System.Resources;
using System.Runtime.InteropServices;

namespace Ophidian.Losgap.Rendering {
	internal sealed class RenderCommandTempMemPool { // Assumed to be thread-local
		[ThreadStatic]
		private static RenderCommandTempMemPool threadLocalMemPool;
		private Allocation[] allocationListSpace = new Allocation[4];
		private int numAllocations = 0;

		private struct Allocation : IEquatable<Allocation> {
			public readonly IntPtr Address;
			public readonly uint Size;
			public bool Reserved;

			public Allocation(IntPtr address, uint size) {
				Address = address;
				Size = size;
				Reserved = false;
			}

			public bool Equals(Allocation other) {
				return Address == other.Address;
			}

			public override bool Equals(object obj) {
				if (ReferenceEquals(null, obj)) {
					return false;
				}
				return obj is Allocation && Equals((Allocation)obj);
			}

			public override int GetHashCode() {
				return Address.GetHashCode();
			}

			public static bool operator ==(Allocation left, Allocation right) {
				return left.Equals(right);
			}

			public static bool operator !=(Allocation left, Allocation right) {
				return !left.Equals(right);
			}
		}

		private RenderCommandTempMemPool() { }

		unsafe ~RenderCommandTempMemPool() {
			allocationListSpace.ForEach(alloc => Marshal.FreeHGlobal(alloc.Address));
		}

		public static RenderCommandTempMemPool GetLocalPool() {
			if (threadLocalMemPool == null) threadLocalMemPool = new RenderCommandTempMemPool();
			return threadLocalMemPool;
		}

		public IntPtr Reserve(uint numBytes) {
			for (int i = 0; i < numAllocations; ++i) {
				Allocation alloc = allocationListSpace[i];
				if (!alloc.Reserved && alloc.Size >= numBytes) {
					allocationListSpace[i].Reserved = true;
					return alloc.Address;
				}
			}

			Allocation newAlloc = new Allocation(Marshal.AllocHGlobal(new IntPtr(numBytes)), numBytes);
			if (numAllocations >= allocationListSpace.Length) {
				Allocation[] oldAllocListSpace = allocationListSpace;
				allocationListSpace = new Allocation[allocationListSpace.Length * 2];
				Array.Copy(oldAllocListSpace, 0, allocationListSpace, 0, oldAllocListSpace.Length);
			}
			newAlloc.Reserved = true;
			allocationListSpace[numAllocations++] = newAlloc;
			return newAlloc.Address;
		}

		public void FreeAll() {
			for (int i = 0; i < numAllocations; ++i) {
				allocationListSpace[i].Reserved = false;
			}
		}
	}
}