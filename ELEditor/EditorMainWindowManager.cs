// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 14 05 2015 at 13:59 by Ben Bowen

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Ophidian.Losgap;
using Ophidian.Losgap.AssetManagement;
using Ophidian.Losgap.Entities;
using Ophidian.Losgap.Input;
using Ophidian.Losgap.Rendering;

namespace Egodystonic.EscapeLizards.Editor {
	public static class EditorMainWindowManager {
		private const float MAX_CURSOR_MOVE_DISTANCE_FOR_PICKING_SQ = 2f * 2f;
		private const float TRANS_GRID_MIN_ROT_RADS = MathUtils.TWO_PI / 32f;
		private const float TRANS_GRID_MIN_TRANSLATION = PhysicsManager.ONE_METRE_SCALED * 0.05f;
		private const float TRANS_GRID_MIN_SCALE_DELTA = 0.05f;
		private static readonly Plane GROUND_PLANE = new Plane(Vector3.UP, Vector3.ZERO);
		private static readonly object staticMutationLock = new object();
		private static readonly Thread masterThread = new Thread(MasterThreadStart);
		private static bool lmbIsDepressed, rmbIsDepressed, windowLocked, ctrlIsDepressed, shiftIsDepressed, thisClickWasTransformAltering;
		private static float cumulativeTransformDelta = 0f;
		private static Vector2 lmbMoveDistance, lmbInitialClick, rmbMoveDistance, rmbInitialClick;
		private static GeometryCache gameObjectCache;

		public static bool EntityTranslationEnabled {
			get {
				lock (staticMutationLock) {
					return ctrlIsDepressed && !shiftIsDepressed;
				}
			}
		}

		public static bool EntityScalingEnabled {
			get {
				lock (staticMutationLock) {
					return ctrlIsDepressed && shiftIsDepressed;
				}
			}
		}

		public static bool EntityRotationEnabled {
			get {
				lock (staticMutationLock) {
					return !ctrlIsDepressed && shiftIsDepressed;
				}
			}
		}

		public static void Init() {
			lock (staticMutationLock) {
				masterThread.IsBackground = false;
				masterThread.Start();
				Monitor.Wait(staticMutationLock);
				InitInput();
			}
		}

		public static void Close() {
			if (!AssetLocator.MainWindow.IsClosed) AssetLocator.MainWindow.Close();
		}

		private static unsafe void MasterThreadStart() {
			string instDir = Environment.GetCommandLineArgs().Length > 1
				? Environment.GetCommandLineArgs()[1]
				: Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles, Environment.SpecialFolderOption.None), @"Egodystonic\Escape Lizards\Inst\");
			string dataDir = Environment.GetCommandLineArgs().Length > 2
				? Environment.GetCommandLineArgs()[2]
				: Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create), @"Egodystonic\Escape Lizards\Data");
			Monitor.Enter(staticMutationLock);
			LosgapSystem.ApplicationName = "SEL Editor";
			LosgapSystem.InstallationDirectory = new DirectoryInfo(instDir);
			LosgapSystem.MutableDataDirectory = new DirectoryInfo(dataDir);

			if (!LosgapSystem.IsModuleAdded(typeof(RenderingModule))) LosgapSystem.AddModule(typeof(RenderingModule));
			if (!LosgapSystem.IsModuleAdded(typeof(InputModule))) LosgapSystem.AddModule(typeof(InputModule));
			if (!LosgapSystem.IsModuleAdded(typeof(EntityModule))) LosgapSystem.AddModule(typeof(EntityModule));

			LosgapSystem.MaxThreadCount = 1U;
			RenderingModule.VSyncEnabled = true; // Just a nice default for an editor I think

			#region Create window + shaders
			AssetLocator.MainWindow = new Window("SEL Editor Main Window", (uint) (1920f * 0.75f), (uint) (1080f * 0.75f), WindowFullscreenState.NotFullscreen);
			AssetLocator.MainWindow.ClearViewports().ForEach(vp => vp.Dispose());
			AssetLocator.MainWindow.AddViewport(ViewportAnchoring.Centered, Vector2.ZERO, Vector2.ONE, Config.DisplayWindowNearPlane, Config.DisplayWindowFarPlane);
			AssetLocator.GameLayer = Scene.CreateLayer("Main Layer");
			AssetLocator.SkyLayer = Scene.CreateLayer("Sky Layer");
			AssetLocator.HudLayer = Scene.CreateLayer("HUD Layer");
			ConstantBuffer<GeomPassProjViewMatrices> vsCameraBuffer = BufferFactory.NewConstantBuffer<GeomPassProjViewMatrices>().WithUsage(ResourceUsage.DiscardWrite);
			AssetLocator.MainGeometryVertexShader = new VertexShader(
				Path.Combine(AssetLocator.ShadersDir, "DLGeometryVS.cso"),
				new VertexInputBinding(0U, "INSTANCE_TRANSFORM"),
				new ConstantBufferBinding(0U, "CameraTransform", vsCameraBuffer),
				new VertexInputBinding(1U, "TEXCOORD"),
				new VertexInputBinding(2U, "POSITION"),
				new VertexInputBinding(3U, "NORMAL"),
				new VertexInputBinding(4U, "TANGENT")
			);

			AssetLocator.SkyVertexShader = new VertexShader(
				Path.Combine(AssetLocator.ShadersDir, "DLGeometryVSSky.cso"),
				new VertexInputBinding(0U, "INSTANCE_TRANSFORM"),
				new ConstantBufferBinding(0U, "CameraTransform", vsCameraBuffer),
				new VertexInputBinding(1U, "TEXCOORD"),
				new VertexInputBinding(2U, "POSITION"),
				new VertexInputBinding(3U, "NORMAL"),
				new VertexInputBinding(4U, "TANGENT")
			);

			ConstantBuffer<Vector4> matPropsBuffer = BufferFactory.NewConstantBuffer<Vector4>().WithUsage(ResourceUsage.DiscardWrite);
			TextureSampler defaultDiffuseSampler = new TextureSampler(
				TextureFilterType.Anisotropic,
				TextureWrapMode.Wrap,
				AnisotropicFilteringLevel.FourTimes
			);
			TextureSampler shadowSampler = new TextureSampler(
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
				new ConstantBufferBinding(0U, "MaterialProperties", matPropsBuffer)
			);
			AssetLocator.GeometryFragmentShader.GetBindingByIdentifier("DiffuseSampler").Bind(defaultDiffuseSampler);
			AssetLocator.GeometryFragmentShader.GetBindingByIdentifier("ShadowSampler").Bind(shadowSampler);

			AssetLocator.SkyGeometryFragmentShader = new FragmentShader(
				Path.Combine(AssetLocator.ShadersDir, "DLGeometryFSSky.cso"),
				new ResourceViewBinding(0U, "DiffuseMap"),
				new TextureSamplerBinding(0U, "DiffuseSampler"),
				new ConstantBufferBinding(0U, "MaterialProperties", matPropsBuffer)
			);
			AssetLocator.SkyGeometryFragmentShader.GetBindingByIdentifier("DiffuseSampler").Bind(defaultDiffuseSampler);

			ConstantBuffer<Vector4> lightVSScalarsBuffer = BufferFactory.NewConstantBuffer<Vector4>().WithUsage(ResourceUsage.DiscardWrite);
			VertexShader lightVS = new VertexShader(
				Path.Combine(AssetLocator.ShadersDir, "DLLightingVS.cso"),
				new VertexInputBinding(0U, "POSITION"),
				new VertexInputBinding(1U, "TEXCOORD"),
				new ConstantBufferBinding(0U, "Scalars", lightVSScalarsBuffer)
			);

			ConstantBuffer<Vector4> fsCameraBuffer = BufferFactory.NewConstantBuffer<Vector4>().WithUsage(ResourceUsage.DiscardWrite);
			ConstantBuffer<Vector4> lightMetaBuffer = BufferFactory.NewConstantBuffer<Vector4>().WithUsage(ResourceUsage.DiscardWrite);
			FragmentShader lightFS = new FragmentShader(
				Path.Combine(AssetLocator.ShadersDir, "DLLightingFS.cso"),
				new ResourceViewBinding(0U, "NormalGB"),
				new ResourceViewBinding(1U, "DiffuseGB"),
				new ResourceViewBinding(2U, "SpecularGB"),
				new ResourceViewBinding(3U, "PositionGB"),
				new ResourceViewBinding(5U, "EmissiveGB"),
				new ConstantBufferBinding(0U, "CameraProperties", fsCameraBuffer),
				new ConstantBufferBinding(1U, "LightMeta", lightMetaBuffer),
				new ResourceViewBinding(4U, "LightBuffer"),
				new TextureSamplerBinding(0U, "NormalSampler")
			);
			FragmentShader dlFinalizationFS = new FragmentShader(
				Path.Combine(AssetLocator.ShadersDir, "DLFinalFS.cso"),
				new ResourceViewBinding(1U, "DiffuseGB")
			);
			TextureSampler normalSampler = new TextureSampler(
				TextureFilterType.Anisotropic,
				TextureWrapMode.Wrap,
				AnisotropicFilteringLevel.FourTimes
			);
			Buffer<LightProperties> lightBuffer = BufferFactory.NewBuffer<LightProperties>()
				.WithLength(DLLightPass.MAX_DYNAMIC_LIGHTS)
				.WithUsage(ResourceUsage.DiscardWrite)
				.WithPermittedBindings(GPUBindings.ReadableShaderResource);
			ShaderBufferResourceView lightBufferView = lightBuffer.CreateView();
			((TextureSamplerBinding) lightFS.GetBindingByIdentifier("NormalSampler")).Bind(normalSampler);
			((ResourceViewBinding) lightFS.GetBindingByIdentifier("LightBuffer")).Bind(lightBufferView);

			FragmentShader outliningShader = new FragmentShader(
				Path.Combine(AssetLocator.ShadersDir, "Outliner.cso"),
				new ResourceViewBinding(0U, "NormalGB"),
				new ResourceViewBinding(1U, "GeomDepthBuffer")
			);

			FragmentShader bloomHShader = new FragmentShader(
				Path.Combine(AssetLocator.ShadersDir, "BloomH.cso"),
				new ResourceViewBinding(0U, "PreBloomImage")
			);
			FragmentShader bloomVShader = new FragmentShader(
				Path.Combine(AssetLocator.ShadersDir, "BloomV.cso"),
				new ResourceViewBinding(0U, "PreBloomImage")
			);

			FragmentShader glowShader = new FragmentShader(
				Path.Combine(AssetLocator.ShadersDir, "Glow.cso"),
				new ResourceViewBinding(0U, "GlowSrc")
			);

			FragmentShader copyShader = new FragmentShader(
				Path.Combine(AssetLocator.ShadersDir, "Copy.cso"),
				new ResourceViewBinding(0U, "SourceTex"),
				new TextureSamplerBinding(0U, "SourceSampler")
			);

			FragmentShader copyReverseShader = new FragmentShader(
				Path.Combine(AssetLocator.ShadersDir, "CopyReverse.cso"),
				new ResourceViewBinding(0U, "SourceTex"),
				new TextureSamplerBinding(0U, "SourceSampler")
			);

			FragmentShader blurShader = new FragmentShader(
				Path.Combine(AssetLocator.ShadersDir, "Blur.cso"),
				new ResourceViewBinding(0U, "UnblurredScene")
			);

			ConstantBuffer<Vector4> dofLensPropsBuffer = BufferFactory.NewConstantBuffer<Vector4>()
				.WithUsage(ResourceUsage.DiscardWrite)
				.WithInitialData(new Vector4(
					Config.DisplayWindowNearPlane,
					Config.DisplayWindowFarPlane,
					1000f,
					12000f
				));
			FragmentShader dofShader = new FragmentShader(
				Path.Combine(AssetLocator.ShadersDir, "DoF.cso"),
				new ResourceViewBinding(0U, "UnblurredScene"),
				new ResourceViewBinding(1U, "BlurredScene"),
				new ResourceViewBinding(2U, "SceneDepth"),
				new TextureSamplerBinding(0U, "SourceSampler"),
				new ConstantBufferBinding(0U, "LensProperties", dofLensPropsBuffer)
			);

			VertexShader shadowVS = new VertexShader(
				Path.Combine(AssetLocator.ShadersDir, "ShadowVS.cso"),
				new VertexInputBinding(0U, "INSTANCE_TRANSFORM"),
				new ConstantBufferBinding(0U, "CameraTransform", vsCameraBuffer),
				new VertexInputBinding(1U, "POSITION")
			);

			FragmentShader shadowFS = new FragmentShader(
				Path.Combine(AssetLocator.ShadersDir, "ShadowFS.cso")
			);

			VertexShader unlitVSSky = new VertexShader(
				Path.Combine(AssetLocator.ShadersDir, "UnlitVSSky.cso"),
				new VertexInputBinding(0U, "INSTANCE_TRANSFORM"),
				new ConstantBufferBinding(0U, "CameraTransform", vsCameraBuffer),
				new VertexInputBinding(1U, "TEXCOORD"),
				new VertexInputBinding(2U, "POSITION")
			);

			AssetLocator.SkyFragmentShader = new FragmentShader(
				Path.Combine(AssetLocator.ShadersDir, "UnlitFS.cso"),
				new TextureSamplerBinding(0U, "DiffuseSampler"),
				new ResourceViewBinding(0U, "DiffuseMap")
			);
			TextureSampler skyDiffuseSampler = new TextureSampler(
				TextureFilterType.Anisotropic,
				TextureWrapMode.Wrap,
				AnisotropicFilteringLevel.FourTimes
			);
			((TextureSamplerBinding) AssetLocator.SkyFragmentShader.GetBindingByIdentifier("DiffuseSampler")).Bind(skyDiffuseSampler);

			ConstantBuffer<Vector4> viewportPropertiesBuffer = BufferFactory.NewConstantBuffer<Vector4>().WithUsage(ResourceUsage.DiscardWrite);
			VertexShader hudVS = new VertexShader(
				Path.Combine(AssetLocator.ShadersDir, "HUDVS.cso"),
				new VertexInputBinding(0U, "INSTANCE_TRANSFORM"),
				new ConstantBufferBinding(1U, "CameraTransform", vsCameraBuffer),
				new ConstantBufferBinding(0U, "ViewportProperties", viewportPropertiesBuffer),
				new VertexInputBinding(1U, "TEXCOORD"),
				new VertexInputBinding(2U, "POSITION")
			);

			ConstantBuffer<Vector4> hudFSColorBuffer = BufferFactory.NewConstantBuffer<Vector4>().WithUsage(ResourceUsage.DiscardWrite);
			AssetLocator.HUDFragmentShader = new FragmentShader(
				Path.Combine(AssetLocator.ShadersDir, "HUDFS.cso"),
				new ResourceViewBinding(0U, "DiffuseMap"),
				new TextureSamplerBinding(0U, "DiffuseSampler"),
				new ConstantBufferBinding(0U, "TextureProperties", hudFSColorBuffer)
			);
			TextureSampler hudFSSampler = new TextureSampler(
				TextureFilterType.None,
				TextureWrapMode.Border,
				AnisotropicFilteringLevel.None,
				new TexelFormat.RGB32Float() {
					R = 1f,
					G = 0f,
					B = 0f
				}
			);
			((TextureSamplerBinding) AssetLocator.HUDFragmentShader.GetBindingByIdentifier("DiffuseSampler")).Bind(hudFSSampler);

			ConstantBuffer<Vector4> hudFSTextColorBuffer = BufferFactory.NewConstantBuffer<Vector4>().WithUsage(ResourceUsage.DiscardWrite);
			AssetLocator.HUDTextFragmentShader = new FragmentShader(
				Path.Combine(AssetLocator.ShadersDir, "HUDFSText.cso"),
				new ResourceViewBinding(0U, "DiffuseMap"),
				new TextureSamplerBinding(0U, "DiffuseSampler"),
				new ConstantBufferBinding(0U, "TextProperties", hudFSTextColorBuffer)
			);
			TextureSampler hudTextSampler = new TextureSampler(
				TextureFilterType.None,
				TextureWrapMode.Border,
				AnisotropicFilteringLevel.None,
				new TexelFormat.RGB32Float() { R = 1f, G = 0f, B = 0f }
			);
			((TextureSamplerBinding) AssetLocator.HUDTextFragmentShader.GetBindingByIdentifier("DiffuseSampler")).Bind(hudTextSampler);

			var simpleFSSampler = new TextureSampler(
				Config.DefaultSamplingFilterType,
				TextureWrapMode.Wrap,
				Config.DefaultSamplingAnisoLevel
			);
			var simpleFSMatPropsBuffer = BufferFactory.NewConstantBuffer<Vector4>().WithUsage(ResourceUsage.DiscardWrite).Create();
			AssetLocator.AlphaFragmentShader = new FragmentShader(
				Path.Combine(AssetLocator.ShadersDir, "SimpleFS.cso"),
				new TextureSamplerBinding(0U, "DiffuseSampler"),
				new ResourceViewBinding(0U, "DiffuseMap"),
				new ConstantBufferBinding(0U, "MaterialProperties", simpleFSMatPropsBuffer)
			);
			((TextureSamplerBinding) AssetLocator.AlphaFragmentShader.GetBindingByIdentifier("DiffuseSampler")).Bind(simpleFSSampler);
			#endregion

			#region Create passes
			var shadowPass = new ShadowPass("Shadow Pass");
			shadowPass.AddLayer(AssetLocator.GameLayer);
			shadowPass.ShadowVS = shadowVS;
			shadowPass.ShadowFS = shadowFS;
			shadowPass.Output = AssetLocator.MainWindow;

			AssetLocator.MainGeometryPass = new DLGeometryPass("Geometry Pass");
			AssetLocator.MainGeometryPass.ClearOutputBeforePass = false;
			AssetLocator.MainGeometryPass.AddLayer(AssetLocator.GameLayer);
			AssetLocator.MainGeometryPass.AddLayer(AssetLocator.SkyLayer);
			AssetLocator.MainGeometryPass.DepthStencilState = new DepthStencilState();
			AssetLocator.MainGeometryPass.Output = AssetLocator.MainWindow;
			AssetLocator.MainGeometryPass.BlendState = new BlendState(BlendOperation.None);
			AssetLocator.MainGeometryPass.RasterizerState = new RasterizerState(false, TriangleCullMode.BackfaceCulling, false);
			AssetLocator.MainGeometryPass.ShadowPass = shadowPass;
			AssetLocator.MainGeometryPass.GeomFSWithShadowSupport = AssetLocator.GeometryFragmentShader;

			AssetLocator.LightPass = new DLLightPass("Light Pass");
			AssetLocator.LightPass.DLLightVertexShader = lightVS;
			AssetLocator.LightPass.DLLightFragmentShader = lightFS;
			AssetLocator.LightPass.DLLightFinalizationShader = dlFinalizationFS;
			AssetLocator.LightPass.BloomHShader = bloomHShader;
			AssetLocator.LightPass.BloomVShader = bloomVShader;
			AssetLocator.LightPass.CopyReverseShader = copyReverseShader;
			AssetLocator.LightPass.CopyShader = copyShader;
			AssetLocator.LightPass.OutliningShader = outliningShader;
			AssetLocator.LightPass.BlurShader = blurShader;
			AssetLocator.LightPass.DoFShader = dofShader;
			AssetLocator.LightPass.GeometryPass = AssetLocator.MainGeometryPass;
			AssetLocator.LightPass.PresentAfterPass = false;
			AssetLocator.LightPass.SetLensProperties(0f, float.MaxValue);

			HUDPass hudPass = new HUDPass("HUD") {
				DepthStencilState = new DepthStencilState(false),
				Input = new Camera(),
				Output = AssetLocator.MainWindow,
				PresentAfterPass = true,
				RasterizerState = new RasterizerState(false, TriangleCullMode.NoCulling, false),
				VertexShader = hudVS,
				BlendState = new BlendState(BlendOperation.Additive),
				GlowShader = glowShader,
				GlowVS = lightVS,
				ScaleUpShader = copyReverseShader,
				ScaleDownShader = copyShader
			};
			hudPass.AddLayer(AssetLocator.HudLayer);
			ConstantBufferBinding vpPropsBinding = (ConstantBufferBinding) hudVS.GetBindingByIdentifier("ViewportProperties");

			hudPass.PrePass += pass => {
				Vector4 vpSizePx = ((SceneViewport) AssetLocator.MainWindow).SizePixels / 2f;
				vpPropsBinding.SetValue((byte*) (&vpSizePx));
			};

			AssetLocator.MainCamera = AssetLocator.MainGeometryPass.Input = new IlluminatingCamera(GameplayConstants.EGG_ILLUMINATION_RADIUS, Vector3.ONE);
			AssetLocator.ShadowcasterCamera = shadowPass.LightCam = new Camera();
			AssetLocator.ShadowcasterCamera.OrthographicDimensions = new Vector3(1000f, 1000f, PhysicsManager.ONE_METRE_SCALED * 100f);

			RenderingModule.AddRenderPass(shadowPass);
			RenderingModule.AddRenderPass(AssetLocator.MainGeometryPass);
			RenderingModule.AddRenderPass(AssetLocator.LightPass);
			RenderingModule.AddRenderPass(hudPass);
			#endregion

			#region HUD
			AssetLocator.MainFont = Font.Load(Path.Combine(AssetLocator.FontsDir, "mainFont.fnt"), AssetLocator.HUDTextFragmentShader, null, null);
			AssetLocator.TitleFont = Font.Load(Path.Combine(AssetLocator.FontsDir, "titleFont.fnt"), AssetLocator.HUDTextFragmentShader, null, null);
			#endregion

			#region Game Objects
			GeometryCacheBuilder<DefaultVertex> gameObjectGCB = new GeometryCacheBuilder<DefaultVertex>();
			FragmentShader fs = AssetLocator.GeometryFragmentShader;

			// Load model streams
			var startFlagModelStream = AssetLoader.LoadModel(Path.Combine(AssetLocator.ModelsDir, "Spawn.obj"));
			var dynamicLightModelStream = AssetLoader.LoadModel(Path.Combine(AssetLocator.ModelsDir, "Lightbulb.obj"));
			var introCameraModelStream = AssetLoader.LoadModel(Path.Combine(AssetLocator.ModelsDir, "camera.obj"));
			var finishingBellModelStream = AssetLoader.LoadModel(Path.Combine(AssetLocator.ModelsDir, "FinishBellBottom.obj"));
			var vultureEggModelStream = AssetLoader.LoadModel(Path.Combine(AssetLocator.ModelsDir, "Playerball.obj"));
			var shadowcasterModelStream = AssetLoader.LoadModel(Path.Combine(AssetLocator.ModelsDir, "ScaleArrow.obj"));
			var coinModelStream = AssetLoader.LoadModel(Path.Combine(AssetLocator.ModelsDir, "LizardCoin.obj"));

			var finishingBellVerts = finishingBellModelStream.GetVertices<DefaultVertex>(typeof(DefaultVertex).GetConstructor(new[] { typeof(Vector3), typeof(Vector3), typeof(Vector3), typeof(Vector2) })).ToList();
			var finishingBellIndices = finishingBellModelStream.GetIndices().ToList();
			var vultureEggVerts = vultureEggModelStream.GetVertices<DefaultVertex>(typeof(DefaultVertex).GetConstructor(new[] { typeof(Vector3), typeof(Vector3), typeof(Vector3), typeof(Vector2) })).ToList();
			var vultureEggIndices = vultureEggModelStream.GetIndices().ToList();
			var coinModelVertices = coinModelStream.GetVertices<DefaultVertex>(typeof(DefaultVertex).GetConstructor(new[] { typeof(Vector3), typeof(Vector3), typeof(Vector3), typeof(Vector2) }));

			// Load models
			AssetLocator.StartFlagModel = gameObjectGCB.AddModel(
				"Game Object: Start Flag",
				startFlagModelStream.GetVertices<DefaultVertex>(typeof(DefaultVertex).GetConstructor(new[] { typeof(Vector3), typeof(Vector3), typeof(Vector3), typeof(Vector2) })).ToList(),
				startFlagModelStream.GetIndices().ToList()
			);
			AssetLocator.DynamicLightModel = gameObjectGCB.AddModel(
				"Game Object: Dynamic Light",
				dynamicLightModelStream.GetVertices<DefaultVertex>(typeof(DefaultVertex).GetConstructor(new[] { typeof(Vector3), typeof(Vector3), typeof(Vector3), typeof(Vector2) })).ToList(),
				dynamicLightModelStream.GetIndices().ToList()
			);
			AssetLocator.IntroCameraModel = gameObjectGCB.AddModel(
				"Game Object: Intro Camera Attracter",
				introCameraModelStream.GetVertices<DefaultVertex>(typeof(DefaultVertex).GetConstructor(new[] { typeof(Vector3), typeof(Vector3), typeof(Vector3), typeof(Vector2) })).ToList(),
				introCameraModelStream.GetIndices().ToList()
			);
			AssetLocator.FinishingBellModel = gameObjectGCB.AddModel(
				"Game Object: Finishing Bell",
				finishingBellVerts,
				finishingBellIndices
			);
			AssetLocator.VultureEggModel = gameObjectGCB.AddModel(
				"Game Object: Vulture Egg",
				vultureEggVerts,
				vultureEggIndices
			);
			AssetLocator.ShadowcasterModel = gameObjectGCB.AddModel(
				"Game Object: Shadowcaster",
				shadowcasterModelStream.GetVerticesWithoutTangents<DefaultVertex>(typeof(DefaultVertex).GetConstructor(new[] { typeof(Vector3), typeof(Vector3), typeof(Vector2) })).ToList(),
				shadowcasterModelStream.GetIndices().ToList()
			);
			AssetLocator.LizardCoinModel = gameObjectGCB.AddModel(
				"Game Object: Lizard Coin",
				coinModelVertices,
				coinModelStream.GetIndices()
			);

			// Set materials
			AssetLocator.StartFlagMaterial = new Material("Game Object Mat: Start Flag", fs);
			AssetLocator.StartFlagMaterial.SetMaterialResource(
				(ResourceViewBinding) fs.GetBindingByIdentifier("DiffuseMap"),
				AssetLocator.LoadTexture("Spawn_bake.jpg").CreateView()
			);
			AssetLocator.StartFlagMaterial.SetMaterialConstantValue(
				(ConstantBufferBinding) fs.GetBindingByIdentifier("MaterialProperties"),
				new Vector4(0f, 0f, 0f, 1f)
			);

			AssetLocator.DynamicLightMaterial = new Material("Game Object Mat: Dynamic Light", fs);
			AssetLocator.DynamicLightMaterial.SetMaterialResource(
				(ResourceViewBinding) fs.GetBindingByIdentifier("DiffuseMap"),
				AssetLocator.LoadTexture("Bulb_bake.jpg").CreateView()
			);
			AssetLocator.DynamicLightMaterial.SetMaterialConstantValue(
				(ConstantBufferBinding) fs.GetBindingByIdentifier("MaterialProperties"),
				new Vector4(0f, 0f, 0f, 1f)
			);

			AssetLocator.IntroCameraMaterial = new Material("Game Object Mat: Intro Camera", fs);
			AssetLocator.IntroCameraMaterial.SetMaterialResource(
				(ResourceViewBinding) fs.GetBindingByIdentifier("DiffuseMap"),
				AssetLocator.LoadTexture("Camera_colo.png").CreateView()
			);
			AssetLocator.IntroCameraMaterial.SetMaterialConstantValue(
				(ConstantBufferBinding) fs.GetBindingByIdentifier("MaterialProperties"),
				new Vector4(0f, 0f, 0f, 1f)
			);

			AssetLocator.FinishingBellMaterial = new Material("Game Object Mat: Finishing Bell", fs);
			AssetLocator.FinishingBellMaterial.SetMaterialResource(
				(ResourceViewBinding) fs.GetBindingByIdentifier("DiffuseMap"),
				AssetLocator.LoadTexture("FinishBellBottom_bake.png").CreateView()
			);
			AssetLocator.FinishingBellMaterial.SetMaterialConstantValue(
				(ConstantBufferBinding) fs.GetBindingByIdentifier("MaterialProperties"),
				new Vector4(1f, 1f, 0.8f, 1f)
			);

			AssetLocator.VultureEggMaterial = new Material("Game Object Mat: Vulture Egg", fs);
			AssetLocator.VultureEggMaterial.SetMaterialResource(
				(ResourceViewBinding) fs.GetBindingByIdentifier("DiffuseMap"),
				AssetLocator.LoadTexture("VEggTexture.png").CreateView()
			);
			AssetLocator.VultureEggMaterial.SetMaterialConstantValue(
				(ConstantBufferBinding) fs.GetBindingByIdentifier("MaterialProperties"),
				new Vector4(1f, 1f, 0.8f, 1f)
			);

			AssetLocator.ShadowcasterMaterial = new Material("Game Object Mat: Shadowcaster", fs);
			AssetLocator.ShadowcasterMaterial.SetMaterialResource(
				(ResourceViewBinding) fs.GetBindingByIdentifier("DiffuseMap"),
				AssetLocator.LoadTexture("Default_s.bmp").CreateView()
			);
			AssetLocator.ShadowcasterMaterial.SetMaterialConstantValue(
				(ConstantBufferBinding) fs.GetBindingByIdentifier("MaterialProperties"),
				new Vector4(0f, 0f, 0f, 1f)
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

			AssetLocator.FinishingBellPhysicsShape = PhysicsManager.CreateConcaveHullShape(
				finishingBellVerts.Select(df => df.Position), 
				finishingBellIndices.Select(i => (int) i), 
				new CollisionShapeOptionsDesc(),
				AssetLocator.CreateACDFilePath("finishingBell")
			);
			AssetLocator.VultureEggPhysicsShape = PhysicsManager.CreateConcaveHullShape(
				vultureEggVerts.Select(df => df.Position), 
				vultureEggIndices.Select(i => (int) i),
				new CollisionShapeOptionsDesc(),
				AssetLocator.CreateACDFilePath("vultureEgg")
			);
			AssetLocator.LizardCoinShape = PhysicsManager.CreateConvexHullShape(
				coinModelVertices.Select(v => v.Position),
				new CollisionShapeOptionsDesc(Vector3.ONE)
			);

			// Build
			gameObjectCache = gameObjectGCB.Build();
			AssetLocator.MainGeometryPass.SetVSForCache(gameObjectCache, AssetLocator.MainGeometryVertexShader);
			#endregion

			#region Misc
			AssetLocator.UnitSphereShape = PhysicsManager.CreateSimpleSphereShape(1f, new CollisionShapeOptionsDesc(Vector3.ONE));
			AssetLocator.DefaultNormalMapView = AssetLocator.LoadTexture("Default_n.bmp").CreateView();
			AssetLocator.DefaultSpecularMapView = AssetLocator.LoadTexture("Default_s.bmp").CreateView();
			AssetLocator.DefaultEmissiveMapView = AssetLocator.LoadTexture("Default_e.bmp").CreateView();
			#endregion

			AssetLocator.MainWindow.WindowClosed += w => LosgapSystem.Exit();
			Monitor.Pulse(staticMutationLock);
			Monitor.Exit(staticMutationLock);
			LosgapSystem.Start();

			#region Disposal
			AssetLocator.UnitSphereShape.Dispose();
			gameObjectCache.Dispose();
			AssetLocator.IntroCameraMaterial.Dispose();
			AssetLocator.DynamicLightMaterial.Dispose();
			AssetLocator.StartFlagMaterial.Dispose();
			AssetLocator.MainFont.Dispose();
			hudPass.DepthStencilState.Dispose();
			hudPass.Input.Dispose();
			hudPass.RasterizerState.Dispose();
			hudPass.BlendState.Dispose();
			hudPass.Dispose();
			AssetLocator.LightPass.Dispose();
			AssetLocator.MainGeometryPass.DepthStencilState.Dispose();
			AssetLocator.MainGeometryPass.RasterizerState.Dispose();
			AssetLocator.MainGeometryPass.Dispose();
			AssetLocator.MainCamera.Dispose();
			hudTextSampler.Dispose();
			AssetLocator.HUDFragmentShader.Dispose();
			hudVS.Dispose();
			shadowVS.Dispose();
			shadowFS.Dispose();
			viewportPropertiesBuffer.Dispose();
			skyDiffuseSampler.Dispose();
			AssetLocator.SkyFragmentShader.Dispose();
			unlitVSSky.Dispose();
			normalSampler.Dispose();
			dofShader.Dispose();
			blurShader.Dispose();
			copyReverseShader.Dispose();
			copyShader.Dispose();
			glowShader.Dispose();
			bloomHShader.Dispose();
			bloomVShader.Dispose();
			outliningShader.Dispose();
			dlFinalizationFS.Dispose();
			lightFS.Dispose();
			lightBufferView.Dispose();
			lightBuffer.Dispose();
			lightMetaBuffer.Dispose();
			fsCameraBuffer.Dispose();
			lightVS.Dispose();
			AssetLocator.GeometryFragmentShader.Dispose();
			shadowSampler.Dispose();
			defaultDiffuseSampler.Dispose();
			matPropsBuffer.Dispose();
			AssetLocator.MainGeometryVertexShader.Dispose();
			AssetLocator.SkyVertexShader.Dispose();
			vsCameraBuffer.Dispose();
			AssetLocator.HudLayer.Dispose();
			AssetLocator.SkyLayer.Dispose();
			AssetLocator.GameLayer.Dispose();
			GC.KeepAlive(AssetLocator.MainWindow);
			#endregion
		}

		private static void InitInput() {
			InputBindingRegistry ipb = AssetLocator.GameLayer.GetResource<InputBindingRegistry>();
			ipb.RegisterKeyEvent(
				new HashSet<VirtualKey>() { VirtualKey.ControlKey, VirtualKey.ShiftKey }, 
				VirtualKeyState.KeyDown | VirtualKeyState.KeyUp,
				(key, state) => {
					lock (staticMutationLock) {
						switch (key) {
							case VirtualKey.ControlKey: ctrlIsDepressed = state == VirtualKeyState.KeyDown; break;
							case VirtualKey.ShiftKey: shiftIsDepressed = state == VirtualKeyState.KeyDown; break;
						}
					}
					EditorToolbar.Instance.BeginInvoke(new Action(EditorToolbar.Instance.UpdateSelectedEntities));
					EditorToolbar.StartNewReversionStep();
				}
			);

			ipb.RegisterKeyEvent(
				new HashSet<VirtualKey> { VirtualKey.W, VirtualKey.Z, VirtualKey.B }, 
				VirtualKeyState.KeyDown,
				(key, _) => {
					lock (staticMutationLock) {
						if (!ctrlIsDepressed) return;
					}
					switch (key) {
						case VirtualKey.W:
							EditorToolbar.CloneSelectedEntities();
							break;
						case VirtualKey.Z:
							EditorToolbar.UndoLast();
							break;
						case VirtualKey.B:
							EditorToolbar.BakeAll();
							break;
					}
				}
			);

			ipb.RegisterKeyEvent(
				new HashSet<VirtualKey>() { VirtualKey.LButton, VirtualKey.RButton }, 
				VirtualKeyState.KeyDown | VirtualKeyState.KeyUp,
				(key, state) => {
					lock (staticMutationLock) {
						Window mainWindow = AssetLocator.MainWindow;
						switch (state) {
							case VirtualKeyState.KeyDown:
								switch (key) {
									case VirtualKey.LButton:
										lmbIsDepressed = true;
										lmbMoveDistance = Vector2.ZERO;
										lmbInitialClick = InputModule.CursorPosition;
										break;
									case VirtualKey.RButton:
										rmbIsDepressed = true;
										rmbMoveDistance = Vector2.ZERO;
										rmbInitialClick = InputModule.CursorPosition;
										break;
								}
								if (InputModule.CursorIsWithinWindow(mainWindow)) {
									InputModule.LockCursorToWindow(mainWindow);
									mainWindow.ShowCursor = false;
									windowLocked = true;
								}
								break;
							case VirtualKeyState.KeyUp:
								cumulativeTransformDelta = 0f;
								if (windowLocked && (!lmbIsDepressed || key == VirtualKey.LButton) && (!rmbIsDepressed || key == VirtualKey.RButton)) {
									InputModule.UnlockCursorFromWindow(mainWindow);
									mainWindow.ShowCursor = true;
								}
								switch (key) {
									case VirtualKey.LButton:
										lmbIsDepressed = false;
										if (InputModule.CursorIsWithinWindow(mainWindow) && !thisClickWasTransformAltering) {
											float lmbDistanceSq = lmbMoveDistance.LengthSquared;
											if (lmbDistanceSq <= MAX_CURSOR_MOVE_DISTANCE_FOR_PICKING_SQ) {
												Vector2 curPosRelative = lmbInitialClick - mainWindow.Position;
												curPosRelative = new Vector2(
													curPosRelative.X - mainWindow.Width * 0.5f,
													curPosRelative.Y - mainWindow.Height * 0.5f
												);
												InputModule.CursorPosition = lmbInitialClick + lmbMoveDistance;

												// Disgusting anti-deadlock hax now, sorry
												Window mainWindowLocal = mainWindow;
												Monitor.Exit(staticMutationLock);
												try {
													EditorToolbar.SelectEntityViaRay(AssetLocator.MainCamera.PixelRayCast(mainWindowLocal, curPosRelative));
												}
												finally {
													Monitor.Enter(staticMutationLock);	
												}
											}
										}
										thisClickWasTransformAltering = false;
										break;
									case VirtualKey.RButton:
										rmbIsDepressed = false;
										if (InputModule.CursorIsWithinWindow(mainWindow) && !thisClickWasTransformAltering) {
											float rmbDistanceSq = rmbMoveDistance.LengthSquared;
											if (rmbDistanceSq <= MAX_CURSOR_MOVE_DISTANCE_FOR_PICKING_SQ) {
												Vector2 curPosRelative = rmbInitialClick - mainWindow.Position;
												curPosRelative = new Vector2(
													curPosRelative.X - mainWindow.Width * 0.5f,
													curPosRelative.Y - mainWindow.Height * 0.5f
												);
												InputModule.CursorPosition = rmbInitialClick + rmbMoveDistance;
												// Disgusting anti-deadlock hax #2
												Window mainWindowLocal = mainWindow;
												Monitor.Exit(staticMutationLock);
												try {
													EditorToolbar.AddGameObject(AssetLocator.MainCamera.PixelRayCast(mainWindowLocal, curPosRelative));
												}
												finally {
													Monitor.Enter(staticMutationLock);
												}
											}
										}
										break;
								}
								break;
						}
					}
					EditorToolbar.StartNewReversionStep();
				}
			);

			ipb.RegisterMouseMoveEvent((_, curDelta) => {
				bool lockTranslationsToGrid = EditorToolbar.LockTranslationsToGrid;
				float maxDelta = curDelta.X;
				if (Math.Abs(curDelta.Y) > Math.Abs(curDelta.X)) maxDelta = curDelta.Y;
				lock (staticMutationLock) {
					if (!InputModule.CursorIsWithinWindow(AssetLocator.MainWindow)) return;
					if (shiftIsDepressed && ctrlIsDepressed) {
						float delta = (maxDelta * -0.0025f);
						if (lockTranslationsToGrid) {
							cumulativeTransformDelta += delta;
							if (Math.Abs(cumulativeTransformDelta) > TRANS_GRID_MIN_SCALE_DELTA) {
								float remainder = (cumulativeTransformDelta % TRANS_GRID_MIN_SCALE_DELTA);
								delta = cumulativeTransformDelta - remainder;
								cumulativeTransformDelta = remainder;
							}
							else return;
						}
						ScaleEntities(delta);
					}
					else if (shiftIsDepressed) {
						float rotRads = maxDelta / -350f;
						if (lockTranslationsToGrid) {
							cumulativeTransformDelta += rotRads;
							if (Math.Abs(cumulativeTransformDelta) > TRANS_GRID_MIN_ROT_RADS) {
								float remainder = (cumulativeTransformDelta % TRANS_GRID_MIN_ROT_RADS);
								rotRads = cumulativeTransformDelta - remainder;
								cumulativeTransformDelta = remainder;
							}
							else return;
						}
						RotateEntities(rotRads);
					}
					else if (ctrlIsDepressed) {
						float distance = maxDelta * -0.25f;
						if (lockTranslationsToGrid) {
							cumulativeTransformDelta += distance;
							if (Math.Abs(cumulativeTransformDelta) > TRANS_GRID_MIN_TRANSLATION) {
								float remainder = (cumulativeTransformDelta % TRANS_GRID_MIN_TRANSLATION);
								distance = cumulativeTransformDelta - remainder;
								cumulativeTransformDelta = remainder;
							}
							else return;
						}
						TranslateEntities(distance);
					}
					else {
						thisClickWasTransformAltering = false;
						MoveCamera(curDelta);
					}
				}
			});

			InputModule.AlwaysNotifyOfInput = true;
		}

		private static void RotateEntities(float rotRads) {
			thisClickWasTransformAltering = true;
			Monitor.Exit(staticMutationLock);
			if (lmbIsDepressed && rmbIsDepressed) { // Y-Axis
				EditorToolbar.RotateEntities(Quaternion.FromAxialRotation(Vector3.UP, rotRads));
			}
			else if (lmbIsDepressed) { // X-Axis
				EditorToolbar.RotateEntities(Quaternion.FromAxialRotation(Vector3.RIGHT, rotRads));
			}
			else if (rmbIsDepressed) { // Z-Axis
				EditorToolbar.RotateEntities(Quaternion.FromAxialRotation(Vector3.FORWARD, rotRads));
			}
			else thisClickWasTransformAltering = false;
			Monitor.Enter(staticMutationLock);
		}

		private static void TranslateEntities(float distance) {
			thisClickWasTransformAltering = true;
			Monitor.Exit(staticMutationLock);
			if (lmbIsDepressed && rmbIsDepressed) { // Y-Axis
				EditorToolbar.TranslateEntities(distance * Vector3.UP);
			}
			else if (lmbIsDepressed) { // X-Axis
				EditorToolbar.TranslateEntities(distance * Vector3.RIGHT);
			}
			else if (rmbIsDepressed) { // Z-Axis
				EditorToolbar.TranslateEntities(distance * Vector3.FORWARD);
			}
			else thisClickWasTransformAltering = false;
			Monitor.Enter(staticMutationLock);
		}

		private static void ScaleEntities(float delta) {
			thisClickWasTransformAltering = true;
			Monitor.Exit(staticMutationLock);
			if (lmbIsDepressed && rmbIsDepressed) { // Y-Axis
				EditorToolbar.ScaleEntities(new Vector3(1f, 1f + delta, 1f));
			}
			else if (lmbIsDepressed) { // X-Axis
				EditorToolbar.ScaleEntities(new Vector3(1f + delta, 1f, 1f));
			}
			else if (rmbIsDepressed) { // Z-Axis
				EditorToolbar.ScaleEntities(new Vector3(1f, 1f, 1f + delta));
			}
			else thisClickWasTransformAltering = false;
			Monitor.Enter(staticMutationLock);
		}

		private static void MoveCamera(Vector2 curDelta) {
			Camera camera = AssetLocator.MainCamera;
			if (lmbIsDepressed && rmbIsDepressed) {
				lmbMoveDistance += curDelta;
				rmbMoveDistance += curDelta;
				camera.Move(
					Quaternion.FromAxialRotation(Vector3.UP, -MathUtils.PI_OVER_TWO) * camera.Orientation * curDelta.X * 0.35f
					+ Vector3.DOWN * curDelta.Y * 0.35f
				);
			}
			else if (lmbIsDepressed) {
				lmbMoveDistance += curDelta;
				camera.Move(-curDelta.Y * GROUND_PLANE.RayProjection(new Ray(camera.Position, camera.Orientation)).Orientation * 0.35f);
				camera.Rotate(Quaternion.FromAxialRotation(Vector3.UP, -curDelta.X * 0.003f));
			}
			else if (rmbIsDepressed) {
				rmbMoveDistance += curDelta;
				Vector3 forwardVector = camera.Orientation;
				Quaternion rotation =
					Quaternion.FromAxialRotation(Vector3.Cross(forwardVector, camera.UpDirection), curDelta.Y * 0.003f)
					* Quaternion.FromAxialRotation(Vector3.UP, -curDelta.X * 0.003f);
				camera.Rotate(rotation);
			}
		}
	}
}