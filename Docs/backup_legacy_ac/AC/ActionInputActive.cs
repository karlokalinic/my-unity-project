using System;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionInputActive : Action
	{
		public int activeInputID;

		public bool newState;

		public ActionInputActive()
		{
			isDisplayed = true;
			category = ActionCategory.Input;
			title = "Toggle active";
			description = "Enables or disables an Active Input";
		}

		public override float Run()
		{
			if (KickStarter.settingsManager != null && KickStarter.settingsManager.activeInputs != null)
			{
				foreach (ActiveInput activeInput in KickStarter.settingsManager.activeInputs)
				{
					if (activeInput.ID == activeInputID)
					{
						activeInput.IsEnabled = newState;
						return 0f;
					}
				}
				LogWarning("Couldn't find the Active Input with ID=" + activeInputID);
				return 0f;
			}
			LogWarning("No Active Inputs found! Is the Settings Manager assigned properly?");
			return 0f;
		}

		public static ActionInputActive CreateNew(int activeInputID, ChangeType changeType)
		{
			ActionInputActive actionInputActive = ScriptableObject.CreateInstance<ActionInputActive>();
			actionInputActive.activeInputID = activeInputID;
			actionInputActive.newState = changeType == ChangeType.Enable;
			return actionInputActive;
		}
	}
}
