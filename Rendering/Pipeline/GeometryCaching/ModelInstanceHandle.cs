// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 23 01 2015 at 17:40 by Ben Bowen

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// A handle that represents a previously-allocated instance of a preloaded model.
	/// </summary>
	public unsafe struct ModelInstanceHandle : IEquatable<ModelInstanceHandle>, IDisposable {
		internal readonly uint MaterialIndex;
		internal readonly uint InstanceIndex;
		private readonly ModelInstanceManager AllocatingManager;

		/// <summary>
		/// Gets or sets the transform applied to this instance.
		/// </summary>
		public Transform Transform {
			get {
				Assure.False(AllocatingManager.IsDisposed, "Attempted to get transform on instance handle provided through now-disposed cache.");
				return AllocatingManager.GetTransform(MaterialIndex, InstanceIndex);
			}
			set {
				Assure.False(AllocatingManager.IsDisposed, "Attempted to set transform on instance handle provided through now-disposed cache.");
				AllocatingManager.SetTransform(MaterialIndex, InstanceIndex, value);
			}
		}

		/// <summary>
		/// The material that this instance was created with.
		/// </summary>
		public Material Material {
			get {
				return Material.GetMaterialByIndex(MaterialIndex);
			}
		}

		internal ModelInstanceHandle(ModelInstanceManager allocatingManager, uint materialIndex, uint instanceIndex) {
			AllocatingManager = allocatingManager;
			MaterialIndex = materialIndex;
			this.InstanceIndex = instanceIndex;
		}

		/// <summary>
		/// Destroys this instance, freeing allocated memory, removing the instance from the scene, and invalidating this handle.
		/// </summary>
		public void Dispose() {
			AllocatingManager.DisposeInstance(MaterialIndex, InstanceIndex);
		}

		/// <summary>
		/// Returns a string representing this instance and its current state/values.
		/// </summary>
		/// <returns>
		/// A new string that details this object.
		/// </returns>
		public override string ToString() {
			return "Model Instance Handle '" + AllocatingManager + "-" + MaterialIndex + "-" + InstanceIndex + "'";
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(ModelInstanceHandle other) {
			return Equals(AllocatingManager, other.AllocatingManager) && MaterialIndex == other.MaterialIndex && InstanceIndex == other.InstanceIndex;
		}

		/// <summary>
		/// Indicates whether this instance and a specified object are equal.
		/// </summary>
		/// <returns>
		/// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false. 
		/// </returns>
		/// <param name="obj">The object to compare with the current instance. </param><filterpriority>2</filterpriority>
		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) {
				return false;
			}
			return obj is ModelInstanceHandle && Equals((ModelInstanceHandle)obj);
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>
		/// A 32-bit signed integer that is the hash code for this instance.
		/// </returns>
		/// <filterpriority>2</filterpriority>
		public override int GetHashCode() {
			unchecked {
				int hashCode = (AllocatingManager != null ? AllocatingManager.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (int)MaterialIndex;
				hashCode = (hashCode * 397) ^ (int)InstanceIndex;
				return hashCode;
			}
		}
		
		/// <summary>
		/// Ascertains whether or not the two given handles refer to the same model instance.
		/// </summary>
		/// <param name="left">The first handle.</param>
		/// <param name="right">The second handle.</param>
		/// <returns>True if <paramref name="left"/> and <paramref name="right"/> both refer to the same model instance.</returns>
		public static bool operator ==(ModelInstanceHandle left, ModelInstanceHandle right) {
			return left.Equals(right);
		}

		/// <summary>
		/// Ascertains whether or not the two given handles refer to different model instances.
		/// </summary>
		/// <param name="left">The first handle.</param>
		/// <param name="right">The second handle.</param>
		/// <returns>True if <paramref name="left"/> and <paramref name="right"/> both refer to different model instances.</returns>
		public static bool operator !=(ModelInstanceHandle left, ModelInstanceHandle right) {
			return !left.Equals(right);
		}
	}
}