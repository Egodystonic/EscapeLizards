// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 22 10 2014 at 15:20 by Ben Bowen

using System;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap.Rendering {
	[TestClass]
	public class WindowTest {
		#region Tests
		[TestMethod]
		[Timeout(10000)]
		public void ShouldCorrectlyLookAfterOpenWindowsList() {
			// Define variables and constants
			

			// Set up context


			// Execute
			new Window("Unit test window");
			Window.OpenWindows.ForEach(win => win.Close());
			Assert.IsFalse(Window.OpenWindows.Any());

			// Assert outcome

		}

		[TestMethod]
		[Timeout(10000)]
		public void TestWindowCloseEvent() {
			// Define variables and constants
			Window testWindow = new Window("Unit test window");
			bool closeCalled = false;

			// Set up context
			testWindow.WindowClosed += win => {
				closeCalled = true;
			};

			// Execute
			testWindow.Close();

			// Assert outcome
			Assert.IsTrue(closeCalled);
			Assert.IsTrue(testWindow.IsClosed);
		}

		[TestMethod]
		[Timeout(20000)]
		public void PropertiesShouldRetainTheirValues() {
			// Define variables and constants
			const uint TEST_HEIGHT_PX = 123;
			const uint TEST_WIDTH_PX = 234;
			const uint TEST_RES_WIDTH_PX = 345;
			const uint TEST_RES_HEIGHT_PX = 456;
			Window testWindow = new Window("Unit test window");

			// Set up context


			// Execute
			testWindow.Height = TEST_HEIGHT_PX;
			Assert.AreEqual(TEST_HEIGHT_PX, testWindow.Height);
			testWindow.Width = TEST_WIDTH_PX;
			Assert.AreEqual(TEST_WIDTH_PX, testWindow.Width);
			// Setting the window to fullscreen will fail if it isn't in focus, so this is commented out.
			//testWindow.IsFullscreen = true;
			//Assert.IsTrue(testWindow.IsFullscreen);
			testWindow.FullscreenState = WindowFullscreenState.NotFullscreen;
			Assert.AreEqual(WindowFullscreenState.NotFullscreen, testWindow.FullscreenState);
			testWindow.FullscreenState = WindowFullscreenState.BorderlessFullscreen;
			Assert.AreEqual(WindowFullscreenState.BorderlessFullscreen, testWindow.FullscreenState);
			testWindow.FullscreenState = WindowFullscreenState.NotFullscreen;
			Assert.AreEqual(WindowFullscreenState.NotFullscreen, testWindow.FullscreenState);
			testWindow.ShowCursor = false;
			Assert.IsFalse(testWindow.ShowCursor);
			testWindow.ShowCursor = true;
			Assert.IsTrue(testWindow.ShowCursor);
			
			testWindow.SetResolution(TEST_RES_WIDTH_PX, TEST_RES_HEIGHT_PX);
			Assert.AreEqual(TEST_RES_WIDTH_PX, testWindow.Width);
			Assert.AreEqual(TEST_RES_HEIGHT_PX, testWindow.Height);

			// Assert outcome
			testWindow.Close();
		}

		[TestMethod]
		[Timeout(10000)]
		public void TestConstructor() {
			// Define variables and constants
			const uint TEST_HEIGHT_PX = 123;
			const uint TEST_WIDTH_PX = 234;
			const WindowFullscreenState TEST_FULLSCREEN_STATE = WindowFullscreenState.NotFullscreen;

			// Set up context


			// Execute
			Window testWindow = new Window(
				"Unit test window",
				TEST_WIDTH_PX,
				TEST_HEIGHT_PX,
				TEST_FULLSCREEN_STATE,
				MultibufferLevel.Triple
			);

			// Assert outcome
			Assert.AreEqual(TEST_WIDTH_PX, testWindow.Width);
			Assert.AreEqual(TEST_HEIGHT_PX, testWindow.Height);
			Assert.AreEqual(TEST_FULLSCREEN_STATE, testWindow.FullscreenState);

			testWindow.Close();
		}

		[TestMethod]
		[Timeout(10000)]
		public void TestMultipleWindows() {
			// Define variables and constants
			

			// Set up context


			// Execute
			Window testWindowA = new Window("Unit test window A");
			Window testWindowB = new Window("Unit test window B");
			Window testWindowC = new Window("Unit test window C");

			// Assert outcome
			testWindowA.Close();
			testWindowB.Close();
			testWindowC.Close();
		}

		[TestMethod]
		public void SetTitleWithNullStringShouldThrowException() {
			// Define variables and constants
			Window testWindow = new Window("TitleTest");

			// Set up context


			// Execute


			// Assert outcome
			Assert.AreEqual("TitleTest", testWindow.Title);
			testWindow.Title = "TitleTest 2";
			Assert.AreEqual("TitleTest 2", testWindow.Title);

			try {
				testWindow.Title = null;
				Assert.Fail();
			}
			catch (ArgumentNullException) {
				
			}

			testWindow.Close();
		}
		#endregion
	}
}