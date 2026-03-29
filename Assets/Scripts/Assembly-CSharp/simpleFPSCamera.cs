using UnityEngine;

public class simpleFPSCamera : MonoBehaviour
{
	public Vector2 sensitivity = new Vector2(1f, 1f);

	public Vector2 speed = new Vector2(1f, 1f);

	public float minimumY = 60f;

	public float maximumY = 60f;

	private float rotationY;

	private float rotationX;

	private void Update()
	{
		rotationX = base.transform.parent.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivity.x;
		rotationY += Input.GetAxis("Mouse Y") * sensitivity.y;
		rotationY = Mathf.Clamp(rotationY, 0f - minimumY, maximumY);
		base.transform.localEulerAngles = new Vector3(0f - rotationY, 0f, 0f);
		base.transform.parent.localEulerAngles = new Vector3(0f, rotationX, 0f);
		base.transform.parent.position += base.transform.parent.forward * Input.GetAxis("Vertical") * speed.y * 0.1f + base.transform.parent.right * Input.GetAxis("Horizontal") * speed.x * 0.1f;
	}
}
