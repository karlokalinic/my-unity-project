using UnityEngine;

public class RFX1_SimpleDecal : MonoBehaviour
{
	public float Offset = 0.05f;

	private Transform t;

	private RaycastHit hit;

	private void Awake()
	{
		t = base.transform;
	}

	private void LateUpdate()
	{
		if (Physics.Raycast(t.parent.position + Vector3.up / 2f, Vector3.down, out hit))
		{
			base.transform.position = hit.point + Vector3.up * Offset;
			base.transform.rotation = Quaternion.LookRotation(-hit.normal);
		}
	}
}
