using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionSound : Action
	{
		public enum SoundAction
		{
			Play = 0,
			FadeIn = 1,
			FadeOut = 2,
			Stop = 3
		}

		public int constantID;

		public int parameterID = -1;

		public Sound soundObject;

		public AudioClip audioClip;

		public int audioClipParameterID = -1;

		public float fadeTime;

		public bool loop;

		public bool ignoreIfPlaying;

		public SoundAction soundAction;

		public bool affectChildren;

		public bool autoEndOtherMusicWhenPlayed = true;

		protected Sound runtimeSound;

		public ActionSound()
		{
			isDisplayed = true;
			category = ActionCategory.Sound;
			title = "Play";
			description = "Triggers a Sound object to start playing. Can be used to fade sounds in or out.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			audioClip = (AudioClip)AssignObject<AudioClip>(parameters, audioClipParameterID, audioClip);
			runtimeSound = AssignFile(parameters, parameterID, constantID, soundObject);
			if (runtimeSound == null && audioClip != null)
			{
				runtimeSound = KickStarter.sceneSettings.defaultSound;
			}
		}

		public override float Run()
		{
			if (runtimeSound == null)
			{
				return 0f;
			}
			if (!isRunning)
			{
				isRunning = true;
				if (ignoreIfPlaying && (soundAction == SoundAction.Play || soundAction == SoundAction.FadeIn) && ((audioClip != null && runtimeSound.IsPlaying(audioClip)) || (audioClip == null && runtimeSound.IsPlaying())))
				{
					return 0f;
				}
				if ((bool)audioClip && (bool)runtimeSound.GetComponent<AudioSource>() && (soundAction == SoundAction.Play || soundAction == SoundAction.FadeIn))
				{
					runtimeSound.GetComponent<AudioSource>().clip = audioClip;
				}
				if (runtimeSound.soundType == SoundType.Music && autoEndOtherMusicWhenPlayed && (soundAction == SoundAction.Play || soundAction == SoundAction.FadeIn))
				{
					Sound[] array = UnityEngine.Object.FindObjectsOfType(typeof(Sound)) as Sound[];
					Sound[] array2 = array;
					foreach (Sound sound in array2)
					{
						sound.EndOld(SoundType.Music, runtimeSound);
					}
				}
				if (soundAction == SoundAction.Play)
				{
					runtimeSound.Play(loop);
					if (!loop && willWait)
					{
						return base.defaultPauseTime;
					}
				}
				else if (soundAction == SoundAction.FadeIn)
				{
					if (fadeTime <= 0f)
					{
						runtimeSound.Play(loop);
					}
					else
					{
						runtimeSound.FadeIn(fadeTime, loop);
						if (!loop && willWait)
						{
							return base.defaultPauseTime;
						}
					}
				}
				else if (soundAction == SoundAction.FadeOut)
				{
					if (fadeTime <= 0f)
					{
						runtimeSound.Stop();
					}
					else
					{
						runtimeSound.FadeOut(fadeTime);
						if (willWait)
						{
							return fadeTime;
						}
					}
				}
				else if (soundAction == SoundAction.Stop)
				{
					runtimeSound.Stop();
					if (affectChildren)
					{
						foreach (Transform item in runtimeSound.transform)
						{
							if ((bool)item.GetComponent<Sound>())
							{
								item.GetComponent<Sound>().Stop();
							}
						}
					}
				}
			}
			else
			{
				if (soundAction == SoundAction.FadeOut)
				{
					isRunning = false;
					return 0f;
				}
				if (runtimeSound.IsPlaying())
				{
					return base.defaultPauseTime;
				}
				isRunning = false;
			}
			return 0f;
		}

		public override void Skip()
		{
			if (soundAction == SoundAction.FadeOut || soundAction == SoundAction.Stop)
			{
				Run();
			}
			else if (loop)
			{
				Run();
			}
		}

		public static ActionSound CreateNew_Play(Sound sound, AudioClip newClip, float fadeDuration = 0f, bool doLoop = false, bool ignoreIfAlreadyPlaying = true, bool waitUntilFinish = false)
		{
			ActionSound actionSound = ScriptableObject.CreateInstance<ActionSound>();
			actionSound.soundAction = ((fadeDuration > 0f) ? SoundAction.FadeIn : SoundAction.Play);
			actionSound.audioClip = newClip;
			actionSound.loop = doLoop;
			actionSound.ignoreIfPlaying = ignoreIfAlreadyPlaying;
			actionSound.willWait = waitUntilFinish;
			return actionSound;
		}

		public static ActionSound CreateNew_Stop(Sound sound, float fadeDuration = 0f, bool waitUntilFinish = false)
		{
			ActionSound actionSound = ScriptableObject.CreateInstance<ActionSound>();
			actionSound.soundAction = ((!(fadeDuration > 0f)) ? SoundAction.Stop : SoundAction.FadeOut);
			actionSound.willWait = waitUntilFinish;
			return actionSound;
		}
	}
}
