// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 20 02 2015 at 15:23 by Ben Bowen

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap.Rendering {
	[TestClass]
	public class ModelInstanceManagerTest {

		[TestInitialize]
		public void SetUp() { }


		#region Tests
		[TestMethod]
		public void TestAllMethods() {
			// Define variables and constants
			ModelInstanceManager testMIM = new ModelInstanceManager();
			const int NUM_ALLOCATIONS = 3000;
			const int NUM_MODELS = 7;
			const int NUM_MATERIALS = 11;
			const int NUM_SCENE_LAYERS = 3;

			ConstantBuffer<Vector4> fsCBuffer = BufferFactory.NewConstantBuffer<Vector4>().WithUsage(ResourceUsage.DiscardWrite);
			FragmentShader testFS = new FragmentShader(@"Tests\SimpleFS.cso", new ConstantBufferBinding(0U, "MaterialProperties", fsCBuffer));
			Material[] materials = new Material[NUM_MATERIALS];
			for (int i = 0; i < NUM_MATERIALS; ++i) {
				materials[i] = new Material(i.ToString(), testFS);
			}

			// Set up context
			ModelInstanceHandle[] instances = new ModelInstanceHandle[NUM_ALLOCATIONS];

			// Execute
			for (int i = 0; i < NUM_ALLOCATIONS; i++) {
				Transform initialTransform = new Transform(
					Vector3.ONE * i,
					Quaternion.FromAxialRotation(Vector3.ONE * (i + 1), MathUtils.PI),
					Vector3.ONE * -i
				);
				instances[i] = testMIM.AllocateInstance(materials[i % NUM_MATERIALS].Index, (uint) i % NUM_MODELS, (uint) i % NUM_SCENE_LAYERS, initialTransform);
			}

			for (int i = 0; i < NUM_ALLOCATIONS; i += 2) {
				instances[i].Transform = instances[i].Transform.With(scale: Vector3.FORWARD * i);
			}

			// Assert outcome
			RenderingModule.RenderStateBarrier.FreezeMutations();
			ArraySlice<KeyValuePair<Material, ModelInstanceManager.MIDArray>> midData = testMIM.GetModelInstanceData();
			RenderingModule.RenderStateBarrier.UnfreezeMutations();
			Assert.AreEqual(NUM_ALLOCATIONS, midData.Sum(kvp => {
				unsafe {
					int val = 0;
					for (int i = 0; i < kvp.Value.Length; ++i) {
						if (kvp.Value.Data[i].InUse) ++val;
					}
					return val;
				}
			}));

			unsafe {
				foreach (KeyValuePair<Material, ModelInstanceManager.MIDArray> kvp in midData) {
					Assert.IsTrue(materials.Contains(kvp.Key));
					for (uint i = 0U; i < kvp.Value.Length; ++i) {
						if (!kvp.Value.Data[i].InUse) continue;
						Assert.AreEqual(1, instances.Count(mih => mih.Transform == kvp.Value.Data[i].Transform));
					}
				}
			}

			materials.ForEach(mat => mat.Dispose());
			testFS.Dispose();
			fsCBuffer.Dispose();
		}
		#endregion
	}
}