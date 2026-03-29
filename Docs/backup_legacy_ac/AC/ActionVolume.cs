using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionVolume : Action
	{
		public int constantID;

		public int parameterID = -1;

		public Sound soundObject;

		protected Sound runtimeSoundObject;

		public float newRelativeVolume = 1f;

		public int newRelativeVolumeParameterID = -1;

		public float changeTime;

		public int changeTimeParameterID = -1;

		public ActionVolume()
		{
			isDisplayed = true;
			category = ActionCategory.Sound;
			title = "Change volume";
			description = "Alters the 'relative volume' of any Sound object.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			runtimeSoundObject = AssignFile(parameters, parameterID, constantID, soundObject);
			newRelativeVolume = AssignFloat(parameters, newRelativeVolumeParameterID, newRelativeVolume);
			changeTime = AssignFloat(parameters, changeTimeParameterID, changeTime);
		}

		public override float Run()
		{
			if (!isRunning)
			{
				if (runtimeSoundObject != null)
				{
					runtimeSoundObject.ChangeRelativeVolume(newRelativeVolume, changeTime);
					if (willWait && changeTime > 0f)
					{
						isRunning = true;
						return changeTime;
					}
				}
			}
			else
			{
				isRunning = false;
			}
			return 0f;
		}

		public static ActionVolume CreateNew(Sound sound, float newRelativeVolume, float transitionTime = 0.5f, bool waitUntilFinish = false)
		{
			ActionVolume actionVolume = ScriptableObject.CreateInstance<ActionVolume>();
			actionVolume.soundObject = sound;
			actionVolume.newRelativeVolume = newRelativeVolume;
			actionVolume.changeTime = transitionTime;
			actionVolume.willWait = waitUntilFinish;
			return actionVolume;
		}
	}
}
