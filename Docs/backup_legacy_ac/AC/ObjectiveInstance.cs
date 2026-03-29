namespace AC
{
	public class ObjectiveInstance
	{
		protected Objective linkedObjective;

		protected int currentStateID;

		public Objective Objective
		{
			get
			{
				return linkedObjective;
			}
		}

		public int CurrentStateID
		{
			get
			{
				return currentStateID;
			}
			set
			{
				if ((CurrentState.stateType == ObjectiveStateType.Complete && linkedObjective.lockStateWhenComplete) || (CurrentState.stateType == ObjectiveStateType.Fail && linkedObjective.lockStateWhenFail))
				{
					return;
				}
				ObjectiveState state = linkedObjective.GetState(value);
				if (state != null)
				{
					int num = currentStateID;
					currentStateID = value;
					if (num != currentStateID)
					{
						KickStarter.eventManager.Call_OnObjectiveUpdate(this);
					}
				}
				else
				{
					ACDebug.LogWarning("Cannot set the state of objective " + linkedObjective.ID + " to " + value + " because it does not exist!");
				}
			}
		}

		public ObjectiveState CurrentState
		{
			get
			{
				return linkedObjective.GetState(currentStateID);
			}
		}

		public string SaveData
		{
			get
			{
				return linkedObjective.ID + ":" + currentStateID;
			}
		}

		public ObjectiveInstance(int objectiveID)
		{
			if (KickStarter.inventoryManager != null)
			{
				linkedObjective = KickStarter.inventoryManager.GetObjective(objectiveID);
				currentStateID = 0;
			}
		}

		public ObjectiveInstance(int objectiveID, int startingStateID)
		{
			if (KickStarter.inventoryManager != null)
			{
				linkedObjective = KickStarter.inventoryManager.GetObjective(objectiveID);
				currentStateID = startingStateID;
			}
		}

		public ObjectiveInstance(string saveData)
		{
			if (!(KickStarter.inventoryManager != null))
			{
				return;
			}
			string[] array = saveData.Split(":"[0]);
			if (array.Length == 2)
			{
				int result = -1;
				if (int.TryParse(array[0], out result))
				{
					linkedObjective = KickStarter.inventoryManager.GetObjective(result);
				}
				int.TryParse(array[1], out currentStateID);
			}
		}
	}
}
