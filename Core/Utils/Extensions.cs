// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 06 10 2014 at 14:12 by Ben Bowen

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Ophidian.Losgap {
	/// <summary>
	/// Contains extension methods used by the rest of the LOSGAP framework.
	/// </summary>
	public static partial class Extensions {
		/// <summary>
		/// Returns the value of ToString on the target object, or <paramref name="nullText"/> if the target object is null.
		/// </summary>
		/// <param name="this">The extended <typeparamref name="T"/>.</param>
		/// <param name="nullText">The string to return if the target object is null. Can be null itself.</param>
		/// <returns>The string representation of the object.</returns>
		public static string ToStringNullSafe<T>(this T @this, string nullText = "null") where T : class {
			return @this != null ? @this.ToString() : nullText;
		}

		/// <summary>
		/// Returns the value of ToString on the target object, or <paramref name="nullText"/> if the target object is null.
		/// </summary>
		/// <param name="this">The extended <typeparamref name="T"/>?.</param>
		/// <param name="nullText">The string to return if the target object is null. Can be null itself.</param>
		/// <returns>The string representation of the object.</returns>
		public static string ToStringNullSafe<T>(this T? @this, string nullText = "null") where T : struct {
			return @this != null ? @this.Value.ToString() : nullText;
		}

		/// <summary>
		/// Reads all remaining lines in the subject TextReader.
		/// </summary>
		/// <param name="this">The extended TextReader.</param>
		/// <returns>A list of strings, each one representing a single line from the stream.</returns>
		public static IList<string> ReadAllLines(this TextReader @this) {
			if (@this == null) throw new ArgumentNullException("this", "ReadAllLines called on a null TextReader.");
			var linesList = new List<string>();
			string nextLine;
			while ((nextLine = @this.ReadLine()) != null) {
				linesList.Add(nextLine);
			}

			return linesList;
		}

		/// <summary>
		/// Erases all data from the stream, and positions the stream at 0.
		/// </summary>
		/// <param name="this">The extended Stream.</param>
		public static void EraseAll(this Stream @this) {
			if (@this == null) throw new ArgumentNullException("this", "EraseAllData called on a null Stream.");
			@this.SetLength(0);
			@this.Seek(0, SeekOrigin.Begin);
		}

		/// <summary>
		/// Gets whether or not the given member is marked with the specified attribute type (at least once).
		/// </summary>
		/// <param name="this">The extended MemberInfo.</param>
		/// <param name="includeInheritors">Whether or not to include inheritors of this type in the decision.</param>
		/// <returns>True if the specified attribute is applied at least once to the member, false if not.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
			Justification = "Correct rule, but I'm copying the way the existing GetCustomAttribute methods work.")]
		public static bool HasCustomAttribute<TAttribute>(this MemberInfo @this, bool includeInheritors = false)
		where TAttribute : Attribute {
			if (@this == null) throw new ArgumentNullException("this", "HasAttribute called on a null MemberInfo.");
			return @this.GetCustomAttribute<TAttribute>(includeInheritors) != null;
		}

		/// <summary>
		/// Gets all declared attributes for the given Enum constant that are of type TAttribute (or derive from it).
		/// </summary>
		/// <typeparam name="TAttribute">The type of attribute to get.</typeparam>
		/// <param name="this">The enum field.</param>
		/// <returns>An array of matching attributes that have been applied to the given field.</returns>
		public static TAttribute[] GetAttributes<TAttribute>(this Enum @this)
			where TAttribute : Attribute {
			if (@this == null) throw new ArgumentNullException("this", "GetAttributes called on a null Enum.");
			
			Type enumType = @this.GetType();

			MemberInfo[] memberInfoArray = enumType.GetMember(@this.ToString());
			if (memberInfoArray.Length != 1) {
				throw new AmbiguousMatchException(memberInfoArray.Length + " MemberInfo objects found for '" + @this + "'...");
			}

			object[] attributes = memberInfoArray[0].GetCustomAttributes(typeof(TAttribute), true);

			return (TAttribute[]) attributes;
		}

		/// <summary>
		/// Gets the first declared attribute for the given Enum constant that is of the type TAttribute (or derived from it).
		/// </summary>
		/// <typeparam name="TAttribute">The type of attribute to get.</typeparam>
		/// <param name="this">The enum field.</param>
		/// <returns>The first matching attribute, or null if there is none.</returns>
		public static TAttribute GetAttribute<TAttribute>(this Enum @this)
			where TAttribute : Attribute {

			TAttribute[] attributes = GetAttributes<TAttribute>(@this);
			if (attributes.Length == 0) return null;
			return attributes[0];
		}

		/// <summary>
		/// Converts the target float in to a string with the specified number of decimal places.
		/// </summary>
		/// <param name="this">The extended float.</param>
		/// <param name="numDecimalPlaces">The number of decimal places to display.</param>
		/// <returns>A string that represents the target float.</returns>
		public static string ToString(this float @this, int numDecimalPlaces) {
			if (numDecimalPlaces < 0) throw new ArgumentException("Number of decimal places must be positive.", "numDecimalPlaces");

			string formatString = "{0:n" + numDecimalPlaces + "}";
			return String.Format(CultureInfo.InvariantCulture, formatString, @this).Snip(',');
		}

		/// <summary>
		/// Converts the target double in to a string with the specified number of decimal places.
		/// </summary>
		/// <param name="this">The extended double.</param>
		/// <param name="numDecimalPlaces">The number of decimal places to display.</param>
		/// <returns>A string that represents the target double.</returns>
		public static string ToString(this double @this, int numDecimalPlaces) {
			if (numDecimalPlaces < 0) throw new ArgumentException("Number of decimal places must be positive.", "numDecimalPlaces");

			string formatString = "{0:n" + numDecimalPlaces + "}";
			return String.Format(CultureInfo.InvariantCulture, formatString, @this).Snip(',');
		}

		/// <summary>
		/// Converts the target decimal in to a string with the specified number of decimal places.
		/// </summary>
		/// <param name="this">The extended decimal.</param>
		/// <param name="numDecimalPlaces">The number of decimal places to display.</param>
		/// <returns>A string that represents the target decimal.</returns>
		public static string ToString(this decimal @this, int numDecimalPlaces) {
			if (numDecimalPlaces < 0) throw new ArgumentException("Number of decimal places must be positive.", "numDecimalPlaces");

			string formatString = "{0:n" + numDecimalPlaces + "}";
			return String.Format(CultureInfo.InvariantCulture, formatString, @this).Snip(',');
		}

		/// <summary>
		/// Performs the given action on this exception and its <see cref="Exception.InnerException"/>, and that exception's InnerException,
		/// etc.
		/// </summary>
		/// <param name="this">The extended Exception.</param>
		/// <param name="action">The action to execute with every exception.</param>
		public static void ForEachException(this Exception @this, Action<Exception> action) {
			if (@this == null) throw new ArgumentNullException("this", "ForEachException called on a null Exception.");
			if (action == null) throw new ArgumentNullException("action");
			for (Exception currentException = @this; currentException != null; currentException = currentException.InnerException) {
				action(currentException);
			}
		}

		/// <summary>
		/// Creates a string that concatenates all messages of the subject exception and all its inner exceptions' messages.
		/// </summary>
		/// <param name="this">The extended Exception.</param>
		/// <param name="includeExceptionNames">Whether or not to include the exception type names before their respective messages.</param>
		/// <param name="messageSeparator">The string that is used to separate each message.</param>
		/// <returns>A string that contains all of the exception messages from the subject exception, in order of outermost to innermost.</returns>
		public static String GetAllMessages(this Exception @this, bool includeExceptionNames = false, string messageSeparator = " >>> ") {
			if (@this == null) throw new ArgumentNullException("this", "GetAllMessages called on a null Exception.");
			StringBuilder messageBuilder = new StringBuilder();

			@this.ForEachException(e => {
				if (e != @this) messageBuilder.Append(messageSeparator);
				if (includeExceptionNames) {
					messageBuilder.Append("[");
					messageBuilder.Append(e.GetType().Name);
					messageBuilder.Append("] ");
				}
				messageBuilder.Append(e.Message);
			});

			return messageBuilder.ToString();
		}

		/// <summary>
		/// Returns a new <typeparamref name="T"/> that is the value of <paramref name="this"/>, constrained between
		/// <paramref name="min"/> and <paramref name="max"/>.
		/// </summary>
		/// <param name="this">The extended T.</param>
		/// <param name="min">The minimum value of the <typeparamref name="T"/> that can be returned.</param>
		/// <param name="max">The maximum value of the <typeparamref name="T"/> that can be returned.</param>
		/// <returns>The equivalent to: <c>this &lt; min ? min : this &gt; max ? max : this</c>.</returns>
		public static T Clamp<T>(this T @this, T min, T max) where T : IComparable<T> {
			if (@this.CompareTo(min) < 0) return min;
			else if (@this.CompareTo(max) > 0) return max;
			else return @this;
		}
	}
}