// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 10 10 2014 at 16:19 by Ben Bowen

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ophidian.Losgap.Interop;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap {
	[TestClass]
	public class NativeTest {
		private const string NATIVE_DLL_NAME = "CoreNative.dll";

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "ReturnSuccess")]
		private static extern InteropBool ReturnSuccess(
			IntPtr failReason
		);

		[DllImport(NATIVE_DLL_NAME, CallingConvention = InteropUtils.DEFAULT_CALLING_CONVENTION,
			EntryPoint = "ReturnFailure")]
		private static extern InteropBool ReturnFailure(
			IntPtr failReason,
			[MarshalAs(InteropUtils.INTEROP_STRING_TYPE)] string customFailureMessage
		);

		[TestInitialize]
		public void SetUp() { }



		#region Tests
		[TestMethod]
		public void TestReturnSuccess() {
			// Define variables and constants


			// Set up context


			// Execute
			bool result;
			unsafe {
				char* failReason = stackalloc char[InteropUtils.MAX_INTEROP_FAIL_REASON_STRING_LENGTH + 1];
				result = ReturnSuccess((IntPtr) failReason);
			}

			// Assert outcome
			Assert.IsTrue(result);
		}

		[TestMethod]
		public void TestReturnFailure() {
			// Define variables and constants


			// Set up context


			// Execute
			bool result;
			unsafe {
				char* failReason = stackalloc char[InteropUtils.MAX_INTEROP_FAIL_REASON_STRING_LENGTH + 1];
				result = ReturnFailure((IntPtr) failReason, null);
			}

			// Assert outcome
			Assert.IsFalse(result);
		}

		[TestMethod]
		public void TestFailureMessage() {
			// Define variables and constants
			const string EXPECTED_FAIL_REASON = "Test failure.";

			// Set up context


			// Execute
			string failureMessage;
			unsafe {
				char* failReason = stackalloc char[InteropUtils.MAX_INTEROP_FAIL_REASON_STRING_LENGTH + 1];
				ReturnFailure((IntPtr) failReason, EXPECTED_FAIL_REASON);
				failureMessage = Marshal.PtrToStringUni((IntPtr) failReason);
			}

			// Assert outcome
			Assert.AreEqual(EXPECTED_FAIL_REASON, failureMessage);
		}

		[TestMethod]
		public void TestMaxFailureLength() {
			// Define variables and constants
			string tooLongReason = "certainty".Repeat(InteropUtils.MAX_INTEROP_FAIL_REASON_STRING_LENGTH);
			string expectedFailReason = tooLongReason.Substring(0, InteropUtils.MAX_INTEROP_FAIL_REASON_STRING_LENGTH);

			// Set up context


			// Execute
			string failureMessage;
			unsafe {
				char* failReason = stackalloc char[InteropUtils.MAX_INTEROP_FAIL_REASON_STRING_LENGTH + 1];
				ReturnFailure((IntPtr) failReason, tooLongReason);
				failureMessage = Marshal.PtrToStringUni((IntPtr) failReason);
			}

			// Assert outcome
			Assert.AreEqual(expectedFailReason, failureMessage);
		}

		[TestMethod]
		public void TestInteropUtilsNativeCallMethods() {
			// Define variables and constants
			const string EXPECTED_FAIL_REASON = "I feel sad, so left alone; words are not enough, to live on";

			// Set up context


			// Execute
			bool manualSuccessResult;
			unsafe {
				char* failReason = stackalloc char[InteropUtils.MAX_INTEROP_FAIL_REASON_STRING_LENGTH + 1];
				manualSuccessResult = ReturnSuccess((IntPtr) failReason);
			}
			bool manualFailureResult;
			string manualFailureMessage;
			unsafe {
				char* failReason = stackalloc char[InteropUtils.MAX_INTEROP_FAIL_REASON_STRING_LENGTH + 1];
				manualFailureResult = ReturnFailure((IntPtr) failReason, EXPECTED_FAIL_REASON);
				manualFailureMessage = Marshal.PtrToStringUni((IntPtr) failReason);
			}

			NativeCallResult successNCR = InteropUtils.CallNative(ReturnSuccess);
			NativeCallResult failureNCR = InteropUtils.CallNative(ReturnFailure, EXPECTED_FAIL_REASON);

			// Assert outcome
			Assert.AreEqual(manualSuccessResult, successNCR.Success);
			Assert.AreEqual(manualFailureResult, failureNCR.Success);
			Assert.IsNull(null, successNCR.FailureMessage);
			Assert.AreEqual(manualFailureMessage, failureNCR.FailureMessage);
		}
		#endregion
	}
}