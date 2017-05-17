// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 05 01 2015 at 11:23 by Ben Bowen

using System;
using System.Collections.Generic;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// A simple vertex definition that can be used when adding geometry to a 
	/// <see cref="GeometryCacheBuilder{TVertex}">GeometryCache&lt;DefaultVertex&gt;</see> that will be paired
	/// with a <see cref="VertexShader.NewDefaultShader">default VertexShader</see>.
	/// </summary>
	public struct DefaultVertex : IEquatable<DefaultVertex> {
		/// <summary>
		/// The position of the vertex relative the model-space origin.
		/// </summary>
		[VertexComponent("POSITION")]
		public readonly Vector3 Position;

		/// <summary>
		/// The texture co-ordinates of the vertex.
		/// </summary>
		[VertexComponent("TEXCOORD")]
		public readonly Vector2 TexUV;

		[VertexComponent("NORMAL")]
		public readonly Vector3 Normal;

		[VertexComponent("TANGENT")]
		public readonly Vector3 Tangent;

		/// <summary>
		/// Constructs a new vertex with the given <paramref name="position"/>.
		/// </summary>
		/// <param name="position">The position of the vertex relative the model-space origin.</param>
		public DefaultVertex(Vector3 position)
			: this(position, Vector3.FORWARD, Vector2.ZERO) { }

		/// <summary>
		/// Constructs a new vertex with the given <paramref name="position"/>.
		/// </summary>
		/// <param name="position">The position of the vertex relative the model-space origin.</param>
		/// <param name="texUv">The texture co-ordinates of the vertex.</param>
		public DefaultVertex(Vector3 position, Vector2 texUv)
			: this(position, Vector3.FORWARD, texUv) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.Object"/> class.
		/// </summary>
		public DefaultVertex(Vector3 position, Vector3 normal, Vector2 texUv) : this(position, normal, Vector3.ONE, texUv) { }

		public DefaultVertex(Vector3 position, Vector3 normal, Vector3 tangent, Vector2 texUv) {
			Position = position;
			TexUV = texUv;
			Normal = normal;
			Tangent = tangent;
		}


		#region Equality
		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(DefaultVertex other) {
			return Position.Equals(other.Position) && TexUV.Equals(other.TexUV) && Normal.Equals(other.Normal) && Tangent.Equals(other.Tangent);
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
			return obj is DefaultVertex && Equals((DefaultVertex)obj);
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>
		/// A 32-bit signed integer that is the hash code for this instance.
		/// </returns>
		public override int GetHashCode() {
			unchecked {
				int hashCode = Position.GetHashCode();
				hashCode = (hashCode * 397) ^ TexUV.GetHashCode();
				hashCode = (hashCode * 397) ^ Normal.GetHashCode();
				hashCode = (hashCode * 397) ^ Tangent.GetHashCode();
				return hashCode;
			}
		}

		public static bool operator ==(DefaultVertex left, DefaultVertex right) {
			return left.Equals(right);
		}

		public static bool operator !=(DefaultVertex left, DefaultVertex right) {
			return !left.Equals(right);
		}
		#endregion

	}
}