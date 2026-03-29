using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionSceneCheck : ActionCheck
	{
		public enum IntCondition
		{
			EqualTo = 0,
			NotEqualTo = 1
		}

		public enum SceneToCheck
		{
			Current = 0,
			Previous = 1
		}

		public ChooseSceneBy chooseSceneBy;

		public SceneToCheck sceneToCheck;

		public int sceneNumberParameterID = -1;

		public int sceneNumber;

		public int sceneNameParameterID = -1;

		public string sceneName;

		public IntCondition intCondition;

		public ActionSceneCheck()
		{
			isDisplayed = true;
			category = ActionCategory.Scene;
			title = "Check";
			description = "Queries either the current scene, or the last one visited.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			sceneNumber = AssignInteger(parameters, sceneNumberParameterID, sceneNumber);
			sceneName = AssignString(parameters, sceneNameParameterID, sceneName);
		}

		public override bool CheckCondition()
		{
			int num = 0;
			string empty = string.Empty;
			if (sceneToCheck == SceneToCheck.Previous)
			{
				num = KickStarter.sceneChanger.GetPreviousSceneInfo().number;
				empty = KickStarter.sceneChanger.GetPreviousSceneInfo().name;
			}
			else
			{
				num = UnityVersionHandler.GetCurrentSceneNumber();
				empty = UnityVersionHandler.GetCurrentSceneName();
			}
			if (intCondition == IntCondition.EqualTo)
			{
				if (chooseSceneBy == ChooseSceneBy.Name && empty == AdvGame.ConvertTokens(sceneName))
				{
					return true;
				}
				if (chooseSceneBy == ChooseSceneBy.Number && num == sceneNumber)
				{
					return true;
				}
			}
			else if (intCondition == IntCondition.NotEqualTo)
			{
				if (chooseSceneBy == ChooseSceneBy.Name && empty != AdvGame.ConvertTokens(sceneName))
				{
					return true;
				}
				if (chooseSceneBy == ChooseSceneBy.Number && num != sceneNumber)
				{
					return true;
				}
			}
			return false;
		}

		public static ActionSceneCheck CreateNew(string sceneName, SceneToCheck sceneToCheck = SceneToCheck.Current)
		{
			ActionSceneCheck actionSceneCheck = ScriptableObject.CreateInstance<ActionSceneCheck>();
			actionSceneCheck.sceneToCheck = sceneToCheck;
			actionSceneCheck.chooseSceneBy = ChooseSceneBy.Name;
			actionSceneCheck.sceneName = sceneName;
			return actionSceneCheck;
		}

		public static ActionSceneCheck CreateNew(int sceneNumber, SceneToCheck sceneToCheck = SceneToCheck.Current)
		{
			ActionSceneCheck actionSceneCheck = ScriptableObject.CreateInstance<ActionSceneCheck>();
			actionSceneCheck.sceneToCheck = sceneToCheck;
			actionSceneCheck.chooseSceneBy = ChooseSceneBy.Number;
			actionSceneCheck.sceneNumber = sceneNumber;
			return actionSceneCheck;
		}
	}
}
