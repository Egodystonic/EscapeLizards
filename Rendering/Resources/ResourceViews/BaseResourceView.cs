// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 12 11 2014 at 10:04 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// The base class of all resource views. Provides some common functionality for all implementing members.
	/// </summary>
	public abstract class BaseResourceView : IResourceView {
		private readonly object instanceMutationLock = new object();
		private readonly ResourceViewHandle resourceViewHandle;
		private readonly IResource resource;
		private bool isDisposed = false;

		public ResourceViewHandle ResourceViewHandle {
			get { return resourceViewHandle; }
		}

		/// <summary>
		/// The resource that this view is connected to. 
		/// </summary>
		public IResource Resource {
			get { return resource; }
		}

		/// <summary>
		/// Whether or not this resource view has been disposed.
		/// </summary>
		public bool IsDisposed {
			get {
				lock (instanceMutationLock) {
					return isDisposed;
				}
			}
		}

		/// <summary>
		/// True if this resource view, its <see cref="IResourceView.Resource"/>, or both have been disposed.
		/// </summary>
		public bool ResourceOrViewDisposed {
			get {
				lock (instanceMutationLock) {
					return isDisposed || resource.IsDisposed;
				}
			}
		}

		internal BaseResourceView(ResourceViewHandle resourceViewHandle, IResource resource) {
			this.resourceViewHandle = resourceViewHandle;
			this.resource = resource;
		}

#if DEBUG
		~BaseResourceView() {
			if (!IsDisposed) Logger.Warn("Resource view of type '" + GetType().Name + "' was not disposed before being garbage collected.");
		}
#endif

		/// <summary>
		/// Disposes this view; making it unavailable for further use, and releasing unmanaged resources associated with it.
		/// Does not dispose the <see cref="Resource"/>.
		/// </summary>
		public void Dispose() {
			lock (instanceMutationLock) {
				if (isDisposed) return;
				isDisposed = true;

				InteropUtils.CallNative(
					NativeMethods.ResourceFactory_ReleaseResource,
					resourceViewHandle
				).ThrowOnFailure();
			}
		}
	}
}