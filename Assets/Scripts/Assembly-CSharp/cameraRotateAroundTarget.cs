using UnityEngine;

[AddComponentMenu("Camera-Control/Mouse Orbit")]
public class cameraRotateAroundTarget : MonoBehaviour
{
	public Transform target;

	public float distance = 10f;

	public float xSpeed = 250f;

	public float ySpeed = 120f;

	public int yMinLimit = -20;

	public int yMaxLimit = 80;

	public int zoomRate = 25;

	public float yOffset;

	private float x;

	private float y;

	public void Start()
	{
		Transform transform = GameObject.Find("Camera Target").transform;
		if (transform != null)
		{
			target = transform;
		}
		Vector3 eulerAngles = base.transform.eulerAngles;
		x = eulerAngles.y;
		y = eulerAngles.x;
	}

	public void Update()
	{
		if (!Input.GetMouseButton(0) && target != null)
		{
			x += Input.GetAxis("Mouse X") * xSpeed * Time.deltaTime;
			y -= Input.GetAxis("Mouse Y") * ySpeed * Time.deltaTime;
			distance += (0f - Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime) * (float)zoomRate * Mathf.Abs(distance);
			y = ClampAngle(y, yMinLimit, yMaxLimit);
			Quaternion quaternion = Quaternion.Euler(y, x, 0f);
			Vector3 vector = new Vector3(target.position.x, target.position.y + yOffset, target.position.z);
			Vector3 position = quaternion * new Vector3(0f, 0f, 0f - distance) + vector;
			base.transform.rotation = quaternion;
			base.transform.position = position;
		}
	}

	public static float ClampAngle(float angle, float min, float max)
	{
		if (angle < -360f)
		{
			angle += 360f;
		}
		if (angle > 360f)
		{
			angle -= 360f;
		}
		return Mathf.Clamp(angle, min, max);
	}
}
