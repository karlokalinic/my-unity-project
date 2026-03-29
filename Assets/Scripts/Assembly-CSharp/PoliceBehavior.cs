using UnityEngine;

[RequireComponent(typeof(Light))]
public class PoliceBehavior : LightBehavior
{
	public float timeBetween = 0.7f;

	public Color secondColor = Color.blue;

	public float distanceBetween = 1f;

	public bool blink;

	public float blinkFrequency = 45f;

	private float changeOn;

	private Vector3 originalPosition;

	private Vector3 secondPosition;

	private bool isOriginalColor;

	private void Start()
	{
		isOriginalColor = true;
		changeOn = base.TimePassed + timeBetween;
		originalPosition = base.ThisLight.transform.localPosition;
	}

	private void Update()
	{
		base.TimePassed = Time.time;
		if (blink)
		{
			if (isOriginalColor)
			{
				base.ThisLight.color = base.OriginalColor * UpdateLightSourceBlink();
			}
			else
			{
				base.ThisLight.color = secondColor * UpdateLightSourceBlink();
			}
		}
		if (base.TimePassed >= changeOn)
		{
			UpdateLightSourcePattern();
			changeOn = base.TimePassed + timeBetween;
		}
	}

	private float UpdateLightSourceBlink()
	{
		base.ChangeValue = (0f - Mathf.Sin(base.TimePassed * blinkFrequency)) * 0.3f + 0.7f;
		return base.ChangeValue;
	}

	private void UpdateLightSourcePattern()
	{
		if (isOriginalColor)
		{
			base.ThisLight.color = secondColor;
			isOriginalColor = false;
		}
		else
		{
			base.ThisLight.color = base.OriginalColor;
			isOriginalColor = true;
		}
		if (base.ThisLight.transform.localPosition == originalPosition)
		{
			secondPosition = originalPosition;
			secondPosition.x += distanceBetween;
			base.ThisLight.transform.localPosition = secondPosition;
		}
		else
		{
			base.ThisLight.transform.localPosition = originalPosition;
		}
	}
}
