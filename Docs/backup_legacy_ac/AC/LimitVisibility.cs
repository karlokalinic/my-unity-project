using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace AC
{
	[AddComponentMenu("Adventure Creator/Camera/Limit visibility to camera")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_limit_visibility.html")]
	public class LimitVisibility : MonoBehaviour
	{
		[HideInInspector]
		public _Camera limitToCamera;

		public List<_Camera> limitToCameras = new List<_Camera>();

		public bool affectChildren;

		public bool negateEffect;

		[HideInInspector]
		public bool isLockedOff;

		protected bool isVisible;

		protected _Camera activeCamera;

		protected _Camera transitionCamera;

		protected Renderer _renderer;

		protected SpriteRenderer spriteRenderer;

		protected Renderer[] childRenderers;

		protected SpriteRenderer[] childSprites;

		protected VideoPlayer videoPlayer;

		protected void Awake()
		{
			_renderer = GetComponent<Renderer>();
			if (_renderer == null)
			{
				spriteRenderer = GetComponent<SpriteRenderer>();
			}
			if (affectChildren)
			{
				childRenderers = GetComponentsInChildren<Renderer>();
				childSprites = GetComponentsInChildren<SpriteRenderer>();
			}
			videoPlayer = GetComponent<VideoPlayer>();
		}

		protected void OnEnable()
		{
			if ((bool)KickStarter.stateHandler)
			{
				KickStarter.stateHandler.Register(this);
			}
		}

		protected void OnDisable()
		{
			if ((bool)KickStarter.stateHandler)
			{
				KickStarter.stateHandler.Unregister(this);
			}
		}

		protected void Start()
		{
			if ((bool)KickStarter.stateHandler)
			{
				KickStarter.stateHandler.Register(this);
			}
			Upgrade();
			if (limitToCameras.Count == 0 || KickStarter.mainCamera == null)
			{
				return;
			}
			activeCamera = KickStarter.mainCamera.attachedCamera;
			if (activeCamera != null && !isLockedOff)
			{
				if (limitToCameras.Contains(activeCamera))
				{
					SetVisibility(true);
				}
				else
				{
					SetVisibility(false);
				}
			}
			else
			{
				SetVisibility(false);
			}
		}

		public void Upgrade()
		{
			if (limitToCameras == null)
			{
				limitToCameras = new List<_Camera>();
			}
			if (limitToCamera != null)
			{
				if (!limitToCameras.Contains(limitToCamera))
				{
					limitToCameras.Add(limitToCamera);
				}
				limitToCamera = null;
			}
		}

		public void _Update()
		{
			if (limitToCameras.Count == 0 || KickStarter.mainCamera == null)
			{
				return;
			}
			activeCamera = KickStarter.mainCamera.attachedCamera;
			transitionCamera = KickStarter.mainCamera.GetTransitionFromCamera();
			if (isLockedOff)
			{
				if (isVisible)
				{
					SetVisibility(false);
				}
			}
			else if (activeCamera != null && limitToCameras.Contains(activeCamera))
			{
				SetVisibility(!negateEffect);
			}
			else if (transitionCamera != null && limitToCameras.Contains(transitionCamera))
			{
				SetVisibility(!negateEffect);
			}
			else
			{
				SetVisibility(negateEffect);
			}
		}

		protected void SetVisibility(bool state)
		{
			if (_renderer != null)
			{
				_renderer.enabled = state;
			}
			else if (this.spriteRenderer != null)
			{
				this.spriteRenderer.enabled = state;
			}
			if (affectChildren)
			{
				Renderer[] array = childRenderers;
				foreach (Renderer renderer in array)
				{
					renderer.enabled = state;
				}
				SpriteRenderer[] array2 = childSprites;
				foreach (SpriteRenderer spriteRenderer in array2)
				{
					spriteRenderer.enabled = state;
				}
			}
			if (videoPlayer != null)
			{
				videoPlayer.targetCameraAlpha = ((!state) ? 0f : 1f);
			}
			isVisible = state;
		}
	}
}
