using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class Document : ITranslatable
	{
		public int ID;

		public string title;

		public int titleLineID = -1;

		public bool rememberLastOpenPage;

		public bool carryOnStart;

		public Texture2D texture;

		public List<JournalPage> pages = new List<JournalPage>();

		public int binID;

		public string Title
		{
			get
			{
				if (string.IsNullOrEmpty(title))
				{
					title = "(Untitled)";
				}
				return title;
			}
		}

		public Document(int[] idArray)
		{
			title = string.Empty;
			titleLineID = -1;
			rememberLastOpenPage = false;
			texture = null;
			pages = new List<JournalPage>();
			carryOnStart = false;
			binID = 0;
			ID = 0;
			foreach (int num in idArray)
			{
				if (ID == num)
				{
					ID++;
				}
			}
		}

		public Document(int _ID)
		{
			title = string.Empty;
			titleLineID = -1;
			rememberLastOpenPage = false;
			texture = null;
			pages = new List<JournalPage>();
			carryOnStart = false;
			binID = 0;
			ID = _ID;
		}

		public string GetPageText(int pageIndex, int languageNumber = 0)
		{
			if (pages != null && pageIndex < pages.Count && pageIndex > 0)
			{
				JournalPage journalPage = pages[pageIndex];
				return KickStarter.runtimeLanguages.GetTranslation(journalPage.text, journalPage.lineID, languageNumber);
			}
			return string.Empty;
		}

		public string GetTitleText(int languageNumber = 0)
		{
			return KickStarter.runtimeLanguages.GetTranslation(title, titleLineID, languageNumber);
		}

		public string GetTranslatableString(int index)
		{
			if (index == 0)
			{
				return Title;
			}
			return pages[index - 1].text;
		}

		public int GetTranslationID(int index)
		{
			if (index == 0)
			{
				return titleLineID;
			}
			return pages[index - 1].lineID;
		}
	}
}
