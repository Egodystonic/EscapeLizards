// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 18 06 2015 at 14:36 by Ben Bowen

using System;
using System.Collections.Generic;
using Ophidian.Losgap;

namespace Egodystonic.EscapeLizards.Editor {
	public class EditReversionStep {
		public readonly string StepDescription;
		public readonly Action ReversionAction;

		public EditReversionStep(string stepDescription, Action reversionAction) {
			Assure.NotNull(reversionAction);
			StepDescription = stepDescription;
			ReversionAction = reversionAction;
		}

		public override string ToString() {
			return StepDescription;
		}
	}
}