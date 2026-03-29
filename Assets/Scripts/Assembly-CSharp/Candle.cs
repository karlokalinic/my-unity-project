using System;
using UnityEngine;

public class Candle : LightBehavior
{
	private void Update()
	{
		base.TimePassed = Time.time;
		base.ThisLight.color = base.OriginalColor * UpdateLightSource();
	}

	private float UpdateLightSource()
	{
		base.ChangeValue = (0f - Mathf.Sin(base.TimePassed * (float)Math.PI)) * 0.04f + 0.96f;
		return base.ChangeValue;
	}
}
