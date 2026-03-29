using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_runtime_inventory.html")]
	public class RuntimeInventory : MonoBehaviour
	{
		protected List<InvItem> _localItems = new List<InvItem>();

		[HideInInspector]
		public List<InvItem> craftingItems = new List<InvItem>();

		[HideInInspector]
		public ActionListAsset unhandledCombine;

		[HideInInspector]
		public ActionListAsset unhandledHotspot;

		[HideInInspector]
		public ActionListAsset unhandledGive;

		protected InvItem selectedItem;

		protected InvItem lastSelectedItem;

		[HideInInspector]
		public InvItem hoverItem;

		[HideInInspector]
		public InvItem highlightItem;

		[HideInInspector]
		public bool showHoverLabel = true;

		[HideInInspector]
		public List<int> matchingInvInteractions = new List<int>();

		protected List<SelectItemMode> matchingItemModes = new List<SelectItemMode>();

		[HideInInspector]
		public InvItem lastClickedItem;

		protected SelectItemMode selectItemMode;

		protected GUIStyle countStyle;

		protected TextEffects countTextEffects;

		protected HighlightState highlightState;

		protected float pulse;

		protected int pulseDirection;

		protected string prefix1 = string.Empty;

		protected string prefix2 = string.Empty;

		public ContainerItem selectedContainerItem { get; protected set; }

		public Container selectedContainerItemContainer { get; protected set; }

		public InvItem SelectedItem
		{
			get
			{
				return selectedItem;
			}
		}

		public List<InvItem> localItems
		{
			get
			{
				return _localItems;
			}
		}

		public InvItem LastSelectedItem
		{
			get
			{
				return lastSelectedItem;
			}
		}

		protected void OnApplicationQuit()
		{
			if (!(KickStarter.inventoryManager != null))
			{
				return;
			}
			foreach (InvItem item in KickStarter.inventoryManager.items)
			{
				if (item.cursorIcon != null)
				{
					item.cursorIcon.ClearCache();
				}
			}
		}

		public void OnStart()
		{
			SetNull();
			hoverItem = null;
			showHoverLabel = true;
			craftingItems.Clear();
			_localItems.Clear();
			GetItemsOnStart();
			if ((bool)KickStarter.inventoryManager)
			{
				unhandledCombine = KickStarter.inventoryManager.unhandledCombine;
				unhandledHotspot = KickStarter.inventoryManager.unhandledHotspot;
				unhandledGive = KickStarter.inventoryManager.unhandledGive;
			}
		}

		public void AfterLoad()
		{
			if (!KickStarter.settingsManager.IsInLoadingScene() && KickStarter.sceneSettings != null)
			{
				SetNull();
				lastSelectedItem = null;
			}
		}

		public void SetNull()
		{
			if (selectedItem != null && _localItems.Contains(selectedItem))
			{
				KickStarter.eventManager.Call_OnChangeInventory(selectedItem, InventoryEventType.Deselect);
			}
			selectedItem = null;
			highlightItem = null;
			lastClickedItem = null;
			selectedContainerItem = null;
			selectedContainerItemContainer = null;
			PlayerMenus.ResetInventoryBoxes();
		}

		public void SelectItemByID(int _id, SelectItemMode _mode = SelectItemMode.Use, bool ignoreInventory = false)
		{
			if (_id == -1)
			{
				SetNull();
				return;
			}
			if (ignoreInventory)
			{
				foreach (InvItem item in KickStarter.inventoryManager.items)
				{
					if (item != null && item.id == _id)
					{
						SetSelectItemMode(_mode);
						lastSelectedItem = (selectedItem = new InvItem(item));
						PlayerMenus.ResetInventoryBoxes();
						KickStarter.eventManager.Call_OnChangeInventory(selectedItem, InventoryEventType.Select);
						break;
					}
				}
				return;
			}
			foreach (InvItem localItem in _localItems)
			{
				if (localItem != null && localItem.id == _id)
				{
					SetSelectItemMode(_mode);
					lastSelectedItem = (selectedItem = localItem);
					selectedContainerItem = null;
					selectedContainerItemContainer = null;
					PlayerMenus.ResetInventoryBoxes();
					KickStarter.eventManager.Call_OnChangeInventory(selectedItem, InventoryEventType.Select);
					return;
				}
			}
			SetNull();
			ACDebug.LogWarning("Want to select inventory item " + KickStarter.inventoryManager.GetLabel(_id) + " but player is not carrying it.");
		}

		public void ReselectLastItem()
		{
			if (lastSelectedItem != null && localItems.Contains(lastSelectedItem))
			{
				SelectItem(lastSelectedItem, selectItemMode);
			}
		}

		public void SelectItem(InvItem item, SelectItemMode _mode = SelectItemMode.Use)
		{
			if (item == null)
			{
				SetNull();
				return;
			}
			if (selectedItem == item)
			{
				SetNull();
				KickStarter.playerCursor.ResetSelectedCursor();
				return;
			}
			SetSelectItemMode(_mode);
			lastSelectedItem = (selectedItem = item);
			selectedContainerItem = null;
			selectedContainerItemContainer = null;
			KickStarter.eventManager.Call_OnChangeInventory(selectedItem, InventoryEventType.Select);
			PlayerMenus.ResetInventoryBoxes();
		}

		public void SelectItem(Container container, ContainerItem containerItem)
		{
			SetNull();
			SelectItemByID(containerItem.linkedID, SelectItemMode.Use, true);
			selectedContainerItemContainer = container;
			selectedContainerItem = containerItem;
			selectedItem.count = containerItem.count;
		}

		public void UpdateSelectItemModeForMenu(MenuInventoryBox inventoryBox, int slotIndex)
		{
			int num = slotIndex + inventoryBox.GetOffset();
			if (selectedItem == null && matchingItemModes != null && num < matchingItemModes.Count)
			{
				SetSelectItemMode(matchingItemModes[num]);
			}
		}

		public void SetSelectItemMode(SelectItemMode _mode)
		{
			if (_mode == SelectItemMode.Give && KickStarter.settingsManager.CanGiveItems())
			{
				selectItemMode = SelectItemMode.Give;
			}
			else
			{
				selectItemMode = SelectItemMode.Use;
			}
		}

		public bool IsGivingItem()
		{
			return selectItemMode == SelectItemMode.Give;
		}

		public void Replace(int _addID, int _removeID, int addAmount = 1)
		{
			int num = -1;
			InvItem invItem = null;
			foreach (InvItem localItem in _localItems)
			{
				if (localItem != null)
				{
					if (localItem.id == _removeID && num == -1)
					{
						num = _localItems.IndexOf(localItem);
						invItem = localItem;
					}
					if (localItem.id == _addID)
					{
						return;
					}
				}
			}
			if (num == -1)
			{
				Add(_addID, addAmount);
				return;
			}
			if (invItem != null)
			{
				KickStarter.eventManager.Call_OnChangeInventory(invItem, InventoryEventType.Remove, invItem.count);
			}
			foreach (InvItem item in KickStarter.inventoryManager.items)
			{
				if (item.id == _addID)
				{
					InvItem invItem2 = new InvItem(item);
					if (!invItem2.canCarryMultiple)
					{
						addAmount = 1;
					}
					invItem2.count = addAmount;
					_localItems[num] = invItem2;
					PlayerMenus.ResetInventoryBoxes();
					KickStarter.eventManager.Call_OnChangeInventory(invItem2, InventoryEventType.Add, addAmount);
					break;
				}
			}
		}

		public void Add(string _name, int amount = 1, bool selectAfter = false, int playerID = -1, bool addToFront = false)
		{
			InvItem item = KickStarter.inventoryManager.GetItem(_name);
			if (item != null)
			{
				Add(item.id, amount, selectAfter, playerID, addToFront);
			}
		}

		public void Add(int _id, int amount = 1, bool selectAfter = false, int playerID = -1, bool addToFront = false)
		{
			if (playerID >= 0 && KickStarter.player.ID != playerID)
			{
				AddToOtherPlayer(_id, amount, playerID, addToFront);
				return;
			}
			_localItems = Add(_id, amount, _localItems, selectAfter, addToFront);
			KickStarter.eventManager.Call_OnChangeInventory(GetItem(_id), InventoryEventType.Add, amount);
		}

		public List<InvItem> Add(int _id, int amount, List<InvItem> itemList, bool selectAfter, bool addToFront = false)
		{
			itemList = ReorderItems(itemList);
			foreach (InvItem item in itemList)
			{
				if (item == null || item.id != _id)
				{
					continue;
				}
				if (item.canCarryMultiple)
				{
					if (item.useSeparateSlots)
					{
						break;
					}
					item.count += amount;
				}
				if (selectAfter)
				{
					SelectItem(item);
				}
				PlayerMenus.ResetInventoryBoxes();
				return itemList;
			}
			foreach (InvItem item2 in KickStarter.inventoryManager.items)
			{
				if (item2.id != _id)
				{
					continue;
				}
				InvItem invItem = new InvItem(item2);
				if (!invItem.canCarryMultiple)
				{
					amount = 1;
				}
				invItem.recipeSlot = -1;
				invItem.count = amount;
				if (KickStarter.settingsManager.canReorderItems)
				{
					if (addToFront && itemList.Count > 0 && itemList[0] != null)
					{
						itemList.Insert(0, invItem);
						if (invItem.canCarryMultiple && invItem.useSeparateSlots)
						{
							int num = invItem.count - 1;
							invItem.count = 1;
							for (int i = 0; i < num; i++)
							{
								itemList.Insert(0, invItem);
							}
						}
						PlayerMenus.ResetInventoryBoxes();
						return itemList;
					}
					for (int j = 0; j < itemList.Count; j++)
					{
						if (itemList[j] != null)
						{
							continue;
						}
						itemList[j] = invItem;
						if (selectAfter)
						{
							SelectItem(invItem);
						}
						if (invItem.canCarryMultiple && invItem.useSeparateSlots)
						{
							int num2 = invItem.count - 1;
							invItem.count = 1;
							for (int k = 0; k < num2; k++)
							{
								itemList.Add(invItem);
							}
						}
						PlayerMenus.ResetInventoryBoxes();
						return itemList;
					}
				}
				if (invItem.canCarryMultiple && invItem.useSeparateSlots)
				{
					int count = invItem.count;
					invItem.count = 1;
					for (int l = 0; l < count; l++)
					{
						if (addToFront)
						{
							itemList.Insert(0, invItem);
						}
						else
						{
							itemList.Add(invItem);
						}
					}
				}
				else if (addToFront)
				{
					itemList.Insert(0, invItem);
				}
				else
				{
					itemList.Add(invItem);
				}
				if (selectAfter)
				{
					SelectItem(invItem);
				}
				PlayerMenus.ResetInventoryBoxes();
				return itemList;
			}
			ACDebug.LogWarning("Cannot add inventory with ID=" + _id + ", because it cannot be found in the Inventory Manager.");
			itemList = RemoveEmptySlots(itemList);
			PlayerMenus.ResetInventoryBoxes();
			return itemList;
		}

		public void Remove(int _id)
		{
			int count = GetCount(_id);
			if (count > 0)
			{
				_localItems = Remove(_id, count, false, _localItems);
				KickStarter.eventManager.Call_OnChangeInventory(GetItem(_id), InventoryEventType.Remove, count);
			}
		}

		public void Remove(int _id, int amount)
		{
			int count = GetCount(_id);
			if (count > 0)
			{
				_localItems = Remove(_id, amount, true, _localItems);
				KickStarter.eventManager.Call_OnChangeInventory(GetItem(_id), InventoryEventType.Remove, amount);
			}
		}

		public void RemoveFromOtherPlayer(int _id, int playerID)
		{
			if (playerID >= 0 && KickStarter.player.ID != playerID)
			{
				RemoveFromOtherPlayer(_id, 1, false, playerID);
			}
			else
			{
				Remove(_id);
			}
		}

		public void RemoveFromOtherPlayer(int _id, int amount, int playerID)
		{
			if (playerID >= 0 && KickStarter.player.ID != playerID)
			{
				RemoveFromOtherPlayer(_id, amount, true, playerID);
			}
			else
			{
				Remove(_id, amount);
			}
		}

		public void Remove(InvItem _item, int amount = 0)
		{
			if (_item != null && _localItems.Contains(_item))
			{
				if (_item == selectedItem)
				{
					SetNull();
				}
				if (amount > 0 && _item.canCarryMultiple && _item.count > amount)
				{
					_item.count -= amount;
				}
				else
				{
					_localItems[_localItems.IndexOf(_item)] = null;
					_localItems = ReorderItems(_localItems);
					_localItems = RemoveEmptySlots(_localItems);
					KickStarter.eventManager.Call_OnChangeInventory(_item, InventoryEventType.Remove);
				}
				PlayerMenus.ResetInventoryBoxes();
			}
		}

		public void Remove(string _name, int amount = 0)
		{
			InvItem item = KickStarter.inventoryManager.GetItem(_name);
			if (item != null)
			{
				if (amount > 0)
				{
					Remove(item.id, amount);
				}
				else
				{
					Remove(item.id, amount);
				}
			}
		}

		public void RemoveAll()
		{
			foreach (InvItem localItem in _localItems)
			{
				Remove(localItem);
			}
		}

		public void RemoveAllInCategory(int categoryID)
		{
			for (int i = 0; i < _localItems.Count; i++)
			{
				if (_localItems[i].binID == categoryID)
				{
					Remove(_localItems[i]);
					i = -1;
				}
			}
		}

		public string GetHotspotPrefixLabel(InvItem item, string itemName, int languageNumber, bool canGive = false)
		{
			prefix1 = string.Empty;
			prefix2 = string.Empty;
			if (canGive && IsGivingItem())
			{
				prefix1 = KickStarter.runtimeLanguages.GetTranslation(KickStarter.cursorManager.hotspotPrefix3.label, KickStarter.cursorManager.hotspotPrefix3.lineID, languageNumber);
				prefix2 = KickStarter.runtimeLanguages.GetTranslation(KickStarter.cursorManager.hotspotPrefix4.label, KickStarter.cursorManager.hotspotPrefix4.lineID, languageNumber);
			}
			else if (item != null && item.overrideUseSyntax)
			{
				prefix1 = KickStarter.runtimeLanguages.GetTranslation(item.hotspotPrefix1.label, item.hotspotPrefix1.lineID, languageNumber);
				prefix2 = KickStarter.runtimeLanguages.GetTranslation(item.hotspotPrefix2.label, item.hotspotPrefix2.lineID, languageNumber);
			}
			else
			{
				prefix1 = KickStarter.runtimeLanguages.GetTranslation(KickStarter.cursorManager.hotspotPrefix1.label, KickStarter.cursorManager.hotspotPrefix1.lineID, languageNumber);
				prefix2 = KickStarter.runtimeLanguages.GetTranslation(KickStarter.cursorManager.hotspotPrefix2.label, KickStarter.cursorManager.hotspotPrefix2.lineID, languageNumber);
			}
			if (string.IsNullOrEmpty(prefix1) && !string.IsNullOrEmpty(prefix2))
			{
				return prefix2;
			}
			if (!string.IsNullOrEmpty(prefix1) && string.IsNullOrEmpty(prefix2))
			{
				return AdvGame.CombineLanguageString(prefix1, itemName, languageNumber);
			}
			if (prefix1 == " " && !string.IsNullOrEmpty(prefix2))
			{
				return AdvGame.CombineLanguageString(itemName, prefix2, languageNumber);
			}
			if (KickStarter.runtimeLanguages.LanguageReadsRightToLeft(languageNumber))
			{
				return prefix2 + " " + itemName + " " + prefix1;
			}
			return prefix1 + " " + itemName + " " + prefix2;
		}

		public List<InvItem> RemoveEmptySlots(List<InvItem> itemList)
		{
			int num = itemList.Count - 1;
			while (num >= 0)
			{
				if (itemList[num] == null)
				{
					itemList.RemoveAt(num);
					num--;
					continue;
				}
				return itemList;
			}
			return itemList;
		}

		public string GetLabel(InvItem item, int languageNumber)
		{
			return item.GetLabel(languageNumber);
		}

		public int GetCount(int _invID)
		{
			int num = 0;
			for (int i = 0; i < _localItems.Count; i++)
			{
				if (_localItems[i] != null && _localItems[i].id == _invID)
				{
					num += _localItems[i].count;
				}
			}
			return num;
		}

		public int GetCount(int _invID, int _playerID)
		{
			List<InvItem> itemsFromPlayer = GetComponent<SaveSystem>().GetItemsFromPlayer(_playerID);
			int num = 0;
			if (itemsFromPlayer != null)
			{
				foreach (InvItem item in itemsFromPlayer)
				{
					if (item != null && item.id == _invID)
					{
						num += item.count;
					}
				}
			}
			return num;
		}

		public int GetNumberOfItemsCarried(bool includeMultipleInSameSlot = false)
		{
			return GetNumberOfItemsCarriedInCategory(-1, includeMultipleInSameSlot);
		}

		public int GetNumberOfItemsCarried(int _playerID, bool includeMultipleInSameSlot = false)
		{
			return GetNumberOfItemsCarriedInCategory(-1, _playerID, includeMultipleInSameSlot);
		}

		public int GetNumberOfItemsCarriedInCategory(int categoryID, bool includeMultipleInSameSlot = false)
		{
			int num = 0;
			for (int i = 0; i < _localItems.Count; i++)
			{
				if (_localItems[i] != null && (categoryID < 0 || _localItems[i].binID == categoryID))
				{
					num = ((!includeMultipleInSameSlot || !_localItems[i].canCarryMultiple) ? (num + 1) : (num + _localItems[i].count));
				}
			}
			return num;
		}

		public int GetNumberOfItemsCarriedInCategory(int categoryID, int _playerID, bool includeMultipleInSameSlot = false)
		{
			int num = 0;
			List<InvItem> itemsFromPlayer = GetComponent<SaveSystem>().GetItemsFromPlayer(_playerID);
			if (itemsFromPlayer != null)
			{
				for (int i = 0; i < itemsFromPlayer.Count; i++)
				{
					if (itemsFromPlayer[i] != null)
					{
						num = ((!includeMultipleInSameSlot || !itemsFromPlayer[i].canCarryMultiple) ? (num + 1) : (num + itemsFromPlayer[i].count));
					}
				}
			}
			return num;
		}

		public InvItem GetCraftingItem(int _id)
		{
			foreach (InvItem craftingItem in craftingItems)
			{
				if (craftingItem.id == _id)
				{
					return craftingItem;
				}
			}
			return null;
		}

		public InvItem GetItem(int _id)
		{
			foreach (InvItem localItem in _localItems)
			{
				if (localItem != null && localItem.id == _id)
				{
					return localItem;
				}
			}
			return null;
		}

		public InvItem GetItem(string _name)
		{
			foreach (InvItem localItem in _localItems)
			{
				if (localItem.label == _name)
				{
					return localItem;
				}
			}
			return null;
		}

		public InvItem[] GetItems(int _id)
		{
			List<InvItem> list = new List<InvItem>();
			foreach (InvItem localItem in _localItems)
			{
				if (localItem.id == _id)
				{
					list.Add(localItem);
				}
			}
			return list.ToArray();
		}

		public InvItem[] GetItems(string _name)
		{
			List<InvItem> list = new List<InvItem>();
			foreach (InvItem localItem in _localItems)
			{
				if (localItem.label == _name)
				{
					list.Add(localItem);
				}
			}
			return list.ToArray();
		}

		public bool IsCarryingItem(int _id)
		{
			foreach (InvItem localItem in _localItems)
			{
				if (localItem != null && localItem.id == _id)
				{
					return true;
				}
			}
			return false;
		}

		public void Look(InvItem item)
		{
			if (item != null && item.recipeSlot <= -1 && (bool)item.lookActionList)
			{
				KickStarter.eventManager.Call_OnUseInventory(item, KickStarter.cursorManager.lookCursor_ID);
				AdvGame.RunActionListAsset(item.lookActionList);
			}
		}

		public void Use(InvItem item)
		{
			if (item != null && item.recipeSlot <= -1)
			{
				if ((bool)item.useActionList)
				{
					SetNull();
					KickStarter.eventManager.Call_OnUseInventory(item, 0);
					AdvGame.RunActionListAsset(item.useActionList);
				}
				else if (KickStarter.settingsManager.CanSelectItems(true))
				{
					SelectItem(item);
				}
			}
		}

		public void RunInteraction(InvItem invItem, int iconID)
		{
			if ((KickStarter.stateHandler.gameState == GameState.DialogOptions && !KickStarter.settingsManager.allowInventoryInteractionsDuringConversations && !KickStarter.settingsManager.allowGameplayDuringConversations) || invItem == null || invItem.recipeSlot > -1)
			{
				return;
			}
			foreach (InvInteraction interaction in invItem.interactions)
			{
				if (interaction.icon.id == iconID)
				{
					if ((bool)interaction.actionList)
					{
						KickStarter.eventManager.Call_OnUseInventory(invItem, iconID);
						AdvGame.RunActionListAsset(interaction.actionList);
						return;
					}
					break;
				}
			}
			if (KickStarter.settingsManager.interactionMethod != AC_InteractionMethod.ContextSensitive && KickStarter.settingsManager.inventoryInteractions == InventoryInteractions.Multiple && KickStarter.settingsManager.CanSelectItems(false))
			{
				if (KickStarter.settingsManager.selectInvWithUnhandled && iconID == KickStarter.settingsManager.selectInvWithIconID)
				{
					SelectItem(invItem);
					return;
				}
				if (KickStarter.settingsManager.giveInvWithUnhandled && iconID == KickStarter.settingsManager.giveInvWithIconID)
				{
					SelectItem(invItem, SelectItemMode.Give);
					return;
				}
			}
			KickStarter.eventManager.Call_OnUseInventory(invItem, iconID);
			AdvGame.RunActionListAsset(KickStarter.cursorManager.GetUnhandledInteraction(iconID));
		}

		public void RunInteraction(int iconID, InvItem clickedItem = null)
		{
			if (clickedItem != null)
			{
				hoverItem = clickedItem;
			}
			RunInteraction(hoverItem, iconID);
		}

		public void ShowInteractions(InvItem item)
		{
			hoverItem = item;
			if (KickStarter.settingsManager.SeeInteractions != SeeInteractions.ViaScriptOnly)
			{
				KickStarter.playerMenus.EnableInteractionMenus(item);
			}
		}

		public void SetHoverItem(InvItem item, MenuInventoryBox menuInventoryBox)
		{
			hoverItem = item;
			if (menuInventoryBox.displayType == ConversationDisplayType.IconOnly)
			{
				if (menuInventoryBox.inventoryBoxType == AC_InventoryBoxType.Container && selectedItem != null)
				{
					showHoverLabel = false;
				}
				else
				{
					showHoverLabel = true;
				}
			}
			else
			{
				showHoverLabel = menuInventoryBox.updateHotspotLabelWhenHover;
			}
		}

		public void SetHoverItem(InvItem item, MenuCrafting menuCrafting)
		{
			hoverItem = item;
			if (menuCrafting.displayType == ConversationDisplayType.IconOnly)
			{
				showHoverLabel = true;
			}
			else
			{
				showHoverLabel = false;
			}
		}

		public void Combine(InvItem item1, int item2ID)
		{
			Combine(item1, GetItem(item2ID));
		}

		public void Combine(InvItem item1, InvItem item2, bool allowSelfCombining = false)
		{
			if (item2 == null || item1 == null || item2.recipeSlot > -1)
			{
				return;
			}
			if (item2 == item1 && !allowSelfCombining)
			{
				if ((KickStarter.settingsManager.interactionMethod != AC_InteractionMethod.ChooseHotspotThenInteraction || KickStarter.settingsManager.inventoryInteractions == InventoryInteractions.Single) && KickStarter.settingsManager.InventoryDragDrop && KickStarter.settingsManager.inventoryDropLook)
				{
					Look(item2);
				}
				SetNull();
				KickStarter.eventManager.Call_OnUseInventory(item1, 0, item2);
			}
			else
			{
				if (selectedItem == null)
				{
					InvItem invItem = item1;
					item1 = item2;
					item2 = invItem;
				}
				KickStarter.eventManager.Call_OnUseInventory(item1, 0, item2);
				for (int i = 0; i < item2.combineID.Count; i++)
				{
					if (item2.combineID[i] == item1.id && item2.combineActionList[i] != null)
					{
						if (KickStarter.settingsManager.inventoryDisableDefined)
						{
							selectedItem = null;
						}
						AdvGame.RunActionListAsset(item2.combineActionList[i]);
						return;
					}
				}
				if (KickStarter.settingsManager.reverseInventoryCombinations || (KickStarter.settingsManager.SelectInteractionMethod() == SelectInteractions.CyclingCursorAndClickingHotspot && KickStarter.settingsManager.inventoryInteractions == InventoryInteractions.Multiple))
				{
					for (int j = 0; j < item1.combineID.Count; j++)
					{
						if (item1.combineID[j] == item2.id && item1.combineActionList[j] != null)
						{
							if (KickStarter.settingsManager.inventoryDisableDefined)
							{
								selectedItem = null;
							}
							ActionListAsset actionListAsset = item1.combineActionList[j];
							AdvGame.RunActionListAsset(actionListAsset);
							return;
						}
					}
				}
				if (KickStarter.settingsManager.inventoryDisableUnhandled)
				{
					selectedItem = null;
				}
				if ((bool)item1.unhandledCombineActionList)
				{
					ActionListAsset unhandledCombineActionList = item1.unhandledCombineActionList;
					AdvGame.RunActionListAsset(unhandledCombineActionList);
				}
				else if ((bool)unhandledCombine)
				{
					AdvGame.RunActionListAsset(unhandledCombine);
				}
			}
			KickStarter.playerCursor.ResetSelectedCursor();
		}

		public bool IsItemCarried(InvItem _item)
		{
			if (_item == null)
			{
				return false;
			}
			foreach (InvItem localItem in _localItems)
			{
				if (localItem == _item)
				{
					return true;
				}
			}
			return false;
		}

		public void RemoveRecipes()
		{
			while (craftingItems.Count > 0)
			{
				for (int i = 0; i < craftingItems.Count; i++)
				{
					Add(craftingItems[i].id, craftingItems[i].count);
					craftingItems.RemoveAt(i);
				}
			}
			PlayerMenus.ResetInventoryBoxes();
		}

		public void TransferCraftingToLocal(int _recipeSlot, bool selectAfter, bool forceAll = false)
		{
			for (int i = 0; i < craftingItems.Count; i++)
			{
				InvItem invItem = craftingItems[i];
				if (invItem.recipeSlot == _recipeSlot)
				{
					if (!forceAll && invItem.CanSelectSingle())
					{
						invItem.count--;
						Add(invItem.id, 1, selectAfter);
					}
					else
					{
						craftingItems.Remove(invItem);
						Add(invItem.id, invItem.count, selectAfter);
					}
					SelectItemByID(invItem.id);
					break;
				}
			}
		}

		public void TransferLocalToCrafting(InvItem _item, int _slot)
		{
			if (_item == null || !_localItems.Contains(_item))
			{
				return;
			}
			for (int i = 0; i < craftingItems.Count; i++)
			{
				InvItem invItem = craftingItems[i];
				if (invItem.recipeSlot != _slot)
				{
					continue;
				}
				if (invItem.id == _item.id && _item.canCarryMultiple)
				{
					if (_item.CanSelectSingle())
					{
						invItem.count++;
						_item.count--;
					}
					else
					{
						invItem.count += _item.count;
						_localItems[_localItems.IndexOf(_item)] = null;
						_localItems = ReorderItems(_localItems);
						_localItems = RemoveEmptySlots(_localItems);
					}
					SetNull();
					return;
				}
				TransferCraftingToLocal(_slot, false, true);
			}
			InvItem invItem2 = new InvItem(_item);
			invItem2.recipeSlot = _slot;
			if (_item.CanSelectSingle())
			{
				invItem2.count = 1;
				_localItems[localItems.IndexOf(_item)].count--;
			}
			else
			{
				_localItems[_localItems.IndexOf(_item)] = null;
				_localItems = ReorderItems(_localItems);
				_localItems = RemoveEmptySlots(_localItems);
			}
			craftingItems.Add(invItem2);
			SetNull();
		}

		public List<InvItem> MatchInteractions()
		{
			List<InvItem> list = new List<InvItem>();
			matchingInvInteractions = new List<int>();
			matchingItemModes = new List<SelectItemMode>();
			if (!KickStarter.settingsManager.cycleInventoryCursors)
			{
				return list;
			}
			if (hoverItem != null)
			{
				list = MatchInteractionsFromItem(list, hoverItem);
			}
			else if ((bool)KickStarter.playerInteraction.GetActiveHotspot())
			{
				List<Button> invButtons = KickStarter.playerInteraction.GetActiveHotspot().invButtons;
				foreach (Button item in invButtons)
				{
					foreach (InvItem localItem in _localItems)
					{
						if (localItem != null && localItem.id == item.invID && !item.isDisabled)
						{
							matchingInvInteractions.Add(invButtons.IndexOf(item));
							matchingItemModes.Add(item.selectItemMode);
							list.Add(localItem);
							break;
						}
					}
				}
			}
			return list;
		}

		public Recipe CalculateRecipe()
		{
			if (KickStarter.inventoryManager == null)
			{
				return null;
			}
			foreach (Recipe recipe in KickStarter.inventoryManager.recipes)
			{
				if (IsRecipeInvalid(recipe) || recipe.ingredients.Count == 0)
				{
					continue;
				}
				bool flag = true;
				while (flag)
				{
					foreach (Ingredient ingredient in recipe.ingredients)
					{
						InvItem craftingItem = GetCraftingItem(ingredient.itemID);
						if (craftingItem == null)
						{
							flag = false;
							break;
						}
						int craftingItemCount = GetCraftingItemCount(ingredient.itemID);
						if ((recipe.useSpecificSlots && craftingItem.recipeSlot == ingredient.slotNumber - 1) || !recipe.useSpecificSlots)
						{
							if ((craftingItem.canCarryMultiple && ingredient.amount <= craftingItemCount) || !craftingItem.canCarryMultiple)
							{
								if (flag && recipe.ingredients.IndexOf(ingredient) == recipe.ingredients.Count - 1)
								{
									return recipe;
								}
							}
							else
							{
								flag = false;
							}
						}
						else
						{
							flag = false;
						}
					}
				}
			}
			return null;
		}

		public void PerformCrafting(Recipe recipe, bool selectAfter)
		{
			foreach (Ingredient ingredient in recipe.ingredients)
			{
				int num = ingredient.amount;
				for (int i = 0; i < craftingItems.Count; i++)
				{
					if (craftingItems[i].id != ingredient.itemID)
					{
						continue;
					}
					if (craftingItems[i].canCarryMultiple && num > 0)
					{
						if (craftingItems[i].count < num)
						{
							num -= craftingItems[i].count;
							craftingItems.RemoveAt(i);
							i = -1;
							continue;
						}
						craftingItems[i].count -= num;
						if (craftingItems[i].count < 1)
						{
							craftingItems.RemoveAt(i);
							i = -1;
						}
					}
					else
					{
						craftingItems.RemoveAt(i);
					}
				}
			}
			RemoveEmptyCraftingSlots();
			Add(recipe.resultID, 1, selectAfter);
		}

		public void MoveItemToIndex(InvItem item, int index)
		{
			if (item != null && _localItems.Contains(item))
			{
				int num = _localItems.IndexOf(item);
				while (_localItems.Count <= Mathf.Max(index, num))
				{
					_localItems.Add(null);
				}
				if (_localItems[index] == null)
				{
					_localItems[index] = item;
					_localItems[num] = null;
				}
				else
				{
					_localItems[num] = null;
					_localItems.Insert(index, item);
				}
				SetNull();
				_localItems = RemoveEmptySlots(_localItems);
			}
			else if (item != null && selectedContainerItem != null)
			{
				while (_localItems.Count <= index)
				{
					_localItems.Add(null);
				}
				if (_localItems[index] == null)
				{
					_localItems[index] = item;
				}
				else
				{
					_localItems.Insert(index, item);
				}
				_localItems = RemoveEmptySlots(_localItems);
				selectedContainerItemContainer.Remove(selectedContainerItem);
				SetNull();
			}
		}

		public void AssignPlayerInventory(List<InvItem> newInventory)
		{
			_localItems = newInventory;
		}

		public List<InvItem> MoveItemToIndex(InvItem item, List<InvItem> items, int index)
		{
			if (item != null && items.Contains(item))
			{
				int num = items.IndexOf(item);
				while (items.Count <= Mathf.Max(index, num))
				{
					items.Add(null);
				}
				if (items[index] == null)
				{
					items[index] = item;
					items[num] = null;
				}
				else
				{
					items[num] = null;
					items.Insert(index, item);
				}
				SetNull();
				items = RemoveEmptySlots(items);
			}
			return items;
		}

		public void SetFont(Font font, int size, Color color, TextEffects textEffects)
		{
			countStyle = new GUIStyle();
			countStyle.font = font;
			countStyle.fontSize = size;
			countStyle.normal.textColor = color;
			countStyle.alignment = TextAnchor.MiddleCenter;
			countTextEffects = textEffects;
		}

		public void DrawHighlighted(Rect _rect)
		{
			if (highlightItem == null || highlightItem.activeTex == null)
			{
				return;
			}
			if (highlightState == HighlightState.None)
			{
				GUI.DrawTexture(_rect, highlightItem.activeTex, ScaleMode.StretchToFill, true, 0f);
				return;
			}
			if (pulseDirection == 0)
			{
				pulse = 0f;
				pulseDirection = 1;
			}
			else if (pulseDirection == 1)
			{
				pulse += KickStarter.settingsManager.inventoryPulseSpeed * Time.deltaTime;
			}
			else if (pulseDirection == -1)
			{
				pulse -= KickStarter.settingsManager.inventoryPulseSpeed * Time.deltaTime;
			}
			if (pulse > 1f)
			{
				pulse = 1f;
				if (highlightState == HighlightState.Normal)
				{
					highlightState = HighlightState.None;
					GUI.DrawTexture(_rect, highlightItem.activeTex, ScaleMode.StretchToFill, true, 0f);
					return;
				}
				pulseDirection = -1;
			}
			else if (pulse < 0f)
			{
				pulse = 0f;
				if (highlightState != HighlightState.Pulse)
				{
					highlightState = HighlightState.None;
					GUI.DrawTexture(_rect, highlightItem.tex, ScaleMode.StretchToFill, true, 0f);
					highlightItem = null;
					return;
				}
				pulseDirection = 1;
			}
			Color color = GUI.color;
			Color color2 = GUI.color;
			color2.a = pulse;
			GUI.color = color2;
			GUI.DrawTexture(_rect, highlightItem.activeTex, ScaleMode.StretchToFill, true, 0f);
			GUI.color = color;
			GUI.DrawTexture(_rect, highlightItem.tex, ScaleMode.StretchToFill, true, 0f);
		}

		public void HighlightItemOnInstant(int _id)
		{
			highlightItem = GetItem(_id);
			highlightState = HighlightState.None;
			pulse = 1f;
		}

		public void HighlightItemOffInstant()
		{
			highlightItem = null;
			highlightState = HighlightState.None;
			pulse = 0f;
		}

		public void HighlightItem(int _id, HighlightType _type)
		{
			highlightItem = GetItem(_id);
			if (highlightItem != null)
			{
				switch (_type)
				{
				case HighlightType.Enable:
					highlightState = HighlightState.Normal;
					pulseDirection = 1;
					break;
				case HighlightType.Disable:
					highlightState = HighlightState.Normal;
					pulseDirection = -1;
					break;
				case HighlightType.PulseOnce:
					highlightState = HighlightState.Flash;
					pulse = 0f;
					pulseDirection = 1;
					break;
				case HighlightType.PulseContinually:
					highlightState = HighlightState.Pulse;
					pulse = 0f;
					pulseDirection = 1;
					break;
				}
			}
		}

		public void DrawInventoryCount(Vector2 cursorPosition, float cursorSize, int count)
		{
			if (count > 1)
			{
				if (countTextEffects != TextEffects.None)
				{
					AdvGame.DrawTextEffect(AdvGame.GUIBox(cursorPosition, cursorSize), count.ToString(), countStyle, Color.black, countStyle.normal.textColor, 2f, countTextEffects);
				}
				else
				{
					GUI.Label(AdvGame.GUIBox(cursorPosition, cursorSize), count.ToString(), countStyle);
				}
			}
		}

		public void ProcessInventoryBoxClick(Menu _menu, MenuInventoryBox inventoryBox, int _slot, MouseState _mouseState)
		{
			switch (inventoryBox.inventoryBoxType)
			{
			case AC_InventoryBoxType.Default:
			case AC_InventoryBoxType.DisplayLastSelected:
				if (KickStarter.settingsManager.inventoryInteractions == InventoryInteractions.Multiple && KickStarter.playerMenus.IsInteractionMenuOn())
				{
					KickStarter.playerMenus.CloseInteractionMenus();
					ClickInvItemToInteract();
				}
				else if (KickStarter.settingsManager.inventoryInteractions == InventoryInteractions.Multiple && KickStarter.settingsManager.SelectInteractionMethod() == SelectInteractions.CyclingCursorAndClickingHotspot)
				{
					if (KickStarter.settingsManager.autoCycleWhenInteract && _mouseState == MouseState.SingleClick && (selectedItem == null || KickStarter.settingsManager.cycleInventoryCursors))
					{
						int interactionIndex = KickStarter.playerInteraction.GetInteractionIndex();
						KickStarter.playerInteraction.SetNextInteraction();
						KickStarter.playerInteraction.SetInteractionIndex(interactionIndex);
					}
					if (!KickStarter.settingsManager.cycleInventoryCursors && selectedItem != null)
					{
						inventoryBox.HandleDefaultClick(_mouseState, _slot, KickStarter.settingsManager.interactionMethod);
					}
					else if (_mouseState != MouseState.RightClick)
					{
						KickStarter.playerMenus.CloseInteractionMenus();
						ClickInvItemToInteract();
					}
					if (KickStarter.settingsManager.autoCycleWhenInteract && _mouseState == MouseState.SingleClick)
					{
						KickStarter.playerInteraction.RestoreInventoryInteraction();
					}
				}
				else if (KickStarter.settingsManager.interactionMethod != AC_InteractionMethod.ContextSensitive && KickStarter.settingsManager.inventoryInteractions == InventoryInteractions.Single)
				{
					inventoryBox.HandleDefaultClick(_mouseState, _slot, AC_InteractionMethod.ContextSensitive);
				}
				else
				{
					inventoryBox.HandleDefaultClick(_mouseState, _slot, KickStarter.settingsManager.interactionMethod);
					if (KickStarter.settingsManager.autoCycleWhenInteract && KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot && KickStarter.settingsManager.inventoryInteractions == InventoryInteractions.Multiple && selectedItem == null)
					{
						KickStarter.playerCursor.ResetSelectedCursor();
					}
				}
				_menu.Recalculate();
				break;
			case AC_InventoryBoxType.Container:
				inventoryBox.ClickContainer(_mouseState, _slot);
				_menu.Recalculate();
				break;
			case AC_InventoryBoxType.HotspotBased:
				if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction)
				{
					if (_menu.TargetInvItem != null)
					{
						Combine(_menu.TargetInvItem, inventoryBox.items[_slot + inventoryBox.GetOffset()], true);
					}
					else if (_menu.TargetHotspot != null)
					{
						InvItem invItem = inventoryBox.items[_slot + inventoryBox.GetOffset()];
						if (invItem != null)
						{
							_menu.TurnOff(false);
							KickStarter.playerInteraction.UseInventoryOnHotspot(_menu.TargetHotspot, invItem.id);
							KickStarter.playerCursor.ResetSelectedCursor();
						}
					}
					else
					{
						ACDebug.LogWarning("Cannot handle inventory click since there is no active Hotspot.");
					}
				}
				else
				{
					ACDebug.LogWarning("This type of InventoryBox only works with the Choose Hotspot Then Interaction method of interaction.");
				}
				break;
			case AC_InventoryBoxType.CustomScript:
			case AC_InventoryBoxType.DisplaySelected:
				break;
			}
		}

		public int GetTotalIntProperty(int ID)
		{
			return GetTotalIntProperty(_localItems.ToArray(), ID);
		}

		public int GetTotalIntProperty(InvItem[] items, int ID)
		{
			int num = 0;
			foreach (InvItem invItem in items)
			{
				foreach (InvVar var in invItem.vars)
				{
					if (var.id == ID && var.type == VariableType.Integer)
					{
						num += var.val;
						break;
					}
				}
			}
			return num;
		}

		public float GetTotalFloatProperty(int ID)
		{
			return GetTotalFloatProperty(_localItems.ToArray(), ID);
		}

		public float GetTotalFloatProperty(InvItem[] items, int ID)
		{
			float num = 0f;
			foreach (InvItem invItem in items)
			{
				foreach (InvVar var in invItem.vars)
				{
					if (var.id == ID && var.type == VariableType.Float)
					{
						num += var.floatVal;
						break;
					}
				}
			}
			return num;
		}

		public MainData SaveMainData(MainData mainData)
		{
			if (selectedItem != null)
			{
				mainData.selectedInventoryID = selectedItem.id;
				mainData.isGivingItem = IsGivingItem();
			}
			else
			{
				mainData.selectedInventoryID = -1;
			}
			return mainData;
		}

		public InvVar GetPropertyTotals(int propertyID)
		{
			InvVar property = KickStarter.inventoryManager.GetProperty(propertyID);
			if (property == null)
			{
				return null;
			}
			InvVar invVar = new InvVar(propertyID, property.type);
			foreach (InvItem localItem in _localItems)
			{
				if (localItem != null)
				{
					InvVar property2 = localItem.GetProperty(propertyID);
					if (property2 != null)
					{
						invVar.TransferValues(property2);
					}
				}
			}
			return invVar;
		}

		public InvVar GetPropertyTotals(int propertyID, int itemID)
		{
			InvVar property = KickStarter.inventoryManager.GetProperty(propertyID);
			if (property == null)
			{
				return null;
			}
			InvVar invVar = new InvVar(propertyID, property.type);
			foreach (InvItem localItem in _localItems)
			{
				if (localItem != null && localItem.id == itemID)
				{
					InvVar property2 = localItem.GetProperty(propertyID, true);
					if (property2 != null)
					{
						invVar.TransferValues(property2);
					}
				}
			}
			return invVar;
		}

		public InvItem[] GetItemsInCategory(int categoryID)
		{
			List<InvItem> list = new List<InvItem>();
			foreach (InvItem localItem in _localItems)
			{
				if (localItem.binID == categoryID)
				{
					list.Add(localItem);
				}
			}
			return list.ToArray();
		}

		public virtual bool CanTransferContainerItemsToInventory(ContainerItem containerItem)
		{
			return containerItem != null && !containerItem.IsEmpty;
		}

		protected void GetItemsOnStart()
		{
			if ((bool)KickStarter.inventoryManager)
			{
				foreach (InvItem item in KickStarter.inventoryManager.items)
				{
					if (item.carryOnStart)
					{
						int num = -1;
						if (KickStarter.settingsManager.playerSwitching == PlayerSwitching.Allow && !KickStarter.settingsManager.shareInventory && item.carryOnStartNotDefault && KickStarter.player != null && item.carryOnStartID != KickStarter.player.ID)
						{
							num = item.carryOnStartID;
						}
						if (!item.canCarryMultiple)
						{
							item.count = 1;
						}
						if (item.count >= 1)
						{
							item.recipeSlot = -1;
							if (item.canCarryMultiple && item.useSeparateSlots)
							{
								for (int i = 0; i < item.count; i++)
								{
									InvItem invItem = new InvItem(item);
									invItem.count = 1;
									if (num != -1)
									{
										Add(invItem.id, invItem.count, false, num);
									}
									else
									{
										_localItems.Add(invItem);
									}
								}
							}
							else if (num != -1)
							{
								Add(item.id, item.count, false, num);
							}
							else
							{
								_localItems.Add(new InvItem(item));
							}
						}
					}
				}
				return;
			}
			ACDebug.LogError("No Inventory Manager found - please use the Adventure Creator window to create one.");
		}

		protected void AddToOtherPlayer(int invID, int amount, int playerID, bool addToFront)
		{
			SaveSystem component = GetComponent<SaveSystem>();
			List<InvItem> itemsFromPlayer = component.GetItemsFromPlayer(playerID);
			itemsFromPlayer = Add(invID, amount, itemsFromPlayer, false, addToFront);
			component.AssignItemsToPlayer(itemsFromPlayer, playerID);
		}

		protected void RemoveFromOtherPlayer(int invID, int amount, bool setAmount, int playerID)
		{
			SaveSystem component = GetComponent<SaveSystem>();
			List<InvItem> itemsFromPlayer = component.GetItemsFromPlayer(playerID);
			itemsFromPlayer = Remove(invID, amount, setAmount, itemsFromPlayer);
			component.AssignItemsToPlayer(itemsFromPlayer, playerID);
		}

		protected List<InvItem> Remove(int _id, int amount, bool setAmount, List<InvItem> itemList)
		{
			if (amount <= 0)
			{
				return itemList;
			}
			foreach (InvItem item in itemList)
			{
				if (item == null || item.id != _id)
				{
					continue;
				}
				KickStarter.eventManager.Call_OnChangeInventory(item, InventoryEventType.Remove, amount);
				if (item.canCarryMultiple && item.useSeparateSlots)
				{
					itemList[itemList.IndexOf(item)] = null;
					amount--;
					if (amount == 0)
					{
						break;
					}
					continue;
				}
				if (!item.canCarryMultiple || !setAmount)
				{
					itemList[itemList.IndexOf(item)] = null;
					amount = 0;
				}
				else
				{
					if (item.count > 0)
					{
						int num = item.count - amount;
						item.count -= amount;
						amount = num;
					}
					if (item.count < 1)
					{
						itemList[itemList.IndexOf(item)] = null;
					}
				}
				itemList = ReorderItems(itemList);
				itemList = RemoveEmptySlots(itemList);
				if (itemList.Count == 0)
				{
					PlayerMenus.ResetInventoryBoxes();
					return itemList;
				}
				if (amount <= 0)
				{
					PlayerMenus.ResetInventoryBoxes();
					return itemList;
				}
			}
			itemList = ReorderItems(itemList);
			itemList = RemoveEmptySlots(itemList);
			PlayerMenus.ResetInventoryBoxes();
			return itemList;
		}

		protected List<InvItem> ReorderItems(List<InvItem> invItems)
		{
			if (!KickStarter.settingsManager.canReorderItems)
			{
				for (int i = 0; i < invItems.Count; i++)
				{
					if (invItems[i] == null)
					{
						invItems.RemoveAt(i);
						i = 0;
					}
				}
			}
			return invItems;
		}

		protected void RemoveEmptyCraftingSlots()
		{
			int num = craftingItems.Count - 1;
			while (num >= 0 && _localItems.Count > num && _localItems[num] == null)
			{
				_localItems.RemoveAt(num);
				num--;
			}
		}

		protected int GetCraftingItemCount(int _id)
		{
			int num = 0;
			for (int i = 0; i < craftingItems.Count; i++)
			{
				if (craftingItems[i].id == _id)
				{
					num = ((!craftingItems[i].canCarryMultiple) ? (num + 1) : (num + craftingItems[i].count));
				}
			}
			return num;
		}

		protected List<InvItem> MatchInteractionsFromItem(List<InvItem> items, InvItem _item)
		{
			if (_item != null && _item.combineID != null)
			{
				foreach (int item in _item.combineID)
				{
					foreach (InvItem localItem in _localItems)
					{
						if (localItem != null && localItem.id == item)
						{
							matchingInvInteractions.Add(_item.combineID.IndexOf(item));
							matchingItemModes.Add(SelectItemMode.Use);
							items.Add(localItem);
							break;
						}
					}
				}
			}
			return items;
		}

		protected bool IsRecipeInvalid(Recipe recipe)
		{
			for (int i = 0; i < craftingItems.Count; i++)
			{
				bool flag = false;
				for (int j = 0; j < recipe.ingredients.Count; j++)
				{
					if (recipe.ingredients[j].itemID == craftingItems[i].id)
					{
						flag = true;
					}
				}
				if (!flag)
				{
					return true;
				}
			}
			return false;
		}

		protected void ClickInvItemToInteract()
		{
			int activeInvButtonID = KickStarter.playerInteraction.GetActiveInvButtonID();
			if (activeInvButtonID == -1)
			{
				RunInteraction(KickStarter.playerInteraction.GetActiveUseButtonIconID());
			}
			else
			{
				Combine(hoverItem, activeInvButtonID);
			}
		}
	}
}
