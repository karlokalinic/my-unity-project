using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionScene : Action
	{
		public ChooseSceneBy chooseSceneBy;

		public int sceneNumber;

		public int sceneNumberParameterID = -1;

		public string sceneName;

		public int sceneNameParameterID = -1;

		public bool assignScreenOverlay;

		public bool onlyPreload;

		public bool relativePosition;

		public Marker relativeMarker;

		protected Marker runtimeRelativeMarker;

		public int relativeMarkerID;

		public int relativeMarkerParameterID = -1;

		public bool forceReload;

		public ActionScene()
		{
			isDisplayed = true;
			category = ActionCategory.Scene;
			title = "Switch";
			description = "Moves the Player to a new scene. The scene must be listed in Unity's Build Settings. By default, the screen will cut to black during the transition, but the last frame of the current scene can instead be overlayed. This allows for cinematic effects: if the next scene fades in, it will cause a crossfade effect; if the next scene doesn't fade, it will cause a straight cut.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			sceneNumber = AssignInteger(parameters, sceneNumberParameterID, sceneNumber);
			sceneName = AssignString(parameters, sceneNameParameterID, sceneName);
			runtimeRelativeMarker = AssignFile(parameters, relativeMarkerParameterID, relativeMarkerID, relativeMarker);
		}

		public override float Run()
		{
			if (!assignScreenOverlay || onlyPreload)
			{
				ChangeScene();
				return 0f;
			}
			if (!isRunning)
			{
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
			if (sceneNumber <= -1 && chooseSceneBy != ChooseSceneBy.Name)
			{
				return;
			}
			SceneInfo sceneInfo = new SceneInfo(chooseSceneBy, AdvGame.ConvertTokens(sceneName), sceneNumber);
			if (sceneInfo.IsNull)
			{
				return;
			}
			if (onlyPreload)
			{
				if (AdvGame.GetReferences().settingsManager.useAsyncLoading)
				{
					KickStarter.sceneChanger.PreloadScene(sceneInfo);
				}
				else if (AdvGame.GetReferences().settingsManager.useLoadingScreen)
				{
					LogWarning("Scenes cannot be preloaded when loading scenes are used in the Settings Manager.");
				}
				else
				{
					LogWarning("To pre-load scenes, 'Load scenes asynchronously?' must be enabled in the Settings Manager.");
				}
				return;
			}
			if (relativePosition && runtimeRelativeMarker != null)
			{
				KickStarter.sceneChanger.SetRelativePosition(runtimeRelativeMarker.transform);
			}
			if (!KickStarter.sceneChanger.ChangeScene(sceneInfo, true, forceReload) && assignScreenOverlay)
			{
				KickStarter.mainCamera.SetFadeTexture(null);
				KickStarter.mainCamera.FadeIn(0f);
			}
		}

		public override ActionEnd End(List<Action> actions)
		{
			if (onlyPreload)
			{
				return base.End(actions);
			}
			if (isAssetFile)
			{
				return base.End(actions);
			}
			return GenerateStopActionEnd();
		}

		public static ActionScene CreateNew_PreloadOnly(SceneInfo newSceneInfo)
		{
			ActionScene actionScene = ScriptableObject.CreateInstance<ActionScene>();
			actionScene.sceneName = newSceneInfo.name;
			actionScene.sceneNumber = newSceneInfo.number;
			actionScene.chooseSceneBy = ((!string.IsNullOrEmpty(newSceneInfo.name)) ? ChooseSceneBy.Name : ChooseSceneBy.Number);
			actionScene.onlyPreload = true;
			return actionScene;
		}

		public static ActionScene CreateNew_Switch(SceneInfo newSceneInfo, bool forceReload, bool overlayCurrentScreen)
		{
			ActionScene actionScene = ScriptableObject.CreateInstance<ActionScene>();
			actionScene.sceneName = newSceneInfo.name;
			actionScene.sceneNumber = newSceneInfo.number;
			actionScene.chooseSceneBy = ((!string.IsNullOrEmpty(newSceneInfo.name)) ? ChooseSceneBy.Name : ChooseSceneBy.Number);
			actionScene.onlyPreload = false;
			actionScene.forceReload = forceReload;
			actionScene.assignScreenOverlay = overlayCurrentScreen;
			return actionScene;
		}
	}
}
