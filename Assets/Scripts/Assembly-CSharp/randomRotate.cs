using UnityEngine;

public class randomRotate : MonoBehaviour
{
	private Quaternion rotTarget;

	public float rotateEverySecond = 1f;

	public void Start()
	{
		randomRot();
		InvokeRepeating("randomRot", 0f, rotateEverySecond);
	}

	public void Update()
	{
		base.transform.rotation = Quaternion.Lerp(base.transform.rotation, rotTarget, Time.time * Time.deltaTime);
	}

	public void randomRot()
	{
		rotTarget = Random.rotation;
	}
}
