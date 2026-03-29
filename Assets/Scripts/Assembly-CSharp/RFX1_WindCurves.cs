using UnityEngine;

public class RFX1_WindCurves : MonoBehaviour
{
	public AnimationCurve WindCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public float GraphTimeMultiplier = 1f;

	public float GraphIntensityMultiplier = 1f;

	public bool IsLoop;

	private bool canUpdate;

	private float startTime;

	private WindZone windZone;

	private void Awake()
	{
		windZone = GetComponent<WindZone>();
		windZone.windMain = WindCurve.Evaluate(0f);
	}

	private void OnEnable()
	{
		startTime = Time.time;
		canUpdate = true;
	}

	private void Update()
	{
		float num = Time.time - startTime;
		if (canUpdate)
		{
			float windMain = WindCurve.Evaluate(num / GraphTimeMultiplier) * GraphIntensityMultiplier;
			windZone.windMain = windMain;
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
