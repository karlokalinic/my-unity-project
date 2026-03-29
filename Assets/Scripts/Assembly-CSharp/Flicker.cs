using UnityEngine;

public class Flicker : LightBehavior
{
	public float minimumValue = 0.5f;

	public int flickerFrequency = 15;

	private int randomValue;

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
		if (flickerFrequency < 0)
		{
			flickerFrequency = 0;
		}
		else if (flickerFrequency > 100)
		{
			flickerFrequency = 100;
		}
	}

	private void Update()
	{
		base.ThisLight.color = base.OriginalColor * UpdateLightSource();
	}

	private float UpdateLightSource()
	{
		randomValue = Random.Range(0, 100);
		if (randomValue <= flickerFrequency)
		{
			base.ChangeValue = 1f;
		}
		else
		{
			base.ChangeValue = minimumValue;
		}
		return base.ChangeValue;
	}
}
