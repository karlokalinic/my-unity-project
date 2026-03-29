using System;
using System.Collections.Generic;

namespace AC
{
	[Serializable]
	public class ActionObjectiveCheck : ActionCheckMultiple
	{
		public int objectiveID;

		public int playerID;

		public bool setPlayer;

		public ActionObjectiveCheck()
		{
			isDisplayed = true;
			category = ActionCategory.Objective;
			title = "Check state";
			description = "Queries the current state of an objective.";
		}

		public override ActionEnd End(List<Action> actions)
		{
			if (numSockets <= 1)
			{
				return GenerateStopActionEnd();
			}
			Objective objective = KickStarter.inventoryManager.GetObjective(objectiveID);
			if (objective != null)
			{
				int num = ((!setPlayer || !KickStarter.inventoryManager.ObjectiveIsPerPlayer(objectiveID)) ? (-1) : playerID);
				ObjectiveState objectiveState = KickStarter.runtimeObjectives.GetObjectiveState(objectiveID, num);
				if (objectiveState != null)
				{
					int num2 = objective.states.IndexOf(objectiveState);
					return ProcessResult(num2 + 1, actions);
				}
				return ProcessResult(0, actions);
			}
			return GenerateStopActionEnd();
		}
	}
}
