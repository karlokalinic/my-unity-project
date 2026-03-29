using UnityEngine;

public class Blink : LightBehavior
{
	public float minimumValue;

	public float timeBetween = 1f;

	public bool startBright = true;

	private float changeLight;

	private void Start()
	{
		if (timeBetween < 0f)
		{
			timeBetween = 0f;
		}
		changeLight = base.TimePassed + timeBetween;
		if (!startBright)
		{
			base.ChangeValue = minimumValue;
		}
	}

	private void Update()
	{
		base.TimePassed = Time.time;
		base.ThisLight.color = base.OriginalColor * UpdateLightSource();
	}

	private float UpdateLightSource()
	{
		if (base.TimePassed >= changeLight)
		{
			if (base.ChangeValue == 1f)
			{
				base.ChangeValue = minimumValue;
			}
			else
			{
				base.ChangeValue = 1f;
			}
			changeLight = base.TimePassed + timeBetween;
		}
		return base.ChangeValue;
	}
}
