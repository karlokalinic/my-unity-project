using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace AC
{
	[Serializable]
	public class ActionTimeline : Action
	{
		public enum ActionDirectorMethod
		{
			Play = 0,
			Stop = 1
		}

		[Serializable]
		protected class BindingData
		{
			public GameObject gameObject;

			public bool isPlayer;

			public int constantID;

			public int parameterID = -1;

			public BindingData()
			{
				gameObject = null;
				isPlayer = false;
				constantID = 0;
				parameterID = -1;
			}

			public BindingData(BindingData bindingData)
			{
				gameObject = bindingData.gameObject;
				isPlayer = bindingData.isPlayer;
				constantID = bindingData.constantID;
				parameterID = bindingData.parameterID;
			}
		}

		public bool disableCamera;

		public PlayableDirector director;

		protected PlayableDirector runtimeDirector;

		public TimelineAsset newTimeline;

		public int directorConstantID;

		public int directorParameterID = -1;

		public ActionDirectorMethod method;

		public bool restart = true;

		public bool pause;

		public bool updateBindings;

		[SerializeField]
		protected BindingData[] newBindings = new BindingData[0];

		public ActionTimeline()
		{
			isDisplayed = true;
			category = ActionCategory.Engine;
			title = "Control Timeline";
			description = "Controls a Timeline.  This is only compatible with Unity 2017 or newer.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			runtimeDirector = AssignFile(parameters, directorParameterID, directorConstantID, director);
			if (newBindings == null)
			{
				return;
			}
			for (int i = 0; i < newBindings.Length; i++)
			{
				if (newBindings[i].isPlayer)
				{
					if (KickStarter.player != null)
					{
						newBindings[i].gameObject = KickStarter.player.gameObject;
					}
					else
					{
						ACDebug.LogWarning("Cannot bind timeline track to Player, because no Player was found!", runtimeDirector);
					}
				}
				else
				{
					newBindings[i].gameObject = AssignFile(parameters, newBindings[i].parameterID, newBindings[i].constantID, newBindings[i].gameObject);
				}
			}
		}

		public override float Run()
		{
			if (!isRunning)
			{
				isRunning = true;
				if (runtimeDirector != null)
				{
					if (method == ActionDirectorMethod.Play)
					{
						isRunning = true;
						if (restart)
						{
							PrepareDirector();
							runtimeDirector.time = 0.0;
							runtimeDirector.Play();
						}
						else
						{
							runtimeDirector.Resume();
						}
						if (willWait)
						{
							if (disableCamera)
							{
								KickStarter.mainCamera.Disable();
							}
							return (float)runtimeDirector.duration - (float)runtimeDirector.time;
						}
					}
					else if (method == ActionDirectorMethod.Stop)
					{
						if (disableCamera)
						{
							KickStarter.mainCamera.Enable();
						}
						if (pause)
						{
							runtimeDirector.Pause();
						}
						else
						{
							PrepareDirectorEnd();
							runtimeDirector.time = runtimeDirector.duration;
							runtimeDirector.Stop();
						}
					}
				}
			}
			else
			{
				if (method == ActionDirectorMethod.Play && disableCamera)
				{
					KickStarter.mainCamera.Enable();
				}
				PrepareDirectorEnd();
				isRunning = false;
			}
			return 0f;
		}

		public override void Skip()
		{
			if (!(runtimeDirector != null))
			{
				return;
			}
			if (disableCamera)
			{
				KickStarter.mainCamera.Enable();
			}
			if (method == ActionDirectorMethod.Play)
			{
				if (runtimeDirector.extrapolationMode == DirectorWrapMode.Loop)
				{
					PrepareDirector();
					if (restart)
					{
						runtimeDirector.Play();
					}
					else
					{
						runtimeDirector.Resume();
					}
				}
				else
				{
					PrepareDirectorEnd();
					runtimeDirector.Stop();
					runtimeDirector.time = runtimeDirector.duration;
				}
			}
			else if (method == ActionDirectorMethod.Stop)
			{
				if (pause)
				{
					runtimeDirector.Pause();
				}
				else
				{
					runtimeDirector.Stop();
				}
			}
		}

		public override void Reset(ActionList actionList)
		{
			if (isRunning)
			{
				isRunning = false;
				Skip();
			}
		}

		protected void PrepareDirector()
		{
			if (newTimeline != null)
			{
				if (runtimeDirector.playableAsset != null && runtimeDirector.playableAsset is TimelineAsset)
				{
					TimelineAsset timelineAsset = (TimelineAsset)runtimeDirector.playableAsset;
					GameObject[] array = new GameObject[timelineAsset.outputTrackCount];
					for (int i = 0; i < array.Length; i++)
					{
						TrackAsset outputTrack = timelineAsset.GetOutputTrack(i);
						array[i] = runtimeDirector.GetGenericBinding(outputTrack) as GameObject;
					}
					runtimeDirector.playableAsset = newTimeline;
					for (int j = 0; j < array.Length; j++)
					{
						if (array[j] != null)
						{
							TrackAsset outputTrack2 = newTimeline.GetOutputTrack(j);
							if (outputTrack2 != null)
							{
								runtimeDirector.SetGenericBinding(outputTrack2, array[j].gameObject);
							}
						}
					}
				}
				else
				{
					runtimeDirector.playableAsset = newTimeline;
				}
			}
			TimelineAsset timelineAsset2 = runtimeDirector.playableAsset as TimelineAsset;
			if (!(timelineAsset2 != null))
			{
				return;
			}
			for (int k = 0; k < timelineAsset2.outputTrackCount; k++)
			{
				TrackAsset outputTrack3 = timelineAsset2.GetOutputTrack(k);
				if (updateBindings && newBindings != null && k < newBindings.Length && newBindings[k] != null && outputTrack3 != null && newBindings[k].gameObject != null)
				{
					SpeechTrack speechTrack = outputTrack3 as SpeechTrack;
					if (speechTrack != null)
					{
						speechTrack.isPlayerLine = newBindings[k].isPlayer;
						speechTrack.speakerObject = newBindings[k].gameObject;
					}
					else
					{
						runtimeDirector.SetGenericBinding(outputTrack3, newBindings[k].gameObject);
					}
				}
				GameObject gameObject = runtimeDirector.GetGenericBinding(outputTrack3) as GameObject;
				if (gameObject == null)
				{
					Animator animator = runtimeDirector.GetGenericBinding(outputTrack3) as Animator;
					if (animator != null)
					{
						gameObject = animator.gameObject;
					}
				}
				if (gameObject != null)
				{
					Char component = gameObject.GetComponent<Char>();
					if (component != null)
					{
						component.OnEnterTimeline(runtimeDirector, k);
					}
				}
			}
		}

		protected void PrepareDirectorEnd()
		{
			TimelineAsset timelineAsset = runtimeDirector.playableAsset as TimelineAsset;
			if (!(timelineAsset != null))
			{
				return;
			}
			for (int i = 0; i < timelineAsset.outputTrackCount; i++)
			{
				TrackAsset outputTrack = timelineAsset.GetOutputTrack(i);
				GameObject gameObject = runtimeDirector.GetGenericBinding(outputTrack) as GameObject;
				if (gameObject == null)
				{
					Animator animator = runtimeDirector.GetGenericBinding(outputTrack) as Animator;
					if (animator != null)
					{
						gameObject = animator.gameObject;
					}
				}
				if (gameObject != null)
				{
					Char component = gameObject.GetComponent<Char>();
					if (component != null)
					{
						component.OnExitTimeline(runtimeDirector, i);
					}
				}
			}
		}

		public static ActionTimeline CreateNew_Play(PlayableDirector director, TimelineAsset timelineAsset = null, bool playFromBeginning = true, bool disableACCamera = false, bool waitUntilFinish = false)
		{
			ActionTimeline actionTimeline = ScriptableObject.CreateInstance<ActionTimeline>();
			actionTimeline.method = ActionDirectorMethod.Play;
			actionTimeline.director = director;
			actionTimeline.newTimeline = timelineAsset;
			actionTimeline.restart = timelineAsset != null && playFromBeginning;
			actionTimeline.disableCamera = disableACCamera;
			actionTimeline.willWait = waitUntilFinish;
			return actionTimeline;
		}

		public static ActionTimeline CreateNew_Stop(PlayableDirector director, bool pauseTimeline = true, bool enableACCamera = true)
		{
			ActionTimeline actionTimeline = ScriptableObject.CreateInstance<ActionTimeline>();
			actionTimeline.method = ActionDirectorMethod.Stop;
			actionTimeline.director = director;
			actionTimeline.pause = pauseTimeline;
			actionTimeline.disableCamera = enableACCamera;
			return actionTimeline;
		}
	}
}
