// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 22 12 2014 at 11:32 by Ben Bowen

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap {
	[TestClass]
	public class SceneLayerTest {

		[TestInitialize]
		public void SetUp() { }


		#region Tests
		[TestMethod]
		public void TestEnableDisableDispose() {
			// Define variables and constants
			SceneLayer testLayer = new SceneLayer("Test Layer");
			bool wasEnabled = false, wasDisabled = false, wasDisposed = false;

			// Set up context
			testLayer.LayerEnabled += layer => wasEnabled = true;
			testLayer.LayerDisabled += layer => wasDisabled = true;
			testLayer.LayerDisposed += layer => wasDisposed = true;

			// Execute
			testLayer.IsEnabled = false;
			testLayer.IsEnabled = true;
			testLayer.Dispose();

			// Assert outcome
			Assert.IsTrue(wasEnabled);
			Assert.IsTrue(wasDisabled);
			Assert.IsTrue(wasDisposed);
		}

		[TestMethod]
		public void TestGetSetProperties() {
			// Define variables and constants
			const int PROP1 = 34;
			const string PROP2 = "Hello";
			const bool PROP3 = false;
			const object PROP4 = null;
			SceneLayer testLayer = new SceneLayer("Test Layer");

			// Set up context
			object prop3Added = null;
			testLayer.PropertySet += (layer, s, arg3) => {
				if (s == "prop3") prop3Added = arg3;
			};

			testLayer.SetProperty("prop1", PROP1);
			testLayer["prop2"] = PROP2;
			testLayer.SetProperty("prop3", PROP3);
			testLayer["prop4"] = PROP4;

			// Execute
			Assert.AreEqual(PROP1, testLayer["prop1"]);
			Assert.AreEqual(PROP2, testLayer["prop2"]);
			Assert.AreEqual(PROP3, testLayer.GetProperty("prop3"));
			Assert.AreEqual(PROP4, testLayer.GetProperty("prop4"));
			Assert.AreEqual(PROP3, prop3Added);

			object prop1Removed = null;
			testLayer.PropertyRemoved += (layer, s, arg3) => {
				if (s == "prop1") prop1Removed = arg3;
			};

			testLayer.RemoveProperty("prop1");
			testLayer.RemoveProperty("prop3");

			// Assert outcome
			Assert.AreEqual(PROP1, prop1Removed);
			Assert.IsFalse(testLayer.ContainsProperty("prop1"));
			Assert.AreEqual(PROP2, testLayer["prop2"]);
			Assert.AreEqual(PROP4, testLayer.GetProperty("prop4"));

			try {
				var obj = testLayer["prop3"];
				Console.WriteLine(obj);
				Assert.Fail();
			}
			catch (KeyNotFoundException) { }

			testLayer.Dispose();
		}

		[TestMethod]
		public void TestResources() {
			// Define variables and constants
			SceneLayer testLayer = new SceneLayer("Test Layer");

			// Set up context
			TestLayerResource testResource = testLayer.GetResource<TestLayerResource>();

			// Execute
			Assert.IsFalse(testResource.IsDisabled);
			testLayer.IsEnabled = false;
			Assert.IsTrue(testResource.IsDisabled);
			testLayer.IsEnabled = true;
			Assert.IsTrue(testResource.IsEnabled);
			Assert.AreEqual(testLayer.GetResource<TestLayerResource>(), testResource);
			Assert.IsFalse(testResource.IsDisposed);
			testLayer.Dispose();
			Assert.IsTrue(testResource.IsDisposed);

			// Assert outcome
		}

		[TestMethod]
		public void ShouldCallEnableOnResourcesWhenConstructed() {
			SceneLayer testLayer = new SceneLayer("Test Layer");
			TestLayerResource testResource = testLayer.GetResource<TestLayerResource>();
			Assert.IsTrue(testResource.IsEnabled);
			testLayer.Dispose();
		}



		private class TestLayerResource : SceneLayerResource {
			public bool IsEnabled = false;
			public bool IsDisabled = false;
			public bool IsDisposed = false;

			public TestLayerResource(SceneLayer owningLayer) : base(owningLayer) { }

			protected internal override void Enable() {
				IsEnabled = true;
			}

			protected internal override void Disable() {
				IsDisabled = true;
			}

			protected internal override void Dispose() {
				IsDisposed = true;
			}
		}
		#endregion
	}
}