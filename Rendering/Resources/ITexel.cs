// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 03 11 2014 at 10:52 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// Represents a valid texel format (i.e. a struct that comprises of colour components).
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces",
		Justification = "The empty interface provides a point to add extension methods to, and a way to constrain generic types.")]
	public interface ITexel { }

	/// <summary>
	/// A static class containing a number of embedded <see cref="ITexel"/> types that can be used when creating and manipulating
	/// <see cref="ITexture">texture</see>s.
	/// </summary>
	[CompilerGenerated] // Total lie, but stops lots of things from crying about the way this is written (which in fairness is 'horribly')
	public static class TexelFormat {
		public static readonly Dictionary<Type, TexelFormatMetadataAttribute> AllFormats = ReflectionUtils.GetChildTypes(typeof(ITexel), false).ToDictionary(type => type, type => type.GetCustomAttribute<TexelFormatMetadataAttribute>());
		internal static readonly ResourceFormat RTV_FORMAT_CODE = (ResourceFormat) typeof(RenderTarget).GetCustomAttribute<TexelFormatMetadataAttribute>().ResourceFormatIndex;
		internal static readonly ResourceFormat DSV_FORMAT_CODE = (ResourceFormat) typeof(DepthStencil).GetCustomAttribute<TexelFormatMetadataAttribute>().ResourceFormatIndex;

#pragma warning disable 1591
		[TexelFormatMetadata(0, 1)]
		public struct Unknown : ITexel {
			
		}

		#region RTV / DSV
		[TexelFormatMetadata(28, 4), StructLayout(LayoutKind.Explicit, Pack = (int) InteropUtils.StructPacking.Safe)]
		public struct RenderTarget : ITexel {
			[FieldOffset(0)]
			private byte r;
			[FieldOffset(1)]
			private byte g;
			[FieldOffset(2)]
			private byte b;
			[FieldOffset(3)]
			private byte a;

			public float R {
				get {
					return NormalizedByteToFloat(r);
				}
				set {
					r = FloatToNormalizedByte(value);
				}
			}
			public float G {
				get {
					return NormalizedByteToFloat(g);
				}
				set {
					g = FloatToNormalizedByte(value);
				}
			}
			public float B {
				get {
					return NormalizedByteToFloat(b);
				}
				set {
					b = FloatToNormalizedByte(value);
				}
			}
			public float A {
				get {
					return NormalizedByteToFloat(a);
				}
				set {
					a = FloatToNormalizedByte(value);
				}
			}
		}

		[TexelFormatMetadata(45, 4), StructLayout(LayoutKind.Explicit, Pack = (int) InteropUtils.StructPacking.Safe)]
		public unsafe struct DepthStencil : ITexel {
			private const float DEPTH_MAX_FLOAT = (float) 0xFFFFFFU;

			[FieldOffset(0)]
			private fixed byte depth[3];
			[FieldOffset(3)]
			public byte Stencil;

			public float Depth {
				get {
					if (BitConverter.IsLittleEndian) {
						fixed (byte* fixedDepth = depth) {
							float value = (float) (uint) ((fixedDepth[2] << 16) | (fixedDepth[1] << 8) | (fixedDepth[0]));
							return value / DEPTH_MAX_FLOAT;
						}
					}
					else {
						fixed (byte* fixedDepth = depth) {
							float value = (float) (uint) ((fixedDepth[0] << 16) | (fixedDepth[1] << 8) | (fixedDepth[2]));
							return value / DEPTH_MAX_FLOAT;
						}
					}
				}
				set {
					Assure.BetweenOrEqualTo(value, 0f, 1f, "Depth value must be between 0f and 1f.");
					uint normValue = (uint) (DEPTH_MAX_FLOAT * value);
					if (BitConverter.IsLittleEndian) {
						fixed (byte* fixedDepth = depth) {
							fixedDepth[0] = (byte) (normValue & 0xFF);
							fixedDepth[1] = (byte) ((normValue & 0xFF00) >> 8);
							fixedDepth[2] = (byte) ((normValue & 0xFF0000) >> 16);
						}
					}
					else {
						fixed (byte* fixedDepth = depth) {
							fixedDepth[2] = (byte) (normValue & 0xFF);
							fixedDepth[1] = (byte) ((normValue & 0xFF00) >> 8);
							fixedDepth[0] = (byte) ((normValue & 0xFF0000) >> 16);
						}
					}
				}
			}
		}
		#endregion

		#region RGBA32
		[TexelFormatMetadata(1, 16), StructLayout(LayoutKind.Explicit, Pack = (int) InteropUtils.StructPacking.Safe)]
		public struct RGBA32Void : ITexel {
			[FieldOffset(0)]
			public uint R;
			[FieldOffset(4)]
			public uint G;
			[FieldOffset(8)]
			public uint B;
			[FieldOffset(12)]
			public uint A;
		}

		[TexelFormatMetadata(2, 16), StructLayout(LayoutKind.Explicit, Pack = (int) InteropUtils.StructPacking.Safe)]
		public struct RGBA32Float : ITexel {
			[FieldOffset(0)]
			public float R;
			[FieldOffset(4)]
			public float G;
			[FieldOffset(8)]
			public float B;
			[FieldOffset(12)]
			public float A;
		}

		[TexelFormatMetadata(3, 16), StructLayout(LayoutKind.Explicit, Pack = (int) InteropUtils.StructPacking.Safe)]
		public struct RGBA32UInt : ITexel {
			[FieldOffset(0)]
			public uint R;
			[FieldOffset(4)]
			public uint G;
			[FieldOffset(8)]
			public uint B;
			[FieldOffset(12)]
			public uint A;
		}

		[TexelFormatMetadata(4, 16), StructLayout(LayoutKind.Explicit, Pack = (int) InteropUtils.StructPacking.Safe)]
		public struct RGBA32Int : ITexel {
			[FieldOffset(0)]
			public int R;
			[FieldOffset(4)]
			public int G;
			[FieldOffset(8)]
			public int B;
			[FieldOffset(12)]
			public int A;
		}
		#endregion

		#region RGB32
		[TexelFormatMetadata(5, 12), StructLayout(LayoutKind.Explicit, Pack = (int) InteropUtils.StructPacking.Safe)]
		public struct RGB32Void : ITexel {
			[FieldOffset(0)]
			public uint R;
			[FieldOffset(4)]
			public uint G;
			[FieldOffset(8)]
			public uint B;
		}

		[TexelFormatMetadata(6, 12), StructLayout(LayoutKind.Explicit, Pack = (int) InteropUtils.StructPacking.Safe)]
		public struct RGB32Float : ITexel {
			[FieldOffset(0)]
			public float R;
			[FieldOffset(4)]
			public float G;
			[FieldOffset(8)]
			public float B;
		}

		[TexelFormatMetadata(7, 12), StructLayout(LayoutKind.Explicit, Pack = (int) InteropUtils.StructPacking.Safe)]
		public struct RGB32UInt : ITexel {
			[FieldOffset(0)]
			public uint R;
			[FieldOffset(4)]
			public uint G;
			[FieldOffset(8)]
			public uint B;
		}

		[TexelFormatMetadata(8, 12), StructLayout(LayoutKind.Explicit, Pack = (int) InteropUtils.StructPacking.Safe)]
		public struct RGB32Int : ITexel {
			[FieldOffset(0)]
			public int R;
			[FieldOffset(4)]
			public int G;
			[FieldOffset(8)]
			public int B;
		}
		#endregion

		#region RGBA16
		[TexelFormatMetadata(9, 8), StructLayout(LayoutKind.Explicit, Pack = (int) InteropUtils.StructPacking.Safe)]
		public struct RGBA16Void : ITexel {
			[FieldOffset(0)]
			public ushort R;
			[FieldOffset(2)]
			public ushort G;
			[FieldOffset(4)]
			public ushort B;
			[FieldOffset(6)]
			public ushort A;
		}

		[TexelFormatMetadata(11, 8), StructLayout(LayoutKind.Explicit, Pack = (int) InteropUtils.StructPacking.Safe)]
		public struct RGBA16UNorm : ITexel {
			[FieldOffset(0)]
			private ushort r;
			[FieldOffset(2)]
			private ushort g;
			[FieldOffset(4)]
			private ushort b;
			[FieldOffset(6)]
			private ushort a;

			public float R {
				get {
					return NormalizedUShortToFloat(r);
				}
				set {
					r = FloatToNormalizedUShort(value);
				}
			}
			public float G {
				get {
					return NormalizedUShortToFloat(g);
				}
				set {
					g = FloatToNormalizedUShort(value);
				}
			}
			public float B {
				get {
					return NormalizedUShortToFloat(b);
				}
				set {
					b = FloatToNormalizedUShort(value);
				}
			}
			public float A {
				get {
					return NormalizedUShortToFloat(a);
				}
				set {
					a = FloatToNormalizedUShort(value);
				}
			}
		}

		[TexelFormatMetadata(12, 8), StructLayout(LayoutKind.Explicit, Pack = (int) InteropUtils.StructPacking.Safe)]
		public struct RGBA16UInt : ITexel {
			[FieldOffset(0)]
			public ushort R;
			[FieldOffset(2)]
			public ushort G;
			[FieldOffset(4)]
			public ushort B;
			[FieldOffset(6)]
			public ushort A;
		}

		[TexelFormatMetadata(13, 8), StructLayout(LayoutKind.Explicit, Pack = (int) InteropUtils.StructPacking.Safe)]
		public struct RGBA16Norm : ITexel {
			[FieldOffset(0)]
			private short r;
			[FieldOffset(2)]
			private short g;
			[FieldOffset(4)]
			private short b;
			[FieldOffset(6)]
			private short a;

			public float R {
				get {
					return NormalizedShortToFloat(r);
				}
				set {
					r = FloatToNormalizedShort(value);
				}
			}
			public float G {
				get {
					return NormalizedShortToFloat(g);
				}
				set {
					g = FloatToNormalizedShort(value);
				}
			}
			public float B {
				get {
					return NormalizedShortToFloat(b);
				}
				set {
					b = FloatToNormalizedShort(value);
				}
			}
			public float A {
				get {
					return NormalizedShortToFloat(a);
				}
				set {
					a = FloatToNormalizedShort(value);
				}
			}
		}

		[TexelFormatMetadata(14, 8), StructLayout(LayoutKind.Explicit, Pack = (int) InteropUtils.StructPacking.Safe)]
		public struct RGBA16Int : ITexel {
			[FieldOffset(0)]
			public short R;
			[FieldOffset(2)]
			public short G;
			[FieldOffset(4)]
			public short B;
			[FieldOffset(6)]
			public short A;
		}
		#endregion

		#region RG32
		[TexelFormatMetadata(15, 8), StructLayout(LayoutKind.Explicit, Pack = (int) InteropUtils.StructPacking.Safe)]
		public struct RG32Void : ITexel {
			[FieldOffset(0)]
			public uint R;
			[FieldOffset(4)]
			public uint G;
		}

		[TexelFormatMetadata(16, 8), StructLayout(LayoutKind.Explicit, Pack = (int) InteropUtils.StructPacking.Safe)]
		public struct RG32Float : ITexel {
			[FieldOffset(0)]
			public float R;
			[FieldOffset(4)]
			public float G;
		}

		[TexelFormatMetadata(17, 8), StructLayout(LayoutKind.Explicit, Pack = (int) InteropUtils.StructPacking.Safe)]
		public struct RG32UInt : ITexel {
			[FieldOffset(0)]
			public uint R;
			[FieldOffset(4)]
			public uint G;
		}

		[TexelFormatMetadata(18, 8), StructLayout(LayoutKind.Explicit, Pack = (int) InteropUtils.StructPacking.Safe)]
		public struct RG32Int : ITexel {
			[FieldOffset(0)]
			public int R;
			[FieldOffset(4)]
			public int G;
		}
		#endregion

		#region RGBA8
		[TexelFormatMetadata(27, 4), StructLayout(LayoutKind.Explicit, Pack = (int) InteropUtils.StructPacking.Safe)]
		public struct RGBA8Void : ITexel {
			[FieldOffset(0)]
			public byte R;
			[FieldOffset(1)]
			public byte G;
			[FieldOffset(2)]
			public byte B;
			[FieldOffset(3)]
			public byte A;
		}

		[TexelFormatMetadata(28, 4), StructLayout(LayoutKind.Explicit, Pack = (int) InteropUtils.StructPacking.Safe)]
		public struct RGBA8UNorm : ITexel {
			[FieldOffset(0)]
			private byte r;
			[FieldOffset(1)]
			private byte g;
			[FieldOffset(2)]
			private byte b;
			[FieldOffset(3)]
			private byte a;

			public float R {
				get {
					return NormalizedByteToFloat(r);
				}
				set {
					r = FloatToNormalizedByte(value);
				}
			}
			public float G {
				get {
					return NormalizedByteToFloat(g);
				}
				set {
					g = FloatToNormalizedByte(value);
				}
			}
			public float B {
				get {
					return NormalizedByteToFloat(b);
				}
				set {
					b = FloatToNormalizedByte(value);
				}
			}
			public float A {
				get {
					return NormalizedByteToFloat(a);
				}
				set {
					a = FloatToNormalizedByte(value);
				}
			}
		}

		[TexelFormatMetadata(29, 4), StructLayout(LayoutKind.Explicit, Pack = (int) InteropUtils.StructPacking.Safe)]
		public struct RGBA8UNormSRGB : ITexel {
			[FieldOffset(0)]
			private byte r;
			[FieldOffset(1)]
			private byte g;
			[FieldOffset(2)]
			private byte b;
			[FieldOffset(3)]
			private byte a;

			public float R {
				get {
					return NormalizedByteToFloat(r);
				}
				set {
					r = FloatToNormalizedByte(value);
				}
			}
			public float G {
				get {
					return NormalizedByteToFloat(g);
				}
				set {
					g = FloatToNormalizedByte(value);
				}
			}
			public float B {
				get {
					return NormalizedByteToFloat(b);
				}
				set {
					b = FloatToNormalizedByte(value);
				}
			}
			public float A {
				get {
					return NormalizedByteToFloat(a);
				}
				set {
					a = FloatToNormalizedByte(value);
				}
			}
		}

		[TexelFormatMetadata(87, 4), StructLayout(LayoutKind.Explicit, Pack = (int) InteropUtils.StructPacking.Safe)]
		public struct BGRA8UNorm : ITexel {
			[FieldOffset(0)]
			private byte b;
			[FieldOffset(1)]
			private byte g;
			[FieldOffset(2)]
			private byte r;
			[FieldOffset(3)]
			private byte a;

			public float R {
				get {
					return NormalizedByteToFloat(r);
				}
				set {
					r = FloatToNormalizedByte(value);
				}
			}
			public float G {
				get {
					return NormalizedByteToFloat(g);
				}
				set {
					g = FloatToNormalizedByte(value);
				}
			}
			public float B {
				get {
					return NormalizedByteToFloat(b);
				}
				set {
					b = FloatToNormalizedByte(value);
				}
			}
			public float A {
				get {
					return NormalizedByteToFloat(a);
				}
				set {
					a = FloatToNormalizedByte(value);
				}
			}
		}

		[TexelFormatMetadata(85, 2), StructLayout(LayoutKind.Explicit, Pack = (int) InteropUtils.StructPacking.Safe)]
		public struct B5G6R5_UNORM : ITexel {
			[FieldOffset(0)]
			public ushort Data;
		}

		[TexelFormatMetadata(91, 4), StructLayout(LayoutKind.Explicit, Pack = (int) InteropUtils.StructPacking.Safe)]
		public struct BGRA8UNormSRGB : ITexel {
			[FieldOffset(0)]
			private byte b;
			[FieldOffset(1)]
			private byte g;
			[FieldOffset(2)]
			private byte r;
			[FieldOffset(3)]
			private byte a;

			public float R {
				get {
					return NormalizedByteToFloat(r);
				}
				set {
					r = FloatToNormalizedByte(value);
				}
			}
			public float G {
				get {
					return NormalizedByteToFloat(g);
				}
				set {
					g = FloatToNormalizedByte(value);
				}
			}
			public float B {
				get {
					return NormalizedByteToFloat(b);
				}
				set {
					b = FloatToNormalizedByte(value);
				}
			}
			public float A {
				get {
					return NormalizedByteToFloat(a);
				}
				set {
					a = FloatToNormalizedByte(value);
				}
			}
		}

		[TexelFormatMetadata(30, 4), StructLayout(LayoutKind.Explicit, Pack = (int) InteropUtils.StructPacking.Safe)]
		public struct RGBA8UInt : ITexel {
			[FieldOffset(0)]
			public byte R;
			[FieldOffset(1)]
			public byte G;
			[FieldOffset(2)]
			public byte B;
			[FieldOffset(3)]
			public byte A;
		}

		[TexelFormatMetadata(31, 4), StructLayout(LayoutKind.Explicit, Pack = (int) InteropUtils.StructPacking.Safe)]
		public struct RGBA8Norm : ITexel {
			[FieldOffset(0)]
			private sbyte r;
			[FieldOffset(1)]
			private sbyte g;
			[FieldOffset(2)]
			private sbyte b;
			[FieldOffset(3)]
			private sbyte a;

			public float R {
				get {
					return NormalizedSByteToFloat(r);
				}
				set {
					r = FloatToNormalizedSByte(value);
				}
			}
			public float G {
				get {
					return NormalizedSByteToFloat(g);
				}
				set {
					g = FloatToNormalizedSByte(value);
				}
			}
			public float B {
				get {
					return NormalizedSByteToFloat(b);
				}
				set {
					b = FloatToNormalizedSByte(value);
				}
			}
			public float A {
				get {
					return NormalizedSByteToFloat(a);
				}
				set {
					a = FloatToNormalizedSByte(value);
				}
			}
		}

		[TexelFormatMetadata(32, 4), StructLayout(LayoutKind.Explicit, Pack = (int) InteropUtils.StructPacking.Safe)]
		public struct RGBA8Int : ITexel {
			[FieldOffset(0)]
			public sbyte R;
			[FieldOffset(1)]
			public sbyte G;
			[FieldOffset(2)]
			public sbyte B;
			[FieldOffset(3)]
			public sbyte A;
		}
		#endregion

		#region RG16
		[TexelFormatMetadata(33, 4), StructLayout(LayoutKind.Explicit, Pack = (int) InteropUtils.StructPacking.Safe)]
		public struct RG16Void : ITexel {
			[FieldOffset(0)]
			public ushort R;
			[FieldOffset(2)]
			public ushort G;
		}

		[TexelFormatMetadata(35, 4), StructLayout(LayoutKind.Explicit, Pack = (int) InteropUtils.StructPacking.Safe)]
		public struct RG16UNorm : ITexel {
			[FieldOffset(0)]
			private ushort r;
			[FieldOffset(2)]
			private ushort g;

			public float R {
				get {
					return NormalizedUShortToFloat(r);
				}
				set {
					r = FloatToNormalizedUShort(value);
				}
			}
			public float G {
				get {
					return NormalizedUShortToFloat(g);
				}
				set {
					g = FloatToNormalizedUShort(value);
				}
			}
		}

		[TexelFormatMetadata(36, 4), StructLayout(LayoutKind.Explicit, Pack = (int) InteropUtils.StructPacking.Safe)]
		public struct RG16UInt : ITexel {
			[FieldOffset(0)]
			public ushort R;
			[FieldOffset(2)]
			public ushort G;
		}

		[TexelFormatMetadata(37, 4), StructLayout(LayoutKind.Explicit, Pack = (int) InteropUtils.StructPacking.Safe)]
		public struct RG16Norm : ITexel {
			[FieldOffset(0)]
			private short r;
			[FieldOffset(2)]
			private short g;

			public float R {
				get {
					return NormalizedShortToFloat(r);
				}
				set {
					r = FloatToNormalizedShort(value);
				}
			}
			public float G {
				get {
					return NormalizedShortToFloat(g);
				}
				set {
					g = FloatToNormalizedShort(value);
				}
			}
		}

		[TexelFormatMetadata(38, 4), StructLayout(LayoutKind.Explicit, Pack = (int) InteropUtils.StructPacking.Safe)]
		public struct RG16Int : ITexel {
			[FieldOffset(0)]
			public short R;
			[FieldOffset(2)]
			public short G;
		}
		#endregion

		#region 32
		[TexelFormatMetadata(39, 4), StructLayout(LayoutKind.Explicit, Pack = (int) InteropUtils.StructPacking.Safe)]
		public struct Void32 : ITexel {
			[FieldOffset(0)]
			public uint Value;
		}

		[TexelFormatMetadata(41, 4), StructLayout(LayoutKind.Explicit, Pack = (int) InteropUtils.StructPacking.Safe)]
		public struct Float32 : ITexel {
			[FieldOffset(0)]
			public float Value;
		}

		[TexelFormatMetadata(42, 4), StructLayout(LayoutKind.Explicit, Pack = (int) InteropUtils.StructPacking.Safe)]
		public struct UInt32 : ITexel {
			[FieldOffset(0)]
			public uint Value;
		}

		[TexelFormatMetadata(43, 4), StructLayout(LayoutKind.Explicit, Pack = (int) InteropUtils.StructPacking.Safe)]
		public struct Int32 : ITexel {
			[FieldOffset(0)]
			public int Value;
		}
		#endregion

		[TexelFormatMetadata(45, 4), StructLayout(LayoutKind.Explicit, Pack = (int) InteropUtils.StructPacking.Safe)]
		public unsafe struct UNorm24UInt8 : ITexel {
			private const float DEPTH_MAX_FLOAT = (float) 0xFFFFFFU;

			[FieldOffset(0)]
			private fixed byte depth[3];
			[FieldOffset(3)]
			public byte Stencil;

			public float Depth {
				get {
					if (BitConverter.IsLittleEndian) {
						fixed (byte* fixedDepth = depth) {
							float value = (float) (uint) ((fixedDepth[2] << 16) | (fixedDepth[1] << 8) | (fixedDepth[0]));
							return value / DEPTH_MAX_FLOAT;
						}
					}
					else {
						fixed (byte* fixedDepth = depth) {
							float value = (float) (uint) ((fixedDepth[0] << 16) | (fixedDepth[1] << 8) | (fixedDepth[2]));
							return value / DEPTH_MAX_FLOAT;
						}
					}
				}
				set {
					Assure.BetweenOrEqualTo(value, 0f, 1f, "Depth value must be between 0f and 1f.");
					uint normValue = (uint) (DEPTH_MAX_FLOAT * value);
					if (BitConverter.IsLittleEndian) {
						fixed (byte* fixedDepth = depth) {
							fixedDepth[0] = (byte) (normValue & 0xFF);
							fixedDepth[1] = (byte) ((normValue & 0xFF00) >> 8);
							fixedDepth[2] = (byte) ((normValue & 0xFF0000) >> 16);
						}
					}
					else {
						fixed (byte* fixedDepth = depth) {
							fixedDepth[2] = (byte) (normValue & 0xFF);
							fixedDepth[1] = (byte) ((normValue & 0xFF00) >> 8);
							fixedDepth[0] = (byte) ((normValue & 0xFF0000) >> 16);
						}
					}
				}
			}
		}

		[TexelFormatMetadata(44, 4), StructLayout(LayoutKind.Explicit, Pack = (int) InteropUtils.StructPacking.Safe)]
		public unsafe struct R24G8Typeless : ITexel {
			[FieldOffset(0)]
			public fixed byte R[3];
			[FieldOffset(3)]
			public byte G;
		}

		[TexelFormatMetadata(46, 4), StructLayout(LayoutKind.Explicit, Pack = (int) InteropUtils.StructPacking.Safe)]
		public unsafe struct R24UnormX8Typeless : ITexel {
			[FieldOffset(0)]
			public fixed byte R[3];
			[FieldOffset(3)]
			public byte X;
		}

		#region RG8
		[TexelFormatMetadata(48, 2), StructLayout(LayoutKind.Explicit, Pack = (int) InteropUtils.StructPacking.Safe)]
		public struct RG8Void : ITexel {
			[FieldOffset(0)]
			public byte R;
			[FieldOffset(1)]
			public byte G;
		}

		[TexelFormatMetadata(49, 2), StructLayout(LayoutKind.Explicit, Pack = (int) InteropUtils.StructPacking.Safe)]
		public struct RG8UNorm : ITexel {
			[FieldOffset(0)]
			private byte r;
			[FieldOffset(1)]
			private byte g;

			public float R {
				get {
					return NormalizedByteToFloat(r);
				}
				set {
					r = FloatToNormalizedByte(value);
				}
			}
			public float G {
				get {
					return NormalizedByteToFloat(g);
				}
				set {
					g = FloatToNormalizedByte(value);
				}
			}
		}

		[TexelFormatMetadata(50, 2), StructLayout(LayoutKind.Explicit, Pack = (int) InteropUtils.StructPacking.Safe)]
		public struct RG8UInt : ITexel {
			[FieldOffset(0)]
			public byte R;
			[FieldOffset(1)]
			public byte G;
		}

		[TexelFormatMetadata(51, 2), StructLayout(LayoutKind.Explicit, Pack = (int) InteropUtils.StructPacking.Safe)]
		public struct RG8Norm : ITexel {
			[FieldOffset(0)]
			private sbyte r;
			[FieldOffset(1)]
			private sbyte g;

			public float R {
				get {
					return NormalizedSByteToFloat(r);
				}
				set {
					r = FloatToNormalizedSByte(value);
				}
			}
			public float G {
				get {
					return NormalizedSByteToFloat(g);
				}
				set {
					g = FloatToNormalizedSByte(value);
				}
			}
		}

		[TexelFormatMetadata(52, 2), StructLayout(LayoutKind.Explicit, Pack = (int) InteropUtils.StructPacking.Safe)]
		public struct RG8Int : ITexel {
			[FieldOffset(0)]
			public sbyte R;
			[FieldOffset(1)]
			public sbyte G;
		}
		#endregion

		#region 16
		[TexelFormatMetadata(53, 2), StructLayout(LayoutKind.Explicit, Pack = (int) InteropUtils.StructPacking.Safe)]
		public struct Void16 : ITexel {
			[FieldOffset(0)]
			public ushort Value;
		}

		[TexelFormatMetadata(56, 2), StructLayout(LayoutKind.Explicit, Pack = (int) InteropUtils.StructPacking.Safe)]
		public struct UNorm16 : ITexel {
			[FieldOffset(0)]
			private ushort value;

			public float Value {
				get {
					return NormalizedUShortToFloat(value);
				}
				set {
					this.value = FloatToNormalizedUShort(value);
				}
			}
		}

		[TexelFormatMetadata(57, 2), StructLayout(LayoutKind.Explicit, Pack = (int) InteropUtils.StructPacking.Safe)]
		public struct UInt16 : ITexel {
			[FieldOffset(0)]
			public ushort Value;
		}

		[TexelFormatMetadata(58, 2), StructLayout(LayoutKind.Explicit, Pack = (int) InteropUtils.StructPacking.Safe)]
		public struct Norm16 : ITexel {
			[FieldOffset(0)]
			private short value;

			public float Value {
				get {
					return NormalizedShortToFloat(value);
				}
				set {
					this.value = FloatToNormalizedShort(value);
				}
			}
		}

		[TexelFormatMetadata(59, 2), StructLayout(LayoutKind.Explicit, Pack = (int) InteropUtils.StructPacking.Safe)]
		public struct Int16 : ITexel {
			[FieldOffset(0)]
			public short Value;
		}
		#endregion

		#region 8
		[TexelFormatMetadata(60, 1), StructLayout(LayoutKind.Explicit, Pack = (int) InteropUtils.StructPacking.Safe)]
		public struct Void8 : ITexel {
			[FieldOffset(0)]
			public byte Value;
		}

		[TexelFormatMetadata(61, 1), StructLayout(LayoutKind.Explicit, Pack = (int) InteropUtils.StructPacking.Safe)]
		public struct UNorm8 : ITexel {
			[FieldOffset(0)]
			private byte value;

			public float Value {
				get {
					return NormalizedByteToFloat(value);
				}
				set {
					this.value = FloatToNormalizedByte(value);
				}
			}
		}

		[TexelFormatMetadata(62, 1), StructLayout(LayoutKind.Explicit, Pack = (int) InteropUtils.StructPacking.Safe)]
		public struct UInt8 : ITexel {
			[FieldOffset(0)]
			public byte Value;
		}

		[TexelFormatMetadata(63, 1), StructLayout(LayoutKind.Explicit, Pack = (int) InteropUtils.StructPacking.Safe)]
		public struct Norm8 : ITexel {
			[FieldOffset(0)]
			private sbyte value;

			public float Value {
				get {
					return NormalizedSByteToFloat(value);
				}
				set {
					this.value = FloatToNormalizedSByte(value);
				}
			}
		}

		[TexelFormatMetadata(64, 1), StructLayout(LayoutKind.Explicit, Pack = (int) InteropUtils.StructPacking.Safe)]
		public struct Int8 : ITexel {
			[FieldOffset(0)]
			public sbyte Value;
		}
		#endregion

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static float NormalizedByteToFloat(byte normalizedValue) {
			return (float) normalizedValue / (float) Byte.MaxValue;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static float NormalizedSByteToFloat(sbyte normalizedValue) {
			return normalizedValue > 0
				? (float) normalizedValue / (float) SByte.MaxValue
				: (float) Math.Abs(normalizedValue) / (float) SByte.MinValue;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static float NormalizedShortToFloat(short normalizedValue) {
			return normalizedValue > 0
				? (float) normalizedValue / (float) System.Int16.MaxValue
				: (float) Math.Abs(normalizedValue) / (float) System.Int16.MinValue;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static float NormalizedUShortToFloat(ushort normalizedValue) {
			return (float) normalizedValue / (float) System.UInt16.MaxValue;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static byte FloatToNormalizedByte(float value) {
			Assure.BetweenOrEqualTo(value, 0f, 1f, "Normalized float value should be between 0f and 1f!");
			return (byte) (value * (float) Byte.MaxValue);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static sbyte FloatToNormalizedSByte(float value) {
			Assure.BetweenOrEqualTo(value, -1f, 1f, "Normalized float value should be between -1f and 1f!");
			return value > 0f
				? (sbyte) (value * (float) SByte.MaxValue)
				: (sbyte) (Math.Abs(value) * (float) SByte.MinValue);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static short FloatToNormalizedShort(float value) {
			Assure.BetweenOrEqualTo(value, -1f, 1f, "Normalized float value should be between -1f and 1f!");
			return value > 0f
				? (short) (value * (float) System.Int16.MaxValue)
				: (short) (Math.Abs(value) * (float) System.Int16.MinValue);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static ushort FloatToNormalizedUShort(float value) {
			Assure.BetweenOrEqualTo(value, 0f, 1f, "Normalized float value should be between 0f and 1f!");
			return (ushort) (value * (float) System.UInt16.MaxValue);
		}
#pragma warning restore 1591
	}


	/// <summary>
	/// Extension class providing additional methods for <see cref="ITexel"/> texel structs.
	/// </summary>
	public static class TexelFormatExtensions {
		internal static int GetResourceFormatIndex(this ITexel @this) {
			Assure.NotNull(@this);
			Assure.True(@this.GetType().HasCustomAttribute<TexelFormatMetadataAttribute>());
			return @this.GetType().GetCustomAttribute<TexelFormatMetadataAttribute>().ResourceFormatIndex;
		}

		/// <summary>
		/// Gets the size of a single texel in the given <see cref="ITexel">texel format</see>.
		/// </summary>
		/// <param name="this">The extended ITexel.</param>
		/// <returns>Returns a <see cref="ByteSize"/> with the correct value.</returns>
		public static ByteSize GetTexelSize(this ITexel @this) {
			if (@this == null) throw new ArgumentNullException("this", "GetTexelSize called on a null ITexel.");
			Assure.True(@this.GetType().HasCustomAttribute<TexelFormatMetadataAttribute>());
			return @this.GetType().GetCustomAttribute<TexelFormatMetadataAttribute>().FormatSizeBytes;
		}
	}
}