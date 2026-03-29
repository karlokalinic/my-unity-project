using UnityEngine;

public class Thunder : LightBehavior
{
	public float timeBetween = 12f;

	public AudioClip sound;

	private float nextThunder;

	private float flashTime;

	private float stopThunder;

	private AudioSource audioSource;

	private void Start()
	{
		if (timeBetween < 0f)
		{
			timeBetween = 0f;
		}
		nextThunder = timeBetween;
		flashTime = 0.4f;
		stopThunder = nextThunder + flashTime;
		if (sound != null)
		{
			audioSource = base.gameObject.AddComponent<AudioSource>();
		}
		base.ChangeValue = 0f;
	}

	private void Update()
	{
		base.TimePassed = Time.time;
		base.ThisLight.color = base.OriginalColor * UpdateLightSource();
	}

	private float UpdateLightSource()
	{
		if (base.TimePassed >= nextThunder)
		{
			base.ChangeValue = (0f - Mathf.Sin(base.TimePassed * 8f)) * Random.value;
			if (base.TimePassed >= stopThunder)
			{
				if (sound != null)
				{
					audioSource.PlayOneShot(sound);
				}
				nextThunder = base.TimePassed + timeBetween;
				stopThunder = nextThunder + flashTime;
			}
		}
		else
		{
			base.ChangeValue = 0f;
		}
		return base.ChangeValue;
	}
}
