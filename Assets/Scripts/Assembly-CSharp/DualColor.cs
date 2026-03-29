using UnityEngine;

public class DualColor : Lerping
{
	public Color secondColor = Color.green;

	public float secondsBetween = 1f;

	private void Start()
	{
		Initialize();
		if (secondsBetween < 0f)
		{
			secondsBetween = 0f;
		}
		Calculate(secondsBetween);
	}

	private void Update()
	{
		ChangeTimer();
		UpdateLightSource();
	}

	private void UpdateLightSource()
	{
		if (base.MovingTowardsSecond)
		{
			base.ThisLight.color = Color.Lerp(base.OriginalColor, secondColor, base.Timer);
		}
		else
		{
			base.ThisLight.color = Color.Lerp(secondColor, base.OriginalColor, base.Timer);
		}
	}
}
