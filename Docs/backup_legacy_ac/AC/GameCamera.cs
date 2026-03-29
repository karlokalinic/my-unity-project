using UnityEngine;

namespace AC
{
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_game_camera.html")]
	public class GameCamera : CursorInfluenceCamera
	{
		public bool actFromDefaultPlayerStart = true;

		public bool lockXLocAxis = true;

		public bool lockYLocAxis = true;

		public bool lockZLocAxis = true;

		public bool lockXRotAxis = true;

		public bool lockYRotAxis = true;

		public bool lockFOV = true;

		public CameraLocConstrainType xLocConstrainType;

		public CameraLocConstrainType yLocConstrainType = CameraLocConstrainType.TargetHeight;

		public CameraLocConstrainType zLocConstrainType;

		public CameraLocConstrainType xRotConstrainType = CameraLocConstrainType.TargetHeight;

		public CameraRotConstrainType yRotConstrainType;

		public float xGradient = 1f;

		public float yGradientLoc = 1f;

		public float zGradient = 1f;

		public float xGradientRot = 2f;

		public float yGradient = 2f;

		public float FOVGradient = 2f;

		public float xOffset;

		public float yOffsetLoc;

		public float zOffset;

		public float xOffsetRot;

		public float yOffset;

		public float FOVOffset;

		public float xFreedom = 2f;

		public float yFreedom = 2f;

		public float zFreedom = 2f;

		public bool limitX;

		public bool limitYLoc;

		public bool limitZ;

		public bool limitXRot;

		public bool limitY;

		public bool limitFOV;

		public float targetHeight;

		public float targetXOffset;

		public float targetZOffset;

		public Vector2 constrainX;

		public Vector2 constrainYLoc;

		public Vector2 constrainZ;

		public Vector2 constrainXRot;

		public Vector2 constrainY;

		public Vector2 constrainFOV;

		public float directionInfluence;

		public float dampSpeed = 0.9f;

		public bool focalPointIsTarget;

		protected Vector3 desiredPosition;

		protected float desiredSpin;

		protected float desiredPitch;

		protected float desiredFOV;

		protected Vector3 originalTargetPosition;

		protected Vector3 originalPosition;

		protected float originalSpin;

		protected float originalPitch;

		protected float originalFOV;

		protected bool haveSetOriginalPosition;

		protected override void Awake()
		{
			base.Awake();
			SetOriginalPosition();
			desiredPosition = originalPosition;
			desiredPitch = originalPitch;
			desiredSpin = originalSpin;
			desiredFOV = originalFOV;
			if (!lockXLocAxis && limitX)
			{
				desiredPosition.x = ConstrainAxis(desiredPosition.x, constrainX);
			}
			if (!lockYLocAxis && limitY)
			{
				desiredPosition.y = ConstrainAxis(desiredPosition.y, constrainYLoc);
			}
			if (!lockZLocAxis && limitZ)
			{
				desiredPosition.z = ConstrainAxis(desiredPosition.z, constrainZ);
			}
			if (!lockXRotAxis && limitXRot)
			{
				desiredPitch = ConstrainAxis(desiredPitch, constrainXRot);
			}
			if (!lockYRotAxis && limitY)
			{
				desiredSpin = ConstrainAxis(desiredSpin, constrainY);
			}
			if (!lockFOV && limitFOV)
			{
				desiredFOV = ConstrainAxis(desiredFOV, constrainFOV);
			}
		}

		protected override void Start()
		{
			base.Start();
			ResetTarget();
			if ((bool)target)
			{
				SetTargetOriginalPosition();
				MoveCameraInstant();
			}
		}

		public override void _Update()
		{
			if (target == null)
			{
				return;
			}
			SetDesired();
			if (!lockXLocAxis || !lockYLocAxis || !lockZLocAxis)
			{
				base.transform.position = ((!(dampSpeed > 0f)) ? desiredPosition : Vector3.Lerp(base.transform.position, desiredPosition, Time.deltaTime * dampSpeed));
			}
			if (!lockFOV)
			{
				if (base.Camera.orthographic)
				{
					base.Camera.orthographicSize = ((!(dampSpeed > 0f)) ? desiredFOV : Mathf.Lerp(base.Camera.orthographicSize, desiredFOV, Time.deltaTime * dampSpeed));
				}
				else
				{
					base.Camera.fieldOfView = ((!(dampSpeed > 0f)) ? desiredFOV : Mathf.Lerp(base.Camera.fieldOfView, desiredFOV, Time.deltaTime * dampSpeed));
				}
			}
			float x = base.transform.eulerAngles.x;
			if (!lockXRotAxis)
			{
				float num = base.transform.eulerAngles.x;
				if (num > 180f)
				{
					num -= 360f;
				}
				x = ((!(dampSpeed > 0f)) ? desiredPitch : Mathf.Lerp(num, desiredPitch, Time.deltaTime * dampSpeed));
			}
			if (!lockYRotAxis)
			{
				if (yRotConstrainType == CameraRotConstrainType.LookAtTarget)
				{
					if ((bool)target)
					{
						Vector3 position = target.position;
						position.y += targetHeight;
						position.x += targetXOffset;
						position.z += targetZOffset;
						Vector3 forward = position - base.transform.position;
						if (!Mathf.Approximately(directionInfluence, 0f))
						{
							forward += base.TargetForward * directionInfluence;
						}
						Quaternion quaternion = Quaternion.LookRotation(forward);
						Quaternion rotation = ((!(dampSpeed > 0f)) ? quaternion : Quaternion.Slerp(base.transform.rotation, quaternion, Time.deltaTime * dampSpeed));
						if (limitY)
						{
							Vector3 eulerAngles = rotation.eulerAngles;
							eulerAngles.y = ConstrainAxis(eulerAngles.y, constrainY);
							base.transform.eulerAngles = eulerAngles;
						}
						else
						{
							base.transform.rotation = rotation;
						}
					}
					else if (!targetIsPlayer)
					{
						ACDebug.LogWarning(base.name + " has no target", base.gameObject);
					}
				}
				else
				{
					float num2 = base.transform.eulerAngles.y;
					if (desiredSpin > num2 + 180f)
					{
						desiredSpin -= 360f;
					}
					else if (num2 > desiredSpin + 180f)
					{
						num2 -= 360f;
					}
					float y = ((!(dampSpeed > 0f)) ? desiredSpin : Mathf.Lerp(num2, desiredSpin, Time.deltaTime * dampSpeed));
					base.transform.eulerAngles = new Vector3(x, y, base.transform.eulerAngles.z);
				}
			}
			else
			{
				base.transform.eulerAngles = new Vector3(x, base.transform.eulerAngles.y, base.transform.eulerAngles.z);
			}
			SetFocalPoint();
		}

		public override void SwitchTarget(Transform _target)
		{
			base.SwitchTarget(_target);
			originalTargetPosition = Vector3.zero;
			SetTargetOriginalPosition();
		}

		public override void MoveCameraInstant()
		{
			if (targetIsPlayer && (bool)KickStarter.player)
			{
				target = KickStarter.player.transform;
			}
			SetOriginalPosition();
			SetDesired();
			if (!lockXLocAxis || !lockYLocAxis || !lockZLocAxis)
			{
				base.transform.position = desiredPosition;
			}
			float x = base.transform.eulerAngles.x;
			if (!lockXRotAxis)
			{
				x = desiredPitch;
			}
			if (!lockYRotAxis)
			{
				if (yRotConstrainType == CameraRotConstrainType.LookAtTarget)
				{
					if ((bool)target)
					{
						Vector3 position = target.position;
						position.y += targetHeight;
						position.x += targetXOffset;
						position.z += targetZOffset;
						Quaternion rotation = Quaternion.LookRotation(position - base.transform.position);
						if (limitY)
						{
							Vector3 eulerAngles = rotation.eulerAngles;
							eulerAngles.y = ConstrainAxis(eulerAngles.y, constrainY);
							base.transform.eulerAngles = eulerAngles;
						}
						else
						{
							base.transform.rotation = rotation;
						}
					}
				}
				else
				{
					base.transform.eulerAngles = new Vector3(x, desiredSpin, base.transform.eulerAngles.z);
				}
			}
			else
			{
				base.transform.eulerAngles = new Vector3(x, base.transform.eulerAngles.y, base.transform.eulerAngles.z);
			}
			SetDesiredFOV();
			if (!lockFOV)
			{
				base.Camera.fieldOfView = desiredFOV;
			}
			SetFocalPoint();
		}

		protected void SetTargetOriginalPosition()
		{
			if (!(originalTargetPosition == Vector3.zero))
			{
				return;
			}
			if (target == null)
			{
				ResetTarget();
			}
			if (actFromDefaultPlayerStart)
			{
				if (KickStarter.sceneSettings != null && KickStarter.sceneSettings.defaultPlayerStart != null)
				{
					originalTargetPosition = KickStarter.sceneSettings.defaultPlayerStart.transform.position;
				}
				else
				{
					originalTargetPosition = target.position;
				}
			}
			else
			{
				originalTargetPosition = target.position;
			}
		}

		protected void TrackTarget2D_X()
		{
			if (target.position.x < base.transform.position.x - xFreedom)
			{
				desiredPosition.x = target.position.x + xFreedom;
			}
			else if (target.position.x > base.transform.position.x + xFreedom)
			{
				desiredPosition.x = target.position.x - xFreedom;
			}
			desiredPosition.x += xOffset;
		}

		protected void TrackTarget2D_Y()
		{
			if (target.position.y < base.transform.position.y - yFreedom)
			{
				desiredPosition.y = target.position.y + yFreedom;
			}
			else if (target.position.y > base.transform.position.y + yFreedom)
			{
				desiredPosition.y = target.position.y - yFreedom;
			}
			desiredPosition.y += yOffsetLoc;
		}

		protected void TrackTarget2D_Z()
		{
			if (target.position.z < base.transform.position.z - zFreedom)
			{
				desiredPosition.z = target.position.z + zFreedom;
			}
			else if (target.position.z > base.transform.position.z + zFreedom)
			{
				desiredPosition.z = target.position.z - zFreedom;
			}
			desiredPosition.z += zOffset;
		}

		protected float GetDesiredPosition(float originalValue, float gradient, float offset, CameraLocConstrainType constrainType)
		{
			float num = originalValue + offset;
			switch (constrainType)
			{
			case CameraLocConstrainType.TargetX:
				num += (target.position.x - originalTargetPosition.x) * gradient;
				break;
			case CameraLocConstrainType.TargetZ:
				num += (target.position.z - originalTargetPosition.z) * gradient;
				break;
			case CameraLocConstrainType.TargetIntoScreen:
				num += (PositionRelativeToCamera(originalTargetPosition).x - PositionRelativeToCamera(target.position).x) * gradient;
				break;
			case CameraLocConstrainType.TargetAcrossScreen:
				num += (PositionRelativeToCamera(originalTargetPosition).z - PositionRelativeToCamera(target.position).z) * gradient;
				break;
			case CameraLocConstrainType.TargetHeight:
				num += (target.position.y - originalTargetPosition.y) * gradient;
				break;
			}
			return num;
		}

		protected bool AllLocked()
		{
			if (lockXLocAxis && lockYLocAxis && lockZLocAxis && lockXRotAxis && lockYRotAxis && lockFOV)
			{
				return true;
			}
			return false;
		}

		protected void SetFocalPoint()
		{
			if (focalPointIsTarget && target != null)
			{
				focalDistance = Vector3.Dot(base.transform.forward, target.position - base.transform.position);
				if (focalDistance < 0f)
				{
					focalDistance = 0f;
				}
			}
		}

		protected void SetOriginalPosition()
		{
			if (!haveSetOriginalPosition)
			{
				originalPosition = base.transform.position;
				originalSpin = base.transform.eulerAngles.y;
				originalPitch = base.transform.eulerAngles.x;
				if (base.Camera != null)
				{
					originalFOV = base.Camera.fieldOfView;
				}
				haveSetOriginalPosition = true;
			}
		}

		protected void SetDesired()
		{
			if (lockXLocAxis)
			{
				desiredPosition.x = base.transform.position.x;
			}
			else
			{
				if ((bool)target)
				{
					if (xLocConstrainType == CameraLocConstrainType.SideScrolling)
					{
						TrackTarget2D_X();
					}
					else
					{
						desiredPosition.x = GetDesiredPosition(originalPosition.x, xGradient, xOffset, xLocConstrainType);
					}
				}
				if (limitX)
				{
					desiredPosition.x = ConstrainAxis(desiredPosition.x, constrainX);
				}
			}
			if (lockYLocAxis)
			{
				desiredPosition.y = base.transform.position.y;
			}
			else
			{
				if ((bool)target)
				{
					if (yLocConstrainType == CameraLocConstrainType.SideScrolling)
					{
						TrackTarget2D_Y();
					}
					else
					{
						desiredPosition.y = GetDesiredPosition(originalPosition.y, yGradientLoc, yOffsetLoc, yLocConstrainType);
					}
				}
				if (limitYLoc)
				{
					desiredPosition.y = ConstrainAxis(desiredPosition.y, constrainYLoc);
				}
			}
			if (lockXRotAxis)
			{
				desiredPitch = base.transform.eulerAngles.x;
			}
			else
			{
				if ((bool)target && xRotConstrainType != CameraLocConstrainType.SideScrolling)
				{
					desiredPitch = GetDesiredPosition(originalPitch, xGradientRot, xOffsetRot, xRotConstrainType);
				}
				if (limitXRot)
				{
					desiredPitch = ConstrainAxis(desiredPitch, constrainXRot);
				}
				desiredPitch = Mathf.Clamp(desiredPitch, -85f, 85f);
			}
			if (lockYRotAxis)
			{
				desiredSpin = 0f;
			}
			else
			{
				if ((bool)target)
				{
					desiredSpin = GetDesiredPosition(originalSpin, yGradient, yOffset, (CameraLocConstrainType)yRotConstrainType);
					if (!Mathf.Approximately(directionInfluence, 0f))
					{
						desiredSpin += Vector3.Dot(base.TargetForward, base.transform.right) * directionInfluence;
					}
				}
				if (limitY)
				{
					desiredSpin = ConstrainAxis(desiredSpin, constrainY);
				}
			}
			if (lockZLocAxis)
			{
				desiredPosition.z = base.transform.position.z;
			}
			else
			{
				if ((bool)target)
				{
					if (zLocConstrainType == CameraLocConstrainType.SideScrolling)
					{
						TrackTarget2D_Z();
					}
					else
					{
						desiredPosition.z = GetDesiredPosition(originalPosition.z, zGradient, zOffset, zLocConstrainType);
					}
				}
				if (limitZ)
				{
					desiredPosition.z = ConstrainAxis(desiredPosition.z, constrainZ);
				}
			}
			SetDesiredFOV();
		}

		protected void SetDesiredFOV()
		{
			if (lockFOV)
			{
				if (base.Camera.orthographic)
				{
					desiredFOV = base.Camera.orthographicSize;
				}
				else
				{
					desiredFOV = base.Camera.fieldOfView;
				}
				return;
			}
			if ((bool)target)
			{
				desiredFOV = GetDesiredPosition(originalFOV, FOVGradient, FOVOffset, CameraLocConstrainType.TargetIntoScreen);
			}
			if (limitFOV)
			{
				desiredFOV = ConstrainAxis(desiredFOV, constrainFOV);
			}
		}
	}
}
