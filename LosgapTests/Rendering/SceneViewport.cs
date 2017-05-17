// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 17 12 2014 at 20:18 by Ben Bowen

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ophidian.Losgap.Interop;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap.Rendering {
	[TestClass]
	public class SceneViewportTest {

		[TestInitialize]
		public void SetUp() { }


		#region Tests
		[TestMethod]
		public void TestCtor() {
			// Define variables and constants
			const ViewportAnchoring TEST_ANCHORING = ViewportAnchoring.BottomRight;
			Vector2 testAnchorOffset = new Vector2(0.3f, 0.4f);
			Vector2 testSize = new Vector2(0.6f, 0.5f);
			const float TEST_NEAR_PLANE = 0.1f;
			const float TEST_FAR_PLANE = 1000f;

			Window testWindow = new Window("VP Test Window");
			testWindow.ClearViewports();
			SceneViewport testViewport = testWindow.AddViewport(TEST_ANCHORING, testAnchorOffset, testSize, TEST_NEAR_PLANE, TEST_FAR_PLANE);

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual(TEST_ANCHORING, testViewport.Anchoring);
			Assert.AreEqual(testAnchorOffset, testViewport.AnchorOffset);
			Assert.AreEqual(testSize, testViewport.Size);
			Assert.AreEqual(TEST_NEAR_PLANE, testViewport.NearPlaneDist);
			Assert.AreEqual(TEST_FAR_PLANE, testViewport.FarPlaneDist);

			testWindow.Close();
		}
		
		[StructLayout(LayoutKind.Sequential, Pack = (int) InteropUtils.StructPacking.Optimal)]
		private struct D3D11_VIEWPORT {
			public float TopLeftX;
			public float TopLeftY;
			public float Width;
			public float Height;
			public float MinDepth;
			public float MaxDepth;
		}

		[TestMethod]
		public void TestResizeCalculations() {
			// Define variables and constants
			const uint WINDOW_WIDTH_A = 800;
			const uint WINDOW_HEIGHT_A = 600;
			const uint WINDOW_WIDTH_B = 1000;
			const uint WINDOW_HEIGHT_B = 500;
			Window testWindow = new Window("VP Test Window", WINDOW_WIDTH_A, WINDOW_HEIGHT_A);
			
			Vector2 anchorOffset = new Vector2(0.1f, 0.25f);
			Vector2 size = new Vector2(0.5f, 0.25f);

			((TestLoggingProvider) Logger.LoggingProvider).SuppressMessages();
			IDictionary<ViewportAnchoring, SceneViewport> viewports = new Dictionary<ViewportAnchoring, SceneViewport>();
			foreach (ViewportAnchoring anchoring in Enum.GetValues(typeof(ViewportAnchoring))) {
				viewports[anchoring] = testWindow.AddViewport(anchoring, anchorOffset, size);
			}

			D3D11_VIEWPORT vp;

			// Set up context


			// Execute
			

			// Assert outcome
			viewports.ForEach(kvp => {
				vp = GetViewportStruct(kvp.Value);
				Assert.AreEqual(400f, vp.Width);
				Assert.AreEqual(150f, vp.Height);
				Assert.AreEqual(0f, vp.MinDepth);
				Assert.AreEqual(1f, vp.MaxDepth);
			});

			vp = GetViewportStruct(viewports[ViewportAnchoring.TopLeft]);
			Assert.AreEqual(80f, vp.TopLeftX);
			Assert.AreEqual(150f, vp.TopLeftY);

			vp = GetViewportStruct(viewports[ViewportAnchoring.TopRight]);
			Assert.AreEqual(320f, vp.TopLeftX);
			Assert.AreEqual(150f, vp.TopLeftY);

			vp = GetViewportStruct(viewports[ViewportAnchoring.TopCentered]);
			Assert.AreEqual(200f, vp.TopLeftX);
			Assert.AreEqual(150f, vp.TopLeftY);

			vp = GetViewportStruct(viewports[ViewportAnchoring.BottomLeft]);
			Assert.AreEqual(80f, vp.TopLeftX);
			Assert.AreEqual(300f, vp.TopLeftY);

			vp = GetViewportStruct(viewports[ViewportAnchoring.BottomRight]);
			Assert.AreEqual(320f, vp.TopLeftX);
			Assert.AreEqual(300f, vp.TopLeftY);

			vp = GetViewportStruct(viewports[ViewportAnchoring.BottomCentered]);
			Assert.AreEqual(200f, vp.TopLeftX);
			Assert.AreEqual(300f, vp.TopLeftY);

			vp = GetViewportStruct(viewports[ViewportAnchoring.CenteredLeft]);
			Assert.AreEqual(80f, vp.TopLeftX);
			Assert.AreEqual(225f, vp.TopLeftY);

			vp = GetViewportStruct(viewports[ViewportAnchoring.CenteredRight]);
			Assert.AreEqual(320f, vp.TopLeftX);
			Assert.AreEqual(225f, vp.TopLeftY);

			vp = GetViewportStruct(viewports[ViewportAnchoring.Centered]);
			Assert.AreEqual(200f, vp.TopLeftX);
			Assert.AreEqual(225f, vp.TopLeftY);

			testWindow.SetResolution(WINDOW_WIDTH_B, WINDOW_HEIGHT_B);

			// Force a resize check here
			LosgapSystem.InvokeOnMaster(testWindow.HydrateMessagePump);

			viewports.ForEach(kvp => {
				vp = GetViewportStruct(kvp.Value);
				Assert.AreEqual(500f, vp.Width);
				Assert.AreEqual(125f, vp.Height);
				Assert.AreEqual(0f, vp.MinDepth);
				Assert.AreEqual(1f, vp.MaxDepth);
			});

			vp = GetViewportStruct(viewports[ViewportAnchoring.TopLeft]);
			Assert.AreEqual(100f, vp.TopLeftX);
			Assert.AreEqual(125f, vp.TopLeftY);

			vp = GetViewportStruct(viewports[ViewportAnchoring.TopRight]);
			Assert.AreEqual(400f, vp.TopLeftX);
			Assert.AreEqual(125f, vp.TopLeftY);

			vp = GetViewportStruct(viewports[ViewportAnchoring.TopCentered]);
			Assert.AreEqual(250f, vp.TopLeftX);
			Assert.AreEqual(125f, vp.TopLeftY);

			vp = GetViewportStruct(viewports[ViewportAnchoring.BottomLeft]);
			Assert.AreEqual(100f, vp.TopLeftX);
			Assert.AreEqual(250f, vp.TopLeftY);

			vp = GetViewportStruct(viewports[ViewportAnchoring.BottomRight]);
			Assert.AreEqual(400f, vp.TopLeftX);
			Assert.AreEqual(250f, vp.TopLeftY);

			vp = GetViewportStruct(viewports[ViewportAnchoring.BottomCentered]);
			Assert.AreEqual(250f, vp.TopLeftX);
			Assert.AreEqual(250f, vp.TopLeftY);

			vp = GetViewportStruct(viewports[ViewportAnchoring.CenteredLeft]);
			Assert.AreEqual(100f, vp.TopLeftX);
			Assert.AreEqual(187f, vp.TopLeftY);

			vp = GetViewportStruct(viewports[ViewportAnchoring.CenteredRight]);
			Assert.AreEqual(400f, vp.TopLeftX);
			Assert.AreEqual(187f, vp.TopLeftY);

			vp = GetViewportStruct(viewports[ViewportAnchoring.Centered]);
			Assert.AreEqual(250f, vp.TopLeftX);
			Assert.AreEqual(187f, vp.TopLeftY);

			testWindow.Close();
			((TestLoggingProvider) Logger.LoggingProvider).AllowMessages();
		}

		[TestMethod]
		public unsafe void TestGetRecalculatedProjMatrix() {
			Window testWindow = new Window("TestWindow", 800U, 600U);
			SceneViewport vp = testWindow.AddViewport(ViewportAnchoring.BottomLeft, Vector2.ZERO, Vector2.ONE, 0.1f, 1000f);
			SceneViewport vp2 = testWindow.AddViewport(ViewportAnchoring.BottomLeft, Vector2.ZERO, Vector2.ONE, 10f, 500f);
			Camera c = new Camera();
			c.Position = Vector3.ZERO;
			c.LookAt(Vector3.FORWARD, Vector3.UP);
			c.SetHorizontalFOV(MathUtils.PI_OVER_TWO);

			Assert.AreEqual(
				new Matrix(1f, 0f, 0f, 0f, 0f, 1.333f, 0f, 0f, 0f, 0f, 1f, 1f, 0f, 0f, -0.1f, 0f),
				*((Matrix*) vp.GetRecalculatedProjectionMatrix(c))
			);

			Assert.AreEqual(
				new Matrix(1f, 0f, 0f, 0f, 0f, 1.333f, 0f, 0f, 0f, 0f, 1.02f, 1f, 0f, 0f, -10.204f, 0f),
				*((Matrix*) vp2.GetRecalculatedProjectionMatrix(c))
			);

			c.SetHorizontalFOV(MathUtils.DegToRad(70f));

			Assert.AreEqual(
				new Matrix(1.428f, 0f, 0f, 0f, 0f, 1.904f, 0f, 0f, 0f, 0f, 1f, 1f, 0f, 0f, -0.1f, 0f),
				*((Matrix*) vp.GetRecalculatedProjectionMatrix(c))
			);

			testWindow.SetResolution(1024U, 768U);

			Assert.AreEqual(
				new Matrix(1.428f, 0f, 0f, 0f, 0f, 1.904f, 0f, 0f, 0f, 0f, 1f, 1f, 0f, 0f, -0.1f, 0f),
				*((Matrix*) vp.GetRecalculatedProjectionMatrix(c))
			);

			testWindow.SetResolution(1000U, 1000U);

			Assert.AreEqual(
				new Matrix(1.428f, 0f, 0f, 0f, 0f, 1.428f, 0f, 0f, 0f, 0f, 1f, 1f, 0f, 0f, -0.1f, 0f),
				*((Matrix*) vp.GetRecalculatedProjectionMatrix(c))
			);

			c.Dispose();
			vp2.Dispose();
			vp.Dispose();
			testWindow.Close();
		}

		private unsafe D3D11_VIEWPORT GetViewportStruct(SceneViewport vp) {
			return *((D3D11_VIEWPORT*) UnsafeUtils.Reinterpret<ViewportHandle, IntPtr>(vp.ViewportHandle, UnsafeUtils.SizeOf<ViewportHandle>()));
		}
		#endregion
	}
}