using System;
using UnityEngine;

public static class RFX1_ColorHelper
{
	public struct HSBColor
	{
		public float H;

		public float S;

		public float B;

		public float A;

		public HSBColor(float h, float s, float b, float a)
		{
			H = h;
			S = s;
			B = b;
			A = a;
		}
	}

	private const float TOLERANCE = 0.0001f;

	private static string[] colorProperties = new string[10] { "_TintColor", "_Color", "_EmissionColor", "_BorderColor", "_ReflectColor", "_RimColor", "_MainColor", "_CoreColor", "_FresnelColor", "_CutoutColor" };

	public static HSBColor ColorToHSV(Color color)
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
			if (Math.Abs(g - num) < 0.0001f)
			{
				result.H = (b - r) / num3 * 60f + 120f;
			}
			else if (Math.Abs(b - num) < 0.0001f)
			{
				result.H = (r - g) / num3 * 60f + 240f;
			}
			else if (b > g)
			{
				result.H = (g - b) / num3 * 60f + 360f;
			}
			else
			{
				result.H = (g - b) / num3 * 60f;
			}
			if (result.H < 0f)
			{
				result.H += 360f;
			}
		}
		else
		{
			result.H = 0f;
		}
		result.H *= 0.0027777778f;
		result.S = num3 / num * 1f;
		result.B = num;
		return result;
	}

	public static Color HSVToColor(HSBColor hsbColor)
	{
		float value = hsbColor.B;
		float value2 = hsbColor.B;
		float value3 = hsbColor.B;
		if (Math.Abs(hsbColor.S) > 0.0001f)
		{
			float b = hsbColor.B;
			float num = hsbColor.B * hsbColor.S;
			float num2 = hsbColor.B - num;
			float num3 = hsbColor.H * 360f;
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
		return new Color(Mathf.Clamp01(value), Mathf.Clamp01(value2), Mathf.Clamp01(value3), hsbColor.A);
	}

	public static Color ConvertRGBColorByHUE(Color rgbColor, float hue)
	{
		float num = ColorToHSV(rgbColor).B;
		if (num < 0.0001f)
		{
			num = 0.0001f;
		}
		HSBColor hsbColor = ColorToHSV(rgbColor / num);
		hsbColor.H = hue;
		Color result = HSVToColor(hsbColor) * num;
		result.a = rgbColor.a;
		return result;
	}

	public static void ChangeObjectColorByHUE(GameObject go, float hue)
	{
		Renderer[] componentsInChildren = go.GetComponentsInChildren<Renderer>(true);
		Renderer[] array = componentsInChildren;
		foreach (Renderer renderer in array)
		{
			Material material = renderer.material;
			if (material == null)
			{
				continue;
			}
			string[] array2 = colorProperties;
			foreach (string name in array2)
			{
				if (material.HasProperty(name))
				{
					setMatHUEColor(material, name, hue);
				}
			}
		}
		ParticleSystemRenderer[] componentsInChildren2 = go.GetComponentsInChildren<ParticleSystemRenderer>(true);
		ParticleSystemRenderer[] array3 = componentsInChildren2;
		foreach (ParticleSystemRenderer particleSystemRenderer in array3)
		{
			Material trailMaterial = particleSystemRenderer.trailMaterial;
			if (trailMaterial == null)
			{
				continue;
			}
			Material material2 = new Material(trailMaterial);
			material2.name = trailMaterial.name + " (Instance)";
			trailMaterial = (particleSystemRenderer.trailMaterial = material2);
			string[] array4 = colorProperties;
			foreach (string name2 in array4)
			{
				if (trailMaterial.HasProperty(name2))
				{
					setMatHUEColor(trailMaterial, name2, hue);
				}
			}
		}
		SkinnedMeshRenderer[] componentsInChildren3 = go.GetComponentsInChildren<SkinnedMeshRenderer>(true);
		SkinnedMeshRenderer[] array5 = componentsInChildren3;
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in array5)
		{
			Material material4 = skinnedMeshRenderer.material;
			if (material4 == null)
			{
				continue;
			}
			string[] array6 = colorProperties;
			foreach (string name3 in array6)
			{
				if (material4.HasProperty(name3))
				{
					setMatHUEColor(material4, name3, hue);
				}
			}
		}
		Projector[] componentsInChildren4 = go.GetComponentsInChildren<Projector>(true);
		Projector[] array7 = componentsInChildren4;
		foreach (Projector projector in array7)
		{
			if (!projector.material.name.EndsWith("(Instance)"))
			{
				projector.material = new Material(projector.material)
				{
					name = projector.material.name + " (Instance)"
				};
			}
			Material material5 = projector.material;
			if (material5 == null)
			{
				continue;
			}
			string[] array8 = colorProperties;
			foreach (string name4 in array8)
			{
				if (material5.HasProperty(name4))
				{
					projector.material = setMatHUEColor(material5, name4, hue);
				}
			}
		}
		Light[] componentsInChildren5 = go.GetComponentsInChildren<Light>(true);
		Light[] array9 = componentsInChildren5;
		foreach (Light light in array9)
		{
			HSBColor hsbColor = ColorToHSV(light.color);
			hsbColor.H = hue;
			light.color = HSVToColor(hsbColor);
		}
		ParticleSystem[] componentsInChildren6 = go.GetComponentsInChildren<ParticleSystem>(true);
		ParticleSystem[] array10 = componentsInChildren6;
		foreach (ParticleSystem particleSystem in array10)
		{
			ParticleSystem.MainModule main = particleSystem.main;
			HSBColor hsbColor2 = ColorToHSV(particleSystem.main.startColor.color);
			hsbColor2.H = hue;
			main.startColor = HSVToColor(hsbColor2);
			ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particleSystem.colorOverLifetime;
			ParticleSystem.MinMaxGradient color = colorOverLifetime.color;
			Gradient gradient = colorOverLifetime.color.gradient;
			GradientColorKey[] colorKeys = colorOverLifetime.color.gradient.colorKeys;
			float num5 = 0f;
			hsbColor2 = ColorToHSV(colorKeys[0].color);
			num5 = Math.Abs(ColorToHSV(colorKeys[1].color).H - hsbColor2.H);
			hsbColor2.H = hue;
			colorKeys[0].color = HSVToColor(hsbColor2);
			for (int num6 = 1; num6 < colorKeys.Length; num6++)
			{
				hsbColor2 = ColorToHSV(colorKeys[num6].color);
				hsbColor2.H = Mathf.Repeat(hsbColor2.H + num5, 1f);
				colorKeys[num6].color = HSVToColor(hsbColor2);
			}
			gradient.colorKeys = colorKeys;
			color.gradient = gradient;
			colorOverLifetime.color = color;
		}
		RFX1_ShaderColorGradient[] componentsInChildren7 = go.GetComponentsInChildren<RFX1_ShaderColorGradient>(true);
		RFX1_ShaderColorGradient[] array11 = componentsInChildren7;
		foreach (RFX1_ShaderColorGradient rFX1_ShaderColorGradient in array11)
		{
			rFX1_ShaderColorGradient.HUE = hue;
		}
	}

	private static Material setMatHUEColor(Material mat, string name, float hueColor)
	{
		Color color = mat.GetColor(name);
		Color value = ConvertRGBColorByHUE(color, hueColor);
		mat.SetColor(name, value);
		return mat;
	}

	private static Material setMatAlphaColor(Material mat, string name, float alpha)
	{
		Color color = mat.GetColor(name);
		color.a = alpha;
		mat.SetColor(name, color);
		return mat;
	}
}
