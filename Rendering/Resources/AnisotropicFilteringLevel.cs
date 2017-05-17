using System;
using System.Linq;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// An enumeration of anisotropic filtering levels; used by <see cref="TextureSampler"/>s when the
	/// <see cref="TextureFilterType.Anisotropic"/> <see cref="TextureFilterType"/> is enabled.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1028:EnumStorageShouldBeInt32",
		Justification = "Losgap is not CLS-Compliant.")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1027:MarkEnumsWithFlags",
		Justification = "The values look like bitfield values, but are in fact not.")]
	public enum AnisotropicFilteringLevel : uint {
		/// <summary>
		/// Disables anisotropic filtering.
		/// </summary>
		None = 0,
		/// <summary>
		/// Two times. Cheapest option (in terms of performance), but with lowest-quality results.
		/// </summary>
		TwoTimes = 2,
		/// <summary>
		/// Four times. Greater performance hit in comparison to <see cref="TwoTimes"/>, but with better quality output.
		/// </summary>
		FourTimes = 4,
		/// <summary>
		/// Eight times. Even greater performance hit in comparison to <see cref="FourTimes"/>, but with even better quality output.
		/// </summary>
		EightTimes = 8,
		/// <summary>
		/// Sixteen times. Greatest performance hit of all options; and with arguably diminishing returns on quality level (but still
		/// the highest quality).
		/// </summary>
		SixteenTimes = 16
	}
}