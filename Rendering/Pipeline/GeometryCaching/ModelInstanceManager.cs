// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 23 01 2015 at 17:53 by Ben Bowen

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap.Rendering {
	internal unsafe sealed class ModelInstanceManager : IDisposable {
		private const uint INITIAL_INSTANCE_ALLOCATION = 4U;
		private const uint MAX_SIZE_BEFORE_LINEAR_GROWTH = INITIAL_INSTANCE_ALLOCATION << 4;
		private const uint LINEAR_GROWTH_AMOUNT = MAX_SIZE_BEFORE_LINEAR_GROWTH;
		private readonly object instanceMutationLock = new object();
		private readonly Dictionary<uint, MIDArray> materialMap = new Dictionary<uint, MIDArray>();
		private KeyValuePair<Material, MIDArray>[] modelInstanceDataHoldingArray = new KeyValuePair<Material, MIDArray>[0];
		private bool isDisposed = false;
		private readonly Func<uint, MIDArray> createNewMIDArrayAct;

		public ModelInstanceManager() {
			createNewMIDArrayAct = CreateNewMIDArray;
		}

		private bool MatMapContainsKey(uint key) {
			foreach (uint matKey in materialMap.Keys) {
				if (matKey == key) return true;
			}
			return false;
		}

		public struct MIDArray : IEquatable<MIDArray> { // ModelInstanceData Array
			public readonly ModelInstanceData* Data;
			public readonly uint Length;

			public MIDArray(ModelInstanceData* data, uint length) {
				Data = data;
				Length = length;
			}

			public bool Equals(MIDArray other) {
				return Data == other.Data;
			}

			public override bool Equals(object obj) {
				if (ReferenceEquals(null, obj)) {
					return false;
				}
				return obj is MIDArray && Equals((MIDArray)obj);
			}

			public override int GetHashCode() {
				return (int) Data;
			}

			public static bool operator ==(MIDArray left, MIDArray right) {
				return left.Equals(right);
			}

			public static bool operator !=(MIDArray left, MIDArray right) {
				return !left.Equals(right);
			}
		}

		public bool IsDisposed {
			get {
				lock (instanceMutationLock) {
					return isDisposed;
				}
			}
		}

		public ModelInstanceHandle AllocateInstance(uint materialIndex, uint modelIndex, uint sceneLayerIndex, Transform initialTransform) {
			using (RenderingModule.RenderStateBarrier.AcquirePermit(withLock: instanceMutationLock)) {
				MIDArray midArray = materialMap.GetOrCreate(materialIndex, createNewMIDArrayAct);
				ModelInstanceData* data = midArray.Data;
				for (uint i = midArray.Length - 1U; i < midArray.Length; --i) {
					if (!data[i].InUse) {
						data[i] = new ModelInstanceData(modelIndex, sceneLayerIndex, initialTransform);
						return new ModelInstanceHandle(this, materialIndex, i);
					}
				}

				// MIDArray is full, so resize...
				uint newSize = midArray.Length << 1;
				uint numBytes = (uint) sizeof(ModelInstanceData) * newSize;
				uint oldNumBytes = (uint) sizeof(ModelInstanceData) * midArray.Length;
				if (midArray.Length >= MAX_SIZE_BEFORE_LINEAR_GROWTH) newSize = midArray.Length + LINEAR_GROWTH_AMOUNT;
				ModelInstanceData* newData = (ModelInstanceData*) Marshal.AllocHGlobal(new IntPtr(numBytes));
				UnsafeUtils.MemCopy((IntPtr) data, (IntPtr) newData, oldNumBytes);
				Marshal.FreeHGlobal((IntPtr) data);
				UnsafeUtils.ZeroMem(((IntPtr) newData) + (int) oldNumBytes, numBytes - oldNumBytes);
				materialMap[materialIndex] = new MIDArray(newData, newSize);
				newData[midArray.Length] = new ModelInstanceData(modelIndex, sceneLayerIndex, initialTransform);
				return new ModelInstanceHandle(this, materialIndex, midArray.Length);
			}
		}

		public void DisposeInstance(uint materialIndex, uint instanceIndex) {
			using (RenderingModule.RenderStateBarrier.AcquirePermit(withLock: instanceMutationLock)) {
				if (isDisposed) return;
				Assure.True(MatMapContainsKey(materialIndex), "Invalid material index.");
				Assure.True(materialMap.GetNoBoxing(materialIndex).Length > instanceIndex, "Invalid instance index.");
				materialMap.GetNoBoxing(materialIndex).Data[instanceIndex].InUse = false;
			}
		}

		public ArraySlice<KeyValuePair<Material, MIDArray>> GetModelInstanceData() {
			Assure.True(RenderingModule.RenderStateBarrier.IsFrozen, "Should not request model instance data while render-state barrier is not frozen!");
			int holdingArrayIndex = 0;
			lock (instanceMutationLock) {
				if (modelInstanceDataHoldingArray.Length < materialMap.Count) modelInstanceDataHoldingArray = new KeyValuePair<Material, MIDArray>[materialMap.Count * 2];
				foreach (var kvp in Material.ShaderMaterialMap) {
					foreach (Material material in kvp.Value) {
						if (material.IsDisposed) continue;
						uint matIndex = material.Index;
						if (materialMap.ContainsKey(matIndex)) modelInstanceDataHoldingArray[holdingArrayIndex++] = new KeyValuePair<Material, MIDArray>(material, materialMap.GetNoBoxing(matIndex));
					}
				}
			}

			return new ArraySlice<KeyValuePair<Material, MIDArray>>(modelInstanceDataHoldingArray, 0U, (uint) holdingArrayIndex);
		}

		public Transform GetTransform(uint materialIndex, uint instanceIndex) {
			lock (instanceMutationLock) {
				Assure.True(MatMapContainsKey(materialIndex), "Invalid material index.");
				Assure.True(materialMap.GetNoBoxing(materialIndex).Length > instanceIndex, "Invalid instance index.");
				return materialMap.GetNoBoxing(materialIndex).Data[instanceIndex].Transform;
			}
		}

		public void SetTransform(uint materialIndex, uint instanceIndex, Transform transform) {
			using (RenderingModule.RenderStateBarrier.AcquirePermit(withLock: instanceMutationLock)) {
				Assure.True(MatMapContainsKey(materialIndex), "Invalid material index.");
				Assure.True(materialMap.GetNoBoxing(materialIndex).Length > instanceIndex, "Invalid instance index.");
				materialMap.GetNoBoxing(materialIndex).Data[instanceIndex].Transform = transform;
			}
		}

		public void Dispose() {
			using (RenderingModule.RenderStateBarrier.AcquirePermit(withLock: instanceMutationLock)) {
				foreach (KeyValuePair<uint, MIDArray> kvp in materialMap) {
					Marshal.FreeHGlobal((IntPtr) kvp.Value.Data);
				}

				materialMap.Clear();

				isDisposed = true;
			}
		}

		/// <summary>
		/// Returns a string representing this instance and its current state/values.
		/// </summary>
		/// <returns>
		/// A new string that details this object.
		/// </returns>
		public override string ToString() {
			return "MIM-" + GetHashCode().ToString("X");
		}

		private MIDArray CreateNewMIDArray(uint materialIndex) {
			uint numBytes = (uint) sizeof(ModelInstanceData) * INITIAL_INSTANCE_ALLOCATION;
			ModelInstanceData* data = (ModelInstanceData*) Marshal.AllocHGlobal(new IntPtr(numBytes));
			UnsafeUtils.ZeroMem((IntPtr) data, numBytes);
			return new MIDArray(data, INITIAL_INSTANCE_ALLOCATION);
		}
	}
}