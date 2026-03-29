using UnityEngine;

public class RFX1_RotationFreeze : MonoBehaviour
{
	public bool LockX = true;

	public bool LockY = true;

	public bool LockZ = true;

	private Vector3 startRotation;

	private void Start()
	{
		startRotation = base.transform.localRotation.eulerAngles;
	}

	private void Update()
	{
		float x = ((!LockX) ? base.transform.rotation.eulerAngles.x : startRotation.x);
		float y = ((!LockY) ? base.transform.rotation.eulerAngles.y : startRotation.y);
		float z = ((!LockZ) ? base.transform.rotation.eulerAngles.z : startRotation.z);
		base.transform.rotation = Quaternion.Euler(x, y, z);
	}
}
