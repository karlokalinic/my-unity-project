using UnityEngine;

public class RFX1_EffectSettingColor : MonoBehaviour
{
	public Color Color = Color.red;

	private Color previousColor;

	private void OnEnable()
	{
		UpdateColor();
	}

	private void Update()
	{
		if (previousColor != Color)
		{
			UpdateColor();
		}
	}

	private void UpdateColor()
	{
		float h = RFX1_ColorHelper.ColorToHSV(Color).H;
		RFX1_ColorHelper.ChangeObjectColorByHUE(base.gameObject, h);
		RFX1_TransformMotion componentInChildren = GetComponentInChildren<RFX1_TransformMotion>(true);
		if (componentInChildren != null)
		{
			componentInChildren.HUE = h;
		}
		previousColor = Color;
	}
}
