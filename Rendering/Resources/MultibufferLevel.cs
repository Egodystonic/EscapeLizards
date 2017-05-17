using System;
using System.Linq;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// Represents a number of buffers to use for multi-buffered windows.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1028:EnumStorageShouldBeInt32",
		Justification = "Losgap is not CLS-Compliant.")]
	public enum MultibufferLevel : uint {
		/// <summary>
		/// Indicates to use only a single buffer. This requires the least memory but can incur a performance penalty.
		/// </summary>
		Single = 0,
		/// <summary>
		/// Indicates to use a double buffer. More performant, but requires more memory, and can cause noticeable input lag when
		/// <see cref="RenderingModule.VSyncEnabled"/> is <c>true</c>.
		/// </summary>
		Double = 1,
		/// <summary>
		/// Indicates to use a triple buffer. Most performant option at the cost of greater memory use. Can ameliorate issues with input
		/// lag when <see cref="RenderingModule.VSyncEnabled"/> is <c>true</c>.
		/// </summary>
		Triple = 2
	}
}