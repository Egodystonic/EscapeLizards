// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 29 10 2014 at 14:00 by Ben Bowen

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// Provides an encapsulation over a pointer to mapped 1D resource data.
	/// </summary>
	/// <typeparam name="T">The data type. Must be <see cref="Extensions.IsBlittable">blittable</see>.</typeparam>
	public struct RawResourceDataView1D<T> : IEnumerable<T>, IEquatable<RawResourceDataView1D<T>>
		where T : struct {
		/// <summary>
		/// A pointer to the start of the mapped data.
		/// </summary>
		public readonly IntPtr Data;
		/// <summary>
		/// The number of elements in the u-direction that are represented by this resource view.
		/// </summary>
		public readonly uint Width;
		private readonly uint sizeOfT;

		private class DataViewEnumerator<TEnum> : IEnumerator<TEnum> where TEnum : struct {
			private RawResourceDataView1D<TEnum> parent;
			private int currentIndexU;

			public DataViewEnumerator(RawResourceDataView1D<TEnum> parent) {
				this.parent = parent;
				Reset();
			}

			public void Dispose() {
				// Do nothing
			}

			public bool MoveNext() {
				return ++currentIndexU < parent.Width;
			}

			public void Reset() {
				currentIndexU = -1;
			}

			object IEnumerator.Current {
				get { return Current; }
			}

			public TEnum Current {
				get {
					return parent[currentIndexU];
				}
			}
		}

		/// <summary>
		/// Creates a new raw resource data view.
		/// </summary>
		/// <param name="data">A pointer to the start of the mapped data.</param>
		/// <param name="sizeOfT">sizeof(<typeparamref name="T"/>).</param>
		/// <param name="width">The number of elements in the u-direction that are represented by this resource view.</param>
		public RawResourceDataView1D(IntPtr data, uint sizeOfT, uint width) {
			Assure.True(typeof(T).IsBlittable());
			Data = data;
			Width = width;
			this.sizeOfT = sizeOfT;
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
		/// </returns>
		/// <filterpriority>1</filterpriority>
		public IEnumerator<T> GetEnumerator() {
			return new DataViewEnumerator<T>(this);
		}

		/// <summary>
		/// Gets or sets the data at the requested co-ordinates.
		/// </summary>
		/// <remarks>
		/// For reading/writing single elements, using the this member is recommended, but may be slow when attempting
		/// to copy large sections of the data. In these circumstances, consider using something like
		/// <see cref="UnsafeUtils.CopyGenericArray{T}(Ophidian.Losgap.ArraySlice{T},System.IntPtr,uint)"/> / 
		/// <see cref="UnsafeUtils.CopyGenericArray{T}(System.IntPtr,Ophidian.Losgap.ArraySlice{T},uint)"/> in combination with the
		/// <see cref="Data"/> member.
		/// </remarks>
		/// <param name="u">The u-coordinate to copy.</param>
		/// <returns>A copy of the data at the requested co-ordinate.</returns>
		public T this[uint u] {
			get {
				Assure.LessThan(u, Width, "Index out of bounds: u");
				return UnsafeUtils.ReadGenericFromPtr<T>(Data + (int) (sizeOfT * u), sizeOfT);
			}
			set {
				Assure.LessThan(u, Width, "Index out of bounds: u");
				UnsafeUtils.WriteGenericToPtr(Data + (int) (sizeOfT * u), value, sizeOfT);
			}
		}
		/// <summary>
		/// Gets or sets the data at the requested co-ordinates.
		/// </summary>
		/// <remarks>
		/// For reading/writing single elements, using this member is recommended, but may be slow when attempting
		/// to copy large sections of the data. In these circumstances, consider using something like
		/// <see cref="UnsafeUtils.CopyGenericArray{T}(Ophidian.Losgap.ArraySlice{T},System.IntPtr,uint)"/> / 
		/// <see cref="UnsafeUtils.CopyGenericArray{T}(System.IntPtr,Ophidian.Losgap.ArraySlice{T},uint)"/> in combination with the
		/// <see cref="Data"/> member.
		/// </remarks>
		/// <param name="u">The u-coordinate to copy.</param>
		/// <returns>A copy of the data at the requested co-ordinate.</returns>
		public T this[int u] {
			get {
				return this[(uint) u];
			}
			set {
				this[(uint) u] = value;
			}
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(RawResourceDataView1D<T> other) {
			return Data == other.Data && Width == other.Width && sizeOfT == other.sizeOfT;
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
			return obj is RawResourceDataView1D<T> && Equals((RawResourceDataView1D<T>)obj);
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
				int hashCode = Data.GetHashCode();
				hashCode = (hashCode * 397) ^ (int) Width;
				hashCode = (hashCode * 397) ^ (int) sizeOfT;
				return hashCode;
			}
		}

		/// <summary>
		/// Indicates whether or not the two operands refer to the same resource, with the same interpretation.
		/// </summary>
		/// <param name="left">The first operand.</param>
		/// <param name="right">The second operand.</param>
		/// <returns>True if the two data views are equal.</returns>
		public static bool operator ==(RawResourceDataView1D<T> left, RawResourceDataView1D<T> right) {
			return left.Equals(right);
		}

		/// <summary>
		/// Indicates whether or not the two operands refer to the same resource, with the same interpretation.
		/// </summary>
		/// <param name="left">The first operand.</param>
		/// <param name="right">The second operand.</param>
		/// <returns>True if the two data views are not equal.</returns>
		public static bool operator !=(RawResourceDataView1D<T> left, RawResourceDataView1D<T> right) {
			return !left.Equals(right);
		}
	}

	/// <summary>
	/// Provides an encapsulation over a pointer to mapped 2D resource data.
	/// </summary>
	/// <typeparam name="T">The data type. Must be <see cref="Extensions.IsBlittable">blittable</see>.</typeparam>
	public struct RawResourceDataView2D<T> : IEnumerable<T>, IEquatable<RawResourceDataView2D<T>>
		where T : struct {
		/// <summary>
		/// A pointer to the start of the mapped data.
		/// </summary>
		public readonly IntPtr Data;
		/// <summary>
		/// The number of elements in the u-direction that are represented by this resource view.
		/// </summary>
		public readonly uint Width;
		/// <summary>
		/// The number of elements in the v-direction that are represented by this resource view.
		/// </summary>
		public readonly uint Height;
		private readonly uint sizeOfT;
		private readonly uint rowStrideBytes;

		private class DataViewEnumerator<TEnum> : IEnumerator<TEnum> where TEnum : struct {
			private RawResourceDataView2D<TEnum> parent;
			private int currentIndexU, currentIndexV;

			public DataViewEnumerator(RawResourceDataView2D<TEnum> parent) {
				this.parent = parent;
				Reset();
			}

			public void Dispose() {
				// Do nothing
			}

			public bool MoveNext() {
				++currentIndexU;
				if (currentIndexU >= parent.Width) {
					currentIndexU = 0;
					++currentIndexV;
				}
				return currentIndexV < parent.Height;
			}

			public void Reset() {
				currentIndexU = -1;
				currentIndexV = 0;
			}

			object IEnumerator.Current {
				get { return Current; }
			}

			public TEnum Current {
				get {
					return parent[currentIndexU, currentIndexV];
				}
			}
		}



		/// <summary>
		/// Creates a new raw resource data view.
		/// </summary>
		/// <param name="data">A pointer to the start of the mapped data.</param>
		/// <param name="sizeOfT">sizeof(<typeparamref name="T"/>).</param>
		/// <param name="width">The number of elements in the u-direction that are represented by this resource view.</param>
		/// <param name="height">The number of elements in the v-direction that are represented by this resource view.</param>
		/// <param name="rowStrideBytes">The number of bytes in the data between the start of two consecutive rows.</param>
		public RawResourceDataView2D(IntPtr data, uint sizeOfT, uint width, uint height, uint rowStrideBytes) {
			Assure.True(typeof(T).IsBlittable());
			Data = data;
			Height = height;
			Width = width;
			this.rowStrideBytes = rowStrideBytes;
			this.sizeOfT = sizeOfT;
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
		/// </returns>
		/// <filterpriority>1</filterpriority>
		public IEnumerator<T> GetEnumerator() {
			return new DataViewEnumerator<T>(this);
		}

		/// <summary>
		/// Gets or sets the data at the requested co-ordinates.
		/// </summary>
		/// <remarks>
		/// For reading/writing single elements, using this member is recommended, but may be slow when attempting
		/// to copy large sections of the data. In these circumstances, consider using something like
		/// <see cref="UnsafeUtils.CopyGenericArray{T}(Ophidian.Losgap.ArraySlice{T},System.IntPtr,uint)"/> / 
		/// <see cref="UnsafeUtils.CopyGenericArray{T}(System.IntPtr,Ophidian.Losgap.ArraySlice{T},uint)"/> in combination with the
		/// <see cref="Data"/> member.
		/// </remarks>
		/// <param name="u">The u-coordinate to copy.</param>
		/// <param name="v">The v-coordinate to copy.</param>
		/// <returns>A copy of the data at the requested co-ordinate.</returns>
		public T this[uint u, uint v] {
			get {
				Assure.LessThan(u, Width, "Index out of bounds: u");
				Assure.LessThan(v, Height, "Index out of bounds: v");
				return UnsafeUtils.ReadGenericFromPtr<T>(Data + (int) (u * sizeOfT + v * rowStrideBytes), sizeOfT);
			}
			set {
				Assure.LessThan(u, Width, "Index out of bounds: u");
				Assure.LessThan(v, Height, "Index out of bounds: v");
				UnsafeUtils.WriteGenericToPtr(Data + (int) (u * sizeOfT + v * rowStrideBytes), value, sizeOfT);
			}
		}
		/// <summary>
		/// Gets or sets the data at the requested co-ordinates.
		/// </summary>
		/// <remarks>
		/// For reading/writing single elements, using this member is recommended, but may be slow when attempting
		/// to copy large sections of the data. In these circumstances, consider using something like
		/// <see cref="UnsafeUtils.CopyGenericArray{T}(Ophidian.Losgap.ArraySlice{T},System.IntPtr,uint)"/> / 
		/// <see cref="UnsafeUtils.CopyGenericArray{T}(System.IntPtr,Ophidian.Losgap.ArraySlice{T},uint)"/> in combination with the
		/// <see cref="Data"/> member.
		/// </remarks>
		/// <param name="u">The u-coordinate to copy.</param>
		/// <param name="v">The v-coordinate to copy.</param>
		/// <returns>A copy of the data at the requested co-ordinate.</returns>
		public T this[int u, int v] {
			get {
				return this[(uint) u, (uint) v];
			}
			set {
				this[(uint) u, (uint) v] = value;
			}
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(RawResourceDataView2D<T> other) {
			return Data == other.Data && Width == other.Width && Height == other.Height && sizeOfT == other.sizeOfT && rowStrideBytes == other.rowStrideBytes;
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
			return obj is RawResourceDataView2D<T> && Equals((RawResourceDataView2D<T>)obj);
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
				int hashCode = Data.GetHashCode();
				hashCode = (hashCode * 397) ^ (int) Width;
				hashCode = (hashCode * 397) ^ (int) Height;
				hashCode = (hashCode * 397) ^ (int) sizeOfT;
				hashCode = (hashCode * 397) ^ (int) rowStrideBytes;
				return hashCode;
			}
		}

		/// <summary>
		/// Indicates whether or not the two operands refer to the same resource, with the same interpretation.
		/// </summary>
		/// <param name="left">The first operand.</param>
		/// <param name="right">The second operand.</param>
		/// <returns>True if the two data views are equal.</returns>
		public static bool operator ==(RawResourceDataView2D<T> left, RawResourceDataView2D<T> right) {
			return left.Equals(right);
		}

		/// <summary>
		/// Indicates whether or not the two operands refer to the same resource, with the same interpretation.
		/// </summary>
		/// <param name="left">The first operand.</param>
		/// <param name="right">The second operand.</param>
		/// <returns>True if the two data views are not equal.</returns>
		public static bool operator !=(RawResourceDataView2D<T> left, RawResourceDataView2D<T> right) {
			return !left.Equals(right);
		}
	}

	/// <summary>
	/// Provides an encapsulation over a pointer to mapped 3D resource data.
	/// </summary>
	/// <typeparam name="T">The data type. Must be <see cref="Extensions.IsBlittable">blittable</see>.</typeparam>
	public struct RawResourceDataView3D<T> : IEnumerable<T>, IEquatable<RawResourceDataView3D<T>>
		where T : struct {
		/// <summary>
		/// A pointer to the start of the mapped data.
		/// </summary>
		public readonly IntPtr Data;
		/// <summary>
		/// The number of elements in the u-direction that are represented by this resource view.
		/// </summary>
		public readonly uint Width;
		/// <summary>
		/// The number of elements in the v-direction that are represented by this resource view.
		/// </summary>
		public readonly uint Height;
		/// <summary>
		/// The number of elements in the w-direction that are represented by this resource view.
		/// </summary>
		public readonly uint Depth;
		private readonly uint sizeOfT;
		private readonly uint rowStrideBytes;
		private readonly uint sliceStrideBytes;

		private class DataViewEnumerator<TEnum> : IEnumerator<TEnum> where TEnum : struct {
			private RawResourceDataView3D<TEnum> parent;
			private int currentIndexU, currentIndexV, currentIndexW;

			public DataViewEnumerator(RawResourceDataView3D<TEnum> parent) {
				this.parent = parent;
				Reset();
			}

			public void Dispose() {
				// Do nothing
			}

			public bool MoveNext() {
				++currentIndexU;
				if (currentIndexU >= parent.Width) {
					currentIndexU = 0;
					++currentIndexV;
				}
				if (currentIndexV >= parent.Height) {
					currentIndexV = 0;
					++currentIndexW;
				}
				return currentIndexW < parent.Depth;
			}

			public void Reset() {
				currentIndexU = -1;
				currentIndexV = 0;
				currentIndexW = 0;
			}

			object IEnumerator.Current {
				get { return Current; }
			}

			public TEnum Current {
				get {
					return parent[currentIndexU, currentIndexV, currentIndexW];
				}
			}
		}



		/// <summary>
		/// Creates a new raw resource data view.
		/// </summary>
		/// <param name="data">A pointer to the start of the mapped data.</param>
		/// <param name="sizeOfT">sizeof(<typeparamref name="T"/>).</param>
		/// <param name="width">The number of elements in the u-direction that are represented by this resource view.</param>
		/// <param name="height">The number of elements in the v-direction that are represented by this resource view.</param>
		/// <param name="depth">The number of elements in the w-direction that are represented by this resource view.</param>
		/// <param name="rowStrideBytes">The number of bytes in the data between the start of two consecutive rows.</param>
		/// <param name="sliceStrideBytes">The number of bytes in the data between the start of two consecutive slices.</param>
		public RawResourceDataView3D(IntPtr data, uint sizeOfT, uint width, uint height, uint depth, 
			uint rowStrideBytes, uint sliceStrideBytes) {
			Assure.True(typeof(T).IsBlittable());
			Data = data;
			Height = height;
			Width = width;
			Depth = depth;
			this.rowStrideBytes = rowStrideBytes;
			this.sliceStrideBytes = sliceStrideBytes;
			this.sizeOfT = sizeOfT;
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
		/// </returns>
		/// <filterpriority>1</filterpriority>
		public IEnumerator<T> GetEnumerator() {
			return new DataViewEnumerator<T>(this);
		}

		/// <summary>
		/// Gets or sets the data at the requested co-ordinates.
		/// </summary>
		/// <remarks>
		/// For reading/writing single elements, using this member is recommended, but may be slow when attempting
		/// to copy large sections of the data. In these circumstances, consider using something like
		/// <see cref="UnsafeUtils.CopyGenericArray{T}(Ophidian.Losgap.ArraySlice{T},System.IntPtr,uint)"/> / 
		/// <see cref="UnsafeUtils.CopyGenericArray{T}(System.IntPtr,Ophidian.Losgap.ArraySlice{T},uint)"/> in combination with the
		/// <see cref="Data"/> member.
		/// </remarks>
		/// <param name="u">The u-coordinate to copy.</param>
		/// <param name="v">The v-coordinate to copy.</param>
		/// <param name="w">The w-coordinate to copy.</param>
		/// <returns>A copy of the data at the requested co-ordinate.</returns>
		public T this[uint u, uint v, uint w] {
			get {
				Assure.LessThan(u, Width, "Index out of bounds: u");
				Assure.LessThan(v, Height, "Index out of bounds: v");
				Assure.LessThan(w, Depth, "Index out of bounds: w");
				return UnsafeUtils.ReadGenericFromPtr<T>(Data + (int) (u * sizeOfT + v * rowStrideBytes + w * sliceStrideBytes), sizeOfT);
			}
			set {
				Assure.LessThan(u, Width, "Index out of bounds: u");
				Assure.LessThan(v, Height, "Index out of bounds: v");
				Assure.LessThan(w, Depth, "Index out of bounds: w");
				UnsafeUtils.WriteGenericToPtr(Data + (int) (u * sizeOfT + v * rowStrideBytes + w * sliceStrideBytes), value, sizeOfT);
			}
		}

		/// <summary>
		/// Gets or sets the data at the requested co-ordinates.
		/// </summary>
		/// <remarks>
		/// For reading/writing single elements, using this member is recommended, but may be slow when attempting
		/// to copy large sections of the data. In these circumstances, consider using something like
		/// <see cref="UnsafeUtils.CopyGenericArray{T}(Ophidian.Losgap.ArraySlice{T},System.IntPtr,uint)"/> / 
		/// <see cref="UnsafeUtils.CopyGenericArray{T}(System.IntPtr,Ophidian.Losgap.ArraySlice{T},uint)"/> in combination with the
		/// <see cref="Data"/> member.
		/// </remarks>
		/// <param name="u">The u-coordinate to copy.</param>
		/// <param name="v">The v-coordinate to copy.</param>
		/// <param name="w">The w-coordinate to copy.</param>
		/// <returns>A copy of the data at the requested co-ordinate.</returns>
		public T this[int u, int v, int w] {
			get {
				return this[(uint) u, (uint) v, (uint) w];
			}
			set {
				this[(uint) u, (uint) v, (uint) w] = value;
			}
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(RawResourceDataView3D<T> other) {
			return Data == other.Data && Width == other.Width && Height == other.Height && Depth == other.Depth && sizeOfT == other.sizeOfT && rowStrideBytes == other.rowStrideBytes && sliceStrideBytes == other.sliceStrideBytes;
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
			return obj is RawResourceDataView3D<T> && Equals((RawResourceDataView3D<T>)obj);
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
				int hashCode = Data.GetHashCode();
				hashCode = (hashCode * 397) ^ (int) Width;
				hashCode = (hashCode * 397) ^ (int) Height;
				hashCode = (hashCode * 397) ^ (int) Depth;
				hashCode = (hashCode * 397) ^ (int) sizeOfT;
				hashCode = (hashCode * 397) ^ (int) rowStrideBytes;
				hashCode = (hashCode * 397) ^ (int) sliceStrideBytes;
				return hashCode;
			}
		}

		/// <summary>
		/// Indicates whether or not the two operands refer to the same resource, with the same interpretation.
		/// </summary>
		/// <param name="left">The first operand.</param>
		/// <param name="right">The second operand.</param>
		/// <returns>True if the two data views are equal.</returns>
		public static bool operator ==(RawResourceDataView3D<T> left, RawResourceDataView3D<T> right) {
			return left.Equals(right);
		}

		/// <summary>
		/// Indicates whether or not the two operands refer to the same resource, with the same interpretation.
		/// </summary>
		/// <param name="left">The first operand.</param>
		/// <param name="right">The second operand.</param>
		/// <returns>True if the two data views are not equal.</returns>
		public static bool operator !=(RawResourceDataView3D<T> left, RawResourceDataView3D<T> right) {
			return !left.Equals(right);
		}
	}
}