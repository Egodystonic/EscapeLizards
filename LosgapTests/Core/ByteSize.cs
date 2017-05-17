// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 14 10 2014 at 12:53 by Ben Bowen

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap {
	[TestClass]
	public class ByteSizeTest {

		[TestInitialize]
		public void SetUp() { }


		#region Tests
		[TestMethod]
		public void TestConversions() {
			// Define variables and constants
			const double TEST_ERROR_MARGIN = 0.001d;
			ByteSize bytes = 985L;
			ByteSize kibibytes = 8478L;
			ByteSize mebibytes = 238478958L;
			ByteSize gibibytes = 45238478958L;
			ByteSize tebibytes = 3044445238478958L;

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(985L, bytes.InBytes);
			Assert.AreEqual(985L, (long) bytes);
			Assert.AreEqual(8.279d, kibibytes.InKibibytes, TEST_ERROR_MARGIN);
			Assert.AreEqual(227.431d, mebibytes.InMebibytes, TEST_ERROR_MARGIN);
			Assert.AreEqual(42.131d, gibibytes.InGibibytes, TEST_ERROR_MARGIN);
			Assert.AreEqual(2768.906d, tebibytes.InTebibytes, TEST_ERROR_MARGIN);
		}

		[TestMethod]
		public void TestSecondaryCtor() {
			// Define variables and constants
			const double NUM_TEBIBYTES = 1.359d;
			const double NUM_GIBIBYTES = 0.5d;
			const double NUM_MEBIBYTES = 120.079d;
			const double NUM_KIBIBYTES = 400d;
			const long NUM_BYTES = 410L;
			const double TEST_ERROR_MARGIN = 1000d; // double is losing accuracy at these ranges and is platform + implementation specific
			const double EXPECTED_TOTAL_KIB = (409600L + 125911957L + 536870912L + 1494236302147L) / 1024d;

			// Set up context


			// Execute
			ByteSize totalBytes = new ByteSize(
				NUM_BYTES, NUM_KIBIBYTES, NUM_MEBIBYTES, NUM_GIBIBYTES, NUM_TEBIBYTES
			);

			// Assert outcome
			Assert.AreEqual(EXPECTED_TOTAL_KIB, totalBytes.InKibibytes, TEST_ERROR_MARGIN);
		}

		[TestMethod]
		public void TestToString() {
			// Define variables and constants
			ByteSize bytes = 985L;
			ByteSize kibibytes = 8478L;
			ByteSize mebibytes = 238478958L;
			ByteSize gibibytes = 45238478958L;
			ByteSize tebibytes = 3044445238478958L;

			// Set up context
			ByteSize.UseIECUnitsInToString = false;

			// Execute


			// Assert outcome
			Assert.AreEqual("985 B", bytes.ToString().Replace(',', ':').Replace('.', ':'));
			Assert.AreEqual("8:279 KB", kibibytes.ToString().Replace(',', ':').Replace('.', ':'));
			Assert.AreEqual("227:431 MB", mebibytes.ToString().Replace(',', ':').Replace('.', ':'));
			Assert.AreEqual("42:132 GB", gibibytes.ToString().Replace(',', ':').Replace('.', ':'));
			Assert.AreEqual("2:768:907 TB", tebibytes.ToString().Replace(',', ':').Replace('.', ':'));
		}

		[TestMethod]
		public void TestOperators() {
			// Define variables and constants
			ByteSize bytes = 985L;
			ByteSize kibibytes = 8478L;
			ByteSize mebibytes = 238478958L;
			ByteSize gibibytes = 45238478958L;
			ByteSize tebibytes = 3044445238478958L;

			// Set up context


			// Execute


			// Assert outcome
#pragma warning disable 1718
			// ReSharper disable once EqualExpressionComparison
			Assert.IsTrue(bytes == bytes);
#pragma warning restore 1718
			Assert.IsTrue(bytes == 985L);
			Assert.IsTrue(bytes > 10L);
			Assert.IsTrue(kibibytes >= 985L);
			Assert.IsTrue(kibibytes < gibibytes);
			Assert.IsTrue(45238478958L <= gibibytes);
			Assert.IsTrue(45238478958L != tebibytes);
			Assert.IsTrue(bytes * 2L == 1970L);
			Assert.IsTrue(kibibytes / 10L == 847L);
		}
		#endregion
	}
}