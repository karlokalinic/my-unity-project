using System;
using UnityEngine;

namespace AC
{
	[RequireComponent(typeof(Rigidbody))]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_moveable___pick_up.html")]
	public class Moveable_PickUp : DragBase
	{
		public bool allowRotation;

		public float breakForce = 300f;

		public bool allowThrow;

		public float chargeTime = 0.5f;

		public float pullbackDistance = 0.6f;

		public float throwForce = 400f;

		public ActionListSource actionListSource;

		public Interaction interactionOnGrab;

		public Interaction interactionOnDrop;

		public ActionListAsset actionListAssetOnGrab;

		public ActionListAsset actionListAssetOnDrop;

		public int moveParameterID = -1;

		public int dropParameterID = -1;

		public float initialLift = 0.05f;

		protected bool isChargingThrow;

		protected float throwCharge;

		protected float chargeStartTime;

		protected bool inRotationMode;

		protected FixedJoint fixedJoint;

		protected float originalDistanceToCamera;

		protected Vector3 worldMousePosition;

		protected Vector3 deltaMovement;

		protected LerpUtils.Vector3Lerp fixedJointLerp = new LerpUtils.Vector3Lerp();

		protected Vector3 fixedJointOffset;

		protected override void Awake()
		{
			base.Awake();
			if (_rigidbody == null)
			{
				ACDebug.LogWarning("A Rigidbody component is required for " + base.name);
			}
		}

		protected override void Start()
		{
			LimitCollisions();
			base.Start();
		}

		protected new void Update()
		{
			if (!isHeld)
			{
				return;
			}
			if (allowThrow)
			{
				if (KickStarter.playerInput.InputGetButton("ThrowMoveable"))
				{
					ChargeThrow();
				}
				else if (isChargingThrow)
				{
					ReleaseThrow();
				}
			}
			if (allowRotation)
			{
				if (KickStarter.playerInput.InputGetButton("RotateMoveable"))
				{
					SetRotationMode(true);
				}
				else if (KickStarter.playerInput.InputGetButtonUp("RotateMoveable"))
				{
					SetRotationMode(false);
					return;
				}
				if (KickStarter.playerInput.InputGetButtonDown("RotateMoveableToggle"))
				{
					SetRotationMode(!inRotationMode);
					if (!inRotationMode)
					{
						return;
					}
				}
			}
			if (allowZooming)
			{
				UpdateZoom();
			}
		}

		protected void LateUpdate()
		{
			if (isHeld && !inRotationMode)
			{
				worldMousePosition = GetWorldMousePosition();
				Vector3 b = (worldMousePosition - fixedJointOffset - fixedJoint.transform.position) * 100f;
				deltaMovement = Vector3.Lerp(deltaMovement, b, Time.deltaTime * 6f);
			}
		}

		protected void OnCollisionEnter(Collision collision)
		{
			BaseOnCollisionEnter(collision);
		}

		protected void OnDestroy()
		{
			if ((bool)fixedJoint)
			{
				UnityEngine.Object.Destroy(fixedJoint.gameObject);
				fixedJoint = null;
			}
		}

		public override void UpdateMovement()
		{
			base.UpdateMovement();
			if ((bool)moveSound && (bool)moveSoundClip && !inRotationMode)
			{
				if (numCollisions > 0)
				{
					PlayMoveSound(_rigidbody.linearVelocity.magnitude, 0.5f);
				}
				else if (moveSound.IsPlaying())
				{
					moveSound.Stop();
				}
			}
		}

		public override void Grab(Vector3 grabPosition)
		{
			inRotationMode = false;
			isChargingThrow = false;
			throwCharge = 0f;
			if (fixedJoint == null)
			{
				CreateFixedJoint();
			}
			fixedJoint.transform.position = grabPosition;
			fixedJointOffset = Vector3.zero;
			deltaMovement = Vector3.zero;
			Rigidbody rigidbody = _rigidbody;
			Vector3 zero = Vector3.zero;
			_rigidbody.angularVelocity = zero;
			rigidbody.linearVelocity = zero;
			originalDistanceToCamera = (grabPosition - KickStarter.CameraMain.transform.position).magnitude;
			base.Grab(grabPosition);
			RunInteraction(true);
		}

		public override void LetGo(bool ignoreEvents = false)
		{
			if (inRotationMode)
			{
				SetRotationMode(false);
			}
			if (fixedJoint != null && (bool)fixedJoint.connectedBody)
			{
				fixedJoint.connectedBody = null;
			}
			_rigidbody.linearDamping = originalDrag;
			_rigidbody.angularDamping = originalAngularDrag;
			if (inRotationMode)
			{
				_rigidbody.linearVelocity = Vector3.zero;
			}
			else if (!isChargingThrow && !ignoreEvents)
			{
				_rigidbody.AddForce(deltaMovement * Time.deltaTime / Time.fixedDeltaTime * 7f);
			}
			_rigidbody.useGravity = true;
			base.LetGo();
			RunInteraction(false);
		}

		protected void RunInteraction(bool onGrab)
		{
			int num = ((!onGrab) ? dropParameterID : moveParameterID);
			switch (actionListSource)
			{
			case ActionListSource.InScene:
			{
				Interaction interaction = ((!onGrab) ? interactionOnDrop : interactionOnGrab);
				if (!(interaction != null) || base.gameObject.layer == LayerMask.NameToLayer(KickStarter.settingsManager.deactivatedLayer) || (onGrab && KickStarter.actionListManager.IsListRunning(interaction)))
				{
					break;
				}
				if (num >= 0)
				{
					ActionParameter parameter2 = interaction.GetParameter(num);
					if (parameter2 != null && parameter2.parameterType == ParameterType.GameObject)
					{
						parameter2.gameObject = base.gameObject;
					}
				}
				interaction.Interact();
				break;
			}
			case ActionListSource.AssetFile:
			{
				ActionListAsset actionListAsset = ((!onGrab) ? actionListAssetOnDrop : actionListAssetOnGrab);
				if (!(actionListAsset != null) || base.gameObject.layer == LayerMask.NameToLayer(KickStarter.settingsManager.deactivatedLayer) || (onGrab && KickStarter.actionListAssetManager.IsListRunning(actionListAsset)))
				{
					break;
				}
				if (num >= 0)
				{
					ActionParameter parameter = actionListAsset.GetParameter(num);
					if (parameter != null && parameter.parameterType == ParameterType.GameObject)
					{
						parameter.gameObject = base.gameObject;
						if ((bool)GetComponent<ConstantID>())
						{
							parameter.intValue = GetComponent<ConstantID>().constantID;
						}
						else
						{
							ACDebug.LogWarning("Cannot set the value of parameter " + num + " ('" + parameter.label + "') as " + base.gameObject.name + " has no Constant ID component.", base.gameObject);
						}
					}
				}
				actionListAsset.Interact();
				break;
			}
			}
		}

		public override bool CanToggleCursor()
		{
			if (isChargingThrow || inRotationMode)
			{
				return false;
			}
			return true;
		}

		public override void ApplyDragForce(Vector3 force, Vector3 _screenMousePosition, float _distanceToCamera)
		{
			distanceToCamera = _distanceToCamera;
			if (inRotationMode)
			{
				force *= speedFactor * _rigidbody.linearDamping * distanceToCamera * Time.deltaTime;
				if (force.magnitude > maxSpeed)
				{
					force *= maxSpeed / force.magnitude;
				}
				Vector3 torque = Vector3.Cross(force, KickStarter.CameraMain.transform.forward);
				torque /= Mathf.Sqrt((grabPoint.position - base.transform.position).magnitude) * 2.4f * rotationFactor;
				_rigidbody.AddTorque(torque);
			}
			else
			{
				UpdateFixedJoint();
			}
		}

		public void UnsetFixedJoint()
		{
			fixedJoint = null;
			isHeld = false;
		}

		protected void ChargeThrow()
		{
			if (!isChargingThrow)
			{
				isChargingThrow = true;
				chargeStartTime = Time.time;
				throwCharge = 0f;
			}
			else if (throwCharge < 1f)
			{
				throwCharge = (Time.time - chargeStartTime) / chargeTime;
			}
			if (throwCharge > 1f)
			{
				throwCharge = 1f;
			}
		}

		protected void ReleaseThrow()
		{
			LetGo();
			_rigidbody.useGravity = true;
			_rigidbody.linearDamping = originalDrag;
			_rigidbody.angularDamping = originalAngularDrag;
			Vector3 normalized = (base.transform.position - KickStarter.CameraMain.transform.position).normalized;
			_rigidbody.AddForce(throwForce * throwCharge * normalized);
		}

		protected void CreateFixedJoint()
		{
			GameObject gameObject = new GameObject(base.name + " (Joint)");
			Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
			rigidbody.constraints = RigidbodyConstraints.FreezeAll;
			rigidbody.useGravity = false;
			fixedJoint = gameObject.AddComponent<FixedJoint>();
			FixedJoint obj = fixedJoint;
			float breakTorque = breakForce;
			fixedJoint.breakTorque = breakTorque;
			obj.breakForce = breakTorque;
			gameObject.AddComponent<JointBreaker>();
		}

		protected void SetRotationMode(bool on)
		{
			_rigidbody.linearVelocity = Vector3.zero;
			_rigidbody.useGravity = !on;
			if (inRotationMode != on)
			{
				if (on)
				{
					KickStarter.playerInput.forceGameplayCursor = ForceGameplayCursor.KeepUnlocked;
					fixedJoint.connectedBody = null;
				}
				else
				{
					KickStarter.playerInput.forceGameplayCursor = ForceGameplayCursor.None;
					if (!KickStarter.playerInput.GetInGameCursorState())
					{
						fixedJointOffset = GetWorldMousePosition() - fixedJoint.transform.position;
						deltaMovement = Vector3.zero;
					}
				}
			}
			inRotationMode = on;
		}

		protected void UpdateFixedJoint()
		{
			if ((bool)fixedJoint)
			{
				fixedJoint.transform.position = fixedJointLerp.Update(fixedJoint.transform.position, worldMousePosition - fixedJointOffset, 10f);
				if (!inRotationMode && fixedJoint.connectedBody != _rigidbody)
				{
					fixedJoint.connectedBody = _rigidbody;
				}
			}
		}

		protected new void UpdateZoom()
		{
			float axis = Input.GetAxis("ZoomMoveable");
			if ((!(originalDistanceToCamera <= minZoom) || !(axis < 0f)) && (!(originalDistanceToCamera >= maxZoom) || !(axis > 0f)))
			{
				originalDistanceToCamera += axis * zoomSpeed / 10f * Time.deltaTime;
			}
			originalDistanceToCamera = Mathf.Clamp(originalDistanceToCamera, minZoom, maxZoom);
		}

		protected void LimitCollisions()
		{
			Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
			Collider[] array = componentsInChildren;
			foreach (Collider collider in array)
			{
				Collider[] array2 = componentsInChildren;
				foreach (Collider collider2 in array2)
				{
					if (!(collider == collider2))
					{
						Physics.IgnoreCollision(collider, collider2, true);
						Physics.IgnoreCollision(collider, collider2, true);
					}
				}
				if (ignorePlayerCollider && KickStarter.player != null)
				{
					Collider[] componentsInChildren2 = KickStarter.player.gameObject.GetComponentsInChildren<Collider>();
					Collider[] array3 = componentsInChildren2;
					foreach (Collider collider3 in array3)
					{
						Physics.IgnoreCollision(collider3, collider, true);
					}
				}
			}
		}

		protected Vector3 GetWorldMousePosition()
		{
			Vector3 vector = KickStarter.playerInput.GetMousePosition();
			float alignedDistance = GetAlignedDistance(vector);
			vector.z = alignedDistance - throwCharge * pullbackDistance;
			Vector3 vector2 = KickStarter.CameraMain.ScreenToWorldPoint(vector);
			return vector2 + Vector3.up * initialLift;
		}

		protected float GetAlignedDistance(Vector3 screenMousePosition)
		{
			screenMousePosition.z = 1f;
			Vector3 vector = KickStarter.CameraMain.ScreenToWorldPoint(screenMousePosition);
			float num = Vector3.Angle(KickStarter.CameraMain.transform.forward, vector - KickStarter.CameraMain.transform.position);
			return originalDistanceToCamera * Mathf.Cos(num * ((float)Math.PI / 180f));
		}
	}
}
