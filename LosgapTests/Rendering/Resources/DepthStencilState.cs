// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 20 02 2015 at 19:57 by Ben Bowen

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap.Rendering {
	[TestClass]
	public class DepthStencilStateTest {

		[TestInitialize]
		public void SetUp() { }


		#region Tests
		[TestMethod]
		public void ShouldCorrectlyAssignAllMembers() {
			const bool ENABLE_DEPTH_TEST = true;
			const DepthComparisonFunction DEPTH_COMPARISON_FUNCTION = DepthComparisonFunction.PassNonCoplanarElements;
			DepthStencilState testDSState = new DepthStencilState(
				ENABLE_DEPTH_TEST,
				DEPTH_COMPARISON_FUNCTION
			);

			Assert.AreEqual(ENABLE_DEPTH_TEST, testDSState.DepthTestingEnabled);
			Assert.AreEqual(DEPTH_COMPARISON_FUNCTION, testDSState.DepthComparisonFunction);

			testDSState.Dispose();
		}
		#endregion
	}
}