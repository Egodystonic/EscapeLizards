// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 24 02 2015 at 14:38 by Ben Bowen

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap.Rendering {
	[TestClass]
	public class ModelInstancePassTest {

		[TestInitialize]
		public void SetUp() { }


		#region Tests
		[TestMethod]
		public void TestIsValid() {
			SceneLayer testLayer = Scene.CreateLayer("Test layer");
			Window testWindow = new Window("Test window");
			ConstantBuffer<Matrix> instanceBuffer = BufferFactory.NewConstantBuffer<Matrix>().WithUsage(ResourceUsage.DiscardWrite);
			VertexShader testShader = VertexShader.NewDefaultShader(instanceBuffer);
			HUDPass testPass = new HUDPass("Test pass");

			Assert.IsFalse(testPass.IsValid);
			testPass.Input = new Camera();
			testPass.AddLayer(testLayer);
			testPass.Output = testWindow;
			testPass.VertexShader = testShader;
			Assert.IsFalse(testPass.IsValid);
			testPass.RasterizerState = new RasterizerState(true, TriangleCullMode.FrontfaceCulling, false);
			testPass.DepthStencilState = new DepthStencilState();
			Assert.IsTrue(testPass.IsValid);
			testPass.RasterizerState.Dispose();
			Assert.IsFalse(testPass.IsValid);

			testPass.DepthStencilState.Dispose();
			testPass.Dispose();
			testShader.Dispose();
			instanceBuffer.Dispose();
			testWindow.Close();
			testLayer.Dispose();
		}
		#endregion
	}
}