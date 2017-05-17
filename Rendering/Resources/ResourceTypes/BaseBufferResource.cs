// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 06 11 2014 at 15:31 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// Abstract class that provides some common functionality for buffer resources.
	/// </summary>
	public unsafe abstract class BaseBufferResource : BaseResource, IBuffer {
		private readonly Type elementType;
		private readonly uint elementSizeBytes;
		private readonly uint length;

		/// <summary>
		/// The element type of each element in this buffer.
		/// </summary>
		public Type ElementType {
			get { return elementType; }
		}

		/// <summary>
		/// The size, in bytes, of a single element in this buffer.
		/// </summary>
		public uint ElementSizeBytes {
			get { return elementSizeBytes; }
		}

		/// <summary>
		/// The number of elements in this buffer.
		/// </summary>
		public uint Length {
			get { return length; }
		}

		internal BaseBufferResource(ResourceHandle resourceHandle, ResourceUsage usage, ByteSize size, 
			Type elementType, uint elementSizeBytes, uint length)
			: base(resourceHandle, usage, size) {
			Assure.NotNull(elementType);
			this.elementType = elementType;
			this.elementSizeBytes = elementSizeBytes;
			this.length = length;
		}

		/// <summary>
		/// Performs a <see cref="ResourceUsage.DiscardWrite"/> on this buffer at the given <paramref name="writeOffset"/>;
		/// by copying <paramref name="numBytesToWrite"/> from <paramref name="data"/>.
		/// </summary>
		/// <param name="data">Where to copy the data from to the resource.</param>
		/// <param name="numBytesToWrite">The number of bytes to write.</param>
		/// <param name="writeOffset">The first byte in the buffer to begin writing to.</param>
		/// <exception cref="ResourceOperationUnavailableException">Thrown if <see cref="BaseResource.CanDiscardWrite"/>
		/// is <c>false</c>.</exception>
		protected void MapDiscardWrite(IntPtr data, uint numBytesToWrite, uint writeOffset) {
			Assure.LessThanOrEqualTo(
				numBytesToWrite + writeOffset,
				Size.InBytes,
				"Buffer overrun. Please ensure that you are not attempting to write past the end of the resource."
			);

			ThrowIfCannotDiscardWrite();
			Mutate_MapWrite(data, numBytesToWrite, writeOffset, ResourceMapping.WriteDiscard);
		}

		/// <summary>
		/// Performs a <see cref="ResourceUsage.Write"/> on this buffer at the given <paramref name="writeOffset"/>;
		/// by copying <paramref name="numBytesToWrite"/> from <paramref name="data"/>.
		/// </summary>
		/// <param name="data">Where to copy the data from to the resource.</param>
		/// <param name="numBytesToWrite">The number of bytes to write.</param>
		/// <param name="writeOffset">The first byte in the buffer to begin writing to.</param>
		/// <exception cref="ResourceOperationUnavailableException">Thrown if <see cref="BaseResource.CanWrite"/>
		/// is <c>false</c>.</exception>
		protected void MapWrite(IntPtr data, uint numBytesToWrite, uint writeOffset) {
			Assure.LessThanOrEqualTo(
				numBytesToWrite + writeOffset,
				Size.InBytes,
				"Buffer overrun. Please ensure that you are not attempting to write past the end of the resource."
			);

			ThrowIfCannotWrite();
			LosgapSystem.InvokeOnMasterAsync(() => Mutate_MapWrite(data, numBytesToWrite, writeOffset, ResourceMapping.Write));
		}

		/// <summary>
		/// Performs a <see cref="ResourceUsage.Write"/> on this buffer at the given region 
		/// (as bounded by the <paramref name="writeStartBytes"/> and <paramref name="writeEndBytes"/> parameters).
		/// </summary>
		/// <param name="data">Pointer to the data to copy. <c>writeEndBytes - writeStartBytes</c> bytes will be copied.</param>
		/// <param name="writeStartBytes">The first byte in the buffer to begin writing to.</param>
		/// <param name="writeEndBytes">The first byte past the end of the region to write to.</param>
		protected void UpdateSubregion(IntPtr data, uint writeStartBytes, uint writeEndBytes) {
			ThrowIfCannotWrite();
			Mutate_UpdateSubresourceRegion(data, new SubresourceBox(writeStartBytes, writeEndBytes));
		}

		/// <summary>
		/// Assists in performing a <see cref="ResourceUsage.StagingRead"/> on this buffer by mapping it to CPU memory
		/// and providing a pointer to the data.
		/// </summary>
		/// <param name="mappedDataPtrAction">An action that usually will copy the data from the given pointer to 
		/// an appropriate managed structure. The supplied <see cref="IntPtr"/> is only valid for as long as the supplied
		/// <see cref="Action"/> is running. The data pointer may ONLY be read from, not written to.</param>
		protected void MapRead(Action<IntPtr> mappedDataPtrAction) {
			ThrowIfCannotRead();
			LosgapSystem.InvokeOnMaster(() => Mutate_MapRead(mappedDataPtrAction, ResourceMapping.Read));
		}

		/// <summary>
		/// Assists in performing a <see cref="ResourceUsage.StagingReadWrite"/> on this buffer by mapping it to CPU memory
		/// and providing a pointer to the data.
		/// </summary>
		/// <param name="mappedDataPtrAction">An action that usually will modify (read/write) the data at the given pointer.
		/// The supplied <see cref="IntPtr"/> is only valid for as long as the supplied
		/// <see cref="Action"/> is running.</param>
		protected void MapReadWrite(Action<IntPtr> mappedDataPtrAction) {
			ThrowIfCannotReadWrite();
			LosgapSystem.InvokeOnMaster(() => Mutate_MapRead(mappedDataPtrAction, ResourceMapping.ReadWrite));
		}

		void IBuffer.DiscardWrite(ArraySlice<byte> data, uint offset) {
			GCHandle pinnedDataHandle = GCHandle.Alloc(data.ContainingArray, GCHandleType.Pinned);

			try {
				MapDiscardWrite(pinnedDataHandle.AddrOfPinnedObject() + (int) data.Offset, data.Length, offset);
			}
			finally {
				pinnedDataHandle.Free();
			}
		}

		void IBuffer.Write(ArraySlice<byte> data, uint offset) {
			GCHandle pinnedDataHandle = GCHandle.Alloc(data.ContainingArray, GCHandleType.Pinned);

			try {
				if (Usage.ShouldUpdateSubresourceRegion()) {
					UpdateSubregion(
						pinnedDataHandle.AddrOfPinnedObject() + (int) data.Offset,
						offset, 
						offset + data.Length
					);
				}
				else {
					MapWrite(
						pinnedDataHandle.AddrOfPinnedObject() + (int) data.Offset,
						data.Length,
						offset
					);
				}

			}
			finally {
				pinnedDataHandle.Free();
			}
		}

		byte[] IBuffer.Read() {
			return LosgapSystem.InvokeOnMaster(() => {
				byte[] result = new byte[Size];
				Mutate_MapRead(
					data => UnsafeUtils.CopyGenericArray<byte>(data, result, sizeof(byte)),
					ResourceMapping.Read
					);
				return result;
			});
		}

		void IBuffer.ReadWrite(Action<byte[]> readWriteAction) {
			LosgapSystem.InvokeOnMaster(() =>
				Mutate_MapRead(
					dataAsPtr => {
						byte[] dataAsArray = new byte[Size];
						UnsafeUtils.CopyGenericArray<byte>(dataAsPtr, dataAsArray, sizeof(byte));
						readWriteAction(dataAsArray);
						UnsafeUtils.CopyGenericArray<byte>(dataAsArray, dataAsPtr, sizeof(byte));
					},
					ResourceMapping.ReadWrite
				)
			);
		}

		private void Mutate_MapWrite(IntPtr dataPtr, uint numBytesToWrite, uint writeOffset, ResourceMapping writeType) {
			lock (InstanceMutationLock) {
				if (IsDisposed) {
					Logger.Warn("Attempted write manipulation on disposed resource of type: " + GetType().Name);
					return;
				}
				IntPtr outDataPtr;
				uint outUnused;

				char* failReason = stackalloc char[InteropUtils.MAX_INTEROP_FAIL_REASON_STRING_LENGTH + 1];
				bool success = NativeMethods.ResourceFactory_MapSubresource(
					(IntPtr) failReason, 
					RenderingModule.DeviceContext,
					ResourceHandle,
					0U,
					writeType,
					(IntPtr) (&outDataPtr),
					(IntPtr) (&outUnused),
					(IntPtr) (&outUnused)
				);
				if (!success) throw new NativeOperationFailedException(Marshal.PtrToStringUni((IntPtr) failReason));

				UnsafeUtils.MemCopy(dataPtr, outDataPtr + (int) writeOffset, numBytesToWrite);

				char* failReason2 = stackalloc char[InteropUtils.MAX_INTEROP_FAIL_REASON_STRING_LENGTH + 1];
				bool success2 = NativeMethods.ResourceFactory_UnmapSubresource(
					(IntPtr) failReason,
					RenderingModule.DeviceContext,
					ResourceHandle,
					0U
				);
				if (!success2) throw new NativeOperationFailedException(Marshal.PtrToStringUni((IntPtr) failReason2));
			}
		}

		private void Mutate_UpdateSubresourceRegion(IntPtr data, SubresourceBox updateBox) {
			lock (InstanceMutationLock) {
				if (IsDisposed) {
					Logger.Warn("Attempted write manipulation on disposed resource of type: " + GetType().Name);
					return;
				}
				InteropUtils.CallNative(
					NativeMethods.ResourceFactory_UpdateSubresourceRegion,
					RenderingModule.DeviceContext,
					ResourceHandle,
					0U,
					(IntPtr) (&updateBox),
					data,
					(uint) Size,
					(uint) Size
				).ThrowOnFailure();
			}
		}

		private void Mutate_MapRead(Action<IntPtr> mappedDataReadAction, ResourceMapping readType) {
			lock (InstanceMutationLock) {
				if (IsDisposed) {
					Logger.Warn("Attempted read manipulation on disposed resource of type: " + GetType().Name);
					return;
				}
				IntPtr outDataPtr;
				uint outUnused;
				InteropUtils.CallNative(
					NativeMethods.ResourceFactory_MapSubresource,
					RenderingModule.DeviceContext,
					ResourceHandle,
					0U,
					readType,
					(IntPtr) (&outDataPtr),
					(IntPtr) (&outUnused),
					(IntPtr) (&outUnused)
				).ThrowOnFailure();

				try {
					mappedDataReadAction(outDataPtr);
				}
				finally {
					InteropUtils.CallNative(
						NativeMethods.ResourceFactory_UnmapSubresource,
						RenderingModule.DeviceContext,
						ResourceHandle,
						0U
					).ThrowOnFailure();
				}
			}
		}
	}
}