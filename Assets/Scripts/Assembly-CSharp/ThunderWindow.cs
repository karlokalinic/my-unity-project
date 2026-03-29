using System;
using UnityEngine;

public class ThunderWindow : LightBehavior
{
	public float timeBetween = 12f;

	private float nextThunder;

	private float flashTime;

	private float stopThunder;

	private int minFlashes;

	private int maxFlashes;

	private int randomValue;

	private int amplitude;

	private bool makeRandom;

	private void Start()
	{
		if (timeBetween < 0f)
		{
			timeBetween = 0f;
		}
		nextThunder = timeBetween;
		flashTime = 0.4f;
		stopThunder = nextThunder + flashTime;
		minFlashes = 1;
		maxFlashes = 3;
		amplitude = 3;
		makeRandom = true;
		base.ChangeValue = 0f;
	}

	private void Update()
	{
		base.TimePassed = Time.time;
		base.ThisLight.color = base.OriginalColor * UpdateLightSource();
	}

	private float UpdateLightSource()
	{
		if (base.TimePassed >= nextThunder)
		{
			if (makeRandom)
			{
				randomValue = UnityEngine.Random.Range(minFlashes, maxFlashes + 1);
				randomValue *= amplitude;
				makeRandom = false;
			}
			base.ChangeValue = Mathf.Sin(base.TimePassed * (float)randomValue * (float)Math.PI);
			if (base.TimePassed >= stopThunder)
			{
				nextThunder = base.TimePassed + timeBetween;
				stopThunder = nextThunder + flashTime;
				makeRandom = true;
			}
		}
		else
		{
			base.ChangeValue = 0f;
		}
		return base.ChangeValue;
	}
}
