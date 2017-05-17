// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 20 01 2015 at 15:48 by Ben Bowen

using System;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap.Rendering {
	[TestClass]
	public class LayerModelInstanceManagerTest {

		[TestInitialize]
		public void SetUp() { }

		private struct TestVertex {
			[VertexComponent("POSITION")]
			private Vector3 position;

			public TestVertex(Vector3 position) {
				this.position = position;
			}
		}

		#region Tests
		[TestMethod]
		public unsafe void TestCreateAndDestroyInstance() {
			// Define variables and constants
			const int NUM_INSTANCES = 1000;

			var gcb = new GeometryCacheBuilder<TestVertex>();
			gcb.AddModel("TCADI_a", new[] { new TestVertex(Vector3.ONE), new TestVertex(Vector3.LEFT),  }, new[] { 0U, 1U, 1U, 0U, 1U, 0U });
			gcb.AddModel("TCADI_b", new[] { new TestVertex(Vector3.RIGHT), new TestVertex(Vector3.UP), }, new[] { 0U, 1U, 1U, 0U, 1U, 0U });
			gcb.AddModel("TCADI_c", new[] { new TestVertex(Vector3.ZERO), new TestVertex(Vector3.DOWN), }, new[] { 0U, 1U, 1U, 0U, 1U, 0U });
			GeometryCache testCache = gcb.Build();

			SceneLayer testLayerA = Scene.CreateLayer("Test Layer A");
			SceneLayer testLayerB = Scene.CreateLayer("Test Layer B");
			ConstantBuffer<Vector4> fsColorBuffer = BufferFactory.NewConstantBuffer<Vector4>().WithUsage(ResourceUsage.DiscardWrite);
			FragmentShader fs = new FragmentShader(@"Tests\SimpleFS.cso", new ConstantBufferBinding(0U, "MaterialProperties", fsColorBuffer));
			Material testMatA = new Material("Brick", fs);
			Material testMatB = new Material("Wood", fs);

			// Set up context


			// Execute
			ModelInstanceHandle[] instanceArr = new ModelInstanceHandle[NUM_INSTANCES];
			for (int i = 0; i < NUM_INSTANCES; ++i) {
				Transform transform = new Transform(
					Vector3.ONE * i,
					Quaternion.FromAxialRotation(Vector3.UP, i),
					Vector3.ONE * -i
				);
				if (i % 5 == 0) instanceArr[i] = testLayerA.CreateModelInstance(new ModelHandle(testCache.ID, (uint) (i % 3)), (i % 2 == 0 ? testMatA : testMatB), transform);
				else instanceArr[i] = testLayerB.CreateModelInstance(new ModelHandle(testCache.ID, (uint) (i % 3)), (i % 2 == 0 ? testMatA : testMatB), transform);
			}

			// Assert outcome
			RenderingModule.RenderStateBarrier.FreezeMutations(); // Cheeky, but we have to on debug mode (and it's re-entrant for now, so no problem)
			var instanceData = testCache.GetModelInstanceData();

			for (int i = 0; i < NUM_INSTANCES; ++i) {
				Material instanceMaterial = Material.GetMaterialByIndex(instanceArr[i].MaterialIndex);
				ModelInstanceManager.MIDArray materialDataArray = instanceData.First(kvp => kvp.Key == instanceMaterial).Value;

				Assert.AreEqual((i % 2 == 0 ? testMatA : testMatB), instanceMaterial);
				Assert.AreEqual((i % 5 == 0 ? testLayerA : testLayerB), Scene.GetLayerByIndex(materialDataArray.Data[GetMIHInstanceIndex(instanceArr[i])].SceneLayerIndex));
				Assert.IsTrue(materialDataArray.Data[GetMIHInstanceIndex(instanceArr[i])].InUse);
				Assert.AreEqual((uint) (i % 3), materialDataArray.Data[GetMIHInstanceIndex(instanceArr[i])].ModelIndex);
				Assert.AreEqual(
					new Transform(
						Vector3.ONE * i,
						Quaternion.FromAxialRotation(Vector3.UP, i),
						Vector3.ONE * -i
					),
					instanceArr[i].Transform
				);
				Assert.AreEqual(instanceArr[i].Transform, materialDataArray.Data[GetMIHInstanceIndex(instanceArr[i])].Transform);

				instanceArr[i].Dispose();
				Assert.IsFalse(materialDataArray.Data[GetMIHInstanceIndex(instanceArr[i])].InUse);
			}

			RenderingModule.RenderStateBarrier.UnfreezeMutations();

			testCache.Dispose();
			testMatA.Dispose();
			testMatB.Dispose();
			testLayerA.Dispose();
			testLayerB.Dispose();
			fs.Dispose();
			fsColorBuffer.Dispose();
		}
		#endregion


		private uint GetMIHInstanceIndex(ModelInstanceHandle mih) {
			FieldInfo fi = typeof(ModelInstanceHandle).GetField("instanceIndex", BindingFlags.NonPublic | BindingFlags.Instance);
			return (uint) fi.GetValue(mih);
		}
	}
}