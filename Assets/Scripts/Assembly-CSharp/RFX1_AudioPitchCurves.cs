using UnityEngine;

public class RFX1_AudioPitchCurves : MonoBehaviour
{
	public AnimationCurve AudioCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public float GraphTimeMultiplier = 1f;

	public float GraphPitchMultiplier = 1f;

	public bool IsLoop;

	private bool canUpdate;

	private float startTime;

	private AudioSource audioSource;

	private float startPitch;

	private void Awake()
	{
		audioSource = GetComponent<AudioSource>();
		startPitch = audioSource.pitch;
		audioSource.pitch = AudioCurve.Evaluate(0f) * GraphPitchMultiplier;
	}

	private void OnEnable()
	{
		startTime = Time.time;
		canUpdate = true;
		if (audioSource != null)
		{
			audioSource.pitch = AudioCurve.Evaluate(0f) * GraphPitchMultiplier;
		}
	}

	private void Update()
	{
		float num = Time.time - startTime;
		if (canUpdate)
		{
			float pitch = AudioCurve.Evaluate(num / GraphTimeMultiplier) * startPitch * GraphPitchMultiplier;
			audioSource.pitch = pitch;
		}
		if (num >= GraphTimeMultiplier)
		{
			if (IsLoop)
			{
				startTime = Time.time;
			}
			else
			{
				canUpdate = false;
			}
		}
	}
}
