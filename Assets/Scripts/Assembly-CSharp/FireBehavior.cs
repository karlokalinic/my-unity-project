using System;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class FireBehavior : LightBehavior
{
	public float fireIntensity = 0.01f;

	public float frequency = 0.01f;

	public bool changeRange;

	public float rangeIntensity = 1f;

	public float rangeFrequency = 0.01f;

	public bool dualMode;

	public float dualFireIntensity = 0.01f;

	public float dualFrequency = 0.01f;

	public bool randomDual;

	public float mode1Time = 1f;

	public float mode2Time = 1f;

	public float chanceOfSwitch = 0.5f;

	public float changeFrequency = 1f;

	public bool windSimulation;

	public float windFrequency = 0.01f;

	public float windStrength = 1.01f;

	public bool moveAround;

	public float moveDistance = 0.01f;

	private float remainder;

	private float randomValue;

	private Vector3 originalPosition;

	private float lightChangeIntensity;

	private bool mode1;

	private float newTimeValue;

	private float changeOn;

	private float goBackOn;

	private void Start()
	{
		originalPosition = base.ThisLight.transform.localPosition;
		mode1 = true;
		newTimeValue = base.TimePassed;
		if (!randomDual)
		{
			changeOn = base.TimePassed + mode1Time;
			goBackOn = changeOn + mode2Time;
		}
		else
		{
			changeOn = base.TimePassed + changeFrequency;
		}
	}

	private void Update()
	{
		clamping();
		base.TimePassed = Time.time;
		if (dualMode)
		{
			ChangeMode();
		}
		if (dualMode)
		{
			if (mode1)
			{
				newTimeValue = base.TimePassed * frequency;
			}
			else
			{
				newTimeValue = base.TimePassed * dualFrequency;
			}
		}
		else
		{
			newTimeValue = base.TimePassed * frequency;
		}
		base.ThisLight.color = base.OriginalColor * UpdateLightSource();
		if (moveAround)
		{
			changePosition();
		}
	}

	private void ChangeMode()
	{
		if (!randomDual)
		{
			if (base.TimePassed >= changeOn)
			{
				mode1 = false;
				if (base.TimePassed >= goBackOn)
				{
					mode1 = true;
					changeOn = base.TimePassed + mode1Time;
					goBackOn = changeOn + mode2Time;
				}
			}
		}
		else
		{
			if (!(base.TimePassed >= changeOn))
			{
				return;
			}
			randomValue = UnityEngine.Random.value;
			if (randomValue <= chanceOfSwitch)
			{
				if (!mode1)
				{
					mode1 = true;
				}
				else
				{
					mode1 = false;
				}
			}
			changeOn = base.TimePassed + changeFrequency;
		}
	}

	private float UpdateLightSource()
	{
		if (dualMode)
		{
			if (mode1)
			{
				calculateChangePattern1(fireIntensity);
			}
			else
			{
				calculateChangePattern1(dualFireIntensity);
			}
		}
		else
		{
			calculateChangePattern1(fireIntensity);
		}
		return base.ChangeValue;
	}

	private void calculateChangePattern1(float intensityValue)
	{
		setIntensity(intensityValue);
		base.ChangeValue = (0f - Mathf.Sin(newTimeValue * 8f * (float)Math.PI - 1.5f)) * lightChangeIntensity + remainder;
	}

	private void setIntensity(float intensityValue)
	{
		if (windSimulation)
		{
			randomValue = UnityEngine.Random.value;
			if (randomValue <= windFrequency)
			{
				lightChangeIntensity = intensityValue * windStrength;
			}
			else
			{
				lightChangeIntensity = intensityValue;
			}
		}
		else
		{
			lightChangeIntensity = intensityValue;
		}
		remainder = 1f - lightChangeIntensity;
	}

	private void changePosition()
	{
		Vector3 localPosition = originalPosition;
		randomValue = UnityEngine.Random.value;
		if (randomValue >= 0.5f)
		{
			localPosition.x -= UnityEngine.Random.Range(0f, moveDistance);
		}
		else
		{
			localPosition.x += UnityEngine.Random.Range(0f, moveDistance);
		}
		randomValue = UnityEngine.Random.value;
		if ((double)randomValue >= 0.5)
		{
			localPosition.z -= UnityEngine.Random.Range(0f, moveDistance);
		}
		else
		{
			localPosition.z += UnityEngine.Random.Range(0f, moveDistance);
		}
		base.ThisLight.transform.localPosition = localPosition;
	}

	private void clamping()
	{
		fireIntensity = Mathf.Clamp(fireIntensity, 0.01f, 0.1f);
		frequency = Mathf.Clamp(frequency, 0.01f, 1f);
		windFrequency = Mathf.Clamp(windFrequency, 0.01f, 0.1f);
		windStrength = Mathf.Clamp(windStrength, 1.01f, 2.3f);
		moveDistance = Mathf.Clamp(moveDistance, 0.01f, 0.25f);
	}
}
