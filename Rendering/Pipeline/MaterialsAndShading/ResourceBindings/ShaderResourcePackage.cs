// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 11 02 2015 at 16:56 by Ben Bowen

using System;
using System.Collections.Generic;
using System.Linq;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// Represents a 'package' of resources that can are associated with each other, and each to their paired <see cref="IShaderResourceBinding"/>s.
	/// </summary>
	public class ShaderResourcePackage {
		private readonly Dictionary<IShaderResourceBinding, object> bindings;
		private readonly Dictionary<ConstantBufferBinding, IntPtr> cbBindings;
		private readonly object instanceMutationLock = new object();

		/// <summary>
		/// Constructs a new resource package.
		/// </summary>
		public ShaderResourcePackage() {
			lock (instanceMutationLock) {
				bindings = new Dictionary<IShaderResourceBinding, object>();
				cbBindings = new Dictionary<ConstantBufferBinding, IntPtr>();
			}
		}

		public ShaderResourcePackage(ShaderResourcePackage c) : this() {
			lock (instanceMutationLock) {
				foreach (var binding in c.bindings) {
					bindings.Add(binding.Key, binding.Value);
				}
				foreach (var binding in c.cbBindings) {
					cbBindings.Add(binding.Key, binding.Value);
				}
			}
		}

		/// <summary>
		/// Sets the resource for the given <paramref name="binding"/>.
		/// </summary>
		/// <typeparam name="TValue">The resource type specified by the supplied <paramref name="binding"/>.</typeparam>
		/// <param name="binding">The <see cref="IShaderResourceBinding"/> to pair a resource with.</param>
		/// <param name="value">The resource to pair to the <paramref name="binding"/>.</param>
		public void SetValue<TValue>(BaseShaderResourceBinding<TValue> binding, TValue value) where TValue : class {
			if (binding is ConstantBufferBinding) {
				SetValue((ConstantBufferBinding) (object) binding, (IntPtr) (object) value);
				return;
			}
			lock (instanceMutationLock) {
				Assure.False(binding is ConstantBufferBinding, "Can not set value for Constant Buffer Bindings.");
				this.bindings[binding] = value;
			}
		}

		/// <summary>
		/// Sets the value for the given <paramref name="binding"/>.
		/// </summary>
		/// <param name="binding">The <see cref="ConstantBufferBinding"/> to pair a value with.</param>
		/// <param name="value">A pointer to the value to pair with the given <paramref name="binding"/>. The pointer must
		/// remain valid for as long as it is set as the value on this resource package.</param>
		public void SetValue(ConstantBufferBinding binding, IntPtr value) {
			lock (instanceMutationLock) {
				this.cbBindings[binding] = value;
			}
		}

		/// <summary>
		/// Gets the currently set value for the given <paramref name="binding"/>.
		/// If no value is currently set on this resource package, this method will return the value set on the binding itself
		/// (with <see cref="IShaderResourceBinding.GetBoundResource"/>).
		/// </summary>
		/// <typeparam name="TValue">The resource type associated with the given <paramref name="binding"/>.</typeparam>
		/// <param name="binding">The binding to get the value for. Must not be null.</param>
		/// <returns>The value set on this resource package that is paired with the given binding if there is one, otherwise the resource
		/// set on the binding itself. May be null.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0",
			Justification = "The parameter is validated by the assurances.")]
		public TValue GetValue<TValue>(BaseShaderResourceBinding<TValue> binding) where TValue : class {
			if (binding is ConstantBufferBinding) {
				return (TValue) (object) GetValue((ConstantBufferBinding) (object) binding);
			}
			lock (instanceMutationLock) {
				Assure.NotNull(binding);
				if (bindings.ContainsKey(binding)) return (TValue) bindings[binding];
				else return binding.GetBoundResource();
			}
		}

		/// <summary>
		/// Gets the currently set pointer-to-value for the given <paramref name="binding"/>.
		/// If no value is currently set on this resource package, this method will return the value pointer set on the binding itself
		/// (with <see cref="ConstantBufferBinding.CurValuePtr"/>).
		/// </summary>
		/// <param name="binding">The binding to get the value for. Must not be null.</param>
		/// <returns>The value pointer set on this resource package that is paired with the given binding if there is one, 
		/// otherwise the value pointer for the binding itself. May be null.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0",
			Justification = "The parameter is validated by the assurances.")]
		public IntPtr GetValue(ConstantBufferBinding binding) {
			lock (instanceMutationLock) {
				Assure.NotNull(binding);
				if (cbBindings.ContainsKey(binding)) return cbBindings[binding];
				else return binding.CurValuePtr;
			}
		}

		public void CopyFrom(ShaderResourcePackage srp) {
			lock (instanceMutationLock) {
				foreach (var binding in srp.bindings) {
					bindings[binding.Key] = binding.Value;
				}
				foreach (var binding in srp.cbBindings) {
					cbBindings[binding.Key] = binding.Value;
				}
			}
		}
	}
}