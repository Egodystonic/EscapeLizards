// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 22 10 2014 at 14:47 by Ben Bowen

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap.Rendering {
	[TestClass]
	public class RenderingModuleTest {
		#region Tests
		[TestMethod]
		public void TestRenderStateParams() {
			// Define variables and constants
			

			// Set up context


			// Execute
			RenderingModule.AntialiasingLevel = MSAALevel.FourTimes;
			Assert.AreEqual(MSAALevel.FourTimes, RenderingModule.AntialiasingLevel);
			RenderingModule.AntialiasingLevel = MSAALevel.None;
			Assert.AreEqual(MSAALevel.None, RenderingModule.AntialiasingLevel);

			RenderingModule.VSyncEnabled = true;
			Assert.AreEqual(true, RenderingModule.VSyncEnabled);
			RenderingModule.VSyncEnabled = false;
			Assert.AreEqual(false, RenderingModule.VSyncEnabled);

			RenderingModule.MaxFrameRateHz = 200L;
			Assert.AreEqual(200L, RenderingModule.MaxFrameRateHz);
			RenderingModule.MaxFrameRateHz = null;
			Assert.AreEqual(null, RenderingModule.MaxFrameRateHz);

			// Assert outcome

		}

		[TestMethod]
		public void TestAddRemoveClearRenderPasses() {
			// Define variables and constants
			RenderPass a = new HUDPass("a");
			RenderPass b = new HUDPass("b");
			RenderPass c = new HUDPass("c");

			// Set up context
			RenderingModule.AddRenderPass(b);
			try {
				RenderingModule.AddRenderPass(b);
				Assert.Fail();
			}
			catch (InvalidOperationException) { }

			try {
				RenderingModule.RemoveRenderPass(c);
				Assert.Fail();
			}
			catch (InvalidOperationException) { }

			RenderingModule.AddRenderPass(c);
			RenderingModule.InsertRenderPass(a, b);

			IReadOnlyList<RenderPass> passList = RenderingModule.AddedPasses;

			Assert.AreEqual(a, passList[0]);
			Assert.AreEqual(b, passList[1]);
			Assert.AreEqual(c, passList[2]);

			RenderingModule.ClearPasses();

			passList = RenderingModule.AddedPasses;

			// Assert outcome
			Assert.AreEqual(0, passList.Count);

			a.Dispose();
			b.Dispose();
			c.Dispose();
		}

		[TestMethod]
		public void TestAutomaticSetupOfLayer() {
			// Define variables and constants
			const string LAYER_NAME = "Auto Layer Setup Layer";

			// Set up context

			// Execute
			Scene.CreateLayer(LAYER_NAME);

			// Assert outcome
			Assert.IsNotNull(Scene.GetLayer(LAYER_NAME).GetResource<LayerModelInstanceManager>());
			Assert.IsTrue(Scene.GetLayer(LAYER_NAME).GetRenderingEnabled());

			Scene.GetLayer(LAYER_NAME).Dispose();
		}

		[TestMethod]
		public void TestSetGlobalDetailReduction() {
			// Define variables and constants
			

			// Set up context


			// Execute
			RenderingModule.SetTextureDetailReduction(0U);
			RenderingModule.SetTextureDetailReduction(1U);
			RenderingModule.SetTextureDetailReduction(4U);
			RenderingModule.SetTextureDetailReduction(100U);
			RenderingModule.SetTextureDetailReduction(0U);

			// Assert outcome

		}
		#endregion
	}
}