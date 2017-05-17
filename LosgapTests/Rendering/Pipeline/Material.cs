// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 20 01 2015 at 15:28 by Ben Bowen

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap.Rendering {
	[TestClass]
	public class MaterialTest {

		[TestInitialize]
		public void SetUp() { }


		#region Tests
		[TestMethod]
		public void TestSettingMaterialProperties() {
			ConstantBuffer<Vector4> matColorBuffer = BufferFactory.NewConstantBuffer<Vector4>().WithUsage(ResourceUsage.DiscardWrite);
			TextureSampler textureSampler = new TextureSampler(TextureFilterType.Anisotropic, TextureWrapMode.Border, AnisotropicFilteringLevel.EightTimes);
			FragmentShader testFS = new FragmentShader(
				@"Tests\SimpleFS.cso", 
				new ConstantBufferBinding(0U, "MaterialColor", matColorBuffer),
				new TextureSamplerBinding(0U, "DefaultSampler")
			);

			Material testMaterial = new Material("Test Material", testFS);

			testMaterial.SetMaterialConstantValue((ConstantBufferBinding) testFS.GetBindingByIdentifier("MaterialColor"), Vector4.ONE);
			testMaterial.SetMaterialResource((TextureSamplerBinding) testFS.GetBindingByIdentifier("DefaultSampler"), textureSampler);

#if !DEVELOPMENT && !RELEASE
			ConstantBufferBinding cb = new ConstantBufferBinding(1U, "Test", matColorBuffer);
			try {
				testMaterial.SetMaterialConstantValue(cb, Vector4.RIGHT);
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
			finally {
				(cb as IDisposable).Dispose();
			}

			try {
				testMaterial.SetMaterialConstantValue((ConstantBufferBinding) testFS.GetBindingByIdentifier("MaterialColor"), Matrix.IDENTITY);
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
#endif

			testFS.Dispose();
			matColorBuffer.Dispose();
			testMaterial.Dispose();
			textureSampler.Dispose();
		}

		[TestMethod]
		public unsafe void TestShaderResourcePackage() {
			ConstantBuffer<Vector4> matColorBuffer = BufferFactory.NewConstantBuffer<Vector4>().WithUsage(ResourceUsage.DiscardWrite);
			TextureSampler textureSampler = new TextureSampler(TextureFilterType.Anisotropic, TextureWrapMode.Border, AnisotropicFilteringLevel.EightTimes);
			FragmentShader testFS = new FragmentShader(
				@"Tests\SimpleFS.cso",
				new ConstantBufferBinding(0U, "MaterialColor", matColorBuffer),
				new TextureSamplerBinding(0U, "DefaultSampler")
			);

			Material testMaterial = new Material("Test Material", testFS);

			testMaterial.SetMaterialConstantValue((ConstantBufferBinding) testFS.GetBindingByIdentifier("MaterialColor"), Vector4.ONE);
			testMaterial.SetMaterialResource((TextureSamplerBinding) testFS.GetBindingByIdentifier("DefaultSampler"), textureSampler);

			ShaderResourcePackage shaderResourcePackage = testMaterial.FragmentShaderResourcePackage;
			Assert.AreEqual(Vector4.ONE, *((Vector4*) shaderResourcePackage.GetValue((ConstantBufferBinding) testFS.GetBindingByIdentifier("MaterialColor"))));
			Assert.AreEqual(textureSampler, shaderResourcePackage.GetValue((TextureSamplerBinding) testFS.GetBindingByIdentifier("DefaultSampler")));

			testFS.Dispose();
			matColorBuffer.Dispose();
			testMaterial.Dispose();
			textureSampler.Dispose();
		}
		#endregion
	}
}