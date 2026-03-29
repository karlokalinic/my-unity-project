using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionSceneSwitchPrevious : Action
	{
		public bool assignScreenOverlay;

		public bool onlyPreload;

		public bool relativePosition;

		public Marker relativeMarker;

		protected Marker runtimeRelativeMarker;

		public int relativeMarkerID;

		public int relativeMarkerParameterID = -1;

		public ActionSceneSwitchPrevious()
		{
			isDisplayed = true;
			category = ActionCategory.Scene;
			title = "Switch previous";
			description = "Moves the Player to the previously-loaded scene. The scene must be listed in Unity's Build Settings. By default, the screen will cut to black during the transition, but the last frame of the current scene can instead be overlayed. This allows for cinematic effects: if the next scene fades in, it will cause a crossfade effect; if the next scene doesn't fade, it will cause a straight cut.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			runtimeRelativeMarker = AssignFile(parameters, relativeMarkerParameterID, relativeMarkerID, relativeMarker);
		}

		public override float Run()
		{
			if (!assignScreenOverlay || (!relativePosition && onlyPreload))
			{
				ChangeScene();
				return 0f;
			}
			if (!isRunning)
			{
				if (KickStarter.sceneChanger.GetPreviousSceneInfo() == null || KickStarter.sceneChanger.GetPreviousSceneInfo().IsNull)
				{
					LogWarning("Cannot load previous scene as there is no data stored - is this the first scene in the game?");
					return 0f;
				}
				isRunning = true;
				KickStarter.mainCamera._ExitSceneWithOverlay();
				return base.defaultPauseTime;
			}
			ChangeScene();
			isRunning = false;
			return 0f;
		}

		public override void Skip()
		{
			ChangeScene();
		}

		protected void ChangeScene()
		{
			SceneInfo previousSceneInfo = KickStarter.sceneChanger.GetPreviousSceneInfo();
			if (previousSceneInfo == null || previousSceneInfo.IsNull)
			{
				LogWarning("Cannot load previous scene as there is no data stored - is this the first scene in the game?");
				return;
			}
			if (!onlyPreload && relativePosition && runtimeRelativeMarker != null)
			{
				KickStarter.sceneChanger.SetRelativePosition(runtimeRelativeMarker.transform);
			}
			if (onlyPreload && !relativePosition)
			{
				if (AdvGame.GetReferences().settingsManager.useAsyncLoading)
				{
					KickStarter.sceneChanger.PreloadScene(previousSceneInfo);
				}
				else
				{
					LogWarning("To pre-load scenes, 'Load scenes asynchronously?' must be enabled in the Settings Manager.");
				}
			}
			else
			{
				KickStarter.sceneChanger.ChangeScene(previousSceneInfo, true);
			}
		}

		public override ActionEnd End(List<Action> actions)
		{
			if (onlyPreload && !relativePosition)
			{
				return base.End(actions);
			}
			return GenerateStopActionEnd();
		}

		public static ActionSceneSwitchPrevious CreateNew(bool overlayCurrentScreen)
		{
			ActionSceneSwitchPrevious actionSceneSwitchPrevious = ScriptableObject.CreateInstance<ActionSceneSwitchPrevious>();
			actionSceneSwitchPrevious.assignScreenOverlay = overlayCurrentScreen;
			return actionSceneSwitchPrevious;
		}
	}
}
