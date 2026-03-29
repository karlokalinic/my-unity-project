using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class bl_OrbitButton : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IEventSystemHandler
{
	[Serializable]
	public enum Axys
	{
		Horizontal = 0,
		Vertical = 1
	}

	[SerializeField]
	private bl_CameraOrbit CameraOrbit;

	[SerializeField]
	private Axys m_Axi;

	[SerializeField]
	[Range(-15f, 15f)]
	private float RotationAmount = 5f;

	[SerializeField]
	private bool Maintain;

	private bool isMaitain;

	public void OnPointerDown(PointerEventData eventData)
	{
		if (CameraOrbit == null)
		{
			Debug.LogWarning("Please assign a camera orbit target");
			return;
		}
		if (m_Axi == Axys.Horizontal)
		{
			CameraOrbit.Horizontal = RotationAmount;
		}
		else if (m_Axi == Axys.Vertical)
		{
			CameraOrbit.Vertical = RotationAmount;
		}
		isMaitain = true;
	}

	private void Update()
	{
		if (Maintain && isMaitain)
		{
			if (m_Axi == Axys.Horizontal)
			{
				CameraOrbit.Horizontal = RotationAmount / 5f;
			}
			else if (m_Axi == Axys.Vertical)
			{
				CameraOrbit.Vertical = RotationAmount / 5f;
			}
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (CameraOrbit == null)
		{
			Debug.LogWarning("Please assign a camera orbit target");
		}
		else
		{
			CameraOrbit.Interact = false;
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (CameraOrbit == null)
		{
			Debug.LogWarning("Please assign a camera orbit target");
			return;
		}
		CameraOrbit.Interact = true;
		isMaitain = false;
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (CameraOrbit == null)
		{
			Debug.LogWarning("Please assign a camera orbit target");
			return;
		}
		CameraOrbit.Interact = true;
		isMaitain = false;
	}
}
