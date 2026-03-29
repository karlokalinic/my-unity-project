using UnityEngine;

public class effectPlayVolume : MonoBehaviour
{
	public ParticleSystem[] effectsToPlay;

	public bool armed;

	private void OnTriggerEnter(Collider other)
	{
		armed = true;
	}

	private void OnTriggerExit(Collider other)
	{
		armed = false;
	}

	private void Update()
	{
		if (!armed || !Input.GetKeyDown(KeyCode.Space))
		{
			return;
		}
		ParticleSystem[] array = effectsToPlay;
		foreach (ParticleSystem particleSystem in array)
		{
			if (particleSystem.isPlaying)
			{
				particleSystem.Clear();
				particleSystem.Play();
			}
			else
			{
				particleSystem.Play();
			}
		}
	}
}
