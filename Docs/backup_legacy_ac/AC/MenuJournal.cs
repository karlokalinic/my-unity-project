using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AC
{
	public class MenuJournal : MenuElement, ITranslatable
	{
		public Text uiText;

		public List<JournalPage> pages = new List<JournalPage>();

		public int numPages = 1;

		public int showPage = 1;

		public bool startFromPage;

		public TextAnchor anchor;

		public TextEffects textEffects;

		public float outlineSize = 2f;

		public ActionListAsset actionListOnAddPage;

		public JournalType journalType;

		public int pageOffset;

		public string otherJournalTitle;

		private string fullText;

		private MenuJournal otherJournal;

		private Document ownDocument;

		public override void Declare()
		{
			uiText = null;
			pages = new List<JournalPage>();
			pages.Add(new JournalPage());
			numPages = 1;
			showPage = 1;
			isVisible = true;
			isClickable = false;
			numSlots = 1;
			anchor = TextAnchor.MiddleCenter;
			SetSize(new Vector2(10f, 5f));
			textEffects = TextEffects.None;
			outlineSize = 2f;
			fullText = string.Empty;
			actionListOnAddPage = null;
			journalType = JournalType.NewJournal;
			pageOffset = 0;
			otherJournalTitle = string.Empty;
			base.Declare();
		}

		public override MenuElement DuplicateSelf(bool fromEditor, bool ignoreUnityUI)
		{
			MenuJournal menuJournal = ScriptableObject.CreateInstance<MenuJournal>();
			menuJournal.Declare();
			menuJournal.CopyJournal(this, fromEditor, ignoreUnityUI);
			return menuJournal;
		}

		private void CopyJournal(MenuJournal _element, bool fromEditor, bool ignoreUnityUI)
		{
			if (ignoreUnityUI)
			{
				uiText = null;
			}
			else
			{
				uiText = _element.uiText;
			}
			pages = new List<JournalPage>();
			foreach (JournalPage page in _element.pages)
			{
				JournalPage journalPage = new JournalPage(page);
				if (fromEditor)
				{
					journalPage.lineID = -1;
				}
				pages.Add(journalPage);
			}
			numPages = _element.numPages;
			startFromPage = _element.startFromPage;
			if (startFromPage)
			{
				showPage = _element.showPage;
			}
			else
			{
				showPage = 1;
			}
			anchor = _element.anchor;
			textEffects = _element.textEffects;
			outlineSize = _element.outlineSize;
			fullText = string.Empty;
			actionListOnAddPage = _element.actionListOnAddPage;
			journalType = _element.journalType;
			pageOffset = _element.pageOffset;
			otherJournalTitle = _element.otherJournalTitle;
			base.Copy(_element);
		}

		public override void Initialise(Menu _menu)
		{
			if (journalType == JournalType.DisplayExistingJournal)
			{
				MenuElement elementWithName = _menu.GetElementWithName(otherJournalTitle);
				if (elementWithName != null && elementWithName is MenuJournal)
				{
					otherJournal = (MenuJournal)elementWithName;
				}
			}
			base.Initialise(_menu);
		}

		public override void LoadUnityUI(Menu _menu, Canvas canvas)
		{
			uiText = LinkUIElement<Text>(canvas);
		}

		public override RectTransform GetRectTransform(int _slot)
		{
			if ((bool)uiText)
			{
				return uiText.rectTransform;
			}
			return null;
		}

		public int GetCurrentPageNumber()
		{
			return showPage;
		}

		public int GetTotalNumberOfPages()
		{
			if (pages != null)
			{
				return pages.Count;
			}
			return 0;
		}

		public override void OnMenuTurnOn(Menu menu)
		{
			base.OnMenuTurnOn(menu);
			if (journalType == JournalType.DisplayActiveDocument && KickStarter.runtimeDocuments.ActiveDocument != null)
			{
				ownDocument = KickStarter.runtimeDocuments.ActiveDocument;
				pages = ownDocument.pages;
				showPage = KickStarter.runtimeDocuments.GetLastOpenPage(ownDocument);
			}
		}

		public override void PreDisplay(int _slot, int languageNumber, bool isActive)
		{
			if (Application.isPlaying && journalType == JournalType.DisplayExistingJournal)
			{
				if (otherJournal != null)
				{
					int num = otherJournal.showPage + pageOffset - 1;
					if (otherJournal.pages.Count > num)
					{
						fullText = TranslatePage(otherJournal.pages[num], languageNumber);
					}
					else
					{
						fullText = string.Empty;
					}
					fullText = AdvGame.ConvertTokens(fullText, languageNumber);
				}
			}
			else
			{
				if (Application.isPlaying && journalType == JournalType.DisplayActiveDocument && ownDocument != KickStarter.runtimeDocuments.ActiveDocument && KickStarter.runtimeDocuments.ActiveDocument != null)
				{
					ownDocument = KickStarter.runtimeDocuments.ActiveDocument;
					pages = ownDocument.pages;
					showPage = KickStarter.runtimeDocuments.GetLastOpenPage(ownDocument);
				}
				if (pages.Count == 0)
				{
					fullText = string.Empty;
				}
				else if (pages.Count >= showPage && showPage > 0)
				{
					fullText = TranslatePage(pages[showPage - 1], languageNumber);
					fullText = AdvGame.ConvertTokens(fullText, languageNumber);
				}
			}
			if (uiText != null)
			{
				UpdateUIElement(uiText);
				uiText.text = fullText;
			}
		}

		public override void Display(GUIStyle _style, int _slot, float zoom, bool isActive)
		{
			base.Display(_style, _slot, zoom, isActive);
			_style.wordWrap = true;
			_style.alignment = anchor;
			if (zoom < 1f)
			{
				_style.fontSize = (int)((float)_style.fontSize * zoom);
			}
			if (pages.Count >= showPage)
			{
				if (textEffects != TextEffects.None)
				{
					AdvGame.DrawTextEffect(ZoomRect(relativeRect, zoom), fullText, _style, Color.black, _style.normal.textColor, outlineSize, textEffects);
				}
				else
				{
					GUI.Label(ZoomRect(relativeRect, zoom), fullText, _style);
				}
			}
		}

		public override string GetLabel(int slot, int languageNumber)
		{
			if (journalType == JournalType.DisplayExistingJournal)
			{
				if (otherJournal != null)
				{
					int num = otherJournal.showPage + pageOffset - 1;
					if (num >= 0 && otherJournal.pages.Count > num)
					{
						return TranslatePage(otherJournal.pages[num], languageNumber);
					}
				}
				return string.Empty;
			}
			int num2 = showPage - 1;
			if (num2 >= 0 && pages.Count > num2)
			{
				return TranslatePage(pages[num2], languageNumber);
			}
			return string.Empty;
		}

		public void Shift(AC_ShiftInventory shiftType, bool doLoop, int amount)
		{
			if (journalType == JournalType.DisplayExistingJournal)
			{
				ACDebug.LogWarning("The journal '" + title + "' cannot be shifted - instead its linked journal (" + otherJournalTitle + ") must be shifted instead.");
				return;
			}
			switch (shiftType)
			{
			case AC_ShiftInventory.ShiftNext:
				showPage += amount;
				break;
			case AC_ShiftInventory.ShiftPrevious:
				showPage -= amount;
				break;
			}
			if (showPage < 1)
			{
				if (doLoop)
				{
					showPage = pages.Count;
				}
				else
				{
					showPage = 1;
				}
			}
			else if (showPage > pages.Count)
			{
				if (doLoop)
				{
					showPage = 1;
				}
				else
				{
					showPage = pages.Count;
				}
			}
			if (journalType == JournalType.DisplayActiveDocument && ownDocument != null)
			{
				KickStarter.runtimeDocuments.SetLastOpenPage(ownDocument, showPage);
			}
			KickStarter.eventManager.Call_OnMenuElementShift(this, shiftType);
		}

		private string TranslatePage(JournalPage page, int languageNumber)
		{
			if (Application.isPlaying)
			{
				return KickStarter.runtimeLanguages.GetTranslation(page.text, page.lineID, languageNumber);
			}
			return page.text;
		}

		protected override void AutoSize()
		{
			string text = string.Empty;
			if (Application.isPlaying && journalType == JournalType.DisplayExistingJournal)
			{
				if (otherJournal != null)
				{
					int num = otherJournal.showPage + pageOffset - 1;
					if (num >= 0 && otherJournal.pages.Count > num)
					{
						text = otherJournal.pages[num].text;
					}
				}
			}
			else
			{
				int num2 = showPage - 1;
				if (num2 >= 0 && pages.Count > num2)
				{
					text = pages[num2].text;
				}
			}
			if (text == string.Empty && backgroundTexture != null)
			{
				GUIContent content = new GUIContent(backgroundTexture);
				AutoSize(content);
			}
			else
			{
				GUIContent content2 = new GUIContent(text);
				AutoSize(content2);
			}
		}

		public override bool CanBeShifted(AC_ShiftInventory shiftType)
		{
			if (journalType == JournalType.DisplayExistingJournal || pages.Count == 0)
			{
				return false;
			}
			if (shiftType == AC_ShiftInventory.ShiftPrevious)
			{
				if (showPage == 1)
				{
					return false;
				}
			}
			else if (pages.Count <= showPage)
			{
				return false;
			}
			return true;
		}

		public void AddPage(JournalPage newPage, bool onlyAddNew, int index = -1)
		{
			if (journalType == JournalType.DisplayExistingJournal)
			{
				ACDebug.LogWarning("The journal '" + title + "' cannot be added to - instead its linked journal (" + otherJournalTitle + ") must be modified instead.");
				return;
			}
			if (journalType == JournalType.DisplayActiveDocument)
			{
				ACDebug.LogWarning("The journal '" + title + "' cannot be added to.");
				return;
			}
			if (onlyAddNew && newPage.lineID >= 0 && pages != null && pages.Count > 0)
			{
				foreach (JournalPage page in pages)
				{
					if (page.lineID == newPage.lineID)
					{
						return;
					}
				}
			}
			if (index == -1)
			{
				index = pages.Count;
			}
			if (index < 0 || index >= pages.Count)
			{
				pages.Add(newPage);
				index = pages.IndexOf(newPage);
			}
			else
			{
				pages.Insert(index, newPage);
			}
			if (showPage > index || showPage == 0)
			{
				showPage++;
			}
			KickStarter.eventManager.Call_OnModifyJournalPage(this, newPage, index, true);
			AdvGame.RunActionListAsset(actionListOnAddPage);
		}

		public void RemovePage(int index = -1)
		{
			if (journalType == JournalType.DisplayExistingJournal)
			{
				ACDebug.LogWarning("The journal '" + title + "' cannot be modified - instead its linked journal (" + otherJournalTitle + ") must be modified instead.");
			}
			else if (pages.Count != 0)
			{
				if (index == -1)
				{
					index = pages.Count - 1;
				}
				if (index < 0)
				{
					index = pages.Count - 1;
				}
				else if (index >= pages.Count)
				{
					ACDebug.LogWarning("The journal '" + title + "' cannot have it's " + index + " page removed, as it only has " + pages.Count + " pages!");
					return;
				}
				if (pages[index].lineID == -1)
				{
					ACDebug.LogWarning("The removed Journal page has no ID number, and the change will not be included in save game files - this can be corrected by clicking 'Gather text' in the Speech Manager.");
				}
				JournalPage page = pages[index];
				pages.RemoveAt(index);
				if (showPage > index)
				{
					showPage--;
				}
				KickStarter.eventManager.Call_OnModifyJournalPage(this, page, index, false);
			}
		}

		public void RemoveAllPages()
		{
			if (journalType == JournalType.DisplayExistingJournal)
			{
				ACDebug.LogWarning("The journal '" + title + "' cannot be modified - instead its linked journal (" + otherJournalTitle + ") must be modified instead.");
			}
			else if (pages.Count != 0)
			{
				pages.Clear();
				showPage = 0;
			}
		}

		public string GetTranslatableString(int index)
		{
			return pages[index].text;
		}

		public int GetTranslationID(int index)
		{
			return pages[index].lineID;
		}
	}
}
