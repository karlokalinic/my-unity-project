using UnityEngine;
using UnityEngine.UI;

namespace AC
{
	public class MenuInput : MenuElement, ITranslatable
	{
		public InputField uiInput;

		public string label = "Element";

		public TextAnchor anchor;

		public TextEffects textEffects;

		public float outlineSize = 2f;

		public AC_InputType inputType;

		public int characterLimit = 10;

		public string linkedButton = string.Empty;

		public bool allowSpaces;

		public UISelectableHideStyle uiSelectableHideStyle;

		public bool requireSelection;

		private bool isSelected;

		public override void Declare()
		{
			uiInput = null;
			label = "Input";
			isVisible = true;
			isClickable = true;
			numSlots = 1;
			anchor = TextAnchor.MiddleCenter;
			SetSize(new Vector2(10f, 5f));
			inputType = AC_InputType.AlphaNumeric;
			characterLimit = 10;
			linkedButton = string.Empty;
			textEffects = TextEffects.None;
			outlineSize = 2f;
			allowSpaces = false;
			uiSelectableHideStyle = UISelectableHideStyle.DisableObject;
			requireSelection = false;
			base.Declare();
		}

		public override MenuElement DuplicateSelf(bool fromEditor, bool ignoreUnityUI)
		{
			MenuInput menuInput = ScriptableObject.CreateInstance<MenuInput>();
			menuInput.Declare();
			menuInput.CopyInput(this, ignoreUnityUI);
			return menuInput;
		}

		private void CopyInput(MenuInput _element, bool ignoreUnityUI)
		{
			if (ignoreUnityUI)
			{
				uiInput = null;
			}
			else
			{
				uiInput = _element.uiInput;
			}
			label = _element.label;
			anchor = _element.anchor;
			textEffects = _element.textEffects;
			outlineSize = _element.outlineSize;
			inputType = _element.inputType;
			characterLimit = _element.characterLimit;
			linkedButton = _element.linkedButton;
			allowSpaces = _element.allowSpaces;
			uiSelectableHideStyle = _element.uiSelectableHideStyle;
			requireSelection = _element.requireSelection;
			base.Copy(_element);
		}

		public override void LoadUnityUI(Menu _menu, Canvas canvas)
		{
			uiInput = LinkUIElement<InputField>(canvas);
		}

		public override RectTransform GetRectTransform(int _slot)
		{
			if (uiInput != null)
			{
				return uiInput.GetComponent<RectTransform>();
			}
			return null;
		}

		public override void SetUIInteractableState(bool state)
		{
			if (uiInput != null)
			{
				uiInput.interactable = state;
			}
		}

		public override GameObject GetObjectToSelect(int slotIndex = 0)
		{
			if (uiInput != null)
			{
				return uiInput.gameObject;
			}
			return null;
		}

		public string GetContents()
		{
			if (uiInput != null)
			{
				if (uiInput.textComponent != null)
				{
					return uiInput.textComponent.text;
				}
				ACDebug.LogWarning(uiInput.gameObject.name + " has no Text component");
			}
			return label;
		}

		public void SetLabel(string _label)
		{
			label = _label;
			if (uiInput != null && uiInput.textComponent != null)
			{
				uiInput.text = _label;
			}
		}

		public override void PreDisplay(int _slot, int languageNumber, bool isActive)
		{
			if (uiInput != null)
			{
				UpdateUISelectable(uiInput, uiSelectableHideStyle);
			}
		}

		public override void Display(GUIStyle _style, int _slot, float zoom, bool isActive)
		{
			base.Display(_style, _slot, zoom, isActive);
			string text = label;
			if (Application.isPlaying && (isSelected || isActive))
			{
				text = AdvGame.CombineLanguageString(text, "|", Options.GetLanguage(), false);
			}
			_style.wordWrap = true;
			_style.alignment = anchor;
			if (zoom < 1f)
			{
				_style.fontSize = (int)((float)_style.fontSize * zoom);
			}
			if (textEffects != TextEffects.None)
			{
				AdvGame.DrawTextEffect(ZoomRect(relativeRect, zoom), text, _style, Color.black, _style.normal.textColor, outlineSize, textEffects);
			}
			else
			{
				GUI.Label(ZoomRect(relativeRect, zoom), text, _style);
			}
		}

		public override string GetLabel(int slot, int languageNumber)
		{
			return TranslateLabel(label, languageNumber);
		}

		public override bool IsSelectedByEventSystem(int slotIndex)
		{
			if (uiInput != null)
			{
				return KickStarter.playerMenus.IsEventSystemSelectingObject(uiInput.gameObject);
			}
			return false;
		}

		private void ProcessReturn(string input, string menuName)
		{
			switch (input)
			{
			case "KeypadEnter":
			case "Return":
			case "Enter":
				if (linkedButton != string.Empty && menuName != string.Empty)
				{
					PlayerMenus.SimulateClick(menuName, PlayerMenus.GetElementWithName(menuName, linkedButton));
				}
				break;
			}
		}

		public void CheckForInput(string keycode, string character, bool shift, string menuName)
		{
			if (uiInput != null)
			{
				return;
			}
			string text = keycode;
			if (inputType == AC_InputType.AllowSpecialCharacters)
			{
				switch (text)
				{
				default:
					text = character;
					break;
				case "KeypadEnter":
				case "Return":
				case "Enter":
				case "Backspace":
					break;
				}
			}
			bool flag = KickStarter.runtimeLanguages.LanguageReadsRightToLeft(Options.GetLanguage());
			isSelected = true;
			switch (text)
			{
			case "Backspace":
				if (label.Length > 1)
				{
					if (flag)
					{
						label = label.Substring(1, label.Length - 1);
					}
					else
					{
						label = label.Substring(0, label.Length - 1);
					}
				}
				else if (label.Length == 1)
				{
					label = string.Empty;
				}
				return;
			case "KeypadEnter":
			case "Return":
			case "Enter":
				ProcessReturn(text, menuName);
				return;
			}
			if ((inputType != AC_InputType.AlphaNumeric || (text.Length != 1 && !text.Contains("Alpha"))) && (inputType != AC_InputType.NumbericOnly || !text.Contains("Alpha")) && (inputType != AC_InputType.AlphaNumeric || !allowSpaces || !(text == "Space")) && (inputType != AC_InputType.AllowSpecialCharacters || (text.Length != 1 && !(text == "Space"))))
			{
				return;
			}
			text = text.Replace("Alpha", string.Empty);
			text = text.Replace("Space", " ");
			if (inputType != AC_InputType.AllowSpecialCharacters)
			{
				text = ((!shift) ? text.ToLower() : text.ToUpper());
			}
			if (characterLimit == 1)
			{
				label = text;
			}
			else if (label.Length < characterLimit)
			{
				if (flag)
				{
					label = text + label;
				}
				else
				{
					label += text;
				}
			}
		}

		public override void RecalculateSize(MenuSource source)
		{
			if (source == MenuSource.AdventureCreator)
			{
				Deselect();
			}
			base.RecalculateSize(source);
		}

		public void Deselect()
		{
			isSelected = false;
		}

		public override void ProcessClick(Menu _menu, int _slot, MouseState _mouseState)
		{
			if (_menu.IsClickable())
			{
				KickStarter.playerMenus.SelectInputBox(this);
				base.ProcessClick(_menu, _slot, _mouseState);
			}
		}

		protected override void AutoSize()
		{
			GUIContent content = new GUIContent(TranslateLabel(label, Options.GetLanguage()));
			AutoSize(content);
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
