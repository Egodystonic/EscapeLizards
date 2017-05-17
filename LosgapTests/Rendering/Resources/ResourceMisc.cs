// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 30 10 2014 at 11:45 by Ben Bowen

using System;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap.Rendering {
	[TestClass]
	public class ResourceMiscTest {
		[SupportedUsages(ResourceUsage.Immutable, ResourceUsage.DiscardWrite, ResourceUsage.Write,
			ResourceUsage.StagingRead, ResourceUsage.StagingWrite, ResourceUsage.StagingReadWrite)]
		private class TestResource : BaseResource {
			public TestResource(ResourceHandle resourceHandle, ResourceUsage usage, ByteSize size) : base(resourceHandle, usage, size) { }

			public override unsafe GPUBindings PermittedBindings {
				get { return GPUBindings.None; }
			}

			public void DiscardWrite() {
				ThrowIfCannotDiscardWrite();
			}

			public void Write() {
				ThrowIfCannotWrite();
			}

			public void Read() {
				ThrowIfCannotRead();
			}

			public void ReadWrite() {
				ThrowIfCannotReadWrite();
			}

			public void BeCopyDest() {
				ThrowIfCannotBeCopyDestination();
			}

			public override unsafe void Dispose() {
				typeof(BaseResource).GetField("isDisposed", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(this, true);
			}

			protected override unsafe string CreateResourceDescString() {
				return "TestResource" + DESC_VALUE_SEPARATOR;
			}
		}

		private class TestResourceBuilder : BaseResourceBuilder<TestResourceBuilder, TestResource, object> {
			public TestResourceBuilder(ResourceUsage usage, object initialData) : base(usage, initialData) { }
			public override TestResourceBuilder WithUsage(ResourceUsage usage) {
				return new TestResourceBuilder(usage, InitialData);
			}

			public override TestResourceBuilder WithInitialData(object initialData) {
				return new TestResourceBuilder(Usage, initialData);
			}

			public override TestResource Create() {
				return new TestResource(ResourceHandle.NULL, Usage, 10L);
			}
		}

		[TestInitialize]
		public void SetUp() { }


		#region Tests
		[TestMethod]
		public void ImplicitCastShouldNeverCreateMultipleResources() {
			// Define variables and constants
			TestResourceBuilder tf = new TestResourceBuilder(ResourceUsage.Immutable, null);

			// Set up context


			// Execute
			TestResource t1 = tf;
			TestResource t2 = tf;
			TestResource t3 = tf.WithInitialData(null);
			TestResource t4 = tf;
			TestResource t5 = tf.Create();
			TestResource t6 = tf;

			// Assert outcome
			Assert.AreSame(t1, t2);
			Assert.AreSame(t2, t4);
			Assert.AreSame(t4, t6);
			Assert.AreNotSame(t1, t3);
			Assert.AreNotSame(t1, t5);
			Assert.AreNotSame(t3, t5);

			t1.Dispose();
			t2.Dispose();
			t3.Dispose();
			t4.Dispose();
			t5.Dispose();
			t6.Dispose();
		}

		[TestMethod]
		public void ShouldCorrectlySpecifyAvailableOperations() {
			// Define variables and constants
			TestResource immutableResource = new TestResource(ResourceHandle.NULL, ResourceUsage.Immutable, 0L);
			TestResource discardWriteableResource = new TestResource(ResourceHandle.NULL, ResourceUsage.DiscardWrite, 0L);
			TestResource writeableResource = new TestResource(ResourceHandle.NULL, ResourceUsage.Write, 0L);
			TestResource stagingReadResource = new TestResource(ResourceHandle.NULL, ResourceUsage.StagingRead, 0L);
			TestResource stagingWriteResource = new TestResource(ResourceHandle.NULL, ResourceUsage.StagingWrite, 0L);
			TestResource stagingReadWriteResource = new TestResource(ResourceHandle.NULL, ResourceUsage.StagingReadWrite, 0L);

			// Set up context
			TestResource[] allResources = new TestResource[] {
				immutableResource,
				discardWriteableResource,
				writeableResource,
				stagingReadResource,
				stagingWriteResource,
				stagingReadWriteResource
			};

			// Execute


			// Assert outcome
			foreach (TestResource resource in allResources) {
				if (resource.CanDiscardWrite) resource.DiscardWrite();
				if (resource.CanWrite) resource.Write();
				if (resource.CanRead) resource.Read();
				if (resource.CanReadWrite) resource.ReadWrite();
				if (resource.CanBeCopyDestination) resource.BeCopyDest();
			}

			immutableResource.Dispose();
			discardWriteableResource.Dispose();
			writeableResource.Dispose();
			stagingReadResource.Dispose();
			stagingWriteResource.Dispose();
			stagingReadWriteResource.Dispose();
		}

		[TestMethod]
		public void TestResourceOperationAvailability() {
			// Define variables and constants
			TestResource immutableResource = new TestResource(ResourceHandle.NULL, ResourceUsage.Immutable, 0L);
			TestResource discardWriteableResource = new TestResource(ResourceHandle.NULL, ResourceUsage.DiscardWrite, 0L);
			TestResource writeableResource = new TestResource(ResourceHandle.NULL, ResourceUsage.Write, 0L);
			TestResource stagingReadResource = new TestResource(ResourceHandle.NULL, ResourceUsage.StagingRead, 0L);
			TestResource stagingWriteResource = new TestResource(ResourceHandle.NULL, ResourceUsage.StagingWrite, 0L);
			TestResource stagingReadWriteResource = new TestResource(ResourceHandle.NULL, ResourceUsage.StagingReadWrite, 0L);

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsFalse(immutableResource.CanDiscardWrite);
			Assert.IsFalse(immutableResource.CanWrite);
			Assert.IsFalse(immutableResource.CanRead);
			Assert.IsFalse(immutableResource.CanReadWrite);
			Assert.IsFalse(immutableResource.CanBeCopyDestination);

			Assert.IsTrue(discardWriteableResource.CanDiscardWrite);
			Assert.IsFalse(discardWriteableResource.CanWrite);
			Assert.IsFalse(discardWriteableResource.CanRead);
			Assert.IsFalse(discardWriteableResource.CanReadWrite);
			Assert.IsTrue(discardWriteableResource.CanBeCopyDestination);

			Assert.IsFalse(writeableResource.CanDiscardWrite);
			Assert.IsTrue(writeableResource.CanWrite);
			Assert.IsFalse(writeableResource.CanRead);
			Assert.IsFalse(writeableResource.CanReadWrite);
			Assert.IsTrue(writeableResource.CanBeCopyDestination);

			Assert.IsFalse(stagingReadResource.CanDiscardWrite);
			Assert.IsFalse(stagingReadResource.CanWrite);
			Assert.IsTrue(stagingReadResource.CanRead);
			Assert.IsFalse(stagingReadResource.CanReadWrite);
			Assert.IsTrue(stagingReadResource.CanBeCopyDestination);

			Assert.IsFalse(stagingWriteResource.CanDiscardWrite);
			Assert.IsTrue(stagingWriteResource.CanWrite);
			Assert.IsFalse(stagingWriteResource.CanRead);
			Assert.IsFalse(stagingWriteResource.CanReadWrite);
			Assert.IsTrue(stagingWriteResource.CanBeCopyDestination);

			Assert.IsFalse(stagingReadWriteResource.CanDiscardWrite);
			Assert.IsTrue(stagingReadWriteResource.CanWrite);
			Assert.IsTrue(stagingReadWriteResource.CanRead);
			Assert.IsTrue(stagingReadWriteResource.CanReadWrite);
			Assert.IsTrue(stagingReadWriteResource.CanBeCopyDestination);

			immutableResource.Dispose();
			discardWriteableResource.Dispose();
			writeableResource.Dispose();
			stagingReadResource.Dispose();
			stagingWriteResource.Dispose();
			stagingReadWriteResource.Dispose();
		}
		#endregion
	}
}