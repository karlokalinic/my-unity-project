using UnityEngine;

public class HorrorFlicker : LightBehavior
{
	public float minimumValue = 0.6f;

	private bool change;

	private void Start()
	{
		if (minimumValue < 0f)
		{
			minimumValue = 0f;
		}
		else if (minimumValue > 1f)
		{
			minimumValue = 1f;
		}
		change = false;
	}

	private void Update()
	{
		base.TimePassed = Time.time;
		base.TimePassed -= Mathf.Floor(base.TimePassed);
		base.ThisLight.color = base.OriginalColor * UpdateLightSource();
	}

	private float UpdateLightSource()
	{
		if (base.TimePassed <= 0.1f)
		{
			base.ChangeValue = 1f - Random.value * 2f;
			change = true;
		}
		else if (change)
		{
			base.ChangeValue = Random.value;
			if (base.ChangeValue >= 0.5f)
			{
				base.ChangeValue = 1f;
			}
			else
			{
				base.ChangeValue = minimumValue;
			}
			change = false;
		}
		return base.ChangeValue;
	}
}
