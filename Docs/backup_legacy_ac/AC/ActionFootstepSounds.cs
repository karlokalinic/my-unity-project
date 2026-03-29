using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionFootstepSounds : Action
	{
		public enum FootstepSoundType
		{
			Walk = 0,
			Run = 1
		}

		public int constantID;

		public int parameterID = -1;

		public FootstepSounds footstepSounds;

		protected FootstepSounds runtimeFootstepSounds;

		public bool isPlayer;

		public FootstepSoundType footstepSoundType;

		public AudioClip[] newSounds;

		public ActionFootstepSounds()
		{
			isDisplayed = true;
			category = ActionCategory.Sound;
			title = "Change footsteps";
			description = "Changes the sounds used by a FootstepSounds component.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			if (isPlayer)
			{
				if (KickStarter.player != null)
				{
					runtimeFootstepSounds = KickStarter.player.GetComponentInChildren<FootstepSounds>();
				}
			}
			else
			{
				runtimeFootstepSounds = AssignFile(parameters, parameterID, constantID, footstepSounds);
			}
		}

		public override float Run()
		{
			if (runtimeFootstepSounds == null)
			{
				LogWarning("No FootstepSounds component set.");
			}
			else if (footstepSoundType == FootstepSoundType.Walk)
			{
				runtimeFootstepSounds.footstepSounds = newSounds;
			}
			else if (footstepSoundType == FootstepSoundType.Run)
			{
				runtimeFootstepSounds.runSounds = newSounds;
			}
			return 0f;
		}

		public static ActionFootstepSounds CreateNew(FootstepSounds footstepSoundsToModify, FootstepSoundType footstepSoundType, AudioClip[] newSounds)
		{
			ActionFootstepSounds actionFootstepSounds = ScriptableObject.CreateInstance<ActionFootstepSounds>();
			actionFootstepSounds.footstepSounds = footstepSoundsToModify;
			actionFootstepSounds.footstepSoundType = footstepSoundType;
			actionFootstepSounds.newSounds = newSounds;
			return actionFootstepSounds;
		}
	}
}
