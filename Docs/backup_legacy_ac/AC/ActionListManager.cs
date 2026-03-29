using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_action_list_manager.html")]
	public class ActionListManager : MonoBehaviour
	{
		[HideInInspector]
		public bool ignoreNextConversationSkip;

		protected bool playCutsceneOnVarChange;

		protected bool saveAfterCutscene;

		protected int playerIDOnStartQueue;

		protected bool noPlayerOnStartQueue;

		[HideInInspector]
		public List<ActiveList> activeLists = new List<ActiveList>();

		public void OnAwake()
		{
			activeLists.Clear();
		}

		protected void OnDestroy()
		{
			activeLists.Clear();
		}

		public void UpdateActionListManager()
		{
			if (saveAfterCutscene && !IsGameplayBlocked())
			{
				saveAfterCutscene = false;
				SaveSystem.SaveAutoSave();
			}
			if (playCutsceneOnVarChange && (bool)KickStarter.stateHandler && (KickStarter.stateHandler.gameState == GameState.Normal || KickStarter.stateHandler.gameState == GameState.DialogOptions))
			{
				playCutsceneOnVarChange = false;
				if (KickStarter.sceneSettings.actionListSource == ActionListSource.InScene && KickStarter.sceneSettings.cutsceneOnVarChange != null)
				{
					KickStarter.sceneSettings.cutsceneOnVarChange.Interact();
				}
				else if (KickStarter.sceneSettings.actionListSource == ActionListSource.AssetFile && KickStarter.sceneSettings.actionListAssetOnVarChange != null)
				{
					KickStarter.sceneSettings.actionListAssetOnVarChange.Interact();
				}
			}
		}

		public void EndCutscene()
		{
			if (!IsInSkippableCutscene())
			{
				return;
			}
			if (AdvGame.GetReferences().settingsManager.blackOutWhenSkipping)
			{
				KickStarter.mainCamera.HideScene();
			}
			Sound[] array = Object.FindObjectsOfType(typeof(Sound)) as Sound[];
			Sound[] array2 = array;
			foreach (Sound sound in array2)
			{
				if ((bool)sound.GetComponent<AudioSource>() && sound.soundType != SoundType.Music && !sound.GetComponent<AudioSource>().loop)
				{
					sound.Stop();
				}
			}
			if (KickStarter.settingsManager.playerSwitching == PlayerSwitching.Allow)
			{
				if (KickStarter.player != null && !noPlayerOnStartQueue && playerIDOnStartQueue != KickStarter.player.ID && playerIDOnStartQueue >= 0)
				{
					Player player = KickStarter.settingsManager.GetPlayer(playerIDOnStartQueue);
					KickStarter.ResetPlayer(player, playerIDOnStartQueue, true, Quaternion.identity, false, true);
				}
				else if (KickStarter.player != null && noPlayerOnStartQueue)
				{
					KickStarter.ResetPlayer(null, KickStarter.settingsManager.GetEmptyPlayerID(), true, Quaternion.identity, false, true);
				}
				else if (KickStarter.player == null && !noPlayerOnStartQueue && playerIDOnStartQueue >= 0)
				{
					Player player2 = KickStarter.settingsManager.GetPlayer(playerIDOnStartQueue);
					KickStarter.ResetPlayer(player2, playerIDOnStartQueue, true, Quaternion.identity, false, true);
				}
			}
			List<ActiveList> list = new List<ActiveList>();
			List<ActiveList> list2 = new List<ActiveList>();
			foreach (ActiveList activeList in activeLists)
			{
				if (!activeList.inSkipQueue && activeList.actionList.IsSkippable())
				{
					list2.Add(activeList);
				}
				else
				{
					list.Add(activeList);
				}
			}
			foreach (ActiveList activeList2 in KickStarter.actionListAssetManager.activeLists)
			{
				if (!activeList2.inSkipQueue && activeList2.actionList.IsSkippable())
				{
					list2.Add(activeList2);
				}
				else
				{
					list.Add(activeList2);
				}
			}
			foreach (ActiveList item in list2)
			{
				item.Reset(true);
			}
			foreach (ActiveList item2 in list)
			{
				item2.Skip();
			}
		}

		public bool IsListRunning(ActionList actionList)
		{
			if (actionList == null)
			{
				return false;
			}
			RuntimeActionList runtimeActionList = actionList as RuntimeActionList;
			if (runtimeActionList != null)
			{
				foreach (ActiveList activeList in KickStarter.actionListAssetManager.activeLists)
				{
					if (activeList.IsFor(runtimeActionList) && activeList.IsRunning())
					{
						return true;
					}
				}
				return false;
			}
			foreach (ActiveList activeList2 in activeLists)
			{
				if (activeList2.IsFor(actionList) && activeList2.IsRunning())
				{
					return true;
				}
			}
			return false;
		}

		public bool IsListRegistered(ActionList actionList)
		{
			if (actionList == null)
			{
				return false;
			}
			RuntimeActionList runtimeActionList = actionList as RuntimeActionList;
			if (runtimeActionList != null)
			{
				foreach (ActiveList activeList in KickStarter.actionListAssetManager.activeLists)
				{
					if (activeList.IsFor(runtimeActionList))
					{
						return true;
					}
				}
				return false;
			}
			foreach (ActiveList activeList2 in activeLists)
			{
				if (activeList2.IsFor(actionList))
				{
					return true;
				}
			}
			return false;
		}

		public bool CanResetSkipVars(ActionList actionList)
		{
			RuntimeActionList runtimeActionList = actionList as RuntimeActionList;
			if (runtimeActionList != null)
			{
				foreach (ActiveList activeList in KickStarter.actionListAssetManager.activeLists)
				{
					if (activeList.IsFor(runtimeActionList))
					{
						return activeList.CanResetSkipVars();
					}
					if (activeList.IsFor(runtimeActionList.assetSource))
					{
						return activeList.CanResetSkipVars();
					}
				}
				return true;
			}
			foreach (ActiveList activeList2 in activeLists)
			{
				if (activeList2.IsFor(actionList))
				{
					return activeList2.CanResetSkipVars();
				}
			}
			return true;
		}

		public bool IsGameplayBlocked(Action _actionToIgnore = null, bool showSaveDebug = false)
		{
			if (KickStarter.stateHandler.IsInScriptedCutscene())
			{
				if (showSaveDebug)
				{
					ACDebug.LogWarning("Cannot save at this time - currently in a scripted cutscene.");
				}
				return true;
			}
			foreach (ActiveList activeList in activeLists)
			{
				if (activeList.actionList.actionListType != ActionListType.PauseGameplay || !activeList.IsRunning() || (_actionToIgnore != null && activeList.actionList.actions.Contains(_actionToIgnore)))
				{
					continue;
				}
				if (showSaveDebug)
				{
					ACDebug.LogWarning("Cannot save at this time - the ActionList '" + activeList.actionList.name + "' is blocking gameplay.", activeList.actionList);
				}
				return true;
			}
			foreach (ActiveList activeList2 in KickStarter.actionListAssetManager.activeLists)
			{
				if (!(activeList2.actionList != null) || activeList2.actionList.actionListType != ActionListType.PauseGameplay || !activeList2.IsRunning() || (_actionToIgnore != null && activeList2.actionList.actions.Contains(_actionToIgnore)))
				{
					continue;
				}
				if (showSaveDebug)
				{
					ACDebug.LogWarning("Cannot save at this time - the ActionListAsset '" + activeList2.actionList.name + "' is blocking gameplay.", activeList2.actionList);
				}
				return true;
			}
			return false;
		}

		public bool IsOverrideConversationRunning()
		{
			if (KickStarter.playerInput.activeConversation != null)
			{
				foreach (ActiveList activeList in activeLists)
				{
					if (activeList.IsConversationOverride())
					{
						return true;
					}
				}
				foreach (ActiveList activeList2 in KickStarter.actionListAssetManager.activeLists)
				{
					if (activeList2.IsConversationOverride())
					{
						return true;
					}
				}
			}
			return false;
		}

		public bool IsGameplayBlockedAndUnfrozen()
		{
			foreach (ActiveList activeList in activeLists)
			{
				if (activeList.CanUnfreezePauseMenus() && activeList.IsRunning())
				{
					return true;
				}
			}
			foreach (ActiveList activeList2 in KickStarter.actionListAssetManager.activeLists)
			{
				if (activeList2.CanUnfreezePauseMenus() && activeList2.IsRunning())
				{
					return true;
				}
			}
			return false;
		}

		public bool IsInSkippableCutscene()
		{
			if (!IsGameplayBlocked())
			{
				return false;
			}
			if (HasSkipQueue())
			{
				return true;
			}
			foreach (ActiveList activeList in activeLists)
			{
				if (activeList.IsRunning() && activeList.actionList.IsSkippable())
				{
					return true;
				}
			}
			foreach (ActiveList activeList2 in KickStarter.actionListAssetManager.activeLists)
			{
				if (activeList2.IsRunning() && activeList2.actionListAsset != null && activeList2.actionListAsset.IsSkippable())
				{
					return true;
				}
			}
			return false;
		}

		public void AddToList(ActionList actionList, bool addToSkipQueue, int _startIndex)
		{
			for (int i = 0; i < activeLists.Count; i++)
			{
				if (activeLists[i].IsFor(actionList))
				{
					activeLists.RemoveAt(i);
				}
			}
			addToSkipQueue = CanAddToSkipQueue(actionList, addToSkipQueue);
			activeLists.Add(new ActiveList(actionList, addToSkipQueue, _startIndex));
			if (!KickStarter.playerMenus.ArePauseMenusOn() || (actionList.actionListType != ActionListType.RunInBackground && (!(actionList is RuntimeActionList) || actionList.actionListType != ActionListType.PauseGameplay || actionList.unfreezePauseMenus)))
			{
				SetCorrectGameState();
			}
		}

		public void EndList(ActionList actionList)
		{
			if (actionList == null)
			{
				return;
			}
			for (int i = 0; i < activeLists.Count; i++)
			{
				if (activeLists[i].IsFor(actionList))
				{
					EndList(activeLists[i]);
					break;
				}
			}
		}

		public void EndList(ActiveList activeList)
		{
			activeList.Reset(false);
			if ((bool)activeList.GetConversationOnEnd())
			{
				ResetSkipVars();
				activeList.RunConversation();
			}
			else if (activeList.actionListAsset != null && activeList.actionList.actionListType == ActionListType.PauseGameplay && !activeList.actionList.unfreezePauseMenus && KickStarter.playerMenus.ArePauseMenusOn())
			{
				if (KickStarter.stateHandler.gameState != GameState.Cutscene)
				{
					ResetSkipVars();
				}
				PurgeLists();
			}
			else
			{
				SetCorrectGameStateEnd();
			}
		}

		public void VariableChanged()
		{
			playCutsceneOnVarChange = true;
		}

		public void KillAllLists()
		{
			foreach (ActiveList activeList in activeLists)
			{
				activeList.Reset(true);
			}
			foreach (ActiveList activeList2 in KickStarter.actionListAssetManager.activeLists)
			{
				activeList2.Reset(true);
			}
		}

		public void KillAllFromScene(SceneInfo sceneInfo)
		{
			foreach (ActiveList activeList in activeLists)
			{
				if (activeList.actionList != null && sceneInfo.Matches(UnityVersionHandler.GetSceneInfoFromGameObject(activeList.actionList.gameObject)) && activeList.actionListAsset == null)
				{
					activeList.Reset(true);
				}
			}
		}

		public void ResetSkippableData()
		{
			ResetSkipVars(true);
		}

		public void SetCorrectGameState()
		{
			if (KickStarter.stateHandler != null)
			{
				if (IsGameplayBlocked())
				{
					if (KickStarter.stateHandler.gameState != GameState.Cutscene)
					{
						ResetSkipVars();
					}
					KickStarter.stateHandler.gameState = GameState.Cutscene;
					if (IsGameplayBlockedAndUnfrozen())
					{
						KickStarter.sceneSettings.UnpauseGame(KickStarter.playerInput.timeScale);
					}
				}
				else if (KickStarter.playerMenus.ArePauseMenusOn())
				{
					KickStarter.stateHandler.gameState = GameState.Paused;
					KickStarter.sceneSettings.PauseGame();
				}
				else if (KickStarter.playerInput.IsInConversation(true))
				{
					KickStarter.stateHandler.gameState = GameState.DialogOptions;
				}
				else
				{
					KickStarter.stateHandler.gameState = GameState.Normal;
				}
			}
			else
			{
				ACDebug.LogWarning("Could not set correct GameState!");
			}
		}

		public void SetConversationPoint(ActionConversation actionConversation)
		{
			foreach (ActiveList activeList in activeLists)
			{
				activeList.SetConversationOverride(actionConversation);
			}
			foreach (ActiveList activeList2 in KickStarter.actionListAssetManager.activeLists)
			{
				activeList2.SetConversationOverride(actionConversation);
			}
		}

		public bool OverrideConversation(int optionIndex)
		{
			foreach (ActiveList activeList in activeLists)
			{
				if (activeList.ResumeConversationOverride())
				{
					return true;
				}
			}
			foreach (ActiveList activeList2 in KickStarter.actionListAssetManager.activeLists)
			{
				if (activeList2.ResumeConversationOverride())
				{
					return true;
				}
			}
			return false;
		}

		public void OnEndConversation()
		{
			for (int i = 0; i < activeLists.Count; i++)
			{
				if (activeLists[i].IsConversationOverride())
				{
					activeLists[i].Reset(true);
					activeLists.RemoveAt(i);
					i--;
				}
			}
			for (int j = 0; j < KickStarter.actionListAssetManager.activeLists.Count; j++)
			{
				if (KickStarter.actionListAssetManager.activeLists[j].IsConversationOverride())
				{
					KickStarter.actionListAssetManager.activeLists[j].Reset(true);
					KickStarter.actionListAssetManager.activeLists.RemoveAt(j);
					j--;
				}
			}
		}

		public bool CanAddToSkipQueue(ActionList actionList, bool originalValue)
		{
			if (!actionList.IsSkippable())
			{
				return false;
			}
			if (!KickStarter.actionListManager.HasSkipQueue())
			{
				if ((bool)KickStarter.player)
				{
					playerIDOnStartQueue = KickStarter.player.ID;
					noPlayerOnStartQueue = false;
				}
				else
				{
					noPlayerOnStartQueue = true;
				}
				return true;
			}
			return originalValue;
		}

		public void AssignResumeIndices(ActionList actionList, int[] resumeIndices)
		{
			foreach (ActiveList activeList in activeLists)
			{
				if (activeList.IsFor(actionList))
				{
					activeList.SetResumeIndices(resumeIndices);
				}
			}
		}

		public void Resume(ActionList actionList, bool rerunPausedActions)
		{
			if (IsListRunning(actionList))
			{
				return;
			}
			for (int i = 0; i < activeLists.Count; i++)
			{
				if (activeLists[i].IsFor(actionList))
				{
					activeLists[i].Resume(null, rerunPausedActions);
					return;
				}
			}
			actionList.Interact();
		}

		public string GetSaveData(SubScene subScene = null)
		{
			PurgeLists();
			string text = string.Empty;
			for (int i = 0; i < activeLists.Count; i++)
			{
				text += activeLists[i].GetSaveData(subScene);
				if (i < activeLists.Count - 1)
				{
					text += "|";
				}
			}
			return text;
		}

		public void LoadData(string _dataString, SubScene subScene = null)
		{
			if (subScene == null)
			{
				activeLists.Clear();
			}
			if (string.IsNullOrEmpty(_dataString))
			{
				return;
			}
			string[] array = _dataString.Split("|"[0]);
			string[] array2 = array;
			foreach (string dataString in array2)
			{
				ActiveList activeList = new ActiveList();
				if (activeList.LoadData(dataString, subScene))
				{
					activeLists.Add(activeList);
				}
			}
		}

		protected void SetCorrectGameStateEnd()
		{
			if (KickStarter.stateHandler != null)
			{
				if (KickStarter.playerMenus.ArePauseMenusOn() && !IsGameplayBlockedAndUnfrozen())
				{
					KickStarter.mainCamera.PauseGame(true);
				}
				else
				{
					KickStarter.stateHandler.RestoreLastNonPausedState();
				}
				if (KickStarter.stateHandler.gameState != GameState.Cutscene)
				{
					ResetSkipVars();
				}
			}
			else
			{
				ACDebug.LogWarning("Could not set correct GameState!");
			}
			PurgeLists();
		}

		protected void PurgeLists()
		{
			bool flag = false;
			for (int i = 0; i < activeLists.Count; i++)
			{
				if (!activeLists[i].IsNecessary())
				{
					if (!saveAfterCutscene && !flag && activeLists[i].actionList != null && activeLists[i].actionList.autosaveAfter)
					{
						flag = true;
					}
					activeLists.RemoveAt(i);
					i--;
				}
			}
			for (int j = 0; j < KickStarter.actionListAssetManager.activeLists.Count; j++)
			{
				if (!KickStarter.actionListAssetManager.activeLists[j].IsNecessary())
				{
					KickStarter.actionListAssetManager.activeLists.RemoveAt(j);
					j--;
				}
			}
			if (flag)
			{
				if (!IsGameplayBlocked())
				{
					SaveSystem.SaveAutoSave();
				}
				else
				{
					saveAfterCutscene = true;
				}
			}
		}

		protected void ResetSkipVars(bool ignoreBlockCheck = false)
		{
			if (!ignoreBlockCheck && IsGameplayBlocked())
			{
				return;
			}
			ignoreNextConversationSkip = false;
			foreach (ActiveList activeList in activeLists)
			{
				activeList.inSkipQueue = false;
			}
			foreach (ActiveList activeList2 in KickStarter.actionListAssetManager.activeLists)
			{
				activeList2.inSkipQueue = false;
			}
			GlobalVariables.BackupAll();
			KickStarter.localVariables.BackupAllValues();
		}

		protected bool HasSkipQueue()
		{
			foreach (ActiveList activeList in activeLists)
			{
				if (activeList.IsRunning() && activeList.inSkipQueue)
				{
					return true;
				}
			}
			foreach (ActiveList activeList2 in KickStarter.actionListAssetManager.activeLists)
			{
				if (activeList2.IsRunning() && activeList2.inSkipQueue)
				{
					return true;
				}
			}
			return false;
		}

		public static void KillAll()
		{
			KickStarter.actionListManager.KillAllLists();
		}
	}
}
