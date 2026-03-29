using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionSaveCheck : ActionCheck
	{
		public SaveCheck saveCheck;

		public bool includeAutoSaves = true;

		public bool checkByElementIndex;

		public bool checkByName;

		public int intValue;

		public int checkParameterID = -1;

		public IntCondition intCondition;

		public string menuName = string.Empty;

		public string elementName = string.Empty;

		public int profileVarID;

		public ActionSaveCheck()
		{
			isDisplayed = true;
			category = ActionCategory.Save;
			title = "Check";
			description = "Queries the number of save files or profiles created, or if saving is possible.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			intValue = AssignInteger(parameters, checkParameterID, intValue);
		}

		public override ActionEnd End(List<Action> actions)
		{
			int fieldValue = 0;
			if (saveCheck == SaveCheck.NumberOfSaveGames)
			{
				fieldValue = KickStarter.saveSystem.GetNumSaves(includeAutoSaves);
			}
			else if (saveCheck == SaveCheck.NumberOfProfiles)
			{
				fieldValue = KickStarter.options.GetNumProfiles();
			}
			else
			{
				if (saveCheck == SaveCheck.IsSlotEmpty)
				{
					return ProcessResult(!SaveSystem.DoesSaveExist(intValue, intValue, !checkByElementIndex), actions);
				}
				if (saveCheck == SaveCheck.DoesProfileExist)
				{
					if (checkByElementIndex)
					{
						int num = Mathf.Max(0, intValue);
						bool includeActive = true;
						if (menuName != string.Empty && elementName != string.Empty)
						{
							MenuElement elementWithName = PlayerMenus.GetElementWithName(menuName, elementName);
							if (elementWithName != null && elementWithName is MenuProfilesList)
							{
								MenuProfilesList menuProfilesList = (MenuProfilesList)elementWithName;
								if (menuProfilesList.fixedOption)
								{
									LogWarning("Cannot refer to ProfilesList " + elementName + " in Menu " + menuName + ", as it lists a fixed profile ID only!");
									return ProcessResult(false, actions);
								}
								num += menuProfilesList.GetOffset();
								includeActive = menuProfilesList.showActive;
							}
							else
							{
								LogWarning("Cannot find ProfilesList element '" + elementName + "' in Menu '" + menuName + "'.");
							}
						}
						else
						{
							LogWarning("No ProfilesList element referenced when trying to delete profile slot " + num);
						}
						bool result = KickStarter.options.DoesProfileExist(num, includeActive);
						return ProcessResult(result, actions);
					}
					bool result2 = Options.DoesProfileIDExist(intValue);
					return ProcessResult(result2, actions);
				}
				if (saveCheck == SaveCheck.DoesProfileNameExist)
				{
					bool result3 = false;
					GVar variable = GlobalVariables.GetVariable(profileVarID);
					if (variable != null)
					{
						string textVal = variable.textVal;
						result3 = KickStarter.options.DoesProfileExist(textVal);
					}
					else
					{
						LogWarning("Could not check for profile name - no variable found.");
					}
					return ProcessResult(result3, actions);
				}
				if (saveCheck == SaveCheck.IsSavingPossible)
				{
					return ProcessResult(!PlayerMenus.IsSavingLocked(this), actions);
				}
			}
			return ProcessResult(CheckCondition(fieldValue), actions);
		}

		protected bool CheckCondition(int fieldValue)
		{
			if (intCondition == IntCondition.EqualTo)
			{
				if (fieldValue == intValue)
				{
					return true;
				}
			}
			else if (intCondition == IntCondition.NotEqualTo)
			{
				if (fieldValue != intValue)
				{
					return true;
				}
			}
			else if (intCondition == IntCondition.LessThan)
			{
				if (fieldValue < intValue)
				{
					return true;
				}
			}
			else if (intCondition == IntCondition.MoreThan && fieldValue > intValue)
			{
				return true;
			}
			return false;
		}

		public static ActionSaveCheck CreateNew_NumberOfSaveGames(int numSaves, bool includeAutosave = true, IntCondition condition = IntCondition.EqualTo)
		{
			ActionSaveCheck actionSaveCheck = ScriptableObject.CreateInstance<ActionSaveCheck>();
			actionSaveCheck.saveCheck = SaveCheck.NumberOfSaveGames;
			actionSaveCheck.intValue = numSaves;
			actionSaveCheck.intCondition = condition;
			actionSaveCheck.includeAutoSaves = includeAutosave;
			return actionSaveCheck;
		}

		public static ActionSaveCheck CreateNew_NumberOfProfiles(int numProfiles, IntCondition condition = IntCondition.EqualTo)
		{
			ActionSaveCheck actionSaveCheck = ScriptableObject.CreateInstance<ActionSaveCheck>();
			actionSaveCheck.saveCheck = SaveCheck.NumberOfProfiles;
			actionSaveCheck.intValue = numProfiles;
			actionSaveCheck.intCondition = condition;
			return actionSaveCheck;
		}

		public static ActionSaveCheck CreateNew_IsSlotEmpty(int saveSlotID)
		{
			ActionSaveCheck actionSaveCheck = ScriptableObject.CreateInstance<ActionSaveCheck>();
			actionSaveCheck.saveCheck = SaveCheck.IsSlotEmpty;
			actionSaveCheck.intValue = saveSlotID;
			return actionSaveCheck;
		}

		public static ActionSaveCheck CreateNew_IsSavingPossible()
		{
			ActionSaveCheck actionSaveCheck = ScriptableObject.CreateInstance<ActionSaveCheck>();
			actionSaveCheck.saveCheck = SaveCheck.IsSavingPossible;
			return actionSaveCheck;
		}

		public static ActionSaveCheck CreateNew_DoesProfileExist(int profileSlotID)
		{
			ActionSaveCheck actionSaveCheck = ScriptableObject.CreateInstance<ActionSaveCheck>();
			actionSaveCheck.saveCheck = SaveCheck.DoesProfileExist;
			actionSaveCheck.intValue = profileSlotID;
			return actionSaveCheck;
		}

		public static ActionSaveCheck DoesProfileNameExist(int globalStringVariableIDWithName)
		{
			ActionSaveCheck actionSaveCheck = ScriptableObject.CreateInstance<ActionSaveCheck>();
			actionSaveCheck.saveCheck = SaveCheck.DoesProfileNameExist;
			actionSaveCheck.profileVarID = globalStringVariableIDWithName;
			return actionSaveCheck;
		}
	}
}
