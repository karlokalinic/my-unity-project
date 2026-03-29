using UnityEngine;

public class MoveOneWay : Lerping
{
	public GameObject targetLocation;

	public float secondsBetween = 1f;

	private void Start()
	{
		Initialize();
		if (targetLocation != null)
		{
			base.SecondPosition = targetLocation.transform.position;
		}
		else
		{
			Debug.LogError("ERROR: targetLocation was null, MoveOneWay.cs");
		}
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
		base.ThisLight.transform.position = Vector3.Lerp(base.OriginalPosition, base.SecondPosition, base.Timer);
	}
}
