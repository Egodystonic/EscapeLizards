// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 18 12 2014 at 14:49 by Ben Bowen

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace Ophidian.Losgap.Interop {
	/// <summary>
	/// Represents a memory-aligned structure allocated in unmanaged memory.
	/// </summary>
	/// <typeparam name="T">The structure type that has been allocated in unmanaged memory.</typeparam>
	public struct AlignedAllocation<T> : IDisposable, IEquatable<AlignedAllocation<T>>
		where T : struct {
		/// <summary>
		/// The raw pointer to the structure.
		/// </summary>
		public readonly IntPtr AlignedPointer;
		/// <summary>
		/// The start of the block of memory allocated (aligned allocations over-allocate, the actual aligned pointer is
		/// referenced by the <see cref="AlignedPointer"/> field).
		/// </summary>
		public readonly IntPtr ReservedMemory;
		private readonly uint sizeOfT;

		/// <summary>
		/// Creates a new aligned <typeparamref name="T"/> with the given <paramref name="alignment"/> in the unmanaged heap.
		/// </summary>
		/// <param name="alignment">The alignment. The allocated pointer's address will be divisible by this value.
		/// Must be a positive value.</param>
		public AlignedAllocation(long alignment) : this(alignment, (uint) UnsafeUtils.SizeOf<T>()) { }

		public AlignedAllocation(long alignment, uint sizeOfT) {
			Assure.GreaterThan(alignment, 0L, "Alignment must be positive.");

			this.sizeOfT = sizeOfT;

			ReservedMemory = Marshal.AllocHGlobal(new IntPtr(sizeOfT + alignment - 1));
			long allocationOffset = (long) ReservedMemory % alignment;
			if (allocationOffset == 0L) AlignedPointer = ReservedMemory;
			else AlignedPointer = ReservedMemory + (int) (alignment - allocationOffset);
		}

		private AlignedAllocation(IntPtr alignedPointer, uint sizeOfT, IntPtr reservedMemory) {
			AlignedPointer = alignedPointer;
			this.sizeOfT = sizeOfT;
			this.ReservedMemory = reservedMemory;
		}

		/// <summary>
		/// Allocates an array of <typeparamref name="T"/>, starting at an aligned address, and returns an <see cref="AlignedAllocation{T}"/>
		/// struct that represents the first element in the array.
		/// </summary>
		/// <param name="alignment">The alignment of the first element in the array. Memory will be allocated for <paramref name="arrLen"/>
		/// members in contiguous memory from the aligned start address.</param>
		/// <param name="arrLen">The number of elements to allocate for. Must be greater than 0.</param>
		/// <returns>A new <see cref="AlignedAllocation{T}"/> whose <see cref="AlignedPointer"/> points to the start of 
		/// an array of <paramref name="arrLen"/> elements of type <typeparamref name="T"/>. Dispose the returned allocation
		/// to release the array.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes",
			Justification = "In this instance the type parameter makes sense.")]
		public static AlignedAllocation<T> AllocArray(long alignment, uint arrLen) {
			Assure.GreaterThan(alignment, 0L, "Alignment must be positive.");

			uint sizeOfT = (uint) UnsafeUtils.SizeOf<T>();
			uint reservationSize = sizeOfT * arrLen;

			IntPtr allocStart = Marshal.AllocHGlobal(new IntPtr(reservationSize + alignment - 1));
			long allocationOffset = (long) allocStart % alignment;

			IntPtr alignedStart;
			if (allocationOffset == 0L) alignedStart = allocStart;
			else alignedStart = allocStart + (int) (alignment - allocationOffset);

			return new AlignedAllocation<T>(alignedStart, sizeOfT, allocStart);
		}

		/// <summary>
		/// Dispose this allocation (and free the unmanaged pointer).
		/// </summary>
		public void Dispose() {
			Marshal.FreeHGlobal(ReservedMemory); // FreeHGlobal does nothing if ReservedMemory is null
		}

		/// <summary>
		/// Read the current value from the <see cref="AlignedPointer"/>.
		/// </summary>
		/// <returns>The value currently stored in this allocation.</returns>
		public T Read() {
			Assure.NotEqual(AlignedPointer, IntPtr.Zero);
			return UnsafeUtils.ReadGenericFromPtr<T>(AlignedPointer, sizeOfT);
		}

		/// <summary>
		/// Write a new value to the <see cref="AlignedPointer"/>.
		/// </summary>
		/// <param name="value">The new value to store in this allocation.</param>
		public void Write(T value) {
			Assure.NotEqual(AlignedPointer, IntPtr.Zero);
			UnsafeUtils.WriteGenericToPtr(AlignedPointer, value, sizeOfT);
		}

		/// <summary>
		/// Returns a string representing the state/values of this instance.
		/// </summary>
		/// <returns>
		/// A new string with details of this instance.
		/// </returns>
		public override string ToString() {
			return "{0x" + ReservedMemory.ToString("X") + " aligned to 0x" + AlignedPointer.ToString("X") + "}";
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(AlignedAllocation<T> other) {
			return AlignedPointer == other.AlignedPointer;
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
			return obj is AlignedAllocation<T> && Equals((AlignedAllocation<T>)obj);
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>
		/// A 32-bit signed integer that is the hash code for this instance.
		/// </returns>
		/// <filterpriority>2</filterpriority>
		public override int GetHashCode() {
			return AlignedPointer.GetHashCode();
		}

		/// <summary>
		/// Determines whether or not the two allocations point to exactly the same <typeparamref name="T"/>.
		/// </summary>
		/// <param name="left">The first allocation.</param>
		/// <param name="right">The second allocation.</param>
		/// <returns>True if <paramref name="left"/> and <paramref name="right"/>'s <see cref="AlignedPointer"/> are
		/// identical, false if not.</returns>
		public static bool operator ==(AlignedAllocation<T> left, AlignedAllocation<T> right) {
			return left.Equals(right);
		}

		/// <summary>
		/// Determines whether or not the two allocations point to different <typeparamref name="T"/>s.
		/// </summary>
		/// <param name="left">The first allocation.</param>
		/// <param name="right">The second allocation.</param>
		/// <returns>True if <paramref name="left"/> and <paramref name="right"/>'s <see cref="AlignedPointer"/> are
		/// different, false if not.</returns>
		public static bool operator !=(AlignedAllocation<T> left, AlignedAllocation<T> right) {
			return !left.Equals(right);
		}

		/// <summary>
		/// Returns the value of the object pointed to by the <paramref name="operand"/>.
		/// </summary>
		/// <param name="operand">The aligned allocation to read. Must not be disposed.</param>
		/// <returns><c>operand.Read()</c>.</returns>
		public static implicit operator T(AlignedAllocation<T> operand) {
			return operand.Read();
		}
	}
}