using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionVisibleCheck : ActionCheck
	{
		public int parameterID = -1;

		public int constantID;

		public GameObject obToAffect;

		protected GameObject runtimeObToAffect;

		public CheckVisState checkVisState;

		public ActionVisibleCheck()
		{
			isDisplayed = true;
			category = ActionCategory.Object;
			title = "Check visibility";
			description = "Checks the visibility of a GameObject.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			runtimeObToAffect = AssignFile(parameters, parameterID, constantID, obToAffect);
		}

		public override bool CheckCondition()
		{
			if ((bool)runtimeObToAffect)
			{
				SpriteFader component = runtimeObToAffect.GetComponent<SpriteFader>();
				if (component != null && component.GetAlpha() <= 0f)
				{
					return false;
				}
				Renderer component2 = runtimeObToAffect.GetComponent<Renderer>();
				if (component2 != null)
				{
					switch (checkVisState)
					{
					case CheckVisState.InCamera:
						return component2.isVisible;
					case CheckVisState.InScene:
						return component2.enabled;
					}
				}
				ACDebug.LogWarning("Cannot check visibility of " + runtimeObToAffect.name + " as it has no renderer component", runtimeObToAffect);
			}
			return false;
		}

		public static ActionVisibleCheck CreateNew(GameObject objectToCheck, CheckVisState visibilityToCheck)
		{
			ActionVisibleCheck actionVisibleCheck = ScriptableObject.CreateInstance<ActionVisibleCheck>();
			actionVisibleCheck.obToAffect = objectToCheck;
			actionVisibleCheck.checkVisState = visibilityToCheck;
			return actionVisibleCheck;
		}
	}
}
