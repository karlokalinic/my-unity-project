using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_action_list_asset_manager.html")]
	public class ActionListAssetManager : MonoBehaviour
	{
		public List<ActiveList> activeLists = new List<ActiveList>();

		public void OnAwake()
		{
			activeLists.Clear();
		}

		protected void OnDestroy()
		{
			activeLists.Clear();
		}

		public bool IsListRunning(ActionListAsset actionListAsset)
		{
			if (actionListAsset == null)
			{
				return false;
			}
			foreach (ActiveList activeList in activeLists)
			{
				if (activeList.IsFor(actionListAsset) && activeList.IsRunning())
				{
					return true;
				}
			}
			return false;
		}

		public void AddToList(RuntimeActionList runtimeActionList, ActionListAsset actionListAsset, bool addToSkipQueue, int _startIndex, bool removeMultipleInstances = false)
		{
			if (!actionListAsset.canRunMultipleInstances || removeMultipleInstances)
			{
				for (int i = 0; i < activeLists.Count; i++)
				{
					if (activeLists[i].IsFor(actionListAsset))
					{
						if (actionListAsset.canRunMultipleInstances && removeMultipleInstances)
						{
							activeLists[i].Reset(false);
						}
						activeLists.RemoveAt(i);
					}
				}
			}
			addToSkipQueue = KickStarter.actionListManager.CanAddToSkipQueue(runtimeActionList, addToSkipQueue);
			activeLists.Add(new ActiveList(runtimeActionList, addToSkipQueue, _startIndex));
			if (!KickStarter.playerMenus.ArePauseMenusOn() || (runtimeActionList.actionListType != ActionListType.RunInBackground && (runtimeActionList.actionListType != ActionListType.PauseGameplay || runtimeActionList.unfreezePauseMenus)))
			{
				KickStarter.actionListManager.SetCorrectGameState();
			}
		}

		public void DestroyAssetList(ActionListAsset asset)
		{
			foreach (ActiveList activeList in activeLists)
			{
				if (activeList.IsFor(asset))
				{
					activeList.Reset(true);
				}
			}
		}

		public int EndAssetList(ActionListAsset asset, Action _action = null, bool forceEndAll = false)
		{
			int num = 0;
			for (int i = 0; i < activeLists.Count; i++)
			{
				if (!activeLists[i].IsFor(asset) || (!(_action == null) && activeLists[i].actionList.actions.Contains(_action)))
				{
					continue;
				}
				activeLists[i].ClearNecessity();
				int count = activeLists.Count;
				KickStarter.actionListManager.EndList(activeLists[i]);
				if (count == activeLists.Count)
				{
					ACDebug.LogWarning("Ended asset " + asset.name + ", but ActiveList data is retained.", asset);
					continue;
				}
				num++;
				i = -1;
				if (asset.canRunMultipleInstances && !forceEndAll)
				{
					return num;
				}
			}
			return num;
		}

		public void EndAssetList(RuntimeActionList runtimeActionList)
		{
			if (!(runtimeActionList != null))
			{
				return;
			}
			for (int i = 0; i < activeLists.Count; i++)
			{
				if (activeLists[i].IsFor(runtimeActionList))
				{
					KickStarter.actionListManager.EndList(activeLists[i]);
					break;
				}
			}
		}

		public void AssignResumeIndices(ActionListAsset actionListAsset, int[] resumeIndices)
		{
			for (int i = 0; i < activeLists.Count; i++)
			{
				if (activeLists[i].IsFor(actionListAsset))
				{
					activeLists[i].SetResumeIndices(resumeIndices);
				}
			}
		}

		public RuntimeActionList[] Pause(ActionListAsset actionListAsset)
		{
			List<RuntimeActionList> list = new List<RuntimeActionList>();
			for (int i = 0; i < activeLists.Count; i++)
			{
				if (activeLists[i].IsFor(actionListAsset))
				{
					RuntimeActionList runtimeActionList = (RuntimeActionList)activeLists[i].actionList;
					runtimeActionList.Pause();
					list.Add(runtimeActionList);
					activeLists[i].UpdateParameterData();
				}
			}
			return list.ToArray();
		}

		public void Resume(ActionListAsset actionListAsset, bool rerunPausedActions)
		{
			if (IsListRunning(actionListAsset) && !actionListAsset.canRunMultipleInstances)
			{
				return;
			}
			bool flag = false;
			for (int i = 0; i < activeLists.Count; i++)
			{
				if (!activeLists[i].IsFor(actionListAsset))
				{
					continue;
				}
				int num = 0;
				foreach (ActiveList activeList in activeLists)
				{
					if (activeList.IsFor(actionListAsset) && activeList.IsRunning())
					{
						num++;
					}
				}
				GameObject gameObject = (GameObject)Object.Instantiate(Resources.Load("RuntimeActionList"));
				gameObject.name = actionListAsset.name;
				if (num > 0)
				{
					gameObject.name = gameObject.name + " " + num;
				}
				RuntimeActionList component = gameObject.GetComponent<RuntimeActionList>();
				component.DownloadActions(actionListAsset, activeLists[i].GetConversationOnEnd(), activeLists[i].startIndex, false, activeLists[i].inSkipQueue, true);
				activeLists[i].Resume(component, rerunPausedActions);
				flag = true;
				if (!actionListAsset.canRunMultipleInstances)
				{
					return;
				}
			}
			if (!flag)
			{
				ACDebug.LogWarning(string.Concat("No resume data found for '", actionListAsset, "' - running from start."), actionListAsset);
				AdvGame.RunActionListAsset(actionListAsset);
			}
		}

		public string GetSaveData()
		{
			PurgeLists();
			string text = string.Empty;
			for (int i = 0; i < activeLists.Count; i++)
			{
				string saveData = activeLists[i].GetSaveData(null);
				if (!string.IsNullOrEmpty(saveData))
				{
					text += saveData;
					if (i < activeLists.Count - 1)
					{
						text += "|";
					}
				}
			}
			return text;
		}

		public void LoadData(string _dataString)
		{
			activeLists.Clear();
			if (string.IsNullOrEmpty(_dataString))
			{
				return;
			}
			string[] array = _dataString.Split("|"[0]);
			string[] array2 = array;
			foreach (string dataString in array2)
			{
				ActiveList activeList = new ActiveList();
				if (activeList.LoadData(dataString))
				{
					activeLists.Add(activeList);
				}
			}
		}

		protected void PurgeLists()
		{
			for (int i = 0; i < activeLists.Count; i++)
			{
				if (!activeLists[i].IsNecessary())
				{
					activeLists.RemoveAt(i);
					i--;
				}
			}
		}
	}
}
