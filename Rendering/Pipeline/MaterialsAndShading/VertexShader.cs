// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 06 01 2015 at 15:01 by Ben Bowen

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// A shader used for transforming vertices ready for rasterization to NDC, and extrapolation of supplied parameters over the rasterized
	/// output.
	/// </summary>
	public sealed class VertexShader : Shader {
		/// <summary>
		/// The maximum number of permitted <see cref="VertexInputBinding"/>s for any one given vertex shader.
		/// </summary>
		public const int MAX_VS_INPUT_BINDINGS = 15;
		internal const ResourceFormat INSTANCE_DATA_FORMAT = ResourceFormat.R32G32B32A32Float;
		private const string DEFAULT_SHADER_FILE_NAME = "UnlitVS.cso";
		[ThreadStatic]
		private static Dictionary<VertexInputBinding, IVertexBuffer> vertexInputValueDict;
		internal readonly VertexInputBinding[] InputBindings;
		internal readonly uint NumInputSlots;
		internal readonly VertexInputBinding InstanceDataBinding;
		internal readonly ConstantBufferBinding ViewProjMatBinding;

		internal override RenderCommandInstruction SetConstantBuffersInstruction {
			get { return RenderCommandInstruction.VSSetCBuffers; }
		}

		internal override RenderCommandInstruction SetTextureSamplersInstruction {
			get { return RenderCommandInstruction.VSSetSamplers; }
		}

		internal override RenderCommandInstruction SetResourcesInstruction {
			get { return RenderCommandInstruction.VSSetResources; }
		}

		internal override RenderCommandInstruction SetShaderInstruction {
			get { return RenderCommandInstruction.VSSetShader; }
		}

		public VertexShader(string filename, params IShaderResourceBinding[] otherBindings) 
			: base(ShaderType.Vertex, filename, otherBindings) {
				InstanceDataBinding = null;
				ViewProjMatBinding = null;

				this.InputBindings = ResourceBindings.OfType<VertexInputBinding>().ToArray();
				if (InputBindings.Length > MAX_VS_INPUT_BINDINGS) {
					throw new ArgumentException("Can not specify more than " + MAX_VS_INPUT_BINDINGS + " vertex input bindings.", "otherBindings");
				}

				this.NumInputSlots = InputBindings.Any() ? InputBindings.Max(bind => bind.SlotIndex) + 1U : 0U;
				if (NumInputSlots > MAX_VS_INPUT_BINDINGS) {
					throw new ArgumentException("Can not set more than " + MAX_VS_INPUT_BINDINGS + " input slots.");
				}
		}

		/// <summary>
		/// Constructs a new vertex shader.
		/// </summary>
		/// <param name="fileName">Name of the compiled shader file relative to the <see cref="LosgapSystem.InstallationDirectory"/>.</param>
		/// <param name="instanceDataBinding">The binding representing instance data supplied by the rendering system.
		/// The bound input must pertain to a <c>column-major float4x4</c> matrix in the shader program
		/// that represents a submitted model instance's world transform.</param>
		/// <param name="viewProjTransformBinding">The binding representing the camera's view/projection transform for a given frame.
		/// The binding must already have a <see cref="ConstantBuffer{TConstants}">ConstantBuffer&lt;Matrix&gt;</see> bound that
		/// will be used by the rendering system to set the view/proj matrix per-frame.</param>
		/// <param name="otherBindings">An array of optional extra resource bindings for resources that will be accessed by this shader.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2",
			Justification = "The parameter is validated by base constructor.")]
		public VertexShader(string fileName,
			VertexInputBinding instanceDataBinding, ConstantBufferBinding viewProjTransformBinding, params IShaderResourceBinding[] otherBindings)
			: base(
				ShaderType.Vertex, 
				fileName, 
				otherBindings.Concat(new IShaderResourceBinding[] { instanceDataBinding, viewProjTransformBinding }).ToArray()
			) 
		{
			InstanceDataBinding = instanceDataBinding;
			ViewProjMatBinding = viewProjTransformBinding;

			this.InputBindings = ResourceBindings.OfType<VertexInputBinding>().ToArray();
			if (InputBindings.Length > MAX_VS_INPUT_BINDINGS) {
				throw new ArgumentException("Can not specify more than " + MAX_VS_INPUT_BINDINGS + " vertex input bindings.", "otherBindings");
			}

			this.NumInputSlots = InputBindings.Any() ? InputBindings.Max(bind => bind.SlotIndex) + 1U : 0U;
			if (NumInputSlots > MAX_VS_INPUT_BINDINGS) {
				throw new ArgumentException("Can not set more than " + MAX_VS_INPUT_BINDINGS + " input slots.");
			}
		}

		/// <summary>
		/// Creates a new default vertex shader.
		/// </summary>
		/// <param name="viewProjTransformBuffer">The constant buffer that will be used as a view/projection matrix
		/// buffer, set per-frame.</param>
		/// <returns>A new vertex shader.</returns>
		public static VertexShader NewDefaultShader(ConstantBuffer<Matrix> viewProjTransformBuffer) {
			if (viewProjTransformBuffer == null) throw new ArgumentNullException("viewProjTransformBuffer");
			if (viewProjTransformBuffer.IsDisposed) throw new ObjectDisposedException("viewProjTransformBuffer");

			return new VertexShader(
				DEFAULT_SHADER_FILE_NAME,
				new VertexInputBinding(0U, "INSTANCE_TRANSFORM"), 
				new ConstantBufferBinding(0U, "CameraTransform", viewProjTransformBuffer),
				new VertexInputBinding(1U, "POSITION"),
				new VertexInputBinding(3U, "TEXCOORD")
			);
		}

		internal override void RCQUpdateResources(RenderCommandQueue commandQueue) {
			base.RCQUpdateResources(commandQueue);

			// Set vertex buffers
			if (InputBindings.Length > 0) {
				if (vertexInputValueDict == null) vertexInputValueDict = new Dictionary<VertexInputBinding, IVertexBuffer>();
				vertexInputValueDict.Clear();
				for (int i = 0; i < InputBindings.Length; ++i) {
					vertexInputValueDict[InputBindings[i]] = InputBindings[i].GetBoundResource();
				}
				commandQueue.QueueCommand(RenderCommand.SetShaderVertexBuffers(this, vertexInputValueDict));
			}
		}

		internal override void RCQUpdateResources(RenderCommandQueue commandQueue, ShaderResourcePackage shaderResources) {
			base.RCQUpdateResources(commandQueue, shaderResources);

			// Set vertex buffers
			if (InputBindings.Length > 0) {
				if (vertexInputValueDict == null) vertexInputValueDict = new Dictionary<VertexInputBinding, IVertexBuffer>();
				vertexInputValueDict.Clear();
				for (int i = 0; i < InputBindings.Length; ++i) {
					vertexInputValueDict[InputBindings[i]] = shaderResources.GetValue(InputBindings[i]);
				}
				commandQueue.QueueCommand(RenderCommand.SetShaderVertexBuffers(this, vertexInputValueDict));
			}
		}
	}
}