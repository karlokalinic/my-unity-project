using UnityEngine;

public class MovementCamera : MonoBehaviour
{
	public GameObject target;

	public float speedMove = 1f;

	private Vector3 point;

	private void Start()
	{
		point = target.transform.position;
	}

	private void Update()
	{
		base.transform.RotateAround(point, new Vector3(0f, 1f, 0f), 20f * Time.deltaTime * speedMove);
	}
}
