// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 03 02 2015 at 14:04 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// An <see cref="IShaderResourceBinding"/> that facilitates setting and retrieving values for a preset <see cref="IConstantBuffer"/> on the
	/// owning shader.
	/// </summary>
	/// <remarks>
	/// ConstantBufferBindings work slightly differently to other <see cref="IShaderResourceBinding"/>s, because they require the
	/// resource to be set at construction, and do not allow it to be set again. Instead, the value of the bound constant buffer may be
	/// set and retrieved.
	/// </remarks>
	public sealed class ConstantBufferBinding : BaseShaderResourceBinding<IConstantBuffer>, IDisposable {
		/// <summary>
		/// A pointer to the current value set on this binding.
		/// </summary>
		/// <remarks>
		/// The pointer is guaranteed to be valid and will point to a value of
		/// size <see cref="BufferSizeBytes"/>, as long as the owning shader (and this binding) has not been disposed.
		/// </remarks>
		public readonly IntPtr CurValuePtr;
		/// <summary>
		/// The size of the value that is represented by the bound <see cref="IConstantBuffer"/>, in bytes.
		/// </summary>
		public readonly uint BufferSizeBytes;
		private int isDisposed = 0; // Bool (0/1), as an int to allow usage in Interlocked.CMPXCHG

		internal bool IsDisposed {
			get {
				Thread.MemoryBarrier();
				bool result = isDisposed == 1;
				Thread.MemoryBarrier();
				return result;
			}
		}

		/// <summary>
		/// Used by the <see cref="BaseShaderResourceBinding{T}.ToString"/> implementation in <see cref="BaseShaderResourceBinding{TResource}"/>.
		/// </summary>
		/// <returns>Returns "cb".</returns>
		protected override string RegisterIdentifier {
			get {
				return "cb";
			}
		}
		
		/// <summary>
		/// Creates a new <see cref="IConstantBuffer"/> binding.
		/// </summary>
		/// <param name="slotIndex">The constant buffer register this binding applies to.</param>
		/// <param name="identifier">The unique identifier for this constant buffer binding.</param>
		/// <param name="buffer">The buffer to permanently bind through this binding. Must not be null.</param>
		public ConstantBufferBinding(uint slotIndex, string identifier, IConstantBuffer buffer) : base(slotIndex, identifier) {
			if (buffer == null) throw new ArgumentNullException("buffer");
			BufferSizeBytes = buffer.ElementSizeBytes;
			CurValuePtr = Marshal.AllocHGlobal(new IntPtr(BufferSizeBytes));
			base.Bind(buffer);
		}

#if DEBUG
		~ConstantBufferBinding() {
			if (!IsDisposed) Logger.Warn(this + " was not disposed before finalization!");
		}
#endif

		/// <summary>
		/// Sets the value for the constant buffer represented by this binding.
		/// </summary>
		/// <param name="value">The value (serialized as a byte array) to set. The <see cref="ArraySlice{T}.Length"/>
		/// must be equal to <see cref="BufferSizeBytes"/>.</param>
		/// <seealso cref="SetValue(byte*)"/>
		public void SetValue(ArraySlice<byte> value) {
			Assure.Equal(value.Length, BufferSizeBytes, "Array length not equal to size of buffer.");
			UnsafeUtils.CopyGenericArray(value, CurValuePtr, sizeof(byte));
		}

		/// <summary>
		/// Sets the value for the constant buffer represented by this binding.
		/// </summary>
		/// <param name="value">The value (serialized as a byte array) to set. <see cref="BufferSizeBytes"/> will be read
		/// from the given pointer and copied to the current value.</param>
		/// <seealso cref="SetValue(Ophidian.Losgap.ArraySlice{byte})"/>
		public unsafe void SetValue(byte* value) {
			byte* destPtr = (byte*) CurValuePtr;
			for (int i = 0; i < BufferSizeBytes; ++i) {
				destPtr[i] = value[i];
			}
		}

		/// <summary>
		/// Bind the given <paramref name="resource"/> to the owning shader at the given 
		/// <see cref="IShaderResourceBinding.SlotIndex"/>/<see cref="IShaderResourceBinding.Identifier"/>.
		/// </summary>
		/// <param name="resource">The resource to bind. May be null (to 'unbind' the resource slot).</param>
		public override void Bind(IConstantBuffer resource) {
			throw new InvalidOperationException("Can not change bound constant buffer. Use SetValue instead.");
		}

		void IDisposable.Dispose() {
			if (Interlocked.CompareExchange(ref isDisposed, 1, 0) == 0) {
				Marshal.FreeHGlobal(CurValuePtr);
			}
		}
	}
}