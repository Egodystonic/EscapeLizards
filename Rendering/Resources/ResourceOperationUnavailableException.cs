// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 29 10 2014 at 10:08 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// A <see cref="System.Exception"/>, thrown when attempting to execute an operation on a resource (e.g. read/write/copy), but the
	/// resource was not created with the relevant <see cref="ResourceUsage"/> 
	/// </summary>
	/// <remarks>
	/// This exception should never be caught, as it represents a design-time failure. 
	/// Check resource usage with the <see cref="IResource.Usage"/> properties, 
	/// or any of the "CanXXX" properties defined on before executing an operation if required.
	/// </remarks>
	[Serializable]
	public class ResourceOperationUnavailableException : System.Exception {
		/// <summary>
		/// Creates a new ResourceOperationUnavailableException.
		/// </summary>
		public ResourceOperationUnavailableException() { }

		/// <summary>
		/// Creates a new ResourceOperationUnavailableException.
		/// </summary>
		/// <param name="message">A description of the error.</param>
		public ResourceOperationUnavailableException(string message) : base(message) { }

		/// <summary>
		/// Creates a new ResourceOperationUnavailableException.
		/// </summary>
		/// <param name="message">A description of the error.</param>
		/// <param name="innerException">The cause of this error.</param>
		public ResourceOperationUnavailableException(string message, Exception innerException) : base(message, innerException) { }

		/// <summary>
		/// Creates a new ResourceOperationUnavailableException with serialized data.
		/// </summary>
		/// <param name="serializationInfo">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
		/// <param name="streamingContext">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
		protected ResourceOperationUnavailableException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext) { }

		 
	}
}

	