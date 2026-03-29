using UnityEngine;
using UnityEngine.Serialization;

public class FreelookCamera : MonoBehaviour
{
	[Tooltip("Default speed in m/s")]
	public float Speed = 10f;

	[FormerlySerializedAs("EnableFastSpeed")]
	[Tooltip("Whether to use the faster speed option while holding the boost key")]
	public bool EnableBoostSpeed = true;

	[FormerlySerializedAs("FastSpeed")]
	[Tooltip("Speed in m/s while holding the boost key")]
	public float BoostSpeed = 20f;

	[Tooltip("Hotkey used to boost movement speed")]
	public KeyCode BoostKey = KeyCode.LeftShift;

	[Tooltip("The speed at which your camera rotates when moving the mouse")]
	public float MouseSensitivity = 3f;

	[Tooltip("Whether the freelook is initially enabled")]
	public bool IsEnabled = true;

	[Tooltip("Whether to lock the cursor while using the freelook camera")]
	public bool LockCursor = true;

	[Tooltip("The hotkey used to enable or disable the freelook camera script")]
	public KeyCode ToggleKey = KeyCode.T;

	[Tooltip("Hotkey used to move upwards on the vertical world axis")]
	public KeyCode UpKey = KeyCode.Space;

	[Tooltip("Hotkey used to move downwards on the vertical world axis")]
	public KeyCode DownKey = KeyCode.C;

	private Quaternion originalRotation;

	private float rotationX;

	private float rotationY;

	private Transform myTransform;

	private bool wasUsingKinematic;

	public void Start()
	{
		myTransform = base.transform;
		originalRotation = myTransform.localRotation;
		if (IsEnabled)
		{
			EnableNoClip();
		}
	}

	public void OnEnable()
	{
		if (IsEnabled)
		{
			EnableNoClip();
		}
	}

	public void OnDisable()
	{
		if (IsEnabled)
		{
			DisableNoClip();
		}
	}

	public void Update()
	{
		if (Input.GetKeyUp(ToggleKey))
		{
			if (IsEnabled)
			{
				DisableNoClip();
			}
			else
			{
				EnableNoClip();
			}
		}
		if (LockCursor)
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
		float num = Speed;
		if (EnableBoostSpeed && Input.GetKey(BoostKey))
		{
			num = BoostSpeed;
		}
		float num2 = 0f;
		if (Input.GetKey(UpKey))
		{
			num2 += 1f;
		}
		if (Input.GetKey(DownKey))
		{
			num2 -= 1f;
		}
		if (IsEnabled)
		{
			rotationX += Input.GetAxis("Mouse X") * MouseSensitivity;
			rotationY += Input.GetAxis("Mouse Y") * MouseSensitivity;
			rotationY = Mathf.Clamp(rotationY, -89f, 89f);
			Quaternion quaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
			Quaternion quaternion2 = Quaternion.AngleAxis(rotationY, -Vector3.right);
			myTransform.rotation = originalRotation * quaternion * quaternion2;
			num *= Time.deltaTime;
			num2 *= num;
			float num3 = Input.GetAxis("Vertical") * num;
			float num4 = Input.GetAxis("Horizontal") * num;
			Vector3 forward = myTransform.forward;
			Vector3 right = myTransform.right;
			myTransform.position += forward * num3 + right * num4 + Vector3.up * num2;
		}
	}

	private void EnableNoClip()
	{
		Behaviour behaviour = base.gameObject.GetComponent("CharacterMotor") as MonoBehaviour;
		if (behaviour != null)
		{
			behaviour.enabled = false;
		}
		Rigidbody component = base.gameObject.GetComponent<Rigidbody>();
		if (component != null)
		{
			wasUsingKinematic = component.isKinematic;
			if (!component.isKinematic)
			{
				component.isKinematic = true;
			}
		}
		IsEnabled = true;
	}

	private void DisableNoClip()
	{
		Behaviour behaviour = base.gameObject.GetComponent("CharacterMotor") as MonoBehaviour;
		if (behaviour != null)
		{
			behaviour.enabled = true;
		}
		Rigidbody component = base.gameObject.GetComponent<Rigidbody>();
		if (component != null)
		{
			if (!component.isKinematic)
			{
				return;
			}
			component.isKinematic = wasUsingKinematic;
		}
		IsEnabled = false;
	}
}
