// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 20 02 2015 at 19:57 by Ben Bowen

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap.Rendering {
	[TestClass]
	public class RasterizerStateTest {

		[TestInitialize]
		public void SetUp() { }


		#region Tests
		[TestMethod]
		public void ShouldCorrectlyAssignAllMembers() {
			const bool WIREFRAME_MODE = true;
			const TriangleCullMode CULL_MODE = TriangleCullMode.NoCulling;
			const bool FLIP_FACES = true;
			const int DEPTH_BIAS = 10;
			const float DEPTH_BIAS_CLAMP = 400f;
			const float SLOPE_SCALED_DEPTH_BIAS = -340f;
			const bool ENABLE_Z_CLIPPING = false;
			RasterizerState testRSState = new RasterizerState(
				WIREFRAME_MODE,
				CULL_MODE,
				FLIP_FACES,
				DEPTH_BIAS,
				DEPTH_BIAS_CLAMP,
				SLOPE_SCALED_DEPTH_BIAS,
				ENABLE_Z_CLIPPING
			);

			Assert.AreEqual(WIREFRAME_MODE, testRSState.WireframeMode);
			Assert.AreEqual(CULL_MODE, testRSState.TriangleCulling);
			Assert.AreEqual(FLIP_FACES, testRSState.FlipFaces);
			Assert.AreEqual(DEPTH_BIAS, testRSState.DepthBias);
			Assert.AreEqual(DEPTH_BIAS_CLAMP, testRSState.DepthBiasClamp);
			Assert.AreEqual(SLOPE_SCALED_DEPTH_BIAS, testRSState.SlopeScaledDepthBias);
			Assert.AreEqual(ENABLE_Z_CLIPPING, testRSState.EnableZClipping);

			testRSState.Dispose();
		}
		#endregion
	}
}