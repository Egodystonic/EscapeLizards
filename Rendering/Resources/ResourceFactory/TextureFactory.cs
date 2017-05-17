// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 27 10 2014 at 12:14 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// Static class that provides various <see cref="ITexture">texture</see> <see cref="IResourceBuilder">resource builder</see>s.
	/// </summary>
	public static class TextureFactory {
		/// <summary>
		/// Creates and returns a new <see cref="Texture1DBuilder{TTexel}"/>.
		/// </summary>
		/// <typeparam name="TTexel">The texel type of the new texture. See: <see cref="ITexel"/>.</typeparam>
		/// <returns>A new <see cref="Texture1DBuilder{TTexel}"/> that can be used to create <see cref="Texture1D{TTexel}"/> objects.
		/// </returns>
		public static Texture1DBuilder<TTexel> NewTexture1D<TTexel>() where TTexel : struct, ITexel {
			return new Texture1DBuilder<TTexel>();
		}

		/// <summary>
		/// Creates and returns a new <see cref="Texture2DBuilder{TTexel}"/>.
		/// </summary>
		/// <typeparam name="TTexel">The texel type of the new texture. See: <see cref="ITexel"/>.</typeparam>
		/// <returns>A new <see cref="Texture2DBuilder{TTexel}"/> that can be used to create <see cref="Texture2D{TTexel}"/> objects.
		/// </returns>
		public static Texture2DBuilder<TTexel> NewTexture2D<TTexel>() where TTexel : struct, ITexel {
			return new Texture2DBuilder<TTexel>();
		}

		
		public static Texture2DLoader LoadTexture2D() {
			return new Texture2DLoader();
		}

		/// <summary>
		/// Creates and returns a new <see cref="Texture3DBuilder{TTexel}"/>.
		/// </summary>
		/// <typeparam name="TTexel">The texel type of the new texture. See: <see cref="ITexel"/>.</typeparam>
		/// <returns>A new <see cref="Texture3DBuilder{TTexel}"/> that can be used to create <see cref="Texture3D{TTexel}"/> objects.
		/// </returns>
		public static Texture3DBuilder<TTexel> NewTexture3D<TTexel>() where TTexel : struct, ITexel {
			return new Texture3DBuilder<TTexel>();
		}
	}
}