using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionAmbience : Action
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

		public float loopingOverlapTime;

		protected Ambience ambience;

		public ActionAmbience()
		{
			isDisplayed = true;
			category = ActionCategory.Sound;
			title = "Play ambience";
			description = "Plays or queues ambience clips.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			trackID = AssignInteger(parameters, trackIDParameterID, trackID);
			fadeTime = AssignFloat(parameters, fadeTimeParameterID, fadeTime);
			ambience = KickStarter.stateHandler.GetAmbienceEngine();
		}

		public override float Run()
		{
			if (ambience == null)
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
				if (CanWaitComplete() && willWaitComplete && ambience.GetCurrentTrackID() == trackID && ambience.IsPlaying())
				{
					return base.defaultPauseTime;
				}
				isRunning = false;
			}
			return 0f;
		}

		public override void Skip()
		{
			if (!(ambience == null))
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
			if (ambience != null)
			{
				if (musicAction == MusicAction.Play)
				{
					return ambience.Play(trackID, loop, isQueued, _time, resumeIfPlayedBefore, 0, loopingOverlapTime);
				}
				if (musicAction == MusicAction.Crossfade)
				{
					return ambience.Crossfade(trackID, loop, isQueued, _time, resumeIfPlayedBefore, 0, loopingOverlapTime);
				}
				if (musicAction == MusicAction.Stop)
				{
					return ambience.StopAll(_time);
				}
				if (musicAction == MusicAction.ResumeLastStopped)
				{
					return ambience.ResumeLastQueue(_time, resumeFromStart);
				}
			}
			return 0f;
		}

		public static ActionAmbience CreateNew_Play(int trackID, bool loop = true, bool addToQueue = false, float transitionTime = 0f, bool doCrossfade = false, bool waitUntilFinish = false)
		{
			ActionAmbience actionAmbience = ScriptableObject.CreateInstance<ActionAmbience>();
			actionAmbience.musicAction = (doCrossfade ? MusicAction.Crossfade : MusicAction.Play);
			actionAmbience.trackID = trackID;
			actionAmbience.loop = loop;
			actionAmbience.isQueued = addToQueue;
			actionAmbience.fadeTime = transitionTime;
			actionAmbience.willWait = waitUntilFinish;
			return actionAmbience;
		}

		public static ActionAmbience CreateNew_Stop(float transitionTime = 0f, bool waitUntilFinish = false)
		{
			ActionAmbience actionAmbience = ScriptableObject.CreateInstance<ActionAmbience>();
			actionAmbience.musicAction = MusicAction.Stop;
			actionAmbience.fadeTime = transitionTime;
			actionAmbience.willWait = waitUntilFinish;
			return actionAmbience;
		}

		public static ActionAmbience CreateNew_ResumeLastTrack(float transitionTime = 0f, bool doRestart = false, bool waitUntilFinish = false)
		{
			ActionAmbience actionAmbience = ScriptableObject.CreateInstance<ActionAmbience>();
			actionAmbience.musicAction = MusicAction.ResumeLastStopped;
			actionAmbience.fadeTime = transitionTime;
			actionAmbience.resumeFromStart = doRestart;
			actionAmbience.willWaitComplete = waitUntilFinish;
			return actionAmbience;
		}
	}
}
