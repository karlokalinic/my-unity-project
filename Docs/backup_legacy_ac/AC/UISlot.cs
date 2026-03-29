using System;
using UnityEngine;
using UnityEngine.UI;

namespace AC
{
	[Serializable]
	public class UISlot
	{
		public UnityEngine.UI.Button uiButton;

		public int uiButtonID;

		public Sprite sprite;

		private Text uiText;

		private Image uiImage;

		private RawImage uiRawImage;

		private Color originalColour;

		private Sprite emptySprite;

		private Texture cacheTexture;

		public bool CanOverrideHotspotLabel
		{
			get
			{
				if (uiButton != null)
				{
					return uiButton.interactable;
				}
				return true;
			}
		}

		private Sprite EmptySprite
		{
			get
			{
				if (emptySprite == null)
				{
					emptySprite = Resources.Load<Sprite>("EmptySlot");
				}
				return emptySprite;
			}
		}

		public UISlot()
		{
			uiButton = null;
			uiButtonID = 0;
			uiText = null;
			uiImage = null;
			uiRawImage = null;
			sprite = null;
		}

		public RectTransform GetRectTransform()
		{
			if (uiButton != null && (bool)uiButton.GetComponent<RectTransform>())
			{
				return uiButton.GetComponent<RectTransform>();
			}
			return null;
		}

		public void LinkUIElements(Canvas canvas, LinkUIGraphic linkUIGraphic, Texture2D emptySlotTexture = null)
		{
			if (canvas != null)
			{
				uiButton = Serializer.GetGameObjectComponent<UnityEngine.UI.Button>(uiButtonID, canvas.gameObject);
			}
			else
			{
				uiButton = null;
			}
			if ((bool)uiButton)
			{
				uiText = uiButton.GetComponentInChildren<Text>();
				uiRawImage = uiButton.GetComponentInChildren<RawImage>();
				switch (linkUIGraphic)
				{
				case LinkUIGraphic.ImageComponent:
					uiImage = uiButton.GetComponentInChildren<Image>();
					break;
				case LinkUIGraphic.ButtonTargetGraphic:
					if (uiButton.targetGraphic != null)
					{
						if (uiButton.targetGraphic is Image)
						{
							uiImage = uiButton.targetGraphic as Image;
							break;
						}
						ACDebug.LogWarning(string.Concat("Cannot assign UI Image for ", uiButton.name, "'s target graphic as ", uiButton.targetGraphic, " is not an Image component."), canvas);
					}
					else
					{
						ACDebug.LogWarning("Cannot assign UI Image for " + uiButton.name + "'s target graphic because it has none.", canvas);
					}
					break;
				}
				originalColour = uiButton.colors.normalColor;
			}
			if (emptySlotTexture != null)
			{
				emptySprite = Sprite.Create(emptySlotTexture, new Rect(0f, 0f, emptySlotTexture.width, emptySlotTexture.height), new Vector2(0.5f, 0.5f), 100f, 0u, SpriteMeshType.FullRect);
			}
		}

		public void SetText(string _text)
		{
			if ((bool)uiText)
			{
				uiText.text = _text;
			}
		}

		public void SetImage(Texture _texture)
		{
			if (uiRawImage != null)
			{
				uiRawImage.texture = _texture;
			}
			else
			{
				if (!(uiImage != null))
				{
					return;
				}
				if (_texture == null)
				{
					sprite = EmptySprite;
				}
				else if (sprite == null || sprite == emptySprite || cacheTexture != _texture)
				{
					if (_texture is Texture2D)
					{
						Texture2D texture2D = (Texture2D)_texture;
						sprite = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f), 100f, 0u, SpriteMeshType.FullRect);
					}
					else
					{
						ACDebug.LogWarning("Cannot show texture " + _texture.name + " in UI Image " + uiImage.name + " because it is not a 2D Texture. Use a UI RawImage component instead.", uiImage);
					}
				}
				if (_texture != null)
				{
					cacheTexture = _texture;
				}
				uiImage.sprite = sprite;
			}
		}

		public void SetImageAsSprite(Sprite _sprite)
		{
			if (uiImage != null)
			{
				if (_sprite == null)
				{
					sprite = EmptySprite;
				}
				else if (sprite == null || sprite == EmptySprite || sprite != _sprite)
				{
					sprite = _sprite;
				}
				uiImage.sprite = sprite;
			}
		}

		public void ShowUIElement(UIHideStyle uiHideStyle)
		{
			if (Application.isPlaying && uiButton != null && uiButton.gameObject != null && uiHideStyle == UIHideStyle.DisableObject && !uiButton.gameObject.activeSelf)
			{
				uiButton.gameObject.SetActive(true);
			}
		}

		public void HideUIElement(UIHideStyle uiHideStyle)
		{
			if (Application.isPlaying && uiButton != null && uiButton.gameObject != null && uiButton.gameObject.activeSelf)
			{
				switch (uiHideStyle)
				{
				case UIHideStyle.DisableObject:
					uiButton.gameObject.SetActive(false);
					break;
				case UIHideStyle.ClearContent:
					SetImage(null);
					SetText(string.Empty);
					break;
				}
			}
		}

		public void AddClickHandler(Menu _menu, MenuElement _element, int _slot)
		{
			UISlotClick uISlotClick = uiButton.gameObject.AddComponent<UISlotClick>();
			uISlotClick.Setup(_menu, _element, _slot);
		}

		public void SetColour(Color newColour)
		{
			if (uiButton != null)
			{
				ColorBlock colors = uiButton.colors;
				colors.normalColor = newColour;
				uiButton.colors = colors;
			}
		}

		public void RestoreColour()
		{
			if (uiButton != null)
			{
				ColorBlock colors = uiButton.colors;
				colors.normalColor = originalColour;
				uiButton.colors = colors;
			}
		}
	}
}
