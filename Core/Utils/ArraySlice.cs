// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 27 10 2014 at 15:50 by Ben Bowen

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Ophidian.Losgap {
	/// <summary>
	/// Encapsulates a segment of a given <see cref="ContainingArray"/>.
	/// </summary>
	/// <remarks>
	/// This class provides an alternative to the .NET <see cref="ArraySegment{T}"/> type with a variety of extra functionality.
	/// </remarks>
	/// <typeparam name="T">The type of element in the given <see cref="ContainingArray"/>.</typeparam>
	public struct ArraySlice<T> : IEnumerable<T>, IEquatable<ArraySlice<T>> {
		/// <summary>
		/// The referenced parent array that this object represents a slice of.
		/// </summary>
		public readonly T[] ContainingArray;
		/// <summary>
		/// The offset in to the <see cref="ContainingArray"/> that is the first element in this array slice.
		/// </summary>
		public readonly uint Offset;
		/// <summary>
		/// The number of elements (starting from <see cref="Offset"/> in this array slice.
		/// </summary>
		public readonly uint Length;

		/// <summary>
		/// Returns <c>true</c> if this array slice represents the entirety of the <see cref="ContainingArray"/> (e.g. 
		/// <see cref="Offset"/> is <c>0</c> and <see cref="Length"/> is <c>ContainingArray.Length</c>).
		/// </summary>
		public bool IsFullArraySlice {
			get {
				return Offset == 0 && Length == ContainingArray.Length;
			}
		}

		/// <summary>
		/// Creates a new array slice with the given <see cref="ContainingArray"/>; and represents the full array with no partial slice 
		/// (e.g. <see cref="IsFullArraySlice"/> will be true).
		/// </summary>
		/// <param name="containingArray">The array to represent. Must not be null.</param>
		public ArraySlice(T[] containingArray) : this(containingArray, 0, containingArray != null ? (uint) containingArray.Length : 0U) { }

		/// <summary>
		/// Creates a new array slice that refers to the given <paramref name="containingArray"/>, minus the first <paramref name="offset"/>
		/// elements.
		/// </summary>
		/// <param name="containingArray">The array to represent. Must not be null.</param>
		/// <param name="offset">The number of elements from the start of <paramref name="containingArray"/> to skip.</param>
		public ArraySlice(T[] containingArray, uint offset)
			: this(containingArray, offset, (containingArray != null ? checked((uint) containingArray.Length - offset) : 0U)) { }

		/// <summary>
		/// Creates a new array slice that refers to <paramref name="count"/> elements in the given <paramref name="containingArray"/>,
		/// starting from the element at index <paramref name="offset"/>.
		/// </summary>
		/// <param name="containingArray">The array to represent. Must not be null.</param>
		/// <param name="offset">The index of the first element in <paramref name="containingArray"/> to include in this slice.</param>
		/// <param name="count">The number of elements in <paramref name="containingArray"/> to include in this slice.</param>
		public ArraySlice(T[] containingArray, uint offset, uint count) {
			Assure.NotNull(containingArray);
			Assure.LessThanOrEqualTo(
				offset + count, 
				containingArray.Length, 
				"offset + count must be less than or equal to the containing array's length."
			);
			this.ContainingArray = containingArray;
			this.Offset = offset;
			this.Length = count;
		}

		/// <summary>
		/// Returns a new array of type <typeparamref name="T"/> that is a shallow copy of the data referred to by this array slice.
		/// </summary>
		/// <returns>A new array of length <see cref="Length"/>, containing <see cref="Length"/> elements from the 
		/// <see cref="ContainingArray"/>, starting at the element at index <see cref="Offset"/>.</returns>
		public unsafe T[] CopySliceToNewArray() {
			T[] result = new T[Length];
			Array.Copy(ContainingArray, Offset, result, 0, Length);
			return result;
		}

		public struct ArraySliceEnumerator<T> : IEnumerator<T> {
			private readonly ArraySlice<T> arrSlice;
			private uint curIndex;

			public ArraySliceEnumerator(ArraySlice<T> arrSlice) {
				this.arrSlice = arrSlice;
				curIndex = arrSlice.Offset - 1U;
			}

			public void Dispose() {
				// Do nothing
			}

			public bool MoveNext() {
				++curIndex;
				return curIndex < arrSlice.Length + arrSlice.Offset;
			}

			public void Reset() {
				curIndex = arrSlice.Offset - 1U;
			}

			object IEnumerator.Current {
				get { return Current; }
			}

			public T Current {
				get { return arrSlice[curIndex]; }
			}
		}

		/// <summary>
		/// Returns an enumerator that iterates through the elements referred to in the <see cref="ContainingArray"/> by this slice.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
		/// </returns>
		/// <filterpriority>1</filterpriority>
		IEnumerator<T> IEnumerable<T>.GetEnumerator() {
			return GetEnumerator();
		}

		public ArraySliceEnumerator<T> GetEnumerator() {
			return new ArraySliceEnumerator<T>(this);
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		/// <summary>
		/// Returns a string representing this array slice.
		/// </summary>
		/// <returns>
		/// A new string detailing the range and offset of the underlying array.
		/// </returns>
		public override string ToString() {
			return ContainingArray + " (Slice from index " + Offset + " of " + Length + " elements)";
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(ArraySlice<T> other) {
			return Equals(ContainingArray, other.ContainingArray) && Offset == other.Offset && Length == other.Length;
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
			return obj is ArraySlice<T> && Equals((ArraySlice<T>)obj);
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
				int hashCode = (ContainingArray != null ? ContainingArray.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (int)Offset;
				hashCode = (hashCode * 397) ^ (int)Length;
				return hashCode;
			}
		}

		/// <summary>
		/// Indicates whether the two slices refer to the same subsection of the same array.
		/// </summary>
		/// <param name="left">The first operand.</param>
		/// <param name="right">The second operand.</param>
		/// <returns>True if <see cref="ContainingArray"/>, <see cref="Offset"/>, and <see cref="Length"/> are the same in
		/// both operands.</returns>
		public static bool operator ==(ArraySlice<T> left, ArraySlice<T> right) {
			return left.Equals(right);
		}

		/// <summary>
		/// Indicates whether the two slices refer to the different subsections of the same array, or different arrays.
		/// </summary>
		/// <param name="left">The first operand.</param>
		/// <param name="right">The second operand.</param>
		/// <returns>False if <see cref="ContainingArray"/>, <see cref="Offset"/>, and <see cref="Length"/> are the same in
		/// both operands.</returns>
		public static bool operator !=(ArraySlice<T> left, ArraySlice<T> right) {
			return !left.Equals(right);
		}

		/// <summary>
		/// Gets or sets the element in the <see cref="ContainingArray"/> at <see cref="Offset"/> + <paramref name="index"/>.
		/// </summary>
		/// <param name="index">The element of this array slice to get or set.</param>
		/// <returns><c>ContainingArray[Offset + index]</c>.</returns>
		public T this[int index] {
			get {
				return ContainingArray[index + Offset];
			}
			set {
				ContainingArray[index + Offset] = value;
			}
		}
		/// <summary>
		/// Gets or sets the element in the <see cref="ContainingArray"/> at <see cref="Offset"/> + <paramref name="index"/>.
		/// </summary>
		/// <param name="index">The element of this array slice to get or set.</param>
		/// <returns><c>ContainingArray[Offset + index]</c>.</returns>
		public T this[uint index] {
			get {
				return ContainingArray[index + Offset];
			}
			set {
				ContainingArray[index + Offset] = value;
			}
		}

		/// <summary>
		/// Creates a new <see cref="ArraySlice{T}"/> that refers to the entire <paramref name="operand"/> array.
		/// </summary>
		/// <param name="operand">The target <see cref="ContainingArray"/>. Must not be null.</param>
		/// <returns><c>new ArraySlice&lt;T&gt;(operand)</c>.</returns>
		public static implicit operator ArraySlice<T>(T[] operand) {
			Assure.NotNull(operand);
			return new ArraySlice<T>(operand);
		}
	}
}