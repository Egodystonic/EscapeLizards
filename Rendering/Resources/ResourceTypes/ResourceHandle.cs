// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 20 10 2014 at 15:59 by Ben Bowen

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// An internal pointer that uniquely identifies a single <see cref="IResource">resource</see> (or its parent).
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = (int) InteropUtils.StructPacking.Safe)]
	public struct ResourceHandle : IEquatable<ResourceHandle> {
		/// <summary>
		/// The equivalent to a <c>null</c> ResourceHandle.
		/// </summary>
		public static readonly ResourceHandle NULL = new ResourceHandle();
		private readonly IntPtr resourcePtr;

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(ResourceHandle other) {
			return resourcePtr == other.resourcePtr;
		}

		/// <summary>
		/// Indicates whether this instance and a specified object are equal.
		/// </summary>
		/// <returns>
		/// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false. 
		/// </returns>
		/// <param name="obj">The object to compare with the current instance. </param><filterpriority>2</filterpriority>
		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) {
				return false;
			}
			return obj is ResourceHandle && Equals((ResourceHandle) obj);
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>
		/// A 32-bit signed integer that is the hash code for this instance.
		/// </returns>
		/// <filterpriority>2</filterpriority>
		public override int GetHashCode() {
			return resourcePtr.GetHashCode();
		}

		/// <summary>
		/// Returns whether the two handles point to the same resource.
		/// </summary>
		/// <param name="left">The first operand.</param>
		/// <param name="right">The second operand.</param>
		/// <returns>True if <paramref name="left"/> and <paramref name="right"/> are a handle to the same resource.</returns>
		public static bool operator ==(ResourceHandle left, ResourceHandle right) {
			return left.Equals(right);
		}

		/// <summary>
		/// Returns whether the two handles point to different resources.
		/// </summary>
		/// <param name="left">The first operand.</param>
		/// <param name="right">The second operand.</param>
		/// <returns>True if <paramref name="left"/> and <paramref name="right"/> are a handle to different resources.</returns>
		public static bool operator !=(ResourceHandle left, ResourceHandle right) {
			return !left.Equals(right);
		}

		/// <summary>
		/// Returns the unique reference handle for this resource.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> containing the numeric pointer handle to this resource.
		/// </returns>
		public override string ToString() {
			return "0x" + resourcePtr.ToString("X");
		}

		/// <summary>
		/// Converts a resource handle to an <see cref="IntPtr"/>. The pointer returned will be a pointer to the actual resource referenced
		/// by the given <paramref name="operand"/>.
		/// </summary>
		/// <param name="operand">The resource handle to convert.</param>
		/// <returns>A pointer to the actual resource.</returns>
		public static explicit operator IntPtr(ResourceHandle operand) {
			return operand.resourcePtr;
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = (int) InteropUtils.StructPacking.Safe)]
	internal struct BufferResourceHandle : IEquatable<BufferResourceHandle> {
		public static readonly BufferResourceHandle NULL = new BufferResourceHandle();
		private readonly IntPtr resourcePtr;

		public bool Equals(BufferResourceHandle other) {
			return resourcePtr == other.resourcePtr;
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) {
				return false;
			}
			return obj is BufferResourceHandle && Equals((BufferResourceHandle) obj);
		}

		public override int GetHashCode() {
			return resourcePtr.GetHashCode();
		}

		public static bool operator ==(BufferResourceHandle left, BufferResourceHandle right) {
			return left.Equals(right);
		}

		public static bool operator !=(BufferResourceHandle left, BufferResourceHandle right) {
			return !left.Equals(right);
		}

		public static unsafe implicit operator ResourceHandle(BufferResourceHandle operand) {
			return *((ResourceHandle*) (&operand));
		}
		public static unsafe explicit operator BufferResourceHandle(ResourceHandle operand) {
			return *((BufferResourceHandle*) (&operand));
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = (int) InteropUtils.StructPacking.Safe)]
	internal struct Texture1DResourceHandle : IEquatable<Texture1DResourceHandle> {
		public static readonly Texture1DResourceHandle NULL = new Texture1DResourceHandle();
		private readonly IntPtr resourcePtr;

		public bool Equals(Texture1DResourceHandle other) {
			return resourcePtr == other.resourcePtr;
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) {
				return false;
			}
			return obj is Texture1DResourceHandle && Equals((Texture1DResourceHandle) obj);
		}

		public override int GetHashCode() {
			return resourcePtr.GetHashCode();
		}

		public static bool operator ==(Texture1DResourceHandle left, Texture1DResourceHandle right) {
			return left.Equals(right);
		}

		public static bool operator !=(Texture1DResourceHandle left, Texture1DResourceHandle right) {
			return !left.Equals(right);
		}

		public static unsafe implicit operator ResourceHandle(Texture1DResourceHandle operand) {
			return *((ResourceHandle*) (&operand));
		}
		public static unsafe explicit operator Texture1DResourceHandle(ResourceHandle operand) {
			return *((Texture1DResourceHandle*) (&operand));
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = (int) InteropUtils.StructPacking.Safe)]
	internal struct Texture2DResourceHandle : IEquatable<Texture2DResourceHandle> {
		public static readonly Texture2DResourceHandle NULL = new Texture2DResourceHandle();
		private readonly IntPtr resourcePtr;

		public bool Equals(Texture2DResourceHandle other) {
			return resourcePtr == other.resourcePtr;
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) {
				return false;
			}
			return obj is Texture2DResourceHandle && Equals((Texture2DResourceHandle) obj);
		}

		public override int GetHashCode() {
			return resourcePtr.GetHashCode();
		}

		public static bool operator ==(Texture2DResourceHandle left, Texture2DResourceHandle right) {
			return left.Equals(right);
		}

		public static bool operator !=(Texture2DResourceHandle left, Texture2DResourceHandle right) {
			return !left.Equals(right);
		}

		public static unsafe implicit operator ResourceHandle(Texture2DResourceHandle operand) {
			return *((ResourceHandle*) (&operand));
		}
		public static unsafe explicit operator Texture2DResourceHandle(ResourceHandle operand) {
			return *((Texture2DResourceHandle*) (&operand));
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = (int) InteropUtils.StructPacking.Safe)]
	internal struct Texture3DResourceHandle : IEquatable<Texture3DResourceHandle> {
		public static readonly Texture3DResourceHandle NULL = new Texture3DResourceHandle();
		private readonly IntPtr resourcePtr;

		public bool Equals(Texture3DResourceHandle other) {
			return resourcePtr == other.resourcePtr;
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) {
				return false;
			}
			return obj is Texture3DResourceHandle && Equals((Texture3DResourceHandle) obj);
		}

		public override int GetHashCode() {
			return resourcePtr.GetHashCode();
		}

		public static bool operator ==(Texture3DResourceHandle left, Texture3DResourceHandle right) {
			return left.Equals(right);
		}

		public static bool operator !=(Texture3DResourceHandle left, Texture3DResourceHandle right) {
			return !left.Equals(right);
		}

		public static unsafe implicit operator ResourceHandle(Texture3DResourceHandle operand) {
			return *((ResourceHandle*) (&operand));
		}
		public static unsafe explicit operator Texture3DResourceHandle(ResourceHandle operand) {
			return *((Texture3DResourceHandle*) (&operand));
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = (int) InteropUtils.StructPacking.Safe)]
	internal struct SamplerStateResourceHandle : IEquatable<SamplerStateResourceHandle> {
		public static readonly SamplerStateResourceHandle NULL = new SamplerStateResourceHandle();
		private readonly IntPtr resourcePtr;

		public bool Equals(SamplerStateResourceHandle other) {
			return resourcePtr == other.resourcePtr;
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) {
				return false;
			}
			return obj is SamplerStateResourceHandle && Equals((SamplerStateResourceHandle) obj);
		}

		public override int GetHashCode() {
			return resourcePtr.GetHashCode();
		}

		public static bool operator ==(SamplerStateResourceHandle left, SamplerStateResourceHandle right) {
			return left.Equals(right);
		}

		public static bool operator !=(SamplerStateResourceHandle left, SamplerStateResourceHandle right) {
			return !left.Equals(right);
		}

		public static unsafe implicit operator ResourceHandle(SamplerStateResourceHandle operand) {
			return *((ResourceHandle*) (&operand));
		}
		public static unsafe explicit operator SamplerStateResourceHandle(ResourceHandle operand) {
			return *((SamplerStateResourceHandle*) (&operand));
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = (int) InteropUtils.StructPacking.Safe)]
	internal struct RasterizerStateResourceHandle : IEquatable<RasterizerStateResourceHandle> {
		public static readonly RasterizerStateResourceHandle NULL = new RasterizerStateResourceHandle();
		private readonly IntPtr resourcePtr;

		public bool Equals(RasterizerStateResourceHandle other) {
			return resourcePtr == other.resourcePtr;
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) {
				return false;
			}
			return obj is RasterizerStateResourceHandle && Equals((RasterizerStateResourceHandle) obj);
		}

		public override int GetHashCode() {
			return resourcePtr.GetHashCode();
		}

		public static bool operator ==(RasterizerStateResourceHandle left, RasterizerStateResourceHandle right) {
			return left.Equals(right);
		}

		public static bool operator !=(RasterizerStateResourceHandle left, RasterizerStateResourceHandle right) {
			return !left.Equals(right);
		}

		public static unsafe implicit operator ResourceHandle(RasterizerStateResourceHandle operand) {
			return *((ResourceHandle*) (&operand));
		}
		public static unsafe explicit operator RasterizerStateResourceHandle(ResourceHandle operand) {
			return *((RasterizerStateResourceHandle*) (&operand));
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = (int) InteropUtils.StructPacking.Safe)]
	internal struct DepthStencilStateResourceHandle : IEquatable<DepthStencilStateResourceHandle> {
		public static readonly DepthStencilStateResourceHandle NULL = new DepthStencilStateResourceHandle();
		private readonly IntPtr resourcePtr;

		public bool Equals(DepthStencilStateResourceHandle other) {
			return resourcePtr == other.resourcePtr;
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) {
				return false;
			}
			return obj is DepthStencilStateResourceHandle && Equals((DepthStencilStateResourceHandle) obj);
		}

		public override int GetHashCode() {
			return resourcePtr.GetHashCode();
		}

		public static bool operator ==(DepthStencilStateResourceHandle left, DepthStencilStateResourceHandle right) {
			return left.Equals(right);
		}

		public static bool operator !=(DepthStencilStateResourceHandle left, DepthStencilStateResourceHandle right) {
			return !left.Equals(right);
		}

		public static unsafe implicit operator ResourceHandle(DepthStencilStateResourceHandle operand) {
			return *((ResourceHandle*) (&operand));
		}
		public static unsafe explicit operator DepthStencilStateResourceHandle(ResourceHandle operand) {
			return *((DepthStencilStateResourceHandle*) (&operand));
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = (int) InteropUtils.StructPacking.Safe)]
	internal struct InputLayoutHandle : IEquatable<InputLayoutHandle> {
		public static readonly InputLayoutHandle NULL = new InputLayoutHandle();
		private readonly IntPtr resourcePtr;

		public bool Equals(InputLayoutHandle other) {
			return resourcePtr == other.resourcePtr;
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) {
				return false;
			}
			return obj is InputLayoutHandle && Equals((InputLayoutHandle) obj);
		}

		public override int GetHashCode() {
			return resourcePtr.GetHashCode();
		}

		public static bool operator ==(InputLayoutHandle left, InputLayoutHandle right) {
			return left.Equals(right);
		}

		public static bool operator !=(InputLayoutHandle left, InputLayoutHandle right) {
			return !left.Equals(right);
		}

		public static unsafe implicit operator ResourceHandle(InputLayoutHandle operand) {
			return *((ResourceHandle*) (&operand));
		}
		public static unsafe explicit operator InputLayoutHandle(ResourceHandle operand) {
			return *((InputLayoutHandle*) (&operand));
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = (int) InteropUtils.StructPacking.Safe)]
	internal struct BlendStateResourceHandle : IEquatable<BlendStateResourceHandle> {
		public static readonly BlendStateResourceHandle NULL = new BlendStateResourceHandle();
		private readonly IntPtr resourcePtr;

		public bool Equals(BlendStateResourceHandle other) {
			return resourcePtr == other.resourcePtr;
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) {
				return false;
			}
			return obj is BlendStateResourceHandle && Equals((BlendStateResourceHandle) obj);
		}

		public override int GetHashCode() {
			return resourcePtr.GetHashCode();
		}

		public static bool operator ==(BlendStateResourceHandle left, BlendStateResourceHandle right) {
			return left.Equals(right);
		}

		public static bool operator !=(BlendStateResourceHandle left, BlendStateResourceHandle right) {
			return !left.Equals(right);
		}

		public static unsafe implicit operator ResourceHandle(BlendStateResourceHandle operand) {
			return *((ResourceHandle*) (&operand));
		}
		public static unsafe explicit operator BlendStateResourceHandle(ResourceHandle operand) {
			return *((BlendStateResourceHandle*) (&operand));
		}
	}
}