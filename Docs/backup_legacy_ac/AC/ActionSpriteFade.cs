using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionSpriteFade : Action
	{
		public int constantID;

		public int parameterID = -1;

		public SpriteFader spriteFader;

		protected SpriteFader runtimeSpriteFader;

		public FadeType fadeType;

		public float fadeSpeed;

		public ActionSpriteFade()
		{
			isDisplayed = true;
			category = ActionCategory.Object;
			title = "Fade sprite";
			description = "Fades a sprite in or out.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			runtimeSpriteFader = AssignFile(parameters, parameterID, constantID, spriteFader);
		}

		public override float Run()
		{
			if (runtimeSpriteFader == null)
			{
				return 0f;
			}
			if (!isRunning)
			{
				isRunning = true;
				runtimeSpriteFader.Fade(fadeType, fadeSpeed);
				if (willWait)
				{
					return fadeSpeed;
				}
			}
			else
			{
				isRunning = false;
			}
			return 0f;
		}

		public override void Skip()
		{
			if (runtimeSpriteFader != null)
			{
				runtimeSpriteFader.Fade(fadeType, 0f);
			}
		}

		public static ActionSpriteFade CreateNew(SpriteFader spriteFaderToAffect, FadeType fadeType, float transitionTime = 1f, bool waitUntilFinish = false)
		{
			ActionSpriteFade actionSpriteFade = ScriptableObject.CreateInstance<ActionSpriteFade>();
			actionSpriteFade.spriteFader = spriteFaderToAffect;
			actionSpriteFade.fadeType = fadeType;
			actionSpriteFade.fadeSpeed = transitionTime;
			actionSpriteFade.willWait = waitUntilFinish;
			return actionSpriteFade;
		}
	}
}
