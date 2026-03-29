using System;
using System.Collections.Generic;

namespace AC
{
	[Serializable]
	public class ActionObjectiveCheckType : ActionCheckMultiple
	{
		public int objectiveID;

		public int playerID;

		public bool setPlayer;

		public ActionObjectiveCheckType()
		{
			isDisplayed = true;
			category = ActionCategory.Objective;
			title = "Check state type";
			description = "Queries the current state type of an objective.";
		}

		public override ActionEnd End(List<Action> actions)
		{
			Objective objective = KickStarter.inventoryManager.GetObjective(objectiveID);
			if (objective != null)
			{
				int num = ((!setPlayer || !KickStarter.inventoryManager.ObjectiveIsPerPlayer(objectiveID)) ? (-1) : playerID);
				ObjectiveState objectiveState = KickStarter.runtimeObjectives.GetObjectiveState(objectiveID, num);
				if (objectiveState != null)
				{
					return ProcessResult((int)objectiveState.stateType, actions);
				}
			}
			return ProcessResult(0, actions);
		}
	}
}
