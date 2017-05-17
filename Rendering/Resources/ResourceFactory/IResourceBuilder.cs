using System;
using System.Linq;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// Resource builders are used to specify the parameters for <see cref="IResource"/>s, thus allowing multiple resources to be created
	/// with similar attributes.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Resource builders offer a 'fluent' approach to altering and generating resources. As an example, when cloning a
	/// <see cref="Texture2D{TTexel}"/>, the <see cref="Texture2D{TTexel}.Clone"/> method returns a <see cref="Texture2DBuilder{TTexel}"/>.
	/// This allows you to adjust parameters on the clone before creating it, e.g.:
	/// <code>
	/// Texture2D&lt;TexelFormat.RGBA8UInt&gt; sourceTex = GetSourceTex();
	/// Texture2D&lt;TexelFormat.RGBA8UInt&gt; cloneTex = sourceTex.Clone()
	///		.WithUsage(ResourceUsage.StagingReadWrite)
	///		.WithInitialData(GetInitialData())
	///		.WithMipAllocation(false);
	/// </code>
	/// </para>
	/// <para>
	/// Although this interface exposes a handful of generic methods for manipulating builders, it is advised to use the narrowed type
	/// that you need (e.g. use a <see cref="VertexBufferBuilder{TVertex}"/> explicitly, instead of accessing one through the 
	/// <see cref="IResourceBuilder"/> interface).
	/// </para>
	/// <para>
	/// Unless otherwise stated, resource builders are always immutable; meaning that they can safely be shared across threads and other
	/// parts of your application.
	/// </para>
	/// </remarks>
	public interface IResourceBuilder {
		/// <summary>
		/// Sets the <see cref="ResourceUsage"/> that resources created with this builder will have applied.
		/// </summary>
		/// <param name="usage">The usage type. Not all resources support all usages. All resources have a 
		/// <see cref="SupportedUsagesAttribute"/> applied that provides more information.</param>
		/// <returns>A new <see cref="IResourceBuilder"/> with the usage set to <paramref name="usage"/>.</returns>
		IResourceBuilder WithUsage(ResourceUsage usage);
		/// <summary>
		/// Sets the initial data for the created resource(s). The data must be of a valid format for the actual type of builder
		/// that you are accessing. If you do not provide a valid object, an exception may be thrown.
		/// </summary>
		/// <param name="data">The data to set. May be null where applicable.</param>
		/// <returns>A new <see cref="IResourceBuilder"/> with the initial data set to <paramref name="data"/>.</returns>
		IResourceBuilder WithInitialData(object data);
		/// <summary>
		/// Creates a new <see cref="IResource"/> according to the parameters set on this builder.
		/// </summary>
		/// <returns>An <see cref="IResource"/> according to the type of builder this is.</returns>
		IResource Create();
	}
}