// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 22 06 2015 at 12:18 by Ben Bowen

using System;
using System.Collections.Generic;
using System.Linq;

namespace Ophidian.Losgap.Entities {
	public class PresetMovementEntity : GeometryEntity {
		private const float X_AXIS_SMOOTHING_EXPANSION_FACTOR = 5.6539f;
		private const float ASYMPTOTIC_CORRECTION = 0.68f;
		private readonly LevelEntityMovementStep[] movementSteps;
		private readonly bool alternateMovementDirection; // Alternate as in "to alternate", not "alternative"
		private readonly float initialDelay;
		private bool currentlyInReverse = false;
		private int targetStepIndex = 1;
		private int startingStepIndex = 0;
		private float timeInToCurrentStep = 0f;
		private float initialDelayTimeRemaining = 0f;
		private LevelEntityMovementStep pausedStep = null;
		private readonly Transform[] tiltedTransforms;

		public PresetMovementEntity(LevelEntityMovementStep[] movementSteps, bool alternateMovementDirection, float initialDelay) {
			Assure.GreaterThan(movementSteps.Length, 1);
			this.movementSteps = movementSteps;
			this.alternateMovementDirection = alternateMovementDirection;
			initialDelayTimeRemaining = this.initialDelay = initialDelay;
			tiltedTransforms = new Transform[movementSteps.Length];
			for (int i = 0; i < movementSteps.Length; i++) {
				tiltedTransforms[i] = movementSteps[i].Transform;
			}
		}

		public void Pause(LevelEntityMovementStep step = null) {
			step = step ?? movementSteps[0];
			lock (InstanceMutationLock) {
				pausedStep = step;
				Transform = step.Transform;
				if (step == movementSteps[0]) initialDelayTimeRemaining = initialDelay;
			}
		}

		public IReadOnlyList<LevelEntityMovementStep> MovementSteps {
			get {
				return movementSteps;
			}
		}

		public void Unpause() {
			lock (InstanceMutationLock) {
				if (pausedStep == null) return;
				targetStepIndex = movementSteps.IndexOf(pausedStep) + 1;
				if (targetStepIndex == movementSteps.Length) targetStepIndex = 0;
				startingStepIndex = targetStepIndex - 1;
				if (startingStepIndex < 0) startingStepIndex = movementSteps.Length - 1;
				timeInToCurrentStep = 0f;
				pausedStep = null;
			}
		}

		public void Tilt(Vector3 pivotPoint, Quaternion tilt) {
			lock (InstanceMutationLock) {
				for (int i = 0; i < tiltedTransforms.Length; i++) {
					tiltedTransforms[i] = tiltedTransforms[i].RotateAround(pivotPoint, tilt);	
				}

				RotateAround(pivotPoint, tilt);
			}
		}

		public void AdvanceMovement(float deltaTime) {
			lock (InstanceMutationLock) {
				if (pausedStep != null) return;

				if (initialDelayTimeRemaining > 0f) {
					if (initialDelayTimeRemaining >= deltaTime) {
						initialDelayTimeRemaining -= deltaTime;
						return;
					}
					else {
						deltaTime -= initialDelayTimeRemaining;
						initialDelayTimeRemaining = 0f;
					}
				}

				timeInToCurrentStep += deltaTime;

				LevelEntityMovementStep targetStep = movementSteps[targetStepIndex];
				while (timeInToCurrentStep > targetStep.TravelTime) {
					timeInToCurrentStep -= targetStep.TravelTime;
					startingStepIndex = targetStepIndex;

					if (alternateMovementDirection) {
						if (currentlyInReverse) {
							targetStepIndex = --targetStepIndex;
							if (targetStepIndex == -1) {
								targetStepIndex = 1;
								currentlyInReverse = false;
							}
						}
						else {
							targetStepIndex = ++targetStepIndex;
							if (targetStepIndex == movementSteps.Length) {
								targetStepIndex = movementSteps.Length - 2;
								currentlyInReverse = true;
							}
						}
					}
					else targetStepIndex = ++targetStepIndex % movementSteps.Length;

					targetStep = movementSteps[targetStepIndex];
				}

				if (targetStep.SmoothTransition) {
					Transform = Transform.Slerp(
						tiltedTransforms[startingStepIndex],
						tiltedTransforms[targetStepIndex],
						(float) Math.Atan((timeInToCurrentStep / targetStep.TravelTime - 0.5f) * X_AXIS_SMOOTHING_EXPANSION_FACTOR) / (MathUtils.PI - ASYMPTOTIC_CORRECTION) + 0.5f
					);
				}
				else {
					Transform = Transform.Lerp(
						tiltedTransforms[startingStepIndex],
						tiltedTransforms[targetStepIndex],
						timeInToCurrentStep / targetStep.TravelTime
					);
				}
			}
		}

		protected override void Tick(float deltaTimeSeconds) {
			base.Tick(deltaTimeSeconds);
			if (!EntityModule.PausePhysics) AdvanceMovement(deltaTimeSeconds);
		}
	}
}