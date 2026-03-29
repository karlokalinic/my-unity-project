using System;
using UnityEngine;

namespace AC
{
	public class MenuSavesList : MenuElement, ITranslatable
	{
		public UISlot[] uiSlots;

		public TextEffects textEffects;

		public float outlineSize = 2f;

		public TextAnchor anchor;

		public AC_SaveListType saveListType;

		public int maxSlots = 5;

		public ActionListAsset actionListOnSave;

		public SaveDisplayType displayType;

		public Texture2D blankSlotTexture;

		public string importProductName;

		public string importSaveFilename;

		public bool checkImportBool;

		public int checkImportVar;

		public bool allowEmptySlots;

		public bool fixedOption;

		public int optionToShow;

		public int parameterID = -1;

		public string newSaveText = "New save";

		public string emptySlotText = string.Empty;

		public int emptySlotTextLineID = -1;

		public bool showNewSaveOption = true;

		public bool hideIfNotValid;

		public bool autoHandle = true;

		public UIHideStyle uiHideStyle;

		public LinkUIGraphic linkUIGraphic;

		private string[] labels;

		private bool newSaveSlot;

		private int eventSlot;

		public override void Declare()
		{
			uiSlots = null;
			newSaveText = "New save";
			emptySlotText = string.Empty;
			emptySlotTextLineID = -1;
			isVisible = true;
			isClickable = true;
			numSlots = 1;
			maxSlots = 5;
			SetSize(new Vector2(20f, 5f));
			anchor = TextAnchor.MiddleCenter;
			saveListType = AC_SaveListType.Save;
			actionListOnSave = null;
			newSaveSlot = false;
			textEffects = TextEffects.None;
			outlineSize = 2f;
			displayType = SaveDisplayType.LabelOnly;
			blankSlotTexture = null;
			allowEmptySlots = false;
			fixedOption = false;
			optionToShow = 1;
			hideIfNotValid = false;
			importProductName = string.Empty;
			importSaveFilename = string.Empty;
			checkImportBool = false;
			checkImportVar = 0;
			showNewSaveOption = true;
			autoHandle = true;
			parameterID = -1;
			uiHideStyle = UIHideStyle.DisableObject;
			linkUIGraphic = LinkUIGraphic.ImageComponent;
			base.Declare();
		}

		public override MenuElement DuplicateSelf(bool fromEditor, bool ignoreUnityUI)
		{
			MenuSavesList menuSavesList = ScriptableObject.CreateInstance<MenuSavesList>();
			menuSavesList.Declare();
			menuSavesList.CopySavesList(this, ignoreUnityUI);
			return menuSavesList;
		}

		private void CopySavesList(MenuSavesList _element, bool ignoreUnityUI)
		{
			if (ignoreUnityUI)
			{
				uiSlots = null;
			}
			else
			{
				uiSlots = _element.uiSlots;
			}
			newSaveText = _element.newSaveText;
			emptySlotText = _element.emptySlotText;
			emptySlotTextLineID = _element.emptySlotTextLineID;
			textEffects = _element.textEffects;
			outlineSize = _element.outlineSize;
			anchor = _element.anchor;
			saveListType = _element.saveListType;
			maxSlots = _element.maxSlots;
			actionListOnSave = _element.actionListOnSave;
			displayType = _element.displayType;
			blankSlotTexture = _element.blankSlotTexture;
			allowEmptySlots = _element.allowEmptySlots;
			fixedOption = _element.fixedOption;
			optionToShow = _element.optionToShow;
			hideIfNotValid = _element.hideIfNotValid;
			importProductName = _element.importProductName;
			importSaveFilename = _element.importSaveFilename;
			checkImportBool = _element.checkImportBool;
			checkImportVar = _element.checkImportVar;
			parameterID = _element.parameterID;
			showNewSaveOption = _element.showNewSaveOption;
			autoHandle = _element.autoHandle;
			uiHideStyle = _element.uiHideStyle;
			linkUIGraphic = _element.linkUIGraphic;
			base.Copy(_element);
		}

		public override void LoadUnityUI(Menu _menu, Canvas canvas)
		{
			int num = 0;
			UISlot[] array = uiSlots;
			foreach (UISlot uISlot in array)
			{
				uISlot.LinkUIElements(canvas, linkUIGraphic);
				if (uISlot != null && uISlot.uiButton != null)
				{
					int j = num;
					uISlot.uiButton.onClick.AddListener(delegate
					{
						ProcessClickUI(_menu, j, KickStarter.playerInput.GetMouseState());
					});
				}
				num++;
			}
		}

		public override GameObject GetObjectToSelect(int slotIndex = 0)
		{
			if (uiSlots != null && uiSlots.Length > slotIndex && uiSlots[slotIndex].uiButton != null && numSlots > slotIndex)
			{
				return uiSlots[slotIndex].uiButton.gameObject;
			}
			return null;
		}

		public override RectTransform GetRectTransform(int _slot)
		{
			if (uiSlots != null && uiSlots.Length > _slot)
			{
				return uiSlots[_slot].GetRectTransform();
			}
			return null;
		}

		public override void SetUIInteractableState(bool state)
		{
			SetUISlotsInteractableState(uiSlots, state);
		}

		public override string GetLabel(int _slot, int languageNumber)
		{
			if (saveListType == AC_SaveListType.Save)
			{
				if (newSaveSlot)
				{
					if (fixedOption)
					{
						if (!SaveSystem.DoesSaveExist(optionToShow))
						{
							return TranslateLabel(newSaveText, lineID, languageNumber);
						}
					}
					else if (_slot + offset == numSlots - 1)
					{
						return TranslateLabel(newSaveText, lineID, languageNumber);
					}
				}
				else if (!fixedOption && allowEmptySlots && !SaveSystem.DoesSaveExist(_slot + offset))
				{
					return TranslateLabel(newSaveText, lineID, languageNumber);
				}
			}
			else if (saveListType == AC_SaveListType.Load)
			{
				string text = SaveSystem.GetSaveSlotLabel(_slot + offset, GetOptionID(_slot), fixedOption || allowEmptySlots);
				if (string.IsNullOrEmpty(text) && (fixedOption || allowEmptySlots))
				{
					text = TranslateLabel(emptySlotText, emptySlotTextLineID, languageNumber);
				}
				return text;
			}
			return SaveSystem.GetSaveSlotLabel(_slot + offset, GetOptionID(_slot), fixedOption || allowEmptySlots);
		}

		private int GetOptionID(int _slot)
		{
			if (fixedOption)
			{
				return optionToShow;
			}
			return _slot + offset;
		}

		public override bool IsSelectedByEventSystem(int slotIndex)
		{
			if (uiSlots != null && slotIndex >= 0 && uiSlots.Length > slotIndex && uiSlots[slotIndex] != null && uiSlots[slotIndex].uiButton != null)
			{
				return KickStarter.playerMenus.IsEventSystemSelectingObject(uiSlots[slotIndex].uiButton.gameObject);
			}
			return false;
		}

		public override void HideAllUISlots()
		{
			LimitUISlotVisibility(uiSlots, 0, uiHideStyle);
		}

		public override void PreDisplay(int _slot, int languageNumber, bool isActive)
		{
			if (displayType != SaveDisplayType.ScreenshotOnly)
			{
				string empty = string.Empty;
				if (newSaveSlot && saveListType == AC_SaveListType.Save)
				{
					empty = ((!fixedOption && _slot + offset == KickStarter.saveSystem.GetNumSaves()) ? TranslateLabel(newSaveText, lineID, languageNumber) : ((!fixedOption) ? SaveSystem.GetSaveSlotLabel(_slot + offset, GetOptionID(_slot), fixedOption || allowEmptySlots) : TranslateLabel(newSaveText, lineID, languageNumber)));
				}
				else if (saveListType == AC_SaveListType.Save && !fixedOption && allowEmptySlots)
				{
					empty = ((!SaveSystem.DoesSaveExist(GetOptionID(_slot))) ? TranslateLabel(newSaveText, lineID, languageNumber) : SaveSystem.GetSaveSlotLabel(_slot + offset, GetOptionID(_slot), fixedOption || allowEmptySlots));
				}
				else if (saveListType == AC_SaveListType.Import)
				{
					empty = SaveSystem.GetImportSlotLabel(_slot + offset, GetOptionID(_slot), fixedOption || allowEmptySlots);
				}
				else
				{
					empty = SaveSystem.GetSaveSlotLabel(_slot + offset, GetOptionID(_slot), fixedOption || allowEmptySlots);
					if (string.IsNullOrEmpty(empty) && saveListType == AC_SaveListType.Load && (fixedOption || allowEmptySlots))
					{
						empty = TranslateLabel(emptySlotText, emptySlotTextLineID, languageNumber);
					}
				}
				if (!Application.isPlaying && (labels == null || labels.Length != numSlots))
				{
					labels = new string[numSlots];
				}
				labels[_slot] = empty;
			}
			if (!Application.isPlaying || uiSlots == null || uiSlots.Length <= _slot)
			{
				return;
			}
			if (saveListType == AC_SaveListType.Load && fixedOption && hideIfNotValid)
			{
				if (!SaveSystem.DoesSaveExist(optionToShow))
				{
					LimitUISlotVisibility(uiSlots, 0, uiHideStyle);
				}
				else
				{
					LimitUISlotVisibility(uiSlots, numSlots, uiHideStyle);
				}
			}
			LimitUISlotVisibility(uiSlots, numSlots, uiHideStyle);
			if (displayType != SaveDisplayType.LabelOnly)
			{
				Texture2D texture2D = null;
				texture2D = ((saveListType != AC_SaveListType.Import) ? SaveSystem.GetSaveSlotScreenshot(_slot + offset, GetOptionID(_slot), fixedOption || allowEmptySlots) : SaveSystem.GetImportSlotScreenshot(_slot + offset, GetOptionID(_slot), fixedOption || allowEmptySlots));
				if (texture2D == null)
				{
					texture2D = blankSlotTexture;
				}
				uiSlots[_slot].SetImage(texture2D);
			}
			if (displayType != SaveDisplayType.ScreenshotOnly)
			{
				uiSlots[_slot].SetText(labels[_slot]);
			}
		}

		public override void Display(GUIStyle _style, int _slot, float zoom, bool isActive)
		{
			base.Display(_style, _slot, zoom, isActive);
			_style.wordWrap = true;
			if (displayType != SaveDisplayType.LabelOnly)
			{
				Texture2D texture2D = null;
				texture2D = ((saveListType != AC_SaveListType.Import) ? SaveSystem.GetSaveSlotScreenshot(_slot + offset, GetOptionID(_slot), fixedOption || allowEmptySlots) : SaveSystem.GetImportSlotScreenshot(_slot + offset, GetOptionID(_slot), fixedOption || allowEmptySlots));
				if (texture2D == null && blankSlotTexture != null)
				{
					texture2D = blankSlotTexture;
				}
				if (texture2D != null)
				{
					GUI.DrawTexture(ZoomRect(GetSlotRectRelative(_slot), zoom), texture2D, ScaleMode.StretchToFill, true, 0f);
				}
			}
			if (displayType != SaveDisplayType.ScreenshotOnly)
			{
				_style.alignment = anchor;
				if (zoom < 1f)
				{
					_style.fontSize = (int)((float)_style.fontSize * zoom);
				}
				if (textEffects != TextEffects.None)
				{
					AdvGame.DrawTextEffect(ZoomRect(GetSlotRectRelative(_slot), zoom), labels[_slot], _style, Color.black, _style.normal.textColor, outlineSize, textEffects);
				}
				else
				{
					GUI.Label(ZoomRect(GetSlotRectRelative(_slot), zoom), labels[_slot], _style);
				}
			}
		}

		public override void ProcessClick(Menu _menu, int _slot, MouseState _mouseState)
		{
			if (KickStarter.stateHandler.gameState == GameState.Cutscene)
			{
				return;
			}
			eventSlot = _slot;
			ClearAllEvents();
			switch (saveListType)
			{
			case AC_SaveListType.Save:
				if (autoHandle)
				{
					if (PlayerMenus.IsSavingLocked(null, true))
					{
						return;
					}
					EventManager.OnFinishSaving = (EventManager.Delegate_SaveFile)Delegate.Combine(EventManager.OnFinishSaving, new EventManager.Delegate_SaveFile(OnCompleteSave));
					EventManager.OnFailSaving = (EventManager.Delegate_SaveID)Delegate.Combine(EventManager.OnFailSaving, new EventManager.Delegate_SaveID(OnFailSaveLoad));
					if (newSaveSlot && _slot == numSlots - 1)
					{
						SaveSystem.SaveNewGame(true, string.Empty);
						if (KickStarter.settingsManager.orderSavesByUpdateTime)
						{
							offset = 0;
						}
						else
						{
							Shift(AC_ShiftInventory.ShiftNext, 1);
						}
					}
					else
					{
						SaveSystem.SaveGame(_slot + offset, GetOptionID(_slot), fixedOption || allowEmptySlots, true, string.Empty);
					}
				}
				else
				{
					RunActionList(_slot);
				}
				break;
			case AC_SaveListType.Load:
				if (autoHandle)
				{
					EventManager.OnFinishLoading = (EventManager.Delegate_Generic)Delegate.Combine(EventManager.OnFinishLoading, new EventManager.Delegate_Generic(OnCompleteLoad));
					EventManager.OnFailLoading = (EventManager.Delegate_SaveID)Delegate.Combine(EventManager.OnFailLoading, new EventManager.Delegate_SaveID(OnFailSaveLoad));
					SaveSystem.LoadGame(_slot + offset, GetOptionID(_slot), fixedOption || allowEmptySlots);
				}
				else
				{
					RunActionList(_slot);
				}
				break;
			case AC_SaveListType.Import:
				EventManager.OnFinishImporting = (EventManager.Delegate_Generic)Delegate.Combine(EventManager.OnFinishImporting, new EventManager.Delegate_Generic(OnCompleteImport));
				EventManager.OnFailImporting = (EventManager.Delegate_Generic)Delegate.Combine(EventManager.OnFailImporting, new EventManager.Delegate_Generic(OnFailImport));
				SaveSystem.ImportGame(_slot + offset, GetOptionID(_slot), fixedOption || allowEmptySlots);
				break;
			}
			base.ProcessClick(_menu, _slot, _mouseState);
		}

		private void OnCompleteSave(SaveFile saveFile)
		{
			ClearAllEvents();
			if (autoHandle)
			{
				parentMenu.TurnOff();
			}
			RunActionList(eventSlot);
		}

		private void OnCompleteLoad()
		{
			ClearAllEvents();
			if (autoHandle)
			{
				parentMenu.TurnOff(false);
			}
			RunActionList(eventSlot);
		}

		private void OnCompleteImport()
		{
			ClearAllEvents();
			RunActionList(eventSlot);
		}

		private void OnFailSaveLoad(int saveID)
		{
			ClearAllEvents();
			if (!autoHandle)
			{
				RunActionList(eventSlot);
			}
		}

		private void OnFailImport()
		{
			ClearAllEvents();
		}

		private void ClearAllEvents()
		{
			EventManager.OnFinishSaving = (EventManager.Delegate_SaveFile)Delegate.Remove(EventManager.OnFinishSaving, new EventManager.Delegate_SaveFile(OnCompleteSave));
			EventManager.OnFailSaving = (EventManager.Delegate_SaveID)Delegate.Remove(EventManager.OnFailSaving, new EventManager.Delegate_SaveID(OnFailSaveLoad));
			EventManager.OnFinishLoading = (EventManager.Delegate_Generic)Delegate.Remove(EventManager.OnFinishLoading, new EventManager.Delegate_Generic(OnCompleteLoad));
			EventManager.OnFailLoading = (EventManager.Delegate_SaveID)Delegate.Remove(EventManager.OnFailLoading, new EventManager.Delegate_SaveID(OnFailSaveLoad));
			EventManager.OnFinishImporting = (EventManager.Delegate_Generic)Delegate.Remove(EventManager.OnFinishImporting, new EventManager.Delegate_Generic(OnCompleteImport));
			EventManager.OnFailImporting = (EventManager.Delegate_Generic)Delegate.Remove(EventManager.OnFailImporting, new EventManager.Delegate_Generic(OnFailImport));
		}

		private void RunActionList(int _slot)
		{
			if (fixedOption)
			{
				AdvGame.RunActionListAsset(actionListOnSave, parameterID, optionToShow);
			}
			else
			{
				AdvGame.RunActionListAsset(actionListOnSave, parameterID, _slot + offset);
			}
		}

		public override void RecalculateSize(MenuSource source)
		{
			newSaveSlot = false;
			if (Application.isPlaying)
			{
				if (saveListType == AC_SaveListType.Import)
				{
					if (checkImportBool)
					{
						KickStarter.saveSystem.GatherImportFiles(importProductName, importSaveFilename, checkImportVar);
					}
					else
					{
						KickStarter.saveSystem.GatherImportFiles(importProductName, importSaveFilename, -1);
					}
				}
				if (fixedOption)
				{
					numSlots = 1;
					if (saveListType == AC_SaveListType.Save)
					{
						newSaveSlot = !SaveSystem.DoesSaveExist(optionToShow);
					}
				}
				else if (allowEmptySlots)
				{
					numSlots = maxSlots;
					offset = Mathf.Min(offset, GetMaxOffset());
				}
				else
				{
					if (saveListType == AC_SaveListType.Import)
					{
						numSlots = SaveSystem.GetNumImportSlots();
					}
					else
					{
						numSlots = SaveSystem.GetNumSlots();
						if (saveListType == AC_SaveListType.Save && numSlots < KickStarter.settingsManager.maxSaves && numSlots < maxSlots && showNewSaveOption)
						{
							newSaveSlot = true;
							numSlots++;
						}
					}
					if (numSlots > maxSlots)
					{
						numSlots = maxSlots;
					}
					offset = Mathf.Min(offset, GetMaxOffset());
				}
			}
			if (Application.isPlaying || labels == null || labels.Length != numSlots)
			{
				labels = new string[numSlots];
			}
			if (Application.isPlaying && uiSlots != null)
			{
				ClearSpriteCache(uiSlots);
			}
			if (!isVisible)
			{
				LimitUISlotVisibility(uiSlots, 0, uiHideStyle);
			}
			base.RecalculateSize(source);
		}

		protected override void AutoSize()
		{
			if (displayType == SaveDisplayType.ScreenshotOnly)
			{
				if (blankSlotTexture != null)
				{
					AutoSize(new GUIContent(blankSlotTexture));
				}
				else
				{
					AutoSize(GUIContent.none);
				}
			}
			else if (displayType == SaveDisplayType.LabelAndScreenshot)
			{
				if (blankSlotTexture != null)
				{
					AutoSize(new GUIContent(blankSlotTexture));
				}
				else
				{
					AutoSize(new GUIContent(SaveSystem.GetSaveSlotLabel(0, optionToShow, fixedOption)));
				}
			}
			else
			{
				AutoSize(new GUIContent(SaveSystem.GetSaveSlotLabel(0, optionToShow, fixedOption)));
			}
		}

		public override bool CanBeShifted(AC_ShiftInventory shiftType)
		{
			if (numSlots == 0 || fixedOption)
			{
				return false;
			}
			if (shiftType == AC_ShiftInventory.ShiftPrevious)
			{
				if (offset == 0)
				{
					return false;
				}
			}
			else if (offset >= GetMaxOffset())
			{
				return false;
			}
			return true;
		}

		private int GetMaxOffset()
		{
			if (numSlots == 0 || fixedOption)
			{
				return 0;
			}
			return Mathf.Max(0, GetNumFilledSlots() - maxSlots);
		}

		public override void Shift(AC_ShiftInventory shiftType, int amount)
		{
			if (!fixedOption && isVisible && numSlots >= maxSlots)
			{
				Shift(shiftType, maxSlots, GetNumFilledSlots(), amount);
			}
		}

		private int GetNumFilledSlots()
		{
			if (!fixedOption && allowEmptySlots)
			{
				return KickStarter.settingsManager.maxSaves;
			}
			if (saveListType == AC_SaveListType.Save && !fixedOption && newSaveSlot && showNewSaveOption)
			{
				return KickStarter.saveSystem.GetNumSaves() + 1;
			}
			return KickStarter.saveSystem.GetNumSaves();
		}

		private string TranslateLabel(string label, int _lineID, int languageNumber)
		{
			if (languageNumber == 0)
			{
				return label;
			}
			return KickStarter.runtimeLanguages.GetTranslation(label, _lineID, languageNumber);
		}

		public string GetTranslatableString(int index)
		{
			if (index == 1)
			{
				return emptySlotText;
			}
			return newSaveText;
		}

		public int GetTranslationID(int index)
		{
			if (index == 1)
			{
				return emptySlotTextLineID;
			}
			return lineID;
		}
	}
}
