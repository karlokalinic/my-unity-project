using UnityEngine;

public class StormVFXTerrainDemoCamera : MonoBehaviour
{
	public float moveSpeed = 5f;

	public float height = 2f;

	[Space]
	public float acceleration = 10f;

	public float deceleration = 5f;

	private Vector3 velocity;

	private void Start()
	{
	}

	private void Update()
	{
		Vector2 vector = default(Vector2);
		vector.x = Input.GetAxisRaw("Horizontal");
		vector.y = Input.GetAxisRaw("Vertical");
		bool flag = vector != Vector2.zero;
		Vector3 zero = Vector3.zero;
		RaycastHit hitInfo;
		bool flag2 = Physics.Raycast(base.transform.position, Vector3.down, out hitInfo);
		Vector3 zero2 = Vector3.zero;
		if (flag2)
		{
			zero.y = hitInfo.point.y + height - base.transform.position.y;
		}
		if (flag)
		{
			zero += base.transform.right * vector.x;
			zero += base.transform.forward * vector.y;
			zero.Normalize();
			zero *= moveSpeed;
			velocity = Vector3.MoveTowards(velocity, zero, Time.deltaTime * acceleration);
		}
		else
		{
			velocity = Vector3.MoveTowards(velocity, zero, Time.deltaTime * deceleration);
		}
		Vector3 vector2 = velocity * Time.deltaTime;
		base.transform.position += vector2;
	}
}
