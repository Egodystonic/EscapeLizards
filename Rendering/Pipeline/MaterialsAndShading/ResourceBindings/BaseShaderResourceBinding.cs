// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 29 01 2015 at 13:39 by Ben Bowen

using System;
using System.Collections.Generic;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// The base class of all <see cref="IShaderResourceBinding"/> implementations. Provides a common abstract functionality.
	/// </summary>
	/// <remarks>
	/// This class is not meant to be used directly in code, use the <see cref="IShaderResourceBinding"/> interface instead to represent
	/// a generic shader binding.
	/// </remarks>
	/// <typeparam name="TResource">The resource type that can be set through child implementers.</typeparam>
	public abstract class BaseShaderResourceBinding<TResource> : IShaderResourceBinding where TResource : class {
		private readonly object instanceMutationLock = new object();
		private readonly uint slotIndex;
		private readonly string identifier;
		private TResource boundResource = null;

		/// <summary>
		/// Resource binding identifier. Unique within the owning shader.
		/// </summary>
		public string Identifier {
			get { return identifier; }
		}

		/// <summary>
		/// The register input slot that is used in the shader program to access the resources set via this binding.
		/// </summary>
		public uint SlotIndex {
			get { return slotIndex; }
		}

		/// <summary>
		/// Used by the <see cref="ToString"/> implementation in <see cref="BaseShaderResourceBinding{TResource}"/>.
		/// Should return the correct register identifier prefix for the binding type, as would be used for resources of
		/// type <typeparamref name="TResource"/>. For example, constant buffers use the register prefix <c>cb</c>.
		/// </summary>
		protected abstract string RegisterIdentifier { get; }

		internal BaseShaderResourceBinding(uint slotIndex, string identifier) {
			if (identifier.IsNullOrWhiteSpace()) throw new ArgumentException("Invalid identifier.", "identifier");
			this.identifier = identifier;
			this.slotIndex = slotIndex;
		}

		/// <summary>
		/// Bind the given <paramref name="resource"/> to the owning shader at the given 
		/// <see cref="IShaderResourceBinding.SlotIndex"/>/<see cref="IShaderResourceBinding.Identifier"/>.
		/// </summary>
		/// <param name="resource">The resource to bind. May be null (to 'unbind' the resource slot).</param>
		public virtual void Bind(TResource resource) {
			lock (instanceMutationLock) {
				boundResource = resource;
			}
		}

		/// <summary>
		/// Get the resource currently bound to the owning shader through this binding.
		/// </summary>
		/// <returns>The currently bound resource. May be null if no resource is bound.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
			Justification = "I don't want to turn it into a property because I only want the 'setter' (Bind()) to be virtual.")]
		public TResource GetBoundResource() {
			lock (instanceMutationLock) {
				return boundResource;
			}
		}

		/// <summary>
		/// Returns a string representing this resource binding.
		/// </summary>
		/// <returns>A string that uniquely identifies this resource binding.</returns>
		public override string ToString() {
			return this.GetType().Name + " '" + RegisterIdentifier + slotIndex + " : " + identifier + "'";
		}

		/// <summary>
		/// Bind the given <paramref name="resource"/> to the owning shader at the given <see cref="IShaderResourceBinding.SlotIndex"/>/<see cref="IShaderResourceBinding.Identifier"/>.
		/// </summary>
		/// <param name="resource">The resource to bind. May be null (to 'unbind' the resource slot). Must be the right type of 
		/// resource (<typeparamref name="TResource"/>) for the binding (otherwise an exception will be thrown).</param>
		void IShaderResourceBinding.Bind(object resource) {
			if (resource != null && !(resource is TResource)) {
				throw new ArgumentException("Bound resource must be of type '" + typeof(TResource).Name + "'!", "resource");
			}

			Bind((TResource) resource);
		}

		/// <summary>
		/// Get the resource currently bound to the owning shader through this binding.
		/// </summary>
		/// <returns>The currently bound resource. May be null if no resource is bound.</returns>
		object IShaderResourceBinding.GetBoundResource() {
			return GetBoundResource();
		}
	}
}