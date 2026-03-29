using UnityEngine;
using UnityEngine.UI;

namespace AC
{
	public class BackgroundImageUI : MonoBehaviour
	{
		public Canvas canvas;

		public RawImage rawImage;

		public Texture emptyTexture;

		protected RectTransform rawImageRectTransform;

		private static BackgroundImageUI instance;

		public static BackgroundImageUI Instance
		{
			get
			{
				if (instance == null)
				{
					instance = Object.FindObjectOfType<BackgroundImageUI>();
					if (instance == null)
					{
						GameObject gameObject = (GameObject)Object.Instantiate(Resources.Load("BackgroundImageUI"));
						instance = gameObject.GetComponent<BackgroundImageUI>();
						gameObject.name = "BackgroundImageUI";
					}
				}
				instance.CorrectLayer();
				instance.AssignCamera();
				return instance;
			}
		}

		protected void Start()
		{
			if (rawImage != null)
			{
				rawImageRectTransform = rawImage.GetComponent<RectTransform>();
			}
			CorrectLayer();
		}

		public void SetTexture(Texture texture)
		{
			if (texture == null)
			{
				return;
			}
			if (canvas.worldCamera == null)
			{
				BackgroundCamera backgroundCamera = Object.FindObjectOfType<BackgroundCamera>();
				if (backgroundCamera != null)
				{
					canvas.worldCamera = backgroundCamera.GetComponent<Camera>();
				}
				else
				{
					ACDebug.LogWarning("No 'BackgroundCamera' found - is it present in the scene? If not, drag it in from /AdventureCreator/Prefabs/Automatic.");
				}
			}
			canvas.planeDistance = 0.015f;
			rawImage.texture = texture;
		}

		public void ClearTexture(Texture texture)
		{
			if (rawImage.texture == texture || texture == null)
			{
				rawImage.texture = emptyTexture;
			}
		}

		public void SetShakeIntensity(float intensity)
		{
			float num = 1f + intensity / 50f;
			rawImageRectTransform.localScale = Vector3.one * num;
			float x = Random.Range(0f - intensity, intensity) * 2f;
			float y = Random.Range(0f - intensity, intensity) * 2f;
			Vector2 vector = new Vector2(x, y);
			rawImageRectTransform.localPosition = vector;
		}

		protected void AssignCamera()
		{
			if (canvas.worldCamera == null)
			{
				BackgroundCamera backgroundCamera = Object.FindObjectOfType<BackgroundCamera>();
				if (backgroundCamera != null)
				{
					canvas.worldCamera = backgroundCamera.GetComponent<Camera>();
				}
				else
				{
					ACDebug.LogWarning("No 'BackgroundCamera' found - is it present in the scene? If not, drag it in from /AdventureCreator/Prefabs/Automatic.");
				}
			}
		}

		protected void CorrectLayer()
		{
			if (LayerMask.NameToLayer(KickStarter.settingsManager.backgroundImageLayer) == -1)
			{
				ACDebug.LogWarning("No '" + KickStarter.settingsManager.backgroundImageLayer + "' layer exists - please define one in the Tags Manager.");
			}
			else
			{
				base.gameObject.layer = LayerMask.NameToLayer(KickStarter.settingsManager.backgroundImageLayer);
			}
		}
	}
}
