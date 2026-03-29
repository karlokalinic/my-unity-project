using UnityEngine;

public class Flickering_Light : MonoBehaviour
{
	public Light flickerLight;

	public float flickerSpeed = 60f;

	private float[] smoothing = new float[40];

	private void Start()
	{
		for (int i = 0; i < smoothing.Length; i++)
		{
			smoothing[i] = 0f;
		}
	}

	private void Update()
	{
		float num = 0f;
		for (int i = 1; i < smoothing.Length; i++)
		{
			smoothing[i - 1] = smoothing[i];
			num += smoothing[i - 1];
		}
		smoothing[smoothing.Length - 1] = Random.value;
		num += smoothing[smoothing.Length - 1];
		flickerLight.intensity = 3f * (num / (float)smoothing.Length);
	}
}
