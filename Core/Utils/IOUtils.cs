// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 06 10 2014 at 14:11 by Ben Bowen

using System;
using System.IO;
using System.Linq;

namespace Ophidian.Losgap {
	/// <summary>
	/// Contains static helper method for working with files and general IO.
	/// </summary>
	public static class IOUtils {
		/// <summary>
		/// Checks if the given <paramref name="filePath"/> represents the location of a valid file. Does not check whether the file path
		/// exists.
		/// </summary>
		/// <param name="filePath">The file path to check. Can be only a filename, or a path and a file name.</param>
		/// <returns>True if the given <paramref name="filePath"/> is valid, or false if not.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0",
			Justification = "filePath is validated (IsNullOrWhiteSpace), but not in a way the code analyzer recognizes.")]
		public static bool IsValidFilePath(string filePath) {
			if (filePath.IsNullOrWhiteSpace()) return false;
			try {
				string filenameWithExt = Path.GetFileName(filePath);
				string path = Path.GetDirectoryName(filePath);

				if (filenameWithExt.IsNullOrWhiteSpace() || path == null) return false;

				// ReSharper disable PossibleNullReferenceException
				return filePath.Length <= 240 
				&& filenameWithExt.IndexOfAny(Path.GetInvalidFileNameChars()) < 0
				&& path.IndexOfAny(Path.GetInvalidPathChars()) < 0
				&& filenameWithExt.Replace(".", String.Empty).Length > 0;
				// ReSharper restore PossibleNullReferenceException
			}
			catch (ArgumentException) {
				return false;
			}
		}
	}
}