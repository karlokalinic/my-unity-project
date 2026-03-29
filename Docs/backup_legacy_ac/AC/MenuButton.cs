using System;
using UnityEngine;
using UnityEngine.UI;

namespace AC
{
	[Serializable]
	public class MenuButton : MenuElement, ITranslatable
	{
		public UnityEngine.UI.Button uiButton;

		public UIPointerState uiPointerState;

		public string label = "Element";

		public string hotspotLabel = string.Empty;

		public int hotspotLabelID = -1;

		public TextAnchor anchor;

		public TextEffects textEffects;

		public float outlineSize = 2f;

		public AC_ButtonClickType buttonClickType;

		public ActionListAsset actionList;

		public int parameterID = -1;

		public int parameterValue;

		public bool doFade;

		public string switchMenuTitle;

		public string inventoryBoxTitle;

		public AC_ShiftInventory shiftInventory;

		public int shiftAmount = 1;

		public bool onlyShowWhenEffective;

		public bool loopJournal;

		public string inputAxis;

		public SimulateInputType simulateInput;

		public float simulateValue = 1f;

		public Texture2D clickTexture;

		public bool allowContinuousClick;

		public UISelectableHideStyle uiSelectableHideStyle;

		private MenuElement elementToShift;

		private float clickAlpha;

		private string fullText;

		private bool disabledUI;

		private Text uiText;

		public override void Declare()
		{
			uiText = null;
			uiButton = null;
			uiPointerState = UIPointerState.PointerClick;
			label = "Button";
			hotspotLabel = string.Empty;
			hotspotLabelID = -1;
			isVisible = true;
			isClickable = true;
			textEffects = TextEffects.None;
			outlineSize = 2f;
			buttonClickType = AC_ButtonClickType.RunActionList;
			simulateInput = SimulateInputType.Button;
			simulateValue = 1f;
			numSlots = 1;
			anchor = TextAnchor.MiddleCenter;
			SetSize(new Vector2(10f, 5f));
			doFade = false;
			switchMenuTitle = string.Empty;
			inventoryBoxTitle = string.Empty;
			shiftInventory = AC_ShiftInventory.ShiftPrevious;
			loopJournal = false;
			actionList = null;
			inputAxis = string.Empty;
			clickTexture = null;
			clickAlpha = 0f;
			shiftAmount = 1;
			onlyShowWhenEffective = false;
			allowContinuousClick = false;
			parameterID = -1;
			parameterValue = 0;
			uiSelectableHideStyle = UISelectableHideStyle.DisableObject;
			base.Declare();
		}

		public override MenuElement DuplicateSelf(bool fromEditor, bool ignoreUnityUI)
		{
			MenuButton menuButton = ScriptableObject.CreateInstance<MenuButton>();
			menuButton.Declare();
			menuButton.CopyButton(this, ignoreUnityUI);
			return menuButton;
		}

		private void CopyButton(MenuButton _element, bool ignoreUnityUI)
		{
			if (ignoreUnityUI)
			{
				uiButton = null;
				uiText = null;
			}
			else
			{
				uiButton = _element.uiButton;
				uiText = _element.uiText;
			}
			uiPointerState = _element.uiPointerState;
			label = _element.label;
			hotspotLabel = _element.hotspotLabel;
			hotspotLabelID = _element.hotspotLabelID;
			anchor = _element.anchor;
			textEffects = _element.textEffects;
			outlineSize = _element.outlineSize;
			buttonClickType = _element.buttonClickType;
			simulateInput = _element.simulateInput;
			simulateValue = _element.simulateValue;
			doFade = _element.doFade;
			switchMenuTitle = _element.switchMenuTitle;
			inventoryBoxTitle = _element.inventoryBoxTitle;
			shiftInventory = _element.shiftInventory;
			loopJournal = _element.loopJournal;
			actionList = _element.actionList;
			inputAxis = _element.inputAxis;
			clickTexture = _element.clickTexture;
			clickAlpha = _element.clickAlpha;
			shiftAmount = _element.shiftAmount;
			onlyShowWhenEffective = _element.onlyShowWhenEffective;
			allowContinuousClick = _element.allowContinuousClick;
			parameterID = _element.parameterID;
			parameterValue = _element.parameterValue;
			uiSelectableHideStyle = _element.uiSelectableHideStyle;
			base.Copy(_element);
		}

		public override void Initialise(Menu _menu)
		{
			if (buttonClickType == AC_ButtonClickType.OffsetElementSlot || buttonClickType == AC_ButtonClickType.OffsetJournal)
			{
				elementToShift = _menu.GetElementWithName(inventoryBoxTitle);
			}
			base.Initialise(_menu);
		}

		public override void LoadUnityUI(Menu _menu, Canvas canvas)
		{
			uiButton = LinkUIElement<UnityEngine.UI.Button>(canvas);
			if ((bool)uiButton)
			{
				uiText = uiButton.GetComponentInChildren<Text>();
				CreateUIEvent(uiButton, _menu, uiPointerState);
			}
		}

		public override GameObject GetObjectToSelect(int slotIndex = 0)
		{
			if ((bool)uiButton)
			{
				return uiButton.gameObject;
			}
			return null;
		}

		public override RectTransform GetRectTransform(int _slot)
		{
			if ((bool)uiButton)
			{
				return uiButton.GetComponent<RectTransform>();
			}
			return null;
		}

		public override void SetUIInteractableState(bool state)
		{
			if ((bool)uiButton)
			{
				disabledUI = !state;
				if (((!state || buttonClickType != AC_ButtonClickType.OffsetElementSlot) && buttonClickType != AC_ButtonClickType.OffsetJournal) || !onlyShowWhenEffective || uiSelectableHideStyle != UISelectableHideStyle.DisableInteractability || !Application.isPlaying || !(elementToShift != null) || (buttonClickType != AC_ButtonClickType.OffsetElementSlot && loopJournal) || elementToShift.CanBeShifted(shiftInventory))
				{
					uiButton.interactable = state;
				}
			}
		}

		public void ShowClick()
		{
			if (isClickable)
			{
				clickAlpha = 1f;
			}
		}

		public override string GetHotspotLabelOverride(int _slot, int _language)
		{
			if (uiButton != null && !uiButton.interactable)
			{
				return string.Empty;
			}
			return GetHotspotLabel(_language);
		}

		public override void PreDisplay(int _slot, int languageNumber, bool isActive)
		{
			SetEffectiveVisibility(true);
			fullText = TranslateLabel(label, languageNumber);
			fullText = AdvGame.ConvertTokens(fullText, languageNumber);
			if (uiButton != null)
			{
				if (uiSelectableHideStyle != UISelectableHideStyle.DisableInteractability || !disabledUI)
				{
					UpdateUISelectable(uiButton, uiSelectableHideStyle);
				}
				if (uiText != null)
				{
					uiText.text = fullText;
				}
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
			if (textEffects != TextEffects.None)
			{
				AdvGame.DrawTextEffect(ZoomRect(relativeRect, zoom), fullText, _style, Color.black, _style.normal.textColor, outlineSize, textEffects);
			}
			else
			{
				GUI.Label(ZoomRect(relativeRect, zoom), fullText, _style);
			}
			if (clickAlpha > 0f)
			{
				if ((bool)clickTexture)
				{
					Color color = GUI.color;
					color.a = clickAlpha;
					GUI.color = color;
					GUI.DrawTexture(ZoomRect(GetSlotRectRelative(_slot), zoom), clickTexture, ScaleMode.StretchToFill, true, 0f);
					color.a = 1f;
					GUI.color = color;
				}
				clickAlpha -= ((KickStarter.stateHandler.gameState != GameState.Paused) ? Time.deltaTime : 0.02f);
				if (clickAlpha < 0f)
				{
					clickAlpha = 0f;
				}
			}
		}

		public override string GetLabel(int slot, int languageNumber)
		{
			return TranslateLabel(label, languageNumber);
		}

		public override bool IsSelectedByEventSystem(int slotIndex)
		{
			if (uiButton != null)
			{
				return KickStarter.playerMenus.IsEventSystemSelectingObject(uiButton.gameObject);
			}
			return false;
		}

		protected override void AutoSize()
		{
			if (label == string.Empty && backgroundTexture != null)
			{
				GUIContent content = new GUIContent(backgroundTexture);
				AutoSize(content);
			}
			else
			{
				GUIContent content2 = new GUIContent(TranslateLabel(label, Options.GetLanguage()));
				AutoSize(content2);
			}
		}

		public override void RecalculateSize(MenuSource source)
		{
			SetEffectiveVisibility(false);
			clickAlpha = 0f;
			base.RecalculateSize(source);
		}

		private void SetEffectiveVisibility(bool fromPreDisplay)
		{
			if ((buttonClickType != AC_ButtonClickType.OffsetElementSlot && buttonClickType != AC_ButtonClickType.OffsetJournal) || !onlyShowWhenEffective || !Application.isPlaying || !(elementToShift != null) || (buttonClickType != AC_ButtonClickType.OffsetElementSlot && loopJournal))
			{
				return;
			}
			bool flag = elementToShift.CanBeShifted(shiftInventory);
			if (flag != isVisible)
			{
				base.IsVisible = flag;
				if (fromPreDisplay)
				{
					parentMenu.Recalculate();
				}
			}
		}

		public override void ProcessClick(Menu _menu, int _slot, MouseState _mouseState)
		{
			if (!_menu.IsClickable())
			{
				return;
			}
			ShowClick();
			if (buttonClickType == AC_ButtonClickType.TurnOffMenu)
			{
				_menu.TurnOff(doFade);
			}
			else if (buttonClickType == AC_ButtonClickType.Crossfade)
			{
				Menu menuWithName = PlayerMenus.GetMenuWithName(switchMenuTitle);
				if (menuWithName != null)
				{
					KickStarter.playerMenus.CrossFade(menuWithName);
				}
				else
				{
					ACDebug.LogWarning("Cannot find any menu of name '" + switchMenuTitle + "'");
				}
			}
			else if (buttonClickType == AC_ButtonClickType.OffsetElementSlot)
			{
				if (elementToShift != null)
				{
					elementToShift.Shift(shiftInventory, shiftAmount);
					elementToShift.RecalculateSize(_menu.menuSource);
					_menu.Recalculate();
				}
				else
				{
					ACDebug.LogWarning("Cannot find '" + inventoryBoxTitle + "' inside '" + _menu.title + "'");
				}
			}
			else if (buttonClickType == AC_ButtonClickType.OffsetJournal)
			{
				MenuJournal menuJournal = (MenuJournal)PlayerMenus.GetElementWithName(_menu.title, inventoryBoxTitle);
				if (menuJournal != null)
				{
					menuJournal.Shift(shiftInventory, loopJournal, shiftAmount);
					menuJournal.RecalculateSize(_menu.menuSource);
					_menu.Recalculate();
				}
				else
				{
					ACDebug.LogWarning("Cannot find '" + inventoryBoxTitle + "' inside '" + _menu.title + "'");
				}
			}
			else if (buttonClickType == AC_ButtonClickType.RunActionList)
			{
				if ((bool)actionList)
				{
					if (!actionList.canRunMultipleInstances)
					{
						KickStarter.actionListAssetManager.EndAssetList(actionList);
					}
					AdvGame.RunActionListAsset(actionList, parameterID, parameterValue);
				}
			}
			else if (buttonClickType == AC_ButtonClickType.CustomScript)
			{
				MenuSystem.OnElementClick(_menu, this, _slot, (int)_mouseState);
			}
			else if (buttonClickType == AC_ButtonClickType.SimulateInput)
			{
				KickStarter.playerInput.SimulateInput(simulateInput, inputAxis, simulateValue);
			}
			base.ProcessClick(_menu, _slot, _mouseState);
		}

		public override void ProcessContinuousClick(Menu _menu, MouseState _mouseState)
		{
			if (buttonClickType == AC_ButtonClickType.SimulateInput)
			{
				if (!(uiButton != null) || uiPointerState != UIPointerState.PointerClick)
				{
					KickStarter.playerInput.SimulateInput(simulateInput, inputAxis, simulateValue);
				}
			}
			else if (buttonClickType == AC_ButtonClickType.CustomScript && allowContinuousClick)
			{
				MenuSystem.OnElementClick(_menu, this, 0, (int)_mouseState);
			}
		}

		public string GetHotspotLabel(int languageNumber)
		{
			if (languageNumber > 0)
			{
				return KickStarter.runtimeLanguages.GetTranslation(hotspotLabel, hotspotLabelID, languageNumber);
			}
			return hotspotLabel;
		}

		public string GetTranslatableString(int index)
		{
			if (index == 0)
			{
				return label;
			}
			return hotspotLabel;
		}

		public int GetTranslationID(int index)
		{
			if (index == 0)
			{
				return lineID;
			}
			return hotspotLabelID;
		}
	}
}
