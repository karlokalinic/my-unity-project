using UnityEngine;

namespace AC
{
	[ExecuteInEditMode]
	[AddComponentMenu("Adventure Creator/Camera/Align to camera")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_align_to_camera.html")]
	public class AlignToCamera : MonoBehaviour
	{
		public _Camera cameraToAlignTo;

		public bool lockDistance = true;

		public float distanceToCamera;

		public bool lockScale;

		public Vector2 scaleFactor = Vector2.zero;

		public AlignType alignType;

		protected void Awake()
		{
			Align();
		}

		protected void Align()
		{
			if (!cameraToAlignTo)
			{
				return;
			}
			if (alignType == AlignType.YAxisOnly)
			{
				base.transform.rotation = Quaternion.Euler(base.transform.rotation.eulerAngles.x, cameraToAlignTo.transform.rotation.eulerAngles.y, base.transform.rotation.eulerAngles.z);
			}
			else
			{
				base.transform.rotation = cameraToAlignTo.transform.rotation;
			}
			if (lockDistance)
			{
				if (distanceToCamera > 0f)
				{
					Vector3 vector = base.transform.position - cameraToAlignTo.transform.position;
					float magnitude = vector.magnitude;
					if (!Mathf.Approximately(magnitude, distanceToCamera))
					{
						if (magnitude > 0f)
						{
							base.transform.position = cameraToAlignTo.transform.position + vector * distanceToCamera / magnitude;
						}
						else
						{
							base.transform.position = cameraToAlignTo.transform.position + cameraToAlignTo.transform.forward * distanceToCamera;
						}
					}
					if (lockScale)
					{
						CalculateScale();
						if (scaleFactor != Vector2.zero)
						{
							base.transform.localScale = scaleFactor * distanceToCamera;
						}
					}
				}
				else if (distanceToCamera < 0f)
				{
					distanceToCamera = 0f;
				}
				else if (Mathf.Approximately(distanceToCamera, 0f))
				{
					float magnitude2 = (base.transform.position - cameraToAlignTo.transform.position).magnitude;
					if (magnitude2 > 0f)
					{
						distanceToCamera = magnitude2;
					}
				}
			}
			if (!lockScale || cameraToAlignTo == null)
			{
				scaleFactor = Vector2.zero;
			}
		}

		protected void CalculateScale()
		{
			if (scaleFactor == Vector2.zero)
			{
				scaleFactor = new Vector2(base.transform.localScale.x / distanceToCamera, base.transform.localScale.y / distanceToCamera);
			}
		}
	}
}
