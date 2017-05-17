// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 13 10 2014 at 09:26 by Ben Bowen

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace Ophidian.Losgap.Interop {
	/// <summary>
	/// Represents the result of a P/Invoke call to the native side of Losgap. Meant to be returned from a call to
	/// <see cref="InteropUtils.CallNative"/>.
	/// </summary>
	public struct NativeCallResult : IEquatable<NativeCallResult> {
		private const string NULL_FAILURE_MESSAGE_SURROGATE = "Unknown internal error.";
		/// <summary>
		/// Whether or not the call succeeded.
		/// </summary>
		public readonly bool Success;
		/// <summary>
		/// If <see cref="Success"/> is <c>false</c>, this string will usually hold a message detailing the reason for the failure.
		/// </summary>
		public readonly string FailureMessage;

		/// <summary>
		/// Creates a new NativeCallResult.
		/// </summary>
		/// <param name="success">Whether or not the native call succeeded.</param>
		/// <param name="failureMessage">The failure message associated with the operation. May be null.</param>
		public NativeCallResult(bool success, string failureMessage) {
			Success = success;
			FailureMessage = failureMessage;
		}

		/// <summary>
		/// Throws a new <see cref="NativeOperationFailedException"/> if <see cref="Success"/> is <c>false</c>; with the exception message
		/// set to <see cref="FailureMessage"/>.
		/// </summary>
		public void ThrowOnFailure() {
			if (!Success) throw new NativeOperationFailedException(FailureMessage ?? NULL_FAILURE_MESSAGE_SURROGATE);
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(NativeCallResult other) {
			return Success.Equals(other.Success) && string.Equals(FailureMessage, other.FailureMessage);
		}

		/// <summary>
		/// Returns a string representing the state/values of this instance.
		/// </summary>
		/// <returns>
		/// A new string with details of this instance.
		/// </returns>
		public override string ToString() {
			return Success ? "Success" : "Failure: " + FailureMessage;
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
			return obj is NativeCallResult && Equals((NativeCallResult)obj);
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
				return (Success.GetHashCode() * 397) ^ (FailureMessage != null ? FailureMessage.GetHashCode() : 0);
			}
		}

		/// <summary>
		/// Checks if <paramref name="left"/> and <paramref name="right"/> have the same value for <see cref="Success"/>, and the same
		/// <see cref="FailureMessage"/>.
		/// </summary>
		/// <param name="left">The first result.</param>
		/// <param name="right">The second result.</param>
		/// <returns>True if the two objects contain equal values.</returns>
		public static bool operator ==(NativeCallResult left, NativeCallResult right) {
			return left.Equals(right);
		}

		/// <summary>
		/// Checks if <paramref name="left"/> and <paramref name="right"/> do not have the same value for <see cref="Success"/> and
		/// <see cref="FailureMessage"/>.
		/// </summary>
		/// <param name="left">The first result.</param>
		/// <param name="right">The second result.</param>
		/// <returns>True if the two objects do not contain equal values.</returns>
		public static bool operator !=(NativeCallResult left, NativeCallResult right) {
			return !left.Equals(right);
		}
	}
}