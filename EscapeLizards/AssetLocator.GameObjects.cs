// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 13 07 2015 at 17:32 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using Ophidian.Losgap.Entities;
using Ophidian.Losgap.Rendering;

namespace Egodystonic.EscapeLizards {
	public static partial class AssetLocator {
		private static ModelHandle startFlagModel;
		private static Material startFlagMaterial;
		private static ModelHandle finishingBellModel;
		private static Material finishingBellMaterial;
		private static ModelHandle finishingBellTopModel;
		private static Material finishingBellTopMaterial;
		private static ModelHandle dynamicLightModel;
		private static Material dynamicLightMaterial;
		private static ModelHandle introCameraModel;
		private static Material introCameraMaterial;
		private static ModelHandle vultureEggModel;
		private static Material vultureEggMaterial;
		private static ModelHandle[] lizardEggModels;
		private static Material[] lizardEggMaterials;
		private static PhysicsShapeHandle lizardEggPhysicsShape;
		private static PhysicsShapeHandle finishingBellPhysicsShape;
		private static PhysicsShapeHandle finishingBellTopPhysicsShape;
		private static PhysicsShapeHandle vultureEggPhysicsShape;
		private static ModelHandle[] sillyBitsHandles;
		private static Material[] sillyBitsMaterials;
		private static PhysicsShapeHandle[] sillyBitsPhysicsShapes;
		private static ModelHandle starModel;
		private static Material starMaterialGold;
		private static Material starMaterialSilver;
		private static Material starMaterialBronze;
		private static PhysicsShapeHandle starPhysicsShape;
		private static ModelHandle twistedArrowModel;
		private static Material twistedArrowMaterial;
		private static ModelHandle lizardCoinModel;
		private static Material lizardCoinMaterial;
		private static Material lizardCoinAlphaMaterial;
		private static PhysicsShapeHandle lizardCoinShape;
		private static ModelHandle worldFlipArrowModel;
		private static ModelHandle shadowcasterModel;
		private static Material shadowcasterMaterial;

		public static ModelHandle StartFlagModel {
			get {
				lock (staticMutationLock) {
					return startFlagModel;
				}
			}
			set {
				lock (staticMutationLock) {
					startFlagModel = value;
				}
			}
		}
		public static Material StartFlagMaterial {
			get {
				lock (staticMutationLock) {
					return startFlagMaterial;
				}
			}
			set {
				lock (staticMutationLock) {
					startFlagMaterial = value;
				}
			}
		}

		public static ModelHandle FinishingBellModel {
			get {
				lock (staticMutationLock) {
					return finishingBellModel;
				}
			}
			set {
				lock (staticMutationLock) {
					finishingBellModel = value;
				}
			}
		}
		public static Material FinishingBellMaterial {
			get {
				lock (staticMutationLock) {
					return finishingBellMaterial;
				}
			}
			set {
				lock (staticMutationLock) {
					finishingBellMaterial = value;
				}
			}
		}
		public static ModelHandle FinishingBellTopModel {
			get {
				lock (staticMutationLock) {
					return finishingBellTopModel;
				}
			}
			set {
				lock (staticMutationLock) {
					finishingBellTopModel = value;
				}
			}
		}
		public static Material FinishingBellTopMaterial {
			get {
				lock (staticMutationLock) {
					return finishingBellTopMaterial;
				}
			}
			set {
				lock (staticMutationLock) {
					finishingBellTopMaterial = value;
				}
			}
		}

		public static ModelHandle DynamicLightModel {
			get {
				lock (staticMutationLock) {
					return dynamicLightModel;
				}
			}
			set {
				lock (staticMutationLock) {
					dynamicLightModel = value;
				}
			}
		}
		public static Material DynamicLightMaterial {
			get {
				lock (staticMutationLock) {
					return dynamicLightMaterial;
				}
			}
			set {
				lock (staticMutationLock) {
					dynamicLightMaterial = value;
				}
			}
		}

		public static ModelHandle IntroCameraModel {
			get {
				lock (staticMutationLock) {
					return introCameraModel;
				}
			}
			set {
				lock (staticMutationLock) {
					introCameraModel = value;
				}
			}
		}
		public static Material IntroCameraMaterial {
			get {
				lock (staticMutationLock) {
					return introCameraMaterial;
				}
			}
			set {
				lock (staticMutationLock) {
					introCameraMaterial = value;
				}
			}
		}

		public static ModelHandle VultureEggModel {
			get {
				lock (staticMutationLock) {
					return vultureEggModel;
				}
			}
			set {
				lock (staticMutationLock) {
					vultureEggModel = value;
				}
			}
		}
		public static Material VultureEggMaterial {
			get {
				lock (staticMutationLock) {
					return vultureEggMaterial;
				}
			}
			set {
				lock (staticMutationLock) {
					vultureEggMaterial = value;
				}
			}
		}

		public static ModelHandle[] LizardEggModels {
			get {
				lock (staticMutationLock) {
					return lizardEggModels;
				}
			}
			set {
				lock (staticMutationLock) {
					lizardEggModels = value;
				}
			}
		}
		public static Material[] LizardEggMaterials {
			get {
				lock (staticMutationLock) {
					return lizardEggMaterials;
				}
			}
			set {
				lock (staticMutationLock) {
					lizardEggMaterials = value;
				}
			}
		}
		public static PhysicsShapeHandle LizardEggPhysicsShape {
			get {
				lock (staticMutationLock) {
					return lizardEggPhysicsShape;
				}
			}
			set {
				lock (staticMutationLock) {
					lizardEggPhysicsShape = value;
				}
			}
		}

		public static PhysicsShapeHandle FinishingBellPhysicsShape {
			get {
				lock (staticMutationLock) {
					return finishingBellPhysicsShape;
				}
			}
			set {
				lock (staticMutationLock) {
					finishingBellPhysicsShape = value;
				}
			}
		}

		public static PhysicsShapeHandle FinishingBellTopPhysicsShape {
			get {
				lock (staticMutationLock) {
					return finishingBellTopPhysicsShape;
				}
			}
			set {
				lock (staticMutationLock) {
					finishingBellTopPhysicsShape = value;
				}
			}
		}

		public static PhysicsShapeHandle VultureEggPhysicsShape {
			get {
				lock (staticMutationLock) {
					return vultureEggPhysicsShape;
				}
			}
			set {
				lock (staticMutationLock) {
					vultureEggPhysicsShape = value;
				}
			}
		}

		public static ModelHandle[] SillyBitsHandles {
			get {
				lock (staticMutationLock) {
					return sillyBitsHandles;
				}
			}
			set {
				lock (staticMutationLock) {
					sillyBitsHandles = value;
				}
			}
		}

		public static Material[] SillyBitsMaterials {
			get {
				lock (staticMutationLock) {
					return sillyBitsMaterials;
				}
			}
			set {
				lock (staticMutationLock) {
					sillyBitsMaterials = value;
				}
			}
		}

		public static PhysicsShapeHandle[] SillyBitsPhysicsShapes {
			get {
				lock (staticMutationLock) {
					return sillyBitsPhysicsShapes;
				}
			}
			set {
				lock (staticMutationLock) {
					sillyBitsPhysicsShapes = value;
				}
			}
		}

		public static ModelHandle StarModel {
			get {
				lock (staticMutationLock) {
					return starModel;
				}
			}
			set {
				lock (staticMutationLock) {
					starModel = value;
				}
			}
		}

		public static Material StarMaterialGold {
			get {
				lock (staticMutationLock) {
					return starMaterialGold;
				}
			}
			set {
				lock (staticMutationLock) {
					starMaterialGold = value;
				}
			}
		}

		public static Material StarMaterialSilver {
			get {
				lock (staticMutationLock) {
					return starMaterialSilver;
				}
			}
			set {
				lock (staticMutationLock) {
					starMaterialSilver = value;
				}
			}
		}

		public static Material StarMaterialBronze {
			get {
				lock (staticMutationLock) {
					return starMaterialBronze;
				}
			}
			set {
				lock (staticMutationLock) {
					starMaterialBronze = value;
				}
			}
		}

		public static PhysicsShapeHandle StarPhysicsShape {
			get {
				lock (staticMutationLock) {
					return starPhysicsShape;
				}
			}
			set {
				lock (staticMutationLock) {
					starPhysicsShape = value;
				}
			}
		}

		public static ModelHandle TwistedArrowModel {
			get {
				lock (staticMutationLock) {
					return twistedArrowModel;
				}
			}
			set {
				lock (staticMutationLock) {
					twistedArrowModel = value;
				}
			}
		}

		public static Material TwistedArrowMaterial {
			get {
				lock (staticMutationLock) {
					return twistedArrowMaterial;
				}
			}
			set {
				lock (staticMutationLock) {
					twistedArrowMaterial = value;
				}
			}
		}

		public static ModelHandle LizardCoinModel {
			get {
				lock (staticMutationLock) {
					return lizardCoinModel;
				}
			}
			set {
				lock (staticMutationLock) {
					lizardCoinModel = value;
				}
			}
		}

		public static Material LizardCoinMaterial {
			get {
				lock (staticMutationLock) {
					return lizardCoinMaterial;
				}
			}
			set {
				lock (staticMutationLock) {
					lizardCoinMaterial = value;
				}
			}
		}

		public static Material LizardCoinAlphaMaterial {
			get {
				lock (staticMutationLock) {
					return lizardCoinAlphaMaterial;
				}
			}
			set {
				lock (staticMutationLock) {
					lizardCoinAlphaMaterial = value;
				}
			}
		}

		public static PhysicsShapeHandle LizardCoinShape {
			get {
				lock (staticMutationLock) {
					return lizardCoinShape;
				}
			}
			set {
				lock (staticMutationLock) {
					lizardCoinShape = value;
				}
			}
		}

		public static ModelHandle WorldFlipArrowModel {
			get {
				lock (staticMutationLock) {
					return worldFlipArrowModel;
				}
			}
			set {
				lock (staticMutationLock) {
					worldFlipArrowModel = value;
				}
			}
		}

		public static ModelHandle ShadowcasterModel {
			get {
				lock (staticMutationLock) {
					return shadowcasterModel;
				}
			}
			set {
				lock (staticMutationLock) {
					shadowcasterModel = value;
				}
			}
		}

		public static Material ShadowcasterMaterial {
			get {
				lock (staticMutationLock) {
					return shadowcasterMaterial;
				}
			}
			set {
				lock (staticMutationLock) {
					shadowcasterMaterial = value;
				}
			}
		}
	}
}