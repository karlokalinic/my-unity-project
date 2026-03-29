using UnityEngine;
using UnityEngine.UI;

namespace AC
{
	public class MenuLabel : MenuElement, ITranslatable
	{
		public Text uiText;

		public string label = "Element";

		public TextAnchor anchor;

		public TextEffects textEffects;

		public float outlineSize = 2f;

		public AC_LabelType labelType;

		public int variableID;

		public bool useCharacterColour;

		public bool autoAdjustHeight = true;

		public bool updateIfEmpty;

		public bool showPendingWhileMovingToHotspot;

		public int itemPropertyID;

		public bool multiplyByItemCount;

		public InventoryPropertyType inventoryPropertyType;

		public SelectedObjectiveLabelType selectedObjectiveLabelType;

		public int itemSlotNumber;

		private MenuJournal linkedJournal;

		private MenuInventoryBox linkedInventoryBox;

		private string newLabel = string.Empty;

		private Speech speech;

		private Color speechColour;

		private bool isDuppingSpeech;

		private Document Document
		{
			get
			{
				return KickStarter.runtimeDocuments.ActiveDocument;
			}
		}

		public override void Declare()
		{
			uiText = null;
			label = "Label";
			isVisible = true;
			isClickable = false;
			numSlots = 1;
			anchor = TextAnchor.MiddleCenter;
			SetSize(new Vector2(10f, 5f));
			labelType = AC_LabelType.Normal;
			variableID = 0;
			useCharacterColour = false;
			autoAdjustHeight = true;
			textEffects = TextEffects.None;
			outlineSize = 2f;
			newLabel = string.Empty;
			updateIfEmpty = false;
			showPendingWhileMovingToHotspot = false;
			inventoryPropertyType = InventoryPropertyType.SelectedItem;
			selectedObjectiveLabelType = SelectedObjectiveLabelType.Title;
			itemPropertyID = 0;
			itemSlotNumber = 0;
			multiplyByItemCount = false;
			base.Declare();
		}

		public override MenuElement DuplicateSelf(bool fromEditor, bool ignoreUnityUI)
		{
			MenuLabel menuLabel = ScriptableObject.CreateInstance<MenuLabel>();
			menuLabel.Declare();
			menuLabel.CopyLabel(this, ignoreUnityUI);
			return menuLabel;
		}

		private void CopyLabel(MenuLabel _element, bool ignoreUnityUI)
		{
			if (ignoreUnityUI)
			{
				uiText = null;
			}
			else
			{
				uiText = _element.uiText;
			}
			label = _element.label;
			anchor = _element.anchor;
			textEffects = _element.textEffects;
			outlineSize = _element.outlineSize;
			labelType = _element.labelType;
			variableID = _element.variableID;
			useCharacterColour = _element.useCharacterColour;
			autoAdjustHeight = _element.autoAdjustHeight;
			updateIfEmpty = _element.updateIfEmpty;
			showPendingWhileMovingToHotspot = _element.showPendingWhileMovingToHotspot;
			newLabel = string.Empty;
			inventoryPropertyType = _element.inventoryPropertyType;
			selectedObjectiveLabelType = _element.selectedObjectiveLabelType;
			itemPropertyID = _element.itemPropertyID;
			itemSlotNumber = _element.itemSlotNumber;
			multiplyByItemCount = _element.multiplyByItemCount;
			base.Copy(_element);
		}

		public override void LoadUnityUI(Menu _menu, Canvas canvas)
		{
			uiText = LinkUIElement<Text>(canvas);
		}

		public override RectTransform GetRectTransform(int _slot)
		{
			if ((bool)uiText)
			{
				return uiText.rectTransform;
			}
			return null;
		}

		public override void SetSpeech(Speech _speech)
		{
			isDuppingSpeech = true;
			speech = _speech;
		}

		public override void ClearSpeech()
		{
			if (labelType == AC_LabelType.DialogueLine || labelType == AC_LabelType.DialogueSpeaker)
			{
				newLabel = string.Empty;
			}
		}

		public override void OnMenuTurnOn(Menu menu)
		{
			base.OnMenuTurnOn(menu);
			PreDisplay(0, Options.GetLanguage(), false);
		}

		public override void PreDisplay(int _slot, int languageNumber, bool isActive)
		{
			if (Application.isPlaying)
			{
				UpdateLabelText(languageNumber);
			}
			else
			{
				newLabel = label;
			}
			newLabel = AdvGame.ConvertTokens(newLabel, languageNumber);
			if (uiText != null && Application.isPlaying)
			{
				uiText.text = newLabel;
				UpdateUIElement(uiText);
			}
		}

		public void UpdateLabelText(int languageNumber = 0)
		{
			string text = newLabel;
			switch (labelType)
			{
			case AC_LabelType.Normal:
				newLabel = TranslateLabel(label, languageNumber);
				break;
			case AC_LabelType.Hotspot:
			{
				string value = string.Empty;
				if (showPendingWhileMovingToHotspot && KickStarter.playerInteraction.GetHotspotMovingTo() != null && KickStarter.playerCursor.GetSelectedCursorID() == -1 && KickStarter.runtimeInventory.SelectedItem == null)
				{
					value = KickStarter.playerInteraction.MovingToHotspotLabel;
				}
				if (string.IsNullOrEmpty(value))
				{
					value = KickStarter.playerMenus.GetHotspotLabel();
				}
				if (!string.IsNullOrEmpty(value) || updateIfEmpty)
				{
					newLabel = value;
				}
				break;
			}
			case AC_LabelType.GlobalVariable:
			{
				GVar variable = GlobalVariables.GetVariable(variableID);
				if (variable != null)
				{
					newLabel = variable.GetValue(languageNumber);
					break;
				}
				ACDebug.LogWarning("Label element '" + title + "' cannot display Global Variable " + variableID + " as it does not exist!");
				break;
			}
			case AC_LabelType.ActiveSaveProfile:
				newLabel = KickStarter.options.GetProfileName();
				break;
			case AC_LabelType.InventoryProperty:
				newLabel = string.Empty;
				if (inventoryPropertyType == InventoryPropertyType.SelectedItem)
				{
					newLabel = GetPropertyDisplayValue(languageNumber, KickStarter.runtimeInventory.SelectedItem);
				}
				else if (inventoryPropertyType == InventoryPropertyType.LastClickedItem)
				{
					newLabel = GetPropertyDisplayValue(languageNumber, KickStarter.runtimeInventory.lastClickedItem);
				}
				else if (inventoryPropertyType == InventoryPropertyType.MouseOverItem)
				{
					newLabel = GetPropertyDisplayValue(languageNumber, KickStarter.runtimeInventory.hoverItem);
				}
				break;
			case AC_LabelType.DialogueLine:
			case AC_LabelType.DialogueSpeaker:
				if (parentMenu != null && parentMenu.IsFadingOut())
				{
					return;
				}
				UpdateSpeechLink();
				if (labelType == AC_LabelType.DialogueLine)
				{
					if (speech != null)
					{
						string displayText = speech.displayText;
						if (displayText != string.Empty || updateIfEmpty)
						{
							newLabel = displayText;
						}
						if (useCharacterColour)
						{
							speechColour = speech.GetColour();
							if ((bool)uiText)
							{
								uiText.color = speechColour;
							}
						}
					}
					else if (!KickStarter.speechManager.keepTextInBuffer)
					{
						newLabel = string.Empty;
					}
				}
				else
				{
					if (labelType != AC_LabelType.DialogueSpeaker)
					{
						break;
					}
					if (speech != null)
					{
						string speaker = speech.GetSpeaker(languageNumber);
						if (speaker != string.Empty || updateIfEmpty || speech.GetSpeakingCharacter() == null)
						{
							newLabel = speaker;
						}
					}
					else if (!KickStarter.speechManager.keepTextInBuffer)
					{
						newLabel = string.Empty;
					}
				}
				break;
			case AC_LabelType.DocumentTitle:
				if (Document != null)
				{
					newLabel = KickStarter.runtimeLanguages.GetTranslation(Document.title, Document.titleLineID, languageNumber);
				}
				break;
			case AC_LabelType.SelectedObjective:
				if (KickStarter.runtimeObjectives.SelectedObjective != null)
				{
					switch (selectedObjectiveLabelType)
					{
					case SelectedObjectiveLabelType.Title:
						newLabel = KickStarter.runtimeObjectives.SelectedObjective.Objective.GetTitle(languageNumber);
						break;
					case SelectedObjectiveLabelType.Description:
						newLabel = KickStarter.runtimeObjectives.SelectedObjective.Objective.GetDescription(languageNumber);
						break;
					case SelectedObjectiveLabelType.StateLabel:
						newLabel = KickStarter.runtimeObjectives.SelectedObjective.CurrentState.GetLabel(languageNumber);
						break;
					case SelectedObjectiveLabelType.StateDescription:
						newLabel = KickStarter.runtimeObjectives.SelectedObjective.CurrentState.GetDescription(languageNumber);
						break;
					case SelectedObjectiveLabelType.StateType:
						newLabel = KickStarter.runtimeObjectives.SelectedObjective.CurrentState.stateType.ToString();
						break;
					}
				}
				else
				{
					newLabel = string.Empty;
				}
				break;
			}
			if (newLabel != text && sizeType == AC_SizeType.Automatic && parentMenu != null && parentMenu.menuSource == MenuSource.AdventureCreator)
			{
				parentMenu.Recalculate();
			}
		}

		private string GetPropertyDisplayValue(int languageNumber, InvItem invItem)
		{
			if (invItem != null)
			{
				InvVar property = invItem.GetProperty(itemPropertyID, multiplyByItemCount);
				if (property != null)
				{
					return property.GetDisplayValue(languageNumber);
				}
			}
			return string.Empty;
		}

		public override void Display(GUIStyle _style, int _slot, float zoom, bool isActive)
		{
			if (Application.isPlaying && labelType == AC_LabelType.DialogueLine)
			{
				if (useCharacterColour)
				{
					_style.normal.textColor = speechColour;
				}
				if ((updateIfEmpty || !string.IsNullOrEmpty(newLabel)) && autoAdjustHeight && sizeType == AC_SizeType.Manual)
				{
					GUIContent content = new GUIContent(newLabel);
					relativeRect.height = _style.CalcHeight(content, relativeRect.width);
				}
			}
			base.Display(_style, _slot, zoom, isActive);
			_style.wordWrap = true;
			_style.alignment = anchor;
			if (zoom < 1f)
			{
				_style.fontSize = (int)((float)_style.fontSize * zoom);
			}
			if (textEffects != TextEffects.None)
			{
				AdvGame.DrawTextEffect(ZoomRect(relativeRect, zoom), newLabel, _style, Color.black, _style.normal.textColor, outlineSize, textEffects);
			}
			else
			{
				GUI.Label(ZoomRect(relativeRect, zoom), newLabel, _style);
			}
		}

		public override string GetLabel(int slot, int languageNumber)
		{
			if (labelType == AC_LabelType.Normal)
			{
				return TranslateLabel(label, languageNumber);
			}
			if (labelType == AC_LabelType.DialogueSpeaker)
			{
				return KickStarter.dialog.GetSpeaker(languageNumber);
			}
			if (labelType == AC_LabelType.GlobalVariable)
			{
				return GlobalVariables.GetVariable(variableID).GetValue(languageNumber);
			}
			if (labelType == AC_LabelType.Hotspot)
			{
				return newLabel;
			}
			if (labelType == AC_LabelType.ActiveSaveProfile)
			{
				if (Application.isPlaying)
				{
					return KickStarter.options.GetProfileName();
				}
				return label;
			}
			return string.Empty;
		}

		private void UpdateSpeechLink()
		{
			if (!isDuppingSpeech && KickStarter.dialog.GetLatestSpeech() != null)
			{
				speech = KickStarter.dialog.GetLatestSpeech();
			}
		}

		protected override void AutoSize()
		{
			int language = Options.GetLanguage();
			string text = ((!Application.isPlaying) ? label : newLabel);
			if (labelType == AC_LabelType.DialogueLine)
			{
				GUIContent gUIContent = new GUIContent(TranslateLabel(text, language));
				GUIStyle gUIStyle = new GUIStyle();
				gUIStyle.font = font;
				gUIStyle.fontSize = (int)(KickStarter.mainCamera.GetPlayableScreenArea(false).size.x * fontScaleFactor / 100f);
				UpdateSpeechLink();
				if (speech != null)
				{
					string text2 = " " + speech.FullText + " ";
					gUIContent = new GUIContent(text2);
					AutoSize(gUIContent);
				}
			}
			else if (labelType == AC_LabelType.ActiveSaveProfile)
			{
				GUIContent content = new GUIContent(GetLabel(0, 0));
				AutoSize(content);
			}
			else if (string.IsNullOrEmpty(text) && backgroundTexture != null)
			{
				GUIContent content2 = new GUIContent(backgroundTexture);
				AutoSize(content2);
			}
			else if (labelType == AC_LabelType.Normal)
			{
				GUIContent content3 = new GUIContent(TranslateLabel(text, language));
				AutoSize(content3);
			}
			else
			{
				GUIContent content4 = new GUIContent(text);
				AutoSize(content4);
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
