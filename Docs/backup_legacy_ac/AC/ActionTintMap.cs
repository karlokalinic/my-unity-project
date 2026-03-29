using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionTintMap : Action
	{
		public bool isPlayer;

		public FollowTintMap followTintMap;

		public int followTintMapConstantID;

		public int followTintMapParameterID = -1;

		protected FollowTintMap runtimeFollowTintMap;

		public TintMapMethod tintMapMethod;

		public float newIntensity = 1f;

		public bool isInstant = true;

		public float timeToChange;

		public bool followDefault;

		public TintMap newTintMap;

		public int newTintMapConstantID;

		public int newTintMapParameterID = -1;

		protected TintMap runtimeNewTintMap;

		public ActionTintMap()
		{
			isDisplayed = true;
			category = ActionCategory.Object;
			title = "Change Tint map";
			description = "Changes which Tint map a Follow Tint Map component uses, and the intensity of the effect.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			if (isPlayer)
			{
				if ((bool)KickStarter.player && KickStarter.player.spriteChild != null && (bool)KickStarter.player.spriteChild.GetComponent<FollowTintMap>())
				{
					runtimeFollowTintMap = KickStarter.player.spriteChild.GetComponent<FollowTintMap>();
				}
			}
			else
			{
				runtimeFollowTintMap = AssignFile(parameters, followTintMapParameterID, followTintMapConstantID, followTintMap);
			}
			if (tintMapMethod == TintMapMethod.ChangeTintMap && !followDefault)
			{
				runtimeNewTintMap = AssignFile(parameters, newTintMapParameterID, newTintMapConstantID, newTintMap);
			}
			if (timeToChange < 0f)
			{
				timeToChange = 0f;
			}
		}

		public override float Run()
		{
			if (runtimeFollowTintMap == null)
			{
				if (isPlayer)
				{
					LogWarning("Could not find a FollowTintMap component on the Player - be sure to place one on the sprite child.");
				}
				return 0f;
			}
			if (!isRunning)
			{
				isRunning = true;
				if (tintMapMethod == TintMapMethod.ChangeIntensity)
				{
					if (isInstant || timeToChange <= 0f)
					{
						runtimeFollowTintMap.SetIntensity(newIntensity);
					}
					else
					{
						runtimeFollowTintMap.SetIntensity(newIntensity, timeToChange);
						if (willWait)
						{
							return timeToChange;
						}
					}
				}
				else if (tintMapMethod == TintMapMethod.ChangeTintMap)
				{
					if (followDefault)
					{
						runtimeFollowTintMap.useDefaultTintMap = true;
						runtimeFollowTintMap.ResetTintMap();
					}
					else if ((bool)runtimeNewTintMap)
					{
						runtimeFollowTintMap.useDefaultTintMap = false;
						runtimeFollowTintMap.tintMap = runtimeNewTintMap;
						runtimeFollowTintMap.ResetTintMap();
					}
					else
					{
						LogWarning("Could not change " + runtimeFollowTintMap.gameObject.name + " - no alternative provided!");
					}
				}
			}
			else
			{
				isRunning = false;
			}
			return 0f;
		}

		public static ActionTintMap CreateNew_ChangeTintMap(FollowTintMap followTintMap, TintMap newTintMap)
		{
			ActionTintMap actionTintMap = ScriptableObject.CreateInstance<ActionTintMap>();
			actionTintMap.tintMapMethod = TintMapMethod.ChangeTintMap;
			actionTintMap.followTintMap = followTintMap;
			actionTintMap.newTintMap = newTintMap;
			return actionTintMap;
		}

		public static ActionTintMap CreateNew_ChangeIntensity(FollowTintMap followTintMap, float newIntensity, float transitionTime = 0f, bool waitUntilFinish = false)
		{
			ActionTintMap actionTintMap = ScriptableObject.CreateInstance<ActionTintMap>();
			actionTintMap.tintMapMethod = TintMapMethod.ChangeIntensity;
			actionTintMap.followTintMap = followTintMap;
			actionTintMap.newIntensity = newIntensity;
			actionTintMap.timeToChange = transitionTime;
			actionTintMap.isInstant = transitionTime <= 0f;
			actionTintMap.willWait = waitUntilFinish;
			return actionTintMap;
		}
	}
}
