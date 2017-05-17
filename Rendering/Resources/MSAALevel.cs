using System;
using System.Linq;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// An enumeration of supported multi-sampling levels. Multisampling is an anti-aliasing technique. Commonly referred to as <c>MSAA</c>.
	/// </summary>
	/// <seealso cref="RenderingModule.AntialiasingLevel"/>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1028:EnumStorageShouldBeInt32",
		Justification = "Losgap is not CLS-Compliant.")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1027:MarkEnumsWithFlags",
		Justification = "The values look like bitfield values, but are in fact not.")]
	public enum MSAALevel : uint {
		/// <summary>
		/// Disable MSAA.
		/// </summary>
		None = 0,
		/// <summary>
		/// Provides a minimal anti-aliased scene with a moderate performance cost.
		/// </summary>
		TwoTimes = 2,
		/// <summary>
		/// Provides an improved anti-aliased scene with a large performance cost.
		/// </summary>
		FourTimes = 4,
		/// <summary>
		/// Provides an excellent anti-aliased scene with a very large performance cost.
		/// </summary>
		EightTimes = 8,
		/// <summary>
		/// Provides an excellent anti-aliased scene with a substantial performance cost.
		/// </summary>
		SixteenTimes = 16
	}
}