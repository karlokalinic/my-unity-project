using UnityEngine;

namespace AC
{
	public class MenuDialogList : MenuElement
	{
		public UISlot[] uiSlots;

		public TextEffects textEffects;

		public float outlineSize = 2f;

		public ConversationDisplayType displayType;

		public Texture2D testIcon;

		public TextAnchor anchor;

		public bool fixedOption;

		public int optionToShow;

		public int maxSlots = 10;

		public bool markAlreadyChosen;

		public Color alreadyChosenFontColour = Color.white;

		public Color alreadyChosenFontHighlightedColour = Color.white;

		public bool showIndexNumbers;

		public UIHideStyle uiHideStyle;

		public LinkUIGraphic linkUIGraphic;

		public bool resetOffsetWhenRestart = true;

		private Conversation linkedConversation;

		private Conversation overrideConversation;

		private int numOptions;

		private string[] labels;

		private CursorIconBase[] icons;

		private bool[] chosens;

		public Conversation OverrideConversation
		{
			set
			{
				overrideConversation = value;
			}
		}

		public override void Declare()
		{
			uiSlots = null;
			isVisible = true;
			isClickable = true;
			fixedOption = false;
			displayType = ConversationDisplayType.TextOnly;
			testIcon = null;
			optionToShow = 1;
			numSlots = 0;
			SetSize(new Vector2(20f, 5f));
			maxSlots = 10;
			anchor = TextAnchor.MiddleLeft;
			textEffects = TextEffects.None;
			outlineSize = 2f;
			markAlreadyChosen = false;
			alreadyChosenFontColour = Color.white;
			alreadyChosenFontHighlightedColour = Color.white;
			showIndexNumbers = false;
			uiHideStyle = UIHideStyle.DisableObject;
			linkUIGraphic = LinkUIGraphic.ImageComponent;
			resetOffsetWhenRestart = true;
			base.Declare();
		}

		public override MenuElement DuplicateSelf(bool fromEditor, bool ignoreUnityUI)
		{
			MenuDialogList menuDialogList = ScriptableObject.CreateInstance<MenuDialogList>();
			menuDialogList.Declare();
			menuDialogList.CopyDialogList(this, ignoreUnityUI);
			return menuDialogList;
		}

		private void CopyDialogList(MenuDialogList _element, bool ignoreUnityUI)
		{
			if (ignoreUnityUI)
			{
				uiSlots = null;
			}
			else
			{
				uiSlots = _element.uiSlots;
			}
			textEffects = _element.textEffects;
			outlineSize = _element.outlineSize;
			displayType = _element.displayType;
			testIcon = _element.testIcon;
			anchor = _element.anchor;
			labels = _element.labels;
			fixedOption = _element.fixedOption;
			optionToShow = _element.optionToShow;
			maxSlots = _element.maxSlots;
			markAlreadyChosen = _element.markAlreadyChosen;
			alreadyChosenFontColour = _element.alreadyChosenFontColour;
			alreadyChosenFontHighlightedColour = _element.alreadyChosenFontHighlightedColour;
			showIndexNumbers = _element.showIndexNumbers;
			uiHideStyle = _element.uiHideStyle;
			linkUIGraphic = _element.linkUIGraphic;
			resetOffsetWhenRestart = _element.resetOffsetWhenRestart;
			base.Copy(_element);
		}

		public override void HideAllUISlots()
		{
			LimitUISlotVisibility(uiSlots, 0, uiHideStyle);
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

		public override string GetHotspotLabelOverride(int _slot, int _language)
		{
			if (uiSlots != null && _slot < uiSlots.Length && !uiSlots[_slot].CanOverrideHotspotLabel)
			{
				return string.Empty;
			}
			if (displayType == ConversationDisplayType.IconOnly && labels.Length > _slot)
			{
				return labels[_slot];
			}
			return string.Empty;
		}

		public override void PreDisplay(int _slot, int languageNumber, bool isActive)
		{
			if (fixedOption)
			{
				_slot = 0;
			}
			if (Application.isPlaying)
			{
				if (uiSlots != null && uiSlots.Length > _slot)
				{
					LimitUISlotVisibility(uiSlots, numSlots, uiHideStyle);
					if (displayType == ConversationDisplayType.IconOnly || displayType == ConversationDisplayType.IconAndText)
					{
						uiSlots[_slot].SetImageAsSprite(icons[_slot].GetAnimatedSprite(isActive));
					}
					if (displayType == ConversationDisplayType.TextOnly || displayType == ConversationDisplayType.IconAndText)
					{
						uiSlots[_slot].SetText(labels[_slot]);
					}
				}
				return;
			}
			string empty = string.Empty;
			if (fixedOption)
			{
				empty = "Dialogue option " + optionToShow;
				empty = AddIndexNumber(empty, optionToShow);
			}
			else
			{
				empty = "Dialogue option " + _slot;
				empty = AddIndexNumber(empty, _slot + 1);
			}
			if (labels == null || labels.Length != numSlots)
			{
				labels = new string[numSlots];
			}
			chosens = new bool[numSlots];
			labels[_slot] = empty;
		}

		private string AddIndexNumber(string _label, int _i)
		{
			if (showIndexNumbers)
			{
				return _i + ". " + _label;
			}
			return _label;
		}

		public override void Display(GUIStyle _style, int _slot, float zoom, bool isActive)
		{
			base.Display(_style, _slot, zoom, isActive);
			if (fixedOption)
			{
				_slot = 0;
			}
			if (markAlreadyChosen)
			{
				if (chosens[_slot])
				{
					if (isActive)
					{
						_style.normal.textColor = alreadyChosenFontHighlightedColour;
					}
					else
					{
						_style.normal.textColor = alreadyChosenFontColour;
					}
				}
				else if (isActive)
				{
					_style.normal.textColor = fontHighlightColor;
				}
				else
				{
					_style.normal.textColor = fontColor;
				}
			}
			_style.wordWrap = true;
			_style.alignment = anchor;
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
			if (Application.isPlaying && icons[_slot] != null)
			{
				icons[_slot].DrawAsInteraction(ZoomRect(GetSlotRectRelative(_slot), zoom), isActive);
			}
			else if (testIcon != null)
			{
				GUI.DrawTexture(ZoomRect(GetSlotRectRelative(_slot), zoom), testIcon, ScaleMode.StretchToFill, true, 0f);
			}
			GUI.Label(ZoomRect(GetSlotRectRelative(_slot), zoom), string.Empty, _style);
		}

		public override void RecalculateSize(MenuSource source)
		{
			if (Application.isPlaying)
			{
				if (linkedConversation != null)
				{
					numOptions = linkedConversation.GetCount();
					if (fixedOption)
					{
						if (numOptions < optionToShow)
						{
							numSlots = 0;
						}
						else
						{
							numSlots = 1;
							labels = new string[numSlots];
							labels[0] = linkedConversation.GetOptionName(optionToShow - 1);
							labels[0] = AddIndexNumber(labels[0], optionToShow);
							icons = new CursorIconBase[numSlots];
							icons[0] = new CursorIconBase();
							icons[0].Copy(linkedConversation.GetOptionIcon(optionToShow - 1));
							chosens = new bool[numSlots];
							chosens[0] = linkedConversation.OptionHasBeenChosen(optionToShow - 1);
						}
					}
					else
					{
						numSlots = numOptions;
						if (numSlots > maxSlots)
						{
							numSlots = maxSlots;
						}
						labels = new string[numSlots];
						icons = new CursorIconBase[numSlots];
						chosens = new bool[numSlots];
						for (int i = 0; i < numSlots; i++)
						{
							labels[i] = linkedConversation.GetOptionName(i + offset);
							labels[i] = AddIndexNumber(labels[i], i + offset + 1);
							icons[i] = new CursorIconBase();
							icons[i].Copy(linkedConversation.GetOptionIcon(i + offset));
							chosens[i] = linkedConversation.OptionHasBeenChosen(i + offset);
						}
						if (markAlreadyChosen && source != MenuSource.AdventureCreator)
						{
							for (int j = 0; j < chosens.Length; j++)
							{
								bool flag = chosens[j];
								if (uiSlots.Length > j)
								{
									if (flag)
									{
										uiSlots[j].SetColour(alreadyChosenFontColour);
									}
									else
									{
										uiSlots[j].RestoreColour();
									}
								}
							}
						}
						LimitOffset(numOptions);
					}
				}
				else
				{
					numSlots = 0;
				}
			}
			else if (fixedOption)
			{
				numSlots = 1;
				offset = 0;
				labels = new string[numSlots];
				icons = new CursorIconBase[numSlots];
				chosens = new bool[numSlots];
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

		public override void Shift(AC_ShiftInventory shiftType, int amount)
		{
			if (isVisible && numSlots >= maxSlots)
			{
				Shift(shiftType, maxSlots, numOptions, amount);
			}
		}

		public override bool CanBeShifted(AC_ShiftInventory shiftType)
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
			else if (maxSlots + offset >= numOptions)
			{
				return false;
			}
			return true;
		}

		public override void OnMenuTurnOn(Menu menu)
		{
			base.OnMenuTurnOn(menu);
			Conversation conversation = linkedConversation;
			linkedConversation = ((!(overrideConversation != null)) ? KickStarter.playerInput.activeConversation : overrideConversation);
			if (conversation != linkedConversation || resetOffsetWhenRestart)
			{
				offset = 0;
			}
			if (linkedConversation != null && !fixedOption)
			{
				LimitOffset(linkedConversation.GetCount());
			}
		}

		public override string GetLabel(int slot, int languageNumber)
		{
			if (labels.Length > slot)
			{
				return labels[slot];
			}
			return string.Empty;
		}

		public override bool IsSelectedByEventSystem(int slotIndex)
		{
			if (uiSlots != null && slotIndex >= 0 && uiSlots.Length > slotIndex && uiSlots[slotIndex] != null && uiSlots[slotIndex].uiButton != null)
			{
				return KickStarter.playerMenus.IsEventSystemSelectingObject(uiSlots[slotIndex].uiButton.gameObject);
			}
			return false;
		}

		public override void ProcessClick(Menu _menu, int _slot, MouseState _mouseState)
		{
			if (KickStarter.stateHandler.gameState != GameState.DialogOptions)
			{
				return;
			}
			if (linkedConversation != null && (linkedConversation == overrideConversation || (overrideConversation == null && (bool)KickStarter.playerInput.activeConversation)))
			{
				if (fixedOption)
				{
					linkedConversation.RunOption(optionToShow - 1);
				}
				else
				{
					linkedConversation.RunOption(_slot + offset);
				}
			}
			base.ProcessClick(_menu, _slot, _mouseState);
		}

		protected override void AutoSize()
		{
			if (displayType == ConversationDisplayType.IconOnly)
			{
				AutoSize(new GUIContent(testIcon));
			}
			else
			{
				AutoSize(new GUIContent("Dialogue option 0"));
			}
		}

		public ButtonDialog GetDialogueOption(int slotIndex)
		{
			slotIndex += offset;
			if ((bool)linkedConversation)
			{
				return linkedConversation.GetOption(slotIndex);
			}
			return null;
		}
	}
}
