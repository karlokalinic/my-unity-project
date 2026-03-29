using UnityEngine;

public class DualColorSimple : Lerping
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
		base.ThisLight.color = Color.Lerp(base.OriginalColor, secondColor, base.Timer);
	}
}
