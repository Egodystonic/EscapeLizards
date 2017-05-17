// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 22 12 2014 at 11:24 by Ben Bowen

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap {
	[TestClass]
	public class SceneTest {

		[TestInitialize]
		public void SetUp() { }


		#region Tests
		[TestMethod]
		public void TestLayerCreatedEvent() {
			// Define variables and constants
			const string TEST_LAYER_NAME = "Test Layer";
			SceneLayer createdLayer = null;

			// Set up context
			Scene.LayerCreated += layer => createdLayer = layer;

			// Execute
			Scene.CreateLayer(TEST_LAYER_NAME).Dispose();

			// Assert outcome
			Assert.AreEqual(TEST_LAYER_NAME, createdLayer.Name);
		}

		[TestMethod]
		public void TestGetLayer() {
			// Define variables and constants
			const string TEST_LAYER_NAME = "Test Layer 2";

			// Set up context
			Scene.CreateLayer(TEST_LAYER_NAME);
			Scene.CreateLayer(TEST_LAYER_NAME + "test");

			// Execute
			

			// Assert outcome
			Assert.AreEqual(TEST_LAYER_NAME, Scene.GetLayer(TEST_LAYER_NAME).Name);

			try {
				Scene.GetLayer("Not a layer");
				Assert.Fail();
			}
			catch (KeyNotFoundException) { }

			Scene.GetLayer(TEST_LAYER_NAME).Dispose();
			Scene.GetLayer(TEST_LAYER_NAME + "test").Dispose();
		}
		#endregion
	}
}