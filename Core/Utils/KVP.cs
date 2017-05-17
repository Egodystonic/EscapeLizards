// All code copyright (c) 2016 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 04 10 2016 at 05:35 by Ben Bowen

using System;
using System.Collections.Generic;

namespace Ophidian.Losgap {
	public struct KVP<TKey, TValue> : IEquatable<KVP<TKey, TValue>> {
		public readonly TKey Key;
		public readonly TValue Value;

		public KVP(TKey key, TValue value) {
			Key = key;
			Value = value;
		}

		public bool Equals(KVP<TKey, TValue> other) {
			return EqualityComparer<TKey>.Default.Equals(Key, other.Key) && EqualityComparer<TValue>.Default.Equals(Value, other.Value);
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) {
				return false;
			}
			return obj is KVP<TKey, TValue> && Equals((KVP<TKey, TValue>)obj);
		}

		public override int GetHashCode() {
			unchecked {
				return (EqualityComparer<TKey>.Default.GetHashCode(Key) * 397) ^ EqualityComparer<TValue>.Default.GetHashCode(Value);
			}
		}

		public static bool operator ==(KVP<TKey, TValue> left, KVP<TKey, TValue> right) {
			return left.Equals(right);
		}

		public static bool operator !=(KVP<TKey, TValue> left, KVP<TKey, TValue> right) {
			return !left.Equals(right);
		}


		public static implicit operator KeyValuePair<TKey, TValue>(KVP<TKey, TValue> lhs) {
			return new KeyValuePair<TKey, TValue>(lhs.Key, lhs.Value);
		}

		public static implicit operator KVP<TKey, TValue>(KeyValuePair<TKey, TValue> lhs) {
			return new KVP<TKey, TValue>(lhs.Key, lhs.Value);
		}
	}
}