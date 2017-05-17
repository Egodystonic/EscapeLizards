// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 19 03 2015 at 20:42 by Ben Bowen

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Entities {
	public sealed class EntityModule : ILosgapModule {
		public const float RAY_TEST_INF_LENGTH_ACTUAL = 20000f;
		public const uint DEFAULT_MAX_RAY_TEST_RESULTS = 100;
		private static readonly object staticMutationLock = new object();
		private static readonly List<KVP<PhysicsBodyHandle, PhysicsBodyHandle>> collisionPairList = new List<KVP<PhysicsBodyHandle, PhysicsBodyHandle>>();
		private static readonly Dictionary<PhysicsBodyHandle, Entity> collisionReportableBodies = new Dictionary<PhysicsBodyHandle, Entity>();
		private static float elapsedTime = 0f;
		private static long tickRateHz = 60L;
		private static bool physicsEngineIsStarted = false;
		private static event Action<float> postTick;
		private static event Action singleFireAfterNextTick;
		private static readonly Dictionary<Action, float> timedActionList = new Dictionary<Action, float>();
		private static readonly Dictionary<Action, float> timedActionListMutationWorkspace = new Dictionary<Action, float>();
		private static bool pausePhysics = false;

		public static bool PausePhysics {
			get {
				lock (staticMutationLock) {
					return pausePhysics;
				}
			}
			set {
				lock (staticMutationLock) {
					pausePhysics = value;
				}
			}
		}

		public static event Action<float> PostTick {
			add {
				lock (staticMutationLock) {
					postTick += value;
				}
			}
			remove {
				lock (staticMutationLock) {
					postTick -= value;
				}
			}
		}

		public static event Action SingleFireAfterNextTick {
			add {
				lock (staticMutationLock) {
					singleFireAfterNextTick += value;
				}
			}
			remove {
				lock (staticMutationLock) {
					singleFireAfterNextTick -= value;
				}
			}
	}

		private static readonly List<Entity> entityList = new List<Entity>();
		private static readonly List<Entity> entitiesToBeAdded = new List<Entity>();
		private static readonly List<Entity> entitiesToBeRemoved = new List<Entity>();

		public static float ElapsedTime {
			get {
				return elapsedTime;
			}
		}

		public static long? TickRateHz {
			get {
				lock (staticMutationLock) {
					return tickRateHz == long.MaxValue ? (long?) null : tickRateHz;
				}
			}
			set {
				lock (staticMutationLock) {
					tickRateHz = value ?? long.MaxValue;
				}
			}
		}

		/// <summary>
		/// The desired minimum number of milliseconds between invocations of <see cref="ILosgapModule.PipelineIterate"/> on this module.
		/// </summary>
		long ILosgapModule.TickIntervalMs {
			get {
				lock (staticMutationLock) {
					return 1000L / tickRateHz;
				}
			}
		}

		public static Entity RayTestNearest(Ray ray, out Vector3 hitPoint) {
			if (ray.IsInfiniteLength) ray = ray.WithLength(RAY_TEST_INF_LENGTH_ACTUAL);
			PhysicsBodyHandle matchingHandle = PhysicsManager.RayTestNearest(ray.StartPoint, ray.EndPoint.Value, out hitPoint);
			if (matchingHandle == PhysicsBodyHandle.NULL) return null;
			lock (staticMutationLock) {
				return entityList.FirstOrDefault(e => e.PhysicsBody == matchingHandle);
			}
		}

		public static IEnumerable<RayTestCollision> RayTestAll(Ray ray, uint maxResults = DEFAULT_MAX_RAY_TEST_RESULTS) {
			if (ray.IsInfiniteLength) ray = ray.WithLength(RAY_TEST_INF_LENGTH_ACTUAL);
			return PhysicsManager.RayTestAll(ray.StartPoint, ray.EndPoint.Value, maxResults)
			.OrderBy(rtcd => Vector3.DistanceSquared((Vector3) rtcd.Position, ray.StartPoint))
			.Select(rtcd => {
				lock (staticMutationLock) {
					return new RayTestCollision(entityList.FirstOrDefault(e => e.PhysicsBody == rtcd.BodyHandle) ?? entitiesToBeAdded.FirstOrDefault(e => e.PhysicsBody == rtcd.BodyHandle), (Vector3) rtcd.Position);
				}
			});
		}

		[ThreadStatic]
		private static List<RayTestCollisionDesc> reusableRTResultsList;
		[ThreadStatic]
		private static RTCDComparer rtcdComparer;
		public static void RayTestAllLessGarbage(Ray ray, List<RayTestCollision> reusableList, uint maxResults = DEFAULT_MAX_RAY_TEST_RESULTS) {
			if (ray.IsInfiniteLength) ray = ray.WithLength(RAY_TEST_INF_LENGTH_ACTUAL);

			if (reusableRTResultsList == null) {
				reusableRTResultsList = new List<RayTestCollisionDesc>();
				rtcdComparer = new RTCDComparer();
			}

			PhysicsManager.RayTestAllLessGarbage(ray.StartPoint, ray.EndPoint.Value, maxResults, reusableRTResultsList);
			rtcdComparer.RayOrigin = ray.StartPoint;
			reusableRTResultsList.Sort(rtcdComparer);

			reusableList.Clear();
			lock (staticMutationLock) {
				for (int i = 0; i < reusableRTResultsList.Count; ++i) {
					var rtcd = reusableRTResultsList[i];
					Entity firstMatchingEntity = null;
					for (int e = 0; e < entityList.Count; ++e) {
						if (entityList[e].PhysicsBody == rtcd.BodyHandle) {
							firstMatchingEntity = entityList[e];
							break;
						}
					}
					if (firstMatchingEntity == null) {
						for (int e = 0; e < entitiesToBeAdded.Count; ++e) {
							if (entitiesToBeAdded[e].PhysicsBody == rtcd.BodyHandle) {
								firstMatchingEntity = entitiesToBeAdded[e];
								break;
							}
						}
					}
					var result = new RayTestCollision(firstMatchingEntity, (Vector3) rtcd.Position);
					reusableList.Add(result);
				}
			}
		}

		internal static void AddActiveEntity(Entity e) {
			lock (staticMutationLock) {
				entitiesToBeAdded.Add(e);
			}
		}

		internal static void RemoveActiveEntity(Entity e) {
			lock (staticMutationLock) {
				entitiesToBeRemoved.Add(e);
			}
		}

		public static void LogAllActiveEntityTypes() {
			lock (staticMutationLock) {
				Logger.Log(entityList.Select(e => e.GetType().Name).ToStringOfContents());
			}
		}

		internal static void DeleteAllEntities() {
			lock (staticMutationLock) {
				foreach (Entity entity in entityList.ToList()) {
					entity.Dispose();
				}
			}
		}

		internal static void AddCollisionCallbackReportingForEntity(Entity e) {
			lock (staticMutationLock) {
				Assure.NotEqual(e.PhysicsBody, PhysicsBodyHandle.NULL);
				collisionReportableBodies.Add(e.PhysicsBody, e);
			}
		}

		internal static void RemoveCollisionCallbackReportingForEntity(Entity e) {
			lock (staticMutationLock) {
				Assure.NotEqual(e.PhysicsBody, PhysicsBodyHandle.NULL);
				collisionReportableBodies.Remove(e.PhysicsBody);
			}
		}

		/// <summary>
		/// Called by the <see cref="LosgapSystem"/> when the module has been added (via <see cref="LosgapSystem.AddModule"/>). This
		/// method is guaranteed to only ever be called once during the lifetime of the application; and is a good place for the module
		/// to perform any initialization logic.
		/// </summary>
		void ILosgapModule.ModuleAdded() {
			lock (staticMutationLock) {
				PhysicsManager.EngineStart();
				physicsEngineIsStarted = true;
			}

			PhysicsManager.SetGravityOnAllBodies(Vector3.DOWN * 9.81f);

			LosgapSystem.SystemStarting += () => {
				lock (staticMutationLock) {
					if (!physicsEngineIsStarted) {
						PhysicsManager.EngineStart();
						physicsEngineIsStarted = true;
					}
				}
			};

			LosgapSystem.SystemExited += () => {
				DeleteAllEntities();
				if (physicsEngineIsStarted) {
					PhysicsManager.EngineStop();
					physicsEngineIsStarted = false;
				}
			};
		}

		/// <summary>
		/// Called by the <see cref="LosgapSystem"/> when it is time for this module to 'tick'. At this point, the module should execute
		/// its logic for the current frame.
		/// </summary>
		/// <param name="parallelizationProvider">An object that facilitates multithreaded execution of state. Never null.</param>
		/// <param name="deltaMs">The time, in milliseconds, that has elapsed since the last invocation of this method.</param>
		void ILosgapModule.PipelineIterate(ParallelizationProvider parallelizationProvider, long deltaMs) {
			float deltaSecs = deltaMs * 0.001f;
			bool pausePhysicsLocal;

			lock (staticMutationLock) {
				foreach (var toBeAdded in entitiesToBeAdded) entityList.Add(toBeAdded);
				foreach (var toBeRemoved in entitiesToBeRemoved) entityList.Remove(toBeRemoved);

				entitiesToBeRemoved.Clear();
				entitiesToBeAdded.Clear();
				pausePhysicsLocal = pausePhysics;
			}
			if (!pausePhysicsLocal) {
				PhysicsManager.Tick(deltaSecs);
				PhysicsManager.GetCollisionPairs(collisionPairList);

				lock (staticMutationLock) {
					foreach (KVP<PhysicsBodyHandle, PhysicsBodyHandle> pair in collisionPairList) {
						if (KeyContainedInCRB(pair.Key)) {
							Entity other = null;
							if (KeyContainedInCRB(pair.Value)) other = collisionReportableBodies[pair.Value];
							else {
								for (int i = 0; i < entityList.Count; ++i) {
									if (entityList[i].PhysicsBody == pair.Value) {
										other = entityList[i];
										break;
									}
								}
							}
							if (other != null) collisionReportableBodies[pair.Key].TouchDetected(other);
						}
						if (KeyContainedInCRB(pair.Value)) {
							Entity other = null;
							if (KeyContainedInCRB(pair.Key)) other = collisionReportableBodies[pair.Key];
							else {
								for (int i = 0; i < entityList.Count; ++i) {
									if (entityList[i].PhysicsBody == pair.Key) {
										other = entityList[i];
										break;
									}
								}
							}
							if (other != null) collisionReportableBodies[pair.Value].TouchDetected(other);
						}
					}
				}	
			}
			
			for (int i = 0; i < entityList.Count; ++i) {
				entityList[i].SynchronizedTick(deltaSecs);
			}

			OnPostTick(deltaSecs);

			elapsedTime += deltaSecs;
		}

		private bool KeyContainedInCRB(PhysicsBodyHandle pbh) { // This function exists because dict.ContainsKey() boxes struct keys
			foreach (PhysicsBodyHandle key in collisionReportableBodies.Keys) {
				if (key == pbh) return true;
			}
			return false;
		}

		private void OnPostTick(float deltaSecs) {
			lock (staticMutationLock) {
				if (postTick != null) postTick(deltaSecs);
				if (singleFireAfterNextTick != null) singleFireAfterNextTick();
				singleFireAfterNextTick = null;

				timedActionListMutationWorkspace.Clear();
				foreach (KeyValuePair<Action, float> kvp in timedActionList) {
					timedActionListMutationWorkspace.Add(kvp.Key, kvp.Value - deltaSecs);
				}

				foreach (KeyValuePair<Action, float> kvp in timedActionListMutationWorkspace) {
					if (kvp.Value <= 0f) {
						timedActionList.Remove(kvp.Key);
						kvp.Key();
					}
					else timedActionList[kvp.Key] = kvp.Value;
				}
			}
		}

		public static void AddTimedAction(Action action, float time) {
			lock (staticMutationLock) {
				timedActionList.Add(action, time);
			}
		}
	}
}