// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 03 11 2014 at 13:15 by Ben Bowen

using System;
using System.Collections.Generic;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// Represents any 'blob' of data that can be stored on the graphics card and used by the graphics pipeline (including
	/// by shader code). Most common resources include <see cref="ITexture">texture</see>s and <see cref="IBuffer">buffer</see>s.
	/// <para>
	/// By default, resources are immutable, meaning they are inherently thread-safe, and can be shared across the application.
	/// In circumstances where data is manipulated (e.g. read/write), the LOSGAP framework makes sure that changes are propagated
	/// across threads correctly.
	/// </para>
	/// </summary>
	public unsafe interface IResource : IDisposable {
		/// <summary>
		/// Gets a handle used to internally identify this resource.
		/// </summary>
		ResourceHandle ResourceHandle { get; }

		/// <summary>
		/// The <see cref="ResourceUsage"/> that determines the set of permissible operations for manipulating this resource.
		/// </summary>
		/// <remarks>
		/// Depending on this value, the data represented by the resource may or may not be written to, overwritten, or
		/// read back from the GPU. However, resources should be created with as restrictive a usage as possible for maximum performance
		/// (see also: <see cref="ResourceUsage"/>).
		/// <para>
		/// You can accurately determine whether a specific manipulation is supported on this resource by querying the 
		/// <c>CanXXX</c> properties (<see cref="CanDiscardWrite"/>, <see cref="CanWrite"/>, <see cref="CanRead"/>, 
		/// <see cref="CanReadWrite"/>, <see cref="CanBeCopyDestination"/>).
		/// </para>
		/// </remarks>
		ResourceUsage Usage { get; }

		/// <summary>
		/// Gets the set of permitted bindings to the GPU for this resource that determine where in the rendering pipeline this resource
		/// may be used.
		/// </summary>
		/// <remarks>
		/// Depending on this value, resources may be bound to one or
		/// more parts of the rendering pipeline; and subsequently may have different optional use-cases. However, resources should be
		/// created only with the <see cref="GPUBindings"/> required for their intended application, for maximum performance (see also:
		/// <see cref="GPUBindings"/>).
		/// </remarks>
		GPUBindings PermittedBindings { get; }

		/// <summary>
		/// If <see cref="IDisposable.Dispose"/> has been called on this resource, this property will return <c>true</c>. Any data
		/// manipulations on disposed resources will fail with a warning log message.
		/// </summary>
		bool IsDisposed { get; }

		/// <summary>
		/// Gets the size of the data represented by this resource (including, where applicable, any mips or array elements).
		/// </summary>
		ByteSize Size { get; }

		/// <summary>
		/// Returns true if the <see cref="Usage"/> for this resource permits invoking <c>DiscardWrite</c> operations on its data.
		/// </summary>
		bool CanDiscardWrite { get; }
		/// <summary>
		/// Returns true if the <see cref="Usage"/> for this resource permits invoking <c>Write</c> operations on its data.
		/// </summary>
		bool CanWrite { get; }
		/// <summary>
		/// Returns true if the <see cref="Usage"/> for this resource permits invoking <c>Read</c> operations on its data.
		/// </summary>
		bool CanRead { get; }
		/// <summary>
		/// Returns true if the <see cref="Usage"/> for this resource permits invoking <c>ReadWrite</c> operations on its data.
		/// </summary>
		bool CanReadWrite { get; }
		/// <summary>
		/// Returns true if the <see cref="Usage"/> for this resource permits invoking <c>Copy</c> operations from another resource on it.
		/// </summary>
		bool CanBeCopyDestination { get; }

		/// <summary>
		/// Copies the entire data from this resource to another.
		/// </summary>
		/// <param name="dest">The destination resource. Must not be null.</param>
		/// <exception cref="AssuranceFailedException">(Only in Debug builds) Thrown if <paramref name="dest"/> is not of identical
		/// type and <see cref="Size"/> to this resource.</exception>
		/// <exception cref="ResourceOperationUnavailableException">Thrown if <paramref name="dest"/> returns <c>false</c> for 
		/// <see cref="CanBeCopyDestination"/>.</exception>
		void CopyTo(IResource dest);
	}
}