// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 06 01 2015 at 17:35 by Ben Bowen

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// Represents a cache of preloaded, immutable vertex data assembled in to discrete 'models'. Each model can have multiple instances
	/// created and loaded in to the <see cref="Scene"/>.
	/// </summary>
	/// <remarks>
	/// Build a cache by creating a new <see cref="GeometryCacheBuilder{TVertex}"/>
	/// (or use <see cref="NewBuilder{TVertex}"/>) with the desired vertex type; and then create instances of the loaded models by calling
	/// <see cref="SceneLayerRenderingExtensions.CreateModelInstance">SceneLayer.CreateModelInstance()</see> with the relevant model indices. 
	/// </remarks>
	public unsafe sealed class GeometryCache : IDisposable {
		private static readonly object staticMutationLock = new object();
		private static readonly Dictionary<int, GeometryCache> activeCaches = new Dictionary<int, GeometryCache>();
		private static readonly List<GeometryCache> activeCacheList = new List<GeometryCache>();
		/// <summary>
		/// The number of discrete models that this cache contains.
		/// </summary>
		public readonly uint NumModels;
		/// <summary>
		/// The vertex type that was used in the <see cref="GeometryCacheBuilder{TVertex}"/> used to create this cache.
		/// </summary>
		public readonly Type VertexType;
		internal readonly int ID;
		private readonly object instanceMutationLock = new object();
		private readonly Dictionary<VertexShader, GeometryInputLayout> assembledInputLayouts = new Dictionary<VertexShader, GeometryInputLayout>();
		private readonly IVertexBuffer[] vertexComponentBuffers;
		private readonly string[] vertexComponentSemantics;
		private readonly ResourceFormat[] vertexComponentFormats;
		private readonly IndexBuffer indices;
		private readonly AlignedAllocation<uint> componentStartPointsAlloc;
		private readonly AlignedAllocation<uint> indexStartPointsAlloc;
		private readonly uint* componentStartPoints;
		private readonly uint* indexStartPoints;
		private readonly ModelInstanceManager instanceManager = new ModelInstanceManager();
		private readonly Dictionary<string, ModelHandle> nameToHandleMap = new Dictionary<string, ModelHandle>();
		private bool isDisposed = false;
		private readonly Func<VertexShader, GeometryInputLayout> createInputLayoutFunc;



	    internal static List<GeometryCache> ActiveCaches {
			get {
				lock (staticMutationLock) {
					return activeCacheList; 
				}
			}
		}

		/// <summary>
		/// True if this cache and its data have been disposed.
		/// </summary>
		public bool IsDisposed {
			get {
				lock (instanceMutationLock) {
					return isDisposed;
				}
			}
		}

		/// <summary>
		/// A read-only list of <see cref="VertexBuffer{TVertex}"/>s that contain the vertex data loaded in to this cache.
		/// Each buffer contains the data for a single component of the cache <see cref="VertexType"/>.
		/// </summary>
		public IReadOnlyList<IBuffer> VertexBuffers {
			get {
				return new ReadOnlyCollection<IBuffer>(vertexComponentBuffers);
			}
		}

		/// <summary>
		/// A read-only list of the <see cref="VertexShader"/> semantics associated with each <see cref="VertexBuffer{TVertex}"/> loaded
		/// by this cache.
		/// </summary>
		/// <seealso cref="VertexBuffers"/>
		public IReadOnlyList<string> VertexSemantics {
			get {
				return new ReadOnlyCollection<string>(vertexComponentSemantics);
			}
		}

		/// <summary>
		/// The buffer containing vertex index information for all models in this cache.
		/// </summary>
		public IndexBuffer IndexBuffer {
			get {
				return indices;
			}
		}

		/// <summary>
		/// The number of vertices loaded in to this cache, and used by all models together.
		/// </summary>
		public uint NumVertices {
			get {
				return vertexComponentBuffers[0].Length;
			}
		}

		internal IReadOnlyList<ResourceFormat> VertexFormats {
			get {
				return new ReadOnlyCollection<ResourceFormat>(vertexComponentFormats);
			}
		}

		internal GeometryCache(IVertexBuffer[] vertexComponentBuffers, string[] vertexComponentSemantics, ResourceFormat[] vertexComponentFormats,
			IndexBuffer indices, AlignedAllocation<uint> componentStartPointsAlloc, AlignedAllocation<uint> indexStartPointsAlloc, uint numModels,
			Type vertexType, int cacheID, Dictionary<string, ModelHandle> nameToHandleMap, bool orderFirst) {
			Assure.NotNull(vertexComponentBuffers);
			Assure.NotNull(vertexComponentSemantics);
			Assure.NotNull(vertexComponentFormats);
			Assure.NotNull(indices);
			Assure.Equal(vertexComponentBuffers.Length, vertexComponentSemantics.Length, "One or more vertex component arrays have different lengths.");
			Assure.Equal(vertexComponentFormats.Length, vertexComponentSemantics.Length, "One or more vertex component arrays have different lengths.");
			Assure.GreaterThan(vertexComponentBuffers.Length, 0, "Geometry cache with no vertex buffers is invalid.");
			Assure.NotNull(nameToHandleMap);
			this.vertexComponentBuffers = vertexComponentBuffers;
			this.vertexComponentSemantics = vertexComponentSemantics;
			this.vertexComponentFormats = vertexComponentFormats;
			this.indices = indices;
			this.componentStartPointsAlloc = componentStartPointsAlloc;
			this.indexStartPointsAlloc = indexStartPointsAlloc;
			this.componentStartPoints = (uint*) this.componentStartPointsAlloc.AlignedPointer;
			this.indexStartPoints = (uint*) this.indexStartPointsAlloc.AlignedPointer;
			this.NumModels = numModels;
			this.VertexType = vertexType;
			this.ID = cacheID;
			this.nameToHandleMap = nameToHandleMap;
			lock (staticMutationLock) {
				activeCaches.Add(ID, this);
				if (orderFirst) activeCacheList.Insert(0, this);
				else activeCacheList.Add(this);
			}
			this.createInputLayoutFunc = CreateInputLayout;
			GC.AddMemoryPressure(sizeof(uint) * 2 * (NumModels + 1));
		}

#if DEBUG
		~GeometryCache() {
			if (!IsDisposed) Logger.Warn(this + " was not disposed before finalization!");
		}
#endif

		/// <summary>
		/// Returns a new <see cref="GeometryCacheBuilder{TVertex}"/> with the given vertex type (<typeparamref name="TVertex"/>).
		/// </summary>
		/// <typeparam name="TVertex">The vertex type. Must be <see cref="Extensions.IsBlittable">blittable</see>.
		/// Every field in the given type must be annotated with a single <see cref="VertexComponentAttribute"/>.</typeparam>
		/// <returns>A new <see cref="GeometryCacheBuilder{TVertex}"/> that can be used to build a new GeometryCache.</returns>
		public static GeometryCacheBuilder<TVertex> NewBuilder<TVertex>() {
			return new GeometryCacheBuilder<TVertex>();
		}

		/// <summary>
		/// Gets the model with the given <paramref name="modelName"/>, by searching every loaded geometry cache for a matching
		/// model handle.
		/// </summary>
		/// <param name="modelName">The name of the model that has been previously loaded in to a GeometryCache.</param>
		/// <returns>The ModelHandle that related to the requested model type.</returns>
		/// <exception cref="KeyNotFoundException">Thrown if no model with the given <paramref name="modelName"/> is loaded in
		/// any of the currently loaded geometry caches.</exception>
		public static ModelHandle GloballyGetModelByName(string modelName) {
			lock (staticMutationLock) {
				foreach (GeometryCache cache in ActiveCaches) {
					if (cache.nameToHandleMap.ContainsKey(modelName)) return cache.GetModelByName(modelName);
				}
			}
			throw new KeyNotFoundException("No model with name '" + modelName + "' exists in any active geometry cache.");
		}

		internal static GeometryCache GetCacheByID(int id) {
			Assure.True(activeCaches.ContainsKey(id), "No active geometry cache with ID '" + id + "' exists!");
			return activeCaches[id];
		}

		internal static bool CheckForModelNameClashes(IEnumerable<string> newNames) {
			lock (staticMutationLock) {
				foreach (GeometryCache cache in ActiveCaches) {
					foreach (string newName in newNames) {
						if (cache.nameToHandleMap.ContainsKey(newName)) return true;
					}
				}
			}
			return false;
		}

		/// <summary>
		/// Retrieves the model that was added with the given <paramref name="modelName"/> to the <see cref="GeometryCacheBuilder{TVertex}"/> used
		/// to create this GeometryCache.
		/// </summary>
		/// <param name="modelName">The name of the model whose ModelHandle it is you wish to get.</param>
		/// <returns>The ModelHandle that relates to the requested <paramref name="modelName"/>.</returns>
		/// <exception cref="KeyNotFoundException">Thrown if no model with the given name was added to this cache.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ModelHandle GetModelByName(string modelName) {
			Assure.NotNull(modelName);
			return nameToHandleMap[modelName]; // Throws KeyNotFoundException if it doesn't exist.
		}

		/// <summary>
		/// Disposes this cache, making it no longer usable, and releasing all resources it created (including vertex/index buffers, 
		/// instance buffers, transform buffers, etc).
		/// </summary>
		public void Dispose() {
			lock (staticMutationLock) {
				activeCaches.Remove(ID);
				activeCacheList.Remove(this);
			}
			lock (instanceMutationLock) {
				if (isDisposed) return;
				GC.RemoveMemoryPressure(sizeof(uint) * 2 * (NumModels + 1));
				vertexComponentBuffers.ForEach(buffer => buffer.Dispose());
				indices.Dispose();
				// ReSharper disable ImpureMethodCallOnReadonlyValueField R# bug
				componentStartPointsAlloc.Dispose();
				indexStartPointsAlloc.Dispose();
				// ReSharper restore ImpureMethodCallOnReadonlyValueField
				instanceManager.Dispose();
				isDisposed = true;
				assembledInputLayouts.ForEach(ail => ail.Value.Dispose());
				assembledInputLayouts.Clear();
			}
		}

		/// <summary>
		/// Returns a string representing this cache. Identifies the cache by the number of models it contains and the vertex semantics.
		/// </summary>
		/// <returns>A string in the format <c>GeoCache&lt;NumModels x (SEMANTIC1 x 1, SEMANTIC2 x 1)&gt;</c>.</returns>
		public override string ToString() {
			// Null check required below incase we call this method mid-construction (happens on a bad assure, etc).
			return "GeoCache<" + NumModels + " x (" 
				+ (vertexComponentSemantics != null ? vertexComponentSemantics.ToStringOfContents() : "None") + ")>";
		}

		internal GeometryInputLayout GetInputLayout(VertexShader shader) {
			lock (instanceMutationLock) {
				return assembledInputLayouts.GetOrCreate(shader, createInputLayoutFunc);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal void GetModelBufferValues(uint modelIndex, out uint vbStartIndex, out uint ibStartIndex, out uint vbCount, out uint ibCount) {
			Assure.LessThan(modelIndex, NumModels);

			vbStartIndex = componentStartPoints[modelIndex];
			vbCount = componentStartPoints[modelIndex + 1] - vbStartIndex;

			ibStartIndex = indexStartPoints[modelIndex];
			ibCount = indexStartPoints[modelIndex + 1] - ibStartIndex;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal ModelInstanceHandle AllocInstance(uint modelIndex, uint sceneLayerIndex, uint materialIndex, Transform initialTransform) {
			return instanceManager.AllocateInstance(materialIndex, modelIndex, sceneLayerIndex, initialTransform);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal ArraySlice<KeyValuePair<Material, ModelInstanceManager.MIDArray>> GetModelInstanceData() {
			return instanceManager.GetModelInstanceData();
		}

		private GeometryInputLayout CreateInputLayout(VertexShader shader) {
			uint numInputBindings = (uint) shader.InputBindings.Length;
			List<KeyValuePair<VertexInputBinding, IVertexBuffer>> inputLayoutComponentBuffers = new List<KeyValuePair<VertexInputBinding, IVertexBuffer>>();
			// +3 because the instance matrix is split in to four row vectors
			InputElementDesc[] inputElements = new InputElementDesc[numInputBindings + 3]; 

			for (int i = 0; i < numInputBindings; ++i) {
				VertexInputBinding curVIB = shader.InputBindings[i];
				if (curVIB == shader.InstanceDataBinding) continue;
				
				string semantic = curVIB.Identifier;
				int componentBufferIndex = Array.IndexOf(vertexComponentSemantics, semantic);
				if (componentBufferIndex < 0) {
					throw new InvalidOperationException("Can not bind " + this + " to vertex shader with semantics " +
						shader.InputBindings.Select(srb => srb.Identifier).ToStringOfContents() + ", " +
						"semantic '" + semantic + "' is not provided in this cache!");
				}
			    
				IVertexBuffer bufferForCurSemantic = vertexComponentBuffers[componentBufferIndex];
				ResourceFormat formatForCurSemantic = vertexComponentFormats[componentBufferIndex];

				inputLayoutComponentBuffers.Add(new KeyValuePair<VertexInputBinding, IVertexBuffer>(curVIB, bufferForCurSemantic));
				inputElements[i] = new InputElementDesc(
					semantic,
					0U,
					formatForCurSemantic,
					curVIB.SlotIndex,
					true
				);
			}

			// Add the instance data slot
			uint semanticIndex = 0U;
			for (uint i = numInputBindings - 1U; i < numInputBindings + 3U; ++i) {
				inputElements[i] = new InputElementDesc(
					shader.InstanceDataBinding.Identifier,
					semanticIndex++,
					VertexShader.INSTANCE_DATA_FORMAT,
					shader.InstanceDataBinding.SlotIndex,
					false
				);
			}

			InputLayoutHandle outInputLayoutPtr;

			NativeCallResult createInputLayoutResult = InteropUtils.CallNative(NativeMethods.ShaderManager_CreateInputLayout,
				RenderingModule.Device,
				shader.Handle,
				inputElements,
				(uint) inputElements.Length,
				(IntPtr) (&outInputLayoutPtr)
			);

			if (!createInputLayoutResult.Success) {
				throw new InvalidOperationException("Given shader could not be bound to this cache, " +
					"this is likely because one or more semantics/bindings are incorrect. Details: " + createInputLayoutResult.FailureMessage);
			}

			Logger.Debug("Successfully created input layout for " + shader + " and " + this + ": " + inputElements.ToStringOfContents());

			return new GeometryInputLayout(outInputLayoutPtr, inputLayoutComponentBuffers.ToArray(), shader, this);
		}
	}
}