using UnityEngine;

public class ZE_MagicSphereDemoGUI : MonoBehaviour
{
	public struct HSBColor
	{
		public float h;

		public float s;

		public float b;

		public float a;

		public HSBColor(float h, float s, float b, float a)
		{
			this.h = h;
			this.s = s;
			this.b = b;
			this.a = a;
		}
	}

	public GameObject[] Prefabs;

	public Texture HUETexture;

	private int currentNomber;

	private GameObject currentInstance;

	private GUIStyle guiStyleHeader = new GUIStyle();

	private float oldIntensity;

	private Color oldAmbientColor;

	private float dpiScale;

	private bool isDay;

	private float colorHUE;

	private void Start()
	{
		if (Screen.dpi < 1f)
		{
			dpiScale = 1f;
		}
		if (Screen.dpi < 200f)
		{
			dpiScale = 1f;
		}
		else
		{
			dpiScale = Screen.dpi / 200f;
		}
		guiStyleHeader.fontSize = (int)(15f * dpiScale);
		guiStyleHeader.normal.textColor = new Color(0.15f, 0.15f, 0.15f);
		ChangeCurrent(0);
	}

	private void OnGUI()
	{
		if (GUI.Button(new Rect(10f * dpiScale, 15f * dpiScale, 135f * dpiScale, 37f * dpiScale), "PREVIOUS EFFECT"))
		{
			ChangeCurrent(-1);
		}
		if (GUI.Button(new Rect(160f * dpiScale, 15f * dpiScale, 135f * dpiScale, 37f * dpiScale), "NEXT EFFECT"))
		{
			ChangeCurrent(1);
		}
		GUI.DrawTexture(new Rect(12f * dpiScale, 60f * dpiScale, 285f * dpiScale, 15f * dpiScale), HUETexture, ScaleMode.StretchToFill, false, 0f);
		float num = colorHUE;
		colorHUE = GUI.HorizontalSlider(new Rect(12f * dpiScale, 80f * dpiScale, 285f * dpiScale, 15f * dpiScale), colorHUE, 0f, 360f);
		if ((double)Mathf.Abs(num - colorHUE) > 0.001)
		{
			ChangeColor();
		}
	}

	private void ChangeCurrent(int delta)
	{
		currentNomber += delta;
		if (currentNomber > Prefabs.Length - 1)
		{
			currentNomber = 0;
		}
		else if (currentNomber < 0)
		{
			currentNomber = Prefabs.Length - 1;
		}
		if (currentInstance != null)
		{
			Object.Destroy(currentInstance);
		}
		currentInstance = Object.Instantiate(Prefabs[currentNomber], base.transform.position, base.transform.rotation);
		CancelInvoke();
	}

	private void Reactivate()
	{
		currentInstance.SetActive(false);
		currentInstance.SetActive(true);
	}

	private Color Hue(float H)
	{
		Color result = new Color(1f, 0f, 0f);
		if (H >= 0f && H < 1f)
		{
			result = new Color(1f, 0f, H);
		}
		if (H >= 1f && H < 2f)
		{
			result = new Color(2f - H, 0f, 1f);
		}
		if (H >= 2f && H < 3f)
		{
			result = new Color(0f, H - 2f, 1f);
		}
		if (H >= 3f && H < 4f)
		{
			result = new Color(0f, 1f, 4f - H);
		}
		if (H >= 4f && H < 5f)
		{
			result = new Color(H - 4f, 1f, 0f);
		}
		if (H >= 5f && H < 6f)
		{
			result = new Color(1f, 6f - H, 0f);
		}
		return result;
	}

	public HSBColor ColorToHSV(Color color)
	{
		HSBColor result = new HSBColor(0f, 0f, 0f, color.a);
		float r = color.r;
		float g = color.g;
		float b = color.b;
		float num = Mathf.Max(r, Mathf.Max(g, b));
		if (num <= 0f)
		{
			return result;
		}
		float num2 = Mathf.Min(r, Mathf.Min(g, b));
		float num3 = num - num2;
		if (num > num2)
		{
			if (g == num)
			{
				result.h = (b - r) / num3 * 60f + 120f;
			}
			else if (b == num)
			{
				result.h = (r - g) / num3 * 60f + 240f;
			}
			else if (b > g)
			{
				result.h = (g - b) / num3 * 60f + 360f;
			}
			else
			{
				result.h = (g - b) / num3 * 60f;
			}
			if (result.h < 0f)
			{
				result.h += 360f;
			}
		}
		else
		{
			result.h = 0f;
		}
		result.h *= 0.0027777778f;
		result.s = num3 / num * 1f;
		result.b = num;
		return result;
	}

	public Color HSVToColor(HSBColor hsbColor)
	{
		float value = hsbColor.b;
		float value2 = hsbColor.b;
		float value3 = hsbColor.b;
		if (hsbColor.s != 0f)
		{
			float b = hsbColor.b;
			float num = hsbColor.b * hsbColor.s;
			float num2 = hsbColor.b - num;
			float num3 = hsbColor.h * 360f;
			if (num3 < 60f)
			{
				value = b;
				value2 = num3 * num / 60f + num2;
				value3 = num2;
			}
			else if (num3 < 120f)
			{
				value = (0f - (num3 - 120f)) * num / 60f + num2;
				value2 = b;
				value3 = num2;
			}
			else if (num3 < 180f)
			{
				value = num2;
				value2 = b;
				value3 = (num3 - 120f) * num / 60f + num2;
			}
			else if (num3 < 240f)
			{
				value = num2;
				value2 = (0f - (num3 - 240f)) * num / 60f + num2;
				value3 = b;
			}
			else if (num3 < 300f)
			{
				value = (num3 - 240f) * num / 60f + num2;
				value2 = num2;
				value3 = b;
			}
			else if (num3 <= 360f)
			{
				value = b;
				value2 = num2;
				value3 = (0f - (num3 - 360f)) * num / 60f + num2;
			}
			else
			{
				value = 0f;
				value2 = 0f;
				value3 = 0f;
			}
		}
		return new Color(Mathf.Clamp01(value), Mathf.Clamp01(value2), Mathf.Clamp01(value3), hsbColor.a);
	}

	private Material SetMatHUEColor(Material mat, string name, float hueColor)
	{
		Color color = mat.GetColor(name);
		float num = color.maxColorComponent;
		if (num < 0.0001f)
		{
			num = 0.0001f;
		}
		HSBColor hsbColor = ColorToHSV(color / num);
		hsbColor.h = hueColor / 360f;
		Color value = HSVToColor(hsbColor) * num;
		value.a = color.a;
		mat.SetColor(name, value);
		return mat;
	}

	private void ChangeColor()
	{
		Renderer[] componentsInChildren = currentInstance.GetComponentsInChildren<Renderer>();
		Renderer[] array = componentsInChildren;
		foreach (Renderer renderer in array)
		{
			Material material = renderer.material;
			if (!(material == null))
			{
				if (material.HasProperty("_TintColor"))
				{
					SetMatHUEColor(material, "_TintColor", colorHUE);
				}
				if (material.HasProperty("_CoreColor"))
				{
					SetMatHUEColor(material, "_CoreColor", colorHUE);
				}
				if (material.HasProperty("_MainColor"))
				{
					SetMatHUEColor(material, "_MainColor", colorHUE);
				}
				if (material.HasProperty("_RimColor"))
				{
					SetMatHUEColor(material, "_RimColor", colorHUE);
				}
			}
		}
		Projector[] componentsInChildren2 = currentInstance.GetComponentsInChildren<Projector>();
		Projector[] array2 = componentsInChildren2;
		foreach (Projector projector in array2)
		{
			Material material2 = projector.material;
			if (!(material2 == null) && material2.HasProperty("_TintColor"))
			{
				projector.material = SetMatHUEColor(material2, "_TintColor", colorHUE);
			}
		}
		Light componentInChildren = currentInstance.GetComponentInChildren<Light>(true);
		if (componentInChildren != null)
		{
			HSBColor hsbColor = ColorToHSV(componentInChildren.color);
			hsbColor.h = colorHUE / 360f;
			componentInChildren.color = HSVToColor(hsbColor);
		}
	}
}
