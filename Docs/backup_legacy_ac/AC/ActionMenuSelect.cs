using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionMenuSelect : Action
	{
		public string menuName;

		public int menuNameParameterID = -1;

		public string elementName;

		public int elementNameParameterID = -1;

		public int slotIndex;

		public int slotIndexParameterID = -1;

		public bool selectFirstVisible;

		public ActionMenuSelect()
		{
			isDisplayed = true;
			category = ActionCategory.Menu;
			title = "Select element";
			description = "Selects an element within an enabled menu.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			menuName = AssignString(parameters, menuNameParameterID, menuName);
			elementName = AssignString(parameters, elementNameParameterID, elementName);
			slotIndex = AssignInteger(parameters, slotIndexParameterID, slotIndex);
		}

		public override float Run()
		{
			if (!string.IsNullOrEmpty(menuName))
			{
				Menu menuWithName = PlayerMenus.GetMenuWithName(menuName);
				if (menuWithName != null)
				{
					if (selectFirstVisible)
					{
						GameObject objectToSelect = menuWithName.GetObjectToSelect();
						if (objectToSelect != null)
						{
							KickStarter.playerMenus.SelectUIElement(objectToSelect);
						}
					}
					else if (!string.IsNullOrEmpty(elementName))
					{
						MenuElement elementWithName = PlayerMenus.GetElementWithName(menuName, elementName);
						if (elementWithName != null)
						{
							menuWithName.Select(elementName, slotIndex);
						}
					}
				}
			}
			return 0f;
		}

		public static ActionMenuSelect CreateNew(string menuName, string elementName = "", int slotIndex = 0)
		{
			ActionMenuSelect actionMenuSelect = ScriptableObject.CreateInstance<ActionMenuSelect>();
			actionMenuSelect.menuName = menuName;
			actionMenuSelect.elementName = elementName;
			actionMenuSelect.selectFirstVisible = string.IsNullOrEmpty(elementName);
			actionMenuSelect.slotIndex = slotIndex;
			return actionMenuSelect;
		}
	}
}
