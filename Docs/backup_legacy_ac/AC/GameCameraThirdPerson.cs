using UnityEngine;

namespace AC
{
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_game_camera_third_person.html")]
	public class GameCameraThirdPerson : _Camera
	{
		public RotationLock spinLock;

		public RotationLock pitchLock = RotationLock.Locked;

		public bool canRotateDuringConversations;

		public bool canRotateDuringCutscenes;

		public float horizontalOffset;

		public float verticalOffset = 2f;

		public float distance = 2f;

		public bool allowMouseWheelZooming;

		public bool detectCollisions = true;

		public float collisionRadius = 0.3f;

		public LayerMask collisionLayerMask;

		public float minDistance = 1f;

		public float maxDistance = 3f;

		public bool focalPointIsTarget;

		public bool toggleCursor;

		public float spinSpeed = 5f;

		public float spinAccleration = 5f;

		public float spinDeceleration = 5f;

		public string spinAxis = string.Empty;

		public bool invertSpin;

		public bool alwaysBehind;

		public bool resetSpinWhenSwitch;

		public float spinOffset;

		public float maxSpin = 40f;

		public float pitchSpeed = 3f;

		public float pitchAccleration = 20f;

		public float pitchDeceleration = 20f;

		public float maxPitch = 40f;

		public float minPitch = -40f;

		public string pitchAxis = string.Empty;

		public bool invertPitch;

		public bool resetPitchWhenSwitch;

		public bool inputAffectsSpeed;

		protected Vector3 actualCollisionOffset;

		protected float deltaDistance;

		protected float deltaSpin;

		protected float deltaPitch;

		protected float roll;

		protected float spin;

		protected float pitch;

		protected float initialPitch;

		protected float initialSpin;

		protected Vector3 centrePosition;

		protected Vector3 targetPosition;

		protected Quaternion targetRotation;

		protected bool autoControlPitch;

		protected bool autoControlSpin;

		protected float autoControlTime;

		protected float autoControlStartTime;

		protected float autoPitchAngle;

		protected float autoSpinAngle;

		protected MoveMethod autoMoveMethod;

		protected AnimationCurve autoMoveCurve;

		protected override void Awake()
		{
			base.Awake();
			targetRotation = base.transform.rotation;
			initialPitch = base.transform.eulerAngles.x;
			initialSpin = base.transform.eulerAngles.y;
			autoControlPitch = (autoControlSpin = false);
		}

		protected override void Start()
		{
			base.Start();
			ResetTarget();
			Vector3 eulerAngles = base.transform.eulerAngles;
			spin = eulerAngles.y;
			roll = eulerAngles.z;
			UpdateTargets(true);
			SnapMovement();
		}

		public override void _Update()
		{
			UpdateTargets();
			DetectCollisions();
			UpdateSelf();
		}

		public void ResetRotation()
		{
			if (pitchLock != RotationLock.Locked && resetPitchWhenSwitch)
			{
				pitch = initialPitch;
			}
			if (spinLock != RotationLock.Locked && resetSpinWhenSwitch)
			{
				spin = initialSpin;
			}
			autoControlPitch = (autoControlSpin = false);
		}

		public void ForceRotation(bool _controlPitch, float _newPitchAngle, bool _controlSpin, float _newSpinAngle, float _transitionTime = 0f, MoveMethod moveMethod = MoveMethod.Linear, AnimationCurve timeCurve = null)
		{
			autoControlPitch = false;
			autoControlPitch = false;
			if (!_controlPitch && !_controlSpin)
			{
				return;
			}
			if (_transitionTime > 0f)
			{
				autoControlPitch = _controlPitch;
				autoControlSpin = _controlSpin;
				autoPitchAngle = _newPitchAngle;
				autoSpinAngle = _newSpinAngle;
				autoMoveMethod = moveMethod;
				autoControlTime = _transitionTime;
				autoControlStartTime = Time.time;
				autoMoveCurve = timeCurve;
			}
			else
			{
				if (_controlPitch)
				{
					pitch = _newPitchAngle;
				}
				if (_controlSpin)
				{
					spin = _newSpinAngle;
				}
			}
		}

		protected void DetectCollisions()
		{
			if (detectCollisions && target != null)
			{
				Vector3 vector = target.position + new Vector3(0f, verticalOffset, 0f) - targetPosition;
				RaycastHit hitInfo;
				if (Physics.SphereCast(vector + targetPosition, collisionRadius, -vector.normalized, out hitInfo, vector.magnitude, collisionLayerMask))
				{
					float a = vector.magnitude - hitInfo.distance;
					float b = vector.magnitude - minDistance;
					a = Mathf.Min(a, b);
					actualCollisionOffset = vector.normalized * a;
				}
				else
				{
					actualCollisionOffset = Vector3.zero;
				}
			}
		}

		protected void UpdateSelf()
		{
			base.transform.rotation = targetRotation;
			base.transform.position = targetPosition + actualCollisionOffset;
		}

		protected void UpdateTargets(bool onStart = false)
		{
			if (!target)
			{
				return;
			}
			if (autoControlPitch || autoControlSpin)
			{
				if (Time.time > autoControlStartTime + autoControlTime)
				{
					autoControlPitch = (autoControlSpin = false);
				}
				else
				{
					if (autoControlPitch)
					{
						pitch = Mathf.Lerp(pitch, autoPitchAngle, AdvGame.Interpolate(autoControlStartTime, autoControlTime, autoMoveMethod, autoMoveCurve));
					}
					if (autoControlSpin)
					{
						spin = Mathf.Lerp(spin, autoSpinAngle, AdvGame.Interpolate(autoControlStartTime, autoControlTime, autoMoveMethod, autoMoveCurve));
					}
				}
			}
			else if (onStart || CanAcceptInput())
			{
				if (allowMouseWheelZooming && minDistance < maxDistance)
				{
					if (Input.GetAxis("Mouse ScrollWheel") < 0f)
					{
						deltaDistance = Mathf.Lerp(deltaDistance, Mathf.Min(spinSpeed, maxDistance - distance), spinAccleration / 5f * Time.deltaTime);
					}
					else if (Input.GetAxis("Mouse ScrollWheel") > 0f)
					{
						deltaDistance = Mathf.Lerp(deltaDistance, 0f - Mathf.Min(spinSpeed, distance - minDistance), spinAccleration / 5f * Time.deltaTime);
					}
					else
					{
						deltaDistance = Mathf.Lerp(deltaDistance, 0f, spinAccleration * Time.deltaTime);
					}
					distance += deltaDistance;
					distance = Mathf.Clamp(distance, minDistance, maxDistance);
				}
				if (KickStarter.playerInput.IsCursorLocked() || !toggleCursor)
				{
					if (!isDragControlled)
					{
						inputMovement = new Vector2(KickStarter.playerInput.InputGetAxis(spinAxis), KickStarter.playerInput.InputGetAxis(pitchAxis));
					}
					else if (KickStarter.playerInput.GetDragState() == DragState._Camera)
					{
						if (KickStarter.playerInput.IsCursorLocked())
						{
							inputMovement = KickStarter.playerInput.GetFreeAim();
						}
						else
						{
							inputMovement = KickStarter.playerInput.GetDragVector();
						}
					}
					else
					{
						inputMovement = Vector2.zero;
					}
					if (spinLock != RotationLock.Locked)
					{
						if (Mathf.Approximately(inputMovement.x, 0f))
						{
							deltaSpin = Mathf.Lerp(deltaSpin, 0f, spinDeceleration * Time.deltaTime);
						}
						else
						{
							float num = 1f;
							if (inputAffectsSpeed)
							{
								num *= Mathf.Abs(inputMovement.x);
								if (isDragControlled)
								{
									num /= 1000f;
								}
							}
							if (inputMovement.x > 0f)
							{
								deltaSpin = Mathf.Lerp(deltaSpin, spinSpeed * num, spinAccleration * Time.deltaTime * inputMovement.x);
							}
							else if (inputMovement.x < 0f)
							{
								deltaSpin = Mathf.Lerp(deltaSpin, (0f - spinSpeed) * num, spinAccleration * Time.deltaTime * (0f - inputMovement.x));
							}
						}
						if (spinLock == RotationLock.Limited)
						{
							if ((invertSpin && deltaSpin > 0f) || (!invertSpin && deltaSpin < 0f))
							{
								if (maxSpin - spin < 5f)
								{
									deltaSpin *= (maxSpin - spin) / 5f;
								}
							}
							else if (((invertSpin && deltaSpin < 0f) || (!invertSpin && deltaSpin > 0f)) && maxSpin + spin < 5f)
							{
								deltaSpin *= (maxSpin + spin) / 5f;
							}
						}
						if (invertSpin)
						{
							spin += deltaSpin;
						}
						else
						{
							spin -= deltaSpin;
						}
						if (spinLock == RotationLock.Limited)
						{
							spin = Mathf.Clamp(spin, 0f - maxSpin, maxSpin);
						}
					}
					else if (alwaysBehind)
					{
						spin = Mathf.LerpAngle(spin, target.eulerAngles.y + spinOffset, spinAccleration * Time.deltaTime);
					}
					if (pitchLock != RotationLock.Locked)
					{
						if (Mathf.Approximately(inputMovement.y, 0f))
						{
							deltaPitch = Mathf.Lerp(deltaPitch, 0f, pitchDeceleration * Time.deltaTime);
						}
						else
						{
							float num2 = 1f;
							if (inputAffectsSpeed)
							{
								num2 *= Mathf.Abs(inputMovement.y);
								if (isDragControlled)
								{
									num2 /= 1000f;
								}
							}
							if (inputMovement.y > 0f)
							{
								deltaPitch = Mathf.Lerp(deltaPitch, pitchSpeed * num2, pitchAccleration * Time.deltaTime * inputMovement.y);
							}
							else if (inputMovement.y < 0f)
							{
								deltaPitch = Mathf.Lerp(deltaPitch, (0f - pitchSpeed) * num2, pitchAccleration * Time.deltaTime * (0f - inputMovement.y));
							}
						}
						if (pitchLock == RotationLock.Limited)
						{
							if ((invertPitch && deltaPitch > 0f) || (!invertPitch && deltaPitch < 0f))
							{
								if (maxPitch - pitch < 5f)
								{
									deltaPitch *= (maxPitch - pitch) / 5f;
								}
							}
							else if (((invertPitch && deltaPitch < 0f) || (!invertPitch && deltaPitch > 0f)) && minPitch - pitch > -5f)
							{
								deltaPitch *= (minPitch - pitch) / -5f;
							}
						}
						if (invertPitch)
						{
							pitch += deltaPitch;
						}
						else
						{
							pitch -= deltaPitch;
						}
						if (pitchLock == RotationLock.Limited)
						{
							pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
						}
					}
				}
			}
			else if (spinLock != RotationLock.Free && alwaysBehind)
			{
				spin = Mathf.LerpAngle(spin, target.eulerAngles.y + spinOffset, spinAccleration * Time.deltaTime);
			}
			if (pitchLock == RotationLock.Locked)
			{
				pitch = maxPitch;
			}
			float num3 = spin;
			float num4 = pitch;
			if (alwaysBehind && spinLock == RotationLock.Limited)
			{
				num3 += target.eulerAngles.y;
			}
			else if (!targetIsPlayer)
			{
				if (spinLock != RotationLock.Locked)
				{
					num3 += target.eulerAngles.y;
				}
				if (pitchLock != RotationLock.Locked)
				{
					num4 += target.eulerAngles.x;
				}
			}
			Quaternion quaternion = (targetRotation = Quaternion.Euler(num4, num3, roll));
			centrePosition = target.position + Vector3.up * verticalOffset + quaternion * Vector3.right * horizontalOffset;
			targetPosition = centrePosition - quaternion * Vector3.forward * distance;
			SetFocalPoint();
		}

		protected bool CanAcceptInput()
		{
			if (KickStarter.mainCamera != null && KickStarter.mainCamera.attachedCamera != this)
			{
				return false;
			}
			if (KickStarter.stateHandler == null || KickStarter.stateHandler.gameState == GameState.Normal || (KickStarter.stateHandler.gameState == GameState.Cutscene && canRotateDuringCutscenes) || (KickStarter.stateHandler.gameState == GameState.DialogOptions && canRotateDuringConversations))
			{
				return true;
			}
			return false;
		}

		protected void SnapMovement()
		{
			base.transform.rotation = targetRotation;
			base.transform.position = targetPosition;
			SetFocalPoint();
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
	}
}
