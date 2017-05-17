// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 14 10 2014 at 16:16 by Ben Bowen

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ophidian.Losgap {
	/// <summary>
	/// A static class containing unsafe utilities that provide extra assistance when working with, amongst other things,
	/// pointers and generics.
	/// </summary>
	/// <remarks>
	/// This class is necessarily unsafe; which means that incorrect usage could result in your application crashing for very difficult-to-find
	/// reasons. Therefore, you should only use it when you're sure you know what you're doing.
	/// </remarks>
	public static unsafe class UnsafeUtils {
		/// <summary>
		/// A runtime equivalent of C#'s sizeof() operator. Returns the width, in bytes, of any given struct as it is used by the CLR.
		/// </summary>
		/// <typeparam name="T">The type whose size it is you wish to get.</typeparam>
		/// <returns>The size, in bytes, of objects of type <typeparamref name="T"/>.</returns>
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
			Justification = "Type parameter is required for the purpose of this method.")]
		public static int SizeOf<T>() where T : struct {
			Type type = typeof(T);
			
			TypeCode typeCode = Type.GetTypeCode(type);
			switch (typeCode) {
				case TypeCode.Boolean:
					return sizeof(bool);
				case TypeCode.Char:
					return sizeof(char);
				case TypeCode.SByte:
					return sizeof(sbyte);
				case TypeCode.Byte:
					return sizeof(byte);
				case TypeCode.Int16:
					return sizeof(short);
				case TypeCode.UInt16:
					return sizeof(ushort);
				case TypeCode.Int32:
					return sizeof(int);
				case TypeCode.UInt32:
					return sizeof(uint);
				case TypeCode.Int64:
					return sizeof(long);
				case TypeCode.UInt64:
					return sizeof(ulong);
				case TypeCode.Single:
					return sizeof(float);
				case TypeCode.Double:
					return sizeof(double);
				case TypeCode.Decimal:
					return sizeof(decimal);
				case TypeCode.DateTime:
					return sizeof(DateTime);
				default:
					// The idea here is to measure the difference between the pointers to two contiguous values of type T.
					T[] tArray = new T[2];
					GCHandle tArrayPinned = GCHandle.Alloc(tArray, GCHandleType.Pinned);
					try {
						TypedReference tRef0 = __makeref(tArray[0]);
						TypedReference tRef1 = __makeref(tArray[1]);
						IntPtr ptrToT0 = *((IntPtr*)&tRef0);
						IntPtr ptrToT1 = *((IntPtr*)&tRef1);

						return (int)(((byte*)ptrToT1) - ((byte*)ptrToT0));
					}
					finally {
						tArrayPinned.Free();
					}
			}
		}

		/// <summary>
		/// Reinterpret <paramref name="curValue"/> as an object of type <typeparamref name="TOut"/>. This function works in a similar way to
		/// C++'s reinterpret_cast.
		/// </summary>
		/// <remarks>
		/// You must take care to ensure that the <see cref="SizeOf{T}">size of</see> <typeparamref name="TOut"/> is the same as the size of
		/// <typeparamref name="TIn"/>; or you will corrupt memory.
		/// </remarks>
		/// <typeparam name="TIn">The input value type (e.g. the type of <paramref name="curValue"/>).</typeparam>
		/// <typeparam name="TOut">The output value type. An object of this type will be returned.</typeparam>
		/// <param name="curValue">The value to reinterpret.</param>
		/// <param name="sizeBytes">The size, in bytes, of <typeparamref name="TIn"/> or <typeparamref name="TOut"/>. Use
		/// <see cref="SizeOf{T}"/> if the size can not be determined before runtime.</param>
		/// <returns>A new <typeparamref name="TOut"/> that is a reinterpretation of <paramref name="curValue"/>.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TOut Reinterpret<TIn, TOut>(TIn curValue, int sizeBytes)
			where TIn : struct
			where TOut : struct 
		{
			Assure.Equal(SizeOf<TIn>(), SizeOf<TOut>(), "TOut must be a type of identical byte size to TIn.");
			Assure.Equal(
				sizeBytes, 
				SizeOf<TIn>(), 
				"Provided sizeBytes value (" + sizeBytes + ") is incorrect (sizeof(TIn) == " + SizeOf<TIn>() + ")"
			);
			Assure.True(typeof(TIn).IsBlittable(), "TIn type is not blittable.");
			Assure.True(typeof(TOut).IsBlittable(), "TOut type is not blittable.");

			TOut result = default(TOut);

			/* What I'm doing is using TypedReference as { IntPtr, IntPtr }; 
			 * where the first IntPtr is a pointer to the __makeref referenced value type */
			TypedReference resultRef = __makeref(result);
			byte* resultPtr = (byte*) *((IntPtr*) &resultRef);

			// Now do the same with curValue
			TypedReference curValueRef = __makeref(curValue);
			byte* curValuePtr = (byte*) *((IntPtr*) &curValueRef);

			// Finally, copy
			for (int i = 0; i < sizeBytes; ++i) {
				resultPtr[i] = curValuePtr[i];
			}

			return result;
		}

		/// <summary>
		/// Writes (blits) <paramref name="value"/> to the address pointed to by <paramref name="dest"/>.
		/// </summary>
		/// <remarks>
		/// This method is about a (decimal) order-of-magnitude slower than a standard copy-to-ptr operation, so it should be used
		/// sparingly, and only when the type of <typeparamref name="T"/> is not known.
		/// </remarks>
		/// <typeparam name="T">The type of <paramref name="value"/>.</typeparam>
		/// <param name="dest">The address to copy <paramref name="value"/> to.</param>
		/// <param name="value">The value to copy to <paramref name="dest"/>.</param>
		/// <param name="sizeOfT">The size of <paramref name="value"/> in bytes.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void WriteGenericToPtr<T>(IntPtr dest, T value, uint sizeOfT) where T : struct {
			Assure.False(dest == IntPtr.Zero);
			Assure.True(typeof(T).IsBlittable());
			Assure.Equal(sizeOfT, (uint) SizeOf<T>());
			byte* bytePtr = (byte*) dest;
			TypedReference valueref = __makeref(value);
			byte* valuePtr = (byte*) *((IntPtr*) &valueref);

			for (uint i = 0U; i < sizeOfT; ++i) {
				bytePtr[i] = valuePtr[i];
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void WriteGenericToPtr<T>(IntPtr dest, T value, int sizeOfT) where T : struct {
			Assure.False(dest == IntPtr.Zero);
			Assure.True(typeof(T).IsBlittable());
			Assure.Equal(sizeOfT, (uint) SizeOf<T>());
			byte* bytePtr = (byte*) dest;
			TypedReference valueref = __makeref(value);
			byte* valuePtr = (byte*) *((IntPtr*) &valueref);

			for (uint i = 0U; i < sizeOfT; ++i) {
				bytePtr[i] = valuePtr[i];
			}
		}

		/// <summary>
		/// Reads (blits) the value from the address pointed to by <paramref name="source"/> in to a new <typeparamref name="T"/>, and returns it.
		/// </summary>
		/// <typeparam name="T">The type of object at the address represented by <paramref name="source"/>.</typeparam>
		/// <param name="source">The address to copy the data from.</param>
		/// <param name="sizeOfT">The size of <typeparamref name="T"/> in bytes.</param>
		/// <returns>A new object of type <typeparamref name="T"/> that is the equivalent of the
		/// value pointed to by <paramref name="source"/>.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T ReadGenericFromPtr<T>(IntPtr source, uint sizeOfT) where T : struct {
			Assure.False(source == IntPtr.Zero);
			Assure.True(typeof(T).IsBlittable());
			Assure.Equal(sizeOfT, (uint) SizeOf<T>());
			byte* bytePtr = (byte*) source;
			T result = default(T);

			TypedReference resultRef = __makeref(result);
			byte* resultPtr = (byte*) *((IntPtr*) &resultRef);

			for (uint i = 0U; i < sizeOfT; ++i) {
				resultPtr[i] = bytePtr[i];
			}

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T ReadGenericFromPtr<T>(IntPtr source, int sizeOfT) where T : struct {
			Assure.False(source == IntPtr.Zero);
			Assure.True(typeof(T).IsBlittable());
			Assure.Equal(sizeOfT, (uint) SizeOf<T>());
			byte* bytePtr = (byte*) source;
			T result = default(T);

			TypedReference resultRef = __makeref(result);
			byte* resultPtr = (byte*) *((IntPtr*) &resultRef);

			for (uint i = 0U; i < sizeOfT; ++i) {
				resultPtr[i] = bytePtr[i];
			}

			return result;
		}

		/// <summary>
		/// Copies the segment of an array specified by the given <see cref="ArraySlice{T}"/> to the <paramref name="destination"/>.
		/// </summary>
		/// <typeparam name="T">The array element type. Must be a <see cref="Extensions.IsBlittable">blittable</see> struct.</typeparam>
		/// <param name="source">The source data to copy. <see cref="ArraySlice{T}.Length"/> elements will be copied, from the array slice
		/// <see cref="ArraySlice{T}.Offset"/>.</param>
		/// <param name="destination">The target pointer to copy the data to. Must not be <see cref="IntPtr.Zero"/>.</param>
		/// <param name="arrayElementSizeBytes">The size of <typeparamref name="T"/>. Use either sizeof() (preferred), or 
		/// <see cref="SizeOf{T}"/> if the type is not known at compile-time.</param>
		public static unsafe void CopyGenericArray<T>(ArraySlice<T> source, IntPtr destination, uint arrayElementSizeBytes) where T : struct {
			Assure.False(destination == IntPtr.Zero);
			Assure.True(typeof(T).IsBlittable());
			Assure.Equal((int) arrayElementSizeBytes, SizeOf<T>());
			GCHandle pinnedArrayHandle = GCHandle.Alloc(source.ContainingArray, GCHandleType.Pinned);
			try {
				MemCopy(pinnedArrayHandle.AddrOfPinnedObject() + (int) (arrayElementSizeBytes * source.Offset),
					destination, source.Length * arrayElementSizeBytes);
			}
			finally {
				pinnedArrayHandle.Free();
			}
		}

		/// <summary>
		/// Copies data from the <paramref name="source"/> pointer to the <paramref name="destination"/> array.
		/// </summary>
		/// <typeparam name="T">The array element type. Must be a <see cref="Extensions.IsBlittable">blittable</see> struct.</typeparam>
		/// <param name="source">The pointer to the source data. Must not be <see cref="IntPtr.Zero"/>.</param>
		/// <param name="destination">The destination array to which the data will be copied. This method will copy
		/// <see cref="ArraySlice{T}.Length"/> elements' worth of data, starting at the <see cref="ArraySlice{T}.Offset"/>.</param>
		/// <param name="arrayElementSizeBytes">The size of <typeparamref name="T"/>. Use either sizeof() (preferred), or 
		/// <see cref="SizeOf{T}"/> if the type is not known at compile-time.</param>
		public static unsafe void CopyGenericArray<T>(IntPtr source, ArraySlice<T> destination, uint arrayElementSizeBytes) where T : struct {
			Assure.False(source == IntPtr.Zero);
			Assure.True(typeof(T).IsBlittable());
			Assure.Equal((int) arrayElementSizeBytes, SizeOf<T>());
			GCHandle pinnedArrayHandle = GCHandle.Alloc(destination.ContainingArray, GCHandleType.Pinned);
			try {
				MemCopy(source,
					pinnedArrayHandle.AddrOfPinnedObject() + (int) (arrayElementSizeBytes * destination.Offset), destination.Length * arrayElementSizeBytes);
			}
			finally {
				pinnedArrayHandle.Free();
			}
		}

		/// <summary>
		/// Uses the MS-Visual-C-Runtime <c>memcpy</c> to copy <paramref name="numBytes"/> bytes
		/// from <paramref name="source"/> to <paramref name="destination"/>.
		/// </summary>
		/// <remarks>
		/// This method is faster than a C# byte-by-byte copy for operations exceeding a certain number of bytes. The number changes 
		/// from host to host, but generally any copies of 128 bytes or more could be considered good candidates for using this method.
		/// </remarks>
		/// <param name="source">The source pointer. Must not be <see cref="IntPtr.Zero"/>.</param>
		/// <param name="destination">The destination pointer. Must not be <see cref="IntPtr.Zero"/>.</param>
		/// <param name="numBytes">The number of bytes to copy from <paramref name="source"/>.</param>
		public static void MemCopy(IntPtr source, IntPtr destination, uint numBytes) {
			Memcpy(destination, source, new UIntPtr(numBytes));
		}

		/// <summary>
		/// Fills the memory at the given <paramref name="dest"/> address with <paramref name="numBytes"/> zeroes.
		/// </summary>
		/// <param name="dest">The destination address. Must not be <see cref="IntPtr.Zero"/>.</param>
		/// <param name="numBytes">The number of bytes from <paramref name="dest"/> to fill with zeroes.</param>
		public static void ZeroMem(IntPtr dest, uint numBytes) {
			Assure.NotEqual(dest, IntPtr.Zero, "Destination address must not be Zero.");
			RtlZeroMemory(dest, new IntPtr(numBytes));
		}

		[SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass",
			Justification = "It's unnecessary to create another static class for a couple of methods, I think it's nicer to just leave it here."), 
		DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
		private static extern IntPtr Memcpy(IntPtr destination, IntPtr source, UIntPtr count);

		[SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass",
			Justification = "It's unnecessary to create another static class for a couple of methods, I think it's nicer to just leave it here."), 
		DllImport("Kernel32.dll", EntryPoint = "RtlZeroMemory", CallingConvention = CallingConvention.StdCall, SetLastError = false)]
		private static extern void RtlZeroMemory(IntPtr dest, IntPtr size);
	}
}