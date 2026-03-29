using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionSoundShot : Action
	{
		public int constantID;

		public int parameterID = -1;

		public Transform origin;

		protected Transform runtimeOrigin;

		public AudioClip audioClip;

		public int audioClipParameterID = -1;

		public ActionSoundShot()
		{
			isDisplayed = true;
			category = ActionCategory.Sound;
			title = "Play one-shot";
			description = "Plays an AudioClip once without the need for a Sound object.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			runtimeOrigin = AssignFile(parameters, parameterID, constantID, origin);
			audioClip = (AudioClip)AssignObject<AudioClip>(parameters, audioClipParameterID, audioClip);
		}

		public override float Run()
		{
			if (audioClip == null)
			{
				return 0f;
			}
			if (!isRunning)
			{
				isRunning = true;
				Vector3 position = KickStarter.CameraMain.transform.position;
				if (runtimeOrigin != null)
				{
					position = runtimeOrigin.position;
				}
				float sFXVolume = Options.GetSFXVolume();
				AudioSource.PlayClipAtPoint(audioClip, position, sFXVolume);
				if (willWait)
				{
					return audioClip.length;
				}
			}
			isRunning = false;
			return 0f;
		}

		public override void Skip()
		{
			if (audioClip == null)
			{
				return;
			}
			AudioSource[] array = UnityEngine.Object.FindObjectsOfType(typeof(AudioSource)) as AudioSource[];
			AudioSource[] array2 = array;
			foreach (AudioSource audioSource in array2)
			{
				if (audioSource.clip == audioClip && audioSource.isPlaying && audioSource.GetComponent<Sound>() == null)
				{
					audioSource.Stop();
					break;
				}
			}
		}

		public static ActionSoundShot CreateNew(AudioClip clipToPlay, Transform origin = null, bool waitUntilFinish = false)
		{
			ActionSoundShot actionSoundShot = ScriptableObject.CreateInstance<ActionSoundShot>();
			actionSoundShot.audioClip = clipToPlay;
			actionSoundShot.origin = origin;
			actionSoundShot.willWait = waitUntilFinish;
			return actionSoundShot;
		}
	}
}
