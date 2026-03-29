using UnityEngine;

public class FlickerBehavior : LightBehavior
{
	public enum stayList
	{
		StayDark = 0,
		StayBright = 1,
		Randomize = 2
	}

	public stayList currentStay;

	public float frequency = 28f;

	public float minimumValue;

	public float chanceToFlicker = 0.1f;

	public float maxFlickerTime = 1f;

	public bool restrict;

	public float restrictValue = 1f;

	private float randomValue;

	private bool flickering;

	private float minFlickerTime = 0.001f;

	private float stopFlickering;

	private float newTimeValue;

	private float stopRestrict;

	private void Start()
	{
		flickering = false;
		stopRestrict = base.TimePassed + restrictValue;
	}

	private void Update()
	{
		base.TimePassed = Time.time;
		if (restrict)
		{
			if (base.TimePassed >= stopRestrict)
			{
				RandomFlickering();
			}
		}
		else
		{
			RandomFlickering();
		}
	}

	private void RandomFlickering()
	{
		if (!flickering)
		{
			randomValue = Random.value;
			if (randomValue <= chanceToFlicker)
			{
				flickering = true;
				randomValue = Random.Range(minFlickerTime, maxFlickerTime);
				stopFlickering = base.TimePassed + randomValue;
			}
		}
		else
		{
			base.ThisLight.color = base.OriginalColor * UpdateLightSource();
		}
	}

	private float UpdateLightSource()
	{
		if (base.TimePassed >= stopFlickering)
		{
			EndFlicker();
		}
		else
		{
			newTimeValue = base.TimePassed * frequency;
			base.ChangeValue = 0f - Mathf.Sin(newTimeValue);
			if (base.ChangeValue <= minimumValue)
			{
				base.ChangeValue = minimumValue;
			}
		}
		return base.ChangeValue;
	}

	private void EndFlicker()
	{
		flickering = false;
		switch (currentStay)
		{
		case stayList.StayDark:
			base.ChangeValue = minimumValue;
			break;
		case stayList.StayBright:
			base.ChangeValue = 1f;
			break;
		case stayList.Randomize:
			randomValue = Random.value;
			if (randomValue <= 0.5f)
			{
				base.ChangeValue = 1f;
			}
			else
			{
				base.ChangeValue = minimumValue;
			}
			break;
		}
		if (restrict)
		{
			stopRestrict = base.TimePassed + restrictValue;
		}
	}
}
