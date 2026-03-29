using UnityEngine;

namespace AC
{
	[RequireComponent(typeof(Camera))]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_background_camera.html")]
	public class BackgroundCamera : MonoBehaviour
	{
		protected Camera _camera;

		protected static BackgroundCamera instance;

		public static BackgroundCamera Instance
		{
			get
			{
				if (instance == null)
				{
					instance = Object.FindObjectOfType<BackgroundCamera>();
				}
				instance.SetCorrectLayer();
				return instance;
			}
		}

		protected void Awake()
		{
			_camera = GetComponent<Camera>();
			UpdateRect();
			SetCorrectLayer();
		}

		protected void OnEnable()
		{
			if ((bool)KickStarter.stateHandler)
			{
				KickStarter.stateHandler.Register(this);
			}
		}

		protected void Start()
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

		public void UpdateRect()
		{
			if (_camera == null)
			{
				_camera = GetComponent<Camera>();
			}
			_camera.rect = KickStarter.CameraMain.rect;
		}

		protected void SetCorrectLayer()
		{
			if ((bool)KickStarter.settingsManager)
			{
				if (LayerMask.NameToLayer(KickStarter.settingsManager.backgroundImageLayer) == -1)
				{
					ACDebug.LogWarning("No '" + KickStarter.settingsManager.backgroundImageLayer + "' layer exists - please define one in the Tags Manager.");
				}
				else
				{
					GetComponent<Camera>().cullingMask = 1 << LayerMask.NameToLayer(KickStarter.settingsManager.backgroundImageLayer);
				}
			}
			else
			{
				ACDebug.LogWarning("A Settings Manager is required for this camera type");
			}
		}
	}
}
