// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 22 04 2015 at 13:12 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Ophidian.Losgap.Rendering {
	[StructLayout(LayoutKind.Explicit)]
	public struct LightProperties {
		[FieldOffset(0)]
		public Vector3 Position;
		[FieldOffset(12)]
		public float Radius;
		[FieldOffset(16)]
		public Vector3 Color;
		[FieldOffset(28)]
		private float _;

		public LightProperties(Vector3 position, float radius, Vector3 color) {
			Position = position;
			Radius = radius;
			Color = color;
			_ = 0f;
		}
	}

	public sealed class Light {
		private readonly object instanceMutationLock = new object();
		private LightProperties properties;

		public Vector3 Position {
			get {
				lock (instanceMutationLock) {
					return properties.Position;
				}
			}
			set {
				lock (instanceMutationLock) {
					properties.Position = value;
				}
			}
		}

		public float Radius {
			get {
				lock (instanceMutationLock) {
					return properties.Radius;
				}
			}
			set {
				lock (instanceMutationLock) {
					properties.Radius = value;
				}
			}
		}

		public Vector3 Color {
			get {
				lock (instanceMutationLock) {
					return properties.Color;
				}
			}
			set {
				lock (instanceMutationLock) {
					properties.Color = value;
				}
			}
		}

		internal LightProperties Properties {
			get {
				lock (instanceMutationLock) {
					return properties;
				}
			}
		}

		public Light(Vector3 position, float radius, Vector3 color) {
			this.Position = position;
			this.Radius = radius;
			this.Color = color;
		}
	}
}