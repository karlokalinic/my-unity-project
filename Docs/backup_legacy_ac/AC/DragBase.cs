using UnityEngine;

namespace AC
{
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_drag_base.html")]
	public class DragBase : Moveable
	{
		public InteractiveBoundary interactiveBoundary;

		protected bool isHeld;

		public bool invertInput;

		public float maxSpeed = 200f;

		public float playerMovementReductionFactor;

		public float playerMovementInfluence = 1f;

		public bool allowZooming;

		public float zoomSpeed = 60f;

		public float minZoom = 1f;

		public float maxZoom = 3f;

		public float rotationFactor = 1f;

		public bool showIcon;

		public int iconID = -1;

		public AudioClip moveSoundClip;

		public AudioClip collideSoundClip;

		public float slideSoundThreshold = 0.03f;

		public float slidePitchFactor = 1f;

		public bool onlyPlayLowerCollisionSound;

		public bool ignoreMoveableRigidbodies;

		public bool ignorePlayerCollider;

		public bool childrenShareLayer;

		protected Transform grabPoint;

		protected float distanceToCamera;

		protected float speedFactor = 0.16f;

		protected float originalDrag;

		protected float originalAngularDrag;

		protected int numCollisions;

		protected CursorIconBase icon;

		protected Sound collideSound;

		protected Sound moveSound;

		public bool IsHeld
		{
			get
			{
				return isHeld;
			}
		}

		protected override void Awake()
		{
			base.Awake();
			GameObject gameObject = new GameObject();
			gameObject.name = base.name + " (Grab point)";
			grabPoint = gameObject.transform;
			grabPoint.parent = base.transform;
			if ((bool)moveSoundClip)
			{
				GameObject gameObject2 = new GameObject();
				gameObject2.name = base.name + " (Move sound)";
				gameObject2.transform.parent = base.transform;
				gameObject2.AddComponent<Sound>();
				gameObject2.GetComponent<AudioSource>().playOnAwake = false;
				moveSound = gameObject2.GetComponent<Sound>();
			}
			icon = GetMainIcon();
			collideSound = GetComponent<Sound>();
		}

		protected void OnEnable()
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

		protected void OnDisable()
		{
			if ((bool)KickStarter.stateHandler)
			{
				KickStarter.stateHandler.Unregister(this);
			}
		}

		public virtual void UpdateMovement()
		{
		}

		public virtual void _FixedUpdate()
		{
		}

		protected void OnCollisionExit(Collision collision)
		{
			if (KickStarter.player != null && collision.gameObject != KickStarter.player.gameObject)
			{
				numCollisions--;
			}
		}

		public void TurnOn()
		{
			PlaceOnLayer(LayerMask.NameToLayer(KickStarter.settingsManager.hotspotLayer));
		}

		public void TurnOff()
		{
			PlaceOnLayer(LayerMask.NameToLayer(KickStarter.settingsManager.deactivatedLayer));
		}

		public virtual bool CanToggleCursor()
		{
			return false;
		}

		public virtual void DrawGrabIcon()
		{
			if (isHeld && showIcon && KickStarter.CameraMain.WorldToScreenPoint(base.transform.position).z > 0f && icon != null)
			{
				Vector3 vector = KickStarter.CameraMain.WorldToScreenPoint(grabPoint.position);
				icon.Draw(new Vector3(vector.x, vector.y));
			}
		}

		public virtual void Grab(Vector3 grabPosition)
		{
			isHeld = true;
			grabPoint.position = grabPosition;
			originalDrag = _rigidbody.linearDamping;
			originalAngularDrag = _rigidbody.angularDamping;
			_rigidbody.linearDamping = 20f;
			_rigidbody.angularDamping = 20f;
		}

		public virtual void LetGo(bool ignoreEvents = false)
		{
			isHeld = false;
			if (!ignoreEvents)
			{
				KickStarter.eventManager.Call_OnDropMoveable(this);
			}
		}

		public bool IsOnScreen()
		{
			Vector2 point = KickStarter.CameraMain.WorldToScreenPoint(grabPoint.position);
			return KickStarter.mainCamera.GetPlayableScreenArea(false).Contains(point);
		}

		public bool IsCloseToCamera(float maxDistance)
		{
			if ((GetGrabPosition() - KickStarter.CameraMain.transform.position).magnitude < maxDistance)
			{
				return true;
			}
			return false;
		}

		public virtual void ApplyDragForce(Vector3 force, Vector3 mousePosition, float distanceToCamera)
		{
		}

		public bool PlayerIsWithinBoundary()
		{
			if (interactiveBoundary == null || KickStarter.player == null)
			{
				return true;
			}
			return interactiveBoundary.PlayerIsPresent;
		}

		protected void PlaceOnLayer(int layerName)
		{
			base.gameObject.layer = layerName;
			if (!childrenShareLayer)
			{
				return;
			}
			foreach (Transform item in base.transform)
			{
				item.gameObject.layer = layerName;
			}
		}

		protected void BaseOnCollisionEnter(Collision collision)
		{
			if (KickStarter.player != null && collision.gameObject != KickStarter.player.gameObject)
			{
				numCollisions++;
				if ((bool)collideSound && (bool)collideSoundClip && Time.time > 0f)
				{
					collideSound.Play(collideSoundClip, false);
				}
			}
		}

		protected void PlayMoveSound(float speed, float trackValue)
		{
			if (speed > slideSoundThreshold)
			{
				moveSound.relativeVolume = speed - slideSoundThreshold;
				moveSound.SetMaxVolume();
				if (slidePitchFactor > 0f)
				{
					moveSound.GetComponent<AudioSource>().pitch = Mathf.Lerp(GetComponent<AudioSource>().pitch, Mathf.Min(1f, speed), Time.deltaTime * 5f);
				}
			}
			if (speed > slideSoundThreshold && !moveSound.IsPlaying())
			{
				moveSound.relativeVolume = speed - slideSoundThreshold;
				moveSound.Play(moveSoundClip, true);
			}
			else if (speed <= slideSoundThreshold && moveSound.IsPlaying() && !moveSound.IsFading())
			{
				moveSound.FadeOut(0.2f);
			}
		}

		protected void UpdateZoom()
		{
			float axis = Input.GetAxis("ZoomMoveable");
			Vector3 normalized = (base.transform.position - KickStarter.CameraMain.transform.position).normalized;
			if (distanceToCamera - minZoom < 1f && axis < 0f)
			{
				normalized *= distanceToCamera - minZoom;
			}
			else if (maxZoom - distanceToCamera < 1f && axis > 0f)
			{
				normalized *= maxZoom - distanceToCamera;
			}
			if ((distanceToCamera < minZoom && axis < 0f) || (distanceToCamera > maxZoom && axis > 0f))
			{
				_rigidbody.AddForce(-normalized * axis * zoomSpeed);
				_rigidbody.linearVelocity = Vector3.zero;
			}
			else
			{
				_rigidbody.AddForce(normalized * axis * zoomSpeed);
			}
		}

		protected void LimitZoom()
		{
			if (distanceToCamera < minZoom)
			{
				base.transform.position = KickStarter.CameraMain.transform.position + (base.transform.position - KickStarter.CameraMain.transform.position) / (distanceToCamera / minZoom);
			}
			else if (distanceToCamera > maxZoom)
			{
				base.transform.position = KickStarter.CameraMain.transform.position + (base.transform.position - KickStarter.CameraMain.transform.position) / (distanceToCamera / maxZoom);
			}
		}

		protected CursorIconBase GetMainIcon()
		{
			if (KickStarter.cursorManager == null || iconID < 0)
			{
				return null;
			}
			return KickStarter.cursorManager.GetCursorIconFromID(iconID);
		}

		public Vector3 GetGrabPosition()
		{
			return grabPoint.position;
		}
	}
}
