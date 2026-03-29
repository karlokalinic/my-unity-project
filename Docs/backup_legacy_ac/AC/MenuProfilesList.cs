using UnityEngine;

namespace AC
{
	public class MenuProfilesList : MenuElement
	{
		public UISlot[] uiSlots;

		public TextEffects textEffects;

		public float outlineSize = 2f;

		public TextAnchor anchor;

		public int maxSlots = 5;

		public ActionListAsset actionListOnClick;

		public bool showActive = true;

		public UIHideStyle uiHideStyle;

		public bool autoHandle = true;

		public LinkUIGraphic linkUIGraphic;

		public bool fixedOption;

		public int optionToShow;

		public int parameterID = -1;

		private string[] labels;

		public override void Declare()
		{
			uiSlots = null;
			isVisible = true;
			isClickable = true;
			numSlots = 1;
			maxSlots = 5;
			showActive = true;
			SetSize(new Vector2(20f, 5f));
			anchor = TextAnchor.MiddleCenter;
			actionListOnClick = null;
			textEffects = TextEffects.None;
			outlineSize = 2f;
			uiHideStyle = UIHideStyle.DisableObject;
			fixedOption = false;
			optionToShow = 0;
			autoHandle = true;
			parameterID = -1;
			linkUIGraphic = LinkUIGraphic.ImageComponent;
			base.Declare();
		}

		public override MenuElement DuplicateSelf(bool fromEditor, bool ignoreUnityUI)
		{
			MenuProfilesList menuProfilesList = ScriptableObject.CreateInstance<MenuProfilesList>();
			menuProfilesList.Declare();
			menuProfilesList.CopyProfilesList(this, ignoreUnityUI);
			return menuProfilesList;
		}

		private void CopyProfilesList(MenuProfilesList _element, bool ignoreUnityUI)
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
			anchor = _element.anchor;
			maxSlots = _element.maxSlots;
			actionListOnClick = _element.actionListOnClick;
			showActive = _element.showActive;
			uiHideStyle = _element.uiHideStyle;
			autoHandle = _element.autoHandle;
			parameterID = _element.parameterID;
			fixedOption = _element.fixedOption;
			optionToShow = _element.optionToShow;
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
			if (uiSlots != null && uiSlots.Length > slotIndex && uiSlots[slotIndex].uiButton != null)
			{
				return uiSlots[0].uiButton.gameObject;
			}
			return null;
		}

		public override void HideAllUISlots()
		{
			LimitUISlotVisibility(uiSlots, 0, uiHideStyle);
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

		public override void Shift(AC_ShiftInventory shiftType, int amount)
		{
			if (!fixedOption && isVisible && numSlots >= maxSlots)
			{
				Shift(shiftType, maxSlots, KickStarter.options.GetNumProfiles(), amount);
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
			if (fixedOption)
			{
				return 0;
			}
			if (!showActive)
			{
				return Mathf.Max(0, KickStarter.options.GetNumProfiles() - 1 - maxSlots);
			}
			return Mathf.Max(0, KickStarter.options.GetNumProfiles() - maxSlots);
		}

		public override string GetLabel(int slot, int languageNumber)
		{
			if (Application.isPlaying)
			{
				if (fixedOption)
				{
					return KickStarter.options.GetProfileIDName(optionToShow);
				}
				return KickStarter.options.GetProfileName(slot + offset, showActive);
			}
			if (fixedOption)
			{
				return "Profile ID " + optionToShow;
			}
			return "Profile " + slot;
		}

		public override bool IsSelectedByEventSystem(int slotIndex)
		{
			if (uiSlots != null && slotIndex >= 0 && uiSlots.Length > slotIndex && uiSlots[slotIndex] != null && uiSlots[slotIndex].uiButton != null)
			{
				return KickStarter.playerMenus.IsEventSystemSelectingObject(uiSlots[slotIndex].uiButton.gameObject);
			}
			return false;
		}

		public override void PreDisplay(int _slot, int languageNumber, bool isActive)
		{
			string label = GetLabel(_slot, languageNumber);
			if (!Application.isPlaying && (labels == null || labels.Length != numSlots))
			{
				labels = new string[numSlots];
			}
			labels[_slot] = label;
			if (Application.isPlaying && uiSlots != null && uiSlots.Length > _slot)
			{
				LimitUISlotVisibility(uiSlots, numSlots, uiHideStyle);
				uiSlots[_slot].SetText(labels[_slot]);
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
				AdvGame.DrawTextEffect(ZoomRect(GetSlotRectRelative(_slot), zoom), labels[_slot], _style, Color.black, _style.normal.textColor, outlineSize, textEffects);
			}
			else
			{
				GUI.Label(ZoomRect(GetSlotRectRelative(_slot), zoom), labels[_slot], _style);
			}
		}

		public override void ProcessClick(Menu _menu, int _slot, MouseState _mouseState)
		{
			if (KickStarter.stateHandler.gameState == GameState.Cutscene)
			{
				return;
			}
			if (autoHandle)
			{
				bool flag = false;
				if ((!fixedOption) ? KickStarter.options.SwitchProfile(_slot + offset, showActive) : Options.SwitchProfileID(optionToShow))
				{
					RunActionList(_slot);
				}
			}
			else
			{
				RunActionList(_slot);
			}
			base.ProcessClick(_menu, _slot, _mouseState);
		}

		private void RunActionList(int _slot)
		{
			if (fixedOption)
			{
				AdvGame.RunActionListAsset(actionListOnClick, parameterID, optionToShow);
			}
			else
			{
				AdvGame.RunActionListAsset(actionListOnClick, parameterID, _slot + offset);
			}
		}

		public override void RecalculateSize(MenuSource source)
		{
			if (Application.isPlaying)
			{
				if (fixedOption)
				{
					numSlots = 1;
				}
				else
				{
					numSlots = KickStarter.options.GetNumProfiles();
					if (!showActive)
					{
						numSlots--;
					}
					if (numSlots > maxSlots)
					{
						numSlots = maxSlots;
					}
					offset = Mathf.Min(offset, GetMaxOffset());
				}
			}
			labels = new string[numSlots];
			if (!isVisible)
			{
				LimitUISlotVisibility(uiSlots, 0, uiHideStyle);
			}
			base.RecalculateSize(source);
		}

		protected override void AutoSize()
		{
			AutoSize(new GUIContent(GetLabel(0, 0)));
		}
	}
}
