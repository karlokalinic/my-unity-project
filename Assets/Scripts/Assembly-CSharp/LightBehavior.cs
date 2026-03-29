using UnityEngine;

public class LightBehavior : MonoBehaviour
{
	private Light thisLight;

	private Color originalColor;

	private float timePassed;

	private float changeValue;

	public Light ThisLight
	{
		get
		{
			return thisLight;
		}
	}

	public Color OriginalColor
	{
		get
		{
			return originalColor;
		}
	}

	public float TimePassed
	{
		get
		{
			return timePassed;
		}
		set
		{
			timePassed = value;
		}
	}

	public float ChangeValue
	{
		get
		{
			return changeValue;
		}
		set
		{
			changeValue = value;
		}
	}

	private void Awake()
	{
		thisLight = GetComponent<Light>();
		if (thisLight != null)
		{
			originalColor = thisLight.color;
		}
		else
		{
			Debug.LogError("ERROR:");
		}
		timePassed = Time.time;
		changeValue = 1f;
	}
}
