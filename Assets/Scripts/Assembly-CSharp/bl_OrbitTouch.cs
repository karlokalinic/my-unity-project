using System;
using UnityEngine;

public class bl_OrbitTouch : MonoBehaviour
{
	[Header("Movement")]
	[SerializeField]
	private bl_CameraOrbit m_CameraOrbit;

	[SerializeField]
	private Vector2 MovementMultiplier = new Vector2(1f, 1f);

	[Header("Pinch Zoom")]
	public bool CancelRotateOnPinch = true;

	[SerializeField]
	[Range(0.01f, 2f)]
	private float m_PinchZoomSpeed = 0.5f;

	private Vector2 direction;

	private Vector2 smoothDirection;

	private bool touched;

	private int pointerID;

	private bool Pinched;

	private void Awake()
	{
		direction = Vector2.zero;
		touched = false;
	}

	private void Update()
	{
		ControlInput();
	}

	private void ControlInput()
	{
		for (int i = 0; i < Input.touchCount; i++)
		{
			Touch data = Input.touches[i];
			if (data.phase == TouchPhase.Began)
			{
				OnPointerDown(data);
			}
			else if (data.phase == TouchPhase.Moved)
			{
				OnDrag(data);
			}
			else if (data.phase == TouchPhase.Ended)
			{
				OnPointerUp(data);
			}
		}
	}

	public void OnPointerDown(Touch data)
	{
		if (m_CameraOrbit == null)
		{
			Debug.LogWarning("Please assign a camera orbit target");
		}
		else if (!touched)
		{
			touched = true;
			pointerID = data.fingerId;
		}
	}

	public void OnDrag(Touch data)
	{
		if (m_CameraOrbit == null)
		{
			Debug.LogWarning("Please assign a camera orbit target");
			return;
		}
		PinchZoom();
		if (!Pinched && data.fingerId == pointerID)
		{
			direction = data.deltaPosition.normalized;
			m_CameraOrbit.Horizontal = direction.x * MovementMultiplier.x;
			m_CameraOrbit.Vertical = (0f - direction.y) * MovementMultiplier.y;
		}
	}

	private void ReanudeControl()
	{
		m_CameraOrbit.Interact = true;
		m_CameraOrbit.CanRotate = true;
		Pinched = false;
	}

	private void PinchZoom()
	{
		if (Input.touchCount == 2 && Input.GetTouch(0).phase == TouchPhase.Moved && Input.GetTouch(1).phase == TouchPhase.Moved)
		{
			Touch touch = Input.GetTouch(0);
			Touch touch2 = Input.GetTouch(1);
			Vector2 vector = touch.position - touch.deltaPosition;
			Vector2 vector2 = touch2.position - touch2.deltaPosition;
			float magnitude = (vector - vector2).magnitude;
			float magnitude2 = (touch.position - touch2.position).magnitude;
			float num = magnitude - magnitude2;
			m_CameraOrbit.SetStaticZoom(num * (m_PinchZoomSpeed / (float)Math.PI));
			if (CancelRotateOnPinch)
			{
				CancelInvoke("ReanudeControl");
				m_CameraOrbit.Interact = false;
				m_CameraOrbit.CanRotate = false;
				Invoke("ReanudeControl", 0.2f);
				Pinched = true;
			}
		}
	}

	public void OnPointerUp(Touch data)
	{
		if (m_CameraOrbit == null)
		{
			Debug.LogWarning("Please assign a camera orbit target");
		}
		else if (data.fingerId == pointerID)
		{
			direction = Vector2.zero;
			touched = false;
		}
	}
}
