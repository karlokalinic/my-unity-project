using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionObjectCheck : ActionCheck
	{
		public GameObject gameObject;

		public int parameterID = -1;

		public int constantID;

		protected GameObject runtimeGameObject;

		public ActionObjectCheck()
		{
			isDisplayed = true;
			category = ActionCategory.Object;
			title = "Check presence";
			description = "Use to determine if a particular GameObject or prefab is present in the current scene.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			runtimeGameObject = AssignFile(parameters, parameterID, constantID, gameObject);
		}

		public override bool CheckCondition()
		{
			if (runtimeGameObject != null && runtimeGameObject.activeInHierarchy)
			{
				return true;
			}
			return false;
		}

		public static ActionObjectCheck CreateNew(GameObject objectToCheck)
		{
			ActionObjectCheck actionObjectCheck = ScriptableObject.CreateInstance<ActionObjectCheck>();
			actionObjectCheck.gameObject = objectToCheck;
			return actionObjectCheck;
		}
	}
}
