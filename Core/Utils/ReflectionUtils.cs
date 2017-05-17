// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 02 03 2015 at 11:59 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace Ophidian.Losgap {
	/// <summary>
	/// A static class containing utilities for reflecting over types/methods/fields in the application.
	/// </summary>
	public static class ReflectionUtils {
		private static readonly Type[] allReferencedTypes = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(ass => ass.GetReferencedAssemblies().Select(Assembly.Load).Concat(ass))
				.Distinct()
				.SelectMany(ass => ass.GetTypes())
				.ToArray();

		/// <summary>
		/// Get all types that inherit from / implement the given <paramref name="parent"/>.
		/// </summary>
		/// <param name="parent">The parent to find implementers/inheritors of. Must not be null.</param>
		/// <param name="includeParent">If <c>true</c>, the parent type will be included in the returned <see cref="IEnumerable{T}"/>. Set to
		/// <c>false</c> to only include child types.</param>
		/// <returns>An enumeration of types that inherit from or implement the given <paramref name="parent"/>.</returns>
		public static IEnumerable<Type> GetChildTypes(Type parent, bool includeParent = true) {
			Assure.NotNull(parent);
			if (includeParent) return allReferencedTypes.Where(parent.IsAssignableFrom);
			else return allReferencedTypes.Where(parent.IsAssignableFrom).Except(new[] { parent });
		}
	}
}