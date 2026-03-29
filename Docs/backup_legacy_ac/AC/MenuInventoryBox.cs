using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AC
{
	public class MenuInventoryBox : MenuElement
	{
		public enum ContainerSelectMode
		{
			MoveToInventory = 0,
			MoveToInventoryAndSelect = 1,
			SelectItemOnly = 2
		}

		private struct LimitedItemList
		{
			private List<InvItem> limitedItems;

			private int offset;

			public List<InvItem> LimitedItems
			{
				get
				{
					return limitedItems;
				}
			}

			public int Offset
			{
				get
				{
					return offset;
				}
			}

			public LimitedItemList(List<InvItem> _limitedItems, int _offset)
			{
				limitedItems = _limitedItems;
				offset = _offset;
			}
		}

		public UISlot[] uiSlots;

		public UIPointerState uiPointerState;

		public TextEffects textEffects;

		public TextAnchor anchor = TextAnchor.MiddleCenter;

		public float outlineSize = 2f;

		public AC_InventoryBoxType inventoryBoxType;

		public ActionListAsset actionListOnClick;

		public int maxSlots;

		public bool limitToCategory;

		public bool limitToDefinedInteractions = true;

		public int categoryID;

		public List<int> categoryIDs = new List<int>();

		public LinkUIGraphic linkUIGraphic;

		public bool preventInteractions;

		public bool limitMaxScroll = true;

		public bool updateHotspotLabelWhenHover;

		public bool hoverSoundOverEmptySlots = true;

		public List<InvItem> items = new List<InvItem>();

		public ContainerSelectMode containerSelectMode = ContainerSelectMode.MoveToInventoryAndSelect;

		public ConversationDisplayType displayType = ConversationDisplayType.IconOnly;

		public UIHideStyle uiHideStyle;

		public bool autoOpenDocument = true;

		public Texture2D emptySlotTexture;

		public InventoryItemCountDisplay inventoryItemCountDisplay;

		public ObjectiveDisplayType objectiveDisplayType;

		private Container overrideContainer;

		private string[] labels;

		private int numDocuments;

		private Texture[] textures;

		public Container OverrideContainer
		{
			set
			{
				overrideContainer = value;
			}
		}

		public override void Declare()
		{
			uiSlots = null;
			uiPointerState = UIPointerState.PointerClick;
			isVisible = true;
			isClickable = true;
			inventoryBoxType = AC_InventoryBoxType.Default;
			actionListOnClick = null;
			anchor = TextAnchor.MiddleCenter;
			numSlots = 0;
			SetSize(new Vector2(6f, 10f));
			maxSlots = 10;
			limitToCategory = false;
			limitToDefinedInteractions = true;
			containerSelectMode = ContainerSelectMode.MoveToInventoryAndSelect;
			categoryID = -1;
			displayType = ConversationDisplayType.IconOnly;
			textEffects = TextEffects.None;
			outlineSize = 2f;
			uiHideStyle = UIHideStyle.DisableObject;
			emptySlotTexture = null;
			objectiveDisplayType = ObjectiveDisplayType.All;
			items = new List<InvItem>();
			categoryIDs = new List<int>();
			linkUIGraphic = LinkUIGraphic.ImageComponent;
			autoOpenDocument = true;
			updateHotspotLabelWhenHover = false;
			hoverSoundOverEmptySlots = true;
			preventInteractions = false;
			limitMaxScroll = true;
			inventoryItemCountDisplay = InventoryItemCountDisplay.OnlyIfMultiple;
		}

		public override MenuElement DuplicateSelf(bool fromEditor, bool ignoreUnityUI)
		{
			MenuInventoryBox menuInventoryBox = ScriptableObject.CreateInstance<MenuInventoryBox>();
			menuInventoryBox.Declare();
			menuInventoryBox.CopyInventoryBox(this, ignoreUnityUI);
			return menuInventoryBox;
		}

		private void CopyInventoryBox(MenuInventoryBox _element, bool ignoreUnityUI)
		{
			if (ignoreUnityUI)
			{
				uiSlots = null;
			}
			else
			{
				uiSlots = _element.uiSlots;
			}
			uiPointerState = _element.uiPointerState;
			isClickable = _element.isClickable;
			textEffects = _element.textEffects;
			outlineSize = _element.outlineSize;
			anchor = _element.anchor;
			inventoryBoxType = _element.inventoryBoxType;
			actionListOnClick = _element.actionListOnClick;
			numSlots = _element.numSlots;
			maxSlots = _element.maxSlots;
			limitToCategory = _element.limitToCategory;
			limitToDefinedInteractions = _element.limitToDefinedInteractions;
			categoryID = _element.categoryID;
			containerSelectMode = _element.containerSelectMode;
			displayType = _element.displayType;
			uiHideStyle = _element.uiHideStyle;
			emptySlotTexture = _element.emptySlotTexture;
			objectiveDisplayType = _element.objectiveDisplayType;
			categoryIDs = _element.categoryIDs;
			linkUIGraphic = _element.linkUIGraphic;
			autoOpenDocument = _element.autoOpenDocument;
			updateHotspotLabelWhenHover = _element.updateHotspotLabelWhenHover;
			hoverSoundOverEmptySlots = _element.hoverSoundOverEmptySlots;
			preventInteractions = _element.preventInteractions;
			limitMaxScroll = _element.limitMaxScroll;
			inventoryItemCountDisplay = _element.inventoryItemCountDisplay;
			UpdateLimitCategory();
			items = GetItemList();
			base.Copy(_element);
			if (Application.isPlaying && (inventoryBoxType != AC_InventoryBoxType.HotspotBased || maxSlots != 1))
			{
				alternativeInputButton = string.Empty;
			}
			Upgrade();
		}

		private void Upgrade()
		{
			if (limitToCategory && categoryID >= 0)
			{
				categoryIDs.Add(categoryID);
				categoryID = -1;
				if (Application.isPlaying)
				{
					ACDebug.Log("The inventory box element '" + title + "' has been upgraded - please view it in the Menu Manager and Save.");
				}
			}
		}

		private void UpdateLimitCategory()
		{
			if (!Application.isPlaying || !(AdvGame.GetReferences().inventoryManager != null) || AdvGame.GetReferences().inventoryManager.bins == null)
			{
				return;
			}
			foreach (InvBin bin in KickStarter.inventoryManager.bins)
			{
				if (!categoryIDs.Contains(bin.id))
				{
					categoryIDs.Remove(bin.id);
				}
			}
		}

		public override void LoadUnityUI(Menu _menu, Canvas canvas)
		{
			int num = 0;
			UISlot[] array = uiSlots;
			foreach (UISlot uISlot in array)
			{
				uISlot.LinkUIElements(canvas, linkUIGraphic, (inventoryBoxType == AC_InventoryBoxType.CollectedDocuments) ? null : emptySlotTexture);
				if (uISlot != null && uISlot.uiButton != null)
				{
					int num2 = num;
					if ((inventoryBoxType == AC_InventoryBoxType.Default || inventoryBoxType == AC_InventoryBoxType.CustomScript) && (!(KickStarter.settingsManager != null) || KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive || KickStarter.settingsManager.inventoryInteractions != InventoryInteractions.Multiple))
					{
						uiPointerState = UIPointerState.PointerClick;
					}
					CreateUIEvent(uISlot.uiButton, _menu, uiPointerState, num2, false);
					uISlot.AddClickHandler(_menu, this, num2);
				}
				num++;
			}
		}

		public UnityEngine.UI.Button GetUIButtonWithItem(int itemID)
		{
			for (int i = 0; i < items.Count; i++)
			{
				if (items[i] != null && items[i].id == itemID)
				{
					if (uiSlots != null && uiSlots.Length > i && uiSlots[i] != null)
					{
						return uiSlots[i].uiButton;
					}
					return null;
				}
			}
			return null;
		}

		public override GameObject GetObjectToSelect(int slotIndex = 0)
		{
			if (uiSlots != null && uiSlots.Length > slotIndex && uiSlots[slotIndex].uiButton != null)
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

		public override void HideAllUISlots()
		{
			LimitUISlotVisibility(uiSlots, 0, uiHideStyle, emptySlotTexture);
		}

		public override void SetUIInteractableState(bool state)
		{
			SetUISlotsInteractableState(uiSlots, state);
		}

		public override string GetHotspotLabelOverride(int _slot, int _language)
		{
			if (uiSlots != null && _slot < uiSlots.Length && !uiSlots[_slot].CanOverrideHotspotLabel)
			{
				return string.Empty;
			}
			if (inventoryBoxType == AC_InventoryBoxType.CollectedDocuments || inventoryBoxType == AC_InventoryBoxType.Objectives)
			{
				if (displayType == ConversationDisplayType.IconOnly || updateHotspotLabelWhenHover)
				{
					return labels[_slot];
				}
				return string.Empty;
			}
			InvItem item = GetItem(_slot);
			if (item == null)
			{
				return string.Empty;
			}
			string label = item.GetLabel(_language);
			if (inventoryBoxType == AC_InventoryBoxType.HotspotBased)
			{
				if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction && KickStarter.settingsManager.SelectInteractionMethod() != SelectInteractions.ClickingMenu)
				{
					return string.Empty;
				}
				if (KickStarter.cursorManager.addHotspotPrefix)
				{
					string hotspotPrefixLabel = KickStarter.runtimeInventory.GetHotspotPrefixLabel(item, label, _language);
					if (parentMenu.TargetInvItem != null)
					{
						return AdvGame.CombineLanguageString(hotspotPrefixLabel, parentMenu.TargetInvItem.GetLabel(_language), _language);
					}
					if (parentMenu.TargetHotspot != null)
					{
						return AdvGame.CombineLanguageString(hotspotPrefixLabel, parentMenu.TargetHotspot.GetName(_language), _language);
					}
				}
				else if (parentMenu.TargetInvItem != null)
				{
					return parentMenu.TargetInvItem.GetLabel(_language);
				}
				return string.Empty;
			}
			InvItem selectedItem = KickStarter.runtimeInventory.SelectedItem;
			if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive)
			{
				if (selectedItem != null && KickStarter.cursorManager.inventoryHandling != InventoryHandling.ChangeCursor)
				{
					if (selectedItem.id == item.id)
					{
						return label;
					}
					string hotspotPrefixLabel2 = KickStarter.runtimeInventory.GetHotspotPrefixLabel(selectedItem, selectedItem.GetLabel(_language), _language);
					return AdvGame.CombineLanguageString(hotspotPrefixLabel2, label, _language);
				}
				return label;
			}
			if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot || KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction)
			{
				if (selectedItem != null)
				{
					if (KickStarter.cursorManager.inventoryHandling != InventoryHandling.ChangeCursor)
					{
						if (selectedItem.id == item.id)
						{
							return label;
						}
						string hotspotPrefixLabel3 = KickStarter.runtimeInventory.GetHotspotPrefixLabel(selectedItem, selectedItem.GetLabel(_language), _language);
						return AdvGame.CombineLanguageString(hotspotPrefixLabel3, label, _language);
					}
				}
				else if (KickStarter.cursorManager.addHotspotPrefix)
				{
					if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction && KickStarter.settingsManager.SelectInteractionMethod() != SelectInteractions.ClickingMenu && KickStarter.settingsManager.inventoryInteractions == InventoryInteractions.Multiple)
					{
						string labelPrefix = KickStarter.playerInteraction.GetLabelPrefix(null, item, _language);
						return AdvGame.CombineLanguageString(labelPrefix, label, _language);
					}
					if (KickStarter.playerCursor.GetSelectedCursor() >= 0)
					{
						string labelFromID = KickStarter.cursorManager.GetLabelFromID(KickStarter.playerCursor.GetSelectedCursorID(), _language);
						return AdvGame.CombineLanguageString(labelFromID, label, _language);
					}
				}
				return label;
			}
			return string.Empty;
		}

		public override void PreDisplay(int _slot, int languageNumber, bool isActive)
		{
			if (inventoryBoxType == AC_InventoryBoxType.CollectedDocuments || inventoryBoxType == AC_InventoryBoxType.Objectives)
			{
				if (Application.isPlaying)
				{
					if (uiSlots != null && uiSlots.Length > _slot)
					{
						LimitUISlotVisibility(uiSlots, numSlots, uiHideStyle);
						if (displayType == ConversationDisplayType.IconOnly || displayType == ConversationDisplayType.IconAndText)
						{
							uiSlots[_slot].SetImage(textures[_slot]);
						}
						if (displayType == ConversationDisplayType.TextOnly || displayType == ConversationDisplayType.IconAndText)
						{
							uiSlots[_slot].SetText(labels[_slot]);
						}
					}
				}
				else
				{
					string empty = string.Empty;
					empty = ((inventoryBoxType != AC_InventoryBoxType.CollectedDocuments) ? ("Objective #" + _slot) : ("Document #" + _slot));
					if (labels == null || labels.Length != numSlots)
					{
						labels = new string[numSlots];
					}
					labels[_slot] = empty;
				}
				return;
			}
			if (items.Count > 0 && items.Count > _slot + offset && items[_slot + offset] != null)
			{
				string text = string.Empty;
				if (displayType == ConversationDisplayType.TextOnly || displayType == ConversationDisplayType.IconAndText)
				{
					text = items[_slot + offset].label;
					if (KickStarter.runtimeInventory != null)
					{
						text = KickStarter.runtimeInventory.GetLabel(items[_slot + offset], languageNumber);
					}
					string count = GetCount(_slot);
					if (!string.IsNullOrEmpty(count))
					{
						text = text + " (" + count + ")";
					}
				}
				else
				{
					string count2 = GetCount(_slot);
					if (!string.IsNullOrEmpty(count2))
					{
						text = count2;
					}
				}
				if (labels == null || labels.Length != numSlots)
				{
					labels = new string[numSlots];
				}
				labels[_slot] = text;
			}
			if (!Application.isPlaying || uiSlots == null || uiSlots.Length <= _slot)
			{
				return;
			}
			LimitUISlotVisibility(uiSlots, numSlots, uiHideStyle, emptySlotTexture);
			uiSlots[_slot].SetText(labels[_slot]);
			if (displayType == ConversationDisplayType.IconOnly || displayType == ConversationDisplayType.IconAndText)
			{
				Texture texture = null;
				if (items.Count > _slot + offset && items[_slot + offset] != null)
				{
					if (inventoryBoxType != AC_InventoryBoxType.DisplaySelected && inventoryBoxType != AC_InventoryBoxType.DisplayLastSelected)
					{
						if (KickStarter.settingsManager.selectInventoryDisplay == SelectInventoryDisplay.HideFromMenu && ItemIsSelected(_slot + offset) && !items[_slot + offset].CanSelectSingle())
						{
							uiSlots[_slot].SetImage(null);
							labels[_slot] = string.Empty;
							uiSlots[_slot].SetText(labels[_slot]);
							return;
						}
						texture = GetTexture(_slot + offset, isActive);
					}
					if (texture == null)
					{
						texture = items[_slot + offset].tex;
					}
				}
				uiSlots[_slot].SetImage(texture);
			}
			if (KickStarter.settingsManager.SelectInteractionMethod() == SelectInteractions.CyclingMenuAndClickingHotspot && inventoryBoxType == AC_InventoryBoxType.HotspotBased && items[_slot + offset].id == KickStarter.playerInteraction.GetActiveInvButtonID() && uiSlots[_slot].uiButton != null)
			{
				uiSlots[_slot].uiButton.Select();
			}
		}

		private bool ItemIsSelected(int index)
		{
			if (items[index] != null && (!KickStarter.settingsManager.InventoryDragDrop || KickStarter.playerInput.GetDragState() == DragState.Inventory))
			{
				if (items[index] == KickStarter.runtimeInventory.SelectedItem)
				{
					return true;
				}
				if (inventoryBoxType == AC_InventoryBoxType.Container && containerSelectMode == ContainerSelectMode.SelectItemOnly && KickStarter.runtimeInventory.selectedContainerItem != null)
				{
					Container container = ((!(overrideContainer != null)) ? KickStarter.playerInput.activeContainer : overrideContainer);
					if (container != null && container.items[index] == KickStarter.runtimeInventory.selectedContainerItem)
					{
						return true;
					}
				}
			}
			return false;
		}

		public override void Display(GUIStyle _style, int _slot, float zoom, bool isActive)
		{
			base.Display(_style, _slot, zoom, isActive);
			_style.wordWrap = true;
			if (displayType == ConversationDisplayType.TextOnly)
			{
				_style.alignment = anchor;
			}
			if (inventoryBoxType == AC_InventoryBoxType.CollectedDocuments || inventoryBoxType == AC_InventoryBoxType.Objectives)
			{
				if (zoom < 1f)
				{
					_style.fontSize = (int)((float)_style.fontSize * zoom);
				}
				if (displayType == ConversationDisplayType.TextOnly)
				{
					if (textEffects != TextEffects.None)
					{
						AdvGame.DrawTextEffect(ZoomRect(GetSlotRectRelative(_slot), zoom), labels[_slot], _style, Color.black, _style.normal.textColor, outlineSize, textEffects);
					}
					else
					{
						GUI.Label(ZoomRect(GetSlotRectRelative(_slot), zoom), labels[_slot], _style);
					}
					return;
				}
				if (Application.isPlaying && textures[_slot] != null)
				{
					GUI.DrawTexture(ZoomRect(GetSlotRectRelative(_slot), zoom), textures[_slot], ScaleMode.StretchToFill, true, 0f);
				}
				GUI.Label(ZoomRect(GetSlotRectRelative(_slot), zoom), string.Empty, _style);
			}
			else if (items.Count > 0 && items.Count > _slot + offset && items[_slot + offset] != null)
			{
				if (Application.isPlaying && KickStarter.settingsManager.selectInventoryDisplay == SelectInventoryDisplay.HideFromMenu && ItemIsSelected(_slot + offset) && !items[_slot + offset].CanSelectSingle())
				{
					return;
				}
				Rect slotRectRelative = GetSlotRectRelative(_slot);
				if (displayType == ConversationDisplayType.IconOnly)
				{
					GUI.Label(slotRectRelative, string.Empty, _style);
					DrawTexture(ZoomRect(slotRectRelative, zoom), _slot + offset, isActive);
					_style.normal.background = null;
					if (textEffects != TextEffects.None)
					{
						AdvGame.DrawTextEffect(ZoomRect(slotRectRelative, zoom), GetCount(_slot), _style, Color.black, _style.normal.textColor, outlineSize, textEffects);
					}
					else
					{
						GUI.Label(ZoomRect(slotRectRelative, zoom), GetCount(_slot), _style);
					}
				}
				else if (displayType == ConversationDisplayType.TextOnly)
				{
					if (textEffects != TextEffects.None)
					{
						AdvGame.DrawTextEffect(ZoomRect(slotRectRelative, zoom), labels[_slot], _style, Color.black, _style.normal.textColor, outlineSize, textEffects);
					}
					else
					{
						GUI.Label(ZoomRect(slotRectRelative, zoom), labels[_slot], _style);
					}
				}
			}
			else if (displayType == ConversationDisplayType.IconOnly && emptySlotTexture != null)
			{
				Rect slotRectRelative2 = GetSlotRectRelative(_slot);
				_style.normal.background = null;
				GUI.Label(slotRectRelative2, string.Empty, _style);
				GUI.DrawTexture(ZoomRect(slotRectRelative2, zoom), emptySlotTexture, ScaleMode.StretchToFill, true, 0f);
			}
		}

		private bool AllowInteractions()
		{
			if (inventoryBoxType == AC_InventoryBoxType.Default || inventoryBoxType == AC_InventoryBoxType.Container)
			{
				return !preventInteractions;
			}
			return true;
		}

		public void HandleDefaultClick(MouseState _mouseState, int _slot, AC_InteractionMethod interactionMethod)
		{
			if (inventoryBoxType == AC_InventoryBoxType.CollectedDocuments || inventoryBoxType == AC_InventoryBoxType.Objectives || (KickStarter.stateHandler.gameState == GameState.DialogOptions && !KickStarter.settingsManager.allowInventoryInteractionsDuringConversations && !KickStarter.settingsManager.allowGameplayDuringConversations) || !(KickStarter.runtimeInventory != null))
			{
				return;
			}
			KickStarter.playerMenus.CloseInteractionMenus();
			KickStarter.runtimeInventory.HighlightItemOffInstant();
			KickStarter.runtimeInventory.SetFont(font, GetFontSize(), fontColor, textEffects);
			int num = _slot + offset;
			if (inventoryBoxType == AC_InventoryBoxType.Default && (items.Count <= num || items[num] == null))
			{
				ContainerItem containerItem = ((KickStarter.runtimeInventory.selectedContainerItem == null) ? null : new ContainerItem(KickStarter.runtimeInventory.selectedContainerItem));
				Container selectedContainerItemContainer = KickStarter.runtimeInventory.selectedContainerItemContainer;
				if (containerItem != null && !KickStarter.runtimeInventory.CanTransferContainerItemsToInventory(containerItem))
				{
					return;
				}
				if (KickStarter.runtimeInventory.SelectedItem != null && KickStarter.settingsManager.canReorderItems)
				{
					if (limitToCategory && categoryIDs != null && categoryIDs.Count > 0)
					{
						List<InvItem> itemList = GetItemList(false);
						num += LimitByCategory(itemList, num).Offset;
					}
					KickStarter.runtimeInventory.MoveItemToIndex(KickStarter.runtimeInventory.SelectedItem, num);
				}
				else if (containerItem != null)
				{
					num = KickStarter.runtimeInventory.localItems.Count;
					KickStarter.runtimeInventory.MoveItemToIndex(KickStarter.runtimeInventory.SelectedItem, num);
				}
				if (containerItem != null && selectedContainerItemContainer != null)
				{
					KickStarter.eventManager.Call_OnUseContainer(false, selectedContainerItemContainer, containerItem);
				}
				KickStarter.runtimeInventory.SetNull();
			}
			else
			{
				if (KickStarter.runtimeInventory.selectedContainerItem != null)
				{
					return;
				}
				switch (interactionMethod)
				{
				case AC_InteractionMethod.ChooseHotspotThenInteraction:
					if (KickStarter.runtimeInventory.SelectedItem != null)
					{
						switch (_mouseState)
						{
						case MouseState.SingleClick:
							if (items.Count <= num)
							{
								break;
							}
							if (!AllowInteractions())
							{
								if (items[num] == KickStarter.runtimeInventory.SelectedItem)
								{
									KickStarter.runtimeInventory.SetNull();
								}
							}
							else
							{
								KickStarter.runtimeInventory.Combine(KickStarter.runtimeInventory.SelectedItem, items[num]);
							}
							break;
						case MouseState.RightClick:
							KickStarter.runtimeInventory.SetNull();
							break;
						}
					}
					else if (items.Count > num)
					{
						if (!AllowInteractions())
						{
							KickStarter.runtimeInventory.SelectItem(items[num]);
						}
						else
						{
							KickStarter.runtimeInventory.ShowInteractions(items[num]);
						}
					}
					break;
				case AC_InteractionMethod.ChooseInteractionThenHotspot:
				{
					if (items.Count <= num || _mouseState != MouseState.SingleClick)
					{
						break;
					}
					int selectedCursorID = KickStarter.playerCursor.GetSelectedCursorID();
					int selectedCursor = KickStarter.playerCursor.GetSelectedCursor();
					if (selectedCursor == -2 && KickStarter.runtimeInventory.SelectedItem != null)
					{
						if (items[num] == KickStarter.runtimeInventory.SelectedItem)
						{
							KickStarter.runtimeInventory.SelectItem(items[num]);
						}
						else if (AllowInteractions())
						{
							KickStarter.runtimeInventory.Combine(KickStarter.runtimeInventory.SelectedItem, items[num]);
						}
					}
					else if ((selectedCursor == -1 && !KickStarter.settingsManager.selectInvWithUnhandled) || !AllowInteractions())
					{
						KickStarter.runtimeInventory.SelectItem(items[num]);
					}
					else if (selectedCursorID > -1)
					{
						KickStarter.runtimeInventory.RunInteraction(items[num], selectedCursorID);
					}
					break;
				}
				case AC_InteractionMethod.ContextSensitive:
					switch (_mouseState)
					{
					case MouseState.SingleClick:
						if (items.Count <= num)
						{
							break;
						}
						if (KickStarter.runtimeInventory.SelectedItem == null)
						{
							if (!AllowInteractions())
							{
								KickStarter.runtimeInventory.SelectItem(items[num]);
							}
							else if (KickStarter.cursorManager.lookUseCursorAction == LookUseCursorAction.RightClickCyclesModes && KickStarter.playerCursor.ContextCycleExamine)
							{
								KickStarter.runtimeInventory.Look(items[num]);
							}
							else
							{
								KickStarter.runtimeInventory.Use(items[num]);
							}
						}
						else if (AllowInteractions())
						{
							KickStarter.runtimeInventory.Combine(KickStarter.runtimeInventory.SelectedItem, items[num]);
						}
						break;
					case MouseState.RightClick:
						if (KickStarter.runtimeInventory.SelectedItem == null)
						{
							if (items.Count > num && KickStarter.cursorManager.lookUseCursorAction != LookUseCursorAction.RightClickCyclesModes && AllowInteractions())
							{
								KickStarter.runtimeInventory.Look(items[num]);
							}
						}
						else
						{
							KickStarter.runtimeInventory.SetNull();
						}
						break;
					}
					break;
				}
			}
		}

		public override void RecalculateSize(MenuSource source)
		{
			if (inventoryBoxType == AC_InventoryBoxType.CollectedDocuments)
			{
				if (Application.isPlaying)
				{
					int[] collectedDocumentIDs = KickStarter.runtimeDocuments.GetCollectedDocumentIDs((!limitToCategory) ? null : categoryIDs.ToArray());
					numDocuments = collectedDocumentIDs.Length;
					numSlots = numDocuments;
					if (numSlots > maxSlots)
					{
						numSlots = maxSlots;
					}
					LimitOffset(numDocuments);
					labels = new string[numSlots];
					textures = new Texture[numSlots];
					int language = Options.GetLanguage();
					for (int i = 0; i < numSlots; i++)
					{
						int iD = collectedDocumentIDs[i + offset];
						Document document = KickStarter.inventoryManager.GetDocument(iD);
						labels[i] = KickStarter.runtimeLanguages.GetTranslation(document.title, document.titleLineID, language);
						textures[i] = document.texture;
					}
					if (uiHideStyle == UIHideStyle.DisableObject && numSlots > numDocuments)
					{
						offset = 0;
						numSlots = numDocuments;
					}
				}
			}
			else if (inventoryBoxType == AC_InventoryBoxType.Objectives)
			{
				if (Application.isPlaying)
				{
					ObjectiveInstance[] objectives = KickStarter.runtimeObjectives.GetObjectives(objectiveDisplayType);
					numDocuments = objectives.Length;
					numSlots = numDocuments;
					if (numSlots > maxSlots)
					{
						numSlots = maxSlots;
					}
					LimitOffset(numDocuments);
					labels = new string[numSlots];
					textures = new Texture[numSlots];
					int language2 = Options.GetLanguage();
					for (int j = 0; j < numSlots; j++)
					{
						labels[j] = objectives[j + offset].Objective.GetTitle(language2);
						textures[j] = objectives[j + offset].Objective.texture;
					}
					if (uiHideStyle == UIHideStyle.DisableObject && numSlots > numDocuments)
					{
						offset = 0;
						numSlots = numDocuments;
					}
				}
			}
			else
			{
				items = GetItemList();
				if (inventoryBoxType == AC_InventoryBoxType.HotspotBased)
				{
					if (Application.isPlaying)
					{
						numSlots = Mathf.Clamp(items.Count, 0, maxSlots);
					}
					else
					{
						numSlots = Mathf.Clamp(numSlots, 0, maxSlots);
					}
				}
				else
				{
					numSlots = maxSlots;
				}
				if (uiHideStyle == UIHideStyle.DisableObject && numSlots > items.Count)
				{
					offset = 0;
					numSlots = items.Count;
				}
				LimitOffset(items.Count);
				labels = new string[numSlots];
				if (Application.isPlaying && uiSlots != null)
				{
					ClearSpriteCache(uiSlots);
				}
			}
			if (!isVisible)
			{
				LimitUISlotVisibility(uiSlots, 0, uiHideStyle, emptySlotTexture);
			}
			base.RecalculateSize(source);
		}

		private List<InvItem> GetItemList(bool doLimit = true)
		{
			List<InvItem> list = new List<InvItem>();
			if (Application.isPlaying)
			{
				if (inventoryBoxType == AC_InventoryBoxType.HotspotBased)
				{
					list = ((!limitToDefinedInteractions && !ForceLimitByReference()) ? KickStarter.runtimeInventory.localItems : KickStarter.runtimeInventory.MatchInteractions());
				}
				else if (inventoryBoxType == AC_InventoryBoxType.DisplaySelected)
				{
					if (KickStarter.runtimeInventory.SelectedItem != null)
					{
						list.Add(KickStarter.runtimeInventory.SelectedItem);
					}
				}
				else if (inventoryBoxType == AC_InventoryBoxType.DisplayLastSelected)
				{
					if (KickStarter.runtimeInventory.LastSelectedItem != null && KickStarter.runtimeInventory.IsItemCarried(KickStarter.runtimeInventory.LastSelectedItem))
					{
						list.Add(KickStarter.runtimeInventory.LastSelectedItem);
					}
				}
				else if (inventoryBoxType == AC_InventoryBoxType.Container)
				{
					if (overrideContainer != null)
					{
						list = GetItemsFromContainer(overrideContainer);
					}
					else if (KickStarter.playerInput.activeContainer != null)
					{
						list = GetItemsFromContainer(KickStarter.playerInput.activeContainer);
					}
				}
				else
				{
					list = new List<InvItem>();
					foreach (InvItem localItem in KickStarter.runtimeInventory.localItems)
					{
						list.Add(localItem);
					}
				}
				list = AddExtraNulls(list);
			}
			else
			{
				list = new List<InvItem>();
				if ((bool)AdvGame.GetReferences().inventoryManager)
				{
					foreach (InvItem item in AdvGame.GetReferences().inventoryManager.items)
					{
						list.Add(item);
						if (item != null)
						{
							item.recipeSlot = -1;
						}
					}
				}
			}
			if (Application.isPlaying && (inventoryBoxType == AC_InventoryBoxType.Default || inventoryBoxType == AC_InventoryBoxType.CustomScript))
			{
				while (AreAnyItemsInRecipe(list))
				{
					foreach (InvItem item2 in list)
					{
						if (item2 != null && item2.recipeSlot > -1)
						{
							if (AdvGame.GetReferences().settingsManager.canReorderItems)
							{
								list[list.IndexOf(item2)] = null;
							}
							else
							{
								list.Remove(item2);
							}
							break;
						}
					}
				}
			}
			if (doLimit && CanBeLimitedByCategory())
			{
				list = LimitByCategory(list, 0).LimitedItems;
			}
			return list;
		}

		private List<InvItem> AddExtraNulls(List<InvItem> _items)
		{
			if (inventoryBoxType != AC_InventoryBoxType.DisplayLastSelected && inventoryBoxType != AC_InventoryBoxType.DisplaySelected && !limitMaxScroll && _items.Count > 0 && _items.Count % maxSlots != 0)
			{
				while (_items.Count % maxSlots != 0)
				{
					_items.Add(null);
				}
			}
			return _items;
		}

		private List<InvItem> GetItemsFromContainer(Container container)
		{
			List<InvItem> list = new List<InvItem>();
			list.Clear();
			foreach (ContainerItem item in container.items)
			{
				if (!item.IsEmpty)
				{
					InvItem invItem = new InvItem(KickStarter.inventoryManager.GetItem(item.linkedID));
					invItem.count = item.count;
					list.Add(invItem);
				}
				else if (KickStarter.settingsManager.canReorderItems)
				{
					list.Add(null);
				}
			}
			if (KickStarter.settingsManager.canReorderItems && list.Count < maxSlots - 1)
			{
				while (list.Count < maxSlots - 1)
				{
					list.Add(null);
				}
			}
			return list;
		}

		private bool CanBeLimitedByCategory()
		{
			if (inventoryBoxType == AC_InventoryBoxType.Default || inventoryBoxType == AC_InventoryBoxType.CustomScript || inventoryBoxType == AC_InventoryBoxType.DisplaySelected || inventoryBoxType == AC_InventoryBoxType.DisplayLastSelected || inventoryBoxType == AC_InventoryBoxType.CollectedDocuments)
			{
				return true;
			}
			if (inventoryBoxType == AC_InventoryBoxType.HotspotBased && !limitToDefinedInteractions && !ForceLimitByReference())
			{
				return true;
			}
			return false;
		}

		public override bool CanBeShifted(AC_ShiftInventory shiftType)
		{
			if (inventoryBoxType == AC_InventoryBoxType.CollectedDocuments || inventoryBoxType == AC_InventoryBoxType.Objectives)
			{
				if (numSlots == 0)
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
				else if (maxSlots + offset >= numDocuments)
				{
					return false;
				}
				return true;
			}
			if (items.Count == 0)
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
			else if (maxSlots + offset >= items.Count)
			{
				return false;
			}
			return true;
		}

		private bool AreAnyItemsInRecipe(List<InvItem> _itemList)
		{
			foreach (InvItem _item in _itemList)
			{
				if (_item != null && _item.recipeSlot >= 0)
				{
					return true;
				}
			}
			return false;
		}

		private LimitedItemList LimitByCategory(List<InvItem> itemsToLimit, int reverseItemIndex)
		{
			int num = 0;
			List<InvItem> list = new List<InvItem>();
			foreach (InvItem item in itemsToLimit)
			{
				list.Add(item);
			}
			if (limitToCategory && categoryIDs.Count > 0)
			{
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i] != null && !categoryIDs.Contains(list[i].binID))
					{
						if (i <= reverseItemIndex)
						{
							num++;
						}
						list.RemoveAt(i);
						i = -1;
					}
				}
				if (list != null && Application.isPlaying)
				{
					list = KickStarter.runtimeInventory.RemoveEmptySlots(list);
				}
				list = AddExtraNulls(list);
			}
			return new LimitedItemList(list, num);
		}

		public override void Shift(AC_ShiftInventory shiftType, int amount)
		{
			if (numSlots >= maxSlots)
			{
				if (inventoryBoxType == AC_InventoryBoxType.CollectedDocuments || inventoryBoxType == AC_InventoryBoxType.Objectives)
				{
					Shift(shiftType, maxSlots, numDocuments, amount);
				}
				else
				{
					Shift(shiftType, maxSlots, items.Count, amount);
				}
			}
		}

		private Texture GetTexture(int itemIndex, bool isActive)
		{
			if (ItemIsSelected(itemIndex))
			{
				switch (KickStarter.settingsManager.selectInventoryDisplay)
				{
				case SelectInventoryDisplay.ShowSelectedGraphic:
					return items[itemIndex].selectedTex;
				case SelectInventoryDisplay.ShowHoverGraphic:
					return items[itemIndex].activeTex;
				}
			}
			else if (isActive && KickStarter.settingsManager.activeWhenHover)
			{
				return items[itemIndex].activeTex;
			}
			return items[itemIndex].tex;
		}

		private void DrawTexture(Rect rect, int itemIndex, bool isActive)
		{
			InvItem invItem = items[itemIndex];
			if (invItem == null)
			{
				return;
			}
			Texture texture = null;
			if (Application.isPlaying && KickStarter.runtimeInventory != null && inventoryBoxType != AC_InventoryBoxType.DisplaySelected)
			{
				if (invItem == KickStarter.runtimeInventory.highlightItem && invItem.activeTex != null)
				{
					KickStarter.runtimeInventory.DrawHighlighted(rect);
					return;
				}
				if (inventoryBoxType != AC_InventoryBoxType.DisplaySelected && inventoryBoxType != AC_InventoryBoxType.DisplayLastSelected)
				{
					texture = GetTexture(itemIndex, isActive);
				}
				if (texture == null)
				{
					texture = invItem.tex;
				}
			}
			else if (invItem.tex != null)
			{
				texture = invItem.tex;
			}
			if (texture != null)
			{
				GUI.DrawTexture(rect, texture, ScaleMode.StretchToFill, true, 0f);
			}
		}

		public override string GetLabel(int i, int languageNumber)
		{
			if (inventoryBoxType == AC_InventoryBoxType.CollectedDocuments || inventoryBoxType == AC_InventoryBoxType.Objectives)
			{
				if (labels.Length > i)
				{
					return labels[i];
				}
				return string.Empty;
			}
			if (items.Count <= i + offset || items[i + offset] == null)
			{
				return string.Empty;
			}
			return items[i + offset].GetLabel(languageNumber);
		}

		public override bool IsSelectedByEventSystem(int slotIndex)
		{
			if (uiSlots != null && slotIndex >= 0 && uiSlots.Length > slotIndex && uiSlots[slotIndex] != null && uiSlots[slotIndex].uiButton != null)
			{
				return KickStarter.playerMenus.IsEventSystemSelectingObject(uiSlots[slotIndex].uiButton.gameObject);
			}
			return false;
		}

		public override AudioClip GetHoverSound(int slot)
		{
			if (!hoverSoundOverEmptySlots && GetItem(slot) == null)
			{
				return null;
			}
			return base.GetHoverSound(slot);
		}

		public InvItem GetItem(int i)
		{
			if (items.Count <= i + offset || items[i + offset] == null)
			{
				return null;
			}
			return items[i + offset];
		}

		private string GetCount(int i)
		{
			if (inventoryItemCountDisplay == InventoryItemCountDisplay.Never)
			{
				return string.Empty;
			}
			if (Application.isPlaying)
			{
				if (items.Count <= i + offset || items[i + offset] == null)
				{
					return string.Empty;
				}
				if (items[i + offset].count < 2 && inventoryItemCountDisplay == InventoryItemCountDisplay.OnlyIfMultiple)
				{
					return string.Empty;
				}
				if (ItemIsSelected(i + offset) && items[i + offset].CanSelectSingle())
				{
					return (items[i + offset].count - 1).ToString();
				}
				return items[i + offset].count.ToString();
			}
			if (items[i + offset].canCarryMultiple && !items[i + offset].useSeparateSlots && (items[i + offset].count > 1 || inventoryItemCountDisplay == InventoryItemCountDisplay.Always))
			{
				return items[i + offset].count.ToString();
			}
			return string.Empty;
		}

		public void ResetOffset()
		{
			offset = 0;
		}

		protected override void AutoSize()
		{
			if (inventoryBoxType == AC_InventoryBoxType.CollectedDocuments || inventoryBoxType == AC_InventoryBoxType.Objectives)
			{
				if (!Application.isPlaying)
				{
				}
				if (numDocuments > 0)
				{
					if (displayType == ConversationDisplayType.IconOnly)
					{
						AutoSize(new GUIContent(textures[0]));
					}
					else
					{
						AutoSize(new GUIContent(labels[0]));
					}
					return;
				}
			}
			else if (items.Count > 0)
			{
				foreach (InvItem item in items)
				{
					if (item != null)
					{
						if (displayType == ConversationDisplayType.IconOnly)
						{
							AutoSize(new GUIContent(item.tex));
						}
						else if (displayType == ConversationDisplayType.TextOnly)
						{
							AutoSize(new GUIContent(item.label));
						}
						return;
					}
				}
			}
			AutoSize(GUIContent.none);
		}

		public void ClickContainer(MouseState _mouseState, int _slot)
		{
			Container container = ((!(overrideContainer != null)) ? KickStarter.playerInput.activeContainer : overrideContainer);
			if (container == null || KickStarter.runtimeInventory == null)
			{
				return;
			}
			KickStarter.runtimeInventory.SetFont(font, GetFontSize(), fontColor, textEffects);
			switch (_mouseState)
			{
			case MouseState.SingleClick:
				if (KickStarter.runtimeInventory.SelectedItem == null)
				{
					if (container.items.Count <= _slot + offset || container.items[_slot + offset].IsEmpty)
					{
						break;
					}
					ContainerItem containerItem = container.items[_slot + offset];
					ContainerItem containerItem2 = new ContainerItem(containerItem);
					InvItem item = KickStarter.inventoryManager.GetItem(containerItem.linkedID);
					if (KickStarter.runtimeInventory.IsCarryingItem(item.id) && !item.canCarryMultiple)
					{
						KickStarter.eventManager.Call_OnUseContainerFail(container, containerItem);
					}
					else if (containerSelectMode == ContainerSelectMode.MoveToInventory || containerSelectMode == ContainerSelectMode.MoveToInventoryAndSelect)
					{
						if (KickStarter.runtimeInventory.CanTransferContainerItemsToInventory(containerItem))
						{
							bool selectAfter = containerSelectMode == ContainerSelectMode.MoveToInventoryAndSelect;
							if (KickStarter.inventoryManager.GetItem(containerItem.linkedID).CanSelectSingle(containerItem.count))
							{
								KickStarter.runtimeInventory.Add(containerItem.linkedID, 1, selectAfter);
								container.items[_slot + offset].count--;
							}
							else
							{
								KickStarter.runtimeInventory.Add(containerItem.linkedID, containerItem.count, selectAfter);
								container.Remove(containerItem);
							}
							KickStarter.eventManager.Call_OnUseContainer(false, container, containerItem2);
						}
					}
					else if (containerSelectMode == ContainerSelectMode.SelectItemOnly)
					{
						KickStarter.runtimeInventory.SelectItem(container, containerItem);
					}
				}
				else if (container.maxSlots > 0 && container.FilledSlots >= container.maxSlots)
				{
					KickStarter.runtimeInventory.SetNull();
				}
				else if (KickStarter.runtimeInventory.selectedContainerItem != null)
				{
					int index = KickStarter.runtimeInventory.selectedContainerItemContainer.items.IndexOf(KickStarter.runtimeInventory.selectedContainerItem);
					if (KickStarter.settingsManager.canReorderItems)
					{
						KickStarter.runtimeInventory.selectedContainerItemContainer.items[index].IsEmpty = true;
					}
					else
					{
						KickStarter.runtimeInventory.selectedContainerItemContainer.Remove(KickStarter.runtimeInventory.selectedContainerItem);
					}
					container.InsertAt(KickStarter.runtimeInventory.SelectedItem, _slot + offset, KickStarter.runtimeInventory.selectedContainerItem.count);
					if (KickStarter.runtimeInventory.selectedContainerItemContainer != container)
					{
						KickStarter.eventManager.Call_OnUseContainer(true, container, KickStarter.runtimeInventory.selectedContainerItem);
					}
					KickStarter.runtimeInventory.SetNull();
				}
				else
				{
					int num = (KickStarter.runtimeInventory.SelectedItem.CanSelectSingle() ? 1 : 0);
					ContainerItem containerItem3 = container.InsertAt(KickStarter.runtimeInventory.SelectedItem, _slot + offset, num);
					if (!containerItem3.IsEmpty)
					{
						KickStarter.runtimeInventory.Remove(KickStarter.runtimeInventory.SelectedItem, num);
						KickStarter.eventManager.Call_OnUseContainer(true, container, containerItem3);
					}
				}
				break;
			case MouseState.RightClick:
				if (KickStarter.runtimeInventory.SelectedItem != null)
				{
					KickStarter.runtimeInventory.SetNull();
				}
				break;
			}
		}

		public override void ProcessClick(Menu _menu, int _slot, MouseState _mouseState)
		{
			if (KickStarter.stateHandler.gameState == GameState.Cutscene)
			{
				return;
			}
			if (_mouseState == MouseState.SingleClick)
			{
				KickStarter.runtimeInventory.lastClickedItem = GetItem(_slot);
			}
			switch (inventoryBoxType)
			{
			case AC_InventoryBoxType.CollectedDocuments:
				if (autoOpenDocument)
				{
					Document document = GetDocument(_slot);
					KickStarter.runtimeDocuments.OpenDocument(document);
				}
				if (actionListOnClick != null)
				{
					actionListOnClick.Interact();
				}
				break;
			case AC_InventoryBoxType.CustomScript:
				MenuSystem.OnElementClick(_menu, this, _slot, (int)_mouseState);
				break;
			case AC_InventoryBoxType.Objectives:
				if (autoOpenDocument)
				{
					ObjectiveInstance selectedObjective = KickStarter.runtimeObjectives.GetObjectives(objectiveDisplayType)[_slot + offset];
					KickStarter.runtimeObjectives.SelectedObjective = selectedObjective;
				}
				if (actionListOnClick != null)
				{
					actionListOnClick.Interact();
				}
				break;
			default:
				KickStarter.runtimeInventory.ProcessInventoryBoxClick(_menu, this, _slot, _mouseState);
				break;
			}
			base.ProcessClick(_menu, _slot, _mouseState);
		}

		public Document GetDocument(int slotIndex)
		{
			if (inventoryBoxType == AC_InventoryBoxType.CollectedDocuments)
			{
				int iD = KickStarter.runtimeDocuments.GetCollectedDocumentIDs((!limitToCategory) ? null : categoryIDs.ToArray())[slotIndex + offset];
				return KickStarter.inventoryManager.GetDocument(iD);
			}
			return null;
		}

		public ObjectiveInstance GetObjective(int slotIndex)
		{
			if (inventoryBoxType == AC_InventoryBoxType.Objectives)
			{
				ObjectiveInstance[] objectives = KickStarter.runtimeObjectives.GetObjectives(objectiveDisplayType);
				return objectives[slotIndex + offset];
			}
			return null;
		}

		public int GetItemSlot(int itemID)
		{
			foreach (InvItem item in items)
			{
				if (item != null && item.id == itemID)
				{
					return items.IndexOf(item) - offset;
				}
			}
			return 0;
		}

		private bool ForceLimitByReference()
		{
			if (KickStarter.settingsManager != null && KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction && KickStarter.settingsManager.cycleInventoryCursors && (KickStarter.settingsManager.selectInteractions == SelectInteractions.CyclingCursorAndClickingHotspot || KickStarter.settingsManager.selectInteractions == SelectInteractions.CyclingMenuAndClickingHotspot))
			{
				return true;
			}
			return false;
		}
	}
}
