using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionSaveHandle : Action
	{
		public SaveHandling saveHandling;

		public SelectSaveType selectSaveType;

		public int saveIndex;

		public int saveIndexParameterID = -1;

		public int varID;

		public int slotVarID;

		public string menuName = string.Empty;

		public string elementName = string.Empty;

		public bool updateLabel;

		public bool customLabel;

		public bool doSelectiveLoad;

		public SelectiveLoad selectiveLoad = new SelectiveLoad();

		protected bool recievedCallback;

		public ActionSaveHandle()
		{
			isDisplayed = true;
			category = ActionCategory.Save;
			title = "Save or load";
			description = "Saves and loads save-game files";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			saveIndex = AssignInteger(parameters, saveIndexParameterID, saveIndex);
		}

		public override float Run()
		{
			if (!isRunning)
			{
				isRunning = true;
				recievedCallback = false;
				PerformSaveOrLoad();
			}
			if (recievedCallback)
			{
				isRunning = false;
				return 0f;
			}
			return base.defaultPauseTime;
		}

		protected void PerformSaveOrLoad()
		{
			ClearAllEvents();
			if (saveHandling == SaveHandling.ContinueFromLastSave || saveHandling == SaveHandling.LoadGame)
			{
				EventManager.OnFinishLoading = (EventManager.Delegate_Generic)Delegate.Combine(EventManager.OnFinishLoading, new EventManager.Delegate_Generic(OnFinishLoading));
				EventManager.OnFailLoading = (EventManager.Delegate_SaveID)Delegate.Combine(EventManager.OnFailLoading, new EventManager.Delegate_SaveID(OnFail));
			}
			else if (saveHandling == SaveHandling.OverwriteExistingSave || saveHandling == SaveHandling.SaveNewGame)
			{
				EventManager.OnFinishSaving = (EventManager.Delegate_SaveFile)Delegate.Combine(EventManager.OnFinishSaving, new EventManager.Delegate_SaveFile(OnFinishSaving));
				EventManager.OnFailSaving = (EventManager.Delegate_SaveID)Delegate.Combine(EventManager.OnFailSaving, new EventManager.Delegate_SaveID(OnFail));
			}
			if ((saveHandling == SaveHandling.LoadGame || saveHandling == SaveHandling.ContinueFromLastSave) && doSelectiveLoad)
			{
				KickStarter.saveSystem.SetSelectiveLoadOptions(selectiveLoad);
			}
			string newLabel = string.Empty;
			if (customLabel && ((updateLabel && saveHandling == SaveHandling.OverwriteExistingSave) || saveHandling == SaveHandling.SaveNewGame) && selectSaveType != SelectSaveType.Autosave)
			{
				GVar variable = GlobalVariables.GetVariable(varID);
				if (variable == null)
				{
					LogWarning("Could not " + saveHandling.ToString() + " - no variable found.");
					return;
				}
				newLabel = variable.GetValue(Options.GetLanguage());
			}
			int num = Mathf.Max(0, saveIndex);
			if (saveHandling == SaveHandling.ContinueFromLastSave)
			{
				SaveSystem.ContinueGame();
				return;
			}
			if (saveHandling == SaveHandling.LoadGame || saveHandling == SaveHandling.OverwriteExistingSave)
			{
				if (selectSaveType == SelectSaveType.Autosave)
				{
					if (saveHandling == SaveHandling.LoadGame)
					{
						SaveSystem.LoadAutoSave();
					}
					else if (PlayerMenus.IsSavingLocked(this, true))
					{
						OnComplete();
					}
					else
					{
						SaveSystem.SaveAutoSave();
					}
					return;
				}
				if (selectSaveType == SelectSaveType.SlotIndexFromVariable)
				{
					GVar variable2 = GlobalVariables.GetVariable(slotVarID);
					if (variable2 == null)
					{
						LogWarning("Could not get save slot index - no variable found.");
						return;
					}
					num = variable2.val;
				}
			}
			if (!string.IsNullOrEmpty(menuName) && !string.IsNullOrEmpty(elementName))
			{
				MenuElement elementWithName = PlayerMenus.GetElementWithName(menuName, elementName);
				if (elementWithName != null && elementWithName is MenuSavesList)
				{
					MenuSavesList menuSavesList = (MenuSavesList)elementWithName;
					num += menuSavesList.GetOffset();
				}
				else
				{
					LogWarning("Cannot find ProfilesList element '" + elementName + "' in Menu '" + menuName + "'.");
				}
			}
			else
			{
				LogWarning("No SavesList element referenced when trying to find slot slot " + num);
			}
			if (saveHandling == SaveHandling.LoadGame)
			{
				SaveSystem.LoadGame(num, -1, false);
			}
			else if (saveHandling == SaveHandling.OverwriteExistingSave || saveHandling == SaveHandling.SaveNewGame)
			{
				if (PlayerMenus.IsSavingLocked(this, true))
				{
					OnComplete();
				}
				else if (saveHandling == SaveHandling.OverwriteExistingSave)
				{
					SaveSystem.SaveGame(num, -1, false, updateLabel, newLabel);
				}
				else if (saveHandling == SaveHandling.SaveNewGame)
				{
					SaveSystem.SaveNewGame(updateLabel, newLabel);
				}
			}
		}

		protected void OnFinishLoading()
		{
			OnComplete();
		}

		protected void OnFinishSaving(SaveFile saveFile)
		{
			OnComplete();
		}

		protected void OnComplete()
		{
			ClearAllEvents();
			recievedCallback = true;
		}

		protected void OnFail(int saveID)
		{
			OnComplete();
		}

		protected void ClearAllEvents()
		{
			EventManager.OnFinishLoading = (EventManager.Delegate_Generic)Delegate.Remove(EventManager.OnFinishLoading, new EventManager.Delegate_Generic(OnFinishLoading));
			EventManager.OnFailLoading = (EventManager.Delegate_SaveID)Delegate.Remove(EventManager.OnFailLoading, new EventManager.Delegate_SaveID(OnFail));
			EventManager.OnFinishSaving = (EventManager.Delegate_SaveFile)Delegate.Remove(EventManager.OnFinishSaving, new EventManager.Delegate_SaveFile(OnFinishSaving));
			EventManager.OnFailSaving = (EventManager.Delegate_SaveID)Delegate.Remove(EventManager.OnFailSaving, new EventManager.Delegate_SaveID(OnFail));
		}

		public override ActionEnd End(List<Action> actions)
		{
			if (saveHandling == SaveHandling.OverwriteExistingSave || saveHandling == SaveHandling.SaveNewGame)
			{
				return base.End(actions);
			}
			return GenerateStopActionEnd();
		}

		public static ActionSaveHandle CreateNew_SaveNew(int customLabelGlobalStringVariableID = -1)
		{
			ActionSaveHandle actionSaveHandle = ScriptableObject.CreateInstance<ActionSaveHandle>();
			actionSaveHandle.saveHandling = SaveHandling.SaveNewGame;
			actionSaveHandle.customLabel = customLabelGlobalStringVariableID >= 0;
			actionSaveHandle.varID = customLabelGlobalStringVariableID;
			return actionSaveHandle;
		}

		public static ActionSaveHandle CreateNew_SaveAutosave()
		{
			ActionSaveHandle actionSaveHandle = ScriptableObject.CreateInstance<ActionSaveHandle>();
			actionSaveHandle.saveHandling = SaveHandling.OverwriteExistingSave;
			actionSaveHandle.selectSaveType = SelectSaveType.Autosave;
			return actionSaveHandle;
		}

		public static ActionSaveHandle CreateNew_LoadFromSlot(string menuName, string savesListElementName, int saveSlotIndex)
		{
			ActionSaveHandle actionSaveHandle = ScriptableObject.CreateInstance<ActionSaveHandle>();
			actionSaveHandle.saveHandling = SaveHandling.LoadGame;
			actionSaveHandle.selectSaveType = SelectSaveType.SetSlotIndex;
			actionSaveHandle.saveIndex = saveSlotIndex;
			actionSaveHandle.menuName = menuName;
			actionSaveHandle.elementName = savesListElementName;
			return actionSaveHandle;
		}

		public static ActionSaveHandle CreateNew_LoadAutosave()
		{
			ActionSaveHandle actionSaveHandle = ScriptableObject.CreateInstance<ActionSaveHandle>();
			actionSaveHandle.saveHandling = SaveHandling.LoadGame;
			actionSaveHandle.selectSaveType = SelectSaveType.Autosave;
			return actionSaveHandle;
		}

		public static ActionSaveHandle CreateNew_ContinueLast()
		{
			ActionSaveHandle actionSaveHandle = ScriptableObject.CreateInstance<ActionSaveHandle>();
			actionSaveHandle.saveHandling = SaveHandling.ContinueFromLastSave;
			return actionSaveHandle;
		}

		public static ActionSaveHandle CreateNew_SaveInSlot(string menuName, string savesListElementName, int saveSlotIndex)
		{
			ActionSaveHandle actionSaveHandle = ScriptableObject.CreateInstance<ActionSaveHandle>();
			actionSaveHandle.saveHandling = SaveHandling.OverwriteExistingSave;
			actionSaveHandle.selectSaveType = SelectSaveType.SetSlotIndex;
			actionSaveHandle.saveIndex = saveSlotIndex;
			actionSaveHandle.menuName = menuName;
			actionSaveHandle.elementName = savesListElementName;
			return actionSaveHandle;
		}
	}
}
