using System;
using UnityEngine;

public class CalmEmergency : LightBehavior
{
	private void Update()
	{
		base.TimePassed = Time.time;
		base.TimePassed -= Mathf.Floor(base.TimePassed);
		base.ThisLight.color = base.OriginalColor * UpdateLightSource();
	}

	private float UpdateLightSource()
	{
		base.ChangeValue = (0f - Mathf.Sin(base.TimePassed * (float)Math.PI)) * 0.3f + 0.7f;
		return base.ChangeValue;
	}
}
