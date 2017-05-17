// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 22 12 2014 at 10:27 by Ben Bowen

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Ophidian.Losgap {
	/// <summary>
	/// Represents an independently-controllable 'layer' of a scene. 
	/// Scene layers can be used to represent different components of the user interface, different areas of the 3D world, etc.
	/// </summary>
	/// <remarks>
	/// By enabling and disabling different layers at different times, it is possible to quickly switch contexts of your application
	/// without paying any performance penalty (however, each scene layer currently added to the <see cref="Scene"/> 
	/// will add memory pressure on the application, even if it is not <see cref="IsEnabled">enabled</see>).
	/// </remarks>
	public sealed class SceneLayer : IDisposable {
		private static readonly object staticMutationLock = new object();
		private static SceneLayer[] activeLayers = new SceneLayer[1];

		/// <summary>
		/// The index of this scene layer.
		/// </summary>
		/// <seealso cref="Scene.AllLayers"/>
		/// <seealso cref="GetLayerByIndex"/>
		public readonly uint Index;
		private readonly IDictionary<string, object> properties = new Dictionary<string, object>();
		private readonly IDictionary<Type, SceneLayerResource> resources = new Dictionary<Type, SceneLayerResource>();
		private readonly object instanceMutationLock = new object();

		/// <summary>
		/// The name of this layer. Never null.
		/// </summary>
		public readonly string Name;
		private bool isDisposed = false;
		private bool isEnabled = true;

		private event Action<SceneLayer> layerEnabled;
		private event Action<SceneLayer> layerDisabled;
		private event Action<SceneLayer> layerDisposed;
		private event Action<SceneLayer, string, object> propertySet;
		private event Action<SceneLayer, string, object> propertyRemoved;

		/// <summary>
		/// An event fired when this layer is enabled. The only parameter is this SceneLayer.
		/// </summary>
		public event Action<SceneLayer> LayerEnabled {
			add {
				lock (instanceMutationLock) {
					layerEnabled += value;
				}
			}
			remove {
				lock (instanceMutationLock) {
					layerEnabled -= value;
				}
			}
		}

		/// <summary>
		/// An event fired when this layer is disabled. The only parameter is this SceneLayer.
		/// </summary>
		public event Action<SceneLayer> LayerDisabled {
			add {
				lock (instanceMutationLock) {
					layerDisabled += value;
				}
			}
			remove {
				lock (instanceMutationLock) {
					layerDisabled -= value;
				}
			}
		}

		/// <summary>
		/// An event fired when this layer is disposed. The only parameter is this SceneLayer.
		/// </summary>
		public event Action<SceneLayer> LayerDisposed {
			add {
				lock (instanceMutationLock) {
					layerDisposed += value;
				}
			}
			remove {
				lock (instanceMutationLock) {
					layerDisposed -= value;
				}
			}
		}

		/// <summary>
		/// An event fired when a property is set on this layer.
		/// The first parameter is this SceneLayer. The second parameter is the property key. The third parameter is the property value.
		/// </summary>
		public event Action<SceneLayer, string, object> PropertySet {
			add {
				lock (instanceMutationLock) {
					propertySet += value;
				}
			}
			remove {
				lock (instanceMutationLock) {
					propertySet -= value;
				}
			}
		}

		/// <summary>
		/// An event fired when a property is removed from this layer.
		/// The first parameter is this SceneLayer. The second parameter is the property key. 
		/// The third parameter is the property value at the time it was removed.
		/// </summary>
		public event Action<SceneLayer, string, object> PropertyRemoved {
			add {
				lock (instanceMutationLock) {
					propertyRemoved += value;
				}
			}
			remove {
				lock (instanceMutationLock) {
					propertyRemoved -= value;
				}
			}
		}

		/// <summary>
		/// Whether or not this layer is 'enabled'. Enabled layers may be rendered, ticked, used in physics calculations, receive input, etc.
		/// Disabled layers are simply 'dormant', exerting no processor pressure.
		/// </summary>
		public bool IsEnabled {
			get {
				lock (instanceMutationLock) {
					return !isDisposed && isEnabled;
				}
			}
			set {
				lock (instanceMutationLock) {
					if (isDisposed || isEnabled == value) return;

					isEnabled = value;

					Scene.UpdateEnabledLayers();
					if (isEnabled) OnLayerEnabled();
					else if (!isEnabled) OnLayerDisabled();
				}
			}
		}

		/// <summary>
		/// True if this scene layer has been disposed.
		/// </summary>
		public bool IsDisposed {
			get {
				lock (instanceMutationLock) {
					return isDisposed;
				}
			}
		}

		internal SceneLayer(string name) {
			if (name == null) throw new ArgumentNullException("name");
			Name = name;

			lock (staticMutationLock) {
				for (uint i = 0; i < activeLayers.Length; ++i) {
					if (activeLayers[i] == null) {
						Index = i;
						goto slotFound;
					}
				}
				SceneLayer[] newMatArray = new SceneLayer[activeLayers.Length * 2];
				Array.Copy(activeLayers, newMatArray, activeLayers.Length);
				Index = (uint) activeLayers.Length;
				activeLayers = newMatArray;
				slotFound: activeLayers[Index] = this;
			}

			foreach (Type resourceType in ReflectionUtils.GetChildTypes(typeof(SceneLayerResource))) {
				if (resourceType.IsInstantiable()) {
					ConstructorInfo sceneLayerCtor = resourceType.GetConstructor(
						BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
						null,
						new[] { typeof(SceneLayer) },
						new[] { new ParameterModifier() }
					);
					if (sceneLayerCtor == null) {
						Logger.Debug("Found instantiable scene layer resource type '" + resourceType.Name + "', but could not instantiate, " +
							"as no constructor with signature " + resourceType.Name + "(SceneLayer) was found.");
					}
					else resources[resourceType] = (SceneLayerResource) sceneLayerCtor.Invoke(new object[] { this });
				}
			}

			OnLayerEnabled();

			Logger.Debug("Created new " + this + ".");
		}

#if DEBUG
		~SceneLayer() {
			if (!IsDisposed) Logger.Warn(this + " was not disposed before finalization!");
		}
#endif

		/// <summary>
		/// Gets the scene layer with the given <paramref name="index"/>.
		/// </summary>
		/// <param name="index">The index of the scene layer you wish to retrieve.</param>
		/// <returns>The SceneLayer with the given index.</returns>
		/// <exception cref="IndexOutOfRangeException">Thrown if no active scene layer with the given <paramref name="index"/> is
		/// currently added.</exception>
		internal static SceneLayer GetLayerByIndex(uint index) {
			lock (staticMutationLock) {
				return activeLayers[index];
			}
		}

		/// <summary>
		/// Sets a property on this layer. A property's <paramref name="value"/> may be any object (or null),
		/// and each property is keyed by a string <paramref name="key"/>.
		/// </summary>
		/// <param name="key">The unique string for identifying this property. Must not be null.</param>
		/// <param name="value">The value of this property. May be null.</param>
		public void SetProperty(string key, object value) {
			lock (instanceMutationLock) {
				if (isDisposed) return;
				properties[key] = value;
				if (propertySet != null) propertySet(this, key, value);
				Logger.Debug("Property '" + key + "' set to value '" + value + "' on " + this + ".");
			}
		}

		/// <summary>
		/// Gets the value of a property on this layer.
		/// </summary>
		/// <param name="key">The property's unique identifier. Must not be null.</param>
		/// <exception cref="KeyNotFoundException">Thrown if no property with the given <paramref name="key"/> is
		/// currently set on this layer.</exception>
		public object GetProperty(string key) {
			lock (instanceMutationLock) {
				if (isDisposed) return null;
				return properties[key];
			}
		}

		/// <summary>
		/// Ascertains whether or not a property with the given <paramref name="key"/> is currently set on this layer.
		/// </summary>
		/// <param name="key">The key to check.</param>
		/// <returns>True if the property is set, false if not.</returns>
		public bool ContainsProperty(string key) {
			lock (instanceMutationLock) {
				if (isDisposed) return false;
				return properties.ContainsKey(key);
			}
		}

		/// <summary>
		/// Removes the given property from this layer.
		/// </summary>
		/// <param name="key">The key of the property to remove.</param>
		/// <exception cref="KeyNotFoundException">Thrown if no property with the given <paramref name="key"/> is
		/// currently set on this layer.</exception>
		public void RemoveProperty(string key) {
			lock (instanceMutationLock) {
				if (isDisposed) return;
				object value = properties[key];
				properties.Remove(key);
				if (propertyRemoved != null) propertyRemoved(this, key, value);
				Logger.Debug("Property '" + key + "' removed from " + this + ".");
			}
		}

		/// <summary>
		/// Gets the value of the resource of the given type (<typeparamref name="T"/>) on this layer.
		/// </summary>
		/// <typeparam name="T">The resource type.</typeparam>
		/// <returns>The resource of the given type.</returns>
		/// <exception cref="KeyNotFoundException">Thrown if no resource of type <typeparamref name="T"/> has been
		/// set on this layer.</exception>
		public T GetResource<T>() where T : SceneLayerResource {
			lock (instanceMutationLock) {
				return (T) resources[typeof(T)];
			}
		}

		/// <summary>
		/// Returns a new string with the name of this scene layer.
		/// </summary>
		/// <returns><c>Scene Layer "Name"</c></returns>
		public override string ToString() {
			return "Scene Layer \"" + Name + "\"";
		}

		/// <summary>
		/// Disposes this layer, removing it entirely from the scene, and making it permanently disabled.
		/// </summary>
		public void Dispose() {
			lock (instanceMutationLock) {
				if (isDisposed) return;
				foreach (KeyValuePair<Type, SceneLayerResource> kvp in resources) {
					kvp.Value.Dispose();
				}
				IsEnabled = false;
				isDisposed = true;
				if (layerDisposed != null) layerDisposed(this);
				Logger.Debug(this + " disposed.");
			}
		}

		private void OnLayerEnabled() {
			foreach (KeyValuePair<Type, SceneLayerResource> kvp in resources) {
				kvp.Value.Enable();
			}
			// Assumed to be called from inside a lock: So no thread safety concerns here
			if (layerEnabled != null) layerEnabled(this);
			Logger.Debug(this + " enabled.");
		}

		private void OnLayerDisabled() {
			foreach (KeyValuePair<Type, SceneLayerResource> kvp in resources) {
				kvp.Value.Disable();
			}
			if (layerDisabled != null) layerDisabled(this);
			Logger.Debug(this + " disabled.");
		}

		/// <summary>
		/// Gets or sets a property on this layer.
		/// </summary>
		/// <param name="key">The property key.</param>
		/// <returns>The property with the given <paramref name="key"/>.</returns>
		/// <seealso cref="SetProperty"/>
		/// <seealso cref="GetProperty"/>
		public object this[string key] {
			get {
				return GetProperty(key);
			}
			set {
				SetProperty(key, value);
			}
		}
	}
}