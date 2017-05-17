// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 13 03 2015 at 16:47 by Ben Bowen

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;

namespace Ophidian.Losgap.CSG {
	internal struct CSGConnection : IEquatable<CSGConnection> {
		public readonly CSGVertex A;
		public readonly CSGVertex B;
		public readonly Ray Ray;

		public CSGConnection(CSGVertex a, CSGVertex b) {
			A = a;
			B = b;
			Ray = Ray.FromStartAndEndPoint(A.Position, B.Position);
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(CSGConnection other) {
			return A.Equals(other.A) && B.Equals(other.B);
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
			return obj is CSGConnection && Equals((CSGConnection)obj);
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>
		/// A 32-bit signed integer that is the hash code for this instance.
		/// </returns>
		public override int GetHashCode() {
			unchecked {
				return (A.GetHashCode() * 397) ^ B.GetHashCode();
			}
		}

		public static bool operator ==(CSGConnection left, CSGConnection right) {
			return left.Equals(right);
		}

		public static bool operator !=(CSGConnection left, CSGConnection right) {
			return !left.Equals(right);
		}
	}

	internal sealed class CSGVertexConnectionGraph {
		private readonly Dictionary<CSGVertex, List<CSGConnection>> mappedConnections = new Dictionary<CSGVertex, List<CSGConnection>>();
		private readonly List<CSGConnection> allConnections = new List<CSGConnection>();

		private bool MappedConnectionsContainsKey(CSGVertex key) {
			foreach (CSGVertex csgVertex in mappedConnections.Keys) {
				if (csgVertex == key) return true;
			}
			return false;
		}

		public void AddConnection(CSGVertex a, CSGVertex b) {
			if (!MappedConnectionsContainsKey(a)) mappedConnections.Add(a, new List<CSGConnection>());
			if (!MappedConnectionsContainsKey(b)) mappedConnections.Add(b, new List<CSGConnection>());

			mappedConnections.GetNoBoxing(a).Add(new CSGConnection(a, b));
			mappedConnections.GetNoBoxing(b).Add(new CSGConnection(b, a));
			allConnections.Add(new CSGConnection(a, b));
			allConnections.Add(new CSGConnection(b, a));
		}

		public void AddConnection(CSGConnection c) {
			AddConnection(c.A, c.B);
		}

		public bool CheckForConnection(CSGVertex a, CSGVertex b) {
			return MappedConnectionsContainsKey(a) && mappedConnections.GetNoBoxing(a).Any(conn => conn.B == b);
		}

		public IEnumerable<CSGConnection> GetConnections(CSGVertex v, bool outboundOnly = false) {
			if (!MappedConnectionsContainsKey(v)) mappedConnections.Add(v, new List<CSGConnection>());
			if (outboundOnly) return mappedConnections.GetNoBoxing(v).Where(conn => conn.A == v);
			else return mappedConnections.GetNoBoxing(v);
		}

		public IReadOnlyList<CSGConnection> GetAllConnections() {
			return new ReadOnlyCollection<CSGConnection>(allConnections);
		}
	}
}