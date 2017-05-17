// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 09 10 2014 at 14:31 by Ben Bowen

using System;
using System.Linq;
using System.Runtime.Serialization;

namespace Ophidian.Losgap.Interop {
	/// <summary>
	/// A <see cref="Exception"/>, thrown when an internal framework call fails.
	/// </summary>
	/// <remarks>
	/// This exception may be caught in certain circumstances, but usually indicates a bug in the framework.
	/// </remarks>
	[Serializable]
	public class NativeOperationFailedException : Exception {
		/// <summary>
		/// Creates a new NativeOperationFailedException.
		/// </summary>
		public NativeOperationFailedException() { }

		/// <summary>
		/// Creates a new NativeOperationFailedException.
		/// </summary>
		/// <param name="message">A description of the error.</param>
		public NativeOperationFailedException(string message) : base(message) { }

		/// <summary>
		/// Creates a new NativeOperationFailedException.
		/// </summary>
		/// <param name="message">A description of the error.</param>
		/// <param name="innerException">The cause of this error.</param>
		public NativeOperationFailedException(string message, Exception innerException) : base(message, innerException) { }

		/// <summary>
		/// Creates a new NativeOperationFailedException with serialized data.
		/// </summary>
		/// <param name="serializationInfo">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
		/// <param name="streamingContext">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
		protected NativeOperationFailedException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext) { }

		 
	}
}

	