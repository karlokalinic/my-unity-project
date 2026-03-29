using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionTagCheck : ActionCheck
	{
		public GameObject objectToCheck;

		public int objectToCheckConstantID;

		public int objectToCheckParameterID = -1;

		protected GameObject runtimeObjectToCheck;

		public string tagsToCheck;

		public int tagsToCheckParameterID = -1;

		public ActionTagCheck()
		{
			isDisplayed = true;
			category = ActionCategory.Object;
			title = "Check tag";
			description = "This action checks which tag has been assigned to a given GameObject.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			runtimeObjectToCheck = AssignFile(parameters, objectToCheckParameterID, objectToCheckConstantID, objectToCheck);
			tagsToCheck = AssignString(parameters, tagsToCheckParameterID, tagsToCheck);
		}

		public override bool CheckCondition()
		{
			if (runtimeObjectToCheck != null && !string.IsNullOrEmpty(tagsToCheck))
			{
				if (!tagsToCheck.StartsWith(";"))
				{
					tagsToCheck = ";" + tagsToCheck;
				}
				if (!tagsToCheck.EndsWith(";"))
				{
					tagsToCheck += ";";
				}
				string tag = runtimeObjectToCheck.tag;
				return tagsToCheck.Contains(";" + tag + ";");
			}
			return false;
		}

		public static ActionTagCheck CreateNew(GameObject gameObject, string tag)
		{
			ActionTagCheck actionTagCheck = ScriptableObject.CreateInstance<ActionTagCheck>();
			actionTagCheck.objectToCheck = gameObject;
			actionTagCheck.tagsToCheck = tag;
			return actionTagCheck;
		}

		public static ActionTagCheck CreateNew(GameObject gameObject, string[] tags)
		{
			ActionTagCheck actionTagCheck = ScriptableObject.CreateInstance<ActionTagCheck>();
			actionTagCheck.objectToCheck = gameObject;
			string text = string.Empty;
			for (int i = 0; i < tags.Length; i++)
			{
				text += tags[i];
				if (i < tags.Length - 1)
				{
					text += ";";
				}
			}
			actionTagCheck.tagsToCheck = text;
			return actionTagCheck;
		}
	}
}
