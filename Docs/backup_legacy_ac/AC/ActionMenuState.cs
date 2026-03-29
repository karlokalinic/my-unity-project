using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionMenuState : Action, ITranslatable
	{
		public enum MenuChangeType
		{
			TurnOnMenu = 0,
			TurnOffMenu = 1,
			HideMenuElement = 2,
			ShowMenuElement = 3,
			LockMenu = 4,
			UnlockMenu = 5,
			AddJournalPage = 6,
			RemoveJournalPage = 7
		}

		public enum RemoveJournalPageMethod
		{
			RemoveSinglePage = 0,
			RemoveAllPages = 1
		}

		public MenuChangeType changeType;

		[SerializeField]
		protected RemoveJournalPageMethod removeJournalPageMethod;

		public string menuToChange = string.Empty;

		public int menuToChangeParameterID = -1;

		public string elementToChange = string.Empty;

		public int elementToChangeParameterID = -1;

		public string journalText = string.Empty;

		public bool onlyAddNewJournal;

		public bool doFade;

		public int lineID = -1;

		public int journalPageIndex = -1;

		public int journalPageIndexParameterID = -1;

		protected LocalVariables localVariables;

		protected string runtimeMenuToChange;

		protected string runtimeElementToChange;

		public ActionMenuState()
		{
			isDisplayed = true;
			lineID = -1;
			category = ActionCategory.Menu;
			title = "Change state";
			description = "Provides various options to show and hide both menus and menu elements.";
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
			menuToChange = AssignString(parameters, menuToChangeParameterID, menuToChange);
			elementToChange = AssignString(parameters, elementToChangeParameterID, elementToChange);
			journalPageIndex = AssignInteger(parameters, journalPageIndexParameterID, journalPageIndex);
			runtimeMenuToChange = AdvGame.ConvertTokens(menuToChange, Options.GetLanguage(), localVariables, parameters);
			runtimeElementToChange = AdvGame.ConvertTokens(elementToChange, Options.GetLanguage(), localVariables, parameters);
		}

		public override float Run()
		{
			if (!isRunning)
			{
				Menu menuWithName = PlayerMenus.GetMenuWithName(runtimeMenuToChange);
				if (menuWithName == null)
				{
					if (!string.IsNullOrEmpty(runtimeMenuToChange))
					{
						ACDebug.LogWarning("Could not find menu of name '" + runtimeMenuToChange + "'", this);
					}
					return 0f;
				}
				isRunning = true;
				switch (changeType)
				{
				case MenuChangeType.TurnOnMenu:
					if (menuWithName.IsManualControlled())
					{
						if (!menuWithName.TurnOn(doFade))
						{
							isRunning = false;
							return 0f;
						}
						if (doFade && willWait)
						{
							return menuWithName.fadeSpeed;
						}
					}
					else
					{
						LogWarning("Can only turn on Menus with an Appear Type of Manual, OnInputKey, OnContainer or OnViewDocument - did you mean 'Unlock Menu'?");
					}
					break;
				case MenuChangeType.TurnOffMenu:
					if (menuWithName.IsManualControlled() || menuWithName.appearType == AppearType.OnInteraction)
					{
						if (!menuWithName.TurnOff(doFade))
						{
							isRunning = false;
							return 0f;
						}
						if (doFade && willWait)
						{
							return menuWithName.fadeSpeed;
						}
					}
					else
					{
						LogWarning("Can only turn off Menus with an Appear Type of Manual, OnInputKey, OnContainer or OnViewDocument - did you mean 'Lock Menu'?");
					}
					break;
				case MenuChangeType.LockMenu:
					if (doFade)
					{
						menuWithName.TurnOff();
					}
					else
					{
						menuWithName.ForceOff();
					}
					menuWithName.isLocked = true;
					if (doFade && willWait)
					{
						return menuWithName.fadeSpeed;
					}
					break;
				default:
					RunInstant(menuWithName);
					break;
				}
				return 0f;
			}
			isRunning = false;
			return 0f;
		}

		public override void Skip()
		{
			Menu menuWithName = PlayerMenus.GetMenuWithName(runtimeMenuToChange);
			if (menuWithName == null)
			{
				if (!string.IsNullOrEmpty(runtimeMenuToChange))
				{
					ACDebug.LogWarning("Could not find menu of name '" + runtimeMenuToChange + "'");
				}
				return;
			}
			switch (changeType)
			{
			case MenuChangeType.TurnOnMenu:
				if (menuWithName.IsManualControlled())
				{
					menuWithName.TurnOn(false);
				}
				break;
			case MenuChangeType.TurnOffMenu:
				if (menuWithName.IsManualControlled() || menuWithName.appearType == AppearType.OnInteraction)
				{
					menuWithName.ForceOff();
				}
				break;
			case MenuChangeType.LockMenu:
				menuWithName.isLocked = true;
				menuWithName.ForceOff();
				break;
			default:
				RunInstant(menuWithName);
				break;
			}
		}

		protected void RunInstant(Menu _menu)
		{
			if (changeType == MenuChangeType.HideMenuElement || changeType == MenuChangeType.ShowMenuElement)
			{
				MenuElement elementWithName = PlayerMenus.GetElementWithName(runtimeMenuToChange, runtimeElementToChange);
				if (elementWithName != null)
				{
					if (changeType == MenuChangeType.HideMenuElement)
					{
						elementWithName.IsVisible = false;
						KickStarter.playerMenus.DeselectInputBox(elementWithName);
					}
					else
					{
						elementWithName.IsVisible = true;
					}
					_menu.ResetVisibleElements();
					_menu.Recalculate();
					KickStarter.playerMenus.FindFirstSelectedElement();
				}
				else
				{
					LogWarning("Could not find element of name '" + elementToChange + "' on menu '" + menuToChange + "'");
				}
			}
			else if (changeType == MenuChangeType.UnlockMenu)
			{
				_menu.isLocked = false;
			}
			else if (changeType == MenuChangeType.AddJournalPage)
			{
				MenuElement elementWithName2 = PlayerMenus.GetElementWithName(runtimeMenuToChange, runtimeElementToChange);
				if (elementWithName2 != null)
				{
					if (!string.IsNullOrEmpty(journalText))
					{
						if (elementWithName2 is MenuJournal)
						{
							MenuJournal menuJournal = (MenuJournal)elementWithName2;
							JournalPage newPage = new JournalPage(lineID, journalText);
							menuJournal.AddPage(newPage, onlyAddNewJournal, journalPageIndex);
							if (lineID == -1)
							{
								LogWarning("The new Journal page has no ID number, and will not be included in save game files - this can be corrected by clicking 'Gather text' in the Speech Manager");
							}
						}
						else
						{
							ACDebug.LogWarning(elementWithName2.title + " is not a journal!");
						}
					}
					else
					{
						ACDebug.LogWarning("No journal text to add!");
					}
				}
				else
				{
					LogWarning("Could not find menu element of name '" + elementToChange + "' inside '" + menuToChange + "'");
				}
				_menu.Recalculate();
			}
			else
			{
				if (changeType != MenuChangeType.RemoveJournalPage)
				{
					return;
				}
				MenuElement elementWithName3 = PlayerMenus.GetElementWithName(runtimeMenuToChange, runtimeElementToChange);
				if (elementWithName3 != null)
				{
					if (elementWithName3 is MenuJournal)
					{
						MenuJournal menuJournal2 = (MenuJournal)elementWithName3;
						if (removeJournalPageMethod == RemoveJournalPageMethod.RemoveAllPages)
						{
							menuJournal2.RemoveAllPages();
						}
						else if (removeJournalPageMethod == RemoveJournalPageMethod.RemoveSinglePage)
						{
							menuJournal2.RemovePage(journalPageIndex);
						}
					}
					else
					{
						LogWarning(elementWithName3.title + " is not a journal!");
					}
				}
				else
				{
					LogWarning("Could not find menu element of name '" + elementToChange + "' inside '" + menuToChange + "'");
				}
				_menu.Recalculate();
			}
		}

		public string GetTranslatableString(int index)
		{
			return journalText;
		}

		public int GetTranslationID(int index)
		{
			return lineID;
		}

		public static ActionMenuState CreateNew_TurnOnMenu(string menuToTurnOn, bool unlockMenu = true, bool doTransition = true, bool waitUntilFinish = false)
		{
			ActionMenuState actionMenuState = ScriptableObject.CreateInstance<ActionMenuState>();
			actionMenuState.changeType = (unlockMenu ? MenuChangeType.UnlockMenu : MenuChangeType.TurnOnMenu);
			actionMenuState.menuToChange = menuToTurnOn;
			actionMenuState.doFade = doTransition;
			actionMenuState.willWait = waitUntilFinish;
			return actionMenuState;
		}

		public static ActionMenuState CreateNew_TurnOffMenu(string menuToTurnOff, bool lockMenu = false, bool doTransition = true, bool waitUntilFinish = false)
		{
			ActionMenuState actionMenuState = ScriptableObject.CreateInstance<ActionMenuState>();
			actionMenuState.changeType = ((!lockMenu) ? MenuChangeType.TurnOffMenu : MenuChangeType.LockMenu);
			actionMenuState.menuToChange = menuToTurnOff;
			actionMenuState.doFade = doTransition;
			actionMenuState.willWait = waitUntilFinish;
			return actionMenuState;
		}

		public static ActionMenuState CreateNew_SetElementVisibility(string menuName, string elementToAffect, bool makeVisible)
		{
			ActionMenuState actionMenuState = ScriptableObject.CreateInstance<ActionMenuState>();
			actionMenuState.changeType = ((!makeVisible) ? MenuChangeType.HideMenuElement : MenuChangeType.ShowMenuElement);
			actionMenuState.menuToChange = menuName;
			actionMenuState.elementToChange = elementToAffect;
			return actionMenuState;
		}

		public static ActionMenuState CreateNew_AddJournalPage(string menuName, string journalElementName, string newPageText, int newPageTranslationID = -1, int pageIndexToInsertInto = -1, bool avoidDuplicates = true)
		{
			ActionMenuState actionMenuState = ScriptableObject.CreateInstance<ActionMenuState>();
			actionMenuState.changeType = MenuChangeType.AddJournalPage;
			actionMenuState.menuToChange = menuName;
			actionMenuState.elementToChange = journalElementName;
			actionMenuState.journalText = newPageText;
			actionMenuState.lineID = newPageTranslationID;
			actionMenuState.journalPageIndex = pageIndexToInsertInto;
			actionMenuState.onlyAddNewJournal = avoidDuplicates;
			return actionMenuState;
		}

		public static ActionMenuState CreateNew_RemoveJournalPage(string menuName, string journalElementName, RemoveJournalPageMethod removeJournalPageMethod = RemoveJournalPageMethod.RemoveSinglePage, int pageIndexToRemove = -1)
		{
			ActionMenuState actionMenuState = ScriptableObject.CreateInstance<ActionMenuState>();
			actionMenuState.changeType = MenuChangeType.RemoveJournalPage;
			actionMenuState.menuToChange = menuName;
			actionMenuState.elementToChange = journalElementName;
			actionMenuState.removeJournalPageMethod = removeJournalPageMethod;
			actionMenuState.journalPageIndex = pageIndexToRemove;
			return actionMenuState;
		}
	}
}
