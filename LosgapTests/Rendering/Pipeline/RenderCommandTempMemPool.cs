// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 27 02 2015 at 17:09 by Ben Bowen

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap.Rendering {
	[TestClass]
	public class RenderCommandTempMemPoolTest {

		[TestInitialize]
		public void SetUp() { }


		#region Tests
		[TestMethod]
		public void TestMemoryReuse() {
			RenderCommandTempMemPool pool = (RenderCommandTempMemPool) Activator.CreateInstance(typeof(RenderCommandTempMemPool), true);

			IntPtr tenBytes = pool.Reserve(10U);
			IntPtr twentyBytes = pool.Reserve(20U);
			IntPtr thirtyBytes = pool.Reserve(30U);
			IntPtr fortyBytes = pool.Reserve(40U);

			pool.FreeAll();

			Assert.AreEqual(tenBytes, pool.Reserve(9U));
			Assert.AreEqual(thirtyBytes, pool.Reserve(21U));
			Assert.AreEqual(fortyBytes, pool.Reserve(40U));
			Assert.AreEqual(twentyBytes, pool.Reserve(1U));

			IntPtr newAllocationA = pool.Reserve(1U);
			IntPtr newAllocationB = pool.Reserve(1U);

			Assert.AreNotEqual(tenBytes, newAllocationA);
			Assert.AreNotEqual(twentyBytes, newAllocationA);
			Assert.AreNotEqual(thirtyBytes, newAllocationA);
			Assert.AreNotEqual(fortyBytes, newAllocationA);
			Assert.AreNotEqual(tenBytes, newAllocationB);
			Assert.AreNotEqual(twentyBytes, newAllocationB);
			Assert.AreNotEqual(thirtyBytes, newAllocationB);
			Assert.AreNotEqual(fortyBytes, newAllocationB);
			
			Assert.AreNotEqual(newAllocationA, newAllocationB);

			pool.FreeAll();
		}
		#endregion
	}
}