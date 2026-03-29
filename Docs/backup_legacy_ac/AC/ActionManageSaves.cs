using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionManageSaves : Action
	{
		public ManageSaveType manageSaveType;

		public SelectSaveType selectSaveType = SelectSaveType.SetSlotIndex;

		public int saveIndex;

		public int saveIndexParameterID = -1;

		public int varID;

		public int slotVarID;

		public string menuName = string.Empty;

		public string elementName = string.Empty;

		public ActionManageSaves()
		{
			isDisplayed = true;
			category = ActionCategory.Save;
			title = "Manage saves";
			description = "Renames and deletes save game files.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			saveIndex = AssignInteger(parameters, saveIndexParameterID, saveIndex);
		}

		public override float Run()
		{
			string newLabel = string.Empty;
			if (manageSaveType == ManageSaveType.RenameSave)
			{
				GVar variable = GlobalVariables.GetVariable(varID);
				if (variable == null)
				{
					LogWarning("Could not " + manageSaveType.ToString() + " - no variable found.");
					return 0f;
				}
				newLabel = variable.textVal;
			}
			int num = Mathf.Max(0, saveIndex);
			if (selectSaveType == SelectSaveType.SlotIndexFromVariable)
			{
				GVar variable2 = GlobalVariables.GetVariable(slotVarID);
				if (variable2 == null)
				{
					LogWarning("Could not rename save - no variable found.");
					return 0f;
				}
				num = variable2.val;
			}
			else if (selectSaveType == SelectSaveType.Autosave)
			{
				if (manageSaveType == ManageSaveType.DeleteSave)
				{
					SaveSystem.DeleteSave(0);
				}
				else if (manageSaveType == ManageSaveType.RenameSave)
				{
					return 0f;
				}
			}
			if (menuName != string.Empty && elementName != string.Empty)
			{
				MenuElement elementWithName = PlayerMenus.GetElementWithName(menuName, elementName);
				if (elementWithName != null && elementWithName is MenuSavesList)
				{
					MenuSavesList menuSavesList = (MenuSavesList)elementWithName;
					num += menuSavesList.GetOffset();
				}
				else
				{
					LogWarning("Cannot find SavesList element '" + elementName + "' in Menu '" + menuName + "'.");
				}
			}
			else
			{
				LogWarning("No SavesList element referenced when trying to find save slot " + num);
			}
			if (manageSaveType == ManageSaveType.DeleteSave)
			{
				KickStarter.saveSystem.DeleteSave(num, -1, false);
			}
			else if (manageSaveType == ManageSaveType.RenameSave)
			{
				KickStarter.saveSystem.RenameSave(newLabel, num);
			}
			return 0f;
		}

		public static ActionManageSaves CreateNew_DeleteSave(string menuName, string savesListElementName, int slotIndex = -1)
		{
			ActionManageSaves actionManageSaves = ScriptableObject.CreateInstance<ActionManageSaves>();
			actionManageSaves.manageSaveType = ManageSaveType.DeleteSave;
			actionManageSaves.selectSaveType = ((slotIndex >= 0) ? SelectSaveType.SetSlotIndex : SelectSaveType.Autosave);
			actionManageSaves.saveIndex = slotIndex;
			actionManageSaves.menuName = menuName;
			actionManageSaves.elementName = savesListElementName;
			return actionManageSaves;
		}

		public static ActionManageSaves CreateNew_RenameSave(string menuName, string savesListElementName, int labelGlobalStringVariableID, int slotIndex)
		{
			ActionManageSaves actionManageSaves = ScriptableObject.CreateInstance<ActionManageSaves>();
			actionManageSaves.manageSaveType = ManageSaveType.RenameSave;
			actionManageSaves.selectSaveType = SelectSaveType.SetSlotIndex;
			actionManageSaves.slotVarID = labelGlobalStringVariableID;
			actionManageSaves.menuName = menuName;
			actionManageSaves.elementName = savesListElementName;
			return actionManageSaves;
		}
	}
}
