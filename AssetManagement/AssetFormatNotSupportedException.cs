using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Ophidian.Losgap.AssetManagement {
	/// <summary>
	/// 
	/// </summary>
	/// <remarks>
	/// 
	/// </remarks>
	[Serializable]
	public class AssetFormatNotSupportedException :Exception {
		/// <summary>
		/// Creates a new AssetFormatNotSupportedException.
		/// </summary>
		public AssetFormatNotSupportedException() { }

		/// <summary>
		/// Creates a new AssetFormatNotSupportedException.
		/// </summary>
		/// <param name="message">A description of the error.</param>
		public AssetFormatNotSupportedException(string message) : base(message) { }

		/// <summary>
		/// Creates a new AssetFormatNotSupportedException.
		/// </summary>
		/// <param name="message">A description of the error.</param>
		/// <param name="innerException">The cause of this error.</param>
		public AssetFormatNotSupportedException(string message, Exception innerException) : base(message, innerException) { }

		/// <summary>
		/// Creates a new AssetFormatNotSupportedException with serialized data.
		/// </summary>
		/// <param name="serializationInfo">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
		/// <param name="streamingContext">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
		protected AssetFormatNotSupportedException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext) { }
	}
}
