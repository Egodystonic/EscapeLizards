// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 12 08 2015 at 16:21 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using Ophidian.Losgap;
using Ophidian.Losgap.Entities;

namespace Egodystonic.EscapeLizards {
	public static class GameplayConstants {
		#region Camera
		/* The camera's 'target position' (i.e. where the camera is always moving towards) is a point in space that is
		 * eggPosition - eggVelocity.WithLength(CAMERA_MIN_VELOCITY_BASED_DIST_TO_EGG) + CAMERA_HEIGHT_ADDITION
		 * When the egg is falling (or moving upwards) then CAMERA_FALLING_HEIGHT_EXTRA is added to the height
		 * 
		 * The Config options CameraHeightAdjust and CameraDistanceAdjust affect this calculation also
		 */
		public const float CAMERA_MIN_VELOCITY_BASED_DIST_TO_EGG = PhysicsManager.ONE_METRE_SCALED; // See note above
		public const float CAMERA_HEIGHT_ADDITION = PhysicsManager.ONE_METRE_SCALED * 1f; // See note above
		public const float CAMERA_FALLING_HEIGHT_EXTRA = PhysicsManager.ONE_METRE_SCALED * 2f; // See note above
		private const float MIN_EGG_SPEED_FOR_VELO_BASED_CAM = PhysicsManager.ONE_METRE_SCALED * 0.07f; // If egg velocity is lower than this value, the camera target is set according to the direction the camera is already looking rather than using the egg's velocity
		private const float MIN_FALL_SPEED_FOR_FALLING_CAM = PhysicsManager.ONE_METRE_SCALED * 3f; // Egg falling/rising faster than this speed makes the camera target calculation use CAMERA_FALLING_HEIGHT_EXTRA instead of CAMERA_HEIGHT_ADDITION
		public const float CAM_MOVE_SPEED_EXPONENT_LINEAR = 0.75f; // Affects how fast the camera moves towards the target position. 1f = always 1 second behind; but number is an exponent (so 0.5f != always 2 seconds behind).
		public const float CAM_MOVE_SPEED_ANGULAR = 6f; // Affects how fast the camera 'boom' can swing around the egg if the target position suddenly goes the other side of it. 1f = always 1 second behind. 2f = always 0.5 seconds behind. etc.
		public const float CAM_ORIENT_SPEED = 6.0f; // Affects how fast the camera re-orients its view to be looking at the egg. 1f = always 1 second behind. 2f = always 0.5 seconds behind. etc.
		public const float CAM_BOARD_TILT_FACTOR = 0.5f; // Alters how much the board tilting affects the target position. 1f = target position is tilted (wrt current pos) same amount as the board, 0.5f = half as much, etc.
		public const float CAM_RS_ALTERATION_FACTOR = 0.05f; // Amount the right-stick alters the camera (1f = full-torque instantly rotates 180)
		public const float OBSCURING_GEOM_MAX_ALPHA = 0.8f;
		public const float OBSCURING_GEOM_MIN_ALPHA = 0.3f;
		public const float OBSCURING_GEOM_ALPHA_REGAIN_SPEED = 6f;
		public const float OBSCURING_GEOM_BRIGHTNESS = 0.4f;
		public const float CAM_UP_SUBROT_PER_SEC = 12f;
		public const float MAX_CAM_ORIENTATION_ADJUSTMENT_X = 0.3f;
		public const float MAX_CAM_ORIENTATION_ADJUSTMENT_Y = 0.2f;
		public const float CAM_ORIENTATION_ADJUSTMENT_DELTA_FRAC_PER_SEC_X = 1.75f;
		public const float CAM_ORIENTATION_ADJUSTMENT_DELTA_FRAC_PER_SEC_X_STOPPING = 6.5f;
		public const float CAM_ORIENTATION_ADJUSTMENT_DELTA_ABS_MIN_PER_SEC_X = 0.075f;
		public const float CAM_ORIENTATION_ADJUSTMENT_DELTA_FRAC_PER_SEC_Y = 1f;
		public const float CAM_ORIENTATION_ADJUSTMENT_DELTA_FRAC_PER_SEC_Y_STOPPING = 5f;
		public const float CAM_ORIENTATION_ADJUSTMENT_DELTA_ABS_MIN_PER_SEC_Y = 0.025f;
		public const float CAM_ADJUSTMENT_TRANSLATION_MAX_X = PhysicsManager.ONE_METRE_SCALED * 0.65f;
		public const float CAM_ADJUSTMENT_TRANSLATION_MAX_Y = PhysicsManager.ONE_METRE_SCALED * 0.5f;
		public const float CAM_Y_ADJUSTMENT_HORIZONTAL_BUFFER_RADIANS_BOTTOM = MathUtils.PI_OVER_TWO * 0.35f;
		public const float CAM_Y_ADJUSTMENT_HORIZONTAL_BUFFER_RADIANS_TOP = MathUtils.PI_OVER_TWO * 0.05f;

		public const float MIN_EGG_SPEED_FOR_VELO_BASED_CAM_SQ = MIN_EGG_SPEED_FOR_VELO_BASED_CAM * MIN_EGG_SPEED_FOR_VELO_BASED_CAM;
		public const float MIN_FALL_SPEED_FOR_FALLING_CAM_SQ = MIN_FALL_SPEED_FOR_FALLING_CAM * MIN_FALL_SPEED_FOR_FALLING_CAM;
		#endregion

		#region Level Intro
		public const float INTRO_EGG_SPAWN_TIME = 0.7f;
		public const float EGG_SPAWN_HEIGHT = PhysicsManager.ONE_METRE_SCALED * 1f;
		#endregion

		#region Egg Properties
		public const float EGG_COLLISION_RADIUS = PhysicsManager.ONE_METRE_SCALED * 0.08f;
		public const float MIN_SPEED_FOR_EGG_CCD = PhysicsManager.ONE_METRE_SCALED * 0.01f;
		public const float EGG_MASS = 1.0f;
		public const float VULTURE_EGG_MASS = EGG_MASS * 0.25f;
		public const float EGG_RESTITUTION = 0.1f;
		public const float EGG_DAMPING_LINEAR = 0.3f;
		public const float EGG_DAMPING_ANGULAR = 0.9f; // Angular rotation is 'faked', gg bullet
		public const float EGG_FRICTION = 0.3f;
		public const float EGG_FRICTION_ROLLING = 0.9f;
		public const float EGG_ILLUMINATION_RADIUS = PhysicsManager.ONE_METRE_SCALED * 0.8f;
		private const float EGG_LIGHT_BUFFER_MULTIPLIER = 0.4f;
		public const float VULTURE_EGG_COLLISION_RADIUS_MULTIPLIER = 1.2f;
		public const float EGG_FLOAT_TEST_RADIUS = EGG_COLLISION_RADIUS * 1.1f;
		public const float EGG_FLOAT_BOOST_FORCE_MAX = PhysicsManager.ONE_METRE_SCALED * 3f * 60f;
		public const float EGG_MAX_SPEED_FOR_BOOST = PhysicsManager.ONE_METRE_SCALED * 5f;
		public const float EGG_FLOAT_ANTIGRAVITY_STRENGTH = 0.05f;
		public const float DIAGONAL_MOTION_FLOAT_BOOST_MULTIPLIER = 0.75f;

		public const float VULTURE_EGG_DISTANCE_FROM_GEOM = EGG_COLLISION_RADIUS * VULTURE_EGG_COLLISION_RADIUS_MULTIPLIER * 1f;
		public const float EGG_LIGHT_HEIGHT = EGG_COLLISION_RADIUS * 2f * EGG_LIGHT_BUFFER_MULTIPLIER;
		public const float EGG_COLLISION_RADIUS_SQUARED = EGG_COLLISION_RADIUS * EGG_COLLISION_RADIUS;
		#endregion

		#region Gameplay
		public const float GRAVITY_ACCELERATION = 9.81f * PhysicsManager.ONE_METRE_SCALED;
		private const float MIN_SPEED_FOR_BOARD_FLIP = 0.1f * PhysicsManager.ONE_METRE_SCALED;
		public const float MAX_PERMISSIBLE_VERTICAL_VELO_FRACTION_FOR_FLIP = 0.65f;
		public const float BOARD_FLIP_TIME = 0.55f;
		public const float MULTIPLE_BOARD_FLIP_DELAY = 0.4f;
		public const float BOARD_TILT_RADIANS = MathUtils.TWO_PI * 0.05f;
		public const float BOARD_TILT_SPEED_RADIANS = BOARD_TILT_RADIANS * 40f;
		public const float FALL_OUT_RADIUS_BUFFER = PhysicsManager.ONE_METRE_SCALED * 2f;
		public const float BELL_TOP_DISTANCE = 10f;
		public const float EGG_HOP_LATERAL_FORCE = PhysicsManager.ONE_METRE_SCALED * 2f * EGG_MASS;
		public const float EGG_HOP_VERTICAL_FORCE = PhysicsManager.ONE_METRE_SCALED * 3f * EGG_MASS;
		public const float EGG_HOP_RESET_MAX_DIST = EGG_COLLISION_RADIUS * 1.1f;

		public const float MIN_SPEED_FOR_BOARD_FLIP_SQUARED = MIN_SPEED_FOR_BOARD_FLIP * MIN_SPEED_FOR_BOARD_FLIP;
		#endregion

		#region Level Pass / Fail
		public const float STAR_SPIN = MathUtils.PI;
		#endregion

		#region Sound
		public const float EGG_SPEED_FOR_MIN_ROLL_FREQ = PhysicsManager.ONE_METRE_SCALED * 0.1f;
		public const float EGG_SPEED_FOR_MAX_ROLL_FREQ = PhysicsManager.ONE_METRE_SCALED * 12f;
		public const float EGG_SPEED_FOR_MIN_ROLL_VOL = PhysicsManager.ONE_METRE_SCALED * 1.4f;
		public const float EGG_SPEED_FOR_MAX_ROLL_VOL = PhysicsManager.ONE_METRE_SCALED * 5f;
		#endregion
	}
}