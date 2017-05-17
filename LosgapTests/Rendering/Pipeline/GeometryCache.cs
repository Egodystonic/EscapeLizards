// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 16 01 2015 at 17:41 by Ben Bowen

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ophidian.Losgap.Interop;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap.Rendering {
	[TestClass]
	public class GeometryCacheTest {

		[TestInitialize]
		public void SetUp() { }



		struct FakeVertex {
			private readonly Vector3 a;
			private readonly Vector2 b;

			public FakeVertex(Vector3 a, Vector2 b) {
				this.a = a;
				this.b = b;
			}
		}


		#region Tests
		[TestMethod]
		public unsafe void TestGetModelBufferValues() {
			// Define variables and constants
			const int FAKE_CACHE_ID = 1351616;
			IVertexBuffer[] buffers = new IVertexBuffer[2];
			buffers[0] = BufferFactory.NewVertexBuffer<Vector3>()
				.WithInitialData(new[] { Vector3.ONE * 0f, Vector3.ONE * 1f, Vector3.ONE * 2f, Vector3.ONE * 3f, Vector3.ONE * 4f, Vector3.ONE * 5f })
				.WithUsage(ResourceUsage.Immutable)
				.Create();
			buffers[1] = BufferFactory.NewVertexBuffer<Vector2>()
				.WithInitialData(new[] { Vector2.ONE * 0f, Vector2.ONE * 1f, Vector2.ONE * 2f, Vector2.ONE * 3f, Vector2.ONE * 4f, Vector2.ONE * 5f })
				.WithUsage(ResourceUsage.Immutable)
				.Create();
			string[] semantics = { "POSITION", "TEXCOORD" };
			ResourceFormat[] formats = { ResourceFormat.R32G32B32Float, ResourceFormat.R32G32Float };
			IndexBuffer indices = BufferFactory.NewIndexBuffer()
				.WithInitialData(new uint[] { 0, 1, 2, 1, 2, 0, 2, 1, 0, 3, 4, 5, 4, 5, 3, 5, 3, 4, 5, 3, 4 })
				.WithUsage(ResourceUsage.Immutable);
			AlignedAllocation<uint> componentStartPointsAlloc = AlignedAllocation<uint>.AllocArray(7L, 3);
			*(((uint*) componentStartPointsAlloc.AlignedPointer) + 0) = 0U;
			*(((uint*) componentStartPointsAlloc.AlignedPointer) + 1) = 3U;
			*(((uint*) componentStartPointsAlloc.AlignedPointer) + 2) = 6U;
			AlignedAllocation<uint> indexStartPointsAlloc = AlignedAllocation<uint>.AllocArray(7L, 3);
			*(((uint*) indexStartPointsAlloc.AlignedPointer) + 0) = 0U;
			*(((uint*) indexStartPointsAlloc.AlignedPointer) + 1) = 9U;
			*(((uint*) indexStartPointsAlloc.AlignedPointer) + 2) = 21U;
			Dictionary<string, ModelHandle> modelNameMap = new Dictionary<string, ModelHandle>() {
				{ FAKE_CACHE_ID + "A", new ModelHandle(FAKE_CACHE_ID, 0U) },
				{ FAKE_CACHE_ID + "B", new ModelHandle(FAKE_CACHE_ID, 1U) }
			};
			const uint NUM_MODELS = 2;

			uint outVBStartIndex, outIBStartIndex, outVBCount, outIBCount;

			// Set up context
			GeometryCache cache = new GeometryCache(
				buffers, semantics, formats, 
				indices, componentStartPointsAlloc, indexStartPointsAlloc,
				NUM_MODELS, typeof(FakeVertex), FAKE_CACHE_ID, modelNameMap
			);

			// Execute
			cache.GetModelBufferValues(0U, out outVBStartIndex, out outIBStartIndex, out outVBCount, out outIBCount);

			// Assert outcome
			Assert.AreEqual(0U, outVBStartIndex);
			Assert.AreEqual(0U, outIBStartIndex);
			Assert.AreEqual(3U, outVBCount);
			Assert.AreEqual(9U, outIBCount);

			cache.GetModelBufferValues(1U, out outVBStartIndex, out outIBStartIndex, out outVBCount, out outIBCount);

			Assert.AreEqual(3U, outVBStartIndex);
			Assert.AreEqual(9U, outIBStartIndex);
			Assert.AreEqual(3U, outVBCount);
			Assert.AreEqual(12U, outIBCount);

#if !DEVELOPMENT && !RELEASE
			try {
				cache.GetModelBufferValues(2U, out outVBStartIndex, out outIBStartIndex, out outVBCount, out outIBCount);
				Assert.Fail();
			}
			catch (AssuranceFailedException) { }
#endif

			cache.Dispose();
		}

		[TestMethod]
		public unsafe void TestProperties() {
			// Define variables and constants
			const int FAKE_CACHE_ID = 1351617;
			IVertexBuffer[] buffers = new IVertexBuffer[2];
			buffers[0] = BufferFactory.NewVertexBuffer<Vector3>()
				.WithInitialData(new[] { Vector3.ONE * 0f, Vector3.ONE * 1f, Vector3.ONE * 2f, Vector3.ONE * 3f, Vector3.ONE * 4f, Vector3.ONE * 5f })
				.WithUsage(ResourceUsage.Immutable)
				.Create();
			buffers[1] = BufferFactory.NewVertexBuffer<Vector2>()
				.WithInitialData(new[] { Vector2.ONE * 0f, Vector2.ONE * 1f, Vector2.ONE * 2f, Vector2.ONE * 3f, Vector2.ONE * 4f, Vector2.ONE * 5f })
				.WithUsage(ResourceUsage.Immutable)
				.Create();
			string[] semantics = { "POSITION", "TEXCOORD" };
			ResourceFormat[] formats = { ResourceFormat.R32G32B32Float, ResourceFormat.R32G32Float };
			IndexBuffer indices = BufferFactory.NewIndexBuffer()
				.WithInitialData(new uint[] { 0, 1, 2, 1, 2, 0, 2, 1, 0, 3, 4, 5, 4, 5, 3, 5, 3, 4, 5, 3, 4 })
				.WithUsage(ResourceUsage.Immutable);
			AlignedAllocation<uint> componentStartPointsAlloc = AlignedAllocation<uint>.AllocArray(7L, 3);
			*(((uint*) componentStartPointsAlloc.AlignedPointer) + 0) = 0U;
			*(((uint*) componentStartPointsAlloc.AlignedPointer) + 1) = 3U;
			*(((uint*) componentStartPointsAlloc.AlignedPointer) + 2) = 6U;
			AlignedAllocation<uint> indexStartPointsAlloc = AlignedAllocation<uint>.AllocArray(7L, 3);
			*(((uint*) indexStartPointsAlloc.AlignedPointer) + 0) = 0U;
			*(((uint*) indexStartPointsAlloc.AlignedPointer) + 1) = 9U;
			*(((uint*) indexStartPointsAlloc.AlignedPointer) + 2) = 21U;
			Dictionary<string, ModelHandle> modelNameMap = new Dictionary<string, ModelHandle>() {
				{ FAKE_CACHE_ID + "A", new ModelHandle(FAKE_CACHE_ID, 0U) },
				{ FAKE_CACHE_ID + "B", new ModelHandle(FAKE_CACHE_ID, 1U) }
			};
			const uint NUM_MODELS = 2;

			// Set up context
			GeometryCache cache = new GeometryCache(
				buffers, semantics, formats,
				indices, componentStartPointsAlloc, indexStartPointsAlloc,
				NUM_MODELS, typeof(FakeVertex), FAKE_CACHE_ID, modelNameMap
			);


			// Execute
			

			// Assert outcome
			Assert.AreEqual(buffers[0], cache.VertexBuffers[0]);
			Assert.AreEqual(buffers[1], cache.VertexBuffers[1]);
			Assert.AreEqual(semantics[0], cache.VertexSemantics[0]);
			Assert.AreEqual(semantics[1], cache.VertexSemantics[1]);
			Assert.AreEqual(formats[0], cache.VertexFormats[0]);
			Assert.AreEqual(formats[1], cache.VertexFormats[1]);
			Assert.AreEqual(indices, cache.IndexBuffer);

			cache.Dispose();
		}

		[TestMethod]
		public unsafe void TestGetInputLayout() {
			// Define variables and constants
			const int FAKE_CACHE_ID = 1351618;
			IVertexBuffer[] buffers = new IVertexBuffer[2];
			buffers[0] = BufferFactory.NewVertexBuffer<Vector3>()
				.WithInitialData(new[] { Vector3.ONE * 0f, Vector3.ONE * 1f, Vector3.ONE * 2f, Vector3.ONE * 3f, Vector3.ONE * 4f, Vector3.ONE * 5f })
				.WithUsage(ResourceUsage.Immutable)
				.Create();
			buffers[1] = BufferFactory.NewVertexBuffer<float>()
				.WithInitialData(new[] { 0f, 1f, 2f, 3f, 4f, 5f })
				.WithUsage(ResourceUsage.Immutable)
				.Create();
			string[] semantics = { "POSITION", "RANDOM_FLOATS" };
			ResourceFormat[] formats = { ResourceFormat.R32G32B32Float, ResourceFormat.R32G32Float };
			IndexBuffer indices = BufferFactory.NewIndexBuffer()
				.WithInitialData(new uint[] { 0, 1, 2, 1, 2, 0, 2, 1, 0, 3, 4, 5, 4, 5, 3, 5, 3, 4, 5, 3, 4 })
				.WithUsage(ResourceUsage.Immutable);
			AlignedAllocation<uint> componentStartPointsAlloc = AlignedAllocation<uint>.AllocArray(7L, 3);
			*(((uint*) componentStartPointsAlloc.AlignedPointer) + 0) = 0U;
			*(((uint*) componentStartPointsAlloc.AlignedPointer) + 1) = 3U;
			*(((uint*) componentStartPointsAlloc.AlignedPointer) + 2) = 6U;
			AlignedAllocation<uint> indexStartPointsAlloc = AlignedAllocation<uint>.AllocArray(7L, 3);
			*(((uint*) indexStartPointsAlloc.AlignedPointer) + 0) = 0U;
			*(((uint*) indexStartPointsAlloc.AlignedPointer) + 1) = 9U;
			*(((uint*) indexStartPointsAlloc.AlignedPointer) + 2) = 21U;
			Dictionary<string, ModelHandle> modelNameMap = new Dictionary<string, ModelHandle>() {
				{ FAKE_CACHE_ID + "A", new ModelHandle(FAKE_CACHE_ID, 0U) },
				{ FAKE_CACHE_ID + "B", new ModelHandle(FAKE_CACHE_ID, 1U) }
			};
			const uint NUM_MODELS = 2;

			// Set up context
			GeometryCache cache = new GeometryCache(
				buffers, semantics, formats,
				indices, componentStartPointsAlloc, indexStartPointsAlloc,
				NUM_MODELS, typeof(Vector4), FAKE_CACHE_ID, modelNameMap
			);

			ConstantBuffer<Matrix> instanceCBuffer = BufferFactory.NewConstantBuffer<Matrix>().WithUsage(ResourceUsage.DiscardWrite);
			VertexShader vs1 = new VertexShader(
				@"Tests\SimpleVS.cso",
				new VertexInputBinding(0U, "INSTANCE_TRANSFORM"),
				new ConstantBufferBinding(0U, "CameraTransform", instanceCBuffer),
				new VertexInputBinding(1U, "POSITION")
			);
			VertexShader vs2 = new VertexShader(
				@"Tests\SimpleVS.cso",
				new VertexInputBinding(0U, "INSTANCE_TRANSFORM"),
				new ConstantBufferBinding(0U, "CameraTransform", instanceCBuffer),
				new VertexInputBinding(1U, "RANDOM_FLOATS"),
				new VertexInputBinding(2U, "POSITION")
			);
			VertexShader vs3 = new VertexShader(
				@"Tests\SimpleVS.cso",
				new VertexInputBinding(0U, "INSTANCE"),
				new ConstantBufferBinding(0U, "VPTransform", instanceCBuffer),
				new VertexInputBinding(1U, "TEXCOORD")
			);

			// Execute
			GeometryInputLayout inputLayout1 = cache.GetInputLayout(vs1);
			GeometryInputLayout inputLayout2 = cache.GetInputLayout(vs2);

			try {
				cache.GetInputLayout(vs3);
				Assert.Fail();
			}
			catch (InvalidOperationException) { }

			// Assert outcome
			Assert.AreEqual(cache, inputLayout1.AssociatedCache);
			Assert.AreEqual(cache, inputLayout2.AssociatedCache);

			Assert.AreEqual(vs1, inputLayout1.AssociatedShader);
			Assert.AreEqual(vs2, inputLayout2.AssociatedShader);

			KeyValuePair<VertexInputBinding, IVertexBuffer>[] boundCompBuffers = inputLayout1.BoundComponentBuffers;
			Assert.AreEqual(buffers[0], boundCompBuffers.First(kvp => kvp.Key == vs1.GetBindingByIdentifier("POSITION")).Value);
			Assert.IsFalse(boundCompBuffers.Any(kvp => kvp.Value == buffers[1]));

			boundCompBuffers = inputLayout2.BoundComponentBuffers;
			Assert.AreEqual(buffers[0], boundCompBuffers.First(kvp => kvp.Key == vs2.GetBindingByIdentifier("POSITION")).Value);
			Assert.AreEqual(buffers[1], boundCompBuffers.First(kvp => kvp.Key == vs2.GetBindingByIdentifier("RANDOM_FLOATS")).Value);

			Assert.AreEqual(inputLayout1, cache.GetInputLayout(vs1));
			Assert.AreEqual(inputLayout2, cache.GetInputLayout(vs2));

			cache.Dispose();
			inputLayout2.Dispose();
			inputLayout1.Dispose();
			vs1.Dispose();
			vs2.Dispose();
			vs3.Dispose();
			instanceCBuffer.Dispose();
			indices.Dispose();
			buffers[1].Dispose();
			buffers[0].Dispose();
		}

		[TestMethod]
		public void TestGettingModelsByName() {
			GeometryCacheBuilder<DefaultVertex> gcbA = new GeometryCacheBuilder<DefaultVertex>();
			ModelHandle modelA = gcbA.AddModel("TGMBN_A", new[] { new DefaultVertex(Vector3.ONE) }, new[] { 1U });
			ModelHandle modelB = gcbA.AddModel("TGMBN_B", new[] { new DefaultVertex(Vector3.ONE) }, new[] { 1U });

			GeometryCache gcA = gcbA.Build();

			GeometryCacheBuilder<DefaultVertex> gcbB = new GeometryCacheBuilder<DefaultVertex>();
			ModelHandle model1 = gcbB.AddModel("TGMBN_1", new[] { new DefaultVertex(Vector3.ONE) }, new[] { 1U });
			ModelHandle model2 = gcbB.AddModel("TGMBN_2", new[] { new DefaultVertex(Vector3.ONE) }, new[] { 1U });
			ModelHandle model3 = gcbB.AddModel("TGMBN_3", new[] { new DefaultVertex(Vector3.ONE) }, new[] { 1U });
			GeometryCache gcB = gcbB.Build();

			Assert.AreEqual(modelA, gcA.GetModelByName("TGMBN_A"));
			Assert.AreEqual(model3, gcB.GetModelByName("TGMBN_3"));

			Assert.AreEqual(model1, GeometryCache.GloballyGetModelByName("TGMBN_1"));
			Assert.AreEqual(model2, GeometryCache.GloballyGetModelByName("TGMBN_2"));
			Assert.AreEqual(modelB, GeometryCache.GloballyGetModelByName("TGMBN_B"));

			try {
				gcA.GetModelByName("TGMBN_1");
				Assert.Fail();
			}
			catch (KeyNotFoundException) { }

			try {
				gcB.GetModelByName("TGMBN_B");
				Assert.Fail();
			}
			catch (KeyNotFoundException) { }

			try {
				GeometryCache.GloballyGetModelByName("TGMBN_C");
				Assert.Fail();
			}
			catch (KeyNotFoundException) { }
		}
		#endregion
	}
}