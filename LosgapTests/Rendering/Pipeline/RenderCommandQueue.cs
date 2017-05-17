// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 24 02 2015 at 15:07 by Ben Bowen

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap.Rendering {
	[TestClass]
	public class RenderCommandQueueTest {

		[TestInitialize]
		public void SetUp() { }


		#region Tests
		[TestMethod]
		public void TestQueue() {
			RenderCommandQueue testRCQ = new ImmediateRCQ();

			Action a = () => Console.WriteLine("A");
			Action b = () => Console.WriteLine("B");
			Action c = () => Console.WriteLine("C");

			testRCQ.QueueAction(a);
			testRCQ.QueueCommand(new RenderCommand(RenderCommandInstruction.FSSetShader, 1, 2, 3));
			testRCQ.QueueCommand(new RenderCommand(RenderCommandInstruction.ClearDepthStencil, -1, -2, -3));
			uint comSlot = testRCQ.ReserveCommandSlot();
			testRCQ.QueueAction(b);
			testRCQ.QueueCommand(new RenderCommand(RenderCommandInstruction.SetVertexBuffers, 9, 10));
			testRCQ.ReserveCommandSlot(); // Deliberately ignored
			testRCQ.QueueCommand(comSlot, new RenderCommand(RenderCommandInstruction.SetIndexBuffer));
			testRCQ.QueueAction(c);

			RCQItem[] queuedItems = testRCQ.GetCurrentQueue();

			Assert.AreEqual(a, (Action) queuedItems[0]);
			Assert.AreEqual(new RenderCommand(RenderCommandInstruction.FSSetShader, 1, 2, 3), (RenderCommand) queuedItems[1]);
			Assert.AreEqual(new RenderCommand(RenderCommandInstruction.ClearDepthStencil, -1, -2, -3), (RenderCommand) queuedItems[2]);
			Assert.AreEqual(new RenderCommand(RenderCommandInstruction.SetIndexBuffer), (RenderCommand) queuedItems[3]);
			Assert.AreEqual(b, (Action) queuedItems[4]);
			Assert.AreEqual(new RenderCommand(RenderCommandInstruction.SetVertexBuffers, 9, 10), (RenderCommand) queuedItems[5]);
#if !DEVELOPMENT && !RELEASE
			Assert.AreEqual(new RenderCommand(RenderCommandInstruction.NoOperation, 12345678, 12345678, 12345678), (RenderCommand) queuedItems[6]);
#endif
			Assert.AreEqual(c, (Action) queuedItems[7]);
		} 
		#endregion
	}
}