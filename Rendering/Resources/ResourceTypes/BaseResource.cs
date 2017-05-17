// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 27 10 2014 at 11:28 by Ben Bowen

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// An abstract class that provides an implementation of common functionality defined by <see cref="IResource"/>.
	/// </summary>
	public unsafe abstract class BaseResource : IResource {
		private static readonly object staticMutationLock = new object();
		private static readonly Dictionary<string, uint> resourceLeakCounter = new Dictionary<string, uint>();

		public static void PrintResourceCounts() {
			lock (staticMutationLock) {
				Logger.Log("Printing resource counts:");
				foreach (var kvp in resourceLeakCounter) {
					Logger.Log("\t" + kvp.Key + ": " + kvp.Value);
				}
			}
		}

		/// <summary>
		/// A string used to separate parameter values when creating a string for <see cref="ToString"/>.
		/// </summary>
		protected const string DESC_VALUE_SEPARATOR = ", ";
		/// <summary>
		/// A string used to separate the resource type from its parameter values when creating a string for <see cref="ToString"/>.
		/// </summary>
		protected const string DESC_TYPE_SEPARATOR = ": ";
		private readonly ResourceHandle resourceHandle;
		/// <summary>
		/// A lock object that must be synchronized on before mutating or reading the underlying resource data or state.
		/// </summary>
		protected readonly object InstanceMutationLock = new object();
		private readonly ResourceUsage usage;
		private readonly ByteSize size;
		private bool isDisposed = false;

		/// <summary>
		/// Gets a handle used to internally identify this resource.
		/// </summary>
		public unsafe ResourceHandle ResourceHandle {
			get { return resourceHandle; }
		}

		/// <summary>
		/// The <see cref="ResourceUsage"/> that determines the set of permissible operations for manipulating this resource.
		/// </summary>
		/// <remarks>
		/// Depending on this value, the data represented by the resource may or may not be written to, overwritten, or
		/// read back from the GPU. However, resources should be created with as restrictive a usage as possible for maximum performance
		/// (see also: <see cref="ResourceUsage"/>).
		/// <para>
		/// You can accurately determine whether a specific manipulation is supported on this resource by querying the 
		/// <c>CanXXX</c> properties (<see cref="IResource.CanDiscardWrite"/>, <see cref="IResource.CanWrite"/>, <see cref="IResource.CanRead"/>, 
		/// <see cref="IResource.CanReadWrite"/>, <see cref="IResource.CanBeCopyDestination"/>).
		/// </para>
		/// </remarks>
		public ResourceUsage Usage {
			get {
				return usage;
			}
		}

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
		public abstract GPUBindings PermittedBindings { get; }

		/// <summary>
		/// If <see cref="IDisposable.Dispose"/> has been called on this resource, this property will return <c>true</c>. Any data
		/// manipulations on disposed resources will fail with a warning log message.
		/// </summary>
		public bool IsDisposed {
			get {
				lock (InstanceMutationLock) {
					return isDisposed;
				}
			}
			internal set {
				lock (InstanceMutationLock) {
					isDisposed = value;
				}
			}
		}

		/// <summary>
		/// Gets the size of the data represented by this resource (including, where applicable, any mips or array elements).
		/// </summary>
		public ByteSize Size {
			get {
				return size;
			}
		}

		/// <summary>
		/// Returns true if the <see cref="IResource.Usage"/> for this resource permits invoking <c>DiscardWrite</c> operations on its data.
		/// </summary>
		public bool CanDiscardWrite {
			get { return usage.ShouldMapWriteDiscard(); }
		}

		/// <summary>
		/// Returns true if the <see cref="IResource.Usage"/> for this resource permits invoking <c>Write</c> operations on its data.
		/// </summary>
		public bool CanWrite {
			get { return usage.ShouldMapWrite() || usage.ShouldUpdateSubresourceRegion(); }
		}

		/// <summary>
		/// Returns true if the <see cref="IResource.Usage"/> for this resource permits invoking <c>Read</c> operations on its data.
		/// </summary>
		public bool CanRead {
			get { return usage.ShouldMapRead(); }
		}

		/// <summary>
		/// Returns true if the <see cref="IResource.Usage"/> for this resource permits invoking <c>ReadWrite</c> operations on its data.
		/// </summary>
		public bool CanReadWrite {
			get { return usage.ShouldMapReadWrite(); }
		}

		/// <summary>
		/// Returns true if the <see cref="IResource.Usage"/> for this resource permits invoking <c>Copy</c> operations from another resource on it.
		/// </summary>
		public bool CanBeCopyDestination {
			get { return usage.ShouldBeCopyResourceDestination() || usage.ShouldBeCopySubresourceRegionDestination(); }
		}

		internal BaseResource(ResourceHandle resourceHandle, ResourceUsage usage, ByteSize size) {
			this.resourceHandle = resourceHandle;
			this.usage = usage;
			this.size = size;

			lock (staticMutationLock) {
				var typeString = GetType().Name;
				if (resourceLeakCounter.ContainsKey(typeString)) resourceLeakCounter[typeString]++;
				else resourceLeakCounter[typeString] = 1U;
			}
		}

#if DEBUG
		~BaseResource() {
			if (!IsDisposed) Logger.Warn("Resource of type '" + GetType().Name + "' was not disposed before finalization: " + this + ".");
		}
#endif

		/// <summary>
		/// Disposes this resource; deleting its data from the graphics card, and invalidating any further usage. If the resource has
		/// already been disposed, no action is taken.
		/// </summary>
		public virtual void Dispose() {
			lock (InstanceMutationLock) {
				if (isDisposed) return;
				isDisposed = true;
				InteropUtils.CallNative(NativeMethods.ResourceFactory_ReleaseResource, ResourceHandle).ThrowOnFailure();

				lock (staticMutationLock) {
					var typeString = GetType().Name;
					resourceLeakCounter[typeString]--;
				}
			}
		}

		/// <summary>
		/// Returns a string that represents the current resource.
		/// </summary>
		/// <returns>
		/// A string that describes the resource by listing the various parameters it was created with, etc.
		/// </returns>
		public override string ToString() {
			return
				"[" +
				CreateResourceDescString() +
				Usage + (PermittedBindings != GPUBindings.None ? " + " + PermittedBindings : String.Empty) + DESC_VALUE_SEPARATOR +
				Size + DESC_VALUE_SEPARATOR +
				(IsDisposed ? "disposed" + DESC_VALUE_SEPARATOR : String.Empty) +
				ResourceHandle + "]";
				
		}

		/// <summary>
		/// Used by the <see cref="ToString"/> implementation in <see cref="BaseResource"/> to create a child-class-specific
		/// string that provides additional data about this resource.
		/// </summary>
		/// <returns>A string in the format DATUM1 + <see cref="DESC_VALUE_SEPARATOR"/> + DATUM2 +
		/// <see cref="DESC_VALUE_SEPARATOR"/> + DATUM3 etc</returns>
		protected abstract string CreateResourceDescString();

		/// <summary>
		/// Throws a <see cref="ResourceOperationUnavailableException"/> if <see cref="CanDiscardWrite"/> returns <c>false</c>.
		/// </summary>
		protected void ThrowIfCannotDiscardWrite() {
			if (!CanDiscardWrite) {
				throw new ResourceOperationUnavailableException("Can not discard-write to resource with usage '" + Usage + "'.");
			}
		}
		/// <summary>
		/// Throws a <see cref="ResourceOperationUnavailableException"/> if <see cref="CanWrite"/> returns <c>false</c>.
		/// </summary>
		protected void ThrowIfCannotWrite() {
			if (!CanWrite) {
				throw new ResourceOperationUnavailableException("Can not write to resource with usage '" + Usage + "'.");
			}
		}
		/// <summary>
		/// Throws a <see cref="ResourceOperationUnavailableException"/> if <see cref="CanRead"/> returns <c>false</c>.
		/// </summary>
		protected void ThrowIfCannotRead() {
			if (!CanRead) {
				throw new ResourceOperationUnavailableException("Can not read from resource with usage '" + Usage + "'.");
			}
		}
		/// <summary>
		/// Throws a <see cref="ResourceOperationUnavailableException"/> if <see cref="CanReadWrite"/> returns <c>false</c>.
		/// </summary>
		protected void ThrowIfCannotReadWrite() {
			if (!CanReadWrite) {
				throw new ResourceOperationUnavailableException("Can not read-write to resource with usage '" + Usage + "'.");
			}
		}
		/// <summary>
		/// Throws a <see cref="ResourceOperationUnavailableException"/> if <see cref="CanBeCopyDestination"/> returns <c>false</c>.
		/// </summary>
		protected void ThrowIfCannotBeCopyDestination() {
			if (!CanBeCopyDestination) {
				throw new ResourceOperationUnavailableException("Can not copy to resource with usage '" + Usage + "'.");
			}
		}

		/// <summary>
		/// Copies this resource to the destination resource.
		/// </summary>
		/// <param name="dest">The destination resource. Must not be null.</param>
		/// <exception cref="AssuranceFailedException">(Debug only) Thrown if both resources' <see cref="Size"/>s or 
		/// <see cref="object.GetType">type</see>s are not equal; or if <c>this == dest</c>.</exception>
		/// <exception cref="ResourceOperationUnavailableException">Thrown if dest returns <c>false</c> for 
		/// <see cref="CanBeCopyDestination"/>.</exception>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0",
			Justification = "The parameter is checked by the assurance.")]
		protected void CopyTo(BaseResource dest) {
			Assure.NotNull(dest);
			Assure.NotEqual(this, dest, "Can not copy to self.");
			Assure.Equal(Size, dest.Size, "Resources must be of equal size.");
			Assure.Equal(GetType(), dest.GetType(), "Resources must be of equal type.");

			dest.ThrowIfCannotBeCopyDestination();

			lock (InstanceMutationLock) {
				if (IsDisposed) {
					Logger.Warn("Attempted copy manipulation from disposed resource of type: " + GetType().Name);
					return;
				}
				lock (dest.InstanceMutationLock) {
					if (dest.IsDisposed) {
						Logger.Warn("Attempted copy manipulation to disposed resource of type: " + GetType().Name);
						return;
					}
					InteropUtils.CallNative(
						NativeMethods.ResourceFactory_CopyResource,
						RenderingModule.DeviceContext,
						ResourceHandle,
						dest.ResourceHandle
					).ThrowOnFailure();
				}
			}
		}

		/// <summary>
		/// Copies a subregion of this resource to a subregion of the destination resource.
		/// </summary>
		/// <param name="dest">The destination resource. May be the <c>this</c>, provided <paramref name="srcSubresourceIndex"/>
		/// and <paramref name="dstSubresourceIndex"/> are different.</param>
		/// <param name="srcBox">The region of this resource to copy.</param>
		/// <param name="srcSubresourceIndex">The subresource to copy data from.</param>
		/// <param name="dstSubresourceIndex">The subresource of <paramref name="dest"/> to copy data to.</param>
		/// <param name="dstX">The X-axis offset in <paramref name="dest"/> to start copying to.</param>
		/// <param name="dstY">The Y-axis offset in <paramref name="dest"/> to start copying to.</param>
		/// <param name="dstZ">The Z-axis offset in <paramref name="dest"/> to start copying to.</param>
		/// <exception cref="AssuranceFailedException">(Debug only) Thrown if <paramref name="srcSubresourceIndex"/> is equal
		/// to <paramref name="dstSubresourceIndex"/>.</exception>
		/// <exception cref="ResourceOperationUnavailableException">Thrown if dest returns <c>false</c> for 
		/// <see cref="CanBeCopyDestination"/>.</exception>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0",
			Justification = "The parameter is checked by the assurance.")]
		protected void CopyTo(BaseResource dest, SubresourceBox srcBox, uint srcSubresourceIndex, uint dstSubresourceIndex, 
			uint dstX, uint dstY, uint dstZ) {
			Assure.NotNull(dest);
			Assure.False(this == dest && srcSubresourceIndex == dstSubresourceIndex, "Can not copy to/from same subresource.");
			dest.ThrowIfCannotBeCopyDestination();

			lock (InstanceMutationLock) {
				if (IsDisposed) {
					Logger.Warn("Attempted copy manipulation from disposed resource of type: " + GetType().Name);
					return;
				}
				lock (dest.InstanceMutationLock) {
					if (IsDisposed) {
						Logger.Warn("Attempted copy manipulation to disposed resource of type: " + GetType().Name);
						return;
					}
					InteropUtils.CallNative(
					NativeMethods.ResourceFactory_CopySubresourceRegion,
					RenderingModule.DeviceContext,
					ResourceHandle,
					srcSubresourceIndex,
					(IntPtr) (&srcBox),
					dest.ResourceHandle,
					dstSubresourceIndex,
					dstX,
					dstY,
					dstZ
					).ThrowOnFailure();
				}
			}
		}

		internal static ResourceFormat GetFormatForType(Type type) {
			if (type == typeof(float)) return ResourceFormat.R32Float;
			if (type == typeof(byte)) return ResourceFormat.R8Uint;
			if (type == typeof(ushort)) return ResourceFormat.R16Uint;
			if (type == typeof(uint)) return ResourceFormat.R32Uint;
			if (type == typeof(sbyte)) return ResourceFormat.R8Sint;
			if (type == typeof(short)) return ResourceFormat.R16Sint;
			if (type == typeof(int)) return ResourceFormat.R32Sint;
			if (type == typeof(Vector4)) return ResourceFormat.R32G32B32A32Float;
			if (type == typeof(Vector3)) return ResourceFormat.R32G32B32Float;
			if (type == typeof(Vector2)) return ResourceFormat.R32G32Float;
			if (typeof(ITexel).IsAssignableFrom(type)) {
				return (ResourceFormat) TexelFormat.AllFormats[type].ResourceFormatIndex;
			}
			return ResourceFormat.Unknown;
		}

		void IResource.CopyTo(IResource dest) {
			CopyTo(dest as BaseResource);
		}
	}
}