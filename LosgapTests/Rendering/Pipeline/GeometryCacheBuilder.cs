// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 19 01 2015 at 13:28 by Ben Bowen

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap.Rendering {
	[TestClass]
	public class GeometryCacheBuilderTest {

		[TestInitialize]
		public void SetUp() { }

		private struct TestVertex {
			[VertexComponent("POSITION")]
			public readonly Vector3 Position;
			[VertexComponent("TEXCOORD")]
			public readonly Vector2 Texcoord;

			public TestVertex(Vector3 position, Vector2 texcoord) {
				Position = position;
				Texcoord = texcoord;
			}
		}

		#region Tests
		[TestMethod]
		public void ShouldCookParametersCorrectly() {
			// Define variables and constants
			GeometryCacheBuilder<TestVertex> testVertexCacheBuilder = new GeometryCacheBuilder<TestVertex>();
			int cacheID = (int) typeof(GeometryCacheBuilder<TestVertex>).GetField("cacheID", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(testVertexCacheBuilder);

			TestVertex[] model1Verts = {
				new TestVertex(Vector3.ONE * 0f, Vector2.ONE * 0f), 
				new TestVertex(Vector3.ONE * 1f, Vector2.ONE * 1f),
				new TestVertex(Vector3.ONE * 2f, Vector2.ONE * 2f),
			};
			TestVertex[] model2Verts = {
				new TestVertex(Vector3.ONE * 3f, Vector2.ONE * 3f), 
				new TestVertex(Vector3.ONE * 4f, Vector2.ONE * 4f),
				new TestVertex(Vector3.ONE * 5f, Vector2.ONE * 5f),
			};

			uint[] model1Indices = {
				0U, 1U, 2U,
				1U, 2U, 0U,
				2U, 0U, 1U
			};
			uint[] model2Indices = {
				3U, 4U, 5U,
				4U, 5U, 3U,
				5U, 3U, 4U,
				3U, 5U, 4U
			};

			uint outVBStartIndex, outIBStartIndex, outVBCount, outIBCount;

			// Set up context
			Assert.AreEqual(new ModelHandle(cacheID, 0U), testVertexCacheBuilder.AddModel("SCPC_a", model1Verts, model1Indices));
			Assert.AreEqual(new ModelHandle(cacheID, 1U), testVertexCacheBuilder.AddModel("SCPC_b", model2Verts, model2Indices));

			// Execute
			GeometryCache result = testVertexCacheBuilder.Build();

			// Assert outcome
			Assert.AreEqual(cacheID, result.ID);
			Assert.AreEqual(result, GeometryCache.GetCacheByID(cacheID));

			Assert.AreEqual((uint) (model1Indices.Length + model2Indices.Length), result.IndexBuffer.Length);
			Assert.AreEqual((uint) (model1Verts.Length + model2Verts.Length), result.VertexBuffers[0].Length);
			Assert.AreEqual(2, result.VertexBuffers.Count);
			Assert.AreEqual(ResourceFormat.R32G32B32Float, result.VertexFormats[0]);
			Assert.AreEqual(ResourceFormat.R32G32Float, result.VertexFormats[1]);
			Assert.AreEqual(2U, result.NumModels);
			Assert.AreEqual(6U, result.NumVertices);

			Assert.AreEqual(typeof(VertexBuffer<Vector3>), result.VertexBuffers[0].GetType());
			Assert.AreEqual(typeof(VertexBuffer<Vector2>), result.VertexBuffers[1].GetType());

			Assert.AreEqual("POSITION", result.VertexSemantics[0]);
			Assert.AreEqual("TEXCOORD", result.VertexSemantics[1]);

			result.GetModelBufferValues(0U, out outVBStartIndex, out outIBStartIndex, out outVBCount, out outIBCount);

			Assert.AreEqual(0U, outVBStartIndex);
			Assert.AreEqual(0U, outIBStartIndex);
			Assert.AreEqual(3U, outVBCount);
			Assert.AreEqual(9U, outIBCount);

			result.GetModelBufferValues(1U, out outVBStartIndex, out outIBStartIndex, out outVBCount, out outIBCount);

			Assert.AreEqual(3U, outVBStartIndex);
			Assert.AreEqual(9U, outIBStartIndex);
			Assert.AreEqual(3U, outVBCount);
			Assert.AreEqual(12U, outIBCount);

#if !DEVELOPMENT && !RELEASE
			try {
				result.GetModelBufferValues(2U, out outVBStartIndex, out outIBStartIndex, out outVBCount, out outIBCount);
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
#endif

			result.Dispose();
		}

		[TestMethod]
		public void ShouldRejectDuplicateModelNames() {
			GeometryCacheBuilder<DefaultVertex> gcbA = new GeometryCacheBuilder<DefaultVertex>();
			gcbA.AddModel("SRDMN_A", new[] { new DefaultVertex(Vector3.ONE) }, new[] { 1U });
			gcbA.AddModel("SRDMN_B", new[] { new DefaultVertex(Vector3.ONE) }, new[] { 1U });

			GeometryCache gcA = gcbA.Build();

			GeometryCacheBuilder<DefaultVertex> gcbB = new GeometryCacheBuilder<DefaultVertex>();
			gcbB.AddModel("SRDMN_1", new[] { new DefaultVertex(Vector3.ONE) }, new[] { 1U });
			gcbB.AddModel("SRDMN_2", new[] { new DefaultVertex(Vector3.ONE) }, new[] { 1U });
			gcbB.AddModel("SRDMN_3", new[] { new DefaultVertex(Vector3.ONE) }, new[] { 1U });
			GeometryCache gcB = gcbB.Build();

			GeometryCacheBuilder<DefaultVertex> gcbC = new GeometryCacheBuilder<DefaultVertex>();
			gcbC.AddModel("SRDMN_1", new[] { new DefaultVertex(Vector3.ONE) }, new[] { 1U });
			try {
				gcbC.Build();
				Assert.Fail();
			}
			catch (InvalidOperationException) { }

			gcbC = new GeometryCacheBuilder<DefaultVertex>();
			gcbC.AddModel("SRDMN_A", new[] { new DefaultVertex(Vector3.ONE) }, new[] { 1U });
			try {
				gcbC.Build();
				Assert.Fail();
			}
			catch (InvalidOperationException) { }

			gcbC = new GeometryCacheBuilder<DefaultVertex>();
			gcbC.AddModel("SRDMN_X", new[] { new DefaultVertex(Vector3.ONE) }, new[] { 1U });
			gcbC.AddModel("SRDMN_Y", new[] { new DefaultVertex(Vector3.ONE) }, new[] { 1U });
			gcbC.AddModel("SRDMN_Z", new[] { new DefaultVertex(Vector3.ONE) }, new[] { 1U });
			gcbC.AddModel("SRDMN_Z", new[] { new DefaultVertex(Vector3.ONE) }, new[] { 1U });
			try {
				gcbC.Build();
				Assert.Fail();
			}
			catch (InvalidOperationException) { }

			gcA.Dispose();
			gcB.Dispose();
		}
		#endregion
	}
}