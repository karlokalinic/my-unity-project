using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AC
{
	[Serializable]
	public class MenuElement : ScriptableObject
	{
		public int ID;

		public string title = "Element";

		public Vector2 slotSize;

		public AC_SizeType sizeType;

		public AC_PositionType2 positionType;

		public float slotSpacing;

		public int lineID = -1;

		public string alternativeInputButton = string.Empty;

		public Font font;

		public float fontScaleFactor = 60f;

		public Color fontColor = Color.white;

		public Color fontHighlightColor = Color.white;

		[SerializeField]
		protected bool isVisible = true;

		public bool isClickable;

		public ElementOrientation orientation = ElementOrientation.Vertical;

		public int gridWidth = 3;

		public Texture2D backgroundTexture;

		public bool singleSlotBackgrounds;

		public Texture2D highlightTexture;

		public AudioClip hoverSound;

		public AudioClip clickSound;

		public bool changeCursor;

		public int cursorID;

		public int linkedUiID;

		protected int offset;

		private string idString;

		private Vector2 dragOffset;

		protected Menu parentMenu;

		[SerializeField]
		protected Rect relativeRect;

		[SerializeField]
		protected Vector2 relativePosition;

		[SerializeField]
		protected int numSlots;

		public bool IsVisible
		{
			get
			{
				return isVisible;
			}
			set
			{
				if (isVisible != value)
				{
					isVisible = value;
					KickStarter.eventManager.Call_OnMenuElementChangeVisibility(this);
				}
			}
		}

		public string IDString
		{
			get
			{
				return idString;
			}
		}

		public Menu ParentMenu
		{
			get
			{
				return parentMenu;
			}
		}

		public virtual void Declare()
		{
			linkedUiID = 0;
			fontScaleFactor = 2f;
			fontColor = Color.white;
			fontHighlightColor = Color.white;
			highlightTexture = null;
			orientation = ElementOrientation.Vertical;
			positionType = AC_PositionType2.Aligned;
			sizeType = AC_SizeType.Automatic;
			gridWidth = 3;
			lineID = -1;
			hoverSound = null;
			clickSound = null;
			dragOffset = Vector2.zero;
			changeCursor = false;
			cursorID = 0;
			alternativeInputButton = string.Empty;
		}

		public virtual MenuElement DuplicateSelf(bool fromEditor, bool ignoreUnityUI)
		{
			return null;
		}

		public virtual void Copy(MenuElement _element)
		{
			linkedUiID = _element.linkedUiID;
			ID = _element.ID;
			title = _element.title;
			slotSize = _element.slotSize;
			sizeType = _element.sizeType;
			positionType = _element.positionType;
			relativeRect = _element.relativeRect;
			numSlots = _element.numSlots;
			lineID = _element.lineID;
			slotSpacing = _element.slotSpacing;
			font = _element.font;
			fontScaleFactor = _element.fontScaleFactor;
			fontColor = _element.fontColor;
			fontHighlightColor = _element.fontHighlightColor;
			highlightTexture = _element.highlightTexture;
			isVisible = _element.isVisible;
			isClickable = _element.isClickable;
			orientation = _element.orientation;
			gridWidth = _element.gridWidth;
			backgroundTexture = _element.backgroundTexture;
			singleSlotBackgrounds = _element.singleSlotBackgrounds;
			hoverSound = _element.hoverSound;
			clickSound = _element.clickSound;
			relativePosition = _element.relativePosition;
			dragOffset = Vector2.zero;
			changeCursor = _element.changeCursor;
			cursorID = _element.cursorID;
			alternativeInputButton = _element.alternativeInputButton;
			idString = ID.ToString();
		}

		public virtual void Initialise(Menu _menu)
		{
			parentMenu = _menu;
		}

		public virtual void LoadUnityUI(Menu _menu, Canvas canvas)
		{
		}

		protected void CreateUIEvent(UnityEngine.UI.Button uiButton, Menu _menu, UIPointerState uiPointerState = UIPointerState.PointerClick, int _slotIndex = 0, bool liveState = true)
		{
			if (uiPointerState == UIPointerState.PointerClick)
			{
				uiButton.onClick.AddListener(delegate
				{
					ProcessClickUI(_menu, _slotIndex, (!liveState) ? MouseState.SingleClick : KickStarter.playerInput.GetMouseState());
				});
				return;
			}
			EventTrigger eventTrigger = uiButton.gameObject.GetComponent<EventTrigger>();
			if (eventTrigger == null)
			{
				eventTrigger = uiButton.gameObject.AddComponent<EventTrigger>();
			}
			EventTrigger.Entry entry = new EventTrigger.Entry();
			switch (uiPointerState)
			{
			case UIPointerState.PointerDown:
				entry.eventID = EventTriggerType.PointerDown;
				break;
			case UIPointerState.PointerEnter:
				entry.eventID = EventTriggerType.PointerEnter;
				break;
			}
			entry.callback.AddListener(delegate
			{
				ProcessClickUI(_menu, _slotIndex, (!liveState) ? MouseState.SingleClick : KickStarter.playerInput.GetMouseState());
			});
			eventTrigger.triggers.Add(entry);
		}

		protected void ProcessClickUI(Menu _menu, int _slot, MouseState _mouseState)
		{
			KickStarter.playerInput.ResetClick();
			ProcessClick(_menu, _slot, _mouseState);
		}

		public virtual void ProcessClick(Menu _menu, int _slot, MouseState _mouseState)
		{
			if (clickSound != null && KickStarter.sceneSettings != null)
			{
				KickStarter.sceneSettings.PlayDefaultSound(clickSound, false);
			}
			KickStarter.eventManager.Call_OnMenuElementClick(_menu, this, _slot, (int)_mouseState);
		}

		public virtual void ProcessContinuousClick(Menu _menu, MouseState _mouseState)
		{
		}

		public virtual GameObject GetObjectToSelect(int slotIndex = 0)
		{
			return null;
		}

		public virtual RectTransform GetRectTransform(int _slot)
		{
			return null;
		}

		public virtual void SetSpeech(Speech _speech)
		{
		}

		public virtual void ClearSpeech()
		{
		}

		public void UpdateID(int[] idArray)
		{
			foreach (int num in idArray)
			{
				if (ID == num)
				{
					ID++;
				}
			}
		}

		protected string TranslateLabel(string label, int languageNumber)
		{
			if (languageNumber == 0)
			{
				return label;
			}
			return KickStarter.runtimeLanguages.GetTranslation(label, lineID, languageNumber);
		}

		public virtual string GetLabel(int slot, int languageNumber)
		{
			return string.Empty;
		}

		public virtual bool IsSelectedByEventSystem(int slotIndex)
		{
			return false;
		}

		public virtual void HideAllUISlots()
		{
		}

		protected void LimitUISlotVisibility(UISlot[] uiSlots, int _numSlots, UIHideStyle uiHideStyle, Texture emptyTexture = null)
		{
			if (uiSlots == null || (!isVisible && _numSlots > 0))
			{
				return;
			}
			for (int i = 0; i < uiSlots.Length; i++)
			{
				if (i < _numSlots)
				{
					uiSlots[i].ShowUIElement(uiHideStyle);
				}
				else
				{
					uiSlots[i].HideUIElement(uiHideStyle);
				}
			}
		}

		public virtual string GetHotspotLabelOverride(int _slot, int _language)
		{
			return string.Empty;
		}

		public virtual void PreDisplay(int _slot, int languageNumber, bool isActive)
		{
		}

		public virtual void Display(GUIStyle _style, int _slot, float zoom, bool isActive)
		{
			if (backgroundTexture != null)
			{
				if (singleSlotBackgrounds)
				{
					Rect slotRectRelative = GetSlotRectRelative(_slot);
					GUI.DrawTexture(ZoomRect(slotRectRelative, zoom), backgroundTexture, ScaleMode.StretchToFill, true, 0f);
				}
				else if (_slot == 0)
				{
					GUI.DrawTexture(ZoomRect(relativeRect, zoom), backgroundTexture, ScaleMode.StretchToFill, true, 0f);
				}
			}
		}

		public virtual void DrawOutline(bool isSelected, Menu _menu)
		{
			Color color = Color.yellow;
			if (isSelected)
			{
				color = Color.red;
			}
			for (int i = 0; i < GetNumSlots(); i++)
			{
				if (i > 0)
				{
					color = Color.blue;
				}
				Rect rectAbsolute = _menu.GetRectAbsolute(GetSlotRectRelative(i));
				DrawStraightLine.DrawBox(rectAbsolute, color, 1f, false, 0);
			}
		}

		public Vector2[] GetSlotCentres(Menu _menu)
		{
			List<Vector2> list = new List<Vector2>();
			if (isClickable)
			{
				for (int i = 0; i < GetNumSlots(); i++)
				{
					Vector2 center = _menu.GetRectAbsolute(GetSlotRectRelative(i)).center;
					list.Add(center);
				}
			}
			return list.ToArray();
		}

		protected Rect ZoomRect(Rect rect, float zoom)
		{
			if (Mathf.Approximately(zoom, 1f))
			{
				if (!Application.isPlaying)
				{
					dragOffset = Vector2.zero;
				}
				if (dragOffset != Vector2.zero)
				{
					rect.x += dragOffset.x;
					rect.y += dragOffset.y;
				}
				return rect;
			}
			return new Rect(rect.x * zoom, rect.y * zoom, rect.width * zoom, rect.height * zoom);
		}

		protected void LimitOffset(int maxValue)
		{
			if (offset > 0 && numSlots + offset > maxValue)
			{
				offset = maxValue - numSlots;
			}
			if (offset < 0)
			{
				offset = 0;
			}
		}

		protected void Shift(AC_ShiftInventory shiftType, int maxSlots, int arraySize, int amount)
		{
			switch (shiftType)
			{
			case AC_ShiftInventory.ShiftNext:
				offset += amount;
				if (maxSlots + offset >= arraySize)
				{
					offset = arraySize - maxSlots;
				}
				break;
			case AC_ShiftInventory.ShiftPrevious:
				if (offset > 0)
				{
					offset -= amount;
					if (offset < 0)
					{
						offset = 0;
					}
				}
				break;
			}
			KickStarter.eventManager.Call_OnMenuElementShift(this, shiftType);
		}

		public virtual void Shift(AC_ShiftInventory shiftType, int amount)
		{
			ACDebug.LogWarning("The MenuElement " + title + " cannot be 'Shifted'");
		}

		public virtual bool CanBeShifted(AC_ShiftInventory shiftType)
		{
			return true;
		}

		public Vector2 GetSize()
		{
			return new Vector2(relativeRect.width, relativeRect.height);
		}

		public Vector2 GetSizeFromCorner()
		{
			return new Vector2(relativeRect.width + relativeRect.x, relativeRect.height + relativeRect.y);
		}

		public void SetSize(Vector2 _size)
		{
			slotSize = new Vector2(_size.x, _size.y);
		}

		protected void SetAbsoluteSize(Vector2 _size)
		{
			Vector2 vector = ((!(KickStarter.mainCamera != null)) ? new Vector2(ACScreen.width, ACScreen.height) : KickStarter.mainCamera.GetPlayableScreenArea(false).size);
			slotSize = new Vector2(_size.x * 100f / vector.x, _size.y * 100f / vector.y);
		}

		public int GetNumSlots()
		{
			return numSlots;
		}

		public Rect GetSlotRectRelative(int _slot)
		{
			Vector2 vector = Vector2.one;
			if (sizeType != AC_SizeType.AbsolutePixels)
			{
				vector = new Vector2(KickStarter.mainCamera.GetPlayableScreenArea(false).size.x / 100f, KickStarter.mainCamera.GetPlayableScreenArea(false).size.y / 100f);
			}
			Rect result = relativeRect;
			result.width = slotSize.x * vector.x;
			result.height = slotSize.y * vector.y;
			if (_slot > numSlots)
			{
				_slot = numSlots;
			}
			if (orientation == ElementOrientation.Horizontal)
			{
				result.x += (slotSize.x + slotSpacing) * (float)_slot * vector.x;
			}
			else if (orientation == ElementOrientation.Vertical)
			{
				result.y += (slotSize.y + slotSpacing) * (float)_slot * vector.y;
			}
			else if (orientation == ElementOrientation.Grid)
			{
				int num = _slot + 1;
				float num2 = Mathf.CeilToInt((float)num / (float)gridWidth) - 1;
				while (num > gridWidth)
				{
					num -= gridWidth;
				}
				num--;
				result.x += (slotSize.x + slotSpacing) * (float)num * vector.x;
				result.y += (slotSize.y + slotSpacing) * num2 * vector.y;
			}
			return result;
		}

		public virtual void RecalculateSize(MenuSource source)
		{
			if (source != MenuSource.AdventureCreator)
			{
				return;
			}
			dragOffset = Vector2.zero;
			Vector2 vector = Vector2.one;
			if (sizeType == AC_SizeType.Automatic)
			{
				AutoSize();
			}
			if (sizeType != AC_SizeType.AbsolutePixels)
			{
				vector = ((!(KickStarter.mainCamera != null)) ? new Vector2((float)ACScreen.width / 100f, (float)ACScreen.height / 100f) : new Vector2(KickStarter.mainCamera.GetPlayableScreenArea(false).size.x / 100f, KickStarter.mainCamera.GetPlayableScreenArea(false).size.y / 100f));
			}
			switch (orientation)
			{
			case ElementOrientation.Horizontal:
				relativeRect.width = slotSize.x * vector.x * (float)numSlots;
				relativeRect.height = slotSize.y * vector.y;
				if (numSlots > 1)
				{
					relativeRect.width += slotSpacing * vector.x * (float)(numSlots - 1);
				}
				break;
			case ElementOrientation.Vertical:
				relativeRect.width = slotSize.x * vector.x;
				relativeRect.height = slotSize.y * vector.y * (float)numSlots;
				if (numSlots > 1)
				{
					relativeRect.height += slotSpacing * vector.y * (float)(numSlots - 1);
				}
				break;
			case ElementOrientation.Grid:
			{
				if (numSlots < gridWidth)
				{
					relativeRect.width = (slotSize.x + slotSpacing) * vector.x * (float)numSlots;
					relativeRect.height = slotSize.y * vector.y;
					break;
				}
				float num = Mathf.CeilToInt((float)numSlots / (float)gridWidth);
				relativeRect.width = slotSize.x * vector.x * (float)gridWidth;
				relativeRect.height = slotSize.y * vector.y * num;
				if (numSlots > 1)
				{
					relativeRect.width += slotSpacing * vector.x * (float)(gridWidth - 1);
					relativeRect.height += slotSpacing * vector.y * (num - 1f);
				}
				break;
			}
			}
		}

		public int GetFontSize()
		{
			if (sizeType == AC_SizeType.AbsolutePixels)
			{
				return (int)(fontScaleFactor * 10f);
			}
			float num = ((!(KickStarter.mainCamera != null)) ? ((float)ACScreen.width) : KickStarter.mainCamera.GetPlayableScreenArea(false).size.x);
			return (int)(num * fontScaleFactor / 100f);
		}

		protected void AutoSize(GUIContent content)
		{
			GUIStyle gUIStyle = new GUIStyle();
			gUIStyle.font = font;
			gUIStyle.fontSize = GetFontSize();
			if (string.IsNullOrEmpty(content.text))
			{
				Vector2 absoluteSize = gUIStyle.CalcSize(content);
				SetAbsoluteSize(absoluteSize);
				return;
			}
			Vector2 vector = gUIStyle.CalcSize(content);
			gUIStyle.wordWrap = true;
			float width = ACScreen.safeArea.width;
			float num = width / 2f;
			float y = gUIStyle.CalcHeight(content, num);
			Vector2 absoluteSize2 = new Vector2(num, y);
			if (absoluteSize2.y == vector.y && vector.x < absoluteSize2.x)
			{
				absoluteSize2.x = vector.x;
			}
			absoluteSize2.x += 2f;
			SetAbsoluteSize(absoluteSize2);
		}

		protected virtual void AutoSize()
		{
			GUIContent content = new GUIContent(backgroundTexture);
			AutoSize(content);
		}

		public void SetPosition(Vector2 _position)
		{
			relativeRect.x = _position.x;
			relativeRect.y = _position.y;
		}

		public void SetRelativePosition(Vector2 _size)
		{
			relativeRect.x = relativePosition.x * _size.x;
			relativeRect.y = relativePosition.y * _size.y;
		}

		public void ResetDragOffset()
		{
			dragOffset = Vector2.zero;
		}

		public void SetDragOffset(Vector2 pos, Rect dragRect)
		{
			if (pos.x < dragRect.x)
			{
				pos.x = dragRect.x;
			}
			else if (pos.x > dragRect.x + dragRect.width - relativeRect.width)
			{
				pos.x = dragRect.x + dragRect.width - relativeRect.width;
			}
			if (pos.y < dragRect.y)
			{
				pos.y = dragRect.y;
			}
			else if (pos.y > dragRect.y + dragRect.height - relativeRect.height)
			{
				pos.y = dragRect.y + dragRect.height - relativeRect.height;
			}
			dragOffset = pos;
		}

		public Vector2 GetDragStart()
		{
			return new Vector2(0f - dragOffset.x, dragOffset.y);
		}

		public void AutoSetVisibility()
		{
			if (numSlots == 0)
			{
				IsVisible = false;
			}
			else
			{
				IsVisible = true;
			}
		}

		protected T LinkUIElement<T>(Canvas canvas) where T : Behaviour
		{
			if (canvas != null)
			{
				T gameObjectComponent = Serializer.GetGameObjectComponent<T>(linkedUiID, canvas.gameObject);
				if (gameObjectComponent == null)
				{
					ACDebug.LogWarning("Cannot find linked UI Element for " + title, canvas);
				}
				return gameObjectComponent;
			}
			return (T)null;
		}

		protected void UpdateUISelectable<T>(T field, UISelectableHideStyle uiSelectableHideStyle) where T : Selectable
		{
			if (Application.isPlaying && field != null)
			{
				switch (uiSelectableHideStyle)
				{
				case UISelectableHideStyle.DisableObject:
					field.gameObject.SetActive(isVisible);
					break;
				case UISelectableHideStyle.DisableInteractability:
					field.interactable = isVisible;
					break;
				}
			}
		}

		protected void UpdateUIElement<T>(T field) where T : Behaviour
		{
			if (Application.isPlaying && field != null && field.gameObject.activeSelf != isVisible)
			{
				field.gameObject.SetActive(isVisible);
			}
		}

		protected void ClearSpriteCache(UISlot[] uiSlots)
		{
			foreach (UISlot uISlot in uiSlots)
			{
				uISlot.sprite = null;
			}
		}

		public virtual void SetUIInteractableState(bool state)
		{
		}

		protected void SetUISlotsInteractableState(UISlot[] uiSlots, bool state)
		{
			foreach (UISlot uISlot in uiSlots)
			{
				if ((bool)uISlot.uiButton)
				{
					uISlot.uiButton.interactable = state;
				}
			}
		}

		public virtual AudioClip GetHoverSound(int slot)
		{
			return hoverSound;
		}

		public int GetOffset()
		{
			return offset;
		}

		public void SetOffset(int value)
		{
			offset = value;
			LimitOffset(numSlots);
		}

		public virtual void OnMenuTurnOn(Menu menu)
		{
			parentMenu = menu;
		}
	}
}
