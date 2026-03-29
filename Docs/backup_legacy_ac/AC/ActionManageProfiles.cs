using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionManageProfiles : Action
	{
		public ManageProfileType manageProfileType;

		public DeleteProfileType deleteProfileType;

		public int profileIndex;

		public int profileIndexParameterID = -1;

		public int varID;

		public int slotVarID;

		public bool useCustomLabel;

		public string menuName = string.Empty;

		public string elementName = string.Empty;

		public ActionManageProfiles()
		{
			isDisplayed = true;
			category = ActionCategory.Save;
			title = "Manage profiles";
			description = "Creates, renames and deletes save game profiles.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			profileIndex = AssignInteger(parameters, profileIndexParameterID, profileIndex);
		}

		public override float Run()
		{
			if (!KickStarter.settingsManager.useProfiles)
			{
				LogWarning("Save game profiles are not enabled - please set in Settings Manager to use this Action.");
				return 0f;
			}
			string text = string.Empty;
			if ((manageProfileType == ManageProfileType.CreateProfile && useCustomLabel) || manageProfileType == ManageProfileType.RenameProfile)
			{
				GVar variable = GlobalVariables.GetVariable(varID);
				if (variable == null)
				{
					LogWarning("Could not " + manageProfileType.ToString() + " - no variable found.");
					return 0f;
				}
				text = variable.textVal;
			}
			if (manageProfileType == ManageProfileType.CreateProfile)
			{
				KickStarter.options.CreateProfile(text);
			}
			else if (manageProfileType == ManageProfileType.DeleteProfile || manageProfileType == ManageProfileType.RenameProfile || manageProfileType == ManageProfileType.SwitchActiveProfile)
			{
				if (deleteProfileType == DeleteProfileType.ActiveProfile)
				{
					if (manageProfileType == ManageProfileType.DeleteProfile)
					{
						KickStarter.saveSystem.DeleteProfile();
					}
					else if (manageProfileType == ManageProfileType.RenameProfile)
					{
						KickStarter.options.RenameProfile(text);
					}
					return 0f;
				}
				if (deleteProfileType == DeleteProfileType.SetProfileID)
				{
					int profileID = Mathf.Max(0, profileIndex);
					if (manageProfileType == ManageProfileType.DeleteProfile)
					{
						KickStarter.saveSystem.DeleteProfileID(profileID);
					}
					else if (manageProfileType == ManageProfileType.RenameProfile)
					{
						KickStarter.options.RenameProfileID(text, profileID);
					}
					else if (manageProfileType == ManageProfileType.SwitchActiveProfile)
					{
						Options.SwitchProfileID(profileID);
					}
				}
				else if (deleteProfileType == DeleteProfileType.SetSlotIndex || deleteProfileType == DeleteProfileType.SlotIndexFromVariable)
				{
					int num = Mathf.Max(0, profileIndex);
					if (deleteProfileType == DeleteProfileType.SlotIndexFromVariable)
					{
						GVar variable2 = GlobalVariables.GetVariable(slotVarID);
						if (variable2 == null)
						{
							LogWarning("Could not " + manageProfileType.ToString() + " - no variable found.");
							return 0f;
						}
						num = variable2.val;
					}
					bool includeActive = true;
					if (menuName != string.Empty && elementName != string.Empty)
					{
						MenuElement elementWithName = PlayerMenus.GetElementWithName(menuName, elementName);
						if (elementWithName != null && elementWithName is MenuProfilesList)
						{
							MenuProfilesList menuProfilesList = (MenuProfilesList)elementWithName;
							if (menuProfilesList.fixedOption)
							{
								LogWarning("Cannot refer to ProfilesLst " + elementName + " in Menu " + menuName + ", as it lists a fixed profile ID only!");
								return 0f;
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
					if (manageProfileType == ManageProfileType.DeleteProfile)
					{
						KickStarter.saveSystem.DeleteProfile(num, includeActive);
					}
					else if (manageProfileType == ManageProfileType.RenameProfile)
					{
						KickStarter.options.RenameProfile(text, num, includeActive);
					}
					else if (manageProfileType == ManageProfileType.SwitchActiveProfile)
					{
						KickStarter.options.SwitchProfile(num, includeActive);
					}
				}
			}
			return 0f;
		}

		public static ActionManageProfiles CreateNew_CreateProfile(int labelGlobalStringVariableID = -1)
		{
			ActionManageProfiles actionManageProfiles = ScriptableObject.CreateInstance<ActionManageProfiles>();
			actionManageProfiles.manageProfileType = ManageProfileType.CreateProfile;
			actionManageProfiles.useCustomLabel = labelGlobalStringVariableID >= 0;
			actionManageProfiles.varID = labelGlobalStringVariableID;
			return actionManageProfiles;
		}

		public static ActionManageProfiles CreateNew_DeleteProfile(DeleteProfileType deleteProfileType, string menuName, string elementName, int indexOrID)
		{
			ActionManageProfiles actionManageProfiles = ScriptableObject.CreateInstance<ActionManageProfiles>();
			actionManageProfiles.manageProfileType = ManageProfileType.DeleteProfile;
			actionManageProfiles.deleteProfileType = deleteProfileType;
			actionManageProfiles.profileIndex = indexOrID;
			actionManageProfiles.slotVarID = indexOrID;
			return actionManageProfiles;
		}

		public static ActionManageProfiles CreateNew_RenameProfile(int labelGlobalStringVariableID, DeleteProfileType renameProfileType, string menuName, string elementName, int indexOrID)
		{
			ActionManageProfiles actionManageProfiles = ScriptableObject.CreateInstance<ActionManageProfiles>();
			actionManageProfiles.manageProfileType = ManageProfileType.RenameProfile;
			actionManageProfiles.deleteProfileType = renameProfileType;
			actionManageProfiles.varID = labelGlobalStringVariableID;
			actionManageProfiles.profileIndex = indexOrID;
			actionManageProfiles.slotVarID = indexOrID;
			return actionManageProfiles;
		}

		public static ActionManageProfiles CreateNew_SwitchActiveProfile(DeleteProfileType selectProfileType, string menuName, string elementName, int indexOrID)
		{
			ActionManageProfiles actionManageProfiles = ScriptableObject.CreateInstance<ActionManageProfiles>();
			actionManageProfiles.manageProfileType = ManageProfileType.SwitchActiveProfile;
			actionManageProfiles.deleteProfileType = selectProfileType;
			actionManageProfiles.profileIndex = indexOrID;
			actionManageProfiles.slotVarID = indexOrID;
			return actionManageProfiles;
		}
	}
}
