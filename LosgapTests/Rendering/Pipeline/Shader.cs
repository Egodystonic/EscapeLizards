// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 20 01 2015 at 15:46 by Ben Bowen

using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap.Rendering {
	[TestClass]
	public class ShaderTest {

		[TestInitialize]
		public void SetUp() { }


		#region Tests
		[TestMethod]
		[ExpectedException(typeof(ArgumentException), AllowDerivedTypes = true)]
		public void TestDisallowsDuplicateBindingNames() {
			Shader shader = new FragmentShader(@"Tests\SimpleFS.cso", new ResourceViewBinding(0U, "A"), new ResourceViewBinding(1U, "A"));
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException), AllowDerivedTypes = true)]
		public void TestDisallowsDuplicateBindingSlots() {
			Shader shader = new FragmentShader(@"Tests\SimpleFS.cso", new TextureSamplerBinding(0U, "A"), new TextureSamplerBinding(0U, "B"));
		}

		[TestMethod]
		public void TestAllowsDuplicateSlotsForDifferentBindingTypes() {
			Shader shader = new FragmentShader(@"Tests\SimpleFS.cso", new TextureSamplerBinding(0U, "A"), new ResourceViewBinding(0U, "B"));
			shader.Dispose();
		}

		[TestMethod]
		public void TestProperties() {
			ConstantBuffer<Matrix> testBuffer = BufferFactory.NewConstantBuffer<Matrix>().WithUsage(ResourceUsage.DiscardWrite);
			Shader shader = new FragmentShader(
				@"Tests\SimpleFS.cso",
				new TextureSamplerBinding(0U, "TS0"),
				new TextureSamplerBinding(1U, "TS1"),
				new TextureSamplerBinding(5U, "TS5"),
				new ResourceViewBinding(0U, "RV0"),
				new ResourceViewBinding(2U, "RV2"),
				new ConstantBufferBinding(3U, "CB3", testBuffer)
			);

			Assert.AreEqual("SimpleFS", shader.Name);
			Assert.AreEqual(6, shader.ResourceBindings.Length);
			Assert.IsTrue(((TextureSamplerBinding) shader.ResourceBindings.Single(bind => bind.Identifier == "TS0")).SlotIndex == 0U);
			Assert.IsTrue(((TextureSamplerBinding) shader.ResourceBindings.Single(bind => bind.Identifier == "TS1")).SlotIndex == 1U);
			Assert.IsTrue(((TextureSamplerBinding) shader.ResourceBindings.Single(bind => bind.Identifier == "TS5")).SlotIndex == 5U);
			Assert.IsTrue(((ResourceViewBinding) shader.ResourceBindings.Single(bind => bind.Identifier == "RV0")).SlotIndex == 0U);
			Assert.IsTrue(((ResourceViewBinding) shader.ResourceBindings.Single(bind => bind.Identifier == "RV2")).SlotIndex == 2U);
			Assert.IsTrue(((ConstantBufferBinding) shader.ResourceBindings.Single(bind => bind.Identifier == "CB3")).SlotIndex == 3U);

			shader.Dispose();
			testBuffer.Dispose();
		}

		[TestMethod]
		public void TestBindingMethods() {
			ConstantBuffer<Matrix> testBuffer = BufferFactory.NewConstantBuffer<Matrix>().WithUsage(ResourceUsage.DiscardWrite);
			IShaderResourceBinding[] bindingArr = {
				new TextureSamplerBinding(0U, "TS0"),
				new TextureSamplerBinding(1U, "TS1"),
				new TextureSamplerBinding(5U, "TS5"),
				new ResourceViewBinding(0U, "RV0"),
				new ResourceViewBinding(2U, "RV2"),
				new ConstantBufferBinding(3U, "CB3", testBuffer)
			};

			Shader shader = new FragmentShader(@"Tests\SimpleFS.cso", bindingArr);

			Assert.AreEqual(bindingArr[0], shader.GetBindingByIdentifier("TS0"));
			Assert.AreEqual(bindingArr[1], shader.GetBindingByIdentifier("TS1"));
			Assert.AreEqual(bindingArr[2], shader.GetBindingByIdentifier("TS5"));
			Assert.AreEqual(bindingArr[3], shader.GetBindingByIdentifier("RV0"));
			Assert.AreEqual(bindingArr[4], shader.GetBindingByIdentifier("RV2"));
			Assert.AreEqual(bindingArr[5], shader.GetBindingByIdentifier("CB3"));

			Assert.IsTrue(shader.ContainsBinding("TS0"));
			Assert.IsTrue(shader.ContainsBinding("TS1"));
			Assert.IsFalse(shader.ContainsBinding("TS2"));

			Assert.IsTrue(shader.ContainsBinding(bindingArr[0]));
			Assert.IsTrue(shader.ContainsBinding(bindingArr[1]));
			Assert.IsFalse(shader.ContainsBinding(new TextureSamplerBinding(0U, "TS0")));

			shader.Dispose();
			testBuffer.Dispose();
		}
		#endregion
	}
}