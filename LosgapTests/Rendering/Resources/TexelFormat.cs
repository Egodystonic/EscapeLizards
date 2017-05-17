// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 19 11 2014 at 14:18 by Ben Bowen

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap.Rendering {
	[TestClass]
	public class TexelFormatTest {

		[TestInitialize]
		public void SetUp() { }


		#region Tests
		[TestMethod]
		public void TestNormalizedFormatsAreConsistent() {
			// Define variables and constants
			const float RED = 0.5f;
			const float GREEN = 0.333f;
			const float BLUE = 0.99f;
			const float ALPHA = 0f;
			const float DELTA = 0.01f;

			IEnumerable<ITexel> allTexels = new ITexel[] {
				new TexelFormat.RGBA16UNorm  (),
				new TexelFormat.RGBA16Norm 	 (),
				new TexelFormat.RGBA8UNorm 	 (),
				new TexelFormat.RGBA8Norm	 (),
				new TexelFormat.RG16UNorm	 (),
				new TexelFormat.RG16Norm	 (),
				new TexelFormat.RG8UNorm	 (),
				new TexelFormat.RG8Norm 	 (),
				new TexelFormat.UNorm16 	 (),
				new TexelFormat.Norm16		 (),
				new TexelFormat.UNorm8		 (),
				new TexelFormat.Norm8		 (),
			};

			// Set up context


			// Execute
			foreach (ITexel texel in allTexels) {
				dynamic dynTexel = texel;
				try {
					dynTexel.R = (texel.GetType().Name.Contains("U") ? RED : -RED);
				}
				catch (RuntimeBinderException) { }
				try {
					dynTexel.G = (texel.GetType().Name.Contains("U") ? GREEN : -GREEN);
				}
				catch (RuntimeBinderException) { }
				try {
					dynTexel.B = BLUE;
				}
				catch (RuntimeBinderException) { }
				try {
					dynTexel.A = ALPHA;
				}
				catch (RuntimeBinderException) { }
				try {
					dynTexel.Value = (texel.GetType().Name.Contains("U") ? RED : -RED);
				}
				catch (RuntimeBinderException) { }
			}

			// Assert outcome
			foreach (ITexel texel in allTexels) {
				dynamic dynTexel = texel;
				try {
					Assert.AreEqual((texel.GetType().Name.Contains("U") ? RED : -RED), dynTexel.R, DELTA);
				}
				catch (RuntimeBinderException) { }
				try {
					Assert.AreEqual((texel.GetType().Name.Contains("U") ? GREEN : -GREEN), dynTexel.G, DELTA);
				}
				catch (RuntimeBinderException) { }
				try {
					Assert.AreEqual(BLUE, dynTexel.B, DELTA);
				}
				catch (RuntimeBinderException) { }
				try {
					Assert.AreEqual(ALPHA, dynTexel.A, DELTA);
				}
				catch (RuntimeBinderException) { }
				try {
					Assert.AreEqual((texel.GetType().Name.Contains("U") ? RED : -RED), dynTexel.Value, DELTA);
				}
				catch (RuntimeBinderException) { }
			}
		}

		[TestMethod]
		public void TestDepthStencilFormat() {
			// Define variables and constants
			const float DEPTH_VALUE_A = 0.66f;
			const float DEPTH_VALUE_B = 1f;
			const float DEPTH_VALUE_C = 0f;
			const float DEPTH_VALUE_D = 0.155f;
			const byte STENCIL_VALUE = 141;
			const float DELTA = 0.001f;
			TexelFormat.DepthStencil texelA = new TexelFormat.DepthStencil();
			TexelFormat.DepthStencil texelB = new TexelFormat.DepthStencil();
			TexelFormat.DepthStencil texelC = new TexelFormat.DepthStencil();
			TexelFormat.DepthStencil texelD = new TexelFormat.DepthStencil();

			// Set up context


			// Execute
			texelA.Stencil = STENCIL_VALUE;
			texelA.Depth = DEPTH_VALUE_A;
			texelB.Depth = DEPTH_VALUE_B;
			texelC.Depth = DEPTH_VALUE_C;
			texelD.Depth = DEPTH_VALUE_D;

			// Assert outcome
			Assert.AreEqual(STENCIL_VALUE, texelA.Stencil);
			Assert.AreEqual(DEPTH_VALUE_A, texelA.Depth, DELTA);
			Assert.AreEqual(DEPTH_VALUE_B, texelB.Depth, DELTA);
			Assert.AreEqual(DEPTH_VALUE_C, texelC.Depth, DELTA);
			Assert.AreEqual(DEPTH_VALUE_D, texelD.Depth, DELTA);
		}

		[TestMethod]
		public void TestCorrectSizes() {
			// Define variables and constants
			IEnumerable<Type> formats = typeof(TexelFormat)
				.GetNestedTypes(BindingFlags.Public)
				.Where(type => type.GetInterfaces().Contains(typeof(ITexel)));

			// Set up context


			// Execute
			MethodInfo runtimeSizeof = typeof(UnsafeUtils).GetMethod("SizeOf");
			foreach (Type format in formats) {
				Assert.AreEqual(
					(uint) (int) runtimeSizeof.MakeGenericMethod(format).Invoke(null, new object[0]), 
					format.GetCustomAttribute<TexelFormatMetadataAttribute>().FormatSizeBytes
				);
			}

			// Assert outcome

		}
		#endregion
	}
}