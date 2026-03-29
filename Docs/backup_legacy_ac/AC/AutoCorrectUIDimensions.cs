using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace AC
{
	[AddComponentMenu("Adventure Creator/UI/Auto-correct UI Dimensions")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_auto_correct_ui_dimensions.html")]
	public class AutoCorrectUIDimensions : MonoBehaviour
	{
		protected Canvas canvas;

		protected CanvasScaler canvasScaler;

		protected RectTransform transformToControl;

		public Vector2 minAnchorPoint = new Vector2(0.5f, 0.5f);

		public Vector2 maxAnchorPoint = new Vector2(0.5f, 0.5f);

		public bool updatePosition = true;

		public bool updateScale = true;

		protected Vector2 originalReferenceResolution;

		protected void Start()
		{
			Initialise();
		}

		protected void OnEnable()
		{
			EventManager.OnUpdatePlayableScreenArea += OnUpdatePlayableScreenArea;
			StartCoroutine(UpdateInOneFrame());
		}

		protected void OnDisable()
		{
			EventManager.OnUpdatePlayableScreenArea -= OnUpdatePlayableScreenArea;
		}

		protected void Initialise()
		{
			canvas = GetComponent<Canvas>();
			canvasScaler = canvas.GetComponent<CanvasScaler>();
			if (canvasScaler != null)
			{
				originalReferenceResolution = canvasScaler.referenceResolution;
			}
			if (canvas == null || canvasScaler == null)
			{
				ACDebug.LogWarning("The AutoCorrectUIDimensions component must be attached to a GameObject with both a Canvas and CanvasScaler component - be sure to attach it to the root Canvas object.", this);
			}
			if (KickStarter.playerMenus != null)
			{
				Menu menuWithCanvas = KickStarter.playerMenus.GetMenuWithCanvas(canvas);
				if (menuWithCanvas != null)
				{
					transformToControl = menuWithCanvas.rectTransform;
				}
			}
		}

		protected IEnumerator UpdateInOneFrame()
		{
			yield return new WaitForEndOfFrame();
			OnUpdatePlayableScreenArea();
		}

		protected void OnUpdatePlayableScreenArea()
		{
			if (updatePosition && transformToControl != null)
			{
				transformToControl.anchorMin = ConvertToPlayableSpace(minAnchorPoint);
				transformToControl.anchorMax = ConvertToPlayableSpace(maxAnchorPoint);
			}
			if (updateScale && canvasScaler != null)
			{
				Vector2 size = KickStarter.mainCamera.GetPlayableScreenArea(true).size;
				canvasScaler.referenceResolution = new Vector2(originalReferenceResolution.x / size.x, originalReferenceResolution.y / size.y);
			}
		}

		protected Vector2 ConvertToPlayableSpace(Vector2 screenPosition)
		{
			Rect playableScreenArea = KickStarter.mainCamera.GetPlayableScreenArea(true);
			return new Vector2(screenPosition.x * playableScreenArea.width, screenPosition.y * playableScreenArea.height) + playableScreenArea.position;
		}
	}
}
