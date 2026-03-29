using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionTimescale : Action
	{
		public float timeScale;

		public int parameterID = -1;

		public bool useTimeCurve;

		public AnimationCurve timeCurve;

		public ActionTimescale()
		{
			isDisplayed = true;
			category = ActionCategory.Engine;
			title = "Change timescale";
			description = "Changes the timescale to a value between 0 and 1. This allows for slow-motion effects.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			timeScale = AssignFloat(parameters, parameterID, timeScale);
			if (timeScale < 0f)
			{
				timeScale = 0f;
			}
		}

		public override float Run()
		{
			if (!isRunning)
			{
				isRunning = true;
				if (useTimeCurve)
				{
					if (timeCurve != null)
					{
						KickStarter.playerInput.SetTimeCurve(timeCurve);
						if (willWait)
						{
							return base.defaultPauseTime;
						}
					}
				}
				else if (timeScale > 0f)
				{
					KickStarter.playerInput.SetTimeScale(timeScale);
				}
				else
				{
					LogWarning("Cannot set timescale to zero!");
				}
			}
			else
			{
				if (KickStarter.playerInput.HasTimeCurve())
				{
					return base.defaultPauseTime;
				}
				isRunning = false;
			}
			return 0f;
		}

		public static ActionTimescale CreateNew(float newTimeScale)
		{
			ActionTimescale actionTimescale = ScriptableObject.CreateInstance<ActionTimescale>();
			actionTimescale.useTimeCurve = false;
			actionTimescale.timeScale = newTimeScale;
			return actionTimescale;
		}

		public static ActionTimescale CreateNew(AnimationCurve newTimeCurve, bool waitUntilFinish = false)
		{
			ActionTimescale actionTimescale = ScriptableObject.CreateInstance<ActionTimescale>();
			actionTimescale.useTimeCurve = true;
			actionTimescale.timeCurve = newTimeCurve;
			actionTimescale.willWait = waitUntilFinish;
			return actionTimescale;
		}
	}
}
