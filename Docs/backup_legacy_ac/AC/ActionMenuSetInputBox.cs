using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionMenuSetInputBox : Action
	{
		public enum SetMenuInputBoxSource
		{
			EnteredHere = 0,
			FromGlobalVariable = 1
		}

		public string menuName;

		public int menuNameParameterID = -1;

		public string elementName;

		public int elementNameParameterID = -1;

		public string newLabel;

		public int newLabelParameterID = -1;

		public SetMenuInputBoxSource setMenuInputBoxSource;

		public int varID;

		public int varParameterID = -1;

		public ActionMenuSetInputBox()
		{
			isDisplayed = true;
			category = ActionCategory.Menu;
			title = "Set Input box text";
			description = "Replaces the text within an Input box element.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			menuName = AssignString(parameters, menuNameParameterID, menuName);
			elementName = AssignString(parameters, elementNameParameterID, elementName);
			newLabel = AssignString(parameters, newLabelParameterID, newLabel);
			varID = AssignVariableID(parameters, varParameterID, varID);
		}

		public override float Run()
		{
			if (!string.IsNullOrEmpty(menuName) && !string.IsNullOrEmpty(elementName))
			{
				MenuElement elementWithName = PlayerMenus.GetElementWithName(menuName, elementName);
				if (elementWithName is MenuInput)
				{
					MenuInput menuInput = (MenuInput)elementWithName;
					if (setMenuInputBoxSource == SetMenuInputBoxSource.EnteredHere)
					{
						menuInput.SetLabel(newLabel);
					}
					else if (setMenuInputBoxSource == SetMenuInputBoxSource.FromGlobalVariable)
					{
						menuInput.SetLabel(GlobalVariables.GetStringValue(varID));
					}
				}
				else
				{
					LogWarning("Cannot find Element '" + elementName + "' within Menu '" + menuName + "'");
				}
			}
			return 0f;
		}

		public static ActionMenuSetInputBox CreateNew_SetDirectly(string menuName, string inputBoxElementName, string newText)
		{
			ActionMenuSetInputBox actionMenuSetInputBox = ScriptableObject.CreateInstance<ActionMenuSetInputBox>();
			actionMenuSetInputBox.menuName = menuName;
			actionMenuSetInputBox.elementName = inputBoxElementName;
			actionMenuSetInputBox.newLabel = newText;
			return actionMenuSetInputBox;
		}

		public static ActionMenuSetInputBox CreateNew_SetFromVariable(string menuName, string inputBoxElementName, int globalStringVariableID)
		{
			ActionMenuSetInputBox actionMenuSetInputBox = ScriptableObject.CreateInstance<ActionMenuSetInputBox>();
			actionMenuSetInputBox.menuName = menuName;
			actionMenuSetInputBox.elementName = inputBoxElementName;
			actionMenuSetInputBox.varID = globalStringVariableID;
			return actionMenuSetInputBox;
		}
	}
}
