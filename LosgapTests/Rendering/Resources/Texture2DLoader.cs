// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 12 03 2015 at 14:14 by Ben Bowen

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap.Rendering {
	[TestClass]
	public class Texture2DLoaderTest {

		[TestInitialize]
		public void SetUp() { }


		#region Tests
		[TestMethod]
		public void TestCreationParameters() {
			var defaultBuilder = TextureFactory.LoadTexture2D().WithUsage(ResourceUsage.Immutable).WithFilePath(@"Tests\potTex.bmp");
			var withStagingUsage = defaultBuilder.WithUsage(ResourceUsage.StagingReadWrite).WithPermittedBindings(GPUBindings.None);
			var withReadWriteBindings = defaultBuilder.WithUsage(ResourceUsage.Write).WithPermittedBindings(GPUBindings.ReadableShaderResource | GPUBindings.WritableShaderResource);

			ITexture2D tex = withStagingUsage.Create();
			Assert.AreEqual(ResourceUsage.StagingReadWrite, tex.Usage);
			tex.Dispose();

			tex = withReadWriteBindings.Create();
			Assert.AreEqual(GPUBindings.ReadableShaderResource | GPUBindings.WritableShaderResource, tex.PermittedBindings);
			tex.Dispose();

#if !DEVELOPMENT && !RELEASE
			try {
				TextureFactory.LoadTexture2D()
					.WithUsage(ResourceUsage.Immutable)
					.Create();
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
			try {
				TextureFactory.LoadTexture2D()
					.WithFilePath(@"Tests\potTex.bmp")
					.WithUsage(ResourceUsage.DiscardWrite)
					.WithPermittedBindings(GPUBindings.None)
					.Create();
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
			try {
				TextureFactory.LoadTexture2D()
					.WithFilePath(@"Tests\potTex.bmp")
					.WithUsage(ResourceUsage.StagingRead)
					.WithPermittedBindings(GPUBindings.ReadableShaderResource)
					.Create();
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
#endif
		}

		[TestMethod]
		public void TestDifferentFormats() {
			var defaultLoader = TextureFactory.LoadTexture2D()
				.WithUsage(ResourceUsage.Immutable)
				.WithPermittedBindings(GPUBindings.ReadableShaderResource);

			ITexture2D result;

			result = defaultLoader.WithFilePath(@"Tests\200x200.bmp").Create();
			Assert.AreEqual(200U, result.Width);
			Assert.AreEqual(200U, result.Height);
			Assert.AreEqual(typeof(Texture2D<TexelFormat.RGBA8UNorm>), result.GetType());
			Assert.AreEqual(0U, result.ArrayIndex);
			Assert.AreEqual(false, result.IsArrayTexture);
			Assert.AreEqual(false, result.IsGlobalDetailTarget);
			Assert.AreEqual(false, result.IsMipGenTarget);
			Assert.AreEqual(false, result.IsMipmapped);
			Assert.AreEqual(false, result.IsMultisampled);
			Assert.AreEqual(1U, result.NumMips);
			Assert.AreEqual(200U * 200U * 4U, result.Size);
			Assert.AreEqual(200U * 200U, result.SizeTexels);
			Assert.AreEqual(typeof(TexelFormat.RGBA8UNorm), result.TexelFormat);
			Assert.AreEqual(4U, result.TexelSizeBytes);

			LosgapSystem.InvokeOnMaster(() => {
				Texture2D<TexelFormat.RGBA8UNorm> dataCopyTex = (result as Texture2D<TexelFormat.RGBA8UNorm>).Clone()
					.WithUsage(ResourceUsage.StagingRead)
					.WithPermittedBindings(GPUBindings.None);

				result.CopyTo(dataCopyTex);

				Assert.AreEqual(UnsafeUtils.Reinterpret<uint, TexelFormat.RGBA8UNorm>(4294958703U, 4), dataCopyTex.Read(0U)[138, 99]);

				dataCopyTex.Dispose();
			});

			result.Dispose();

			result = defaultLoader.WithFilePath(@"Tests\200x200.png").Create();
			Assert.AreEqual(200U, result.Width);
			Assert.AreEqual(200U, result.Height);
			Assert.AreEqual(typeof(Texture2D<TexelFormat.RGBA8UNormSRGB>), result.GetType());
			Assert.AreEqual(0U, result.ArrayIndex);
			Assert.AreEqual(false, result.IsArrayTexture);
			Assert.AreEqual(false, result.IsGlobalDetailTarget);
			Assert.AreEqual(false, result.IsMipGenTarget);
			Assert.AreEqual(false, result.IsMipmapped);
			Assert.AreEqual(false, result.IsMultisampled);
			Assert.AreEqual(1U, result.NumMips);
			Assert.AreEqual(200U * 200U * 4U, result.Size);
			Assert.AreEqual(200U * 200U, result.SizeTexels);
			Assert.AreEqual(typeof(TexelFormat.RGBA8UNormSRGB), result.TexelFormat);
			Assert.AreEqual(4U, result.TexelSizeBytes);

			LosgapSystem.InvokeOnMaster(() => {
				Texture2D<TexelFormat.RGBA8UNormSRGB> dataCopyTex = (result as Texture2D<TexelFormat.RGBA8UNormSRGB>).Clone()
					.WithUsage(ResourceUsage.StagingRead)
					.WithPermittedBindings(GPUBindings.None);

				result.CopyTo(dataCopyTex);

				Assert.AreEqual(UnsafeUtils.Reinterpret<uint, TexelFormat.RGBA8UNormSRGB>(4294958703U, 4), dataCopyTex.Read(0U)[138, 99]);

				dataCopyTex.Dispose();
			});

			result.Dispose();

			result = defaultLoader.WithFilePath(@"Tests\200x200.gif").Create();
			Assert.AreEqual(200U, result.Width);
			Assert.AreEqual(200U, result.Height);
			Assert.AreEqual(typeof(Texture2D<TexelFormat.RGBA8UNorm>), result.GetType());
			Assert.AreEqual(0U, result.ArrayIndex);
			Assert.AreEqual(false, result.IsArrayTexture);
			Assert.AreEqual(false, result.IsGlobalDetailTarget);
			Assert.AreEqual(false, result.IsMipGenTarget);
			Assert.AreEqual(false, result.IsMipmapped);
			Assert.AreEqual(false, result.IsMultisampled);
			Assert.AreEqual(1U, result.NumMips);
			Assert.AreEqual(200U * 200U * 4U, result.Size);
			Assert.AreEqual(200U * 200U, result.SizeTexels);
			Assert.AreEqual(typeof(TexelFormat.RGBA8UNorm), result.TexelFormat);
			Assert.AreEqual(4U, result.TexelSizeBytes);

			LosgapSystem.InvokeOnMaster(() => {
				Texture2D<TexelFormat.RGBA8UNorm> dataCopyTex = (result as Texture2D<TexelFormat.RGBA8UNorm>).Clone()
					.WithUsage(ResourceUsage.StagingRead)
					.WithPermittedBindings(GPUBindings.None);

				result.CopyTo(dataCopyTex);

				Assert.AreEqual(UnsafeUtils.Reinterpret<uint, TexelFormat.RGBA8UNorm>(4294958702U, 4), dataCopyTex.Read(0U)[138, 99]);

				dataCopyTex.Dispose();
			});

			result.Dispose();

			result = defaultLoader.WithFilePath(@"Tests\200x200.jpg").Create();
			Assert.AreEqual(200U, result.Width);
			Assert.AreEqual(200U, result.Height);
			Assert.AreEqual(typeof(Texture2D<TexelFormat.RGBA8UNorm>), result.GetType());
			Assert.AreEqual(0U, result.ArrayIndex);
			Assert.AreEqual(false, result.IsArrayTexture);
			Assert.AreEqual(false, result.IsGlobalDetailTarget);
			Assert.AreEqual(false, result.IsMipGenTarget);
			Assert.AreEqual(false, result.IsMipmapped);
			Assert.AreEqual(false, result.IsMultisampled);
			Assert.AreEqual(1U, result.NumMips);
			Assert.AreEqual(200U * 200U * 4U, result.Size);
			Assert.AreEqual(200U * 200U, result.SizeTexels);
			Assert.AreEqual(typeof(TexelFormat.RGBA8UNorm), result.TexelFormat);
			Assert.AreEqual(4U, result.TexelSizeBytes);

			LosgapSystem.InvokeOnMaster(() => {
				Texture2D<TexelFormat.RGBA8UNorm> dataCopyTex = (result as Texture2D<TexelFormat.RGBA8UNorm>).Clone()
					.WithUsage(ResourceUsage.StagingRead)
					.WithPermittedBindings(GPUBindings.None);

				result.CopyTo(dataCopyTex);

				Assert.AreEqual(UnsafeUtils.Reinterpret<uint, TexelFormat.RGBA8UNorm>(4294955895U, 4), dataCopyTex.Read(0U)[138, 99]);

				dataCopyTex.Dispose();
			});

			result.Dispose();
		}
		#endregion
	}
}