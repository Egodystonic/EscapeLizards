// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 20 11 2014 at 13:30 by Ben Bowen

using System;
using System.Collections.Generic;

namespace Ophidian.Losgap {
	/// <summary>
	/// Extension of the .NET <see cref="IEquatable{T}"/> type that provisions for testing for equality with a tolerance to accumulated
	/// floating-point maths errors.
	/// </summary>
	/// <typeparam name="T">The equatable type.</typeparam>
	public interface IToleranceEquatable<T> : IEquatable<T> {
		/// <summary>
		/// Test whether this object is EXACTLY equal to <paramref name="other"/>. No floating-point tolerance will be accounted
		/// for: The two structures must be bitwise identical.
		/// </summary>
		/// <param name="other">The other <typeparamref name="T"/> to test against.</param>
		/// <returns>True if this object is EXACTLY equal to <paramref name="other"/>.</returns>
		bool EqualsExactly(T other);

		/// <summary>
		/// Test whether this object is equal to <paramref name="other"/> within a given <paramref name="tolerance"/>. Even if the
		/// two structures are not identical, this method will return <c>true</c> if they are 'close enough' (e.g. within the tolerance).
		/// </summary>
		/// <param name="other">The other <typeparamref name="T"/> to test against.</param>
		/// <param name="tolerance">The amount of tolerance to give. For any floating-point component in both objects, if the difference
		/// between them is less than this value, they will be considered equal.</param>
		/// <returns>True if this object is equal to <paramref name="other"/> with the given <paramref name="tolerance"/>.</returns>
		bool EqualsWithTolerance(T other, double tolerance);
	}
}