// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 22 01 2015 at 17:25 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// A shader used for selecting the eventual colour (and other properties) of each pixel on the output.
	/// </summary>
	public sealed class FragmentShader : Shader {
		private const string DEFAULT_SHADER_FILE_NAME = "UnlitFS.cso";

		internal override RenderCommandInstruction SetConstantBuffersInstruction {
			get { return RenderCommandInstruction.FSSetCBuffers; }
		}

		internal override RenderCommandInstruction SetTextureSamplersInstruction {
			get { return RenderCommandInstruction.FSSetSamplers; }
		}

		internal override RenderCommandInstruction SetResourcesInstruction {
			get { return RenderCommandInstruction.FSSetResources; }
		}

		internal override RenderCommandInstruction SetShaderInstruction {
			get { return RenderCommandInstruction.FSSetShader; }
		}

		/// <summary>
		/// Constructs a new fragment shader.
		/// </summary>
		/// <param name="fileName">Name of the compiled shader file relative to the <see cref="LosgapSystem.InstallationDirectory"/>.</param>
		/// <param name="resourceBindings">An array of resource bindings that represent bind-points for resources that can be used
		/// by the loaded shader program.</param>
		public FragmentShader(string fileName, params IShaderResourceBinding[] resourceBindings) 
			: base(ShaderType.Fragment, fileName, resourceBindings) { }

		/// <summary>
		/// Creates a new default fragment shader.
		/// </summary>
		/// <returns>A new default fragment shader.</returns>
		public static FragmentShader NewDefaultShader() {

			return new FragmentShader(
				DEFAULT_SHADER_FILE_NAME,
				new ResourceViewBinding(0U, "DiffuseMap"),
				new TextureSamplerBinding(0U, "DiffuseSampler")
			);
		}
	}
}