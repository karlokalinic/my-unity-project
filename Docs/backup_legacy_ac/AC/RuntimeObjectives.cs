using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace AC
{
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_runtime_objectives.html")]
	public class RuntimeObjectives : MonoBehaviour
	{
		protected List<ObjectiveInstance> playerObjectiveInstances = new List<ObjectiveInstance>();

		protected List<ObjectiveInstance> globalObjectiveInstances = new List<ObjectiveInstance>();

		protected ObjectiveInstance selectedObjectiveInstance;

		public ObjectiveInstance SelectedObjective
		{
			get
			{
				return selectedObjectiveInstance;
			}
			set
			{
				selectedObjectiveInstance = value;
				if (selectedObjectiveInstance != null)
				{
					KickStarter.eventManager.Call_OnObjectiveSelect(selectedObjectiveInstance);
				}
			}
		}

		public void SetObjectiveState(int objectiveID, int newStateID, bool selectAfter = false)
		{
			foreach (ObjectiveInstance playerObjectiveInstance in playerObjectiveInstances)
			{
				if (playerObjectiveInstance.Objective.ID == objectiveID)
				{
					playerObjectiveInstance.CurrentStateID = newStateID;
					if (selectAfter)
					{
						SelectedObjective = playerObjectiveInstance;
					}
					return;
				}
			}
			foreach (ObjectiveInstance globalObjectiveInstance in globalObjectiveInstances)
			{
				if (globalObjectiveInstance.Objective.ID == objectiveID)
				{
					globalObjectiveInstance.CurrentStateID = newStateID;
					if (selectAfter)
					{
						SelectedObjective = globalObjectiveInstance;
					}
					return;
				}
			}
			ObjectiveInstance objectiveInstance = new ObjectiveInstance(objectiveID, newStateID);
			if (objectiveInstance.Objective != null)
			{
				if (objectiveInstance.Objective.perPlayer && KickStarter.settingsManager.playerSwitching == PlayerSwitching.Allow)
				{
					playerObjectiveInstances.Add(objectiveInstance);
				}
				else
				{
					globalObjectiveInstances.Add(objectiveInstance);
				}
				if (selectAfter)
				{
					SelectedObjective = objectiveInstance;
				}
				KickStarter.eventManager.Call_OnObjectiveUpdate(objectiveInstance);
			}
			else
			{
				ACDebug.LogWarning("Cannot set the state of objective " + objectiveID + " because that ID does not exist!");
			}
		}

		public void SetObjectiveState(int objectiveID, int newStateID, int playerID)
		{
			if (!KickStarter.inventoryManager.ObjectiveIsPerPlayer(objectiveID))
			{
				SetObjectiveState(objectiveID, newStateID);
				return;
			}
			if (playerID < 0 || ((bool)KickStarter.player && KickStarter.player.ID == playerID))
			{
				SetObjectiveState(objectiveID, newStateID);
				return;
			}
			PlayerData playerData = KickStarter.saveSystem.GetPlayerData(playerID);
			if (playerData != null)
			{
				ObjectiveInstance[] array = ExtractPlayerObjectiveData(playerData).ToArray();
				ObjectiveInstance[] array2 = array;
				foreach (ObjectiveInstance objectiveInstance in array2)
				{
					if (objectiveInstance.Objective.ID == objectiveID)
					{
						objectiveInstance.CurrentStateID = newStateID;
						string dataString = RecordPlayerObjectiveData(array);
						KickStarter.saveSystem.AssignObjectivesToPlayer(dataString, playerID);
						return;
					}
				}
			}
			List<ObjectiveInstance> list = new List<ObjectiveInstance>();
			ObjectiveInstance objectiveInstance2 = new ObjectiveInstance(objectiveID, newStateID);
			if (objectiveInstance2.Objective != null)
			{
				list.Add(objectiveInstance2);
				string dataString2 = RecordPlayerObjectiveData(list.ToArray());
				KickStarter.saveSystem.AssignObjectivesToPlayer(dataString2, playerID);
			}
		}

		public ObjectiveState GetObjectiveState(int objectiveID)
		{
			foreach (ObjectiveInstance playerObjectiveInstance in playerObjectiveInstances)
			{
				if (playerObjectiveInstance.Objective.ID == objectiveID)
				{
					return playerObjectiveInstance.CurrentState;
				}
			}
			foreach (ObjectiveInstance globalObjectiveInstance in globalObjectiveInstances)
			{
				if (globalObjectiveInstance.Objective.ID == objectiveID)
				{
					return globalObjectiveInstance.CurrentState;
				}
			}
			return null;
		}

		public ObjectiveState GetObjectiveState(int objectiveID, int playerID)
		{
			if (!KickStarter.inventoryManager.ObjectiveIsPerPlayer(objectiveID))
			{
				return GetObjectiveState(objectiveID);
			}
			if (playerID < 0 || ((bool)KickStarter.player && KickStarter.player.ID == playerID))
			{
				return GetObjectiveState(objectiveID);
			}
			PlayerData playerData = KickStarter.saveSystem.GetPlayerData(playerID);
			if (playerData != null)
			{
				ObjectiveInstance[] array = ExtractPlayerObjectiveData(playerData).ToArray();
				ObjectiveInstance[] array2 = array;
				foreach (ObjectiveInstance objectiveInstance in array2)
				{
					if (objectiveInstance.Objective.ID == objectiveID)
					{
						return objectiveInstance.CurrentState;
					}
				}
				return null;
			}
			return null;
		}

		public void CancelObjective(int objectiveID)
		{
			foreach (ObjectiveInstance playerObjectiveInstance in playerObjectiveInstances)
			{
				if (playerObjectiveInstance.Objective.ID == objectiveID)
				{
					playerObjectiveInstances.Remove(playerObjectiveInstance);
					return;
				}
			}
			foreach (ObjectiveInstance globalObjectiveInstance in globalObjectiveInstances)
			{
				if (globalObjectiveInstance.Objective.ID == objectiveID)
				{
					globalObjectiveInstances.Remove(globalObjectiveInstance);
					break;
				}
			}
		}

		public ObjectiveInstance GetObjective(int objectiveID)
		{
			foreach (ObjectiveInstance playerObjectiveInstance in playerObjectiveInstances)
			{
				if (playerObjectiveInstance.Objective.ID == objectiveID)
				{
					return playerObjectiveInstance;
				}
			}
			foreach (ObjectiveInstance globalObjectiveInstance in globalObjectiveInstances)
			{
				if (globalObjectiveInstance.Objective.ID == objectiveID)
				{
					return globalObjectiveInstance;
				}
			}
			return null;
		}

		public ObjectiveInstance[] GetObjectives()
		{
			List<ObjectiveInstance> list = new List<ObjectiveInstance>();
			foreach (ObjectiveInstance playerObjectiveInstance in playerObjectiveInstances)
			{
				list.Add(playerObjectiveInstance);
			}
			foreach (ObjectiveInstance globalObjectiveInstance in globalObjectiveInstances)
			{
				list.Add(globalObjectiveInstance);
			}
			return list.ToArray();
		}

		public ObjectiveInstance[] GetObjectives(ObjectiveStateType objectiveStateType)
		{
			List<ObjectiveInstance> list = new List<ObjectiveInstance>();
			foreach (ObjectiveInstance playerObjectiveInstance in playerObjectiveInstances)
			{
				if (playerObjectiveInstance.CurrentState.stateType == objectiveStateType)
				{
					list.Add(playerObjectiveInstance);
				}
			}
			foreach (ObjectiveInstance globalObjectiveInstance in globalObjectiveInstances)
			{
				if (globalObjectiveInstance.CurrentState.stateType == objectiveStateType)
				{
					list.Add(globalObjectiveInstance);
				}
			}
			return list.ToArray();
		}

		public ObjectiveInstance[] GetObjectives(ObjectiveDisplayType objectiveDisplayType)
		{
			List<ObjectiveInstance> list = new List<ObjectiveInstance>();
			foreach (ObjectiveInstance playerObjectiveInstance in playerObjectiveInstances)
			{
				if (playerObjectiveInstance.CurrentState.DisplayTypeMatches(objectiveDisplayType))
				{
					list.Add(playerObjectiveInstance);
				}
			}
			foreach (ObjectiveInstance globalObjectiveInstance in globalObjectiveInstances)
			{
				if (globalObjectiveInstance.CurrentState.DisplayTypeMatches(objectiveDisplayType))
				{
					list.Add(globalObjectiveInstance);
				}
			}
			return list.ToArray();
		}

		public void SelectObjective(int objectiveID)
		{
			SelectedObjective = GetObjective(objectiveID);
		}

		public void DeselectObjective()
		{
			SelectedObjective = null;
		}

		public void ClearAll()
		{
			ClearUniqueToPlayer();
			globalObjectiveInstances.Clear();
		}

		public void ClearUniqueToPlayer()
		{
			playerObjectiveInstances.Clear();
		}

		public PlayerData SavePlayerObjectives(PlayerData playerData)
		{
			playerData.playerObjectivesData = RecordPlayerObjectiveData(playerObjectiveInstances.ToArray());
			return playerData;
		}

		public void AssignPlayerObjectives(PlayerData playerData)
		{
			playerObjectiveInstances.Clear();
			SelectedObjective = null;
			playerObjectiveInstances = ExtractPlayerObjectiveData(playerData);
		}

		public MainData SaveGlobalObjectives(MainData mainData)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (ObjectiveInstance globalObjectiveInstance in globalObjectiveInstances)
			{
				stringBuilder.Append(globalObjectiveInstance.SaveData);
				stringBuilder.Append("|");
			}
			if (globalObjectiveInstances.Count > 0)
			{
				stringBuilder.Remove(stringBuilder.Length - 1, 1);
			}
			mainData.globalObjectivesData = stringBuilder.ToString();
			return mainData;
		}

		public void AssignGlobalObjectives(MainData mainData)
		{
			globalObjectiveInstances.Clear();
			SelectedObjective = null;
			if (string.IsNullOrEmpty(mainData.globalObjectivesData))
			{
				return;
			}
			string[] array = mainData.globalObjectivesData.Split("|"[0]);
			string[] array2 = array;
			foreach (string saveData in array2)
			{
				ObjectiveInstance objectiveInstance = new ObjectiveInstance(saveData);
				if (objectiveInstance.Objective != null)
				{
					globalObjectiveInstances.Add(objectiveInstance);
				}
			}
		}

		private List<ObjectiveInstance> ExtractPlayerObjectiveData(PlayerData playerData)
		{
			List<ObjectiveInstance> list = new List<ObjectiveInstance>();
			if (!string.IsNullOrEmpty(playerData.playerObjectivesData))
			{
				string[] array = playerData.playerObjectivesData.Split("|"[0]);
				string[] array2 = array;
				foreach (string saveData in array2)
				{
					ObjectiveInstance objectiveInstance = new ObjectiveInstance(saveData);
					if (objectiveInstance.Objective != null)
					{
						list.Add(objectiveInstance);
					}
				}
			}
			return list;
		}

		private string RecordPlayerObjectiveData(ObjectiveInstance[] objectivesInstances)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (ObjectiveInstance objectiveInstance in objectivesInstances)
			{
				stringBuilder.Append(objectiveInstance.SaveData);
				stringBuilder.Append("|");
			}
			if (objectivesInstances.Length > 0)
			{
				stringBuilder.Remove(stringBuilder.Length - 1, 1);
			}
			return stringBuilder.ToString();
		}
	}
}
