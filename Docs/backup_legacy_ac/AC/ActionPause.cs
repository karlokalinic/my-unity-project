using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionPause : Action
	{
		public int parameterID = -1;

		public float timeToPause;

		public ActionPause()
		{
			isDisplayed = true;
			category = ActionCategory.Engine;
			title = "Wait";
			description = "Waits a set time before continuing.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			timeToPause = AssignFloat(parameters, parameterID, timeToPause);
		}

		public override float Run()
		{
			if (!isRunning)
			{
				isRunning = true;
				if (timeToPause < 0f)
				{
					return base.defaultPauseTime;
				}
				return timeToPause;
			}
			isRunning = false;
			return 0f;
		}

		public static ActionPause CreateNew(float waitTime)
		{
			ActionPause actionPause = ScriptableObject.CreateInstance<ActionPause>();
			actionPause.timeToPause = waitTime;
			return actionPause;
		}
	}
}
