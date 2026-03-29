using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionMenuCheck : ActionCheck
	{
		public enum MenuCheckType
		{
			MenuIsVisible = 0,
			MenuIsLocked = 1,
			ElementIsVisible = 2
		}

		public MenuCheckType checkType;

		public string menuToCheck = string.Empty;

		public int menuToCheckParameterID = -1;

		public string elementToCheck = string.Empty;

		public int elementToCheckParameterID = -1;

		protected LocalVariables localVariables;

		protected string _menuToCheck;

		protected string _elementToCheck;

		public ActionMenuCheck()
		{
			isDisplayed = true;
			category = ActionCategory.Menu;
			title = "Check state";
			description = "Queries the visibility of menu elements, and the enabled or locked state of menus.";
		}

		public override void AssignParentList(ActionList actionList)
		{
			if (actionList != null)
			{
				localVariables = UnityVersionHandler.GetLocalVariablesOfGameObject(actionList.gameObject);
			}
			if (localVariables == null)
			{
				localVariables = KickStarter.localVariables;
			}
			base.AssignParentList(actionList);
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			menuToCheck = AssignString(parameters, menuToCheckParameterID, menuToCheck);
			elementToCheck = AssignString(parameters, elementToCheckParameterID, elementToCheck);
			_menuToCheck = AdvGame.ConvertTokens(menuToCheck, Options.GetLanguage(), localVariables, parameters);
			_elementToCheck = AdvGame.ConvertTokens(elementToCheck, Options.GetLanguage(), localVariables, parameters);
		}

		public override bool CheckCondition()
		{
			Menu menuWithName = PlayerMenus.GetMenuWithName(_menuToCheck);
			if (menuWithName != null)
			{
				if (checkType == MenuCheckType.MenuIsVisible)
				{
					return menuWithName.IsVisible();
				}
				if (checkType == MenuCheckType.MenuIsLocked)
				{
					return menuWithName.isLocked;
				}
				if (checkType == MenuCheckType.ElementIsVisible)
				{
					MenuElement elementWithName = PlayerMenus.GetElementWithName(_menuToCheck, _elementToCheck);
					if (elementWithName != null)
					{
						return elementWithName.IsVisible;
					}
				}
			}
			return false;
		}

		public static ActionMenuCheck CreateNew_MenuIsLocked(string menuName)
		{
			ActionMenuCheck actionMenuCheck = ScriptableObject.CreateInstance<ActionMenuCheck>();
			actionMenuCheck.checkType = MenuCheckType.MenuIsLocked;
			actionMenuCheck.menuToCheck = menuName;
			return actionMenuCheck;
		}

		public static ActionMenuCheck CreateNew_MenuIsOn(string menuName)
		{
			ActionMenuCheck actionMenuCheck = ScriptableObject.CreateInstance<ActionMenuCheck>();
			actionMenuCheck.checkType = MenuCheckType.MenuIsVisible;
			actionMenuCheck.menuToCheck = menuName;
			return actionMenuCheck;
		}

		public static ActionMenuCheck CreateNew_ElementIsVisible(string menuName, string elementName)
		{
			ActionMenuCheck actionMenuCheck = ScriptableObject.CreateInstance<ActionMenuCheck>();
			actionMenuCheck.checkType = MenuCheckType.ElementIsVisible;
			actionMenuCheck.menuToCheck = menuName;
			actionMenuCheck.elementToCheck = elementName;
			return actionMenuCheck;
		}
	}
}
