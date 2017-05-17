// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 18 12 2014 at 14:33 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// Cameras represent a point, direction, and orientation in a 3D scene that the current frame is rendered from; as well as other properties
	/// such as the field of view, etc.
	/// </summary>
	/// <remarks>
	/// Generally, a Camera is linked to a <see cref="SceneViewport"/>, which displays the rendered scene captured from the point-of-view of the
	/// camera.
	/// </remarks>
	public unsafe class Camera : IDisposable {
		private const uint XMMATRIX_ALIGNMENT = 16U;
		/// <summary>
		/// A lock object that must be entered when making modifications to any part of the state of this camera.
		/// </summary>
		protected readonly object InstanceMutationLock = new object();
		private readonly AlignedAllocation<Matrix> viewMatrix = new AlignedAllocation<Matrix>(XMMATRIX_ALIGNMENT);
		/// <summary>
		/// The camera's current position.
		/// </summary>
		protected Vector3 _Position = Vector3.ZERO;
		/// <summary>
		/// The camera's current orientation.
		/// </summary>
		protected Vector3 _Orientation = Vector3.FORWARD;
		/// <summary>
		/// The camera's current up-direction.
		/// </summary>
		protected Vector3 _UpDirection = Vector3.UP;
		/// <summary>
		/// The camera's current field of view, in radians. Indicates a horizontal FOV if <see cref="FovIsHorizontal"/> is <c>true</c>,
		/// or a vertical one if <see cref="FovIsHorizontal"/> is <c>false</c>.
		/// </summary>
		protected float FovRadians = MathUtils.PI_OVER_TWO;
		/// <summary>
		/// True if the current value in <see cref="FovRadians"/> represents a horizontal field-of-view, false if it represents a vertical one.
		/// </summary>
		protected bool FovIsHorizontal = true;
		private bool isDisposed = false;
		private Vector3? orthographicDimensions = null;

		public Vector3? OrthographicDimensions {
			get {
				return orthographicDimensions;
			}
			set {
				orthographicDimensions = value;
			}
		}

		/// <summary>
		/// The current position in the 3D world of this camera.
		/// </summary>
		/// <remarks>
		/// If moving this camera, rather than just setting its position, use <see cref="Move"/> for a slight performance increase.
		/// </remarks>
		public virtual Vector3 Position {
			get {
				lock (InstanceMutationLock) {
					return _Position;
				}
			}
			set {
				lock (InstanceMutationLock) {
					_Position = value;
				}
			}
		}

		/// <summary>
		/// The current direction that this camera is looking in. Guaranteed to be unit-length.
		/// </summary>
		/// <remarks>
		/// The orientation can be set with the <see cref="Orient"/>, <see cref="LookAt"/>, or <see cref="Rotate"/> methods.
		/// </remarks>
		public Vector3 Orientation {
			get {
				lock (InstanceMutationLock) {
					return _Orientation;
				}
			}
		}

		/// <summary>
		/// The direction that is 'up' from the camera's perspective; used to orient the camera the right way 'up'. Guaranteed to be unit length.
		/// </summary>
		/// /// <remarks>
		/// The up direction can be set with the <see cref="Orient"/>, <see cref="LookAt"/>, or <see cref="Rotate"/> methods.
		/// </remarks>
		public Vector3 UpDirection {
			get {
				lock (InstanceMutationLock) {
					return _UpDirection;
				}
			}
		}

		/// <summary>
		/// Whether or not this camera has been disposed.
		/// </summary>
		public bool IsDisposed {
			get {
				lock (InstanceMutationLock) {
					return isDisposed;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sv"></param>
		/// <param name="pixel">Relative to viewport centre</param>
		/// <returns></returns>
		public unsafe Ray PixelRayCast(SceneViewport sv, Vector2 pixel) {
			Vector2 viewportSizePixels = sv.SizePixels;
			Matrix invProjMat = ((Matrix*) sv.GetRecalculatedProjectionMatrix(this))->Inverse;
			Matrix invViewMat = ((Matrix*) GetRecalculatedViewMatrix())->Inverse;
			Vector4 viewSpaceDir = invProjMat * new Vector4(
				((2f * pixel.X) / viewportSizePixels.X),	// These two lines are converting the pixel in to NDC ([-1:1, -1:1, -1:1, -1:1])
				((2f * pixel.Y) / -viewportSizePixels.Y),
				1f,	// These two lines are for homogenous clip space. Z=1f is forwards, out of the screen
				1f // Perspective divide = W
			);
			Vector3 worldSpaceDir = (Vector3) (invViewMat * new Vector4(viewSpaceDir, z: 1f, w: 0f)).ToUnit();
			return new Ray(Position, worldSpaceDir);
		}

		public unsafe Vector2 WorldToScreen(SceneViewport sv, Vector3 worldCoordinates) {
			Vector2 viewportSizePixels = sv.SizePixels;
			Matrix vpMat = *((Matrix*) GetRecalculatedViewMatrix()) * *((Matrix*) sv.GetRecalculatedProjectionMatrix(this));

			var clipSpaceCoordinates = worldCoordinates * vpMat;

			float halfWidth = 0.5f * viewportSizePixels.X;
			float halfHeight = 0.5f * viewportSizePixels.Y;

			return new Vector2(
				(clipSpaceCoordinates.X / clipSpaceCoordinates.Z) * halfWidth + halfWidth,
				viewportSizePixels.Y - ((clipSpaceCoordinates.Y / clipSpaceCoordinates.Z) * halfHeight + halfHeight)
			);
		}

		public unsafe Vector2 WorldToScreenNormalized(SceneViewport sv, Vector3 worldCoordinates) {
			var nonNormalized = WorldToScreen(sv, worldCoordinates);
			return new Vector2(
				nonNormalized.X / sv.SizePixels.X,
				nonNormalized.Y / sv.SizePixels.Y
			);
		}

		public unsafe Vector2 WorldToScreenNormalizedClipped(SceneViewport sv, Vector3 worldCoordinates) {
			var normalized = WorldToScreenNormalized(sv, worldCoordinates);
			return new Vector2(
				(float) MathUtils.Clamp(normalized.X, 0f, 1f),
				(float) MathUtils.Clamp(normalized.Y, 0f, 1f)
			);
		}

		/// <summary>
		/// Derives a vertical field-of-view from a given horizontal field-of-view and the target aspect ratio.
		/// </summary>
		/// <param name="horizontalFOVRadians">The horizontal field-of-view, in radians.</param>
		/// <param name="aspectRatio">The output (<see cref="SceneViewport"/>) aspect ratio.</param>
		/// <returns>The vertical field-of-view, in radians, that provides the same output as the given
		/// <paramref name="horizontalFOVRadians"/> for the given <paramref name="aspectRatio"/>.</returns>
		public static float DeriveVerticalFOV(float horizontalFOVRadians, float aspectRatio) {
			return 2f * (float) Math.Atan(Math.Tan(horizontalFOVRadians / 2f) / aspectRatio);
		}

		/// <summary>
		/// Derives a horizontal field-of-view from a given vertical field-of-view and the target aspect ratio.
		/// </summary>
		/// <param name="verticalFOVRadians">The vertical field-of-view, in radians.</param>
		/// <param name="aspectRatio">The output (<see cref="SceneViewport"/>) aspect ratio.</param>
		/// <returns>The horizontal field-of-view, in radians, that provides the same output as the given
		/// <paramref name="verticalFOVRadians"/> for the given <paramref name="aspectRatio"/>.</returns>
		public static float DeriveHorizontalFOV(float verticalFOVRadians, float aspectRatio) {
			return 2f * (float) Math.Atan(aspectRatio * Math.Tan(verticalFOVRadians / 2f));
		}

		/// <summary>
		/// Moves this camera by the given <paramref name="distance"/>. The distance will be added to the camera's <see cref="Position"/>. 
		/// </summary>
		/// <param name="distance">The distance to move this camera.</param>
		public virtual void Move(Vector3 distance) {
			lock (InstanceMutationLock) {
				_Position += distance;
			}
		}

		/// <summary>
		/// Orients this camera with the given <paramref name="orientation"/> and <paramref name="upDirection"/>.
		/// </summary>
		/// <param name="orientation">The direction that this camera should face. Does not need to be unit length.</param>
		/// <param name="upDirection">The direction that is 'up' from this camera's perspective. Will be
		/// <see cref="Vector3.OrthonormalizedAgainst">orthonormalized against</see> the <paramref name="orientation"/>.</param>
		public virtual void Orient(Vector3 orientation, Vector3 upDirection) {
			lock (InstanceMutationLock) {
				this._Orientation = orientation.ToUnit();
				this._UpDirection = upDirection.OrthonormalizedAgainst(this._Orientation);
			}
		}

		/// <summary>
		/// Orients this camera to look at the given <paramref name="point"/>, with the given <paramref name="upDirection"/>.
		/// </summary>
		/// <param name="point">The point that the camera should "look at" (face).</param>
		/// <param name="upDirection">The direction that is 'up' from this camera's perspective. Will be
		/// <see cref="Vector3.OrthonormalizedAgainst">orthonormalized against</see> the resultant <see cref="Orientation"/>.</param>
		public virtual void LookAt(Vector3 point, Vector3 upDirection) {
			lock (InstanceMutationLock) {
				_Orientation = (point - _Position).ToUnit();
				this._UpDirection = upDirection.OrthonormalizedAgainst(_Orientation);
			}
		}

		/// <summary>
		/// Rotates the camera's <see cref="Orientation"/> (and <see cref="UpDirection"/>) by the given <paramref name="rotation"/>.
		/// </summary>
		/// <param name="rotation">The rotation to apply to this camera.</param>
		public virtual void Rotate(Quaternion rotation) {
			lock (InstanceMutationLock) {
				_Orientation = _Orientation.RotateBy(rotation);
				_UpDirection = _UpDirection.RotateBy(rotation);
			}
		}

		/// <summary>
		/// Sets this camera's horizontal field-of-view. A larger value results in a camera that can 'see' wider peripheral angles.
		/// </summary>
		/// <param name="fovRadians">The field of view, in radians.</param>
		public virtual void SetHorizontalFOV(float fovRadians) {
			lock (InstanceMutationLock) {
				this.FovRadians = fovRadians;
				FovIsHorizontal = true;
			}
		}

		/// <summary>
		/// Sets this camera's vertical field-of-view. A larger value results in a camera that can 'see' wider peripheral angles.
		/// </summary>
		/// <param name="fovRadians">The field of view, in radians.</param>
		public virtual void SetVerticalFOV(float fovRadians) {
			lock (InstanceMutationLock) {
				this.FovRadians = fovRadians;
				FovIsHorizontal = false;
			}
		}

		/// <summary>
		/// Disposes the camera, making it no longer usable, but releasing manually-managed memory that it contains.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "viewMatrix",
			Justification = "It DOES call Dispose on viewMatrix... Bugged rule. :(")]
		public virtual void Dispose() {
			lock (InstanceMutationLock) {
				if (isDisposed) return;
				isDisposed = true;
				// ReSharper disable once ImpureMethodCallOnReadonlyValueField Resharper bug, Dispose is not impure
				viewMatrix.Dispose();
			}
		}

		/// <summary>
		/// Returns a string representing this instance and its current state/values.
		/// </summary>
		/// <returns>
		/// A new string that details this object.
		/// </returns>
		public override string ToString() {
			return "Camera (at " + Position + ", with orientation " + Orientation + " and up-direction " + UpDirection + ")";
		}

		internal IntPtr GetRecalculatedViewMatrix() {
			lock (InstanceMutationLock) {
				if (isDisposed) return IntPtr.Zero;

				Vector3 u = _UpDirection * _Orientation;
				Vector3 v = _Orientation * u;

				// ReSharper disable once ImpureMethodCallOnReadonlyValueField Resharper bug, Write is not impure
				viewMatrix.Write(new Matrix(
					r0C0: u.X, r1C0: u.Y, r2C0: u.Z,
					r0C1: v.X, r1C1: v.Y, r2C1: v.Z,
					r0C2: _Orientation.X, r1C2: _Orientation.Y, r2C2: _Orientation.Z,
					r3C0: Vector3.Dot(_Position, -u),
					r3C1: Vector3.Dot(_Position, -v),
					r3C2: Vector3.Dot(_Position, -_Orientation),
					r3C3: 1f
				));

				return viewMatrix.AlignedPointer;
			}
		}

		internal float GetVerticalFOV(float aspectRatio) {
			lock (InstanceMutationLock) {
				if (FovIsHorizontal) return DeriveVerticalFOV(FovRadians, aspectRatio);
				else return FovRadians;
			}
		}

		public CameraFrustum GetFrustum(SceneViewport viewport) {
			lock (InstanceMutationLock) {
				float aspectRatio = viewport.AspectRatio;
				float verticalFOV = FovIsHorizontal ? DeriveVerticalFOV(FovRadians, aspectRatio) : FovRadians;
				float tanHalfFOV = (float) Math.Tan(verticalFOV * 0.5f);
				float nearHeightHalf = tanHalfFOV * viewport.NearPlaneDist;
				float nearWidthHalf = nearHeightHalf * aspectRatio;

				Vector3 camPos = Position;
				Vector3 forwardVec = Orientation;
				Vector3 upVec = UpDirection;
				Vector3 rightVec = upVec * forwardVec;

				Vector3 nearCentre = camPos + forwardVec.WithLength(viewport.NearPlaneDist);
				Vector3 farCentre = camPos + forwardVec.WithLength(viewport.FarPlaneDist);

				Vector3 rightPoint = nearCentre + rightVec.WithLength(nearWidthHalf);
				Vector3 rightNormal = (rightPoint - camPos).ToUnit() * upVec;
				Vector3 leftPoint = nearCentre - rightVec.WithLength(nearWidthHalf);
				Vector3 leftNormal = upVec * (leftPoint - camPos).ToUnit();
				Vector3 topPoint = nearCentre + upVec.WithLength(nearWidthHalf);
				Vector3 topNormal = rightVec * (topPoint - camPos).ToUnit();
				Vector3 bottomPoint = nearCentre - upVec.WithLength(nearWidthHalf);
				Vector3 bottomNormal = (bottomPoint - camPos).ToUnit() * rightVec;

				return new CameraFrustum(
					nearPlane: new Plane(forwardVec, nearCentre),
					farPlane: new Plane(-forwardVec, farCentre),
					leftPlane: new Plane(leftNormal, leftPoint),
					rightPlane: new Plane(rightNormal, rightPoint),
					topPlane: new Plane(topNormal, topPoint),
					bottomPlane: new Plane(bottomNormal, bottomPoint)
				);
			}
		}
	}
}