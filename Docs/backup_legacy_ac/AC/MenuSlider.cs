using UnityEngine;
using UnityEngine.UI;

namespace AC
{
	public class MenuSlider : MenuElement, ITranslatable
	{
		public Slider uiSlider;

		public float amount;

		public float minValue;

		public float maxValue = 1f;

		public string label;

		public TextEffects textEffects;

		public float outlineSize = 2f;

		public TextAnchor anchor;

		public Texture2D sliderTexture;

		public SliderDisplayType sliderDisplayType;

		public AC_SliderType sliderType;

		public Vector2 blockSize = new Vector2(0.05f, 1f);

		public bool useFullWidth;

		public int varID;

		public int numberOfSteps;

		public ActionListAsset actionListOnChange;

		public UISelectableHideStyle uiSelectableHideStyle;

		private float visualAmount;

		private string fullText;

		public override void Declare()
		{
			uiSlider = null;
			label = "Slider";
			isVisible = true;
			isClickable = true;
			numSlots = 1;
			amount = 1f;
			minValue = 0f;
			maxValue = 1f;
			anchor = TextAnchor.MiddleLeft;
			sliderType = AC_SliderType.CustomScript;
			sliderDisplayType = SliderDisplayType.FillBar;
			blockSize = new Vector2(0.05f, 1f);
			useFullWidth = false;
			varID = 0;
			textEffects = TextEffects.None;
			outlineSize = 2f;
			numberOfSteps = 0;
			actionListOnChange = null;
			uiSelectableHideStyle = UISelectableHideStyle.DisableObject;
			base.Declare();
		}

		public override MenuElement DuplicateSelf(bool fromEditor, bool ignoreUnityUI)
		{
			MenuSlider menuSlider = ScriptableObject.CreateInstance<MenuSlider>();
			menuSlider.Declare();
			menuSlider.CopySlider(this, ignoreUnityUI);
			return menuSlider;
		}

		private void CopySlider(MenuSlider _element, bool ignoreUnityUI)
		{
			if (ignoreUnityUI)
			{
				uiSlider = null;
			}
			else
			{
				uiSlider = _element.uiSlider;
			}
			label = _element.label;
			isClickable = _element.isClickable;
			textEffects = _element.textEffects;
			outlineSize = _element.outlineSize;
			amount = _element.amount;
			minValue = _element.minValue;
			maxValue = _element.maxValue;
			anchor = _element.anchor;
			sliderTexture = _element.sliderTexture;
			sliderType = _element.sliderType;
			sliderDisplayType = _element.sliderDisplayType;
			blockSize = _element.blockSize;
			useFullWidth = _element.useFullWidth;
			varID = _element.varID;
			numberOfSteps = _element.numberOfSteps;
			actionListOnChange = _element.actionListOnChange;
			uiSelectableHideStyle = _element.uiSelectableHideStyle;
			base.Copy(_element);
		}

		public override void LoadUnityUI(Menu _menu, Canvas canvas)
		{
			uiSlider = LinkUIElement<Slider>(canvas);
			if (!uiSlider)
			{
				return;
			}
			uiSlider.interactable = isClickable;
			if (isClickable)
			{
				uiSlider.onValueChanged.AddListener(delegate
				{
					ProcessClickUI(_menu, 0, KickStarter.playerInput.GetMouseState());
				});
			}
		}

		public override GameObject GetObjectToSelect(int slotIndex = 0)
		{
			if ((bool)uiSlider)
			{
				return uiSlider.gameObject;
			}
			return null;
		}

		public override RectTransform GetRectTransform(int _slot)
		{
			if ((bool)uiSlider)
			{
				return uiSlider.GetComponent<RectTransform>();
			}
			return null;
		}

		public override void SetUIInteractableState(bool state)
		{
			if ((bool)uiSlider)
			{
				uiSlider.interactable = state;
			}
		}

		public override void PreDisplay(int _slot, int languageNumber, bool isActive)
		{
			CalculateValue();
			fullText = AdvGame.ConvertTokens(TranslateLabel(label, languageNumber));
			if ((bool)uiSlider)
			{
				uiSlider.value = visualAmount;
				UpdateUISelectable(uiSlider, uiSelectableHideStyle);
			}
		}

		public override void Display(GUIStyle _style, int _slot, float zoom, bool isActive)
		{
			base.Display(_style, _slot, zoom, isActive);
			GUI.Label(ZoomRect(relativeRect, zoom), string.Empty, _style);
			if ((bool)sliderTexture)
			{
				DrawSlider(zoom);
			}
			_style.alignment = anchor;
			if (zoom < 1f)
			{
				_style.fontSize = (int)((float)_style.fontSize * zoom);
			}
			_style.normal.background = null;
			if (textEffects != TextEffects.None)
			{
				AdvGame.DrawTextEffect(ZoomRect(relativeRect, zoom), fullText, _style, Color.black, _style.normal.textColor, outlineSize, textEffects);
			}
			else
			{
				GUI.Label(ZoomRect(relativeRect, zoom), fullText, _style);
			}
		}

		private void DrawSlider(float zoom)
		{
			Rect rect = relativeRect;
			if (sliderDisplayType == SliderDisplayType.FillBar)
			{
				if (useFullWidth)
				{
					rect.x = relativeRect.x;
					rect.width = slotSize.x * visualAmount;
				}
				else
				{
					rect.x = relativeRect.x + relativeRect.width / 2f;
					rect.width = slotSize.x * visualAmount * 0.5f;
				}
				if (sizeType != AC_SizeType.AbsolutePixels)
				{
					rect.width *= KickStarter.mainCamera.GetPlayableScreenArea(false).size.x / 100f;
				}
			}
			else if (sliderDisplayType == SliderDisplayType.MoveableBlock)
			{
				rect.width *= blockSize.x;
				rect.height *= blockSize.y;
				rect.y += (relativeRect.height - rect.height) / 2f;
				if (useFullWidth)
				{
					rect.x += (relativeRect.width - rect.width) * visualAmount;
				}
				else
				{
					rect.x += (relativeRect.width - rect.width) / 2f;
					rect.x += (relativeRect.width - rect.width) * visualAmount / 2f;
				}
			}
			GUI.DrawTexture(ZoomRect(rect, zoom), sliderTexture, ScaleMode.StretchToFill, true, 0f);
		}

		public override string GetLabel(int slot, int languageNumber)
		{
			return AdvGame.ConvertTokens(TranslateLabel(label, languageNumber));
		}

		public override bool IsSelectedByEventSystem(int slotIndex)
		{
			if (uiSlider != null)
			{
				return KickStarter.playerMenus.IsEventSystemSelectingObject(uiSlider.gameObject);
			}
			return false;
		}

		private void Change()
		{
		}

		private void Change(float mouseX)
		{
			if (useFullWidth)
			{
				mouseX -= relativeRect.x;
				visualAmount = mouseX / relativeRect.width;
			}
			else
			{
				mouseX = mouseX - relativeRect.x - relativeRect.width / 2f;
				visualAmount = mouseX / (relativeRect.width / 2f);
			}
			UpdateValue();
		}

		private void UpdateValue()
		{
			if (uiSlider == null)
			{
				visualAmount = Mathf.Clamp(visualAmount, 0f, 1f);
				if (numberOfSteps > 0)
				{
					visualAmount = Mathf.Round(visualAmount * (float)numberOfSteps) / (float)numberOfSteps;
				}
				amount = visualAmount * (maxValue - minValue) + minValue;
			}
			else
			{
				amount = visualAmount;
			}
			switch (sliderType)
			{
			case AC_SliderType.Speech:
				Options.SetSpeechVolume(amount);
				break;
			case AC_SliderType.Music:
				Options.SetMusicVolume(amount);
				break;
			case AC_SliderType.SFX:
				Options.SetSFXVolume(amount);
				break;
			case AC_SliderType.FloatVariable:
				if (varID >= 0)
				{
					GlobalVariables.SetFloatValue(varID, amount);
				}
				break;
			}
			if (!KickStarter.actionListAssetManager.IsListRunning(actionListOnChange))
			{
				AdvGame.RunActionListAsset(actionListOnChange);
			}
		}

		private void CalculateValue()
		{
			if (!Application.isPlaying)
			{
				visualAmount = 0.5f;
				return;
			}
			if (sliderType == AC_SliderType.Speech || sliderType == AC_SliderType.SFX || sliderType == AC_SliderType.Music)
			{
				if (Options.optionsData != null)
				{
					if (sliderType == AC_SliderType.Speech)
					{
						amount = Options.optionsData.speechVolume;
					}
					else if (sliderType == AC_SliderType.Music)
					{
						amount = Options.optionsData.musicVolume;
					}
					else if (sliderType == AC_SliderType.SFX)
					{
						amount = Options.optionsData.sfxVolume;
					}
				}
			}
			else if (sliderType == AC_SliderType.FloatVariable && varID >= 0)
			{
				GVar variable = GlobalVariables.GetVariable(varID);
				if (variable != null)
				{
					if (variable.type != VariableType.Float)
					{
						ACDebug.LogWarning("Cannot link MenuSlider " + title + " to Variable " + varID + " as it is not a Float.");
					}
					else
					{
						amount = Mathf.Clamp(variable.floatVal, minValue, maxValue);
						variable.SetFloatValue(amount);
					}
				}
				else
				{
					ACDebug.LogWarning("Slider " + label + " is referencing Gloval Variable " + varID + ", which does not exist.");
				}
			}
			if (uiSlider != null)
			{
				visualAmount = amount;
			}
			else
			{
				visualAmount = (amount - minValue) / (maxValue - minValue);
			}
		}

		public override void ProcessClick(Menu _menu, int _slot, MouseState _mouseState)
		{
			if (_menu.IsClickable())
			{
				if (uiSlider != null)
				{
					visualAmount = uiSlider.value;
					UpdateValue();
				}
				else if (KickStarter.playerInput.canKeyboardControlMenusDuringGameplay && (KickStarter.stateHandler.gameState == GameState.DialogOptions || KickStarter.stateHandler.gameState == GameState.Paused || (KickStarter.stateHandler.IsInGameplay() && KickStarter.playerInput.canKeyboardControlMenusDuringGameplay)))
				{
					Change();
				}
				else
				{
					Change(KickStarter.playerInput.GetMousePosition().x - _menu.GetRect().x);
				}
				if (sliderType == AC_SliderType.CustomScript)
				{
					MenuSystem.OnElementClick(_menu, this, _slot, (int)_mouseState);
				}
				KickStarter.eventManager.Call_OnMenuElementClick(_menu, this, _slot, (int)_mouseState);
			}
		}

		public bool KeyboardControl(Vector2 direction)
		{
			if (direction == Vector2.right)
			{
				visualAmount += 0.02f;
				UpdateValue();
				return true;
			}
			if (direction == Vector2.left)
			{
				visualAmount -= 0.02f;
				UpdateValue();
				return true;
			}
			return false;
		}

		public override void ProcessContinuousClick(Menu _menu, MouseState _mouseState)
		{
			if (KickStarter.stateHandler.gameState != GameState.Cutscene)
			{
				if (uiSlider != null)
				{
					visualAmount = uiSlider.value;
					UpdateValue();
				}
				else if (KickStarter.settingsManager.inputMethod == InputMethod.KeyboardOrController)
				{
					Change();
				}
				else
				{
					Change(KickStarter.playerInput.GetMousePosition().x - _menu.GetRect().x);
				}
				if (sliderType == AC_SliderType.CustomScript)
				{
					MenuSystem.OnElementClick(_menu, this, 0, (int)_mouseState);
				}
			}
		}

		protected override void AutoSize()
		{
			AutoSize(new GUIContent(TranslateLabel(label, Options.GetLanguage())));
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
