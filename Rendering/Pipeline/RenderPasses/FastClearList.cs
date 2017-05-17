// All code copyright (c) 2016 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 05 10 2016 at 05:36 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Ophidian.Losgap.Rendering {
	public sealed class FastClearList<T> where T : struct {
		private T[] storage;
		private int counter;

		public int Count {
			get {
				return counter;
			}
		}

		public FastClearList() {
			storage = new T[4];
			counter = 0;
		}

		public void Add(T item) {
			if (counter >= storage.Length) {
				var newStorage = new T[storage.Length << 1];
				Array.Copy(storage, 0, newStorage, 0, storage.Length);
				storage = newStorage;
			}
			storage[counter++] = item;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear() {
			counter = 0;
		}

		public T this[int index] {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get {
				return storage[index];
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set {
				storage[index] = value;
			}
		}
	}
}