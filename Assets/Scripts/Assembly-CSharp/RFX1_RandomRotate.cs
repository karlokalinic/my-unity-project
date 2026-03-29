using UnityEngine;

public class RFX1_RandomRotate : MonoBehaviour
{
	public int x = 300;

	public int y = 300;

	public int z = 300;

	private float rangeX;

	private float rangeY;

	private float rangeZ;

	private void Start()
	{
		rangeX = (float)Random.Range(0, 10000) / 100f;
		rangeY = (float)Random.Range(0, 10000) / 100f;
		rangeZ = (float)Random.Range(0, 10000) / 100f;
	}

	private void Update()
	{
		base.transform.Rotate(Time.deltaTime * Mathf.Sin(Time.time + rangeX) * (float)x, Time.deltaTime * Mathf.Sin(Time.time + rangeY) * (float)y, Time.deltaTime * Mathf.Sin(Time.time + rangeZ) * (float)z);
	}
}
