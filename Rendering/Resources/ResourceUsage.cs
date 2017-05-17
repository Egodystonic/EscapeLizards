// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 27 10 2014 at 10:46 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// An enumeration of usage paradigms for <see cref="IResource">resource</see>s. The <see cref="IResource.Usage"/> applied
	/// to a resource determines the manipulations that are available on its data. However, a more restrictive ResourceUsage can
	/// make it easier for the GPU and rendering pipeline to optimize the resource.
	/// </summary>
	/// <remarks>
	/// Note that certain combinations of ResourceUsage and <see cref="GPUBindings"/> are invalid. Primarily; staging resources
	/// (<see cref="StagingRead"/>/<see cref="StagingWrite"/>/<see cref="StagingReadWrite"/>) can not have any GPU bindings set.
	/// </remarks>
	public enum ResourceUsage {
		/// <summary>
		/// Indicates that the resource can never be altered once it is created. When using this usage you must supply
		/// some initial data before using the resource.
		/// <remarks>
		/// Resources with this usage can never be read back from the GPU, and support no operations.
		/// <para>
		/// This usage translates to the Direct3D <c>USAGE_IMMUTABLE</c>.
		/// </para>
		/// </remarks>
		/// </summary>
		Immutable = 0x1,
		/// <summary>
		/// Indicates that the resource can be altered after it is created. The resource is optimized for frequent updates
		/// (e.g. in the order of one or more updates per frame), but each update will cause the previous data to be discarded.
		/// <remarks>
		/// Resources with this usage only support DiscardWrite operations; or being copied to from other resources. 
		/// Resources with this usage can never be read back from the GPU.
		/// <para>
		/// This usage translates to the Direct3D <c>USAGE_DYNAMIC</c> with <c>CPU_ACCESS_WRITE</c>.
		/// </para>
		/// </remarks>
		/// </summary>
		DiscardWrite = 0x10002,
		/// <summary>
		/// Indicates that the resource can be altered after it is created. The resource is only optimized for occasional updates
		/// (e.g. in the order of one update every few frames or less). However, the write operations can be partial, and previous data
		/// will not be lost.
		/// <remarks>
		/// Resources with this usage only support Write operations; or being copied to from other resources.
		/// Resources with this usage can never be read back from the GPU.
		/// <para>
		/// This usage translates to the Direct3D <c>USAGE_DEFAULT</c>.
		/// </para>
		/// </remarks>
		/// </summary>
		Write = 0x0,
		/// <summary>
		/// Indicates a resource that may not be supplied to the rendering pipeline; but is used primarily as a copy destination
		/// from another resource.
		/// </summary>
		/// <remarks>
		/// Resources with this usage support Read operations as well as allowing other resources to copy data to them, and then
		/// copying data elsewhere (hence the name 'staging').
		/// <para>
		/// Staging resources may not be bound to the GPU (see: <see cref="GPUBindings"/>/<see cref="IResource.PermittedBindings"/>).
		/// </para>
		/// <para>
		/// This usage translates to the Direct3D <c>USAGE_STAGING</c> with <c>CPU_ACCESS_READ</c>.
		/// </para>
		/// </remarks>
		StagingRead = 0x20003,
		/// <summary>
		/// Indicates a resource that may not be supplied to the rendering pipeline; but is used primarily as a copy destination
		/// from another resource.
		/// </summary>
		/// <remarks>
		/// Resources with this usage support Write operations as well as allowing other resources to copy data to them, and then
		/// copying data elsewhere (hence the name 'staging').
		/// <para>
		/// Staging resources may not be bound to the GPU (see: <see cref="GPUBindings"/>/<see cref="IResource.PermittedBindings"/>).
		/// </para>
		/// <para>
		/// This usage translates to the Direct3D <c>USAGE_STAGING</c> with <c>CPU_ACCESS_WRITE</c>.
		/// </para>
		/// </remarks>
		StagingWrite = 0x10003,
		/// <summary>
		/// Indicates a resource that may not be supplied to the rendering pipeline; but is used primarily as a copy destination
		/// from another resource.
		/// </summary>
		/// <remarks>
		/// Resources with this usage support Read and Write operations as well as allowing other resources to copy data to them, and then
		/// copying data elsewhere (hence the name 'staging').
		/// <para>
		/// Staging resources may not be bound to the GPU (see: <see cref="GPUBindings"/>/<see cref="IResource.PermittedBindings"/>).
		/// </para>
		/// <para>
		/// This usage translates to the Direct3D <c>USAGE_STAGING</c> with <c>CPU_ACCESS_READ</c> and <c>CPU_ACCESS_WRITE</c>.
		/// </para>
		/// </remarks>
		StagingReadWrite = 0x30003,
	}

	internal static class ResourceUsageExtensions {
		public static int GetUsage(this ResourceUsage @this) {
			return ((int) @this) & 0xF;
		}
		public static int GetCPUUsage(this ResourceUsage @this) {
			return ((int) @this) & 0xF0000;
		}

		public static bool ShouldMapRead(this ResourceUsage @this) {
			return @this == ResourceUsage.StagingReadWrite || @this == ResourceUsage.StagingRead;
		}
		public static bool ShouldMapWrite(this ResourceUsage @this) {
			return @this == ResourceUsage.StagingReadWrite || @this == ResourceUsage.StagingWrite;
		}
		public static bool ShouldMapReadWrite(this ResourceUsage @this) {
			return @this == ResourceUsage.StagingReadWrite;
		}
		public static bool ShouldMapWriteDiscard(this ResourceUsage @this) {
			return @this == ResourceUsage.DiscardWrite;
		}
		public static bool ShouldUpdateSubresourceRegion(this ResourceUsage @this) {
			return @this == ResourceUsage.Write;
		}
		public static bool ShouldBeCopyResourceDestination(this ResourceUsage @this) {
			return @this != ResourceUsage.Immutable;
		}
		public static bool ShouldBeCopySubresourceRegionDestination(this ResourceUsage @this) {
			return @this != ResourceUsage.Immutable;
		}
	}
}