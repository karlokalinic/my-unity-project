using System;
using System.Collections.Generic;

namespace AC
{
	[Serializable]
	public class ActionCinemaDirector : Action
	{
		public bool disableCamera;

		public int constantID;

		public int parameterID = -1;

		public ActionCinemaDirector()
		{
			isDisplayed = true;
			category = ActionCategory.ThirdParty;
			title = "Cinema Director";
			description = "Runs a Cutscene built with Cinema Director. Note that Cinema Director is a separate Unity Asset, and the 'CinemaDirectorIsPresent' preprocessor must be defined for this to work.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
		}

		public override float Run()
		{
			return 0f;
		}

		public override void Skip()
		{
		}
	}
}
