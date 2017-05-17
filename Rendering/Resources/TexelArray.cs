// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 06 11 2014 at 16:55 by Ben Bowen

using System;
using System.Collections;
using System.Collections.Generic;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// Provides a 1-dimensional view onto a flat texel array.
	/// </summary>
	/// <remarks>
	/// This type provides no real benefit over exposing the underlying data array directly, but is provided for consistency's sake
	/// alongside the 2D and 3D variants.
	/// </remarks>
	/// <typeparam name="TTexel">The texel format.</typeparam>
	public struct TexelArray1D<TTexel> : IEnumerable<TTexel>, IEquatable<TexelArray1D<TTexel>>
		where TTexel : struct, ITexel {
		/// <summary>
		/// The underlying flat texel array.
		/// </summary>
		public readonly TTexel[] Data;

		/// <summary>
		/// The number of texels encapsulated in this texel array.
		/// </summary>
		public readonly uint Width;

		/// <summary>
		/// Creates a new 1D texel array with the given data.
		/// </summary>
		/// <param name="data">The data to encapsulate. Must not be null.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0",
			Justification = "The parameter is checked by the assurance.")]
		public TexelArray1D(TTexel[] data) {
			Assure.NotNull(data);
			this.Data = data;
			Width = (uint) data.Length;
		}

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
		/// </returns>
		/// <filterpriority>1</filterpriority>
		public IEnumerator<TTexel> GetEnumerator() {
			return (Data as IEnumerable<TTexel>).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		/// <summary>
		/// Gets or sets the texel at the specified index.
		/// </summary>
		/// <param name="u">The x-coordinate of the texel to get.</param>
		/// <returns>The <typeparamref name="TTexel"/> at the given index.</returns>
		public TTexel this[int u] {
			get {
				Assure.LessThan(u, Width, "Index out of bounds: u");
				return Data[u];
			}
			set {
				Assure.LessThan(u, Width, "Index out of bounds: u");
				Data[u] = value;
			}
		}
		/// <summary>
		/// Gets the texel at the specified index.
		/// </summary>
		/// <param name="u">The x-coordinate of the texel to get.</param>
		/// <returns>The <typeparamref name="TTexel"/> at the given index.</returns>
		public TTexel this[uint u] {
			get {
				Assure.LessThan(u, Width, "Index out of bounds: u");
				return Data[u];
			}
			set {
				Assure.LessThan(u, Width, "Index out of bounds: u");
				Data[u] = value;
			}
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(TexelArray1D<TTexel> other) {
			return Equals(Data, other.Data) && Width == other.Width;
		}

		/// <summary>
		/// Indicates whether this instance and a specified object are equal.
		/// </summary>
		/// <returns>
		/// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false. 
		/// </returns>
		/// <param name="obj">The object to compare with the current instance. </param><filterpriority>2</filterpriority>
		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) {
				return false;
			}
			return obj is TexelArray1D<TTexel> && Equals((TexelArray1D<TTexel>)obj);
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>
		/// A 32-bit signed integer that is the hash code for this instance.
		/// </returns>
		/// <filterpriority>2</filterpriority>
		public override int GetHashCode() {
			unchecked {
				return ((Data != null ? Data.GetHashCode() : 0) * 397) ^ (int)Width;
			}
		}

		/// <summary>
		/// Indicates whether or not the two texel arrays refer to the same data, with the same representation of that data.
		/// </summary>
		/// <param name="left">The first operand.</param>
		/// <param name="right">The second operand.</param>
		/// <returns>True if both operands point to the same data, and have the same criteria for its dimensions.</returns>
		public static bool operator ==(TexelArray1D<TTexel> left, TexelArray1D<TTexel> right) {
			return left.Equals(right);
		}

		/// <summary>
		/// Indicates whether or not the two texel arrays refer to the same data, with the same representation of that data.
		/// </summary>
		/// <param name="left">The first operand.</param>
		/// <param name="right">The second operand.</param>
		/// <returns>False if both operands point to the same data, and have the same criteria for its dimensions.</returns>
		public static bool operator !=(TexelArray1D<TTexel> left, TexelArray1D<TTexel> right) {
			return !left.Equals(right);
		}
	}

	/// <summary>
	/// Provides a 2-dimensional view onto a flat texel array.
	/// </summary>
	/// <typeparam name="TTexel">The texel format.</typeparam>
	public struct TexelArray2D<TTexel> : IEnumerable<TTexel>, IEquatable<TexelArray2D<TTexel>>
		where TTexel : struct, ITexel {
		/// <summary>
		/// The underlying flat texel array.
		/// </summary>
		public readonly TTexel[] Data;

		/// <summary>
		/// The number of texels in any row of this data.
		/// </summary>
		public readonly uint Width;

		/// <summary>
		/// The number of rows of texels in this data.
		/// </summary>
		public readonly uint Height;

		/// <summary>
		/// Creates a new 2D texel array with the given data.
		/// </summary>
		/// <param name="data">The data to encapsulate. Must not be null.</param>
		/// <param name="texWidthTx">The number of texels in a single row.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0",
			Justification = "The parameter is checked by the assurance.")]
		public TexelArray2D(TTexel[] data, uint texWidthTx) {
			Assure.NotNull(data);
			this.Data = data;
			this.Width = texWidthTx;
			this.Height = (uint) data.Length / Width;
		}

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
		/// </returns>
		/// <filterpriority>1</filterpriority>
		public IEnumerator<TTexel> GetEnumerator() {
			return (Data as IEnumerable<TTexel>).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		/// <summary>
		/// Gets or sets the texel at the specified index.
		/// </summary>
		/// <param name="u">The x-coordinate of the texel to get.</param>
		/// <param name="v">The y-coordinate of the texel to get.</param>
		/// <returns>The <typeparamref name="TTexel"/> at the requested co-ordinates.</returns>
		public TTexel this[int u, int v] {
			get {
				Assure.LessThan(u, Width, "Index out of bounds: u");
				Assure.LessThan(v, Height, "Index out of bounds: v");
				return Data[u + (Width * v)];
			}
			set {
				Assure.LessThan(u, Width, "Index out of bounds: u");
				Assure.LessThan(v, Height, "Index out of bounds: v");
				Data[u + (Width * v)] = value;
			}
		}

		/// <summary>
		/// Gets or sets the texel at the specified index.
		/// </summary>
		/// <param name="u">The x-coordinate of the texel to get.</param>
		/// <param name="v">The y-coordinate of the texel to get.</param>
		/// <returns>The <typeparamref name="TTexel"/> at the requested co-ordinates.</returns>
		public TTexel this[uint u, uint v] {
			get {
				Assure.LessThan(u, Width, "Index out of bounds: u");
				Assure.LessThan(v, Height, "Index out of bounds: v");
				return Data[u + (Width * v)];
			}
			set {
				Assure.LessThan(u, Width, "Index out of bounds: u");
				Assure.LessThan(v, Height, "Index out of bounds: v");
				Data[u + (Width * v)] = value;
			}
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(TexelArray2D<TTexel> other) {
			return Equals(Data, other.Data) && Width == other.Width && Height == other.Height;
		}

		/// <summary>
		/// Indicates whether this instance and a specified object are equal.
		/// </summary>
		/// <returns>
		/// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false. 
		/// </returns>
		/// <param name="obj">The object to compare with the current instance. </param><filterpriority>2</filterpriority>
		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) {
				return false;
			}
			return obj is TexelArray2D<TTexel> && Equals((TexelArray2D<TTexel>)obj);
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>
		/// A 32-bit signed integer that is the hash code for this instance.
		/// </returns>
		/// <filterpriority>2</filterpriority>
		public override int GetHashCode() {
			unchecked {
				int hashCode = (Data != null ? Data.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (int)Width;
				hashCode = (hashCode * 397) ^ (int)Height;
				return hashCode;
			}
		}

		/// <summary>
		/// Indicates whether or not the two texel arrays refer to the same data, with the same representation of that data.
		/// </summary>
		/// <param name="left">The first operand.</param>
		/// <param name="right">The second operand.</param>
		/// <returns>True if both operands point to the same data, and have the same criteria for its dimensions.</returns>
		public static bool operator ==(TexelArray2D<TTexel> left, TexelArray2D<TTexel> right) {
			return left.Equals(right);
		}

		/// <summary>
		/// Indicates whether or not the two texel arrays refer to the same data, with the same representation of that data.
		/// </summary>
		/// <param name="left">The first operand.</param>
		/// <param name="right">The second operand.</param>
		/// <returns>False if both operands point to the same data, and have the same criteria for its dimensions.</returns>
		public static bool operator !=(TexelArray2D<TTexel> left, TexelArray2D<TTexel> right) {
			return !left.Equals(right);
		}
	}

	/// <summary>
	/// Provides a 3-dimensional view onto a flat texel array.
	/// </summary>
	/// <typeparam name="TTexel">The texel format.</typeparam>
	public struct TexelArray3D<TTexel> : IEnumerable<TTexel>, IEquatable<TexelArray3D<TTexel>>
		where TTexel : struct, ITexel {
		/// <summary>
		/// The underlying flat texel array.
		/// </summary>
		public readonly TTexel[] Data;

		/// <summary>
		/// The number of texels in any row of this data.
		/// </summary>
		public readonly uint Width;

		/// <summary>
		/// The number of rows of in any slice of this data.
		/// </summary>
		public readonly uint Height;

		/// <summary>
		/// The number of slices of texels in this data.
		/// </summary>
		public readonly uint Depth;
		private readonly uint texHeightTimesTexWidthTx;

		/// <summary>
		/// Creates a new 3D texel array with the given data.
		/// </summary>
		/// <param name="data">The data to encapsulate. Must not be null.</param>
		/// <param name="texWidthTx">The number of texels in a single row.</param>
		/// <param name="texHeightTx">The number of rows in a single slice.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0",
			Justification = "The parameter is checked by the assurance.")]
		public TexelArray3D(TTexel[] data, uint texWidthTx, uint texHeightTx) {
			Assure.NotNull(data);
			this.Data = data;
			this.Width = texWidthTx;
			this.Height = texHeightTx;
			this.texHeightTimesTexWidthTx = texHeightTx * texWidthTx;
			this.Depth = (uint) data.Length / texHeightTimesTexWidthTx;
		}

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
		/// </returns>
		/// <filterpriority>1</filterpriority>
		public IEnumerator<TTexel> GetEnumerator() {
			return (Data as IEnumerable<TTexel>).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		/// <summary>
		/// Gets or sets the texel at the specified index.
		/// </summary>
		/// <param name="u">The x-coordinate of the texel to get.</param>
		/// <param name="v">The y-coordinate of the texel to get.</param>
		/// <param name="w">The z-coordinate of the texel to get.</param>
		/// <returns>The <typeparamref name="TTexel"/> at the requested co-ordinates.</returns>
		public TTexel this[int u, int v, int w] {
			get {
				Assure.LessThan(u, Width, "Index out of bounds: u");
				Assure.LessThan(v, Height, "Index out of bounds: v");
				Assure.LessThan(w, Depth, "Index out of bounds: w");
				return Data[u + (Width * v) + (texHeightTimesTexWidthTx * w)];
			}
			set {
				Assure.LessThan(u, Width, "Index out of bounds: u");
				Assure.LessThan(v, Height, "Index out of bounds: v");
				Assure.LessThan(w, Depth, "Index out of bounds: w");
				Data[u + (Width * v) + (texHeightTimesTexWidthTx * w)] = value;
			}
		}
		/// <summary>
		/// Gets or sets the texel at the specified index.
		/// </summary>
		/// <param name="u">The x-coordinate of the texel to get.</param>
		/// <param name="v">The y-coordinate of the texel to get.</param>
		/// <param name="w">The z-coordinate of the texel to get.</param>
		/// <returns>The <typeparamref name="TTexel"/> at the requested co-ordinates.</returns>
		public TTexel this[uint u, uint v, uint w] {
			get {
				Assure.LessThan(u, Width, "Index out of bounds: u");
				Assure.LessThan(v, Height, "Index out of bounds: v");
				Assure.LessThan(w, Depth, "Index out of bounds: w");
				return Data[u + (Width * v) + (texHeightTimesTexWidthTx * w)];
			}
			set {
				Assure.LessThan(u, Width, "Index out of bounds: u");
				Assure.LessThan(v, Height, "Index out of bounds: v");
				Assure.LessThan(w, Depth, "Index out of bounds: w");
				Data[u + (Width * v) + (texHeightTimesTexWidthTx * w)] = value;
			}
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(TexelArray3D<TTexel> other) {
			return Equals(Data, other.Data) && Width == other.Width && Height == other.Height && Depth == other.Depth;
		}

		/// <summary>
		/// Indicates whether this instance and a specified object are equal.
		/// </summary>
		/// <returns>
		/// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false. 
		/// </returns>
		/// <param name="obj">The object to compare with the current instance. </param><filterpriority>2</filterpriority>
		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) {
				return false;
			}
			return obj is TexelArray3D<TTexel> && Equals((TexelArray3D<TTexel>)obj);
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>
		/// A 32-bit signed integer that is the hash code for this instance.
		/// </returns>
		/// <filterpriority>2</filterpriority>
		public override int GetHashCode() {
			unchecked {
				int hashCode = (Data != null ? Data.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (int)Width;
				hashCode = (hashCode * 397) ^ (int)Height;
				hashCode = (hashCode * 397) ^ (int)Depth;
				return hashCode;
			}
		}

		/// <summary>
		/// Indicates whether or not the two texel arrays refer to the same data, with the same representation of that data.
		/// </summary>
		/// <param name="left">The first operand.</param>
		/// <param name="right">The second operand.</param>
		/// <returns>True if both operands point to the same data, and have the same criteria for its dimensions.</returns>
		public static bool operator ==(TexelArray3D<TTexel> left, TexelArray3D<TTexel> right) {
			return left.Equals(right);
		}

		/// <summary>
		/// Indicates whether or not the two texel arrays refer to the same data, with the same representation of that data.
		/// </summary>
		/// <param name="left">The first operand.</param>
		/// <param name="right">The second operand.</param>
		/// <returns>False if both operands point to the same data, and have the same criteria for its dimensions.</returns>
		public static bool operator !=(TexelArray3D<TTexel> left, TexelArray3D<TTexel> right) {
			return !left.Equals(right);
		}
	}
}