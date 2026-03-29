using UnityEngine;
using UnityEngine.UI;

namespace AC
{
	public class MenuToggle : MenuElement, ITranslatable
	{
		public Toggle uiToggle;

		public AC_ToggleType toggleType;

		public ActionListAsset actionListOnClick;

		public string label;

		public bool isOn;

		public TextEffects textEffects;

		public float outlineSize = 2f;

		public TextAnchor anchor;

		public int varID;

		public bool appendState = true;

		public Texture2D onTexture;

		public Texture2D offTexture;

		public UISelectableHideStyle uiSelectableHideStyle;

		public string onText = "On";

		public int onTextLineID = -1;

		public string offText = "Off";

		public int offTextLineID = -1;

		private Text uiText;

		private string fullText;

		public override void Declare()
		{
			uiToggle = null;
			uiText = null;
			label = "Toggle";
			isOn = false;
			isVisible = true;
			isClickable = true;
			toggleType = AC_ToggleType.CustomScript;
			numSlots = 1;
			varID = 0;
			SetSize(new Vector2(15f, 5f));
			anchor = TextAnchor.MiddleLeft;
			appendState = true;
			onTexture = null;
			offTexture = null;
			textEffects = TextEffects.None;
			outlineSize = 2f;
			actionListOnClick = null;
			uiSelectableHideStyle = UISelectableHideStyle.DisableObject;
			onText = "On";
			offText = "Off";
			onTextLineID = -1;
			offTextLineID = -1;
			base.Declare();
		}

		public override MenuElement DuplicateSelf(bool fromEditor, bool ignoreUnityUI)
		{
			MenuToggle menuToggle = ScriptableObject.CreateInstance<MenuToggle>();
			menuToggle.Declare();
			menuToggle.CopyToggle(this, ignoreUnityUI);
			return menuToggle;
		}

		private void CopyToggle(MenuToggle _element, bool ignoreUnityUI)
		{
			if (ignoreUnityUI)
			{
				uiToggle = null;
			}
			else
			{
				uiToggle = _element.uiToggle;
			}
			uiText = null;
			label = _element.label;
			isOn = _element.isOn;
			textEffects = _element.textEffects;
			outlineSize = _element.outlineSize;
			anchor = _element.anchor;
			toggleType = _element.toggleType;
			varID = _element.varID;
			appendState = _element.appendState;
			onTexture = _element.onTexture;
			offTexture = _element.offTexture;
			actionListOnClick = _element.actionListOnClick;
			uiSelectableHideStyle = _element.uiSelectableHideStyle;
			onText = _element.onText;
			offText = _element.offText;
			onTextLineID = _element.onTextLineID;
			offTextLineID = _element.offTextLineID;
			isClickable = _element.isClickable;
			base.Copy(_element);
		}

		public override void LoadUnityUI(Menu _menu, Canvas canvas)
		{
			uiToggle = LinkUIElement<Toggle>(canvas);
			if (!uiToggle)
			{
				return;
			}
			uiText = uiToggle.GetComponentInChildren<Text>();
			uiToggle.interactable = isClickable;
			if (isClickable)
			{
				uiToggle.onValueChanged.AddListener(delegate
				{
					ProcessClickUI(_menu, 0, KickStarter.playerInput.GetMouseState());
				});
			}
		}

		public override GameObject GetObjectToSelect(int slotIndex = 0)
		{
			if ((bool)uiToggle)
			{
				return uiToggle.gameObject;
			}
			return null;
		}

		public override RectTransform GetRectTransform(int _slot)
		{
			if ((bool)uiToggle)
			{
				return uiToggle.GetComponent<RectTransform>();
			}
			return null;
		}

		public override void SetUIInteractableState(bool state)
		{
			if ((bool)uiToggle)
			{
				uiToggle.interactable = state;
			}
		}

		public override void PreDisplay(int _slot, int languageNumber, bool isActive)
		{
			CalculateValue();
			fullText = TranslateLabel(label, languageNumber);
			if (appendState)
			{
				if (languageNumber == 0)
				{
					if (isOn)
					{
						fullText = fullText + " : " + onText;
					}
					else
					{
						fullText = fullText + " : " + offText;
					}
				}
				else if (isOn)
				{
					fullText = fullText + " : " + KickStarter.runtimeLanguages.GetTranslation(onText, onTextLineID, languageNumber);
				}
				else
				{
					fullText = fullText + " : " + KickStarter.runtimeLanguages.GetTranslation(offText, offTextLineID, languageNumber);
				}
			}
			if ((bool)uiToggle)
			{
				if ((bool)uiText)
				{
					uiText.text = fullText;
				}
				uiToggle.isOn = isOn;
				UpdateUISelectable(uiToggle, uiSelectableHideStyle);
			}
		}

		public override void Display(GUIStyle _style, int _slot, float zoom, bool isActive)
		{
			base.Display(_style, _slot, zoom, isActive);
			_style.alignment = anchor;
			if (zoom < 1f)
			{
				_style.fontSize = (int)((float)_style.fontSize * zoom);
			}
			Rect rect = ZoomRect(relativeRect, zoom);
			if (isOn && onTexture != null)
			{
				GUI.DrawTexture(rect, onTexture, ScaleMode.StretchToFill, true, 0f);
			}
			else if (!isOn && offTexture != null)
			{
				GUI.DrawTexture(rect, offTexture, ScaleMode.StretchToFill, true, 0f);
			}
			if (textEffects != TextEffects.None)
			{
				AdvGame.DrawTextEffect(rect, fullText, _style, Color.black, _style.normal.textColor, outlineSize, textEffects);
			}
			else
			{
				GUI.Label(rect, fullText, _style);
			}
		}

		public override string GetLabel(int slot, int languageNumber)
		{
			if (appendState)
			{
				if (isOn)
				{
					return TranslateLabel(label, languageNumber) + " : " + KickStarter.runtimeLanguages.GetTranslation(onText, onTextLineID, languageNumber);
				}
				return TranslateLabel(label, languageNumber) + " : " + KickStarter.runtimeLanguages.GetTranslation(offText, offTextLineID, languageNumber);
			}
			return TranslateLabel(label, languageNumber);
		}

		public override bool IsSelectedByEventSystem(int slotIndex)
		{
			if (uiToggle != null)
			{
				return KickStarter.playerMenus.IsEventSystemSelectingObject(uiToggle.gameObject);
			}
			return false;
		}

		public override void ProcessClick(Menu _menu, int _slot, MouseState _mouseState)
		{
			if (!_menu.IsClickable())
			{
				return;
			}
			if (uiToggle != null)
			{
				isOn = uiToggle.isOn;
			}
			else if (isOn)
			{
				isOn = false;
			}
			else
			{
				isOn = true;
			}
			if (toggleType == AC_ToggleType.Subtitles)
			{
				Options.SetSubtitles(isOn);
			}
			else if (toggleType == AC_ToggleType.Variable && varID >= 0)
			{
				GVar variable = GlobalVariables.GetVariable(varID);
				if (variable.type == VariableType.Boolean)
				{
					if (isOn)
					{
						variable.val = 1;
					}
					else
					{
						variable.val = 0;
					}
					variable.Upload();
				}
			}
			if (toggleType == AC_ToggleType.CustomScript)
			{
				MenuSystem.OnElementClick(_menu, this, _slot, (int)_mouseState);
			}
			if ((bool)actionListOnClick)
			{
				AdvGame.RunActionListAsset(actionListOnClick);
			}
			base.ProcessClick(_menu, _slot, _mouseState);
		}

		private void CalculateValue()
		{
			if (!Application.isPlaying)
			{
				return;
			}
			if (toggleType == AC_ToggleType.Subtitles)
			{
				if (Options.optionsData != null)
				{
					isOn = Options.optionsData.showSubtitles;
				}
			}
			else
			{
				if (toggleType != AC_ToggleType.Variable || varID < 0)
				{
					return;
				}
				GVar variable = GlobalVariables.GetVariable(varID);
				if (variable != null && variable.type == VariableType.Boolean)
				{
					if (variable.val == 1)
					{
						isOn = true;
					}
					else
					{
						isOn = false;
					}
					return;
				}
				ACDebug.LogWarning("Cannot link MenuToggle " + title + " to Variable " + varID + " as it is not a Boolean.");
			}
		}

		protected override void AutoSize()
		{
			int language = Options.GetLanguage();
			if (appendState)
			{
				AutoSize(new GUIContent(TranslateLabel(label, language) + " : Off"));
			}
			else
			{
				AutoSize(new GUIContent(TranslateLabel(label, language)));
			}
		}

		public string GetTranslatableString(int index)
		{
			switch (index)
			{
			case 0:
				return label;
			case 1:
				return onText;
			default:
				return offText;
			}
		}

		public int GetTranslationID(int index)
		{
			switch (index)
			{
			case 0:
				return lineID;
			case 1:
				return onTextLineID;
			default:
				return offTextLineID;
			}
		}
	}
}
