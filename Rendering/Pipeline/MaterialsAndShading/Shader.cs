// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 06 01 2015 at 11:35 by Ben Bowen

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// Represents a loaded shader program (e.g. vertex shader, fragment shader, etc), that is used in the graphics pipeline
	/// to generate the scene visuals.
	/// </summary>
	public abstract class Shader : IDisposable {
		
		[ThreadStatic]
		private static FastClearList<KVP<TextureSamplerBinding, TextureSampler>> texSamplerValueDict;
		[ThreadStatic]
		private static FastClearList<KVP<ResourceViewBinding, IResourceView>> resViewValueDict;
		/// <summary>
		/// The name of this shader.
		/// </summary>
		public readonly string Name;
		/// <summary>
		/// An array of all resource bindings associated with (owned by) this shader.
		/// </summary>
		public readonly IShaderResourceBinding[] ResourceBindings;
		/// <summary>
		/// The lock object to synchronize over when making mutations to internal state on this shader object.
		/// </summary>
		protected readonly object InstanceMutationLock = new object();
		internal readonly ConstantBufferBinding[] ConstantBufferBindings;
		internal readonly TextureSamplerBinding[] TextureSamplerBindings;
		internal readonly ResourceViewBinding[] ResourceViewBindings;
		internal readonly uint NumConstantBufferSlots;
		internal readonly uint NumTextureSamplerSlots;
		internal readonly uint NumResourceViewSlots;
		internal readonly ShaderHandle Handle;
		private readonly Dictionary<string, IShaderResourceBinding> bindingIdentMap = new Dictionary<string, IShaderResourceBinding>();
		private readonly ShaderType typeKey;
		
		private bool isDisposed = false;

		/// <summary>
		/// Whether or not this shader has been disposed.
		/// </summary>
		public bool IsDisposed {
			get {
				lock (InstanceMutationLock) {
					return isDisposed;
				}
			}
		}

		internal abstract RenderCommandInstruction SetConstantBuffersInstruction { get; }
		internal abstract RenderCommandInstruction SetTextureSamplersInstruction { get; }
		internal abstract RenderCommandInstruction SetResourcesInstruction { get; }
		internal abstract RenderCommandInstruction SetShaderInstruction { get; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors",
			Justification = "I'm trusting the call to do nothing but return a constant")]
		internal unsafe Shader(ShaderType typeKey, string fileName, IShaderResourceBinding[] resourceBindings) {
			if (fileName == null) throw new ArgumentNullException("fileName");
			if (resourceBindings == null) throw new ArgumentNullException("resourceBindings");
			
			string filePath = Path.Combine(LosgapSystem.InstallationDirectory.FullName, fileName);

			if (!IOUtils.IsValidFilePath(filePath)) throw new ArgumentException("Invalid shader file path: " + filePath, "fileName");

			ShaderHandle handle;
			InteropUtils.CallNative(NativeMethods.ShaderManager_LoadShader,
				RenderingModule.Device,
				filePath,
				typeKey,
				(IntPtr) (&handle)
			).ThrowOnFailure();

			Handle = handle;
			this.typeKey = typeKey;
			Name = Path.GetFileNameWithoutExtension(fileName);

			try {
				if (resourceBindings.Any(bind => bind == null)) {
					throw new ArgumentNullException("One or more provided shader resource bindings were null.", null as Exception);
				}
				ResourceBindings = resourceBindings;

				ConstantBufferBindings = ResourceBindings
					.OfType<ConstantBufferBinding>()
					.ToArray();
				TextureSamplerBindings = ResourceBindings
					.OfType<TextureSamplerBinding>()
					.ToArray();
				ResourceViewBindings = ResourceBindings
					.OfType<ResourceViewBinding>()
					.ToArray();

				NumConstantBufferSlots = ConstantBufferBindings.Any() ? ConstantBufferBindings.Max(bind => bind.SlotIndex) + 1U : 0U;
				NumTextureSamplerSlots = TextureSamplerBindings.Any() ? TextureSamplerBindings.Max(bind => bind.SlotIndex) + 1U : 0U;
				NumResourceViewSlots = ResourceViewBindings.Any() ? ResourceViewBindings.Max(bind => bind.SlotIndex) + 1U : 0U;

				ResourceBindings.ForEach(res => bindingIdentMap.Add(res.Identifier, res));
			
				TestForBindingClashes();
			}
			catch {
				Dispose();
				throw;
			}
		}

#if DEBUG
		~Shader() {
			if (!IsDisposed) Logger.Warn(this + " was not disposed before finalization!");
		}
#endif

		/// <summary>
		/// Get an associated shader binding by its identifier.
		/// </summary>
		/// <param name="bindingIdentifier">The identifier of the binding to retrieve. Should not be null.</param>
		/// <exception cref="KeyNotFoundException">Thrown if no binding with the given <paramref name="bindingIdentifier"/>
		/// is associated with this shader.</exception>
		/// <returns>The associated resource binding with the given <paramref name="bindingIdentifier"/>.</returns>
		/// <seealso cref="ContainsBinding(string)"/>
		public IShaderResourceBinding GetBindingByIdentifier(string bindingIdentifier) {
			if (bindingIdentifier == null) throw new ArgumentNullException("bindingIdentifier");
			return bindingIdentMap[bindingIdentifier];
		}

		/// <summary>
		/// Ascertains whether or not a binding with the given identifier is associated with this shader.
		/// </summary>
		/// <param name="bindingIdentifier">The identifier to search for. Should not be null.</param>
		/// <returns>True if a binding with the given identifier is associated with this shader, false if not.</returns>
		public bool ContainsBinding(string bindingIdentifier) {
			if (bindingIdentifier == null) throw new ArgumentNullException("bindingIdentifier");
			return bindingIdentMap.ContainsKey(bindingIdentifier);
		}

		/// <summary>
		/// Ascertains whether or not the given <paramref name="binding"/> is associated with this shader.
		/// </summary>
		/// <param name="binding">The binding to check for association with this shader. Must not be null.</param>
		/// <returns>True if the given <paramref name="binding"/> is owned by this shader, false if not.</returns>
		public bool ContainsBinding(IShaderResourceBinding binding) {
			if (binding == null) throw new ArgumentNullException("binding");
			return ResourceBindings.Contains(binding);
		}

		/// <summary>
		/// Disposes this shader, and relinquishes memory and other resources held.
		/// </summary>
		public void Dispose() {
			lock (InstanceMutationLock) {
				if (isDisposed) return;

				isDisposed = true;

				ConstantBufferBindings.ForEach(cbb => ((IDisposable) cbb).Dispose());

				InteropUtils.CallNative(NativeMethods.ShaderManager_UnloadShader,
					Handle,
					typeKey
				).ThrowOnFailure();
			}
		}

		/// <summary>
		/// Returns a string identifying this shader by name,
		/// </summary>
		/// <returns>A string identifying this shader by name,</returns>
		public override string ToString() {
			return typeKey + " Shader '" + Name + "'";
		}

		internal virtual void RCQSwitchToShader(RenderCommandQueue commandQueue) {
			// Set shader & constant buffers
			commandQueue.QueueCommand(RenderCommand.SetShader(this));
			if (NumConstantBufferSlots > 0U) commandQueue.QueueCommand(RenderCommand.SetShaderConstantBuffers(this));
		}

		internal virtual void RCQUpdateResources(RenderCommandQueue commandQueue) {
			for (int i = 0; i < ConstantBufferBindings.Length; i++) {
				commandQueue.QueueCommand(RenderCommand.DiscardWriteShaderConstantBuffer(
					ConstantBufferBindings[i], ConstantBufferBindings[i].CurValuePtr
				));
			}

			if (TextureSamplerBindings.Length > 0) {
				if (texSamplerValueDict == null) texSamplerValueDict = new FastClearList<KVP<TextureSamplerBinding, TextureSampler>>();
				texSamplerValueDict.Clear();
				for (int i = 0; i < TextureSamplerBindings.Length; ++i) {
					texSamplerValueDict.Add(new KVP<TextureSamplerBinding, TextureSampler>(TextureSamplerBindings[i], TextureSamplerBindings[i].GetBoundResource()));
				}
				commandQueue.QueueCommand(RenderCommand.SetShaderTextureSamplers(this, texSamplerValueDict));
			}

			if (ResourceViewBindings.Length > 0) {
				if (resViewValueDict == null) resViewValueDict = new FastClearList<KVP<ResourceViewBinding, IResourceView>>();
				resViewValueDict.Clear();
				for (int i = 0; i < ResourceViewBindings.Length; ++i) {
					resViewValueDict.Add(new KVP<ResourceViewBinding, IResourceView>(ResourceViewBindings[i], ResourceViewBindings[i].GetBoundResourceAsBaseResourceView()));
					Assure.False(ResourceViewBindings[i].GetBoundResourceAsBaseResourceView().Resource.PermittedBindings == GPUBindings.None, "Underlying resource has no permitted GPU bindings.");
				}
				commandQueue.QueueCommand(RenderCommand.SetShaderResourceViews(this, resViewValueDict));
			}
		}

		internal virtual void RCQUpdateResources(RenderCommandQueue commandQueue, ShaderResourcePackage shaderResources) {
			for (int i = 0; i < ConstantBufferBindings.Length; i++) {
				commandQueue.QueueCommand(RenderCommand.DiscardWriteShaderConstantBuffer(
					ConstantBufferBindings[i], shaderResources.GetValue(ConstantBufferBindings[i])
				));
			}

			if (TextureSamplerBindings.Length > 0) {
				if (texSamplerValueDict == null) texSamplerValueDict = new FastClearList<KVP<TextureSamplerBinding, TextureSampler>>();
				texSamplerValueDict.Clear();
				for (int i = 0; i < TextureSamplerBindings.Length; ++i) {
					var tsb = TextureSamplerBindings[i];
					texSamplerValueDict.Add(new KVP<TextureSamplerBinding, TextureSampler>(tsb, shaderResources.GetValue(tsb)));
				}
				commandQueue.QueueCommand(RenderCommand.SetShaderTextureSamplers(this, texSamplerValueDict));
			}

			if (ResourceViewBindings.Length > 0) {
				if (resViewValueDict == null) resViewValueDict = new FastClearList<KVP<ResourceViewBinding, IResourceView>>();
				resViewValueDict.Clear();
				for (int i = 0; i < ResourceViewBindings.Length; ++i) {
					var rvb = ResourceViewBindings[i];
					resViewValueDict.Add(new KVP<ResourceViewBinding, IResourceView>(rvb, shaderResources.GetValue(rvb)));
					Assure.False(
						((BaseResourceView) shaderResources.GetValue(ResourceViewBindings[i])) != null
							&& ((BaseResourceView) shaderResources.GetValue(ResourceViewBindings[i])).Resource.PermittedBindings == GPUBindings.None,
						"Underlying resource has no permitted GPU bindings."
					);
				}
				commandQueue.QueueCommand(RenderCommand.SetShaderResourceViews(this, resViewValueDict));
			}
		}

		private void TestForBindingClashes() {
			// Test for clashes on input slot
			KeyValuePair<ConstantBufferBinding, ConstantBufferBinding>? matchingCBuffers =
				ConstantBufferBindings.FirstRelationship((a, b) => a.SlotIndex == b.SlotIndex);
			if (matchingCBuffers != null) {
				throw new ArgumentException("Constant buffer bindings '" + matchingCBuffers.Value.Key + "' and '" + matchingCBuffers.Value.Value + "' " +
					"must not share the same input slot (" + matchingCBuffers.Value.Key.SlotIndex + ")!");
			}

			KeyValuePair<TextureSamplerBinding, TextureSamplerBinding>? matchingSamplers =
				TextureSamplerBindings.FirstRelationship((a, b) => a.SlotIndex == b.SlotIndex);
			if (matchingSamplers != null) {
				throw new ArgumentException("Texture sampler bindings '" + matchingSamplers.Value.Key + "' and '" + matchingSamplers.Value.Value + "' " +
					"must not share the same input slot (" + matchingSamplers.Value.Key.SlotIndex + ")!");
			}

			KeyValuePair<ResourceViewBinding, ResourceViewBinding>? matchingResourceViews =
				ResourceViewBindings.FirstRelationship((a, b) => a.SlotIndex == b.SlotIndex);
			if (matchingResourceViews != null) {
				throw new ArgumentException("Bindings '" + matchingResourceViews.Value.Key + "' and '" + matchingResourceViews.Value.Value + "' " +
					"must not share the same input slot (" + matchingResourceViews.Value.Key.SlotIndex + ")!");
			}

			// Test for clashes on identifier
			KeyValuePair<IShaderResourceBinding, IShaderResourceBinding>? matchingIdentifiers =
				ResourceBindings.FirstRelationship((a, b) => a.Identifier.Equals(b.Identifier, StringComparison.OrdinalIgnoreCase));
			if (matchingIdentifiers != null) {
				throw new ArgumentException("Bindings '" + matchingIdentifiers.Value.Key + "' and '" + matchingIdentifiers.Value.Value + "' " +
					"must not share the same identifier (" + matchingIdentifiers.Value.Key.Identifier + ")!");
			}
		}
	}
}