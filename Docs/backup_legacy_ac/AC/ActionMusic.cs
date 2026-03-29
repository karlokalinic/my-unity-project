using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionMusic : Action
	{
		public int trackID;

		public int trackIDParameterID = -1;

		public float fadeTime;

		public int fadeTimeParameterID = -1;

		public bool loop;

		public bool isQueued;

		public bool resumeFromStart = true;

		public bool resumeIfPlayedBefore;

		public bool willWaitComplete;

		public MusicAction musicAction;

		protected Music music;

		public float loopingOverlapTime;

		public ActionMusic()
		{
			isDisplayed = true;
			category = ActionCategory.Sound;
			title = "Play music";
			description = "Plays or queues music clips.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			trackID = AssignInteger(parameters, trackIDParameterID, trackID);
			fadeTime = AssignFloat(parameters, fadeTimeParameterID, fadeTime);
			music = KickStarter.stateHandler.GetMusicEngine();
		}

		public override float Run()
		{
			if (music == null)
			{
				return 0f;
			}
			if (!isRunning)
			{
				isRunning = true;
				float num = Perform(fadeTime);
				if (CanWaitComplete() && willWaitComplete)
				{
					return base.defaultPauseTime;
				}
				if (willWait && num > 0f && !isQueued)
				{
					return num;
				}
			}
			else
			{
				if (CanWaitComplete() && willWaitComplete && music.GetCurrentTrackID() == trackID && music.IsPlaying())
				{
					return base.defaultPauseTime;
				}
				isRunning = false;
			}
			return 0f;
		}

		public override void Skip()
		{
			if (!(music == null))
			{
				Perform(0f);
			}
		}

		protected bool CanWaitComplete()
		{
			return !loop && !isQueued && (musicAction == MusicAction.Play || musicAction == MusicAction.Crossfade);
		}

		protected float Perform(float _time)
		{
			if (musicAction == MusicAction.Play)
			{
				return music.Play(trackID, loop, isQueued, _time, resumeIfPlayedBefore, 0, loopingOverlapTime);
			}
			if (musicAction == MusicAction.Crossfade)
			{
				return music.Crossfade(trackID, loop, isQueued, _time, resumeIfPlayedBefore, 0, loopingOverlapTime);
			}
			if (musicAction == MusicAction.Stop)
			{
				return music.StopAll(_time);
			}
			if (musicAction == MusicAction.ResumeLastStopped)
			{
				return music.ResumeLastQueue(_time, resumeFromStart);
			}
			return 0f;
		}

		public static ActionMusic CreateNew_Play(int trackID, bool loop = true, bool addToQueue = false, float transitionTime = 0f, bool doCrossfade = false, bool waitUntilFinish = false)
		{
			ActionMusic actionMusic = ScriptableObject.CreateInstance<ActionMusic>();
			actionMusic.musicAction = (doCrossfade ? MusicAction.Crossfade : MusicAction.Play);
			actionMusic.trackID = trackID;
			actionMusic.loop = loop;
			actionMusic.isQueued = addToQueue;
			actionMusic.fadeTime = transitionTime;
			actionMusic.willWait = waitUntilFinish;
			return actionMusic;
		}

		public static ActionMusic CreateNew_Stop(float transitionTime = 0f, bool waitUntilFinish = false)
		{
			ActionMusic actionMusic = ScriptableObject.CreateInstance<ActionMusic>();
			actionMusic.musicAction = MusicAction.Stop;
			actionMusic.fadeTime = transitionTime;
			actionMusic.willWait = waitUntilFinish;
			return actionMusic;
		}

		public static ActionMusic CreateNew_ResumeLastTrack(float transitionTime = 0f, bool doRestart = false, bool waitUntilFinish = false)
		{
			ActionMusic actionMusic = ScriptableObject.CreateInstance<ActionMusic>();
			actionMusic.musicAction = MusicAction.ResumeLastStopped;
			actionMusic.fadeTime = transitionTime;
			actionMusic.resumeFromStart = doRestart;
			actionMusic.willWaitComplete = waitUntilFinish;
			return actionMusic;
		}
	}
}
