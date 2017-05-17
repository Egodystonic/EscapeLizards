// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 19 11 2014 at 15:45 by Ben Bowen

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap.Rendering {
	[TestClass]
	public class SubresourceBoxTest {

		[TestInitialize]
		public void SetUp() { }


		#region Tests
		[TestMethod]
		public void TestDerivedProperties() {
			// Define variables and constants
			SubresourceBox box1 = new SubresourceBox(10, 30);
			SubresourceBox box2 = new SubresourceBox(10, 30, 20, 50);
			SubresourceBox box3 = new SubresourceBox(10, 30, 20, 50, 60, 100);

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(20U, box1.Width);
			Assert.AreEqual(20U, box2.Width);
			Assert.AreEqual(20U, box3.Width);
			Assert.AreEqual(30U, box2.Height);
			Assert.AreEqual(30U, box3.Height);
			Assert.AreEqual(40U, box3.Depth);
			Assert.AreEqual(20U, box1.Volume);
			Assert.AreEqual(600U, box2.Volume);
			Assert.AreEqual(24000U, box3.Volume);
		} 
		#endregion
	}
}