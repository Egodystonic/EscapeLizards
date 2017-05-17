// All code copyright (c) 2016 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 19 10 2016 at 16:10 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;

namespace Egodystonic.EscapeLizards {
	public static class TutorialManager {
		private static readonly object staticMutationLock = new object();

		public const char TUTORIAL_TEXT_LINE_SEPARATOR = '~';

		public static string GetTutorialTitle(int tutorialIndex) {
			switch (tutorialIndex) {
				case 0: return "Movement";
				case 1: return "Stars";
				case 2: return "Coins";
				case 3: return "Golden Eggs";
				case 4: return "Hopping";
				case 5: return "Double Hop";
				case 6: return "World Flip";
				case 7: return "Medals";
				case 8: return "All At Once";
				case 9: return "Final Levels";
				default: return String.Empty;
			}
		}

		public static string GetTutorialText(int tutorialIndex) {
			switch (tutorialIndex) {
				case 0: return // Movement
					"The lizards need your help to guide their eggs to safety!"
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					String.Empty
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"Roll the LIZARD EGG from its starting point using"
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"the left analog stick on your controller or the"
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"movement keys (" + Config.Key_TiltGameboardLeft + "/" + Config.Key_TiltGameboardRight + "/" + Config.Key_TiltGameboardForward + "/" + Config.Key_TiltGameboardBackward + ")."
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					String.Empty
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"Guide the egg over the platform and touch the FINISHING BELL"
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"without falling off the edge to complete the level."
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					String.Empty
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"You can also FAST RESTART a level by pressing B on"
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"the controller or '" + Config.Key_FastRestart + "' on the keyboard."
					;
				case 1: return // Stars
					"Every level has three STARS to collect (bronze, silver, and gold)."
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					String.Empty
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"You can get better stars by getting to the finishing"
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"bell and completing the level faster."
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					String.Empty
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"The times to beat for each star are shown below"
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"the countdown clock in the upper-left corner."
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					String.Empty
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"Stars are required to unlock new worlds on the menu,"
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"so be sure to collect as many as you can!"
					;
				case 2: return // Coins
					"There are a number of LIZARD COINS scattered around"
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"on every level. Coins, like stars, are used to unlock"
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"new worlds from the play menu."
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					String.Empty
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"Every coin on every level can only ever be collected once."
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"However, you can always come back to a level you've"
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"previously played and pick up any stars or coins you've missed."
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					String.Empty
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"The level select screen on the play menu will let you"
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"see which levels have uncollected stars and coins."
					;
				case 3: return // Eggs
					String.Empty
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"The Evil Vulture Clan have left their own vulture eggs in various"
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"places in the world. Knock all of the vulture eggs off the edge"
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"before completing the level in order to earn a GOLDEN EGG."
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					String.Empty
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"Collect enough golden eggs and you can unlock cool"
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"new colours and materials for your lizard egg!"
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					String.Empty
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"Also, you can SPEED UP THE LEVEL INTRODUCTION by holding the A"
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"button on the controller or the " + Config.Key_AccelerateLevelIntroduction + " key."
					;
				case 4: return // Hop
					"On some levels, the Lizard Wizards will grant"
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"your lizard egg the power to make a number of HOPS."
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"You can use the hops to jump over gaps or find shortcuts,"
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"granting you a better overall time!"
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					String.Empty
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"Press A on the controller or the"
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					Config.Key_HopEgg + " key to use a hop."
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					String.Empty
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"You can only hop on certain levels, and you will only have a"
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"limited number of hops (indicated at the lower-right corner)."
					;
				case 5: return // Double-Hop
					"After hopping, you can press the hop button again"
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"in mid-air to do a DOUBLE HOP"
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					String.Empty
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"Press A on the controller or the"
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					Config.Key_HopEgg + " key to use a hop, and then press"
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"it again shortly after to perform a second, mid-air hop."
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					String.Empty
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"The mid-air hop can let you jump even wider gaps and"
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"maneuver your way through even better shortcuts!"
					;
				case 6: return // World flip
					"At any time on any level, you can use the Lizard Wizards'"
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"greatest magic to FLIP GRAVITY. Press the controller's"
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"right-bumper (or the " + Config.Key_FlipGameboardLeft + " key)"
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"to flip the world clockwise, and the left-bumper (or the"
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					Config.Key_FlipGameboardRight + " key) to flip it anti-clockwise."
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					String.Empty
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"You can only flip the world while the lizard egg is already"
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"rolling sufficiently fast, and you can't flip the world while"
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"falling. Use this powerful magic to roll along walls and ceilings,"
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"in order to solve complex stages and find clever shortcuts!"
					;
				case 7: return // Medals
					"For each level, your three fastest Steam friends'"
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"times are displayed in the upper-right corner, beneath"
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"your own personal best time."
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"You can get MEDALS by beating your quickest friends,"
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"landing a place on the medals table (accessible from"
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"the main menu)."
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					String.Empty
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"Although they have no in-game value, you can earn"
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"bragging rights and show off your superior lizard-skills"
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"by getting as many 1st, 2nd, and 3rd-place ranks as possible!"
					;
				case 8: return // All At Once
					"As you progress through the worlds, you'll start"
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"to encounter levels that require using all your"
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"newfound tricks and skills together!"
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					String.Empty
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"Some levels will also incorporate puzzle elements"
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"where the path to the goal or goodies such as"
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"vulture eggs is not so clear!"
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					String.Empty
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"Keep your wits about you and your eyes sharp in order"
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"to collect as many stars and coins as possible!"
					;
				case 9: return // Final Levels
					"The last level in every world will be a little trickier"
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"than what you've faced before. They will serve as a true"
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"test for the skills and knowledge you've accrued so far!"
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					String.Empty
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"That being said, there's no obligation to finish every"
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"level in order. You can always exit to the main menu and"
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"retry any level or even switch worlds at any time!"
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					String.Empty
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"Finally, you don't always have to 100% pass a world to unlock the"
					+ TUTORIAL_TEXT_LINE_SEPARATOR +
					"next, so it's okay to leave a hard level and come back later!"
					;
				default: return String.Empty;
			}
		}

		public static bool ShouldDisplayTutorial(LevelID lid) {
			if (lid.WorldIndex != 0) return false;
			if (GetTutorialText(lid.LevelIndex) == String.Empty) return false;
			if (PersistedWorldData.GetBestTimeForLevel(lid) != null) return false;

			return true;
		}
	}
}