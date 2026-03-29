using System;
using UnityEngine;

public class Torch : LightBehavior
{
	private void Update()
	{
		base.TimePassed = Time.time;
		base.ThisLight.color = base.OriginalColor * UpdateLightSource();
	}

	private float UpdateLightSource()
	{
		base.ChangeValue = (0f - Mathf.Sin(base.TimePassed * 8f * (float)Math.PI)) * 0.05f + 0.95f;
		return base.ChangeValue;
	}
}
