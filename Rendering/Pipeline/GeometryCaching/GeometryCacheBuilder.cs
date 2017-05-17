// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 07 01 2015 at 11:17 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// A class used to build <see cref="GeometryCache"/>s. Create a <see cref="GeometryCacheBuilder{TVertex}"/> with the desired
	/// vertex type <typeparamref name="TVertex"/>, add models using <see cref="AddModel"/>, and then build the cache using
	/// <see cref="Build"/>.
	/// </summary>
	/// <typeparam name="TVertex">The vertex type. Must be <see cref="Extensions.IsBlittable">blittable</see>.
	/// Every field in the given type must be annotated with a single <see cref="VertexComponentAttribute"/>.</typeparam>
	public sealed class GeometryCacheBuilder<TVertex> {
		private const int START_POINT_ARRAY_ALIGNMENT = 7; // Prime number to avoid page alignment
		private static int nextCacheID = -1;
		private readonly int cacheID = Interlocked.Increment(ref nextCacheID);
		private readonly object instanceMutationLock = new object();
		private readonly List<TVertex> vertices = new List<TVertex>();
		private readonly List<uint> indices = new List<uint>();
		private readonly List<uint> vertexCounts = new List<uint>();
		private readonly List<uint> indexCounts = new List<uint>();
		private readonly List<string> modelNames = new List<string>();
		private bool isBuilt = false;
		private bool orderFirst = false;

		public bool OrderFirst {
			get {
				lock (instanceMutationLock) {
					return orderFirst;
				}
			}
			set {
				lock (instanceMutationLock) {
					orderFirst = value;
				}
			}
		}

		/// <summary>
		/// Adds a new model to the cache. The given list of vertices and indices will constitute a single model that can 
		/// be instanced and placed anywhere in the scene (through a <see cref="SceneLayer"/>); referenced by the returned
		/// model index.
		/// </summary>
		/// <param name="modelName">The unique name for this model.</param>
		/// <param name="vertices">The list of vertices that constitute this model. Must not be null.</param>
		/// <param name="indices">The list of 0-based indices that index in to the given vertex list. Must not be null.</param>
		/// <returns>A model index that can be used to create instances of this model via the built <see cref="GeometryCache"/>.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "indices",
			Justification = "Indices is more in-tune with the graphics-programming side of the world.")]
		public ModelHandle AddModel(string modelName, IList<TVertex> vertices, IList<uint> indices) {
			if (vertices == null) throw new ArgumentNullException("vertices");
			if (indices == null) throw new ArgumentNullException("indices");
			if (vertices.Count == 0) throw new ArgumentException("Invalid model: Empty vertex list.", "vertices");
			if (indices.Count == 0) throw new ArgumentException("Invalid model: Empty index list.", "indices");
			if (modelName.IsNullOrWhiteSpace()) throw new ArgumentException("Invalid model name. Can not be null or whitespace.", "modelName");
			
			lock (instanceMutationLock) {
				if (isBuilt) throw new InvalidOperationException("Can not add models after cache has been built.");
				this.vertices.AddRange(vertices);
				this.indices.AddRange(indices);
				vertexCounts.Add((uint) vertices.Count);
				indexCounts.Add((uint) indices.Count);
				modelNames.Add(modelName);
				return new ModelHandle(cacheID, (uint)vertexCounts.Count - 1U);
			}
		}

		/// <summary>
		/// Builds the <see cref="GeometryCache"/> with all the models that have previously been added with <see cref="AddModel"/>.
		/// This method may only be called once per GeometryCacheBuilder.
		/// </summary>
		/// <returns>A new <see cref="GeometryCache"/> containing 'baked' resource data for all the loaded models and an
		/// "instance pool", allowing model instances to be created and loaded in to the <see cref="Scene"/>.</returns>
		public unsafe GeometryCache Build() {
			lock (instanceMutationLock) {
				if (isBuilt) throw new InvalidOperationException("Cache has already been built!");

				FieldInfo[] vertexComponents = typeof(TVertex).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (vertexComponents.Length > VertexShader.MAX_VS_INPUT_BINDINGS) {
					throw new InvalidOperationException("Vertex type '" + typeof(TVertex).Name + "' has too many components. Maximum " +
						"permissible is " + VertexShader.MAX_VS_INPUT_BINDINGS + ", but the vertex type has " + vertexComponents.Length + " fields.");
				}
				else if (vertexComponents.Length == 0) {
					throw new InvalidOperationException("Vertex type '" + typeof(TVertex).Name + "' has no components. Please choose a type with " +
						"at least one field.");
				}

				if (vertexCounts.Count == 0) {
					throw new InvalidOperationException("Can not build empty geometry cache: Please add at least one model first!");
				}

				if (modelNames.AnyDuplicates()) {
					throw new InvalidOperationException("No two models should have the same name.");
				}
				if (GeometryCache.CheckForModelNameClashes(modelNames)) {
					throw new InvalidOperationException("One or more model names added to this builder have already been used in other active geometry caches.");
				}



				IVertexBuffer[] vertexComponentBuffers = new IVertexBuffer[vertexComponents.Length];
				string[] vertexComponentSemantics = new string[vertexComponents.Length];
				ResourceFormat[] vertexComponentFormats = new ResourceFormat[vertexComponents.Length];

				for (int i = 0; i < vertexComponents.Length; ++i) {
					FieldInfo component = vertexComponents[i];
					if (!component.FieldType.IsBlittable()) {
						throw new InvalidOperationException("Invalid vertex component type: '" + component.FieldType.Name + "'.");
					}
					if (!component.HasCustomAttribute<VertexComponentAttribute>()) {
						throw new InvalidOperationException("Every field on given vertex type (" + typeof(TVertex).Name + ") must be annoted with " +
							"a " + typeof(VertexComponentAttribute).Name + "!");
					}

					typeof(GeometryCacheBuilder<TVertex>)
						.GetMethod("FillComponentBuffer", BindingFlags.NonPublic | BindingFlags.Instance) // TODO replace with nameof() operator when C#6 is released
						.MakeGenericMethod(component.FieldType)
						.Invoke(this, new object[] { component, i, vertexComponentBuffers, vertexComponentSemantics, vertexComponentFormats });
				}

				IndexBuffer indexBuffer = BufferFactory.NewIndexBuffer().WithInitialData(indices.ToArray()).WithUsage(ResourceUsage.Immutable);

				Assure.Equal(vertexCounts.Count, indexCounts.Count);
				AlignedAllocation<uint> componentStartPointsAlloc = AlignedAllocation<uint>.AllocArray(
					START_POINT_ARRAY_ALIGNMENT, 
					(uint) vertexCounts.Count + 1U // Extra one so we can set the last value to one-past-the-end (performance improvement later on)
				);
				AlignedAllocation<uint> indexStartPointsAlloc = AlignedAllocation<uint>.AllocArray(
					START_POINT_ARRAY_ALIGNMENT,
					(uint) vertexCounts.Count + 1U // Extra one so we can set the last value to one-past-the-end (performance improvement later on)
				);
				uint* componentStartPtr = (uint*) componentStartPointsAlloc.AlignedPointer;
				uint* indexStartPtr = (uint*) indexStartPointsAlloc.AlignedPointer;
				uint vbCounter = 0U;
				uint ibCounter = 0U;
				for (int i = 0; i < vertexCounts.Count; ++i) {
					componentStartPtr[i] = vbCounter;
					indexStartPtr[i] = ibCounter;
					vbCounter += vertexCounts[i];
					ibCounter += indexCounts[i];
				}

				// Set the last two elements of each start-point array to one-past-the-last, so we don't have to test
				// for being the 'last' count later on
				componentStartPtr[vertexCounts.Count] = (uint) vertices.Count;
				indexStartPtr[vertexCounts.Count] = (uint) indices.Count;

				Dictionary<string, ModelHandle> modelNameToHandleMap = new Dictionary<string, ModelHandle>();
				for (uint i = 0U; i < modelNames.Count; ++i) {
					modelNameToHandleMap.Add(modelNames[(int) i], new ModelHandle(cacheID, i));
				}

				isBuilt = true;

				return new GeometryCache(
					vertexComponentBuffers, 
					vertexComponentSemantics,
					vertexComponentFormats, 
					indexBuffer, 
					componentStartPointsAlloc, 
					indexStartPointsAlloc,
					(uint) vertexCounts.Count,
					typeof(TVertex),
					cacheID,
					modelNameToHandleMap,
					orderFirst
				);
			}
		}

		// ReSharper disable once UnusedMember.Local Is used through reflection
		private void FillComponentBuffer<TComponent>(FieldInfo component, int componentIndex, 
			IBuffer[] vertexComponentBuffers, string[] vertexComponentSemantics, ResourceFormat[] vertexComponentFormats) where TComponent : struct {
			TComponent[] initialData = new TComponent[vertices.Count];

			for (int i = 0; i < vertices.Count; ++i) {
				initialData[i] = (TComponent) component.GetValue(vertices[i]);
			}

			VertexBuffer<TComponent> componentBuffer = BufferFactory.NewVertexBuffer<TComponent>()
				.WithInitialData(initialData)
				.WithUsage(ResourceUsage.Immutable);

			vertexComponentBuffers[componentIndex] = componentBuffer;
			vertexComponentSemantics[componentIndex] = component.GetCustomAttribute<VertexComponentAttribute>().SemanticName;
			ResourceFormat componentFormat = (ResourceFormat) component.GetCustomAttribute<VertexComponentAttribute>().D3DResourceFormat;
			if (componentFormat == ResourceFormat.Unknown) {
				componentFormat = BaseResource.GetFormatForType(typeof(TComponent));
				if (componentFormat == ResourceFormat.Unknown) {
					throw new InvalidOperationException("Could not discern resource format for type '" + typeof(TComponent).Name + "' " +
						"with semantic '" + vertexComponentSemantics[componentIndex] + "', " +
						"please specify a resource format using the \"D3DResourceFormat\" field of the VertexComponent attribute.");
				}
			}
			vertexComponentFormats[componentIndex] = componentFormat;
		}
	}
}