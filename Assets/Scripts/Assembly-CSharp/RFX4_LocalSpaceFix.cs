using UnityEngine;

[ExecuteInEditMode]
public class RFX4_LocalSpaceFix : MonoBehaviour
{
	private void Update()
	{
		Matrix4x4 worldToLocalMatrix = base.transform.worldToLocalMatrix;
		ParticleSystemRenderer component = GetComponent<ParticleSystemRenderer>();
		if (component != null)
		{
			if (Application.isPlaying)
			{
				component.material.SetMatrix("_InverseTransformMatrix", worldToLocalMatrix);
			}
			else
			{
				component.sharedMaterial.SetMatrix("_InverseTransformMatrix", worldToLocalMatrix);
			}
		}
	}
}
