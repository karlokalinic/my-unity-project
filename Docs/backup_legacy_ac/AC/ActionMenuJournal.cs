using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionMenuJournal : Action
	{
		public string menuToChange = string.Empty;

		public int menuToChangeParameterID = -1;

		public string elementToChange = string.Empty;

		public int elementToChangeParameterID = -1;

		public SetJournalPage setJournalPage;

		public int pageNumber;

		public int pageNumberParameterID = -1;

		public ActionMenuJournal()
		{
			isDisplayed = true;
			category = ActionCategory.Menu;
			title = "Set Journal page";
			description = "Set which page of a Journal is currently open.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			menuToChange = AssignString(parameters, menuToChangeParameterID, menuToChange);
			elementToChange = AssignString(parameters, elementToChangeParameterID, elementToChange);
			pageNumber = AssignInteger(parameters, pageNumberParameterID, pageNumber);
		}

		public override float Run()
		{
			MenuElement elementWithName = PlayerMenus.GetElementWithName(menuToChange, elementToChange);
			if (elementWithName != null)
			{
				if (elementWithName is MenuJournal)
				{
					MenuJournal menuJournal = (MenuJournal)elementWithName;
					if (menuJournal.pages.Count > 0)
					{
						if (setJournalPage == SetJournalPage.FirstPage)
						{
							menuJournal.showPage = 1;
						}
						else if (setJournalPage == SetJournalPage.LastPage)
						{
							menuJournal.showPage = menuJournal.pages.Count;
						}
						else if (setJournalPage == SetJournalPage.SetHere)
						{
							menuJournal.showPage = Mathf.Min(menuJournal.pages.Count, pageNumber);
						}
					}
				}
				else
				{
					LogWarning(elementWithName.title + " is not a journal!");
				}
			}
			else
			{
				LogWarning("Could not find menu element of name '" + elementToChange + "' inside '" + menuToChange + "'");
			}
			return 0f;
		}

		public static ActionMenuJournal CreateNew(string menuName, string journalElementName, int pageIndexNumber)
		{
			ActionMenuJournal actionMenuJournal = ScriptableObject.CreateInstance<ActionMenuJournal>();
			actionMenuJournal.menuToChange = menuName;
			actionMenuJournal.elementToChange = journalElementName;
			actionMenuJournal.pageNumber = pageIndexNumber;
			switch (pageIndexNumber)
			{
			case 0:
				actionMenuJournal.setJournalPage = SetJournalPage.FirstPage;
				break;
			case -1:
				actionMenuJournal.setJournalPage = SetJournalPage.LastPage;
				break;
			default:
				actionMenuJournal.setJournalPage = SetJournalPage.SetHere;
				break;
			}
			return actionMenuJournal;
		}
	}
}
