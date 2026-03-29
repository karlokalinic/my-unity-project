using UnityEngine;

namespace AC
{
	[AddComponentMenu("Adventure Creator/Camera/Basic camera")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1___camera.html")]
	public class _Camera : MonoBehaviour
	{
		public bool targetIsPlayer = true;

		public Transform target;

		public bool isDragControlled;

		public float focalDistance = 10f;

		protected Char targetChar;

		protected Camera _camera;

		protected Vector2 inputMovement;

		[SerializeField]
		[HideInInspector]
		protected bool is2D;

		public Transform Target
		{
			get
			{
				if (targetIsPlayer)
				{
					if (KickStarter.player != null)
					{
						return KickStarter.player.transform;
					}
					return null;
				}
				return target;
			}
		}

		public Transform CameraTransform
		{
			get
			{
				return Camera.transform;
			}
		}

		public Camera Camera
		{
			get
			{
				if (_camera == null)
				{
					_camera = GetComponent<Camera>();
					if (_camera == null)
					{
						_camera = GetComponentInChildren<Camera>();
					}
					if (_camera == null)
					{
						ACDebug.LogWarning(base.name + " has no Camera component!", this);
					}
				}
				return _camera;
			}
		}

		public bool isFor2D
		{
			get
			{
				return is2D;
			}
			set
			{
				is2D = value;
			}
		}

		protected Vector3 TargetForward
		{
			get
			{
				if (targetChar != null)
				{
					return targetChar.TransformForward;
				}
				if (target != null)
				{
					return target.forward;
				}
				return Vector3.zero;
			}
		}

		protected virtual void Awake()
		{
			if (Camera != null && Camera == GetComponent<Camera>() && (bool)KickStarter.mainCamera)
			{
				Camera.enabled = false;
			}
			SwitchTarget(target);
		}

		protected virtual void OnEnable()
		{
			if ((bool)KickStarter.stateHandler)
			{
				KickStarter.stateHandler.Register(this);
			}
		}

		protected virtual void Start()
		{
			if ((bool)KickStarter.stateHandler)
			{
				KickStarter.stateHandler.Register(this);
			}
		}

		protected virtual void OnDisable()
		{
			if ((bool)KickStarter.stateHandler)
			{
				KickStarter.stateHandler.Unregister(this);
			}
		}

		public virtual Vector2 CreateRotationOffset()
		{
			return Vector2.zero;
		}

		public virtual void SwitchTarget(Transform _target)
		{
			target = _target;
			if (target != null)
			{
				targetChar = _target.GetComponent<Char>();
			}
			else
			{
				targetChar = null;
			}
		}

		public virtual bool Is2D()
		{
			return is2D;
		}

		public virtual void _Update()
		{
		}

		public virtual void ResetTarget()
		{
			if (targetIsPlayer && (bool)KickStarter.player)
			{
				SwitchTarget(KickStarter.player.transform);
			}
		}

		public virtual void MoveCameraInstant()
		{
		}

		public void SetSplitScreen()
		{
			Camera.enabled = true;
			Camera.rect = KickStarter.mainCamera.GetSplitScreenRect(false);
		}

		public void RemoveSplitScreen()
		{
			if (Camera.enabled)
			{
				Camera.rect = new Rect(0f, 0f, 1f, 1f);
				Camera.enabled = false;
			}
		}

		public virtual Vector2 GetPerspectiveOffset()
		{
			return Vector2.zero;
		}

		public bool IsActive()
		{
			if (KickStarter.mainCamera != null)
			{
				return KickStarter.mainCamera.attachedCamera == this;
			}
			return false;
		}

		protected Vector3 PositionRelativeToCamera(Vector3 _position)
		{
			return _position.x * ForwardVector() + _position.z * RightVector();
		}

		protected Vector3 RightVector()
		{
			return base.transform.right;
		}

		protected Vector3 ForwardVector()
		{
			Vector3 forward = base.transform.forward;
			forward.y = 0f;
			return forward;
		}

		protected float ConstrainAxis(float desired, Vector2 range)
		{
			desired = ((range.x < range.y) ? Mathf.Clamp(desired, range.x, range.y) : ((!(range.x > range.y)) ? range.x : Mathf.Clamp(desired, range.y, range.x)));
			return desired;
		}
	}
}
