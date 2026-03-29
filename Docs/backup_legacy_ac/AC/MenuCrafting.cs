using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	public class MenuCrafting : MenuElement
	{
		public UISlot[] uiSlots;

		public TextEffects textEffects;

		public float outlineSize = 2f;

		public CraftingElementType craftingType;

		public List<InvItem> items = new List<InvItem>();

		public ConversationDisplayType displayType = ConversationDisplayType.IconOnly;

		public UIHideStyle uiHideStyle;

		public ActionListAsset actionListOnWrongIngredients;

		public LinkUIGraphic linkUIGraphic;

		public bool autoCreate = true;

		public InventoryItemCountDisplay inventoryItemCountDisplay;

		private Recipe activeRecipe;

		private bool[] isFilled;

		private string[] labels;

		public override void Declare()
		{
			uiSlots = null;
			isVisible = true;
			isClickable = true;
			numSlots = 4;
			SetSize(new Vector2(6f, 10f));
			textEffects = TextEffects.None;
			outlineSize = 2f;
			craftingType = CraftingElementType.Ingredients;
			displayType = ConversationDisplayType.IconOnly;
			uiHideStyle = UIHideStyle.DisableObject;
			actionListOnWrongIngredients = null;
			linkUIGraphic = LinkUIGraphic.ImageComponent;
			items = new List<InvItem>();
			autoCreate = true;
			inventoryItemCountDisplay = InventoryItemCountDisplay.OnlyIfMultiple;
		}

		public override MenuElement DuplicateSelf(bool fromEditor, bool ignoreUnityUI)
		{
			MenuCrafting menuCrafting = ScriptableObject.CreateInstance<MenuCrafting>();
			menuCrafting.Declare();
			menuCrafting.CopyCrafting(this, ignoreUnityUI);
			return menuCrafting;
		}

		private void CopyCrafting(MenuCrafting _element, bool ignoreUnityUI)
		{
			if (ignoreUnityUI)
			{
				uiSlots = null;
			}
			else
			{
				uiSlots = _element.uiSlots;
			}
			isClickable = _element.isClickable;
			textEffects = _element.textEffects;
			outlineSize = _element.outlineSize;
			numSlots = _element.numSlots;
			craftingType = _element.craftingType;
			displayType = _element.displayType;
			uiHideStyle = _element.uiHideStyle;
			actionListOnWrongIngredients = _element.actionListOnWrongIngredients;
			linkUIGraphic = _element.linkUIGraphic;
			autoCreate = _element.autoCreate;
			inventoryItemCountDisplay = _element.inventoryItemCountDisplay;
			PopulateList(MenuSource.AdventureCreator);
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
						ProcessClickUI(_menu, j, MouseState.SingleClick);
					});
				}
				num++;
			}
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

		public override void SetUIInteractableState(bool state)
		{
			SetUISlotsInteractableState(uiSlots, state);
		}

		public override void HideAllUISlots()
		{
			LimitUISlotVisibility(uiSlots, 0, uiHideStyle);
		}

		public override string GetHotspotLabelOverride(int _slot, int _language)
		{
			if (uiSlots != null && _slot < uiSlots.Length && !uiSlots[_slot].CanOverrideHotspotLabel)
			{
				return string.Empty;
			}
			InvItem item = GetItem(_slot);
			if (item != null)
			{
				if (_language > 0)
				{
					return KickStarter.runtimeLanguages.GetTranslation(item.label, item.lineID, _language);
				}
				if (!string.IsNullOrEmpty(item.altLabel))
				{
					return item.altLabel;
				}
				return item.label;
			}
			return string.Empty;
		}

		public override void PreDisplay(int _slot, int languageNumber, bool isActive)
		{
			string text = string.Empty;
			if (displayType == ConversationDisplayType.TextOnly || displayType == ConversationDisplayType.IconAndText)
			{
				InvItem item = GetItem(_slot);
				if (item != null)
				{
					text = item.label;
					if (KickStarter.runtimeInventory != null)
					{
						text = KickStarter.runtimeInventory.GetLabel(item, languageNumber);
					}
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
			if (craftingType == CraftingElementType.Ingredients)
			{
				if (isFilled == null || isFilled.Length != numSlots)
				{
					isFilled = new bool[numSlots];
				}
				isFilled[_slot] = false;
				foreach (InvItem item2 in items)
				{
					if (item2.recipeSlot == _slot)
					{
						isFilled[_slot] = true;
						break;
					}
				}
			}
			if (!Application.isPlaying || uiSlots == null || uiSlots.Length <= _slot)
			{
				return;
			}
			LimitUISlotVisibility(uiSlots, numSlots, uiHideStyle);
			uiSlots[_slot].SetText(labels[_slot]);
			if (displayType == ConversationDisplayType.IconOnly || displayType == ConversationDisplayType.IconAndText)
			{
				if ((craftingType == CraftingElementType.Ingredients && isFilled[_slot]) || (craftingType == CraftingElementType.Output && items.Count > 0))
				{
					uiSlots[_slot].SetImage(GetTexture(_slot));
				}
				else
				{
					uiSlots[_slot].SetImage(null);
				}
			}
		}

		public override void Display(GUIStyle _style, int _slot, float zoom, bool isActive)
		{
			base.Display(_style, _slot, zoom, isActive);
			if (craftingType == CraftingElementType.Ingredients)
			{
				if (displayType == ConversationDisplayType.IconOnly)
				{
					GUI.Label(GetSlotRectRelative(_slot), string.Empty, _style);
					if (isFilled == null || isFilled.Length != numSlots)
					{
						isFilled = new bool[numSlots];
					}
					if (!isFilled[_slot] && Application.isPlaying)
					{
						return;
					}
					DrawTexture(ZoomRect(GetSlotRectRelative(_slot), zoom), _slot);
					_style.normal.background = null;
				}
				else if (!isFilled[_slot] && Application.isPlaying)
				{
					GUI.Label(GetSlotRectRelative(_slot), string.Empty, _style);
				}
				DrawText(_style, _slot, zoom);
			}
			else
			{
				if (craftingType != CraftingElementType.Output)
				{
					return;
				}
				GUI.Label(GetSlotRectRelative(_slot), string.Empty, _style);
				if (items.Count > 0)
				{
					if (displayType == ConversationDisplayType.IconOnly)
					{
						DrawTexture(ZoomRect(GetSlotRectRelative(_slot), zoom), _slot);
					}
					DrawText(_style, _slot, zoom);
				}
			}
		}

		private void DrawText(GUIStyle _style, int _slot, float zoom)
		{
			if (textEffects != TextEffects.None)
			{
				AdvGame.DrawTextEffect(ZoomRect(GetSlotRectRelative(_slot), zoom), labels[_slot], _style, Color.black, _style.normal.textColor, outlineSize, textEffects);
			}
			else
			{
				GUI.Label(ZoomRect(GetSlotRectRelative(_slot), zoom), labels[_slot], _style);
			}
		}

		private void HandleDefaultClick(MouseState _mouseState, int _slot)
		{
			if (craftingType != CraftingElementType.Ingredients)
			{
				return;
			}
			switch (_mouseState)
			{
			case MouseState.SingleClick:
				if (KickStarter.runtimeInventory.SelectedItem == null)
				{
					if (GetItem(_slot) != null)
					{
						KickStarter.runtimeInventory.TransferCraftingToLocal(GetItem(_slot).recipeSlot, true);
					}
				}
				else
				{
					KickStarter.runtimeInventory.TransferLocalToCrafting(KickStarter.runtimeInventory.SelectedItem, _slot);
				}
				break;
			case MouseState.RightClick:
				if (KickStarter.runtimeInventory.SelectedItem != null)
				{
					KickStarter.runtimeInventory.SetNull();
				}
				break;
			}
			PlayerMenus.ResetInventoryBoxes();
		}

		private void ClickOutput(Menu _menu, MouseState _mouseState)
		{
			if (items.Count <= 0)
			{
				return;
			}
			if (_mouseState == MouseState.SingleClick && KickStarter.runtimeInventory.SelectedItem == null)
			{
				if (activeRecipe.onCreateRecipe == OnCreateRecipe.SelectItem)
				{
					KickStarter.runtimeInventory.PerformCrafting(activeRecipe, true);
				}
				else if (activeRecipe.onCreateRecipe == OnCreateRecipe.RunActionList)
				{
					KickStarter.runtimeInventory.PerformCrafting(activeRecipe, false);
					if (activeRecipe.invActionList != null)
					{
						AdvGame.RunActionListAsset(activeRecipe.invActionList);
					}
				}
				else
				{
					KickStarter.runtimeInventory.PerformCrafting(activeRecipe, false);
				}
			}
			PlayerMenus.ResetInventoryBoxes();
		}

		public override void RecalculateSize(MenuSource source)
		{
			PopulateList(source);
			isFilled = new bool[numSlots];
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

		private void PopulateList(MenuSource source)
		{
			if (Application.isPlaying)
			{
				if (craftingType == CraftingElementType.Ingredients)
				{
					items = new List<InvItem>();
					{
						foreach (InvItem craftingItem in KickStarter.runtimeInventory.craftingItems)
						{
							items.Add(craftingItem);
						}
						return;
					}
				}
				if (craftingType != CraftingElementType.Output)
				{
					return;
				}
				if (autoCreate)
				{
					SetOutput(source);
				}
				else if (activeRecipe != null)
				{
					Recipe recipe = KickStarter.runtimeInventory.CalculateRecipe();
					if (recipe != activeRecipe)
					{
						activeRecipe = null;
						items = new List<InvItem>();
					}
				}
				return;
			}
			items = new List<InvItem>();
			if (!(AdvGame.GetReferences().inventoryManager != null))
			{
				return;
			}
			foreach (InvItem item in AdvGame.GetReferences().inventoryManager.items)
			{
				items.Add(item);
				if (craftingType == CraftingElementType.Output || items.Count >= numSlots)
				{
					break;
				}
			}
		}

		public void SetOutput(MenuSource source)
		{
			if (craftingType != CraftingElementType.Output)
			{
				return;
			}
			items = new List<InvItem>();
			activeRecipe = KickStarter.runtimeInventory.CalculateRecipe();
			if (activeRecipe != null)
			{
				AdvGame.RunActionListAsset(activeRecipe.actionListOnCreate);
				foreach (InvItem item in AdvGame.GetReferences().inventoryManager.items)
				{
					if (item.id == activeRecipe.resultID)
					{
						InvItem invItem = new InvItem(item);
						invItem.count = 1;
						items.Add(invItem);
					}
				}
				KickStarter.eventManager.Call_OnCraftingSucceed(activeRecipe);
			}
			else if (!autoCreate && actionListOnWrongIngredients != null)
			{
				actionListOnWrongIngredients.Interact();
			}
		}

		private Texture GetTexture(int i)
		{
			Texture result = null;
			if (Application.isPlaying)
			{
				result = GetItem(i).tex;
			}
			else if (items.Count > i && items[i].tex != null)
			{
				result = items[i].tex;
			}
			return result;
		}

		private void DrawTexture(Rect rect, int i)
		{
			Texture texture = GetTexture(i);
			if (texture != null)
			{
				GUI.DrawTexture(rect, texture, ScaleMode.StretchToFill, true, 0f);
			}
		}

		public override string GetLabel(int i, int languageNumber)
		{
			if (languageNumber > 0)
			{
				return KickStarter.runtimeLanguages.GetTranslation(GetItem(i).label, GetItem(i).lineID, languageNumber);
			}
			if (GetItem(i).altLabel != string.Empty)
			{
				return GetItem(i).altLabel;
			}
			return GetItem(i).label;
		}

		public override bool IsSelectedByEventSystem(int slotIndex)
		{
			if (uiSlots != null && slotIndex >= 0 && uiSlots.Length > slotIndex && uiSlots[slotIndex] != null && uiSlots[slotIndex].uiButton != null)
			{
				return KickStarter.playerMenus.IsEventSystemSelectingObject(uiSlots[slotIndex].uiButton.gameObject);
			}
			return false;
		}

		public InvItem GetItem(int i)
		{
			if (craftingType == CraftingElementType.Output)
			{
				if (items.Count > i)
				{
					return items[i];
				}
			}
			else if (craftingType == CraftingElementType.Ingredients)
			{
				foreach (InvItem item in items)
				{
					if (item.recipeSlot == i || !Application.isPlaying)
					{
						return item;
					}
				}
			}
			return null;
		}

		private string GetCount(int i)
		{
			if (inventoryItemCountDisplay == InventoryItemCountDisplay.Never)
			{
				return string.Empty;
			}
			InvItem item = GetItem(i);
			if (item != null)
			{
				if (GetItem(i).count < 2 && inventoryItemCountDisplay == InventoryItemCountDisplay.OnlyIfMultiple)
				{
					return string.Empty;
				}
				return GetItem(i).count.ToString();
			}
			return string.Empty;
		}

		public override void ProcessClick(Menu _menu, int _slot, MouseState _mouseState)
		{
			if (KickStarter.stateHandler.gameState != GameState.Cutscene)
			{
				if (craftingType == CraftingElementType.Ingredients)
				{
					HandleDefaultClick(_mouseState, _slot);
				}
				else if (craftingType == CraftingElementType.Output)
				{
					ClickOutput(_menu, _mouseState);
				}
				_menu.Recalculate();
				base.ProcessClick(_menu, _slot, _mouseState);
			}
		}

		protected override void AutoSize()
		{
			if (items.Count > 0)
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
						break;
					}
				}
				return;
			}
			AutoSize(GUIContent.none);
		}

		public int GetItemSlot(int itemID)
		{
			foreach (InvItem item in items)
			{
				if (item.id == itemID)
				{
					if (craftingType == CraftingElementType.Ingredients)
					{
						return item.recipeSlot;
					}
					return items.IndexOf(item) - offset;
				}
			}
			return 0;
		}
	}
}
