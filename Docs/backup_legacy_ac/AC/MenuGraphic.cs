using UnityEngine;
using UnityEngine.UI;

namespace AC
{
	public class MenuGraphic : MenuElement
	{
		private enum UIImageType
		{
			Image = 0,
			RawImage = 1
		}

		public Image uiImage;

		public AC_GraphicType graphicType;

		public CursorIconBase graphic;

		public RawImage uiRawImage;

		[SerializeField]
		private UIImageType uiImageType;

		private Texture localTexture;

		private Sprite sprite;

		private Speech speech;

		private bool speechIsAnimating;

		private Rect speechRect;

		private bool isDuppingSpeech;

		public override void Declare()
		{
			uiImage = null;
			uiRawImage = null;
			graphicType = AC_GraphicType.Normal;
			isVisible = true;
			isClickable = false;
			graphic = new CursorIconBase();
			numSlots = 1;
			SetSize(new Vector2(10f, 5f));
			base.Declare();
		}

		public override MenuElement DuplicateSelf(bool fromEditor, bool ignoreUnityUI)
		{
			MenuGraphic menuGraphic = ScriptableObject.CreateInstance<MenuGraphic>();
			menuGraphic.Declare();
			menuGraphic.CopyGraphic(this, ignoreUnityUI);
			return menuGraphic;
		}

		private void CopyGraphic(MenuGraphic _element, bool ignoreUnityUI)
		{
			if (ignoreUnityUI)
			{
				uiImage = null;
			}
			else
			{
				uiImage = _element.uiImage;
			}
			uiRawImage = _element.uiRawImage;
			uiImageType = _element.uiImageType;
			graphicType = _element.graphicType;
			graphic = new CursorIconBase();
			graphic.Copy(_element.graphic);
			base.Copy(_element);
		}

		public override void LoadUnityUI(Menu _menu, Canvas canvas)
		{
			if (uiImageType == UIImageType.Image)
			{
				uiImage = LinkUIElement<Image>(canvas);
			}
			else if (uiImageType == UIImageType.RawImage)
			{
				uiRawImage = LinkUIElement<RawImage>(canvas);
			}
		}

		public override RectTransform GetRectTransform(int _slot)
		{
			if (uiImageType == UIImageType.Image && uiImage != null)
			{
				return uiImage.rectTransform;
			}
			if (uiImageType == UIImageType.RawImage && uiRawImage != null)
			{
				return uiRawImage.rectTransform;
			}
			return null;
		}

		public void SetNormalGraphicTexture(Texture newTexture)
		{
			if (graphicType == AC_GraphicType.Normal)
			{
				graphic.texture = newTexture;
				graphic.ClearCache();
			}
		}

		private void UpdateSpeechLink()
		{
			if (!isDuppingSpeech && KickStarter.dialog.GetLatestSpeech() != null)
			{
				speech = KickStarter.dialog.GetLatestSpeech();
			}
		}

		public override void SetSpeech(Speech _speech)
		{
			isDuppingSpeech = true;
			speech = _speech;
		}

		public override void ClearSpeech()
		{
			if (graphicType == AC_GraphicType.DialoguePortrait)
			{
				localTexture = null;
			}
		}

		public override void OnMenuTurnOn(Menu menu)
		{
			base.OnMenuTurnOn(menu);
			PreDisplay(0, Options.GetLanguage(), false);
		}

		public override void PreDisplay(int _slot, int languageNumber, bool isActive)
		{
			switch (graphicType)
			{
			case AC_GraphicType.DialoguePortrait:
				UpdateSpeechLink();
				if (speech != null)
				{
					localTexture = speech.GetPortrait();
					speechIsAnimating = speech.IsAnimating();
				}
				break;
			case AC_GraphicType.DocumentTexture:
				if (!Application.isPlaying || KickStarter.runtimeDocuments.ActiveDocument == null)
				{
					break;
				}
				if (localTexture != KickStarter.runtimeDocuments.ActiveDocument.texture)
				{
					if (KickStarter.runtimeDocuments.ActiveDocument.texture != null)
					{
						Texture2D texture2 = KickStarter.runtimeDocuments.ActiveDocument.texture;
						sprite = Sprite.Create(texture2, new Rect(0f, 0f, texture2.width, texture2.height), new Vector2(0.5f, 0.5f));
					}
					else
					{
						sprite = null;
					}
				}
				localTexture = KickStarter.runtimeDocuments.ActiveDocument.texture;
				break;
			case AC_GraphicType.ObjectiveTexture:
				if (Application.isPlaying && KickStarter.runtimeObjectives.SelectedObjective != null)
				{
					if (localTexture != KickStarter.runtimeObjectives.SelectedObjective.Objective.texture && KickStarter.runtimeObjectives.SelectedObjective.Objective.texture != null)
					{
						Texture2D texture = KickStarter.runtimeObjectives.SelectedObjective.Objective.texture;
						sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
					}
					localTexture = KickStarter.runtimeObjectives.SelectedObjective.Objective.texture;
				}
				break;
			}
			SetUIGraphic();
		}

		public override void Display(GUIStyle _style, int _slot, float zoom, bool isActive)
		{
			base.Display(_style, _slot, zoom, isActive);
			switch (graphicType)
			{
			case AC_GraphicType.Normal:
				if (graphic != null)
				{
					graphic.DrawAsInteraction(ZoomRect(relativeRect, zoom), true);
				}
				break;
			case AC_GraphicType.DialoguePortrait:
				if (!(localTexture != null))
				{
					break;
				}
				if (speechIsAnimating)
				{
					if (speech != null)
					{
						speechRect = speech.GetAnimatedRect();
					}
					GUI.DrawTextureWithTexCoords(ZoomRect(relativeRect, zoom), localTexture, speechRect);
				}
				else
				{
					GUI.DrawTexture(ZoomRect(relativeRect, zoom), localTexture, ScaleMode.StretchToFill, true, 0f);
				}
				break;
			case AC_GraphicType.DocumentTexture:
			case AC_GraphicType.ObjectiveTexture:
				if (localTexture != null)
				{
					GUI.DrawTexture(ZoomRect(relativeRect, zoom), localTexture, ScaleMode.StretchToFill, true, 0f);
				}
				break;
			}
		}

		public override void RecalculateSize(MenuSource source)
		{
			graphic.Reset();
			SetUIGraphic();
			base.RecalculateSize(source);
		}

		private void SetUIGraphic()
		{
			if (uiImageType == UIImageType.Image && uiImage != null)
			{
				switch (graphicType)
				{
				case AC_GraphicType.Normal:
					uiImage.sprite = graphic.GetAnimatedSprite(true);
					break;
				case AC_GraphicType.DialoguePortrait:
					if (speech != null)
					{
						uiImage.sprite = speech.GetPortraitSprite();
					}
					break;
				case AC_GraphicType.DocumentTexture:
				case AC_GraphicType.ObjectiveTexture:
					uiImage.sprite = sprite;
					break;
				}
				UpdateUIElement(uiImage);
			}
			if (uiImageType != UIImageType.RawImage || !(uiRawImage != null))
			{
				return;
			}
			switch (graphicType)
			{
			case AC_GraphicType.Normal:
				if (graphic.texture != null && graphic.texture is RenderTexture)
				{
					uiRawImage.texture = graphic.texture;
				}
				else
				{
					uiRawImage.texture = graphic.GetAnimatedTexture();
				}
				break;
			case AC_GraphicType.DocumentTexture:
			case AC_GraphicType.ObjectiveTexture:
				uiRawImage.texture = localTexture;
				break;
			case AC_GraphicType.DialoguePortrait:
				if (speech != null)
				{
					uiRawImage.texture = speech.GetPortrait();
				}
				break;
			}
			UpdateUIElement(uiRawImage);
		}

		protected override void AutoSize()
		{
			if (graphicType == AC_GraphicType.Normal && graphic.texture != null)
			{
				GUIContent content = new GUIContent(graphic.texture);
				AutoSize(content);
			}
		}
	}
}
