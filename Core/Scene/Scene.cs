// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 22 12 2014 at 10:26 by Ben Bowen

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Ophidian.Losgap {
	/// <summary>
	/// A static class representing the entire scene (possibly containing a number of <see cref="SceneLayer"/>s).
	/// </summary>
	public static class Scene {
		private static readonly object staticMutationLock = new object();
		private static readonly ICollection<SceneLayer> createdLayers = new HashSet<SceneLayer>();
		private static List<SceneLayer> enabledLayers;

		private static event Action<SceneLayer> layerCreated;

		/// <summary>
		/// Event fired when a <see cref="SceneLayer"/> is added to the scene (with <see cref="CreateLayer"/>).
		/// The only parameter is a reference to the newly created layer.
		/// </summary>
		public static event Action<SceneLayer> LayerCreated {
			add {
				lock (staticMutationLock) {
					layerCreated += value;
				}
			}
			remove {
				lock (staticMutationLock) {
					layerCreated -= value;
				}
			}
		}

		/// <summary>
		/// A read-only collection of the <see cref="SceneLayer"/>s currently added to the scene that are 
		/// <see cref="SceneLayer.IsEnabled">enabled</see>.
		/// </summary>
		public static List<SceneLayer> EnabledLayers {
			get {
				lock (staticMutationLock) {
					return enabledLayers;
				}
			}
		}

		/// <summary>
		/// A read-only collection of the <see cref="SceneLayer"/>s currently added to the scene (including those that are
		/// not currently <see cref="SceneLayer.IsEnabled">enabled</see>).
		/// </summary>
		public static List<SceneLayer> AllLayers {
			get {
				lock (staticMutationLock) {
					// Creating a list over the collection also means that we avoid concurrent modifications on it
					return createdLayers.ToList();
				}
			}
		}

		/// <summary>
		/// Creates a new <see cref="SceneLayer"/> and adds it to the scene.
		/// Scenes are created as disabled (e.g. <see cref="SceneLayer.IsEnabled"/> is false).
		/// </summary>
		/// <param name="layerName">The name for the layer. Must not be null.</param>
		/// <exception cref="InvalidOperationException">Thrown if a layer with the given name already exists in the scene.</exception>
		/// <returns>The newly created layer. The layer will also be added to the <see cref="AllLayers"/> collection,
		/// so you do not need to retain a reference to the returned object to keep it from becoming garbage.</returns>
		public static SceneLayer CreateLayer(string layerName) {
			lock (staticMutationLock) {
				if (createdLayers.Any(layer => layer.Name == layerName)) {
					throw new InvalidOperationException("An undisposed layer with the name '" + layerName + "' already exists in the scene.");
				}
				SceneLayer result = new SceneLayer(layerName);
				result.LayerDisposed += LayerDisposed;
				createdLayers.Add(result);
				if (layerCreated != null) layerCreated(result);
				UpdateEnabledLayers();
				return result;
			}
		}

		/// <summary>
		/// Gets the scene layer that was created with the given <paramref name="layerName"/>.
		/// </summary>
		/// <param name="layerName">The name of the layer you wish to retrieve.</param>
		/// <exception cref="KeyNotFoundException">Thrown if no undisposed layer exists in the scene with the given 
		/// <paramref name="layerName"/>.</exception>
		/// <returns>The scene layer that has the given name.</returns>
		public static SceneLayer GetLayer(string layerName) {
			lock (staticMutationLock) {
				try {
					return createdLayers.First(layer => layer.Name == layerName);
				}
				catch (InvalidOperationException e) {
					throw new KeyNotFoundException("Layer with name '" + layerName + "' is not in the ActiveLayers collection.", e);
				}
			}
		}

		/// <summary>
		/// Gets the scene layer with the given <paramref name="index"/>.
		/// </summary>
		/// <param name="index">The index of the scene layer you wish to retrieve.</param>
		/// <returns>The SceneLayer with the given index.</returns>
		/// <exception cref="IndexOutOfRangeException">Thrown if no active scene layer with the given <paramref name="index"/> is
		/// currently added.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SceneLayer GetLayerByIndex(uint index) {
			return SceneLayer.GetLayerByIndex(index);
		}

		private static void LayerDisposed(SceneLayer layer) {
			lock (staticMutationLock) {
				createdLayers.Remove(layer);
				UpdateEnabledLayers();
			}
		}

		internal static void UpdateEnabledLayers() {
			lock (staticMutationLock) {
				enabledLayers = createdLayers.Where(layer => layer.IsEnabled).ToList();
			}
		}
	}
}