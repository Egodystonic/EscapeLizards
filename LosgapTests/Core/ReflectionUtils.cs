// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 02 03 2015 at 12:43 by Ben Bowen

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap {
	[TestClass]
	public class ReflectionUtilsTest {

		[TestInitialize]
		public void SetUp() { }


		#region Tests
		[TestMethod]
		public void TestGetChildTypes() {
			IEnumerable<Type> stringChildren = ReflectionUtils.GetChildTypes(typeof(String), false);
			IEnumerable<Type> stringChildrenWithString = ReflectionUtils.GetChildTypes(typeof(String));

			Assert.AreEqual(0, stringChildren.Count());
			Assert.AreEqual(1, stringChildrenWithString.Count());
			Assert.AreEqual(typeof(String), stringChildrenWithString.Single());

			IEnumerable<Type> testInterfaceAChildren = ReflectionUtils.GetChildTypes(typeof(ITestInterfaceA));
			Assert.AreEqual(6, testInterfaceAChildren.Count());
			Assert.IsTrue(testInterfaceAChildren.Contains(typeof(ITestInterfaceA)));
			Assert.IsTrue(testInterfaceAChildren.Contains(typeof(ITestInterfaceB)));
			Assert.IsTrue(testInterfaceAChildren.Contains(typeof(TestClassA)));
			Assert.IsTrue(testInterfaceAChildren.Contains(typeof(TestClassB)));
			Assert.IsTrue(testInterfaceAChildren.Contains(typeof(TestClassC)));
			Assert.IsTrue(testInterfaceAChildren.Contains(typeof(TestClassE)));

			IEnumerable<Type> testInterfaceBChildren = ReflectionUtils.GetChildTypes(typeof(ITestInterfaceB), false);
			Assert.AreEqual(1, testInterfaceBChildren.Count());
			Assert.IsTrue(testInterfaceBChildren.Contains(typeof(TestClassE)));

			IEnumerable<Type> testClassAChildren = ReflectionUtils.GetChildTypes(typeof(TestClassA));
			Assert.AreEqual(3, testClassAChildren.Count());
			Assert.IsTrue(testClassAChildren.Contains(typeof(TestClassA)));
			Assert.IsTrue(testClassAChildren.Contains(typeof(TestClassB)));
			Assert.IsTrue(testClassAChildren.Contains(typeof(TestClassC)));
		}
		#endregion

		private interface ITestInterfaceA { }

		private interface ITestInterfaceB : ITestInterfaceA { }

		private class TestClassA : ITestInterfaceA { }

		private class TestClassB : TestClassA { }

		private class TestClassC : TestClassB { }

		private class TestClassD { }

		private class TestClassE : TestClassD, ITestInterfaceB { }

	}
}