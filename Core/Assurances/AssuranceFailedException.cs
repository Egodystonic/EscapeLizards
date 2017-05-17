// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 06 10 2014 at 15:55 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ophidian.Losgap {
	/// <summary>
	/// A <see cref="System.ApplicationException"/>, thrown when an <see cref="Assure"/> method fails, 
	/// and <see cref="Assure.FailureStrategy"/> is set to <see cref="AssuranceFailureStrategy.ThrowException"/>.
	/// </summary>
	/// <remarks>
	/// Catching this exception is never recommended, as it will add debug-specific branching to all code, 
	/// not just debug code. If you need to disable the throwing of exceptions from Assurances temporarily, 
	/// set <see cref="Assure.FailureStrategy"/> to something other than <see cref="AssuranceFailureStrategy.ThrowException"/>.
	/// </remarks>
	[Serializable]
	public class AssuranceFailedException : Exception {
		/// <summary>
		/// Creates a new AssuranceFailedException.
		/// </summary>
		public AssuranceFailedException() { }

		/// <summary>
		/// Creates a new AssuranceFailedException.
		/// </summary>
		/// <param name="message">A description of the error.</param>
		public AssuranceFailedException(string message) : base(message) { }

		/// <summary>
		/// Creates a new AssuranceFailedException.
		/// </summary>
		/// <param name="message">A description of the error.</param>
		/// <param name="innerException">The cause of this error.</param>
		public AssuranceFailedException(string message, Exception innerException) : base(message, innerException) { }

		/// <summary>
		/// Creates a new AssuranceFailedException with serialized data.
		/// </summary>
		/// <param name="serializationInfo">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
		/// <param name="streamingContext">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
		protected AssuranceFailedException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext) { }
	}
}

	