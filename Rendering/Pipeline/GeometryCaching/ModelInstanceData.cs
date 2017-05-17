// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 09 01 2015 at 12:33 by Ben Bowen

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Rendering {
	internal unsafe struct ModelInstanceData : IEquatable<ModelInstanceData> {
		public bool InUse;
		public uint ModelIndex;
		public uint SceneLayerIndex;
		public Transform Transform;

		public ModelInstanceData(uint modelIndex, uint sceneLayerIndex, Transform transform) {
			InUse = true;
			ModelIndex = modelIndex;
			SceneLayerIndex = sceneLayerIndex;
			Transform = transform;
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(ModelInstanceData other) {
			return ModelIndex == other.ModelIndex && SceneLayerIndex == other.SceneLayerIndex;
		}

		/// <summary>
		/// Indicates whether this instance and a specified object are equal.
		/// </summary>
		/// <returns>
		/// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false. 
		/// </returns>
		/// <param name="obj">The object to compare with the current instance. </param>
		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) {
				return false;
			}
			return obj is ModelInstanceData && Equals((ModelInstanceData)obj);
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>
		/// A 32-bit signed integer that is the hash code for this instance.
		/// </returns>
		public override int GetHashCode() {
			unchecked {
				return ((int)ModelIndex * 397) ^ (int)SceneLayerIndex;
			}
		}

		public static bool operator ==(ModelInstanceData left, ModelInstanceData right) {
			return left.Equals(right);
		}

		public static bool operator !=(ModelInstanceData left, ModelInstanceData right) {
			return !left.Equals(right);
		}
	}
}