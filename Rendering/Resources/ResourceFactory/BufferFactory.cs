// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 27 10 2014 at 12:14 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// Static class that provides various <see cref="IBuffer">buffer</see> <see cref="IResourceBuilder">resource builder</see>s.
	/// </summary>
	public static class BufferFactory {
		/// <summary>
		/// Creates and returns a new <see cref="VertexBufferBuilder{TVertex}"/>.
		/// </summary>
		/// <typeparam name="TVertex">A <see cref="Extensions.IsBlittable">blittable</see> struct type that represents a single vertex.
		/// </typeparam>
		/// <returns>A new <see cref="VertexBufferBuilder{TVertex}"/> that can be used 
		/// to create <see cref="VertexBuffer{TVertex}"/> objects.
		/// </returns>
		public static VertexBufferBuilder<TVertex> NewVertexBuffer<TVertex>() where TVertex : struct {
			return new VertexBufferBuilder<TVertex>();
		}

		/// <summary>
		/// Creates and returns a new <see cref="IndexBufferBuilder"/>.
		/// </summary>
		/// <returns>A new <see cref="IndexBufferBuilder"/> that can be used 
		/// to create <see cref="IndexBuffer"/> objects.
		/// </returns>
		public static IndexBufferBuilder NewIndexBuffer() {
			return new IndexBufferBuilder();
		}

		/// <summary>
		/// Creates and returns a new <see cref="ConstantBufferBuilder{TConstants}"/>.
		/// </summary>
		/// <typeparam name="TConstants">A <see cref="Extensions.IsBlittable">blittable</see> struct type that represents the data
		/// that can be set in the created <see cref="ConstantBuffer{TConstants}"/>.
		/// </typeparam>
		/// <returns>A new <see cref="ConstantBufferBuilder{TConstants}"/> that can be used 
		/// to create <see cref="ConstantBuffer{TConstants}"/> objects.
		/// </returns>
		public static ConstantBufferBuilder<TConstants> NewConstantBuffer<TConstants>() where TConstants : struct {
			return new ConstantBufferBuilder<TConstants>();
		}

		/// <summary>
		/// Creates and returns a new <see cref="BufferBuilder{TElement}"/>.
		/// </summary>
		/// <typeparam name="TElement">A <see cref="Extensions.IsBlittable">blittable</see> struct type that represents a single element
		/// in the buffer.
		/// </typeparam>
		/// <returns>A new <see cref="BufferBuilder{TElement}"/> that can be used 
		/// to create <see cref="Buffer{TElement}"/> objects.
		/// </returns>
		public static BufferBuilder<TElement> NewBuffer<TElement>() where TElement : struct {
			return new BufferBuilder<TElement>();
		}
	}
}