// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 26 05 2015 at 13:49 by Ben Bowen

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime;
using System.Runtime.Serialization.Formatters.Binary;
using Ophidian.Losgap;
using Ophidian.Losgap.Entities;
using Ophidian.Losgap.Rendering;
using Binder = Microsoft.CSharp.RuntimeBinder.Binder;

namespace Egodystonic.EscapeLizards {
	public static partial class AssetLocator {
		private static readonly object staticMutationLock = new object();
		private static readonly Dictionary<string, ITexture2D> loadedTextures = new Dictionary<string, ITexture2D>();
		private static readonly Dictionary<string, uint> textureRefCounts = new Dictionary<string, uint>();
		private static SceneLayer gameLayer;
		private static SceneLayer gameAlphaLayer;
		private static SceneLayer skyLayer;
		private static SceneLayer hudLayer;
		private static DLLightPass lightPass;
		private static DLGeometryPass mainGeometryPass;
		private static AlphaPass alphaPass;
		private static Font mainFont;
		private static Font titleFont;
		private static Font titleFontGlow;
		private static Window mainWindow;
		private static VertexShader mainGeomVS;
		private static VertexShader skyVS;
		private static FragmentShader geomFS;
		private static FragmentShader geomFSSky;
		private static FragmentShader skyFS;
		private static FragmentShader hudFS;
		private static FragmentShader hudFSText;
		private static FragmentShader alphaFS;
		private static Camera mainCamera;
		private static Camera shadowcasterCamera;
		private static PhysicsShapeHandle unitSphereShape;
		private static ShaderResourceView defaultNormalMapView;
		private static ShaderResourceView defaultSpecularMapView;
		private static ShaderResourceView defaultEmissiveMapView;
		private static Material greyedOutMaterial;

		public static Camera MainCamera {
			get {
				lock (staticMutationLock) {
					return mainCamera;
				}
			}
			set {
				lock (staticMutationLock) {
					mainCamera = value;
				}
			}
		}

		public static Camera ShadowcasterCamera {
			get {
				lock (staticMutationLock) {
					return shadowcasterCamera;
				}
			}
			set {
				lock (staticMutationLock) {
					shadowcasterCamera = value;
				}
			}
		}

		public static SceneLayer GameLayer {
			get {
				lock (staticMutationLock) {
					return gameLayer;
				}
			}
			set {
				lock (staticMutationLock) {
					gameLayer = value;
				}
			}
		}

		public static SceneLayer GameAlphaLayer {
			get {
				lock (staticMutationLock) {
					return gameAlphaLayer;
				}
			}
			set {
				lock (staticMutationLock) {
					gameAlphaLayer = value;
				}
			}
		}

		public static SceneLayer SkyLayer {
			get {
				lock (staticMutationLock) {
					return skyLayer;
				}
			}
			set {
				lock (staticMutationLock) {
					skyLayer = value;
				}
			}
		}

		public static SceneLayer HudLayer {
			get {
				lock (staticMutationLock) {
					return hudLayer;
				}
			}
			set {
				lock (staticMutationLock) {
					hudLayer = value;
				}
			}
		}

		public static DLLightPass LightPass {
			get {
				lock (staticMutationLock) {
					return lightPass;
				}
			}
			set {
				lock (staticMutationLock) {
					lightPass = value;
				}
			}
		}

		public static DLGeometryPass MainGeometryPass {
			get {
				lock (staticMutationLock) {
					return mainGeometryPass;
				}
			}
			set {
				lock (staticMutationLock) {
					mainGeometryPass = value;
				}
			}
		}

		public static AlphaPass AlphaPass {
			get {
				lock (staticMutationLock) {
					return alphaPass;
				}
			}
			set {
				lock (staticMutationLock) {
					alphaPass = value;
				}
			}
		}

		public static Font MainFont {
			get {
				lock (staticMutationLock) {
					return mainFont;
				}
			}
			set {
				lock (staticMutationLock) {
					mainFont = value;
				}
			}
		}

		public static Font TitleFont {
			get {
				lock (staticMutationLock) {
					return titleFont;
				}
			}
			set {
				lock (staticMutationLock) {
					titleFont = value;
				}
			}
		}

		public static Font TitleFontGlow {
			get {
				lock (staticMutationLock) {
					return titleFontGlow;
				}
			}
			set {
				lock (staticMutationLock) {
					titleFontGlow = value;
				}
			}
		}

		public static Window MainWindow {
			get {
				lock (staticMutationLock) {
					return mainWindow;
				}
			}
			set {
				lock (staticMutationLock) {
					mainWindow = value;
				}
			}
		}

		public static VertexShader MainGeometryVertexShader {
			get {
				lock (staticMutationLock) {
					return mainGeomVS;
				}
			}
			set {
				lock (staticMutationLock) {
					mainGeomVS = value;
				}
			}
		}

		public static VertexShader SkyVertexShader {
			get {
				lock (staticMutationLock) {
					return skyVS;
				}
			}
			set {
				lock (staticMutationLock) {
					skyVS = value;
				}
			}
		}

		public static FragmentShader GeometryFragmentShader {
			get {
				lock (staticMutationLock) {
					return geomFS;
				}
			}
			set {
				lock (staticMutationLock) {
					geomFS = value;
				}
			}
		}

		public static FragmentShader SkyGeometryFragmentShader {
			get {
				lock (staticMutationLock) {
					return geomFSSky;
				}
			}
			set {
				lock (staticMutationLock) {
					geomFSSky = value;
				}
			}
		}

		public static FragmentShader SkyFragmentShader {
			get {
				lock (staticMutationLock) {
					return skyFS;
				}
			}
			set {
				lock (staticMutationLock) {
					skyFS = value;
				}
			}
		}

		public static FragmentShader HUDFragmentShader {
			get {
				lock (staticMutationLock) {
					return hudFS;
				}
			}
			set {
				lock (staticMutationLock) {
					hudFS = value;
				}
			}
		}

		public static FragmentShader HUDTextFragmentShader {
			get {
				lock (staticMutationLock) {
					return hudFSText;
				}
			}
			set {
				lock (staticMutationLock) {
					hudFSText = value;
				}
			}
		}

		public static FragmentShader AlphaFragmentShader {
			get {
				lock (staticMutationLock) {
					return alphaFS;
				}
			}
			set {
				lock (staticMutationLock) {
					alphaFS = value;
				}
			}
		}

		public static PhysicsShapeHandle UnitSphereShape {
			get {
				lock (staticMutationLock) {
					return unitSphereShape;
				}
			}
			set {
				lock (staticMutationLock) {
					unitSphereShape = value;
				}
			}
		}

		public static string LevelsDir {
			get {
				return Path.Combine(LosgapSystem.InstallationDirectory.FullName, "Levels\\");
			}
		}

		public static string ModelsDir {
			get {
				return Path.Combine(LosgapSystem.InstallationDirectory.FullName, "Models\\");
			}
		}

		public static string MaterialsDir {
			get {
				return Path.Combine(LosgapSystem.InstallationDirectory.FullName, "Materials\\");
			}
		}

		public static string ShadersDir {
			get {
				return Path.Combine(LosgapSystem.InstallationDirectory.FullName, "Shaders\\");
			}
		}

		public static string FontsDir {
			get {
				return Path.Combine(LosgapSystem.InstallationDirectory.FullName, "Fonts\\");
			}
		}

		public static string AudioDir {
			get {
				return Path.Combine(LosgapSystem.InstallationDirectory.FullName, "Audio\\");
			}
		}

		public static ShaderResourceView DefaultNormalMapView {
			get {
				lock (staticMutationLock) {
					return defaultNormalMapView;
				}
			}
			set {
				lock (staticMutationLock) {
					defaultNormalMapView = value;
				}
			}
		}

		public static ShaderResourceView DefaultSpecularMapView {
			get {
				lock (staticMutationLock) {
					return defaultSpecularMapView;
				}
			}
			set {
				lock (staticMutationLock) {
					defaultSpecularMapView = value;
				}
			}
		}

		public static ShaderResourceView DefaultEmissiveMapView {
			get {
				lock (staticMutationLock) {
					return defaultEmissiveMapView;
				}
			}
			set {
				lock (staticMutationLock) {
					defaultEmissiveMapView = value;
				}
			}
		}


		public static Material GreyedOutMaterial {
			get {
				lock (staticMutationLock) {
					return greyedOutMaterial;
				}
			}
			set {
				lock (staticMutationLock) {
					greyedOutMaterial = value;
				}
			}
		}

		public static string CreateACDFilePath(string acdFileName) {
			string filenameWithoutExt = Path.GetFileNameWithoutExtension(acdFileName);
			return Path.Combine(ModelsDir, filenameWithoutExt + ".acd");
		}

		public static ITexture2D LoadTexture(string filename, bool generateMips = false) {
			Assure.NotNull(filename);
			lock (staticMutationLock) {
				if (loadedTextures.ContainsKey(filename)) {
					++textureRefCounts[filename];
					return loadedTextures[filename];
				}
			}
			string fullFilePath = Path.Combine(MaterialsDir, filename);
			if (!File.Exists(fullFilePath)) throw new FileNotFoundException("Given texture file '" + filename + "' is not in the materials folder.");

			ITexture2D loadedTex = null;

			GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
			GC.Collect(2, GCCollectionMode.Forced, true);

			if (generateMips) {
				var mipmapFilePath = Path.Combine(MaterialsDir, "Mipmaps\\", Path.GetFileNameWithoutExtension(filename)) + ".mipmap";
				if (File.Exists(mipmapFilePath)) {
					byte[] dataBytes = File.ReadAllBytes(mipmapFilePath);
					int texelTypeID;
					uint width, height;
					using (MemoryStream ms = new MemoryStream(dataBytes)) {
						BinaryReader br = new BinaryReader(ms);
						texelTypeID = br.ReadInt32();
						width = br.ReadUInt32();
						height = br.ReadUInt32();
					}

					Type texelType = TexelFormat.AllFormats.First(kvp => kvp.Value.ResourceFormatIndex == texelTypeID).Key;
					var createTex2DMethod = 
						typeof(TextureFactory)
						.GetMethod("NewTexture2D", BindingFlags.Public | BindingFlags.Static)
						.MakeGenericMethod(texelType);
					dynamic builder = createTex2DMethod.Invoke(null, new object[0]);
					loadedTex = builder.WithPermittedBindings(GPUBindings.ReadableShaderResource)
						.WithUsage(ResourceUsage.Immutable)
						.WithMipAllocation(true)
						.WithWidth(width)
						.WithHeight(height)
						.WithInitialData(new ArraySlice<byte>(dataBytes, 4U))
						.Create();
				}
				else {
					Logger.Log("No mipmap data found for file '" + fullFilePath + "'; ");
					LosgapSystem.InvokeOnMaster(() => {
						ITexture2D singleTex = TextureFactory.LoadTexture2D().WithFilePath(fullFilePath)
							.WithPermittedBindings(GPUBindings.None)
							.WithUsage(ResourceUsage.StagingReadWrite)
							.Create();

						ITexture2D mipGenTarget = ((dynamic) singleTex).Clone(false)
							.WithMipAllocation(true)
							.WithMipGenerationTarget(true)
							.WithPermittedBindings(GPUBindings.RenderTarget | GPUBindings.ReadableShaderResource)
							.WithUsage(ResourceUsage.Write)
							.Create();

						singleTex.GetType().GetMethod(
							"CopyTo",
							new[] { singleTex.GetType(), typeof(SubresourceBox), typeof(UInt32), typeof(UInt32), typeof(UInt32), typeof(UInt32) }
							).Invoke(
								singleTex,
								new object[] {
								mipGenTarget, new SubresourceBox(0U, singleTex.Width, 0U, singleTex.Height), 0U, 0U, 0U, 0U
							}
							);
						mipGenTarget.GenerateMips();

						ITexture2D stagingTex = ((dynamic) mipGenTarget).Clone(false)
							.WithPermittedBindings(GPUBindings.None)
							.WithUsage(ResourceUsage.StagingReadWrite)
							.WithMipGenerationTarget(false)
							.Create();

						mipGenTarget.CopyTo(stagingTex);

						using (MemoryStream ms = new MemoryStream()) {
							BinaryWriter bw = new BinaryWriter(ms);
							bw.Write(TexelFormat.AllFormats[mipGenTarget.TexelFormat].ResourceFormatIndex);
							bw.Write(stagingTex.Width);
							bw.Write(stagingTex.Height);
							bw.Write(stagingTex.ReadRaw());
							ms.Seek(0L, SeekOrigin.Begin);
							File.WriteAllBytes(mipmapFilePath, ms.ToArray());
						}
	
						loadedTex = ((dynamic) stagingTex).Clone(true)
							.WithPermittedBindings(GPUBindings.ReadableShaderResource)
							.WithUsage(ResourceUsage.Immutable)
							.Create();

						singleTex.Dispose();
						mipGenTarget.Dispose();
						stagingTex.Dispose();
					});
				}
			}
			else {
				loadedTex = TextureFactory.LoadTexture2D().WithFilePath(fullFilePath)
					.WithPermittedBindings(GPUBindings.ReadableShaderResource)
					.WithUsage(ResourceUsage.Immutable)
					.Create();
			}

			lock (staticMutationLock) {
				loadedTextures[filename] = loadedTex;
				textureRefCounts[filename] = 1U;
			}

			GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
			GC.Collect(2, GCCollectionMode.Forced, true);

			return loadedTex;
		}

		public static void UnloadTexture(string filename) {
			Assure.NotNull(filename);
			lock (staticMutationLock) {
				if (!textureRefCounts.ContainsKey(filename)) throw new KeyNotFoundException("Texture is already unloaded (or was never loaded in the first place).");
				if (--textureRefCounts[filename] == 0U) {
					loadedTextures[filename].Dispose();
					loadedTextures.Remove(filename);
				}
			}
		}
	}
}