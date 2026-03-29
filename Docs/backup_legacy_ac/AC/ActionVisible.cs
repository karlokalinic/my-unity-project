using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionVisible : Action
	{
		public int parameterID = -1;

		public int constantID;

		public GameObject obToAffect;

		protected GameObject runtimeObToAffect;

		public bool affectChildren;

		public VisState visState;

		public ActionVisible()
		{
			isDisplayed = true;
			category = ActionCategory.Object;
			title = "Visibility";
			description = "Hides or shows a GameObject. Can optionally affect the GameObject's children.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			runtimeObToAffect = AssignFile(parameters, parameterID, constantID, obToAffect);
		}

		public override float Run()
		{
			bool flag = false;
			if (visState == VisState.Visible)
			{
				flag = true;
			}
			if (runtimeObToAffect != null)
			{
				if ((bool)runtimeObToAffect.GetComponent<LimitVisibility>())
				{
					runtimeObToAffect.GetComponent<LimitVisibility>().isLockedOff = !flag;
				}
				else if ((bool)runtimeObToAffect.GetComponent<Renderer>())
				{
					runtimeObToAffect.GetComponent<Renderer>().enabled = flag;
				}
				if (affectChildren)
				{
					Renderer[] componentsInChildren = runtimeObToAffect.GetComponentsInChildren<Renderer>();
					foreach (Renderer renderer in componentsInChildren)
					{
						renderer.enabled = flag;
					}
				}
			}
			return 0f;
		}

		public static ActionVisible CreateNew(GameObject objectToAffect, VisState newVisiblityState, bool affectChildren = false)
		{
			ActionVisible actionVisible = ScriptableObject.CreateInstance<ActionVisible>();
			actionVisible.obToAffect = objectToAffect;
			actionVisible.visState = newVisiblityState;
			actionVisible.affectChildren = affectChildren;
			return actionVisible;
		}
	}
}
