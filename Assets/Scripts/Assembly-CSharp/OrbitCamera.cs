using UnityEngine;

public class OrbitCamera : MonoBehaviour
{
	public GameObject target;

	private float speedMod = 0.2f;

	private Vector3 point;

	private void Start()
	{
		point = target.transform.position;
		base.transform.LookAt(point);
	}

	private void Update()
	{
		base.transform.RotateAround(point, new Vector3(0f, 1f, 0f), -10f * Time.deltaTime * speedMod);
	}
}
