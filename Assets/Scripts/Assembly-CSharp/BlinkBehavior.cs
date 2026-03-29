using UnityEngine;

[RequireComponent(typeof(Light))]
public class BlinkBehavior : LightBehavior
{
	public enum typeList
	{
		OnOff = 0,
		ColorChange = 1
	}

	public typeList currentType;

	public int blinks = 2;

	public bool startBright = true;

	public float frequency = 1f;

	public float minimumValue;

	public Color secondColor = Color.red;

	private float changeOn;

	private void Start()
	{
		changeOn = base.TimePassed + frequency;
		if (startBright)
		{
			base.ThisLight.color = base.OriginalColor;
		}
		else
		{
			base.ThisLight.color = base.OriginalColor * minimumValue;
		}
	}

	private void Update()
	{
		base.TimePassed = Time.time;
		frequency = Mathf.Clamp(frequency, 0f, 100f);
		if (base.TimePassed >= changeOn)
		{
			switch (currentType)
			{
			case typeList.OnOff:
				base.ThisLight.color = base.OriginalColor * UpdateLightSourceOnOff();
				break;
			case typeList.ColorChange:
				UpdateLightSourceColorChange();
				break;
			default:
				Debug.LogError("ERROR: Unrecognized type in currentType.");
				break;
			}
			changeOn = base.TimePassed + frequency;
		}
	}

	private void UpdateLightSourceColorChange()
	{
		if (base.ThisLight.color == base.OriginalColor)
		{
			base.ThisLight.color = secondColor;
		}
		else
		{
			base.ThisLight.color = base.OriginalColor;
		}
	}

	private float UpdateLightSourceOnOff()
	{
		if (base.ThisLight.color == base.OriginalColor)
		{
			base.ChangeValue = minimumValue;
		}
		else
		{
			base.ChangeValue = 1f;
		}
		return base.ChangeValue;
	}
}
