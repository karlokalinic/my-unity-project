using System;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class MenuDrag : MenuElement, ITranslatable
	{
		public string label = "Element";

		public TextAnchor anchor;

		public TextEffects textEffects;

		public Rect dragRect;

		public DragElementType dragType;

		public string elementName;

		private Vector2 dragStartPosition;

		private Menu menuToDrag;

		private MenuElement elementToDrag;

		private string fullText;

		public override void Declare()
		{
			label = "Button";
			isVisible = true;
			isClickable = true;
			textEffects = TextEffects.None;
			numSlots = 1;
			anchor = TextAnchor.MiddleCenter;
			SetSize(new Vector2(10f, 5f));
			dragRect = new Rect(0f, 0f, 0f, 0f);
			dragType = DragElementType.EntireMenu;
			elementName = string.Empty;
			base.Declare();
		}

		public override MenuElement DuplicateSelf(bool fromEditor, bool ignoreUnityUI)
		{
			MenuDrag menuDrag = ScriptableObject.CreateInstance<MenuDrag>();
			menuDrag.Declare();
			menuDrag.CopyDrag(this);
			return menuDrag;
		}

		private void CopyDrag(MenuDrag _element)
		{
			label = _element.label;
			anchor = _element.anchor;
			textEffects = _element.textEffects;
			dragRect = _element.dragRect;
			dragType = _element.dragType;
			elementName = _element.elementName;
			base.Copy(_element);
		}

		public override void PreDisplay(int _slot, int languageNumber, bool isActive)
		{
			fullText = TranslateLabel(label, languageNumber);
		}

		public override void Display(GUIStyle _style, int _slot, float zoom, bool isActive)
		{
			base.Display(_style, _slot, zoom, isActive);
			_style.alignment = anchor;
			if (zoom < 1f)
			{
				_style.fontSize = (int)((float)_style.fontSize * zoom);
			}
			if (textEffects != TextEffects.None)
			{
				AdvGame.DrawTextEffect(ZoomRect(relativeRect, zoom), fullText, _style, Color.black, _style.normal.textColor, 2f, textEffects);
			}
			else
			{
				GUI.Label(ZoomRect(relativeRect, zoom), fullText, _style);
			}
		}

		public override string GetLabel(int slot, int languageNumber)
		{
			return TranslateLabel(label, languageNumber);
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

		private void StartDrag(Menu _menu)
		{
			menuToDrag = _menu;
			if (dragType == DragElementType.SingleElement)
			{
				if (elementName != string.Empty)
				{
					MenuElement elementWithName = PlayerMenus.GetElementWithName(_menu.title, elementName);
					if (elementWithName == null)
					{
						ACDebug.LogWarning("Cannot drag " + elementName + " as it cannot be found on " + _menu.title);
						return;
					}
					if (elementWithName.positionType == AC_PositionType2.Aligned)
					{
						ACDebug.LogWarning("Cannot drag " + elementName + " as its Position is set to Aligned");
						return;
					}
					if (_menu.sizeType == AC_SizeType.Automatic)
					{
						ACDebug.LogWarning("Cannot drag " + elementName + " as its parent Menu's Size is set to Automatic");
						return;
					}
					elementToDrag = elementWithName;
					dragStartPosition = elementToDrag.GetDragStart();
				}
			}
			else
			{
				dragStartPosition = _menu.GetDragStart();
			}
		}

		public bool DoDrag(Vector2 _dragVector)
		{
			if (dragType == DragElementType.EntireMenu)
			{
				if (menuToDrag == null)
				{
					return false;
				}
				if (!menuToDrag.IsEnabled() || menuToDrag.IsFading())
				{
					return false;
				}
			}
			if (elementToDrag == null && dragType == DragElementType.SingleElement)
			{
				return false;
			}
			Rect rect = dragRect;
			if (sizeType != AC_SizeType.AbsolutePixels)
			{
				Vector2 size = KickStarter.mainCamera.GetPlayableScreenArea(false).size;
				rect = new Rect(dragRect.x * size.x / 100f, dragRect.y * size.y / 100f, dragRect.width * size.x / 100f, dragRect.height * size.y / 100f);
			}
			if (dragType == DragElementType.EntireMenu)
			{
				menuToDrag.SetDragOffset(_dragVector + dragStartPosition, rect);
			}
			else if (dragType == DragElementType.SingleElement)
			{
				elementToDrag.SetDragOffset(_dragVector + dragStartPosition, rect);
			}
			return true;
		}

		public bool CheckStop(Vector2 mousePosition)
		{
			if (menuToDrag == null)
			{
				return false;
			}
			if (dragType == DragElementType.EntireMenu && !menuToDrag.IsPointerOverSlot(this, 0, mousePosition))
			{
				return true;
			}
			if (dragType == DragElementType.SingleElement && elementToDrag != null && !menuToDrag.IsPointerOverSlot(this, 0, mousePosition))
			{
				return true;
			}
			return false;
		}

		private Rect GetDragRectRelative()
		{
			Rect result = dragRect;
			if (sizeType != AC_SizeType.AbsolutePixels)
			{
				result.x = dragRect.x / 100f * KickStarter.mainCamera.GetPlayableScreenArea(false).width;
				result.y = dragRect.y / 100f * KickStarter.mainCamera.GetPlayableScreenArea(false).height;
				result.width = dragRect.width / 100f * KickStarter.mainCamera.GetPlayableScreenArea(false).width;
				result.height = dragRect.height / 100f * KickStarter.mainCamera.GetPlayableScreenArea(false).height;
			}
			return result;
		}

		public override void ProcessClick(Menu _menu, int _slot, MouseState _mouseState)
		{
			if (_mouseState == MouseState.SingleClick)
			{
				StartDrag(_menu);
				KickStarter.playerInput.SetActiveDragElement(this);
				base.ProcessClick(_menu, _slot, _mouseState);
			}
		}

		public string GetTranslatableString(int index)
		{
			return label;
		}

		public int GetTranslationID(int index)
		{
			return lineID;
		}
	}
}
