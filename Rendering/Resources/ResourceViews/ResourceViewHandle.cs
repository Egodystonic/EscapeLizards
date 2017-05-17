// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 20 10 2014 at 16:38 by Ben Bowen

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Rendering {
	[StructLayout(LayoutKind.Sequential, Pack = (int) InteropUtils.StructPacking.Safe)]
	public struct ResourceViewHandle : IEquatable<ResourceViewHandle> {
		public static readonly ResourceViewHandle NULL = new ResourceViewHandle();
		private readonly IntPtr resourceViewPtr;

		public override string ToString() {
			return "0x" + resourceViewPtr.ToString("X");
		}

		public bool Equals(ResourceViewHandle other) {
			return resourceViewPtr == other.resourceViewPtr;
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) {
				return false;
			}
			return obj is ResourceViewHandle && Equals((ResourceViewHandle) obj);
		}

		public override int GetHashCode() {
			return resourceViewPtr.GetHashCode();
		}

		public static bool operator ==(ResourceViewHandle left, ResourceViewHandle right) {
			return left.Equals(right);
		}

		public static bool operator !=(ResourceViewHandle left, ResourceViewHandle right) {
			return !left.Equals(right);
		}

		public static unsafe implicit operator ResourceViewHandle(RenderTargetViewHandle operand) {
			return *((ResourceViewHandle*) (&operand));
		}
		public static unsafe implicit operator ResourceViewHandle(DepthStencilViewHandle operand) {
			return *((ResourceViewHandle*) (&operand));
		}
		public static unsafe implicit operator ResourceViewHandle(ShaderResourceViewHandle operand) {
			return *((ResourceViewHandle*) (&operand));
		}
		public static unsafe implicit operator ResourceViewHandle(UnorderedAccessViewHandle operand) {
			return *((ResourceViewHandle*) (&operand));
		}

		public static unsafe explicit operator RenderTargetViewHandle(ResourceViewHandle operand) {
			return *((RenderTargetViewHandle*) (&operand));
		}
		public static unsafe explicit operator DepthStencilViewHandle(ResourceViewHandle operand) {
			return *((DepthStencilViewHandle*) (&operand));
		}
		public static unsafe explicit operator ShaderResourceViewHandle(ResourceViewHandle operand) {
			return *((ShaderResourceViewHandle*) (&operand));
		}
		public static unsafe explicit operator UnorderedAccessViewHandle(ResourceViewHandle operand) {
			return *((UnorderedAccessViewHandle*) (&operand));
		}

		public static explicit operator IntPtr(ResourceViewHandle operand) {
			return operand.resourceViewPtr;
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = (int) InteropUtils.StructPacking.Safe)]
	public struct RenderTargetViewHandle : IEquatable<RenderTargetViewHandle> {
		public static readonly RenderTargetViewHandle NULL = new RenderTargetViewHandle();
		private readonly IntPtr resourceViewPtr;

		public bool Equals(RenderTargetViewHandle other) {
			return resourceViewPtr == other.resourceViewPtr;
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) {
				return false;
			}
			return obj is RenderTargetViewHandle && Equals((RenderTargetViewHandle) obj);
		}

		public override int GetHashCode() {
			return resourceViewPtr.GetHashCode();
		}

		public static bool operator ==(RenderTargetViewHandle left, RenderTargetViewHandle right) {
			return left.Equals(right);
		}

		public static bool operator !=(RenderTargetViewHandle left, RenderTargetViewHandle right) {
			return !left.Equals(right);
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = (int) InteropUtils.StructPacking.Safe)]
	public struct DepthStencilViewHandle : IEquatable<DepthStencilViewHandle> {
		public static readonly DepthStencilViewHandle NULL = new DepthStencilViewHandle();
		private readonly IntPtr resourceViewPtr;

		public bool Equals(DepthStencilViewHandle other) {
			return resourceViewPtr == other.resourceViewPtr;
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) {
				return false;
			}
			return obj is DepthStencilViewHandle && Equals((DepthStencilViewHandle) obj);
		}

		public override int GetHashCode() {
			return resourceViewPtr.GetHashCode();
		}

		public static bool operator ==(DepthStencilViewHandle left, DepthStencilViewHandle right) {
			return left.Equals(right);
		}

		public static bool operator !=(DepthStencilViewHandle left, DepthStencilViewHandle right) {
			return !left.Equals(right);
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = (int) InteropUtils.StructPacking.Safe)]
	public struct ShaderResourceViewHandle : IEquatable<ShaderResourceViewHandle> {
		public static readonly ShaderResourceViewHandle NULL = new ShaderResourceViewHandle();
		private readonly IntPtr resourceViewPtr;

		public bool Equals(ShaderResourceViewHandle other) {
			return resourceViewPtr == other.resourceViewPtr;
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) {
				return false;
			}
			return obj is ShaderResourceViewHandle && Equals((ShaderResourceViewHandle) obj);
		}

		public override int GetHashCode() {
			return resourceViewPtr.GetHashCode();
		}

		public static bool operator ==(ShaderResourceViewHandle left, ShaderResourceViewHandle right) {
			return left.Equals(right);
		}

		public static bool operator !=(ShaderResourceViewHandle left, ShaderResourceViewHandle right) {
			return !left.Equals(right);
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = (int) InteropUtils.StructPacking.Safe)]
	public struct UnorderedAccessViewHandle : IEquatable<UnorderedAccessViewHandle> {
		public static readonly UnorderedAccessViewHandle NULL = new UnorderedAccessViewHandle();
		private readonly IntPtr resourceViewPtr;

		public bool Equals(UnorderedAccessViewHandle other) {
			return resourceViewPtr == other.resourceViewPtr;
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) {
				return false;
			}
			return obj is UnorderedAccessViewHandle && Equals((UnorderedAccessViewHandle) obj);
		}

		public override int GetHashCode() {
			return resourceViewPtr.GetHashCode();
		}

		public static bool operator ==(UnorderedAccessViewHandle left, UnorderedAccessViewHandle right) {
			return left.Equals(right);
		}

		public static bool operator !=(UnorderedAccessViewHandle left, UnorderedAccessViewHandle right) {
			return !left.Equals(right);
		}
	}
}