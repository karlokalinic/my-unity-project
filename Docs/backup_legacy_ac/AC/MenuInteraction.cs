using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AC
{
	public class MenuInteraction : MenuElement
	{
		public UnityEngine.UI.Button uiButton;

		public UIPointerState uiPointerState;

		public AC_DisplayType displayType;

		public TextAnchor anchor;

		public TextEffects textEffects;

		public float outlineSize = 2f;

		public int iconID;

		public UISelectableHideStyle uiSelectableHideStyle;

		public bool overrideTexture;

		public Texture activeTexture;

		private Text uiText;

		private Image uiImage;

		private CursorIcon icon;

		private string label = string.Empty;

		private bool isDefaultIcon;

		private CursorManager cursorManager;

		public bool IsDefaultIcon
		{
			get
			{
				return isDefaultIcon;
			}
		}

		public override void Declare()
		{
			uiButton = null;
			uiPointerState = UIPointerState.PointerClick;
			uiImage = null;
			uiText = null;
			isVisible = true;
			isClickable = true;
			numSlots = 1;
			anchor = TextAnchor.MiddleCenter;
			SetSize(new Vector2(5f, 5f));
			iconID = -1;
			textEffects = TextEffects.None;
			outlineSize = 2f;
			overrideTexture = false;
			activeTexture = null;
			uiSelectableHideStyle = UISelectableHideStyle.DisableObject;
			base.Declare();
		}

		public override MenuElement DuplicateSelf(bool fromEditor, bool ignoreUnityUI)
		{
			MenuInteraction menuInteraction = ScriptableObject.CreateInstance<MenuInteraction>();
			menuInteraction.Declare();
			menuInteraction.CopyInteraction(this, ignoreUnityUI);
			return menuInteraction;
		}

		private void CopyInteraction(MenuInteraction _element, bool ignoreUnityUI)
		{
			if (ignoreUnityUI)
			{
				uiButton = null;
			}
			else
			{
				uiButton = _element.uiButton;
			}
			uiPointerState = _element.uiPointerState;
			uiText = null;
			uiImage = null;
			displayType = _element.displayType;
			anchor = _element.anchor;
			textEffects = _element.textEffects;
			outlineSize = _element.outlineSize;
			iconID = _element.iconID;
			overrideTexture = _element.overrideTexture;
			activeTexture = _element.activeTexture;
			uiSelectableHideStyle = _element.uiSelectableHideStyle;
			base.Copy(_element);
		}

		public override void LoadUnityUI(Menu _menu, Canvas canvas)
		{
			uiButton = LinkUIElement<UnityEngine.UI.Button>(canvas);
			if ((bool)uiButton)
			{
				uiText = uiButton.GetComponentInChildren<Text>();
				uiImage = uiButton.GetComponentInChildren<Image>();
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
				uiButton.interactable = state;
			}
		}

		public override void PreDisplay(int _slot, int languageNumber, bool isActive)
		{
			isDefaultIcon = false;
			if (Application.isPlaying && KickStarter.stateHandler.IsInGameplay() && KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot)
			{
				if (KickStarter.settingsManager.allowDefaultinteractions && KickStarter.runtimeInventory.SelectedItem == null && KickStarter.playerInteraction.GetActiveHotspot() != null && KickStarter.playerInteraction.GetActiveHotspot().GetFirstUseIcon() == iconID)
				{
					isActive = true;
					isDefaultIcon = true;
				}
				else if (KickStarter.settingsManager.allowDefaultInventoryInteractions && KickStarter.settingsManager.inventoryInteractions == InventoryInteractions.Multiple && KickStarter.settingsManager.CanSelectItems(false) && KickStarter.playerInteraction.GetActiveHotspot() == null && KickStarter.runtimeInventory.SelectedItem == null && KickStarter.runtimeInventory.hoverItem != null && KickStarter.runtimeInventory.hoverItem.GetFirstStandardIcon() == iconID)
				{
					isActive = true;
					isDefaultIcon = true;
				}
			}
			if (uiButton != null)
			{
				UpdateUISelectable(uiButton, uiSelectableHideStyle);
				if (displayType != AC_DisplayType.IconOnly && uiText != null)
				{
					uiText.text = label;
				}
				if (displayType == AC_DisplayType.IconOnly && uiImage != null && icon != null && icon.isAnimated)
				{
					uiImage.sprite = icon.GetAnimatedSprite(isActive);
				}
				if (KickStarter.settingsManager.SelectInteractionMethod() == SelectInteractions.CyclingMenuAndClickingHotspot && iconID == KickStarter.playerInteraction.GetActiveUseButtonIconID())
				{
					uiButton.Select();
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
			if (displayType != AC_DisplayType.IconOnly)
			{
				if (textEffects != TextEffects.None)
				{
					AdvGame.DrawTextEffect(ZoomRect(relativeRect, zoom), label, _style, Color.black, _style.normal.textColor, outlineSize, textEffects);
				}
				else
				{
					GUI.Label(ZoomRect(relativeRect, zoom), label, _style);
				}
			}
			else
			{
				GUI.Label(ZoomRect(relativeRect, zoom), string.Empty, _style);
			}
			if (overrideTexture)
			{
				if (iconID >= 0 && KickStarter.playerCursor.GetSelectedCursorID() == iconID && activeTexture != null)
				{
					GUI.DrawTexture(ZoomRect(GetSlotRectRelative(_slot), zoom), activeTexture, ScaleMode.StretchToFill, true, 0f);
				}
			}
			else if (displayType != AC_DisplayType.TextOnly && icon != null)
			{
				icon.DrawAsInteraction(ZoomRect(relativeRect, zoom), isActive);
			}
		}

		public void MatchInteractions(InvItem item)
		{
			bool flag = false;
			foreach (InvInteraction interaction in item.interactions)
			{
				if (interaction.icon.id == iconID)
				{
					flag = true;
					break;
				}
			}
			base.IsVisible = flag;
		}

		public void MatchInteractions(List<Button> buttons)
		{
			bool flag = false;
			foreach (Button button in buttons)
			{
				if (button.iconID == iconID && !button.isDisabled)
				{
					flag = true;
					break;
				}
			}
			base.IsVisible = flag;
		}

		public void MatchUseInteraction(Button button)
		{
			if (button.iconID == iconID && !button.isDisabled)
			{
				base.IsVisible = true;
			}
		}

		public void MatchInteraction(int _iconID)
		{
			if (_iconID == iconID)
			{
				base.IsVisible = true;
			}
		}

		public override string GetLabel(int slot, int languageNumber)
		{
			return label;
		}

		public override bool IsSelectedByEventSystem(int slotIndex)
		{
			if (uiButton != null)
			{
				return KickStarter.playerMenus.IsEventSystemSelectingObject(uiButton.gameObject);
			}
			return false;
		}

		public override void RecalculateSize(MenuSource source)
		{
			if ((bool)AdvGame.GetReferences().cursorManager)
			{
				CursorIcon cursorIconFromID = AdvGame.GetReferences().cursorManager.GetCursorIconFromID(iconID);
				if (cursorIconFromID != null)
				{
					icon = cursorIconFromID;
					if (Application.isPlaying)
					{
						label = KickStarter.runtimeLanguages.GetTranslation(cursorIconFromID.label, cursorIconFromID.lineID, Options.GetLanguage());
					}
					else
					{
						label = cursorIconFromID.label;
					}
					icon.Reset();
				}
			}
			base.RecalculateSize(source);
		}

		public override void ProcessClick(Menu _menu, int _slot, MouseState _mouseState)
		{
			if (KickStarter.stateHandler.gameState != GameState.Cutscene && _mouseState != MouseState.RightClick)
			{
				KickStarter.playerInteraction.ClickInteractionIcon(_menu, iconID);
				base.ProcessClick(_menu, _slot, _mouseState);
			}
		}

		protected override void AutoSize()
		{
			if (displayType == AC_DisplayType.IconOnly && icon != null && icon.texture != null)
			{
				GUIContent content = new GUIContent(icon.texture);
				AutoSize(content);
			}
			else
			{
				GUIContent content2 = new GUIContent(TranslateLabel(label, Options.GetLanguage()));
				AutoSize(content2);
			}
		}

		public override string GetHotspotLabelOverride(int _slot, int _language)
		{
			if (uiButton != null && !uiButton.interactable)
			{
				return string.Empty;
			}
			if (KickStarter.cursorManager.addHotspotPrefix)
			{
				if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction)
				{
					if (KickStarter.settingsManager.SelectInteractionMethod() != SelectInteractions.ClickingMenu)
					{
						return string.Empty;
					}
					if (parentMenu.TargetInvItem != null)
					{
						return AdvGame.CombineLanguageString(KickStarter.cursorManager.GetLabelFromID(iconID, _language), parentMenu.TargetInvItem.GetLabel(_language), _language);
					}
					if (KickStarter.settingsManager.SelectInteractionMethod() != SelectInteractions.ClickingMenu)
					{
						return string.Empty;
					}
					if (parentMenu.TargetHotspot != null)
					{
						return AdvGame.CombineLanguageString(KickStarter.cursorManager.GetLabelFromID(iconID, _language), parentMenu.TargetHotspot.GetName(_language), _language);
					}
				}
				else if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot && KickStarter.settingsManager.ShowHoverInteractionInHotspotLabel() && KickStarter.playerCursor.GetSelectedCursor() == -1)
				{
					return KickStarter.cursorManager.GetLabelFromID(iconID, _language);
				}
			}
			return string.Empty;
		}
	}
}
