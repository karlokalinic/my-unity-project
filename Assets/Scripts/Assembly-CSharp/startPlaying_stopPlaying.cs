using UnityEngine;

public class startPlaying_stopPlaying : MonoBehaviour
{
	public Animator anim;

	public bool armed = true;

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
		if (armed && Input.GetKeyDown(KeyCode.Space))
		{
			StartPlaying();
		}
	}

	private void StartPlaying()
	{
		anim.speed = 1f;
	}

	private void StopPlaying()
	{
		anim.speed = 0f;
	}
}
