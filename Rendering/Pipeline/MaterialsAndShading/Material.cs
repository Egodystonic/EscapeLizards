// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 05 01 2015 at 12:47 by Ben Bowen

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// A material is a shaded style used to represent the surface of an object.
	/// </summary>
	public sealed class Material : IDisposable {
		private static readonly object staticMutationLock = new object();
		private static readonly Dictionary<FragmentShader, List<Material>> shaderMaterialMap = new Dictionary<FragmentShader, List<Material>>();
		private static Material[] activeMaterials = new Material[1];
		/// <summary>
		/// Name of this material.
		/// </summary>
		public readonly string Name;
		/// <summary>
		/// The shader used to render objects using this material.
		/// </summary>
		public readonly FragmentShader Shader;
		internal readonly uint Index;
		private readonly object instanceMutationLock = new object();
		private readonly Dictionary<ConstantBufferBinding, IntPtr> cbufferValuePtrs = new Dictionary<ConstantBufferBinding, IntPtr>();
		private readonly ShaderResourcePackage fragmentShaderResources = new ShaderResourcePackage();
		private bool isDisposed = false;
		private int zIndex = 0;

		public static Dictionary<FragmentShader, List<Material>> ShaderMaterialMap {
			get {
				return shaderMaterialMap;
			}
		}

		/// <summary>
		/// True if this material has been disposed.
		/// </summary>
		public bool IsDisposed {
			get {
				lock (instanceMutationLock) {
					return isDisposed;
				}
			}
		}

		public int ZIndex {
			get {
				lock (instanceMutationLock) {
					return zIndex;
				}
			}
			set {
				lock (instanceMutationLock) {
					zIndex = value;
				}
			}
		}

		/// <summary>
		/// Returns the shader resource package associated with this material that can be used to set the <see cref="Shader"/> resources
		/// by a <see cref="RenderPass"/>.
		/// </summary>
		public ShaderResourcePackage FragmentShaderResourcePackage {
			get {
				lock (instanceMutationLock) { // Locked over because it's not a thread-safe collection
					return fragmentShaderResources;
				}
			}
		}

		/// <summary>
		/// Constructs a new material.
		/// </summary>
		/// <param name="name">The name of the material. Must not be null or whitespace.</param>
		/// <param name="shader">The fragment shader used to draw objects rendered with this material.</param>
		public Material(string name, FragmentShader shader) {
			Assure.False(name.IsNullOrEmpty(), "Invalid material name.");
			Assure.NotNull(shader);
			Assure.False(shader.IsDisposed, "Shader must not be disposed.");
			Name = name;
			Shader = shader;
			lock (staticMutationLock) {
				for (uint i = 0; i < activeMaterials.Length; ++i) {
					if (activeMaterials[i] == null) {
						Index = i;
						goto slotFound;
					}
				}
				Material[] newMatArray = new Material[activeMaterials.Length * 2];
				Array.Copy(activeMaterials, newMatArray, activeMaterials.Length);
				Index = (uint) activeMaterials.Length;
				activeMaterials = newMatArray;
				slotFound: activeMaterials[Index] = this;

				shaderMaterialMap.GetOrCreate(shader, _ => new List<Material>()).Add(this);
			}
		}

#if DEBUG
		~Material() {
			if (!IsDisposed) Logger.Warn(this + " was not disposed before finalization!");
		}
#endif

		internal static Material GetMaterialByIndex(uint index) {
			lock (staticMutationLock) {
				return activeMaterials[index];
			}
		}

		/// <summary>
		/// Set a constant buffer <paramref name="value"/> associated with this material. This value will be written to the specified
		/// <paramref name="binding"/> for the set <see cref="Shader"/> when objects with this material are being rendered.
		/// </summary>
		/// <typeparam name="TValue">The type of value of the constant buffer bound to <paramref name="binding"/>.</typeparam>
		/// <param name="binding">The binding that represents the resource whose value will be set when rendering this material.</param>
		/// <param name="value">The value to set when rendering this material.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0",
			Justification = "The parameter is validated by the assurances.")]
		public unsafe void SetMaterialConstantValue<TValue>(ConstantBufferBinding binding, TValue value) where TValue : struct {
			Assure.True(Shader.ContainsBinding(binding), "Binding is not attributed to the FragmentShader set for this material.");
			Assure.True(
				binding.GetBoundResource() is ConstantBuffer<TValue>,
				"Expected a resource of type 'ConstantBuffer<" + typeof(TValue).Name + ">' set at " +
					binding + ", but instead found " + binding.GetBoundResource().ToStringNullSafe("no binding") + "."
			);
			Assure.True(
				binding.GetBoundResource().CanDiscardWrite,
				"Given shader resource (" + binding.GetBoundResource() + ") must have discard-write capability."
			);

			if (cbufferValuePtrs.ContainsKey(binding)) Marshal.FreeHGlobal(cbufferValuePtrs[binding]);
			IntPtr valuePtr = Marshal.AllocHGlobal(new IntPtr(binding.BufferSizeBytes));
			UnsafeUtils.WriteGenericToPtr(valuePtr, value, binding.BufferSizeBytes);
			using (RenderingModule.RenderStateBarrier.AcquirePermit(withLock: instanceMutationLock)) {
				cbufferValuePtrs[binding] = valuePtr;
				fragmentShaderResources.SetValue(binding, valuePtr);
			}
		}

		/// <summary>
		/// Set a shader resource (<paramref name="value"/>) associated with this material. The <paramref name="value"/> will be bound
		/// to the <see cref="Shader"/> when objects with this material are being rendered.
		/// </summary>
		/// <param name="binding">The binding that represents where the resource value will be bound to the <see cref="Shader"/>.</param>
		/// <param name="value">The resource / value to bind.</param>
		public void SetMaterialResource<TValue>(BaseShaderResourceBinding<TValue> binding, TValue value) where TValue : class {
			Assure.True(Shader.ContainsBinding(binding), "Binding is not attributed to the FragmentShader set for this material.");

			using (RenderingModule.RenderStateBarrier.AcquirePermit(withLock: instanceMutationLock)) {
				fragmentShaderResources.SetValue(binding, value);
			}
		}

		/// <summary>
		/// Cleans up resources, and releases memory reserved for setting shader values.
		/// </summary>
		public void Dispose() {
			lock (instanceMutationLock) {
				isDisposed = true;

				foreach (var ptr in cbufferValuePtrs.Values) {
					Marshal.FreeHGlobal(ptr);
				}
			}
			lock (staticMutationLock) {
				activeMaterials[Index] = null;
				shaderMaterialMap[Shader].Remove(this);
			}
		}

		/// <summary>
		/// Returns a string with this material's name.
		/// </summary>
		/// <returns>A string with this material's name.</returns>
		public override string ToString() {
			return "Material '" + Name + "'";
		}
	}
}