using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionMenuSlotCheck : ActionCheck
	{
		public string menuToCheck = string.Empty;

		public int menuToCheckParameterID = -1;

		public string elementToCheck = string.Empty;

		public int elementToCheckParameterID = -1;

		public int numToCheck;

		public int numToCheckParameterID = -1;

		public IntCondition intCondition;

		public ActionMenuSlotCheck()
		{
			isDisplayed = true;
			category = ActionCategory.Menu;
			title = "Check num slots";
			description = "Queries the number of slots on a given menu element.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			menuToCheck = AssignString(parameters, menuToCheckParameterID, menuToCheck);
			elementToCheck = AssignString(parameters, elementToCheckParameterID, elementToCheck);
			numToCheck = AssignInteger(parameters, numToCheckParameterID, numToCheck);
		}

		public override bool CheckCondition()
		{
			MenuElement elementWithName = PlayerMenus.GetElementWithName(menuToCheck, elementToCheck);
			if (elementWithName != null)
			{
				int numSlots = elementWithName.GetNumSlots();
				if (intCondition == IntCondition.EqualTo)
				{
					if (numToCheck == numSlots)
					{
						return true;
					}
				}
				else if (intCondition == IntCondition.LessThan)
				{
					if (numToCheck > numSlots)
					{
						return true;
					}
				}
				else if (intCondition == IntCondition.MoreThan)
				{
					if (numToCheck < numSlots)
					{
						return true;
					}
				}
				else if (intCondition == IntCondition.NotEqualTo && numToCheck != numSlots)
				{
					return true;
				}
			}
			return false;
		}

		public static ActionMenuSlotCheck CreateNew(string menuName, string elementName, int numSlots, IntCondition condition = IntCondition.EqualTo)
		{
			ActionMenuSlotCheck actionMenuSlotCheck = ScriptableObject.CreateInstance<ActionMenuSlotCheck>();
			actionMenuSlotCheck.menuToCheck = menuName;
			actionMenuSlotCheck.elementToCheck = elementName;
			actionMenuSlotCheck.intCondition = condition;
			actionMenuSlotCheck.numToCheck = numSlots;
			return actionMenuSlotCheck;
		}
	}
}
