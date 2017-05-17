// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 06 10 2014 at 14:12 by Ben Bowen

using System;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace Ophidian.Losgap {
	public static partial class Extensions {
		/// <summary>
		/// Returns whether or not a string is null or empty.
		/// </summary>
		/// <seealso cref="string.IsNullOrEmpty"/>
		/// <param name="this">The extended string.</param>
		/// <returns>True if the subject string is null or contains no characters.</returns>
		public static bool IsNullOrEmpty(this string @this) { return String.IsNullOrEmpty(@this); }

		/// <summary>
		/// Returns whether or not a string is null, empty, or contains only whitespace.
		/// </summary>
		/// <seealso cref="string.IsNullOrWhiteSpace"/>
		/// <param name="this">The extended string.</param>
		/// <returns>True if the subject string is null, contains no characters, or contains only whitespace characters.</returns>
		public static bool IsNullOrWhiteSpace(this string @this) { return String.IsNullOrWhiteSpace(@this); }

		/// <summary>
		/// Same as calling <see cref="String.Split(string[], StringSplitOptions)"/> with an array of size 1.
		/// </summary>
		/// <param name="this">The extended string.</param>
		/// <param name="separator">The delimiter that splits substrings in the given string. Must not be null.</param>
		/// <param name="splitOptions">RemoveEmptyEntries to omit empty array elements from the array returned; or None to include empty array elements in the array returned.</param>
		/// <returns>See: <see cref="String.Split(string[], StringSplitOptions)"/>.</returns>
		public static string[] Split(this string @this, string separator, StringSplitOptions splitOptions = StringSplitOptions.None) {
			if (@this == null) throw new ArgumentNullException("this", "Split called on a null String.");
			if (separator == null) throw new ArgumentNullException("separator");
			return @this.Split(new[] { separator }, splitOptions);
		}

		/// <summary>
		/// Same as calling <see cref="String.Split(string[], StringSplitOptions)"/> with an array of size 1.
		/// </summary>
		/// <param name="this">The extended string.</param>
		/// <param name="separator">The delimiter that splits substrings in the given string.</param>
		/// <param name="splitOptions">RemoveEmptyEntries to omit empty array elements from the array returned; or None to include empty array elements in the array returned.</param>
		/// <returns>See: <see cref="String.Split(string[], StringSplitOptions)"/>.</returns>
		public static string[] Split(this string @this, char separator, StringSplitOptions splitOptions = StringSplitOptions.None) {
			if (@this == null) throw new ArgumentNullException("this", "Split called on a null String.");
			return @this.Split(new[] { separator }, splitOptions);
		}

		/// <summary>
		/// Splits the string only to the Nth occurrence of <paramref name="separator"/>.
		/// </summary>
		/// <param name="this">The extended string.</param>
		/// <param name="separator">The delimiter that splits substrings in the given string. Must not be null.</param>
		/// <param name="n">The index of last occurrence of <paramref name="separator"/> that causes a split.
		/// The maximum size of the returned array will be n + 2. Supplying a value of -1 will result in a single-element array returned
		/// containing only the original string.</param>
		/// <param name="splitOptions">RemoveEmptyEntries to omit empty array elements from the array returned; or None to include empty array elements in the array returned.</param>
		/// <returns>Similar to <see cref="String.Split(string[], StringSplitOptions)"/> but with repeated instances of the given separator
		/// (past <paramref name="n"/> occurrences) ignored in the last element of the returned array.</returns>
		public static string[] SplitToN(this string @this, string separator, int n, StringSplitOptions splitOptions = StringSplitOptions.None) {
			if (@this == null) throw new ArgumentNullException("this", "Split called on a null String.");
			if (separator == null) throw new ArgumentNullException("separator");
			if (n < 0) return new[] { @this };
			var conventionalSplit = @this.Split(separator, splitOptions);
			if (conventionalSplit.Length < n + 2) return conventionalSplit;
			var result = new string[n + 2];
			for (int i = 0; i < n + 1; ++i) result[i] = conventionalSplit[i];
			result[n + 1] = conventionalSplit.Skip(n + 1).Join(separator);
			return result;
		}

		/// <summary>
		/// Splits the string only to the Nth occurrence of <paramref name="separator"/>.
		/// </summary>
		/// <param name="this">The extended string.</param>
		/// <param name="separator">The delimiter that splits substrings in the given string.</param>
		/// <param name="n">The index of last occurrence of <paramref name="separator"/> that causes a split.
		/// The maximum size of the returned array will be n + 2. Supplying a value of -1 will result in a single-element array returned
		/// containing only the original string.</param>
		/// <param name="splitOptions">RemoveEmptyEntries to omit empty array elements from the array returned; or None to include empty array elements in the array returned.</param>
		/// <returns>Similar to <see cref="String.Split(string[], StringSplitOptions)"/> but with repeated instances of the given separator
		/// (past <paramref name="n"/> occurrences) ignored in the last element of the returned array.</returns>
		public static string[] SplitToN(this string @this, char separator, int n, StringSplitOptions splitOptions = StringSplitOptions.None) {
			if (@this == null) throw new ArgumentNullException("this", "Split called on a null String.");
			if (n < 0) return new[] { @this };
			var conventionalSplit = @this.Split(separator, splitOptions);
			if (conventionalSplit.Length < n + 2) return conventionalSplit;
			var result = new string[n + 2];
			for (int i = 0; i < n + 1; ++i) result[i] = conventionalSplit[i];
			result[n + 1] = conventionalSplit.Skip(n + 1).Join(separator.ToString());
			return result;
		}

		/// <summary>
		/// Extension method overload for <see cref="string.Join(string,System.Collections.Generic.IEnumerable{string})"/>.
		/// </summary>
		/// <param name="this">The extended IEnumerable&lt;string&gt;.</param>
		/// <returns>A string that consists of the members of <paramref name="this"/>, delimited by the <paramref name="separator"/> string. If <paramref name="this"/> has no members, the method returns <see cref="F:System.String.Empty"/>.</returns>
		public static string Join(this IEnumerable<string> @this, string separator) {
			if (@this == null) throw new ArgumentNullException("this", "Join called on a null IEnumerable<string>.");
			return String.Join(separator, @this);
		}

		/// <summary>
		/// Repeats the supplied string the specified number of times, putting the separator string between each repetition.
		/// </summary>
		/// <param name="this">The extended string.</param>
		/// <param name="repetitions">The number of repetitions of the string to make. Must not be negative.</param>
		/// <param name="separator">The separator string to place between each repetition. Must not be null.</param>
		/// <returns>The subject string, repeated n times, where n = repetitions. Between each repetition will be the separator string. If n is 0, this method will return String.Empty.</returns>
		public static string Repeat(this string @this, int repetitions, string separator = "") {
			if (@this == null) throw new ArgumentNullException("this", "Repeat called on a null string.");
			if (separator == null) throw new ArgumentNullException("separator");
			if (repetitions < 0) throw new ArgumentOutOfRangeException("repetitions", "Value must not be negative.");

			if (repetitions == 0) return String.Empty;
			StringBuilder builder = new StringBuilder(@this.Length * repetitions + separator.Length * (repetitions - 1));
			for (int i = 0; i < repetitions; ++i) {
				if (i > 0) builder.Append(separator);
				builder.Append(@this);
			}
			return builder.ToString();
		}

		/// <summary>
		/// Capitalizes the first character of the provided string.
		/// </summary>
		/// <param name="this">The extended string.</param>
		/// <returns>Returns a new string which is identical to the subject string; except that the first character will be upper case if it is a valid letter.</returns>
		public static string CapitalizeFirst(this string @this) {
			if (@this == null) throw new ArgumentNullException("this", "CapitalizeFirst called on a null string.");
			char[] chars = @this.ToCharArray();
			chars[0] = Char.ToUpper(chars[0]);
			return new String(chars);
		}

		/// <summary>
		/// Removes the specified characters from the string.
		/// </summary>
		/// <param name="this">The extended string.</param>
		/// <param name="charsToRemove">The characters to remove from the string.</param>
		/// <returns>A string where all specified characters have been removed.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "chars",
			Justification = "It's a plural noun, which is fine with a collection of the type that has no otherwise meaningful representation.")]
		public static string Snip(this string @this, params char[] charsToRemove) {
			if (@this == null) throw new ArgumentNullException("this", "Snip called on a null string.");
			return Snip(@this, charsToRemove.Select(c => c.ToString(CultureInfo.InvariantCulture)).ToArray());
		}

		/// <summary>
		/// Removes any occurrences of the specified strings from the target string.
		/// </summary>
		/// <param name="this">The extended string.</param>
		/// <param name="stringsToRemove">The strings to search for (and remove).</param>
		/// <returns>A new string where any matching substrings have been removed.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "strings",
			Justification = "It's a plural noun, which is fine with a collection of the type that has no otherwise meaningful representation.")]
		public static string Snip(this string @this, params string[] stringsToRemove) {
			if (@this == null) throw new ArgumentNullException("this", "Snip called on a null string.");
			stringsToRemove.ForEach(s => @this = @this.Replace(s, String.Empty));
			return @this;
		}

		/// <summary>
		/// Performs the same action as <see cref="string.Substring(int)"/> but counting from the end of the string (instead of the start).
		/// </summary>
		/// <param name="this">The extended string.</param>
		/// <param name="endIndex">The zero-based starting character position (from the end) of a substring in this instance.</param>
		/// <returns>Returns the original string with <paramref name="endIndex"/> characters removed from the end.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if endIndex is greater than the length of the string (or negative).</exception>
		public static string SubstringFromEnd(this string @this, int endIndex) {
			if (@this == null) throw new ArgumentNullException("this", "SubstringFromEnd called on a null string.");
			if (endIndex < 0 || endIndex > @this.Length) throw new ArgumentOutOfRangeException("endIndex");
			return @this.Substring(0, @this.Length - endIndex);
		}

		/// <summary>
		/// Performs the same action as <see cref="string.Substring(int, int)"/> but counting from the end of the string (instead of the start).
		/// </summary>
		/// <param name="this">The extended string.</param>
		/// <param name="endIndex">The zero-based starting character position (from the end) of a substring in this instance.</param>
		/// <param name="length">The number of characters in the substring.</param>
		/// <returns>Returns <paramref name="length"/> characters of the subject string, counting backwards from
		/// <paramref name="endIndex"/>.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if endIndex is greater than the length of the string (or negative).</exception>
		public static string SubstringFromEnd(this string @this, int endIndex, int length) {
			if (@this == null) throw new ArgumentNullException("this", "SubstringFromEnd called on a null string.");
			if (endIndex < 0 || endIndex > @this.Length) throw new ArgumentOutOfRangeException("endIndex");
			return @this.Substring((@this.Length - endIndex) - length, length);
		}

		/// <summary>
		/// Same as <see cref="string.Contains"/> but allows checking for multiple match values.
		/// </summary>
		/// <param name="this">The extended string.</param>
		/// <param name="matchValues">The array of strings to check for any occurrence of in <paramref name="this"/>.</param>
		/// <returns>True if <paramref name="this"/> contains any of the given <paramref name="matchValues"/>.</returns>
		public static bool Contains(this string @this, params string[] matchValues) {
			if (@this == null) throw new ArgumentNullException("this", "Contains called on a null string.");
			return matchValues.Any(@this.Contains);
		}

		/// <summary>
		/// Same as <see cref="string.Contains"/> but allows checking for multiple match values.
		/// </summary>
		/// <param name="this">The extended string.</param>
		/// <param name="matchValues">The array of chars to check for any occurrence of in <paramref name="this"/>.</param>
		/// <returns>True if <paramref name="this"/> contains any of the given <paramref name="matchValues"/>.</returns>
		public static bool Contains(this string @this, params char[] matchValues) {
			if (@this == null) throw new ArgumentNullException("this", "Contains called on a null string.");
			return matchValues.Any(@this.Contains);
		}

		/// <summary>
		/// Returns <paramref name="this"/> but culled to a maximum length of <paramref name="maxLength"/> characters.
		/// </summary>
		/// <param name="this">The extended string.</param>
		/// <param name="maxLength">The maximum desired length of the string.</param>
		/// <returns>A string containing the first <c>Min(this.Length, maxLength)</c> characters from the extended string.</returns>
		public static string WithMaxLength(this string @this, int maxLength) {
			if (@this == null) throw new ArgumentNullException("this", "WithMaxLength called on a null string.");
			return @this.Substring(0, Math.Min(@this.Length, maxLength));
		}
	}
}