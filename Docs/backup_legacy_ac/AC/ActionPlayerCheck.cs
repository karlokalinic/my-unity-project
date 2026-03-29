using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionPlayerCheck : ActionCheck
	{
		public int playerID;

		public int playerIDParameterID;

		public ActionPlayerCheck()
		{
			isDisplayed = true;
			category = ActionCategory.Player;
			title = "Check";
			description = "Queries which Player prefab is currently being controlled. This only applies to games for which 'Player switching' has been allowed in the Settings Manager.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			playerID = AssignInteger(parameters, playerIDParameterID, playerID);
		}

		public override bool CheckCondition()
		{
			if ((bool)KickStarter.player && KickStarter.player.ID == playerID)
			{
				return true;
			}
			return false;
		}

		public static ActionPlayerCheck CreateNew(int playerIDToCheck)
		{
			ActionPlayerCheck actionPlayerCheck = ScriptableObject.CreateInstance<ActionPlayerCheck>();
			actionPlayerCheck.playerID = playerIDToCheck;
			return actionPlayerCheck;
		}
	}
}
