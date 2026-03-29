using System;
using UnityEngine;

public class VolatileFire : LightBehavior
{
	public int calmTime = 5;

	public int intenseTime = 3;

	private float changeFireOn;

	private float stopChangeFireOn;

	private void Start()
	{
		if (calmTime < 1)
		{
			calmTime = 1;
		}
		if (intenseTime < 1)
		{
			intenseTime = 1;
		}
		changeFireOn = base.TimePassed + (float)calmTime;
		stopChangeFireOn = changeFireOn + (float)intenseTime;
	}

	private void Update()
	{
		base.TimePassed = Time.time;
		base.ThisLight.color = base.OriginalColor * UpdateLightSource();
	}

	private float UpdateLightSource()
	{
		if (base.TimePassed >= changeFireOn)
		{
			base.ChangeValue = (0f - Mathf.Sin(base.TimePassed * 8f * (float)Math.PI - 1.5f)) * 0.08f + 0.92f;
			if (base.TimePassed >= stopChangeFireOn)
			{
				changeFireOn = base.TimePassed + (float)calmTime;
				stopChangeFireOn = changeFireOn + (float)intenseTime;
			}
		}
		else
		{
			base.ChangeValue = (0f - Mathf.Sin(base.TimePassed * 8f * (float)Math.PI - 1.5f)) * 0.05f + 0.95f;
		}
		return base.ChangeValue;
	}
}
