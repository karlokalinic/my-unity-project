using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AC
{
	public class MenuCycle : MenuElement, ITranslatable
	{
		public UnityEngine.UI.Button uiButton;

		public Dropdown uiDropdown;

		public ActionListAsset actionListOnClick;

		public string label = "Element";

		public TextEffects textEffects;

		public float outlineSize = 2f;

		public TextAnchor anchor;

		public int selected;

		public List<string> optionsArray = new List<string>();

		public AC_CycleType cycleType;

		public SplitLanguageType splitLanguageType;

		public int varID;

		public UISelectableHideStyle uiSelectableHideStyle;

		public CycleUIBasis cycleUIBasis;

		public Texture2D[] optionTextures = new Texture2D[0];

		private RawImage rawImage;

		private GVar linkedVariable;

		private string cycleText;

		private Text uiText;

		public override void Declare()
		{
			uiText = null;
			uiButton = null;
			label = "Cycle";
			selected = 0;
			isVisible = true;
			isClickable = true;
			numSlots = 1;
			textEffects = TextEffects.None;
			outlineSize = 2f;
			SetSize(new Vector2(15f, 5f));
			anchor = TextAnchor.MiddleLeft;
			cycleType = AC_CycleType.CustomScript;
			splitLanguageType = SplitLanguageType.TextAndVoice;
			varID = 0;
			optionsArray = new List<string>();
			cycleText = string.Empty;
			actionListOnClick = null;
			uiSelectableHideStyle = UISelectableHideStyle.DisableObject;
			cycleUIBasis = CycleUIBasis.Button;
			optionTextures = new Texture2D[0];
			rawImage = null;
			linkedVariable = null;
			uiDropdown = null;
			base.Declare();
		}

		public override MenuElement DuplicateSelf(bool fromEditor, bool ignoreUnityUI)
		{
			MenuCycle menuCycle = ScriptableObject.CreateInstance<MenuCycle>();
			menuCycle.Declare();
			menuCycle.CopyCycle(this, ignoreUnityUI);
			return menuCycle;
		}

		private void CopyCycle(MenuCycle _element, bool ignoreUnityUI)
		{
			if (ignoreUnityUI)
			{
				uiButton = null;
			}
			else
			{
				uiButton = _element.uiButton;
			}
			uiText = null;
			label = _element.label;
			textEffects = _element.textEffects;
			outlineSize = _element.outlineSize;
			anchor = _element.anchor;
			selected = _element.selected;
			optionsArray = _element.optionsArray;
			cycleType = _element.cycleType;
			splitLanguageType = _element.splitLanguageType;
			varID = _element.varID;
			cycleText = string.Empty;
			actionListOnClick = _element.actionListOnClick;
			uiSelectableHideStyle = _element.uiSelectableHideStyle;
			cycleUIBasis = _element.cycleUIBasis;
			optionTextures = _element.optionTextures;
			linkedVariable = null;
			uiDropdown = _element.uiDropdown;
			base.Copy(_element);
		}

		public override void LoadUnityUI(Menu _menu, Canvas canvas)
		{
			rawImage = null;
			if (_menu.menuSource == MenuSource.AdventureCreator)
			{
				return;
			}
			if (cycleUIBasis == CycleUIBasis.Button)
			{
				uiButton = LinkUIElement<UnityEngine.UI.Button>(canvas);
				if ((bool)uiButton)
				{
					rawImage = uiButton.GetComponentInChildren<RawImage>();
					uiText = uiButton.GetComponentInChildren<Text>();
					uiButton.onClick.AddListener(delegate
					{
						ProcessClickUI(_menu, 0, KickStarter.playerInput.GetMouseState());
					});
				}
			}
			else
			{
				if (cycleUIBasis != CycleUIBasis.Dropdown)
				{
					return;
				}
				uiDropdown = LinkUIElement<Dropdown>(canvas);
				if (uiDropdown != null)
				{
					uiDropdown.value = selected;
					uiDropdown.onValueChanged.AddListener(delegate
					{
						uiDropdownValueChangedHandler(uiDropdown);
					});
				}
			}
		}

		private void uiDropdownValueChangedHandler(Dropdown _dropdown)
		{
			ProcessClickUI(parentMenu, 0, KickStarter.playerInput.GetMouseState());
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
			CalculateValue();
			cycleText = TranslateLabel(label, languageNumber) + " : ";
			if (Application.isPlaying && uiDropdown != null)
			{
				cycleText = string.Empty;
			}
			cycleText += GetOptionLabel(selected);
			if (Application.isPlaying && optionTextures.Length > 0 && selected < optionTextures.Length && optionTextures[selected] != null)
			{
				backgroundTexture = optionTextures[selected];
				if (rawImage != null)
				{
					rawImage.texture = backgroundTexture;
				}
			}
			if ((bool)uiButton)
			{
				if ((bool)uiText)
				{
					uiText.text = cycleText;
				}
				UpdateUISelectable(uiButton, uiSelectableHideStyle);
			}
			else if (uiDropdown != null && Application.isPlaying)
			{
				uiDropdown.value = selected;
				UpdateUISelectable(uiDropdown, uiSelectableHideStyle);
			}
		}

		private string GetOptionLabel(int index)
		{
			if (index >= 0 && index < GetNumOptions())
			{
				if (cycleType == AC_CycleType.Variable && linkedVariable != null && linkedVariable.type == VariableType.PopUp)
				{
					return linkedVariable.GetPopUpForIndex(index, Options.GetLanguage());
				}
				return optionsArray[index];
			}
			if (Application.isPlaying)
			{
				ACDebug.Log("Could not gather options for MenuCycle " + label);
				return string.Empty;
			}
			return "Default option";
		}

		private int GetNumOptions()
		{
			if (!Application.isPlaying && cycleType == AC_CycleType.Variable && (linkedVariable == null || linkedVariable.id != varID) && AdvGame.GetReferences().variablesManager != null)
			{
				linkedVariable = AdvGame.GetReferences().variablesManager.GetVariable(varID);
			}
			if (cycleType == AC_CycleType.Variable && linkedVariable != null && linkedVariable.type == VariableType.PopUp)
			{
				return linkedVariable.GetNumPopUpValues();
			}
			return optionsArray.Count;
		}

		private void CycleOption()
		{
			selected++;
			if (selected >= GetNumOptions())
			{
				selected = 0;
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
			if (textEffects != TextEffects.None)
			{
				AdvGame.DrawTextEffect(ZoomRect(relativeRect, zoom), cycleText, _style, Color.black, _style.normal.textColor, outlineSize, textEffects);
			}
			else
			{
				GUI.Label(ZoomRect(relativeRect, zoom), cycleText, _style);
			}
		}

		public override string GetLabel(int slot, int languageNumber)
		{
			string optionLabel = GetOptionLabel(selected);
			if (!string.IsNullOrEmpty(optionLabel))
			{
				return TranslateLabel(label, languageNumber) + " : " + optionLabel;
			}
			return TranslateLabel(label, languageNumber);
		}

		public override bool IsSelectedByEventSystem(int slotIndex)
		{
			if (uiButton != null)
			{
				return KickStarter.playerMenus.IsEventSystemSelectingObject(uiButton.gameObject);
			}
			return false;
		}

		public override void ProcessClick(Menu _menu, int _slot, MouseState _mouseState)
		{
			if (!_menu.IsClickable())
			{
				return;
			}
			if (uiDropdown != null)
			{
				selected = uiDropdown.value;
			}
			else
			{
				CycleOption();
			}
			if (cycleType == AC_CycleType.Language)
			{
				if (selected == 0 && KickStarter.speechManager.ignoreOriginalText && KickStarter.runtimeLanguages.Languages.Count > 1)
				{
					selected = 1;
				}
				if (KickStarter.speechManager != null && KickStarter.speechManager.separateVoiceAndTextLanguages)
				{
					switch (splitLanguageType)
					{
					case SplitLanguageType.TextAndVoice:
						Options.SetLanguage(selected);
						Options.SetVoiceLanguage(selected);
						break;
					case SplitLanguageType.TextOnly:
						Options.SetLanguage(selected);
						break;
					case SplitLanguageType.VoiceOnly:
						Options.SetVoiceLanguage(selected);
						break;
					}
				}
				else
				{
					Options.SetLanguage(selected);
				}
			}
			else if (cycleType == AC_CycleType.Variable && linkedVariable != null)
			{
				linkedVariable.IntegerValue = selected;
				linkedVariable.Upload();
			}
			if (cycleType == AC_CycleType.CustomScript)
			{
				MenuSystem.OnElementClick(_menu, this, _slot, (int)_mouseState);
			}
			if ((bool)actionListOnClick)
			{
				AdvGame.RunActionListAsset(actionListOnClick);
			}
			base.ProcessClick(_menu, _slot, _mouseState);
		}

		public override void RecalculateSize(MenuSource source)
		{
			if (Application.isPlaying && uiDropdown != null)
			{
				if (uiDropdown.captionText != null)
				{
					string optionLabel = GetOptionLabel(selected);
					if (!string.IsNullOrEmpty(optionLabel))
					{
						uiDropdown.captionText.text = optionLabel;
					}
				}
				for (int i = 0; i < GetNumOptions(); i++)
				{
					if (uiDropdown.options.Count > i && uiDropdown.options[i] != null)
					{
						uiDropdown.options[i].text = GetOptionLabel(i);
					}
				}
			}
			base.RecalculateSize(source);
		}

		public override void OnMenuTurnOn(Menu menu)
		{
			if (cycleType == AC_CycleType.Variable)
			{
				linkedVariable = GlobalVariables.GetVariable(varID);
				if (linkedVariable != null)
				{
					if (linkedVariable.type != VariableType.Integer && linkedVariable.type != VariableType.PopUp)
					{
						ACDebug.LogWarning("Cannot link the variable '" + linkedVariable.label + "' to Cycle element '" + title + "' because it is not an Integer or PopUp.");
						linkedVariable = null;
					}
				}
				else
				{
					ACDebug.LogWarning("Cannot find the variable with ID=" + varID + " to link to the Cycle element '" + title + "'");
				}
			}
			base.OnMenuTurnOn(menu);
		}

		private void CalculateValue()
		{
			if (!Application.isPlaying)
			{
				return;
			}
			if (cycleType == AC_CycleType.Language)
			{
				if (Application.isPlaying)
				{
					optionsArray = KickStarter.runtimeLanguages.Languages;
				}
				else
				{
					optionsArray = AdvGame.GetReferences().speechManager.languages;
				}
				if (Options.optionsData != null)
				{
					selected = Options.optionsData.language;
					if (KickStarter.speechManager != null && KickStarter.speechManager.separateVoiceAndTextLanguages && splitLanguageType == SplitLanguageType.VoiceOnly)
					{
						selected = Options.optionsData.voiceLanguage;
					}
				}
			}
			else if (cycleType == AC_CycleType.Variable && linkedVariable != null)
			{
				if (GetNumOptions() > 0)
				{
					selected = Mathf.Clamp(linkedVariable.IntegerValue, 0, GetNumOptions() - 1);
				}
				else
				{
					selected = 0;
				}
			}
		}

		protected override void AutoSize()
		{
			AutoSize(new GUIContent(TranslateLabel(label, Options.GetLanguage()) + " : Default option"));
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
