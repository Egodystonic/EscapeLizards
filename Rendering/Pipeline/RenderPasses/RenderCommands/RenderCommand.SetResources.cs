// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 26 01 2015 at 18:16 by Ben Bowen

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Ophidian.Losgap.Rendering {
	public partial struct RenderCommand {
		/// <summary>
		/// Creates a render command that instructs the graphics pipeline to set the given vertex buffer as a model-instance transform buffer.
		/// </summary>
		/// <param name="instanceBuffer">The buffer to set. Must not be null or disposed.</param>
		/// <param name="inputSlot">The input-assembler input slot to bind this buffer to.</param>
		/// <returns>A new RenderCommand that represents the requested instruction and given parameters.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0",
			Justification = "The parameter is validated by the assurances."), 
		System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters",
			Justification = "Using VertexBuffer<Matrix> instead of BaseResource provides the type safety for the non-type-safe ResourceHandle.")]
		public static RenderCommand SetInstanceBuffer(VertexBuffer<Matrix> instanceBuffer, uint inputSlot) {
			Assure.NotNull(instanceBuffer);
			Assure.False(instanceBuffer.IsDisposed, "Instance buffer was disposed.");
			return new RenderCommand(RenderCommandInstruction.SetInstanceBuffer, (IntPtr) instanceBuffer.ResourceHandle, inputSlot);
		}

		/// <summary>
		/// Creates a render command that instructs the graphics pipeline to set the given resource as the index buffer used by
		/// all proceeding draw calls.
		/// </summary>
		/// <param name="buffer">The index buffer to set. Must not be null or disposed.</param>
		/// <returns>A new RenderCommand that represents the requested instruction and given parameters.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0",
			Justification = "The parameter is validated by the assurances."), 
		System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters",
			Justification = "Using IndexBuffer instead of BaseResource provides the type safety for the non-type-safe ResourceHandle.")]
		public static RenderCommand SetIndexBuffer(IndexBuffer buffer) {
			Assure.NotNull(buffer);
			Assure.False(buffer.IsDisposed, "Index buffer was disposed.");
			return new RenderCommand(RenderCommandInstruction.SetIndexBuffer, (IntPtr) buffer.ResourceHandle);
		}

		/// <summary>
		/// Creates a render command that instructs the graphics pipeline to set the constant buffers for the given shader.
		/// </summary>
		/// <param name="shader">The shader whose <see cref="Shader.ConstantBufferBindings"/> will be set. Must not be null or disposed.</param>
		/// <returns>A new RenderCommand that represents the requested instruction and given parameters.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0",
			Justification = "The parameter is validated by the assurances.")]
		public static unsafe RenderCommand SetShaderConstantBuffers(Shader shader) {
			Assure.NotNull(shader);
			Assure.False(shader.IsDisposed, "Shader was disposed.");
			IntPtr* resourceHandleArrayPtr = (IntPtr*) AllocAndZeroTemp(shader.NumConstantBufferSlots * (uint) sizeof(IntPtr));
			for (int i = 0; i < shader.ConstantBufferBindings.Length; i++) {
				IConstantBuffer boundResource = shader.ConstantBufferBindings[i].GetBoundResource();
				if (boundResource == null) continue;
				resourceHandleArrayPtr[shader.ConstantBufferBindings[i].SlotIndex] = (IntPtr) boundResource.ResourceHandle;
			}
			return new RenderCommand(shader.SetConstantBuffersInstruction, (IntPtr) resourceHandleArrayPtr, shader.NumConstantBufferSlots, 0U);
		}

		/// <summary>
		/// Creates a render command that instructs the graphics pipeline to set the texture samplers for the given shader.
		/// </summary>
		/// <param name="shader">The shader whose <see cref="Shader.TextureSamplerBindings"/> will be set. Must not be null or disposed.</param>
		/// <param name="values">The <see cref="TextureSamplerBinding"/>s to set, paired with the values to bind to them. Must not be null.
		/// Keys may not be null, but values may be null to 'unbind' resources. Resources may not be disposed.</param>
		/// <returns>A new RenderCommand that represents the requested instruction and given parameters.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1",
			Justification = "The parameter is validated by the assurances."), 
		System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0",
			Justification = "The parameter is validated by the assurances.")]
		public static unsafe RenderCommand SetShaderTextureSamplers(Shader shader, FastClearList<KVP<TextureSamplerBinding, TextureSampler>> values) {
			Assure.NotNull(shader);
			Assure.False(shader.IsDisposed, "Shader was disposed.");
			Assure.NotNull(values);
			IntPtr* resourceHandleArrayPtr = (IntPtr*) AllocAndZeroTemp(shader.NumTextureSamplerSlots * (uint) sizeof(IntPtr));
			for (int i = 0; i < values.Count; ++i) {
				var kvp = values[i];
				if (kvp.Value == null) continue; // Legitimate: If we want to set a bind to null, this is how it's done
				resourceHandleArrayPtr[kvp.Key.SlotIndex] = (IntPtr) kvp.Value.ResourceHandle;
			}
			return new RenderCommand(shader.SetTextureSamplersInstruction, (IntPtr) resourceHandleArrayPtr, shader.NumTextureSamplerSlots, 0U);
		}

		/// <summary>
		/// Creates a render command that instructs the graphics pipeline to set the resources for the given shader.
		/// </summary>
		/// <param name="shader">The shader whose <see cref="Shader.ResourceViewBindings"/> will be set. Must not be null or disposed.</param>
		/// <param name="values">The <see cref="ResourceViewBinding"/>s to set, paired with the values to bind to them. Must not be null.
		/// Keys may not be null, but values may be null to 'unbind' resources. Resources may not be disposed.</param>
		/// <returns>A new RenderCommand that represents the requested instruction and given parameters.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1",
			Justification = "The parameter is validated by the assurances."), 
		System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0",
			Justification = "The parameter is validated by the assurances.")]
		public static unsafe RenderCommand SetShaderResourceViews(Shader shader, FastClearList<KVP<ResourceViewBinding, IResourceView>> values) {
			Assure.NotNull(shader);
			Assure.False(shader.IsDisposed, "Shader was disposed.");
			Assure.NotNull(values);
			IntPtr* resourceHandleArrayPtr = (IntPtr*) AllocAndZeroTemp(shader.NumResourceViewSlots * (uint) sizeof(IntPtr));
			for (int i = 0; i < values.Count; ++i) {
				var kvp = values[i];
				if (kvp.Value == null) continue; // Legitimate: If we want to set a bind to null, this is how it's done
				resourceHandleArrayPtr[kvp.Key.SlotIndex] = (IntPtr) kvp.Value.ResourceViewHandle;
			}
			return new RenderCommand(shader.SetResourcesInstruction, (IntPtr) resourceHandleArrayPtr, shader.NumResourceViewSlots, 0U);
		}

		/// <summary>
		/// Creates a render command that instructs the graphics pipeline to set the vertex buffers for the given vertex shader.
		/// </summary>
		/// <param name="shader">The shader whose <see cref="VertexShader.InputBindings"/> will be set. Must not be null or disposed.</param>
		/// <param name="values">The <see cref="VertexInputBinding"/>s to set, paired with the values to bind to them. Must not be null.
		/// Keys may not be null, but values may be null to 'unbind' resources. Resources may not be disposed.</param>
		/// <returns>A new RenderCommand that represents the requested instruction and given parameters.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1",
			Justification = "The parameter is validated by the assurances."), 
		System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0",
			Justification = "The parameter is validated by the assurances.")]
		public static unsafe RenderCommand SetShaderVertexBuffers(VertexShader shader, Dictionary<VertexInputBinding, IVertexBuffer> values) {
			Assure.NotNull(shader);
			Assure.False(shader.IsDisposed, "Shader was disposed.");
			Assure.NotNull(values);
			Assure.None(values.Keys, key => key == null, "One or more keys in the given dictionary of values was null.");
			Assure.None(values.Values, val => val != null && val.IsDisposed, "One or more values in the given dictionary of values was disposed.");
			IntPtr* resourceHandleArrayPtr = (IntPtr*) AllocAndZeroTemp(shader.NumInputSlots * (uint) sizeof(IntPtr));
			uint* bufferStrideArrPtr = (uint*) AllocAndZeroTemp(shader.NumInputSlots * sizeof(uint));

			foreach (KeyValuePair<VertexInputBinding, IVertexBuffer> kvp in values) {
				if (kvp.Value == null) continue; // Legitimate: If we want to set a bind to null, this is how it's done
				resourceHandleArrayPtr[kvp.Key.SlotIndex] = (IntPtr) kvp.Value.ResourceHandle;
				bufferStrideArrPtr[kvp.Key.SlotIndex] = kvp.Value.ElementSizeBytes;
			}

			return new RenderCommand(
				RenderCommandInstruction.SetVertexBuffers,
				(IntPtr) resourceHandleArrayPtr,
				(IntPtr) bufferStrideArrPtr,
				shader.NumInputSlots
			);
		}

		/// <summary>
		/// Creates a render command that instructs the graphics pipeline to write a value to a shader constant buffer.
		/// </summary>
		/// <param name="binding">The binding representing the constant buffer. Must not be null.</param>
		/// <param name="valuePtr">The pointer to the value. The pointer must remain valid until the command queue is flushed.</param>
		/// <returns>A new RenderCommand that represents the requested instruction and given parameters.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "ptr",
			Justification = "Calling a pointer 'ptr' is fine."), 
		System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0",
			Justification = "The parameter is validated by the assurances.")]
		public static RenderCommand DiscardWriteShaderConstantBuffer(ConstantBufferBinding binding, IntPtr valuePtr) {
			Assure.NotNull(binding);
			Assure.NotEqual(valuePtr, IntPtr.Zero, "valuePtr must not be IntPtr.Zero!");
			Assure.False(binding.IsDisposed || binding.GetBoundResource().IsDisposed, "Given binding or its resource was disposed.");
			IntPtr cbufferValPtr = AllocAndZeroTemp(binding.BufferSizeBytes);
			UnsafeUtils.MemCopy(valuePtr, cbufferValPtr, binding.BufferSizeBytes);
			return new RenderCommand(RenderCommandInstruction.CBDiscardWrite, (IntPtr) binding.GetBoundResource().ResourceHandle, cbufferValPtr, binding.BufferSizeBytes);
		}

		public static RenderCommand DiscardWriteShaderConstantBuffer<T>(Buffer<T> buffer, ArraySlice<T> data, uint sizeofT) where T : struct {
			var dataSize = data.Length * sizeofT;
			IntPtr cbufferValPtr = AllocAndZeroTemp(dataSize);
			GCHandle pinnedDataHandle = GCHandle.Alloc(data.ContainingArray, GCHandleType.Pinned);

			try {
				UnsafeUtils.MemCopy(pinnedDataHandle.AddrOfPinnedObject(), cbufferValPtr, dataSize);
			}
			finally {
				pinnedDataHandle.Free();
			}

			return new RenderCommand(RenderCommandInstruction.BufferWrite, (IntPtr) buffer.ResourceHandle, cbufferValPtr, dataSize);
		}

	}
}