using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// Represents a model added to a <see cref="GeometryCache"/>.
	/// </summary>
	public struct ModelHandle : IEquatable<ModelHandle> {
		internal readonly int GeoCacheID;
		internal readonly uint ModelIndex;

		internal ModelHandle(int geoCacheId, uint modelIndex) {
			ModelIndex = modelIndex;
			GeoCacheID = geoCacheId;
		}

		/// <summary>
		/// Returns a string representing this instance and its current state/values.
		/// </summary>
		/// <returns>
		/// A new string that details this object.
		/// </returns>
		public override string ToString() {
			return "ModelHandle(" + GeoCacheID + "/" + ModelIndex + ")";
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(ModelHandle other) {
			return GeoCacheID == other.GeoCacheID && ModelIndex == other.ModelIndex;
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
			return obj is ModelHandle && Equals((ModelHandle)obj);
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>
		/// A 32-bit signed integer that is the hash code for this instance.
		/// </returns>
		/// <filterpriority>2</filterpriority>
		public override int GetHashCode() {
			return ModelIndex.GetHashCode();
		}

		/// <summary>
		/// Ascertains whether or not the two given handles refer to the same model instance.
		/// </summary>
		/// <param name="left">The first handle.</param>
		/// <param name="right">The second handle.</param>
		/// <returns>True if <paramref name="left"/> and <paramref name="right"/> both refer to the same model instance.</returns>
		public static bool operator ==(ModelHandle left, ModelHandle right) {
			return left.Equals(right);
		}

		/// <summary>
		/// Ascertains whether or not the two given handles refer to different model instances.
		/// </summary>
		/// <param name="left">The first handle.</param>
		/// <param name="right">The second handle.</param>
		/// <returns>True if <paramref name="left"/> and <paramref name="right"/> both refer to different model instances.</returns>
		public static bool operator !=(ModelHandle left, ModelHandle right) {
			return !left.Equals(right);
		}
	}
}
