// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 16 12 2014 at 15:24 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// Represents a rectangular subregion of a <see cref="Window"/> that a given render pass may be rendered to.
	/// </summary>
	/// <remarks>
	/// Put differently, a SceneViewport acts as a 'viewport' in to the scene through a region of its <see cref="TargetWindow"/>.
	/// </remarks>
	public sealed class SceneViewport : IDisposable {
		/// <summary>
		/// The Window that this viewport is defining a region of.
		/// </summary>
		public readonly Window TargetWindow;
		/// <summary>
		/// The side or corner of this viewport that retains its relative position when the <see cref="TargetWindow"/> is resized.
		/// Also indicates the area of the <see cref="TargetWindow"/> that the viewport's <see cref="AnchorOffset"/> is calculated from.
		/// </summary>
		/// <remarks>
		/// For example, if the Anchoring value is set to <see cref="ViewportAnchoring.BottomRight"/>, the viewport's bottom-right corner
		/// will always be a constant percentage-distance from the bottom-right corner of the containing window (where the percentage is
		/// specified by <see cref="AnchorOffset"/>).
		/// </remarks>
		public readonly ViewportAnchoring Anchoring;
		/// <summary>
		/// The offset from the <see cref="Anchoring">anchored</see> corner or side of the <see cref="TargetWindow"/>
		/// that this viewport resides at.
		/// </summary>
		/// <remarks>
		/// For example, if the <see cref="Anchoring"/> is <see cref="ViewportAnchoring.TopLeft"/>, then an AnchorOffset value of
		/// <c>[0.25f, 0.1f]</c> would mean that this viewport's top-left corner would start at a distance 25% along the width and 10% along
		/// the height of the <see cref="TargetWindow"/>.
		/// <para>
		/// As the <see cref="TargetWindow"/> is resized, the viewport will resize/move in order to maintain the same ratios specified by
		/// AnchorOffset and <see cref="Size"/>.
		/// </para>
		/// <para>
		/// If you've chosen a <c>Centered</c> <see cref="Anchoring"/>, one or both of the values in this property may be ignored (as centered
		/// viewports are always positioned so that they reside in the centre of the window in one or more dimensions).
		/// </para>
		/// </remarks>
		public readonly Vector2 AnchorOffset;
		/// <summary>
		/// The size of this viewport, specified as a ratio of the <see cref="TargetWindow"/>'s resolution.
		/// </summary>
		/// <remarks>
		/// For example, a Size value of <c>[0.5f, 1f]</c> indicates a viewport that occupies 50% of the <see cref="TargetWindow"/>'s width, 
		/// and 100% of its height.
		/// <para>
		/// As the <see cref="TargetWindow"/> is resized, the viewport will resize/move in order to maintain the same ratios specified by
		/// <see cref="AnchorOffset"/> and Size.
		/// </para>
		/// </remarks>
		public readonly Vector2 Size;
		/// <summary>
		/// The minimum distance between any object and the <see cref="Camera"/> rendering to this viewport permissible for an object to be drawn
		/// </summary>
		public readonly float NearPlaneDist;
		/// <summary>
		/// The maximum distance between any object and the <see cref="Camera"/> rendering to this viewport permissible for an object to be drawn.
		/// </summary>
		public readonly float FarPlaneDist;
		internal readonly ViewportHandle ViewportHandle; // May be NULL if isDisposed is true
		private readonly AlignedAllocation<Matrix> projectionMatrix = new AlignedAllocation<Matrix>(16L);
		private bool isDisposed = false;

		/// <summary>
		/// Returns the aspect ratio (width / height) of the viewport at its current size.
		/// This value will change as the <see cref="TargetWindow"/>'s resolution changes.
		/// </summary>
		public float AspectRatio {
			get {
				lock (TargetWindow.WindowMutationLock) {
					float width = TargetWindow.Width;
					float height = TargetWindow.Height;

					return (width * Size.X) / (height * Size.Y);
				}
			}
		}

		public Vector2 SizePixels {
			get {
				uint windowWidth = TargetWindow.Width;
				uint windowHeight = TargetWindow.Height;

				uint topLeftX, topLeftY;

				// ReSharper disable CompareOfFloatsByEqualityOperator Direct comparison to 0f is desired
				// Anchoring enum values are bitmasks to make this bit work. See comments in ViewportAnchoring
				// Falling to default values in both switch statements will result in a standard top-left corner selection
				switch ((int) Anchoring & 0xF) { // Horizontal bits
					case 0x1: // 1 == "Right"
						topLeftX = (uint) (windowWidth - Math.Floor((AnchorOffset.X + Size.X) * windowWidth));
						break;
					case 0x2: // 2 == "Centered"
						if (AnchorOffset.X != 0f) {
							Logger.Warn(this + ": Anchor x-offset of " + AnchorOffset.X + " will be ignored " +
								"as the viewport is Anchored centrally in the horizontal direction.");
						}
						topLeftX = (uint) ((0.5f - Size.X / 2f) * windowWidth);
						break;
					default: // 0 == "Left"
						topLeftX = (uint) (AnchorOffset.X * windowWidth);
						break;
				}
				switch ((int) Anchoring & 0xF0) { // Vertical bits
					case 0x10: // 1 == "Bottom"
						topLeftY = (uint) (windowHeight - Math.Floor((AnchorOffset.Y + Size.Y) * windowHeight));
						break;
					case 0x20: // 2 == "Centered"
						if (AnchorOffset.Y != 0f) {
							Logger.Warn(this + ": Anchor y-offset of " + AnchorOffset.Y + " will be ignored " +
								"as the viewport is Anchored centrally in the vertical direction.");
						}
						topLeftY = (uint) ((0.5f - Size.Y / 2f) * windowHeight);
						break;
					default: // 0 == "Top"
						topLeftY = (uint) (AnchorOffset.Y * windowHeight);
						break;
				}
				// ReSharper restore CompareOfFloatsByEqualityOperator

				return new Vector2(
					(uint) MathUtils.Clamp(windowWidth * Size.X, 0U, windowWidth - topLeftX),
					(uint) MathUtils.Clamp(windowHeight * Size.Y, 0U, windowHeight - topLeftY)
				);
			}
		}

		/// <summary>
		/// Whether or not this viewport has been disposed.
		/// </summary>
		public bool IsDisposed {
			get {
				lock (TargetWindow.WindowMutationLock) {
					return isDisposed;
				}
			}
		}

		/// <summary>
		/// Disposes the memory associated with this viewport and invalidates its usage.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "projectionMatrix",
			Justification = "Dispose IS called, just not if we've already disposed.")]
		public void Dispose() {
			lock (TargetWindow.WindowMutationLock) {
				if (isDisposed) return;

				projectionMatrix.Dispose();

				try {
					InteropUtils.CallNative(NativeMethods.WindowFactory_DestroyViewport,
						ViewportHandle
						).ThrowOnFailure();
				}
				finally {
					isDisposed = true;
				}
			}
		}

		// No lock on WindowTarget.WindowMutationLock here because it's an internal method, only accessible from Window.AddViewport.
		// If that changes in the future, add a lock!
		internal SceneViewport(Window targetWindow, ViewportAnchoring anchoring, Vector2 anchorOffset, Vector2 size, 
			float nearPlaneDist, float farPlaneDist) {
			if (targetWindow == null) throw new ArgumentNullException("targetWindow");
			if (!Enum.IsDefined(typeof(ViewportAnchoring), anchoring)) throw new ArgumentException("Invalid enum value.", "anchoring");
			//if (anchorOffset.X < 0f || anchorOffset.X > 1f || anchorOffset.Y < 0f || anchorOffset.Y > 1f) {
			//	throw new ArgumentException("Both anchor offset dimensions must be between 0f and 1f.", "anchorOffset");
			//}
			//if (size.X < 0f || size.X > 1f - anchorOffset.X || size.Y < 0f || size.Y > 1f - anchorOffset.Y) {
			//	throw new ArgumentException("combination of size and anchor offset in either dimension must be between 0f and 1f.", "size");
			//}
			if (nearPlaneDist <= 0f) throw new ArgumentException("Near plane must be greater than 0f.", "nearPlaneDist");
			if (farPlaneDist <= 0f) throw new ArgumentException("Far plane must be greater than 0f.", "farPlaneDist");
			
			this.TargetWindow = targetWindow;
			this.Anchoring = anchoring;
			this.AnchorOffset = anchorOffset;
			this.Size = size;
			this.NearPlaneDist = nearPlaneDist;
			this.FarPlaneDist = farPlaneDist;

			if (targetWindow.IsClosed) {
				ViewportHandle = ViewportHandle.NULL;
				isDisposed = true;
			}
			else {
				ViewportHandle outViewportHandle;
				unsafe {
					InteropUtils.CallNative(NativeMethods.WindowFactory_CreateViewport,
						(IntPtr) (&outViewportHandle)
					).ThrowOnFailure();
				}
				ViewportHandle = outViewportHandle;
			}

			TargetWindow_WindowResized(TargetWindow, TargetWindow.Width, TargetWindow.Height);
			TargetWindow.WindowResized += TargetWindow_WindowResized;
		}

#if DEBUG
		~SceneViewport() {
			if (!IsDisposed) Logger.Warn(this + " was not disposed before finalization!");
		}
#endif

		/// <summary>
		/// Returns a string detailing the viewport's attributes.
		/// </summary>
		/// <returns>A string detailing the viewport's attributes.</returns>
		public override string ToString() {
			return "Viewport (anchored as \"" + Anchoring + " + " + AnchorOffset + "\" to " + TargetWindow + ")";
		}

		internal IntPtr GetRecalculatedProjectionMatrix(Camera camera) {
			Assure.NotNull(camera);

			lock (TargetWindow.WindowMutationLock) {
				if (isDisposed) return IntPtr.Zero;

				if (camera.OrthographicDimensions != null) {
					projectionMatrix.Write(new Matrix(
						r0C0: 2f / camera.OrthographicDimensions.Value.X,
						r1C1: 2f / camera.OrthographicDimensions.Value.Y,
						r2C2: 1f / (camera.OrthographicDimensions.Value.Z - NearPlaneDist),
						r2C3: -NearPlaneDist / (camera.OrthographicDimensions.Value.Z - NearPlaneDist),
						r3C3: 1f
					).Transpose);
				}
				else {
					float verticalFOV = camera.GetVerticalFOV(AspectRatio);

					projectionMatrix.Write(new Matrix(
						r0C0: 1f / (AspectRatio * (float) Math.Tan(verticalFOV / 2f)),
						r1C1: 1f / (float) Math.Tan(verticalFOV / 2f),
						r2C2: FarPlaneDist / (FarPlaneDist - NearPlaneDist),
						r2C3: 1f,
						r3C2: (-NearPlaneDist * FarPlaneDist) / (FarPlaneDist - NearPlaneDist)
					));
				}
				
				return projectionMatrix.AlignedPointer;
			}
		}

		private void TargetWindow_WindowResized(Window target, uint width, uint height) {
			lock (TargetWindow.WindowMutationLock) { // Lock here because this method is called from a 'public' event (async resize)
				if (isDisposed || TargetWindow.IsClosed) return;

				if (Size.EqualsExactly(Vector2.ONE)) { // Special case for ONE vector, to avoid rounding errors etc that would otherwise occur
					InteropUtils.CallNative(NativeMethods.WindowFactory_AlterViewport,
						ViewportHandle,
						0U,
						0U,
						TargetWindow.Width,
						TargetWindow.Height
					).ThrowOnFailure();

					Logger.Debug("Viewport with anchoring " + Anchoring + " with offset " + AnchorOffset + " and size " + Size + " on" +
						" " + TargetWindow + ". Set to full window size.");
				}
				else {
					float windowWidth = TargetWindow.Width;
					float windowHeight = TargetWindow.Height;

					uint topLeftX, topLeftY;

					// ReSharper disable CompareOfFloatsByEqualityOperator Direct comparison to 0f is desired
					// Anchoring enum values are bitmasks to make this bit work. See comments in ViewportAnchoring
					// Falling to default values in both switch statements will result in a standard top-left corner selection
					switch ((int) Anchoring & 0xF) { // Horizontal bits
						case 0x1: // 1 == "Right"
							topLeftX = (uint) (windowWidth - Math.Floor((AnchorOffset.X + Size.X) * windowWidth));
							break;
						case 0x2: // 2 == "Centered"
							if (AnchorOffset.X != 0f) {
								Logger.Warn(this + ": Anchor x-offset of " + AnchorOffset.X + " will be ignored " +
									"as the viewport is Anchored centrally in the horizontal direction.");
							}
							topLeftX = (uint) ((0.5f - Size.X / 2f) * windowWidth);
							break;
						default: // 0 == "Left"
							topLeftX = (uint) (AnchorOffset.X * windowWidth);
							break;
					}
					switch ((int) Anchoring & 0xF0) { // Vertical bits
						case 0x10: // 1 == "Bottom"
							topLeftY = (uint) (windowHeight - Math.Floor((AnchorOffset.Y + Size.Y) * windowHeight));
							break;
						case 0x20: // 2 == "Centered"
							if (AnchorOffset.Y != 0f) {
								Logger.Warn(this + ": Anchor y-offset of " + AnchorOffset.Y + " will be ignored " +
									"as the viewport is Anchored centrally in the vertical direction.");
							}
							topLeftY = (uint) ((0.5f - Size.Y / 2f) * windowHeight);
							break;
						default: // 0 == "Top"
							topLeftY = (uint) (AnchorOffset.Y * windowHeight);
							break;
					}
					// ReSharper restore CompareOfFloatsByEqualityOperator

					topLeftX = (uint) MathUtils.Clamp(topLeftX, 0U, (uint) windowWidth);
					topLeftY = (uint) MathUtils.Clamp(topLeftY, 0U, (uint) windowHeight);

					uint widthPx = (uint) MathUtils.Clamp(windowWidth * Size.X, 0U, (uint) windowWidth - topLeftX);
					uint heightPx = (uint) MathUtils.Clamp(windowHeight * Size.Y, 0U, (uint) windowHeight - topLeftY);

					InteropUtils.CallNative(NativeMethods.WindowFactory_AlterViewport,
						ViewportHandle,
						topLeftX,
						topLeftY,
						widthPx,
						heightPx
					).ThrowOnFailure();

					Logger.Debug("Viewport with anchoring " + Anchoring + " with offset " + AnchorOffset + " and size " + Size + " on" +
						" " + TargetWindow + ". Actual dimensions: TopLeft: " + topLeftX + "x" + topLeftY + ", Size: " + widthPx + "x" + heightPx);
				}
			}
		}
	}
}