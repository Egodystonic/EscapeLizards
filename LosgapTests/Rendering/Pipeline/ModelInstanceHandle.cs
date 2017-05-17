// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 16 02 2015 at 13:59 by Ben Bowen

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap.Rendering {
	[TestClass]
	public class ModelInstanceHandleTest {

		[TestInitialize]
		public void SetUp() { }


		#region Tests
		[TestMethod]
		public void TestCtor() {
			// Define variables and constants
			ModelInstanceManager mim = new ModelInstanceManager();
			ConstantBuffer<Vector4> fsCB = BufferFactory.NewConstantBuffer<Vector4>().WithUsage(ResourceUsage.DiscardWrite);
			FragmentShader fs = new FragmentShader(@"Tests\SimpleFS.cso", new ConstantBufferBinding(0U, "MaterialProperties", fsCB));
			Material testMat = new Material("TestMat", fs);
			SceneLayer testLayer = Scene.CreateLayer("TestLayer");
			ModelInstanceHandle testHandle = mim.AllocateInstance(testMat.Index, 0U, testLayer.Index, Transform.DEFAULT_TRANSFORM);

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(testMat, testHandle.Material);
			testHandle.Dispose();
			testLayer.Dispose();
			mim.Dispose();
			testMat.Dispose();
			fs.Dispose();
			fsCB.Dispose();
		}

		[TestMethod]
		public void TestTransform() {
			// Define variables and constants
			ModelInstanceManager mim = new ModelInstanceManager();
			ConstantBuffer<Vector4> fsCB = BufferFactory.NewConstantBuffer<Vector4>().WithUsage(ResourceUsage.DiscardWrite);
			FragmentShader fs = new FragmentShader(@"Tests\SimpleFS.cso", new ConstantBufferBinding(0U, "MaterialProperties", fsCB));
			Material testMat = new Material("TestMat", fs);
			SceneLayer testLayer = Scene.CreateLayer("TestLayer");
			ModelInstanceHandle testHandle = mim.AllocateInstance(testMat.Index, 0U, testLayer.Index, Transform.DEFAULT_TRANSFORM);

			// Set up context


			// Execute
			testHandle.Transform = new Transform(Vector3.ONE * 4f, Quaternion.IDENTITY, Vector3.ONE * -15f);

			// Assert outcome
			Assert.AreEqual(new Transform(Vector3.ONE * 4f, Quaternion.IDENTITY, Vector3.ONE * -15f), testHandle.Transform);
			testHandle.Dispose();
			testLayer.Dispose();
			mim.Dispose();
			testMat.Dispose();
			fs.Dispose();
			fsCB.Dispose();
		}
		#endregion
	}
}