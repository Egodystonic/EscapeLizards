// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 09 10 2014 at 14:38 by Ben Bowen

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ophidian.Losgap.Interop {
	/// <summary>
	/// A static class containing constant values that used in managed/native interop.
	/// </summary>
	public static class InteropUtils {
		/// <summary>
		/// The calling convention (e.g. argument loading order, placement) used by exported native methods.
		/// </summary>
		public const CallingConvention DEFAULT_CALLING_CONVENTION = CallingConvention.Cdecl;

		/// <summary>
		/// The default charset used for strings in interop calls.
		/// </summary>
		public const CharSet DEFAULT_CHAR_SET = CharSet.Unicode;

		/// <summary>
		/// The maximum permitted length of the error message (not including null terminator) that a native operation writes.
		/// </summary>
		public const int MAX_INTEROP_FAIL_REASON_STRING_LENGTH = 200;

		/// <summary>
		/// The default marshalling applied to strings that are marshalled from managed space to unmanaged space.
		/// The actual definition in unmanaged space is a char16_t*, interpreted as a null-terminated Unicode 16-byte string.
		/// </summary>
		// Of course, it would be nicer to use UTF8, but C# is UTF16, and I don't fancy all that effort just for a 'good feeling'.
		public const UnmanagedType INTEROP_STRING_TYPE = UnmanagedType.LPWStr;

		/// <summary>
		/// An enumeration of possible memory layout packing sizes used by structs designed for native interop.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1027:MarkEnumsWithFlags",
			Justification = "Not a flags enum.")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue", 
			Justification = "A zero value for this enum would make no sense.")]
		public enum StructPacking {
			/// <summary>
			/// A packing amount that is guaranteed to produce the same memory layout on all architectures.
			/// </summary>
			Safe = 1,
			/// <summary>
			/// A packing amount that is chosen to provide the layout with the fastest theoretical memory access in the general case.
			/// </summary>
			Optimal = 8
		}

		#region CallNative Methods
		/// <summary>
		/// Calls a native (extern) method with the standard P/Invoke template, and no further arguments.
		/// </summary>
		/// <remarks>
		/// The standard P/Invoke template (with no additional arguments) is:
		/// <code>
		/// [DllImport(InteropUtils.NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
		/// 	EntryPoint = "EntryPoint")]
		/// private static extern InteropBool ReturnSuccess(
		/// 	IntPtr failReason
		/// );
		/// </code>
		/// </remarks>
		/// <param name="nativeFunc">The native func to call.</param>
		/// <returns>A <see cref="NativeCallResult"/> that encapsulates the success status of the call, 
		/// and the failure message (if any).</returns>
		[CompilerGenerated]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe NativeCallResult CallNative(Func<IntPtr, InteropBool> nativeFunc) {
			Assure.NotNull(nativeFunc);
			char* failReason = stackalloc char[MAX_INTEROP_FAIL_REASON_STRING_LENGTH + 1];
			bool success = nativeFunc((IntPtr) failReason);
			return new NativeCallResult(success, success ? null : Marshal.PtrToStringUni((IntPtr)failReason));
		}

		/// <summary>
		/// Calls a native (extern) method with the standard P/Invoke template, and one further argument.
		/// </summary>
		/// <remarks>
		/// The standard P/Invoke template (with one additional argument) is:
		/// <code>
		/// [DllImport(InteropUtils.NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
		/// 	EntryPoint = "EntryPoint")]
		/// private static extern InteropBool ReturnSuccess(
		/// 	IntPtr failReason,
		/// 	TArg1 arg1
		/// );
		/// </code>
		/// </remarks>
		/// <typeparam name="TArg1">The type of the first argument to the native method after <c>failReason</c>.</typeparam>
		/// <param name="nativeFunc">The native func to call.</param>
		/// <param name="arg1">The first additional argument value.</param>
		/// <returns>A <see cref="NativeCallResult"/> that encapsulates the success status of the call, 
		/// and the failure message (if any).</returns>
		[CompilerGenerated]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe NativeCallResult CallNative<TArg1>(Func<IntPtr, TArg1, InteropBool> nativeFunc, TArg1 arg1) {
			Assure.NotNull(nativeFunc);
			try {
				char* failReason = stackalloc char[MAX_INTEROP_FAIL_REASON_STRING_LENGTH + 1];
				bool success = nativeFunc((IntPtr) failReason, arg1);
				return new NativeCallResult(success, success ? null : Marshal.PtrToStringUni((IntPtr) failReason));
			}
			catch (Exception e) {
				throw new NativeOperationFailedException("Cross-language invocation failed.", e);
			}
		}

		/// <summary>
		/// Calls a native (extern) method with the standard P/Invoke template, and two further arguments.
		/// </summary>
		/// <remarks>
		/// The standard P/Invoke template (with two additional arguments) is:
		/// <code>
		/// [DllImport(InteropUtils.NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
		/// 	EntryPoint = "EntryPoint")]
		/// private static extern InteropBool ReturnSuccess(
		/// 	IntPtr failReason,
		/// 	TArg1 arg1,
		/// 	TArg2 arg2
		/// );
		/// </code>
		/// </remarks>
		/// <typeparam name="TArg1">The type of the first argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg2">The type of the second argument to the native method after <c>failReason</c>.</typeparam>
		/// <param name="nativeFunc">The native func to call.</param>
		/// <param name="arg1">The first additional argument value.</param>
		/// <param name="arg2">The second additional argument value.</param>
		/// <returns>A <see cref="NativeCallResult"/> that encapsulates the success status of the call, 
		/// and the failure message (if any).</returns>
		[CompilerGenerated]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe NativeCallResult CallNative<TArg1, TArg2>(Func<IntPtr, TArg1, TArg2, InteropBool> nativeFunc,
		TArg1 arg1, TArg2 arg2
		) {
			Assure.NotNull(nativeFunc);
			try {
				char* failReason = stackalloc char[MAX_INTEROP_FAIL_REASON_STRING_LENGTH + 1];
				bool success = nativeFunc((IntPtr) failReason, arg1, arg2);
				return new NativeCallResult(success, success ? null : Marshal.PtrToStringUni((IntPtr) failReason));
			}
			catch (Exception e) {
				throw new NativeOperationFailedException("Cross-language invocation failed.", e);
			}
		}

		/// <summary>
		/// Calls a native (extern) method with the standard P/Invoke template, and three further arguments.
		/// </summary>
		/// <remarks>
		/// The standard P/Invoke template (with three additional arguments) is:
		/// <code>
		/// [DllImport(InteropUtils.NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
		/// 	EntryPoint = "EntryPoint")]
		/// private static extern InteropBool ReturnSuccess(
		/// 	IntPtr failReason,
		/// 	TArg1 arg1,
		/// 	TArg2 arg2,
		/// 	TArg3 arg3
		/// );
		/// </code>
		/// </remarks>
		/// <typeparam name="TArg1">The type of the first argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg2">The type of the second argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg3">The type of the third argument to the native method after <c>failReason</c>.</typeparam>
		/// <param name="nativeFunc">The native func to call.</param>
		/// <param name="arg1">The first additional argument value.</param>
		/// <param name="arg2">The second additional argument value.</param>
		/// <param name="arg3">The third additional argument value.</param>
		/// <returns>A <see cref="NativeCallResult"/> that encapsulates the success status of the call, 
		/// and the failure message (if any).</returns>
		
		[CompilerGenerated]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe NativeCallResult CallNative<TArg1, TArg2, TArg3>(Func<IntPtr, TArg1, TArg2, TArg3, InteropBool> nativeFunc,
		TArg1 arg1, TArg2 arg2, TArg3 arg3
		) {
			Assure.NotNull(nativeFunc);
			try {
				char* failReason = stackalloc char[MAX_INTEROP_FAIL_REASON_STRING_LENGTH + 1];
				bool success = nativeFunc((IntPtr) failReason, arg1, arg2, arg3);
				return new NativeCallResult(success, success ? null : Marshal.PtrToStringUni((IntPtr) failReason));
			}
			catch (Exception e) {
				throw new NativeOperationFailedException("Cross-language invocation failed.", e);
			}
		}

		/// <summary>
		/// Calls a native (extern) method with the standard P/Invoke template, and four further arguments.
		/// </summary>
		/// <remarks>
		/// The standard P/Invoke template (with four additional arguments) is:
		/// <code>
		/// [DllImport(InteropUtils.NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
		/// 	EntryPoint = "EntryPoint")]
		/// private static extern InteropBool ReturnSuccess(
		/// 	IntPtr failReason,
		/// 	TArg1 arg1,
		/// 	TArg2 arg2,
		/// 	TArg3 arg3,
		/// 	TArg4 arg4
		/// );
		/// </code>
		/// </remarks>
		/// <typeparam name="TArg1">The type of the first argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg2">The type of the second argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg3">The type of the third argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg4">The type of the fourth argument to the native method after <c>failReason</c>.</typeparam>
		/// <param name="nativeFunc">The native func to call.</param>
		/// <param name="arg1">The first additional argument value.</param>
		/// <param name="arg2">The second additional argument value.</param>
		/// <param name="arg3">The third additional argument value.</param>
		/// <param name="arg4">The fourth additional argument value.</param>
		/// <returns>A <see cref="NativeCallResult"/> that encapsulates the success status of the call, 
		/// and the failure message (if any).</returns>
		
		[CompilerGenerated]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe NativeCallResult CallNative<TArg1, TArg2, TArg3, TArg4>(
			Func<IntPtr, TArg1, TArg2, TArg3, TArg4, InteropBool> nativeFunc,
			TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4
		) {
			Assure.NotNull(nativeFunc);
			try {
				char* failReason = stackalloc char[MAX_INTEROP_FAIL_REASON_STRING_LENGTH + 1];
				bool success = nativeFunc((IntPtr) failReason, arg1, arg2, arg3, arg4);
				return new NativeCallResult(success, success ? null : Marshal.PtrToStringUni((IntPtr) failReason));
			}
			catch (Exception e) {
				throw new NativeOperationFailedException("Cross-language invocation failed.", e);
			}
		}

		/// <summary>
		/// Calls a native (extern) method with the standard P/Invoke template, and five further arguments.
		/// </summary>
		/// <remarks>
		/// The standard P/Invoke template (with five additional arguments) is:
		/// <code>
		/// [DllImport(InteropUtils.NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
		/// 	EntryPoint = "EntryPoint")]
		/// private static extern InteropBool ReturnSuccess(
		/// 	IntPtr failReason,
		/// 	TArg1 arg1,
		/// 	TArg2 arg2,
		/// 	TArg3 arg3,
		/// 	TArg4 arg4,
		///		TArg5 arg5
		/// );
		/// </code>
		/// </remarks>
		/// <typeparam name="TArg1">The type of the first argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg2">The type of the second argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg3">The type of the third argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg4">The type of the fourth argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg5">The type of the fifth argument to the native method after <c>failReason</c>.</typeparam>
		/// <param name="nativeFunc">The native func to call.</param>
		/// <param name="arg1">The first additional argument value.</param>
		/// <param name="arg2">The second additional argument value.</param>
		/// <param name="arg3">The third additional argument value.</param>
		/// <param name="arg4">The fourth additional argument value.</param>
		/// <param name="arg5">The fifth additional argument value.</param>
		/// <returns>A <see cref="NativeCallResult"/> that encapsulates the success status of the call, 
		/// and the failure message (if any).</returns>
		
		[CompilerGenerated]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe NativeCallResult CallNative<TArg1, TArg2, TArg3, TArg4, TArg5>(
			Func<IntPtr, TArg1, TArg2, TArg3, TArg4, TArg5, InteropBool> nativeFunc,
			TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5
		) {
			Assure.NotNull(nativeFunc);
			try {
				char* failReason = stackalloc char[MAX_INTEROP_FAIL_REASON_STRING_LENGTH + 1];
				bool success = nativeFunc((IntPtr) failReason, arg1, arg2, arg3, arg4, arg5);
				return new NativeCallResult(success, success ? null : Marshal.PtrToStringUni((IntPtr) failReason));
			}
			catch (Exception e) {
				throw new NativeOperationFailedException("Cross-language invocation failed.", e);
			}
		}

		/// <summary>
		/// Calls a native (extern) method with the standard P/Invoke template, and six further arguments.
		/// </summary>
		/// <remarks>
		/// The standard P/Invoke template (with six additional arguments) is:
		/// <code>
		/// [DllImport(InteropUtils.NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
		/// 	EntryPoint = "EntryPoint")]
		/// private static extern InteropBool ReturnSuccess(
		/// 	IntPtr failReason,
		/// 	TArg1 arg1,
		/// 	TArg2 arg2,
		/// 	TArg3 arg3,
		/// 	TArg4 arg4,
		///		TArg5 arg5,
		/// 	TArg6 arg6
		/// );
		/// </code>
		/// </remarks>
		/// <typeparam name="TArg1">The type of the first argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg2">The type of the second argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg3">The type of the third argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg4">The type of the fourth argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg5">The type of the fifth argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg6">The type of the sixth argument to the native method after <c>failReason</c>.</typeparam>
		/// <param name="nativeFunc">The native func to call.</param>
		/// <param name="arg1">The first additional argument value.</param>
		/// <param name="arg2">The second additional argument value.</param>
		/// <param name="arg3">The third additional argument value.</param>
		/// <param name="arg4">The fourth additional argument value.</param>
		/// <param name="arg5">The fifth additional argument value.</param>
		/// <param name="arg6">The sixth additional argument value.</param>
		/// <returns>A <see cref="NativeCallResult"/> that encapsulates the success status of the call, 
		/// and the failure message (if any).</returns>
		
		[CompilerGenerated]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe NativeCallResult CallNative<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(
			Func<IntPtr, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, InteropBool> nativeFunc,
			TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6
		) {
			Assure.NotNull(nativeFunc);
			try {
				char* failReason = stackalloc char[MAX_INTEROP_FAIL_REASON_STRING_LENGTH + 1];
				bool success = nativeFunc((IntPtr) failReason, arg1, arg2, arg3, arg4, arg5, arg6);
				return new NativeCallResult(success, success ? null : Marshal.PtrToStringUni((IntPtr) failReason));
			}
			catch (Exception e) {
				throw new NativeOperationFailedException("Cross-language invocation failed.", e);
			}
		}

		/// <summary>
		/// Calls a native (extern) method with the standard P/Invoke template, and seven further arguments.
		/// </summary>
		/// <remarks>
		/// The standard P/Invoke template (with seven additional arguments) is:
		/// <code>
		/// [DllImport(InteropUtils.NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
		/// 	EntryPoint = "EntryPoint")]
		/// private static extern InteropBool ReturnSuccess(
		/// 	IntPtr failReason,
		/// 	TArg1 arg1,
		/// 	TArg2 arg2,
		/// 	TArg3 arg3,
		/// 	TArg4 arg4,
		///		TArg5 arg5,
		/// 	TArg6 arg6,
		///		TArg7 arg7
		/// );
		/// </code>
		/// </remarks>
		/// <typeparam name="TArg1">The type of the first argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg2">The type of the second argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg3">The type of the third argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg4">The type of the fourth argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg5">The type of the fifth argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg6">The type of the sixth argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg7">The type of the seventh argument to the native method after <c>failReason</c>.</typeparam>
		/// <param name="nativeFunc">The native func to call.</param>
		/// <param name="arg1">The first additional argument value.</param>
		/// <param name="arg2">The second additional argument value.</param>
		/// <param name="arg3">The third additional argument value.</param>
		/// <param name="arg4">The fourth additional argument value.</param>
		/// <param name="arg5">The fifth additional argument value.</param>
		/// <param name="arg6">The sixth additional argument value.</param>
		/// <param name="arg7">The seventh additional argument value.</param>
		/// <returns>A <see cref="NativeCallResult"/> that encapsulates the success status of the call, 
		/// and the failure message (if any).</returns>
		
		[CompilerGenerated]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe NativeCallResult CallNative<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(
			Func<IntPtr, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, InteropBool> nativeFunc,
			TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7
		) {
			Assure.NotNull(nativeFunc);
			try {
				char* failReason = stackalloc char[MAX_INTEROP_FAIL_REASON_STRING_LENGTH + 1];
				bool success = nativeFunc((IntPtr) failReason, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
				return new NativeCallResult(success, success ? null : Marshal.PtrToStringUni((IntPtr) failReason));
			}
			catch (Exception e) {
				throw new NativeOperationFailedException("Cross-language invocation failed.", e);
			}
		}

		/// <summary>
		/// Calls a native (extern) method with the standard P/Invoke template, and eight further arguments.
		/// </summary>
		/// <remarks>
		/// The standard P/Invoke template (with eight additional arguments) is:
		/// <code>
		/// [DllImport(InteropUtils.NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
		/// 	EntryPoint = "EntryPoint")]
		/// private static extern InteropBool ReturnSuccess(
		/// 	IntPtr failReason,
		/// 	TArg1 arg1,
		/// 	TArg2 arg2,
		/// 	TArg3 arg3,
		/// 	TArg4 arg4,
		///		TArg5 arg5,
		/// 	TArg6 arg6,
		///		TArg7 arg7,
		///		TArg8 arg8
		/// );
		/// </code>
		/// </remarks>
		/// <typeparam name="TArg1">The type of the first argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg2">The type of the second argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg3">The type of the third argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg4">The type of the fourth argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg5">The type of the fifth argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg6">The type of the sixth argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg7">The type of the seventh argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg8">The type of the eighth argument to the native method after <c>failReason</c>.</typeparam>
		/// <param name="nativeFunc">The native func to call.</param>
		/// <param name="arg1">The first additional argument value.</param>
		/// <param name="arg2">The second additional argument value.</param>
		/// <param name="arg3">The third additional argument value.</param>
		/// <param name="arg4">The fourth additional argument value.</param>
		/// <param name="arg5">The fifth additional argument value.</param>
		/// <param name="arg6">The sixth additional argument value.</param>
		/// <param name="arg7">The seventh additional argument value.</param>
		/// <param name="arg8">The eighth additional argument value.</param>
		/// <returns>A <see cref="NativeCallResult"/> that encapsulates the success status of the call, 
		/// and the failure message (if any).</returns>
		
		[CompilerGenerated]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe NativeCallResult CallNative<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8>(
			Func<IntPtr, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, InteropBool> nativeFunc,
			TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8
		) {
			Assure.NotNull(nativeFunc);
			try {
				char* failReason = stackalloc char[MAX_INTEROP_FAIL_REASON_STRING_LENGTH + 1];
				bool success = nativeFunc((IntPtr) failReason, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
				return new NativeCallResult(success, success ? null : Marshal.PtrToStringUni((IntPtr) failReason));
			}
			catch (Exception e) {
				throw new NativeOperationFailedException("Cross-language invocation failed.", e);
			}
		}

		/// <summary>
		/// Calls a native (extern) method with the standard P/Invoke template, and nine further arguments.
		/// </summary>
		/// <remarks>
		/// The standard P/Invoke template (with nine additional arguments) is:
		/// <code>
		/// [DllImport(InteropUtils.NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
		/// 	EntryPoint = "EntryPoint")]
		/// private static extern InteropBool ReturnSuccess(
		/// 	IntPtr failReason,
		/// 	TArg1 arg1,
		/// 	TArg2 arg2,
		/// 	TArg3 arg3,
		/// 	TArg4 arg4,
		///		TArg5 arg5,
		/// 	TArg6 arg6,
		///		TArg7 arg7,
		///		TArg8 arg8,
		///		TArg9 arg9
		/// );
		/// </code>
		/// </remarks>
		/// <typeparam name="TArg1">The type of the first argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg2">The type of the second argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg3">The type of the third argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg4">The type of the fourth argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg5">The type of the fifth argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg6">The type of the sixth argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg7">The type of the seventh argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg8">The type of the eighth argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg9">The type of the ninth argument to the native method after <c>failReason</c>.</typeparam>
		/// <param name="nativeFunc">The native func to call.</param>
		/// <param name="arg1">The first additional argument value.</param>
		/// <param name="arg2">The second additional argument value.</param>
		/// <param name="arg3">The third additional argument value.</param>
		/// <param name="arg4">The fourth additional argument value.</param>
		/// <param name="arg5">The fifth additional argument value.</param>
		/// <param name="arg6">The sixth additional argument value.</param>
		/// <param name="arg7">The seventh additional argument value.</param>
		/// <param name="arg8">The eighth additional argument value.</param>
		/// <param name="arg9">The ninth additional argument value.</param>
		/// <returns>A <see cref="NativeCallResult"/> that encapsulates the success status of the call, 
		/// and the failure message (if any).</returns>
		
		[CompilerGenerated]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe NativeCallResult CallNative<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9>(
			Func<IntPtr, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, InteropBool> nativeFunc,
			TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9
		) {
			Assure.NotNull(nativeFunc);
			try {
				char* failReason = stackalloc char[MAX_INTEROP_FAIL_REASON_STRING_LENGTH + 1];
				bool success = nativeFunc((IntPtr) failReason, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
				return new NativeCallResult(success, success ? null : Marshal.PtrToStringUni((IntPtr) failReason));
			}
			catch (Exception e) {
				throw new NativeOperationFailedException("Cross-language invocation failed.", e);
			}
		}

		/// <summary>
		/// Calls a native (extern) method with the standard P/Invoke template, and ten further arguments.
		/// </summary>
		/// <remarks>
		/// The standard P/Invoke template (with ten additional arguments) is:
		/// <code>
		/// [DllImport(InteropUtils.NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
		/// 	EntryPoint = "EntryPoint")]
		/// private static extern InteropBool ReturnSuccess(
		/// 	IntPtr failReason,
		/// 	TArg1 arg1,
		/// 	TArg2 arg2,
		/// 	TArg3 arg3,
		/// 	TArg4 arg4,
		///		TArg5 arg5,
		/// 	TArg6 arg6,
		///		TArg7 arg7,
		///		TArg8 arg8,
		///		TArg9 arg9,
		///		TArg10 arg10
		/// );
		/// </code>
		/// </remarks>
		/// <typeparam name="TArg1">The type of the first argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg2">The type of the second argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg3">The type of the third argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg4">The type of the fourth argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg5">The type of the fifth argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg6">The type of the sixth argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg7">The type of the seventh argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg8">The type of the eighth argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg9">The type of the ninth argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg10">The type of the tenth argument to the native method after <c>failReason</c>.</typeparam>
		/// <param name="nativeFunc">The native func to call.</param>
		/// <param name="arg1">The first additional argument value.</param>
		/// <param name="arg2">The second additional argument value.</param>
		/// <param name="arg3">The third additional argument value.</param>
		/// <param name="arg4">The fourth additional argument value.</param>
		/// <param name="arg5">The fifth additional argument value.</param>
		/// <param name="arg6">The sixth additional argument value.</param>
		/// <param name="arg7">The seventh additional argument value.</param>
		/// <param name="arg8">The eighth additional argument value.</param>
		/// <param name="arg9">The ninth additional argument value.</param>
		/// <param name="arg10">The tenth additional argument value.</param>
		/// <returns>A <see cref="NativeCallResult"/> that encapsulates the success status of the call, 
		/// and the failure message (if any).</returns>
		
		[CompilerGenerated]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe NativeCallResult CallNative<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10>(
			Func<IntPtr, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, InteropBool> nativeFunc,
			TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10
		) {
			Assure.NotNull(nativeFunc);
			try {
				char* failReason = stackalloc char[MAX_INTEROP_FAIL_REASON_STRING_LENGTH + 1];
				bool success = nativeFunc((IntPtr) failReason, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
				return new NativeCallResult(success, success ? null : Marshal.PtrToStringUni((IntPtr) failReason));
			}
			catch (Exception e) {
				throw new NativeOperationFailedException("Cross-language invocation failed.", e);
			}
		}

		/// <summary>
		/// Calls a native (extern) method with the standard P/Invoke template, and eleven further arguments.
		/// </summary>
		/// <remarks>
		/// The standard P/Invoke template (with eleven additional arguments) is:
		/// <code>
		/// [DllImport(InteropUtils.NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
		/// 	EntryPoint = "EntryPoint")]
		/// private static extern InteropBool ReturnSuccess(
		/// 	IntPtr failReason,
		/// 	TArg1 arg1,
		/// 	TArg2 arg2,
		/// 	TArg3 arg3,
		/// 	TArg4 arg4,
		///		TArg5 arg5,
		/// 	TArg6 arg6,
		///		TArg7 arg7,
		///		TArg8 arg8,
		///		TArg9 arg9,
		///		TArg10 arg10,
		///		TArg11 arg11
		/// );
		/// </code>
		/// </remarks>
		/// <typeparam name="TArg1">The type of the first argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg2">The type of the second argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg3">The type of the third argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg4">The type of the fourth argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg5">The type of the fifth argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg6">The type of the sixth argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg7">The type of the seventh argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg8">The type of the eighth argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg9">The type of the ninth argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg10">The type of the tenth argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg11">The type of the eleventh argument to the native method after <c>failReason</c>.</typeparam>
		/// <param name="nativeFunc">The native func to call.</param>
		/// <param name="arg1">The first additional argument value.</param>
		/// <param name="arg2">The second additional argument value.</param>
		/// <param name="arg3">The third additional argument value.</param>
		/// <param name="arg4">The fourth additional argument value.</param>
		/// <param name="arg5">The fifth additional argument value.</param>
		/// <param name="arg6">The sixth additional argument value.</param>
		/// <param name="arg7">The seventh additional argument value.</param>
		/// <param name="arg8">The eighth additional argument value.</param>
		/// <param name="arg9">The ninth additional argument value.</param>
		/// <param name="arg10">The tenth additional argument value.</param>
		/// <param name="arg11">The eleventh additional argument value.</param>
		/// <returns>A <see cref="NativeCallResult"/> that encapsulates the success status of the call, 
		/// and the failure message (if any).</returns>
		
		[CompilerGenerated]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe NativeCallResult CallNative<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, 
			TArg10, TArg11>(
			Func<IntPtr, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, 
			TArg10, TArg11, InteropBool> nativeFunc,
			TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, 
			TArg10 arg10, TArg11 arg11
		) {
			Assure.NotNull(nativeFunc);
			try {
				char* failReason = stackalloc char[MAX_INTEROP_FAIL_REASON_STRING_LENGTH + 1];
				bool success = nativeFunc((IntPtr) failReason, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
				return new NativeCallResult(success, success ? null : Marshal.PtrToStringUni((IntPtr) failReason));
			}
			catch (Exception e) {
				throw new NativeOperationFailedException("Cross-language invocation failed.", e);
			}
		}

		/// <summary>
		/// Calls a native (extern) method with the standard P/Invoke template, and twelve further arguments.
		/// </summary>
		/// <remarks>
		/// The standard P/Invoke template (with twelve additional arguments) is:
		/// <code>
		/// [DllImport(InteropUtils.NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
		/// 	EntryPoint = "EntryPoint")]
		/// private static extern InteropBool ReturnSuccess(
		/// 	IntPtr failReason,
		/// 	TArg1 arg1,
		/// 	TArg2 arg2,
		/// 	TArg3 arg3,
		/// 	TArg4 arg4,
		///		TArg5 arg5,
		/// 	TArg6 arg6,
		///		TArg7 arg7,
		///		TArg8 arg8,
		///		TArg9 arg9,
		///		TArg10 arg10,
		///		TArg11 arg11,
		///		TArg12 arg12
		/// );
		/// </code>
		/// </remarks>
		/// <typeparam name="TArg1">The type of the first argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg2">The type of the second argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg3">The type of the third argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg4">The type of the fourth argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg5">The type of the fifth argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg6">The type of the sixth argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg7">The type of the seventh argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg8">The type of the eighth argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg9">The type of the ninth argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg10">The type of the tenth argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg11">The type of the eleventh argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg12">The type of the twelfth argument to the native method after <c>failReason</c>.</typeparam>
		/// <param name="nativeFunc">The native func to call.</param>
		/// <param name="arg1">The first additional argument value.</param>
		/// <param name="arg2">The second additional argument value.</param>
		/// <param name="arg3">The third additional argument value.</param>
		/// <param name="arg4">The fourth additional argument value.</param>
		/// <param name="arg5">The fifth additional argument value.</param>
		/// <param name="arg6">The sixth additional argument value.</param>
		/// <param name="arg7">The seventh additional argument value.</param>
		/// <param name="arg8">The eighth additional argument value.</param>
		/// <param name="arg9">The ninth additional argument value.</param>
		/// <param name="arg10">The tenth additional argument value.</param>
		/// <param name="arg11">The eleventh additional argument value.</param>
		/// <param name="arg12">The twelfth additional argument value.</param>
		/// <returns>A <see cref="NativeCallResult"/> that encapsulates the success status of the call, 
		/// and the failure message (if any).</returns>
		
		[CompilerGenerated]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe NativeCallResult CallNative<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9,
			TArg10, TArg11, TArg12>(
			Func<IntPtr, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9,
			TArg10, TArg11, TArg12, InteropBool> nativeFunc,
			TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9,
			TArg10 arg10, TArg11 arg11, TArg12 arg12
		) {
			Assure.NotNull(nativeFunc);
			try {
				char* failReason = stackalloc char[MAX_INTEROP_FAIL_REASON_STRING_LENGTH + 1];
				bool success = nativeFunc((IntPtr) failReason, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, 
					arg10, arg11, arg12);
				return new NativeCallResult(success, success ? null : Marshal.PtrToStringUni((IntPtr) failReason));
			}
			catch (Exception e) {
				throw new NativeOperationFailedException("Cross-language invocation failed.", e);
			}
		}

		/// <summary>
		/// Calls a native (extern) method with the standard P/Invoke template, and thirteen further arguments.
		/// </summary>
		/// <remarks>
		/// The standard P/Invoke template (with thirteen additional arguments) is:
		/// <code>
		/// [DllImport(InteropUtils.NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
		/// 	EntryPoint = "EntryPoint")]
		/// private static extern InteropBool ReturnSuccess(
		/// 	IntPtr failReason,
		/// 	TArg1 arg1,
		/// 	TArg2 arg2,
		/// 	TArg3 arg3,
		/// 	TArg4 arg4,
		///		TArg5 arg5,
		/// 	TArg6 arg6,
		///		TArg7 arg7,
		///		TArg8 arg8,
		///		TArg9 arg9,
		///		TArg10 arg10,
		///		TArg11 arg11,
		///		TArg12 arg12,
		///		TArg13 arg13
		/// );
		/// </code>
		/// </remarks>
		/// <typeparam name="TArg1">The type of the first argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg2">The type of the second argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg3">The type of the third argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg4">The type of the fourth argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg5">The type of the fifth argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg6">The type of the sixth argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg7">The type of the seventh argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg8">The type of the eighth argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg9">The type of the ninth argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg10">The type of the tenth argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg11">The type of the eleventh argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg12">The type of the twelfth argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg13">The type of the thirteenth argument to the native method after <c>failReason</c>.</typeparam>
		/// <param name="nativeFunc">The native func to call.</param>
		/// <param name="arg1">The first additional argument value.</param>
		/// <param name="arg2">The second additional argument value.</param>
		/// <param name="arg3">The third additional argument value.</param>
		/// <param name="arg4">The fourth additional argument value.</param>
		/// <param name="arg5">The fifth additional argument value.</param>
		/// <param name="arg6">The sixth additional argument value.</param>
		/// <param name="arg7">The seventh additional argument value.</param>
		/// <param name="arg8">The eighth additional argument value.</param>
		/// <param name="arg9">The ninth additional argument value.</param>
		/// <param name="arg10">The tenth additional argument value.</param>
		/// <param name="arg11">The eleventh additional argument value.</param>
		/// <param name="arg12">The twelfth additional argument value.</param>
		/// <param name="arg13">The thirteenth additional argument value.</param>
		/// <returns>A <see cref="NativeCallResult"/> that encapsulates the success status of the call, 
		/// and the failure message (if any).</returns>
		
		[CompilerGenerated]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe NativeCallResult CallNative<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9,
			TArg10, TArg11, TArg12, TArg13>(
			Func<IntPtr, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9,
			TArg10, TArg11, TArg12, TArg13, InteropBool> nativeFunc,
			TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9,
			TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13
		) {
			Assure.NotNull(nativeFunc);
			try {
				char* failReason = stackalloc char[MAX_INTEROP_FAIL_REASON_STRING_LENGTH + 1];
				bool success = nativeFunc((IntPtr) failReason, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9,
					arg10, arg11, arg12, arg13);
				return new NativeCallResult(success, success ? null : Marshal.PtrToStringUni((IntPtr) failReason));
			}
			catch (Exception e) {
				throw new NativeOperationFailedException("Cross-language invocation failed.", e);
			}
		}

		/// <summary>
		/// Calls a native (extern) method with the standard P/Invoke template, and fourteen further arguments.
		/// </summary>
		/// <remarks>
		/// The standard P/Invoke template (with fourteen additional arguments) is:
		/// <code>
		/// [DllImport(InteropUtils.NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
		/// 	EntryPoint = "EntryPoint")]
		/// private static extern InteropBool ReturnSuccess(
		/// 	IntPtr failReason,
		/// 	TArg1 arg1,
		/// 	TArg2 arg2,
		/// 	TArg3 arg3,
		/// 	TArg4 arg4,
		///		TArg5 arg5,
		/// 	TArg6 arg6,
		///		TArg7 arg7,
		///		TArg8 arg8,
		///		TArg9 arg9,
		///		TArg10 arg10,
		///		TArg11 arg11,
		///		TArg12 arg12,
		///		TArg13 arg13,
		///		TArg14 arg14
		/// );
		/// </code>
		/// </remarks>
		/// <typeparam name="TArg1">The type of the first argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg2">The type of the second argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg3">The type of the third argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg4">The type of the fourth argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg5">The type of the fifth argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg6">The type of the sixth argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg7">The type of the seventh argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg8">The type of the eighth argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg9">The type of the ninth argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg10">The type of the tenth argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg11">The type of the eleventh argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg12">The type of the twelfth argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg13">The type of the thirteenth argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg14">The type of the fourteenth argument to the native method after <c>failReason</c>.</typeparam>
		/// <param name="nativeFunc">The native func to call.</param>
		/// <param name="arg1">The first additional argument value.</param>
		/// <param name="arg2">The second additional argument value.</param>
		/// <param name="arg3">The third additional argument value.</param>
		/// <param name="arg4">The fourth additional argument value.</param>
		/// <param name="arg5">The fifth additional argument value.</param>
		/// <param name="arg6">The sixth additional argument value.</param>
		/// <param name="arg7">The seventh additional argument value.</param>
		/// <param name="arg8">The eighth additional argument value.</param>
		/// <param name="arg9">The ninth additional argument value.</param>
		/// <param name="arg10">The tenth additional argument value.</param>
		/// <param name="arg11">The eleventh additional argument value.</param>
		/// <param name="arg12">The twelfth additional argument value.</param>
		/// <param name="arg13">The thirteenth additional argument value.</param>
		/// <param name="arg14">The fourteenth additional argument value.</param>
		/// <returns>A <see cref="NativeCallResult"/> that encapsulates the success status of the call, 
		/// and the failure message (if any).</returns>
		
		[CompilerGenerated]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe NativeCallResult CallNative<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9,
			TArg10, TArg11, TArg12, TArg13, TArg14>(
			Func<IntPtr, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9,
			TArg10, TArg11, TArg12, TArg13, TArg14, InteropBool> nativeFunc,
			TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9,
			TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13, TArg14 arg14
		) {
			Assure.NotNull(nativeFunc);
			try {
				char* failReason = stackalloc char[MAX_INTEROP_FAIL_REASON_STRING_LENGTH + 1];
				bool success = nativeFunc((IntPtr) failReason, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9,
					arg10, arg11, arg12, arg13, arg14);
				return new NativeCallResult(success, success ? null : Marshal.PtrToStringUni((IntPtr) failReason));
			}
			catch (Exception e) {
				throw new NativeOperationFailedException("Cross-language invocation failed.", e);
			}
		}

		/// <summary>
		/// Calls a native (extern) method with the standard P/Invoke template, and fifteen further arguments.
		/// </summary>
		/// <remarks>
		/// The standard P/Invoke template (with fifteen additional arguments) is:
		/// <code>
		/// [DllImport(InteropUtils.NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
		/// 	EntryPoint = "EntryPoint")]
		/// private static extern InteropBool ReturnSuccess(
		/// 	IntPtr failReason,
		/// 	TArg1 arg1,
		/// 	TArg2 arg2,
		/// 	TArg3 arg3,
		/// 	TArg4 arg4,
		///		TArg5 arg5,
		/// 	TArg6 arg6,
		///		TArg7 arg7,
		///		TArg8 arg8,
		///		TArg9 arg9,
		///		TArg10 arg10,
		///		TArg11 arg11,
		///		TArg12 arg12,
		///		TArg13 arg13,
		///		TArg14 arg14,
		///		TArg15 arg15
		/// );
		/// </code>
		/// </remarks>
		/// <typeparam name="TArg1">The type of the first argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg2">The type of the second argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg3">The type of the third argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg4">The type of the fourth argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg5">The type of the fifth argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg6">The type of the sixth argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg7">The type of the seventh argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg8">The type of the eighth argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg9">The type of the ninth argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg10">The type of the tenth argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg11">The type of the eleventh argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg12">The type of the twelfth argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg13">The type of the thirteenth argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg14">The type of the fourteenth argument to the native method after <c>failReason</c>.</typeparam>
		/// <typeparam name="TArg15">The type of the fifteenth argument to the native method after <c>failReason</c>.</typeparam>
		/// <param name="nativeFunc">The native func to call.</param>
		/// <param name="arg1">The first additional argument value.</param>
		/// <param name="arg2">The second additional argument value.</param>
		/// <param name="arg3">The third additional argument value.</param>
		/// <param name="arg4">The fourth additional argument value.</param>
		/// <param name="arg5">The fifth additional argument value.</param>
		/// <param name="arg6">The sixth additional argument value.</param>
		/// <param name="arg7">The seventh additional argument value.</param>
		/// <param name="arg8">The eighth additional argument value.</param>
		/// <param name="arg9">The ninth additional argument value.</param>
		/// <param name="arg10">The tenth additional argument value.</param>
		/// <param name="arg11">The eleventh additional argument value.</param>
		/// <param name="arg12">The twelfth additional argument value.</param>
		/// <param name="arg13">The thirteenth additional argument value.</param>
		/// <param name="arg14">The fourteenth additional argument value.</param>
		/// <param name="arg15">The fifteenth additional argument value.</param>
		/// <returns>A <see cref="NativeCallResult"/> that encapsulates the success status of the call, 
		/// and the failure message (if any).</returns>
		
		[CompilerGenerated]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe NativeCallResult CallNative<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9,
			TArg10, TArg11, TArg12, TArg13, TArg14, TArg15>(
			Func<IntPtr, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9,
			TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, InteropBool> nativeFunc,
			TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9,
			TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13, TArg14 arg14, TArg15 arg15
		) {
			Assure.NotNull(nativeFunc);
			try {
				char* failReason = stackalloc char[MAX_INTEROP_FAIL_REASON_STRING_LENGTH + 1];
				bool success = nativeFunc((IntPtr) failReason, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9,
					arg10, arg11, arg12, arg13, arg14, arg15);
				return new NativeCallResult(success, success ? null : Marshal.PtrToStringUni((IntPtr) failReason));
			}
			catch (Exception e) {
				throw new NativeOperationFailedException("Cross-language invocation failed.", e);
			}
		}
		#endregion

	}
}