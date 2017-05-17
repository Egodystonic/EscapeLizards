// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 14 05 2015 at 16:43 by Ben Bowen

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Threading;
using Ophidian.Losgap;
using Ophidian.Losgap.AssetManagement;
using Ophidian.Losgap.Audio;
using Ophidian.Losgap.CSG;
using Ophidian.Losgap.Entities;
using Ophidian.Losgap.Input;
using Ophidian.Losgap.Rendering;

namespace Egodystonic.EscapeLizards {
	public static class EntryPoint {
		public static volatile bool InEditor = true;
		#region Disposables
		// Thread safety is no concern here because these fields will be set on the Main() thread and disposed on the Main() thread
		// Resource fields
		private static GeometryCache gameObjectCache;

		// Shader fields
		private static ConstantBuffer<GeomPassProjViewMatrices> geomVSCameraBuffer;
		private static ConstantBuffer<Vector4> geomMatPropsBuffer;
		private static TextureSampler defaultDiffuseSampler;
		private static TextureSampler shadowSampler;
		private static VertexShader shadowVS;
		private static FragmentShader shadowFS;
		private static VertexShader lightVS;
		private static ConstantBuffer<Vector4> lightFSCameraBuffer;
		private static ConstantBuffer<Vector4> lightMetaBuffer;
		private static Buffer<LightProperties> lightBuffer;
		private static ShaderBufferResourceView lightBufferView;
		private static ConstantBuffer<Vector4> lightVSScalarsBuffer;
		private static FragmentShader lightFS;
		private static FragmentShader finalizationFS;
		private static FragmentShader blurShader;
		private static ConstantBuffer<Vector4> dofLensPropsBuffer;
		private static FragmentShader dofShader;
		private static FragmentShader bloomHShader, bloomVShader;
		private static FragmentShader copyShader;
		private static FragmentShader copyReverseShader;
		private static FragmentShader outliningShader, outliningNoopShader;
		private static TextureSampler gbufferNormalSampler;
		private static VertexShader skyVS;
		private static TextureSampler skyDiffuseSampler;
		private static ConstantBuffer<Vector4> hudVPPropsBuffer;
		private static VertexShader hudVS;
		private static VertexShader simpleVS;
		private static TextureSampler simpleFSSampler;
		private static ConstantBuffer<Vector4> simpleFSMatPropsBuffer;
		private static ConstantBuffer<Vector4> hudFSColorBuffer;
		private static ConstantBuffer<Vector4> hudTextFSColorBuffer;
		private static TextureSampler hudFSSampler;
		private static TextureSampler hudTextFSSampler;
		private static FragmentShader glowShader;

		// Pass fields
		private static ShadowPass shadowPass;
		private static HUDPass hudPass;
		#endregion

		public static void Main(string[] args) {
			Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
			string instDir;
			string dataDir;
			try {
				instDir = args.Length > 0 && !args[0].Equals("default", StringComparison.OrdinalIgnoreCase)
					? args[0]
					: Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles, Environment.SpecialFolderOption.None), @"Egodystonic\Escape Lizards\Inst\");
				dataDir = args.Length > 1 && !args[1].Equals("default", StringComparison.OrdinalIgnoreCase)
					? args[1]
					: Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create), @"Egodystonic\Escape Lizards\Data\");
			}
			catch (Exception e) {
				LosgapSystem.ExitWithError("Error accessing installation data. Try running the game as administrator?", e);
				return;
			}

			InitEngine(instDir, dataDir);
			PhysicsManager.SetPhysicsTickrate(5f);
			InitDisplayWindow();
			InitLayers();
			InitShaders();
			InitPasses();
			InitResources();

			LevelDatabase.LoadAllMetadata();
			PersistedWorldData.LoadFromDisk();
			BGMManager.Init();
			SteamworksManager.Init();
			LeaderboardManager.Init();
			LeaderboardManager.LoadCachedLeaderboardData();


			PretriangulateLevels();

			//EntityModule.PostTick += deltaTime => {
			//	AssetLocator.MainWindow.Title = (1f / deltaTime) + " FPS";
			//};

			StartEngine(); // Blocks until engine is stopped

			SteamworksManager.ShutdownSteamAPI();
			DisposeAll();
		}

		private static void InitEngine(string instDir, string dataDir) {
			InEditor = false;
			LosgapSystem.ApplicationName = "Escape Lizards";
			LosgapSystem.InstallationDirectory = new DirectoryInfo(instDir);
			LosgapSystem.MutableDataDirectory = new DirectoryInfo(dataDir);

			if (!LosgapSystem.IsModuleAdded(typeof(RenderingModule))) LosgapSystem.AddModule(typeof(RenderingModule));
			if (!LosgapSystem.IsModuleAdded(typeof(InputModule))) LosgapSystem.AddModule(typeof(InputModule));
			if (!LosgapSystem.IsModuleAdded(typeof(EntityModule))) LosgapSystem.AddModule(typeof(EntityModule));
			if (!LosgapSystem.IsModuleAdded(typeof(AudioModule))) LosgapSystem.AddModule(typeof(AudioModule));
			//if (!LosgapSystem.IsModuleAdded(typeof(SteamModule))) LosgapSystem.AddModule(typeof(SteamModule));

			Config.Load();
			Config.ConfigRefresh += ConfigRefresh;

			if (Config.GPUSelectionOverride != null || Config.OutputSelectionOverride != null) {
				try {
					int outGPUIndex, outOutputGPUIndex, outOutputIndex;
					RenderingModule.GetRecommendedHardwareIndices(out outGPUIndex, out outOutputGPUIndex, out outOutputIndex);

					RenderingModule.SetHardware((int) (Config.GPUSelectionOverride ?? (uint) outGPUIndex), (int) (Config.OutputGPUSelectionOverride ?? (uint) outOutputGPUIndex), Config.OutputSelectionOverride.HasValue ? (int?) Config.OutputSelectionOverride.Value : null);
				}
				catch (ArgumentOutOfRangeException e) {
					Logger.Warn("Invalid GPU or Output override.", e);
				}
			}

			LosgapSystem.MaxThreadCount = Config.MaxThreadCount; //Config.MaxThreadCount;
			LosgapSystem.SlaveThreadPriority = Config.SlaveThreadPriority;
			LosgapSystem.ThreadOversubscriptionFactor = Config.ThreadOversubscriptionFactor;
			RenderingModule.VSyncEnabled = Config.VSyncEnabled;
			RenderingModule.MaxFrameRateHz = Config.MaxFrameRateHz == 0L ? (long?) null : Config.MaxFrameRateHz;
			RenderingModule.AntialiasingLevel = MSAALevel.None; // MSAA can not be used with deferred lighting
			InputModule.InputStateUpdateRateHz = Config.InputStateUpdateRateHz == 0L ? (long?) null : Config.InputStateUpdateRateHz;
			EntityModule.TickRateHz = Config.TickRateHz == 0L ? (long?) null : Config.TickRateHz;
			AudioModule.AudioUpdateRateHz = Config.TickRateHz == 0L ? (long?) null : Config.TickRateHz;
			//SteamModule.TickRateHz = Config.TickRateHz;
		}

		private static void InitDisplayWindow() {
			uint windowWidth = Config.DisplayWindowWidth == 0U
				? RenderingModule.SelectedOutputDisplay.HighestResolution.ResolutionWidth
				: Config.DisplayWindowWidth;
			uint windowHeight = Config.DisplayWindowHeight == 0U
				? RenderingModule.SelectedOutputDisplay.HighestResolution.ResolutionHeight
				: Config.DisplayWindowHeight;
			AssetLocator.MainWindow = new Window(
				LosgapSystem.ApplicationName,
				windowWidth,
				windowHeight,
				Config.DisplayWindowFullscreenState,
				Config.DisplayWindowMultibufferLevel,
				Path.Combine(LosgapSystem.InstallationDirectory.FullName, "elFavicon.ico")
			);
			AssetLocator.MainWindow.ClearViewports().ForEach(vp => vp.Dispose());
			AssetLocator.MainWindow.AddViewport(ViewportAnchoring.Centered, Vector2.ZERO, Vector2.ONE, Config.DisplayWindowNearPlane, Config.DisplayWindowFarPlane);
			AssetLocator.MainWindow.WindowClosed += _ => LosgapSystem.Exit();
			AssetLocator.MainWindow.ShowCursor = false;
		}

		private static void InitLayers() {
			AssetLocator.GameLayer = Scene.CreateLayer("Main Layer");
			AssetLocator.GameAlphaLayer = Scene.CreateLayer("Main Alpha Layer");
			AssetLocator.SkyLayer = Scene.CreateLayer("Sky Layer");
			AssetLocator.HudLayer = Scene.CreateLayer("HUD Layer");
		}

		private static void InitShaders() {
			geomVSCameraBuffer = BufferFactory.NewConstantBuffer<GeomPassProjViewMatrices>().WithUsage(ResourceUsage.DiscardWrite);

			//	- Shadow VS
			shadowVS = new VertexShader(
				Path.Combine(AssetLocator.ShadersDir, "ShadowVS.cso"),
				new VertexInputBinding(0U, "INSTANCE_TRANSFORM"),
				new ConstantBufferBinding(0U, "CameraTransform", geomVSCameraBuffer),
				new VertexInputBinding(1U, "POSITION")
			);

			//	- Shadow FS
			shadowFS = new FragmentShader(
				Path.Combine(AssetLocator.ShadersDir, "ShadowFS.cso")
			);

			//	- Geometry VS
			AssetLocator.MainGeometryVertexShader = new VertexShader(
				Path.Combine(AssetLocator.ShadersDir, "DLGeometryVS.cso"),
				new VertexInputBinding(0U, "INSTANCE_TRANSFORM"),
				new ConstantBufferBinding(0U, "CameraTransform", geomVSCameraBuffer),
				new VertexInputBinding(1U, "TEXCOORD"),
				new VertexInputBinding(2U, "POSITION"),
				new VertexInputBinding(3U, "NORMAL"),
				new VertexInputBinding(4U, "TANGENT")
			);

			//	- Geometry VS Sky
			AssetLocator.SkyVertexShader = new VertexShader(
				Path.Combine(AssetLocator.ShadersDir, "DLGeometryVSSky.cso"),
				new VertexInputBinding(0U, "INSTANCE_TRANSFORM"),
				new ConstantBufferBinding(0U, "CameraTransform", geomVSCameraBuffer),
				new VertexInputBinding(1U, "TEXCOORD"),
				new VertexInputBinding(2U, "POSITION"),
				new VertexInputBinding(3U, "NORMAL"),
				new VertexInputBinding(4U, "TANGENT")
			);


			//	- Geometry FS
			geomMatPropsBuffer = BufferFactory.NewConstantBuffer<Vector4>().WithUsage(ResourceUsage.DiscardWrite);
			defaultDiffuseSampler = new TextureSampler(
				Config.DefaultSamplingFilterType,
				TextureWrapMode.Wrap,
				Config.DefaultSamplingAnisoLevel
			);
			shadowSampler = new TextureSampler(
				TextureFilterType.MinMagMipLinear,
				TextureWrapMode.Clamp,
				AnisotropicFilteringLevel.FourTimes
			);
			AssetLocator.GeometryFragmentShader = new FragmentShader(
				Path.Combine(AssetLocator.ShadersDir, "DLGeometryFS.cso"),
				new ResourceViewBinding(0U, "DiffuseMap"),
				new ResourceViewBinding(1U, "NormalMap"),
				new ResourceViewBinding(2U, "SpecularMap"),
				new ResourceViewBinding(3U, "EmissiveMap"),
				new ResourceViewBinding(4U, "ShadowMap"),
				new TextureSamplerBinding(0U, "DiffuseSampler"),
				new TextureSamplerBinding(1U, "ShadowSampler"),
				new ConstantBufferBinding(0U, "MaterialProperties", geomMatPropsBuffer)
			);
			AssetLocator.GeometryFragmentShader.GetBindingByIdentifier("DiffuseSampler").Bind(defaultDiffuseSampler);
			AssetLocator.GeometryFragmentShader.GetBindingByIdentifier("ShadowSampler").Bind(shadowSampler);

			//	- Sky Geometry FS
			AssetLocator.SkyGeometryFragmentShader = new FragmentShader(
				Path.Combine(AssetLocator.ShadersDir, "DLGeometryFSSky.cso"),
				new ResourceViewBinding(0U, "DiffuseMap"),
				new TextureSamplerBinding(0U, "DiffuseSampler"),
				new ConstantBufferBinding(0U, "MaterialProperties", geomMatPropsBuffer)
			);
			AssetLocator.SkyGeometryFragmentShader.GetBindingByIdentifier("DiffuseSampler").Bind(defaultDiffuseSampler);

			//	- Light VS
			lightVSScalarsBuffer = BufferFactory.NewConstantBuffer<Vector4>().WithUsage(ResourceUsage.DiscardWrite);
			lightVS = new VertexShader(
				Path.Combine(AssetLocator.ShadersDir, "DLLightingVS.cso"),
				new VertexInputBinding(0U, "POSITION"),
				new VertexInputBinding(1U, "TEXCOORD"),
				new ConstantBufferBinding(0U, "Scalars", lightVSScalarsBuffer)
			);


			//	- Light FS
			lightFSCameraBuffer = BufferFactory.NewConstantBuffer<Vector4>().WithUsage(ResourceUsage.DiscardWrite);
			lightMetaBuffer = BufferFactory.NewConstantBuffer<Vector4>().WithUsage(ResourceUsage.DiscardWrite);
			lightBuffer = BufferFactory.NewBuffer<LightProperties>()
				.WithLength(DLLightPass.MAX_DYNAMIC_LIGHTS)
				.WithUsage(ResourceUsage.DiscardWrite)
				.WithPermittedBindings(GPUBindings.ReadableShaderResource);
			lightBufferView = lightBuffer.CreateView();
			lightFS = new FragmentShader(
				Path.Combine(AssetLocator.ShadersDir, "DLLightingFS.cso"),
				new ResourceViewBinding(0U, "NormalGB"),
				new ResourceViewBinding(1U, "DiffuseGB"),
				new ResourceViewBinding(2U, "SpecularGB"),
				new ResourceViewBinding(3U, "PositionGB"),
				new ResourceViewBinding(5U, "EmissiveGB"),
				new ConstantBufferBinding(0U, "CameraProperties", lightFSCameraBuffer),
				new ConstantBufferBinding(1U, "LightMeta", lightMetaBuffer),
				new ResourceViewBinding(4U, "LightBuffer"),
				new TextureSamplerBinding(0U, "NormalSampler")
			);
			gbufferNormalSampler = new TextureSampler(
				TextureFilterType.None,
				TextureWrapMode.Wrap,
				AnisotropicFilteringLevel.None
				);
			((ResourceViewBinding) lightFS.GetBindingByIdentifier("LightBuffer")).Bind(lightBufferView);
			((TextureSamplerBinding) lightFS.GetBindingByIdentifier("NormalSampler")).Bind(gbufferNormalSampler);


			//	- Light Finalization
			finalizationFS = new FragmentShader(
				Path.Combine(AssetLocator.ShadersDir, "DLFinalFS.cso"),
				new ResourceViewBinding(1U, "DiffuseGB")
			);

			//	- Outlining
			outliningShader = new FragmentShader(
				Path.Combine(AssetLocator.ShadersDir, "Outliner.cso"),
				new ResourceViewBinding(0U, "NormalGB"),
				new ResourceViewBinding(1U, "GeomDepthBuffer")
			);
			outliningNoopShader = new FragmentShader(
				Path.Combine(AssetLocator.ShadersDir, "OutlinerNoop.cso"),
				new ResourceViewBinding(0U, "NormalGB"),
				new ResourceViewBinding(1U, "GeomDepthBuffer")
			);

			//	- Bloom Shader (H)
			bloomHShader = new FragmentShader(
				Path.Combine(AssetLocator.ShadersDir, "BloomH.cso"),
				new ResourceViewBinding(0U, "PreBloomImage")
			);

			//	- Bloom Shader (V)
			bloomVShader = new FragmentShader(
				Path.Combine(AssetLocator.ShadersDir, "BloomV.cso"),
				new ResourceViewBinding(0U, "PreBloomImage")
			);

			//	- Copy Shader
			copyShader = new FragmentShader(
				Path.Combine(AssetLocator.ShadersDir, "Copy.cso"),
				new ResourceViewBinding(0U, "SourceTex"),
				new TextureSamplerBinding(0U, "SourceSampler")
			);

			//	- Copy Reverse Shader
			copyReverseShader = new FragmentShader(
				Path.Combine(AssetLocator.ShadersDir, "CopyReverse.cso"),
				new ResourceViewBinding(0U, "SourceTex"),
				new TextureSamplerBinding(0U, "SourceSampler")
			);

			//	- Blur Shader
			blurShader = new FragmentShader(
				Path.Combine(AssetLocator.ShadersDir, "Blur.cso"),
				new ResourceViewBinding(0U, "UnblurredScene")
			);

			//	- DoF Shader
			dofLensPropsBuffer = BufferFactory.NewConstantBuffer<Vector4>()
				.WithUsage(ResourceUsage.DiscardWrite);
			dofShader = new FragmentShader(
				Path.Combine(AssetLocator.ShadersDir, "DoF.cso"),
				new ResourceViewBinding(0U, "UnblurredScene"),
				new ResourceViewBinding(1U, "BlurredScene"),
				new ResourceViewBinding(2U, "SceneDepth"),
				new TextureSamplerBinding(0U, "SourceSampler"),
				new ConstantBufferBinding(0U, "LensProperties", dofLensPropsBuffer)
			);

			//	- Sky VS
			skyVS = new VertexShader(
				Path.Combine(AssetLocator.ShadersDir, "UnlitVSSky.cso"),
				new VertexInputBinding(0U, "INSTANCE_TRANSFORM"),
				new ConstantBufferBinding(0U, "CameraTransform", geomVSCameraBuffer),
				new VertexInputBinding(1U, "TEXCOORD"),
				new VertexInputBinding(2U, "POSITION")
			);


			//	- Sky FS
			AssetLocator.SkyFragmentShader = new FragmentShader(
				Path.Combine(AssetLocator.ShadersDir, "UnlitFS.cso"),
				new TextureSamplerBinding(0U, "DiffuseSampler"),
				new ResourceViewBinding(0U, "DiffuseMap")
			);
			skyDiffuseSampler = new TextureSampler(
				Config.DefaultSamplingFilterType,
				TextureWrapMode.Wrap,
				Config.DefaultSamplingAnisoLevel
			);
			((TextureSamplerBinding) AssetLocator.SkyFragmentShader.GetBindingByIdentifier("DiffuseSampler")).Bind(skyDiffuseSampler);


			//	- Simple VS
			simpleVS = new VertexShader(
				Path.Combine(AssetLocator.ShadersDir, "SimpleVS.cso"),
				new VertexInputBinding(0U, "INSTANCE_TRANSFORM"),
				new ConstantBufferBinding(0U, "CameraTransform", geomVSCameraBuffer),
				new VertexInputBinding(1U, "TEXCOORD"),
				new VertexInputBinding(2U, "POSITION")
			);

			//	- Simple FS
			simpleFSSampler = new TextureSampler(
				Config.DefaultSamplingFilterType,
				TextureWrapMode.Wrap,
				Config.DefaultSamplingAnisoLevel
			);
			simpleFSMatPropsBuffer = BufferFactory.NewConstantBuffer<Vector4>().WithUsage(ResourceUsage.DiscardWrite);
			AssetLocator.AlphaFragmentShader = new FragmentShader(
				Path.Combine(AssetLocator.ShadersDir, "SimpleFS.cso"),
				new TextureSamplerBinding(0U, "DiffuseSampler"),
				new ResourceViewBinding(0U, "DiffuseMap"),
				new ConstantBufferBinding(0U, "MaterialProperties", simpleFSMatPropsBuffer)
			);
			((TextureSamplerBinding) AssetLocator.AlphaFragmentShader.GetBindingByIdentifier("DiffuseSampler")).Bind(simpleFSSampler);


			//	- Hud VS
			hudVPPropsBuffer = BufferFactory.NewConstantBuffer<Vector4>().WithUsage(ResourceUsage.DiscardWrite);
			hudVS = new VertexShader(
				Path.Combine(AssetLocator.ShadersDir, "HUDVS.cso"),
				new VertexInputBinding(0U, "INSTANCE_TRANSFORM"),
				new ConstantBufferBinding(1U, "CameraTransform", geomVSCameraBuffer),
				new ConstantBufferBinding(0U, "ViewportProperties", hudVPPropsBuffer),
				new VertexInputBinding(1U, "TEXCOORD"),
				new VertexInputBinding(2U, "POSITION")
			);


			//	- Hud FS
			hudFSColorBuffer = BufferFactory.NewConstantBuffer<Vector4>().WithUsage(ResourceUsage.DiscardWrite);
			AssetLocator.HUDFragmentShader = new FragmentShader(
				Path.Combine(AssetLocator.ShadersDir, "HUDFS.cso"),
				new ResourceViewBinding(0U, "DiffuseMap"),
				new TextureSamplerBinding(0U, "DiffuseSampler"),
				new ConstantBufferBinding(0U, "TextureProperties", hudFSColorBuffer)
			);
			hudFSSampler = new TextureSampler(
				TextureFilterType.None,
				TextureWrapMode.Mirror,
				AnisotropicFilteringLevel.None,
				new TexelFormat.RGB32Float {
					R = 1f,
					G = 0f,
					B = 0f
				}
			);
			((TextureSamplerBinding) AssetLocator.HUDFragmentShader.GetBindingByIdentifier("DiffuseSampler")).Bind(hudFSSampler);

			//	- Glow Shader
			glowShader = new FragmentShader(
				Path.Combine(AssetLocator.ShadersDir, "Glow.cso"),
				new ResourceViewBinding(0U, "GlowSrc")
			);

			//	- Hud Text FS
			hudTextFSColorBuffer = BufferFactory.NewConstantBuffer<Vector4>().WithUsage(ResourceUsage.DiscardWrite);
			AssetLocator.HUDTextFragmentShader = new FragmentShader(
				Path.Combine(AssetLocator.ShadersDir, "HUDFSText.cso"),
				new ResourceViewBinding(0U, "DiffuseMap"),
				new TextureSamplerBinding(0U, "DiffuseSampler"),
				new ConstantBufferBinding(0U, "TextProperties", hudTextFSColorBuffer)
			);
			hudTextFSSampler = new TextureSampler(
				TextureFilterType.None,
				TextureWrapMode.Border,
				AnisotropicFilteringLevel.None,
				new TexelFormat.RGB32Float() {
					R = 1f,
					G = 0f,
					B = 0f
				}
			);
			((TextureSamplerBinding) AssetLocator.HUDTextFragmentShader.GetBindingByIdentifier("DiffuseSampler")).Bind(hudTextFSSampler);
		}

		private static void InitPasses() {
			// Shadow Pass
			shadowPass = new ShadowPass("Shadow Pass");
			shadowPass.AddLayer(AssetLocator.GameLayer);
			shadowPass.ShadowVS = shadowVS;
			shadowPass.ShadowFS = shadowFS;
			shadowPass.Output = AssetLocator.MainWindow;

			// Main Pass (Geometry)
			AssetLocator.MainGeometryPass = new DLGeometryPass("Main Pass (Geometry)");
			AssetLocator.MainGeometryPass.ClearOutputBeforePass = false;
			AssetLocator.MainGeometryPass.AddLayer(AssetLocator.GameLayer);
			AssetLocator.MainGeometryPass.AddLayer(AssetLocator.SkyLayer);
			AssetLocator.MainGeometryPass.DepthStencilState = new DepthStencilState();
			AssetLocator.MainGeometryPass.Output = AssetLocator.MainWindow;
			AssetLocator.MainGeometryPass.BlendState = new BlendState(BlendOperation.None);
			AssetLocator.MainGeometryPass.RasterizerState = new RasterizerState(false, TriangleCullMode.BackfaceCulling, false);
			AssetLocator.MainGeometryPass.ShadowPass = shadowPass;
			AssetLocator.MainGeometryPass.GeomFSWithShadowSupport = AssetLocator.GeometryFragmentShader;

			// Main Pass (Lighting)
			AssetLocator.LightPass = new DLLightPass("Main Pass (Lighting)");
			AssetLocator.LightPass.DLLightVertexShader = lightVS;
			AssetLocator.LightPass.DLLightFragmentShader = lightFS;
			AssetLocator.LightPass.DLLightFinalizationShader = finalizationFS;
			AssetLocator.LightPass.BloomHShader = bloomHShader;
			AssetLocator.LightPass.BloomVShader = bloomVShader;
			AssetLocator.LightPass.CopyReverseShader = copyReverseShader;
			AssetLocator.LightPass.CopyShader = copyShader;
			AssetLocator.LightPass.OutliningShader = Config.EnableOutlining ? outliningShader : outliningNoopShader;
			AssetLocator.LightPass.BlurShader = blurShader;
			AssetLocator.LightPass.DoFShader = dofShader;
			AssetLocator.LightPass.GeometryPass = AssetLocator.MainGeometryPass;
			AssetLocator.LightPass.PresentAfterPass = false;
			AssetLocator.LightPass.SetLensProperties(Config.DofLensFocalDist, Config.DofLensMaxBlurDist);

			// Alpha Pass
			AssetLocator.AlphaPass = new AlphaPass("Alpha Pass");
			AssetLocator.AlphaPass.AddLayer(AssetLocator.GameAlphaLayer);
			AssetLocator.AlphaPass.BlendState = new BlendState(BlendOperation.AlphaAwareSrcOverride);
			AssetLocator.AlphaPass.DepthStencilState = new DepthStencilState(true);
			AssetLocator.AlphaPass.Output = AssetLocator.MainWindow;
			AssetLocator.AlphaPass.PresentAfterPass = false;
			AssetLocator.AlphaPass.RasterizerState = new RasterizerState(false, TriangleCullMode.BackfaceCulling, false);
			AssetLocator.AlphaPass.VertexShader = simpleVS;
			AssetLocator.AlphaPass.MainGeomPass = AssetLocator.MainGeometryPass;

			// Hud Pass
			hudPass = new HUDPass("Hud Pass") {
				DepthStencilState = new DepthStencilState(false),
				Input = new Camera(),
				Output = AssetLocator.MainWindow,
				PresentAfterPass = true,
				RasterizerState = new RasterizerState(false, TriangleCullMode.NoCulling, false),
				VertexShader = hudVS,
				BlendState = new BlendState(BlendOperation.AlphaAwareSrcOverride)
			};
			hudPass.GlowVS = lightVS;
			hudPass.GlowShader = glowShader;
			hudPass.ScaleDownShader = copyShader;
			hudPass.ScaleUpShader = copyReverseShader;
			hudPass.AddLayer(AssetLocator.HudLayer);
			ConstantBufferBinding vpPropsBinding = (ConstantBufferBinding) hudVS.GetBindingByIdentifier("ViewportProperties");
			hudPass.PrePass += pass => {
				Vector4 vpSizePx = ((SceneViewport) AssetLocator.MainWindow).SizePixels * 0.5f;
				unsafe {
					vpPropsBinding.SetValue((byte*) (&vpSizePx));
				}
			};

			// Set camera + add passes
			AssetLocator.MainCamera = AssetLocator.MainGeometryPass.Input = AssetLocator.AlphaPass.Input = new Camera();
			AssetLocator.ShadowcasterCamera = shadowPass.LightCam = new Camera();
			AssetLocator.ShadowcasterCamera.OrthographicDimensions = new Vector3(1000f, 1000f, PhysicsManager.ONE_METRE_SCALED * 100f);
			RenderingModule.AddRenderPass(shadowPass);
			RenderingModule.AddRenderPass(AssetLocator.MainGeometryPass);
			RenderingModule.AddRenderPass(AssetLocator.LightPass);
			RenderingModule.AddRenderPass(AssetLocator.AlphaPass);
			RenderingModule.AddRenderPass(hudPass);
		}

		private static void InitResources() {
			// Game Objects
			GeometryCacheBuilder<DefaultVertex> gameObjectGCB = new GeometryCacheBuilder<DefaultVertex>();

			//		- Coin
			var coinModelStream = AssetLoader.LoadModel(Path.Combine(AssetLocator.ModelsDir, "LizardCoin.obj"));
			var coinModelVertices = coinModelStream.GetVertices<DefaultVertex>(typeof(DefaultVertex).GetConstructor(new[] { typeof(Vector3), typeof(Vector3), typeof(Vector3), typeof(Vector2) }));
			AssetLocator.LizardCoinModel = gameObjectGCB.AddModel(
				"Game Object: Lizard Coin",
				coinModelVertices,
				coinModelStream.GetIndices()
			);
			AssetLocator.LizardCoinMaterial = new Material("Game Object Mat: Lizard Coin", AssetLocator.GeometryFragmentShader);
			AssetLocator.LizardCoinMaterial.SetMaterialResource(
				(ResourceViewBinding) AssetLocator.GeometryFragmentShader.GetBindingByIdentifier("DiffuseMap"),
				AssetLocator.LoadTexture("LizardCoin.png", true).CreateView()
			);
			AssetLocator.LizardCoinMaterial.SetMaterialResource(
				(ResourceViewBinding) AssetLocator.GeometryFragmentShader.GetBindingByIdentifier("SpecularMap"),
				AssetLocator.LoadTexture("LizardCoin_s.png", true).CreateView()
			);
			AssetLocator.LizardCoinMaterial.SetMaterialResource(
				(ResourceViewBinding) AssetLocator.GeometryFragmentShader.GetBindingByIdentifier("NormalMap"),
				AssetLocator.LoadTexture("LizardCoin_n.png", true).CreateView()
			);
			AssetLocator.LizardCoinMaterial.SetMaterialResource(
				(ResourceViewBinding) AssetLocator.GeometryFragmentShader.GetBindingByIdentifier("EmissiveMap"),
				AssetLocator.LoadTexture("LizardCoin_e.png", true).CreateView()
			);
			AssetLocator.LizardCoinMaterial.SetMaterialConstantValue(
				(ConstantBufferBinding) AssetLocator.GeometryFragmentShader.GetBindingByIdentifier("MaterialProperties"),
				Vector4.ONE
			);
			AssetLocator.LizardCoinAlphaMaterial = new Material("Game Object Mat: Lizard Coin Alpha", AssetLocator.AlphaFragmentShader);
			AssetLocator.LizardCoinAlphaMaterial.SetMaterialResource(
				(ResourceViewBinding) AssetLocator.AlphaFragmentShader.GetBindingByIdentifier("DiffuseMap"),
				AssetLocator.LoadTexture("LizardCoin.png", true).CreateView()
			);
			AssetLocator.LizardCoinAlphaMaterial.SetMaterialConstantValue(
				(ConstantBufferBinding) AssetLocator.AlphaFragmentShader.GetBindingByIdentifier("MaterialProperties"),
				new Vector4(1f, 1f, 1f, 0.35f)
			);
			//AssetLocator.LizardCoinShape = PhysicsManager.CreateConvexHullShape(
			//	coinModelVertices.Select(v => v.Position),
			//	new CollisionShapeOptionsDesc(Vector3.ONE)
			//);
			AssetLocator.LizardCoinShape = PhysicsManager.CreateSimpleSphereShape(
				GameplayConstants.EGG_COLLISION_RADIUS * 1.15f,
				new CollisionShapeOptionsDesc(Vector3.ONE)
			);

			//		- Lizard Egg
			CreatePlayerballModels(gameObjectGCB);
			AssetLocator.LizardEggPhysicsShape = PhysicsManager.CreateSimpleSphereShape(GameplayConstants.EGG_COLLISION_RADIUS, new CollisionShapeOptionsDesc(Vector3.ONE));

			//		- Finishing Bell
			var finishingBellModelStream = AssetLoader.LoadModel(Path.Combine(AssetLocator.ModelsDir, "FinishBellBottom.obj"));
			var finishingBellVerts = finishingBellModelStream.GetVertices<DefaultVertex>(typeof(DefaultVertex).GetConstructor(new[] { typeof(Vector3), typeof(Vector3), typeof(Vector3), typeof(Vector2) }));
			var finishingBellIndices = finishingBellModelStream.GetIndices();
			for (int i = 0; i < finishingBellVerts.Length; i++) {
				Vector3 originalPos = finishingBellVerts[i].Position;
				finishingBellVerts[i] = new DefaultVertex(
					new Vector3(originalPos, x: -originalPos.X),
					new Vector3(finishingBellVerts[i].Normal, x: -finishingBellVerts[i].Normal.X),
					finishingBellVerts[i].TexUV
				);
			}
			for (int i = 0; i < finishingBellIndices.Length; i += 3) {
				uint index1 = finishingBellIndices[i];
				finishingBellIndices[i] = finishingBellIndices[i + 1];
				finishingBellIndices[i + 1] = index1;
			}
			AssetLocator.FinishingBellModel = gameObjectGCB.AddModel(
				"Game Object: Finishing Bell",
				finishingBellVerts,
				finishingBellIndices
			);
			AssetLocator.FinishingBellMaterial = new Material("Game Object Mat: Finishing Bell", AssetLocator.GeometryFragmentShader);
			AssetLocator.FinishingBellMaterial.SetMaterialResource(
				(ResourceViewBinding) AssetLocator.GeometryFragmentShader.GetBindingByIdentifier("DiffuseMap"),
				AssetLocator.LoadTexture("FinishBellBottom_bake.png", true).CreateView()
			);
			AssetLocator.FinishingBellMaterial.SetMaterialResource(
				(ResourceViewBinding) AssetLocator.GeometryFragmentShader.GetBindingByIdentifier("NormalMap"),
				AssetLocator.LoadTexture("FinishBellBottom_bake_n.png", true).CreateView()
			);
			AssetLocator.FinishingBellMaterial.SetMaterialResource(
				(ResourceViewBinding) AssetLocator.GeometryFragmentShader.GetBindingByIdentifier("SpecularMap"),
				AssetLocator.LoadTexture("FinishBellBottom_bake_s.png", true).CreateView()
			);
			AssetLocator.FinishingBellMaterial.SetMaterialResource(
				(ResourceViewBinding) AssetLocator.GeometryFragmentShader.GetBindingByIdentifier("EmissiveMap"),
				AssetLocator.LoadTexture("FinishBellBottom_bake_e.png", true).CreateView()
			);
			AssetLocator.FinishingBellMaterial.SetMaterialConstantValue(
				(ConstantBufferBinding) AssetLocator.GeometryFragmentShader.GetBindingByIdentifier("MaterialProperties"),
				new Vector4(0f, 0f, 0f, 0.85f)
			);
			AssetLocator.FinishingBellPhysicsShape = PhysicsManager.CreateConcaveHullShape(
				finishingBellVerts.Select(df => df.Position),
				finishingBellIndices.Select(i => (int) i),
				new CollisionShapeOptionsDesc(Vector3.ONE),
				AssetLocator.CreateACDFilePath("finishingBell")
			);

			var finishingBellTopModelStream = AssetLoader.LoadModel(Path.Combine(AssetLocator.ModelsDir, "FinishBellTop.obj"));
			var finishingBellTopVerts = finishingBellTopModelStream.GetVertices<DefaultVertex>(typeof(DefaultVertex).GetConstructor(new[] { typeof(Vector3), typeof(Vector3), typeof(Vector3), typeof(Vector2) }));
			var finishingBellTopIndices = finishingBellTopModelStream.GetIndices();
			for (int i = 0; i < finishingBellTopVerts.Length; i++) {
				Vector3 originalPos = finishingBellTopVerts[i].Position;
				finishingBellTopVerts[i] = new DefaultVertex(
					new Vector3(originalPos, x: -originalPos.X),
					new Vector3(finishingBellTopVerts[i].Normal, x: -finishingBellTopVerts[i].Normal.X),
					finishingBellTopVerts[i].TexUV
				);
			}
			for (int i = 0; i < finishingBellTopIndices.Length; i += 3) {
				uint index1 = finishingBellTopIndices[i];
				finishingBellTopIndices[i] = finishingBellTopIndices[i + 1];
				finishingBellTopIndices[i + 1] = index1;
			}
			AssetLocator.FinishingBellTopModel = gameObjectGCB.AddModel(
				"Game Object: Finishing Bell Top",
				finishingBellTopVerts,
				finishingBellTopIndices
			);
			AssetLocator.FinishingBellTopMaterial = new Material("Game Object Mat: Finishing Bell Top", AssetLocator.GeometryFragmentShader);
			AssetLocator.FinishingBellTopMaterial.SetMaterialResource(
				(ResourceViewBinding) AssetLocator.GeometryFragmentShader.GetBindingByIdentifier("DiffuseMap"),
				AssetLocator.LoadTexture("FinishBellTop_bake.png", true).CreateView()
			);
			AssetLocator.FinishingBellTopMaterial.SetMaterialResource(
				(ResourceViewBinding) AssetLocator.GeometryFragmentShader.GetBindingByIdentifier("NormalMap"),
				AssetLocator.LoadTexture("FinishBellTop_bake_n.png", true).CreateView()
			);
			AssetLocator.FinishingBellTopMaterial.SetMaterialResource(
				(ResourceViewBinding) AssetLocator.GeometryFragmentShader.GetBindingByIdentifier("SpecularMap"),
				AssetLocator.LoadTexture("FinishBellTop_bake_s.png", true).CreateView()
			);
			AssetLocator.FinishingBellTopMaterial.SetMaterialResource(
				(ResourceViewBinding) AssetLocator.GeometryFragmentShader.GetBindingByIdentifier("EmissiveMap"),
				AssetLocator.LoadTexture("Default_e.bmp", true).CreateView()
			);
			AssetLocator.FinishingBellTopMaterial.SetMaterialConstantValue(
				(ConstantBufferBinding) AssetLocator.GeometryFragmentShader.GetBindingByIdentifier("MaterialProperties"),
				new Vector4(1f, 1f, 0.8f, 1f)
			);
			AssetLocator.FinishingBellTopPhysicsShape = PhysicsManager.CreateConcaveHullShape(
				finishingBellTopVerts.Select(df => df.Position),
				finishingBellTopIndices.Select(i => (int) i),
				new CollisionShapeOptionsDesc(Vector3.ONE),
				AssetLocator.CreateACDFilePath("finishingBellTop")
			);

			//		- Vulture Egg
			var vultureEggModelStream = AssetLoader.LoadModel(Path.Combine(AssetLocator.ModelsDir, "Playerball.obj"));
			var vultureEggVerts = vultureEggModelStream.GetVertices<DefaultVertex>(typeof(DefaultVertex).GetConstructor(new[] { typeof(Vector3), typeof(Vector3), typeof(Vector3), typeof(Vector2) }));
			vultureEggVerts = vultureEggVerts.Select(
				vert => new DefaultVertex(
					vert.Position * GameplayConstants.VULTURE_EGG_COLLISION_RADIUS_MULTIPLIER,
					vert.Normal,
					vert.Tangent,
					vert.TexUV
				)
			).ToArray();
			var vultureEggIndices = vultureEggModelStream.GetIndices();
			AssetLocator.VultureEggModel = gameObjectGCB.AddModel(
				"Game Object: Vulture Egg",
				vultureEggVerts,
				vultureEggIndices
			);
			AssetLocator.MainGeometryPass._VEGG_MH = AssetLocator.VultureEggModel;
			AssetLocator.VultureEggMaterial = new Material("Game Object Mat: Vulture Egg", AssetLocator.GeometryFragmentShader);
			AssetLocator.VultureEggMaterial.SetMaterialResource(
				(ResourceViewBinding) AssetLocator.GeometryFragmentShader.GetBindingByIdentifier("DiffuseMap"),
				AssetLocator.LoadTexture("VEggTexture.png", true).CreateView()
			);
			AssetLocator.VultureEggMaterial.SetMaterialResource(
				(ResourceViewBinding) AssetLocator.GeometryFragmentShader.GetBindingByIdentifier("NormalMap"),
				AssetLocator.LoadTexture("PlayerTexture_n_0.png", true).CreateView()
			);
			AssetLocator.VultureEggMaterial.SetMaterialResource(
				(ResourceViewBinding) AssetLocator.GeometryFragmentShader.GetBindingByIdentifier("SpecularMap"),
				AssetLocator.LoadTexture("PlayerTexture_s_0.png", true).CreateView()
			);
			AssetLocator.VultureEggMaterial.SetMaterialResource(
				(ResourceViewBinding) AssetLocator.GeometryFragmentShader.GetBindingByIdentifier("EmissiveMap"),
				AssetLocator.LoadTexture("Default_e.bmp", true).CreateView()
			);
			AssetLocator.VultureEggMaterial.SetMaterialConstantValue(
				(ConstantBufferBinding) AssetLocator.GeometryFragmentShader.GetBindingByIdentifier("MaterialProperties"),
				new Vector4(1f, 1f, 1f, 0.2f)
			);
			AssetLocator.VultureEggPhysicsShape = PhysicsManager.CreateSimpleSphereShape(
				GameplayConstants.EGG_COLLISION_RADIUS * GameplayConstants.VULTURE_EGG_COLLISION_RADIUS_MULTIPLIER,
				new CollisionShapeOptionsDesc(Vector3.ONE)
			);

			//		- Silly Bits
			const int NUM_SILLY_BITS = 9;
			ModelHandle[] sillyBitsHandles = new ModelHandle[NUM_SILLY_BITS];
			Material[] sillyBitsMaterials = new Material[NUM_SILLY_BITS];
			PhysicsShapeHandle[] sillyBitsPhysicsShapes = new PhysicsShapeHandle[NUM_SILLY_BITS];
			for (int s = 0; s < NUM_SILLY_BITS; ++s) {
				string fileNameCommonPart;
				switch (s) {
					case 0: fileNameCommonPart = "SillyBits_Ball"; break;
					case 1: fileNameCommonPart = "SillyBits_Cactus"; break;
					case 2: fileNameCommonPart = "SillyBits_Fish"; break;
					case 3: fileNameCommonPart = "SillyBits_Flower"; break;
					case 4: fileNameCommonPart = "SillyBits_Hammer"; break;
					case 5: fileNameCommonPart = "SillyBits_Mushroom"; break;
					case 6: fileNameCommonPart = "SillyBits_MusicNote"; break;
					case 7: fileNameCommonPart = "SillyBits_Spring"; break;
					case 8: fileNameCommonPart = "SillyBits_Tail"; break;
					default: throw new ArgumentOutOfRangeException();
				}
				var sillyBitModelStream = AssetLoader.LoadModel(Path.Combine(AssetLocator.ModelsDir, fileNameCommonPart + ".obj"));
				var sillyBitVerts = sillyBitModelStream.GetVertices<DefaultVertex>(typeof(DefaultVertex).GetConstructor(new[] { typeof(Vector3), typeof(Vector3), typeof(Vector3), typeof(Vector2) }));
				var sillyBitIndices = sillyBitModelStream.GetIndices();
				sillyBitsHandles[s] = gameObjectGCB.AddModel(
					"Game Object: Silly Bit #" + s,
					sillyBitVerts,
					sillyBitIndices
				);
				sillyBitsMaterials[s] = new Material("Game Object Mat: Silly Bit #" + s, AssetLocator.GeometryFragmentShader);
				sillyBitsMaterials[s].SetMaterialResource(
					(ResourceViewBinding) AssetLocator.GeometryFragmentShader.GetBindingByIdentifier("DiffuseMap"),
					AssetLocator.LoadTexture(fileNameCommonPart + "_bake.png", true).CreateView()
				);
				sillyBitsMaterials[s].SetMaterialResource(
					(ResourceViewBinding) AssetLocator.GeometryFragmentShader.GetBindingByIdentifier("NormalMap"),
					AssetLocator.LoadTexture("Default_n.bmp", true).CreateView()
				);
				sillyBitsMaterials[s].SetMaterialResource(
					(ResourceViewBinding) AssetLocator.GeometryFragmentShader.GetBindingByIdentifier("SpecularMap"),
					AssetLocator.LoadTexture("Default_s.bmp", true).CreateView()
				);
				sillyBitsMaterials[s].SetMaterialResource(
					(ResourceViewBinding) AssetLocator.GeometryFragmentShader.GetBindingByIdentifier("EmissiveMap"),
					AssetLocator.LoadTexture("Default_e.bmp", true).CreateView()
				);
				sillyBitsMaterials[s].SetMaterialConstantValue(
					(ConstantBufferBinding) AssetLocator.GeometryFragmentShader.GetBindingByIdentifier("MaterialProperties"),
					new Vector4(0f, 0f, 0f, 0.2f)
				);
				sillyBitsPhysicsShapes[s] = PhysicsManager.CreateSimpleSphereShape(
					PhysicsManager.ONE_METRE_SCALED * 0.05f,
					new CollisionShapeOptionsDesc(Vector3.ONE)
				);
			}
			AssetLocator.SillyBitsHandles = sillyBitsHandles;
			AssetLocator.SillyBitsMaterials = sillyBitsMaterials;
			AssetLocator.SillyBitsPhysicsShapes = sillyBitsPhysicsShapes;

			//		- Star
			var starModelStream = AssetLoader.LoadModel(Path.Combine(AssetLocator.ModelsDir, "star.obj"));
			var starVerts = starModelStream.GetVertices<DefaultVertex>(typeof(DefaultVertex).GetConstructor(new[] { typeof(Vector3), typeof(Vector3), typeof(Vector3), typeof(Vector2) }));
			var starIndices = starModelStream.GetIndices();
			AssetLocator.StarModel = gameObjectGCB.AddModel(
				"Game Object: Star",
				starVerts,
				starIndices
			);
			AssetLocator.StarMaterialGold = new Material("Game Object Mat: Gold Star", AssetLocator.GeometryFragmentShader);
			AssetLocator.StarMaterialGold.SetMaterialResource(
				(ResourceViewBinding) AssetLocator.GeometryFragmentShader.GetBindingByIdentifier("DiffuseMap"),
				AssetLocator.LoadTexture("star_gold_bake.png", true).CreateView()
			);
			AssetLocator.StarMaterialGold.SetMaterialResource(
				(ResourceViewBinding) AssetLocator.GeometryFragmentShader.GetBindingByIdentifier("SpecularMap"),
				AssetLocator.LoadTexture("Default_s.bmp", true).CreateView()
			);
			AssetLocator.StarMaterialGold.SetMaterialResource(
				(ResourceViewBinding) AssetLocator.GeometryFragmentShader.GetBindingByIdentifier("NormalMap"),
				AssetLocator.LoadTexture("Default_n.bmp", true).CreateView()
			);
			AssetLocator.StarMaterialGold.SetMaterialResource(
				(ResourceViewBinding) AssetLocator.GeometryFragmentShader.GetBindingByIdentifier("EmissiveMap"),
				AssetLocator.LoadTexture("star_gold_bake_e.png", true).CreateView()
			);
			AssetLocator.StarMaterialGold.SetMaterialConstantValue(
				(ConstantBufferBinding) AssetLocator.GeometryFragmentShader.GetBindingByIdentifier("MaterialProperties"),
				Vector4.ONE
			);
			AssetLocator.StarMaterialSilver = new Material("Game Object Mat: Silver Star", AssetLocator.GeometryFragmentShader);
			AssetLocator.StarMaterialSilver.SetMaterialResource(
				(ResourceViewBinding) AssetLocator.GeometryFragmentShader.GetBindingByIdentifier("DiffuseMap"),
				AssetLocator.LoadTexture("star_silver_bake.png", true).CreateView()
			);
			AssetLocator.StarMaterialSilver.SetMaterialResource(
				(ResourceViewBinding) AssetLocator.GeometryFragmentShader.GetBindingByIdentifier("SpecularMap"),
				AssetLocator.LoadTexture("Default_s.bmp", true).CreateView()
			);
			AssetLocator.StarMaterialSilver.SetMaterialResource(
				(ResourceViewBinding) AssetLocator.GeometryFragmentShader.GetBindingByIdentifier("NormalMap"),
				AssetLocator.LoadTexture("Default_n.bmp", true).CreateView()
			);
			AssetLocator.StarMaterialSilver.SetMaterialResource(
				(ResourceViewBinding) AssetLocator.GeometryFragmentShader.GetBindingByIdentifier("EmissiveMap"),
				AssetLocator.LoadTexture("star_silver_bake_e.png", true).CreateView()
			);
			AssetLocator.StarMaterialSilver.SetMaterialConstantValue(
				(ConstantBufferBinding) AssetLocator.GeometryFragmentShader.GetBindingByIdentifier("MaterialProperties"),
				Vector4.ONE
			);
			AssetLocator.StarMaterialBronze = new Material("Game Object Mat: Bronze Star", AssetLocator.GeometryFragmentShader);
			AssetLocator.StarMaterialBronze.SetMaterialResource(
				(ResourceViewBinding) AssetLocator.GeometryFragmentShader.GetBindingByIdentifier("DiffuseMap"),
				AssetLocator.LoadTexture("star_bronze_bake.png", true).CreateView()
			);
			AssetLocator.StarMaterialBronze.SetMaterialResource(
				(ResourceViewBinding) AssetLocator.GeometryFragmentShader.GetBindingByIdentifier("SpecularMap"),
				AssetLocator.LoadTexture("Default_s.bmp", true).CreateView()
			);
			AssetLocator.StarMaterialBronze.SetMaterialResource(
				(ResourceViewBinding) AssetLocator.GeometryFragmentShader.GetBindingByIdentifier("NormalMap"),
				AssetLocator.LoadTexture("Default_n.bmp", true).CreateView()
			);
			AssetLocator.StarMaterialBronze.SetMaterialResource(
				(ResourceViewBinding) AssetLocator.GeometryFragmentShader.GetBindingByIdentifier("EmissiveMap"),
				AssetLocator.LoadTexture("star_bronze_bake_e.png", true).CreateView()
			);
			AssetLocator.StarMaterialBronze.SetMaterialConstantValue(
				(ConstantBufferBinding) AssetLocator.GeometryFragmentShader.GetBindingByIdentifier("MaterialProperties"),
				Vector4.ONE
			);
			AssetLocator.StarPhysicsShape = PhysicsManager.CreateSimpleSphereShape(GameplayConstants.EGG_COLLISION_RADIUS, new CollisionShapeOptionsDesc(Vector3.ONE));

			//		- Arrows
			var twistedArrowModelStream = AssetLoader.LoadModel(Path.Combine(AssetLocator.ModelsDir, "Rotaarrow.obj"));
			AssetLocator.TwistedArrowModel = gameObjectGCB.AddModel(
				"Game Object: Twisted Arrow",
				twistedArrowModelStream.GetVertices<DefaultVertex>(typeof(DefaultVertex).GetConstructor(new[] { typeof(Vector3), typeof(Vector3), typeof(Vector3), typeof(Vector2) })),
				twistedArrowModelStream.GetIndices()
			);
			AssetLocator.TwistedArrowMaterial = new Material("Game Object Mat: Twisted Arrow", AssetLocator.AlphaFragmentShader);
			AssetLocator.TwistedArrowMaterial.SetMaterialResource(
				(ResourceViewBinding) AssetLocator.AlphaFragmentShader.GetBindingByIdentifier("DiffuseMap"),
				AssetLocator.LoadTexture("VEggTexture.png", true).CreateView()
			);
			var staggeredArrowModelStream = AssetLoader.LoadModel(Path.Combine(AssetLocator.ModelsDir, "ScaleArrow.obj"));
			AssetLocator.WorldFlipArrowModel = gameObjectGCB.AddModel(
				"Game Object: Staggered Arrow",
				staggeredArrowModelStream.GetVerticesWithoutTangents<DefaultVertex>(typeof(DefaultVertex).GetConstructor(new[] { typeof(Vector3), typeof(Vector3), typeof(Vector2) }))
					.Select(vert => new DefaultVertex(vert.Position, vert.Normal, vert.Tangent, (vert.TexUV * 0.01f) + Vector2.ONE * 0.3f))
					.ToList(),
				staggeredArrowModelStream.GetIndices()
			);


			//		- HUD Texture Square
			HUDTexture.SetHUDTextureModel(
				gameObjectGCB.AddModel(
					"HUD Texture Model",
					new[] {
						new DefaultVertex(
							new Vector3(0f, 0f, 1f),
							Vector3.BACKWARD,
							Vector3.RIGHT,
							new Vector2(0f, 1f)
						),
						new DefaultVertex(
							new Vector3(1f, 0f, 1f),
							Vector3.BACKWARD,
							Vector3.RIGHT,
							new Vector2(1f, 1f)
						),
						new DefaultVertex(
							new Vector3(1f, 1f, 1f),
							Vector3.BACKWARD, 
							Vector3.RIGHT,
							new Vector2(1f, 0f)
						),
						new DefaultVertex(
							new Vector3(0f, 1f, 1f),
							Vector3.BACKWARD, 
							Vector3.RIGHT,
							new Vector2(0f, 0f)
						)
					},
					new[] { 0U, 1U, 3U, 1U, 2U, 3U }
				)
			);

			gameObjectCache = gameObjectGCB.Build();
			AssetLocator.MainGeometryPass.SetVSForCache(gameObjectCache, AssetLocator.MainGeometryVertexShader);

			// HUD Objects
			AssetLocator.EggCounterBackdrop = AssetLocator.LoadTexture("HUD_EggDisplay.png");
			AssetLocator.VultureEgg = AssetLocator.LoadTexture("HUD_VultureEggColor.png");
			AssetLocator.VultureEggInvert = AssetLocator.LoadTexture("HUD_VultureEggColorInvert.png");
			AssetLocator.VultureEggDimmed = AssetLocator.LoadTexture("HUD_VultureEggGray.png");
			AssetLocator.VultureEggCross = AssetLocator.LoadTexture("HUD_EggX.png");
			AssetLocator.TimerBackdrop = AssetLocator.LoadTexture("HUD_MainTimerElement.png");
			AssetLocator.TimerBackdropGlass = AssetLocator.LoadTexture("HUD_MainTimerElementGlass.png");
			ITexture2D[] timerFigures = new ITexture2D[10 * 15];
			for (int i = 0; i < 10; ++i) {
				for (int j = 1; j <= 15; ++j) {
					timerFigures[i * 15 + (j - 1)] = AssetLocator.LoadTexture(
						(9 - i) + "_" + (i < 9 ? 8 - i : 9) + "_00" + (j >= 10 ? j.ToString() : "0" + j) + ".png"
					);
				}
			}
			AssetLocator.TimerFigures = timerFigures;
			AssetLocator.LevelNameBackdrop = AssetLocator.LoadTexture("HUD_MapNameElement.png");
			AssetLocator.BronzeStarTimeBackdrop = AssetLocator.LoadTexture("HUD_TimeBronze.png");
			AssetLocator.SilverStarTimeBackdrop = AssetLocator.LoadTexture("HUD_TimeSilver.png");
			AssetLocator.GoldStarTimeBackdrop = AssetLocator.LoadTexture("HUD_TimeGold.png");
			AssetLocator.BronzeStar = AssetLocator.LoadTexture("HUD_StarBronze.png");
			AssetLocator.SilverStar = AssetLocator.LoadTexture("HUD_StarSilver.png");
			AssetLocator.GoldStar = AssetLocator.LoadTexture("HUD_StarGold.png");
			AssetLocator.PersonalBestTimeBackdrop = AssetLocator.LoadTexture("HUD_FriendTimesTop.png");
			AssetLocator.FriendsGoldTimeBackdrop = AssetLocator.LoadTexture("HUD_FriendTimesGold.png");
			AssetLocator.FriendsSilverTimeBackdrop = AssetLocator.LoadTexture("HUD_FriendTimesSilver.png");
			AssetLocator.FriendsBronzeTimeBackdrop = AssetLocator.LoadTexture("HUD_FriendTimesBronze.png");
			AssetLocator.FinishingBell = AssetLocator.LoadTexture("HUD_Bell.png");
			AssetLocator.FriendsGoldTimeToken = AssetLocator.LoadTexture("goldnumber.png");
			AssetLocator.FriendsSilverTimeToken = AssetLocator.LoadTexture("silvernumber.png");
			AssetLocator.FriendsBronzeTimeToken = AssetLocator.LoadTexture("bronzenumber.png");
			AssetLocator.PassMessageTex = AssetLocator.LoadTexture("Pass.png");
			AssetLocator.FailMessageTex = AssetLocator.LoadTexture("Fail.png");
			AssetLocator.DroppedMessageTex = AssetLocator.LoadTexture("Dropped.png");
			AssetLocator.TimeUpMessageTex = AssetLocator.LoadTexture("TimeUp.png");
			AssetLocator.ReadyTex = AssetLocator.LoadTexture("READY.png");
			AssetLocator.GoTex = AssetLocator.LoadTexture("GO.png");
			AssetLocator.CoinBackdrop = AssetLocator.LoadTexture("HUD_CoinElement.png");
			ITexture2D[] coinFrames = new ITexture2D[32];
			for (int i = 0; i < 32; ++i) {
				coinFrames[i] = AssetLocator.LoadTexture(
					"CoinAnimation" + (i + 1).ToString("0000") + ".png"
				);
			}
			AssetLocator.CoinFrames = coinFrames;
			AssetLocator.ELLogo = AssetLocator.LoadTexture("elLogo.png");
			AssetLocator.MainMenuPlayButton = AssetLocator.LoadTexture("mainMenuButtonPlay.png");
			AssetLocator.MainMenuMedalsButton = AssetLocator.LoadTexture("mainMenuButtonMedal.png");
			AssetLocator.MainMenuOptionsButton = AssetLocator.LoadTexture("mainMenuButtonOption.png");
			AssetLocator.MainMenuExitButton = AssetLocator.LoadTexture("mainMenuButtonExit.png");
			AssetLocator.MainMenuPlayButtonFront = AssetLocator.LoadTexture("mainMenuButtonPlayFront.png");
			AssetLocator.MainMenuMedalsButtonFront = AssetLocator.LoadTexture("mainMenuButtonMedalFront.png");
			AssetLocator.MainMenuOptionsButtonFront = AssetLocator.LoadTexture("mainMenuButtonOptionFront.png");
			AssetLocator.MainMenuExitButtonFront = AssetLocator.LoadTexture("mainMenuButtonExitFront.png");
			AssetLocator.MainMenuButtonRing = AssetLocator.LoadTexture("mainMenuButtonRing.png");
			AssetLocator.LineSeparator = AssetLocator.LoadTexture("LineSeparator.png");
			AssetLocator.LineSelectionFill = AssetLocator.LoadTexture("LineSelectionFill.png");
			AssetLocator.Chevrons = AssetLocator.LoadTexture("Chevrons.png");
			AssetLocator.WorldIcons = new ITexture2D[11] {
				AssetLocator.LoadTexture("worldIconHomeForest.png"),
				AssetLocator.LoadTexture("worldIconHumanTown.png"),
				AssetLocator.LoadTexture("worldIconChinatown.png"),
				AssetLocator.LoadTexture("worldIconWorkshop.png"),
				AssetLocator.LoadTexture("worldIconFactory.png"),
				AssetLocator.LoadTexture("worldIconDesert.png"),
				AssetLocator.LoadTexture("worldIconDarkCity.png"),
				AssetLocator.LoadTexture("worldIconRustySewer.png"),
				AssetLocator.LoadTexture("worldIconIcyPlain.png"),
				AssetLocator.LoadTexture("worldIconPearlyPath.png"),
				AssetLocator.LoadTexture("worldIconElysianFields.png")
			};
			AssetLocator.WorldIconsGreyed = new ITexture2D[11] {
				AssetLocator.LoadTexture("worldIconHomeForestLocked.png"),
				AssetLocator.LoadTexture("worldIconHumanTownLocked.png"),
				AssetLocator.LoadTexture("worldIconChinatownLocked.png"),
				AssetLocator.LoadTexture("worldIconWorkshopLocked.png"),
				AssetLocator.LoadTexture("worldIconFactoryLocked.png"),
				AssetLocator.LoadTexture("worldIconDesertLocked.png"),
				AssetLocator.LoadTexture("worldIconDarkCityLocked.png"),
				AssetLocator.LoadTexture("worldIconRustySewerLocked.png"),
				AssetLocator.LoadTexture("worldIconIcyPlainLocked.png"),
				AssetLocator.LoadTexture("worldIconPearlyPathLocked.png"),
				AssetLocator.LoadTexture("worldIconElysianFieldsLocked.png")
			};
			AssetLocator.LockedWorldIcons = new ITexture2D[11] {
				AssetLocator.LoadTexture("worldButtonHomeForestLocked.png"),
				AssetLocator.LoadTexture("worldButtonHumanTownLocked.png"),
				AssetLocator.LoadTexture("worldButtonChinatownLocked.png"),
				AssetLocator.LoadTexture("worldButtonWorkshopLocked.png"),
				AssetLocator.LoadTexture("worldButtonFactoryLocked.png"),
				AssetLocator.LoadTexture("worldButtonDesertLocked.png"),
				AssetLocator.LoadTexture("worldButtonDarkCityLocked.png"),
				AssetLocator.LoadTexture("worldButtonRustySewerLocked.png"),
				AssetLocator.LoadTexture("worldButtonIcyPlainLocked.png"),
				AssetLocator.LoadTexture("worldButtonPearlyPathLocked.png"),
				AssetLocator.LoadTexture("worldButtonElysianFieldsLocked.png")
			};
			AssetLocator.UnlockedWorldIcons = new ITexture2D[11] {
				AssetLocator.LoadTexture("worldButtonHomeForestUnlocked.png"),
				AssetLocator.LoadTexture("worldButtonHumanTownUnlocked.png"),
				AssetLocator.LoadTexture("worldButtonChinatownUnlocked.png"),
				AssetLocator.LoadTexture("worldButtonWorkshopUnlocked.png"),
				AssetLocator.LoadTexture("worldButtonFactoryUnlocked.png"),
				AssetLocator.LoadTexture("worldButtonDesertUnlocked.png"),
				AssetLocator.LoadTexture("worldButtonDarkCityUnlocked.png"),
				AssetLocator.LoadTexture("worldButtonRustySewerUnlocked.png"),
				AssetLocator.LoadTexture("worldButtonIcyPlainUnlocked.png"),
				AssetLocator.LoadTexture("worldButtonPearlyPathUnlocked.png"),
				AssetLocator.LoadTexture("worldButtonElysianFieldsUnlocked.png")
			};
			AssetLocator.PlayMenuMainPanel = AssetLocator.LoadTexture("playMenuMainPanel.png");
			AssetLocator.PlayMenuSecondaryPanel = AssetLocator.LoadTexture("playMenuSecondaryPanel.png");
			AssetLocator.PlayMenuLevelBar = AssetLocator.LoadTexture("playMenuLevelBar.png");
			AssetLocator.PlayMenuLevelBarHighlight = AssetLocator.LoadTexture("playMenuLevelBarHighlight.png");
			AssetLocator.PlayMenuWorldButtonHighlight = AssetLocator.LoadTexture("worldButtonHighlight.png");
			AssetLocator.PlayMenuWorldButtonUnlockedHighlight = AssetLocator.LoadTexture("worldButtonHighlightUnlocked.png");
			AssetLocator.StarIndent = AssetLocator.LoadTexture("StarIndent.png");
			AssetLocator.EggIndent = AssetLocator.LoadTexture("EggIndent.png");
			AssetLocator.CoinIndent = AssetLocator.LoadTexture("CoinIndent.png");
			AssetLocator.PreviewImageBracket = AssetLocator.LoadTexture("PreviewImageBracket.png");
			AssetLocator.PreviewIndent = AssetLocator.LoadTexture("PreviewIndent.png");
			AssetLocator.GreyedOutMaterial = new Material("Greyed Out Mat", AssetLocator.GeometryFragmentShader);
			AssetLocator.GreyedOutMaterial.SetMaterialResource(
				(ResourceViewBinding) AssetLocator.GeometryFragmentShader.GetBindingByIdentifier("DiffuseMap"),
				AssetLocator.LoadTexture("grey.bmp", true).CreateView()
			);
			AssetLocator.GreyedOutMaterial.SetMaterialResource(
				(ResourceViewBinding) AssetLocator.GeometryFragmentShader.GetBindingByIdentifier("SpecularMap"),
				AssetLocator.LoadTexture("Default_s.bmp", true).CreateView()
			);
			AssetLocator.GreyedOutMaterial.SetMaterialResource(
				(ResourceViewBinding) AssetLocator.GeometryFragmentShader.GetBindingByIdentifier("NormalMap"),
				AssetLocator.LoadTexture("Default_n.bmp", true).CreateView()
			);
			AssetLocator.GreyedOutMaterial.SetMaterialResource(
				(ResourceViewBinding) AssetLocator.GeometryFragmentShader.GetBindingByIdentifier("EmissiveMap"),
				AssetLocator.LoadTexture("Default_e.bmp", true).CreateView()
			);
			AssetLocator.LoadingScreen = AssetLocator.LoadTexture("loadingScreen.png");
			AssetLocator.MedalLevelBackdrop = AssetLocator.LoadTexture("medalLevelBackdrop.png");
			AssetLocator.White1x1 = AssetLocator.LoadTexture("white.bmp");
			AssetLocator.TutorialArrow = AssetLocator.LoadTexture("tutArrow.png");
			AssetLocator.HopArrow = AssetLocator.LoadTexture("hopArrow.png");
			AssetLocator.Tick = AssetLocator.LoadTexture("tick.png");
			AssetLocator.IntroGoalIndicatorCircle = AssetLocator.LoadTexture("introIndicatorGoal.png");
			AssetLocator.IntroGoalIndicatorArrow = AssetLocator.LoadTexture("introIndicatorGoalArrow.png");
			AssetLocator.IntroSpawnIndicatorCircle = AssetLocator.LoadTexture("introIndicatorSpawn.png");
			AssetLocator.IntroSpawnIndicatorArrow = AssetLocator.LoadTexture("introIndicatorSpawnArrow.png");
			AssetLocator.ReloadingIcon = AssetLocator.LoadTexture("reloadingIcon.png");
			AssetLocator.DifficultyEasyIcon = AssetLocator.LoadTexture("difficulty_easy.png");
			AssetLocator.DifficultyTrickyIcon = AssetLocator.LoadTexture("difficulty_tricky.png");
			AssetLocator.DifficultyHardIcon = AssetLocator.LoadTexture("difficulty_hard.png");
			AssetLocator.DifficultyVeryHardIcon = AssetLocator.LoadTexture("difficulty_veryhard.png");


			// Audio
			//		- Bounce (Acute)
			AssetLocator.AcuteBounceSound = Path.Combine(AssetLocator.AudioDir, "bounce_acute.wav");
			AudioModule.LoadSound(AssetLocator.AcuteBounceSound, "bounce");
			//		- Bounce (Obtuse)
			AssetLocator.ObtuseBounceSound = Path.Combine(AssetLocator.AudioDir, "bounce_obtuse.wav");
			AudioModule.LoadSound(AssetLocator.ObtuseBounceSound, "bounce");
			//		- Bounce (All)
			AssetLocator.AllBounceSounds = new[] {
				Path.Combine(AssetLocator.AudioDir, "bounce_all_1.wav"),
				Path.Combine(AssetLocator.AudioDir, "bounce_all_2.wav"),
				Path.Combine(AssetLocator.AudioDir, "bounce_all_3.wav"),
				Path.Combine(AssetLocator.AudioDir, "bounce_all_4.wav")
			};
			foreach (var sound in AssetLocator.AllBounceSounds) {
				AudioModule.LoadSound(sound, "bounce");
			}

			//		- Roll
			AssetLocator.RollSound = new LoopingSound(Path.Combine(AssetLocator.AudioDir, "roll.wav"));
			AudioModule.LoadSound(AssetLocator.RollSound.File, "roll");
			int instanceId = AudioModule.CreateSoundInstance(AssetLocator.RollSound.File);
			AssetLocator.RollSound.AddInstance(instanceId);

			//		- Impact
			const string AUDIO_CHANNEL_IMPACT = "impact";
			AssetLocator.ImpactSounds = new[] {
				Path.Combine(AssetLocator.AudioDir, "impact_1.wav"),
				Path.Combine(AssetLocator.AudioDir, "impact_2.wav"),
				Path.Combine(AssetLocator.AudioDir, "impact_3.wav"),
			};
			foreach (var sound in AssetLocator.ImpactSounds) {
				AudioModule.LoadSound(sound, AUDIO_CHANNEL_IMPACT);
			}

			//		- Highspeed (Up)
			const string AUDIO_CHANNEL_HIGHSPEED_UP = "highspeedUp";
			AssetLocator.HighSpeedUpSounds = new[] {
				Path.Combine(AssetLocator.AudioDir, "highspeed_up_1.wav"),
				Path.Combine(AssetLocator.AudioDir, "highspeed_up_2.wav"),
			};
			foreach (var sound in AssetLocator.HighSpeedUpSounds) {
				AudioModule.LoadSound(sound, AUDIO_CHANNEL_HIGHSPEED_UP);
			}

			//		- Highspeed (Down)
			const string AUDIO_CHANNEL_HIGHSPEED_DOWN = "highspeedDown";
			AssetLocator.HighSpeedDownSounds = new[] {
				Path.Combine(AssetLocator.AudioDir, "highspeed_down_1.wav"),
				Path.Combine(AssetLocator.AudioDir, "highspeed_down_2.wav"),
				Path.Combine(AssetLocator.AudioDir, "highspeed_down_3.wav"),
				Path.Combine(AssetLocator.AudioDir, "highspeed_down_4.wav"),
			};
			foreach (var sound in AssetLocator.HighSpeedDownSounds) {
				AudioModule.LoadSound(sound, AUDIO_CHANNEL_HIGHSPEED_DOWN);
			}

			//		- Highspeed (Parallel)
			const string AUDIO_CHANNEL_HIGHSPEED_PARALLEL = "highspeedParallel";
			AssetLocator.HighSpeedParallelSounds = new[] {
				Path.Combine(AssetLocator.AudioDir, "highspeed_parallel_1.wav"),
				Path.Combine(AssetLocator.AudioDir, "highspeed_parallel_2.wav"),
			};
			foreach (var sound in AssetLocator.HighSpeedParallelSounds) {
				AudioModule.LoadSound(sound, AUDIO_CHANNEL_HIGHSPEED_PARALLEL);
			}

			//		- Emotes (Pass)
			const string AUDIO_CHANNEL_EMOTE = "emote";
			AssetLocator.EmotePassSounds = new[] {
				Path.Combine(AssetLocator.AudioDir, "emote_pass_1.wav"),
				Path.Combine(AssetLocator.AudioDir, "emote_pass_2.wav"),
				Path.Combine(AssetLocator.AudioDir, "emote_pass_3.wav"),
			};
			foreach (var sound in AssetLocator.EmotePassSounds) {
				AudioModule.LoadSound(sound, AUDIO_CHANNEL_EMOTE);
			}

			//		- Emotes (Fail)
			AssetLocator.EmoteFailSounds = new[] {
				Path.Combine(AssetLocator.AudioDir, "emote_fail_1.wav"),
				Path.Combine(AssetLocator.AudioDir, "emote_fail_2.wav"),
				Path.Combine(AssetLocator.AudioDir, "emote_fail_3.wav"),
			};
			foreach (var sound in AssetLocator.EmoteFailSounds) {
				AudioModule.LoadSound(sound, AUDIO_CHANNEL_EMOTE);
			}

			//		- Fail Aww
			const string AUDIO_CHANNEL_FAIL_AWW = "failaww";
			AssetLocator.FailAwwSounds = new[] {
				Path.Combine(AssetLocator.AudioDir, "failaww_1.wav"),
				Path.Combine(AssetLocator.AudioDir, "failaww_2.wav"),
			};
			foreach (var sound in AssetLocator.FailAwwSounds) {
				AudioModule.LoadSound(sound, AUDIO_CHANNEL_FAIL_AWW);
			}

			//		- Fail (Fall)
			AssetLocator.FailFallSound = Path.Combine(AssetLocator.AudioDir, "fail_fall.wav");
			AudioModule.LoadSound(AssetLocator.FailFallSound, "failFall");

			//		- Fail (Timeout)
			AssetLocator.FailTimeoutSound = Path.Combine(AssetLocator.AudioDir, "fail_timeout.wav");
			AudioModule.LoadSound(AssetLocator.FailTimeoutSound, "failTimeout");

			//		- Countdown Loop
			AssetLocator.CountdownLoopSound = new LoopingSound(Path.Combine(AssetLocator.AudioDir, "countdown.wav"));
			AudioModule.LoadSound(AssetLocator.CountdownLoopSound.File, "countdown");
			AssetLocator.CountdownLoopSound.AddInstance(AudioModule.CreateSoundInstance(AssetLocator.CountdownLoopSound.File));

			//		- Bell
			AssetLocator.BellSound = Path.Combine(AssetLocator.AudioDir, "bell.wav");
			AudioModule.LoadSound(AssetLocator.BellSound, "bell");

			//		- Tick
			AssetLocator.TickSound = Path.Combine(AssetLocator.AudioDir, "tick.wav");
			AudioModule.LoadSound(AssetLocator.TickSound, "tick");

			//		- Egg Crunch
			AssetLocator.EggCrunchSound = Path.Combine(AssetLocator.AudioDir, "eggcrunch.wav");
			AudioModule.LoadSound(AssetLocator.EggCrunchSound, "eggcrunch");

			//		- Vulture Cry
			AssetLocator.VultureCrySound = Path.Combine(AssetLocator.AudioDir, "vulturecry.wav");
			AudioModule.LoadSound(AssetLocator.VultureCrySound, "vulturecry");

			//		- Star Fall
			AssetLocator.StarFallSound = Path.Combine(AssetLocator.AudioDir, "starfall.wav");
			AudioModule.LoadSound(AssetLocator.StarFallSound, "starfall");

			//		- World Flip
			AssetLocator.WorldFlipSound = Path.Combine(AssetLocator.AudioDir, "flip.wav");
			AudioModule.LoadSound(AssetLocator.WorldFlipSound, "worldflip");

			//		- Pass
			AssetLocator.PassSound = Path.Combine(AssetLocator.AudioDir, "pass.wav");
			AudioModule.LoadSound(AssetLocator.PassSound, "pass");
			//		- Pass (Gold)
			AssetLocator.PassGoldSound = Path.Combine(AssetLocator.AudioDir, "pass_gold.wav");
			AudioModule.LoadSound(AssetLocator.PassGoldSound, "pass");

			//		- Post-Pass (Time)
			AssetLocator.PostPassTimeSound = Path.Combine(AssetLocator.AudioDir, "postpass_time.wav");
			AudioModule.LoadSound(AssetLocator.PostPassTimeSound, "postpass");

			//		- Post-Pass (Bronze Star)
			AssetLocator.PostPassBronzeStarSound = Path.Combine(AssetLocator.AudioDir, "postpass_bronzestar.wav");
			AudioModule.LoadSound(AssetLocator.PostPassBronzeStarSound, "postpass");
			//		- Post-Pass (Silver Star)
			AssetLocator.PostPassSilverStarSound = Path.Combine(AssetLocator.AudioDir, "postpass_silverstar.wav");
			AudioModule.LoadSound(AssetLocator.PostPassSilverStarSound, "postpass");
			//		- Post-Pass (Gold Star)
			AssetLocator.PostPassGoldStarSound = Path.Combine(AssetLocator.AudioDir, "postpass_goldstar.wav");
			AudioModule.LoadSound(AssetLocator.PostPassGoldStarSound, "postpass");

			//		- Post-Pass (Bronze Medal)
			AssetLocator.PostPassBronzeMedalSound = Path.Combine(AssetLocator.AudioDir, "postpass_bronzemedal.wav");
			AudioModule.LoadSound(AssetLocator.PostPassBronzeMedalSound, "postpass");
			//		- Post-Pass (Silver Medal)
			AssetLocator.PostPassSilverMedalSound = Path.Combine(AssetLocator.AudioDir, "postpass_silvermedal.wav");
			AudioModule.LoadSound(AssetLocator.PostPassSilverMedalSound, "postpass");
			//		- Post-Pass (Gold Medal)
			AssetLocator.PostPassGoldMedalSound = Path.Combine(AssetLocator.AudioDir, "postpass_goldmedal.wav");
			AudioModule.LoadSound(AssetLocator.PostPassGoldMedalSound, "postpass");

			//		- Post-Pass (Personal Best)
			AssetLocator.PostPassPersonalBestSound = Path.Combine(AssetLocator.AudioDir, "postpass_personalbest.wav");
			AudioModule.LoadSound(AssetLocator.PostPassPersonalBestSound, "postpass");

			//		- Post-Pass (Line Reveal)
			AssetLocator.PostPassLineRevealSound = Path.Combine(AssetLocator.AudioDir, "postpass_linereveal.wav");
			AudioModule.LoadSound(AssetLocator.PostPassLineRevealSound, "postpass");

			//		- Post-Pass (Leaderboard)
			AssetLocator.PostPassLeaderboardSound = Path.Combine(AssetLocator.AudioDir, "postpass_leaderboard.wav");
			AudioModule.LoadSound(AssetLocator.PostPassLeaderboardSound, "postpass");

			//		- Post-Pass (Show Menu)
			AssetLocator.PostPassShowMenuSound = Path.Combine(AssetLocator.AudioDir, "postpass_showmenu.wav");
			AudioModule.LoadSound(AssetLocator.PostPassShowMenuSound, "postpass");

			//		- Post-Pass (Egg Pop)
			AssetLocator.PostPassEggPopSound = Path.Combine(AssetLocator.AudioDir, "postpass_eggpop.wav");
			AudioModule.LoadSound(AssetLocator.PostPassEggPopSound, "postpass");
			//		- Post-Pass (Smashed Egg)
			AssetLocator.PostPassSmashedEggSound = Path.Combine(AssetLocator.AudioDir, "postpass_smashedegg.wav");
			AudioModule.LoadSound(AssetLocator.PostPassSmashedEggSound, "postpass");
			//		- Post-Pass (Intact Egg)
			AssetLocator.PostPassIntactEggSound = Path.Combine(AssetLocator.AudioDir, "postpass_intactegg.wav");
			AudioModule.LoadSound(AssetLocator.PostPassIntactEggSound, "postpass");
			//		- Post-Pass (All Eggs)
			AssetLocator.PostPassAllEggsSound = Path.Combine(AssetLocator.AudioDir, "postpass_alleggs.wav");
			AudioModule.LoadSound(AssetLocator.PostPassAllEggsSound, "postpass");

			//		- Post-Pass (Star Spawn)
			AssetLocator.PostPassStarSpawnSound = Path.Combine(AssetLocator.AudioDir, "postpass_starspawn.wav");
			AudioModule.LoadSound(AssetLocator.PostPassStarSpawnSound, "postpass_starspawn");

			//		- Post-Pass (Gold Applause)
			AssetLocator.PostPassGoldApplauseSound = new LoopingSound(Path.Combine(AssetLocator.AudioDir, "postpass_goldapplause.wav"));
			AudioModule.LoadSound(AssetLocator.PostPassGoldApplauseSound.File, "postpass_applause");
			AssetLocator.PostPassGoldApplauseSound.AddInstance(AudioModule.CreateSoundInstance(AssetLocator.PostPassGoldApplauseSound.File));
			//		- Post-Pass (Silver Applause)
			AssetLocator.PostPassSilverApplauseSound = new LoopingSound(Path.Combine(AssetLocator.AudioDir, "postpass_silverapplause.wav"));
			AudioModule.LoadSound(AssetLocator.PostPassSilverApplauseSound.File, "postpass_applause");
			AssetLocator.PostPassSilverApplauseSound.AddInstance(AudioModule.CreateSoundInstance(AssetLocator.PostPassSilverApplauseSound.File));
			//		- Post-Pass (Bronze Applause)
			AssetLocator.PostPassBronzeApplauseSound = new LoopingSound(Path.Combine(AssetLocator.AudioDir, "postpass_bronzeapplause.wav"));
			AudioModule.LoadSound(AssetLocator.PostPassBronzeApplauseSound.File, "postpass_applause");
			AssetLocator.PostPassBronzeApplauseSound.AddInstance(AudioModule.CreateSoundInstance(AssetLocator.PostPassBronzeApplauseSound.File));

			//		- UI (Option Change)
			AssetLocator.UIOptionChangeSound = Path.Combine(AssetLocator.AudioDir, "ui_optionchange.wav");
			AudioModule.LoadSound(AssetLocator.UIOptionChangeSound, "ui");

			//		- UI (Option Select)
			AssetLocator.UIOptionSelectSound = Path.Combine(AssetLocator.AudioDir, "ui_optionselect.wav");
			AudioModule.LoadSound(AssetLocator.UIOptionSelectSound, "ui");

			//		- UI (Option Select Negative)
			AssetLocator.UIOptionSelectNegativeSound = Path.Combine(AssetLocator.AudioDir, "ui_optionselectnegative.wav");
			AudioModule.LoadSound(AssetLocator.UIOptionSelectNegativeSound, "ui");

			//		- Intro (Ready)
			AssetLocator.IntroReadySound = Path.Combine(AssetLocator.AudioDir, "ready.wav");
			AudioModule.LoadSound(AssetLocator.IntroReadySound, "intro");
			//		- Intro (Go)
			AssetLocator.IntroGoSound = Path.Combine(AssetLocator.AudioDir, "go.wav");
			AudioModule.LoadSound(AssetLocator.IntroGoSound, "intro");

			//		- Unavailable
			AssetLocator.UnavailableSound = Path.Combine(AssetLocator.AudioDir, "unavailable.wav");
			AudioModule.LoadSound(AssetLocator.UnavailableSound, "unavailable");

			//		- Hop
			AssetLocator.HopSound = Path.Combine(AssetLocator.AudioDir, "hop.wav");
			AudioModule.LoadSound(AssetLocator.HopSound, "hop");
			AssetLocator.ShowHopSound = Path.Combine(AssetLocator.AudioDir, "show_hop.wav");
			AudioModule.LoadSound(AssetLocator.ShowHopSound, "showhop");

			//		- Coin
			AssetLocator.CoinSound = Path.Combine(AssetLocator.AudioDir, "coin.wav");
			AudioModule.LoadSound(AssetLocator.CoinSound, "coin");
			//		- Coin (Final)
			AssetLocator.FinalCoinSound = Path.Combine(AssetLocator.AudioDir, "coinFinal.wav");
			AudioModule.LoadSound(AssetLocator.FinalCoinSound, "coin");
			//		- Coin (Stale)
			AssetLocator.StaleCoinSound = Path.Combine(AssetLocator.AudioDir, "coinStale.wav");
			AudioModule.LoadSound(AssetLocator.StaleCoinSound, "coin");

			//		- BGM
			AssetLocator.BackgroundMusic = new LoopingSound[12];
			for (int i = 0; i < 11; ++i) {
				var filePath = Path.Combine(AssetLocator.AudioDir, @"BGM\") + i + ".mp3";
				if (!File.Exists(filePath)) continue;
				AssetLocator.BackgroundMusic[i] = new LoopingSound(filePath);
				AudioModule.LoadSound(AssetLocator.BackgroundMusic[i].File, "bgm");
				AssetLocator.BackgroundMusic[i].AddInstance(AudioModule.CreateSoundInstance(AssetLocator.BackgroundMusic[i].File));
			}
			AssetLocator.BackgroundMusic[11] = new LoopingSound(Path.Combine(AssetLocator.AudioDir, @"BGM\") + "title.mp3");
			AudioModule.LoadSound(AssetLocator.BackgroundMusic[11].File, "bgm");
			AssetLocator.BackgroundMusic[11].AddInstance(AudioModule.CreateSoundInstance(AssetLocator.BackgroundMusic[11].File));

			//		- Xtra Sounds
			AssetLocator.XMenuLoad = Path.Combine(AssetLocator.AudioDir, "x_menu_load.wav");
			AudioModule.LoadSound(AssetLocator.XMenuLoad, "x");
			AssetLocator.XAnnounceMain = Path.Combine(AssetLocator.AudioDir, "x_announce_main.wav");
			AudioModule.LoadSound(AssetLocator.XAnnounceMain, "x");
			AssetLocator.XAnnounceWorldSelect = Path.Combine(AssetLocator.AudioDir, "x_announce_world.wav");
			AudioModule.LoadSound(AssetLocator.XAnnounceWorldSelect, "x");
			AssetLocator.XAnnounceLevelSelect = Path.Combine(AssetLocator.AudioDir, "x_announce_level.wav");
			AudioModule.LoadSound(AssetLocator.XAnnounceLevelSelect, "x");
			AssetLocator.XAnnounceMedals = Path.Combine(AssetLocator.AudioDir, "x_announce_medals.wav");
			AudioModule.LoadSound(AssetLocator.XAnnounceMedals, "x");
			AssetLocator.XAnnounceOptions = Path.Combine(AssetLocator.AudioDir, "x_announce_options.wav");
			AudioModule.LoadSound(AssetLocator.XAnnounceOptions, "x");
			AssetLocator.XAnnounceExit = Path.Combine(AssetLocator.AudioDir, "x_announce_exit.wav");
			AudioModule.LoadSound(AssetLocator.XAnnounceExit, "x");

			// Fonts
			AssetLocator.MainFont = Font.Load(Path.Combine(AssetLocator.FontsDir, "mainFont.fnt"), AssetLocator.HUDTextFragmentShader, Config.MainFontLineHeight, Config.MainFontKerning);
			AssetLocator.TitleFont = Font.Load(Path.Combine(AssetLocator.FontsDir, "titleFont.fnt"), AssetLocator.HUDTextFragmentShader, Config.TitleFontLineHeight, Config.TitleFontKerning);
			AssetLocator.TitleFontGlow = Font.Load(Path.Combine(AssetLocator.FontsDir, "titleFontGlow.fnt"), AssetLocator.HUDTextFragmentShader, Config.TitleFontLineHeight, Config.TitleFontKerning - 24);

			// Misc
			AssetLocator.UnitSphereShape = PhysicsManager.CreateSimpleSphereShape(1f, new CollisionShapeOptionsDesc(Vector3.ONE));
			AssetLocator.DefaultNormalMapView = AssetLocator.LoadTexture("Default_n.bmp").CreateView();
			AssetLocator.DefaultSpecularMapView = AssetLocator.LoadTexture("Default_s.bmp").CreateView();
			AssetLocator.DefaultEmissiveMapView = AssetLocator.LoadTexture("Default_e.bmp").CreateView();
		}

		private static void StartEngine() {
			Action<float> postStartAction = null;
			postStartAction = _ => {
				MenuCoordinator.Init();
				GameCoordinator.Init();
				MenuCoordinator.LoadMenuFrontend();
				EntityModule.PostTick -= postStartAction;
			};
			EntityModule.PostTick += postStartAction;
			LosgapSystem.Start();
		}

		private static void DisposeAll() {
			// Resources
			AssetLocator.UnitSphereShape.Dispose();
			AssetLocator.LizardEggPhysicsShape.Dispose();
			gameObjectCache.Dispose();
			AssetLocator.MainFont.Dispose();
			AssetLocator.TitleFont.Dispose();

			// Everything else
			shadowPass.Dispose();
			hudPass.DepthStencilState.Dispose();
			hudPass.Input.Dispose();
			hudPass.RasterizerState.Dispose();
			hudPass.BlendState.Dispose();
			hudPass.Dispose();
			AssetLocator.LightPass.Dispose();
			AssetLocator.MainGeometryPass.DepthStencilState.Dispose();
			AssetLocator.MainGeometryPass.RasterizerState.Dispose();
			AssetLocator.MainGeometryPass.BlendState.Dispose();
			AssetLocator.MainGeometryPass.Dispose();
			AssetLocator.MainCamera.Dispose();
			hudFSColorBuffer.Dispose();
			hudTextFSColorBuffer.Dispose();
			hudTextFSSampler.Dispose();
			hudFSSampler.Dispose();
			AssetLocator.HUDFragmentShader.Dispose();
			AssetLocator.HUDTextFragmentShader.Dispose();
			simpleVS.Dispose();
			AssetLocator.AlphaFragmentShader.Dispose();
			simpleFSSampler.Dispose();
			simpleFSMatPropsBuffer.Dispose();
			hudVS.Dispose();
			hudVPPropsBuffer.Dispose();
			skyDiffuseSampler.Dispose();
			AssetLocator.SkyFragmentShader.Dispose();
			skyVS.Dispose();
			gbufferNormalSampler.Dispose();
			outliningShader.Dispose();
			copyReverseShader.Dispose();
			copyShader.Dispose();
			blurShader.Dispose();
			dofShader.Dispose();
			dofLensPropsBuffer.Dispose();
			bloomHShader.Dispose();
			bloomVShader.Dispose();
			finalizationFS.Dispose();
			lightFS.Dispose();
			shadowFS.Dispose();
			shadowVS.Dispose();
			lightBufferView.Dispose();
			lightMetaBuffer.Dispose();
			lightBuffer.Dispose();
			lightVSScalarsBuffer.Dispose();
			lightFSCameraBuffer.Dispose();
			lightVS.Dispose();
			glowShader.Dispose();
			AssetLocator.GeometryFragmentShader.Dispose();
			shadowSampler.Dispose();
			defaultDiffuseSampler.Dispose();
			geomMatPropsBuffer.Dispose();
			AssetLocator.MainGeometryVertexShader.Dispose();
			geomVSCameraBuffer.Dispose();
			AssetLocator.HudLayer.Dispose();
			AssetLocator.SkyLayer.Dispose();
			AssetLocator.GameAlphaLayer.Dispose();
			AssetLocator.GameLayer.Dispose();
		}

		private static void ConfigRefresh() {
			RenderingModule.VSyncEnabled = Config.VSyncEnabled;
			RenderingModule.MaxFrameRateHz = Config.MaxFrameRateHz == 0L ? (long?) null : Config.MaxFrameRateHz;
			InputModule.InputStateUpdateRateHz = Config.InputStateUpdateRateHz == 0L ? (long?) null : Config.InputStateUpdateRateHz;
			EntityModule.TickRateHz = Config.TickRateHz == 0L ? (long?) null : Config.TickRateHz;
			AudioModule.AudioUpdateRateHz = Config.TickRateHz == 0L ? (long?) null : Config.TickRateHz;

			if (AssetLocator.MainWindow.FullscreenState != Config.DisplayWindowFullscreenState) {
				AssetLocator.MainWindow.FullscreenState = Config.DisplayWindowFullscreenState;
			}

			var width = Config.DisplayWindowWidth == 0U ? RenderingModule.SelectedOutputDisplay.HighestResolution.ResolutionWidth : Config.DisplayWindowWidth;
			var height = Config.DisplayWindowHeight == 0U ? RenderingModule.SelectedOutputDisplay.HighestResolution.ResolutionHeight : Config.DisplayWindowHeight;
			if (AssetLocator.MainWindow.Width != width || AssetLocator.MainWindow.Height != height) {
				AssetLocator.MainWindow.SetResolution(width, height);	
			}

			var oldDiffuseSampler = defaultDiffuseSampler;
			defaultDiffuseSampler = new TextureSampler(
				Config.DefaultSamplingFilterType,
				TextureWrapMode.Wrap,
				Config.DefaultSamplingAnisoLevel
			);
			AssetLocator.GeometryFragmentShader.GetBindingByIdentifier("DiffuseSampler").Bind(defaultDiffuseSampler);
			oldDiffuseSampler.Dispose();

			AssetLocator.LightPass.OutliningShader = Config.EnableOutlining ? outliningShader : outliningNoopShader;

			PhysicsManager.SetPhysicsTickrate(Config.GetTickrateForPhysicsLevel(Config.PhysicsLevel));
		}

		private static unsafe void PretriangulateLevels() {
			var allFiles = LevelDatabase.GetAllLevelFilesInOrder();
			bool shownPrompt = false;
			for (int i = 0; i < allFiles.Count; ++i) {
				var fullFilePath = Path.Combine(AssetLocator.LevelsDir, Path.GetFileNameWithoutExtension(allFiles[i].LevelFileName)) + ".ell";
				if (!File.Exists(fullFilePath)) continue;

				var pretriangulationFileFullPath = Path.Combine(AssetLocator.LevelsDir, Path.GetFileNameWithoutExtension(allFiles[i].LevelFileName)) + ".ptd";
				if (!File.Exists(pretriangulationFileFullPath)) {
					if (!shownPrompt) {
						var cancel = !PopupUtils.ShowConfirmationPopup(
							"Pretriangulation Data Missing",
							"Pretriangulation data for one or more levels is missing."
							+ Environment.NewLine + Environment.NewLine +
							"The application can now recalculate these values before starting. This may take several minutes."
							+ Environment.NewLine + Environment.NewLine +
							"Press OK to begin the process, or Cancel to skip it at this point (will result in longer loading times in-game)."
						);
						if (cancel) return;
						shownPrompt = true;
					}
				}
				else continue;

				Logger.Log("Pretriangulating '" + fullFilePath + "'; storing data in '" + pretriangulationFileFullPath + "'...");
				var loadedFile = LevelDescription.Load(fullFilePath, false);
				var precalculatedData = loadedFile.PrecalculateGeometryTriangulation();

				var fileStream = File.Open(pretriangulationFileFullPath, FileMode.Create, FileAccess.Write, FileShare.None);
				BinaryWriter bw = new BinaryWriter(fileStream);

				bw.Write(precalculatedData.Count);
				foreach (var kvp in precalculatedData) {
					bw.Write(kvp.Key);
					bw.Write(kvp.Value.Item1.Count);
					bw.Write(kvp.Value.Item2.Count);
					foreach (var vert in kvp.Value.Item1) {
						bw.Write(vert.Position.X);
						bw.Write(vert.Position.Y);
						bw.Write(vert.Position.Z);
						bw.Write(vert.Normal.X);
						bw.Write(vert.Normal.Y);
						bw.Write(vert.Normal.Z);
						bw.Write(vert.TexUV.X);
						bw.Write(vert.TexUV.Y);
						bw.Write(vert.Tangent.X);
						bw.Write(vert.Tangent.Y);
						bw.Write(vert.Tangent.Z);
					}
					foreach (var index in kvp.Value.Item2) bw.Write(index);
				}

				fileStream.Dispose();
				loadedFile.Dispose();

				GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
				GC.Collect(2, GCCollectionMode.Forced, true);
			}
			
		}

		private static void PrerenderMipmaps() {
			
		}

		private static void CreatePlayerballModels(GeometryCacheBuilder<DefaultVertex> geomCache) {
			AssetLocator.LizardEggModels = new ModelHandle[22];
			AssetLocator.LizardEggMaterials = new Material[22];

			int index = 0;

			CreatePlayerballSet("0_Standard", geomCache, index++);
			CreatePlayerballSet("0_Billiardball", geomCache, index++);
			CreatePlayerballSet("0_Brickball", geomCache, index++);
			CreatePlayerballSet("0_Cheeseball", geomCache, index++);
			CreatePlayerballSet("0_DiamondBall", geomCache, index++);

			CreatePlayerballSet("0_Lavaball", geomCache, index++);
			CreatePlayerballSet("0_Marble", geomCache, index++);
			CreatePlayerballSet("0_Melon", geomCache, index++);
			CreatePlayerballSet("0_PaintedWoodBall", geomCache, index++);
			CreatePlayerballSet("0_Scalesball", geomCache, index++);

			CreatePlayerballSet("0_Snowball", geomCache, index++);
			CreatePlayerballSet("0_Tennisball", geomCache, index++);
			CreatePlayerballSet("1_CardboardBall", geomCache, index++);
			CreatePlayerballSet("1_Couchball", geomCache, index++);
			CreatePlayerballSet("1_RopeBall", geomCache, index++);

			CreatePlayerballSet("2_EmissiveBall", geomCache, index++);
			CreatePlayerballSet("3_Pumpkin", geomCache, index++);
			CreatePlayerballSet("4_Ringball", geomCache, index++);
			CreatePlayerballSet("5_Soccerball1", geomCache, index++);
			CreatePlayerballSet("5_Soccerball2", geomCache, index++);

			CreatePlayerballSet("6_SpiralBall", geomCache, index++);
			CreatePlayerballSet("7_WireFrame", geomCache, index++);
		}

		private static void CreatePlayerballSet(string folderName, GeometryCacheBuilder<DefaultVertex> geomCache, int index) {
			var folder = Path.Combine(AssetLocator.MaterialsDir, "Playerballs\\", folderName + "\\");
			var ballModelStream = AssetLoader.LoadModel(Path.Combine(folder, "Playerball.obj"));
			var ballVerts = ballModelStream.GetVertices<DefaultVertex>(typeof(DefaultVertex).GetConstructor(new[] { typeof(Vector3), typeof(Vector3), typeof(Vector3), typeof(Vector2) }));

			AssetLocator.LizardEggModels[index] = geomCache.AddModel(
				"Game Object: Player ball '" + folderName + "'",
				ballVerts,
				ballModelStream.GetIndices()
			);

			AssetLocator.LizardEggMaterials[index] = new Material("Game Object Mat: Player Ball '" + folderName + "'", AssetLocator.GeometryFragmentShader);
			AssetLocator.LizardEggMaterials[index].SetMaterialResource(
				(ResourceViewBinding) AssetLocator.GeometryFragmentShader.GetBindingByIdentifier("DiffuseMap"),
				AssetLocator.LoadTexture(Path.Combine(folder, "PlayerTexture_0.png"), false).CreateView()
			);
			AssetLocator.LizardEggMaterials[index].SetMaterialResource(
				(ResourceViewBinding) AssetLocator.GeometryFragmentShader.GetBindingByIdentifier("NormalMap"),
				AssetLocator.LoadTexture(Path.Combine(folder, "PlayerTexture_n_0.png"), false).CreateView()
			);
			AssetLocator.LizardEggMaterials[index].SetMaterialResource(
				(ResourceViewBinding) AssetLocator.GeometryFragmentShader.GetBindingByIdentifier("SpecularMap"),
				AssetLocator.LoadTexture(Path.Combine(folder, "PlayerTexture_s_0.png"), false).CreateView()
			);
			AssetLocator.LizardEggMaterials[index].SetMaterialResource(
				(ResourceViewBinding) AssetLocator.GeometryFragmentShader.GetBindingByIdentifier("EmissiveMap"),
				AssetLocator.LoadTexture(Path.Combine(folder, "PlayerTexture_e_0.png"), false).CreateView()
			);
			AssetLocator.LizardEggMaterials[index].SetMaterialConstantValue(
				(ConstantBufferBinding) AssetLocator.GeometryFragmentShader.GetBindingByIdentifier("MaterialProperties"),
				new Vector4(0.0f, 0.0f, 0.0f, 0.875f)
			);
		}
	}
}