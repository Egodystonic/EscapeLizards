// All code copyright (c) 2016 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 17 10 2016 at 12:53 by Ben Bowen

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Ophidian.Losgap.Entities;

namespace Egodystonic.EscapeLizards {
	public static class WorldModelCache {
		private static readonly object staticMutationLock = new object();
		private static readonly Dictionary<string, PhysicsShapeHandle> cache = new Dictionary<string, PhysicsShapeHandle>();

		public static void ClearAndDisposeCache() {
			lock (staticMutationLock) {
				foreach (var handle in cache.Values) handle.Dispose();
				cache.Clear();
			}
		}

		public static PhysicsShapeHandle? GetCachedModel(string filename) {
			lock (staticMutationLock) {
				if (cache.ContainsKey(filename)) return cache[filename];
				return null;
			}
		}

		public static void SetCachedModel(string filename, PhysicsShapeHandle handle) {
			lock (staticMutationLock) {
				cache[filename] = handle;
			}
		}

		public static bool CacheContainsHandle(PhysicsShapeHandle handle) {
			lock (staticMutationLock) {
				return cache.ContainsValue(handle);
			}
		}
	}
}