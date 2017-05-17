// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 17 08 2015 at 11:41 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using Ophidian.Losgap;
using Ophidian.Losgap.Rendering;

namespace Egodystonic.EscapeLizards {
	public enum LizardEggMaterial {
		LizardEgg = 0,
		BilliardBall,
		BrickBall,
		CheeseBall,
		DiamondBall,
		LavaBall,
		Marble,
		Melon,
		PaintedWoodBall,
		ScalesBall,
		Snowball,
		TennisBall,
		CardboardBall,
		CouchBall,
		RopeBall,
		EmissiveBall,
		Pumpkin,
		RingBall,
		SoccerballOne,
		SoccerballTwo,
		SpiralBall,
		WireFrame
	}

	public static class LizardEggMaterialExtensions {
		private static readonly object staticMutationLock = new object();
		private static List<LizardEggMaterial> materialsOrderedByUnlockCost;

		public static Vector3 CameraIlluminationColor(this LizardEggMaterial extended) {
			switch (extended) {
				case LizardEggMaterial.ScalesBall: return new Vector3(0.6f, 1f, 0.6f) * 1.0f;
				case LizardEggMaterial.DiamondBall: return new Vector3(1f, 1f, 0.6f) * 1.3f;
				case LizardEggMaterial.Melon: return new Vector3(0.5f, 1f, 0.0f) * 2.0f;
				case LizardEggMaterial.Pumpkin: return new Vector3(1.0f, 0.5f, 0.0f) * 1.0f;
				case LizardEggMaterial.Marble: return new Vector3(0.5f, 0.0f, 1.0f) * 1.3f;
				case LizardEggMaterial.BilliardBall: return new Vector3(0.6f, 0.6f, 1.0f) * 1.3f;
				case LizardEggMaterial.SoccerballOne: return new Vector3(1f, 1f, 1f) * 2.0f;
				case LizardEggMaterial.PaintedWoodBall: return new Vector3(1.0f, 0.4f, 0.0f) * 1.0f;
				case LizardEggMaterial.TennisBall: return new Vector3(1.0f, 0.0f, 1.0f) * 0.8f;
				case LizardEggMaterial.SoccerballTwo: return new Vector3(0.0f, 1.0f, 0.0f) * 4.0f;
				case LizardEggMaterial.RingBall: return new Vector3(1.0f, 0.0f, 1.0f) * 1.5f;
				case LizardEggMaterial.SpiralBall: return new Vector3(1.0f, 0.65f, 0.0f) * 1.2f;
				case LizardEggMaterial.CheeseBall: return new Vector3(1.0f, 1.0f, 0.0f) * 2.0f;
				case LizardEggMaterial.BrickBall: return new Vector3(1f, 1f, 1f) * 1.0f;
				case LizardEggMaterial.RopeBall: return new Vector3(0.5f, 0.0f, 0.0f) * 1.0f;
				case LizardEggMaterial.EmissiveBall: return new Vector3(0.3f, 0.5f, 1.0f) * 4.0f;
				case LizardEggMaterial.Snowball: return new Vector3(1f, 1f, 1f) * 2.5f;
				case LizardEggMaterial.CardboardBall: return new Vector3(0.4f, 0.1f, 0.1f) * 2.0f;
				case LizardEggMaterial.CouchBall: return new Vector3(0.5f, 0.0f, 0.0f) * 1.2f;
				case LizardEggMaterial.LavaBall: return new Vector3(1.0f, 0.0f, 0.0f) * 4.0f;
				case LizardEggMaterial.WireFrame: return new Vector3(1.0f, 1.0f, 0.0f) * 4.0f;
				default: return new Vector3(0.6f, 1f, 0.6f) * 0.8f;
			}
		}

		public static Material Material(this LizardEggMaterial extended) {
			return AssetLocator.LizardEggMaterials[(int) extended];
		}

		public static ModelHandle Model(this LizardEggMaterial extended) {
			return AssetLocator.LizardEggModels[(int) extended];
		}

		public static int GoldenEggCost(this LizardEggMaterial extended) {
			switch (extended) {
				case LizardEggMaterial.ScalesBall: return 5;
				case LizardEggMaterial.DiamondBall: return 10;
				case LizardEggMaterial.Melon: return 15;
				case LizardEggMaterial.Pumpkin: return 20;
				case LizardEggMaterial.Marble: return 25;
				case LizardEggMaterial.BilliardBall: return 30;
				case LizardEggMaterial.SoccerballOne: return 35;
				case LizardEggMaterial.PaintedWoodBall: return 40;
				case LizardEggMaterial.TennisBall: return 45;
				case LizardEggMaterial.SoccerballTwo: return 50;
				case LizardEggMaterial.RingBall: return 55;
				case LizardEggMaterial.SpiralBall: return 60;
				case LizardEggMaterial.CheeseBall: return 65;
				case LizardEggMaterial.BrickBall: return 70;
				case LizardEggMaterial.RopeBall: return 75;
				case LizardEggMaterial.EmissiveBall: return 80;
				case LizardEggMaterial.Snowball: return 85;
				case LizardEggMaterial.CardboardBall: return 90;
				case LizardEggMaterial.CouchBall: return 95;
				case LizardEggMaterial.LavaBall: return 100;
				case LizardEggMaterial.WireFrame: return 105;
				default: return 0;
			}
		}

		public static string Title(this LizardEggMaterial extended) {
			switch (extended) {
				case LizardEggMaterial.ScalesBall: return "Gecko Egg";
				case LizardEggMaterial.DiamondBall: return "Skink Egg";
				case LizardEggMaterial.Melon: return "Fruity";
				case LizardEggMaterial.Pumpkin: return "Spooky";
				case LizardEggMaterial.Marble: return "Miss Marble";
				case LizardEggMaterial.BilliardBall: return "Right On Cue";
				case LizardEggMaterial.SoccerballOne: return "Goal!";
				case LizardEggMaterial.PaintedWoodBall: return "Strike!";
				case LizardEggMaterial.TennisBall: return "Game, Set and Match";
				case LizardEggMaterial.SoccerballTwo: return "Radioactive Striker";
				case LizardEggMaterial.RingBall: return "Springy";
				case LizardEggMaterial.SpiralBall: return "Rainbow Roller";
				case LizardEggMaterial.CheeseBall: return "Cheesy Premise";
				case LizardEggMaterial.BrickBall: return "Solid Foundations";
				case LizardEggMaterial.RopeBall: return "Ropey Old Thing";
				case LizardEggMaterial.EmissiveBall: return "Neon Dreams";
				case LizardEggMaterial.Snowball: return "Don't Throw It!";
				case LizardEggMaterial.CardboardBall: return "Don't Get Wet";
				case LizardEggMaterial.CouchBall: return "Luxury Lizard Egg";
				case LizardEggMaterial.LavaBall: return "Too Hot To Handle";
				case LizardEggMaterial.WireFrame: return "Back to Basics";
				default: return "Lizard Egg";
			}
		}

		public static List<LizardEggMaterial> MaterialsOrderedByUnlockCost {
			get {
				lock (staticMutationLock) {
					if (materialsOrderedByUnlockCost == null) {
						materialsOrderedByUnlockCost = Enum.GetValues(typeof(LizardEggMaterial))
							.Cast<LizardEggMaterial>()
							.OrderBy(lem => lem.GoldenEggCost())
							.ToList();
					}
					return materialsOrderedByUnlockCost;
				}
			}
		}
	}
}