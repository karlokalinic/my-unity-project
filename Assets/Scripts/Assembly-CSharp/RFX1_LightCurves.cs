using UnityEngine;

public class RFX1_LightCurves : MonoBehaviour
{
	public AnimationCurve LightCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public float GraphTimeMultiplier = 1f;

	public float GraphIntensityMultiplier = 1f;

	public bool IsLoop;

	[HideInInspector]
	public bool canUpdate;

	private float startTime;

	private Light lightSource;

	private void Awake()
	{
		lightSource = GetComponent<Light>();
		lightSource.intensity = LightCurve.Evaluate(0f) * GraphIntensityMultiplier;
	}

	private void OnEnable()
	{
		startTime = Time.time;
		canUpdate = true;
		if (lightSource != null)
		{
			lightSource.intensity = LightCurve.Evaluate(0f) * GraphIntensityMultiplier;
		}
	}

	private void Update()
	{
		float num = Time.time - startTime;
		if (canUpdate)
		{
			float intensity = LightCurve.Evaluate(num / GraphTimeMultiplier) * GraphIntensityMultiplier;
			lightSource.intensity = intensity;
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
