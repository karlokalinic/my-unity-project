using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionSceneAdd : Action
	{
		public enum SceneAddRemove
		{
			Add = 0,
			Remove = 1
		}

		public SceneAddRemove sceneAddRemove;

		public bool runCutsceneOnStart;

		public bool runCutsceneIfAlreadyOpen;

		public ChooseSceneBy chooseSceneBy;

		public int sceneNumber;

		public int sceneNumberParameterID = -1;

		public string sceneName;

		public int sceneNameParameterID = -1;

		protected bool waitedOneMoreFrame;

		public ActionSceneAdd()
		{
			isDisplayed = true;
			category = ActionCategory.Scene;
			title = "Add or remove";
			description = "Adds or removes a scene without affecting any other open scenes.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			sceneNumber = AssignInteger(parameters, sceneNumberParameterID, sceneNumber);
			sceneName = AssignString(parameters, sceneNameParameterID, sceneName);
		}

		public override float Run()
		{
			SceneInfo sceneInfo = new SceneInfo(chooseSceneBy, AdvGame.ConvertTokens(sceneName), sceneNumber);
			if (!isRunning)
			{
				waitedOneMoreFrame = false;
				isRunning = true;
				if (KickStarter.sceneSettings.OverridesCameraPerspective())
				{
					ACDebug.LogError("The current scene overrides the default camera perspective - this feature should not be used in conjunction with multiple-open scenes.");
				}
				if (sceneAddRemove == SceneAddRemove.Add)
				{
					if (KickStarter.sceneChanger.AddSubScene(sceneInfo))
					{
						return base.defaultPauseTime;
					}
					if (runCutsceneIfAlreadyOpen && runCutsceneOnStart)
					{
						KickStarter.sceneSettings.cutsceneOnStart.Interact();
					}
				}
				else if (sceneAddRemove == SceneAddRemove.Remove)
				{
					KickStarter.sceneChanger.RemoveScene(sceneInfo);
				}
			}
			else
			{
				if (!waitedOneMoreFrame)
				{
					waitedOneMoreFrame = true;
					return base.defaultPauseTime;
				}
				if (sceneAddRemove == SceneAddRemove.Add)
				{
					bool flag = false;
					SubScene[] subScenes = KickStarter.sceneChanger.GetSubScenes();
					foreach (SubScene subScene in subScenes)
					{
						if (subScene.SceneInfo.Matches(sceneInfo))
						{
							flag = true;
							if (runCutsceneOnStart && subScene.SceneSettings != null && subScene.SceneSettings.cutsceneOnStart != null)
							{
								subScene.SceneSettings.cutsceneOnStart.Interact();
							}
						}
					}
					if (!flag)
					{
						LogWarning("Adding a non-AC scene additively!  A GameEngine prefab must be placed in scene '" + sceneInfo.GetLabel() + "'.");
					}
				}
				isRunning = false;
			}
			return 0f;
		}

		public static ActionSceneAdd CreateNew_Add(SceneInfo newSceneInfo, bool runCutsceneOnStart)
		{
			ActionSceneAdd actionSceneAdd = ScriptableObject.CreateInstance<ActionSceneAdd>();
			actionSceneAdd.sceneAddRemove = SceneAddRemove.Add;
			actionSceneAdd.sceneName = newSceneInfo.name;
			actionSceneAdd.sceneNumber = newSceneInfo.number;
			actionSceneAdd.chooseSceneBy = ((!string.IsNullOrEmpty(newSceneInfo.name)) ? ChooseSceneBy.Name : ChooseSceneBy.Number);
			actionSceneAdd.runCutsceneOnStart = runCutsceneOnStart;
			return actionSceneAdd;
		}

		public static ActionSceneAdd CreateNew_Remove(SceneInfo removeSceneInfo)
		{
			ActionSceneAdd actionSceneAdd = ScriptableObject.CreateInstance<ActionSceneAdd>();
			actionSceneAdd.sceneAddRemove = SceneAddRemove.Remove;
			actionSceneAdd.sceneName = removeSceneInfo.name;
			actionSceneAdd.sceneNumber = removeSceneInfo.number;
			actionSceneAdd.chooseSceneBy = ((!string.IsNullOrEmpty(removeSceneInfo.name)) ? ChooseSceneBy.Name : ChooseSceneBy.Number);
			return actionSceneAdd;
		}
	}
}
