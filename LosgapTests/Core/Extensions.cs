// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 08 10 2014 at 13:37 by Ben Bowen

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap {
	[TestClass]
	public partial class ExtensionsTest {

		[TestInitialize]
		public void SetUp() { }


		#region Tests
		[TestMethod]
		public void TestToStringNullSafe() {
			// Define variables and constants
			int? nonNullInt = 3;
			int? nullInt = null;
			const string CUSTOM_NULL_MESSAGE = "TEST STRING";

			// Set up context


			// Execute
			string nonNullIntToString = nonNullInt.ToStringNullSafe();
			string nullIntToString = nullInt.ToStringNullSafe(CUSTOM_NULL_MESSAGE);

			// Assert outcome
			Assert.AreEqual("3", nonNullIntToString);
			Assert.AreEqual(CUSTOM_NULL_MESSAGE, nullIntToString);
		}

		[TestMethod]
		public void TestHasCustomAttribute() {
			// Define variables and constants
			MethodInfo thisMethod = typeof(ExtensionsTest).GetMethod("TestHasCustomAttribute");

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsTrue(thisMethod.HasCustomAttribute<TestMethodAttribute>());
			Assert.IsFalse(thisMethod.HasCustomAttribute<ObsoleteAttribute>());
		}

		enum TestEnum {
			[Obsolete]
			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			A,
			[DebuggerDisplay("Test!")]
			B,
			C
		}

		[TestMethod]
		public void TestGetAttributes() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsTrue(TestEnum.C.GetAttributes<DebuggerDisplayAttribute>().Length == 0);
			Assert.IsTrue(TestEnum.B.GetAttributes<DebuggerDisplayAttribute>().Length == 1);
#pragma warning disable 612
			Assert.IsTrue(TestEnum.A.GetAttributes<Attribute>().Length == 2);
#pragma warning restore 612
		}

		[TestMethod]
		public void TestGetAttribute() {
			// Define variables and constants
			

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsTrue(TestEnum.B.GetAttribute<DebuggerDisplayAttribute>().Value == "Test!");
#pragma warning disable 612
			Assert.IsTrue(TestEnum.A.GetAttribute<DebuggerDisplayAttribute>() == null);
#pragma warning restore 612
		}

		[TestMethod]
		public void TestNumericToString() {
			// Define variables and constants
			const float FLOAT_A = 3.14f;
			const float FLOAT_B = 9.678913f;
			const double DOUBLE_A = 61361.7663d;
			const double DOUBLE_B = 10616d;
			const decimal DECIMAL_A = 1234.5678m;
			const decimal DECIMAL_B = 900.1513513616m;

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual("3:14", FLOAT_A.ToString(2).Replace(',', ':').Replace('.', ':'));
			Assert.AreEqual("9:679", FLOAT_B.ToString(3).Replace(',', ':').Replace('.', ':'));
			Assert.AreEqual("61:362", DOUBLE_A.ToString(0).Replace(',', ':').Replace('.', ':'));
			Assert.AreEqual("10:616:0000", DOUBLE_B.ToString(4).Replace(',', ':').Replace('.', ':'));
			Assert.AreEqual("1:234:5678", DECIMAL_A.ToString(4).Replace(',', ':').Replace('.', ':'));
			Assert.AreEqual("900:1513513616", DECIMAL_B.ToString(10).Replace(',', ':').Replace('.', ':'));
		}

		[TestMethod]
		public void TestForEachException() {
			// Define variables and constants
			IList<string> iteratedExceptionMessages = new List<string>();
			Exception testException =
				new ApplicationException("0",
					new ApplicationException("1",
						new ApplicationException("2",
							new ApplicationException("3",
								new ApplicationException("4")))));

			// Set up context


			// Execute
			testException.ForEachException(e => iteratedExceptionMessages.Add(e.Message));

			// Assert outcome
			for (int i = 0; i < 5; ++i) {
				Assert.AreEqual(i.ToString(), iteratedExceptionMessages[i]);
			}
		}

		[TestMethod]
		public void TestGetAllMessages() {
			// Define variables and constants
			const string MESSAGE_SEPARATOR = " >>> ";
			Exception testException =
				new ApplicationException("0",
					new ApplicationException("1",
						new ApplicationException("2",
							new ApplicationException("3",
								new ApplicationException("4")))));

			// Set up context


			// Execute
			string allMessages = testException.GetAllMessages(includeExceptionNames: false, messageSeparator: MESSAGE_SEPARATOR);

			// Assert outcome
			Assert.AreEqual(
				"0" + MESSAGE_SEPARATOR +
				"1" + MESSAGE_SEPARATOR +
				"2" + MESSAGE_SEPARATOR +
				"3" + MESSAGE_SEPARATOR +
				"4",
				allMessages
			);
		}

		[TestMethod]
		public void TestClamp() {
			// Define variables and constants
			const long LONG_MIN = -35L;
			const long LONG_MAX = 900L;
			const long LONG_TWELVE = 12L;
			const long LONG_MINUS_NINETY = -90L;
			const long LONG_FOUR_THOUSAND = 4000L;

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(LONG_TWELVE, LONG_TWELVE.Clamp(LONG_MIN, LONG_MAX));
			Assert.AreEqual(LONG_MIN, LONG_MINUS_NINETY.Clamp(LONG_MIN, LONG_MAX));
			Assert.AreEqual(LONG_MAX, LONG_FOUR_THOUSAND.Clamp(LONG_MIN, LONG_MAX));
		}
		#endregion
	}
}