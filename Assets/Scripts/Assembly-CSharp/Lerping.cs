using UnityEngine;

public class Lerping : LightBehavior
{
	private Vector3 originalPosition;

	private Vector3 secondPosition;

	private float changeInTimer;

	private bool movingTowardSecond;

	private float timer;

	public Vector3 OriginalPosition
	{
		get
		{
			return originalPosition;
		}
	}

	public Vector3 SecondPosition
	{
		get
		{
			return secondPosition;
		}
		set
		{
			secondPosition = value;
		}
	}

	public float ChangeInTimer
	{
		get
		{
			return changeInTimer;
		}
	}

	public bool MovingTowardsSecond
	{
		get
		{
			return movingTowardSecond;
		}
		set
		{
			movingTowardSecond = value;
		}
	}

	public float Timer
	{
		get
		{
			return timer;
		}
		set
		{
			timer = value;
		}
	}

	public void Initialize()
	{
		originalPosition = base.ThisLight.transform.position;
		timer = 0f;
		movingTowardSecond = true;
	}

	public void ChangeTimer()
	{
		timer += changeInTimer;
		if (timer >= 1f)
		{
			timer = 0f;
			movingTowardSecond = !movingTowardSecond;
		}
	}

	public void Calculate(float secondsBetween)
	{
		changeInTimer = 1f / (60f * secondsBetween);
	}
}
