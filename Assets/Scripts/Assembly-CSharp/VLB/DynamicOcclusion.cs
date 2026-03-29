using UnityEngine;

namespace VLB
{
	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	[RequireComponent(typeof(VolumetricLightBeam))]
	[HelpURL("http://saladgamer.com/vlb-doc/comp-dynocclusion/")]
	public class DynamicOcclusion : MonoBehaviour
	{
		public enum PlaneAlignment
		{
			Surface = 0,
			Beam = 1
		}

		public LayerMask layerMask = -1;

		public float minOccluderArea;

		public int waitFrameCount = 3;

		public PlaneAlignment planeAlignment;

		public float planeOffset = 0.1f;

		private VolumetricLightBeam m_Master;

		private int m_FrameCountToWait;

		private void OnValidate()
		{
			minOccluderArea = Mathf.Max(minOccluderArea, 0f);
			waitFrameCount = Mathf.Clamp(waitFrameCount, 1, 60);
		}

		private void OnEnable()
		{
			m_Master = GetComponent<VolumetricLightBeam>();
		}

		private void OnDisable()
		{
			SetHitNull();
		}

		private void LateUpdate()
		{
			if (m_FrameCountToWait <= 0)
			{
				ProcessRaycasts();
				m_FrameCountToWait = waitFrameCount;
			}
			m_FrameCountToWait--;
		}

		private Vector3 GetRandomVectorAround(Vector3 direction, float angleDiff)
		{
			float num = angleDiff * 0.5f;
			return Quaternion.Euler(Random.Range(0f - num, num), Random.Range(0f - num, num), Random.Range(0f - num, num)) * direction;
		}

		private RaycastHit GetBestHit()
		{
			Vector3 forward = base.transform.forward;
			RaycastHit[] array = Physics.RaycastAll(base.transform.position, forward, m_Master.fadeEnd, layerMask.value);
			int num = -1;
			float num2 = float.MaxValue;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].collider.bounds.GetMaxArea2D() >= minOccluderArea && array[i].distance < num2)
				{
					num2 = array[i].distance;
					num = i;
				}
			}
			if (num != -1)
			{
				return array[num];
			}
			return default(RaycastHit);
		}

		private void ProcessRaycasts()
		{
			RaycastHit bestHit = GetBestHit();
			if ((bool)bestHit.collider)
			{
				SetHit(bestHit);
			}
			else
			{
				SetHitNull();
			}
		}

		private void SetHit(RaycastHit hit)
		{
			switch (planeAlignment)
			{
			case PlaneAlignment.Beam:
				SetClippingPlane(new Plane(-base.transform.forward, hit.point));
				break;
			default:
				SetClippingPlane(new Plane(hit.normal, hit.point));
				break;
			}
		}

		private void SetHitNull()
		{
			SetClippingPlaneOff();
		}

		private void SetClippingPlane(Plane planeWS)
		{
			planeWS = planeWS.TranslateCustom(planeWS.normal * planeOffset);
			m_Master.SetClippingPlane(planeWS);
		}

		private void SetClippingPlaneOff()
		{
			m_Master.SetClippingPlaneOff();
		}
	}
}
