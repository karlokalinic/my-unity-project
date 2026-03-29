using System;

namespace AC
{
	[Serializable]
	public class ActionObjectiveSet : Action
	{
		public int objectiveID;

		public int newStateID;

		public bool selectAfter;

		public int playerID;

		public bool setPlayer;

		public ActionObjectiveSet()
		{
			isDisplayed = true;
			category = ActionCategory.Objective;
			title = "Set state";
			description = "Updates an objective's current state.";
		}

		public override float Run()
		{
			if (KickStarter.inventoryManager.ObjectiveIsPerPlayer(objectiveID) && setPlayer)
			{
				KickStarter.runtimeObjectives.SetObjectiveState(objectiveID, newStateID, playerID);
			}
			else
			{
				KickStarter.runtimeObjectives.SetObjectiveState(objectiveID, newStateID, selectAfter);
			}
			Menu[] array = PlayerMenus.GetMenus(true).ToArray();
			Menu[] array2 = array;
			foreach (Menu menu in array2)
			{
				menu.Recalculate();
			}
			return 0f;
		}
	}
}
