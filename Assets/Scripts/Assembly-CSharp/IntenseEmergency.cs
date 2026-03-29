using System;
using UnityEngine;

public class IntenseEmergency : LightBehavior
{
	private void Update()
	{
		base.TimePassed = Time.time;
		base.TimePassed -= Mathf.Floor(base.TimePassed);
		base.ThisLight.color = base.OriginalColor * UpdateLightSource();
	}

	private float UpdateLightSource()
	{
		base.ChangeValue = 0f - Mathf.Sin(base.TimePassed * (float)Math.PI) + 1f;
		return base.ChangeValue;
	}
}
