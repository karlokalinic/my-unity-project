using System;
using System.Collections;
using UnityEngine;

public class bl_CameraOrbit : bl_CameraBase
{
	[HideInInspector]
	public bool m_Interact = true;

	[Header("Target")]
	public Transform Target;

	[Header("Settings")]
	public bool isForMobile;

	public bool AutoTakeInfo = true;

	public float Distance = 5f;

	[Range(0.01f, 5f)]
	public float SwichtSpeed = 2f;

	public Vector2 DistanceClamp = new Vector2(1.5f, 5f);

	public Vector2 YLimitClamp = new Vector2(-20f, 80f);

	public Vector2 XLimitClamp = new Vector2(360f, 360f);

	public Vector2 SpeedAxis = new Vector2(100f, 100f);

	public bool LockCursorOnRotate = true;

	[Header("Input")]
	public bool RequiredInput = true;

	public CameraMouseInputType RotateInputKey = CameraMouseInputType.LeftAndRight;

	[Range(0.001f, 0.07f)]
	public float InputMultiplier = 0.02f;

	[Range(0.1f, 15f)]
	public float InputLerp = 7f;

	public bool useKeys;

	[Header("Movement")]
	public CameraMovementType MovementType;

	[Range(-90f, 90f)]
	public float TouchZoomAmount = -5f;

	[Range(0.1f, 20f)]
	public float LerpSpeed = 7f;

	[Range(1f, 100f)]
	public float OutInputSpeed = 20f;

	[Header("Fog")]
	[Range(5f, 179f)]
	public float StartFov = 179f;

	[Range(0.1f, 15f)]
	public float FovLerp = 7f;

	[Range(0f, 7f)]
	public float DelayStartFoV = 1.2f;

	[Range(1f, 10f)]
	public float ScrollSensitivity = 5f;

	[Range(1f, 25f)]
	public float ZoomSpeed = 7f;

	[Header("Auto Rotation")]
	public bool AutoRotate = true;

	public CameraAutoRotationType AutoRotationType;

	[Range(0f, 20f)]
	public float AutoRotSpeed = 5f;

	[Header("Collision")]
	public bool DetectCollision = true;

	public bool TeleporOnHit = true;

	[Range(0.01f, 4f)]
	public float CollisionRadius = 2f;

	public LayerMask DetectCollisionLayers;

	[Header("Fade")]
	public bool FadeOnStart = true;

	[Range(0.01f, 5f)]
	public float FadeSpeed = 2f;

	[SerializeField]
	private Texture2D FadeTexture;

	private float y;

	private float x;

	private Ray Ray;

	private bool LastHaveInput;

	private float distance;

	private float currentFog = 60f;

	private float defaultFog;

	private float horizontal;

	private float vertical;

	private float defaultAutoSpeed;

	private float lastHorizontal;

	private bool canFogControl;

	private bool haveHit;

	private float LastDistance;

	private bool m_CanRotate = true;

	private Vector3 ZoomVector;

	private Quaternion CurrentRotation;

	private Vector3 CurrentPosition;

	private float FadeAlpha = 1f;

	private bool isSwitchingTarget;

	private bool isDetectingHit;

	private float initXRotation;

	private bool isInputKeyRotate
	{
		get
		{
			switch (RotateInputKey)
			{
			case CameraMouseInputType.All:
				return Input.GetKey(KeyCode.Mouse0) || Input.GetKey(KeyCode.Mouse1) || Input.GetKey(KeyCode.Mouse2) || Input.GetMouseButton(0);
			case CameraMouseInputType.LeftAndRight:
				return Input.GetKey(KeyCode.Mouse0) || Input.GetKey(KeyCode.Mouse1);
			case CameraMouseInputType.LeftMouse:
				return Input.GetKey(KeyCode.Mouse0);
			case CameraMouseInputType.RightMouse:
				return Input.GetKey(KeyCode.Mouse1);
			case CameraMouseInputType.MouseScroll:
				return Input.GetKey(KeyCode.Mouse2);
			case CameraMouseInputType.MobileTouch:
				return Input.GetMouseButton(0) || Input.GetMouseButton(1);
			default:
				return Input.GetKey(KeyCode.Mouse0);
			}
		}
	}

	private bool isInputUpKeyRotate
	{
		get
		{
			switch (RotateInputKey)
			{
			case CameraMouseInputType.All:
				return Input.GetKeyUp(KeyCode.Mouse0) || Input.GetKeyUp(KeyCode.Mouse1) || Input.GetKeyUp(KeyCode.Mouse2) || Input.GetMouseButtonUp(0);
			case CameraMouseInputType.LeftAndRight:
				return Input.GetKeyUp(KeyCode.Mouse0) || Input.GetKeyUp(KeyCode.Mouse1);
			case CameraMouseInputType.LeftMouse:
				return Input.GetKeyUp(KeyCode.Mouse0) || Input.GetMouseButtonUp(0);
			case CameraMouseInputType.RightMouse:
				return Input.GetKeyUp(KeyCode.Mouse1);
			case CameraMouseInputType.MouseScroll:
				return Input.GetKeyUp(KeyCode.Mouse2);
			case CameraMouseInputType.MobileTouch:
				return Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1);
			default:
				return Input.GetKey(KeyCode.Mouse0) || Input.GetMouseButton(0);
			}
		}
	}

	public float Horizontal
	{
		get
		{
			return horizontal;
		}
		set
		{
			horizontal += value;
			lastHorizontal = horizontal;
		}
	}

	public float Vertical
	{
		get
		{
			return vertical;
		}
		set
		{
			vertical += value;
		}
	}

	public bool Interact
	{
		get
		{
			return m_Interact;
		}
		set
		{
			m_Interact = value;
		}
	}

	public bool CanRotate
	{
		get
		{
			return m_CanRotate;
		}
		set
		{
			m_CanRotate = value;
		}
	}

	public float AutoRotationSpeed
	{
		get
		{
			return defaultAutoSpeed;
		}
		set
		{
			defaultAutoSpeed = value;
		}
	}

	private void Start()
	{
		SetUp();
	}

	private void SetUp()
	{
		if (AutoTakeInfo)
		{
			distance = Vector3.Distance(base.transform.position, Target.position);
			Distance = distance;
			Vector3 eulerAngles = base.Transform.eulerAngles;
			x = eulerAngles.y;
			y = eulerAngles.x;
			initXRotation = eulerAngles.y;
			horizontal = x;
			vertical = y;
		}
		else
		{
			distance = Distance;
		}
		currentFog = base.GetCamera.fieldOfView;
		defaultFog = currentFog;
		base.GetCamera.fieldOfView = StartFov;
		defaultAutoSpeed = AutoRotSpeed;
		StartCoroutine(IEDelayFog());
		if (RotateInputKey == CameraMouseInputType.MobileTouch && UnityEngine.Object.FindObjectOfType<bl_OrbitTouchPad>() == null)
		{
			Debug.LogWarning("For use  mobile touched be sure to put the 'OrbitTouchArea in the canvas of scene");
		}
	}

	private void LateUpdate()
	{
		if (Target == null)
		{
			Debug.LogWarning("Target is not assigned to orbit camera!", this);
		}
		else
		{
			if (isSwitchingTarget)
			{
				return;
			}
			if (CanRotate)
			{
				ZoomControll(false);
				OrbitControll();
				if (AutoRotate && !isInputKeyRotate)
				{
					AutoRotation();
				}
			}
			else
			{
				ZoomControll(true);
			}
			if (m_Interact)
			{
				FogControl();
				InputControl();
			}
		}
	}

	private void InputControl()
	{
		if (LockCursorOnRotate && !useKeys && !isForMobile)
		{
			if (!isInputKeyRotate && LastHaveInput)
			{
				if (LockCursorOnRotate && Interact)
				{
					bl_CameraUtils.LockCursor(false);
				}
				LastHaveInput = false;
				if (lastHorizontal >= 0f)
				{
					AutoRotSpeed = OutInputSpeed;
				}
				else
				{
					AutoRotSpeed = 0f - OutInputSpeed;
				}
			}
			if (isInputKeyRotate && !LastHaveInput)
			{
				if (LockCursorOnRotate && Interact)
				{
					bl_CameraUtils.LockCursor(true);
				}
				LastHaveInput = true;
			}
		}
		if (isInputUpKeyRotate)
		{
			currentFog -= TouchZoomAmount;
		}
	}

	private void AutoRotation()
	{
		switch (AutoRotationType)
		{
		case CameraAutoRotationType.Dinamicaly:
			AutoRotSpeed = ((!(lastHorizontal > 0f)) ? Mathf.Lerp(AutoRotSpeed, 0f - defaultAutoSpeed, Time.deltaTime / 2f) : Mathf.Lerp(AutoRotSpeed, defaultAutoSpeed, Time.deltaTime / 2f));
			break;
		case CameraAutoRotationType.Left:
			AutoRotSpeed = Mathf.Lerp(AutoRotSpeed, defaultAutoSpeed, Time.deltaTime / 2f);
			break;
		case CameraAutoRotationType.Right:
			AutoRotSpeed = Mathf.Lerp(AutoRotSpeed, 0f - defaultAutoSpeed, Time.deltaTime / 2f);
			break;
		}
		horizontal += Time.deltaTime * AutoRotSpeed;
	}

	private void FogControl()
	{
		if (canFogControl)
		{
			currentFog = Mathf.SmoothStep(currentFog, defaultFog, Time.deltaTime * FovLerp);
			base.GetCamera.fieldOfView = Mathf.Lerp(base.GetCamera.fieldOfView, currentFog, Time.deltaTime * FovLerp);
		}
	}

	private void OrbitControll()
	{
		if (m_Interact && !isForMobile)
		{
			if ((RequiredInput && !useKeys && isInputKeyRotate) || !RequiredInput)
			{
				horizontal += SpeedAxis.x * InputMultiplier * base.AxisX;
				vertical -= SpeedAxis.y * InputMultiplier * base.AxisY;
				lastHorizontal = base.AxisX;
			}
			else if (useKeys)
			{
				horizontal -= base.KeyAxisX * SpeedAxis.x * InputMultiplier;
				vertical += base.KeyAxisY * SpeedAxis.y * InputMultiplier;
				lastHorizontal = base.KeyAxisX;
			}
		}
		vertical = bl_CameraUtils.ClampAngle(vertical, YLimitClamp.x, YLimitClamp.y);
		if (XLimitClamp.x < 360f && XLimitClamp.y < 360f)
		{
			horizontal = bl_CameraUtils.ClampAngle(horizontal, initXRotation - XLimitClamp.y, XLimitClamp.x + initXRotation);
		}
		x = Mathf.Lerp(x, horizontal, Time.deltaTime * InputLerp);
		y = Mathf.Lerp(y, vertical, Time.deltaTime * InputLerp);
		y = bl_CameraUtils.ClampAngle(y, YLimitClamp.x, YLimitClamp.y);
		CurrentRotation = Quaternion.Euler(y, x, 0f);
		CurrentPosition = CurrentRotation * ZoomVector + Target.position;
		switch (MovementType)
		{
		case CameraMovementType.Dynamic:
			base.Transform.position = Vector3.Lerp(base.Transform.position, CurrentPosition, LerpSpeed * Time.deltaTime);
			base.Transform.rotation = Quaternion.Lerp(base.Transform.rotation, CurrentRotation, LerpSpeed * 2f * Time.deltaTime);
			break;
		case CameraMovementType.Normal:
			base.Transform.rotation = CurrentRotation;
			base.Transform.position = CurrentPosition;
			break;
		case CameraMovementType.Towars:
			base.Transform.rotation = Quaternion.RotateTowards(base.Transform.rotation, CurrentRotation, LerpSpeed);
			base.Transform.position = Vector3.MoveTowards(base.Transform.position, CurrentPosition, LerpSpeed);
			break;
		}
	}

	private void ZoomControll(bool autoApply)
	{
		bool flag = false;
		float deltaTime = Time.deltaTime;
		distance = Mathf.Clamp(distance - base.MouseScrollWheel * ScrollSensitivity, DistanceClamp.x, DistanceClamp.y);
		if (DetectCollision)
		{
			Vector3 vector = base.Transform.position - Target.position;
			Ray = new Ray(Target.position, vector.normalized);
			RaycastHit hitInfo;
			if (Physics.SphereCast(Ray.origin, CollisionRadius, Ray.direction, out hitInfo, distance, DetectCollisionLayers))
			{
				if (!haveHit)
				{
					LastDistance = distance;
					haveHit = true;
				}
				distance = Mathf.Clamp(hitInfo.distance, DistanceClamp.x, DistanceClamp.y);
				if (TeleporOnHit)
				{
					Distance = distance;
				}
				flag = true;
			}
			else if (!isDetectingHit)
			{
				StartCoroutine(DetectHit());
			}
			distance = ((!(distance < 1f)) ? distance : 1f);
			if (!haveHit || !TeleporOnHit)
			{
				float num = ((!flag) ? 1f : ((float)Math.PI));
				Distance = Mathf.SmoothStep(Distance, distance, deltaTime * (ZoomSpeed * num));
			}
		}
		else
		{
			distance = ((!(distance < 1f)) ? distance : 1f);
			Distance = Mathf.SmoothStep(Distance, distance, deltaTime * ZoomSpeed);
		}
		ZoomVector = new Vector3(0f, 0f, 0f - Distance);
		if (autoApply)
		{
			CurrentPosition = CurrentRotation * ZoomVector + Target.position;
			switch (MovementType)
			{
			case CameraMovementType.Dynamic:
				base.Transform.position = Vector3.Lerp(base.Transform.position, CurrentPosition, LerpSpeed * deltaTime);
				base.Transform.rotation = Quaternion.Lerp(base.Transform.rotation, CurrentRotation, LerpSpeed * 2f * deltaTime);
				break;
			case CameraMovementType.Normal:
				base.Transform.rotation = CurrentRotation;
				base.Transform.position = CurrentPosition;
				break;
			case CameraMovementType.Towars:
				base.Transform.rotation = Quaternion.RotateTowards(base.Transform.rotation, CurrentRotation, LerpSpeed);
				base.Transform.position = Vector3.MoveTowards(base.Transform.position, CurrentPosition, LerpSpeed);
				break;
			}
		}
	}

	private void OnGUI()
	{
		if (isSwitchingTarget)
		{
			GUI.color = new Color(1f, 1f, 1f, FadeAlpha);
			GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), FadeTexture, ScaleMode.StretchToFill);
		}
		else if (FadeOnStart && FadeAlpha > 0f)
		{
			FadeAlpha -= Time.deltaTime * FadeSpeed;
			GUI.color = new Color(1f, 1f, 1f, FadeAlpha);
			GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), FadeTexture, ScaleMode.StretchToFill);
		}
	}

	public void SetTarget(Transform newTarget)
	{
		StopCoroutine("TranslateTarget");
		StartCoroutine("TranslateTarget", newTarget);
	}

	public void SetViewPoint(int side)
	{
		AutoRotate = false;
		switch (side)
		{
		case 0:
			vertical = 90f;
			horizontal = 0f;
			break;
		case 1:
			vertical = 0f;
			horizontal = 0f;
			break;
		case 2:
			vertical = 0f;
			horizontal = -90f;
			break;
		case 3:
			vertical = 0f;
			horizontal = 90f;
			break;
		case 4:
			vertical = 0f;
			horizontal = 180f;
			break;
		}
	}

	private IEnumerator TranslateTarget(Transform newTarget)
	{
		isSwitchingTarget = true;
		while (FadeAlpha < 1f)
		{
			base.transform.position = Vector3.Lerp(base.transform.position, base.transform.position + new Vector3(0f, 2f, -2f), Time.deltaTime);
			FadeAlpha += Time.smoothDeltaTime * SwichtSpeed;
			yield return null;
		}
		Target = newTarget;
		isSwitchingTarget = false;
		while (FadeAlpha > 0f)
		{
			FadeAlpha -= Time.smoothDeltaTime * SwichtSpeed;
			yield return null;
		}
	}

	private IEnumerator DetectHit()
	{
		isDetectingHit = true;
		yield return new WaitForSeconds(0.4f);
		if (haveHit)
		{
			distance = LastDistance;
			haveHit = false;
		}
		isDetectingHit = false;
	}

	private IEnumerator IEDelayFog()
	{
		yield return new WaitForSeconds(DelayStartFoV);
		canFogControl = true;
	}

	public void SetZoom(float value)
	{
		distance += (0f - value * 0.5f) * ScrollSensitivity;
	}

	public void SetStaticZoom(float value)
	{
		distance += value;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = new Color32(0, 221, 221, byte.MaxValue);
		if (Target != null)
		{
			Gizmos.DrawLine(base.transform.position, Target.position);
			Gizmos.matrix = Matrix4x4.TRS(Target.position, base.transform.rotation, new Vector3(1f, 0f, 1f));
			Gizmos.DrawWireSphere(Target.position, Distance);
			Gizmos.matrix = Matrix4x4.identity;
		}
		Gizmos.DrawCube(base.transform.position, new Vector3(1f, 0.2f, 0.2f));
		Gizmos.DrawCube(base.transform.position, Vector3.one / 2f);
	}
}
